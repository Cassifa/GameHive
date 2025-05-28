#!/usr/bin/env python3
"""
服务器环境检查脚本
验证PyTorch 2.5.1 + CUDA 12.4 + Python 3.12环境
"""

import sys
import platform
import subprocess

def check_system_info():
    """检查系统信息"""
    print("=" * 60)
    print("🖥️ 系统环境检查")
    print("=" * 60)
    
    print(f"操作系统: {platform.system()} {platform.release()}")
    print(f"Python版本: {platform.python_version()}")
    print(f"架构: {platform.machine()}")
    
    # 检查CUDA
    try:
        result = subprocess.run(['nvcc', '--version'], capture_output=True, text=True)
        if result.returncode == 0:
            cuda_version = result.stdout.split('release ')[1].split(',')[0]
            print(f"CUDA版本: {cuda_version}")
        else:
            print("❌ CUDA未安装或不可用")
    except FileNotFoundError:
        print("❌ nvcc命令未找到，CUDA可能未安装")
    
    # 检查GPU
    try:
        result = subprocess.run(['nvidia-smi'], capture_output=True, text=True)
        if result.returncode == 0:
            lines = result.stdout.split('\n')
            for line in lines:
                if 'Driver Version:' in line:
                    driver_version = line.split('Driver Version: ')[1].split()[0]
                    print(f"NVIDIA驱动: {driver_version}")
                    break
        else:
            print("❌ nvidia-smi命令失败")
    except FileNotFoundError:
        print("❌ nvidia-smi命令未找到")

def check_pytorch():
    """检查PyTorch环境"""
    print("\n" + "=" * 60)
    print("🔥 PyTorch环境检查")
    print("=" * 60)
    
    try:
        import torch
        print(f"✅ PyTorch版本: {torch.__version__}")
        
        # 检查CUDA支持
        cuda_available = torch.cuda.is_available()
        print(f"✅ CUDA可用: {cuda_available}")
        
        if cuda_available:
            print(f"✅ CUDA版本: {torch.version.cuda}")
            print(f"✅ GPU数量: {torch.cuda.device_count()}")
            
            for i in range(torch.cuda.device_count()):
                gpu_name = torch.cuda.get_device_name(i)
                gpu_memory = torch.cuda.get_device_properties(i).total_memory / 1024**3
                print(f"✅ GPU {i}: {gpu_name} ({gpu_memory:.1f}GB)")
        else:
            print("❌ CUDA不可用")
            return False
            
    except ImportError:
        print("❌ PyTorch未安装")
        return False
    
    return True

def check_dependencies():
    """检查其他依赖"""
    print("\n" + "=" * 60)
    print("📦 依赖包检查")
    print("=" * 60)
    
    dependencies = ['numpy', 'matplotlib', 'onnx', 'onnxruntime']
    
    for dep in dependencies:
        try:
            __import__(dep)
            print(f"✅ {dep}: 已安装")
        except ImportError:
            print(f"❌ {dep}: 未安装")

def test_gpu_performance():
    """测试GPU性能"""
    print("\n" + "=" * 60)
    print("⚡ GPU性能测试")
    print("=" * 60)
    
    try:
        import torch
        import time
        
        if not torch.cuda.is_available():
            print("❌ 跳过GPU测试（CUDA不可用）")
            return
        
        device = torch.device('cuda')
        
        # 测试矩阵乘法性能
        print("🔄 测试GPU计算性能...")
        size = 4096
        a = torch.randn(size, size, device=device)
        b = torch.randn(size, size, device=device)
        
        # 预热
        for _ in range(3):
            _ = torch.mm(a, b)
        torch.cuda.synchronize()
        
        # 性能测试
        start_time = time.time()
        for _ in range(10):
            c = torch.mm(a, b)
        torch.cuda.synchronize()
        end_time = time.time()
        
        avg_time = (end_time - start_time) / 10
        gflops = (2 * size**3) / (avg_time * 1e9)
        
        print(f"✅ 矩阵乘法性能: {avg_time:.3f}秒")
        print(f"✅ 计算性能: {gflops:.1f} GFLOPS")
        
        # 内存测试
        memory_allocated = torch.cuda.memory_allocated() / 1024**3
        memory_reserved = torch.cuda.memory_reserved() / 1024**3
        print(f"✅ GPU内存使用: {memory_allocated:.2f}GB / {memory_reserved:.2f}GB")
        
    except Exception as e:
        print(f"❌ GPU测试失败: {e}")

def check_training_readiness():
    """检查训练就绪状态"""
    print("\n" + "=" * 60)
    print("🎯 训练就绪检查")
    print("=" * 60)
    
    # 检查项目文件
    import os
    required_files = [
        'config.py', 'board.py', 'policy_network.py', 
        'tree_node.py', 'mcts.py', 'game.py', 
        'trainer.py', 'train.py'
    ]
    
    missing_files = []
    for file in required_files:
        if os.path.exists(f'../{file}'):
            print(f"✅ {file}: 存在")
        else:
            print(f"❌ {file}: 缺失")
            missing_files.append(file)
    
    if missing_files:
        print(f"\n❌ 缺失关键文件: {missing_files}")
        return False
    
    print("\n✅ 所有训练文件就绪！")
    return True

def main():
    """主函数"""
    print("🚀 服务器环境检查 - PyTorch 2.5.1 + CUDA 12.4")
    
    # 系统检查
    check_system_info()
    
    # PyTorch检查
    pytorch_ok = check_pytorch()
    
    # 依赖检查
    check_dependencies()
    
    # GPU性能测试
    if pytorch_ok:
        test_gpu_performance()
    
    # 训练就绪检查
    training_ready = check_training_readiness()
    
    # 总结
    print("\n" + "=" * 60)
    print("📋 检查总结")
    print("=" * 60)
    
    if pytorch_ok and training_ready:
        print("🎉 环境检查通过！可以开始10万次训练")
        print("\n推荐训练命令:")
        print("python train.py --max_games 100000 --batch_size 4096 --mcts_simulations 200 --use_gpu")
    else:
        print("⚠️ 环境存在问题，请先解决上述问题")

if __name__ == "__main__":
    main() 