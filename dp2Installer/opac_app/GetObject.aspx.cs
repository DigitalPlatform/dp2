#define CHANNEL_POOL

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
using System.Drawing;

using DigitalPlatform;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

public partial class GetObject : MyWebPage
{
    //OpacApplication app = null;
    //SessionInfo sessioninfo = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        /*
        // 是否登录?
        if (sessioninfo.UserID == "")
        {
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }
         * */
        string strError = "";
        int nRet = 0;
        // string strAction = Request.QueryString["action"];
        string strURI = Request.QueryString["uri"];
        string strStyle = Request.QueryString["style"];

        LibraryChannel channel = null;
#if CHANNEL_POOL
        channel = sessioninfo.GetChannel(true, sessioninfo.Parameters);
#else
        channel = sessioninfo.GetChannel(false);
#endif
        try
        {
            Uri uri = new Uri(strURI);
            if (uri.Scheme == "http" || uri.Scheme == "https")
            {
                if (StringUtil.IsInList("hitcount", strStyle) == true)
                {
#if NO
                    if (app.SearchLog != null)
                    {
                        long lHitCount = app.SearchLog.GetHitCount(strURI);
                        OpacApplication.OutputImage(this,
                            Color.FromArgb(200, Color.Blue),
                            lHitCount.ToString());
                        this.Response.End();
                        return;
                    }
                    OpacApplication.OutputImage(this,
                        Color.FromArgb(200, Color.Blue),
                        "*"); // 星号表示尚未启用外部链接计数功能
                    this.Response.End();
#endif

                    string strText = "";
                    long lValue = 0;
                    long lRet = app.GetHitCount(channel,
            strURI,
            out lValue,
            out strError);
                    if (lRet == -1)
                        strText = strError;
                    else
                        strText = lValue.ToString();
                    OpacApplication.OutputImage(this,
    Color.FromArgb(200, Color.Blue),
    strText);
                    this.Response.End();
                    return;
                }
                else
                {
#if NO
                    if (app.SearchLog != null)
                        app.SearchLog.IncHitCount(strURI);
#endif
                    long lRet = app.IncHitCount(channel,
strURI,
this.Request.UserHostAddress,
true,
out strError);
                    if (lRet == -1)
                        Response.Write("IncHitCount 出错: " + strError);
                    else
                        this.Response.Redirect(strURI, true);
                    this.Response.End();
                    return;
                }
            }

            string strSaveAs = Request.QueryString["saveas"];
            bool bSaveAs = false;
            if (strSaveAs == "true")
                bSaveAs = true;

            // FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

            this.Response.BufferOutput = false;
            this.Server.ScriptTimeout = 10 * 60 * 60;    // 10 个小时

            nRet = app.DownloadObject(
                this,
                // flushdelegate,
                // sessioninfo.Channels,
                channel,
                strURI,
                bSaveAs,
                strStyle,
                out strError);
            if (nRet == -1)
                Response.Write(strError);

            Response.End();
            return;
        }
        finally
        {

#if CHANNEL_POOL
            sessioninfo.ReturnChannel(channel);
#endif
        }
    }

    /*
    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }
     * */

}