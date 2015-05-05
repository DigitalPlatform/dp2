namespace dp2Circulation
{
    partial class LabelDesignForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabelDesignForm));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_findLabelDefFilename = new System.Windows.Forms.Button();
            this.textBox_labelDefFilename = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelDefControl1 = new dp2Circulation.LabelDefControl();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(460, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(379, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_findLabelDefFilename
            // 
            this.button_findLabelDefFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findLabelDefFilename.Location = new System.Drawing.Point(334, 229);
            this.button_findLabelDefFilename.Margin = new System.Windows.Forms.Padding(2);
            this.button_findLabelDefFilename.Name = "button_findLabelDefFilename";
            this.button_findLabelDefFilename.Size = new System.Drawing.Size(34, 22);
            this.button_findLabelDefFilename.TabIndex = 3;
            this.button_findLabelDefFilename.Text = "...";
            this.button_findLabelDefFilename.UseVisualStyleBackColor = true;
            this.button_findLabelDefFilename.Click += new System.EventHandler(this.button_findLabelDefFilename_Click);
            // 
            // textBox_labelDefFilename
            // 
            this.textBox_labelDefFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_labelDefFilename.Location = new System.Drawing.Point(130, 231);
            this.textBox_labelDefFilename.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_labelDefFilename.Name = "textBox_labelDefFilename";
            this.textBox_labelDefFilename.Size = new System.Drawing.Size(200, 21);
            this.textBox_labelDefFilename.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 234);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "标签定义文件名(&D):";
            // 
            // labelDefControl1
            // 
            this.labelDefControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDefControl1.Changed = false;
            this.labelDefControl1.CurrentUnit = System.Drawing.GraphicsUnit.Display;
            this.labelDefControl1.DecimalPlaces = 0;
            this.labelDefControl1.GridLine = true;
            this.labelDefControl1.LabelParam = null;
            this.labelDefControl1.Location = new System.Drawing.Point(12, 12);
            this.labelDefControl1.Name = "labelDefControl1";
            this.labelDefControl1.SampleLabelText = "";
            this.labelDefControl1.Size = new System.Drawing.Size(523, 211);
            this.labelDefControl1.TabIndex = 0;
            this.labelDefControl1.UiState = resources.GetString("labelDefControl1.UiState");
            this.labelDefControl1.Xml = "";
            // 
            // LabelDesignForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(547, 264);
            this.Controls.Add(this.labelDefControl1);
            this.Controls.Add(this.button_findLabelDefFilename);
            this.Controls.Add(this.textBox_labelDefFilename);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "LabelDesignForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "标签设计";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LabelDesignForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LabelDesignForm_FormClosed);
            this.Load += new System.EventHandler(this.LabelDesignForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_findLabelDefFilename;
        private System.Windows.Forms.TextBox textBox_labelDefFilename;
        private System.Windows.Forms.Label label3;
        private LabelDefControl labelDefControl1;
    }
}