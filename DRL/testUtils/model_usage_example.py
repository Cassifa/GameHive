#!/usr/bin/env python3
"""
模型使用示例：详细展示如何使用训练好的模型
"""

import torch
import numpy as np
import sys
import os

# 添加父目录到路径，以便导入主项目模块
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from policy_network import PolicyValueNet
from board import Board
from config import Config

def demonstrate_model_usage():
    """演示模型的详细使用方法"""
    
    print("=" * 60)
    print("AlphaGo Zero 模型使用详细示例")
    print("=" * 60)
    
    # 1. 加载训练好的模型
    print("\n1. 加载模型")
    print("-" * 30)
    
    model = PolicyValueNet()
    model.load_model("../models/final_policy.pth")
    model.eval()  # 设置为评估模式
    
    print(f"模型架构:")
    print(f"  - 输入通道: {model.input_channels}")
    print(f"  - 棋盘尺寸: {model.board_width}x{model.board_height}")
    print(f"  - 隐藏层滤波器: {model.hidden_filters}")
    print(f"  - 残差层数: {model.num_res_layers}")
    print(f"  - 总参数数: {sum(p.numel() for p in model.parameters()):,}")
    
    # 2. 准备输入数据
    print("\n2. 输入数据格式")
    print("-" * 30)
    
    # 创建一个示例棋盘状态
    board = Board()
    board.init_board()
    
    # 下几步棋作为示例
    moves = [28, 29, 36, 37]  # 中心区域的几步
    for i, move in enumerate(moves):
        board.do_move(move)
        print(f"第{i+1}步: 位置{move} -> 坐标({move//8}, {move%8})")
    
    # 获取当前棋盘状态
    state = board.current_state()
    print(f"\n棋盘状态张量形状: {state.shape}")
    print(f"数据类型: {state.dtype}")
    print(f"数值范围: [{state.min():.1f}, {state.max():.1f}]")
    
    print(f"\n4个通道的含义:")
    print(f"  - 通道0: 当前玩家的棋子位置 (1表示有棋子，0表示无)")
    print(f"  - 通道1: 对手玩家的棋子位置 (1表示有棋子，0表示无)")
    print(f"  - 通道2: 最后一步落子位置 (1表示最后一步，0表示其他)")
    print(f"  - 通道3: 当前玩家标识 (全1表示先手，全0表示后手)")
    
    # 3. 模型推理
    print("\n3. 模型推理")
    print("-" * 30)
    
    # 方法1: 直接使用forward方法
    print("方法1: 直接forward推理")
    input_tensor = torch.FloatTensor(state).unsqueeze(0)  # 添加batch维度
    print(f"输入张量形状: {input_tensor.shape}")
    
    with torch.no_grad():
        policy_output, value_output = model(input_tensor)
    
    print(f"策略输出形状: {policy_output.shape}")
    print(f"价值输出形状: {value_output.shape}")
    print(f"策略概率和: {policy_output.sum().item():.6f}")
    print(f"价值预测: {value_output.item():.4f}")
    
    # 方法2: 使用policy_value_fn方法（推荐）
    print("\n方法2: 使用policy_value_fn方法（推荐）")
    act_probs, value = model.policy_value_fn(board)
    act_probs = list(act_probs)
    
    print(f"可用位置数量: {len(board.availables)}")
    print(f"动作概率对数量: {len(act_probs)}")
    print(f"价值评估: {value:.4f}")
    
    # 4. 输出解析
    print("\n4. 输出解析")
    print("-" * 30)
    
    # 解析策略输出
    print("策略输出解析:")
    print("  - 输出64个位置的落子概率")
    print("  - 概率和为1.0")
    print("  - 只有合法位置的概率有意义")
    
    # 显示前5个推荐位置
    act_probs.sort(key=lambda x: x[1], reverse=True)
    print(f"\n前5个推荐位置:")
    for i, (move, prob) in enumerate(act_probs[:5]):
        row, col = move // 8, move % 8
        print(f"  {i+1}. 位置{move} 坐标({row},{col}) 概率:{prob:.4f}")
    
    # 解析价值输出
    print(f"\n价值输出解析:")
    print(f"  - 范围: [-1, 1]")
    print(f"  - 当前值: {value:.4f}")
    if value > 0:
        print(f"  - 含义: 当前玩家优势，胜率约{(value + 1) / 2 * 100:.1f}%")
    elif value < 0:
        print(f"  - 含义: 对手玩家优势，当前玩家胜率约{(value + 1) / 2 * 100:.1f}%")
    else:
        print(f"  - 含义: 局面均衡")
    
    # 5. ONNX模型使用
    print("\n5. ONNX模型使用")
    print("-" * 30)
    
    try:
        import onnxruntime as ort
        
        # 加载ONNX模型
        session = ort.InferenceSession("../models/final_policy.onnx")
        
        # 准备输入数据
        input_data = state.reshape(1, 4, 8, 8).astype(np.float32)
        
        # 运行推理
        outputs = session.run(None, {'input': input_data})
        onnx_policy, onnx_value = outputs
        
        print(f"ONNX输入形状: {input_data.shape}")
        print(f"ONNX策略输出形状: {onnx_policy.shape}")
        print(f"ONNX价值输出形状: {onnx_value.shape}")
        print(f"ONNX价值预测: {onnx_value[0][0]:.4f}")
        
        # 验证PyTorch和ONNX结果一致性
        policy_diff = np.abs(policy_output.numpy() - onnx_policy).max()
        value_diff = np.abs(value_output.numpy() - onnx_value).max()
        print(f"策略输出差异: {policy_diff:.8f}")
        print(f"价值输出差异: {value_diff:.8f}")
        
    except ImportError:
        print("ONNX Runtime未安装，跳过ONNX演示")
    except Exception as e:
        print(f"ONNX演示失败: {e}")
    
    # 6. 批量推理示例
    print("\n6. 批量推理示例")
    print("-" * 30)
    
    # 创建多个棋盘状态
    batch_states = []
    for i in range(3):
        temp_board = Board()
        temp_board.init_board()
        # 随机下几步
        for _ in range(i + 1):
            if temp_board.availables:
                move = np.random.choice(temp_board.availables)
                temp_board.do_move(move)
        batch_states.append(temp_board.current_state())
    
    # 批量推理
    batch_tensor = torch.FloatTensor(np.array(batch_states))
    print(f"批量输入形状: {batch_tensor.shape}")
    
    with torch.no_grad():
        batch_policy, batch_value = model(batch_tensor)
    
    print(f"批量策略输出形状: {batch_policy.shape}")
    print(f"批量价值输出形状: {batch_value.shape}")
    
    for i in range(len(batch_states)):
        print(f"  样本{i+1}: 价值={batch_value[i].item():.4f}")

def print_current_board(board):
    """打印当前棋盘状态"""
    print("\n当前棋盘:")
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

if __name__ == "__main__":
    demonstrate_model_usage() 