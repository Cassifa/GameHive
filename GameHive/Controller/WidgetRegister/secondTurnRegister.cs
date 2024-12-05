/*************************************************************************************
 * 文 件 名:   secondTurnRegister.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:31
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Controller {
    //用于注册切换到人类后手
    internal partial class Controller {
        private void RegisterSecondTurn() {
            secondTurnRegister += SecondTurnCheckedChanged;
        }

        private void SecondTurnCheckedChanged(object sender, EventArgs e) {
            if (mainForm.secondTurn.Checked) return;
            //如果已经开始游戏则不可设置
            if (boardManager.gameRunning) return;
            //通知棋盘管理类切换先后手
            ModelMessageSetPlayerTurnOrder(Role.AI);
            //通知显示层先后手变化
            ViewMessageSetFirst(Role.AI);
        }
    }
}
