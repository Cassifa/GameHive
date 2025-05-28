# 8*8五子棋深度强化学习模块

## 概述
本模块实现了基于深度强化学习的8*8五子棋AI算法，使用ONNX格式的预训练模型进行推理。

## 文件结构
- `GoBang88DRL.cs` - 深度强化学习具体产品实现
- `DeepRL.cs` - 深度强化学习抽象基类
- `model_3000.onnx` - 预训练的ONNX模型文件

## 模型信息
- **输入格式**: 4通道8x8张量 [batch_size=1, channels=4, height=8, width=8]
  - 通道0: 当前玩家(AI)的棋子位置
  - 通道1: 对手(Player)的棋子位置  
  - 通道2: 最后一步落子位置
  - 通道3: 当前玩家标识(AI=1, Player=0)

- **输出格式**: 
  - policy: 64维概率分布(8x8棋盘的每个位置的落子概率)
  - value: 标量值(当前局面的价值评估)

## 难度级别
- **LEVEL_1 (简单)**: 30%使用模型推理，70%随机落子
- **LEVEL_2 (中等)**: 70%使用模型推理，30%随机落子  
- **LEVEL_3 (困难)**: 100%使用模型推理，支持蒙特卡洛搜索

## 使用方式
```csharp
//创建深度强化学习AI实例
var drlAI = new GoBang88DRL(8, DifficultyLevel.LEVEL_3, useMonteCarlo: true);

//获取AI的下一步移动
var nextMove = drlAI.GetNextAIMove(currentBoard, lastX, lastY);

//检查游戏是否结束
var gameResult = drlAI.CheckGameOverByPiece(currentBoard, x, y);
```

## 特性
1. **模型预热**: 自动进行模型预热以提高首次推理速度
2. **错误处理**: 模型加载失败时自动降级为随机策略
3. **蒙特卡洛搜索**: 支持将模型用于蒙特卡洛搜索过程
4. **难度调节**: 通过混合模型推理和随机策略实现不同难度
5. **资源管理**: 自动管理ONNX模型资源的加载和释放

## 注意事项
- 模型文件必须放置在 `Resources/DRLModels/` 目录下
- 确保已安装 Microsoft.ML.OnnxRuntime NuGet包
- 模型推理需要一定的计算资源，建议在性能较好的设备上运行 