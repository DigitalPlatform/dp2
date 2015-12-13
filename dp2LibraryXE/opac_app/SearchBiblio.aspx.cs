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
using System.Runtime.Serialization.Json;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using System.Collections;
using DigitalPlatform.LibraryClient;

public partial class SearchBiblio : MyWebPage
{
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
ref this.app,
ref this.sessioninfo) == false)
            return;

        this.TitleBarControl1.CurrentColumn = TitleColumn.Search;

        this.TitleBarControl1.LibraryCodeChanged -= new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);
        this.TitleBarControl1.LibraryCodeChanged += new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);

        this.BiblioSearchControl1.DefaultHiddenMatchStyle = "middle";  // 当匹配方式列隐藏时，是依中间一致来进行的。
        this.BiblioSearchControl1.DefaultVisibleMatchStyle = "middle";  // 当匹配方式列出现时，是依中间一致来进行的。
        /*
        this.AdvanceSearchControl1.SearchPanelStyle = SearchPanelStyle.Advance
            | SearchPanelStyle.MatchStyleColumn
            | SearchPanelStyle.PanelStyleSwitch;
         * */
        // this.BiblioSearchControl1.SearchPanelStyle |= SearchPanelStyle.QueryXml;

        this.BrowseSearchResultControl1.DefaultFormatName = "详细";
        this.BrowseSearchResultControl1.Visible = false;
        this.filter.Visible = false;

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        this.BrowseSearchResultControl1.ResetAllItemsControlPager();
    }

    void SetSideBarVisible()
    {
        bool bItemSearchVisible = true;
        bool bCommentSearchVisible = true;
        XmlNode nodeItemSearch = app.WebUiDom.DocumentElement.SelectSingleNode("itemSearch");
        if (nodeItemSearch != null)
            bItemSearchVisible = DomUtil.GetBooleanParam(nodeItemSearch,
                "visible",
                true);
        XmlNode nodeCommentSearch = app.WebUiDom.DocumentElement.SelectSingleNode("commentSearch");
        if (nodeCommentSearch != null)
            bCommentSearchVisible = DomUtil.GetBooleanParam(nodeCommentSearch,
                "visible",
                true);
        if (bItemSearchVisible == false && bCommentSearchVisible == false)
        {
            this.SideBarControl1.Visible = false;
        }
    }

    void VisibleFilter(bool bVisible)
    {
        if (bVisible == false)
        {
            this.filter.Visible = false;
            return;
        }

        string strFilePath = PathUtil.MergePath(app.DataDir, "cfgs/facet.fltx");
        if (File.Exists(strFilePath) == false)
            this.filter.Visible = false;
        else
            this.filter.Visible = true;
    }



    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;


        string strSideBarFile = Server.MapPath("./search_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        SetSideBarVisible();

        /*
        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }
         * */

        if (this.BrowseSearchResultControl1.ResultCount > 0)
        {
            this.BrowseSearchResultControl1.Visible = true;
            VisibleFilter(true);
        }

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
            /*
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
             * */
        }

        string strError = "";
        int nRet = 0;

#if NO
        string strAction = this.Request["action"];
        if (strAction == "getdblist")
        {
            DoGetDbNameList();
            return;
        }
#endif

        // 如果有参数
        string strWord = this.Request["word"];
        if (String.IsNullOrEmpty(strWord) == false
            && this.IsPostBack == false)
        {
            string strXml = "";

            // string strWord = "";
            string strDbName = "";
            string strFrom = "";
            string strMatchStyle = "";

            GetSearchParams(out strWord,
                out strDbName,
                out strFrom,
                out strMatchStyle);

            // 根据检索参数创建XML检索式
            nRet = OpacApplication.BuildQueryXml(
                this.app,
                strDbName,  // this.Request["dbname"],
                strWord,
                strFrom,    // this.Request["from"],
                strMatchStyle,  // this.Request["matchstyle"],
                null,
                null,
                app.SearchMaxResultCount,
                this.BiblioSearchControl1.SearchStyle, // strSearchStyle
                out strXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strResultSetNamePrefix = "";

            strResultSetNamePrefix = this.Request["resultsetname"];
            if (String.IsNullOrEmpty(strResultSetNamePrefix) == true)
            {
                strResultSetNamePrefix = Session.SessionID + "_opac_1";
            }
            else
            {
                strResultSetNamePrefix = Session.SessionID + "_" + strResultSetNamePrefix;
            }

            string strResultSetName = GetResultSetName(strResultSetNamePrefix);

            sessioninfo.Channel.Idle += new IdleEventHandler(channel_Idle);
            try
            {
                long lRet = sessioninfo.Channel.Search(
                    null,
                    strXml,
                    strResultSetName,
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                sessioninfo.SetFilterTask(strResultSetName, null);

                if (app.SearchLog != null)
                {
                    SearchLogItem log = new SearchLogItem();
                    log.IP = this.Request.UserHostAddress.ToString();
                    log.Query = SearchLog.BuildLogQueryString(
                        this.Request["dbname"],
                        strWord,
                        this.Request["from"],
                        this.Request["matchstyle"]);
                    log.Time = DateTime.UtcNow;
                    log.HitCount = nRet;
                    log.Format = "searchcount";
                    app.SearchLog.AddLogItem(log);
                }

                // not found
                if (lRet == 0)
                {
                    this.BrowseSearchResultControl1.Visible = false;
                    this.filter.Visible = false;
                    strError = "没有找到";
                    goto ERROR1;
                }

                this.BrowseSearchResultControl1.Clear();
                this.BrowseSearchResultControl1.Visible = true;
                VisibleFilter(true);

                this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
                this.BrowseSearchResultControl1.ResultCount = (int)lRet;
                this.BrowseSearchResultControl1.StartIndex = 0; // 2008/12/15

                this.filter.ResultSetName = strResultSetName;

                string strFormat = this.Request["format"];
                if (String.IsNullOrEmpty(strFormat) == false)
                {
                    this.BrowseSearchResultControl1.FormatName = strFormat;
                }
                return;
            }
            finally
            {
                sessioninfo.Channel.Idle -= new IdleEventHandler(channel_Idle);
            }
        }

        // 观察特定的结果集
        if (this.IsPostBack == false)
        {
            string strResultSet = this.Request["resultset"];
            string strBaseResultSet = this.Request["base"];
            string strTitle = this.Request["title"];
            if (string.IsNullOrEmpty(strResultSet) == false)
            {
                string strResultsetFilename = PathUtil.MergePath(sessioninfo.GetTempDir(), strResultSet);
                if (File.Exists(strResultsetFilename) == true)
                {
                    long lHitCount = CacheBuilder.GetCount(app, strResultsetFilename, true);

                    this.BrowseSearchResultControl1.ResultsetFilename = strResultsetFilename;

                    this.BrowseSearchResultControl1.Visible = true;
                    VisibleFilter(true);

                    this.filter.ResultSetName = strBaseResultSet;

                    string strOffset = this.Request["offset"];
                    this.BrowseSearchResultControl1.ResultsetOffset = strOffset;

                    this.filter.SelectedNodePath = MakeSelectedPath(strResultSet, strOffset);

                    if (string.IsNullOrEmpty(strOffset) == false)
                    {
                        int nStart = 0;
                        int nLength = -1;
                        BrowseSearchResultControl.ParseOffsetString(strOffset,
            out nStart,
            out nLength);
                        if (nLength == -1)
                            nLength = (int)lHitCount - nStart;
                        this.BrowseSearchResultControl1.ResultCount = nLength;
                    }
                    else
                    {
                        this.BrowseSearchResultControl1.ResultCount = (int)lHitCount;
                    }

                    if (string.IsNullOrEmpty(strTitle) == false)
                    {
                        this.BrowseSearchResultControl1.Title = strTitle;
                    }
                }
                else
                {
                    strError = "结果集文件 '" + strResultSet + "' 不存在";
                    goto ERROR1;
                }
            }
            else if (string.IsNullOrEmpty(strBaseResultSet) == false)
            {
                // 只用了base参数
                this.BrowseSearchResultControl1.Clear();
                this.BrowseSearchResultControl1.Visible = true;
                VisibleFilter(true);

                this.BrowseSearchResultControl1.ResultSetName = strBaseResultSet;
                this.BrowseSearchResultControl1.ResultCount = (int)MyWebPage.GetServerResultCount(sessioninfo, strBaseResultSet);
                this.BrowseSearchResultControl1.StartIndex = 0;

                this.filter.ResultSetName = strBaseResultSet;

            }
        }

        //this.AdvanceSearchControl1.Channels = sessioninfo.Channels;
        //this.AdvanceSearchControl1.ServerUrl = app.WsUrl;

        // this.HeadBarControl1.CurrentColumn = HeaderColumn.Search;
        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
    }

    Encoding DetectEncoding(string strFlag)
    {

        Encoding[] encodings = new Encoding[] {
            Encoding.Unicode,
            Encoding.UTF8,
            Encoding.GetEncoding(936)
        };

        foreach (Encoding encoding in encodings)
        {
            string strText = HttpUtility.UrlDecode(strFlag, encoding);
            if (strText == "数字平台")
                return encoding;
        }

        return null;
    }

    void GetSearchParams(out string strWord,
        out string strDbName,
        out string strFrom,
        out string strMatchStyle)
    {
        strWord = "";
        strDbName = "";
        strFrom = "";
        strMatchStyle = "";

        string strQueryString = "";

        if (this.Request.HttpMethod == "GET")
            strQueryString = this.Request.ServerVariables["QUERY_STRING"];
        else if (this.Request.HttpMethod == "POST")
        {
            strWord = this.Request["word"];
            strDbName = this.Request["dbname"];
            strFrom = this.Request["from"];
            strMatchStyle = this.Request["matchstyle"];
            return;
        }

        Hashtable table = StringUtil.ParseParameters(strQueryString, '&', '=', "");

        string strFlag = (string)table["flag"];


        Encoding encoding = DetectEncoding(strFlag);
        if (encoding == null)
        {
            strWord = this.Request["word"];
            strDbName = this.Request["dbname"];
            strFrom = this.Request["from"];
            strMatchStyle = this.Request["matchstyle"];
            return;
        }

        strWord = HttpUtility.UrlDecode((string)table["word"], encoding);
        strDbName = HttpUtility.UrlDecode((string)table["dbname"], encoding);
        strFrom = HttpUtility.UrlDecode((string)table["from"], encoding);
        strMatchStyle = HttpUtility.UrlDecode((string)table["matchstyle"], encoding);

    }

    static string MakeSelectedPath(string strResultsetName, string strOffset)
    {
        if (string.IsNullOrEmpty(strOffset) == false)
            return strResultsetName + "/" + strOffset;
        return strResultsetName;
    }

#if NO
    // ajax请求获得书目库名
    // searchbiblio.aspx?action=getdblist
    void DoGetDbNameList()
    {
        string strError = "";
        int nRet = 0;

        string strResult = "";
        List<string> list = this.BiblioSearchControl1.GetDbNameList();
        foreach (string s in list)
        {
            string strText = s; //  s.Replace("<", "[").Replace(">", "]");
            strText = HttpUtility.HtmlEncode(strText);
            strResult += "<option value='" + strText + "'>" + strText + "</option>";
        }

        GetDbNameList result_info = new GetDbNameList();
        result_info.ResultText = strResult;

    END_GETINFO:
        this.Response.Write(GetResultString(result_info));
        this.Response.End();
    }

    static string GetResultString(object obj)
    {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());

        MemoryStream ms = new MemoryStream();
        ser.WriteObject(ms, obj);
        string strResult = Encoding.UTF8.GetString(ms.ToArray());
        ms.Close();

        return strResult;
    }
#endif

    void channel_Idle(object sender, IdleEventArgs e)
    {
        bool bConnected = this.Response.IsClientConnected;

        if (bConnected == false)
        {
            LibraryChannel channel = (LibraryChannel)sender;
            channel.Abort();
        }

        e.bDoEvents = false;
    }

    // 通过前缀字符串和Session中存储的号码，构造一个新的结果集名
    string GetResultSetName(string strResultSetNamePrefix)
    {
        string strResultSetID = (string)Session[strResultSetNamePrefix + "_resultset_id"];
        if (String.IsNullOrEmpty(strResultSetID) == true)
            strResultSetID = "1";

        string strResultSetName = strResultSetNamePrefix + "_" + strResultSetID;
        // 立即增量
        int nNumber = Convert.ToInt32(strResultSetID) + 1;
        if (nNumber > 10)
            nNumber = 1;    // 最多10个以后折回
        strResultSetID = nNumber.ToString();
        Session[strResultSetNamePrefix + "_resultset_id"] = strResultSetID;

        return "#" + strResultSetName;
    }


    protected void BiblioSearchControl1_Search(object sender, DigitalPlatform.OPAC.Web.SearchEventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strError = "";

        string strResultSetNamePrefix = Session.SessionID + "_opac_base";  //  "opac_base";

        string strResultSetName = GetResultSetName(strResultSetNamePrefix);

        sessioninfo.Channel.Idle += new IdleEventHandler(channel_Idle);
        try
        {
            long lRet = sessioninfo.Channel.Search(
                null,
                e.QueryXml,
                strResultSetName,
                "", // strOutputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;
            sessioninfo.SetFilterTask(strResultSetName, null);

            if (app.SearchLog != null)
            {
                SearchLogItem log = new SearchLogItem();
                log.IP = this.Request.UserHostAddress.ToString();
                log.Query = e.QueryXml;
                log.Time = DateTime.UtcNow;
                log.HitCount = (int)lRet;
                log.Format = "searchcount";
                app.SearchLog.AddLogItem(log);
            }

            // not found
            if (lRet == 0)
            {
                this.BrowseSearchResultControl1.Visible = false;
                this.filter.Visible = false;

                strError = "没有找到";
                goto ERROR1;
            }

            e.ErrorInfo = string.Format(
                this.BiblioSearchControl1.GetString("hit_records_number"),
                lRet.ToString());
            // e.ErrorInfo = "命中记录 " +lRet.ToString()+ " 条";
            this.BrowseSearchResultControl1.Clear();
            this.BrowseSearchResultControl1.Visible = true;
            VisibleFilter(true);

            this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
            this.BrowseSearchResultControl1.ResultCount = (int)lRet;
            this.BrowseSearchResultControl1.StartIndex = 0; // 2008/12/15

            this.filter.ResultSetName = strResultSetName;
        }
        finally
        {
            sessioninfo.Channel.Idle -= new IdleEventHandler(channel_Idle);

        }
        return;
    ERROR1:
        e.ErrorInfo = strError;
        this.BrowseSearchResultControl1.ResultSetName = "";
        this.BrowseSearchResultControl1.ResultCount = 0;

        this.filter.ResultSetName = "";
    }

    public void Page_Error(object sender, EventArgs e)
    {
        // http://support.microsoft.com/kb/306355
        Exception objErr = Server.GetLastError().GetBaseException();

        if (objErr is HttpException)
        {
            Server.ClearError();
            Response.Write("<p>" + objErr.Message + "</p><a href='./login.aspx'>重新登录</a>");
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



    // 观看二级节点的其他片断
    protected void filter_TreeItemClick(object sender, TreeItemClickEventArgs e)
    {
        string strError = "";
        string strResultsetName = this.filter.ResultSetName;
        FilterTask t = sessioninfo.FindFilterTask(strResultsetName);    // Task对象是利用Session内结果集名来进行管理的
        if (t == null)
        {
            strError = "结果集名 '" + strResultsetName + "' 没有找到对应的任务对象";
            goto ERROR1;
        }
        if (t.TaskState == TaskState.Processing)
        {
            strError = "任务对象 '" + strResultsetName + "' 正在创建过程中，请稍后再访问";
            goto ERROR1;
        }

        string strParameters = this.filter.SelectedNodePath;
        string[] parameters = strParameters.Split(new char[] { ',' });
        string strNode = "";
        int nStart = 0;
        if (parameters.Length >= 1)
            strNode = parameters[0];
        if (parameters.Length >= 2)
            Int32.TryParse(parameters[1], out nStart);

        int nRet = ResultsetFilter.SwitchPage(ref t.ResultItems,
            strNode,
            nStart,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        // 确保上一级被选定。但此时和右边的 browselist 内容就不对应了
        this.filter.SelectedNodePath = GetParentResultsetName(this.filter.SelectedNodePath) + "/nav";

        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
    }

    // 获得路径的第一级
    static string GetParentResultsetName(string strSelectedPath)
    {
        int nRet = strSelectedPath.IndexOf(",");
        if (nRet == -1)
            return strSelectedPath.Replace("_sub", "");
        return strSelectedPath.Substring(0, nRet).Replace("_sub", "");
    }

    public class GetDbNameList
    {
        // 输出结果
        public string ResultText = null;    // HTML片断，可以直接插入<select>元素下
        public string ErrorString = "";
    }

    protected void TitleBarControl1_Refreshing(object sender, RefreshingEventArgs e)
    {
        sessioninfo.ClearLoginReaderDomCache();
        e.Cancel = true;
    }
}