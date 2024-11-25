using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Controller {
    //用于注册切换到人类后手
    internal partial class Controller {
        private void RegisterSecondTurn() {
            secondTurnRegister += SecondTurnCheckedChanged;
        }

        private void SecondTurnCheckedChanged(object sender, EventArgs e) {
            //TODO: 通知棋盘管理类切换先后手
        }
    }
}
