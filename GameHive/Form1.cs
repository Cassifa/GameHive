/*************************************************************************************
 * �� �� ��:   Form1.cs
 * ��    ��: 
 * ��    ����  V1.0
 * �� �� �ߣ�  Cassifa
 * ����ʱ�䣺  2024/11/24 22:47
*************************************************************************************/
namespace GameHive.MainForm {
    public partial class Form1 : Form {
        private Controller.Controller controller;
        public Form1() {
            //�����������ʼ�����
            InitializeComponent();
            //ע�������
            controller = new Controller.Controller(this);
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}


