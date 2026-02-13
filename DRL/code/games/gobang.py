from __future__ import print_function
import numpy as np

from .base import BoardBase

class GobangBoard(BoardBase):
    """
    五子棋棋盘
    """
    def __init__(self, width=8, height=8, n_in_row=5):
        super().__init__(width, height, n_in_row)


    def initBoard(self, start_player=0):
        super().initBoard(start_player)

    def current_state(self):
        """
        构建当前棋盘的状态特征张量，供神经网络作为输入。
        返回形状为 [4, Width, Height] 的 3D 张量。
        
        关键概念：【相对视角】(Canonical State)
        神经网络不区分"我是黑棋"还是"我是白棋"，它只区分"我是当前要下棋的一方 (Self)" 和 "我的对手 (Opponent)"。
        这样做的好处是模型可以同时适用于先手和后手，无需训练两个模型。
        
        四个特征平面 (Channels) 的含义：
        1. [Channel 0] Self: 当前玩家的棋子位置。
        2. [Channel 1] Opponent: 对手玩家的棋子位置。
        3. [Channel 2] Last Move: 上一步落子的位置。
           - 在交互式对战中，这是真正的对手刚刚下的棋。
           - 在 MCTS 模拟中，如果我们模拟了"自己"下了一步棋到达当前状态，那么对于当前状态（轮到对方下）来说，
             "上一步"就是"自己"刚刚下的那一步。
           - 无论哪种情况，相对当前视角的玩家而言，Last Move 总是"对方"（刚刚行动完的人）留下的。
        4. [Channel 3] Color: 颜色标记。用于让 AI 区分目前是先手局还是后手局（因为五子棋先手有优势，策略不同）。
        """
        # 使用 4*W*H 存储棋盘的状态
        square_state = np.zeros((4, self.width, self.height))
        if self.states:
            moves, players = np.array(list(zip(*self.states.items())))
            # moves 数组: 记录了棋盘上所有已落子的位置索引
            # players 数组: 记录了对应位置是谁下的 (1或2)

            # 区分当前视角下的 "己方" 和 "敌方"
            # self.current_player 是当前等待落子的一方
            move_curr = moves[players == self.current_player] # 属于当前玩家的落子
            move_oppo = moves[players != self.current_player] # 属于对手玩家的落子

            # 填充特征平面
            # Channel 0: 己方棋子分布
            square_state[0][move_curr // self.width, move_curr % self.height] = 1.0 
            # Channel 1: 敌方棋子分布
            square_state[1][move_oppo // self.width, move_oppo % self.height] = 1.0 

            # Channel 2: 上一步落子位置
            # 这个特征非常重要，因为它是局面的"动量"。它告诉 AI 哪里是战场的最新焦点。
            # 在 MCTS 搜索树中，这代表了父节点采取的动作。
            square_state[2][self.last_move // self.width, self.last_move % self.height] = 1.0

        # Channel 3: 颜色/轮次标记
        # 这是一个全图常量平面。
        # 偶数步 (len(states)%2==0) 意味着当前是初始局面或双方各下了N子 -> 轮到黑棋(先手)
        # 奇数步 -> 轮到白棋(后手)
        # 这个平面帮助网络了解自己是执黑还是执白，从而做出激进或防守的宏观决策。
        if len(self.states) % 2 == 0:
            square_state[3][:, :] = 1.0 

        # [:, ::-1, :] 对高度方向进行翻转
        # 这是一个实现细节，可能是为了匹配训练数据或 UI 坐标系的 y 轴方向 (0在下还是在上)
        return square_state[:, ::-1, :]

    def do_move(self, move):
        super().do_move(move)
        # 大棋盘启发式剪枝：只保留有子区域附近的空位，减少 MCTS 无效分支
        if self.width >= 10 and len(self.states) > 0:
            self._prune_availables(distance=3)

    def _prune_availables(self, distance=3):
        """
        启发式剪枝：将 availables 限制为已有棋子周围 distance 格内的空位。
        每次落子后从全部空位重新计算，确保不会遗漏随对局发展新进入范围的位置。
        """
        if not self.states:
            return

        width, height = self.width, self.height

        # 从棋盘总位置减去已落子位置，得到全部空位
        occupied = set(self.states.keys())
        full_empty = set(range(width * height)) - occupied

        # 收集所有棋子周围 distance 格内的空位
        nearby = set()
        for m in occupied:
            r, c = m // width, m % width
            for i in range(max(0, r - distance), min(height, r + distance + 1)):
                for j in range(max(0, c - distance), min(width, c + distance + 1)):
                    pos = i * width + j
                    if pos in full_empty:
                        nearby.add(pos)

        self.availables = sorted(nearby)

    def has_a_winner(self):
        """
        检查是否有玩家达成 n_in_row 连珠获胜。
        遍历所有已落子位置，检查横、竖、两条对角线四个方向。
        """
        width = self.width
        height = self.height
        states = self.states
        n = self.n_in_row

        moved = list(self.states.keys())

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

    def gameIsOver(self):
        """
        判断游戏是否结束。
        胜负由 has_a_winner 决定；平局通过已落子数是否填满棋盘来判断。
        """
        win, winner = self.has_a_winner()
        if win:
            return True, winner
        if len(self.states) >= self.width * self.height:
            return True, -1  # 棋盘满了，平局
        return False, -1

    def getCurrentPlayer(self):
        return self.current_player
