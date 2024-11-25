using GameHive.MainForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
