namespace dp2Catalog
{
    partial class SruSearchForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_use = new System.Windows.Forms.ComboBox();
            this.comboBox_format = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_server = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_search = new System.Windows.Forms.Button();
            this.listView_browse = new System.Windows.Forms.ListView();
            this.columnHeader_index = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_author = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_publisher = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "检索词:";
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Location = new System.Drawing.Point(147, 28);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(267, 28);
            this.textBox_queryWord.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "检索途径:";
            // 
            // comboBox_use
            // 
            this.comboBox_use.FormattingEnabled = true;
            this.comboBox_use.Location = new System.Drawing.Point(147, 63);
            this.comboBox_use.Name = "comboBox_use";
            this.comboBox_use.Size = new System.Drawing.Size(267, 26);
            this.comboBox_use.TabIndex = 3;
            this.comboBox_use.DropDown += new System.EventHandler(this.comboBox_use_DropDown);
            // 
            // comboBox_format
            // 
            this.comboBox_format.FormattingEnabled = true;
            this.comboBox_format.Location = new System.Drawing.Point(147, 95);
            this.comboBox_format.Name = "comboBox_format";
            this.comboBox_format.Size = new System.Drawing.Size(267, 26);
            this.comboBox_format.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "数据格式:";
            // 
            // comboBox_server
            // 
            this.comboBox_server.FormattingEnabled = true;
            this.comboBox_server.Location = new System.Drawing.Point(147, 127);
            this.comboBox_server.Name = "comboBox_server";
            this.comboBox_server.Size = new System.Drawing.Size(267, 26);
            this.comboBox_server.TabIndex = 7;
            this.comboBox_server.SelectedIndexChanged += new System.EventHandler(this.comboBox_server_SelectedIndexChanged);
            this.comboBox_server.TextChanged += new System.EventHandler(this.comboBox_server_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "检索目标:";
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(459, 31);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(127, 34);
            this.button_search.TabIndex = 8;
            this.button_search.Text = "检索";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // listView_browse
            // 
            this.listView_browse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_index,
            this.columnHeader_title,
            this.columnHeader_author,
            this.columnHeader_publisher});
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.Location = new System.Drawing.Point(12, 164);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(776, 274);
            this.listView_browse.TabIndex = 9;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_index
            // 
            this.columnHeader_index.Text = "序号";
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "题名";
            this.columnHeader_title.Width = 200;
            // 
            // columnHeader_author
            // 
            this.columnHeader_author.Text = "著者";
            this.columnHeader_author.Width = 200;
            // 
            // columnHeader_publisher
            // 
            this.columnHeader_publisher.Text = "出版者";
            this.columnHeader_publisher.Width = 200;
            // 
            // SruSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listView_browse);
            this.Controls.Add(this.button_search);
            this.Controls.Add(this.comboBox_server);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBox_format);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_use);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_queryWord);
            this.Controls.Add(this.label1);
            this.Name = "SruSearchForm";
            this.Text = "SruSearchForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SruSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.SruSearchForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_use;
        private System.Windows.Forms.ComboBox comboBox_format;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_server;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.ListView listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_index;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_author;
        private System.Windows.Forms.ColumnHeader columnHeader_publisher;
    }
}