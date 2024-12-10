/*************************************************************************************
 * 文 件 名:   DRL.cs
 * 描    述: 深度强化学习抽象产品
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class DRL : AbstractAIStrategy {
        public DRL() {
            //加载模型
            OnnxModel = new InferenceSession(ModelPath);
        }

        //模型实例
        private InferenceSession OnnxModel;
        protected string ModelPath;

        // 获取模型决策
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            var modelInput = GetInput(currentBoard, lastX, lastY);
            return RunModel(modelInput);
        }

        // 获取模型合法输入
        private float[] GetInput(List<List<Role>> currentBoard, int lastX, int lastY) {
            // 转换当前棋盘状态为模型可接受的输入格式
            float[] modelInput = new float[4 * 8 * 8]; // 4 通道，8x8棋盘

            // 根据当前的棋盘状态和最近落子的情况来填充模型输入
            // currentBoard: 当前棋盘状态
            // lastX, lastY: 最近落子的坐标

            // 示例填充逻辑：
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    var role = currentBoard[i][j];
                    int index = i * 8 + j;
                    // 根据 role 填充模型输入（示例逻辑）
                    modelInput[index] = role == Role.AI ? 1.0f : (role == Role.Player ? -1.0f : 0.0f);
                }
            }

            // 标记最近一次落子的坐标
            modelInput[lastX * 8 + lastY + 4 * 64] = 1.0f;

            return modelInput;
        }

        // 运行模型并返回决策
        private Tuple<int, int> RunModel(float[] modelInput) {
            // 创建输入张量
            var inputTensor = new DenseTensor<float>(modelInput, new[] { 1, 4, 8, 8 });

            // 创建输入字典
            var inputs = new NamedOnnxValue[] {
                NamedOnnxValue.CreateFromTensor("input", inputTensor)
            };

            // 执行推理
            var results = OnnxModel.Run(inputs);

            // 获取模型输出结果
            var output = results.First().AsTensor<float>();

            // 处理输出结果，选择落子位置
            // 假设输出中包含多个决策，选择最高概率的
            int bestMoveIndex = Array.IndexOf(output.ToArray(), output.Max());
            int moveX = bestMoveIndex / 8;
            int moveY = bestMoveIndex % 8;

            return Tuple.Create(moveX, moveY);
        }




        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) { }
        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() { }

        //游戏开始
        public override void GameStart(bool IsAIFirst) { }
    }
}
