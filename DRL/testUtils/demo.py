#!/usr/bin/env python3
"""
演示脚本：展示如何使用训练好的PyTorch模型
"""

import torch
import numpy as np
import os
import sys

# 添加父目录到路径，以便导入主项目模块
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config import Config
from board import Board
from policy_network import PolicyValueNet
from mcts import MCTSPlayer
from game import Game

def load_model(model_path):
    """加载训练好的模型"""
    if not os.path.exists(model_path):
        print(f"模型文件 {model_path} 不存在")
        return None
    
    # 创建网络
    net = PolicyValueNet()
    
    # 加载模型权重
    net.load_model(model_path)
    net.eval()  # 设置为评估模式
    
    return net

def demo_single_prediction():
    """演示单次预测"""
    print("=== 单次预测演示 ===")
    
    # 加载模型
    model_path = "../models/final_policy.pth"
    net = load_model(model_path)
    if net is None:
        return
    
    # 创建棋盘
    board = Board()
    board.init_board()
    
    # 在中心下一子
    board.do_move(28)  # 位置(3,4)
    
    print("当前棋盘状态:")
    print_board(board)
    
    # 获取AI预测
    act_probs, value = net.policy_value_fn(board)
    act_probs = list(act_probs)
    
    print(f"\n局面评估值: {value:.4f}")
    print("推荐落子位置（前5个）:")
    
    # 按概率排序
    act_probs.sort(key=lambda x: x[1], reverse=True)
    for i, (move, prob) in enumerate(act_probs[:5]):
        row, col = move // 8, move % 8
        print(f"  {i+1}. 位置({row},{col}) - 概率: {prob:.4f}")

def demo_self_play():
    """演示自我对弈"""
    print("\n=== 自我对弈演示 ===")
    
    # 加载模型
    model_path = "../models/final_policy.pth"
    net = load_model(model_path)
    if net is None:
        return
    
    # 创建MCTS玩家
    player = MCTSPlayer(
        net.policy_value_fn,
        c_puct=5,
        n_playout=50,  # 减少模拟次数以加快演示
        is_selfplay=1
    )
    
    # 创建游戏
    game = Game()
    
    print("开始自我对弈...")
    winner, play_data = game.start_self_play(player, is_shown=1, temp=0.1)
    
    if winner == -1:
        print("游戏结果: 平局")
    else:
        print(f"游戏结果: 玩家 {winner} 获胜")

def demo_vs_random():
    """演示AI vs 随机玩家"""
    print("\n=== AI vs 随机玩家演示 ===")
    
    # 加载模型
    model_path = "../models/final_policy.pth"
    net = load_model(model_path)
    if net is None:
        return
    
    # 创建AI玩家
    ai_player = MCTSPlayer(
        net.policy_value_fn,
        c_puct=5,
        n_playout=100,
        is_selfplay=0
    )
    
    # 创建随机玩家
    class RandomPlayer:
        def __init__(self):
            self.player = None
        
        def set_player_ind(self, p):
            self.player = p
        
        def get_action(self, board):
            return np.random.choice(board.availables)
    
    random_player = RandomPlayer()
    
    # 创建游戏
    game = Game()
    
    print("AI vs 随机玩家对战...")
    winner = game.start_play(ai_player, random_player, start_player=0, is_shown=1)
    
    if winner == -1:
        print("游戏结果: 平局")
    elif winner == 1:
        print("游戏结果: AI获胜")
    else:
        print("游戏结果: 随机玩家获胜")

def print_board(board):
    """打印棋盘"""
    print("   ", end="")
    for i in range(board.width):
        print(f"{i:2}", end=" ")
    print()
    
    for i in range(board.height):
        print(f"{i:2} ", end="")
        for j in range(board.width):
            pos = i * board.width + j
            if pos in board.states:
                if board.states[pos] == 1:
                    print(" X", end=" ")
                else:
                    print(" O", end=" ")
            else:
                print(" .", end=" ")
        print()

def demo_onnx_model():
    """演示ONNX模型使用"""
    print("\n=== ONNX模型演示 ===")
    
    try:
        import onnxruntime as ort
        
        onnx_path = "../models/final_policy.onnx"
        if not os.path.exists(onnx_path):
            print(f"ONNX模型文件 {onnx_path} 不存在")
            return
        
        # 加载ONNX模型
        session = ort.InferenceSession(onnx_path)
        
        # 创建测试输入
        board = Board()
        board.init_board()
        board.do_move(28)  # 下一子
        
        state = board.current_state()
        input_data = state.reshape(1, 4, 8, 8).astype(np.float32)
        
        # 运行推理
        outputs = session.run(None, {'input': input_data})
        policy, value = outputs
        
        print(f"ONNX模型预测成功!")
        print(f"策略输出形状: {policy.shape}")
        print(f"价值输出形状: {value.shape}")
        print(f"价值预测: {value[0][0]:.4f}")
        
    except ImportError:
        print("ONNX Runtime未安装，跳过ONNX演示")
    except Exception as e:
        print(f"ONNX演示失败: {e}")

def main():
    """主函数"""
    print("=" * 60)
    print("PyTorch AlphaGo Zero 模型演示")
    print("=" * 60)
    
    # 检查模型文件是否存在
    model_path = "../models/final_policy.pth"
    if not os.path.exists(model_path):
        print(f"模型文件 {model_path} 不存在")
        print("请先运行训练脚本生成模型")
        return
    
    try:
        # 演示各种功能
        demo_single_prediction()
        demo_onnx_model()
        
        # 询问是否进行对弈演示
        print("\n是否进行对弈演示？(y/n): ", end="")
        choice = input().lower().strip()
        
        if choice == 'y' or choice == 'yes':
            demo_vs_random()
            
            print("\n是否进行自我对弈演示？(y/n): ", end="")
            choice = input().lower().strip()
            
            if choice == 'y' or choice == 'yes':
                demo_self_play()
        
        print("\n演示完成！")
        
    except KeyboardInterrupt:
        print("\n演示被用户中断")
    except Exception as e:
        print(f"\n演示过程中出现错误: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main() 