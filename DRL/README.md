# AlphaGo Zero五子棋 - PyTorch实现

基于PyTorch实现的AlphaGo Zero算法，用于训练五子棋AI。这是对原TensorFlow版本的完整重写，保持了相同的训练逻辑和参数设置。

## 🚀 特性

- **纯PyTorch实现**: 使用PyTorch 2.4.0构建的深度残差神经网络
- **GPU/CPU支持**: 可选择使用GPU或CPU进行训练
- **ONNX导出**: 训练完成的模型可导出为ONNX格式
- **命令行接口**: 灵活的参数配置
- **数据增强**: 通过旋转和翻转增加训练数据
- **实时监控**: 训练过程中的损失和胜率监控

## 📋 环境要求

```
Python 3.12+
PyTorch 2.4.0+
NumPy
ONNX (用于模型导出)
```

## 🛠️ 安装依赖

```bash
pip install onnx onnxruntime
```

## 🎯 快速开始

### 基本训练
```bash
python train.py
```

### 使用GPU训练
```bash
python train.py --use_gpu
```

### 强制使用CPU训练
```bash
python train.py --use_cpu
```

### 自定义参数训练
```bash
python train.py --max_games 4000 --batch_size 1024 --learning_rate 1e-3 --mcts_simulations 200
```

### 从已有模型继续训练
```bash
python train.py --resume models/policy_1000.pth
```

## ⚙️ 参数配置

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `--max_games` | 2000 | 最大训练局数 |
| `--save_freq` | 100 | 模型保存频率 |
| `--batch_size` | 2048 | 训练批次大小 |
| `--learning_rate` | 2e-3 | 学习率 |
| `--hidden_filters` | 38 | 隐藏层滤波器数量 |
| `--num_res_layers` | 5 | 残差层数量 |
| `--mcts_simulations` | 100 | MCTS模拟次数 |
| `--c_puct` | 5.0 | UCB探索常数 |
| `--model_dir` | models | 模型保存目录 |

## 🏗️ 项目结构

```
DRL/
├── config.py          # 配置参数管理
├── board.py           # 五子棋棋盘逻辑
├── policy_network.py  # PyTorch残差神经网络
├── tree_node.py       # MCTS树节点实现
├── mcts.py           # 蒙特卡洛树搜索算法
├── game.py           # 游戏逻辑和自我对弈
├── trainer.py        # 训练器核心逻辑
├── train.py          # 主训练脚本
├── README.md         # 项目说明文档
├── requirements.txt  # 依赖包列表
├── models/           # 模型保存目录
│   ├── every_1000/   # 每1000次训练保存
│   ├── every_100/    # 每100次训练保存（默认保存频率）
│   ├── final_policy.pth   # 最终PyTorch模型
│   └── final_policy.onnx  # 最终ONNX模型
└── testUtils/        # 测试和演示工具
    ├── test_setup.py     # 代码测试脚本
    ├── demo.py           # 模型演示脚本
    └── model_usage_example.py  # 模型使用示例
```

## 🧠 网络架构

- **输入**: 4通道的8x8棋盘状态
- **主干**: 5层残差卷积网络
- **策略头**: 输出64个位置的落子概率
- **价值头**: 输出当前局面的胜率评估

### 网络结构详解

```
输入: (batch_size, 4, 8, 8) - 4通道的8x8棋盘状态
    ↓
输入卷积层: Conv2d(4→38, 3x3) + BatchNorm + LeakyReLU
    ↓
残差块1-5: Conv2d(38→38, 3x3) + BN + LeakyReLU + 残差连接
    ↓
    ├─策略头─→ Conv2d(38→2, 1x1) + BN + LeakyReLU + Linear(128→64) + Softmax
    │         输出: (batch_size, 64) - 64个位置的落子概率
    │
    └─价值头─→ Conv2d(38→1, 1x1) + BN + LeakyReLU + Linear(64→32) + Linear(32→1) + Tanh
              输出: (batch_size, 1) - 局面评估值 [-1, 1]
```

**网络参数**: 总计142,653个可训练参数

## 🎮 训练过程

1. **自我对弈**: AI与自己对战生成训练数据
2. **数据收集**: 收集(状态, 动作概率, 胜负结果)三元组
3. **数据增强**: 通过旋转和翻转扩充数据
4. **网络训练**: 使用收集的数据训练策略价值网络
5. **模型评估**: 定期评估模型性能
6. **模型保存**: 定期保存模型和导出ONNX格式

## 📚 核心概念解释

### 局面长度 (Episode Length)
- **含义**: 一局游戏从开始到结束的总步数
- **例子**: 如果一局五子棋下了36步分出胜负，局面长度就是36
- **意义**: 反映对弈质量，适中长度说明双方实力相当

### 数据池大小 (Data Buffer Size)
- **含义**: 存储历史对弈数据的缓冲区中的样本数量
- **组成**: 每个样本包含(棋盘状态, MCTS搜索概率, 最终胜负)
- **作用**: 数据池越大，网络能学习到的经验越丰富
- **当前配置**: 最大36,000个样本，批次大小2,048

### 数据生成过程
1. **自我对弈**: AI用MCTS搜索与自己下棋，记录每步的状态和概率
2. **数据收集**: 一局36步游戏产生36个原始样本
3. **数据增强**: 通过8倍旋转翻转增强，产生36×8=288个训练样本
4. **网络训练**: 从数据池随机采样2048个样本训练神经网络

### AlphaGo Zero vs 传统MCTS
**传统MCTS**:
- 每次搜索从零开始
- 使用随机策略或简单启发式
- 搜索深度有限

**AlphaGo Zero MCTS**:
- 使用神经网络指导搜索
- 网络提供先验概率和价值评估
- 搜索更高效准确
- 通过自我对弈不断改进

### 训练过程直观理解
想象围棋高手教学:
1. 高手(MCTS)与自己对弈，展示好的下法
2. 学生(神经网络)观察并记录这些对弈
3. 学生通过大量观察学会模仿高手思路
4. 学生变强后，反过来帮助高手搜索更好
5. 循环往复，双方都越来越强

## 💻 代码结构说明

### trainer.py vs train.py
- **trainer.py**: 训练器核心类，包含所有训练算法逻辑
  - Trainer类封装自我对弈、数据收集、网络更新等方法
  - 面向对象设计，可复用
  - 专注算法逻辑，不处理命令行参数

- **train.py**: 训练启动脚本，提供命令行接口
  - 解析命令行参数，设置配置
  - 创建Trainer实例并启动训练
  - 用户友好的界面，专注用户交互

**关系**: train.py是用户界面，trainer.py是核心引擎

## 📊 模型保存策略

训练过程中采用分层保存策略：

### 每100次保存（默认保存频率）
- `models/every_100/model_100.pth` - 每100次PyTorch模型
- `models/every_100/model_100.onnx` - 每100次ONNX模型

### 每1000次保存
- `models/every_1000/model_1000.pth` - 每1000次PyTorch模型
- `models/every_1000/model_1000.onnx` - 每1000次ONNX模型

### 特殊规则
- 每当训练满1000次时，会自动清除`every_100`目录下的所有文件
- 最终模型文件名包含训练参数，如：
  - `final_policy_games_2000_filters_38_layers_5_mcts_100_batch_2048.pth`
  - 同时保存兼容版本：`final_policy.pth`

### 测试和演示
- 使用`python testUtils/test_setup.py`运行功能测试
- 使用`python testUtils/demo.py`演示模型效果
- 使用`python testUtils/model_usage_example.py`查看详细使用示例
- 使用`python testUtils/training_parameters_guide.py`查看训练参数指南

## 🔧 自定义配置

可以通过修改`config.py`文件来调整更多参数：

```python
# 游戏参数
BOARD_WIDTH = 8
BOARD_HEIGHT = 8
N_IN_ROW = 5

# 网络参数
INPUT_CHANNELS = 4
HIDDEN_FILTERS = 38
NUM_RES_LAYERS = 5

# 训练参数
TRAIN_DATA_POOL_SIZE = 36000
EPOCHS_PER_UPDATE = 10
```

## 📈 监控训练

训练过程中会显示：
- 当前训练局数
- 局面长度
- 数据池大小
- 训练损失
- 策略熵
- 模型胜率（定期评估）

## 🎯 与原项目的兼容性

本实现与原AlphaGo-Zero-Gobang项目完全兼容：
- 相同的棋盘尺寸和规则
- 相同的网络架构
- 相同的训练参数
- 相同的MCTS算法

## 🚨 注意事项

1. 训练需要大量计算资源，建议使用GPU
2. 完整训练可能需要数小时到数天
3. 模型文件较大，注意磁盘空间
4. 可以随时中断训练，模型会自动保存

## 🤝 贡献

欢迎提交Issue和Pull Request来改进这个项目！ 