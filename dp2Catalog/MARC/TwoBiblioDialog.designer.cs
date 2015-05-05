namespace dp2Catalog
{
    partial class TwoBiblioDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TwoBiblioDialog));
            this.splitContainer_two_marc = new System.Windows.Forms.SplitContainer();
            this.marcEditor1 = new DigitalPlatform.Marc.MarcEditor();
            this.label_left = new System.Windows.Forms.Label();
            this.checkBox_editTarget = new System.Windows.Forms.CheckBox();
            this.marcEditor2 = new DigitalPlatform.Marc.MarcEditor();
            this.label_right = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_yes = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_no = new System.Windows.Forms.Button();
            this.tableLayoutPanel_left = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_right = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer_two_marc.Panel1.SuspendLayout();
            this.splitContainer_two_marc.Panel2.SuspendLayout();
            this.splitContainer_two_marc.SuspendLayout();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_left.SuspendLayout();
            this.tableLayoutPanel_right.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_two_marc
            // 
            this.splitContainer_two_marc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_two_marc.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_two_marc.Name = "splitContainer_two_marc";
            // 
            // splitContainer_two_marc.Panel1
            // 
            this.splitContainer_two_marc.Panel1.Controls.Add(this.tableLayoutPanel_left);
            // 
            // splitContainer_two_marc.Panel2
            // 
            this.splitContainer_two_marc.Panel2.Controls.Add(this.tableLayoutPanel_right);
            this.splitContainer_two_marc.Size = new System.Drawing.Size(505, 218);
            this.splitContainer_two_marc.SplitterDistance = 245;
            this.splitContainer_two_marc.SplitterWidth = 8;
            this.splitContainer_two_marc.TabIndex = 0;
            // 
            // marcEditor1
            // 
            this.marcEditor1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.marcEditor1.Changed = true;
            this.marcEditor1.ContentBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor1.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.marcEditor1.CurrentImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.marcEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.marcEditor1.DocumentOrgX = 0;
            this.marcEditor1.DocumentOrgY = 0;
            this.marcEditor1.FieldNameCaptionWidth = 0;
            this.marcEditor1.FocusedField = null;
            this.marcEditor1.FocusedFieldIndex = -1;
            this.marcEditor1.HorzGridColor = System.Drawing.Color.LightGray;
            this.marcEditor1.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor1.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.marcEditor1.IndicatorTextColor = System.Drawing.Color.Green;
            this.marcEditor1.Location = new System.Drawing.Point(3, 18);
            this.marcEditor1.Marc = "????????????????????????";
            this.marcEditor1.MarcDefDom = null;
            this.marcEditor1.Name = "marcEditor1";
            this.marcEditor1.NameBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor1.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.marcEditor1.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.marcEditor1.NameTextColor = System.Drawing.Color.Blue;
            this.marcEditor1.ReadOnly = false;
            this.marcEditor1.SelectionStart = -1;
            this.marcEditor1.Size = new System.Drawing.Size(239, 197);
            this.marcEditor1.TabIndex = 1;
            this.marcEditor1.Text = "marcEditor1";
            this.marcEditor1.VertGridColor = System.Drawing.Color.LightGray;
            // 
            // label_left
            // 
            this.label_left.AutoSize = true;
            this.label_left.Location = new System.Drawing.Point(3, 0);
            this.label_left.Name = "label_left";
            this.label_left.Size = new System.Drawing.Size(30, 15);
            this.label_left.TabIndex = 0;
            this.label_left.Text = "左:";
            // 
            // checkBox_editTarget
            // 
            this.checkBox_editTarget.AutoSize = true;
            this.checkBox_editTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBox_editTarget.Location = new System.Drawing.Point(3, 196);
            this.checkBox_editTarget.Name = "checkBox_editTarget";
            this.checkBox_editTarget.Size = new System.Drawing.Size(246, 19);
            this.checkBox_editTarget.TabIndex = 0;
            this.checkBox_editTarget.Text = "直接修改目标记录(&E)";
            this.checkBox_editTarget.UseVisualStyleBackColor = true;
            this.checkBox_editTarget.CheckedChanged += new System.EventHandler(this.checkBox_editTarget_CheckedChanged);
            // 
            // marcEditor2
            // 
            this.marcEditor2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.marcEditor2.Changed = true;
            this.marcEditor2.ContentBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor2.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.marcEditor2.CurrentImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.marcEditor2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.marcEditor2.DocumentOrgX = 0;
            this.marcEditor2.DocumentOrgY = 0;
            this.marcEditor2.FieldNameCaptionWidth = 0;
            this.marcEditor2.FocusedField = null;
            this.marcEditor2.FocusedFieldIndex = -1;
            this.marcEditor2.HorzGridColor = System.Drawing.Color.LightGray;
            this.marcEditor2.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor2.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.marcEditor2.IndicatorTextColor = System.Drawing.Color.Green;
            this.marcEditor2.Location = new System.Drawing.Point(3, 18);
            this.marcEditor2.Marc = "????????????????????????";
            this.marcEditor2.MarcDefDom = null;
            this.marcEditor2.Name = "marcEditor2";
            this.marcEditor2.NameBackColor = System.Drawing.SystemColors.Window;
            this.marcEditor2.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.marcEditor2.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.marcEditor2.NameTextColor = System.Drawing.Color.Blue;
            this.marcEditor2.ReadOnly = true;
            this.marcEditor2.SelectionStart = -1;
            this.marcEditor2.Size = new System.Drawing.Size(246, 172);
            this.marcEditor2.TabIndex = 1;
            this.marcEditor2.Text = "marcEditor2";
            this.marcEditor2.VertGridColor = System.Drawing.Color.LightGray;
            // 
            // label_right
            // 
            this.label_right.AutoSize = true;
            this.label_right.Location = new System.Drawing.Point(3, 0);
            this.label_right.Name = "label_right";
            this.label_right.Size = new System.Drawing.Size(30, 15);
            this.label_right.TabIndex = 0;
            this.label_right.Text = "右:";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(442, 315);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "中断";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_yes
            // 
            this.button_yes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_yes.Location = new System.Drawing.Point(184, 315);
            this.button_yes.Name = "button_yes";
            this.button_yes.Size = new System.Drawing.Size(99, 28);
            this.button_yes.TabIndex = 1;
            this.button_yes.Text = "覆盖(&O)";
            this.button_yes.UseVisualStyleBackColor = true;
            this.button_yes.Click += new System.EventHandler(this.button_yes_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(12, 13);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.textBox_message);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.splitContainer_two_marc);
            this.splitContainer_main.Size = new System.Drawing.Size(505, 296);
            this.splitContainer_main.SplitterDistance = 70;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 7;
            // 
            // textBox_message
            // 
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(0, 0);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(505, 70);
            this.textBox_message.TabIndex = 0;
            // 
            // button_no
            // 
            this.button_no.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_no.Location = new System.Drawing.Point(289, 315);
            this.button_no.Name = "button_no";
            this.button_no.Size = new System.Drawing.Size(99, 28);
            this.button_no.TabIndex = 2;
            this.button_no.Text = "不覆盖(&N)";
            this.button_no.UseVisualStyleBackColor = true;
            this.button_no.Click += new System.EventHandler(this.button_no_Click);
            // 
            // tableLayoutPanel_left
            // 
            this.tableLayoutPanel_left.ColumnCount = 1;
            this.tableLayoutPanel_left.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_left.Controls.Add(this.label_left, 0, 0);
            this.tableLayoutPanel_left.Controls.Add(this.marcEditor1, 0, 1);
            this.tableLayoutPanel_left.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_left.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_left.Name = "tableLayoutPanel_left";
            this.tableLayoutPanel_left.RowCount = 3;
            this.tableLayoutPanel_left.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_left.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_left.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_left.Size = new System.Drawing.Size(245, 218);
            this.tableLayoutPanel_left.TabIndex = 2;
            // 
            // tableLayoutPanel_right
            // 
            this.tableLayoutPanel_right.ColumnCount = 1;
            this.tableLayoutPanel_right.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_right.Controls.Add(this.label_right, 0, 0);
            this.tableLayoutPanel_right.Controls.Add(this.checkBox_editTarget, 0, 2);
            this.tableLayoutPanel_right.Controls.Add(this.marcEditor2, 0, 1);
            this.tableLayoutPanel_right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_right.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_right.Name = "tableLayoutPanel_right";
            this.tableLayoutPanel_right.RowCount = 3;
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.Size = new System.Drawing.Size(252, 218);
            this.tableLayoutPanel_right.TabIndex = 2;
            // 
            // TwoBiblioDialog
            // 
            this.AcceptButton = this.button_yes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(529, 355);
            this.Controls.Add(this.button_no);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_yes);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TwoBiblioDialog";
            this.ShowInTaskbar = false;
            this.Text = "TwoBiblioDialog";
            this.Load += new System.EventHandler(this.TwoBiblioDialog_Load);
            this.splitContainer_two_marc.Panel1.ResumeLayout(false);
            this.splitContainer_two_marc.Panel2.ResumeLayout(false);
            this.splitContainer_two_marc.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel1.PerformLayout();
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_left.ResumeLayout(false);
            this.tableLayoutPanel_left.PerformLayout();
            this.tableLayoutPanel_right.ResumeLayout(false);
            this.tableLayoutPanel_right.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_two_marc;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_yes;
        private DigitalPlatform.Marc.MarcEditor marcEditor1;
        private System.Windows.Forms.Label label_left;
        private System.Windows.Forms.Label label_right;
        private DigitalPlatform.Marc.MarcEditor marcEditor2;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_no;
        private System.Windows.Forms.CheckBox checkBox_editTarget;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_left;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_right;
    }
}