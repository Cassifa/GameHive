import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F
import numpy as np
import os
import random
from collections import deque

class Net(nn.Module):
    """
    策略价值网络 (Policy-Value Network)
    基于 ResNet 架构，包含一个公共骨干网络和两个输出头：策略头(Policy Head)与价值头(Value Head)
    """
    # ---------------- 策略头 (Policy Head) ----------------
    # 用于预测落子概率分布
    # Conv 1x1: 降维，聚焦于每个位置的动作价值
    def __init__(self, board_width, board_height):
        super(Net, self).__init__()
        self.board_width = board_width
        self.board_height = board_height
        
        # ---------------- 公共骨干网络 (Backbone) ----------------
        self.conv1 = nn.Conv2d(4, 32, kernel_size=3, padding=1)
        self.conv2 = nn.Conv2d(32, 64, kernel_size=3, padding=1)
        self.conv3 = nn.Conv2d(64, 128, kernel_size=3, padding=1)
        
        # ---------------- 策略头 (Policy Head) ----------------
        self.act_conv1 = nn.Conv2d(128, 4, kernel_size=1)
        self.act_fc1 = nn.Linear(4 * board_width * board_height, board_width * board_height)
        
        # ---------------- 价值头 (Value Head) ----------------
        self.val_conv1 = nn.Conv2d(128, 2, kernel_size=1)
        self.val_fc1 = nn.Linear(2 * board_width * board_height, 64)
        self.val_fc2 = nn.Linear(64, 1)

    def forward(self, state_input):
        # state_input: batch_size x 4 x width x height
        
        # --- 骨干网络前向传播 ---
        x = F.relu(self.conv1(state_input))
        x = F.relu(self.conv2(x))
        x = F.relu(self.conv3(x))
        
        # --- 策略头前向传播 ---
        x_act = F.relu(self.act_conv1(x))
        x_act = x_act.view(-1, 4 * self.board_width * self.board_height) # 展平
        # LogSoftmax: 输出对数概率，方便计算交叉熵损失
        x_act = F.log_softmax(self.act_fc1(x_act), dim=1)
        
        # --- 价值头前向传播 ---
        x_val = F.relu(self.val_conv1(x))
        x_val = x_val.view(-1, 2 * self.board_width * self.board_height) # 展平
        x_val = F.relu(self.val_fc1(x_val))
        # Tanh: 将输出压缩到 [-1, 1] 区间，对应 输/赢
        x_val = torch.tanh(self.val_fc2(x_val))
        
        return x_act, x_val

class PolicyValueNet:
    """
    策略价值网络的高级封装类
    负责模型的训练、预测、保存和加载
    """
    def __init__(self, board_width, board_height, model_file=None):
        self.board_width = board_width
        self.board_height = board_height
        self.learning_rate = 2e-3
        self.l2_const = 1e-4  # L2 正则化系数，防止过拟合
        
        # 自动选择计算设备 (GPU > CPU)
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        
        # 初始化模型并移动到设备
        self.policy_value_net = Net(board_width, board_height).to(self.device)
        
        # 优化器: Adam
        self.optimizer = optim.Adam(self.policy_value_net.parameters(), 
                                    weight_decay=self.l2_const, 
                                    lr=self.learning_rate)
        
        # 如果提供了模型路径，尝试加载
        if model_file and os.path.exists(model_file):
            # print(f"正在从 {model_file} 加载模型...")
            # 加载 state_dict，如果模型是在 CPU 训练并在 GPU 上加载（或反之），map_location 很重要
            try:
                checkpoint = torch.load(model_file, map_location=self.device, weights_only=True)
            except TypeError:
                # 兼容旧版本 PyTorch (不支持 weights_only 参数)
                checkpoint = torch.load(model_file, map_location=self.device)
            
            # 兼容性处理：如果 checkpoint 包含 optimizer 状态 (虽然我们目前的 save_model 只存了 net.state_dict)
            if isinstance(checkpoint, dict) and 'model_state_dict' in checkpoint:
                self.policy_value_net.load_state_dict(checkpoint['model_state_dict'])
                # 如果未来保存了 optimizer，这里可以加载:
                # if 'optimizer_state_dict' in checkpoint:
                #     self.optimizer.load_state_dict(checkpoint['optimizer_state_dict'])
            else:
                # 旧版本或简单保存方式，直接是 state_dict
                self.policy_value_net.load_state_dict(checkpoint)
        else:
            if model_file:
                print(f"警告: 未找到模型文件 {model_file}。将使用随机权重初始化。")

        # 经验回放池 (Replay Buffer)
        # 存储自我对弈产生的数据 (state, mcts_probs, winner_z)
        self.trainDataPool = deque(maxlen=10000)
        self.trainBatchSize = 512

    def policy_NN(self, board):
        """
        预测接口：输入棋盘对象，返回动作概率和局面价值
        输入: board
        输出: (action, probability) 列表, value 标量
        """
        available_moves = board.availables
        
        # 获取当前棋盘状态特征张量 [4, W, H]
        current_state = np.ascontiguousarray(board.current_state().reshape(
                -1, 4, self.board_width, self.board_height))
        
        # 转换为 Tensor 并移动到设备
        current_state = torch.from_numpy(current_state).float().to(self.device)
        
        with torch.no_grad(): # 推理模式，不计算梯度
            log_act_probs, value = self.policy_value_net(current_state)
            act_probs = np.exp(log_act_probs.cpu().numpy().flatten()) # LogSoftmax -> Prob
            value = value.cpu().item()
            
        # 仅返回合法动作的概率
        act_probs = zip(available_moves, act_probs[available_moves])
        return act_probs, value

    def update(self, scrollText=None):
        """
        训练接口：从数据池采样并更新网络参数
        """
        # 随机采样一个 Batch
        minibatch = random.sample(self.trainDataPool, self.trainBatchSize)
        state_batch = [data[0] for data in minibatch]
        mcts_probs_batch = [data[1] for data in minibatch]
        winner_batch = [data[2] for data in minibatch]
        
        # 数据转换
        state_batch = np.array(state_batch).astype('float32')
        mcts_probs_batch = np.array(mcts_probs_batch).astype('float32')
        winner_batch = np.array(winner_batch).astype('float32')
        
        # 转换为 Tensor
        state_batch = torch.FloatTensor(state_batch).to(self.device)
        mcts_probs_batch = torch.FloatTensor(mcts_probs_batch).to(self.device)
        winner_batch = torch.FloatTensor(winner_batch).to(self.device)
        
        # 清空梯度
        self.optimizer.zero_grad()
        
        # 前向传播
        log_act_probs, value = self.policy_value_net(state_batch)
        
        # 计算损失函数
        # Value Loss (MSE): (预测价值 - 真实胜负)^2
        value_loss = F.mse_loss(value.view(-1), winner_batch)
        
        # Policy Loss (Cross Entropy): - sum(MCTS概率 * log(预测概率))
        # 目标是让神经网络的预测分布逼近 MCTS 的搜索分布
        policy_loss = -torch.mean(torch.sum(mcts_probs_batch * log_act_probs, 1))
        
        # 总损失
        loss = value_loss + policy_loss
        
        # 反向传播与参数更新
        loss.backward()
        self.optimizer.step()
        
        # 计算熵 (Entropy)，用于监控策略的随机性/探索程度
        entropy = -torch.mean(torch.sum(torch.exp(log_act_probs) * log_act_probs, 1))
        
        if scrollText:
            scrollText.insert('end', f"loss: {loss.item():.4f}, entropy: {entropy.item():.4f}\n")
            scrollText.see('end')
            
        return loss.item()

    def get_equi_data(self, play_data):
        """
        数据增强 (Data Augmentation)
        通过旋转和翻转棋盘，将 1 局数据扩充为 8 局
        play_data: [(state, mcts_prob, winner_z), ...]
        """
        extend_data = []
        for state, mcts_porb, winner in play_data:
            # 原始数据的 8 种变换
            # 1. 旋转 0, 90, 180, 270 度
            # 2. 水平翻转后，再旋转 0, 90, 180, 270 度
            for i in [1, 2, 3, 4]:
                # 旋转 state
                # state shape: [4, W, H]
                # np.rot90 默认旋转最后两个维度
                equi_state = np.array([np.rot90(s, i) for s in state])
                
                # 旋转概率分布 mcts_prob
                # mcts_prob 是一个长度为 W*H 的一维向量，需要先 reshape 成二维再旋转
                equi_mcts_prob = np.rot90(np.flipud(
                    mcts_porb.reshape(self.board_height, self.board_width)), i)
                
                extend_data.append((equi_state, np.flipud(equi_mcts_prob).flatten(), winner))
                
                # 水平翻转 (fliplr)
                equi_state = np.array([np.fliplr(s) for s in equi_state])
                equi_mcts_prob = np.fliplr(equi_mcts_prob)
                
                extend_data.append((equi_state, np.flipud(equi_mcts_prob).flatten(), winner))
                
        return extend_data

    def memory(self, play_data):
        """
        将对局数据存入经验回放池
        **在此处进行数据增强**
        """
        # 如果棋盘是正方形，可以进行旋转翻转增强
        if self.board_width == self.board_height:
            augmented_data = self.get_equi_data(play_data)
            self.trainDataPool.extend(augmented_data)
        else:
            # 非正方形棋盘无法旋转90度，只能进行有限的翻转增强，这里暂不处理
            self.trainDataPool.extend(play_data)

    def save_model(self, model_file):
        """保存 PyTorch 模型参数"""
        torch.save(self.policy_value_net.state_dict(), model_file)

    def export_onnx(self, model_file):
        """导出模型为 ONNX 格式 (用于跨平台/加速推理)"""
        dummy_input = torch.randn(1, 4, self.board_width, self.board_height, device=self.device)
        torch.onnx.export(self.policy_value_net, dummy_input, model_file, 
                          input_names=['input'], output_names=['action_probs', 'value'],
                          dynamic_axes={'input': {0: 'batch_size'},
                                        'action_probs': {0: 'batch_size'},
                                        'value': {0: 'batch_size'}})

class PolicyValueNetONNX:
    """
    ONNX 模型推理包装类
    """
    def __init__(self, board_width, board_height, model_file):
        self.board_width = board_width
        self.board_height = board_height
        import onnxruntime
        # 创建 ONNX Runtime 会话
        self.ort_session = onnxruntime.InferenceSession(model_file)

    def policy_NN(self, board):
        """
        ONNX 推理接口，与 PyTorch 版本保持一致的输入输出格式
        """
        available_moves = board.availables
        current_state = np.ascontiguousarray(board.current_state().reshape(
                -1, 4, self.board_width, self.board_height).astype('float32'))
        
        ort_inputs = {self.ort_session.get_inputs()[0].name: current_state}
        ort_outs = self.ort_session.run(None, ort_inputs)
        
        log_act_probs = ort_outs[0]
        value = ort_outs[1]
        
        act_probs = np.exp(log_act_probs.flatten())
        value = value.flatten()[0]
        
        act_probs = zip(available_moves, act_probs[available_moves])
        return act_probs, value
