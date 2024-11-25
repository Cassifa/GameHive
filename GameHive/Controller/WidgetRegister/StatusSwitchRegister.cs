using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Controller {
    //用于注册游戏状态切换
    internal partial class Controller {
        private void RegisterStatusSwitch() {
            statusSwitchRegister += StatusSwitchClick;
        }
        private void StatusSwitchClick(object sender, EventArgs e) {
            //TODO: 判断当前游戏状态
            //运行中-终止 非运行-开始/清空
        }
    }
}
