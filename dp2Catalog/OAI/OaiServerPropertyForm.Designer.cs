namespace dp2Catalog
{
    partial class OaiServerPropertyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OaiServerPropertyForm));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_general = new System.Windows.Forms.TabPage();
            this.button_gotoHomepage = new System.Windows.Forms.Button();
            this.textBox_homepage = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox_initializeInformation = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_baseUrl = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serverName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_database = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_databaseNames = new System.Windows.Forms.TextBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_general.SuspendLayout();
            this.tabPage_database.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(447, 452);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(366, 452);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_general);
            this.tabControl_main.Controls.Add(this.tabPage_database);
            this.tabControl_main.Location = new System.Drawing.Point(12, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(510, 433);
            this.tabControl_main.TabIndex = 3;
            // 
            // tabPage_general
            // 
            this.tabPage_general.Controls.Add(this.button_gotoHomepage);
            this.tabPage_general.Controls.Add(this.textBox_homepage);
            this.tabPage_general.Controls.Add(this.label16);
            this.tabPage_general.Controls.Add(this.textBox_initializeInformation);
            this.tabPage_general.Controls.Add(this.label4);
            this.tabPage_general.Controls.Add(this.textBox_baseUrl);
            this.tabPage_general.Controls.Add(this.label2);
            this.tabPage_general.Controls.Add(this.textBox_serverName);
            this.tabPage_general.Controls.Add(this.label1);
            this.tabPage_general.Location = new System.Drawing.Point(4, 24);
            this.tabPage_general.Name = "tabPage_general";
            this.tabPage_general.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_general.Size = new System.Drawing.Size(502, 405);
            this.tabPage_general.TabIndex = 0;
            this.tabPage_general.Text = "一般属性";
            this.tabPage_general.UseVisualStyleBackColor = true;
            // 
            // button_gotoHomepage
            // 
            this.button_gotoHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_gotoHomepage.Image = ((System.Drawing.Image)(resources.GetObject("button_gotoHomepage.Image")));
            this.button_gotoHomepage.Location = new System.Drawing.Point(466, 108);
            this.button_gotoHomepage.Name = "button_gotoHomepage";
            this.button_gotoHomepage.Size = new System.Drawing.Size(30, 27);
            this.button_gotoHomepage.TabIndex = 10;
            this.button_gotoHomepage.UseVisualStyleBackColor = true;
            // 
            // textBox_homepage
            // 
            this.textBox_homepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_homepage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_homepage.Location = new System.Drawing.Point(131, 108);
            this.textBox_homepage.Name = "textBox_homepage";
            this.textBox_homepage.Size = new System.Drawing.Size(329, 25);
            this.textBox_homepage.TabIndex = 7;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(6, 113);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(93, 15);
            this.label16.TabIndex = 6;
            this.label16.Text = "Web主页(&H):";
            // 
            // textBox_initializeInformation
            // 
            this.textBox_initializeInformation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_initializeInformation.Location = new System.Drawing.Point(6, 166);
            this.textBox_initializeInformation.Multiline = true;
            this.textBox_initializeInformation.Name = "textBox_initializeInformation";
            this.textBox_initializeInformation.ReadOnly = true;
            this.textBox_initializeInformation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_initializeInformation.Size = new System.Drawing.Size(490, 221);
            this.textBox_initializeInformation.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 148);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 15);
            this.label4.TabIndex = 8;
            this.label4.Text = "初始化信息(&I):";
            // 
            // textBox_baseUrl
            // 
            this.textBox_baseUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_baseUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_baseUrl.Location = new System.Drawing.Point(131, 45);
            this.textBox_baseUrl.Name = "textBox_baseUrl";
            this.textBox_baseUrl.Size = new System.Drawing.Size(365, 25);
            this.textBox_baseUrl.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "基地址(&B):";
            // 
            // textBox_serverName
            // 
            this.textBox_serverName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_serverName.Location = new System.Drawing.Point(131, 13);
            this.textBox_serverName.Name = "textBox_serverName";
            this.textBox_serverName.Size = new System.Drawing.Size(365, 25);
            this.textBox_serverName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "服务器名(&N):";
            // 
            // tabPage_database
            // 
            this.tabPage_database.Controls.Add(this.label10);
            this.tabPage_database.Controls.Add(this.label9);
            this.tabPage_database.Controls.Add(this.textBox_databaseNames);
            this.tabPage_database.Location = new System.Drawing.Point(4, 24);
            this.tabPage_database.Name = "tabPage_database";
            this.tabPage_database.Size = new System.Drawing.Size(502, 405);
            this.tabPage_database.TabIndex = 3;
            this.tabPage_database.Text = "数据库";
            this.tabPage_database.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(4, 235);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(368, 15);
            this.label10.TabIndex = 2;
            this.label10.Text = "(注：可输入多个数据库名。格式为每行一个数据库名)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 21);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(99, 15);
            this.label9.TabIndex = 1;
            this.label9.Text = "数据库名(&N):";
            // 
            // textBox_databaseNames
            // 
            this.textBox_databaseNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_databaseNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_databaseNames.Location = new System.Drawing.Point(4, 42);
            this.textBox_databaseNames.Multiline = true;
            this.textBox_databaseNames.Name = "textBox_databaseNames";
            this.textBox_databaseNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_databaseNames.Size = new System.Drawing.Size(495, 186);
            this.textBox_databaseNames.TabIndex = 0;
            // 
            // OaiServerPropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(535, 492);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Name = "OaiServerPropertyForm";
            this.Text = "OAI服务器属性";
            this.Load += new System.EventHandler(this.OaiServerPropertyForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_general.ResumeLayout(false);
            this.tabPage_general.PerformLayout();
            this.tabPage_database.ResumeLayout(false);
            this.tabPage_database.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_general;
        private System.Windows.Forms.Button button_gotoHomepage;
        private System.Windows.Forms.TextBox textBox_homepage;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox textBox_initializeInformation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_baseUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_serverName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_database;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_databaseNames;
    }
}