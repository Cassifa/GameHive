#!/usr/bin/env python3
"""
GPU训练要求检测脚本
检查系统是否满足GPU训练的条件
"""

import sys
import os
sys.path.append('..')

import torch
import platform
from config import Config

def check_gpu_requirements():
    """检查GPU训练要求"""
    print("=" * 70)
    print("🎮 GPU训练要求检测")
    print("=" * 70)
    
    # 1. 系统信息
    print("\n📋 系统信息:")
    print(f"  操作系统: {platform.system()} {platform.release()}")
    print(f"  Python版本: {platform.python_version()}")
    print(f"  PyTorch版本: {torch.__version__}")
    
    # 2. CUDA检测
    print("\n🔍 CUDA检测:")
    cuda_available = torch.cuda.is_available()
    print(f"  CUDA可用: {cuda_available}")
    
    if cuda_available:
        print(f"  CUDA版本: {torch.version.cuda}")
        print(f"  cuDNN版本: {torch.backends.cudnn.version()}")
        gpu_count = torch.cuda.device_count()
        print(f"  GPU数量: {gpu_count}")
        
        for i in range(gpu_count):
            gpu_name = torch.cuda.get_device_name(i)
            gpu_memory = torch.cuda.get_device_properties(i).total_memory / 1024**3
            print(f"  GPU {i}: {gpu_name} ({gpu_memory:.1f}GB)")
    else:
        print("  ❌ 未检测到CUDA支持")
    
    # 3. GPU训练条件检查
    print("\n✅ GPU训练条件检查:")
    
    conditions = []
    
    # 条件1: CUDA可用
    if cuda_available:
        conditions.append("✅ CUDA支持: 已安装")
    else:
        conditions.append("❌ CUDA支持: 未安装")
    
    # 条件2: GPU内存
    if cuda_available and gpu_count > 0:
        gpu_memory = torch.cuda.get_device_properties(0).total_memory / 1024**3
        if gpu_memory >= 4:
            conditions.append(f"✅ GPU内存: {gpu_memory:.1f}GB (推荐4GB+)")
        else:
            conditions.append(f"⚠️ GPU内存: {gpu_memory:.1f}GB (建议4GB+)")
    else:
        conditions.append("❌ GPU内存: 无GPU设备")
    
    # 条件3: PyTorch CUDA版本
    if hasattr(torch.version, 'cuda') and torch.version.cuda:
        conditions.append(f"✅ PyTorch CUDA版本: {torch.version.cuda}")
    else:
        conditions.append("❌ PyTorch CUDA版本: CPU版本")
    
    for condition in conditions:
        print(f"  {condition}")
    
    # 4. 训练建议
    print("\n💡 训练建议:")
    
    if cuda_available:
        print("  🎉 您的系统支持GPU训练!")
        print("  📝 使用GPU训练的命令:")
        print("     python train.py --use_gpu")
        print("     python train.py  # 默认会自动使用GPU")
        
        if gpu_count > 0:
            gpu_memory = torch.cuda.get_device_properties(0).total_memory / 1024**3
            if gpu_memory >= 8:
                print("  🚀 推荐配置 (高性能):")
                print("     --batch_size 4096")
                print("     --mcts_simulations 200")
            elif gpu_memory >= 4:
                print("  ⚖️ 推荐配置 (平衡):")
                print("     --batch_size 2048")
                print("     --mcts_simulations 100")
            else:
                print("  💾 推荐配置 (节省内存):")
                print("     --batch_size 1024")
                print("     --mcts_simulations 50")
    else:
        print("  ⚠️ 您的系统不支持GPU训练")
        print("  📝 使用CPU训练的命令:")
        print("     python train.py --use_cpu")
        print("  💾 CPU训练推荐配置:")
        print("     --batch_size 512")
        print("     --mcts_simulations 50")
        print("     --max_games 500  # 减少训练量")
    
    return cuda_available

def explain_gpu_requirements():
    """详细解释GPU训练要求"""
    print("\n" + "=" * 70)
    print("📚 GPU训练要求详解")
    print("=" * 70)
    
    print("\n🔧 硬件要求:")
    print("  1. NVIDIA GPU (支持CUDA)")
    print("     - 推荐: GTX 1060 6GB 或更好")
    print("     - 最低: GTX 1050 Ti 4GB")
    print("     - 不支持: AMD GPU, Intel集显")
    
    print("\n  2. GPU内存要求:")
    print("     - 4GB: 基础训练 (batch_size=1024)")
    print("     - 6GB: 标准训练 (batch_size=2048)")
    print("     - 8GB+: 高性能训练 (batch_size=4096)")
    
    print("\n💿 软件要求:")
    print("  1. NVIDIA驱动程序")
    print("     - 版本: 450.80.02+ (Linux) 或 452.39+ (Windows)")
    print("     - 检查命令: nvidia-smi")
    
    print("\n  2. CUDA Toolkit")
    print("     - 版本: 11.8 或 12.1")
    print("     - 下载: https://developer.nvidia.com/cuda-downloads")
    
    print("\n  3. PyTorch CUDA版本")
    print("     - 当前版本: CPU版本 (不支持GPU)")
    print("     - 需要安装: GPU版本")
    print("     - 安装命令:")
    print("       pip install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118")
    
    print("\n🚀 性能对比:")
    print("  训练速度 (相对CPU):")
    print("     - GTX 1060 6GB: 5-8倍")
    print("     - RTX 3060 12GB: 10-15倍")
    print("     - RTX 4080 16GB: 20-30倍")
    
    print("\n⚠️ 常见问题:")
    print("  1. 'CUDA out of memory'")
    print("     解决: 减小 --batch_size")
    
    print("\n  2. 'CUDA not available'")
    print("     解决: 重新安装PyTorch GPU版本")
    
    print("\n  3. 训练很慢")
    print("     检查: 是否真的在使用GPU")

def test_gpu_training():
    """测试GPU训练功能"""
    print("\n" + "=" * 70)
    print("🧪 GPU训练功能测试")
    print("=" * 70)
    
    if not torch.cuda.is_available():
        print("  ❌ 跳过GPU测试 (CUDA不可用)")
        return False
    
    try:
        print("  🔄 创建测试张量...")
        device = torch.device('cuda')
        x = torch.randn(100, 100).to(device)
        y = torch.randn(100, 100).to(device)
        
        print("  🔄 执行GPU计算...")
        z = torch.mm(x, y)
        
        print("  ✅ GPU计算成功!")
        print(f"  📊 结果形状: {z.shape}")
        print(f"  💾 GPU内存使用: {torch.cuda.memory_allocated() / 1024**2:.1f}MB")
        
        # 清理GPU内存
        del x, y, z
        torch.cuda.empty_cache()
        
        return True
        
    except Exception as e:
        print(f"  ❌ GPU测试失败: {e}")
        return False

def main():
    """主函数"""
    # 检查GPU要求
    gpu_available = check_gpu_requirements()
    
    # 详细解释要求
    explain_gpu_requirements()
    
    # 测试GPU功能
    if gpu_available:
        test_gpu_training()
    
    print("\n" + "=" * 70)
    print("📋 总结")
    print("=" * 70)
    
    if gpu_available:
        print("  🎉 恭喜! 您的系统支持GPU训练")
        print("  🚀 建议使用: python train.py --use_gpu")
    else:
        print("  ⚠️ 您的系统目前不支持GPU训练")
        print("  💻 建议使用: python train.py --use_cpu")
        print("  📝 如需GPU训练，请安装CUDA和PyTorch GPU版本")

if __name__ == "__main__":
    main() 