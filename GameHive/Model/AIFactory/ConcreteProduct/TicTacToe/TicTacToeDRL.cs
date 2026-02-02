/*************************************************************************************
 * 文 件 名:   TicTacToeDRL.cs
 * 描    述: 
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2026/2/2 11:01
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class TicTacToeDRL : DeepRL {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(3);

        public TicTacToeDRL(int boardSize, DifficultyLevel level) {
            this.boardSize = boardSize;  // 3x3
            concreteProductInfo.TotalPiecesCnt = boardSize;
            exploreFactor = 5.0;

            byte[] modelBytes = Properties.Resources.tictactoe_2000;

            switch (level) {
                case DifficultyLevel.LEVEL_1: // 纯神经网络评估
                    useMonteCarlo = false;
                    SearchCount = 0;
                    break;
                case DifficultyLevel.LEVEL_2: // 基础 MCTS 搜索
                    useMonteCarlo = true;
                    SearchCount = 100;
                    break;
                case DifficultyLevel.LEVEL_3: // 高强度 MCTS 搜索
                    useMonteCarlo = true;
                    SearchCount = 1600;
                    break;
                default:
                    useMonteCarlo = false;
                    SearchCount = 0;
                    break;
            }

            if (modelBytes == null || modelBytes.Length == 0) {
                throw new InvalidOperationException($"难度级别 {level} 对应的模型加载失败");
            }

            //加载模型
            LoadModel(modelBytes);
        }

        //判断游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            // 从棋盘计算棋子数量（用于 MCTS 模拟中的正确判断）
            int pieceCount = 0;
            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                    if (currentBoard[i][j] != Role.Empty)
                        pieceCount++;

            if (x == -1) {
                if (pieceCount >= boardSize * boardSize) {
                    return Role.Draw;
                }
                return Role.Empty;
            }

            // 检查行
            if (currentBoard[x][0] != Role.Empty &&
                currentBoard[x][0] == currentBoard[x][1] &&
                currentBoard[x][1] == currentBoard[x][2]) {
                return currentBoard[x][0];
            }
            // 检查列
            if (currentBoard[0][y] != Role.Empty &&
                currentBoard[0][y] == currentBoard[1][y] &&
                currentBoard[1][y] == currentBoard[2][y]) {
                return currentBoard[0][y];
            }
            // 检查主对角线
            if (x == y && currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0];
            }
            // 检查副对角线
            if (x + y == boardSize - 1 && currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2];
            }
            // 检查平局
            if (pieceCount >= boardSize * boardSize)
                return Role.Draw;
            return Role.Empty;
        }

        //获取可行落子点
        protected override List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard) {
            var availablePositions = new List<Tuple<int, int>>();
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    if (currentBoard[i][j] == Role.Empty) {
                        availablePositions.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return availablePositions;
        }
    }
}
