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


namespace dp2Catalog
{
    /// <summary>
    /// 连接容器
    /// </summary>
    public class ZConnectionCollection : List<ZConnection>
    {
        public event EventHandler LinkStop = null;
        public event EventHandler UnlinkStop = null;

        public MainForm MainForm
        {
            get
            {
                return this.IZSearchForm.MainForm;
            }
        }
        public IZSearchForm IZSearchForm = null;

        // 获得和treenode关联的一个ZConnection。如果已经有了，就直接使用；如果没有，就创建一个
        public ZConnection GetZConnection(TreeNode treenode)
        {
            ZConnection connection = null;

            // 看看是否已经有了
            connection = FindZConnection(treenode);
            if (connection != null)
                return connection;

            // 新创建一个
            connection = new ZConnection();
            connection.ZChannel = new ZChannel();
            connection.TreeNode = treenode;
            connection.Container = this;
            this.Add(connection);

            connection.ZChannel.CommIdle -= new CommIdleEventHandle(ZChannel_CommIdle);
            connection.ZChannel.CommIdle += new CommIdleEventHandle(ZChannel_CommIdle);

            if (this.LinkStop != null)
            {
                this.LinkStop(connection, new EventArgs());
            }
            else
            {
                Debug.Assert(this.MainForm != null, "");
                connection.LinkStop(treenode.Text);
            }

            return connection;
        }

        // 根据treenode寻找已经存在的ZConnection
        public ZConnection FindZConnection(TreeNode treenode)
        {
            for (int i = 0; i < this.Count; i++)
            {
                ZConnection connection = this[i];

                if (connection.TreeNode == treenode)
                    return connection;
            }

            return null;
        }

        // 根据Stop寻找已经存在的ZConnection
        public ZConnection FindZConnection(Stop stop)
        {
            for (int i = 0; i < this.Count; i++)
            {
                ZConnection connection = this[i];

                if (connection.Stop == stop)
                    return connection;
            }

            return null;
        }


        void ZChannel_CommIdle(object sender, CommIdleEventArgs e)
        {
            Application.DoEvents();
        }

        public void CloseAllSocket()
        {
            for (int i = 0; i < this.Count; i++)
            {
                ZConnection connection = this[i];

                connection.ZChannel.CloseSocket();
            }
        }

        // 将所有stop和管理器脱离关联
        public void UnlinkAllStop()
        {
            for (int i = 0; i < this.Count; i++)
            {
                ZConnection connection = this[i];

                if (this.UnlinkStop != null)
                {
                    this.UnlinkStop(connection, new EventArgs());
                }
                else
                    connection.UnlinkStop();
            }
        }

        // 搜集ZConnnectionCollection中全部服务器节点的检索式XML，用于保存
        // 所创建的XML格式，为<root>元素下有若干<connection>元素，
        // <connection>元素有两个属性：treeNodePath，机载TreeNode的全路径；而queryXml则是检索式Xml字符串
        public string GetAllQueryXml()
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root/>");

            int nCount = 0;
            for (int i = 0; i < this.Count; i++)
            {
                ZConnection connection = this[i];

                // 优化。忽略那些没有query xml内容的连接
                if (String.IsNullOrEmpty(connection.QueryXml) == true)
                    continue;

                XmlNode nodeConnection = dom.CreateElement("connection");
                dom.DocumentElement.AppendChild(nodeConnection);

                DomUtil.SetAttr(nodeConnection, "treeNodePath", ZTargetControl.GetNodeFullPath(connection.TreeNode, '\\'));
                DomUtil.SetAttr(nodeConnection, "queryXml", connection.QueryXml);
                nCount++;
            }

            if (nCount == 0)
                return null;    // 优化

            return dom.OuterXml;
        }

        public void SetAllQueryXml(string strXml,
            TreeView tree)
        {
            if (String.IsNullOrEmpty(strXml) == true)
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("connection");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strTreeNodePath = DomUtil.GetAttr(node, "treeNodePath");
                string strQueryXml = DomUtil.GetAttr(node, "queryXml");

                TreeNode tree_node = TreeViewUtil.GetTreeNode(tree, 
                    strTreeNodePath);
                if (tree_node == null)
                    continue;

                ZConnection connection = GetZConnection(tree_node);
                connection.QueryXml = strQueryXml;
            }
        }
    }

    /// <summary>
    /// 连接
    /// </summary>
    public class ZConnection
    {
        public bool OwnerStop = true;   // 是否拥有独立的Stop
        public AutoResetEvent CompleteEvent = new AutoResetEvent(false);

        public int Searching = 0;   // 0 还没有开始 1 开始了 2 结束了

        public string ElementSetName = "";

        // 缺省的元素集名
        // 确保去掉了--部分
        public string DefaultElementSetName
        {
            get
            {
                if (String.IsNullOrEmpty(this.ElementSetName) == true)
                    return ZTargetControl.GetLeftValue(this.TargetInfo.DefaultElementSetName);

                return ZTargetControl.GetLeftValue(this.ElementSetName);
            }
        }

        public string RecordSyntax = "";

        // 推荐的记录语法
        // 确保已经去掉了--部分
        public string PreferredRecordSyntax
        {
            get
            {
                // return "1.2.840.1003.5.109.10";


                if (String.IsNullOrEmpty(this.RecordSyntax) == true)
                    return ZTargetControl.GetLeftValue(this.TargetInfo.PreferredRecordSyntax);

                return ZTargetControl.GetLeftValue(this.RecordSyntax);
            }
        }

        public VirtualItemCollection VirtualItems = new VirtualItemCollection();

        public bool Enabled = true; // 检索式界面是否应当Enabled?

        public string CurrentRefID = "0";

        public ZConnectionCollection Container = null;


        public IZSearchForm IZSearchForm
        {
            get
            {
                Debug.Assert(this.Container.IZSearchForm != null);
                return this.Container.IZSearchForm;
            }
        }

        public DigitalPlatform.Stop Stop = null;

        public TreeNode TreeNode = null;
        public ZChannel ZChannel = null;

        public TargetInfo TargetInfo = null;
        public string QueryString = "";
        public int ResultCount = -3; // -3表示未曾检索过(不显示数字); -2表示正在检索; -1表示检索出错; >=0表示命中数量

        public string QueryXml = "";    // XML检索式

        public Encoding ForcedRecordsEncoding = null;

        public RecordCollection Records = new RecordCollection();    // search/present得到的结果集

        public string ErrorInfo = "";

        // 关联Stop
        public void LinkStop(string strStopName)
        {
            Debug.Assert(this.IZSearchForm != null, "");
            Debug.Assert(this.IZSearchForm.MainForm != null, "");

            this.Stop = new DigitalPlatform.Stop();
            this.Stop.Name = strStopName;
            this.Stop.Register(this.IZSearchForm.MainForm.stopManager,
                false);	// 和容器关联
        }

        // 脱离关联
        public void UnlinkStop()
        {
            if (this.Stop != null) // 脱离关联
            {
                // Stop.DoStop();

                this.Stop.Unregister();	// 和容器关联
                this.Stop = null;
            }
        }

        // 获得当前编码方式
        // 如何让后面选定的强制编码方式生效?
        public Encoding GetRecordsEncoding(
            MainForm MainForm,
            string strRecordSyntaxOID)
        {
            if (this.ForcedRecordsEncoding != null)
                return this.ForcedRecordsEncoding;

            if (String.IsNullOrEmpty(strRecordSyntaxOID) == true)
            {
                if (this.TargetInfo == null)
                    return Encoding.GetEncoding("gb2312");

                return this.TargetInfo.DefaultRecordsEncoding;
            }

            string strEncodingName = this.TargetInfo.Bindings.GetEncodingName(strRecordSyntaxOID);

            if (String.IsNullOrEmpty(strEncodingName) == true)
            {
                if (this.TargetInfo == null)
                    return Encoding.GetEncoding("gb2312");

                return this.TargetInfo.DefaultRecordsEncoding;
            }
            Encoding encoding = null;
            string strError = "";
            int nRet = MainForm.GetEncoding(strEncodingName,
                out encoding,
                out strError);
            if (nRet == -1)
            {
                if (this.TargetInfo == null)
                    return Encoding.GetEncoding("gb2312");

                return this.TargetInfo.DefaultRecordsEncoding;
            }

            return encoding;
        }


        public void DoStop()
        {
            if (this.ZChannel.Connected == true)
            {
                /*
                this.ZChannel.CloseSocket();
                if (this.CurrentTargetInfo != null)
                    this.CurrentTargetInfo.OfflineServerIcon();
                 * */
                CloseConnection();
            }
            else if (this.ZChannel != null)
            {
                // 如果处在没有连接的状态
                this.ZChannel.Stop();
            }
        }

        public void CloseConnection()
        {
            this.ZChannel.CloseSocket();
            this.ZChannel.Initialized = false;  // 迫使重新初始化

            if (this.TreeNode != null
                && ZTargetControl.IsServerOnlineType(this.TreeNode) == true)
            {
                ZTargetControl.OnlineServerNodeIcon(this.TreeNode, false);
            }


            // 设置当前树上已经选择的节点的扩展信息
            string strError = "";
            int nRet = ZTargetControl.SetCurrentTargetExtraInfo(
                this.TreeNode,
                "", // strInitialResultInfo,
                out strError);

        }

        public void EnableControls(bool bEnable)
        {
            this.Enabled = bEnable;

            /*
            object[] pList = { this, bEnable };
            ZSearchForm.Invoke(
                new ZSearchForm.Delegate_EnableQueryControl(ZSearchForm.EnableQueryControl), pList);
             * */
            this.IZSearchForm.EnableQueryControl(this, bEnable);
        }

        void MessageBox(string strText)
        {
            /*
            object[] pList = { this, strText };
            ZSearchForm.Invoke(
                new ZSearchForm.Delegate_ShowMessageBox(ZSearchForm.ShowMessageBox), pList);
             * */
            IZSearchForm.ShowMessageBox(this, strText);
        }

        public void ShowQueryResultInfo(string strText)
        {
            this.ErrorInfo = strText;

            /*
            if (this.ZSearchForm.IsDisposed == true)
                return;

            object[] pList = { this, strText };
            ZSearchForm.Invoke(
                new ZSearchForm.Delegate_ShowQueryResultInfo(ZSearchForm.ShowQueryResultInfo), pList);
             * */
            IZSearchForm.ShowQueryResultInfo(this, strText);
        }

        public void AsyncShowQueryResultInfo(string strText)
        {
            /*
            if (this.ZSearchForm.IsDisposed == true)
                return;

            object[] pList = { this, strText };
            ZSearchForm.Invoke(
                new ZSearchForm.Delegate_ShowQueryResultInfo(ZSearchForm.ShowQueryResultInfo), pList);
             * */
            IZSearchForm.ShowQueryResultInfo(this, strText);
        }

        // 清除以往的(已经获取到前端)结果集信息和相应的显示
        // thread:
        //      界面线程
        public void ClearResultInfo()
        {
            this.ResultCount = -2;    // 表示正在检索

            if (this.Records != null)
                this.Records.Clear();

            // this.listView_browse.Items.Clear();
            if (this.VirtualItems != null)
            {
                this.VirtualItems.Clear();

                // 显示出来
                DisplayBrowseItem(true);
                /*
                if (current_connection == connection)
                    LinkRecordsToListView(connection.VirtualItems); // listview是公用的
                 * */
            }

            /*
            if (current_connection == connection)
            {
                ShowQueryResultInfo(connection, "");
            }*/
            ShowQueryResultInfo("");

        }

        // 检索一个服务器
        // 启动检索以后控制就立即返回
        public int Search()
        {
            Thread clientThread = new Thread(new ThreadStart(SearchThread));
            clientThread.Start();
            return 0;
        }

        // 将浏览事项显示出来
        void DisplayBrowseItem(bool bTriggerSelChanged = false)
        {
            /*
            // 因为涉及到Form界面元素操作，所以要用Invoke来操作
            object[] pList = {this, bTriggerSelChanged};
            ZSearchForm.Invoke(
                new ZSearchForm.Delegate_DisplayBrowseItems(ZSearchForm.DisplayBrowseItems), pList);
             * */
            IZSearchForm.DisplayBrowseItems(this, bTriggerSelChanged);
        }

        // 检索线程
        public void SearchThread()
        {
            this.Searching = 1;

            this.ErrorInfo = "";

            string strError = "";
            int nRet = 0;

            // 发出指令，清除结果集显示
            // this.ClearResultInfo(connection);
            this.ClearResultInfo();

            // 把检索式界面Disable
            this.EnableControls(false);


            this.Stop.OnStop += new StopEventHandler(Stop_OnStop);
            this.Stop.SetMessage("开始检索 ...");
            this.Stop.BeginLoop();

            /*
            this.Update();
            this.MainForm.Update();
             * */
            try
            {

                this.Stop.SetMessage("正在连接 " + this.TargetInfo.HostName + " : " + this.TargetInfo.Port.ToString() + " ...");

                if (this.ZChannel.Connected == false
                    || this.ZChannel.HostName != this.TargetInfo.HostName
                    || this.ZChannel.Port != this.TargetInfo.Port)
                {
                    nRet = this.ZChannel.NewConnectSocket(this.TargetInfo.HostName,
                        this.TargetInfo.Port,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;


                    this.Stop.SetMessage("正在执行Z39.50初始化 ...");

                    // 暂时online一下图标
                    this.TargetInfo.OnlineServerIcon(true);


                    // Initial
                    string strInitialResultInfo = "";
                    nRet = this.DoInitial(
                        this.TargetInfo.IgnoreReferenceID,
                        out strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        this.ResultCount = -1;
                        try
                        {
                            this.TargetInfo.OnlineServerIcon(false);
                        }
                        catch { }   // 可能由于窗口已经关闭，对象已经释放

                        goto ERROR1;
                    }

                    // 设置当前树上已经选择的节点的扩展信息
                    nRet = ZTargetControl.SetCurrentTargetExtraInfo(
                        this.TreeNode,
                        strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // this.TargetInfo.OnlineServerIcon();
                }

                this.Stop.SetMessage("正在检索 '" + this.QueryString + "' ...");


                int nResultCount = 0;
                // return:
                //		-1	error
                //		0	fail
                //		1	succeed
                nRet = this.DoSearch(
                    this.QueryString,
                    this.TargetInfo.DefaultQueryTermEncoding,
                    this.TargetInfo.DbNames,
                    this.TargetInfo.DefaultResultSetName,
                    out nResultCount,
                    out strError);
                if (nRet == -1)
                {
                    this.ResultCount = -1;
                    goto ERROR1;
                }

                if (nRet == 0)
                {
                    this.ResultCount = -1;
                    goto ERROR1;
                }

                this.ResultCount = nResultCount;

                // 显示命中结果
                this.ShowQueryResultInfo("命中结果条数:" + this.ResultCount.ToString());

                if (nResultCount != 0)
                {

                    int nCount = Math.Min(this.TargetInfo.PresentPerBatchCount, nResultCount);

                    this.Stop.SetMessage("正在获取命中结果 ( 1-" + nCount.ToString() + " of " + nResultCount + " ) ...");

                    // string strElementSetName = ZTargetControl.GetLeftValue(this.comboBox_elementSetName.Text);  // this.CurrentTargetInfo.DefaultElementSetName;
                    string strElementSetName = this.DefaultElementSetName;

                    if (strElementSetName == "B"
                        && this.TargetInfo.FirstFull == true)
                        strElementSetName = "F";

                    RecordCollection records = null;

                    nRet = this.DoPresent(
                        this.TargetInfo.DefaultResultSetName,
                        0, // nStart,
                        nCount, // nCount,
                        this.TargetInfo.PresentPerBatchCount,   // 推荐的每次数量
                        strElementSetName,    // "F" strElementSetName,
                        this.PreferredRecordSyntax,
                        // ZTargetControl.GetLeftValue(this.comboBox_recordSyntax.Text),    // this.CurrentTargetInfo.PreferredRecordSyntax,
                        true,   // 立即显示出来
                        out records,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    else
                    {
                        /*
                        nRet = FillRecordsToVirtualItems(
                            this.Stop,
                            records,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        DisplayBrowsItem();
                         * */

                    }
                }

            }
            finally
            {
                try
                {
                    this.Stop.EndLoop();
                    this.Stop.OnStop -= new StopEventHandler(Stop_OnStop);
                    this.Stop.Initial("");

                    this.EnableControls(true);
                }
                catch { }

                this.Searching = 2;
            }

            /*
            if (this.listView_browse.Items.Count > 0)
                this.listView_browse.Focus();
            else
                this.queryControl1.Focus();
             * */


            return;
        ERROR1:
            /*
            try // 防止最后退出时报错
            {
                MessageBox.Show(this, strError);
                this.queryControl1.Focus();
            }
            catch
            {
            }
             * */
            this.ErrorInfo = strError;
            this.ShowQueryResultInfo(this.ErrorInfo);
            MessageBox(this.ErrorInfo);
            return;
        }

        List<string> m_commands = null;
        public bool Stopped = false;    // 是否被中断

        // 检索一个服务器
        // 启动检索以后控制就立即返回
        public int BeginCommands(List<string> commands)
        {
            this.Searching = 1;
            this.CompleteEvent.Reset();

            this.ErrorInfo = "";

            this.Stopped = false;

            this.m_commands = new List<string>();
            if (commands != null)
                this.m_commands.AddRange(commands);

            int nRet = 0;

            // 发出指令，清除结果集显示
            // this.ClearResultInfo(connection);

            // 只有当包含 search 命令的时候才清除命中结果数。
            // 如果只有 present 命令，则不清除
            if (this.m_commands.IndexOf("search") != -1)
                this.ClearResultInfo();

            // 把检索式界面Disable
            this.EnableControls(false);


            this.Stop.OnStop += new StopEventHandler(Stop_OnStop);

            if (this.OwnerStop == true)
            {
                this.Stop.SetMessage("开始检索 ...");
                this.Stop.BeginLoop();
            }

            this.CommandsComplete -= new EventHandler(ZConnection_CommandsComplete);
            this.CommandsComplete += new EventHandler(ZConnection_CommandsComplete);

            bool bError = false;
            try
            {
                if (this.ZChannel.Connected == false
                    || this.ZChannel.HostName != this.TargetInfo.HostName
                    || this.ZChannel.Port != this.TargetInfo.Port)
                {
                    this.Stop.SetMessage("正在连接 " + this.TargetInfo.HostName + " : " + this.TargetInfo.Port.ToString() + " ...");

                    this.ZChannel.ConnectComplete -= new EventHandler(ZChannel_ConnectComplete);
                    this.ZChannel.ConnectComplete += new EventHandler(ZChannel_ConnectComplete);

                    nRet = this.ZChannel.ConnectAsync(this.TargetInfo.HostName,
                        this.TargetInfo.Port);
                    if (nRet == -1)
                    {
                        bError = true;
                        return -1;
                    }
                }
                else
                {
                    ZConnection_InitialComplete(null, null);
                }

                return 0;
            }
            finally
            {
                if (bError == true)
                    this.CommandsComplete(this, new EventArgs());
            }
        }

        void ZConnection_CommandsComplete(object sender, EventArgs e)
        {
            try
            {
                if (this.Stop.State != 0)
                {
                    this.Stopped = true;
                }

                if (this.OwnerStop == true)
                {

                    this.Stop.EndLoop();
                    this.Stop.Initial("");
                }
                this.Stop.OnStop -= new StopEventHandler(Stop_OnStop);

                this.EnableControls(true);
            }
            catch { }

            this.Searching = 2;
            this.CompleteEvent.Set();

            if (string.IsNullOrEmpty(this.ErrorInfo) == false)
            {
                this.ShowQueryResultInfo(this.ErrorInfo);
                MessageBox(this.ErrorInfo);
            }
        }

        // Connect结束，进行Z39.50初始化
        void ZChannel_ConnectComplete(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.ZChannel.ErrorInfo) == false)
            {
                this.ErrorInfo = this.ZChannel.ErrorInfo;
                this.ResultCount = -1;

                if (this.CommandsComplete != null)
                    this.CommandsComplete(this, new EventArgs());
                return;
            }

            this.Stop.SetMessage("正在执行Z39.50初始化 ...");

            // 暂时online一下图标
            this.TargetInfo.OnlineServerIcon(true);

            // Initial
            this.InitialComplete -= new EventHandler(ZConnection_InitialComplete);
            this.InitialComplete += new EventHandler(ZConnection_InitialComplete);
            int nRet = this.DoInitialAsync(
                this.TargetInfo.IgnoreReferenceID);
            if (nRet == -1)
            {
                this.ResultCount = -1;
                try
                {
                    this.TargetInfo.OnlineServerIcon(false);
                }
                catch { }   // 可能由于窗口已经关闭，对象已经释放

                return;
            }
        }

        void ZConnection_InitialComplete(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.ErrorInfo) == false
                || this.m_commands.Count == 0)
            {
                if (string.IsNullOrEmpty(this.ErrorInfo) == false)
                    this.ResultCount = -1;

                if (this.CommandsComplete != null)
                    this.CommandsComplete(this, new EventArgs());
                return;
            }

            if (this.m_commands.Count > 0
                && this.m_commands[0] == "search")
            {
            // 执行Search
                this.SearchComplete -= new EventHandler(ZConnection_SearchComplete);
                this.SearchComplete += new EventHandler(ZConnection_SearchComplete);
                int nRet = DoSearchAsync();
                if (nRet == -1)
                    return;
            }
            else if (this.m_commands.Count > 0
                && this.m_commands[0] == "present")
            {
                // 执行Present
                this.PresentComplete -= new EventHandler(ZConnection_PresentComplete);
                this.PresentComplete += new EventHandler(ZConnection_PresentComplete);
                int nRet = DoPresentAsync(true);
                if (nRet == -1)
                    return;
            }
        }

        void ZConnection_SearchComplete(object sender, EventArgs e)
        {
            if (this.m_commands.Count > 0)
                this.m_commands.RemoveAt(0);

            if (string.IsNullOrEmpty(this.ErrorInfo) == false
                || this.m_commands.Count == 0)
            {
                if (string.IsNullOrEmpty(this.ErrorInfo) == false)
                    this.ResultCount = -1;

                if (this.CommandsComplete != null)
                    this.CommandsComplete(this, new EventArgs());
                return;
            }

            // 显示检索命中结果

            if (this.m_commands.Count > 0
                && this.m_commands[0] == "present")
            {
                // 继续Present
                this.PresentComplete -= new EventHandler(ZConnection_PresentComplete);
                this.PresentComplete += new EventHandler(ZConnection_PresentComplete);
                int nRet = DoPresentAsync(true);
                if (nRet == -1)
                    return;
            }
        }

        void ZConnection_PresentComplete(object sender, EventArgs e)
        {
            if (this.CommandsComplete != null)
                this.CommandsComplete(this, new EventArgs());
        }

        // return:
        //      -1  error
        //      0   线程已经启动，但是没有等它结束
        //      1   线程已经结束
        public int NextBatch(bool bWaitFinish)
        {
            Thread clientThread = new Thread(new ThreadStart(NextBatchThread));
            clientThread.Start();

            if (bWaitFinish == false)
                return 0;

            /*
            int nIdleTimeCount = 0;
            int nIdleTicks = 100;
             * */

            while (true)
            {
                Application.DoEvents();

                if (clientThread.IsAlive == false)
                    break;

                Thread.Sleep(100);
            }

            return 1;
        }

        public int NextAllBatch(bool bWaitFinish)
        {
            Thread clientThread = new Thread(new ThreadStart(NextAllBatchThread));
            clientThread.Start();

            if (bWaitFinish == false)
                return 0;

            /*
            int nIdleTimeCount = 0;
            int nIdleTicks = 100;
             * */

            while (true)
            {
                Application.DoEvents();

                if (clientThread.IsAlive == false)
                    break;

                Thread.Sleep(100);
            }

            return 1;
        }        
        
        // 新装入一批记录的线程主函数
        public void NextBatchThread()
        {
            Debug.Assert(this.ResultCount >= 0, "");

            int nCount = Math.Min(this.TargetInfo.PresentPerBatchCount,
                this.ResultCount - this.Records.Count);

            NextBatchThreadBase(nCount);
        }

        // 新装入后面未装的所有批记录的线程主函数
        public void NextAllBatchThread()
        {
            Debug.Assert(this.ResultCount >= 0, "");

            int nCount = this.ResultCount - this.Records.Count;

            NextBatchThreadBase(nCount);
        }

        // 线程主函数 基础函数。不能当线程函数用
        public void NextBatchThreadBase(int nCount)
        {
            this.ErrorInfo = "";

            string strError = "";
            int nRet = 0;

            if (this.Records == null)
                this.Records = new RecordCollection();

            /*
            // 新装入一批记录
            int nCount = Math.Min(this.TargetInfo.PresentPerBatchCount,
                this.ResultCount - this.Records.Count);
             * */

            if (nCount <= 0)
            {
                // 没有必要么
                strError = "命中结果已经全部获取完毕。";
                goto ERROR1;
            }

            this.Stop.OnStop += new StopEventHandler(this.Stop_OnStop);
            this.Stop.SetMessage("从服务器装入记录 ...");
            this.Stop.BeginLoop();

            this.EnableControls(false);

            /*
            this.Update();
            this.MainForm.Update();
             * */

            try
            {

                // string strElementSetName = ZTargetControl.GetLeftValue(this.comboBox_elementSetName.Text);  // this.CurrentTargetInfo.DefaultElementSetName;
                string strElementSetName = this.DefaultElementSetName;

                if (strElementSetName == "B"
                    && this.TargetInfo.FirstFull == true)
                    strElementSetName = "F";

                RecordCollection records = null;

                nRet = DoPresent(
                    this.TargetInfo.DefaultResultSetName,
                    this.Records.Count, // nStart,
                    nCount, // nCount,
                    this.TargetInfo.PresentPerBatchCount,   // 推荐的每次数量
                    strElementSetName,    // "F" strElementSetName,
                    // ZTargetControl.GetLeftValue(this.comboBox_recordSyntax.Text),
                    this.PreferredRecordSyntax,
                    true,   // 立即显示出来
                    out records,
                    out strError);
                if (nRet == -1)
                {
                    strError = "从 " + this.Records.Count.ToString()
                        + " 开始装入新的一批记录时出错：" + strError;
                    goto ERROR1;
                }
                else
                {

                    /*
                    nRet = FillRecordsToVirtualItems(
                        this.Stop,
                        records,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    DisplayBrowsItem();
                     * */


                }
            }
            finally
            {
                this.Stop.EndLoop();
                this.Stop.OnStop -= new StopEventHandler(this.Stop_OnStop);
                this.Stop.Initial("");

                this.EnableControls(true);
            }

            return;
        ERROR1:
            // MessageBox.Show(this, strError);
            this.ErrorInfo = strError;
            this.ShowQueryResultInfo(this.ErrorInfo);
            MessageBox(this.ErrorInfo);
            return;
        }

        void Stop_OnStop(object sender, StopEventArgs e)
        {
            this.DoStop();
        }

        #region 异步 Initial

        public event EventHandler InitialComplete = null;
        string m_strRequestRefID = "";  // 请求中使用的refid。如果为""表示不检查
        string m_strResultInfo = "";

        // 执行初始化
        // 异步模式
        // parameters:
        //      strResultInfo   [out]返回说明初始化结果的文字
        int DoInitialAsync(
            bool bIgnoreReferenceID)
        {
            this.m_strResultInfo = "";

            ZConnection connection = this;

            byte[] baPackage = null;
            BerTree tree = new BerTree();
            INIT_REQUEST struInit_request = new INIT_REQUEST();
            int nRet;

            TargetInfo targetinfo = connection.TargetInfo;

            if (connection.ZChannel.Initialized == true)
            {
                this.ErrorInfo = "Already Initialized";
                goto ERROR1;
            }

            if (bIgnoreReferenceID == false)
            {
                this.m_strRequestRefID = this.CurrentRefID;
            }
            else
            {
                this.m_strRequestRefID = "";
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

            struInit_request.m_strImplementationId = "DigitalPlatform";
            struInit_request.m_strImplementationVersion = "2.1.0";
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
                this.ErrorInfo = "CBERTree::InitRequest() fail!";
                goto ERROR1;
            }

            if (connection.ZChannel.Connected == false)
            {
                this.ErrorInfo = "socket尚未连接或者已经被关闭";
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
            ClearSendRecvCompleteEvent();
            connection.ZChannel.SendRecvComplete += new EventHandler(ZChannel_initial_SendRecvComplete);
                    // 发出请求包，接收响应包
        // return:
        //      -1  出错
        //      0   成功启动
        //      1   发出前，发现流中有未读入的数据
            nRet = connection.ZChannel.SendRecvAsync(baPackage);
            if (nRet != 0)
                goto ERROR1;

            return 0;
        ERROR1:
            if (this.InitialComplete != null)
                this.InitialComplete(this, new EventArgs());

            return -1;
        }

        void ZChannel_initial_SendRecvComplete(object sender, EventArgs e)
        {
            ZConnection connection = this;

            if (string.IsNullOrEmpty(this.ZChannel.ErrorInfo) == false)
            {
                this.ErrorInfo = this.ZChannel.ErrorInfo;
                goto ERROR1;
            }

            byte[] baOutPackage = connection.ZChannel.RecvPackage;
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
            string strError = "";

            INIT_RESPONSE init_response = new INIT_RESPONSE();
            int nRet = BerTree.GetInfo_InitResponse(tree1.GetAPDuRoot(),
                                 ref init_response,
                                 out strError);
            if (nRet == -1)
            {
                this.ErrorInfo = strError;
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.m_strRequestRefID) == false)
            {
                // 2007/11/2。可以帮助发现旧版本dp2zserver的错误
                if (this.m_strRequestRefID != init_response.m_strReferenceId)
                {
                    this.ErrorInfo = "请求的 reference id [" + this.m_strRequestRefID + "] 和 响应的 reference id [" + init_response.m_strReferenceId + "] 不一致！";
                    goto ERROR1;
                }
            }

            // 2007/11/5检查version和options
            bool bOption_0 = BerTree.GetBit(init_response.m_strOptions,
                0);
            if (bOption_0 == false)
            {
                this.ErrorInfo = "服务器响应的 option bit 0 表示不支持 search";
                goto ERROR1;
            }

            bool bOption_1 = BerTree.GetBit(init_response.m_strOptions,
                1);
            if (bOption_1 == false)
            {
                this.ErrorInfo = "服务器响应的 option bit 1 表示不支持 present";
                goto ERROR1;
            }

            if (init_response.m_nResult != 0)
            {
                // strError = "Initial OK";
            }
            else
            {
                this.ErrorInfo = "Initial被拒绝。\r\n\r\n错误码 ["
                    + init_response.m_lErrorCode.ToString()
                    + "]\r\n错误消息["
                    + init_response.m_strErrorMessage + "]";

                this.m_strResultInfo = BuildInitialResultInfo(init_response);
                goto ERROR1;
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
                    this.TargetInfo.DefaultQueryTermEncoding = Encoding.UTF8;
                    this.TargetInfo.Changed = true;

                    if (init_response.m_charNego.RecordsInSelectedCharsets == 1)
                        connection.ForcedRecordsEncoding = Encoding.UTF8;
                }
            }

            this.m_strResultInfo = BuildInitialResultInfo(init_response);

            if (this.InitialComplete != null)
                this.InitialComplete(null, null);

            return;
            ERROR1:
            if (this.InitialComplete != null)
                this.InitialComplete(null, null);
        }

#endregion

        // 执行初始化
        // 同步模式
        // parameters:
        //      strResultInfo   [out]返回说明初始化结果的文字
        int DoInitial(
            bool bIgnoreReferenceID,
            out string strResultInfo,
            out string strError)
        {
            strResultInfo = "";
            strError = "";

            ZConnection connection = this;

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


            if (bIgnoreReferenceID == false)
            {
                // 2007/11/2。可以帮助发现旧版本dp2zserver的错误
                if (struInit_request.m_strReferenceId != init_response.m_strReferenceId)
                {
                    strError = "请求的 reference id [" + struInit_request.m_strReferenceId + "] 和 响应的 reference id [" + init_response.m_strReferenceId + "] 不一致！";
                    goto ERROR1;
                }
            }

            // 2007/11/5检查version和options
            bool bOption_0 = BerTree.GetBit(init_response.m_strOptions,
                0);
            if (bOption_0 == false)
            {
                strError = "服务器响应的 option bit 0 表示不支持 search";
                goto ERROR1;
            }

            bool bOption_1 = BerTree.GetBit(init_response.m_strOptions,
                1);
            if (bOption_1 == false)
            {
                strError = "服务器响应的 option bit 1 表示不支持 present";
                goto ERROR1;
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

                strResultInfo = BuildInitialResultInfo(init_response);
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

            strResultInfo = BuildInitialResultInfo(init_response);

            return 0;
        ERROR1:
            strResultInfo = strError;
            return -1;
        }

        // return:
        //		-1	error
        //		0	fail
        //		1	succeed
        int DoSearch(
            string strQuery,
            Encoding queryTermEncoding,
            string[] dbnames,
            string strResultSetName,
            out int nResultCount,
            out string strError)
        {
            strError = "";

            ZConnection connection = this;

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
            struSearch_request.m_strPreferredRecordSyntax = this.PreferredRecordSyntax; //  ZTargetControl.GetLeftValue(this.TargetInfo.PreferredRecordSyntax);   // BerTree.MARC_SYNTAX;
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

            byte[] baOutPackage = null;
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

        #region 异步 Search

        public event EventHandler SearchComplete = null;

        string m_strQuery = "";
        Encoding m_queryTermEncoding = Encoding.UTF8;
        string [] m_dbnames = null;
        string m_strResultSetName = "";
        // int m_nResultCount = 0;
        int m_nSearchStatus = 0;    // 检索是否成功 0 失败 1 成功

        // 设置检索参数
        public void SetSearchParameters(
            string strQuery,
            Encoding queryTermEncoding,
            string[] dbnames,
            string strResultSetName
            )
        {
            this.m_strQuery = strQuery;
            this.m_queryTermEncoding = queryTermEncoding;
            this.m_dbnames = dbnames;
            this.m_strResultSetName = strResultSetName;
            this.ResultCount = 0;
            this.m_nSearchStatus = 0;
        }

        int DoSearchAsync()
        {
            ZConnection connection = this;

            this.Stop.SetMessage("正在对服务器 "+this.TargetInfo.ServerName+" 检索 '" + this.m_strQuery + "' ...");

            BerTree tree = new BerTree();
            SEARCH_REQUEST struSearch_request = new SEARCH_REQUEST();
            byte[] baPackage = null;
            int nRet;

            // -->
            BerTree tree1 = new BerTree();

            this.ResultCount = 0;
            this.m_nSearchStatus = 0;

            struSearch_request.m_dbnames = this.m_dbnames;

            Debug.Assert(struSearch_request.m_dbnames.Length != 0, "");

            struSearch_request.m_strReferenceId = this.CurrentRefID;
            struSearch_request.m_lSmallSetUpperBound = 0;
            struSearch_request.m_lLargeSetLowerBound = 1;
            struSearch_request.m_lMediumSetPresentNumber = 0;
            struSearch_request.m_nReplaceIndicator = 1;
            struSearch_request.m_strResultSetName = this.m_strResultSetName;   // "default";
            struSearch_request.m_strSmallSetElementSetNames = "";
            struSearch_request.m_strMediumSetElementSetNames = "";
            struSearch_request.m_strPreferredRecordSyntax = this.PreferredRecordSyntax; //  ZTargetControl.GetLeftValue(this.TargetInfo.PreferredRecordSyntax);   // BerTree.MARC_SYNTAX;
            struSearch_request.m_strQuery = this.m_strQuery;
            struSearch_request.m_nQuery_type = 1;
            struSearch_request.m_queryTermEncoding = this.m_queryTermEncoding;

            nRet = tree.SearchRequest(struSearch_request,
                out baPackage);

            if (nRet == -1)
            {
                this.ErrorInfo = "CBERTree::SearchRequest() fail!";
                goto ERROR1;
            }

#if DUMPTOFILE
            string strBinFile = this.MainForm.DataDir + "\\searchrequest.bin";
            File.Delete(strBinFile);
            DumpPackage(strBinFile,
                baPackage);
            string strLogFile = this.MainForm.DataDir + "\\searchrequest.txt";
            File.Delete(strLogFile);
            tree.m_RootNode.DumpToFile(strLogFile);
#endif
            ClearSendRecvCompleteEvent();
            connection.ZChannel.SendRecvComplete += new EventHandler(ZChannel_search_SendRecvComplete);
            // 发出请求包，接收响应包
            // return:
            //      -1  出错
            //      0   成功启动
            //      1   发出前，发现流中有未读入的数据
            nRet = connection.ZChannel.SendRecvAsync(baPackage);
            if (nRet != 0)
            {
                this.ErrorInfo = connection.ZChannel.ErrorInfo;
                goto ERROR1;
            }

            return 0;
        ERROR1:
            this.ResultCount = -1;

            if (this.SearchComplete != null)
                this.SearchComplete(this, new EventArgs());
            return -1;
        }

        void ZChannel_search_SendRecvComplete(object sender, EventArgs e)
        {
            ZConnection connection = this;

            if (string.IsNullOrEmpty(this.ZChannel.ErrorInfo) == false)
            {
                this.ErrorInfo = this.ZChannel.ErrorInfo;
                goto ERROR1;
            }

            byte[] baOutPackage = connection.ZChannel.RecvPackage;

#if DUPMTOFILE
	DeleteFile("searchresponse.bin");
	DumpPackage("searchresponse.bin",
				(char *)baOutPackage.GetData(),
				baOutPackage.GetSize());
#endif
            BerTree tree1 = new BerTree();
            int nTotlen = 0;

            tree1.m_RootNode.BuildPartTree(baOutPackage,
                0,
                baOutPackage.Length,
                out nTotlen);

            string strError = "";
            SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
            int nRet = BerTree.GetInfo_SearchResponse(tree1.GetAPDuRoot(),
                                   ref search_response,
                                   true,
                                   out strError);
            if (nRet == -1)
            {
                this.ErrorInfo = strError;
                goto ERROR1;
            }

#if DUMPTOFILE
	DeleteFile("SearchResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("SearchResponse.txt");
#endif

            this.ResultCount = (int)search_response.m_lResultCount;

            if (search_response.m_nSearchStatus == 0)
            {
                this.ErrorInfo = "Search Fail: diagRecords:\r\n" + search_response.m_diagRecords.GetMessage();
                this.m_nSearchStatus = 0;// search fail
                goto ERROR1;
            }

            this.m_nSearchStatus = 1;   // search_response.m_nSearchStatus != 0	// 不一定是1

            // 显示命中结果
            this.AsyncShowQueryResultInfo("命中结果条数:" + this.ResultCount.ToString());

            if (this.SearchComplete != null)
            {
                this.SearchComplete(null, null);
            }
            return;
            ERROR1:
            this.ResultCount = -1;
            if (this.SearchComplete != null)
            {
                this.SearchComplete(null, null);
            }
        }

        #endregion

        // 获得记录
        // 确保一定可以获得nCount个
        // parameters:
        //          nPreferedEachCount  推荐的每次条数。这涉及到响应的敏捷性。如果为-1或者0，表示最大
        public int DoPresent(
            string strResultSetName,
            int nStart,
            int nCount,
            int nPreferedEachCount,  // 推荐的每次条数。这涉及到响应的敏捷性
            string strElementSetName,
            string strPreferredRecordSyntax,
            bool bDisplay,
            out RecordCollection records,
            out string strError)
        {
            ZConnection connection = this;

            records = new RecordCollection();
            if (nCount == 0)
            {
                strError = "nCount为0";
                return 0;
            }

            StopStyle old_style = StopStyle.None;
            if (this.Stop != null)
            {
                old_style = this.Stop.Style;
                this.Stop.Style = StopStyle.EnableHalfStop; // 允许半中断
            }

            try
            {


                int nGeted = 0;
                for (; ; )
                {
                    if (this.Stop != null)
                    {
                        if (this.Stop.State != 0)
                        {
                            strError = "用户中断";
                            break;
                        }
                    }

                    int nPerCount = 0;

                    if (nPreferedEachCount == -1 || nPreferedEachCount == 0)
                        nPerCount = nCount - nGeted;
                    else
                        nPerCount = Math.Min(nPreferedEachCount, nCount - nGeted);

                    this.Stop.SetMessage("正在获取命中结果 ( " + (nStart + nGeted + 1).ToString() + "-" + (nStart + nGeted + nPerCount).ToString() + " of " + this.ResultCount + " ) ...");

                    RecordCollection temprecords = null;
                    int nRet = DoOncePresent(
                        strResultSetName,
                        nStart + nGeted,
                        nPerCount,
                        strElementSetName,
                        strPreferredRecordSyntax,
                        out temprecords,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (temprecords == null)
                        break;

                    if (bDisplay == true)
                    {
                        this.Stop.SetMessage("正在显示已经获取的命中结果 ( " + (nStart + nGeted + 1).ToString() + "-" + (nStart + nGeted + temprecords.Count).ToString() + " of " + this.ResultCount.ToString() + " ) ...");

                        // 
                        // 如果必要，加入新的一批
                        if (nStart + nGeted >= connection.Records.Count)
                            connection.Records.AddRange(temprecords);
                        else
                        {
                            for (int i = 0; i < temprecords.Count; i++)
                            {
                                if (i + nStart + nGeted >= connection.Records.Count)
                                    connection.Records.Add(temprecords[i]); // 追加
                                else
                                    connection.Records[i + nStart + nGeted] = temprecords[i];   // 替换
                            }
                        }

                        // 2007/12/17 changed
                        nRet = FillRecordsToVirtualItems(
                            this.Stop,
                            connection.Records,
                            nStart + nGeted,
                            temprecords.Count,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // TODO: 看看有没有必要追加或补充。如果没有必要追加或补充，那就替换部分

                        /*
                        nRet = FillRecordsToVirtualItems(
                            this.Stop,
                            temprecords,
                            out strError);
                        if (nRet == -1)
                            return -1;
                         * */

                        DisplayBrowseItem();
                    }

                    nGeted += temprecords.Count;
                    if (temprecords.Count > 0)
                        records.AddRange(temprecords);

                    if (nGeted >= nCount || temprecords.Count == 0)
                        break;
                }

                return 0;
            }
            finally
            {
                if (this.Stop != null)
                {
                    this.Stop.Style = old_style;
                }
            }
        }

        #region 异步 Present

        public event EventHandler CommandsComplete = null;
        public event EventHandler PresentComplete = null;

        string m_strPresendResultName = "";
        int m_nPresentStart = 0;
        int m_nPresentCount = 0;
        int m_nPreferedEachCount = 0;
        string m_strElementSetName = "";
        string m_strPreferredRecordSyntax = "";
        bool m_bDisplayPresentResult = false;
        RecordCollection m_records = null;
        StopStyle m_oldStopStyle;

        public void SetPresentParameters(string strResultSetName,
    int nStart,
    int nCount,
    int nPreferedEachCount,  // 推荐的每次条数。这涉及到响应的敏捷性
    string strElementSetName,
    string strPreferredRecordSyntax,
    bool bDisplay)
        {
            this.m_strPresendResultName = strResultSetName;
            this.m_nPresentStart = nStart;
            this.m_nPresentCount = nCount;
            /*
             *             if (nCount == 0)
            {
                strError = "nCount为0";
                return 0;
            }
             * */
            this.m_nPreferedEachCount = nPreferedEachCount;
            this.m_strElementSetName = strElementSetName;
            this.m_strPreferredRecordSyntax = strPreferredRecordSyntax;
            this.m_bDisplayPresentResult = bDisplay;
            this.m_records = null;
        }

        public int DoPresentAsync(bool bFirst)
        {
            ZConnection connection = this;

            if (bFirst == true)
            {
                this.m_records = new RecordCollection();

                m_oldStopStyle = StopStyle.None;
                if (this.Stop != null)
                {
                    m_oldStopStyle = this.Stop.Style;
                    this.Stop.Style = StopStyle.EnableHalfStop; // 允许半中断
                }

                if (this.m_nPresentCount > this.ResultCount)
                    this.m_nPresentCount = this.ResultCount;

                if (this.m_nPresentCount == -1)
                    this.m_nPresentCount = this.ResultCount;
            }


            BerTree tree = new BerTree();
            PRESENT_REQUEST struPresent_request = new PRESENT_REQUEST();
            byte[] baPackage = null;
            int nRet;

            // -->
            BerTree tree1 = new BerTree();

            struPresent_request.m_strReferenceId = this.CurrentRefID;
            struPresent_request.m_strResultSetName = this.m_strPresendResultName; // "default";
            struPresent_request.m_lResultSetStartPoint = this.m_nPresentStart + 1;
            struPresent_request.m_lNumberOfRecordsRequested = this.m_nPresentCount;
            struPresent_request.m_strElementSetNames = this.m_strElementSetName;
            struPresent_request.m_strPreferredRecordSyntax = this.m_strPreferredRecordSyntax;

            nRet = tree.PresentRequest(struPresent_request,
                                     out baPackage);
            if (nRet == -1)
            {
                this.ErrorInfo = "CBERTree::PresentRequest() fail!";
                goto ERROR1;
            }


#if DUMPTOFILE
	DeleteFile("presentrequest.bin");
	DumpPackage("presentrequest.bin",
		(char *)baPackage.GetData(),
		baPackage.GetSize());
	DeleteFile ("presentrequest.txt");
	tree.m_RootNode.DumpToFile("presentrequest.txt");
#endif
            ClearSendRecvCompleteEvent();
            connection.ZChannel.SendRecvComplete += new EventHandler(ZChannel_present_SendRecvComplete);
            // 发出请求包，接收响应包
            // return:
            //      -1  出错
            //      0   成功启动
            //      1   发出前，发现流中有未读入的数据
            nRet = connection.ZChannel.SendRecvAsync(baPackage);
            if (nRet != 0)
            {
                this.ErrorInfo = connection.ZChannel.ErrorInfo;
                goto ERROR1;
            }

            return 0;
        ERROR1:
            if (this.PresentComplete != null)
                this.PresentComplete(this, new EventArgs());
            return -1;
        }

        void ClearSendRecvCompleteEvent()
        {
            this.ZChannel.SendRecvComplete -= new EventHandler(ZChannel_initial_SendRecvComplete);
            this.ZChannel.SendRecvComplete -= new EventHandler(ZChannel_search_SendRecvComplete);
            this.ZChannel.SendRecvComplete -= new EventHandler(ZChannel_present_SendRecvComplete);
        }

        void ZChannel_present_SendRecvComplete(object sender, EventArgs e)
        {
            ZConnection connection = this;

            if (string.IsNullOrEmpty(this.ZChannel.ErrorInfo) == false)
            {
                this.ErrorInfo = this.ZChannel.ErrorInfo;
                goto ERROR1;
            }

            byte[] baOutPackage = connection.ZChannel.RecvPackage;


#if DUMPTOFILE	
	DeleteFile("presendresponse.bin");
	DumpPackage("presentresponse.bin",
				(char *)baPackage.GetData(),
				baPackage.GetSize());
#endif
            BerTree tree1 = new BerTree();
            int nTotlen = 0;

            tree1.m_RootNode.BuildPartTree(baOutPackage,
                0,
                baOutPackage.Length,
                out nTotlen);

#if DUMPTOFILE
	DeleteFile("PresentResponse.txt"); 
	tree1.m_RootNode.DumpDebugInfoToFile("PresentResponse.txt");
#endif
            string strError = "";
            SEARCH_RESPONSE search_response = new SEARCH_RESPONSE();
            RecordCollection records = null;
            int nRet = BerTree.GetInfo_PresentResponse(tree1.GetAPDuRoot(),
                                    ref search_response,
                                    out records,
                                    true,
                                    out strError);
            if (nRet == -1)
            {
                this.ErrorInfo = strError;
                goto ERROR1;
            }

            int nGeted = m_records.Count;   // 本次操作前集合的尺寸

            // 2011/11/1
            if (records == null)
                goto ERROR1;

            m_records.AddRange(records);    // 本次

            SetElementSetName(records, this.m_strElementSetName);

            if (search_response.m_diagRecords.Count != 0)
            {
                this.ErrorInfo = "error diagRecords:\r\n\r\n---\r\n" + search_response.m_diagRecords.GetMessage();
                goto ERROR1;
            }

            if (this.m_bDisplayPresentResult == true)
            {
                this.Stop.SetMessage("正在显示已经获取的命中结果 ( " + (m_nPresentStart + 1).ToString() + "-" + (m_nPresentStart + nGeted).ToString() + " of " + this.ResultCount.ToString() + " ) ...");

#if NO
                // 
                // 如果必要，加入新的一批
                if (m_nPresentStart + nGeted >= connection.Records.Count)
                    connection.Records.AddRange(records);
                else
                {
                    for (int i = 0; i < records.Count; i++)
                    {
                        if (i + m_nPresentStart + nGeted >= connection.Records.Count)
                            connection.Records.Add(records[i]); // 追加
                        else
                            connection.Records[i + m_nPresentStart + nGeted] = records[i];   // 替换
                    }
                }
#endif
                // 2013/11/25
                for (int i = 0; i < records.Count; i++)
                {
                    int index = m_nPresentStart + i;    // connection.Records 中的下标
                    while (index >= connection.Records.Count)
                        connection.Records.Add(null);   // 填充空白

                    connection.Records[index] = records[i];   // 替换
                }

                nRet = FillRecordsToVirtualItems(
                    this.Stop,
                    connection.Records,
                    m_nPresentStart,    // m_nPresentStart + nGeted,
                    records.Count,
                    out strError);
                if (nRet == -1)
                {
                    this.ErrorInfo = strError;
                    goto ERROR1;
                }

                DisplayBrowseItem();
            }

            if (nGeted + records.Count < this.m_nPresentCount)
            {
                nRet = DoPresentAsync(false);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                // 结束
                if (this.Stop != null)
                {
                    this.Stop.Style = m_oldStopStyle;
                }

                if (this.PresentComplete != null)
                    this.PresentComplete(this, new EventArgs());
            }

            return;

        ERROR1:
            if (this.PresentComplete != null)
                this.PresentComplete(this, new EventArgs());
        }

        #endregion

        // 2011/9/10
        static void SetElementSetName(RecordCollection records,
            string strElementSetName)
        {
            if (records == null)
                return;

            foreach(DigitalPlatform.Z3950.Record record in records)
            {
                // 非诊断记录
                if (record.m_nDiagCondition == 0)
                {
                    record.m_strElementSetName = strElementSetName;
                }
            }
        }

        // 获得记录
        // 不确保一定可以获得nCount个
        // parameters:
        //		nStart	开始记录(从0计算)
        int DoOncePresent(
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

            ZConnection connection = this;

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

            byte[] baOutPackage = null;
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

            SetElementSetName(records, strElementSetName);

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

        public static string BuildInitialResultInfo(INIT_RESPONSE info)
        {
            string strText = "";

            strText += "reference-id:\t" + info.m_strReferenceId + "\r\n";
            strText += "options:\t" + info.m_strOptions + "\r\n";
            strText += "preferred-message-size:\t" + info.m_lPreferredMessageSize.ToString() + "\r\n";
            strText += "exceptional-record-size:\t" + info.m_lExceptionalRecordSize.ToString() + "\r\n";

            strText += "\r\n--- Result Code ---\r\n";
            strText += "initial-result:\t" + info.m_nResult.ToString() + "\r\n";

            strText += "\r\n--- Implementation information ---\r\n";
            strText += "implementation-id:\t" + info.m_strImplementationId + "\r\n";
            strText += "implementation-name:\t" + info.m_strImplementationName + "\r\n";
            strText += "implementation-version:\t" + info.m_strImplementationVersion + "\r\n";

            strText += "\r\n--- Error Code and Message ---\r\n";
            strText += "error-code:\t" + info.m_lErrorCode.ToString() + "\r\n";
            strText += "error-messsage:\t" + info.m_strErrorMessage + "\r\n";

            if (info.m_charNego != null)
            {
                strText += "\r\n--- Charset Negotiation Parameters ---\r\n";

                if (BerTree.GetBit(info.m_strOptions, 17) == false)
                    strText += "options bit 17 is false, not allow negotiation\r\n";

                strText += "charnego-encoding-level-oid:\t" + info.m_charNego.EncodingLevelOID + "(note: UTF-8 is: " + CharsetNeogatiation.Utf8OID + ")\r\n";

                strText += "charnego-records-in-selected-charsets:\t" + info.m_charNego.RecordsInSelectedCharsets.ToString() + "\r\n";
            }

            return strText;
        }

        // 发送数据前检查连接
        int CheckConnect(
            out string strError)
        {
            strError = "";
            int nRet = 0;

            ZConnection connection = this;

            if (connection.ZChannel.DataAvailable == true)
            {
                string strMessage = "";
                // 处理Server可能发来的Close
                // return:
                //      -1  error
                //      0   不是Close
                //      1   是Close，已经迫使ZChannel处于尚未初始化状态
                nRet = CheckServerCloseRequest(
                    out strMessage,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    Debug.Assert(this.ZChannel.Initialized == false, "");

                    nRet = connection.ZChannel.NewConnectSocket(connection.TargetInfo.HostName,
                        connection.TargetInfo.Port,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    connection.TargetInfo.OnlineServerIcon(true);

                }

                // 2007/8/2 changed
                if (this.ZChannel.Initialized == false)
                {
                    string strInitialResultInfo = "";
                    nRet = this.DoInitial(
                        this.TargetInfo.IgnoreReferenceID,
                        out strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        connection.TargetInfo.OnlineServerIcon(false);
                        return -1;
                    }

                    // 设置当前树上已经选择的节点的扩展信息
                    nRet = ZTargetControl.SetCurrentTargetExtraInfo(
                        this.TreeNode,  // this.zTargetControl1.SelectedNode,
                        strInitialResultInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                }
            }

            return 0;
        }

        // 结果集追加到listview中
        // parameters:
        //      records 当前新获得一批记录。需要追加到connection的Records中
        public int FillRecordsToVirtualItems(
            Stop stop,
            RecordCollection records,
            out string strError)
        {
            ZConnection connection = this;

            VirtualItemCollection items = this.VirtualItems;

            // Debug.Assert(connection == this.GetCurrentZConnection(), "不是当前connection，装入listview会破坏界面");

            strError = "";
            if (connection.Records == null)
                connection.Records = new RecordCollection();

            int nExistCount = connection.Records.Count;
            Debug.Assert(items.Count == nExistCount, "");

            // 加入新的一批
            connection.Records.AddRange(records);

            int nRet = FillRecordsToVirtualItems(
                stop,
                connection.Records,
                nExistCount,
                records.Count,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 把存储在records结构中的信息填入虚拟listview
        // parameters:
        int FillRecordsToVirtualItems(
            Stop stop,
            RecordCollection records,
            int nStart,
            int nCount,
            // ref VirtualItemCollection items,
            out string strError)
        {
            strError = "";

            ZConnection connection = this;

            VirtualItemCollection items = this.VirtualItems;

            if (records == null)
                return 0;

            if (nStart + nCount > records.Count)
            {
                strError = "nStart[" + nStart.ToString() + "]和nCount[" + nCount.ToString() + "]参数之和超出records集合的尺寸[" + records.Count.ToString() + "]";
                return -1;
            }

            for (int i = nStart; i < nStart + nCount; i++)
            {
                // Application.DoEvents();	// 出让界面控制权 BUG 如果在非界面线程中执行会导致 InvalidOperationException

                /*
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                        // TODO: 中断后怎么办？似最后一条记录不代表Records中的最后了
                    }
                }*/

                DigitalPlatform.Z3950.Record record = records[i];

                VirtualItem item = new VirtualItem(
                    (i + 1).ToString(),
                    record.m_nDiagCondition == 0 ? ZSearchForm.BROWSE_TYPE_NORMAL : ZSearchForm.BROWSE_TYPE_DIAG);

                string strBrowseText = "";
                int nRet = 0;

                int nImageIndex = 0;

                nRet = this.IZSearchForm.BuildBrowseText(
                    connection,
                    record,
                    "marc", // 偏向MARC
                    out strBrowseText,
                    out nImageIndex,
                    out strError);
                if (nRet == -1)
                    strBrowseText = strError;


            // DOADD:
                string[] cols = null;
                cols = strBrowseText.Split(new char[] { '\t' });
            // DOADDCOLS:
                for (int j = 0; j < cols.Length; j++)
                {
                    item.SubItems.Add(cols[j]);
                }

                item.Tag = record;
                item.ImageIndex = nImageIndex;

                if (i >= items.Count)
                {

                    // 2013/11/23
                    while (i > items.Count)
                        items.Add(null);

                    // 原来的语句
                    items.Add(item);
                }
                else
                {
                    VirtualItem old_item = items[i];
                    items[i] = item;

                    // 2011/9/11
                    // 继承以前的选择状态
                    if (old_item != null)
                        item.Selected = old_item.Selected;
                }
            }
            return 0;
        }


        // 处理Server可能发来的Close
        // return:
        //      -1  error
        //      0   不是Close
        //      1   是Close，已经迫使ZChannel处于尚未初始化状态
        int CheckServerCloseRequest(
            out string strMessage,
            out string strError)
        {
            strMessage = "";
            strError = "";

            ZConnection connection = this;

            if (connection.ZChannel.DataAvailable == false)
                return 0;

            int nRecvLen = 0;
            byte[] baPackage = null;
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

            connection.CloseConnection();

            return 1;
        }
    }
}
