/*************************************************************************************
 * 文 件 名:   LoginRegister.cs
 * 描    述: 
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 2:56
*************************************************************************************/
using GameHive.MainForm;
using GameHive.Net;

namespace GameHive.Controller {
    internal partial class Controller {
        private void RegisterLoginStatus(DoubleBufferedForm mainForm) {
            // 设置登录状态标签的初始状态
            mainForm.loginStatus.Text = "点此登录";
            mainForm.loginStatus.AutoSize = true;
            mainForm.loginStatus.Cursor = Cursors.Hand;

            // 注册事件处理
            LoginStatusRegister += LoginStatus_Click;

            // 如果已经登录，更新显示
            if (UserInfo.Instance.IsLoggedIn) {
                UpdateLoginStatus();
            }
        }

        private async void LoginStatus_Click(object sender, EventArgs e) {
            if (UserInfo.Instance.IsLoggedIn) {
                ShowUserInfoDialog();
                return;
            }

            // 创建登录对话框
            using (var loginDialog = new Form {
                Width = 400,
                Height = 300,
                Text = "用户登录",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                Padding = new Padding(20)
            }) {
                var panel = new TableLayoutPanel {
                    Dock = DockStyle.Fill,
                    RowCount = 4,
                    ColumnCount = 2,
                    Padding = new Padding(20)
                };

                // 用户名输入框
                var usernameLabel = new Label {
                    Text = "用户名:",
                    TextAlign = ContentAlignment.MiddleRight,
                    Dock = DockStyle.Fill
                };
                var usernameBox = new TextBox {
                    Width = 200,
                    Height = 30,
                    Font = new Font("Microsoft YaHei", 10),
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                // 密码输入框
                var passwordLabel = new Label {
                    Text = "密码:",
                    TextAlign = ContentAlignment.MiddleRight,
                    Dock = DockStyle.Fill
                };
                var passwordBox = new TextBox {
                    Width = 200,
                    Height = 30,
                    PasswordChar = '●',
                    Font = new Font("Microsoft YaHei", 10),
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                // 按钮面板
                var buttonFlowPanel = new FlowLayoutPanel {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    AutoSize = true
                };

                // 登录按钮
                var loginButton = new Button {
                    Text = "登录",
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Height = 35,
                    Font = new Font("Microsoft YaHei", 10),
                    BackColor = Color.FromArgb(0, 122, 204),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(5)
                };
                loginButton.FlatAppearance.BorderSize = 0;

                // 取消按钮
                var cancelButton = new Button {
                    Text = "取消",
                    DialogResult = DialogResult.Cancel,
                    Width = 100,
                    Height = 35,
                    Font = new Font("Microsoft YaHei", 10),
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(5)
                };

                // 添加控件到面板
                panel.Controls.Add(usernameLabel, 0, 0);
                panel.Controls.Add(usernameBox, 1, 0);
                panel.Controls.Add(passwordLabel, 0, 1);
                panel.Controls.Add(passwordBox, 1, 1);

                buttonFlowPanel.Controls.Add(loginButton);
                buttonFlowPanel.Controls.Add(cancelButton);
                panel.Controls.Add(buttonFlowPanel, 1, 2);

                // 添加面板到对话框
                loginDialog.Controls.Add(panel);
                loginDialog.AcceptButton = loginButton;
                loginDialog.CancelButton = cancelButton;

                // 显示对话框
                if (loginDialog.ShowDialog() == DialogResult.OK) {
                    // 尝试登录
                    var client = new GameHiveClient();
                    var result = await client.LoginAsync(usernameBox.Text, passwordBox.Text);

                    // 只检查code是否为200
                    if (result.Code == 200) {
                        // 登录成功
                        UpdateLoginStatus();
                    } else {
                        // 登录失败
                        MessageBox.Show(result.Msg, "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void ShowUserInfoDialog() {
            using (var userInfoDialog = new Form {
                Width = 250,
                Height = 180,
                Text = "用户信息",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            }) {
                // 用户名
                var userLabel = new Label {
                    Text = UserInfo.Instance.UserName,
                    Font = new Font("Microsoft YaHei", 10),
                    Width = 250,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 40)
                };

                // 退出登录按钮
                var logoutButton = new Button {
                    Text = "退出登录",
                    Width = 100,
                    Height = 35,
                    Font = new Font("Microsoft YaHei", 10),
                    BackColor = Color.FromArgb(220, 53, 69),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(75, 80)
                };
                logoutButton.FlatAppearance.BorderSize = 0;

                // 注册按钮点击事件
                logoutButton.Click += (s, ev) => {
                    UserInfo.Instance.Clear();
                    UpdateLoginStatus();
                    userInfoDialog.Close();
                };

                userInfoDialog.Controls.Add(userLabel);
                userInfoDialog.Controls.Add(logoutButton);
                userInfoDialog.ShowDialog();
            }
        }

        private void UpdateLoginStatus() {
            // Check if invoking is required (i.e., called from a non-UI thread)
            if (mainForm.loginStatus.InvokeRequired) {
                mainForm.loginStatus.Invoke(new MethodInvoker(delegate {
                    UpdateLoginStatusUI();
                }));
            } else {
                UpdateLoginStatusUI();
            }
        }

        // Helper method to perform the actual UI update
        private void UpdateLoginStatusUI() {
            if (UserInfo.Instance.IsLoggedIn) {
                mainForm.loginStatus.Text = UserInfo.Instance.UserName; // Use UserName
                mainForm.loginStatus.Cursor = Cursors.Hand;
            } else {
                mainForm.loginStatus.Text = "点此登录";
                mainForm.loginStatus.Cursor = Cursors.Hand;
            }
        }
    }
}
