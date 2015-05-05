namespace dp2Circulation
{
    partial class CommentControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommentControl));
            this.listView = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_orderSuggestion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_author = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_subject = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_summary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_content = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_createTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_lastModified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_refID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_operations = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_recpath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_pie = new System.Windows.Forms.Panel();
            this.comboBox_libraryCodeFilter = new System.Windows.Forms.ComboBox();
            this.pieChartControl1 = new System.Drawing.PieChart.PieChartControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.panel_pie.SuspendLayout();
            this.SuspendLayout();
            // 
            // ListView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_errorInfo,
            this.columnHeader_state,
            this.columnHeader_type,
            this.columnHeader_orderSuggestion,
            this.columnHeader_title,
            this.columnHeader_author,
            this.columnHeader_libraryCode,
            this.columnHeader_subject,
            this.columnHeader_summary,
            this.columnHeader_content,
            this.columnHeader_createTime,
            this.columnHeader_lastModified,
            this.columnHeader_refID,
            this.columnHeader_operations,
            this.columnHeader_recpath});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.HideSelection = false;
            this.listView.LargeImageList = this.imageList_itemType;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Margin = new System.Windows.Forms.Padding(2);
            this.listView.Name = "ListView";
            this.listView.Size = new System.Drawing.Size(251, 138);
            this.listView.SmallImageList = this.imageList_itemType;
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.ListView_SelectedIndexChanged);
            this.listView.DoubleClick += new System.EventHandler(this.ListView_DoubleClick);
            this.listView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ListView_MouseUp);
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "编号";
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "错误信息";
            this.columnHeader_errorInfo.Width = 120;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            // 
            // columnHeader_type
            // 
            this.columnHeader_type.Text = "类型";
            this.columnHeader_type.Width = 100;
            // 
            // columnHeader_orderSuggestion
            // 
            this.columnHeader_orderSuggestion.Text = "订购建议";
            this.columnHeader_orderSuggestion.Width = 80;
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "标题";
            this.columnHeader_title.Width = 120;
            // 
            // columnHeader_author
            // 
            this.columnHeader_author.Text = "作者";
            this.columnHeader_author.Width = 120;
            // 
            // columnHeader_libraryCode
            // 
            this.columnHeader_libraryCode.Text = "馆代码";
            this.columnHeader_libraryCode.Width = 100;
            // 
            // columnHeader_subject
            // 
            this.columnHeader_subject.Text = "主题";
            this.columnHeader_subject.Width = 120;
            // 
            // columnHeader_summary
            // 
            this.columnHeader_summary.Text = "摘要";
            this.columnHeader_summary.Width = 120;
            // 
            // columnHeader_content
            // 
            this.columnHeader_content.Text = "内容";
            this.columnHeader_content.Width = 120;
            // 
            // columnHeader_createTime
            // 
            this.columnHeader_createTime.Text = "创建时间";
            this.columnHeader_createTime.Width = 80;
            // 
            // columnHeader_lastModified
            // 
            this.columnHeader_lastModified.Text = "最后修改时间";
            this.columnHeader_lastModified.Width = 66;
            // 
            // columnHeader_refID
            // 
            this.columnHeader_refID.Text = "参考ID";
            this.columnHeader_refID.Width = 100;
            // 
            // columnHeader_operations
            // 
            this.columnHeader_operations.Text = "操作";
            this.columnHeader_operations.Width = 150;
            // 
            // columnHeader_recpath
            // 
            this.columnHeader_recpath.Text = "记录路径";
            this.columnHeader_recpath.Width = 120;
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList_itemType.Images.SetKeyName(0, "normal_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(1, "new_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(2, "changed_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(3, "deleted_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(4, "error_entity.bmp");
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_pie);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView);
            this.splitContainer_main.Size = new System.Drawing.Size(382, 138);
            this.splitContainer_main.SplitterDistance = 127;
            this.splitContainer_main.TabIndex = 3;
            // 
            // panel_pie
            // 
            this.panel_pie.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel_pie.Controls.Add(this.comboBox_libraryCodeFilter);
            this.panel_pie.Controls.Add(this.pieChartControl1);
            this.panel_pie.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_pie.Location = new System.Drawing.Point(0, 0);
            this.panel_pie.Name = "panel_pie";
            this.panel_pie.Size = new System.Drawing.Size(127, 138);
            this.panel_pie.TabIndex = 1;
            // 
            // comboBox_libraryCodeFilter
            // 
            this.comboBox_libraryCodeFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_libraryCodeFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBox_libraryCodeFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_libraryCodeFilter.ForeColor = System.Drawing.Color.White;
            this.comboBox_libraryCodeFilter.FormattingEnabled = true;
            this.comboBox_libraryCodeFilter.Location = new System.Drawing.Point(3, 3);
            this.comboBox_libraryCodeFilter.Name = "comboBox_libraryCodeFilter";
            this.comboBox_libraryCodeFilter.Size = new System.Drawing.Size(121, 20);
            this.comboBox_libraryCodeFilter.TabIndex = 1;
            this.comboBox_libraryCodeFilter.SelectedIndexChanged += new System.EventHandler(this.comboBox_libraryCodeFilter_SelectedIndexChanged);
            this.comboBox_libraryCodeFilter.SizeChanged += new System.EventHandler(this.comboBox_libraryCodeFilter_SizeChanged);
            // 
            // pieChartControl1
            // 
            this.pieChartControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pieChartControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pieChartControl1.ForeColor = System.Drawing.Color.Black;
            this.pieChartControl1.Location = new System.Drawing.Point(3, 29);
            this.pieChartControl1.Name = "pieChartControl1";
            this.pieChartControl1.Size = new System.Drawing.Size(121, 106);
            this.pieChartControl1.TabIndex = 0;
            this.pieChartControl1.ToolTips = null;
            this.pieChartControl1.SizeChanged += new System.EventHandler(this.pieChartControl1_SizeChanged);
            // 
            // CommentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer_main);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CommentControl";
            this.Size = new System.Drawing.Size(382, 138);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.panel_pie.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_author;
        private System.Windows.Forms.ColumnHeader columnHeader_subject;
        private System.Windows.Forms.ColumnHeader columnHeader_content;
        private System.Windows.Forms.ColumnHeader columnHeader_createTime;
        private System.Windows.Forms.ColumnHeader columnHeader_lastModified;
        private System.Windows.Forms.ColumnHeader columnHeader_refID;
        private System.Windows.Forms.ColumnHeader columnHeader_operations;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ColumnHeader columnHeader_summary;
        private System.Windows.Forms.ColumnHeader columnHeader_type;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.ColumnHeader columnHeader_orderSuggestion;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryCode;
        private System.Drawing.PieChart.PieChartControl pieChartControl1;
        private System.Windows.Forms.Panel panel_pie;
        private System.Windows.Forms.ComboBox comboBox_libraryCodeFilter;
    }
}
