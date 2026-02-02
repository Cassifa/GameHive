try:
    from tkinter import END
except ImportError:
    # 服务器环境下没有 tkinter，定义一个伪常量
    END = 'end'
import numpy as np
from games.antigo import AntigoBoard

class Game():
    """
    游戏控制器类
    负责管理棋盘状态、绘制界面、处理用户输入以及控制游戏流程 (自我对弈/人机对战)
    """
    boardWidth = 8
    boardHeight = 8
    n_in_row = 5
    flag_human_click = False  # 标记人类是否点击了棋盘
    move_human = -1  # 记录人类点击的位置

    def __init__(self, Canvas, scrollText, board, flag_is_shown=True, flag_is_train=True):
        self.flag_is_shown = flag_is_shown
        self.flag_is_train = flag_is_train
        
        self.board = board
        self.boardWidth = board.width
        self.boardHeight = board.height
        
        self.n_in_row = getattr(board, 'n_in_row', None)
        
        self.Canvas = Canvas
        self.scrollText = scrollText
        self.rect = None

    def Show(self, board, KEY=False):
        """
        在 Canvas 上绘制落子，并在日志框显示信息
        """
        x = board.last_move // board.width
        y = board.last_move % board.height
        # 绘制棋子
        # board.last_move 是"上一手"落子的位置。
        # 如果当前轮到玩家 1 (黑)，说明上一手是玩家 2 (白) 下的。
        # 如果当前轮到玩家 2 (白)，说明上一手是玩家 1 (黑) 下的。
        # 所以这里的 player 参数应该传入“刚刚下这步棋的玩家”
        last_player = 1 if board.current_player == 2 else 2
        self.drawPieces(player=last_player, rc_pos=(x, y), Index=len(board.states))

        if KEY:
            # 构造日志信息
            if self.flag_is_train == False:
                playerName = 'you'
                if board.current_player != 1:
                    playerName = 'AI'
            else:
                playerName = 'AI-' + str(board.current_player)

            self.drawText(str(len(board.states)) + ' ' + playerName + ':' + str(x) + ' ' + str(y))

    def drawText(self, string):
        """向滚动文本框添加一行日志"""
        self.scrollText.insert(END, string + '\n')
        self.scrollText.see(END)
        self.scrollText.update()

    def drawPieces(self, player, rc_pos, Index, RADIUS=15, draw_rect=True):
        """
        在 Canvas 上画一个棋子
        :param player: 当前玩家 ID (1 或 2)
        :param rc_pos: (row, col) 坐标
        :param Index: 手数 (显示在棋子上)
        """
        x, y = self.convert_rc_to_xy(rc_pos)
        
        # 修正颜色逻辑：Player 1 (先手) 黑棋，Player 2 (后手) 白棋
        # 注意：这里传入的 player 是 *刚刚落子* 的玩家，也就是 board.current_player 的 *上一个* 状态
        # 但由于 board.current_player 在 do_move 后已经切换了，所以这里传入的其实是 *当前等待落子* 的玩家？
        # 不，调用处是 Show(board)，此时 board.current_player 已经是 *下一个* 玩家了。
        # 实际上，Show 函数里画的是 board.last_move，也就是 *上一个* 玩家下的棋。
        # 上一个玩家 = current_player 的对手。
        # 如果 current_player 是 1 (黑)，那上一步是 2 (白) 下的。
        # 如果 current_player 是 2 (白)，那上一步是 1 (黑) 下的。
        
        # 让我们重新理一下逻辑：
        # Game.py 的 Show 函数传入的是 board.current_player。
        # 在 do_move 之后调用 Show。
        # do_move 之后，current_player 已经切换为 *下一手* 的玩家。
        # 所以 Show 函数里传入的 player 是 *下一手* 的玩家。
        # 但是我们画的是 last_move，也就是 *上一手* 的棋子。
        # 所以棋子的颜色应该是 player 的 *对手* 的颜色。
        
        # 如果传入 player=1 (当前轮到黑棋)，说明上一手是白棋(2)下的 -> 画白棋
        # 如果传入 player=2 (当前轮到白棋)，说明上一手是黑棋(1)下的 -> 画黑棋
        
        colorText = 'white' if player == 1 else 'black'
        colorPiece = 'black' if player == 1 else 'white'
        
        self.Canvas.create_oval(x - RADIUS, y - RADIUS, x + RADIUS, y + RADIUS, fill=colorPiece, outline=colorPiece)
        
        # 绘制红色方框标记最新落子
        if draw_rect == True:
            if self.rect == None:
                OFFSET = 20
                self.rect = self.Canvas.create_rectangle(x - OFFSET, y - OFFSET, x + OFFSET, y + OFFSET,
                                                         outline="#c1005d")
                self.rect_xy_pos = (x, y)
            else:
                rc_pos = self.convert_xy_to_rc((x, y))
                old_x, old_y = self.rect_xy_pos
                new_x, new_y = self.convert_rc_to_xy(rc_pos)
                dx, dy = new_x - old_x, new_y - old_y
                self.Canvas.move(self.rect, dx, dy)
                self.rect_xy_pos = (new_x, new_y)
        
        # 在棋子上写手数
        self.Canvas.create_text(x, y, text=str(Index), fill=colorText, )
        self.Canvas.update()

    def convert_rc_to_xy(self, rc_pos):
        """将行列坐标 (row, col) 转换为 Canvas 像素坐标 (x, y)"""
        SIDE = (435 - 400) / 2
        DELTA = (400 - 2) / (self.boardWidth - 1) if self.boardWidth > 1 else 400
        
        # 井字棋修正：棋子显示在格子中心
        # 不围棋也使用格子画法
        if self.boardWidth == 3 or isinstance(self.board, AntigoBoard):
            cell_size = 400 / self.boardWidth
            r, c = rc_pos
            x = SIDE + c * cell_size + cell_size / 2
            y = SIDE + r * cell_size + cell_size / 2
        else:
            # 五子棋等：棋子在交叉点上
            r, c = rc_pos
            x = c * DELTA + SIDE
            y = r * DELTA + SIDE
            
        return x, y

    def convert_xy_to_rc(self, xy_pos):
        """将 Canvas 像素坐标 (x, y) 转换为行列坐标 (row, col)"""
        SIDE = (435 - 400) / 2
        DELTA = (400 - 2) / (self.boardWidth - 1) if self.boardWidth > 1 else 400
        x, y = xy_pos
        
        if self.boardWidth == 3 or isinstance(self.board, AntigoBoard):
            cell_size = 400 / self.boardWidth
            c = int((x - SIDE) / cell_size)
            r = int((y - SIDE) / cell_size)
            c = max(0, min(self.boardWidth - 1, c))
            r = max(0, min(self.boardHeight - 1, r))
        else:
            r = round((y - SIDE) / DELTA)
            c = round((x - SIDE) / DELTA)
            
        return r, c

    def selfPlay(self, player, Index=0, check_stop=None):
        """ 
        [自我对弈循环]
        AI vs AI，生成训练数据
        :param player: AI 玩家对象
        :param Index: 当前对局编号
        :param check_stop: 回调函数，用于检查是否需要中断训练
        :return: winner, play_data (zip(states, mcts_probs, winners_z))
        """

        self.board.initBoard()
        boards, probs, currentPlayer = [], [], []

        while True:
            # 检查外部中断信号 (例如用户点击了重置)
            if check_stop and check_stop():
                return -1, []

            # 获取 AI 动作 (MCTS)
            move, move_probs = player.getAction(self.board, self.flag_is_train)

            # 收集数据：当前状态、MCTS概率、当前玩家
            boards.append(self.board.current_state())
            probs.append(move_probs)
            currentPlayer.append(self.board.current_player)

            # 执行落子
            self.board.do_move(move)

            # 刷新界面
            if self.flag_is_shown:
                self.Show(self.board)

            # 检查游戏结束
            gameOver, winner = self.board.gameIsOver()

            if gameOver:
                # 构造训练标签 Z (胜负结果)
                # 赢家视角的 Z 为 +1，输家为 -1
                winners_z = np.zeros(len(currentPlayer))
                if winner != -1:
                    winners_z[np.array(currentPlayer) == winner] = 1.0
                    winners_z[np.array(currentPlayer) != winner] = -1.0

                # 重置 MCTS 树
                player.resetMCTS()

                if self.flag_is_shown:
                    if winner != -1:
                        playerName = 'AI-' + str(self.board.current_player) # 这里的 current_player 是输家
                        # 修正显示逻辑：winner 是赢家的 ID
                        win_name = 'AI-1' if winner == 1 else 'AI-2'
                        self.drawText("Game end. Winner is : " + str(win_name))
                    else:
                        self.drawText("Game end. Tie")

                self.rect = None
                return winner, zip(boards, probs, winners_z)

    def humanMove(self, event):
        """处理鼠标点击事件"""
        self.flag_human_click = True
        x, y = event.x, event.y
        r, c = self.convert_xy_to_rc((x, y))
        self.move_human = r * self.boardWidth + c

    def playWithHuman(self, player, human_player_id=2):
        """ 
        [人机对战循环]
        Human vs AI
        :param player: AI 玩家对象
        :param human_player_id: 人类玩家执子 (1=黑棋/先手, 2=白棋/后手)
        """
        self.Canvas.bind("<Button-1>", self.humanMove)
        self.board.initBoard(0) # 0 表示玩家1 (黑棋) 先手

        KEY = 0
        while True:
            current_p = self.board.current_player
            
            if current_p != human_player_id:
                # AI 行动
                move, move_probs = player.getAction(self.board, self.flag_is_train)
                self.board.do_move(move)
                KEY = 1
            else:
                # 人类行动
                if self.flag_human_click:
                    if self.move_human in self.board.availables:
                        self.flag_human_click = False
                        self.board.do_move(self.move_human)
                        KEY = 1
                    else:
                        self.flag_human_click = False
                        print("无效区域")

            if self.flag_is_shown and KEY == 1:
                self.Show(self.board)
                KEY = 0

            gameOver, winner = self.board.gameIsOver()
            if gameOver:
                player.resetMCTS()
                if self.flag_is_shown:
                    if winner != -1:
                        win_name = 'Human' if winner == human_player_id else 'AI'
                        self.drawText("Game end. Winner is : " + str(win_name))
                    else:
                        self.drawText("Game end. Tie")
                break
        self.rect = None
