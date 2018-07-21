using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.ServiceModel.Channels;

using DigitalPlatform;
using DigitalPlatform.rms;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.ResultSet;
using System.IO.Compression;

namespace dp2Kernel
{
    /*
    public static class GlobalVars
    {
        public static KernelApplication KernelApplication = null;
        public static object LockObject = new object();
    }
     * */

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple,
        Namespace = "http://dp2003.com/dp2kernel/")]
    public class KernelService : IKernelService, IDisposable
    {
        KernelApplication app = null;
        SessionInfo sessioninfo = null;
        User user = null;

        public void Dispose()
        {
            if (this.sessioninfo != null)
            {
                this.sessioninfo.Close();
            }
        }

        #region 基础函数

        int InitialApplication(out string strError)
        {
            strError = "";

            if (this.app != null)
                return 0;   // 已经初始化

            HostInfo info = OperationContext.Current.Host.Extensions.Find<HostInfo>();
            if (info == null)
            {
                strError = "没有找到 HostInfo";
                return -1;
            }

            info.LockForRead();
            try
            {
                if (info.App != null)
                {
                    if (info.App.Dbs == null || info.App.Users == null)
                    {
                        // 以前残余的对象没有释放
                        try
                        {
                            info.App.Close();
                        }
                        catch
                        {
                        }

                        info.App = null;
                    }
                    else
                    {
                        this.app = info.App;

                        Debug.Assert(info.App.Dbs != null, "");
                        Debug.Assert(info.App.Users != null, "");

                        return 0;
                    }
                }
            }
            finally
            {
                info.UnlockForRead();
            }

            // lock (info.LockObject)

            info.LockForWrite();
            try
            {
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
                info.App = new KernelApplication();
                // parameter:
                //		strDataDir	data目录
                //		strError	out参数，返回出错信息
                // return:
                //		-1	出错
                //		0	成功
                // 线: 安全的
                int nRet = info.App.Initial(strDataDir,
                            strBinDir, //  + "\\bin",
                            out strError);
                if (nRet == -1)
                    return -1;

                Debug.Assert(info.App.Dbs != null, "");
                Debug.Assert(info.App.Users != null, "");

                this.app = info.App;
            }
            finally
            {
                info.UnlockForWrite();
            }

            return 0;
        }

        public static void GetClientAddress(out string strIP, out string strVia)
        {
            strIP = "";
            MessageProperties prop = OperationContext.Current.IncomingMessageProperties;

            strVia = prop.Via.ToString();

            if (prop.Via.Scheme == "net.pipe")
            {
                // 没有 IP
                strIP = "localhost";
                return;
            }
            try
            {
                RemoteEndpointMessageProperty endpoint = prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                strIP = endpoint.Address;
            }
            catch
            {
            }
            strVia = prop.Via.ToString();
        }

        int InitialSession(out string strError)
        {
            strError = "";

            string strIP = "";
            string strVia = "";
            GetClientAddress(out strIP, out strVia);

            sessioninfo = new SessionInfo();
            // return:
            //      -1  出错
            //      0   成功
            int nRet = sessioninfo.Initial(app,
                OperationContext.Current.SessionId,
                strIP,
                strVia,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 准备Application和SessionInfo环境
        // return:
        //      Value == 0  正常
        //      Value == -1 不正常
        Result PrepareEnvironment(bool bPrepareSessionInfo)
        {
            Result result = new Result();

            string strError = "";
            int nRet = 0;

            if (this.app == null)
            {
                nRet = InitialApplication(out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorString = "InitialApplication fail: " + strError;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    return result;
                }
                Debug.Assert(this.app != null, "");
            }

            if (bPrepareSessionInfo == true
                && this.sessioninfo == null)
            {
                nRet = InitialSession(out strError);
                if (nRet == -1)
                {
                    result.Value = -1;
                    result.ErrorString = "InitialSession fail: " + strError;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    return result;
                }
                Debug.Assert(this.sessioninfo != null, "");
            }

            return result;
        }

        // 准备this.user对象
        int PrepareUser(ref Result result)
        {

            this.user = this.GetUser(out string strError);
            if (user == null)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strError;  // "名为'" + this.sessioninfo.UserName + "'帐户对象不存在。";
                return -1;
            }

            return 0;
        }

        // 得到用户对象
        public User GetUser(out string strError)
        {
            // string strError = "";
            if (app == null)
            {
                strError = "忘记调用PrepareEnvironment()了 1...";
                return null;
            }
            if (this.sessioninfo == null)
            {
                strError = "忘记调用PrepareEnvironment()了 2...";
                return null;
            }
            // 2015/6/11
            if (app.Users == null)
            {
                strError = "app.Users == null";
                return null;
            }
            // 不管UserName对应的用户对象是否在内存, 都可以找到或创建
            // return:
            //      -1  出错
            //      0   未找到
            //      1   找到
            int nRet = app.Users.GetUserSafety(
                false,
                this.sessioninfo.UserName,
                out User user,
                out strError);
            if (nRet != 1)
            {
                return null;
            }
            return user;
        }

        #endregion

        // 2012/1/5
        // 获得版本号
        //      2.2 第一个具有版本号的版本。特点是增加了SearchEx() API，另外Record结构也有改动，增加了RecordBody成员
        //      2.3 records 表中会自动增加一个列 newdptimestamp
        //      2.4 records 表中会自动增加两个列 filename newfilename -- 2012/2/8
        //      2.5 支持4种数据库引擎
        //      2.51 新增freetime时间类型 2012/5/15
        //      2.52 Dir() API 中检索途径节点的 TypeString 对时间检索途径包含 _time _freetime _rfc1123time _utime 子串
        //      2.53 将GetRes() API的nStart参数从int修改为long类型 2012/8/26
        //      2.54 实现全局结果集 2013/1/4
        //      2.55 GetRecords()/GetBrowse()等的 strStyle 新增 format:@coldef:xxx|xxx 功能
        //      2.56 大幅度改进 WriteRecords() 等 API，提高了批处理 I/O 的速度 2013/2/21
        //      2.57 2015/1/21 改进 CopyRecord() API 增加了 strMergeStyle 和 strIdChangeList 参数。允许源和目标的对象都保留在目标记录中
        //      2.58 2015/8/25 修改空值检索的错误。( keycount 和 keyid 风格之下 不正确的 not in ... union 语句)
        //      2.59 2015/8/27 GetRecords()/GetBrowse()等 API 中 strStyle 的 format:@coldef:xxx|xxx 格式，其中 xxx 除了原先 xpath 用法外，还可以使用 xpath->convert 格式。
        //      2.60 2015/9/26 WriteXml() 对整个操作超过一秒的情况，会将时间构成详情写入错误日志
        //      2.61 2015/11/8 Search() 和 SearchEx() 中，XML 检索式的 target 元素增加了 hint 属性。如果 hint 属性包含 first 子串，则当 target 元素的 list 属性包含多个数据库时，顺次检索的过程中只要有一次命中，就立即停止检索返回。此方式能提高检索速度，但不保证能检索全命中结果。比较适合用于册条码号等特定的检索途径进行借书还书操作
        //      2.62 2015/11/14 GetBrowse() API 允许获得对象记录的 metadata 和 timestamp
        //      2.63 2015/11/16 WriteRes() API WriteRes() API 允许通过 lTotalLength 为 -1 调用，作用是仅修改 metadata
        //      2.64 2016/1/6 MySQL 版本在删除和创建检索点的时候所使用的 SQL 语句多了一个分号。此 Bug 已经排除
        //      2.65 2016/5/14 WriteRecords() API 支持上载结果集。XML 检索式为 item 元素增加 resultset 属性，允许已有结果集参与逻辑运算。优化 resultset[] 操作符速度。
        //      2.66 2016/12/13 若干 API 支持 simulate style
        //      2.67 2017/5/11 GetBrowse() API 支持 @coldef: 中使用名字空间和(匹配命中多个XmlNode时串接用的)分隔符号
        //                      例如: "id,cols,format:@coldef://marc:record/marc:datafield[@tag='690']/marc:subfield[@code='a']->nl:marc=http://dp2003.com/UNIMARC->dm:\t|//marc:record/marc:datafield[@tag='093']/marc:subfield[@code='a']->nl:marc=http://www.loc.gov/MARC21/slim->dm:\t";
        //                      | 分隔多个栏目的定义段落。每个栏目的定义中：
        //                      ->nl:表示名字空间列表。多个名字空间之间用分号间隔
        //                      ->dm:表示串接用的符号，当 XPath 匹配上多个 XmlNode 时用这种符号拼接结果字符串
        //                      ->cv:表示转换方法。以前的方法，这样定义也是可以的 xxxx->cccc 其中 xxxx 是 XPath 部分，cccc 是 convert method 部分。新用法老用法都兼容
        //                      '->' 分隔的第一个部分默认就是 XPath。
        //      2.68 2017/6/7 为 WriteRes() API 的 strStyle 参数增加 simulate 用法 (当写入对象资源时)
        //      2.69 2017/10/7 为 GetRes() 和 WriteRes() API 的 strStyle 参数增加 gzip 用法
        //      3.0 2018/6/23 改为用 .NET Framework 4.6.1 编译
        public Result GetVersion()
        {
            Result result = new Result
            {
                Value = 0,
                ErrorString = "3.0"
            };
            return result;
        }

        // 登录
        // parameter:
        //		strUserName 用户名
        //		strPassword 密码
        // return:
        //		Result对象
        //		value == -1 出错
        //				 0  用户名或密码不正确
        //				 1  成功
        public Result Login(string strUserName,
            string strPassword)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                if (String.IsNullOrEmpty(strUserName) == true)
                {
                    result.Value = 0;
                    result.ErrorCode = ErrorCodeValue.UserNameEmpty;
                    result.ErrorString = "用户名不能为空字符串...";
                    return result;
                }

                string strError = "";
                int nRet = 0;
                string strError0 = "";

                nRet = app.UserNameTable.BeforeLogin(strUserName,
    sessioninfo.ClientIP,
    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                // 如果先前已经登录过，这里先Logout
                if (String.IsNullOrEmpty(this.sessioninfo.UserName) == false)
                {
                    // return:
                    //      -1  出错
                    //      0   未找到
                    //      1   找到，并从集合中清除
                    nRet = app.Users.Logout(this.sessioninfo.UserName,
                        out strError);
                    if (nRet == -1)
                        strError0 = "登录前的登出操作错误 : " + strError;

                    this.sessioninfo.UserName = "";
                }

                Debug.Assert(app.Users != null, "");

                User user = null;
                // return:
                //		-1	出错
                //		0	用户名不存在，或密码不正确
                //      1   成功
                nRet = app.Users.Login(
                    strUserName,
                    strPassword,
                    out user,
                    out strError);

                if (nRet == 0 || nRet == 1)
                {
                    string strLogText = app.UserNameTable.AfterLogin(strUserName,
                        sessioninfo.ClientIP,
                        nRet);
                    if (string.IsNullOrEmpty(strLogText) == false)
                        app.WriteErrorLog("!!! " + strLogText);
                }

                if (nRet == -1)
                {
                    result.Value = -1;
                    // 出错信息处理
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    result.ErrorString = strError0 + strError;
                    return result;
                }

                if (nRet == 0)
                {
                    this.sessioninfo.UserName = "";
                    result.Value = 0;
                    result.ErrorCode = ErrorCodeValue.UserNameOrPasswordMismatch;
                    result.ErrorString = strError0 + "用户名或者密码不正确..."; // strErrorInfo不暴露给一般用户?
                    return result;
                }

                Debug.Assert(nRet == 1, "");
                Debug.Assert(user != null, "此时user对象不可能为null.");

                this.sessioninfo.UserName = user.Name;
                result.Value = 1;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorString = strError;
                result.ErrorCode = ErrorCodeValue.CommonError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "Login() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 登出
        public Result Logout()
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                if (this.sessioninfo.UserName != "")
                {
                    string strError = "";
                    // return:
                    //      -1  出错
                    //      0   未找到
                    //      1   找到，并从集合中清除
                    int nRet = app.Users.Logout(this.sessioninfo.UserName,
                        out strError);
                    this.sessioninfo.UserName = "";
                    if (nRet == -1)
                    {
                        app.WriteErrorLog("logout error : " + strError);
                        result.Value = -1;
                        result.ErrorCode = ErrorCodeValue.CommonError;
                        result.ErrorString = strError;
                        return result;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "Logout() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        public int DoTest(string strText)
        {
            int v = 0;
            v++;
            Thread.Sleep(new TimeSpan(0, 2, 0));
            return v;
        }

        /*
        public Result Stop()
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            if (sessioninfo.InSearching > 0)
            {
                if (sessioninfo.ChannelHandle != null)
                    sessioninfo.ChannelHandle.DoStop();

                app.Dbs.MyWriteDebugInfo("因后一个stop的到来，前一个search不得不中断 ");

                result.Value = 1;
                return result;
            }

            result.Value = 0;
            return result;
        }
        */

        public void Stop()
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return;

            if (sessioninfo.InSearching > 0)
            {
                if (sessioninfo.ChannelHandle != null)
                    sessioninfo.ChannelHandle.DoStop();

                app.MyWriteDebugInfo("因后一个stop的到来，前一个search不得不中断 ");
            }
        }

        // 2012/1/5
        // 带有PiggyBack功能的检索
        // parameters:
        //		strQuery	XML检索式
        //      strResultSetName    结果集名
        //      strSearchStyle  检索风格
        //      lRecordCount    希望获得的记录数量。-1表示尽可能多。如果为0，表示不想获得任何记录
        //                      总是从偏移量0开始获得记录
        //      strRecordStyle  获得记录的风格。以逗号分隔，id表示取id,cols表示取浏览格式
        //                      xml timestamp metadata 分别表示要获取的记录体的XML字符串、时间戳、元数据
        // return:
        //		Result对象
        //		Value -1	出错
        //			   0	没命中
        //			   >=1	命中的记录数
        public Result SearchEx(string strQuery,
            string strResultSetName,
            string strSearchStyle,
            long lRecordCount,
            string strLang,
            string strRecordStyle,
            out Record[] records)
        {
            records = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            if (sessioninfo.InSearching > 0)
            {
                if (sessioninfo.ChannelHandle != null)
                    sessioninfo.ChannelHandle.DoStop();

                app.MyWriteDebugInfo("因后一个search(ex)的到来，前一个search(ex)不得不中断 ");
            }

            sessioninfo.BeginSearch();
            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                ChannelHandle handle = new ChannelHandle();
                handle.App = app;
                handle.Idle += new ChannelIdleEventHandler(handle_Idle);
                handle.Stop += new EventHandler(handle_Stop);

                sessioninfo.ChannelHandle = handle;

                DpResultSet resultSet = null;

                // Debug.WriteLine(strQuery);

                // resultSet = this.sessioninfo.GetResultSet(strResultSetName, false);
                if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                    resultSet = app.ResultSets.GetResultSet(strResultSetName.Substring(1), false);
                else
                    resultSet = this.sessioninfo.GetResultSet(strResultSetName, false);

                lock (result)
                {
                    if (resultSet != null)
                        resultSet.Clear();

                    int nRet = 0;
                    string strError = "";

                    app.MyWriteDebugInfo("begin searchex1 " + strQuery);

                    DpResultSet old_resultset = resultSet;
                    // return:
                    //		-1	出错
                    //      -6  权限不够
                    //		0	成功
                    nRet = app.Dbs.API_Search(
                        sessioninfo,
                        strQuery,
                        ref resultSet,
                        user,           //注意测一下没有权限的帐户
                        handle,
                        strSearchStyle,
                        out strError);

                    app.MyWriteDebugInfo("end searchex1 lRet=" + nRet.ToString() + " " + strQuery);

                    if (nRet <= -1)
                    {
                        result.Value = -1;
                        if (handle.Stopped == true)
                            result.ErrorCode = ErrorCodeValue.Canceled;
                        else
                            result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                        result.ErrorString = strError;
                        return result;
                    }

                    // throw new Exception("test exception"); 测试用

                    result.Value = resultSet.Count;  //执行成功时，result.Value等于提取记录的数量

#if NO
                    if (old_resultset != resultSet)
                        sessioninfo.SetResultSet(strResultSetName, resultSet);
#endif
                    if (old_resultset != resultSet)
                    {
                        if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                            app.ResultSets.SetResultset(strResultSetName.Substring(1), resultSet);
                        else
                            sessioninfo.SetResultSet1(strResultSetName, resultSet);
                    }

                    // GC.Collect();    // 可以确认结果集临时文件立即删除了

                    if (lRecordCount != 0 && resultSet.Count > 0)
                    {
                        // 获得若干记录
                        // result:
                        //		-1	出错
                        //		>=0	结果集的总数
                        long lRet = this.sessioninfo.API_GetRecords(
                            resultSet,
                            0,
                            lRecordCount,
                            strLang,
                            strRecordStyle,
                            out records,
                            out strError);
                        if (lRet <= -1)
                        {
                            result.Value = -1;
                            result.ErrorCode = KernelApplication.Ret2ErrorCode((int)lRet);
                            result.ErrorString = "虽然检索已经成功，但是获取记录时发生错误: " + strError;
                            return result;
                        }
                    }
                    else
                    {
                        records = new Record[0];    // 2017/8/23
                    }

                } // end of lock
            }
            catch (Exception ex)    // TODO: 将来把异常处理在中层函数内
            {
                string strErrorText = "SearchEx() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                app.MyWriteDebugInfo("searchex throw exception " + strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = ex.Message
                    + ex.Source.ToString()
                    + ex.StackTrace.ToString();
                return result;
            }
            finally
            {
                sessioninfo.EndSearch();
            }

            return result;
        }

        // 检索
        // parameter:
        //		strQuery	XML检索式
        //      strResultSetName    结果集名
        // return:
        //		Result对象
        //		Value -1	出错
        //			   0	没命中
        //			   >=1	命中的记录数
        public Result Search(string strQuery,
            string strResultSetName,
            string strOutputStyle)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            if (sessioninfo.InSearching > 0)
            {
                if (sessioninfo.ChannelHandle != null)
                    sessioninfo.ChannelHandle.DoStop();

                app.MyWriteDebugInfo("因后一个search的到来，前一个search不得不中断 ");
            }


            sessioninfo.BeginSearch();
            try
            {

                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

#if NO
                //定义一个Delegate对象
                Delegate_isConnected procIsConnected =
                    new Delegate_isConnected(this.myIsConnected);
#endif

                ChannelHandle handle = new ChannelHandle();
                handle.App = app;
                handle.Idle += new ChannelIdleEventHandler(handle_Idle);
                handle.Stop += new EventHandler(handle_Stop);

                sessioninfo.ChannelHandle = handle;

                DpResultSet resultSet = null;

                if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                    resultSet = app.ResultSets.GetResultSet(strResultSetName.Substring(1), false);
                else
                    resultSet = this.sessioninfo.GetResultSet(strResultSetName, false);

                lock (result)
                {
                    if (resultSet != null)
                        resultSet.Clear();

                    int nRet = 0;
                    string strError = "";

                    app.MyWriteDebugInfo("begin search1 " + strQuery);

                    DpResultSet old_resultset = resultSet;
                    // return:
                    //		-1	出错
                    //      -6  权限不够
                    //		0	成功
                    nRet = app.Dbs.API_Search(
                        sessioninfo,
                        strQuery,
                        ref resultSet,
                        user,           //注意测一下没有权限的帐户
                        handle,
                        // procIsConnected,
                        strOutputStyle,
                        out strError);

                    /*
                    int v = 0;
                    for (int i = 0; i < 10000; i++)
                    {
                        Thread.Sleep(1);
                        if (OperationContext.Current.Channel.State == CommunicationState.Closed
                            || OperationContext.Current.Channel.State == CommunicationState.Closing)
                        {
                            app.Dbs.MyWriteDebugInfo("abort Test ");
                            break;   //中断
                        }
                        v++;
                    }
                    result.Value = v;
                    return result;
                     * */


                    app.MyWriteDebugInfo("end search1 lRet=" + nRet.ToString() + " " + strQuery);


                    if (nRet <= -1)
                    {
                        result.Value = -1;
                        if (handle.Stopped == true)
                            result.ErrorCode = ErrorCodeValue.Canceled;
                        else
                            result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                        result.ErrorString = strError;
                        return result;
                    }

                    // throw new Exception("test exception"); 测试用

                    result.Value = resultSet.Count;  //执行成功时，result.Value等于提取记录的数量

                    if (old_resultset != resultSet)
                    {
                        if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                            app.ResultSets.SetResultset(strResultSetName.Substring(1), resultSet);
                        else
                            sessioninfo.SetResultSet1(strResultSetName, resultSet);

                    }

                } // end of lock
            }
            catch (Exception ex)    // TODO: 将来把异常处理在中层函数内
            {
                string strErrorText = "Search() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                app.MyWriteDebugInfo("search throw exception " + strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = ex.Message
                    + ex.Source.ToString()
                    + ex.StackTrace.ToString();
                return result;
            }
            finally
            {
                sessioninfo.EndSearch();
            }

            return result;
        }

        void handle_Stop(object sender, EventArgs e)
        {
            // 要保持通道继续可用,就不要做任何动作了
            // OperationContext.Current.Channel.Abort();
        }

        void handle_Idle(object sender, ChannelIdleEventArgs e)
        {
            CommunicationState state = OperationContext.Current.Channel.State;
            if (state == CommunicationState.Closed
                || state == CommunicationState.Closing)
                e.Continue = false;
            else
                e.Continue = true;
        }

        // 当前用户修改自己的密码
        // parameter:
        //		strNewPassword	新密码
        public Result ChangePassword(string strNewPassword)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (string.IsNullOrEmpty(this.sessioninfo.UserName) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                string strError = "";

                // return:
                //      -1  出错
                //      -4  记录不存在
                //		0   成功
                int nRet = user.ChangePassword(
                    strNewPassword,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "ChangePassword() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 修改自己或者其他人的密码
        // parameter:
        //		strUserName	用户名
        //		strNewPassword	新密码
        // 说明: 普通用户只能修改自己的密码，管理员可以修改其它帐户的密码
        public Result ChangeOtherPassword(
            string strUserName,
            string strNewPassword)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (string.IsNullOrEmpty(this.sessioninfo.UserName) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                string strError = "";
                int nRet = 0;

                if (PrepareUser(ref result) == -1)
                    return result;

                if (user.Name == strUserName)  // 自己修改自己的情况
                {
                    // return:
                    //      -1  出错
                    //      -4  记录不存在
                    //		0   成功
                    nRet = user.ChangePassword(strNewPassword,
                        out strError);
                    if (nRet <= -1)
                    {
                        result.Value = -1;
                        result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                        result.ErrorString = strError;
                        return result;
                    }
                }
                else
                {   // 修改其他人的密码
                    // return:
                    //      -1  出错
                    //      -4  记录不存在
                    //      -6  权限不够
                    //		0   成功
                    nRet = app.Users.ChangePassword(
                        user,
                        strUserName,
                        strNewPassword,
                        out strError);
                    if (nRet <= -1)
                    {
                        result.Value = -1;
                        result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                        result.ErrorString = strError;
                        return result;
                    }
                }

                // result.Value = 0
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "ChangeOtherPassword() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 根据服务器上的指定路径列出其下级的事项
        // parameters:
        //		strResPath	路径,不带服务器部分，
        //				格式为: "数据库名/下级名/下级名",
        //				当为null或者为""时，表示列出该服务器下第一级对象，多为数据库
        //		lStart	起始位置,从0开始 ,不能小于0
        //		lLength	长度 -1表示从lStart到最后
        //		strLang	语言版本 用标准语言代码表示法，如zh-CN
        //      strStyle    列出具体哪些事项 "alllang"表示也要列出所有语言下的名字
        //		items	 out参数，返回下级事项数组
        // return:
        //		Result对象
        //		    -1  出错
        //		    >=0 总共的下级事项数量
        // 说明	只有当前帐户对事项有"list"权限时，才能列出来。
        //		如果列本服务器的数据库时，当前用户对所有的数据库都没有dir权限，
        //      则按错误处理，以便与没有任何数据库事项的情形区分开。
        public Result Dir(string strResPath,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out ResInfoItem[] items)
        {
            items = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                string strError = "";

                // DateTime time = DateTime.Now;

                long lMaxLength = 500;   // 最大500项

                int nTotalLength = 0;
                int nRet = 0;
                // return:
                //		-1  出错
                //      -4  strResPath 对应的对象没有找到
                //      -6  权限不够
                //		0   正常
                nRet = app.Dbs.API_Dir(strResPath,
                    lStart,
                    lLength,
                    lMaxLength,
                    strLang,
                    strStyle,
                    user,
                    out items,
                    out nTotalLength,
                    out strError);

                // TimeSpan useTime = DateTime.Now - time;
                // this.app.Dbs.WriteDebugInfo("Dir()用的时间：" + useTime.ToString());


                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                result.Value = nTotalLength;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "Dir() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 初始化数据库
        // parameter:
        //		strDbName   数据库名称
        // return:
        //		value == -1 出错
        //				 0  成功
        // 说明: 只有当前对指定的数据库有"management"权限时，才能初始化数据库
        public Result InitializeDb(string strDbName)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;


                string strError = "";
                // 检查权限(在函数里实现)
                // return:
                //      -1  出错
                //      -5  数据库不存在
                //      -6  权限不够
                //      0   成功
                int nRet = app.Dbs.API_InitializePhysicalDatabase(
                    user,
                    strDbName,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "InitializeDb() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 2008/11/14
        // 刷新数据库定义
        // parameter:
        //      strAction   动作。begin为开始刷新。end为结束刷新。
        //                  beginfastappend/endfastappend 开始和结束快速写入模式。在开始的时候，系统drop所有keys表的索引，以便提高数据装入速度；在结束的时候，重新建立所有keys表的索引。这样导致的后果是，在装入中途，检索keys表只能是全表扫描，速度很慢
        //		strDbName   数据库名称
        // return:
        //		value == -1 出错
        //				 0  成功
        // 说明: 只有当前对指定的数据库有"management"权限时，才能刷新数据库定义
        public Result RefreshDb(
            string strAction,
            string strDbName,
            bool bClearAllKeyTables)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            // 兼容旧的API
            if (String.IsNullOrEmpty(strAction) == true)
            {
                strAction = "begin";
            }

            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;


                string strError = "";
                // 检查权限(在函数里实现)
                // return:
                //      -1  出错
                //      -5  数据库不存在
                //      -6  权限不够
                //      0   成功
                int nRet = app.Dbs.API_RefreshPhysicalDatabase(
                    // sessioninfo,
                    user,
                    strAction,
                    strDbName,
                    bClearAllKeyTables,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "RefreshDb() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 从结果集中提取指定范围的浏览格式记录
        // 建议函数名修改为GetSearchResult
        // parameter:
        //		lStart	开始序号
        //		lLength	长度. -1表示从lStart到末尾
        //		strLang	语言版本，用来获得记录路径
        //		strStyle	样式,以逗号分隔，id:表示取id,cols表示取浏览格式
        //		records	得到的记录数组，成员为类型为Record
        // Result.Value
        //		value == -1	出错。如果错误码为 ErrorCodeValue.NotFound，表示结果集不存在
        //			  >= 0	结果集内的记录总数
        public Result GetRecords(
            string strResultSetName,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out Record[] records)
        {
            records = null;
            string strError = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                DpResultSet resultset = null;

                bool bCommand = false;
                if (string.IsNullOrEmpty(strStyle) == false && strStyle[0] == '@')
                    bCommand = true;

                if (bCommand == true && StringUtil.HasHead(strStyle, "@remove:") == true)
                {
                    // 不必获得当前Session内结果集对象指针了。strResultSetName参数也无用
                }
                else
                {
                    // if (string.IsNullOrEmpty(strResultSetName) == false && strResultSetName[0] == '#')
                    if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                    {
                        resultset = app.ResultSets.GetResultSet(strResultSetName.Substring(1), false);
                        // TODO: 结果集没有找到，如何报错
                    }
                    else
                        resultset = this.sessioninfo.GetResultSet(strResultSetName);

                    // 2016/5/15
                    if (resultset == null)
                    {
                        result.Value = -1;
                        result.ErrorCode = ErrorCodeValue.NotFound;
                        result.ErrorString = "结果集 '" + strResultSetName + "' 不存在";
                        return result;
                    }
                }

                if (bCommand == true)
                {
                    if (StringUtil.HasHead(strStyle, "@share:") == true)
                    {
                        if (resultset == null)
                        {
                            strError = "打算共享的结果集 '" + strResultSetName + "' 不存在...";
                            goto ERROR1;
                        }
                        string strGlobalResultsetName = strStyle.Substring("@share:".Length);
                        app.ResultSets.SetResultset(strGlobalResultsetName, resultset);
                        result.Value = 0;
                        return result;
                    }
                    else if (StringUtil.HasHead(strStyle, "@remove:") == true)
                    {
                        string strGlobalResultsetName = strStyle.Substring("@remove:".Length);
                        app.ResultSets.SetResultset(strGlobalResultsetName, null);
                        result.Value = 0;
                        return result;
                    }

                    // TODO: 如何防止全局结果集的数目非常大？是否当原来的Session摧毁的时候，也自动摧毁全局结果集对象指针?
                    strError = "不能识别的指令 '" + strStyle + "'";
                    goto ERROR1;
                }

                // result:
                //		-1	出错
                //		>=0	结果集的总数
                long lRet = this.sessioninfo.API_GetRecords(
                    resultset,
                    lStart,
                    lLength,
                    strLang,
                    strStyle,
                    out records,
                    out strError);
                if (lRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode((int)lRet);
                    result.ErrorString = strError;
                    return result;
                }

                // 最大值
                result.Value = lRet;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "GetRecords() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }

        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCodeValue.CommonError;
            result.ErrorString = strError;
            return result;
        }

        // 根据记录路径(数组)获得一批记录的浏览格式
        // parameter:
        //		paths	记录路径数组
        //		strStyle	风格
        //		aRecord	得到的记录数组，成员为类型为Record
        // result:
        //		Result对象,
        //		value == -1	出错
        //			  == 0	成功
        public Result GetBrowse(
            string[] paths,
            string strStyle,
            out Record[] records)
        {
            records = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                string strError = "";
                // result:
                //      -1  出错
                //      0   成功
                int nRet = this.sessioninfo.API_GetBrowse(paths,
                    strStyle,
                    out records,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "GetBrowse() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // TODO: 尚未测试
        // 从结果集中提取指定范围的记录，特别是XML记录体
        // 如果strStyle中有“xml”，则表示获得XML记录体。
        // 如果中间某条记录出错，会将出错信息放在RichRecord的Result成员中。
        // parameter:
        //		strRanges	范围
        //		strStyle	样式,以逗号分隔，id:表示取id,cols表示取浏览格式  xml timestamp
        //		strLang     语言版本，用来获得记录路径
        //		richRecords	得到的记录数组，成员为类型为Record
        // result:
        //		Result对象,
        //		value == -1	出错
        //			  >= 1	结果集的总数
        //			  == 0	0条
        public Result GetRichRecords(
            string strResultSetName,
            string strRanges,
            string strLang,
            string strStyle,
            out RichRecord[] richRecords)
        {
            richRecords = null;
            string strError = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                // DpResultSet resultset = this.sessioninfo.GetResultSet(strResultSetName);

                DpResultSet resultset = null;

                bool bCommand = false;
                if (string.IsNullOrEmpty(strStyle) == false && strStyle[0] == '@')
                    bCommand = true;

                if (bCommand == true && StringUtil.HasHead(strStyle, "@remove:") == true)
                {
                    // 不必获得当前Session内结果集对象指针了。strResultSetName参数也无用
                }
                else
                {
                    // if (string.IsNullOrEmpty(strResultSetName) == false && strResultSetName[0] == '#')
                    if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                    {
                        resultset = app.ResultSets.GetResultSet(strResultSetName.Substring(1), false);
                        // TODO: 结果集没有找到，如何报错
                    }
                    else
                        resultset = this.sessioninfo.GetResultSet(strResultSetName);
                }

                if (bCommand == true)
                {
                    if (StringUtil.HasHead(strStyle, "@share:") == true)
                    {
                        if (resultset == null)
                        {
                            strError = "打算共享的结果集 '" + strResultSetName + "' 不存在...";
                            goto ERROR1;
                        }
                        string strGlobalResultsetName = strStyle.Substring("@share:".Length);
                        app.ResultSets.SetResultset(strGlobalResultsetName, resultset);
                        result.Value = 0;
                        return result;
                    }
                    else if (StringUtil.HasHead(strStyle, "@remove:") == true)
                    {
                        string strGlobalResultsetName = strStyle.Substring("@remove:".Length);
                        app.ResultSets.SetResultset(strGlobalResultsetName, null);
                        result.Value = 0;
                        return result;
                    }

                    // TODO: 如何防止全局结果集的数目非常大？是否当原来的Session摧毁的时候，也自动摧毁全局结果集对象指针?
                    strError = "不能识别的指令 '" + strStyle + "'";
                    goto ERROR1;
                }


                // result:
                //		-1  出错
                //		>=0	结果集的总数
                long lRet = this.sessioninfo.API_GetRichRecords(
                    resultset,
                    strRanges,
                    strLang,
                    strStyle,
                    out richRecords,
                    out strError);
                if (lRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode((int)lRet);
                    result.ErrorString = strError;
                    return result;
                }

                result.Value = lRet;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "GetRichRecords() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        ERROR1:
            result.Value = -1;
            result.ErrorCode = ErrorCodeValue.CommonError;
            result.ErrorString = strError;
            return result;
        }

        // 获得资源
        // 任延华注：GetRes()用range不太好实现,因为原来当请求的长度超过允许的长度时,长度会自动为截取
        // 而如果用range来表示,则不知该截短哪部分好。
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录体，对象资源，部分记录体
        //						配置事项: 库名/配置事项路径
        //						记录体: 库名/记录号
        //						对象资源: 库名/记录号/object/资源ID
        //						部分记录体: 库名/记录/xpath/<locate>hitcount</locate><action>AddInteger</action> 或者 库名/记录/xpath/@hitcount
        //		lStart	起始长度
        //		lLength	总长度,-1:从start到最后
        //		strStyle	取资源的风格，以逗号间隔的字符串
        /*
        strStyle用法

        1.控制数据存放的位置
        content		把返回的数据放到字节数组参数里。这个用法目前已经废除，因为现在一直用baContent参数来返回数据，不再用附件机制。不过值得注意的是，strStyle中要包含data子串才能确保baContent中返回数据
        attachment	把返回的数据放到附件中,并返回附件的id。已经废除。

        2.控制返回的数据
        metadata	返回metadata信息
        timestamp	返回timestamp
        length		数据总长度，始终都有值
        data		返回数据体
        outputpath  实际使用的路径     // respath		返回记录路径,目前始终都有值
        all			返回所有值

        3.控制记录号
        prev		前一条
        prev,myself	自己或前一条
        next		下一条
        next,myself	自己或下一条
        实际访问的记录路径会放到strOutputResPath参数里
        */
        //		baContent	    用content字节数组返回资源内容
        //		strAttachmentID	用附件返回资源内容
        //		strMetadata	    返回的metadata内容
        //		strOutputResPath	返回的资源路径
        //		baTimestamp	    返回的资源时间戳
        // result:
        //		result.value
        //				-1	出错
        //				>=0	表示资源总长度
        // 代码实现:
        //		根据不同的风格取不同的资源,不可混合使用
        //		根据不同的风格返回不同的值,可混合使用
        public Result GetRes(string strResPath,
            long nStart,
            int nLength,
            string strStyle,
            out byte[] baContent,
            // out string strAttachmentID,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp)
        {
            baContent = null;
            baOutputTimestamp = null;
            // strAttachmentID = "";
            strMetadata = "";
            strOutputResPath = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                if (String.IsNullOrEmpty(strResPath) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    result.ErrorString = "资源路径为空，不合法。";
                    return result;
                }

                if (StringUtil.IsInList("attachment", strStyle) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    result.ErrorString = "不再支持attachment style。";
                    return result;
                }

                // 每次的最大长度
                int nMaxLength = 0;
                if (StringUtil.IsInList("attachment", strStyle) == true)
                    nMaxLength = 300 * 1024; //300K;   
                else
                    nMaxLength = 100 * 1024; //100K;   

                string strError = "";
                // byte[] baData = null;
                int nAdditionError = 0;
                // return:
                //		-1	一般性错误
                //		-4	未找到路径指定的资源
                //		-5	未找到数据库
                //		-6	没有足够的权限
                //		-7	路径不合法
                //		-10	未找到记录xpath对应的节点
                //      -50 有一个以上下级资源记录不存在
                //		>= 0	成功，返回最大长度
                long nRet = app.Dbs.API_GetRes(strResPath,
                    nStart,
                    nLength,
                    strStyle,
                    user,
                    nMaxLength,
                    out baContent,
                    out strMetadata,
                    out strOutputResPath,
                    out baOutputTimestamp,
                    out nAdditionError,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode((int)nRet);
                    result.ErrorString = strError;
                    return result;
                }

                result.Value = nRet;   // 总长度

                // 压缩内容
                // 2017/10/6
                if (StringUtil.IsInList("gzip", strStyle)
                    && baContent != null && baContent.Length > 0)
                {
                    baContent = ByteArray.CompressGzip(baContent);
                    result.ErrorCode = ErrorCodeValue.Compressed;
                }

                if (nAdditionError == -50)
                {
                    result.ErrorCode = ErrorCodeValue.NotFoundSubRes;    // 2006/7/3
                    result.ErrorString = strError;
                }

                /*
                // 给content赋值
                if (StringUtil.IsInList("content", strStyle) == true)
                {
                    baContent = baData;
                }
                 * */

                // 给strAttachmentID赋值
                if (StringUtil.IsInList("attachment", strStyle) == true)
                {
                    /*
                                    MemoryStream s = new MemoryStream();
                                    s.Write(baData,
                                        0,
                                        baData.Length);  //注意s不能关闭

                                    SoapContext responseContext = ResponseSoapContext.Current;
                                    Attachment  dimeImage = new  Attachment (
                                        "application/octect-stream", 
                                        s);

                                    strAttachmentID = "uri:" + Guid.NewGuid().ToString();
                                    dimeImage.Id = strAttachmentID;
                                    responseContext.Attachments.Add(dimeImage);
                     */

                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    result.ErrorString = "目前不支持attachment风格。";
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "GetRes() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 成批写入XML记录
        // 每个元素中Xml成员内放了一条完整的XML记录。如果记录不完整，请不要使用此API。
        // results中返回和inputs一样数目的元素，每个元素表示对应的inputs元素写入是否成功，返回时间戳和实际写入的路径
        // 在中途出错的情况下，results中的元素数目会比inputs中的少，但从前到后顺序是固定的，可以对应
        public Result WriteRecords(
            RecordBody[] inputs,
            string strStyle,
            out RecordBody[] results)
        {
            results = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            List<RecordBody> output_records = new List<RecordBody>();

            try
            {
                // 检查是否登录
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                string strError = "";
                int nRet = 0;

                if (StringUtil.IsInList("renameResultset", strStyle) == true)
                {
                    DpResultSet resultSet = null;

                    string strOldName = StringUtil.GetStyleParam(strStyle, "oldname");
                    string strNewName = StringUtil.GetStyleParam(strStyle, "newname");
                    if (KernelApplication.IsGlobalResultSetName(strOldName) == true)
                    {
                        if (KernelApplication.IsGlobalResultSetName(strNewName) == false)
                        {
                            strError = "strStyle '" + strStyle + "' 中 newname 参数值 '" + strNewName + "' 应该和 oldname 一致，为全局结果集名称形态";
                            throw new ArgumentException(strError);
                        }

                        resultSet = app.ResultSets.GetResultSet(strOldName.Substring(1), true);
                        app.ResultSets.RenameResultSet(strOldName.Substring(1), strNewName.Substring(1));
                    }
                    else
                    {
                        if (KernelApplication.IsGlobalResultSetName(strNewName) == true)
                        {
                            strError = "strStyle '" + strStyle + "' 中 newname 参数值 '" + strNewName + "' 应该和 oldname 一致，为通道结果集名称形态";
                            throw new ArgumentException(strError);
                        }

                        resultSet = this.sessioninfo.GetResultSet(strOldName, true);
                        this.sessioninfo.SetResultSet1(strOldName, null);
                        this.sessioninfo.SetResultSet1(strNewName, resultSet);
                    }

                    // 设为永久属性
                    if (StringUtil.IsInList("permanent", strStyle) == true)
                        resultSet.Permanent = true;

                    // 顺便进行排序
                    if (StringUtil.IsInList("sort", strStyle) == true)
                        resultSet.Sort();

                    return result;
                }

                if (StringUtil.IsInList("createResultset", strStyle) == true)
                {
                    DpResultSet resultSet = null;

                    string strResultSetName = StringUtil.GetStyleParam(strStyle, "name");
                    if (KernelApplication.IsGlobalResultSetName(strResultSetName) == true)
                        resultSet = app.ResultSets.GetResultSet(strResultSetName.Substring(1), true);
                    else
                        resultSet = this.sessioninfo.GetResultSet(strResultSetName, true);

                    // 设为永久属性
                    if (StringUtil.IsInList("permanent", strStyle) == true)
                        resultSet.Permanent = true;

                    lock (result)
                    {
                        if (StringUtil.IsInList("clear", strStyle) == true && resultSet != null)
                            resultSet.Clear();

                        foreach (RecordBody body in inputs)
                        {
                            // body.Path 要翻译为 kernel 内部形态
                            DigitalPlatform.rms.DatabaseCollection.PathInfo info = null;
                            // 解析资源路径
                            // return:
                            //      -1  一般性错误
                            //		-5	未找到数据库
                            //		-7	路径不合法
                            //      0   成功
                            nRet = app.Dbs.ParsePath(body.Path,
                out info,
                out strError);
                            if (nRet <= -1)
                            {
                                result.Value = -1;
                                result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                                result.ErrorString = strError;
                                return result;
                            }
                            Database database = app.Dbs.GetDatabase(info.DbName);

                            DpRecord record = new DpRecord(database.FullID + "/" + info.RecordID10);
                            resultSet.Add(record);
                        }
                    }

                    return result;
                }

                // 得到当前帐户对象
                if (PrepareUser(ref result) == -1)
                    return result;

                nRet = app.Dbs.API_WriteRecords(
                    // this.sessioninfo,
                    user,
                    inputs,
                    strStyle,
                    out output_records,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;

                    // 前面部分成功的，连同最后一条出错信息，也要返回
                    if (output_records != null)
                    {
#if NO
                        results = new RecordBody[output_records.Count];
                        output_records.CopyTo(results);
#endif
                        results = output_records.ToArray();
                    }
                    return result;
                }

                // 全部成功
                Debug.Assert(output_records != null, "");
                results = new RecordBody[output_records.Count];
                output_records.CopyTo(results);
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "WriteRecords() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                // result依然返回错误
                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;

                // 前面部分成功的，也要返回
                if (output_records != null)
                {
#if NO
                    results = new RecordBody[output_records.Count];
                    output_records.CopyTo(results);
#endif
                    results = output_records.ToArray();
                }
                return result;
            }
        }

#if NOOOOO
        // 成批写入XML记录
        // 每个元素中Xml成员内放了一条完整的XML记录。如果记录不完整，请不要使用此API。
        // results中返回和inputs一样数目的元素，每个元素表示对应的inputs元素写入是否成功，返回时间戳和实际写入的路径
        // 在中途出错的情况下，results中的元素数目会比inputs中的少，但从前到后顺序是固定的，可以对应
        public Result WriteRecords(
            RecordBody[] inputs,
            string strStyle,
            out RecordBody[] results)
        {
            results = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            List<RecordBody> output_records = new List<RecordBody>();

            try
            {
                // 检查是否登录
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                // 得到当前帐户对象
                if (PrepareUser(ref result) == -1)
                    return result;

                foreach (RecordBody record in inputs)
                {
                    int nRet = 0;
                    string strError = "";
                    string strOutputValue = "";
                    string strOutputResPath = "";
                    byte[] baOutputTimestamp = null;


                    byte[] baContent = Encoding.UTF8.GetBytes(record.Xml);
                    long lTotalLength = baContent.Length;
                    string strRanges = "0-" + (lTotalLength - 1).ToString();
                    // return:
                    //		-1	一般性错误
                    //		-2	时间戳不匹配
                    //		-4	未找到路径指定的资源
                    //		-5	未找到数据库
                    //		-6	没有足够的权限
                    //		-7	路径不合法
                    //		-8	已经存在同名同类型的项
                    //		-9	已经存在同名但不同类型的项
                    //		0	成功
                    nRet = app.Dbs.API_WriteRes(record.Path,
                        strRanges,
                        lTotalLength,
                        baContent,
                        // streamContent,
                        record.Metadata,
                        strStyle,
                        record.Timestamp,
                        user,
                        out strOutputResPath,
                        out baOutputTimestamp,
                        out strOutputValue,
                        out strError);
                    RecordBody output_record = new RecordBody();
                    output_records.Add(output_record);
                    if (nRet <= -1)
                    {
                        output_record.Result.Value = -1;
                        output_record.Result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                        output_record.Result.ErrorString = strError;
                    }
                    if (strOutputValue != "")
                        output_record.Result.ErrorString = strOutputValue;    // 特殊情况：取局部记录时，明明没有错，也要把局部信息放在Result.ErrorString中返回。建议增添一个专用的out参数

                    output_record.Timestamp = baOutputTimestamp;
                    output_record.Path = strOutputResPath;
                }

                // 全部成功
                results = new RecordBody[output_records.Count];
                output_records.CopyTo(results);

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "BulkWriteRes() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

#if NO
                // 引起异常的一条，加入results
                {
                    RecordBody output_record = new RecordBody();
                    output_records.Add(output_record);

                    output_record.Result.Value = -1;
                    output_record.Result.ErrorCode = ErrorCodeValue.CommonError;
                    output_record.Result.ErrorString = strErrorText;
                }
#endif

                // result依然返回错误
                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;

                // 前面部分成功的，连同最后一条出错信息，也要返回
                results = new RecordBody[output_records.Count];
                output_records.CopyTo(results);
                return result;
            }
        }

#endif

        // 写资源
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录体，对象资源，部分记录体
        //						配置事项: 库名/配置事项路径
        //						记录体: 库名/记录号
        //						对象资源: 库名/记录号/object/资源ID
        //						部分记录体: 库名/记录/xpath/<locate>hitcount</locate><action>AddInteger</action> 或者 库名/记录/xpath/@hitcount
        //		strRanges		目标的位置,多个range用逗号分隔,null认为是空字符串，空字符串认为是0-(lTotalLength-1)
        //		lTotalLength	资源总长度,可以为 0。如果为 -1，表示仅修改 metadata
        //		baContent		用byte[]数据传送的资源内容，如果为null则表示是0字节的数组
        //		strAttachmentID	用附件传送的资源内容,null认为是空字符串
        //		strMetadata		元数据内容，null认为是空字符串，注:有些元数据虽然传过来，但服务器不认，比如长度
        //		strStyle		风格,null认为是空字符串
        //						ignorechecktimestamp 忽略时间戳;
        //						createdir,创建目录,路径表示待创建的目录路径
        //						autocreatedir	自动创建中间层的目录
        //						content	数据放在baContent参数里
        //						attachment	数据放在附件里
        //		baInputTimestamp	输入的时间戳,对于创建目录，不检查时间戳
        //		strOutputResPath	返回的资源路径
        //							比如追加记录时，返回实际的路径
        //							其它资源返回的路径与输入的路径相同
        //		baOutputTimestamp	返回时间戳
        //							当为目录时，返回的时间戳为null
        // return:
        //      result.Value    -1  出错
        //                      0   成功
        // 说明：
        //		本函数实际代表了两种情况，新建资源，覆盖资源
        //		baContent，strAttachmentID只能使用一个，与strStyle配置使用
        public Result WriteRes(string strResPath,
            string strRanges,
            long lTotalLength,
            byte[] baContent,
            // string strAttachmentID,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out string strOutputResPath,
            out byte[] baOutputTimestamp)
        {
            baOutputTimestamp = null;
            strOutputResPath = strResPath;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                // 检查是否登录
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

#if NO
                // 检查输入参数是否合法，并做规范化处理
                if (strResPath == null
                    || strResPath == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.PathError;
                    result.ErrorString = "资源路径'" + strResPath + "'不合法，不能为null或空字符串。";
                    return result;
                }
                if (lTotalLength < 0)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    result.ErrorString = "WriteRes()，lTotalLength不能为'" + Convert.ToString(lTotalLength) + "'，必须>=0。";
                    return result;
                }
#endif

                // 得到要写入的字节数组,并且判断给定的范围是否合法
                //Microsoft.Web.Services2.Attachments.Attachment attachment = null;
                // Stream streamContent = null;
                if (StringUtil.IsInList("attachment", strStyle) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.CommonError;
                    result.ErrorString = "目前不支持attachment风格。";
                    return result;

                }
                else
                {

                    // 目前content风格没有用了
                    /*
                    if (String.IsNullOrEmpty(strAttachmentID) == false)
                    {
                        result.Value = -1;
                        result.ErrorCode = ErrorCodeValue.CommonError;
                        result.ErrorString = "style风格中指定'content'，所以strAttachmentID不应再赋值。只能为null或者空字符串";
                        return result;
                    }*/

                    if (baContent == null)
                        baContent = new byte[0];
                }


                // 得到当前帐户对象
                if (PrepareUser(ref result) == -1)
                    return result;

                // 2017/10/7
                if (StringUtil.IsInList("gzip", strStyle)
                    && baContent != null && baContent.Length > 0)
                {
                    baContent = ByteArray.DecompressGzip(baContent);
                }

                // 调数据库集合的WriteRes();
                // return:
                //		-1	一般性错误
                //		-2	时间戳不匹配
                //		-4	未找到路径指定的资源
                //		-5	未找到数据库
                //		-6	没有足够的权限
                //		-7	路径不合法
                //		-8	已经存在同名同类型的项
                //		-9	已经存在同名但不同类型的项
                //		0	成功
                int nRet = app.Dbs.API_WriteRes(strResPath,
                    strRanges,
                    lTotalLength,
                    baContent,
                    // streamContent,
                    strMetadata,
                    strStyle,
                    baInputTimestamp,
                    user,
                    out strOutputResPath,
                    out baOutputTimestamp,
                    out string strOutputValue,
                    out string strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }
                if (strOutputValue != "")
                    result.ErrorString = strOutputValue;    // 特殊情况：取局部记录时，明明没有错，也要把局部信息放在Result.ErrorString中返回。建议增添一个专用的out参数

                return result;
            }
            catch (Exception ex)
            {
                if (ex is TailNumberException)
                    app.ActivateWorker();

                string strErrorText = "WriteRes() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 删除资源，可以是记录 或 配置事项，不支持对象资源或部分记录体
        // 本函数也可以用于删除数库库对象。此功能和DeleteDb() API一样。
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						资源类型可以是数据库配置事项(目录或文件)，记录
        //						配置事项: 库名/配置事项路径
        //						记录: 库名/记录号
        //		baInputTimestamp	输入的时间戳
        //		baOutputTimestamp	返回的时间戳
        // return:
        //		-1	出错
        //		0	成功
        // 说明: 
        // 1)删除需要当前帐户对将被删除的记录的有delete权限		
        // 2)删除记录的明确含义是删除记录体，并且删除该记录包含的所有对象资源
        // 3)删除配置目录不要求时间戳,同时baOutputTimestamp也是null
        public Result DeleteRes(string strResPath,
            byte[] baInputTimestamp,
            string strStyle,
            out byte[] baOutputTimestamp)
        {
            baOutputTimestamp = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (strResPath == null || strResPath == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.PathError;
                    result.ErrorString = "资源路径'" + strResPath + "'不合法，不能为null或空字符串。";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                string strError;
                int nRet = 0;
                // return:
                //      -1	一般性错误，例如输入参数不合法等
                //      -2	时间戳不匹配
                //      -4	未找到路径对应的资源
                //      -5	未找到数据库
                //      -6	没有足够的权限
                //      -7	路径不合法
                //      0	操作成功
                nRet = app.Dbs.API_DeleteRes(strResPath,
                    user,
                    baInputTimestamp,
                    strStyle,
                    out baOutputTimestamp,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "DeleteRes() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 刷新和数据库记录相关的keys
        // parameter:
        //		strResPath		资源路径,不能为null或空字符串
        //						记录: 库名/记录号
        //      strStyle    next prev outputpath forcedeleteoldkeys
        //                  forcedeleteoldkeys 要在创建新keys前强制删除一下旧有的keys? 如果为包含，则强制删除原有的keys；如果为不包含，则试探着创建新的keys，如果有旧的keys和新打算创建的keys重合，那就不重复创建；如果旧的keys有残余没有被删除，也不管它们了
        //                          包含 一般用在单条记录的处理；不包含 一般用在预先删除了所有keys表的内容行以后在循环重建库中每条记录的批处理方式
        /*      
                3.控制记录号
        prev		前一条
        prev,myself	自己或前一条
        next		下一条
        next,myself	自己或下一条
        实际记录的path放到strOutputResPath参数里
        */
        // return:
        //		-1	出错
        //		0	成功
        // 说明: 
        // 1) 重建keys需要当前帐户对将被处理的记录的有overwrite权限		
        public Result RebuildResKeys(string strResPath,
            string strStyle,
            out string strOutputResPath)
        {
            strOutputResPath = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (String.IsNullOrEmpty(strResPath) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.PathError;
                    result.ErrorString = "资源路径'" + strResPath + "'不合法，不能为null或空字符串。";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                string strError;
                int nRet = 0;
                // return:
                //      -1	一般性错误，例如输入参数不合法等
                //      -2	时间戳不匹配
                //      -4	未找到路径对应的资源
                //      -5	未找到数据库
                //      -6	没有足够的权限
                //      -7	路径不合法
                //      0	操作成功
                nRet = app.Dbs.API_RebuildResKeys(strResPath,
                    user,
                    strStyle,
                    out strOutputResPath,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "RebuildResKeys() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }


        // 修改数据库基本信息
        // parameter:
        //		strDbName	        数据库名称
        //		strLang	            对应的语言版本，如果语言版本为null或者为空字符串，则从所有的语言版本中找
        //		logicNames	        LogicNameItem数组
        //		strType	            数据库类型,以逗号分隔，可以是file,accout，目前无效，因为涉及到是文件库，还是sql库的问题
        //		strSqlDbName	    指定的新Sql数据库名称,，目前无效
        //		strKeysText         keys配置信息
        //		strBrowseText       browse配置信息
        public Result SetDbInfo(string strDbName,
            LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysText,
            string strBrowseText)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                // 得到当前帐户对象
                if (PrepareUser(ref result) == -1)
                    return result;


                // 执行
                string strError = "";
                // return:
                //      -1  一般性错误
                //      -2  已存在同名的数据库
                //      -5  未找到数据库对象
                //      -6  没有足够的权限
                //      0   成功
                int nRet = app.Dbs.API_SetDbInfo(user,
                    strDbName,
                    logicNames,
                    strType,
                    strSqlDbName,
                    strKeysText,
                    strBrowseText,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "SetDbInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 获得数据库基本信息
        // parameter:
        //		strDbName	        数据库名称
        //      strStyle            获得那些输出参数? all表示全部 分别指定则是logicnames/type/sqldbname/keystext/browsetext
        //		logicNames	        out参数，返回LogicNameItem数组
        //		strType	            out参数，返回数据库类型,以逗号分隔，可以是file,accout
        //		strSqlDbName	    out参数，返回指定的Sql数据库名称,可以为null，系统自动生成一个
        //		strKeysText         out参数，返回keys配置信息
        //		strBrowseText	    out参数，返回browse配置信息
        public Result GetDbInfo(string strDbName,
            string strStyle,
            out LogicNameItem[] logicNames,
            out string strType,
            out string strSqlDbName,
            out string strKeysText,
            out string strBrowseText)
        {
            logicNames = null;
            strType = "";
            strSqlDbName = "";
            strKeysText = "";
            strBrowseText = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                // 得到当前帐户对象
                if (PrepareUser(ref result) == -1)
                    return result;


                // 执行
                string strError = "";
                // return:
                //      -1  一般性错误
                //      -5  未找到数据库对象
                //      -6  没有足够的权限
                //      0   成功
                int nRet = app.Dbs.API_GetDbInfo(
                    true,
                    user,
                    strDbName,
                    strStyle,
                    out logicNames,
                    out strType,
                    out strSqlDbName,
                    out strKeysText,
                    out strBrowseText,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "GetDbInfo() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 新建数据库
        // 不带初始化数据库功能
        // parameter:
        //		logicNames	        LogicNameItem数组
        //		strType	            数据库类型,以逗号分隔，可以是file,account
        //		strSqlDbName	    指定的Sql数据库名称,可以为null，系统自动生成一个
        //		strkeysDefault	    keys配置信息
        //		strBrowseDefault	browse配置信息
        // return:
        //      Result.Value    -1  出错
        //                      0   成功
        public Result CreateDb(LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysDefault,
            string strBrowseDefault)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                // 判断是否是登录状态
                if (string.IsNullOrEmpty(this.sessioninfo.UserName) == true)
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;

                string strError = "";
                // return:
                //      -3	在新建库中，发现已经存在同名数据库, 本次不能创建
                //      -2	没有足够的权限
                //      -1	一般性错误，例如输入参数不合法等
                //      0	操作成功
                int nRet = app.Dbs.API_CreateDb(user,
                    logicNames,
                    strType,
                    strSqlDbName,
                    strKeysDefault,
                    strBrowseDefault,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    if (nRet == -1)
                        result.ErrorCode = ErrorCodeValue.CommonError;
                    else if (nRet == -2)
                        result.ErrorCode = ErrorCodeValue.NotHasEnoughRights;
                    else if (nRet == -3)
                        result.ErrorCode = ErrorCodeValue.AlreadyExist;
                    else
                    {
                        Debug.Assert(false, "");
                        result.ErrorCode = ErrorCodeValue.CommonError;
                    }

                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "CreateDb() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 删除数据库
        // parameters
        //		strDbName	数据库名称，可以是任意逻辑号名，或者id号
        public Result DeleteDb(string strDbName)
        {
            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                // 权限
                // 检查用户对这个库是否有读权限
                if (PrepareUser(ref result) == -1)
                    return result;

                string strError = "";
                // return:
                //		-1	出错
                //      -4  数据库不存在  2008/4/27
                //      -5  未找到数据库
                //		-6	无足够的权限
                //		0	成功
                int nRet = app.Dbs.API_DeleteDb(user,
                    strDbName,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "DeleteDb() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }


        // 根据前端发来的Xml数据模拟创建检索点
        // parameter:
        //		strXml      xml数据
        //		strRecPath	记录路径
        //		lStart      起始序号
        //		lLength     长度
        //		strLang     语言版本
        //		strStyle    样式,控制返回值
        //		keys	返回的检索点数组
        public Result CreateKeys(string strXml,
            string strRecPath,
            int lStart,
            int lLength,
            string strLang,
            // string strStyle, // 2008/3/19 删除
            out KeyInfo[] keys)
        {
            keys = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                //是否登录的判断
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                //

                return API_CreateKeys(
                    strXml,
                    strRecPath,
                    lStart,
                    lLength,
                    strLang,
                    // strStyle,
                    out keys);
            }
            catch (Exception ex)
            {
                string strErrorText = "CreateKeys() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }


        // 以下建议单独写成一个中层函数
        Result API_CreateKeys(
            string strXml,
            string strRecPath,
            int lStart,
            int lLength,
            string strLang,
            // string strStyle,
            out KeyInfo[] keys)
        {
            keys = null;
            Result result = new Result();

            //找到数据库
            DbPath dbPath = new DbPath(strRecPath);
            Database db = app.Dbs.GetDatabaseSafety(dbPath.Name);
            if (db == null)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.NotFoundDb;
                result.ErrorString = "PretendWrite(),通过'" + dbPath.Name + "'未找到数据库对象";
                return result;
            }

            string strError = "";
            KeyCollection allKeys = null;
            // return:
            //		-1	出错
            //		0	成功
            int nRet = db.API_PretendWrite(strXml,
                dbPath.ID,
                strLang,
                // strStyle,
                out allKeys,
                out strError);
            if (nRet <= -1)
            {
                result.Value = -1;
                result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                result.ErrorString = strError;
                return result;
            }
            result.Value = allKeys.Count;


            int nMaxLength = 500;
            long lOutputLength = 0;
            // return:
            //		-1  出错
            //		0   成功
            nRet = ConvertUtil.GetRealLength((int)lStart,
                (int)lLength,
                allKeys.Count,
                nMaxLength,
                out lOutputLength,
                out strError);
            if (nRet == -1)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strError;
                return result;
            }

            keys = new KeyInfo[lOutputLength];
            for (int i = 0; i < lOutputLength; i++)
            {
                KeyItem dpKey = (KeyItem)allKeys[i + lStart];
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.ID = dpKey.RecordID;
                keyInfo.Key = dpKey.Key;
                keyInfo.KeyNoProcess = dpKey.KeyNoProcess;
                keyInfo.FromValue = dpKey.FromValue;
                keyInfo.Num = dpKey.Num;
                keyInfo.FromName = dpKey.FromName;
                keys[i] = keyInfo;
            }

            //this.GetSessionInfo().Keys = allKeys;
            return result;
        }

        // 拷贝一条源记录到目标记录
        // 根据目标记录路径追加或覆盖
        // parameter:
        //		strOriginRecordPath	源记录路径
        //		strTargetRecordPath	目标记录路径
        //		bDeleteOriginRecord	是否删除源记录
        //		strOutputRecordPath	输出参数，返回目标记录路径，当追加的时候有用
        //		baOutputRecordTimestamp	输出参数，返回目标记录的时间戳
        public Result CopyRecord(string strOriginRecordPath,
            string strTargetRecordPath,
            bool bDeleteOriginRecord,
            string strMergeStyle,
            out string strIdChangeList,
            out string strOutputRecordPath,
            out byte[] baOutputRecordTimestamp)
        {
            strOutputRecordPath = "";
            baOutputRecordTimestamp = null;
            strIdChangeList = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                // 判断是否是登录状态
                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                if (PrepareUser(ref result) == -1)
                    return result;


                string strError = "";
                // return:
                //		-1	一般性错误
                //      -4  未找到记录
                //      -5  未找到数据库
                //      -6  没有足够的权限
                //      -7  路径不合法
                //		0	成功
                int nRet = app.Dbs.API_CopyRecord(user,
                    strOriginRecordPath,
                    strTargetRecordPath,
                    bDeleteOriginRecord,
                    strMergeStyle,
                    out strIdChangeList,
                    out strOutputRecordPath,
                    out baOutputRecordTimestamp,
                    out strError);
                if (nRet <= -1)
                {
                    result.Value = -1;
                    result.ErrorCode = KernelApplication.Ret2ErrorCode(nRet);
                    result.ErrorString = strError;
                    return result;
                }

                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "CopyRecord() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }

        // 2013/3/13
        // 操作一个批处理任务
        public Result BatchTask(
            string strName,
            string strAction,
            TaskInfo info,
            out TaskInfo[] results)
        {
            string strError = "";
            results = null;

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {
                int nRet = 0;

                TaskInfo resultInfo = null;

                if (strAction == "start")
                {
                    nRet = app.StartBatchTask(strName,
                        info,
                        out resultInfo,
                        out strError);
                    if (resultInfo != null)
                    {
                        results = new TaskInfo[1];
                        results[0] = resultInfo;
                    }
                }
                else if (strAction == "stop")
                {
                    nRet = app.StopBatchTask(strName,
                        info,
                        out results,
                        out strError);
                }
                else if (strAction == "continue")
                {
                    nRet = app.StartBatchTask("!continue",
                        null,
                        out resultInfo,
                        out strError);
                    if (resultInfo != null)
                    {
                        results = new TaskInfo[1];
                        results[0] = resultInfo;
                    }
                }
                else if (strAction == "pause")
                {
                    nRet = app.StopBatchTask("!pause",
                        null,
                        out results,
                        out strError);
                }
                else if (strAction == "getinfo")
                {
                    if (string.IsNullOrEmpty(strName) == true || strName == "*")
                    {
                        nRet = app.ListBatchTasks(
                           out results,
                           out strError);
                    }
                    else
                    {
                        nRet = app.GetBatchTaskInfo(strName,
                            info,
                            out results,
                            out strError);
                        if (resultInfo != null)
                        {
                            results = new TaskInfo[1];
                            results[0] = resultInfo;
                        }
                    }
                }
                else
                {
                    strError = "不能识别的strAction参数 '" + strAction + "'";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;

                result.Value = nRet;
                return result;
            ERROR1:
                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strError;
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "BatchTask() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }


        // 返回通道信息，目前只包括用户名
        // parameter:
        //		strProperty	返回通道信息xml字符串
        public Result GetProperty(out string strProperty)
        {
            strProperty = "";

            Result result = this.PrepareEnvironment(true);
            if (result.Value == -1)
                return result;

            try
            {

                if (this.sessioninfo.UserName == "")
                {
                    result.Value = -1;
                    result.ErrorCode = ErrorCodeValue.NotLogin;
                    result.ErrorString = "尚未登录";
                    return result;
                }

                strProperty = "<root username='" + this.sessioninfo.UserName + "' />";
                return result;
            }
            catch (Exception ex)
            {
                string strErrorText = "GetProperty() API出现异常: " + ExceptionUtil.GetDebugText(ex);
                app.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCodeValue.CommonError;
                result.ErrorString = strErrorText;
                return result;
            }
        }
    }

    public class HostInfo : IExtension<ServiceHostBase>, IDisposable
    {
        // public object LockObject = new object();

        private MyReaderWriterLock m_lock = new MyReaderWriterLock();
        private int m_nLockTimeOut = 1000 * 60;	//1分钟

        ServiceHostBase owner = null;
        public KernelApplication App = null;
        public string DataDir = ""; // 数据目录

        // 2017/9/3
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
            if (this.App != null)
            {
                // lock (this.LockObject)
                this.LockForWrite();
                try
                {
                    this.App.Close();
                    this.App.Dispose(); // 2016/1/25
                    this.App = null;
                }
                finally
                {
                    this.UnlockForWrite();
                }
            }
        }

        public void LockForWrite()
        {
            this.m_lock.AcquireWriterLock(this.m_nLockTimeOut);
        }

        public void UnlockForWrite()
        {
            this.m_lock.ReleaseWriterLock();
        }

        public void LockForRead()
        {
            this.m_lock.AcquireReaderLock(this.m_nLockTimeOut);
        }

        public void UnlockForRead()
        {
            this.m_lock.ReleaseReaderLock();
        }
    }
}
