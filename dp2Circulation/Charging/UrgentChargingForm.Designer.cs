namespace dp2Circulation
{
    partial class UrgentChargingForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UrgentChargingForm));
            this.tableLayoutPanel_biblioAndItem = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_operation = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_verifyReaderPassword = new System.Windows.Forms.Button();
            this.button_itemAction = new System.Windows.Forms.Button();
            this.contextMenuStrip_selectFunc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_borrow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_return = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_verifyReturn = new System.Windows.Forms.ToolStripMenuItem();
            this.textBox_itemBarcode = new System.Windows.Forms.TextBox();
            this.textBox_readerPassword = new System.Windows.Forms.TextBox();
            this.button_loadReader = new System.Windows.Forms.Button();
            this.textBox_readerBarcode = new System.Windows.Forms.TextBox();
            this.splitContainer_biblioAndItem = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_biblioInfo = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_operationInfo = new System.Windows.Forms.WebBrowser();
            this.label5 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_itemInfo = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser_otherInfo = new System.Windows.Forms.WebBrowser();
            this.label6 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_biblioAndItem.SuspendLayout();
            this.tableLayoutPanel_operation.SuspendLayout();
            this.contextMenuStrip_selectFunc.SuspendLayout();
            this.splitContainer_biblioAndItem.Panel1.SuspendLayout();
            this.splitContainer_biblioAndItem.Panel2.SuspendLayout();
            this.splitContainer_biblioAndItem.SuspendLayout();
            this.tableLayoutPanel_biblioInfo.SuspendLayout();
            this.tableLayoutPanel_itemInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_biblioAndItem
            // 
            this.tableLayoutPanel_biblioAndItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_biblioAndItem.ColumnCount = 1;
            this.tableLayoutPanel_biblioAndItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_biblioAndItem.Controls.Add(this.tableLayoutPanel_operation, 0, 1);
            this.tableLayoutPanel_biblioAndItem.Controls.Add(this.splitContainer_biblioAndItem, 0, 0);
            this.tableLayoutPanel_biblioAndItem.Location = new System.Drawing.Point(0, 9);
            this.tableLayoutPanel_biblioAndItem.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_biblioAndItem.Name = "tableLayoutPanel_biblioAndItem";
            this.tableLayoutPanel_biblioAndItem.RowCount = 3;
            this.tableLayoutPanel_biblioAndItem.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblioAndItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioAndItem.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioAndItem.Size = new System.Drawing.Size(723, 397);
            this.tableLayoutPanel_biblioAndItem.TabIndex = 11;
            // 
            // tableLayoutPanel_operation
            // 
            this.tableLayoutPanel_operation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_operation.AutoScroll = true;
            this.tableLayoutPanel_operation.AutoSize = true;
            this.tableLayoutPanel_operation.ColumnCount = 4;
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operation.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_operation.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel_operation.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel_operation.Controls.Add(this.button_verifyReaderPassword, 2, 1);
            this.tableLayoutPanel_operation.Controls.Add(this.button_itemAction, 2, 2);
            this.tableLayoutPanel_operation.Controls.Add(this.textBox_itemBarcode, 1, 2);
            this.tableLayoutPanel_operation.Controls.Add(this.textBox_readerPassword, 1, 1);
            this.tableLayoutPanel_operation.Controls.Add(this.button_loadReader, 2, 0);
            this.tableLayoutPanel_operation.Controls.Add(this.textBox_readerBarcode, 1, 0);
            this.tableLayoutPanel_operation.Location = new System.Drawing.Point(0, 292);
            this.tableLayoutPanel_operation.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_operation.MaximumSize = new System.Drawing.Size(400, 0);
            this.tableLayoutPanel_operation.Name = "tableLayoutPanel_operation";
            this.tableLayoutPanel_operation.RowCount = 4;
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operation.Size = new System.Drawing.Size(400, 105);
            this.tableLayoutPanel_operation.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(4, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 35);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号(&R)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(4, 35);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 35);
            this.label3.TabIndex = 3;
            this.label3.Text = "读者密码(&P)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(4, 70);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 35);
            this.label2.TabIndex = 6;
            this.label2.Text = "册条码号(&I)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button_verifyReaderPassword
            // 
            this.button_verifyReaderPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_verifyReaderPassword.AutoSize = true;
            this.button_verifyReaderPassword.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_verifyReaderPassword.Enabled = false;
            this.button_verifyReaderPassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_verifyReaderPassword.Image = ((System.Drawing.Image)(resources.GetObject("button_verifyReaderPassword.Image")));
            this.button_verifyReaderPassword.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button_verifyReaderPassword.Location = new System.Drawing.Point(307, 39);
            this.button_verifyReaderPassword.Margin = new System.Windows.Forms.Padding(4);
            this.button_verifyReaderPassword.Name = "button_verifyReaderPassword";
            this.button_verifyReaderPassword.Size = new System.Drawing.Size(89, 27);
            this.button_verifyReaderPassword.TabIndex = 5;
            this.button_verifyReaderPassword.Text = "验证(&V)";
            this.button_verifyReaderPassword.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_verifyReaderPassword.UseVisualStyleBackColor = true;
            // 
            // button_itemAction
            // 
            this.button_itemAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_itemAction.AutoSize = true;
            this.button_itemAction.ContextMenuStrip = this.contextMenuStrip_selectFunc;
            this.button_itemAction.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_itemAction.Image = ((System.Drawing.Image)(resources.GetObject("button_itemAction.Image")));
            this.button_itemAction.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button_itemAction.Location = new System.Drawing.Point(307, 74);
            this.button_itemAction.Margin = new System.Windows.Forms.Padding(4);
            this.button_itemAction.Name = "button_itemAction";
            this.button_itemAction.Size = new System.Drawing.Size(89, 27);
            this.button_itemAction.TabIndex = 8;
            this.button_itemAction.Text = "执行(&E)";
            this.button_itemAction.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_itemAction.UseVisualStyleBackColor = true;
            this.button_itemAction.Click += new System.EventHandler(this.button_itemAction_Click);
            // 
            // contextMenuStrip_selectFunc
            // 
            this.contextMenuStrip_selectFunc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_borrow,
            this.toolStripMenuItem_return,
            this.toolStripMenuItem_verifyReturn});
            this.contextMenuStrip_selectFunc.Name = "contextMenuStrip_selectFunc";
            this.contextMenuStrip_selectFunc.Size = new System.Drawing.Size(122, 70);
            // 
            // toolStripMenuItem_borrow
            // 
            this.toolStripMenuItem_borrow.Name = "toolStripMenuItem_borrow";
            this.toolStripMenuItem_borrow.Size = new System.Drawing.Size(121, 22);
            this.toolStripMenuItem_borrow.Text = "借";
            this.toolStripMenuItem_borrow.Click += new System.EventHandler(this.toolStripMenuItem_borrow_Click);
            // 
            // toolStripMenuItem_return
            // 
            this.toolStripMenuItem_return.Name = "toolStripMenuItem_return";
            this.toolStripMenuItem_return.Size = new System.Drawing.Size(121, 22);
            this.toolStripMenuItem_return.Text = "还";
            this.toolStripMenuItem_return.Click += new System.EventHandler(this.toolStripMenuItem_return_Click);
            // 
            // toolStripMenuItem_verifyReturn
            // 
            this.toolStripMenuItem_verifyReturn.Name = "toolStripMenuItem_verifyReturn";
            this.toolStripMenuItem_verifyReturn.Size = new System.Drawing.Size(121, 22);
            this.toolStripMenuItem_verifyReturn.Text = "验证还";
            this.toolStripMenuItem_verifyReturn.Click += new System.EventHandler(this.toolStripMenuItem_verifyReturn_Click);
            // 
            // textBox_itemBarcode
            // 
            this.textBox_itemBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_itemBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_itemBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_itemBarcode.Location = new System.Drawing.Point(118, 74);
            this.textBox_itemBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_itemBarcode.MinimumSize = new System.Drawing.Size(100, 4);
            this.textBox_itemBarcode.Name = "textBox_itemBarcode";
            this.textBox_itemBarcode.Size = new System.Drawing.Size(181, 25);
            this.textBox_itemBarcode.TabIndex = 7;
            this.textBox_itemBarcode.Enter += new System.EventHandler(this.textBox_itemBarcode_Enter);
            // 
            // textBox_readerPassword
            // 
            this.textBox_readerPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_readerPassword.Enabled = false;
            this.textBox_readerPassword.Location = new System.Drawing.Point(118, 39);
            this.textBox_readerPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_readerPassword.MinimumSize = new System.Drawing.Size(100, 4);
            this.textBox_readerPassword.Name = "textBox_readerPassword";
            this.textBox_readerPassword.PasswordChar = '*';
            this.textBox_readerPassword.Size = new System.Drawing.Size(181, 25);
            this.textBox_readerPassword.TabIndex = 4;
            this.textBox_readerPassword.Enter += new System.EventHandler(this.textBox_readerPassword_Enter);
            // 
            // button_loadReader
            // 
            this.button_loadReader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_loadReader.AutoSize = true;
            this.button_loadReader.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_loadReader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_loadReader.Image = ((System.Drawing.Image)(resources.GetObject("button_loadReader.Image")));
            this.button_loadReader.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button_loadReader.Location = new System.Drawing.Point(307, 4);
            this.button_loadReader.Margin = new System.Windows.Forms.Padding(4);
            this.button_loadReader.Name = "button_loadReader";
            this.button_loadReader.Size = new System.Drawing.Size(89, 27);
            this.button_loadReader.TabIndex = 2;
            this.button_loadReader.Text = "装载(&L)";
            this.button_loadReader.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_loadReader.UseVisualStyleBackColor = true;
            this.button_loadReader.Click += new System.EventHandler(this.button_loadReader_Click);
            // 
            // textBox_readerBarcode
            // 
            this.textBox_readerBarcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_readerBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_readerBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.textBox_readerBarcode.Location = new System.Drawing.Point(118, 4);
            this.textBox_readerBarcode.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_readerBarcode.MinimumSize = new System.Drawing.Size(100, 4);
            this.textBox_readerBarcode.Name = "textBox_readerBarcode";
            this.textBox_readerBarcode.Size = new System.Drawing.Size(181, 25);
            this.textBox_readerBarcode.TabIndex = 1;
            this.textBox_readerBarcode.Enter += new System.EventHandler(this.textBox_readerBarcode_Enter);
            // 
            // splitContainer_biblioAndItem
            // 
            this.splitContainer_biblioAndItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_biblioAndItem.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_biblioAndItem.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_biblioAndItem.Name = "splitContainer_biblioAndItem";
            this.splitContainer_biblioAndItem.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_biblioAndItem.Panel1
            // 
            this.splitContainer_biblioAndItem.Panel1.Controls.Add(this.tableLayoutPanel_biblioInfo);
            // 
            // splitContainer_biblioAndItem.Panel2
            // 
            this.splitContainer_biblioAndItem.Panel2.Controls.Add(this.tableLayoutPanel_itemInfo);
            this.splitContainer_biblioAndItem.Size = new System.Drawing.Size(723, 292);
            this.splitContainer_biblioAndItem.SplitterDistance = 236;
            this.splitContainer_biblioAndItem.SplitterWidth = 5;
            this.splitContainer_biblioAndItem.TabIndex = 0;
            // 
            // tableLayoutPanel_biblioInfo
            // 
            this.tableLayoutPanel_biblioInfo.ColumnCount = 1;
            this.tableLayoutPanel_biblioInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblioInfo.Controls.Add(this.webBrowser_operationInfo, 0, 1);
            this.tableLayoutPanel_biblioInfo.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel_biblioInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_biblioInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_biblioInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_biblioInfo.Name = "tableLayoutPanel_biblioInfo";
            this.tableLayoutPanel_biblioInfo.RowCount = 2;
            this.tableLayoutPanel_biblioInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_biblioInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_biblioInfo.Size = new System.Drawing.Size(723, 236);
            this.tableLayoutPanel_biblioInfo.TabIndex = 1;
            // 
            // webBrowser_operationInfo
            // 
            this.webBrowser_operationInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_operationInfo.Location = new System.Drawing.Point(0, 15);
            this.webBrowser_operationInfo.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_operationInfo.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser_operationInfo.Name = "webBrowser_operationInfo";
            this.webBrowser_operationInfo.Size = new System.Drawing.Size(723, 221);
            this.webBrowser_operationInfo.TabIndex = 0;
            this.webBrowser_operationInfo.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "操作信息";
            // 
            // tableLayoutPanel_itemInfo
            // 
            this.tableLayoutPanel_itemInfo.ColumnCount = 1;
            this.tableLayoutPanel_itemInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_itemInfo.Controls.Add(this.webBrowser_otherInfo, 0, 1);
            this.tableLayoutPanel_itemInfo.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel_itemInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_itemInfo.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_itemInfo.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_itemInfo.Name = "tableLayoutPanel_itemInfo";
            this.tableLayoutPanel_itemInfo.RowCount = 2;
            this.tableLayoutPanel_itemInfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_itemInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_itemInfo.Size = new System.Drawing.Size(723, 51);
            this.tableLayoutPanel_itemInfo.TabIndex = 1;
            // 
            // webBrowser_otherInfo
            // 
            this.webBrowser_otherInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_otherInfo.Location = new System.Drawing.Point(0, 15);
            this.webBrowser_otherInfo.Margin = new System.Windows.Forms.Padding(0);
            this.webBrowser_otherInfo.MinimumSize = new System.Drawing.Size(27, 25);
            this.webBrowser_otherInfo.Name = "webBrowser_otherInfo";
            this.webBrowser_otherInfo.Size = new System.Drawing.Size(723, 36);
            this.webBrowser_otherInfo.TabIndex = 0;
            this.webBrowser_otherInfo.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "其他信息";
            // 
            // UrgentChargingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Khaki;
            this.ClientSize = new System.Drawing.Size(723, 415);
            this.Controls.Add(this.tableLayoutPanel_biblioAndItem);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UrgentChargingForm";
            this.Text = "应急出纳";
            this.Load += new System.EventHandler(this.UrgentChargingForm_Load);
            this.Activated += new System.EventHandler(this.UrgentChargingForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UrgentChargingForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UrgentChargingForm_FormClosing);
            this.tableLayoutPanel_biblioAndItem.ResumeLayout(false);
            this.tableLayoutPanel_biblioAndItem.PerformLayout();
            this.tableLayoutPanel_operation.ResumeLayout(false);
            this.tableLayoutPanel_operation.PerformLayout();
            this.contextMenuStrip_selectFunc.ResumeLayout(false);
            this.splitContainer_biblioAndItem.Panel1.ResumeLayout(false);
            this.splitContainer_biblioAndItem.Panel2.ResumeLayout(false);
            this.splitContainer_biblioAndItem.ResumeLayout(false);
            this.tableLayoutPanel_biblioInfo.ResumeLayout(false);
            this.tableLayoutPanel_biblioInfo.PerformLayout();
            this.tableLayoutPanel_itemInfo.ResumeLayout(false);
            this.tableLayoutPanel_itemInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_biblioAndItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_operation;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_verifyReaderPassword;
        private System.Windows.Forms.Button button_itemAction;
        private System.Windows.Forms.TextBox textBox_itemBarcode;
        private System.Windows.Forms.TextBox textBox_readerPassword;
        private System.Windows.Forms.Button button_loadReader;
        private System.Windows.Forms.TextBox textBox_readerBarcode;
        private System.Windows.Forms.SplitContainer splitContainer_biblioAndItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_biblioInfo;
        private System.Windows.Forms.WebBrowser webBrowser_operationInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_itemInfo;
        private System.Windows.Forms.WebBrowser webBrowser_otherInfo;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_selectFunc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_borrow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_return;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_verifyReturn;
    }
}