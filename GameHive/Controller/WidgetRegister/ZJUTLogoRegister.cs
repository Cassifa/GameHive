/*************************************************************************************
 * 文 件 名:   ZJUTLogoRegister.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:31
*************************************************************************************/
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
