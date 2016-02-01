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
// using DigitalPlatform.CirculationClient;

public partial class sample : System.Web.UI.Page
{
    OpacApplication app = null;
    SessionInfo sessioninfo = null;

    protected void Page_Init(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;


        this.BrowseSearchResultControl1.DefaultFormatName = "tablebrief";
        // this.BrowseSearchResultControl1.Visible = false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
        }

        // this.BrowseSearchResultControl1.FormatName = "tablebrief";

    }
    protected void Button_search_Click(object sender, EventArgs e)
    {
#if NO
        string strError = "";
        string strQueryXml = "";
        long lRet = sessioninfo.Channel.SearchBiblio(
    null,
    this.DropDownList_dbname.Text,
    this.TextBox_word.Text,
    5000,
    this.DropDownList_from.SelectedValue,
    this.DropDownList_matchStyle.SelectedValue,
    "zh",
    "default",
    "",
    "",
    out strQueryXml,
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

        this.BrowseSearchResultControl1.ResultSetName = "default";
        this.BrowseSearchResultControl1.ResultCount = (int)lRet;
        this.BrowseSearchResultControl1.StartIndex = 0;

        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
#endif
    }
}