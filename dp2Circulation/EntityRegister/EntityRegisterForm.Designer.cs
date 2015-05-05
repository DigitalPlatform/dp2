namespace dp2Circulation
{
    partial class EntityRegisterForm
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
            this.entityRegisterControl1 = new dp2Circulation.EntityRegisterControl();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.button_start_createCfgFile = new System.Windows.Forms.Button();
            this.tabPage_defaultTemplate = new System.Windows.Forms.TabPage();
            this.entityEditControl_quickRegisterDefault = new dp2Circulation.EntityEditControl();
            this.tabPage_register = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.colorSummaryControl1 = new dp2Circulation.ColorSummaryControl();
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            this.tabPage_defaultTemplate.SuspendLayout();
            this.tabPage_register.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // entityRegisterControl1
            // 
            this.entityRegisterControl1.AutoScroll = true;
            this.entityRegisterControl1.AutoSize = true;
            this.entityRegisterControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityRegisterControl1.IsFocued = false;
            this.entityRegisterControl1.Location = new System.Drawing.Point(0, 0);
            this.entityRegisterControl1.MainForm = null;
            this.entityRegisterControl1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
            this.entityRegisterControl1.Name = "entityRegisterControl1";
            this.entityRegisterControl1.ServersDom = null;
            this.entityRegisterControl1.Size = new System.Drawing.Size(0, 0);
            this.entityRegisterControl1.TabIndex = 0;
            this.entityRegisterControl1.DisplayError += new dp2Circulation.DisplayErrorEventHandler(this.entityRegisterControl1_DisplayError);
            this.entityRegisterControl1.SizeChanged += new System.EventHandler(this.entityRegisterControl1_SizeChanged);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_start);
            this.tabControl_main.Controls.Add(this.tabPage_defaultTemplate);
            this.tabControl_main.Controls.Add(this.tabPage_register);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(342, 264);
            this.tabControl_main.TabIndex = 1;
            // 
            // tabPage_start
            // 
            this.tabPage_start.Controls.Add(this.button_start_createCfgFile);
            this.tabPage_start.Location = new System.Drawing.Point(4, 22);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_start.Size = new System.Drawing.Size(334, 238);
            this.tabPage_start.TabIndex = 0;
            this.tabPage_start.Text = "开始";
            this.tabPage_start.UseVisualStyleBackColor = true;
            // 
            // button_start_createCfgFile
            // 
            this.button_start_createCfgFile.Location = new System.Drawing.Point(9, 28);
            this.button_start_createCfgFile.Name = "button_start_createCfgFile";
            this.button_start_createCfgFile.Size = new System.Drawing.Size(203, 23);
            this.button_start_createCfgFile.TabIndex = 0;
            this.button_start_createCfgFile.Text = "重新创建服务器配置文件";
            this.button_start_createCfgFile.UseVisualStyleBackColor = true;
            this.button_start_createCfgFile.Click += new System.EventHandler(this.button_start_createCfgFile_Click);
            // 
            // tabPage_defaultTemplate
            // 
            this.tabPage_defaultTemplate.Controls.Add(this.entityEditControl_quickRegisterDefault);
            this.tabPage_defaultTemplate.Location = new System.Drawing.Point(4, 22);
            this.tabPage_defaultTemplate.Name = "tabPage_defaultTemplate";
            this.tabPage_defaultTemplate.Size = new System.Drawing.Size(334, 238);
            this.tabPage_defaultTemplate.TabIndex = 2;
            this.tabPage_defaultTemplate.Text = "册记录缺省值";
            this.tabPage_defaultTemplate.UseVisualStyleBackColor = true;
            this.tabPage_defaultTemplate.Enter += new System.EventHandler(this.tabPage_defaultTemplate_Enter);
            this.tabPage_defaultTemplate.Leave += new System.EventHandler(this.tabPage_defaultTemplate_Leave);
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
            this.entityEditControl_quickRegisterDefault.CreateState = dp2Circulation.ItemDisplayState.Normal;
            this.entityEditControl_quickRegisterDefault.DisplayMode = "full";
            this.entityEditControl_quickRegisterDefault.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityEditControl_quickRegisterDefault.ErrorInfo = "";
            this.entityEditControl_quickRegisterDefault.Initializing = true;
            this.entityEditControl_quickRegisterDefault.Intact = "";
            this.entityEditControl_quickRegisterDefault.Location = new System.Drawing.Point(0, 0);
            this.entityEditControl_quickRegisterDefault.LocationString = "";
            this.entityEditControl_quickRegisterDefault.Margin = new System.Windows.Forms.Padding(2);
            this.entityEditControl_quickRegisterDefault.MaximumSize = new System.Drawing.Size(260, 2000);
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
            this.entityEditControl_quickRegisterDefault.Size = new System.Drawing.Size(260, 238);
            this.entityEditControl_quickRegisterDefault.Source = "";
            this.entityEditControl_quickRegisterDefault.State = "";
            this.entityEditControl_quickRegisterDefault.TabIndex = 2;
            this.entityEditControl_quickRegisterDefault.Volume = "";
            this.entityEditControl_quickRegisterDefault.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(this.entityEditControl_quickRegisterDefault_GetValueTable);
            // 
            // tabPage_register
            // 
            this.tabPage_register.Controls.Add(this.panel1);
            this.tabPage_register.Controls.Add(this.colorSummaryControl1);
            this.tabPage_register.Location = new System.Drawing.Point(4, 22);
            this.tabPage_register.Name = "tabPage_register";
            this.tabPage_register.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_register.Size = new System.Drawing.Size(334, 238);
            this.tabPage_register.TabIndex = 1;
            this.tabPage_register.Text = "登记";
            this.tabPage_register.UseVisualStyleBackColor = true;
            this.tabPage_register.SizeChanged += new System.EventHandler(this.tabPage_register_SizeChanged);
            this.tabPage_register.Resize += new System.EventHandler(this.tabPage_register_Resize);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.entityRegisterControl1);
            this.panel1.Location = new System.Drawing.Point(0, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(0, 0);
            this.panel1.TabIndex = 8;
            // 
            // colorSummaryControl1
            // 
            this.colorSummaryControl1.ColorList = "";
            this.colorSummaryControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.colorSummaryControl1.Location = new System.Drawing.Point(3, 225);
            this.colorSummaryControl1.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.colorSummaryControl1.Name = "colorSummaryControl1";
            this.colorSummaryControl1.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.colorSummaryControl1.Size = new System.Drawing.Size(328, 10);
            this.colorSummaryControl1.TabIndex = 6;
            this.colorSummaryControl1.Click += new System.EventHandler(this.colorSummaryControl1_Click);
            // 
            // EntityRegisterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(342, 264);
            this.Controls.Add(this.tabControl_main);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Name = "EntityRegisterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "册登记";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EntityRegisterForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.EntityRegisterForm_FormClosed);
            this.Load += new System.EventHandler(this.EntityRegisterForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.tabPage_defaultTemplate.ResumeLayout(false);
            this.tabPage_register.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private EntityRegisterControl entityRegisterControl1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.TabPage tabPage_register;
        private System.Windows.Forms.TabPage tabPage_defaultTemplate;
        private EntityEditControl entityEditControl_quickRegisterDefault;
        private ColorSummaryControl colorSummaryControl1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_start_createCfgFile;
    }
}