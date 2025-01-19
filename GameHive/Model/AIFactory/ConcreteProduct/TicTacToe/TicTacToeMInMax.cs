/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMInMax.cs
 * 描    述: α-β剪枝博弈树井字棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIUtils.AlgorithmUtils;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class TicTacToeMinMax : MinMax {
        private List<List<Role>> NormalBoard;
        public TicTacToeMinMax() {
            //井字棋直接搜完
            maxDeep = 12; TotalPiecesCnt = 3;
            //初始缓存表
            MinMaxCache = new ZobristHashingCache<int>(TotalPiecesCnt, TotalPiecesCnt);
            NormalBoard = new List<List<Role>>(TotalPiecesCnt);
        }
        /*****实现两个博弈树策略*****/
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (x == -1) return Role.Empty;
            if (currentBoard[x][0] != Role.Empty &&
                currentBoard[x][0] == currentBoard[x][1] &&
                currentBoard[x][1] == currentBoard[x][2]) {
                return currentBoard[x][0];
            }
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
            if (x + y == TotalPiecesCnt - 1 && currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2];
            }
            for (int i = 0; i < TotalPiecesCnt; i++)
                for (int j = 0; j < TotalPiecesCnt; j++)
                    if (currentBoard[i][j] == Role.Empty)
                        return Role.Empty;
            return Role.Draw;
        }

        // 获取所有可下棋点位
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
        
        protected override List<Tuple<int, int>> GetAvailableMovesByNewPieces(List<List<Role>> currentBoard, List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            return GetAvailableMoves(currentBoard);
        }


        //在博弈树内部维护棋盘x,y下棋 数组坐标
        protected override void PlayChess(int x, int y, Role role) {
            //更新缓存表
            if (role == Role.Empty) {
                MinMaxCache.UpdateCurrentBoardHash(x, y, NormalBoard[x][y]);
                PlayedPiecesCnt--;
            } else {
                MinMaxCache.UpdateCurrentBoardHash(x, y, role);
                PlayedPiecesCnt++;
            }
            NormalBoard[x][y] = role;
        }

        protected override List<List<Role>> GetCurrentBoard() {
            return NormalBoard;
        }

        protected override void InitGame() {
            NormalBoard.Clear();
            for (int i = 0; i < TotalPiecesCnt; i++) {
                NormalBoard.Add(new List<Role>(new Role[TotalPiecesCnt]));
                for (int j = 0; j < TotalPiecesCnt; j++)
                    NormalBoard[i][j] = Role.Empty;
            }
        }

        /*****对于井字棋的搜索空间不需要*****/
        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            throw new NotImplementedException();
        }

    }
}
