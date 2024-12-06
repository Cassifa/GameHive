/*************************************************************************************
 * 文 件 名:   LogoView.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 19:04
*************************************************************************************/
namespace GameHive.View {
    //控制校徽的动画
    internal partial class View {
        public void ShowLogo() {
            // 创建一个新窗体用于显示Logo和提示文字
            Form logoForm = new Form {
                Text = "作者信息",
                Size = new Size(400, 400),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };

            // 添加图片框
            PictureBox pictureBox = new PictureBox {
                Image = Properties.Resources.logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Top,
                Height = 200
            };
            // 添加超链接标签
            LinkLabel linkLabel = new LinkLabel {
                Text = "作者GitHub主页：https://github.com/Cassifa",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 添加点击事件以实现跳转功能
            linkLabel.Click += (sender, e) => {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
                    FileName = "https://github.com/Cassifa",
                    UseShellExecute = true
                });
            };
            logoForm.Controls.Add(linkLabel);
            logoForm.Controls.Add(pictureBox);

            // 显示窗体
            logoForm.ShowDialog();
        }
    }
}

