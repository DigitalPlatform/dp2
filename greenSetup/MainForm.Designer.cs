namespace greenSetup
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_test = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button_createShortcut = new System.Windows.Forms.Button();
            // 
            // button_test
            // 
            this.button_test.Location = new System.Drawing.Point(15, 10);
            this.button_test.Name = "button_test";
            this.button_test.Size = new System.Drawing.Size(219, 54);
            this.button_test.TabIndex = 0;
            this.button_test.Text = "test";
            this.button_test.UseVisualStyleBackColor = true;
            this.button_test.Visible = false;
            this.button_test.Click += new System.EventHandler(this.button_test_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(0, 165);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(899, 92);
            this.label_message.TabIndex = 1;
            this.label_message.Text = "label1";
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 270);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(899, 40);
            this.progressBar1.TabIndex = 2;
            // 
            // button_createShortcut
            // 
            this.button_createShortcut.Location = new System.Drawing.Point(336, 10);
            this.button_createShortcut.Name = "button_createShortcut";
            this.button_createShortcut.Size = new System.Drawing.Size(219, 54);
            this.button_createShortcut.TabIndex = 0;
            this.button_createShortcut.Text = "create Shortcut";
            this.button_createShortcut.UseVisualStyleBackColor = true;
            this.button_createShortcut.Visible = false;
            this.button_createShortcut.Click += new System.EventHandler(this.button_createShortcut_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(899, 310);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_test);
            this.Controls.Add(this.button_createShortcut);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);

        }

        #endregion

        private System.Windows.Forms.Button button_test;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button_createShortcut;
    }
}

