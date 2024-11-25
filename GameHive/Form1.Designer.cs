namespace GameHive.MainForm {
    partial class Form1 {
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
            secondTurn = new RadioButton();
            firstTurn = new RadioButton();
            statusSwitch = new Button();
            AIType = new ComboBox();
            LogPanel = new Panel();
            LogListBox = new ListBox();
            ZJUTLogo = new PictureBox();
            LeftPanel = new Panel();
            BoardPanel = new Panel();
            rightPanel.SuspendLayout();
            LogPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ZJUTLogo).BeginInit();
            LeftPanel.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip 条目可被点击，点击后切换到对应游戏
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
            // secondTurn 选择后切换到后手
            // 
            secondTurn.AutoSize = true;
            secondTurn.Location = new Point(180, 360);
            secondTurn.Name = "secondTurn";
            secondTurn.Size = new Size(71, 28);
            secondTurn.TabIndex = 6;
            secondTurn.TabStop = true;
            secondTurn.Text = "后手";
            secondTurn.UseVisualStyleBackColor = true;
            // 
            // firstTurn 选择后切换到先手
            // 
            firstTurn.AutoSize = true;
            firstTurn.Location = new Point(69, 360);
            firstTurn.Name = "firstTurn";
            firstTurn.Size = new Size(71, 28);
            firstTurn.TabIndex = 5;
            firstTurn.TabStop = true;
            firstTurn.Text = "先手";
            firstTurn.UseVisualStyleBackColor = true;
            // 
            // statusSwitch 点击触发开始游戏/终止游戏/清空游戏状态
            // 
            statusSwitch.Location = new Point(69, 495);
            statusSwitch.Name = "statusSwitch";
            statusSwitch.Size = new Size(182, 46);
            statusSwitch.TabIndex = 4;
            statusSwitch.Text = "开始";
            statusSwitch.UseVisualStyleBackColor = true;
            // 
            // AIType 选择以切换对战的AI
            // 
            AIType.DropDownStyle = ComboBoxStyle.DropDownList;
            AIType.FormattingEnabled = true;
            AIType.Location = new Point(69, 420);
            AIType.Name = "AIType";
            AIType.Size = new Size(182, 32);
            AIType.TabIndex = 3;
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
            LogListBox.Dock = DockStyle.Fill;
            LogListBox.FormattingEnabled = true;
            LogListBox.ItemHeight = 24;
            LogListBox.Location = new Point(0, 0);
            LogListBox.Name = "LogListBox";
            LogListBox.Size = new Size(300, 325);
            LogListBox.TabIndex = 0;
            // 
            // ZJUTLogo 双击弹窗
            // 
            ZJUTLogo.BackgroundImage = Properties.Resources.logo;
            ZJUTLogo.BackgroundImageLayout = ImageLayout.Stretch;
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1198, 924);
            Controls.Add(LeftPanel);
            Controls.Add(rightPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "Form1";
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
        private Panel LeftPanel;
        //可交互组件:
        public MenuStrip menuStrip;
        public Panel BoardPanel;
        public PictureBox ZJUTLogo;
        public RadioButton secondTurn;
        public RadioButton firstTurn;
        public ComboBox AIType;
        public Button statusSwitch;
        public ListBox LogListBox;
    }
}
