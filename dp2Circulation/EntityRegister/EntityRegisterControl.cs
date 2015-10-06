using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.Net;

using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.AmazonInterface;

namespace dp2Circulation
{
    /// <summary>
    /// 快速册登记主体控件
    /// 每个书目信息行后面跟一个册信息行
    /// </summary>
    public partial class EntityRegisterControl : TableControlBase
    {
        EntityRegisterBase _base = new EntityRegisterBase();

        // 长操作时的动画
        public Image LoaderImage = null;

        public ColorSummaryControl ColorControl = null;

        public event DisplayErrorEventHandler DisplayError = null;

        FillThread _fillThread = null;

        internal ImageManager _imageManager = null;

        MainForm _mainForm = null;
        public MainForm MainForm
        {
            get
            {
                // return this._mainForm;
                return this._base.MainForm;
            }
            set
            {
                // this._mainForm = value;
                this._base.MainForm = value;
                if (value != null && this._imageManager != null)
                {
                    this._imageManager.TempFileDir = value.UserTempDir;
                }
            }
        }



        // public LibraryChannel Channel = null;


        List<LineTask> _loadEntityTasks = new List<LineTask>();

        public EntityRegisterControl()
        {
            InitializeComponent();

            this.TableLayoutPanel = this.tableLayoutPanel1;

            Debug.Assert(tableLayoutPanel1.RowStyles.Count == 1, "");

            this.tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            this.tableLayoutPanel1.Padding = new Padding(0,0, SystemInformation.VerticalScrollBarWidth, 0);

            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.AutoScroll = false;

            // 确保垂直卷滚条显示出来
            // this.tableLayoutPanel1.RowStyles[this.tableLayoutPanel1.RowStyles.Count - 1] = new RowStyle(SizeType.Absolute, Screen.PrimaryScreen.Bounds.Height + 100);
#if NO
            this.tableLayoutPanel1.AutoScroll = false;
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;

            this.AutoScroll = false;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
#endif
            // this._channelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);

            this._imageManager = new ImageManager();
            this._imageManager.ChannelPool = this._base._channelPool;
            this._imageManager.GetObjectComplete += new GetObjectCompleteEventHandler(_imageManager_GetObjectComplete);
            this._imageManager.BeginThread();
        }

        void _imageManager_GetObjectComplete(object sender, GetObjectCompleteEventArgs e)
        {
            EventFilter filter = e.TraceObject.Tag as EventFilter;
            filter.BiblioRegister.AsyncGetImageComplete(filter.Row,
                filter.BiblioRecPath,
                e.TraceObject != null ? e.TraceObject.FileName : null,
                e.ErrorInfo);
        }

        internal void OnGetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";

#if NO
            // sender 为 BiblioResiterControl
            BiblioRegisterControl control = sender as BiblioRegisterControl;
            string strServerName = control.ServerName;
            string strServerType = GetServerType(strServerName);
            if (strServerType == "amazon" || strServerName == "!unknown")
            {

            }

            AccountInfo account = this.GetAccountInfo(strServerName);
            if (account == null)
            {
                strError = "服务器名 '" + strServerName + "' 没有配置";
                goto ERROR1;
            }

            if (account.IsLocalServer == true)
            {
                string[] values = null;
                int nRet = MainForm.GetValueTable(e.TableName,
                    e.DbName,
                    out values,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                e.values = values;
            }
#endif
            // 无论是否本地的记录，都按照本地的值列表来显示
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            e.values = values;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#if NO
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
#endif
        }

        // 新添加一个书目事项
        public void AddNewBiblio(string strText)
        {
            RegisterLine line = new RegisterLine(this);
            line.BiblioBarcode = strText;

            this.InsertNewLine(0, line, true);
            line.label_color.Focus();

            line._biblioRegister.BarColor = "B";    // 黑色，表示刚加入，还来不及处理
            this.SetColorList();

            this.AddTask(line, "search_biblio");

            // 选定刚新增的事项
            this.SelectItem(line, true);
            line._biblioRegister.Focus();
            // 确保事项可见
            this.EnsureVisible(line);
        }

        // 新添加一个实体事项
        public int AddNewEntity(string strText, out string strError)
        {
            strError = "";
            int nRet = 0;

            List<TableItemBase> selected_items = this.SelectedItems;
            if (selected_items.Count == 0)
            {
                strError = "当前没有选定的书目事项，新增册记录操作无法进行。请先选定一个要添加册记录的书目事项；或先扫入一个 ISBN 定位到书目事项";
                goto ERROR1;
            }
            else if (selected_items.Count > 1)
            {
                strError = "当前选定了多于一个的书目事项，新增册记录操作无法进行。请先选定一个要添加册记录的书目事项；或先扫入一个 ISBN 定位到书目事项";
                goto ERROR1;
            }
            else
            {
                RegisterLine line = selected_items[0] as RegisterLine;

                if (line != null)
                {
                    // 根据缺省值，构造最初的 XML
                    string strQuickDefault = this.MainForm.AppInfo.GetString(
"entityform_optiondlg",
"quickRegister_default",
"<root />");
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strQuickDefault);
                    }
                    catch (Exception ex)
                    {
                        strError = "缺省记录装入 XMLDOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    DomUtil.SetElementText(dom.DocumentElement, "barcode", strText);
                    DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());

                    // 兑现 @price 宏，如果有书目记录的话
                    // TODO: 当书目记录切换的时候，是否还要重新兑现一次宏?

                    // 记录路径可以保持为空，直到保存前才构造
                    nRet = line.ReplaceEntityMacro(dom,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // 添加一个新的册对象
                    nRet = line.NewEntity(dom.DocumentElement.OuterXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 如果不是详细模式，会看不到新插入的实体，所以需要切换为详细模式
                    if (line._biblioRegister.DisplayMode != "detail")
                        line._biblioRegister.DisplayMode = "detail";
                }
            }

            return 0;
        ERROR1:
            return -1;
        }

        public void SelectLine(int index)
        {
            RegisterLine line = this.Items[index] as RegisterLine;
            // 选定刚新增的事项
            this.SelectItem(line, true);
            // 确保事项可见
            this.EnsureVisible(line);
        }

        private void EntityRegisterControl_SizeChanged(object sender, EventArgs e)
        {
            // this.AdjectHeight();

            // this.AutoScrollMinSize = this.tableLayoutPanel1.Size;

            int i = 0;
            i++;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.AdjectHeight();

            base.OnSizeChanged(e);
        }

        void AdjectHeight()
        {
            foreach (RegisterLine line in this.Items)
            {
                line.AdjustHeight();
            }
        }



        public void MarcEditor_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            // Debug.Assert(false, "");

            // 路径中应该包含 @服务器名

            if (String.IsNullOrEmpty(e.Path) == true)
            {
                e.ErrorInfo = "e.Path 为空，无法获得配置文件";
                goto ERROR1;
            }

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(e.Path, "@", out strPath, out strServerName);

            string strServerType = _base.GetServerType(strServerName);
            if (strServerType == "amazon" || strServerName == "!unknown")
            {
                // TODO: 如何知道 MARC 记录是什么具体的 MARC 格式?
                // 可能需要在服务器信息中增加一个首选的 MARC 格式属性
                string strFileName = Path.Combine(this.MainForm.DataDir, "unimarc_cfgs/" + strPath);

                // 在cache中寻找
                e.XmlDocument = this.MainForm.DomCache.FindObject(strFileName);
                if (e.XmlDocument != null)
                    return;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "配置文件 '" + strFileName + "' 装入 XMLDOM 时出错: " + ex.Message;
                    goto ERROR1;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strFileName, dom);  // 保存到缓存
                return;
            }

            AccountInfo account = _base.GetAccountInfo(strServerName);
            if (account == null)
            {
                e.ErrorInfo = "e.Path 中 '"+e.Path+"' 服务器名 '"+strServerName+"' 没有配置";
                goto ERROR1;
            }

            Debug.Assert(strServerType == "dp2library", "");

            BiblioRegisterControl control = sender as BiblioRegisterControl;
            string strBiblioDbName = Global.GetDbName(control.BiblioRecPath);

            // 得到干净的文件名
            string strCfgFilePath = strBiblioDbName + "/cfgs/" + strPath;    // e.Path;
            int nRet = strCfgFilePath.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFilePath = strCfgFilePath.Substring(0, nRet);
            }

            // 在cache中寻找
            e.XmlDocument = this.MainForm.DomCache.FindObject(strCfgFilePath);
            if (e.XmlDocument != null)
                return;

            // TODO: 可以通过服务器名，得到 url username 等配置参数

            // 下载配置文件
            string strContent = "";
            string strError = "";

            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetCfgFileContent(
                account.ServerUrl,
                account.UserName,
                strCfgFilePath,
                out strContent,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                e.ErrorInfo = "获得配置文件 '" + strCfgFilePath + "' 时出错：" + strError;
                goto ERROR1;
            }
            else
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strContent);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "配置文件 '" + strCfgFilePath + "' 装入XMLDUM时出错: " + ex.Message;
                    goto ERROR1;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strCfgFilePath, dom);  // 保存到缓存
            }

            return;
        ERROR1:
            this.DisplayFloatErrorText(e.ErrorInfo, "");
        }

        internal void AsyncGetImageFile(BiblioRegisterControl biblioRegister,
            AsyncGetImageEventArgs e)
        {
            string strError = "";
            if (String.IsNullOrEmpty(e.RecPath) == true)
            {
                strError = "e.RecPath 为空，无法获得图像文件";
                goto ERROR1;
            }

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(e.RecPath, "@", out strPath, out strServerName);

            AccountInfo account = _base.GetAccountInfo(strServerName);
            if (account == null)
            {
                strError = "e.RecPath 中 '" + e.RecPath + "' 服务器名 '" + strServerName + "' 没有配置";
                goto ERROR1;
            }
            string strServerUrl = account.ServerUrl;
            string strUserName = account.UserName;

            if (EntityRegisterBase.IsDot(strServerUrl) == true)
                strServerUrl = this.MainForm.LibraryServerUrl;
            if (EntityRegisterBase.IsDot(strUserName) == true)
                strUserName = this.MainForm.DefaultUserName;

            EventFilter filter = new EventFilter();
            filter.BiblioRegister = biblioRegister;
            filter.Row = e.Row;
            filter.BiblioRecPath = e.RecPath;

            string strObjectPath = strPath + "/object/" + e.ObjectPath;
            this._imageManager.AsyncGetObjectFile(strServerUrl, 
                strUserName,
                strObjectPath,
                e.FileName,
                filter);

            return;
        ERROR1:
            biblioRegister.AsyncGetImageComplete(e.Row,
                "",
                null,
                strError);
        }

        // 事件筛选
        class EventFilter
        {
            public BiblioRegisterControl BiblioRegister = null;
            public DpRow Row = null;    // 浏览行对象
            public string BiblioRecPath = "";   // 书目记录路径
        }

        int m_nInGetCfgFile = 0;

        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetCfgFileContent(
            string strServerUrl,
            string strUserName,
            string strCfgFilePath,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strContent = "";

            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }


            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在下载配置文件 ...");
            Progress.BeginLoop();

            m_nInGetCfgFile++;

            LibraryChannel channel = _base.GetChannel(strServerUrl, strUserName);


            try
            {
                Progress.SetMessage("正在下载配置文件 " + strCfgFilePath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                // TODO: 应该按照 URL 区分
                long lRet = channel.GetRes(Progress,
                    MainForm.cfgCache,
                    strCfgFilePath,
                    strStyle,
                    null,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    goto ERROR1;
                }
            }
            finally
            {
                _base.ReturnChannel(channel);

                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }
#if NO
        public void SetWidth(int nWidth)
        {
            this.tableLayoutPanel1.Width = nWidth;
            foreach (RegisterLine line in this.Items)
            {
                line.SetWidth(nWidth);
            }
            this.tableLayoutPanel1.PerformLayout();
        }
#endif
        Rectangle GetRect(RegisterLine item)
        {
            int[] row_heights = this.tableLayoutPanel1.GetRowHeights();
            int nYOffs = 0;
            int i = 0;
            foreach (RegisterLine cur_item in this.Items)
            {
                if (cur_item == item)
                    break;
                nYOffs += row_heights[i++];
            }

            int nHeight = 0;
            if (i < row_heights.Length)
                nHeight = row_heights[i];

            Rectangle rect = new Rectangle(0, nYOffs, this.tableLayoutPanel1.Width, nHeight); 

            return rect;
        }

        void SetControlOffs(ScrollableControl control, int x, int y)
        {
            this.BeginInvoke(new Action<ScrollableControl, int, int>(_SetControlOffs), control, x, y);
        }

        void _SetControlOffs(ScrollableControl control, int x, int y)
        {
            control.AutoScrollPosition = new Point(x, y);
        }

        public void EnsureVisible(RegisterLine base_line, Rectangle rect)
        {
            // http://msdn.microsoft.com/en-us/library/system.windows.forms.scrollablecontrol.autoscrollposition(v=vs.110).aspx
            // The X and Y coordinate values retrieved are negative if the control has scrolled away from its starting position (0,0). When you set this property, you must always assign positive X and Y values to set the scroll position relative to the starting position.
            ScrollableControl parent = this.tableLayoutPanel1;

            if (base_line != null)
                rect.Y += GetRect(base_line).Y;

            // this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, 1000);
            if (rect.Y < -parent.AutoScrollPosition.Y)
            {
                // parent.AutoScrollPosition = new Point(-parent.AutoScrollPosition.X, rect.Y);
                SetControlOffs(parent, -parent.AutoScrollPosition.X, rect.Y);
            }
            else if (rect.Y + rect.Height > -(parent.AutoScrollPosition.Y - this.ClientSize.Height))
            {
                // 刚好进入下部
                // parent.AutoScrollPosition = new Point(-parent.AutoScrollPosition.X, (rect.Y + rect.Height - this.ClientSize.Height));
                SetControlOffs(parent, -parent.AutoScrollPosition.X, rect.Y + rect.Height - this.ClientSize.Height);
            }
        }

        // parameters:
        //      strPart 事项的那个部分需要可见? all / biblio / items
        public void EnsureVisible(RegisterLine item,
            string strPart = "all")
        {
            if (strPart == "all")
            {
                Rectangle rect = this.GetRect(item);
                EnsureVisible(null, rect);
                return;
            }
            if (strPart == "biblio")
            {
                EnsureVisible(item, item._biblioRegister.GetRect("biblio"));
                return;
            }
            if (strPart == "items")
            {
                EnsureVisible(item, item._biblioRegister.GetRect("items"));
                return;
            }
        }
#if NO
        public void EnsureVisible(RegisterLine item)
        {
            int[] row_heights = this.tableLayoutPanel1.GetRowHeights();
            int nYOffs = 0; // row_heights[0];
            int i = 0;  // 1
            foreach (RegisterLine cur_item in this.Items)
            {
                if (cur_item == item)
                    break;
                nYOffs += row_heights[i++];
            }

            // this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, 1000);
            if (nYOffs < -this.AutoScrollPosition.Y)
            {
                this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, nYOffs);
            }
            else if (nYOffs + row_heights[i] > -(this.AutoScrollPosition.Y - this.ClientSize.Height))
            {
                // 刚好进入下部
                this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, nYOffs - this.ClientSize.Height + row_heights[i]);
            }
        }
#endif

        // 获得那些尚未初始化的行
        void GetNewlyLines(out List<RegisterLine> lines)
        {
            lines = new List<RegisterLine>();

            foreach (RegisterLine line in this.Items)
            {
                if (string.IsNullOrEmpty(line.BiblioSearchState) == true)
                    lines.Add(line);
            }
        }

        #region 创建书目记录的浏览格式

        // 创建MARC格式记录的浏览格式
        // paramters:
        //      strMARC MARC机内格式
        public int BuildMarcBrowseText(
            string strMarcSyntax,
            string strMARC,
            out string strBrowseText,
            out string strError)
        {
            strBrowseText = "";
            strError = "";

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this.MainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = Path.Combine(this.MainForm.DataDir, strMarcSyntax.Replace(".", "_") + "_cfgs\\marc_browse.fltx");

            int nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBrowseText = host.ResultString;

            }
            finally
            {
                // 归还对象
                this.MainForm.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        public int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)this.MainForm.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new BrowseFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " BrowseFilterDocument doc = (BrowseFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            // filter.Load(strFilterFileName);

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = "EntityRegisterControl filter.Load() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strBinDir = Environment.CurrentDirectory;

            string[] saAddRef1 = {
										 strBinDir + "\\digitalplatform.marcdom.dll",
										 // this.BinDir + "\\digitalplatform.marckernel.dll",
										 // this.BinDir + "\\digitalplatform.libraryserver.dll",
										 strBinDir + "\\digitalplatform.dll",
										 strBinDir + "\\digitalplatform.Text.dll",
										 strBinDir + "\\digitalplatform.IO.dll",
										 strBinDir + "\\digitalplatform.Xml.dll",
										 strBinDir + "\\dp2circulation.exe" };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
                out assembly,
                out strError,
                out strWarning);

            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

        #region 负责检索书目记录的线程

        public void BeginThread()
        {
            if (this._fillThread == null)
            {
                this._fillThread = new FillThread();
                this._fillThread.Container = this;
                this._fillThread.BeginThread();
            }
        }

        public void ActivateThread()
        {
            if (this._fillThread != null)
                this._fillThread.Activate();

        }

        public void StopThread()
        {
            this._fillThread.StopThread(true);
            this._fillThread = null;
        }

#if NO
        // 获得具有实体库的全部书目库名列表
        // parameters:
        //      strServerName   服务器名。可以为 .
        string GetBiblioDbNames(string strServerName)
        {
            if (IsDot(strServerName) == true)
            {
                List<string> results = new List<string>();
                if (this.MainForm.BiblioDbProperties != null)
                {
                    foreach (BiblioDbProperty prop in this.MainForm.BiblioDbProperties)
                    {
                        if (string.IsNullOrEmpty(prop.DbName) == false &&
                            string.IsNullOrEmpty(prop.ItemDbName) == false)
                        {
                            results.Add(prop.DbName);
                        }
                    }
                }

                return StringUtil.MakePathList(results);
            }

            AccountInfo info = GetAccountInfo(strServerName);
            if (info == null)
                return "<全部>";


        }
#endif

        #region 保存书目记录

        // 判断一个 server 是否适合写入
        bool IsWritable(XmlElement server,
            string strEditBiblioRecPath)
        {
            string strBiblioDbName = Global.GetDbName(strEditBiblioRecPath);
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                return false;

            XmlNodeList databases = server.SelectNodes("database[@name='"+strBiblioDbName+"']");
            foreach (XmlElement database in databases)
            {
                bool bIsTarget = DomUtil.GetBooleanParam(database, "isTarget", false);
                if (bIsTarget == true)
                {
                    string strAccess = database.GetAttribute("access");
                    bool bAppend = Global.IsAppendRecPath(strEditBiblioRecPath);
                    if (bAppend == true && StringUtil.IsInList("append", strAccess) == true)
                        return true;
                    if (bAppend == false && StringUtil.IsInList("overwrite", strAccess) == true)
                        return true;
                }
            }

            return false;
        }

        // 根据书目记录的路径，匹配适当的目标
        // parameters:
        //      bAllowCopyTo    是否允许书目记录复制到其他库？这发生在原始库不让 overwrite 的时候
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetTargetInfo(RegisterLine line,
            bool bAllowCopyTo,
            out string strServerName,
            out string strBiblioRecPath)
        {
            strServerName = "";
            strBiblioRecPath = "";

            // string strEditServerUrl = "";
            string strEditServerName = line._biblioRegister.ServerName;
            string strEditBiblioRecPath = line._biblioRegister.BiblioRecPath;

            if (string.IsNullOrEmpty(strEditServerName) == false
                && string.IsNullOrEmpty(strEditBiblioRecPath) == false)
            {
                // 验证 edit 中的书目库名，是否是可以写入的 ?
                XmlElement server = (XmlElement)this.ServersDom.DocumentElement.SelectSingleNode("server[@name='" + strEditServerName + "']");
                if (server != null)
                {
                    if (IsWritable(server, strEditBiblioRecPath) == true)
                    {
                        strServerName = strEditServerName;
                        strBiblioRecPath = strEditBiblioRecPath;
                        return 1;
                    }

                    if (bAllowCopyTo == false)
                        return 0;
                }
            }

            // 此后都是寻找可以追加写入的
            // 获得第一个可以写入的服务器名
            XmlNodeList servers = this.ServersDom.DocumentElement.SelectNodes("server");
            foreach (XmlElement server in servers)
            {
                XmlNodeList databases = server.SelectNodes("database");
                foreach (XmlElement database in databases)
                {
                    string strDatabaseName = database.GetAttribute("name");
                    if (string.IsNullOrEmpty(strDatabaseName) == true)
                        continue;
                    bool bIsTarget = DomUtil.GetBooleanParam(database, "isTarget", false);
                    if (bIsTarget == false)
                        continue;

                    string strAccess = database.GetAttribute("access");
                    if (StringUtil.IsInList("append", strAccess) == false)
                        continue;
                    strServerName = server.GetAttribute("name");
                    strBiblioRecPath = strDatabaseName + "/?";
                    return 1;
                }
            }

            return 0;
        }

        void DeleteItem(RegisterLine line, EntityEditControl edit)
        {
            string strError = "";
            int nRet = 0;

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            this.Progress.BeginLoop();
            try
            {
#if NO
                this._currentAccount = this.GetAccountInfo(line._biblioRegister.ServerName);
                if (this._currentAccount == null)
                {
                    strError = "服务器名 '" + line._biblioRegister.ServerName + "' 没有配置";
                    goto ERROR1;
                }
#endif

                List<EntityEditControl> controls = new List<EntityEditControl>();
                controls.Add(edit);

                // line.SetDisplayMode("summary");

                // 删除下属的册记录
                {
                    EntityInfo[] entities = null;
                    // 构造用于保存的实体信息数组
                    nRet = line._biblioRegister.BuildSaveEntities(
                        "delete",
                        controls,
                        out entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 分批进行保存
                    // return:
                    //      -2  部分成功，部分失败
                    //      -1  出错
                    //      0   保存成功，没有错误和警告
                    nRet = SaveEntities(
                        line,
                        entities,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;

                    // 兑现视觉删除
                    line._biblioRegister.RemoveEditControl(edit);

                    // line._biblioRegister.EntitiesChanged = false;
                }

                return;
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");
            }
        ERROR1:
            // line.SetDisplayMode("summary");
            // line.SetBiblioSearchState("error");
            // line.BiblioSummary = strError;
            BiblioRegisterControl.SetEditErrorInfo(edit, strError);
            line._biblioRegister.BarColor = "R";   // 红色
            this.SetColorList();
        }


        // 保存书目记录和下属的册记录
        void SaveLine(RegisterLine line)
        {
            string strError = "";
            int nRet = 0;

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            // this.Progress.Initial("进行一轮任务处理...");
            this.Progress.BeginLoop();
            try
            {
                string strCancelSaveBiblio = "";

                AccountInfo _currentAccount = _base.GetAccountInfo(line._biblioRegister.ServerName);
            if (_currentAccount == null)
            {
                strError = "服务器名 '" + line._biblioRegister.ServerName + "' 没有配置";
                goto ERROR1;
            }

                // line.SetBiblioSearchState("searching");
                if (line._biblioRegister.BiblioChanged == true
                    || Global.IsAppendRecPath(line._biblioRegister.BiblioRecPath) == true
                    || _currentAccount.IsLocalServer == false)
                {
                    // TODO: 确定目标 dp2library 服务器 目标书目库
                    string strServerName = "";
                    string strBiblioRecPath = "";
                    // 根据书目记录的路径，匹配适当的目标
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetTargetInfo(line,
                        _currentAccount.IsLocalServer == false? true : false,
                        out strServerName,
                        out strBiblioRecPath);
#if NO
                    if (nRet != 1)
                    {
                        strError = "line (servername='" + line._biblioRegister.ServerName + "' bibliorecpath='" + line._biblioRegister.BiblioRecPath + "') 没有找到匹配的保存目标";
                        goto ERROR1;
                    }
#endif
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "来自服务器 '" + line._biblioRegister.ServerName + "' 的书目记录 '" + line._biblioRegister.BiblioRecPath + "' 没有找到匹配的保存目标";
                        bool bAppend = Global.IsAppendRecPath(line._biblioRegister.BiblioRecPath);
                        if (bAppend == true || _currentAccount.IsLocalServer == false)
                            goto ERROR1;

                        // 虽然书目记录无法保存，但继续寻求保存册记录
                        strCancelSaveBiblio = strError;
                        goto SAVE_ENTITIES;
                    }

                    // if nRet == 0 并且 书目记录路径不是追加型的
                    // 虽然无法兑现修改后保存,但是否可以依然保存实体记录?

                    string strXml = "";
                    nRet = GetBiblioXml(
                        line,
                        out strXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strWarning = "";
                    string strOutputPath = "";
                    byte[] baNewTimestamp = null;
                    nRet = SaveXmlBiblioRecordToDatabase(
                        strServerName,
                        strBiblioRecPath,
                        strXml,
                        line._biblioRegister.Timestamp,
                        out strOutputPath,
                out baNewTimestamp,
                out strWarning,
                out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    line._biblioRegister.ServerName = strServerName;
                    line._biblioRegister.BiblioRecPath = strOutputPath;
                    line._biblioRegister.Timestamp = baNewTimestamp;
                    line._biblioRegister.BiblioChanged = false;
                }

                // line.SetDisplayMode("summary");

                SAVE_ENTITIES:
                // 保存下属的册记录
                {
                    EntityInfo[] entities = null;
                    // 构造用于保存的实体信息数组
                    nRet = line._biblioRegister.BuildSaveEntities(
                        "change",
                        null,
                        out entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (entities.Length > 0)
                    {
                        // 分批进行保存
                        // return:
                        //      -2  部分成功，部分失败
                        //      -1  出错
                        //      0   保存成功，没有错误和警告
                        nRet = SaveEntities(
                            line,
                            entities,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                            goto ERROR1;
                        line._biblioRegister.EntitiesChanged = false;
                    }
                    else
                    {
                        line._biblioRegister.EntitiesChanged = false;
                        line._biblioRegister.BarColor = "G";   // 绿色
                        return;
                    }
                }

                line.SetDisplayMode("summary");

                if (string.IsNullOrEmpty(strCancelSaveBiblio) == false)
                {
                    line.BiblioSummary = "书目记录无法保存，但册记录保存成功\r\n(" + strCancelSaveBiblio + ")";

                    line._biblioRegister.BarColor = "R";   // 表示操作未完全完成
                }
                else
                {
                    line.BiblioSummary = "保存成功";

                    line._biblioRegister.BarColor = "G";   // 绿色
                }
                this.SetColorList();
                return;
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");
            }
        ERROR1:
            line.SetDisplayMode("summary");
            line.SetBiblioSearchState("error");
            line.BiblioSummary = strError;

            line._biblioRegister.BarColor = "R";   // 红色
            this.SetColorList();
        }

        static EntityInfo[] GetPart(EntityInfo[] source,
int nStart,
int nCount)
        {
            EntityInfo[] result = new EntityInfo[nCount];
            for (int i = 0; i < nCount; i++)
            {
                result[i] = source[i + nStart];
            }
            return result;
        }

        // 分批进行保存
        // return:
        //      -2  部分成功，部分失败
        //      -1  出错
        //      0   保存成功，没有错误和警告
        int SaveEntities(
            RegisterLine line,
            EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            bool bWarning = false;
            EntityInfo[] errorinfos = null;
            string strWarning = "";

            // 确定目标服务器 目标书目库
            AccountInfo _currentAccount = _base.GetAccountInfo(line._biblioRegister.ServerName);
            if (_currentAccount == null)
            {
                strError = "' 服务器名 '" + line._biblioRegister.ServerName + "' 没有配置";
                return -1;
            }

            _channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);
            try
            {

                string strBiblioRecPath = line._biblioRegister.BiblioRecPath;

                int nBatch = 100;
                for (int i = 0; i < (entities.Length / nBatch) + ((entities.Length % nBatch) != 0 ? 1 : 0); i++)
                {
                    int nCurrentCount = Math.Min(nBatch, entities.Length - i * nBatch);
                    EntityInfo[] current = GetPart(entities, i * nBatch, nCurrentCount);

                    long lRet = _channel.SetEntities(
         Progress,
         strBiblioRecPath,
         entities,
         out errorinfos,
         out strError);
                    if (lRet == -1)
                        return -1;

                    // 把出错的事项和需要更新状态的事项兑现到显示、内存
                    string strError1 = "";
                    if (line._biblioRegister.RefreshOperResult(errorinfos, out strError1) == true)
                    {
                        bWarning = true;
                        strWarning += " " + strError1;
                    }

                    if (lRet == -1)
                        return -1;
                }

                if (string.IsNullOrEmpty(strWarning) == false)
                    strError += " " + strWarning;

                if (bWarning == true)
                    return -2;

                // line._biblioRegister.EntitiesChanged = false;    // 所有册都保存成功了
                return 0;
            }
            finally
            {
                _base.ReturnChannel(_channel);
                _channel = null;
                _currentAccount = null;
            }
        }

        // 获得书目记录的XML格式
        // parameters:
        int GetBiblioXml(
            RegisterLine line,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strBiblioDbName = Global.GetDbName(line._biblioRegister.BiblioRecPath);

            string strMarcSyntax = "";

            // TODO: 如何获得远程其他 dp2library 服务器的书目库的 syntax?
            // 获得库名，根据库名得到marc syntax
            if (String.IsNullOrEmpty(strBiblioDbName) == false)
                strMarcSyntax = MainForm.GetBiblioSyntax(strBiblioDbName);

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            // 2008/5/16 changed
            string strMARC = line._biblioRegister.GetMarc();
            XmlDocument domMarc = null;
            int nRet = MarcUtil.Marc2Xml(strMARC,
                strMarcSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 因为domMarc是根据MARC记录合成的，所以里面没有残留的<dprms:file>元素，也就没有(创建新的id前)清除的需要

            Debug.Assert(domMarc != null, "");

#if NO
            // 合成其它XML片断
            if (domXmlFragment != null
                && string.IsNullOrEmpty(domXmlFragment.DocumentElement.InnerXml) == false)
            {
                XmlDocumentFragment fragment = domMarc.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = domXmlFragment.DocumentElement.InnerXml;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                domMarc.DocumentElement.AppendChild(fragment);
            }
#endif

            strXml = domMarc.OuterXml;
            return 0;
        }

        // 保存XML格式的书目记录到数据库
        // parameters:
        int SaveXmlBiblioRecordToDatabase(
            string strServerName,
            string strPath,
            string strXml,
            byte[] baTimestamp,
            out string strOutputPath,
            out byte[] baNewTimestamp,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            baNewTimestamp = null;
            strOutputPath = "";

            AccountInfo _currentAccount = _base.GetAccountInfo(strServerName);
            if (_currentAccount == null)
            {
                strError = "服务器名 '" + strServerName + "' 没有配置";
                return -1;
            }
            _channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);

            try
            {
                string strAction = "change";

                if (Global.IsAppendRecPath(strPath) == true)
                    strAction = "new";

            REDO:
                long lRet = _channel.SetBiblioInfo(
                    Progress,
                    strAction,
                    strPath,
                    "xml",
                    strXml,
                    baTimestamp,
                    "",
                    out strOutputPath,
                    out baNewTimestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存书目记录 '" + strPath + "' 时出错: " + strError;
                    goto ERROR1;
                }
                if (_channel.ErrorCode == ErrorCode.PartialDenied)
                {
                    strWarning = "书目记录 '" + strPath + "' 保存成功，但所提交的字段部分被拒绝 (" + strError + ")。请留意刷新窗口，检查实际保存的效果";
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                _base.ReturnChannel(_channel);
                this._channel = null;
                // _currentAccount = null;
            }
        }

        #endregion // 保存书目记录

        #region 检索书目

        void SearchLine(RegisterLine line)
        {
            string strError = "";
            int nRet = 0;

            if (this.ServersDom == null)
            {
                strError = "ServersDom 为空";
                goto ERROR1;
            }

            string strTotalError = "";

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            // this.Progress.Initial("进行一轮任务处理...");
            this.Progress.BeginLoop();
            try
            {
                int nHitCount = 0;

                line.SetBiblioSearchState("searching");
                line.BiblioSummary = "正在检索 " + line.BiblioBarcode + " ...";

                XmlNodeList servers = this.ServersDom.DocumentElement.SelectNodes("server");
                foreach (XmlElement server in servers)
                {
                    AccountInfo account = EntityRegisterBase.GetAccountInfo(server);
                    Debug.Assert(account != null, "");
                    _base.CurrentAccount = account;
#if NO
                string strName = server.GetAttribute("name");
                string strType = server.GetAttribute("type");
                string strUrl = server.GetAttribute("url");
                string strUserName = server.GetAttribute("userName");
                string strPassword = server.GetAttribute("password");
                // e.Password = this.MainForm.DecryptPasssword(e.Password);
                string strIsReader = server.GetAttribute("isReader");
#endif

                    if (account.ServerType == "dp2library")
                    {
                        nRet = SearchLineDp2library(line,
                            account,
                            out strError);
                        if (nRet == -1)
                            strTotalError += strError + "\r\n";
                        else
                            nHitCount += nRet;
                    }
                    else if (account.ServerType == "amazon")
                    {
                        nRet = SearchLineAmazon(line,
                            account,
                            out strError);
                        if (nRet == -1)
                            strTotalError += strError + "\r\n";
                        else
                            nHitCount += nRet;
                    }
                }

                line.SetBiblioSearchState(nHitCount.ToString());

                // 
                if (nHitCount == 1)
                {
                    // TODO: 如果有报错的行，是否就不要自动模拟双击了？ 假如这种情况是因为红泥巴服务器持续无法访问引起的，需要有配置办法可以临时禁用这个数据源

                    // 模拟双击
                    int index = line._biblioRegister.GetFirstRecordIndex();
                    if (index == -1)
                    {
                        strError = "获得第一个浏览记录 index 时出错";
                        goto ERROR1;
                    }
                    nRet = line._biblioRegister.SelectBiblio(index,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    if (nHitCount > 1)
                        line.SetDisplayMode("select");
                    else
                    {
                        // 进入详细状态，可以新创建一条记录
                        line.AddBiblioBrowseLine(BiblioRegisterControl.TYPE_INFO,
                            "没有命中书目记录。双击本行新创建书目记录",
                            "",
                            BuildBlankBiblioInfo(line.BiblioBarcode)); 
                        line.SetDisplayMode("select");
                    }
                }
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");
            }

            if (string.IsNullOrEmpty(strTotalError) == false)
            {
                // DisplayFloatErrorText(strTotalError);

                line._biblioRegister.BarColor = "R";   // 红色，需引起注意
                this.SetColorList();
            }
            else
            {
                line._biblioRegister.BarColor = "Y";   // 黄色表示等待选择?
                this.SetColorList();
            }
            return;
        ERROR1:
            line.SetDisplayMode("summary");
            line.SetBiblioSearchState("error");
            line.BiblioSummary = strError;
            DisplayFloatErrorText(strError);

            line._biblioRegister.BarColor = "R";   // 红色，需引起注意
            this.SetColorList();
        }

        // 寻找一个可以创建新书目记录的数据库信息
        // return:
        //      false 没有找到
        //      ture 找到
        bool GetTargetDatabaseInfo(out string strServerName,
            out string strBiblioDbName)
        {
            strServerName = "";
            strBiblioDbName = "";

            List<XmlElement> database_nodes = new List<XmlElement>();
            XmlNodeList databases = this.ServersDom.DocumentElement.SelectNodes("server/database");
            foreach (XmlElement database in databases)
            {
                string strValue = database.GetAttribute("isTarget");
                if (string.IsNullOrEmpty(strValue) == true)
                    continue;
                if (DomUtil.IsBooleanTrue(strValue) == true)
                    database_nodes.Add(database);
            }

            // 选择第一个具有 append 权限的
            foreach (XmlElement database in database_nodes)
            {
                string strAccess = database.GetAttribute("access");
                if (StringUtil.IsInList("append", strAccess) == true)
                {
                    strServerName = (database.ParentNode as XmlElement).GetAttribute("name");
                    strBiblioDbName = database.GetAttribute("name");
                    return true;
                }
            }

            return false;
        }

        RegisterBiblioInfo BuildBlankBiblioInfo(string strISBN)
        {
            // 获得一个可以保存新书目记录的服务器地址和书目库名
            string strServerName = "";
            string strBiblioDbName = "";
            // 寻找一个可以创建新书目记录的数据库信息
            // return:
            //      false 没有找到
            //      ture 找到
            GetTargetDatabaseInfo(out strServerName,
                out strBiblioDbName);


            // 装入一条空白书目记录
            RegisterBiblioInfo info = new RegisterBiblioInfo();

#if NO
            if (string.IsNullOrEmpty(strBiblioDbName) == false)
                info.RecPath = strBiblioDbName + "/?@" + strServerName; 
#endif

            info.MarcSyntax = "unimarc";
            MarcRecord record = new MarcRecord();
            record.add(new MarcField('$', "010  $a" + strISBN + "$dCNY??"));
            record.add(new MarcField('$', "2001 $a$f"));
            record.add(new MarcField('$', "210  $a$c$d"));
            record.add(new MarcField('$', "215  $a$d??cm"));
            record.add(new MarcField('$', "690  $a"));
            record.add(new MarcField('$', "701  $a"));
            info.OldXml = record.Text;

            return info;
        }

        // 显示色条
        internal void SetColorList()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(SetColorList));
                return;
            }

            //int nWaitingCount = 0;
            StringBuilder text = new StringBuilder(256);
            foreach (RegisterLine line in this.Items)
            {
                char color = 'W';   // 缺省为白色
                if (string.IsNullOrEmpty(line._biblioRegister.BarColor) == false)
                    color = Char.ToUpper(line._biblioRegister.BarColor[0]);
                text.Append(color);
            }

            this.ColorControl.ColorList = text.ToString();

#if NO
            // TODO: 是否延迟显示，避免反复出现和隐藏
            if (nWaitingCount > 0)
            {
                string strState = "";
                if (this._taskList.Stopped == true)
                    strState = "已暂停任务处理。\r\n";
                this._floatingMessage.Text = strState + "有 " + nWaitingCount.ToString() + " 个任务尚未完成 ...";
            }
            else
            {
                if (this._taskList.Stopped == true)
                    this._floatingMessage.Text = "已暂停任务处理。";
                else
                    this._floatingMessage.Text = "";
            }
#endif
        }


        // return:
        //      -1  出错
        //      >=0 命中的记录数
        int SearchLineAmazon(RegisterLine line,
            AccountInfo account,
            out string strError)
        {
            strError = "";

            // ??? this._currentAccount = account;

            line.BiblioSummary = "正在针对 " + account.ServerName + " \r\n检索 " + line.BiblioBarcode + " ...";

            AmazonSearch search = new AmazonSearch();
            // search.MainForm = this.MainForm;
            search.TempFileDir = this.MainForm.UserTempDir;

            // 多行检索中的一行检索
            int nRedoCount = 0;
        REDO:
            int nRet = search.Search(
                account.ServerUrl,
                line.BiblioBarcode.Replace("-", ""),
                "ISBN",
                "[default]",
                true,
                out strError);
            if (nRet == -1)
            {
                if (search.Exception != null && search.Exception is WebException)
                {
                    WebException e = search.Exception as WebException;
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        // 重做
                        if (nRedoCount < 2)
                        {
                            nRedoCount++;
                            Thread.Sleep(1000);
                            goto REDO;
                        }

#if NO
                        // 询问是否重做
                        DialogResult result = MessageBox.Show(this,
"检索 '" + strLine + "' 时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 跳过这一行继续检索后面的行； Cancel: 中断整个检索操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Retry)
                        {
                            Thread.Sleep(1000);
                            goto REDO;
                        }
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        goto CONTINUE;
#endif
                        goto ERROR1;
                    }
                }
                goto ERROR1;
            }


            nRet = search.LoadBrowseLines(appendBrowseLine,
                line,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return nRet;

            ERROR1:
            strError = "针对服务器 '" + account.ServerName + "' 检索出错: " + strError;
            line.AddBiblioBrowseLine(strError, BiblioRegisterControl.TYPE_ERROR);
            return -1;
        }

        // 针对亚马逊服务器检索，装入一个浏览行的回调函数
        int appendBrowseLine(string strRecPath,
    string strRecord,
    object param,
            bool bAutoSetFocus,
    out string strError)
        {
            strError = "";

            RegisterLine line = param as RegisterLine;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strRecord);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", AmazonSearch.NAMESPACE);

            List<string> cols = null;
            string strASIN = "";
            string strCoverImageUrl = "";
            int nRet = AmazonSearch.ParseItemXml(dom.DocumentElement,
                nsmgr,
                out strASIN,
                out strCoverImageUrl,
                out cols,
                out strError);
            if (nRet == -1)
                return -1;

            string strMARC = "";
            // 将亚马逊 XML 格式转换为 UNIMARC 格式
            nRet = AmazonSearch.AmazonXmlToUNIMARC(dom.DocumentElement,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            RegisterBiblioInfo info = new RegisterBiblioInfo();
            info.OldXml = strMARC;
            info.Timestamp = null;
            info.RecPath = strASIN + "@" + _base.CurrentAccount.ServerName;
            info.MarcSyntax = "unimarc";
            line.AddBiblioBrowseLine(
                -1,
                info.RecPath,
                StringUtil.MakePathList(cols, "\t"),
                info);

            return 0;
        }


        // 针对 dp2library 服务器进行检索
        // parameters:
        //  
        // return:
        //      -1  出错
        //      >=0 命中的记录数
        int SearchLineDp2library(RegisterLine line,
            AccountInfo account,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // ??? _currentAccount = account;

            _channel = _base.GetChannel(account.ServerUrl, account.UserName);
            _channel.Timeout = new TimeSpan(0, 0, 5);   // 超时值为 5 秒
            try
            {
                string strQueryWord = line.BiblioBarcode;

                string strFromStyle = "";

                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle("ISBN");
                }
                catch (Exception ex)
                {
                    strError = "EntityRegisterControl GetBiblioFromStyle() exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + "ISBN" + "' 对应的style字符串";
                    goto ERROR1;
                }

                string strMatchStyle = "left";  // BiblioSearchForm.GetCurrentMatchStyle(this.comboBox_matchStyle.Text);
                if (string.IsNullOrEmpty(strQueryWord) == true)
                {
                    if (strMatchStyle == "null")
                    {
                        strQueryWord = "";

                        // 专门检索空值
                        strMatchStyle = "exact";
                    }
                    else
                    {
                        // 为了在检索词为空的时候，检索出全部的记录
                        strMatchStyle = "left";
                    }
                }
                else
                {
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        goto ERROR1;
                    }
                }

                ServerInfo server_info = null;

                if (line != null)
                    line.BiblioSummary = "正在获取服务器 " + account.ServerName + " 的配置信息 ...";

                // 准备服务器信息
                nRet = _base.GetServerInfo(
                    // line,
                    _channel,
                    account,
                    out server_info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;    // 可以不报错 ?

                line.BiblioSummary = "正在针对 "+account.ServerName+ " \r\n检索 " + line.BiblioBarcode + " ...";

                string strQueryXml = "";
                long lRet = _channel.SearchBiblio(Progress,
                    server_info == null ? "<全部>" : server_info.GetBiblioDbNames(),    // "<全部>",
                    strQueryWord,   // this.textBox_queryWord.Text,
                    1000,
                    strFromStyle,
                    strMatchStyle,
                    _base.Lang,
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 '"+account.ServerName+"' 检索出错: " + strError;
                    goto ERROR1;
                }

                // 装入浏览格式
                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                string strStyle = "id";

                List<string> biblio_recpaths = new List<string>();
                // 装入浏览格式
                for (; ; )
                {
                    if (this.Progress != null && this.Progress.State != 0)
                    {
                        break;
                    }
                    // DoTasks();

                    lRet = _channel.GetSearchResult(
                        this.Progress,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                        _base.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                        break;

                    // 处理浏览结果

                    foreach (DigitalPlatform.CirculationClient.localhost.Record searchresult in searchresults)
                    {
                        biblio_recpaths.Add(searchresult.Path);
                    }

                    {
                        // 获得书目摘要
                        BiblioLoader loader = new BiblioLoader();
                        loader.Channel = _channel;
                        loader.Stop = this.Progress;
                        loader.Format = "xml";
                        loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp;
                        loader.RecPaths = biblio_recpaths;

                        try
                        {
                            int i = 0;
                            foreach (BiblioItem item in loader)
                            {
                                string strXml = item.Content;

                                string strMARC = "";
                                string strMarcSyntax = "";
                                // 将XML格式转换为MARC格式
                                // 自动从数据记录中获得MARC语法
                                nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                                    true,
                                    null,
                                    out strMarcSyntax,
                                    out strMARC,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "XML转换到MARC记录时出错: " + strError;
                                    goto ERROR1;
                                }

                                string strBrowseText = "";
                                nRet = BuildMarcBrowseText(
                                    strMarcSyntax,
                                    strMARC,
                                    out strBrowseText,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "MARC记录转换到浏览格式时出错: " + strError;
                                    goto ERROR1;
                                }

                                RegisterBiblioInfo info = new RegisterBiblioInfo();
                                info.OldXml = strMARC;
                                info.Timestamp = item.Timestamp;
                                info.RecPath = item.RecPath + "@" + account.ServerName;
                                info.MarcSyntax = strMarcSyntax;
                                line.AddBiblioBrowseLine(
                                    -1,
                                    info.RecPath,
                                    strBrowseText,
                                    info);
                                i++;
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "EntityRegisterControl {F8A02071-E276-4C77-B915-42564A4EB418} exception: " + ExceptionUtil.GetAutoText(ex);
                            goto ERROR1;
                        }


                        // lIndex += biblio_recpaths.Count;
                        biblio_recpaths.Clear();
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }


                return (int)lHitCount;

            }
            finally
            {
                _base.ReturnChannel(_channel);
                _channel = null;
            }

        ERROR1:
#if NO
            line.SetDisplayMode("summary");
            line.SetBiblioSearchState("error");
            line.BiblioSummary = strError;
#endif
            line.AddBiblioBrowseLine(strError, BiblioRegisterControl.TYPE_ERROR);

            return -1;
        }


        #endregion // 检索书目

        LibraryChannel _channel = null;

        public XmlDocument ServersDom
        {
            get
            {
                return this._base.ServersDom;
            }
            set
            {
                this._base.ServersDom = value;
            }
        }

        public Stop Progress
        {
            get
            {
                return _base.Progress;
            }
            set
            {
                _base.Progress = value;
            }
        }

        public override bool Changed
        {
            get
            {
                foreach (RegisterLine line in this.Items)
                {
                    if (line._biblioRegister.BiblioChanged == true
                        || line._biblioRegister.EntitiesChanged == true)
                        return true;
                }
                return false;
            }
            set
            {
                base.Changed = value;
            }
        }

        public int AddSaveAllTask()
        {
            int nCount = 0;
            foreach (RegisterLine line in this.Items)
            {
                if (line._biblioRegister.Changed == true)
                {
                    this.AddTask(line, "save");
                    nCount++;
                }
            }

            return nCount;
        }

        public void AddDeleteItemTask(RegisterLine line, EntityEditControl edit)
        {
            this.AddTask(line, "delete_items", edit);
        }

        // load_items / save / search_biblio
        internal void AddTask(RegisterLine line,
            string strTaskName,
            object tag = null)
        {
            LineTask task = new LineTask();
            task.TaskName = strTaskName;
            task.Line = line;
            task.Tag = tag;
            lock (this._loadEntityTasks)
            {
                this._loadEntityTasks.Add(task);
            }

            this.ActivateThread();
        }

        internal void DoTasks()
        {
            int nCount = 0;

            lock (this._loadEntityTasks)
            {
                nCount = this._loadEntityTasks.Count;
            }
            if (nCount == 0)
                return;

            for (int i = 0; i < nCount; i++)
            {
                if (this._fillThread.Stopped == true)
                    return;

                LineTask task = _loadEntityTasks[i];

                string strError = "";

                task.Line._biblioRegister.BarColor = "P";   // 表示正在处理
                this.SetColorList();

                if (task.TaskName == "load_items")
                {
                    // 将一条书目记录下属的若干册记录装入列表
                    // return:
                    //      -2  用户中断
                    //      -1  出错
                    //      >=0 装入的册记录条数
                    int nRet = LoadBiblioSubItems(
                        task.Line,
                        out strError);
                    if (nRet < 0)
                    {
                        DisplayFloatErrorText("装载册记录时出错: " + strError);

                        task.Line._biblioRegister.BarColor = "R";   // 红色，需引起注意
                        this.SetColorList();
                    }
                    else
                    {
                        task.Line._biblioRegister.BarColor = "W";   // 白色，表示正常
                        this.SetColorList();
                    }
                }
                else if (task.TaskName == "search_biblio")
                {
                    this.SearchLine(task.Line);
                }
                else if (task.TaskName == "save")
                {
                    this.SaveLine(task.Line);
                }
                else if (task.TaskName == "delete_items")
                {
                    this.DeleteItem(task.Line, task.Tag as EntityEditControl);
                }
            }

            // 清除已经做过的事项
            lock (this._loadEntityTasks)
            {
                for (int i = 0; i < nCount; i++)
                {
                    _loadEntityTasks.RemoveAt(0);
                }
            }
        }

        void DisplayFloatErrorText(string strText, string strColor = "")
        {
            if (this.DisplayError != null)
            {
                DisplayErrorEventArgs e = new DisplayErrorEventArgs();
                e.Text = strText;
                e.Color = strColor;
                this.DisplayError(this, e);
            }
        }

        // 将一条书目记录下属的若干册记录装入列表
        // return:
        //      -2  用户中断
        //      -1  出错
        //      >=0 装入的册记录条数
        int LoadBiblioSubItems(
            RegisterLine line,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            line._biblioRegister.ClearEntityEditControls("normal");

            string strBiblioRecPath = line._biblioRegister.BiblioRecPath;
            if (string.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                // 册信息部分显示为空
                line._biblioRegister.TrySetBlank("none");
                return 0;
            }

            AccountInfo _currentAccount = _base.GetAccountInfo(line._biblioRegister.ServerName);
            if (_currentAccount == null)
            {
                strError = "服务器名 '" + line._biblioRegister.ServerName + "' 没有配置";
                return -1;
            }

            // 如果不是本地服务器，则不需要装入册记录
            if (_currentAccount.IsLocalServer == false)
            {
                _base.CurrentAccount = null;
                // 册信息部分显示为空
                line._biblioRegister.TrySetBlank("none");
                return 0;
            }

            _channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            //this.Progress.Initial("正在装入书目记录 '" + strBiblioRecPath + "' 下属的册记录 ...");
            this.Progress.BeginLoop();
            try
            {
                int nCount = 0;

                long lPerCount = 100; // 每批获得多少个
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                for (; ; )
                {
                    if (Progress.State != 0)
                    {
                        strError = "用户中断";
                        return -2;
                    }

                    EntityInfo[] entities = null;

                    long lRet = _channel.GetEntities(
             Progress,
             strBiblioRecPath,
             lStart,
             lCount,
             "",  // bDisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
             "zh",
             out entities,
             out strError);
                    if (lRet == -1)
                        return -1;

                    lResultCount = lRet;

                    if (lRet == 0)
                    {
                        // 册信息部分显示为空
                        line._biblioRegister.TrySetBlank("none");
                        return nCount;
                    }

                    Debug.Assert(entities != null, "");

                    foreach (EntityInfo entity in entities)
                    {
                        // string strXml = entity.OldRecord;
                        if (entity.ErrorCode != ErrorCodeValue.NoError)
                        {
                            // TODO: 显示错误信息
                            continue;
                        }

                        // 添加一个新的册对象
                        nRet = line._biblioRegister.NewEntity(entity.OldRecPath,
                            entity.OldTimestamp,
                            entity.OldRecord,
                            false,  // 不必滚入视野
                            out strError);
                        if (nRet == -1)
                            return -1;

                        nCount++;
                    }

                    lStart += entities.Length;
                    if (lStart >= lResultCount)
                        break;

                    if (lCount == -1)
                        lCount = lPerCount;

                    if (lStart + lCount > lResultCount)
                        lCount = lResultCount - lStart;
                }

                return nCount;
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");

                _base.ReturnChannel(_channel);
                _channel = null;
                _currentAccount = null;
            }
        }


        internal void DoStop(object sender, StopEventArgs e)
        {
            if (this._channel != null)
                this._channel.Abort();
#if NO
            if (this.Channel != null)
                this.Channel.Abort();
#endif
        }

        class FillThread : ThreadBase
        {
            internal ReaderWriterLock m_lock = new ReaderWriterLock();
            internal static int m_nLockTimeout = 5000;	// 5000=5秒

            public EntityRegisterControl Container = null;

            // 工作线程每一轮循环的实质性工作
            public override void Worker()
            {
                //string strError = "";
                //int nRet = 0;

                this.m_lock.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    if (this.Stopped == true)
                        return;

                    this.Container.DoTasks();

#if NO
                    // 找到那些需要补充内容的行
                    List<RegisterLine> lines = null;
                    this.Container.GetNewlyLines(out lines);

                    if (lines.Count > 0)
                    {
                        this.Container.SearchLines(lines);
                    }
#endif

                    // m_bStopThread = true;   // 只作一轮就停止
                }
                finally
                {
                    this.m_lock.ReleaseWriterLock();
                }

            ERROR1:
                // Safe_setError(this.Container.listView_in, strError);
                return;
            }

        }


        #endregion

        // 一个命令
        class LineTask
        {
            public string TaskName = "";    // load_items / save / search_biblio / delete_items
            public RegisterLine Line = null;
            public object Tag = null;
        }

        private void tableLayoutPanel1_Scroll(object sender, ScrollEventArgs e)
        {
#if NO
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.Focus();
#endif
        }



    }

    /// <summary>
    /// 视觉行基类
    /// </summary>
    public class RegisterLine : TableItemBase
    {
        // public EntitiesControl Container = null;

        string _biblioBarcode = "";
        // ISBN 条码号或者其他。用于检索书目记录
        public string BiblioBarcode
        {
            get
            {
                return this._biblioBarcode;
            }
            set
            {
                this._biblioBarcode = value;

                // this._biblioRegister.Summary = value;
                this._biblioRegister.BiblioBarcode = value;
            }
        }

        public string BiblioSummary
        {
            get
            {
                return this._biblioRegister.Summary;
            }
            set
            {
                this._biblioRegister.Summary = value;
            }
        }

       // 书目检索的状态。
        public string BiblioSearchState
        {
            get
            {
                return this._biblioRegister.GetSearchState();
            }
        }

        public BiblioRegisterControl _biblioRegister = null;

        public override void DisposeChildControls()
        {
            _biblioRegister.Dispose();
            base.DisposeChildControls();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="container"></param>
        public RegisterLine(EntityRegisterControl container)
        {
            this.Container = container;

            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            label_color.Size = new Size(6, 23);
            label_color.Margin = new Padding(0, 0, 0, 0);

            _biblioRegister = new BiblioRegisterControl();
            _biblioRegister.Dock = DockStyle.Fill;
            // _biblioRegister.Size = new Size(100, 100);
            _biblioRegister.Margin = new Padding(0, 0, 0, 0);
            _biblioRegister.AutoSize = true;
            _biblioRegister.AutoScroll = false;

            // 册信息部分显示为空
            _biblioRegister.TrySetBlank("not_initial");
            _biblioRegister.LoaderImage = (this.Container as EntityRegisterControl).LoaderImage;

            // _biblioRegister.SetWidth(this.Container.ClientSize.Width);
        }

        // 移除本行相关的控件
        public override void RemoveControls(TableLayoutPanel table)
        {
            table.Controls.Remove(this.label_color);
            table.Controls.Remove(this._biblioRegister);
        }

        public override void AddControls(TableLayoutPanel table, int nRow)
        {
            int nColumnIndex = 0;
            int nRowIndex = nRow + this.Container.RESERVE_LINES;
            table.Controls.Add(this.label_color, nColumnIndex++, nRowIndex);
            table.Controls.Add(this._biblioRegister, nColumnIndex++, nRowIndex);
        }

        public override void SetReaderOnly(bool bReadOnly)
        {
            // 
            // this._biblioRegister.ReadOnly = bReadOnly;
        }

        public override void AddEvents(bool bAdd)
        {
            if (bAdd)
            {
                this.label_color.MouseUp += new MouseEventHandler(label_color_MouseUp);

                this.label_color.MouseClick += new MouseEventHandler(label_color_MouseClick);

                this._biblioRegister.Enter += new EventHandler(control_Enter);

                this._biblioRegister.GetConfigDom += new GetConfigDomEventHandle(_biblioRegister_GetConfigDom);

                this._biblioRegister.LoadEntities += new EventHandler(_biblioRegister_LoadEntities);

                this._biblioRegister.EnsureVisible += new EnsureVisibleEventHandler(_biblioRegister_EnsureVisible);

                this._biblioRegister.AsyncGetImage += new AsyncGetImageEventHandler(_biblioRegister_AsyncGetImage);
#if NO
            this._biblioRegister.GetServerType -= new GetServerTypeEventHandler(_biblioRegister_GetServerType);
            this._biblioRegister.GetServerType += new GetServerTypeEventHandler(_biblioRegister_GetServerType);
#endif
                this._biblioRegister.GetValueTable += new GetValueTableEventHandler(_biblioRegister_GetValueTable);

                this._biblioRegister.DeleteItem += new DeleteItemEventHandler(_biblioRegister_DeleteItem);
            }
            else
            {
                this.label_color.MouseUp -= new MouseEventHandler(label_color_MouseUp);
                this.label_color.MouseClick -= new MouseEventHandler(label_color_MouseClick);
                this._biblioRegister.Enter -= new EventHandler(control_Enter);
                this._biblioRegister.GetConfigDom -= new GetConfigDomEventHandle(_biblioRegister_GetConfigDom);
                this._biblioRegister.LoadEntities -= new EventHandler(_biblioRegister_LoadEntities);
                this._biblioRegister.EnsureVisible -= new EnsureVisibleEventHandler(_biblioRegister_EnsureVisible);
                this._biblioRegister.AsyncGetImage -= new AsyncGetImageEventHandler(_biblioRegister_AsyncGetImage);
                this._biblioRegister.GetValueTable -= new GetValueTableEventHandler(_biblioRegister_GetValueTable);
                this._biblioRegister.DeleteItem -= new DeleteItemEventHandler(_biblioRegister_DeleteItem);
            }

            base.AddEvents(bAdd);
        }

        void _biblioRegister_DeleteItem(object sender, DeleteItemEventArgs e)
        {
            (this.Container as EntityRegisterControl).AddDeleteItemTask(this, e.Control);
        }

        void _biblioRegister_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            EntityRegisterControl container = this.Container as EntityRegisterControl;
            container.OnGetValueTable(sender, e);   // sender 为 BiblioResiterControl
        }

        void _biblioRegister_AsyncGetImage(object sender, AsyncGetImageEventArgs e)
        {
            EntityRegisterControl container = this.Container as EntityRegisterControl;
            BiblioRegisterControl biblioRegister = sender as BiblioRegisterControl;
            container.AsyncGetImageFile(biblioRegister, e);
        }

#if NO
        void _biblioRegister_GetServerType(object sender, GetServerTypeEventArgs e)
        {
            e.ServerType = (this.Container as EntityRegisterControl).GetServerType(e.ServerName);
        }
#endif

        void label_color_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedCount = this.Container.SelectedIndices.Count;



            //
            menuItem = new MenuItem("全部保存(&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveAll_Click);
            if (this.Container.Items.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

#if NO
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteElements_Click);
            contextMenu.MenuItems.Add(menuItem);
#endif

            contextMenu.Show(this.label_color, new Point(e.X, e.Y));
        }

        // 全部保存
        void menu_saveAll_Click(object sender, EventArgs e)
        {
            // (this.Container as EntityRegisterControl).AddTask(this, "save");
            (this.Container as EntityRegisterControl).AddSaveAllTask();
        }

        void _biblioRegister_EnsureVisible(object sender, EnsureVisibleEventArgs e)
        {
            //e.Rect.X -= this.label_color.Location.X;
            //e.Rect.Y -= this.label_color.Location.Y;

            (this.Container as EntityRegisterControl).EnsureVisible(this, e.Rect);
        }

        void control_Enter(object sender, EventArgs e)
        {
            this.Container.SelectItem(this, true);
        }

        void label_color_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    this.Container.ToggleSelectItem(this);
                }
                else if (Control.ModifierKeys == Keys.Shift)
                    this.Container.RangeSelectItem(this);
                else
                {
                    this.Container.SelectItem(this, true);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 如果当前有多重选择，则不必作什么l
                // 如果当前为单独一个选择或者0个选择，则选择当前对象
                // 这样做的目的是方便操作
                if (this.Container.SelectedIndices.Count < 2)
                {
                    this.Container.SelectItem(this, true);
                }
            }
        }


        void _biblioRegister_LoadEntities(object sender, EventArgs e)
        {
            (this.Container as EntityRegisterControl).AddTask(this, "load_items");
        }

        void _biblioRegister_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            (this.Container as EntityRegisterControl).MarcEditor_GetConfigDom(sender, e);
        }


        
        // 添加一个新的册对象
        public int NewEntity(string strXml,
            out string strError)
        {
            return this._biblioRegister.NewEntity("",
                null,
                strXml,
                true,
                out strError);
        }

        public int ReplaceEntityMacro(XmlDocument dom,
            out string strError)
        {
            return this._biblioRegister.ReplaceEntityMacro(dom, out strError);
        }

#if NO
        public void SetWidth(int nWidth)
        {
            this._biblioRegister.SetWidth(nWidth);
        }
#endif
        public void AdjustHeight()
        {
            this._biblioRegister.AdjustFlowLayoutHeight();
        }

        /// <summary>
        /// 设置 “选择书目” 按钮的状态
        /// </summary>
        /// <param name="strState">状态字符串。为 searching / error / 其他</param>
        public void SetBiblioSearchState(string strState)
        {
            this._biblioRegister.SetSearchState(strState);
        }

        public void AddBiblioBrowseLine(
            int nType,
            string strBiblioRecPath,
            string strBrowseText,
            RegisterBiblioInfo info)
        {
            this._biblioRegister.AddBiblioBrowseLine(
                nType,
                strBiblioRecPath,
                strBrowseText,
                info);
        }

        public void AddBiblioBrowseLine(string strText,
            int nType)
        {
            // this._biblioRegister.AddBiblioBrowseLine(strText, nType);
            this._biblioRegister.AddBiblioBrowseLine(
    nType,
    strText,
    "",
    null);
        }

        public void SetDisplayMode(string strMode)
        {
            EntityRegisterControl container = (this.Container as EntityRegisterControl);
            if (container.InvokeRequired)
            {
                container.Invoke(new Action<string>(SetDisplayMode), strMode);
                return;
            }

            Point point = container.TableLayoutPanel.AutoScrollPosition;
            this._biblioRegister.DisplayMode = strMode;

            container.TableLayoutPanel.AutoScrollPosition = new Point(-point.X, -point.Y);
        }

    }

    public class FilterHost
    {
        public MainForm MainForm = null;
        public string ID = "";
        public string ResultString = "";    // 结果字符串。用 \t 字符分隔
        public string ColumnTitles = "";    // 栏目标题。用 \t 字符分隔 2015/8/11
    }

    public class BrowseFilterDocument : FilterDocument
    {
        public FilterHost FilterHost = null;
    }

    /// <summary>
    /// 确保可见事件事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void EnsureVisibleEventHandler(object sender,
    EnsureVisibleEventArgs e);

    /// <summary>
    /// 确保可见事件的参数
    /// </summary>
    public class EnsureVisibleEventArgs : EventArgs
    {
        public Control Control = null;
        // 需要可见的部件边框位置
        // 以该控件父对象左上角为原点
        public Rectangle Rect = new Rectangle();
    }

    /// <summary>
    /// 显示错误信息事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void DisplayErrorEventHandler(object sender,
    DisplayErrorEventArgs e);

    /// <summary>
    /// 显示错误信息事件的参数
    /// </summary>
    public class DisplayErrorEventArgs : EventArgs
    {
        public string Text = "";    // [in]
        public string Color = "";   // [in]
    }

#if NO
    /// <summary>
    /// 获得服务器类型事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetServerTypeEventHandler(object sender,
        GetServerTypeEventArgs e);

    /// <summary>
    /// 获得服务器类型事件的参数
    /// </summary>
    public class GetServerTypeEventArgs : EventArgs
    {

        public string ServerName = "";  // [in]
        public string ServerType = "";  // [out]
    }
#endif
}
