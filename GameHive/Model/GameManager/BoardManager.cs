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
using GameHive.Controller;
using Microsoft.VisualBasic.Devices;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        //控制层实例
        private Controller.Controller controller;
        //当前在玩的游戏
        private GameType gameType;
        //当前在使用的AI算法
        private AIAlgorithmType aIAlgorithmType;
        public bool gameRunning { get; private set; }
        public Role first { get; set; }

        //当前在运行的AI产品实例
        private AbstractAIStrategy runningAI;
        //游戏情况
        public GameBoardInfo BoardInfo { get; private set; }
        //当前棋盘
        public List<List<Role>> board { get; private set; }

        //检查是否结束 返回赢家，如果没人赢返回Role.Empty
        public Role CheckGameOver() {
            return runningAI.CheckGameOver(board);
        }

        //执行下一步
        public void Move() { 
            GetNextAIMove();
        }


        //获取AI下一步输出
        private void GetNextAIMove(){
            //获取下一步
            Tuple<int,int> nextMove=runningAI.GetNextAIMove(board);
            //记录下一步
            board[nextMove.Item1][nextMove.Item2] = Role.AI;
            SendAIPlayChess(nextMove.Item1, nextMove.Item2);
            //检查游戏是否结束
            Role winner=CheckGameOver();
            if (winner != Role.Empty) {
                SendGameOver(winner);
            }
        }


        //单例模式
#pragma warning disable CS8618 
        private static BoardManager _instance;
#pragma warning restore CS8618 
        private BoardManager(Controller.Controller controller) {
            this.controller = controller;
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
