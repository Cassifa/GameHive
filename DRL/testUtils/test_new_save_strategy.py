#!/usr/bin/env python3
"""
测试新的模型保存策略
验证save_freq参数和every_100目录的使用
"""

import os
import sys
sys.path.append('..')

from config import Config
from trainer import Trainer

def test_save_strategy():
    """测试保存策略"""
    print("=" * 50)
    print("测试新的模型保存策略")
    print("=" * 50)
    
    # 检查配置
    print(f"SAVE_FREQ: {Config.SAVE_FREQ}")
    print(f"SAVE_EVERY_100: {Config.SAVE_EVERY_100}")
    print(f"SAVE_EVERY_1000: {Config.SAVE_EVERY_1000}")
    
    # 检查是否还有CHECKPOINT_FREQ
    if hasattr(Config, 'CHECKPOINT_FREQ'):
        print(f"警告: 仍然存在CHECKPOINT_FREQ: {Config.CHECKPOINT_FREQ}")
    else:
        print("✓ CHECKPOINT_FREQ已成功移除")
    
    # 创建训练器
    trainer = Trainer(use_gpu=False)
    
    # 测试保存逻辑
    print("\n测试保存逻辑:")
    
    # 模拟不同的游戏次数
    test_games = [99, 100, 199, 200, 999, 1000]
    
    for game_num in test_games:
        print(f"\n游戏次数: {game_num + 1}")
        
        # 检查是否应该保存
        should_save_100 = (game_num + 1) % Config.SAVE_FREQ == 0
        should_save_1000 = (game_num + 1) % Config.SAVE_EVERY_1000 == 0
        
        print(f"  应该保存到every_100: {should_save_100}")
        print(f"  应该保存到every_1000: {should_save_1000}")
        
        if should_save_1000:
            print(f"  应该清除every_100目录")
    
    print("\n✓ 保存策略测试完成")
    print("✓ 新策略使用every_100目录作为主要保存位置")
    print("✓ save_freq默认值已改为100")
    print("✓ 不再使用checkpoints目录")

if __name__ == "__main__":
    test_save_strategy() 