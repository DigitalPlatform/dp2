using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;

using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// dp2资源树控件
    /// </summary>
    public partial class dp2ResTree : System.Windows.Forms.TreeView
    {
        public CfgCache cfgCache = null;

        public bool SortTableChanged = false;
        public Hashtable sort_tables = new Hashtable();

        DigitalPlatform.Stop m_current_stop = null;

        public string Lang = "zh";

        public dp2ServerCollection Servers = null;	// 引用
        public LibraryChannelCollection Channels = null;    // 引用

        public bool TestMode = false;   // 是否为评估模式

        public DigitalPlatform.StopManager stopManager = null;

        /// <summary>
        /// 通讯通道
        /// </summary>
        LibraryChannel channel = null;

        public int[] EnabledIndices = null;	// null表示全部发黑。如果对象存在，但是元素个数为0，表示全部发灰

        #region	资源类型。可作Icon下标用

        public const int RESTYPE_SERVER = 2;
        public const int RESTYPE_DB = 0;
        public const int RESTYPE_FROM = 1;
        public const int RESTYPE_LOADING = 3;
        public const int RESTYPE_FOLDER = 4;
        public const int RESTYPE_FILE = 5;

        #endregion

        public dp2ResTree()
        {
            InitializeComponent();
        }

        // 2011/11/23
        // 当前用户名
        // 最后一次使用过Channel的当前用户名
        public string CurrentUserName
        {
            get
            {
                if (this.channel == null)
                    return "";
                return this.channel.UserName;
            }
        }

        // 刷新风格
        [Flags]
        public enum RefreshStyle
        {
            All = 0xffff,
            Servers = 0x01,
            Selected = 0x02,
        }

        void Rearrange(List<string> order_table,
            ref List<NormalDbProperty> properties)
        {
            List<NormalDbProperty> result = new List<NormalDbProperty>();

            foreach (string strName in order_table)
            {
                foreach (NormalDbProperty property in properties)
                {
                    if (property.DbName == strName)
                    {
                        result.Add(property);
                        properties.Remove(property);
                        break;
                    }
                }
            }

            // 剩下的
            result.AddRange(properties);
            properties = result;
        }

        void Rearrange(List<string> order_table,
        ref List<string> froms,
        ref List<String> styles)
        {
            List<string> result_froms = new List<string>();
            List<string> result_styles = new List<string>();

            foreach (string strName in order_table)
            {
                foreach (string from in froms)
                {
                    if (strName == from)
                    {
                        int pos = froms.IndexOf(from);

                        result_froms.Add(from);
                        froms.RemoveAt(pos);

                        result_styles.Add(styles[pos]);
                        styles.RemoveAt(pos);
                        break;
                    }
                }
            }

            // 剩下的
            result_froms.AddRange(froms);
            result_styles.AddRange(styles);

            froms = result_froms;
            styles = result_styles;
        }

        public int Fill(TreeNode node)
        {
            string strError = "";
            try
            {
                int nRet = Fill(node, out strError);
                if (nRet == -1)
                {
                    try
                    {
                        MessageBox.Show(this,
                            strError);
                    }
                    catch
                    {
                        // this可能已经不存在
                    }
                    return -1;
                }
                return nRet;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
        }

        // 递归
        public int Fill(TreeNode node,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.Servers == null)
            {
                strError = "this.Servers == null";
                return -1;
            }

            this.Enabled = false;
            this.Update();
            try
            {
                TreeNodeCollection children = null;

                if (node == null)
                {
                    children = this.Nodes;
                }
                else
                {
                    children = node.Nodes;
                }

                // 填充根
                if (node == null)
                {
                    children.Clear();

                    for (int i = 0; i < Servers.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        dp2Server server = (dp2Server)Servers[i];
                        TreeNode nodeNew = new TreeNode(server.Name,
                            RESTYPE_SERVER,
                            RESTYPE_SERVER);
                        SetLoading(nodeNew);

                        if (EnabledIndices != null
                            && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                            nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                        children.Add(nodeNew);

                        dp2ServerNodeInfo info = new dp2ServerNodeInfo();
                        info.Name = server.Name;
                        info.Url = server.Url;

                        nodeNew.Tag = info;
                    }

                    return 0;
                }

                // *** 根以下的节点类型

                // 服务器
                if (node.ImageIndex == RESTYPE_SERVER)
                {
                    Application.DoEvents();	// 出让界面控制权

                    ResPath respath = new ResPath(node);

                    List<NormalDbProperty> properties = null;
                    // string[] dbnames = null;
                    nRet = GetDbNames(
                        respath.Url,
                        // out dbnames,
                        out properties,
                        out strError);
                    if (nRet == -1)
                    {
                        if (node != null)
                        {
                            SetLoading(node);	// 出错的善后处理，重新出现+号
                            node.Collapse();
                        }
                        goto ERROR1;
                    }

                    children.Clear();

                    List<string> order_table = (List<string>)this.sort_tables[respath.FullPath];

                    if (order_table != null)
                    {
                        Rearrange(order_table,
        ref properties);
                    }

                    for (int i = 0; i < properties.Count; i++)
                    {
                        NormalDbProperty prop = properties[i];

                        string strDbName = prop.DbName;
                        TreeNode nodeNew = new TreeNode(strDbName,
                            RESTYPE_DB, RESTYPE_DB);

                        // nodeNew.Tag = items[i].TypeString;  // 类型字符串

                        SetLoading(nodeNew);

                        if (EnabledIndices != null
                            && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                            nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                        children.Add(nodeNew);

                        nodeNew.Tag = prop;
                    }

                }

                // 数据库
                if (node.ImageIndex == RESTYPE_DB)
                {
                    ResPath respath = new ResPath(node);

                    List<string> froms = null;
                    List<string> styles = null;
                    nRet = GetFroms(
                        respath.Url,
                        respath.Path,
                        out froms,
                        out styles,
                        out strError);
                    if (nRet == -1)
                    {
                        if (node != null)
                        {
                            SetLoading(node);	// 出错的善后处理，重新出现+号
                            node.Collapse();
                        }
                        goto ERROR1;
                    }

                    Debug.Assert(froms.Count == styles.Count, "");

                    children.Clear();

                    List<string> order_table = (List<string>)this.sort_tables[respath.FullPath];

                    if (order_table != null)
                    {
                        Rearrange(order_table,
        ref froms,
        ref styles);
                    }


                    for (int i = 0; i < froms.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        string strFrom = froms[i];
                        TreeNode nodeNew = new TreeNode(strFrom,
                            RESTYPE_FROM, RESTYPE_FROM);

                        dp2FromInfo info = new dp2FromInfo();
                        info.Caption = strFrom;
                        info.Style = styles[i];

                        nodeNew.Tag = info;  // dp2FromInfo类

                        // SetLoading(nodeNew);

                        if (EnabledIndices != null
                            && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                            nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                        children.Add(nodeNew);
                    }
                }
            }
            finally
            {
                this.Enabled = true;
            }
            return 0;
        ERROR1:
            return -1;
        }

        // 将Hashtable对象转换为便于存储的字符串
        public static string SaveSortTables(Hashtable sort_table)
        {
            if (sort_table == null)
                return "";

            StringBuilder strResult = new StringBuilder(4096);
            foreach (string key in sort_table.Keys)
            {
                List<string> values = (List<string>)sort_table[key];
                if (values == null)
                    continue;
                string strValues = StringUtil.MakePathList(values);
                if (strResult.Length > 0)
                    strResult.Append(";");
                strResult.Append(key + "=" + strValues);
            }

            return strResult.ToString();
        }

        // 将字符串恢复为Hashtable对象
        public static Hashtable RestoreSortTables(string strText)
        {
            Hashtable table = new Hashtable();
            if (String.IsNullOrEmpty(strText) == true)
                return table;

            string[] parts = strText.Split(new char[] {';'});
            foreach (string part in parts)
            {
                string strName = "";
                string strValues = "";
                int nRet = part.IndexOf("=");
                if (nRet == -1)
                    strName = part;
                else
                {
                    strName = part.Substring(0, nRet);
                    strValues = part.Substring(nRet + 1);
                }

                table[strName] = StringUtil.SplitList(strValues);
            }

            return table;
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        // 根据路径逐步展开
        // 注意：respath的Url中是服务器的URL，而TreeView的第一级为服务器名，不一样
        public void ExpandPath(ResPath respath)
        {

            string[] aName = respath.Path.Split(new Char[] { '/' });

            TreeNode node = null;
            TreeNode nodeThis = null;


            string[] temp = new string[aName.Length + 1];
            Array.Copy(aName, 0, temp, 1, aName.Length);
            temp[0] = respath.Url;

            aName = temp;

            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = this.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    TreeNode currrent_node = nodes[j];

                    string strName = "";
                    if (currrent_node.Tag is dp2ServerNodeInfo)
                    {
                        // 如果是server节点，则需要取出URL
                        // 2008/3/9
                        dp2ServerNodeInfo info = (dp2ServerNodeInfo)currrent_node.Tag;
                        if (info == null)
                        {
                            // 希望不会出现这种情况
                            strName = currrent_node.Text;
                            Debug.Assert(false, "server类型的node Tag为空");
                        }
                        else
                            strName = info.Url;
                    }
                    else
                    {
                        // 如果是其他节点，取节点正文即可
                        strName = currrent_node.Text;
                    }

                    if (aName[i] == strName)
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
                    Fill(node);
                }
                node.Expand();  // 即便最终层次没有找到，也要展开中间层次
            }

            if (nodeThis != null && nodeThis.Parent != null)
                nodeThis.Parent.Expand();

            this.SelectedNode = nodeThis;
        }


        // 解析记录路径出的各部分
        // 记录路径有两种形式：正序和倒序
        // 正序 本地服务器/中文图书/1
        // 倒序 中文图书/1@本地服务器
        public static void ParseRecPath(string strFullPath,
            out string strServerName,
            out string strPath)
        {
            int nRet = strFullPath.IndexOf("@");
            if (nRet == -1)
            {
                // 表明是正序
                nRet = strFullPath.IndexOf("/");
                if (nRet == -1)
                {
                    strServerName = strFullPath;
                    strPath = "";
                    return;
                }

                strServerName = strFullPath.Substring(0, nRet);
                strPath = strFullPath.Substring(nRet + 1);
                return;
            }

            // 否则是倒序
            strPath = strFullPath.Substring(0, nRet).Trim();
            strServerName = strFullPath.Substring(nRet + 1).Trim();

        }

        // 根据全路径逐步展开
        // 路径有两种形式：正序和倒序
        // 正序 本地服务器/中文图书/1
        // 倒序 中文图书/1@本地服务器
        public void ExpandPath(string strFullPath)
        {
            string strServerName = "";
            string strPath = "";

            ParseRecPath(strFullPath,
                out strServerName,
                out strPath);

            string[] aName = strPath.Split(new Char[] { '/' });

            TreeNode node = null;
            TreeNode nodeThis = null;

            string[] temp = new string[aName.Length + 1];
            Array.Copy(aName, 0, temp, 1, aName.Length);
            temp[0] = strServerName;

            aName = temp;

            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = this.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
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
                    Fill(node);
                }
                node.Expand();  // 即便最终层次没有找到，也要展开中间层次
            }

            if (nodeThis != null && nodeThis.Parent != null)
                nodeThis.Parent.Expand();

            this.SelectedNode = nodeThis;
        }

        // 在一个节点下级插入"loading..."，以便出现+号
        public static void SetLoading(TreeNode node)
        {
            if (node == null)
                return;

            // 新node
            TreeNode nodeNew = new TreeNode("loading...", RESTYPE_LOADING, RESTYPE_LOADING);

            node.Nodes.Clear();
            node.Nodes.Add(nodeNew);
        }

        // 下级是否包含loading...?
        public static bool IsLoading(TreeNode node)
        {
            if (node.Nodes.Count == 0)
                return false;

            if (node.Nodes[0].Text == "loading...")
                return true;

            return false;
        }

        public void Refresh(RefreshStyle style)
        {
            ResPath OldPath = null;
            bool bExpanded = false;

            bool bFocused = this.Focused;

            // 保存
            if (this.SelectedNode != null)
            {
                OldPath = new ResPath(this.SelectedNode);
                bExpanded = this.SelectedNode.IsExpanded;
            }

            // 刷新服务器级
            if ((style & RefreshStyle.Servers) == RefreshStyle.Servers)
            {
                this.Fill(null);
            }

            // 刷新当前选择的节点
            if (OldPath != null
                && (style & RefreshStyle.Selected) == RefreshStyle.Selected)
            {
                ResPath respath = OldPath.Clone();

                // 刷新

                respath.Path = "";
                ExpandPath(respath);	// 选中服务器，以下节点清除
                SetLoading(this.SelectedNode);

                ExpandPath(OldPath);
                if (bExpanded == true && this.SelectedNode != null)
                    this.SelectedNode.Expand();
            }

            if (this.Focused != bFocused)
                this.Focus();
        }

        delegate int Delegate_Fill(TreeNode node);

        private void dp2ResTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
                return;

            // 需要展开
            if (IsLoading(node) == true)
            {
                // this.Fill(node);

                this.Update();

                object[] pList = new object[] { node };
                this.BeginInvoke(new Delegate_Fill(this.Fill), pList);

            }
        }

        public void Stop()
        {
            if (this.m_current_stop != null)
                this.m_current_stop.DoStop();
        }

        // 获得一个服务器下的全部数据库名
        int GetDbNames(
            string strServerUrl,
            // out string [] dbnames,
            out List<NormalDbProperty> properties,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            properties = new List<NormalDbProperty>();

            if (this.Channels == null)
            {
                strError = "this.Channels == null";
                return -1;
            }

            this.channel = this.Channels.GetChannel(strServerUrl);
            if (this.channel == null)
            {
                strError = "GetChannel() error. strServerUrl '"+strServerUrl+"'";
                return -1;
            }

            DigitalPlatform.Stop stop = null;
            if (this.stopManager != null)
            {
                stop = new DigitalPlatform.Stop();
                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获得服务器 "+strServerUrl+" 的库名列表 ...");
                stop.BeginLoop();

                this.m_current_stop = stop;
            }

            try
            {
#if NO
                string strValue = "";
                long lRet = this.channel.GetSystemParameter(stop,
                    "biblio",
                    "dbnames",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + this.channel.Url + " 获得编目库名列表过程发生错误：" + strError;
                    return -1;
                }

                string [] dbnames = strValue.Split(new char [] {','});
                for (int i = 0; i < dbnames.Length; i++)
                {
                    string strDbName = dbnames[i].Trim();
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;
                    NormalDbProperty prop = new NormalDbProperty();
                    prop.DbName = strDbName;
                    properties.Add(prop);
                }
#endif

                double version = 0;
                // return:
                //      -1  error
                //      0   dp2Library的版本号过低。警告信息在strError中
                //      1   dp2Library版本号符合要求
                nRet = LibraryChannel.GetServerVersion(
                    this.channel,
                    stop,
                    out version,
                    out strError);
                if (nRet != 1)
                    return -1;

                if (this.TestMode == true && version < 2.34)
                {
                    strError = "dp2 前端的评估模式只能在所连接的 dp2library 版本为 2.34 以上时才能使用 (当前 dp2library 版本为 " + version.ToString() + ")";
                    return -1;
                }

                double base_version = 2.60;

                // 检查 dp2library 最低版本要求 2.60
                if (version < base_version) // 2.48
                {
                    strError = "dp2 前端所连接的 dp2library 版本必须升级为 " + base_version + " 以上时才能使用 (当前 dp2library 版本为 " + version.ToString() + ")";
                    return -1;
                }

                string strValue = "";
                long lRet = this.channel.GetSystemParameter(stop,
                    "system",
                    "biblioDbGroup",
                    out strValue,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + this.channel.Url + " 获得编目库配置信息的过程发生错误：" + strError;
                    return -1;
                }

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                try
                {
                    dom.DocumentElement.InnerXml = strValue;
                }
                catch (Exception ex)
                {
                    strError = "category=system,name=biblioDbGroup所返回的XML片段在装入InnerXml时出错: " + ex.Message;
                    return -1;
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    string strDbName = DomUtil.GetAttr(node, "biblioDbName");
                    if (String.IsNullOrEmpty(strDbName) == true)
                        continue;

                    NormalDbProperty prop = new NormalDbProperty();
                    properties.Add(prop);
                    prop.DbName = strDbName;

                    prop.ItemDbName = DomUtil.GetAttr(node, "itemDbName");
                    prop.Syntax = DomUtil.GetAttr(node, "syntax");
                    prop.IssueDbName = DomUtil.GetAttr(node, "issueDbName");
                    prop.OrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    prop.CommentDbName = DomUtil.GetAttr(node, "commentDbName");
                    prop.Role = DomUtil.GetAttr(node, "role");

                    bool bValue = true;
                    nRet = DomUtil.GetBooleanParam(node,
                        "inCirculation",
                        true,
                        out bValue,
                        out strError);
                    prop.InCirculation = bValue;
                }

                if (properties.Count > 0)
                {
                    // return:
                    //      -1  出错，不希望继续以后的操作
                    //      0   成功
                    nRet = GetBrowseColumns(
                        this.channel,
                        stop,
                        ref properties,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }
            finally
            {
                if (this.stopManager != null)
                {
                    this.m_current_stop = null;

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    stop.Unregister();	// 和容器脱离关联
                }
            }

            // this.channel = null;
            return 0;
        }

        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        public int GetBrowseColumns(
            LibraryChannel Channel,
            Stop stop,
            ref List<NormalDbProperty> properties,
            out string strError)
        {
            strError = "";

            // Stop.Initial("正在获得普通库属性列表 ...");


            // 获得browse配置文件
            for (int i = 0; i < properties.Count; i++)
            {
                Application.DoEvents();	// 出让界面控制权

                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                NormalDbProperty normal = properties[i];

                // normal.ColumnNames = new List<string>();
                normal.ColumnNames = new ColumnPropertyCollection();

                string strContent = "";
                byte[] baCfgOutputTimestamp = null;
                int nRet = GetCfgFile(
                    Channel,
                    stop,   // stop
                    normal.DbName,
                    "browse",
                    out strContent,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    return -1;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strContent);
                }
                catch (Exception ex)
                {
                    strError = "数据库 " + normal.DbName + " 的browse配置文件内容装入XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//col");
                foreach (XmlNode node in nodes)
                {
                    string strColumnType = DomUtil.GetAttr(node, "type");
                    // 2013/10/23
                    string strColumnTitle = GetColumnTitle(node,
                        this.Lang); 
                    // normal.ColumnNames.Add(strColumnTitle);
                    normal.ColumnNames.Add(strColumnTitle, strColumnType);
                }
            }

            return 0;
        }

        // 获得 col 元素的 title 属性值，或者下属的语言相关的 title 元素值
        /*
<col>
	<title>
		<caption lang='zh-CN'>书名</caption>
		<caption lang='en'>Title</caption>
	</title>
         * */
        public static string GetColumnTitle(XmlNode nodeCol,
            string strLang = "zh")
        {
            string strColumnTitle = DomUtil.GetAttr(nodeCol, "title");
            if (string.IsNullOrEmpty(strColumnTitle) == false)
                return strColumnTitle;
            XmlNode nodeTitle = nodeCol.SelectSingleNode("title");
            if (nodeTitle == null)
                return "";
            return DomUtil.GetCaption(strLang, nodeTitle);
        }

        // 列出服务器节点名字
        public List<string> GetServerNames()
        {
            // 需要展开
            if (this.Nodes.Count == 0)
            {
                string strError = "";
                int nRet = Fill(null, out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }

            List<string> results = new List<string>();
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                TreeNode currrent_node = this.Nodes[i];
                Debug.Assert(currrent_node.Tag is dp2ServerNodeInfo, "");
                dp2ServerNodeInfo info = (dp2ServerNodeInfo)currrent_node.Tag;

                results.Add(info.Name);
            }

            return results;
        }

        public List<string> GetDbNames(string strServerName)
        {
            TreeNode server_node = FindServer(strServerName);
            if (server_node == null)
            {
                throw new Exception("没有找到名为 '"+strServerName+"' 的服务器节点");
            }

            // 需要展开
            if (IsLoading(server_node) == true)
            {
                string strError = "";
                int nRet = Fill(server_node,out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            } 
            
            List<string> results = new List<string>();

            Debug.Assert(server_node != null, "");
            for (int i = 0; i < server_node.Nodes.Count; i++)
            {
                TreeNode node = server_node.Nodes[i];
                results.Add(node.Text);
            }

            return results;
        }

        public NormalDbProperty GetDbProperty(string strServerName,
    string strDbName)
        {
            TreeNode server_node = FindServer(strServerName);
            if (server_node == null)
            {
                throw new Exception("没有找到名为 '" + strServerName + "' 的服务器节点");
            }

            string strError = "";
            // 需要展开
            if (IsLoading(server_node) == true)
            {
                int nRet = Fill(server_node, out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }

            TreeNode db_node = FindDb(server_node, strDbName);
            if (db_node == null)
            {
                throw new Exception("在服务器 '" + strServerName + "' 下没有找到名为 '" + strDbName + "' 的数据库节点");
            }

            return (NormalDbProperty)db_node.Tag;
        }

        public List<string> GetFromNames(string strServerName,
            string strDbName)
        {
            TreeNode server_node = FindServer(strServerName);
            if (server_node == null)
            {
                throw new Exception("没有找到名为 '" + strServerName + "' 的服务器节点");
            }

            string strError = "";
            // 需要展开
            if (IsLoading(server_node) == true)
            {
                int nRet = Fill(server_node, out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }

            TreeNode db_node = FindDb(server_node, strDbName);
            if (db_node == null)
            {
                throw new Exception("在服务器 '" + strServerName + "' 下没有找到名为 '" + strDbName + "' 的数据库节点");
            }

            // 需要展开
            if (IsLoading(db_node) == true)
            {
                int nRet = Fill(db_node, out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }

            List<string> results = new List<string>();

            Debug.Assert(db_node != null, "");
            for (int i = 0; i < db_node.Nodes.Count; i++)
            {
                TreeNode node = db_node.Nodes[i];
                results.Add(node.Text);
            }

            return results;
        }

        // 找到服务器节点
        TreeNode FindServer(string strServerUrlOrName)
        {
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                TreeNode currrent_node = this.Nodes[i];
                Debug.Assert(currrent_node.Tag is dp2ServerNodeInfo, "");
                dp2ServerNodeInfo info = (dp2ServerNodeInfo)currrent_node.Tag;

                if (info.Url == strServerUrlOrName
                    || info.Name == strServerUrlOrName)
                    return currrent_node;
            }

            return null;
        }

        static TreeNode FindDb(TreeNode server_node, string strDbName)
        {
            Debug.Assert(server_node != null, "");
            for (int i = 0; i < server_node.Nodes.Count; i++)
            {
                TreeNode node = server_node.Nodes[i];
                if (node.Text == strDbName)
                    return node;
            }

            return null;
        }

        public ColumnPropertyCollection GetBrowseColumnNames(string strServerUrlOrName,
            string strDbName)
        {
            // 找到服务器节点
            TreeNode node_server = this.FindServer(strServerUrlOrName);
            if (node_server == null)
                return null;    // not found server

            // 找到数据库节点
            TreeNode node_db = FindDb(node_server, strDbName);
            if (node_db == null)
                return null;    // not found db

            NormalDbProperty prop = (NormalDbProperty)node_db.Tag;

            return prop.ColumnNames;
        }

        // 获得配置文件
        public int GetCfgFile(
            LibraryChannel Channel,
            Stop stop,
            string strDbName,
            string strCfgFileName,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strContent = "";

            /*
            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }*/

            /*
            if (stop != null)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载配置文件 ...");
                stop.BeginLoop();
            }*/

            // m_nInGetCfgFile++;

            try
            {
                string strPath = strDbName + "/cfgs/" + strCfgFileName;

                if (stop != null)   // 2006/12/16
                    stop.SetMessage("正在下载配置文件 " + strPath + " ...");

                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = 0;

                if (this.cfgCache != null)
                lRet = Channel.GetRes(stop,
                    this.cfgCache,
                    strPath,
                    strStyle,
                    null,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                else
                lRet = Channel.GetRes(stop,
    strPath,
    strStyle,
    out strContent,
    out strMetaData,
    out baOutputTimestamp,
    out strOutputPath,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                /*
                if (stop != null)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }*/

                // m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 获得一个数据库下的检索途径
        int GetFroms(
            string strServerUrl,
            string strDbName,
            out List<string> froms,
            out List<string> styles,
            out string strError)
        {
            froms = new List<string>();
            styles = new List<string>();

            strError = "";

            // Debug.Assert(false, "");

            this.channel = this.Channels.GetChannel(strServerUrl);
            Debug.Assert(this.channel != null, "");

            DigitalPlatform.Stop stop = null;
            if (this.stopManager != null)
            {
                stop = new DigitalPlatform.Stop();
                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获得库 "+strDbName+" 的检索途径列表 ...");
                stop.BeginLoop();
                this.m_current_stop = stop;
            }

            try
            {
                BiblioDbFromInfo[] infos = null;

                long lRet = this.channel.ListDbFroms(stop,
                    "biblio",
                    this.Lang,
                    out infos,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + this.channel.Url + " 列出书目库检索途径过程发生错误：" + strError;
                    return -1;
                }

                for (int i = 0; i < infos.Length; i++)
                {
                    froms.Add(infos[i].Caption);
                    styles.Add(infos[i].Style);
                }
            }
            finally
            {
                if (this.stopManager != null)
                {
                    this.m_current_stop = null;
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    stop.Unregister();	// 和容器脱离关联
                }
            }

            // this.channel = null;

            return 0;
        }

        // 过滤掉 _ 开头的那些style子串
        // parameters:
        //      bRemove2    是否滤除 __ 前缀的
        //      bRemove1    是否滤除 _ 前缀的
        public static string GetDisplayFromStyle(string strStyles,
            bool bRemove2,
            bool bRemove1)
        {
            string[] parts = strStyles.Split(new char[] { ',' });
            List<string> results = new List<string>();
            foreach (string part in parts)
            {
                string strText = part.Trim();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;

                if (strText[0] == '_')
                {
                    if (bRemove1 == true)
                    {
                        if (strText.Length >= 2 && /*strText[0] == '_' &&*/ strText[1] != '_')
                            continue;
#if NO
                        if (strText[0] == '_')
                            continue;
#endif
                        if (strText.Length == 1)
                            continue;
                    }

                    if (bRemove2 == true && strText.Length >= 2)
                    {
                        if (/*strText[0] == '_' && */ strText[1] == '_')
                            continue;
                    }
                }


                results.Add(strText);
            }

            return StringUtil.MakePathList(results, ",");
        }

        public static int GetNodeInfo(TreeNode node,
            out string strServerName,
            out string strServerUrl,
            out string strDbName,
            out string strFrom,
            out string strFromStyle,
            out string strError)
        {
            strError = "";
            strServerName = "";
            strServerUrl = "";
            strDbName = "";
            strFrom = "";
            strFromStyle = "";

            List<TreeNode> node_path = new List<TreeNode>();
            while (true)
            {
                if (node == null)
                    break;
                node_path.Insert(0, node);
                node = node.Parent;
            }

            if (node_path.Count > 0)
            {
                dp2ServerNodeInfo server_info = (dp2ServerNodeInfo)node_path[0].Tag;
                strServerName = server_info.Name;
                strServerUrl = server_info.Url;
            }

            if (node_path.Count > 1)
            {
                strDbName = node_path[1].Text;
            }

            if (node_path.Count > 2)
            {
                dp2FromInfo from_info = (dp2FromInfo)node_path[2].Tag;
                strFrom = from_info.Caption;
                strFromStyle = from_info.Style;
            }


            return 0;
        }

        static bool IsFirstChild(TreeNode node)
        {
            if (node == null)
                return false;
            TreeNodeCollection nodes = null;
            if (node.Parent != null)
                nodes = node.Parent.Nodes;
            else
                nodes = node.TreeView.Nodes;

            if (nodes.IndexOf(node) == 0)
                return true;

            return false;
        }

        static bool IsLastChild(TreeNode node)
        {
            if (node == null)
                return false;

            TreeNodeCollection nodes = null;
            if (node.Parent != null)
                nodes = node.Parent.Nodes;
            else
                nodes = node.TreeView.Nodes;

            if (nodes.IndexOf(node) == nodes.Count - 1)
                return true;

            return false;
        }

        private void dp2ResTree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            TreeNode node = this.SelectedNode;

            //
            menuItem = new MenuItem("上移(&U)");
            menuItem.Click += new System.EventHandler(this.button_moveUp_Click);
            if (node == null || IsFirstChild(node) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("下移(&D)");
            menuItem.Click += new System.EventHandler(this.button_moveDown_Click);
            if (node == null || IsLastChild(node) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            // 
            menuItem = new MenuItem("允许复选(&M)");
            menuItem.Click += new System.EventHandler(this.menu_toggleCheckBoxes);
            if (this.CheckBoxes == true)
                menuItem.Checked = true;
            else
                menuItem.Checked = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除全部复选(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearCheckBoxes);
            if (this.CheckBoxes == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refresh);
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this, new Point(e.X, e.Y));	
        }

        // 刷新
        public void menu_refresh(object sender, System.EventArgs e)
        {
            this.Refresh(RefreshStyle.All);
        }

        void menu_clearCheckBoxes(object sender, System.EventArgs e)
        {
            this.ClearChildrenCheck(null);
        }

        // 清除下级所有的选中的项(不包括自己)
        // parameters:
        //      nodeStart   起点node。如果为null, 表示从根层开始，清除全部
        public void ClearChildrenCheck(TreeNode nodeStart)
        {
            TreeNodeCollection nodes = null;
            if (nodeStart == null)
            {
                nodes = this.Nodes;
            }
            else
                nodes = nodeStart.Nodes;

            foreach (TreeNode node in nodes)
            {
                node.Checked = false;
                ClearChildrenCheck(node);	// 递归
            }
        }

        void menu_toggleCheckBoxes(object sender, System.EventArgs e)
        {
            if (this.CheckBoxes == true)
            {
                this.ClearChildrenCheck(null);
                this.CheckBoxes = false;
            }
            else
                this.CheckBoxes = true;
        }

        void button_moveUp_Click(object sender, EventArgs e)
        {
            MoveUpOrDown(this.SelectedNode,
    true);

        }

        void button_moveDown_Click(object sender, EventArgs e)
        {
            MoveUpOrDown(this.SelectedNode,
false);

        }

        bool MoveUpOrDown(TreeNode node,
            bool bUp)
        {
            TreeNodeCollection nodes = null;
            if (node.Parent != null)
                nodes = node.Parent.Nodes;
            else
                nodes = node.TreeView.Nodes;

            int nOldIndex = nodes.IndexOf(node);
            int nNewIndex = -1;
            if (bUp == true)
            {
                nNewIndex = nOldIndex - 1;
                if (nNewIndex < 0)
                    return false;
            }
            else
            {
                nNewIndex = nOldIndex + 1;
                if (nNewIndex >= nodes.Count)
                    return false;
            }

            nodes.RemoveAt(nOldIndex);
            nodes.Insert(nNewIndex, node);

            if (node.Tag is dp2ServerNodeInfo)
            {
                dp2Server server = (dp2Server)this.Servers[nOldIndex];
                this.Servers.RemoveAt(nOldIndex);
                this.Servers.Insert(nNewIndex, server);
                this.Servers.Changed = true;
            }
            else
            {
                // 更新sort_tables
                if (node.Tag is NormalDbProperty
                    || node.Tag is dp2FromInfo)
                {
                    ResPath respath = new ResPath(node.Parent);
                    List<string> values = new List<string>();
                    foreach (TreeNode temp in node.Parent.Nodes)
                    {
                        values.Add(temp.Text);
                    }
                    this.sort_tables[respath.FullPath] = values;
                    this.SortTableChanged = true;
                }
            }

            this.SelectedNode = node;

            return true;
        }

        // 按下键
        protected override void OnKeyDown(KeyEventArgs e)
        {
            Debug.WriteLine(e.KeyCode.ToString());

            switch (e.KeyCode)
            {
                case Keys.Up:
                    {
                        if (e.Modifiers == Keys.Control
                            && this.SelectedNode != null)
                        {
                            MoveUpOrDown(this.SelectedNode,
                true);
                        }
                    }
                    break;
                case Keys.Down:
                    {
                        if (e.Modifiers == Keys.Control
                            && this.SelectedNode != null)
                        {
                            MoveUpOrDown(this.SelectedNode,
                false);
                        }
                    }
                    break;
                default:
                    break;
            }

            base.OnKeyDown(e);
        }

        // 清除下级所有的选中的项(不包括自己)
        public static void ClearOneLevelChildrenCheck(TreeNode nodeStart)
        {
            if (nodeStart == null)
                return;
            foreach (TreeNode node in nodeStart.Nodes)
            {
                node.Checked = false;
                // ClearChildrenCheck(node);	// 暂时不递归
            }
        }

        private void dp2ResTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;

            if (e.Node.Checked == true)
            {
                e.Node.BackColor = Color.Yellow;
                e.Node.ForeColor = Color.Black;

                e.Node.NodeFont = new Font(this.Font, FontStyle.Bold);
            }
            else
            {
                e.Node.BackColor = this.BackColor;
                e.Node.ForeColor = this.ForeColor;

                e.Node.NodeFont = null;
            }

            if (e.Node.Checked == false)
            {
                ClearOneLevelChildrenCheck(e.Node);
            }
            else
            {
                if (e.Node.Parent != null)
                    e.Node.Parent.Checked = true;
            }

            // 注：事件自己会递归
        }

        // 第一阶段。负责产生TargetItem的Url和Target内容
        // 根据树上的选择状态生成检索目标字符串
        // 不同的服务器中的字符串分开放
        // return:
        //      -1  出错
        //      0   尚未选定检索目标
        //      1   成功
        public int GetSearchTarget(out TargetItemCollection result_items,
            out string strError)
        {
            strError = "";
            result_items = new TargetItemCollection();

            if (this.CheckBoxes == false)
            {
                List<TreeNode> aNode = new List<TreeNode>();
                TreeNode node = this.SelectedNode;
                if (node == null)
                {
                    strError = "尚未选定检索目标";
                    return 0;
                }

                for (; node != null; )
                {
                    aNode.Insert(0, node);
                    node = node.Parent;
                }

                if (aNode.Count == 0)
                    goto END1;


                TargetItem item = new TargetItem();
                item.Lang = this.Lang;

                result_items.Add(item);

                item.ServerName = ((TreeNode)aNode[0]).Text;
                // 获得server url
                dp2Server server = this.Servers.GetServerByName(item.ServerName);
                if (server == null)
                {
                    strError = "名为 '" + item.ServerName + "' 的服务器在检索窗中尚未定义...";
                    return -1;
                }
                item.Url = server.Url;

                if (aNode.Count == 1)
                    goto END1;

                item.Target = ((TreeNode)aNode[1]).Text;


                if (aNode.Count == 2)
                    goto END1;

                item.Target += ":" + ((TreeNode)aNode[2]).Text;
            END1:
                return 1;
            }

            // 找选中的服务器
            foreach (TreeNode nodeServer in this.Nodes)
            {
                if (nodeServer.Checked == false)
                    continue;

                int nDbCount = 0;   // checked的数据库对象个数
                string strTargetList = "";

                // 找选中的数据库
                foreach (TreeNode nodeDb in nodeServer.Nodes)
                {
                    if (nodeDb.Checked == false)
                        continue;

                    if (nodeDb.ImageIndex != RESTYPE_DB)
                        continue;   // 因为可能有配置文件目录或者文件对象需要跳过

                    nDbCount++;

                    if (strTargetList != "")
                        strTargetList += ";";
                    strTargetList += nodeDb.Text + ":";

                    // 用一个strFrom新变量，可以很好地处理逗号
                    string strFrom = "";
                    // 找选中的from
                    foreach (TreeNode nodeFrom in nodeDb.Nodes)
                    {
                        if (nodeFrom.Checked == true)
                        {
                            if (strFrom != "")
                                strFrom += ",";
                            strFrom += nodeFrom.Text;
                        }
                    }
                    strTargetList += strFrom;
                }

                if (nDbCount == 0)
                {
                    strError = "需在服务器 '" + nodeServer .Text+ "' 节点下级勾选一个或者多个数据库节点，检索方可进行。如果不希望检索此服务器节点，则需清除其勾选状态。";
                    return -1;
                }

                TargetItem item = new TargetItem();
                item.ServerName = nodeServer.Text;

                // 获得server url
                dp2Server server = this.Servers.GetServerByName(item.ServerName);
                if (server == null)
                {
                    strError = "名为 '" + item.ServerName + "' 的服务器在检索窗中尚未定义...";
                    return -1;
                }

                item.Url = server.Url;
                item.Target = strTargetList;
                item.Lang = this.Lang;

                result_items.Add(item);

                strTargetList = "";
            }

            if (result_items.Count == 0)
            {
                strError = "尚未选定检索目标";
                return 0;
            }

            return 1;
        }
    }

    // 检索目标事项
    public class TargetItem
    {
        public string Lang = "";

        public string ServerName = "";  // 服务器表意文字
        public string Url = "";         // 服务器URL

        public string Target = "";	// 检索目标字符串，例如"库1:from1,from2;库2:from1,from2"

        public string Words = "";	// 原始态的检索词,尚未切割
        public string[] aWord = null;	// MakeWordPhrases()加工后的字符串
        public string Xml = "";
        public int MaxCount = -1;	// 检索的最大条数
    }

    // 检索目标容器
    public class TargetItemCollection : List<TargetItem>
    {
        // 获得数据库名列表
        // 每个数据库名的格式为 "数据库名@服务器名"
        public int GetDbNameList(out List<string> dbnames,
            out string strError)
        {
            strError = "";
            dbnames = new List<string>();

            foreach (TargetItem item in this)
            {
                List<string> current_dbnames = ParseDbNamesInTargetString(item.Target);
                StringUtil.RemoveDupNoSort(ref current_dbnames);
                foreach (string strDbName in current_dbnames)
                {
                    dbnames.Add(strDbName + "@" + item.ServerName);
                }
            }

            StringUtil.RemoveDupNoSort(ref dbnames);
            return 0;
        }

        // 从Target字符串中剖析出数据库名
        public static List<string> ParseDbNamesInTargetString(string strTargetString)
        {
            List<string> results = new List<string>();
            string[] parts = strTargetString.Split(new char[] {';'});
            foreach (string strPart in parts)
            {
                string strText = strPart.Trim();
                string strDbName = "";
                int nRet = strText.IndexOf(":");
                if (nRet != -1)
                    strDbName = strText.Substring(0, nRet);
                else
                    strDbName = strText;

                results.Add(strDbName);
            }

            return results;
        }

        // 第二阶段: 根据每个TargetItem中Words中原始形态的检索词，切割为string[] aWord
        // 调用本函数前，应当为每个TargetItem对象设置好Words成员值
        // 第二阶段和第一阶段先后顺序不重要。
        public int MakeWordPhrases(
            string strDefaultMatchStyle = "left",
            bool bSplitWords = true,
            bool bAutoDetectRange = true,
            bool bAutoDetectRelation = true)
        {
            for (int i = 0; i < this.Count; i++)
            {
                TargetItem item = this[i];
                item.aWord = MakeWordPhrases(item.Words,
                    strDefaultMatchStyle,
                    bSplitWords,
                    bAutoDetectRange,
                    bAutoDetectRelation);
            }

            return 0;
        }


        // 第三阶段：根据每个TargetItem中的Target和aWord，构造出Xml内容
        public int MakeXml()
        {
            string strText = "";
            for (int i = 0; i < this.Count; i++)
            {
                TargetItem item = this[i];

                strText = "";

                string strCount = "";

                if (item.MaxCount != -1)
                    strCount = "<maxCount>" + Convert.ToString(item.MaxCount) + "</maxCount>";

                for (int j = 0; j < item.aWord.Length; j++)
                {
                    if (j != 0)
                    {
                        strText += "<operator value='OR' />";
                    }

                    strText += "<item>" + item.aWord[j] + strCount + "</item>";
                }

                strText = "<target list='"
                    + StringUtil.GetXmlStringSimple(item.Target)       // 2007/9/14
                    + "'>" + strText
                    + "<lang>" + item.Lang + "</lang></target>";

                item.Xml = strText;
            }

            return 0;
        }

        // 匹配左右括号
        static bool MatchTailQuote(char left, char right)
        {
            if (left == '“' && right == '”')
                return true;
            if (left == '‘' && right == '’')
                return true;

            if (left == '\'' && right == '\'')
                return true;

            if (left == '"' && right == '"')
                return true;

            return false;
        }

        // 按照空格切割出检索词
        static List<string> SplitWords(string strWords)
        {
            List<string> results = new List<string>();
            string strWord = "";
            bool bInQuote = false;
            char chQuote = '\'';
            for (int i = 0; i < strWords.Length; i++)
            {
                if ("\'\"“”‘’".IndexOf(strWords[i]) != -1)
                {
                    if (bInQuote == true
                        && MatchTailQuote(chQuote, strWords[i]) == true)
                    {
                        bInQuote = false;
                        continue;   // 在结果中忽略这个符号
                    }
                    else if (bInQuote == false)
                    {
                        bInQuote = true;
                        chQuote = strWords[i];
                        continue;   // 在结果中忽略这个符号
                    }
                }

                if ((strWords[i] == ' ' || strWords[i] == '　')
                    && bInQuote == false
                    && String.IsNullOrEmpty(strWord) == false)
                {
                    results.Add(strWord);
                    strWord = "";
                }
                else
                {
                    strWord += strWords[i];
                }
            }

            if (String.IsNullOrEmpty(strWord) == false)
            {
                results.Add(strWord);
                strWord = "";
            }


            return results;
        }

        //将"***-***"拆分成两部分
        public static int SplitRangeID(string strRange,
            out string strID1,
            out string strID2)
        {
            int nPosition;
            nPosition = strRange.IndexOf("-");
            strID1 = "";
            strID2 = "";
            if (nPosition > 0)
            {
                strID1 = strRange.Substring(0, nPosition).Trim();
                strID2 = strRange.Substring(nPosition + 1).Trim();
                if (strID2 == "")
                    strID2 = "9999999999";
            }
            if (nPosition == 0)
            {
                strID1 = "0";
                strID2 = strRange.Substring(1).Trim();
            }
            if (nPosition < 0)
            {
                strID1 = strRange.Trim();
                strID2 = strRange.Trim();
            }
            return 0;
        }

        // 根据一个检索词字符串，按照空白切割成单个检索词，
        // 并且根据检索词是否为数字等等猜测出其它检索参数，构造成
        // 含<item>内部分元素的字符串。调用者然后可增加<target>等元素，
        // 最终构成完整的<item>字符串
        public static string[] MakeWordPhrases(string strWords,
            string strDefaultMatchStyle = "left",
            bool bSplitWords = true,
            bool bAutoDetectRange = true,
            bool bAutoDetectRelation = true)
        {
            /*
			string[] aWord;
			aWord = strWords.Split(new Char [] {' '});
             */
            List<string> aWord = null;

            if (bSplitWords == true)
                aWord = SplitWords(strWords);

            if (aWord == null || aWord.Count == 0)
            {
                aWord = new List<string>();
                aWord.Add(strWords);
            }

            string strXml = "";
            string strWord = "";
            string strMatch = "";
            string strRelation = "";
            string strDataType = "";

            ArrayList aResult = new ArrayList();

            foreach (string strOneWord in aWord)
            {
                if (bAutoDetectRange == true)
                {
                    string strID1;
                    string strID2;

                    SplitRangeID(strOneWord, out strID1, out strID2);
                    if (StringUtil.IsNum(strID1) == true
                        && StringUtil.IsNum(strID2) && strOneWord != "")
                    {
                        strWord = strOneWord;
                        strMatch = "exact";
                        strRelation = "draw";
                        strDataType = "number";
                        goto CONTINUE;
                    }
                }


                if (bAutoDetectRelation == true)
                {
                    string strOperatorTemp;
                    string strRealText;

                    int ret;
                    ret = GetPartCondition(strOneWord,
                        out strOperatorTemp,
                        out strRealText);

                    if (ret == 0 && strOneWord != "")
                    {
                        strWord = strRealText;
                        strMatch = "exact";
                        strRelation = strOperatorTemp;
                        if (StringUtil.IsNum(strRealText) == true)
                            strDataType = "number";
                        else
                            strDataType = "string";
                        goto CONTINUE;
                    }
                }

                strWord = strOneWord;
                strMatch = strDefaultMatchStyle;
                strRelation = "=";
                strDataType = "string";
            CONTINUE:

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strXml += "<word>"
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word>"
                    + "<match>" + strMatch + "</match>"
                    + "<relation>" + strRelation + "</relation>"
                    + "<dataType>" + strDataType + "</dataType>";

                aResult.Add(strXml);

                strXml = "";
            }
            string[] array = new string[aResult.Count];
            aResult.CopyTo(array);

            return array;

            // return ConvertUtil.GetStringArray(0, aResult);
        }

        // 根据表示式，得到操作符和值
        // return:
        //		0	有关系操作符
        //		-1	无关系操作符				
        public static int GetPartCondition(string strText,
            out string strOperator,
            out string strRealText)
        {
            strText = strText.Trim();
            strOperator = "=";
            strRealText = strText;
            int nPosition;
            nPosition = strText.IndexOf(">=");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);

                strOperator = ">=";
                return 0;
            }
            nPosition = strText.IndexOf("<=");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<=";
                return 0;
            }
            nPosition = strText.IndexOf("<>");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<>";
                return 0;
            }

            nPosition = strText.IndexOf("><");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<>";
                return 0;
            }
            nPosition = strText.IndexOf("!=");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<>";
                return 0;
            }
            nPosition = strText.IndexOf(">");
            int nPosition2 = strText.IndexOf(">=");
            if (nPosition2 < 0 && nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 1);
                strOperator = ">";
                return 0;
            }
            nPosition = strText.IndexOf("<");
            nPosition2 = strText.IndexOf("<=");
            if (nPosition2 < 0 && nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 1);
                strOperator = "<";
                return 0;
            }
            return -1;
        }
    }

    // 树形控件Server类型节点Tag内信息结构
    public class dp2ServerNodeInfo
    {
        public string Name = "";    // 显示出来的名字
        public string Url = ""; // dp2library URL。例如http://localhost:8001/dp2library
    }

    /*
    public class dp2ServerInfo
    {
        public string Url = "";
        public string Name = "";
        public List<dp2DbInfo> DbInfos = null;
    }

    public class dp2DbInfo
    {
        public string DbName = "";
        public dp2FromInfo FromInfo = null;
    }
     * */

    // From节点Tag内信息
    public class dp2FromInfo
    {
        public string Caption = "";
        public string Style = "";

    }

    // 普通库的属性
    public class NormalDbProperty
    {
        public string DbName = "";
        // public List<string> ColumnNames = new List<string>();
        public ColumnPropertyCollection ColumnNames = new ColumnPropertyCollection();
        
        public string Syntax = "";  // 格式语法
        public string ItemDbName = "";  // 对应的实体库名

        public string IssueDbName = ""; // 对应的期库名
        public string OrderDbName = ""; // 对应的订购库名

        public string CommentDbName = "";   // 对应的评注库名
        public string Role = "";    // 角色
        public bool InCirculation = true;  // 是否参与流通
    }
}
