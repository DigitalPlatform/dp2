using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms;
using System.Xml;

using AmazonProductAdvtApi;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using System.Diagnostics;
using dp2Secure;

namespace DigitalPlatform.AmazonInterface
{
    /// <summary>
    /// 检索亚马逊服务器
    /// </summary>
    public class AmazonSearch : IDisposable
    {
        public int SleepTime = 2000;

        /// <summary>
        /// 最近一次访问服务器的时间
        /// </summary>
        public DateTime LastTime = DateTime.Now;
        /// <summary>
        /// 等待的间隙
        /// </summary>
        public event EventHandler Idle = null;

        // public MainForm MainForm = null;
        public string TempFileDir = "";

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // 删除临时文件
                DeleteTempFile();

            }
            disposed = true;
        }

        ~AmazonSearch()
        {
            Dispose(false);
        }

        // 构造检索式
        int BuildQueryString(
            string strWord,
            string strFrom,
            string strMatch,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            if (string.IsNullOrEmpty(strWord) == true)
            {
                strError = "检索词不能为空";
                return -1;
            }

            // TODO: 检查 strFrom 和 strMatch 的合法性

            if (strMatch == "[default]")
                strMatch = "";

            strText = strFrom + strMatch;
            if (strText.IndexOf(":") == -1)
                strText += ": ";

            strText += " " + strWord;
            return 0;
        }

        // private const string DESTINATION = "webservices.amazon.cn";  // "ecs.amazonaws.com";

        // 准备用于检索的 URL
        int GetOneLineSearchRequestUrl(
            string strServerUrl,
            string strWord,
            string strFrom,
            string strMatch,
            bool bUseFullElementSet,
            out string strUrl,
            out string strError)
        {
            strError = "";
            strUrl = "";

            if (string.IsNullOrEmpty(strWord) == true)
            {
                strError = "strWord 参数值不能为空";
                return -1;
            }

            string strText = "";
            // 构造检索式
            int nRet = BuildQueryString(
            strWord,
            strFrom,
            strMatch,
            out strText,
            out strError);
            if (nRet == -1)
                return -1;

            AmazonSignedRequestHelper helper = new AmazonSignedRequestHelper(
//MY_AWS_ACCESS_KEY_ID,
//MY_AWS_SECRET_KEY,
strServerUrl);

            IDictionary<string, string> parameters = new Dictionary<string, String>();

            parameters["Service"] = "AWSECommerceService";
            parameters["Version"] = "2011-08-01";
            parameters["Operation"] = "ItemSearch";
            parameters["SearchIndex"] = "Books";
            parameters["Power"] = strText;

            if (bUseFullElementSet == true)
                parameters["ResponseGroup"] = "Large";
            else
                parameters["ResponseGroup"] = "Small";

#if TTT
            parameters["AssociateTag"] = ASSOCIATEKEY;
#endif

            m_searchParameters = parameters;
            // m_strCurrentSearchedServer = this.CurrentServer;

            strUrl = helper.Sign(parameters);
            return 0;
        }

        // 准备用于获得下一批浏览记录的请求的 URL
        int GetNextRequestUrl(
            string strServerUrl,
            out string strUrl,
            out string strError)
        {
            strError = "";
            strUrl = "";

            AmazonSignedRequestHelper helper = new AmazonSignedRequestHelper(
//MY_AWS_ACCESS_KEY_ID,
//MY_AWS_SECRET_KEY,
strServerUrl);

            if (this.m_nCurrentPageNo == -1)
            {
                strError = "m_nCurrentPageNo 尚未初始化";
                return -1;
            }

            // ItemPage URL 参数的值是从 1 开始计算的
            m_searchParameters["ItemPage"] = (this.m_nCurrentPageNo + 1 + 1).ToString();

            strUrl = helper.Sign(m_searchParameters);
            return 0;
        }

        public const string NAMESPACE = "http://webservices.amazon.com/AWSECommerceService/2011-08-01";

        WebClient webClient = null;
        public string TempFilename = "";


        // 准备临时文件名
        void PrepareTempFile()
        {
            // 如果以前有临时文件名，就直接沿用
            if (string.IsNullOrEmpty(this.TempFilename) == true)
            {
                // this.TempFilename = Path.Combine(this.MainForm.DataDir, "~webclient_response_" + Guid.NewGuid().ToString());
                Debug.Assert(string.IsNullOrEmpty(this.TempFileDir) == false, "");
                this.TempFilename = Path.Combine(this.TempFileDir, "~webclient_response_" + Guid.NewGuid().ToString());
            }

            try
            {
            }
            catch
            {
            }
                File.Delete(this.TempFilename);
        }

        void DeleteTempFile()
        {
            // 删除临时文件
            if (string.IsNullOrEmpty(this.TempFilename) == false)
            {
                try
                {
                }
                catch
                {
                }
                    File.Delete(this.TempFilename);
                this.TempFilename = "";
            }
        }

        AutoResetEvent eventComplete = new AutoResetEvent(false);
        bool m_bError = false;   // 最近一次异步操作是否因报错结束
        Exception m_exception = null;

        public Exception Exception
        {
            get
            {
                return this.m_exception;
            }
        }

        string m_strError = ""; // 异步操作中用于保存出错信息

        bool _userFullElementSet = true;

        // 多行检索中的一行检索
        // return:
        //      -1  出错
        //      0   成功
        public int Search(
            string strServerUrl,
            string strWord,
            string strFrom,
            string strMatch,
            bool bUseFullElementSet,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            _userFullElementSet = bUseFullElementSet;

            string strUrl = "";
            nRet = GetOneLineSearchRequestUrl(
                strServerUrl,
                strWord,
                strFrom,
                strMatch,
                bUseFullElementSet,
                out strUrl,
                out strError);
            if (nRet == -1)
                return -1;

            // this.m_multiSearchInfo.CurrentWord = strLine;

            // m_reloadInfo = null;

            this.PrepareTempFile();

            this.m_bError = false;
            this.m_exception = null;

#if NO
            if (this.webClient != null)
            {
                webClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(webClient_MultiLineDownloadFileCompleted);
                this.webClient.Dispose();
                this.webClient = null;
            }
#endif
            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_MultiLineDownloadFileCompleted);
            // webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            try
            {
                Delay();

                this.LastTime = DateTime.Now;
                webClient.DownloadFileAsync(new Uri(strUrl, UriKind.Absolute),
                    this.TempFilename, null);
            }
            catch (Exception ex)
            {
                this.m_exception = ex;
                strError = ex.Message;
                this.m_bError = true;
                return -1;
            }

            // 等待检索结束
            bool bError = WaitSearchFinish();
            if (bError == true)
            {
                strError = this.m_strError;
                return -1;
            }

            {
                AsyncCompletedEventArgs e = _end_e;

                if (e == null || e.Cancelled == true)
                {
                    strError = "请求被取消";
                    return -1;
                }

                if (e != null && e.Error != null)
                {
                    strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                    this.m_exception = e.Error;
                    return -1;
                }
            }

            // 如果要求每行的检索命中装入大于 10 条，需要在这里获取后面几批的浏览结果

            return 0;
        }

        // 休眠一段时间
        public void Delay()
        {
            TimeSpan delta = new TimeSpan(0, 0, 0, 0, SleepTime);
            TimeSpan delta1 = DateTime.Now - this.LastTime;
            if (this.Idle == null)
            {
                if (delta1 < delta)
                {
                    Thread.Sleep((int)((delta - delta1).TotalMilliseconds));
                }
            }
            else
            {
                while (DateTime.Now - this.LastTime < delta)
                {
                    if (this.Idle != null)
                    {
                        this.Idle(this, new EventArgs());
                    } 
                    Thread.Sleep(10);
                }
            }
        }

        public bool HasNextBatch()
        {
            if (m_nTotalPages == 0)
                return false;
            if (m_nCurrentPageNo < m_nTotalPages - 1)
                return true;
            return false;
        }

        public int HitCount
        {
            get
            {
                return (int)this.m_nTotalResults;
            }
        }

        // 获得后一批结果
        public int NextBatch(
    string strServerUrl,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            string strUrl = "";
            nRet = GetNextRequestUrl(
                strServerUrl,
                out strUrl,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(string.IsNullOrEmpty(strUrl) == false, "");

            this.PrepareTempFile();

            this.m_bError = false;
            this.m_exception = null;

            webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_MultiLineDownloadFileCompleted);
            // webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            try
            {
                Delay();

                this.LastTime = DateTime.Now;
                webClient.DownloadFileAsync(new Uri(strUrl, UriKind.Absolute),
                    this.TempFilename, null);
            }
            catch (Exception ex)
            {
                this.m_exception = ex;
                strError = ex.Message;
                this.m_bError = true;
                return -1;
            }

            // 等待检索结束
            bool bError = WaitSearchFinish();
            if (bError == true)
            {
                strError = this.m_strError;
                return -1;
            }

            {
                AsyncCompletedEventArgs e = _end_e;

                if (e == null || e.Cancelled == true)
                {
                    strError = "请求被取消";
                    return -1;
                }

                if (e != null && e.Error != null)
                {
                    strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                    this.m_exception = e.Error;
                    return -1;
                }
            }


            return 0;
        }

        /// <summary>
        /// 超时值。单位为毫秒
        /// </summary>
        public int Timeout = 5000;

        // TODO: 加入一旦窗口关闭就跳出循环的逻辑
        // return:
        //      true    已经报错
        //      false   正常
        bool WaitSearchFinish()
        {
            int nTimeCount = 0; // 如果为 -1 表示已经 Cancel
            while (true)
            {
                if (this.Idle != null)
                {
                    this.Idle(this, new EventArgs());
                }

                if (nTimeCount > 0 && nTimeCount > this.Timeout)
                {
                    webClient.CancelAsync();
                    nTimeCount = -1;
                    this.m_strError = "超时";
                    this.m_bError = true;
                    break;  // 2015/5/22
                }
                // Application.DoEvents();
                if (eventComplete.WaitOne(100) == true)
                    break;
                if (nTimeCount >= 0)
                    nTimeCount += 100;
            }

            return this.m_bError;
        }

        // 终止检索
        public void CancelSearch()
        {
            if (webClient != null)
                webClient.CancelAsync();
        }

        AsyncCompletedEventArgs _end_e = null;

        void webClient_MultiLineDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
#if NO
            Delegate_EndSearch d = new Delegate_EndSearch(EndMultiLineSearch);
            object[] args = new object[] { e };
            this.MainForm.Invoke(d, args);
#endif
            _end_e = e;

            if (this.eventComplete != null)
                this.eventComplete.Set();
        }

        // 重新装载浏览记录的任务信息
        class ReloadTaskInfo
        {
            public string ElementSet = "";
            public List<ListViewItem> TotalItems = null;
            public List<ListViewItem> CurrentItems = null;
            public int StartIndex = 0;  // 开始的偏移
            public bool Cancel = false; // 是否中途放弃，或者因为出错而需要中断
        }
        ReloadTaskInfo m_reloadInfo = null;
        long m_nTotalResults = 0;    // 当前检索命中的结果数
        long m_nTotalPages = 0;     // 命中结果的总页数
        int m_nCurrentPageNo = -1;  // 当前已经装入的最后一个页号
        IDictionary<string, string> m_searchParameters = null;  // 当前检索参数

        void ClearResultSetParameters()
        {
            this.m_nCurrentPageNo = -1; // 表示这是第一次检索
            this.m_nTotalResults = 0;
            this.m_nTotalPages = 0;
            this.m_searchParameters = null;
            //this.m_strCurrentSearchedServer = "";
        }

        // return:
        //      -1  出错
        //      0   正常
        //      1   希望停止
        public delegate int Delegate_appendBrowseLine(string strRecPath,
            string strRecord,
            object param,
            bool bAutoSetFocus,
            out string strError);

        // return:
        //      -1  出错
        //      >=0 命中的记录数
        public int LoadBrowseLines(Delegate_appendBrowseLine appendBrowseLineProc,
            object param,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";

            XmlDocument doc = new XmlDocument();
            doc.Load(this.TempFilename);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", NAMESPACE);

            XmlNodeList errors = doc.DocumentElement.SelectNodes("amazon:Items/amazon:Request/amazon:Errors/amazon:Error", nsmgr);
            if (errors.Count > 0)
            {
                string strCode = DomUtil.GetElementText(errors[0], "amazon:Code", nsmgr);
                string strMessage = DomUtil.GetElementText(errors[0], "amazon:Message", nsmgr);
                if (strCode == "AWS.ECommerceService.NoExactMatches")
                {
                    strError = strCode;
                    goto ERROR1;
                }
                strError = strMessage;
                goto ERROR1;
            }

            int nHitCount = 0;

            if (this.m_reloadInfo == null)
            {
                // Items/TotalResults
                string strTotalResults = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalResults", nsmgr);
                Int64.TryParse(strTotalResults, out this.m_nTotalResults);

                // Items/TotalPages
                string strTotalPages = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalPages", nsmgr);
                Int64.TryParse(strTotalPages, out this.m_nTotalPages);

                nHitCount = (int)Math.Min(m_nTotalResults, 10);

#if NO
                // TODO: 显示单行命中的消息
                if (m_nTotalResults > 0)
                {
                    int nHitCount = (int)Math.Min(m_nTotalResults, 10);
                    this.m_multiSearchInfo.HitWords.Add(this.m_multiSearchInfo.CurrentWord + "\t" + nHitCount.ToString());
                    this.m_multiSearchInfo.HitCount += nHitCount;
                }
                else
                    this.m_multiSearchInfo.NotHitWords.Add(this.m_multiSearchInfo.CurrentWord);
#endif

                // this.amazonSimpleQueryControl_simple.Comment = "命中记录:\t" + m_nTotalResults.ToString();
            }

            int nRet = LoadResults(doc,
                // _userFullElementSet,
                appendBrowseLineProc,
                param,
                bAutoSetFocus,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.m_reloadInfo == null)
            {
                this.m_nCurrentPageNo++;    // 第一次 从 -1 ++ 正好等于 0
            }
            return nHitCount;
        ERROR1:
            if (this.m_reloadInfo != null)
                this.m_reloadInfo.Cancel = true;

            this.DeleteTempFile();

            if (strError == "AWS.ECommerceService.NoExactMatches")
            {
                // TODO: 显示单行没有命中的消息
                ////this.m_multiSearchInfo.NotHitWords.Add(this.m_multiSearchInfo.CurrentWord);
                return 0;
            }
            // TODO: 显示单行检索出错的消息
            // this.amazonSimpleQueryControl_simple.Comment = "检索发生错误:\r\n" + strError;
            this.m_bError = true;
            this.m_strError = strError;
            return -1;
        }

#if NO
        delegate void Delegate_EndSearch(AsyncCompletedEventArgs e);
        void EndMultiLineSearch(AsyncCompletedEventArgs e)
        {
            string strError = "";
            if (e == null || e.Cancelled == true)
            {
                strError = "请求被取消";
                goto ERROR1;
            }

            if (e != null && e.Error != null)
            {
                strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                this.m_exception = e.Error; // 2013/4/25
                goto ERROR1;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(this.TempFilename);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", NAMESPACE);

            XmlNodeList errors = doc.DocumentElement.SelectNodes("amazon:Items/amazon:Request/amazon:Errors/amazon:Error", nsmgr);
            if (errors.Count > 0)
            {
                string strCode = DomUtil.GetElementText(errors[0], "amazon:Code", nsmgr);
                string strMessage = DomUtil.GetElementText(errors[0], "amazon:Message", nsmgr);
                if (strCode == "AWS.ECommerceService.NoExactMatches")
                {
                    strError = strCode;
                    goto ERROR1;
                }
                strError = strMessage;
                goto ERROR1;
            }

            if (this.m_reloadInfo == null)
            {
                // Items/TotalResults
                string strTotalResults = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalResults", nsmgr);
                Int64.TryParse(strTotalResults, out this.m_nTotalResults);

                // Items/TotalPages
                string strTotalPages = DomUtil.GetElementText(doc.DocumentElement,
                    "amazon:Items/amazon:TotalPages", nsmgr);
                Int64.TryParse(strTotalPages, out this.m_nTotalPages);

#if NO
                // TODO: 显示单行命中的消息
                if (m_nTotalResults > 0)
                {
                    int nHitCount = (int)Math.Min(m_nTotalResults, 10);
                    this.m_multiSearchInfo.HitWords.Add(this.m_multiSearchInfo.CurrentWord + "\t" + nHitCount.ToString());
                    this.m_multiSearchInfo.HitCount += nHitCount;
                }
                else
                    this.m_multiSearchInfo.NotHitWords.Add(this.m_multiSearchInfo.CurrentWord);
#endif

                // this.amazonSimpleQueryControl_simple.Comment = "命中记录:\t" + m_nTotalResults.ToString();
            }

            int nRet = LoadResults(doc, _userFullElementSet, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.m_reloadInfo == null)
            {
                this.m_nCurrentPageNo++;    // 第一次 从 -1 ++ 正好等于 0
            }
            return;
        ERROR1:
            if (this.m_reloadInfo != null)
                this.m_reloadInfo.Cancel = true;

            this.DeleteTempFile();

            if (strError == "AWS.ECommerceService.NoExactMatches")
            {
                // TODO: 显示单行没有命中的消息
                ////this.m_multiSearchInfo.NotHitWords.Add(this.m_multiSearchInfo.CurrentWord);
                return;
            }
            // TODO: 显示单行检索出错的消息
            // this.amazonSimpleQueryControl_simple.Comment = "检索发生错误:\r\n" + strError;
            this.m_bError = true;
            this.m_strError = strError;
        }
#endif

        // 装入检索命中记录到浏览列表中
        int LoadResults(XmlDocument response_dom,
            // bool bDisplayInfo,
            Delegate_appendBrowseLine appendBrowseLineProc,
            object param,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // string strElementSet = "";

            // 建立 ASIN --> ListViewItem 对照表
            Hashtable table = new Hashtable();
#if NO
            if (this.m_reloadInfo != null)
            {
                foreach (ListViewItem item in this.m_reloadInfo.CurrentItems)
                {
                    table[item.Text] = item;
                }
                strElementSet = this.m_reloadInfo.ElementSet;
            }
            else
            {
                if (bUseFullElementSet == true)
                    strElementSet = "F";
                else
                    strElementSet = "B";
            }
#endif

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", NAMESPACE);

            XmlNodeList items = response_dom.DocumentElement.SelectNodes("amazon:Items/amazon:Item", nsmgr);

            foreach (XmlNode item in items)
            {
                var element = item as XmlElement;

                List<string> cols = null;
                string strASIN = "";
                string strCoverImageUrl = "";
                nRet = ParseItemXml(element,
                    nsmgr,
                    out strASIN,
                    out strCoverImageUrl,
                    out cols,
                    out strError);
                if (nRet == -1)
                    return -1;
                nRet = appendBrowseLineProc("", element.OuterXml, param, bAutoSetFocus, out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    break;

#if NO
                if (info.EelementSet == "B")
                    listitem.ImageIndex = BROWSE_TYPE_BRIEF;
                else
                    listitem.ImageIndex = BROWSE_TYPE_FULL;
#endif
            }

#if NO
            if (this.m_reloadInfo == null)
            {
                if (bDisplayInfo == true)
                {
                    this.amazonSimpleQueryControl_simple.Comment = "命中记录:\t" + m_nTotalResults.ToString()
                    + "\r\n已装入浏览记录:\t" + this.listView_browse.Items.Count.ToString();
                }
            }
            else
            {
                if (table.Count != 0)
                {
                    strError = "下列事项没有被重新装载 '" + Join(table.Keys) + "'";
                    return -1;
                }
            }
#endif

            return 0;
        }

        static string Join(ICollection list)
        {
            StringBuilder text = new StringBuilder(4096);
            foreach (string s in list)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(s);
            }

            return text.ToString();
        }

        static string[] imagesize_names = new string[] {
            "SmallImage",
            "MediumImage",
            "LargeImage"};

        public static int GetImageUrl(XmlElement root,
    XmlNamespaceManager nsmgr,
    out Hashtable table,
    out string strError)
        {
            strError = "";
            table = new Hashtable();

            foreach (string s in imagesize_names)
            {
                List<ImageInfo> images = GetImageValues(root,
    nsmgr,
    "amazon:" + s);
                if (images.Count > 0)
                    table[s] = images[0];
            }
            return 0;
        }

        // 解析 <Item> 内的基本信息
        // parameters:
        //      strASIN
        //      cols    浏览列信息
        public static int ParseItemXml(XmlElement root,
            XmlNamespaceManager nsmgr,
            out string strASIN,
            out string strCoverImageUrl,
            out List<string> cols,
            out string strError)
        {
            strError = "";
            strASIN = "";
            strCoverImageUrl = "";
            cols = new List<string>();

            strASIN = DomUtil.GetElementText(root, "amazon:ASIN", nsmgr);

#if NO
            // ISBN
            cols.Add(
                GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ISBN | amazon:ItemAttributes/amazon:ISSN",
                " ; ")
                );
#endif

            // title
            cols.Add(
                GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:Title",
                " ; ")
                );

            // author
            cols.Add(
    GetFieldValues(root,
    nsmgr,
    "amazon:ItemAttributes/amazon:Creator",
    " ; ")
    );
            // publisher
            cols.Add(
GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:Manufacturer",   // Publisher 在 Small 的时候没有提供
" ; ")
);
            // publish date
            cols.Add(
GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:PublicationDate", // PublicationDate 在 Small 的时候没有提供
" ; ")
);

#if NO
            // EAN
            cols.Add(
                GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:EAN",
                " ; ")
                );
#endif
            List<ImageInfo> images = GetImageValues(root,
nsmgr,
"amazon:MediumImage");
            if (images.Count > 0)
                strCoverImageUrl = images[0].Url;
            return 0;
        }

        // 获得一种字段的值。如果字段多次出现，用 strSep 符号连接
        static string GetFieldValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath,
            string strSep)
        {
            StringBuilder text = new StringBuilder(4096);
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlNode node in nodes)
            {
                if (text.Length > 0)
                    text.Append(strSep);
                text.Append(node.InnerText);
            }

            return text.ToString();
        }

        // 获得一种字段的值
        static List<string> GetFieldValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlNode node in nodes)
            {
                results.Add(node.InnerText.Trim());
            }

            return results;
        }

        // 获得价格字段的值
        static List<string> GetPriceValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlElement node in nodes)
            {
                results.Add(GetPriceValue(node, nsmgr));
            }

            return results;
        }

        static string GetPriceValue(XmlElement element,
            XmlNamespaceManager nsmgr)
        {
            XmlElement amount = element.SelectSingleNode("amazon:Amount", nsmgr) as XmlElement;
            XmlElement code = element.SelectSingleNode("amazon:CurrencyCode", nsmgr) as XmlElement;

            string strText = "";
            if (code != null)
                strText += code.InnerText.Trim();
            if (amount != null)
            {
                string strAmount = amount.InnerText.Trim();
                if (string.IsNullOrEmpty(strAmount) == false)
                {
                    long v = 0;
                    if (long.TryParse(strAmount, out v) == false)
                    {
                        strText += " 数字 '" + strAmount + "' 格式错误";
                    }
                    else
                    {
                        strAmount = (((decimal)v) / 100).ToString();
                        strText += strAmount;
                    }
                }
            }
            return strText;
        }

        class Creator
        {
            public string Name = "";
            public string Role = "";
        }

        // 获得创建者字段的值
        static List<Creator> GetCreatorValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath)
        {
            List<Creator> results = new List<Creator>();
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlElement node in nodes)
            {
                results.Add(GetCreatorValue(node, nsmgr));
            }

            return results;
        }

        static Creator GetCreatorValue(XmlElement element,
    XmlNamespaceManager nsmgr)
        {
            string strRole = element.GetAttribute("Role");

            if (strRole == "作者")
                strRole = "著";
            if (strRole == "译者")
                strRole = "译";

            Creator creator = new Creator();
            creator.Name = element.InnerText.Trim();
            creator.Role = strRole;

            return  creator;
        }

        // 获得尺寸字段的值
        static List<string> GetHeightValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlElement node in nodes)
            {
                results.Add(GetHeightValue(node, nsmgr));
            }

            return results;
        }

        static string GetHeightValue(XmlElement element,
            XmlNamespaceManager nsmgr)
        {
            XmlElement height = element.SelectSingleNode("amazon:Length", nsmgr) as XmlElement;

            if (height != null)
            {
                string strUnits = height.GetAttribute("Units");
                string strValue = height.InnerText.Trim();

                if (string.IsNullOrEmpty(strValue) == false)
                {
                    double v = 0;
                    if (double.TryParse(strValue, out v) == false)
                    {
                        return " 数字 '" + strValue + "' 格式错误";
                    }

                    if (strUnits == "inches")
                        return Math.Ceiling(((double)v) * (double)2.54).ToString() + "cm";

                    return strValue + strUnits;
                }
            }

            return "";
        }

        public class ImageInfo
        {
            public string Url = "";
            public string Size = "";
        }

        // 获得图像字段的值
        static List<ImageInfo> GetImageValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath)
        {
            List<ImageInfo> results = new List<ImageInfo>();
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlElement node in nodes)
            {
                ImageInfo info = GetImageValue(node, nsmgr);
                if (info != null)
                    results.Add(info);
            }

            return results;
        }

        static string GetHeight(XmlElement element,
            XmlNamespaceManager nsmgr)
        {
            string strUnits = element.GetAttribute("Units");
            string strValue = element.InnerText.Trim();

            if (strUnits == "pixels")
                strUnits = "px";

            return strValue + strUnits;
        }

        static ImageInfo GetImageValue(XmlElement element,
            XmlNamespaceManager nsmgr)
        {
            XmlElement url = element.SelectSingleNode("amazon:URL", nsmgr) as XmlElement;
            XmlElement height = element.SelectSingleNode("amazon:Height", nsmgr) as XmlElement;
            XmlElement width = element.SelectSingleNode("amazon:Width", nsmgr) as XmlElement;

            ImageInfo info = new ImageInfo();

            if (url != null)
                info.Url = url.InnerText.Trim();

            // 2014/12/14
            // <URL>http://g-ec4.images-amazon.com/images/G/28/x-site/icons/no-img-sm._V192562228_._SL75_.gif</URL> 
            if (info.Url.IndexOf("/no-img-") != -1)
                return null;

            if (height != null && width != null)
            {
                string strHeight = GetHeight(height, nsmgr);
                string strWidth = GetHeight(width, nsmgr);

                string strError = "";
                string strHeightValue = "";
                string strHeightUnit = "";
                int nRet = StringUtil.ParseUnit(strHeight,
    out strHeightValue,
    out strHeightUnit,
    out strError);
                if (nRet == -1)
                    strHeightValue = strHeight;

                string strWidthValue = "";
                string strWidthUnit = "";
                nRet = StringUtil.ParseUnit(strWidth,
    out strWidthValue,
    out strWidthUnit,
    out strError);
                if (nRet == -1)
                    strWidthValue = strWidth;

                if (strHeightUnit == strWidthUnit)
                    info.Size = strWidthValue + "X" + strHeightValue + strHeightUnit;    // 省略法
                else
                    info.Size = strWidth + "X" + strHeight;
            }
            return info;
        }

        /*
    <EditorialReviews>
        <EditorialReview>
            <Source>内容简介</Source>
            <Content>  这本由佛瑞斯特·卡特著的《少年小树之歌》讲述了：差不多在印第安紫罗兰开花以后，你会讶异地发现，拂上脸庞的风竟然如羽毛般温柔暖和，而且还带着一些泥土的气息，春天的脚步正渐渐地接近。&lt;br&gt;    接着闪电又来了，它击中了山顶的岩石，巨大的蓝色火球四处进落，照得整个天空一片闪动着的蓝。豆大的雨点稀落地从云端洒下，仿佛预告着接下来大得惊人的雨即将降临。&lt;br&gt;    夏天则是生命力量最澎湃的时刻，就像人在壮年一般。而当萧瑟的秋风吹起，我们觉得自己开始苍老，一股归乡的思绪充塞心头，有些人称这感觉为乡愁。万物销声匿迹的冬天就像我们躯体的死亡，但是当春天来临时，它们又会重生。</Content>
            <IsLinkSuppressed>0</IsLinkSuppressed>
        </EditorialReview>
    </EditorialReviews>
         * */
        // 获得价格字段的值
        // parameters:
        //      strXPath    EditorialReviews/EditorialReview
        //      strSourceType   "内容简介"
        static List<string> GetCommentValues(XmlNode root,
            XmlNamespaceManager nsmgr,
            string strXPath,
            string strSourceType)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = root.SelectNodes(strXPath, nsmgr);
            foreach (XmlElement node in nodes)
            {
                string strText = GetCommentValue(node, nsmgr, strSourceType);
                if (string.IsNullOrEmpty(strText) == false)
                    results.Add(strText);
            }

            return results;
        }

        static string GetCommentValue(XmlElement element,
            XmlNamespaceManager nsmgr,
            string strSourceType)
        {
            XmlElement source = element.SelectSingleNode("amazon:Source", nsmgr) as XmlElement;
            XmlElement content = element.SelectSingleNode("amazon:Content", nsmgr) as XmlElement;

            if (source == null)
                return "";
            if (source != null && source.InnerText.Trim() != strSourceType)
                return "";
            return content.InnerText.Trim();
        }


        // 将亚马逊 XML 格式转换为 UNIMARC 格式
        public static int AmazonXmlToUNIMARC(XmlNode root,
            out string strMARC,
            out string strError)
        {
            strMARC = "";
            strError = "";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", AmazonSearch.NAMESPACE);

            MarcRecord record = new MarcRecord();

            // *** 010
            // ISBN
            List<string> isbns = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ISBN");
            if (isbns.Count == 0)
                isbns = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:EAN");

            // 价格
            List<string> prices = GetPriceValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ListPrice");

            for (int i = 0; i < Math.Max(isbns.Count, prices.Count); i++)
            {
                string isbn = "";
                string price = "";

                if (i < isbns.Count)
                    isbn = isbns[i];
                if (i < prices.Count)
                    price = prices[i];
                MarcField field = new MarcField("010", "  ");
                record.ChildNodes.add(field);

                if (string.IsNullOrEmpty(isbn) == false)
                    field.ChildNodes.add(new MarcSubfield("a", isbn));
                if (string.IsNullOrEmpty(price) == false)
                    field.ChildNodes.add(new MarcSubfield("d", price));
            }

            // 011
            List<string> issns = GetFieldValues(root,
            nsmgr,
            "amazon:ItemAttributes/amazon:ISSN");

            // 200
            List<string> titles = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:Title");
            List<Creator> creators = GetCreatorValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:Creator");
            {
                MarcField field = new MarcField("200", "1 ");
                record.ChildNodes.add(field);

                foreach (string title in titles)
                {
                    field.ChildNodes.add(new MarcSubfield("a", title));
                }
                int i = 0;
                foreach (Creator creator in creators)
                {
                    field.ChildNodes.add(new MarcSubfield(i == 0 ? "f" : "g", creator.Name + creator.Role));
                    i++;
                }
            }

            // 2015/7/19
            // 205
            List<string> editions = GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:Edition");
            foreach (string edition in editions)
            {
                MarcField field = new MarcField("205", "  ");
                record.ChildNodes.add(field);

                field.ChildNodes.add(new MarcSubfield("a", edition));
            }

            // 210
            List<string> publishers = GetFieldValues(root,
    nsmgr,
    "amazon:ItemAttributes/amazon:Publisher");
            List<string> publication_dates = GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:PublicationDate");
            {
                MarcField field = new MarcField("210", "  ");
                record.ChildNodes.add(field);

                foreach (string s in publishers)
                {
                    field.ChildNodes.add(new MarcSubfield("c", s));
                }
                foreach (string s in publication_dates)
                {
                    // 日期字符串需要变换一下
                    field.ChildNodes.add(new MarcSubfield("d", GetPublishDateString(s)));
                }
            }

            // 215 a d
            List<string> pages = GetFieldValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:NumberOfPages");
            List<string> heights = GetHeightValues(root,
nsmgr,
"amazon:ItemAttributes/amazon:PackageDimensions");
            {
                MarcField field = new MarcField("215", "  ");
                record.ChildNodes.add(field);

                foreach (string s in pages)
                {
                    field.ChildNodes.add(new MarcSubfield("a", s + "页"));
                }
                foreach (string s in heights)
                {
                    field.ChildNodes.add(new MarcSubfield("d", s));
                }
            }

            // 2015/7/19
            // 330
            List<string> reviews = GetCommentValues(root,
    nsmgr,
    "amazon:EditorialReviews/amazon:EditorialReview",
    "内容简介");
            if (reviews.Count > 0)
            {
                MarcField field = new MarcField("330", "  ");
                record.ChildNodes.add(field);
                foreach (string review in reviews)
                {
                    field.ChildNodes.add(new MarcSubfield("a", review));
                }
            }

            // 2015/7/19
            // 610
            List<string> subjects = GetFieldValues(root,
    nsmgr,
    "amazon:Subjects/amazon:Subject");
            if (subjects.Count > 0)
            {
                MarcField field = new MarcField("610", "0 ");
                record.ChildNodes.add(field);
                foreach (string subject in subjects)
                {
                    field.ChildNodes.add(new MarcSubfield("a", subject));
                }
            }

            // 2015/7/19
            // 7xx
            // authors 里面的元素通常已经被 creators 包含，creators 元素多余 authors。
            // 转换策略是，把 authors 里的作为 701，creators 里面余下的作为 702
            List<Creator> authors = GetCreatorValues(root,
    nsmgr,
    "amazon:ItemAttributes/amazon:Author");
            foreach (Creator author in authors)
            {
                MarcField field = new MarcField("701", " 0");
                record.ChildNodes.add(field);

                field.ChildNodes.add(new MarcSubfield("a", author.Name));
                if (string.IsNullOrEmpty(author.Role) == false)
                    field.ChildNodes.add(new MarcSubfield("4", author.Role));
            }

            foreach(Creator creator in creators)
            {
                if (IndexOf(authors, creator.Name) != -1)
                    continue;
                MarcField field = new MarcField("702", " 0");
                record.ChildNodes.add(field);

                field.ChildNodes.add(new MarcSubfield("a", creator.Name));
                if (string.IsNullOrEmpty(creator.Role) == false)
                    field.ChildNodes.add(new MarcSubfield("4", creator.Role));
            }

            // 856
            string[] names = new string[] { 
                "SmallImage",
                "MediumImage",
                "LargeImage"};
            foreach (string name in names)
            {
                List<ImageInfo> small_images = GetImageValues(root,
    nsmgr,
    "amazon:" + name);
                foreach (ImageInfo info in small_images)
                {
                    MarcField field = new MarcField("856", "4 ");
                    record.ChildNodes.add(field);

                    field.ChildNodes.add(new MarcSubfield("3", "Cover image"));

                    field.ChildNodes.add(new MarcSubfield("u", info.Url));
                    field.ChildNodes.add(new MarcSubfield("q", GetMime(info.Url)));

                    field.ChildNodes.add(new MarcSubfield("x", "type:FrontCover." + name + ";size:" + info.Size + ";source:Amazon:"));
                    //  + dlg.SelectedItem.ASIN
                }
            }

            strMARC = record.Text;
            return 0;
        }

        static int IndexOf(List<Creator> list, string strName)
        {
            int i = 0;
            foreach(Creator creator in list)
            {
                if (creator.Name == strName)
                    return i;
                i++;
            }
            return -1;
        }

        // 根据 URL 获得媒体类型
        public static string GetMime(string strUrl)
        {
#if NO
            if (Path.GetExtension(strUrl).ToLower() == ".jpg")
                return "image/jpeg";
            return "";
#endif
            string strExt = Path.GetExtension(strUrl).ToLower();
            if (string.IsNullOrEmpty(strExt) == true)
                return "";

            return GetMimeType(strExt);
        }

        private static IDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {

        #region Big freaking list of mime types
        // combination of values from Windows 7 Registry and 
        // from C:\Windows\System32\inetsrv\config\applicationHost.config
        // some added, including .7z and .dat
        {".323", "text/h323"},
        {".3g2", "video/3gpp2"},
        {".3gp", "video/3gpp"},
        {".3gp2", "video/3gpp2"},
        {".3gpp", "video/3gpp"},
        {".7z", "application/x-7z-compressed"},
        {".aa", "audio/audible"},
        {".AAC", "audio/aac"},
        {".aaf", "application/octet-stream"},
        {".aax", "audio/vnd.audible.aax"},
        {".ac3", "audio/ac3"},
        {".aca", "application/octet-stream"},
        {".accda", "application/msaccess.addin"},
        {".accdb", "application/msaccess"},
        {".accdc", "application/msaccess.cab"},
        {".accde", "application/msaccess"},
        {".accdr", "application/msaccess.runtime"},
        {".accdt", "application/msaccess"},
        {".accdw", "application/msaccess.webapplication"},
        {".accft", "application/msaccess.ftemplate"},
        {".acx", "application/internet-property-stream"},
        {".AddIn", "text/xml"},
        {".ade", "application/msaccess"},
        {".adobebridge", "application/x-bridge-url"},
        {".adp", "application/msaccess"},
        {".ADT", "audio/vnd.dlna.adts"},
        {".ADTS", "audio/aac"},
        {".afm", "application/octet-stream"},
        {".ai", "application/postscript"},
        {".aif", "audio/x-aiff"},
        {".aifc", "audio/aiff"},
        {".aiff", "audio/aiff"},
        {".air", "application/vnd.adobe.air-application-installer-package+zip"},
        {".amc", "application/x-mpeg"},
        {".application", "application/x-ms-application"},
        {".art", "image/x-jg"},
        {".asa", "application/xml"},
        {".asax", "application/xml"},
        {".ascx", "application/xml"},
        {".asd", "application/octet-stream"},
        {".asf", "video/x-ms-asf"},
        {".ashx", "application/xml"},
        {".asi", "application/octet-stream"},
        {".asm", "text/plain"},
        {".asmx", "application/xml"},
        {".aspx", "application/xml"},
        {".asr", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".atom", "application/atom+xml"},
        {".au", "audio/basic"},
        {".avi", "video/x-msvideo"},
        {".axs", "application/olescript"},
        {".bas", "text/plain"},
        {".bcpio", "application/x-bcpio"},
        {".bin", "application/octet-stream"},
        {".bmp", "image/bmp"},
        {".c", "text/plain"},
        {".cab", "application/octet-stream"},
        {".caf", "audio/x-caf"},
        {".calx", "application/vnd.ms-office.calx"},
        {".cat", "application/vnd.ms-pki.seccat"},
        {".cc", "text/plain"},
        {".cd", "text/plain"},
        {".cdda", "audio/aiff"},
        {".cdf", "application/x-cdf"},
        {".cer", "application/x-x509-ca-cert"},
        {".chm", "application/octet-stream"},
        {".class", "application/x-java-applet"},
        {".clp", "application/x-msclip"},
        {".cmx", "image/x-cmx"},
        {".cnf", "text/plain"},
        {".cod", "image/cis-cod"},
        {".config", "application/xml"},
        {".contact", "text/x-ms-contact"},
        {".coverage", "application/xml"},
        {".cpio", "application/x-cpio"},
        {".cpp", "text/plain"},
        {".crd", "application/x-mscardfile"},
        {".crl", "application/pkix-crl"},
        {".crt", "application/x-x509-ca-cert"},
        {".cs", "text/plain"},
        {".csdproj", "text/plain"},
        {".csh", "application/x-csh"},
        {".csproj", "text/plain"},
        {".css", "text/css"},
        {".csv", "text/csv"},
        {".cur", "application/octet-stream"},
        {".cxx", "text/plain"},
        {".dat", "application/octet-stream"},
        {".datasource", "application/xml"},
        {".dbproj", "text/plain"},
        {".dcr", "application/x-director"},
        {".def", "text/plain"},
        {".deploy", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dgml", "application/xml"},
        {".dib", "image/bmp"},
        {".dif", "video/x-dv"},
        {".dir", "application/x-director"},
        {".disco", "text/xml"},
        {".dll", "application/x-msdownload"},
        {".dll.config", "text/xml"},
        {".dlm", "text/dlm"},
        {".doc", "application/msword"},
        {".docm", "application/vnd.ms-word.document.macroEnabled.12"},
        {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        {".dot", "application/msword"},
        {".dotm", "application/vnd.ms-word.template.macroEnabled.12"},
        {".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
        {".dsp", "application/octet-stream"},
        {".dsw", "text/plain"},
        {".dtd", "text/xml"},
        {".dtsConfig", "text/xml"},
        {".dv", "video/x-dv"},
        {".dvi", "application/x-dvi"},
        {".dwf", "drawing/x-dwf"},
        {".dwp", "application/octet-stream"},
        {".dxr", "application/x-director"},
        {".eml", "message/rfc822"},
        {".emz", "application/octet-stream"},
        {".eot", "application/octet-stream"},
        {".eps", "application/postscript"},
        {".etl", "application/etl"},
        {".etx", "text/x-setext"},
        {".evy", "application/envoy"},
        {".exe", "application/octet-stream"},
        {".exe.config", "text/xml"},
        {".fdf", "application/vnd.fdf"},
        {".fif", "application/fractals"},
        {".filters", "Application/xml"},
        {".fla", "application/octet-stream"},
        {".flr", "x-world/x-vrml"},
        {".flv", "video/x-flv"},
        {".fsscript", "application/fsharp-script"},
        {".fsx", "application/fsharp-script"},
        {".generictest", "application/xml"},
        {".gif", "image/gif"},
        {".group", "text/x-ms-group"},
        {".gsm", "audio/x-gsm"},
        {".gtar", "application/x-gtar"},
        {".gz", "application/x-gzip"},
        {".h", "text/plain"},
        {".hdf", "application/x-hdf"},
        {".hdml", "text/x-hdml"},
        {".hhc", "application/x-oleobject"},
        {".hhk", "application/octet-stream"},
        {".hhp", "application/octet-stream"},
        {".hlp", "application/winhlp"},
        {".hpp", "text/plain"},
        {".hqx", "application/mac-binhex40"},
        {".hta", "application/hta"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".htt", "text/webviewhtml"},
        {".hxa", "application/xml"},
        {".hxc", "application/xml"},
        {".hxd", "application/octet-stream"},
        {".hxe", "application/xml"},
        {".hxf", "application/xml"},
        {".hxh", "application/octet-stream"},
        {".hxi", "application/octet-stream"},
        {".hxk", "application/xml"},
        {".hxq", "application/octet-stream"},
        {".hxr", "application/octet-stream"},
        {".hxs", "application/octet-stream"},
        {".hxt", "text/html"},
        {".hxv", "application/xml"},
        {".hxw", "application/octet-stream"},
        {".hxx", "text/plain"},
        {".i", "text/plain"},
        {".ico", "image/x-icon"},
        {".ics", "application/octet-stream"},
        {".idl", "text/plain"},
        {".ief", "image/ief"},
        {".iii", "application/x-iphone"},
        {".inc", "text/plain"},
        {".inf", "application/octet-stream"},
        {".inl", "text/plain"},
        {".ins", "application/x-internet-signup"},
        {".ipa", "application/x-itunes-ipa"},
        {".ipg", "application/x-itunes-ipg"},
        {".ipproj", "text/plain"},
        {".ipsw", "application/x-itunes-ipsw"},
        {".iqy", "text/x-ms-iqy"},
        {".isp", "application/x-internet-signup"},
        {".ite", "application/x-itunes-ite"},
        {".itlp", "application/x-itunes-itlp"},
        {".itms", "application/x-itunes-itms"},
        {".itpc", "application/x-itunes-itpc"},
        {".IVF", "video/x-ivf"},
        {".jar", "application/java-archive"},
        {".java", "application/octet-stream"},
        {".jck", "application/liquidmotion"},
        {".jcz", "application/liquidmotion"},
        {".jfif", "image/pjpeg"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpb", "application/octet-stream"},
        {".jpe", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".json", "application/json"},
        {".jsx", "text/jscript"},
        {".jsxbin", "text/plain"},
        {".latex", "application/x-latex"},
        {".library-ms", "application/windows-library+xml"},
        {".lit", "application/x-ms-reader"},
        {".loadtest", "application/xml"},
        {".lpk", "application/octet-stream"},
        {".lsf", "video/x-la-asf"},
        {".lst", "text/plain"},
        {".lsx", "video/x-la-asf"},
        {".lzh", "application/octet-stream"},
        {".m13", "application/x-msmediaview"},
        {".m14", "application/x-msmediaview"},
        {".m1v", "video/mpeg"},
        {".m2t", "video/vnd.dlna.mpeg-tts"},
        {".m2ts", "video/vnd.dlna.mpeg-tts"},
        {".m2v", "video/mpeg"},
        {".m3u", "audio/x-mpegurl"},
        {".m3u8", "audio/x-mpegurl"},
        {".m4a", "audio/m4a"},
        {".m4b", "audio/m4b"},
        {".m4p", "audio/m4p"},
        {".m4r", "audio/x-m4r"},
        {".m4v", "video/x-m4v"},
        {".mac", "image/x-macpaint"},
        {".mak", "text/plain"},
        {".man", "application/x-troff-man"},
        {".manifest", "application/x-ms-manifest"},
        {".map", "text/plain"},
        {".master", "application/xml"},
        {".mda", "application/msaccess"},
        {".mdb", "application/x-msaccess"},
        {".mde", "application/msaccess"},
        {".mdp", "application/octet-stream"},
        {".me", "application/x-troff-me"},
        {".mfp", "application/x-shockwave-flash"},
        {".mht", "message/rfc822"},
        {".mhtml", "message/rfc822"},
        {".mid", "audio/mid"},
        {".midi", "audio/mid"},
        {".mix", "application/octet-stream"},
        {".mk", "text/plain"},
        {".mmf", "application/x-smaf"},
        {".mno", "text/xml"},
        {".mny", "application/x-msmoney"},
        {".mod", "video/mpeg"},
        {".mov", "video/quicktime"},
        {".movie", "video/x-sgi-movie"},
        {".mp2", "video/mpeg"},
        {".mp2v", "video/mpeg"},
        {".mp3", "audio/mpeg"},
        {".mp4", "video/mp4"},
        {".mp4v", "video/mp4"},
        {".mpa", "video/mpeg"},
        {".mpe", "video/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpf", "application/vnd.ms-mediapackage"},
        {".mpg", "video/mpeg"},
        {".mpp", "application/vnd.ms-project"},
        {".mpv2", "video/mpeg"},
        {".mqv", "video/quicktime"},
        {".ms", "application/x-troff-ms"},
        {".msi", "application/octet-stream"},
        {".mso", "application/octet-stream"},
        {".mts", "video/vnd.dlna.mpeg-tts"},
        {".mtx", "application/xml"},
        {".mvb", "application/x-msmediaview"},
        {".mvc", "application/x-miva-compiled"},
        {".mxp", "application/x-mmxp"},
        {".nc", "application/x-netcdf"},
        {".nsc", "video/x-ms-asf"},
        {".nws", "message/rfc822"},
        {".ocx", "application/octet-stream"},
        {".oda", "application/oda"},
        {".odc", "text/x-ms-odc"},
        {".odh", "text/plain"},
        {".odl", "text/plain"},
        {".odp", "application/vnd.oasis.opendocument.presentation"},
        {".ods", "application/oleobject"},
        {".odt", "application/vnd.oasis.opendocument.text"},
        {".one", "application/onenote"},
        {".onea", "application/onenote"},
        {".onepkg", "application/onenote"},
        {".onetmp", "application/onenote"},
        {".onetoc", "application/onenote"},
        {".onetoc2", "application/onenote"},
        {".orderedtest", "application/xml"},
        {".osdx", "application/opensearchdescription+xml"},
        {".p10", "application/pkcs10"},
        {".p12", "application/x-pkcs12"},
        {".p7b", "application/x-pkcs7-certificates"},
        {".p7c", "application/pkcs7-mime"},
        {".p7m", "application/pkcs7-mime"},
        {".p7r", "application/x-pkcs7-certreqresp"},
        {".p7s", "application/pkcs7-signature"},
        {".pbm", "image/x-portable-bitmap"},
        {".pcast", "application/x-podcast"},
        {".pct", "image/pict"},
        {".pcx", "application/octet-stream"},
        {".pcz", "application/octet-stream"},
        {".pdf", "application/pdf"},
        {".pfb", "application/octet-stream"},
        {".pfm", "application/octet-stream"},
        {".pfx", "application/x-pkcs12"},
        {".pgm", "image/x-portable-graymap"},
        {".pic", "image/pict"},
        {".pict", "image/pict"},
        {".pkgdef", "text/plain"},
        {".pkgundef", "text/plain"},
        {".pko", "application/vnd.ms-pki.pko"},
        {".pls", "audio/scpls"},
        {".pma", "application/x-perfmon"},
        {".pmc", "application/x-perfmon"},
        {".pml", "application/x-perfmon"},
        {".pmr", "application/x-perfmon"},
        {".pmw", "application/x-perfmon"},
        {".png", "image/png"},
        {".pnm", "image/x-portable-anymap"},
        {".pnt", "image/x-macpaint"},
        {".pntg", "image/x-macpaint"},
        {".pnz", "image/png"},
        {".pot", "application/vnd.ms-powerpoint"},
        {".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12"},
        {".potx", "application/vnd.openxmlformats-officedocument.presentationml.template"},
        {".ppa", "application/vnd.ms-powerpoint"},
        {".ppam", "application/vnd.ms-powerpoint.addin.macroEnabled.12"},
        {".ppm", "image/x-portable-pixmap"},
        {".pps", "application/vnd.ms-powerpoint"},
        {".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
        {".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
        {".ppt", "application/vnd.ms-powerpoint"},
        {".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
        {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        {".prf", "application/pics-rules"},
        {".prm", "application/octet-stream"},
        {".prx", "application/octet-stream"},
        {".ps", "application/postscript"},
        {".psc1", "application/PowerShell"},
        {".psd", "application/octet-stream"},
        {".psess", "application/xml"},
        {".psm", "application/octet-stream"},
        {".psp", "application/octet-stream"},
        {".pub", "application/x-mspublisher"},
        {".pwz", "application/vnd.ms-powerpoint"},
        {".qht", "text/x-html-insertion"},
        {".qhtm", "text/x-html-insertion"},
        {".qt", "video/quicktime"},
        {".qti", "image/x-quicktime"},
        {".qtif", "image/x-quicktime"},
        {".qtl", "application/x-quicktimeplayer"},
        {".qxd", "application/octet-stream"},
        {".ra", "audio/x-pn-realaudio"},
        {".ram", "audio/x-pn-realaudio"},
        {".rar", "application/octet-stream"},
        {".ras", "image/x-cmu-raster"},
        {".rat", "application/rat-file"},
        {".rc", "text/plain"},
        {".rc2", "text/plain"},
        {".rct", "text/plain"},
        {".rdlc", "application/xml"},
        {".resx", "application/xml"},
        {".rf", "image/vnd.rn-realflash"},
        {".rgb", "image/x-rgb"},
        {".rgs", "text/plain"},
        {".rm", "application/vnd.rn-realmedia"},
        {".rmi", "audio/mid"},
        {".rmp", "application/vnd.rn-rn_music_package"},
        {".roff", "application/x-troff"},
        {".rpm", "audio/x-pn-realaudio-plugin"},
        {".rqy", "text/x-ms-rqy"},
        {".rtf", "application/rtf"},
        {".rtx", "text/richtext"},
        {".ruleset", "application/xml"},
        {".s", "text/plain"},
        {".safariextz", "application/x-safari-safariextz"},
        {".scd", "application/x-msschedule"},
        {".sct", "text/scriptlet"},
        {".sd2", "audio/x-sd2"},
        {".sdp", "application/sdp"},
        {".sea", "application/octet-stream"},
        {".searchConnector-ms", "application/windows-search-connector+xml"},
        {".setpay", "application/set-payment-initiation"},
        {".setreg", "application/set-registration-initiation"},
        {".settings", "application/xml"},
        {".sgimb", "application/x-sgimb"},
        {".sgml", "text/sgml"},
        {".sh", "application/x-sh"},
        {".shar", "application/x-shar"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".sitemap", "application/xml"},
        {".skin", "application/xml"},
        {".sldm", "application/vnd.ms-powerpoint.slide.macroEnabled.12"},
        {".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide"},
        {".slk", "application/vnd.ms-excel"},
        {".sln", "text/plain"},
        {".slupkg-ms", "application/x-ms-license"},
        {".smd", "audio/x-smd"},
        {".smi", "application/octet-stream"},
        {".smx", "audio/x-smd"},
        {".smz", "audio/x-smd"},
        {".snd", "audio/basic"},
        {".snippet", "application/xml"},
        {".snp", "application/octet-stream"},
        {".sol", "text/plain"},
        {".sor", "text/plain"},
        {".spc", "application/x-pkcs7-certificates"},
        {".spl", "application/futuresplash"},
        {".src", "application/x-wais-source"},
        {".srf", "text/plain"},
        {".SSISDeploymentManifest", "text/xml"},
        {".ssm", "application/streamingmedia"},
        {".sst", "application/vnd.ms-pki.certstore"},
        {".stl", "application/vnd.ms-pki.stl"},
        {".sv4cpio", "application/x-sv4cpio"},
        {".sv4crc", "application/x-sv4crc"},
        {".svc", "application/xml"},
        {".swf", "application/x-shockwave-flash"},
        {".t", "application/x-troff"},
        {".tar", "application/x-tar"},
        {".tcl", "application/x-tcl"},
        {".testrunconfig", "application/xml"},
        {".testsettings", "application/xml"},
        {".tex", "application/x-tex"},
        {".texi", "application/x-texinfo"},
        {".texinfo", "application/x-texinfo"},
        {".tgz", "application/x-compressed"},
        {".thmx", "application/vnd.ms-officetheme"},
        {".thn", "application/octet-stream"},
        {".tif", "image/tiff"},
        {".tiff", "image/tiff"},
        {".tlh", "text/plain"},
        {".tli", "text/plain"},
        {".toc", "application/octet-stream"},
        {".tr", "application/x-troff"},
        {".trm", "application/x-msterminal"},
        {".trx", "application/xml"},
        {".ts", "video/vnd.dlna.mpeg-tts"},
        {".tsv", "text/tab-separated-values"},
        {".ttf", "application/octet-stream"},
        {".tts", "video/vnd.dlna.mpeg-tts"},
        {".txt", "text/plain"},
        {".u32", "application/octet-stream"},
        {".uls", "text/iuls"},
        {".user", "text/plain"},
        {".ustar", "application/x-ustar"},
        {".vb", "text/plain"},
        {".vbdproj", "text/plain"},
        {".vbk", "video/mpeg"},
        {".vbproj", "text/plain"},
        {".vbs", "text/vbscript"},
        {".vcf", "text/x-vcard"},
        {".vcproj", "Application/xml"},
        {".vcs", "text/plain"},
        {".vcxproj", "Application/xml"},
        {".vddproj", "text/plain"},
        {".vdp", "text/plain"},
        {".vdproj", "text/plain"},
        {".vdx", "application/vnd.ms-visio.viewer"},
        {".vml", "text/xml"},
        {".vscontent", "application/xml"},
        {".vsct", "text/xml"},
        {".vsd", "application/vnd.visio"},
        {".vsi", "application/ms-vsi"},
        {".vsix", "application/vsix"},
        {".vsixlangpack", "text/xml"},
        {".vsixmanifest", "text/xml"},
        {".vsmdi", "application/xml"},
        {".vspscc", "text/plain"},
        {".vss", "application/vnd.visio"},
        {".vsscc", "text/plain"},
        {".vssettings", "text/xml"},
        {".vssscc", "text/plain"},
        {".vst", "application/vnd.visio"},
        {".vstemplate", "text/xml"},
        {".vsto", "application/x-ms-vsto"},
        {".vsw", "application/vnd.visio"},
        {".vsx", "application/vnd.visio"},
        {".vtx", "application/vnd.visio"},
        {".wav", "audio/wav"},
        {".wave", "audio/wav"},
        {".wax", "audio/x-ms-wax"},
        {".wbk", "application/msword"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wcm", "application/vnd.ms-works"},
        {".wdb", "application/vnd.ms-works"},
        {".wdp", "image/vnd.ms-photo"},
        {".webarchive", "application/x-safari-webarchive"},
        {".webtest", "application/xml"},
        {".wiq", "application/xml"},
        {".wiz", "application/msword"},
        {".wks", "application/vnd.ms-works"},
        {".WLMP", "application/wlmoviemaker"},
        {".wlpginstall", "application/x-wlpg-detect"},
        {".wlpginstall3", "application/x-wlpg3-detect"},
        {".wm", "video/x-ms-wm"},
        {".wma", "audio/x-ms-wma"},
        {".wmd", "application/x-ms-wmd"},
        {".wmf", "application/x-msmetafile"},
        {".wml", "text/vnd.wap.wml"},
        {".wmlc", "application/vnd.wap.wmlc"},
        {".wmls", "text/vnd.wap.wmlscript"},
        {".wmlsc", "application/vnd.wap.wmlscriptc"},
        {".wmp", "video/x-ms-wmp"},
        {".wmv", "video/x-ms-wmv"},
        {".wmx", "video/x-ms-wmx"},
        {".wmz", "application/x-ms-wmz"},
        {".wpl", "application/vnd.ms-wpl"},
        {".wps", "application/vnd.ms-works"},
        {".wri", "application/x-mswrite"},
        {".wrl", "x-world/x-vrml"},
        {".wrz", "x-world/x-vrml"},
        {".wsc", "text/scriptlet"},
        {".wsdl", "text/xml"},
        {".wvx", "video/x-ms-wvx"},
        {".x", "application/directx"},
        {".xaf", "x-world/x-vrml"},
        {".xaml", "application/xaml+xml"},
        {".xap", "application/x-silverlight-app"},
        {".xbap", "application/x-ms-xbap"},
        {".xbm", "image/x-xbitmap"},
        {".xdr", "text/plain"},
        {".xht", "application/xhtml+xml"},
        {".xhtml", "application/xhtml+xml"},
        {".xla", "application/vnd.ms-excel"},
        {".xlam", "application/vnd.ms-excel.addin.macroEnabled.12"},
        {".xlc", "application/vnd.ms-excel"},
        {".xld", "application/vnd.ms-excel"},
        {".xlk", "application/vnd.ms-excel"},
        {".xll", "application/vnd.ms-excel"},
        {".xlm", "application/vnd.ms-excel"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
        {".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12"},
        {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
        {".xlt", "application/vnd.ms-excel"},
        {".xltm", "application/vnd.ms-excel.template.macroEnabled.12"},
        {".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
        {".xlw", "application/vnd.ms-excel"},
        {".xml", "text/xml"},
        {".xmta", "application/xml"},
        {".xof", "x-world/x-vrml"},
        {".XOML", "text/plain"},
        {".xpm", "image/x-xpixmap"},
        {".xps", "application/vnd.ms-xpsdocument"},
        {".xrm-ms", "text/xml"},
        {".xsc", "application/xml"},
        {".xsd", "text/xml"},
        {".xsf", "text/xml"},
        {".xsl", "text/xml"},
        {".xslt", "text/xml"},
        {".xsn", "application/octet-stream"},
        {".xss", "application/xml"},
        {".xtp", "application/octet-stream"},
        {".xwd", "image/x-xwindowdump"},
        {".z", "application/x-compress"},
        {".zip", "application/x-zip-compressed"},
        #endregion

        };

        // http://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension
        public static string GetMimeType(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            string mime;

            return _mappings.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
        }



        static string GetPublishDateString(string strText)
        {
            strText = strText.Replace("-", "");

            if (strText.Length < 6)
                return strText;

            return strText.Substring(0, 4) + "." + strText.Substring(4, 2).TrimStart(new char[] { '0' });
        }
    }

    [Serializable]  // 为了 Copy / Paste
    public class AmazonItemInfo
    {
        public string Xml = ""; // XML 记录
        public string EelementSet = ""; // "B" "F"
        public string PreferSyntaxOID = ""; // 优先要转换成的 MARC 格式
    }
}
