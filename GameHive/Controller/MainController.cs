using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameHive.MainForm;
namespace GameHive.Controller {
    //这个函数用于执行控制器生命周期主流程
    internal partial class Controller {
        private Form1 mainForm;
        public Controller(Form1 mainForm) {
            this.mainForm = mainForm;
            Init();
        }
        private void Init( ) {
            //注册除运行算法外所有用户点击事件
            RegisterEvent();
            //初始化界面状态
            SetupInitialState();
        }
        private void SetupInitialState() { 
            //初始化选择的棋盘
            //初始化选择的算法
            //初始化先后手
        }
    }
}
