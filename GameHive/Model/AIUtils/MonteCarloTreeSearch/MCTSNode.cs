/*************************************************************************************
 * 文 件 名:   MCTSNode.cs
 * 描    述: 蒙特卡洛节点类
 *          负责：四过程中的反向传播
 *          提供方法：1.反向传播 2.计算自身UCB 3.获取UCB最大的儿子节点 4.添加儿子节点 5.判断是否为新叶节点或终止点
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/8 20:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIUtils.MonteCarloTreeSearch {
    internal class MCTSNode {
        public MCTSNode(List<List<Role>> board, MCTSNode father, int x, int y,
                    Role view, Role winner, List<Tuple<int, int>> availablePiece) {
            //初始化参数 价值、访问次数、是否为叶子节点
            VisitedTimes = 0; TotalValue = 0;
            IsLeaf = true;

            //初始化传递成员 
            //当前视角 游戏是否结束
            LeadToThisStatus = view; Winner = winner;
            //父节点 相对父节点落子
            Father = father; PieceSelectedCompareToFather = new Tuple<int, int>(x, y);

            //初始化数据结构
            NodeBoard = board;
            ChildrenMap = new Dictionary<Tuple<int, int>, MCTSNode>();
            AvailablePiece = availablePiece;
        }
        //当前视角下当前节点价值，胜利＋1，失败-1 平局0
        double TotalValue;
        //访问次数 N
        int VisitedTimes;
        //是否为叶子节点
        public bool IsLeaf { get; set; }
        //游戏是否终止 若不是Role.Empty表示赢家
        public Role Winner { get; set; }
        //当前视角
        public Role LeadToThisStatus { get; set; }
        //节点拥有的棋盘
        public List<List<Role>> NodeBoard { get; set; }
        //相比与父节点哪里落子了
        public Tuple<int, int> PieceSelectedCompareToFather { get; set; }
        //父节点
        MCTSNode Father;
        //落子-孩子的地图
        public Dictionary<Tuple<int, int>, MCTSNode> ChildrenMap { get; set; }
        //可落子地方
        public List<Tuple<int, int>> AvailablePiece { get; set; }

        //反向传播,根据单次模拟的结果更新参数
        public void BackPropagation(Role winner) {
            MCTSNode currentPropagate = this;
            while (currentPropagate != null) {
                currentPropagate.VisitedTimes++;
                //如果结果使得导致下棋后导致本局面的玩家获胜则加一分
                if (winner != Role.Draw)
                    currentPropagate.TotalValue += winner == currentPropagate.LeadToThisStatus ? 1 : -1;
                currentPropagate = currentPropagate.Father;
            }
        }

        //获取UCB,被父节点调用，父子节点视角不同
        public double GetUCB() {
            int N = Father.VisitedTimes;
            if (VisitedTimes == 0) return double.PositiveInfinity;
            double ans = 1.0 * TotalValue / (double)VisitedTimes
                + 1.414 * Math.Sqrt(Math.Log2(N) / VisitedTimes);
            return ans;
        }

        //获取UCB最大的节点
        public MCTSNode GetGreatestUCB() {
            MCTSNode chosenSon = null;
            double maxValue = double.NegativeInfinity;
            foreach (KeyValuePair<Tuple<int, int>, MCTSNode> entry in ChildrenMap) {
                MCTSNode node = entry.Value;
                double t = node.GetUCB();
                if (t > maxValue) {
                    maxValue = t;
                    chosenSon = node;
                }
            }
            return chosenSon;
        }

        //是否为新节点或为终止节点的等价新节点
        public bool IsNewLeaf() {
            if (VisitedTimes == 0) return true;
            //终止节点等价为新叶子节点
            if (Winner != Role.Empty) return true;
            return false;
        }

        //加入一个子节点
        public void AddSon(MCTSNode son, int x, int y) {
            ChildrenMap[new Tuple<int, int>(x, y)] = son;
        }
    }
}
