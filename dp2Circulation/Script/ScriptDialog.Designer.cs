namespace dp2Circulation.Script
{
    partial class ScriptDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.panel_main = new System.Windows.Forms.Panel();
            this.button_biblio_deleteScriptFile = new System.Windows.Forms.Button();
            this.button_biblio_createScriptFile = new System.Windows.Forms.Button();
            this.textBox_biblio_filterScriptCode = new System.Windows.Forms.TextBox();
            this.comboBox_biblio_filterScript = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(741, 516);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(596, 516);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // panel_main
            // 
            this.panel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_main.Controls.Add(this.button_biblio_deleteScriptFile);
            this.panel_main.Controls.Add(this.button_biblio_createScriptFile);
            this.panel_main.Controls.Add(this.textBox_biblio_filterScriptCode);
            this.panel_main.Controls.Add(this.comboBox_biblio_filterScript);
            this.panel_main.Controls.Add(this.label2);
            this.panel_main.Location = new System.Drawing.Point(13, 13);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(866, 496);
            this.panel_main.TabIndex = 12;
            // 
            // button_biblio_deleteScriptFile
            // 
            this.button_biblio_deleteScriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_biblio_deleteScriptFile.Location = new System.Drawing.Point(781, 28);
            this.button_biblio_deleteScriptFile.Name = "button_biblio_deleteScriptFile";
            this.button_biblio_deleteScriptFile.Size = new System.Drawing.Size(82, 30);
            this.button_biblio_deleteScriptFile.TabIndex = 11;
            this.button_biblio_deleteScriptFile.Text = "删除";
            this.button_biblio_deleteScriptFile.UseVisualStyleBackColor = true;
            this.button_biblio_deleteScriptFile.Click += new System.EventHandler(this.button_biblio_deleteScriptFile_Click);
            // 
            // button_biblio_createScriptFile
            // 
            this.button_biblio_createScriptFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_biblio_createScriptFile.Location = new System.Drawing.Point(686, 28);
            this.button_biblio_createScriptFile.Name = "button_biblio_createScriptFile";
            this.button_biblio_createScriptFile.Size = new System.Drawing.Size(89, 30);
            this.button_biblio_createScriptFile.TabIndex = 10;
            this.button_biblio_createScriptFile.Text = "创建";
            this.button_biblio_createScriptFile.UseVisualStyleBackColor = true;
            this.button_biblio_createScriptFile.Click += new System.EventHandler(this.button_biblio_createScriptFile_Click);
            // 
            // textBox_biblio_filterScriptCode
            // 
            this.textBox_biblio_filterScriptCode.AcceptsReturn = true;
            this.textBox_biblio_filterScriptCode.AcceptsTab = true;
            this.textBox_biblio_filterScriptCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblio_filterScriptCode.HideSelection = false;
            this.textBox_biblio_filterScriptCode.Location = new System.Drawing.Point(3, 64);
            this.textBox_biblio_filterScriptCode.Multiline = true;
            this.textBox_biblio_filterScriptCode.Name = "textBox_biblio_filterScriptCode";
            this.textBox_biblio_filterScriptCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_biblio_filterScriptCode.Size = new System.Drawing.Size(860, 429);
            this.textBox_biblio_filterScriptCode.TabIndex = 9;
            this.textBox_biblio_filterScriptCode.TextChanged += new System.EventHandler(this.textBox_biblio_filterScriptCode_TextChanged);
            // 
            // comboBox_biblio_filterScript
            // 
            this.comboBox_biblio_filterScript.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_biblio_filterScript.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_biblio_filterScript.FormattingEnabled = true;
            this.comboBox_biblio_filterScript.Location = new System.Drawing.Point(3, 28);
            this.comboBox_biblio_filterScript.Name = "comboBox_biblio_filterScript";
            this.comboBox_biblio_filterScript.Size = new System.Drawing.Size(677, 29);
            this.comboBox_biblio_filterScript.TabIndex = 8;
            this.comboBox_biblio_filterScript.SelectedIndexChanged += new System.EventHandler(this.comboBox_biblio_filterScript_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(159, 21);
            this.label2.TabIndex = 7;
            this.label2.Text = "脚本文件名(&S):";
            // 
            // ScriptDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(892, 569);
            this.Controls.Add(this.panel_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ScriptDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "选择脚本";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ScriptDialog_FormClosed);
            this.Load += new System.EventHandler(this.ScriptDialog_Load);
            this.panel_main.ResumeLayout(false);
            this.panel_main.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.Button button_biblio_deleteScriptFile;
        private System.Windows.Forms.Button button_biblio_createScriptFile;
        private System.Windows.Forms.TextBox textBox_biblio_filterScriptCode;
        private System.Windows.Forms.ComboBox comboBox_biblio_filterScript;
        private System.Windows.Forms.Label label2;
    }
}