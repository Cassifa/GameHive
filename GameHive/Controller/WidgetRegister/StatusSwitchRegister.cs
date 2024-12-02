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
            //运行中-终止 非运行-开始/清空
            if (boardManager.gameRunning) {
                //终止游戏
                ModelMessageStartGame();
                ViewMessageStartGame();
                //处理组件显示
                mainForm.statusSwitch.Text = "开始游戏";
                mainForm.statusSwitch.BackColor = Color.Green;
                mainForm.firstTurn.Enabled = true;
                mainForm.secondTurn.Enabled = true;
                mainForm.AIType.Enabled = true;
            } else {
                //开始游戏
                ModelMessageEndGame();
                ViewMessageEndGame();
                //处理组件显示
                mainForm.statusSwitch.Text = "终止游戏";
                mainForm.statusSwitch.BackColor = Color.Red;
                mainForm.firstTurn.Enabled = false;
                mainForm.secondTurn.Enabled = false;
                mainForm.AIType.Enabled = false;
            }
        }
    }
}
