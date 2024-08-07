namespace dp2Circulation
{
    partial class ExportMarcHoldingDialog
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
            this.DisposeFreeControls();

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
            this.checkBox_905 = new System.Windows.Forms.CheckBox();
            this.comboBox_905_style = new System.Windows.Forms.ComboBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_removeOld905 = new System.Windows.Forms.CheckBox();
            this.checkBox_906 = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_ext = new System.Windows.Forms.TabPage();
            this.tabPage_biblio = new System.Windows.Forms.TabPage();
            this.button_biblio_findFilterScriptFileName = new System.Windows.Forms.Button();
            this.textBox_biblio_filterScriptFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_biblio_removeFieldNameList = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_items = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage_biblio.SuspendLayout();
            this.tabPage_items.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_905
            // 
            this.checkBox_905.AutoSize = true;
            this.checkBox_905.Location = new System.Drawing.Point(16, 22);
            this.checkBox_905.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_905.Name = "checkBox_905";
            this.checkBox_905.Size = new System.Drawing.Size(302, 25);
            this.checkBox_905.TabIndex = 0;
            this.checkBox_905.Text = "创建 905 字段[根据册记录]";
            this.checkBox_905.UseVisualStyleBackColor = true;
            this.checkBox_905.CheckedChanged += new System.EventHandler(this.checkBox_905_CheckedChanged);
            // 
            // comboBox_905_style
            // 
            this.comboBox_905_style.FormattingEnabled = true;
            this.comboBox_905_style.Items.AddRange(new object[] {
            "只创建单个 905 字段",
            "每册一个 905 字段"});
            this.comboBox_905_style.Location = new System.Drawing.Point(106, 60);
            this.comboBox_905_style.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_905_style.Name = "comboBox_905_style";
            this.comboBox_905_style.Size = new System.Drawing.Size(382, 29);
            this.comboBox_905_style.TabIndex = 1;
            this.comboBox_905_style.Visible = false;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(548, 468);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(403, 468);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_removeOld905
            // 
            this.checkBox_removeOld905.AutoSize = true;
            this.checkBox_removeOld905.Location = new System.Drawing.Point(106, 106);
            this.checkBox_removeOld905.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_removeOld905.Name = "checkBox_removeOld905";
            this.checkBox_removeOld905.Size = new System.Drawing.Size(343, 25);
            this.checkBox_removeOld905.TabIndex = 10;
            this.checkBox_removeOld905.Text = "移除书目记录中原有的 905 字段";
            this.checkBox_removeOld905.UseVisualStyleBackColor = true;
            this.checkBox_removeOld905.Visible = false;
            // 
            // checkBox_906
            // 
            this.checkBox_906.AutoSize = true;
            this.checkBox_906.Location = new System.Drawing.Point(16, 172);
            this.checkBox_906.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_906.Name = "checkBox_906";
            this.checkBox_906.Size = new System.Drawing.Size(302, 25);
            this.checkBox_906.TabIndex = 11;
            this.checkBox_906.Text = "创建 906 字段[根据册记录]";
            this.checkBox_906.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_ext);
            this.tabControl1.Controls.Add(this.tabPage_biblio);
            this.tabControl1.Controls.Add(this.tabPage_items);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(674, 440);
            this.tabControl1.TabIndex = 12;
            // 
            // tabPage_ext
            // 
            this.tabPage_ext.Location = new System.Drawing.Point(4, 31);
            this.tabPage_ext.Name = "tabPage_ext";
            this.tabPage_ext.Size = new System.Drawing.Size(666, 405);
            this.tabPage_ext.TabIndex = 2;
            this.tabPage_ext.Text = "扩展页";
            this.tabPage_ext.UseVisualStyleBackColor = true;
            // 
            // tabPage_biblio
            // 
            this.tabPage_biblio.Controls.Add(this.button_biblio_findFilterScriptFileName);
            this.tabPage_biblio.Controls.Add(this.textBox_biblio_filterScriptFileName);
            this.tabPage_biblio.Controls.Add(this.label2);
            this.tabPage_biblio.Controls.Add(this.textBox_biblio_removeFieldNameList);
            this.tabPage_biblio.Controls.Add(this.label1);
            this.tabPage_biblio.Location = new System.Drawing.Point(4, 31);
            this.tabPage_biblio.Name = "tabPage_biblio";
            this.tabPage_biblio.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_biblio.Size = new System.Drawing.Size(666, 405);
            this.tabPage_biblio.TabIndex = 0;
            this.tabPage_biblio.Text = "书目";
            this.tabPage_biblio.UseVisualStyleBackColor = true;
            // 
            // button_biblio_findFilterScriptFileName
            // 
            this.button_biblio_findFilterScriptFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_biblio_findFilterScriptFileName.Location = new System.Drawing.Point(566, 142);
            this.button_biblio_findFilterScriptFileName.Name = "button_biblio_findFilterScriptFileName";
            this.button_biblio_findFilterScriptFileName.Size = new System.Drawing.Size(81, 31);
            this.button_biblio_findFilterScriptFileName.TabIndex = 5;
            this.button_biblio_findFilterScriptFileName.Text = "...";
            this.button_biblio_findFilterScriptFileName.UseVisualStyleBackColor = true;
            this.button_biblio_findFilterScriptFileName.Click += new System.EventHandler(this.button_biblio_findFilterScriptFileName_Click);
            // 
            // textBox_biblio_filterScriptFileName
            // 
            this.textBox_biblio_filterScriptFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblio_filterScriptFileName.Location = new System.Drawing.Point(22, 142);
            this.textBox_biblio_filterScriptFileName.Name = "textBox_biblio_filterScriptFileName";
            this.textBox_biblio_filterScriptFileName.Size = new System.Drawing.Size(537, 31);
            this.textBox_biblio_filterScriptFileName.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 118);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "过滤用的脚本(&S):";
            // 
            // textBox_biblio_removeFieldNameList
            // 
            this.textBox_biblio_removeFieldNameList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblio_removeFieldNameList.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_biblio_removeFieldNameList.Location = new System.Drawing.Point(22, 59);
            this.textBox_biblio_removeFieldNameList.Name = "textBox_biblio_removeFieldNameList";
            this.textBox_biblio_removeFieldNameList.Size = new System.Drawing.Size(625, 31);
            this.textBox_biblio_removeFieldNameList.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(328, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "自动滤除下列字段[逗号间隔](&F):";
            // 
            // tabPage_items
            // 
            this.tabPage_items.Controls.Add(this.checkBox_removeOld905);
            this.tabPage_items.Controls.Add(this.checkBox_906);
            this.tabPage_items.Controls.Add(this.checkBox_905);
            this.tabPage_items.Controls.Add(this.comboBox_905_style);
            this.tabPage_items.Location = new System.Drawing.Point(4, 31);
            this.tabPage_items.Name = "tabPage_items";
            this.tabPage_items.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_items.Size = new System.Drawing.Size(666, 405);
            this.tabPage_items.TabIndex = 1;
            this.tabPage_items.Text = "905/906 字段";
            this.tabPage_items.UseVisualStyleBackColor = true;
            // 
            // ExportMarcHoldingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(706, 528);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "ExportMarcHoldingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "导出 MARC 文件";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExportMarcHoldingDialog_FormClosed);
            this.Load += new System.EventHandler(this.ExportMarcHoldingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_biblio.ResumeLayout(false);
            this.tabPage_biblio.PerformLayout();
            this.tabPage_items.ResumeLayout(false);
            this.tabPage_items.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_905;
        private System.Windows.Forms.ComboBox comboBox_905_style;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_removeOld905;
        private System.Windows.Forms.CheckBox checkBox_906;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_biblio;
        private System.Windows.Forms.TabPage tabPage_items;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_biblio_removeFieldNameList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage_ext;
        private System.Windows.Forms.TextBox textBox_biblio_filterScriptFileName;
        private System.Windows.Forms.Button button_biblio_findFilterScriptFileName;
    }
}