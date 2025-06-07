import numpy as np
import copy
from tree_node import TreeNode
from config import Config

def softmax(x):
    """Softmax函数"""
    probs = np.exp(x - np.max(x))
    probs /= np.sum(probs)
    return probs

class MCTS:
    """蒙特卡洛树搜索"""
    
    def __init__(self, policy_value_fn, c_puct=None, n_playout=None):
        """
        Args:
            policy_value_fn: 策略价值函数
            c_puct: UCB公式中的探索常数
            n_playout: 每次搜索的模拟次数
        """
        self._root = TreeNode(None, 1.0)
        self._policy = policy_value_fn
        self._c_puct = c_puct or Config.C_PUCT
        self._n_playout = n_playout or Config.MCTS_SIMULATIONS
    
    def _playout(self, state):
        """执行一次模拟"""
        node = self._root
        
        while True:
            if node.is_leaf():
                break
            # 贪婪选择下一个动作
            action, node = node.select(self._c_puct)
            state.do_move(action)
        
        # 使用神经网络评估叶节点
        action_probs, leaf_value = self._policy(state)
        
        # 检查游戏是否结束
        end, winner = state.game_end()
        if not end:
            node.expand(action_probs)
        else:
            # 游戏结束，根据结果设置叶节点值
            if winner == -1:  # 平局
                leaf_value = 0.0
            else:
                leaf_value = (1.0 if winner == state.get_current_player() else -1.0)
        
        # 反向传播更新节点值
        node.update_recursive(-leaf_value)
    
    def get_move_probs(self, state, temp=1e-3):
        """
        获取所有可能动作的概率分布
        
        Args:
            state: 当前游戏状态
            temp: 温度参数，控制探索程度
        
        Returns:
            acts: 动作列表
            probs: 对应的概率分布
        """
        # 执行n_playout次模拟
        for _ in range(self._n_playout):
            state_copy = copy.deepcopy(state)
            self._playout(state_copy)
        
        # 根据访问次数计算动作概率
        act_visits = [(act, node.n_visits) 
                     for act, node in self._root.children.items()]
        acts, visits = zip(*act_visits)
        act_probs = softmax(1.0/temp * np.log(np.array(visits) + 1e-10))
        
        return acts, act_probs
    
    def update_with_move(self, last_move):
        """
        更新根节点到对应的子节点
        
        Args:
            last_move: 上一步的动作
        """
        if last_move in self._root.children:
            self._root = self._root.children[last_move]
            self._root._parent = None
        else:
            self._root = TreeNode(None, 1.0)
    
    def __str__(self):
        return "MCTS"

class MCTSPlayer:
    """基于MCTS的AI玩家"""
    
    def __init__(self, policy_value_function, c_puct=None, n_playout=None, is_selfplay=0):
        self.mcts = MCTS(policy_value_function, c_puct, n_playout)
        self._is_selfplay = is_selfplay
    
    def set_player_ind(self, p):
        """设置玩家ID"""
        self.player = p
    
    def reset_player(self):
        """重置MCTS"""
        self.mcts.update_with_move(-1)
    
    def get_action(self, board, temp=1e-3, return_prob=0):
        """
        获取下一步动作
        
        Args:
            board: 当前棋盘状态
            temp: 温度参数
            return_prob: 是否返回概率分布
        
        Returns:
            action: 选择的动作
            move_probs: 动作概率分布（如果return_prob=1）
        """
        sensible_moves = board.availables
        move_probs = np.zeros(board.width * board.height)
        
        if len(sensible_moves) > 0:
            acts, probs = self.mcts.get_move_probs(board, temp)
            move_probs[list(acts)] = probs
            
            if self._is_selfplay:
                # 自我对弈时添加狄利克雷噪声
                move = np.random.choice(acts, p=0.75*probs + 0.25*np.random.dirichlet(0.3*np.ones(len(probs))))
                # 更新根节点
                self.mcts.update_with_move(move)
            else:
                # 对弈时选择概率最大的动作
                move = np.random.choice(acts, p=probs)
                # 保留搜索树，提高效率
                self.mcts.update_with_move(move)
            
            if return_prob:
                return move, move_probs
            else:
                return move
        else:
            print("WARNING: 棋盘已满")
    
    def __str__(self):
        return "MCTS {}".format(self.player) 