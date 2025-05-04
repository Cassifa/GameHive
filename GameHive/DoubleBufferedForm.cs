/*************************************************************************************
 * �� �� ��:   Form1.cs
 * ��    ��: 
 * ��    ����  V2.0 .NET�ͻ��˳���
 * �� �� �ߣ�  Cassifa
 * ����ʱ�䣺  2024/11/24 22:47
*************************************************************************************/
namespace GameHive.MainForm {
    public partial class DoubleBufferedForm : Form {
        private Controller.Controller controller;
        public DoubleBufferedForm() {
            //�����������ʼ�����
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedDialog;
            LogListBox.DrawMode = DrawMode.OwnerDrawFixed;
            LogListBox.DrawItem += (sender, e) => {
                if (e.Index < 0) return;
                var listBox = sender as ListBox;
                var item = listBox?.Items[e.Index] as ColoredListItem;
                if (item == null) return;
                // �����ı�
                using (Brush brush = new SolidBrush(item.TextColor)) {
                    e.Graphics.DrawString(item.Text, e.Font, brush, e.Bounds);
                }
                // ���ƽ����
                e.DrawFocusRectangle();
            };

            //ע�������
            controller = new Controller.Controller(this);

        }

        private void Form1_Load(object sender, EventArgs e) {

        }

    }
}


