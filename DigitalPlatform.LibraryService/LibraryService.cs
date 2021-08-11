using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Message;
using DigitalPlatform.LibraryServer.Common;

using DigitalPlatform.rms;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace dp2Library
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        Namespace = "http://dp2003.com/dp2library/")]
    public class LibraryService : ILibraryService, ILibraryServiceREST, IDisposable
    {
        LibraryApplication app = null;
        SessionInfo sessioninfo = null;
        bool RestMode = false;

        int _nStop = 0;   // 0 没有中断 1 提出中断 2 已经进行了中断

        string _ip = "";    // 没有 sessioninfo 的通道，记载 ip，便于最后释放计数

        public void Dispose()
        {
#if NO
            if (this.RestMode == false && this.sessioninfo != null)
            {
                this.sessioninfo.CloseSession();
            }
#endif
            // 2017/5/7
            if (this.sessioninfo != null)
            {
                if (this.RestMode)
                    this.sessioninfo.Used--;
            }

            if (this.RestMode == false)
            {
                if (this.sessioninfo != null)
                {
                    this.app.SessionTable.DeleteSession(sessioninfo);
                    this.sessioninfo = null;
                }
                else if (string.IsNullOrEmpty(this._ip) == false)
                {
                    // 减量，以便管理配额
                    this.app.SessionTable.IncNullIpCount(this._ip, -1);
                    this._ip = null;
                }
            }
        }

        #region 基础函数

        public int InitialApplication(out string strError)
        {
            strError = "";

            if (this.app != null)
                return 0;   // 已经初始化

            HostInfo info = OperationContext.Current.Host.Extensions.Find<HostInfo>();
            if (info.App != null)
            {
                this.app = info.App;
                return 0;
            }

            string strBinDir = System.Reflection.Assembly.GetExecutingAssembly().Location;   //  Environment.CurrentDirectory;
            strBinDir = PathUtil.PathPart(strBinDir);

            string strDataDir = info.DataDir;

            // 兼容以前的习惯。从执行文件目录中取得start.xml
            if (String.IsNullOrEmpty(strDataDir) == true)
            {
                // 从start.xml文件查找数据目录
                string strStartFileName = PathUtil.MergePath(strBinDir, "start.xml");

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strStartFileName);
                }
                catch (FileNotFoundException)
                {
                    // 文件没有找到。把执行目录的下级data目录当作数据目录
                    strDataDir = PathUtil.MergePath(strBinDir, "data");
                    goto START;
                }
                catch (Exception ex)
                {
                    strError = "文件 '" + strStartFileName + "' 装载到XMLDOM时出错, 原因：" + ex.Message;
                    return -1;
                }
                strDataDir = DomUtil.GetAttr(dom.DocumentElement, "datadir");
            }

        START:
            lock (info.LockObject)
            {
                // 2019/4/27 避免重复 new LibraryApplication
                if (info.App != null)
                {
                    this.app = info.App;
                    return 0;
                }

                info.App = new LibraryApplication();

                info.App.TestMode = info.TestMode;
                info.App.MaxClients = info.MaxClients;
                info.App.LicenseType = info.LicenseType;
                info.App.Function = info.Function;

                // parameter:
                //		strDataDir	data目录
                //		strError	out参数，返回出错信息
                // return:
                //		-1	出错
                //		0	成功
                // 线: 安全的
                int nRet = info.App.LoadCfg(
                        false,
                        strDataDir,
                        strBinDir,
                        out strError);
                if (nRet == -1)
                    return -1;
                nRet = info.App.Verify(out strError);
                if (nRet == -1)
                    return -1;
            }

            this.app = info.App;
            return 0;
        }

        public List<RemoteAddress> GetClientAddress()
        {
            List<RemoteAddress> results = new List<RemoteAddress>();

            if (OperationContext.Current.IncomingMessageProperties.ContainsKey("httpRequest"))
            {
                var headers = OperationContext.Current.IncomingMessageProperties["httpRequest"];
                {
                    var ip = ((HttpRequestMessageProperty)headers).Headers["_dp2router_clientip"];
                    if (string.IsNullOrEmpty(ip) == false)
                        results.Add(new RemoteAddress(ip, "", "dp2Router"));
                }

                // 2018/3/30
                var clientIpList = ((HttpRequestMessageProperty)headers).Headers["_clientip"];
                if (string.IsNullOrEmpty(clientIpList) == false)
                {
                    string[] list = clientIpList.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in list)
                    {
                        var entry = s.Trim();
                        if (string.IsNullOrEmpty(entry))
                            continue;
                        List<string> parts = StringUtil.ParseTwoPart(entry, ":");
                        results.Add(new RemoteAddress(parts[1], "", parts[0]));
                    }
                }

            }

            results.Add(GetNearestClientAddress());
            return results;
        }

        public RemoteAddress GetNearestClientAddress()
        {
            string strIP = "";
            MessageProperties prop = OperationContext.Current.IncomingMessageProperties;

            string strVia = prop.Via.ToString();

            if (prop.Via.Scheme == "net.pipe")
            {
                // 没有 IP
                strIP = "::1";  // "localhost";
                return new RemoteAddress(strIP, strVia, "");
            }
            try
            {
                RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                strIP = endpoint.Address;
                if (string.IsNullOrEmpty(strIP))
                    throw new Exception("endpoint.Address 为空");
            }
            catch (Exception ex)
            {
                strIP = "";
                // 2016/11/2
                this.app.WriteErrorLog("GetNearestClientAddress() 中出现异常(strVia='" + strVia + "'): " + ExceptionUtil.GetDebugText(ex));
            }
            // strVia = prop.Via.ToString();

            // 如果是 rest.http 协议类型，其 Via 的字符串要多出一截。多出来 API Name。需要截掉。
            if (prop.Encoder != null
                && prop.Encoder.MediaType == "application/json")
            {
                int nRet = strVia.LastIndexOf("/");
                if (nRet != -1)
                    strVia = strVia.Substring(0, nRet);
            }

            return new RemoteAddress(strIP, strVia, "");
        }

        // return:
        //      -2  达到通道配额上限
        //      -1  一般错误
        //      0   成功
        int InitialSession(out string strError)
        {
            strError = "";

            List<RemoteAddress> address_list = GetClientAddress();
            try
            {
                // sessioninfo = new SessionInfo(app, GetClientAddress());
                this.sessioninfo = this.app.SessionTable.PrepareSession(this.app,
    OperationContext.Current.SessionId,
    address_list);
            }
            catch (OutofSessionException ex)
            {
                RemoteAddress address = RemoteAddress.FindClientAddress(address_list, "");

                // TODO: 注意防止错误日志文件耗费太多空间
                // TODO: 这里适合对现有通道做一些清理。由于 IP 相同并不意味着就是来自同一台电脑，因此需要 MAC 地址、账户名等其他信息辅助判断
                this.WriteDebugInfo("*** 前端 '"
                    + (address == null ? "" : address.ClientIP + "@" + address.Via)
                    + "' 新分配通道的请求被拒绝:" + ex.Message);
                // OperationContext.Current.InstanceContext.ReleaseServiceInstance();

                // 为了防止攻击，需要立即切断通道。否则 1000 个通道很快会被耗尽
                try
                {
                    OperationContext.Current.Channel.Close(new TimeSpan(0, 0, 1));  // 一秒
                }
                catch
                {
                    // TimeoutException
                }
                strError = "InitialSession() 出现异常: " + ex.Message;
                return -2;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }

        // 准备Application和SessionInfo环境
        // return:
        //      Value == 0  正常
        //      Value == -1 不正常
        LibraryServerResult PrepareEnvironment(
            string strApiName,
            bool bPrepareSessionInfo,
            bool bCheckLogin = false,
            bool bCheckHangup = false)
        {
#if DEBUG
            if (bPrepareSessionInfo == false)
            {
                Debug.Assert(bCheckLogin == false, "bPrepareSessionInfo为false时， bCheckLogin必须为false");
            }
#endif
            LibraryServerResult result = new LibraryServerResult();

            string strError = "";
            int nRet = 0;

            if (this.sessioninfo != null)
            {
                // Session 如果曾经被释放过
                if (this.sessioninfo.Closed == true)
                {
                    // TODO: 其实也可以不报错，直接开辟新的通道继续操作

                    this._ip = "";
                    this.sessioninfo = null;
                    OperationContext.Current.InstanceContext.ReleaseServiceInstance();
                    result.Value = -1;
                    result.ErrorInfo = "通道先前已经被释放，本次操作失败。请重试操作";
                    result.ErrorCode = ErrorCode.ChannelReleased;
                    return result;
                }
            }

            if (this.app == null)
            {
                nRet = InitialApplication(out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = "InitialApplication fail: " + strError;
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }
                Debug.Assert(this.app != null, "");
            }

            if (bPrepareSessionInfo == true
                && this.sessioninfo == null)
            {
                if (OperationContext.Current.SessionId == null)
                {
                    //OperationContext.Current.Channel.Closing += Channel_Closing;
                    //OperationContext.Current.Channel.Closed += Channel_Closed;

                    this.RestMode = true;

                    string strCookie = WebOperationContext.Current.IncomingRequest.Headers["Cookie"];
                    Hashtable table = StringUtil.ParseParameters(strCookie, ';', '=');
                    string strSessionID = (string)table["sessionid"];
                    if (string.IsNullOrEmpty(strSessionID) == true)
                    {
                        strSessionID = Guid.NewGuid().ToString();
                        table["sessionid"] = strSessionID;
                        table["path"] = "/";
                        WebOperationContext.Current.OutgoingResponse.Headers.Add("Set-Cookie", StringUtil.BuildParameterString(table, ';', '='));   // "sessionid=" + strSessionID + "; path=/"
                    }

                    // TODO: 需要按照前端 IP 地址对 sessionid 个数进行管理。如果超过一定数目则限制这个 IP 创建新的 sessionid，但不影响到其他 IP 创建新的 sessionid

                    List<RemoteAddress> address_list = GetClientAddress();
                    try
                    {
                        this.sessioninfo = this.app.SessionTable.PrepareSession(this.app,
                            strSessionID,
                            address_list,
                            strApiName == "Logout" ? false : true);
                        // 2008/8/17
                        if (this.sessioninfo == null && strApiName == "Logout")
                        {
                            // Logout 时发现 session 并不存在
                            this._ip = "";
                            this.sessioninfo = null;
                            OperationContext.Current.InstanceContext.ReleaseServiceInstance();
                            result.Value = -1;
                            result.ErrorInfo = "通道先前已经被释放，本次 Logout 操作失败";
                            result.ErrorCode = ErrorCode.ChannelReleased;
                            return result;
                        }
                    }
#if NO
                    catch (OutofSessionException ex)
                    {
                        OperationContext.Current.InstanceContext.ReleaseServiceInstance();
                        result.Value = -1;
                        result.ErrorInfo = "InitialSession fail: " + ex.Message;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
#endif
                    catch (Exception ex)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "dp2Library 初始化通道失败: " + ex.Message;
                        if (ex is OutofSessionException)
                        {
                            RemoteAddress address = RemoteAddress.FindClientAddress(address_list, "");

                            // TODO: 注意防止错误日志文件耗费太多空间
                            // TODO: 这里适合对现有通道做一些清理。由于 IP 相同并不意味着就是来自同一台电脑，因此需要 MAC 地址、账户名等其他信息辅助判断
                            this.WriteDebugInfo("*** 前端 '"
                                + (address == null ? "" : address.ClientIP + "@" + address.Via)
                                + "' 新分配通道的请求被拒绝:" + ex.Message);

                            result.ErrorCode = ErrorCode.OutofSession;
                            // OperationContext.Current.InstanceContext.ReleaseServiceInstance();

                            // 为了防止攻击，需要立即切断通道。否则 1000 个通道很快会被耗尽
                            try
                            {
                                OperationContext.Current.Channel.Close(new TimeSpan(0, 0, 1));  // 一秒
                            }
                            catch
                            {
                                // TimeoutException
                            }
                        }
                        else
                            result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
                }
                else
                {
                    // return:
                    //      -2  达到通道配额上限
                    //      -1  一般错误
                    //      0   成功
                    nRet = InitialSession(out strError);
                    if (nRet < 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "dp2Library 初始化通道失败: " + strError;
                        if (nRet == -2)
                            result.ErrorCode = ErrorCode.OutofSession;
                        else
                            result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
                    this.sessioninfo.NeedAutoClean = false; // WCF 自己管理这些通道，不需要主动清理
                }

                Debug.Assert(this.sessioninfo != null, "");
            }

            // 2011/1/27
            if (sessioninfo != null)
            {
                this.sessioninfo.CallCount++;

#if NO
                if (string.IsNullOrEmpty(this.sessioninfo.UserID) == true)
                    this.sessioninfo.UserID = "#";  // 为了显示的需要
#endif

                SetLang(sessioninfo.Lang);

                // 2017/5/7
                if (this.RestMode)
                    this.sessioninfo.Used++;
            }

            if (bPrepareSessionInfo == false && this.sessioninfo == null)
            {
                // 增量 NullIpTable，以便管理配额

                RemoteAddress address = GetNearestClientAddress();
                this._ip = address.ClientIP;
                this.app.SessionTable.IncNullIpCount(address.ClientIP, 1);
            }

            if (bCheckHangup == true)
            {
                if (app.HangupList.Count > 0)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCode.Hangup;
                    if (app.ContainsHangup("Expire"))
                        result.ErrorInfo = "系统当前处于维护状态，本功能暂时不能使用。原因: dp2library(又名图书馆应用服务器)(注意这是服务器模块) 版本太旧，请立即升级到最新版本";
                    else
                        result.ErrorInfo = "因系统处于维护状态 " + StringUtil.MakePathList(app.HangupList) + "，本功能暂时不能使用";
                    return result;
                }
            }

            if (bCheckLogin == true)
            {
                Debug.Assert(sessioninfo != null, "");
                if (sessioninfo != null && sessioninfo.UserID == "")
                {
                    result.Value = -1;
                    result.ErrorInfo = "尚未登录";
                    result.ErrorCode = ErrorCode.NotLogin;
                    return result;
                }
            }

            return result;
        }

        /*
        private void Channel_Closed(object sender, EventArgs e)
        {
            this.app.SessionTable.DeleteSession(sessioninfo);

            OperationContext.Current.Channel.Closed -= Channel_Closed;
        }

        private void Channel_Closing(object sender, EventArgs e)
        {
            this.app.SessionTable.DeleteSession(sessioninfo);

            OperationContext.Current.Channel.Closing -= Channel_Closing;
        }
        */

        #endregion

        // 2012/4/15
        // 获得版本号
        public LibraryServerResult GetVersion(out string uid)
        {
#if NO
            throw new WebFaultException<string>(
     "测试 My error description.", HttpStatusCode.BadRequest); 
#endif

            uid = "";
            LibraryServerResult result = this.PrepareEnvironment("GerVersion", false);
            if (result.Value == -1)
                return result;

            Debug.Assert(app != null, "");

#if NO
            if (app.LibraryCfgDom != null && app.LibraryCfgDom.DocumentElement != null)
                uid = app.LibraryCfgDom.DocumentElement.GetAttribute("uid");    // 这个写法的问题是可能 app.UID 还来不及保存到 xml 文件，那么就无法通过 xml 文件获得最新信息了
            else
                uid = "";
#endif

            uid = app.UID;  // 2015/7/6

            // LibraryServerResult result = new LibraryServerResult();
            result.Value = 0;
            result.ErrorInfo = LibraryApplication.Version;  // "2.18";
            return result;
        }

        // 登录
        // parameters:
        //      strUserName 用户名
        //      strPassword 密码
        //      strParamaters   登录参数 location=???,index=???,type=reader
        //              如果是 public 登录，type应该是worker，不能是reader。因为reader会被程序把strUserName当成条码号来登录。public登录成功后，身份还是reader
        // return: result.Value:
        //      -1  error
        //      0   user not found, or password error
        //      1   succeed
        public LibraryServerResult Login(string strUserName,
            string strPassword,
            string strParameters,
            out string strOutputUserName,
            out string strRights,
            out string strLibraryCode)
        {
            strRights = "";
            strOutputUserName = "";
            strLibraryCode = "";
            /*
            string strCookie = WebOperationContext.Current.IncomingRequest.Headers["Cookie"];
            WebOperationContext.Current.OutgoingResponse.Headers.Add("Set-Cookie", "name=value");
             * */

            LibraryServerResult result = this.PrepareEnvironment("Login", true);
            if (result.Value == -1)
                return result;

            try
            {
                string strError = "";
                int nRet = 0;

                if (string.IsNullOrEmpty(strUserName) == true)
                {
                    strError = "用户名不能为空";
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strParameters) == false
    && strParameters.IndexOf("=") == -1)
                {
                    strError = "strParameters参数应该采用新的用法： location=???,index=???";
                    goto ERROR1;
                }

                Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');

                // 2018/7/11
                // 使用可信的前端提供的 clientip 参数
                string strClientIP = parameters.ContainsKey("clientip") == true ? parameters["clientip"] as string : sessioninfo.ClientIP;

                nRet = app.UserNameTable.BeforeLogin(strUserName,
                strClientIP,
                out strError);
                if (nRet == -1)
                {
                    // app.WriteErrorLog("前端 ["+sessioninfo.ClientIP+"] 对用户名 ["+strUserName+"] 进行了密码试探攻击，被惩罚延时: " + strError);
                    goto ERROR1;
                }

                // 避免有安全漏洞
                if (strPassword == null)
                    strPassword = "";

                // 判断是否允许用 token: 登录
                if (string.IsNullOrEmpty(strPassword) == false
                    && StringUtil.HasHead(strPassword, "token:") == true)
                {
                    strError = "不允许用 token 登录";
                    goto ERROR1;
                }

                // 评估模式
                if (parameters.ContainsKey("testmode") == true)
                {
                    if (app.TestMode == false)
                    {
                        strError = "dp2library 服务器当前不允许使用评估模式";
                        sessioninfo.Account = null;
                        goto ERROR1;
                    }
                    sessioninfo.TestMode = true;
                }

                // 加入失效 MAC 地址表格
                string strExpire = (string)parameters["expire"];
                if (string.IsNullOrEmpty(strExpire) == false)
                {
                    List<string> macs = StringUtil.SplitList(strExpire, '|');
                    foreach (string s in macs)
                    {
                        if (app.ExpireMacTable.ContainsKey(s) == false)
                            app.ExpireMacTable[s] = true;
                    }
                }

                // 观察 Client MAC 地址是否在失效列表中
                string strMac = (string)parameters["mac"];
                if (string.IsNullOrEmpty(strMac) == false)
                {
                    List<string> macs = StringUtil.SplitList(strMac, '|');
                    foreach (string mac in macs)
                    {
                        if (app.ExpireMacTable.ContainsKey(mac) == true)
                        {
                            strError = "此前端已被禁用";
                            goto ERROR1;
                        }
                    }
                }

                string strLocation = (string)parameters["location"];
                if (strLocation == null)
                    strLocation = "";   // 2016/10/25 添加。防范 library.xml 中脚本函数 ItemCanBorrow() 或者 ItemCanReturn() 中使用 account.Location.IndexOf() 时抛出异常
                string strLibraryCodeList = (string)parameters["libraryCode"];
                if (string.IsNullOrEmpty(strLibraryCodeList) == false)
                    strLibraryCodeList = strLibraryCodeList.Replace("|", ",");

                // 2013/12/24
                string strLang = (string)parameters["lang"];
                SetLang(strLang);

                // 2015/1/16
                // gettoken 的参数值是有效期 day / month / year
                string strGetToken = (string)parameters["gettoken"];

                // TODO: 图书馆代码列表需要和当前 sessioninfo 的馆代码列表交叉，排除可能多余的部分

                bool bReader = false;
                string strType = (string)parameters["type"];
                if (strType == null)
                    strType = "";
                if (strType.ToLower() == "reader")
                    bReader = true;

                List<string> alter_type_list = null;

#if NO
                bool bSimulateLogin = false;
                string strSimulate = (string)parameters["simulate"];
                if (strSimulate == null)
                    strSimulate = "";
                strSimulate = strSimulate.ToLower();
                if (strSimulate == "yes" || strSimulate == "on"
                || strSimulate == "1" || strSimulate == "true")
                    bSimulateLogin = true;
#endif
                bool bSimulateLogin = StringUtil.GetBooleanValue((string)parameters["simulate"], false);
                string strLimitRights = ""; // 代理者的权限。用于限定(代理登录方式下)被代理者的危险权限
                if (bReader == false)
                {
                    // 2021/7/8
                    // 检查特殊账户名
                    if (strUserName.ToLower() == "reader")
                    {
                        strError = "不允许使用名为 reader 的账户直接登录";
                        goto ERROR1;
                    }

                    if (bSimulateLogin == true)
                    {
#if NO
                        strError = "strParameters 中 type 子参数值为 reader 以外类型时，不允许进行 simulate 方式的登录";
                        goto ERROR1;
#endif
                        if (String.IsNullOrEmpty(strPassword) == true)
                        {
                            strError = "simulate 状态下登录时，strPassword 参数不能为空";
                            goto ERROR1;
                        }

                        SimulateLoginInfo info = ParseManagerUserNamePassword(strPassword);
                        if (info.ManagerPassword == null)
                            info.ManagerPassword = "";  // 保护好

                        if (string.IsNullOrEmpty(info.UserToken) == true)
                        {
#if NO
                            strError = "simulate 状态下登录(模拟工作人员帐户)时，strPassword 参数内必须提供被模拟帐户的 token 参数";
                            goto ERROR1;
#endif
                            // 2016/10/21
                            // 令第二阶段不要进行密码判断
                            strPassword = null;
                        }
                        else
                        {
#if NO
                            // 令第二阶段不要进行密码判断
                            strPassword = null;
#endif
                            strPassword = "token:" + info.UserToken;
                        }

                        // 第一阶段：先用 manager 用户名和密码验证
                        try
                        {
                            string strTemp = "";
                            List<string> temp_list = null;
                            // 工作人员登录(代理者登录)
                            nRet = sessioninfo.Login(info.ManagerUserName,
                                 info.ManagerPassword,
                                 "#simulate",
                                 false,
                                 null,
                                 "*",
                                 null,
                                 out bool passwordExpired1,
                                 out temp_list,
                                 out strRights,
                                 out strTemp,
                                 out strError);
                            if (nRet != 1)
                            {
                                strError = "simulate 登录时第一阶段利用管理员帐户 '" + info.ManagerUserName + "' 验证登录失败: " + strError;
                                if (nRet == 0 || nRet == 1)
                                {
                                    string strLogText = app.UserNameTable.AfterLogin(info.ManagerUserName,
                                        strClientIP,
                                        nRet);
                                    if (string.IsNullOrEmpty(strLogText) == false)
                                        app.WriteErrorLog("!!!(simulate 1) " + strLogText);
                                }
                                goto ERROR1;
                            }

                            if (passwordExpired1 == true)
                            {
                                strError = "simulate 登录时第一阶段利用管理员帐户 '" + info.ManagerUserName + "' 验证登录失败: 密码已经失效";
                                sessioninfo.Account = null;
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.PasswordExpired;
                                return result;
                            }

                            // 这种情况下使用代理者的权限
                            if (strPassword == null)
                                strLimitRights = sessioninfo.Rights;

                            // 检查工作人员帐户是否具备 simulateworker 权限
                            if (StringUtil.IsInList("simulateworker", strRights) == false)
                            {
                                result.Value = -1;
                                result.ErrorInfo = "模拟工作人员登录被拒绝。帐户 '" + info.ManagerUserName + "' 不具备 simulateworker 权限";
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
                        }
                        finally
                        {
                            // 防止出现漏洞
                            sessioninfo.Account = null;
                        }

                        // 第二阶段：
                    }

                    // 工作人员登录(或者被代理者登录)
                    nRet = sessioninfo.Login(strUserName,
                         strPassword,
                         strLocation,
                         StringUtil.HasHead(strLocation, "#opac") == true || strLocation == "@web" ? true : false,  // todo: 判断 #opac 或者 #opac_xxxx
                         sessioninfo.ClientIP,
                         sessioninfo.RouterClientIP,
                         strGetToken,
                         out bool passwordExpired2,
                         out alter_type_list,
                         out strRights,
                         out strLibraryCode,
                         out strError);
                    strOutputUserName = strUserName;

                    // 2016/10/21
                    if (string.IsNullOrEmpty(strLimitRights) == false)
                    {
#if NO
                        if (sessioninfo.Account != null)
                        {
                            strRights = strLimitRights;
                            sessioninfo.Account.Rights = strLimitRights;
                        }
#endif
                        // 2017/1/17
                        if (sessioninfo.Account != null)
                        {
                            string removed = "";
                            sessioninfo.Account.Rights = LibraryApplication.LimitRights(strRights, strLimitRights, out removed);
                        }
                    }

                    if (nRet == 1 && passwordExpired2 == true)
                    {
                        strError = $"'{strUserName }' 登录失败: 密码已经失效";
                        sessioninfo.Account = null;
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.PasswordExpired;
                        return result;
                    }
                }
                else
                {
                    if (bSimulateLogin == true)
                    {
                        //string strManagerUserName = "";
                        //string strManagerPassword = "";

                        if (String.IsNullOrEmpty(strPassword) == true)
                        {
                            strError = "simulate 状态下登录时，strPassword 参数不能为空";
                            goto ERROR1;
                        }

#if NO
                        nRet = strPassword.IndexOf(",");
                        if (nRet == -1)
                            strManagerUserName = strPassword;
                        else
                        {
                            strManagerUserName = strPassword.Substring(0, nRet);
                            strManagerPassword = strPassword.Substring(nRet + 1);
                        }
#endif

                        SimulateLoginInfo info = ParseManagerUserNamePassword(strPassword);
                        if (info.ManagerPassword == null)
                            info.ManagerPassword = "";  // 保护好

                        if (string.IsNullOrEmpty(info.UserToken) == false)
                            strPassword = "token:" + info.UserToken;
                        else
                        {
                            // 令第二阶段不要进行密码判断
                            strPassword = null;
                        }

                        // TODO: 是否限定当代理账户密码为空时，前端必须和服务器同一台机器？另外一个方法是规定这种特殊账户必须在账户特性里面限定前端的 IP 地址
                        // 第一阶段：先用manager用户名和密码验证
                        try
                        {
                            string strTemp = "";
                            List<string> temp_list = null;

                            // 工作人员登录
                            nRet = sessioninfo.Login(info.ManagerUserName,
                                 info.ManagerPassword,
                                 "#simulate",
                                 false,
                                 null,
                                 "*",
                                 null,
                                 out bool passwordExpired3,
                                 out temp_list,
                                 out strRights,
                                 out strTemp,
                                 out strError);
                            if (nRet != 1)
                            {
                                strError = "simulate 登录时第一阶段利用管理员帐户 '" + info.ManagerUserName + "' 验证登录失败: " + strError;
                                if (nRet == 0 || nRet == 1)
                                {
                                    string strLogText = app.UserNameTable.AfterLogin(info.ManagerUserName,
                                        strClientIP,
                                        nRet);
                                    if (string.IsNullOrEmpty(strLogText) == false)
                                        app.WriteErrorLog("!!!(simulate 2) " + strLogText);
                                }
                                goto ERROR1;
                            }

                            // 检查工作人员帐户是否具备 simulatereader 权限
                            if (StringUtil.IsInList("simulatereader", strRights) == false)
                            {
                                result.Value = -1;
                                result.ErrorInfo = "模拟读者登录被拒绝。帐户 '" + info.ManagerUserName + "' 不具备 simulatereader 权限";
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }

                            if (nRet == 1 && passwordExpired3 == true)
                            {
                                strError = "simulate 登录时第一阶段利用管理员帐户 '" + info.ManagerUserName + "' 验证登录失败: 密码已经失效";
                                sessioninfo.Account = null;
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.PasswordExpired;
                                return result;
                            }
                        }
                        finally
                        {
                            // 防止出现漏洞
                            sessioninfo.Account = null;
                        }

                        // 第二阶段：
                    }

                    string strIndex = (string)parameters["index"];
                    int nIndex = -1;
                    if (String.IsNullOrEmpty(strIndex) == false)
                    {
                        if (Int32.TryParse(strIndex, out nIndex) == false)
                        {
                            strError = "strParameters参数中的index子参数值 '" + strIndex + "' 格式不正确，应当为整数字符串";
                            goto ERROR1;
                        }
                    }

                    // 读者身份登录
                    // return:
                    //      -1  error
                    //      0   登录未成功
                    //      1   登录成功
                    //      >1  有多个账户符合条件。
                    nRet = app.LoginForReader(sessioninfo,
                        strUserName,
                        strPassword,
                        strLocation,
                        strLibraryCodeList,
                        nIndex, // -1,
                        strGetToken,
                        out bool passwordExpired10,
                        out alter_type_list,
                        out strOutputUserName,
                        out strRights,
                        out strLibraryCode,
                        out strError);

                    // 2021/7/5
                    if (nRet != -1 && passwordExpired10 == true)
                    {
                        strError = "密码已经失效";
                        sessioninfo.Account = null;
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.PasswordExpired;
                        return result;
                    }
                }

                if (app.LicenseType == "server")
                    strRights += ",serverlicensed";

                // 对试探密码的处理
#if NO
                if (nRet == 0)
                {
                    if (sessioninfo.LoginErrorCount > 0)
                        Thread.Sleep(1000 * sessioninfo.LoginErrorCount); // 越来越慢
                    sessioninfo.LoginErrorCount++;
                }
                else if (nRet == 1)
                {
                    sessioninfo.LoginErrorCount = 0;    // 复位，重新计算
                }
#endif
                if (nRet == 0 || nRet == 1)
                {
                    string strLogText = app.UserNameTable.AfterLogin(strUserName,
                        strClientIP,
                        nRet);
                    if (string.IsNullOrEmpty(strLogText) == false)
                        app.WriteErrorLog("!!! " + strLogText);
                }

                // 检查前端版本
                if (nRet == 1
                    && StringUtil.IsInList("checkclientversion", strRights) == true
                    )
                {
                    string strClientVersion = (string)parameters["client"];
                    if (string.IsNullOrEmpty(strClientVersion) == true)
                    {
                        sessioninfo.Account = null;
                        strError = "前端版本太旧，未达到 dp2library 服务器对前端版本的最低要求，登录失败。请立即升级前端程序到最新版本";
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ClientVersionTooOld;
                        return result;
                    }
                    // 参数值格式为 clientname|versionstring
                    strError = CheckClientVersion(strClientVersion);
                    if (string.IsNullOrEmpty(strError) == false)
                    {
                        sessioninfo.Account = null;
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ClientVersionTooOld;
                        return result;
                    }
                }

                /// 处理短信验证码登录有关的流程
                if (nRet == 1)
                {
                    if (DoSmsCheck(sessioninfo,
                parameters,
                alter_type_list,
                result) == true)
                    {
                        sessioninfo.Account = null; // 2017/5/5 堵住漏洞
                        return result;
                    }
                }

                // END1:
                result.Value = nRet;
                result.ErrorInfo = strError;
                if (nRet == 0)
                    result.ErrorCode = ErrorCode.NotFound;
                if (nRet == -1)
                    result.ErrorCode = ErrorCode.SystemError;

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Login() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // return:
        //      true    本函数返回后立即结束
        //      false   继续
        bool DoSmsCheck(SessionInfo sessioninfo,
            Hashtable parameters,
            List<string> alter_type_list,
            LibraryServerResult result)
        {
            string strError = "";
            int nRet = 0;

            // 用于构造验证码名称的 IP 地址
            string strClientIP = sessioninfo.ClientIP;
            if (string.IsNullOrEmpty(sessioninfo.RouterClientIP) == false)
                strClientIP = "r:" + sessioninfo.RouterClientIP;

            List<string> rest_list = null;
            string strPhoneNumber = (string)parameters["phoneNumber"];
            string strTempCode = (string)parameters["tempCode"];
            if (string.IsNullOrEmpty(strTempCode) == false)
            {
                // 准备手机短信验证登录的第二阶段：匹配验证码
                if (sessioninfo.Account == null)
                {
                    strError = "手机短信验证方式登录时，sessioninfo.Account 不应为 null";
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(strPhoneNumber))
                {
                    strError = "当包含 tempCode 参数时，也要包含 phoneNumber 参数";
                    goto ERROR1;
                }

                if (sessioninfo.Account.MatchTempPassword(app.TempCodeTable,
                    strPhoneNumber,
                    strClientIP,
                    strTempCode,
                    out strError) == false)
                {
                    sessioninfo.Account = null; // 2017/5/5 堵住漏洞

                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.TempCodeMismatch;
                    return true;
                }

                alter_type_list.Add("sms"); // 表示验证过了

                // 未处理的、和需要补做的类型
                rest_list = GetRest(sessioninfo, alter_type_list);

            }
            else
            {
#if NO
                bool bAlterSms = false;
                if (alter_type_list != null && alter_type_list.IndexOf("sms") != -1)
                    bAlterSms = true;
#endif

                // 未处理的、和需要补做的类型
                rest_list = GetRest(sessioninfo, alter_type_list);

#if NO
                // 竖线间隔的手机号码列表
                // return:
                //      null    没有找到前缀
                //      ""      找到了前缀，并且值部分为空
                //      其他     返回值部分
                string strSmsBinding = sessioninfo.Account == null ? "" : sessioninfo.Account.GetPhoneNumberBindingString();

                // TODO: 如果 strSmsBinding 里面包含了 *ip，而 IP 检查已经进行过了，就可以不用要求验证码了

                if ((bAlterSms == true || string.IsNullOrEmpty(strSmsBinding) == false)
                    && string.IsNullOrEmpty(strPhoneNumber) == true)

#endif

                if (rest_list.Count == 1 && rest_list.IndexOf("sms") != -1
                    && string.IsNullOrEmpty(strPhoneNumber) == true)
                {
                    sessioninfo.Account = null;
                    result.Value = 0;
                    result.ErrorInfo = "此账户当前无法用传统方式登录。可改用手机号+验证码方式登录";
                    result.ErrorCode = ErrorCode.NeedSmsLogin;
                    return true;
                }

                // TODO: 走到这里。如果有多种类型没有验证，只要里面有 ip 和 router_ip 就不必提示 sms 没有通过验证了。因为确实没有给 sms 验证的机会(这样也不必要，因为即便通过 sms 验证也无法通过全部验证)

                if (string.IsNullOrEmpty(strPhoneNumber) == false)
                {
                    // 准备手机短信验证登录的第一阶段：产生验证码

                    if (sessioninfo.Account == null)
                    {
                        strError = "手机短信验证方式登录时，sessioninfo.Account 不应为 null";
                        goto ERROR1;
                    }

                    // 
                    TempCode code = null;

                    // return:
                    //      -1  出错
                    //      0   沿用以前的验证码
                    //      1   用新的验证码
                    int nPasswordRet = sessioninfo.Account.PrepareTempPassword(
                        app.TempCodeTable,
                        strClientIP,
                        strPhoneNumber,
                        out code,
                        out strError);
                    if (nPasswordRet == -1)
                        goto ERROR1;

                    if (nPasswordRet == 1)
                    {
                        string strMessage = "登录验证码为 " + code.Code + "。(用户 " + sessioninfo.Account.UserID + ") " + Account.TempCodeExpireLength.TotalMinutes + "分钟以后失效";
                        string strLibraryName = app.LibraryName;
                        if (string.IsNullOrEmpty(strLibraryName) == false)
                        {
                            strMessage = "登录验证码为 " + code.Code + "。(用户 " + sessioninfo.Account.UserID + " " + strLibraryName + ") " + Account.TempCodeExpireLength.TotalMinutes + "分钟以后失效";
                        }

                        // 通过 MSMQ 发送手机短信
                        // parameters:
                        //      strUserName 账户名，或者读者证件条码号，或者 "@refID:xxxx"
                        nRet = app.SendSms(
                            sessioninfo.Account.UserID,
                            strPhoneNumber,
                            strMessage,
                            out strError);
                        if (nRet == -1)
                        {
                            // 出错后，清除已经存储的验证码
                            app.TempCodeTable.Remove(code.Key);
                            goto ERROR1;
                        }
                    }

                    // 返回 0 并且 ErrorCode 为 RetryLogin，表示已经发出短信，需要再次登录(携带刚收到的验证码)
                    sessioninfo.Account = null;
                    result.Value = 0;
                    if (nPasswordRet == 0)
                        result.ErrorInfo = "请使用手机 " + strPhoneNumber + " 早先收到过的短信验证码再次提交登录";
                    else
                        result.ErrorInfo = "验证短信已经发送给手机 " + strPhoneNumber + "。请使用收到的短信验证码再次提交登录";

#if NO
                    if (nPasswordRet == 0)
                        result.ErrorInfo = "请使用手机号 " + strPhoneNumber + "(" + code.Code + ") 早先收到过的验证码再次提交登录";
                    else
                        result.ErrorInfo = "验证短信已经发送给手机号 " + strPhoneNumber + "(" + code.Code + ")。请使用收到的验证码再次提交登录";
#endif

                    result.ErrorCode = ErrorCode.RetryLogin;
                    return true;
                }
            }

            // 最后看看是否还有剩余的未能验证的
            {
                if (rest_list.Count > 0)
                {
                    sessioninfo.Account = null;
                    result.Value = 0;
                    result.ErrorInfo = "登录失败。方式 " + StringUtil.MakePathList(rest_list) + " 未通过验证";
                    result.ErrorCode = ErrorCode.NotFound;  // TODO: 需要一个新的错误码 VerifyFail
                    return true;
                }
            }

            return false;
        ERROR1:
            sessioninfo.Account = null; // 2017/5/5 堵住漏洞
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return true;
        }

        static List<string> GetRest(SessionInfo sessioninfo, List<string> alter_type_list)
        {
            // 所有绑定类型
            List<string> types = Account.GetBindingTypes(sessioninfo.Account.Binding);
            if (sessioninfo.RouterClientIP == null)
                types.Remove("router_ip");

            // 没有处理过的类型
            List<string> not_process = Account.Sub(types, Account.GetProcessed(alter_type_list));

            // 需要补做的类型
            List<string> rest_list = Account.MergeBindingType(alter_type_list);

            // 合并两个数组
            rest_list.AddRange(not_process);
            StringUtil.RemoveDupNoSort(ref rest_list);

            // 对于剩下的类型，要检查是否有已经验证过的类型可以替代
            // 从 types 列表中移走那些验证过的可以替代的类型
            // parameters:
            //      types   需要检查的类型
            //      processed   已经验证过的类型
            Account.RemoveAlterTypes(ref rest_list,
                Account.GetMatched(alter_type_list),
                sessioninfo.Account.Binding);

            return rest_list;
        }

        static string base_version = "2.14";

        static string CheckClientVersion(string strText)
        {
            StringUtil.ParseTwoPart(strText, "|", out string strName, out string strVersion);
            if (string.IsNullOrEmpty(strName) == true
                || string.IsNullOrEmpty(strVersion) == true)
                return "前端版本太旧，未达到 dp2library 服务器对前端版本的最低要求，登录失败。请立即升级前端程序到最新版本";
            Version version = new Version(strVersion);
            strName = strName.ToLower();
            if (strName == "dp2circulation")
            {
                if (version.CompareTo(new Version(base_version)) < 0) // 2.14
                    return $"前端 dp2circulation (内务)版本太旧(低于{base_version})，登录失败。请立即升级到最新版本";
            }

            return null;    // 表示版本满足要求
        }

#if NO
        // 取出 12.34 形态
        static double GetVersionValue(string strVersion)
        {
            string[] parts = strVersion.Split(new char[] { '.' });
            List<string> list = parts.ToList<string>();
            while(list.Count > 2)
            {
                list.RemoveAt(2);
            }
            double result = 0;
            if (double.TryParse(string.Join(".", list.ToArray()), out result) == false)
                return 0;
            return result;
        }
#endif

        class SimulateLoginInfo
        {
            public string ManagerUserName = "";
            public string ManagerPassword = "";
            public string UserToken = "";   // 被模拟帐户的 token
        }

        // 解析出 xxxx,xxxx 两个部分
        static SimulateLoginInfo ParseManagerUserNamePassword(string strText)
        {
            SimulateLoginInfo info = new SimulateLoginInfo();

            if (String.IsNullOrEmpty(strText) == true)
                return info;

            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, "|||", out strLeft, out strRight);
            if (string.IsNullOrEmpty(strRight) == false)
            {
                if (StringUtil.HasHead(strRight, "token:") == true)
                    info.UserToken = strRight.Substring("token:".Length);
            }

            strText = strLeft;

            int nRet = strText.IndexOf(",");
            if (nRet == -1)
                info.ManagerUserName = strText;
            else
            {
                info.ManagerUserName = strText.Substring(0, nRet);
                info.ManagerPassword = strText.Substring(nRet + 1);
            }

            return info;
        }

        // 登出
        public LibraryServerResult Logout()
        {
            LibraryServerResult result = this.PrepareEnvironment("Logout", true);
            if (result.Value == -1)
                return result;

            try
            {
                sessioninfo.Account = null;

                if (OperationContext.Current.SessionId == null)
                {
                    // REST 情形下
                    // Session.Abandon(); 
                    this.app.SessionTable.DeleteSession(sessioninfo);
                }
                else
                {
                    // 其他具有Session的binding情形下
                    OperationContext.Current.InstanceContext.ReleaseServiceInstance();
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Logout() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        public LibraryServerResult SetLang(string strLang,
            out string strOldLang)
        {
            strOldLang = "";
            LibraryServerResult result = this.PrepareEnvironment("SetLang", true);
            if (result.Value == -1)
                return result;

            strOldLang = sessioninfo.Lang;
#if NO
            if (String.IsNullOrEmpty(strLang) == false)
                sessioninfo.Lang = strLang;
#endif
            // 2016/1/28
            SetLang(strLang);

            // END1:
            result.Value = 0;
            result.ErrorInfo = "";
            return result;
            /*
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
             * */
        }

        object m_nInSearching = 0;

        public int InSearching
        {
            get
            {
                return (int)m_nInSearching;
            }
            set
            {
                m_nInSearching = value;
            }
        }

        public int BeginSearch()
        {
            this._nStop = 0;
            lock (this.m_nInSearching)
            {
                int v = (int)m_nInSearching;
                m_nInSearching = v + 1;
                return v;
            }
        }

        public void EndSearch()
        {
            lock (this.m_nInSearching)
            {
                int v = (int)m_nInSearching;
                m_nInSearching = v - 1;
            }
            this._nStop = 1;
        }

        public void Stop()
        {
            /*
            LibraryServerResult result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return;
             * */

            if (this.InSearching > 0)
            {
                this._nStop = 1;

                WriteDebugInfo("因后一个stop的到来，前一个search不得不中断 ");
            }
        }

        // 验证读者密码
        // parameters:
        //      strReaderBarcode    读者证条码号。
        //                          如果为 "!getpatrontempid:" 开头，表示希望返回(关于一个读者的)二维码字符串，这个操作需要具有 getpatrontempid 权限才行
        // Result.Value -1出错 0密码不正确 1密码正确
        // 权限: 
        //		工作人员或者读者，必须有verifyreaderpassword权限
        //		如果为读者, 附加限制还只能验证属于自己的密码
        public LibraryServerResult VerifyReaderPassword(
            string strReaderBarcodeParam,
            string strReaderPassword)
        {
            LibraryServerResult result = this.PrepareEnvironment("VerifyReaderPassword", true, true);
            if (result.Value == -1)
                return result;

            LibraryApplication.ParseOI(strReaderBarcodeParam, out string strReaderBarcode, out string strOwnerInstitution);

            string strCommand = "";
            if (string.IsNullOrEmpty(strReaderBarcode) == false
                && StringUtil.HasHead(strReaderBarcode, "!getpatrontempid:") == true)
            {
                // 命令方式
                strCommand = "getpatrontempid";
                strReaderBarcode = strReaderBarcode.Substring("!getpatrontempid:".Length);
            }

            string strToken = "";
            if (string.IsNullOrEmpty(strReaderPassword) == false
                && StringUtil.HasHead(strReaderPassword, "token:") == true)
            {
                strToken = strReaderPassword.Substring("token:".Length);
            }
            // 权限判断

            // 权限字符串
            if (strCommand == "getpatrontempid")
            {
                if (StringUtil.IsInList("getpatrontempid", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得读者证号二维码被拒绝。不具备 getpatrontempid 权限";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strToken) == false)
                {
                    if (StringUtil.IsInList("verifytoken", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "验证 token 被拒绝。不具备 verifytoken 权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else
                {
                    if (StringUtil.IsInList("verifyreaderpassword", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "验证读者密码被拒绝。不具备 verifyreaderpassword 权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            try
            {
                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    if (sessioninfo.Account != null
                        && strReaderBarcode != sessioninfo.Account.Barcode)
                    {
                        result.Value = -1;
                        if (strCommand == "getpatrontempid")
                            result.ErrorInfo = "获得读者证号二维码被拒绝。作为读者只能获得自己的读者证号二维码";
                        else
                        {
                            if (string.IsNullOrEmpty(strToken) == false)
                                result.ErrorInfo = "验证 token 被拒绝。作为读者只能验证自己的 token";
                            else
                                result.ErrorInfo = "验证读者密码被拒绝。作为读者只能验证自己的密码";
                        }
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strXml = "";
                string strError = "";
                string strOutputPath = "";

                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = app.GetReaderRecXml(
                    channel,    // sessioninfo.Channels,
                    strReaderBarcode,
                    out strXml,
                    out strOutputPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "证条码号为 '" + strReaderBarcode + "' 的读者不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "获得证条码号为 '" + strReaderBarcode + "' 的读者记录时出错: " + strError;
                    goto ERROR1;
                }

                if (nRet > 1)
                {
                    strError = "系统错误: 证条码号为 '" + strReaderBarcode + "' 的读者记录多于一个";
                    goto ERROR1;
                }

                // 所操作的读者库的馆代码
                string strLibraryCode = "";

                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strOutputPath) == false)
                {
                    // 2021/3/3
                    // 获得读者库的馆代码
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = app.GetLibraryCode(
            strOutputPath,
            out strLibraryCode,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (app.IsCurrentChangeableReaderPath(strOutputPath,
            sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                // 2021/3/3
                // 补充判断机构代码
                if (strOwnerInstitution != null)
                {
                    // return:
                    //      -1  出错
                    //      0   没有通过较验
                    //      1   通过了较验
                    nRet = app.VerifyPatronOI(
                        strOutputPath,
                        strLibraryCode,
                        strOwnerInstitution,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                        return result;
                    }
                }

                if (strCommand == "getpatrontempid")
                {
                    result.Value = 1;
                    result.ErrorInfo = LibraryApplication.BuildQrCode(strReaderBarcode, app.UID);
                    return result;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                // 验证读者密码。包括普通密码和临时密码，或者 token
                // return:
                //      -1  error
                //      0   密码不正确
                //      1   密码正确
                nRet = app.VerifyReaderPassword(
                    sessioninfo.ClientIP,
                    readerdom,
                    strReaderPassword,
                    app.Clock.Now,
                    (StringBuilder)null,
                    out bool passwordExpired,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2021/7/5
                if (passwordExpired)
                {
                    result.Value = 0;
                    result.ErrorCode = ErrorCode.PasswordExpired;
                    result.ErrorInfo = "密码已经失效";
                    return result;
                }

                if (nRet == 0)
                {
                    result.Value = 0;
                    result.ErrorInfo = strError;
                }
                else
                {
                    result.Value = 1;
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library VerifyReaderPassword() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 修改读者密码
        //		工作人员或者读者，必须有changereaderpassword权限
        //		如果为读者, 附加限制还只能修改属于自己的密码
        //      2021/7/5
        //      工作人员身份调用本 API 修改读者密码，应先用工作人员身份登录。而读者调用本 API 修改自己的密码不需要先登录
        //      如果调用本 API 前已经用读者身份登录过了，并且打算调用本 API 修改一个不是登录者自己的读者的密码，那需要先 Logout() 再调用本 API 才行
        // parameters:
        //      strReaderOldPassword    旧密码。如果想达到不验证旧密码的效果，可以用 null 调用，但仅限工作人员身份调用的情况。读者身份是必须验证旧密码的
        // Result.Value
        //      -1  出错
        //      0   旧密码不正确
        //      1   旧密码正确,已修改为新密码
        // 权限: 
        //		工作人员或者读者，必须有changereaderpassword权限
        //		如果为读者, 附加限制还只能修改属于自己的密码
        // 日志:
        //      要产生日志
        public LibraryServerResult ChangeReaderPassword(string strReaderBarcode,
            string strReaderOldPassword,
            string strReaderNewPassword)
        {
            LibraryServerResult result = this.PrepareEnvironment("ChangeReaderPassword",
                true,
                strReaderOldPassword == null ? true : false,
                true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.ChangeReaderPassword(
                    sessioninfo,
                    strReaderBarcode,
                    strReaderOldPassword,
                    strReaderNewPassword);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ChangeReaderPassword() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }

        }

        void SetLang(string strLang)
        {
            if (String.IsNullOrEmpty(strLang) == true)
                return;

            Thread.CurrentThread.CurrentCulture =
    CultureInfo.CreateSpecificCulture(strLang);
            Thread.CurrentThread.CurrentUICulture = new
                CultureInfo(strLang);

            if (sessioninfo != null)
                sessioninfo.Lang = strLang;
        }

        // 获得读者信息
        // parameters:
        //      strBarcode  读者证条码号。如果前方引导以"@path:"，则表示读者记录路径。在@path引导下，路径后面还可以跟随 "$prev"或"$next"表示方向
        //      strResultTypeList   结果类型数组 xml/html/text/calendar/advancexml/recpaths/summary
        //              其中calendar表示获得读者所关联的日历名；advancexml表示经过运算了的提供了丰富附加信息的xml，例如具有超期和停借期附加信息
        //              advancexml_borrow_bibliosummary/advancexml_overdue_bibliosummary/advancexml_history_bibliosummary
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限: 
        //		工作人员或者读者，必须有getreaderinfo权限
        //		如果为读者, 附加限制还只能看属于自己的读者信息
        public LibraryServerResult GetReaderInfo(
            string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp)
        {
            results = null;
            baTimestamp = null;
            strRecPath = "";

            LibraryServerResult result = this.PrepareEnvironment("GetReaderInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetReaderInfo(
                    sessioninfo,
                    strBarcode,
                    strResultTypeList,
                    out results,
                    out strRecPath,
                    out baTimestamp);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetReaderInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 修改读者记录
        // 需要一并发来旧记录的原因, 是为了和数据库中当前可能已经变化了的记录进行比较，
        // 如果SetReaderInfo能覆盖的部分字段，这一部分没有发生实质性变化，整条记录仅仅是
        // 流通实时信息发生了变化，本函数就能仍适当合并后保存记录，而不会返回错误，增加
        // 了API的可用性。如果实际运用中不允许发回旧记录，可发来空字符串，就会牺牲上述
        // 可用性，变成，不论数据库中当前记录的改变具体在那些字段范围，都只能报错返回了。
        // paramters:
        //      strOperation    操作。new change delete
        //      strRecPath  希望保存到的记录路径。可以为空。
        //      strNewXml   希望保存的记录体
        //      strOldXml   原先获得的旧记录体。可以为空。
        //      baOldTimestamp  原先获得旧记录的时间戳。可以为空。
        //      strExistringXml 覆盖操作失败时，返回数据库中已经存在的记录，供前端参考
        //      strSavedXml 实际保存的新记录。内容可能和strNewXml有所差异。
        //      strSavedRecPath 实际保存的记录路径
        //      baNewTimestamp  实际保存后的新时间戳
        // return:
        //      result -1失败 0 正常 1部分字段被拒绝
        // 权限：
        //      读者不能修改任何人的读者记录，包括他自己的。
        //      工作人员则要看 setreaderinfo权限是否具备
        // 日志:
        //      要产生日志
        public LibraryServerResult SetReaderInfo(
            string strAction,
            string strRecPath,
            string strNewXml,
            string strOldXml,
            byte[] baOldTimestamp,
            out string strExistingXml,
            out string strSavedXml,
            out string strSavedRecPath,
            out byte[] baNewTimestamp,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode)
        {
            strExistingXml = "";
            strSavedXml = "";
            strSavedRecPath = "";
            baNewTimestamp = null;
            kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            LibraryServerResult result = this.PrepareEnvironment("SetReaderInfo", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.SetReaderInfo(sessioninfo,
                    strAction,
                    strRecPath,
                    strNewXml,
                    strOldXml,
                    baOldTimestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedRecPath,
                    out baNewTimestamp,
                    out kernel_errorcode);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetReaderInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 2012/1/11
        // 移动读者记录
        // return:
        // result.Value:
        //      -1  error
        //      0   已经成功移动
        // 权限：
        //      需要movereaderinfo权限
        // 日志:
        //      要产生日志
        public LibraryServerResult MoveReaderInfo(
            string strSourceRecPath,
            ref string strTargetRecPath,
            out byte[] target_timestamp)
        {
            target_timestamp = null;

            LibraryServerResult result = this.PrepareEnvironment("MoveReaderInfo", true, true, true);
            if (result.Value == -1)
                return result;

            // 对读者身份的判断
            if (sessioninfo.UserType == "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "移动读者记录被拒绝。作为读者不能进行此项操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            try
            {
                // 移动读者信息
                // result.Value:
                //      -1  error
                //      0   已经成功移动
                return app.MoveReaderInfo(
                        sessioninfo,
                        strSourceRecPath,
                        ref strTargetRecPath,
                        out target_timestamp);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library MoveReaderInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 转移借阅信息
        // return:
        // result.Value:
        //      -1  error
        //      0   没有必要转移。即源读者记录中没有需要转移的借阅信息
        //      1   已经成功转移
        // 权限：
        //      需要devolvereaderinfo权限
        // 日志:
        //      要产生日志
        public LibraryServerResult DevolveReaderInfo(
            string strSourceReaderBarcode,
            string strTargetReaderBarcode)
        {
            LibraryServerResult result = this.PrepareEnvironment("DevolveReaderInfo", true, true, true);
            if (result.Value == -1)
                return result;

            // 对读者身份的判断
            if (sessioninfo.UserType == "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "转移借阅信息被拒绝。作为读者不能进行此项操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            try
            {
                // 转移借阅信息
                // result.Value:
                //      -1  error
                //      0   没有必要转移。即源读者记录中没有需要转移的借阅信息
                //      1   已经成功转移
                return app.DevolveReaderInfo(
                        sessioninfo,
                        strSourceReaderBarcode,
                        strTargetReaderBarcode);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library DevolveReaderInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }
#if NOO
        // 下级函数
        // 合并新旧读者xml记录
        // 进一步增强：
        // 这里可以加入保护功能，限定根下只有那些元素读者才有覆盖的权力
        // 另外，barcode等要害字段，即便工作人员也不能简单覆盖，要考虑查重问题
        /*
         * 
         * 作为读者身份只能修改这些字段:
    name
    gender
    birthDate(birthday)
    idCardNumber
    department
    post
    address
    tel
    email* 
         */
        // parameters:
        //      bPartDenied 是否有某些元素的修改被拒绝。如果 == true，strError中将返回被拒绝的元素名列表
        static int MergOldNewReaderInfo(
            SessionInfo sessioninfo,
            string strOldXml,
            string strNewXml,
            out string strResultXml,
            out bool bPartDenied,
            out string strError)
        {
            strResultXml = "";
            strError = "";

            bPartDenied = false;    // 是否有局部字段的修改被拒绝?
            string strDeniedElementNames = "";

            XmlDocument olddom = new XmlDocument();
            try
            {
                olddom.LoadXml(strOldXml);
            }
            catch (Exception ex)
            {
                strError = "装载旧记录XML到DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlDocument newdom = new XmlDocument();
            try
            {
                newdom.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "装载新记录XML到DOM时发生错误: " + ex.Message;
                return -1;
            }

            Hashtable names = new Hashtable();

            // 遍历新记录所有根级对象，替换到旧记录中
            for (int i = 0; i < newdom.DocumentElement.ChildNodes.Count; i++)
            {
                XmlNode node = newdom.DocumentElement.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (names[node.Name] != null
                    && (int)names[node.Name] == 1)
                {
                    strError = "新记录中根下出现重复的元素名 '" + node.Name + "'，这是不允许的...";
                    return -1;
                }



                XmlNodeList matchnodes = olddom.DocumentElement.SelectNodes(node.Name);
                if (matchnodes.Count == 0)
                {
                    // 对读者的附加限制
                    if (sessioninfo.Account.Type == "reader")
                    {
                        // 只有少量元素让修改
                        if (node.Name != "name"
                            && node.Name != "gender"
                            && node.Name != "birthday"
                            && node.Name != "idCardNumber"
                            && node.Name != "department"
                            && node.Name != "post"
                            && node.Name != "address"
                            && node.Name != "tel"
                            && node.Name != "email")
                        {
                            if (strDeniedElementNames != "")
                                strDeniedElementNames = ",";
                            strDeniedElementNames += node.Name;

                            bPartDenied = true;
                            continue;
                        }
                    }

                    // 插入到旧记录末尾
                    XmlDocumentFragment fragment = olddom.CreateDocumentFragment();
                    fragment.InnerXml = node.OuterXml;

                    olddom.DocumentElement.AppendChild(fragment);
                }
                else
                {
                    XmlNode pos = matchnodes[0];

                    // 如果新旧内容完全相同，则不必修改了
                    if (pos.OuterXml == node.OuterXml)
                        continue;

                    // 对读者的附加限制
                    if (sessioninfo.Account.Type == "reader")
                    {
                        // 只有少量元素让修改
                        if (node.Name != "name"
                            && node.Name != "gender"
                            && node.Name != "birthday"
                            && node.Name != "idCardNumber"
                            && node.Name != "department"
                            && node.Name != "post"
                            && node.Name != "address"
                            && node.Name != "tel"
                            && node.Name != "email")
                        {
                            if (strDeniedElementNames != "")
                                strDeniedElementNames = ",";
                            strDeniedElementNames += node.Name;

                            bPartDenied = true;
                            continue;
                        }
                    }

                    // 替换旧记录已经存在的元素
                    XmlDocumentFragment fragment = olddom.CreateDocumentFragment();
                    fragment.InnerXml = node.OuterXml;

                    pos.ParentNode.InsertBefore(fragment, pos);
                    pos.ParentNode.RemoveChild(pos);
                }

                names[node.Name] = 1;
            }

            strResultXml = olddom.OuterXml;

            if (strDeniedElementNames != "")
                strError = strDeniedElementNames;

            return 0;
        }
#endif

        // 检索读者信息
        // parameters:
        //      strReaderDbNames    读者库名。可以为单个库名，也可以是逗号(半角)分割的读者库名列表。还可以为 <全部>/<all> 之一，表示全部读者库。
        //      strQueryWord    检索词
        //      nPerMax 一次命中结果的最大数。如果为-1，表示不限制。
        //      strFrom 检索途径
        //      strMathStyle    匹配方式 exact left right middle
        //      strLang 语言代码。一般为"zh"
        //      strResultSetName    结果集名。
        // rights：
        //      读者不能检索任何人的读者记录，包括他自己的；
        //      工作人员需要 searchreader 权限
        // return:
        //      result.Value    命中结果总数。如果为-1，则表示检索出错
        public LibraryServerResult SearchReader(
            string strReaderDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strOutputStyle)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchReader", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchreader", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索读者信息被拒绝。不具备searchreader权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索读者信息被拒绝。作为读者不能检索任何读者信息";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                List<string> dbnames = new List<string>();

                if (String.IsNullOrEmpty(strReaderDbNames) == true
                    || strReaderDbNames == "<全部>"
                    || strReaderDbNames.ToLower() == "<all>")
                {
#if NO
                    for (int i = 0; i < app.ReaderDbs.Count; i++)
                    {
                        string strDbName = app.ReaderDbs[i].DbName;
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        if (string.IsNullOrEmpty(sessioninfo.LibraryCode) == false)
                        {
                            string strLibraryCode = app.ReaderDbs[i].LibraryCode;
                            // 匹配图书馆代码
                            // parameters:
                            //      strSingle   单个图书馆代码。空的总是不能匹配
                            //      strList     图书馆代码列表，例如"第一个,第二个"，或者"*"。空表示都匹配
                            // return:
                            //      false   没有匹配上
                            //      true    匹配上
                            if (LibraryApplication.MatchLibraryCode(strLibraryCode, sessioninfo.LibraryCode) == false)
                                continue;
                        }

                        dbnames.Add(strDbName);
                    }
#endif
                    dbnames = app.GetCurrentReaderDbNameList(sessioninfo.LibraryCodeList);

                }
                else
                {
                    List<string> notmatches = new List<string>();
                    string[] splitted = strReaderDbNames.Split(new char[] { ',' });
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        string strDbName = splitted[i];
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        string strLibraryCode = "";
                        if (app.IsReaderDbName(strDbName, out strLibraryCode) == false)
                        {
                            strError = "库名 '" + strDbName + "' 不是合法的读者库名";
                            goto ERROR1;
                        }

                        if (string.IsNullOrEmpty(sessioninfo.LibraryCodeList) == false)
                        {
                            // 匹配图书馆代码
                            // parameters:
                            //      strSingle   单个图书馆代码。空的总是匹配
                            //      strList     图书馆代码列表，例如"第一个,第二个"，或者"*"。空表示都匹配
                            // return:
                            //      false   没有匹配上
                            //      true    匹配上
                            if (LibraryApplication.MatchLibraryCode(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                            {
                                notmatches.Add(strDbName);
                                continue;
                            }
                        }

                        dbnames.Add(strDbName);
                    }

                    if (notmatches.Count > 0)
                    {
                        strError = "读者库 " + StringUtil.MakePathList(notmatches) + " 因为馆代码限制，不允许当前用户检索";
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                }

                if (dbnames.Count == 0)
                {
                    strError = "读者库名 '" + strReaderDbNames + "' 没有匹配上任何读者库";
                    goto ERROR1;
                }

                // 构造检索式
                string strQueryXml = "";
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    string strRelation = "=";
                    string strDataType = "string";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOf("-") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";

                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                        else if (String.IsNullOrEmpty(strQueryWord) == false)
                        {
                            strDataType = "number";

                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                    }
                    else if (strFrom == "失效日期")
                    {
                        // 如果为范围式
                        if (strQueryWord.IndexOf("~") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else
                        {
                            strDataType = "number";

                            // 2012/3/29
                            // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                            // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                            if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                            {
                                strMatchStyle = "exact";
                                strRelation = "range";
                                strQueryWord = "~";
                            }
                        }
                        // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                        strMatchStyle = "exact";
                    }

                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)   // 2007/9/14 
                        + "'><option warning='0'/><item><word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                    if (i > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                }

                if (dbnames.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,   // "default",
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchReader() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // parameters:
        //      actions noResult 表示不返回 results，只返回 result.Value(totalCount)
        public LibraryServerResult SearchCharging(
            string patronBarcode,
            string timeRange,
            string actions,
            string order,
            long start,
            long count,
            out ChargingItemWrapper[] results)
        {
            results = null;

            LibraryServerResult result = this.PrepareEnvironment("SearchCharging", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                if (StringUtil.IsInList("searchcharging", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得出纳历史信息被拒绝。不具备 searchcharging 权限";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    if (sessioninfo.Account != null
                        && patronBarcode != sessioninfo.Account.Barcode)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "获得出纳历史信息被拒绝。作为读者只能获得自己的出纳历史信息";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                string strStart = "";
                string strEnd = "";
                StringUtil.ParseTwoPart(timeRange, "~", out strStart, out strEnd);
                DateTime startTime = string.IsNullOrEmpty(strStart) ? new DateTime(0) : DateTime.Parse(strStart);
                DateTime endTime = string.IsNullOrEmpty(strEnd) ? new DateTime(0) : DateTime.Parse(strEnd);

                string strError = "";
                IEnumerable<ChargingOperItem> collection = app.ChargingOperDatabase.Find(
                    patronBarcode,
                    startTime,
                    endTime,
                    actions,
                    order,
                    (int)start,
                    out long totalCount);
                if (collection == null)
                {
                    strError = "ChargingOperDatabase 尚未启用";
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }
                if (count == 0)
                {
                    result.Value = totalCount;
                    return result;
                }

                // 2021/6/8
                bool no_result = StringUtil.IsInList("noResult", actions);

                if (no_result == false)
                {
                    int MAXITEMS = 100;    // 每次最多返回的事项数
                    List<ChargingItemWrapper> infos = new List<ChargingItemWrapper>();
                    long i = 0;
                    foreach (ChargingOperItem item in collection)
                    {
                        if (i >= MAXITEMS)
                            break;
                        if (count != -1 && i >= count)
                            break;
                        ChargingItemWrapper wrapper = new ChargingItemWrapper();
                        wrapper.Item = new ChargingItem(item);
                        if (item.Operation == "return"
                            && item.Action != "read")
                        {
                            ChargingOperItem rel = app.ChargingOperDatabase.FindRelativeBorrowItem(item);
                            if (rel != null)
                                wrapper.RelatedItem = new ChargingItem(rel);
                        }
                        infos.Add(wrapper);
                        i++;
                    }
                    results = infos.ToArray();
                }

                result.Value = totalCount;
                return result;
#if NO
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
#endif
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchCharging() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置好友关系
        // parameters:
        //      strAction   'request' 请求添加好友。当前用户为发起者, strReaderBarcode 中为被请求者
        //                  'agree' 同意添加好友。strReaderBarcode中要提供 message 的 ID。功能需要从 message 中提取两个证条码号，然后对读者记录进行操作，最后把 message 设为“已处理”状态(以后就不会再出现“同意”按钮了)。要求当前用户是 message 中的 recipient
        //                  'remove' 删除好友关系。当前用户为发起者, strReaderBarcode 中为被请求者
        //      strReaderBarcode    读者证条码号。
        // Result.Value -1出错 0请求成功(注意，并不代表对方同意) 1:请求前已经是好友关系了，没有必要重复请求 2:已经成功添加
        // 权限: 
        //		暂时不要求权限
        public LibraryServerResult SetFriends(
            string strAction,
            string strReaderBarcode,
            string strComment,
            string strStyle)
        {
            LibraryServerResult result = this.PrepareEnvironment("SetFriends", true, true, true);
            if (result.Value == -1)
                return result;

            if (sessioninfo.UserType != "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "当前用户不是读者，不能进行好友操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // 权限判断

            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strXml = "";
                string strError = "";
                string strOutputPath = "";

                // 获得读者记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = app.GetReaderRecXml(
                    channel,    // sessioninfo.Channels,
                    strReaderBarcode,
                    out strXml,
                    out strOutputPath,
                    out strError);
                if (nRet == 0)
                {
                    strError = "证条码号为 '" + strReaderBarcode + "' 的读者不存在";
                    goto ERROR1;
                }
                if (nRet == -1)
                {
                    strError = "获得证条码号为 '" + strReaderBarcode + "' 的读者记录时出错: " + strError;
                    goto ERROR1;
                }

                if (nRet > 1)
                {
                    strError = "系统错误: 证条码号为 '" + strReaderBarcode + "' 的读者记录多于一个";
                    goto ERROR1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                string strBack = DomUtil.GetElementText(readerdom.DocumentElement, "friends");
                string strTo = DomUtil.GetElementText(sessioninfo.Account?.PatronDom?.DocumentElement, "friends");

                if (StringUtil.IsInList(strReaderBarcode, strTo) == true
                    && StringUtil.IsInList(sessioninfo.UserID, strBack) == true)
                {
                    result.ErrorInfo = "读者 " + sessioninfo.UserID + " 和 " + strReaderBarcode + " 在请求前已经具有好友关系了";
                    result.Value = 1;
                    return result;
                }

                if (StringUtil.IsInList(sessioninfo.UserID, strBack) == true)
                {
                    // 对方已经认为我是好友，只要我添加即可
                    // 为两个读者记录互相添加好友关系
                    nRet = app.AddFriends(
                        sessioninfo,
                        sessioninfo.UserID,
                        strReaderBarcode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    result.ErrorInfo = "读者 " + sessioninfo.UserID + " 和 " + strReaderBarcode + " 的好友关系已经成功添加";
                    result.Value = 2;
                    return result;
                }

                string strBody = "证条码号为 " + sessioninfo.UserID + " 的读者希望和您成为好友。鉴别文字为 '" + strComment + "'。";

                // 发送消息
                // return:
                //      -1  出错
                //      0   成功
                nRet = app.MessageCenter.SendMessage(
                    sessioninfo.Channels,
                    strReaderBarcode,
                    sessioninfo.UserID, // "图书馆",
                    "{fr} " + sessioninfo.UserID + " 请求和您成为好友",
                    "text",    // "text",
                    strBody,
                    false,
                    out strError);
                if (nRet == -1)
                {
                    strError = "发送dpmail出错: " + strError;
                    goto ERROR1;
                }
                else
                {
                    if (app.Statis != null)
                        app.Statis.IncreaseEntryValue(
                        sessioninfo.LibraryCodeList,
                        "读者",
                        "请求好友次",
                        1);
                }

                result.Value = 0;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetFriends() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 如果通讯中断，则也切断和dp2Kernel的通讯。
        void channel_IdleEvent(object sender, IdleEventArgs e)
        {
            if (this._nStop == 1)
            {
                RmsChannel channel = (RmsChannel)sender;
                channel.Abort();
                this._nStop = 2;
                WriteDebugInfo("channel call abort");
            }
            else if (this._nStop == 2)
            {
                // 已经实施了中断，但是还没有来得及生效
                Thread.Sleep(10);
            }

            // e.bDoEvents = false;
        }

        // 检索任意数据库
        public LibraryServerResult SearchOneDb(
            string strQueryWord,
            string strDbName,
            string strFrom,
            string strMatchStyle,
            string strLang,
            long lMaxCount,
            string strResultSetName,
            string strOutputStyle)
        {
            LibraryServerResult result = this.PrepareEnvironment("SearchOneDb", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchonedb", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索任意数据库信息被拒绝。不具备searchonedb权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索 任意数据库 被拒绝。作为读者不能检索任意数据库";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 需要限制检索读者库为当前管辖的范围
                {
                    string strLibraryCode = "";
                    bool bReaderDbInCirculation = true;
                    if (app.IsReaderDbName(strDbName,
                        out bReaderDbInCirculation,
                        out strLibraryCode) == true)
                    {
                        // 检查当前操作者是否管辖这个读者库
                        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                        if (app.IsCurrentChangeableReaderPath(strDbName + "/?",
                sessioninfo.LibraryCodeList) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "读者库 '" + strDbName + "' 不在当前用户管辖范围内";
                            result.ErrorCode = ErrorCode.SystemError;
                            return result;
                        }
                    }
                }

                // 构造检索式
                string strQueryXml = "";

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)   // 2007/9/14 
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strQueryWord)
                    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>" + lMaxCount.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";


                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strError = "";

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,   // "default",
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";

                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchOneDb() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // TODO: 对于读者，限定他们检索<virtualDatabases>里面定义的数据库(以及书目库下属的实体库、评注库)就可以了
        // 检索任意数据库
        public LibraryServerResult Search(
            string strQueryXml,
            string strResultSetName,
            string strOutputStyle)
        {
            LibraryServerResult result = this.PrepareEnvironment("Search", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // test
                // Thread.Sleep(new TimeSpan(0, 0, 30));

                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("search", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索任意数据库信息被拒绝。不具备search权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                string strError = "";
                int nRet = 0;

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    // 检查检索式内有没有超越规定读者检索的数据库
                    // return:
                    //      -1  error
                    //      0   没有超越要求
                    //      1   超越了要求
                    nRet = app.CheckReaderOnlyXmlQuery(strQueryXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "检索被拒绝。" + strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else
                {
                    // 2012/9/15
                    // 检查检索式内有没有超越当前用户管辖的读者库范围的读者库
                    // return:
                    //      -1  error
                    //      0   没有超越要求
                    //      1   超越了要求
                    nRet = app.CheckReaderDbXmlQuery(strQueryXml,
                        sessioninfo.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "检索被拒绝。" + strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strTargetQueryXml = "";

                /*
                // TODO: 把这个初始化放在正规的初始化中？
                nRet = app.InitialVdbs(sessioninfo.Channels,
                    out strError);
                if (nRet == -1)
                {
                    strError = "InitialVdbs error : " + strError;
                    goto ERROR1;
                }
                 * */
                if (app.vdbs == null)
                {
                    app.ActivateManagerThreadForLoad();
                    strError = "app.vdbs == null。故障原因请检查dp2Library日志";
                    goto ERROR1;
                }

                // 将包含虚拟库要求的XML检索式变换为内核能够理解的实在库XML检索式
                // return:
                //      -1  error
                //      0   没有发生变化
                //      1   发生了变化
                nRet = app.KernelizeXmlQuery(strQueryXml,
                    out strTargetQueryXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strTargetQueryXml,
                        strResultSetName,   // "default",
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                        goto ERROR1;


                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";

                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Search() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得检索命中的结果集信息
        // (注: 本方法基本上是内核对应功能GetRecords()的浅包装)
        // parameters:
        //      strResultSetName    结果集名。如果为空，表示使用当前缺省结果集"default"
        //      lStart  要获取的开始位置。从0开始计数
        //      lCount  要获取的个数
        //      strBrowseInfoStyle  所返回的SearchResult中包含哪些信息。为逗号分隔的字符串列表值，取值可为 id/cols 之一。例如，"id,cols"表示同时获取id和浏览信息各列，而"id"表示仅取得id列。
        //      strLang 语言代码。一般为"zh"
        //      searchresults   返回包含记录信息的SearchResult对象数组
        // rights:
        //      没有限制
        // result.Value    -1 出错；>=0 结果集内记录的总数(注意，并不是本批返回的记录数)
        public LibraryServerResult GetSearchResult(
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out Record[] searchresults)
        {
            searchresults = null;

            LibraryServerResult result = this.PrepareEnvironment("GetSearchResult", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strError = "";

                if (String.IsNullOrEmpty(strResultSetName) == true)
                    strResultSetName = "default";

                long lRet = channel.DoGetSearchResult(
                    strResultSetName,
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    strLang,
                    null,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    // 2018/7/1
                    result.ErrorCode = LibraryServerResult.FromErrorValue(channel.OriginErrorCode); // localhost.ErrorCodeValue --> LibraryServer.ErrorCode
                    // result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                // 2021/7/15
                // 过滤结果
                // 对 XML 记录进行过滤或者修改
                if (searchresults != null
                    && StringUtil.IsInList("keycount", strBrowseInfoStyle) == false)
                {
                    // 2012/9/15
                    // 当前用户是否能管辖一个读者库。key 为数据库名，value 为 true 或 false
                    Hashtable table = new Hashtable();  // 加快运算速度

                    // return:
                    //      null    没有找到 getreaderinfo 前缀
                    //      ""      找到了前缀，并且 level 部分为空
                    //      其他     返回 level 部分
                    string read_level = LibraryApplication.GetReaderInfoLevel("getreaderinfo", sessioninfo.RightsOrigin);
                    bool bHasGetReaderInfoRight = read_level != null;
                    // bool bHasGetReaderInfoRight = StringUtil.IsInList("getreaderinfo", sessioninfo.RightsOrigin);

                    bool bHasGetBiblioInfoRight = StringUtil.IsInList("getbiblioinfo", sessioninfo.RightsOrigin);

                    foreach (Record record in searchresults)
                    {
                        string strDbName = ResPath.GetDbName(record.Path);

                        bool bIsReader = sessioninfo.UserType == "reader";

                        if (app.IsReaderDbName(strDbName))
                        {
                            // 2021/7/20
                            // 先检测当前用户是否能管辖读者库 strDbName
                            bool bChangeable = true;

                            if (bIsReader)
                            {
                                // 不是读者自己的读者记录或者下级记录(比如对象记录)
                                if (sessioninfo.Account == null
                                    || StringUtil.IsEqualOrSubPath(sessioninfo.Account.ReaderDomPath, record.Path) == false)
                                    bChangeable = false;
                            }
                            else
                            {
                                object o = table[strDbName];
                                if (o == null)
                                {
                                    if (app.IsReaderDbName(strDbName,
                                        out bool bReaderDbInCirculation,
                                        out string strLibraryCode) == true)
                                    {
                                        // 检查当前操作者是否管辖这个读者库
                                        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                                        bChangeable = app.IsCurrentChangeableReaderPath(strDbName + "/?",
                                sessioninfo.LibraryCodeList);
                                    }
                                    table[strDbName] = bChangeable; // 记忆
                                }
                                else
                                    bChangeable = (bool)o;
                            }

                            if (bChangeable == false)
                            {
                                // 当前用户不管辖此读者库
                                ClearRecord(record, $"当前用户不管辖读者库 '{strDbName}'");
                                record.Path = "";
                                /*
                                record.Cols = null;
                                record.RecordBody = null;
                                */
                            }
                            else
                            {
                                // 过滤
                                FilterPatronRecord(record,
        strDbName,
        read_level,
        strBrowseInfoStyle);
                            }
                        }
                        else if (app.IsBiblioDbName(strDbName))
                        {
                            /*
                            if (bHasGetBiblioInfoRight == false)
                            {
                                ClearCols(record, "[滤除]");
                            }
                            */
                            FilterBiblioRecord(record,
                                strDbName,
                                bHasGetBiblioInfoRight,
                                sessioninfo.Access,
                                strBrowseInfoStyle);
                        }
                        else if (app.IsItemDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getiteminfo,getentities,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                            else
                                AddItemOI(app, record);
                        }
                        else if (app.IsIssueDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getissues,getissueinfo,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.IsOrderDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getorders,getorderinfo,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.IsCommentDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getcommentinfo,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.ArrivedDbName == strDbName)
                        {
                            if (StringUtil.IsInList("borrow,return", sessioninfo.RightsOrigin) == true
                                || read_level == "")
                            {

                            }
                            else
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.AmerceDbName == strDbName)
                        {
                            if (StringUtil.IsInList("getrecord", sessioninfo.RightsOrigin) == false
                                || sessioninfo.UserType == "reader")
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                            else
                                FilterAmerceRecord(record);
                            // 注: GetRecord() API 可以获取违约金记录
                            // settlement undosettlement deletesettlement
                        }
                        else if (app.MessageDbName == strDbName)
                        {
                            if (StringUtil.IsInList("managedatabase", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.PinyinDbName == strDbName
                            || app.GcatDbName == strDbName
                            || app.WordDbName == strDbName)
                        {
                            if (StringUtil.IsInList("managedatabase", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else
                        {
                            // 实用库包括 publisher / zhongcihao / dictionary / inventory 类型
                            string util_type = ServerDatabaseUtility.GetUtilDbType(app.LibraryCfgDom,
    strDbName);
                            if (util_type != null)
                            {
                                if (util_type == "inventory")
                                {
                                    // TODO: 盘点操作的用户具有什么权限?
                                    if (StringUtil.IsInList("inventory", sessioninfo.RightsOrigin) == false)
                                    {
                                        ClearXml(record);
                                        ClearCols(record, LibraryApplication.FILTERED);
                                    }
                                }
                                else if (util_type == "publisher")
                                {
                                    // 公开
                                }
                            }
                            else
                            {
                                // TODO: 根据具体情况再扩展 if
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                    }
                }

                result.Value = lRet;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetSearchResult() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        /*
         * record.Cols 和 record.RecordBody.Xml 需要进行过滤处理
         * 1) (style 中包含 xml)如果 .Xml 被 access 处理(减少字段或者禁止)
         *  1.1 如果 style 中有 cols，则重新创建 cols
         *  1.2 如果 style 中没有 cols，则设置 cols 为 null
         * 2) 如果 style 中没有 xml，也就是说 .Xml == null，
         * 并且发现 XML 需要按照 access 处理(减少字段或者禁止)，那么 Cols 设置 [滤除]
         * */
        void FilterBiblioRecord(Record record,
            string strBiblioDbName,
            bool has_getbiblioinfo_right,
            string access,
            string strBrowseInfoStyle)
        {
            bool bHasCols = StringUtil.IsInList("cols", strBrowseInfoStyle);
            bool bHasXml = StringUtil.IsInList("xml", strBrowseInfoStyle);

            string origin_xml = record.RecordBody?.Xml;

            /*
            if (has_getbiblioinfo_right == false)
            {
                if (string.IsNullOrEmpty(record.RecordBody.Xml) == false)
                {
                    record.RecordBody.Xml = "";
                }
            }
            */

            if (string.IsNullOrEmpty(access))
                return; // 保持 XML 和 Cols 不变

            /*
            if (record.RecordBody == null
                || string.IsNullOrEmpty(record.RecordBody.Xml))
                return;
            */

            if (string.IsNullOrEmpty(origin_xml))
                return;

            bool bHasRight = true;
            string strAccessParameters = "";

            string strAction = "*";

            // 按照 Access 定义来过滤 XML 记录
            // return:
            //      null    指定的操作类型的权限没有定义
            //      ""      定义了指定类型的操作权限，但是否定的定义
            //      其它      权限列表。* 表示通配的权限列表
            string strActionList = LibraryApplication.GetDbOperRights(access,
                strBiblioDbName,
                "getbiblioinfo");
            if (strActionList == null)
            {
                if (LibraryApplication.GetDbOperRights(access,
                    "",
                    "getbiblioinfo") != null)
                {
                    bHasRight = false;
                }
                else
                {
                    // 没有定义任何 getbiblioinfo 的存取定义(虽然定义了其他操作的存取定义)
                    // 这时应该转过去看普通的权限
                    // TODO: 这种算法，速度较慢
                    if (has_getbiblioinfo_right == false)
                    {
                        bHasRight = false;
                    }
                }
            }
            else if (strActionList == "*")
            {
                // 通配
            }
            else
            {
                if (LibraryApplication.IsInAccessList(strAction, strActionList, out strAccessParameters) == false)
                {
                    bHasRight = false;
                }
            }

            if (bHasRight == false)
            {
                ClearXml(record);
                if (record.Cols != null && record.Cols.Length > 0)
                    ClearCols(record, LibraryApplication.FILTERED);
                return;
            }

            if (record.RecordBody == null
    || string.IsNullOrEmpty(record.RecordBody.Xml))
                return;

            // 过滤书目记录的字段
            if (string.IsNullOrEmpty(origin_xml) == false
                && string.IsNullOrEmpty(strAccessParameters) == false)
            {
                string xml = origin_xml;
                // 根据字段权限定义过滤出允许的内容
                // parameters:
                //      strUserRights   用户权限。如果为 null，表示不启用过滤 856 字段功能
                // return:
                //      -1  出错
                //      0   成功
                //      1   有部分字段被修改或滤除
                int nRet = LibraryApplication.FilterBiblioByFieldNameList(
                    StringUtil.IsInList("objectRights", app.Function) == true ? sessioninfo.Rights : null,
                    strAccessParameters,
                    ref xml,
                    out string strError);
                if (nRet == -1)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<root />");
                    dom.DocumentElement.SetAttribute("error", strError);
                    record.RecordBody.Xml = dom.DocumentElement.OuterXml;
                    // TODO: 此时 Cols 需要清除么?
                }
                else
                    ClearXml(record);

                if (nRet == 1)  // XML 记录发生过改变，也要重新创建 Cols
                {
                    if (bHasCols)
                    {
                        // 有可能返回 null
                        string strFormat = StringUtil.GetStyleParam(strBrowseInfoStyle, "format");

                        RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                        // return:
                        //      // cols中包含的字符总数
                        //      -1  出错
                        //      0   记录没有找到
                        //      其他  cols 中包含的字符总数。最少为 1，不会为 0
                        nRet = app.GetCols(
                            channel,
                            strFormat,
                            strBiblioDbName,
                            xml,
                            0,  // nStartCol,
                            out string[] cols,
                            out strError);
                        // TODO: 检查 nRet == -1 时候 cols 是否为报错信息
                        record.Cols = cols;
                    }
                    else
                        record.Cols = null;
                }
            }
        }

        /*
         * record.Cols 和 record.RecordBody.Xml 需要进行过滤处理
         * 1) (style 中包含 xml)如果 .Xml 被 :level 处理(减少字段或者禁止)
         *  1.1 如果 style 中有 cols，则重新创建 cols
         *  1.2 如果 style 中没有 cols，则设置 cols 为 null
         * 2) 如果 style 中没有 xml，也就是说 .Xml == null，
         * 并且发现 XML 需要按照 :level 处理(减少字段或者禁止)，那么 Cols 设置 [滤除]
         * */
        // 过滤读者记录 XML
        // parameters:
        //      read_level
        //                  null    没有找到 getreaderinfo 前缀
        //                  ""      找到了前缀，并且 level 部分为空
        //                  其他     返回 level 部分
        void FilterPatronRecord(Record record,
            string strPatronDbName,
            string read_level,
            string strBrowseInfoStyle)
        {
            bool bHasCols = StringUtil.IsInList("cols", strBrowseInfoStyle);
            bool bHasXml = StringUtil.IsInList("xml", strBrowseInfoStyle);

            // style 中包含 'xml'
            if (bHasXml == true)
            {
                // *** 没有 getreaderinfo 权限
                if (read_level == null)
                {
                    ClearXml(record);
                    ClearCols(record, LibraryApplication.FILTERED);
                    return;
                }

                // *** 完整的 getreaderinfo 权限
                if (read_level == "")
                {
                    RemovePassword(record);
                    return; // 保持 XML 和 Cols 不变
                }

                // *** 不完整的 getreaderinfo 权限

                string origin_xml = record.RecordBody?.Xml;
                if (string.IsNullOrEmpty(origin_xml))
                {
                    // 没有传递 XML，那么只能把 Cols 滤除
                    ClearCols(record, LibraryApplication.FILTERED);
                    return;
                }

                // 下面是有 XML 的情况，把 XML 过滤，然后重新生成 Cols
                XmlDocument domNewRec = new XmlDocument();
                domNewRec.LoadXml(origin_xml);

                // 给读者记录加上 oi 元素
                {
                    if (app.IsReaderDbName(strPatronDbName,
                        out bool _,
                        out string library_code) == false)
                    {
                    }
                    else
                    {
                        app.AddPatronOI(domNewRec, library_code);
                    }
                }

                var changed = LibraryApplication.FilterByLevel(domNewRec, read_level);
                record.RecordBody.Xml = domNewRec.DocumentElement == null ? domNewRec.OuterXml : domNewRec.DocumentElement.OuterXml;

                if (changed)
                {
                    if (bHasCols)
                    {
                        // 有可能返回 null
                        string strFormat = StringUtil.GetStyleParam(strBrowseInfoStyle, "format");

                        RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                        // return:
                        //      // cols中包含的字符总数
                        //      -1  出错
                        //      0   记录没有找到
                        //      其他  cols 中包含的字符总数。最少为 1，不会为 0
                        int nRet = app.GetCols(
                            channel,
                            strFormat,
                            strPatronDbName,
                            record.RecordBody.Xml,
                            0,  // nStartCol,
                            out string[] cols,
                            out string strError);
                        // TODO: 检查 nRet == -1 时候 cols 是否为报错信息
                        record.Cols = cols;
                    }
                    else
                        record.Cols = null;
                }
            }
            else
            {
                // bHashXml == false

                // *** 没有 getreaderinfo 权限
                if (read_level == null)
                {
                    ClearXml(record);
                    ClearCols(record, LibraryApplication.FILTERED);
                    return;
                }

                // *** 完整的 getreaderinfo 权限
                if (read_level == "")
                {
                    RemovePassword(record);
                    return; // 保持 XML 和 Cols 不变
                }

                // *** 不完整的 getreaderinfo 权限
                ClearXml(record);
                ClearCols(record, LibraryApplication.FILTERED);
            }

            // 去除 password 元素
            void RemovePassword(Record r)
            {
                if (r.RecordBody == null || string.IsNullOrEmpty(r.RecordBody.Xml))
                    return;
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(r.RecordBody.Xml);
                }
                catch
                {
                    dom.LoadXml("<root />");
                    dom.DocumentElement.SetAttribute("error", "xml parse error");
                    record.RecordBody.Xml = dom.DocumentElement.OuterXml;
                    return;
                }

                DomUtil.DeleteElement(dom.DocumentElement, "password");
                r.RecordBody.Xml = dom.DocumentElement == null ? dom.OuterXml : dom.DocumentElement.OuterXml;
            }
        }

        void FilterAmerceRecord(Record record)
        {
            var origin_xml = record.RecordBody?.Xml;
            if (string.IsNullOrEmpty(origin_xml))
                return;

            // 当前用户只能获取和管辖的馆代码关联的违约金记录
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(origin_xml);
            }
            catch (Exception ex)
            {
                // strError = "违约金记录 '" + strPath + "' 装入XMLDOM时出错: " + ex.Message;
                ClearXml(record);
                return;
            }
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
            if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
            {
                /*
                result.Value = -1;
                result.ErrorInfo = "违约金记录 '" + strPath + "' 超出当前用户管辖范围，无法获取";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
                */
                ClearXml(record);
                ClearCols(record, LibraryApplication.FILTERED);
            }
        }

        // 获得数据库记录
        // parameters:
        //
        // 权限：读者不能获取任何数据库记录。
        //      工作人员则要看 getrecord 权限是否具备
        public LibraryServerResult GetRecord(
            string strPath,
            out byte[] timestamp,
            out string strXml)
        {
            timestamp = null;
            strXml = "";
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("GetRecord", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("getrecord", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取数据库记录被拒绝。不具备getrecord权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                string strDbName = "";

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取数据库记录。作为读者不能获取任何数据库记录";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                else
                {
                    // 需要限制检索读者库为当前管辖的范围
                    string strLibraryCode = "";
                    strDbName = ResPath.GetDbName(strPath);
                    bool bReaderDbInCirculation = true;
                    if (app.IsReaderDbName(strDbName,
                        out bReaderDbInCirculation,
                        out strLibraryCode) == true)
                    {
                        // 检查当前操作者是否管辖这个读者库
                        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                        if (app.IsCurrentChangeableReaderPath(strDbName + "/?",
                sessioninfo.LibraryCodeList) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "读者库 '" + strDbName + "' 不在当前用户管辖范围内";
                            result.ErrorCode = ErrorCode.SystemError;
                            return result;
                        }
                    }
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strMetaData = "";
                string strOutputPath = "";

                long lRet = channel.GetRes(strPath,
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                // 当前用户只能获取和管辖的馆代码关联的违约金记录
                if (strDbName == app.AmerceDbName
                    && sessioninfo.GlobalUser == false)
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "违约金记录 '" + strPath + "' 装入XMLDOM时出错: " + ex.Message;
                        goto ERROR1;
                    }
                    string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
                    if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "违约金记录 '" + strPath + "' 超出当前用户管辖范围，无法获取";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                result.Value = lRet;
                result.ErrorInfo = strError;

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetRecord() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }

        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 获得指定记录的浏览信息
        // (注: 本方法基本上是内核对应功能GetBrowse()的浅包装)
        // parameters:
        //      searchresults   返回包含记录信息的SearchResult对象数组
        // rights:
        //      没有限制
        // return:
        //      result.Value    -1 出错；0 成功
        public LibraryServerResult GetBrowseRecords(
            string[] paths,
            string strBrowseInfoStyle,
            out Record[] searchresults)
        {
            searchresults = null;

            LibraryServerResult result = this.PrepareEnvironment("GetBrowseRecords", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strError = "";

                long lRet = channel.GetBrowseRecords(paths,
                    strBrowseInfoStyle,
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

#if REMOVED
                if (searchresults != null)
                {
                    // 如果当前身份是读者，或者没有getreaderinfo权限，则要过滤掉属于读者库的记录
                    // TODO: 今后对书目库、各种功能库的访问也要加以限制
                    bool bIsReader = sessioninfo.UserType == "reader";
                    bool bHasGetReaderInfoRight = StringUtil.IsInList("getreaderinfo", sessioninfo.RightsOrigin);
                    if (bIsReader == true || bHasGetReaderInfoRight == false)
                    {
                        foreach (Record record in searchresults)
                        {
                            string strDbName = ResPath.GetDbName(record.Path);
                            
                            // 2021/5/10
                            if (app.IsItemDbName(strDbName))
                            {
                                AddItemOI(app, record);
                            }
                            
                            bool bIsReaderRecord = app.IsReaderDbName(strDbName);
                            if ((bIsReader == true || bHasGetReaderInfoRight == false)
                                && bIsReaderRecord == true)
                            {
                                if (sessioninfo.Account == null
                                    || StringUtil.IsEqualOrSubPath(sessioninfo.Account.ReaderDomPath, record.Path) == false)
                                {
                                    record.Path = "";
                                    record.Cols = null;
                                    record.RecordBody = null;
                                }
                            }
                        }
                    }
                    else if (sessioninfo.GlobalUser == false && bIsReader == false)
                    {
                        // 2012/9/15
                        Hashtable table = new Hashtable();  // 加快运算速度
                        // 读者库的管辖范围
                        foreach (Record record in searchresults)
                        {
                            string strDbName = ResPath.GetDbName(record.Path);

                            // 2021/5/10
                            if (app.IsItemDbName(strDbName))
                            {
                                AddItemOI(app, record);
                            }

                            bool bChangeable = true;

                            object o = table[strDbName];
                            if (o == null)
                            {
                                string strLibraryCode = "";
                                bool bReaderDbInCirculation = true;
                                if (app.IsReaderDbName(strDbName,
                                    out bReaderDbInCirculation,
                                    out strLibraryCode) == true)
                                {
                                    // 检查当前操作者是否管辖这个读者库
                                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                                    bChangeable = app.IsCurrentChangeableReaderPath(strDbName + "/?",
                            sessioninfo.LibraryCodeList);
                                }
                                table[strDbName] = bChangeable; // 记忆
                            }
                            else
                                bChangeable = (bool)o;

                            if (bChangeable == false)
                            {
                                record.Path = "";
                                record.Cols = null;
                                record.RecordBody = null;
                            }
                        }
                    }
                }

#endif

                // 对 XML 记录进行过滤或者修改
                if (searchresults != null)
                {
                    // 2012/9/15
                    // 当前用户是否能管辖一个读者库。key 为数据库名，value 为 true 或 false
                    Hashtable table = new Hashtable();  // 加快运算速度

                    // 如果当前身份是读者，或者没有getreaderinfo权限，则要过滤掉属于读者库的记录
                    // TODO: 今后对书目库、各种功能库的访问也要加以限制
                    bool bIsReader = sessioninfo.UserType == "reader";
                    // bool bHasGetReaderInfoRight = StringUtil.IsInList("getreaderinfo", sessioninfo.RightsOrigin);

                    // return:
                    //      null    没有找到 getreaderinfo 前缀
                    //      ""      找到了前缀，并且 level 部分为空
                    //      其他     返回 level 部分
                    string read_level = LibraryApplication.GetReaderInfoLevel("getreaderinfo", sessioninfo.RightsOrigin);
                    bool bHasGetReaderInfoRight = read_level != null;

                    bool bHasGetBiblioInfoRight = StringUtil.IsInList("getbiblioinfo", sessioninfo.RightsOrigin);

                    foreach (Record record in searchresults)
                    {
                        string strDbName = ResPath.GetDbName(record.Path);

                        if (app.IsItemDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getiteminfo,getentities,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                            else
                                AddItemOI(app, record);
                        }
                        else if (app.IsReaderDbName(strDbName))
                        {
                            /*
                            // 如果 level 不是空，则 mask 全部浏览列内容
                            if (read_level != "")
                                ClearCols(record, LibraryApplication.FILTERED);
                            */

                            // 过滤
                            FilterPatronRecord(record,
    strDbName,
    read_level,
    strBrowseInfoStyle);
                        }
                        else if (app.IsBiblioDbName(strDbName))
                        {
                            /*
                            if (bHasGetBiblioInfoRight == false)
                            {
                                ClearCols(record, "[滤除]");
                            }
                            */
                            FilterBiblioRecord(record,
                                strDbName,
                                bHasGetBiblioInfoRight,
                                sessioninfo.Access,
                                strBrowseInfoStyle);
                        }
                        else if (app.IsIssueDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getissues,getissueinfo,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.IsOrderDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getorders,getorderinfo,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else if (app.IsCommentDbName(strDbName))
                        {
                            if (StringUtil.IsInList("getcommentinfo,order", sessioninfo.RightsOrigin) == false)
                            {
                                ClearXml(record);
                                ClearCols(record, LibraryApplication.FILTERED);
                            }
                        }
                        else
                        {
                            // TODO: 根据实际需要增补 else if
                            ClearXml(record);
                            ClearCols(record, LibraryApplication.FILTERED);
                        }

                        // 读者身份，没有 getreaderinfo 权限。针对读者记录
                        if (bIsReader == true || bHasGetReaderInfoRight == false)
                        {
                            bool bIsReaderRecord = app.IsReaderDbName(strDbName);
                            if ((bIsReader == true || bHasGetReaderInfoRight == false)
                                && bIsReaderRecord == true)
                            {
                                // 不是读者自己的读者记录或者下级记录(比如对象记录)
                                if (sessioninfo.Account == null
                                    || StringUtil.IsEqualOrSubPath(sessioninfo.Account.ReaderDomPath, record.Path) == false)
                                {
                                    ClearRecord(record, "不具备 getreaderinfo 权限");
                                    /*
                                    record.Path = "";
                                    record.Cols = null;
                                    record.RecordBody = null;
                                    */
                                }
                            }
                        }
                        else if (sessioninfo.GlobalUser == false && bIsReader == false)
                        {
                            // 不是全局用户，不是读者。即不是全局用户的工作人员账户

                            bool bChangeable = true;

                            object o = table[strDbName];
                            if (o == null)
                            {
                                string strLibraryCode = "";
                                bool bReaderDbInCirculation = true;
                                if (app.IsReaderDbName(strDbName,
                                    out bReaderDbInCirculation,
                                    out strLibraryCode) == true)
                                {
                                    // 检查当前操作者是否管辖这个读者库
                                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                                    bChangeable = app.IsCurrentChangeableReaderPath(strDbName + "/?",
                            sessioninfo.LibraryCodeList);
                                }
                                table[strDbName] = bChangeable; // 记忆
                            }
                            else
                                bChangeable = (bool)o;

                            if (bChangeable == false)
                            {
                                // 当前用户不管辖此读者库
                                ClearRecord(record, "不具备 getreaderinfo 权限");
                                /*
                                record.Path = "";
                                record.Cols = null;
                                record.RecordBody = null;
                                */
                            }
                        }
                    }
                }

                // 注: 0 并不表示没有命中。命中数要看 searchresults.Length
                result.Value = lRet;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetBrowseRecords() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        static void ClearXml(Record record)
        {
            if (record == null || record.RecordBody == null)
                return;
            record.RecordBody.Xml = null;
        }

        // 2021/7/15
        // 清除全部 Cols 列内容
        static void ClearCols(Record record, string mask_string = "")
        {
            if (record == null || record.Cols == null || record.Cols.Length == 0)
                return;
            for (int i = 0; i < record.Cols.Length; i++)
            {
                if (i == 0)
                    record.Cols[i] = mask_string;
                else
                    record.Cols[i] = "";
            }
        }

        static void ClearRecord(Record record,
            string errorInfo)
        {
            // record.Path = "";    // 2021.5.19 考虑不滤除 Path
            record.Cols = null;
            record.RecordBody = new RecordBody
            {
                Path = null,
                Xml = null,
                Timestamp = null,
                Metadata = null,
                Result = new Result
                {
                    Value = -1,
                    ErrorString = errorInfo,
                    ErrorCode = ErrorCodeValue.AccessDenied
                }
            };
        }

        // 给册记录 XML 加上 OI 元素
        static void AddItemOI(LibraryApplication app,
            Record record)
        {
            if (record == null
                || record.RecordBody == null
                || string.IsNullOrEmpty(record.RecordBody.Xml))
                return;
            try
            {
                XmlDocument itemdom = new XmlDocument();
                itemdom.LoadXml(record.RecordBody.Xml);
                app.AddItemOI(itemdom);
                record.RecordBody.Xml = itemdom.DocumentElement?.OuterXml;
            }
            catch
            {

            }
        }

        // 列出 书目库/读者库/订购库/期库/评注库/发票库/违约金库/预约到书库 检索途径信息
        // parameters:
        //      strLang 语言代码。一般为"zh"
        //      infos   返回检索途径信息数组
        // rights:
        //      需要 listbibliodbfroms 或 listdbfroms 或 order 权限
        // return:
        //      result.Value    -1 出错；0 当前系统中没有定义此类数据库; 1: 成功(有至少一个此类数据库)
        public LibraryServerResult ListBiblioDbFroms(
            string strDbType,
            string strLang,
            out BiblioDbFromInfo[] infos)
        {
            infos = null;

            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("ListBiblioDbFroms", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("listbibliodbfroms,listdbfroms,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "列出书目库检索途径 被拒绝。不具备order或listbibliodbfroms或listdbfroms权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 列出某类数据库的检索途径信息
                // return:
                //      -1  出错
                //      0   没有定义
                //      1   成功
                int nRet = app.ListDbFroms(strDbType,
                    strLang,
                    sessioninfo.LibraryCodeList,
                    out infos,
                    out strError);

                result.Value = nRet;
                result.ErrorInfo = strError;

                return result;
                /*
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
                 * */
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ListBiblioDbFroms() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
#if NO
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
#endif
        }

        void WriteDebugInfo(string strTitle)
        {
            /*
            if (app.DebugMode == false)
                return;
            StreamUtil.WriteText(app.LogDir + "\\debug.txt", "-- " + DateTime.Now.ToString("u") + " " + strTitle + "\r\n");
             * */
            app.WriteDebugInfo(strTitle);
        }

        // 检索书目信息
        // parameters:
        //      strBiblioDbNames    书目库名。可以为单个库名，也可以是逗号(半角)分割的读者库名列表。还可以为 <全部>/<all> 之一，表示全部书目库。
        //      strQueryWord    检索词
        //      nPerMax 一次命中结果的最大数。如果为-1，表示不限制。
        //      strFromStyle 检索途径角色值。
        //      strMathStyle    匹配方式 exact left right middle
        //      strLang 语言代码。一般为"zh"
        //      strResultSetName    结果集名。
        //      strQueryXml 返回数据库内核层所使用的XML检索式，便于进行调试
        //      strSearchStyle  可以包含 desc，表示命中结果按照降序排列
        //      strOutputStyle  如果为"keycount"，表示输出key+count形式
        //      strLocationFilter   馆藏地点过滤条件
        // rights:
        //      需要 searchbiblio 权限
        // return:
        //      result.Value    命中结果总数。如果为-1，则表示检索出错
        public LibraryServerResult SearchBiblio(
            string strBiblioDbNames,
            string strQueryWord,
            int nPerMax,
            string strFromStyle,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            string strLocationFilter,
            out string strQueryXml)
        {
            strQueryXml = "";
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchBiblio", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchbiblio,order,searchauthority", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索书目或规范信息被拒绝。不具备order、searchbiblio或searchauthority权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                Int64 limit_maxCount = -1;
                // 根据权限，决定如何限定最大命中数
                if (StringUtil.IsInList("searchbiblio_unlimited", sessioninfo.RightsOrigin) == false)
                {
                    // library.xml 中配置一个 SearchBiblio() API 的常规限制极限命中数
                    limit_maxCount = app.BiblioSearchMaxCount;
                }

                if (limit_maxCount != -1)
                {
                    if (nPerMax == -1 || nPerMax > limit_maxCount)
                    {
                        // nPerMax = (int)limit_maxCount;
                        strError = $"检索被拒绝。因前端请求针对书目检索的最大命中结果数 {nPerMax} 超过 {limit_maxCount} 限制";
                        goto ERROR1;
                    }
                }
#if NO
                List<string> dbnames = new List<string>();

                if (String.IsNullOrEmpty(strBiblioDbNames) == true
                    || strBiblioDbNames == "<全部>"
                    || strBiblioDbNames.ToLower() == "<all>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strDbName = app.ItemDbs[i].BiblioDbName;

                        // 2008/10/16 
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        dbnames.Add(strDbName);
                    }
                }
                else if (strBiblioDbNames == "<全部图书>"
                    || strBiblioDbNames.ToLower() == "<all book>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        ItemDbCfg cfg = app.ItemDbs[i];
                        if (String.IsNullOrEmpty(cfg.IssueDbName) == false)
                            continue;
                        string strDbName = cfg.BiblioDbName;

                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        dbnames.Add(strDbName);
                    }
                }
                else if (strBiblioDbNames == "<全部期刊>"
                    || strBiblioDbNames.ToLower() == "<all series>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        ItemDbCfg cfg = app.ItemDbs[i];
                        if (String.IsNullOrEmpty(cfg.IssueDbName) == true)
                            continue;
                        string strDbName = cfg.BiblioDbName;

                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        dbnames.Add(strDbName);
                    }
                }
                else
                {
                    string[] splitted = strBiblioDbNames.Split(new char[] { ',' });
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        string strDbName = splitted[i];
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        if (app.IsBiblioDbName(strDbName) == false)
                        {
                            strError = "库名 '" + strDbName + "' 不是合法的书目库名";
                            goto ERROR1;
                        }

                        dbnames.Add(strDbName);
                    }

                }

                bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

                // 构造检索式
                string strFromList = "";
                string strUsedFromCaptions = "";
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    strError = EnsureKdbs(false);
                    if (strError != null)
                        goto ERROR1;

                    string strFromCaptions = app.kdbs.BuildCaptionListByStyleList(strDbName, strFromStyle, strLang);

                    if (String.IsNullOrEmpty(strFromCaptions) == true)
                    {
                        continue;
                    }

                    strUsedFromCaptions = strFromCaptions;

                    if (String.IsNullOrEmpty(strFromList) == false)
                        strFromList += ";";
                    strFromList += strDbName + ":" + strFromCaptions;
                }

                if (String.IsNullOrEmpty(strFromList) == true)
                {
                    strError = "在数据库 '" + StringUtil.MakePathList(dbnames) + "' 中没有找到匹配风格 '" + strFromStyle + "' 的From Caption";
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.FromNotFound;
                    return result;
                }

                string strRelation = "=";
                string strDataType = "string";

                if (strUsedFromCaptions == "__id")
                {
                    // 如果为范围式
                    if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOfAny(new char[] { '-', '~' }) != -1)
                    {
                        strRelation = "range";
                        strDataType = "number";
                        // 2012/3/29
                        strMatchStyle = "exact";
                    }
                    else if (String.IsNullOrEmpty(strQueryWord) == false)
                    {
                        // 2008/3/9 
                        strDataType = "number";
                        // 2012/3/29
                        strMatchStyle = "exact";
                    }
                }
                /*
            else if (strUsedFromCaptions == "操作时间"
                || strUsedFromCaptions == "出版时间")                     * */
                else if (StringUtil.IsInList("_time", strFromStyle) == true)
                {
                    // 如果为范围式
                    if (strQueryWord.IndexOf("~") != -1)
                    {
                        strRelation = "range";
                        strDataType = "number";
                    }
                    else
                    {
                        strDataType = "number";

                        // 2012/3/29
                        // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                        // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                        if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                        {
                            strMatchStyle = "exact";
                            strRelation = "range";
                            strQueryWord = "~";
                        }
                    }

                    // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                    strMatchStyle = "exact";
                }

                strQueryXml = "";
                strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strFromList)
                    + "'><option warning='0'/><item>"
                    + (bDesc == true ? "<order>DESC</order>" : "")
                    + "<word>"
                    + StringUtil.GetXmlStringSimple(strQueryWord)
                    + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";
#endif

                // 构造检索书目库的 XML 检索式
                // return:
                //      -2  没有找到指定风格的检索途径
                //      -1  出错
                //      0   没有发现任何书目库定义
                //      1   成功
                int nRet = app.BuildSearchBiblioQuery(
        strBiblioDbNames,
        strQueryWord,
        nPerMax,
        strFromStyle,
        strMatchStyle,
        strLang,
        strSearchStyle,
        out List<string> dbTypes,
        out strQueryXml,
                out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;
                if (nRet == -2)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.FromNotFound;
                    return result;
                }

                // 再次检查权限。分为书目和规范两种细致检查
                foreach (string type in dbTypes)
                {
                    if (type == "biblio")
                    {
                        if (StringUtil.IsInList("searchbiblio,order", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "检索书目信息被拒绝。不具备 order 或 searchbiblio权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    else if (type == "authority")
                    {
                        if (StringUtil.IsInList("searchauthority", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "检索规范信息被拒绝。不具备 searchauthority 权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                // strLocationFilter = "海淀分馆"; // testing
                if (string.IsNullOrEmpty(strLocationFilter) == false)
                {
                    string strOperator = "AND";
                    if (strLocationFilter.StartsWith("-"))
                    {
                        strLocationFilter = strLocationFilter.Substring(1);
                        strOperator = "SUB";
                    }
                    string strLocationQueryXml = "<item resultset='#" + strLocationFilter + "' />";
                    strQueryXml = $"<group>{strQueryXml}<operator value='{strOperator}'/>{strLocationQueryXml}</group>";    // !!!
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,   // "default",
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        if (channel.ErrorCode == ChannelErrorCode.RequestCanceled)
                            result.ErrorCode = ErrorCode.RequestCanceled;
                        else
                            result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchBiblio() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // TODO: 需要增加返回保存后记录 XML 的参数。因为保存过程中，可能会略微修改前端提交的记录，比如增加一些字段
        // 设置书目信息或者规范信息(目前只能xml一种格式)
        // 权限:   需要具有setbiblioinfo权限
        // parameters:
        //      strAction   动作。为"new" "change" "delete" "onlydeletebiblio" "onlydeletesubrecord"之一。"delete"在删除书目记录的同时，会自动删除下属的实体记录。不过要求实体均未被借出才能删除。
        //      strBiblioType   xml 或 iso2709。iso2709 格式可以包含编码方式，例如 iso2709:utf-8
        //      baTimestamp 时间戳。如果为新创建记录，可以为null 
        //      strOutputBiblioRecPath 输出的书目记录路径。当strBiblioRecPath中末级为问号，表示追加保存书目记录的时候，本参数返回实际保存的书目记录路径
        //                      此参数也用于，当保存前查重时发现了重复的书目记录，这里返回这些书目记录的路径
        //      baOutputTimestamp   操作完成后，新的时间戳
        // Result.Value -1出错 0成功 >0 表示查重发现了重复的书目记录，保存被拒绝
        public LibraryServerResult SetBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            string strStyle,    // 2016/12/22
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp)
        {
            strOutputBiblioRecPath = "";
            baOutputTimestamp = null;

            LibraryServerResult result = this.PrepareEnvironment("SetBiblioInfo", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                if (strAction == "notifynewbook")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("setbiblioinfo,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "通知读者新书到达被拒绝。不具备order或setbiblioinfo权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    return app.NotifyNewBook(
                       sessioninfo,
                       strBiblioRecPath,
                       strBiblioType);
                }

                return app.SetBiblioInfo(
                    sessioninfo,
                    strAction,
                    strBiblioRecPath,
                    strBiblioType,
                    strBiblio,
                    baTimestamp,
                    strComment,
                    strStyle,
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetBiblioInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 2009/10/31 
        // 复制或者移动书目信息(目前只能xml一种格式)
        // 权限:   需要具有setbiblioinfo权限
        // parameters:
        //      strAction   动作。为"onlycopybiblio" "onlymovebiblio" "copy" "move" 之一
        //      strBiblioType   目前只允许xml一种
        //      strBiblio   源书目记录。目前需要用null调用
        //      baTimestamp 源记录的时间戳
        //      strNewBiblio    需要在目标记录中更新的内容。如果 == null，表示不特意更新
        //      strOutputBiblioRecPath 输出的书目记录路径。当strBiblioRecPath中末级为问号，表示追加保存书目记录的时候，本参数返回实际保存的书目记录路径
        //      baOutputTimestamp   操作完成后，新的时间戳
        // result.Value:
        //      -1  出错
        //      0   成功，没有警告信息。
        //      1   成功，有警告信息。警告信息在 result.ErrorInfo 中
        public LibraryServerResult CopyBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strNewBiblioRecPath,
            string strNewBiblio,
            string strMergeStyle,
            out string strOutputBiblio,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp)
        {
            strOutputBiblioRecPath = "";
            baOutputTimestamp = null;
            strOutputBiblio = "";

            LibraryServerResult result = this.PrepareEnvironment("CopyBiblioInfo", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                /*
                // 权限字符串
                if (StringUtil.IsInList("setbiblioinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "复制书目信息被拒绝。不具备 order 或 setbiblioinfo 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                */

                return app.CopyBiblioInfo(
                    sessioninfo,
                    strAction,
                    strBiblioRecPath,
                    strBiblioType,
                    strBiblio,
                    baTimestamp,
                    strNewBiblioRecPath,
                    strNewBiblio,
                    strMergeStyle,
                    out strOutputBiblio,
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library CopyBiblioInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得书目信息
        // parameters:
        //      strBiblioRecPath    书目记录路径。
        //      strBiblioXml    如果不为空，表示前端发送过来的一条XML格式的记录，就不用从数据库中去取了
        //      strBiblioType   xml targetrecpath html text @...
        // Result.Value -1出错 0没有找到 1找到
        // 权限:   需要具有getbiblioinfo权限
        public LibraryServerResult GetBiblioInfo(
    string strBiblioRecPath,
    string strBiblioXml,
    string strBiblioType,
    out string strBiblio)
        {
            strBiblio = "";

            LibraryServerResult result = this.PrepareEnvironment("GetBiblioInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // test
                // Thread.Sleep(new TimeSpan(0, 0, 30));

                string[] results = null;
                byte[] baTimestamp = null;

                string[] formats = new string[1];
                formats[0] = strBiblioType;
                result = app.GetBiblioInfos(
                    sessioninfo,
                    strBiblioRecPath,
                    strBiblioXml,
                    formats,
                    out results,
                    out baTimestamp);
                if (results != null && results.Length > 0)
                    strBiblio = results[0];

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetBiblioInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

#if NO
        // 获得书目信息(可以用html或xml两种格式之一) 2006/9/18 
        // parameters:
        //      strBiblioRecPath    书目记录路径。
        //      strBiblioXml    如果不为空，表示前端发送过来的一条XML格式的记录，就不用从数据库中去取了
        //      strBiblioType   xml targetrecpath html text @...
        // Result.Value -1出错 0没有找到 1找到
        // 权限:   需要具有getbiblioinfo权限
        public LibraryServerResult GetBiblioInfo(
            string strBiblioRecPath,
            string strBiblioXml,
            string strBiblioType,
            out string strBiblio)
        {
            strBiblio = "";

            LibraryServerResult result = this.PrepareEnvironment(true, true);
            if (result.Value == -1)
                return result;

            try
            {
                int nRet = 0;
                long lRet = 0;

                // string strXml = "";
                string strError = "";
                string strOutputPath = "";

                if (String.IsNullOrEmpty(strBiblioType) == true)
                {
                    strError = "strBiblioType参数不能为空";
                    goto ERROR1;
                }

                // 检查数据库路径，看看是不是已经正规定义的编目库？
                if (String.IsNullOrEmpty(strBiblioRecPath) == true)
                {
                    strError = "strBiblioRecPath参数不能为空";
                    goto ERROR1;
                }

                string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

                if (app.IsBiblioDbName(strBiblioDbName) == false)
                {
                    strError = "书目记录路径 '" + strBiblioRecPath + "' 中包含的数据库名 '" + strBiblioDbName + "' 不是合法的书目库名";
                    goto ERROR1;
                }

                bool bRightVerified = false;

                // 检查存取权限
                if (String.IsNullOrEmpty(sessioninfo.Access) == false)
                {
                    string strAction = "*";

                    string strActionList = LibraryApplication.GetDbOperRights(sessioninfo.Access,
                        strBiblioDbName,
                        "getbiblioinfo");
                    if (String.IsNullOrEmpty(strActionList) == true)
                    {
                        strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 getbiblioinfo 操作的存取权限";
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    if (strActionList == "*")
                    {
                        // 通配
                    }
                    else
                    {
                        if (StringUtil.IsInList(strAction, strActionList) == false)
                        {
                            strError = "当前用户 '" + sessioninfo.UserID + "' 不具备 针对数据库 '" + strBiblioDbName + "' 执行 getbiblioinfo 操作的存取权限";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    bRightVerified = true;
                }

                if (bRightVerified == false)
                {
                    // 权限字符串
                    if (StringUtil.IsInList("getbiblioinfo", sessioninfo.RightsOrigin) == false
                        && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "获取书目信息被拒绝。不具备order或getbiblioinfo权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // string strBiblioXml = "";

                if (String.IsNullOrEmpty(strBiblioXml) == false)
                {
                    // 前端已经发送过来一条记录

                }
                else
                {
                    // 从数据库中获取
                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    string strMetaData = "";
                    byte[] timestamp = null;
                    lRet = channel.GetRes(strBiblioRecPath,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            result.Value = 0;
                            return result;
                        }
                        strError = "获得书目记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }
                }

                // 表明只需获取局部数据
                if (strBiblioType[0] == '@')
                {
                    string strPartName = strBiblioType.Substring(1);

                    XmlDocument bibliodom = new XmlDocument();

                    try
                    {
                        bibliodom.LoadXml(strBiblioXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "将XML装入DOM时失败: " + ex.Message;
                        goto ERROR1;
                    }
                    int nResultValue = 0;

                    // 执行脚本函数GetBiblioPart
                    // parameters:
                    // return:
                    //      -2  not found script
                    //      -1  出错
                    //      0   成功
                    nRet = app.DoGetBiblioPartScriptFunction(
                        bibliodom,
                        strPartName,
                        out nResultValue,
                        out strBiblio,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                    {
                        strError = "获得书目记录 '" + strBiblioRecPath + "' 的局部 " + strBiblioType + " 时出错: " + strError;
                        goto ERROR1;
                    }

                    result.Value = nResultValue;
                    return result;
                }

                // 如果只需要种记录的XML格式
                if (String.Compare(strBiblioType, "xml", true) == 0)
                {
                    strBiblio = strBiblioXml;
                }
                // 目标记录路径
                else if (String.Compare(strBiblioType, "targetrecpath", true) == 0)
                {
                    // 获得目标记录路径。998$t
                    // return:
                    //      -1  error
                    //      0   OK
                    nRet = LibraryApplication.GetTargetRecPath(strBiblioXml,
                        out strBiblio,
                        out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                }
                else if (String.Compare(strBiblioType, "html", true) == 0)
                {
                    // string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                    // 是否需要检查这个数据库名确实为书目库名？

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    nRet = app.MapKernelScriptFile(
                        sessioninfo,
                        strBiblioDbName,
                        "./cfgs/loan_biblio.fltx",
                        out strLocalPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;


                    // 将种记录数据从XML格式转换为HTML格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";

                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = app.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                                strBiblioXml,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strBiblio = "";
                }
                else if (String.Compare(strBiblioType, "text", true) == 0)
                {
                    // string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                    // 是否需要检查这个数据库名确实为书目库名？

                    // 需要从内核映射过来文件
                    string strLocalPath = "";
                    nRet = app.MapKernelScriptFile(
                        sessioninfo,
                        strBiblioDbName,
                        "./cfgs/loan_biblio_text.fltx",
                        out strLocalPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;


                    // 将种记录数据从XML格式转换为text格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";

                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = app.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                                strBiblioXml,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strBiblio = "";
                }
                else
                {
                    strError = "未知的书目格式 '" + strBiblioType + "'";
                    goto ERROR1;
                }

                result.Value = 1;

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetBiblioInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }


#endif

        // 获得书目记录信息(一次可以获得多种)
        // parameters:
        //      strBiblioRecPath    书目记录的路径
        //      formats 格式列表。可以用后列的多种格式：xml html text @??? summary
        //      results 返回的结果字符串数组
        //      baTimestamp 返回的记录时间戳
        // rights:
        //      需要 getbiblioinfo 权限
        //      如果formats中包含了"summary"格式，还需要 getbibliosummary 权限
        // return:
        //      result.Value    -1出错; 0 没有找到; 1 找到
        public LibraryServerResult GetBiblioInfos(
            string strBiblioRecPath,
            string strBiblioXml,    // 2013/3/6
            string[] formats,
            out string[] results,
            out byte[] baTimestamp)
        {
            results = null;
            baTimestamp = null;

            LibraryServerResult result = this.PrepareEnvironment("GetBiblioInfos", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                /*
                // 权限字符串
                if (StringUtil.IsInList("getbiblioinfo", sessioninfo.RightsOrigin) == false
                    && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取书目信息被拒绝。不具备order或getbiblioinfo权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
                 * */
                // test
                // Thread.Sleep(new TimeSpan(0, 0, 30));

                return app.GetBiblioInfos(
                        sessioninfo,
                        strBiblioRecPath,
                        strBiblioXml,
                        formats,
                        out results,
                        out baTimestamp);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetBiblioInfos() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 检索册信息
        // parameters:
        //      strQueryWord    检索词
        //      strFrom 检索途径
        //      strMathStyle    匹配方式 exact left right middle
        //      strOutputStyle  特殊用法, 如果包含 __buildqueryxml，则在result.ErrorInfo中返回XML检索式，但不进行检索
        // 权限: 
        //      需要 searchitem 权限
        // return:
        //      result.Value    命中结果总数。如果为-1，则表示检索出错
        public LibraryServerResult SearchItem(
            string strItemDbName,   // 2007/9/25 
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchItem", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // testing
                // Thread.Sleep(1000*60*5);

                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchitem,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索实体信息被拒绝。不具备order、searchitem权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

#if NO
                List<string> dbnames = new List<string>();

                if (String.IsNullOrEmpty(strItemDbName) == true
                    || strItemDbName == "<全部>"
                    || strItemDbName.ToLower() == "<all>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strDbName = app.ItemDbs[i].DbName;
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;
                        dbnames.Add(strDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何实体库";
                        goto ERROR1;
                    }

                }
                else if (strItemDbName == "<全部期刊>"
                    || strItemDbName.ToLower() == "<all series>")
                {
                    // 2009/2/2 
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strCurrentItemDbName = app.ItemDbs[i].DbName;
                        string strCurrentIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strCurrentItemDbName) == true)
                            continue;

                        if (String.IsNullOrEmpty(strCurrentIssueDbName) == true)
                            continue;

                        dbnames.Add(strCurrentItemDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何期刊实体库";
                        goto ERROR1;
                    }
                }
                else if (strItemDbName == "<全部图书>"
                    || strItemDbName.ToLower() == "<all book>")
                {
                    // 2009/2/2 
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strCurrentItemDbName = app.ItemDbs[i].DbName;
                        string strCurrentIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strCurrentItemDbName) == true)
                            continue;

                        // 大书目库中必须不包含期库，说明才是图书用途
                        if (String.IsNullOrEmpty(strCurrentIssueDbName) == false)
                            continue;

                        dbnames.Add(strCurrentItemDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何图书实体库";
                        goto ERROR1;
                    }
                }
                else
                {
                    string[] splitted = strItemDbName.Split(new char[] { ',' });
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        string strDbName = splitted[i];
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        if (app.IsItemDbName(strDbName) == false)
                        {
                            strError = "库名 '" + strDbName + "' 不是合法的实体库名";
                            goto ERROR1;
                        }

                        dbnames.Add(strDbName);
                    }

                }

                bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

                // 构造检索式
                string strQueryXml = "";
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    strError = EnsureKdbs(false);
                    if (strError != null)
                        goto ERROR1;

                    string strFromStyle = app.kdbs.GetFromStyles(strDbName, strFrom, strLang);

                    string strRelation = "=";
                    string strDataType = "string";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOfAny(new char[] { '-', '~' }) != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                        else if (String.IsNullOrEmpty(strQueryWord) == false)
                        {
                            strDataType = "number";
                            // 2012/3/29
                            strMatchStyle = "exact";
                        }
                    }
                    // 2014/8/28
                    else if (StringUtil.IsInList("_time", strFromStyle) == true)
                    {
                        // 如果为范围式
                        if (strQueryWord.IndexOf("~") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else
                        {
                            strDataType = "number";

                            // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                            // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                            if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                            {
                                strMatchStyle = "exact";
                                strRelation = "range";
                                strQueryWord = "~";
                            }
                        }

                        // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                        strMatchStyle = "exact";
                    }

                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)    // 2007/9/14 
                        + "'><item>"
                        + (bDesc == true ? "<order>DESC</order>" : "")
                    + "<word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                    if (i > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                }

                if (dbnames.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }

#endif

                string strQueryXml = "";
                // 构造检索实体库的 XML 检索式
                // return:
                //      -1  出错
                //      0   没有发现任何实体库定义
                //      1   成功
                int nRet = app.BuildSearchItemQuery(
    strItemDbName,
    strQueryWord,
    nPerMax,
    strFrom,
    strMatchStyle,
    strLang,
    strSearchStyle,
                out strQueryXml,
            out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                if (StringUtil.IsInList("__buildqueryxml", strOutputStyle) == true)
                {
                    result.Value = 0;
                    result.ErrorInfo = strQueryXml;
                    return result;
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,   // "default",
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                return result;

            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchItem() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }

        }

        // 获得册信息
        // TODO: 需要改进为，如果册记录存在，但是书目记录不存在，也能够适当返回
        // parameters:
        //      strItemDbType   2015/1/30
        //      strBarcode  册条码号。特殊情况下，可以使用"@path:"引导的册记录路径(只需要库名和id两个部分)作为检索入口。在@path引导下，路径后面还可以跟随 "$prev"或"$next"表示方向
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据，strBilbioRecPath中也不返回路径。
        //                      如果要仅仅在strBiblioRecPath中返回路径，请使用"recpath"作为strBiblioType参数的值。
        //                      如果为"html"或"xml"之一，则会在strBiblioRecPath中返回路径。
        //                      之所以要这样设计，主要是为了效率考虑。用""调用时，甚至不需要返回书目记录路径，这会更多地省去一些关于种的操作。
        //      strBiblioRecPath    返回书目记录路径
        // return:
        // Result.Value -1出错 0册记录没有找到 1册记录找到 >1册记录命中多于1条
        // 权限:   需要具有getiteminfo权限
        public LibraryServerResult GetItemInfo(
            string strItemDbType,
            string strBarcode,
            string strItemXml,  // 前端提供给服务器的记录内容。例如，需要模拟创建检索点，就需要前端提供记录内容
            string strResultType,
            out string strResult,
            out string strItemRecPath,
            out byte[] item_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strBiblioRecPath)
        {
            strResult = "";
            strBiblio = "";
            strItemRecPath = "";
            strBiblioRecPath = "";
            item_timestamp = null;

            LibraryServerResult result = this.PrepareEnvironment("GetItemInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                if (string.IsNullOrEmpty(strItemDbType) == true)
                    strItemDbType = "item";

                if (strItemDbType == "item")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("getiteminfo,getentities,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "用户 '" + sessioninfo.UserID + "' 获取实体信息被拒绝。不具备order、getiteminfo或getentities权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else if (strItemDbType == "order")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("getorderinfo,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "用户 '" + sessioninfo.UserID + "' 获取订购信息被拒绝。不具备order或getorderinfo权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else if (strItemDbType == "issue")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("getissueinfo,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "用户 '" + sessioninfo.UserID + "' 获取期信息被拒绝。不具备order或getissueinfo权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else if (strItemDbType == "comment")
                {
                    // 权限字符串
                    if (StringUtil.IsInList("getcommentinfo,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "用户 '" + sessioninfo.UserID + "' 获取评注信息(GetItemInfo()被拒绝。不具备 getcommentinfo 或 order 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // test
                // Thread.Sleep(new TimeSpan(0, 0, 30));

                int nRet = 0;
                long lRet = 0;

                string strXml = "";
                string strError = "";
                // string strOutputPath = "";

                if (String.IsNullOrEmpty(strBarcode) == true)
                {
                    strError = "strBarcode参数不能为空";
                    goto ERROR1;
                }

                string strBiblioDbName1 = "";

                // 特殊用法 @barcode-list: 获得册记录路径列表
                if (StringUtil.HasHead(strBarcode, "@barcode-list:") == true
                    && strResultType == "get-path-list")
                {
                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    nRet = app.GetItemRecPathList(
                        // sessioninfo.Channels,
                        channel,
                        strItemDbType,  // "item",
                        "册条码",
                        strBarcode.Substring("@barcode-list:".Length),
                        true,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    result.ErrorInfo = "";
                    result.Value = 1;
                    return result;
                }

                // 特殊用法 @refid-list: 获得册记录路径列表
                if (StringUtil.HasHead(strBarcode, "@refid-list:") == true
                    && strResultType == "get-path-list")
                {
                    string strFrom = "";
                    if (strItemDbType == "item")
                        strFrom = "参考ID";
                    else
                    {
                        strError = "@refid-list 功能只能针对 item 类型的库";
                        goto ERROR1;
                    }
                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    nRet = app.GetItemRecPathList(
                        // sessioninfo.Channels,
                        channel,
                        strItemDbType,  // "item",
                        strFrom,    // "参考ID",  // "册条码号"?
                        strBarcode.Substring("@refid-list:".Length),
                        true,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    result.ErrorInfo = "";
                    result.Value = 1;
                    return result;
                }

                // 特殊用法 @item-refid-list: 获得册记录路径列表
                if (StringUtil.HasHead(strBarcode, "@item-refid-list:") == true
                    && strResultType == "get-path-list")
                {
                    string strFrom = "";
                    if (strItemDbType == "order")
                        strFrom = "册参考ID";
                    else if (strItemDbType == "issue")
                        strFrom = "册参考ID";
                    else
                    {
                        strError = "@item-refid-list 功能只能针对 order issue 类型的库";
                        goto ERROR1;
                    }

                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    nRet = app.GetItemRecPathList(
                        // sessioninfo.Channels,
                        channel,
                        strItemDbType,  // "item",
                        strFrom,    // "册参考ID",
                        strBarcode.Substring("@item-refid-list:".Length),
                        true,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    result.ErrorInfo = "";
                    result.Value = 1;
                    return result;
                }

                // 获得一条册、期、订购、评注记录
                // parameters:
                //      strRefID    当 strDbType 为 "item" 时，strRefID 中是册条码号。其他情况，是 refid 字符串
                //      item_dom    [out] 返回记录的 XmlDocument 形态。注意有时候可能为 null (当 strXml 为空时)
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetItemXml(
                    strItemDbType,  // "item",
                    strBarcode,
                    strItemXml,
                    ref strItemRecPath,
                    ref item_timestamp,
                    ref strBiblioDbName1,
                    ref strBiblioRecPath,
                    ref result,
                    out strXml,
                    out XmlDocument item_dom,
                    out strError);
                if (nRet == 0)
                    return result;
                if (nRet == -1)
                    goto ERROR1;
#if NO
                // 命令状态
                if (strBarcode[0] == '@')
                {
                    // 获得册记录，通过册记录路径

                    string strLeadPath = "@path:";
                    string strLeadRefID = "@refID:";

                    /*
                    if (strBarcode.Length <= strLeadPath.Length)
                    {
                        strError = "错误的检索词格式: '" + strBarcode + "'";
                        goto ERROR1;
                    }
                    string strPart = strBarcode.Substring(0, strLeadPath.Length);
                     * */


                    if (StringUtil.HasHead(strBarcode, strLeadPath) == true)
                    {
                        strItemRecPath = strBarcode.Substring(strLeadPath.Length);

                        // 2009/10/18 
                        // 继续分离出(方向)命令部分
                        string strCommand = "";
                        nRet = strItemRecPath.IndexOf("$");
                        if (nRet != -1)
                        {
                            strCommand = strItemRecPath.Substring(nRet + 1);
                            strItemRecPath = strItemRecPath.Substring(0, nRet);
                        }

                        string strItemDbName = ResPath.GetDbName(strItemRecPath);
                        // 需要检查一下数据库名是否在允许的实体库名之列
                        if (app.IsItemDbName(strItemDbName) == false)
                        {
                            strError = "册记录路径 '" + strItemRecPath + "' 中的数据库名 '" + strItemDbName + "' 不在配置的实体库名之列，因此拒绝操作。";
                            goto ERROR1;
                        }

                        string strMetaData = "";
                        // byte[] timestamp = null;
                        string strTempOutputPath = "";

                        RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                        if (channel == null)
                        {
                            strError = "get channel error";
                            goto ERROR1;
                        }

                        // 2009/10/18 
                        string strStyle = "content,data,metadata,timestamp,outputpath";

                        // 为了便于处理对象资源
                        strStyle += ",withresmetadata";

                        if (String.IsNullOrEmpty(strCommand) == false
                            && (strCommand == "prev" || strCommand == "next"))
                        {
                            strStyle += "," + strCommand;
                        }

                        /*
                        lRet = channel.GetRes(strItemRecPath,
                            out strXml,
                            out strMetaData,
                            out item_timestamp,
                            out strTempOutputPath,
                            out strError);
                         * */

                        lRet = channel.GetRes(strItemRecPath,
                            strStyle,
                            out strXml,
                            out strMetaData,
                            out item_timestamp,
                            out strTempOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            {
                                result.Value = 0;
                                if (strCommand == "prev")
                                    result.ErrorInfo = "到头";
                                else if (strCommand == "next")
                                    result.ErrorInfo = "到尾";
                                else
                                    result.ErrorInfo = "没有找到";
                                result.ErrorCode = ErrorCode.NotFound;
                                return result;
                            }
                            goto ERROR1;
                        }
                        
                        strItemRecPath = strTempOutputPath;

                        result.ErrorInfo = "";
                        result.Value = 1;
                        goto GET_OTHERINFO;
                    }
                    else if (StringUtil.HasHead(strBarcode, strLeadRefID) == true)
                    {
                        // 继续向后处理
                    }
                    else
                    {
                        strError = "不支持的检索词格式: '" + strBarcode + "'。目前仅支持'@path:'和'@refID:'引导的检索词";
                        goto ERROR1;
                    }
                }


                {
                    List<string> PathList = null;
                    // byte[] timestamp = null;
                    // 获得册记录
                    // 本函数可获得超过1条以上的路径
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = app.GetItemRecXml(
                            sessioninfo.Channels,
                            strBarcode,
                            "withresmetadata",
                            out strXml,
                            100,
                            out PathList,
                            out item_timestamp,
                            out strError);
                    if (nRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "没有找到";
                        result.ErrorCode = ErrorCode.NotFound;
                        return result;
                    }

                    if (nRet == -1)
                        goto ERROR1;


                    /*
                    Debug.Assert(PathList != null, "");
                    // 构造路径字符串。逗号间隔
                    string[] paths = new string[PathList.Count];
                    PathList.CopyTo(paths);

                    strItemRecPath = String.Join(",", paths);
                     * */
                    strItemRecPath = StringUtil.MakePathList(PathList);

                    result.ErrorInfo = strError;
                    result.Value = nRet;    // 可能会多于1条
                }

#endif

                // GET_OTHERINFO:

                // 过滤<borrower>元素
                // Debug.Assert(string.IsNullOrEmpty(strBiblioRecPath) == false, "string.IsNullOrEmpty(strBiblioRecPath) == false 未满足");
                // Debug.Assert(item_dom != null, "item_dom != null 未满足");

                // 修改<borrower>
                if (item_dom != null && item_dom.DocumentElement != null
                    && strItemDbType == "item" && sessioninfo.GlobalUser == false) // 分馆用户必须要过滤，因为要修改<borrower>
                {
#if NO
                    nRet = LibraryApplication.LoadToDom(strXml,
                        out itemdom,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#endif

                    {
                        // 检查一个册记录的馆藏地点是否符合当前用户管辖的馆代码列表要求
                        // return:
                        //      -1  检查过程出错
                        //      0   符合要求
                        //      1   不符合要求
                        nRet = app.CheckItemLibraryCode(item_dom,
                                    sessioninfo.LibraryCodeList,
                                    out string strLibraryCode,
                                    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (nRet == 1)
                        {
                            // 把借阅人的证条码号覆盖
                            string strBorrower = DomUtil.GetElementText(item_dom.DocumentElement,
                                "borrower");
                            if (string.IsNullOrEmpty(strBorrower) == false)
                                DomUtil.SetElementText(item_dom.DocumentElement,
                                    "borrower", new string('*', strBorrower.Length));
                            strXml = item_dom.DocumentElement.OuterXml;
                        }
                    }
                }

                // 取得册信息
                if (String.IsNullOrEmpty(strResultType) == true
                    || String.Compare(strResultType, "recpath", true) == 0)
                {
                    strResult = ""; // 不返回任何结果
                }
                else if (String.Compare(strResultType, "xml", true) == 0)
                {
                    // 2020/8/27
                    // 加上 oi 元素
                    try
                    {
                        XmlDocument itemdom = new XmlDocument();
                        itemdom.LoadXml(strXml);
                        app.AddItemOI(itemdom);
                        strResult = itemdom.DocumentElement.OuterXml;
                    }
                    catch (Exception ex)
                    {
                        app.WriteErrorLog($"(GetItemInfo())为册记录添加 oi 元素时出现异常: \r\n{ExceptionUtil.GetDebugText(ex)}\r\n\r\n册记录 XML:'{strXml}'");
                        strResult = strXml;
                    }
                }
                else if (String.Compare(strResultType, "uii", true) == 0)
                {
                    try
                    {
                        XmlDocument itemdom = new XmlDocument();
                        itemdom.LoadXml(strXml);
                        // 加上 oi 元素
                        app.AddItemOI(itemdom);
                        string oi = DomUtil.GetElementText(itemdom.DocumentElement, "oi");
                        string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
                        strResult = oi + "." + barcode;
                        if (string.IsNullOrEmpty(oi))
                            strResult = barcode;
                    }
                    catch (Exception ex)
                    {
                        app.WriteErrorLog($"(GetItemInfo())为册记录添加 oi 元素时出现异常: \r\n{ExceptionUtil.GetDebugText(ex)}\r\n\r\n册记录 XML:'{strXml}'");
                        strError = $"(GetItemInfo())为册记录添加 oi 元素时出现异常: {ex.Message}";
                        goto ERROR1;
                    }
                }
                else if (String.Compare(strResultType, "html", true) == 0)
                {
                    // 将册记录数据从XML格式转换为HTML格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\" + strItemDbType + "xml2html.cs",
                        app.CfgDir + "\\" + strItemDbType + "xml2html.cs.ref",
                        strXml,
                        strItemRecPath, // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (String.Compare(strResultType, "text", true) == 0)
                {
                    // 将册记录数据从XML格式转换为text格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\" + strItemDbType + "xml2text.cs",
                        app.CfgDir + "\\" + strItemDbType + "xml2text.cs.ref",
                        strXml,
                        strItemRecPath, // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                // 模拟创建检索点
                else if (String.Compare(strResultType, "keys", true) == 0)
                {
                    nRet = app.GetKeys(sessioninfo,
                        strItemRecPath,
                        strXml,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strError = "未知的" + strItemDbType + "记录结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }

                // 若需要同时取得种记录
                if (String.IsNullOrEmpty(strBiblioType) == false
                    && string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    // Debug.Assert(string.IsNullOrEmpty(strBiblioRecPath) == false, "string.IsNullOrEmpty(strBiblioRecPath) == false 未满足");

#if NO
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        string strItemDbName = ResPath.GetDbName(strItemRecPath);
                        string strBiblioDbName = "";

                        // 根据实体库名, 找到对应的书目库名
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = app.GetBiblioDbNameByItemDbName(strItemDbName,
                            out strBiblioDbName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            strError = "实体库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                            goto ERROR1;
                        }

                        string strBiblioRecID = "";

                        if (itemdom == null)
                        {
                            itemdom = new XmlDocument();
                            try
                            {
                                itemdom.LoadXml(strXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "册记录XML装载到DOM出错:" + ex.Message;
                                goto ERROR1;
                            }
                        }

                        strBiblioRecID = DomUtil.GetElementText(itemdom.DocumentElement, "parent"); //
                        if (String.IsNullOrEmpty(strBiblioRecID) == true)
                        {
                            strError = "册记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                            goto ERROR1;
                        }

                        strBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
                    }
#endif

                    if (String.Compare(strBiblioType, "recpath", true) == 0)
                    {
                        // 如果仅仅需要获得书目记录recpath，则不需要获得书目记录
                        goto END1;
                    }

                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }

                    lRet = channel.GetRes(strBiblioRecPath,
                        out string strBiblioXml,
                        out string strMetaData,
                        out byte[] timestamp,
                        out string strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            result.ErrorInfo = "书目记录 " + strBiblioRecPath + " 不存在";
                            return result;
                        }

                        strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }

                    // 如果只需要种记录的XML格式
                    if (String.Compare(strBiblioType, "xml", true) == 0)
                    {
                        strBiblio = strBiblioXml;
                        goto END1;
                    }

                    // 需要从内核映射过来文件
                    string strLocalPath = "";

                    if (String.Compare(strBiblioType, "html", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName1,
                            "./cfgs/loan_biblio.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else if (String.Compare(strBiblioType, "text", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName1,
                            "./cfgs/loan_biblio_text.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else
                    {
                        strError = "不能识别的strBiblioType类型 '" + strBiblioType + "'";
                        goto ERROR1;
                    }

                    if (nRet == -1)
                        goto ERROR1;

                    // 将种记录数据从XML格式转换为HTML格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = app.ConvertBiblioXmlToHtml(
                                strFilterFileName,
                                strBiblioXml,
                                null,
                                strBiblioRecPath,
                                out strBiblio,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strBiblio = "";
                }

            END1:
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetItemInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // *** 此API已经废止 ***
        // 对册条码号进行查重
        // 2006/9/22 新增API
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有searchitemdup权限
        public LibraryServerResult SearchItemDup(string strBarcode,
            int nMax,
            out string[] paths)
        {
            paths = null;

            LibraryServerResult result = this.PrepareEnvironment("SearchItemDup", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("searchitem,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "册条码号查重被拒绝。不具备order或searchitem权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;

                List<string> aPath = null;
                string strError = "";

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {

                    // 根据册条码号对实体库进行查重
                    // 本函数只负责查重, 并不获得记录体
                    // return:
                    //      -1  error
                    //      其他    命中记录条数(不超过nMax规定的极限)
                    nRet = app.SearchItemRecDup(
                        // sessioninfo.Channels,
                        channel,
                        strBarcode,
                        "册条码",
                        nMax,
                        out aPath,
                        out strError);
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    paths = new string[0];
                    result.Value = 0;
                    result.ErrorInfo = "没有找到";
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }

                // 复制到结果中
                paths = new string[aPath.Count];
                for (int i = 0; i < aPath.Count; i++)
                {
                    paths[i] = aPath[i];
                }

                result.Value = paths.Length;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchItemDup() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 从册条码号(+册记录路径)获得种记录摘要，或者从订购记录路径、期记录路径、评注记录路径获得种记录摘要
        // parameters:
        //      strItemBarcode  册条码号。可以使用 @refID: @bibliorecpath: 前缀
        // Result.Value -1出错 0没有找到 1找到
        // 权限:   需要具备getbibliosummary权限
        public LibraryServerResult GetBiblioSummary(
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary)
        {
            strBiblioRecPath = "";
            strSummary = "";
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("GetBiblioSummary", true, true);
            if (result.Value == -1)
                return result;

            RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            BeginSearch();
            channel.Idle += new IdleEventHandler(channel_IdleEvent);

            try
            {
#if NO
                // test
                Random random = new Random();
                Thread.Sleep(random.Next(0, 10000)); // test
#endif

                // parameters:
                //      strBiblioRecPathExclude   除开列表中的这些种路径, 才返回摘要内容, 否则仅仅返回种路径即可
                return app.GetBiblioSummary(
                        sessioninfo,
                        channel,
                        strItemBarcode,
                        strConfirmItemRecPath,
                        strBiblioRecPathExclude,
                        out strBiblioRecPath,
                        out strSummary);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetBiblioSummary() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
            finally
            {
                channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                EndSearch();
            }
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }


        // 借书
        // parameters:
        //      strReaderBarcode    读者证条码
        //      strItemBarcode  册条码号
        //      strConfirmItemRecPath  册记录路径。在册条码号重复的情况下，才需要使用这个参数，平时为null即可
        //      saBorrowedItemBarcode   同一读者先前已经借阅成功的册条码号集合。用于在返回的读者html中显示出特定的颜色而已。
        //      strStyle    操作风格。
        //                  "item"表示将返回册记录；"reader"表示将返回读者记录
        //                  "auto_renew"  表示如果当前处在已经借出状态，并且发起借书的又是同一人，自动当作续借请求进行操作
        //      strItemFormatList   规定strItemRecord参数所返回的数据格式
        //      item_records   返回册记录
        //      strReaderFormatList 规定strReaderRecord参数所返回的数据格式
        //      reader_records 返回读者记录
        //      aDupPath    如果发生条码号重复，这里返回了相关册记录的路径
        // 权限：无论工作人员还是读者，首先应具备borrow或renew权限。
        //      对于读者，还需要他进行的借阅(续借)操作是针对自己的，即strReaderBarcode必须和账户信息中的证条码号一致。
        //      也就是说，读者不允许替他人借阅(续借)图书，这样规定是为了防止读者捣乱。
        // 日志：
        //      要产生日志
        public LibraryServerResult Borrow(
            bool bRenew,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            bool bForce,
            string[] saBorrowedItemBarcode,
            string strStyle,
            string strItemFormatList,
            out string[] item_records,
            string strReaderFormatList,
            out string[] reader_records,
            string strBiblioFormatList,
            out string[] biblio_records,
            out BorrowInfo borrow_info,
            out string[] aDupPath,
            out string strOutputReaderBarcode)
        {
            item_records = null;
            reader_records = null;
            biblio_records = null;
            aDupPath = null;
            strOutputReaderBarcode = "";
            borrow_info = new BorrowInfo();

            LibraryServerResult result = this.PrepareEnvironment("Borrow", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // Thread.Sleep(5000);

                result = app.Borrow(
                    sessioninfo,
                    bRenew,
                    strReaderBarcode,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    bForce,
                    saBorrowedItemBarcode,
                    strStyle,

                    strItemFormatList,
                    out item_records,

                    strReaderFormatList,
                    out reader_records,

                    strBiblioFormatList,
                    out biblio_records,

                    out aDupPath,
                    out strOutputReaderBarcode,
                    out borrow_info);
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Borrow() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 还书
        // paramters:
        //      strAction   动作。有 return/lost/inventory/read/transfer
        //      strReaderBarcode   读者证条码号
        //      strItemBarcode  册条码号
        //      bForce  是否强制执行还书操作。用于某些配置参数和数据结构不正确的特殊情况
        //      strStyle   风格。"reader" 表示希望返回处理完后的读者记录
        //      strReaderFormat 指明strReaderRecord参数中所返回的读者记录格式。为"xml"或"html"
        //      strReaderFormat 读者记录
        // return:
        //      Result.Value    -1  出错 0 操作成功 1 操作成功，并且有值得留意的情况：如有超期情况；发现条码号重复；需要放入预约架
        // 日志：
        //      要产生日志
        public LibraryServerResult Return(
            string strAction,
            string strReaderBarcode,
            string strItemBarcode,
            string strComfirmItemRecPath,
            bool bForce,
            string strStyle,
            string strItemFormatList,
            out string[] item_records,
            string strReaderFormatList,
            out string[] reader_records,
            string strBiblioFormatList,
            out string[] biblio_records,
            out string[] aDupPath,
            out string strOutputReaderBarcode,
            out ReturnInfo return_info)
        {
            item_records = null;
            reader_records = null;
            biblio_records = null;
            aDupPath = null;
            strOutputReaderBarcode = "";
            return_info = new ReturnInfo();

            LibraryServerResult result = this.PrepareEnvironment("Return", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // Thread.Sleep(5000);

                return app.Return(sessioninfo,
                    strAction,
                    strReaderBarcode,
                    strItemBarcode,
                    strComfirmItemRecPath,
                    bForce,
                    strStyle,

                    strItemFormatList,
                    out item_records,

                    strReaderFormatList,
                    out reader_records,

                    strBiblioFormatList,
                    out biblio_records,

                    out aDupPath,
                    out strOutputReaderBarcode,
                    out return_info);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Return() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 预约
        // parameters:
        //      strItemBarcodeList  册条码号列表，逗号间隔
        // 权限：需要有reservation权限
        // 日志：
        //      要产生日志。等待编写。
        public LibraryServerResult Reservation(
            string strFunction,
            string strReaderBarcode,
            string strItemBarcodeList)
        {
            LibraryServerResult result = this.PrepareEnvironment("Reservation", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.Reservation(sessioninfo,
                    strFunction,
                    strReaderBarcode,
                    strItemBarcodeList);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Reservation() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 交违约金
        // parameters:
        // 权限：需要有amerce/amercemodifyprice/amerceundo/amercemodifycomment等权限
        // 日志：
        //      要产生日志
        // return:
        //      result.Value    0 成功；1 部分成功(result.ErrorInfo中有信息)
        public LibraryServerResult Amerce(
            string strFunction,
            string strReaderBarcode,
            AmerceItem[] amerce_items,
            out AmerceItem[] failed_items,  // 2011/6/27
            out string strReaderXml)
        {
            strReaderXml = "";
            failed_items = null;

            LibraryServerResult result = this.PrepareEnvironment("Amerce", true, true, true);
            if (result.Value == -1)
                return result;

            // 对读者身份的附加判断
            if (sessioninfo.UserType == "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "违约金操作被拒绝。作为读者不能进行此项操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            try
            {
                return app.Amerce(sessioninfo,
                    strFunction,
                    strReaderBarcode,
                    amerce_items,
                    out failed_items,
                    out strReaderXml);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Amerce() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        //
        // 获得期信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      lStart  返回从第几个开始
        //      lCount  总共返回几个。0和-1都表示全部返回(0是为了兼容旧API)
        //      strStyle    "onlygetpath"   仅返回每个路径(OldRecPath)
        //                  "getfirstxml"   是对onlygetpath的补充，仅获得第一个元素的XML记录，其余的依然只返回路径
        //                  "query:父记录+期号|..." 使用特定的检索途径和检索词。...部分表示检索词，例如 1|2005|1|，默认前方一致
        //      issueinfos 返回的期信息数组
        // 权限：需要有getissueinfo权限(兼容getissues权限)
        public LibraryServerResult GetIssues(
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,
            string strLang,
            out EntityInfo[] issueinfos)
        {
            issueinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("GetIssues", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("getissues,getissueinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得期信息 操作被拒绝。不具备order、getissueinfo或getissues权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.IssueItemDatabase.GetItems(sessioninfo,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    out issueinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetIssues() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置/保存期信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      issueinfos 要提交的的期信息数组
        // 权限：需要有setissueinfo权限(兼容setissues权限)
        // 日志：
        //      要产生日志
        public LibraryServerResult SetIssues(
            string strBiblioRecPath,
            EntityInfo[] issueinfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("SetIssues", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改期记录被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (StringUtil.IsInList("setissues,setissueinfo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "保存期信息 操作被拒绝。不具备setissueinfo或setissues权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }


                return app.IssueItemDatabase.SetItems(sessioninfo,
                    strBiblioRecPath,
                    issueinfos,
                    out errorinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetIssues() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // TODO: 改造，实体库，如果必要可以检索登录号字段
        // 获得一条册、期、订购、评注记录
        // parameters:
        //      strRefID    当 strDbType 为 item 时，strRefID 中是册条码号。其他情况，是 refid 字符串
        //      item_dom    [out] 返回记录的 XmlDocument 形态。注意有时候可能为 null (当 strXml 为空时)
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetItemXml(
            string strDbType,
            string strRefID,
            string strItemXml,
            ref string strIssueRecPath,
            ref byte[] issue_timestamp,
            ref string strBiblioDbName,
            ref string strOutputBiblioRecPath,
            ref LibraryServerResult result,
            out string strXml,
            out XmlDocument item_dom,
            out string strError)
        {
            strError = "";
            strXml = "";
            item_dom = null;
            int nRet = 0;

            ItemDatabase itemDatabase = null;
            if (strDbType == "issue")
                itemDatabase = app.IssueItemDatabase;
            else if (strDbType == "order")
                itemDatabase = app.OrderItemDatabase;
            else if (strDbType == "comment")
                itemDatabase = app.CommentItemDatabase;
            else if (strDbType == "item")
            {
                itemDatabase = null;
            }
            else
            {
                strError = "不支持的数据库类型 '" + strDbType + "'";
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 前端提供临时记录
            if (strRefID[0] == '<')
            {
                strXml = strRefID;
                // strOutputPath = "?";
                // TODO: 数据库名需要从前端发来的XML记录中获取?

                item_dom = new XmlDocument();
                try
                {
                    item_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "评注记录 XML 装载到 DOM 时出错:" + ex.Message;
                    goto ERROR1;
                }

                strIssueRecPath = DomUtil.GetElementText(item_dom.DocumentElement, "_recPath");

                if (string.IsNullOrEmpty(strIssueRecPath) == false)
                {
                    nRet = GetParentPath(app,
            item_dom,
            strIssueRecPath,
            out strBiblioDbName,
            out strOutputBiblioRecPath,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                    strIssueRecPath = "?";

                return 1;
            }

            bool bProcessed = false;

            // 命令状态
            if (strRefID[0] == '@')
            {

                // TODO: refid

                // 获得期记录，通过期记录路径

#if NO
                string strLead = "@path:";
                if (strRefID.Length <= strLead.Length)
                {
                    strError = "错误的检索词格式: '" + strRefID + "'";
                    goto ERROR1;
                }
                string strPart = strRefID.Substring(0, strLead.Length);

                if (strPart != strLead)
                {
                    strError = "不支持的检索词格式: '" + strRefID + "'。目前仅支持'@path:'引导的检索词";
                    goto ERROR1;
                }
                    strIssueRecPath = strRefID.Substring(strLead.Length);
#endif
                if (StringUtil.HasHead(strRefID, "@path:") == true)
                {
                    strIssueRecPath = strRefID.Substring("@path:".Length);

                    // 继续分离出(方向)命令部分
                    string strCommand = "";
                    nRet = strIssueRecPath.IndexOf("$");
                    if (nRet != -1)
                    {
                        strCommand = strIssueRecPath.Substring(nRet + 1);
                        strIssueRecPath = strIssueRecPath.Substring(0, nRet);
                    }

                    string strCurrentIssueDbName = ResPath.GetDbName(strIssueRecPath);

                    string strCurrentDbType = app.GetDbType(strCurrentIssueDbName);
                    if (strCurrentDbType != strDbType)
                    {
                        strError = "记录路径 '" + strIssueRecPath + "' 中的数据库名 '" + strCurrentIssueDbName + "' 不是类型 " + strDbType + "，因此拒绝操作。";
                        goto ERROR1;
                    }
#if NO
                // 需要检查一下数据库名是否在允许的期库名之列

                if (app.IsIssueDbName(strCurrentIssueDbName) == false)
                {
                    strError = "期记录路径 '" + strIssueRecPath + "' 中的数据库名 '" + strCurrentIssueDbName + "' 不在配置的期库名之列，因此拒绝操作。";
                    goto ERROR1;
                }
#endif


                    if (string.IsNullOrEmpty(strItemXml) == false)
                    {
                        strXml = strItemXml;
                        result.ErrorInfo = "";
                        result.Value = 1;
#if NO
                        // 还需要得到 strOutputBiblioRecPath

                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = app.GetBiblioDbNameByChildDbName(ResPath.GetDbName(strIssueRecPath),
                out strBiblioDbName,
                out strError);
                        if (nRet == -1 || nRet == 0)
                        {
                            strError = "根据子记录路径 '' 获得书目库名时出错: " +strError;
                            goto ERROR1;
                        }
                        strOutputBiblioRecPath = strBiblioDbName + "/?";
                        return 1;
#endif
                        goto GET_DOM_AND_BIBLIORECPATH;
                    }

                    string strMetaData = "";
                    string strTempOutputPath = "";

                    string strStyle = "content,data,metadata,timestamp,outputpath";

                    // 为了便于处理对象资源
                    strStyle += ",withresmetadata";

                    if (String.IsNullOrEmpty(strCommand) == false
                        && (strCommand == "prev" || strCommand == "next"))
                    {
                        strStyle += "," + strCommand;
                    }

                    long lRet = channel.GetRes(strIssueRecPath,
    strStyle,
    out strXml,
    out strMetaData,
    out issue_timestamp,
    out strTempOutputPath,
    out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            result.Value = 0;
                            if (strCommand == "prev")
                                result.ErrorInfo = "到头";
                            else if (strCommand == "next")
                                result.ErrorInfo = "到尾";
                            else
                                result.ErrorInfo = "没有找到";
                            result.ErrorCode = ErrorCode.NotFound;
                            // return result;
                            return 0;
                        }
                        goto ERROR1;
                    }

                    strIssueRecPath = strTempOutputPath;

                    //
                    result.ErrorInfo = "";
                    result.Value = 1;

                    bProcessed = true;
                }
                else if (StringUtil.HasHead(strRefID, "@refID:", true) == true)
                {
#if NO
                    string strTempRefID = strRefID.Substring("@refid:".Length);

                    string strTempOutputPath = "";                            // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
                    nRet = app.GetCommentRecXml(
                        sessioninfo.Channels,
                        strTempRefID,
            out strXml,
            out strTempOutputPath,
            out issue_timestamp,
            out strError);
                    if (nRet == -1)
                    {
                        strError = "用refid '" + strRefID + "' 检索评注记录时出错: " + strError;
                        goto ERROR1;
                    }
#endif
                    if (itemDatabase != null)
                        strRefID = strRefID.Substring("@refid:".Length);
                }
                else
                {
                    strError = "不支持的检索词格式: '" + strRefID + "'。目前仅支持'@path:'和'@refid:'引导的检索词";
                    goto ERROR1;
                }

                result.ErrorInfo = "";
                result.Value = 1;
            }

            if (bProcessed == false)
            {
                if (string.IsNullOrEmpty(strItemXml) == false)
                {
                    strXml = strItemXml;
                    result.ErrorInfo = "";
                    result.Value = 1;

                    strIssueRecPath = strRefID;
                    goto GET_DOM_AND_BIBLIORECPATH;
                }

                List<string> PathList = null;

                if (itemDatabase == null)
                {
                    // *** 针对册条码号或者 @xxx: 途径进行检索

                    string strOwnerInstitution = null;
                    if (strRefID.StartsWith("@") == false)
                    {
                        // 分割 OI 和 PII
                        if (strRefID.IndexOf(".") != -1)
                        {
                            var parts = StringUtil.ParseTwoPart(strRefID, ".");
                            strOwnerInstitution = parts[0];
                            strRefID = parts[1];
                        }
                    }

                    // 获得册记录
                    // 本函数可获得超过1条以上的路径
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = app.GetItemRecXml(
                            // sessioninfo.Channels,
                            channel,
                            // strDbType == "item" ? strRefID : "@refid:" + strRefID,
                            strRefID,
                            "withresmetadata",
                            out strXml,
                            100,
                            out PathList,
                            out issue_timestamp,
                            out strError);
                    // 2018/10/15
                    // 如果需要，从登录号等辅助途径进行检索
                    if (nRet == 0 && strRefID.StartsWith("@") == false)
                    {
                        foreach (string strFrom in app.ItemAdditionalFroms)
                        {
                            // return:
                            //      -1  error
                            //      0   not found
                            //      1   命中1条
                            //      >1  命中多于1条
                            nRet = app.GetOneItemRec(
                                channel,
                                "item",
                                strRefID,
                                strFrom,
                                "xml,timestamp,withresmetadata",
                                out strXml,
                                100,
                                out PathList,
                                out issue_timestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                // text-level: 内部错误
                                strError = "用" + strFrom + " '" + strRefID + "' 从途径 '" + strFrom + "' 读入册记录时发生错误: " + strError;
                                break;
                            }
                            if (nRet == 0)
                                continue;
                            if (nRet > 1)
                            {
                                //result.Value = -1;
                                //result.ErrorInfo = "用" + strFrom + " '" + strItemBarcode + "' 检索册记录命中 " + nRet.ToString() + " 条，因此无法用" + strFrom + "来进行借还操作。请改用册条码号来进行借还操作。";
                                //result.ErrorCode = ErrorCode.ItemBarcodeDup;
                                break;
                            }

                            // strItemFrom = strFrom;
                            break;
                        }
                    }

                    // 用 OI 判断这一条册记录是否符合要求
                    if (strOwnerInstitution != null
                        && string.IsNullOrEmpty(strXml) == false)
                    {
                        // return:
                        //      -1  出错
                        //      0   没有通过较验
                        //      1   通过了较验
                        int nRet0 = app.VerifyItemOI(
            PathList != null && PathList.Count > 0 ? PathList[0] : "",
            strXml,
            strOwnerInstitution,
            out strError);
                        if (nRet0 == 0)
                        {
                            result.Value = 0;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.NotFound;
                            return 0;
                        }
                        if (nRet0 != 1)
                            goto ERROR1;
                    }
                }
                else
                {

                    nRet = itemDatabase.BuildLocateParam(
                        // strBiblioRecPath,
                        strRefID,
                        out List<string> locateParam,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nRet = itemDatabase.GetItemRecXml(
                            // sessioninfo.Channels,
                            channel,
                            locateParam,
                            "withresmetadata",
                            out strXml,
                            100,
                            out PathList,
                            out issue_timestamp,
                            out strError);
                }

                if (nRet == 0)
                {
                    result.Value = 0;
                    result.ErrorInfo = "没有找到";
                    result.ErrorCode = ErrorCode.NotFound;
                    // return result;
                    return 0;
                }

                if (nRet == -1)
                    goto ERROR1;

                strIssueRecPath = StringUtil.MakePathList(PathList);

                result.ErrorInfo = strError;
                result.Value = nRet;    // 可能会多于1条
            }

        GET_DOM_AND_BIBLIORECPATH:

            if (string.IsNullOrEmpty(strXml) == false)   // 是否有节省运算的办法?
            {
                Debug.Assert(item_dom == null, "item_dom == null 未满足");

                // 从期记录<parent>元素中取得书目记录的id，然后拼装成书目记录路径放入strOutputBiblioRecPath
                item_dom = new XmlDocument();
                try
                {
                    item_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "记录 " + strIssueRecPath + " 的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }


#if NO
                    // 根据期库名, 找到对应的书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = app.GetBiblioDbNameByIssueDbName(strCurrentIssueDbName,
                        out strBiblioDbName,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;
#endif
                string strCurrentIssueDbName = ResPath.GetDbName(strIssueRecPath);

                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = app.GetBiblioDbNameByChildDbName(strCurrentIssueDbName,
        out strBiblioDbName,
        out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                string strParentID = DomUtil.GetElementText(item_dom.DocumentElement,
                    "parent");
                if (String.IsNullOrEmpty(strParentID) == true)
                {
#if NO
                    strError = "记录 " + strIssueRecPath + " 中没有<parent>元素值，因此无法定位其从属的书目记录";
                    goto ERROR1;
#endif
                    strParentID = "?";
                }
                string strBiblioRecPath = strBiblioDbName + "/" + strParentID;
                strOutputBiblioRecPath = strBiblioRecPath;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // TODO: 建议主体移动到ItemDatabase中，可以节省多种类的代码
        // 获得期信息
        // parameters:
        //      strRefID  参考ID。特殊情况下，可以使用"@path:"引导的期记录路径(只需要库名和id两个部分)作为检索入口。在@path引导下，路径后面还可以跟随 "$prev"或"$next"表示方向
        //      strBiblioRecPath    指定书目记录路径
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据。
        // return:
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有getissueinfo权限
        public LibraryServerResult GetIssueInfo(
            string strRefID,
            // string strBiblioRecPath,
            string strItemXml,  // 前端提供给服务器的记录内容。例如，需要模拟创建检索点，就需要前端提供记录内容
            string strResultType,
            out string strResult,
            out string strIssueRecPath,
            out byte[] issue_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath)
        {
            strResult = "";
            strBiblio = "";
            strIssueRecPath = "";
            issue_timestamp = null;
            strOutputBiblioRecPath = "";

            LibraryServerResult result = this.PrepareEnvironment("GetIssueInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("getissueinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取期信息被拒绝。不具备order或getissueinfo权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;
                long lRet = 0;

                string strXml = "";
                string strError = "";
                // string strOutputPath = "";

                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "strPublishTime参数不能为空";
                    goto ERROR1;
                }

                string strBiblioDbName = "";
                // string strParentID = "";
                XmlDocument issue_dom = null;

                // 特殊用法 @barcode-list: 获得册记录路径列表
                if (StringUtil.HasHead(strRefID, "@item-refid-list:") == true
                    && strResultType == "get-path-list")
                {
                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    nRet = app.GetItemRecPathList(
                        // sessioninfo.Channels,
                        channel,
                        "issue",
                        "册参考ID",
                        strRefID.Substring("@item-refid-list:".Length),
                        true,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    result.ErrorInfo = "";
                    result.Value = 1;
                    return result;
                }

                // 获得一条册、期、订购、评注记录
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetItemXml(
                    "issue",
                    strRefID,
                    strItemXml,
                    ref strIssueRecPath,
                    ref issue_timestamp,
                    ref strBiblioDbName,
                    ref strOutputBiblioRecPath,
                    ref result,
                    out strXml,
                    out issue_dom,
                    out strError);
                if (nRet == 0)
                    return result;
                if (nRet == -1)
                    goto ERROR1;

#if NO
                if (string.IsNullOrEmpty(strItemXml) == false)
                {
                    strXml = strItemXml;
                    result.ErrorInfo = "";
                    result.Value = 1;
                }
                else
                {
                    // 命令状态
                    if (strRefID[0] == '@')
                    {

                        // TODO: refid

                        // 获得期记录，通过期记录路径

                        string strLead = "@path:";
                        if (strRefID.Length <= strLead.Length)
                        {
                            strError = "错误的检索词格式: '" + strRefID + "'";
                            goto ERROR1;
                        }
                        string strPart = strRefID.Substring(0, strLead.Length);

                        if (strPart != strLead)
                        {
                            strError = "不支持的检索词格式: '" + strRefID + "'。目前仅支持'@path:'引导的检索词";
                            goto ERROR1;
                        }

                        strIssueRecPath = strRefID.Substring(strLead.Length);

                        // 继续分离出(方向)命令部分
                        string strCommand = "";
                        nRet = strIssueRecPath.IndexOf("$");
                        if (nRet != -1)
                        {
                            strCommand = strIssueRecPath.Substring(nRet + 1);
                            strIssueRecPath = strIssueRecPath.Substring(0, nRet);
                        }

                        string strCurrentIssueDbName = ResPath.GetDbName(strIssueRecPath);
                        // 需要检查一下数据库名是否在允许的期库名之列
                        if (app.IsIssueDbName(strCurrentIssueDbName) == false)
                        {
                            strError = "期记录路径 '" + strIssueRecPath + "' 中的数据库名 '" + strCurrentIssueDbName + "' 不在配置的期库名之列，因此拒绝操作。";
                            goto ERROR1;
                        }

                        string strMetaData = "";
                        string strTempOutputPath = "";

                        RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                        if (channel == null)
                        {
                            strError = "get channel error";
                            goto ERROR1;
                        }

                        string strStyle = "content,data,metadata,timestamp,outputpath";

                        // 为了便于处理对象资源
                        strStyle += ",withresmetadata";

                        if (String.IsNullOrEmpty(strCommand) == false
                            && (strCommand == "prev" || strCommand == "next"))
                        {
                            strStyle += "," + strCommand;
                        }

                        /*
                        lRet = channel.GetRes(strIssueRecPath,
                            out strXml,
                            out strMetaData,
                            out issue_timestamp,
                            out strTempOutputPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                         * */
                        lRet = channel.GetRes(strIssueRecPath,
        strStyle,
        out strXml,
        out strMetaData,
        out issue_timestamp,
        out strTempOutputPath,
        out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            {
                                result.Value = 0;
                                if (strCommand == "prev")
                                    result.ErrorInfo = "到头";
                                else if (strCommand == "next")
                                    result.ErrorInfo = "到尾";
                                else
                                    result.ErrorInfo = "没有找到";
                                result.ErrorCode = ErrorCode.NotFound;
                                return result;
                            }
                            goto ERROR1;
                        }

                        strIssueRecPath = strTempOutputPath;


                        //

                        if (true)   // 是否有节省运算的办法?
                        {

                            // 从期记录<parent>元素中取得书目记录的id，然后拼装成书目记录路径放入strOutputBiblioRecPath
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(strXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "记录 " + strIssueRecPath + " 的XML装入DOM时出错: " + ex.Message;
                                goto ERROR1;
                            }

                            // 根据期库名, 找到对应的书目库名
                            // return:
                            //      -1  出错
                            //      0   没有找到
                            //      1   找到
                            nRet = app.GetBiblioDbNameByIssueDbName(strCurrentIssueDbName,
                                out strBiblioDbName,
                                out strError);
                            if (nRet == -1 || nRet == 0)
                                goto ERROR1;

                            strParentID = DomUtil.GetElementText(dom.DocumentElement,
                                "parent");
                            if (String.IsNullOrEmpty(strParentID) == true)
                            {
                                strError = "期记录 " + strIssueRecPath + " 中没有<parent>元素值，因此无法定位其从属的书目记录";
                                goto ERROR1;
                            }
                            string strBiblioRecPath = strBiblioDbName + "/" + strParentID;
                            strOutputBiblioRecPath = strBiblioRecPath;
                        }

                        //

                        result.ErrorInfo = "";
                        result.Value = 1;
                    }
                    else
                    {
#if NO
                    //
                    strOutputBiblioRecPath = strBiblioRecPath;

                    strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);

                    // 根据书目库名, 找到对应的期库名
                    // return:
                    //      -1  出错
                    //      0   没有找到(书目库)
                    //      1   找到
                    nRet = app.GetIssueDbName(strBiblioDbName,
                        out strIssueDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                        goto ERROR1;
                    }
                    strParentID = ResPath.GetRecordId(strBiblioRecPath);

                    //
                    List<string> locateParam = new List<string>();
                    //locateParam.Add(strIssueDbName);
                    //locateParam.Add(strParentID);
                    locateParam.Add(strPublishTime);
#endif
                        List<string> locateParam = null;

                        nRet = app.IssueItemDatabase.BuildLocateParam(
                            // strBiblioRecPath,
                            strRefID,
                            out locateParam,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        List<string> PathList = null;

                        // byte[] timestamp = null;
                        // 获得册记录
                        // 本函数可获得超过1条以上的路径
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        /*
                        nRet = app.GetIssueRecXml(
                                sessioninfo.Channels,
                                strIssueDbName,
                                strParentID,
                                strPublishTime,
                                out strXml,
                                100,
                                out PathList,
                                out issue_timestamp,
                                out strError);
                         * */

                        nRet = app.IssueItemDatabase.GetItemRecXml(
                                sessioninfo.Channels,
                                locateParam,
                                "withresmetadata",
                                out strXml,
                                100,
                                out PathList,
                                out issue_timestamp,
                                out strError);

                        if (nRet == 0)
                        {
                            result.Value = 0;
                            result.ErrorInfo = "没有找到";
                            result.ErrorCode = ErrorCode.NotFound;
                            return result;
                        }

                        if (nRet == -1)
                            goto ERROR1;

                        /*
                        Debug.Assert(PathList != null, "");
                        // 构造路径字符串。逗号间隔
                        string[] paths = new string[PathList.Count];
                        PathList.CopyTo(paths);

                        strIssueRecPath = String.Join(",", paths);
                         * */
                        strIssueRecPath = StringUtil.MakePathList(PathList);

                        result.ErrorInfo = strError;
                        result.Value = nRet;    // 可能会多于1条
                    }

                }
#endif

                // 若需要同时取得种记录
                if (String.IsNullOrEmpty(strBiblioType) == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(strOutputBiblioRecPath) == false, "");
                    Debug.Assert(issue_dom != null, "");

#if NO
                    string strBiblioRecID = "";

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "期记录XML装载到DOM出错:" + ex.Message;
                        goto ERROR1;
                    }

                    strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
                    if (String.IsNullOrEmpty(strBiblioRecID) == true)
                    {
                        strError = "期记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                        goto ERROR1;
                    }

                    strOutputBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
#endif

                    string strBiblioXml = "";

                    if (String.Compare(strBiblioType, "recpath", true) == 0)
                    {
                        // 如果仅仅需要获得书目记录recpath，则不需要获得书目记录
                        goto DOISSUE;
                    }

                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strTempOutputPath = "";
                    lRet = channel.GetRes(strOutputBiblioRecPath,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得种记录 '" + strOutputBiblioRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }

                    // 如果只需要种记录的XML格式
                    if (String.Compare(strBiblioType, "xml", true) == 0)
                    {
                        strBiblio = strBiblioXml;
                        goto DOISSUE;
                    }


                    // 需要从内核映射过来文件
                    string strLocalPath = "";

                    if (String.Compare(strBiblioType, "html", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else if (String.Compare(strBiblioType, "text", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio_text.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else
                    {
                        strError = "不能识别的strBiblioType类型 '" + strBiblioType + "'";
                        goto ERROR1;
                    }

                    if (nRet == -1)
                        goto ERROR1;

                    // 将种记录数据从XML格式转换为HTML格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = app.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                            strBiblioXml,
                                    null,
                            strOutputBiblioRecPath,
                            out strBiblio,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strBiblio = "";
                }

            DOISSUE:
                // 取得期信息
                if (String.IsNullOrEmpty(strResultType) == true
                    || String.Compare(strResultType, "recpath", true) == 0)
                {
                    strResult = ""; // 不返回任何结果
                }
                else if (String.Compare(strResultType, "xml", true) == 0)
                {
                    strResult = strXml;
                }
                else if (String.Compare(strResultType, "html", true) == 0)
                {
                    // 将期记录数据从XML格式转换为HTML格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\issuexml2html.cs",
                        app.CfgDir + "\\issuexml2html.cs.ref",
                        strXml,
                        strIssueRecPath,    // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (String.Compare(strResultType, "text", true) == 0)
                {
                    // 将期记录数据从XML格式转换为text格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\issuexml2text.cs",
                        app.CfgDir + "\\issuexml2text.cs.ref",
                        strXml,
                        strIssueRecPath,    // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                // 模拟创建检索点
                else if (String.Compare(strResultType, "keys", true) == 0)
                {
                    nRet = app.GetKeys(sessioninfo,
                        strIssueRecPath,
                        strItemXml,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strError = "未知的期记录结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetIssueInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // *** 此API已经废止 ***
        // 对(期)出版日期进行查重
        // 2007/10/19 新增API
        // parameters:
        //          strPublishTime  实际上要在这里使用参考ID。2012/4/6
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有searchissuedup权限
        public LibraryServerResult SearchIssueDup(string strPublishTime,
            string strBiblioRecPath,
            int nMax,
            out string[] paths)
        {
            paths = null;

            LibraryServerResult result = this.PrepareEnvironment("SearchIssueDup", true, true);
            if (result.Value == -1)
                return result;

            try
            {

                // 权限字符串
                if (StringUtil.IsInList("searchissue,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "对期记录的参考ID查重被拒绝。不具备order或searchissue权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;
                string strError = "";

                List<string> locateParam = null;

                nRet = app.IssueItemDatabase.BuildLocateParam(
                    // strBiblioRecPath,
                    strPublishTime,
                    out locateParam,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    return app.IssueItemDatabase.SearchItemDup(
                        // sessioninfo.Channels,
                        channel,
                        locateParam,
                        nMax,
                        out paths);
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchIssueDup() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 检索期信息
        // parameters:
        //      strQueryWord    检索词
        //      strFrom 检索途径
        //      strMathStyle    匹配方式 exact left right middle
        // 权限: 
        //      需要 searchissue 权限
        // return:
        //      result.Value    命中结果总数。如果为-1，则表示检索出错
        public LibraryServerResult SearchIssue(
            string strIssueDbName,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchIssue", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchissue", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索期信息被拒绝。不具备searchissue权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                List<string> dbnames = new List<string>();

                if (String.IsNullOrEmpty(strIssueDbName) == true
                    || strIssueDbName == "<全部>"
                    || strIssueDbName.ToLower() == "<all>"
                    || strIssueDbName == "<全部期刊>"
                    || strIssueDbName.ToLower() == "<all series>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strDbName = app.ItemDbs[i].IssueDbName;
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;
                        dbnames.Add(strDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何期库";
                        goto ERROR1;
                    }
                }
                else if (strIssueDbName == "<全部图书>"
                    || strIssueDbName.ToLower() == "<all book>")
                {
                    strError = "SearchIssue() API中不能使用库名 '" + strIssueDbName + "'";
                    goto ERROR1;
                }
                else
                {
                    string[] splitted = strIssueDbName.Split(new char[] { ',' });
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        string strDbName = splitted[i];
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        if (app.IsIssueDbName(strDbName) == false)
                        {
                            strError = "库名 '" + strDbName + "' 不是合法的期库名";
                            goto ERROR1;
                        }

                        dbnames.Add(strDbName);
                    }

                }

                bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

                // 构造检索式
                string strQueryXml = "";
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    string strRelation = "=";
                    string strDataType = "string";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOf("-") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else if (String.IsNullOrEmpty(strQueryWord) == false)
                        {
                            strDataType = "number";
                        }
                    }

                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)    // 2007/9/14 
                        + "'><item>"
                        + (bDesc == true ? "<order>DESC</order>" : "")
                        + "<word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                    if (i > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                }

                if (dbnames.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                return result;

            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchIssue() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        //

        // 获得册信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      lStart  返回从第几个开始    2009/6/7 add
        //      lCount  总共返回几个。0和-1都表示全部返回(0是为了兼容旧API)
        //      strStyle    "opac" 把实体记录按照OPAC要求进行加工，增补一些元素
        //                  "onlygetpath"   仅返回每个路径
        //                  "getfirstxml"   是对onlygetpath的补充，仅获得第一个元素的XML记录，其余的依然只返回路径
        //                  "getotherlibraryitem"    返回全部分馆的记录的详情。这个用法只对分馆用户有用。因为分馆用户如果不用这个style，则只获得属于自己管辖分馆的册记录的详情
        //      entityinfos 返回的实体信息数组
        //      Result.Value    -1出错 0没有找到 其他 总的实体记录的个数(本次返回的，可以通过entities.Count得到)
        // 权限：需要有getiteminfo或order权限(兼容getentities权限)
        public LibraryServerResult GetEntities(
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,    // 2011/1/21
            string strLang,     // 2011/1/21
            out EntityInfo[] entityinfos)
        {
            entityinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("GetEntities", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetEntities(sessioninfo,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,    // 2011/1/21
                    strLang,     // 2011/1/21
                    out entityinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetEntities() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置/保存册信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      entityinfos 要提交的的实体信息数组
        // 权限：需要有setiteminfo权限(兼容setentities权限)
        // 日志：
        //      要产生日志
        public LibraryServerResult SetEntities(
            string strBiblioRecPath,
            EntityInfo[] entityinfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("SetEntities", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.SetEntities(sessioninfo,
                    strBiblioRecPath,
                    entityinfos,
                    out errorinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetEntities() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        /// *** 订购相关功能
        /// 

        // 获得订购信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      lStart  返回从第几个开始
        //      lCount  总共返回几个。0和-1都表示全部返回(0是为了兼容旧API)
        //      strStyle    "onlygetpath"   仅返回每个路径(OldRecPath)
        //                  "getfirstxml"   是对onlygetpath的补充，仅获得第一个元素的XML记录，其余的依然只返回路径
        //      orderinfos 返回的订购信息数组
        // 权限：需要有getorderinfo权限(兼容以前的getorders权限)
        public LibraryServerResult GetOrders(
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,
            string strLang,
            out EntityInfo[] orderinfos)
        {
            orderinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("GetOrders", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("getorders,getorderinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得订购信息 操作被拒绝。不具备order、getorderinfo或getorders权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.OrderItemDatabase.GetItems(sessioninfo,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    out orderinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetOrders() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置/保存订购信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      orderinfos 要提交的的订购信息数组
        // 权限：需要有setorderinfo权限(兼容setorders权限)
        // 日志：
        //      要产生日志
        public LibraryServerResult SetOrders(
            string strBiblioRecPath,
            EntityInfo[] orderinfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("SetOrders", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改订购记录的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (StringUtil.IsInList("setorders,setorderinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "保存订购信息 操作被拒绝。不具备order、setorderinfo或setorders权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.OrderItemDatabase.SetItems(sessioninfo,
                    strBiblioRecPath,
                    orderinfos,
                    out errorinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetOrders() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // TODO: 建议主体移动到ItemDatabase中，可以节省多种类的代码
        // 获得订购信息
        // parameters:
        //      strRefID  参考ID。特殊情况下，可以使用"@path:"引导的订购记录路径(只需要库名和id两个部分)作为检索入口。在@path引导下，路径后面还可以跟随 "$prev"或"$next"表示方向
        //      strBiblioRecPath    指定书目记录路径
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据。
        //      strOutputBiblioRecPath  输出的书目记录路径。当strIndex的第一字符为'@'时，strBiblioRecPath必须为空，函数返回后，strOutputBiblioRecPath中会包含从属的书目记录路径
        // return:
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有getorderinfo权限
        public LibraryServerResult GetOrderInfo(
            string strRefID,
            // string strBiblioRecPath,
            string strItemXml,  // 前端提供给服务器的记录内容。例如，需要模拟创建检索点，就需要前端提供记录内容
            string strResultType,
            out string strResult,
            out string strOrderRecPath,
            out byte[] order_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath)
        {
            strResult = "";
            strBiblio = "";
            strOrderRecPath = "";
            order_timestamp = null;
            strOutputBiblioRecPath = "";

            LibraryServerResult result = this.PrepareEnvironment("GetOrderInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("getorderinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取订购信息被拒绝。不具备order或getorderinfo权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }


                int nRet = 0;
                long lRet = 0;

                string strXml = "";
                string strError = "";

                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "strIndex参数不能为空";
                    goto ERROR1;
                }

                string strBiblioDbName = "";
                //string strOrderDbName = "";
                //string strParentID = "";
                XmlDocument order_dom = null;

                // 特殊用法 @barcode-list: 获得册记录路径列表
                if (StringUtil.HasHead(strRefID, "@item-refid-list:") == true
                    && strResultType == "get-path-list")
                {
                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    nRet = app.GetItemRecPathList(
                        // sessioninfo.Channels,
                        channel,
                        "order",
                        "册参考ID",
                        strRefID.Substring("@item-refid-list:".Length),
                        true,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    result.ErrorInfo = "";
                    result.Value = 1;
                    return result;
                }

                // 获得一条册、期、订购、评注记录
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetItemXml(
                    "order",
                    strRefID,
                    strItemXml,
                    ref strOrderRecPath,
                    ref order_timestamp,
                    ref strBiblioDbName,
                    ref strOutputBiblioRecPath,
                    ref result,
                    out strXml,
                    out order_dom,
                    out strError);
                if (nRet == 0)
                    return result;
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // 命令状态
                if (strRefID[0] == '@')
                {
#if NO
                    if (String.IsNullOrEmpty(strBiblioRecPath) == false)
                    {
                        strError = "当strIndex参数为'@'引导的命令形态时，strBiblioRecPath参数必须为空";
                        goto ERROR1;
                    }
#endif

                    // TODO: "@refID:";

                    // 获得订购记录，通过订购记录路径

                    string strLead = "@path:";
                    if (strRefID.Length <= strLead.Length)
                    {
                        strError = "错误的检索词格式: '" + strRefID + "'";
                        goto ERROR1;
                    }
                    string strPart = strRefID.Substring(0, strLead.Length);

                    if (strPart != strLead)
                    {
                        strError = "不支持的检索词格式: '" + strRefID + "'。目前仅支持'@path:'引导的检索词";
                        goto ERROR1;
                    }

                    strOrderRecPath = strRefID.Substring(strLead.Length);

                    // 继续分离出(方向)命令部分
                    string strCommand = "";
                    nRet = strOrderRecPath.IndexOf("$");
                    if (nRet != -1)
                    {
                        strCommand = strOrderRecPath.Substring(nRet + 1);
                        strOrderRecPath = strOrderRecPath.Substring(0, nRet);
                    }


                    strOrderDbName = ResPath.GetDbName(strOrderRecPath);
                    // 需要检查一下数据库名是否在允许的订购库名之列
                    if (app.IsOrderDbName(strOrderDbName) == false)
                    {
                        strError = "订购记录路径 '" + strOrderRecPath + "' 中的数据库名 '" + strOrderDbName + "' 不在配置的订购库名之列，因此拒绝操作。";
                        goto ERROR1;
                    }

                    string strMetaData = "";
                    string strTempOutputPath = "";

                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }

                    string strStyle = "content,data,metadata,timestamp,outputpath";

                    // 为了便于处理对象资源
                    strStyle += ",withresmetadata";

                    if (String.IsNullOrEmpty(strCommand) == false
                        && (strCommand == "prev" || strCommand == "next"))
                    {
                        strStyle += "," + strCommand;
                    }

                    /*
                    lRet = channel.GetRes(strOrderRecPath,
                        out strXml,
                        out strMetaData,
                        out order_timestamp,
                        out strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;
                     * * */
                    lRet = channel.GetRes(strOrderRecPath,
    strStyle,
    out strXml,
    out strMetaData,
    out order_timestamp,
    out strTempOutputPath,
    out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            result.Value = 0;
                            if (strCommand == "prev")
                                result.ErrorInfo = "到头";
                            else if (strCommand == "next")
                                result.ErrorInfo = "到尾";
                            else
                                result.ErrorInfo = "没有找到";
                            result.ErrorCode = ErrorCode.NotFound;
                            return result;
                        }
                        goto ERROR1;
                    }

                    strOrderRecPath = strTempOutputPath;

                    // 从订购记录<parent>元素中取得书目记录的id，然后拼装成书目记录路径放入strOutputBiblioRecPath
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 " + strOrderRecPath + " 的XML装入DOM时出错: " + ex.Message;
                        goto ERROR1;
                    }

                    // 根据订购库名, 找到对应的书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = app.GetBiblioDbNameByOrderDbName(strOrderDbName,
                        out strBiblioDbName,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;

                    strParentID = DomUtil.GetElementText(dom.DocumentElement,
                        "parent");
                    if (String.IsNullOrEmpty(strParentID) == true)
                    {
                        strError = "订购记录 " + strOrderRecPath + " 中没有<parent>元素值，因此无法定位其从属的书目记录";
                        goto ERROR1;
                    }
                    string strBiblioRecPath = strBiblioDbName + "/" + strParentID;
                    strOutputBiblioRecPath = strBiblioRecPath;

                    result.ErrorInfo = "";
                    result.Value = 1;
                }
                else
                {
#if NO
                    strOutputBiblioRecPath = strBiblioRecPath;

                    strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                    // 根据书目库名, 找到对应的订购名
                    // return:
                    //      -1  出错
                    //      0   没有找到(书目库)
                    //      1   找到
                    nRet = app.GetOrderDbName(strBiblioDbName,
                        out strOrderDbName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                        goto ERROR1;
                    }
                    strParentID = ResPath.GetRecordId(strBiblioRecPath);



                    List<string> locateParam = new List<string>();
                    // locateParam.Add(strOrderDbName);
                    // locateParam.Add(strParentID);
                    locateParam.Add(strIndex);
#endif              
                    List<string> PathList = null;

                    List<string> locateParam = null;

                    nRet = app.OrderItemDatabase.BuildLocateParam(
                        // strBiblioRecPath,
                        strRefID,
                        out locateParam,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // byte[] timestamp = null;
                    // 获得册记录
                    // 本函数可获得超过1条以上的路径
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = app.OrderItemDatabase.GetItemRecXml(
                            sessioninfo.Channels,
                            locateParam,
                            "withresmetadata",
                            out strXml,
                            100,
                            out PathList,
                            out order_timestamp,
                            out strError);

                    if (nRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "没有找到";
                        result.ErrorCode = ErrorCode.NotFound;
                        return result;
                    }

                    if (nRet == -1)
                        goto ERROR1;

                    /*
                    Debug.Assert(PathList != null, "");
                    // 构造路径字符串。逗号间隔
                    string[] paths = new string[PathList.Count];
                    PathList.CopyTo(paths);

                    strOrderRecPath = String.Join(",", paths);
                     * */
                    strOrderRecPath = StringUtil.MakePathList(PathList);

                    result.ErrorInfo = strError;
                    result.Value = nRet;    // 可能会多于1条
                }

#endif

                // 若需要同时取得种记录
                if (String.IsNullOrEmpty(strBiblioType) == false)
                {
                    Debug.Assert(string.IsNullOrEmpty(strOutputBiblioRecPath) == false, "");
                    Debug.Assert(order_dom != null, "");

#if NO
                    string strBiblioRecID = "";

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "订购记录XML装载到DOM出错:" + ex.Message;
                        goto ERROR1;
                    }

                    strBiblioRecID = DomUtil.GetElementText(dom.DocumentElement, "parent"); //
                    if (String.IsNullOrEmpty(strBiblioRecID) == true)
                    {
                        strError = "订购记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                        goto ERROR1;
                    }

                    strOutputBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
#endif

                    string strBiblioXml = "";

                    if (String.Compare(strBiblioType, "recpath", true) == 0)
                    {
                        // 如果仅仅需要获得书目记录recpath，则不需要获得书目记录
                        goto DOORDER;
                    }

                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strTempOutputPath = "";
                    lRet = channel.GetRes(strOutputBiblioRecPath,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得种记录 '" + strOutputBiblioRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }

                    // 如果只需要种记录的XML格式
                    if (String.Compare(strBiblioType, "xml", true) == 0)
                    {
                        strBiblio = strBiblioXml;
                        goto DOORDER;
                    }

                    // 需要从内核映射过来文件
                    string strLocalPath = "";

                    if (String.Compare(strBiblioType, "html", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else if (String.Compare(strBiblioType, "text", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio_text.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else
                    {
                        strError = "不能识别的strBiblioType类型 '" + strBiblioType + "'";
                        goto ERROR1;
                    }

                    if (nRet == -1)
                        goto ERROR1;

                    // 将种记录数据从XML格式转换为HTML格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";

                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = app.ConvertBiblioXmlToHtml(
                                strFilterFileName,
                                strBiblioXml,
                                    null,
                                strOutputBiblioRecPath,
                                out strBiblio,
                                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strBiblio = "";
                }

            DOORDER:
                // 取得订购信息
                if (String.IsNullOrEmpty(strResultType) == true
                    || String.Compare(strResultType, "recpath", true) == 0)
                {
                    strResult = ""; // 不返回任何结果
                }
                else if (String.Compare(strResultType, "xml", true) == 0)
                {
                    strResult = strXml;
                }
                else if (String.Compare(strResultType, "html", true) == 0)
                {
                    // 将订购记录数据从XML格式转换为HTML格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\orderxml2html.cs",
                        app.CfgDir + "\\orderxml2html.cs.ref",
                        strXml,
                        strOrderRecPath,    // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (String.Compare(strResultType, "text", true) == 0)
                {
                    // 将订购记录数据从XML格式转换为text格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\orderxml2text.cs",
                        app.CfgDir + "\\orderxml2text.cs.ref",
                        strXml,
                        strOrderRecPath,    // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                // 模拟创建检索点
                else if (String.Compare(strResultType, "keys", true) == 0)
                {
                    nRet = app.GetKeys(sessioninfo,
                        strOrderRecPath,
                        strItemXml,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strError = "未知的订购记录结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetOrderInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // *** 此API已经废止 ***
        // 对(订购)编号进行查重
        // parameters:
        //          strIndex  实际上要在这里使用参考ID。2012/4/6
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有searchorderdup权限
        public LibraryServerResult SearchOrderDup(string strIndex,
            string strBiblioRecPath,
            int nMax,
            out string[] paths)
        {
            paths = null;

            LibraryServerResult result = this.PrepareEnvironment("SearchOrderDup", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("searchorder", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "编号查重被拒绝。不具备searchorder权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;
                string strError = "";

                List<string> locateParam = null;

                nRet = app.OrderItemDatabase.BuildLocateParam(
                    // strBiblioRecPath,
                    strIndex,
                    out locateParam,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    return app.OrderItemDatabase.SearchItemDup(
                        // sessioninfo.Channels,
                        channel,
                        locateParam,
                        nMax,
                        out paths);
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchOrderDup() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 检索订购信息
        // parameters:
        //      strQueryWord    检索词
        //      strFrom 检索途径
        //      strMathStyle    匹配方式 exact left right middle
        // 权限: 
        //      需要 searchorder 权限
        // return:
        //      result.Value    命中结果总数。如果为-1，则表示检索出错
        public LibraryServerResult SearchOrder(
            string strOrderDbName,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchOrder", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchorder,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索订购信息被拒绝。不具备order、searchorder权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                List<string> dbnames = new List<string>();

                if (String.IsNullOrEmpty(strOrderDbName) == true
                    || strOrderDbName == "<全部>"
                    || strOrderDbName.ToLower() == "<all>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strDbName = app.ItemDbs[i].OrderDbName;
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;
                        dbnames.Add(strDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何订购库";
                        goto ERROR1;
                    }
                }
                else if (strOrderDbName == "<全部期刊>"
                    || strOrderDbName.ToLower() == "<all series>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strCurrentOrderDbName = app.ItemDbs[i].OrderDbName;
                        string strCurrentIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strCurrentOrderDbName) == true)
                            continue;

                        if (String.IsNullOrEmpty(strCurrentIssueDbName) == true)
                            continue;

                        dbnames.Add(strCurrentOrderDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何期刊订购库";
                        goto ERROR1;
                    }
                }
                else if (strOrderDbName == "<全部图书>"
                    || strOrderDbName.ToLower() == "<all book>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strCurrentOrderDbName = app.ItemDbs[i].OrderDbName;
                        string strCurrentIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strCurrentOrderDbName) == true)
                            continue;

                        // 大书目库中必须不包含期库，说明才是图书用途
                        if (String.IsNullOrEmpty(strCurrentIssueDbName) == false)
                            continue;

                        dbnames.Add(strCurrentOrderDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何图书订购库";
                        goto ERROR1;
                    }
                }
                else
                {
                    string[] splitted = strOrderDbName.Split(new char[] { ',' });
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        string strDbName = splitted[i];
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        if (app.IsOrderDbName(strDbName) == false)
                        {
                            strError = "库名 '" + strDbName + "' 不是合法的订购库名";
                            goto ERROR1;
                        }

                        dbnames.Add(strDbName);
                    }

                }

                bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

                // 构造检索式
                string strQueryXml = "";
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    string strRelation = "=";
                    string strDataType = "string";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOf("-") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else if (String.IsNullOrEmpty(strQueryWord) == false)
                        {
                            strDataType = "number";

                            // 2012/8/20
                            strMatchStyle = "exact";
                        }
                    }
                    else if (strFrom == "订购时间")
                    {
                        // 如果为范围式
                        if (strQueryWord.IndexOf("~") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else
                        {
                            strDataType = "number";

                            // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                            // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                            if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                            {
                                strMatchStyle = "exact";
                                strRelation = "range";
                                strQueryWord = "~";
                            }
                        }
                        // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                        strMatchStyle = "exact";
                    }

                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)    // 2007/9/14 
                        + "'><item>"
                        + (bDesc == true ? "<order>DESC</order>" : "")
                    + "<word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                    if (i > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                }

                if (dbnames.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                return result;

            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchOrder() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置流通时钟
        // parameters:
        //      strTime RFC1123时间格式。如果为空，表示还原为服务器机器时钟
        // 权限：作为工作人员，应当有setclock权限。读者不能设置时钟。
        public LibraryServerResult SetClock(string strTime)
        {
            LibraryServerResult result = this.PrepareEnvironment("SetClock", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断
                if (sessioninfo.GlobalUser == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置时钟被拒绝。全局用户才能进行此操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (StringUtil.IsInList("setclock", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置时钟被拒绝。不具备setclock权限";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置时钟被拒绝。作为读者不能设置时钟";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                string strError = "";
                int nRet = app.Clock.SetClock(strTime, out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCode.SystemError;
                    result.ErrorInfo = strError;
                    return result;
                }

                app.Changed = true;

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetClock() API 出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得流通时钟
        // parameters:
        //      strTime 返回的时间为RFC1123时间格式。
        // 权限：暂时不需要任何权限
        public LibraryServerResult GetClock(out string strTime)
        {
            strTime = "";

            LibraryServerResult result = this.PrepareEnvironment("GetClock", false);
            if (result.Value == -1)
                return result;

            // 不需要登录?
            strTime = app.Clock.GetClock();
            return result;
        }

        // 读者找回密码
        // parameters:
        //      strParameters   参数列表。
        //                      tel=?????,barcode=?????,name=?????
        //                      email=?????,barcode=??????,name=??????
        //                      librarycode=????
        //      strMessageTempate   消息文字模板。其中可以使用 %name% %barcode% %temppassword% %expiretime% %period% 等宏
        // result.Value:
        //      -1  出错
        //      0   因为条件不具备功能没有成功执行(其中，如果 ErrorCode 为 NotFound，表示读者账户尚未注册手机号)
        //      1   功能成功执行
        public LibraryServerResult ResetPassword(string strParameters,
            string strMessageTemplate,
            out string strMessage)
        {
            strMessage = "";

            Hashtable parameters = StringUtil.ParseParameters(strParameters, ',', '=');
            string strStyle = (string)parameters["style"];
            bool bReturnMessage = StringUtil.IsInList("returnMessage", strStyle);

            LibraryServerResult result = this.PrepareEnvironment("ResetPassword", bReturnMessage, bReturnMessage, true);
            if (result.Value == -1)
                return result;

            if (bReturnMessage)
            {
                if (sessioninfo.RightsOriginList.IsInList("resetpasswordreturnmessage") == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "(style为'returnMessage'的)重设密码的操作被拒绝。不具备 resetpasswordreturnmessage 权限";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            string strError = "";

            // 不需要登录
            // return:
            //      -2  读者的图书馆账户尚未注册手机号
            //      -1  出错
            //      0   因为条件不具备功能没有成功执行
            //      1   功能成功执行
            int nRet = app.ResetPassword(
                // sessioninfo.LibraryCodeList,
                strParameters,
                strMessageTemplate,
                out strMessage,
                out strError);
            result.ErrorInfo = strError;
            if (nRet == -2)
            {
                result.Value = 0;
                result.ErrorCode = ErrorCode.NotFound;
            }
            else
            {
                result.Value = nRet;
            }
            return result;
        }

        // 为读者记录绑定新的号码
        // parameters:
        //      strAction   动作。有 bind/unbind
        //      strQueryWord    用于定位读者记录的检索词。
        //          0) 如果以"RI:"开头，表示利用 参考ID 进行检索
        //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
        //          2) 如果以"EM:"开头，表示利用email地址进行检索。注意 email 本身应该是 email:xxxx 这样的形态。也就是说，整个加起来是 EM:email:xxxxx
        //          3) 如果以"TP:"开头，表示利用电话号码进行检索
        //          4) 如果以"ID:"开头，表示利用身份证号进行检索
        //          5) 如果以"CN:"开头，表示利用证件号码进行检索
        //          6) 如果以"UN:"开头，表示利用用户名进行检索，意思就是工作人员的账户名了，不是针对读者绑定
        //          7) 否则用证条码号进行检索
        //      strPassword     读者记录的密码
        //      strBindingID    要绑定的号码。格式如 email:xxxx 或 weixinid:xxxxx
        //      strStyle    风格。multiple/single/singlestrict。默认 single
        //                  multiple 表示允许多次绑定同一类型号码；single 表示同一类型号码只能绑定一次，如果多次绑定以前的同类型号码会被清除; singlestrict 表示如果以前存在同类型号码，本次绑定会是失败
        //                  如果包含 null_password，表示不用读者密码，strPassword 参数无效。但这个功能只能被工作人员使用
        //      strResultTypeList   结果类型数组 xml/html/text/calendar/advancexml/recpaths/summary
        //              其中calendar表示获得读者所关联的日历名；advancexml表示经过运算了的提供了丰富附加信息的xml，例如具有超期和停借期附加信息
        //              advancexml_borrow_bibliosummary/advancexml_overdue_bibliosummary/advancexml_history_bibliosummary
        //      results 返回操作成功后的读者记录
        public LibraryServerResult BindPatron(
            string strAction,
            string strQueryWord,
            string strPassword,
            string strBindingID,
            string strStyle,
            string strResultTypeList,
            out string[] results)
        {
            results = null;

            LibraryServerResult result = this.PrepareEnvironment("BindingPatron", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (sessioninfo.RightsOriginList.IsInList("bindpatron") == false)
                {
                    result.Value = -1;
                    // result.ErrorInfo = "绑定号码的操作被拒绝。当前用户 '"+sessioninfo.UserID+"' 不具备 bindpatron 权限。";
                    result.ErrorInfo = "绑定号码的操作被拒绝。当前用户不具备 bindpatron 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                string strError = "";
                // return:
                //      -2  权限不够，操作被拒绝
                //      -1  出错
                //      0   因为条件不具备功能没有成功执行
                //      1   功能成功执行
                int nRet = app.BindPatron(
                    sessioninfo,
                    strAction,
                    strQueryWord,
                    strPassword,
                    strBindingID,
                    strStyle,
                    strResultTypeList,
                    out results,
                    out strError);
                if (nRet == -2)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCode.AccessDenied;
                }
                else
                    result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library BindPatron() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得值列表
        // parameters:
        //      values 返回值列表。
        // 权限：暂时不需要任何权限
        public LibraryServerResult GetValueTable(
            string strTableName,
            string strDbName,
            out string[] values)
        {
            values = null;

            LibraryServerResult result = this.PrepareEnvironment("GetValueTable", true, true); // false
            if (result.Value == -1)
                return result;
            try
            {
                // 先从 <valueTables> 中找
                values = app.GetValueTable(
                    sessioninfo.LibraryCodeList,
                    strTableName,
                    strDbName);
#if NO
                if (
                    strTableName == "location" &&
                    (values == null || values.Length == 0)
                    )
                {
                    // 然后从 locationTypes 中找到
                    values = app.GetLocationValueList(sessioninfo.LibraryCodeList);
                }
#endif

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetValueTable() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 成批获得日志记录
        // parameters:
        //      nCount  本次希望获取的记录数。如果==-1，表示希望尽可能多地获取
        //      strStyle    level-0/level-1/level-2 表示详略级别
        //                  level-0   全部
        //                  level-1   删除 读者记录和册记录
        //                  level-2   删除 读者记录和册记录中的 <borrowHistory>
        // return:
        //      result.Value
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围，本次调用无效
        public LibraryServerResult GetOperLogs(
            string strFileName,
            long lIndex,
            long lHint,
            int nCount,
            string strStyle,
            string strFilter,
            out OperLogInfo[] records)
        {
            records = null;

            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("GetOperLogs", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取操作日志信息被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (sessioninfo.RightsOriginList.IsInList("getoperlog") == false
                    && sessioninfo.RightsOriginList.IsInList("replication") == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得日志记录被拒绝。不具备 getoperlog 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;
                if (StringUtil.IsInList("accessLog", strStyle) == true)
                {
                    if (string.IsNullOrEmpty(app.MongoDbConnStr) == true
                        || app.AccessLogDatabase == null)
                    {
                        // accessLog 尚未使用
                        records = null;
                        nRet = 0;
                    }
                    else
                    {
                        nRet = app.AccessLogDatabase.GetOperLogs(
                            sessioninfo.LibraryCodeList,
                            strFileName,
                            lIndex,
                            lHint,
                            nCount,
                            strStyle,
                            strFilter,
                            out records,
                            out strError);
                    }
                }
                else
                {
                    // 从 strStyle 里移走 supervisor，避免前端通过本 API 看到日志记录中读者记录的 password 元素
                    // if (sessioninfo.RightsOriginList.IsInList("replication") == false)
                    //    StringUtil.RemoveFromInList("supervisor", true, ref strStyle);
                    if (StringUtil.IsInList("supervisor", strStyle) && sessioninfo.RightsOriginList.IsInList("replication") == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "以敏感信息方式获得日志记录被拒绝。不具备 replication 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    DateTime start_time = DateTime.Now;

                REDO:
                    // return:
                    //      -1  error
                    //      0   file not found
                    //      1   succeed
                    //      2   超过范围
                    nRet = app.OperLog.GetOperLogs(
                        sessioninfo.LibraryCodeList,
                        strFileName,
                        lIndex,
                        lHint,
                        nCount,
                        strStyle,
                        strFilter,
                        out records,
                        out strError);
                    if ((nRet == 2 || nRet == 1) && (records == null || records.Length == 0)
                        && StringUtil.IsInList("wait", strStyle) == true
                        && DateTime.Now - start_time < TimeSpan.FromSeconds(30))
                    {
                        // 敏捷中断
                        Task.Delay(1000, app.AppDownToken).Wait();
                        goto REDO;
                    }
                }
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetOperLogs() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得日志记录
        // parameters:
        //      strFileName 纯文件名,不含路径部分。但要包括".log"部分。
        //      lIndex  记录序号。从0开始计数。
        //              lIndex 为 -1 调用本函数：
        //              1) 若 strStyle 中不包含 "getcount" 时，表示希望获得整个文件尺寸值，将返回在 lHintNext 中；
        //              2) 若 strStyle 中包含 "getcount"，表示希望获得整个文件中包含的日志记录数，将返回在 lHintNext 中。
        //      lHint   记录位置暗示性参数。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。
        //              目前的含义是记录起始位置。
        //      strStyle    level-0/level-1/level-2 表示详略级别
        //                  level-0   全部
        //                  level-1   删除 读者记录和册记录
        //                  level-2   删除 读者记录和册记录中的 <borrowHistory>
        //                  getcount    表示希望获得指定日志文件中的记录总数。返回在 lHintNext 参数中
        //                  如果包含 dont_return_xml 表示在 strXml 不返回内容
        // 权限：需要getoperlog权限
        // return:
        // result.Value
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public LibraryServerResult GetOperLog(
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            string strFilter,
            out string strXml,
            out long lHintNext,
            long lAttachmentFragmentStart,
            int nAttachmentFragmentLength,
            out byte[] attachment_data,
            out long lAttachmentTotalLength)
        {
            strXml = "";
            lHintNext = -1;
            attachment_data = null;
            lAttachmentTotalLength = 0;

            string strError = "";
            int nRet = 0;

            LibraryServerResult result = this.PrepareEnvironment("GetOperLog", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取操作日志信息被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (sessioninfo.RightsOriginList.IsInList("getoperlog") == false
                    && sessioninfo.RightsOriginList.IsInList("replication") == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得日志记录被拒绝。不具备getoperlog权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (StringUtil.IsInList("accessLog", strStyle) == true)
                {
                    if (lIndex == -1    // 要获得文件尺寸
                        || StringUtil.IsInList("getcount", strStyle) == true)// 要获得总记录数
                    {
                        // 获得文件尺寸。用事项个数代替文件尺寸
                        // -1 表示集合不存在。通常是因为 mongodb 没有配置或没有启动
                        if (string.IsNullOrEmpty(app.MongoDbConnStr) == true
                            || app.AccessLogDatabase == null)
                            lHintNext = -1;
                        else
                            lHintNext = app.AccessLogDatabase.GetItemCount(strFileName.Substring(0, 8));
                        result.Value = 1;
                        result.ErrorInfo = strError;
                        return result;
                    }

                    if (string.IsNullOrEmpty(app.MongoDbConnStr) == true
    || app.AccessLogDatabase == null)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "访问日志尚未启用";
                        return result;
                    }

                    // 从 strStyle 里移走 supervisor，避免前端通过本 API 看到日志记录中读者记录的 password 元素
                    // StringUtil.RemoveFromInList("supervisor", true, ref strStyle);
                    if (StringUtil.IsInList("supervisor", strStyle) && sessioninfo.RightsOriginList.IsInList("replication") == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "以敏感信息方式获得日志记录被拒绝。不具备 replication 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    OperLogInfo[] records = null;
                    nRet = app.AccessLogDatabase.GetOperLogs(
                        sessioninfo.LibraryCodeList,
                        strFileName,
                        lIndex,
                        lHint,
                        1,
                        strStyle,
                        strFilter,
                        out records,
                        out strError);
                    if (nRet == 1)
                    {
                        OperLogInfo info = records[0];
                        // 2017/12/5
                        if (StringUtil.IsInList("dont_return_xml", strStyle) == true)
                            strXml = "";
                        else
                            strXml = info.Xml;
                        lAttachmentTotalLength = info.AttachmentLength;
                        lHintNext = info.HintNext;
                    }
                    result.Value = nRet;
                    result.ErrorInfo = strError;
                    return result;
                }

                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                nRet = app.OperLog.GetOperLog(
                    sessioninfo.LibraryCodeList,
                    strFileName,
                    lIndex,
                    lHint,
                    strStyle,
                    strFilter,
                    out lHintNext,
                    out strXml,
                    out lAttachmentTotalLength,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                result.ErrorInfo = strError;

                if (nRet == 1 && lAttachmentTotalLength > 0
                    && (nAttachmentFragmentLength > 0 || nAttachmentFragmentLength == -1))  // -1 是 2017/5/16 增加的
                {
                    // 读出attachment片断
                    // attachment.Seek(0, SeekOrigin.Begin);    // 不必要了

                    if (lAttachmentFragmentStart > lAttachmentTotalLength)
                    {
                        strError = "lAttachmentFragmentStart参数的值超过附件的尺寸";
                        goto ERROR1;
                    }

                    long lTemp = 0;
                    // parameters:
                    //      nAttachmentFragmentLength   要读出的附件内容字节数。如果为 -1，表示尽可能多读出内容
                    // return:
                    //      -1  error
                    //      0   file not found
                    //      1   succeed
                    //      2   超过范围
                    nRet = app.OperLog.GetOperLogAttachment(
                        sessioninfo.LibraryCodeList,
                        strFileName,
                        lIndex,
                        lHint,
                        lAttachmentFragmentStart,
                        nAttachmentFragmentLength,
                        out attachment_data,
                        out lTemp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetOperLog() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得日历
        // return:
        //      result.Value    -1 错误; 其他 返回总结果数量
        public LibraryServerResult GetCalendar(
            string strAction,
            string strName,
            int nStart,
            int nCount,
            out CalenderInfo[] contents)
        {
            contents = null;

            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("GetCalendar", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("getcalendar", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得日历操作被拒绝。不具备getcalendar权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                List<CalenderInfo> result_contents = null;
                int nRet = app.GetCalendar(strAction,
                    sessioninfo.LibraryCodeList,
                    strName,
                    nStart,
                    nCount,
                    out result_contents,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (result_contents != null)
                {
                    contents = new CalenderInfo[result_contents.Count];
                    result_contents.CopyTo(contents);
                }

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetCalendar() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 修改日历
        // return:
        //      result.Value    -1 错误
        public LibraryServerResult SetCalendar(
            string strAction,
            CalenderInfo info)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SetCalendar", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改日历的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (strAction == "new")
                {
                    if (StringUtil.IsInList("newcalendar", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "创建新日历的操作被拒绝。不具备newcalendar权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                if (strAction == "delete")
                {
                    if (StringUtil.IsInList("deletecalendar", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "删除日历的操作被拒绝。不具备deletecalendar权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                if (strAction == "change")
                {
                    if (StringUtil.IsInList("changecalendar", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改日历的操作被拒绝。不具备changecalendar权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                if (strAction == "overwrite")
                {
                    if (StringUtil.IsInList("changecalendar", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "覆盖日历的操作被拒绝。不具备changecalendar权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    if (StringUtil.IsInList("newcalendar", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "覆盖日历的操作被拒绝。不具备newcalendar权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                int nRet = app.SetCalendar(strAction,
                    sessioninfo.LibraryCodeList,
                    info,
                    out ErrorCode error_code,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2013/12/25
                // 促使立即写入 library.xml
                if (app.Changed == true)
                    app.ActivateManagerThread();

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                if (error_code == ErrorCode.NoError)
                    result.ErrorCode = ErrorCode.SystemError;
                else
                    result.ErrorCode = error_code;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetCalendar() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        /*
2016/11/6 9:16:39 dp2Library BatchTask() API出现异常: Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
   在 DigitalPlatform.LibraryServer.LibraryApplication.GetBatchTaskInfo(String strName, BatchTaskInfo param, BatchTaskInfo& info, String& strError)
   在 dp2Library.LibraryService.BatchTask(String strName, String strAction, BatchTaskInfo info, BatchTaskInfo& resultInfo)
         * */
        // 操作一个批处理任务
        public LibraryServerResult BatchTask(
            string strName,
            string strAction,
            BatchTaskInfo info,
            out BatchTaskInfo resultInfo)
        {
            string strError = "";
            resultInfo = null;

            bool check_hangup = strAction == "getinfo" || strAction == "stop" ? false : true;
            LibraryServerResult result = this.PrepareEnvironment("BatchTask", true, true, check_hangup);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "批处理任务操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (sessioninfo.GlobalUser == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "操作批处理任务 被拒绝。只有全局用户才能进行这样的操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (strName == "大备份" && StringUtil.IsInList("backup", sessioninfo.RightsOrigin) == true)
                {
                    // 大备份任务可以由 backup 权限的账户启动
                }
                else if (StringUtil.IsInList("batchtask", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "操作批处理任务 被拒绝。不具备batchtask权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;

                if (strAction == "start")
                {
                    nRet = app.StartBatchTask(strName,
                        info,
                        out resultInfo,
                        out strError);
                }
                else if (strAction == "stop")
                {
                    nRet = app.StopBatchTask(strName,
                        info,
                        out resultInfo,
                        out strError);
                }
                else if (strAction == "abort")
                {
                    // 2017/8/27
                    nRet = app.AbortBatchTask(strName,
                        info,
                        out resultInfo,
                        out strError);
                }
                else if (strAction == "continue")
                {
                    nRet = app.StartBatchTask("!continue",
                        null,
                        out resultInfo,
                        out strError);
                }
                else if (strAction == "pause")
                {
                    nRet = app.StopBatchTask("!pause",
                        null,
                        out resultInfo,
                        out strError);
                }
                else if (strAction == "getinfo")
                {
                    nRet = app.GetBatchTaskInfo(strName,
                        info,
                        out resultInfo,
                        out strError);
                }
                else
                {
                    strError = "不能识别的strAction参数 '" + strAction + "'";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                result.ErrorInfo = strError;    // 2014/11/26
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library BatchTask() API出现异常: "
                    + ExceptionUtil.GetDebugText(ex)
                    + string.Format("\r\nstrName='{0}', strAction='{1}', info='{2}'",
                    strName,
                    strAction,
                    info == null ? "(null)" : info.Dump().Replace("\r\n", ";"));
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 清除所有数据库内数据
        // return:
        //      result.Value    -1 错误
        public LibraryServerResult ClearAllDbs()
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("ClearAllDbs", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("clearalldbs", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "清除所有数据库内数据的操作被拒绝。不具备clearalldbs权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = app.ClearAllDbs(sessioninfo.Channels,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ClearAllDbs() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 管理数据库
        // parameters:
        //      strAction   动作。create delete initialize backup getinfo
        // return:
        //      result.Value    -1 错误。如果数据库不存在，同时还要返回 ErrorCode 为 NotFound
        public LibraryServerResult ManageDatabase(string strAction,
            string strDatabaseName,
            string strDatabaseInfo,
            string strStyle,
            out string strOutputInfo)
        {
            string strError = "";
            strOutputInfo = "";

            LibraryServerResult result = this.PrepareEnvironment("ManageDatabase", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // getinfo 动作 权限单独判断 2013/1/27
                if (strAction == "getinfo")
                {
                    if (StringUtil.IsInList("managedatabase,getsystemparameter,order", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "管理数据库的操作 '" + strAction + "' 被拒绝。不具备 getsystemparameter 或 order 或 managedatabase 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else
                {
                    // 对读者身份的判断
                    if (sessioninfo.UserType == "reader")
                    {
                        result.Value = -1;
                        result.ErrorInfo = "管理数据库的操作被拒绝。作为读者不能管理任何数据库(strAction='getinfo' 除外)";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 权限判断
                    if (StringUtil.IsInList("managedatabase", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "管理数据库的操作 '" + strAction + "' 被拒绝。不具备managedatabase权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // return:
                //      -1  出错
                //      0   没有找到
                //      1   成功
                int nRet = app.ManageDatabase(
                    sessioninfo,
                    sessioninfo.Channels,
                    sessioninfo.LibraryCodeList,
                    strAction,
                    strDatabaseName,
                    strDatabaseInfo,
                    strStyle,
                    out strOutputInfo,
                    out strError);
                if (nRet == -1)
                {
                    result.ErrorCode = ErrorCode.SystemError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    result.ErrorCode = ErrorCode.NotFound;
                    goto ERROR1;
                }

                result.Value = nRet;    // 原来这里返回 0
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ManageDatabase() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得用户
        // parameters:
        //      strAction   暂未使用。保持空即可。
        //      strName     用户名。如果为空，表示希望列出全部用户信息
        // return:
        //      result.Value    -1 错误; 其他 返回总结果数量
        public LibraryServerResult GetUser(
            string strAction,
            string strName,
            int nStart,
            int nCount,
            out UserInfo[] contents)
        {
            contents = null;

            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("GetUser", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "获取用户信息的操作被拒绝。作为读者不能任何用户信息";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (StringUtil.IsInList("getuser", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得用户操作被拒绝。不具备getuser权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = app.ListUsers(
                    sessioninfo.LibraryCodeList,
                    strName,
                    nStart,
                    nCount,
                    out contents,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetUser() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 修改用户
        // parameters:
        //      strAction   new delete change resetpassword
        //              当 action 为 "change" 时，如果要在修改其他信息的同时修改密码，info.SetPassword必须为true；
        //              而当action为"resetpassword"时，则info.ResetPassword状态不起作用，无论怎样都要修改密码。resetpassword并不修改其他信息，也就是说info中除了Password/UserName以外其他成员的值无效。
        //              当 action 为 "changeandclose" 时，效果同 "change"，只是最后还要自动切断此用户的 session
        // return:
        //      result.Value    -1 错误
        public LibraryServerResult SetUser(
            string strAction,
            UserInfo info)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SetUser", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改用户的操作被拒绝。作为读者不能修改任何用户信息";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (strAction == "closechannel")
                {
                    if (StringUtil.IsInList("changeuser", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "关闭用户账户相关通道的操作被拒绝。不具备changeuser权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    result.Value = app.SessionTable.CloseSessionByUserID(info.UserName);
                    return result;
                }

                // 权限字符串
                if (strAction == "new")
                {
                    if (StringUtil.IsInList("newuser", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "创建新用户的操作被拒绝。不具备newuser权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                if (strAction == "delete")
                {
                    if (StringUtil.IsInList("deleteuser", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "删除用户的操作被拒绝。不具备deleteuser权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                if (strAction == "change" || strAction == "changeandclose")
                {
                    if (StringUtil.IsInList("changeuser", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改用户的操作被拒绝。不具备changeuser权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    if (info.SetPassword == true)
                    {
                        // 这是指强制修改他人的密码的操作。这是一个系统管理员才能进行的高级操作。
                        if (StringUtil.IsInList("changeuserpassword", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "强制修改(其他)用户密码的操作被拒绝。不具备changeuserpassword权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }

                if (strAction == "resetpassword")
                {
                    // 这是指强制修改他人的密码的操作。这是一个系统管理员才能进行的高级操作。
                    if (StringUtil.IsInList("changeuserpassword", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改用户密码的操作被拒绝。不具备changeuserpassword权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                bool bClose = strAction == "changeandclose";
                if (strAction == "changeandclose")
                    strAction = "change";

                int nRet = app.SetUser(
                    sessioninfo.LibraryCodeList,
                    strAction,
                    sessioninfo.UserID,
                    info,
                    sessioninfo.ClientAddress,
                    out ErrorCode error_code,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2013/3/6
                // 促使立即写入 library.xml
                if (app.Changed == true)
                    app.ActivateManagerThread();

                if (bClose)
                    app.SessionTable.CloseSessionByUserID(info.UserName);

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                if (error_code != ErrorCode.NoError)    // 2021/7/17
                    result.ErrorCode = error_code;
                else
                    result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetUser() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得通道信息
        // return:
        //      result.Value    -1 错误; 其他 返回总结果数量
        public LibraryServerResult GetChannelInfo(
            string strQuery,
            string strStyle,
            int nStart,
            int nCount,
            out ChannelInfo[] contents)
        {
            contents = null;

            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("GetChannelInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得通道信息被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (StringUtil.IsInList("getchannelinfo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得通道信息被拒绝。不具备 getchannelinfo 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = app.ListChannels(
                    strQuery,
                    strStyle,
                    nStart,
                    nCount,
                    out contents,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetChannelInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 管理通道
        // return:
        //      result.Value    -1 错误; 其他 返回总结果数量
        public LibraryServerResult ManageChannel(
            string strAction,
            string strStyle,
            ChannelInfo[] requests,
            out ChannelInfo[] results)
        {
            results = null;

            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("ManageChannel", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "管理通道操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限字符串
                if (StringUtil.IsInList("managechannel", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "管理通道操作被拒绝。不具备 managechannel 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = app.ManageChannel(
                    strAction,
                    strStyle,
                    requests,
                    out results,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ManageChannel() API 出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 修改用户自己的密码
        // 只能用本 API 修改自己的密码。如果要强制修改别人的密码(意思是修改时并不知道旧密码)，请使用SetUser() API
        // 注：调用本 API 修改前，无需登录
        // return.Value:
        //      -1  出错
        //      0   成功
        public LibraryServerResult ChangeUserPassword(
            string strUserName,
            string strOldPassword,
            string strNewPassword)
        {
            string strError = "";
            int nRet = 0;

            LibraryServerResult result = null;

            try
            {
                // 特殊功能：修改 library.xml 中 dp2kernel 用户的密码，并访问 dp2kernel 修改此用户的密码
                if (strUserName == "!changeKernelPassword")
                {
                    result = this.PrepareEnvironment("ChangeUserPassword",
    true,
    true,  // 检查是否登录
    true);
                    if (result.Value == -1)
                        return result;

                    if (StringUtil.IsInList("supervisor", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "当前登录用户 " + sessioninfo.UserID + " 不具备 supervisor 权限，无法修改 kernel 密码";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    nRet = app.ChangeKernelPassword(
                        sessioninfo,
                        strOldPassword,
                        strNewPassword,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // 促使立即写入 library.xml
                    if (app.Changed == true)
                        app.ActivateManagerThread();

                    result.Value = nRet;
                    return result;
                }

                // 特殊功能：修改 library.xml 中所有的匹配的馆代码
                if (strUserName == "!changeLibraryCode")
                {
                    result = this.PrepareEnvironment("ChangeUserPassword",
true,
true,  // 检查是否登录
true);
                    if (result.Value == -1)
                        return result;

                    if (StringUtil.IsInList("supervisor", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "当前登录用户 " + sessioninfo.UserID + " 不具备 supervisor 权限，无法修改 library.xml 中的馆代码定义";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    nRet = app.ChangeLibraryCode(
                        sessioninfo,
                        strOldPassword,
                        strNewPassword,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 促使立即写入 library.xml
                    if (app.Changed == true)
                        app.ActivateManagerThread();

                    result.Value = nRet;
                    return result;
                }

                {
                    result = this.PrepareEnvironment("ChangeUserPassword",
    true,
    false,  // 不检查是否登录
    true);
                    if (result.Value == -1)
                        return result;

                    // 根据用户名，获得账户权限和图书馆代码

                    // 2021/7/4
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = app.GetUserInfo(strUserName,
                        out UserInfo userinfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                        goto ERROR1;

                    // 权限判断

                    /*
                    // 只能自己修改自己的密码
                    if (sessioninfo.UserID != strUserName)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "当前登录用户 " + sessioninfo.UserID + " 只能修改自己的密码，不能修改别人(" + strUserName + ")的密码。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    */

                    if (StringUtil.IsInList("denychangemypassword", userinfo.Rights/*sessioninfo.RightsOrigin*/) == true)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "当前登录用户 " + userinfo.Rights/*sessioninfo.UserID*/ + " 因被设定了 denychangemypassword 权限，不能修改自己的密码";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    nRet = app.ChangeUserPassword(
                        userinfo.LibraryCode,   // sessioninfo.LibraryCodeList,
                        strUserName,
                        strOldPassword,
                        strNewPassword,
                        sessioninfo.ClientIP,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 2014/12/3
                    // 促使立即写入 library.xml
                    if (app.Changed == true)
                        app.ActivateManagerThread();

                    result.Value = nRet;
                    return result;
                }
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ChangeUserPassword() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 校验条码号
        // parameters:
        //      strBarcode 条码号
        // return:
        //      result.Value 0: 不是合法的条码号 1:合法的读者证条码号 2:合法的册条码号
        // 权限：暂时不需要任何权限
        public LibraryServerResult VerifyBarcode(
            string strAction,
            string strLibraryCode,
            string strBarcode,
            out string strOutputBarcode)
        {
            strOutputBarcode = "";

            // LibraryServerResult result = this.PrepareEnvironment(false);
            LibraryServerResult result = this.PrepareEnvironment("VerifyBarcode", true, true);
            if (result.Value == -1)
                return result;

            // 不需要登录
            try
            {
                // string strResultString = "";
                int nResultValue = -1;
                string strError = "";

                if (StringUtil.HasHead(strBarcode, "PQR:") == true)
                {
                    result.ErrorInfo = "这是读者证号二维码";
                    result.Value = 1;
                    return result;
                }

#if NO
                if (app.m_assemblyLibraryHost == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "没有配置<script>，无法校验条码号";
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }
#endif
                int nRet = 0;
                // TODO: 注意这里可能会面临新的 XML 校验规则判断
                if (app.BarcodeValidation == false) // 2019/7/12
                {
                    // 当前是否定义了脚本?
                    // return:
                    //      -1  定义了，但编译有错
                    //      0   没有定义
                    //      1   定义了
                    nRet = app.HasScript(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "没有配置<script>，无法校验条码号";
                        result.ErrorCode = ErrorCode.NotFound;
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(strAction)
                    || strAction == "verify" || strAction == "VerifyBarcode")
                {
                    // 旧的 C# 校验脚本，和新的校验规则(XML 格式)都能兼容
                    // return:
                    //      -2  not found script
                    //      -1  出错
                    //      0   成功
                    nRet = app.DoVerifyBarcodeScriptFunction(
                        null,
                        strLibraryCode, // sessioninfo.LibraryCodeList,
                        strBarcode,
                        out nResultValue,
                        out strError);
                }
                else if (strAction == "transform" || strAction == "TransformBarcode")
                {
                    // return:
                    //      -2  not found script
                    //      -1  出错
                    //      0   成功
                    nRet = app.DoTransformBarcodeScriptFunction(
                        null,
                        strLibraryCode, // sessioninfo.LibraryCodeList,
                        ref strBarcode,
                        out nResultValue,
                        out strError);
                    strOutputBarcode = strBarcode;
                }
                else
                {
                    strError = "无法识别的 strAction 参数值 '" + strAction + "'";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;
                if (nRet == -2)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }

                result.ErrorInfo = strError;
                result.Value = nResultValue;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library VerifyBarcode() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 列出文件目录，转移当前目录，删除文件和目录
        // parameters:
        //      strAction   可用值 list/cd/delete
        public LibraryServerResult ListFile(
            string strAction,
            string strCategory,
            string strFileName,
            long lStart,
            long lLength,
            out List<FileItemInfo> infos)
        {
            string strError = "";
            int nRet = 0;
            infos = new List<FileItemInfo>();

            LibraryServerResult result = this.PrepareEnvironment("ListFile", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                {
                    string strLibraryCode = ""; // 实际写入操作的读者库馆代码
                    string strTemp = "";

                    if (strAction == "delete")
                    {
                        // 检查用户使用 WriteRes API 的权限
                        // return:
                        //      -1  error
                        //      0   不具备权限
                        //      1   具备权限
                        nRet = app.CheckWriteResRights(
                            sessioninfo.LibraryCodeList,
                            sessioninfo.RightsOrigin,
                            strCategory,
                            out strLibraryCode,
                            out strError);
                    }
                    else
                    {
                        nRet = app.CheckGetResRights(
                            sessioninfo,
                            sessioninfo.LibraryCodeList,
                            sessioninfo.RightsOrigin,
                            strCategory,
                            out strLibraryCode,
                            out strTemp,
                            out strError);
                    }

                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    if (nRet == -1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(strCategory) == false && strCategory[0] == '!')
                {
                    // TODO: 根据不同的权限限定不同的 root 起点
                    string strRoot = Path.Combine(app.DataDir, "upload");
                    if (StringUtil.IsInList("managedatabase,backup", sessioninfo.RightsOrigin) == true)
                    {
                        strRoot = app.DataDir.Replace("/", "\\");  // 权力很大，能看到数据目录下的全部文件和目录了
                        if (strRoot.EndsWith("\\") == false)
                            strRoot += "\\";
                    }

                    string strCurrentDirectory = Path.Combine(app.DataDir, strCategory.Substring(1));

                    if (strAction == "delete")
                    {
                        List<string> root_paths = new List<string>();
                        // 根据不同的权限限定不同的 root 起点
                        if (StringUtil.IsInList("backup,managedatabase", sessioninfo.RightsOrigin))
                        {
                            root_paths.Add(EnsureRootPath(Path.Combine(app.DataDir, "backup")));
                        }
                        if (StringUtil.IsInList("upload,managedatabase", sessioninfo.RightsOrigin))
                        {
                            root_paths.Add(EnsureRootPath(Path.Combine(app.DataDir, "upload")));
                        }
#if NO
                        if (StringUtil.IsInList("managedatabase", sessioninfo.RightsOrigin) == true)
                        {
                            string strTemp = app.DataDir.Replace("/", "\\");  // 权力很大，能看到数据目录下的全部文件和目录了
                            if (strTemp.EndsWith("\\") == false)
                                strTemp += "\\";
                            root_paths.Add(strTemp);
                        }
#endif

                        // 删除文件或者目录
                        // return:
                        //      -1  出错
                        //      其他  实际删除的文件和目录个数。若 strError 中返回了值，也表示出错
                        nRet = app.DeleteFile(
                            root_paths,
                            strCurrentDirectory,
                            strFileName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        result.Value = nRet;
                        return result;
                    }

                    if (strAction == "cd")
                    {

                        nRet = LibraryApplication.ChangeDirectory(
                            strRoot,
                            strCurrentDirectory,
                            strFileName,
                            out string strResult,  // 注意返回的是物理路径
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            result.ErrorCode = ErrorCode.NotFound;
                            result.ErrorInfo = strError;
                            result.Value = -1;
                            return result;
                        }
                        infos = new List<FileItemInfo>();
                        FileItemInfo info = new FileItemInfo();
                        infos.Add(info);
                        info.Name = strResult.Substring(strRoot.Length).Replace("\\", "/");    // 这里需要逻辑路径
                        result.Value = 1;
                        return result;
#if NO
                        string strResult = "";
                        try
                        {
                            FileSystemLoader.ChangeDirectory(strCurrentDirectory.Substring(strRoot.Length), strFileName, out strResult);
                        }
                        catch (Exception ex)
                        {
                            result.ErrorCode = ErrorCode.SystemError;
                            result.ErrorInfo = ex.Message;
                            result.Value = -1;
                            return result;
                        }

                        // 检测物理目录是否存在
                        string strPhysical = Path.Combine(strRoot, PathUtil.RemoveRootSlash(strResult));    // 返还为物理路径
                        if (Directory.Exists(strPhysical) == false)
                        {
                            result.ErrorCode = ErrorCode.NotFound;
                            result.ErrorInfo = "目录 '"+strResult+"' 不存在";
                            result.Value = -1;
                            return result;
                        }

                        infos = new List<FileItemInfo>();
                        FileItemInfo info = new FileItemInfo();
                        infos.Add(info);
                        info.Name = strResult;
                        result.Value = 1;
                        return result;
#endif
                    }

                    if (strAction == "list")
                    {
                        // parameters:
                        //      strCurrentDirectory 当前路径。物理路径
                        // return:
                        //      -1  出错
                        //      其他  列出的事项总数。注意，不是 lLength 所指出的本次返回数
                        nRet = app.ListFile(
                            strRoot,
                            strCurrentDirectory,
                            strFileName,
                            lStart,
                            lLength,
                            out infos,
                            out strError);
                        if (nRet == -1)
                        {
                            result.ErrorCode = ErrorCode.SystemError;
                            result.Value = -1;
                        }
                        else
                            result.Value = nRet;

                        result.ErrorInfo = strError;
                        return result;
                    }

                    strError = "未知的 strAction '" + strAction + "'";
                    goto ERROR1;
                }
                else
                {
                    result.ErrorCode = ErrorCode.SystemError;
                    result.Value = -1;
                    result.ErrorInfo = "暂不支持 strCategory '" + strCategory + "'";

                    return result;
                }
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ListFile() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        ERROR1:
            result.ErrorInfo = strError;
            result.Value = -1;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        static string EnsureRootPath(string strTemp)
        {
            strTemp = strTemp.Replace("/", "\\");
            if (strTemp.EndsWith("\\") == false)
                strTemp += "\\";
            return strTemp;
        }

        // 获得系统配置文件
        // parameters:
        //      strCategory 文件分类。目前只能使用 cfgs
        //      lStart  需要获得文件内容的起点。如果为-1，表示(baContent中)不返回文件内容
        //      lLength 需要获得的从lStart开始算起的byte数。如果为-1，表示希望尽可能多地取得(但是不能保证一定到尾)
        //      strStyle    风格。gzip 表示希望获得压缩后的结果。如果确实返回了压缩后的结果，LibraryServerResult 中 ErrorCode 会返回 Compressed
        // rights:
        //      需要 getsystemparameter 权限
        // return:
        //      result.Value    -1 错误；其他 文件的总长度
        public LibraryServerResult GetFile(
            string strCategory,
            string strFileName,
            long lStart,
            long lLength,
            string strStyle,    // 2017/10/7
            out byte[] baContent,
            out string strFileTime)
        {
            string strError = "";
            strFileTime = "";
            baContent = null;

            LibraryServerResult result = this.PrepareEnvironment("GetFile", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断
                if (StringUtil.IsInList("getsystemparameter,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得系统文件的操作被拒绝。不具备order或getsystemparameter权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                if (strCategory == "cfgs")
                {
                    int nMaxLength = 100 * 1024;
                    // TODO: 为了安全，将最后路径限定在特定子目录下
                    string strFilePath = app.DataDir + "/cfgs/" + strFileName;

                    if (lStart >= 0)
                    {
                        try
                        {
                            using (Stream stream = File.Open(
                                strFilePath,
                                FileMode.Open,
                                FileAccess.ReadWrite,
                                FileShare.ReadWrite))
                            {
                                if (lStart >= stream.Length)
                                {
                                    strError = "lStart参数值 " + lStart.ToString() + " 超过文件长度 " + stream.Length.ToString();
                                    goto ERROR1;
                                }

                                if (lLength == -1)
                                    lLength = stream.Length - lStart;
                                else
                                {
                                    if (lStart + lLength > stream.Length)
                                    {
                                        strError = "lLength参数值 " + lLength.ToString() + " 超过范围。(lStart参数值 " + lStart.ToString() + " ,文件长度 " + stream.Length.ToString() + ")";
                                        goto ERROR1;
                                    }
                                }

                                // 每次传输限制在100K以内
                                if (lLength > nMaxLength)
                                    lLength = nMaxLength;

                                baContent = new byte[lLength];

                                stream.Seek(lStart, SeekOrigin.Begin);
                                stream.Read(baContent, 0, (int)lLength);
                                result.Value = stream.Length;

                                // 压缩内容
                                // 2017/10/7
                                if (StringUtil.IsInList("gzip", strStyle)
                                    && baContent != null && baContent.Length > 0)
                                {
                                    baContent = ByteArray.CompressGzip(baContent);
                                    result.ErrorCode = ErrorCode.Compressed;
                                }
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            strError = "文件 " + strFileName + "没有找到";
                            goto ERROR1;
                        }
                        catch (Exception ex)
                        {
                            strError = "打开文件 '" + strFileName + "' 时发生错误: " + ex.Message;
                            goto ERROR1;
                        }
                    }


                    {
                        // 只获得文件长度和最后修改时间
                        try
                        {
                            FileInfo fi = new FileInfo(strFilePath);
                            result.Value = fi.Length;
                            strFileTime = DateTimeUtil.Rfc1123DateTimeString(fi.LastWriteTimeUtc);
                        }
                        catch (Exception ex)
                        {
                            strError = ExceptionUtil.GetAutoText(ex);
                            goto ERROR1;
                        }
                    }
                }
                else
                {
                    strError = "未知的 strCategory 值 '" + strCategory + "'";
                    goto ERROR1;
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetFile() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得系统参数
        // parameters:
        //      strCategory 参数所在目录
        //      strName 参数名
        //      strValue    返回参数值
        // rights:
        //      需要 getsystemparameter 权限
        // return:
        //      result.Value    -1 错误；0 没有得到所要求的参数值；1 得到所要求的参数值
        public LibraryServerResult GetSystemParameter(
            string strCategory,
            string strName,
            out string strValue)
        {
            string strError = "";
            strValue = "";

            bool isPublic = false;  // 是否不需要登录
            if (strCategory == "library" && strName == "name")
                isPublic = true;

            LibraryServerResult result = null;
            if (isPublic)
            {
                result = this.PrepareEnvironment("GetSystemParameter", false);  // 不准备 sessioninfo
                sessioninfo = null;
            }
            else
                result = this.PrepareEnvironment("GetSystemParameter", true, true, true);

            if (result.Value == -1)
                return result;

            // 2016/5/3
            // 两个参数都为空的情况，可以用来迫使前端登录一次
            if (string.IsNullOrEmpty(strCategory) == true
                && string.IsNullOrEmpty(strName) == true)
                return result;

            try
            {
                // 权限判断
                if (isPublic == false && StringUtil.IsInList("getsystemparameter,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得系统参数的操作被拒绝。不具备 order 或 getsystemparameter 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 1;

                nRet = app.GetSystemParameter(
                    sessioninfo,
                    strCategory,
                    strName,
                    out strValue,
                    out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCode.SystemError;
                    result.ErrorInfo = strError;
                    return result;
                }
                if (nRet == 0)
                {
                    result.Value = 0;
                    result.ErrorCode = ErrorCode.NotFound;  // 表示配置节点没有找到
                    if (String.IsNullOrEmpty(strError) == true)
                        result.ErrorInfo = "未知的 category '" + strCategory + "' 和 name '" + strName + "'";
                    else
                        result.ErrorInfo = strError;
                    return result;
                }

                result.Value = nRet;
                return result;
            }
            catch (ApplicationException ex)
            {
                string strErrorText = "dp2Library GetSystemParameter() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.ServerTimeout;
                result.ErrorInfo = strErrorText;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetSystemParameter() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        string EnsureKdbs(bool bThrowException = true)
        {
            return this.app.EnsureKdbs(bThrowException);
        }

        // 设置系统参数
        // parameters:
        //      strCategory 参数所在目录
        //      strName 参数名
        //      strValue    参数值
        // rights:
        //      需要 setsystemparameter 权限
        // return:
        //      result.Value    -1 错误；0 成功
        public LibraryServerResult SetSystemParameter(
            string strCategory,
            string strName,
            string strValue)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SetSystemParameter", true, true, true);
            if (result.Value == -1)
                return result;

            app.LockForWrite();
            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置系统参数的操作被拒绝。作为读者不能设置任何系统参数";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("setsystemparameter", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置系统参数的操作被拒绝。不具备setsystemparameter权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;

                if (strCategory == "center")
                {
                    // 分馆用户不能修改定义
                    if (sessioninfo.GlobalUser == false)
                    {
                        strError = "分馆用户不允许修改<center>元素定义";
                        goto ERROR1;
                    }


                    // 修改 <center> 内的定义
                    // return:
                    //      -1  error
                    //      0   not change
                    //      1   changed
                    nRet = app.SetCenterDef(strName,
                        strValue,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        app.Changed = true;
                        app.ActivateManagerThread();
                    }
                    goto END1;
                }

                // 值列表
                // 2008/8/21 
                if (strCategory == "valueTable")
                {
                    // TODO: 需要进行针对分馆用户的改造
                    // 分馆用户不能修改定义
                    if (sessioninfo.GlobalUser == false)
                    {
                        strError = "分馆用户不允许修改<valueTables>元素定义";
                        goto ERROR1;
                    }

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strValue);
                    }
                    catch (Exception ex)
                    {
                        strError = "strValue装入XMLDOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    string strNameParam = DomUtil.GetAttr(dom.DocumentElement, "name");
                    string strDbNameParam = DomUtil.GetAttr(dom.DocumentElement, "dbname");
                    string strValueParam = dom.DocumentElement.InnerText;

                    // 修改值列表
                    // 2008/8/21 
                    // parameters:
                    //      strAction   "new" "change" "overwirte" "delete"
                    // return:
                    //      -1  error
                    //      0   not change
                    //      1   changed
                    nRet = app.SetValueTable(strName,
                        strNameParam,
                        strDbNameParam,
                        strValueParam,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        app.Changed = true;
                        app.ActivateManagerThread();
                    }
                    goto END1;
                }

                // 读者权限
                if (strCategory == "circulation")
                {
                    // 2021/8/6
                    // 临时修改内存中的 app.AcceptBlankReaderBarcode 值
                    if (strName == "?AcceptBlankReaderBarcode")
                    {
                        app.AcceptBlankReaderBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.AcceptBlankItemBarcode 值
                    if (strName == "?AcceptBlankItemBarcode")
                    {
                        app.AcceptBlankItemBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyBarcode 值
                    if (strName == "?VerifyBarcode")
                    {
                        app.VerifyBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.UpperCaseItemBarcode 值
                    if (strName == "?UpperCaseItemBarcode")
                    {
                        app.UpperCaseItemBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.UpperCaseReaderBarcode 值
                    if (strName == "?UpperCaseReaderBarcode")
                    {
                        app.UpperCaseReaderBarcode = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyBookType 值
                    if (strName == "?VerifyBookType")
                    {
                        app.VerifyBookType = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyReaderType 值
                    if (strName == "?VerifyReaderType")
                    {
                        app.VerifyReaderType = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.BorrowCheckOverdue 值
                    if (strName == "?BorrowCheckOverdue")
                    {
                        app.BorrowCheckOverdue = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.CirculationNotifyTypes 值
                    if (strName == "?CirculationNotifyTypes")
                    {
                        app.CirculationNotifyTypes = strValue;
                        goto END1;
                    }

                    // 临时修改内存中的 app.AcceptBlankRoomName 值
                    if (strName == "?AcceptBlankRoomName")
                    {
                        app.AcceptBlankRoomName = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.VerifyRegisterNoDup 值
                    if (strName == "?VerifyRegisterNoDup")
                    {
                        app.AcceptBlankRoomName = DomUtil.IsBooleanTrue(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.PatronAdditionalFroms 值
                    if (strName == "?PatronAdditionalFroms")
                    {
                        app.PatronAdditionalFroms = StringUtil.SplitList(strValue);
                        goto END1;
                    }

                    // 临时修改内存中的 app.PatronAdditionalFields 值
                    if (strName == "?PatronAdditionalFields")
                    {
                        app.PatronAdditionalFields = StringUtil.SplitList(strValue);
                        goto END1;
                    }

                    // 设置<valueTables>元素
                    // strValue中是下级片断定义，没有<valueTables>元素作为根。
                    if (strName == "valueTables")
                    {
                        nRet = app.SetValueTablesXml(
                            sessioninfo.LibraryCodeList,
                            strValue,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        app.Changed = true;
                        app.ActivateManagerThread();
                        goto END1;
                    }

                    // 设置<rightsTable>元素
                    // strValue中是下级片断定义，没有<rightsTable>元素作为根。
                    if (strName == "rightsTable")
                    {
#if NO
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable");   // 0.02前为rightstable
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("rightsTable");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<rightsTable>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }
#endif
                        nRet = app.SetRightsTableXml(
    sessioninfo.LibraryCodeList,
    strValue,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        app.Changed = true;
                        app.ActivateManagerThread();

                        goto END1;
                    }

                    // 2008/10/10 
                    // 设置<locationtypes>元素
                    // strValue中是下级片断定义，没有<locationTypes>元素作为根。
                    /*
                     *  <locationTypes>
                            <item canborrow="yes">流通库</item>
                            <item>阅览室</item>
                        </locationTypes>
                     * */
                    if (strName == "locationTypes")
                    {
#if NO
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes"); // 0.02前为locationtypes
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("locationTypes");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<locationTypes>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }
#endif
                        nRet = app.SetLocationTypesXml(
                            sessioninfo.LibraryCodeList,
                            strValue,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        app.Changed = true;
                        app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2008/10/12 
                    // 设置<zhongcihao>元素
                    // strValue中是下级片断定义，没有<zhongcihao>元素作为根。
                    /*
                        <zhongcihao>
                            <nstable name="nstable">
                                <item prefix="marc" uri="http://dp2003.com/UNIMARC" />
                            </nstable>
                            <group name="中文书目" zhongcihaodb="种次号">
                                <database name="中文图书" leftfrom="索取类号" rightxpath="//marc:record/marc:datafield[@tag='905']/marc:subfield[@code='e']/text()" titlexpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='a']/text()" authorxpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f' or @code='g']/text()" />
                            </group>
                        </zhongcihao>
                     * */
                    if (strName == "zhongcihao")
                    {
                        // 分馆用户不能修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            strError = "分馆用户不允许修改<zhongcihao>元素定义";
                            goto ERROR1;
                        }

                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("zhongcihao");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("zhongcihao");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<zhongcihao>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2009/2/18 
                    // 设置<callNumber>元素
                    // strValue中是下级片断定义，没有<callNumber>元素作为根。
                    /*
            <callNumber>
                <group name="中文" zhongcihaodb="种次号">
                    <location name="基藏库" />
                    <location name="流通库" />
                </group>
                <group name="英文" zhongcihaodb="新种次号库">
                    <location name="英文基藏库" />
                    <location name="英文流通库" />
                </group>
            </callNumber>             * */
                    if (strName == "callNumber")
                    {
                        // 分馆用户可以修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            // 修改 <callNumber> 元素定义。本函数专用于分馆用户。全局用户可以直接修改这个元素的 InnerXml 即可
                            nRet = app.SetCallNumberXml(
                                sessioninfo.LibraryCodeList,
                                strValue,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            app.ActivateManagerThread();
                            goto END1;
                        }

                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("callNumber");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("callNumber");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<callNumber>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2009/3/9 
                    // 设置<dup>元素
                    // strValue中是下级片断定义，没有<dup>元素作为根。
                    /*
         <dup>
                <project name="采购查重" comment="示例方案">
                    <database name="测试书目库" threshold="60">
                        <accessPoint name="著者" weight="50" searchStyle="" />
                        <accessPoint name="题名" weight="70" searchStyle="" />
                        <accessPoint name="索书类号" weight="10" searchStyle="" />
                    </database>
                    <database name="编目库" threshold="60">
                        <accessPoint name="著者" weight="50" searchStyle="" />
                        <accessPoint name="题名" weight="70" searchStyle="" />
                        <accessPoint name="索书类号" weight="10" searchStyle="" />
                    </database>
                </project>
                <project name="编目查重" comment="这是编目查重示例方案">
                    <database name="中文图书" threshold="100">
                        <accessPoint name="责任者" weight="50" searchStyle="" />
                        <accessPoint name="ISBN" weight="80" searchStyle="" />
                        <accessPoint name="题名" weight="20" searchStyle="" />
                    </database>
                    <database name="图书测试" threshold="100">
                        <accessPoint name="责任者" weight="50" searchStyle="" />
                        <accessPoint name="ISBN" weight="80" searchStyle="" />
                        <accessPoint name="题名" weight="20" searchStyle="" />
                    </database>
                </project>
                <default origin="中文图书" project="编目查重" />
                <default origin="图书测试" project="编目查重" />
            </dup>             * */
                    if (strName == "dup")
                    {
                        // 分馆用户不能修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            strError = "分馆用户不允许修改<dup>元素定义";
                            goto ERROR1;
                        }

                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("dup");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("dup");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<dup>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        app.ActivateManagerThread();
                        goto END1;
                    }

                    // 2008/10/13 2019/5/31
                    // 设置 <script> 或 <barcodeValidation> 元素
                    // strValue中是下级片断定义，没有<script>元素作为根。
                    if (strName == "script" || strName == "barcodeValidation")
                    {
                        // 分馆用户不能修改定义
                        if (sessioninfo.GlobalUser == false)
                        {
                            strError = $"分馆用户不允许修改<{strName}>元素定义";
                            goto ERROR1;
                        }

                        bool changed = false;
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode(strName);
                        if (string.IsNullOrEmpty(strValue) == false)
                        {
                            if (root == null)
                            {
                                root = app.LibraryCfgDom.CreateElement(strName);
                                app.LibraryCfgDom.DocumentElement.AppendChild(root);
                                changed = true;
                            }
                        }
                        else
                        {
                            if (root != null)
                            {
                                root.ParentNode.RemoveChild(root);
                                changed = true;
                            }
                        }

                        try
                        {
                            root.InnerXml = ConvertCrLf(strValue);
                            changed = true;
                        }
                        catch (Exception ex)
                        {
                            strError = $"设置 <{strName}> 元素的 InnerXml 时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = changed;

                        if (strName == "script" && changed)
                        {
                            // 注意检测编译错误
                            // 初始化LibraryHostAssembly对象
                            // 必须在ReadersMonitor以前启动。否则其中用到脚本代码时会出错。2007/10/10 changed
                            // return:
                            //		-1	出错
                            //		0	脚本代码没有找到
                            //      1   成功
                            nRet = app.InitialLibraryHostAssembly(out strError);
                            if (nRet == -1)
                            {
                                app.ActivateManagerThread(); // 促使尽快保存
                                app.WriteErrorLog(strError);
                                goto ERROR1;
                            }

                        }
                        if (changed)
                            app.ActivateManagerThread();
                        goto END1;
                    }

                    strError = "(strCategory为 '" + strCategory + "' 时)未知的strName值 '" + strName + "' ";
                    goto ERROR1;
                }

                // OPAC检索
                if (strCategory == "opac")
                {
                    // 分馆用户不能修改定义
                    if (sessioninfo.GlobalUser == false)
                    {
                        strError = "分馆用户不允许修改OPAC查询参数定义";
                        goto ERROR1;
                    }

                    // 设置<virtualDatabases>元素
                    // strValue中是下级片断定义，没有<virtualDatabases>元素作为根。
                    if (strName == "databases")
                    {
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("virtualDatabases");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("virtualDatabases");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<virtualDatabases>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        app.ActivateManagerThread();

                        // 重新初始化虚拟库定义
                        app.vdbs = null;
                        nRet = app.InitialVdbs(sessioninfo.Channels,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        goto END1;
                    }

                    // 设置<browseformats>元素
                    // strValue中是下级片断定义，没有<browseformats>元素作为根。
                    if (strName == "browseformats")
                    {
                        XmlNode root = app.LibraryCfgDom.DocumentElement.SelectSingleNode("browseformats");
                        if (root == null)
                        {
                            root = app.LibraryCfgDom.CreateElement("browseformats");
                            app.LibraryCfgDom.DocumentElement.AppendChild(root);
                        }

                        try
                        {
                            root.InnerXml = strValue;
                        }
                        catch (Exception ex)
                        {
                            strError = "设置<browseformats>元素的InnerXml时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        app.Changed = true;
                        app.ActivateManagerThread();

                        // TODO: 刷新OPAC界面中的浏览格式列表？

                        goto END1;
                    }

                    // 2011/2/15
                    if (strName == "serverDirectory")
                    {
                        /*
                        XmlNode node = app.LibraryCfgDom.SelectSingleNode("//opacServer");
                        if (node == null)
                        {
                            node = app.LibraryCfgDom.CreateElement("opacServer");
                            app.LibraryCfgDom.DocumentElement.AppendChild(node);
                        }

                        DomUtil.SetAttr(node, "url", strValue);
                         * */
                        app.OpacServerUrl = strValue;
                        app.Changed = true;
                        app.ActivateManagerThread();
                        goto END1;
                    }

                    strError = "(strCategory为 '" + strCategory + "' 时)未知的strName值 '" + strName + "' ";
                    goto ERROR1;
                }

            END1:
                result.Value = nRet;
                if (WriteOperLog(strCategory,
                    strName,
                    strValue,
                    sessioninfo.LibraryCodeList,
                    out strError) == -1)
                    goto ERROR1;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetSystemParameter() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
            finally
            {
                app.UnlockForWrite();
            }
        }

        // 2020/8/28
        int WriteOperLog(string strCategory,
            string strName,
            string strValue,
            string strLibraryCodeList,
            out string strError)
        {
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");

            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation",
                "setSystemParameter");

            DomUtil.SetElementText(domOperLog.DocumentElement,
    "category",
    strCategory);

            DomUtil.SetElementText(domOperLog.DocumentElement,
"name",
strName);

            DomUtil.SetElementText(domOperLog.DocumentElement,
"value",
strValue);

            // 请求者所管辖的分馆代码列表
            DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCodeList",
strLibraryCodeList);

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
    sessioninfo.UserID);

            string strOperTime = app.Clock.GetClock();

            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTime);

            int nRet = app.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "GetSystemParameter() API 写入日志时发生错误: " + strError;
                return -1;
            }

            return 0;
        }

        static string ConvertCrLf(string strText)
        {
            strText = strText.Replace("\r\n", "\r");
            strText = strText.Replace("\n", "\r");
            return strText.Replace("\r", "\r\n");
        }

        // 应急恢复
        // return:
        //      result.Value    -1 错误 0 not found 1 found
        public LibraryServerResult UrgentRecover(
            string strXML)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("UrgentRecover", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "紧急恢复操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("urgentrecover", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "紧急恢复的操作被拒绝。不具备 urgentrecover 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 暂时限定全局用户才能进行
                if (sessioninfo.GlobalUser == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "紧急恢复的操作被拒绝。只有全局用户才能进行。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXML);
                }
                catch (Exception ex)
                {
                    strError = "装载日志记录 XML 进入 DOM 发生错误: " + ex.Message;
                    goto ERROR1;
                }

                string strOperation = DomUtil.GetElementText(dom.DocumentElement,
            "operation");
                if (strOperation == "borrow")
                {
                    // TODO: 检查读者库的管辖，和册记录的管辖问题
                    nRet = app.RecoverBorrow(sessioninfo.Channels,
                        RecoverLevel.Logic,
                        dom,
                        true,
                        out strError);
                }
                else if (strOperation == "return")
                {
                    // TODO: 检查读者库的管辖，和册记录的管辖问题
                    nRet = app.RecoverReturn(sessioninfo.Channels,
                        RecoverLevel.Logic,
                        dom,
                        true,
                        out strError);
                }
                else
                {
                    strError = "不能识别的日志操作类型 '" + strOperation + "'";
                    goto ERROR1;
                }

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library UrgentRecover() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 修复读者或者册记录
        // return:
        //      result.Value    -1 错误 0 not found 1 found
        public LibraryServerResult RepairBorrowInfo(
            string strAction,
            string strReaderBarcode,
            string strItemBarcode,
            string strConfirmItemRecPath,
            int nStart,
            int nCount,
            out int nProcessedBorrowItems,
            out int nTotalBorrowItems,
            out string strOutputReaderBarcode,
            out string[] aDupPath)
        {
            // string strError = "";
            nProcessedBorrowItems = 0;
            nTotalBorrowItems = 0;
            strOutputReaderBarcode = "";
            aDupPath = null;

            LibraryServerResult result = this.PrepareEnvironment("RepairBorrowInfo", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "修复借还信息的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("repairborrowinfo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修复借还信息的操作被拒绝。不具备 repairborrowinfo 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 暂时限定全局用户才能进行
                if (sessioninfo.GlobalUser == false)
                {
                    if (strAction == "upgradefromdt1000_crossref"
                        || strAction == "repairreaderside"
                        || strAction == "repairitemside")
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修复借还信息的操作被拒绝。只有全局用户才能进行 strAction 为 '" + strAction + "' 的操作。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                // 2008/8/3 
                if (strAction == "upgradefromdt1000_crossref")
                {
                    return app.CrossRefBorrowInfo(
                        // sessioninfo.Channels,
                        channel,
                        strReaderBarcode,
                        nStart,
                        nCount,
                        out nProcessedBorrowItems,
                        out nTotalBorrowItems);
                }

                if (strAction == "checkfromreader")
                {
                    // 检查一个读者记录的借还信息是否异常。
                    // result.Value
                    //      -1  错误。
                    //      0   检查无错。
                    //      1   检查发现有错。
                    return app.CheckReaderBorrowInfo(
                        // sessioninfo.Channels,
                        channel,
                        strReaderBarcode,
                        nStart,
                        nCount,
                        out nProcessedBorrowItems,
                        out nTotalBorrowItems);
                }

                if (strAction == "checkfromitem")
                {
                    // string strOutputReaderBarcode = "";

                    // 检查一个实体记录的借还信息是否异常。
                    // parameters:
                    //      strLockedReaderBarcode  外层已经加锁过的条码号。本函数根据这个信息，可以避免重复加锁。
                    //      exist_readerdom 已经装载入DOM的读者记录。其读者证条码号是strLockedReaderBarcode。如果提供了这个值，本函数会优化性能。
                    // result.Value
                    //      -1  出错。
                    //      0   实体记录中没有借阅信息，或者检查发现无错。
                    //      1   检查发现有错。
                    return app.CheckItemBorrowInfo(
                        // sessioninfo.Channels,
                        channel,
                        null,   // string strLockedReaderBarcode,
                        null,   // XmlDocument exist_readerdom,
                        null,   // string strExistReaderRecPath,
                        strItemBarcode,
                        strConfirmItemRecPath,   // strConfirmItemRecPath,
                        out strOutputReaderBarcode,
                        out aDupPath);
                }

                if (strAction == "repairreaderside")
                {
                    return app.RepairReaderSideError(
                        sessioninfo,
                        strReaderBarcode,
                        strItemBarcode,
                        strConfirmItemRecPath,
                        out aDupPath);
                }
                if (strAction == "repairitemside")
                {
                    return app.RepairItemSideError(
                        sessioninfo,
                        strReaderBarcode,
                        strItemBarcode,
                        strConfirmItemRecPath,
                        out aDupPath);
                }

                result.Value = -1;
                result.ErrorInfo = "不能识别的 strAction参数值 '" + strAction + "'。";
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library RepairBorrowInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 入馆登记
        // parameters:
        //      strReaderBarcode    读者证条码号
        //      strGateName 门的名字
        // return:
        //      result.Value    -1 错误 其他 本次的累计量
        public LibraryServerResult PassGate(
            string strReaderBarcode,
            string strGateName,
            string strResultTypeList,
            out string[] results)
        {
            results = null;

            LibraryServerResult result = this.PrepareEnvironment("PassGate", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "入馆登记操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("passgate", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "入馆登记的操作被拒绝。不具备 passgate 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.PassGate(
                    sessioninfo,
                    strReaderBarcode,
                    strGateName,
                    strResultTypeList,
                    out results);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library PassGate() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 创建押金交费请求
        // parameters:
        //      strAction   值为foregift return之一
        //      strReaderBarcode    读者证条码号
        //      strOutputReaderXml 返回修改后的读者记录
        //      strOutputID 返回本次创建的交费请求的 ID
        // return:
        //      result.Value    -1 错误 其他 本次的累计量
        public LibraryServerResult Foregift(
            string strAction,
            string strReaderBarcode,
            out string strOutputReaderXml,
            out string strOutputID)
        {
            strOutputReaderXml = "";
            strOutputID = "";

            LibraryServerResult result = this.PrepareEnvironment("Foregift", true, true, true);
            if (result.Value == -1)
                return result;

            // 对读者身份的判断
            if (sessioninfo.UserType == "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "押金操作被拒绝。作为读者不能进行此项操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            try
            {
                return app.Foregift(
                    sessioninfo,
                    strAction,
                    strReaderBarcode,
                    out strOutputReaderXml,
                    out strOutputID);
                /*
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strError;
                return result;
                 * */
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Foregift() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 创建租金交费请求
        // parameters:
        //      strReaderBarcode    读者证条码号
        //      strOutputReaderXml 返回修改后的读者记录
        //      strOutputID 返回本次创建的交费请求的 ID
        // return:
        //      result.Value    -1 错误 其他 本次的累计量
        public LibraryServerResult Hire(
            string strAction,
            string strReaderBarcode,
            out string strOutputReaderXml,
            out string strOutputID)
        {
            strOutputReaderXml = "";
            strOutputID = "";

            LibraryServerResult result = this.PrepareEnvironment("Hire", true, true, true);
            if (result.Value == -1)
                return result;

            // 对读者身份的判断
            if (sessioninfo.UserType == "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "租金操作被拒绝。作为读者不能进行此项操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            try
            {
                return app.Hire(
                    sessioninfo,
                    strAction,
                    strReaderBarcode,
                    out strOutputReaderXml,
                    out strOutputID);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Hire() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // Settlement 结算
        // parameters:
        //      ids    ID的集合
        // return:
        //      result.Value    -1 错误 其他 本次的累计量
        public LibraryServerResult Settlement(
            string strAction,
            string[] ids)
        {
            LibraryServerResult result = this.PrepareEnvironment("Settlement", true, true, true);
            if (result.Value == -1)
                return result;

            // 对读者身份的判断
            if (sessioninfo.UserType == "reader")
            {
                result.Value = -1;
                result.ErrorInfo = "结算操作被拒绝。作为读者不能进行此项操作";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            try
            {
                return app.Settlement(
                    sessioninfo,
                    strAction,
                    ids);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Settlement() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // (根据一定排架体系)检索出某一类的同类书的索取号
        // parameters:
        //      strQueryXml 返回本次所使用的检索式XML，供调试用
        public LibraryServerResult SearchOneClassCallNumber(
            string strArrangeGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml)
        {
            strQueryXml = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchOneClassCallNumber", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.SearchOneClassCallNumber(
                        sessioninfo,
                        strArrangeGroupName,
                        strClass,
                        strResultSetName,
                        out strQueryXml);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchOneClassCallNumber() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得索取号检索命中信息
        // parameters:
        //      lStart  返回命中结果集起始位置
        //      lCount  返回命中结果集的记录个数
        //      strBrowseInfoStyle  所返回的CallNumberSearchResult中包含哪些信息
        //      searchresults   包含记录信息的CallNumberSearchResult数组
        // 权限:   没有限制
        public LibraryServerResult GetCallNumberSearchResult(
            string strArrangeGroupName,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out CallNumberSearchResult[] searchresults)
        {
            searchresults = null;

            LibraryServerResult result = this.PrepareEnvironment("GetCallNumberSearchResult", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetCallNumberSearchResult(sessioninfo,
                    strArrangeGroupName,
                    strResultSetName,
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    strLang,
                    out searchresults);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetCallNumberSearchResult() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得种次号尾号
        public LibraryServerResult GetOneClassTailNumber(
            string strArrangeGroupName,
            string strClass,
            out string strTailNumber)
        {
            strTailNumber = "";

            LibraryServerResult result = this.PrepareEnvironment("GetOneClassTailNumber", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetOneClassTailNumber(
                    sessioninfo,
                    strArrangeGroupName,
                    strClass,
                    out strTailNumber);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetOneClassTailNumber() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置种次号尾号
        // parameters:
        //      strAction   "testpush"试探推动尾号。此时strTestNumber为要测试的号码
        //                  "increase"增量尾号。此时strTestNumber为缺省号码
        //                  "save"  保存尾号。此时strTestNumber为要保存的号码
        public LibraryServerResult SetOneClassTailNumber(
            string strAction,
            string strArrangeGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber)
        {
            strOutputNumber = "";

            LibraryServerResult result = this.PrepareEnvironment("SetOneClassTailNumber", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置种次号尾号的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("settailnumber", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置种次号尾号的操作被拒绝。不具备settailnumber权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.SetOneClassTailNumber(
                    sessioninfo,
                    strAction,
                    strArrangeGroupName,
                    strClass,
                    strTestNumber,
                    out strOutputNumber);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetOneClassTailNumber() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 检索同类书记录，返回种次号和摘要信息
        // parameters:
        //      strQueryXml 返回本次所使用的检索式XML，供调试用
        public LibraryServerResult SearchUsedZhongcihao(
            string strZhongcihaoGroupName,
            string strClass,
            string strResultSetName,
            out string strQueryXml)
        {
            strQueryXml = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchUsedZhongcihao", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.SearchUsedZhongcihao(
                        sessioninfo,
                        strZhongcihaoGroupName,
                        strClass,
                        strResultSetName,
                        out strQueryXml);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchUsedZhongcihao() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }


        // 获得种次号检索命中信息
        // parameters:
        //      lStart  返回命中结果集起始位置
        //      lCount  返回命中结果集的记录个数
        //      strBrowseInfoStyle  所返回的ZhongcihaoSearchResult中包含哪些信息
        //      searchresults   包含记录信息的ZhongcihaoSearchResult数组
        // 权限:   没有限制
        public LibraryServerResult GetZhongcihaoSearchResult(
            string strZhongcihaoGroupName,
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out ZhongcihaoSearchResult[] searchresults)
        {
            searchresults = null;

            LibraryServerResult result = this.PrepareEnvironment("GetZhongcihaoSearchResult", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetZhongcihaoSearchResult(sessioninfo,
                    strZhongcihaoGroupName,
                    strResultSetName,
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    strLang,
                    out searchresults);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetZhongcihaoSearchResult() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得种次号尾号
        public LibraryServerResult GetZhongcihaoTailNumber(
            string strZhongcihaoGroupName,
            string strClass,
            out string strTailNumber)
        {
            strTailNumber = "";

            LibraryServerResult result = this.PrepareEnvironment("GetZhongcihaoTailNumber", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetZhongcihaoTailNumber(
                    sessioninfo,
                    strZhongcihaoGroupName,
                    strClass,
                    out strTailNumber);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetZhongcihaoTailNumber() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 试探推动种次号尾号
        // parameters:
        //      strAction   "testpush"试探推动尾号。此时strTestNumber为要测试的号码
        //                  "increase"增量尾号。此时strTestNumber为缺省号码
        //                  "save"  保存尾号。此时strTestNumber为要保存的号码
        public LibraryServerResult SetZhongcihaoTailNumber(
            string strAction,
            string strZhongcihaoGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber)
        {
            strOutputNumber = "";

            LibraryServerResult result = this.PrepareEnvironment("SetZhongcihaoTailNumber", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置种次号尾号的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("settailnumber", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置种次号尾号的操作被拒绝。不具备settailnumber权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.SetZhongcihaoTailNumber(
                    sessioninfo,
                    strAction,
                    strZhongcihaoGroupName,
                    strClass,
                    strTestNumber,
                    out strOutputNumber);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetZhongcihaoTailNumber() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 书目查重
        // parameters:
        //      strOriginBiblioRecPath  发起的书目记录路径
        //      strOriginBiblioRecXml   发起的书目记录XML
        //      strProjectName  查重方案名
        //      strStyle    includeoriginrecord输出结果中包含发起记录(缺省为不包含)
        public LibraryServerResult SearchDup(
            string strOriginBiblioRecPath,
            string strOriginBiblioRecXml,
            string strProjectName,
            string strStyle,
            out string strUsedProjectName)
        {
            string strError = "";
            strUsedProjectName = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchDup", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    return app.SearchDup(
                            sessioninfo,
                            channel,
                            strOriginBiblioRecPath,
                            strOriginBiblioRecXml,
                            strProjectName,
                            strStyle,
                            out strUsedProjectName);
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchDup() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得查重检索命中结果
        // parameters:
        //      lStart  返回命中结果集起始位置
        //      lCount  返回命中结果集的记录个数
        //      strBrowseInfoStyle  所返回的DupSearchResult中包含哪些信息
        //      searchresults   包含记录信息的DupSearchResult数组
        // 权限:   没有限制
        public LibraryServerResult GetDupSearchResult(
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            out DupSearchResult[] searchresults)
        {
            searchresults = null;

            LibraryServerResult result = this.PrepareEnvironment("GetDupSearchResult", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.GetDupSearchResult(sessioninfo,
                    lStart,
                    lCount,
                    strBrowseInfoStyle,
                    out searchresults);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetDupSearchResult() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 列出查重方案信息
        // parameters:
        public LibraryServerResult ListDupProjectInfos(
            string strOriginBiblioDbName,
            out DupProjectInfo[] results)
        {
            results = null;

            LibraryServerResult result = this.PrepareEnvironment("ListDupProjectInfos", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                return app.ListDupProjectInfos(
                    strOriginBiblioDbName,
                    out results);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ListDupProjectInfos() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得实用库信息
        public LibraryServerResult GetUtilInfo(
            string strAction,
            string strDbName,
            string strFrom,
            string strKey,
            string strValueAttrName,
            out string strValue)
        {
            strValue = "";

            LibraryServerResult result = this.PrepareEnvironment("GetUtilInfo", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // TODO: 是否要检查数据库名确实属于当前已经定义的实用库

                return app.GetUtilInfo(
                    sessioninfo,
                    strAction,
                    strDbName,
                    strFrom,
                    strKey,
                    strValueAttrName,
                    out strValue);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetUtilInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置实用库信息
        // parameters:
        //      strRootElementName  根元素名。如果为空，系统自会用<r>作为根元素
        public LibraryServerResult SetUtilInfo(
            string strAction,
            string strDbName,
            string strFrom,
            string strRootElementName,
            string strKeyAttrName,
            string strValueAttrName,
            string strKey,
            string strValue)
        {
            LibraryServerResult result = this.PrepareEnvironment("SetUtilInfo", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 对读者身份的判断
                if (sessioninfo.UserType == "reader")
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置实用库记录信息的操作被拒绝。作为读者不能进行此项操作";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 权限判断
                if (StringUtil.IsInList("setutilinfo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "设置实用库记录信息的操作被拒绝。不具备setutilinfo权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // TODO: 是否要检查数据库名确实属于当前已经定义的实用库
                return app.SetUtilInfo(
                    sessioninfo,
                    strAction,
                    strDbName,
                    strFrom,
                    strRootElementName,
                    strKeyAttrName,
                    strValueAttrName,
                    strKey,
                    strValue);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetUtilInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 列出内核资源目录，或列出 dp2library 本地文件和目录
        public LibraryServerResult Dir(string strResPath,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out ResInfoItem[] items,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode)
        {
            items = null;
            kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            LibraryServerResult result = this.PrepareEnvironment("Dir", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    result.Value = -1;
                    result.ErrorInfo = "get channel error";
                    result.ErrorCode = ErrorCode.SystemError;
                    return result;
                }

                string strError = "";

                if (string.IsNullOrEmpty(strResPath) == false
                    && strResPath.StartsWith("!"))
                {
                    // 2016/11/6
                    // 列出本地文件目录
                    List<FileItemInfo> infos = null;
                    result = ListFile("list",
                        strResPath,
                        "", // pattern
                        lStart,
                        lLength,
                        out infos);
                    if (result.Value == -1)
                    {
                        return result;
                    }
                    List<ResInfoItem> item_list = new List<ResInfoItem>();
                    foreach (FileItemInfo info in infos)
                    {
                        ResInfoItem item = new ResInfoItem();
                        string name = info.Name.Substring(strResPath.Length - 1);
                        // 去掉第一个字符的 \ 或者 /
                        if (string.IsNullOrEmpty(name) == false
                            && (name[0] == '\\' || name[0] == '/'))
                            name = name.Substring(1);

                        item.Name = name;
                        /*
        public int Type;	// 类型：0 库 / 1 途径 / 4 cfgs / 5 file
                        // TODO: 将此定义移动到 DigitalPlatform.rms 中
                         * */
                        if (info.Size == -1)
                        {
                            item.Type = 4;
                            item.HasChildren = true;
                        }
                        else
                        {
                            item.Type = 5;
                            item.HasChildren = false;
                            item.TypeString = "size:" + info.Size.ToString();
                        }

                        item_list.Add(item);
                    }
                    items = item_list.ToArray();
                    return result;
                }
                else
                {
                    if (strResPath.StartsWith(KernelServerUtil.LOCAL_PREFIX) == true
                        && StringUtil.IsInList("managedatabase", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "不具备 managedatabase 权限，无法列出内核数据目录和文件";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    long lRet = channel.DoDir(
                        strResPath,
                        (int)lStart,
                        (int)lLength,
                        strLang,
                        strStyle,
                        out items,
                        out strError);
                    kernel_errorcode = channel.OriginErrorCode;
                    result.ErrorInfo = strError;
                    result.Value = lRet;
                    return result;
                }
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library Dir() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获取资源
        // parameters:
        //      strResPath  资源的路径。一般数据库记录为"数据库名/1"形态；而数据库记录所属的对象资源，则为"数据库名/object/0"形态
        //      nStart  本次要获得的byte开始位置
        //      nLength 本次要获得的byte个数
        //      strStyle    风格列表，为逗号分隔的字符串值列表。取值为data/metadata/timestamp/outputpath之一
        //                  data表示要在baContent参数内返回资源本体内容
        //                  metadata表示要在strMetadata参数内返回元数据内容
        //                  timestamp表示要在baOutputTimestam参数内返回资源的时间戳内容
        //                  outputpath表示要在strOutputResPath参数内返回实际记录路径内容
        //                  skipLog 表示不希望在 dp2library 范围内记入日志
        //                  clientAddress:xxxxxx 表示前端地址
        //      baContent   返回的byte数组
        //      strMetadata 返回的元数据内容
        //      strOutputResPath    返回的实际记录路径
        //      baOutputTimestamp   返回的资源时间戳
        // rights:
        //      需要 getres 权限
        // return:
        //      result.Value    -1 出错；0 成功
        public LibraryServerResult GetRes(string strResPath,
            long nStart,
            int nLength,
            string strStyle,
            out byte[] baContent,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp)
        {
            baContent = null;
            strMetadata = "";
            strOutputResPath = "";
            baOutputTimestamp = null;

            LibraryServerResult result = this.PrepareEnvironment("GetRes", true, true);
            if (result.Value == -1)
                return result;

            // Thread.Sleep(2000); // testing

            try
            {
                string strError = "";
                long lRet = 0;

                /*
2016/11/7 13:35:12 dp2Library GetRes() API出现异常: Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
   在 DigitalPlatform.rms.Client.ResPath.GetDbName(String strLongPath)
   在 dp2Library.LibraryService.GetRes(String strResPath, Int64 nStart, Int32 nLength, String strStyle, Byte[]& baContent, String& strMetadata, String& strOutputResPath, Byte[]& baOutputTimestamp)
                 * */
                // 2016/11/7
                if (string.IsNullOrEmpty(strResPath))
                {
                    strError = "strResPath 参数值不应为空";
                    goto ERROR1;
                }

                // '!' 前缀表示获取 dp2library 管辖的本地文件。
                if (string.IsNullOrEmpty(strResPath) == false
    && strResPath[0] == '!')
                {

                    // 检查用户使用 GetRes API 的权限
                    // return:
                    //      -1  error
                    //      0   不具备权限
                    //      1   具备权限
                    int nRet = app.CheckGetResRights(
                        sessioninfo,
                        sessioninfo.LibraryCodeList,
                        sessioninfo.RightsOrigin,
                        strResPath,
                        out string strLibraryCode,
                        out string strFilePath,
                        out strError);
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    if (nRet == -1)
                        goto ERROR1;
#if NO
                    if (StringUtil.IsInList("download", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "获取 dp2Library 文件被拒绝。不具备 download 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
#endif

                    // 下载本地文件
                    // TODO: 限制 nMaxLength 最大值
                    // parameters:
                    //      strStyle    "uploadedPartial" 表示操作都是针对已上载临时部分的。比如希望获得这个局部的长度，时间戳，等等
                    // return:
                    //      -2      文件不存在
                    //		-1      出错
                    //		>= 0	成功，返回最大长度
                    lRet = app.GetFile(
                        strFilePath,
                        nStart,
                        nLength,
                        100 * 1024,
                        strStyle,
                        out baContent,
                        out baOutputTimestamp,
                        out strError);
                    if (lRet == -2)
                    {
                        result.ErrorCode = ErrorCode.NotFound;
                        result.Value = -1;
                    }
                    else if (lRet == -1)
                    {
                        result.ErrorCode = ErrorCode.SystemError;
                        result.Value = -1;
                    }
                    else
                        result.Value = lRet;

                    // 压缩内容
                    // 2017/10/6
                    if (StringUtil.IsInList("gzip", strStyle)
                        && baContent != null && baContent.Length > 0)
                    {
                        baContent = ByteArray.CompressGzip(baContent);
                        result.ErrorCode = ErrorCode.Compressed;
                    }

                    result.ErrorInfo = strError;
                    strOutputResPath = strResPath;
                    return result;
                }

                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("getres", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "用户 " + sessioninfo.UserID + " 获取资源被拒绝。不具备 getres 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                // 判断资源读取权限
                {
                    string strFilePath = "";
                    string strLibraryCode = "";

                    // 检查用户使用 GetRes API 的权限
                    // return:
                    //      -1  error
                    //      0   不具备权限
                    //      1   具备权限
                    int nRet = app.CheckGetResRights(
                        sessioninfo,
                        sessioninfo.LibraryCodeList,
                        sessioninfo.RightsOrigin,
                        strResPath,
                        out strLibraryCode,
                        out strFilePath,
                        out strError);
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    if (nRet == -1)
                        goto ERROR1;
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                bool bIsReaderDb = false;

                // 2012/9/16
                // 需要限制检索读者库为当前管辖的范围
                // TODO: 读者身份是否还需要作更细致的限定?
                {
                    bool bIsReader = sessioninfo.UserType == "reader";
                    string strLibraryCode = "";
                    string strDbName = ResPath.GetDbName(strResPath);
                    bool bReaderDbInCirculation = true;
                    if (app.IsReaderDbName(strDbName,
                        out bReaderDbInCirculation,
                        out strLibraryCode) == true)
                    {
                        bIsReaderDb = true;

                        if (bIsReader == true)
                        {
                            DigitalPlatform.LibraryServer.LibraryApplication.ResPathType type = LibraryApplication.GetResPathType(strResPath);
                            if (type == DigitalPlatform.LibraryServer.LibraryApplication.ResPathType.Record || type == DigitalPlatform.LibraryServer.LibraryApplication.ResPathType.CfgFile)
                            {
                                strError = "读者身份不被允许用 GetRes() 来获得读者记录或数据库配置文件";
                                goto ERROR1;
                            }

                            // TODO: 对于获取对象的请求，要看读者是否具有管辖的其他读者，或者好友。否则只能获取自己的读者记录的对象
                        }

                        // 检查当前操作者是否管辖这个读者库
                        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                        if (app.IsCurrentChangeableReaderPath(strDbName + "/?",
                sessioninfo.LibraryCodeList) == false)
                        {
                            strError = "读者库 '" + strDbName + "' 不在当前用户管辖范围内";
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    else if (app.AmerceDbName == strDbName)
                    {
                        DigitalPlatform.LibraryServer.LibraryApplication.ResPathType type = LibraryApplication.GetResPathType(strResPath);
                        if (type == LibraryApplication.ResPathType.CfgFile)
                        {
                            // 获取配置文件的请求要被允许
                            // 2015/8/23 以前这里有个 bug，不允许用本 API 获得违约金库 cfgs 下的配置文件
                        }
                        else
                        {
                            if (bIsReader == true)
                            {
                                strError = "读者身份不被允许用GetRes()来获得违约金记录";
                                goto ERROR1;
                            }
                            strError = "不被允许用GetRes()来获得违约金记录";
                            goto ERROR1;
                        }
                    }
                }

                bool bWriteLog = false; // 是否需要记入操作日志
                bool bClearMetadata = false;    // 是否需要在返回前清除 strMetadata 内容
                string strXmlRecPath = "";
                string strObjectID = "";
                // 解析对象路径
                // parameters:
                //      strPathParam    等待解析的路径
                //      strXmlRecPath   返回元数据记录路径
                //      strObjectID     返回对象 ID
                // return:
                //      false   不是记录路径
                //      true    是记录路径
                StringUtil.ParseObjectPath(strResPath,
                    out strXmlRecPath,
                    out strObjectID);
                if (// app.GetObjectWriteToOperLog == true &&
                    StringUtil.IsInList("skipLog", strStyle) == false
                    && nStart == 0 // 获取全部二进制信息的循环中，只记载第一次 API 访问
                    && string.IsNullOrEmpty(strObjectID) == false
                    && StringUtil.IsInList("data", strStyle) == true)
                {
                    if (app.GetObjectWriteToOperLog == true)
                    {
                        bWriteLog = true;
                        // 为了获得对象的大小信息，需要得到 metadata
                        if (StringUtil.IsInList("metadata", strStyle) == false)
                        {
                            StringUtil.SetInList(ref strStyle, "metadata", true);
                            bClearMetadata = true;
                        }
                    }
                    else
                    {
                        // 限制最大事项数
                        string date = DateTimeUtil.DateTimeToString8(DateTime.Now);
                        long newValue = app.DailyItemCountTable.GetValue("warning_getres", date, 1);
                        if (newValue <= 1)
                        {
                            // 希望写入存取日志，但 library.xml 中的 object/@writeGetResOperLog 属性值为 false，此时需要记入错误日志表示警告
                            app.WriteErrorLog("警告：有前端请求 GetRes()，希望连带写入存取日志，但 library.xml 中的 object/@writeGetResOperLog 属性值为 false，导致写入存取日志动作被忽略");
                        }
                    }
                }

                // 访问读者库的动作不记入访问日志
                if (bWriteLog == true && bIsReaderDb == true)
                    bWriteLog = false;

                lRet = channel.GetRes(strResPath,
                    nStart,
                    nLength,
                    strStyle,
                    out baContent,
                    out strMetadata,
                    out strOutputResPath,
                    out baOutputTimestamp,
                    out strError);
                result.Value = lRet;
                result.ErrorInfo = strError;

                // 做错误码的翻译工作
                // 2008/7/28
                ConvertKernelErrorCode(channel.ErrorCode,
                    ref result);

                if (bWriteLog)
                {
                    string strClientAddress = StringUtil.GetStyleParam(strStyle, "clientAddress");

                    Hashtable table = StringUtil.ParseMetaDataXml(strMetadata,
                        out strError);
                    if (table != null)
                    {
                        Int64 v = 0;
                        if (app.Statis != null && Int64.TryParse((string)table["size"], out v) == true)
                            app.Statis.IncreaseEntryValue(
                            sessioninfo.LibraryCodeList,
                            "获取对象",
                            "尺寸",
                            v);
                    }

                    if (app.Statis != null)
                        app.Statis.IncreaseEntryValue(
                        sessioninfo.LibraryCodeList,
                        "获取对象",
                        "次",
                        1);
                    if (string.IsNullOrEmpty(app.MongoDbConnStr) == false)
                    {
                        long size = 0;
                        long.TryParse((string)table["size"], out size);

                        app.AccessLogDatabase.Add("getRes",
                            strResPath,
                            size,
                            (string)table["mimetype"],
                            string.IsNullOrEmpty(strClientAddress) == false ? strClientAddress : sessioninfo.ClientAddress,
                            1,
                            sessioninfo.UserID,
                            DateTime.Now,
                            app.AccessLogMaxCountPerDay);
                    }
                    else
                    {
                        XmlDocument domOperLog = new XmlDocument();
                        domOperLog.LoadXml("<root />");

                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "operation",
                            "getRes");
                        DomUtil.SetElementText(domOperLog.DocumentElement, "path",
        strResPath);
                        if (table != null)
                        {
                            DomUtil.SetElementText(domOperLog.DocumentElement, "size",
                                (string)table["size"]);
                            DomUtil.SetElementText(domOperLog.DocumentElement, "mime",
                                (string)table["mimetype"]);
                        }

                        DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                            sessioninfo.UserID);

                        string strOperTime = app.Clock.GetClock();

                        DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                            strOperTime);

                        int nRet = app.OperLog.WriteOperLog(domOperLog,
                            string.IsNullOrEmpty(strClientAddress) == false ? strClientAddress : sessioninfo.ClientAddress,
                            out strError);
                        if (nRet == -1)
                        {
                            if (bClearMetadata == true)
                                strMetadata = "";
                            strError = "GetRes() API 写入日志时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }
                }

                if (bClearMetadata == true)
                    strMetadata = "";

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetRes() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        public static void ConvertKernelErrorCode(ChannelErrorCode origin,
            ref LibraryServerResult result)
        {
            if (origin == ChannelErrorCode.AlreadyExist)
            {
                result.ErrorCode = ErrorCode.AlreadyExist;
                return;
            }
            if (origin == ChannelErrorCode.AlreadyExistOtherType)
            {
                result.ErrorCode = ErrorCode.AlreadyExistOtherType;
                return;
            }
            if (origin == ChannelErrorCode.ApplicationStartError)
            {
                result.ErrorCode = ErrorCode.ApplicationStartError;
                return;
            }
            if (origin == ChannelErrorCode.EmptyRecord)
            {
                result.ErrorCode = ErrorCode.EmptyRecord;
                return;
            }
            if (origin == ChannelErrorCode.None)
            {
                result.ErrorCode = ErrorCode.NoError;
                return;
            }
            if (origin == ChannelErrorCode.NotFound)
            {
                result.ErrorCode = ErrorCode.NotFound;
                return;
            }
            if (origin == ChannelErrorCode.NotFoundSubRes)
            {
                result.ErrorCode = ErrorCode.NotFoundSubRes;
                return;
            }
            if (origin == ChannelErrorCode.NotHasEnoughRights)
            {
                result.ErrorCode = ErrorCode.NotHasEnoughRights;
                return;
            }

            if (origin == ChannelErrorCode.OtherError)
            {
                result.ErrorCode = ErrorCode.OtherError;
                return;
            }
            if (origin == ChannelErrorCode.PartNotFound)
            {
                result.ErrorCode = ErrorCode.PartNotFound;
                return;
            }
            if (origin == ChannelErrorCode.RequestCanceled)
            {
                result.ErrorCode = ErrorCode.RequestCanceled;
                return;
            }
            if (origin == ChannelErrorCode.RequestCanceledByEventClose)
            {
                result.ErrorCode = ErrorCode.RequestCanceledByEventClose;
                return;
            }
            if (origin == ChannelErrorCode.RequestError)
            {
                result.ErrorCode = ErrorCode.RequestError;
                return;
            }
            if (origin == ChannelErrorCode.RequestTimeOut)
            {
                result.ErrorCode = ErrorCode.RequestTimeOut;
                return;
            }
            if (origin == ChannelErrorCode.TimestampMismatch)
            {
                result.ErrorCode = ErrorCode.TimestampMismatch;
                return;
            }

            // TODO: 其实可以用 Parse() 来翻译值
            if (origin == ChannelErrorCode.Compressed)
            {
                result.ErrorCode = ErrorCode.Compressed;
                return;
            }

            if (origin == ChannelErrorCode.NotLogin)
            {
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = "内核登录失败: " + result.ErrorInfo;
                return;
            }

            result.ErrorCode = ErrorCode.SystemError;
        }

        /*
        public static ErrorCode ConvertKernelErrorCode(ChannelErrorCode origin)
        {
            if (origin == ChannelErrorCode.AlreadyExist)
                return ErrorCode.AlreadyExist;
            if (origin == ChannelErrorCode.AlreadyExistOtherType)
                return ErrorCode.AlreadyExistOtherType;
            if (origin == ChannelErrorCode.ApplicationStartError)
                return ErrorCode.ApplicationStartError;
            if (origin == ChannelErrorCode.EmptyRecord)
                return ErrorCode.EmptyRecord;
            if (origin == ChannelErrorCode.None)
                return ErrorCode.NoError;
            if (origin == ChannelErrorCode.NotFound)
                return ErrorCode.NotFound;
            if (origin == ChannelErrorCode.NotFoundSubRes)
                return ErrorCode.NotFoundSubRes;
            if (origin == ChannelErrorCode.NotHasEnoughRights)
                return ErrorCode.NotHasEnoughRights;

            if (origin == ChannelErrorCode.OtherError)
                return ErrorCode.OtherError;
            if (origin == ChannelErrorCode.PartNotFound)
                return ErrorCode.PartNotFound;
            if (origin == ChannelErrorCode.RequestCanceled)
                return ErrorCode.RequestCanceled;
            if (origin == ChannelErrorCode.RequestCanceledByEventClose)
                return ErrorCode.RequestCanceledByEventClose;
            if (origin == ChannelErrorCode.RequestError)
                return ErrorCode.RequestError;
            if (origin == ChannelErrorCode.RequestTimeOut)
                return ErrorCode.RequestTimeOut;
            if (origin == ChannelErrorCode.TimestampMismatch)
                return ErrorCode.TimestampMismatch;

            if (origin == ChannelErrorCode.NotLogin)
                return ErrorCode.SystemError;

            return ErrorCode.SystemError;
        }
         * */

        // 写入资源
        // 注：写入 backup 或 cfgs 目录要求具备 backup 或者 managedatabase 权限；
        //      写入 upload 目录需要 upload 或者 managedatabase 权限
        //      写入 library.xml 需要 managedatabase 权限
        //      写入 cfgs/template 文件需要 writetemplate 权限
        //      写入数据库记录，需要 writerecord 权限
        //      写入数据库记录下的对象，需要 writeobject 权限
        //      写入其他(比如 库名/cfgs/配置文件名 )资源，需要 writeres 权限
        //      writeres 权限
        public LibraryServerResult WriteRes(
            string strResPath,
            string strRanges,
            long lTotalLength,
            byte[] baContent,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out string strOutputResPath,
            out byte[] baOutputTimestamp)
        {
            strOutputResPath = "";
            baOutputTimestamp = null;

            // TODO: 这里不检查 hangup。但具体功能细节里面可以检查。在 hangup 情况下，可以专门放行修改 library.xml 的请求，而其他请求此时不允许操作
            LibraryServerResult result = this.PrepareEnvironment("WriteRes", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                bool bWriteOperLog = true;
                string strError = "";
                string strLibraryCode = ""; // 实际写入操作的读者库馆代码

                {
                    // 检查用户使用 WriteRes API 的权限
                    // return:
                    //      -1  error
                    //      0   不具备权限
                    //      1   具备权限
                    int nRet = app.CheckWriteResRights(
                        sessioninfo.LibraryCodeList,
                        sessioninfo.RightsOrigin,
                        strResPath,
                        out strLibraryCode,
                        out strError);

                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    if (nRet == -1)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
                }

                if (string.IsNullOrEmpty(strResPath) == false
                    && strResPath[0] == '!')
                {
                    // TODO: 检查权限。如果权限足够，也可以访问某些 数据目录下的二级目录
                    string strTargetDir = app.DataDir;
                    string strFilePath = Path.Combine(strTargetDir, strResPath.Substring(1));
                    // return:
                    //      -2  时间戳不匹配
                    //      -1  一般性错误
                    //      0   成功
                    //      其他  成功删除的文件和目录个数
                    int nRet = app.WriteFile(
                        Path.Combine(app.DataDir, "upload"),    // TODO: ?? 这里要测试一下，可能请求意图写入 backup 子目录呢
                        strFilePath,
                        strRanges,
                        lTotalLength,
                        baContent,
                        strStyle,
                        baInputTimestamp,
                        out baOutputTimestamp,
                        out strError);
                    if (nRet == -2)
                    {
                        result.ErrorCode = ErrorCode.TimestampMismatch;
                        result.Value = -1;
                    }
                    else if (nRet == -1)
                    {
                        result.ErrorCode = ErrorCode.SystemError;
                        result.Value = -1;

                    }
                    else
                        result.Value = nRet;

                    result.ErrorInfo = strError;

                    strOutputResPath = strResPath;

                    bWriteOperLog = false;  // 不要写入日志
                }
                else
                {
                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "get channel error";
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }

                    long lRet = 0;
                    // 2015/9/3 新增删除资源的功能
                    if (StringUtil.IsInList("delete", strStyle) == true)
                    {
                        string strDbName = ResPath.GetDbName(strResPath);
                        if (ServerDatabaseUtility.IsUtilDbName(app.LibraryCfgDom, strDbName, "inventory") == true)
                        {
                            if (StringUtil.IsInList("inventorydelete", sessioninfo.RightsOrigin) == false)
                            {
                                result.Value = -1;
                                result.ErrorInfo = "当前用户缺乏删除盘点库记录 '" + strResPath + "' 所需要的 inventorydelete 权限";
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }
                            lRet = channel.DoDeleteRes(strResPath,
                                baInputTimestamp,
                                strStyle,   // 2017/9/16 增加
                                out baOutputTimestamp,
                                out strError);
                        }
                        else
                        {
                            result.Value = -1;
                            result.ErrorInfo = "删除资源 '" + strResPath + "' 的权限不够";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    else
                    {
                        lRet = channel.WriteRes(strResPath,
                            strRanges,
                            lTotalLength,
                            baContent,
                            strMetadata,
                            strStyle,
                            baInputTimestamp,
                            out strOutputResPath,
                            out baOutputTimestamp,
                            out strError);
                    }
                    result.Value = lRet;
                    result.ErrorInfo = strError;

                    // 2016/10/12
                    app.ClearCacheCfgs(strResPath);

                    // 做错误码的翻译工作
                    // 2008/7/28
                    // result.ErrorCode = ConvertKernelErrorCode(channel.ErrorCode);
                    ConvertKernelErrorCode(channel.ErrorCode,
        ref result);
                }

                // 2017/6/7
                if (StringUtil.IsInList("simulate", strStyle))
                    bWriteOperLog = false;

                if (bWriteOperLog == true
                    && result.Value != -1)  // 2014/4/21 调用出错的情况就不要写入日志了
                {
                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");

                    // 读者所在的馆代码。如果操作不涉及到读者库，则没有<libraryCode>元素
                    if (string.IsNullOrEmpty(strLibraryCode) == false)
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "libraryCode",
                            strLibraryCode);
                    }

                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "operation", "writeRes");

                    string strOperTimeString = app.Clock.GetClock();   // RFC1123格式

                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTimeString);

                    DomUtil.SetElementText(domOperLog.DocumentElement, "requestResPath",
                        strResPath);

                    if (StringUtil.IsInList("delete", strStyle) == false)
                    {
                        Debug.Assert(string.IsNullOrEmpty(strOutputResPath) == false, "");
                        DomUtil.SetElementText(domOperLog.DocumentElement, "resPath",
                            strOutputResPath);

                        DomUtil.SetElementText(domOperLog.DocumentElement, "ranges",
                            strRanges);
                        DomUtil.SetElementText(domOperLog.DocumentElement, "totalLength",
                            lTotalLength.ToString());
                        DomUtil.SetElementText(domOperLog.DocumentElement, "metadata",
                            strMetadata);
                    }

                    DomUtil.SetElementText(domOperLog.DocumentElement, "style",
    strStyle);

                    Stream attachment = null;
                    if (baContent != null && baContent.Length > 0)
                        attachment = new MemoryStream(baContent);
                    try
                    {
                        int nRet = app.OperLog.WriteOperLog(domOperLog,
                            sessioninfo.ClientAddress,
                            attachment,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "WriteRes() API 写入日志时发生错误: " + strError;
                            result.Value = -1;
                            result.ErrorCode = ErrorCode.SystemError;
                            result.ErrorInfo = strError;
                            return result;
                        }
                    }
                    finally
                    {
                        if (attachment != null)
                            attachment.Close();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library WriteRes() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        /// *** 评注相关功能
        /// 

        // TODO: 要能够获得一个特点范围的记录，或者根据指定的recpath集合获得若干评注记录
        // 获得评注信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      lStart  返回从第几个开始
        //      lCount  总共返回几个。0和-1都表示全部返回(0是为了兼容旧API)
        //      strStyle    "onlygetpath"   仅返回每个路径(OldRecPath)
        //                  "getfirstxml"   是对onlygetpath的补充，仅获得第一个元素的XML记录，其余的依然只返回路径
        //      commentinfos 返回的评注信息数组
        // 权限：需要有getcommentinfo权限
        public LibraryServerResult GetComments(
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,
            string strLang,
            out EntityInfo[] commentinfos)
        {
            commentinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("GetComments", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("getcommentinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "获得评注信息 操作被拒绝。不具备 getcommentinfo 或 order 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.CommentItemDatabase.GetItems(sessioninfo,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    strStyle,
                    strLang,
                    out commentinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetComments() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 设置/保存评注信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      commentinfos 要提交的的评注信息数组
        // 权限：需要有setcommentinfo权限
        // 日志：
        //      要产生日志
        public LibraryServerResult SetComments(
            string strBiblioRecPath,
            EntityInfo[] commentinfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = this.PrepareEnvironment("SetComments", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("setcommentinfo", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "保存评注信息 操作被拒绝。不具备setcommentinfo权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                return app.CommentItemDatabase.SetItems(sessioninfo,
                    strBiblioRecPath,
                    commentinfos,
                    out errorinfos);
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetComments() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        static int GetParentPath(LibraryApplication app,
            XmlDocument item_dom,
            string strItemRecPath,
            out string strBiblioDbName,
            out string strOutputBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioDbName = "";
            strOutputBiblioRecPath = "";

            string strCommentDbName = ResPath.GetDbName(strItemRecPath);

#if NO
            // 需要检查一下数据库名是否在允许的评注库名之列
            if (app.IsCommentDbName(strCommentDbName) == false)
            {
                strError = "评注记录路径 '" + strCommentRecPath + "' 中的数据库名 '" + strCommentDbName + "' 不在配置的评注库名之列，因此拒绝操作。";
                return -1;
            }

            // 根据评注库名, 找到对应的书目库名
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = app.GetBiblioDbNameByCommentDbName(strCommentDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;
#endif
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = app.GetBiblioDbNameByChildDbName(strCommentDbName,
                out strBiblioDbName,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            string strRootID = DomUtil.GetElementText(item_dom.DocumentElement,
                "parent");
            if (String.IsNullOrEmpty(strRootID) == true)
            {
                strError = "下级记录 " + strItemRecPath + " 中没有<parent>元素值，因此无法定位其从属的书目记录";
                return -1;
            }
            string strBiblioRecPath = strBiblioDbName + "/" + strRootID;
            strOutputBiblioRecPath = strBiblioRecPath;
            return 0;
        }

        // TODO: 建议主体移动到ItemDatabase中，可以节省多种类的代码
        // 获得评注信息
        // parameters:
        //      strRefID  参考ID。特殊情况下，可以使用"@path:"引导的评注记录路径(只需要库名和id两个部分)作为检索入口。在@path引导下，路径后面还可以跟随 "$prev"或"$next"表示方向
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据。
        //      strOutputBiblioRecPath  输出的书目记录路径。当strIndex的第一字符为'@'时，strBiblioRecPath必须为空，函数返回后，strOutputBiblioRecPath中会包含从属的书目记录路径
        // return:
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有getcommentinfo权限
        public LibraryServerResult GetCommentInfo(
            string strRefID,
            // string strBiblioRecPath,
            string strItemXml,  // 前端提供给服务器的记录内容。例如，需要模拟创建检索点，就需要前端提供记录内容
            string strResultType,
            out string strResult,
            out string strCommentRecPath,
            out byte[] comment_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath)
        {
            strResult = "";
            strBiblio = "";
            strCommentRecPath = "";
            comment_timestamp = null;
            strOutputBiblioRecPath = "";

            LibraryServerResult result = this.PrepareEnvironment("GetCommentInfo", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("getcommentinfo,order", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    // result.ErrorInfo = "获取评注信息被拒绝。不具备 getcommentinfo 或 order 权限。";
                    result.ErrorInfo = "用户 '" + sessioninfo.UserID + "' 获取评注信息被拒绝。不具备 getcommentinfo 或 order 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }


                int nRet = 0;
                long lRet = 0;

                string strXml = "";
                string strError = "";

                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    strError = "strIndex参数不能为空";
                    goto ERROR1;
                }

                string strBiblioDbName = "";
                // string strCommentDbName = "";
                // string strRootID = "";
                XmlDocument comment_dom = null;

                // 获得一条册、期、订购、评注记录
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetItemXml(
                    "comment",
                    strRefID,
                    strItemXml,
                    ref strCommentRecPath,
                    ref comment_timestamp,
                    ref strBiblioDbName,
                    ref strOutputBiblioRecPath,
                    ref result,
                    out strXml,
                    out comment_dom,
                    out strError);
                if (nRet == 0)
                    return result;
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // 前端提供临时记录
                if (strRefID[0] == '<')
                {
                    strXml = strRefID;
                    // strOutputPath = "?";
                    // TODO: 数据库名需要从前端发来的XML记录中获取?
                    {
                        comment_dom = new XmlDocument();
                        try
                        {
                            comment_dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "评注记录XML装载到DOM出错:" + ex.Message;
                            goto ERROR1;
                        }
                    }

                    strCommentRecPath = DomUtil.GetElementText(comment_dom.DocumentElement, "_recPath");

                    if (string.IsNullOrEmpty(strCommentRecPath) == false)
                    {
                        nRet = GetCommentParentPath(app,
                comment_dom,
                strCommentRecPath,
                out strBiblioDbName,
                out strOutputBiblioRecPath,
                out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strCommentRecPath = "?";

                    goto SKIP1;
                }

                // 命令状态
                if (strRefID[0] == '@')
                {
#if NO
                    if (String.IsNullOrEmpty(strBiblioRecPath) == false)
                    {
                        strError = "当strIndex参数为'@'引导的命令形态时，strBiblioRecPath参数必须为空";
                        goto ERROR1;
                    }
#endif

                    // 获得评注记录，通过评注记录路径
                    if (StringUtil.HasHead(strRefID, "@path:") == true)
                    {
                        strCommentRecPath = strRefID.Substring("@path:".Length);

                        // 继续分离出(方向)命令部分
                        string strCommand = "";
                        nRet = strCommentRecPath.IndexOf("$");
                        if (nRet != -1)
                        {
                            strCommand = strCommentRecPath.Substring(nRet + 1);
                            strCommentRecPath = strCommentRecPath.Substring(0, nRet);
                        }

#if NO
                        strCommentDbName = ResPath.GetDbName(strCommentRecPath);
                        // 需要检查一下数据库名是否在允许的评注库名之列
                        if (app.IsCommentDbName(strCommentDbName) == false)
                        {
                            strError = "评注记录路径 '" + strCommentRecPath + "' 中的数据库名 '" + strCommentDbName + "' 不在配置的评注库名之列，因此拒绝操作。";
                            goto ERROR1;
                        }
#endif

                        string strMetaData = "";
                        string strTempOutputPath = "";

                        RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                        if (channel == null)
                        {
                            strError = "get channel error";
                            goto ERROR1;
                        }

                        string strStyle = "content,data,metadata,timestamp,outputpath";

                        // 为了便于处理对象资源
                        strStyle += ",withresmetadata";

                        if (String.IsNullOrEmpty(strCommand) == false
                            && (strCommand == "prev" || strCommand == "next"))
                        {
                            strStyle += "," + strCommand;
                        }

                        lRet = channel.GetRes(strCommentRecPath,
    strStyle,
    out strXml,
    out strMetaData,
    out comment_timestamp,
    out strTempOutputPath,
    out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            {
                                result.Value = 0;
                                if (strCommand == "prev")
                                    result.ErrorInfo = "到头";
                                else if (strCommand == "next")
                                    result.ErrorInfo = "到尾";
                                else
                                    result.ErrorInfo = "没有找到";
                                result.ErrorCode = ErrorCode.NotFound;
                                return result;
                            }
                            goto ERROR1;
                        }

                        strCommentRecPath = strTempOutputPath;

                    }
                    else if (StringUtil.HasHead(strRefID, "@refid:") == true)
                    {
                        string strTempRefID = strRefID.Substring("@refid:".Length);

                        string strTempOutputPath = "";                            // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条(即便在这种情况下, strOutputPath也返回了第一条的路径)
                        nRet = app.GetCommentRecXml(
                            sessioninfo.Channels,
                            strTempRefID,
                out strXml,
                out strTempOutputPath,
                out comment_timestamp,
                out strError);
                        if (nRet == -1)
                        {
                            strError = "用refid '" + strRefID + "' 检索评注记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "不支持的检索词格式: '" + strRefID + "'。目前仅支持'@path:'和'@refid:'引导的检索词";
                        goto ERROR1;
                    }

                    // 从订购记录<parent>元素中取得书目记录的id，然后拼装成书目记录路径放入strOutputBiblioRecPath
                    if (comment_dom == null)
                    {
                        comment_dom = new XmlDocument();
                        try
                        {
                            comment_dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "记录 " + strCommentRecPath + " 的XML装入DOM时出错: " + ex.Message;
                            goto ERROR1;
                        }
                    }

#if NO
                    // 根据评注库名, 找到对应的书目库名
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = app.GetBiblioDbNameByCommentDbName(strCommentDbName,
                        out strBiblioDbName,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;

                    strRootID = DomUtil.GetElementText(comment_dom.DocumentElement,
                        "parent");
                    if (String.IsNullOrEmpty(strRootID) == true)
                    {
                        strError = "评注记录 " + strCommentRecPath + " 中没有<parent>元素值，因此无法定位其从属的书目记录";
                        goto ERROR1;
                    }
                    string strBiblioRecPath = strBiblioDbName + "/" + strRootID;
                    strOutputBiblioRecPath = strBiblioRecPath;
#endif
                    nRet = GetCommentParentPath(app,
comment_dom,
strCommentRecPath,
out strBiblioDbName,
out strOutputBiblioRecPath,
out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    result.ErrorInfo = "";
                    result.Value = 1;
                } // 命令状态结束
                else
                {
                    // strOutputBiblioRecPath = strBiblioRecPath;


                    List<string> PathList = null;

                    // byte[] timestamp = null;
                    // 获得册记录
                    // 本函数可获得超过1条以上的路径
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条

                    /*
                    List<string> locateParam = new List<string>();
                    locateParam.Add(strCommentDbName);
                    locateParam.Add(strRootID);
                    locateParam.Add(strIndex);
                     * */

                    List<string> locateParam = null;

                    nRet = app.CommentItemDatabase.BuildLocateParam(
                        // strBiblioRecPath,
                        strRefID,
                        out locateParam,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nRet = app.CommentItemDatabase.GetItemRecXml(
                            sessioninfo.Channels,
                            locateParam,
                            "withresmetadata",
                            out strXml,
                            100,
                            out PathList,
                            out comment_timestamp,
                            out strError);

                    if (nRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "没有找到";
                        result.ErrorCode = ErrorCode.NotFound;
                        return result;
                    }

                    if (nRet == -1)
                        goto ERROR1;

                    strCommentRecPath = StringUtil.MakePathList(PathList);

                    result.ErrorInfo = strError;
                    result.Value = nRet;    // 可能会多于1条
                }

            SKIP1:
#endif


                // 若需要同时取得种记录
                if (String.IsNullOrEmpty(strBiblioType) == false)
                {



#if NO
                    string strBiblioRecID = "";
                    strBiblioRecID = DomUtil.GetElementText(comment_dom.DocumentElement, "parent"); //
                    if (String.IsNullOrEmpty(strBiblioRecID) == true)
                    {
                        strError = "评注记录XML中<parent>元素缺乏或者值为空, 因此无法定位种记录";
                        goto ERROR1;
                    }

                    strOutputBiblioRecPath = strBiblioDbName + "/" + strBiblioRecID;
#endif

                    Debug.Assert(string.IsNullOrEmpty(strOutputBiblioRecPath) == false, "");
                    Debug.Assert(comment_dom != null, "");
#if NO
                    if (string.IsNullOrEmpty(strOutputBiblioRecPath) == true)
                    {
                        if (comment_dom == null)
                        {
                            comment_dom = new XmlDocument();
                            try
                            {
                                comment_dom.LoadXml(strXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "评注记录XML装载到DOM出错:" + ex.Message;
                                goto ERROR1;
                            }
                        }

                        nRet = GetParentPath(app,
    comment_dom,
    strCommentRecPath,
    out strBiblioDbName,
    out strOutputBiblioRecPath,
    out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
#endif

                    string strBiblioXml = "";

                    if (String.Compare(strBiblioType, "recpath", true) == 0)
                    {
                        // 如果仅仅需要获得书目记录recpath，则不需要获得书目记录
                        goto DO_COMMENT;
                    }

                    RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                    if (channel == null)
                    {
                        strError = "channel == null";
                        goto ERROR1;
                    }
                    string strMetaData = "";
                    byte[] timestamp = null;
                    string strTempOutputPath = "";
                    lRet = channel.GetRes(strOutputBiblioRecPath,
                        out strBiblioXml,
                        out strMetaData,
                        out timestamp,
                        out strTempOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得种记录 '" + strOutputBiblioRecPath + "' 时出错: " + strError;
                        goto ERROR1;
                    }

                    // 如果只需要种记录的XML格式
                    if (String.Compare(strBiblioType, "xml", true) == 0)
                    {
                        strBiblio = strBiblioXml;
                        goto DO_COMMENT;
                    }


                    // 需要从内核映射过来文件
                    string strLocalPath = "";

                    if (String.Compare(strBiblioType, "html", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else if (String.Compare(strBiblioType, "text", true) == 0)
                    {
                        nRet = app.MapKernelScriptFile(
                            sessioninfo,
                            strBiblioDbName,
                            "./cfgs/loan_biblio_text.fltx",
                            out strLocalPath,
                            out strError);
                    }
                    else
                    {
                        strError = "不能识别的strBiblioType类型 '" + strBiblioType + "'";
                        goto ERROR1;
                    }

                    if (nRet == -1)
                        goto ERROR1;

                    // 将种记录数据从XML格式转换为HTML格式
                    string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                    if (string.IsNullOrEmpty(strBiblioXml) == false)
                    {
                        nRet = app.ConvertBiblioXmlToHtml(
                            strFilterFileName,
                            strBiblioXml,
                                    null,
                            strOutputBiblioRecPath,
                            out strBiblio,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                        strBiblio = "";
                }

            DO_COMMENT:
                // 取得评注信息
                if (String.IsNullOrEmpty(strResultType) == true
                    || String.Compare(strResultType, "recpath", true) == 0)
                {
                    strResult = ""; // 不返回任何结果
                }
                else if (String.Compare(strResultType, "xml", true) == 0)
                {
                    strResult = strXml;
                }
                else if (String.Compare(strResultType, "html", true) == 0)
                {
                    // 将评注记录数据从XML格式转换为HTML格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\commentxml2html.cs",
                        app.CfgDir + "\\commentxml2html.cs.ref",
                        strXml,
                        strCommentRecPath,  // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (String.Compare(strResultType, "text", true) == 0)
                {
                    // 将评注记录数据从XML格式转换为text格式
                    nRet = app.ConvertItemXmlToHtml(
                        app.CfgDir + "\\commentxml2text.cs",
                        app.CfgDir + "\\commentxml2text.cs.ref",
                        strXml,
                        strCommentRecPath,  // 2009/10/18 
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                // 模拟创建检索点
                else if (String.Compare(strResultType, "keys", true) == 0)
                {
                    nRet = app.GetKeys(sessioninfo,
                        strCommentRecPath,
                        strItemXml,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    strError = "未知的评注记录结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }

                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetCommentInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // *** 此API已经废止 ***
        // 对(评注)编号进行查重
        // parameters:
        //          strIndex  实际上要在这里使用参考ID。2012/4/6
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限:   需要具有searchcommentdup权限
        public LibraryServerResult SearchCommentDup(string strIndex,
            string strBiblioRecPath,
            int nMax,
            out string[] paths)
        {
            paths = null;

            LibraryServerResult result = this.PrepareEnvironment("SearchCommentDup", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限字符串
                if (StringUtil.IsInList("searchcomment", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "对评注记录的参考ID查重被拒绝。不具备searchcomment权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                int nRet = 0;
                string strError = "";

                List<string> locateParam = null;

                nRet = app.CommentItemDatabase.BuildLocateParam(
                    // strBiblioRecPath,
                    strIndex,
                    out locateParam,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    return app.CommentItemDatabase.SearchItemDup(
                    // sessioninfo.Channels,
                    channel,
                    locateParam,
                    nMax,
                    out paths);
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchCommentDup() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 检索评注信息
        // parameters:
        //      strQueryWord    检索词
        //      strFrom 检索途径
        //      strMathStyle    匹配方式 exact left right middle
        // 权限: 
        //      需要 searchcomment 权限
        // return:
        //      result.Value    命中结果总数。如果为-1，则表示检索出错
        public LibraryServerResult SearchComment(
            string strCommentDbName,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SearchComment", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // 权限判断

                // 权限字符串
                if (StringUtil.IsInList("searchcomment", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "检索评注信息被拒绝。不具备searchcomment权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                List<string> dbnames = new List<string>();

                if (String.IsNullOrEmpty(strCommentDbName) == true
                    || strCommentDbName == "<全部>"
                    || strCommentDbName.ToLower() == "<all>")
                {
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strDbName = app.ItemDbs[i].CommentDbName;
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;
                        dbnames.Add(strDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何评注库";
                        goto ERROR1;
                    }
                }
                else if (strCommentDbName == "<全部期刊>"
|| strCommentDbName.ToLower() == "<all series>")
                {
                    // 2012/9/3
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strCurrentCommentDbName = app.ItemDbs[i].CommentDbName;
                        string strCurrentIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strCurrentCommentDbName) == true)
                            continue;

                        if (String.IsNullOrEmpty(strCurrentIssueDbName) == true)
                            continue;

                        dbnames.Add(strCurrentCommentDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何期刊评注库";
                        goto ERROR1;
                    }
                }
                else if (strCommentDbName == "<全部图书>"
                    || strCommentDbName.ToLower() == "<all book>")
                {
                    // 2012/9/3
                    for (int i = 0; i < app.ItemDbs.Count; i++)
                    {
                        string strCurrentCommentDbName = app.ItemDbs[i].CommentDbName;
                        string strCurrentIssueDbName = app.ItemDbs[i].IssueDbName;

                        if (String.IsNullOrEmpty(strCurrentCommentDbName) == true)
                            continue;

                        // 大书目库中必须不包含期库，说明才是图书用途
                        if (String.IsNullOrEmpty(strCurrentIssueDbName) == false)
                            continue;

                        dbnames.Add(strCurrentCommentDbName);
                    }

                    if (dbnames.Count == 0)
                    {
                        strError = "没有发现任何图书评注库";
                        goto ERROR1;
                    }
                }
                else
                {
                    string[] splitted = strCommentDbName.Split(new char[] { ',' });
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        string strDbName = splitted[i];
                        if (String.IsNullOrEmpty(strDbName) == true)
                            continue;

                        if (app.IsCommentDbName(strDbName) == false)
                        {
                            strError = "库名 '" + strDbName + "' 不是合法的评注库名";
                            goto ERROR1;
                        }

                        dbnames.Add(strDbName);
                    }

                }

                bool bDesc = StringUtil.IsInList("desc", strSearchStyle);

                // 构造检索式
                string strQueryXml = "";
                for (int i = 0; i < dbnames.Count; i++)
                {
                    string strDbName = dbnames[i];

                    Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "");

                    string strRelation = "=";
                    string strDataType = "string";

                    if (strFrom == "__id")
                    {
                        // 如果为范围式
                        if (String.IsNullOrEmpty(strQueryWord) == false // 2013/3/25
                            && strQueryWord.IndexOf("-") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else if (String.IsNullOrEmpty(strQueryWord) == false)
                        {
                            strDataType = "number";

                            // 2012/8/20
                            strMatchStyle = "exact";
                        }
                    }
                    else if (strFrom == "最后修改时间")
                    {
                        // 如果为范围式
                        if (strQueryWord.IndexOf("~") != -1)
                        {
                            strRelation = "range";
                            strDataType = "number";
                        }
                        else
                        {
                            strDataType = "number";

                            // 如果检索词为空，并且匹配方式为前方一致、中间一致、后方一致，那么认为这是意图要命中全部记录
                            // 注意：如果检索词为空，并且匹配方式为精确一致，则需要认为是获取空值，也就是不存在对应检索点的记录
                            if (strMatchStyle != "exact" && string.IsNullOrEmpty(strQueryWord) == true)
                            {
                                strMatchStyle = "exact";
                                strRelation = "range";
                                strQueryWord = "~";
                            }
                        }
                        // 最后统一修改为exact。不能在一开始修改，因为strMatchStyle值还有帮助判断的作用
                        strMatchStyle = "exact";
                    }

                    string strOneDbQuery = "<target list='"
                        + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)    // 2007/9/14 
                        + "'><item>"
                        + (bDesc == true ? "<order>DESC</order>" : "")
                        + "<word>"
                        + StringUtil.GetXmlStringSimple(strQueryWord)
                        + "</word><match>" + strMatchStyle + "</match><relation>" + strRelation + "</relation><dataType>" + strDataType + "</dataType><maxCount>" + nPerMax.ToString() + "</maxCount></item><lang>" + strLang + "</lang></target>";

                    if (i > 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneDbQuery;
                }

                if (dbnames.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                BeginSearch();
                channel.Idle += new IdleEventHandler(channel_IdleEvent);
                try
                {
                    WriteDebugInfo("begin search " + strQueryXml);
                    long lRet = channel.DoSearch(strQueryXml,
                        strResultSetName,
                        strOutputStyle,
                        out strError);
                    WriteDebugInfo("end search lRet=" + lRet.ToString() + " " + strQueryXml);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        result.Value = 0;
                        result.ErrorInfo = "not found";
                        return result;
                    }

                    result.Value = lRet;
                    result.ErrorInfo = "";
                }
                finally
                {
                    channel.Idle -= new IdleEventHandler(channel_IdleEvent);
                    EndSearch();
                }

                return result;

            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SearchComment() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 获得消息
        public LibraryServerResult GetMessage(
            string[] message_ids,
            MessageLevel messagelevel,
            out List<MessageData> messages)
        {
            string strError = "";
            messages = null;

            LibraryServerResult result = this.PrepareEnvironment("GetMessage", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                if (message_ids.Length == 2 && message_ids[0] == "!msmq")
                {
                    if (StringUtil.IsInList("getmsmqmessage", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "从 MSMQ 获取消息被拒绝。当前用户不具备 getmsmqmessage 权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    if (app.operLogThread == null || app.operLogThread.MqNotifyEnabled == false)
                    {
                        strError = "dp2library circulation MSMQ 通知功能尚未启用";
                        goto ERROR1;
                    }

                    // action=get,count=10
                    // action=remove,count=10
                    Hashtable table = StringUtil.ParseParameters(message_ids[1]);
                    string strAction = (string)table["action"];
                    string strCount = (string)table["count"];
                    int nCount = 0;
                    if (Int32.TryParse(strCount, out nCount) == false)
                    {
                        strError = "count 子参数 '" + strCount + "' 格式不正确。应该为纯数字";
                        goto ERROR1;
                    }

                    if (strAction == "get")
                    {
                        TimeSpan timeout = new TimeSpan(0, 1, 0);   // 最多等待一分钟

                        messages = app.operLogThread.GetMessage(nCount, timeout);
                        return result;
                    }
                    else if (strAction == "remove")
                    {
                        app.operLogThread.RemoveMessage(nCount);
                        return result;
                    }
                    else
                    {
                        strError = "无法识别的 action 子参数 '" + strAction + "'";
                        goto ERROR1;
                    }
                }

                int nRet = app.MessageCenter.GetMessage(
        sessioninfo.Channels,
        sessioninfo.UserID,
        message_ids,
        messagelevel,
        out messages,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetMessage() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 列出消息
        //      strStyle    search / untouched / touched
        //                  有search表示进行检索和获取，没有search就表示不检索而获取先前检索的结果集。
        //                  untoched和touched应当和search联用。否则只能获取先前的结果数
        public LibraryServerResult ListMessage(
            string strStyle,
            string strResultsetName,
            string strBoxType,
            MessageLevel messagelevel,
            int nStart,
            int nCount,
            out int nTotalCount,
            out List<MessageData> messages)
        {
            nTotalCount = 0;
            messages = null;
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("ListMessage", true, true);
            if (result.Value == -1)
                return result;

            try
            {
                int nRet = app.MessageCenter.GetMessage(
                sessioninfo.Channels,
                strResultsetName,
                strStyle,
                sessioninfo.UserID,
                strBoxType,
                    messagelevel,
                nStart,
                nCount,
                out nTotalCount,
                out messages,
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;

            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ListMessage() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

#if NO
        // 发送消息
        LibraryServerResult SendMessage(
            string strRecipient,
            string strSubject,
            string strMime,
            string strBody)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            if (sessioninfo.UserID == "")
            {
                result.Value = -1;
                result.ErrorInfo = "尚未登录";
                result.ErrorCode = ErrorCode.NotLogin;
                return result;
            }

            int nRet = app.MessageCenter.SendMessage(
            sessioninfo.Channels,
            strRecipient,
            sessioninfo.UserID,
            strSubject,
            strMime,
            strBody,
            true,
            out strError);
            if (nRet == -1)
                goto ERROR1;

            result.Value = nRet;
            result.ErrorInfo = strError;
            return result;

        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }
#endif

        // parameters:
        //      strAction   "save" "delete" "send"
        public LibraryServerResult SetMessage(string strAction,
            string strStyle,
            List<MessageData> messages,
            out List<MessageData> output_messages)
        {
            string strError = "";
            output_messages = null;

            LibraryServerResult result = this.PrepareEnvironment("SetMessage", true, true, true);
            if (result.Value == -1)
                return result;

            try
            {
                int nRet = 0;

                // TODO: 应考虑权限问题。
                if (strAction == "delete")
                {
                    bool bMoveToRecycleBin = StringUtil.IsInList("movetorecyclebin", strStyle);
                    List<string> ids = new List<string>();
                    List<byte[]> timestamps = new List<byte[]>();

                    foreach (MessageData data in messages)
                    {
                        ids.Add(data.strRecordID);
                        timestamps.Add(data.TimeStamp);
                    }

                    nRet = app.MessageCenter.DeleteMessage(
                        bMoveToRecycleBin,
                        sessioninfo.Channels,
                        ids,
                        timestamps,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (strAction == "deleteall")
                {
                    bool bMoveToRecycleBin = StringUtil.IsInList("movetorecyclebin", strStyle);

                    // 除去 movetorecyclebin 以后就是boxtype字符串
                    StringUtil.RemoveFromInList("movetorecyclebin", true, ref strStyle);

                    // 删除一个box中的全部消息
                    nRet = app.MessageCenter.DeleteMessage(
                        sessioninfo.Channels,
                        sessioninfo.UserID,
                        bMoveToRecycleBin,
                        strStyle,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (strAction == "save")
                {
                    output_messages = new List<MessageData>();
                    foreach (MessageData data in messages)
                    {
                        byte[] baOutputTimestamp = null;
                        string strOutputID = "";
                        nRet = app.MessageCenter.SaveMessage(
                            sessioninfo.Channels,
                            data.strRecipient,
                            sessioninfo.UserID, // data.strSender,
                            data.strSubject,
                            data.strMime,
                            data.strBody,
                            data.strRecordID,   // 如果有，就覆盖，否则就新创建
                            data.TimeStamp,
                            out baOutputTimestamp,
                            out strOutputID,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        MessageData output_data = new MessageData(data);
                        output_data.TimeStamp = baOutputTimestamp;
                        output_data.strRecordID = strOutputID;
                        output_messages.Add(output_data);
                    }
                }
                else if (strAction == "send")
                {
                    foreach (MessageData data in messages)
                    {
                        // 统计信息写入操作日志
                        if (data.strRecipient == "!statis")
                        {
                            XmlDocument data_dom = new XmlDocument();
                            data_dom.LoadXml(data.strBody);

                            string uid = DomUtil.GetElementText(data_dom.DocumentElement,
                                "uid");
                            if (string.IsNullOrEmpty(uid))
                            {
                                strError = "data body 中必须具备 uid 元素";
                                goto ERROR1;
                            }

                            if (app.StatisLogUidTable.Contains(uid))
                            {
                                strError = $"UID 为 {uid} 的统计日志已经写入过了，无法重复写入";
                                result.Value = -1;
                                result.ErrorInfo = strError;
                                result.ErrorCode = ErrorCode.AlreadyExist;
                                return result;
                            }

                            XmlDocument domOperLog = new XmlDocument();
                            domOperLog.LoadXml("<root />");

                            DomUtil.SetElementText(domOperLog.DocumentElement,
                                "operation",
                                "statis");
                            // 第一级元素
                            if (data.strMime == "text/xml")
                            {
                                string[] reserve_list = new string[] {
                                    "operation","operator","operTime"
                                };
                                XmlNodeList nodes = data_dom.DocumentElement.SelectNodes("*");
                                foreach (XmlElement node in nodes)
                                {
                                    if (Array.IndexOf(reserve_list, node.Name) != -1)
                                        continue;
                                    // XmlElement new_node = domOperLog.CreateElement(node.Name);
                                    XmlNode importNode = domOperLog.ImportNode(node, true);
                                    domOperLog.DocumentElement.AppendChild(importNode);
                                }
                            }
                            else
                            {
                                DomUtil.SetElementText(domOperLog.DocumentElement,
    "subject",
    data.strSubject);
                                DomUtil.SetElementText(domOperLog.DocumentElement,
                                    "sender",
                                    data.strSender);
                                DomUtil.SetElementText(domOperLog.DocumentElement,
    "mime",
    data.strMime);
                                DomUtil.SetElementText(domOperLog.DocumentElement,
                                    "content",
                                    data.strBody);
                            }

                            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
    sessioninfo.UserID);
                            string strOperTime = app.Clock.GetClock();
                            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                                strOperTime);
                            nRet = app.OperLog.WriteOperLog(domOperLog,
                                sessioninfo.ClientAddress,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "统计信息写入操作日志时出错: " + strError;
                                goto ERROR1;
                            }

                            /*
                            // testing
                            {
                                strError = "统计信息写入操作日志时出错: 测试";
                                goto ERROR1;
                            }
                            */

                            app.StatisLogUidTable.Set(uid);

                            // 2021/7/13
                            continue;
                        }

                        // 异常报告还要写入操作日志
                        if (data.strRecipient == "crash")
                        {
                            XmlDocument domOperLog = new XmlDocument();
                            domOperLog.LoadXml("<root />");

                            DomUtil.SetElementText(domOperLog.DocumentElement,
                                "operation",
                                "crashReport");
                            DomUtil.SetElementText(domOperLog.DocumentElement, "subject",
                                data.strSubject);
                            DomUtil.SetElementText(domOperLog.DocumentElement, "sender",
                                data.strSender);
                            DomUtil.SetElementText(domOperLog.DocumentElement, "mime",
                                data.strMime);
                            DomUtil.SetElementText(domOperLog.DocumentElement, "content",
                                data.strBody);

                            string strOperTime = app.Clock.GetClock();
                            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                                strOperTime);
                            nRet = app.OperLog.WriteOperLog(domOperLog,
                                sessioninfo.ClientAddress,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "异常报告写入操作日志时出错: " + strError;
                                goto ERROR1;
                            }

                            // 发送到 dp2 内置信箱
                            nRet = app.MessageCenter.SendMessage(
                            sessioninfo.Channels,
                            data.strRecipient,
                            sessioninfo.UserID,
                            data.strSubject,
                            data.strMime,
                            data.strBody,
                            true,
                            out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            // 2021/7/13
                            continue;
                        }

                        // 2020/4/20
                        if (data.strRecipient.StartsWith("!mq:"))
                        {
                            if (StringUtil.IsInList("sendmq", sessioninfo.RightsOrigin) == false)
                            {
                                result.Value = -1;
                                result.ErrorInfo = "发送 MQ 消息被拒绝。不具备 sendmq 权限";
                                result.ErrorCode = ErrorCode.AccessDenied;
                                return result;
                            }

                            nRet = app.SendMessageQueue(
    data.strRecipient.Substring("!mq:".Length),
    data.strMime,
    data.strBody,
    out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            // 2021/7/13
                            continue;
                        }

                        // 2021/7/13
                        // 以上之外的 recipient，默认发送到 dp2 内置信箱
                        nRet = app.MessageCenter.SendMessage(
                        sessioninfo.Channels,
                        data.strRecipient,
                        sessioninfo.UserID,
                        data.strSubject,
                        data.strMime,
                        data.strBody,
                        true,
                        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetMessage() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 需要一个API，可以探测一个时间段范围内到底哪些日期存在统计文件
        public LibraryServerResult ExistStatisInfo(string strDateRangeString,
            out List<DateExist> dates)
        {
            string strError = "";
            dates = null;

            LibraryServerResult result = this.PrepareEnvironment("ExistStatisInfo", true);
            if (result.Value == -1)
                return result;

            try
            {
                int nRet = 0;
                this.BeginSearch();
                try
                {
                    nRet = app.Exists(
                        ref this._nStop,
        strDateRangeString,
        out dates,
        out strError);

                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    this.EndSearch();
                }

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library ExistStatisInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        public LibraryServerResult GetStatisInfo(string strDateRangeString,
            string strStyle,
            out RangeStatisInfo info,
            out string strXml)
        {
            string strError = "";
            strXml = "";
            info = null;

            LibraryServerResult result = this.PrepareEnvironment("GetStatisInfo", true,
                true);
            if (result.Value == -1)
                return result;

            try
            {
                int nRet = 0;
                string strOutputFilename = app.GetTempFileName("statis");   //  Path.GetTempFileName();
                this.BeginSearch();
                try
                {
                    // 合并时间范围内的多个XML文件
                    nRet = app.MergeXmlFiles(
                        "", // sessioninfo.LibraryCodeList,
                        ref this._nStop,
                        strDateRangeString,
                        strStyle,
                        strOutputFilename,
                        out info,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    try
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.Load(strOutputFilename);
                        strXml = dom.OuterXml;
                    }
                    catch (FileNotFoundException)   // 2011/6/6
                    {
                        strXml = "";
                    }
                    catch (Exception ex)
                    {
                        strError = "统计结果装载到XMLDOM时出错: " + ex.Message;
                        goto ERROR1;
                    }
                }
                finally
                {
                    File.Delete(strOutputFilename);
                    this.EndSearch();
                }

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetStatisInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // parameters:
        //      strName 名字。为 书目记录路径 + '|' + URL
        //      Value   [out]返回整数值。strAction 为 "inc" 或 "inc_and_log" 的时候不使用这个参数
        public LibraryServerResult HitCounter(string strAction,
            string strName,
            string strClientAddress,
            out long Value)
        {
            string strError = "";
            Value = 0;

            LibraryServerResult result = this.PrepareEnvironment("HitCounter", true,
                true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // TODO: 检查权限。至少 set 要求权限。读者身份不应该能进行此项操作。如果功能需要，可以用代理账户身份进行

                StringUtil.ParseTwoPart(strName, "|", out string strBiblioRecPath, out string strUrl);

                if (strAction == "get")
                    Value = app.HitCountDatabase.GetHitCount(strUrl);   // 如果 mongodb 没有打开，则这里会返回 Value = -1，但 ErrorCode 没有错误码
                else if (strAction == "inc")
                {
                    if (app.HitCountDatabase.IncHitCount(strUrl) == false)
                        result.Value = 0;   // mongodb 没有打开
                    else
                        result.Value = 1;

                    if (app.Statis != null)
                        app.Statis.IncreaseEntryValue(
                        sessioninfo.LibraryCodeList,
                        "增量外部对象计数器",
                        "次",
                        1);
                }
                else if (strAction == "inc_and_log")    // 增量计数器，并且同时记载到访问日志中
                {
                    if (app.HitCountDatabase.IncHitCount(strUrl) == false)
                        result.Value = 0;   // mongodb 没有打开
                    else
                        result.Value = 1;

                    if (app.Statis != null)
                        app.Statis.IncreaseEntryValue(
                        sessioninfo.LibraryCodeList,
                        "获取外部对象",
                        "次",
                        1);

                    // 写入日志
                    if (app.GetObjectWriteToOperLog == true)
                    {
                        string strResPath = strBiblioRecPath + "/url/" + strUrl;

                        // TODO: 在日志记录中如何明显辨别是 HitCounter() API 写入的，还是 GetRes() API 写入的?
                        if (string.IsNullOrEmpty(app.MongoDbConnStr) == false)
                        {
                            app.AccessLogDatabase.Add("getRes",
                                strResPath,
                                0,  // size,
                                "", // (string)table["mimetype"],
                                string.IsNullOrEmpty(strClientAddress) == true ? sessioninfo.ClientAddress : strClientAddress,
                                1,
                                sessioninfo.UserID,
                                DateTime.Now,
                                app.AccessLogMaxCountPerDay);
                        }
                        else
                        {
                            XmlDocument domOperLog = new XmlDocument();
                            domOperLog.LoadXml("<root />");

                            DomUtil.SetElementText(domOperLog.DocumentElement,
                                "operation",
                                "getRes");
                            DomUtil.SetElementText(domOperLog.DocumentElement, "path",
            strResPath);

                            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                                sessioninfo.UserID);
                            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                                app.Clock.GetClock());
#if NO
                            if (string.IsNullOrEmpty(strClientAddress) == false)
                                DomUtil.SetElementText(domOperLog.DocumentElement, "requestClientAddress",
                                    strClientAddress);
#endif

                            int nRet = app.OperLog.WriteOperLog(domOperLog,
                                string.IsNullOrEmpty(strClientAddress) == true ? sessioninfo.ClientAddress : strClientAddress,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "HitCounter() API 写入日志时发生错误: " + strError;
                                goto ERROR1;
                            }
                        }
                    }
                }
                else
                {
                    strError = "未知的 strAction '" + strAction + "'";
                    goto ERROR1;
                }
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.SystemError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library HitCounter() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // TODO: questions 要限制尺寸
        // parameters:
        //		strAuthor	著者字符串
        //		strNumber	返回号码
        //		strError	出错信息
        // return:
        // result.Value:
        //      -4  "著者 'xxx' 的整体或局部均未检索命中" 2017/3/1
        //		-3	需要回答问题
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public LibraryServerResult GetAuthorNumber(
            string strAuthor,
            bool bSelectPinyin,
            bool bSelectEntry,
            bool bOutputDebugInfo,
            ref List<Question> questions,
            out string strNumber,
            out string strDebugInfo)
        {
            string strError = "";
            strNumber = "";
            strDebugInfo = "";

            LibraryServerResult result = this.PrepareEnvironment("GetAuthorNumber", true,
    true);
            if (result.Value == -1)
                return result;

            try
            {

                int nStep = 0;

                if (questions == null)
                {
                    questions = new List<Question>();
                }

                // TODO: 验证身份
                // return:
                //      -4  "著者 'xxx' 的整体或局部均未检索命中" 2017/3/1
                //		-3	需要回答问题
                //      -1  出错
                //      0   成功
                int nRet = app.GetNumberInternal(
                    sessioninfo,
                    ref nStep,
                    strAuthor,
                    bSelectPinyin,
                    bSelectEntry,
                    bOutputDebugInfo,
                    ref questions,
                    out strNumber,
                    out StringBuilder debug_info,
                    out strError);
                if (debug_info != null)
                    strDebugInfo = debug_info.ToString();
                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetAuthorNumber() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // parameters:
        //      strType 类型。如果为空或者 pinyin，表示希望获得拼音。如果为 sjhm，表示希望获得四角号码
        // return:
        //      -2  strID 验证失败
        //      -1  出错
        //      0   成功
        public LibraryServerResult GetPinyin(
            string strType, // 2018/10/25 增加此参数
            string strText,
            out string strPinyinXml)
        {
            string strError = "";
            strPinyinXml = "";

            LibraryServerResult result = this.PrepareEnvironment("GetPinyin", true,
    true);
            if (result.Value == -1)
                return result;

            try
            {
                // return:
                //		-3	需要回答问题
                //      -1  出错
                //      0   成功
                int nRet = 0;

                if (string.IsNullOrEmpty(strType) || strType == "pinyin")
                    nRet = app.GetPinyinInternal(
                        sessioninfo,
                        strText,
                        out strPinyinXml,
                        out strError);
                else if (strType == "sjhm")
                    nRet = app.GetSjhmInternal(
    sessioninfo,
    strText,
    out strPinyinXml,
    out strError);
                else
                {
                    nRet = -1;
                    strError = "未知的 strType '" + strType + "'";
                }

                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library GetPinyin() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // return:
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public LibraryServerResult SetPinyin(
string strPinyinXml)
        {
            string strError = "";

            LibraryServerResult result = this.PrepareEnvironment("SetPinyin", true,
true, true);
            if (result.Value == -1)
                return result;

            try
            {
                // return:
                //		-3	需要回答问题
                //      -1  出错
                //      0   成功
                int nRet = app.SetPinyinInternal(
                    sessioninfo,
                    strPinyinXml,
                    out strError);
                result.Value = nRet;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SetPinyin() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 分词
        // return:
        //      -2  strID验证失败
        //      -1  出错
        //      0   成功
        public LibraryServerResult SplitHanzi(
            string strID,
            string strText,
            out List<string> tokens)
        {
            string strError = "";
            // int nRet = 0;

            tokens = null;

            LibraryServerResult result = this.PrepareEnvironment("SplitHanzi", true,
true);
            if (result.Value == -1)
                return result;

            try
            {
                tokens = LibraryApplication.SplitText(strText);

                result.Value = 0;
                result.ErrorInfo = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library SplitHanzi() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

        // 在线统计
        public LibraryServerResult OnlineStatis(
        string action,
        string category,
        string uid,
        string style,
        out List<string> results)
        {
            results = null;

            LibraryServerResult result = this.PrepareEnvironment("OnlineStatis", false,
false);
            if (result.Value == -1)
                return result;

            try
            {
                if (action == "activate")
                {
                    result.Value = DigitalPlatform.LibraryServer.OnlineStatis.Activate(uid, category);
                }
                else if (action == "getCount")
                {
                    result.Value = DigitalPlatform.LibraryServer.OnlineStatis.GetCount(category);
                }
                else
                {
                    result.Value = -1;
                    result.ErrorInfo = $"未知的 action '{action}'";
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "dp2Library OnlineStatis() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
                return result;
            }
        }

    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class ChargingItemWrapper
    {
        // 基本 Item
        [DataMember]
        public ChargingItem Item { get; set; }

        // 相关的 Item。比如一个 return 动作的 item 就可能具有一个 borrow 动作的 item
        [DataMember]
        public ChargingItem RelatedItem { get; set; }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class ChargingItem
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string LibraryCode { get; set; } // 访问者的图书馆代码
        [DataMember]
        public string Operation { get; set; } // 操作名
        [DataMember]
        public string Action { get; set; }  // 动作

        [DataMember]
        public string ItemBarcode { get; set; }
        [DataMember]
        public string PatronBarcode { get; set; }
        [DataMember]
        public string BiblioRecPath { get; set; }

        [DataMember]
        public string Period { get; set; }  // 期限
        [DataMember]
        public string No { get; set; }  // 续借次，序号

        // 2017/5/22
        [DataMember]
        public string Volume { get; set; }  // 卷册

        [DataMember]
        public string ClientAddress { get; set; }  // 访问者的IP地址

        [DataMember]
        public string Operator { get; set; }  // 操作者(访问者)
        [DataMember]
        public string OperTime { get; set; } // 操作时间。? 格式

        public ChargingItem(ChargingOperItem item)
        {
            this.Id = item.Id;
            this.LibraryCode = item.LibraryCode;
            this.Operation = item.Operation;
            this.Action = item.Action;
            this.ItemBarcode = item.ItemBarcode;
            this.PatronBarcode = item.PatronBarcode;
            this.BiblioRecPath = item.BiblioRecPath;
            this.Period = item.Period;
            this.Volume = item.Volume;
            this.No = item.No;
            this.ClientAddress = item.ClientAddress;
            this.Operator = item.Operator;
            this.OperTime = item.OperTime.ToString("G");
        }
    }

    public class HostInfo : IExtension<ServiceHostBase>, IDisposable
    {
        public object LockObject = new object();
        ServiceHostBase owner = null;
        public LibraryApplication App = null;
        public string DataDir = ""; // 数据目录

        public bool TestMode = false;   // 是否需要启动为评估模式
        public int MaxClients = 5;      // 最多允许的前端机器台数 -1表示不限定
        public string LicenseType = ""; // 许可类型 server 表示服务器授权模式
        public string Function = "";    // 许可的功能列表
        public string Protocol = "";    // 所绑定的协议。例如 http net.tcp 等

        // 2017/8/30
        // 实例名
        public string InstanceName { get; set; }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            this.owner = owner;
        }
        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            lock (this.LockObject)
            {
                if (this.App != null)
                {
                    this.App.Close();
                    this.App = null;
                }
            }
        }
    }

}
