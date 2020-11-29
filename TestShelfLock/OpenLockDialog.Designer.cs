
namespace TestShelfLock
{
    partial class OpenLockDialog
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
            this.comboBox_cardNo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_lockNo = new System.Windows.Forms.ComboBox();
            this.button_openLock = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_result = new System.Windows.Forms.TextBox();
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
            // comboBox_cardNo
            // 
            this.comboBox_cardNo.FormattingEnabled = true;
            this.comboBox_cardNo.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.comboBox_cardNo.Location = new System.Drawing.Point(202, 48);
            this.comboBox_cardNo.Name = "comboBox_cardNo";
            this.comboBox_cardNo.Size = new System.Drawing.Size(121, 29);
            this.comboBox_cardNo.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "锁控板编号(&B):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 21);
            this.label3.TabIndex = 5;
            this.label3.Text = "锁编号(&L):";
            // 
            // comboBox_lockNo
            // 
            this.comboBox_lockNo.FormattingEnabled = true;
            this.comboBox_lockNo.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
            this.comboBox_lockNo.Location = new System.Drawing.Point(202, 83);
            this.comboBox_lockNo.Name = "comboBox_lockNo";
            this.comboBox_lockNo.Size = new System.Drawing.Size(121, 29);
            this.comboBox_lockNo.TabIndex = 4;
            // 
            // button_openLock
            // 
            this.button_openLock.Location = new System.Drawing.Point(202, 119);
            this.button_openLock.Name = "button_openLock";
            this.button_openLock.Size = new System.Drawing.Size(200, 51);
            this.button_openLock.TabIndex = 6;
            this.button_openLock.Text = "开锁(&O)";
            this.button_openLock.UseVisualStyleBackColor = true;
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
            // OpenLockDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox_result);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_openLock);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_lockNo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_cardNo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox_comPort);
            this.Name = "OpenLockDialog";
            this.Text = "OpenLockDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_comPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_cardNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_lockNo;
        private System.Windows.Forms.Button button_openLock;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_result;
    }
}