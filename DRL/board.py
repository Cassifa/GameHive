import numpy as np
from config import Config

class Board:
    """五子棋棋盘类"""
    
    def __init__(self, width=None, height=None, n_in_row=None):
        self.width = width or Config.BOARD_WIDTH
        self.height = height or Config.BOARD_HEIGHT
        self.n_in_row = n_in_row or Config.N_IN_ROW
        
        # 棋盘状态：0-空位，1-玩家1，2-玩家2
        self.states = {}
        # 可用位置
        self.availables = list(range(self.width * self.height))
        # 当前玩家
        self.current_player = 1
        # 最后一步
        self.last_move = -1
        
    def init_board(self, start_player=1):
        """初始化棋盘"""
        if self.width < self.n_in_row or self.height < self.n_in_row:
            raise Exception('棋盘尺寸太小')
        self.current_player = start_player
        self.availables = list(range(self.width * self.height))
        self.states = {}
        self.last_move = -1
        self.players = [1, 2]  # 添加玩家列表
        
    def move_to_location(self, move):
        """将移动转换为坐标"""
        h = move // self.width
        w = move % self.width
        return [h, w]
    
    def location_to_move(self, location):
        """将坐标转换为移动"""
        if len(location) != 2:
            return -1
        h, w = location
        move = h * self.width + w
        if move not in range(self.width * self.height):
            return -1
        return move
    
    def current_state(self):
        """返回当前棋盘状态的特征表示"""
        square_state = np.zeros((4, self.width, self.height))
        if self.states:
            moves, players = np.array(list(zip(*self.states.items())))
            move_curr = moves[players == self.current_player]
            move_oppo = moves[players != self.current_player]
            
            # 当前玩家的棋子
            square_state[0][move_curr // self.width, move_curr % self.width] = 1.0
            # 对手的棋子
            square_state[1][move_oppo // self.width, move_oppo % self.width] = 1.0
                    # 最后一步
        if self.last_move != -1:
            square_state[2][self.last_move // self.width, self.last_move % self.width] = 1.0
        
        # 当前玩家标识
        if len(self.states) % 2 == 0:
            square_state[3][:, :] = 1.0  # 先手
        
        return square_state[:, ::-1, :].copy()
    
    def do_move(self, move):
        """执行移动"""
        self.states[move] = self.current_player
        self.availables.remove(move)
        self.current_player = 3 - self.current_player  # 切换玩家
        self.last_move = move
        
    def has_a_winner(self):
        """检查是否有获胜者"""
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
            
            # 检查四个方向
            if (self._check_direction(h, w, 1, 0, player, states, n) or  # 垂直
                self._check_direction(h, w, 0, 1, player, states, n) or  # 水平
                self._check_direction(h, w, 1, 1, player, states, n) or  # 对角线
                self._check_direction(h, w, 1, -1, player, states, n)):   # 反对角线
                return True, player
                
        return False, -1
    
    def _check_direction(self, h, w, dh, dw, player, states, n):
        """检查特定方向是否有n个连续的棋子"""
        count = 1
        # 正方向
        for i in range(1, n):
            nh, nw = h + i * dh, w + i * dw
            if (0 <= nh < self.height and 0 <= nw < self.width and
                states.get(nh * self.width + nw) == player):
                count += 1
            else:
                break
        
        # 负方向
        for i in range(1, n):
            nh, nw = h - i * dh, w - i * dw
            if (0 <= nh < self.height and 0 <= nw < self.width and
                states.get(nh * self.width + nw) == player):
                count += 1
            else:
                break
                
        return count >= n
    
    def game_end(self):
        """检查游戏是否结束"""
        win, winner = self.has_a_winner()
        if win:
            return True, winner
        elif not len(self.availables):
            return True, -1  # 平局
        return False, -1
    
    def get_current_player(self):
        """获取当前玩家"""
        return self.current_player 