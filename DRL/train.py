#!/usr/bin/env python3
"""
AlphaGo Zero五子棋训练脚本
基于PyTorch实现的深度强化学习训练
"""

import argparse
import os
import sys
from trainer import Trainer
from config import Config

def parse_args():
    """解析命令行参数"""
    parser = argparse.ArgumentParser(description='AlphaGo Zero五子棋训练')
    
    # 训练参数
    parser.add_argument('--max_games', type=int, default=2000,
                       help='最大训练局数 (默认: 2000)')
    parser.add_argument('--save_freq', type=int, default=100,
                       help='模型保存频率 (默认: 100)')
    parser.add_argument('--batch_size', type=int, default=2048,
                       help='训练批次大小 (默认: 2048)')
    parser.add_argument('--learning_rate', type=float, default=2e-3,
                       help='学习率 (默认: 2e-3)')
    
    # 网络参数
    parser.add_argument('--hidden_filters', type=int, default=38,
                       help='隐藏层滤波器数量 (默认: 38)')
    parser.add_argument('--num_res_layers', type=int, default=5,
                       help='残差层数量 (默认: 5)')
    
    # MCTS参数
    parser.add_argument('--mcts_simulations', type=int, default=100,
                       help='MCTS模拟次数 (默认: 100)')
    parser.add_argument('--c_puct', type=float, default=5.0,
                       help='UCB探索常数 (默认: 5.0)')
    
    # 设备参数
    parser.add_argument('--use_gpu', action='store_true', default=True,
                       help='使用GPU训练 (默认: True)')
    parser.add_argument('--use_cpu', action='store_true',
                       help='强制使用CPU训练')
    
    # 模型参数
    parser.add_argument('--model_dir', type=str, default='models',
                       help='模型保存目录 (默认: models)')
    parser.add_argument('--resume', type=str, default=None,
                       help='从指定模型继续训练')
    parser.add_argument('--resume_checkpoint', type=str, default=None,
                       help='从完整检查点继续训练（包含优化器状态和数据池）')
    
    return parser.parse_args()

def update_config(args):
    """根据命令行参数更新配置"""
    Config.MAX_GAMES = args.max_games
    Config.SAVE_FREQ = args.save_freq
    Config.TRAIN_BATCH_SIZE = args.batch_size
    Config.LEARNING_RATE = args.learning_rate
    Config.HIDDEN_FILTERS = args.hidden_filters
    Config.NUM_RES_LAYERS = args.num_res_layers
    Config.MCTS_SIMULATIONS = args.mcts_simulations
    Config.C_PUCT = args.c_puct
    Config.MODEL_DIR = args.model_dir
    
    # 设备配置
    use_gpu = args.use_gpu and not args.use_cpu
    Config.USE_GPU = use_gpu

def main():
    """主函数"""
    print("=" * 60)
    print("AlphaGo Zero五子棋训练 - PyTorch实现")
    print("=" * 60)
    
    # 解析命令行参数
    args = parse_args()
    
    # 更新配置
    update_config(args)
    
    # 打印配置信息
    print("\n训练配置:")
    print(f"  最大训练局数: {Config.MAX_GAMES}")
    print(f"  保存频率: {Config.SAVE_FREQ}")
    print(f"  批次大小: {Config.TRAIN_BATCH_SIZE}")
    print(f"  学习率: {Config.LEARNING_RATE}")
    print(f"  隐藏层滤波器: {Config.HIDDEN_FILTERS}")
    print(f"  残差层数: {Config.NUM_RES_LAYERS}")
    print(f"  MCTS模拟次数: {Config.MCTS_SIMULATIONS}")
    print(f"  UCB探索常数: {Config.C_PUCT}")
    print(f"  模型保存目录: {Config.MODEL_DIR}")
    print(f"  使用GPU: {Config.USE_GPU}")
    
    try:
        # 创建训练器
        trainer = Trainer(use_gpu=Config.USE_GPU)
        
        # 检查是否从完整检查点恢复
        if args.resume_checkpoint:
            if os.path.exists(args.resume_checkpoint):
                start_game_num = trainer.load_complete_checkpoint(args.resume_checkpoint)
                trainer.set_start_game_num(start_game_num)
                print(f"从完整检查点 {args.resume_checkpoint} 恢复训练")
            else:
                print(f"警告: 检查点文件 {args.resume_checkpoint} 不存在，从头开始训练")
                trainer.set_start_game_num(0)
        # 如果指定了恢复模型，加载它
        elif args.resume:
            if os.path.exists(args.resume):
                trainer.policy_value_net.load_model(args.resume)
                print(f"从 {args.resume} 恢复训练")
                print("⚠️  警告: 仅加载了模型权重，优化器状态和数据池已重置")
                print("⚠️  这可能导致性能退化，建议使用 --resume_checkpoint 参数")
                
                # 从文件名中提取训练局数
                import re
                filename = os.path.basename(args.resume)
                match = re.search(r'model_(\d+)', filename)
                if match:
                    start_game_num = int(match.group(1))
                    trainer.set_start_game_num(start_game_num)
                else:
                    print("无法从文件名中解析局数，从第0局开始")
                    trainer.set_start_game_num(0)
            else:
                print(f"警告: 模型文件 {args.resume} 不存在，从头开始训练")
                trainer.set_start_game_num(0)
        else:
            trainer.set_start_game_num(0)
        
        # 开始训练
        print("\n开始训练...")
        trainer.run()
        
    except KeyboardInterrupt:
        print("\n训练被用户中断")
    except Exception as e:
        print(f"\n训练过程中出现错误: {e}")
        import traceback
        traceback.print_exc()
    finally:
        print("训练结束")

if __name__ == "__main__":
    main() 