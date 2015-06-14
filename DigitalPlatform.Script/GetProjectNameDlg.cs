using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;

namespace DigitalPlatform.Script
{
	/// <summary>
	/// Summary description for GetProjectNameDlg.
	/// </summary>
	public class GetProjectNameDlg : System.Windows.Forms.Form
	{
        public bool DisableNoneProject = false;
		public ScriptManager scriptManager = null;

		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_projectName;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.ImageList imageList_projectNodeType;
		private System.Windows.Forms.CheckBox checkBox_noneProject;
		private System.ComponentModel.IContainer components;

		public GetProjectNameDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetProjectNameDlg));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_projectName = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.imageList_projectNodeType = new System.Windows.Forms.ImageList(this.components);
            this.checkBox_noneProject = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(9, 9);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(391, 196);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 215);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "方案名:";
            // 
            // textBox_projectName
            // 
            this.textBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_projectName.Location = new System.Drawing.Point(91, 212);
            this.textBox_projectName.Name = "textBox_projectName";
            this.textBox_projectName.Size = new System.Drawing.Size(229, 21);
            this.textBox_projectName.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_OK.Location = new System.Drawing.Point(324, 209);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(76, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(324, 237);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(76, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // imageList_projectNodeType
            // 
            this.imageList_projectNodeType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_projectNodeType.ImageStream")));
            this.imageList_projectNodeType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_projectNodeType.Images.SetKeyName(0, "");
            this.imageList_projectNodeType.Images.SetKeyName(1, "");
            // 
            // checkBox_noneProject
            // 
            this.checkBox_noneProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_noneProject.AutoSize = true;
            this.checkBox_noneProject.Location = new System.Drawing.Point(9, 241);
            this.checkBox_noneProject.Name = "checkBox_noneProject";
            this.checkBox_noneProject.Size = new System.Drawing.Size(90, 16);
            this.checkBox_noneProject.TabIndex = 5;
            this.checkBox_noneProject.Text = "忽略方案(&N)";
            this.checkBox_noneProject.CheckedChanged += new System.EventHandler(this.checkBox_noneProject_CheckedChanged);
            // 
            // GetProjectNameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(408, 269);
            this.Controls.Add(this.checkBox_noneProject);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_projectName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GetProjectNameDlg";
            this.ShowInTaskbar = false;
            this.Text = "指定方案名";
            this.Load += new System.EventHandler(this.GetProjectNameDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void GetProjectNameDlg_Load(object sender, System.EventArgs e)
		{
			treeView1.ImageList = imageList_projectNodeType;
			treeView1.PathSeparator = "/";
			this.AcceptButton = this.button_OK;

			if (scriptManager != null)
			{
				try 
				{
					scriptManager.FillTree(this.treeView1);
				}
				catch(System.IO.FileNotFoundException ex) 
				{
					/*
					MessageBox.Show("装载" + scriptManager.CfgFilePath + "文件失败，原因:"
						+ ex.Message);
					*/
					MessageBox.Show(ex.Message);
					return;
				}
				catch(System.Xml.XmlException ex)
				{
					MessageBox.Show("装载" + scriptManager.CfgFilePath + "文件失败，原因:"
						+ ex.Message);
					return;
				}
			}

			if (textBox_projectName.Text != "") 
			{
                Debug.Assert(treeView1.PathSeparator == "/", "");

				TreeViewUtil.SelectTreeNode(treeView1, 
					textBox_projectName.Text,
                    '/');
			}

		
			checkBox_noneProject_CheckedChanged(null, null);

            // 2009/2/8
            if (this.DisableNoneProject == true)
                this.checkBox_noneProject.Enabled = false;
		}

		private void treeView1_AfterSelect(object sender, 
			System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node == null)
				return;
			if (e.Node.ImageIndex != 1)
				return;

			textBox_projectName.Text = e.Node.FullPath;
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_projectName.Text == "" 
                && this.checkBox_noneProject.Checked == false) 
			{
				MessageBox.Show("尚未指定方案名");
				this.DialogResult = DialogResult.None;
				return;
			}
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void checkBox_noneProject_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.checkBox_noneProject.Checked == true)
			{
				this.textBox_projectName.Text = "";
				this.textBox_projectName.Enabled = false;
				this.treeView1.Enabled = false;
			}
			else 
			{
				this.textBox_projectName.Enabled = true;
				this.treeView1.Enabled = true;

                // 让textbox中重新具有内容
                if (this.treeView1.SelectedNode != null)
                {
                    TreeNode node = this.treeView1.SelectedNode;
                    this.treeView1.SelectedNode = null;
                    this.treeView1.SelectedNode = node;
                }

			}
		
		}

		public bool NoneProject
		{
			get 
			{
				return this.checkBox_noneProject.Checked;
			}
			set 
			{
				this.checkBox_noneProject.Checked = value;
			}
		}

        public string ProjectName
        {
            get
            {
                return this.textBox_projectName.Text;
            }
            set
            {
                this.textBox_projectName.Text = value;
            }
        }
	}
}

