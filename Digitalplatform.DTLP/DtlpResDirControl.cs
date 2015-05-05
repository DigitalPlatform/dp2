using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using System.Text;

using DigitalPlatform;  // ByteArray
// using DigitalPlatform.DTLP;


namespace DigitalPlatform.DTLP
{

	/// <summary>
	/// DTLP资源目录树控件
	/// </summary>
	public class DtlpResDirControl : System.Windows.Forms.TreeView
	{
		#region Icon排列顺序常量

		public const int OFFS_FOLDER = 0;
		public const int OFFS_STDBASE =  1;
		public const int OFFS_SMDBASE =  2;
		public const int OFFS_STDFILE =  3;
		public const int OFFS_CFGFILE =  4;
		public const int OFFS_TCPS =     5;
		public const int OFFS_MYCOMPUTER =   6;
		public const int OFFS_NORMAL =       7;
		public const int OFFS_KERNEL =       8;
		public const int OFFS_FROM =         9;
		public const int OFFS_CDROM =        10;
		public const int OFFS_MYDESKTOP =    11;

		#endregion

        public DigitalPlatform.Stop Stop = null;

		public DtlpChannelArray channelarray = null;
		public DtlpChannel Channel = null;

		Hashtable	m_itemInfoTable = new Hashtable();

		// public Delegate_ItemSelected procItemSelected = null;
        public event ItemSelectedEventHandle ItemSelected = null;

		// public Delegate_ItemText procItemText = null;
        public event GetItemTextStyleEventHandle GetItemTextStyle = null;

		// public string DefaultUserName = "public";
		// public string DefaultPassword = "";
		private System.Windows.Forms.ImageList imageList_resIcon16;
		private System.ComponentModel.IContainer components;

		public DtlpResDirControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitComponent call
			this.ImageList = imageList_resIcon16;

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DtlpResDirControl));
            this.imageList_resIcon16 = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList_resIcon16
            // 
            this.imageList_resIcon16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon16.ImageStream")));
            this.imageList_resIcon16.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon16.Images.SetKeyName(0, "");
            this.imageList_resIcon16.Images.SetKeyName(1, "");
            this.imageList_resIcon16.Images.SetKeyName(2, "");
            this.imageList_resIcon16.Images.SetKeyName(3, "");
            this.imageList_resIcon16.Images.SetKeyName(4, "");
            this.imageList_resIcon16.Images.SetKeyName(5, "");
            this.imageList_resIcon16.Images.SetKeyName(6, "");
            this.imageList_resIcon16.Images.SetKeyName(7, "");
            this.imageList_resIcon16.Images.SetKeyName(8, "");
            this.imageList_resIcon16.Images.SetKeyName(9, "");
            this.imageList_resIcon16.Images.SetKeyName(10, "");
            this.imageList_resIcon16.Images.SetKeyName(11, "");
            // 
            // DtlpResDirControl
            // 
            this.HideSelection = false;
            this.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DtlpResDirControl_AfterSelect);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DtlpResDirControl_MouseUp);
            this.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.DtlpResDirControl_AfterExpand);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DtlpResDirControl_MouseDown);
            this.ResumeLayout(false);

		}
		#endregion


        // 得到当前选择事项的路径
        public string SelectedPath1
        {
            get
            {
                if (this.SelectedNode == null)
                    return null;
                char sep = this.PathSeparator[0];

                return GetPath(this.SelectedNode, sep); // '/'
            }

            set
            {
                char sep = this.PathSeparator[0];

                // Debug.Assert(this.PathSeparator == "/", "");
                ExpandPath(value, sep);    // '/'
            }

        }

        // 得到当前选择事项的掩码
        public int SelectedMask
        {
            get
            {
                if (this.SelectedNode == null)
                    return 0;
                string strPath = GetPath(this.SelectedNode, '/');

                ItemInfo iteminfo = (ItemInfo)
                    m_itemInfoTable[strPath];
                if (iteminfo == null)
                {
                    Debug.Assert(false, "路径[" + strPath + "]没有找到对应的ItemInfo事项");
                    return 0;
                }

                return iteminfo.Mask;
            }
        }



		// strStart, // 起始路径, ""表示根
		Package GetOneLevelDirPackage(string strStart)
		{
			int nRet;
			byte [] baPackage = null;

			// bool bSetDefault = false;	// 表示是否使用过缺省帐户

			//bool bFirstLogin = true;

			Package package = new Package();

			// CWaitCursor cursor;
			if (Channel == null) 
			{
				Channel = channelarray.CreateChannel(0);
			}

			Debug.Assert(Channel != null, "channel尚未初始化");

			Cursor.Current = Cursors.WaitCursor;
            if (Stop != null)
            {
                Stop.OnStop += new StopEventHandler(this.DoStop);
                Stop.SetMessage("正在列目录 '" + strStart + "' ...");
                Stop.BeginLoop();
            }
            try
            {
                nRet = Channel.Dir(strStart,
                    out baPackage);
            }
            finally
            {
                if (Stop != null)
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    Stop.Initial("");
                }

                Cursor.Current = Cursors.Default;
            }

			if (nRet == -1) 
			{
				Channel.ErrorBox(this,
					"restree",
					"列目录发生错误");

				goto ERROR1;
			}


			package.LoadPackage(baPackage, Channel.GetPathEncoding(strStart));
			package.Parse(PackageFormat.String);

			return package;
			ERROR1:
				return null;
		}

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Cancel();
        }

		// 填充下级对象
		public void FillSub(TreeNode node)
		{
			string strPath = "";

			if (node != null) 
			{
				node.Nodes.Clear();	// 清除以前的下级事项
			}


			Package package = null;

			if (node != null)
			{
				strPath = GetPath(node, '/');
			}

			/*
			ItemInfo iteminfo = (ItemInfo)
				m_itemInfoTable[strPath];
			if (iteminfo == null) 
			{
				iteminfo = new ItemInfo();
				m_itemInfoTable.Add(strPath, iteminfo);
			}
			*/

			this.Update();

			package = GetOneLevelDirPackage(strPath/*, iteminfo*/);
			if (package == null) 
			{ // error
				// 最好将本Item图标改为带有红色出错标记的样式
				if (node != null) 
				{
					SetLoading(node);
					node.Collapse();
				}

				return;
			}

			for(int i=0;i<package.Count;i++) 
			{
				Cell cell = (Cell)package[i];

				string strCurPath = cell.Path;
				if (strPath != "")
				{
					// 前方应当完全一致
					Debug.Assert(strCurPath.Length >= strPath.Length+1);
					strCurPath = strCurPath.Remove(0,strPath.Length + 1);
				}

				int nImage = GetImageIndex(cell.Mask);

				// 新node
				TreeNode nodeNew = new TreeNode(strCurPath,nImage,nImage);

				if ((cell.Mask & DtlpChannel.AttrExtend) != 0) 
				{
					SetLoading(nodeNew);
				}

                /*
				if (procItemText != null) 
				{
					string strFontFace;
					int nFontSize;
					FontStyle FontStyle;
					Color ForeColor = nodeNew.ForeColor;
					int nRet = procItemText(GetPath(nodeNew, '/'),cell.Mask, 
						out strFontFace, 
						out nFontSize,
						out FontStyle,
						ref ForeColor);
					if (nRet == 1) 
					{
						if (strFontFace != "") 
						{
							Font font = new Font(strFontFace, nFontSize, FontStyle);
							nodeNew.NodeFont = font;
						}
						nodeNew.ForeColor = ForeColor;
					}
				}*/

                if (this.GetItemTextStyle != null)
                {
                    GetItemTextStyleEventArgs e = new GetItemTextStyleEventArgs();
                    e.Path = GetPath(nodeNew, '/');
                    e.Mask = cell.Mask;
                    e.ForeColor = nodeNew.ForeColor;
                    this.GetItemTextStyle(this, e);
                    if (e.Result == 1)
                    {
                        if (e.FontFace != "")
                        {
                            Font font = new Font(e.FontFace,
                                e.FontSize,
                                e.FontStyle);
                            nodeNew.NodeFont = font;
                        }
                        nodeNew.ForeColor = e.ForeColor;
                    }

                }


				if (node == null)
					this.Nodes.Add(nodeNew);
				else
					node.Nodes.Add(nodeNew);

				// 确保有一个对应的ItemInfo对象

				string strKeyPath = GetPath(nodeNew, '/');
				ItemInfo iteminfo = (ItemInfo)
					m_itemInfoTable[strKeyPath];
				if (iteminfo == null) 
				{
					iteminfo = new ItemInfo();
					m_itemInfoTable.Add(strKeyPath, iteminfo);
				}

				iteminfo.Mask = cell.Mask;

				// nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

			}

		}

		// 在一个节点下级插入"loading..."，以便出现+号
		static void SetLoading(TreeNode node)
		{
			// 新node
			TreeNode nodeNew = new TreeNode("loading...",0,0);

			node.Nodes.Add(nodeNew);
		}

		// 下级是否包含loading...?
		static bool IsLoading(TreeNode node)
		{
			if (node.Nodes.Count == 0)
				return false;

			if (node.Nodes[0].Text == "loading...")
				return true;

			return false;
		}

		static string GetPath(TreeNode node,
            char delimeter)
		{
			TreeNode nodeCur = node;
			string strPath = "";

			while(true)
			{
				if (nodeCur == null)
					break;
				if (strPath != "")
					strPath = nodeCur.Text + new string(delimeter, 1) + strPath;
				else
					strPath = nodeCur.Text;

				nodeCur = nodeCur.Parent;
			}

			return strPath;
		}

		private void DtlpResDirControl_AfterExpand(object sender,
			System.Windows.Forms.TreeViewEventArgs e)
		{
			TreeNode node = e.Node;

			if (node == null)
				return;

			// 需要展开
			if (IsLoading(node) == true) 
			{
				FillSub(node);
			}
		
		}

		// 根据路径逐步展开
		public void ExpandPath(string strPath,
            char delimeter)
		{
            if (String.IsNullOrEmpty(strPath) == true)
                return;

            string[] aName = strPath.Split(new Char[] { delimeter });   // old '/'

			TreeNode node = null;
			TreeNode nodeThis = null;
			for(int i=0;i<aName.Length;i++)
			{
				TreeNodeCollection nodes = null;

				if (node == null)
					nodes = this.Nodes;
				else 
					nodes = node.Nodes;

				bool bFound = false;
				for(int j=0;j<nodes.Count;j++)
				{
					if (aName[i] == nodes[j].Text) 
					{
						bFound = true;
						nodeThis = nodes[j];
						break;
					}
				}
				if (bFound == false)
					break;

				node = nodeThis;


				// 需要展开
				if (IsLoading(node) == true) 
				{
					FillSub(node);
				}

                this.SelectedNode = nodeThis;   // 2006/11/15 逐步选择，避免最末一级才选择
			}

			if (nodeThis!= null && nodeThis.Parent != null)
				nodeThis.Parent.Expand();

			this.SelectedNode = nodeThis;
		}


		// 根据掩码计算出ICON图象下标
		static int GetImageIndex(Int32 lMask)
		{

			if ((lMask & DtlpChannel.TypeStdbase) != 0) 
			{
				if ((lMask&DtlpChannel.AttrRdOnly) != 0)
					return OFFS_CDROM;
			}

			if ((lMask&DtlpChannel.TypeStdbase)  != 0)
			{
				return OFFS_STDBASE;
			}
			if ((lMask&DtlpChannel.TypeSmdbase) != 0)
				return OFFS_SMDBASE;
			if ((lMask&DtlpChannel.TypeStdfile) != 0)
				return OFFS_STDFILE;
			if ((lMask&DtlpChannel.TypeCfgfile) != 0)
				return OFFS_CFGFILE;
			if ((lMask&DtlpChannel.AttrTcps)   != 0)
				return OFFS_TCPS;
			if ((lMask&DtlpChannel.TypeKernel) != 0)
				return OFFS_MYCOMPUTER;
			if ((lMask&DtlpChannel.TypeFrom) != 0)
				return OFFS_FROM;
			return 0;
		}


		private void DtlpResDirControl_AfterSelect(object sender, 
            System.Windows.Forms.TreeViewEventArgs e)
		{
            /*
			if (procItemSelected != null) 
			{
				string strPath = GetPath(this.SelectedNode, '/');
				ItemInfo iteminfo = (ItemInfo)
					m_itemInfoTable[strPath];
				if (iteminfo == null) 
				{
					Debug.Assert(false, "路径[" + strPath +"]没有找到对应的ItemInfo事项");
					procItemSelected(strPath, 0);	// 无法得到mask，于是用0代替mask
					return;
				}

				procItemSelected(strPath, iteminfo.Mask);
			}
             * */

            // 2007/11/13 改造
            if (this.ItemSelected != null)
            {
                string strPath = GetPath(this.SelectedNode, '/');
                ItemInfo iteminfo = (ItemInfo)
                    m_itemInfoTable[strPath];
                int nMask = 0;
                if (iteminfo == null)
                {
                    Debug.Assert(false, "路径[" + strPath + "]没有找到对应的ItemInfo事项");
                    nMask = 0;	// 无法得到mask，于是用0代替mask
                }
                else
                    nMask = iteminfo.Mask;

                ItemSelectedEventArgs e1 = new ItemSelectedEventArgs();
                e1.Path = strPath;
                e1.Mask = nMask;

                this.ItemSelected(this, e1);
            }

		}

        private void DtlpResDirControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            // ToolStripMenuItem subMenuItem = null;

            TreeNode node = this.SelectedNode;

            // 编辑配置文件
            menuItem = new ToolStripMenuItem("编辑配置文件(&E)");
            if (node == null
                || (node != null && node.ImageIndex != OFFS_CFGFILE))
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_editCfgFile_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this, e.Location);
        }

        // 编辑配置文件
        void menuItem_editCfgFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strContent = "";

            if (this.SelectedNode == null)
            {
                strError = "尚未选择要编辑的配置文件节点";
                goto ERROR1;
            }

            if (this.SelectedNode.ImageIndex != OFFS_CFGFILE)
            {
                strError = "所选择的节点不是配置文件类型";
                goto ERROR1;
            }

            string strPath = GetPath(this.SelectedNode, '/');

            if (Channel == null)
            {
                Channel = channelarray.CreateChannel(0);
            }

            Debug.Assert(Channel != null, "channel尚未初始化");

            Cursor.Current = Cursors.WaitCursor;
            int nRet = Channel.GetCfgFile(strPath,
                out strContent,
                out strError);
            Cursor.Current = Cursors.Default;

            if (nRet == -1)
                goto ERROR1;

            EditCfgForm dlg = new EditCfgForm();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.CfgPath = strPath;
            dlg.Content = strContent;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (dlg.Changed == false)
                return;

            // 保存到(服务器端)配置文件
            // return:
            //      -1  出错
            //      0   成功
            Cursor.Current = Cursors.WaitCursor;
            nRet = Channel.WriteCfgFile(dlg.CfgPath,
                dlg.Content,
                out strError);
            Cursor.Current = Cursors.Default;
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "配置文件 '" + dlg.CfgPath +"' 保存成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        /*
        int GetCfgFile(string strCfgFilePath,
            out string strContent,
            out string strError)
        {
            strError = "";
            strContent = "";
            int nRet;
            byte[] baPackage = null;

            if (channel == null)
            {
                channel = channelarray.CreateChannel(0);
            }

            Debug.Assert(channel != null, "channel尚未初始化");

            bool bFirst = true;

            byte[] baNext = null;
            int nStyle = DtlpChannel.XX_STYLE;

            byte[] baContent = null;

            Encoding encoding = channel.GetPathEncoding(strCfgFilePath);


            for (; ; )
            {
                Cursor.Current = Cursors.WaitCursor;

                if (bFirst == true)
                {
                    nRet = channel.Search(strCfgFilePath,
                        DtlpChannel.XX_STYLE,
                        out baPackage);
                }
                else
                {
                    nRet = channel.Search(strCfgFilePath,
                        baNext,
                        DtlpChannel.XX_STYLE,
                        out baPackage);
                }

                Cursor.Current = Cursors.Default;

                if (nRet == -1)
                {
                    channel.ErrorBox(this,
                        "restree",
                        "获取配置文件 '" + strCfgFilePath + " ' 时发生错误");

                    goto ERROR1;
                }

                Package package = new Package();
                package.LoadPackage(baPackage, encoding);
                package.Parse(PackageFormat.Binary);

                bFirst = false;

                byte [] baPart = null;
                package.GetFirstBin(out baPart);
                if (baContent == null)
                    baContent = baPart;
                else
                    baContent = ByteArray.Add(baContent, baPart);

                if (package.ContinueString != "")
                {
                    nStyle |= DtlpChannel.CONT_RECORD;
                    baNext = package.ContinueBytes;
                }
                else
                {
                    break;
                }
            }

            if (baContent != null)
            {
                for (int i = 0; i < baContent.Length; i++)
                {
                    if (baContent[i] == 0)
                        baContent[i] = (byte)'\r';
                }
                strContent = encoding.GetString(baContent).Replace("\r", "\r\n");
            }

            return 0;
        ERROR1:
            return -1;
        }
         * */

        private void DtlpResDirControl_MouseDown(object sender, MouseEventArgs e)
        {
            // 使得右鼠标键也选取treenode
            TreeNode curSelectedNode = this.GetNodeAt(e.X, e.Y);

            if (this.SelectedNode != curSelectedNode)
            {
                this.SelectedNode = curSelectedNode;

                /*
                if (this.SelectedNode == null)
                    treeView1_AfterSelect(null, null);	// 补丁
                 * */
            }

        }

    }

    /*
	// Item被选择了
	public delegate void Delegate_ItemSelected(string strPath,
	Int32 nMask);
     * */


    public delegate void ItemSelectedEventHandle(object sender,
ItemSelectedEventArgs e);

    public class ItemSelectedEventArgs : EventArgs
    {
        public string Path = "";
        public Int32 Mask = 0;
    }


    /*
	// 请求给出Item文字参数
	// return:
	//	1	设置了输出参数
	//	0	没有设置
	public delegate int Delegate_ItemText(string strPath,
	Int32 nMask,
	out string strFontFace,
	out int nFontSize,
	out FontStyle FontStyle,
	ref Color ForeColor);
     * */

    public delegate void GetItemTextStyleEventHandle(object sender,
        GetItemTextStyleEventArgs e);

    public class GetItemTextStyleEventArgs : EventArgs
    {
        public string Path = "";    // in
        public Int32 Mask = 0;  // in
        public string FontFace = ""; // out
	    public int FontSize = 0;    // out
	    public FontStyle FontStyle = FontStyle.Regular; // out
        public Color ForeColor = Color.Black;   // ref
        public int Result = 0;  // 0表示没有返回有用信息；1表示返回了有用信息
    }



	class ItemInfo
	{
		public Int32	Mask = 0;		// 特性掩码，用来控制显示ICON
	};

	/*
	 * 
	 * 
	 *
	 Icon排列顺序
folder.bmp
std_database.bmp
simple_database.bmp
std_file.bmp
cfg_file.bmp
tcps.bmp
mycomputer.bmp
normal_folder.bmp
kernel_folder.bmp
from.bmp
cdrom.bmp
mydesktop.bmp

	*/

}

