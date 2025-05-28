import os
import random
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim
from collections import deque
from tqdm import tqdm

from config import Config
from policy_network import PolicyValueNet
from mcts import MCTSPlayer
from game import Game
from board import Board

class Trainer:
    """AlphaGo Zero训练器"""
    
    def __init__(self, use_gpu=True):
        # 设置设备
        Config.set_device(use_gpu)
        
        # 初始化棋盘和游戏
        self.board = Board()
        self.game = Game(self.board)
        
        # 初始化神经网络
        self.policy_value_net = PolicyValueNet().to(Config.DEVICE)
        
        # 初始化MCTS玩家
        self.mcts_player = MCTSPlayer(
            self.policy_value_net.policy_value_fn,
            c_puct=Config.C_PUCT,
            n_playout=Config.MCTS_SIMULATIONS,
            is_selfplay=1
        )
        
        # 训练数据池
        self.data_buffer = deque(maxlen=Config.TRAIN_DATA_POOL_SIZE)
        
        # 优化器
        self.optimizer = optim.Adam(
            self.policy_value_net.parameters(),
            lr=Config.LEARNING_RATE,
            weight_decay=Config.L2_REGULARIZATION
        )
        
        # 损失函数
        self.mse_loss = nn.MSELoss()
        self.cross_entropy_loss = nn.CrossEntropyLoss()
        
        # 创建模型保存目录
        os.makedirs(Config.MODEL_DIR, exist_ok=True)
        os.makedirs(Config.EVERY_1000_DIR, exist_ok=True)
        os.makedirs(Config.EVERY_100_DIR, exist_ok=True)
        
        print(f"训练器初始化完成，使用设备: {Config.DEVICE}")
        print(f"网络参数数量: {sum(p.numel() for p in self.policy_value_net.parameters())}")
    
    def collect_selfplay_data(self, n_games=1):
        """收集自我对弈数据"""
        for i in range(n_games):
            winner, play_data = self.game.start_self_play(self.mcts_player, temp=1.0)
            play_data = list(play_data)
            self.episode_len = len(play_data)
            
            # 数据增强
            play_data = self.get_equi_data(play_data)
            self.data_buffer.extend(play_data)
    
    def get_equi_data(self, play_data):
        """数据增强：通过旋转和翻转增加数据"""
        extend_data = []
        for state, mcts_porb, winner in play_data:
            for i in [1, 2, 3, 4]:
                # 旋转
                equi_state = np.array([np.rot90(s, i) for s in state])
                equi_mcts_prob = np.rot90(np.flipud(
                    mcts_porb.reshape(Config.BOARD_HEIGHT, Config.BOARD_WIDTH)), i)
                extend_data.append((equi_state, 
                                  np.flipud(equi_mcts_prob).flatten(), 
                                  winner))
                
                # 水平翻转
                equi_state = np.array([np.fliplr(s) for s in equi_state])
                equi_mcts_prob = np.fliplr(equi_mcts_prob)
                extend_data.append((equi_state, 
                                  equi_mcts_prob.flatten(), 
                                  winner))
        return extend_data
    
    def policy_update(self):
        """策略网络更新"""
        mini_batch = random.sample(self.data_buffer, Config.TRAIN_BATCH_SIZE)
        state_batch = [data[0] for data in mini_batch]
        mcts_probs_batch = [data[1] for data in mini_batch]
        winner_batch = [data[2] for data in mini_batch]
        
        # 转换为tensor
        state_batch = torch.FloatTensor(np.array(state_batch)).to(Config.DEVICE)
        mcts_probs_batch = torch.FloatTensor(np.array(mcts_probs_batch)).to(Config.DEVICE)
        winner_batch = torch.FloatTensor(np.array(winner_batch)).to(Config.DEVICE)
        
        # 前向传播
        log_act_probs, value = self.policy_value_net(state_batch)
        
        # 计算损失
        value_loss = self.mse_loss(value.view(-1), winner_batch)
        policy_loss = -torch.mean(torch.sum(mcts_probs_batch * torch.log(log_act_probs + 1e-10), 1))
        loss = value_loss + policy_loss
        
        # 反向传播
        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()
        
        # 计算策略熵（用于监控）
        entropy = -torch.mean(torch.sum(log_act_probs * torch.log(log_act_probs + 1e-10), 1))
        
        return loss.item(), entropy.item()
    
    def policy_evaluate(self, n_games=10):
        """评估当前策略"""
        current_mcts_player = MCTSPlayer(
            self.policy_value_net.policy_value_fn,
            c_puct=Config.C_PUCT,
            n_playout=Config.MCTS_SIMULATIONS
        )
        
        # 创建纯MCTS玩家作为基准
        pure_mcts_player = MCTSPlayer(
            self.pure_mcts_policy,
            c_puct=5,
            n_playout=1000
        )
        
        win_cnt = {'current': 0, 'pure': 0, 'tie': 0}
        for i in range(n_games):
            winner = self.game.start_play(current_mcts_player, pure_mcts_player, 
                                        start_player=i % 2, is_shown=0)
            if winner == 1:
                win_cnt['current'] += 1
            elif winner == 2:
                win_cnt['pure'] += 1
            else:
                win_cnt['tie'] += 1
        
        win_ratio = 1.0 * (win_cnt['current'] + 0.5 * win_cnt['tie']) / n_games
        print(f"胜率: {win_ratio:.3f}, 胜: {win_cnt['current']}, "
              f"负: {win_cnt['pure']}, 平: {win_cnt['tie']}")
        return win_ratio
    
    def pure_mcts_policy(self, board):
        """纯MCTS策略（无神经网络）"""
        # 返回均匀概率分布和随机价值
        action_probs = np.ones(len(board.availables)) / len(board.availables)
        return zip(board.availables, action_probs), 0
    
    def save_model_by_strategy(self, game_num):
        """根据策略保存模型"""
        # 1. 每100次保存（使用SAVE_FREQ参数）
        if (game_num + 1) % Config.SAVE_FREQ == 0:
            model_path = os.path.join(Config.EVERY_100_DIR, f"model_{game_num+1}.pth")
            onnx_path = os.path.join(Config.EVERY_100_DIR, f"model_{game_num+1}.onnx")
            self.policy_value_net.save_model(model_path)
            self.policy_value_net.export_onnx(onnx_path)
            print(f"模型已保存: {model_path}")
        
        # 2. 每1000次保存
        if (game_num + 1) % Config.SAVE_EVERY_1000 == 0:
            model_path = os.path.join(Config.EVERY_1000_DIR, f"model_{game_num+1}.pth")
            onnx_path = os.path.join(Config.EVERY_1000_DIR, f"model_{game_num+1}.onnx")
            self.policy_value_net.save_model(model_path)
            self.policy_value_net.export_onnx(onnx_path)
            print(f"每1000次模型已保存: {model_path}")
            
            # 特殊规则：清除every_100目录下的所有文件
            self.clear_every_100_models()
    
    def clear_every_100_models(self):
        """清除every_100目录下的所有文件"""
        import glob
        
        # 删除every_100目录下的所有.pth和.onnx文件
        pth_files = glob.glob(os.path.join(Config.EVERY_100_DIR, "*.pth"))
        onnx_files = glob.glob(os.path.join(Config.EVERY_100_DIR, "*.onnx"))
        
        deleted_count = 0
        for file_path in pth_files + onnx_files:
            try:
                os.remove(file_path)
                deleted_count += 1
            except Exception as e:
                print(f"删除文件失败 {file_path}: {e}")
        
        if deleted_count > 0:
            print(f"已清除every_100目录下的 {deleted_count} 个文件")
    
    def run(self):
        """运行训练"""
        total_games = 0
        try:
            for i in range(Config.MAX_GAMES):
                total_games = i + 1
                print(f"\n=== 第 {total_games} 局训练 ===")
                
                # 收集自我对弈数据
                self.collect_selfplay_data(1)
                print(f"局面长度: {self.episode_len}, 数据池大小: {len(self.data_buffer)}")
                
                # 如果数据足够，进行训练
                if len(self.data_buffer) > Config.TRAIN_BATCH_SIZE:
                    loss, entropy = self.policy_update()
                    print(f"损失: {loss:.4f}, 熵: {entropy:.4f}")
                
                # 使用新的保存策略
                self.save_model_by_strategy(i)
                
                # 定期评估模型（仅在保存模型时评估）
                if (i + 1) % Config.SAVE_FREQ == 0 and i > 200:
                    win_ratio = self.policy_evaluate()
                    if win_ratio > 0.6:  # 如果胜率超过60%，可以考虑增加MCTS模拟次数
                        print("模型表现良好，可以考虑增加MCTS模拟次数")
                
                # 定期清理数据池（可选）
                if len(self.data_buffer) > Config.TRAIN_DATA_POOL_SIZE * 0.9:
                    print("数据池接近满载，清理旧数据")
                    # 可以在这里实现数据池清理逻辑
        
        except KeyboardInterrupt:
            print(f"\n训练被用户中断，已完成 {total_games} 局训练")
        finally:
            # 生成包含训练参数的文件名
            param_suffix = (f"games_{total_games}_"
                          f"filters_{Config.HIDDEN_FILTERS}_"
                          f"layers_{Config.NUM_RES_LAYERS}_"
                          f"mcts_{Config.MCTS_SIMULATIONS}_"
                          f"batch_{Config.TRAIN_BATCH_SIZE}")
            
            # 保存最终模型，文件名包含训练参数
            final_model_path = os.path.join(Config.MODEL_DIR, f"final_policy_{param_suffix}.pth")
            self.policy_value_net.save_model(final_model_path)
            
            final_onnx_path = os.path.join(Config.MODEL_DIR, f"final_policy_{param_suffix}.onnx")
            self.policy_value_net.export_onnx(final_onnx_path)
            
            # 同时保存一份不带参数的版本（向后兼容）
            compat_model_path = os.path.join(Config.MODEL_DIR, "final_policy.pth")
            self.policy_value_net.save_model(compat_model_path)
            
            compat_onnx_path = os.path.join(Config.MODEL_DIR, "final_policy.onnx")
            self.policy_value_net.export_onnx(compat_onnx_path)
            
            print(f"训练完成，共进行了 {total_games} 局训练")
            print(f"最终模型已保存: {final_model_path}")
            print(f"兼容版本已保存: {compat_model_path}")

if __name__ == "__main__":
    # 创建训练器并开始训练
    trainer = Trainer(use_gpu=True)  # 设置为False使用CPU
    trainer.run() 