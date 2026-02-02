import copy
from TreeNode import *


def softmax(x):
    """归一化函数，将数值转换为概率分布"""
    probs = np.exp(x - np.max(x))
    probs /= np.sum(probs)
    return probs


class MCTS():
    """
    蒙特卡洛树搜索 (Monte Carlo Tree Search)
    AlphaZero 的核心决策引擎
    """

    def __init__(self, policy_NN, factor=5, simulations=100):
        self.root = TreeNode(None, 1.0)  # 初始化根节点
        self.policy_NN = policy_NN  # 策略价值网络
        self.fator = factor  # c_puct 探索常数，控制探索程度
        self.simulations = simulations  # 每次决策进行的模拟次数

    def playout(self, state):
        """
        一次完整的 MCTS 模拟过程：
        1. 选择 (Selection)
        2. 评估 (Evaluation) & 扩展 (Expansion)
        3. 回溯 (Backup)
        """
        node = self.root
        
        # 1. 选择 (Selection)
        # 从根节点向下走，直到到达叶子节点
        while True:
            if node.isLeaf():
                break
            # 选择 Q+U 值最大的子节点
            action, node = node.select(self.fator)
            state.do_move(action)
            
        # 2. 评估 (Evaluation)
        # 使用神经网络评估当前的叶子节点局面
        # action_probs: 策略头输出的落子概率 (用于扩展)
        # leaf_value: 价值头输出的胜率预估 (用于回溯)
        action_probs, leaf_value = self.policy_NN(state)

        # 检查游戏是否结束
        gameOver, winner = state.gameIsOver()

        # 如果这盘棋还没结束，扩展该节点
        if not gameOver:
            node.expand(action_probs)
        else:
            # 如果游戏结束，直接使用真实胜负作为 leaf_value
            if winner == -1: # 平局
                leaf_value = 0.0
            else:
                # 如果获胜的是当前视角的玩家，收益为 +1，否则 -1
                leaf_value = (
                    1.0 if winner == state.getCurrentPlayer() else -1.0
                )

        # 3. 回溯 (Backup)
        # 将叶子节点的价值 leaf_value 沿着路径反向传播更新所有父节点
        # 注意取反，因为每一层玩家交替
        node.updateRecursive(-leaf_value)

    def getMoveProbs(self, state, flag_is_train):
        """
        执行多次模拟，并返回落子概率分布 π
        :param state: 当前棋盘状态
        :param flag_is_train: 是否处于训练模式 (训练模式下会增加探索性)
        """
        # exploration 参数: 控制 softmax 温度
        # 训练时为 1.0，保证一定的随机性
        # 对战时(非训练) 极小值，趋向于选择访问次数最多的点 (Greedy)
        exploration = 1.0 if flag_is_train else 1e-3

        # 执行 simulations 次模拟
        for _ in range(self.simulations):
            state_copy = copy.deepcopy(state) # 必须拷贝棋盘，以免污染真实状态
            self.playout(state_copy)

        # 统计根节点下所有子节点的访问次数 N
        # MCTS 的核心思想：访问次数越多，说明该点越好
        if not self.root.children:
            # 防御性编程：如果根节点没有任何子节点（例如游戏已经结束，或者模拟失败）
            # 返回空列表或根据游戏状态处理
            # 对于 Antigo，可能出现开局即无路可走的情况（虽然不太可能），或者在模拟中由于规则导致所有动作非法
            print("Warning: MCTS root has no children. Game might be over or stuck.")
            # 尝试返回所有可行位置的均匀分布，或者直接抛出更详细的错误
            available_moves = state.availables
            if not available_moves:
                 return [], [] # 真的无路可走
            
            # 兜底逻辑：如果没有子节点，就从未扩展的动作中随机选一个
            # 这种情况通常发生在该节点第一次被访问且没有扩展成功时
            # 但在正常的 simulations 循环后不应发生，除非 simulations=0
            return list(available_moves), np.ones(len(available_moves)) / len(available_moves)

        act_visits = [(act, node.N_visits) for act, node in self.root.children.items()]
        acts, visits = zip(*act_visits)
        
        # 将访问次数转换为概率分布
        act_probs = softmax(1.0 / exploration * np.log(np.array(visits) + 1e-10))

        return acts, act_probs

    def updateMCTS(self, move):
        """
        在真实落子后，重用这棵搜索树
        """
        if move in self.root.children:
            # 如果走的这一步在就是当前树根的孩子之一
            # 直接将根节点移动到该孩子节点，保留该子树的统计信息
            self.root = self.root.children[move]
            self.root.father = None
        else:
            # 如果这一步不在孩子中 (极少见，或者游戏刚开始)，重置整棵树
            self.root = TreeNode(None, 1.0)

    def __str__(self):
        return "MCTS"
