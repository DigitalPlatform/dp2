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
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using Newtonsoft.Json;
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

        static string _baseRights = "getsystemparameter,getbiblioinfo,getbibliosummary,getiteminfo,getoperlog,getreaderinfo,getres,searchbiblio,searchitem,searchreader,borrow,renew,return,setreaderinfo,writeobject,setiteminfo";

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
            // XmlDocument cfg_dom,
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
            ClientInfo.WriteInfoLog($"开始下载全部册记录到本地缓存");
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

                    // 把超时时间改短一点
                    channel.Timeout = TimeSpan.FromSeconds(20);

                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;

                    if (hitcount > 0)
                    {
                        string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

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

                    ClientInfo.WriteInfoLog($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");
                }
                return new NormalResult
                {
                    Value = uid_table.Count,
                };
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"DownloadItemRecordAsync() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadItemRecordAsync() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                ReturnChannel(channel);

                ClientInfo.WriteInfoLog($"结束下载全部册记录到本地缓存");
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
                        List<NormalResult> errors = new List<NormalResult>();

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
                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
                                });
                            }
                            else if (lRet == 0)
                                errors.Add(new NormalResult
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

                                errors.Add(new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError,
                                    ErrorCode = channel.ErrorCode.ToString()
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

                    /*
                    // 即便册记录没有发生修改，也要产生 transfer 操作日志记录。这样便于进行典藏移交清单统计打印
                    commands.Add("forceLog");
                    */
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



#if NO

        static async Task AddOrUpdateAsync(BiblioCacheContext context,
    BiblioSummaryItem item)
        {
            try
            {
                // 保存到本地数据库
                context.BiblioSummaries.Add(item);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SqliteException sqlite_exception = ex.InnerException as SqliteException;
                if (sqlite_exception != null && sqlite_exception.SqliteErrorCode == 19)
                {
                    // PII 发生重复了
                    goto UPDATE;
                }
                else
                    throw ex;
            }

        UPDATE:
            // 更新到本地数据库
            context.BiblioSummaries.Update(item);
            await context.SaveChangesAsync();
        }


        // 从 OI.PII 中获得 PII 部分
        static string GetPurePII(string text)
        {
            if (text == null)
                return "";
            if (text.Contains(".") == false)
                return text;
            return StringUtil.ParseTwoPart(text, ".")[1];
        }

        // 从本地数据库获得册记录信息和书目摘要信息
        // .Value
        //      0   没有找到
        //      1   找到一种
        //      2   两种都找到了
        public static GetEntityDataResult LocalGetEntityData(string pii)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    if (_cacheDbCreated == false)
                    {
                        context.Database.EnsureCreated();
                        _cacheDbCreated = true;
                    }

                    GetEntityDataResult result = null;

                    // ***
                    // 第一步：获取册记录

                    // 从本地实体库中获得记录
                    var entity_record = context.Entities.Where(o => o.PII == pii).FirstOrDefault();

                    // 2020/9/3
                    // 对没有点的 PII 字符串尝试后方一致匹配
                    if (entity_record == null && pii.IndexOf(".") == -1)
                        entity_record = context.Entities.Where(o => o.PII.EndsWith("." + pii)).FirstOrDefault();

                    // EntityItem entity_record = null;   // testing

                    if (entity_record != null)
                        result = new GetEntityDataResult
                        {
                            Value = 1,
                            ItemXml = entity_record.Xml,
                            ItemRecPath = entity_record.RecPath,
                            Title = "",
                        };

                    // ***
                    /// 第二步：获取书目摘要

                    // 从本地书目库中获取书目摘要

                    var item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();
                    if (item != null
                        && string.IsNullOrEmpty(item.BiblioSummary) == false)
                    {
                        if (result == null)
                            result = new GetEntityDataResult();

                        result.Title = item.BiblioSummary;
                        result.Value++;
                    }

                    if (result == null)
                        return new GetEntityDataResult { Value = 0 };

                    return result;
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"LocalGetEntityDataAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new GetEntityDataResult
                {
                    Value = -1,
                    ErrorInfo = $"LocalGetEntityDataAsync() 出现异常: {ex.Message}",
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        static void SetPII(EntityItem item)
        {
            try
            {
                XmlDocument itemdom = new XmlDocument();
                itemdom.LoadXml(item.Xml);

                string oi = DomUtil.GetElementText(itemdom.DocumentElement, "oi");
                string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");

                if (string.IsNullOrEmpty(oi))
                    item.PII = barcode;
                else
                    item.PII = oi + "." + barcode;
            }
            catch
            {

            }
        }

        static async Task AddOrUpdateAsync(BiblioCacheContext context,
            EntityItem item)
        {
            // 调整 PII 字段，尽量规整为 OI.PII 形态
            SetPII(item);

            try
            {
                // 保存到本地数据库
                context.Entities.Add(item);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                SqliteException sqlite_exception = ex.InnerException as SqliteException;
                if (sqlite_exception != null && sqlite_exception.SqliteErrorCode == 19)
                {
                    // PII 发生重复了
                    goto UPDATE;
                }
                else
                    throw ex;
            }

        UPDATE:
            // 更新到本地数据库
            context.Entities.Update(item);
            await context.SaveChangesAsync();
        }

        public static string GetBiblioSummaryFromLocal(string pii)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    var item = context.BiblioSummaries.Where(o => o.PII == pii).FirstOrDefault();
                    if (item != null
                        && string.IsNullOrEmpty(item.BiblioSummary) == false)
                        return item.BiblioSummary;
                    return "";
                }
            }
            catch
            {
                return null;
            }
        }

        // 2020/9/23
        // 从 dp2library 服务器获得书目摘要
        public static async Task<string> GetBiblioSummaryFromNetworkAsync(string pii)
        {
            using (var releaser = await _channelLimit.EnterAsync())
            {
                LibraryChannel channel = App.CurrentApp.GetChannel();
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(10);
                try
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

                        return null;
                    }

                    return strSummary?.Replace(". -- ", "\r\n");
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }
            }
        }

        // 探测和 dp2library 服务器的通讯是否正常
        // return.Value
        //      -1  本函数执行出现异常
        //      0   网络不正常
        //      1   网络正常
        public static NormalResult DetectLibraryNetwork()
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(5);  // 设置 5 秒超时，避免等待太久
            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetClock(null,
                    out string _,
                    out string strError);
                if (lRet == -1)
                {
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
                        Value = 0,
                        ErrorInfo = strError
                    };
                }

                return new NormalResult { Value = 1 };
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DetectNetwork() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        // result.Value
        //      0   没有找到记录。没有发生更新
        //      1   成功更新
        public static async Task<NormalResult> UpdateEntityXmlAsync(string pii,
            string item_xml,
            byte[] timestamp)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    if (_cacheDbCreated == false)
                    {
                        context.Database.EnsureCreated();
                        _cacheDbCreated = true;
                    }

                    // 先尝试从本地实体库中获得原记录
                    var entity_record = context.Entities.Where(o => o.PII == pii).FirstOrDefault();
                    if (entity_record == null)
                        return new NormalResult { Value = 0 };

                    if (string.IsNullOrEmpty(item_xml) == true)
                    {
                        context.Remove(entity_record);
                    }
                    else
                    {
                        entity_record.Xml = item_xml;
                        entity_record.Timestamp = timestamp;

                        // 保存到本地数据库
                        context.Entities.Update(entity_record);
                    }
                    await context.SaveChangesAsync();
                    return new NormalResult { Value = 1 };
                }
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"UpdateEntityXmlAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"UpdateEntityXmlAsync() 出现异常: {ex.Message}"
                };
            }
        }

        public class SetReaderInfoResult : NormalResult
        {
            public byte[] NewTimestamp { get; set; }
        }

        public static Task<SetReaderInfoResult> SetReaderInfoAsync(string recpath,
            string xml,
            string old_xml,
            byte[] timestamp)
        {
            return Task<SetReaderInfoResult>.Run(() =>
            {
                LibraryChannel channel = App.CurrentApp.GetChannel();
                TimeSpan old_timeout = channel.Timeout;
                channel.Timeout = TimeSpan.FromSeconds(10);
                try
                {
                    long lRet = channel.SetReaderInfo(null,
                        "change",
                        recpath,
                        xml,
                        old_xml,
                        timestamp,
                        out string existing_xml,
                        out string saved_xml,
                        out string saved_recpath,
                        out byte[] new_timestamp,
                        out ErrorCodeValue error_code,
                        out string strError);
                    if (lRet == -1)
                        return new SetReaderInfoResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            NewTimestamp = new_timestamp
                        };
                    if (lRet == 0)
                        return new SetReaderInfoResult
                        {
                            Value = 0,
                            ErrorInfo = strError,
                            NewTimestamp = new_timestamp
                        };
                    return new SetReaderInfoResult
                    {
                        Value = 1,
                        NewTimestamp = new_timestamp
                    };
                }
                finally
                {
                    channel.Timeout = old_timeout;
                    App.CurrentApp.ReturnChannel(channel);
                }
            });
        }

        public class GetReaderInfoResult : NormalResult
        {
            public string RecPath { get; set; }
            public string ReaderXml { get; set; }
            public byte[] Timestamp { get; set; }
        }

        // 从本地数据库获取读者记录
        // parameters:
        //      buildBorrowInfo 是否根据本地动作信息合成 borrows/borrow 元素
        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static GetReaderInfoResult GetReaderInfoFromLocal(string pii,
            bool buildBorrowInfo)
        {
            try
            {
                using (BiblioCacheContext context = new BiblioCacheContext())
                {
                    if (_cacheDbCreated == false)
                    {
                        context.Database.EnsureCreated();
                        _cacheDbCreated = true;
                    }

                    pii = pii?.ToUpper();

                    string query = $",{pii},";
                    var patrons = context.Patrons
                        .Where(o => o.PII == pii || o.Bindings.Contains(query))
                        .ToList();
                    if (patrons.Count == 0)
                    {
                        // 再尝试后方一致匹配一次
                        if (pii != null && pii.Contains(".") == false)
                        {
                            patrons = context.Patrons
                                .Where(o => o.PII.EndsWith($".{pii}"))
                                .ToList();
                        }

                        if (patrons.Count == 0)
                            return new GetReaderInfoResult
                            {
                                Value = 0,
                                ErrorInfo = $"PII 为 '{pii}' 的本地读者记录没有找到"
                            };
                    }

                    // 命中读者记录多于一条
                    if (patrons.Count > 1)
                    {
                        return new GetReaderInfoResult
                        {
                            Value = -1,
                            ErrorInfo = $"装载本地读者记录失败：'{pii}' 检索命中读者记录 {patrons.Count} 条"
                        };
                    }

                    var patron = patrons[0];

                    if (buildBorrowInfo)
                    {
                        // 2020/5/8
                        // 添加用本地信息模拟出来的 borrows/borrow 元素
                        XmlDocument patron_dom = new XmlDocument();
                        patron_dom.LoadXml(patron.Xml);
                        SetBorrowInfo(patron_dom, patron.LastWriteTime);

                        patron.Xml = patron_dom.OuterXml;
                    }

                    return new GetReaderInfoResult
                    {
                        Value = 1,
                        RecPath = patron.RecPath,
                        ReaderXml = patron.Xml,
                        Timestamp = patron.Timestamp
                    };
                }
            }
            catch (Exception ex)
            {
                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = $"装载本地读者记录(PII 为 '{pii}')时出现异常: {ex.Message}"
                };
            }
        }

        static string GetPii(XmlDocument dom)
        {
            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(pii))
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
            return pii;
        }

        // 获得读者的 PII。注意包含了 OI 部分
        static string GetPatronOiPii(XmlDocument dom)
        {
            string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(pii))
            {
                pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                return pii;
            }

            string oi = DomUtil.GetElementText(dom.DocumentElement, "oi");
            if (oi == null)
                oi = "";

            // 2020/9/25
            // 如果读者记录中没有 oi 元素，则从 libraryCode 元素推导
            if (string.IsNullOrEmpty(oi))
            {
                string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
                var ret = ShelfData.GetOwnerInstitution(libraryCode + "/", out string isil, out string alternative);
                if (ret == true)
                {
                    if (string.IsNullOrEmpty(isil) == false)
                        oi = isil;
                    else if (string.IsNullOrEmpty(alternative) == false)
                        oi = alternative;
                }
            }

            // 注意返回的是严格形态
            return oi + "." + pii;
        }


        // 获得 oi.pii 的 oi 部分
        public static string GetOiPart(string oi_pii, bool return_null)
        {
            if (oi_pii.IndexOf(".") == -1)
            {
                if (return_null)
                    return null;
                return "";
            }
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[0];
        }

        // 获得 oi.pii 的 pii 部分
        public static string GetPiiPart(string oi_pii)
        {
            if (oi_pii.IndexOf(".") == -1)
                return oi_pii;
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[1];
        }

        // 根据本地历史记录，在读者记录中添加 borrows/borrow 元素
        // parameters:
        //      lastWriteTime   读者 XML 记录最近更新时间。只取这个时间以后的本地未还借书动作
        static NormalResult SetBorrowInfo(XmlDocument patron_dom,
            DateTime lastWriteTime)
        {
            string pii = GetPii(patron_dom);

            bool changed = false;
            XmlElement root = patron_dom.DocumentElement.SelectSingleNode("borrows") as XmlElement;
            if (root == null)
            {
                root = patron_dom.CreateElement("borrows");
                patron_dom.DocumentElement.AppendChild(root);
            }

            /*
            // 删除原有 borrows/borrow 元素
            if (root.ChildNodes.Count > 0)
            {
                root.RemoveAll();
                changed = true;
            }
            */


            using (var context = new RequestContext())
            {
                // 显示该读者的在借册情况
                var borrows = context.Requests
                    .Where(o => o.OperatorID == pii && o.Action == "borrow" && o.LinkID == null
                    && o.OperTime > lastWriteTime)
                    .OrderBy(o => o.ID).ToList();
                /* testing
                var borrows = context.Requests
    .Where(o => o.OperatorID == pii && o.Action == "borrow")
    .OrderBy(o => o.ID).ToList();
                */
                foreach (var item in borrows)
                {
                    // 2020/6/20
                    // 查重 合并 barcode 相同的 borrow 元素
                    // 注意，XPath 中 and 不能用大写，只能用小写
                    var dup = root.SelectSingleNode($"borrow[@barcode='{GetPiiPart(item.PII)}' and @oi='{GetOiPart(item.PII, false)}']") as XmlElement;
                    if (dup != null)
                        continue;

                    var borrow_info = JsonConvert.DeserializeObject<BorrowInfo>(item.ActionString);

                    XmlElement new_borrow = patron_dom.CreateElement("borrow");
                    root.AppendChild(new_borrow);
                    // var title = GetEntityTitle(item.EntityString);

                    new_borrow.SetAttribute("barcode", GetPiiPart(item.PII));
                    new_borrow.SetAttribute("oi", GetOiPart(item.PII, false));
                    new_borrow.SetAttribute("borrowDate", DateTimeUtil.Rfc1123DateTimeStringEx(item.OperTime));
                    if (borrow_info != null)
                    {
                        /*
{"BorrowCount":0,
"BorrowOperator":"supervisor",
"DenyPeriod":"",
"ItemBarcode":"T0000131",
"LatestReturnTime":"Mon, 08 Jun 2020 12:00:00 +0800",
"Overflows":null,
"Period":"31day"}
* */
                        new_borrow.SetAttribute("returningDate", borrow_info.LatestReturnTime);
                        new_borrow.SetAttribute("period", borrow_info.Period);
                        if (borrow_info.Overflows != null)
                            new_borrow.SetAttribute("overflow", string.Join("; ", borrow_info.Overflows));
                        new_borrow.SetAttribute("no", borrow_info.BorrowCount.ToString());
                    }
                    changed = true;
                }
            }

            if (changed)
                return new NormalResult { Value = 1 };
            return new NormalResult();
        }


        // 把读者记录保存(更新)到本地数据库
        // result.Value
        //      -1  出错
        //      0   没有发生修改
        //      1   发生了创建或者修改
        public static NormalResult UpdateLocalPatronRecord(
            GetReaderInfoResult get_result,
            DateTime lastWriteTime)
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(get_result.ReaderXml);
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者记录装载进入 XMLDOM 时出错:{ex.Message}",
                        ErrorCode = "loadXmlError"
                    };
                }
                /*
                string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(pii))
                    pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                    */
                string oi_pii = GetPatronOiPii(dom);
                var patron = context.Patrons
    .Where(o => o.PII == oi_pii)
    .FirstOrDefault();
                if (patron != null)
                {
                    // 如果已经存在的读者记录比打算写入的要新，则放弃写入
                    if (patron.LastWriteTime > lastWriteTime)
                        return new NormalResult { Value = 0 };
                    Set(patron, dom);
                    context.Patrons.Update(patron);
                }
                else
                {
                    patron = new PatronItem
                    {
                        PII = oi_pii?.ToUpper(),
                    };
                    Set(patron, dom);
                    context.Patrons.Add(patron);
                }

                context.SaveChanges();
                return new NormalResult { Value = 1 };
            }

            void Set(PatronItem patron, XmlDocument dom)
            {
                string cardNumber = DomUtil.GetElementText(dom.DocumentElement, "cardNumber");
                cardNumber = cardNumber.ToUpper();
                if (string.IsNullOrEmpty(cardNumber) == false)
                    cardNumber = "," + cardNumber + ",";

                if (get_result.RecPath != null)
                    patron.RecPath = get_result.RecPath;
                patron.Bindings = cardNumber;
                patron.Xml = get_result.ReaderXml;
                patron.Timestamp = get_result.Timestamp;
                patron.LastWriteTime = lastWriteTime;

#if REMOVED
                // 2020/9/25
                // 把 PII 规整为包含 OI 的形态
                if (patron.PII == null
                    || patron.PII?.IndexOf(".") == -1 || patron.PII.StartsWith("."))
                {
                    string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    if (string.IsNullOrEmpty(pii))
                        pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");

                    if (pii.StartsWith("@") == false)
                    {
                        string libraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
                        var ret = ShelfData.GetOwnerInstitution(libraryCode + "/", out string isil, out string alternative);
                        if (ret == true)
                        {
                            // 应该是 xxx.xxx 形态
                            if (string.IsNullOrEmpty(isil) == false)
                                pii = isil + "." + pii;
                            else if (string.IsNullOrEmpty(alternative) == false)
                                pii = alternative + "." + pii;
                        }
                        WpfClientInfo.WriteInfoLog($"写入本地读者缓存以前，修正 PII '{patron.PII}' 为 '{pii}' (UpdateLocalPatronRecord())");
                        patron.PII = pii;
                    }
                }
#endif
            }
        }

        public static NormalResult DeleteLocalPatronRecord(string strXml)
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"读者记录装载进入 XMLDOM 时出错:{ex.Message}",
                        ErrorCode = "loadXmlError"
                    };
                }
                /*
                string pii = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(pii))
                    pii = "@refID:" + DomUtil.GetElementText(dom.DocumentElement, "refID");
                    */
                string oi_pii = GetPatronOiPii(dom);
                var patron = context.Patrons
    .Where(o => o.PII == oi_pii)
    .FirstOrDefault();
                if (patron != null)
                {
                    context.Patrons.Remove(patron);
                    context.SaveChanges();
                }

                return new NormalResult { Value = 0 };
            }
        }

        public static PatronItem GetPatronItem(string pii)
        {
            using (BiblioCacheContext context = new BiblioCacheContext())
            {
                if (_cacheDbCreated == false)
                {
                    context.Database.EnsureCreated();
                    _cacheDbCreated = true;
                }

                var patron = context.Patrons
    .Where(o => o.PII == pii)
    .FirstOrDefault();
                return patron;
            }
        }

        // return.Value:
        //      -1  出错
        //      0   读者记录没有找到
        //      1   成功
        public static GetReaderInfoResult GetReaderInfo(string pii)
        {
            /*
            reader_xml = "";
            recpath = "";
            timestamp = null;
            */
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new GetReaderInfoResult
                {
                    Value = -1,
                    ErrorInfo = "dp2library 服务器 URL 尚未配置，无法获得读者信息"
                };
            LibraryChannel channel = App.CurrentApp.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(5);  // 设置 5 秒超时，避免等待太久
            try
            {
                int nRedoCount = 0;
            REDO:
                long lRet = channel.GetReaderInfo(null,
                    pii,
                    "advancexml", // "xml",
                    out string[] results,
                    out string recpath,
                    out byte[] timestamp,
                    out string strError);
                if (lRet == -1 || lRet == 0)
                {
                    // 2020/4/24 增加一次重试机会
                    if (lRet == -1
                        && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }

                    // 如果发生通讯失败，则主动重新探测一次网络状况
                    if (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                    {
                        ShelfData.DetectLibraryNetwork();
                    }

                    return new GetReaderInfoResult
                    {
                        Value = (int)lRet,
                        ErrorInfo = strError,
                        RecPath = recpath,
                        Timestamp = timestamp
                    };
                }

                // 2019/12/19
                // 命中读者记录多于一条
                if (lRet > 1)
                {
                    return new GetReaderInfoResult
                    {
                        Value = -1,
                        ErrorInfo = $"装载读者记录失败：'{pii}' 检索命中读者记录 {lRet} 条"
                    };
                }

                string reader_xml = "";
                if (results != null && results.Length > 0)
                    reader_xml = results[0];
                return new GetReaderInfoResult
                {
                    Value = 1,
                    RecPath = recpath,
                    Timestamp = timestamp,
                    ReaderXml = reader_xml
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public class LoginResult : NormalResult
        {
            public string OutputUserName { get; set; }
            public string Rights { get; set; }
            public string LibraryCode { get; set; }
        }

        // result.Value
        //      -1:   出错
        //      0:    登录未成功
        //      1:    登录成功
        public static LoginResult WorkerLogin(string userName, string password)
        {
            if (string.IsNullOrEmpty(App.dp2ServerUrl) == true)
                return new LoginResult
                {
                    Value = -1,
                    ErrorInfo = "dp2library 服务器 URL 尚未配置，无法进行工作人员登录"
                };
            LibraryChannel channel = App.CurrentApp.GetChannel(userName);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                // -1:   出错
                // 0:    登录未成功
                // 1:    登录成功
                long lRet = channel.Login(userName,
                    password,
                    "type=worker,client=dp2ssl|" + WpfClientInfo.ClientVersion,
                    out string strOutputUserName,
                    out string strRights,
                    out string strLibraryCode,
                    out string strError);
                if (lRet == -1 || lRet == 0)
                    return new LoginResult
                    {
                        Value = (int)lRet,
                        ErrorInfo = strError,
                    };

                // testing
                // channel.Logout(out strError);

                return new LoginResult
                {
                    Value = 1,
                    OutputUserName = strOutputUserName,
                    Rights = strRights,
                    LibraryCode = strLibraryCode,
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }



        // 从本地获得馆藏地列表(不访问 dp2library 服务器)
        public static GetLocationListResult GetLocationListFromLocal()
        {
            string value = WpfClientInfo.Config.Get("cache",
    "locationList",
    null);
            if (value == null)
                return new GetLocationListResult();

            return new GetLocationListResult
            {
                Value = 1,
                List = JsonConvert.DeserializeObject<List<string>>(value)
            };
        }

        public class GetRightsTableResult : NormalResult
        {
            public string Xml { get; set; }
        }

        // 获得读者权限定义 XML
        public static GetRightsTableResult GetRightsTable()
        {
            LibraryChannel channel = App.CurrentApp.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromSeconds(10);

            try
            {
                long lRet = channel.GetSystemParameter(
        null,
        "circulation",
        "rightsTable",
        out string strOutputInfo,
        out string strError);
                if (lRet == -1)
                    return new GetRightsTableResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };

                if (string.IsNullOrEmpty(strOutputInfo) == false)
                    strOutputInfo = "<rightsTable>" + strOutputInfo + "</rightsTable>";

                return new GetRightsTableResult
                {
                    Value = (int)lRet,
                    Xml = strOutputInfo
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        public delegate void delegate_showText(string text);

#endif

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

                    // 把超时时间改短一点
                    channel.Timeout = TimeSpan.FromSeconds(20);

                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;

                    if (hitcount > 0)
                    {
                        // string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

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


    }
}
