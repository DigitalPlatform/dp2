using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

// using Microsoft.AspNet.SignalR.Client.Hubs;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.MessageClient;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 负责消息交换的类
    /// 能响应外部发来的检索请求
    /// </summary>
    public class MessageHub : MessageConnection
    {
        // 检索响应的事件
        public event SearchResponseEventHandler SearchResponseEvent = null;

        public MainForm MainForm = null;

        public WebBrowser webBrowser1 = null;

        internal LibraryChannelPool _channelPool = new LibraryChannelPool();

        public void Initial(MainForm main_form,
            WebBrowser webBrowser)
        {
            this.MainForm = main_form;
            this.webBrowser1 = webBrowser;

            ClearHtml();

            this._channelPool.BeforeLogin += new BeforeLoginEventHandle(_channelPool_BeforeLogin);

#if NO
            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false)
            {
                this.MainForm.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
            }

            _timer.Interval = 1000 * 30;
            _timer.Elapsed += _timer_Elapsed;
#endif
            this.RefreshUserName();

            base.Initial();
        }

        public void RefreshUserName()
        {
            // 2016/4/24
            this.UserName = this.MainForm.MessageUserName;
            this.Password = this.MainForm.MessagePassword;
            Hashtable table = new Hashtable();
            table["libraryUID"] = this.MainForm.ServerUID;    // 测试用 Guid.NewGuid().ToString(),
            table["libraryName"] = this.MainForm.LibraryName;
            table["propertyList"] = (this.ShareBiblio ? "biblio_search" : "");
            table["libraryUserName"] = this.MainForm.GetCurrentUserName();
            this.Parameters = StringUtil.BuildParameterString(table, ',', '=', "url");
        }

        public override void Destroy()
        {
            base.Destroy();

            this._channelPool.BeforeLogin -= new BeforeLoginEventHandle(_channelPool_BeforeLogin);
            this._channelPool.Close();
        }

        void _channelPool_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.LibraryServerUrl = this.MainForm.LibraryServerUrl;
                bool bIsReader = false;

                e.UserName = this.MainForm.AppInfo.GetString(
                "default_account",
                "username",
                "");

                e.Password = this.MainForm.AppInfo.GetString(
"default_account",
"password",
"");
                e.Password = this.MainForm.DecryptPasssword(e.Password);

                bIsReader =
this.MainForm.AppInfo.GetBoolean(
"default_account",
"isreader",
false);
                Debug.Assert(this.MainForm != null, "");

                string strLocation = this.MainForm.AppInfo.GetString(
                "default_account",
                "location",
                "");
                e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                // 2014/9/13
                e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

#if SN
                // 从序列号中获得 expire= 参数值
                string strExpire = this.MainForm.GetExpireParam();
                if (string.IsNullOrEmpty(strExpire) == false)
                    e.Parameters += ",expire=" + strExpire;
#endif

                e.Parameters += ",client=dp2circulation|" + Program.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            // TODO: 可以出现对话框，但要注意跨线程的问题
            // TODO: 当首次登录对话框没有输入密码的时候，这里就必须出现对话框询问密码了
            e.Cancel = true;
        }

        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.MainForm.LibraryServerUrl;
            string strUserName = this.MainForm.DefaultUserName;

            return this._channelPool.GetChannel(strServerUrl, strUserName);
        }

        public override string dp2MServerUrl
        {
            get
            {
                // dp2MServer URL
                return this.MainForm.AppInfo.GetString("config",
                    "im_server_url",
                    "http://dp2003.com:8083/dp2MServer");
            }
            set
            {
                this.MainForm.AppInfo.SetString("config",
                    "im_server_url",
                    value);
            }
        }

        #region IE 控件显示

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.MainForm.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                this.webBrowser1.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public override void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            this.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(DateTime.Now.ToShortTimeString() + " " + strText).Replace("\r\n", "<br/>") + "</div>");
        }

#if NO
        void AddInfoLine(string strContent)
        {
#if NO
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
#endif
            OutputText(strContent, 0);
        }
#endif

        public override void OnAddMessageRecieved(string strName, string strContent)
        {
            if (strName == null)
                strName = "";
            if (strContent == null)
                strContent = "";

            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strName).Replace("\r\n", "<br/>") + "</div>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

#if NO
        void AddErrorLine(string strContent)
        {
#if NO
            string strText = "<div class='item error'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
#endif
            OutputText(strContent, 2);
        }
#endif

        #endregion


        // bool tryingToReconnect = false;

        // 响应 server 发来的消息 SearchBiblio
        public override void OnSearchBiblioRecieved(
#if NO
            string searchID,
            string dbNameList,
             string queryWord,
             string fromList,
             string matchStyle,
             string formatList,
             long maxResults
#endif
SearchRequest searchParam
            )
        {
            // 单独给一个线程来执行
            Task.Factory.StartNew(() => SearchAndResponse(searchParam));
        }

#if NO
        public override void OnSetInfoRecieved(SetInfoRequest request)
        {
            Task.Factory.StartNew(() => SetInfoAndResponse(request));
        }
#endif

#if NO
        暂时不启用
        void GetPatronInfo(SearchRequest searchParam)
        {
            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.MessageClient.Record> records = new List<DigitalPlatform.MessageClient.Record>();

            if (string.IsNullOrEmpty(searchParam.FormatList) == true)
            {
                strError = "FormatList 不应为空";
                goto ERROR1;
            }

            LibraryChannel channel = GetChannel();
            try
            {
                string[] results = null;
                string strRecPath = "";
                byte[] baTimestamp = null;

                long lRet = channel.GetReaderInfo(null,
                    searchParam.QueryWord,
                    searchParam.FormatList,
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                strErrorCode = channel.ErrorCode.ToString();
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0
                        || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                    {
                        // 没有命中
                        ResponseSearch(
searchParam.TaskID,
0,
0,
records,
strError,  // 出错信息大概为 not found。
strErrorCode);
                        return;
                    }
                    goto ERROR1;
                }

                if (results == null)
                    results = new string[0];

                records.Clear();
                string[] formats = searchParam.FormatList.Split(new char[] { ',' });
                int i = 0;
                foreach (string result in results)
                {
                    DigitalPlatform.MessageClient.Record biblio = new DigitalPlatform.MessageClient.Record();
                    biblio.RecPath = strRecPath;
                    biblio.Data = result;
                    biblio.Format = formats[i];
                    records.Add(biblio);
                    i++;
                }

                ResponseSearch(
                    searchParam.TaskID,
                    records.Count,  // lHitCount,
                    0, // lStart,
                    records,
                    "",
                    strErrorCode);
            }
            catch (Exception ex)
            {
                AddErrorLine("GetPatronInfo() 出现异常: " + ex.Message);
                strError = "GetPatronInfo() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError,
strErrorCode);
        }
#endif
        static void SetValue(Entity entity, EntityInfo info)
        {
            if (entity.OldRecord != null)
            {
                info.OldRecPath = entity.OldRecord.RecPath;
                info.OldRecord = entity.OldRecord.Data;
                info.OldTimestamp = ByteArray.GetTimeStampByteArray(entity.OldRecord.Timestamp);
            }

            if (entity.NewRecord != null)
            {
                info.NewRecPath = entity.NewRecord.RecPath;
                info.NewRecord = entity.NewRecord.Data;
                info.NewTimestamp = ByteArray.GetTimeStampByteArray(entity.NewRecord.Timestamp);
            }

            info.Action = entity.Action;
            info.RefID = entity.RefID;
            info.Style = entity.Style;
        }

        static void SetValue(EntityInfo info, Entity entity)
        {
            entity.OldRecord = new DigitalPlatform.MessageClient.Record();
            {
                entity.OldRecord.RecPath = info.OldRecPath;
                entity.OldRecord.Data = info.OldRecord;
                entity.OldRecord.Timestamp = ByteArray.GetHexTimeStampString(info.OldTimestamp);
            }

            entity.NewRecord = new DigitalPlatform.MessageClient.Record();
            {
                entity.NewRecord.RecPath = info.NewRecPath;
                entity.NewRecord.Data = info.NewRecord;
                entity.NewRecord.Timestamp = ByteArray.GetHexTimeStampString(info.NewTimestamp);
            }

            entity.Action = info.Action;
            entity.RefID = info.RefID;
            entity.Style = info.Style;
            entity.ErrorInfo = info.ErrorInfo;
            entity.ErrorCode = info.ErrorCode.ToString();
        }

        // TODO: 本函数最好放在一个工作线程内执行
        // Form Close 的时候要及时中断工作线程
        void SearchAndResponse(SearchRequest searchParam)
        {
#if NO
            if (searchParam.Operation == "getPatronInfo")
            {
                GetPatronInfo(searchParam);
                return;
            }
#endif

            string strError = "";
            string strErrorCode = "";
            IList<DigitalPlatform.MessageClient.Record> records = new List<DigitalPlatform.MessageClient.Record>();
            long batch_size = 50;

            string strResultSetName = searchParam.ResultSetName;
            if (string.IsNullOrEmpty(strResultSetName) == true)
                strResultSetName = "default";
            else
                strResultSetName = "#" + strResultSetName;  // 如果请求方指定了结果集名，则在 dp2library 中处理为全局结果集名

            LibraryChannel channel = GetChannel();
            try
            {
                string strQueryXml = "";
                long lRet = 0;

                if (searchParam.QueryWord == "!getResult")
                {
                    lRet = -1;
                }
                else
                {
                    if (searchParam.Operation == "searchBiblio")
                    {
                        lRet = channel.SearchBiblio(null,
                             searchParam.DbNameList,
                             searchParam.QueryWord,
                             (int)searchParam.MaxResults,
                             searchParam.UseList,
                             searchParam.MatchStyle,
                             "zh",
                             strResultSetName,
                             "", // strSearchStyle
                             "", // strOutputStyle
                             out strQueryXml,
                             out strError);
                    }
                    else if (searchParam.Operation == "searchPatron")
                    {
                        lRet = channel.SearchReader(null,
                            searchParam.DbNameList,
                            searchParam.QueryWord,
                            (int)searchParam.MaxResults,
                            searchParam.UseList,
                            searchParam.MatchStyle,
                            "zh",
                            strResultSetName,
                            "",
                            out strError);
                    }
                    else
                    {
                        lRet = -1;
                        strError = "无法识别的 Operation 值 '" + searchParam.Operation + "'";
                    }

                    strErrorCode = channel.ErrorCode.ToString();

                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0
                            || (lRet == -1 && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound))
                        {
                            // 没有命中
                            ResponseSearch(
    searchParam.TaskID,
    0,
    0,
    records,
    strError,  // 出错信息大概为 not found。
    strErrorCode);
                            return;
                        }
                        goto ERROR1;
                    }
                }


                {
                    long lHitCount = lRet;

                    if (searchParam.Count == 0)
                    {
                        // 返回命中数
                        ResponseSearch(
                            searchParam.TaskID,
                            lHitCount,
0,
records,
"本次没有返回任何记录",
strErrorCode);
                        return;
                    }

                    long lStart = searchParam.Start;
                    long lPerCount = searchParam.Count; // 本次拟返回的个数

                    if (lHitCount != -1)
                    {
                        if (lPerCount == -1)
                            lPerCount = lHitCount - lStart;
                        else
                            lPerCount = Math.Min(lPerCount, lHitCount - lStart);

                        if (lPerCount <= 0)
                        {
                            strError = "命中结果总数为 " + lHitCount + "，取结果开始位置为 " + lStart + "，它已超出结果集范围";
                            goto ERROR1;
                        }
                    }

                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        string strBrowseStyle = searchParam.FormatList; // "id,xml";

                        lRet = channel.GetSearchResult(
            null,
            strResultSetName,
            lStart,
            lPerCount,
            strBrowseStyle,
            "zh", // this.Lang,
            out searchresults,
            out strError);
                        strErrorCode = channel.ErrorCode.ToString();
                        if (lRet == -1)
                            goto ERROR1;

                        if (searchresults.Length == 0)
                        {
                            strError = "GetSearchResult() searchResult empty";
                            goto ERROR1;
                        }

                        if (lHitCount == -1)
                            lHitCount = lRet;   // 延迟得到命中总数

                        records.Clear();
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                        {
#if NO
                            DigitalPlatform.MessageClient.Record biblio = new DigitalPlatform.MessageClient.Record();
                            biblio.RecPath = record.Path;
                            biblio.Data = record.RecordBody.Xml;
                            records.Add(biblio);
#endif
                            DigitalPlatform.MessageClient.Record biblio = FillBiblio(record);
                            records.Add(biblio);
                        }

#if NO
                        ResponseSearch(
                            searchParam.TaskID,
                            lHitCount,
                            lStart,
                            records,
                            "",
                            strErrorCode);
#endif
                        bool bRet = TryResponseSearch(
searchParam.TaskID,
lHitCount,
lStart,
records,
"",
strErrorCode,
ref batch_size);
                        // Console.WriteLine("ResponseSearch called " + records.Count.ToString() + ", bRet=" + bRet);
                        if (bRet == false)
                            return;

                        lStart += searchresults.Length;

                        if (lPerCount != -1)
                            lPerCount -= searchresults.Length;

                        if (lStart >= lHitCount || (lPerCount <= 0 && lPerCount != -1))
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLine("SearchAndResponse() 出现异常: " + ex.Message);
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }

            this.AddInfoLine("search and response end");
            return;
        ERROR1:
            // 报错
            ResponseSearch(
searchParam.TaskID,
-1,
0,
records,
strError,
strErrorCode);
        }

        static DigitalPlatform.MessageClient.Record FillBiblio(DigitalPlatform.LibraryClient.localhost.Record record)
        {
            DigitalPlatform.MessageClient.Record biblio = new DigitalPlatform.MessageClient.Record();
            biblio.RecPath = record.Path;

            if (record.RecordBody != null
                && record.RecordBody.Result != null
                && record.RecordBody.Result.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.NotFound)
                return biblio;  // 记录不存在

            // biblio 中里面应该有表示错误码的成员就好了。Result.ErrorInfo 提供了错误信息

            XmlDocument dom = new XmlDocument();
            if (record.RecordBody != null
                && string.IsNullOrEmpty(record.RecordBody.Xml) == false)
            {
                // xml
                dom.LoadXml(record.RecordBody.Xml);
            }
            else
            {
                dom.LoadXml("<root />");
            }

            if (record.Cols != null)
            {
                // cols
                foreach (string s in record.Cols)
                {
                    XmlElement col = dom.CreateElement("col");
                    dom.DocumentElement.AppendChild(col);
                    col.InnerText = s;
                }
            }

            biblio.Format = "";
            if (record.RecordBody != null)
            {
                // metadata
                if (string.IsNullOrEmpty(record.RecordBody.Metadata) == false)
                {
                    // 在根元素下放一个 metadata 元素
                    XmlElement metadata = dom.CreateElement("metadata");
                    dom.DocumentElement.AppendChild(metadata);

                    try
                    {
                        XmlDocument metadata_dom = new XmlDocument();
                        metadata_dom.LoadXml(record.RecordBody.Metadata);

                        foreach (XmlAttribute attr in metadata_dom.DocumentElement.Attributes)
                        {
                            metadata.SetAttribute(attr.Name, attr.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        metadata.SetAttribute("error", "metadata XML '" + record.RecordBody.Metadata + "' 装入 DOM 时出错: " + ex.Message);
                    }
                }
                // timestamp
                biblio.Timestamp = ByteArray.GetHexTimeStampString(record.RecordBody.Timestamp);
            }

            biblio.Data = dom.DocumentElement.OuterXml;
            return biblio;
        }

#if NO
        // 写入实体库
        void SetEntity(SetInfoRequest request)
        {
            string strError = "";
            IList<Entity> entities = new List<Entity>();

            LibraryChannel channel = GetChannel();
            try
            {
                List<EntityInfo> input_items = new List<EntityInfo>();
                foreach (Entity entity in request.Entities)
                {
                    EntityInfo info = new EntityInfo();
                    SetValue(entity, info);
                    input_items.Add(info);
                }

                EntityInfo[] output_items = null;

                long lRet = channel.SetEntities(null,
                    request.BiblioRecPath,
                    input_items.ToArray(),
                    out output_items,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (output_items != null)
                {
                    foreach (EntityInfo info in output_items)
                    {
                        Entity entity = new Entity();
                        SetValue(info, entity);
                        entities.Add(entity);
                    }
                }

                ResponseSetInfo(
                    request.TaskID,
                    lRet,
                    entities,
                    "");
            }
            catch (Exception ex)
            {
                AddErrorLine("SetEntity() 出现异常: " + ex.Message);
                strError = "SetEntity() 异常：" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this._channelPool.ReturnChannel(channel);
            }
            return;
        ERROR1:
            // 报错
            ResponseSetInfo(
request.TaskID,
-1,
entities,
strError);
        }

        void SetInfoAndResponse(SetInfoRequest request)
        {
            if (request.Operation == "setEntity")
            {
                SetEntity(request);
                return;
            }
        }

#endif

        public override void OnSearchResponseRecieved(string searchID,
            long resultCount,
            long start,
            IList<DigitalPlatform.MessageClient.Record> records,
            string errorInfo,
            string errorCode)
        {
#if NO
            int i = 0;
            foreach (BiblioRecord record in records)
            {
                AddInfoLine((i + 1).ToString());
                AddInfoLine("data: " + record.Data);
                i++;
            }
#endif
            if (SearchResponseEvent != null)
            {
                SearchResponseEventArgs e = new SearchResponseEventArgs();
                e.TaskID = searchID;
                e.ResultCount = resultCount;
                e.Start = start;
                e.Records = records;
                e.ErrorInfo = errorInfo;
                e.ErrorCode = errorCode;
                this.SearchResponseEvent(this, e);
            }
        }

        public bool ShareBiblio
        {
            get
            {
                // 共享书目数据
                if (this.MainForm == null || this.MainForm.AppInfo == null)
                    return false;
                return this.MainForm.AppInfo.GetBoolean(
                    "message",
                    "share_biblio",
                    false);
            }
            set
            {
                if (this.MainForm == null || this.MainForm.AppInfo == null)
                    return;
                bool bOldValue = this.MainForm.AppInfo.GetBoolean(
                    "message",
                    "share_biblio",
                    false);
                if (bOldValue != value)
                {
                    this.MainForm.AppInfo.SetBoolean(
                        "message",
                        "share_biblio",
                        value);
                    if (this.IsConnected)
                    {
                        // this.Login();    // 重新登录
                        this.CloseConnection();

                        this.RefreshUserName();
                        this.Connect();
                    }
                }
            }
        }

#if NO
        public override void Login()
        {
            LoginRequest param = new LoginRequest();
            param.UserName = this.MainForm.MessageUserName;
            param.Password = this.MainForm.MessagePassword;
            param.LibraryUID = this.MainForm.ServerUID;    // 测试用 Guid.NewGuid().ToString(),
            param.LibraryName = this.MainForm.LibraryName;
            param.PropertyList = (this.ShareBiblio ? "biblio_search" : "");
            param.LibraryUserName = this.MainForm.GetCurrentUserName();
            Login(param);
        }
#endif

#if NO
        public override void WaitTaskComplete(Task task)
        {
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }
        }
#endif
    }


    /// <summary>
    /// 检索响应事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void SearchResponseEventHandler(object sender,
    SearchResponseEventArgs e);

    /// <summary>
    /// 检索响应事件的参数
    /// </summary>
    public class SearchResponseEventArgs : EventArgs
    {
        public string TaskID = "";   // 检索请求的 ID
        public long ResultCount = 0;    // 整个结果集中记录个数
        public long Start = 0;  // Records 从整个结果集的何处开始
        public IList<DigitalPlatform.MessageClient.Record> Records = null;  // 命中的书目记录集合
        public string ErrorInfo = "";   // 错误信息
        public string ErrorCode = "";   // 错误代码。2016/4/15 增加
    }
}
