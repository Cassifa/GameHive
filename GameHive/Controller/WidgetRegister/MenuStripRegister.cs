using GameHive.Constants.GameTypeEnum;
using GameHive.MainForm;

namespace GameHive.Controller {
    //用于注册顶部导航栏目和点击事件
    internal partial class Controller {
        private void RegisterMenuStrip(Form1 mainForm) {
            MenuStrip menuStrip = mainForm.Controls.OfType<MenuStrip>().FirstOrDefault();
            if (menuStrip == null) {
                throw new InvalidOperationException("主窗体未包含 MenuStrip 控件");
            }
            // 遍历 GameType 枚举并添加对应的菜单项
            foreach (GameType gameType in Enum.GetValues(typeof(GameType))) {
                string chineseName = gameType.GetChineseName(); // 获取枚举的中文名
                ToolStripMenuItem menuItem = new ToolStripMenuItem(chineseName) {
                    Tag = gameType // 将枚举值存储到 Tag 属性
                };
                // 为菜单项添加点击事件
                menuItem.Click += (sender, e) => GameMenuItem_Click(sender, e, gameType);

                menuStrip.Items.Add(menuItem);
            }

        }
        // 菜单项点击事件的具体实现
        private void GameMenuItem_Click(object sender, EventArgs e, GameType gameType) {
            //发送切换游戏的信号
            switchGameType(gameType);
        }
    }
}
