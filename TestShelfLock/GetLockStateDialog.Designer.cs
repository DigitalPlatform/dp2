
namespace TestShelfLock
{
    partial class GetLockStateDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox_comPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_getLockState = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_result = new System.Windows.Forms.TextBox();
            this.textBox_lockPath = new System.Windows.Forms.TextBox();
            this.button_loopQuery = new System.Windows.Forms.Button();
            this.button_stopLoop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox_comPort
            // 
            this.comboBox_comPort.FormattingEnabled = true;
            this.comboBox_comPort.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8"});
            this.comboBox_comPort.Location = new System.Drawing.Point(202, 12);
            this.comboBox_comPort.Name = "comboBox_comPort";
            this.comboBox_comPort.Size = new System.Drawing.Size(284, 29);
            this.comboBox_comPort.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "COM 端口(&C):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "锁路径(&P):";
            // 
            // button_getLockState
            // 
            this.button_getLockState.Location = new System.Drawing.Point(202, 119);
            this.button_getLockState.Name = "button_getLockState";
            this.button_getLockState.Size = new System.Drawing.Size(200, 51);
            this.button_getLockState.TabIndex = 6;
            this.button_getLockState.Text = "获得锁状态(&G)";
            this.button_getLockState.UseVisualStyleBackColor = true;
            this.button_getLockState.Click += new System.EventHandler(this.button_getLockState_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 196);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "返回结果(&R):";
            // 
            // textBox_result
            // 
            this.textBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_result.Location = new System.Drawing.Point(16, 221);
            this.textBox_result.Multiline = true;
            this.textBox_result.Name = "textBox_result";
            this.textBox_result.ReadOnly = true;
            this.textBox_result.Size = new System.Drawing.Size(772, 217);
            this.textBox_result.TabIndex = 8;
            // 
            // textBox_lockPath
            // 
            this.textBox_lockPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_lockPath.Location = new System.Drawing.Point(202, 48);
            this.textBox_lockPath.Name = "textBox_lockPath";
            this.textBox_lockPath.Size = new System.Drawing.Size(586, 31);
            this.textBox_lockPath.TabIndex = 9;
            // 
            // button_loopQuery
            // 
            this.button_loopQuery.Location = new System.Drawing.Point(434, 119);
            this.button_loopQuery.Name = "button_loopQuery";
            this.button_loopQuery.Size = new System.Drawing.Size(227, 51);
            this.button_loopQuery.TabIndex = 10;
            this.button_loopQuery.Text = "持续获得锁状态(&L)";
            this.button_loopQuery.UseVisualStyleBackColor = true;
            this.button_loopQuery.Click += new System.EventHandler(this.button_loopQuery_Click);
            // 
            // button_stopLoop
            // 
            this.button_stopLoop.Enabled = false;
            this.button_stopLoop.Location = new System.Drawing.Point(667, 119);
            this.button_stopLoop.Name = "button_stopLoop";
            this.button_stopLoop.Size = new System.Drawing.Size(103, 51);
            this.button_stopLoop.TabIndex = 11;
            this.button_stopLoop.Text = "停止";
            this.button_stopLoop.UseVisualStyleBackColor = true;
            this.button_stopLoop.Click += new System.EventHandler(this.button_stopLoop_Click);
            // 
            // GetLockStateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_stopLoop);
            this.Controls.Add(this.button_loopQuery);
            this.Controls.Add(this.textBox_lockPath);
            this.Controls.Add(this.textBox_result);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_getLockState);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_comPort);
            this.Name = "GetLockStateDialog";
            this.Text = "GetLockStateDialog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetLockStateDialog_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_comPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getLockState;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_result;
        private System.Windows.Forms.TextBox textBox_lockPath;
        private System.Windows.Forms.Button button_loopQuery;
        private System.Windows.Forms.Button button_stopLoop;
    }
}