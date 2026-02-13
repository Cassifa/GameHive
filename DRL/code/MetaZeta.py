import os
import tkinter as tk
import threading
from tkinter import *
from tkinter import scrolledtext, messagebox, ttk
import torch

from AIplayer import *
from Game import *
from PolicyNN import PolicyValueNet, PolicyValueNetONNX
from games.gobang import GobangBoard
from games.tictactoe import TicTacToeBoard
from games.antigo import AntigoBoard


class MetaZeta(threading.Thread):
    """
    主程序类，继承自 Thread (虽然 GUI 应该在主线程，这里的设计是将训练/对战逻辑放入子线程)
    负责 GUI 界面构建、参数配置和启动游戏线程
    """
    save_ParaFreq = 100  # 模型保存频率 (每多少局)
    MAX_Games = 2000     # 默认最大训练局数

    def __init__(self, flag_is_shown=True, flag_is_train=True):
        super().__init__()
        self.flag_is_shown = flag_is_shown
        self.flag_is_train = flag_is_train
        self.stop_training = False

        self.window = tk.Tk()
        self.window.title('AlphaGoZero Mini')

        self.window.geometry('900x650')
        self.window.configure(bg='#f0f0f0')
        
        # 设置样式
        style = ttk.Style()
        style.theme_use('clam')
        style.configure('TButton', font=('Microsoft YaHei', 10), padding=5)
        style.configure('TLabel', font=('Microsoft YaHei', 10), background='#f0f0f0')
        style.configure('TRadiobutton', font=('Microsoft YaHei', 10), background='#f0f0f0')
        style.configure('TLabelframe', font=('Microsoft YaHei', 10, 'bold'), background='#f0f0f0')
        style.configure('TLabelframe.Label', font=('Microsoft YaHei', 10, 'bold'), background='#f0f0f0')

        # 主容器
        main_container = tk.Frame(self.window, bg='#f0f0f0')
        main_container.pack(fill=BOTH, expand=True, padx=20, pady=20)

        # 左侧：游戏区域
        left_panel = tk.Frame(main_container, bg='#f0f0f0')
        left_panel.pack(side=LEFT, fill=BOTH, expand=True)

        # 棋盘画布
        canvas_frame = tk.Frame(left_panel, bg='#CD853F', bd=2, relief=RIDGE)
        canvas_frame.pack(pady=(0, 20))
        self.canvas = tk.Canvas(canvas_frame, bg='#CD853F', height=440, width=440, highlightthickness=0)
        self.canvas.pack(padx=5, pady=5)

        # 日志区域
        log_frame = ttk.LabelFrame(left_panel, text="运行日志")
        log_frame.pack(fill=BOTH, expand=True)
        self.scrollText = scrolledtext.ScrolledText(log_frame, width=50, height=8, font=('Consolas', 9))
        self.scrollText.pack(fill=BOTH, expand=True, padx=5, pady=5)

        # 右侧：控制面板
        right_panel = tk.Frame(main_container, bg='#f0f0f0', width=300)
        right_panel.pack(side=RIGHT, fill=Y, padx=(20, 0), anchor='n')

        # 5. 操作按钮 (先 Pack 到底部，确保可见)
        btn_frame = tk.Frame(right_panel, bg='#f0f0f0')
        btn_frame.pack(side=BOTTOM, fill=X, pady=20)
        
        self.is_training_active = False
        self.is_game_active = False

        self.btGame = ttk.Button(btn_frame, text='开始游戏', command=self.toggle_game_btn)
        self.btGame.pack(fill=X, pady=5)
        
        self.btTrain = ttk.Button(btn_frame, text='开始训练', command=self.toggle_training_btn)
        self.btTrain.pack(fill=X, pady=5)

        self.btReset = ttk.Button(btn_frame, text='重置棋盘', command=self.resetCanvas)
        self.btReset.pack(fill=X, pady=5)

        # 1. 游戏设置 (Pack 到顶部)
        game_group = ttk.LabelFrame(right_panel, text="游戏设置")
        game_group.pack(side=TOP, fill=X, pady=(0, 10), ipady=5)
        
        self.game_type = tk.StringVar(value="gobang")
        ttk.Radiobutton(game_group, text='五子棋 (8x8)', variable=self.game_type, value='gobang', command=self.reset_game_env).pack(anchor='w', padx=10, pady=1)
        ttk.Radiobutton(game_group, text='五子棋 (15x15)', variable=self.game_type, value='gobang15', command=self.reset_game_env).pack(anchor='w', padx=10, pady=1)
        ttk.Radiobutton(game_group, text='不围棋 (7x7)', variable=self.game_type, value='antigo', command=self.reset_game_env).pack(anchor='w', padx=10, pady=1)
        ttk.Radiobutton(game_group, text='井字棋 (3x3)', variable=self.game_type, value='tictactoe', command=self.reset_game_env).pack(anchor='w', padx=10, pady=1)
        
        # 3. AI 设置
        ai_group = ttk.LabelFrame(right_panel, text="AI 配置")
        ai_group.pack(side=TOP, fill=X, pady=(0, 5), ipady=2)
        
        ai_frame = tk.Frame(ai_group, bg='#f0f0f0')
        ai_frame.pack(fill=X, padx=10, pady=1)
        ttk.Label(ai_frame, text="决策:").pack(side=LEFT)
        self.ai_decision_mode = tk.StringVar(value="mcts")
        ttk.Radiobutton(ai_frame, text='MCTS', variable=self.ai_decision_mode, value='mcts').pack(side=LEFT, padx=5)
        ttk.Radiobutton(ai_frame, text='纯策略网', variable=self.ai_decision_mode, value='pure_nn').pack(side=LEFT)
        
        train_frame = tk.Frame(ai_group, bg='#f0f0f0')
        train_frame.pack(fill=X, padx=10, pady=2)
        ttk.Label(train_frame, text="训练轮数:").pack(side=LEFT)
        self.train_rounds = tk.IntVar(value=2000)
        ttk.Entry(train_frame, textvariable=self.train_rounds, width=8).pack(side=LEFT, padx=5)
        
        self.device_str = "CUDA" if torch.cuda.is_available() else "CPU"
        ttk.Label(ai_group, text=f"当前设备: {self.device_str}", foreground='blue').pack(anchor='w', padx=10, pady=(0, 2))
        
        # 4. 玩家设置
        player_group = ttk.LabelFrame(right_panel, text="玩家执子")
        player_group.pack(side=TOP, fill=X, pady=(0, 5), ipady=2)
        
        self.human_color = tk.IntVar(value=1) # 默认先手 (黑棋)
        ttk.Radiobutton(player_group, text='黑棋 (先手)', variable=self.human_color, value=1).pack(anchor='w', padx=10, pady=1)
        ttk.Radiobutton(player_group, text='白棋 (后手)', variable=self.human_color, value=2).pack(anchor='w', padx=10, pady=1)


        # 初始化游戏环境
        self.game = None
        self.NN = None
        self.MCTSPlayer = None
        self.reset_game_env()

        self.window.protocol("WM_DELETE_WINDOW", self.on_closing)
        self.window.mainloop()

    def reset_game_env(self):
        """初始化或重置游戏环境 (切换游戏类型时调用)"""
        game_type = self.game_type.get()
        
        if game_type == 'gobang':
            board = GobangBoard(width=8, height=8, n_in_row=5)
        elif game_type == 'gobang15':
            board = GobangBoard(width=15, height=15, n_in_row=5)
        elif game_type == 'antigo':
            board = AntigoBoard(width=7, height=7)
        elif game_type == 'tictactoe':
            board = TicTacToeBoard()
        else:
            board = GobangBoard(width=8, height=8, n_in_row=5)

        # 重置 Game 控制器
        self.game = Game(Canvas=self.canvas, scrollText=self.scrollText, 
                         board=board,
                         flag_is_shown=self.flag_is_shown, flag_is_train=self.flag_is_train)
        
        # 重置神经网络 (根据新棋盘大小重新初始化)
        self.NN = PolicyValueNet(board_width=board.width, board_height=board.height)
        
        # 重置 MCTS 玩家
        self.MCTSPlayer = MCTSPlayer(policy_NN=self.NN.policy_NN)
        
        self.resetCanvas()
        self.drawScrollText(f"已切换到: {game_type} ({board.width}x{board.height})")

    def threadTrain(self, func):
        """在子线程中运行耗时任务 (训练/对战)，避免阻塞 GUI"""
        myThread = threading.Thread(target=func)
        myThread.setDaemon(True)
        myThread.start()

    def DrawCanvas(self):
        """绘制棋盘网格"""
        self.canvas.delete("all")
        
        board_width = self.game.boardWidth
        # 调整边距以适应新的 Canvas 大小 (440x440)，棋盘区域保持 400x400 居中
        # 440 - 400 = 40, side_margin = 20
        side_margin = 20
        board_size = 400
        game_type = self.game_type.get()
        
        if board_width == 3 or game_type == 'antigo': # 井字棋 或 不围棋 画法 (格子)
            # 使用矩形格子的画法
            # 修正坐标计算逻辑
            # 格子画法：棋盘是 N x N 的格子
            # 棋子应该画在每个格子的中心
            
            # 画外边框
            self.canvas.create_rectangle(side_margin, side_margin, side_margin + board_size, side_margin + board_size, width=2)
            
            cell_size = board_size / board_width
            
            # 画竖线 (从第1条线画到第N-1条线)
            for i in range(1, board_width):
                pos = side_margin + i * cell_size
                self.canvas.create_line(pos, side_margin, pos, side_margin + board_size, width=2)
            
            # 画横线
            for i in range(1, board_width):
                pos = side_margin + i * cell_size
                self.canvas.create_line(side_margin, pos, side_margin + board_size, pos, width=2)
            
            # 标记坐标数字 (可选，辅助调试)
            # self.canvas.delete("label")
            # for i in range(board_width):
            #     center = side_margin + i * cell_size + cell_size / 2
            #     self.canvas.create_text(side_margin - 15, center, text=str(i))
            #     self.canvas.create_text(center, side_margin - 15, text=str(i))
            
        else: # 五子棋画法 (交叉点)
            grid_size = (board_size) / (board_width - 1) if board_width > 1 else board_size
            for i in range(board_width): 
                 pos = side_margin + i * grid_size
                 self.canvas.create_line(side_margin, pos, side_margin + board_size, pos)
                 self.canvas.create_line(pos, side_margin, pos, side_margin + board_size)
                 
            self.canvas.delete("label")
            for i in range(board_width):
                self.canvas.create_text(side_margin - 15, side_margin + i * grid_size, text=str(i))
                self.canvas.create_text(side_margin + i * grid_size, side_margin - 15, text=str(i))

    def drawScrollText(self, string):
        self.scrollText.insert(END, string + '\n')
        self.scrollText.see(END)

    def resetCanvas(self, stop_only=False, reset_buttons=True):
        """重置画布并中断训练"""
        self.stop_training = True
        
        # 如果只是为了停止，不需要重绘和保存模型逻辑（或者根据需求调整）
        # 这里保留原有逻辑，但在 Start 时调用会先触发一次保存（如果是从训练状态切换过来），这其实是好事。
        
        if hasattr(self, 'flag_is_train') and self.flag_is_train:
            try:
                # 只有当确实在训练且被中断时才保存
                # 简单判断：如果 stop_only=False (即点击重置按钮或开始新游戏时)，保存当前进度
                if not os.path.exists("models"):
                    os.makedirs("models")
                
                # 避免在刚启动还没训练时就保存空模型
                if self.NN is not None and len(self.NN.trainDataPool) > 0:
                     save_path = f'models/{self.game_type.get()}-interrupted.pth'
                     self.NN.save_model(save_path)
                     self.drawScrollText(f"训练中断，模型已保存至: {save_path}")
            except Exception as e:
                print(f"保存模型失败: {e}")

        self.canvas.delete("all")
        self.scrollText.delete(1.0, END)
        self.DrawCanvas()
        
        if reset_buttons:
            # 恢复按钮状态 (如果是因为点击重置而停止)
            self.is_training_active = False
            self.is_game_active = False
            try:
                self.btTrain.configure(text='开始训练')
                self.btGame.configure(text='开始游戏')
            except:
                pass

    def export_onnx(self):
        # ... (if any other method exists, place it before that, or at end of class)
        pass

    def toggle_game_btn(self):
        """开始/停止 游戏按钮逻辑"""
        if self.is_game_active:
            # 停止游戏
            self.stopTraining() # 复用停止逻辑
            self.btGame.configure(text='开始游戏')
            self.is_game_active = False
        else:
            # 开始游戏
            if self.is_training_active:
                messagebox.showwarning("提示", "请先停止训练！")
                return
            
            # 强制重置棋盘（保留按钮状态，不重置按钮）
            self.resetCanvas(stop_only=False, reset_buttons=False)
            self.stop_training = False # resetCanvas 可能会将其置为 True，这里要重新打开
            
            self.is_game_active = True
            self.btGame.configure(text='停止游戏')
            
            self.flag_is_train = False
            self.game.flag_is_train = False
            
            self.threadTrain(self.run_play_with_human_wrapper)

    def toggle_training_btn(self):
        """开始/停止 训练按钮逻辑"""
        if self.is_training_active:
            # 停止训练
            self.stopTraining()
            self.btTrain.configure(text='开始训练')
            self.is_training_active = False
        else:
            # 开始训练
            if self.is_game_active:
                messagebox.showwarning("提示", "请先停止当前游戏！")
                return
            
            # 强制重置棋盘（保留按钮状态，不重置按钮）
            self.resetCanvas(stop_only=False, reset_buttons=False)
            self.stop_training = False
            
            self.is_training_active = True
            self.btTrain.configure(text='停止训练')
            
            self.flag_is_train = True
            self.game.flag_is_train = True
            
            self.threadTrain(self.run_training_wrapper)

    def run_training_wrapper(self):
        try:
            self.run_training()
        finally:
            # 训练结束（无论是正常结束还是异常），恢复按钮状态
            # 注意：在非 UI 线程操作 UI 控件可能会有问题，但在 Tkinter 中某些操作是允许的，或者使用 after
            # 为了安全，这里不直接改 UI，或者假设 stopTraining 已经被调用
            # 实际上如果用户点击停止，UI已经变了。如果是自动结束，这里应该变 UI。
            # 我们简单处理：不做自动恢复，或者仅处理 stop_training 标志
            self.is_training_active = False
            try:
                self.btTrain.configure(text='开始训练')
            except:
                pass

    def run_play_with_human_wrapper(self):
        try:
            self.run_play_with_human()
        finally:
            self.is_game_active = False
            try:
                self.btGame.configure(text='开始游戏')
            except:
                pass

    def stopTraining(self):
        """停止当前训练"""
        self.stop_training = True
        self.drawScrollText("已发送停止信号，请等待当前对局结束...")


    def run_training(self):
        """执行训练循环"""
        Loss = []
        
        # 确保基础模型目录存在
        if not os.path.exists("models"):
            os.makedirs("models")

        total_games = self.train_rounds.get()
        game_type = self.game_type.get()
        
        # 确保当前游戏类型的模型目录存在
        game_model_dir = os.path.join("models", game_type)
        if not os.path.exists(game_model_dir):
            os.makedirs(game_model_dir)

        # 辅助函数：保存模型
        def save_model_to_disk(round_num, is_final=False):
            # 目录策略：
            # models/{game_type}/every_100/  (每100轮)
            # models/{game_type}/every_1000/ (每1000轮)
            # models/{game_type}/final/      (最终模型)
            
            # 确定保存子目录
            sub_dirs = []
            
            if is_final:
                sub_dirs.append("final")
            
            if round_num % 1000 == 0:
                sub_dirs.append("every_1000")
            elif round_num % 100 == 0:
                # 只有非1000倍数的100倍数才存入 every_100 (避免重复，或者都存也可以，这里按需求逻辑)
                # 需求描述："按照是100次训练还是1000次...来分层"
                sub_dirs.append("every_100")
            
            # 如果是 final 且又是 1000 的倍数，会存两份：一份在 final，一份在 every_1000
            
            # 构造文件名: {game_type}_{round_num}
            filename_base = f"{game_type}_{round_num}"
            
            for sub in sub_dirs:
                save_dir = os.path.join(game_model_dir, sub)
                if not os.path.exists(save_dir):
                    os.makedirs(save_dir)
                
                pth_path = os.path.join(save_dir, f"{filename_base}.pth")
                onnx_path = os.path.join(save_dir, f"{filename_base}.onnx")
                
                try:
                    self.NN.save_model(pth_path)
                    self.NN.export_onnx(onnx_path)
                    self.drawScrollText(f"模型已保存: {sub}/{filename_base}")
                except Exception as e:
                    print(f"Save model error ({sub}): {e}")

            # 清理逻辑：每次保存1000次模型时，把所有训练次数少于自己的100次模型删除
            if round_num % 1000 == 0:
                clean_every_100_dir(round_num)

        def clean_every_100_dir(current_1000_round):
            # 删除 models/{game_type}/every_100/ 下所有 round < current_1000_round 的模型
            every_100_dir = os.path.join(game_model_dir, "every_100")
            if not os.path.exists(every_100_dir):
                return
            
            import re
            for f in os.listdir(every_100_dir):
                # 匹配文件名中的轮数
                # 格式: {game_type}_{round}.pth 或 .onnx
                # 注意 game_type 可能包含下划线，所以最好用正则匹配最后的数字
                match = re.search(r'_(\d+)\.(pth|onnx)$', f)
                if match:
                    r_num = int(match.group(1))
                    if r_num < current_1000_round:
                        file_path = os.path.join(every_100_dir, f)
                        try:
                            os.remove(file_path)
                            print(f"已清理过期模型: {f}")
                        except Exception as e:
                            print(f"清理模型失败 {f}: {e}")
            self.drawScrollText(f"已清理 {current_1000_round} 轮之前的中间模型")

        for oneGame in range(total_games):
            if self.stop_training: break
            
            self.canvas.delete("all")
            self.DrawCanvas()

            self.drawScrollText(f'正在 第{oneGame + 1}/{total_games}轮 自我训练...')
            # 执行一局自我对弈
            winner, play_data = self.game.selfPlay(self.MCTSPlayer, Index=oneGame + 1, check_stop=lambda: self.stop_training)
            
            if winner == -1 and not play_data:
                break
                
            # 存储数据
            self.NN.memory(play_data)

            # 只要数据够了就开始训练 (每局结束后尝试更新一次)
            if len(self.NN.trainDataPool) > self.NN.trainBatchSize:
                loss = self.NN.update(scrollText=self.scrollText)
                Loss.append(loss)
            else:
                self.drawScrollText(f"收集数据: {len(self.NN.trainDataPool)/self.NN.trainBatchSize*100:.1f}%")

            # 保存模型逻辑
            current_round = oneGame + 1
            is_final = (current_round == total_games)
            
            # 满足保存条件：是100倍数，或者是最后一轮
            if current_round % 100 == 0 or is_final:
                save_model_to_disk(current_round, is_final)

    def run_play_with_human(self):
        """执行人机对战"""
        import glob
        import re
        
        game_type = self.game_type.get()
        # 模型现在存储在 models/{game_type}/ 下的子目录中
        # 优先查找 final，其次 every_1000，其次 every_100
        game_model_dir = os.path.join("models", game_type)
        
        search_paths = [
            os.path.join(game_model_dir, "final"),
            os.path.join(game_model_dir, "every_1000"),
            os.path.join(game_model_dir, "every_100")
        ]
        
        best_model = None
        max_round = -1
        
        for search_path in search_paths:
            if not os.path.exists(search_path):
                continue
                
            pattern = os.path.join(search_path, f"{game_type}_*.onnx")
            onnx_files = glob.glob(pattern)
            
            for f in onnx_files:
                basename = os.path.basename(f)
                # 匹配文件名最后的数字
                match = re.search(r'_(\d+)\.onnx$', basename)
                if match:
                    round_num = int(match.group(1))
                    if round_num > max_round:
                        max_round = round_num
                        best_model = f
            
            # 如果在优先级高的目录找到了模型，就停止搜索（假设高优先级的肯定更好，或者至少是我们想要的Final）
            # 但为了保险起见，我们还是遍历所有，找轮数最大的
            # 上面的循环已经涵盖了所有路径，max_round 会记录所有路径中最大的
        
        if best_model and os.path.exists(best_model):
            self.drawScrollText(f"自动加载最高轮次ONNX模型: {best_model} (轮次: {max_round})")
            # 强制使用 ONNX 模型
            onnx_net = PolicyValueNetONNX(self.game.boardWidth, self.game.boardHeight, best_model)
            
            # 选择 AI 策略
            if self.ai_decision_mode.get() == 'pure_nn':
                self.drawScrollText("使用模式: 纯策略网络 (无MCTS)")
                player = NNPlayer(policy_NN=onnx_net.policy_NN)
            else:
                self.drawScrollText("使用模式: MCTS 搜索")
                player = MCTSPlayer(policy_NN=onnx_net.policy_NN)
                
        else:
            self.drawScrollText(f"错误: 未找到 {game_type} 的可用ONNX模型！")
            self.drawScrollText(f"搜索路径: {game_model_dir}")
            self.drawScrollText("请先进行训练并导出ONNX模型，或者手动放置模型文件。")
            return

        self.game.playWithHuman(player, human_player_id=self.human_color.get(), check_stop=lambda: self.stop_training)

    def on_closing(self):
        self.stop_training = True
        self.window.destroy()

if __name__ == '__main__':
    metaZeta = MetaZeta()
