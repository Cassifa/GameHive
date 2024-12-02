/*************************************************************************************
 * 文 件 名:   ViewUtils.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/2 19:01
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.MainForm;

namespace GameHive.View {
    internal partial class View {
        private Controller.Controller controller;
        private Form1 mainForm;

        //游戏结束
        public void GameOver(Role role) { }
        public void SetFirst(Role role) {
            first = role;
        }

        //单例模式
#pragma warning disable CS8618
        private static View _instance;
#pragma warning restore CS8618 
        private View(Controller.Controller controller, Form1 form) {
            this.controller = controller;
            this.mainForm = form;
        }
        private static readonly object _lock = new object();
        public static View Instance(Controller.Controller controller, Form1 form) {
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
