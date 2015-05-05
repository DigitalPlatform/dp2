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
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.OPAC.Web;
using DigitalPlatform.CirculationClient;

public partial class Management : MyWebPage
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

        this.TitleBarControl1.CurrentColumn = TitleColumn.Management;

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (WebUtil.PrepareEnvironment(this,
ref app,
ref sessioninfo) == false)
            return;

        string strError = "";

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
            sessioninfo.LoginCallStack.Push(Request.RawUrl);
            Response.Redirect("login.aspx", true);
            return;
        }

        LoginState loginstate = Global.GetLoginState(this.Page);
        if (loginstate != LoginState.Librarian)
        {
            strError = "只有工作人员身份才能使用本模块";
            goto ERROR1;
        }

        string strAction = Request["action"];

        if (strAction == "geterrorlog")
        {
            string strFilename = this.Request["filename"];
            string strFilePath = "";
            if (string.IsNullOrEmpty(strFilename) == false)
                strFilePath = app.DataDir + "/log/" + strFilename;
            else
                strFilePath = app.DataDir + "/log/log_" + DateTimeUtil.DateTimeToLong8(DateTime.Now) + ".txt";
            
            int nRet = DumpFile(
                strFilePath,
                "text/plain",
                out strError);
            if (nRet == 0)
            {
                this.Response.ContentType = "text/html";
                this.Response.StatusCode = 404;
                this.Response.StatusDescription = strError;
                this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
                this.Response.Flush(); 
                this.Response.End();
                return;
            }
            if (nRet == -1)
            {
                this.Response.ContentType = "text/html";
                this.Response.StatusCode = 500;
                this.Response.StatusDescription = strError;
                this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
                this.Response.Flush();
                this.Response.End();
                return;
            }

            this.Response.Flush();
            this.Response.End();
            return;
        }

        if (strAction == "geteventlog")
        {
            EventLog Log = new EventLog();
            Log.Source = "dp2opac";

            // return:
            //      -1  出错
            //      0   成功
            int nRet = DumpEventLog(Log,
                out strError);
            if (nRet == -1)
            {
                this.Response.ContentType = "text/html";
                this.Response.StatusCode = 500;
                this.Response.StatusDescription = strError;
                this.Response.Write("<html><body><p>" + HttpUtility.HtmlEncode(strError) + "</p></body></html>");
                this.Response.Flush();
                this.Response.End();
                return;
            }
            this.Response.Flush();
            this.Response.End();
            return;
        }


        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        this.Response.Flush();
        Response.End();
    }


    // return:
    //      -1  出错
    //      0   成功
    int DumpEventLog(EventLog Log,
        out string strError)
    {
        strError = "";

        // 不让浏览器缓存页面
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");

        this.Response.ContentType = "text/plain";

        try
        {
            foreach (EventLogEntry entry in Log.Entries)
            {
                if (this.Response.IsClientConnected == false)
                    break;

                string strText = "*********\r\n"
                    + "Machine Name:\t" + entry.MachineName + "\r\n"
                    + "Source:\t" + entry.Source + "\r\n"
                    + "Category:\t" + entry.Category + "\r\n"
                    + "Entry Type:\t" + entry.EntryType.ToString() + "\r\n"
                    + "Event ID:\t" + entry.InstanceId.ToString() + "\r\n"
                    + "User Name:\t" + entry.UserName + "\r\n"
                    + "Time Generated:\t" + entry.TimeGenerated.ToString() + "\r\n"
                    + "message:\r\n" + entry.Message + "\r\n\r\n";

                this.Response.Write(strText);
                this.Response.Flush();
            }
        }
        catch (Exception ex)
        {
            strError = ex.Message;
            return -1;
        }

        return 0;
    }



    // return:
    //      -1  出错
    //      0   成功
    //      1   暂时不能访问
    int DumpFile(string strFilename,
        string strContentType,
        out string strError)
    {
        strError = "";

        // 不让浏览器缓存页面
        this.Response.AddHeader("Pragma", "no-cache");
        this.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        this.Response.AddHeader("Expires", "0");

        this.Response.ContentType = strContentType;

        try
        {

            Stream stream = File.Open(strFilename,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.ReadWrite);
            try
            {
                this.Response.AddHeader("Content-Length", stream.Length.ToString());

                FlushOutput flushdelegate = new FlushOutput(MyFlushOutput);

                stream.Seek(0, SeekOrigin.Begin);

                StreamUtil.DumpStream(stream, this.Response.OutputStream,
                    flushdelegate);
            }
            finally
            {
                stream.Close();
            }

        }
        catch (FileNotFoundException)
        {
            strError = "文件 '"+strFilename+"' 不存在";
            return 0;
        }
        catch (DirectoryNotFoundException)
        {
            strError = "文件 '" + strFilename + "' 路径中某一级目录不存在";
            return 0;
        }
        catch (Exception ex)
        {
            strError = ex.Message;
            return -1;
        }

        return 1;
    }

    bool MyFlushOutput()
    {
        Response.Flush();
        return Response.IsClientConnected;
    }

    protected void Button_refreshCfg_Click(object sender, EventArgs e)
    {
        string strError = "";
        if (StringUtil.IsInList("managecache", sessioninfo.RightsOrigin) == false)
        {
            strError = "当前用户不具备 managecache 权限，无法进行刷新配置的操作";
            goto ERROR1;
        }

        string strDebugInfo = "";
        int nRet = app.RefreshCfgs(
            out strDebugInfo,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        Response.Write(HttpUtility.HtmlEncode("刷新成功") + HttpUtility.HtmlEncode("\r\n" + strDebugInfo).Replace("\r\n", "<br/>"));
        Response.End();
        return;
    ERROR1:
        Response.Write(HttpUtility.HtmlEncode(strError));
        Response.End();
    }
}