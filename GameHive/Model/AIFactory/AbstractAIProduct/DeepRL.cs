/*************************************************************************************
 * 文 件 名:   DeepRL.cs
 * 描    述:   深度强化学习抽象产品
 *          核心功能：神经网络策略评估 + 价值网络 + MCTS融合
 *          支持模式：1.直接神经网络评估 2.搜索树重用DRL-MCTS
 *          提供方法：1.获取AI下一步（实现抽象方法）
 *                  2.直接神经网络决策
 *                  3.搜索树重用DRL
 *                  4.神经网络推理和策略选择
 *                  5.基于神经网络先验概率的MCTS扩展
 *                  6.神经网络价值评估替代随机Rollout
 *          搜索树重用：1.开启游戏，构建根节点，开启后台搜索线程
 *                  2.后台持续进行神经网络指导的MCTS模拟
 *                  3.获取AI下一步时换根操作，复用搜索结果
 *                  4.支持玩家思考时AI持续计算
 *          模型管理：1.ONNX模型加载和预热
 *                  2.4通道输入转换（AI棋子、玩家棋子、最后落子、当前玩家）
 *                  3.策略概率和价值评估输出
 *                  4.模型生命周期管理和资源释放
 *          抽象方法：
 *                  1.GetAvailablePositions 获取所有可行落子点
 *                  2.CheckGameOverByPiece  判断游戏结束状态
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/4/27 19:58
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class DeepRL : AbstractAIStrategy {
        //DRL参数
        protected InferenceSession? session;
        protected int boardSize;
        protected int PlayedPiecesCnt = 0;
        protected bool isModelLoaded = false;
        protected bool useMonteCarlo = false; //是否使用蒙特卡洛搜索
        protected bool isModelWarmedUp = false; //模型是否已 warm-up
        protected byte[]? modelBytes; //模型

        //DRL-MCTS相关参数
        protected double exploreFactor = 5.0; //DRL-UCB探索常数

        //搜索树重用参数
        private MCTSNode? RootNode;//根节点
        private readonly Mutex mutex = new Mutex();//互斥锁
        private volatile bool end = false;//游戏结束信号
        protected int SearchCount = 4000;//单次游戏搜索轮数
        private int AIMoveSearchCount;//互斥期间搜索次数-用于判断是否达到SearchCount
        private bool PlayerPlaying;//是否玩家正在思考
        private Task? searchTask;//搜索任务
        protected bool SearchTreeReuseEnabled = true;//是否启用搜索树重用
        protected int MinSearchCount = 1000;//最小搜索次数（达到后释放一次锁）

        //获取可行落子点-可以不返回所有空节点，看子类实现策略
        protected abstract List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard);

        /**********算法输入获取方法**********/
        //获取AI的下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //根据是否使用蒙特卡洛搜索决定策略
            Tuple<int, int> move;
            if (useMonteCarlo && SearchTreeReuseEnabled) {
                move = GetMCTSMoveWithSearchTreeReuse(currentBoard, lastX, lastY);
            } else {
                move = GetDirectModelMove(currentBoard, lastX, lastY);
            }
            PlayedPiecesCnt++;
            return move;
        }

        //直接使用DRL模型获取
        protected virtual Tuple<int, int> GetDirectModelMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            var input = ConvertBoardToInput(currentBoard, lastX, lastY);
            var (policy, value) = RunInference(input);
            return SelectBestMove(policy, currentBoard);
        }

        //重用搜索树
        protected virtual Tuple<int, int> GetMCTSMoveWithSearchTreeReuse(List<List<Role>> currentBoard, int lastX, int lastY) {
            //争夺锁，换根
            lock (mutex) {
                PlayerPlaying = false;
                //根据玩家决策换根
                if (lastX != -1 && RootNode != null) {
                    RootNode = RootNode.MoveRoot(lastX, lastY, (node, force) => NodeExpansionWithNNForced(node, currentBoard, force));
                }
                AIMoveSearchCount = 0;

                //释放锁并等待搜索线程通知-收到通知后判断是否达标
                while (AIMoveSearchCount <= SearchCount)
                    Monitor.Wait(mutex);

                //根据AI决策换根
                if (RootNode == null) {
                    throw new InvalidOperationException("根节点为空，无法获取AI决策");
                }

                var bestChild = RootNode.ChildrenMap.Values.OrderByDescending(child => child.VisitedTimes).FirstOrDefault();
                if (bestChild == null) {
                    //如果没有子节点，使用传统方法
                    PlayerPlaying = true;
                    return GetDirectModelMove(currentBoard, lastX, lastY);
                }

                Tuple<int, int> AIDecision = bestChild.PieceSelectedCompareToFather;
                RootNode = RootNode.MoveRoot(AIDecision.Item1, AIDecision.Item2, (node, force) => NodeExpansionWithNNForced(node, GetBoardAfterMove(currentBoard, AIDecision), force));

                //轮到玩家
                PlayerPlaying = true;
                return AIDecision;
            }
        }

        //获取执行移动后的棋盘状态
        private List<List<Role>> GetBoardAfterMove(List<List<Role>> board, Tuple<int, int> move) {
            var newBoard = CopyBoard(board);
            newBoard[move.Item1][move.Item2] = Role.AI;
            return newBoard;
        }

        //执行一次搜索任务
        private void EvalToGo() {
            //一直执行直到结束
            while (!end) {
                lock (mutex) {
                    //禁用搜索树重用标记或玩家正在思考时跳过
                    if (!SearchTreeReuseEnabled && PlayerPlaying) {
                        continue;
                    }

                    //确保根节点存在
                    if (RootNode == null) {
                        continue;
                    }

                    //搜最小单元后释放一次锁
                    for (int i = 0; i < MinSearchCount && RootNode != null; i++) {
                        SimulationOnceWithNN(RootNode, CopyBoard(RootNode.NodeBoard));
                    }
                    AIMoveSearchCount += MinSearchCount;
                    Monitor.Pulse(mutex);
                }
            }
        }

        /**********嵌入DRL的MCTS方法**********/
        //基于DRL的MCTS过程
        private void SimulationOnceWithNN(MCTSNode node, List<List<Role>> board) {
            var path = new List<MCTSNode>();
            var currentNode = node;
            var currentBoard = board;
            //维护当前模拟中的棋子数量
            var currentPieceCount = PlayedPiecesCnt;

            //Selection-选择阶段：从根节点向下选择到叶子节点
            while (!currentNode.IsLeaf) {
                path.Add(currentNode);
                var selectedChild = currentNode.GetGreatestUCB(exploreFactor);

                //确保 selectedChild 不为 null
                if (selectedChild == null) {
                    Console.WriteLine("错误：GetGreatestUCB返回 null！");
                    break;
                }

                var move = selectedChild.PieceSelectedCompareToFather;
                currentNode = selectedChild;
                //在棋盘上执行动作
                currentBoard[move.Item1][move.Item2] = currentNode.LeadToThisStatus;
                currentPieceCount++;
            }

            path.Add(currentNode);

            //评估阶段：检查游戏是否结束
            var currentMove = currentNode.PieceSelectedCompareToFather ?? new Tuple<int, int>(-1, -1);

            //临时保存全局计数，使用模拟中的计数进行游戏结束判断
            var originalPieceCount = PlayedPiecesCnt;
            PlayedPiecesCnt = currentPieceCount;
            var gameResult = CheckGameOverByPiece(currentBoard, currentMove.Item1, currentMove.Item2);
            PlayedPiecesCnt = originalPieceCount; //恢复全局计数

            Role winner;

            if (gameResult != Role.Empty) {
                //游戏已结束-使用真实结果
                winner = gameResult;
            } else {
                //游戏未结束，使用神经网络评估
                var input = ConvertBoardToInput(currentBoard, currentMove.Item1, currentMove.Item2);
                var (policy, networkValue) = RunInference(input);

                //新叶子节点-扩展
                if (currentNode.IsNewLeaf()) {
                    NodeExpansionWithNN(currentNode, currentBoard, policy, currentPieceCount);
                }

                //使用神经网络的价值评估作为 leaf_value
                winner = (networkValue > 0) ? Role.AI : Role.Player;
            }
            //反向传播
            currentNode.BackPropagation(winner);
        }

        //DRL拓展 - 传入策略概率避免重复推理
        private void NodeExpansionWithNN(MCTSNode node, List<List<Role>> board, float[] policy, int currentPieceCount) {
            var availableMoves = GetAvailablePositions(board);
            var nextPlayer = (node.LeadToThisStatus == Role.AI) ? Role.Player : Role.AI;

            foreach (var move in availableMoves) {
                var newBoard = CopyBoard(board);
                newBoard[move.Item1][move.Item2] = nextPlayer;

                //获取该位置的先验概率
                int policyIndex = move.Item1 * boardSize + move.Item2;
                double priorProbability = (policyIndex < policy.Length) ? policy[policyIndex] : 0.0;

                //使用带先验概率的构造函数创建子节点
                var childNode = new MCTSNode(newBoard, node, move.Item1, move.Item2, nextPlayer, Role.Empty,
                    GetAvailablePositions(newBoard), priorProbability);

                node.AddSon(childNode, move.Item1, move.Item2);
            }

            node.IsLeaf = false;
        }

        //强制拓展节点 - 用于换根时确保节点被正确扩展
        private void NodeExpansionWithNNForced(MCTSNode node, List<List<Role>> board, bool force = false) {
            if (!node.IsLeaf && !force)
                return; // 已经扩展过了，除非强制扩展

            var availableMoves = GetAvailablePositions(board);
            if (availableMoves.Count == 0) {
                // 如果强制扩展但没有可用移动，获取所有空位置
                if (force) {
                    availableMoves = GetAllEmptyPositions(board);
                }
                if (availableMoves.Count == 0)
                    return; // 仍然没有可用移动
            }

            //获取策略概率
            var input = ConvertBoardToInput(board, -1, -1);
            var (policy, _) = RunInference(input);

            NodeExpansionWithNN(node, board, policy, PlayedPiecesCnt);
        }

        //获取所有空位置 - 防止换根失败
        private List<Tuple<int, int>> GetAllEmptyPositions(List<List<Role>> board) {
            var positions = new List<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] == Role.Empty) {
                        positions.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return positions;
        }

        //复制棋盘
        private List<List<Role>> CopyBoard(List<List<Role>> board) {
            return board.Select(row => new List<Role>(row)).ToList();
        }

        /**********模型调用工具函数**********/
        //将棋盘状态转换为模型输入格式
        protected virtual float[,,,] ConvertBoardToInput(List<List<Role>> currentBoard, int lastX = -1, int lastY = -1) {
            //创建4通道输入: [batch_size=1, channels=4, height=8, width=8]
            var input = new float[1, 4, boardSize, boardSize];

            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    //通道0: AI的棋子
                    if (currentBoard[i][j] == Role.AI) {
                        input[0, 0, i, j] = 1.0f;
                    }
                    //通道1: 玩家的棋子
                    else if (currentBoard[i][j] == Role.Player) {
                        input[0, 1, i, j] = 1.0f;
                    }
                    //通道2: 最后一步落子位置
                    if (lastX == i && lastY == j) {
                        input[0, 2, i, j] = 1.0f;
                    }
                }
            }

            //通道3: 当前玩家标识 (AI回合为1)
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    input[0, 3, i, j] = 1.0f;
                }
            }

            return input;
        }

        //使用模型进行推理
        protected virtual (float[] policy, float value) RunInference(float[,,,] input) {
            if (session == null) {
                throw new InvalidOperationException("模型未正确加载");
            }
            //将4维数组转换为1维数组
            var flatInput = new float[1 * 4 * boardSize * boardSize];
            int index = 0;
            for (int b = 0; b < 1; b++) {
                for (int c = 0; c < 4; c++) {
                    for (int h = 0; h < boardSize; h++) {
                        for (int w = 0; w < boardSize; w++) {
                            flatInput[index++] = input[b, c, h, w];
                        }
                    }
                }
            }
            //创建输入张量
            var inputTensor = new DenseTensor<float>(flatInput, new int[] { 1, 4, boardSize, boardSize });
            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };
            //运行推理
            using var results = session.Run(inputs);
            //获取输出
            var policyOutput = results.First(r => r.Name == "policy").AsTensor<float>();
            var valueOutput = results.First(r => r.Name == "value").AsTensor<float>();
            //转换为数组
            var policy = new float[boardSize * boardSize];
            for (int i = 0; i < policy.Length; i++) {
                policy[i] = policyOutput[0, i];
            }
            float value = valueOutput[0, 0];
            return (policy, value);
        }

        //根据策略概率选择最佳移动
        private Tuple<int, int> SelectBestMove(float[] policy, List<List<Role>> currentBoard) {
            //获取可行落子点
            var availablePositions = GetAvailablePositions(currentBoard);

            //在可行点中找到概率最高的位置
            var bestMove = availablePositions[0];
            float bestProb = policy[bestMove.Item1 * boardSize + bestMove.Item2];

            foreach (var pos in availablePositions) {
                int index = pos.Item1 * boardSize + pos.Item2;
                if (policy[index] > bestProb) {
                    bestProb = policy[index];
                    bestMove = pos;
                }
            }

            return bestMove;
        }

        //加载ONNX模型
        protected void LoadModel(byte[] modelBytes) {
            if (modelBytes == null || modelBytes.Length == 0) {
                throw new InvalidOperationException("模型资源不存在");
            }
            session = new InferenceSession(modelBytes);
            isModelLoaded = true;
            this.modelBytes = modelBytes;
        }

        //预热模型
        private void WarmUpModel() {
            if (session == null) {
                throw new InvalidOperationException("模型未加载，无法预热");
            }
            if (isModelWarmedUp) {
                return;
            }
            //创建虚拟输入进行预热
            var dummyInput = new float[1 * 4 * boardSize * boardSize];
            var inputTensor = new DenseTensor<float>(dummyInput, new int[] { 1, 4, boardSize, boardSize });
            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };
            //运行一次推理进行预热
            using var results = session.Run(inputs);
            isModelWarmedUp = true;
            Console.WriteLine("模型预热完成");
        }


        /**********模型生命周期管理**********/
        //游戏开始时的初始化 - 参考MCTS.cs的GameStart
        public override void GameStart(bool IsAIFirst) {
            Console.WriteLine($"GameStart: session={session != null}, isModelLoaded={isModelLoaded}, isModelWarmedUp={isModelWarmedUp}");

            //停止之前的搜索
            end = true;
            if (searchTask != null && !searchTask.IsCompleted) {
                try {
                    searchTask.Wait(1000);
                } catch {
                    // 忽略等待异常
                }
            }

            //重置状态
            end = false;
            PlayedPiecesCnt = 0;

            //如果 session 已被释放或模型未加载，重新加载模型
            if (session == null || !isModelLoaded) {
                Console.WriteLine("重新加载模型...");
                if (modelBytes == null) {
                    throw new InvalidOperationException("模型字节数组为空，无法重新加载模型");
                }
                LoadModel(modelBytes);
            }
            //在游戏开始时预热模型
            WarmUpModel();

            //创建根节点 - 参考MCTS.cs
            if (useMonteCarlo && SearchTreeReuseEnabled) {
                List<List<Role>> board = new List<List<Role>>(boardSize);
                for (int i = 0; i < boardSize; i++) {
                    List<Role> row = new List<Role>(boardSize);
                    for (int j = 0; j < boardSize; j++) {
                        row.Add(Role.Empty);
                    }
                    board.Add(row);
                }

                //非先手者作为虚拟节点（根节点的父节点）所属人
                Role WhoLeadTo;
                if (IsAIFirst) {
                    WhoLeadTo = Role.Player;
                    PlayerPlaying = false;
                } else {
                    WhoLeadTo = Role.AI;
                    PlayerPlaying = true;
                }

                RootNode = new MCTSNode(board, null, -1, -1, WhoLeadTo, Role.Empty, GetAvailablePositions(board));

                //启动搜索任务
                searchTask = Task.Run(() => EvalToGo());
            }
        }

        //释放资源 - 参考MCTS.cs的GameForcedEnd
        public override void GameForcedEnd() {
            Console.WriteLine("GameForcedEnd: 释放模型资源");
            end = true;

            // 通知等待的线程
            lock (mutex) {
                Monitor.PulseAll(mutex);
            }

            // 等待搜索任务结束
            if (searchTask != null && !searchTask.IsCompleted) {
                try {
                    searchTask.Wait(2000); // 等待最多2秒
                } catch {
                    // 忽略等待异常
                }
            }

            searchTask = null;
            RootNode = null;

            session?.Dispose();
            session = null;
            isModelLoaded = false;
            isModelWarmedUp = false;
        }

        public override void UserPlayPiece(int lastX, int lastY) {
            PlayedPiecesCnt++; //用户下棋后增加计数
        }
    }
}
