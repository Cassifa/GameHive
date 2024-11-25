using GameHive.Controller;
using GameHive.MainForm;
namespace GameHive.MainForm {
    public partial class Form1 : Form {
        private Controller.Controller controller;
        public Form1() {
            InitializeComponent();
            controller = new Controller.Controller(this);
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
    }
}


