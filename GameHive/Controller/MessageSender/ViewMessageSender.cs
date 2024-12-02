using GameHive.Constants.RoleTypeEnum;
using GameHive.Model;

namespace GameHive.Controller {
    //用于向视图层发送命令
    internal partial class Controller {
        //增加一条记录
        private void ViewMessageAddLog(String log) {
            view.AddLog(log);
        }
        //logo弹窗
        private void ViewMessageLogoShow() {
            view.ShowLogo();
        }
        //在数组坐标系x,y位置下棋
        private void ViewMessagePlayChess(double x, double y, Role role) {
            view.DrawChess(x, y, Role.Player);
        }
        //绘制一张地图x*x 标号是否居中
        private void ViewMessageDrawMap(GameBoardInfo info) {
            view.DrawBoard(info);
        }
        //设定先手
        private void ViewMessageSetFirst(Role first) {
            view.SetFirst(first);
        }
        //开始游戏
        private void ViewMessageStartGame() {
            view.StartGame();
        }
        //结束游戏
        private void ViewMessageEndGame(Role role) {
            view.EndGame(role);
        }
    }
}
