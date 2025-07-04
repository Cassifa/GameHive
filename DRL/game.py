import numpy as np
from board import Board
from config import Config

class Game:
    """五子棋游戏类"""
    
    def __init__(self, board=None):
        self.board = board or Board()
    
    def graphic(self, board, player1, player2):
        """打印棋盘状态"""
        width = board.width
        height = board.height
        
        print("Player", player1, "with X".rjust(3))
        print("Player", player2, "with O".rjust(3))
        print()
        
        for x in range(width):
            print("{0:8}".format(x), end='')
        print('\r\n')
        
        for i in range(height - 1, -1, -1):
            print("{0:4d}".format(i), end='')
            for j in range(width):
                loc = i * width + j
                p = board.states.get(loc, 0)
                if p == player1:
                    print('X'.center(8), end='')
                elif p == player2:
                    print('O'.center(8), end='')
                else:
                    print('_'.center(8), end='')
            print('\r\n\r\n')
    
    def start_play(self, player1, player2, start_player=0, is_shown=1):
        """
        开始游戏
        
        Args:
            player1: 玩家1
            player2: 玩家2  
            start_player: 先手玩家 (0 or 1)
            is_shown: 是否显示棋盘
        
        Returns:
            winner: 获胜者
        """
        if start_player not in (0, 1):
            raise Exception('start_player should be either 0 (player1 first) '
                          'or 1 (player2 first)')
        
        self.board.init_board(start_player + 1)  # 转换为1,2
        p1, p2 = self.board.players
        player1.set_player_ind(p1)
        player2.set_player_ind(p2)
        players = {p1: player1, p2: player2}
        
        if is_shown:
            self.graphic(self.board, player1.player, player2.player)
        
        while True:
            current_player = self.board.get_current_player()
            player_in_turn = players[current_player]
            move = player_in_turn.get_action(self.board)
            self.board.do_move(move)
            
            if is_shown:
                self.graphic(self.board, player1.player, player2.player)
            
            end, winner = self.board.game_end()
            if end:
                if is_shown:
                    if winner != -1:
                        print("Game end. Winner is", players[winner])
                    else:
                        print("Game end. Tie")
                return winner
    
    def start_self_play(self, player, is_shown=0, temp=1.0):
        """
        自我对弈
        
        Args:
            player: MCTS玩家
            is_shown: 是否显示过程
            temp: 温度参数
        
        Returns:
            winner: 获胜者
            zip(states, mcts_probs, winners): 训练数据
        """
        self.board.init_board()
        p1, p2 = self.board.players
        states, mcts_probs, current_players = [], [], []
        
        while True:
            move, move_probs = player.get_action(self.board, temp=temp, return_prob=1)
            
            # 存储数据
            states.append(self.board.current_state())
            mcts_probs.append(move_probs)
            current_players.append(self.board.current_player)
            
            # 执行移动
            self.board.do_move(move)
            
            if is_shown:
                self.graphic(self.board, p1, p2)
            
            end, winner = self.board.game_end()
            if end:
                # 从每个状态的角度设置获胜者
                winners_z = np.zeros(len(current_players))
                if winner != -1:
                    winners_z[np.array(current_players) == winner] = 1.0
                    winners_z[np.array(current_players) != winner] = -1.0
                # 重置MCTS根节点，为下一局准备
                player.reset_player()
                
                if is_shown:
                    if winner != -1:
                        print("Game end. Winner is player:", winner)
                    else:
                        print("Game end. Tie")
                
                return winner, zip(states, mcts_probs, winners_z) 