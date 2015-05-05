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
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;

public partial class Search2 : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

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

        // this.BrowseSearchResultControl1.DefaultFormatName = "详细";
        this.ViewResultsetControl1.Visible = false;

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        // this.BrowseSearchResultControl1.ResetAllItemsControlPager();
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

    void GetResultSetFrame(string strResultsetName, int nStartIndex, int nCount = 10)
    {
        string strError = "";
        GetResultSetFrame result_info = new GetResultSetFrame();
        if (string.IsNullOrEmpty(strResultsetName) == true)
        {
            strError = "结果集名不应为空";
            goto ERROR1;
        }
        string strResult = "";
        int nRet = ViewResultsetControl.GetContentText(
            app,
            sessioninfo,
            strResultsetName,
            nStartIndex,
            nCount,
            "zh",
            out strResult,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        result_info.Html = strResult;
        this.Response.Write(MyWebPage.GetResultString(result_info));
        this.Response.End();
        return;
    ERROR1:
        result_info.ErrorString = strError;
        this.Response.Write(MyWebPage.GetResultString(result_info));
        this.Response.End();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strAction = this.Page.Request["action"];

        // Ajax命令
        if (strAction == "getresultsetframe")
        {
            string strResultsetName = this.Request["resultset"];
            string strStart = this.Request["start"];
            int nStart = 0;
            Int32.TryParse(strStart, out nStart);
            GetResultSetFrame(strResultsetName, nStart);
            return;
        }

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

        if (string.IsNullOrEmpty(this.ViewResultsetControl1.ResultSetName) == false)
            this.ViewResultsetControl1.Visible = true;


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
            // 根据检索参数创建XML检索式
            nRet = OpacApplication.BuildQueryXml(
                this.app,
                this.Request["dbname"],
                strWord,
                this.Request["from"],
                this.Request["matchstyle"],
                null,
                null,
                app.SearchMaxResultCount,
                "", // strSearchStyle
                out strXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            string strResultSetNamePrefix = "";

            strResultSetNamePrefix = this.Request["resultsetname"];
            if (String.IsNullOrEmpty(strResultSetNamePrefix) == true)
            {
                strResultSetNamePrefix = "opac_1";
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

                // not found
                if (lRet == 0)
                {
                    this.ViewResultsetControl1.ResultSetName = "";
                    this.ViewResultsetControl1.Visible = false;
                    strError = "没有找到";
                    goto ERROR1;
                }

                this.ViewResultsetControl1.Visible = true;
                this.ViewResultsetControl1.ResultSetName = strResultSetName;
                this.ViewResultsetControl1.ResultCount = (int)lRet;
                this.ViewResultsetControl1.StartIndex = 0; 

                /*
                this.BrowseSearchResultControl1.Clear();
                this.BrowseSearchResultControl1.Visible = true;
                this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
                this.BrowseSearchResultControl1.ResultCount = (int)lRet;
                this.BrowseSearchResultControl1.StartIndex = 0; // 2008/12/15 new add
                 * */

                this.resultsetname.Value = strResultSetName;

                string strFormat = this.Request["format"];
                if (String.IsNullOrEmpty(strFormat) == false)
                {
                    this.ViewResultsetControl1.FormatName = strFormat;
                }
                return;
            }
            finally
            {
                sessioninfo.Channel.Idle -= new IdleEventHandler(channel_Idle);
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

        return strResultSetName;
    }


    protected void BiblioSearchControl1_Search(object sender, DigitalPlatform.OPAC.Web.SearchEventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strError = "";

        string strResultSetNamePrefix = "opac_base";

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

            // not found
            if (lRet == 0)
            {
                this.ViewResultsetControl1.ResultSetName = "";
                this.ViewResultsetControl1.Visible = false;
                strError = "没有找到";
                goto ERROR1;
            }

            e.ErrorInfo = string.Format(
                this.BiblioSearchControl1.GetString("hit_records_number"),
                lRet.ToString());
            // e.ErrorInfo = "命中记录 " +lRet.ToString()+ " 条";

            /*
            this.BrowseSearchResultControl1.Clear();
            this.BrowseSearchResultControl1.Visible = true;
            this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
            this.BrowseSearchResultControl1.ResultCount = (int)lRet;
            this.BrowseSearchResultControl1.StartIndex = 0; // 2008/12/15 new add
             * */
            this.ViewResultsetControl1.Visible = true;
            this.ViewResultsetControl1.ResultSetName = strResultSetName;
            this.ViewResultsetControl1.ResultCount = (int)lRet;
            this.ViewResultsetControl1.StartIndex = 0; 

            this.resultsetname.Value = strResultSetName;
        }
        finally
        {
            sessioninfo.Channel.Idle -= new IdleEventHandler(channel_Idle);

        }
        return;
    ERROR1:
        e.ErrorInfo = strError;
        this.ViewResultsetControl1.ResultSetName = "";
        this.ViewResultsetControl1.ResultCount = 0;

        this.resultsetname.Value = "";

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

}

public class GetResultSetFrame
{
    public string Html = "";
    public string ErrorString = "";
}