using GameHive.Constants.RoleTypeEnum;
using GameHive.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.View {
    //控制棋盘上所有动画
    internal partial class View {
        //本局已经落子数
        private int currentCnt = 0;
        //单位长度
        private double unitLength;
        private Role first;
        //绘制一个棋盘
        public void DrawBoard(GameBoardInfo boardInfo) {
            currentCnt = 0;
        }
        //画一颗棋子在数组坐坐标系的x,y落子
        public void DrawChess(double x, double y, Role role) { }
    }
}
