﻿using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.GameInfo;

namespace GameHive.Controller {
    //用于向视图层发送命令
    internal partial class Controller {
        //增加一条记录
        private void ViewMessageLogMove(Role role, int x, int y) {
            view.LogMove(role, x, y);
        }

        //logo弹窗
        private void ViewMessageLogoShow() {
            view.ShowLogo();
        }
        //在数组坐标系x,y位置下棋
        private void ViewMessagePlayChess(double x, double y, Role role) {
            view.DrawChess(x, y, role);
        }
        
        //在数组坐标系x,y位置下棋（需要坐标转换）
        private void ViewMessagePlayChessFromArray(int arrayX, int arrayY, Role role) {
            // 从数组坐标转换为画布坐标
            var center = boardManager.BoardInfo.ChessCenter[arrayX][arrayY];
            double canvasX = center.Item1;
            double canvasY = center.Item2;
            view.DrawChess(canvasX, canvasY, role);
        }
        //绘制一张地图
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

        //开始匹配
        private void ViewMessageStartMatching() {
            view.StartMatching();
        }

        //终止匹配
        private void ViewMessageStopMatching() {
            view.StopMatching();
        }

        //切换先手
        private void ViewMessageSwitchGame(GameType game) {
            view.LogSwitchGame(game);
        }

        //切换算法
        private void ViewMessageSwitchAlgorithm(AIAlgorithmType algorithmType) {
            view.LogSwitchAlgorithm(algorithmType);
        }

        //设置对手名称
        private void ViewMessageSetOpponentName(string opponentName) {
            view.SetOpponentName(opponentName);
        }
    }
}
