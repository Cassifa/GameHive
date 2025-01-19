/*************************************************************************************
 * 文 件 名:   LogView.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 19:04
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.View {
    //控制历史记录条目
    internal partial class View {
        public void AddLog(Color color, string logContext) {
            logContext = " " + logContext;
            if (mainForm.LogListBox.InvokeRequired) {
                // 跨线程更新控件
                mainForm.LogListBox.Invoke(new Action(() => {
                    mainForm.LogListBox.Items.Add(new ColoredListItem(logContext, color));
                    mainForm.LogListBox.TopIndex = mainForm.LogListBox.Items.Count - 1;
                }));
            } else {
                mainForm.LogListBox.Items.Add(new ColoredListItem(logContext, color));
                mainForm.LogListBox.TopIndex = mainForm.LogListBox.Items.Count - 1;
            }
        }
        //下棋
        public void LogMove(Role role, int x, int y) {
            string name = role.GetChineseName();
            char colChar = (char)('A' + x);
            if (name == "AI") name = "  AI  ";
            string logMessage = $"{name} 在 ({colChar}, {boardInfo.Column - 1 - y}) 下棋了";
            AddLog(Color.Black, logMessage);
        }

        //游戏结束
        private void LogWin(Role role) {
            if (role == Role.Empty) {
                string message = $"游戏终止";
                AddLog(Color.Gray, message);
                return;
            }
            string logMessage = $"{role.GetChineseName()}获胜！";
            if (role == Role.Draw)
                logMessage = $"{role.GetChineseName()}！";
            AddLog(Color.Green, logMessage);
        }
        //切换游戏
        public void LogSwitchGame(GameType game) {
            string logMessage = $"切换至{game.GetChineseName()}游戏";
            AddLog(Color.Gray, logMessage);
        }
        //切换算法
        public void LogSwitchAlgorithm(AIAlgorithmType algorithmType) {
            string logMessage = $"切换至{algorithmType.GetChineseName()} AI";
            AddLog(Color.Gray, logMessage);
        }


    }
}
