using System;
using System.Collections.Generic;
using System.Collections;   // Hashtable
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Range;
using DigitalPlatform.Text;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 二进制资源管理控件
    /// </summary>
    public partial class BinaryResControl : UserControl
    {
        // Ctrl+A自动创建数据
        /// <summary>
        /// 自动创建数据
        /// </summary>
        public event GenerateDataEventHandler GenerateData = null;

        /// <summary>
        /// 边框风格
        /// </summary>
        [Category("Appearance")]
        [DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "None")]
        public new BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        bool m_bChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                bool bOldValue = this.m_bChanged;

                this.m_bChanged = value;

                // 触发事件
                if (bOldValue != value && this.ContentChanged != null)
                {
                    ContentChangedEventArgs e = new ContentChangedEventArgs();
                    e.OldChanged = bOldValue;
                    e.CurrentChanged = value;
                    ContentChanged(this, e);
                }
            }
        }


        /// <summary>
        /// 权限值配置文件全路径
        /// </summary>
        public string RightsCfgFileName
        {
            get;
            set;
        }

        public const string CaptionNormal = "已上载";
        public const string CaptionNew = "尚未上载(新增对象)";
        public const string CaptionChanged = "尚未上载(修改过的对象)";
        public const string CaptionDeleted = "标记删除";
        public const string CaptionError = "错误";

        public const int COLUMN_ID = 0;
        public const int COLUMN_STATE = 1;
        public const int COLUMN_LOCALPATH = 2;
        public const int COLUMN_SIZE = 3;
        public const int COLUMN_MIME = 4;
        public const int COLUMN_TIMESTAMP = 5;
        public const int COLUMN_USAGE = 6;
        public const int COLUMN_RIGHTS = 7;

        /*
        public const int TYPE_UPLOADED = 0;
        public const int TYPE_NOT_UPLOAD = 1;
        public const int TYPE_ERROR = 2;
         * */

        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = null;

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        string m_strBiblioRecPath = "";

        public BinaryResControl()
        {
            InitializeComponent();
        }

        public string BiblioRecPath
        {
            get
            {
                return this.m_strBiblioRecPath;
            }
            set
            {
                this.m_strBiblioRecPath = value;
            }
        }

        public int ObjectCount
        {
            get
            {
                return this.ListView.Items.Count;
            }
        }

        public void Clear()
        {
            this.ListView.Items.Clear();
        }

        // return:
        //      -1  error
        //      0   没有装载
        //      1   已经装载
        public int LoadObject(string strBiblioRecPath,
            string strXml,
            out string strError)
        {
            strError = "";

            this.ErrorInfo = "";

            // 2007/12/2 
            if (String.IsNullOrEmpty(strXml) == true)
            {
                this.Changed = false;
                return 0;
            }

            this.BiblioRecPath = strBiblioRecPath;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装载到DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);

            return LoadObject(nodes,
                out strError);
        }

#if NO
        static Hashtable ParseMedaDataXml(string strXml,
            out string strError)
        {
            strError = "";
            Hashtable result = new Hashtable();

            if (strXml == "")
                return result;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }

            XmlAttributeCollection attrs = dom.DocumentElement.Attributes;
            for (int i = 0; i < attrs.Count; i++)
            {
                string strName = attrs[i].Name;
                string strValue = attrs[i].Value;

                result.Add(strName, strValue);
            }

            return result;
        }
#endif

        // 填充列表内容
        // return:
        //      -1  error
        //      0   没有填充任何内容，列表为空
        //      1   已经填充了内容
        public int LoadObject(XmlNodeList nodes,
            out string strError)
        {
            strError = "";

            bool bOldEnabled = this.Enabled;

            this.Enabled = bOldEnabled;
            try
            {
                this.ListView.Items.Clear();

                List<ListViewItem> items = new List<ListViewItem>();
                // 第一阶段，把来自 XML 记录中的 <file> 元素信息填入。
                // 这样就保证了至少可以在保存书目记录阶段能还原 XML 记录中的相关部分
                foreach(XmlElement node in nodes)
                {
                    string strID = DomUtil.GetAttr(node, "id");
                    string strUsage = DomUtil.GetAttr(node, "usage");
                    string strRights = DomUtil.GetAttr(node, "rights");

                    ListViewItem item = new ListViewItem();

                    // state
                    SetLineInfo(item,
                        LineState.Normal);

                    // id
                    ListViewUtil.ChangeItemText(item, COLUMN_ID, strID);
                    // usage
                    ListViewUtil.ChangeItemText(item, COLUMN_USAGE, strUsage);
                    // rights
                    ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, strRights);
                    this.ListView.Items.Add(item);

                    items.Add(item);
                }

                // 第二阶段，从 dp2library 服务器获取 metadata 信息，填充其他字段内容
                foreach(ListViewItem item in items)
                {
                    string strID = ListViewUtil.GetItemText(item, COLUMN_ID);

                    string strMetadataXml = "";
                    byte[] baMetadataTimestamp = null;
                    // 获得一个对象资源的元数据
                    int nRet = GetOneObjectMetadata(
                        this.BiblioRecPath,
                        strID,
                        out strMetadataXml,
                        out baMetadataTimestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        if (Channel.ErrorCode == localhost.ErrorCode.AccessDenied)
                        {
                            return -1;
                        }
                        // item.SubItems.Add(strError);
                        ListViewUtil.ChangeItemText(item, COLUMN_STATE, strError);
                        item.ImageIndex = 1;    // error!
                        continue;
                    }

                    // 取metadata值
                    Hashtable values = StringUtil.ParseMedaDataXml(strMetadataXml,
                        out strError);
                    if (values == null)
                    {
                        ListViewUtil.ChangeItemText(item, COLUMN_STATE, strError);
                        item.ImageIndex = 1;    // error!
                        continue;
                    }

                    // localpath
                    ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, (string)values["localpath"]);

                    // size
                    ListViewUtil.ChangeItemText(item, COLUMN_SIZE, (string)values["size"]);

                    // mime
                    ListViewUtil.ChangeItemText(item, COLUMN_MIME, (string)values["mimetype"]);

                    // tiemstamp
                    string strTimestamp = ByteArray.GetHexTimeStampString(baMetadataTimestamp);
                    ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, strTimestamp);
                }

                this.Changed = false;

                if (this.ListView.Items.Count > 0)
                    return 1;

                return 0;
            }
            finally
            {
                this.Enabled = bOldEnabled;
            }
        }

#if NO
        // return:
        //      -1  error
        //      0   没有装载
        //      1   已经装载
        public int LoadObject(XmlNodeList nodes,
            out string strError)
        {
            strError = "";

            bool bOldEnabled = this.Enabled;

            this.Enabled = bOldEnabled;
            try
            {
                this.ListView.Items.Clear();

                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    string strID = DomUtil.GetAttr(node, "id");
                    string strUsage = DomUtil.GetAttr(node, "usage");
                    string strRights = DomUtil.GetAttr(node, "rights");

                    ListViewItem item = new ListViewItem();
                    // item.Text = strID;
                    ListViewUtil.ChangeItemText(item, COLUMN_ID, strID);

                    this.ListView.Items.Add(item);

                    string strMetadataXml = "";
                    byte[] baMetadataTimestamp = null;
                    // 获得一个对象资源的元数据
                    int nRet = GetOneObjectMetadata(
                        this.BiblioRecPath,
                        strID,
                        out strMetadataXml,
                        out baMetadataTimestamp,
                        out strError);
                    if (nRet == -1)
                    {
                        if (Channel.ErrorCode == localhost.ErrorCode.AccessDenied)
                        {
                            return -1;
                        }
                        // item.SubItems.Add(strError);
                        ListViewUtil.ChangeItemText(item, COLUMN_STATE, strError);
                        item.ImageIndex = 1;    // error!
                        continue;
                    }

                    // 取metadata值
                    Hashtable values = ParseMedaDataXml(strMetadataXml,
                        out strError);
                    if (values == null)
                    {
                        // item.SubItems.Add(strError);
                        ListViewUtil.ChangeItemText(item, COLUMN_STATE, strError);
                        item.ImageIndex = 1;    // error!
                        continue;
                    }

                    // state
                    SetLineInfo(item,
                        // strUsage,
                        LineState.Normal);

                    // localpath
                    // item.SubItems.Add((string)values["localpath"]);
                    ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, (string)values["localpath"]);

                    // size
                    // item.SubItems.Add((string)values["size"]);
                    ListViewUtil.ChangeItemText(item, COLUMN_SIZE, (string)values["size"]);

                    // mime
                    // item.SubItems.Add((string)values["mimetype"]);
                    ListViewUtil.ChangeItemText(item, COLUMN_MIME, (string)values["mimetype"]);

                    // tiemstamp
                    string strTimestamp = ByteArray.GetHexTimeStampString(baMetadataTimestamp);
                    // item.SubItems.Add(strTimestamp);
                    ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, strTimestamp);

                    // usage
                    ListViewUtil.ChangeItemText(item, COLUMN_USAGE, strUsage);

                    // rights
                    ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, strRights);
                }

                this.Changed = false;

                if (this.ListView.Items.Count > 0)
                    return 1;

                return 0;
            }
            finally
            {
                this.Enabled = bOldEnabled;
            }
        }
#endif

        static LineState GetLineState(ListViewItem item)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                return LineState.Error;
                // throw new Exception("空的Tag");
            }

            return info.LineState;
        }

        LineState GetOldLineState(ListViewItem item)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                return LineState.Error;
                // throw new Exception("空的Tag");
            }

            return info.OldLineState;
        }

        void SetOldLineState(ListViewItem item,
            LineState old_state)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                info = new LineInfo();
                item.Tag = info;
            }

            info.OldLineState = old_state;
        }

        void SetResChanged(ListViewItem item,
    bool bChanged)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                info = new LineInfo();
                item.Tag = info;
            }

            info.ResChanged = bChanged;
        }

        void SetXmlChanged(ListViewItem item,
bool bChanged)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                info = new LineInfo();
                item.Tag = info;
            }

            info.XmlChanged = bChanged;
        }

        /*
        void SetLineState(ListViewItem item,
            LineState state)
        {
            if (state == LineState.Normal)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionNormal);
                item.ForeColor = Color.Black;
                item.BackColor = Color.White;
            }
            else if (state == LineState.New)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionNew);
                item.ForeColor = Color.Black;
                item.BackColor = Color.LightGreen;
            }
            else if (state == LineState.Changed)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionChanged);
                item.ForeColor = Color.Black;
                item.BackColor = Color.Yellow;
            }
            else if (state == LineState.Deleted)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionDeleted);
                item.ForeColor = Color.DarkGray;
                item.BackColor = Color.White;
            }
            else if (state == LineState.Error)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionError);
                item.ForeColor = Color.Red;
                item.BackColor = Color.White;
            }

            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                info = new LineInfo();
                item.Tag = info;
            }

            info.LineState = state;
        }
         * */

        // 设置 item 的 Tag，并设置 item 的前景背景颜色
        // parameters:
        //      strInitialUsage 如果为null，则不设置此项
        void SetLineInfo(ListViewItem item,
            // string strInitialUsage,
            LineState state)
        {
            if (state == LineState.Normal)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionNormal);
                item.ForeColor = Color.Black;
                item.BackColor = Color.White;
            }
            else if (state == LineState.New)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionNew);
                item.ForeColor = Color.Black;
                item.BackColor = Color.LightGreen;
            }
            else if (state == LineState.Changed)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionChanged);
                item.ForeColor = Color.Black;
                item.BackColor = Color.Yellow;
            }
            else if (state == LineState.Deleted)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionDeleted);
                item.ForeColor = Color.DarkGray;
                item.BackColor = Color.White;
            }
            else if (state == LineState.Error)
            {
                ListViewUtil.ChangeItemText(item, 1, CaptionError);
                item.ForeColor = Color.Red;
                item.BackColor = Color.White;
            }

            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
            {
                info = new LineInfo();
                item.Tag = info;
            }

#if NO
            if (strInitialUsage != null)
                info.InitialUsage = strInitialUsage;
#endif
            info.LineState = state;
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        // 获得一个对象资源的元数据
        int GetOneObjectMetadata(
            string strBiblioRecPath,
            string strID,
            out string strMetadataXml,
            out byte[] timestamp,
            out string strError)
        {
            timestamp = null;
            strError = "";

            string strResPath = strBiblioRecPath + "/object/" + strID;

            strResPath = strResPath.Replace(":", "/");

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在下载对象的元数据 " + strResPath);
            Stop.BeginLoop();

            try
            {
                string strOutputPath = "";

                // EnableControlsInLoading(true);
                string strResult = "";
                // 只得到metadata
                long lRet = this.Channel.GetRes(
                    Stop,
                    strResPath,
                    "metadata,timestamp,outputpath",
                    out strResult,
                    out strMetadataXml,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "下载对象 " + strResPath + " 元数据失败，原因: " + strError;
                    return -1;
                }

                return 0;
            }
            finally
            {
                // EnableControlsInLoading(false);
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }
        }

        private void ListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool bHasBillioLoaded = false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
                bHasBillioLoaded = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modify_Click);
            if (this.ListView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_new_Click);
            if (bHasBillioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("标记删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_delete_Click);
            if (this.ListView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("撤销标记删除(&U)");
            menuItem.Click += new System.EventHandler(this.menu_undoDelete_Click);
            if (this.ListView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("导出(&E)");
            menuItem.Click += new System.EventHandler(this.menu_export_Click);
            if (this.ListView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // TODO: 这两个菜单项其实可以由 EntityForm 负责追加，处理函数也写在 EntityForm 中
            if (this.GenerateData != null)
            {
                // -----
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);

                // 创建数据
                menuItem = new MenuItem("创建数据[Ctrl+A](&G)");
                menuItem.Click += new System.EventHandler(this.menu_generateData_Click);
                /*
                if (this.ListView.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                 * */
                contextMenu.MenuItems.Add(menuItem);

                // 创建维护856字段
                menuItem = new MenuItem("创建维护856字段(&C)");
                menuItem.Click += new System.EventHandler(this.menu_manage856_Click);
                /*
                if (this.ListView.SelectedItems.Count == 0)
                    menuItem.Enabled = false;
                 * */
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.Show(this.ListView, new Point(e.X, e.Y));	
        }

        // 创建数据
        void menu_generateData_Click(object sender, EventArgs e)
        {
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.ListView;
                e1.ScriptEntry = "";    // 启动Ctrl+A菜单
                this.GenerateData(this, e1);
            }
            else
            {
                MessageBox.Show(this, "EntityControl没有挂接GenerateData事件");
            }
        }

        // 创建索取号
        void menu_manage856_Click(object sender, EventArgs e)
        {
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.ListView;
                e1.ScriptEntry = "Manage856";    // 直接启动Manage856()函数
                this.GenerateData(this, e1);
            }
            else
            {
                MessageBox.Show(this, "EntityControl没有挂接GenerateData事件");
            }
        }

        void menu_modify_Click(object sender, EventArgs e)
        {
            if (this.ListView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的行...");
                return;
            }

            ListViewItem item = this.ListView.SelectedItems[0];
            LineState old_state = GetLineState(item);

            if (old_state == LineState.Deleted)
            {
                MessageBox.Show(this, "对已经标记删除的行不能进行修改...");
                return;
            }

            ResObjectDlg dlg = new ResObjectDlg();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.ID = ListViewUtil.GetItemText(item, COLUMN_ID);

            dlg.State = ListViewUtil.GetItemText(item, COLUMN_STATE);
            dlg.Mime = ListViewUtil.GetItemText(item, COLUMN_MIME);
            dlg.LocalPath = ListViewUtil.GetItemText(item, COLUMN_LOCALPATH);
            dlg.SizeString = ListViewUtil.GetItemText(item, COLUMN_SIZE);
            dlg.Timestamp = ListViewUtil.GetItemText(item, COLUMN_TIMESTAMP);
            dlg.Usage = ListViewUtil.GetItemText(item, COLUMN_USAGE);
            dlg.Rights = ListViewUtil.GetItemText(item, COLUMN_RIGHTS);
            dlg.RightsCfgFileName = this.RightsCfgFileName;

            string strOldUsage = dlg.Usage;
            string strOldRights = dlg.Rights;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (old_state != LineState.New)
            {
                SetLineInfo(item, 
                    // null, 
                    LineState.Changed);
                SetResChanged(item, dlg.ResChanged);
            }
            else
            {
                SetResChanged(item, true);
            }

            if (strOldRights != dlg.Rights
                || strOldUsage != dlg.Usage)
                SetXmlChanged(item, true);

            ListViewUtil.ChangeItemText(item, COLUMN_MIME, dlg.Mime);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, dlg.LocalPath);
            ListViewUtil.ChangeItemText(item, COLUMN_SIZE, dlg.SizeString);
            ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, dlg.Timestamp);
            ListViewUtil.ChangeItemText(item, COLUMN_USAGE, dlg.Usage);
            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, dlg.Rights);
            this.Changed = true;
        }

        string GetNewID()
        {
            List<string> ids = new List<string>();
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                string strCurrentID = ListViewUtil.GetItemText(this.ListView.Items[i], COLUMN_ID);
                ids.Add(strCurrentID);
            }

            int nSeed = 0;
            string strID = "";
            for (; ; )
            {
                strID = Convert.ToString(nSeed++);
                if (ids.IndexOf(strID) == -1)
                    return strID;
            }

        }

        void menu_new_Click(object sender, EventArgs e)
        {
            ResObjectDlg dlg = new ResObjectDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.ID = GetNewID();
            dlg.State = "";
            dlg.RightsCfgFileName = this.RightsCfgFileName;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewItem item = new ListViewItem();
            this.ListView.Items.Add(item);

            SetLineInfo(item,
                // null,
                LineState.New);
            SetResChanged(item, true);
            SetXmlChanged(item, true);

            ListViewUtil.ChangeItemText(item, COLUMN_ID, dlg.ID);
            ListViewUtil.ChangeItemText(item, COLUMN_MIME, dlg.Mime);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, dlg.LocalPath);
            ListViewUtil.ChangeItemText(item, COLUMN_SIZE, dlg.SizeString);
            ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, dlg.Timestamp);
            ListViewUtil.ChangeItemText(item, COLUMN_USAGE, dlg.Usage);
            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, dlg.Rights);
            this.Changed = true;
        }

        // 按照usage字符串搜寻事项
        public List<ListViewItem> FindItemByUsage(string strUsage)
        {
            List<ListViewItem> results = new List<ListViewItem>();
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                ListViewItem item = this.ListView.Items[i];
                string strCurrentUsage = ListViewUtil.GetItemText(item, COLUMN_USAGE);
                if (strCurrentUsage == strUsage)
                    results.Add(item);
            }

            return results;
        }

        // TODO: findItemByRights 可以用一个或者多个 right 值进行搜寻

        // 返回全部已经标记删除的事项
        public List<ListViewItem> FindAllMaskDeleteItem()
        {
            List<ListViewItem> results = new List<ListViewItem>();
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                ListViewItem item = this.ListView.Items[i];
                LineState old_state = GetLineState(item);

                if (old_state == LineState.Deleted)
                    results.Add(item);
            }

            return results;
        }

        // 按照id字符串搜寻事项
        public List<ListViewItem> FindItemByID(string strID)
        {
            List<ListViewItem> results = new List<ListViewItem>();
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                ListViewItem item = this.ListView.Items[i];
                string strCurrentID = ListViewUtil.GetItemText(item, COLUMN_ID);
                if (strID == "*" || strCurrentID == strID)
                    results.Add(item);
            }

            return results;
        }

        // 获得一个事项的尚未上载的本地文件名
        // parameters:
        // return:
        //      -1  出错
        //      0   不属于修改或者创建后尚未上载的情况
        //      1   成功
        public int GetUnuploadFilePath(ListViewItem item,
            out string strLocalPath,
            out string strError)
        {
            strError = "";
            strLocalPath = "";

            if (this.ListView.Items.IndexOf(item) == -1)
            {
                strError = "item不是当前ListView的事项之一";
                return -1;
            }

            LineState state = GetLineState(item);
            LineInfo info = (LineInfo)item.Tag;

            if (state == LineState.Changed ||
                state == LineState.New)
            {
                if (state == LineState.Changed)
                {
                    if (info != null
                        && info.ResChanged == false)
                    {
                        return 0;   // 资源没有修改的
                    }
                }
                strLocalPath = ListViewUtil.GetItemText(item, COLUMN_LOCALPATH);
                return 1;
            }

            return 0;
        }

        public bool ChangeObjectRights(ListViewItem item, string strRights)
        {
            string strExistingRights = ListViewUtil.GetItemText(item, COLUMN_RIGHTS);
            if (strRights == strExistingRights)
                return false;

            LineState old_state = GetLineState(item);

            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, strRights);

            if (old_state != LineState.New)
                SetLineInfo(item, LineState.Changed);

            SetXmlChanged(item, true);
            this.Changed = true;
            return true;
        }

        public int ChangeObjectFile(ListViewItem item,
    string strObjectFilePath,
    string strUsage,
    out string strError)
        {
            return ChangeObjectFile(item, strObjectFilePath, strUsage, "", out strError);
        }

        public int ChangeObjectFile(ListViewItem item,
            string strObjectFilePath,
            string strUsage,
            string strRights,
            out string strError)
        {
            strError = "";

            if (this.ListView.Items.IndexOf(item) == -1)
            {
                strError = "item不是当前ListView的事项之一";
                return -1;
            }

            LineState old_state = GetLineState(item);
            if (old_state == LineState.Deleted)
            {
                strError = "对已经标记删除的行不能进行修改...";
                return -1;
            }
            ResObjectDlg dlg = new ResObjectDlg();
            dlg.ID = ListViewUtil.GetItemText(item, COLUMN_ID);

            dlg.State = ListViewUtil.GetItemText(item, COLUMN_STATE);
            dlg.Mime = ListViewUtil.GetItemText(item, COLUMN_MIME);
            dlg.LocalPath = ListViewUtil.GetItemText(item, COLUMN_LOCALPATH);
            dlg.SizeString = ListViewUtil.GetItemText(item, COLUMN_SIZE);
            dlg.Timestamp = ListViewUtil.GetItemText(item, COLUMN_TIMESTAMP);
            dlg.Usage = strUsage;
            dlg.Rights = strRights;
            dlg.RightsCfgFileName = this.RightsCfgFileName;

            string strOldUsage = dlg.Usage;
            string strOldRights = dlg.Rights;

            int nRet = dlg.SetObjectFilePath(strObjectFilePath,
            out strError);
            if (nRet == -1)
                return -1;

            if (old_state != LineState.New)
            {
                SetLineInfo(item, 
                    // null, 
                    LineState.Changed);
                SetResChanged(item, true);
            }
            else
            {
                SetResChanged(item, true);
            }

            if (strOldRights != dlg.Rights
                || strOldUsage != dlg.Usage)
                SetXmlChanged(item, true);

            ListViewUtil.ChangeItemText(item, COLUMN_MIME, dlg.Mime);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, dlg.LocalPath);
            ListViewUtil.ChangeItemText(item, COLUMN_SIZE, dlg.SizeString);
            ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, dlg.Timestamp);
            ListViewUtil.ChangeItemText(item, COLUMN_USAGE, dlg.Usage);
            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, dlg.Rights);
            this.Changed = true;
            return 0;
        }

        // 包装后的版本，兼容以前的用法
        public int AppendNewItem(
            string strObjectFilePath,
            string strUsage,
            out ListViewItem item,
            out string strError)
        {
            return AppendNewItem(
                strObjectFilePath,
                strUsage,
                "",
                out item,
                out strError);
        }

        /// <summary>
        /// 追加一个对象
        /// </summary>
        /// <param name="strObjectFilePath">对象文件名全路径</param>
        /// <param name="strUsage">用途字符串</param>
        /// <param name="strRights">权限</param>
        /// <param name="item">返回 ListView 中心创建的 ListViewItem 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int AppendNewItem(
            string strObjectFilePath,
            string strUsage,
            string strRights,
            out ListViewItem item,
            out string strError)
        {
            strError = "";
            item = null;

            ResObjectDlg dlg = new ResObjectDlg();
            dlg.ID = GetNewID();

            dlg.State = "";
            dlg.Usage = strUsage;
            dlg.Rights = strRights;
            dlg.RightsCfgFileName = this.RightsCfgFileName;

            int nRet = dlg.SetObjectFilePath(strObjectFilePath,
                out strError);
            if (nRet == -1)
                return -1;

            item = new ListViewItem();
            this.ListView.Items.Add(item);

            SetLineInfo(item,
                // null,
                LineState.New);
            SetResChanged(item, true);
            SetXmlChanged(item, true);

            ListViewUtil.ChangeItemText(item, COLUMN_ID, dlg.ID);
            ListViewUtil.ChangeItemText(item, COLUMN_MIME, dlg.Mime);
            ListViewUtil.ChangeItemText(item, COLUMN_LOCALPATH, dlg.LocalPath);
            ListViewUtil.ChangeItemText(item, COLUMN_SIZE, dlg.SizeString);
            ListViewUtil.ChangeItemText(item, COLUMN_TIMESTAMP, dlg.Timestamp);
            ListViewUtil.ChangeItemText(item, COLUMN_USAGE, dlg.Usage);
            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, dlg.Rights);
            this.Changed = true;
            return 0;
        }

        // 设置一行的修改状态
        // 当外部脚本直接修改 ListViewItem 的 SubItems 以后，还需要用本函数标定修改状态，后面才能顺利保存修改
        public void SetItemChanged(ListViewItem item,
            bool bResChanged = true,
            bool bXmlChanged = true)
        {
            LineState old_state = GetLineState(item);

            if (old_state == LineState.Deleted)
            {
                throw new Exception("对已经标记删除的行不能进行修改...");
            }

            if (bResChanged)
                SetResChanged(item, true);
            if (bXmlChanged)
                SetXmlChanged(item, true);

            if (old_state != LineState.New)
                SetLineInfo(item, LineState.Changed);

            this.Changed = true;
        }

        // 标记删除若干事项
        public int MaskDelete(List<ListViewItem> items)
        {
            //bool bRemoved = false;   // 是否发生过物理删除listview item的情况
            int nMaskDeleteCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                LineState state = GetLineState(item);

                // 如果本来就是已经标记删除的事项
                if (state == LineState.Deleted)
                    continue;

                // 如果本来就是新增事项，那么彻底从listview中移除
                if (state == LineState.New)
                {
                    //bRemoved = true;
                    this.ListView.Items.Remove(item);
                    continue;
                }

                // 保存旧状态
                SetOldLineState(item, state);

                SetLineInfo(item, 
                    // null, 
                    LineState.Deleted);

                this.Changed = true;

                nMaskDeleteCount++;
            }

            return nMaskDeleteCount;
        }

        void menu_delete_Click(object sender, EventArgs e)
        {
            if (this.ListView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的行...");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要标记删除选定的 "+this.ListView.SelectedItems.Count.ToString()+" 个对象? ",
                "BinaryResControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.ListView.SelectedItems)
            {
                items.Add(item);
            }

            bool bRemoved = false;   // 是否发生过物理删除listview item的情况
            int nMaskDeleteCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                LineState state = GetLineState(item);

                // 如果本来就是已经标记删除的事项
                if (state == LineState.Deleted)
                    continue;

                // 如果本来就是新增事项，那么彻底从listview中移除
                if (state == LineState.New)
                {
                    bRemoved = true;
                    this.ListView.Items.Remove(item);
                    continue;
                }

                // 保存旧状态
                SetOldLineState(item, state);

                SetLineInfo(item, 
                    // null, 
                    LineState.Deleted);

                this.Changed = true;

                nMaskDeleteCount++;
            }

            if (bRemoved == true)
            {
                // 需要看看listview中是不是至少有一个需要保存的事项？否则Changed设为false
                if (IsChanged() == false)
                {
                    this.Changed = false;
                    return;
                }
            }

            if (nMaskDeleteCount > 0)
                MessageBox.Show(this, "注意：标记删除的事项直到提交/保存时才会真正从服务器删除。");
        }


        void menu_undoDelete_Click(object sender, EventArgs e)
        {
            if (this.ListView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要撤销标记删除的行...");
                return;
            }

            int nNotDeleted = 0;    // 用户选择要撤销删除的事项中，有多少项本来就不是已经标记删除的事项？
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.ListView.SelectedItems)
            {
                // ListViewItem item = this.ListView.SelectedItems[i];

                if (GetLineState(item) != LineState.Deleted)
                {
                    nNotDeleted ++;
                    continue;
                }

                items.Add(item);
            }

            foreach (ListViewItem item in items)
            {
                // ListViewItem item = items[i];

                LineState old_state = GetOldLineState(item);

                Debug.Assert(old_state != LineState.Deleted, "");

                // 恢复标记删除前的旧状态
                SetLineInfo(item,
                    // null, 
                    old_state);

                this.Changed = true;
            }

            // 需要看看listview中是不是至少有一个需要保存的事项？否则Changed设为false
            if (IsChanged() == false)
                this.Changed = false;
        }

        // 导出对象到文件
        void menu_export_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.ListView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要导出的行...");
                return;
            }

            if (this.ListView.SelectedItems.Count != 1)
            {
                MessageBox.Show(this, "一次只能选择一行导出...");
                return;
            }

            ListViewItem item = this.ListView.SelectedItems[0];

            LineState state = GetLineState(item);

            if (state == LineState.New)
            {
                strError = "尚未上载的对象，本来就在本地，无需导出";
                goto ERROR1;
            }

            if (state == LineState.Changed)
            {
                strError = "已经修改而未提交的对象，本来就在本地，无需导出";
                goto ERROR1;
            }

            string strID = ListViewUtil.GetItemText(item, COLUMN_ID);
            string strLocalPath = ListViewUtil.GetItemText(item, COLUMN_LOCALPATH);

            string strResPath = this.BiblioRecPath + "/object/" + strID;

            strResPath = strResPath.Replace(":", "/");


            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的本地文件名";
            dlg.CreatePrompt = false;
            dlg.FileName = strLocalPath == "" ? strID + ".res" : strLocalPath;
            dlg.InitialDirectory = Environment.CurrentDirectory;
            // dlg.Filter = "projects files (outer*.xml)|outer*.xml|All files (*.*)|*.*" ;

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在下载对象 " + strResPath);
            Stop.BeginLoop();

            try
            {
                byte[] baOutputTimeStamp = null;

                // EnableControlsInLoading(true);

                string strMetaData;
                string strOutputPath = "";

                long lRet = this.Channel.GetRes(
                    Stop,
                    strResPath,
                    dlg.FileName,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                // EnableControlsInLoading(false);
                if (lRet == -1)
                {
                    strError = "下载资源文件失败，原因: " + strError;
                    goto ERROR1;
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 确认是否还有增删改的事项
        // 而this.Changed不是那么精确的
        bool IsChanged()
        {
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                LineState state = GetLineState(this.ListView.Items[i]);
                if (state == LineState.Changed
                    || state == LineState.Deleted
                    || state == LineState.New)
                    return true;
            }

            return false;
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            menu_modify_Click(sender, e);
        }

        // ID和usage是否发生改变?
        // 有增删对象的操作，ID会发生改变。单纯修改一行，ID不发生改变
        // ID有改变，就需要重新构造biblioxml保存到服务器
        public bool IsIdUsageChanged()
        {
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                ListViewItem item = this.ListView.Items[i];

                LineState state = GetLineState(item);

                if (state == LineState.New
                    || state == LineState.Deleted)
                    return true;

                // 观察usage是否改变
                LineInfo info = (LineInfo)item.Tag;
                if (info != null)
                {
#if NO
                    string strUsage = ListViewUtil.GetItemText(item, COLUMN_USAGE);
                    if (strUsage != info.InitialUsage)
                        return true;
#endif
                    if (info.XmlChanged == true)
                        return true;
                }
            }

            return false;
        }

        // 在 XmlDocument 对象中添加 <file> 元素。新元素加入在根之下
        public int AddFileFragments(ref XmlDocument domRecord,
            out string strError)
        {
            strError = "";
            foreach (ListViewItem item in this.ListView.Items)
            {
                string strID = ListViewUtil.GetItemText(item, COLUMN_ID);

                if (String.IsNullOrEmpty(strID) == true)
                    continue;

                LineState state = GetLineState(item);
                // 如果是已经标记删除的事项
                if (state == LineState.Deleted)
                    continue;

                string strUsage = ListViewUtil.GetItemText(item, COLUMN_USAGE);
                string strRights = ListViewUtil.GetItemText(item, COLUMN_RIGHTS);

                XmlElement node = domRecord.CreateElement("dprms",
                    "file",
                    DpNs.dprms);
                domRecord.DocumentElement.AppendChild(node);

                node.SetAttribute("id", strID);
                if (string.IsNullOrEmpty(strUsage) == false)
                    node.SetAttribute("usage", strUsage);
                if (string.IsNullOrEmpty(strRights) == false)
                    node.SetAttribute("rights", strRights);
            }

            return 0;
        }

#if NO
        // 获得全部ID
        public List<string> GetIds()
        {
            List<string> results = new List<string>();

            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                ListViewItem item = this.ListView.Items[i];

                LineState state = GetLineState(item);

                if (state == LineState.Deleted)
                    continue;   // 忽略标记删除的事项

                string strID = ListViewUtil.GetItemText(item ,COLUMN_ID);

                results.Add(strID);
            }

            return results;
        }

        // 获得全部usage
        public List<string> GetUsages()
        {
            List<string> results = new List<string>();

            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                ListViewItem item = this.ListView.Items[i];

                LineState state = GetLineState(item);

                if (state == LineState.Deleted)
                    continue;   // 忽略标记删除的事项

                string strUsage = ListViewUtil.GetItemText(item, COLUMN_USAGE);
                results.Add(strUsage);
            }

            return results;
        }

#endif

        // 从路径中取出记录号部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

        // 是否为新增记录的路径
        public static bool IsNewPath(string strPath)
        {
            if (String.IsNullOrEmpty(strPath) == true)
                return true;    //???? 空路径当作新路径?

            string strID = GetRecordID(strPath);

            if (strID == "?"
                || String.IsNullOrEmpty(strID) == true) // 2008/11/28 
                return true;

            return false;
        }

        // 保存资源到服务器
        // return:
        //		-1	error
        //		>=0 实际上载的资源对象数
        public int Save(
            out string strError)
        {
            strError = "";

            if (this.ListView.Items.Count == 0)
                return 0;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未指定BiblioRecPath";
                return -1;
            }

            if (IsNewPath(this.BiblioRecPath) == true)
            {
                strError = "书目记录路径 '" + this.BiblioRecPath + "' 不是已保存的记录路径，无法用于对象资源上载";
                return -1;
            }

            if (this.Channel == null)
            {
                strError = "BinaryResControl尚未指定Channel";
                return -1;
            }

            StopStyle old_stop_style = StopStyle.None;

            if (Stop != null)
            {
                old_stop_style = Stop.Style;
                Stop.Style = StopStyle.EnableHalfStop;

                Stop.OnStop += new StopEventHandler(this.DoStop);
                Stop.Initial("正在上载资源 ...");
                Stop.BeginLoop();
            }

            int nUploadCount = 0;   // 实际上载的资源个数

            try
            {
                // bNotAskTimestampMismatchWhenOverwrite = false;

                for (int i = 0; i < this.ListView.Items.Count; i++)
                {
                    ListViewItem item = this.ListView.Items[i];
                    LineInfo info = (LineInfo)item.Tag;
                    // string strUsage = ListViewUtil.GetItemText(item, COLUMN_USAGE);

                    LineState state = GetLineState(item);

                    if (state == LineState.Changed ||
                        state == LineState.New)
                    {
                        if (state == LineState.Changed)
                        {
                            if (info != null
                                && info.ResChanged == false)
                            {
                                SetLineInfo(item,
                                    // strUsage, 
                                    LineState.Normal);
                                SetXmlChanged(item, false);
                                SetResChanged(item, false);
                                continue;   // 资源没有修改的，则跳过上载
                            }
                        }
                    }
                    else
                    {
                        // 标记删除的事项，只要书目XML重新构造的时候
                        // 不包含其ID，书目XML保存后，就等于删除了该事项。
                        // 所以本函数只是简单Remove这样的listview事项即可
                        if (state == LineState.Deleted)
                        {
                            this.ListView.Items.Remove(item);
                            i--;
                        }

                        continue;
                    }

                    string strState = ListViewUtil.GetItemText(item, COLUMN_STATE);

                    string strID = ListViewUtil.GetItemText(item, COLUMN_ID);
                    string strResPath = this.BiblioRecPath + "/object/" + ListViewUtil.GetItemText(item, COLUMN_ID);
                    string strLocalFilename = ListViewUtil.GetItemText(item, COLUMN_LOCALPATH);
                    string strMime = ListViewUtil.GetItemText(item, COLUMN_MIME);
                    string strTimestamp = ListViewUtil.GetItemText(item, COLUMN_TIMESTAMP);

                    // 检测文件尺寸
                    FileInfo fi = new FileInfo(strLocalFilename);

                    if (fi.Exists == false)
                    {
                        strError = "文件 '" + strLocalFilename + "' 不存在...";
                        return -1;
                    }

                    string[] ranges = null;

                    if (fi.Length == 0)
                    {
                        // 空文件
                        ranges = new string[1];
                        ranges[0] = "";
                    }
                    else
                    {
                        string strRange = "";
                        strRange = "0-" + Convert.ToString(fi.Length - 1);

                        // 按照100K作为一个chunk
                        // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                        ranges = RangeList.ChunkRange(strRange,
                            500 * 1024);
                    }

                    byte[] timestamp = ByteArray.GetTimeStampByteArray(strTimestamp);
                    byte[] output_timestamp = null;

                    nUploadCount++;

                    // REDOWHOLESAVE:
                    string strWarning = "";

                    for (int j = 0; j < ranges.Length; j++)
                    {
                        // REDOSINGLESAVE:

                        Application.DoEvents();	// 出让界面控制权

                        if (Stop.State != 0)
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
                            strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                        }

                        if (Stop != null)
                            Stop.SetMessage("正在上载 " + ranges[j] + "/"
                                + Convert.ToString(fi.Length)
                                + " " + strPercent + " " + strLocalFilename + strWarning + strWaiting);

                        long lRet = this.Channel.SaveResObject(
                            Stop,
                            strResPath,
                            strLocalFilename,
                            strLocalFilename,
                            strMime,
                            ranges[j],
                            j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                            timestamp,
                            out output_timestamp,
                            out strError);
                        timestamp = output_timestamp;

                        ListViewUtil.ChangeItemText(item,
                            COLUMN_TIMESTAMP,
                            ByteArray.GetHexTimeStampString(timestamp));

                        strWarning = "";

                        if (lRet == -1)
                        {
                            /*
                            if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                            {

                                if (this.bNotAskTimestampMismatchWhenOverwrite == true)
                                {
                                    timestamp = new byte[output_timestamp.Length];
                                    Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                                    strWarning = " (时间戳不匹配, 自动重试)";
                                    if (ranges.Length == 1 || j == 0)
                                        goto REDOSINGLESAVE;
                                    goto REDOWHOLESAVE;
                                }


                                DialogResult result = MessageDlg.Show(this,
                                    "上载 '" + strLocalFilename + "' (片断:" + ranges[j] + "/总尺寸:" + Convert.ToString(fi.Length)
                                    + ") 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
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
                                    if (ranges.Length == 1 || j == 0)
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
                             * */

                            goto ERROR1;
                        }
                    }

                    SetLineInfo(item, 
                        // strUsage, 
                        LineState.Normal);
                    SetXmlChanged(item, false);
                    SetResChanged(item, false);
                }

                this.Changed = false;
                return nUploadCount;
            ERROR1:
                return -1;
            }
            finally
            {
                if (Stop != null)
                {
                    Stop.EndLoop();
                    Stop.OnStop -= new StopEventHandler(this.DoStop);
                    if (nUploadCount > 0)
                        Stop.Initial("上载资源完成");
                    else
                        Stop.Initial("");
                    Stop.Style = old_stop_style;
                }
            }
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control == true)
            {
                // Ctrl+A
                menu_generateData_Click(sender, null);
            }
        }

        public string ErrorInfo
        {
            get
            {
                return this.Text;
            }
            set
            {
                this.Text = value;
                if (this.ListView != null)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        this.ListView.Visible = true;
                    else
                        this.ListView.Visible = false;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 绘制错误信息字符串
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
#if NO
            Brush brush = new SolidBrush(Color.FromArgb(100, 0,0,255));
            e.Graphics.FillEllipse(brush, 30, 30, 100, 100);
#endif
            if (string.IsNullOrEmpty(this.Text) == true)
                return;

            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            format.Alignment = StringAlignment.Center;
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = e.Graphics.MeasureString(this.Text,
                this.Font,
                this.Size.Width,
                format);

            RectangleF textRect = new RectangleF(
(this.Size.Width - size.Width) / 2,
(this.Size.Height - size.Height) / 2,
size.Width,
size.Height);
            using (Brush brush = new SolidBrush(this.ForeColor))
            {
                e.Graphics.DrawString(
                    this.Text,
                    this.Font,
                    brush,
                    textRect,
                    format);
            }
        }

    }

    class LineInfo
    {
        public LineState LineState = LineState.Normal;

        // 标记删除前的状态
        public LineState OldLineState = LineState.Normal;

        // public string InitialUsage = "";    // 最初的usage值

        public bool ResChanged = false; // 资源是否修改过。如果一个事项修改过，但是资源没有修改过，则为usage修改过

        public bool XmlChanged = false; // 描述资源的 usage rights 是否修改过。2015/7/11
    }

    enum LineState
    {
        Normal = 0, // 普通，已经上载的事项
        New = 1,    // 新增的，尚未上载的时候
        Changed = 2,    // 修改过的，新内容尚未上载的事项
        Deleted = 3,    // 标记删除的，尚未提交删除的事项
        Error = 4,  // 获得metadata时出错的事项
    }
}
