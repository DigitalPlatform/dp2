namespace dp2Circulation
{
    partial class AddSubjectDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddSubjectDialog));
            this.textBox_exist = new System.Windows.Forms.TextBox();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.splitContainer_exist = new System.Windows.Forms.SplitContainer();
            this.toolStrip_reserve = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.textBox_reserve = new System.Windows.Forms.TextBox();
            this.toolStrip_exist = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_splitExist = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_new = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_splitNew = new System.Windows.Forms.ToolStripButton();
            this.textBox_new = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.toolStripButton_importNewSubjects = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel_new = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_reserve = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_exist = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_exist)).BeginInit();
            this.splitContainer_exist.Panel1.SuspendLayout();
            this.splitContainer_exist.Panel2.SuspendLayout();
            this.splitContainer_exist.SuspendLayout();
            this.toolStrip_reserve.SuspendLayout();
            this.toolStrip_exist.SuspendLayout();
            this.toolStrip_new.SuspendLayout();
            this.tableLayoutPanel_new.SuspendLayout();
            this.tableLayoutPanel_reserve.SuspendLayout();
            this.tableLayoutPanel_exist.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_exist
            // 
            this.textBox_exist.AcceptsReturn = true;
            this.textBox_exist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_exist.Location = new System.Drawing.Point(0, 25);
            this.textBox_exist.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_exist.MaxLength = 0;
            this.textBox_exist.Multiline = true;
            this.textBox_exist.Name = "textBox_exist";
            this.textBox_exist.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_exist.Size = new System.Drawing.Size(210, 81);
            this.textBox_exist.TabIndex = 1;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(12, 12);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.splitContainer_exist);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_new);
            this.splitContainer_main.Size = new System.Drawing.Size(403, 211);
            this.splitContainer_main.SplitterDistance = 106;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 2;
            // 
            // splitContainer_exist
            // 
            this.splitContainer_exist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_exist.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_exist.Name = "splitContainer_exist";
            // 
            // splitContainer_exist.Panel1
            // 
            this.splitContainer_exist.Panel1.Controls.Add(this.tableLayoutPanel_reserve);
            // 
            // splitContainer_exist.Panel2
            // 
            this.splitContainer_exist.Panel2.Controls.Add(this.tableLayoutPanel_exist);
            this.splitContainer_exist.Size = new System.Drawing.Size(403, 106);
            this.splitContainer_exist.SplitterDistance = 185;
            this.splitContainer_exist.SplitterWidth = 8;
            this.splitContainer_exist.TabIndex = 2;
            // 
            // toolStrip_reserve
            // 
            this.toolStrip_reserve.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_reserve.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel3});
            this.toolStrip_reserve.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_reserve.Name = "toolStrip_reserve";
            this.toolStrip_reserve.Size = new System.Drawing.Size(185, 25);
            this.toolStrip_reserve.TabIndex = 0;
            this.toolStrip_reserve.Text = "toolStrip1";
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(95, 22);
            this.toolStripLabel3.Text = "被保留的自由词:";
            // 
            // textBox_reserve
            // 
            this.textBox_reserve.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_reserve.Location = new System.Drawing.Point(0, 25);
            this.textBox_reserve.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_reserve.MaxLength = 0;
            this.textBox_reserve.Multiline = true;
            this.textBox_reserve.Name = "textBox_reserve";
            this.textBox_reserve.ReadOnly = true;
            this.textBox_reserve.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_reserve.Size = new System.Drawing.Size(185, 81);
            this.textBox_reserve.TabIndex = 1;
            // 
            // toolStrip_exist
            // 
            this.toolStrip_exist.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_exist.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripButton_splitExist});
            this.toolStrip_exist.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_exist.Name = "toolStrip_exist";
            this.toolStrip_exist.Size = new System.Drawing.Size(210, 25);
            this.toolStrip_exist.TabIndex = 0;
            this.toolStrip_exist.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(71, 22);
            this.toolStripLabel1.Text = "现有自由词:";
            // 
            // toolStripButton_splitExist
            // 
            this.toolStripButton_splitExist.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_splitExist.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_splitExist.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_splitExist.Image")));
            this.toolStripButton_splitExist.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_splitExist.Name = "toolStripButton_splitExist";
            this.toolStripButton_splitExist.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_splitExist.Text = "拆为每行一个";
            this.toolStripButton_splitExist.Click += new System.EventHandler(this.toolStripButton_splitExist_Click);
            // 
            // toolStrip_new
            // 
            this.toolStrip_new.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_new.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.toolStripButton_splitNew,
            this.toolStripButton_importNewSubjects});
            this.toolStrip_new.Location = new System.Drawing.Point(0, 0);
            this.toolStrip_new.Name = "toolStrip_new";
            this.toolStrip_new.Size = new System.Drawing.Size(403, 25);
            this.toolStrip_new.TabIndex = 0;
            this.toolStrip_new.Text = "toolStrip1";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(95, 22);
            this.toolStripLabel2.Text = "拟新增的自由词:";
            // 
            // toolStripButton_splitNew
            // 
            this.toolStripButton_splitNew.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_splitNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_splitNew.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_splitNew.Image")));
            this.toolStripButton_splitNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_splitNew.Name = "toolStripButton_splitNew";
            this.toolStripButton_splitNew.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_splitNew.Text = "拆为每行一个";
            this.toolStripButton_splitNew.Click += new System.EventHandler(this.toolStripButton_splitNew_Click);
            // 
            // textBox_new
            // 
            this.textBox_new.AcceptsReturn = true;
            this.textBox_new.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_new.Location = new System.Drawing.Point(0, 25);
            this.textBox_new.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_new.MaxLength = 0;
            this.textBox_new.Multiline = true;
            this.textBox_new.Name = "textBox_new";
            this.textBox_new.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_new.Size = new System.Drawing.Size(403, 72);
            this.textBox_new.TabIndex = 1;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(259, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(340, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // toolStripButton_importNewSubjects
            // 
            this.toolStripButton_importNewSubjects.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_importNewSubjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_importNewSubjects.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_importNewSubjects.Image")));
            this.toolStripButton_importNewSubjects.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_importNewSubjects.Name = "toolStripButton_importNewSubjects";
            this.toolStripButton_importNewSubjects.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_importNewSubjects.Text = "导入拟新增的自由词";
            this.toolStripButton_importNewSubjects.Click += new System.EventHandler(this.toolStripButton_importNewSubjects_Click);
            // 
            // tableLayoutPanel_new
            // 
            this.tableLayoutPanel_new.ColumnCount = 1;
            this.tableLayoutPanel_new.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_new.Controls.Add(this.toolStrip_new, 0, 0);
            this.tableLayoutPanel_new.Controls.Add(this.textBox_new, 0, 1);
            this.tableLayoutPanel_new.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_new.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_new.Name = "tableLayoutPanel_new";
            this.tableLayoutPanel_new.RowCount = 2;
            this.tableLayoutPanel_new.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_new.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_new.Size = new System.Drawing.Size(403, 97);
            this.tableLayoutPanel_new.TabIndex = 4;
            // 
            // tableLayoutPanel_reserve
            // 
            this.tableLayoutPanel_reserve.ColumnCount = 1;
            this.tableLayoutPanel_reserve.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_reserve.Controls.Add(this.toolStrip_reserve, 0, 0);
            this.tableLayoutPanel_reserve.Controls.Add(this.textBox_reserve, 0, 1);
            this.tableLayoutPanel_reserve.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_reserve.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_reserve.Name = "tableLayoutPanel_reserve";
            this.tableLayoutPanel_reserve.RowCount = 2;
            this.tableLayoutPanel_reserve.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_reserve.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_reserve.Size = new System.Drawing.Size(185, 106);
            this.tableLayoutPanel_reserve.TabIndex = 5;
            // 
            // tableLayoutPanel_exist
            // 
            this.tableLayoutPanel_exist.ColumnCount = 1;
            this.tableLayoutPanel_exist.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_exist.Controls.Add(this.toolStrip_exist, 0, 0);
            this.tableLayoutPanel_exist.Controls.Add(this.textBox_exist, 0, 1);
            this.tableLayoutPanel_exist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_exist.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_exist.Name = "tableLayoutPanel_exist";
            this.tableLayoutPanel_exist.RowCount = 2;
            this.tableLayoutPanel_exist.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_exist.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_exist.Size = new System.Drawing.Size(210, 106);
            this.tableLayoutPanel_exist.TabIndex = 5;
            // 
            // AddSubjectDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(427, 264);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.splitContainer_main);
            this.Name = "AddSubjectDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "新增自由词";
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_exist.Panel1.ResumeLayout(false);
            this.splitContainer_exist.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_exist)).EndInit();
            this.splitContainer_exist.ResumeLayout(false);
            this.toolStrip_reserve.ResumeLayout(false);
            this.toolStrip_reserve.PerformLayout();
            this.toolStrip_exist.ResumeLayout(false);
            this.toolStrip_exist.PerformLayout();
            this.toolStrip_new.ResumeLayout(false);
            this.toolStrip_new.PerformLayout();
            this.tableLayoutPanel_new.ResumeLayout(false);
            this.tableLayoutPanel_new.PerformLayout();
            this.tableLayoutPanel_reserve.ResumeLayout(false);
            this.tableLayoutPanel_reserve.PerformLayout();
            this.tableLayoutPanel_exist.ResumeLayout(false);
            this.tableLayoutPanel_exist.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_exist;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TextBox textBox_new;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.SplitContainer splitContainer_exist;
        private System.Windows.Forms.TextBox textBox_reserve;
        private System.Windows.Forms.ToolStrip toolStrip_exist;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButton_splitExist;
        private System.Windows.Forms.ToolStrip toolStrip_new;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton toolStripButton_splitNew;
        private System.Windows.Forms.ToolStrip toolStrip_reserve;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripButton toolStripButton_importNewSubjects;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_new;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_reserve;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_exist;
    }
}