import torch
import torch.nn as nn
import torch.nn.functional as F
import numpy as np
from config import Config

class ResidualBlock(nn.Module):
    """残差块"""
    
    def __init__(self, channels, kernel_size=3):
        super(ResidualBlock, self).__init__()
        self.conv1 = nn.Conv2d(channels, channels, kernel_size, padding=1, bias=False)
        self.bn1 = nn.BatchNorm2d(channels)
        self.conv2 = nn.Conv2d(channels, channels, kernel_size, padding=1, bias=False)
        self.bn2 = nn.BatchNorm2d(channels)
        
    def forward(self, x):
        residual = x
        out = F.leaky_relu(self.bn1(self.conv1(x)))
        out = self.bn2(self.conv2(out))
        out += residual
        out = F.leaky_relu(out)
        return out

class PolicyValueNet(nn.Module):
    """策略价值网络"""
    
    def __init__(self, board_width=None, board_height=None, input_channels=None, 
                 hidden_filters=None, num_res_layers=None):
        super(PolicyValueNet, self).__init__()
        
        self.board_width = board_width or Config.BOARD_WIDTH
        self.board_height = board_height or Config.BOARD_HEIGHT
        self.input_channels = input_channels or Config.INPUT_CHANNELS
        self.hidden_filters = hidden_filters or Config.HIDDEN_FILTERS
        self.num_res_layers = num_res_layers or Config.NUM_RES_LAYERS
        
        # 输入卷积层
        self.conv_block = nn.Sequential(
            nn.Conv2d(self.input_channels, self.hidden_filters, 
                     kernel_size=Config.KERNEL_SIZE, padding=1, bias=False),
            nn.BatchNorm2d(self.hidden_filters),
            nn.LeakyReLU()
        )
        
        # 残差层
        self.res_layers = nn.ModuleList([
            ResidualBlock(self.hidden_filters, Config.KERNEL_SIZE) 
            for _ in range(self.num_res_layers)
        ])
        
        # 策略头
        self.policy_head = nn.Sequential(
            nn.Conv2d(self.hidden_filters, 2, kernel_size=1, bias=False),
            nn.BatchNorm2d(2),
            nn.LeakyReLU(),
            nn.Flatten(),
            nn.Linear(2 * self.board_width * self.board_height, 
                     self.board_width * self.board_height),
            nn.Softmax(dim=1)
        )
        
        # 价值头
        self.value_head = nn.Sequential(
            nn.Conv2d(self.hidden_filters, 1, kernel_size=1, bias=False),
            nn.BatchNorm2d(1),
            nn.LeakyReLU(),
            nn.Flatten(),
            nn.Linear(self.board_width * self.board_height, 32),
            nn.LeakyReLU(),
            nn.Linear(32, 1),
            nn.Tanh()
        )
        
        # 权重初始化
        self.apply(self._init_weights)
        
    def _init_weights(self, m):
        """权重初始化"""
        if isinstance(m, nn.Conv2d) or isinstance(m, nn.Linear):
            nn.init.xavier_uniform_(m.weight)
            if m.bias is not None:
                nn.init.constant_(m.bias, 0)
        elif isinstance(m, nn.BatchNorm2d):
            nn.init.constant_(m.weight, 1)
            nn.init.constant_(m.bias, 0)
    
    def forward(self, x):
        """前向传播"""
        # 输入卷积
        out = self.conv_block(x)
        
        # 残差层
        for res_layer in self.res_layers:
            out = res_layer(out)
        
        # 策略和价值预测
        policy = self.policy_head(out)
        value = self.value_head(out)
        
        return policy, value
    
    def policy_value_fn(self, board):
        """策略价值函数，用于MCTS"""
        # 获取可用位置
        legal_positions = board.availables
        current_state = board.current_state()
        
        # 转换为tensor
        state_tensor = torch.FloatTensor(current_state).unsqueeze(0).to(Config.DEVICE)
        
        with torch.no_grad():
            policy_probs, value = self.forward(state_tensor)
            policy_probs = policy_probs.cpu().numpy().flatten()
            value = value.cpu().numpy()[0][0]
        
        # 只保留合法位置的概率
        act_probs = zip(legal_positions, policy_probs[legal_positions])
        
        return act_probs, value
    
    def save_model(self, model_path):
        """保存模型"""
        torch.save(self.state_dict(), model_path)
        print(f"模型已保存到: {model_path}")
    
    def load_model(self, model_path):
        """加载模型"""
        self.load_state_dict(torch.load(model_path, map_location=Config.DEVICE))
        print(f"模型已从 {model_path} 加载")
    
    def export_onnx(self, onnx_path, input_shape=None):
        """导出ONNX模型"""
        if input_shape is None:
            input_shape = (1, self.input_channels, self.board_width, self.board_height)
        
        dummy_input = torch.randn(input_shape).to(Config.DEVICE)
        
        torch.onnx.export(
            self,
            dummy_input,
            onnx_path,
            export_params=True,
            opset_version=11,
            do_constant_folding=True,
            input_names=['input'],
            output_names=['policy', 'value'],
            dynamic_axes={
                'input': {0: 'batch_size'},
                'policy': {0: 'batch_size'},
                'value': {0: 'batch_size'}
            }
        )
        print(f"ONNX模型已导出到: {onnx_path}") 