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
* 版    本：  V1.0
* 创 建 者：  Cassifa
* 创建时间：  2024/11/26 18:36
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIUtils.AlgorithmUtils;
using GameHive.Model.AIUtils.AlphaBetaPruning;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class GoBang88MinMax : MinMax {
        //AC自动机执行工具类
        protected ACAutomaton ACAutomaton;
        protected KillingBoardACAutomaton KillingBoardACAutomaton;
        //四个变化坐标映射的棋盘，用于优化估值速度
        private List<List<Role>> NormalBoard, XYReversedBoard;
        private List<List<Role>> MainDiagonalBoard, AntiDiagonalBoard;
        public GoBang88MinMax(Dictionary<List<Role>, int> RewardTable, Dictionary<List<Role>, KillTypeEnum> killingTable) {
            maxDeep = 4; killingMaxDeep = 12;
            TotalPiecesCnt = 8;
            ACAutomaton = new ACAutomaton(RewardTable);
            KillingBoardACAutomaton = new KillingBoardACAutomaton(killingTable);

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
            //foreach (var row in NormalBoard)
            //    ans += ACAutomaton.CalculateLineValue(row);
            //foreach (var col in XYReversedBoard)
            //    ans += ACAutomaton.CalculateLineValue(col);
            //foreach (var mainDiagonal in MainDiagonalBoard)
            //    ans += ACAutomaton.CalculateLineValue(mainDiagonal);
            //foreach (var antiDiagonal in AntiDiagonalBoard)
            //    ans += ACAutomaton.CalculateLineValue(antiDiagonal);
            //return ans;
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
            var t = GetMainAndAntiDiagonalCoordinates(x, y);
            //主对角线从左下到右上0~2*TotalPiecesCnt-1
            MainDiagonalBoard[t.Item1][t.Item2] = role;
            //反对角线从左上到右下0~2*TotalPiecesCnt-1
            AntiDiagonalBoard[t.Item3][t.Item4] = role;
        }

        //获取主、反对角线坐标映射
        private Tuple<int, int, int, int> GetMainAndAntiDiagonalCoordinates(int i, int j) {
            return new Tuple<int, int, int, int>
                (TotalPiecesCnt - (i - j) - 1, i - j > 0 ? j : i,
                i + j, i + j < TotalPiecesCnt ? i : TotalPiecesCnt - j - 1);
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

        //对一个点落子后形成杀棋局面进行评估
        void UpdateKillingBoard(KillingBoard killingBoard, Role role, List<List<Role>> board, int rowId,
            ref int threeAliveCount, ref int fourBlockedCount, ref int ThreeAliveWithFourBlockedCount) {
            int three = killingBoard.typeRecord[KillTypeEnum.ThreeAlive];
            int four = killingBoard.typeRecord[KillTypeEnum.FourBlocked];
            KillingBoardACAutomaton.CalculateLineValue(board[rowId], role, killingBoard);
            //有活三更新活三值
            if (killingBoard.typeRecord[KillTypeEnum.ThreeAlive] > three) {
                threeAliveCount++;
                //同时活三冲四更新活三
                if (killingBoard.typeRecord[KillTypeEnum.FourBlocked] > four) ThreeAliveWithFourBlockedCount++;
            } else if (killingBoard.typeRecord[KillTypeEnum.FourBlocked] > four) fourBlockedCount++;
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
            UpdateKillingBoard(killingBoard, role, NormalBoard, i, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            UpdateKillingBoard(killingBoard, role, XYReversedBoard, j, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            UpdateKillingBoard(killingBoard, role, MainDiagonalBoard, t.Item1, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);
            UpdateKillingBoard(killingBoard, role, AntiDiagonalBoard, t.Item3, ref threeAliveCount, ref fourBlockedCount, ref ThreeAliveWithFourBlockedCount);

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
        //获取可杀气列表 第三项为此点的杀棋估值
        private List<Tuple<int, int, int>> GetVctPoints(Role type, List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            bool isAI = type == Role.AI;
            // 进攻点列表
            List<Tuple<int, int, int>> attackPointList = new List<Tuple<int, int, int>>();
            // 防守点列表
            List<Tuple<int, int, int>> defensePointList = new List<Tuple<int, int, int>>();
            // VCT列表
            List<Tuple<int, int, int>> vctPointList = new List<Tuple<int, int, int>>();
            // 局势是否危险
            bool isDanger = false;

            List<Tuple<int, int>> availableMoves = GetAvailableMovesByNewPieces(NormalBoard, lastAvailableMoves, lastX, lastY);

            foreach (var move in availableMoves) {
                int i = move.Item1, j = move.Item2;

                // 考虑自己的落子情况
                KillingBoard killingBoard = EvaluateKill(new Tuple<int, int>(i, j), type);
                if (killingBoard.type == KillingRiskEnum.High)
                    // 自己可以连五，直接返回
                    return new List<Tuple<int, int, int>> { new Tuple<int, int, int>(i, j, killingBoard.score) };

                //存在危险只找可以连五的棋子，尝试连续冲四防御
                if (isDanger) continue;

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
                if (killingBoard.type >= KillingRiskEnum.Middle) {
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
                    if (isAI) return attackPointList;
                    //对手可以选择进攻和防守
                    pointList.AddRange(attackPointList);
                }
                //VCT进攻
                pointList.AddRange(vctPointList);
            }
            if (defensePointList.Count > 0) {
                //进行防守
                //AI优先选择进攻，把防守点位放后面
                if (isAI) pointList.AddRange(defensePointList);
                //对手优先考虑防守，把防守点位放前面
                else pointList.InsertRange(0, defensePointList);
            }
            return pointList;
        }


        //计算VCT杀棋
        protected override Tuple<int, int>? VCT(Role type, int depth,
            List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            // 算杀失败
            if (depth == killingMaxDeep) return null;
            bool isAI = type == Role.AI;

            Tuple<int, int>? best = null;
            List<Tuple<int, int, int>> pointList = GetVctPoints(type, lastAvailableMoves, lastX, lastY);
            foreach (var point in pointList) {
                //已经形成必胜棋型
                if (point.Item3 >= (int)KillingRiskEnum.High)
                    //AI-返回当前节点 玩家-直接返回空
                    return isAI ? new Tuple<int, int>(point.Item1, point.Item2) : null;

                PlayChess(point.Item1, point.Item2, type);
                best = VCT(type == Role.AI ? Role.Player : Role.AI, depth + 1, lastAvailableMoves, point.Item1, point.Item2);
                PlayChess(point.Item1, point.Item2, Role.Empty);

                if (best == null) {
                    //AI还没找到可以算杀成功的棋子，继续找
                    if (isAI) continue;
                    //对手拦截成功，返回空表示算杀失败
                    return null;
                }
                //找到了 best,记录当前节点导致 best的节点
                best = new Tuple<int, int>(point.Item1, point.Item2);
                //已经找到杀棋点位
                if (isAI) break;

            }
            return best;
        }

    }
}
