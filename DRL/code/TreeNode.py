import numpy as np


class TreeNode():
    """
    蒙特卡洛搜索树的节点类
    每个节点代表棋盘的一个状态
    """

    def __init__(self, parent, prior_p):
        """
        初始化节点
        :param parent: 父节点
        :param prior_p: 该节点被选择的先验概率 (由策略网络预测)
        """
        self.NUM = 1
        self.father = parent  # 父节点指针
        self.children = {}  # 孩子节点字典 {action: TreeNode}
        self.N_visits = 0  # N: 该节点的访问次数
        self.Q = 0  # Q: 节点的平均价值 (Total Value / N)
        self.U = 0  # U: 探索价值 (Upper Confidence Bound)，与 P 成正比，与 N 成反比
        self.P = prior_p  # P: 走某一步棋(a)的先验概率 (Policy)

    def getValue(self, factor):
        """
        计算该节点的综合评分 (Q + U)，用于 MCTS 的选择步骤 (Selection)
        :param factor: 探索常数 c_puct，控制探索(Exploration)与利用(Exploitation)的平衡
        """
        # UCB 公式:
        # U = c_puct * P * sqrt(Parent_N) / (1 + N)
        # - 如果节点被访问很少 (N小)，U 值会大，鼓励探索
        # - 如果先验概率 P 大，U 值会大，鼓励搜索神经网络认为好的点
        # Q 值代表目前为止模拟的平均胜率
        self.U = (factor * self.P * np.sqrt(self.father.N_visits) / (1 + self.N_visits))
        return self.Q + self.U

    def expand(self, action_priors):
        """
        扩展 (Expansion): 当搜索到达叶子节点且该节点未结束时，生成子节点
        :param action_priors: (action, probability) 列表，由策略网络输出
        """
        for action, prob in action_priors:
            if action not in self.children:
                self.children[action] = TreeNode(self, prob)

    def select(self, factor):
        """
        选择 (Selection): 在当前节点的所有子节点中，选择分数 (Q+U) 最高的那个
        :return: (best_action, best_child_node)
        """
        return max(self.children.items(), key=lambda act_node: act_node[1].getValue(factor))

    def update(self, leaf_value):
        """
        更新 (Update): 更新当前节点的统计信息 (N, Q)
        :param leaf_value: 这次模拟的最终价值 (从当前玩家视角看)
        """
        self.N_visits += 1
        # Q = Q_old + (New_Value - Q_old) / N
        # 这是一个增量平均数公式
        self.Q += 1.0 * (leaf_value - self.Q) / self.N_visits

    def updateRecursive(self, leaf_value):
        """
        回溯 (Backup): 递归更新从当前节点一直到根节点的路径
        :param leaf_value: 叶子节点的价值评估
        """
        # 注意: leaf_value 在每一层递归时要取反 (-leaf_value)
        # 因为父节点是对手，对手的收益 = -我的收益
        if self.father:
            self.father.updateRecursive(-leaf_value)
        self.update(leaf_value)

    def isLeaf(self):
        """
        判断该节点是不是叶子节点：没有孩子的就是叶子
        """
        return self.children == {}

    def isRoot(self):
        """
        判断该节点是不是根节点：没有父亲的就是根
        """
        return self.father is None

    def __str__(self):
        return "Node(" + str(self.NUM) + ',' + str(len(self.children)) + ')'
