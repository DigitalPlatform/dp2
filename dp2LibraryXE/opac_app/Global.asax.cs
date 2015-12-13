using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Diagnostics;
using System.Web.Routing;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.IO;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2OPAC
{
    /// <summary>
    /// Summary description for Global.
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Global()
        {
            InitializeComponent();
        }

        protected void Application_Start(Object sender, EventArgs e)
        {
            RegisterRoutes(RouteTable.Routes);

            OpacApplication app = null;

            try
            {
                int nRet = 0;
                string strError = "";

#if NO
                string strErrorLogDir = this.Server.MapPath(".\\log");
                PathUtil.CreateDirIfNeed(strErrorLogDir);
                string strErrorLogFileName = PathUtil.MergePath(strErrorLogDir, "error.txt");
#endif

                try
                {
                    string strDataDir = "";

                    // reutrn:
                    //      -1  error
                    //      0   not found start.xml
                    //      1   found start.xml
                    nRet = OpacApplication.GetDataDir(this.Server.MapPath("~/start.xml"),
                        out strDataDir,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "搜寻配置文件start.xml时出错: " + strError;
#if NO
                        OpacApplication.WriteErrorLog(strErrorLogFileName,
                            strError);
#endif
                        OpacApplication.WriteWindowsLog(strError);

                        Application["errorinfo"] = strError;
                        return;
                    }
                    if (nRet == 0)
                    {
#if NO
                        // 实在没有start.xml文件, 默认虚拟目录就是数据目录
                        strDataDir = this.Server.MapPath(".");
#endif
                        // 2014/2/21
                        // 不再允许虚拟目录中 start.xml 没有的情况
                        OpacApplication.WriteWindowsLog("OpacApplication.GetDataDir() error : " + strError);

                        Application["errorinfo"] = strError;
                        return;
                    }

                    app = new OpacApplication();
                    Application["app"] = app;

                    // string strHostDir = this.Server.MapPath(".");
                    string strHostDir = Path.GetDirectoryName(this.Server.MapPath("~/start.xml"));  // 2015/7/20

                    nRet = app.Load(
                        false,
                        strDataDir,
                        strHostDir,
                        out strError);
                    if (nRet == -1)
                    {
#if NO
                        OpacApplication.WriteErrorLog(strErrorLogFileName,
                            strError);
#endif
                        // TODO: 预先试探一下写入数据目录中的错误日志文件是否成功。如果成功，则可以把错误信息写入那里
                        OpacApplication.WriteWindowsLog(strError);

                        // Application["errorinfo"] = strError;
                        app.GlobalErrorInfo = strError;
                    }
                    else
                    {
                        nRet = app.Verify(out strError);
                        if (nRet == -1)
                        {
#if NO
                            OpacApplication.WriteErrorLog(strErrorLogFileName,
                                strError);
#endif
                            OpacApplication.WriteWindowsLog(strError);

                            // Application["errorinfo"] = strError;
                            app.GlobalErrorInfo = strError;
                        }
                    }

                    OpacApplication.WriteWindowsLog("dp2OPAC Application 启动成功", EventLogEntryType.Information);
                }
                catch (Exception ex)
                {
                    strError = "装载配置文件 opac.xml 出错: " + ExceptionUtil.GetDebugText(ex);
#if NO
                    OpacApplication.WriteErrorLog(strErrorLogFileName,
                        strError);
#endif
                    OpacApplication.WriteWindowsLog(strError);

                    if (app != null)
                        app.GlobalErrorInfo = strError;
                    else
                        Application["errorinfo"] = strError;
                }


            }
            catch (Exception ex)
            {
                string strErrorText = "Application_Start阶段出现异常: " + ExceptionUtil.GetDebugText(ex);
                OpacApplication.WriteWindowsLog(strErrorText);

                if (app != null)
                    app.GlobalErrorInfo = strErrorText;
                else
                    Application["errorinfo"] = strErrorText;
            }

        }

        // TODO: 以后白名单里面可以配置更大的数量

        protected void Session_Start(Object sender, EventArgs e)
        {
            OpacApplication app = null;
            try
            {
                app = (OpacApplication)Application["app"];

                if (app == null)
                {
                    throw new Exception("app == null while Session_Start(). global error info : " + (string)Application["errorinfo"]);
                }

                string strClientIP = HttpContext.Current.Request.UserHostAddress.ToString();
                // 增量计数
                if (app != null)
                {

                    long v = app.IpTable.IncIpCount(strClientIP, 1);
                    if (v >= app.IpTable.MAX_SESSIONS_PER_IP)
                    {
                        app.IpTable.IncIpCount(strClientIP, -1);
                        Session.Abandon();
                        return;
                    }
                }

                SessionInfo sessioninfo = new SessionInfo(app);
                sessioninfo.ClientIP = strClientIP;
                Session["sessioninfo"] = sessioninfo;
                // throw new Exception("test exception");
            }
            catch (Exception ex)
            {
                string strErrorText = "Session_Start阶段出现异常: " + ExceptionUtil.GetDebugText(ex);
                OpacApplication.WriteWindowsLog(strErrorText);
                if (app != null)
                    app.GlobalErrorInfo = strErrorText;
                else
                    Application["errorinfo"] = strErrorText;
            }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {

        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {

        }

        protected void Application_Error(Object sender, EventArgs e)
        {
            // OpacApplication app = (OpacApplication)Application["app"];

            Exception ex = HttpContext.Current.Server.GetLastError();

            string strText = ExceptionUtil.GetDebugText(ex)
                + "\r\n\r\n版本: " + System.Reflection.Assembly.GetAssembly(typeof(OpacApplication)).GetName().ToString();

            string strError = "";
            try
            {
                string strSender = HttpContext.Current.Server.MachineName;
                // 崩溃报告
                int nRet = LibraryChannel.CrashReport(
                    strSender,
                    "dp2OPAC",
                    strText,
                    out strError);
            }
            catch (Exception ex0)
            {
                strError = "CrashReport() 过程出现异常: " + ExceptionUtil.GetDebugText(ex0);
                // nRet = -1;
            }
        }

        protected void Session_End(Object sender, EventArgs e)
        {
            SessionInfo sessioninfo = (SessionInfo)Session["sessioninfo"];
            Session["sessioninfo"] = null;

            if (sessioninfo != null)
            {
                sessioninfo.CloseSession();

                // 减去计数
                // string strClientIP = HttpContext.Current.Request.UserHostAddress.ToString();
                OpacApplication app = (OpacApplication)Application["app"];
                if (app != null)
                    app.IpTable.IncIpCount(sessioninfo.ClientIP, -1);
            }
        }

        protected void Application_End(Object sender, EventArgs e)
        {
            OpacApplication app = null;

            try
            {
                /*
                // 调试用
                OpacApplication.WriteErrorLog("成功进入Application_End", EventLogEntryType.Information);
                 * */

                // 错误信息采用两级存放策略。
                // 如果LibraryAppliation对象已经存在，则采用其ErrorInfo成员的值；
                // 否则，采用Application["errorinfo"]值
                string strErrorInfo = "";
                app = (OpacApplication)Application["app"];
                if (app != null)
                    strErrorInfo = app.GlobalErrorInfo;
                else
                    strErrorInfo = (string)Application["errorinfo"];

                // 系统错误字符串为空时才保存.xml配置文件
                if (String.IsNullOrEmpty(strErrorInfo) == true)
                {
                    /*
                    // 调试用
                    OpacApplication.WriteWindowsLog("进行了 opac.xml 保存", EventLogEntryType.Information);
                     * */

                    app.Save(null, false);

                    app.Close();
                }
                else
                {
                    /*
                    // 调试用
                    OpacApplication.WriteWindowsLog("没有进行 opac.xml 保存，因为错误字符串 '" + ErrorInfo + "'", EventLogEntryType.Information);
                     * */
                }
            }
            catch (Exception ex)
            {
                string strErrorText = "Application_End阶段出现异常: " + ExceptionUtil.GetDebugText(ex);
                OpacApplication.WriteWindowsLog(strErrorText);

                if (app != null)
                    app.GlobalErrorInfo = strErrorText;
                else
                    Application["errorinfo"] = strErrorText;
            }
        }

        void RegisterRoutes(RouteCollection routes)
        {
            routes.MapPageRoute("globalRoute3",
                "stylenew/{style}/{filename}",
                "~/css.aspx");
            // 2014/12/2
            routes.MapPageRoute("globalRoute2",
    "stylenew/{filename}",
    "~/css.aspx");
            routes.MapPageRoute("libraryRoute4",
                "stylenew/{librarycode}/{style}/{filename}",
                "~/css.aspx");
        }

        #region Web Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
        }
        #endregion
    }
}

