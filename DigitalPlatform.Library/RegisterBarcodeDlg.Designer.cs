namespace DigitalPlatform.Library
{
    partial class RegisterBarcodeDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterBarcodeDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.flowLayoutPanel_query = new System.Windows.Forms.FlowLayoutPanel();
            this.button_search = new System.Windows.Forms.Button();
            this.checkBox_autoDetectQueryBarcode = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer_recordAndItems = new System.Windows.Forms.SplitContainer();
            this.label_biblioRecPath = new System.Windows.Forms.Label();
            this.webBrowser_record = new System.Windows.Forms.WebBrowser();
            this.listView_items = new System.Windows.Forms.ListView();
            this.columnHeader_barcode = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_state = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_location = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_price = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_bookType = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_borrower = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_borrowDate = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_borrowPeriod = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_recpath = new System.Windows.Forms.ColumnHeader();
            this.label_target = new System.Windows.Forms.Label();
            this.button_save = new System.Windows.Forms.Button();
            this.button_target = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_itemBarcode = new System.Windows.Forms.TextBox();
            this.button_register = new System.Windows.Forms.Button();
            this.flowLayoutPanel_query.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.splitContainer_recordAndItems.Panel1.SuspendLayout();
            this.splitContainer_recordAndItems.Panel2.SuspendLayout();
            this.splitContainer_recordAndItems.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.label1.Size = new System.Drawing.Size(75, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "ÖÖ¼ìË÷´Ê:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Location = new System.Drawing.Point(87, 4);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(219, 25);
            this.textBox_queryWord.TabIndex = 1;
            this.textBox_queryWord.Enter += new System.EventHandler(this.textBox_queryWord_Enter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(314, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(13, 4, 0, 4);
            this.label2.Size = new System.Drawing.Size(88, 30);
            this.label2.TabIndex = 2;
            this.label2.Text = "¼ìË÷Í¾¾¶:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.UseCompatibleTextRendering = true;
            // 
            // comboBox_from
            // 
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.Location = new System.Drawing.Point(410, 4);
            this.comboBox_from.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(160, 23);
            this.comboBox_from.TabIndex = 3;
            // 
            // flowLayoutPanel_query
            // 
            this.flowLayoutPanel_query.AutoSize = true;
            this.flowLayoutPanel_query.Controls.Add(this.label1);
            this.flowLayoutPanel_query.Controls.Add(this.textBox_queryWord);
            this.flowLayoutPanel_query.Controls.Add(this.label2);
            this.flowLayoutPanel_query.Controls.Add(this.comboBox_from);
            this.flowLayoutPanel_query.Controls.Add(this.button_search);
            this.flowLayoutPanel_query.Controls.Add(this.checkBox_autoDetectQueryBarcode);
            this.flowLayoutPanel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_query.Location = new System.Drawing.Point(0, 17);
            this.flowLayoutPanel_query.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_query.Name = "flowLayoutPanel_query";
            this.flowLayoutPanel_query.Size = new System.Drawing.Size(599, 70);
            this.flowLayoutPanel_query.TabIndex = 4;
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(4, 37);
            this.button_search.Margin = new System.Windows.Forms.Padding(4);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(100, 29);
            this.button_search.TabIndex = 4;
            this.button_search.Text = "¼ìË÷";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // checkBox_autoDetectQueryBarcode
            // 
            this.checkBox_autoDetectQueryBarcode.AutoSize = true;
            this.checkBox_autoDetectQueryBarcode.Checked = true;
            this.checkBox_autoDetectQueryBarcode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_autoDetectQueryBarcode.Location = new System.Drawing.Point(112, 37);
            this.checkBox_autoDetectQueryBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_autoDetectQueryBarcode.Name = "checkBox_autoDetectQueryBarcode";
            this.checkBox_autoDetectQueryBarcode.Padding = new System.Windows.Forms.Padding(13, 4, 0, 4);
            this.checkBox_autoDetectQueryBarcode.Size = new System.Drawing.Size(170, 27);
            this.checkBox_autoDetectQueryBarcode.TabIndex = 5;
            this.checkBox_autoDetectQueryBarcode.Text = "ÊÊÓ¦ISBNÌõÂëºÅ(&A)";
            this.checkBox_autoDetectQueryBarcode.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_main.Controls.Add(this.flowLayoutPanel_query, 0, 1);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_recordAndItems, 0, 2);
            this.tableLayoutPanel_main.Controls.Add(this.label_target, 0, 0);
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(12, 11);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.RowCount = 3;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(599, 294);
            this.tableLayoutPanel_main.TabIndex = 5;
            // 
            // splitContainer_recordAndItems
            // 
            this.splitContainer_recordAndItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_recordAndItems.Location = new System.Drawing.Point(4, 91);
            this.splitContainer_recordAndItems.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_recordAndItems.Name = "splitContainer_recordAndItems";
            this.splitContainer_recordAndItems.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_recordAndItems.Panel1
            // 
            this.splitContainer_recordAndItems.Panel1.Controls.Add(this.label_biblioRecPath);
            this.splitContainer_recordAndItems.Panel1.Controls.Add(this.webBrowser_record);
            // 
            // splitContainer_recordAndItems.Panel2
            // 
            this.splitContainer_recordAndItems.Panel2.Controls.Add(this.listView_items);
            this.splitContainer_recordAndItems.Size = new System.Drawing.Size(591, 199);
            this.splitContainer_recordAndItems.SplitterDistance = 95;
            this.splitContainer_recordAndItems.SplitterWidth = 5;
            this.splitContainer_recordAndItems.TabIndex = 5;
            // 
            // label_biblioRecPath
            // 
            this.label_biblioRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_biblioRecPath.AutoSize = true;
            this.label_biblioRecPath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_biblioRecPath.Location = new System.Drawing.Point(0, 0);
            this.label_biblioRecPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_biblioRecPath.Name = "label_biblioRecPath";
            this.label_biblioRecPath.Size = new System.Drawing.Size(65, 17);
            this.label_biblioRecPath.TabIndex = 6;
            this.label_biblioRecPath.Text = "       ";
            this.label_biblioRecPath.DoubleClick += new System.EventHandler(this.label_biblioRecPath_DoubleClick);
            // 
            // webBrowser_record
            // 
            this.webBrowser_record.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser_record.Location = new System.Drawing.Point(0, 24);
            this.webBrowser_record.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser_record.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser_record.Name = "webBrowser_record";
            this.webBrowser_record.Size = new System.Drawing.Size(591, 68);
            this.webBrowser_record.TabIndex = 5;
            // 
            // listView_items
            // 
            this.listView_items.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_barcode,
            this.columnHeader_state,
            this.columnHeader_location,
            this.columnHeader_price,
            this.columnHeader_bookType,
            this.columnHeader_comment,
            this.columnHeader_borrower,
            this.columnHeader_borrowDate,
            this.columnHeader_borrowPeriod,
            this.columnHeader_recpath});
            this.listView_items.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_items.FullRowSelect = true;
            this.listView_items.HideSelection = false;
            this.listView_items.Location = new System.Drawing.Point(0, 0);
            this.listView_items.Margin = new System.Windows.Forms.Padding(4);
            this.listView_items.Name = "listView_items";
            this.listView_items.Size = new System.Drawing.Size(591, 99);
            this.listView_items.TabIndex = 0;
            this.listView_items.UseCompatibleStateImageBehavior = false;
            this.listView_items.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_barcode
            // 
            this.columnHeader_barcode.Text = "²áÌõÂëºÅ";
            this.columnHeader_barcode.Width = 150;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "×´Ì¬";
            this.columnHeader_state.Width = 100;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "¹Ý²ØµØµã";
            this.columnHeader_location.Width = 150;
            // 
            // columnHeader_price
            // 
            this.columnHeader_price.Text = "²á¼Û¸ñ";
            this.columnHeader_price.Width = 150;
            // 
            // columnHeader_bookType
            // 
            this.columnHeader_bookType.Text = "Í¼ÊéÀàÐÍ";
            this.columnHeader_bookType.Width = 150;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "¸½×¢";
            this.columnHeader_comment.Width = 150;
            // 
            // columnHeader_borrower
            // 
            this.columnHeader_borrower.Text = "½èÔÄÕß";
            this.columnHeader_borrower.Width = 150;
            // 
            // columnHeader_borrowDate
            // 
            this.columnHeader_borrowDate.Text = "½èÔÄÈÕÆÚ";
            this.columnHeader_borrowDate.Width = 150;
            // 
            // columnHeader_borrowPeriod
            // 
            this.columnHeader_borrowPeriod.Text = "½èÔÄÆÚÏÞ";
            this.columnHeader_borrowPeriod.Width = 150;
            // 
            // columnHeader_recpath
            // 
            this.columnHeader_recpath.Text = "²á¼ÇÂ¼Â·¾¶";
            this.columnHeader_recpath.Width = 200;
            // 
            // label_target
            // 
            this.label_target.AutoSize = true;
            this.label_target.BackColor = System.Drawing.SystemColors.Info;
            this.label_target.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_target.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_target.Location = new System.Drawing.Point(4, 0);
            this.label_target.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_target.Name = "label_target";
            this.label_target.Size = new System.Drawing.Size(591, 17);
            this.label_target.TabIndex = 6;
            this.label_target.Text = "target info";
            // 
            // button_save
            // 
            this.button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_save.Location = new System.Drawing.Point(503, 358);
            this.button_save.Margin = new System.Windows.Forms.Padding(4);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(100, 29);
            this.button_save.TabIndex = 6;
            this.button_save.Text = "±£´æ";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_target
            // 
            this.button_target.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_target.Location = new System.Drawing.Point(16, 358);
            this.button_target.Margin = new System.Windows.Forms.Padding(4);
            this.button_target.Name = "button_target";
            this.button_target.Size = new System.Drawing.Size(100, 29);
            this.button_target.TabIndex = 7;
            this.button_target.Text = "Ä¿±ê...";
            this.button_target.UseVisualStyleBackColor = true;
            this.button_target.Click += new System.EventHandler(this.button_target_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 322);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "²áÌõÂëºÅ(&B):";
            // 
            // textBox_itemBarcode
            // 
            this.textBox_itemBarcode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_itemBarcode.Location = new System.Drawing.Point(136, 319);
            this.textBox_itemBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_itemBarcode.Name = "textBox_itemBarcode";
            this.textBox_itemBarcode.Size = new System.Drawing.Size(357, 25);
            this.textBox_itemBarcode.TabIndex = 9;
            this.textBox_itemBarcode.Enter += new System.EventHandler(this.textBox_itemBarcode_Enter);
            // 
            // button_register
            // 
            this.button_register.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_register.Location = new System.Drawing.Point(503, 319);
            this.button_register.Margin = new System.Windows.Forms.Padding(4);
            this.button_register.Name = "button_register";
            this.button_register.Size = new System.Drawing.Size(100, 29);
            this.button_register.TabIndex = 10;
            this.button_register.Text = "µÇ¼Ç";
            this.button_register.UseVisualStyleBackColor = true;
            this.button_register.Click += new System.EventHandler(this.button_register_Click);
            // 
            // RegisterBarcodeDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 401);
            this.Controls.Add(this.button_register);
            this.Controls.Add(this.textBox_itemBarcode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_target);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RegisterBarcodeDlg";
            this.Text = "µä²Ø²áµÇÂ¼";
            this.Load += new System.EventHandler(this.RegisterBarcodeDlg_Load);
            this.Activated += new System.EventHandler(this.RegisterBarcodeDlg_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RegisterBarcodeDlg_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RegisterBarcodeDlg_FormClosing);
            this.flowLayoutPanel_query.ResumeLayout(false);
            this.flowLayoutPanel_query.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.splitContainer_recordAndItems.Panel1.ResumeLayout(false);
            this.splitContainer_recordAndItems.Panel1.PerformLayout();
            this.splitContainer_recordAndItems.Panel2.ResumeLayout(false);
            this.splitContainer_recordAndItems.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_from;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_query;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.WebBrowser webBrowser_record;
        private System.Windows.Forms.SplitContainer splitContainer_recordAndItems;
        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Label label_target;
        private System.Windows.Forms.Button button_target;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_itemBarcode;
        private System.Windows.Forms.CheckBox checkBox_autoDetectQueryBarcode;
        private System.Windows.Forms.Button button_register;
        private System.Windows.Forms.Label label_biblioRecPath;
        private System.Windows.Forms.ListView listView_items;
        private System.Windows.Forms.ColumnHeader columnHeader_barcode;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ColumnHeader columnHeader_price;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ColumnHeader columnHeader_borrower;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowDate;
        private System.Windows.Forms.ColumnHeader columnHeader_borrowPeriod;
        private System.Windows.Forms.ColumnHeader columnHeader_recpath;
        private System.Windows.Forms.ColumnHeader columnHeader_bookType;
    }
}