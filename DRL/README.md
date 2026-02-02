<h1 align="center">AlphaGo-Zero-Mini</h1>

---

## 1. 环境要求 (Environment)

### 软件依赖
- **Python** >= 3.8
- **PyTorch** 
- **tkinter** (仅 GUI 模式需要，Linux服务器需安装 `python3-tk`)
- 其他依赖见 `requirements.txt`：
  - onnx >= 1.18.0
  - onnxruntime >= 1.22.0
  - numpy
---

## 2. 使用方法 (Usage)

### GUI 模式 (本地运行)
提供可视化的对弈界面，支持人机对战和实时观察 AI 自我训练。
```bash
python code/MetaZeta.py
```
- **训练**：选择游戏类型 -> 勾选 `AI 自我训练` -> 点击 `开始/训练`。
- **对战**：选择游戏类型 -> 勾选 `与 AI 对战` -> 选择先/后手 -> 点击 `开始`。

### 命令行模式 (服务器训练)
适合在远程服务器（如 Linux Headless 环境）进行长时间训练。
```bash
# 训练五子棋 (接续训练，目标 10000 轮)
python3 code/train.py --game_type gobang --max_games 10000 --continue_train

# 训练不围棋 (从头开始)
python3 code/train.py --game_type antigo --max_games 10000
```
- **参数说明**：
  - `--game_type`: `gobang` / `antigo` / `tictactoe`
  - `--max_games`: 总训练轮数
  - `--batch_size`: 训练批次大小 (默认 512)
  - `--mcts_simulations`: 每次落子前的搜索次数 (默认 400)

---

## 3. 文件结构 (File Structure)

```text
AlphaGo-Zero-Gobang-main/
├── README.md             # 项目文档
├── requirements.txt      # 依赖列表
├── run_server.sh         # 服务器一键训练脚本 (示例)
├── code/                 # 源代码
│   ├── MetaZeta.py       # [入口] GUI 主程序
│   ├── train.py          # [入口] 命令行训练脚本
│   ├── Game.py           # 游戏控制器 (管理对局流程、绘制)
│   ├── AIplayer.py       # AI 玩家封装 (MCTSPlayer / NNPlayer)
│   ├── MCTS.py           # 蒙特卡洛树搜索核心逻辑
│   ├── TreeNode.py       # MCTS 树节点类
│   ├── PolicyNN.py       # 神经网络模型 (ResNet) & 数据增强
│   ├── games/            # 各种棋类逻辑实现
│   │   ├── base.py       # 棋盘基类
│   │   ├── gobang.py     # 五子棋 (8x8)
│   │   ├── antigo.py     # 不围棋 (7x7)
│   │   └── tictactoe.py  # 井字棋 (3x3)
│   └── models/           # 模型保存目录
│       └── gobang/       # 按游戏分类
│           ├── final/    # 最终模型
│           └── every_100 # 过程检查点-100
│           └── every_1000# 过程检查点-1000
└── docs/                 # 详细原理文档
```

---

## 4. 棋盘设计 (Board Design)

我们将棋盘状态转化为神经网络可以理解的 **张量 (Tensor)** 格式。
输入形状：`[Batch, 4, Width, Height]`

- **Channel 0**: **我方**棋子位置 (1为有子, 0为无子)。
- **Channel 1**: **对手**棋子位置。
- **Channel 2**: **上一手**落子位置 (标识局面的最新变化点)。
- **Channel 3**: **颜色标记** (全1或全0，帮助 AI 区分先手/后手优势)。

> 详见 `code/games/*.py` 中的 `current_state()` 方法。

---

## 5. 蒙特卡洛过程设计 (MCTS Design)

AI 不直接使用神经网络的输出下棋，而是将其作为 **“直觉”** 指导 MCTS 搜索。

**一次模拟 (Simulation) 的四个步骤：**

```text
       [根节点 Root]
           |
   1. 选择 (Selection)   ----> 沿着 Q+U 值最大的路径向下走
           |
           v
       [叶子节点 Leaf]
           |
   2. 扩展 (Expansion)   ----> 如果没结束，展开所有可能的下一步
           |
           v
   3. 评估 (Evaluation)  ----> 神经网络打分：(P: 概率, V: 胜率)
           |
           v
   4. 回溯 (Backup)      ----> 将 V 反向传回，更新沿途节点的 N 和 Q
           ^
           |
       (回到根节点)
```

**核心代码实现 (`code/MCTS.py`):**

```python
def playout(self, state):
    node = self.root
    # 1. 选择 (Selection)
    while True:
        if node.isLeaf(): break
        # 根据 Q+U 选择最佳子节点
        action, node = node.select(self.fator)
        state.do_move(action)
    
    # 2. 评估 (Evaluation)
    # 神经网络输出：action_probs (策略 P), leaf_value (价值 V)
    action_probs, leaf_value = self.policy_NN(state)
    
    # 检查游戏是否结束
    gameOver, winner = state.gameIsOver()

    if not gameOver:
        # 3. 扩展 (Expansion)
        node.expand(action_probs)
    else:
        # 如果结束，直接使用真实胜负作为价值
        leaf_value = 1.0 if winner == current_player else -1.0
    
    # 4. 回溯 (Backup)
    # 递归更新父节点，注意每一层 value 取反
    node.updateRecursive(-leaf_value)
```

> 最终落子策略 $\pi$ 基于根节点子节点的访问次数 $N$ 决定（而非直接用 Q 值），这使得搜索更加鲁棒。

---

## 6. 强化学习设计 (RL Loop)

系统通过 **自我对弈 (Self-Play)** 实现进化，无需人类棋谱。这是一个闭环的“自举”过程。

```text
                 +---------------------------+
                 |      自我对弈 (Self-Play)   |
                 |  MCTS vs MCTS 生成对局数据  |
                 +---------------------------+
                              |
                              v
                    [ 数据池 (Replay Buffer) ]
            (s, π, z) 样本: 状态, 搜索概率, 真实胜负
                              |
                              v
                 +---------------------------+
                 |      神经网络训练 (Train)   | <---+
                 |   让 P 逼近 π, V 逼近 z     |     |
                 +---------------------------+     | 数据增强
                              |                    | (旋转/翻转)
                              v                    |
                    [ 新模型 (New Model) ] ---------+
                              |
                              v
                 (用于下一轮自我对弈)
```

1.  **数据收集**：AI 控制黑白双方进行对局，每一步利用 MCTS 进行 400 次模拟。
2.  **数据增强**：利用棋盘的对称性，将 **1局数据扩充为8局**，极大提高数据利用率。
3.  **模型更新**：当数据池积累足够（> Batch Size），从中随机采样进行梯度下降。

---

## 7. 神经网络设计 (Neural Network)

基于 **ResNet (残差网络)** 的双头架构。

```text
[ 输入状态 Input (4xWxH) ]
         |
         v
+-------------------------+
|  公共骨干网络 (Backbone)  |
|  Conv2d + BatchNorm     |
|  ResBlock x N           |
|  ResBlock x N           |
+-------------------------+
         |
         +-----------------------------+
         |                             |
         v                             v
+------------------+          +------------------+
|  策略头 (Policy)  |          |  价值头 (Value)   |
|  Conv 1x1        |          |  Conv 1x1        |
|  Fully Connected |          |  Fully Connected |
|  Softmax         |          |  Tanh            |
+------------------+          +------------------+
         |                             |
         v                             v
[ 落子概率 P (WxH) ]          [ 胜率预估 V (1) ]
```

- **输入**: `[Batch, 4, Width, Height]` (特征层：我方、对手、上一手、颜色)
- **策略头**: 输出每个位置的落子概率 `P`。
- **价值头**: 输出当前局面的胜率评估 `V` (-1 到 1)。

**损失函数 (Loss Function):**
$$Loss = (z - v)^2 - \pi^T \log(p) + c||\theta||^2$$

1.  **价值损失**: $(z - v)^2$ (MSE) - 让预测胜率 $v$ 逼近真实结果 $z$。
2.  **策略损失**: $-\pi^T \log(p)$ (Cross Entropy) - 让直觉概率 $p$ 逼近 MCTS 搜索结果 $\pi$。
3.  **正则化**: $c||\theta||^2$ - L2 惩罚，防止过拟合。

---

## 8. 训练结果保存逻辑

为了防止训练中断丢失进度，采用了分层保存策略：

- **目录结构**：`models/{game_type}/`
  - `every_100/`: 每 100 轮保存一次（会自动清理旧模型，只保留最近的）。
  - `every_1000/`: 每 1000 轮保存一次（永久保留，作为里程碑）。
  - `final/`: 训练结束或用户中断(`Ctrl+C`)时保存的模型。
- **文件名**：`{game_type}_{round}.pth` (PyTorch权重) 和 `.onnx` (通用推理格式)。
- **断点续训**：使用 `--continue_train` 参数启动时，会自动查找并加载轮数最大的模型继续训练。
