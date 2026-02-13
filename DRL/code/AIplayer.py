from MCTS import *


class MCTSPlayer():
    """
    基于 MCTS 的 AI 玩家
    使用蒙特卡洛树搜索来决定落子
    """

    def __init__(self, policy_NN):
        self.simulations_play = 2000   # 对战模式模拟次数
        self.simulations_train = 1000  # 训练模式模拟次数
        self.factor = 5  # c_puct 探索因子
        self.MCTS = MCTS(policy_NN, self.factor, self.simulations_play)

    def resetMCTS(self):
        """重置搜索树 (通常在游戏结束时调用)"""
        self.MCTS.updateMCTS(-1)

    def getAction(self, board, flag_is_train):
        """
        获取 AI 的下一步动作
        :param board: 当前棋盘
        :param flag_is_train: 是否是训练模式
        """
        # 根据模式动态调整模拟次数
        self.MCTS.simulations = self.simulations_train if flag_is_train else self.simulations_play

        emptySpacesBoard = board.availables
        move_probs = np.zeros(board.width * board.height)

        if len(emptySpacesBoard) > 0:
            # 运行 MCTS 获取落子概率分布 π
            acts, probs = self.MCTS.getMoveProbs(board, flag_is_train)
            move_probs[list(acts)] = probs

            if flag_is_train:
                # 训练模式：添加 Dirichlet 噪声，鼓励探索
                move = np.random.choice(
                    acts,
                    p=0.75 * probs + 0.25 * np.random.dirichlet(0.3 * np.ones(len(probs)))
                )
                # 复用搜索树
                self.MCTS.updateMCTS(move)
            else:
                # 对战模式：Argmax，选择概率最高的点（绝对贪婪）
                move = acts[np.argmax(probs)]
                # 每步重新搜索
                self.MCTS.updateMCTS(-1)

            return move, move_probs
        else:
            print("WARNING: the board is full")

    def __str__(self):
        return "MCTS"


class NNPlayer():
    """
    纯神经网络 AI 玩家 (不使用 MCTS)
    直接使用策略网络 (Policy Head) 的输出进行落子，速度极快，但棋力较弱
    """
    def __init__(self, policy_NN):
        self.policy_NN = policy_NN

    def resetMCTS(self):
        pass

    def getAction(self, board, flag_is_train=False):
        emptySpacesBoard = board.availables
        move_probs = np.zeros(board.width * board.height)

        if len(emptySpacesBoard) > 0:
            # 直接前向传播
            action_probs, _ = self.policy_NN(board)
            acts, probs = zip(*list(action_probs))
            
            move_probs[list(acts)] = probs

            if flag_is_train:
                 move = np.random.choice(acts, p=probs)
            else:
                # 选择概率最大的动作 (Greedy)
                move = acts[np.argmax(probs)]
            
            return move, move_probs
        else:
            print("WARNING: the board is full")
            return -1, move_probs

    def __str__(self):
        return "PolicyNN"
