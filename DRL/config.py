import torch

class Config:
    """训练配置参数"""
    
    # 游戏参数
    BOARD_WIDTH = 8
    BOARD_HEIGHT = 8
    N_IN_ROW = 5  # 五子棋
    
    # 网络参数
    INPUT_CHANNELS = 4  # 输入通道数
    HIDDEN_FILTERS = 38  # 隐藏层滤波器数量
    KERNEL_SIZE = 3  # 卷积核大小
    NUM_RES_LAYERS = 5  # 残差层数量
    L2_REGULARIZATION = 1e-4  # L2正则化系数
    
    # MCTS参数
    MCTS_SIMULATIONS = 100  # MCTS模拟次数
    C_PUCT = 5  # UCB公式中的探索常数
    
    # 训练参数
    MAX_GAMES = 2000  # 最大训练局数
    SAVE_FREQ = 100  # 模型保存频率
    TRAIN_BATCH_SIZE = 2048  # 训练批次大小
    TRAIN_DATA_POOL_SIZE = 36000  # 训练数据池大小
    EPOCHS_PER_UPDATE = 10  # 每次更新的训练轮数
    LEARNING_RATE = 2e-3  # 学习率
    KL_TARGET = 0.02  # KL散度目标
    
    # 设备配置
    DEVICE = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
    USE_GPU = True  # 是否使用GPU
    
    # 模型保存路径
    MODEL_DIR = 'models'
    CHECKPOINT_DIR = 'models/checkpoints'  # 检查点目录
    EVERY_1000_DIR = 'models/every_1000'   # 每1000次保存目录
    EVERY_100_DIR = 'models/every_100'     # 每100次保存目录
    ONNX_MODEL_PATH = 'models/policy_model.onnx'
    
    # 保存频率设置
    SAVE_EVERY_100 = 100     # 每100次保存
    SAVE_EVERY_1000 = 1000   # 每1000次保存
    
    @classmethod
    def set_device(cls, use_gpu=True):
        """设置训练设备"""
        if use_gpu and torch.cuda.is_available():
            cls.DEVICE = torch.device('cuda')
            print(f"使用GPU训练: {torch.cuda.get_device_name()}")
        else:
            cls.DEVICE = torch.device('cpu')
            print("使用CPU训练") 