/*************************************************************************************
 * 文 件 名:   ACAutomaton.cs
 * 描    述: AC自动机工具类，通过得分规则构造自动机，并且提供估值方法
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/6 15:15
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Model.AIUtils.AlgorithmUtils {
    public class Node {
        public int[] Children = new int[3];
        public int Fail = 0;
        public int Value = 0;
        public Node() {
            Array.Fill(Children, -1); // 初始化为 -1 表示无效索引
        }
    }

    public class ACAutomaton {
        private List<Node> tree = new List<Node>();
        private List<int> pos = new List<int>();
        private List<List<int>> adjList = new List<List<int>>();

        public ACAutomaton(Dictionary<List<Role>, int> rewardTable) {
            tree.Add(new Node()); // 根节点
            adjList.Add(new List<int>());
            foreach (var entry in rewardTable) {
                int node = Insert(entry.Key);
                tree[node].Value = entry.Value;
            }
            Build();
        }

        //提供计算列表对于AI估值方法
        public int CalculateLineValue(List<Role> sequence) {
            int score = 0;
            int p = 0;
            foreach (var role in sequence) {
                int index = (int)role;
                p = tree[p].Children[index] == -1 ? tree[tree[p].Fail].Children[index] : tree[p].Children[index];
                for (int temp = p; temp > 0; temp = tree[temp].Fail) {
                    score += tree[temp].Value;
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
                    tree.Add(new Node());
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
//using GameHive.Constants.RoleTypeEnum;
//using System.Text.RegularExpressions;

//namespace GameHive.Model.AIUtils.AlgorithmUtils {
//    internal class ACAutomaton {
//        // 得分表
//        private Dictionary<string, int> AIRewardTable = new Dictionary<string, int>();
//        private Dictionary<string, int> PlayerRewardTable = new Dictionary<string, int>();
//        // 已记录匹配串
//        private Dictionary<string, int> AIScoreTable = new Dictionary<string, int>();
//        private Dictionary<string, int> PlayerScoreTable = new Dictionary<string, int>();

//        // 通过得分表构造 AC 自动机
//        public ACAutomaton(Dictionary<List<Role>, int> rewardTable) {
//            foreach (var entry in rewardTable) {
//                // 将 List<Role> 转换为字符串表示保存到 AIRewardTable
//                string key = new string(entry.Key.Select(role =>
//                    role == Role.Empty ? 'E' :
//                    role == Role.AI ? 'A' :
//                    role == Role.Player ? 'P' : 'X' // 默认值 'X' 对应 Role.Block
//                ).ToArray());
//                AIRewardTable[key] = entry.Value;
//                string reversedKey = new string(entry.Key.Select(role =>
//                    role == Role.Empty ? 'E' :
//                    role == Role.Player ? 'A' :
//                    role == Role.AI ? 'P' : 'X' // 默认值 'X' 对应 Role.Block
//                ).ToArray());
//                PlayerRewardTable[reversedKey] = entry.Value;
//            }
//        }

//        // 计算一组序列对于 role 的价值
//        public int CalculateLineValue(List<Role> list, Role role) {
//            string mode = string.Join("", list.Select(item =>
//                item == Role.AI ? "A" :
//                item == Role.Player ? "P" :
//                "E"));
//            int score = 0;
//            if (role == Role.AI) {
//                if (AIScoreTable.TryGetValue(mode, out score))
//                    return score;
//            } else {
//                if (PlayerScoreTable.TryGetValue(mode, out score))
//                    return score;
//            }
//            return ACAutomatonCalculateLineValue(mode, role);
//        }

//        // AC 自动机计算未出现过的序列对于 role 的价值，并返回
//        private int ACAutomatonCalculateLineValue(string mode, Role role) {
//            int score = 0;
//            var table = role == Role.AI ? AIRewardTable : PlayerRewardTable;

//            foreach (var pattern in table) {
//                string t = pattern.Key;
//                if (mode.Length < t.Length) continue;

//                // 使用正则表达式计算模式匹配的次数
//                int count = Regex.Matches(mode, Regex.Escape(t)).Count;

//                // 根据出现次数累加分数
//                score += count * pattern.Value;
//            }

//            // 缓存计算结果
//            var scoreTable = role == Role.AI ? AIScoreTable : PlayerScoreTable;
//            scoreTable[mode] = score;

//            return score;
//        }

//    }

//}
