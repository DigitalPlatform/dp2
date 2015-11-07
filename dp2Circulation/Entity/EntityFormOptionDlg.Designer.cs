namespace dp2Circulation
{
    partial class EntityFormOptionDlg
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
            this.DisposeFreeControls();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityFormOptionDlg));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_normalEntityRegisterDefault = new System.Windows.Forms.TabPage();
            this.entityEditControl_normalRegisterDefault = new dp2Circulation.EntityEditControl();
            this.tabPage_quickEntityRegisterDefault = new System.Windows.Forms.TabPage();
            this.entityEditControl_quickRegisterDefault = new dp2Circulation.EntityEditControl();
            this.tabPage_normalIssueRegisterDefault = new System.Windows.Forms.TabPage();
            this.issueEditControl_normalRegisterDefault = new dp2Circulation.IssueEditControl();
            this.tabPage_quickIssueRegisterDefault = new System.Windows.Forms.TabPage();
            this.issueEditControl_quickRegisterDefault = new dp2Circulation.IssueEditControl();
            this.tabPage_normalOrderRegisterDefault = new System.Windows.Forms.TabPage();
            this.orderEditControl_normalRegisterDefault = new dp2Circulation.OrderEditControl();
            this.tabPage_normalCommentRegisterDefault = new System.Windows.Forms.TabPage();
            this.commentEditControl1 = new dp2Circulation.CommentEditControl();
            this.tabPage_parameters = new System.Windows.Forms.TabPage();
            this.checkBox_verifyItemBarcode = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_normalRegister_simple = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel_normalRegister = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl_main.SuspendLayout();
            this.tabPage_normalEntityRegisterDefault.SuspendLayout();
            this.tabPage_quickEntityRegisterDefault.SuspendLayout();
            this.tabPage_normalIssueRegisterDefault.SuspendLayout();
            this.tabPage_quickIssueRegisterDefault.SuspendLayout();
            this.tabPage_normalOrderRegisterDefault.SuspendLayout();
            this.tabPage_normalCommentRegisterDefault.SuspendLayout();
            this.tabPage_parameters.SuspendLayout();
            this.tableLayoutPanel_normalRegister.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_normalEntityRegisterDefault);
            this.tabControl_main.Controls.Add(this.tabPage_quickEntityRegisterDefault);
            this.tabControl_main.Controls.Add(this.tabPage_normalIssueRegisterDefault);
            this.tabControl_main.Controls.Add(this.tabPage_quickIssueRegisterDefault);
            this.tabControl_main.Controls.Add(this.tabPage_normalOrderRegisterDefault);
            this.tabControl_main.Controls.Add(this.tabPage_normalCommentRegisterDefault);
            this.tabControl_main.Controls.Add(this.tabPage_parameters);
            this.tabControl_main.Location = new System.Drawing.Point(10, 11);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(341, 286);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_normalEntityRegisterDefault
            // 
            this.tabPage_normalEntityRegisterDefault.Controls.Add(this.tableLayoutPanel_normalRegister);
            this.tabPage_normalEntityRegisterDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normalEntityRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_normalEntityRegisterDefault.Name = "tabPage_normalEntityRegisterDefault";
            this.tabPage_normalEntityRegisterDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_normalEntityRegisterDefault.Size = new System.Drawing.Size(333, 260);
            this.tabPage_normalEntityRegisterDefault.TabIndex = 0;
            this.tabPage_normalEntityRegisterDefault.Text = " 一般册登记缺省值 ";
            this.tabPage_normalEntityRegisterDefault.UseVisualStyleBackColor = true;
            // 
            // entityEditControl_normalRegisterDefault
            // 
            this.entityEditControl_normalRegisterDefault.AccessNo = "";
            this.entityEditControl_normalRegisterDefault.AutoScroll = true;
            this.entityEditControl_normalRegisterDefault.Barcode = "";
            this.entityEditControl_normalRegisterDefault.BatchNo = "";
            this.entityEditControl_normalRegisterDefault.Binding = "";
            this.entityEditControl_normalRegisterDefault.BindingCost = "";
            this.entityEditControl_normalRegisterDefault.BookType = "";
            this.entityEditControl_normalRegisterDefault.BorrowDate = "";
            this.entityEditControl_normalRegisterDefault.Borrower = "";
            this.entityEditControl_normalRegisterDefault.BorrowPeriod = "";
            this.entityEditControl_normalRegisterDefault.Changed = false;
            this.entityEditControl_normalRegisterDefault.Comment = "";
            this.entityEditControl_normalRegisterDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl_normalRegisterDefault.Initializing = true;
            this.entityEditControl_normalRegisterDefault.Intact = "";
            this.entityEditControl_normalRegisterDefault.Location = new System.Drawing.Point(2, 2);
            this.entityEditControl_normalRegisterDefault.LocationString = "";
            this.entityEditControl_normalRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl_normalRegisterDefault.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl_normalRegisterDefault.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl_normalRegisterDefault.MergeComment = "";
            this.entityEditControl_normalRegisterDefault.MinimumSize = new System.Drawing.Size(56, 0);
            this.entityEditControl_normalRegisterDefault.Name = "entityEditControl_normalRegisterDefault";
            this.entityEditControl_normalRegisterDefault.Operations = "";
            this.entityEditControl_normalRegisterDefault.ParentId = "";
            this.entityEditControl_normalRegisterDefault.Price = "";
            this.entityEditControl_normalRegisterDefault.PublishTime = "";
            this.entityEditControl_normalRegisterDefault.RecPath = "";
            this.entityEditControl_normalRegisterDefault.RefID = "";
            this.entityEditControl_normalRegisterDefault.RegisterNo = "";
            this.entityEditControl_normalRegisterDefault.Seller = "";
            this.entityEditControl_normalRegisterDefault.Size = new System.Drawing.Size(325, 228);
            this.entityEditControl_normalRegisterDefault.Source = "";
            this.entityEditControl_normalRegisterDefault.State = "";
            this.entityEditControl_normalRegisterDefault.TabIndex = 0;
            this.entityEditControl_normalRegisterDefault.Volume = "";
            // 
            // tabPage_quickEntityRegisterDefault
            // 
            this.tabPage_quickEntityRegisterDefault.Controls.Add(this.entityEditControl_quickRegisterDefault);
            this.tabPage_quickEntityRegisterDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_quickEntityRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_quickEntityRegisterDefault.Name = "tabPage_quickEntityRegisterDefault";
            this.tabPage_quickEntityRegisterDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_quickEntityRegisterDefault.Size = new System.Drawing.Size(333, 260);
            this.tabPage_quickEntityRegisterDefault.TabIndex = 1;
            this.tabPage_quickEntityRegisterDefault.Text = "快速册登记缺省值";
            this.tabPage_quickEntityRegisterDefault.UseVisualStyleBackColor = true;
            // 
            // entityEditControl_quickRegisterDefault
            // 
            this.entityEditControl_quickRegisterDefault.AccessNo = "";
            this.entityEditControl_quickRegisterDefault.AutoScroll = true;
            this.entityEditControl_quickRegisterDefault.Barcode = "";
            this.entityEditControl_quickRegisterDefault.BatchNo = "";
            this.entityEditControl_quickRegisterDefault.Binding = "";
            this.entityEditControl_quickRegisterDefault.BindingCost = "";
            this.entityEditControl_quickRegisterDefault.BookType = "";
            this.entityEditControl_quickRegisterDefault.BorrowDate = "";
            this.entityEditControl_quickRegisterDefault.Borrower = "";
            this.entityEditControl_quickRegisterDefault.BorrowPeriod = "";
            this.entityEditControl_quickRegisterDefault.Changed = false;
            this.entityEditControl_quickRegisterDefault.Comment = "";
            this.entityEditControl_quickRegisterDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl_quickRegisterDefault.Initializing = true;
            this.entityEditControl_quickRegisterDefault.Intact = "";
            this.entityEditControl_quickRegisterDefault.Location = new System.Drawing.Point(2, 2);
            this.entityEditControl_quickRegisterDefault.LocationString = "";
            this.entityEditControl_quickRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl_quickRegisterDefault.MemberBackColor = System.Drawing.Color.WhiteSmoke;
            this.entityEditControl_quickRegisterDefault.MemberForeColor = System.Drawing.SystemColors.ControlText;
            this.entityEditControl_quickRegisterDefault.MergeComment = "";
            this.entityEditControl_quickRegisterDefault.MinimumSize = new System.Drawing.Size(56, 0);
            this.entityEditControl_quickRegisterDefault.Name = "entityEditControl_quickRegisterDefault";
            this.entityEditControl_quickRegisterDefault.Operations = "";
            this.entityEditControl_quickRegisterDefault.ParentId = "";
            this.entityEditControl_quickRegisterDefault.Price = "";
            this.entityEditControl_quickRegisterDefault.PublishTime = "";
            this.entityEditControl_quickRegisterDefault.RecPath = "";
            this.entityEditControl_quickRegisterDefault.RefID = "";
            this.entityEditControl_quickRegisterDefault.RegisterNo = "";
            this.entityEditControl_quickRegisterDefault.Seller = "";
            this.entityEditControl_quickRegisterDefault.Size = new System.Drawing.Size(329, 256);
            this.entityEditControl_quickRegisterDefault.Source = "";
            this.entityEditControl_quickRegisterDefault.State = "";
            this.entityEditControl_quickRegisterDefault.TabIndex = 1;
            this.entityEditControl_quickRegisterDefault.Volume = "";
            // 
            // tabPage_normalIssueRegisterDefault
            // 
            this.tabPage_normalIssueRegisterDefault.Controls.Add(this.issueEditControl_normalRegisterDefault);
            this.tabPage_normalIssueRegisterDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normalIssueRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_normalIssueRegisterDefault.Name = "tabPage_normalIssueRegisterDefault";
            this.tabPage_normalIssueRegisterDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_normalIssueRegisterDefault.Size = new System.Drawing.Size(333, 260);
            this.tabPage_normalIssueRegisterDefault.TabIndex = 3;
            this.tabPage_normalIssueRegisterDefault.Text = " 一般期登记缺省值 ";
            this.tabPage_normalIssueRegisterDefault.UseVisualStyleBackColor = true;
            // 
            // issueEditControl_normalRegisterDefault
            // 
            this.issueEditControl_normalRegisterDefault.BatchNo = "";
            this.issueEditControl_normalRegisterDefault.Changed = false;
            this.issueEditControl_normalRegisterDefault.Comment = "";
            this.issueEditControl_normalRegisterDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.issueEditControl_normalRegisterDefault.Initializing = true;
            this.issueEditControl_normalRegisterDefault.Issue = "";
            this.issueEditControl_normalRegisterDefault.Location = new System.Drawing.Point(2, 2);
            this.issueEditControl_normalRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.issueEditControl_normalRegisterDefault.Name = "issueEditControl_normalRegisterDefault";
            this.issueEditControl_normalRegisterDefault.Operations = "";
            this.issueEditControl_normalRegisterDefault.OrderInfo = "";
            this.issueEditControl_normalRegisterDefault.ParentId = "";
            this.issueEditControl_normalRegisterDefault.PublishTime = "";
            this.issueEditControl_normalRegisterDefault.RecPath = "";
            this.issueEditControl_normalRegisterDefault.RefID = "";
            this.issueEditControl_normalRegisterDefault.Size = new System.Drawing.Size(329, 256);
            this.issueEditControl_normalRegisterDefault.State = "";
            this.issueEditControl_normalRegisterDefault.TabIndex = 0;
            this.issueEditControl_normalRegisterDefault.Volume = "";
            this.issueEditControl_normalRegisterDefault.Zong = "";
            // 
            // tabPage_quickIssueRegisterDefault
            // 
            this.tabPage_quickIssueRegisterDefault.Controls.Add(this.issueEditControl_quickRegisterDefault);
            this.tabPage_quickIssueRegisterDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_quickIssueRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_quickIssueRegisterDefault.Name = "tabPage_quickIssueRegisterDefault";
            this.tabPage_quickIssueRegisterDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_quickIssueRegisterDefault.Size = new System.Drawing.Size(333, 260);
            this.tabPage_quickIssueRegisterDefault.TabIndex = 4;
            this.tabPage_quickIssueRegisterDefault.Text = "快速期登记缺省值";
            this.tabPage_quickIssueRegisterDefault.UseVisualStyleBackColor = true;
            // 
            // issueEditControl_quickRegisterDefault
            // 
            this.issueEditControl_quickRegisterDefault.BatchNo = "";
            this.issueEditControl_quickRegisterDefault.Changed = false;
            this.issueEditControl_quickRegisterDefault.Comment = "";
            this.issueEditControl_quickRegisterDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.issueEditControl_quickRegisterDefault.Initializing = true;
            this.issueEditControl_quickRegisterDefault.Issue = "";
            this.issueEditControl_quickRegisterDefault.Location = new System.Drawing.Point(2, 2);
            this.issueEditControl_quickRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.issueEditControl_quickRegisterDefault.Name = "issueEditControl_quickRegisterDefault";
            this.issueEditControl_quickRegisterDefault.Operations = "";
            this.issueEditControl_quickRegisterDefault.OrderInfo = "";
            this.issueEditControl_quickRegisterDefault.ParentId = "";
            this.issueEditControl_quickRegisterDefault.PublishTime = "";
            this.issueEditControl_quickRegisterDefault.RecPath = "";
            this.issueEditControl_quickRegisterDefault.RefID = "";
            this.issueEditControl_quickRegisterDefault.Size = new System.Drawing.Size(329, 256);
            this.issueEditControl_quickRegisterDefault.State = "";
            this.issueEditControl_quickRegisterDefault.TabIndex = 0;
            this.issueEditControl_quickRegisterDefault.Volume = "";
            this.issueEditControl_quickRegisterDefault.Zong = "";
            // 
            // tabPage_normalOrderRegisterDefault
            // 
            this.tabPage_normalOrderRegisterDefault.Controls.Add(this.orderEditControl_normalRegisterDefault);
            this.tabPage_normalOrderRegisterDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normalOrderRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_normalOrderRegisterDefault.Name = "tabPage_normalOrderRegisterDefault";
            this.tabPage_normalOrderRegisterDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_normalOrderRegisterDefault.Size = new System.Drawing.Size(333, 260);
            this.tabPage_normalOrderRegisterDefault.TabIndex = 5;
            this.tabPage_normalOrderRegisterDefault.Text = " 一般订购缺省值 ";
            this.tabPage_normalOrderRegisterDefault.UseVisualStyleBackColor = true;
            // 
            // orderEditControl_normalRegisterDefault
            // 
            this.orderEditControl_normalRegisterDefault.BatchNo = "";
            this.orderEditControl_normalRegisterDefault.CatalogNo = "";
            this.orderEditControl_normalRegisterDefault.Changed = false;
            this.orderEditControl_normalRegisterDefault.Class = "";
            this.orderEditControl_normalRegisterDefault.Comment = "";
            this.orderEditControl_normalRegisterDefault.Copy = "";
            this.orderEditControl_normalRegisterDefault.Distribute = "";
            this.orderEditControl_normalRegisterDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.orderEditControl_normalRegisterDefault.Index = "";
            this.orderEditControl_normalRegisterDefault.Initializing = true;
            this.orderEditControl_normalRegisterDefault.IssueCount = "";
            this.orderEditControl_normalRegisterDefault.Location = new System.Drawing.Point(2, 2);
            this.orderEditControl_normalRegisterDefault.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.orderEditControl_normalRegisterDefault.MinimumSize = new System.Drawing.Size(75, 0);
            this.orderEditControl_normalRegisterDefault.Name = "orderEditControl_normalRegisterDefault";
            this.orderEditControl_normalRegisterDefault.Operations = "";
            this.orderEditControl_normalRegisterDefault.OrderID = "";
            this.orderEditControl_normalRegisterDefault.OrderTime = "";
            this.orderEditControl_normalRegisterDefault.ParentId = "";
            this.orderEditControl_normalRegisterDefault.Price = "";
            this.orderEditControl_normalRegisterDefault.Range = "";
            this.orderEditControl_normalRegisterDefault.RecPath = "";
            this.orderEditControl_normalRegisterDefault.RefID = "";
            this.orderEditControl_normalRegisterDefault.Seller = "";
            this.orderEditControl_normalRegisterDefault.SellerAddress = "";
            this.orderEditControl_normalRegisterDefault.Size = new System.Drawing.Size(329, 256);
            this.orderEditControl_normalRegisterDefault.Source = "";
            this.orderEditControl_normalRegisterDefault.State = "";
            this.orderEditControl_normalRegisterDefault.TabIndex = 0;
            this.orderEditControl_normalRegisterDefault.TotalPrice = "";
            // 
            // tabPage_normalCommentRegisterDefault
            // 
            this.tabPage_normalCommentRegisterDefault.Controls.Add(this.commentEditControl1);
            this.tabPage_normalCommentRegisterDefault.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normalCommentRegisterDefault.Name = "tabPage_normalCommentRegisterDefault";
            this.tabPage_normalCommentRegisterDefault.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_normalCommentRegisterDefault.Size = new System.Drawing.Size(333, 260);
            this.tabPage_normalCommentRegisterDefault.TabIndex = 6;
            this.tabPage_normalCommentRegisterDefault.Text = " 一般评注缺省值 ";
            this.tabPage_normalCommentRegisterDefault.UseVisualStyleBackColor = true;
            // 
            // commentEditControl1
            // 
            this.commentEditControl1.Changed = false;
            this.commentEditControl1.Content = "";
            this.commentEditControl1.CreateTime = "";
            this.commentEditControl1.Creator = "";
            this.commentEditControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commentEditControl1.Index = "";
            this.commentEditControl1.Initializing = true;
            this.commentEditControl1.LastModified = "";
            this.commentEditControl1.Location = new System.Drawing.Point(2, 2);
            this.commentEditControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.commentEditControl1.Name = "commentEditControl1";
            this.commentEditControl1.Operations = "";
            this.commentEditControl1.OrderSuggestion = "";
            this.commentEditControl1.ParentId = "";
            this.commentEditControl1.RecPath = "";
            this.commentEditControl1.RefID = "";
            this.commentEditControl1.Size = new System.Drawing.Size(329, 256);
            this.commentEditControl1.State = "";
            this.commentEditControl1.Subject = "";
            this.commentEditControl1.Summary = "";
            this.commentEditControl1.TabIndex = 0;
            this.commentEditControl1.Title = "";
            this.commentEditControl1.TypeString = "";
            // 
            // tabPage_parameters
            // 
            this.tabPage_parameters.Controls.Add(this.checkBox_verifyItemBarcode);
            this.tabPage_parameters.Location = new System.Drawing.Point(4, 22);
            this.tabPage_parameters.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_parameters.Name = "tabPage_parameters";
            this.tabPage_parameters.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_parameters.Size = new System.Drawing.Size(333, 260);
            this.tabPage_parameters.TabIndex = 2;
            this.tabPage_parameters.Text = "配置参数";
            this.tabPage_parameters.UseVisualStyleBackColor = true;
            // 
            // checkBox_verifyItemBarcode
            // 
            this.checkBox_verifyItemBarcode.AutoSize = true;
            this.checkBox_verifyItemBarcode.Location = new System.Drawing.Point(4, 19);
            this.checkBox_verifyItemBarcode.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_verifyItemBarcode.Name = "checkBox_verifyItemBarcode";
            this.checkBox_verifyItemBarcode.Size = new System.Drawing.Size(150, 16);
            this.checkBox_verifyItemBarcode.TabIndex = 5;
            this.checkBox_verifyItemBarcode.Text = "校验输入的册条码号(&V)";
            this.checkBox_verifyItemBarcode.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(234, 302);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(295, 302);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_quickRegister_simple
            // 
            this.checkBox_normalRegister_simple.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_normalRegister_simple.AutoSize = true;
            this.checkBox_normalRegister_simple.Location = new System.Drawing.Point(3, 237);
            this.checkBox_normalRegister_simple.Name = "checkBox_quickRegister_simple";
            this.checkBox_normalRegister_simple.Size = new System.Drawing.Size(90, 16);
            this.checkBox_normalRegister_simple.TabIndex = 3;
            this.checkBox_normalRegister_simple.Text = "简单模板(&S)";
            this.checkBox_normalRegister_simple.UseVisualStyleBackColor = true;
            this.checkBox_normalRegister_simple.CheckedChanged += new System.EventHandler(this.checkBox_normalRegister_simple_CheckedChanged);
            // 
            // tableLayoutPanel_normalRegister
            // 
            this.tableLayoutPanel_normalRegister.ColumnCount = 1;
            this.tableLayoutPanel_normalRegister.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_normalRegister.Controls.Add(this.checkBox_normalRegister_simple, 0, 1);
            this.tableLayoutPanel_normalRegister.Controls.Add(this.entityEditControl_normalRegisterDefault, 0, 0);
            this.tableLayoutPanel_normalRegister.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_normalRegister.Location = new System.Drawing.Point(2, 2);
            this.tableLayoutPanel_normalRegister.Name = "tableLayoutPanel_normalRegister";
            this.tableLayoutPanel_normalRegister.RowCount = 2;
            this.tableLayoutPanel_normalRegister.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.82569F));
            this.tableLayoutPanel_normalRegister.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.174312F));
            this.tableLayoutPanel_normalRegister.Size = new System.Drawing.Size(329, 256);
            this.tableLayoutPanel_normalRegister.TabIndex = 4;
            // 
            // EntityFormOptionDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(360, 330);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EntityFormOptionDlg";
            this.ShowInTaskbar = false;
            this.Text = "选项";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EntityFormOptionDlg_FormClosed);
            this.Load += new System.EventHandler(this.EntityFormOptionDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_normalEntityRegisterDefault.ResumeLayout(false);
            this.tabPage_quickEntityRegisterDefault.ResumeLayout(false);
            this.tabPage_normalIssueRegisterDefault.ResumeLayout(false);
            this.tabPage_quickIssueRegisterDefault.ResumeLayout(false);
            this.tabPage_normalOrderRegisterDefault.ResumeLayout(false);
            this.tabPage_normalCommentRegisterDefault.ResumeLayout(false);
            this.tabPage_parameters.ResumeLayout(false);
            this.tabPage_parameters.PerformLayout();
            this.tableLayoutPanel_normalRegister.ResumeLayout(false);
            this.tableLayoutPanel_normalRegister.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_normalEntityRegisterDefault;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private EntityEditControl entityEditControl_normalRegisterDefault;
        private System.Windows.Forms.TabPage tabPage_quickEntityRegisterDefault;
        private EntityEditControl entityEditControl_quickRegisterDefault;
        private System.Windows.Forms.TabPage tabPage_parameters;
        private System.Windows.Forms.CheckBox checkBox_verifyItemBarcode;
        private System.Windows.Forms.TabPage tabPage_normalIssueRegisterDefault;
        private System.Windows.Forms.TabPage tabPage_quickIssueRegisterDefault;
        private IssueEditControl issueEditControl_normalRegisterDefault;
        private IssueEditControl issueEditControl_quickRegisterDefault;
        private System.Windows.Forms.TabPage tabPage_normalOrderRegisterDefault;
        private OrderEditControl orderEditControl_normalRegisterDefault;
        private System.Windows.Forms.TabPage tabPage_normalCommentRegisterDefault;
        private CommentEditControl commentEditControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_normalRegister;
        private System.Windows.Forms.CheckBox checkBox_normalRegister_simple;

    }
}