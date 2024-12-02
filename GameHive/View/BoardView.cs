using GameHive.Constants.RoleTypeEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.View {
    //控制棋盘上所有动画
    internal partial class View {
        //本剧已经落子数
        private int currentCnt = 0;
        //单位长度
        private double unitLenth;
        private Role first;
        //绘制一个棋盘
        public void DrowBoard(int Column, bool isCenter,Role first) {
            currentCnt = 0;
        }
        //画一颗棋子在数组坐标的x,y
        public void DrowChess(int x, int y, Role role) { }
    }
}
