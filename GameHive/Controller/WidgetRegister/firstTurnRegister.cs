using GameHive.Constants.RoleTypeEnum;
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
            //如果已经开始游戏则不可设置
            if (boardManager.gameRunning) return;
            //若不是由点击触发的补齐选中事件
            if(!mainForm.firstTurn.Checked) mainForm.firstTurn.Checked = true;
            //TODO: 通知棋盘管理类切换先后手
            ModelMessageSetPlayerTurnOrder(Role.Player);
            //通知显示层先后手变化
            ViewMessageSetFirst(Role.Player);
        }
    }
}
