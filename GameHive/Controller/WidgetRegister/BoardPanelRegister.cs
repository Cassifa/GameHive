﻿/*************************************************************************************
 * 文 件 名:   BoardPanelRegister.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:30
*************************************************************************************/
using GameHive.Constants.GameModeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils;
namespace GameHive.Controller {
    //用于注册棋盘点击事件事件
    internal partial class Controller {
        private void RegisterBoardPanel() {
            BoardPanelRegister += BoardPanelClick;
        }
        private void BoardPanelClick(object sender, EventArgs e) {
            // 根据游戏模式检查状态
            if (CurrentGameMode == GameMode.LocalGame) {
                // 本地对战模式检查
                if (!boardManager.gameRunning || boardManager.AIMoving)
                    return;
            } else {
                // 联机或大模型对战模式检查
                if (gameSession == null || !gameSession.IsMyTurn)
                    return;
                // 确保棋盘已初始化
                if (!boardManager.gameRunning) {
                    boardManager.StartGame(false);  // 传入false表示不启动AI
                }
            }

            // 从事件参数中获取点击的鼠标位置
            var mouseEvent = (MouseEventArgs)e;
            double clickX = mouseEvent.X;
            double clickY = mouseEvent.Y;

            // 遍历中心点，找到是否点击了某合法位置
            //由于ChessCenter中存了对应(x,y)可相应范围，且范围不会重叠，不用考虑坐标变化
            var chessCenters = boardManager.BoardInfo.ChessCenter;
            double r = (double)boardManager.BoardInfo.InputR; // 可落子的半径
            for (int x = 0; x < chessCenters.Count; x++) {
                for (int y = 0; y < chessCenters[x].Count; y++) {
                    //棋盘x,y变化坐标后对应的物理可点击位置中心
                    var center = chessCenters[x][y];
                    double centerX = center.Item1;
                    double centerY = center.Item2;
                    // 判断是否在合法范围内
                    if (Math.Pow(clickX - centerX, 2) + Math.Pow(clickY - centerY, 2) <= r * r) {
                        //启用模拟，修改下棋点位
                        if (RecordSimulateUtil.ActiveSimulate) {
                            RecordSimulateUtil.SimulateKillBoardController(ref x, ref y);
                            centerX = chessCenters[x][y].Item1;
                            centerY = chessCenters[x][y].Item2;
                        }
                        //已经被下过棋则忽略
                        if (!ModelMessageCheckValid(x, y))
                            return;

                        if (CurrentGameMode == GameMode.LocalGame) {
                            //本地对战模式
                            //给显示层发消息
                            ViewMessagePlayChess(centerX, centerY, Role.Player);
                            ViewMessageLogMove(Role.Player, x, y);
                            //给模型层发消息,并获取是否结束
                            bool isEnd = ModelMessageUserPlayChess(x, y);
                            if (!isEnd) {
                                //没有结束，令AI计算下一步,异步计算
                                Thread aiThread = new Thread(() => ModelMessageAskAIMove(x, y));
                                aiThread.Start();
                            }
                        } else {
                            //联机或大模型对战模式
                            //只发送落子消息到服务器，不立即显示棋子
                            //等待服务器通过OnOpponentMove事件确认后再显示
                            gameSession.SendMoveAsync(x, y).Wait();
                        }
                        return;
                    }
                }
            }
        }


    }
}
