/*************************************************************************************
 * 文 件 名:   DeepRL.cs
 * 描    述:   深度强化学习抽象产品 (简化版)
 *          核心功能：神经网络策略评估 + 价值网络 + MCTS融合
 *          支持模式：1.直接神经网络评估 2.基础 DRL-MCTS 3.MCTS+搜索树重用
 * 版    本：  V4.4 添加搜索树重用功能
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

        protected InferenceSession? session;
        protected int boardSize;
        protected int PlayedPiecesCnt = 0;
        protected bool isModelLoaded = false;
        protected bool useMonteCarlo = false;
        protected bool isModelWarmedUp = false;
        protected byte[]? modelBytes;
        protected double exploreFactor = 5.0;
        protected int SearchCount = 1600;
        
        // 搜索树重用相关
        protected bool reuseSearchTree = false;  // 是否重用搜索树
        private MCTSNode? persistentRoot = null; // 持久化的搜索树根节点

        private readonly object inferenceLock = new object();

        protected abstract List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard);


        /**********算法输出获取方法**********/
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            Tuple<int, int> move;
            if (useMonteCarlo) {
                move = GetMCTSMove(currentBoard, lastX, lastY);
            } else {
                move = GetDirectModelMove(currentBoard, lastX, lastY);
            }
            PlayedPiecesCnt++;
            return move;
        }

        //模式1：直接使用DRL模型获取
        protected virtual Tuple<int, int> GetDirectModelMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            var input = ConvertBoardToInput(currentBoard, Role.AI, lastX, lastY);
            var (policy, value) = RunInference(input);
            return SelectBestMove(policy, currentBoard);
        }

        //模式2：基础 MCTS 搜索（支持搜索树重用）
        protected virtual Tuple<int, int> GetMCTSMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            MCTSNode rootNode;
            
            if (reuseSearchTree && persistentRoot != null) {
                // 重用搜索树：尝试找到对应玩家落子的子节点
                rootNode = MoveToChild(persistentRoot, lastX, lastY);
                if (rootNode == null) {
                    // 如果找不到对应子节点，创建新的根节点
                    rootNode = new MCTSNode(currentBoard, null, lastX, lastY, Role.Player, Role.Empty, GetAvailablePositions(currentBoard));
                }
            } else {
                // 不重用搜索树：每次创建新的根节点
                rootNode = new MCTSNode(currentBoard, null, lastX, lastY, Role.Player, Role.Empty, GetAvailablePositions(currentBoard));
            }
            
            // 执行固定次数的模拟（Python 风格：不预先扩展 root）
            for (int i = 0; i < SearchCount; i++) {
                SimulationOnce(rootNode, CopyBoard(currentBoard));
            }

            // 选择访问次数最多的子节点
            var bestChild = rootNode.ChildrenMap.Values.OrderByDescending(child => child.VisitedTimes).FirstOrDefault();
            if (bestChild == null) {
                persistentRoot = null;
                return GetDirectModelMove(currentBoard, lastX, lastY);
            }

            var bestMove = bestChild.PieceSelectedCompareToFather;
            
            // 更新持久化根节点（AI 下棋后，子节点成为新的根）
            if (reuseSearchTree) {
                persistentRoot = MoveToChild(rootNode, bestMove.Item1, bestMove.Item2);
            }

            return bestMove;
        }
        
        // 将搜索树根节点移动到指定子节点（用于搜索树重用）
        private MCTSNode? MoveToChild(MCTSNode node, int x, int y) {
            if (x == -1 || y == -1) return null;
            
            int key = x * 100 + y;
            if (node.ChildrenMap.TryGetValue(key, out MCTSNode? child)) {
                // 断开与父节点的连接，使其成为新的根
                return child;
            }
            return null;
        }

        // 扩展节点
        private void ExpandNode(MCTSNode node, List<List<Role>> board, float[] logPolicy, Role nextPlayer) {
            var availableMoves = GetAvailablePositions(board);
            if (availableMoves.Count == 0) return;
            
            var probs = ConvertLogProbToProb(logPolicy, availableMoves);

            for (int i = 0; i < availableMoves.Count; i++) {
                var move = availableMoves[i];
                var childNode = new MCTSNode(null, node, move.Item1, move.Item2, nextPlayer, Role.Empty, null, probs[i]);
                node.AddSon(childNode, move.Item1, move.Item2);
            }
            node.IsLeaf = false;
        }


        /**********MCTS 核心方法**********/
        private void SimulationOnce(MCTSNode rootNode, List<List<Role>> board) {
            var currentNode = rootNode;
            var currentBoard = board;

            // Selection - 使用 DRL 专用 UCB
            while (!currentNode.IsLeaf) {
                var selectedChild = currentNode.GetGreatestDRLUCB(exploreFactor);
                if (selectedChild == null) break;

                var move = selectedChild.PieceSelectedCompareToFather;
                currentNode = selectedChild;
                currentBoard[move.Item1][move.Item2] = currentNode.LeadToThisStatus;
            }

            Role currentPlayer = (currentNode.LeadToThisStatus == Role.AI) ? Role.Player : Role.AI;
            var currentMove = currentNode.PieceSelectedCompareToFather ?? new Tuple<int, int>(-1, -1);

            // Evaluation
            var gameResult = CheckGameOverByPiece(currentBoard, currentMove.Item1, currentMove.Item2);

            double leafValue;
            if (gameResult != Role.Empty) {
                if (gameResult == Role.Draw) {
                    leafValue = 0.0;
                } else {
                    leafValue = (gameResult == currentPlayer) ? 1.0 : -1.0;
                }
            } else {
                var input = ConvertBoardToInput(currentBoard, currentPlayer, currentMove.Item1, currentMove.Item2);
                var (logPolicy, networkValue) = RunInference(input);

                if (currentNode.IsNewLeaf()) {
                    ExpandNode(currentNode, currentBoard, logPolicy, currentPlayer);
                }
                
                leafValue = networkValue;
            }

            // Backup
            double valueForBackprop = (currentPlayer == Role.AI) ? leafValue : -leafValue;
            currentNode.BackPropagationWithValue(valueForBackprop);
        }
        
        // 将 log probability 转换为 probability
        // Python: act_probs = np.exp(log_act_probs) 直接 exp，不归一化！
        // 重要：policy 输出使用原始位置索引，不需要翻转！翻转只在输入时做
        private float[] ConvertLogProbToProb(float[] logPolicy, List<Tuple<int, int>> availableMoves) {
            var probs = new float[availableMoves.Count];
            
            for (int i = 0; i < availableMoves.Count; i++) {
                var move = availableMoves[i];
                // 使用原始位置索引（Python: act_probs[available_moves]）
                int policyIndex = move.Item1 * boardSize + move.Item2;
                float logProb = (policyIndex < logPolicy.Length) ? logPolicy[policyIndex] : -100f;
                // 直接 exp，不归一化（和 Python 一致）
                probs[i] = (float)Math.Exp(logProb);
            }
            
            return probs;
        }


        /**********工具函数**********/
        private List<List<Role>> CopyBoard(List<List<Role>> board) {
            return board.Select(row => new List<Role>(row)).ToList();
        }

        /**********模型调用**********/
        protected virtual (float[] policy, float value) RunInference(float[,,,] input) {
            if (session == null) throw new InvalidOperationException("模型未正确加载");

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

            var inputTensor = new DenseTensor<float>(flatInput, new int[] { 1, 4, boardSize, boardSize });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", inputTensor) };

            lock (inferenceLock) {
                using var results = session.Run(inputs);
                var policyOutput = results.First(r => r.Name == "action_probs").AsTensor<float>();
                var valueOutput = results.First(r => r.Name == "value").AsTensor<float>();
                
                var policy = new float[boardSize * boardSize];
                for (int i = 0; i < policy.Length; i++) {
                    policy[i] = policyOutput[0, i];
                }
                return (policy, valueOutput[0, 0]);
            }
        }

        /// <summary>
        /// 将棋盘状态转换为神经网络输入（相对视角 + 行翻转）
        /// Python: square_state[:, ::-1, :] 对 row 维度翻转
        /// </summary>
        protected virtual float[,,,] ConvertBoardToInput(List<List<Role>> currentBoard, Role currentPlayer, int lastX = -1, int lastY = -1) {
            var input = new float[1, 4, boardSize, boardSize];
            Role opponent = (currentPlayer == Role.AI) ? Role.Player : Role.AI;
            
            int pieceCount = 0;
            
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    // 关键：翻转 row 来匹配 Python 的 [:, ::-1, :] 翻转
                    int flippedI = boardSize - 1 - i;
                    
                    // Channel 0: 当前玩家（Self）的棋子
                    if (currentBoard[i][j] == currentPlayer) {
                        input[0, 0, flippedI, j] = 1.0f;
                        pieceCount++;
                    }
                    // Channel 1: 对手（Opponent）的棋子
                    else if (currentBoard[i][j] == opponent) {
                        input[0, 1, flippedI, j] = 1.0f;
                        pieceCount++;
                    }
                    // Channel 2: 上一步落子位置
                    if (lastX == i && lastY == j && lastX != -1) {
                        input[0, 2, flippedI, j] = 1.0f;
                    }
                }
            }
            
            // Channel 3: 颜色/轮次标记
            if (pieceCount % 2 == 0) {
                for (int i = 0; i < boardSize; i++)
                    for (int j = 0; j < boardSize; j++)
                        input[0, 3, i, j] = 1.0f;
            }
            
            return input;
        }

        private Tuple<int, int> SelectBestMove(float[] logPolicy, List<List<Role>> currentBoard) {
            var availablePositions = GetAvailablePositions(currentBoard);
            var bestMove = availablePositions[0];
            // 使用原始位置索引（不翻转）
            float bestLogProb = logPolicy[bestMove.Item1 * boardSize + bestMove.Item2];
            
            foreach (var pos in availablePositions) {
                int index = pos.Item1 * boardSize + pos.Item2;
                if (logPolicy[index] > bestLogProb) {
                    bestLogProb = logPolicy[index];
                    bestMove = pos;
                }
            }
            return bestMove;
        }

        protected void LoadModel(byte[] modelBytes) {
            if (modelBytes == null || modelBytes.Length == 0) throw new InvalidOperationException("模型资源不存在");
            session = new InferenceSession(modelBytes);
            isModelLoaded = true;
            this.modelBytes = modelBytes;
        }

        private void WarmUpModel() {
            if (session == null || isModelWarmedUp) return;
            var dummyInput = new float[1 * 4 * boardSize * boardSize];
            var inputTensor = new DenseTensor<float>(dummyInput, new int[] { 1, 4, boardSize, boardSize });
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", inputTensor) };
            using var results = session.Run(inputs);
            isModelWarmedUp = true;
        }

        /**********模型生命周期管理**********/
        public override void GameStart(bool IsAIFirst) {
            PlayedPiecesCnt = 0;
            persistentRoot = null;  // 重置搜索树
            if (session == null || !isModelLoaded) {
                if (modelBytes != null) LoadModel(modelBytes);
            }
            WarmUpModel();
        }

        public override void GameForcedEnd() {
            session?.Dispose();
            session = null;
            isModelLoaded = false;
            isModelWarmedUp = false;
            persistentRoot = null;  // 清理搜索树
        }

        public override void UserPlayPiece(int lastX, int lastY) {
            PlayedPiecesCnt++;
            // 搜索树重用：玩家下棋后，更新根节点到对应的子节点
            if (reuseSearchTree && persistentRoot != null) {
                persistentRoot = MoveToChild(persistentRoot, lastX, lastY);
            }
        }
    }
}
