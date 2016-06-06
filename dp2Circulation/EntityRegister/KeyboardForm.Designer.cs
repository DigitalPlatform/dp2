namespace dp2Circulation
{
    partial class KeyboardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeyboardForm));
            this.button_scan = new System.Windows.Forms.Button();
            this.textBox_input = new System.Windows.Forms.TextBox();
            this.label_name = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel_barcode = new System.Windows.Forms.Panel();
            this.webBrowser_info = new System.Windows.Forms.WebBrowser();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_start = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_dock = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel_barcode.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_scan
            // 
            this.button_scan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_scan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_scan.Location = new System.Drawing.Point(243, 24);
            this.button_scan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_scan.Name = "button_scan";
            this.button_scan.Size = new System.Drawing.Size(81, 27);
            this.button_scan.TabIndex = 15;
            this.button_scan.Text = "提交(&S)";
            this.button_scan.UseVisualStyleBackColor = true;
            this.button_scan.Click += new System.EventHandler(this.button_scan_Click);
            // 
            // textBox_input
            // 
            this.textBox_input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_input.BackColor = System.Drawing.Color.DimGray;
            this.textBox_input.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_input.ForeColor = System.Drawing.Color.White;
            this.textBox_input.HideSelection = false;
            this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.textBox_input.Location = new System.Drawing.Point(0, 22);
            this.textBox_input.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_input.Name = "textBox_input";
            this.textBox_input.Size = new System.Drawing.Size(240, 29);
            this.textBox_input.TabIndex = 14;
            this.textBox_input.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_input_KeyDown);
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(-2, 6);
            this.label_name.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(77, 12);
            this.label_name.TabIndex = 13;
            this.label_name.Text = "册条码号(&B):";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel_barcode, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.webBrowser_info, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(332, 238);
            this.tableLayoutPanel1.TabIndex = 17;
            this.tableLayoutPanel1.VisibleChanged += new System.EventHandler(this.tableLayoutPanel1_VisibleChanged);
            this.tableLayoutPanel1.Enter += new System.EventHandler(this.tableLayoutPanel1_Enter);
            this.tableLayoutPanel1.Leave += new System.EventHandler(this.tableLayoutPanel1_Leave);
            this.tableLayoutPanel1.Move += new System.EventHandler(this.tableLayoutPanel1_Move);
            this.tableLayoutPanel1.Resize += new System.EventHandler(this.tableLayoutPanel1_Resize);
            // 
            // panel_barcode
            // 
            this.panel_barcode.Controls.Add(this.textBox_input);
            this.panel_barcode.Controls.Add(this.label_name);
            this.panel_barcode.Controls.Add(this.button_scan);
            this.panel_barcode.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_barcode.Location = new System.Drawing.Point(3, 155);
            this.panel_barcode.Name = "panel_barcode";
            this.panel_barcode.Size = new System.Drawing.Size(326, 55);
            this.panel_barcode.TabIndex = 18;
            // 
            // webBrowser_info
            // 
            this.webBrowser_info.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_info.Location = new System.Drawing.Point(3, 3);
            this.webBrowser_info.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_info.Name = "webBrowser_info";
            this.webBrowser_info.Size = new System.Drawing.Size(326, 146);
            this.webBrowser_info.TabIndex = 19;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_start,
            this.toolStripButton_dock});
            this.toolStrip1.Location = new System.Drawing.Point(0, 213);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(332, 25);
            this.toolStrip1.TabIndex = 20;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_start
            // 
            this.toolStripButton_start.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_start.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_start.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_start.Image")));
            this.toolStripButton_start.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_start.Name = "toolStripButton_start";
            this.toolStripButton_start.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_start.Text = "重新开始";
            this.toolStripButton_start.Click += new System.EventHandler(this.toolStripButton_start_Click);
            // 
            // toolStripButton_dock
            // 
            this.toolStripButton_dock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dock.ForeColor = System.Drawing.Color.White;
            this.toolStripButton_dock.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dock.Image")));
            this.toolStripButton_dock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dock.Name = "toolStripButton_dock";
            this.toolStripButton_dock.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_dock.Text = "停靠";
            this.toolStripButton_dock.ToolTipText = "停靠到固定面板区";
            this.toolStripButton_dock.Click += new System.EventHandler(this.toolStripButton_dock_Click);
            // 
            // KeyboardForm
            // 
            this.AcceptButton = this.button_scan;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(332, 238);
            this.Controls.Add(this.tableLayoutPanel1);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "KeyboardForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "KeyboardForm";
            this.Activated += new System.EventHandler(this.KeyboardForm_Activated);
            this.Deactivate += new System.EventHandler(this.KeyboardForm_Deactivate);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KeyboardForm_FormClosed);
            this.Load += new System.EventHandler(this.KeyboardForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.KeyboardForm_Paint);
            this.Move += new System.EventHandler(this.KeyboardForm_Move);
            this.Resize += new System.EventHandler(this.KeyboardForm_Resize);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel_barcode.ResumeLayout(false);
            this.panel_barcode.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_scan;
        private System.Windows.Forms.TextBox textBox_input;
        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel_barcode;
        private System.Windows.Forms.WebBrowser webBrowser_info;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_start;
        private System.Windows.Forms.ToolStripButton toolStripButton_dock;
    }
}