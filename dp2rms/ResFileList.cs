using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml;
using System.IO;

using DigitalPlatform;
//using DigitalPlatform.XmlEditor;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Range;
using DigitalPlatform.Xml;
using DigitalPlatform.rms;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;

namespace dp2rms
{
	public class ResFileList : ListViewNF
	{
		public XmlEditor editor = null;	// 本对象所关联的XmlEditor

		public const int COLUMN_ID = 0;
		public const int COLUMN_STATE = 1;
		public const int COLUMN_LOCALPATH = 2;
		public const int COLUMN_SIZE = 3;
		public const int COLUMN_MIME = 4;
		public const int COLUMN_TIMESTAMP = 5;

		bool bNotAskTimestampMismatchWhenOverwrite = false;

		public Delegate_DownloadFiles procDownloadFiles = null;
		public Delegate_DownloadOneMetaData procDownloadOneMetaData = null;


		Hashtable m_tableFileId = new Hashtable();

		bool m_bChanged = false;

		private System.Windows.Forms.ColumnHeader columnHeader_state;
		private System.Windows.Forms.ColumnHeader columnHeader_serverName;
		private System.Windows.Forms.ColumnHeader columnHeader_localPath;
		private System.Windows.Forms.ColumnHeader columnHeader_size;
		private System.Windows.Forms.ColumnHeader columnHeader_mime;
		private System.Windows.Forms.ColumnHeader columnHeader_timestamp;

		private System.ComponentModel.Container components = null;

		public ResFileList()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.columnHeader_state = new System.Windows.Forms.ColumnHeader();
			this.columnHeader_serverName = new System.Windows.Forms.ColumnHeader();
			this.columnHeader_localPath = new System.Windows.Forms.ColumnHeader();
			this.columnHeader_size = new System.Windows.Forms.ColumnHeader();
			this.columnHeader_mime = new System.Windows.Forms.ColumnHeader();
			this.columnHeader_timestamp = new System.Windows.Forms.ColumnHeader();
			// 
			// columnHeader_state
			// 
			this.columnHeader_state.Text = "状态";
			this.columnHeader_state.Width = 100;
			// 
			// columnHeader_serverName
			// 
			this.columnHeader_serverName.Text = "服务器端别名";
			this.columnHeader_serverName.Width = 200;
			// 
			// columnHeader_localPath
			// 
			this.columnHeader_localPath.Text = "本地物理路径";
			this.columnHeader_localPath.Width = 200;
			// 
			// columnHeader_size
			// 
			this.columnHeader_size.Text = "尺寸";
			this.columnHeader_size.Width = 100;
			// 
			// columnHeader_mime
			// 
			this.columnHeader_mime.Text = "媒体类型";
			this.columnHeader_mime.Width = 200;
			// 
			// columnHeader_timestamp
			// 
			this.columnHeader_timestamp.Text = "时间戳";
			this.columnHeader_timestamp.Width = 200;
			// 
			// ResFileList
			// 
			this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																			  this.columnHeader_serverName,
																			  this.columnHeader_state,
																			  this.columnHeader_localPath,
																			  this.columnHeader_size,
																			  this.columnHeader_mime,
																			  this.columnHeader_timestamp});
			this.FullRowSelect = true;
			this.HideSelection = false;
			this.View = System.Windows.Forms.View.Details;
			this.DoubleClick += new System.EventHandler(this.ResFileList_DoubleClick);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ResFileList_MouseUp);

		}
		#endregion


		public bool Changed
		{
			get 
			{
				return m_bChanged;
			}
			set 
			{
				m_bChanged = value;
			}
		}


		// 初始化列表内容
		// parameters:
		public void Initial(XmlEditor editor)
		{
			//this.Items.Clear();
			m_tableFileId.Clear();

			this.editor = editor;

			this.editor.ItemCreated -=  new ItemCreatedEventHandle(this.ItemCreated);
			this.editor.ItemChanged -=  new ItemChangedEventHandle(this.ItemChanged);
			this.editor.BeforeItemCreate -=  new BeforeItemCreateEventHandle(this.BeforeItemCreate);
			this.editor.ItemDeleted -= new ItemDeletedEventHandle(this.ItemDeleted);


			this.editor.ItemCreated +=  new ItemCreatedEventHandle(this.ItemCreated);
			this.editor.ItemChanged +=  new ItemChangedEventHandle(this.ItemChanged);
			this.editor.BeforeItemCreate +=  new BeforeItemCreateEventHandle(this.BeforeItemCreate);
			this.editor.ItemDeleted += new ItemDeletedEventHandle(this.ItemDeleted);


		}

		#region 接管的事件

		bool IsFileElement(DigitalPlatform.Xml.Item item)
		{
			if (!(item is ElementItem))
				return false;

			ElementItem element = (ElementItem)item;

			if (element.LocalName != "file")
				return false;

			if (element.NamespaceURI == DpNs.dprms )
				return true;

			return false;
		}

		// 接管XmlEditor中各种渠道创建<file>对象的事件
		void BeforeItemCreate(object sender,
			DigitalPlatform.Xml.BeforeItemCreateEventArgs e)
		{
			/*
			if (!(e.item is ElementItem))
				return;

			if (IsFileElement(e.item) == false)
				return;

			ResObjectDlg dlg = new ResObjectDlg();
			dlg.ShowDialog(this);
			if (dlg.DialogResult != DialogResult.OK) 
			{
				e.Cancel = true;
				return;
			}

			SetItemProperty(editor, 
				(ElementItem)e.item,
				dlg.textBox_mime.Text,
				dlg.textBox_localPath.Text,
				dlg.textBox_size.Text);

			return;
			*/
		}


		void ItemCreated(object sender,
			DigitalPlatform.Xml.ItemCreatedEventArgs e)
		{
			if (e.item is AttrItem)
			{
				ElementItem parent = e.item.parent;

				if (parent == null)
					return;

				if (this.IsFileElement(parent) == false)
					return;

				/*
				string strId = parent.GetAttrValue("id");
				if (strId == null || strId == "")
					return;
				*/

				if (e.item.Name == "id") 
				{
					ChangeFileAttr((AttrItem)e.item,
						"",
						e.item.Value);
				}
				else 
				{
					ChangeFileAttr((AttrItem)e.item,
						null,
						e.item.Value);
				}

				return;
			}


			if (!(e.item is ElementItem))
				return;

			if (IsFileElement(e.item) == false)
				return;

			ElementItem element = (ElementItem)e.item;


			// 看看创建时是否已经有id属性
			string strID = element.GetAttrValue("id");

			// 客户端
			if (strID == null || strID == "")
			{
				NewLine(element,
					true);

				ResObjectDlg dlg = new ResObjectDlg();
                dlg.Font = GuiUtil.GetDefaultFont();
                dlg.ShowDialog(this);
				if (dlg.DialogResult != DialogResult.OK) 
				{
					// e.Cancel = true;
					// 删除刚刚创建的element
					ElementItem parent = element.parent;
					parent.Remove(element);
					return;
				}

				// 直接对xmleditor进行修改
				element.SetAttrValue("__mime",dlg.textBox_mime.Text);
				element.SetAttrValue("__localpath",dlg.textBox_localPath.Text);
				element.SetAttrValue("__size",dlg.textBox_size.Text);

				strID =  NewFileId();

				// 用到了id
				if (m_tableFileId.Contains((object)strID) == false)
					m_tableFileId.Add(strID, (object)true);

				element.SetAttrValue("id", strID);

				/*
				SetItemProperty(editor, 
					(ElementItem)e.item,
					dlg.textBox_mime.Text,
					dlg.textBox_localPath.Text,
					dlg.textBox_size.Text);
				NewLine((ElementItem)e.item,
					true);
				*/

			}
			else // 来自服务器端的
			{

				string strState = element.GetAttrValue("__state");

				if (strState == null || strState == "")
				{
					NewLine(element,
						false);
					GetMetaDataParam(element);
				}
				else 
				{
					
					NewLine(element,
						IsNewFileState(strState));


					// 跟踪全部xml属性
					ChangeLine(strID, 
						null,	// newid
						element.GetAttrValue("__state"),
						element.GetAttrValue("__localpath"),
						element.GetAttrValue("__mime"),
						element.GetAttrValue("__size"),
						element.GetAttrValue("__timestamp"));
				}
			}

		}

		
		
		void ItemDeleted(object sender,
			DigitalPlatform.Xml.ItemDeletedEventArgs e)
		{

			if (!(e.item is ElementItem))
				return;

			e.RecursiveChildEvents = true;
			e.RiseAttrsEvents = true;

			// e.item中还残存原有的id
			if (IsFileElement(e.item) == false)
				return;

			string strID = ((ElementItem)e.item).GetAttrValue("id");

			// m_tableFileId.Remove((object)strID);	// 此句可以造成删除后的id重复使用的效果

			DeleteLineById(strID);

		}

	
		void ItemChanged(object sender,
			DigitalPlatform.Xml.ItemChangedEventArgs e)
		{

			if (!(e.item is AttrItem))
				return;	// 只关心属性改变

			ElementItem parent = (ElementItem)e.item.parent;


			if (parent == null)
			{
				// 节点尚未插入
				return;
			}

			// e.item已经是一个属性结点
			ChangeFileAttr((AttrItem)e.item,
				e.OldValue,
				e.NewValue);

		}


		void ChangeFileAttr(AttrItem attr,
			string strOldValue,
			string strNewValue)
		{
			ElementItem parent = attr.parent;

			string strID = parent.GetAttrValue("id");
			if (strID == null)
				strID = "";

			if (attr.Name == "id") 
			{
				ChangeLine(strOldValue, strNewValue, null, null, null, null, null);
			}

			else if (attr.Name == "__mime") 
			{

				ChangeLine(strID, null, null, null, strNewValue, null, null);
			}


			else if (attr.Name == "__localpath") 
			{
				ChangeLine(strID, null, null, strNewValue, null, null, null);
			}

			else if (attr.Name == "__state") 
			{
				ChangeLine(strID, null, strNewValue, null, null, null, null);
			}

			else if (attr.Name == "__size") 
			{
				ChangeLine(strID, null, null, null, null, strNewValue, null);
			}
			else if (attr.Name == "__timestamp") 
			{
				ChangeLine(strID, null, null, null, null, null, strNewValue);
			}
		}
		
		#endregion

		// 从列表中检索一行是否存在
		public ListViewItem SearchLine(string strID)
		{
			for(int i=0;i<this.Items.Count;i++)
			{
				if (ListViewUtil.GetItemText(this.Items[i], COLUMN_ID) == strID)
				{
					return this.Items[i];
				}
			}
			return null;
		}

		// ?
		// 在listview加一行,如果此行已存在，则修改其内容
		public void NewLine(DigitalPlatform.Xml.ElementItem fileitem,
			bool bIsNewFile)
		{
			string strID = fileitem.GetAttrValue("id");

			if (strID == null || strID == "")
			{
				Debug.Assert(bIsNewFile == true, "必须是客户端文件才能无id属性");
			}

			string strState;
			if (bIsNewFile == false)
				strState = this.ServerFileState;
			else
				strState = this.NewFileState;


			// 维护id表
			if (strID != null && strID != "") 
			{
				if (m_tableFileId.Contains((object)strID) == false)
					m_tableFileId.Add(strID, (object)true);
			}



			ListViewItem item = SearchLine(strID);
            if (item == null)
            {
                item = new ListViewItem(strID, 0);
                this.Items.Add(item);

                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                m_bChanged = true;

            }
            else
            {
                // 2006/6/22
                // 重复插入.
                // 插入在已经发现的事项前面
                int index = this.Items.IndexOf(item);

                item = new ListViewItem(strID, 0);
                this.Items.Insert(index, item);

                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                m_bChanged = true;
            }

			string strOldState = fileitem.GetAttrValue("__state");
			if (strOldState != strState)
				fileitem.SetAttrValue("__state", strState);
		}

		// 从服务器获得元数据
		public void GetMetaDataParam(DigitalPlatform.Xml.ElementItem element)
		{
			string strID = element.GetAttrValue("id");

			if (strID == null || strID == "")
			{
				Debug.Assert(false);
			}


			// 如果没有挂回调函数，警告
			if (this.procDownloadOneMetaData == null) 
			{
				element.SetAttrValue("__mime", "?mime");
				element.SetAttrValue("__localpath", "?localpath");
				element.SetAttrValue("__size", "?size");
				element.SetAttrValue("__timestamp", "?timestamp");
				m_bChanged = true;
			}
			else 
			{

				string strExistTimeStamp = element.GetAttrValue("__timestamp");



				if (strExistTimeStamp != null && strExistTimeStamp != "")
				{
					ChangeLine(strID, 
						null,	// newid
						null,	// state
						element.GetAttrValue("__localpath"),
						element.GetAttrValue("__mime"),
						element.GetAttrValue("__size"),
						element.GetAttrValue("__timestamp") );
					return;
				}

				m_bChanged = true;

				string strMetaData = "";
				string strError = "";
				byte [] timestamp = null;
				int nRet = this.procDownloadOneMetaData(
					strID, 
					out strMetaData,
					out timestamp,
					out strError);
				if (nRet == -1) 
				{
					element.SetAttrValue("__localpath",
						strError);
					return;
				}

				if (strMetaData == "")
				{
					return;
				}

				// 取metadata
				Hashtable values = StringUtil.ParseMedaDataXml(strMetaData,
					out strError);
				if (values == null)
				{
					element.SetAttrValue("__localpath",
						strError);
					return;
				}

				string strTimeStamp = ByteArray.GetHexTimeStampString(timestamp);

				element.SetAttrValue("__mime",
					(string)values["mimetype"]);
				element.SetAttrValue("__localpath",
					(string)values["localpath"]);
				element.SetAttrValue("__size",
					(string)values["size"]);
				element.SetAttrValue("__timestamp",
					(string)strTimeStamp);
			}
		}


	// 删除一行(暂时无用)
		void DeleteLineById(string strId)
		{
			bool bFound = false;
			// 1.先根据传来的id删除相关行
			for(int i=0;i<this.Items.Count;i++)
			{
				if (ListViewUtil.GetItemText(this.Items[i], COLUMN_ID) == strId)
				{
					this.Items.RemoveAt(i);
					bFound = true;
					m_bChanged = true;
					break;
				}
			}

			if (bFound == false) 
			{
				Debug.Assert(false, "id[" + strId + "]在listview中没有找到...");
			}
		}

		
		// 跟随事件，修改listview一行
		void ChangeLine(string strID, 
			string strNewID,
			string strState,
			string strLocalPath,
			string strMime,
			string strSize,
			string strTimestamp)
		{

			for(int i=0;i<this.Items.Count;i++)
			{
				if (ListViewUtil.GetItemText(this.Items[i], COLUMN_ID) == strID) 
				{
					if (strNewID != null)
						ListViewUtil.ChangeItemText(this.Items[i], COLUMN_ID, 
							strNewID);
					if (strState != null)
						ListViewUtil.ChangeItemText(this.Items[i], COLUMN_STATE, 
							strState);
					if (strLocalPath != null)
						ListViewUtil.ChangeItemText(this.Items[i], COLUMN_LOCALPATH, 
							strLocalPath);
					if (strMime != null)
						ListViewUtil.ChangeItemText(this.Items[i], COLUMN_MIME, 
							strMime);
					if (strSize != null)
						ListViewUtil.ChangeItemText(this.Items[i], COLUMN_SIZE, 
							strSize);
					if (strTimestamp != null)
						ListViewUtil.ChangeItemText(this.Items[i], COLUMN_TIMESTAMP, 
							strTimestamp);

					m_bChanged = true;

					break;
				}
			}

		}




		private void ResFileList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{

			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			bool bSelected = this.SelectedItems.Count > 0;

			//
			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.button_modifyFile_Click);
			if (bSelected == false) 
			{
				menuItem.Enabled = false;
			}
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.DeleteLines_Click);
			if (bSelected == false)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			//
			menuItem = new MenuItem("新增(&N)");
			menuItem.Click += new System.EventHandler(this.NewLine_Click);
			contextMenu.MenuItems.Add(menuItem);


			// ---
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			//
			menuItem = new MenuItem("下载(&D)");
			menuItem.Click += new System.EventHandler(this.DownloadLine_Click);
			bool bFound = false;
			if (bSelected == true) 
			{
				for(int i=0;i<this.SelectedItems.Count;i++) 
				{
					if (IsNewFileState(ListViewUtil.GetItemText(this.SelectedItems[i], COLUMN_STATE) ) == false) 
					{
						bFound = true;
						break;
					}
				}
			}


			if (bFound == false || procDownloadFiles == null)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);


			/*
			menuItem = new MenuItem("测试");
			menuItem.Click += new System.EventHandler(this.Test_click);
			contextMenu.MenuItems.Add(menuItem);
			
			*/


			contextMenu.Show(this, new Point(e.X, e.Y) );		
			
		}

		public void Test_click(object sender, System.EventArgs e)
		{
			XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
			mngr.AddNamespace("dprms", DpNs.dprms);
			
			ItemList fileItems = this.editor.VirtualRoot.SelectItems("//dprms:file",
				mngr);

			MessageBox.Show("选中" + Convert.ToString(fileItems.Count) + "个");

		}

		DigitalPlatform.Xml.ElementItem GetFileItem(string strID)
		{
			XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
			mngr.AddNamespace("dprms", DpNs.dprms);

			//StreamUtil.WriteText("I:\\debug.txt","dprms的值:'" + DpNs.dprms + "'\r\n");

			//mngr.AddNamespace("abc","http://purl.org/dc/elements/1.1/");
			ItemList items = this.editor.VirtualRoot.SelectItems("//dprms:file[@id='" + strID + "']",
				mngr);
			if (items.Count == 0) 
				return null;

			return (ElementItem)items[0];
		}
		
		// 菜单:删除一行或者多行
		void DeleteLines_Click(object sender, System.EventArgs e)
		{
			if (this.SelectedItems.Count == 0) 
			{
				MessageBox.Show(this, "尚未选择要删除的行...");
				return;
			}
			string[] ids = new string[this.SelectedItems.Count];

			for(int i=0;i<ids.Length;i++) 
			{
				ids[i] = this.SelectedItems[i].Text;
			}

			for(int i=0;i<ids.Length;i++) 
			{
				/*
				XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
				mngr.AddNamespace("dprms", DpNs.dprms);
				ItemList items = this.editor.SelectItems("//dprms:file[@id='" + ids[i]+ "']",
					mngr);
				if (items.Count == 0) 
				{
					MessageBox.Show(this, "警告: id为[" +ids[i]+ "]的<dprms:file>元素在editor中不存在...");
				}
				else 
				{
					this.editor.Remove(items[0]);	// 自然会触发事件,更新listview
				}
				*/

				DigitalPlatform.Xml.Item item = GetFileItem(ids[i]);
				if (item == null) 
				{
					MessageBox.Show(this, "警告: id为[" +ids[i]+ "]的<dprms:file>元素在editor中不存在...");
					continue;
				}

				ElementItem parent = item.parent;
				parent.Remove(item);	// 自然会触发事件,更新listview

				m_bChanged = true;
			}

		}

		// 菜单：修改一行
		void button_modifyFile_Click(object sender, System.EventArgs e)
		{
			if (this.SelectedItems.Count == 0) 
			{
				MessageBox.Show(this, "尚未选择要修改的行...");
				return ;
			}
			ResObjectDlg dlg = new ResObjectDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            dlg.textBox_serverName.Text = ListViewUtil.GetItemText(this.SelectedItems[0], COLUMN_ID);
			dlg.textBox_state.Text = ListViewUtil.GetItemText(this.SelectedItems[0], COLUMN_STATE);
			dlg.textBox_mime.Text = ListViewUtil.GetItemText(this.SelectedItems[0], COLUMN_MIME);
			dlg.textBox_localPath.Text = ListViewUtil.GetItemText(this.SelectedItems[0], COLUMN_LOCALPATH);
			dlg.textBox_size.Text = ListViewUtil.GetItemText(this.SelectedItems[0], COLUMN_SIZE);
			dlg.textBox_timestamp.Text = ListViewUtil.GetItemText(this.SelectedItems[0], COLUMN_TIMESTAMP);

			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			Cursor cursorSave = Cursor;
			Cursor = Cursors.WaitCursor;
			this.Enabled = false;

			DigitalPlatform.Xml.ElementItem item = GetFileItem(dlg.textBox_serverName.Text);

			if (item != null) 
			{
				item.SetAttrValue("__mime", dlg.textBox_mime.Text);
				item.SetAttrValue("__localpath", dlg.textBox_localPath.Text);
				item.SetAttrValue("__state", this.NewFileState);
				item.SetAttrValue("__size", dlg.textBox_size.Text);
				item.SetAttrValue("__timestamp", dlg.textBox_timestamp.Text);

				m_bChanged = true;
			}
			else 
			{
				Debug.Assert(false, "xmleditor中居然不存在id为["
					+ dlg.textBox_serverName.Text 
					+ "]的<dprms:file>元素");
			}

			this.Enabled = true;
			Cursor = cursorSave;


		}

		
		// 菜单：下载一行或多行
		void DownloadLine_Click(object sender, System.EventArgs e)
		{
			bool bFound = false;
			for(int i=0;i<this.SelectedItems.Count;i++) 
			{
				if (IsNewFileState(ListViewUtil.GetItemText(this.SelectedItems[i], COLUMN_STATE)) == false) 
				{
					bFound = true;
				}
			}

			if (bFound == false) 
			{
				MessageBox.Show(this, "尚未选择要下载的事项，或者所选择的事项中没有状态为'已上载'的事项...");
				return;
			}

			if (procDownloadFiles == null)
			{
				MessageBox.Show(this, "procDownloadFiles尚未设置...");
				return;
			}

			procDownloadFiles();

		}

		// 菜单：新增一行
		void NewLine_Click(object sender, System.EventArgs e)
		{
			/*
			ResObjectDlg dlg = new ResObjectDlg();
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;
			*/

			DigitalPlatform.Xml.ElementItem fileitem = null;
			fileitem = CreateFileElementItem(editor);

			// string strError;

			try 
			{
				editor.DocumentElement.AutoAppendChild(fileitem);	
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			/*
			fileitem.SetAttrValue("__mime", dlg.textBox_mime.Text);
			fileitem.SetAttrValue("__localpath", dlg.textBox_localPath.Text);
			fileitem.SetAttrValue("__size", dlg.textBox_size.Text);

			*/
			m_bChanged = true;

			// -2为用户取消的状态
		}


		

		// 创建资源节点函数
		static ElementItem CreateFileElementItem(XmlEditor editor)
		{
			ElementItem item = editor.CreateElementItem("dprms", 
				"file",
				DpNs.dprms);

			return item;
		}


		string NewFileId()
		{
			int nSeed = 0;
			string strID = "";
			for(;;) {
				strID = Convert.ToString(nSeed++);
				if (m_tableFileId.Contains((object)strID) == false)
					return strID;
			}

		}

		/*
		// 设置<file>元素特有的特性
		// 在新建元素的时候调此函数
		void SetItemProperty(XmlEditor editor,
			DigitalPlatform.Xml.ElementItem item,
			string strMime,
			string strLocalPath,
			string strSize)
		{
			AttrItem attr = null;

			// id属性
			attr = editor.CreateAttrItem("id");
			attr.Value = NewFileId();	// editor.GetFileNo(null);
			item.AppendAttr(attr);

			// 加__mime属性
			attr = editor.CreateAttrItem("__mime");
			attr.Value = strMime;
			item.AppendAttr(attr);


			// 加__localpath属性
			attr = editor.CreateAttrItem("__localpath");
			attr.Value = strLocalPath;
			item.AppendAttr (attr);

			// __size属性
			attr = editor.CreateAttrItem("__size");
			attr.Value = strSize;
			item.AppendAttr(attr);

			// __state属性
			attr = editor.CreateAttrItem("__state");
			attr.Name = "__state";
			attr.Value = this.NewFileState;
			item.AppendAttr(attr);
			// 这个属性在对应的资源上载完后要改过来

			m_bChanged = true;
		}
		*/

#if NO
		public static void ChangeMetaData(ref string strMetaData,
			string strID,
			string strLocalPath,
			string strMimeType,
			string strLastModified,
			string strPath,
			string strTimestamp)
		{
			XmlDocument dom = new XmlDocument();

			if (strMetaData == "")
				strMetaData = "<file/>";

			dom.LoadXml(strMetaData);

			if (strID != null)
				DomUtil.SetAttr(dom.DocumentElement, "id", strID);

			if (strLocalPath != null)
				DomUtil.SetAttr(dom.DocumentElement, "localpath", strLocalPath);

			if (strMimeType != null)
				DomUtil.SetAttr(dom.DocumentElement, "mimetype", strMimeType);

			if (strLastModified != null)
				DomUtil.SetAttr(dom.DocumentElement, "lastmodified", strLastModified);

			if (strPath != null)
				DomUtil.SetAttr(dom.DocumentElement, "path", strPath);

			if (strTimestamp != null)
				DomUtil.SetAttr(dom.DocumentElement, "timestamp", strTimestamp);


			strMetaData = dom.OuterXml;
		}
#endif

		// 从窗口中查得localpath
		public string GetLocalFileName(string strID)
		{
			for(int i=0;i<this.Items.Count;i++)
			{
				string strCurID = ListViewUtil.GetItemText(this.Items[i], COLUMN_ID);
				string strLocalFileName = ListViewUtil.GetItemText(this.Items[i], COLUMN_LOCALPATH);

				if (strID == strCurID)
					return strLocalFileName;
			}

			return "";
		}

		// 下载资源，保存到备份文件
		public int DoSaveResToBackupFile(
			Stream outputfile,
			string strXmlRecPath,
			RmsChannel channel,
			DigitalPlatform.Stop stop,
			out string strError)
		{
			strError = "";

			string strTempFileName = Path.GetTempFileName();
			try 
			{
				long lRet;

				for(int i=0;i<this.Items.Count;i++)
				{
					string strState = ListViewUtil.GetItemText(this.Items[i], COLUMN_STATE);
					string strID = ListViewUtil.GetItemText(this.Items[i], COLUMN_ID);
					string strResPath = strXmlRecPath + "/object/" + ListViewUtil.GetItemText(this.Items[i], COLUMN_ID);
					string strLocalFileName = ListViewUtil.GetItemText(this.Items[i], COLUMN_LOCALPATH);
					string strMime =  ListViewUtil.GetItemText(this.Items[i], COLUMN_MIME);

					string strMetaData;

					// 服务器端文件
					if (IsNewFileState(strState) == false) 
					{
						if (stop != null)
							stop.SetMessage("正在下载 " + strResPath);

						byte [] baOutputTimeStamp = null;
						string strOutputPath;

						lRet = channel.GetRes(strResPath,
							strTempFileName,
							stop,
							out strMetaData,
							out baOutputTimeStamp,
							out strOutputPath,
							out strError);
						if (lRet == -1)
							return -1;

						ResPath respath = new ResPath();
						respath.Url = channel.Url;
						respath.Path = strResPath;

						// strMetaData还要加入资源id?
						ExportUtil.ChangeMetaData(ref strMetaData,
							strID,
							null,
							null,
							null,
							respath.FullPath,
							ByteArray.GetHexTimeStampString(baOutputTimeStamp));


						lRet = Backup.WriteOtherResToBackupFile(outputfile,
							strMetaData,
							strTempFileName);

					}
					else // 本地新文件
					{
						if (stop != null)
							stop.SetMessage("正在复制 " + strLocalFileName);

						// strMetaData = "<file mimetype='"+ strMime+"' localpath='"+strLocalPath+"' id='"+strID+"'></file>";

						ResPath respath = new ResPath();
						respath.Url = channel.Url;
						respath.Path = strResPath;

						strMetaData = "";
						FileInfo fi = new FileInfo(strLocalFileName);
						ExportUtil.ChangeMetaData(ref strMetaData,
							strID,
							strLocalFileName,
							strMime,
							fi.LastWriteTimeUtc.ToString(),
							respath.FullPath,
							"");

						lRet = Backup.WriteOtherResToBackupFile(outputfile,
							strMetaData,
							strLocalFileName);

					}

				}

				if (stop != null)
					stop.SetMessage("保存资源到备份文件全部完成");

			}
			finally 
			{

				if (strTempFileName != "")
					File.Delete(strTempFileName);
			}

			return 0;
		}


		// 上载资源
		// return:
		//		-1	error
		//		>=0 实际上载的资源对象数
		public int DoUpload(
			string strXmlRecPath,
			RmsChannel channel,
			DigitalPlatform.Stop stop,
			out string strError)
		{
			strError = "";
            
			int nUploadCount = 0;

            string strLastModifyTime = DateTime.UtcNow.ToString("u");

			bNotAskTimestampMismatchWhenOverwrite = false;

			for(int i=0;i<this.Items.Count;i++)
			{
				string strState = ListViewUtil.GetItemText(this.Items[i], COLUMN_STATE);
				string strID = ListViewUtil.GetItemText(this.Items[i], COLUMN_ID);
				string strResPath = strXmlRecPath + "/object/" + ListViewUtil.GetItemText(this.Items[i], COLUMN_ID);
				string strLocalFileName = ListViewUtil.GetItemText(this.Items[i], COLUMN_LOCALPATH);
				string strMime =  ListViewUtil.GetItemText(this.Items[i], COLUMN_MIME);
				string strTimeStamp =  ListViewUtil.GetItemText(this.Items[i], COLUMN_TIMESTAMP);

				if (IsNewFileState(strState) == false)
					continue;

				// 检测文件尺寸
				FileInfo fi = new FileInfo(strLocalFileName);


				if (fi.Exists == false) 
				{
					strError = "文件 '" + strLocalFileName + "' 不存在...";
					return -1;
				}

				string[] ranges = null;

				if (fi.Length == 0)	
				{ // 空文件
					ranges = new string[1];
					ranges[0] = "";
				}
				else 
				{
					string strRange = "";
					strRange = "0-" + Convert.ToString(fi.Length-1);

					// 按照100K作为一个chunk
					ranges = RangeList.ChunkRange(strRange,
                        100 * 1024
                        );
				}

				byte [] timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
				byte [] output_timestamp = null;

				nUploadCount ++;

			REDOWHOLESAVE:
				string strWarning = "";


				for(int j=0;j<ranges.Length;j++) 
				{
				REDOSINGLESAVE:

					Application.DoEvents();	// 出让界面控制权

					if (stop.State != 0)
					{
						strError = "用户中断";
						goto ERROR1;
					}

					string strWaiting = "";
					if (j == ranges.Length - 1)
						strWaiting = " 请耐心等待...";

					string strPercent = "";
					RangeList rl = new RangeList(ranges[j]);
					if (rl.Count >= 1) 
					{
						double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
						strPercent = String.Format("{0,3:N}",ratio * (double)100) + "%";
					}

					if (stop != null)
						stop.SetMessage("正在上载 " + ranges[j] + "/"
							+ Convert.ToString(fi.Length)
							+ " " + strPercent + " " + strLocalFileName + strWarning + strWaiting);

					/*
					if (stop != null)
						stop.SetMessage("正在上载 " + ranges[j] + "/" + Convert.ToString(fi.Length) + " " + strLocalFileName);
					*/

					long lRet = channel.DoSaveResObject(strResPath,
						strLocalFileName,
						strLocalFileName,
						strMime,
                        strLastModifyTime,
						ranges[j],
						j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
						timestamp,
						out output_timestamp,
						out strError);
					timestamp = output_timestamp;
					ListViewUtil.ChangeItemText(this.Items[i], COLUMN_TIMESTAMP, ByteArray.GetHexTimeStampString(timestamp));

					strWarning = "";

					if (lRet == -1) 
					{
						if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
						{

							if (this.bNotAskTimestampMismatchWhenOverwrite == true) 
							{
								timestamp = new byte[output_timestamp.Length];
								Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
								strWarning = " (时间戳不匹配, 自动重试)";
								if (ranges.Length == 1 || j==0) 
									goto REDOSINGLESAVE;
								goto REDOWHOLESAVE;
							}


							DialogResult result = MessageDlg.Show(this, 
								"上载 '" + strLocalFileName + "' (片断:" + ranges[j] + "/总尺寸:"+Convert.ToString(fi.Length)
								+") 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
								+ strError + "\r\n---\r\n\r\n是否以新时间戳强行上载?\r\n注：(是)强行上载 (否)忽略当前记录或资源上载，但继续后面的处理 (取消)中断整个批处理",
								"dp2batch",
								MessageBoxButtons.YesNoCancel,
								MessageBoxDefaultButton.Button1,
								ref this.bNotAskTimestampMismatchWhenOverwrite);
							if (result == DialogResult.Yes) 
							{
								timestamp = new byte[output_timestamp.Length];
								Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
								strWarning = " (时间戳不匹配, 应用户要求重试)";
								if (ranges.Length == 1 || j==0) 
									goto REDOSINGLESAVE;
								goto REDOWHOLESAVE;
							}

							if (result == DialogResult.No) 
							{
								goto END1;	// 继续作后面的资源
							}

							if (result == DialogResult.Cancel) 
							{
								strError = "用户中断";
								goto ERROR1;	// 中断整个处理
							}
						}

						goto ERROR1;
					}
					//timestamp = output_timestamp;

				}


				DigitalPlatform.Xml.ElementItem item = GetFileItem(strID);

				if (item != null) 
				{
					item.SetAttrValue("__state", this.ServerFileState);
				}
				else 
				{
					Debug.Assert(false, "xmleditor中居然不存在id为[" + strID + "]的<dprms:file>元素");
				}


			}

			END1:
			if (stop != null)
				stop.SetMessage("上载资源全部完成");

			return nUploadCount;
			ERROR1:
				return -1;
		}

		// 获得当前所选择的可以用于下载的全部id
		public string[] GetSelectedDownloadIds()
		{
			ArrayList aText = new ArrayList();

			int i=0;
			for(i=0;i<this.SelectedItems.Count;i++) 
			{
				if (IsNewFileState(ListViewUtil.GetItemText(this.SelectedItems[i], COLUMN_STATE)) == false) 
				{
					aText.Add(ListViewUtil.GetItemText(this.SelectedItems[i], COLUMN_ID));
				}
			}

			string[] result = new string[aText.Count];
			for(i=0;i<aText.Count;i++)
			{
				result[i] = (string)aText[i];
			}

			return result;
		}

		// 从XML数据中移除工作用的临时属性
        // parameters:
        //      bHasUploadedFile    是否含有已上载资源的<file>元素?
		public static int RemoveWorkingAttrs(string strXml,
			out string strResultXml,
            out bool bHasUploadedFile,
			out string strError)
		{
            bHasUploadedFile = false;
			strResultXml = strXml;
			strError = "";
			XmlDocument dom = new XmlDocument();

			try 
			{
				dom.LoadXml(strXml);
			}
			catch (Exception ex)
			{
                strError = ExceptionUtil.GetAutoText(ex);
				return -1;
			}

			XmlNamespaceManager mngr = new XmlNamespaceManager(dom.NameTable);
			mngr.AddNamespace("dprms", DpNs.dprms);
			
			XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", mngr);

			for(int i=0; i<nodes.Count; i++)
			{
				XmlNode node = nodes[i];

                string strState = DomUtil.GetAttr(node, "__state");
                if (IsNewFileState(strState) == false)
                    bHasUploadedFile = true;


				DomUtil.SetAttr(node, "__mime", null);
				DomUtil.SetAttr(node, "__localpath", null);
				DomUtil.SetAttr(node, "__state", null);
				DomUtil.SetAttr(node, "__size", null);

				DomUtil.SetAttr(node, "__timestamp", null);

			}

			strResultXml = dom.OuterXml;	// ??

			return 0;
		}


#if NO
		// 得到Xml记录中所有<file>元素的id属性值
		public static int GetFileIds(string strXml,
			out string[] ids,
			out string strError)
		{
			ids = null;
			strError = "";
			XmlDocument dom = new XmlDocument();

			try 
			{
				dom.LoadXml(strXml);
			}
			catch (Exception ex)
			{
				strError = "装载 XML 进入 DOM 时出错: " + ex.Message;
				return -1;
			}

			XmlNamespaceManager mngr = new XmlNamespaceManager(dom.NameTable);
			mngr.AddNamespace("dprms", DpNs.dprms);
			
			XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", mngr);

			ids = new string[nodes.Count];
			for(int i=0; i<nodes.Count; i++)
			{
				XmlNode node = nodes[i];

				ids[i] = DomUtil.GetAttr(node, "id");
			}
			return 0;
		}
#endif

		static bool IsNewFileState(string strState)
		{
			if (String.IsNullOrEmpty(strState) == true)
				return false;
			if (strState == "已上载")
				return false;
			if (strState == "尚未上载")
				return true;

			// Debug.Assert(false, "未定义的状态");
			return false;
		}

		private void ResFileList_DoubleClick(object sender, System.EventArgs e)
		{
			if (this.SelectedItems.Count == 0)
				return;

			button_modifyFile_Click(null, null);
		}

		string ServerFileState
		{
			get 
			{
				return "已上载";
			}
		}
		string NewFileState
		{
			get 
			{
				return "尚未上载";
			}
		}

	}

	public delegate void Delegate_DownloadFiles(); 

	// strID:	资源id。
	public delegate int Delegate_DownloadOneMetaData(string strID,
	out string strResultXml,
	out byte [] timestamp,
	out string strError);

}

