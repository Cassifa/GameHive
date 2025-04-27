/*************************************************************************************
 * 文 件 名:   EventRegister.cs
 * 描    述: 注册组件所有事件，延迟注册AI算法条目绑定模块
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 21:55
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model;

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
        //注册除选择AI外事件
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
                ModelMessageSwitchAI(selectedAI);
            };

        }
    }
}
