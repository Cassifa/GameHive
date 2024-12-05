/*************************************************************************************
 * 文 件 名:   BoardView.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 19:03
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model;
using System.Drawing;

namespace GameHive.View {
    //控制棋盘上所有动画
    internal partial class View {
        //本局已经落子数
        private int currentCnt = 0;
        //单位长度
        private double unitLength;
        private Role first;
        private GameBoardInfo boardInfo;
        private Graphics? graphics;
        private Bitmap? boardBitmap;
        //绘制一个棋盘
        public void DrawBoard(GameBoardInfo boardInfo) {
            this.boardInfo = boardInfo;
            currentCnt = 0;
            // 初始化画布
            InitializeGraphics();
            if (graphics == null) return;
            // 绘制横线
            foreach (var row in boardInfo.Rows) {
                graphics.DrawLine(Pens.Black, (int)boardInfo.Bias, (float)row, (float)boardInfo.BoardLength, (float)row);
            }
            // 绘制纵线
            foreach (var col in boardInfo.Columns) {
                graphics.DrawLine(Pens.Black, (float)col, (int)boardInfo.Bias, (float)col, (float)boardInfo.BoardLength);
            }
            // 在 Panel 或 PictureBox 上显示棋盘
            mainForm.BoardPanel.BackgroundImage = boardBitmap;
        }
        //画一颗棋子在数组坐坐标系的x,y落子
        public void DrawChess(double x, double y, Role role) {
            if (graphics == null || boardBitmap == null) return;

            // 根据 Role 确定棋子颜色
            Brush brush = role switch {
                Role.Player => Brushes.Black,
                Role.AI => Brushes.White,
                _ => Brushes.Transparent,
            };
            // 绘制棋子
            graphics.FillEllipse(brush,
                (float)(x - boardInfo.R),
                (float)(y - boardInfo.R),
                (float)(2 * boardInfo.R),
                (float)(2 * boardInfo.R));
            // 刷新 Panel 或 PictureBox
            mainForm.BoardPanel.Refresh();
        }
        private void InitializeGraphics() {
            // 创建画布
            boardBitmap = new Bitmap((int)boardInfo.totalSize, (int)boardInfo.totalSize);
            graphics = Graphics.FromImage(boardBitmap);
            graphics.Clear(Color.White);
        }
        // 清空棋盘
        public void ClearBoard() {
            if (graphics == null || boardBitmap == null) return;

            graphics.Clear(Color.White);
            DrawBoard(boardInfo); // 重新绘制空棋盘
            mainForm.BoardPanel.Refresh();
        }

        //设置先手
        public void SetFirst(Role role) {
            first = role;
        }
        //游戏结束
        public void GameOver(Role role) {
            string winner = "";
            // 判断赢家
            switch (role) {
                case Role.Player:
                    winner = "Player 1 wins!";
                    break;
                case Role.AI:
                    winner = "AI 2 wins!";
                    break;
                case Role.Empty:
                    winner = "It's a draw!";
                    break;
                default:
                    winner = "Game Over!";
                    break;
            }
            // 弹出提示框展示赢家
            MessageBox.Show(winner, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

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

    }
}
