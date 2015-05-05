namespace dp2Circulation
{
    partial class TestSearchForm
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
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_searchBiblio = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_searchBiblio_beforeAbort = new System.Windows.Forms.TextBox();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.button_searchBiblio_findFilename = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_biblioSearch_queryFilename = new System.Windows.Forms.TextBox();
            this.button_beginSearch = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_searchBiblio_loopTimes = new System.Windows.Forms.TextBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_searchBiblio.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_searchBiblio);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(366, 211);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_searchBiblio
            // 
            this.tabPage_searchBiblio.Controls.Add(this.label3);
            this.tabPage_searchBiblio.Controls.Add(this.textBox_searchBiblio_loopTimes);
            this.tabPage_searchBiblio.Controls.Add(this.label2);
            this.tabPage_searchBiblio.Controls.Add(this.textBox_searchBiblio_beforeAbort);
            this.tabPage_searchBiblio.Controls.Add(this.webBrowser1);
            this.tabPage_searchBiblio.Controls.Add(this.button_searchBiblio_findFilename);
            this.tabPage_searchBiblio.Controls.Add(this.label1);
            this.tabPage_searchBiblio.Controls.Add(this.textBox_biblioSearch_queryFilename);
            this.tabPage_searchBiblio.Location = new System.Drawing.Point(4, 22);
            this.tabPage_searchBiblio.Name = "tabPage_searchBiblio";
            this.tabPage_searchBiblio.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_searchBiblio.Size = new System.Drawing.Size(358, 185);
            this.tabPage_searchBiblio.TabIndex = 0;
            this.tabPage_searchBiblio.Text = "检索书目库";
            this.tabPage_searchBiblio.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "切断时刻(&A):";
            // 
            // textBox_searchBiblio_beforeAbort
            // 
            this.textBox_searchBiblio_beforeAbort.Location = new System.Drawing.Point(101, 34);
            this.textBox_searchBiblio_beforeAbort.Name = "textBox_searchBiblio_beforeAbort";
            this.textBox_searchBiblio_beforeAbort.Size = new System.Drawing.Size(90, 21);
            this.textBox_searchBiblio_beforeAbort.TabIndex = 4;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(8, 88);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(344, 91);
            this.webBrowser1.TabIndex = 3;
            // 
            // button_searchBiblio_findFilename
            // 
            this.button_searchBiblio_findFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_searchBiblio_findFilename.Location = new System.Drawing.Point(313, 5);
            this.button_searchBiblio_findFilename.Name = "button_searchBiblio_findFilename";
            this.button_searchBiblio_findFilename.Size = new System.Drawing.Size(39, 23);
            this.button_searchBiblio_findFilename.TabIndex = 2;
            this.button_searchBiblio_findFilename.Text = "...";
            this.button_searchBiblio_findFilename.UseVisualStyleBackColor = true;
            this.button_searchBiblio_findFilename.Click += new System.EventHandler(this.button_searchBiblio_findFilename_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "检索式文件(&F):";
            // 
            // textBox_biblioSearch_queryFilename
            // 
            this.textBox_biblioSearch_queryFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_biblioSearch_queryFilename.Location = new System.Drawing.Point(101, 7);
            this.textBox_biblioSearch_queryFilename.Name = "textBox_biblioSearch_queryFilename";
            this.textBox_biblioSearch_queryFilename.Size = new System.Drawing.Size(206, 21);
            this.textBox_biblioSearch_queryFilename.TabIndex = 0;
            // 
            // button_beginSearch
            // 
            this.button_beginSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_beginSearch.Location = new System.Drawing.Point(303, 229);
            this.button_beginSearch.Name = "button_beginSearch";
            this.button_beginSearch.Size = new System.Drawing.Size(75, 23);
            this.button_beginSearch.TabIndex = 1;
            this.button_beginSearch.Text = "开始检索";
            this.button_beginSearch.UseVisualStyleBackColor = true;
            this.button_beginSearch.Click += new System.EventHandler(this.button_beginSearch_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "循环次数(&L):";
            // 
            // textBox_searchBiblio_loopTimes
            // 
            this.textBox_searchBiblio_loopTimes.Location = new System.Drawing.Point(101, 61);
            this.textBox_searchBiblio_loopTimes.Name = "textBox_searchBiblio_loopTimes";
            this.textBox_searchBiblio_loopTimes.Size = new System.Drawing.Size(90, 21);
            this.textBox_searchBiblio_loopTimes.TabIndex = 6;
            // 
            // TestSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 264);
            this.Controls.Add(this.button_beginSearch);
            this.Controls.Add(this.tabControl_main);
            this.Name = "TestSearchForm";
            this.Text = "TestSearchForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TestSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.TestSearchForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_searchBiblio.ResumeLayout(false);
            this.tabPage_searchBiblio.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_searchBiblio;
        private System.Windows.Forms.Button button_searchBiblio_findFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_biblioSearch_queryFilename;
        private System.Windows.Forms.Button button_beginSearch;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_searchBiblio_beforeAbort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_searchBiblio_loopTimes;
    }
}