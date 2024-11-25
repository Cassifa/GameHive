using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Controller {
    //用于注册棋盘点击事件事件
    internal partial class Controller {
        private void RegisterBoardPanel() {
            BoardPanelRegister += BoardPanelClick;
        }

        private void BoardPanelClick(object sender, EventArgs e) {
            //TODO: 根据BoardInfo先判断是否合法，合法再交给棋盘管理判断合法
            //通知棋盘管理类下棋，通知展示层下棋，获取棋盘管理类下一步
        }
    }
}
