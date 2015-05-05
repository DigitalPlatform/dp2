namespace DigitalPlatform.DTLP
{
    partial class DupCfgDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DupCfgDialog));
            this.listView_projects = new System.Windows.Forms.ListView();
            this.columnHeader_name = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.button_newProject = new System.Windows.Forms.Button();
            this.button_modifyProject = new System.Windows.Forms.Button();
            this.button_deleteProject = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.panel_projects = new System.Windows.Forms.Panel();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.button_deleteDefault = new System.Windows.Forms.Button();
            this.button_newDefault = new System.Windows.Forms.Button();
            this.button_modifyDefaut = new System.Windows.Forms.Button();
            this.listView_defaults = new System.Windows.Forms.ListView();
            this.columnHeader_databaseName = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_defaultProjectName = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.button_upgradeFromGcsIni = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_projects = new System.Windows.Forms.TabPage();
            this.tabPage_command = new System.Windows.Forms.TabPage();
            this.button_viewDupXml = new System.Windows.Forms.Button();
            this.panel_projects.SuspendLayout();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_projects.SuspendLayout();
            this.tabPage_command.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_projects
            // 
            this.listView_projects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_projects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_comment});
            this.listView_projects.Font = new System.Drawing.Font("SimSun", 9F);
            this.listView_projects.FullRowSelect = true;
            this.listView_projects.HideSelection = false;
            this.listView_projects.Location = new System.Drawing.Point(0, 0);
            this.listView_projects.Name = "listView_projects";
            this.listView_projects.Size = new System.Drawing.Size(390, 133);
            this.listView_projects.TabIndex = 0;
            this.listView_projects.UseCompatibleStateImageBehavior = false;
            this.listView_projects.View = System.Windows.Forms.View.Details;
            this.listView_projects.DoubleClick += new System.EventHandler(this.listView_projects_DoubleClick);
            this.listView_projects.SelectedIndexChanged += new System.EventHandler(this.listView_projects_SelectedIndexChanged);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "查重方案";
            this.columnHeader_name.Width = 170;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 300;
            // 
            // button_newProject
            // 
            this.button_newProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newProject.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_newProject.Location = new System.Drawing.Point(0, 139);
            this.button_newProject.Name = "button_newProject";
            this.button_newProject.Size = new System.Drawing.Size(75, 28);
            this.button_newProject.TabIndex = 1;
            this.button_newProject.Text = "新增(&N)";
            this.button_newProject.UseVisualStyleBackColor = true;
            this.button_newProject.Click += new System.EventHandler(this.button_newProject_Click);
            // 
            // button_modifyProject
            // 
            this.button_modifyProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_modifyProject.Enabled = false;
            this.button_modifyProject.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_modifyProject.Location = new System.Drawing.Point(81, 139);
            this.button_modifyProject.Name = "button_modifyProject";
            this.button_modifyProject.Size = new System.Drawing.Size(75, 28);
            this.button_modifyProject.TabIndex = 2;
            this.button_modifyProject.Text = "修改(&M)";
            this.button_modifyProject.UseVisualStyleBackColor = true;
            this.button_modifyProject.Click += new System.EventHandler(this.button_modifyProject_Click);
            // 
            // button_deleteProject
            // 
            this.button_deleteProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_deleteProject.Enabled = false;
            this.button_deleteProject.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_deleteProject.Location = new System.Drawing.Point(178, 139);
            this.button_deleteProject.Name = "button_deleteProject";
            this.button_deleteProject.Size = new System.Drawing.Size(75, 28);
            this.button_deleteProject.TabIndex = 3;
            this.button_deleteProject.Text = "删除(&D)";
            this.button_deleteProject.UseVisualStyleBackColor = true;
            this.button_deleteProject.Click += new System.EventHandler(this.button_deleteProject_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(341, 353);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // panel_projects
            // 
            this.panel_projects.Controls.Add(this.listView_projects);
            this.panel_projects.Controls.Add(this.button_newProject);
            this.panel_projects.Controls.Add(this.button_deleteProject);
            this.panel_projects.Controls.Add(this.button_modifyProject);
            this.panel_projects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_projects.Location = new System.Drawing.Point(0, 0);
            this.panel_projects.Name = "panel_projects";
            this.panel_projects.Size = new System.Drawing.Size(390, 167);
            this.panel_projects.TabIndex = 0;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(3, 3);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_projects);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.button_deleteDefault);
            this.splitContainer_main.Panel2.Controls.Add(this.button_newDefault);
            this.splitContainer_main.Panel2.Controls.Add(this.button_modifyDefaut);
            this.splitContainer_main.Panel2.Controls.Add(this.listView_defaults);
            this.splitContainer_main.Panel2.Controls.Add(this.label1);
            this.splitContainer_main.Size = new System.Drawing.Size(390, 301);
            this.splitContainer_main.SplitterDistance = 167;
            this.splitContainer_main.SplitterWidth = 16;
            this.splitContainer_main.TabIndex = 0;
            // 
            // button_deleteDefault
            // 
            this.button_deleteDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_deleteDefault.Enabled = false;
            this.button_deleteDefault.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_deleteDefault.Location = new System.Drawing.Point(178, 90);
            this.button_deleteDefault.Name = "button_deleteDefault";
            this.button_deleteDefault.Size = new System.Drawing.Size(75, 28);
            this.button_deleteDefault.TabIndex = 4;
            this.button_deleteDefault.Text = "删除(&D)";
            this.button_deleteDefault.UseVisualStyleBackColor = true;
            this.button_deleteDefault.Click += new System.EventHandler(this.button_deleteDefault_Click);
            // 
            // button_newDefault
            // 
            this.button_newDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_newDefault.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_newDefault.Location = new System.Drawing.Point(0, 90);
            this.button_newDefault.Name = "button_newDefault";
            this.button_newDefault.Size = new System.Drawing.Size(75, 28);
            this.button_newDefault.TabIndex = 2;
            this.button_newDefault.Text = "新增(&N)";
            this.button_newDefault.UseVisualStyleBackColor = true;
            this.button_newDefault.Click += new System.EventHandler(this.button_newDefault_Click);
            // 
            // button_modifyDefaut
            // 
            this.button_modifyDefaut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_modifyDefaut.Enabled = false;
            this.button_modifyDefaut.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_modifyDefaut.Location = new System.Drawing.Point(81, 90);
            this.button_modifyDefaut.Name = "button_modifyDefaut";
            this.button_modifyDefaut.Size = new System.Drawing.Size(75, 28);
            this.button_modifyDefaut.TabIndex = 3;
            this.button_modifyDefaut.Text = "修改(&M)";
            this.button_modifyDefaut.UseVisualStyleBackColor = true;
            this.button_modifyDefaut.Click += new System.EventHandler(this.button_modifyDefaut_Click);
            // 
            // listView_defaults
            // 
            this.listView_defaults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_defaults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_databaseName,
            this.columnHeader_defaultProjectName});
            this.listView_defaults.FullRowSelect = true;
            this.listView_defaults.HideSelection = false;
            this.listView_defaults.Location = new System.Drawing.Point(0, 22);
            this.listView_defaults.MultiSelect = false;
            this.listView_defaults.Name = "listView_defaults";
            this.listView_defaults.Size = new System.Drawing.Size(390, 62);
            this.listView_defaults.TabIndex = 1;
            this.listView_defaults.UseCompatibleStateImageBehavior = false;
            this.listView_defaults.View = System.Windows.Forms.View.Details;
            this.listView_defaults.DoubleClick += new System.EventHandler(this.listView_defaults_DoubleClick);
            this.listView_defaults.SelectedIndexChanged += new System.EventHandler(this.listView_defaults_SelectedIndexChanged);
            // 
            // columnHeader_databaseName
            // 
            this.columnHeader_databaseName.Text = "数据库";
            this.columnHeader_databaseName.Width = 230;
            // 
            // columnHeader_defaultProjectName
            // 
            this.columnHeader_defaultProjectName.Text = "缺省查重方案名";
            this.columnHeader_defaultProjectName.Width = 166;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-1, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "缺省关系(&F):";
            // 
            // button_upgradeFromGcsIni
            // 
            this.button_upgradeFromGcsIni.Location = new System.Drawing.Point(6, 52);
            this.button_upgradeFromGcsIni.Name = "button_upgradeFromGcsIni";
            this.button_upgradeFromGcsIni.Size = new System.Drawing.Size(242, 28);
            this.button_upgradeFromGcsIni.TabIndex = 1;
            this.button_upgradeFromGcsIni.Text = "从gcs.ini获取查重配置(&U)...";
            this.button_upgradeFromGcsIni.UseVisualStyleBackColor = true;
            this.button_upgradeFromGcsIni.Click += new System.EventHandler(this.button_upgradeFromGcsIni_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_projects);
            this.tabControl_main.Controls.Add(this.tabPage_command);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(404, 335);
            this.tabControl_main.TabIndex = 3;
            // 
            // tabPage_projects
            // 
            this.tabPage_projects.Controls.Add(this.splitContainer_main);
            this.tabPage_projects.Location = new System.Drawing.Point(4, 24);
            this.tabPage_projects.Name = "tabPage_projects";
            this.tabPage_projects.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_projects.Size = new System.Drawing.Size(396, 307);
            this.tabPage_projects.TabIndex = 0;
            this.tabPage_projects.Text = "查重方案";
            this.tabPage_projects.UseVisualStyleBackColor = true;
            // 
            // tabPage_command
            // 
            this.tabPage_command.Controls.Add(this.button_viewDupXml);
            this.tabPage_command.Controls.Add(this.button_upgradeFromGcsIni);
            this.tabPage_command.Location = new System.Drawing.Point(4, 24);
            this.tabPage_command.Name = "tabPage_command";
            this.tabPage_command.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_command.Size = new System.Drawing.Size(396, 307);
            this.tabPage_command.TabIndex = 1;
            this.tabPage_command.Text = "其它";
            this.tabPage_command.UseVisualStyleBackColor = true;
            // 
            // button_viewDupXml
            // 
            this.button_viewDupXml.Location = new System.Drawing.Point(6, 18);
            this.button_viewDupXml.Name = "button_viewDupXml";
            this.button_viewDupXml.Size = new System.Drawing.Size(242, 28);
            this.button_viewDupXml.TabIndex = 2;
            this.button_viewDupXml.Text = "打开查重配置文件(&V)...";
            this.button_viewDupXml.UseVisualStyleBackColor = true;
            this.button_viewDupXml.Click += new System.EventHandler(this.button_viewDupXml_Click);
            // 
            // DupCfgDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(428, 393);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DupCfgDialog";
            this.ShowInTaskbar = false;
            this.Text = "全部查重方案";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DupCfgDialog_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DupCfgDialog_FormClosing);
            this.Load += new System.EventHandler(this.DupCfgDialog_Load);
            this.panel_projects.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.Panel2.PerformLayout();
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_projects.ResumeLayout(false);
            this.tabPage_command.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_projects;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.Button button_newProject;
        private System.Windows.Forms.Button button_modifyProject;
        private System.Windows.Forms.Button button_deleteProject;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Panel panel_projects;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listView_defaults;
        private System.Windows.Forms.ColumnHeader columnHeader_databaseName;
        private System.Windows.Forms.ColumnHeader columnHeader_defaultProjectName;
        private System.Windows.Forms.Button button_modifyDefaut;
        private System.Windows.Forms.Button button_upgradeFromGcsIni;
        private System.Windows.Forms.Button button_deleteDefault;
        private System.Windows.Forms.Button button_newDefault;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_projects;
        private System.Windows.Forms.TabPage tabPage_command;
        private System.Windows.Forms.Button button_viewDupXml;
    }
}