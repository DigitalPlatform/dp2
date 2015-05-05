namespace dp2Circulation
{
    partial class ChangeItemActionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeItemActionDialog));
            this.label_state = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.changeStateActionControl1 = new dp2Circulation.ChangeStateActionControl();
            this.listView_actions = new System.Windows.Forms.ListView();
            this.columnHeader_fieldName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_fieldValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // label_state
            // 
            this.label_state.BackColor = System.Drawing.SystemColors.Control;
            this.label_state.Location = new System.Drawing.Point(114, 12);
            this.label_state.Margin = new System.Windows.Forms.Padding(0);
            this.label_state.Name = "label_state";
            this.label_state.Size = new System.Drawing.Size(10, 72);
            this.label_state.TabIndex = 18;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(264, 269);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 17;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(185, 269);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 15;
            this.label3.Text = "状态(&S):";
            // 
            // changeStateActionControl1
            // 
            this.changeStateActionControl1.ActionString = "<不改变>";
            this.changeStateActionControl1.AddString = "";
            this.changeStateActionControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.changeStateActionControl1.Location = new System.Drawing.Point(127, 12);
            this.changeStateActionControl1.Name = "changeStateActionControl1";
            this.changeStateActionControl1.RemoveString = "";
            this.changeStateActionControl1.Size = new System.Drawing.Size(212, 72);
            this.changeStateActionControl1.TabIndex = 19;
            this.changeStateActionControl1.AddOrRemoveListDropDown += new System.EventHandler(this.changeStateActionControl1_AddOrRemoveListDropDown);
            this.changeStateActionControl1.ActionChanged += new System.EventHandler(this.changeStateActionControl1_ActionChanged);
            // 
            // listView_actions
            // 
            this.listView_actions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_actions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_fieldName,
            this.columnHeader_fieldValue});
            this.listView_actions.FullRowSelect = true;
            this.listView_actions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_actions.HideSelection = false;
            this.listView_actions.Location = new System.Drawing.Point(13, 90);
            this.listView_actions.Name = "listView_actions";
            this.listView_actions.Size = new System.Drawing.Size(325, 174);
            this.listView_actions.TabIndex = 26;
            this.listView_actions.UseCompatibleStateImageBehavior = false;
            this.listView_actions.View = System.Windows.Forms.View.Details;
            this.listView_actions.DoubleClick += new System.EventHandler(this.listView_actions_DoubleClick);
            this.listView_actions.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_actions_MouseUp);
            // 
            // columnHeader_fieldName
            // 
            this.columnHeader_fieldName.Text = "字段名";
            this.columnHeader_fieldName.Width = 89;
            // 
            // columnHeader_fieldValue
            // 
            this.columnHeader_fieldValue.Text = "值";
            this.columnHeader_fieldValue.Width = 200;
            // 
            // ChangeItemActionDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(350, 303);
            this.Controls.Add(this.listView_actions);
            this.Controls.Add(this.changeStateActionControl1);
            this.Controls.Add(this.label_state);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangeItemActionDialog";
            this.ShowInTaskbar = false;
            this.Text = "动作参数";
            this.Load += new System.EventHandler(this.ChangeItemActionDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_state;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label3;
        private ChangeStateActionControl changeStateActionControl1;
        private System.Windows.Forms.ListView listView_actions;
        private System.Windows.Forms.ColumnHeader columnHeader_fieldName;
        private System.Windows.Forms.ColumnHeader columnHeader_fieldValue;
    }
}