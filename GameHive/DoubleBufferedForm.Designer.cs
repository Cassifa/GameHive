namespace GameHive.MainForm {
    partial class DoubleBufferedForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            menuStrip = new MenuStrip();
            rightPanel = new Panel();
            GameModeSelector = new ComboBox();
            loginStatus = new Label();
            DifficultySelector = new ComboBox();
            secondTurn = new RadioButton();
            firstTurn = new RadioButton();
            statusSwitch = new Button();
            AIType = new ComboBox();
            LogPanel = new Panel();
            LogListBox = new ListBox();
            ZJUTLogo = new PictureBox();
            LeftPanel = new MyPanel();
            BoardPanel = new MyPanel();
            rightPanel.SuspendLayout();
            LogPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ZJUTLogo).BeginInit();
            LeftPanel.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(24, 24);
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1198, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip1";
            // 
            // rightPanel
            // 
            rightPanel.AccessibleRole = AccessibleRole.PageTabList;
            rightPanel.Controls.Add(GameModeSelector);
            rightPanel.Controls.Add(loginStatus);
            rightPanel.Controls.Add(DifficultySelector);
            rightPanel.Controls.Add(secondTurn);
            rightPanel.Controls.Add(firstTurn);
            rightPanel.Controls.Add(statusSwitch);
            rightPanel.Controls.Add(AIType);
            rightPanel.Controls.Add(LogPanel);
            rightPanel.Controls.Add(ZJUTLogo);
            rightPanel.Location = new Point(898, 24);
            rightPanel.Name = "rightPanel";
            rightPanel.Size = new Size(300, 900);
            rightPanel.TabIndex = 1;
            // 
            // GameModeSelector
            // 
            GameModeSelector.Cursor = Cursors.Hand;
            GameModeSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            GameModeSelector.FormattingEnabled = true;
            GameModeSelector.Location = new Point(127, 325);
            GameModeSelector.Name = "GameModeSelector";
            GameModeSelector.Size = new Size(150, 32);
            GameModeSelector.TabIndex = 8;
            // 
            // loginStatus
            // 
            loginStatus.AutoSize = true;
            loginStatus.Location = new Point(28, 333);
            loginStatus.Name = "loginStatus";
            loginStatus.Size = new Size(82, 24);
            loginStatus.TabIndex = 0;
            loginStatus.Text = "点此登录";
            loginStatus.TextAlign = ContentAlignment.MiddleCenter;
            loginStatus.Click += loginStatus_Click;
            // 
            // DifficultySelector
            // 
            DifficultySelector.Cursor = Cursors.Hand;
            DifficultySelector.DropDownStyle = ComboBoxStyle.DropDownList;
            DifficultySelector.FormattingEnabled = true;
            DifficultySelector.Location = new Point(127, 436);
            DifficultySelector.Name = "DifficultySelector";
            DifficultySelector.Size = new Size(150, 32);
            DifficultySelector.TabIndex = 7;
            // 
            // secondTurn
            // 
            secondTurn.AutoSize = true;
            secondTurn.Cursor = Cursors.Hand;
            secondTurn.Location = new Point(39, 440);
            secondTurn.Name = "secondTurn";
            secondTurn.Size = new Size(71, 28);
            secondTurn.TabIndex = 6;
            secondTurn.TabStop = true;
            secondTurn.Text = "后手";
            secondTurn.UseVisualStyleBackColor = true;
            // 
            // firstTurn
            // 
            firstTurn.AutoSize = true;
            firstTurn.Cursor = Cursors.Hand;
            firstTurn.Location = new Point(39, 382);
            firstTurn.Name = "firstTurn";
            firstTurn.Size = new Size(71, 28);
            firstTurn.TabIndex = 5;
            firstTurn.TabStop = true;
            firstTurn.Text = "先手";
            firstTurn.UseVisualStyleBackColor = true;
            // 
            // statusSwitch
            // 
            statusSwitch.Cursor = Cursors.Hand;
            statusSwitch.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Italic, GraphicsUnit.Point, 134);
            statusSwitch.Location = new Point(39, 491);
            statusSwitch.Name = "statusSwitch";
            statusSwitch.Size = new Size(238, 61);
            statusSwitch.TabIndex = 4;
            statusSwitch.Text = "开始";
            statusSwitch.UseVisualStyleBackColor = true;
            // 
            // AIType
            // 
            AIType.Cursor = Cursors.Hand;
            AIType.DropDownStyle = ComboBoxStyle.DropDownList;
            AIType.FormattingEnabled = true;
            AIType.Location = new Point(127, 378);
            AIType.Name = "AIType";
            AIType.Size = new Size(150, 32);
            AIType.TabIndex = 3;
            AIType.SelectedIndexChanged += AIType_SelectedIndexChanged;
            // 
            // LogPanel
            // 
            LogPanel.Controls.Add(LogListBox);
            LogPanel.Dock = DockStyle.Bottom;
            LogPanel.Location = new Point(0, 575);
            LogPanel.Name = "LogPanel";
            LogPanel.Size = new Size(300, 325);
            LogPanel.TabIndex = 1;
            // 
            // LogListBox
            // 
            LogListBox.Cursor = Cursors.HSplit;
            LogListBox.Dock = DockStyle.Fill;
            LogListBox.FormattingEnabled = true;
            LogListBox.ItemHeight = 24;
            LogListBox.Location = new Point(0, 0);
            LogListBox.Name = "LogListBox";
            LogListBox.Size = new Size(300, 325);
            LogListBox.TabIndex = 0;
            // 
            // ZJUTLogo
            // 
            ZJUTLogo.BackgroundImage = Properties.Resources.logo;
            ZJUTLogo.BackgroundImageLayout = ImageLayout.Stretch;
            ZJUTLogo.Cursor = Cursors.Hand;
            ZJUTLogo.Dock = DockStyle.Top;
            ZJUTLogo.Location = new Point(0, 0);
            ZJUTLogo.Name = "ZJUTLogo";
            ZJUTLogo.Size = new Size(300, 300);
            ZJUTLogo.TabIndex = 0;
            ZJUTLogo.TabStop = false;
            // 
            // LeftPanel
            // 
            LeftPanel.Controls.Add(BoardPanel);
            LeftPanel.Dock = DockStyle.Left;
            LeftPanel.Location = new Point(0, 24);
            LeftPanel.Name = "LeftPanel";
            LeftPanel.Size = new Size(900, 900);
            LeftPanel.TabIndex = 2;
            // 
            // BoardPanel
            // 
            BoardPanel.Location = new Point(45, 45);
            BoardPanel.Name = "BoardPanel";
            BoardPanel.Size = new Size(810, 810);
            BoardPanel.TabIndex = 0;
            // 
            // DoubleBufferedForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1198, 924);
            Controls.Add(LeftPanel);
            Controls.Add(rightPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "DoubleBufferedForm";
            Text = "GameHive";
            Load += Form1_Load;
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            LogPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ZJUTLogo).EndInit();
            LeftPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel rightPanel;
        private Panel LogPanel;
        private MyPanel LeftPanel;
        //可交互组件:
        public MenuStrip menuStrip;
        public PictureBox ZJUTLogo;
        public RadioButton secondTurn;
        public RadioButton firstTurn;
        public ComboBox AIType;
        public Button statusSwitch;
        public ListBox LogListBox;
        public ComboBox DifficultySelector;
        public MyPanel BoardPanel;
        public Label loginStatus;
        public ComboBox GameModeSelector;
    }
}
