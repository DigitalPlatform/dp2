namespace DigitalPlatform.DTLP
{
    partial class ProjectDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_projectName = new System.Windows.Forms.TextBox();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_databases = new System.Windows.Forms.ListView();
            this.columnHeader_targetDatabase = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_threshold = new System.Windows.Forms.ColumnHeader();
            this.label3 = new System.Windows.Forms.Label();
            this.listView_accessPoints = new System.Windows.Forms.ListView();
            this.columnHeader_fromName = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_weight = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_searchStyle = new System.Windows.Forms.ColumnHeader();
            this.button_deleteAccessPoint = new System.Windows.Forms.Button();
            this.button_modifyAccessPoint = new System.Windows.Forms.Button();
            this.button_newAccessPoint = new System.Windows.Forms.Button();
            this.button_deleteDatabase = new System.Windows.Forms.Button();
            this.button_modifyDatabase = new System.Windows.Forms.Button();
            this.button_newDatabase = new System.Windows.Forms.Button();
            this.panel_accessPoints = new System.Windows.Forms.Panel();
            this.panel_databases = new System.Windows.Forms.Panel();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_accessPoints.SuspendLayout();
            this.panel_databases.SuspendLayout();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "查重方案名(&N):";
            // 
            // textBox_projectName
            // 
            this.textBox_projectName.Location = new System.Drawing.Point(146, 12);
            this.textBox_projectName.Name = "textBox_projectName";
            this.textBox_projectName.Size = new System.Drawing.Size(182, 25);
            this.textBox_projectName.TabIndex = 1;
            this.textBox_projectName.TextChanged += new System.EventHandler(this.textBox_projectName_TextChanged);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Location = new System.Drawing.Point(146, 44);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(274, 55);
            this.textBox_comment.TabIndex = 3;
            this.textBox_comment.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "说明(&C):";
            // 
            // button_OK
            // 
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(334, 12);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(86, 28);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_databases
            // 
            this.listView_databases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_databases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_targetDatabase,
            this.columnHeader_threshold});
            this.listView_databases.FullRowSelect = true;
            this.listView_databases.HideSelection = false;
            this.listView_databases.Location = new System.Drawing.Point(0, 0);
            this.listView_databases.MultiSelect = false;
            this.listView_databases.Name = "listView_databases";
            this.listView_databases.Size = new System.Drawing.Size(315, 133);
            this.listView_databases.TabIndex = 0;
            this.listView_databases.UseCompatibleStateImageBehavior = false;
            this.listView_databases.View = System.Windows.Forms.View.Details;
            this.listView_databases.DoubleClick += new System.EventHandler(this.listView_databases_DoubleClick);
            this.listView_databases.SelectedIndexChanged += new System.EventHandler(this.listView_databases_SelectedIndexChanged);
            // 
            // columnHeader_targetDatabase
            // 
            this.columnHeader_targetDatabase.Text = "目标库";
            this.columnHeader_targetDatabase.Width = 200;
            // 
            // columnHeader_threshold
            // 
            this.columnHeader_threshold.Text = "阈值";
            this.columnHeader_threshold.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_threshold.Width = 100;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-3, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "检索点(&A):";
            // 
            // listView_accessPoints
            // 
            this.listView_accessPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_accessPoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_fromName,
            this.columnHeader_weight,
            this.columnHeader_searchStyle});
            this.listView_accessPoints.FullRowSelect = true;
            this.listView_accessPoints.HideSelection = false;
            this.listView_accessPoints.Location = new System.Drawing.Point(0, 19);
            this.listView_accessPoints.MultiSelect = false;
            this.listView_accessPoints.Name = "listView_accessPoints";
            this.listView_accessPoints.Size = new System.Drawing.Size(315, 168);
            this.listView_accessPoints.TabIndex = 1;
            this.listView_accessPoints.UseCompatibleStateImageBehavior = false;
            this.listView_accessPoints.View = System.Windows.Forms.View.Details;
            this.listView_accessPoints.DoubleClick += new System.EventHandler(this.listView_accessPoints_DoubleClick);
            this.listView_accessPoints.SelectedIndexChanged += new System.EventHandler(this.listView_accessPoints_SelectedIndexChanged);
            // 
            // columnHeader_fromName
            // 
            this.columnHeader_fromName.Text = "来源名";
            this.columnHeader_fromName.Width = 120;
            // 
            // columnHeader_weight
            // 
            this.columnHeader_weight.Text = "权值";
            this.columnHeader_weight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_weight.Width = 70;
            // 
            // columnHeader_searchStyle
            // 
            this.columnHeader_searchStyle.Text = "检索方式";
            this.columnHeader_searchStyle.Width = 100;
            // 
            // button_deleteAccessPoint
            // 
            this.button_deleteAccessPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_deleteAccessPoint.Enabled = false;
            this.button_deleteAccessPoint.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_deleteAccessPoint.Location = new System.Drawing.Point(321, 97);
            this.button_deleteAccessPoint.Name = "button_deleteAccessPoint";
            this.button_deleteAccessPoint.Size = new System.Drawing.Size(75, 28);
            this.button_deleteAccessPoint.TabIndex = 4;
            this.button_deleteAccessPoint.Text = "删除(&D)";
            this.button_deleteAccessPoint.UseVisualStyleBackColor = true;
            this.button_deleteAccessPoint.Click += new System.EventHandler(this.button_deleteAccessPoint_Click);
            // 
            // button_modifyAccessPoint
            // 
            this.button_modifyAccessPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_modifyAccessPoint.Enabled = false;
            this.button_modifyAccessPoint.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_modifyAccessPoint.Location = new System.Drawing.Point(321, 49);
            this.button_modifyAccessPoint.Name = "button_modifyAccessPoint";
            this.button_modifyAccessPoint.Size = new System.Drawing.Size(75, 28);
            this.button_modifyAccessPoint.TabIndex = 3;
            this.button_modifyAccessPoint.Text = "修改(&M)";
            this.button_modifyAccessPoint.UseVisualStyleBackColor = true;
            this.button_modifyAccessPoint.Click += new System.EventHandler(this.button_modifyAccessPoint_Click);
            // 
            // button_newAccessPoint
            // 
            this.button_newAccessPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_newAccessPoint.Enabled = false;
            this.button_newAccessPoint.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_newAccessPoint.Location = new System.Drawing.Point(321, 19);
            this.button_newAccessPoint.Name = "button_newAccessPoint";
            this.button_newAccessPoint.Size = new System.Drawing.Size(75, 28);
            this.button_newAccessPoint.TabIndex = 2;
            this.button_newAccessPoint.Text = "新增(&N)";
            this.button_newAccessPoint.UseVisualStyleBackColor = true;
            this.button_newAccessPoint.Click += new System.EventHandler(this.button_newAccessPoint_Click);
            // 
            // button_deleteDatabase
            // 
            this.button_deleteDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_deleteDatabase.Enabled = false;
            this.button_deleteDatabase.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_deleteDatabase.Location = new System.Drawing.Point(321, 78);
            this.button_deleteDatabase.Name = "button_deleteDatabase";
            this.button_deleteDatabase.Size = new System.Drawing.Size(75, 28);
            this.button_deleteDatabase.TabIndex = 3;
            this.button_deleteDatabase.Text = "删除(&D)";
            this.button_deleteDatabase.UseVisualStyleBackColor = true;
            this.button_deleteDatabase.Click += new System.EventHandler(this.button_deleteDatabase_Click);
            // 
            // button_modifyDatabase
            // 
            this.button_modifyDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_modifyDatabase.Enabled = false;
            this.button_modifyDatabase.Font = new System.Drawing.Font("SimSun", 9F);
            this.button_modifyDatabase.Location = new System.Drawing.Point(321, 30);
            this.button_modifyDatabase.Name = "button_modifyDatabase";
            this.button_modifyDatabase.Size = new System.Drawing.Size(75, 28);
            this.button_modifyDatabase.TabIndex = 2;
            this.button_modifyDatabase.Text = "修改(&M)";
            this.button_modifyDatabase.UseVisualStyleBackColor = true;
            this.button_modifyDatabase.Click += new System.EventHandler(this.button_modifyDatabase_Click);
            // 
            // button_newDatabase
            // 
            this.button_newDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_newDatabase.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_newDatabase.Location = new System.Drawing.Point(321, 0);
            this.button_newDatabase.Name = "button_newDatabase";
            this.button_newDatabase.Size = new System.Drawing.Size(75, 28);
            this.button_newDatabase.TabIndex = 1;
            this.button_newDatabase.Text = "新增(&N)";
            this.button_newDatabase.UseVisualStyleBackColor = true;
            this.button_newDatabase.Click += new System.EventHandler(this.button_newDatabase_Click);
            // 
            // panel_accessPoints
            // 
            this.panel_accessPoints.Controls.Add(this.button_deleteAccessPoint);
            this.panel_accessPoints.Controls.Add(this.label3);
            this.panel_accessPoints.Controls.Add(this.listView_accessPoints);
            this.panel_accessPoints.Controls.Add(this.button_newAccessPoint);
            this.panel_accessPoints.Controls.Add(this.button_modifyAccessPoint);
            this.panel_accessPoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_accessPoints.Location = new System.Drawing.Point(0, 0);
            this.panel_accessPoints.Name = "panel_accessPoints";
            this.panel_accessPoints.Size = new System.Drawing.Size(396, 190);
            this.panel_accessPoints.TabIndex = 0;
            // 
            // panel_databases
            // 
            this.panel_databases.Controls.Add(this.listView_databases);
            this.panel_databases.Controls.Add(this.button_newDatabase);
            this.panel_databases.Controls.Add(this.button_deleteDatabase);
            this.panel_databases.Controls.Add(this.button_modifyDatabase);
            this.panel_databases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_databases.Location = new System.Drawing.Point(0, 0);
            this.panel_databases.Name = "panel_databases";
            this.panel_databases.Size = new System.Drawing.Size(396, 133);
            this.panel_databases.TabIndex = 0;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(13, 123);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_databases);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.panel_accessPoints);
            this.splitContainer_main.Size = new System.Drawing.Size(396, 339);
            this.splitContainer_main.SplitterDistance = 133;
            this.splitContainer_main.SplitterWidth = 16;
            this.splitContainer_main.TabIndex = 16;
            // 
            // ProjectDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 474);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_projectName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ProjectDialog";
            this.ShowInTaskbar = false;
            this.Text = "查重方案";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProjectDialog_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProjectDialog_FormClosing);
            this.Load += new System.EventHandler(this.ProjectDialog_Load);
            this.panel_accessPoints.ResumeLayout(false);
            this.panel_accessPoints.PerformLayout();
            this.panel_databases.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_projectName;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ListView listView_databases;
        private System.Windows.Forms.ColumnHeader columnHeader_targetDatabase;
        private System.Windows.Forms.ColumnHeader columnHeader_threshold;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView listView_accessPoints;
        private System.Windows.Forms.ColumnHeader columnHeader_fromName;
        private System.Windows.Forms.ColumnHeader columnHeader_weight;
        private System.Windows.Forms.ColumnHeader columnHeader_searchStyle;
        private System.Windows.Forms.Button button_deleteAccessPoint;
        private System.Windows.Forms.Button button_modifyAccessPoint;
        private System.Windows.Forms.Button button_newAccessPoint;
        private System.Windows.Forms.Button button_deleteDatabase;
        private System.Windows.Forms.Button button_modifyDatabase;
        private System.Windows.Forms.Button button_newDatabase;
        private System.Windows.Forms.Panel panel_accessPoints;
        private System.Windows.Forms.Panel panel_databases;
        private System.Windows.Forms.SplitContainer splitContainer_main;
    }
}