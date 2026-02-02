from .base import BoardBase

class TicTacToeBoard(BoardBase):
    """
    井字棋棋盘 (3x3, 3连珠)
    """
    def __init__(self):
        super().__init__(width=3, height=3, n_in_row=3)

    def current_state(self):
        """
        构建井字棋的状态特征张量。
        逻辑与五子棋完全一致，只是尺寸不同 (3x3)。
        """
        import numpy as np
        square_state = np.zeros((4, self.width, self.height))
        if self.states:
            moves, players = np.array(list(zip(*self.states.items())))
            
            # 分离己方 (move_curr) 和 敌方 (move_oppo)
            # 在 MCTS 搜索中，如果我们在评估一个"我想下在这里"的子节点，
            # 那么该子节点的状态是轮到"对手"下棋。
            # 此时 move_curr 指的是"对手"(因为轮到他了)，move_oppo 指的是"我"(刚下完)。
            move_curr = moves[players == self.current_player]
            move_oppo = moves[players != self.current_player]
            
            # Channel 0: 当前行动方的棋子
            square_state[0][move_curr // self.width, move_curr % self.height] = 1.0
            # Channel 1: 非当前行动方(刚刚行动完)的棋子
            square_state[1][move_oppo // self.width, move_oppo % self.height] = 1.0
            # Channel 2: 刚刚落下的那颗子(Last Move)
            square_state[2][self.last_move // self.width, self.last_move % self.height] = 1.0

        # Channel 3: 标记当前是先手(1)还是后手(0)的回合
        if len(self.states) % 2 == 0:
            square_state[3][:, :] = 1.0

        return square_state[:, ::-1, :]

    def has_a_winner(self):
        width = self.width
        height = self.height
        states = self.states
        n = self.n_in_row

        moved = list(set(range(width * height)) - set(self.availables))
        if len(moved) < self.n_in_row * 2 - 1:
            return False, -1

        for m in moved:
            h = m // width
            w = m % width
            player = states[m]

            if (w in range(width - n + 1) and
                    len(set(states.get(i, -1) for i in range(m, m + n))) == 1):
                return True, player

            if (h in range(height - n + 1) and
                    len(set(states.get(i, -1) for i in range(m, m + n * width, width))) == 1):
                return True, player

            if (w in range(width - n + 1) and h in range(height - n + 1) and
                    len(set(states.get(i, -1) for i in range(m, m + n * (width + 1), width + 1))) == 1):
                return True, player

            if (w in range(n - 1, width) and h in range(height - n + 1) and
                    len(set(states.get(i, -1) for i in range(m, m + n * (width - 1), width - 1))) == 1):
                return True, player

        return False, -1
