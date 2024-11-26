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

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        //玩家终止游戏
        public void EndGame() {
            gameRunning = false;
        }
        //玩家开始游戏
        public void StartGame() {
            gameRunning = true;
        }
        //检查此处落子是否有效
        public bool CheckValid(int x, int y) {
            if (x >= board.Count || y >= board[0].Count) return false;
            return board[x][y] == 0;
        }
        //用户设置先后手
        public void SetFirst(Role first) {
            this.first = first;
        }
        //用户在x,y下棋
        public void UserPalyChess(int x, int y) {
            board[x][y] = (int)Role.Player;
        }
        //切换算法
        public void SwitchGame(GameType gameType) {

        }
        public void SwitchAIType(AIAlgorithmType type) {

        }
    }
}
