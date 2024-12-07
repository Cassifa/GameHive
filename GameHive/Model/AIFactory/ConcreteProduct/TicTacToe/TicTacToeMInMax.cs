/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMInMax.cs
 * 描    述: α-β剪枝博弈树井字棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class TicTacToeMinMax : MinMax {
        private int PlayedPiecesCnt;
        private int TotalPiecesCnt;
        private List<List<Role>> NormalBoard;
        public TicTacToeMinMax() {
            //井字棋直接搜完
            maxDeep = 10;
            PlayedPiecesCnt = 0; TotalPiecesCnt = 3;
            NormalBoard = new List<List<Role>>(TotalPiecesCnt);
            InitBoard();
        }
        /*****实现两个博弈树策略*****/
        public override Role CheckGameOver(List<List<Role>> currentBoard) {
            int boardSize = currentBoard.Count;
            // 检查行
            for (int i = 0; i < boardSize; i++) {
                if (currentBoard[i][0] != Role.Empty &&
                    currentBoard[i][0] == currentBoard[i][1] &&
                    currentBoard[i][1] == currentBoard[i][2]) {
                    return currentBoard[i][0];
                }
            }
            // 检查列
            for (int j = 0; j < boardSize; j++) {
                if (currentBoard[0][j] != Role.Empty &&
                    currentBoard[0][j] == currentBoard[1][j] &&
                    currentBoard[1][j] == currentBoard[2][j]) {
                    return currentBoard[0][j];
                }
            }
            // 检查主对角线
            if (currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0];
            }
            // 检查副对角线
            if (currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2];
            }
            return PlayedPiecesCnt == TotalPiecesCnt * TotalPiecesCnt ? Role.Draw : Role.Empty;
        }

        // 获取所有可下棋点位
        protected override HashSet<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            var moves = new HashSet<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] == Role.Empty) {
                        moves.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return moves;
        }
        protected override HashSet<Tuple<int, int>> GetAvailableMovesByNewPieces(List<List<Role>> currentBoard, HashSet<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            return GetAvailableMoves(currentBoard);
        }


        //在x,y下棋 数组坐标
        protected override void PlayChess(int x, int y, Role role) {
            if (x == -1) return;
            if (role == Role.Empty) PlayedPiecesCnt--;
            else PlayedPiecesCnt++;
            NormalBoard[x][y] = role;
        }

        protected override List<List<Role>> GetCurrentBoard() {
            return NormalBoard;
        }

        private void InitBoard() {
            NormalBoard.Clear();
            for (int i = 0; i < TotalPiecesCnt; i++) {
                NormalBoard.Add(new List<Role>(new Role[TotalPiecesCnt]));
                for (int j = 0; j < TotalPiecesCnt; j++)
                    NormalBoard[i][j] = Role.Empty;
            }
        }

        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) {
            NormalBoard[lastX][lastY] = Role.Player;
        }
        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() {
            InitBoard();
        }
        /*****对于井字棋的搜索空间不需要*****/
        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            throw new NotImplementedException();
        }

    }
}
