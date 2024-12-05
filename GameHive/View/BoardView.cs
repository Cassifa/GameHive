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

        public void DrawBoard(GameBoardInfo boardInfo) {
            this.boardInfo = boardInfo;
            currentCnt = 0;
            // 初始化画布
            InitializeGraphics();
            // 绘制横线
            foreach (var row in boardInfo.Rows) {
                graphics.DrawLine(Pens.Black, (int)boardInfo.Bias, (float)row, (float)boardInfo.BoardLength, (float)row);
            }
            // 绘制纵线
            foreach (var col in boardInfo.Columns) {
                graphics.DrawLine(Pens.Black, (float)col, (int)boardInfo.Bias, (float)col, (float)boardInfo.BoardLength);
            }
            //绘制轴线
            if (boardInfo.IsCenter) DrawCenterAxis();
            else DrawUnCenterAxis();
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
                (float)(x - boardInfo.showR),
                (float)(y - boardInfo.showR),
                (float)(2 * boardInfo.showR),
                (float)(2 * boardInfo.showR));
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


        private void DrawCenterAxis() {
            int fontSize = (int)boardInfo.R / 6 * 3;
            var font = new Font("Arial", fontSize, FontStyle.Regular); // 坐标字体
            var brush = Brushes.Black; // 坐标字体颜色

            // 绘制横线和行坐标
            for (int i = 0; i < boardInfo.Rows.Count - 1; i++) {
                float row = (float)boardInfo.Rows[i];
                // 在横线左侧绘制行坐标，从上到下为 row-1 到 0
                string rowText = (boardInfo.Rows.Count - i - 1 - 1).ToString();
                graphics.DrawString(rowText, font, brush,
                    (float)boardInfo.Bias / 4 - (float)fontSize / 2,//x坐标
                    row - fontSize + (float)boardInfo.R); // y坐标
            }

            // 绘制纵线和列坐标
            for (int i = 0; i < boardInfo.Columns.Count - 1; i++) {
                float col = (float)boardInfo.Columns[i];
                char colChar = (char)('A' + i); // 动态生成字母列标
                string colText = $"{colChar}";
                graphics.DrawString(colText, font, brush,
                    col - fontSize + (float)boardInfo.R,//x坐标
                    (float)boardInfo.BoardLength + (float)boardInfo.Bias / 4 - (float)fontSize / 2); // y坐标
            }
        }
        private void DrawUnCenterAxis() {
            int fontSize = (int)boardInfo.R / 2;
            var font = new Font("Arial", fontSize, FontStyle.Regular); // 坐标字体
            var brush = Brushes.Black; // 坐标字体颜色

            // 绘制横线和行坐标
            for (int i = 0; i < boardInfo.Rows.Count; i++) {
                float row = (float)boardInfo.Rows[i];
                // 在横线左侧绘制行坐标，从上到下为 row-1 到 0
                string rowText;
                if (boardInfo.Rows.Count < 10)
                    rowText = (boardInfo.Rows.Count - i - 1).ToString();
                else rowText = (boardInfo.Rows.Count - i - 1).ToString().PadLeft(2, ' ');

                graphics.DrawString(rowText, font, brush,
                    0,//x坐标
                    row - fontSize); // y坐标
            }

            // 绘制纵线和列坐标
            for (int i = 0; i < boardInfo.Columns.Count; i++) {
                float col = (float)boardInfo.Columns[i];
                char colChar = (char)('A' + i); // 动态生成字母列标
                string colText = $"{colChar}";
                graphics.DrawString(colText, font, brush,
                    col - fontSize ,//x坐标
                    (float)boardInfo.BoardLength + (float)boardInfo.Bias / 2); // y坐标
            }

        }

    }
}
