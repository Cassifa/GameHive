/*************************************************************************************
 * 文 件 名:   MisereTicTacToeNegamax.cs
 * 描    述: 负极大值搜索井字棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class TicTacToeNegamax : Negamax {
        /*****实现两个策略*****/
        //检查游戏是否结束，并返回赢家（平局Draw 未结束Empty）
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            int boardSize = currentBoard.Count;
            // 检查行
            for (int i = 0; i < boardSize; i++) {
                if (currentBoard[i][0] != Role.Empty &&
                    currentBoard[i][0] == currentBoard[i][1] &&
                    currentBoard[i][1] == currentBoard[i][2]) {
                    return currentBoard[i][0]; // 返回当前行的赢家
                }
            }
            // 检查列
            for (int j = 0; j < boardSize; j++) {
                if (currentBoard[0][j] != Role.Empty &&
                    currentBoard[0][j] == currentBoard[1][j] &&
                    currentBoard[1][j] == currentBoard[2][j]) {
                    return currentBoard[0][j]; // 返回当前列的赢家
                }
            }
            // 检查主对角线
            if (currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0]; // 返回主对角线的赢家
            }
            // 检查副对角线
            if (currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2]; // 返回副对角线的赢家
            }
            foreach (var row in currentBoard) {
                if (row.Contains(Role.Empty)) {
                    return Role.Empty;
                }
            }
            return Role.Draw;
        }

        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            var moves = new List<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] == Role.Empty) {
                        moves.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return moves;
        }
    }
}
