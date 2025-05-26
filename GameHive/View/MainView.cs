/*************************************************************************************
 * 文 件 名:   ViewUtils.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/2 19:01
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.MainForm;

namespace GameHive.View {
    internal partial class View {
        private Controller.Controller controller;
        private DoubleBufferedForm mainForm;

        //设置先手
        public void SetFirst(Role role) {
            first = role;
        }

        //游戏开始
        public void StartGame() {
            ClearBoard();
            //处理组件显示
            mainForm.statusSwitch.Text = "终止游戏";
            mainForm.statusSwitch.BackColor = Color.Red;
            mainForm.firstTurn.Enabled = false;
            mainForm.secondTurn.Enabled = false;
            mainForm.AIType.Enabled = false;
            mainForm.DifficultySelector.Enabled = false;
            mainForm.GameModeSelector.Enabled = false;
            ClearMoveRecords();
        }

        //开始匹配
        public void StartMatching() {
            mainForm.statusSwitch.Text = "终止匹配";
            mainForm.statusSwitch.BackColor = Color.Red;
            mainForm.firstTurn.Enabled = false;
            mainForm.secondTurn.Enabled = false;
            mainForm.AIType.Enabled = false;
            mainForm.DifficultySelector.Enabled = false;
            mainForm.GameModeSelector.Enabled = false;
        }

        //终止匹配
        public void StopMatching() {
            mainForm.statusSwitch.Text = "开始匹配";
            mainForm.statusSwitch.BackColor = Color.Green;
            mainForm.firstTurn.Enabled = true;
            mainForm.secondTurn.Enabled = true;
            mainForm.AIType.Enabled = true;
            mainForm.DifficultySelector.Enabled = true;
            mainForm.GameModeSelector.Enabled = true;
        }

        //游戏结束或终止
        public void EndGame(Role role) {
            //处理组件显示
            mainForm.Invoke(new Action(() => {
                mainForm.statusSwitch.Text = "开始游戏";
                mainForm.statusSwitch.BackColor = Color.Green;
                mainForm.firstTurn.Enabled = true;
                mainForm.secondTurn.Enabled = true;
                mainForm.AIType.Enabled = true;
                mainForm.DifficultySelector.Enabled = true;
                mainForm.GameModeSelector.Enabled = true;
            }));
            LogWin(role);
            // 上传对局结果
            if (controller.CurrentGameMode != Constants.GameModeEnum.GameMode.LocalGame)
                UploadGameResult(role, first, controller.GetCurrentAIType(), controller.GetCurrentGameType());
        }

        //游戏结束
        private void GameOverShow(Role role) {
            string winner = "";
            switch (role) {
                case Role.Player:
                    winner = "玩家胜利";
                    break;
                case Role.AI:
                    winner = "AI获胜";
                    break;
                case Role.Draw:
                    winner = "平局";
                    break;
            }
            // 弹出提示框展示赢家
            MessageBox.Show(winner, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        //单例模式
#pragma warning disable CS8618
        private static View _instance;
#pragma warning restore CS8618 
        private View(Controller.Controller controller, DoubleBufferedForm form) {
            this.controller = controller;
            this.mainForm = form;
        }
        private static readonly object _lock = new object();
        public static View Instance(Controller.Controller controller, DoubleBufferedForm form) {
            if (_instance == null) {
                lock (_lock) {
                    if (_instance == null) {
                        _instance = new View(controller, form);
                    }
                }
            }
            return _instance;
        }
    }
}
