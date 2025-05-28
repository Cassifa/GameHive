#!/usr/bin/env python3
"""
训练参数指南 - 详细说明所有可配置的训练参数
"""

import sys
import os
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from config import Config

def show_training_parameters():
    """显示所有可配置的训练参数"""
    
    print("=" * 80)
    print("AlphaGo Zero 训练参数完整指南")
    print("=" * 80)
    
    print("\n🚀 基本训练命令")
    print("-" * 50)
    print("# 最简单的训练命令")
    print("python train.py")
    print()
    print("# 使用GPU训练")
    print("python train.py --use_gpu")
    print()
    print("# 强制使用CPU训练")
    print("python train.py --use_cpu")
    
    print("\n⚙️ 可配置参数详解")
    print("-" * 50)
    
    # 训练控制参数
    print("\n📊 训练控制参数:")
    print(f"--max_games          最大训练局数 (默认: {Config.MAX_GAMES})")
    print("                     建议: 新手500-1000局, 正式训练2000-5000局")
    print()
    print(f"--batch_size         训练批次大小 (默认: {Config.TRAIN_BATCH_SIZE})")
    print("                     建议: GPU用2048-4096, CPU用512-1024")
    print()
    print(f"--learning_rate      学习率 (默认: {Config.LEARNING_RATE})")
    print("                     建议: 0.001-0.005, 太大不稳定, 太小收敛慢")
    
    # 网络架构参数
    print("\n🧠 网络架构参数:")
    print(f"--hidden_filters     隐藏层滤波器数量 (默认: {Config.HIDDEN_FILTERS})")
    print("                     建议: 32-64, 越大网络越强但训练越慢")
    print()
    print(f"--num_res_layers     残差层数量 (默认: {Config.NUM_RES_LAYERS})")
    print("                     建议: 3-10层, 越深网络越强但训练越慢")
    
    # MCTS参数
    print("\n🌳 MCTS搜索参数:")
    print(f"--mcts_simulations   MCTS模拟次数 (默认: {Config.MCTS_SIMULATIONS})")
    print("                     建议: 快速测试50-100, 正式训练100-400")
    print()
    print(f"--c_puct             UCB探索常数 (默认: {Config.C_PUCT})")
    print("                     建议: 1-10, 越大越探索, 越小越利用")
    
    # 保存参数
    print("\n💾 模型保存参数:")
    print(f"--checkpoint_freq    检查点保存频率 (默认: {Config.CHECKPOINT_FREQ})")
    print("                     建议: 100-500, 太频繁占用磁盘, 太少怕丢失")
    print()
    print(f"--save_every_100     每100次保存 (默认: {Config.SAVE_EVERY_100})")
    print("                     建议: 保持100, 用于观察训练进度")
    print()
    print(f"--save_every_1000    每1000次保存 (默认: {Config.SAVE_EVERY_1000})")
    print("                     建议: 保持1000, 用于长期存档")
    
    # 设备参数
    print("\n💻 设备选择参数:")
    print("--use_gpu            强制使用GPU训练")
    print("--use_cpu            强制使用CPU训练")
    print("                     默认: 自动检测, 有GPU用GPU, 无GPU用CPU")
    
    print("\n📋 完整参数示例")
    print("-" * 50)
    
    print("\n# 快速测试 (10分钟内完成)")
    print("python train.py --max_games 50 --batch_size 512 --mcts_simulations 50 --use_cpu")
    
    print("\n# 中等训练 (几小时完成)")
    print("python train.py --max_games 1000 --batch_size 2048 --mcts_simulations 100 --use_gpu")
    
    print("\n# 完整训练 (一天以上)")
    print("python train.py --max_games 5000 --batch_size 4096 --mcts_simulations 200 --learning_rate 0.001 --use_gpu")
    
    print("\n# 强力网络 (更强但更慢)")
    print("python train.py --max_games 3000 --hidden_filters 64 --num_res_layers 8 --mcts_simulations 300 --use_gpu")
    
    print("\n# 快速网络 (较弱但较快)")
    print("python train.py --max_games 2000 --hidden_filters 32 --num_res_layers 3 --mcts_simulations 80 --use_gpu")

def show_parameter_effects():
    """显示参数对训练效果的影响"""
    
    print("\n" + "=" * 80)
    print("参数对训练效果的影响")
    print("=" * 80)
    
    print("\n📈 训练速度 vs 模型质量权衡")
    print("-" * 50)
    print("影响训练速度的参数 (从大到小):")
    print("  1. mcts_simulations  - 影响最大, 每局训练时间成正比")
    print("  2. batch_size        - 影响中等, 但太小会不稳定")
    print("  3. hidden_filters    - 影响中等, 网络计算量")
    print("  4. num_res_layers    - 影响中等, 网络深度")
    print("  5. max_games         - 影响总时间, 不影响单局时间")
    
    print("\n影响模型质量的参数 (从大到小):")
    print("  1. max_games         - 影响最大, 训练数据量")
    print("  2. mcts_simulations  - 影响很大, 搜索质量")
    print("  3. hidden_filters    - 影响中等, 网络表达能力")
    print("  4. num_res_layers    - 影响中等, 网络深度")
    print("  5. learning_rate     - 影响中等, 学习效率")
    
    print("\n⚖️ 推荐配置方案")
    print("-" * 50)
    
    print("\n🏃 快速验证方案 (适合测试代码):")
    print("  max_games: 100")
    print("  mcts_simulations: 50")
    print("  batch_size: 512")
    print("  预计时间: 30分钟 (CPU)")
    
    print("\n🚶 平衡方案 (适合日常训练):")
    print("  max_games: 2000")
    print("  mcts_simulations: 100")
    print("  batch_size: 2048")
    print("  预计时间: 4-8小时 (GPU)")
    
    print("\n🏋️ 高质量方案 (适合最终模型):")
    print("  max_games: 5000")
    print("  mcts_simulations: 200")
    print("  hidden_filters: 64")
    print("  num_res_layers: 8")
    print("  预计时间: 1-2天 (GPU)")
    
    print("\n💡 调参建议")
    print("-" * 50)
    print("1. 先用快速方案验证代码能跑通")
    print("2. 再用平衡方案训练一个基础模型")
    print("3. 最后用高质量方案训练最终模型")
    print("4. 如果GPU内存不够, 减小batch_size")
    print("5. 如果训练太慢, 减小mcts_simulations")
    print("6. 如果想要更强模型, 增加max_games")

def show_hardware_requirements():
    """显示硬件需求"""
    
    print("\n" + "=" * 80)
    print("硬件需求指南")
    print("=" * 80)
    
    print("\n💻 CPU训练:")
    print("-" * 30)
    print("适用场景: 测试代码, 小规模训练")
    print("推荐配置:")
    print("  - CPU: 4核以上")
    print("  - 内存: 8GB以上")
    print("  - 存储: 5GB可用空间")
    print("训练时间: 比GPU慢5-10倍")
    
    print("\n🎮 GPU训练:")
    print("-" * 30)
    print("适用场景: 正式训练, 大规模训练")
    print("推荐配置:")
    print("  - GPU: GTX 1060 6GB 或更好")
    print("  - 内存: 16GB以上")
    print("  - 存储: 10GB可用空间")
    print("训练时间: 标准速度")
    
    print("\n⚡ 高端配置:")
    print("-" * 30)
    print("适用场景: 专业研究, 超大规模训练")
    print("推荐配置:")
    print("  - GPU: RTX 3080 或更好")
    print("  - 内存: 32GB以上")
    print("  - 存储: 50GB可用空间")
    print("训练时间: 2-3倍加速")

if __name__ == "__main__":
    show_training_parameters()
    show_parameter_effects()
    show_hardware_requirements()
    
    print("\n" + "=" * 80)
    print("开始训练吧! 记住先用小参数测试, 再用大参数正式训练")
    print("=" * 80) 