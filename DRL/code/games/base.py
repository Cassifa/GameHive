class BoardBase(object):
    """
    棋盘逻辑基类
    """
    def __init__(self, width, height, n_in_row):
        self.width = width
        self.height = height
        self.states = {}  # key: location, value: player
        self.n_in_row = n_in_row
        self.players = [1, 2]
        self.current_player = 1
        self.availables = []
        self.last_move = -1

    def initBoard(self, start_player=0):
        if self.width < self.n_in_row or self.height < self.n_in_row:
            raise Exception('Board width and height can not be less than {}'.format(self.n_in_row))
        self.current_player = self.players[start_player]
        self.availables = list(range(self.width * self.height))
        self.states = {}
        self.last_move = -1

    def current_state(self):
        """返回当前棋盘状态，供神经网络使用"""
        raise NotImplementedError

    def do_move(self, move):
        self.states[move] = self.current_player
        self.availables.remove(move)
        self.current_player = (
            self.players[0] if self.current_player == self.players[1]
            else self.players[1]
        )
        self.last_move = move

    def has_a_winner(self):
        raise NotImplementedError

    def gameIsOver(self):
        win, winner = self.has_a_winner()
        if win:
            return True, winner
        elif not len(self.availables):
            return True, -1
        return False, -1

    def getCurrentPlayer(self):
        return self.current_player
