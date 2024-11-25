using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Controller {
    //用于注册切换到人类先手
    internal partial class Controller {
        private void RegisterFirstTurn() {
            firstTurnRegister += FirstTurnCheckedChanged;
        }
        private void FirstTurnCheckedChanged(object sender, EventArgs e) {
            //TODO: 通知棋盘管理类切换先后手
        }
    }
}
