namespace DigitalPlatform.Marc
{
    partial class PropertyDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PropertyDlg));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_keyinput = new System.Windows.Forms.TabPage();
            this.checkBox_enterAsAutoGenerate = new System.Windows.Forms.CheckBox();
            this.tabPage_lang = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_uiLanguage = new DigitalPlatform.CommonControl.TabComboBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tabControl_main.SuspendLayout();
            this.tabPage_keyinput.SuspendLayout();
            this.tabPage_lang.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_keyinput);
            this.tabControl_main.Controls.Add(this.tabPage_lang);
            this.tabControl_main.Location = new System.Drawing.Point(10, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(361, 223);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_keyinput
            // 
            this.tabPage_keyinput.Controls.Add(this.checkBox_enterAsAutoGenerate);
            this.tabPage_keyinput.Location = new System.Drawing.Point(4, 22);
            this.tabPage_keyinput.Name = "tabPage_keyinput";
            this.tabPage_keyinput.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage_keyinput.Size = new System.Drawing.Size(353, 197);
            this.tabPage_keyinput.TabIndex = 0;
            this.tabPage_keyinput.Text = "键盘输入";
            this.tabPage_keyinput.UseVisualStyleBackColor = true;
            // 
            // checkBox_enterAsAutoGenerate
            // 
            this.checkBox_enterAsAutoGenerate.AutoSize = true;
            this.checkBox_enterAsAutoGenerate.Location = new System.Drawing.Point(7, 7);
            this.checkBox_enterAsAutoGenerate.Name = "checkBox_enterAsAutoGenerate";
            this.checkBox_enterAsAutoGenerate.Size = new System.Drawing.Size(156, 16);
            this.checkBox_enterAsAutoGenerate.TabIndex = 0;
            this.checkBox_enterAsAutoGenerate.Text = "回车用作数据加工触发键";
            this.checkBox_enterAsAutoGenerate.UseVisualStyleBackColor = true;
            // 
            // tabPage_lang
            // 
            this.tabPage_lang.Controls.Add(this.label1);
            this.tabPage_lang.Controls.Add(this.comboBox_uiLanguage);
            this.tabPage_lang.Location = new System.Drawing.Point(4, 22);
            this.tabPage_lang.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_lang.Name = "tabPage_lang";
            this.tabPage_lang.Size = new System.Drawing.Size(353, 197);
            this.tabPage_lang.TabIndex = 1;
            this.tabPage_lang.Text = "语言";
            this.tabPage_lang.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "界面语言(&L):";
            // 
            // comboBox_uiLanguage
            // 
            this.comboBox_uiLanguage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_uiLanguage.DropDownHeight = 300;
            this.comboBox_uiLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_uiLanguage.FormattingEnabled = true;
            this.comboBox_uiLanguage.IntegralHeight = false;
            this.comboBox_uiLanguage.Items.AddRange(new object[] {
            "zh\t中文",
            "en\t英文"});
            this.comboBox_uiLanguage.Location = new System.Drawing.Point(112, 7);
            this.comboBox_uiLanguage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_uiLanguage.Name = "comboBox_uiLanguage";
            this.comboBox_uiLanguage.Size = new System.Drawing.Size(133, 22);
            this.comboBox_uiLanguage.TabIndex = 0;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(214, 242);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(296, 242);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // PropertyDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(380, 276);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PropertyDlg";
            this.ShowInTaskbar = false;
            this.Text = "MARC编辑器属性";
            this.Load += new System.EventHandler(this.PropertyDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_keyinput.ResumeLayout(false);
            this.tabPage_keyinput.PerformLayout();
            this.tabPage_lang.ResumeLayout(false);
            this.tabPage_lang.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_keyinput;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_enterAsAutoGenerate;
        private System.Windows.Forms.TabPage tabPage_lang;
        private System.Windows.Forms.Label label1;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_uiLanguage;
    }
}