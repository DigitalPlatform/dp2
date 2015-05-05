namespace dp2Circulation
{
    partial class QuickChangeBiblioForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickChangeBiblioForm));
            this.tabControl_input = new System.Windows.Forms.TabControl();
            this.tabPage_paths = new System.Windows.Forms.TabPage();
            this.textBox_paths = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_recpathFile = new System.Windows.Forms.TabPage();
            this.button_file_getRecpathFilename = new System.Windows.Forms.Button();
            this.textBox_recpathFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_begin = new System.Windows.Forms.Button();
            this.button_changeParam = new System.Windows.Forms.Button();
            this.tabControl_input.SuspendLayout();
            this.tabPage_paths.SuspendLayout();
            this.tabPage_recpathFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_input
            // 
            this.tabControl_input.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_input.Controls.Add(this.tabPage_paths);
            this.tabControl_input.Controls.Add(this.tabPage_recpathFile);
            this.tabControl_input.Location = new System.Drawing.Point(1, 10);
            this.tabControl_input.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl_input.Name = "tabControl_input";
            this.tabControl_input.SelectedIndex = 0;
            this.tabControl_input.Size = new System.Drawing.Size(358, 180);
            this.tabControl_input.TabIndex = 0;
            // 
            // tabPage_paths
            // 
            this.tabPage_paths.Controls.Add(this.textBox_paths);
            this.tabPage_paths.Controls.Add(this.label1);
            this.tabPage_paths.Location = new System.Drawing.Point(4, 22);
            this.tabPage_paths.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_paths.Name = "tabPage_paths";
            this.tabPage_paths.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_paths.Size = new System.Drawing.Size(350, 154);
            this.tabPage_paths.TabIndex = 0;
            this.tabPage_paths.Text = "记录路径";
            this.tabPage_paths.UseVisualStyleBackColor = true;
            // 
            // textBox_paths
            // 
            this.textBox_paths.AcceptsReturn = true;
            this.textBox_paths.AcceptsTab = true;
            this.textBox_paths.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_paths.HideSelection = false;
            this.textBox_paths.Location = new System.Drawing.Point(4, 22);
            this.textBox_paths.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_paths.MaxLength = 0;
            this.textBox_paths.Multiline = true;
            this.textBox_paths.Name = "textBox_paths";
            this.textBox_paths.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_paths.Size = new System.Drawing.Size(342, 132);
            this.textBox_paths.TabIndex = 1;
            this.textBox_paths.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "记录路径[每行一个](&P):";
            // 
            // tabPage_recpathFile
            // 
            this.tabPage_recpathFile.Controls.Add(this.button_file_getRecpathFilename);
            this.tabPage_recpathFile.Controls.Add(this.textBox_recpathFile);
            this.tabPage_recpathFile.Controls.Add(this.label2);
            this.tabPage_recpathFile.Location = new System.Drawing.Point(4, 22);
            this.tabPage_recpathFile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_recpathFile.Name = "tabPage_recpathFile";
            this.tabPage_recpathFile.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_recpathFile.Size = new System.Drawing.Size(350, 154);
            this.tabPage_recpathFile.TabIndex = 1;
            this.tabPage_recpathFile.Text = "文件";
            this.tabPage_recpathFile.UseVisualStyleBackColor = true;
            // 
            // button_file_getRecpathFilename
            // 
            this.button_file_getRecpathFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_file_getRecpathFilename.Location = new System.Drawing.Point(310, 22);
            this.button_file_getRecpathFilename.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_file_getRecpathFilename.Name = "button_file_getRecpathFilename";
            this.button_file_getRecpathFilename.Size = new System.Drawing.Size(38, 22);
            this.button_file_getRecpathFilename.TabIndex = 2;
            this.button_file_getRecpathFilename.Text = "...";
            this.button_file_getRecpathFilename.UseVisualStyleBackColor = true;
            this.button_file_getRecpathFilename.Click += new System.EventHandler(this.button_file_getRecpathFilename_Click);
            // 
            // textBox_recpathFile
            // 
            this.textBox_recpathFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recpathFile.Location = new System.Drawing.Point(4, 22);
            this.textBox_recpathFile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_recpathFile.Name = "textBox_recpathFile";
            this.textBox_recpathFile.Size = new System.Drawing.Size(302, 21);
            this.textBox_recpathFile.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "记录路径文件名(&F):";
            // 
            // button_begin
            // 
            this.button_begin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_begin.Image = ((System.Drawing.Image)(resources.GetObject("button_begin.Image")));
            this.button_begin.Location = new System.Drawing.Point(232, 194);
            this.button_begin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_begin.Name = "button_begin";
            this.button_begin.Size = new System.Drawing.Size(127, 22);
            this.button_begin.TabIndex = 2;
            this.button_begin.Text = "启动修改(&B)";
            this.button_begin.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_begin.UseVisualStyleBackColor = true;
            this.button_begin.Click += new System.EventHandler(this.button_begin_Click);
            // 
            // button_changeParam
            // 
            this.button_changeParam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_changeParam.Image = ((System.Drawing.Image)(resources.GetObject("button_changeParam.Image")));
            this.button_changeParam.Location = new System.Drawing.Point(1, 194);
            this.button_changeParam.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_changeParam.Name = "button_changeParam";
            this.button_changeParam.Size = new System.Drawing.Size(105, 22);
            this.button_changeParam.TabIndex = 1;
            this.button_changeParam.Text = "动作参数(&P)...";
            this.button_changeParam.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_changeParam.UseVisualStyleBackColor = true;
            this.button_changeParam.Click += new System.EventHandler(this.button_changeParam_Click);
            // 
            // QuickChangeBiblioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 226);
            this.Controls.Add(this.button_changeParam);
            this.Controls.Add(this.tabControl_input);
            this.Controls.Add(this.button_begin);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "QuickChangeBiblioForm";
            this.Text = "批修改书目";
            this.Activated += new System.EventHandler(this.QuickChangeBiblioForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.QuickChangeBiblioForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.QuickChangeBiblioForm_FormClosed);
            this.Load += new System.EventHandler(this.QuickChangeBiblioForm_Load);
            this.tabControl_input.ResumeLayout(false);
            this.tabPage_paths.ResumeLayout(false);
            this.tabPage_paths.PerformLayout();
            this.tabPage_recpathFile.ResumeLayout(false);
            this.tabPage_recpathFile.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_input;
        private System.Windows.Forms.TabPage tabPage_paths;
        private System.Windows.Forms.TextBox textBox_paths;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_recpathFile;
        private System.Windows.Forms.Button button_file_getRecpathFilename;
        private System.Windows.Forms.TextBox textBox_recpathFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.Button button_changeParam;
    }
}