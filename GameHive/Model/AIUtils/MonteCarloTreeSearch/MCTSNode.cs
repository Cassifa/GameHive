/*************************************************************************************
 * 文 件 名:   MCTSNode.cs
 * 描    述: 蒙特卡洛节点类
 *          负责：四过程中的反向传播
 *          提供方法：1.反向传播 
 *                  2.计算自身UCB 
 *                  3.获取UCB最大的儿子节点 
 *                  4.添加儿子节点 
 *                  5.判断是否为新叶节点或终止点 
 *                  6.换根
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/8 20:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIUtils.MonteCarloTreeSearch {
    internal class MCTSNode {
        public MCTSNode(List<List<Role>> board, MCTSNode father, int x, int y,
                    Role view, Role winner, List<Tuple<int, int>> availablePiece) {
            //初始化参数 价值、访问次数、是否为叶子节点
            VisitedTimes = 0;
            TotalValue = 0;
            IsLeaf = true;

            //初始化传递成员 
            //当前视角 游戏是否结束
            LeadToThisStatus = view;
            setWinner(winner);
            //父节点 相对父节点落子
            Father = father;
            PieceSelectedCompareToFather = new Tuple<int, int>(x, y);

            //初始化数据结构
            NodeBoard = board;
            ChildrenMap = new Dictionary<int, MCTSNode>();
            AvailablePiece = availablePiece;

            //默认先验概率为0（传统MCTS不使用先验概率）
            PriorProbability = 0.0;
        }

        //重载构造函数：支持AlphaGo-Zero风格的先验概率
        public MCTSNode(List<List<Role>> board, MCTSNode father, int x, int y,
                    Role view, Role winner, List<Tuple<int, int>> availablePiece, double priorProbability)
            : this(board, father, x, y, view, winner, availablePiece) {
            //设置神经网络提供的先验概率
            PriorProbability = priorProbability;
        }

        //当前视角下当前节点价值，胜利＋1，失败-1 平局0
        double TotalValue;
        //是否为叶子节点
        public bool IsLeaf { get; set; }
        //游戏是否终止 若不是Role.Empty表示赢家
        private Role Winner;
        //访问次数 N
        public int VisitedTimes;
        //当前视角
        public Role LeadToThisStatus { get; set; }
        //节点拥有的棋盘
        public List<List<Role>> NodeBoard { get; set; }
        //相比与父节点哪里落子了
        public Tuple<int, int> PieceSelectedCompareToFather { get; set; }
        //父节点
        private MCTSNode? Father;
        //落子-孩子的地图
        public Dictionary<int, MCTSNode> ChildrenMap { get; set; }
        //可落子地方
        public List<Tuple<int, int>> AvailablePiece { get; set; }
        //先验概率 P - 用于AlphaGo-Zero风格的UCB公式
        public double PriorProbability { get; set; }
        public Role getWinner() {
            return Winner;
        }
        private void setWinner(Role role) {
            Winner = role;
        }
        //反向传播,根据单次模拟的结果更新参数
        public void BackPropagation(Role winner) {
            MCTSNode? currentPropagate = this;
            while (currentPropagate != null) {
                currentPropagate.VisitedTimes++;
                //如果结果使得导致下棋后导致本局面的玩家获胜则加一分
                if (winner != Role.Draw)
                    currentPropagate.TotalValue += winner == currentPropagate.LeadToThisStatus ? 1 : -1;
                currentPropagate = currentPropagate.Father;
            }
        }

        //DRL反向传播：使用神经网络value进行反向传播
        public void BackPropagationWithValue(double networkValue) {
            MCTSNode? currentPropagate = this;
            while (currentPropagate != null) {
                currentPropagate.VisitedTimes++;
                //神经网络 value 是从当前玩家视角的评估，需要根据节点视角调整
                //networkValue > 0表示当前玩家优势，转换为节点视角
                if (currentPropagate.LeadToThisStatus == Role.AI) {
                    currentPropagate.TotalValue += networkValue; // AI视角：直接使用 value
                } else {
                    currentPropagate.TotalValue -= networkValue; // Player视角：取反
                }
                currentPropagate = currentPropagate.Father;
            }
        }

        //运行反向传播MinMax 传入刚拓展过的父节点，会反向传播其子节点的胜利信息 此处未考虑平局
        public void RunBackPropagateMinMax() {
            //开启反向传播MinMax
            MCTSNode? currentPropagate = this;
            Role result;
            do {
                result = BackPropagateMinMax(currentPropagate);
                currentPropagate = currentPropagate.Father;
            }
            //MinMax一旦成功，继续向上MinMax操作
            while (result.IsVictory() && currentPropagate != null);

        }

        //反向传播MinMax 根据子节点检查自身胜利状态
        private Role BackPropagateMinMax(MCTSNode node) {
            //查到了终局节点（有赢家）直接返回
            if (node.Winner.IsVictory())
                return node.Winner;
            //若子树尚未拓展，直接返回
            if (node.ChildrenMap.Count == 0)
                return node.Winner;
            //判断第一个子节点赢家
            Role firstChildWinner = BackPropagateMinMax(node.ChildrenMap.Values.First());
            if (!firstChildWinner.IsVictory())
                return Role.Empty;
            //如果剩余子节点胜利状态全部一致
            bool allSameWinner = ChildrenMap.Values.All(node => BackPropagateMinMax(node) == firstChildWinner);
            if (allSameWinner)
                node.Winner = (firstChildWinner == Role.AI) ? Role.Player : Role.AI;
            return node.Winner;
        }

        //换根，换根操作一定滞后与拓展操作，不需要记录缓存表的 value
        public MCTSNode MoveRoot(int x, int y, Action<MCTSNode, bool> NodeExpansion) {
            //由于进入必败局面导致未找到可行点，换根目标节点为随机选择，未拓展过。需要强制拓展
            if (!ChildrenMap.ContainsKey(x * 100 + y))
                NodeExpansion(this, true);
            MCTSNode son = ChildrenMap[x * 100 + y];
            //解除指针，释放父节点内存
            son.Father = null;
            return son;
        }

        //获取UCB,被父节点调用，父子节点视角不同
        //UCB公式: Q + 1.414 * sqrt(ln(N_parent) / N_child)
        //Q = 平均收益, 1.414 ≈ sqrt(2), N_parent = 父节点访问次数, N_child = 子节点访问次数
        public double GetUCB() {
            //如果已经是终局节点则价值直接最大或最小
            if (Winner != Role.Empty)
                return Winner == LeadToThisStatus ? double.PositiveInfinity : double.NegativeInfinity;
            int N = Father.VisitedTimes;
            if (VisitedTimes == 0)
                return double.PositiveInfinity;
            double ans = 1.0 * TotalValue / (double)VisitedTimes
                + 1.414 * Math.Sqrt(Math.Log2(N) / VisitedTimes);
            return ans;
        }

        //DRL-UCB公式: Q + factor * P * sqrt(N_parent) / (1 + N_child)
        //其中: Q = 平均收益, factor = 探索常数, P = 先验概率, N_parent = 父节点访问次数, N_child = 子节点访问次数
        public double GetUCB(double factor) {
            //如果已经是终局节点则价值直接最大或最小
            if (Winner != Role.Empty)
                return Winner == LeadToThisStatus ? double.PositiveInfinity : double.NegativeInfinity;
            int N = Father.VisitedTimes;
            if (VisitedTimes == 0)
                return double.PositiveInfinity;
            double Q = 1.0 * TotalValue / (double)VisitedTimes;
            double U = factor * PriorProbability * Math.Sqrt(N) / (1 + VisitedTimes);
            return Q + U;
        }

        //获取UCB最大的节点
        public MCTSNode GetGreatestUCB() {
            MCTSNode chosenSon = null;
            double maxValue = double.NegativeInfinity;
            foreach (var entry in ChildrenMap) {
                MCTSNode node = entry.Value;
                double t = node.GetUCB();
                //>=涵盖了maxValue==double.NegativeInfinity情况，使得搜到无解局面时得以反向传播
                if (t >= maxValue) {
                    if (t == double.PositiveInfinity)
                        return node;
                    maxValue = t;
                    chosenSon = node;
                }
            }
            return chosenSon;
        }

        //重载GetGreatestUCB：支持AlphaGo-Zero风格的UCB选择
        public MCTSNode GetGreatestUCB(double factor) {
            MCTSNode chosenSon = null;
            double maxValue = double.NegativeInfinity;
            foreach (var entry in ChildrenMap) {
                MCTSNode node = entry.Value;
                double t = node.GetUCB(factor);
                if (t >= maxValue) {
                    if (t == double.PositiveInfinity)
                        return node;
                    maxValue = t;
                    chosenSon = node;
                }
            }
            return chosenSon;
        }

        //DRL专用UCB公式：正确处理未访问节点的先验概率
        //Python: U = factor * P * sqrt(N_parent) / (1 + N_child)
        //未访问节点：Q=0, U = factor * P * sqrt(N_parent)
        public double GetDRLUCB(double factor) {
            if (Winner != Role.Empty)
                return Winner == LeadToThisStatus ? double.PositiveInfinity : double.NegativeInfinity;
            
            int N = Father?.VisitedTimes ?? 1;
            
            if (VisitedTimes == 0) {
                // 未访问节点：只有探索项，高先验概率的动作优先
                return factor * PriorProbability * Math.Sqrt(N);
            }
            
            double Q = TotalValue / (double)VisitedTimes;
            double U = factor * PriorProbability * Math.Sqrt(N) / (1 + VisitedTimes);
            return Q + U;
        }

        //DRL专用：获取DRL-UCB最大的子节点
        public MCTSNode? GetGreatestDRLUCB(double factor) {
            MCTSNode? chosenSon = null;
            double maxValue = double.NegativeInfinity;
            foreach (var entry in ChildrenMap) {
                MCTSNode node = entry.Value;
                double t = node.GetDRLUCB(factor);
                if (t == double.PositiveInfinity)
                    return node;
                if (t > maxValue) {
                    maxValue = t;
                    chosenSon = node;
                }
            }
            return chosenSon;
        }

        //是否为新节点或为终止节点的等价新节点
        public bool IsNewLeaf() {
            if (VisitedTimes == 0)
                return true;
            //终止节点等价为新叶子节点
            if (Winner != Role.Empty)
                return true;
            return false;
        }

        //加入一个子节点
        public void AddSon(MCTSNode son, int x, int y) {
            ChildrenMap[x * 100 + y] = son;
        }
    }
}
