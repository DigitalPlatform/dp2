namespace dp2Circulation
{
    partial class PaddingDialog
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
            this.numericUpDown_left = new DigitalPlatform.CommonControl.UniverseNumericUpDown();
            this.numericUpDown_top = new DigitalPlatform.CommonControl.UniverseNumericUpDown();
            this.numericUpDown_right = new DigitalPlatform.CommonControl.UniverseNumericUpDown();
            this.numericUpDown_bottom = new DigitalPlatform.CommonControl.UniverseNumericUpDown();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_left)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_top)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_right)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_bottom)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown_left
            // 
            this.numericUpDown_left.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.numericUpDown_left.Location = new System.Drawing.Point(12, 48);
            this.numericUpDown_left.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_left.Name = "numericUpDown_left";
            this.numericUpDown_left.Size = new System.Drawing.Size(67, 21);
            this.numericUpDown_left.TabIndex = 0;
            this.numericUpDown_left.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_left.UniverseValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_left.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDown_top
            // 
            this.numericUpDown_top.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.numericUpDown_top.Location = new System.Drawing.Point(107, 12);
            this.numericUpDown_top.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_top.Name = "numericUpDown_top";
            this.numericUpDown_top.Size = new System.Drawing.Size(67, 21);
            this.numericUpDown_top.TabIndex = 1;
            this.numericUpDown_top.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_top.UniverseValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_top.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDown_right
            // 
            this.numericUpDown_right.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.numericUpDown_right.Location = new System.Drawing.Point(205, 48);
            this.numericUpDown_right.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_right.Name = "numericUpDown_right";
            this.numericUpDown_right.Size = new System.Drawing.Size(67, 21);
            this.numericUpDown_right.TabIndex = 2;
            this.numericUpDown_right.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_right.UniverseValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_right.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // numericUpDown_bottom
            // 
            this.numericUpDown_bottom.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.numericUpDown_bottom.Location = new System.Drawing.Point(107, 84);
            this.numericUpDown_bottom.Maximum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_bottom.Name = "numericUpDown_bottom";
            this.numericUpDown_bottom.Size = new System.Drawing.Size(67, 21);
            this.numericUpDown_bottom.TabIndex = 3;
            this.numericUpDown_bottom.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericUpDown_bottom.UniverseValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.numericUpDown_bottom.ValueChanged += new System.EventHandler(this.numericUpDown_ValueChanged);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(197, 119);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(116, 119);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(85, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(114, 39);
            this.panel1.TabIndex = 7;
            // 
            // PaddingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(284, 154);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.numericUpDown_bottom);
            this.Controls.Add(this.numericUpDown_right);
            this.Controls.Add(this.numericUpDown_top);
            this.Controls.Add(this.numericUpDown_left);
            this.Name = "PaddingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "边距设定";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_left)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_top)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_right)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_bottom)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.CommonControl.UniverseNumericUpDown numericUpDown_left;
        private DigitalPlatform.CommonControl.UniverseNumericUpDown numericUpDown_top;
        private DigitalPlatform.CommonControl.UniverseNumericUpDown numericUpDown_right;
        private DigitalPlatform.CommonControl.UniverseNumericUpDown numericUpDown_bottom;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Panel panel1;
    }
}