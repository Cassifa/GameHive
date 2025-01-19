/*************************************************************************************
 * 文 件 名:   BoardManager.cs
 * 描    述:  棋盘管理类，单例，用于屏蔽所有交互，根据策略模式调取AI下一步操作
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:19
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        //控制层实例
        private Controller.Controller controller;
        //当前在玩的游戏
        private GameType gameType;
        //当前在使用的AI算法
        private AIAlgorithmType aIAlgorithmType;
        //当前运行游戏的代码
        private int RoundId;
        //AI正在决策
        public bool AIMoving { get; private set; }
        public bool gameRunning { get; private set; }
        public Role first { get; set; }

        //当前在运行的AI产品实例
        private AbstractAIStrategy runningAI;
        //游戏情况
        public GameBoardInfo BoardInfo { get; private set; }
        //当前棋盘
        public List<List<Role>> board { get; private set; }

        //获取AI下一步输出
        private void LetAIMove(int lastX, int lastY) {

            AIMoving = true;
            //记录本次ID
            int currentID = RoundId;
            //获取下一步
            Tuple<int, int> nextMove = runningAI.GetNextAIMove(board, lastX, lastY);
            AIMoving = false;
            //记录下一步,由于耗时可能很长，先判断本局游戏有没有被终止
            if (currentID == RoundId && gameRunning) {
                //通知控制层AI的决策
                SendAIPlayChess(nextMove.Item1, nextMove.Item2);
                PlayChess(Role.AI, nextMove.Item1, nextMove.Item2);
            }
        }

        //下棋，并返回此次下棋是否导致游戏终止 若终止会触发游戏结束事件
        private bool PlayChess(Role role, int x, int y) {
            board[x][y] = role;
            //处理落子结果
            Role winner = runningAI.CheckGameOverByPiece(board,x,y);
            //胜者不为空说明 AI胜利/玩家胜利/平局
            if (winner != Role.Empty) {
                //处理结束后工作
                Task.Run(() => SendGameOver(winner));
                return true;
            }
            return false;
        }


        //单例模式
#pragma warning disable CS8618 
        private static BoardManager _instance;
#pragma warning restore CS8618 
        private BoardManager(Controller.Controller controller) {
            this.controller = controller;
            AIMoving = false;
        }
        private static readonly object _lock = new object();
        public static BoardManager Instance(Controller.Controller controller) {
            if (_instance == null) {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = new BoardManager(controller);
                    }
                }
            }
            return _instance;
        }

    }
}
