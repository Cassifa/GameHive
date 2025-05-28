import numpy as np
import copy

class TreeNode:
    """MCTS树节点"""
    
    def __init__(self, parent, prior_p):
        self._parent = parent
        self._children = {}  # 子节点字典 {action: TreeNode}
        self._n_visits = 0   # 访问次数
        self._Q = 0          # 平均动作价值
        self._u = 0          # UCB值
        self._P = prior_p    # 先验概率
        
    def expand(self, action_priors):
        """扩展节点，添加子节点"""
        for action, prob in action_priors:
            if action not in self._children:
                self._children[action] = TreeNode(self, prob)
    
    def select(self, c_puct):
        """选择UCB值最大的子节点"""
        return max(self._children.items(),
                  key=lambda act_node: act_node[1].get_value(c_puct))
    
    def update(self, leaf_value):
        """更新节点值"""
        # 更新访问次数
        self._n_visits += 1
        # 更新Q值：Q = (Q * (n-1) + v) / n
        self._Q += 1.0 * (leaf_value - self._Q) / self._n_visits
    
    def update_recursive(self, leaf_value):
        """递归更新父节点"""
        # 如果不是根节点，先更新父节点
        if self._parent:
            self._parent.update_recursive(-leaf_value)
        # 更新当前节点
        self.update(leaf_value)
    
    def get_value(self, c_puct):
        """计算UCB值"""
        self._u = (c_puct * self._P * 
                  np.sqrt(self._parent._n_visits) / (1 + self._n_visits))
        return self._Q + self._u
    
    def is_leaf(self):
        """判断是否为叶节点"""
        return self._children == {}
    
    def is_root(self):
        """判断是否为根节点"""
        return self._parent is None
    
    @property
    def children(self):
        return self._children
    
    @property
    def n_visits(self):
        return self._n_visits
    
    @property
    def Q(self):
        return self._Q
    
    @property
    def P(self):
        return self._P 