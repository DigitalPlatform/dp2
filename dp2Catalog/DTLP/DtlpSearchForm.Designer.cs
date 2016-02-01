namespace dp2Catalog
{
    partial class DtlpSearchForm
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

            this.EventLoadFinish.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DtlpSearchForm));
            this.dtlpResDirControl1 = new DigitalPlatform.DTLP.DtlpResDirControl();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.splitContainer_up = new System.Windows.Forms.SplitContainer();
            this.panel_target = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_resPath = new System.Windows.Forms.TextBox();
            this.splitContainer_queryAndResultInfo = new System.Windows.Forms.SplitContainer();
            this.panel_query = new System.Windows.Forms.Panel();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_resultInfo = new System.Windows.Forms.TextBox();
            this.listView_browse = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_5 = new System.Windows.Forms.ColumnHeader();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.splitContainer_up.Panel1.SuspendLayout();
            this.splitContainer_up.Panel2.SuspendLayout();
            this.splitContainer_up.SuspendLayout();
            this.panel_target.SuspendLayout();
            this.splitContainer_queryAndResultInfo.Panel1.SuspendLayout();
            this.splitContainer_queryAndResultInfo.Panel2.SuspendLayout();
            this.splitContainer_queryAndResultInfo.SuspendLayout();
            this.panel_query.SuspendLayout();
            this.SuspendLayout();
            // 
            // dtlpResDirControl1
            // 
            this.dtlpResDirControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dtlpResDirControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dtlpResDirControl1.HideSelection = false;
            this.dtlpResDirControl1.ImageIndex = 0;
            this.dtlpResDirControl1.Location = new System.Drawing.Point(3, 3);
            this.dtlpResDirControl1.Name = "dtlpResDirControl1";
            this.dtlpResDirControl1.SelectedImageIndex = 0;
            this.dtlpResDirControl1.SelectedPath1 = null;
            this.dtlpResDirControl1.Size = new System.Drawing.Size(319, 145);
            this.dtlpResDirControl1.TabIndex = 0;
            this.dtlpResDirControl1.ItemSelected += new DigitalPlatform.DTLP.ItemSelectedEventHandle(this.dtlpResDirControl1_ItemSelected);
            this.dtlpResDirControl1.GetItemTextStyle += new DigitalPlatform.DTLP.GetItemTextStyleEventHandle(this.dtlpResDirControl1_GetItemTextStyle);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.splitContainer_up);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView_browse);
            this.splitContainer_main.Size = new System.Drawing.Size(631, 328);
            this.splitContainer_main.SplitterDistance = 175;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 1;
            // 
            // splitContainer_up
            // 
            this.splitContainer_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_up.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_up.Name = "splitContainer_up";
            // 
            // splitContainer_up.Panel1
            // 
            this.splitContainer_up.Panel1.Controls.Add(this.panel_target);
            // 
            // splitContainer_up.Panel2
            // 
            this.splitContainer_up.Panel2.Controls.Add(this.splitContainer_queryAndResultInfo);
            this.splitContainer_up.Size = new System.Drawing.Size(631, 175);
            this.splitContainer_up.SplitterDistance = 325;
            this.splitContainer_up.TabIndex = 0;
            // 
            // panel_target
            // 
            this.panel_target.Controls.Add(this.label2);
            this.panel_target.Controls.Add(this.textBox_resPath);
            this.panel_target.Controls.Add(this.dtlpResDirControl1);
            this.panel_target.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_target.Location = new System.Drawing.Point(0, 0);
            this.panel_target.Name = "panel_target";
            this.panel_target.Size = new System.Drawing.Size(325, 175);
            this.panel_target.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 154);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Ä¿±êÂ·¾¶(&P):";
            // 
            // textBox_resPath
            // 
            this.textBox_resPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_resPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_resPath.Location = new System.Drawing.Point(105, 154);
            this.textBox_resPath.Name = "textBox_resPath";
            this.textBox_resPath.ReadOnly = true;
            this.textBox_resPath.Size = new System.Drawing.Size(217, 18);
            this.textBox_resPath.TabIndex = 1;
            // 
            // splitContainer_queryAndResultInfo
            // 
            this.splitContainer_queryAndResultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_queryAndResultInfo.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_queryAndResultInfo.Name = "splitContainer_queryAndResultInfo";
            this.splitContainer_queryAndResultInfo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_queryAndResultInfo.Panel1
            // 
            this.splitContainer_queryAndResultInfo.Panel1.Controls.Add(this.panel_query);
            // 
            // splitContainer_queryAndResultInfo.Panel2
            // 
            this.splitContainer_queryAndResultInfo.Panel2.Controls.Add(this.textBox_resultInfo);
            this.splitContainer_queryAndResultInfo.Size = new System.Drawing.Size(302, 175);
            this.splitContainer_queryAndResultInfo.SplitterDistance = 43;
            this.splitContainer_queryAndResultInfo.TabIndex = 0;
            // 
            // panel_query
            // 
            this.panel_query.Controls.Add(this.textBox_queryWord);
            this.panel_query.Controls.Add(this.label1);
            this.panel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_query.Location = new System.Drawing.Point(0, 0);
            this.panel_query.Name = "panel_query";
            this.panel_query.Size = new System.Drawing.Size(302, 43);
            this.panel_query.TabIndex = 0;
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_queryWord.Location = new System.Drawing.Point(94, 1);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(205, 25);
            this.textBox_queryWord.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "¼ìË÷´Ê(&Q):";
            // 
            // textBox_resultInfo
            // 
            this.textBox_resultInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_resultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_resultInfo.Location = new System.Drawing.Point(0, 0);
            this.textBox_resultInfo.Multiline = true;
            this.textBox_resultInfo.Name = "textBox_resultInfo";
            this.textBox_resultInfo.ReadOnly = true;
            this.textBox_resultInfo.Size = new System.Drawing.Size(302, 128);
            this.textBox_resultInfo.TabIndex = 1;
            // 
            // listView_browse
            // 
            this.listView_browse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1,
            this.columnHeader_2,
            this.columnHeader_3,
            this.columnHeader_4,
            this.columnHeader_5});
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.Location = new System.Drawing.Point(0, 0);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(631, 145);
            this.listView_browse.TabIndex = 1;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_browse_ColumnClick);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "¼ÇÂ¼Â·¾¶";
            this.columnHeader_path.Width = 250;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 200;
            // 
            // columnHeader_2
            // 
            this.columnHeader_2.Text = "2";
            this.columnHeader_2.Width = 100;
            // 
            // columnHeader_3
            // 
            this.columnHeader_3.Text = "3";
            this.columnHeader_3.Width = 50;
            // 
            // columnHeader_4
            // 
            this.columnHeader_4.Text = "4";
            this.columnHeader_4.Width = 150;
            // 
            // columnHeader_5
            // 
            this.columnHeader_5.Text = "5";
            this.columnHeader_5.Width = 100;
            // 
            // DtlpSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 328);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DtlpSearchForm";
            this.ShowInTaskbar = false;
            this.Text = "DTLP¼ìË÷´°";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DtlpSearchForm_FormClosed);
            this.Activated += new System.EventHandler(this.DtlpSearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DtlpSearchForm_FormClosing);
            this.Load += new System.EventHandler(this.DtlpSearchForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_up.Panel1.ResumeLayout(false);
            this.splitContainer_up.Panel2.ResumeLayout(false);
            this.splitContainer_up.ResumeLayout(false);
            this.panel_target.ResumeLayout(false);
            this.panel_target.PerformLayout();
            this.splitContainer_queryAndResultInfo.Panel1.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.Panel2.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.Panel2.PerformLayout();
            this.splitContainer_queryAndResultInfo.ResumeLayout(false);
            this.panel_query.ResumeLayout(false);
            this.panel_query.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.DTLP.DtlpResDirControl dtlpResDirControl1;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.SplitContainer splitContainer_up;
        private System.Windows.Forms.Panel panel_target;
        private System.Windows.Forms.TextBox textBox_resPath;
        private System.Windows.Forms.SplitContainer splitContainer_queryAndResultInfo;
        private System.Windows.Forms.Panel panel_query;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_resultInfo;
        private DigitalPlatform.GUI.ListViewNF listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.ColumnHeader columnHeader_2;
        private System.Windows.Forms.ColumnHeader columnHeader_3;
        private System.Windows.Forms.ColumnHeader columnHeader_4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader_5;
    }
}