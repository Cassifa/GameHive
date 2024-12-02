using GameHive.Constants.RoleTypeEnum;
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
            //如果已经开始游戏则不可设置
            if (boardManager.gameRunning) return;
            //通知棋盘管理类切换先后手
            ModelMessageSetPlayerTurnOrder(Role.AI);
            //通知显示层先后手变化
            ViewMessageSetFirst(Role.AI);
        }
    }
}
