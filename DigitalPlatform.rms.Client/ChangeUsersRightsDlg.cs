using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms.Client
{
	/// <summary>
	/// 一次性修改多个用户针对某个数据库对象的权限 的 对话框
	/// </summary>
	public class ChangeUsersRightsDlg : System.Windows.Forms.Form
	{

		public ServerCollection Servers = null;	// 引用
		public RmsChannelCollection Channels = null;

		public DigitalPlatform.StopManager stopManager = null;

		// Channel channel = null;

		public string ServerUrl = "";

		public string Lang = "zh";

		private System.Windows.Forms.ListView listView_users;
		private System.Windows.Forms.ColumnHeader columnHeader_userName;
		private System.Windows.Forms.ColumnHeader columnHeader_rights;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_databaseObject;
		private System.Windows.Forms.CheckBox checkBox_read;
		private System.Windows.Forms.CheckBox checkBox_write;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ChangeUsersRightsDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeUsersRightsDlg));
            this.listView_users = new System.Windows.Forms.ListView();
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_rights = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_databaseObject = new System.Windows.Forms.TextBox();
            this.checkBox_read = new System.Windows.Forms.CheckBox();
            this.checkBox_write = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView_users
            // 
            this.listView_users.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_users.CheckBoxes = true;
            this.listView_users.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_userName,
            this.columnHeader_rights});
            this.listView_users.FullRowSelect = true;
            this.listView_users.HideSelection = false;
            this.listView_users.Location = new System.Drawing.Point(12, 88);
            this.listView_users.Name = "listView_users";
            this.listView_users.Size = new System.Drawing.Size(400, 151);
            this.listView_users.TabIndex = 0;
            this.listView_users.UseCompatibleStateImageBehavior = false;
            this.listView_users.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 93;
            // 
            // columnHeader_rights
            // 
            this.columnHeader_rights.Text = "权限";
            this.columnHeader_rights.Width = 300;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "针对数据库对象(&O):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_databaseObject
            // 
            this.textBox_databaseObject.Location = new System.Drawing.Point(140, 12);
            this.textBox_databaseObject.Name = "textBox_databaseObject";
            this.textBox_databaseObject.Size = new System.Drawing.Size(272, 21);
            this.textBox_databaseObject.TabIndex = 2;
            // 
            // checkBox_read
            // 
            this.checkBox_read.AutoSize = true;
            this.checkBox_read.Location = new System.Drawing.Point(140, 53);
            this.checkBox_read.Name = "checkBox_read";
            this.checkBox_read.Size = new System.Drawing.Size(54, 16);
            this.checkBox_read.TabIndex = 3;
            this.checkBox_read.Text = "读(&R)";
            // 
            // checkBox_write
            // 
            this.checkBox_write.AutoSize = true;
            this.checkBox_write.Location = new System.Drawing.Point(270, 53);
            this.checkBox_write.Name = "checkBox_write";
            this.checkBox_write.Size = new System.Drawing.Size(54, 16);
            this.checkBox_write.TabIndex = 4;
            this.checkBox_write.Text = "写(&W)";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 23);
            this.label2.TabIndex = 5;
            this.label2.Text = "全部设置为(&S):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(256, 245);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(337, 245);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // ChangeUsersRightsDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(424, 280);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBox_write);
            this.Controls.Add(this.checkBox_read);
            this.Controls.Add(this.textBox_databaseObject);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_users);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangeUsersRightsDlg";
            this.ShowInTaskbar = false;
            this.Text = "ChangeUsersRightsDlg";
            this.Load += new System.EventHandler(this.ChangeUsersRightsDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion


		public void Initial(ServerCollection servers,
			RmsChannelCollection channels,
			DigitalPlatform.StopManager stopManager,
			string serverUrl,
			string strDatabaseObject)
		{
			this.Servers = servers;
			this.Channels = channels;
			this.stopManager = stopManager;
			this.ServerUrl = serverUrl;

			this.textBox_databaseObject.Text = strDatabaseObject;
		}

		// 填充listview
		public int Fill(string strLang,
			out string strError)
		{
			listView_users.Items.Clear();
			strError = "";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(Defs.DefaultUserDb.Name)     // 2007/9/14
                + ":" + "__id'><item><word>"
				+ "" + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>zh</lang></target>";

			RmsChannel channel = Channels.GetChannel(this.ServerUrl);
			if (channel == null)
			{
				strError = "Channels.GetChannel 异常";
				return -1;
			}

            long nRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
                    out strError);
			if (nRet == -1) 
			{
				strError = "检索帐户库时出错: " + strError;
				return -1;
			}

			if (nRet == 0)
				return 0;	// not found

			long lTotalCount = nRet;	// 总命中数
			long lThisCount = lTotalCount;
			long lStart = 0;

			for(;;)
			{

				ArrayList aLine = null;
				nRet = channel.DoGetSearchFullResult(
                    "default",
					lStart,
					lThisCount,
					strLang,
					null,	// stop,
					out aLine,
					out strError);
				if (nRet == -1) 
				{
					strError = "检索注册用户库获取检索结果时出错: " + strError;
					return -1;
				}

				for(int i=0;i<aLine.Count;i++)
				{
					string[] acol = (string[])aLine[i];
					if (acol.Length < 1)
						continue;
					if (acol.Length < 2)
					{
						// 列中没有用户名, 用获取记录来补救?
					}
					ListViewItem item = new ListViewItem(acol[1], 0);
					this.listView_users.Items.Add(item);
					item.SubItems.Add(acol[0]);
				}


				if (lStart + aLine.Count >= lTotalCount)
					break;

				lStart += aLine.Count;
				lThisCount -= aLine.Count;

			}


			return 0;
		}

		private void ChangeUsersRightsDlg_Load(object sender, System.EventArgs e)
		{
			string strError = "";
			int nRet = Fill(this.Lang,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(this, strError);
				return;
			}
		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
		
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
