namespace dp2Circulation
{
    partial class RunScriptDialog
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
            this.button_getScriptFileName = new System.Windows.Forms.Button();
            this.textBox_scriptFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_forceSave = new System.Windows.Forms.CheckBox();
            this.checkBox_autoSaveChanges = new System.Windows.Forms.CheckBox();
            this.checkBox_noOperation = new System.Windows.Forms.CheckBox();
            this.checkBox_dontTriggerAutoGen = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_dontLogging = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_getScriptFileName
            // 
            this.button_getScriptFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getScriptFileName.Location = new System.Drawing.Point(743, 36);
            this.button_getScriptFileName.Margin = new System.Windows.Forms.Padding(5);
            this.button_getScriptFileName.Name = "button_getScriptFileName";
            this.button_getScriptFileName.Size = new System.Drawing.Size(88, 40);
            this.button_getScriptFileName.TabIndex = 2;
            this.button_getScriptFileName.Text = "...";
            this.button_getScriptFileName.UseVisualStyleBackColor = true;
            this.button_getScriptFileName.Click += new System.EventHandler(this.button_getScriptFileName_Click);
            // 
            // textBox_scriptFileName
            // 
            this.textBox_scriptFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_scriptFileName.Location = new System.Drawing.Point(20, 36);
            this.textBox_scriptFileName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_scriptFileName.Name = "textBox_scriptFileName";
            this.textBox_scriptFileName.Size = new System.Drawing.Size(712, 31);
            this.textBox_scriptFileName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(201, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "脚本程序文件名(&S):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(546, 326);
            this.button_OK.Margin = new System.Windows.Forms.Padding(5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(137, 40);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(694, 326);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(137, 40);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_forceSave
            // 
            this.checkBox_forceSave.AutoSize = true;
            this.checkBox_forceSave.Location = new System.Drawing.Point(26, 41);
            this.checkBox_forceSave.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_forceSave.Name = "checkBox_forceSave";
            this.checkBox_forceSave.Size = new System.Drawing.Size(153, 25);
            this.checkBox_forceSave.TabIndex = 4;
            this.checkBox_forceSave.Text = "强制保存(&F)";
            this.checkBox_forceSave.UseVisualStyleBackColor = true;
            // 
            // checkBox_autoSaveChanges
            // 
            this.checkBox_autoSaveChanges.AutoSize = true;
            this.checkBox_autoSaveChanges.Location = new System.Drawing.Point(20, 78);
            this.checkBox_autoSaveChanges.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_autoSaveChanges.Name = "checkBox_autoSaveChanges";
            this.checkBox_autoSaveChanges.Size = new System.Drawing.Size(195, 25);
            this.checkBox_autoSaveChanges.TabIndex = 3;
            this.checkBox_autoSaveChanges.Text = "自动保存修改(&S)";
            this.checkBox_autoSaveChanges.UseVisualStyleBackColor = true;
            this.checkBox_autoSaveChanges.CheckedChanged += new System.EventHandler(this.checkBox_autoSaveChanges_CheckedChanged);
            // 
            // checkBox_noOperation
            // 
            this.checkBox_noOperation.AutoSize = true;
            this.checkBox_noOperation.Location = new System.Drawing.Point(331, 41);
            this.checkBox_noOperation.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_noOperation.Name = "checkBox_noOperation";
            this.checkBox_noOperation.Size = new System.Drawing.Size(295, 25);
            this.checkBox_noOperation.TabIndex = 5;
            this.checkBox_noOperation.Text = "不产生 operation 元素(&O)";
            this.checkBox_noOperation.UseVisualStyleBackColor = true;
            // 
            // checkBox_dontTriggerAutoGen
            // 
            this.checkBox_dontTriggerAutoGen.AutoSize = true;
            this.checkBox_dontTriggerAutoGen.Location = new System.Drawing.Point(26, 157);
            this.checkBox_dontTriggerAutoGen.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_dontTriggerAutoGen.Name = "checkBox_dontTriggerAutoGen";
            this.checkBox_dontTriggerAutoGen.Size = new System.Drawing.Size(258, 25);
            this.checkBox_dontTriggerAutoGen.TabIndex = 6;
            this.checkBox_dontTriggerAutoGen.Text = "不触发前端自动创建(&N)";
            this.checkBox_dontTriggerAutoGen.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBox_dontLogging);
            this.groupBox1.Controls.Add(this.checkBox_noOperation);
            this.groupBox1.Controls.Add(this.checkBox_dontTriggerAutoGen);
            this.groupBox1.Controls.Add(this.checkBox_forceSave);
            this.groupBox1.Location = new System.Drawing.Point(20, 111);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(811, 207);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // checkBox_dontLogging
            // 
            this.checkBox_dontLogging.AutoSize = true;
            this.checkBox_dontLogging.Location = new System.Drawing.Point(26, 74);
            this.checkBox_dontLogging.Name = "checkBox_dontLogging";
            this.checkBox_dontLogging.Size = new System.Drawing.Size(216, 25);
            this.checkBox_dontLogging.TabIndex = 7;
            this.checkBox_dontLogging.Text = "不产生操作日志(&L)";
            this.checkBox_dontLogging.UseVisualStyleBackColor = true;
            // 
            // RunScriptDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(846, 381);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBox_autoSaveChanges);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_getScriptFileName);
            this.Controls.Add(this.textBox_scriptFileName);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RunScriptDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "执行 C# 脚本";
            this.Load += new System.EventHandler(this.RunScriptDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_getScriptFileName;
        private System.Windows.Forms.TextBox textBox_scriptFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.CheckBox checkBox_forceSave;
        private System.Windows.Forms.CheckBox checkBox_autoSaveChanges;
        private System.Windows.Forms.CheckBox checkBox_noOperation;
        private System.Windows.Forms.CheckBox checkBox_dontTriggerAutoGen;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_dontLogging;
    }
}