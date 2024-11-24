namespace dp2Circulation
{
    partial class GetDbNameDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetDbNameDlg));
            this.label_dbNameList = new System.Windows.Forms.Label();
            this.listBox_biblioDbNames = new System.Windows.Forms.ListBox();
            this.label_dbName = new System.Windows.Forms.Label();
            this.textBox_dbName = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_notAsk = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label_dbNameList
            // 
            this.label_dbNameList.AutoSize = true;
            this.label_dbNameList.Location = new System.Drawing.Point(15, 19);
            this.label_dbNameList.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_dbNameList.Name = "label_dbNameList";
            this.label_dbNameList.Size = new System.Drawing.Size(180, 21);
            this.label_dbNameList.TabIndex = 0;
            this.label_dbNameList.Text = "书目库名列表(&L):";
            // 
            // listBox_biblioDbNames
            // 
            this.listBox_biblioDbNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_biblioDbNames.FormattingEnabled = true;
            this.listBox_biblioDbNames.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.listBox_biblioDbNames.ItemHeight = 21;
            this.listBox_biblioDbNames.Location = new System.Drawing.Point(18, 46);
            this.listBox_biblioDbNames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listBox_biblioDbNames.Name = "listBox_biblioDbNames";
            this.listBox_biblioDbNames.Size = new System.Drawing.Size(585, 130);
            this.listBox_biblioDbNames.TabIndex = 1;
            this.listBox_biblioDbNames.SelectedIndexChanged += new System.EventHandler(this.listBox_dbNames_SelectedIndexChanged);
            this.listBox_biblioDbNames.DoubleClick += new System.EventHandler(this.listBox_dbNames_DoubleClick);
            // 
            // label_dbName
            // 
            this.label_dbName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_dbName.AutoSize = true;
            this.label_dbName.Location = new System.Drawing.Point(15, 203);
            this.label_dbName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_dbName.Name = "label_dbName";
            this.label_dbName.Size = new System.Drawing.Size(138, 21);
            this.label_dbName.TabIndex = 2;
            this.label_dbName.Text = "书目库名(&N):";
            // 
            // textBox_dbName
            // 
            this.textBox_dbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dbName.Location = new System.Drawing.Point(158, 200);
            this.textBox_dbName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_dbName.Name = "textBox_dbName";
            this.textBox_dbName.Size = new System.Drawing.Size(334, 31);
            this.textBox_dbName.TabIndex = 3;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(502, 200);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(502, 247);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_notAsk
            // 
            this.checkBox_notAsk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_notAsk.AutoSize = true;
            this.checkBox_notAsk.Enabled = false;
            this.checkBox_notAsk.Location = new System.Drawing.Point(18, 260);
            this.checkBox_notAsk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox_notAsk.Name = "checkBox_notAsk";
            this.checkBox_notAsk.Size = new System.Drawing.Size(246, 25);
            this.checkBox_notAsk.TabIndex = 6;
            this.checkBox_notAsk.Text = "下次不再出现此对话框";
            // 
            // GetDbNameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(622, 303);
            this.Controls.Add(this.checkBox_notAsk);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_dbName);
            this.Controls.Add(this.label_dbName);
            this.Controls.Add(this.listBox_biblioDbNames);
            this.Controls.Add(this.label_dbNameList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "GetDbNameDlg";
            this.ShowInTaskbar = false;
            this.Text = "指定书目库名";
            this.Load += new System.EventHandler(this.GetDbNameDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_dbNameList;
        private System.Windows.Forms.ListBox listBox_biblioDbNames;
        private System.Windows.Forms.Label label_dbName;
        private System.Windows.Forms.TextBox textBox_dbName;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_notAsk;
    }
}