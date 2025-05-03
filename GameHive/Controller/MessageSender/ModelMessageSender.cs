using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.GameInfo;
namespace GameHive.Controller {
    //用于向Model发送命令，并调用命令发送后相关的函数
    internal partial class Controller {
        //通知切换游戏类型
        private GameBoardInfo ModelMessageSwitchGameType(GameType game) {
            //调用Model切换游戏类别并获取返回的GameInfo
            return boardManager.SwitchGame(game);
        }
        //通知切换算法
        private ConcreteProductInfo ModelMessageSwitchAI(AIAlgorithmType type) {
            return boardManager.SwitchAIType(type);
        }
        //通知切换难度
        private void ModelMessageSwitchDifficulty(DifficultyLevel level) {
            boardManager.SwitchDifficulty(level);
        }
        //设定先后手
        private void ModelMessageSetPlayerTurnOrder(Role role) {
            //通知 boardManager
            boardManager.SetFirst(role);
        }
        //游戏开始
        private void ModelMessageStartGame(bool IsAIFirst) {
            //改变 boardManager状态，不允许再修改参数
            boardManager.StartGame(IsAIFirst);
        }
        //游戏终止
        private void ModelMessageEndGame() {
            //改变 boardManager状态，允许修改参数
            boardManager.UserEndGame();
        }
        //用户在此处下棋
        private bool ModelMessageUserPlayChess(int x, int y) {
            return boardManager.UserPalyChess(x, y);
        }
        //要求AI下棋
        private void ModelMessageAskAIMove(int lastX, int lastY) {
            boardManager.AskAIMove(lastX, lastY);
        }

        //检查是否合法
        private bool ModelMessageCheckValid(int x, int y) {
            return boardManager.CheckValid(x, y);
        }
    }
}
