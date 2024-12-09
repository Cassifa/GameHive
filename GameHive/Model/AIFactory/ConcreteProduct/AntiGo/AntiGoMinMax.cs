/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMInMax.cs
 * 描    述: α-β剪枝博弈树黑白棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class AntiGoMinMax : MinMax {
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (currentBoard[0][0] != Role.Empty) {
                return currentBoard[0][0];
            }
            return Role.Empty;
        }

        public override void GameForcedEnd() {
            throw new NotImplementedException();
        }

        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            Random rand = new Random();
            // 获取棋盘大小
            int rowCount = currentBoard.Count;
            int colCount = currentBoard[0].Count;
            // 收集所有不为 Empty 的点
            List<Tuple<int, int>> availableMoves = new List<Tuple<int, int>>();

            for (int i = 0; i < rowCount; i++) {
                for (int j = 0; j < colCount; j++) {
                    if (currentBoard[i][j] == Role.Empty) {
                        availableMoves.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            int randomIndex = rand.Next(availableMoves.Count);
            return availableMoves[randomIndex];
        }

        public override void UserPlayPiece(int lastX, int lastY) {
            throw new NotImplementedException();
        }

        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            throw new NotImplementedException();
        }

        protected override HashSet<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            throw new NotImplementedException();
        }

        protected override HashSet<Tuple<int, int>> GetAvailableMovesByNewPieces(List<List<Role>> currentBoard, HashSet<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            throw new NotImplementedException();
        }

        protected override List<List<Role>> GetCurrentBoard() {
            throw new NotImplementedException();
        }

        protected override void PlayChess(int x, int y, Role role) {
            throw new NotImplementedException();
        }
    }
}
