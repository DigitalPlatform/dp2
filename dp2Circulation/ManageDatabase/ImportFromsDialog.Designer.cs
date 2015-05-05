namespace dp2Circulation
{
    partial class ImportFromsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportFromsDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.label_comment = new System.Windows.Forms.Label();
            this.button_selectAll = new System.Windows.Forms.Button();
            this.button_unSelectAll = new System.Windows.Forms.Button();
            this.fromEditControl1 = new DigitalPlatform.CommonControl.FromEditControl();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(320, 236);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(260, 236);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(150, 175);
            this.label_message.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(226, 18);
            this.label_message.TabIndex = 9;
            // 
            // label_comment
            // 
            this.label_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_comment.BackColor = System.Drawing.SystemColors.Info;
            this.label_comment.Location = new System.Drawing.Point(9, 200);
            this.label_comment.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_comment.Name = "label_comment";
            this.label_comment.Padding = new System.Windows.Forms.Padding(2);
            this.label_comment.Size = new System.Drawing.Size(368, 34);
            this.label_comment.TabIndex = 10;
            this.label_comment.Text = "请用鼠标点每行左端的区域，选定要导入的检索途径事项。按住Ctrl或Shift键可实现复选。";
            // 
            // button_selectAll
            // 
            this.button_selectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_selectAll.Location = new System.Drawing.Point(9, 175);
            this.button_selectAll.Margin = new System.Windows.Forms.Padding(2);
            this.button_selectAll.Name = "button_selectAll";
            this.button_selectAll.Size = new System.Drawing.Size(56, 22);
            this.button_selectAll.TabIndex = 11;
            this.button_selectAll.Text = "全选(&A)";
            this.button_selectAll.UseVisualStyleBackColor = true;
            this.button_selectAll.Click += new System.EventHandler(this.button_selectAll_Click);
            // 
            // button_unSelectAll
            // 
            this.button_unSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_unSelectAll.Location = new System.Drawing.Point(70, 175);
            this.button_unSelectAll.Margin = new System.Windows.Forms.Padding(2);
            this.button_unSelectAll.Name = "button_unSelectAll";
            this.button_unSelectAll.Size = new System.Drawing.Size(74, 22);
            this.button_unSelectAll.TabIndex = 12;
            this.button_unSelectAll.Text = "全不选(&U)";
            this.button_unSelectAll.UseVisualStyleBackColor = true;
            this.button_unSelectAll.Click += new System.EventHandler(this.button_unSelectAll_Click);
            // 
            // fromEditControl1
            // 
            this.fromEditControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.fromEditControl1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.fromEditControl1.Changed = false;
            this.fromEditControl1.HasCaptionsTitleLine = false;
            this.fromEditControl1.Location = new System.Drawing.Point(9, 10);
            this.fromEditControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.fromEditControl1.Name = "fromEditControl1";
            this.fromEditControl1.Size = new System.Drawing.Size(368, 161);
            this.fromEditControl1.TabIndex = 0;
            this.fromEditControl1.SelectedIndexChanged += new System.EventHandler(this.fromEditControl1_SelectedIndexChanged);
            // 
            // ImportFromsDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(386, 268);
            this.Controls.Add(this.button_unSelectAll);
            this.Controls.Add(this.button_selectAll);
            this.Controls.Add(this.label_comment);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.fromEditControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ImportFromsDialog";
            this.ShowInTaskbar = false;
            this.Text = "导入检索途径";
            this.Load += new System.EventHandler(this.ImportFromsDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.CommonControl.FromEditControl fromEditControl1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Label label_comment;
        private System.Windows.Forms.Button button_selectAll;
        private System.Windows.Forms.Button button_unSelectAll;
    }
}