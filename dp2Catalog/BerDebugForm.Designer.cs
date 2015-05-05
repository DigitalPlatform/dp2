namespace dp2Catalog
{
    partial class BerDebugForm
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
            this.button_load = new System.Windows.Forms.Button();
            this.button_findLogFilename = new System.Windows.Forms.Button();
            this.textBox_logFilename = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.treeView_ber = new System.Windows.Forms.TreeView();
            this.binaryEditor_onePackage = new DigitalPlatform.CommonControl.BinaryEditor();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_load
            // 
            this.button_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_load.Location = new System.Drawing.Point(536, 12);
            this.button_load.Name = "button_load";
            this.button_load.Size = new System.Drawing.Size(61, 27);
            this.button_load.TabIndex = 8;
            this.button_load.Text = "&Load";
            this.button_load.UseVisualStyleBackColor = true;
            this.button_load.Click += new System.EventHandler(this.button_load_Click);
            // 
            // button_findLogFilename
            // 
            this.button_findLogFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findLogFilename.Location = new System.Drawing.Point(483, 12);
            this.button_findLogFilename.Name = "button_findLogFilename";
            this.button_findLogFilename.Size = new System.Drawing.Size(47, 27);
            this.button_findLogFilename.TabIndex = 7;
            this.button_findLogFilename.Text = "...";
            this.button_findLogFilename.UseVisualStyleBackColor = true;
            this.button_findLogFilename.Click += new System.EventHandler(this.button_findLogFilename_Click);
            // 
            // textBox_logFilename
            // 
            this.textBox_logFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_logFilename.Location = new System.Drawing.Point(140, 12);
            this.textBox_logFilename.Name = "textBox_logFilename";
            this.textBox_logFilename.Size = new System.Drawing.Size(337, 25);
            this.textBox_logFilename.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Log File&name:";
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(13, 45);
            this.splitContainer_main.Name = "splitContainer_main";
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.treeView_ber);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.binaryEditor_onePackage);
            this.splitContainer_main.Size = new System.Drawing.Size(584, 362);
            this.splitContainer_main.SplitterDistance = 194;
            this.splitContainer_main.TabIndex = 9;
            // 
            // treeView_ber
            // 
            this.treeView_ber.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_ber.Location = new System.Drawing.Point(0, 0);
            this.treeView_ber.Name = "treeView_ber";
            this.treeView_ber.Size = new System.Drawing.Size(194, 362);
            this.treeView_ber.TabIndex = 0;
            this.treeView_ber.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_ber_AfterSelect);
            this.treeView_ber.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_ber_MouseUp);
            this.treeView_ber.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_ber_MouseDown);
            // 
            // binaryEditor_onePackage
            // 
            this.binaryEditor_onePackage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.binaryEditor_onePackage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.binaryEditor_onePackage.DocumentOrgX = ((long)(0));
            this.binaryEditor_onePackage.DocumentOrgY = ((long)(0));
            this.binaryEditor_onePackage.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.binaryEditor_onePackage.Location = new System.Drawing.Point(0, 0);
            this.binaryEditor_onePackage.Name = "binaryEditor_onePackage";
            this.binaryEditor_onePackage.Size = new System.Drawing.Size(386, 362);
            this.binaryEditor_onePackage.TabIndex = 0;
            this.binaryEditor_onePackage.Text = "binaryEditor1";
            // 
            // BerDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 419);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_load);
            this.Controls.Add(this.button_findLogFilename);
            this.Controls.Add(this.textBox_logFilename);
            this.Controls.Add(this.label1);
            this.Name = "BerDebugForm";
            this.Text = "BerDebugForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BerDebugForm_FormClosed);
            this.Load += new System.EventHandler(this.BerDebugForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_load;
        private System.Windows.Forms.Button button_findLogFilename;
        private System.Windows.Forms.TextBox textBox_logFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TreeView treeView_ber;
        private DigitalPlatform.CommonControl.BinaryEditor binaryEditor_onePackage;
    }
}