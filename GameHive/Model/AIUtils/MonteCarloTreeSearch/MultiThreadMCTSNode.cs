/*************************************************************************************
 * 文 件 名:   MultiThreadMCTSNode.cs
 * 描    述: 多线程蒙特卡洛节点类
 *          基于AlphaGo-Zero思路，使用虚拟损失(Virtual Loss)机制避免频繁加锁
 *          核心特性：1.虚拟损失协调多线程访问
 *                  2.原子操作更新统计信息
 *                  3.支持搜索树重用和换根操作
 *                  4.线程安全的UCB计算
 *          虚拟损失：当线程选择节点时临时增加损失，模拟完成后移除，避免多线程冲突
 * 版    本：  V3.0 引入多线程DRL搜索
 * 创 建 者：  Cassifa
 * 创建时间：  2025/1/15
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIUtils.MonteCarloTreeSearch {
    internal class MultiThreadMCTSNode {
        public MultiThreadMCTSNode(List<List<Role>> board, MultiThreadMCTSNode father, int x, int y,
                    Role view, Role winner, List<Tuple<int, int>> availablePiece, double priorProbability = 0.0) {
            //初始化统计信息
            VisitedTimes = 0;
            TotalValue = 0.0;
            VirtualLoss = 0;
            IsExpanding = 0;
            IsLeaf = true;

            //初始化游戏状态
            LeadToThisStatus = view;
            SetWinner(winner);
            Father = father;
            PieceSelectedCompareToFather = new Tuple<int, int>(x, y);

            //初始化数据结构
            NodeBoard = board;
            ChildrenMap = new Dictionary<int, MultiThreadMCTSNode>();
            AvailablePiece = availablePiece;
            PriorProbability = priorProbability;

            //线程安全锁
            ExpandLock = new object();
        }

        //统计信息 - 使用原子操作更新
        private long VisitedTimes;
        private double TotalValue;
        private int VirtualLoss; //虚拟损失，用于协调多线程访问
        private int IsExpanding; //是否正在扩展，0表示未扩展，1表示正在扩展

        //游戏状态
        public bool IsLeaf { get; set; }
        private Role Winner;
        public Role LeadToThisStatus { get; set; }
        public List<List<Role>> NodeBoard { get; set; }
        public Tuple<int, int> PieceSelectedCompareToFather { get; set; }
        private MultiThreadMCTSNode? Father;
        public Dictionary<int, MultiThreadMCTSNode> ChildrenMap { get; set; }
        public List<Tuple<int, int>> AvailablePiece { get; set; }
        public double PriorProbability { get; set; }

        //拓展锁-线程安全
        private readonly object ExpandLock;

        //获取访问次数-线程安全
        public long GetVisitedTimes() {
            return Interlocked.Read(ref VisitedTimes);
        }

        //获取平均价值-线程安全
        public double GetAverageValue() {
            long visits = Interlocked.Read(ref VisitedTimes);
            if (visits == 0)
                return 0.0;
            return TotalValue / visits;
        }

        //获取胜者
        public Role GetWinner() {
            return Winner;
        }

        //设置胜者
        private void SetWinner(Role role) {
            Winner = role;
        }

        //批量添加虚拟损失
        public void AddVirtualLoss(int count = 1) {
            if (count == 1) {
                Interlocked.Increment(ref VirtualLoss);
            } else {
                Interlocked.Add(ref VirtualLoss, count);
            }
        }

        //批量移除虚拟损失
        public void RemoveVirtualLoss(int count = 1) {
            if (count == 1) {
                Interlocked.Decrement(ref VirtualLoss);
            } else {
                Interlocked.Add(ref VirtualLoss, -count);
            }
        }

        //更新统计信息（模拟完成后调用）
        public void UpdateStats(double value) {
            Interlocked.Increment(ref VisitedTimes);
            //使用锁保护浮点数更新
            lock (this) {
                TotalValue += value;
            }
        }

        //反向传播（线程安全版本）
        public void BackPropagation(Role winner) {
            MultiThreadMCTSNode? currentPropagate = this;
            while (currentPropagate != null) {
                double value = 0.0;
                if (winner != Role.Draw) {
                    value = winner == currentPropagate.LeadToThisStatus ? 1.0 : -1.0;
                }
                currentPropagate.UpdateStats(value);
                currentPropagate = currentPropagate.Father;
            }
        }

        //DRL反向传播：使用神经网络Value进行反向传播（AlphaGo正确设计，线程安全）
        public void BackPropagationWithValue(double networkValue) {
            MultiThreadMCTSNode? currentPropagate = this;
            while (currentPropagate != null) {
                //神经网络Value是从当前玩家视角的评估，需要根据节点视角调整
                //networkValue > 0表示当前玩家优势，转换为节点视角
                double adjustedValue;
                if (currentPropagate.LeadToThisStatus == Role.AI) {
                    adjustedValue = networkValue; // AI视角：直接使用Value
                } else {
                    adjustedValue = -networkValue; // Player视角：取反
                }
                currentPropagate.UpdateStats(adjustedValue);
                currentPropagate = currentPropagate.Father;
            }
        }

        //计算UCB值（考虑虚拟损失）
        public double GetUCB(double factor) {
            //如果已经是终局节点
            if (Winner != Role.Empty) {
                return Winner == LeadToThisStatus ? double.PositiveInfinity : double.NegativeInfinity;
            }

            if (Father == null)
                return double.PositiveInfinity;

            long visits = GetVisitedTimes();
            int virtualLoss = Interlocked.CompareExchange(ref VirtualLoss, 0, 0);

            //考虑虚拟损失的访问次数
            long effectiveVisits = visits + virtualLoss;
            if (effectiveVisits == 0)
                return double.PositiveInfinity;

            long parentVisits = Father.GetVisitedTimes();
            double Q = GetAverageValue() - (double)virtualLoss / effectiveVisits; //虚拟损失降低Q值
            double U = factor * PriorProbability * Math.Sqrt(parentVisits) / (1 + effectiveVisits);

            return Q + U;
        }

        //获取UCB最大的子节点-线程安全
        public MultiThreadMCTSNode? GetGreatestUCB(double factor) {
            MultiThreadMCTSNode? chosenSon = null;
            double maxValue = double.NegativeInfinity;

            foreach (var entry in ChildrenMap) {
                MultiThreadMCTSNode node = entry.Value;
                double ucbValue = node.GetUCB(factor);

                if (ucbValue >= maxValue) {
                    if (ucbValue == double.PositiveInfinity) {
                        return node;
                    }
                    maxValue = ucbValue;
                    chosenSon = node;
                }
            }
            return chosenSon;
        }

        //线程安全的节点扩展
        public bool TryStartExpansion() {
            return Interlocked.CompareExchange(ref IsExpanding, 1, 0) == 0;
        }

        //完成扩展
        public void FinishExpansion() {
            Interlocked.Exchange(ref IsExpanding, 0);
            IsLeaf = false;
        }

        //检查是否正在扩展
        public bool IsCurrentlyExpanding() {
            return Interlocked.CompareExchange(ref IsExpanding, 0, 0) == 1;
        }

        //是否为新叶子节点
        public bool IsNewLeaf() {
            if (GetVisitedTimes() == 0)
                return true;
            if (Winner != Role.Empty)
                return true;
            return false;
        }

        //添加子节点-线程安全
        public void AddSon(MultiThreadMCTSNode son, int x, int y) {
            lock (ExpandLock) {
                ChildrenMap[x * 100 + y] = son;
            }
        }

        //换根操作-线程安全
        public MultiThreadMCTSNode MoveRoot(int x, int y, Action<MultiThreadMCTSNode, bool> NodeExpansion) {
            //检查子节点是否存在
            if (!ChildrenMap.ContainsKey(x * 100 + y)) {
                NodeExpansion(this, true);
            }

            MultiThreadMCTSNode son = ChildrenMap[x * 100 + y];
            //解除父子关系
            son.Father = null;
            return son;
        }
    }
}

