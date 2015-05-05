namespace DigitalPlatform.Script
{
    partial class ScriptActionMenuDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptActionMenuDlg));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_autoRun = new System.Windows.Forms.CheckBox();
            this.ActionTable = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_name = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_shortcutKey = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_comment = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_entry = new DigitalPlatform.CommonControl.DpColumn();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(199, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(280, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_autoRun
            // 
            this.checkBox_autoRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoRun.AutoSize = true;
            this.checkBox_autoRun.Location = new System.Drawing.Point(12, 233);
            this.checkBox_autoRun.Name = "checkBox_autoRun";
            this.checkBox_autoRun.Size = new System.Drawing.Size(138, 16);
            this.checkBox_autoRun.TabIndex = 1;
            this.checkBox_autoRun.Text = "自动执行加亮事项(&A)";
            this.checkBox_autoRun.UseVisualStyleBackColor = true;
            // 
            // ActionTable
            // 
            this.ActionTable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ActionTable.AutoDocCenter = true;
            this.ActionTable.BackColor = System.Drawing.SystemColors.Window;
            this.ActionTable.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ActionTable.Columns.Add(this.dpColumn_name);
            this.ActionTable.Columns.Add(this.dpColumn_shortcutKey);
            this.ActionTable.Columns.Add(this.dpColumn_comment);
            this.ActionTable.Columns.Add(this.dpColumn_entry);
            this.ActionTable.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.ActionTable.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.ActionTable.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.ActionTable.DocumentMargin = new System.Windows.Forms.Padding(8);
            this.ActionTable.DocumentOrgX = ((long)(0));
            this.ActionTable.DocumentOrgY = ((long)(0));
            this.ActionTable.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ActionTable.FocusedItem = null;
            this.ActionTable.FullRowSelect = true;
            this.ActionTable.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.ActionTable.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.ActionTable.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.ActionTable.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.ActionTable.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ActionTable.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.ActionTable.Location = new System.Drawing.Point(12, 12);
            this.ActionTable.Name = "ActionTable";
            this.ActionTable.Padding = new System.Windows.Forms.Padding(16);
            this.ActionTable.Size = new System.Drawing.Size(343, 211);
            this.ActionTable.TabIndex = 0;
            this.ActionTable.Text = "dpTable1";
            this.ActionTable.DoubleClick += new System.EventHandler(this.dpTable1_DoubleClick);
            this.ActionTable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dpTable1_KeyDown);
            // 
            // dpColumn_name
            // 
            this.dpColumn_name.Alignment = System.Drawing.StringAlignment.Far;
            this.dpColumn_name.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_name.Font = null;
            this.dpColumn_name.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_name.Text = "名称";
            this.dpColumn_name.Width = 148;
            // 
            // dpColumn_shortcutKey
            // 
            this.dpColumn_shortcutKey.Alignment = System.Drawing.StringAlignment.Center;
            this.dpColumn_shortcutKey.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_shortcutKey.Font = new System.Drawing.Font("微软雅黑", 7.5F);
            this.dpColumn_shortcutKey.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_shortcutKey.Text = "快捷键";
            this.dpColumn_shortcutKey.Width = 45;
            // 
            // dpColumn_comment
            // 
            this.dpColumn_comment.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_comment.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_comment.Font = null;
            this.dpColumn_comment.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_comment.Text = "说明";
            this.dpColumn_comment.Width = 204;
            // 
            // dpColumn_entry
            // 
            this.dpColumn_entry.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_entry.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_entry.Font = null;
            this.dpColumn_entry.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_entry.Text = "入口函数";
            this.dpColumn_entry.Width = 150;
            // 
            // ScriptActionMenuDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(367, 264);
            this.Controls.Add(this.ActionTable);
            this.Controls.Add(this.checkBox_autoRun);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ScriptActionMenuDlg";
            this.ShowInTaskbar = false;
            this.Text = "选择脚本功能";
            this.Load += new System.EventHandler(this.ScriptActionMenuDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_autoRun;
        private CommonControl.DpColumn dpColumn_name;
        private CommonControl.DpColumn dpColumn_comment;
        private CommonControl.DpColumn dpColumn_entry;
        private CommonControl.DpColumn dpColumn_shortcutKey;
        public CommonControl.DpTable ActionTable;
    }
}