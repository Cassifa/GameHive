/*************************************************************************************
* 文 件 名:   GoBangMinMax.cs
* 描    述: α-β剪枝博弈树8*8五子棋产品实例
*           实现方法：
*              影响算法效率：
*                  1.通过新落子判断游戏是否结束 复杂度：常数
*                  2.计算棋盘估值 复杂度：4*TotalPiecesCnt^2,横纵正反对角线估值，用AC自动机实现单串匹配O(n)复杂度
*                  3.根据当前最新落子获取可落子列表 复杂度： 上次列表长度
*                  4.下棋,使用四个坐标轴映射后棋盘优化 复杂度：常数
*              不影响算法效率：
*                  1.根据棋盘获取可落子列表 2.初始化棋盘 3.获取当前棋盘
*           定义方法：
* 版    本：  V2.0 .NET客户端初版
* 创 建 者：  Cassifa
* 创建时间：  2024/11/26 18:36
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIUtils.AlgorithmUtils;
using GameHive.Model.AIUtils.AlphaBetaPruning;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class GoBang88MinMax : MinMax {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(4);

        /**********成员申明与初始化**********/
        //AC自动机执行工具类
        protected ACAutomaton ACAutomaton;
        protected ACAutomaton PlayerACAutomaton;
        protected KillingBoardACAutomaton AIKillingBoardACAutomaton;
        protected KillingBoardACAutomaton PlayerAIKillingBoardACAutomaton;
        //缓存
        protected ZobristHashingCache<int> BoardValueCache;
        protected ZobristHashingCache<Tuple<int, int>> KillingCache;
        //四个变化坐标映射的棋盘，用于优化估值速度
        private List<List<Role>> NormalBoard, XYReversedBoard;
        private List<List<Role>> MainDiagonalBoard, AntiDiagonalBoard;
        public GoBang88MinMax(int Column, DifficultyLevel level, Dictionary<List<Role>, int> RewardTable, Dictionary<List<Role>, KillTypeEnum> killingTable) {
            TotalPiecesCnt = Column;
            //初始缓存表
            MinMaxCache = new ZobristHashingCache<int>(TotalPiecesCnt, TotalPiecesCnt);
            BoardValueCache = new ZobristHashingCache<int>(TotalPiecesCnt, TotalPiecesCnt);
            KillingCache = new ZobristHashingCache<Tuple<int, int>>(TotalPiecesCnt, TotalPiecesCnt);
            //初始化自动机工具类
            ACAutomaton = new ACAutomaton(RewardTable);
            PlayerACAutomaton = new ACAutomaton(RewardTableUtil.SwitchAIPlayerRewardTable(RewardTable));
            AIKillingBoardACAutomaton = new KillingBoardACAutomaton(killingTable);
            PlayerAIKillingBoardACAutomaton = new KillingBoardACAutomaton(RewardTableUtil.SwitchAIPlayer(killingTable));
            //初始化棋盘对象
            NormalBoard = new List<List<Role>>(TotalPiecesCnt);
            XYReversedBoard = new List<List<Role>>(TotalPiecesCnt);
            MainDiagonalBoard = new List<List<Role>>(TotalPiecesCnt * 2 - 1);
            AntiDiagonalBoard = new List<List<Role>>(TotalPiecesCnt * 2 - 1);

            switch (level) {
                case DifficultyLevel.LEVEL_1:
                    //初始化搜索深度
                    maxDeep = 4;
                    RunKillBoard = false;
                    DeepeningKillingActivated = true;
                    break;
                case DifficultyLevel.LEVEL_2:
                    //初始化搜索深度
                    maxDeep = 6;
                    killingMaxDeep = 10;
                    RunKillBoard = true;
                    DeepeningKillingActivated = true;
                    IsVCT = false;
                    break;
                case DifficultyLevel.LEVEL_3:
                    //初始化搜索深度
                    maxDeep = 8;
                    killingMaxDeep = 12;
                    RunKillBoard = true;
                    DeepeningKillingActivated = true;
                    IsVCT = false;
                    break;
                case DifficultyLevel.LEVEL_4:
                    //初始化搜索深度
                    maxDeep = 8;
                    killingMaxDeep = 12;
                    RunKillBoard = true;
                    DeepeningKillingActivated = true;
                    IsVCT = true;
                    break;
            }
        }

        /**********1.启发函数**********/

        //获取AI在此点落子的局面估值增益,不考虑人类在这里会怎样
        private int GetAIPointDeltaScore(int x, int y) {
            int score = -1 * EvalNowSituation(NormalBoard, Role.AI);
            var t = GetMainAndAntiDiagonalCoordinates(x, y);
            PlayChess(x, y, Role.AI);
            score += EvalNowSituation(NormalBoard, Role.AI);
            PlayChess(x, y, Role.Empty);
            return score;
        }
        //获取人类在此点落子的局面估值增益，考虑自身收益同时考虑考虑此字对AI增益,阻挠AI增益视为自身增益
        private int GetPlayerPointDeltaScore(int x, int y) {
            int score = 0;
            PlayChess(x, y, Role.Player);
            var t = GetMainAndAntiDiagonalCoordinates(x, y);
            score += PlayerACAutomaton.CalculateLineValue(NormalBoard[x]);
            score += PlayerACAutomaton.CalculateLineValue(XYReversedBoard[y]);
            score += PlayerACAutomaton.CalculateLineValue(MainDiagonalBoard[t.Item1]);
            score += PlayerACAutomaton.CalculateLineValue(AntiDiagonalBoard[t.Item3]);
            PlayChess(x, y, Role.Empty);
            score -= PlayerACAutomaton.CalculateLineValue(NormalBoard[x]);
            score -= PlayerACAutomaton.CalculateLineValue(XYReversedBoard[y]);
            score -= PlayerACAutomaton.CalculateLineValue(MainDiagonalBoard[t.Item1]);
            score -= PlayerACAutomaton.CalculateLineValue(AntiDiagonalBoard[t.Item3]);
            return score + GetAIPointDeltaScore(x, y);
        }

        //关键函数✨将底数限制为8,不超过12 对可行点进行排序：在AI-Max层有限搜估值高或危险的点位，Player-Min层搜人类增益最高的点
        //AI要防人杀棋同时贪心的搜当前价值最高点，人要贪心的走价值增益最大的点 由于运行MinMax前已经计算过AI杀棋，所以不考虑AI杀情况
        //如果是AI-将落子点位对AI增益从大到小，并加入人类VCT杀棋点,预防人类连续冲三攻击。若存在有价值点则不考虑无价值点
        //如果是人类 落子点增益最大的10个点(考虑了AI的增益)
        protected override void HeuristicSort(ref List<Tuple<int, int>> moves, int lastX, int lastY) {
            bool isAI = (lastX == -1 || NormalBoard[lastX][lastY] == Role.AI) ? false : true;
            if (isAI) { //将可行点位按增益从大到小排序，前置人类VCT杀棋点
                //暂存队列，item1为点位，item2为对于AI估值增益和玩家杀棋价值
                var scoredMoves = new List<Tuple<Tuple<int, int>, Tuple<int, KillingBoard>>>();
                foreach (var move in moves)
                    scoredMoves.Add(Tuple.Create(move, Tuple.Create(GetAIPointDeltaScore(move.Item1, move.Item2),
                        EvaluateKill(move, Role.Player))));
                moves.Clear();
                scoredMoves.Sort((a, b) => {
                    int scoreA = a.Item2.Item1;
                    int scoreB = b.Item2.Item1;
                    // 如果杀棋价值不同，优先考虑杀棋->人类有潜力VCT的全部前置,防止被VCT
                    int killA = a.Item2.Item2.score;
                    int killB = b.Item2.Item2.score;
                    if (killA != killB)
                        return killB.CompareTo(killA);
                    return scoreB.CompareTo(scoreA);
                });
                //如果存在有分数的且人类无法形成VCT那么不考虑没分数的，若有分数点已经8个或者VCT＋有分数已经15个，拒绝价值更低点位
                int hasScore = 0;
                for (int i = 0; i < scoredMoves.Count; i++) {
                    var t = scoredMoves[i];
                    if (moves.Count != 0 && t.Item2.Item1 <= 0 && t.Item2.Item2.score <= 0)
                        break;
                    if (t.Item2.Item1 != 0)
                        hasScore++;
                    moves.Add(t.Item1);
                    if (hasScore > 8 || i > 12)
                        break;
                }
                if (moves.Count == 0)
                    return;
            } else {
                //人类在下棋，优先搜可下棋后人类增益最大的点（考虑了阻碍AI增益）
                var scoredMoves = new List<Tuple<Tuple<int, int>, int>>();
                foreach (var move in moves)
                    scoredMoves.Add(Tuple.Create(move, GetPlayerPointDeltaScore(move.Item1, move.Item2)));
                moves.Clear();
                //将杀棋点前置并将可行点位按AI增益估值从小到大排序，不考虑AI估值大于KillTypeEnum.Low
                scoredMoves.Sort((a, b) => {
                    //杀棋值相同，按估值从小到大排序
                    return b.Item2.CompareTo(a.Item2);
                });
                //如果已经有大量有增益的，不考虑毫无增益的
                //如果大于10个有增益的直接终止
                foreach (var t in scoredMoves) {
                    if ((moves.Count > 5 && t.Item2 == 0) || moves.Count >= 8)
                        break;
                    moves.Add(t.Item1);
                }
                if (moves.Count == 0)
                    return;
            }
        }


        /**********2.基础功能**********/

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
                if (count >= 5)
                    return currentPlayer;
            }
            if (TotalPiecesCnt * TotalPiecesCnt == PlayedPiecesCnt)
                return Role.Draw;
            return Role.Empty;
        }

        //局面估值  单次代价10(期望查找)*4(棋盘数量)*22(行列数量)=460次
        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            int ans = 0;
            //尝试从缓存中查值
            if (BoardValueCache.GetValue(ref ans) != -1)
                return ans;
            foreach (var row in NormalBoard)
                ans += ACAutomaton.CalculateLineValue(row);
            foreach (var col in XYReversedBoard)
                ans += ACAutomaton.CalculateLineValue(col);
            foreach (var mainDiagonal in MainDiagonalBoard) {
                if (mainDiagonal.Count < 5)
                    continue;
                ans += ACAutomaton.CalculateLineValue(mainDiagonal);
            }
            foreach (var antiDiagonal in AntiDiagonalBoard) {
                if (antiDiagonal.Count < 5)
                    continue;
                ans += ACAutomaton.CalculateLineValue(antiDiagonal);
            }
            //记录缓存值
            BoardValueCache.Log(ans);
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
                    if (board[i][j] == Role.Empty)
                        continue;
                    for (int k = 0; k < 24; k++) {
                        int x = i + dx[k], y = j + dy[k];
                        if (x < 0 || y < 0 || x >= board.Count || y >= board[0].Count)
                            continue;
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
            if (usefulSteps.Count > 0)
                return usefulSteps;
            //如果是AI先手之间下中心
            if (PlayedPiecesCnt == 0)
                usefulSteps.Add(new Tuple<int, int>(board.Count / 2, board[0].Count / 2));
            //否则返回所有空点
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
                if (newX < 0 || newY < 0 || newX >= TotalPiecesCnt || newY >= TotalPiecesCnt)
                    continue;
                if (currentBoard[newX][newY] == Role.Empty) {
                    newAvailableMoves.Add(new Tuple<int, int>(newX, newY));
                }
            }
            int newSize = newAvailableMoves.Count;
            //上次可用点加入新表，上次可用点为浅拷贝,不存重复点
            foreach (var move in lastAvailableMoves) {
                if (move.Item1 == lastX && move.Item2 == lastY)
                    continue;
                bool flag = true;
                //搜当前点是否在新加入点中出现过
                for (int i = 0; i < newSize; i++) {
                    var t = newAvailableMoves[i];
                    if (move.Item1 == t.Item1 && move.Item2 == t.Item2) {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    newAvailableMoves.Add(move);
            }
            return newAvailableMoves;
        }

        //在数组坐标(x,y)下棋 
        protected override void PlayChess(int x, int y, Role role) {
            //更新缓存表
            if (role == Role.Empty) {
                BoardValueCache.UpdateCurrentBoardHash(x, y, NormalBoard[x][y]);
                MinMaxCache.UpdateCurrentBoardHash(x, y, NormalBoard[x][y]);
                KillingCache.UpdateCurrentBoardHash(x, y, NormalBoard[x][y]);
                PlayedPiecesCnt--;
            } else {
                BoardValueCache.UpdateCurrentBoardHash(x, y, role);
                MinMaxCache.UpdateCurrentBoardHash(x, y, role);
                KillingCache.UpdateCurrentBoardHash(x, y, role);
                PlayedPiecesCnt++;
            }
            NormalBoard[x][y] = role;
            XYReversedBoard[y][x] = role;
            var t = GetMainAndAntiDiagonalCoordinates(x, y);
            //主对角线从左下到右上0~2*TotalPiecesCnt-1
            MainDiagonalBoard[t.Item1][t.Item2] = role;
            //反对角线从左上到右下0~2*TotalPiecesCnt-1
            AntiDiagonalBoard[t.Item3][t.Item4] = role;
        }


        /**********3.工具函数**********/

        //获取主、反对角线坐标映射
        private Tuple<int, int, int, int> GetMainAndAntiDiagonalCoordinates(int i, int j) {
            return new Tuple<int, int, int, int>
                (TotalPiecesCnt - (i - j) - 1, i - j > 0 ? j : i,
                i + j, i + j < TotalPiecesCnt ? i : TotalPiecesCnt - j - 1);
        }

        // 初始化所有维度的棋盘
        protected override void InitGame() {
            int size = TotalPiecesCnt;
            NormalBoard.Clear();
            XYReversedBoard.Clear();
            MainDiagonalBoard.Clear();
            MainDiagonalBoard.Clear();
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

        //开始游戏，刷新缓存棋盘状态
        public override void GameStart(bool IsAIFirst) {
            base.GameStart(IsAIFirst);
            BoardValueCache.RefreshLog();
            KillingCache.RefreshLog();
        }


        /**********杀棋函数**********/

        //计算VCT杀棋
        protected override Tuple<int, int> VCT(Role type, int leftDepth) {
            Tuple<int, int> best = Tuple.Create(-1, -1);
            //查缓存
            if (KillingCache.GetValue(ref best) >= leftDepth)
                if (best.Item1 != -1)
                    return best;
                else
                    return best;

            // 算杀失败
            if (leftDepth == 0 || GameOver) {
                KillingCache.Log(best, leftDepth);
                return best;
            }
            bool isAI = type == Role.AI;

            List<Tuple<int, int, int>> pointList = GetVctPoints(type);
            foreach (var point in pointList) {
                //已经形成必胜棋型
                if (point.Item3 >= (int)KillingRiskEnum.High) {
                    //AI-返回当前节点 玩家-直接返回空
                    best = isAI ? new Tuple<int, int>(point.Item1, point.Item2) : Tuple.Create(-1, -1);
                    break;
                }
                //if (leftDepth == 8 && point.Item1 == 4 && point.Item2 == 10)
                //leftDepth = leftDepth;
                PlayChess(point.Item1, point.Item2, type);
                best = VCT(type == Role.AI ? Role.Player : Role.AI, leftDepth - 1);
                PlayChess(point.Item1, point.Item2, Role.Empty);

                if (best.Item1 == -1) {
                    //AI还没找到可以算杀成功的棋子，继续找
                    if (isAI)
                        continue;
                    //对手拦截成功，返回空表示算杀失败
                    break; //return null
                }
                //找到了 best,记录当前节点导致 best的节点
                best = new Tuple<int, int>(point.Item1, point.Item2);
                //已经找到杀棋点位
                if (isAI)
                    break;

            }
            //更新缓存
            KillingCache.Log(best, leftDepth);
            return best;
        }

        //获取可杀气列表 第三项为此点的杀棋估值得分
        private List<Tuple<int, int, int>> GetVctPoints(Role type) {
            bool isAI = type == Role.AI;
            // 进攻点列表
            List<Tuple<int, int, int>> attackPointList = new List<Tuple<int, int, int>>();
            // 防守点列表
            List<Tuple<int, int, int>> defensePointList = new List<Tuple<int, int, int>>();
            // VCT列表
            List<Tuple<int, int, int>> vctPointList = new List<Tuple<int, int, int>>();
            // 局势是否危险
            bool isDanger = false;

            List<Tuple<int, int>> availableMoves = GetAvailableMoves(NormalBoard);

            foreach (var move in availableMoves) {
                int i = move.Item1, j = move.Item2;

                // 考虑自己的落子情况
                KillingBoard killingBoard = EvaluateKill(new Tuple<int, int>(i, j), type);
                if (killingBoard.typeRecord[KillTypeEnum.Five] > 0)
                    // 自己可以连五，直接返回
                    return new List<Tuple<int, int, int>> { new Tuple<int, int, int>(i, j, killingBoard.score) };

                //存在危险只找可以连五的棋子，尝试连续冲四防御
                if (isDanger)
                    continue;

                //考虑对手的落子情况
                KillingBoard foeKillingBoard = EvaluateKill(new Tuple<int, int>(i, j), type == Role.AI ? Role.Player : Role.AI);

                if (foeKillingBoard.typeRecord[KillTypeEnum.Five] > 0) {
                    //地方落子直接导致连五，必须防御
                    isDanger = true;
                    defensePointList.Clear();
                    defensePointList.Add(new Tuple<int, int, int>(i, j, killingBoard.score));
                    continue;
                }

                //看看自己有没有大于等于中风险分值的点位
                if (killingBoard.score >= (int)KillingRiskEnum.Middle) {
                    attackPointList.Add(new Tuple<int, int, int>(i, j, killingBoard.score));
                    continue;
                }

                if (isAI) {
                    //AI进攻VCT-找冲四、活三
                    if (killingBoard.typeRecord[KillTypeEnum.FourBlocked] > 0 ||
                        killingBoard.typeRecord[KillTypeEnum.ThreeAlive] > 0)
                        vctPointList.Add(new Tuple<int, int, int>(i, j, killingBoard.score));
                } else {
                    //人类防守VCT-拦截连五、活四、找冲四反击
                    //选择冲四或防守对方的活四
                    if (killingBoard.typeRecord[KillTypeEnum.FourBlocked] > 0 ||
                        foeKillingBoard.typeRecord[KillTypeEnum.FourAlive] > 0)
                        defensePointList.Add(new Tuple<int, int, int>(i, j, killingBoard.score));
                }
            }


            List<Tuple<int, int, int>> pointList = new List<Tuple<int, int, int>>();
            //没风险就进攻
            if (!isDanger) {
                //优先强进攻
                if (attackPointList.Count > 0) {
                    //按分数从大到小排序
                    attackPointList.Sort((p1, p2) => p2.Item3.CompareTo(p1.Item3));
                    //AI有强进攻点位,不考虑后面的点位
                    if (isAI)
                        return attackPointList;
                    //对手可以选择进攻和防守
                    pointList.AddRange(attackPointList);
                }
                //VCT进攻
                pointList.AddRange(vctPointList);
            }
            if (defensePointList.Count > 0) {
                //进行防守
                //AI优先选择进攻，把防守点位放后面
                if (isAI)
                    pointList.AddRange(defensePointList);
                //对手优先考虑防守，把防守点位放前面
                else
                    pointList.InsertRange(0, defensePointList);
            }
            return pointList;
        }

        //对一个点落子后形成杀棋局面进行评估
        private void UpdateKillingBoard(KillingBoard killingBoard, Role role, List<List<Role>> board, int rowId,
            ref int threeAliveCount, ref int fourBlockedCount, ref int ThreeAliveWithFourBlockedCount) {
            int three = killingBoard.typeRecord[KillTypeEnum.ThreeAlive];
            int four = killingBoard.typeRecord[KillTypeEnum.FourBlocked];
            if (role == Role.AI)
                AIKillingBoardACAutomaton.CalculateLineValue(board[rowId], killingBoard);
            else
                PlayerAIKillingBoardACAutomaton.CalculateLineValue(board[rowId], killingBoard);
            //有活三更新活三值
            if (killingBoard.typeRecord[KillTypeEnum.ThreeAlive] > three) {
                threeAliveCount++;
                //同时活三冲四更新活三
                if (killingBoard.typeRecord[KillTypeEnum.FourBlocked] > four)
                    ThreeAliveWithFourBlockedCount++;
            } else if (killingBoard.typeRecord[KillTypeEnum.FourBlocked] > four)
                fourBlockedCount++;
        }

        //计算此点对于当前角色的杀棋价值
        private KillingBoard EvaluateKill(Tuple<int, int> point, Role role) {
            KillingBoard killingBoard = new KillingBoard();
            // 从 typeRecord 获取统计数据
            int threeAliveCount = 0;
            int fourBlockedCount = 0;
            //活三冲四同时出现
            int ThreeAliveWithFourBlockedCount = 0;

            int i = point.Item1, j = point.Item2;
            var t = GetMainAndAntiDiagonalCoordinates(i, j);
            //分别计算四个角度
            PlayChess(i, j, role);
            UpdateKillingBoard(killingBoard, role, NormalBoard, i, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            UpdateKillingBoard(killingBoard, role, XYReversedBoard, j, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            UpdateKillingBoard(killingBoard, role, MainDiagonalBoard, t.Item1, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            UpdateKillingBoard(killingBoard, role, AntiDiagonalBoard, t.Item3, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            PlayChess(i, j, Role.Empty);

            //风险级别计算并更新得分
            // 冲四数大于1，+高风险评分
            if (fourBlockedCount > 1 || ThreeAliveWithFourBlockedCount > 1) {
                killingBoard.type = KillingRiskEnum.High;
                killingBoard.score += (int)KillingRiskEnum.High;
            }//冲四又活三，+中风险评分
            else if ((fourBlockedCount > 0 && threeAliveCount > 0) ||
                (ThreeAliveWithFourBlockedCount > 0 && threeAliveCount > 1)) {
                killingBoard.type = KillingRiskEnum.Middle;
                killingBoard.score += (int)KillingRiskEnum.Middle;
            }//活三数大于1，+低风险评分
            else if (threeAliveCount > 1) {
                killingBoard.type = KillingRiskEnum.Low;
                killingBoard.score += (int)KillingRiskEnum.Low;
            }

            return killingBoard;
        }

    }
}
