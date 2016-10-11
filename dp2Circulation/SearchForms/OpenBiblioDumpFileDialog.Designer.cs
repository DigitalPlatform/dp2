namespace dp2Circulation
{
    partial class OpenBiblioDumpFileDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_biblioDumpFileName = new System.Windows.Forms.TextBox();
            this.button_getBiblioDumpFileName = new System.Windows.Forms.Button();
            this.button_getObjectDirectoryName = new System.Windows.Forms.Button();
            this.textBox_objectDirectoryName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBox_includeObjectFile = new System.Windows.Forms.CheckBox();
            this.checkBox_includeEntities = new System.Windows.Forms.CheckBox();
            this.checkBox_includeIssues = new System.Windows.Forms.CheckBox();
            this.checkBox_includeOrders = new System.Windows.Forms.CheckBox();
            this.checkBox_includeComments = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "书目转储文件名(&F):";
            // 
            // textBox_biblioDumpFileName
            // 
            this.textBox_biblioDumpFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblioDumpFileName.Location = new System.Drawing.Point(12, 31);
            this.textBox_biblioDumpFileName.Name = "textBox_biblioDumpFileName";
            this.textBox_biblioDumpFileName.Size = new System.Drawing.Size(380, 21);
            this.textBox_biblioDumpFileName.TabIndex = 1;
            this.textBox_biblioDumpFileName.TextChanged += new System.EventHandler(this.textBox_biblioDumpFileName_TextChanged);
            // 
            // button_getBiblioDumpFileName
            // 
            this.button_getBiblioDumpFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getBiblioDumpFileName.Location = new System.Drawing.Point(398, 31);
            this.button_getBiblioDumpFileName.Name = "button_getBiblioDumpFileName";
            this.button_getBiblioDumpFileName.Size = new System.Drawing.Size(48, 23);
            this.button_getBiblioDumpFileName.TabIndex = 2;
            this.button_getBiblioDumpFileName.Text = "...";
            this.button_getBiblioDumpFileName.UseVisualStyleBackColor = true;
            this.button_getBiblioDumpFileName.Click += new System.EventHandler(this.button_getBiblioDumpFileName_Click);
            // 
            // button_getObjectDirectoryName
            // 
            this.button_getObjectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getObjectDirectoryName.Location = new System.Drawing.Point(398, 191);
            this.button_getObjectDirectoryName.Name = "button_getObjectDirectoryName";
            this.button_getObjectDirectoryName.Size = new System.Drawing.Size(48, 23);
            this.button_getObjectDirectoryName.TabIndex = 10;
            this.button_getObjectDirectoryName.Text = "...";
            this.button_getObjectDirectoryName.UseVisualStyleBackColor = true;
            this.button_getObjectDirectoryName.Click += new System.EventHandler(this.button_getObjectDirectoryName_Click);
            // 
            // textBox_objectDirectoryName
            // 
            this.textBox_objectDirectoryName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_objectDirectoryName.Location = new System.Drawing.Point(36, 193);
            this.textBox_objectDirectoryName.Name = "textBox_objectDirectoryName";
            this.textBox_objectDirectoryName.Size = new System.Drawing.Size(356, 21);
            this.textBox_objectDirectoryName.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 178);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "对象文件目录(&O):";
            // 
            // checkBox_includeObjectFile
            // 
            this.checkBox_includeObjectFile.AutoSize = true;
            this.checkBox_includeObjectFile.Checked = true;
            this.checkBox_includeObjectFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_includeObjectFile.Location = new System.Drawing.Point(12, 159);
            this.checkBox_includeObjectFile.Name = "checkBox_includeObjectFile";
            this.checkBox_includeObjectFile.Size = new System.Drawing.Size(114, 16);
            this.checkBox_includeObjectFile.TabIndex = 7;
            this.checkBox_includeObjectFile.Text = "包含对象文件(&O)";
            this.checkBox_includeObjectFile.UseVisualStyleBackColor = true;
            this.checkBox_includeObjectFile.CheckedChanged += new System.EventHandler(this.checkBox_includeObjectFile_CheckedChanged);
            // 
            // checkBox_includeEntities
            // 
            this.checkBox_includeEntities.AutoSize = true;
            this.checkBox_includeEntities.Checked = true;
            this.checkBox_includeEntities.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_includeEntities.Location = new System.Drawing.Point(12, 65);
            this.checkBox_includeEntities.Name = "checkBox_includeEntities";
            this.checkBox_includeEntities.Size = new System.Drawing.Size(102, 16);
            this.checkBox_includeEntities.TabIndex = 3;
            this.checkBox_includeEntities.Text = "包含册记录(&E)";
            this.checkBox_includeEntities.UseVisualStyleBackColor = true;
            // 
            // checkBox_includeIssues
            // 
            this.checkBox_includeIssues.AutoSize = true;
            this.checkBox_includeIssues.Checked = true;
            this.checkBox_includeIssues.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_includeIssues.Location = new System.Drawing.Point(12, 87);
            this.checkBox_includeIssues.Name = "checkBox_includeIssues";
            this.checkBox_includeIssues.Size = new System.Drawing.Size(102, 16);
            this.checkBox_includeIssues.TabIndex = 4;
            this.checkBox_includeIssues.Text = "包含期记录(&I)";
            this.checkBox_includeIssues.UseVisualStyleBackColor = true;
            // 
            // checkBox_includeOrders
            // 
            this.checkBox_includeOrders.AutoSize = true;
            this.checkBox_includeOrders.Checked = true;
            this.checkBox_includeOrders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_includeOrders.Location = new System.Drawing.Point(12, 107);
            this.checkBox_includeOrders.Name = "checkBox_includeOrders";
            this.checkBox_includeOrders.Size = new System.Drawing.Size(114, 16);
            this.checkBox_includeOrders.TabIndex = 5;
            this.checkBox_includeOrders.Text = "包含订购记录(&O)";
            this.checkBox_includeOrders.UseVisualStyleBackColor = true;
            // 
            // checkBox_includeComments
            // 
            this.checkBox_includeComments.AutoSize = true;
            this.checkBox_includeComments.Checked = true;
            this.checkBox_includeComments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_includeComments.Location = new System.Drawing.Point(12, 129);
            this.checkBox_includeComments.Name = "checkBox_includeComments";
            this.checkBox_includeComments.Size = new System.Drawing.Size(114, 16);
            this.checkBox_includeComments.TabIndex = 6;
            this.checkBox_includeComments.Text = "包含评注记录(&C)";
            this.checkBox_includeComments.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(371, 247);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 12;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(290, 247);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // OpenBiblioDumpFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(458, 282);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.checkBox_includeComments);
            this.Controls.Add(this.checkBox_includeOrders);
            this.Controls.Add(this.checkBox_includeIssues);
            this.Controls.Add(this.checkBox_includeEntities);
            this.Controls.Add(this.checkBox_includeObjectFile);
            this.Controls.Add(this.button_getObjectDirectoryName);
            this.Controls.Add(this.textBox_objectDirectoryName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_getBiblioDumpFileName);
            this.Controls.Add(this.textBox_biblioDumpFileName);
            this.Controls.Add(this.label1);
            this.Name = "OpenBiblioDumpFileDialog";
            this.ShowInTaskbar = false;
            this.Text = "打开书目转储文件";
            this.Load += new System.EventHandler(this.OpenBiblioDumpFileDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_biblioDumpFileName;
        private System.Windows.Forms.Button button_getBiblioDumpFileName;
        private System.Windows.Forms.Button button_getObjectDirectoryName;
        private System.Windows.Forms.TextBox textBox_objectDirectoryName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox_includeObjectFile;
        private System.Windows.Forms.CheckBox checkBox_includeEntities;
        private System.Windows.Forms.CheckBox checkBox_includeIssues;
        private System.Windows.Forms.CheckBox checkBox_includeOrders;
        private System.Windows.Forms.CheckBox checkBox_includeComments;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}