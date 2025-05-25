/*************************************************************************************
 * 文 件 名:   MainController.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:04
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.GameModeEnum;
using GameHive.Constants.GameStatusEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.MainForm;
using GameHive.Model.GameInfo;
using GameHive.Model.GameManager;
using GameHive.Net;

namespace GameHive.Controller {
    //这个函数用于执行控制器生命周期主流程
    internal partial class Controller {
        private DoubleBufferedForm mainForm;
        private BoardManager boardManager;
        private View.View view;
        public GameMode CurrentGameMode { get; private set; } = GameMode.LocalGame;
        public GameStatus CurrentGameStatus { get; private set; } = GameStatus.NotStarted;

        //联机游戏相关字段
        private GameSession gameSession;
        private bool isMyTurn = false;

        public Controller(DoubleBufferedForm mainForm) {
            //绑定页面、视图层、模型层
            this.mainForm = mainForm;
            boardManager = BoardManager.Instance(this);
            view = View.View.Instance(this, mainForm);
            Init();
        }
        private void Init() {
            //注册除运行算法外所有用户点击事件
            RegisterEvent();
            //初始化界面选择状态
            SetupInitialState();
        }

        //设定初始状态 0号游戏，0号AI，先手
        private void SetupInitialState() {
            //初始化先后手（触发一个点击先手事件）
            //FirstTurnCheckedChanged(null, null);
            mainForm.firstTurn.Checked = true;
            //初始化选择的棋盘（触发一次点击第一个菜单事件）
            MenuStrip menuStrip = mainForm.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (menuStrip != null && menuStrip.Items.Count > 0) {
                // 获取第一个菜单项并触发点击事件
                ToolStripMenuItem firstMenuItem = menuStrip.Items[0] as ToolStripMenuItem;
                firstMenuItem?.PerformClick();
            }
        }

        //切换游戏种类
        private void SwitchGame(GameType gameType) {
            //切换AI并获取 boardInfo信息
            GameBoardInfo boardInfo = ModelMessageSwitchGameType(gameType);
            //用GameInfo去AI注册器更新AI列表
            RegisterAIType(boardInfo);
            //重新设置默认AI
            SetDefaultAI();
            //使用GameInfo命令View绘制地图,先后手保持不变
            ViewMessageDrawMap(boardInfo);
            //加入历史记录
            ViewMessageSwitchGame(gameType);
        }

        //设置默认出战AI（GameInfo的第一个）触发点击事件
        private void SetDefaultAI() {
            // 获取默认的 AI 类型（第一个可用的 AI 类型）
            var defaultAI = boardManager.BoardInfo.AllAIType[0];
            // 获取默认 AI 的中文名称
            var defaultAIName = defaultAI.GetChineseName();
            // 查找 ComboBox 中对应的项并设置为选中
            int defaultIndex = mainForm.AIType.Items.IndexOf(defaultAIName);
            if (defaultIndex >= 0) {
                mainForm.AIType.SelectedIndex = defaultIndex; // 触发 SelectedIndexChanged 事件
                //通知模型切换
                ConcreteProductInfo info = ModelMessageSwitchAI(defaultAI);
                //根据切换来的默认AI信息选择默认难度
                RegisterDifficultySelector(info);
                SetDefaultDifficulty();
            }
        }

        //设置默认难度
        private void SetDefaultDifficulty() {
            // 获取默认的难度等级（第一个可用的难度等级）
            DifficultyLevel defaultLevel = boardManager.ConcreteProductInfo.DifficultyLevels[0];
            // 获取默认难度的中文名称
            var defaultLevelName = defaultLevel.GetChineseName();
            // 查找 ComboBox 中对应的项并设置为选中
            int defaultIndex = mainForm.DifficultySelector.Items.IndexOf(defaultLevelName);
            if (defaultIndex >= 0) {
                mainForm.DifficultySelector.SelectedIndex = defaultIndex; // 触发 SelectedIndexChanged 事件
                //通知模型切换
                ModelMessageSwitchDifficulty(defaultLevel);
            }
        }

        private async void StartGame() {
            if (CurrentGameMode == GameMode.LocalGame) {
                CurrentGameStatus = GameStatus.Playing;
                ModelMessageStartGame(boardManager.first == Role.AI);
                ViewMessageStartGame();
                if (boardManager.first == Role.AI)
                    ModelMessageAskAIMove(-1, -1);
            } else {
                try {
                    CurrentGameStatus = GameStatus.Matching;
                    string wsUrl = "ws://localhost:3000";
                    gameSession = new GameSession(wsUrl, UserInfo.Instance.UserId.ToString());
                    gameSession.OnGameStart += (s, e) => {
                        CurrentGameStatus = GameStatus.Playing;
                        isMyTurn = e.IsFirst;
                        ViewMessageStartGame();
                    };
                    gameSession.OnOpponentMove += (s, e) => {
                        ViewMessagePlayChess(e.X, e.Y, Role.AI);
                        ViewMessageLogMove(Role.AI, e.Y, e.X);
                        isMyTurn = true;
                    };
                    gameSession.OnGameResult += (s, e) => EndGame(e.Winner);
                    gameSession.OnError += (s, e) => {
                        MessageBox.Show("游戏发生错误，请重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    };

                    ViewMessageStartMatching();
                    await gameSession.StartSessionAsync(GetCurrentGameType().GetChineseName(), CurrentGameMode ==GameMode.LMMGame);
                } catch (Exception ex) {
                    CurrentGameStatus = GameStatus.NotStarted;
                }
            }
        }

        private async void EndGame(Role role) {
            if (CurrentGameMode == GameMode.LocalGame) {
                CurrentGameStatus = GameStatus.NotStarted;
                ModelMessageEndGame();
                ViewMessageEndGame(role);
            } else {
                try {
                    if (gameSession != null) {
                        await gameSession.EndSessionAsync();
                        gameSession = null;
                    }
                    CurrentGameStatus = GameStatus.NotStarted;
                    ViewMessageEndGame(role);
                } catch (Exception) {
                    // 如果发生错误，强制清理
                    gameSession = null;
                    CurrentGameStatus = GameStatus.NotStarted;
                    ViewMessageEndGame(role);
                }
            }
        }

        public AIAlgorithmType GetCurrentAIType() {
            return boardManager.aIAlgorithmType;
        }

        public GameType GetCurrentGameType() {
            return boardManager.gameType;
        }
    }
}
