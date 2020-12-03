
namespace RfidTool
{
    partial class SettingDialog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_rfid = new System.Windows.Forms.TabPage();
            this.textBox_rfid_aoi = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_rfid_oi = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_rfid.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_rfid);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(802, 485);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_rfid
            // 
            this.tabPage_rfid.Controls.Add(this.textBox_rfid_aoi);
            this.tabPage_rfid.Controls.Add(this.label2);
            this.tabPage_rfid.Controls.Add(this.textBox_rfid_oi);
            this.tabPage_rfid.Controls.Add(this.label1);
            this.tabPage_rfid.Location = new System.Drawing.Point(4, 37);
            this.tabPage_rfid.Name = "tabPage_rfid";
            this.tabPage_rfid.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_rfid.Size = new System.Drawing.Size(794, 444);
            this.tabPage_rfid.TabIndex = 0;
            this.tabPage_rfid.Text = "RFID";
            this.tabPage_rfid.UseVisualStyleBackColor = true;
            // 
            // textBox_rfid_aoi
            // 
            this.textBox_rfid_aoi.Location = new System.Drawing.Point(229, 58);
            this.textBox_rfid_aoi.Name = "textBox_rfid_aoi";
            this.textBox_rfid_aoi.Size = new System.Drawing.Size(347, 34);
            this.textBox_rfid_aoi.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(193, 28);
            this.label2.TabIndex = 2;
            this.label2.Text = "非标准机构代码(&A):";
            // 
            // textBox_rfid_oi
            // 
            this.textBox_rfid_oi.Location = new System.Drawing.Point(229, 18);
            this.textBox_rfid_oi.Name = "textBox_rfid_oi";
            this.textBox_rfid_oi.Size = new System.Drawing.Size(347, 34);
            this.textBox_rfid_oi.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "机构代码(&O):";
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 37);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(794, 444);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(543, 504);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(131, 40);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Location = new System.Drawing.Point(680, 504);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(131, 40);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // SettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(827, 556);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Name = "SettingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_rfid.ResumeLayout(false);
            this.tabPage_rfid.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_rfid;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_rfid_oi;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_rfid_aoi;
        private System.Windows.Forms.Label label2;
    }
}