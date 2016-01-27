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

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
// using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

public partial class SearchItem : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.Search;

        this.TitleBarControl1.LibraryCodeChanged -= new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);
        this.TitleBarControl1.LibraryCodeChanged += new LibraryCodeChangedEventHandler(TitleBarControl1_LibraryCodeChanged);

        this.ItemSearchControl1.DefaultHiddenMatchStyle = "middle";  // 当匹配方式列隐藏时，是依中间一致来进行的。
        this.ItemSearchControl1.DefaultVisibleMatchStyle = "middle";  // 当匹配方式列出现时，是依中间一致来进行的。

        this.BrowseSearchResultControl1.DefaultFormatName = "详细";
        this.BrowseSearchResultControl1.Visible = false;

        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    void TitleBarControl1_LibraryCodeChanged(object sender, LibraryCodeChangedEventArgs e)
    {
        this.BrowseSearchResultControl1.ResetAllItemsControlPager();
    }

    bool GetVisible(string strType)
    {
        XmlNode nodeItemSearch = app.WebUiDom.DocumentElement.SelectSingleNode(strType.ToLower() + "Search");
        if (nodeItemSearch == null)
            return true;
        return DomUtil.GetBooleanParam(nodeItemSearch,
            "visible",
            true);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strType = this.Request["type"];
        if (String.IsNullOrEmpty(strType) == true)
            strType = "item";

        if (GetVisible(strType) == false)
        {
            this.Response.Redirect("./searchbiblio.aspx", true);
            this.Response.End();
            return;
        }

        string strSideBarFile = Server.MapPath("./search_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;

        this.ItemSearchControl1.DbType = strType;

        this.ItemSearchControl1.FillList();

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
            this.BrowseSearchResultControl1.Visible = true;


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
             */
        }

        string strError = "";


        // 如果有参数
        string strWord = this.Request["word"];
        if (String.IsNullOrEmpty(strWord) == false)
        {
            string strXml = "";
            // 根据检索参数创建XML检索式
            int nRet = OpacApplication.BuildQueryXml(
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
                // strResultSetNamePrefix = "opac_item_1";
                strResultSetNamePrefix = Session.SessionID + "opac_item_1";
            }
            else
            {
                strResultSetNamePrefix = Session.SessionID + "_" + strResultSetNamePrefix;
            }

            string strResultSetName = GetResultSetName(strResultSetNamePrefix);

            LibraryChannel channel = sessioninfo.GetChannel(true);
            //sessioninfo.Channel.
            channel.Idle += new IdleEventHandler(channel_Idle);
            try
            {
                long lRet = //sessioninfo.Channel.
                    channel.Search(
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
                    this.BrowseSearchResultControl1.Visible = false;
                    strError = "没有找到";
                    goto ERROR1;
                }

                this.BrowseSearchResultControl1.Clear();
                this.BrowseSearchResultControl1.Visible = true;
                this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
                this.BrowseSearchResultControl1.ResultCount = (int)lRet;
                this.BrowseSearchResultControl1.StartIndex = 0; // 2008/12/15

                string strFormat = this.Request["format"];
                if (String.IsNullOrEmpty(strFormat) == false)
                {
                    this.BrowseSearchResultControl1.FormatName = strFormat;
                }
                return;
            }
            finally
            {
                //sessioninfo.Channel.
                channel.Idle -= new IdleEventHandler(channel_Idle);
                sessioninfo.ReturnChannel(channel);
            }
        }

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

        // e.bDoEvents = false;
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




    protected void ItemSearchControl1_Search(object sender, DigitalPlatform.OPAC.Web.SearchEventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strError = "";

        // string strResultSetNamePrefix = "opac_item_base";
        string strResultSetNamePrefix = Session.SessionID + "opac_item_base";

        string strResultSetName = GetResultSetName(strResultSetNamePrefix);

        LibraryChannel channel = sessioninfo.GetChannel(true);
        // sessioninfo.Channel.
        channel.Idle += new IdleEventHandler(channel_Idle);
        try
        {
            long lRet = // sessioninfo.Channel.
                channel.Search(
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
                this.BrowseSearchResultControl1.Visible = false;
                strError = "没有找到";
                goto ERROR1;
            }

            e.ErrorInfo = string.Format(
                this.ItemSearchControl1.GetString("hit_records_number"),
                lRet.ToString());
            // e.ErrorInfo = "命中记录 " +lRet.ToString()+ " 条";
            this.BrowseSearchResultControl1.Clear();
            this.BrowseSearchResultControl1.Visible = true;
            this.BrowseSearchResultControl1.ResultSetName = strResultSetName;
            this.BrowseSearchResultControl1.ResultCount = (int)lRet;
            this.BrowseSearchResultControl1.StartIndex = 0; // 2008/12/15
        }
        finally
        {
            // sessioninfo.Channel.
            channel.Idle -= new IdleEventHandler(channel_Idle);
            sessioninfo.ReturnChannel(channel);
        }
        return;
    ERROR1:
        /*
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
         * */
        e.ErrorInfo = strError;
        this.BrowseSearchResultControl1.ResultSetName = "";
        this.BrowseSearchResultControl1.ResultCount = 0;

    }
}