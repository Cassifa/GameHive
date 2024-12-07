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
        public GoBangMinMax() {
            maxDeep = 4;
            InitACAutomaton();
        }
        /*****实现四个博弈树策略*****/
        public override Role CheckGameOver(List<List<Role>> currentBoard) {
            // 横向检查
            for (int i = 0; i < currentBoard.Count; i++) {
                for (int j = 0; j <= currentBoard[i].Count - 5; j++) {
                    if (currentBoard[i][j] == Role.AI && currentBoard[i][j + 1] == Role.AI &&
                        currentBoard[i][j + 2] == Role.AI && currentBoard[i][j + 3] == Role.AI &&
                        currentBoard[i][j + 4] == Role.AI) {
                        return Role.AI; // AI 胜利
                    }
                    if (currentBoard[i][j] == Role.Player && currentBoard[i][j + 1] == Role.Player &&
                        currentBoard[i][j + 2] == Role.Player && currentBoard[i][j + 3] == Role.Player &&
                        currentBoard[i][j + 4] == Role.Player) {
                        return Role.Player; // Player 胜利
                    }
                }
            }
            // 纵向检查
            for (int i = 0; i <= currentBoard.Count - 5; i++) {
                for (int j = 0; j < currentBoard[i].Count; j++) {
                    if (currentBoard[i][j] == Role.AI && currentBoard[i + 1][j] == Role.AI &&
                        currentBoard[i + 2][j] == Role.AI && currentBoard[i + 3][j] == Role.AI &&
                        currentBoard[i + 4][j] == Role.AI) {
                        return Role.AI; // AI 胜利
                    }
                    if (currentBoard[i][j] == Role.Player && currentBoard[i + 1][j] == Role.Player &&
                        currentBoard[i + 2][j] == Role.Player && currentBoard[i + 3][j] == Role.Player &&
                        currentBoard[i + 4][j] == Role.Player) {
                        return Role.Player; // Player 胜利
                    }
                }
            }

            // 主对角线检查
            for (int i = 0; i <= currentBoard.Count - 5; i++) {
                for (int j = 0; j <= currentBoard[i].Count - 5; j++) {
                    if (currentBoard[i][j] == Role.AI && currentBoard[i + 1][j + 1] == Role.AI &&
                        currentBoard[i + 2][j + 2] == Role.AI && currentBoard[i + 3][j + 3] == Role.AI &&
                        currentBoard[i + 4][j + 4] == Role.AI) {
                        return Role.AI; // AI 胜利
                    }
                    if (currentBoard[i][j] == Role.Player && currentBoard[i + 1][j + 1] == Role.Player &&
                        currentBoard[i + 2][j + 2] == Role.Player && currentBoard[i + 3][j + 3] == Role.Player &&
                        currentBoard[i + 4][j + 4] == Role.Player) {
                        return Role.Player; // Player 胜利
                    }
                }
            }

            // 副对角线检查
            for (int i = 0; i <= currentBoard.Count - 5; i++) {
                for (int j = 4; j < currentBoard[i].Count; j++) {
                    if (currentBoard[i][j] == Role.AI && currentBoard[i + 1][j - 1] == Role.AI &&
                        currentBoard[i + 2][j - 2] == Role.AI && currentBoard[i + 3][j - 3] == Role.AI &&
                        currentBoard[i + 4][j - 4] == Role.AI) {
                        return Role.AI; // AI 胜利
                    }
                    if (currentBoard[i][j] == Role.Player && currentBoard[i + 1][j - 1] == Role.Player &&
                        currentBoard[i + 2][j - 2] == Role.Player && currentBoard[i + 3][j - 3] == Role.Player &&
                        currentBoard[i + 4][j - 4] == Role.Player) {
                        return Role.Player; // Player 胜利
                    }
                }
            }
            for (int i = 0; i < currentBoard.Count; i++)
                for (int j = 0; j < currentBoard[i].Count; j++)
                    if (currentBoard[i][j] == Role.Empty)
                        return Role.Empty;

            return Role.Draw;
        }


        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            int ans = 0;

            // 估值行
            foreach (var row in currentBoard)
                ans += ACautomaton.CalculateLineValue(row, role);

            // 估值列
            for (int col = 0; col < currentBoard.Count; col++) {
                var sequence = new List<Role>();
                for (int row = 0; row < currentBoard.Count; row++) {
                    sequence.Add(currentBoard[row][col]);  // 保持空格
                }
                ans += ACautomaton.CalculateLineValue(sequence, role);
            }
            // 估值主对角线 (左下到右上)
            // 从左下开始到右上的对角线
            for (int start = 0; start < currentBoard.Count; start++) {
                var sequence = new List<Role>();
                for (int i = 0, j = start; i < currentBoard.Count && j >= 0; i++, j--) {
                    sequence.Add(currentBoard[i][j]);  // 保持空格
                }
                ans += ACautomaton.CalculateLineValue(sequence, role);
            }
            // 从第一行到右上半棋盘的对角线
            for (int start = 1; start < currentBoard.Count; start++) {
                var sequence = new List<Role>();
                for (int i = start, j = currentBoard.Count - 1; i < currentBoard.Count && j >= 0; i++, j--) {
                    sequence.Add(currentBoard[i][j]);  // 保持空格
                }
                ans += ACautomaton.CalculateLineValue(sequence, role);
            }
            // 估值副对角线 (左上到右下)
            for (int b = -currentBoard.Count + 1; b < currentBoard.Count; b++) {
                var sequence = new List<Role>();
                for (int x = 0; x < currentBoard.Count; x++) {
                    int y = x + b;
                    if (y >= 0 && y < currentBoard.Count) {
                        sequence.Add(currentBoard[x][y]);  // 保持空格
                    }
                }
                ans += ACautomaton.CalculateLineValue(sequence, role);
            }
            return ans;
        }


        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            // 最终结果
            var usefulSteps = new List<Tuple<int, int>>();
            int[,] priorityMap = new int[board.Count, board[0].Count];
            // 初始化优先级表
            for (int i = 0; i < board.Count; i++) {
                for (int j = 0; j < board[i].Count; j++) {
                    if (board[i][j] != Role.Empty) {
                        priorityMap[i, j] = int.MinValue; // 已落子的点赋值为不可用
                    }
                }
            }
            // 添加权重
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1, -2, -2, -2, -1, -1, 0, 0, 1, 1, 2, 2, 2, -2, 0, 2, 0 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1, -2, 0, 2, -2, 2, -2, 2, -2, 2, -2, 0, 2, 0, 2, 0, -2 };
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
            //temp.Sort((a, b) => b.Item1.CompareTo(a.Item1));
            foreach (var t in temp) {
                usefulSteps.Add(t.Item2);
            }
            // 如果没有有价值的点位，则随机走中间九格之一
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


        protected override void InitACAutomaton() {
            var rewardTable = new Dictionary<List<Role>, int>();
            // 四
            // 活四 _OOOO_
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.AI, Role.AI, Role.AI, Role.Empty }] = 5000;
            // 冲四 O_OOO _OOOOX OO_OO
            rewardTable[new List<Role> { Role.AI, Role.Empty, Role.AI, Role.AI, Role.AI }] = 700;
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.AI, Role.AI, Role.AI, Role.Player }] = 1000;
            rewardTable[new List<Role> { Role.AI, Role.AI, Role.Empty, Role.AI, Role.AI }] = 700;

            // 三
            // 活三（可成活四）_OOO_ O_OO
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.AI, Role.AI, Role.Empty }] = 800;
            rewardTable[new List<Role> { Role.AI, Role.Empty, Role.AI, Role.AI }] = 150;
            // 眠三 __OOOX _O_OOX _OO_OX O__OO O_O_O X_OOO_X
            rewardTable[new List<Role> { Role.Empty, Role.Empty, Role.AI, Role.AI, Role.AI, Role.Player }] = 100;
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.Empty, Role.AI, Role.AI, Role.Player }] = 80;
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.AI, Role.Empty, Role.AI, Role.Player }] = 60;
            rewardTable[new List<Role> { Role.AI, Role.Empty, Role.Empty, Role.AI, Role.AI }] = 60;
            rewardTable[new List<Role> { Role.AI, Role.Empty, Role.AI, Role.Empty, Role.AI }] = 60;
            rewardTable[new List<Role> { Role.Player, Role.Empty, Role.AI, Role.AI, Role.AI, Role.Empty, Role.Player }] = 60;

            // 二
            // 活二 __OO__ _O_O_ O__O
            rewardTable[new List<Role> { Role.Empty, Role.Empty, Role.AI, Role.AI, Role.Empty, Role.Empty }] = 50;
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.Empty, Role.AI, Role.Empty }] = 20;
            rewardTable[new List<Role> { Role.AI, Role.Empty, Role.Empty, Role.AI }] = 20;
            // 眠二 ___OOX __O_OX _O__OX O___O
            rewardTable[new List<Role> { Role.Empty, Role.Empty, Role.Empty, Role.AI, Role.AI, Role.Player }] = 10;
            rewardTable[new List<Role> { Role.Empty, Role.Empty, Role.AI, Role.Empty, Role.AI, Role.Player }] = 10;
            rewardTable[new List<Role> { Role.Empty, Role.AI, Role.Empty, Role.Empty, Role.AI, Role.Player }] = 10;
            rewardTable[new List<Role> { Role.AI, Role.Empty, Role.Empty, Role.Empty, Role.AI }] = 10;

            // 添加翻转情况
            var reversedTable = new Dictionary<List<Role>, int>();
            foreach (var entry in rewardTable) {
                var reversed = new List<Role>(entry.Key);
                reversed.Reverse();
                reversedTable[reversed] = entry.Value;
            }

            foreach (var entry in reversedTable) {
                rewardTable[entry.Key] = entry.Value;
            }

            // 构造 AC 自动机
            ACautomaton = new ACAutomaton(rewardTable);
        }

    }
}
