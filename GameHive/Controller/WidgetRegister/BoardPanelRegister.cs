/*************************************************************************************
 * 文 件 名:   BoardPanelRegister.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:30
*************************************************************************************/
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
            //由于ChessCenter中存了对应(x,y)可相应范围，且范围不会重叠，不用考虑坐标变化
            var chessCenters = boardManager.BoardInfo.ChessCenter;
            double r = boardManager.BoardInfo.R; // 可落子的半径
            for (int x = 0; x < chessCenters.Count; x++) {
                for (int y = 0; y < chessCenters[x].Count; y++) {
                    //棋盘x,y变化坐标后对应的物理可点击位置中心
                    var center = chessCenters[x][y];
                    double centerX = center.Item1;
                    double centerY = center.Item2;
                    // 判断是否在合法范围内
                    if (Math.Pow(clickX - centerX, 2) + Math.Pow(clickY - centerY, 2) <= r * r) {
                        //已经被下过棋则忽略
                        if (!ModelMessageCheckValid(x,y)) return;
                        //给显示层发消息
                        ViewMessagePlayChess(centerX, centerY, Role.Player);
                        //给模型层发消息,并获取是否结束
                        bool isEnd = ModelMessageUserPlayChess(x, y);
                        if (!isEnd) {
                            //没有结束，令AI计算下一步,异步计算
                            Task.Run(() => ModelMessageAskAIMove());
                        }
                        return;
                    }
                }
            }
        }


    }
}
