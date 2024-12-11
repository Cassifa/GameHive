/*************************************************************************************
* 文 件 名:   GoBangMinMax.cs
* 描    述: α-β剪枝博弈树8*8五子棋产品实例
*           实现方法：
*              影响算法效率：
*                  1.通过新落子判断游戏是否结束 复杂度：常数
*                  2.计算棋盘估值 复杂度：4*TotalPiecesCnt*单串匹配代价(命中字典O(length),未命中O(length))
*                  3.根据当前最新落子获取可落子列表 复杂度： 上次列表长度
*                  4.下棋,使用四个坐标轴映射后棋盘优化 复杂度：常数
*              不影响算法效率：
*                  1.根据棋盘获取可落子列表 2.初始化棋盘 3.获取当前棋盘
*           定义方法：
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
        public GoBang88MinMax(Dictionary<List<Role>, int> RewardTable) {
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
            foreach (var col in XYReversedBoard)
                ans += ACAutomaton.CalculateLineValue(col, role);
            foreach (var mainDiagonal in MainDiagonalBoard)
                ans += ACAutomaton.CalculateLineValue(mainDiagonal, role);
            foreach (var antiDiagonal in AntiDiagonalBoard)
                ans += ACAutomaton.CalculateLineValue(antiDiagonal, role);
            return ans;
        }


        // 获取所有可下棋点位-周围两格距离内有落子过
        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            var usefulSteps = new List<Tuple<int, int>>();
            int[,] priorityMap = new int[board.Count, board[0].Count];
            // 初始化优先级表
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] != Role.Empty) {
                        priorityMap[i, j] = int.MinValue;
                    }
                }
            }
            // 添加权重
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1,  // 内圈
             -2, -2, -2, -2, -1, -1, 0, 0, 1, 1, 2, 2, 2, 2, -1, 1 }; // 外圈
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1,
             -2, -1, 0, 1, -2, 2, -2, 2, -2, 2, -2, -1, 0, 1, -2, -2 };
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] == Role.Empty) continue;
                    for (int k = 0; k < 24; k++) {
                        int x = i + dx[k], y = j + dy[k];
                        if (x < 0 || y < 0 || x >= board.Count || y >= board[0].Count) continue;
                        int power = (k < 8) ? 2 : 1; // 内圈权重更高
                        priorityMap[x, y] += power;
                    }
                }
            }
            // 收集所有有效点位并排序
            var temp = new List<Tuple<int, Tuple<int, int>>>();
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (priorityMap[i, j] > 0) {
                        temp.Add(new Tuple<int, Tuple<int, int>>(priorityMap[i, j], new Tuple<int, int>(i, j)));
                    }
                }
            }
            temp.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            foreach (var t in temp) {
                usefulSteps.Add(t.Item2);
            }
            //成功获取可行点
            if (usefulSteps.Count > 0) return usefulSteps;

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

        //使用历史可用与最新落子获取最新可用
        //从历史可落子点位中移除本次落子点(若存在)
        //添加新可用点至表头，老可用点引用传入，返回深拷贝列表(老可用点为浅拷贝)
        protected override List<Tuple<int, int>> GetAvailableMovesByNewPieces(
            List<List<Role>> currentBoard, List<Tuple<int, int>> lastAvailableMoves,
            int lastX, int lastY) {
            List<Tuple<int, int>> newAvailableMoves = new List<Tuple<int, int>>();
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1, -2, -2, -2, -2, -1, -1, 0, 0, 1, 1, 2, 2, 2, 2, -1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1, -2, -1, 0, 1, -2, 2, -2, 2, -2, 2, -2, -1, 0, 1, -2, -2 };
            for (int i = 0; i < 24; i++) {
                int newX = lastX + dx[i];
                int newY = lastY + dy[i];
                if (newX < 0 || newY < 0 || newX >= TotalPiecesCnt || newY >= TotalPiecesCnt ||
                    (lastX == newX && lastY == newY)) continue;
                if (currentBoard[newX][newY] == Role.Empty) {
                    newAvailableMoves.Add(new Tuple<int, int>(newX, newY));
                }
            }
            //上次可用点加入新表，上次可用点为浅拷贝
            foreach (var move in lastAvailableMoves)
                if (move.Item1 != lastX || move.Item2 != lastY)
                    newAvailableMoves.Add(move);
            return newAvailableMoves;
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
        protected override void InitGame() {
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

    }
}
