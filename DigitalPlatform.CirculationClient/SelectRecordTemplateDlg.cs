using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CirculationClient
{
	/// <summary>
    /// 选择新记录模板名的对话框
    /// 本对话框根据一个xml模板文件，列出其中<template>元素的name属性值，让用户选择，
	/// 最后对话框将选择的元素以及其下全部元素创建一个新的xml文档(字符串形式).
	/// 本对话框不负责从服务获得文件。
	/// </summary>
	public class SelectRecordTemplateDlg : System.Windows.Forms.Form
	{
        const int WM_AUTO_CLOSE = API.WM_USER + 200;
        public bool AutoClose = false;  // 对话框口打开后立即关闭?

        // 2008/6/24 new add
        public bool SaveMode = false;   // 是否为保存模式？

		public ApplicationInfo ap = null;	// 引用
		public string ApCfgTitle = "";	// 在ap中保存窗口外观状态的标题字符串


		// public string InputXml = "";
		// public string OutputXml = "";

		public string SelectedRecordXml = "";

		public bool CheckNameExist = true;

		XmlDocument dom = null;
		bool m_bChanged = false;	// DOM内容是否有变化

        private DigitalPlatform.GUI.ListViewNF listView1;
		private System.Windows.Forms.ColumnHeader columnHeader_name;
		private System.Windows.Forms.ColumnHeader columnHeader_comment;
		private System.Windows.Forms.CheckBox checkBox_notAsk;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textBox_name;

		private System.ComponentModel.Container components = null;

		public SelectRecordTemplateDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectRecordTemplateDlg));
            this.listView1 = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBox_notAsk = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_comment});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(9, 9);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(403, 233);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "模板名";
            this.columnHeader_name.Width = 200;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 300;
            // 
            // checkBox_notAsk
            // 
            this.checkBox_notAsk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_notAsk.AutoSize = true;
            this.checkBox_notAsk.Enabled = false;
            this.checkBox_notAsk.Location = new System.Drawing.Point(9, 277);
            this.checkBox_notAsk.Name = "checkBox_notAsk";
            this.checkBox_notAsk.Size = new System.Drawing.Size(144, 16);
            this.checkBox_notAsk.TabIndex = 1;
            this.checkBox_notAsk.Text = "下次不再出现此对话框";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(337, 247);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(337, 273);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 249);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "模板名(&N):";
            // 
            // textBox_name
            // 
            this.textBox_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_name.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_name.Location = new System.Drawing.Point(85, 247);
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(247, 21);
            this.textBox_name.TabIndex = 5;
            this.textBox_name.TextChanged += new System.EventHandler(this.textBox_name_TextChanged);
            // 
            // SelectRecordTemplateDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(421, 304);
            this.Controls.Add(this.textBox_name);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_notAsk);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelectRecordTemplateDlg";
            this.ShowInTaskbar = false;
            this.Text = "请选择新记录模板";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SelectRecordTemplateDlg_Closing);
            this.Closed += new System.EventHandler(this.SelectRecordTemplateDlg_Closed);
            this.Load += new System.EventHandler(this.SelectRecordTemplateDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void SelectRecordTemplateDlg_Load(object sender, System.EventArgs e)
		{
			if (ap != null) 
			{
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

			if (dom != null)
			{
				FillList(true);
			}
			else 
			{
				Debug.Assert(true, "你一定忘记了先用Initial()");
			}

            if (this.SaveMode == false)
                this.checkBox_notAsk.Enabled = true;

            if (this.AutoClose == true)
                API.PostMessage(this.Handle, WM_AUTO_CLOSE, 0, 0);

		}

		private void SelectRecordTemplateDlg_Closed(object sender, System.EventArgs e)
		{
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

		public int Initial(
            bool bSaveMode,
            string strInputXml,
			out string strError)
		{
			strError = "";

            this.SaveMode = bSaveMode;

			dom = new XmlDocument();

			try 
			{
				dom.LoadXml(strInputXml);
			}
			catch (Exception ex)
			{
				strError = ex.Message;
				return -1;
			}

			return 0;
		}

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_AUTO_CLOSE:
                    this.button_OK_Click(this, null);
                    return;
            }
            base.DefWndProc(ref m);
        }


		void FillList(bool bAutoSelect)
		{
			listView1.Items.Clear();
			listView1_SelectedIndexChanged(null, null);

			XmlNodeList nodes = dom.DocumentElement.SelectNodes("template");

			for(int i=0;i<nodes.Count; i++) 
			{
				string strName = DomUtil.GetAttr(nodes[i], "name");
				string strComment =  DomUtil.GetAttr(nodes[i], "comment");

				ListViewItem item = new ListViewItem(strName, 0);

				listView1.Items.Add(item);

				item.SubItems.Add(strComment);
			}

			// 选择第一项
			if (bAutoSelect == true) 
			{
				if (listView1.Items.Count != 0)
					listView1.Items[0].Selected = true;
			}

		}

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			// 如果m_bChanged == true，允许空白着OK退出
			if (m_bChanged == false && textBox_name.Text == "")
			{
                MessageBox.Show(this, "尚未指定模板名");
				return ;
			}

            /*
            if (checkBox_delete.Checked == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "确实要删除下列模板记录?\r\n\r\n" + textBox_name.Text,
                    "SelectRecordTemplateDlg",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, 
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
                goto END1;
            }
            */


            string strName = textBox_name.Text;

			XmlNode node = dom.DocumentElement.SelectSingleNode("template[@name='" + strName + "']");

			if (CheckNameExist == true 
				&& node == null) 
			{
                MessageBox.Show(this, "模板名 '" + strName + "在模板文件中不存在...");
				// MessageBox.Show(this, "SelectSingleNode()失败...");
				return;
			}

			if (node != null) 
			{
				if (node.ChildNodes.Count == 0) 
				{
					MessageBox.Show(this, "<template name='"+strName+"'>元素下必须有一个儿子节点，这个节点将充当根节点...");
					return;
				}

				SelectedRecordXml = node.ChildNodes[0].OuterXml;
			}
			else
			{
				SelectedRecordXml = "";
			}

			//END1:
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void listView1_DoubleClick(object sender, System.EventArgs e)
		{
			button_OK_Click(null, null);
		}

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (listView1.SelectedItems.Count == 0)
			{
				textBox_name.Text = "";
			}
			else 
			{
				/*
				if () // 复选
				{
					textBox_name.Text = "";

					for(int i=0;i<listView1.SelectedItems.Count;i++)
					{
						if (textBox_name.Text != "")
							textBox_name.Text += ",";

						textBox_name.Text += listView1.SelectedItems[i].Text;
					}

				}
				else
				*/
					textBox_name.Text = listView1.SelectedItems[0].Text;
			}
		
		}

		private void textBox_name_TextChanged(object sender, System.EventArgs e)
		{
			/*
			if (textBox_name.Text != "")
			{
				button_OK.Enabled = true;
			}
			else 
			{
				button_OK.Enabled = false;
			}
			*/
		}

		// 替换或者追加一个记录
		public int ReplaceRecord(string strName,
			string strContent,
			out string strError)
		{
			strError = "";
//			strOutputXml = "";

			if (dom == null)
			{
				strError = "dom为null";
				return -1;
			}

			XmlNode node = dom.DocumentElement.SelectSingleNode("template[@name='" + strName + "']");

			if (node == null) 
			{
				
				node = dom.CreateElement("template");
				DomUtil.SetAttr(node, "name", strName);
				// 新建立一个记录
				node = dom.DocumentElement.AppendChild(node);
			}


			// 要防止strContent是全XML文件内容
			XmlDocument temp = new XmlDocument();
			try 
			{
				temp.LoadXml(strContent);
			}
			catch (Exception ex)
			{
				strError = ex.Message;
				return -1;
			}

			node.InnerXml = temp.DocumentElement.OuterXml;	// 根的纯XML

			m_bChanged = true;

			// strOutputXml = dom.DocumentElement.OuterXml;	// DomUtil.GetXml(dom);

			return 0;
		}

		/*
		// 删除若干记录
		public int DeleteRecords(string strNameList,
			out string strOutputXml,
			out string strError)
		{
			strError = "";
			strOutputXml = "";

			if (dom == null)
			{
				strError = "dom为null";
				return -1;
			}

			string[] aName = strNameList.Split(new Char [] {','});


			for(int i=0;i<aName.Length;i++)
			{
				string strName = aName[i].Trim();
				if (strName == "")
					continue;

				XmlNode node = dom.DocumentElement.SelectSingleNode("template[@name='" + strName + "']");

				if (node == null) 
					continue;

				node.ParentNode.RemoveChild(node);
			}

			strOutputXml = dom.DocumentElement.OuterXml;	// DomUtil.GetXml(dom);

			return 0;
		}
		*/

		private void listView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			bool bSelected = listView1.SelectedItems.Count > 0;

			//
			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.menu_Modify);
			if (bSelected == false || this.SaveMode == false) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.menu_deleteRecord);
			if (bSelected == false || this.SaveMode == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			contextMenu.Show(listView1, new Point(e.X, e.Y) );		
			
		}

		// 修改名字和注释
		void menu_Modify(object sender, System.EventArgs e)
		{
			if (listView1.SelectedItems.Count == 0)
			{
                MessageBox.Show(this, "尚未选择拟修改的模板记录事项...");
				return;
			}
			TemplateRecordDlg dlg = new TemplateRecordDlg();

			string strOldName = ListViewUtil.GetItemText(listView1.SelectedItems[0], 0);

			dlg.TemplateName = ListViewUtil.GetItemText(listView1.SelectedItems[0], 0);
			dlg.TemplateComment = ListViewUtil.GetItemText(listView1.SelectedItems[0], 1);

			dlg.ShowDialog(this);
			if (dlg.DialogResult != DialogResult.OK)
				return;

			string strError = "";
			int nRet = ChangeRecordProperty(strOldName, 
				dlg.TemplateName,
				dlg.TemplateComment,
				out strError);
			if (nRet == -1) 
			{
				MessageBox.Show(this, strError);
				return;
			}

			FillList(false);

		}
	
		void menu_deleteRecord(object sender, System.EventArgs e)
		{
			if (listView1.SelectedItems.Count == 0)
			{
                MessageBox.Show(this, "尚未选择拟删除的模板记录事项...");
				return;
			}

			string strError = "";
			int nRet = 0;

			DialogResult result = MessageBox.Show(this,
                "确实要删除所选择的模板记录?",
				"SelectRecordTemplateDlg",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button2);
			if (result != DialogResult.Yes)
				return;

			
			foreach(ListViewItem item in listView1.SelectedItems)
			{
				string strOldName = ListViewUtil.GetItemText(item, 0);
	
				nRet = ChangeRecordProperty(strOldName, 
					null,
					null,
					out strError);
				if (nRet == -1) 
				{
					MessageBox.Show(this, strError);
					return;
				}
			}

			FillList(false);
		}

		// 修改DOM中的记录属性，或者删除DOM中的记录
		// parameters:
		//		strNewName	如果==null，表示删除此记录
		int ChangeRecordProperty(string strOldName,
			string strNewName,
			string strNewComment,
			out string strError)
		{

			strError = "";

			if (dom == null)
			{
				strError = "dom为null";
				return -1;
			}

			XmlNode node = dom.DocumentElement.SelectSingleNode("template[@name='" + strOldName + "']");

			if (node == null) 
			{
                strError = "模板记录 '" + strOldName + "' 没有找到...";
				return -1;
			}

			if (strNewName == null || strNewName == "")
			{
				node.ParentNode.RemoveChild(node);
			}
			else 
			{
				DomUtil.SetAttr(node, "name", strNewName);
				DomUtil.SetAttr(node, "comment", strNewComment);
			}

			m_bChanged = true;

			return 0;
		}

		private void SelectRecordTemplateDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.DialogResult != DialogResult.OK
				&& m_bChanged == true)
			{
				DialogResult result = MessageBox.Show(this,
                    "确实要放弃先前所做的全部修改么?\r\n\r\n(是)放弃修改 (否)不关闭窗口\r\n\r\n(注: 模板名为空的情况下仍可以按\"确定\"按钮保存所做的修改。)",
					"SelectRecordTemplateDlg",
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

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
		{
			get 
			{
				return m_bChanged;
			}
		}
	
		public string OutputXml
		{
			get 
			{
				return dom.DocumentElement.OuterXml;	// DomUtil.GetXml(dom);
			}
		}

        public string SelectedName
        {
            get
            {
                return this.textBox_name.Text;
            }
            set
            {
                this.textBox_name.Text = value;
            }
        }

        public bool NotAsk
        {
            get
            {
                return this.checkBox_notAsk.Checked;
            }
            set
            {
                this.checkBox_notAsk.Checked = value;
            }
        }
	}
}
