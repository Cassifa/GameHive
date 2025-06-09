/*************************************************************************************
 * 文 件 名:   DeepRL.cs
 * 描    述:   深度强化学习抽象产品
 *          核心功能：神经网络策略评估 + 价值网络 + MCTS融合
 *          支持模式：1.直接神经网络评估 2.搜索树重用DRL-MCTS 3.多线程DRL-MCTS
 *          提供方法：1.获取AI下一步（实现抽象方法）
 *                  2.直接神经网络决策
 *                  3.搜索树重用DRL-MCTS决策（持久搜索树+换根）
 *                  4.多线程DRL-MCTS决策（虚拟损失+搜索树重用）
 *                  5.神经网络推理和策略选择
 *                  6.基于神经网络先验概率的MCTS扩展
 *                  7.神经网络价值评估替代随机Rollout
 *          搜索树重用：1.开启游戏，构建根节点，开启后台搜索线程
 *                  2.后台持续进行神经网络指导的MCTS模拟
 *                  3.获取AI下一步时换根操作，复用搜索结果
 *                  4.支持玩家思考时AI持续计算
 *          多线程搜索：1.基于AlphaGo-Zero思路，使用虚拟损失避免频繁加锁
 *                  2.多个工作线程并行执行MCTS模拟
 *                  3.支持搜索树重用和换根操作
 *                  4.原子操作保证线程安全性
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

        /**********类变量定义区**********/

        //DRL基础参数
        protected InferenceSession? session;
        protected int boardSize;
        protected int PlayedPiecesCnt = 0;//反映了当前玩家可见棋盘的落子数量
        protected bool isModelLoaded = false;
        protected bool useMonteCarlo = false; //是否使用蒙特卡洛搜索
        protected bool isModelWarmedUp = false; //模型是否已 warm-up
        protected byte[]? modelBytes; //模型
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

        //多线程搜索参数
        private MultiThreadMCTSNode? MultiThreadRootNode;//多线程根节点
        private volatile bool multiThreadEnd = false;//多线程游戏结束信号
        protected int ThreadCount = 4;//搜索线程数量
        protected int MultiThreadSearchCount = 8000;//多线程搜索轮数
        protected int MultiThreadMinSearchCount = 1000;//多线程最小搜索次数（批量大小）
        private volatile int completedSearches = 0;//已完成的搜索次数
        private List<Task>? multiThreadSearchTasks;//多线程搜索任务列表
        protected bool MultiThreadSearchEnabled = false;//是否启用多线程搜索
        private readonly object multiThreadLock = new object();//多线程同步锁
        private readonly object inferenceLock = new object();//模型推理锁，保护Session线程安全

        //线程本地缓存，减少内存分配
        private static readonly ThreadLocal<List<MultiThreadMCTSNode>> pathCache =
            new ThreadLocal<List<MultiThreadMCTSNode>>(() => new List<MultiThreadMCTSNode>(32));
        private static readonly ThreadLocal<List<List<Role>>> boardCache =
            new ThreadLocal<List<List<Role>>>(() => new List<List<Role>>());
        private static readonly ThreadLocal<List<Tuple<int, int>>> moveCache =
            new ThreadLocal<List<Tuple<int, int>>>(() => new List<Tuple<int, int>>(32));

        //AlphaGo Zero风-批量推理优化
        private readonly Queue<InferenceRequest> inferenceQueue = new Queue<InferenceRequest>();
        private readonly object queueLock = new object();
        private readonly AutoResetEvent inferenceReady = new AutoResetEvent(false);
        private Task? batchInferenceTask;
        private volatile bool stopBatchInference = false;
        private const int BATCH_SIZE = 8; // 批处理大小
        private const int BATCH_TIMEOUT_MS = 5; // 批处理超时(毫秒)

        //推理请求类
        private class InferenceRequest {
            public float[,,,] Input { get; set; }
            public TaskCompletionSource<(float[] policy, float value)> CompletionSource { get; set; }

            public InferenceRequest(float[,,,] input) {
                Input = input;
                CompletionSource = new TaskCompletionSource<(float[] policy, float value)>();
            }
        }

        //获取可行落子点-可以不返回所有空节点，看子类实现策略
        protected abstract List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard);


        /**********算法输出获取方法**********/
        //获取AI的下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //根据搜索模式决定策略
            Tuple<int, int> move;
            if (useMonteCarlo && MultiThreadSearchEnabled) {
                move = GetMultiThreadMCTSMove(currentBoard, lastX, lastY);
            } else if (useMonteCarlo && SearchTreeReuseEnabled) {
                move = GetMCTSMoveWithSearchTreeReuse(currentBoard, lastX, lastY);
            } else {
                move = GetDirectModelMove(currentBoard, lastX, lastY);
            }
            PlayedPiecesCnt++;
            return move;
        }

        //模式1：直接使用DRL模型获取
        protected virtual Tuple<int, int> GetDirectModelMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            var input = ConvertBoardToInput(currentBoard, lastX, lastY);
            var (policy, value) = RunInference(input);
            return SelectBestMove(policy, currentBoard);
        }

        //模式2：重用搜索树
        protected virtual Tuple<int, int> GetMCTSMoveWithSearchTreeReuse(List<List<Role>> currentBoard, int lastX, int lastY) {
            System.Diagnostics.Debug.WriteLine($"开始搜索树重用模式，目标次数: {SearchCount}");
            
            //争夺锁，换根
            lock (mutex) {
                PlayerPlaying = false;
                //根据玩家决策换根
                if (lastX != -1 && RootNode != null) {
                    System.Diagnostics.Debug.WriteLine($"玩家落子: ({lastX}, {lastY})，执行换根");
                    RootNode = RootNode.MoveRoot(lastX, lastY, (node, force) => NodeExpansionWithNNForced(node, currentBoard, force));
                }
                AIMoveSearchCount = 0;
                System.Diagnostics.Debug.WriteLine("重置搜索计数，开始等待搜索完成");

                //释放锁并等待搜索线程通知-收到通知后判断是否达标
                int waitCount = 0;
                while (AIMoveSearchCount < SearchCount) { // 修复：应该是 < 而不是 <=
                    Monitor.Wait(mutex, 1000);
                    waitCount++;
                    System.Diagnostics.Debug.WriteLine($"等待第{waitCount}次，当前搜索进度: {AIMoveSearchCount}/{SearchCount}");
                    
                    // 防止无限等待
                    if (waitCount > 60) {
                        System.Diagnostics.Debug.WriteLine("搜索超时，使用当前结果");
                        break;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"搜索完成，实际完成次数: {AIMoveSearchCount}");

                //根据AI决策换根
                if (RootNode == null) {
                    throw new InvalidOperationException("根节点为空，无法获取AI决策");
                }

                var bestChild = RootNode.ChildrenMap.Values.OrderByDescending(child => child.VisitedTimes).FirstOrDefault();
                if (bestChild == null) {
                    //如果没有子节点，使用传统方法
                    System.Diagnostics.Debug.WriteLine("没有找到最佳子节点，使用直接模型决策");
                    PlayerPlaying = true;
                    return GetDirectModelMove(currentBoard, lastX, lastY);
                }

                Tuple<int, int> AIDecision = bestChild.PieceSelectedCompareToFather;
                System.Diagnostics.Debug.WriteLine($"AI决策: ({AIDecision.Item1}, {AIDecision.Item2})，访问次数: {bestChild.VisitedTimes}");
                
                RootNode = RootNode.MoveRoot(AIDecision.Item1, AIDecision.Item2, (node, force) => NodeExpansionWithNNForced(node, GetBoardAfterMove(currentBoard, AIDecision), force));

                //轮到玩家
                PlayerPlaying = true;
                return AIDecision;
            }
        }

        //模式3：多线程搜索
        protected virtual Tuple<int, int> GetMultiThreadMCTSMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            System.Diagnostics.Debug.WriteLine($"开始多线程搜索，目标次数: {MultiThreadSearchCount}");
            
            lock (multiThreadLock) {
                //根据玩家决策换根
                if (lastX != -1 && MultiThreadRootNode != null) {
                    System.Diagnostics.Debug.WriteLine($"玩家落子: ({lastX}, {lastY})，执行换根");
                    MultiThreadRootNode = MultiThreadRootNode.MoveRoot(lastX, lastY, (node, force) => MultiThreadNodeExpansionWithNNForced(node, currentBoard, force));
                }

                //重置搜索计数
                Interlocked.Exchange(ref completedSearches, 0);
                System.Diagnostics.Debug.WriteLine("重置搜索计数，开始等待搜索完成");

                //等待搜索完成
                int waitCount = 0;
                while (Interlocked.CompareExchange(ref completedSearches, 0, 0) < MultiThreadSearchCount) {
                    Monitor.Wait(multiThreadLock, 1000); // 增加超时时间
                    waitCount++;
                    int currentSearches = Interlocked.CompareExchange(ref completedSearches, 0, 0);
                    System.Diagnostics.Debug.WriteLine($"等待第{waitCount}次，当前搜索进度: {currentSearches}/{MultiThreadSearchCount}");
                    
                    // 防止无限等待
                    if (waitCount > 60) { // 最多等待60秒
                        System.Diagnostics.Debug.WriteLine("搜索超时，使用当前结果");
                        break;
                    }
                }

                int finalSearches = Interlocked.CompareExchange(ref completedSearches, 0, 0);
                System.Diagnostics.Debug.WriteLine($"搜索完成，实际完成次数: {finalSearches}");

                //选择最佳移动
                if (MultiThreadRootNode == null) {
                    throw new InvalidOperationException("多线程根节点为空，无法获取AI决策");
                }

                var bestChild = MultiThreadRootNode.ChildrenMap.Values.OrderByDescending(child => child.GetVisitedTimes()).FirstOrDefault();
                if (bestChild == null) {
                    System.Diagnostics.Debug.WriteLine("没有找到最佳子节点，使用直接模型决策");
                    return GetDirectModelMove(currentBoard, lastX, lastY);
                }

                Tuple<int, int> aiDecision = bestChild.PieceSelectedCompareToFather;
                System.Diagnostics.Debug.WriteLine($"AI决策: ({aiDecision.Item1}, {aiDecision.Item2})，访问次数: {bestChild.GetVisitedTimes()}");
                
                MultiThreadRootNode = MultiThreadRootNode.MoveRoot(aiDecision.Item1, aiDecision.Item2,
                    (node, force) => MultiThreadNodeExpansionWithNNForced(node, GetBoardAfterMove(currentBoard, aiDecision), force));

                return aiDecision;
            }
        }


        /**********搜索执行层**********/
        //执行一次搜索任务 - 支持玩家回合持续思考
        private void EvalToGo() {
            //一直执行直到结束
            while (!end) {
                lock (mutex) {
                    //检查搜索树重用是否启用和根节点存在
                    if (!SearchTreeReuseEnabled || RootNode == null) {
                        // 如果搜索树重用被禁用或根节点不存在，短暂等待后重试
                        Monitor.Wait(mutex, 10);
                        continue;
                    }

                    //搜最小单元后释放一次锁
                    for (int i = 0; i < MinSearchCount && RootNode != null && !end; i++) {
                        SimulationOnceWithNN(RootNode, CopyBoard(RootNode.NodeBoard));
                    }
                    AIMoveSearchCount += MinSearchCount;
                    Monitor.Pulse(mutex);
                }

                //短暂休眠，避免过度占用CPU（特别是玩家思考时）
                if (PlayerPlaying) {
                    Thread.Sleep(1); // 玩家思考时降低搜索强度
                }
            }
        }

        //多线程搜索任务执行器
        private void MultiThreadSearchWorker() {
            while (!multiThreadEnd) {
                // 等待搜索任务或检测到游戏结束
                if (MultiThreadRootNode == null) {
                    Thread.Sleep(10);
                    continue;
                }

                // 检查当前是否有搜索需求
                int currentTotal = Interlocked.CompareExchange(ref completedSearches, 0, 0);
                if (currentTotal >= MultiThreadSearchCount) {
                    // 当前轮次搜索已完成，等待下一轮
                    Thread.Sleep(1);
                    continue;
                }

                // 执行批量搜索
                var rootBoard = MultiThreadRootNode.NodeBoard;
                int batchActualCount = 0;
                
                for (int i = 0; i < MultiThreadMinSearchCount && !multiThreadEnd && MultiThreadRootNode != null; i++) {
                    // 再次检查是否已达标，避免超额搜索
                    int checkTotal = Interlocked.CompareExchange(ref completedSearches, 0, 0);
                    if (checkTotal >= MultiThreadSearchCount) {
                        break;
                    }

                    MultiThreadSimulationOnceWithNN(MultiThreadRootNode, CopyBoard(rootBoard));
                    batchActualCount++;
                }

                // 更新搜索计数
                if (batchActualCount > 0) {
                    int totalCompleted = Interlocked.Add(ref completedSearches, batchActualCount);

                    // 检查是否完成本轮搜索并通知主线程
                    if (totalCompleted >= MultiThreadSearchCount) {
                        lock (multiThreadLock) {
                            Monitor.PulseAll(multiThreadLock);
                        }
                    }
                }
            }
        }

        //多线程版本的MCTS模拟
        private void MultiThreadSimulationOnceWithNN(MultiThreadMCTSNode node, List<List<Role>> rootBoard) {
            var path = pathCache.Value;
            var moves = moveCache.Value;
            path.Clear();
            moves.Clear();

            var currentNode = node;
            var simulationPieceCount = PlayedPiecesCnt; // 从真实游戏状态开始计数

            //Selection阶段：记录路径但不修改棋盘，减少拷贝开销
            while (!currentNode.IsLeaf && !currentNode.IsCurrentlyExpanding()) {
                path.Add(currentNode);
                var selectedChild = currentNode.GetGreatestUCB(exploreFactor);

                if (selectedChild == null)
                    break;

                //批量添加虚拟损失，减少原子操作
                if (currentNode.ChildrenMap.Count > 1) {
                    selectedChild.AddVirtualLoss();
                }

                var move = selectedChild.PieceSelectedCompareToFather;
                moves.Add(move);
                currentNode = selectedChild;
                simulationPieceCount++;
            }

            path.Add(currentNode);

            //Expansion和Evaluation阶段 - 构建模拟棋盘进行推理
            var currentMove = currentNode.PieceSelectedCompareToFather ?? new Tuple<int, int>(-1, -1);

            //使用缓存棋盘，只在需要推理时构建
            var simulationBoard = ConstructSimulationBoard(rootBoard, moves);
            var gameResult = CheckGameOverByPiece(simulationBoard, currentMove.Item1, currentMove.Item2);

            if (gameResult != Role.Empty) {
                //游戏已结束-使用真实胜负结果进行反向传播
                currentNode.BackPropagation(gameResult);
            } else {
                var input = ConvertBoardToInput(simulationBoard, currentMove.Item1, currentMove.Item2);
                var (policy, networkValue) = RunInference(input);

                //尝试扩展节点（线程安全）
                if (currentNode.IsNewLeaf() && currentNode.TryStartExpansion()) {
                    MultiThreadNodeExpansionWithNN(currentNode, simulationBoard, policy, simulationPieceCount);
                    currentNode.FinishExpansion();
                }

                //🔥 关键修正：使用神经网络value进行反向传播，而不是直接判断胜负
                currentNode.BackPropagationWithValue(networkValue);
            }

            //Backpropagation阶段 - 批量更新减少竞争
            //批量清除虚拟损失，减少原子操作
            for (int i = path.Count - 1; i >= 0; i--) {
                var pathNode = path[i];
                if (pathNode.ChildrenMap.Count > 1) {
                    pathNode.RemoveVirtualLoss();
                }
            }
        }

        //高效构建模拟棋盘 - 只在推理时调用
        private List<List<Role>> ConstructSimulationBoard(List<List<Role>> rootBoard, List<Tuple<int, int>> moves) {
            var board = boardCache.Value;
            //确保棋盘大小正确
            if (board.Count != boardSize) {
                board.Clear();
                for (int i = 0; i < boardSize; i++) {
                    var row = new List<Role>(boardSize);
                    for (int j = 0; j < boardSize; j++) {
                        row.Add(Role.Empty);
                    }
                    board.Add(row);
                }
            }

            //重置棋盘为根状态
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    board[i][j] = rootBoard[i][j];
                }
            }

            //应用路径上的移动
            Role currentPlayer = Role.AI; // 假设AI先手，需要根据实际情况调整
            foreach (var move in moves) {
                board[move.Item1][move.Item2] = currentPlayer;
                currentPlayer = (currentPlayer == Role.AI) ? Role.Player : Role.AI;
            }

            return board;
        }

        //多线程节点扩展
        private void MultiThreadNodeExpansionWithNN(MultiThreadMCTSNode node, List<List<Role>> board, float[] policy, int simulationPieceCount) {
            var availableMoves = GetAvailablePositions(board);
            var nextPlayer = (node.LeadToThisStatus == Role.AI) ? Role.Player : Role.AI;

            foreach (var move in availableMoves) {
                var newBoard = CopyBoard(board);
                newBoard[move.Item1][move.Item2] = nextPlayer;

                int policyIndex = move.Item1 * boardSize + move.Item2;
                double priorProbability = (policyIndex < policy.Length) ? policy[policyIndex] : 0.0;

                var childNode = new MultiThreadMCTSNode(newBoard, node, move.Item1, move.Item2, nextPlayer, Role.Empty,
                    GetAvailablePositions(newBoard), priorProbability);

                node.AddSon(childNode, move.Item1, move.Item2);
            }
        }

        //强制扩展多线程节点
        private void MultiThreadNodeExpansionWithNNForced(MultiThreadMCTSNode node, List<List<Role>> board, bool force = false) {
            if (!node.IsLeaf && !force)
                return;

            var availableMoves = GetAvailablePositions(board);
            if (availableMoves.Count == 0 && force) {
                availableMoves = GetAllEmptyPositions(board);
            }
            if (availableMoves.Count == 0)
                return;

            var input = ConvertBoardToInput(board, -1, -1);
            var (policy, _) = RunInference(input);

            MultiThreadNodeExpansionWithNN(node, board, policy, PlayedPiecesCnt);
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
                    System.Diagnostics.Debug.WriteLine("错误：GetGreatestUCB返回 null！");
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

            //直接调用游戏结束判断，不需要传递棋子数
            var gameResult = CheckGameOverByPiece(currentBoard, currentMove.Item1, currentMove.Item2);

            Role winner;

            if (gameResult != Role.Empty) {
                //游戏已结束-使用真实胜负结果进行反向传播
                currentNode.BackPropagation(gameResult);
            } else {
                //游戏未结束，使用神经网络评估
                var input = ConvertBoardToInput(currentBoard, currentMove.Item1, currentMove.Item2);
                var (policy, networkValue) = RunInference(input);

                //新叶子节点-扩展
                if (currentNode.IsNewLeaf()) {
                    NodeExpansionWithNN(currentNode, currentBoard, policy, currentPieceCount);
                }

                //🔥 关键修正：使用神经网络value进行反向传播，而不是直接判断胜负
                currentNode.BackPropagationWithValue(networkValue);
            }
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


        /**********工具函数**********/
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

        //获取执行移动后的棋盘状态
        private List<List<Role>> GetBoardAfterMove(List<List<Role>> board, Tuple<int, int> move) {
            var newBoard = CopyBoard(board);
            newBoard[move.Item1][move.Item2] = Role.AI;
            return newBoard;
        }

        /**********模型调用工具函数-基础工具**********/
        //选择模型推理策略
        protected virtual (float[] policy, float value) RunInference(float[,,,] input) {
            //多线程模式：使用批量推理队列
            if (MultiThreadSearchEnabled) {
                return RunBatchInference(input);
            }
            //单线程模式：使用锁保护的直接推理
            return RunDirectInference(input);
        }

        //直接推理 - 单线程模式下的ONNX模型推理（带线程安全锁）
        private (float[] policy, float value) RunDirectInference(float[,,,] input) {
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

            //使用锁保护推理，避免多线程竞争
            lock (inferenceLock) {
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
        }

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
        }


        /**********模型调用工具函数-多线程使用**********/
        //批量推理 - AlphaGo Zero优化方案
        private (float[] policy, float value) RunBatchInference(float[,,,] input) {
            var request = new InferenceRequest(input);

            lock (queueLock) {
                inferenceQueue.Enqueue(request);
                inferenceReady.Set(); // 通知批处理线程
            }
            // 等待推理完成
            return request.CompletionSource.Task.Result;
        }

        //批量推理处理器 - 单线程处理所有推理请求
        private async Task BatchInferenceProcessor() {
            var requests = new List<InferenceRequest>();

            while (!stopBatchInference) {
                try {
                    // 等待推理请求
                    inferenceReady.WaitOne(BATCH_TIMEOUT_MS);

                    // 收集批量请求
                    lock (queueLock) {
                        while (inferenceQueue.Count > 0 && requests.Count < BATCH_SIZE) {
                            requests.Add(inferenceQueue.Dequeue());
                        }
                    }

                    if (requests.Count == 0)
                        continue;

                    // 执行批量推理
                    await ProcessBatchInference(requests);
                    requests.Clear();

                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine($"批量推理异常: {ex.Message}");
                    // 处理异常：完成所有等待的请求
                    foreach (var req in requests) {
                        req.CompletionSource.TrySetException(ex);
                    }
                    requests.Clear();
                }
            }
        }

        //处理批量推理
        private async Task ProcessBatchInference(List<InferenceRequest> requests) {
            if (session == null) {
                var error = new InvalidOperationException("模型未正确加载");
                foreach (var req in requests) {
                    req.CompletionSource.SetException(error);
                }
                return;
            }

            int batchSize = requests.Count;

            try {
                if (batchSize == 1) {
                    // 单个推理：直接处理
                    var singleResult = await Task.Run(() => ProcessSingleInference(requests[0].Input));
                    requests[0].CompletionSource.SetResult(singleResult);
                } else {
                    // 批量推理：组合输入
                    var batchInput = new float[batchSize, 4, boardSize, boardSize];
                    for (int i = 0; i < batchSize; i++) {
                        var input = requests[i].Input;
                        for (int c = 0; c < 4; c++) {
                            for (int h = 0; h < boardSize; h++) {
                                for (int w = 0; w < boardSize; w++) {
                                    batchInput[i, c, h, w] = input[0, c, h, w];
                                }
                            }
                        }
                    }

                    // 执行批量推理
                    var results = await Task.Run(() => ProcessBatchInferenceCore(batchInput, batchSize));

                    // 分发结果
                    for (int i = 0; i < batchSize; i++) {
                        requests[i].CompletionSource.SetResult(results[i]);
                    }
                }

            } catch (Exception ex) {
                foreach (var req in requests) {
                    req.CompletionSource.TrySetException(ex);
                }
            }
        }

        //单个推理处理
        private (float[] policy, float value) ProcessSingleInference(float[,,,] input) {
            // 转换输入
            var flatInput = new float[1 * 4 * boardSize * boardSize];
            int index = 0;
            for (int c = 0; c < 4; c++) {
                for (int h = 0; h < boardSize; h++) {
                    for (int w = 0; w < boardSize; w++) {
                        flatInput[index++] = input[0, c, h, w];
                    }
                }
            }

            var inputTensor = new DenseTensor<float>(flatInput, new int[] { 1, 4, boardSize, boardSize });
            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using var results = session!.Run(inputs);

            var policyOutput = results.First(r => r.Name == "policy").AsTensor<float>();
            var valueOutput = results.First(r => r.Name == "value").AsTensor<float>();

            var policy = new float[boardSize * boardSize];
            for (int i = 0; i < policy.Length; i++) {
                policy[i] = policyOutput[0, i];
            }

            return (policy, valueOutput[0, 0]);
        }

        //批量推理核心处理
        private List<(float[] policy, float value)> ProcessBatchInferenceCore(float[,,,] batchInput, int batchSize) {
            // 转换批量输入为1维数组
            var flatInput = new float[batchSize * 4 * boardSize * boardSize];
            int index = 0;
            for (int b = 0; b < batchSize; b++) {
                for (int c = 0; c < 4; c++) {
                    for (int h = 0; h < boardSize; h++) {
                        for (int w = 0; w < boardSize; w++) {
                            flatInput[index++] = batchInput[b, c, h, w];
                        }
                    }
                }
            }

            var inputTensor = new DenseTensor<float>(flatInput, new int[] { batchSize, 4, boardSize, boardSize });
            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            using var results = session!.Run(inputs);

            var policyOutput = results.First(r => r.Name == "policy").AsTensor<float>();
            var valueOutput = results.First(r => r.Name == "value").AsTensor<float>();

            var resultList = new List<(float[] policy, float value)>();
            for (int b = 0; b < batchSize; b++) {
                var policy = new float[boardSize * boardSize];
                for (int i = 0; i < policy.Length; i++) {
                    policy[i] = policyOutput[b, i];
                }
                resultList.Add((policy, valueOutput[b, 0]));
            }

            return resultList;
        }


        /**********模型生命周期管理**********/
        //游戏开始时的初始化
        public override void GameStart(bool IsAIFirst) {
            end = true;
            multiThreadEnd = true;

            if (searchTask != null && !searchTask.IsCompleted) {
                try {
                    searchTask.Wait(1000);
                } catch {
                    // 忽略等待异常
                }
            }

            if (multiThreadSearchTasks != null) {
                try {
                    Task.WaitAll(multiThreadSearchTasks.ToArray(), 1000);
                } catch {
                    // 忽略等待异常
                }
            }

            //重置状态
            end = false;
            multiThreadEnd = false;
            PlayedPiecesCnt = 0;
            Interlocked.Exchange(ref completedSearches, 0);

            //如果 session 已被释放或模型未加载，重新加载模型
            if (session == null || !isModelLoaded) {
                if (modelBytes == null) {
                    throw new InvalidOperationException("模型字节数组为空，无法重新加载模型");
                }
                LoadModel(modelBytes);
            }
            //在游戏开始时预热模型
            WarmUpModel();

            //初始化棋盘
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

            //根据搜索模式创建相应的根节点和搜索任务
            if (useMonteCarlo && MultiThreadSearchEnabled) {
                //多线程搜索模式：启动批量推理处理器
                MultiThreadRootNode = new MultiThreadMCTSNode(board, null, -1, -1, WhoLeadTo, Role.Empty, GetAvailablePositions(board));

                //启动批量推理处理器
                stopBatchInference = false;
                batchInferenceTask = Task.Run(BatchInferenceProcessor);

                //启动多线程搜索任务
                multiThreadSearchTasks = new List<Task>();
                for (int i = 0; i < ThreadCount; i++) {
                    multiThreadSearchTasks.Add(Task.Run(() => MultiThreadSearchWorker()));
                }
                System.Diagnostics.Debug.WriteLine($"批量推理多线程搜索已启动：{ThreadCount}个搜索线程");
            } else if (useMonteCarlo && SearchTreeReuseEnabled) {
                //单线程搜索树重用模式
                RootNode = new MCTSNode(board, null, -1, -1, WhoLeadTo, Role.Empty, GetAvailablePositions(board));
                searchTask = Task.Run(() => EvalToGo());
            }
        }

        //释放资源
        public override void GameForcedEnd() {
            System.Diagnostics.Debug.WriteLine("GameForcedEnd: 释放模型资源");
            end = true;
            multiThreadEnd = true;
            stopBatchInference = true;

            // 通知等待的线程
            lock (mutex) {
                Monitor.PulseAll(mutex);
            }

            lock (multiThreadLock) {
                Monitor.PulseAll(multiThreadLock);
            }

            // 通知批量推理处理器停止
            inferenceReady.Set();

            // 等待批量推理任务结束
            if (batchInferenceTask != null && !batchInferenceTask.IsCompleted) {
                try {
                    batchInferenceTask.Wait(2000);
                } catch {
                    // 忽略等待异常
                }
            }

            // 等待搜索任务结束
            if (searchTask != null && !searchTask.IsCompleted) {
                try {
                    searchTask.Wait(2000); // 等待最多2秒
                } catch {
                    // 忽略等待异常
                }
            }

            // 等待多线程搜索任务结束
            if (multiThreadSearchTasks != null) {
                try {
                    Task.WaitAll(multiThreadSearchTasks.ToArray(), 2000);
                } catch {
                    // 忽略等待异常
                }
            }

            searchTask = null;
            RootNode = null;
            multiThreadSearchTasks = null;
            MultiThreadRootNode = null;
            batchInferenceTask = null;

            //清理推理队列
            lock (queueLock) {
                while (inferenceQueue.Count > 0) {
                    var request = inferenceQueue.Dequeue();
                    request.CompletionSource.TrySetCanceled();
                }
            }

            //释放session和清理资源
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
