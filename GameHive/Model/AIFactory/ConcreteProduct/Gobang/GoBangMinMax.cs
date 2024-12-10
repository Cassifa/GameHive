/*************************************************************************************
 * 文 件 名:   GoBangMinMax.cs
 * 描    述: α-β剪枝博弈树五子棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:36
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIUtils.AlgorithmUtils;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class GoBangMinMax : MinMax {
        //AC自动机执行工具类
        protected ACAutomaton ACAutomaton;
        //四个变化坐标映射的棋盘，用于优化估值速度
        private List<List<Role>> Board;
        private List<string> NormalBoard, XYReversedBoard;
        private List<string> MainDiagonalBoard, AntiDiagonalBoard;
        public GoBangMinMax(Dictionary<string, int> RewardTable) {
            maxDeep = 4; TotalPiecesCnt = 15;
            ACAutomaton = new ACAutomaton(RewardTable);

            Board = new List<List<Role>>(TotalPiecesCnt);
            NormalBoard = new List<string>(TotalPiecesCnt);
            XYReversedBoard = new List<string>(TotalPiecesCnt);
            MainDiagonalBoard = new List<string>(TotalPiecesCnt * 2 - 1);
            AntiDiagonalBoard = new List<string>(TotalPiecesCnt * 2 - 1);
        }
        /*****实现七个博弈树策略*****/
        //判断游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (x == -1) return Role.Empty;
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
                    if (nx < 0 || ny < 0 || nx >= TotalPiecesCnt || ny >= TotalPiecesCnt || currentBoard[nx][ny] != currentPlayer)
                        break;
                    count++;
                }
                for (int step = 1; step <= 4; step++) {
                    int nx = x - dx[direction] * step;
                    int ny = y - dy[direction] * step;
                    if (nx < 0 || ny < 0 || nx >= TotalPiecesCnt || ny >= TotalPiecesCnt || currentBoard[nx][ny] != currentPlayer)
                        break;
                    count++;
                }
                if (count >= 5) return currentPlayer;
            }
            if (TotalPiecesCnt * TotalPiecesCnt == PlayedPiecesCnt) return Role.Draw;
            return Role.Empty;
        }

        //public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
        //    if (EvalNowSituation(GetCurrentBoard(), Role.AI) >= 1_000_000) return Role.AI;
        //    if (EvalNowSituation(GetCurrentBoard(), Role.Player) >= 1_000_000) return Role.Player;
        //    return PlayedPiecesCnt == TotalPiecesCnt * TotalPiecesCnt ? Role.Draw : Role.Empty;

        //}

        //单次代价10(期望查找)*4*22(行列数量)=880次
        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            int ans = 0;
            foreach (var row in NormalBoard)
                ans += ACAutomaton.CalculateLineValue(row, role);
            foreach (var col in XYReversedBoard)
                ans += ACAutomaton.CalculateLineValue(col, role);
            foreach (var mainDiagonal in MainDiagonalBoard)
                ans += ACAutomaton.CalculateLineValue(mainDiagonal, role);
            foreach (var antiDiagonal in AntiDiagonalBoard)
                ans += ACAutomaton.CalculateLineValue(antiDiagonal, role);
            return ans;
        }

        // 获取所有可下棋点位
        protected override HashSet<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            var usefulSteps = new HashSet<Tuple<int, int>>();
            int[] dx = { -2, -1, 0, 1, 2 };
            int[] dy = { -2, -1, 0, 1, 2 };
            //寻找所有落子点附近的可用位置
            for (int i = 0; i < TotalPiecesCnt; i++)
                for (int j = 0; j < TotalPiecesCnt; j++) {
                    if (board[i][j] == Role.Empty) continue;
                    // 在落子点周围扩展
                    for (int a = 0; a < 5; a++) {
                        for (int b = 0; b < 5; b++) {
                            int newX = i + dx[a];
                            int newY = j + dy[b];
                            if (newX < 0 || newY < 0 || newX >= TotalPiecesCnt || newY >= TotalPiecesCnt || (i == newX && j == newY)) continue;
                            if (board[newX][newY] == Role.Empty) {
                                usefulSteps.Add(new Tuple<int, int>(newX, newY));
                            }
                        }
                    }
                }
            if (PlayedPiecesCnt == 0)
                usefulSteps.Add(new Tuple<int, int>(board.Count / 2, board[0].Count / 2));
            else {
                for (int i = 0; i < board.Count; i++)
                    for (int j = 0; j < board[i].Count; j++)
                        if (board[i][j] == Role.Empty)
                            usefulSteps.Add(new Tuple<int, int>(i, j));
            }
            return usefulSteps;
        }

        //使用历史可用与最新落子获取最新可用-从历史可落子点位中移除已不可用的点，添加新可用点，返回深拷贝
        protected override HashSet<Tuple<int, int>> GetAvailableMovesByNewPieces(
            List<List<Role>> currentBoard, HashSet<Tuple<int, int>> lastAvailableMoves,
            int lastX, int lastY) {
            HashSet<Tuple<int, int>> newSet = new HashSet<Tuple<int, int>>(lastAvailableMoves);
            //移除
            var lastMove = new Tuple<int, int>(lastX, lastY);
            newSet.Remove(lastMove);
            int[] dx = { -2, -1, 0, 1, 2 };
            int[] dy = { -2, -1, 0, 1, 2 };
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    int newX = lastX + dx[i];
                    int newY = lastY + dy[j];
                    if (newX < 0 || newY < 0 || newX >= TotalPiecesCnt || newY >= TotalPiecesCnt || (lastX == newX && lastY == newY)) continue;
                    if (currentBoard[newX][newY] == Role.Empty) {
                        newSet.Add(new Tuple<int, int>(newX, newY));
                    }
                }
            }
            return newSet;
        }


        //在x,y下棋 数组坐标
        protected override void PlayChess(int x, int y, Role role) {
            if (role == Role.Empty) PlayedPiecesCnt--;
            else PlayedPiecesCnt++;
            Board[x][y] = role;
            char charRole = role == Role.Empty ? '_' : (role == Role.AI ? 'A' : 'P');

            var normalRowChars = NormalBoard[x].ToCharArray();
            normalRowChars[y] = charRole;
            NormalBoard[x] = new string(normalRowChars);
            var xyRowChars = XYReversedBoard[y].ToCharArray();
            xyRowChars[x] = charRole;
            XYReversedBoard[y] = new string(xyRowChars);

            //主对角线从左下到右上0~2*TotalPiecesCnt-1
            int mainDiagonalIndex = TotalPiecesCnt - (x - y) - 1;
            var mainDiagonalRowChars = MainDiagonalBoard[mainDiagonalIndex].ToCharArray();
            mainDiagonalRowChars[x - y > 0 ? y : x] = charRole;
            MainDiagonalBoard[mainDiagonalIndex] = new string(mainDiagonalRowChars);
            //反对角线从左上到右下0~2*TotalPiecesCnt-1
            var antiDiagonalRowChars = AntiDiagonalBoard[x + y].ToCharArray();
            antiDiagonalRowChars[x + y < TotalPiecesCnt ? x : TotalPiecesCnt - y - 1] = charRole;
            AntiDiagonalBoard[x + y] = new string(antiDiagonalRowChars);
        }

        //初始化所有维度的棋盘
        protected override void InitBoards() {
            int size = TotalPiecesCnt;
            Board.Clear();
            NormalBoard.Clear(); XYReversedBoard.Clear();
            MainDiagonalBoard.Clear(); MainDiagonalBoard.Clear();
            for (int i = 0; i < size; i++) 
                Board.Add(new List<Role>(new Role[size]));
            NormalBoard = new List<string>(size);
            XYReversedBoard = new List<string>(size);
            MainDiagonalBoard = new List<string>(2 * size - 1);
            AntiDiagonalBoard = new List<string>(2 * size - 1);

            // 初始化 NormalBoard 和 XYReversedBoard
            for (int i = 0; i < size; i++) {
                NormalBoard.Add(new string('E', size)); // 'E' 表示 Role.Empty
                XYReversedBoard.Add(new string('E', size));
            }

            // 初始化 MainDiagonalBoard 和 AntiDiagonalBoard
            for (int i = 0; i < 2 * size - 1; i++) {
                int diagonalLength = (i < size) ? (i + 1) : (2 * size - 1 - i);
                MainDiagonalBoard.Add(new string('E', diagonalLength));
                AntiDiagonalBoard.Add(new string('E', diagonalLength));
            }
        }

        //获取当前的棋盘
        protected override List<List<Role>> GetCurrentBoard() {
            return Board;
        }
    }
}
