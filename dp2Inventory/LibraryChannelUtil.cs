using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Runtime.Remoting.Activation;
using System.Threading;
using System.Collections;

using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryServer;

namespace dp2Inventory
{
    /// <summary>
    /// 和 dp2library 通道有关的功能
    /// </summary>
    public static class LibraryChannelUtil
    {

        #region LibraryChannel

        // 主要的通道池，用于当前服务器
        public static LibraryChannelPool _channelPool = new LibraryChannelPool();

        public static NormalResult Initial()
        {
            Free();

            if (string.IsNullOrEmpty(DataModel.dp2libraryServerUrl))
                return new NormalResult { Value = 0 };

            _channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            _channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            var result = PrepareConfigDom();
            if (result.Value == -1)
                return result;

            return new NormalResult { Value = 1 };
        }

        public static void Free()
        {
            _channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            _channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
        }

        public class Account
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string LibraryCodeList { get; set; } // 馆代码列表

            public static bool IsGlobalUser(string strLibraryCodeList)
            {
                if (strLibraryCodeList == "*" || string.IsNullOrEmpty(strLibraryCodeList) == true)
                    return true;
                return false;
            }

            public static bool MatchLibraryCode(string strLibraryCode, string strLocationLibraryCode)
            {
                if (IsGlobalUser(strLibraryCode) == true)
                    return true;
                if (strLibraryCode == strLocationLibraryCode)
                    return true;
                return false;
            }
        }

        static Dictionary<string, Account> _accounts = new Dictionary<string, Account>();

        public static Account FindAccount(string userName)
        {
            if (_accounts.ContainsKey(userName) == false)
                return null;
            return _accounts[userName];
        }

        public static void SetAccount(string userName, string password, string libraryCode)
        {
            Account account = null;
            if (_accounts.ContainsKey(userName) == false)
            {
                account = new Account
                {
                    UserName = userName,
                    Password = password,
                    LibraryCodeList = libraryCode,
                };
                _accounts[userName] = account;
            }
            else
            {
                account = _accounts[userName];
                account.Password = password;
            }
        }

        public static void RemoveAccount(string userName)
        {
            if (_accounts.ContainsKey(userName))
                _accounts.Remove(userName);
        }

        internal static void Channel_BeforeLogin(object sender,
DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            if (e.FirstTry == true)
            {
                // TODO: 从工作人员用户名密码记载里面检查，如果是工作人员账户，则 ...
                Account account = FindAccount(channel.UserName);
                if (account != null)
                {
                    e.UserName = account.UserName;
                    e.Password = account.Password;
                }
                else
                {
                    e.UserName = DataModel.dp2libraryUserName;

                    e.Password = DataModel.dp2libraryPassword;

                    bool bIsReader = false;

                    e.Parameters = "location=" + DataModel.dp2libraryLocation;
                    if (bIsReader == true)
                        e.Parameters += ",type=reader";
                }

                e.Parameters += ",client=dp2Inventory|" + ClientInfo.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
                else
                {
                    e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
                    e.Cancel = true;
                }
            }

            // e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
            e.Cancel = true;
        }

        // getbibliosummary 和 order 只要有一个就可以
        // getsystemparameter,getiteminfo,setiteminfo,return
        // static string _baseRights = "getsystemparameter,getbibliosummary,getiteminfo,setiteminfo,return";
        static string _baseRights = "getsystemparameter,getiteminfo,setiteminfo,return";

        static void VerifyRights(string rights)
        {
            List<string> missing_rights = new List<string>();
            var base_rights = StringUtil.SplitList(_baseRights);
            foreach (var right in base_rights)
            {
                if (StringUtil.IsInList(right, rights) == false)
                    missing_rights.Add(right);
            }

            if (missing_rights.Count > 0)
                throw new Exception($"账户 {_currentUserName} 缺乏必备的权限 {StringUtil.MakePathList(missing_rights)}");
        }

        static string _currentUserName = "";

        // public static string ServerUID = "";

        internal static void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            _currentUserName = channel.UserName;

            // 2020/9/18
            // 检查 rights
            VerifyRights(channel.Rights);

            //_currentUserRights = channel.Rights;
            //_currentLibraryCodeList = channel.LibraryCodeList;
        }

        static object _syncRoot_channelList = new object();
        static List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public static void AbortAllChannel()
        {
            lock (_syncRoot_channelList)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public static LibraryChannel GetChannel(string strUserName = "")
        {
            string strServerUrl = DataModel.dp2libraryServerUrl;

            if (string.IsNullOrEmpty(strUserName))
                strUserName = DataModel.dp2libraryUserName;

            LibraryChannel channel = _channelPool.GetChannel(strServerUrl, strUserName);
            lock (_syncRoot_channelList)
            {
                _channelList.Add(channel);
            }
            // TODO: 检查数组是否溢出
            return channel;
        }

        public static void ReturnChannel(LibraryChannel channel)
        {
            _channelPool.ReturnChannel(channel);
            lock (_syncRoot_channelList)
            {
                _channelList.Remove(channel);
            }
        }

        public static void Clear()
        {
            _channelPool.Clear();
        }

        #endregion

        #region dp2library 服务器配置

        // 图书馆名字
        static string _libraryName = null;

        static internal List<string> _locationList = null;

        // 2020/7/15
        // 从 dp2library library.xml 中获取的 RFID 配置信息
        static XmlDocument _rfidCfgDom = null;

        // 获得 dp2library 服务器一端的配置信息
        // exception:
        //      可能会抛出异常
        public static NormalResult PrepareConfigDom()
        {

            {
                // 获得馆藏地列表
                GetLocationListResult result = null;

                result = LibraryChannelUtil.GetLocationList();

                if (result.Value == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"获得馆藏地列表时出错: {result.ErrorInfo}"
                    };
                else
                    _locationList = result.List;
            }

            {
                _rfidCfgDom = new XmlDocument();

                // 获得 RFID 配置信息
                GetRfidCfgResult result = null;
                result = LibraryChannelUtil.GetRfidCfg();

                if (result.Value == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"从 dp2library 服务器获得 RFID 配置信息时出错: {result.ErrorInfo}"
                    };
                else
                {
                    if (string.IsNullOrEmpty(result.Xml))
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"从 dp2library 服务器获得 RFID 配置信息时出错: library.xml 中没有定义 rfid 元素"
                        };
                    }
                    _rfidCfgDom = new XmlDocument();
                    _rfidCfgDom.LoadXml(result.Xml);

                    _libraryName = result.LibraryName;
                }
            }
            return new NormalResult();
        }

        // 注意，只能用于册记录，不能用于读者记录
        /*
<rfid>
<ownerInstitution>
<item map="海淀分馆/" isil="test" />
<item map="西城/" alternative="xc" />
</ownerInstitution>
</rfid>
map 为 "/" 或者 "/阅览室" 可以匹配 "图书总库" "阅览室" 这样的 strLocation
map 为 "海淀分馆/" 可以匹配 "海淀分馆/" "海淀分馆/阅览室" 这样的 strLocation
最好单元测试一下这个函数
* */
        // parameters:
        //      cfg_dom 根元素是 rfid
        //      strLocation 纯净的 location 元素内容。
        //      isil    [out] 返回 ISIL 形态的代码
        //      alternative [out] 返回其他形态的代码
        // return:
        //      true    找到。信息在 isil 和 alternative 参数里面返回
        //      false   没有找到
        public static bool GetOwnerInstitution(
            string strLocation,
            out string isil,
            out string alternative)
        {
            isil = "";
            alternative = "";

        REDO:
            var cfg_dom = _rfidCfgDom;

            if (cfg_dom == null)
            {
                var prepare_result = PrepareConfigDom();
                if (prepare_result.Value == -1)
                    throw new Exception(prepare_result.ErrorInfo);
                goto REDO;
                // return false;
            }

            if (cfg_dom.DocumentElement == null)
                return false;

            return LibraryServerUtil.GetOwnerInstitution(
                cfg_dom.DocumentElement,
                strLocation,
                "entity",
                out isil,
                out alternative);
        }

        public class GetLocationListResult : NormalResult
        {
            public List<string> List { get; set; }
        }

        // 获得馆藏地列表
        public static GetLocationListResult GetLocationList()
        {
            string strOutputInfo = "";
            LibraryChannel channel = GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(30); // dp2library 刚启动后，第一次响应 GetSystemParameter() API 可能比较慢

            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetSystemParameter(
null,
"circulation",
"locationTypes",
out strOutputInfo,
out string strError);
                if (lRet == -1)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestTimeOut
                        || channel.ErrorCode == ErrorCode.ServerTimeout)
                        && nRedoCount < 3)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    return new GetLocationListResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);
            }

            // 
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strOutputInfo;
            }
            catch (Exception ex)
            {
                return new GetLocationListResult
                {
                    Value = -1,
                    ErrorInfo = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message
                };
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
<locationTypes>
    <item canborrow="yes" itembarcodeNullable="yes">流通库</item>
    <item>阅览室</item>
    <library code="分馆1">
        <item canborrow="yes">流通库</item>
        <item>阅览室</item>
    </library>
</locationTypes>
*/

            List<string> results = new List<string>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//item");
            foreach (XmlElement node in nodes)
            {
                string strText = node.InnerText;

                // 
                string strLibraryCode = "";
                XmlElement parent = node.ParentNode as XmlElement;
                if (parent.Name == "library")
                {
                    strLibraryCode = parent.GetAttribute("code");
                }

                results.Add(string.IsNullOrEmpty(strLibraryCode) ? strText : strLibraryCode + "/" + strText);
            }

            return new GetLocationListResult
            {
                Value = 1,
                List = results
            };
        }

        #region GetRfidCfg

        public class GetRfidCfgResult : NormalResult
        {
            public string Xml { get; set; }
            public string LibraryName { get; set; }

            public override string ToString()
            {
                return $"Xml='{Xml}',LibraryName='{LibraryName}'," + base.ToString();
            }
        }

        // 获得 RFID 配置信息
        public static GetRfidCfgResult GetRfidCfg()
        {
            string strOutputInfo = "";
            string libraryName = "";

            LibraryChannel channel = GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                long lRet = channel.GetSystemParameter(
                    null,
                    "system",
                    "rfid",
                    out strOutputInfo,
                    out string strError);
                if (lRet == -1)
                    return new GetRfidCfgResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };

                lRet = channel.GetSystemParameter(
                    null,
                    "library",
                    "name",
                    out libraryName,
                    out strError);
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);
            }

            return new GetRfidCfgResult
            {
                Value = 1,
                Xml = strOutputInfo,
                LibraryName = libraryName,
            };
        }

        #endregion

        #endregion

        #region UID --> UII 对照

        public delegate void delegate_showText(string text, long bytes, long total);

        // parameters:
        //      uid_table   返回 UID --> PII 对照表
        public static NormalResult DownloadUidTable(
            List<string> item_dbnames,
            Hashtable uid_table,
            delegate_showText func_showProgress,
            // Delegate_writeLog writeLog,
            CancellationToken token)
        {
            ClientInfo.WriteInfoLog($"开始下载 UID-->UII 对照关系");
            LibraryChannel channel = GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为册记录检索需要一定时间
            try
            {
                if (item_dbnames == null)
                {
                    long lRet = channel.GetSystemParameter(
    null,
    "item",
    "dbnames",
    out string strValue,
    out string strError);
                    if (lRet == -1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    item_dbnames = StringUtil.SplitList(strValue);
                    StringUtil.RemoveBlank(ref item_dbnames);
                }

                foreach (string dbName in item_dbnames)
                {
                    func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ...", -1, -1);

                    int nRedoCount = 0;
                REDO:
                    if (token.IsCancellationRequested)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "用户中断"
                        };
                    // 检索全部读者库记录
                    long lRet = channel.SearchItem(null,
    dbName, // "<all>",
    "",
    -1,
    "RFID UID", // "__id",
    "left",
    "zh",
    null,   // strResultSetName
    "", // strSearchStyle
    "", // strOutputStyle
    out string strError);
                    if (lRet == -1)
                    {
                        ClientInfo.WriteErrorLog($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                        // 一次重试机会
                        if (lRet == -1
                            && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                            && nRedoCount < 2)
                        {
                            nRedoCount++;
                            goto REDO;
                        }

                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    }

                    long hitcount = lRet;

                    ClientInfo.WriteInfoLog($"{dbName} 共检索命中册记录 {hitcount} 条");


                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;

                    if (hitcount > 0)
                    {
                        string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                        // 把超时时间改短一点
                        var timeout0 = channel.Timeout;
                        channel.Timeout = TimeSpan.FromSeconds(20);

                        try
                        {

                            // 获取和存储记录
                            ResultSetLoader loader = new ResultSetLoader(channel,
                null,
                null,
                strStyle,   // $"id,xml,timestamp",
                "zh");

                            // loader.Prompt += this.Loader_Prompt;
                            int i = 0;
                            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                            {
                                if (token.IsCancellationRequested)
                                    return new NormalResult
                                    {
                                        Value = -1,
                                        ErrorInfo = "用户中断"
                                    };

                                if (record.Cols != null)
                                {
                                    string barcode = "";
                                    if (record.Cols.Length > 0)
                                        barcode = record.Cols[0];
                                    string location = "";
                                    if (record.Cols.Length > 1)
                                        location = record.Cols[1];

                                    // 2021/1/31
                                    // 推算出 OI
                                    string oi = "";
                                    {
                                        location = StringUtil.GetPureLocation(location);
                                        var ret = GetOwnerInstitution(location, out string isil, out string alternative);
                                        if (ret == true)
                                        {
                                            if (string.IsNullOrEmpty(isil) == false)
                                                oi = isil;
                                            else if (string.IsNullOrEmpty(alternative) == false)
                                                oi = alternative;
                                        }
                                    }


                                    string uid = "";
                                    if (record.Cols.Length > 2)
                                        uid = record.Cols[2];
                                    if (string.IsNullOrEmpty(barcode) == false
                                        && string.IsNullOrEmpty(uid) == false)
                                        uid_table[uid] = oi + "." + barcode;
                                }

                                i++;

                                if ((i % 100) == 0)
                                {
                                    func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ({i.ToString()}/{hitcount}) {record.Path} ...", i, hitcount);
                                }
                            }
                        }
                        finally
                        {
                            channel.Timeout = timeout0;
                        }
                    }

                    ClientInfo.WriteInfoLog($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");
                }
                return new NormalResult
                {
                    Value = uid_table.Count,
                };
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"DownloadUidTable() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadUidTable() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);

                ClientInfo.WriteInfoLog($"结束下载 UID-->UII 对照关系");
            }
        }


        #endregion


        public class GetEntityDataResult : NormalResult
        {
            public string Title { get; set; }
            public string ItemXml { get; set; }
            public string ItemRecPath { get; set; }

            // 2021/4/1
            public byte[] ItemTimestamp { get; set; }
        }

        // static bool _cacheDbCreated = false;

        static AsyncSemaphore _channelLimit = new AsyncSemaphore(2);



        // 获得册记录信息和书目摘要信息
        // parameters:
        //      style   风格。network 表示只从网络获取册记录；否则优先从本地获取，本地没有再从网络获取册记录。无论如何，书目摘要都是尽量从本地获取
        // .Value
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static async Task<GetEntityDataResult> GetEntityDataAsync(string pii,
            string style)
        {
            bool network = StringUtil.IsInList("network", style);
            try
            {
                using (var releaser = await _channelLimit.EnterAsync())
                // using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    LibraryChannel channel = GetChannel();
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    try
                    {
                        GetEntityDataResult result = null;
                        List<GetEntityDataResult> errors = new List<GetEntityDataResult>();

                        // ***
                        // 第一步：获取册记录


                        {
                            // 尝试从 dp2library 服务器获取
                            int nRedoCount = 0;
                        REDO_GETITEMINFO:
                            long lRet = channel.GetItemInfo(null,
                                "item",
                                pii,
                                "",
                                "xml",
                                out string item_xml,
                                out string item_recpath,
                                out byte[] timestamp,
                                "",
                                out _,
                                out _,
                                out string strError);
                            if (lRet == -1)
                            {
                                if ((channel.ErrorCode == ErrorCode.RequestError ||
                                    channel.ErrorCode == ErrorCode.RequestTimeOut)
                                    && nRedoCount < 2)
                                {
                                    nRedoCount++;
                                    goto REDO_GETITEMINFO;
                                }
                                // TODO: 这里不着急返回，还需要尝试获得书目摘要
                                errors.Add(new GetEntityDataResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                });
                            }
                            else if (lRet == 0)
                                errors.Add(new GetEntityDataResult
                                {
                                    Value = 0,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                });
                            else
                            {
                                result = new GetEntityDataResult
                                {
                                    Value = 1,
                                    ItemXml = item_xml,
                                    ItemRecPath = item_recpath,
                                    ItemTimestamp = timestamp,
                                    Title = "",
                                };

                            }
                        }

                        // ***
                        /// 第二步：获取书目摘要

                        {
                            // 从 dp2library 服务器获取书目摘要
                            int nRedoCount = 0;
                        REDO_GETBIBLIOSUMMARY:
                            long lRet = channel.GetBiblioSummary(
                null,
                pii,
                "", // strConfirmItemRecPath,
                null,
                out _,
                out string strSummary,
                out string strError);
                            if (lRet == -1)
                            {
                                if ((channel.ErrorCode == ErrorCode.RequestError ||
                channel.ErrorCode == ErrorCode.RequestTimeOut)
                && nRedoCount < 2)
                                {
                                    nRedoCount++;
                                    goto REDO_GETBIBLIOSUMMARY;
                                }

                                strSummary = $"error:" + strError;

                                errors.Add(new GetEntityDataResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString(),
                                    Title = strSummary
                                });
                                /*
                                return new GetEntityDataResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString(),
                                };
                                */
                            }
                            else
                            {
                                strSummary = strSummary?.Replace(". -- ", "\r\n");   // .Replace("/", "\r\n");

                                if (result == null)
                                    result = new GetEntityDataResult();

                                result.Title = strSummary;
                            }
                        }

                        // 完全成功
                        if (result != null && errors.Count == 0)
                            return result;
                        if (result == null)
                            return new GetEntityDataResult
                            {
                                Value = errors[0].Value,
                                ErrorInfo = errors[0].ErrorInfo,
                                ErrorCode = errors[0].ErrorCode
                            };
                        result.ErrorInfo = errors[0].ErrorInfo;
                        result.ErrorCode = errors[0].ErrorCode;
                        if (string.IsNullOrEmpty(result.Title))
                            result.Title = errors[0].Title;
                        return result;
                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        ReturnChannel(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"GetEntityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"GetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        public class RequestInventoryResult : NormalResult
        {
            public string ItemXml { get; set; }
        }

        // 向 dp2library 服务器发出还书请求
        public static RequestInventoryResult RequestReturn(
            string pii,
            string itemRecPath,
            string batchNo,
            string strUserName,
            string style)
        {
            // TODO: 是否要用特定的工作人员身份进行还书?
            LibraryChannel channel = GetChannel(strUserName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                string strStyle = "item";
                string operTimeStyle = "";

                int nRedoCount = 0;
            REDO:
                long lRet = channel.Return(null,
                    "return",
                    "", // _patron.Barcode,
                    pii,    // entity.PII,
                    itemRecPath,
                    false,
                    strStyle + operTimeStyle, // style,
                    "xml", // item_format_list
                    out string[] item_records,
                    "xml",
                    out string[] reader_records,
                    "summary",
                    out string[] biblio_records,
                    out string[] dup_path,
                    out string output_reader_barcode,
                    out ReturnInfo return_info,
                    out string strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotBorrowed)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestError
        || channel.ErrorCode == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            return new RequestInventoryResult
                            {
                                Value = -1,
                                ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                ErrorCode = "requestError"
                            };
                        }
                    }

                    return new RequestInventoryResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                // 更新册记录
                string entity_xml = null;
                if (item_records?.Length > 0)
                    entity_xml = item_records[0];
                return new RequestInventoryResult { ItemXml = entity_xml };
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);
            }
        }

        public class RequestSetUidResult : NormalResult
        {
            public string NewItemXml { get; set; }
            public byte[] NewTimestamp { get; set; }
        }

        // 向 dp2library 服务器发出设置册记录 UID 的请求
        public static RequestSetUidResult RequestSetUID(
            string strRecPath,
            string strOldXml,
            byte[] old_timestamp,
            string uid,
            // string batchNo,
            string strUserName,
            string style)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strOldXml);

            string old_uid = DomUtil.GetElementText(dom.DocumentElement, "uid");
            if (old_uid == uid)
            {
                return new RequestSetUidResult { Value = 0 };    // 没有必要修改
            }
            DomUtil.SetElementText(dom.DocumentElement, "uid", uid);


            List<EntityInfo> entityArray = new List<EntityInfo>();

            {
                EntityInfo item_info = new EntityInfo();

                item_info.OldRecPath = strRecPath;
                item_info.Action = "setuid";
                item_info.NewRecPath = strRecPath;

                item_info.NewRecord = dom.OuterXml;
                item_info.NewTimestamp = null;

                item_info.OldRecord = strOldXml;
                item_info.OldTimestamp = old_timestamp;

                entityArray.Add(item_info);
            }

            // TODO: 是否要用特定的工作人员身份进行盘点?
            LibraryChannel channel = GetChannel(strUserName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.SetEntities(
                 null,
                 "",
                 entityArray.ToArray(),
                 out EntityInfo[] errorinfos,
                 out string strError);
                if (lRet == -1)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestError
        || channel.ErrorCode == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            return new RequestSetUidResult
                            {
                                Value = -1,
                                ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                ErrorCode = "requestError"
                            };
                        }
                    }

                    return new RequestSetUidResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                if (errorinfos == null)
                    return new RequestSetUidResult { };

                List<string> errors = new List<string>();
                string strNewXml = "";
                byte[] baNewTimestamp = null;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    var info = errorinfos[i];

                    if (i == 0)
                    {
                        baNewTimestamp = info.NewTimestamp;
                        strNewXml = info.NewRecord;
                    }

                    // 正常信息处理
                    if (info.ErrorCode == ErrorCodeValue.NoError)
                        continue;

                    errors.Add(info.RefID + " 在提交保存过程中发生错误 -- " + info.ErrorInfo);
                }

                if (errors.Count > 0)
                    return new RequestSetUidResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, ";")
                    };

                return new RequestSetUidResult
                {
                    Value = 1,
                    NewItemXml = strNewXml,
                    NewTimestamp = baNewTimestamp
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);
            }
        }

        // 向 dp2library 服务器发出盘点请求
        public static RequestInventoryResult RequestInventory(string uid,
            string pii,
            string currentLocation,
            string location,
            string shelfNo,
            string batchNo,
            string strUserName,
            string style)
        {
            if (currentLocation == null && location == null)
                return new RequestInventoryResult { Value = 0 };    // 没有必要修改

            // TODO: 是否要用特定的工作人员身份进行盘点?
            LibraryChannel channel = GetChannel(strUserName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                // currentLocation 元素内容。格式为 馆藏地:架号
                // 注意馆藏地和架号字符串里面不应包含逗号和冒号
                List<string> commands = new List<string>();
                if (string.IsNullOrEmpty(currentLocation) == false)
                    commands.Add($"currentLocation:{StringUtil.EscapeString(currentLocation, ":,")}");
                if (string.IsNullOrEmpty(location) == false)
                    commands.Add($"location:{StringUtil.EscapeString(location, ":,")}");
                if (string.IsNullOrEmpty(shelfNo) == false)
                    commands.Add($"shelfNo:{StringUtil.EscapeString(shelfNo, ":,")}");
                if (string.IsNullOrEmpty(batchNo) == false)
                {
                    commands.Add($"batchNo:{StringUtil.EscapeString(batchNo, ":,")}");
                }

                // 2021/5/6
                if (StringUtil.IsInList("forceLog", style))
                {
                    // 即便册记录没有发生修改，也要产生 transfer 操作日志记录。这样便于进行典藏移交清单统计打印
                    commands.Add("forceLog");
                }

                string strStyle = "item";

                int nRedoCount = 0;
            REDO:
                long lRet = channel.Return(null,
                    "transfer",
                    "", // _patron.Barcode,
                    pii,    // entity.PII,
                    null,   // entity.ItemRecPath,
                    false,
                    $"{strStyle},{StringUtil.MakePathList(commands, ",")}", // style,
                    "xml", // item_format_list
                    out string[] item_records,
                    "xml",
                    out string[] reader_records,
                    "summary",
                    out string[] biblio_records,
                    out string[] dup_path,
                    out string output_reader_barcode,
                    out ReturnInfo return_info,
                    out string strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotChanged)
                {
                    if ((channel.ErrorCode == ErrorCode.RequestError
        || channel.ErrorCode == ErrorCode.RequestTimeOut))
                    {
                        nRedoCount++;

                        if (nRedoCount < 2)
                            goto REDO;
                        else
                        {
                            return new RequestInventoryResult
                            {
                                Value = -1,
                                ErrorInfo = "因网络出现问题，请求 dp2library 服务器失败",
                                ErrorCode = "requestError"
                            };
                        }
                    }

                    return new RequestInventoryResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                // 更新册记录
                string entity_xml = null;
                if (item_records?.Length > 0)
                    entity_xml = item_records[0];
                return new RequestInventoryResult { ItemXml = entity_xml };
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);
            }
        }

        public class SimuTagInfo
        {
            public string PII { get; set; }
            public string OI { get; set; }
            public string UID { get; set; }
            public string AccessNo { get; set; }

            public string ReaderName { get; set; }
            public uint AntennaID { get; set; }
        }

        public class TagsInfoResult : NormalResult
        {
            public List<SimuTagInfo> TagInfos { get; set; }
        }

#if REMOVED
        // 从 dp2library 服务器检索获得模拟 RFID 图书标签所需的数据
        // parameters:
        //
        public static TagsInfoResult DownloadTagsInfo(
            List<string> item_dbnames,
            int max_count,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            ClientInfo.WriteInfoLog($"开始 DownloadTagsInfo()");
            LibraryChannel channel = GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为册记录检索需要一定时间
            try
            {
                List<SimuTagInfo> infos = new List<SimuTagInfo>();

                if (item_dbnames == null)
                {
                    long lRet = channel.GetSystemParameter(
    null,
    "item",
    "dbnames",
    out string strValue,
    out string strError);
                    if (lRet == -1)
                        return new TagsInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    item_dbnames = StringUtil.SplitList(strValue);
                    StringUtil.RemoveBlank(ref item_dbnames);
                }

                foreach (string dbName in item_dbnames)
                {
                    func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ...", -1, -1);

                    int nRedoCount = 0;
                REDO:
                    if (token.IsCancellationRequested)
                        return new TagsInfoResult
                        {
                            Value = -1,
                            ErrorInfo = "用户中断"
                        };
                    // 检索全部读者库记录
                    long lRet = channel.SearchItem(null,
    dbName, // "<all>",
    "",
    -1,
    "__id",
    "left",
    "zh",
    null,   // strResultSetName
    "", // strSearchStyle
    "", // strOutputStyle
    out string strError);
                    if (lRet == -1)
                    {
                        ClientInfo.WriteErrorLog($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                        // 一次重试机会
                        if (lRet == -1
                            && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                            && nRedoCount < 2)
                        {
                            nRedoCount++;
                            goto REDO;
                        }

                        return new TagsInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    }

                    long hitcount = lRet;

                    ClientInfo.WriteInfoLog($"{dbName} 共检索命中册记录 {hitcount} 条");


                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;

                    if (hitcount > 0)
                    {
                        // string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                        // 把超时时间改短一点
                        var timeout0 = channel.Timeout;
                        channel.Timeout = TimeSpan.FromSeconds(20);

                        try
                        {
                            // 获取和存储记录
                            ResultSetLoader loader = new ResultSetLoader(channel,
                null,
                null,
                "id,xml",
                "zh");

                            // loader.Prompt += this.Loader_Prompt;
                            int i = 0;
                            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                            {
                                if (token.IsCancellationRequested)
                                    return new TagsInfoResult
                                    {
                                        Value = -1,
                                        ErrorInfo = "用户中断"
                                    };

                                var xml = record.RecordBody.Xml;

                                XmlDocument dom = new XmlDocument();
                                dom.LoadXml(xml);

                                var info = new SimuTagInfo();
                                info.PII = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                                if (string.IsNullOrEmpty(info.PII))
                                    continue;

                                if (info.PII.Contains("_"))
                                    continue;

                                {
                                    string oi = "";
                                    string location = DomUtil.GetElementText(dom.DocumentElement, "location");
                                    location = StringUtil.GetPureLocation(location);
                                    var ret = GetOwnerInstitution(location, out string isil, out string alternative);
                                    if (ret == true)
                                    {
                                        if (string.IsNullOrEmpty(isil) == false)
                                            oi = isil;
                                        else if (string.IsNullOrEmpty(alternative) == false)
                                            oi = alternative;
                                    }
                                    info.OI = oi;
                                }

                                // info.OI = DomUtil.GetElementText(dom.DocumentElement, "oi");

                                if (string.IsNullOrEmpty(info.OI))
                                    continue;

                                info.UID = DomUtil.GetElementText(dom.DocumentElement, "uid");
                                info.AccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
                                infos.Add(info);
                                i++;

                                if (i >= max_count)
                                    break;
                            }
                        }
                        finally
                        {
                            channel.Timeout = timeout0;
                        }
                    }

                    ClientInfo.WriteInfoLog($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");

                }
                return new TagsInfoResult
                {
                    TagInfos = infos,
                };
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"DownloadTagsInfo() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new TagsInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadTagsInfo() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);

                ClientInfo.WriteInfoLog($"结束 DownloadTagsInfo()");
            }
        }

#endif
    }
}
