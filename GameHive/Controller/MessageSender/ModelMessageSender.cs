using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Controller {
    //用于向Model发送命令，并调用命令发送后相关的函数
    internal partial class Controller {
        //通知切换游戏类型
        private void switchGameType(GameType game) {
            //调用Model切换游戏类别并获取返回的GameInfo
            //用GameInfo更新AI列表
            //设置默认AI
            SetDefaultAI();
            //使用GameInfo命令View绘制地图

        }
        //通知切换算法
        private void switchAI(AIAlgorithmType type) {

        }
        //设定先后手
        private void setPlayerTurnOrder(Role role) {

        }
        //游戏开始
        private void startGame() {

        }
        //游戏终止
        private void endGame() { }
        //用户下棋位置
        private void userPlayChess(int x, int y) { }
    }
}
