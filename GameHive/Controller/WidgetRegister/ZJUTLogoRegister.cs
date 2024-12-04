using GameHive.Constants.GameTypeEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Controller {
    //用于注册双击校徽
    internal partial class Controller {
        private void RegisterZJUTLogo() {
            ZJUTLogoRegister += ZJUTLogoDoubleClicked;
        }
        private void ZJUTLogoDoubleClicked(object sender, EventArgs e) {
            ViewMessageLogoShow();
        }
    }
}
