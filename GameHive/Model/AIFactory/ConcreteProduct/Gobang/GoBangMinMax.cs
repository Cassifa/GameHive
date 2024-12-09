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
        private int PlayedPiecesCnt;
        private int TotalPiecesCnt;
        private List<List<Role>> NormalBoard, XYReversedBoard;
        private List<List<Role>> MainDiagonalBoard, AntiDiagonalBoard;
        public GoBangMinMax(Dictionary<List<Role>, int> RewardTable) {
            maxDeep = 4;
            PlayedPiecesCnt = 0; TotalPiecesCnt = 15;
            ACautomaton = new ACAutomaton(RewardTable);

            NormalBoard = new List<List<Role>>(TotalPiecesCnt);
            XYReversedBoard = new List<List<Role>>(TotalPiecesCnt);
            MainDiagonalBoard = new List<List<Role>>(TotalPiecesCnt * 2 - 1);
            AntiDiagonalBoard = new List<List<Role>>(TotalPiecesCnt * 2 - 1);
            InitBoards();
        }
        /*****实现七个博弈树策略*****/
        //判断游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (EvalNowSituation(GetCurrentBoard(), Role.AI) >= 1_000_000) return Role.AI;
            if (EvalNowSituation(GetCurrentBoard(), Role.Player) >= 1_000_000) return Role.Player;
            return PlayedPiecesCnt == TotalPiecesCnt * TotalPiecesCnt ? Role.Draw : Role.Empty;

        }

        //单次代价10(期望查找)*4*22(行列数量)=880次
        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            int ans = 0;
            // 估值行
            foreach (var row in NormalBoard)
                ans += ACautomaton.CalculateLineValue(row, role);
            foreach (var col in XYReversedBoard)
                ans += ACautomaton.CalculateLineValue(col, role);
            foreach (var mainDiagonal in MainDiagonalBoard)
                ans += ACautomaton.CalculateLineValue(mainDiagonal, role);
            foreach (var antiDiagonal in AntiDiagonalBoard)
                ans += ACautomaton.CalculateLineValue(antiDiagonal, role);
            return ans;
        }

        // 获取所有可下棋点位
        protected override HashSet<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            // 最终结果
            var usefulSteps = new HashSet<Tuple<int, int>>();
            int[] dx = { -2, -1, 0, 1, 2 };
            int[] dy = { -2, -1, 0, 1, 2 };

            // 遍历棋盘，寻找所有落子点附近的可用位置
            for (int i = 0; i < board.Count; i++)
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] == Role.Empty) continue;
                    // 在落子点周围扩展
                    for (int a = 0; i < 5; i++) {
                        for (int b = 0; j < 5; j++) {
                            int newX = i + dx[a];
                            int newY = j + dy[b];
                            if (newX < 0 || newY < 0 || newX >= board.Count || newY >= board[0].Count || (i == newX && j == newY)) continue;
                            if (board[newX][newY] == Role.Empty) {
                                usefulSteps.Add(new Tuple<int, int>(newX, newY));
                            }
                        }
                    }
                }
            // 如果没有可行 随机生成中间九格之一
            if (usefulSteps.Count == 0) {
                var random = new Random();
                int[] centerDx = { -1, 0, 1, -1, 0, 1, -1, 0, 1 };
                int[] centerDy = { -1, 0, 1, 0, 0, 0, 1, 1, 1 };
                int t = random.Next(100);
                if (t < 80) {
                    usefulSteps.Add(new Tuple<int, int>(board.Count / 2, board[0].Count / 2));
                } else {
                    int index = random.Next(centerDx.Length);
                    usefulSteps.Add(new Tuple<int, int>(board.Count / 2 + centerDx[index], board[0].Count / 2 + centerDy[index]));
                }
            }
            return usefulSteps;
        }

        //使用历史可用与最新落子获取最新可用
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
                    if (newX < 0 || newY < 0 || newX >= currentBoard.Count || newY >= currentBoard[0].Count || (lastX == newX && lastY == newY)) continue;
                    if (currentBoard[newX][newY] == Role.Empty) {
                        newSet.Add(new Tuple<int, int>(newX, newY));
                    }
                }
            }
            return newSet;
        }


        //在x,y下棋 数组坐标
        protected override void PlayChess(int x, int y, Role role) {
            if (x == -1 || (NormalBoard[x][y] != Role.Empty && role != Role.Empty)) return;
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
        private void InitBoards() {
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

        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) {
            PlayChess(lastX, lastY, Role.Player);
        }
        //开始游戏

        public override void GameStart(bool IsAIFirst) {
            PlayedPiecesCnt = 0;
            InitBoards();
        }
    }
}
