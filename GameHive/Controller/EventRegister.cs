/*************************************************************************************
 * 文 件 名:   EventRegister.cs
 * 描    述: 注册组件所有事件，延迟注册AI算法条目绑定模块
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 21:55
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Model.GameInfo;

namespace GameHive.Controller {
    internal partial class Controller {
        //导航委托
        public EventHandler menuStripHandler;
        //棋盘委托
        public EventHandler BoardPanelRegister;
        //右侧选项委托
        public EventHandler ZJUTLogoRegister;
        public EventHandler firstTurnRegister;
        public EventHandler secondTurnRegister;
        public EventHandler statusSwitchRegister;
        //历史记录
        public EventHandler LogListBoxRegister;
        //难度选择
        public EventHandler DifficultySelectorRegister;
        // 保存难度选择器的事件处理器引用
        private EventHandler currentDifficultySelectorHandler;
        //注册除选择AI、难度选择外事件
        public void RegisterEvent() {
            //注册配置无关组件
            RegisterZJUTLogo();
            RegisterFirstTurn();
            RegisterSecondTurn();
            RegisterLogList();
            RegisterStatusSwitch();
            RegisterBoardPanel();
            //注册导航并读取配置加入导航栏目
            RegisterMenuStrip(mainForm);
            //绑定到 mainForm.Controls的对应组件上
            BindEventHandlersToControls();
        }
        //绑定事件到组件
        private void BindEventHandlersToControls() {
            // 为棋盘相关控件绑定事件
            mainForm.BoardPanel.Click += BoardPanelRegister;
            // 为右侧选项相关控件绑定事件
            mainForm.ZJUTLogo.DoubleClick += ZJUTLogoRegister;
            mainForm.firstTurn.CheckedChanged += firstTurnRegister;
            mainForm.secondTurn.CheckedChanged += secondTurnRegister;
            mainForm.statusSwitch.Click += statusSwitchRegister;
            //mainForm.LogListBox.SelectedIndexChanged += LogListBoxRegister;
        }
        //注册并绑定切换AI事件，清除原先选项
        private void RegisterAIType(GameBoardInfo info) {
            //设置居中绘制模式
            mainForm.AIType.DrawMode = DrawMode.OwnerDrawFixed;
            mainForm.AIType.DrawItem += (sender, e) => {
                if (e.Index < 0) return;
                e.DrawBackground();
                if (e.State == DrawItemState.Selected) {
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
                }
                if (sender is ComboBox combo) {
                    string text = combo.Items[e.Index].ToString();
                    var textSize = e.Graphics.MeasureString(text, e.Font);
                    var brush = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? 
                        new SolidBrush(SystemColors.HighlightText) : new SolidBrush(combo.ForeColor);
                    e.Graphics.DrawString(text, e.Font, brush,
                        e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2,
                        e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2);
                }
                e.DrawFocusRectangle();
            };

            //清空原有选择
            mainForm.AIType.Items.Clear();
            //加入所有可用AI
            foreach (var aiType in info.AllAIType) {
                mainForm.AIType.Items.Add(aiType.GetChineseName());
            }
            //绑定点击事件
            mainForm.AIType.SelectedIndexChanged += (sender, e) => {
                // 获取当前选中项的中文名称
                var selectedName = mainForm.AIType.SelectedItem.ToString();
                // 根据中文名称查找对应的枚举值
                var selectedAI = info.AllAIType.FirstOrDefault(aiType => aiType.GetChineseName() == selectedName);
                // 切换到选中的 AI 类型
                ConcreteProductInfo productInfo = ModelMessageSwitchAI(selectedAI);
                RegisterDifficultySelector(productInfo);
            };
        }
        //注册难度等级
        private void RegisterDifficultySelector(ConcreteProductInfo info) {
            //设置居中绘制模式
            mainForm.DifficultySelector.DrawMode = DrawMode.OwnerDrawFixed;
            mainForm.DifficultySelector.DrawItem += (sender, e) => {
                if (e.Index < 0) return;
                e.DrawBackground();
                if (e.State == DrawItemState.Selected) {
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
                }
                if (sender is ComboBox combo) {
                    string text = combo.Items[e.Index].ToString();
                    var textSize = e.Graphics.MeasureString(text, e.Font);
                    var brush = (e.State & DrawItemState.Selected) == DrawItemState.Selected ? 
                        new SolidBrush(SystemColors.HighlightText) : new SolidBrush(combo.ForeColor);
                    e.Graphics.DrawString(text, e.Font, brush,
                        e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2,
                        e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2);
                }
                e.DrawFocusRectangle();
            };

            // 如果存在旧的事件处理器，先移除它
            if (currentDifficultySelectorHandler != null) {
                mainForm.DifficultySelector.SelectedIndexChanged -= currentDifficultySelectorHandler;
            }

            //清空原有选择
            mainForm.DifficultySelector.Items.Clear();
            //加入所有难度等级
            foreach (DifficultyLevel level in info.DifficultyLevels) {
                mainForm.DifficultySelector.Items.Add(level.GetChineseName());
            }

            // 创建新的事件处理器并保存引用
            currentDifficultySelectorHandler = (sender, e) => {
                if (mainForm.DifficultySelector.SelectedIndex >= 0 && 
                    mainForm.DifficultySelector.SelectedIndex < info.DifficultyLevels.Count) {
                    var selectedLevel = info.DifficultyLevels[mainForm.DifficultySelector.SelectedIndex];
                    ModelMessageSwitchDifficulty(selectedLevel);
                }
            };

            // 绑定新的事件处理器
            mainForm.DifficultySelector.SelectedIndexChanged += currentDifficultySelectorHandler;

            //设置默认选择第一项
            if (mainForm.DifficultySelector.Items.Count > 0) {
                mainForm.DifficultySelector.SelectedIndex = 0;
            }
        }
    }
}
