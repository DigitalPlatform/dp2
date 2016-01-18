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
using DigitalPlatform.IO;

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
            DeleteTempFile();

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
            // 这一句移到 try ... catch 以外，是为了观察测试抛出异常的情况。如果测试完成后，需要放回里面去
            File.Delete(this.TempFilename);
        }

        void DeleteTempFile()
        {
            // DisposeWebClients();

            // 删除临时文件
            if (string.IsNullOrEmpty(this.TempFilename) == false)
            {
                try
                {
                }
                catch
                {
                }
                // 这一句移到 try ... catch 以外，是为了观察测试抛出异常的情况。如果测试完成后，需要放回里面去
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

#if NO
        List<WebClient> _webClients = new List<WebClient>();

        void DisposeWebClients()
        {
            foreach (WebClient webClient in _webClients)
            {
                if (webClient != null)
                {
                    webClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(webClient_MultiLineDownloadFileCompleted);
                    webClient.Dispose();
                }
            }

            _webClients.Clear();
        }
#endif

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

            // _webClients.Add(webClient);

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
                strError = ExceptionUtil.GetAutoText(ex);
                this.m_bError = true;
                goto ERROR1;
            }

            // 等待检索结束
            bool bError = WaitSearchFinish();
            if (bError == true)
            {
                strError = this.m_strError;
                goto ERROR1;
            }

            {
                AsyncCompletedEventArgs e = _end_e;

                if (e == null || e.Cancelled == true)
                {
                    strError = "请求被取消";
                    goto ERROR1;
                }

                if (e != null && e.Error != null)
                {
                    strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                    this.m_exception = e.Error;
                    goto ERROR1;
                }
            }

            // 如果要求每行的检索命中装入大于 10 条，需要在这里获取后面几批的浏览结果
            return 0;
        ERROR1:
            // 迫使更换临时文件
            DeleteTempFile();   // 2015/8/4
            return -1;
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
                strError = ExceptionUtil.GetAutoText(ex);
                this.m_bError = true;
                goto ERROR1;
            }

            // 等待检索结束
            bool bError = WaitSearchFinish();
            if (bError == true)
            {
                strError = this.m_strError;
                goto ERROR1;
            }

            {
                AsyncCompletedEventArgs e = _end_e;

                if (e == null || e.Cancelled == true)
                {
                    strError = "请求被取消";
                    goto ERROR1;
                }

                if (e != null && e.Error != null)
                {
                    strError = "请求过程发生错误: " + ExceptionUtil.GetExceptionMessage(e.Error);
                    this.m_exception = e.Error;
                    goto ERROR1;
                }
            }

            return 0;
        ERROR1:
            // 迫使更换临时文件
            DeleteTempFile();   // 2015/8/4
            return -1;
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
#if NO
                    nTimeCount = -1;
                    this.m_strError = "超时";
                    this.m_bError = true;
                    break;  // 2015/5/22
#endif
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
                    if (code.InnerText.Trim() == "CNY")
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
                    else
                        strText += strAmount;
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

            return creator;
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
                    if (strUnits == "hundredths-inches")
                        return Math.Ceiling(((double)v / (double)100) * (double)2.54).ToString() + "cm";

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
        // parameters:
        //      strStyle    转换风格。！856 表示不包含 856 字段
        public static int AmazonXmlToUNIMARC(XmlNode root,
            string strStyle,
            out string strMARC,
            out string strError)
        {
            strMARC = "";
            strError = "";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", AmazonSearch.NAMESPACE);

            MarcRecord record = new MarcRecord();

            // *** 001
            string strASIN = DomUtil.GetElementText(root, "amazon:ASIN", nsmgr);
            if (string.IsNullOrEmpty(strASIN) == false)
                record.ChildNodes.add(new MarcField("001", "ASIN:" + strASIN));

            // *** 010
            // ISBN
#if NO
            List<string> isbns = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ISBN");
            if (isbns.Count == 0)
                isbns = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:EAN");
#endif
            List<string> isbns = GetFieldValues(root,
    nsmgr,
    "amazon:ItemAttributes/amazon:EAN");
            if (isbns.Count == 0)
                isbns = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ISBN");

            // Binding
            List<string> bindings = GetFieldValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:Binding");

            // 价格
            List<string> prices = GetPriceValues(root,
                nsmgr,
                "amazon:ItemAttributes/amazon:ListPrice");

            for (int i = 0; i < Math.Max(isbns.Count, prices.Count); i++)
            {
                string isbn = "";
                string binding = "";
                string price = "";

                if (i < isbns.Count)
                    isbn = isbns[i];
                if (i < bindings.Count)
                    binding = bindings[i];
                if (i < prices.Count)
                    price = prices[i];
                MarcField field = new MarcField("010", "  ");
                record.ChildNodes.add(field);

                if (string.IsNullOrEmpty(isbn) == false)
                    field.ChildNodes.add(new MarcSubfield("a", isbn));
                if (string.IsNullOrEmpty(binding) == false
                    && binding != "平装")
                    field.ChildNodes.add(new MarcSubfield("b", binding));
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

            foreach (Creator creator in creators)
            {
                if (IndexOf(authors, creator.Name) != -1)
                    continue;
                MarcField field = new MarcField("702", " 0");
                record.ChildNodes.add(field);

                field.ChildNodes.add(new MarcSubfield("a", creator.Name));
                if (string.IsNullOrEmpty(creator.Role) == false)
                    field.ChildNodes.add(new MarcSubfield("4", creator.Role));
            }

            if (StringUtil.IsInList("!856", strStyle) == false)
            {
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
            }

            strMARC = record.Text;
            return 0;
        }

        static int IndexOf(List<Creator> list, string strName)
        {
            int i = 0;
            foreach (Creator creator in list)
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

            return PathUtil.GetMimeTypeByFileExtension(strExt);
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
