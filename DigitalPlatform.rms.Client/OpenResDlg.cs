using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// Summary description for OpenResDlg.
	/// </summary>
	public class OpenResDlg : System.Windows.Forms.Form
	{
		public string Path = "";

		public string Paths = "";

		public bool MultiSelect = false;

		public ApplicationInfo ap = null;	// 引用
		public string ApCfgTitle = "";	// 在ap中保存窗口外观状态的标题字符串

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_resPath;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
        private ResTree resTree;
        private IContainer components;

		public OpenResDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenResDlg));
            this.resTree = new DigitalPlatform.rms.Client.ResTree();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_resPath = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // resTree
            // 
            this.resTree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resTree.HideSelection = false;
            this.resTree.ImageIndex = 0;
            this.resTree.Location = new System.Drawing.Point(12, 12);
            this.resTree.Name = "resTree";
            this.resTree.SelectedImageIndex = 0;
            this.resTree.Size = new System.Drawing.Size(496, 224);
            this.resTree.TabIndex = 0;
            this.resTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.resTree_AfterCheck);
            this.resTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.resTree_AfterSelect);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 253);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "资源路径(&P):";
            // 
            // textBox_resPath
            // 
            this.textBox_resPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_resPath.Location = new System.Drawing.Point(12, 268);
            this.textBox_resPath.Multiline = true;
            this.textBox_resPath.Name = "textBox_resPath";
            this.textBox_resPath.Size = new System.Drawing.Size(415, 48);
            this.textBox_resPath.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(433, 266);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(433, 293);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // OpenResDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(520, 328);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_resPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.resTree);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OpenResDlg";
            this.ShowInTaskbar = false;
            this.Text = "指定资源路径";
            this.Closed += new System.EventHandler(this.OpenResDlg_Closed);
            this.Load += new System.EventHandler(this.OpenResDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion



		public int [] EnabledIndices
		{
			get 
			{
				return this.resTree.EnabledIndices;
			}
			set 
			{
				this.resTree.EnabledIndices = value;
			}
		}

		// 初始化，在打开前调用
		public void Initial(ServerCollection servers,
			RmsChannelCollection channels)
		{
			this.resTree.Servers = servers;
			this.resTree.Channels = channels;
		}

		private void OpenResDlg_Load(object sender, System.EventArgs e)
		{
			if (ap != null) {
				if (ApCfgTitle != "" && ApCfgTitle != null) 
				{
					ap.LoadFormStates(this,
						ApCfgTitle);
				}
				else 
				{
					Debug.Assert(true, "若要用ap保存和恢复窗口外观状态，必须先设置ApCfgTitle成员");
				}

			}

            if (this.resTree.Servers != null)
            {
                this.resTree.Servers.ServerChanged += new ServerChangedEventHandle(Servers_ServerChanged);
            }
                // resTree.EnabledIndices = new int[] { ResTree.RESTYPE_DB };

			// 填充内容
			resTree.Fill(null);

			if (MultiSelect == true) 
			{
				resTree.CheckBoxes = true;	// 允许复选
			}

			if (this.Path != "") 
			{
				ResPath respath = new ResPath(this.Path);

				// 展开到指定的节点
                if (resTree.ExpandPath(respath) == true
                    && resTree.SelectedNode != null
                    && EnabledIndices != null)
                {
                    // 2013/2/15
                    // 如果下一级是全部灰色的节点，则不要展开它们
                    bool bFound = false;
                    foreach(TreeNode child in resTree.SelectedNode.Nodes)
                    {
                        if (StringUtil.IsInList(child.ImageIndex, EnabledIndices) == true)
                            bFound = true;
                    }

                    if (bFound == false)
                        resTree.SelectedNode.Collapse();
                }
			}



			if (this.Paths != "")
			{
				resTree.CheckBoxes = true;	// 允许复选

				string[] aPath = this.Paths.Split(new char[]{';'});

				for(int i=0;i<aPath.Length; i++)
				{
					if (aPath[i].Trim() == "")
						continue;

					ResPath respath = new ResPath(aPath[i].Trim());

					// 展开到指定的节点
					resTree.ExpandPath(respath);

					bool bRet = this.resTree.CheckNode(respath,
						true);
				}

			}
		}

        void Servers_ServerChanged(object sender, ServerChangedEventArgs e)
        {
            this.resTree.Refresh(ResTree.RefreshStyle.Servers);
        }

		private void OpenResDlg_Closed(object sender, System.EventArgs e)
		{
            if (this.resTree.Servers != null)
            {
                this.resTree.Servers.ServerChanged -= new ServerChangedEventHandle(Servers_ServerChanged);
            }


			if (ap != null) 
			{
				if (ApCfgTitle != "" && ApCfgTitle != null) 
				{
					ap.SaveFormStates(this,
						ApCfgTitle);
				}
				else 
				{
					Debug.Assert(true, "若要用ap保存和恢复窗口外观状态，必须先设置ApCfgTitle成员");
				}

			}
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (textBox_resPath.Text == "")
			{
				MessageBox.Show(this, "尚未指定资源路径");
				return;
			}

			if (resTree.CheckBoxes == true)
			{
				this.Paths = textBox_resPath.Text;
			}
			else
			{
				this.Path = textBox_resPath.Text;
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void resTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (resTree.SelectedNode == null)
				return;

			if (EnabledIndices != null
				&& StringUtil.IsInList(resTree.SelectedNode.ImageIndex, EnabledIndices) == false)
			{
				textBox_resPath.Text = "";
				return;
			}

			/*
			if (resTree.SelectedNode.ImageIndex != ResTree.RESTYPE_DB) 
			{
				textBox_resPath.Text = "";
				return;
			}
			*/

			ResPath respath = new ResPath(resTree.SelectedNode);

			textBox_resPath.Text = respath.FullPath;

		}

		private void resTree_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (resTree.CheckBoxes != true)
				return;


			TreeNode[] nodes = TreeViewUtil.GetCheckedNodes(this.resTree);

			if (nodes.Length == 0) 
			{
				textBox_resPath.Text = "";
				return;
			}

			textBox_resPath.Text = "";

			for(int i=0;i<nodes.Length;i++)
			{
				if (EnabledIndices != null
					&& StringUtil.IsInList(nodes[i].ImageIndex, EnabledIndices) == false)
					continue;

				/*
				if (nodes[i].ImageIndex != ResTree.RESTYPE_DB) 
					continue;
				*/

				if (textBox_resPath.Text != "")
					textBox_resPath.Text += ";";

				ResPath respath = new ResPath(nodes[i]);
				textBox_resPath.Text += respath.FullPath;
			}


		}
	}
}
