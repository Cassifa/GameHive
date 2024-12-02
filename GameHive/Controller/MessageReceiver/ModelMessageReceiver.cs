/*************************************************************************************
 * 文 件 名:   ModelMessageReceiver.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 3:36
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Controller {
    //用于接Model的消息
    internal partial class Controller {
        //在x,y落子
        public void ChessPlay(Role role, int x, int y) {
            //向 View 转发下棋信息
            view.DrawChess(x, y, role);
        }
        public void GameOver(Role role) {
            EndGame(role);
        }
    }
}
