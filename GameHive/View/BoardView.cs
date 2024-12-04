/*************************************************************************************
 * 文 件 名:   BoardView.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 19:03
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model;

namespace GameHive.View {
    //控制棋盘上所有动画
    internal partial class View {
        //本局已经落子数
        private int currentCnt = 0;
        //单位长度
        private double unitLength;
        private Role first;
        private GameBoardInfo boardInfo;

        //设置先手
        public void SetFirst(Role role) {
            first = role;
        }
        //游戏结束
        public void GameOver(Role role) { }
        //开启游戏
        public void StartGame() {
            //处理组件显示
            mainForm.statusSwitch.Text = "终止游戏";
            mainForm.statusSwitch.BackColor = Color.Red;
            mainForm.firstTurn.Enabled = false;
            mainForm.secondTurn.Enabled = false;
            mainForm.AIType.Enabled = false;
        }
        public void EndGame(Role role) {
            //处理组件显示
            mainForm.statusSwitch.Text = "开始游戏";
            mainForm.statusSwitch.BackColor = Color.Green;
            mainForm.firstTurn.Enabled = true;
            mainForm.secondTurn.Enabled = true;
            mainForm.AIType.Enabled = true;
        }
        //绘制一个棋盘
        public void DrawBoard(GameBoardInfo boardInfo) {
            this.boardInfo = boardInfo;
            currentCnt = 0;
        }
        //画一颗棋子在数组坐坐标系的x,y落子
        public void DrawChess(double x, double y, Role role) { }
    }
}
