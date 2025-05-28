#!/usr/bin/env python3
"""
测试脚本：验证PyTorch AlphaGo Zero实现是否正常工作
"""

import torch
import numpy as np
import sys
import os

# 添加父目录到路径，以便导入主项目模块
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config import Config
from board import Board
from policy_network import PolicyValueNet
from mcts import MCTSPlayer
from game import Game

def test_board():
    """测试棋盘功能"""
    print("测试棋盘功能...")
    board = Board()
    board.init_board()
    
    # 测试基本操作
    assert len(board.availables) == 64
    assert board.current_player == 1
    
    # 测试落子
    board.do_move(28)  # 中心位置
    assert 28 not in board.availables
    assert board.current_player == 2
    
    # 测试状态表示
    state = board.current_state()
    assert state.shape == (4, 8, 8)
    
    print("✓ 棋盘功能测试通过")

def test_network():
    """测试神经网络"""
    print("测试神经网络...")
    
    # 创建网络
    net = PolicyValueNet()
    
    # 测试前向传播
    dummy_input = torch.randn(1, 4, 8, 8)
    policy, value = net(dummy_input)
    
    assert policy.shape == (1, 64)
    assert value.shape == (1, 1)
    assert torch.allclose(policy.sum(dim=1), torch.ones(1), atol=1e-6)  # 概率和为1
    assert -1 <= value.item() <= 1  # 价值在[-1,1]范围内
    
    print("✓ 神经网络测试通过")

def test_mcts():
    """测试MCTS"""
    print("测试MCTS...")
    
    board = Board()
    board.init_board()
    
    # 创建简单的策略函数
    def simple_policy(board):
        n = len(board.availables)
        probs = np.ones(n) / n
        return zip(board.availables, probs), 0.0
    
    # 创建MCTS玩家
    player = MCTSPlayer(simple_policy, n_playout=10, is_selfplay=1)
    
    # 测试获取动作
    move, move_probs = player.get_action(board, return_prob=1)
    
    assert move in board.availables
    assert len(move_probs) == 64
    assert abs(move_probs.sum() - 1.0) < 1e-6
    
    print("✓ MCTS测试通过")

def test_game():
    """测试游戏逻辑"""
    print("测试游戏逻辑...")
    
    game = Game()
    
    # 创建简单策略
    def simple_policy(board):
        n = len(board.availables)
        probs = np.ones(n) / n
        return zip(board.availables, probs), 0.0
    
    player = MCTSPlayer(simple_policy, n_playout=5, is_selfplay=1)
    
    # 测试自我对弈（限制步数避免无限循环）
    game.board.init_board()
    states, mcts_probs, current_players = [], [], []
    
    for _ in range(5):  # 只测试5步
        if game.board.game_end()[0]:
            break
            
        move, move_probs = player.get_action(game.board, temp=1.0, return_prob=1)
        states.append(game.board.current_state())
        mcts_probs.append(move_probs)
        current_players.append(game.board.current_player)
        game.board.do_move(move)
    
    assert len(states) > 0
    assert len(mcts_probs) > 0
    assert len(current_players) > 0
    
    print("✓ 游戏逻辑测试通过")

def test_integration():
    """集成测试"""
    print("测试完整集成...")
    
    # 测试网络与MCTS的集成
    net = PolicyValueNet()
    board = Board()
    board.init_board()
    
    # 测试策略价值函数
    act_probs, value = net.policy_value_fn(board)
    act_probs = list(act_probs)
    
    assert len(act_probs) == len(board.availables)
    assert -1 <= value <= 1
    
    # 测试MCTS与网络的集成
    player = MCTSPlayer(net.policy_value_fn, n_playout=5, is_selfplay=1)
    move, move_probs = player.get_action(board, return_prob=1)
    
    assert move in board.availables
    assert len(move_probs) == 64
    
    print("✓ 集成测试通过")

def main():
    """运行所有测试"""
    print("=" * 50)
    print("PyTorch AlphaGo Zero 实现测试")
    print("=" * 50)
    
    try:
        test_board()
        test_network()
        test_mcts()
        test_game()
        test_integration()
        
        print("\n" + "=" * 50)
        print("🎉 所有测试通过！代码可以正常运行。")
        print("=" * 50)
        
        # 显示设备信息
        print(f"\n设备信息:")
        print(f"  PyTorch版本: {torch.__version__}")
        print(f"  CUDA可用: {torch.cuda.is_available()}")
        if torch.cuda.is_available():
            print(f"  GPU设备: {torch.cuda.get_device_name()}")
        print(f"  当前设备: {Config.DEVICE}")
        
        print(f"\n网络参数:")
        net = PolicyValueNet()
        total_params = sum(p.numel() for p in net.parameters())
        trainable_params = sum(p.numel() for p in net.parameters() if p.requires_grad)
        print(f"  总参数数: {total_params:,}")
        print(f"  可训练参数: {trainable_params:,}")
        
    except Exception as e:
        print(f"\n❌ 测试失败: {e}")
        import traceback
        traceback.print_exc()
        return False
    
    return True

if __name__ == "__main__":
    main() 