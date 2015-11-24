using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using System.Xml;
using System.Globalization;
using System.IO;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;
using System.Web.UI.HtmlControls;

public partial class Browse2 : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

    string SelectingNodePath = "";
    // string SelectedNodeCaption = "";

#if NO
    protected override void InitializeCulture()
    {
        WebUtil.InitLang(this);
        base.InitializeCulture();
    }
#endif

    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        if (String.IsNullOrEmpty(sessioninfo.UserID) == true
            || StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
        {
            this.ButtonRefreshCache.Visible = false;
            this.ButtonRefreshAllCache.Visible = false;
            this.ButtonAppendAllCache.Visible = false;
        }

        string strDataFile = (string)this.Session["__browse_data_filename__"];  // 只能使用备用值
        // string strDataFile = BrowseDataFileName.Value;   // 此时取不到的隐藏字段的值

        if (string.IsNullOrEmpty(strDataFile) == false)
        {
            this.TreeView1.XmlFileName = strDataFile;

            this.PanelControl1.Title = GetTitle(strDataFile);
        }

        this.BrowseSearchResultControl1.DefaultFormatName = "详细";
        this.BrowseSearchResultControl1.PageNoUrlMode = true;

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;

        this.TitleBarControl1.CurrentColumn = TitleColumn.Browse;
        this.TitleBarControl1.LibraryCodeChanged -= new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);
        this.TitleBarControl1.LibraryCodeChanged += new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);

    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        this.BrowseSearchResultControl1.ResetAllItemsControlPager();
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            if (this.Page.Request["forcelogin"] == "on")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx", true);
                return;
            }
            if (this.Page.Request["forcelogin"] == "userid")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx?loginstyle=librarian", true);
                return;
            }
            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
        }

        // this.ErrorInfo.Visible = false;

        string strCfgFileName = this.Request["sidebar"];

        if (String.IsNullOrEmpty(strCfgFileName) == true)
            this.SideBarControl1.Visible = false;
        else
            this.SideBarControl1.CfgFile = app.DataDir + "\\browse\\" + strCfgFileName;


        string strDataFileName = this.Request["datafile"];

        if (String.IsNullOrEmpty(strDataFileName) == true)
            strDataFileName = "browse.xml";

        string strAction = this.Request["action"];
        if (strAction == null)
            strAction = "";

        string strNode = this.Request["node"];

        if (strAction.ToLower() == "rss")
        {
            string strError = "";
            int nRet = BuildRssOutput(
                strDataFileName,
                strNode,
                out strError);
            if (nRet == -1)
            {
                this.app.WriteErrorLog(strError);
                this.Response.ContentType = "text/plain";
                this.Response.StatusCode = 500;
                this.Response.Write(strError);
                this.Response.End();
                return;
            }

            this.Response.Flush();
            this.Response.End();
            return;
        }

        if (this.IsPostBack == false
            // 如果数据文件名和参数中的不吻合，需要重新设置数据集
            || (string.IsNullOrEmpty(this.TreeView1.XmlFileName) == false && PathUtil.PureName(this.TreeView1.XmlFileName).ToLower() != strDataFileName.ToLower())
            )
        {
            this.TreeView1.XmlFileName = app.DataDir + "/browse/" + strDataFileName;

            // this.Session["__browse_data_filename__"] = source.DataFile; // store
            this.BrowseDataFileName.Value = strDataFileName; // 通过Web页面的隐藏字段记忆当前的数据文件
            this.Session["__browse_data_filename__"] = this.TreeView1.XmlFileName; // 补充保存在Session中，备用。因为在_Init()阶段是得不到隐藏字段的值的

            this.PanelControl1.Title = GetTitle(this.TreeView1.XmlFileName);
        }

        if (String.IsNullOrEmpty(strNode) == false)
        {
            this.SelectingNodePath = strNode;
        }

        if (this.IsPostBack == false
            && string.IsNullOrEmpty(strDataFileName) == false)
        {
            DisplayNode(strDataFileName, strNode);
        }

        string strPageNo = this.Request["pageno"];
        if (this.IsPostBack == false
            && string.IsNullOrEmpty(strPageNo) == false)
        {
            int nPageNo = 0;
            Int32.TryParse(strPageNo, out nPageNo);
            int nPageIndex = nPageNo - 1;
            if (nPageIndex < 0)
                nPageIndex = 0;
            this.BrowseSearchResultControl1.StartIndex = nPageIndex * this.BrowseSearchResultControl1.PageMaxLines;
        }

        string strFormat = this.Request["format"];
        if (this.IsPostBack == false
            && String.IsNullOrEmpty(strFormat) == false)
        {
            this.BrowseSearchResultControl1.FormatName = strFormat;
        }
    }

    public void Page_Error(object sender, EventArgs e)
    {
        // http://support.microsoft.com/kb/306355
        Exception objErr = Server.GetLastError().GetBaseException();

        if (objErr is ArgumentException)
        {
            Server.ClearError();
            this.SetErrorInfo("请重新点击树节点");
        }
        else
        {
            string err = "<b>Error Caught in Page_Error event</b><hr><br>" +
                    "<br><b>Error in: </b>" + Request.Url.ToString() +
                    "<br><b>Error Message: </b>" + objErr.Message.ToString() +
                    "<br><b>Stack Trace:</b><br>" +
                              objErr.StackTrace.ToString();
            Response.Write(err.ToString());
            Server.ClearError();
        }
    }

    // 设置出错信息
    // parameters:
    //      strText 其中可以包含HTML代码
    void SetErrorInfo(string strText)
    {
        if (String.IsNullOrEmpty(strText) == true)
        {
            this.ErrorInfo.Text = "";
            this.ErrorInfo.Visible = false;
            return;
        }

        this.ErrorInfo.Visible = true;
        this.ErrorInfo.Text = "<div class='errorinfo'>" + strText + "</div>";
    }

    static bool IsParentPath(string strShortPath, string strLongPath)
    {
        string[] parts1 = strShortPath.Split(new char[] {'_'});
        string[] parts2 = strLongPath.Split(new char[] { '_' });

        if (parts1.Length >= parts2.Length)
            return false;

        for (int i = 0; i < Math.Min(parts1.Length, parts2.Length - 1); i++)
        {
            if (parts1[i] != parts2[i])
                return false;
        }

        return true;
    }

    protected void TreeView1_GetNodeData(object sender, GetNodeDataEventArgs e)
    {
        /*
        if (e.Node == e.Node.OwnerDocument.DocumentElement)
            return;
         * */
        if (e.Node.Name[0] == '_')
            return;

        string strName = DomUtil.GetAttr(e.Node, "name");
        if (string.IsNullOrEmpty(strName) == true)
            return;

        bool bCommand = true;
        string strCommand = DomUtil.GetAttr(e.Node, "command");
        if (string.IsNullOrEmpty(strCommand) == false
            && strCommand[0] == '~')
            bCommand = false;

        ///

        string strSideBarFileName = Path.GetFileName(this.SideBarControl1.CfgFile).ToLower();
        string strDataFile = Path.GetFileName(this.TreeView1.XmlFileName).ToLower();
        string strNodePath = CacheBuilder.MakeNodePath(e.Node);

        string strCount = "";

        if (bCommand == true)
        {
            strCount = app.GetBrowseNodeCount(strDataFile, strNodePath);
            if (string.IsNullOrEmpty(strCount) == true
                && app.CacheBuilder != null)
            {
                long lCount = app.CacheBuilder.GetCountByNodePath(strDataFile,
                    strNodePath,
                    false);
                if (lCount == -1)
                    strCount = "?";
                else
                    strCount = lCount.ToString();

                app.SetBrowseNodeCount(strDataFile, strNodePath, strCount);
            }
        }

        if (string.IsNullOrEmpty(strCount) == false)
        {
            // e.Name = strName + " (" + strCount + ")";
            e.Name = strName;
            e.Count = strCount;
        }
        else
            e.Name = strName;


        if (strNodePath == this.SelectingNodePath)
        {
            e.Seletected = true;
            /*
            // 如果有更适合的标题文字
            if (string.IsNullOrEmpty(this.SelectedNodeCaption) == false)
                e.Name = this.SelectedNodeCaption;
             * */
        }

        // TODO: 是否可以给每个节点都显示包含记录的数字? 为了提高速度，是否可以用一个hashtable来存储这个数字对照关系?
        string strSideBarParam = "";
        if (string.IsNullOrEmpty(strSideBarFileName) == false)
            strSideBarParam = "&sidebar=" + HttpUtility.UrlEncode(strSideBarFileName);

        string strFormatParam = "";

            if (string.IsNullOrEmpty(this.BrowseSearchResultControl1.CurrentFormat) == false)
                strFormatParam = "&format=" + HttpUtility.UrlEncode(this.BrowseSearchResultControl1.CurrentFormat);
            else if (string.IsNullOrEmpty(this.BrowseSearchResultControl1.FormatName) == false)
                strFormatParam = "&format=" + HttpUtility.UrlEncode(this.BrowseSearchResultControl1.FormatName);


        e.Url = "./browse.aspx?datafile="+HttpUtility.UrlEncode(strDataFile)+ strSideBarParam + "&node=" + strNodePath + strFormatParam;

        if (e.Node == e.Node.OwnerDocument.DocumentElement
            || IsParentPath(strNodePath, this.SelectingNodePath) == true)
            e.Closed = false;
    }

    static XmlNode GetFirstNode(XmlNode parent)
    {
        if (parent.Name[0] == '_')
            return null;    // 下级也不用进入了
        {
            string strName = DomUtil.GetAttr(parent, "name");
            if (string.IsNullOrEmpty(strName) == false)
                return parent;
        }

        foreach (XmlNode node in parent.ChildNodes)
        {
            if (node.NodeType != XmlNodeType.Element)
                continue;

            XmlNode result = GetFirstNode(node);
            if (result != null)
                return result;
        }

        return null;
    }

    void DisplayNode(string strDataFileName, string strNodePath)
    {
        int nRet = 0;
        string strError = "";

        this.BrowseSearchResultControl1.SelectAll(false);

        string strXmlFilePath = app.DataDir + "/browse/" + strDataFileName;
        XmlDocument dom = new XmlDocument();
        dom.Load(strXmlFilePath);

        // 2014/12/2
        // 兑现宏
        nRet = CacheBuilder.MacroDom(dom,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        XmlNode node = null;
        if (string.IsNullOrEmpty(strNodePath) == true)
        {
            // 找到第一个节点
            node = GetFirstNode(dom.DocumentElement);
            if (node == null)
                return;
            strNodePath = CacheBuilder.MakeNodePath(node);
        }
        else
        {
            // TODO: 也可以用节点文字名字来选定
            node = CacheBuilder.GetDataNode(dom.DocumentElement, strNodePath);
            if (node == null)
                return;
        }

        this.TreeView1.SelectedNodePath = "";

        this.SelectingNodePath = strNodePath;
        string strPureCaption = DomUtil.GetAttr(node, "name");

        string strDescription = DomUtil.GetAttr(node, "description");
        if (String.IsNullOrEmpty(strDescription) == true)
            this.Description1.Text = "";
        else
            this.Description1.Text = "<div class='text'>" + strDescription + "</div>";

        string strCommand = DomUtil.GetAttr(node, "command");

        bool bRss = false;
        long nMaxCount = -1;
        string strDirection = "";
        // parameters:
        //      nMaxCount   -1表示无穷多
        //      strDirection    head/tail
        CacheBuilder.GetRssParam(node,
            out bRss,
            out nMaxCount,
            out strDirection);

        // this.HyperLink_rss.Visible = bRss;

        if (strCommand == "~hidelist~")
        {
            // 
            this.BrowseSearchResultControl1.ResultSetName = "";
            this.BrowseSearchResultControl1.ResultsetFilename = "";
            this.BrowseSearchResultControl1.ResultCount = 0;
            this.BrowseSearchResultControl1.StartIndex = 0;
            this.BrowseSearchResultControl1.Visible = false;
            return;
        }

        this.BrowseSearchResultControl1.Visible = true;

        if (strCommand == "~none~")
        {
            // 
            this.BrowseSearchResultControl1.ResultSetName = "";
            this.BrowseSearchResultControl1.ResultsetFilename = "";
            this.BrowseSearchResultControl1.ResultCount = 0;
            this.BrowseSearchResultControl1.StartIndex = 0;
            return;
        }

#if NO
        string strPureCaption = this.TreeView1.SelectedNode.Text;
        nRet = strPureCaption.IndexOf("(");
        if (nRet != -1)
            strPureCaption = strPureCaption.Substring(0, nRet).Trim();
#endif

        string strDataFile = strDataFileName;  //  PathUtil.PureName(strDataFile);

        string strPrefix = CacheBuilder.MakeNodePath(node);
        string strCacheDir = app.DataDir + "/browse/cache/" + strDataFile;

        PathUtil.CreateDirIfNeed(strCacheDir);
        string strResultsetFilename = strCacheDir + "/" + strPrefix;

        string strRssString = "datafile=" + strDataFile + "&node=" + strPrefix;
        // this.HyperLink_rss.NavigateUrl = "browse.aspx?action=rss&" + strRssString;
        string strRssNavigateUrl = "browse.aspx?action=rss&" + strRssString;

        bool bRedo = false;

    REDO:
        // 如果文件已经存在，就不要从rmsws获取了
        try
        {
            app.ResultsetLocks.LockForRead(strResultsetFilename, 500);
            try
            {
                if (this.Response.IsClientConnected == false)
                    return;

                if (File.Exists(strResultsetFilename) == true)
                {
                    // 2010/12/21
                    string strBuildStyle = "";
                    // 看看是否为每日强制更新的节点 
                    // 获得Build相关参数
                    // parameters:
                    //      strBuildStyle    创建风格 perday / perhour
                    CacheBuilder.GetBuildParam(node,
                        out strBuildStyle);
                    if (String.IsNullOrEmpty(strBuildStyle) == false
                        && strBuildStyle.ToLower() != "disable")
                    {
                        // 比较文件创建时间和当前时间，看看是否超过重建周期
                        if (CacheBuilder.HasExpired(strResultsetFilename,
            strBuildStyle) == true)
                            goto DO_REBUILD;
                    }

                    long lHitCount = CacheBuilder.GetCount(app, strResultsetFilename, false);

                    // 记忆下来
                    app.SetBrowseNodeCount(strDataFile, strNodePath, lHitCount.ToString());

                    this.BrowseSearchResultControl1.ResultsetFilename = strResultsetFilename;
                    this.BrowseSearchResultControl1.ResultCount = (int)lHitCount;
                    this.BrowseSearchResultControl1.StartIndex = 0;
                    this.CreateRssLink(strPureCaption, strRssNavigateUrl);
                    this.Page.Title = strPureCaption; 
                    // this.SelectedNodeCaption = strPureCaption + "(" + lHitCount.ToString() + ")";
                    this.TreeView1.SelectedNodePath = strNodePath;
                    return;
                }
            }
            finally
            {
                app.ResultsetLocks.UnlockForRead(strResultsetFilename);
            }
        }
        catch (System.ApplicationException /*ex*/)
        {
            this.SetErrorInfo(strPureCaption + " 相关缓存文件暂时被占用，请稍后重新访问");
            goto END1;
        }

    DO_REBUILD:

        if (bRedo == false)
        {
            /*
            // 加入列表
            lock (app.PendingCacheFiles)
            {
                string strLine = strDataFile + ":" + strPrefix;
                if (app.PendingCacheFiles.IndexOf(strLine) == -1)
                    app.PendingCacheFiles.Add(strLine);
            }
            app.ActivateCacheBuilder();
             * */
            CacheBuilder.AddToPendingList(app,
                    strDataFile,
                    strPrefix,
                    "");


            bRedo = true;
            if (Wait() == true)
                return;
            goto REDO;
        }
        else
        {
            this.SetErrorInfo(strPureCaption + " 相关缓存正在建立，请稍后重新访问");
            // this.SelectedNodeCaption = strPureCaption;
        }

    END1:
        this.CreateRssLink(strPureCaption, strRssNavigateUrl);
        this.Page.Title = strPureCaption;

        this.BrowseSearchResultControl1.ResultSetName = "";
        this.BrowseSearchResultControl1.ResultsetFilename = "";

        this.BrowseSearchResultControl1.ResultCount = 0;
        this.BrowseSearchResultControl1.StartIndex = 0;
        this.TreeView1.SelectedNodePath = strNodePath;

        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
    }

    void CreateRssLink(
        string strTitle,
        string strRssNavigateUrl)
    {
        if (string.IsNullOrEmpty(strRssNavigateUrl) == true)
            return;

        this.BrowseSearchResultControl1.Title = "<div class='resulttitle'><div class='text'>" + strTitle + "</div>" + "<div class='rss'><a class='rss' href='" + strRssNavigateUrl + "' title='RSS订阅'><img src='"+MyWebPage.GetStylePath(app, "rss.gif") + "'></img></a></div><div class='clear'></div>" + "</div>";

        HtmlLink link = new HtmlLink();
        link.Href = strRssNavigateUrl;
        link.Attributes.Add("rel", "alternate");
        link.Attributes.Add("type", "application/rss+xml");
        link.Attributes.Add("title", strTitle);

        this.Page.Header.Controls.Add(link);
    }

    // return:
    //      true    已经中断
    //      false   尚未中断
    bool Wait()
    {
        for (int i = 0; i < 100; i++)
        {
            Thread.Sleep(10);
            if (this.Response.IsClientConnected == false)
                return true;
        }
        return false;
    }

    // 刷新一个缓存
    protected void ButtonRefreshCache_Click(object sender, EventArgs e)
    {
        int nRet = 0;
        string strError = "";

        string strNodePath = this.TreeView1.SelectedNodePath;

        if (string.IsNullOrEmpty(strNodePath) == true)
        {
            this.SetErrorInfo("尚未选定要刷新缓存的树节点");
            return;
        }

        if (String.IsNullOrEmpty(sessioninfo.UserID) == true
    || StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
        {
            this.SetErrorInfo("当前用户不具备 managecache 权限，不能刷新缓存");
            return;
        }

        string strDataFileName = this.BrowseDataFileName.Value;

        string strXmlFilePath = app.DataDir + "/browse/" + strDataFileName;
        XmlDocument dom = new XmlDocument();
        dom.Load(strXmlFilePath);

        // 2014/12/2
        // 兑现宏
        nRet = CacheBuilder.MacroDom(dom,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        XmlNode node = CacheBuilder.GetDataNode(dom.DocumentElement, strNodePath);
        if (node == null)
            return;

        string strPrefix = CacheBuilder.MakeNodePath(node);
        // TODO: 需要把权限检查也放在AddToPendingList()等函数中
        nRet = CacheBuilder.AddToPendingList(app,
            strDataFileName,
            strPrefix,
            "");
        this.SetErrorInfo("刷新成功。共创建了" + nRet.ToString() + "  个新队列事项");

        // this.SelectedNodeCaption = "";
        
        // app.ClearBrowseNodeCount(strDataFileName, strNodePath);

        {
            this.BrowseSearchResultControl1.ResultSetName = "";
            this.BrowseSearchResultControl1.ResultsetFilename = "";

            this.BrowseSearchResultControl1.ResultCount = 0;
            this.BrowseSearchResultControl1.StartIndex = 0;
        }

        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
    }

    protected void ButtonRefreshAllCache_Click(object sender, EventArgs e)
    {
        string strError = "";

        if (String.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
        {
            this.SetErrorInfo("当前用户不具备 managecache 权限，不能刷新缓存");
            return;
        }

        // TODO: 检查，防止越界
        string strDataFileName = this.BrowseDataFileName.Value;

        int nRet = CacheBuilder.RefreshAll(app,
            strDataFileName,
            false,
            out strError);
        if (nRet == -1)
        {
            this.SetErrorInfo(strError);
            return;
        }

        this.SetErrorInfo("刷新成功。共创建了" + nRet.ToString() + "  个新队列事项");

        // app.ClearBrowseNodeCount(strDataFileName, "");

        if (string.IsNullOrEmpty(this.TreeView1.SelectedNodePath) == false)
        {
            this.BrowseSearchResultControl1.ResultSetName = "";
            this.BrowseSearchResultControl1.ResultsetFilename = "";
            this.BrowseSearchResultControl1.ResultCount = 0;
            this.BrowseSearchResultControl1.StartIndex = 0;

            this.TreeView1.SelectedNodePath = "";
            this.SelectingNodePath = "";
            // this.SelectedNodeCaption = "";
            return;
        }

        return;
    }

    protected void ButtonAppendAllCache_Click(object sender, EventArgs e)
    {
        string strError = "";

        if (String.IsNullOrEmpty(sessioninfo.UserID) == true
|| StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
        {
            this.SetErrorInfo("当前用户不具备 managecache 权限，不能增补缓存");
            return;
        }

        // TODO: 检查，防止越界
        string strDataFileName = this.BrowseDataFileName.Value;

        int nRet = CacheBuilder.RefreshAll(app,
            strDataFileName,
            true,
            out strError);
        if (nRet == -1)
        {
            this.SetErrorInfo(strError);
            return;
        }

        this.SetErrorInfo("增补成功。共创建了" + nRet.ToString() + "  个新队列事项");
        return;
    }

    static string GetTitle(string strXmlFileName)
    {
        try
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFileName);
            }
            catch
            {
                return "";
            }

            if (dom.DocumentElement == null)
                return "";
            XmlNode node = dom.DocumentElement.SelectSingleNode("_title");
            if (node == null)
                return "";
            return DomUtil.GetCaption(Lang, node);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    int BuildRssOutput(
    string strDataFile,
    string strNode,
    out string strError)
    {
        int nRet = 0;
        strError = "";

        string strDataFilePath = app.DataDir + "/browse/" + strDataFile;

        XmlDocument dom = new XmlDocument();
        try
        {
            dom.Load(strDataFilePath);
        }
        catch (Exception ex)
        {
            strError = "装载文件 '" + strDataFilePath + "' 时出错: " + ex.Message;
            return -1;
        }

        // 2014/12/2
        // 兑现宏
        nRet = CacheBuilder.MacroDom(dom,
            out strError);
        if (nRet == -1)
            return -1;

        XmlNode node = CacheBuilder.GetDataNode(dom.DocumentElement,
            strNode);

        if (node == null)
        {
            strError = "路径 '" + strNode + "' 在文件 '" + strDataFile + "' 中没有找到对应的节点";
            return -1;
        }

        bool bRss = false;
        long nMaxCount = -1;
        string strDirection = "";
        // parameters:
        //      nMaxCount   -1表示无穷多
        //      strDirection    head/tail
        CacheBuilder.GetRssParam(node,
            out bRss,
            out nMaxCount,
            out strDirection);

        if (bRss == false)
        {
            strError = "此节点不允许输出RSS";
            return -1;
        }
        string strCommand = DomUtil.GetAttr(node, "command");
        string strPureCaption = DomUtil.GetAttr(node, "name");
        string strDescription = DomUtil.GetAttr(node, "description");

        if (strCommand == "~hidelist~")
        {
            strError = "此节点 ~hidelist~ 不允许输出RSS";
            return -1;
        }

        if (strCommand == "~none~")
        {
            strError = "此节点 ~none~ 不允许输出RSS";
            return -1;

        }

        // strDataFile 中为纯文件名
        string strPrefix = strNode;
        string strCacheDir = app.DataDir + "/browse/cache/" + strDataFile;

        PathUtil.CreateDirIfNeed(strCacheDir);
        string strResultsetFilename = strCacheDir + "/" + strPrefix;


        // 如果RSS文件已经存在，就不要从rmsws获取了
        if (File.Exists(strResultsetFilename + ".rss") == true)
        {
            // return:
            //      -1  出错
            //      0   成功
            //      1   暂时不能访问
            nRet = DumpRssFile(strResultsetFilename + ".rss",
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 加入列表，要求创建RSS文件
        /*
        lock (app.PendingCacheFiles)
        {
            string strLine = strDataFile + ":" + strNode + ":rss";  // 仅仅要求创建RSS文件
            if (app.PendingCacheFiles.IndexOf(strLine) == -1)
                app.PendingCacheFiles.Add(strLine);
        }
        app.ActivateCacheBuilder();
         * */
        CacheBuilder.AddToPendingList(app,
            strDataFile,
            strNode,
            "rss");

        // "相关缓存正在被创建，请稍后重新访问";
        strError = "RSS file is building, please retry soon later... 相关缓存正在被创建，请稍后重新访问";
        this.Response.ContentType = "text/plain; charset=utf-8";
        this.Response.StatusCode = 503;
        this.Response.StatusDescription = strError;
        this.Response.Write(strError);
        return 0;
    }

    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }

    // return:
    //      -1  出错
    //      0   成功
    //      1   暂时不能访问
    int DumpRssFile(string strRssFile,
        out string strError)
    {
        strError = "";

        // 不让浏览器缓存页面
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");

        this.Response.ContentType = "application/rss+xml";

        try
        {
            app.ResultsetLocks.LockForRead(strRssFile, 500);

            try
            {

                using (Stream stream = File.Open(strRssFile,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite))
                {
                    this.Response.AddHeader("Content-Length", stream.Length.ToString());

                    FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                    stream.Seek(0, SeekOrigin.Begin);

                    StreamUtil.DumpStream(stream, this.Response.OutputStream,
                        flushdelegate);
                }
            }
            finally
            {
                app.ResultsetLocks.UnlockForRead(strRssFile);
            }
        }
        catch (System.ApplicationException /*ex*/)
        {
            this.Response.ContentType = "text/plain";
            this.Response.StatusCode = 503;
            this.Response.StatusDescription = "相关的XML缓存正在被创建，请稍后重新访问";
            return 1;
        }

        return 0;
    }
}