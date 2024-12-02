using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Controller {
    //用于接Model的消息
    internal partial class Controller {
        //在x,y落子
        public void ChessPlay(Role role, int x, int y) {
            //像 View 发出下棋信息
            view.DrowChess(x, y,role);
        }
        public void GameOver(Role role) {
            //像 View 转发游戏结束信息
            view.GameOver(role);
        }
    }
}
