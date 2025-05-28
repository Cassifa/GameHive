/*************************************************************************************
 * 文 件 名:   DeepRL.cs
 * 描    述:   深度强化学习抽象产品
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
        protected InferenceSession? session;
        protected int boardSize;
        protected int PlayedPiecesCnt = 0;
        protected bool isModelLoaded = false;
        protected bool useMonteCarlo = false; //是否使用蒙特卡洛搜索
        protected bool isModelWarmedUp = false; //模型是否已 warm-up
        protected byte[]? modelBytes; //模型

        //MCTS相关参数
        protected int MCTSimulations = 400; //MCTS模拟次数
        protected double exploreFactor = 5.0; //DRL-UCB探索常数

        //获取可行落子点
        protected abstract List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard);

        /**********算法输入获取方法**********/
        //获取AI的下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //根据是否使用蒙特卡洛搜索决定策略
            Tuple<int, int> move;
            if (useMonteCarlo) {
                move = GetMCTSMoveWithNN(currentBoard, lastX, lastY);
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


        //使用DRL-MCTS获取
        protected virtual Tuple<int, int> GetMCTSMoveWithNN(List<List<Role>> currentBoard, int lastX, int lastY) {
            //创建MCTS根 - 当前轮到AI下棋
            var rootNode = new MCTSNode(currentBoard, null, -1, -1, Role.AI, Role.Empty, GetAvailablePositions(currentBoard));
            //执行MCTS模拟
            for (int i = 0; i < MCTSimulations; i++) {
                SimulationOnceWithNN(rootNode, CopyBoard(currentBoard));
            }
            //根据访问次数选择最佳移动
            var bestMove = SelectMoveFromMCTSWithNN(rootNode);
            return bestMove;
        }


        /**********嵌入DRL的MCTS方法**********/
        //DRL-MCTS改进：
        //1.用神经网络价值评估替代随机 rollout - 提供更准确的叶子节点估值
        //2.用神经网络策略概率作为UCB公式的先验概率 - 指导更智能的探索
        //3.大幅减少模拟次数的同时获得更好效果

        //从MCTS结果中选择移动
        private Tuple<int, int> SelectMoveFromMCTSWithNN(MCTSNode rootNode) {
            //选择访问次数最多的移动
            var bestChild = rootNode.ChildrenMap.Values.OrderByDescending(child => child.VisitedTimes).First();
            return bestChild.PieceSelectedCompareToFather;
        }

        //基于DRL的MCTS过程
        //关键创新：用神经网络估值替代传统的随机模拟到底
        private void SimulationOnceWithNN(MCTSNode node, List<List<Role>> board) {
            var path = new List<MCTSNode>();
            var currentNode = node;
            var currentBoard = board;
            //维护当前模拟中的棋子数量
            var currentPieceCount = PlayedPiecesCnt;

            //Selection-选择阶段：从根节点向下选择到叶子节点
            //使用ADRL-先验概率UCB选择
            while (!currentNode.IsLeaf) {
                path.Add(currentNode);
                var selectedChild = currentNode.GetGreatestUCB(exploreFactor);

                //确保 selectedChild 不为 null - 理论上不会发生，因为已经确保根节点被扩展
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
            //获取当前节点的移动位置，如果是根节点则使用(-1, -1)
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
                    //使用神经网络的策略概率指导扩展 - 这为UCB提供了智能的先验概率
                    NodeExpansionWithNN(currentNode, currentBoard, policy, currentPieceCount);
                }

                //使用神经网络的价值评估作为 leaf_value (Evaluation)
                //networkValue > 0 表示对AI有利，< 0 表示对Player有利
                winner = (networkValue > 0) ? Role.AI : Role.Player;
            }
            //反向传播-真实游戏结果还是神经网络估值，都通过相同的方式传播
            currentNode.BackPropagation(winner);
        }

        //DRL拓展 - 传入策略概率避免重复推理
        private void NodeExpansionWithNN(MCTSNode node, List<List<Role>> board, float[] policy, int currentPieceCount) {
            var availableMoves = GetAvailablePositions(board);
            var nextPlayer = (node.LeadToThisStatus == Role.AI) ? Role.Player : Role.AI;

            foreach (var move in availableMoves) {
                var newBoard = CopyBoard(board);
                newBoard[move.Item1][move.Item2] = nextPlayer;

                //获取该位置的先验概率 - 这是AlphaGo-Zero的核心创新
                int policyIndex = move.Item1 * boardSize + move.Item2;
                double priorProbability = (policyIndex < policy.Length) ? policy[policyIndex] : 0.0;

                //使用带先验概率的构造函数创建子节点 - 完全模仿AlphaGo-Zero
                var childNode = new MCTSNode(newBoard, node, move.Item1, move.Item2, nextPlayer, Role.Empty,
                    GetAvailablePositions(newBoard), priorProbability);

                node.AddSon(childNode, move.Item1, move.Item2);
            }

            node.IsLeaf = false;
        }

        //复制棋盘
        private List<List<Role>> CopyBoard(List<List<Role>> board) {
            return board.Select(row => new List<Role>(row)).ToList();
        }


        /**********模型调用工具函数**********/
        //将棋盘状态转换为模型输入格式
        //currentBoard:当前棋盘状态
        //lastX:上一步X坐标
        //lastY:上一步Y坐标
        //返回:4通道的输入张量
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
        //input:输入张量
        //返回:策略概率和价值评估
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


        /**********模型生命周期管理**********/
        //游戏开始时的初始化
        public override void GameStart(bool IsAIFirst) {
            Console.WriteLine($"GameStart: session={session != null}, isModelLoaded={isModelLoaded}, isModelWarmedUp={isModelWarmedUp}");

            //重置棋子计数
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

        //释放资源
        public override void GameForcedEnd() {
            Console.WriteLine("GameForcedEnd: 释放模型资源");
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
