/*************************************************************************************
 * 文 件 名:   KillingBoardACAutomaton.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/13 1:03
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Model.AIUtils.AlphaBetaPruning {
    public class KillingBoardNode {
        public int[] Children = new int[3];
        public int Fail = 0;
        public KillTypeEnum Value = KillTypeEnum.Empty;
        public KillingBoardNode() {
            Array.Fill(Children, -1); // 初始化为 -1 表示无效索引
        }
    }

    public class KillingBoardACAutomaton {
        private List<KillingBoardNode> tree = new List<KillingBoardNode>();
        private List<int> pos = new List<int>();
        private List<List<int>> adjList = new List<List<int>>();
        Dictionary<List<Role>, KillTypeEnum> rewardTable;
        public KillingBoardACAutomaton(Dictionary<List<Role>, KillTypeEnum> rewardTable) {
            this.rewardTable = rewardTable;
            tree.Add(new KillingBoardNode());
            adjList.Add(new List<int>());
            foreach (var entry in rewardTable) {
                int node = Insert(entry.Key);
                tree[node].Value = entry.Value;
            }
            Build();
        }

        //提供计算列表估值方法,计算对应角色的估值并
        public int CalculateLineValue(List<Role> sequence, KillingBoard killingBoard) {
            int totalScore = 0;

            // 遍历sequence中的每个角色
            for (int i = 0; i < sequence.Count; i++) {
                // 遍历rewardTable中的每一项
                foreach (var entry in rewardTable) {
                    List<Role> roleSequence = entry.Key;  // 角色序列
                    KillTypeEnum killType = entry.Value;  // 对应的杀棋类型

                    // 判断当前sequence中，从位置i开始的子序列是否与roleSequence匹配
                    if (i + roleSequence.Count <= sequence.Count) { // 确保剩余长度足够匹配
                        bool match = true;
                        for (int j = 0; j < roleSequence.Count; j++) {
                            if (sequence[i + j] != roleSequence[j]) {
                                match = false;
                                break;
                            }
                        }

                        // 如果匹配成功，计算得分并更新typeRecord
                        if (match) {
                            totalScore += (int)killType;
                            killingBoard.typeRecord[killType]++;
                        }
                    }
                }
            }
            killingBoard.score += totalScore;
            return totalScore;
            int score = 0;
            int p = 0;
            int t = 0;
            for (int i = 0; i < sequence.Count; i++)
                if (i != 0 && sequence[i] == Role.AI && sequence[i - 1] == Role.AI) t++;
            if (t == 1)
                t++;
            foreach (var role in sequence) {
                int index = (int)role;
                p = tree[p].Children[index] == -1 ? tree[tree[p].Fail].Children[index] : tree[p].Children[index];
                for (int temp = p; temp > 0; temp = tree[temp].Fail) {
                    if (tree[temp].Value != KillTypeEnum.Empty) {
                        score += (int)tree[temp].Value;
                        killingBoard.typeRecord[tree[temp].Value]++;
                    }
                }
            }

            return score;
        }

        //插入一个估值表
        private int Insert(List<Role> roles) {
            int p = 0;
            foreach (var role in roles) {
                int index = (int)role;
                if (tree[p].Children[index] == -1) {
                    tree.Add(new KillingBoardNode());
                    adjList.Add(new List<int>());
                    tree[p].Children[index] = tree.Count - 1;
                }
                p = tree[p].Children[index];
            }
            return p;
        }

        //构建自动机
        private void Build() {
            Queue<int> q = new Queue<int>();
            for (int i = 0; i < tree[0].Children.Length; i++) {
                int c = tree[0].Children[i];
                if (c != -1) {
                    tree[c].Fail = 0;
                    adjList[0].Add(c);
                    q.Enqueue(c);
                }
            }
            while (q.Count > 0) {
                int t = q.Dequeue();
                for (int i = 0; i < tree[t].Children.Length; i++) {
                    int child = tree[t].Children[i];
                    if (child != -1) {
                        tree[child].Fail = tree[tree[t].Fail].Children[i] == -1 ? 0 : tree[tree[t].Fail].Children[i];
                        adjList[tree[child].Fail].Add(child);
                        q.Enqueue(child);
                    } else {
                        tree[t].Children[i] = tree[tree[t].Fail].Children[i] == -1 ? 0 : tree[tree[t].Fail].Children[i];
                    }
                }
            }
        }
    }
}
