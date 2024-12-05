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
            currentCnt++;
            int t = currentCnt % 2;
            // 根据 Role 确定棋子颜色
            Brush brush = t switch {
                1 => Brushes.Black,
                0 => Brushes.Red,
                _ => Brushes.Transparent,
            };
            // 绘制棋子
            graphics.FillEllipse(brush,
                (float)(x - boardInfo.R),
                (float)(y - boardInfo.R),
                (float)(2 * boardInfo.R),
                (float)(2 * boardInfo.R));
            // 刷新 Panel 或 PictureBox
            if (mainForm.BoardPanel.InvokeRequired) {
                mainForm.BoardPanel.Invoke(new Action(() => mainForm.BoardPanel.Refresh()));
            } else {
                mainForm.BoardPanel.Refresh();
            }
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

    }
}
