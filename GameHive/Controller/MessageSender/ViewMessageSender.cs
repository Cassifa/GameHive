using GameHive.Constants.RoleTypeEnum;
using GameHive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Controller {
    //用于向视图层发送命令
    internal partial class Controller {
        //增加一条记录
        private void ViewMessageAddLog(String log) {
            view.AddLog(log);
        }
        //logo弹窗
        private void ViewMessageLogoShow() { 
            
        }
        //在x,y 下棋
        private void ViewMessagePlayChess(double x,double y,Role role) { 

        }
        //绘制一张地图x*x 标号是否居中
        private void ViewMessageDrawMap(GameBoardInfo info) { }
        //设定先手
        private void ViewMessageSetFirst(Role first) { }
        //开始游戏
        private void ViewMessageStartGame() { }
        //结束游戏
        private void ViewMessageEndGame() { }
    }
}
