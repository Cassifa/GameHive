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
        protected bool isModelWarmedUp = false; //模型是否已预热
        protected byte[]? modelBytes; //模型字节数组

        //MCTS相关参数
        protected int mctsSimulations = 400; //MCTS模拟次数
        protected double cPuct = 5.0; //UCB公式中的探索常数
        protected double temperature = 1e-3; //温度参数，控制探索程度
        protected bool isTraining = false; //是否为训练模式

        //获取可行落子点(纯虚函数，由子类实现)
        protected abstract List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard);


        /**********算法输入获取方法**********/
        //获取AI的下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //根据是否使用蒙特卡洛搜索决定策略
            if (useMonteCarlo) {
                return GetMCTSMove(currentBoard, lastX, lastY);
            } else {
                return GetDirectModelMove(currentBoard, lastX, lastY);
            }
        }

        //直接使用模型获取移动
        protected virtual Tuple<int, int> GetDirectModelMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            var input = ConvertBoardToInput(currentBoard, lastX, lastY);
            var (policy, value) = RunInference(input);
            return SelectBestMove(policy, currentBoard);
        }


        //使用蒙特卡洛搜索获取最佳移动(基于深度学习模型优化的MCTS)
        protected virtual Tuple<int, int> GetMCTSMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            Console.WriteLine($"开始MCTS搜索，模拟次数: {mctsSimulations}");

            //创建MCTS根节点 - 当前轮到AI下棋
            var rootNode = new MCTSNode(currentBoard, null, -1, -1, Role.AI, Role.Empty, GetAvailablePositions(currentBoard));

            //执行MCTS模拟
            for (int i = 0; i < mctsSimulations; i++) {
                MCTSPlayoutWithNN(rootNode, CopyBoard(currentBoard));
            }

            //根据访问次数选择最佳移动
            var bestMove = SelectMoveFromMCTS(rootNode);
            Console.WriteLine($"MCTS搜索完成，选择移动: ({bestMove.Item1}, {bestMove.Item2})");
            return bestMove;
        }


        /**********嵌入DRL的MCTS方法**********/

        //基于神经网络的MCTS推演过程
        private void MCTSPlayoutWithNN(MCTSNode node, List<List<Role>> board) {
            var path = new List<MCTSNode>();
            var currentNode = node;
            var currentBoard = board;

            //选择阶段：从根节点向下选择到叶子节点
            while (!currentNode.IsLeaf) {
                path.Add(currentNode);
                var selectedChild = currentNode.GetGreatestUCB();
                var move = selectedChild.PieceSelectedCompareToFather;
                currentNode = selectedChild;
                //在棋盘上执行动作
                currentBoard[move.Item1][move.Item2] = currentNode.LeadToThisStatus;
            }

            path.Add(currentNode);

            //评估阶段：使用神经网络评估叶子节点
            var gameResult = CheckGameOverByPiece(currentBoard, currentNode.PieceSelectedCompareToFather.Item1, currentNode.PieceSelectedCompareToFather.Item2);
            Role winner;

            if (gameResult != Role.Empty) {
                //游戏已结束，使用真实结果
                winner = gameResult;
            } else {
                //游戏未结束，使用神经网络评估并扩展节点
                if (currentNode.IsNewLeaf()) {
                    //扩展节点时使用神经网络指导
                    ExpandNodeWithNN(currentNode, currentBoard);
                    //使用神经网络评估当前局面
                    var input = ConvertBoardToInput(currentBoard, currentNode.PieceSelectedCompareToFather.Item1, currentNode.PieceSelectedCompareToFather.Item2);
                    var (policy, networkValue) = RunInference(input);
                    //根据网络评估决定模拟胜者
                    winner = (networkValue > 0) ? Role.AI : Role.Player;
                } else {
                    //已经扩展过的节点，继续使用传统MCTS
                    winner = SimulateRandomPlayout(currentBoard, currentNode.LeadToThisStatus);
                }
            }

            //反向传播阶段
            currentNode.BackPropagation(winner);
        }

        //使用神经网络指导扩展节点
        private void ExpandNodeWithNN(MCTSNode node, List<List<Role>> board) {
            var availableMoves = GetAvailablePositions(board);
            var nextPlayer = (node.LeadToThisStatus == Role.AI) ? Role.Player : Role.AI;

            //获取神经网络的策略输出
            var input = ConvertBoardToInput(board, node.PieceSelectedCompareToFather.Item1, node.PieceSelectedCompareToFather.Item2);
            var (policy, value) = RunInference(input);

            foreach (var move in availableMoves) {
                var newBoard = CopyBoard(board);
                newBoard[move.Item1][move.Item2] = nextPlayer;

                var childNode = new MCTSNode(newBoard, node, move.Item1, move.Item2, nextPlayer, Role.Empty, GetAvailablePositions(newBoard));

                //使用神经网络的策略概率来影响初始访问
                int policyIndex = move.Item1 * boardSize + move.Item2;
                if (policyIndex < policy.Length && policy[policyIndex] > 0.1) {
                    //对于高概率的移动，给予额外的初始访问次数
                    childNode.BackPropagation(nextPlayer); //模拟一次有利结果
                }

                node.AddSon(childNode, move.Item1, move.Item2);
            }

            node.IsLeaf = false;
        }

        //传统随机模拟
        private Role SimulateRandomPlayout(List<List<Role>> board, Role currentPlayer) {
            var simulationBoard = CopyBoard(board);
            var player = (currentPlayer == Role.AI) ? Role.Player : Role.AI;
            var random = new Random();

            while (true) {
                var availableMoves = GetAvailablePositions(simulationBoard);
                if (availableMoves.Count == 0) {
                    return Role.Draw;
                }

                var move = availableMoves[random.Next(availableMoves.Count)];
                simulationBoard[move.Item1][move.Item2] = player;

                var result = CheckGameOverByPiece(simulationBoard, move.Item1, move.Item2);
                if (result != Role.Empty) {
                    return result;
                }

                player = (player == Role.AI) ? Role.Player : Role.AI;
            }
        }

        //从MCTS结果中选择移动
        private Tuple<int, int> SelectMoveFromMCTS(MCTSNode rootNode) {
            if (rootNode.ChildrenMap.Count == 0) {
                //如果没有子节点，随机选择一个可行位置
                var availableMoves = GetAvailablePositions(rootNode.NodeBoard);
                Console.WriteLine("警告：根节点没有子节点，随机选择移动");
                return availableMoves[new Random().Next(availableMoves.Count)];
            }

            //打印所有子节点的统计信息
            Console.WriteLine("MCTS子节点统计:");
            var sortedChildren = rootNode.ChildrenMap.Values.OrderByDescending(child => GetNodeVisitCount(child));
            foreach (var child in sortedChildren.Take(5)) { //只显示前5个
                var move = child.PieceSelectedCompareToFather;
                Console.WriteLine($"  位置({move.Item1},{move.Item2}): 访问{GetNodeVisitCount(child)}次, UCB{child.GetUCB():F3}");
            }

            //选择访问次数最多的移动
            var bestChild = rootNode.ChildrenMap.Values.OrderByDescending(child => GetNodeVisitCount(child)).First();
            return bestChild.PieceSelectedCompareToFather;
        }

        //获取节点访问次数
        private int GetNodeVisitCount(MCTSNode node) {
            return node.VisitedTimes;
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
        }
    }
}
