/*************************************************************************************
 * 文 件 名:   ModelMessageReceiver.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 3:36
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Controller {
    //用于接Model的消息
    internal partial class Controller {
        //在x,y落子
        public void ChessPlay(int x, int y) {
            var center = boardManager.BoardInfo.ChessCenter[x][y];
            double centerX = center.Item1;
            double centerY = center.Item2;
            //向 View 转发下棋信息
            view.DrawChess(centerX, centerY, Role.AI);
            ViewMessageLogMove(Role.AI, y, x);
        }
        public void GameOver(Role role) {
            EndGame(role);
        }
    }
}
