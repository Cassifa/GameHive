import argparse
import os
import torch
import glob
import re
import sys

# 1. 获取当前脚本所在的绝对路径 (即 I:\AlphaGo-Zero-Gobang-main\code)
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
# 2. 将此路径加入系统路径，确保能导入同级模块
sys.path.append(BASE_DIR)
# 3. 定义模型存储的绝对路径 (即 I:\AlphaGo-Zero-Gobang-main\code\models)
MODELS_DIR = os.path.join(BASE_DIR, "models")

from AIplayer import MCTSPlayer
from Game import Game
from PolicyNN import PolicyValueNet
from games.gobang import GobangBoard
from games.tictactoe import TicTacToeBoard
from games.antigo import AntigoBoard

class MockCanvas:
    """
    模拟 Canvas 类
    用于在无 GUI 环境下初始化 Game 类，避免报错
    """
    def delete(self, *args): pass
    def create_line(self, *args): pass
    def create_text(self, *args): pass
    def create_oval(self, *args): pass
    def bind(self, *args): pass
    def update(self, *args): pass
    def create_rectangle(self, *args): return 1
    def move(self, *args): pass

class MockScrollText:
    """
    模拟 ScrollText 类
    用于接收 Game 类的日志输出，可选择性打印到控制台
    """
    def insert(self, *args): 
        # args[1] 是文本内容，如果需要详细日志可以取消下面这行的注释
        # print(args[1], end='') 
        pass
    def see(self, *args): pass
    def update(self, *args): pass
    def delete(self, *args): pass

def get_latest_model(game_type, model_dir=MODELS_DIR):
    """
    查找指定游戏类型轮次最大的模型 (优先在 final 目录中查找)
    :param game_type: 游戏类型 (gobang/tictactoe)
    :param model_dir: 模型根目录 (默认使用绝对路径 MODELS_DIR)
    :return: (最佳模型路径, 最大轮次)
    """
    # 按照优先级查找目录
    search_dirs = [
        os.path.join(model_dir, game_type, "final"),
        os.path.join(model_dir, game_type, "every_1000"),
        os.path.join(model_dir, game_type, "every_100")
    ]
    
    best_model = None
    max_round = 0
    
    for search_dir in search_dirs:
        if not os.path.exists(search_dir):
            continue
            
        files = glob.glob(os.path.join(search_dir, f"{game_type}_*.pth"))
        
        for f in files:
            # 从文件名中提取轮次数字
            # 注意：如果目录包含完整路径，basename 提取文件名
            basename = os.path.basename(f)
            match = re.search(r'_(\d+)\.pth$', basename)
            if match:
                round_num = int(match.group(1))
                if round_num > max_round:
                    max_round = round_num
                    best_model = f
    
    return best_model, max_round

def save_model_to_disk(policy_nn, game_type, round_num, is_final=False, model_dir=MODELS_DIR):
    """
    保存模型到磁盘，按照分层目录结构存储
    :param policy_nn: 神经网络实例
    :param game_type: 游戏类型
    :param round_num: 当前训练轮次
    :param is_final: 是否是最终模型
    :param model_dir: 模型根目录
    """
    game_model_dir = os.path.join(model_dir, game_type)
    if not os.path.exists(game_model_dir):
        os.makedirs(game_model_dir)

    # 确定保存的子目录列表
    sub_dirs = []
    if is_final:
        sub_dirs.append("final")
    
    if round_num % 1000 == 0:
        sub_dirs.append("every_1000")
    elif round_num % 100 == 0:
        sub_dirs.append("every_100")
        
    filename_base = f"{game_type}_{round_num}"
    
    for sub in sub_dirs:
        save_dir = os.path.join(game_model_dir, sub)
        if not os.path.exists(save_dir):
            os.makedirs(save_dir)
        
        pth_path = os.path.join(save_dir, f"{filename_base}.pth")
        onnx_path = os.path.join(save_dir, f"{filename_base}.onnx")
        
        try:
            policy_nn.save_model(pth_path)
            policy_nn.export_onnx(onnx_path)
            # print(f"模型已保存: {sub}/{filename_base}")
        except Exception as e:
            print(f"保存模型错误 ({sub}): {e}")

    # 触发清理逻辑：如果是1000轮倍数，清理之前的每100轮模型
    if round_num % 1000 == 0:
        clean_every_100_dir(game_model_dir, round_num)

def clean_every_100_dir(game_model_dir, current_1000_round):
    """
    清理 every_100 目录中的旧模型
    :param game_model_dir: 游戏模型目录
    :param current_1000_round: 当前的千轮里程碑，小于此轮次的百轮模型将被删除
    """
    every_100_dir = os.path.join(game_model_dir, "every_100")
    if not os.path.exists(every_100_dir):
        return
    
    for f in os.listdir(every_100_dir):
        match = re.search(r'_(\d+)\.(pth|onnx)$', f)
        if match:
            r_num = int(match.group(1))
            if r_num < current_1000_round:
                try:
                    os.remove(os.path.join(every_100_dir, f))
                except: pass

def main():
    parser = argparse.ArgumentParser(description="AlphaZero 五子棋/井字棋 训练脚本")
    parser.add_argument("--game_type", type=str, required=True, choices=['gobang', 'tictactoe', 'antigo', 'gobang15'], help="游戏类型: gobang 或 gobang15 或 tictactoe 或 antigo")
    parser.add_argument("--continue_train", action="store_true", help="是否从最新的检查点继续训练")
    parser.add_argument("--max_games", type=int, default=4000, help="总自我对弈训练局数")
    parser.add_argument("--batch_size", type=int, default=512, help="训练时的 Batch Size")
    parser.add_argument("--learning_rate", type=float, default=2e-3, help="学习率")
    parser.add_argument("--mcts_simulations", type=int, default=1000, help="训练时每步棋的 MCTS 模拟次数 (默认 1000)")
    
    args = parser.parse_args()
    
    # 1. 设备信息检测与输出
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    print(f"训练设备: {device}")
    if device.type == 'cuda':
        print(f"GPU 型号: {torch.cuda.get_device_name(0)}")
    
    # 2. 初始化棋盘
    if args.game_type == 'gobang':
        board = GobangBoard(width=8, height=8, n_in_row=5)
    elif args.game_type == 'gobang15':
        board = GobangBoard(width=15, height=15, n_in_row=5)
    elif args.game_type == 'antigo':
        # 不围棋默认 7x7，可根据需求调整
        board = AntigoBoard(width=7, height=7)
    else:
        board = TicTacToeBoard()
        
    # 3. 初始化策略价值网络
    model_file = None
    start_game_idx = 0
    
    if args.continue_train:
        # 使用更新后的 get_latest_model，默认使用 MODELS_DIR 绝对路径
        model_file, latest_round = get_latest_model(args.game_type)
        if model_file:
            print(f"发现检查点: {model_file}")
            print(f"当前已训练轮次: {latest_round}")
            start_game_idx = latest_round
            
            # 检查 max_games 设置
            # 如果目标是 4000，但已经训练了 2000，则从 2001 继续训练到 4000
            if args.max_games <= start_game_idx:
                print(f"目标 max_games ({args.max_games}) 小于或等于当前已训练轮次 ({start_game_idx})。")
                print("训练已完成，或请增加 --max_games 参数值。")
                return
        else:
            print(f"在 {os.path.join(MODELS_DIR, args.game_type)} 下未找到检查点。将从头开始训练。")

    policy_value_net = PolicyValueNet(board.width, board.height, model_file)
    # 更新超参数
    policy_value_net.trainBatchSize = args.batch_size
    policy_value_net.learning_rate = args.learning_rate
    # 注意: 优化器在 __init__ 中已初始化，若需动态修改 LR:
    for param_group in policy_value_net.optimizer.param_groups:
        param_group['lr'] = args.learning_rate

    # 4. 初始化 MCTS 玩家
    mcts_player = MCTSPlayer(policy_value_net.policy_NN)
    mcts_player.simulations_train = args.mcts_simulations
    
    # 5. 初始化游戏控制
    # 使用 Mock 类，因为不需要 GUI 界面
    game = Game(MockCanvas(), MockScrollText(), board, flag_is_shown=False, flag_is_train=True)
    
    # 6. 训练主循环
    print(f"开始训练 {args.game_type}...")
    print(f"进度: 从第 {start_game_idx + 1} 轮 到 {args.max_games} 轮")
    
    loss_list_10 = []   # 存储最近10轮
    loss_list_100 = []  # 存储最近100轮

    try:
        for i in range(start_game_idx, args.max_games):
            current_round = i + 1
            
            # 自我对弈 (Self-play)
            winner, play_data = game.selfPlay(mcts_player)
            policy_value_net.memory(play_data)
            
            # 网络训练 (Training)
            loss = 0.0
            # 只有当数据池中数据量达到 Batch Size 时才开始训练
            if len(policy_value_net.trainDataPool) > policy_value_net.trainBatchSize:
                loss = policy_value_net.update() 
                loss_list_10.append(loss)
                loss_list_100.append(loss)
                
                if len(loss_list_10) > 10: loss_list_10.pop(0)
                if len(loss_list_100) > 100: loss_list_100.pop(0)
            
            # 日志输出
            if current_round % 10 == 0:
                percent = (current_round / args.max_games) * 100
                bar_length = 50
                filled_length = int(bar_length * current_round // args.max_games)
                bar = '█' * filled_length + '-' * (bar_length - filled_length)
                
                # 检查数据量
                data_count = len(policy_value_net.trainDataPool)
                batch_size = policy_value_net.trainBatchSize
                
                if data_count > batch_size:
                    # 计算 Loss
                    # 每100轮显示最近100轮平均值，其他每10轮显示最近10轮平均值
                    if current_round % 100 == 0:
                        avg_loss = sum(loss_list_100) / len(loss_list_100) if loss_list_100 else 0.0
                        loss_info = f"Loss (Avg 100): {avg_loss:.4f}"
                    else:
                        avg_loss = sum(loss_list_10) / len(loss_list_10) if loss_list_10 else 0.0
                        loss_info = f"Loss (Avg 10): {avg_loss:.4f}"
                else:
                    loss_info = f"Collecting Data ({data_count}/{batch_size})"

                print(f"\r进度: [{bar}] {percent:.1f}% ({current_round}/{args.max_games}) | {loss_info}", end="")
                if current_round % 100 == 0:
                    print() # 换行，避免被覆盖

            # 模型保存
            is_final = (current_round == args.max_games)
            if current_round % 100 == 0 or is_final:
                save_model_to_disk(policy_value_net, args.game_type, current_round, is_final)
                
        print("训练完成!")
        
    except KeyboardInterrupt:
        print(f"\n用户中断训练。正在保存当前模型 (Round {current_round}) 到 final 目录...")
        # 强制保存到 final 目录，并以当前轮次命名
        save_model_to_disk(policy_value_net, args.game_type, current_round, is_final=True)
        print("模型已保存。")

if __name__ == "__main__":
    main()
