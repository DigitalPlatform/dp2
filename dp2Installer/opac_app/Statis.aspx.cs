using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

public partial class Statis : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = DigitalPlatform.OPAC.Web.TitleColumn.Statis;
        this.SideBarControl1.LayoutStyle = SideBarLayoutStyle.Horizontal;
    }

    string GetStatisColumnVisible()
    {
        // 元素缺乏时的缺省值
        string strStatisColumnVisible = "reader,librarian";
        if (app.WebUiDom == null)
            return strStatisColumnVisible;

        XmlNode nodeStatisColumn = app.WebUiDom.DocumentElement.SelectSingleNode("titleBarControl/statisColumn");
        if (nodeStatisColumn != null)
        {
            // 一旦元素具备，就没有缺省值了
            strStatisColumnVisible = DomUtil.GetAttr(nodeStatisColumn, "visible");
        }

        return strStatisColumnVisible;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string strError = "";

        string strStatisColumnVisible = GetStatisColumnVisible();

        LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

        if (StringUtil.IsInList("all", strStatisColumnVisible) == true
            || (loginstate == LoginState.Librarian && StringUtil.IsInList("librarian", strStatisColumnVisible) == true)
            || (loginstate == LoginState.Reader && StringUtil.IsInList("reader", strStatisColumnVisible) == true)
            || (loginstate == LoginState.Public && StringUtil.IsInList("public", strStatisColumnVisible) == true)
            || (loginstate == LoginState.NotLogin && StringUtil.IsInList("notlogin", strStatisColumnVisible) == true)
            )
        {
            // 2013/12/27
            if (sessioninfo.UserID == "")
            {
                sessioninfo.UserID = "public";
                sessioninfo.IsReader = false;
            }
        }
        else
        {
            Response.Write("必须是 " + strStatisColumnVisible + " 状态才能使用statis.aspx");
            Response.End();
            /*
            // 是否登录?
            if (sessioninfo.UserID == "")
            {
                sessioninfo.LoginCallStack.Push(Request.RawUrl);
                Response.Redirect("login.aspx", true);
                return;
            }*/
        }

        string strSideBarFile = Server.MapPath("./statis_sidebar.xml");
        if (File.Exists(strSideBarFile) == true)
            this.SideBarControl1.CfgFile = strSideBarFile;
        else
            this.SideBarControl1.Visible = false;


        // 不是回调的情况
        if (true/*!IsPostBack*/)
        {
            string strDate = this.Page.Request["date"];

            // 2009/7/22
            if (String.IsNullOrEmpty(strDate) == true)
            {
                if (this.HiddenField_activetab.Value == "day"
                    || string.IsNullOrEmpty(this.HiddenField_activetab.Value) == true)
                {
                    if (this.Calendar1.SelectedDate != DateTime.MinValue)
                        strDate = DateTimeUtil.DateTimeToString8(this.Calendar1.SelectedDate);
                    else
                    {
                        // 给今天的日期
                        strDate = DateTimeUtil.DateTimeToString8(DateTime.Now);
                    }
                }
                else
                    strDate = this.TextBox_dateRange.Text;
            }

            if (String.IsNullOrEmpty(strDate) == false)
            {
                int nRet = GetResult(sessioninfo.Channel, strDate, out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
        }

        // this.PanelControl1.Title = "";
        this.PanelControl2.Title = "";
        // this.PanelControl3.Title = "";
        return;
    ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;
    }


    // TODO: 是否缓冲一下，提高速度?
    bool ExistStatisFile(
        LibraryChannel channel,
        string strDate8)
    {
        DateExist [] dates = null;
        string strError = "";
        long lRet = channel.ExistStatisInfo(
            strDate8,
            out dates,
            out strError);
        if (lRet == -1)
            return false;
        if (dates == null || dates.Length == 0)
            return false;
        if (dates[0].Exist == true)
            return true;
        return false;
    }

    protected void Calendar1_DayRender(object sender,
        DayRenderEventArgs e)
    {
        if (e.Day.Date <= DateTime.Now
            && ExistStatisFile(sessioninfo.Channel, DateTimeUtil.DateTimeToString8(e.Day.Date)) == true)
        {
            // e.SelectUrl = app.LibraryServerUrl + "/statis.aspx?date=" + DateTimeUtil.DateTimeToString8(e.Day.Date);
            e.Day.IsSelectable = true;
            // e.Cell.ForeColor = System.Drawing.Color.Black;
        }
        else
        {
            e.Day.IsSelectable = false;
            e.Cell.ForeColor = System.Drawing.Color.Gray;

            /*
            string strText =
                GetLocalResourceObject("(无)").ToString();

            e.Cell.Controls.Add(new LiteralControl(strText));    // "(无)"
             * */
        }
    }


    protected void Calendar1_SelectionChanged(object sender,
        EventArgs e)
    {
        string strError = "";
        string strDate = "";
        // Iterate through the SelectedDates collection and display the
        // dates selected in the Calendar control.
        foreach (DateTime day in Calendar1.SelectedDates)
        {
            /*
            this.Page.Response.Redirect(app.LibraryServerUrl + "/statis.aspx?date=" + DateTimeUtil.DateTimeToString8(day),
                true);
             * */
            strDate = DateTimeUtil.DateTimeToString8(day);
            break;
        }

        if (String.IsNullOrEmpty(strDate) == false)
        {
            RangeStatisInfo info = null;
            string strXml = "";

            long lRet = sessioninfo.Channel.GetStatisInfo(strDate,
                "",
                out info,
                out strXml,
                out strError);
            if (lRet == -1)
            {
                goto ERROR1;
            }

            this.StatisViewControl1.Xml = strXml;
            this.StatisViewControl1.DateRange = strDate;
            this.StatisViewControl1.IsRange = false;
            this.StatisViewControl1.RangeStatisInfo = info;

            DateTime current_date = DateTimeUtil.Long8ToDateTime(strDate);
            this.Calendar1.SelectedDate = current_date;
            this.Calendar1.TodaysDate = current_date;
        }
        return;
    ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;
    }

    protected void Button_beginStatis_Click(object sender, EventArgs e)
    {
        string strError = "";

#if NO
        RangeStatisInfo info = null;
        string strXml = "";
        string strDate = this.TextBox_dateRange.Text;
        long lRet = sessioninfo.Channel.GetStatisInfo(strDate,
            out info,
            out strXml,
            out strError);
        if (lRet == -1)
        {
            goto ERROR1;
        }

        this.StatisViewControl1.RangeStatisInfo = info;
        this.StatisViewControl1.DateRange = strDate;
        // this.StatisViewControl1.XmlFilename = strOutputFilename;
        this.StatisViewControl1.Xml = strXml;
#endif
        // 如果时间范围为空，则等于当前月份
        if (string.IsNullOrEmpty(this.TextBox_dateRange.Text) == true)
        {
            this.TextBox_dateRange.Text = DateTimeUtil.DateTimeToString8(DateTime.Now).Substring(0, 6);
        }

        int nRet = GetResult(sessioninfo.Channel, this.TextBox_dateRange.Text, out strError);
        if (nRet == -1)
            goto ERROR1;
        return;
    ERROR1:
        this.Page.Response.Write(HttpUtility.HtmlEncode(strError));
        this.Page.Response.End();
        return;
    }

    int GetResult(
        LibraryChannel channel,
        string strDate,
        out string strError)
    {
        strError = "";

        RangeStatisInfo info = null;
        string strXml = "";
        long lRet = channel.GetStatisInfo(strDate,
            strDate.Length == 8 ? "" : "list",
            out info,
            out strXml,
            out strError);
        if (lRet == -1)
            return -1;
        this.StatisViewControl1.Xml = strXml;
        this.StatisViewControl1.DateRange = strDate;

        // 8字符的情况
        if (strDate.Length == 8)
        {
            DateTime current_date = DateTimeUtil.Long8ToDateTime(strDate);
            this.Calendar1.SelectedDate = current_date;
            this.Calendar1.TodaysDate = current_date;
            this.StatisViewControl1.IsRange = false;
        }
        else
            this.StatisViewControl1.IsRange = true;

        this.StatisViewControl1.RangeStatisInfo = info;
        return 0;
    }

}