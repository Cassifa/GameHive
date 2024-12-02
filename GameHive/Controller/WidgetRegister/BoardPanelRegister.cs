using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Controller {
    //用于注册棋盘点击事件事件
    internal partial class Controller {
        private void RegisterBoardPanel() {
            BoardPanelRegister += BoardPanelClick;
        }
        private void BoardPanelClick(object sender, EventArgs e) {
            // 游戏未开始，直接返回
            if (!boardManager.gameRunning) return;

            // 从事件参数中获取点击的鼠标位置
            var mouseEvent = (MouseEventArgs)e;
            double clickX = mouseEvent.X;
            double clickY = mouseEvent.Y;

            // 遍历中心点，找到是否点击了某合法位置
            //由于ChessCenter中存了对应xy可相应范围，且范围不会重叠，不用考虑坐标变化
            var chessCenters = boardManager.BoardInfo.ChessCenter;
            double r = boardManager.BoardInfo.R; // 可落子的半径
            for (int x = 0; x < chessCenters.Count; x++) {
                for (int y = 0; y < chessCenters[x].Count; y++) {
                    var center = chessCenters[x][y];
                    //此处xy在初始化时已经被变化坐标
                    double centerX = center.Item1;
                    double centerY = center.Item2;

                    // 判断是否在合法范围内
                    if (Math.Pow(clickX - centerX, 2) + Math.Pow(clickY - centerY, 2) <= r * r) {
                        //已经被下过棋则忽略
                        if (boardManager.board[x][y] != Role.Empty) return;
                        //给模型层发消息
                        UserPlayChess(x, y);
                        //给显示层发消息
                        PlayChess(centerX, centerY,Role.Player);
                        // 结束处理，防止重复匹配
                        return;
                    }
                }
            }
        }


    }
}
