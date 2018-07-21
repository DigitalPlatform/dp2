#define CHANNEL_POOL

using System;
using System.Drawing;

using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;

public partial class GetObject : MyWebPage
{
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
        if (sessioninfo.UserID == "")
        {
            sessioninfo.UserID = "public";
            sessioninfo.IsReader = false;
        }

        string strError = "";
        int nRet = 0;
        // string strAction = Request.QueryString["action"];
        string strURI = Request.QueryString["uri"];
        string strStyle = Request.QueryString["style"];
        string strBiblioRecPath = Request.QueryString["biblioRecPath"];

        LibraryChannel channel = null;
#if CHANNEL_POOL
        channel = sessioninfo.GetChannel(true/*, sessioninfo.Parameters*/);
#else
        channel = sessioninfo.GetChannel(false);
#endif
        try
        {
            Uri uri = GetUri(strURI);
            if (uri != null
                && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                // 以下是处理 dp2 系统外部的 URL
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
                    int nFontSize = 8;
                    string strFontSize = Request.QueryString["fontSize"];
                    if (string.IsNullOrEmpty(strFontSize) == false)
                        Int32.TryParse(strFontSize, out nFontSize);

                    if (StringUtil.IsInList("hitcount", app.SearchLogEnable) == false)
                    {
                        OpacApplication.OutputImage(this,
                            Color.FromArgb(200, Color.Blue),
                            "*",
                            nFontSize); // 星号表示尚未启用外部链接计数功能
                        this.Response.End();
                        return;
                    }
                    // TODO: 这里可以优化一下，当 lValue == -1 的时候，可以为 App 设置一个标志，以后就不再为外部 URL 请求 dp2library 的 HitCounver() API 了
                    string strText = "";
                    long lValue = 0;
                    long lRet = app.GetHitCount(channel,
                        strBiblioRecPath + "|" + strURI,
                        out lValue,
                        out strError);
                    if (lRet == -1)
                        strText = strError;
                    else
                        strText = (lValue == -1 ? "*" : lValue.ToString());    // * 表示 dp2library 中 mongodb 没有启用
                    OpacApplication.OutputImage(this,
                        Color.FromArgb(200, Color.Blue),
                        strText,
                        nFontSize);

                    // 不但返回图像，而且滞后增量一次
                    if (StringUtil.IsInList("inc", strStyle) == true)
                    {
                        lRet = app.IncHitCount(channel,
        strBiblioRecPath + "|" + strURI,
        this.Request.UserHostAddress,
        false,    // 是否要创建日志
        out strError);
                        if (lRet == -1)
                        {
                            Response.Write("IncHitCount 出错: " + strError);
                            this.Response.End();
                            return;
                        }
                    }

                    this.Response.End();
                    return;
                }


                else
                {
#if NO
                    if (app.SearchLog != null)
                        app.SearchLog.IncHitCount(strURI);
#endif
                    if (StringUtil.IsInList("hitcount", app.SearchLogEnable) == false)
                    {
                        this.Response.Redirect(strURI, true);
                        return;
                    }
                    long lRet = app.IncHitCount(channel,
                        strBiblioRecPath + "|" + strURI,
                        this.Request.UserHostAddress,
                        StringUtil.IsInList("log", app.SearchLogEnable),    // 是否要创建日志
                        out strError);
                    if (lRet == -1)
                    {
                        Response.Write("IncHitCount 出错: " + strError);
                        this.Response.End();
                    }
                    else
                        this.Response.Redirect(strURI, true);
                    return;
                }
            }

            // *** 以下是处理 dp2 系统内部对象
            // TODO: dp2 系统内部对象总是有访问计数功能的，是否需要设计为 SearchLogEnable 中的 hitcount 具备与否对它无影响?

            string strSaveAs = Request.QueryString["saveas"];
            bool bSaveAs = false;
            if (strSaveAs == "true")
                bSaveAs = true;

            // FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

            // this.Response.BufferOutput = false;
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
            {
                // Response.Write(strError);
                OpacApplication.OutputImage(Page,
    Color.FromArgb(200, Color.DarkRed),
    strError);
            }

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

    static Uri GetUri(string strURI)
    {
        try
        {
            return new Uri(strURI);
        }
        catch
        {
            return null;
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