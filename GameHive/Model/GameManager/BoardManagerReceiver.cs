/*************************************************************************************
 * 文 件 名:   BoardManagerReceiver.cs
 * 描    述: 棋盘管理类-接收信息
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:20
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        AbstractFactory factory;
        private Gobang88Factory gobang88Factory = null;
        private GobangFactory gobangFactory = null;
        private MisereTicTacToeFactory misere = null;
        private ReversiFactory reversiFactory = null;
        private TicTacToeFactory ticTacToeFactory = null;
        //玩家终止游戏
        public void UserEndGame() {
            gameRunning = false;
        }
        //玩家开始游戏
        public void StartGame() {
            //玩家开始游戏并且AI先手情况下，调用获取输入的逻辑由 controller 执行
            gameRunning = true;
        }
        //检查此处落子是否有效
        public bool CheckValid(int x, int y) {
            //不越界且未落子则有效
            if (x >= board.Count || y >= board[0].Count) return false;
            return board[x][y] == Role.Empty;
        }
        //用户设置先后手
        public void SetFirst(Role first) {
            this.first = first;
        }
        //用户在x,y下棋 返回是否结束
        public bool UserPalyChess(int x, int y) {
            board[x][y] = Role.Player;
            //处理落子结果
            Role winner = CheckGameOver();
            if (winner != Role.Empty) {
                //处理结束后工作
                Task.Run(() => SendGameOver(winner));
                return true;
            }
            return false;
        }

        //切换算法
        public GameBoardInfo SwitchGame(GameType gameType) {
            this.gameType = gameType;
            // 根据当前游戏类型选择对应的工厂
            factory = gameType switch {
                GameType.Gobang88 => gobang88Factory ??= Gobang88Factory.Instance,
                GameType.Gobang => gobangFactory ??= GobangFactory.Instance,
                GameType.MisereTicTacToe => misere ??= MisereTicTacToeFactory.Instance,
                GameType.Reversi => reversiFactory ??= ReversiFactory.Instance,
                GameType.TicTacToe => ticTacToeFactory ??= TicTacToeFactory.Instance,
                _ => throw new NotSupportedException($"Unsupported game type: {gameType}")
            };
            this.BoardInfo = factory.GetBoardInfoProduct();
            return BoardInfo;
        }
        public void SwitchAIType(AIAlgorithmType type) {
            this.aIAlgorithmType = type;
            // 根据 AI 类型从工厂获取对应的实例
            runningAI = type switch {
                AIAlgorithmType.DRL => factory.GetDRLProduct(),
                AIAlgorithmType.MCTS => factory.GetMCTSProduct(),
                AIAlgorithmType.AlphaBetaPruning => factory.GetMinMaxProduct(),
                AIAlgorithmType.Negamax => factory.GetNegamaxProduct(),
                _ => throw new NotSupportedException($"Unsupported AI algorithm type: {type}")
            };
        }

    }
}
