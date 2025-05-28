/*************************************************************************************
 * 文 件 名:   DeepRL.cs
 * 描    述:   深度强化学习抽象产品
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/4/27 19:58
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class DeepRL : AbstractAIStrategy {
        protected InferenceSession? session;
        protected string modelResourceName;
        protected int boardSize;
        protected int PlayedPiecesCnt = 0;
        protected bool isModelLoaded = false;
        protected bool useMonteCarlo = false; //是否使用蒙特卡洛搜索
        protected bool isModelWarmedUp = false; //模型是否已预热

        //获取可行落子点(纯虚函数，由子类实现)
        protected abstract List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard);


        //获取AI的下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //根据是否使用蒙特卡洛搜索决定策略
            if (useMonteCarlo) {
                return GetMCTSMove(currentBoard, lastX, lastY);
            } else {
                return GetDirectModelMove(currentBoard, lastX, lastY);
            }
        }


        //使用蒙特卡洛搜索获取最佳移动(子类可重写实现具体的MCTS逻辑)
        protected virtual Tuple<int, int> GetMCTSMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //默认实现：直接使用模型推理
            var input = ConvertBoardToInput(currentBoard, lastX, lastY);
            var (policy, value) = RunInference(input);
            return SelectBestMove(policy, currentBoard);
        }

        //直接使用模型获取移动
        protected virtual Tuple<int, int> GetDirectModelMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            var input = ConvertBoardToInput(currentBoard, lastX, lastY);
            var (policy, value) = RunInference(input);
            return SelectBestMove(policy, currentBoard);
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
                LoadModel();
            }
            //在游戏开始时预热模型
            WarmUpModel();
        }

        //加载ONNX模型
        protected void LoadModel() {
            //通过资源管理器加载模型
            var modelBytes = GetModelBytesFromResource(modelResourceName);
            if (modelBytes == null || modelBytes.Length == 0) {
                throw new InvalidOperationException($"模型资源不存在: {modelResourceName}");
            }
            session = new InferenceSession(modelBytes);
            isModelLoaded = true;
            Console.WriteLine($"成功加载模型: {modelResourceName}");
        }

        //从资源中获取模型字节数组
        private byte[]? GetModelBytesFromResource(string resourceName) {
            var modelBytes = Properties.Resources.model_3000;//默认值
            switch (resourceName) {
                case "model_3000.onnx":
                    modelBytes = Properties.Resources.model_3000;
                    break;
                default:
                    modelBytes = Properties.Resources.model_3000;
                    break;
            }
            if (resourceName == "model_3000.onnx") {
                if (modelBytes == null || modelBytes.Length == 0) {
                    throw new InvalidOperationException($"模型资源 {resourceName} 为空或未正确嵌入");
                }
                Console.WriteLine($"成功读取模型资源，大小: {modelBytes.Length} 字节");
                return modelBytes;
            }

            throw new InvalidOperationException($"不支持的模型资源: {resourceName}");
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
