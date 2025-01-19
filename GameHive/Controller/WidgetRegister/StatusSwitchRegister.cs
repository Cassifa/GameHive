/*************************************************************************************
 * 文 件 名:   StatusSwitchRegister.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:32
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Controller {
    //用于注册游戏状态切换
    internal partial class Controller {
        private void RegisterStatusSwitch() {
            statusSwitchRegister += StatusSwitchClick;
        }
        private void StatusSwitchClick(object sender, EventArgs e) {
            //运行中-终止 非运行-开始/清空
            if (boardManager.gameRunning) {
                EndGame(Role.Empty);
            } else {
                StartGame();
            }
        }
    }
}
