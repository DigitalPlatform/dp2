using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;

namespace dp2Manager
{
	/// <summary>
	/// Summary description for UserRightsDlg.
	/// </summary>
	public class UserRightsDlg : System.Windows.Forms.Form
	{
		public MainForm MainForm = null;
		public string ServerUrl = "";

		public byte[] TimeStamp = null;
		public string UserRecPath = "";
        public XmlDocument UserRecDom = null;
        private ResRightTree treeView_resRightTree;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_userName;
		private System.Windows.Forms.Button button_resetPassword;
		private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_userRecord;
        private TableLayoutPanel tableLayoutPanel_right;
        private Label label_objectRights_rights;
        private ResRightList listView_resRightList;
        private TextBox textBox_objectRights_rights;
        private Button button_objectRights_editRights;
        private SplitContainer splitContainer_rights;
        private TabControl tabControl_main;
        private TabPage tabPage_generalInfo;
        private TabPage tabPage_objectRights;
        private TabPage tabPage_userRecord;
        private IContainer components;

		public UserRightsDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRightsDlg));
            this.button_resetPassword = new System.Windows.Forms.Button();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_right = new System.Windows.Forms.TableLayoutPanel();
            this.listView_resRightList = new DigitalPlatform.rms.Client.ResRightList();
            this.label_objectRights_rights = new System.Windows.Forms.Label();
            this.textBox_objectRights_rights = new System.Windows.Forms.TextBox();
            this.button_objectRights_editRights = new System.Windows.Forms.Button();
            this.treeView_resRightTree = new DigitalPlatform.rms.Client.ResRightTree();
            this.textBox_userRecord = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.splitContainer_rights = new System.Windows.Forms.SplitContainer();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_generalInfo = new System.Windows.Forms.TabPage();
            this.tabPage_objectRights = new System.Windows.Forms.TabPage();
            this.tabPage_userRecord = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_right.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rights)).BeginInit();
            this.splitContainer_rights.Panel1.SuspendLayout();
            this.splitContainer_rights.Panel2.SuspendLayout();
            this.splitContainer_rights.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_generalInfo.SuspendLayout();
            this.tabPage_objectRights.SuspendLayout();
            this.tabPage_userRecord.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_resetPassword
            // 
            this.button_resetPassword.Location = new System.Drawing.Point(70, 46);
            this.button_resetPassword.Name = "button_resetPassword";
            this.button_resetPassword.Size = new System.Drawing.Size(144, 23);
            this.button_resetPassword.TabIndex = 2;
            this.button_resetPassword.Text = "重设密码(&R)";
            this.button_resetPassword.Click += new System.EventHandler(this.button_resetPassword_Click);
            // 
            // textBox_userName
            // 
            this.textBox_userName.Location = new System.Drawing.Point(70, 19);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(248, 21);
            this.textBox_userName.TabIndex = 1;
            this.textBox_userName.TextChanged += new System.EventHandler(this.textBox_userName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名:";
            // 
            // tableLayoutPanel_right
            // 
            this.tableLayoutPanel_right.ColumnCount = 1;
            this.tableLayoutPanel_right.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_right.Controls.Add(this.listView_resRightList, 0, 3);
            this.tableLayoutPanel_right.Controls.Add(this.label_objectRights_rights, 0, 0);
            this.tableLayoutPanel_right.Controls.Add(this.textBox_objectRights_rights, 0, 1);
            this.tableLayoutPanel_right.Controls.Add(this.button_objectRights_editRights, 0, 2);
            this.tableLayoutPanel_right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_right.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_right.Name = "tableLayoutPanel_right";
            this.tableLayoutPanel_right.RowCount = 4;
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_right.Size = new System.Drawing.Size(338, 273);
            this.tableLayoutPanel_right.TabIndex = 2;
            // 
            // listView_resRightList
            // 
            this.listView_resRightList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_resRightList.FullRowSelect = true;
            this.listView_resRightList.HideSelection = false;
            this.listView_resRightList.Location = new System.Drawing.Point(3, 72);
            this.listView_resRightList.Name = "listView_resRightList";
            this.listView_resRightList.Size = new System.Drawing.Size(332, 198);
            this.listView_resRightList.TabIndex = 3;
            this.listView_resRightList.UseCompatibleStateImageBehavior = false;
            this.listView_resRightList.View = System.Windows.Forms.View.Details;
            // 
            // label_objectRights_rights
            // 
            this.label_objectRights_rights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_objectRights_rights.AutoSize = true;
            this.label_objectRights_rights.Location = new System.Drawing.Point(3, 0);
            this.label_objectRights_rights.Name = "label_objectRights_rights";
            this.label_objectRights_rights.Size = new System.Drawing.Size(332, 12);
            this.label_objectRights_rights.TabIndex = 0;
            this.label_objectRights_rights.Text = "权限(&R):";
            // 
            // textBox_objectRights_rights
            // 
            this.textBox_objectRights_rights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_objectRights_rights.Location = new System.Drawing.Point(3, 15);
            this.textBox_objectRights_rights.Name = "textBox_objectRights_rights";
            this.textBox_objectRights_rights.Size = new System.Drawing.Size(332, 21);
            this.textBox_objectRights_rights.TabIndex = 1;
            this.textBox_objectRights_rights.TextChanged += new System.EventHandler(this.textBox_objectRights_rights_TextChanged);
            // 
            // button_objectRights_editRights
            // 
            this.button_objectRights_editRights.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_objectRights_editRights.Location = new System.Drawing.Point(298, 42);
            this.button_objectRights_editRights.Name = "button_objectRights_editRights";
            this.button_objectRights_editRights.Size = new System.Drawing.Size(37, 24);
            this.button_objectRights_editRights.TabIndex = 2;
            this.button_objectRights_editRights.Text = "...";
            this.button_objectRights_editRights.UseVisualStyleBackColor = true;
            this.button_objectRights_editRights.Click += new System.EventHandler(this.button_objectRights_editRights_Click);
            // 
            // treeView_resRightTree
            // 
            this.treeView_resRightTree.Changed = false;
            this.treeView_resRightTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_resRightTree.HideSelection = false;
            this.treeView_resRightTree.ImageIndex = 0;
            this.treeView_resRightTree.Location = new System.Drawing.Point(0, 0);
            this.treeView_resRightTree.Name = "treeView_resRightTree";
            this.treeView_resRightTree.SelectedImageIndex = 0;
            this.treeView_resRightTree.Size = new System.Drawing.Size(170, 273);
            this.treeView_resRightTree.TabIndex = 0;
            this.treeView_resRightTree.OnSetMenu += new DigitalPlatform.GUI.GuiAppendMenuEventHandle(this.treeView_resRightTree_OnSetMenu);
            this.treeView_resRightTree.OnNodeRightsChanged += new DigitalPlatform.rms.Client.NodeRightsChangedEventHandle(this.treeView_resRightTree_OnNodeRightsChanged);
            this.treeView_resRightTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_resRightTree_AfterSelect);
            // 
            // textBox_userRecord
            // 
            this.textBox_userRecord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_userRecord.HideSelection = false;
            this.textBox_userRecord.Location = new System.Drawing.Point(6, 6);
            this.textBox_userRecord.Multiline = true;
            this.textBox_userRecord.Name = "textBox_userRecord";
            this.textBox_userRecord.ReadOnly = true;
            this.textBox_userRecord.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_userRecord.Size = new System.Drawing.Size(512, 273);
            this.textBox_userRecord.TabIndex = 0;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(383, 329);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(464, 329);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(76, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // splitContainer_rights
            // 
            this.splitContainer_rights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_rights.Location = new System.Drawing.Point(6, 6);
            this.splitContainer_rights.Name = "splitContainer_rights";
            // 
            // splitContainer_rights.Panel1
            // 
            this.splitContainer_rights.Panel1.Controls.Add(this.treeView_resRightTree);
            // 
            // splitContainer_rights.Panel2
            // 
            this.splitContainer_rights.Panel2.Controls.Add(this.tableLayoutPanel_right);
            this.splitContainer_rights.Size = new System.Drawing.Size(512, 273);
            this.splitContainer_rights.SplitterDistance = 170;
            this.splitContainer_rights.TabIndex = 3;
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_generalInfo);
            this.tabControl_main.Controls.Add(this.tabPage_objectRights);
            this.tabControl_main.Controls.Add(this.tabPage_userRecord);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(532, 311);
            this.tabControl_main.TabIndex = 3;
            // 
            // tabPage_generalInfo
            // 
            this.tabPage_generalInfo.Controls.Add(this.button_resetPassword);
            this.tabPage_generalInfo.Controls.Add(this.textBox_userName);
            this.tabPage_generalInfo.Controls.Add(this.label1);
            this.tabPage_generalInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_generalInfo.Name = "tabPage_generalInfo";
            this.tabPage_generalInfo.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_generalInfo.Size = new System.Drawing.Size(524, 285);
            this.tabPage_generalInfo.TabIndex = 0;
            this.tabPage_generalInfo.Text = "一般信息";
            this.tabPage_generalInfo.UseVisualStyleBackColor = true;
            // 
            // tabPage_objectRights
            // 
            this.tabPage_objectRights.Controls.Add(this.splitContainer_rights);
            this.tabPage_objectRights.Location = new System.Drawing.Point(4, 22);
            this.tabPage_objectRights.Name = "tabPage_objectRights";
            this.tabPage_objectRights.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_objectRights.Size = new System.Drawing.Size(524, 285);
            this.tabPage_objectRights.TabIndex = 1;
            this.tabPage_objectRights.Text = "对象权限";
            this.tabPage_objectRights.UseVisualStyleBackColor = true;
            // 
            // tabPage_userRecord
            // 
            this.tabPage_userRecord.Controls.Add(this.textBox_userRecord);
            this.tabPage_userRecord.Location = new System.Drawing.Point(4, 22);
            this.tabPage_userRecord.Name = "tabPage_userRecord";
            this.tabPage_userRecord.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_userRecord.Size = new System.Drawing.Size(524, 285);
            this.tabPage_userRecord.TabIndex = 2;
            this.tabPage_userRecord.Text = "用户记录";
            this.tabPage_userRecord.UseVisualStyleBackColor = true;
            // 
            // UserRightsDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(556, 369);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UserRightsDlg";
            this.ShowInTaskbar = false;
            this.Text = "用户管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.UserRightsDlg_Closing);
            this.Load += new System.EventHandler(this.UserRightsDlg_Load);
            this.tableLayoutPanel_right.ResumeLayout(false);
            this.tableLayoutPanel_right.PerformLayout();
            this.splitContainer_rights.Panel1.ResumeLayout(false);
            this.splitContainer_rights.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_rights)).EndInit();
            this.splitContainer_rights.ResumeLayout(false);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_generalInfo.ResumeLayout(false);
            this.tabPage_generalInfo.PerformLayout();
            this.tabPage_objectRights.ResumeLayout(false);
            this.tabPage_userRecord.ResumeLayout(false);
            this.tabPage_userRecord.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		private void UserRightsDlg_Load(object sender, System.EventArgs e)
		{
			string strError = "";
			string strXml = "";


            if (this.MainForm == null)
            {
                throw (new Exception("MainForm成员尚未初始化"));
            }

            if (this.ServerUrl == "")
            {
                throw (new Exception("ServerUrl成员尚未初始化"));
            }

            if (this.UserName != "" || this.UserRecPath != "")
            {
                // 获得被管理的帐户记录
                int nRet = 0;

                if (this.UserRecPath != "")
                {
                    nRet = MainForm.GetUserRecord(
                         this.ServerUrl,
                         this.UserRecPath,
                         out strXml,
                         out this.TimeStamp,
                         out strError);
                    Debug.Assert(nRet <= 1, "nRet绝对不会大于1");
                }
                else
                {
                    nRet = MainForm.GetUserRecord(
                         this.ServerUrl,
                         this.UserName,
                         out this.UserRecPath,
                         out strXml,
                         out this.TimeStamp,
                         out strError);
                    if (nRet > 1)
                    {
                        strError = "用户名为 '" + this.UserName + "' 的帐户记录不止一条，为 " + Convert.ToString(nRet) + " 条，属于不正常情况，请尽快用帐户记录id进行维护操作。";
                        goto ERROR1;
                    }
                }

                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    strError = "帐户记录不存在";
                    goto ERROR1;
                }


                this.UserRecDom = new XmlDocument();
                try
                {
                    this.UserRecDom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("用户记录XML装载到dom时出错: " + ex.Message);
                    // MessageBox.Show("XML记录体: " + strXml);
                    this.textBox_userRecord.Text = strXml;
                    this.tabPage_userRecord.Focus();
                    return;
                }
            }
            else
            {
                this.UserRecDom = new XmlDocument();
                this.UserRecDom.LoadXml("<record><name /><password /></record>");

                // UserRecPath    为空
                // TimeStamp    为空


                // 设置用户名
                DomUtil.SetElementText(UserRecDom.DocumentElement,
                    "name",
                    this.UserName);


                // 一开始是空密码
                DomUtil.SetElementText(UserRecDom.DocumentElement,
                   "password",
                    Cryptography.GetSHA1(""));

            }

            this.textBox_userName.Text = DomUtil.GetElementText(UserRecDom.DocumentElement, "name");


            UpdateXmlTextDisplay();

            //
            treeView_resRightTree.Initial(
                MainForm.Servers,
                MainForm.Channels,
                MainForm.stopManager,
                this.ServerUrl,
                this.UserRecDom);
            treeView_resRightTree.PropertyCfgFileName = "userrightsdef.xml";

            listView_resRightList.Initial(treeView_resRightTree);

            if (treeView_resRightTree.SelectedNode == null && treeView_resRightTree.Nodes.Count > 0)
                treeView_resRightTree.SelectedNode = treeView_resRightTree.Nodes[0];

            return;
            ERROR1:
            MessageBox.Show(strError);
            return;

        }

        private void treeView_resRightTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (treeView_resRightTree.SelectedNode == null)
                return;

			// NodeInfo nodeinfo = (NodeInfo)treeView_resRightTree.SelectedNode.Tag;

			// bool bExpandable = false;
			
			// if (nodeinfo != null)
			// 	bExpandable = nodeinfo.Expandable;

            ResPath respath = new ResPath(treeView_resRightTree.SelectedNode);

            this.textBox_objectRights_rights.Text = ResRightTree.GetNodeRights(treeView_resRightTree.SelectedNode);
            this.label_objectRights_rights.Text = "对象 '" + respath.Path + "' 的权限(&R)";

            if (ResRightTree.GetNodeExpandable(treeView_resRightTree.SelectedNode) == false)
            {
                listView_resRightList.Items.Clear();
            }
            else
            {

                listView_resRightList.Path = respath.Path;
                listView_resRightList.Fill();
            }
		}


		private void button_resetPassword_Click(object sender, System.EventArgs e)
		{
			ResetPasswordDlg dlg = new ResetPasswordDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.ShowDialog(this);
			if (dlg.DialogResult != DialogResult.OK)
				return;

			DomUtil.SetElementText(UserRecDom.DocumentElement,
				"password",
				Cryptography.GetSHA1(dlg.textBox_password.Text));

            UpdateXmlTextDisplay();
		}

		// 保存修改
		private void button_OK_Click(object sender, System.EventArgs e)
		{
            string strError = "";
            long lRet = 0;

            if (this.UserName == "")
            {
                MessageBox.Show(this, "用户名不能为空。");
                goto ERROR1;
            }

            // 兑现本次树上修改
            if (treeView_resRightTree.Changed == true)
                treeView_resRightTree.FinishRightsParam();


            // MessageBox.Show(this, DomUtil.GetIndentXml(this.UserRecDom));

            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "Channels.GetChannel 异常";
                goto ERROR1;
            }



            // UserRecPath为空表示为新创建账户，需要查重
            if (this.UserRecPath == "")
            {
                // 查重
                // return:
                //      -1  出错
                //      其他   检索命中的记录数。只要大于1，就表示有多于一条的存在。
                lRet = SearchUserNameDup(
                    this.UserName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet >= 1)
                {
                    strError = "用户名 '" + this.UserName + "' 已经存在，无法创建新的用户记录。";
                    goto ERROR1;
                }
            }

            // 旧版本的<rightsItem>需要去掉
            XmlNode node = this.UserRecDom.DocumentElement.SelectSingleNode("rightsItem");
            if (node != null)
                node.ParentNode.RemoveChild(node);

            string strXml = DomUtil.GetIndentXml(this.UserRecDom);
            string strOutputPath = "";
            byte[] baOutputTimeStamp;

            lRet = channel.DoSaveTextRes(
                this.UserRecPath == "" ? 
                (Defs.DefaultUserDb.Name + "/" +"?") : this.UserRecPath,
                strXml,
                false,	// bInlucdePreamble
                "",	// style
                this.TimeStamp,	// baTimeStamp,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            this.TimeStamp = baOutputTimeStamp;

            treeView_resRightTree.Changed = false;

            // MessageBox.Show("帐户记录保存成功。");

            // 保存成功后再查重一次
            lRet = SearchUserNameDup(
                this.UserName,
                out strError);
            if (lRet == -1)
                goto ERROR1;
            if (lRet > 1)
            {
                strError = "警告：用户名 '" + this.UserName + "' 目前存在" + Convert.ToString(lRet) + "条记录。这是不正常的情况，会引起这个用户名无法登录，请尽快修正。";
                MessageBox.Show(this, strError);
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(strError);
            return;

        }

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();

		}

		private void UserRightsDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (treeView_resRightTree.Changed == true)
			{
				DialogResult result = MessageBox.Show(this,
					"当前对话框有修改内容尚未保存。确实要关闭对话框? (此时关闭所有修改内容将丢失)",
					"dp2manager",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button2);
				if (result != DialogResult.Yes)
				{
					e.Cancel = true;
					return;
				}
			}
		}

        // 查重
        // return:
        //      -1  出错
        //      其他   检索命中的记录数。只要大于1，就表示有多于一条的存在。
        public long SearchUserNameDup(
            string strUserName,
            out string strError)
        {
            strError = "";

            string strQueryXml = "<target list='" + Defs.DefaultUserDb.Name + ":"
                + Defs.DefaultUserDb.SearchPath.UserName + "'><item><word>"
                + strUserName + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>chi</lang></target>";

            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "Channels.GetChannel 异常";
                return -1;
            }

            long nRet = channel.DoSearch(strQueryXml,
                "default",
                out strError);
            if (nRet == -1)
            {
                strError = "检索数据库'" + Defs.DefaultUserDb.Name + "'时出错: " + strError;
                return -1;
            }

            return nRet;
        }

        private void textBox_userName_TextChanged(object sender, EventArgs e)
        {
            if (UserRecDom != null)
            {
                // 设置用户名
                DomUtil.SetElementText(UserRecDom.DocumentElement,
                    "name",
                    this.UserName);

                UpdateXmlTextDisplay();
            }

        }

        void UpdateXmlTextDisplay()
        {
            this.textBox_userRecord.Text = DomUtil.GetIndentXml(this.UserRecDom);
        }

        // 被管理的用户名
        public string UserName 
        {
            get
            {
                return this.textBox_userName.Text;
            }
            set
            {
                this.textBox_userName.Text = value;
            }
        }

        private void treeView_resRightTree_OnSetMenu(object sender, GuiAppendMenuEventArgs e)
        {
            Debug.Assert(e.ContextMenu != null, "e不能为null");

            MenuItem menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);

            TreeNode node = this.treeView_resRightTree.SelectedNode;
            string strText = "快速设置权限(&R)";

            if (node == null || node.ImageIndex == ResTree.RESTYPE_DB)
                strText = "快速设置权限[数据库整体](&R)";
            else
                strText = "快速设置权限[对象'" + node.Text + "'](&R)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.menu_quickSetRights_Click);

            e.ContextMenu.MenuItems.Add(menuItem);
        }

        void menu_quickSetRights_Click(object sender, EventArgs e)
        {
            /*
            // 兑现本次树上修改
            if (treeView_resRightTree.Changed == true)
                treeView_resRightTree.FinishRightsParam();
            */

            TreeNode node = this.treeView_resRightTree.SelectedNode;
            if (node == null)
                node = this.treeView_resRightTree.Nodes[0];

            QuickSetDatabaseRightsDlg dlg = new QuickSetDatabaseRightsDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.CfgFileName = "quickrights.xml";
            dlg.AllObjectNames = new List<ObjectInfo>();

            // 根
            ObjectInfo objectinfo = new ObjectInfo();
            objectinfo.Path = "服务器";
            objectinfo.ImageIndex = ResTree.RESTYPE_SERVER;
            dlg.AllObjectNames.Add(objectinfo);

            for (int i = 0; i < treeView_resRightTree.Nodes[0].Nodes.Count; i++)
            {
                objectinfo = new ObjectInfo();
                objectinfo.Path = treeView_resRightTree.Nodes[0].Nodes[i].Text;
                objectinfo.ImageIndex = treeView_resRightTree.Nodes[0].Nodes[i].ImageIndex;
                dlg.AllObjectNames.Add(objectinfo);
            }

            dlg.SelectedObjectNames = new List<ObjectInfo>();
            if (node.ImageIndex == ResTree.RESTYPE_DB)
            {
                objectinfo = new ObjectInfo();
                objectinfo.Path = node.Text;
                objectinfo.ImageIndex = node.ImageIndex;

                // 如果选定的是数据库节点
                dlg.SelectedObjectNames.Add(objectinfo);
            }
            else if (node.ImageIndex == ResTree.RESTYPE_SERVER)
            {
                // 如果选定的是服务器节点
                dlg.SelectedObjectNames.AddRange(dlg.AllObjectNames);
            }
            else
            {
                // 如果选定的是其他类型节点
                // 得到路径
                string strPath = "";
                TreeNode nodeCur = node;
                while(nodeCur != null)
                {
                    if (nodeCur.ImageIndex == ResTree.RESTYPE_SERVER)
                        break;
                    if (strPath != "")
                        strPath = "/" + strPath;
                    strPath = nodeCur.Text + strPath;
                    nodeCur = nodeCur.Parent;
                }

                objectinfo = new ObjectInfo();
                objectinfo.Path = strPath;
                objectinfo.ImageIndex = node.ImageIndex;


                dlg.SelectedObjectNames.Add(objectinfo);
            }

    
            this.MainForm.AppInfo.LinkFormState(dlg, "QuickSetDatabaseRightsDlg_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;


            // 兑现修改
            ModiRights(dlg.SelectedObjectNames,
                dlg.QuickRights);

            this.listView_resRightList.RefreshList();

            this.textBox_objectRights_rights.Text = ResRightTree.GetNodeRights(treeView_resRightTree.SelectedNode);

        }

        // ????????
        void ModiRights(List<ObjectInfo> aName,
            QuickRights quickrights)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            for (int i = 0; i < aName.Count; i++)
            {
                ObjectInfo objectinfo = aName[i];
                string strName = objectinfo.Path;

                // 将名字转换为节点对象指针
                TreeNode node = null;

                if (objectinfo.ImageIndex == ResTree.RESTYPE_SERVER)
                    node = this.treeView_resRightTree.Nodes[0];
                else
                    node = TreeViewUtil.GetTreeNode(this.treeView_resRightTree.Nodes[0], strName);
                if (node == null)
                {
                    MessageBox.Show(this, "节点路径 '" +strName+ "' 没有找到对应的对象...");
                    continue;
                }

                nodes.Add(node);
            }

            ModiRights(nodes, quickrights);
        }

        // 按照预定模式修改一个节点以及以下的全部子节点的权限
        void ModiRights(List<TreeNode> nodes,
            QuickRights quickrights)
        {
            quickrights.GetNodeStyle += new GetNodeStyleEventHandle(quickrights_GetNodeStyle);
            quickrights.SetRights +=new SetRightsEventHandle(quickrights_SetRights);
            try
            {
                quickrights.ModiRights(nodes, this.treeView_resRightTree.Nodes[0]);
            }
            finally
            {
                quickrights.GetNodeStyle -= new GetNodeStyleEventHandle(quickrights_GetNodeStyle);
                quickrights.SetRights -= new SetRightsEventHandle(quickrights_SetRights);
            }

            /*
            for (int j = 0; j < quickrights.Count; j++)
            {
                QuickRightsItem item = quickrights[j];

                bool bRet = QuickRights.MatchType(parent.ImageIndex,
                    item.Type);
                if (bRet == false)
                    continue;

                if (item.Style != 0)
                {
                    if (item.Style != ResRightTree.GetNodeStyle(parent))
                        continue;
                }

                this.treeView_resRightTree.SetNodeRights(parent,
                    item.Rights);

            }


            for (int i = 0; i < parent.Nodes.Count; i++)
            {
                TreeNode node = parent.Nodes[i];
                ModiRights(node,
                    quickrights);
            }
             */
        }

        void quickrights_GetNodeStyle(object sender, GetNodeStyleEventArgs e)
        {
            e.Style = ResRightTree.GetNodeStyle(e.Node);
        }

        
        void quickrights_SetRights(object sender, SetRightsEventArgs e)
        {
            this.treeView_resRightTree.SetNodeRights(e.Node,
                   e.Rights);
        }
        

        /*
        void quickrights_GetTreeNodeByPath(object sender, GetTreeNodeByPathEventArgs e)
        {
            e.Node = TreeViewUtil.GetTreeNode(this.treeView_resRightTree.Nodes[0], e.Path);
        }
         */

        private void button_objectRights_editRights_Click(object sender, EventArgs e)
        {
            string strRights = "";
            DialogResult result = this.treeView_resRightTree.NodeRightsDlg(null,
                out strRights);

            if (result == DialogResult.OK)
                this.textBox_objectRights_rights.Text = strRights;

        }

        private void textBox_objectRights_rights_TextChanged(object sender, EventArgs e)
        {
            this.treeView_resRightTree.SetNodeRights(treeView_resRightTree.SelectedNode,
                this.textBox_objectRights_rights.Text);
        }

        // editbox跟踪对象Tree上的修改
        private void treeView_resRightTree_OnNodeRightsChanged(object sender, NodeRightsChangedEventArgs e)
        {
            if (e.Node == this.treeView_resRightTree.SelectedNode)
            {
                if (this.textBox_objectRights_rights.Text != e.Rights)
                    this.textBox_objectRights_rights.Text = e.Rights;
            }
        }

	}
}
