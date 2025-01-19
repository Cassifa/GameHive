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