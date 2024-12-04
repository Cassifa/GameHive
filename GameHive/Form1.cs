/*************************************************************************************
 * 文 件 名:   Form1.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/24 22:47
*************************************************************************************/
namespace GameHive.MainForm {
    public partial class Form1 : Form {
        private Controller.Controller controller;
        public Form1() {
            //创建组件并初始化外观
            InitializeComponent();
            //注册控制器
            controller = new Controller.Controller(this);
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}


