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
            this.ToolStripMenuItem_moveBiblioRecord = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_compileAllProjects = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_parsePriceUnit = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_objectWriteRead = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_logAndRecover = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_getPdfSinglePage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.ToolStripMenuItem_testGCAT = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Left;
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_test,
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(189, 392);
            this.menuStrip1.Stretch = false;
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ToolStripMenuItem_test
            // 
            this.ToolStripMenuItem_test.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_createAccessNo,
            this.ToolStripMenuItem_moveBiblioRecord,
            this.ToolStripMenuItem_compileAllProjects,
            this.ToolStripMenuItem_parsePriceUnit,
            this.ToolStripMenuItem_objectWriteRead,
            this.ToolStripMenuItem_logAndRecover,
            this.ToolStripMenuItem_getPdfSinglePage,
            this.ToolStripMenuItem_testGCAT});
            this.ToolStripMenuItem_test.Name = "ToolStripMenuItem_test";
            this.ToolStripMenuItem_test.Size = new System.Drawing.Size(170, 28);
            this.ToolStripMenuItem_test.Text = "测试";
            // 
            // ToolStripMenuItem_createAccessNo
            // 
            this.ToolStripMenuItem_createAccessNo.Name = "ToolStripMenuItem_createAccessNo";
            this.ToolStripMenuItem_createAccessNo.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_createAccessNo.Text = "创建索取号(种次号)";
            this.ToolStripMenuItem_createAccessNo.Click += new System.EventHandler(this.ToolStripMenuItem_createAccessNo_Click);
            // 
            // ToolStripMenuItem_moveBiblioRecord
            // 
            this.ToolStripMenuItem_moveBiblioRecord.Name = "ToolStripMenuItem_moveBiblioRecord";
            this.ToolStripMenuItem_moveBiblioRecord.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_moveBiblioRecord.Text = "移动书目记录";
            this.ToolStripMenuItem_moveBiblioRecord.Click += new System.EventHandler(this.ToolStripMenuItem_moveBiblioRecord_Click);
            // 
            // ToolStripMenuItem_compileAllProjects
            // 
            this.ToolStripMenuItem_compileAllProjects.Name = "ToolStripMenuItem_compileAllProjects";
            this.ToolStripMenuItem_compileAllProjects.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_compileAllProjects.Text = "编译所有统计方案";
            this.ToolStripMenuItem_compileAllProjects.Click += new System.EventHandler(this.ToolStripMenuItem_compileAllProjects_Click);
            // 
            // ToolStripMenuItem_parsePriceUnit
            // 
            this.ToolStripMenuItem_parsePriceUnit.Name = "ToolStripMenuItem_parsePriceUnit";
            this.ToolStripMenuItem_parsePriceUnit.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_parsePriceUnit.Text = "ParsePriceUnit()";
            this.ToolStripMenuItem_parsePriceUnit.Click += new System.EventHandler(this.ToolStripMenuItem_parsePriceUnit_Click);
            // 
            // ToolStripMenuItem_objectWriteRead
            // 
            this.ToolStripMenuItem_objectWriteRead.Name = "ToolStripMenuItem_objectWriteRead";
            this.ToolStripMenuItem_objectWriteRead.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_objectWriteRead.Text = "元数据和对象的写入和读出";
            this.ToolStripMenuItem_objectWriteRead.Click += new System.EventHandler(this.ToolStripMenuItem_objectWriteRead_Click);
            // 
            // ToolStripMenuItem_logAndRecover
            // 
            this.ToolStripMenuItem_logAndRecover.Name = "ToolStripMenuItem_logAndRecover";
            this.ToolStripMenuItem_logAndRecover.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_logAndRecover.Text = "测试日志和恢复";
            this.ToolStripMenuItem_logAndRecover.Click += new System.EventHandler(this.ToolStripMenuItem_logAndRecover_Click);
            // 
            // ToolStripMenuItem_getPdfSinglePage
            // 
            this.ToolStripMenuItem_getPdfSinglePage.Name = "ToolStripMenuItem_getPdfSinglePage";
            this.ToolStripMenuItem_getPdfSinglePage.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_getPdfSinglePage.Text = "测试获取 PDF 单页";
            this.ToolStripMenuItem_getPdfSinglePage.Click += new System.EventHandler(this.ToolStripMenuItem_getPdfSinglePage_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(170, 28);
            this.toolStripMenuItem1.Text = "2";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(170, 28);
            this.toolStripMenuItem2.Text = "3";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(33, 82);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 34);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(189, 370);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip1.Size = new System.Drawing.Size(237, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(189, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(237, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // menuStrip2
            // 
            this.menuStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip2.Size = new System.Drawing.Size(426, 36);
            this.menuStrip2.TabIndex = 4;
            this.menuStrip2.Text = "menuStrip2";
            this.menuStrip2.Visible = false;
            // 
            // ToolStripMenuItem_testGCAT
            // 
            this.ToolStripMenuItem_testGCAT.Name = "ToolStripMenuItem_testGCAT";
            this.ToolStripMenuItem_testGCAT.Size = new System.Drawing.Size(308, 30);
            this.ToolStripMenuItem_testGCAT.Text = "测试 GCAT";
            this.ToolStripMenuItem_testGCAT.Click += new System.EventHandler(this.ToolStripMenuItem_testGCAT_Click);
            // 
            // TestingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(426, 392);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.menuStrip2);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_compileAllProjects;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_parsePriceUnit;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_objectWriteRead;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_logAndRecover;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_getPdfSinglePage;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_testGCAT;
    }
}