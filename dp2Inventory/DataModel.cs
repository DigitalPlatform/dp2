using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Xml;
using System.IO;
using System.Windows.Forms;

using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using static dp2Inventory.LibraryChannelUtil;

namespace dp2Inventory
{
    public static class DataModel
    {
        // 当前采用的通讯协议
        public static string Protocol
        {
            get
            {
                if (string.IsNullOrEmpty(sipServerAddr) == false)
                    return "sip";
                return "dp2library";
            }
        }


        #region 配置参数

        // RFID 中心 URL
        public static string RfidCenterUrl
        {
            get
            {
                return ClientInfo.Config.Get("rfid", "rfidCenterUrl", null);
            }
            set
            {
                ClientInfo.Config.Set("rfid", "rfidCenterUrl", value);
            }
        }

        // 扫描前倒计时秒数
        public static int BeforeScanSeconds
        {
            get
            {
                return ClientInfo.Config.GetInt("rfid", "beforeScanSeconds", 5);
            }
            set
            {
                ClientInfo.Config.SetInt("rfid", "beforeScanSeconds", value);
            }
        }

        // 启用 R-PAN 标签类型切换
        public static bool RfidRpanTypeSwitch
        {
            get
            {
                return ClientInfo.Config.GetBoolean("rfid", "rpanTypeSwitch", true);
            }
            set
            {
                ClientInfo.Config.SetBoolean("rfid", "rpanTypeSwitch", value);
            }
        }

        // dp2library 服务器 URL 是否被序列号锁定
        public static bool is_dp2libraryServerUrl_locked
        {
            get
            {
                var url = FormClientInfo.GetSerialCodeFunctionValueByPrefix("dp2library:");
                if (string.IsNullOrEmpty(url) == false)
                    return true;
                return false;
            }
        }

        public static string dp2libraryServerUrl
        {
            get
            {
                // 如果序列号的 function 中有 dp2library:xxx 部分，xxx 表示锁定的 dp2library URL
                var url = FormClientInfo.GetSerialCodeFunctionValueByPrefix("dp2library:");
                if (string.IsNullOrEmpty(url) == false)
                    return url;
                return ClientInfo.Config.Get("dp2library", "serverUrl", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2library", "serverUrl", value);
            }
        }

        public static string dp2libraryUserName
        {
            get
            {
                return ClientInfo.Config.Get("dp2library", "userName", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2library", "userName", value);
            }
        }

        public static string dp2libraryPassword
        {
            get
            {
                string password = ClientInfo.Config.Get("dp2library", "password", null);
                return DecryptPasssword(password);
            }
            set
            {
                string password = EncryptPassword(value);
                ClientInfo.Config.Set("dp2library", "password", password);
            }
        }

        public static string dp2libraryLocation
        {
            get
            {
                return ClientInfo.Config.Get("dp2library", "location", null);
            }
            set
            {
                ClientInfo.Config.Set("dp2library", "location", value);
            }
        }

        public static string sipServerAddr
        {
            get
            {
                return ClientInfo.Config.Get("sip", "serverAddr", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "serverAddr", value);
            }
        }

        public static int sipServerPort
        {
            get
            {
                return ClientInfo.Config.GetInt("sip", "serverPort", 8100);
            }
            set
            {
                ClientInfo.Config.SetInt("sip", "serverPort", value);
            }
        }

        public static string sipUserName
        {
            get
            {
                return ClientInfo.Config.Get("sip", "userName", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "userName", value);
            }
        }

        public static string sipPassword
        {
            get
            {
                string password = ClientInfo.Config.Get("sip", "password", null);
                return DecryptPasssword(password);
            }
            set
            {
                string password = EncryptPassword(value);
                ClientInfo.Config.Set("sip", "password", password);
            }
        }

        public static string sipEncoding
        {
            get
            {
                return ClientInfo.Config.Get("sip", "encoding", "utf-8");
            }
            set
            {
                ClientInfo.Config.Set("sip", "encoding", value);
            }
        }

        public static string sipInstitution
        {
            get
            {
                return ClientInfo.Config.Get("sip", "institution", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "institution", value);
            }
        }

        // SIP 馆藏地列表。逗号分隔的字符串
        public static string sipLocationList
        {
            get
            {
                return ClientInfo.Config.Get("sip", "locationList", null);
            }
            set
            {
                ClientInfo.Config.Set("sip", "locationList", value);
            }
        }

        // 变换为 List<string> 并去掉中间的空字符串事项
        public static List<string> GetSipLocationList()
        {
            var lines = StringUtil.SplitList(sipLocationList, ",");
            StringUtil.RemoveBlank(ref lines);
            return lines;
        }

        // 启用 SIP 本地存储
        public static bool sipLocalStore
        {
            get
            {
                return ClientInfo.Config.GetBoolean("sip", "localStore", false);
            }
            set
            {
                ClientInfo.Config.SetBoolean("sip", "localStore", value);
            }
        }

        // 上传接口 URL
        public static string uploadInterfaceUrl
        {
            get
            {
                return ClientInfo.Config.Get("uploadInterface", "url", null);
            }
            set
            {
                ClientInfo.Config.Set("uploadInterface", "url", value);
            }
        }

        // 启用标签缓存
        // 缺省为 false (2021/5/12)
        public static bool EnableTagCache
        {
            get
            {
                return ClientInfo.Config.GetBoolean("general", "enableTagCache", false);
            }
            set
            {
                ClientInfo.Config.SetBoolean("general", "enableTagCache", value);
            }
        }

        // PII 号码校验规则
        public static string PiiVerifyRule
        {
            get
            {
                return ClientInfo.Config.Get("general", "pii_verify_rule", null);
            }
            set
            {
                ClientInfo.Config.Set("general", "pii_verify_rule", value);
            }
        }

        static string EncryptKey = "dp2inventory_key";

        internal static string DecryptPasssword(string strEncryptedText)
        {
            if (String.IsNullOrEmpty(strEncryptedText) == false)
            {
                try
                {
                    string strPassword = Cryptography.Decrypt(
        strEncryptedText,
        EncryptKey);
                    return strPassword;
                }
                catch
                {
                    return "errorpassword";
                }
            }

            return "";
        }

        internal static string EncryptPassword(string strPlainText)
        {
            return Cryptography.Encrypt(strPlainText, EncryptKey);
        }

        #endregion


        #region UID table

        // 预先从全部实体记录中准备好 UID 到 PII 的对照关系。这一部分标签就不需要 GetTagData 了
        // UID --> PII
        static Hashtable _uidTable = new Hashtable();

        // 2021/5/12
        public static void ClearUidTable()
        {
            _uidTable.Clear();
        }

        public static void SetUidTable(Hashtable table)
        {
            _uidTable = table;
        }

        // 检查是否存在 UID --> UII(OI.PII) 事项
        public static bool UidExsits(string uid, out string uii)
        {
            uii = (string)_uidTable[uid];
            if (string.IsNullOrEmpty(uii) == false)
            {
                return true;
            }
            return false;
        }

        // 限制本地数据库操作，同一时刻只能一个函数进入
        static AsyncSemaphore _cacheLimit = new AsyncSemaphore(1);

        // public delegate void delegate_showText(string text);

        // 从本地数据库中装载 uid 对照表
        public static async Task<NormalResult> LoadUidTableAsync(Hashtable uid_table,
            delegate_showText func_showProgress,
            CancellationToken token)
        {
            try
            {
                using (var releaser = await _cacheLimit.EnterAsync())
                using (var context = new ItemCacheContext())
                {
                    context.Database.EnsureCreated();

                    long total_count = context.Uids.LongCount();

                    long i = 0;
                    // var all = context.Uids.Where(o => string.IsNullOrEmpty(o.PII) == false && string.IsNullOrEmpty(o.UID) == false);
                    foreach (var item in context.Uids)
                    {
                        if (token.IsCancellationRequested)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = "中断"
                            };

                        string uid = item.UID;
                        string barcode = item.PII;

                        if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(barcode))
                            continue;

                        // 2021/1/31
                        // 跳过那些没有 OI 的
                        ParseOiPii(barcode, out string pii, out string oi);
                        if (string.IsNullOrEmpty(oi))
                        {
                            i++;
                            continue;
                        }

                        if ((i % 100) == 0)
                            func_showProgress?.Invoke($"{uid} --> {barcode} ({i}/{total_count})...", i, total_count);

                        uid_table[uid] = barcode;

                        i++;
                    }

                    return new NormalResult
                    {
                        Value = uid_table.Count,
                    };
                }
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"LoadUidTable() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"LoadUidTable() 出现异常：{ex.Message}"
                };
            }
        }

        #endregion

        public static void ParseOiPii(string text,
    out string pii,
    out string oi)
        {
            pii = "";
            oi = "";

            if (string.IsNullOrEmpty(text))
                return;

            if (text.Contains(".") == false)
            {
                pii = text;
                oi = "";
                return;
            }

            var parts = StringUtil.ParseTwoPart(text, ".");
            oi = parts[0];
            pii = parts[1];
        }

        public static string MakeOiPii(string pii, string oi)
        {
            if (string.IsNullOrEmpty(oi))
                return pii;
            return oi + "." + pii;
        }


        #region SIP2

        public static XmlDocument GetInventoryDom()
        {
            string filename = Path.Combine(ClientInfo.UserDir, "inventory.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(filename);
                return dom;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

#if REMOVED
        static string _sipLocalStore = null;

        public static string SipLocalStore
        {
            get
            {
                if (_sipLocalStore == null)
                    _sipLocalStore = GetSipLocalStoreDef();

                return _sipLocalStore;
            }
        }

        // 获得 inventory.xml 中 sip/@localStore 参数
        public static string GetSipLocalStoreDef()
        {
            var dom = GetInventoryDom();
            if (dom == null)
                return "";
            var attr = dom.DocumentElement.SelectSingleNode("sip/@localStore");
            if (attr == null)
                return "";
            return attr.Value;
        }
#endif

        public static async Task<BookItem> FindBookItemAsync(string barcode)
        {
            using (var releaser = await _cacheLimit.EnterAsync())
            using (var context = new ItemCacheContext())
            {
                return context.Items.Where(o => o.Barcode == barcode).FirstOrDefault();
            }
        }

#endregion
    }

    // 任务完成情况
    public class TaskInfo
    {
        // 任务名
        public string Name { get; set; }
        // 执行结果。Value == 0 表示成功
        public NormalResult Result { get; set; }
    }

    // Entity 附加的处理信息
    public class ProcessInfo
    {
        // 是否已经存在 UID-->UII 对照事项
        public bool FoundUii { get; set; }

        public ListViewItem ListViewItem { get; set; }

        public Entity Entity { get; set; }

        // 状态
        public string State { get; set; }

        // 是否为层架标？
        public bool IsLocation { get; set; }

        // 是否为读者卡？
        public bool IsPatron { get; set; }

        public string ItemXml { get; set; }

        public string GetTagInfoError { get; set; }
        // GetTagInfo() 出错的次数
        public int ErrorCount { get; set; }

        // 批次号
        public string BatchNo { get; set; }

        // 希望修改成的 currentLocation 字段内容
        public string TargetCurrentLocation { get; set; }
        // 希望修改成的 location 字段内容
        public string TargetLocation { get; set; }
        // 希望修改成的 shelfNo 字段内容
        public string TargetShelfNo { get; set; }

        // 希望修改成的 EAS 内容。on/off/(null) 其中 (null) 表示不必进行修改
        public string TargetEas { get; set; }

        public List<TaskInfo> Tasks { get; set; }

        // 操作者(工作人员)用户名
        public string UserName { get; set; }

        // 设置任务信息
        // parameters:
        //      result  要设置的 NormalResult 对象。如果为 null，表示要删除这个任务条目
        public void SetTaskInfo(string name, NormalResult result)
        {
            if (Tasks == null)
                Tasks = new List<TaskInfo>();
            var info = Tasks.Find((t) => t.Name == name);
            if (info == null)
            {
                if (result == null)
                    return;
                Tasks.Add(new TaskInfo
                {
                    Name = name,
                    Result = result
                });
            }
            else
            {
                if (result == null)
                {
                    Tasks.Remove(info);
                    return;
                }
                info.Result = result;
            }
        }

        public bool IsAllTaskCompleted()
        {
            if (Tasks == null)
                return true;
            foreach (var task in Tasks)
            {
                if (task.Result.Value == -1)
                    return false;
            }

            return true;
        }

        // 检测一个任务是否已经完成
        public bool IsTaskCompleted(string name)
        {
            if (Tasks == null)
                return false;

            var info = Tasks.Find((t) => t.Name == name);
            if (info == null)
                return false;
            return info.Result.Value != -1;
        }

        // 探测是否包含指定名字的任务信息
        public bool ContainTask(string name)
        {
            if (Tasks == null)
                return false;

            var info = Tasks.Find((t) => t.Name == name);
            return info != null;
        }
    }

}
