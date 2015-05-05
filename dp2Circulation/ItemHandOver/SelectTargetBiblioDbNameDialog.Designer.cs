namespace dp2Circulation
{
    partial class SelectTargetBiblioDbNameDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectTargetBiblioDbNameDialog));
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.listView_dbNames = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_dbName = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_dbName = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(13, 13);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_message.Size = new System.Drawing.Size(417, 101);
            this.textBox_message.TabIndex = 5;
            // 
            // listView_dbNames
            // 
            this.listView_dbNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.listView_dbNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_dbName,
            this.columnHeader_comment});
            this.listView_dbNames.FullRowSelect = true;
            this.listView_dbNames.HideSelection = false;
            this.listView_dbNames.Location = new System.Drawing.Point(13, 120);
            this.listView_dbNames.Name = "listView_dbNames";
            this.listView_dbNames.Size = new System.Drawing.Size(417, 144);
            this.listView_dbNames.TabIndex = 0;
            this.listView_dbNames.UseCompatibleStateImageBehavior = false;
            this.listView_dbNames.View = System.Windows.Forms.View.Details;
            this.listView_dbNames.SelectedIndexChanged += new System.EventHandler(this.listView_dbNames_SelectedIndexChanged);
            this.listView_dbNames.DoubleClick += new System.EventHandler(this.listView_dbNames_DoubleClick);
            // 
            // columnHeader_dbName
            // 
            this.columnHeader_dbName.Text = "书目库名";
            this.columnHeader_dbName.Width = 150;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 200;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 277);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "书目库名(&D):";
            // 
            // textBox_dbName
            // 
            this.textBox_dbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dbName.Location = new System.Drawing.Point(125, 270);
            this.textBox_dbName.Name = "textBox_dbName";
            this.textBox_dbName.Size = new System.Drawing.Size(212, 25);
            this.textBox_dbName.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(355, 270);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(355, 304);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // SelectTargetBiblioDbNameDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(442, 344);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_dbName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_dbNames);
            this.Controls.Add(this.textBox_message);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelectTargetBiblioDbNameDialog";
            this.ShowInTaskbar = false;
            this.Text = "指定目标书目库";
            this.Load += new System.EventHandler(this.SelectTargetBiblioDbNameDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_message;
        private DigitalPlatform.GUI.ListViewNF listView_dbNames;
        private System.Windows.Forms.ColumnHeader columnHeader_dbName;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dbName;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
    }
}