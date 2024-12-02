/*************************************************************************************
 * 文 件 名:   BoardManagerSender.cs
 * 描    述: 棋盘管理类-发送信息
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:20
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        //游戏运行结束，产生赢家
        private void SendGameOver(Role winner) {
            //通知控制层游戏结束并公布赢家
            controller.GameOver(winner);
        }
        //AI在x,y下棋
        private void SendAIPlayChess(int x, int y) {
            controller.ChessPlay(Role.AI, x, y);
        }
    }
}
