namespace DigitalPlatform.OPAC
{
    partial class InstanceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstanceDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_deleteInstance = new System.Windows.Forms.Button();
            this.button_modifyInstance = new System.Windows.Forms.Button();
            this.button_newInstance = new System.Windows.Forms.Button();
            this.listView_instance = new System.Windows.Forms.ListView();
            this.columnHeader_site = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_virtualDir = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_dataDir = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_physicalPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_Comment = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(419, 281);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 20;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(338, 281);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 19;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_deleteInstance
            // 
            this.button_deleteInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_deleteInstance.Location = new System.Drawing.Point(205, 252);
            this.button_deleteInstance.Name = "button_deleteInstance";
            this.button_deleteInstance.Size = new System.Drawing.Size(75, 23);
            this.button_deleteInstance.TabIndex = 18;
            this.button_deleteInstance.Text = "删除(&D)";
            this.button_deleteInstance.UseVisualStyleBackColor = true;
            this.button_deleteInstance.Click += new System.EventHandler(this.button_deleteInstance_Click);
            // 
            // button_modifyInstance
            // 
            this.button_modifyInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_modifyInstance.Location = new System.Drawing.Point(109, 252);
            this.button_modifyInstance.Name = "button_modifyInstance";
            this.button_modifyInstance.Size = new System.Drawing.Size(90, 23);
            this.button_modifyInstance.TabIndex = 17;
            this.button_modifyInstance.Text = "修改(&M)...";
            this.button_modifyInstance.UseVisualStyleBackColor = true;
            this.button_modifyInstance.Click += new System.EventHandler(this.button_modifyInstance_Click);
            // 
            // button_newInstance
            // 
            this.button_newInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newInstance.Location = new System.Drawing.Point(12, 252);
            this.button_newInstance.Name = "button_newInstance";
            this.button_newInstance.Size = new System.Drawing.Size(91, 23);
            this.button_newInstance.TabIndex = 16;
            this.button_newInstance.Text = "新增(&N)...";
            this.button_newInstance.UseVisualStyleBackColor = true;
            this.button_newInstance.Click += new System.EventHandler(this.button_newInstance_Click);
            // 
            // listView_instance
            // 
            this.listView_instance.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_instance.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_site,
            this.columnHeader_virtualDir,
            this.columnHeader_errorInfo,
            this.columnHeader_dataDir,
            this.columnHeader_physicalPath});
            this.listView_instance.FullRowSelect = true;
            this.listView_instance.HideSelection = false;
            this.listView_instance.Location = new System.Drawing.Point(12, 62);
            this.listView_instance.MultiSelect = false;
            this.listView_instance.Name = "listView_instance";
            this.listView_instance.Size = new System.Drawing.Size(482, 184);
            this.listView_instance.TabIndex = 15;
            this.listView_instance.UseCompatibleStateImageBehavior = false;
            this.listView_instance.View = System.Windows.Forms.View.Details;
            this.listView_instance.SelectedIndexChanged += new System.EventHandler(this.listView_instance_SelectedIndexChanged);
            this.listView_instance.DoubleClick += new System.EventHandler(this.listView_instance_DoubleClick);
            // 
            // columnHeader_site
            // 
            this.columnHeader_site.Text = "网站";
            this.columnHeader_site.Width = 114;
            // 
            // columnHeader_virtualDir
            // 
            this.columnHeader_virtualDir.Text = "虚拟目录";
            this.columnHeader_virtualDir.Width = 129;
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "出错信息";
            // 
            // columnHeader_dataDir
            // 
            this.columnHeader_dataDir.Text = "数据目录";
            this.columnHeader_dataDir.Width = 189;
            // 
            // columnHeader_physicalPath
            // 
            this.columnHeader_physicalPath.Text = "应用目录";
            this.columnHeader_physicalPath.Width = 374;
            // 
            // textBox_Comment
            // 
            this.textBox_Comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Comment.Location = new System.Drawing.Point(12, 12);
            this.textBox_Comment.Multiline = true;
            this.textBox_Comment.Name = "textBox_Comment";
            this.textBox_Comment.ReadOnly = true;
            this.textBox_Comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_Comment.Size = new System.Drawing.Size(482, 44);
            this.textBox_Comment.TabIndex = 14;
            this.textBox_Comment.Text = "dp2OPAC 可以安装多个实例。每个实例具有独立的虚拟目录和数据目录。";
            // 
            // InstanceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 316);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_deleteInstance);
            this.Controls.Add(this.button_modifyInstance);
            this.Controls.Add(this.button_newInstance);
            this.Controls.Add(this.listView_instance);
            this.Controls.Add(this.textBox_Comment);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InstanceDialog";
            this.ShowInTaskbar = false;
            this.Text = "实例设置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InstanceDialog_FormClosed);
            this.Load += new System.EventHandler(this.InstanceDialog_Load);
            this.Move += new System.EventHandler(this.InstanceDialog_Move);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_deleteInstance;
        private System.Windows.Forms.Button button_modifyInstance;
        private System.Windows.Forms.Button button_newInstance;
        private System.Windows.Forms.ListView listView_instance;
        private System.Windows.Forms.ColumnHeader columnHeader_virtualDir;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_dataDir;
        private System.Windows.Forms.ColumnHeader columnHeader_physicalPath;
        private System.Windows.Forms.TextBox textBox_Comment;
        private System.Windows.Forms.ColumnHeader columnHeader_site;
    }
}
