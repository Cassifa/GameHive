/*************************************************************************************
 * 文 件 名:   GoBang88DRL.cs
 * 描    述: 深度强化学习8*8五子棋产品实例
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:36
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class GoBang88DRL : DeepRL {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(3);

        private DifficultyLevel currentLevel;

        public GoBang88DRL(int boardSize, DifficultyLevel level) {
            this.boardSize = boardSize;
            currentLevel = level;
            concreteProductInfo.TotalPiecesCnt = boardSize;
            switch (level) {
                case DifficultyLevel.LEVEL_1:
                    useMonteCarlo = false;
                    modelResourceName = "model_3000.onnx";
                    break;
                case DifficultyLevel.LEVEL_2:
                    useMonteCarlo = true;
                    modelResourceName = "model_3000.onnx";
                    break;
            }
            //加载模型
            LoadModel();
        }

        //判断游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (x == -1)
                return Role.Empty;
            Role currentPlayer = currentBoard[x][y];
            //水平、垂直、主对角线、副对角线
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };
            for (int direction = 0; direction < 4; direction++) {
                int count = 1;
                // 检查当前方向上的连续棋子，向正方向（dx[direction], dy[direction]）和反方向（-dx[direction], -dy[direction]）扩展
                for (int step = 1; step <= 4; step++) {
                    int nx = x + dx[direction] * step;
                    int ny = y + dy[direction] * step;
                    if (nx < 0 || ny < 0 || nx >= boardSize || ny >= boardSize || currentBoard[nx][ny] != currentPlayer)
                        break;
                    count++;
                }
                for (int step = 1; step <= 4; step++) {
                    int nx = x - dx[direction] * step;
                    int ny = y - dy[direction] * step;
                    if (nx < 0 || ny < 0 || nx >= boardSize || ny >= boardSize || currentBoard[nx][ny] != currentPlayer)
                        break;
                    count++;
                }
                if (count >= 5)
                    return currentPlayer;
            }
            if (boardSize * boardSize == PlayedPiecesCnt)
                return Role.Draw;
            return Role.Empty;
        }

        //获取可行落子点
        protected override List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard) {
            var availablePositions = new List<Tuple<int, int>>();
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    if (currentBoard[i][j] == Role.Empty) {
                        availablePositions.Add(Tuple.Create(i, j));
                    }
                }
            }
            return availablePositions;
        }
    }
}
