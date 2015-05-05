namespace dp2Circulation
{
    partial class ChangeStateActionControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.checkedComboBox_stateRemove = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkedComboBox_stateAdd = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_state = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // checkedComboBox_stateRemove
            // 
            this.checkedComboBox_stateRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateRemove.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateRemove.Location = new System.Drawing.Point(65, 48);
            this.checkedComboBox_stateRemove.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateRemove.Name = "checkedComboBox_stateRemove";
            this.checkedComboBox_stateRemove.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateRemove.Size = new System.Drawing.Size(163, 22);
            this.checkedComboBox_stateRemove.TabIndex = 13;
            this.checkedComboBox_stateRemove.DropDown += new System.EventHandler(this.checkedComboBox_stateRemove_DropDown);
            this.checkedComboBox_stateRemove.TextChanged += new System.EventHandler(this.checkedComboBox_stateRemove_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1, 50);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "减(&R):";
            // 
            // checkedComboBox_stateAdd
            // 
            this.checkedComboBox_stateAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedComboBox_stateAdd.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_stateAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_stateAdd.Location = new System.Drawing.Point(65, 26);
            this.checkedComboBox_stateAdd.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_stateAdd.Name = "checkedComboBox_stateAdd";
            this.checkedComboBox_stateAdd.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_stateAdd.Size = new System.Drawing.Size(163, 22);
            this.checkedComboBox_stateAdd.TabIndex = 11;
            this.checkedComboBox_stateAdd.DropDown += new System.EventHandler(this.checkedComboBox_stateAdd_DropDown);
            this.checkedComboBox_stateAdd.TextChanged += new System.EventHandler(this.checkedComboBox_stateAdd_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1, 28);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "增(&A):";
            // 
            // comboBox_state
            // 
            this.comboBox_state.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_state.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_state.FormattingEnabled = true;
            this.comboBox_state.Items.AddRange(new object[] {
            "<不改变>",
            "<增、减>"});
            this.comboBox_state.Location = new System.Drawing.Point(2, 2);
            this.comboBox_state.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_state.Name = "comboBox_state";
            this.comboBox_state.Size = new System.Drawing.Size(226, 20);
            this.comboBox_state.TabIndex = 9;
            this.comboBox_state.Text = "<不改变>";
            this.comboBox_state.SizeChanged += new System.EventHandler(this.comboBox_state_SizeChanged);
            this.comboBox_state.TextChanged += new System.EventHandler(this.comboBox_state_TextChanged);
            // 
            // ChangeStateActionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkedComboBox_stateRemove);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkedComboBox_stateAdd);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_state);
            this.Name = "ChangeStateActionControl";
            this.Size = new System.Drawing.Size(230, 72);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateRemove;
        private System.Windows.Forms.Label label4;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_stateAdd;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_state;
    }
}
