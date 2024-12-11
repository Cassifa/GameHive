/*************************************************************************************
 * 文 件 名:   ACAutomaton.cs
 * 描    述: AC自动机工具类，通过得分规则构造自动机，并且提供估值方法
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/6 15:15
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

public class Node {
    public int[] Children = new int[3];
    public int Fail = 0;
    public int Count = 0;
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

    //提供计算列表估值方法
    public int CalculateLineValue(List<Role> sequence) {
        int score = 0;
        int p = 0;
        foreach (var role in sequence) {
            int index = (int)role;
            p = tree[p].Children[index] == -1 ? tree[tree[p].Fail].Children[index] : tree[p].Children[index];
            for (int temp = p; temp > 0; temp = tree[temp].Fail) {
                score += tree[temp].Value;
                tree[temp].Count++;
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
    private void Dfs(int u) {
        foreach (var v in adjList[u]) {
            Dfs(v);
            tree[u].Count += tree[v].Count;
        }
    }
}

//namespace GameHive.Model.AIUtils.AlgorithmUtils {
//    internal class ACAutomaton {
//        //得分表
//        private Dictionary<List<Role>, int> AIRewardTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
//        private Dictionary<List<Role>, int> PlayerRewardTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
//        //已记录匹配串
//        private Dictionary<List<Role>, int> AIScoreTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
//        private Dictionary<List<Role>, int> PlayerScoreTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
//        //通过得分表构造AC自动机
//        public ACAutomaton(Dictionary<List<Role>, int> RewardTable) {
//            foreach (var entry in RewardTable) {
//                // 原始规则保存到 AITable
//                AIRewardTable[entry.Key] = entry.Value;
//                // 构造 PlayerTable，反转 AI 和 Player
//                var reversedKey = entry.Key.Select(r =>
//                    r == Role.AI ? Role.Player : r == Role.Player ? Role.AI : r
//                ).ToList();
//                PlayerRewardTable[reversedKey] = entry.Value;
//            }
//        }
//        //计算一组序列对对于 role 的价值
//        public int CalculateLineValue(List<Role> mode, Role role) {
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


//        //AC自动机计算未出现过的序列对于 role 的价值,并返回
//        private int ACAutomatonCalculateLineValue(List<Role> mode, Role role) {
//            int score = 0;
//            var table = role == Role.AI ? AIRewardTable : PlayerRewardTable;
//            foreach (var pattern in table) {
//                int count = 0;
//                var t = pattern.Key;
//                if (mode.Count < t.Count) continue;
//                for (int i = 0; i <= mode.Count - t.Count; i++) {
//                    bool match = true;
//                    // 内部循环: 比较 `mode[i + j]` 和 `t[j]`
//                    for (int j = 0; j < t.Count; j++) {
//                        if (mode[i + j] != t[j]) {
//                            match = false;
//                            break; // 如果不匹配，直接跳出内层循环
//                        }
//                    }
//                    if (match) {
//                        count++;
//                        i += t.Count - 1;
//                    }
//                }
//                score += count * pattern.Value; // 出现次数 * 价值
//            }
//            var scoreTable = role == Role.AI ? AIScoreTable : PlayerScoreTable;
//            scoreTable[mode] = score;
//            return score;
//        }
//    }
//    public class ListRoleComparer : IEqualityComparer<List<Role>> {
//        public bool Equals(List<Role> x, List<Role> y) {
//            if (x.Count != y.Count) return false;
//            for (int i = 0; i < x.Count; i++) {
//                if (x[i] != y[i]) {
//                    return false;
//                }
//            }
//            return true;
//        }
//        public int GetHashCode(List<Role> obj) {
//            int hash = 17;
//            foreach (var item in obj) {
//                hash = hash * 31 + (int)item;
//            }
//            return hash;
//        }
//    }

//}



