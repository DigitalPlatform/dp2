namespace dp2ManageCenter.Message
{
    partial class ShelfSearchForm
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
            this.panel_query = new System.Windows.Forms.Panel();
            this.comboBox_query_matchStyle = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_query_from = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_query_word = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_query_shelfAccount = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_query_myAccount = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_search = new System.Windows.Forms.Button();
            this.listView_records = new dp2ManageCenter.Message.DoubleBufferdListView();
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_action = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_operTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_syncCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_syncOperTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_batchNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_toShelfNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_toLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_transferDirection = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel_query.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_query
            // 
            this.panel_query.Controls.Add(this.comboBox_query_matchStyle);
            this.panel_query.Controls.Add(this.label5);
            this.panel_query.Controls.Add(this.comboBox_query_from);
            this.panel_query.Controls.Add(this.label4);
            this.panel_query.Controls.Add(this.textBox_query_word);
            this.panel_query.Controls.Add(this.label3);
            this.panel_query.Controls.Add(this.comboBox_query_shelfAccount);
            this.panel_query.Controls.Add(this.label2);
            this.panel_query.Controls.Add(this.comboBox_query_myAccount);
            this.panel_query.Controls.Add(this.label1);
            this.panel_query.Location = new System.Drawing.Point(13, 12);
            this.panel_query.Name = "panel_query";
            this.panel_query.Size = new System.Drawing.Size(713, 202);
            this.panel_query.TabIndex = 0;
            // 
            // comboBox_query_matchStyle
            // 
            this.comboBox_query_matchStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_query_matchStyle.FormattingEnabled = true;
            this.comboBox_query_matchStyle.Items.AddRange(new object[] {
            "left",
            "middle",
            "right",
            "exact"});
            this.comboBox_query_matchStyle.Location = new System.Drawing.Point(132, 158);
            this.comboBox_query_matchStyle.Name = "comboBox_query_matchStyle";
            this.comboBox_query_matchStyle.Size = new System.Drawing.Size(552, 29);
            this.comboBox_query_matchStyle.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(18, 161);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 21);
            this.label5.TabIndex = 8;
            this.label5.Text = "匹配方式";
            // 
            // comboBox_query_from
            // 
            this.comboBox_query_from.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_query_from.FormattingEnabled = true;
            this.comboBox_query_from.Items.AddRange(new object[] {
            "ID",
            "Action",
            "State",
            "Operator-",
            "PII",
            "ItemRecPath-",
            "OperTime",
            "BatchNo",
            "CurrentShelfNo",
            "Location",
            "TransferDirection",
            "SyncErrorCode",
            "SyncErrorInfo",
            "SyncCount"});
            this.comboBox_query_from.Location = new System.Drawing.Point(132, 123);
            this.comboBox_query_from.Name = "comboBox_query_from";
            this.comboBox_query_from.Size = new System.Drawing.Size(552, 29);
            this.comboBox_query_from.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 21);
            this.label4.TabIndex = 6;
            this.label4.Text = "检索途径";
            // 
            // textBox_query_word
            // 
            this.textBox_query_word.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_query_word.Location = new System.Drawing.Point(132, 86);
            this.textBox_query_word.Name = "textBox_query_word";
            this.textBox_query_word.Size = new System.Drawing.Size(552, 31);
            this.textBox_query_word.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "检索词";
            // 
            // comboBox_query_shelfAccount
            // 
            this.comboBox_query_shelfAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_query_shelfAccount.FormattingEnabled = true;
            this.comboBox_query_shelfAccount.Location = new System.Drawing.Point(132, 50);
            this.comboBox_query_shelfAccount.Name = "comboBox_query_shelfAccount";
            this.comboBox_query_shelfAccount.Size = new System.Drawing.Size(552, 29);
            this.comboBox_query_shelfAccount.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "书柜";
            // 
            // comboBox_query_myAccount
            // 
            this.comboBox_query_myAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_query_myAccount.FormattingEnabled = true;
            this.comboBox_query_myAccount.Location = new System.Drawing.Point(132, 15);
            this.comboBox_query_myAccount.Name = "comboBox_query_myAccount";
            this.comboBox_query_myAccount.Size = new System.Drawing.Size(552, 29);
            this.comboBox_query_myAccount.TabIndex = 1;
            this.comboBox_query_myAccount.DropDown += new System.EventHandler(this.comboBox_query_myAccount_DropDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "账户名";
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(732, 163);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(164, 41);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "检索";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // listView_records
            // 
            this.listView_records.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_id,
            this.columnHeader_action,
            this.columnHeader_pii,
            this.columnHeader_operTime,
            this.columnHeader_state,
            this.columnHeader_errorCode,
            this.columnHeader_errorInfo,
            this.columnHeader_syncCount,
            this.columnHeader_syncOperTime,
            this.columnHeader_batchNo,
            this.columnHeader_toShelfNo,
            this.columnHeader_toLocation,
            this.columnHeader_transferDirection});
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(13, 221);
            this.listView_records.Name = "listView_records";
            this.listView_records.ShowItemToolTips = true;
            this.listView_records.Size = new System.Drawing.Size(883, 326);
            this.listView_records.TabIndex = 1;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            this.listView_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_records_MouseUp);
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "ID";
            this.columnHeader_id.Width = 105;
            // 
            // columnHeader_action
            // 
            this.columnHeader_action.Text = "动作";
            this.columnHeader_action.Width = 130;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 186;
            // 
            // columnHeader_operTime
            // 
            this.columnHeader_operTime.Text = "操作时间";
            this.columnHeader_operTime.Width = 216;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 118;
            // 
            // columnHeader_errorCode
            // 
            this.columnHeader_errorCode.Text = "错误码";
            this.columnHeader_errorCode.Width = 122;
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "错误信息";
            this.columnHeader_errorInfo.Width = 293;
            // 
            // columnHeader_syncCount
            // 
            this.columnHeader_syncCount.Text = "同步次数";
            this.columnHeader_syncCount.Width = 100;
            // 
            // columnHeader_syncOperTime
            // 
            this.columnHeader_syncOperTime.Text = "最后同步时间";
            this.columnHeader_syncOperTime.Width = 180;
            // 
            // columnHeader_batchNo
            // 
            this.columnHeader_batchNo.Text = "批次号";
            this.columnHeader_batchNo.Width = 180;
            // 
            // columnHeader_toShelfNo
            // 
            this.columnHeader_toShelfNo.Text = "去向最新架位";
            this.columnHeader_toShelfNo.Width = 180;
            // 
            // columnHeader_toLocation
            // 
            this.columnHeader_toLocation.Text = "去向永久架位";
            this.columnHeader_toLocation.Width = 180;
            // 
            // columnHeader_transferDirection
            // 
            this.columnHeader_transferDirection.Text = "典藏移交方向";
            this.columnHeader_transferDirection.Width = 180;
            // 
            // ShelfSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 559);
            this.Controls.Add(this.button_search);
            this.Controls.Add(this.listView_records);
            this.Controls.Add(this.panel_query);
            this.Name = "ShelfSearchForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "检索书柜";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShelfSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShelfSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.ShelfSearchForm_Load);
            this.panel_query.ResumeLayout(false);
            this.panel_query.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_query;
        private System.Windows.Forms.ComboBox comboBox_query_shelfAccount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_query_myAccount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_query_word;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_query_from;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_query_matchStyle;
        private System.Windows.Forms.Label label5;
        private DoubleBufferdListView listView_records;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_action;
        private System.Windows.Forms.ColumnHeader columnHeader_operTime;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_errorCode;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.ColumnHeader columnHeader_syncCount;
        private System.Windows.Forms.ColumnHeader columnHeader_syncOperTime;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.ColumnHeader columnHeader_toShelfNo;
        private System.Windows.Forms.ColumnHeader columnHeader_toLocation;
        private System.Windows.Forms.ColumnHeader columnHeader_transferDirection;
    }
}