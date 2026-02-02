from __future__ import print_function
import numpy as np

from .base import BoardBase

class AntigoBoard(BoardBase):
    """
    不围棋(Anti-Go)棋盘
    规则：如果落子导致自己或对方失去"气"则失败。
    """
    def __init__(self, width=9, height=9, n_in_row=0):
        # n_in_row 在此游戏中不用于判断胜负，但父类可能需要。
        # 这里设置为 0 或不使用。
        super().__init__(width, height, n_in_row)

    def initBoard(self, start_player=0):
        super().initBoard(start_player)
        # 必须重置此标记，否则上一局的状态会影响新开局，导致 MCTS 误判游戏立即结束
        if hasattr(self, 'last_move_lost'):
            del self.last_move_lost

    def current_state(self):
        """
        构建当前棋盘的状态特征张量，供神经网络作为输入。
        与五子棋逻辑一致。
        """
        square_state = np.zeros((4, self.width, self.height))
        if self.states:
            moves, players = np.array(list(zip(*self.states.items())))
            move_curr = moves[players == self.current_player]
            move_oppo = moves[players != self.current_player]

            square_state[0][move_curr // self.width, move_curr % self.height] = 1.0 
            square_state[1][move_oppo // self.width, move_oppo % self.height] = 1.0 
            square_state[2][self.last_move // self.width, self.last_move % self.height] = 1.0

        if len(self.states) % 2 == 0:
            square_state[3][:, :] = 1.0 

        return square_state[:, ::-1, :]

    def do_move(self, move):
        """
        执行落子
        注意：在不围棋中，如果落子导致任何一方无气，则当前下棋者判负。
        """
        # 数据增强：随机旋转/翻转棋盘
        # 注意：这通常在训练数据收集阶段（selfPlay）进行，而不是在游戏逻辑（Board）中进行。
        # 这里只负责纯粹的规则逻辑。
        self.states[move] = self.current_player
        self.availables.remove(move)
        self.last_move = move

        # 检查此步是否导致游戏结束（有人无气）
        self.last_move_lost = self.check_loss_by_move(move)

        self.current_player = (
            self.players[0] if self.current_player == self.players[1]
            else self.players[1]
        )

    def has_a_winner(self):
        """
        判断是否比出输赢
        在 do_move 中我们已经计算了 self.last_move_lost
        如果 last_move_lost 为 True，说明刚刚下棋的人输了，也就是现在轮到的这个人（current_player）赢了。
        """
        if hasattr(self, 'last_move_lost') and self.last_move_lost:
            # 刚刚下棋的人输了，所以现在 current_player 是赢家
            return True, self.current_player
        
        return False, -1

    def gameIsOver(self):
        """
        判断游戏是否结束
        """
        win, winner = self.has_a_winner()
        if win:
            return True, winner
        elif not len(self.availables):
            return True, -1 # 平局
        return False, -1

    def check_loss_by_move(self, move):
        """
        检查最后一步落子是否导致任何一方无气
        move: 刚刚落子的位置 (index)
        """
        x = move // self.width
        y = move % self.width # Assuming square board based on implementation details
        
        # 检查顺序：先检查周围4个邻居（可能是对手），再检查自己
        # 如果对手无气 -> 我输
        # 如果自己无气 -> 我输
        # 只要有任何无气的情况发生，当前行动者判负。

        neighbors = self.get_neighbors(x, y)
        
        # 1. 检查邻居的连通块是否有气
        for nx, ny in neighbors:
            n_idx = self.location_to_move(nx, ny)
            if n_idx not in self.states:
                continue
            
            # 只有当邻居有棋子时才检查
            if not self.has_liberty(nx, ny):
                return True # 导致邻居无气，判负
        
        # 2. 检查自己的连通块是否有气
        if not self.has_liberty(x, y):
            return True # 导致自己无气，判负
            
        return False

    def has_liberty(self, x, y):
        """
        检查 (x, y) 所在的连通块是否有气
        使用 DFS 搜索
        """
        color = self.states.get(self.location_to_move(x, y))
        if color is None:
            return True # 空位算有气

        stack = [(x, y)]
        visited = set()
        visited.add((x, y))
        
        while stack:
            cx, cy = stack.pop()
            
            # 检查 cx, cy 的邻居
            neighbors = self.get_neighbors(cx, cy)
            for nx, ny in neighbors:
                n_idx = self.location_to_move(nx, ny)
                
                # 如果有空位 -> 有气 -> True
                if n_idx not in self.states:
                    return True
                
                # 如果是同色棋子且未访问 -> 加入搜索
                if self.states[n_idx] == color and (nx, ny) not in visited:
                    visited.add((nx, ny))
                    stack.append((nx, ny))
        
        # 遍历完整个连通块都没找到气
        return False

    def get_neighbors(self, x, y):
        """获取 (x,y) 的上下左右合法邻居坐标"""
        offsets = [(-1, 0), (1, 0), (0, -1), (0, 1)]
        neighbors = []
        for dx, dy in offsets:
            nx, ny = x + dx, y + dy
            if 0 <= nx < self.width and 0 <= ny < self.height:
                neighbors.append((nx, ny))
        return neighbors

    def location_to_move(self, x, y):
        return x * self.width + y
