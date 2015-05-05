namespace dp2Circulation
{
    partial class MessageForm
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
            this.listView_message = new System.Windows.Forms.ListView();
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_sender = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_recipient = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_subject = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_date = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_size = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.comboBox_box = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_message
            // 
            this.listView_message.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_id,
            this.columnHeader_sender,
            this.columnHeader_recipient,
            this.columnHeader_subject,
            this.columnHeader_date,
            this.columnHeader_size});
            this.listView_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_message.FullRowSelect = true;
            this.listView_message.HideSelection = false;
            this.listView_message.Location = new System.Drawing.Point(3, 32);
            this.listView_message.Name = "listView_message";
            this.listView_message.Size = new System.Drawing.Size(429, 229);
            this.listView_message.TabIndex = 0;
            this.listView_message.UseCompatibleStateImageBehavior = false;
            this.listView_message.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "ID";
            // 
            // columnHeader_sender
            // 
            this.columnHeader_sender.Text = "发件人";
            this.columnHeader_sender.Width = 100;
            // 
            // columnHeader_recipient
            // 
            this.columnHeader_recipient.Text = "收件人";
            this.columnHeader_recipient.Width = 100;
            // 
            // columnHeader_subject
            // 
            this.columnHeader_subject.Text = "主题";
            this.columnHeader_subject.Width = 200;
            // 
            // columnHeader_date
            // 
            this.columnHeader_date.Text = "日期";
            this.columnHeader_date.Width = 130;
            // 
            // columnHeader_size
            // 
            this.columnHeader_size.Text = "尺寸";
            this.columnHeader_size.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.listView_message, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(435, 264);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.comboBox_box);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(241, 23);
            this.panel1.TabIndex = 1;
            // 
            // comboBox_box
            // 
            this.comboBox_box.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_box.FormattingEnabled = true;
            this.comboBox_box.Items.AddRange(new object[] {
            "收件箱",
            "草稿",
            "已发送",
            "废件箱"});
            this.comboBox_box.Location = new System.Drawing.Point(63, 0);
            this.comboBox_box.Name = "comboBox_box";
            this.comboBox_box.Size = new System.Drawing.Size(173, 20);
            this.comboBox_box.TabIndex = 1;
            this.comboBox_box.SelectedIndexChanged += new System.EventHandler(this.comboBox_box_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "信箱(&B):";
            // 
            // MessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 264);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MessageForm";
            this.Text = "消息";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MessageForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MessageForm_FormClosed);
            this.Load += new System.EventHandler(this.MessageForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_message;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_sender;
        private System.Windows.Forms.ColumnHeader columnHeader_recipient;
        private System.Windows.Forms.ColumnHeader columnHeader_subject;
        private System.Windows.Forms.ColumnHeader columnHeader_date;
        private System.Windows.Forms.ColumnHeader columnHeader_size;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_box;
    }
}