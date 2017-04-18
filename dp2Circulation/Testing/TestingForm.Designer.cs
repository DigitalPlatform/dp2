namespace dp2Circulation
{
    partial class TestingForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ToolStripMenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createAccessNo = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.ToolStripMenuItem_moveBiblioRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_test});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(284, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ToolStripMenuItem_test
            // 
            this.ToolStripMenuItem_test.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_createAccessNo,
            this.ToolStripMenuItem_moveBiblioRecord});
            this.ToolStripMenuItem_test.Name = "ToolStripMenuItem_test";
            this.ToolStripMenuItem_test.Size = new System.Drawing.Size(44, 21);
            this.ToolStripMenuItem_test.Text = "测试";
            // 
            // ToolStripMenuItem_createAccessNo
            // 
            this.ToolStripMenuItem_createAccessNo.Name = "ToolStripMenuItem_createAccessNo";
            this.ToolStripMenuItem_createAccessNo.Size = new System.Drawing.Size(180, 22);
            this.ToolStripMenuItem_createAccessNo.Text = "创建索取号(种次号)";
            this.ToolStripMenuItem_createAccessNo.Click += new System.EventHandler(this.ToolStripMenuItem_createAccessNo_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(22, 55);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 239);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(284, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Location = new System.Drawing.Point(0, 25);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(284, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(284, 24);
            this.menuStrip2.TabIndex = 4;
            this.menuStrip2.Text = "menuStrip2";
            this.menuStrip2.Visible = false;
            // 
            // ToolStripMenuItem_moveBiblioRecord
            // 
            this.ToolStripMenuItem_moveBiblioRecord.Name = "ToolStripMenuItem_moveBiblioRecord";
            this.ToolStripMenuItem_moveBiblioRecord.Size = new System.Drawing.Size(180, 22);
            this.ToolStripMenuItem_moveBiblioRecord.Text = "移动书目记录";
            this.ToolStripMenuItem_moveBiblioRecord.Click += new System.EventHandler(this.ToolStripMenuItem_moveBiblioRecord_Click);
            // 
            // TestingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.menuStrip2);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TestingForm";
            this.Text = "TestingForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestingForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TestingForm_FormClosed);
            this.Load += new System.EventHandler(this.TestingForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_test;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createAccessNo;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_moveBiblioRecord;

    }
}