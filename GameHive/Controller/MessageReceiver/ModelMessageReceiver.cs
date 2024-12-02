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
            //发出下棋信息
        }
        public void GameOver(Role role) {
            //发出游戏结束信息
        }
    }
}
