namespace dp2Circulation
{
    partial class ReaderManageForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Dispose();

            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderManageForm));
            this.button_load = new System.Windows.Forms.Button();
            this.textBox_readerBarcode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_operation = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.panel_operation = new System.Windows.Forms.Panel();
            this.textBox_operator = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_save = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_readerInfo = new System.Windows.Forms.TabControl();
            this.tabPage_normalInfo = new System.Windows.Forms.TabPage();
            this.webBrowser_normalInfo = new System.Windows.Forms.WebBrowser();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.webBrowser_xml = new System.Windows.Forms.WebBrowser();
            this.panel_operation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_readerInfo.SuspendLayout();
            this.tabPage_normalInfo.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_load
            // 
            this.button_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_load.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_load.Location = new System.Drawing.Point(356, 0);
            this.button_load.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(104, 38);
            this.button_load.TabIndex = 2;
            this.button_load.Text = "装载(&L)";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // textBox_readerBarcode
            // 
            this.textBox_readerBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_readerBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerBarcode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_readerBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_readerBarcode.Location = new System.Drawing.Point(156, 0);
            this.textBox_readerBarcode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_readerBarcode.Name = "textBox_readerBarcode";
            this.textBox_readerBarcode.Size = new System.Drawing.Size(192, 35);
            this.textBox_readerBarcode.TabIndex = 1;
            this.textBox_readerBarcode.Enter += new System.EventHandler(this.textBox_readerBarcode_Enter);
            this.textBox_readerBarcode.Leave += new System.EventHandler(this.textBox_readerBarcode_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-4, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "证条码号(&B):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-4, 47);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "操作(&O):";
            // 
            // comboBox_operation
            // 
            this.comboBox_operation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_operation.DropDownHeight = 300;
            this.comboBox_operation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_operation.FormattingEnabled = true;
            this.comboBox_operation.IntegralHeight = false;
            this.comboBox_operation.Location = new System.Drawing.Point(156, 44);
            this.comboBox_operation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox_operation.Name = "comboBox_operation";
            this.comboBox_operation.Size = new System.Drawing.Size(303, 29);
            this.comboBox_operation.TabIndex = 4;
            this.comboBox_operation.DropDown += new System.EventHandler(this.comboBox_operation_DropDown);
            this.comboBox_operation.SizeChanged += new System.EventHandler(this.comboBox_operation_SizeChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-4, 88);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 21);
            this.label3.TabIndex = 5;
            this.label3.Text = "注释(&C):";
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_comment.Location = new System.Drawing.Point(156, 84);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(303, 333);
            this.textBox_comment.TabIndex = 6;
            this.textBox_comment.Enter += new System.EventHandler(this.textBox_comment_Enter);
            // 
            // panel_operation
            // 
            this.panel_operation.BackColor = System.Drawing.Color.Lavender;
            this.panel_operation.Controls.Add(this.textBox_operator);
            this.panel_operation.Controls.Add(this.label4);
            this.panel_operation.Controls.Add(this.button_save);
            this.panel_operation.Controls.Add(this.label1);
            this.panel_operation.Controls.Add(this.textBox_comment);
            this.panel_operation.Controls.Add(this.textBox_readerBarcode);
            this.panel_operation.Controls.Add(this.label3);
            this.panel_operation.Controls.Add(this.button_load);
            this.panel_operation.Controls.Add(this.comboBox_operation);
            this.panel_operation.Controls.Add(this.label2);
            this.panel_operation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_operation.Location = new System.Drawing.Point(0, 0);
            this.panel_operation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel_operation.Name = "panel_operation";
            this.panel_operation.Size = new System.Drawing.Size(460, 508);
            this.panel_operation.TabIndex = 15;
            // 
            // textBox_operator
            // 
            this.textBox_operator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_operator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_operator.Location = new System.Drawing.Point(156, 425);
            this.textBox_operator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox_operator.Name = "textBox_operator";
            this.textBox_operator.ReadOnly = true;
            this.textBox_operator.Size = new System.Drawing.Size(303, 31);
            this.textBox_operator.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-2, 429);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "操作员(&R):";
            // 
            // button_save
            // 
            this.button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_save.Location = new System.Drawing.Point(156, 467);
            this.button_save.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(139, 38);
            this.button_save.TabIndex = 9;
            this.button_save.Text = "保存(&S)";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 18);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_operation);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tabControl_readerInfo);
            this.splitContainer_main.Size = new System.Drawing.Size(922, 508);
            this.splitContainer_main.SplitterDistance = 460;
            this.splitContainer_main.SplitterWidth = 11;
            this.splitContainer_main.TabIndex = 16;
            // 
            // tabControl_readerInfo
            // 
            this.tabControl_readerInfo.Controls.Add(this.tabPage_normalInfo);
            this.tabControl_readerInfo.Controls.Add(this.tabPage_xml);
            this.tabControl_readerInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_readerInfo.Location = new System.Drawing.Point(0, 0);
            this.tabControl_readerInfo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl_readerInfo.Name = "tabControl_readerInfo";
            this.tabControl_readerInfo.SelectedIndex = 0;
            this.tabControl_readerInfo.Size = new System.Drawing.Size(451, 508);
            this.tabControl_readerInfo.TabIndex = 0;
            // 
            // tabPage_normalInfo
            // 
            this.tabPage_normalInfo.Controls.Add(this.webBrowser_normalInfo);
            this.tabPage_normalInfo.Location = new System.Drawing.Point(4, 31);
            this.tabPage_normalInfo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_normalInfo.Name = "tabPage_normalInfo";
            this.tabPage_normalInfo.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_normalInfo.Size = new System.Drawing.Size(443, 473);
            this.tabPage_normalInfo.TabIndex = 0;
            this.tabPage_normalInfo.Text = "基本信息";
            this.tabPage_normalInfo.UseVisualStyleBackColor = true;
            // 
            // webBrowser_normalInfo
            // 
            this.webBrowser_normalInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_normalInfo.Location = new System.Drawing.Point(4, 4);
            this.webBrowser_normalInfo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.webBrowser_normalInfo.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser_normalInfo.Name = "webBrowser_normalInfo";
            this.webBrowser_normalInfo.Size = new System.Drawing.Size(435, 465);
            this.webBrowser_normalInfo.TabIndex = 0;
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.Controls.Add(this.webBrowser_xml);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 31);
            this.tabPage_xml.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage_xml.Size = new System.Drawing.Size(443, 473);
            this.tabPage_xml.TabIndex = 1;
            this.tabPage_xml.Text = "XML";
            this.tabPage_xml.UseVisualStyleBackColor = true;
            // 
            // webBrowser_xml
            // 
            this.webBrowser_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_xml.Location = new System.Drawing.Point(4, 4);
            this.webBrowser_xml.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.webBrowser_xml.MinimumSize = new System.Drawing.Size(28, 28);
            this.webBrowser_xml.Name = "webBrowser_xml";
            this.webBrowser_xml.Size = new System.Drawing.Size(435, 465);
            this.webBrowser_xml.TabIndex = 0;
            // 
            // ReaderManageForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 542);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ReaderManageForm";
            this.ShowInTaskbar = false;
            this.Text = "停借窗";
            this.Activated += new System.EventHandler(this.ReaderManageForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReaderManageForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReaderManageForm_FormClosed);
            this.Load += new System.EventHandler(this.ReaderManageForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ReaderManageForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ReaderManageForm_DragEnter);
            this.panel_operation.ResumeLayout(false);
            this.panel_operation.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_readerInfo.ResumeLayout(false);
            this.tabPage_normalInfo.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.TextBox textBox_readerBarcode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_operation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Panel panel_operation;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.TabControl tabControl_readerInfo;
        private System.Windows.Forms.TabPage tabPage_normalInfo;
        private System.Windows.Forms.TabPage tabPage_xml;
        private System.Windows.Forms.WebBrowser webBrowser_normalInfo;
        private System.Windows.Forms.WebBrowser webBrowser_xml;
        private System.Windows.Forms.TextBox textBox_operator;
        private System.Windows.Forms.Label label4;
    }
}