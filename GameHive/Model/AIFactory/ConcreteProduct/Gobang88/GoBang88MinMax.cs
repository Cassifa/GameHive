/*************************************************************************************
 * 文 件 名:   GoBangMinMax.cs
 * 描    述: α-β剪枝博弈树8*8五子棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:36
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIUtils.AlgorithmUtils;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class GoBang88MinMax : MinMax {
        //AC自动机执行工具类
        protected ACAutomaton ACAutomaton;
        //四个变化坐标映射的棋盘，用于优化估值速度
        private List<List<Role>> NormalBoard, XYReversedBoard;
        private List<List<Role>> MainDiagonalBoard, AntiDiagonalBoard;
        public GoBang88MinMax(Dictionary<string, int> RewardTable) {
            maxDeep = 4; TotalPiecesCnt = 8;
            ACAutomaton = new ACAutomaton(RewardTable);

            NormalBoard = new List<List<Role>>(TotalPiecesCnt);
            XYReversedBoard = new List<List<Role>>(TotalPiecesCnt);
            MainDiagonalBoard = new List<List<Role>>(TotalPiecesCnt * 2 - 1);
            AntiDiagonalBoard = new List<List<Role>>(TotalPiecesCnt * 2 - 1);
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

        //单次代价10(期望查找)*4(棋盘数量)*22(行列数量)=460次
        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            int ans = 0;
            foreach (var row in NormalBoard)
                ans += ACAutomaton.CalculateLineValue(row, role);
            //foreach (var col in XYReversedBoard)
            //    ans += ACAutomaton.CalculateLineValue(col, role);
            //foreach (var mainDiagonal in MainDiagonalBoard)
            //    ans += ACAutomaton.CalculateLineValue(mainDiagonal, role);
            //foreach (var antiDiagonal in AntiDiagonalBoard)
            //    ans += ACAutomaton.CalculateLineValue(antiDiagonal, role);
            //return ans;
            int n = TotalPiecesCnt;
            for (int col = 0; col < n; col++) {
                List<Role> column = new List<Role>();
                for (int row = 0; row < n; row++) {
                    column.Add(currentBoard[row][col]);
                }
                ans += ACAutomaton.CalculateLineValue(column, role);
            }

            // 估值主对角线
            for (int start = 0; start <= n - 5; start++) { // 确保对角线长度 >= 5
                List<Role> diag = new List<Role>();
                for (int i = 0; i + start < n; i++) {
                    diag.Add(currentBoard[i][i + start]);
                }
                ans += ACAutomaton.CalculateLineValue(diag, role);
            }
            for (int start = 1; start <= n - 5; start++) { // 主对角线另一半
                List<Role> diag = new List<Role>();
                for (int i = 0; i + start < n; i++) {
                    diag.Add(currentBoard[i + start][i]);
                }
                ans += ACAutomaton.CalculateLineValue(diag, role);
            }

            // 估值副对角线
            for (int start = 0; start <= n - 5; start++) { // 确保对角线长度 >= 5
                List<Role> diag = new List<Role>();
                for (int i = 0; i + start < n; i++) {
                    diag.Add(currentBoard[i][n - 1 - (i + start)]);
                }
                ans += ACAutomaton.CalculateLineValue(diag, role);
            }
            for (int start = 1; start <= n - 5; start++) { // 副对角线另一半
                List<Role> diag = new List<Role>();
                for (int i = 0; i + start < n; i++) {
                    diag.Add(currentBoard[i + start][n - 1 - i]);
                }
                ans += ACAutomaton.CalculateLineValue(diag, role);
            }
            if(ans>100)
                return ans;
            return ans;
        }


        // 获取所有可下棋点位-周围两格距离内有落子过
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
            //if (x == -1 || (NormalBoard[x][y] != Role.Empty && role != Role.Empty)) return;
            if (role == Role.Empty) PlayedPiecesCnt--;
            else PlayedPiecesCnt++;
            NormalBoard[x][y] = role;
            XYReversedBoard[y][x] = role;
            //主对角线从左下到右上0~2*TotalPiecesCnt-1
            MainDiagonalBoard[TotalPiecesCnt - (x - y) - 1][x - y > 0 ? y : x] = role;
            //反对角线从左上到右下0~2*TotalPiecesCnt-1
            AntiDiagonalBoard[x + y][x + y < TotalPiecesCnt ? x : TotalPiecesCnt - y - 1] = role;
        }

        // 初始化所有维度的棋盘
        protected override void InitBoards() {
            int size = TotalPiecesCnt;
            NormalBoard.Clear(); XYReversedBoard.Clear();
            MainDiagonalBoard.Clear(); MainDiagonalBoard.Clear();
            for (int i = 0; i < size; i++) {
                NormalBoard.Add(new List<Role>(new Role[size]));
                XYReversedBoard.Add(new List<Role>(new Role[size]));
            }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++) {
                    NormalBoard[i][j] = Role.Empty;
                    XYReversedBoard[j][i] = Role.Empty;
                }
            for (int i = 0; i < 2 * size - 1; i++) {
                // 计算每条对角线的长度
                int diagonalLength = (i < size) ? (i + 1) : (2 * size - 1 - i);
                MainDiagonalBoard.Add(new List<Role>(new Role[diagonalLength]));
                AntiDiagonalBoard.Add(new List<Role>(new Role[diagonalLength]));
            }

            for (int i = 0; i < 2 * size - 1; i++)
                for (int j = 0; j < (i < size ? i + 1 : 2 * size - 1 - i); j++) {
                    MainDiagonalBoard[i][j] = Role.Empty;
                    AntiDiagonalBoard[i][j] = Role.Empty;
                }
        }

        //获取当前的棋盘
        protected override List<List<Role>> GetCurrentBoard() {
            return NormalBoard;
        }

        //public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
        //    if (EvalNowSituation(GetCurrentBoard(), Role.AI) >= 1_000_000) return Role.AI;
        //    if (EvalNowSituation(GetCurrentBoard(), Role.Player) >= 1_000_000) return Role.Player;
        //    return PlayedPiecesCnt == TotalPiecesCnt * TotalPiecesCnt ? Role.Draw : Role.Empty;

        //}

    }
}
