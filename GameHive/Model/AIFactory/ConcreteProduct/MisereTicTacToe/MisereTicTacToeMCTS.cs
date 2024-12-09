/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMCTS.cs
 * 描    述: 蒙塔卡洛搜索反井字棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:37
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class MisereTicTacToeMCTS : MCTS {
        /*****实现两个策略*****/
        public override Role CheckGameOver(List<List<Role>> currentBoard) {
            int boardSize = currentBoard.Count;

            // 检查行
            for (int i = 0; i < boardSize; i++)
                if (currentBoard[i][0] != Role.Empty &&
                    currentBoard[i][0] == currentBoard[i][1] &&
                    currentBoard[i][1] == currentBoard[i][2]) {
                    return currentBoard[i][0] == Role.AI ? Role.Player : Role.AI;
                }
            // 检查列
            for (int j = 0; j < boardSize; j++)
                if (currentBoard[0][j] != Role.Empty &&
                    currentBoard[0][j] == currentBoard[1][j] &&
                    currentBoard[1][j] == currentBoard[2][j]) {
                    return currentBoard[0][j] == Role.AI ? Role.Player : Role.AI;
                }

            // 检查主对角线
            if (currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0] == Role.AI ? Role.Player : Role.AI;
            }

            // 检查副对角线
            if (currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2] == Role.AI ? Role.Player : Role.AI;
            }
            //return PlayedPiecesCnt == TotalPiecesCnt * TotalPiecesCnt ? Role.Draw : Role.Empty;
            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                    if (currentBoard[i][j] == Role.Empty)
                        return Role.Empty;
            return Role.Draw;
        }

        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            List<Tuple<int, int>> ans = new List<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++)
                for (int j = 0; j < board[i].Count; j++)
                    if (board[i][j] == Role.Empty) ans.Add(new Tuple<int, int>(i, j));
            return ans;
        }
    }
}
