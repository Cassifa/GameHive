﻿/*************************************************************************************
 * 文 件 名:   EventRegister.cs
 * 描    述: 注册组件所有事件，延迟注册AI算法条目绑定模块
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 21:55
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.GameModeEnum;
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
        //登录状态委托
        public EventHandler LoginStatusRegister;
        // 保存难度选择器的事件处理器引用
        private EventHandler currentDifficultySelectorHandler;
        // 保存游戏模式选择器的事件处理器引用
        private EventHandler currentGameModeSelectorHandler;

        //注册除选择AI、难度选择外事件
        public void RegisterEvent() {
            //注册配置无关组件
            RegisterLoginStatus(mainForm);
            RegisterZJUTLogo();
            RegisterFirstTurn();
            RegisterSecondTurn();
            RegisterLogList();
            RegisterStatusSwitch();
            RegisterBoardPanel();
            RegisterGameModeSelector(); // 注册游戏模式选择器
            //注册导航并读取配置加入导航栏目
            RegisterMenuStrip(mainForm);
            //绑定到 mainForm.Controls的对应组件上
            BindEventHandlersToControls();
        }
        //绑定事件到组件
        private void BindEventHandlersToControls() {
            // 为登录状态标签绑定事件
            mainForm.loginStatus.Click += LoginStatusRegister;
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
            mainForm.AIType.DropDownStyle = ComboBoxStyle.DropDownList;
            mainForm.AIType.DrawItem += (sender, e) => {
                if (e.Index < 0)
                    return;
                e.DrawBackground();
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && 
                    (e.State & DrawItemState.Focus) == DrawItemState.Focus) {
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
                }
                if (sender is ComboBox combo) {
                    string text = combo.Items[e.Index].ToString();
                    var textSize = e.Graphics.MeasureString(text, e.Font);
                    var brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected && 
                               (e.State & DrawItemState.Focus) == DrawItemState.Focus) ?
                        new SolidBrush(SystemColors.HighlightText) : new SolidBrush(combo.ForeColor);
                    e.Graphics.DrawString(text, e.Font, brush,
                        e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2,
                        e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2);
                }
                e.DrawFocusRectangle();
            };

            //移除旧的事件处理器
            if (currentAITypeSelectorHandler != null) {
                mainForm.AIType.SelectedIndexChanged -= currentAITypeSelectorHandler;
            }

            //清空原有选择
            mainForm.AIType.Items.Clear();
            //加入所有可用AI
            foreach (var aiType in info.AllAIType) {
                mainForm.AIType.Items.Add(aiType.GetChineseName());
            }

            //创建新的事件处理器并保存引用
            currentAITypeSelectorHandler = (sender, e) => {
                //获取当前选中项的中文名称
                var selectedName = mainForm.AIType.SelectedItem.ToString();
                //根据中文名称查找对应的枚举值
                var selectedAI = info.AllAIType.FirstOrDefault(aiType => aiType.GetChineseName() == selectedName);
                //切换到选中的 AI 类型
                ConcreteProductInfo productInfo = ModelMessageSwitchAI(selectedAI);
                RegisterDifficultySelector(productInfo);
                // 让下拉框失去焦点
                mainForm.AIType.Parent.Focus();
            };

            //绑定新的事件处理器
            mainForm.AIType.SelectedIndexChanged += currentAITypeSelectorHandler;

            //设置默认选择第一项
            if (mainForm.AIType.Items.Count > 0) {
                mainForm.AIType.SelectedIndex = 0;
            }
        }

        //注册难度等级
        private void RegisterDifficultySelector(ConcreteProductInfo info) {
            //设置居中绘制模式
            mainForm.DifficultySelector.DrawMode = DrawMode.OwnerDrawFixed;
            mainForm.DifficultySelector.DropDownStyle = ComboBoxStyle.DropDownList;
            mainForm.DifficultySelector.DrawItem += (sender, e) => {
                if (e.Index < 0)
                    return;
                e.DrawBackground();
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && 
                    (e.State & DrawItemState.Focus) == DrawItemState.Focus) {
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
                }
                if (sender is ComboBox combo) {
                    string text = combo.Items[e.Index].ToString();
                    var textSize = e.Graphics.MeasureString(text, e.Font);
                    var brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected && 
                               (e.State & DrawItemState.Focus) == DrawItemState.Focus) ?
                        new SolidBrush(SystemColors.HighlightText) : new SolidBrush(combo.ForeColor);
                    e.Graphics.DrawString(text, e.Font, brush,
                        e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2,
                        e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2);
                }
                e.DrawFocusRectangle();
            };

            //移除旧的事件处理器
            if (currentDifficultySelectorHandler != null) {
                mainForm.DifficultySelector.SelectedIndexChanged -= currentDifficultySelectorHandler;
            }

            //清空原有选择
            mainForm.DifficultySelector.Items.Clear();
            //加入所有难度等级
            foreach (DifficultyLevel level in info.DifficultyLevels) {
                mainForm.DifficultySelector.Items.Add(level.GetChineseName());
            }

            //创建新的事件处理器并保存引用
            currentDifficultySelectorHandler = (sender, e) => {
                if (mainForm.DifficultySelector.SelectedIndex >= 0 &&
                    mainForm.DifficultySelector.SelectedIndex < info.DifficultyLevels.Count) {
                    var selectedLevel = info.DifficultyLevels[mainForm.DifficultySelector.SelectedIndex];
                    ModelMessageSwitchDifficulty(selectedLevel);
                }
                // 让下拉框失去焦点
                mainForm.DifficultySelector.Parent.Focus();
            };

            // 绑定新的事件处理器
            mainForm.DifficultySelector.SelectedIndexChanged += currentDifficultySelectorHandler;

            //设置默认选择第一项
            if (mainForm.DifficultySelector.Items.Count > 0) {
                mainForm.DifficultySelector.SelectedIndex = 0;
            }
        }

        // 注册游戏模式选择器事件
        private void RegisterGameModeSelector() {
            //居中
            mainForm.GameModeSelector.DrawMode = DrawMode.OwnerDrawFixed;
            mainForm.GameModeSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            mainForm.GameModeSelector.DrawItem += (sender, e) => {
                if (e.Index < 0)
                    return;
                e.DrawBackground();
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && 
                    (e.State & DrawItemState.Focus) == DrawItemState.Focus) {
                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
                }
                if (sender is ComboBox combo) {
                    string text = combo.Items[e.Index].ToString();
                    var textSize = e.Graphics.MeasureString(text, e.Font);
                    var brush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected && 
                               (e.State & DrawItemState.Focus) == DrawItemState.Focus) ?
                        new SolidBrush(SystemColors.HighlightText) : new SolidBrush(combo.ForeColor);
                    e.Graphics.DrawString(text, e.Font, brush,
                        e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2,
                        e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2);
                }
                e.DrawFocusRectangle();
            };

            // 订阅登录状态改变事件
            GameHive.Net.UserInfo.Instance.LoginStatusChanged += (sender, e) => {
                RefreshGameModeSelector();
            };

            RefreshGameModeSelector();
        }

        // 刷新游戏模式选择器
        private void RefreshGameModeSelector() {
            //移除旧的事件处理器
            if (currentGameModeSelectorHandler != null) {
                mainForm.GameModeSelector.SelectedIndexChanged -= currentGameModeSelectorHandler;
            }

            //清空原有选择
            mainForm.GameModeSelector.Items.Clear();
            //加入所有游戏模式
            foreach (GameMode mode in Enum.GetValues(typeof(GameMode))) {
                // 只有在登录状态下才添加AI和联机对战选项
                if (mode == GameMode.LocalGame || GameHive.Net.UserInfo.Instance.IsLoggedIn) {
                    mainForm.GameModeSelector.Items.Add(mode.GetChineseName());
                }
            }

            //创建新的事件处理器并保存引用
            currentGameModeSelectorHandler = (sender, e) => {
                if (mainForm.GameModeSelector.SelectedIndex >= 0) {
                    var selectedMode = (GameMode)mainForm.GameModeSelector.SelectedIndex;
                    CurrentGameMode = selectedMode;

                    //禁用或启用相关组件
                    bool isLocalGame = selectedMode == GameMode.LocalGame;
                    mainForm.firstTurn.Enabled = isLocalGame;
                    mainForm.secondTurn.Enabled = isLocalGame;
                    mainForm.AIType.Enabled = isLocalGame;
                    mainForm.DifficultySelector.Enabled = isLocalGame;

                    // 设置文字颜色
                    mainForm.AIType.ForeColor = isLocalGame ? SystemColors.WindowText : SystemColors.GrayText;
                    mainForm.DifficultySelector.ForeColor = isLocalGame ? SystemColors.WindowText : SystemColors.GrayText;

                    // 更新状态切换按钮文字
                    UpdateStatusSwitchText();
                }
                // 让下拉框失去焦点
                mainForm.GameModeSelector.Parent.Focus();
            };

            //绑定新的事件处理器
            mainForm.GameModeSelector.SelectedIndexChanged += currentGameModeSelectorHandler;

            //设置默认选择第一项
            if (mainForm.GameModeSelector.Items.Count > 0) {
                mainForm.GameModeSelector.SelectedIndex = 0;
            }
        }
    }
}
