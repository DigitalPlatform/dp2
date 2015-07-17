using System;
using System.Drawing;
using System.Collections;

using System.ComponentModel;
using System.Windows.Forms;

using System.Xml;

using DigitalPlatform.Xml;

namespace dp2Manager
{
	/// <summary>
	/// Summary description for QuickSetRightsDlg.
	/// </summary>
	public class QuickSetRightsDlg : System.Windows.Forms.Form
	{
		XmlDocument cfgDom = new XmlDocument();

		public string CfgFileName = "";	// 配置文件名

		public QuickRights QuickRights = null;	// 返回选择的权限参数

		public bool AllUsers = false;	// 返回是否选择了针对全部用户

		public ArrayList AllUserNames = new ArrayList();
		public ArrayList SelectedUserNames = new ArrayList();

		private System.Windows.Forms.ListView listView_style;
		private System.Windows.Forms.ColumnHeader columnHeader_name;
		private System.Windows.Forms.ColumnHeader columnHeader_comment;
		private System.Windows.Forms.RadioButton radioButton_selectedUsers;
		private System.Windows.Forms.RadioButton radioButton_allUsers;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.ListView listView_userNames;
		private System.Windows.Forms.ColumnHeader columnHeader_userName;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public QuickSetRightsDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuickSetRightsDlg));
            this.listView_style = new System.Windows.Forms.ListView();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.radioButton_selectedUsers = new System.Windows.Forms.RadioButton();
            this.radioButton_allUsers = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listView_userNames = new System.Windows.Forms.ListView();
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_style
            // 
            this.listView_style.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_style.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_comment});
            this.listView_style.FullRowSelect = true;
            this.listView_style.HideSelection = false;
            this.listView_style.Location = new System.Drawing.Point(8, 8);
            this.listView_style.MultiSelect = false;
            this.listView_style.Name = "listView_style";
            this.listView_style.Size = new System.Drawing.Size(416, 128);
            this.listView_style.TabIndex = 0;
            this.listView_style.UseCompatibleStateImageBehavior = false;
            this.listView_style.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "风格名";
            this.columnHeader_name.Width = 146;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "注释";
            this.columnHeader_comment.Width = 279;
            // 
            // radioButton_selectedUsers
            // 
            this.radioButton_selectedUsers.Checked = true;
            this.radioButton_selectedUsers.Location = new System.Drawing.Point(176, 160);
            this.radioButton_selectedUsers.Name = "radioButton_selectedUsers";
            this.radioButton_selectedUsers.Size = new System.Drawing.Size(104, 24);
            this.radioButton_selectedUsers.TabIndex = 3;
            this.radioButton_selectedUsers.TabStop = true;
            this.radioButton_selectedUsers.Text = "所选用户(&S)";
            // 
            // radioButton_allUsers
            // 
            this.radioButton_allUsers.Location = new System.Drawing.Point(32, 160);
            this.radioButton_allUsers.Name = "radioButton_allUsers";
            this.radioButton_allUsers.Size = new System.Drawing.Size(104, 24);
            this.radioButton_allUsers.TabIndex = 2;
            this.radioButton_allUsers.Text = "全部用户(&A)";
            this.radioButton_allUsers.CheckedChanged += new System.EventHandler(this.radioButton_allUsers_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listView_userNames);
            this.groupBox1.Location = new System.Drawing.Point(8, 144);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(416, 160);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 针对: ";
            // 
            // listView_userNames
            // 
            this.listView_userNames.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_userNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_userName});
            this.listView_userNames.FullRowSelect = true;
            this.listView_userNames.HideSelection = false;
            this.listView_userNames.Location = new System.Drawing.Point(16, 40);
            this.listView_userNames.Name = "listView_userNames";
            this.listView_userNames.Size = new System.Drawing.Size(384, 112);
            this.listView_userNames.TabIndex = 0;
            this.listView_userNames.UseCompatibleStateImageBehavior = false;
            this.listView_userNames.View = System.Windows.Forms.View.Details;
            this.listView_userNames.SelectedIndexChanged += new System.EventHandler(this.listView_userNames_SelectedIndexChanged);
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 288;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(272, 320);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(352, 320);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // QuickSetRightsDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(432, 352);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.radioButton_allUsers);
            this.Controls.Add(this.radioButton_selectedUsers);
            this.Controls.Add(this.listView_style);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QuickSetRightsDlg";
            this.ShowInTaskbar = false;
            this.Text = "快速设置权限";
            this.Load += new System.EventHandler(this.QuickSetRightsDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void QuickSetRightsDlg_Load(object sender, System.EventArgs e)
		{
			string strError = "";

			FillUserNameList();

			int nRet = FillList(out strError);
			if (nRet == -1)
			{
				MessageBox.Show(this, strError);
				return ;
			}

			// 选择第一项
			if (this.listView_style.Items.Count >= 1)
				this.listView_style.Items[0].Selected = true;
		
		}


		void FillUserNameList()
		{
			this.listView_userNames.Items.Clear();
			for(int i=0;i<this.AllUserNames.Count;i++)
			{
				string strUserName = (string)this.AllUserNames[i];

				ListViewItem item = new ListViewItem(strUserName, 0);

				this.listView_userNames.Items.Add(item);

				for(int j=0;j<this.SelectedUserNames.Count;j++)
				{
					if (strUserName == (string)this.SelectedUserNames[j])
					{
						item.Selected = true;
						break;
					}
				}
			}
		}

		int FillList(out string strError)
		{

			strError = "";
			try 
			{
				cfgDom.Load(this.CfgFileName);
			}
			catch(Exception ex)
			{
				strError = ex.Message;
				return -1;
			}

			this.listView_style.Items.Clear();
			XmlNodeList nodes = cfgDom.DocumentElement.SelectNodes("style");
			for(int i=0;i<nodes.Count;i++)
			{
				XmlNode node = nodes[i];
				string strName = DomUtil.GetAttr(node, "name");
				string strComment = DomUtil.GetAttr(node, "comment");

				ListViewItem item = new ListViewItem(strName, 0);
				item.SubItems.Add(strComment);

				this.listView_style.Items.Add(item);
			}

			return 0;
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
            string strError = "";
			if (this.listView_style.SelectedItems.Count == 0)
			{
				strError = "尚未选定风格名";
				goto ERROR1;
			}

			if (this.listView_userNames.SelectedItems.Count == 0)
			{
				strError = "尚未选定要针对的用户名";
				goto ERROR1;
			}

			string strName = this.listView_style.SelectedItems[0].Text;

            int nRet = QuickRights.Build(this.cfgDom,
                strName,
                out this.QuickRights,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            /*
			string strXPath = "//style[@name='" +strName+ "']";
			XmlNode parent = this.cfgDom.DocumentElement.SelectSingleNode(strXPath);
			if (parent == null)
			{
				MessageBox.Show(this, "dom出错");
				return;
			}

			this.QuickRights = new QuickRights();

            XmlNodeList nodes = parent.SelectNodes("rights");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                QuickRightsItem item = new QuickRightsItem();
                item.Type = DomUtil.GetAttr(node, "type");
                item.Name = DomUtil.GetAttr(node, "name");
                item.Rights = DomUtil.GetNodeText(node);
                int nStyle = 0;
                try
                {
                    nStyle = Convert.ToInt32(DomUtil.GetAttr(node, "style"));
                }
                catch
                {
                }
                item.Style = nStyle;

                this.QuickRights.Add(item);

            }
             */

            /*
			this.QuickRights.ServerRights = DomUtil.GetElementText(parent, "rights[@name='server']");
			this.QuickRights.DatabaseRights = DomUtil.GetElementText(parent, "rights[@name='database']");
			this.QuickRights.DirectoryRights = DomUtil.GetElementText(parent, "rights[@name='directory']");
			this.QuickRights.FileRights = DomUtil.GetElementText(parent, "rights[@name='file']");
             */


			/*
			if (this.radioButton_allUsers.Checked == true)
				this.AllUsers = true;
			else
				this.AllUsers = false;
			*/
			// 收集已经选择的用户名
			this.SelectedUserNames.Clear();
			for(int i=0;i<this.listView_userNames.SelectedItems.Count;i++)
			{
				this.SelectedUserNames.Add(this.listView_userNames.SelectedItems[i].Text);
			}
	
			this.DialogResult = DialogResult.OK;
			this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
		
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void radioButton_allUsers_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.radioButton_allUsers.Checked == true)
			{
				// 全选
				for(int i=0;i<this.listView_userNames.Items.Count;i++)
				{
					if (this.listView_userNames.Items[i].Selected != true)
						this.listView_userNames.Items[i].Selected = true;
				}
			}
		}

		private void listView_userNames_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// 是否全选
			if (this.listView_userNames.SelectedItems.Count == this.listView_userNames.Items.Count)
			{
				if (this.radioButton_allUsers.Checked != true)
				{
					this.radioButton_allUsers.Checked = true;
				}
					return;
			}

			this.radioButton_selectedUsers.Checked = true;
		}


	}


}
