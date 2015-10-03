using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Z3950;

using DigitalPlatform.Script;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;    // for NormalDbProperty

namespace dp2Catalog
{
    public partial class ZSearchForm : Form, ISearchForm, IZSearchForm
    {
        public string UsedLogFilename = ""; // 曾经用过的通讯包日志文件名

        // int m_nGroupSearchCount = 0;
        Stop m_stopDir = null;
        ZConnection m_connectionDir = null;
        int m_nTotalHitCount = 0;
        int m_nCompleteServerCount = 0; // 已经完成检索的服务器数
        int m_nServerCount = 0; // 需要检索的服务器总数。和m_stops.Count应该相等
        List<Stop> m_stops = new List<Stop>();
        bool m_bStartGroupSearch = false;

        VirtualItemCollection CurrentBrowseItems = null;
        // string m_strInitialResultInfo = ""; // 初始化结果信息

        // 浏览事项的类型下标
        public const int BROWSE_TYPE_NORMAL = 0;   // 普通记录
        public const int BROWSE_TYPE_DIAG = 1;     // 诊断记录 或者 4
        public const int BROWSE_TYPE_BRIEF = 2;     // 简化格式
        public const int BROWSE_TYPE_FULL = 3;     // 详细格式

        const int WM_LOADSIZE = API.WM_USER + 201;


        public string CurrentRefID = "0";   // "1 0 116101 11 1";

        MainForm m_mainForm = null;

        public MainForm MainForm
        {
            get
            {
                return this.m_mainForm;
            }
            set
            {
                this.m_mainForm = value;
            }
        }

        // DigitalPlatform.Stop Stop = null;

        // public ZChannel ZChannel = new ZChannel();
        public ZConnectionCollection ZConnections = new ZConnectionCollection();


        public string BinDir = "";

        // MarcFilter对象缓冲池
        public FilterCollection Filters = new FilterCollection();

        // 检索式和结果集参数
        // public TargetInfo CurrentTargetInfo = null;
        // public string CurrentQueryString = "";
        // public int ResultCount = 0;

        // Encoding ForcedRecordsEncoding = null;

        // 



        #region 常量



        #endregion

        public ZSearchForm()
        {
            InitializeComponent();
        }

        private void ZSearchForm_Load(object sender, EventArgs e)
        {
            if (this.m_mainForm != null)
            {
                GuiUtil.SetControlFont(this, this.m_mainForm.DefaultFont);
            }

            this.ZConnections.IZSearchForm = this;

            this.BinDir = Environment.CurrentDirectory;

            /*
            Stop = new DigitalPlatform.Stop();
            Stop.Register(MainForm.stopManager);	// 和容器关联
             * */

            string strWidths = this.m_mainForm.AppInfo.GetString(
"zsearchform",
"record_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                    strWidths,
                    true);
            }
            string[] fromlist = this.m_mainForm.GetFromList();

            for (int i = 0; i < 4; i++)
            {
                this.queryControl1.AddLine(fromlist);
            }

            int nRet = 0;
            string strError = "";
            nRet = this.zTargetControl1.Load(Path.Combine(m_mainForm.UserDir, "zserver.xml"),
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            this.zTargetControl1.Marc8Encoding = this.m_mainForm.Marc8Encoding;

            this.zTargetControl1.MainForm = this.m_mainForm;  // 2007/12/16

            //// this.ZChannel.CommIdle += new CommIdleEventHandle(ZChannel_CommIdle);
            this.zTargetControl1.AllowCheckbox = false;


            // 恢复上次留下的检索式
            string strContentsXml = m_mainForm.AppInfo.GetString(
                "zsearchform",
                "query_contents",
                "");
            /*
            if (String.IsNullOrEmpty(strContentXml) == false)
                this.queryControl1.SetContent(strContentXml);
             * */
            this.ZConnections.SetAllQueryXml(strContentsXml,
                this.zTargetControl1);

            // 选定上次选定的树节点
            string strLastTargetPath = m_mainForm.AppInfo.GetString(
                "zsearchform",
                "last_targetpath",
                "");
            if (String.IsNullOrEmpty(strLastTargetPath) == false)
            {
                TreeViewUtil.SelectTreeNode(this.zTargetControl1,
                    strLastTargetPath,
                    '\\');
            }

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            m_mainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);


            // 获得splitContainer_main的状态
            /*
            int nValue = MainForm.AppInfo.GetInt(
            "zsearchform",
            "splitContainer_main",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_main.SplitterDistance = nValue;
                }
                catch
                {
                }
            }
             * */
            this.m_mainForm.LoadSplitterPos(
this.splitContainer_main,
"zsearchform",
"splitContainer_main");


            // 获得splitContainer_up的状态
            /*
            nValue = MainForm.AppInfo.GetInt(
            "zsearchform",
            "splitContainer_up",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_up.SplitterDistance = nValue;
                }
                catch
                {
                }
            }
             * */
            this.m_mainForm.LoadSplitterPos(
this.splitContainer_up,
"zsearchform",
"splitContainer_up");

            // 获得splitContainer_queryAndResultInfo的状态
            /*
            nValue = MainForm.AppInfo.GetInt(
            "zsearchform",
            "splitContainer_queryAndResultInfo",
            -1);
            if (nValue != -1)
            {
                try
                {
                    this.splitContainer_queryAndResultInfo.SplitterDistance = nValue;
                }
                catch
                {
                }
            }
             * */
            this.m_mainForm.LoadSplitterPos(
this.splitContainer_queryAndResultInfo,
"zsearchform",
"splitContainer_queryAndResultInfo");

        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                m_mainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");

                // 保存splitContainer_main的状态
                /*
                MainForm.AppInfo.SetInt(
                    "zsearchform",
                    "splitContainer_main",
                    this.splitContainer_main.SplitterDistance);
                 * */
                this.m_mainForm.SaveSplitterPos(
        this.splitContainer_main,
        "zsearchform",
        "splitContainer_main");

                // 保存splitContainer_up的状态
                /*
                MainForm.AppInfo.SetInt(
                    "zsearchform",
                    "splitContainer_up",
                    this.splitContainer_up.SplitterDistance);
                 * */
                this.m_mainForm.SaveSplitterPos(
    this.splitContainer_up,
    "zsearchform",
    "splitContainer_up");

                // 保存splitContainer_queryAndResultInfo的状态
                /*
                MainForm.AppInfo.SetInt(
                    "zsearchform",
                    "splitContainer_queryAndResultInfo",
                    this.splitContainer_queryAndResultInfo.SplitterDistance);
                 * */
                this.m_mainForm.SaveSplitterPos(
    this.splitContainer_queryAndResultInfo,
    "zsearchform",
    "splitContainer_queryAndResultInfo");
            }
        }

        void ZChannel_CommIdle(object sender, CommIdleEventArgs e)
        {
            Application.DoEvents();
        }

        private void ZSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ZConnection connection = this.GetCurrentZConnection();
                if (connection != null)
                {
                    if (connection.Stop.State == 0)
                    {
                        DialogResult result = MessageBox.Show(this,
    "检索正在进行。需要先停止检索操作，才能关闭窗口。\r\n\r\n要停止检索操作么?",
    "ZSearchForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        {
                            connection.Stop.DoStop();
                        }
                        e.Cancel = true;
                        return;

                    }
                }
            }
            catch
            {
            }

            if (this.m_stops != null
                && this.m_stops.Count > 0)
            {
                DialogResult result = MessageBox.Show(this,
                "群检正在进行。需要先停止检索操作，才能关闭窗口。\r\n\r\n要停止检索操作么?",
                "ZSearchForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    StopDirSearchStops(true);

                }
                e.Cancel = true;
                return;
            }

            if (this.m_stops != null)
            {
                StopDirSearchStops(true);
            }

            //// this.ZChannel.CloseSocket();
            this.ZConnections.CloseAllSocket();
        }



        private void ZSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            if (Stop != null) // 脱离关联
            {
                Stop.DoStop();

                Stop.Unregister();	// 和容器关联
                Stop = null;
            }*/
            this.ZConnections.UnlinkAllStop();

            //// this.ZChannel.CommIdle -= new CommIdleEventHandle(ZChannel_CommIdle);

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                string strLastTargetPath = ZTargetControl.GetNodeFullPath(this.zTargetControl1.SelectedNode,
                    '\\');

                // TODO: applicationInfo有时为null
                m_mainForm.AppInfo.SetString(
                    "zsearchform",
                    "last_targetpath",
                    strLastTargetPath);

                // 促使当前一个treenode的检索式送到connections结构中，以便保存
                zTargetControl1_BeforeSelect(null, null);

                m_mainForm.AppInfo.SetString(
                    "zsearchform",
                    "query_contents",
                    this.ZConnections.GetAllQueryXml());

                // "query_content",
                // this.queryControl1.GetContent());

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
                this.m_mainForm.AppInfo.SetString(
                    "zsearchform",
                    "record_list_column_width",
                    strWidths);
            }

            SaveSize();

            this.zTargetControl1.Save();

        }



        // 2007/7/28
        // 获得当前选中的一个服务器目标相关的ZConnection
        // 如果没有ZConnection，不自动创建
        // 如果当前选择的是database类型，则要向上找到其所从属的server节点
        // TODO: 如果当前选择的是一个dir类型节点，是不是意味着要中断这个目录中的所有节点呢?
        ZConnection FindCurrentZConnection()
        {
            TreeNode curTreeNode = ZTargetControl.GetServerNode(this.zTargetControl1.SelectedNode);
            if (curTreeNode == null)
                return null;

            ZConnection result = this.ZConnections.FindZConnection(curTreeNode);

            return result;
        }


        // 2007/7/28
        // 获得和一个服务器树节点相关的ZConnection
        // 如果没有ZConnection，自动创建
        ZConnection GetZConnection(TreeNode node)
        {

            /*
            if (ZTargetControl.IsServer(nodeServerOrDatabase.ImageIndex) == false
                && ZTargetControl.IsDatabaseType(nodeServerOrDatabase) == false)
            {
                string strError = "所给出的树节点不是服务器类型 或 数据库类型";
                throw new Exception(strError);
            }
             * 
            TreeNode nodeServer = ZTargetControl.GetServerNode(nodeServerOrDatabase);
            if (nodeServer == null)
            {
                string strError = "所给出的树节点是数据库类型，但其父节点居然不是服务器类型";
                throw new Exception(strError);
                // return null;
            }


            ZConnection connection = this.ZConnections.GetZConnection(nodeServer);
             * */
            ZConnection connection = this.ZConnections.GetZConnection(node);

            if (connection.TargetInfo == null
                && ZTargetControl.IsDirType(node) == false)
            {
                string strError = "";
                TargetInfo targetinfo = null;
                int nRet = this.zTargetControl1.GetTarget(
                    node,
                    out targetinfo,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception("GetCurrentZConnection() error: " + strError);
                    // return null;
                }

                connection.TargetInfo = targetinfo;

                /*
                // 当前选择的如果是database类型节点，则修改targetinfo
                // TODO: 注意，将来真的在server类型节点上选择时，可不要沿用这个修改过的targetinfo
                if (nodeServer != nodeServerOrDatabase)
                {
                    targetinfo.DbNames = new string[1];
                    targetinfo.DbNames[0] = nodeServerOrDatabase.Text;  // 注意后面有没有命中数部分？
                }*/
            }

            return connection;
        }

        // 2007/7/28
        // 获得当前选中的一个服务器目标相关的ZConnection
        // 如果没有ZConnection，自动创建
        // 如果当前选择的是database类型，则要向上找到其所从属的server节点
        ZConnection GetCurrentZConnection()
        {
            /*
            // 如果必要，上升为server类型节点
            TreeNode curTreeNode = this.zTargetControl1.GetServerNode(this.zTargetControl1.SelectedNode);
            if (curTreeNode == null)
                return null;
             * */
            TreeNode curTreeNode = this.zTargetControl1.SelectedNode;
            if (curTreeNode == null)
                return null;

            ZConnection connection = this.ZConnections.GetZConnection(curTreeNode);

            if (connection.TargetInfo == null
                && ZTargetControl.IsDirType(curTreeNode) == false)
            {
                string strError = "";
                TargetInfo targetinfo = null;
                int nRet = this.zTargetControl1.GetTarget(
                    curTreeNode,
                    out targetinfo,
                    out strError);
                if (nRet == -1)
                {
                    throw new Exception("GetCurrentZConnection() error: " + strError);
                    // return null;
                }

                connection.TargetInfo = targetinfo;
            }

            return connection;
        }

        // 停止当前的一个Z连接
        void DoStop(object sender, StopEventArgs e)
        {
            /*
            if (this.ZChannel.Connected == true)
            {
                CloseConnection();
            }
            else if (this.ZChannel != null)
            {
                // 如果处在没有连接的状态
                this.ZChannel.Stop();
            }
             * */
            ZConnection connection = this.FindCurrentZConnection();
            if (connection != null)
            {
                connection.DoStop();
            }
        }

        // 检索一个目录。
        // 同时检索目录下的所有服务器
        public int PrepareSearchOneDir(
            TreeNode nodeDir,
            string strQueryXml,
            ref List<ZConnection> connections,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 如果用空调用，表示使用当前树上选定的节点
            if (nodeDir == null)
            {
                nodeDir = this.zTargetControl1.SelectedNode;
                if (ZTargetControl.IsDirType(nodeDir) == false)
                {
                    strError = "当前树节点类型不是目录， 不能用nodeDir为空调用DoSearchOneDir()";
                    goto ERROR1;
                }
            }

            TreeNodeCollection nodes = nodeDir.Nodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                TreeNode node = nodes[i];

                if (ZTargetControl.IsDirType(node) == true)
                {
                    nRet = PrepareSearchOneDir(node,
                        strQueryXml,
                        ref connections,
                        out strError);
                }
                else if (ZTargetControl.IsDatabaseType(node) == true
                    || ZTargetControl.IsServerType(node) == true)
                {
                    // return:
                    //      -2  尚未输入检索词
                    //      -1  一般错误
                    //      0   成功准备检索
                    nRet = PrepareSearchOneServer(
                        node,
                        strQueryXml,
                        ref connections,
                        out strError);
                    // TODO: 报错最好显示在各自的检索词面板上
                }

                /*
                if (nRet == -1)
                    return -1;
                 * */
            }

            return 0;
        ERROR1:
            return -1;
        }

        int m_nInTestSearching = 0;
        bool m_bTestStop = false;

        public int DoTestSearch()
        {
            if (this.m_nInTestSearching > 1)
            {
                this.m_bTestStop = true;
                return 0;
            }

            m_nInTestSearching++;
            try
            {
                m_bTestStop = false;
                for (; ; )
                {
                    if (m_bTestStop == true)
                        break;

                    DoSearch();

                    ZConnection connection = this.GetZConnection(this.zTargetControl1.SelectedNode);

                    while (true)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1000);
                        if (connection.Searching == 2)
                        {
                            break;
                        }
                    }

                    connection.CloseConnection();
                }
            }
            finally
            {
                m_nInTestSearching--;
            }

            MessageBox.Show(this, "end");
            return 0;
        }

        // 检索一个服务器或者目录
        // 这是包装后可以外部调用的版本 -- 检索当前目标树上选定的节点
        public int DoSearch()
        {
            string strError = "";
            int nRet = 0;

            TreeNode node = this.zTargetControl1.SelectedNode;
            if (ZTargetControl.IsServerType(node) == true
                || ZTargetControl.IsDatabaseType(node) == true)
            {
                // return:
                //      -2  尚未输入检索词
                //      -1  一般错误
                //      0   成功启动检索
                nRet = DoSearchOneServer(
                    node,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;
            }
            else
            {
                Debug.Assert(ZTargetControl.IsDirType(node) == true, "");

                if (this.m_stops.Count != 0)
                {
                    // TODO: 是否需要提示出哪个目录正在进行群检?
                    strError = "当前正在进行着群检，不允许多次启动群检";
                    goto ERROR1;
                }

                ZConnection connection = this.GetCurrentZConnection();

                // 获得共同的检索式
                connection.QueryXml = this.queryControl1.GetContent(true);
                if (String.IsNullOrEmpty(connection.QueryXml) == true)
                {
                    strError = "尚未输入检索式";
                    goto ERROR1;
                }

                // 准备工作
                this.m_nTotalHitCount = 0;
                this.m_nCompleteServerCount = 0;
                this.m_nServerCount = 0;

                this.m_stopDir = connection.Stop;
                this.m_connectionDir = connection;
                this.m_stops.Clear();
                this.m_bStartGroupSearch = false;

                node.Expand();

                /*
                lock (this)
                {
                } 
                 * */

#if NOOOOOOOOOOOOO
                this.m_stopDir.OnStop += new StopEventHandler(m_stopDir_OnStop);
                    this.m_stopDir.SetMessage("开始检索 ...");

                    this.m_stopDir.BeginLoop();
                    this.m_connectionDir.EnableControls(false);
#endif


                List<ZConnection> connections = new List<ZConnection>();

                nRet = PrepareSearchOneDir(node,
                    connection.QueryXml,
                    ref connections,
                    out strError);
                if (nRet == -1 || this.m_stops.Count == 0)
                {
#if NOOOOOOOOOOOOOO
                    lock (this)
                    {
                        this.m_connectionDir.EnableControls(true);
                        this.m_stopDir.EndLoop();
                        this.m_stopDir.OnStop -= new StopEventHandler(m_stopDir_OnStop);
                        this.m_stopDir.Initial("");

                        // 解挂全部事件
                        for (int i = 0; i < this.m_stops.Count; i++)
                        {
                            Stop stop = this.m_stops[i];

                            stop.OnBeginLoop -= new BeginLoopEventHandler(Stop_OnBeginLoop);
                            stop.OnEndLoop -= new EndLoopEventHandler(Stop_OnEndLoop);
                        }

                        this.m_stops.Clear();
                    }
#endif
                    goto ERROR1;
                }

                this.m_nServerCount = this.m_stops.Count;

                // 启动检索
                for (int i = 0; i < connections.Count; i++)
                {
                    ZConnection temp = connections[i];
#if THREAD_POOLING
                    List<string> commands = new List<string>();
                    commands.Add("search");
                    commands.Add("present");

                    temp.SetSearchParameters(
            temp.QueryString,
            temp.TargetInfo.DefaultQueryTermEncoding,
            temp.TargetInfo.DbNames,
            temp.TargetInfo.DefaultResultSetName);

                    temp.SetPresentParameters(
            temp.TargetInfo.DefaultResultSetName,
            0, // nStart,
            temp.TargetInfo.PresentPerBatchCount, // nCount,
            temp.TargetInfo.PresentPerBatchCount,   // 推荐的每次数量
            temp.DefaultElementSetName,    // "F" strElementSetName,
            temp.PreferredRecordSyntax,
            true);

                    temp.BeginCommands(commands);

#else
                    temp.Search();
#endif
                }
            }


            return 0;
        ERROR1:
            // 这里的报错，是在界面线程的报错
            try // 防止最后退出时报错
            {
                MessageBox.Show(this, strError);
                this.queryControl1.Focus();
            }
            catch
            {
            }
            return -1;
        }

        // 检索一个服务器
        // 包装后可以外部调用的版本 -- 检索当前目标树上选定的服务器节点
        public int DoSearchOneServer()
        {
            string strError = "";
            int nRet = 0;

            TreeNode nodeServerOrDatabase = this.zTargetControl1.SelectedNode;
            if (ZTargetControl.IsServerType(nodeServerOrDatabase) == false
                && ZTargetControl.IsDatabaseType(nodeServerOrDatabase) == false)
            {
                strError = "当前选择的节点不是服务器类型 或 数据库类型";
                goto ERROR1;
            }

            // return:
            //      -2  尚未输入检索词
            //      -1  一般错误
            //      0   成功启动检索
            nRet = DoSearchOneServer(nodeServerOrDatabase, 
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            return 0;
        ERROR1:
            // 这里的报错，是在界面线程的报错
            try // 防止最后退出时报错
            {
                MessageBox.Show(this, strError);
                this.queryControl1.Focus();
            }
            catch
            {
            }
            return -1;
        }

        // 准备检索一个服务器
        // 并不启动检索
        // thread:
        //      界面线程
        // return:
        //      -2  尚未输入检索词
        //      -1  一般错误
        //      0   成功准备检索
        public int PrepareSearchOneServer(
            TreeNode nodeServerOrDatabase,
            string strQueryXml,
            ref List<ZConnection> connections,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ZConnection connection = this.GetZConnection(nodeServerOrDatabase);
            Debug.Assert(connection.TargetInfo != null, "");

            Debug.Assert(connection.TreeNode == nodeServerOrDatabase, "");

            string strQueryString = "";

            IsbnConvertInfo isbnconvertinfo = new IsbnConvertInfo();
            isbnconvertinfo.IsbnSplitter = this.m_mainForm.IsbnSplitter;
            isbnconvertinfo.ConvertStyle =
                (connection.TargetInfo.IsbnAddHyphen == true ? "addhyphen," : "")
                + (connection.TargetInfo.IsbnRemoveHyphen == true ? "removehyphen," : "")
                + (connection.TargetInfo.IsbnForce10 == true ? "force10," : "")
                + (connection.TargetInfo.IsbnForce13 == true ? "force13," : "")
                + (connection.TargetInfo.IsbnWild == true ? "wild," : "");


            nRet = ZQueryControl.GetQueryString(
                this.m_mainForm.Froms,
                strQueryXml,
                isbnconvertinfo,
                out strQueryString,
                out strError);
            if (nRet == -1)
                return -1;

            connection.QueryString = strQueryString;
            connection.QueryXml = strQueryXml;


            if (strQueryString == "")
            {
                strError = "尚未输入检索词";
                return -2;
            }


            // this.m_nServerCount++;  // 累加服务器数
            this.m_stops.Add(connection.Stop);

            connection.Stop.OnBeginLoop -= new BeginLoopEventHandler(Stop_OnBeginLoop);
            connection.Stop.OnEndLoop -= new EndLoopEventHandler(Stop_OnEndLoop);

            connection.Stop.OnBeginLoop += new BeginLoopEventHandler(Stop_OnBeginLoop);
            connection.Stop.OnEndLoop += new EndLoopEventHandler(Stop_OnEndLoop);

            connections.Add(connection);

            return 0;
        }

        // 检索一个服务器
        // 启动检索以后控制就立即返回
        // thread:
        //      界面线程
        // return:
        //      -2  尚未输入检索词
        //      -1  一般错误
        //      0   成功启动检索
        public int DoSearchOneServer(
            // bool bInDirSearch,
            TreeNode nodeServerOrDatabase,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ZConnection connection = null;

            try
            {
                connection = this.GetZConnection(nodeServerOrDatabase);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            Debug.Assert(connection.TargetInfo != null, "");

            if (connection.TargetInfo.DbNames == null
                || connection.TargetInfo.DbNames.Length == 0)
            {
                strError = "服务器节点 '" + nodeServerOrDatabase.Text + "' 下的 " + nodeServerOrDatabase.Nodes.Count.ToString() + "  个数据库节点全部为 '在全选时不参与检索' 属性，所以通过选定该服务器节点无法直接进行检索，只能通过选定其下的某个数据库节点进行检索";
                return -1;
            }

            connection.Searching = 0;

            Debug.Assert(connection.TreeNode == nodeServerOrDatabase, "");

            string strQueryString = "";
            if (nodeServerOrDatabase == this.zTargetControl1.SelectedNode)
            {

                connection.QueryXml = this.queryControl1.GetContent(true);

                connection.TargetInfo.PreferredRecordSyntax = this.comboBox_recordSyntax.Text;
                connection.TargetInfo.DefaultElementSetName = this.comboBox_elementSetName.Text; 

                // this.ClearResultInfo(connection);
            }
            else
            {
                // strQueryString = connection.QueryString;

            }
            IsbnConvertInfo isbnconvertinfo = new IsbnConvertInfo();
            isbnconvertinfo.IsbnSplitter = this.m_mainForm.IsbnSplitter;
            isbnconvertinfo.ConvertStyle =
                (connection.TargetInfo.IsbnAddHyphen == true ? "addhyphen," : "")
                + (connection.TargetInfo.IsbnRemoveHyphen == true ? "removehyphen," : "")
                + (connection.TargetInfo.IsbnForce10 == true ? "force10," : "")
                + (connection.TargetInfo.IsbnForce13 == true ? "force13," : "")
                + (connection.TargetInfo.IsbnWild == true ? "wild," : "");


                nRet = ZQueryControl.GetQueryString(
                    this.m_mainForm.Froms,
                    connection.QueryXml,
                    isbnconvertinfo,
                    out strQueryString,
                    out strError);
                if (nRet == -1)
                    return -1;

                connection.QueryString = strQueryString;

            if (strQueryString == "")
            {
                strError = "尚未输入检索词";
                return -2;
            }

#if THREAD_POOLING
            List<string> commands = new List<string>();
            commands.Add("search");
            commands.Add("present");

            connection.SetSearchParameters(
    connection.QueryString,
    connection.TargetInfo.DefaultQueryTermEncoding,
    connection.TargetInfo.DbNames,
    connection.TargetInfo.DefaultResultSetName);

            connection.SetPresentParameters(
    connection.TargetInfo.DefaultResultSetName,
    0, // nStart,
    connection.TargetInfo.PresentPerBatchCount, // nCount,
    connection.TargetInfo.PresentPerBatchCount,   // 推荐的每次数量
    connection.DefaultElementSetName,    // "F" strElementSetName,
    connection.PreferredRecordSyntax,
    true);

            connection.BeginCommands(commands);
#else
            connection.Search();
#endif
            return 0;
        }

        void Stop_OnBeginLoop(object sender, BeginLoopEventArgs e)
        {
            lock (this)
            {
                // 第一次OnBegin触发
                if (this.m_bStartGroupSearch == false)
                {
                    this.m_stopDir.OnStop += new StopEventHandler(m_stopDir_OnStop);
                    this.m_stopDir.SetMessage("开始检索 ...");

                    /*
                    Stop active = this.MainForm.stopManager.ActiveStop;
                    Debug.Assert(this.MainForm.stopManager.IsActive(this.m_stopDir) == true, "");
                     * */

                    this.m_stopDir.BeginLoop();
                    this.m_connectionDir.EnableControls(false);
                }

                this.m_bStartGroupSearch = true;
            }
#if NOOOOOOOOOOOOOOOO
            lock (this)
            {
                this.m_stops.Add((Stop)sender);

                // 第一次OnBegin触发
                if (this.m_nGroupSearchCount == 0)
                {
                    this.m_stopDir.OnStop += new StopEventHandler(m_stopDir_OnStop);
                    this.m_stopDir.SetMessage("开始检索 ...");

                    /*
                    Stop active = this.MainForm.stopManager.ActiveStop;
                    Debug.Assert(this.MainForm.stopManager.IsActive(this.m_stopDir) == true, "");
                     * */

                    this.m_stopDir.BeginLoop();
                    this.m_connectionDir.EnableControls(false);
                }


                this.m_nGroupSearchCount++;
                this.m_nServerCount++;

                this.m_stopDir.SetMessage("正在检索 (目标数 " + this.m_nGroupSearchCount.ToString() + ") ...");
            }
#endif
        }

        // 目录检索停止按钮被触发
        void m_stopDir_OnStop(object sender, StopEventArgs e)
        {
            lock (this)
            {
                /*
                for (int i = 0; i < this.m_stops.Count; i++)
                {
                    Stop stop = this.m_stops[i];
                    if (stop.State == 0)
                        stop.DoStop(false);
                }*/
                StopDirSearchStops(false);
            }
        }

        // 停止正在进行的目录检索
        // parameters:
        //      bForce  是否进行最强悍的停止(强悍指close socket)。==false表示进行比较温和的停止，也就是对于已经结束检索的(socket还保持着)，不要去close socket
        void StopDirSearchStops(bool bForce)
        {
            for (int i = 0; i < this.m_stops.Count; i++)
            {
                Stop stop = this.m_stops[i];
                if (bForce == true)
                    stop.DoStop();
                else
                {
                    if (stop.State == 0)
                        stop.DoStop();
                }
            }
        }

        void Stop_OnEndLoop(object sender, EndLoopEventArgs e)
        {
            lock (this)
            {
                this.m_nCompleteServerCount++;
            }

            ZConnection connection = this.ZConnections.FindZConnection((Stop)sender);
            int nResultCount = Math.Max(0, connection.ResultCount);
            this.m_nTotalHitCount += nResultCount;

            this.m_connectionDir.ResultCount = this.m_nTotalHitCount;
            this.m_connectionDir.ShowQueryResultInfo("命中结果总数: " + this.m_nTotalHitCount.ToString()
                + (this.m_nCompleteServerCount == this.m_nServerCount ? "" : "..."));

            this.m_stopDir.SetMessage("正在检索。已完成检索的服务器数 " + this.m_nCompleteServerCount.ToString() + " (参与检索服务器总数 " + this.m_nServerCount.ToString() + ")...");

            // 最后一次OnEnd触发
            if (this.m_nCompleteServerCount == this.m_nServerCount)
            {
                this.m_connectionDir.EnableControls(true);

                this.m_stopDir.EndLoop();
                this.m_stopDir.OnStop -= new StopEventHandler(m_stopDir_OnStop);
                // this.m_stopDir.Initial("");

                // 收尾
                this.m_connectionDir = null;
                this.m_stopDir = null;
                this.m_nTotalHitCount = 0;
                this.m_nCompleteServerCount = 0;
                this.m_nServerCount = 0;

                // 解挂全部事件
                for (int i = 0; i < this.m_stops.Count; i++)
                {
                    Stop stop = this.m_stops[i];

                    stop.OnBeginLoop -= new BeginLoopEventHandler(Stop_OnBeginLoop);
                    stop.OnEndLoop -= new EndLoopEventHandler(Stop_OnEndLoop);
                }

                this.m_stops.Clear();
            }
        }



#if NOOOOOOOOOOOOOO
        // 清除以往的(已经获取到前端)结果集信息和相应的显示
        // thread:
        //      界面线程
        void ClearResultInfo(ZConnection connection)
        {
            ZConnection current_connection = this.GetCurrentZConnection();

            connection.ResultCount = -2;    // 表示正在检索

            if (connection.Records != null)
                connection.Records.Clear();

            // this.listView_browse.Items.Clear();
            if (connection.VirtualItems != null)
            {
                connection.VirtualItems.Clear();

                if (current_connection == connection)
                    LinkRecordsToListView(connection.VirtualItems); // listview是公用的
            }

            /*
            if (current_connection == connection)
            {
                this.textBox_resultInfo.Text = "";  // 这个textbox是公用的
            }
             * */
            if (current_connection == connection)
            {
                ShowQueryResultInfo(connection, "");
            }
           
        }

#endif

        /*
        void SetResultInfo(ZConnection connection)
        {
            this.textBox_resultInfo.Text = "命中结果条数:" + connection.ResultCount.ToString();
        }*/

        void EnableControls(bool bEnable)
        {
            this.zTargetControl1.Enabled = bEnable;
            this.queryControl1.Enabled = bEnable;
            this.listView_browse.Enabled = bEnable;
            this.textBox_resultInfo.Enabled = bEnable;

            this.comboBox_elementSetName.Enabled = bEnable;
            this.comboBox_recordSyntax.Enabled = bEnable;
        }

        // 处理Server可能发来的Close
        // return:
        //      -1  error
        //      0   不是Close
        //      1   是Close，已经迫使ZChannel处于尚未初始化状态
        int CheckServerCloseRequest(
            ZConnection connection,
            out string strMessage,
            out string strError)
        {
            strMessage = "";
            strError = "";

            if (connection.ZChannel.DataAvailable == false)
                return 0;

            int nRecvLen = 0;
            byte [] baPackage = null;
            int nRet = connection.ZChannel.RecvTcpPackage(
                        out baPackage,
                        out nRecvLen,
                        out strError);
            if (nRet == -1)
                return -1;

            BerTree tree1 = new BerTree();
            int nTotlen = 0;

            tree1.m_RootNode.BuildPartTree(baPackage,
                0,
                baPackage.Length,
                out nTotlen);

            if (tree1.GetAPDuRoot().m_uTag != BerTree.z3950_close)
            {
                // 不是Close
                return 0;
            }

            CLOSE_REQUEST closeStruct = new CLOSE_REQUEST();
            nRet = BerTree.GetInfo_closeRequest(
                tree1.GetAPDuRoot(),
                ref closeStruct,
                out strError);
            if (nRet == -1)
                return -1;

            strMessage = closeStruct.m_strDiagnosticInformation;

            /*
            this.ZChannel.CloseSocket();
            this.ZChannel.Initialized = false;  // 迫使重新初始化
            if (this.CurrentTargetInfo != null)
                this.CurrentTargetInfo.OfflineServerIcon();
             * */
            connection.CloseConnection();

            return 1;
        }


        // 发送原始包
        // parameters:
        //      strResultInfo   [out]返回说明初始化结果的文字
        int DoSendOriginPackage(
            ZConnection connection,
            byte [] baPackage,
            out string strError)
        {
            strError = "";

            TargetInfo targetinfo = connection.TargetInfo;
            /*
            if (connection.ZChannel.Initialized == true)
            {
                strError = "Already Initialized";
                goto ERROR1;
            }*/

            if (connection.ZChannel.Connected == false)
            {
                strError = "socket尚未连接或者已经被关闭";
                goto ERROR1;
            }


            byte[] baOutPackage = null;
            int nRecvLen = 0;
            int nRet = connection.ZChannel.SendAndRecv(
                baPackage,
                out baOutPackage,
                out nRecvLen,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            return 0;
        ERROR1:
            return -1;
        }

        // parameters:
        //      strResultInfo   [out]返回说明初始化结果的文字
        int DoInitial(
            ZConnection connection,
            out string strResultInfo,
            out string strError)
        {
            strResultInfo = "";
            strError = "";

            byte[] baPackage = null;
            BerTree tree = new BerTree();
            INIT_REQUEST struInit_request = new INIT_REQUEST();
            int nRet;
            int nRecvLen;

            TargetInfo targetinfo = connection.TargetInfo;

            if (connection.ZChannel.Initialized == true)
            {
                strError = "Already Initialized";
                goto ERROR1;
            }

            struInit_request.m_strReferenceId = this.CurrentRefID;  //  "0";!!!
            struInit_request.m_strOptions = "yynnnnnnnnnnnnnnnn";   // "yyynynnyynynnnyn";

            struInit_request.m_lPreferredMessageSize = 0x100000; ////16384;
            struInit_request.m_lExceptionalRecordSize = 0x100000;

            if (String.IsNullOrEmpty(targetinfo.UserName) == false)
            {
                struInit_request.m_strID = targetinfo.UserName;
                struInit_request.m_strPassword = targetinfo.Password;
                struInit_request.m_strGroupID = targetinfo.GroupID;
                struInit_request.m_nAuthenticationMethod = targetinfo.AuthenticationMethod;
            }
            else
            {
                struInit_request.m_strID = "";
                struInit_request.m_strPassword = "";
                struInit_request.m_strGroupID = "";
                struInit_request.m_nAuthenticationMethod = -1;
            }

            /*
            struInit_request.m_strImplementationId = "81";    // "81";
            struInit_request.m_strImplementationVersion = "2.0.3 WIN32 Debug";
            struInit_request.m_strImplementationName = "Index Data/YAZ";
             * */

            struInit_request.m_strImplementationId = "DigitalPlatform";
            struInit_request.m_strImplementationVersion = "1.1.0";
            struInit_request.m_strImplementationName = "dp2Catalog";

            if (targetinfo.CharNegoUTF8 == true)
            {
                struInit_request.m_charNego = new CharsetNeogatiation();
                struInit_request.m_charNego.EncodingLevelOID = CharsetNeogatiation.Utf8OID; //  "1.0.10646.1.0.8";   // utf-8
                struInit_request.m_charNego.RecordsInSelectedCharsets = (targetinfo.CharNegoRecordsUTF8 == true ? 1 : 0);
            }

            nRet = tree.InitRequest(struInit_request,
                   targetinfo.DefaultQueryTermEncoding,
                    out baPackage);
            if (nRet == -1)
            {
                strError = "CBERTree::InitRequest() fail!";
                goto ERROR1;
            }

            if (connection.ZChannel.Connected == false)
            {
                strError = "socket尚未连接或者已经被关闭";
                goto ERROR1;
            }


#if DUMPTOFILE
	DeleteFile("initrequest.bin");
	DumpPackage("initrequest.bin",
				(char *)baPackage.GetData(),
				baPackage.GetSize());
	DeleteFile ("initrequest.txt");
	tree.m_RootNode.DumpToFile("initrequest.txt");
#endif

            /*
            nRet = this.ZChannel.SendTcpPackage(
                baPackage,
                baPackage.Length,
                out strError);
            if (nRet == -1 || nRet == 1)
            {
                // CloseZAssociation();
                return -1;
            }

            baPackage = null;
            nRet = this.ZChannel.RecvTcpPackage(
                        out baPackage,
                        out nRecvLen,
                        out strError);
            if (nRet == -1)
            {
                // CloseZAssociation();
                return -1;
            }
             * */

            byte[] baOutPackage = null;
            nRet = connection.ZChannel.SendAndRecv(
                baPackage,
                out baOutPackage,
                out nRecvLen,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }



#if DUMPTOFILE
	DeleteFile("initresponse.bin");
	DumpPackage("initresponse.bin",
				(char *)baOutPackage.GetData(),
				baOutPackage.GetSize());
#endif

            ////////////////////////////////////////////////////////////////
            BerTree tree1 = new BerTree();
            int nTotlen = 0;

            tree1.m_RootNode.BuildPartTree(baOutPackage,
                0,
                baOutPackage.Length,
                out nTotlen);


#if DUMPTOFILE
	DeleteFile("InitResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("InitResponse.txt");
#endif

            /*
	nRet = FitDebugInfo_InitResponse(&tree1,
							  strError);
	if (nRet == -1) {
		return -1;
	}
	*/


            INIT_RESPONSE init_response = new INIT_RESPONSE();
            nRet = BerTree.GetInfo_InitResponse(tree1.GetAPDuRoot(),
                                 ref init_response,
                                 out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            if (targetinfo.IgnoreReferenceID == false)
            {
                // 2007/11/2。可以帮助发现旧版本dp2zserver的错误
                if (struInit_request.m_strReferenceId != init_response.m_strReferenceId)
                {
                    strError = "请求的 reference id [" + struInit_request.m_strReferenceId + "] 和 响应的 reference id [" + init_response.m_strReferenceId + "] 不一致！";
                    goto ERROR1;
                }
            }


            if (init_response.m_nResult != 0)
            {
                strError = "Initial OK";
            }
            else
            {
                strError = "Initial被拒绝。\r\n\r\n错误码 ["
                    + init_response.m_lErrorCode.ToString()
                    + "]\r\n错误消息["
                    + init_response.m_strErrorMessage + "]";

                strResultInfo = ZConnection.BuildInitialResultInfo(init_response);
                return -1;
            }

            /*
	this->m_init_strOption = init_response.m_strOptions;
	this->m_init_lPreferredMessageSize = init_response.m_lPreferredMessageSize;
	this->m_init_lExceptionalRecordSize = init_response.m_lExceptionalRecordSize;
	this->m_init_nResult = init_response.m_nResult;
             * */

            connection.ZChannel.Initialized = true;

            // 字符集协商
            if (init_response.m_charNego != null
                && BerTree.GetBit(init_response.m_strOptions, 17) == true)
            {
                if (init_response.m_charNego.EncodingLevelOID == CharsetNeogatiation.Utf8OID)
                {
                    // 临时修改检索词的编码方式。
                    // 但是还无法反映到PropertyDialog上。最好能反馈。
                    targetinfo.DefaultQueryTermEncoding = Encoding.UTF8;
                    targetinfo.Changed = true;

                    if (init_response.m_charNego.RecordsInSelectedCharsets == 1)
                        connection.ForcedRecordsEncoding = Encoding.UTF8;
                }
            }

            strResultInfo = ZConnection.BuildInitialResultInfo(init_response);

            return 0;
        ERROR1:
            strResultInfo = strError;
            return -1;
        }



        // 发送数据前检查连接
        int CheckConnect(
            ZConnection connection,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (connection.ZChannel.DataAvailable == true)
            {
                string strMessage = "";
                // 处理Server可能发来的Close
                // return:
                //      -1  error
                //      0   不是Close
                //      1   是Close，已经迫使ZChannel处于尚未初始化状态
                nRet = CheckServerCloseRequest(
                    connection,
                    out strMessage,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    nRet = connection.ZChannel.NewConnectSocket(connection.TargetInfo.HostName,
                        connection.TargetInfo.Port,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    connection.TargetInfo.OnlineServerIcon(true);

                    string strInitialResultInfo = "";
                    nRet = this.DoInitial(
                        connection,
                        // connection.TargetInfo,
                        out strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        connection.TargetInfo.OnlineServerIcon(false);
                        return -1;
                    }

                    // 设置当前树上已经选择的节点的扩展信息
                    nRet = ZTargetControl.SetCurrentTargetExtraInfo(
                        this.zTargetControl1.SelectedNode,
                        strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                }
            }

            return 0;
        }

#if NO_USE

        // return:
        //		-1	error
        //		0	fail
        //		1	succeed
        int DoSearch(
            ZConnection connection,
            string strQuery,
            Encoding queryTermEncoding,
            string[] dbnames,
            string strResultSetName,
            out int nResultCount,
            out string strError)
        {
            strError = "";

            BerTree tree = new BerTree();
            SEARCH_REQUEST struSearch_request = new SEARCH_REQUEST();
            byte[] baPackage = null;
            int nRet;
            int nRecvLen;
            //int nMax;
            //int i;



            // -->
            BerTree tree1 = new BerTree();
            int nTotlen = 0;


            nResultCount = 0;

            struSearch_request.m_dbnames = dbnames;

            Debug.Assert(struSearch_request.m_dbnames.Length != 0, "");

            struSearch_request.m_strReferenceId = this.CurrentRefID;
            struSearch_request.m_lSmallSetUpperBound = 0;
            struSearch_request.m_lLargeSetLowerBound = 1;
            struSearch_request.m_lMediumSetPresentNumber = 0;
            struSearch_request.m_nReplaceIndicator = 1;
            struSearch_request.m_strResultSetName = strResultSetName;   // "default";
            struSearch_request.m_strSmallSetElementSetNames = "";
            struSearch_request.m_strMediumSetElementSetNames = "";
            struSearch_request.m_strPreferredRecordSyntax = ZTargetControl.GetLeftValue(this.comboBox_recordSyntax.Text);    //  this.CurrentTargetInfo.PreferredRecordSyntax;   // BerTree.MARC_SYNTAX;
            struSearch_request.m_strQuery = strQuery;
            struSearch_request.m_nQuery_type = 1;
            struSearch_request.m_queryTermEncoding = queryTermEncoding;
           

            // m_search_response.m_lErrorCode = 0;

            nRet = tree.SearchRequest(struSearch_request,
                out baPackage);

            if (nRet == -1)
            {
                strError = "CBERTree::SearchRequest() fail!";
                return -1;
            }
#if NOTCPIP
	if (m_hSocket == INVALID_SOCKET) {
		strError = "socket已经关闭!";
		return -1;
	}
#endif


            #if DUMPTOFILE
            string strBinFile = this.MainForm.DataDir + "\\searchrequest.bin";
            File.Delete(strBinFile);
            DumpPackage(strBinFile,
                baPackage);
            string strLogFile = this.MainForm.DataDir + "\\searchrequest.txt";
            File.Delete(strLogFile);
            tree.m_RootNode.DumpToFile(strLogFile);
            #endif



            nRet = CheckConnect(
                connection,
                out strError);
            if (nRet == -1)
                return -1;

            /*
            nRet = this.ZChannel.SendTcpPackage(
                baPackage,
                baPackage.Length,
                out strError);
            if (nRet == -1 || nRet == 1)
            {
                // CloseZAssociation();
                return -1;
            }
            //AfxMessageBox("发送成功");


            baPackage = null;
            nRet = this.ZChannel.RecvTcpPackage(
                        out baPackage,
                        out nRecvLen,
                        out strError);
            if (nRet == -1)
            {
                // CloseZAssociation();
                return -1;
            }
             * */

            byte [] baOutPackage = null;
            nRet = connection.ZChannel.SendAndRecv(
                baPackage,
                out baOutPackage,
                out nRecvLen,
                out strError);
            if (nRet == -1)
                return -1;

#if DEBUG
            if (nRet == 0)
            {
                Debug.Assert(strError == "", "");
            }
#endif

#if DUPMTOFILE
	DeleteFile("searchresponse.bin");
	DumpPackage("searchresponse.bin",
				(char *)baOutPackage.GetData(),
				baOutPackage.GetSize());
#endif

            tree1.m_RootNode.BuildPartTree(baOutPackage,
                0,
                baOutPackage.Length,
                out nTotlen);

            SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
            nRet = BerTree.GetInfo_SearchResponse(tree1.GetAPDuRoot(),
                                   ref search_response,
                                   true,
                                   out strError);
            if (nRet == -1)
                return -1;

#if DUMPTOFILE
	DeleteFile("SearchResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("SearchResponse.txt");
#endif
            /*
	nRet = FitDebugInfo_SearchResponse(&tree1,
							  strError);
	if (nRet == -1) {
		AfxMessageBox(strError);
		return;
	}
	*/
            nResultCount = (int)search_response.m_lResultCount;

            if (search_response.m_nSearchStatus != 0)	// 不一定是1
                return 1;

            strError = "Search Fail: diagRecords:\r\n" + search_response.m_diagRecords.GetMessage();
            return 0;	// search fail
        }



        // 获得记录
        // 确保一定可以获得nCount个
        int DoPresent(
            ZConnection connection,
            string strResultSetName,
            int nStart,
            int nCount,
            string strElementSetName,
            string strPreferredRecordSyntax,
            out RecordCollection records,
            out string strError)
        {
            records = new RecordCollection();
            if (nCount == 0)
            {
                strError = "nCount为0";
                return 0;
            }

            int nGeted = 0;
            for (; ; )
            {
                RecordCollection temprecords = null;
                int nRet = DoOncePresent(
                    connection,
                    strResultSetName,
                    nStart + nGeted,
                    nCount - nGeted,
                    strElementSetName,
                    strPreferredRecordSyntax,
                    out temprecords,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (temprecords == null)
                    break;

                nGeted += temprecords.Count;
                if (temprecords.Count > 0)
                    records.AddRange(temprecords);

                if (nGeted >= nCount || temprecords.Count == 0)
                    break;
            }

            return 0;
        }

        // 获得记录
        // 不确保一定可以获得nCount个
        // parameters:
        //		nStart	开始记录(从0计算)
        int DoOncePresent(
            ZConnection connection,
            string strResultSetName,
            int nStart,
            int nCount,
            string strElementSetName,
            string strPreferredRecordSyntax,
            out RecordCollection records,
            out string strError)
        {
            records = null;
            strError = "";

            if (nCount == 0)
            {
                strError = "nCount为0";
                return 0;
            }


            BerTree tree = new BerTree();
            PRESENT_REQUEST struPresent_request = new PRESENT_REQUEST();
            byte[] baPackage = null;
            int nRet;
            int nRecvLen;

            // -->
            BerTree tree1 = new BerTree();
            int nTotlen = 0;

            struPresent_request.m_strReferenceId = this.CurrentRefID;
            struPresent_request.m_strResultSetName = strResultSetName; // "default";
            struPresent_request.m_lResultSetStartPoint = nStart + 1;
            struPresent_request.m_lNumberOfRecordsRequested = nCount;
            struPresent_request.m_strElementSetNames = strElementSetName;
            struPresent_request.m_strPreferredRecordSyntax = strPreferredRecordSyntax;

            nRet = tree.PresentRequest(struPresent_request,
                                     out baPackage);
            if (nRet == -1)
            {
                strError = "CBERTree::PresentRequest() fail!";
                return -1;
            }


#if DUMPTOFILE
	DeleteFile("presentrequest.bin");
	DumpPackage("presentrequest.bin",
		(char *)baPackage.GetData(),
		baPackage.GetSize());
	DeleteFile ("presentrequest.txt");
	tree.m_RootNode.DumpToFile("presentrequest.txt");
#endif

            nRet = CheckConnect(
                connection,
                out strError);
            if (nRet == -1)
                return -1;

            /*
            nRet = this.ZChannel.SendTcpPackage(
                baPackage,
                baPackage.Length,
                out strError);
            if (nRet == -1 || nRet == 1)
            {
                // CloseZAssociation();
                return -1;
            }

            //////////////


            baPackage = null;
            nRet = this.ZChannel.RecvTcpPackage(
                        out baPackage,
                        out nRecvLen,
                        out strError);
            if (nRet == -1)
            {
                // CloseZAssociation();
                goto ERROR1;
            }
             * */

            byte [] baOutPackage = null;
            nRet = connection.ZChannel.SendAndRecv(
                baPackage,
                out baOutPackage,
                out nRecvLen,
                out strError);
            if (nRet == -1)
                return -1;

#if DUMPTOFILE	
	DeleteFile("presendresponse.bin");
	DumpPackage("presentresponse.bin",
				(char *)baPackage.GetData(),
				baPackage.GetSize());
#endif


            tree1.m_RootNode.BuildPartTree(baOutPackage,
                0,
                baOutPackage.Length,
                out nTotlen);

#if DUMPTOFILE
	DeleteFile("PresentResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("PresentResponse.txt");
#endif

            SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
            nRet = BerTree.GetInfo_PresentResponse(tree1.GetAPDuRoot(),
                                   ref search_response,
                                   out records,
                                   true,
                                   out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            nRet = FitDebugInfo_PresentResponse(&tree1,
                                      strError);
            if (nRet == -1) {
                goto ERROR1;
            }

            DeleteFile("PresentResponse.txt"); 
            tree1.m_RootNode.DumpDebugInfoToFile("PresentResponse.txt");
            */


            if (search_response.m_diagRecords.Count != 0)
            {
                /*
                string strDiagText;
                string strAddInfo;

                nRet = GetDiagTextByNumber("bib1diag.txt",
                                m_search_response.m_nDiagCondition,
                                strDiagText,
                                strAddInfo,
                                strError);
                if (nRet == -1) {
                    if (this->m_bAllowMessageBox)
                        AfxMessageBox(strError);
                    return -1;
                }
                if (strDiagText.GetLength())
                    strError = strDiagText;
                else
                    strError.Format("diag condition[%d] diag set id[%s]",
                    m_search_response.m_nDiagCondition,
                    m_search_response.m_strDiagSetID);
                 * */
                strError = "error diagRecords:\r\n\r\n---\r\n" + search_response.m_diagRecords.GetMessage();
                return -1;
            }

            return 0;
        ERROR1:
            return -1;
        }

#endif


        void DumpPackage(string strFileName,
            byte[] baPackage)
        {
            Stream stream = File.Create(strFileName);

            stream.Write(baPackage, 0, baPackage.Length);

            stream.Close();
        }

        #region IZSearchForm 接口实现

        public delegate void Delegate_EnableQueryControl(
            ZConnection connection,
            bool bEnable);

        // 根据connection是否为当前connection，决定是否执行
        // Enable/Disable检索式控件的操作
        void __EnableQueryControl(
            ZConnection connection,
            bool bEnable)
        {
            ZConnection cur_connection = this.GetCurrentZConnection();
            if (cur_connection == connection)
            {
                EnableQueryControl(bEnable);
            }
        }

        public void EnableQueryControl(
    ZConnection connection,
    bool bEnable)
        {
            object[] pList = { connection, bEnable };
            this.Invoke(
                new ZSearchForm.Delegate_EnableQueryControl(__EnableQueryControl), pList);
        }

        void EnableQueryControl(bool bEnable)
        {
            this.queryControl1.Enabled = bEnable;
            this.comboBox_elementSetName.Enabled = bEnable;
            this.comboBox_recordSyntax.Enabled = bEnable;
        }

        public delegate bool Delegate_DisplayBrowseItems(ZConnection connection,
            bool bTriggerSelChanged);

        // 显示当前新获得的浏览记录
        // 函数会自动判断，只有当前ZConnection才会显示出来
        bool __DisplayBrowseItems(ZConnection connection,
            bool bTriggerSelChanged = false)
        {
            bool bRet = false;  // 没有被显示
            if (connection == this.GetCurrentZConnection())
            {
                LinkRecordsToListView(connection.VirtualItems);
                bRet = true;    // 被显示了
            }

            if (bTriggerSelChanged == true)
            {
                listView_browse_SelectedIndexChanged(null, null);
            }

            return bRet;   
        }

        public bool DisplayBrowseItems(ZConnection connection,
            bool bTriggerSelChanged = false)
        {
            object[] pList = { connection, bTriggerSelChanged };
            return (bool)this.Invoke(
                new ZSearchForm.Delegate_DisplayBrowseItems(__DisplayBrowseItems), pList);
        }

        public delegate bool Delegate_ShowMessageBox(ZConnection connection,
            string strText);

        // 显示MessageBox()
        // 函数会自动判断，只有当前ZConnection才会显示出来
        bool __ShowMessageBox(ZConnection connection,
            string strText)
        {
            if (connection == this.GetCurrentZConnection())
            {
                MessageBox.Show(this, strText);
                return true;    // 被显示了
            }

            return false;   // 没有被显示
        }

        public bool ShowMessageBox(ZConnection connection,
           string strText)
        {
            object[] pList = { connection, strText };
            if (this.IsDisposed == true)
                return false; // 防止窗口关闭后这里抛出异常 2014/5/15
            return (bool)this.Invoke(
                new ZSearchForm.Delegate_ShowMessageBox(__ShowMessageBox), pList);
        }

        public delegate bool Delegate_ShowQueryResultInfo(ZConnection connection,
            string strText);

        // 显示查询结果信息，在检索式的下部textbox中
        // 函数会自动判断，只有当前ZConnection才会显示出来
        bool __ShowQueryResultInfo(ZConnection connection,
            string strText)
        {
            // 修改treenode节点上的命中数显示
            ZTargetControl.SetNodeResultCount(connection.TreeNode,
                connection.ResultCount);

            if (connection == this.GetCurrentZConnection())
            {
                this.textBox_resultInfo.Text = strText;
                return true;    // 被显示了
            }

            return false;   // 没有被显示
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2catalog 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的异常: 
Type: System.InvalidOperationException
Message: 在创建窗口句柄之前，不能在控件上调用 Invoke 或 BeginInvoke。
Stack:
在 System.Windows.Forms.Control.WaitForWaitHandle(WaitHandle waitHandle)
在 System.Windows.Forms.Control.MarshaledInvoke(Control caller, Delegate method, Object[] args, Boolean synchronous)
在 System.Windows.Forms.Control.Invoke(Delegate method, Object[] args)
在 dp2Catalog.ZSearchForm.ShowQueryResultInfo(ZConnection connection, String strText)
在 dp2Catalog.ZConnection.ShowQueryResultInfo(String strText)
在 dp2Catalog.ZConnection.ZConnection_CommandsComplete(Object sender, EventArgs e)
在 dp2Catalog.ZConnection.ZChannel_ConnectComplete(Object sender, EventArgs e)
在 DigitalPlatform.Z3950.ZChannel.ConnectCallback(IAsyncResult ar)
在 System.Net.LazyAsyncResult.Complete(IntPtr userToken)
在 System.Net.ContextAwareResult.CompleteCallback(Object state)
在 System.Threading.ExecutionContext.runTryCode(Object userData)
在 System.Runtime.CompilerServices.RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(TryCode code, CleanupCode backoutCode, Object userData)
在 System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)
在 System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean ignoreSyncCtx)
在 System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
在 System.Net.ContextAwareResult.Complete(IntPtr userToken)
在 System.Net.LazyAsyncResult.ProtectedInvokeCallback(Object result, IntPtr userToken)
在 System.Net.LazyAsyncResult.InvokeCallback(Object result)
在 System.Net.Sockets.Socket.MultipleAddressConnectCallback(IAsyncResult result)
在 System.Net.LazyAsyncResult.Complete(IntPtr userToken)
在 System.Net.ContextAwareResult.Complete(IntPtr userToken)
在 System.Net.LazyAsyncResult.ProtectedInvokeCallback(Object result, IntPtr userToken)
在 System.Net.Sockets.BaseOverlappedAsyncResult.CompletionPortCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* nativeOverlapped)
在 System.Threading._IOCompletionCallback.PerformIOCompletionCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* pOVERLAP)


dp2Catalog 版本: dp2Catalog, Version=2.4.5701.40614, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7600.0 
操作时间 2015/8/20 14:22:46 (Thu, 20 Aug 2015 14:22:46 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 

         * */
        public bool ShowQueryResultInfo(ZConnection connection,
           string strText)
        {
            if (this.IsDisposed == true
                || this.IsHandleCreated == false)   // 2015/8/21
                return false;

            object[] pList = { connection, strText };
            return (bool)this.Invoke(
                new ZSearchForm.Delegate_ShowQueryResultInfo(__ShowQueryResultInfo), pList);
        }

        // 根据不同格式自动创建浏览格式
        public int BuildBrowseText(
            ZConnection connection,
            DigitalPlatform.Z3950.Record record,
            string strStyle,
            out string strBrowseText,
            out int nImageIndex,
            out string strError)
        {
            strBrowseText = "";
            strError = "";
            int nRet = 0;

            nImageIndex = BROWSE_TYPE_NORMAL;

            if (record.m_nDiagCondition != 0)
            {
                strBrowseText = "诊断记录 condition=" + record.m_nDiagCondition.ToString() + "; addinfo=\"" + record.m_strAddInfo + "\"; diagSetOID=" + record.m_strDiagSetID;
                nImageIndex = BROWSE_TYPE_DIAG;
                return 0;
            }

            string strElementSetName = record.m_strElementSetName;

            if (strElementSetName == "B")
                nImageIndex = BROWSE_TYPE_BRIEF;
            else if (strElementSetName == "F")
                nImageIndex = BROWSE_TYPE_FULL;

            Encoding currrentEncoding = connection.GetRecordsEncoding(
                this.m_mainForm,
                record.m_strSyntaxOID);

            string strSytaxOID = record.m_strSyntaxOID;
            string strData = currrentEncoding.GetString(record.m_baRecord);

            // string strOutFormat = "";
            string strMARC = "";    // 暂存MARC机内格式数据

            // 如果为XML格式
            if (record.m_strSyntaxOID == "1.2.840.10003.5.109.10")
            {
                // 如果偏向MARC
                if (StringUtil.IsInList("marc", strStyle) == true)
                {
                    // 看根节点的名字空间，如果符合MARCXML, 就先转换为USMARC，否则，就直接根据名字空间找样式表加以转换
                    string strNameSpaceUri = "";
                    nRet = GetRootNamespace(strData,
                        out strNameSpaceUri,
                        out strError);
                    if (nRet == -1)
                    {
                        // 取根节点的名字空间时出错
                        return -1;
                    }

                    if (strNameSpaceUri == Ns.usmarcxml)
                    {
                        string strOutMarcSyntax = "";

                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(strData,
                            true,
                            "usmarc",
                            out strOutMarcSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                        {
                            // XML转换为MARC时出错
                            return -1;
                        }

                        // strOutFormat = "marc";
                        strSytaxOID = "1.2.840.10003.5.10";
                        goto DO_BROWSE;
                    }

                }

                // 不是MARCXML格式
                // strOutFormat = "xml";
                goto DO_BROWSE;
            }

            // SUTRS
            if (record.m_strSyntaxOID == "1.2.840.10003.5.101")
            {
                // strOutFormat = "sutrs";
                goto DO_BROWSE;
            }

            if (record.m_strSyntaxOID == "1.2.840.10003.5.1"    // unimarc
                || record.m_strSyntaxOID == "1.2.840.10003.5.10")  // usmarc
            {
                // ISO2709转换为机内格式
                nRet = Marc8Encoding.ConvertByteArrayToMarcRecord(
                    record.m_baRecord,
                    connection.GetRecordsEncoding(this.m_mainForm, record.m_strSyntaxOID),  // Encoding.GetEncoding(936),
                    true,
                    out strMARC,
                    out strError);
                if (nRet < 0)
                {
                    return -1;
                }

                // 如果需要自动探测MARC记录从属的格式：
                if (connection.TargetInfo.DetectMarcSyntax == true)
                {
                    // return:
                    //		-1	无法探测
                    //		1	UNIMARC	规则：包含200字段
                    //		10	USMARC	规则：包含008字段(innopac的UNIMARC格式也有一个奇怪的008)
                    nRet = ZSearchForm.DetectMARCSyntax(strMARC);
                    if (nRet == 1)
                        strSytaxOID = "1.2.840.10003.5.1";
                    else if (nRet == 10)
                        strSytaxOID = "1.2.840.10003.5.10";

                    // 把自动识别的结果保存下来
                    record.AutoDetectedSyntaxOID = strSytaxOID;
                }

                // strOutFormat = "marc";
                goto DO_BROWSE;
            }

            // 不能识别的格式。原样放置
            strBrowseText = strData;
            return 0;

        DO_BROWSE:

            if (strSytaxOID == "1.2.840.10003.5.1"    // unimarc
                || strSytaxOID == "1.2.840.10003.5.10")  // usmarc
            {
                return BuildMarcBrowseText(
                strSytaxOID,
                strMARC,
                out strBrowseText,
                out strError);
            }

            // XML还暂时没有转换办法
            strBrowseText = strData;
            return 0;
        }



        // 创建MARC格式记录的浏览格式
        // paramters:
        //      strMARC MARC机内格式
        public int BuildMarcBrowseText(
            string strSytaxOID,
            string strMARC,
            out string strBrowseText,
            out string strError)
        {
            strBrowseText = "";
            strError = "";

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this.m_mainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = this.m_mainForm.DataDir + "\\" + strSytaxOID.Replace(".", "_") + "\\marc_browse.fltx";

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
                this.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        #endregion

#if NOOOOOOOOOOOOOOOOO
        // 结果集追加到listview中
        // parameters:
        //      records 当前新获得一批记录。需要追加到connection的Records中
        int FillRecordsToBrowseView(
            Stop stop,
            ZConnection connection,
            RecordCollection records,
            out string strError)
        {

            Debug.Assert(connection == this.GetCurrentZConnection(), "不是当前connection，装入listview会破坏界面");

            strError = "";
            if (connection.Records == null)
                connection.Records = new RecordCollection();

            int nExistCount = connection.Records.Count;
            Debug.Assert(this.listView_browse.Items.Count == nExistCount, "");

            // 加入新的一批
            connection.Records.AddRange(records);

            int nRet = FillRecordsToBrowseView(
                stop,
                connection,
                connection.Records,
                nExistCount,
                records.Count,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }
#endif

        public int PrepareMarcFilter(
    FilterHost host,
    string strFilterFileName,
    out BrowseFilterDocument filter,
    out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)this.Filters.GetFilter(strFilterFileName);

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
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string[] saAddRef1 = {
										 this.BinDir + "\\digitalplatform.marcdom.dll",
										 // this.BinDir + "\\digitalplatform.marckernel.dll",
										 // this.BinDir + "\\digitalplatform.libraryserver.dll",
										 this.BinDir + "\\digitalplatform.dll",
										 this.BinDir + "\\digitalplatform.Text.dll",
										 this.BinDir + "\\digitalplatform.IO.dll",
										 this.BinDir + "\\digitalplatform.Xml.dll",
										 this.BinDir + "\\dp2catalog.exe" };

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



        public void GetAllRecords()
        {
            ZConnection connection = this.GetCurrentZConnection();

            GetNextAllBatch(connection);
        }

        public void GetNextAllBatch(ZConnection connection)
        {
            if (/*this.listView_browse.Items.Count*/
                connection.Records.Count
                >= connection.ResultCount)
            {
                MessageBox.Show(this, "已经获得了全部命中记录");
                return;
            }


            /*
            while(
                connection.Records.Count < connection.ResultCount )
            {
                connection.NextBatch(true);
            }
             * */
            connection.NextAllBatch(true);
        }

        // 获得下一批结果集中数据
        // 启动后控制就立即返回
        // thread:
        //      界面线程
        // return:
        //      -1  error
        //      0   线程已经启动，但是没有等它结束
        //      1   线程已经结束
        public int NextBatch()
        {
            ZConnection connection = this.GetCurrentZConnection();

            if (connection.Records.Count >= connection.ResultCount)
            {
                MessageBox.Show(this, "已经获得了结果集中全部记录");
                return 1;
            }

            return connection.NextBatch(false);
        }

#if NO_USE
        public int NextBatch()
        {
            ZConnection connection = this.GetCurrentZConnection();
            return NextBatch(connection);
        }

        public int NextBatch(ZConnection connection)
        {
            string strError = "";
            int nRet = 0;

            // 新装入一批记录
            int nCount = Math.Min(connection.TargetInfo.PresentPerBatchCount,
                connection.ResultCount - this.listView_browse.Items.Count);

            if (nCount <= 0)
            {
                // 没有必要么
                strError = "命中结果已经全部获取完毕。";
                goto ERROR1;
            }

            // ZConnection connection = this.GetCurrentZConnection();

            connection.Stop.OnStop += new StopEventHandler(this.DoStop);
            connection.Stop.SetMessage("从服务器装入记录 ...");
            connection.Stop.BeginLoop();

            EnableControls(false);

            this.Update();
            this.MainForm.Update();

            try
            {

                string strElementSetName = ZTargetControl.GetLeftValue(this.comboBox_elementSetName.Text);  // this.CurrentTargetInfo.DefaultElementSetName;

                if (strElementSetName == "B"
                    && connection.TargetInfo.FirstFull == true)
                    strElementSetName = "F";

                RecordCollection records = null;

                nRet = DoPresent(
                    connection,
                    connection.TargetInfo.DefaultResultSetName,
                    this.listView_browse.Items.Count, // nStart,
                    nCount, // nCount,
                    strElementSetName,    // "F" strElementSetName,
                    ZTargetControl.GetLeftValue(this.comboBox_recordSyntax.Text), // this.CurrentTargetInfo.PreferredRecordSyntax,
                    out records,
                    out strError);
                if (nRet == -1)
                {
                    strError = "从 " + this.listView_browse.Items.Count.ToString()
                        + " 开始装入新的一批记录时出错：" + strError;
                    goto ERROR1;
                }
                else
                {
                    nRet = FillRecordsToBrowseView(
                        connection.Stop,
                        connection,
                        records,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
            }
            finally
            {
                connection.Stop.EndLoop();
                connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                connection.Stop.Initial("");

                EnableControls(true);
            }

            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

#endif

        #region ISearchForm 接口函数

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

        public string CurrentProtocol
        {
            get
            {
                return "Z39.50";
            }
        }

        public string CurrentResultsetPath
        {
            get
            {
                ZConnection connection = this.GetCurrentZConnection();
                if (connection == null)
                    return "";

                return connection.TargetInfo.HostName
                    + ":" + connection.TargetInfo.Port.ToString()
                    + "/" + string.Join(",", connection.TargetInfo.DbNames)
                    + "/default";
            }
        }

        // 刷新一条MARC记录
        // parameters:
        //      strAction   refresh / delete
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            if (this.IsDisposed == true)
            {
                strError = "相关的Z39.50检索窗已经销毁，没有必要刷新";
                return 0;
            }

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            if (index == -1)
            {
                strError = "暂时不支持没有 index 的用法";
                return -1;
            }

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                return -1;
            }

            if (index >= connection.ResultCount)
            {
                strError = "越过结果集尾部";
                return -1;
            }

            if (strAction == "refresh")
            {

                // 新装入一批记录
                int nCount = 1;

                Debug.Assert(connection.Stop != null, "");

                connection.Stop.OnStop += new StopEventHandler(this.DoStop);
                connection.Stop.SetMessage("从服务器装入一条记录 ...");
                connection.Stop.BeginLoop();

                // EnableControls(false);
                EnableQueryControl(false);

                this.Update();
                // this.m_mainForm.Update();

                try
                {
                    string strElementSetName = ZTargetControl.GetLeftValue(this.comboBox_elementSetName.Text);

                    if (strElementSetName == "B"
                        && connection.TargetInfo.FirstFull == true)
                        strElementSetName = "F";

                    RecordCollection records = null;

                    // TODO: 这里要的不是追加效果，而是替换一个已经存在的事项
                    nRet = connection.DoPresent(
                        connection.TargetInfo.DefaultResultSetName,
                        index, // nStart,
                        nCount, // nCount,
                        connection.TargetInfo.PresentPerBatchCount, // 推荐的每次数量
                        strElementSetName,    // "F" strElementSetName,
                        connection.PreferredRecordSyntax,  //this.comboBox_recordSyntax.Text),    //this.CurrentTargetInfo.PreferredRecordSyntax,
                        true,   // 立即显示出来
                        out records,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "从 " + index.ToString()
                            + " 位置(从0开始计数)装入一条记录时出错：" + strError;
                        return -1;
                    }
                }
                finally
                {
                    connection.Stop.EndLoop();
                    connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                    connection.Stop.Initial("");

                    // EnableControls(true);
                    EnableQueryControl(true);
                }
            }

            return 1;
        }

#if NOOOOOOOOOOOOOOOOOOOO
        // return:
        //      -1  error
        //      0   没有探测出来
        //      1   探测出来了，结果在strMarcSyntaxOID参数中
        public static int DetectMarcSyntax(string strOID,
            string strContent,
            out string strMarcSyntaxOID,
            out string strError)
        {
            strError = "";
            strMarcSyntax = "";

            // 可能为XML格式
            if (strOID == "1.2.840.10003.5.109.10")
            {
                // 看根节点的名字空间，如果符合MARCXML, 就先转换为USMARC，否则，就直接根据名字空间找样式表加以转换
                string strNameSpaceUri = "";
                nRet = GetRootNamespace(strContent,
                    out strNameSpaceUri,
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }

                if (strNameSpaceUri == Ns.usmarcxml)
                {
                    strMarcSyntaxOID = "1.2.840.10003.5.109.10";
                    return 1;
                }
            }

            return 0;
        }

#endif

        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            return 0;
        }

        // 获得一条MARC/XML记录
        // return:
        //      -1  error
        //      0   suceed
        //      1   为诊断记录
        public int GetOneRecord(
            string strStyle,
            int nTest,
            string strPathParam,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strMARC,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.Z3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo,
            out string strError)
        {
            strXmlFragment = "";
            strMARC = "";
            record = null;
            strError = "";
            currrentEncoding = null;
            baTimestamp = null;
            strSavePath = "";
            strOutStyle = "";
            logininfo = new LoginInfo();
            lVersion = 0;

            int nRet = 0;

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            if (index == -1)
            {
                strError = "暂时不支持没有 index 的用法";
                return -1;
            }

            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);
            bool bForceFullElementSet = StringUtil.IsInList("force_full", strParameters);

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                return -1;
            }

            if (index >= this.listView_browse.Items.Count)
            {
                if (index >= connection.ResultCount)
                {
                    strError = "越过结果集尾部";
                    return -1;
                }

                // 新装入一批记录
                int nCount = Math.Min(connection.TargetInfo.PresentPerBatchCount,
                    connection.ResultCount - this.listView_browse.Items.Count);

                if (nCount <= 0)
                {
                    strError = "此时不可能nCount为 " + nCount.ToString();
                    return -1;
                }

                connection.Stop.OnStop += new StopEventHandler(this.DoStop);
                connection.Stop.SetMessage("从服务器装入记录 ...");
                connection.Stop.BeginLoop();

                // EnableControls(false);
                EnableQueryControl(false);

                this.Update();
                this.m_mainForm.Update();

                ActivateStopDisplay();  // 2011/9/11

                try
                {
                    string strElementSetName = ZTargetControl.GetLeftValue(this.comboBox_elementSetName.Text);  // this.CurrentTargetInfo.DefaultElementSetName;

                    if (strElementSetName == "B"
                        && connection.TargetInfo.FirstFull == true)
                        strElementSetName = "F";

                    if (bForceFullElementSet == true)
                        strElementSetName = "F";

                    RecordCollection records = null;

                    nRet = connection.DoPresent(
                        connection.TargetInfo.DefaultResultSetName,
                        this.listView_browse.Items.Count, // nStart,
                        nCount, // nCount,
                        connection.TargetInfo.PresentPerBatchCount, // 推荐的每次数量
                        strElementSetName,    // "F" strElementSetName,
                        connection.PreferredRecordSyntax,  //this.comboBox_recordSyntax.Text),    //this.CurrentTargetInfo.PreferredRecordSyntax,
                        true,   // 立即显示出来
                        out records,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "从 " + this.listView_browse.Items.Count.ToString()
                            + " 开始装入新的一批记录时出错：" + strError;
                        return -1;
                    }
                    else
                    {
                        /*
                        nRet = connection.FillRecordsToVirtualItems(
                            connection.Stop,
                            records,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        DisplayBrowseItems(connection);
                         * */
                    }
                }
                finally
                {
                    connection.Stop.EndLoop();
                    connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                    connection.Stop.Initial("");

                    // EnableControls(true);
                    EnableQueryControl(true);
                }

                if (index >= this.listView_browse.Items.Count)
                {
                    strError = "index越界";
                    return -1;
                }
            }

            if (bHilightBrowseLine == true)
            {
                // 修改listview中事项的选定状态
                for (int i = 0; i < connection.VirtualItems.SelectedIndices.Count; i++)
                {
                    int temp_index = connection.VirtualItems.SelectedIndices[i];
                    if (temp_index != index)
                    {
                        if (this.listView_browse.Items[temp_index].Selected != false)
                            this.listView_browse.Items[temp_index].Selected = false;
                    }
                }

                ListViewItem curListViewItem = this.listView_browse.Items[index];
                if (curListViewItem.Selected != true)
                    curListViewItem.Selected = true;
                curListViewItem.EnsureVisible();
            }

            // 
            // strSavePath = (index+1).ToString();

            // 
            record = (DigitalPlatform.Z3950.Record)
                connection.VirtualItems[index].Tag; //  curListViewItem.Tag;

            if (record == null)
            {
                strError = "VirtualItem Tag为空";
                return -1;
            }

            if (record.m_nDiagCondition != 0)
            {
                strError = "这是一条诊断记录";
                strOutStyle = "marc";
                strMARC = "012345678901234567890123001这是一条诊断记录";
                return 1;
            }


            {
                Debug.Assert(string.IsNullOrEmpty(record.m_strElementSetName) == false, "");

                string strCurrentElementSetName = record.m_strElementSetName;
                string strElementSetName = strCurrentElementSetName;

                if (strCurrentElementSetName == "B"
                    && connection.TargetInfo.FirstFull == true)
                    strElementSetName = "F";

                if (bForceFullElementSet == true)
                    strElementSetName = "F";

                if (strCurrentElementSetName != strElementSetName)
                {
                    connection.Stop.OnStop += new StopEventHandler(this.DoStop);
                    connection.Stop.SetMessage("从服务器装入记录 ...");
                    connection.Stop.BeginLoop();

                    EnableQueryControl(false);
                    try
                    {

                        RecordCollection records = null;
                        nRet = connection.DoPresent(
            connection.TargetInfo.DefaultResultSetName,
            index, // nStart,
            1, // nCount,
            connection.TargetInfo.PresentPerBatchCount, // 推荐的每次数量
            strElementSetName,    // "F" strElementSetName,
            connection.PreferredRecordSyntax,  //this.comboBox_recordSyntax.Text),    //this.CurrentTargetInfo.PreferredRecordSyntax,
            true,   // 立即显示出来
            out records,
            out strError);
                        if (nRet == -1)
                        {
                            strError = "于 " + index.ToString()
                                + " 位置装入记录时出错：" + strError;
                            return -1;
                        }
                        if (records != null && records.Count > 0)
                        {
                            record = records[0];
                        }
                        else
                        {
                            Debug.Assert(false, "");
                        }
                    }
                    finally
                    {
                        connection.Stop.EndLoop();
                        connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                        connection.Stop.Initial("");

                        // EnableControls(true);
                        EnableQueryControl(true);
                    }
                }

            }

            byte[] baRecord = record.m_baRecord;    // Encoding.ASCII.GetBytes(record.m_strRecord);

            currrentEncoding = connection.GetRecordsEncoding(
                this.m_mainForm,
                record.m_strSyntaxOID);


            // 可能为XML格式
            if (record.m_strSyntaxOID == "1.2.840.10003.5.109.10")
            {

                // string strContent = Encoding.UTF8.GetString(baRecord);
                string strContent = currrentEncoding.GetString(baRecord);

                if (strStyle == "marc")
                {
                    // 看根节点的名字空间，如果符合MARCXML, 就先转换为USMARC，否则，就直接根据名字空间找样式表加以转换

                    string strNameSpaceUri = "";
                    nRet = GetRootNamespace(strContent,
                        out strNameSpaceUri,
                        out strError);
                    if (nRet == -1)
                    {
                        return -1;
                    }

                    if (strNameSpaceUri == Ns.usmarcxml)
                    {
                        string strOutMarcSyntax = "";

                        // 将MARCXML格式的xml记录转换为marc机内格式字符串
                        // parameters:
                        //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                        //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                        //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                        nRet = MarcUtil.Xml2Marc(strContent,
                            true,
                            "usmarc",
                            out strOutMarcSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                        {
                            return -1;
                        }

                        strOutStyle = "marc";
                        // currrentEncoding = connection.GetRecordsEncoding(this.MainForm, "1.2.840.10003.5.10");
                        return 0;
                    }
                }

                // 不是MARCXML格式
                // currrentEncoding = connection.GetRecordsEncoding(this.MainForm, record.m_strMarcSyntaxOID);
                strMARC = strContent;
                strOutStyle = "xml";
                return 0;
            }

            // SUTRS
            if (record.m_strSyntaxOID == "1.2.840.10003.5.101")
            {
                string strContent = currrentEncoding.GetString(baRecord);
                if (strStyle == "marc")
                {
                    // TODO: 按照回车草率转换为MARC
                    strMARC = strContent;

                    // strMarcSyntaxOID = "1.2.840.10003.5.10";
                    strOutStyle = "marc";
                    return 0;
                }

                // 不是MARCXML格式
                strMARC = strContent;
                strOutStyle = "xml";
                return 0;
            }

            // ISO2709转换为机内格式
            nRet = Marc8Encoding.ConvertByteArrayToMarcRecord(
                baRecord,
                connection.GetRecordsEncoding(this.m_mainForm, record.m_strSyntaxOID),  // Encoding.GetEncoding(936),
                true,
                out strMARC,
                out strError);
            if (nRet < 0)
            {
                return -1;
            }

            // 观察
            // connection.TargetInfo.UnionCatalogBindingDp2ServerUrl
            // 如果配置有绑定的dp2serverurl，则看看记录中有没有901字段，
            // 如果有，返还为strSavePath和baTimestamp
            if (connection.TargetInfo != null
                && String.IsNullOrEmpty(connection.TargetInfo.UnionCatalogBindingDp2ServerName) == false)
            {
                string strLocalPath = "";
                // 从MARC记录中得到901字段相关信息
                // return:
                //      -1  error
                //      0   not found field 901
                //      1   found field 901
                nRet = GetField901Info(strMARC,
                    out strLocalPath,
                    out baTimestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "MARC记录中未包含901字段，无法完成绑定操作。要具备901字段，请为dp2ZServer服务器的相关数据库增加addField901='true'属性。要避免此报错，也可在Z39.50服务器属性中去掉联合编目绑定定义";
                    return -1;
                }
                strSavePath = "dp2library:" + strLocalPath + "@" + connection.TargetInfo.UnionCatalogBindingDp2ServerName;
                logininfo.UserName = connection.TargetInfo.UserName;
                logininfo.Password = connection.TargetInfo.Password;
            }

            if (connection.TargetInfo != null
    && String.IsNullOrEmpty(connection.TargetInfo.UnionCatalogBindingUcServerUrl) == false)
            {
                string strLocalPath = "";
                // 从MARC记录中得到901字段相关信息
                // return:
                //      -1  error
                //      0   not found field 901
                //      1   found field 901
                nRet = GetField901Info(strMARC,
                    out strLocalPath,
                    out baTimestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "MARC记录中未包含901字段，无法完成绑定操作。要具备901字段，请为dp2ZServer服务器的相关数据库增加addField901='true'属性。要避免此报错，也可在Z39.50服务器属性中去掉联合编目绑定定义";
                    return -1;
                }
                strSavePath = "unioncatalog:" + strLocalPath + "@" + connection.TargetInfo.UnionCatalogBindingUcServerUrl;
                logininfo.UserName = connection.TargetInfo.UserName;
                logininfo.Password = connection.TargetInfo.Password;
            }


            currrentEncoding = connection.GetRecordsEncoding(this.m_mainForm, record.m_strSyntaxOID);
            strOutStyle = "marc";
            return 0;
        }

        // 从MARC记录中得到901字段相关信息
        // parameters:
        //      strPath [out]记录的本地路径。例如："中文图书/1"
        // return:
        //      -1  error
        //      0   not found field 901
        //      1   found field 901
        public static int GetField901Info(string strMARC,
            out string strPath,
            out byte [] baTimestamp,
            out string strError)
        {
            strPath = "";
            strError = "";
            baTimestamp = null;

            string strField = "";
            string strNextFieldName = "";
            // 从记录中得到一个字段
            // parameters:
            //		strMARC		机内格式MARC记录
            //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
            //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
            //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
            //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
            //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            int nRet = MarcUtil.GetField(strMARC,
                "901",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == -1)
            {
                strError = "GetField 901 error";
                return -1;
            }
            if (nRet == 0)
            {
                strError = "Field 901 not found";
                return 0;
            }

            string strSubfield = "";
            string strNextSubfieldName = "";
            		// 从字段或子字段组中得到一个子字段
		// parameters:
		//		strText		字段内容，或者子字段组内容。
		//		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
		//		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
		//					形式为'a'这样的。
		//		nIndex			想要获得同名子字段中的第几个。从0开始计算。
		//		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
		//		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
		// return:
		//		-1	出错
		//		0	所指定的子字段没有找到
		//		1	找到。找到的子字段返回在strSubfield参数中
            nRet = MarcUtil.GetSubfield(strField,
                DigitalPlatform.Marc.ItemType.Field,
                "p",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (nRet == -1)
            {
                strError = "GetSubfield p error";
                return -1;
            }

            if (strSubfield.Length > 1)
                strPath = strSubfield.Substring(1);

            nRet = MarcUtil.GetSubfield(strField,
                DigitalPlatform.Marc.ItemType.Field,
                "t",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (nRet == -1)
            {
                strError = "GetSubfield t error";
                return -1;
            }


            if (strSubfield.Length > 1)
            {
                string strHexTimestamp = "";
                strHexTimestamp = strSubfield.Substring(1);
                baTimestamp = ByteArray.GetTimeStampByteArray(strHexTimestamp);
            }

            return 1;
        }

        #endregion

        // 浏览窗上双击
        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            int nIndex = -1;

            // TODO: 如果记录为SUTRS格式，则只能装入XML详窗；
            // 如果记录为MARCXML，则两种窗口都可以装，优选装入MARC详窗

            if (this.listView_browse.SelectedIndices.Count > 0)
                nIndex = this.listView_browse.SelectedIndices[0];
            else
            {
                if (this.listView_browse.FocusedItem == null)
                    return;
                nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
                if (nIndex == -1)
                {
                    MessageBox.Show(this, "尚未选定要装入记录窗的事项");
                    return;
                }
            }

            LoadDetail(nIndex);
        }

        void menuItem_loadMarcDetail_Click(object sender, EventArgs e)
        {
            int index = -1;

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                MessageBox.Show(this, "当前ZConnection为空");
                return;
            }

            if (connection.VirtualItems.SelectedIndices.Count > 0)
                index = connection.VirtualItems.SelectedIndices[0];
            else
            {
                MessageBox.Show(this, "尚未选择要装入的记录");
                return;
            }
            

            DigitalPlatform.Z3950.Record record = null;
            if (index < connection.VirtualItems.Count)
            {
                record = (DigitalPlatform.Z3950.Record)
                    connection.VirtualItems[index].Tag;
                Debug.Assert(record != null, "");
            }
            else
            {
                MessageBox.Show(this, "index越界");
                return;
            }

            MarcDetailForm form = new MarcDetailForm();

            form.MdiParent = this.m_mainForm;
            form.MainForm = this.m_mainForm;


            // 继承自动识别的OID
            if (connection.TargetInfo != null
                && connection.TargetInfo.DetectMarcSyntax == true)
            {
                //form.AutoDetectedMarcSyntaxOID = record.AutoDetectedSyntaxOID;
                form.UseAutoDetectedMarcSyntaxOID = true;
            }
            form.Show();

            form.LoadRecord(this, index);
        }

        // 装入DC详窗
        void menuItem_loadDcDetail_Click(object sender, EventArgs e)
        {
            int index = -1;
            string strError = "";

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                goto ERROR1;
            }

            if (connection.VirtualItems.SelectedIndices.Count > 0)
                index = connection.VirtualItems.SelectedIndices[0];
            else
            {
                strError = "尚未选择要装入的记录";
                goto ERROR1;
            }

            DigitalPlatform.Z3950.Record record = null;
            if (index < connection.VirtualItems.Count)
            {
                record = (DigitalPlatform.Z3950.Record)
                    connection.VirtualItems[index].Tag;
            }
            else
            {
                strError = "index越界";
                goto ERROR1;
            }

            // XML格式或者SUTRS
            if (record.m_strSyntaxOID == "1.2.840.10003.5.109.10" // XML
                )
            {
                DcForm form = new DcForm();

                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;
                form.Show();

                form.LoadRecord(this, index);
                return;
            }
            else
            {
                strError = "记录不是XML格式";
                goto ERROR1;
            }

            // return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void menuItem_loadXmlDetail_Click(object sender, EventArgs e)
        {
            int index = -1;
            string strError = "";

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                goto ERROR1;
            }

            if (connection.VirtualItems.SelectedIndices.Count > 0)
                index = connection.VirtualItems.SelectedIndices[0];
            else
            {
                strError = "尚未选择要装入的记录";
                goto ERROR1;
            }

            DigitalPlatform.Z3950.Record record = null;
            if (index < connection.VirtualItems.Count)
            {
                record = (DigitalPlatform.Z3950.Record)
                    connection.VirtualItems[index].Tag;
            }
            else
            {
                strError = "index越界";
                goto ERROR1;
            }

            // XML格式或者SUTRS
            if (record.m_strSyntaxOID == "1.2.840.10003.5.109.10"
                || record.m_strSyntaxOID == "1.2.840.10003.5.101")
            {
                XmlDetailForm form = new XmlDetailForm();

                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;
                form.Show();

                form.LoadRecord(this, index);
                return;
            }
            else
            {
                strError = "记录不是XML格式或SUTRS格式";
                goto ERROR1;
            }

            // return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 自动根据情况，装载到MARC或者XML记录窗
        void LoadDetail(int index)
        {
            if (index < 0)
                throw new ArgumentException("index 值不应该小于 0","index");

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                MessageBox.Show(this, "当前ZConnection为空");
                return;
            }

            DigitalPlatform.Z3950.Record record = null;
            if (index < connection.VirtualItems.Count)
            {
                record = (DigitalPlatform.Z3950.Record)
                    connection.VirtualItems[index].Tag;
            }
            else
            {
                MessageBox.Show(this, "index越界");
                return;
            }

            if (record.m_nDiagCondition != 0)
            {
                MessageBox.Show(this, "这是一条诊断记录");
                return;
            }

            // 2014/5/18
            string strSyntaxOID = record.AutoDetectedSyntaxOID;
            if (string.IsNullOrEmpty(strSyntaxOID) == true)
                strSyntaxOID = record.m_strSyntaxOID;

            // XML格式或者SUTRS格式
            if (strSyntaxOID == "1.2.840.10003.5.109.10"
                || strSyntaxOID == "1.2.840.10003.5.101")
            {
                XmlDetailForm form = new XmlDetailForm();

                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;
                form.Show();

                form.LoadRecord(this, index);
                return;
            }

            {
                MarcDetailForm form = new MarcDetailForm();


                form.MdiParent = this.m_mainForm;
                form.MainForm = this.m_mainForm;

                // 继承自动识别的OID
                if (connection.TargetInfo != null
                    && connection.TargetInfo.DetectMarcSyntax == true)
                {
                    // form.AutoDetectedMarcSyntaxOID = record.AutoDetectedSyntaxOID;

                    form.UseAutoDetectedMarcSyntaxOID = true;
                }
                form.Show();

                form.LoadRecord(this, index);
            }
        }

        // 临时激活Stop显示
        public void ActivateStopDisplay()
        {
            ZConnection connection = this.GetCurrentZConnection();
            if (connection != null)
            {
                m_mainForm.stopManager.Active(connection.Stop);
            }
            else
            {
                m_mainForm.stopManager.Active(null);
            }
        }

        private void ZSearchForm_Activated(object sender, EventArgs e)
        {
            /*
            if (Stop != null)
                MainForm.stopManager.Active(this.Stop);
             * */
            ZConnection connection = null;

            try
            {
                connection = this.GetCurrentZConnection();
            }
            catch
            {
                return;
            }

            int nSelectedCount = 0;
            if (connection != null)
            {
                m_mainForm.stopManager.Active(connection.Stop);

                nSelectedCount = connection.VirtualItems.SelectedIndices.Count;
            }
            else
            {
                m_mainForm.stopManager.Active(null);
            }

            m_mainForm.SetMenuItemState();

            // 菜单
            if (nSelectedCount == 0)
            {
                m_mainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                m_mainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            }
            else
            {
                m_mainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                m_mainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;
            }


            m_mainForm.MenuItem_font.Enabled = false;

            // 工具条按钮
            if (nSelectedCount == 0)
            {
                m_mainForm.toolButton_saveTo.Enabled = false;
                m_mainForm.toolButton_loadFullRecord.Enabled = false;
            }
            else
            {
                m_mainForm.toolButton_saveTo.Enabled = true;
                m_mainForm.toolButton_loadFullRecord.Enabled = true;
            }

            m_mainForm.toolButton_save.Enabled = false;
            m_mainForm.toolButton_search.Enabled = true;
            m_mainForm.toolButton_prev.Enabled = false;
            m_mainForm.toolButton_next.Enabled = false;
            m_mainForm.toolButton_nextBatch.Enabled = true;

            m_mainForm.toolButton_getAllRecords.Enabled = true;

            m_mainForm.toolButton_delete.Enabled = false;

            m_mainForm.toolButton_loadTemplate.Enabled = false;

            m_mainForm.toolButton_dup.Enabled = false;
            m_mainForm.toolButton_verify.Enabled = false;
            m_mainForm.toolButton_refresh.Enabled = false;
        }


        // 浏览框上的右鼠标键Popup menu
        private void listView_browse_MouseUp(object sender,
            MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            /*
            ToolStripMenuItem subMenuItem = null;
            ToolStripSeparator menuSepItem = null;
             * */
            ToolStripSeparator sep = null;

            DigitalPlatform.Z3950.Record record = null;
            int index = -1;

            ZConnection connection = this.GetCurrentZConnection();

            int nSelectedCount = 0;
            if (connection != null)
            {
                // SaveListViewSelectedToVirtual(connection);

                nSelectedCount = connection.VirtualItems.SelectedIndices.Count;
            }

            if (nSelectedCount > 0)
            {
                index = connection.VirtualItems.SelectedIndices[0];
                record = (DigitalPlatform.Z3950.Record)
                 connection.VirtualItems[index].Tag;
            }


            // 装入MARC记录窗
            menuItem = new ToolStripMenuItem("装入MARC记录窗(&M)");
            menuItem.Click += new EventHandler(menuItem_loadMarcDetail_Click);
            if (record != null
                && (record.m_strSyntaxOID == "1.2.840.10003.5.1"
                || record.m_strSyntaxOID == "1.2.840.10003.5.10")
                )
            {
                menuItem.Enabled = true;
            }
            else if (record != null && record.m_strSyntaxOID == "1.2.840.10003.5.109.10")
            {
                // 还要细判断名字空间
                string strNameSpaceUri = "";
                string strContent = Encoding.UTF8.GetString(record.m_baRecord);
                string strError = "";
                int nRet = GetRootNamespace(strContent,
                    out strNameSpaceUri,
                    out strError);
                if (nRet != -1 && strNameSpaceUri == Ns.usmarcxml)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;
            }
            else
                menuItem.Enabled = false;

            contextMenu.Items.Add(menuItem);

            // 装入XML记录窗
            menuItem = new ToolStripMenuItem("装入XML记录窗(&X)");
            menuItem.Click += new EventHandler(menuItem_loadXmlDetail_Click);
            if (record != null 
                && 
                (record.m_strSyntaxOID == "1.2.840.10003.5.109.10"  // XML
                || record.m_strSyntaxOID == "1.2.840.10003.5.101")  // SUTRS
                )
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 装入DC记录窗
            menuItem = new ToolStripMenuItem("装入DC记录窗(&D)");
            menuItem.Click += new EventHandler(menuItem_loadDcDetail_Click);
            if (record != null
                &&
                (record.m_strSyntaxOID == "1.2.840.10003.5.109.10"  // XML
                )
                )
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 缺省编码方式
            menuItem = new ToolStripMenuItem("缺省编码方式");
            menuItem.Click += new EventHandler(menuItem_setDefualtEncoding_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 全选
            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(menuItem_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 装入Full元素集记录
            menuItem = new ToolStripMenuItem("重新装入完整格式记录 [" + nSelectedCount.ToString() +"] (&F)...");
            menuItem.Click += new System.EventHandler(this.menu_reloadFullElementSet_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 追加保存到数据库
            menuItem = new ToolStripMenuItem("以追加方式保存到数据库 ["+ nSelectedCount.ToString() +"] (&A)...");
            menuItem.Click += new System.EventHandler(this.menu_saveToDatabase_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // ---
            sep = new ToolStripSeparator();
            contextMenu.Items.Add(sep);

            // 保存原始记录到工作单文件
            menuItem = new ToolStripMenuItem("保存到工作单文件 [" + nSelectedCount.ToString()
                + "] (&W)");
            if (record != null
                && (record.m_strSyntaxOID == "1.2.840.10003.5.1"
                || record.m_strSyntaxOID == "1.2.840.10003.5.10"))
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToWorksheet_Click);
            contextMenu.Items.Add(menuItem);


            // 保存原始记录到ISO2709文件
            menuItem = new ToolStripMenuItem("保存到 MARC 文件 ["+ nSelectedCount.ToString()
                +"] (&S)");
            if (record != null
                && (record.m_strSyntaxOID == "1.2.840.10003.5.1"
                || record.m_strSyntaxOID == "1.2.840.10003.5.10"))
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_saveOriginRecordToIso2709_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.listView_browse, e.Location);
        }

        void menu_reloadFullElementSet_Click(object sender, EventArgs e)
        {
            ReloadFullElementSet();
        }

        // 为选定的行装入Full元素集的记录
        public void ReloadFullElementSet()
        {
            string strError = "";
            int nRet = 0;

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                goto ERROR1;
            }

            if (connection.VirtualItems.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要装入完整格式的浏览行";
                goto ERROR1;
            }


            DigitalPlatform.Stop stop = null;
            stop = new DigitalPlatform.Stop();
            stop.Register(m_mainForm.stopManager, true);	// 和容器关联

            stop.BeginLoop();

            this.EnableControls(false);
            try
            {
                List<int> selected = new List<int>();
                selected.AddRange(connection.VirtualItems.SelectedIndices);
                stop.SetProgressRange(0, selected.Count);

                for (int i = 0; i < selected.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    int index = selected[i];

                    stop.SetMessage("正在重新装载记录 "+(index+1).ToString()+" 的详细格式...");

                    byte[] baTimestamp = null;
                    string strSavePath = "";
                    string strOutStyle = "";
                    LoginInfo logininfo = null;
                    long lVersion = 0;
                    string strXmlFragment = "";
                    DigitalPlatform.Z3950.Record record = null;
                    Encoding currentEncoding = null;
                    string strMARC = "";

                    nRet = this.GetOneRecord(
                        "marc",
                        index,  // 即将废止
                        "index:" + index.ToString(),
                        "force_full", // false,
                        out strSavePath,
                        out strMARC,
                        out strXmlFragment,
                        out strOutStyle,
                        out baTimestamp,
                        out lVersion,
                        out record,
                        out currentEncoding,
                        out logininfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    stop.SetProgressValue(i);

                }

                return;
            }
            finally
            {
                stop.EndLoop();
                stop.SetMessage("");
                stop.Unregister();	// 和容器关联
                stop = null;

                this.EnableControls(true);
            }

    // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 选定的行中是否包含了Brief格式的记录
        bool HasSelectionContainBriefRecords()
        {
            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                return false;
            }
            for (int i = 0; i < connection.VirtualItems.SelectedIndices.Count; i++)
            {
                int index = connection.VirtualItems.SelectedIndices[i];
                DigitalPlatform.Z3950.Record record = (DigitalPlatform.Z3950.Record)
    connection.VirtualItems[index].Tag;

                if (record == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (record.m_strElementSetName == "B")
                    return true;
            }

            return false;
        }

        // 追加保存到数据库
        void menu_saveToDatabase_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strLastSavePath = m_mainForm.LastSavePath;
            if (String.IsNullOrEmpty(strLastSavePath) == false)
            {
                string strOutputPath = "";
                nRet = MarcDetailForm.ChangePathToAppendStyle(strLastSavePath,
                    out strOutputPath,
                    out strError);
                if (nRet == -1)
                {
                    m_mainForm.LastSavePath = ""; // 避免下次继续出错
                    goto ERROR1;
                }
                strLastSavePath = strOutputPath;
            }


            SaveRecordDlg dlg = new SaveRecordDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.SaveToDbMode = true;    // 不允许在textbox中修改路径

            dlg.MainForm = this.m_mainForm;
            dlg.GetDtlpSearchParam += new GetDtlpSearchParamEventHandle(dlg_GetDtlpSearchParam);
            dlg.GetDp2SearchParam += new GetDp2SearchParamEventHandle(dlg_GetDp2SearchParam);
            {
                dlg.RecPath = strLastSavePath;
                dlg.Text = "请选择目标数据库";
            }
            //dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "SaveRecordDlg_state");
            dlg.UiState = this.MainForm.AppInfo.GetString("ZSearchForm", "SaveRecordDlg_uiState", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString("ZSearchForm", "SaveRecordDlg_uiState", dlg.UiState);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            m_mainForm.LastSavePath = dlg.RecPath;

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(dlg.RecPath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                goto ERROR1;
            }

            if (connection.VirtualItems.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要保存记录的浏览行";
                goto ERROR1;
            }

            bool bForceFull = false;

            if (HasSelectionContainBriefRecords() == true)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有Brief(简要)格式的记录，是否在保存前重新获取为Full(完整)格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"ZSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                    bForceFull = true;
            }

            // TODO: 禁止问号以外的其它ID
            DigitalPlatform.Stop stop = null;
            stop = new DigitalPlatform.Stop();
            stop.Register(m_mainForm.stopManager, true);	// 和容器关联

            stop.BeginLoop();

            this.EnableControls(false);
            try
            {

                // dtlp协议的记录保存
                if (strProtocol.ToLower() == "dtlp")
                {
                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    for (int i = 0; i < connection.VirtualItems.SelectedIndices.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                        }

                        int index = connection.VirtualItems.SelectedIndices[i];

                        byte[] baTimestamp = null;
                        string strSavePath = "";
                        string strOutStyle = "";
                        LoginInfo logininfo = null;
                        long lVersion = 0;
                        string strXmlFragment = "";
                        DigitalPlatform.Z3950.Record record = null;
                        Encoding currentEncoding = null;
                        string strMARC = "";

                        nRet = this.GetOneRecord(
                            "marc",
                            index, // 即将废止
                            "index:" + index.ToString(),
                            bForceFull == true ? "force_full" : "", // false,
                            out strSavePath,
                            out strMARC,
                            out strXmlFragment,
                            out strOutStyle,
                            out baTimestamp,
                            out lVersion,
                            out record,
                            out currentEncoding,
                            out logininfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;


                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        // TODO: 有些格式不适合保存到目标数据库

                        byte[] baOutputTimestamp = null;
                        string strOutputPath = "";
                        nRet = dtlp_searchform.SaveMarcRecord(
                            strPath,
                            strMARC,
                            baTimestamp,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    MessageBox.Show(this, "保存成功");
                    return;
                }
                else if (strProtocol.ToLower() == "dp2library")
                {
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    string strDp2ServerName = "";
                    string strPurePath = "";
                    // 解析记录路径。
                    // 记录路径为如下形态 "中文图书/1 @服务器"
                    dp2SearchForm.ParseRecPath(strPath,
                        out strDp2ServerName,
                        out strPurePath);

                    string strTargetMarcSyntax = "";

                    try
                    {
                        NormalDbProperty prop = dp2_searchform.GetDbProperty(strDp2ServerName,
             dp2SearchForm.GetDbName(strPurePath));
                        strTargetMarcSyntax = prop.Syntax;
                        if (string.IsNullOrEmpty(strTargetMarcSyntax) == true)
                            strTargetMarcSyntax = "unimarc";
                    }
                    catch (Exception ex)
                    {
                        strError = "在获得目标库特性时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    bool bSkip = false;
                    int nSavedCount = 0;

                    for (int i = 0; i < connection.VirtualItems.SelectedIndices.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null)
                        {
                            if (stop.State != 0)
                            {
                                strError = "用户中断";
                                goto ERROR1;
                            }
                        }

                        int index = connection.VirtualItems.SelectedIndices[i];

                        byte[] baTimestamp = null;
                        string strSavePath = "";
                        string strOutStyle = "";
                        LoginInfo logininfo = null;
                        long lVersion = 0;
                        string strXmlFragment = "";
                        DigitalPlatform.Z3950.Record record = null;
                        Encoding currentEncoding = null;
                        string strMARC = "";

                        nRet = this.GetOneRecord(
                            "marc",
                            index,  // 即将废止
                            "index:" + index.ToString(),
                            bForceFull == true ? "force_full" : "", // false,
                            out strSavePath,
                            out strMARC,
                            out strXmlFragment,
                            out strOutStyle,
                            out baTimestamp,
                            out lVersion,
                            out record,
                            out currentEncoding,
                            out logininfo,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

#if NO
                        string strMarcSyntax = "";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";
#endif
                        // 2014/5/12
                        string strMarcSyntax = MarcDetailForm.GetMarcSyntax(record.m_strSyntaxOID);

                        // 有些格式不适合保存到目标数据库
                        if (strTargetMarcSyntax != strMarcSyntax)
                        {
                            if (bSkip == true)
                                continue;
                            strError = "记录 "+(index+1).ToString()+" 的格式类型为 '"+strMarcSyntax+"'，和目标库的格式类型 '"+strTargetMarcSyntax+"' 不符合，因此无法保存到目标库";
                            DialogResult result = MessageBox.Show(this,
        strError + "\r\n\r\n要跳过这些记录而继续保存后面的记录么?\r\n\r\n(Yes: 跳过格式不吻合的记录，继续保存后面的; No: 放弃整个保存操作)",
        "ZSearchForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto ERROR1;
                            bSkip = true;
                            continue;
                        }

                        string strProtocolPath = this.CurrentProtocol + ":"
    + this.CurrentResultsetPath
    + "/" + (index + 1).ToString();

                        string strOutputPath = "";
                        byte[] baOutputTimestamp = null;
                        string strComment = "copy from " + strProtocolPath; // strSavePath;
                        // return:
                        //      -2  timestamp mismatch
                        //      -1  error
                        //      0   succeed
                        nRet = dp2_searchform.SaveMarcRecord(
                            false,
                            strPath,
                            strMARC,
                            strMarcSyntax,
                            baTimestamp,
                            strXmlFragment,
                            strComment,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        nSavedCount++;

                    }
                    MessageBox.Show(this, "共保存记录 "+nSavedCount.ToString()+" 条");
                    return;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持Z39.50协议的保存操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }

            }
            finally
            {
                stop.EndLoop();

                stop.Unregister();	// 和容器关联
                stop = null;

                this.EnableControls(true);
            }

            // return 0;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = null;

            dtlp_searchform = this.m_mainForm.TopDtlpSearchForm;

            if (dtlp_searchform == null)
            {
                // 新开一个dtlp检索窗
                FormWindowState old_state = this.WindowState;

                dtlp_searchform = new DtlpSearchForm();
                dtlp_searchform.MainForm = this.m_mainForm;
                dtlp_searchform.MdiParent = this.m_mainForm;
                dtlp_searchform.WindowState = FormWindowState.Minimized;
                dtlp_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dtlp_searchform.WaitLoadFinish();
            }

            return dtlp_searchform;
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;


            dp2_searchform = this.m_mainForm.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // 新开一个dp2检索窗
                FormWindowState old_state = this.WindowState;

                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this.m_mainForm;
                dp2_searchform.MdiParent = this.m_mainForm;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // 需要等待初始化操作彻底完成
                dp2_searchform.WaitLoadFinish();
            }

            return dp2_searchform;
        }

        void dlg_GetDp2SearchParam(object sender, GetDp2SearchParamEventArgs e)
        {
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            e.dp2Channels = dp2_searchform.Channels;
            e.MainForm = this.m_mainForm;
        }

        void dlg_GetDtlpSearchParam(object sender, GetDtlpSearchParamEventArgs e)
        {
            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            e.DtlpChannels = dtlp_searchform.DtlpChannels;
            e.DtlpChannel = dtlp_searchform.DtlpChannel;
        }

        void menuItem_selectAll_Click(object sender,
            EventArgs e)
        {
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                this.listView_browse.Items[i].Selected = true;
            }
        }

        // 变换ISO2709记录的编码方式
        public static int ChangeIso2709Encoding(
            Encoding sourceEncoding,
            byte [] baSource,
            Encoding targetEncoding,
            string strMarcSyntax,
            out byte [] baTarget,
            out string strError)
        {
            baTarget = null;
            strError = "";

            string strMARC = "";
            // 把byte[]类型的MARC记录转换为机内格式
            // return:
            //		-2	MARC格式错
            //		-1	一般错误
            //		0	正常
            int nRet = MarcUtil.ConvertByteArrayToMarcRecord(
                baSource,
                sourceEncoding,
                true,   // bool bForce,
                out strMARC,
                out strError);
            if (nRet == -1 || nRet == -2)
                return -1;


            // 将MARC机内格式转换为ISO2709格式
            // parameters:
            //		strMarcSyntax   "unimarc" "usmarc"
            //		strSourceMARC		[in]机内格式MARC记录。
            //		targetEncoding	[in]输出ISO2709的编码方式为 UTF8 codepage-936等等
            //		baResult	[out]输出的ISO2709记录。字符集受nCharset参数控制。
            //					注意，缓冲区末尾不包含0字符。
            nRet = MarcUtil.CvtJineiToISO2709(
                strMARC,
                strMarcSyntax,
                targetEncoding,
                out baTarget,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        public void menuItem_saveOriginRecordToWorksheet_Click(object sender,
    EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                goto ERROR1;
            }

            if (connection.VirtualItems.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要保存记录的浏览行";
                goto ERROR1;
            }

            bool bForceFull = false;

            if (HasSelectionContainBriefRecords() == true)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有Brief(简要)格式的记录，是否在保存前重新获取为Full(完整)格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"ZSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                    bForceFull = true;
            }

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的工作单文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = m_mainForm.LastWorksheetFileName;
            dlg.Filter = "工作单文件 (*.wor)|*.wor|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;


            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "ZSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }


            m_mainForm.LastWorksheetFileName = dlg.FileName;

            StreamWriter sw = null;

            try
            {
                // 创建文件
                sw = new StreamWriter(m_mainForm.LastWorksheetFileName,
                    bAppend,	// append
                    System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + m_mainForm.LastWorksheetFileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            Encoding preferredEncoding = Encoding.UTF8;

            try
            {
                for (int i = 0; i < connection.VirtualItems.SelectedIndices.Count; i++)
                {
                    int index = connection.VirtualItems.SelectedIndices[i];

                    byte[] baTimestamp = null;
                    string strSavePath = "";
                    string strOutStyle = "";
                    LoginInfo logininfo = null;
                    long lVersion = 0;
                    string strXmlFragment = "";
                    DigitalPlatform.Z3950.Record record = null;
                    Encoding currentEncoding = null;
                    string strMARC = "";

                    nRet = this.GetOneRecord(
                        "marc",
                        index,  // 即将废止
                        "index:" + index.ToString(),
                        bForceFull == true ? "force_full" : "", // false,
                        out strSavePath,
                        out strMARC,
                        out strXmlFragment,
                        out strOutStyle,
                        out baTimestamp,
                        out lVersion,
                        out record,
                        out currentEncoding,
                        out logininfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;


                    string strMarcSyntax = "";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                        strMarcSyntax = "unimarc";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                        strMarcSyntax = "usmarc";

                    // TODO: 有些格式不适合保存到工作单文件

                    List<string> lines = null;
                    // 将机内格式变换为工作单格式
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcUtil.CvtJineiToWorksheet(
                        strMARC,
                        -1,
                        out lines,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }

                }

                // 
                if (bAppend == true)
                    m_mainForm.MessageText = connection.VirtualItems.SelectedIndices.Count.ToString()
                        + "条记录成功追加到文件 " + m_mainForm.LastWorksheetFileName + " 尾部";
                else
                    m_mainForm.MessageText = connection.VirtualItems.SelectedIndices.Count.ToString()
                        + "条记录成功保存到新文件 " + m_mainForm.LastWorksheetFileName + " 尾部";

            }
            catch (Exception ex)
            {
                strError = "写入文件 " + m_mainForm.LastWorksheetFileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                sw.Close();
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public void menuItem_saveOriginRecordToIso2709_Click(object sender,
            EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
            {
                strError = "当前ZConnection为空";
                goto ERROR1;
            }

            if (connection.VirtualItems.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要保存记录的浏览行";
                goto ERROR1;
            }

            bool bForceFull = false;

            if (HasSelectionContainBriefRecords() == true)
            {
                DialogResult result = MessageBox.Show(this,
"即将保存的记录中有Brief(简要)格式的记录，是否在保存前重新获取为Full(完整)格式的记录?\r\n\r\n(Yes: 是，要完整格式的记录; No: 否，依然保存简明格式的记录； Cancel: 取消，放弃整个保存操作",
"ZSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                    bForceFull = true;
            }


            Encoding preferredEncoding = null;
            

            // string strPreferedMarcSyntax = "";

            {
                // 观察要保存的第一条记录的marc syntax
                int first_index = connection.VirtualItems.SelectedIndices[0];
                VirtualItem first_item = connection.VirtualItems[first_index];
                DigitalPlatform.Z3950.Record first_record = (DigitalPlatform.Z3950.Record)first_item.Tag;

                /*
                if (first_record.m_strMarcSyntaxOID == "1.2.840.10003.5.1")
                    strPreferedMarcSyntax = "unimarc";
                if (first_record.m_strMarcSyntaxOID == "1.2.840.10003.5.10")
                    strPreferedMarcSyntax = "usmarc";
                 * */

                preferredEncoding = connection.GetRecordsEncoding(
                    this.m_mainForm,
                    first_record.m_strSyntaxOID);

            }


            /*
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = MainForm.LastIso2709FileName;
            dlg.RestoreDirectory = true;
            dlg.CreatePrompt = true;
            dlg.OverwritePrompt = false;
            dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "iso2709 files (*.mrc)|*.mrc|All files (*.*)|*.*";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            MainForm.LastIso2709FileName = dlg.FileName;
             * */

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = true;
            dlg.GetEncoding -= new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.GetEncoding += new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.FileName = m_mainForm.LastIso2709FileName;
            dlg.CrLf = m_mainForm.LastCrLfIso2709;
            dlg.RemoveField998Visible = false;
            dlg.Mode880Visible = false; // 暂时不支持 880 模式转换
            dlg.EncodingListItems = Global.GetEncodingList(true);
            dlg.EncodingName =
                (String.IsNullOrEmpty(m_mainForm.LastEncodingName) == true ? GetEncodingForm.GetEncodingName(preferredEncoding) : m_mainForm.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            Encoding targetEncoding = null;

            if (dlg.EncodingName == "MARC-8"
                && preferredEncoding.Equals(this.m_mainForm.Marc8Encoding) == false)
            {
                strError = "保存操作无法进行。只有在记录的原始编码方式为 MARC-8 时，才能使用这个编码方式保存记录。";
                goto ERROR1;
            }

            nRet = this.m_mainForm.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // targetEncoding = dlg.Encoding;

            /*
            strPreferedMarcSyntax = dlg.MarcSyntax;
            if (strPreferedMarcSyntax == "<自动>")
                strPreferedMarcSyntax = "";
             * */

            string strLastFileName = m_mainForm.LastIso2709FileName;
            string strLastEncodingName = m_mainForm.LastEncodingName;


            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "ZSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性
            if (strLastFileName == dlg.FileName
                && bAppend == true)
            {
                if (strLastEncodingName != ""
                    && strLastEncodingName != dlg.EncodingName)
                {
                    DialogResult result = MessageBox.Show(this,
                        "文件 '" + dlg.FileName + "' 已在先前已经用 " + strLastEncodingName + " 编码方式存储了记录，现在又以不同的编码方式 " + dlg.EncodingName + " 追加记录，这样会造成同一文件中存在不同编码方式的记录，可能会令它无法被正确读取。\r\n\r\n是否继续? (是)追加  (否)放弃操作",
                        "ZSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃处理...";
                        goto ERROR1;
                    }

                }
            }

            m_mainForm.LastIso2709FileName = dlg.FileName;
            m_mainForm.LastCrLfIso2709 = dlg.CrLf;
            m_mainForm.LastEncodingName = dlg.EncodingName;

            Stream s = null;

            try
            {
                s = File.Open(m_mainForm.LastIso2709FileName,
                     FileMode.OpenOrCreate);
                if (bAppend == false)
                    s.SetLength(0);
                else
                    s.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + m_mainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                for (int i = 0; i < connection.VirtualItems.SelectedIndices.Count; i++)
                {
                    int index = connection.VirtualItems.SelectedIndices[i];

                    /*
                    VirtualItem item = connection.VirtualItems[index];

                    DigitalPlatform.Z3950.Record record = (DigitalPlatform.Z3950.Record)item.Tag;
                    */
                    byte[] baTimestamp = null;
                    string strSavePath = "";
                    string strOutStyle = "";
                    LoginInfo logininfo = null;
                    long lVersion = 0;
                    string strXmlFragment = "";
                    DigitalPlatform.Z3950.Record record = null;
                    Encoding currentEncoding = null;
                    string strMARC = "";

                    nRet = this.GetOneRecord(
                        "marc",
                        index,  // 即将废止
                        "index:" + index.ToString(),
                        bForceFull == true ? "force_full" : "", // false,
                        out strSavePath,
                        out strMARC,
                        out strXmlFragment,
                        out strOutStyle,
                        out baTimestamp,
                        out lVersion,
                        out record,
                        out currentEncoding,
                        out logininfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    byte[] baTarget = null;

                    Encoding sourceEncoding = connection.GetRecordsEncoding(
                        this.m_mainForm,
                        record.m_strSyntaxOID);

                    string strMarcSyntax = "";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.1")
                        strMarcSyntax = "unimarc";
                    if (record.m_strSyntaxOID == "1.2.840.10003.5.10")
                        strMarcSyntax = "usmarc";

                    if (sourceEncoding.Equals(targetEncoding) == true)
                    {
                        // source和target编码方式相同，不用转换
                        // baTarget = record.m_baRecord;

                        // 规范化 ISO2709 物理记录
                        // 主要是检查里面的记录结束符是否正确，去掉多余的记录结束符
                        baTarget = MarcUtil.CononicalizeIso2709Bytes(targetEncoding,
                            record.m_baRecord);
                    }
                    else
                    {
                        nRet = ChangeIso2709Encoding(
                            sourceEncoding,
                            record.m_baRecord,
                            targetEncoding,
                            strMarcSyntax,
                            out baTarget,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    s.Write(baTarget, 0,
                        baTarget.Length);

                    if (dlg.CrLf == true)
                    {
                        byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                        s.Write(baCrLf, 0,
                            baCrLf.Length);
                    }
                }

                // 
                if (bAppend == true)
                    m_mainForm.MessageText = connection.VirtualItems.SelectedIndices.Count.ToString() 
                        + "条记录成功追加到文件 " + m_mainForm.LastIso2709FileName + " 尾部";
                else
                    m_mainForm.MessageText = connection.VirtualItems.SelectedIndices.Count.ToString()
                        + "条记录成功保存到新文件 " + m_mainForm.LastIso2709FileName + " 尾部";

            }
            catch (Exception ex)
            {
                strError = "写入文件 " + m_mainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                s.Close();
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void dlg_GetEncoding(object sender, GetEncodingEventArgs e)
        {
            string strError = "";
            Encoding encoding = null;
            int nRet = this.m_mainForm.GetEncoding(e.EncodingName,
                out encoding,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }
            e.Encoding = encoding;
        }


        void menuItem_setDefualtEncoding_Click(object sender,
            EventArgs e)
        {
            ZConnection connection = this.GetCurrentZConnection();

            GetEncodingForm dlg = new GetEncodingForm();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.Encoding = connection.ForcedRecordsEncoding;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            connection.ForcedRecordsEncoding = dlg.Encoding;

            // 刷新listview内的全部行
            RefreshBrowseViewDisplay();
        }

        // 探测MARC记录从属的格式：
        // return:
        //		-1	无法探测
        //		1	UNIMARC	规则：包含200字段
        //		10	USMARC	规则：包含008字段(innopac的UNIMARC格式也有一个奇怪的008)
        public static int DetectMARCSyntax(string strMARC)
        {
            int nRet = 0;

            if (String.IsNullOrEmpty(strMARC) == true)
                return -1;

            string strField = "";
            string strNextFieldName = "";

            nRet = MarcUtil.GetField(strMARC,
                "200",
                0,
                out strField,
                out strNextFieldName);
            if (nRet != -1 && nRet != 0)
                return 1;	// UNIMARC

            nRet = MarcUtil.GetField(strMARC,
                "008",
                0,
                out strField,
                out strNextFieldName);
            if (nRet != -1 && nRet != -1)
                return 10;	// USMARC

            return -1;
        }

        int RefreshBrowseViewDisplay()
        {
            ZConnection connection = this.GetCurrentZConnection();

            for (int i = 0; i < connection.VirtualItems.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                /*
                if (stop != null)
                {
                    if (stop.State != 0)
                        break;
                }
                 * */


                DigitalPlatform.Z3950.Record record = (DigitalPlatform.Z3950.Record)
                    connection.VirtualItems[i].Tag;

                string strError = "";
                string strBrowseText = "";
                int nRet = 0;

                /*

                string strMARC = "";
                byte[] baRecord = record.m_baRecord;    //Encoding.ASCII.GetBytes(record.m_strRecord);
                string strMarcSyntaxOID = record.m_strMarcSyntaxOID;
                // ISO2709转换为机内格式
                nRet = Marc8Encoding.ConvertByteArrayToMarcRecord(
                    baRecord,
                    connection.GetRecordsEncoding(this.MainForm, strMarcSyntaxOID),  // Encoding.GetEncoding(936),
                    true,
                    out strMARC,
                    out strError);
                if (nRet < 0)
                {
                    strBrowseText = strError;
                    goto DOREFRESH;
                }


                if (connection.TargetInfo.DetectMarcSyntax == true)
                {
                    // 探测MARC记录从属的格式：
                    // return:
                    //		-1	无法探测
                    //		1	UNIMARC	规则：包含200字段
                    //		10	USMARC	规则：包含008字段(innopac的UNIMARC格式也有一个奇怪的008)
                    nRet = DetectMARCSyntax(strMARC);
                    if (nRet == 1)
                        strMarcSyntaxOID = "1.2.840.10003.5.1";
                    else if (nRet == 10)
                        strMarcSyntaxOID = "1.2.840.10003.5.10";


                    // 把自动识别的结果保存下来
                    record.AutoDetectedMarcSyntaxOID = strMarcSyntaxOID;
                }


                nRet = BuildMarcBrowseText(
                    strMarcSyntaxOID,
                    strMARC,
                    out strBrowseText,
                    out strError);
                if (nRet == -1)
                    strBrowseText = strError;
                 * */
                int nImageIndex = 0;

                nRet = BuildBrowseText(
                    connection,
                    record,
                    "marc", // 偏向MARC
                    out strBrowseText,
                    out nImageIndex,
                    out strError);
                if (nRet == -1)
                    strBrowseText = strError;


            // DOREFRESH:

                VirtualItem item = connection.VirtualItems[i];

                string[] cols = strBrowseText.Split(new char[] { '\t' });
                for (int j = 0; j < cols.Length; j++)
                {
                    item.SubItems[j+1] = cols[j];
                }

                item.ImageIndex = nImageIndex;

            }

            this.listView_browse.Invalidate();
            return 0;
        }



        int GetBrowseListViewSelectedItemCount()
        {
            ZConnection connection = this.GetCurrentZConnection();
            if (connection == null)
                return 0;
            return connection.VirtualItems.SelectedIndices.Count;
        }


        // 捕获键盘的输入
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 回车
            if (keyData == Keys.Enter)
            {
                if (this.queryControl1.Focused == true)
                {
                    // 检索词那里回车
                    // this.DoSearchOneServer();
                    this.DoSearch();
                }
                else if (this.listView_browse.Focused == true)
                {
                    // 浏览框中回车
                    listView_browse_DoubleClick(this, null);
                }

                return true;
            }

            return false;
        }

        // 获得根节点的名字空间
        public static int GetRootNamespace(string strXml,
            out string strNameSpaceUri,
            out string strError)
        {
            strNameSpaceUri = "";
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装载到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            XmlNode root = dom.DocumentElement;

            strNameSpaceUri = root.NamespaceURI;
            return 0;
        }

        void InitialPresentFormat(TargetInfo targetInfo)
        {
            if (targetInfo == null)
            {
                this.comboBox_elementSetName.Text = "";
                this.comboBox_recordSyntax.Text = "";
            }
            else
            {
                this.comboBox_elementSetName.Text = targetInfo.DefaultElementSetName;
                this.comboBox_recordSyntax.Text = targetInfo.PreferredRecordSyntax;
            }
        }

        // 目标树上选择即将发生改变
        private void zTargetControl1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                ZConnection connection = this.GetCurrentZConnection();

                // 保存检索式控件内容
                if (connection != null)
                {
                    // 保存检索式
                    connection.QueryXml = this.queryControl1.GetContent(true);
                    // 保存记录语法
                    connection.RecordSyntax = this.comboBox_recordSyntax.Text;
                    // 保存元素集名
                    connection.ElementSetName = this.comboBox_elementSetName.Text;

                    // 保存listview中的选定事项
                    SaveListViewSelectedToVirtual(connection);
                }
            }
            catch
            {
            }
        }

        void SaveListViewSelectedToVirtual(ZConnection connection)
        {
            // 保存listview中的选定事项
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];

                connection.VirtualItems[i].Selected = item.Selected;
            }
        }

        // 目标树上选择已经发生改变
        private void zTargetControl1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // string strError = "";

            ZConnection connection = null;

            try
            {
                connection = this.GetCurrentZConnection();
            }
            catch
            {
                return;
            }

            // 激活当前Stop对象
            m_mainForm.stopManager.Active(connection == null ? null : connection.Stop);

            /*
            if (connection.TargetInfo == null)
            {
                TargetInfo targetinfo = null;
                int nRet = this.zTargetControl1.GetCurrentTarget(
                    out targetinfo,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                connection.TargetInfo = targetinfo;
            }
             * */

            // Debug.Assert(connection.TargetInfo != null, "");

            // 初始化present format面板显示
            InitialPresentFormat(connection == null ? null : connection.TargetInfo);

            // 设置检索式控件内容
            this.queryControl1.SetContent(connection == null ? null : connection.QueryXml);

            // 设置记录语法
            if (String.IsNullOrEmpty(connection.RecordSyntax) == false)
                this.comboBox_recordSyntax.Text = connection.RecordSyntax;

            // 设置元素集名
            if (String.IsNullOrEmpty(connection.ElementSetName) == false)
                this.comboBox_elementSetName.Text = connection.ElementSetName;


            // 决定检索式控件Enabled状态
            EnableQueryControl(connection == null ? false : connection.Enabled);

            /*
            // 初始化浏览显示
            this.listView_browse.Items.Clear();
             * */

            LinkRecordsToListView(connection == null ? null : connection.VirtualItems);
            /*
            if (connection != null
                && connection.Records != null
                && connection.Records.Count != 0)
            {
                Debug.Assert(connection.Stop.State != 0, "不能在stop表示正在处理的时候，又使用同一个stop表示新的循环");
                // TODO: 建议在出现这种尴尬情况的时候，在listview的第一行放一条错误信息。这样用户可以用来回切换的方法，重新填满listview

                connection.Stop.OnStop += new StopEventHandler(this.DoStop);
                connection.Stop.SetMessage("重新装入浏览信息 ...");
                connection.Stop.BeginLoop();

                // EnableControls(false);

                try
                {

                    int nRet = FillRecordsToBrowseView(
                        connection.Stop,
                        connection,
                        connection.Records,
                        0,
                        connection.Records.Count,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    connection.Stop.EndLoop();
                    connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                    connection.Stop.Initial("");

                    // EnableControls(true);
                }
            }
             * */

            // this.textBox_resultInfo.Text = (connection == null ? "" : connection.ErrorInfo);
            this.textBox_resultInfo.Text = "";
            if (connection != null)
            {
                if (String.IsNullOrEmpty(connection.ErrorInfo) == false)
                    this.textBox_resultInfo.Text = connection.ErrorInfo;
                else
                {
                    if (connection.ResultCount >= 0)
                        this.textBox_resultInfo.Text = "命中结果数: " + connection.ResultCount.ToString();
                }
            }


            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
            return;
             * */
        }

        // 把虚拟集合事项连接到当前listview上
        // 注意：listview是公用的，所以应当连接当前ZConnection的虚拟集合，不要弄错了
        void LinkRecordsToListView(VirtualItemCollection items)
        {
            this.CurrentBrowseItems = items;
            items.ExpireSelectedIndices();

            if (this.CurrentBrowseItems != null)
                this.listView_browse.VirtualListSize = this.CurrentBrowseItems.Count;
            else
                this.listView_browse.VirtualListSize = 0;

            // 恢复Selected状态
            if (items != null)
            {
                for (int i = 0; i < items.SelectedIndices.Count; i++)
                {
                    int index = items.SelectedIndices[i];
                    this.listView_browse.Items[index].Selected = true;
                }
            }

            // 迫使刷新
            this.listView_browse.Invalidate();
        }


        /*
        void LinkRecordsToListView(RecordCollection records)
        {
            this.CurrentRecords = records;

            if (this.CurrentRecords != null)
                this.listView_browse.VirtualListSize = this.CurrentRecords.Count;
            else
                this.listView_browse.VirtualListSize = 0;

            // 迫使刷新
            this.listView_browse.Invalidate();
        }*/

#if NOOOOOOOOOOOOOOOOOOOOO
        // 把存储在records结构中的信息填入listview
        // parameters:
        int FillRecordsToBrowseView(
            Stop stop,
            ZConnection connection,
            RecordCollection records,
            int nStart,
            int nCount,
            out string strError)
        {
            strError = "";

            if (records == null)
                return 0;

            if (nStart + nCount > records.Count)
            {
                strError = "nStart["+nStart.ToString()+"]和nCount["+nCount.ToString()+"]参数之和超出records集合的尺寸["+records.Count.ToString()+"]";
                return -1;
            }

            for (int i = nStart; i < nStart + nCount; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                        // TODO: 中断后怎么办？似最后一条记录不代表Records中的最后了
                    }
                }

                DigitalPlatform.Z3950.Record record = records[i];

                ListViewItem item = new ListViewItem(
                    (nStart + i + 1).ToString(),
                    record.m_nDiagCondition == 0 ? BROWSE_TYPE_NORMAL : BROWSE_TYPE_DIAG);

                string strBrowseText = "";

                int nRet = 0;
                string[] cols = null;

                if (record.m_nDiagCondition != 0)
                {
                    strBrowseText = "诊断记录 condition=" + record.m_nDiagCondition.ToString() + "; addinfo=\"" + record.m_strAddInfo + "\"; diagSetOID=" + record.m_strDiagSetID;
                    goto DOADD;
                }
                else
                {
                    byte[] baRecord = record.m_baRecord;    //Encoding.ASCII.GetBytes(record.m_strRecord);

                    string strMARC = "";
                    string strMarcSyntaxOID = "";

                    // 可能为XML格式
                    if (record.m_strMarcSyntaxOID == "1.2.840.10003.5.109.10")
                    {
                        // 看根节点的名字空间，如果符合MARCXML, 就先转换为USMARC，否则，就直接根据名字空间找样式表加以转换
                        string strContent = Encoding.UTF8.GetString(baRecord);

                        string strNameSpaceUri = "";
                        nRet = GetRootNamespace(strContent,
                            out strNameSpaceUri,
                            out strError);
                        if (nRet == -1)
                        {
                            strBrowseText = strError;
                            goto DOADD;
                        }

                        if (strNameSpaceUri == Ns.usmarcxml)
                        {
                            string strOutMarcSyntax = "";
                            // 将MARCXML格式的xml记录转换为marc机内格式字符串
                            // parameters:
                            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                            nRet = MarcUtil.Xml2Marc(strContent,
                                true,
                                "usmarc",
                                out strOutMarcSyntax,
                                out strMARC,
                                out strError);
                            if (nRet == -1)
                            {
                                strBrowseText = strError;
                                goto DOADD;
                            }

                            strMarcSyntaxOID = "1.2.840.10003.5.10";

                            nRet = GetBrowseText(
                                strMarcSyntaxOID,
                                strMARC,
                                out strBrowseText,
                                out strError);
                            if (nRet == -1)
                                strBrowseText = strError;

                            goto DOADD;

                        }

                        cols = new string[1];
                        cols[0] = strContent;
                        goto DOADDCOLS;
                    }

                    strMarcSyntaxOID = record.m_strMarcSyntaxOID;

                    // ISO2709转换为机内格式
                    nRet = Marc8Encoding.ConvertByteArrayToMarcRecord(
                        baRecord,
                        connection.GetRecordsEncoding(this.MainForm, strMarcSyntaxOID),  // Encoding.GetEncoding(936),
                        true,
                        out strMARC,
                        out strError);
                    if (nRet < 0)
                    {
                        strBrowseText = strError;
                        goto DOADD;
                    }


                    if (connection.TargetInfo.DetectMarcSyntax == true)
                    {
                        // 探测MARC记录从属的格式：
                        // return:
                        //		-1	无法探测
                        //		1	UNIMARC	规则：包含200字段
                        //		10	USMARC	规则：包含008字段(innopac的UNIMARC格式也有一个奇怪的008)
                        nRet = DetectMARCSyntax(strMARC);
                        if (nRet == 1)
                            strMarcSyntaxOID = "1.2.840.10003.5.1";
                        else if (nRet == 10)
                            strMarcSyntaxOID = "1.2.840.10003.5.10";

                        // 把自动识别的结果保存下来
                        record.AutoDetectedMarcSyntaxOID = strMarcSyntaxOID;
                    }


                    nRet = GetBrowseText(
                        strMarcSyntaxOID,
                        strMARC,
                        out strBrowseText,
                        out strError);
                    if (nRet == -1)
                        strBrowseText = strError;


                }

            DOADD:
                cols = strBrowseText.Split(new char[] { '\t' });
            DOADDCOLS:
                for (int j = 0; j < cols.Length; j++)
                {
                    item.SubItems.Add(cols[j]);
                }

                item.Tag = record;
                this.listView_browse.Items.Add(item);
            }
            return 0;
        }
#endif

        // 增补ztargetcontrol的popup菜单
        private void zTargetControl1_OnSetMenu(object sender, DigitalPlatform.GUI.GuiAppendMenuEventArgs e)
        {
            ContextMenuStrip contextMenu = e.ContextMenuStrip;

            ToolStripMenuItem menuItem = null;
            // ToolStripMenuItem subMenuItem = null;

            TreeNode node = this.zTargetControl1.SelectedNode;


            // --
            contextMenu.Items.Add(new ToolStripSeparator());

            // Z39.50初始化
            menuItem = new ToolStripMenuItem("初始化连接(&I)");
            if (node == null
                || (node != null && ZTargetControl.IsServerType(node) == false)
                || (ZTargetControl.IsServerOnlineType(node) == true))
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_initialZAssociation_Click);
            contextMenu.Items.Add(menuItem);


            // 切断连接
            menuItem = new ToolStripMenuItem("断开连接(&C)");
            if (node == null
                || (node != null && ZTargetControl.IsServerType(node) == false)
                || (ZTargetControl.IsServerOfflineType(node) == true))
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_closeZAssociation_Click);
            contextMenu.Items.Add(menuItem);

            // --
            contextMenu.Items.Add(new ToolStripSeparator());

            // 发送原始包
            menuItem = new ToolStripMenuItem("发送原始包(&S)");
            menuItem.Click += new EventHandler(menuItem_sendOriginPackage_Click);
            contextMenu.Items.Add(menuItem);

        }


        // 发送原始包。调试用。
        void menuItem_sendOriginPackage_Click(object sender,
            EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            SelectLogRecordDlg dlg = new SelectLogRecordDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Filename = this.UsedLogFilename;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.UsedLogFilename = dlg.Filename;

            ZConnection connection = this.GetCurrentZConnection();
            Debug.Assert(connection.TargetInfo != null, "");

            /*
            connection.QueryString = "";
            connection.QueryXml = this.queryControl1.GetContent(true);
            connection.ClearResultInfo();
             * */

            EnableQueryControl(false);

            connection.Stop.OnStop += new StopEventHandler(this.DoStop);
            connection.Stop.SetMessage("准备发送原始包 ...");
            connection.Stop.BeginLoop();

            this.Update();
            this.m_mainForm.Update();


            try
            {
                connection.Stop.SetMessage("正在连接 " + connection.TargetInfo.HostName + " : " + connection.TargetInfo.Port.ToString() + " ...");

                if (connection.ZChannel.Connected == false
                    || connection.ZChannel.HostName != connection.TargetInfo.HostName
                    || connection.ZChannel.Port != connection.TargetInfo.Port)
                {
                    nRet = connection.ZChannel.NewConnectSocket(connection.TargetInfo.HostName,
                        connection.TargetInfo.Port,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    connection.Stop.SetMessage("正在发送原始包 ...");

                    connection.TargetInfo.OnlineServerIcon(true);

                    // 
                    nRet = DoSendOriginPackage(
                        connection,
                        dlg.Package,
                        out strError);
                    if (nRet == -1)
                    {
                        connection.TargetInfo.OnlineServerIcon(false);
                        goto ERROR1;
                    }

                }

            }
            finally
            {
                try
                {
                    connection.Stop.EndLoop();
                    connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                    connection.Stop.Initial("");

                    EnableQueryControl(true);
                }
                catch { }
            }

            MessageBox.Show(this, "针对服务器 " + connection.TargetInfo.Name + " ("
                + connection.TargetInfo.HostNameAndPort
                + ") 发送原始包成功。");
            this.queryControl1.Focus();

            return;
        ERROR1:
            try // 防止最后退出时报错
            {
                MessageBox.Show(this, strError);
                this.queryControl1.Focus();
            }
            catch
            {
            }
            return;

        }

        // 初始化Z连接
        void menuItem_initialZAssociation_Click(object sender,
            EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            string strInitialResultInfo = "";

            ZConnection connection = this.GetCurrentZConnection();
            Debug.Assert(connection.TargetInfo != null, "");

            /*
            if (connection.TargetInfo == null)
            {

                TargetInfo targetinfo = null;
                nRet = this.zTargetControl1.GetCurrentTarget(
                    out targetinfo,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                connection.TargetInfo = targetinfo;
            }*/


            connection.QueryString = "";
            connection.QueryXml = this.queryControl1.GetContent(true);


            connection.ClearResultInfo();

            // ZConnection connection = this.GetCurrentZConnection();

            //EnableControls(false);
            EnableQueryControl(false);

            connection.Stop.OnStop += new StopEventHandler(this.DoStop);
            connection.Stop.SetMessage("开始初始化连接 ...");
            connection.Stop.BeginLoop();

            this.Update();
            this.m_mainForm.Update();


            try
            {
                connection.Stop.SetMessage("正在连接 " + connection.TargetInfo.HostName + " : " + connection.TargetInfo.Port.ToString() + " ...");

                if (connection.ZChannel.Connected == false
                    || connection.ZChannel.HostName != connection.TargetInfo.HostName
                    || connection.ZChannel.Port != connection.TargetInfo.Port)
                {
                    nRet = connection.ZChannel.NewConnectSocket(connection.TargetInfo.HostName,
                        connection.TargetInfo.Port,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    connection.Stop.SetMessage("正在执行Z39.50初始化 ...");

                    connection.TargetInfo.OnlineServerIcon(true);

                    // Initial
                    nRet = DoInitial(
                        connection,
                        // this.CurrentTargetInfo,
                        out strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        connection.TargetInfo.OnlineServerIcon(false);
                        goto ERROR1;
                    }

                    // 设置当前树上已经选择的节点的扩展信息
                    nRet = ZTargetControl.SetCurrentTargetExtraInfo(
                        this.zTargetControl1.SelectedNode,
                        strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;


                }

            }
            finally
            {
                try
                {
                    connection.Stop.EndLoop();
                    connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                    connection.Stop.Initial("");

                    // EnableControls(true);
                    EnableQueryControl(true);
                }
                catch { }
            }

            MessageBox.Show(this, "针对服务器 " + connection.TargetInfo.Name + " ("
                + connection.TargetInfo.HostNameAndPort
                + ") 初始化连接成功。\r\n\r\n初始化信息:\r\n"
                + strInitialResultInfo);

            this.queryControl1.Focus();

            return;
        ERROR1:
            try // 防止最后退出时报错
            {
                MessageBox.Show(this, strError);
                this.queryControl1.Focus();
            }
            catch
            {
            }
            return;
        }

        // 切断Z连接
        void menuItem_closeZAssociation_Click(object sender,
            EventArgs e)
        {
            // string strError = "";
//             int nRet = 0;

            ZConnection connection = this.GetCurrentZConnection();
            Debug.Assert(connection.TargetInfo != null, "");

            /*
            if (connection.TargetInfo == null)
            {
                TargetInfo targetinfo = null;
                nRet = this.zTargetControl1.GetCurrentTarget(
                    out targetinfo,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                connection.TargetInfo = targetinfo;
            }*/


            // EnableControls(false);
            EnableQueryControl(false);

            connection.Stop.OnStop += new StopEventHandler(this.DoStop);
            connection.Stop.SetMessage("正在切断连接 ...");
            connection.Stop.BeginLoop();

            this.Update();
            this.m_mainForm.Update();
            try
            {

                connection.Stop.SetMessage("正在切断连接 " + connection.TargetInfo.HostName + " : " + connection.TargetInfo.Port.ToString() + " ...");

                if (connection.ZChannel.Connected == true
                    && connection.ZChannel.HostName == connection.TargetInfo.HostName
                    && connection.ZChannel.Port == connection.TargetInfo.Port)
                {
                    connection.CloseConnection();
                }


            }
            finally
            {
                try
                {
                    connection.Stop.EndLoop();
                    connection.Stop.OnStop -= new StopEventHandler(this.DoStop);
                    connection.Stop.Initial("");

                    // EnableControls(true);
                    EnableQueryControl(true);
                }
                catch { }
            }

            return;

            /*
        ERROR1:
            try // 防止最后退出时报错
            {
                MessageBox.Show(this, strError);
                this.queryControl1.Focus();
            }
            catch
            {
            }
            return;
            */
        }

        private void listView_browse_RetrieveVirtualItem(object sender,
            RetrieveVirtualItemEventArgs e)
        {
            /*
            e.Item = new ListViewItem(e.ItemIndex.ToString());
            for (int i = 0; i < this.listView_browse.Columns.Count - 1; i++)
            {
                e.Item.SubItems.Add("");
            }*/
            e.Item = this.CurrentBrowseItems[e.ItemIndex].GetListViewItem(this.listView_browse.Columns.Count);
        }

        private void listView_browse_VirtualItemsSelectionRangeChanged(object sender,
            ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            ZConnection connection = this.GetCurrentZConnection();
            if (connection != null)
                SaveListViewSelectedToVirtual(connection);

            // 需要补发SelectedChanged消息。否则，后到来的本消息，不会促使菜单和工具条按钮状态发生变化
            listView_browse_SelectedIndexChanged(null, null);

#if NOOOOOOOOOOOOOOOO

            if (e.IsSelected == false)
            {
                /*
                if (e.StartIndex == 0
                    && e.EndIndex == 0)
                {
                    Debug.Assert(this.listView_browse.VirtualListSize
                    == this.CurrentBrowseItems.Count, "");
                    for (int i = 0; i < this.listView_browse.VirtualListSize; i++)
                        this.CurrentBrowseItems[i].Selected = false;
                }
                else
                {*/
                    for (int i = e.StartIndex; i <= e.EndIndex; i++)
                        this.CurrentBrowseItems[i].Selected = false;
                // }
            }
            else
            {
                /*
                if (e.StartIndex == 0
                    && e.EndIndex == 0)
                {
                    Debug.Assert(this.listView_browse.VirtualListSize
                    == this.CurrentBrowseItems.Count, "");

                    for (int i = 0; i < this.listView_browse.VirtualListSize; i++)
                        this.CurrentBrowseItems[i].Selected = true;
                }
                else
                {*/
                    for (int i = e.StartIndex; i <= e.EndIndex; i++)
                        this.CurrentBrowseItems[i].Selected = true;
                // }
            }
#endif
        }

        private void listView_browse_ItemSelectionChanged(object sender, 
            ListViewItemSelectionChangedEventArgs e)
        {
            // this.CurrentBrowseItems[e.ItemIndex].Selected = e.IsSelected;
        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 2011/9/10
            if (this.CurrentBrowseItems != null)
                this.CurrentBrowseItems.ExpireSelectedIndices();


            ZConnection connection = this.GetCurrentZConnection();
            if (connection != null
                && !(sender == null && e == null))  // 排除listView_browse_VirtualItemsSelectionRangeChanged()那里专门转过来的调用
                SaveListViewSelectedToVirtual(connection);

            int nSelectedCount = GetBrowseListViewSelectedItemCount();

            // 菜单动态变化
            if (nSelectedCount == 0)
            {
                m_mainForm.toolButton_saveTo.Enabled = false;
                m_mainForm.MenuItem_saveOriginRecordToIso2709.Enabled = false;
                m_mainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
                m_mainForm.toolButton_loadFullRecord.Enabled = false;
            }
            else
            {
                m_mainForm.toolButton_saveTo.Enabled = true;
                m_mainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
                m_mainForm.MenuItem_saveOriginRecordToWorksheet.Enabled = true;
                m_mainForm.toolButton_loadFullRecord.Enabled = true;
            }

        }

        // 服务器特性重新配置了
        private void zTargetControl1_OnServerChanged(object sender, ServerChangedEventArgs e)
        {
            ZConnection connection = this.ZConnections.GetZConnection(e.TreeNode);
            if (connection == null)
                return;

            connection.TargetInfo = null;
        }

        private void comboBox_recordSyntax_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_recordSyntax.Invalidate();
        }

        private void comboBox_elementSetName_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_elementSetName.Invalidate();
        }

        /*
        void CloseConnection()
        {
            this.ZChannel.CloseSocket();
            this.ZChannel.Initialized = false;  // 迫使重新初始化
            if (this.CurrentTargetInfo != null)
                this.CurrentTargetInfo.OfflineServerIcon();
            // 设置当前树上已经选择的节点的扩展信息
            string strError = "";
            int nRet = this.zTargetControl1.SetCurrentTargetExtraInfo(
                "", // strInitialResultInfo,
                out strError);
        }*/

    }


}