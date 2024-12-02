using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model;
namespace GameHive.Controller {
    //用于向Model发送命令，并调用命令发送后相关的函数
    internal partial class Controller {
        //通知切换游戏类型
        private GameBoardInfo ModelMessageSwitchGameType(GameType game) {
            //调用Model切换游戏类别并获取返回的GameInfo
            return boardManager.SwitchGame(game);
        }
        //通知切换算法
        private void ModelMessageSwitchAI(AIAlgorithmType type) {
            boardManager.SwitchAIType(type);
        }
        //设定先后手
        private void ModelMessageSetPlayerTurnOrder(Role role) {
            //通知 boardManager
            boardManager.first = role;
        }
        //游戏开始
        private void ModelMessageStartGame() {
            //改变 boardManager状态，不允许再修改参数
            boardManager.StartGame();
        }
        //游戏终止
        private void ModelMessageEndGame() {
            //改变 boardManager状态，允许修改参数
            boardManager.UserEndGame();
        }
        //用户下棋位置
        private bool ModelMessageUserPlayChess(int x, int y) {
            return boardManager.UserPalyChess(x, y);
        }
    }
}
