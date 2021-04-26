
namespace dp2Inventory
{
    partial class BeginInventoryDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_action = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_action_slowMode = new System.Windows.Forms.CheckBox();
            this.textBox_action_batchNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_action_location = new System.Windows.Forms.ComboBox();
            this.checkBox_action_verifyEas = new System.Windows.Forms.CheckBox();
            this.checkBox_action_setLocation = new System.Windows.Forms.CheckBox();
            this.checkBox_action_setCurrentLocation = new System.Windows.Forms.CheckBox();
            this.checkBox_action_setUID = new System.Windows.Forms.CheckBox();
            this.tabPage_other = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage_action.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(800, 697);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(131, 53);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(663, 697);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(131, 53);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_action);
            this.tabControl1.Controls.Add(this.tabPage_other);
            this.tabControl1.Location = new System.Drawing.Point(14, 15);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(917, 677);
            this.tabControl1.TabIndex = 5;
            // 
            // tabPage_action
            // 
            this.tabPage_action.AutoScroll = true;
            this.tabPage_action.Controls.Add(this.groupBox1);
            this.tabPage_action.Controls.Add(this.checkBox_action_slowMode);
            this.tabPage_action.Controls.Add(this.textBox_action_batchNo);
            this.tabPage_action.Controls.Add(this.label2);
            this.tabPage_action.Controls.Add(this.label1);
            this.tabPage_action.Controls.Add(this.comboBox_action_location);
            this.tabPage_action.Controls.Add(this.checkBox_action_verifyEas);
            this.tabPage_action.Controls.Add(this.checkBox_action_setLocation);
            this.tabPage_action.Controls.Add(this.checkBox_action_setCurrentLocation);
            this.tabPage_action.Controls.Add(this.checkBox_action_setUID);
            this.tabPage_action.Location = new System.Drawing.Point(4, 37);
            this.tabPage_action.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_action.Name = "tabPage_action";
            this.tabPage_action.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage_action.Size = new System.Drawing.Size(909, 636);
            this.tabPage_action.TabIndex = 0;
            this.tabPage_action.Text = "动作参数";
            this.tabPage_action.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(409, 52);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(10, 153);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // checkBox_action_slowMode
            // 
            this.checkBox_action_slowMode.AutoSize = true;
            this.checkBox_action_slowMode.Location = new System.Drawing.Point(20, 396);
            this.checkBox_action_slowMode.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_action_slowMode.Name = "checkBox_action_slowMode";
            this.checkBox_action_slowMode.Size = new System.Drawing.Size(148, 32);
            this.checkBox_action_slowMode.TabIndex = 8;
            this.checkBox_action_slowMode.Text = "慢速模式(&S)";
            this.checkBox_action_slowMode.UseVisualStyleBackColor = true;
            this.checkBox_action_slowMode.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // textBox_action_batchNo
            // 
            this.textBox_action_batchNo.Location = new System.Drawing.Point(192, 302);
            this.textBox_action_batchNo.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_action_batchNo.Name = "textBox_action_batchNo";
            this.textBox_action_batchNo.Size = new System.Drawing.Size(402, 35);
            this.textBox_action_batchNo.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 305);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 28);
            this.label2.TabIndex = 6;
            this.label2.Text = "批次号(&B):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 257);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 28);
            this.label1.TabIndex = 5;
            this.label1.Text = "馆藏地(&O):";
            // 
            // comboBox_action_location
            // 
            this.comboBox_action_location.FormattingEnabled = true;
            this.comboBox_action_location.Location = new System.Drawing.Point(192, 254);
            this.comboBox_action_location.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_action_location.Name = "comboBox_action_location";
            this.comboBox_action_location.Size = new System.Drawing.Size(402, 36);
            this.comboBox_action_location.TabIndex = 4;
            // 
            // checkBox_action_verifyEas
            // 
            this.checkBox_action_verifyEas.AutoSize = true;
            this.checkBox_action_verifyEas.Location = new System.Drawing.Point(20, 147);
            this.checkBox_action_verifyEas.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_action_verifyEas.Name = "checkBox_action_verifyEas";
            this.checkBox_action_verifyEas.Size = new System.Drawing.Size(151, 32);
            this.checkBox_action_verifyEas.TabIndex = 3;
            this.checkBox_action_verifyEas.Text = "校验 EAS(&E)";
            this.checkBox_action_verifyEas.UseVisualStyleBackColor = true;
            this.checkBox_action_verifyEas.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBox_action_setLocation
            // 
            this.checkBox_action_setLocation.AutoSize = true;
            this.checkBox_action_setLocation.Location = new System.Drawing.Point(480, 67);
            this.checkBox_action_setLocation.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_action_setLocation.Name = "checkBox_action_setLocation";
            this.checkBox_action_setLocation.Size = new System.Drawing.Size(251, 32);
            this.checkBox_action_setLocation.TabIndex = 2;
            this.checkBox_action_setLocation.Text = "更新永久位置(&L) [慎选]";
            this.checkBox_action_setLocation.UseVisualStyleBackColor = true;
            this.checkBox_action_setLocation.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBox_action_setCurrentLocation
            // 
            this.checkBox_action_setCurrentLocation.AutoSize = true;
            this.checkBox_action_setCurrentLocation.Location = new System.Drawing.Point(20, 107);
            this.checkBox_action_setCurrentLocation.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_action_setCurrentLocation.Name = "checkBox_action_setCurrentLocation";
            this.checkBox_action_setCurrentLocation.Size = new System.Drawing.Size(192, 32);
            this.checkBox_action_setCurrentLocation.TabIndex = 1;
            this.checkBox_action_setCurrentLocation.Text = "更新当前位置(&C)";
            this.checkBox_action_setCurrentLocation.UseVisualStyleBackColor = true;
            this.checkBox_action_setCurrentLocation.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // checkBox_action_setUID
            // 
            this.checkBox_action_setUID.AutoSize = true;
            this.checkBox_action_setUID.Location = new System.Drawing.Point(20, 67);
            this.checkBox_action_setUID.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_action_setUID.Name = "checkBox_action_setUID";
            this.checkBox_action_setUID.Size = new System.Drawing.Size(154, 32);
            this.checkBox_action_setUID.TabIndex = 0;
            this.checkBox_action_setUID.Text = "记忆 UID(&U)";
            this.checkBox_action_setUID.UseVisualStyleBackColor = true;
            this.checkBox_action_setUID.CheckedChanged += new System.EventHandler(this.checkBoxes_CheckedChanged);
            // 
            // tabPage_other
            // 
            this.tabPage_other.Location = new System.Drawing.Point(4, 37);
            this.tabPage_other.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_other.Name = "tabPage_other";
            this.tabPage_other.Size = new System.Drawing.Size(909, 636);
            this.tabPage_other.TabIndex = 1;
            this.tabPage_other.Text = "其它";
            this.tabPage_other.UseVisualStyleBackColor = true;
            // 
            // BeginInventoryDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(945, 765);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "BeginInventoryDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "开始盘点";
            this.Load += new System.EventHandler(this.BeginModifyDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_action.ResumeLayout(false);
            this.tabPage_action.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_action;
        private System.Windows.Forms.TabPage tabPage_other;
        private System.Windows.Forms.CheckBox checkBox_action_setUID;
        private System.Windows.Forms.CheckBox checkBox_action_setCurrentLocation;
        private System.Windows.Forms.CheckBox checkBox_action_setLocation;
        private System.Windows.Forms.CheckBox checkBox_action_verifyEas;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_action_location;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_action_batchNo;
        private System.Windows.Forms.CheckBox checkBox_action_slowMode;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}