/*************************************************************************************
 * 文 件 名:   BoardManagerReceiver.cs
 * 描    述: 棋盘管理类-接收信息
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:20
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory;
using GameHive.Model.GameInfo;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        AbstractFactory factory;
        private Gobang88Factory gobang88Factory = null;
        private GobangFactory gobangFactory = null;
        private MisereTicTacToeFactory misere = null;
        private AntiGoFactory antiGoFactory = null;
        private TicTacToeFactory ticTacToeFactory = null;
        //玩家终止游戏
        public void UserEndGame() {
            gameRunning = false;
            board = null;
            //通知算法对象游戏被终止，释放资源，重新初始化自带资源
            runningAI.GameForcedEnd();
        }
        //玩家开始游戏
        public void StartGame(bool IsAIFirst) {
            Random random = new Random();
            RoundId = random.Next();
            //玩家开始游戏并且AI先手情况下，调用获取输入的逻辑由 controller 执行
            board = new List<List<Role>>();
            for (int i = 0; i < BoardInfo.Column; i++) {
                var row = new List<Role>();
                for (int j = 0; j < BoardInfo.Column; j++) {
                    row.Add(Role.Empty);
                }
                board.Add(row);
            }
            gameRunning = true;
            AIMoving = false;
            runningAI.GameStart(IsAIFirst);
        }

        //检查此处落子是否有效
        public bool CheckValid(int x, int y) {
            //不越界且未落子则有效
            if (x >= board.Count || y >= board[0].Count)
                return false;
            return board[x][y] == Role.Empty;
        }
        //要求AI移动
        public void AskAIMove(int lastX, int lastY) {
            LetAIMove(lastX, lastY);
        }
        //用户设置先后手
        public void SetFirst(Role first) {
            this.first = first;
        }
        //用户在x,y下棋 返回是否结束
        public bool UserPalyChess(int x, int y) {
            //此线程与算法线程天然互斥
            runningAI.UserPlayPiece(x, y);
            return PlayChess(Role.Player, x, y);
            //TODO: 游戏无法判断玩家平局 AI智力降低
        }

        //切换游戏
        public GameBoardInfo SwitchGame(GameType gameType) {
            this.gameType = gameType;
            // 根据当前游戏类型选择对应的工厂
            factory = gameType switch {
                GameType.Gobang88 => gobang88Factory ??= Gobang88Factory.Instance,
                GameType.Gobang => gobangFactory ??= GobangFactory.Instance,
                GameType.MisereTicTacToe => misere ??= MisereTicTacToeFactory.Instance,
                GameType.AntiGo => antiGoFactory ??= AntiGoFactory.Instance,
                GameType.TicTacToe => ticTacToeFactory ??= TicTacToeFactory.Instance,
                _ => throw new NotSupportedException($"不支持此游戏类型: {gameType}")
            };
            //更新为新游戏的BoardInfo并返回
            this.BoardInfo = factory.GetBoardInfoProduct();
            return BoardInfo;
        }
        //切换算法类型
        public ConcreteProductInfo SwitchAIType(AIAlgorithmType type) {
            this.aIAlgorithmType = type;
            // 根据 AI 类型从工厂获取对应的实例
            runningAI = type switch {
                AIAlgorithmType.MCTS => factory.GetMCTSProduct(),
                AIAlgorithmType.Minimax => factory.GetMinMaxProduct(),
                AIAlgorithmType.Negamax => factory.GetNegamaxProduct(),
                AIAlgorithmType.DeepRL => factory.GetDeepRLProduct(),
                _ => throw new NotSupportedException($"不支持此算法类型: {type}")
            };
            this.ConcreteProductInfo = factory.GetConcreteProductInfo();
            return ConcreteProductInfo;
        }

        //切换游戏难度
        public void SwitchDifficulty(DifficultyLevel level) {
            //根据 AI 类型从工厂获取对应的实例
            if (level > ConcreteProductInfo.MaximumLevel)
                throw new NotSupportedException($"不支持此难度类型: {level.GetChineseName()}");
            runningAI = this.aIAlgorithmType switch {
                AIAlgorithmType.MCTS => factory.GetMCTSProduct(level),
                AIAlgorithmType.Minimax => factory.GetMinMaxProduct(level),
                AIAlgorithmType.Negamax => factory.GetNegamaxProduct(level),
                AIAlgorithmType.DeepRL => factory.GetDeepRLProduct(level),
                _ => throw new NotSupportedException($"不支持此算法类型: {this.aIAlgorithmType}")
            };
        }

    }
}
