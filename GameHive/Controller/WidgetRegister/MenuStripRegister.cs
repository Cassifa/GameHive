using GameHive.Constants.GameTypeEnum;
using GameHive.MainForm;

namespace GameHive.Controller {
    //用于注册顶部导航栏目和点击事件
    internal partial class Controller {
        // 保存AI类型选择器的事件处理器引用
        private EventHandler currentAITypeSelectorHandler;

        private void RegisterMenuStrip(DoubleBufferedForm mainForm) {
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
                menuItem.Click += (sender, e) => GameMenuItem_Click(gameType);
                //讲已经绑定过事件的条目加入一级菜单
                menuStrip.Items.Add(menuItem);
            }
        }

        // 菜单项点击事件的具体实现
        private void GameMenuItem_Click(GameType gameType) {
            //如果已经开始游戏则不可切换
            if (boardManager.gameRunning) return;

            //移除旧的AI类型选择器事件处理器
            if (currentAITypeSelectorHandler != null) {
                mainForm.AIType.SelectedIndexChanged -= currentAITypeSelectorHandler;
                currentAITypeSelectorHandler = null;
            }

            //执行切换游戏行为
            SwitchGame(gameType);
        }
    }
}
