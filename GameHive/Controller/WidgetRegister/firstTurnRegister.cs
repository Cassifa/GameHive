/*************************************************************************************
 * 文 件 名:   firstTurnRegister.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:31
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Controller {
    //用于注册切换到人类先手
    internal partial class Controller {
        private void RegisterFirstTurn() {
            firstTurnRegister += FirstTurnCheckedChanged;
        }
        private void FirstTurnCheckedChanged(object sender, EventArgs e) {
            if (!mainForm.firstTurn.Checked) return;
            //如果已经开始游戏则不可设置
            if (boardManager.gameRunning) return;
            //通知棋盘管理类切换先后手
            ModelMessageSetPlayerTurnOrder(Role.Player);
            //通知显示层先后手变化
            ViewMessageSetFirst(Role.Player);
        }
    }
}
