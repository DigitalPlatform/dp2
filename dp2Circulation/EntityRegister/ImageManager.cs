using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Net;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    // TODO: 增加下载 http 图像文件的能力
    /// <summary>
    /// dp2 图像对象管理器
    /// 用独立线程获取图像存储到本地文件
    /// </summary>
    public class ImageManager : ThreadBase
    {
        public LibraryChannelPool ChannelPool = null;
        public string TempFileDir = "";

        public event GetObjectCompleteEventHandler GetObjectComplete = null;

        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        // 存储图像和行的对应关系，好在图像获取完成后，加入到显示
        List<TraceObject> _trace_images = new List<TraceObject>();

        Hashtable _localFileCache = new Hashtable();

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (this.Stopped == true)
                    return;

                try
                {
                    this.DownloadImages();
                }
                catch
                {
                    // TODO: 如何报错
                }

                // m_bStopThread = true;   // 只作一轮就停止
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

        ERROR1:
            // Safe_setError(this.Container.listView_in, strError);
            return;
        }

        public void ClearList()
        {
            // 放弃尚未获取的排队请求
            lock (_trace_images)
            {
                _trace_images.Clear();
            }

            if (this._webClient != null)
            {
                // this._webClient.CancelAsync();
                this._webClient.Cancel();
                this._webClient = null;
            }

            // TODO: 如何中断正在进行的 dp2library 操作?

            DeleteTempFiles();

            if (_localFileCache != null)
                _localFileCache.Clear();
        }

        MyWebClient _webClient = null;

        // 下载图像文件
        void DownloadImages()
        {
            string strError = "";

            TraceObject info = null;
            lock (_trace_images)
            {
                if (_trace_images.Count == 0)
                    return;

                info = _trace_images[0];
                _trace_images.RemoveAt(0);
            }

            if (string.IsNullOrEmpty(info.FileName) == true)
                info.FileName = GetTempFileName();

            // http 协议的图像文件
            if (StringUtil.HasHead(info.ObjectPath, "http:") == true)
            {
                // 先从 cache 中找
                if (_localFileCache != null)
                {
                    string strLocalFile = (string)_localFileCache[info.ObjectPath];
                    if (string.IsNullOrEmpty(strLocalFile) == false)
                    {
                        if (string.IsNullOrEmpty(info.FileName) == false)
                        {
                            DeleteTempFile(info.FileName);
                            info.FileName = "";
                        }
                        info.FileName = strLocalFile;
                        goto END1;
                    }
                }

                if (_webClient == null)
                {
                    _webClient = new MyWebClient();
                    _webClient.Timeout = 5000;
                    _webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
                }

                try
                {
                    _webClient.DownloadFile(new Uri(info.ObjectPath, UriKind.Absolute), info.FileName);
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                strError = ex.Message;
                                goto ERROR1;
                            }
                        }
                    }

                    strError = ex.Message;
                    goto ERROR1;
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    goto ERROR1;
                }

                _localFileCache[info.ObjectPath] = info.FileName;
            }
            else
            {
                // dp2library 协议的对象资源

                LibraryChannel channel = this.ChannelPool.GetChannel(info.ServerUrl, info.UserName);

                byte[] baOutputTimeStamp = null;
                string strMetaData = "";
                string strTempOutputPath = "";

                long lRet = channel.GetRes(
                    null,
                    info.ObjectPath,
                    info.FileName,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strTempOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "下载资源文件失败，原因: " + strError;
                    goto ERROR1;
                }

                _localFileCache[info.ObjectPath] = info.FileName;
            }

            END1:
            // 通知
            if (this.GetObjectComplete != null)
            {
                GetObjectCompleteEventArgs e = new GetObjectCompleteEventArgs();
                e.TraceObject = info;
                this.GetObjectComplete(this, e);
            }
            this.Activate();
            return;
        ERROR1:
            // 通知
            if (this.GetObjectComplete != null)
            {
                GetObjectCompleteEventArgs e = new GetObjectCompleteEventArgs();
                e.TraceObject = info;
                e.ErrorInfo = strError;
                this.GetObjectComplete(this, e);
            }
        }

        // 加入一个获取对象文件的请求
        // parameters:
        //      strFileName 临时文件名。可为空，功能中会自动给出一个临时文件名
        public void AsyncGetObjectFile(string strServerUrl,
            string strUserName,
            string strObjectPath,
            string strFileName,
            object tag)
        {
            TraceObject trace = new TraceObject();
            trace.ServerUrl = strServerUrl;
            trace.UserName = strUserName;
            trace.ObjectPath = strObjectPath;
            trace.FileName = strFileName;
            trace.Tag = tag;
            lock (_trace_images)
            {
                _trace_images.Add(trace);
            }
        }

        // TODO: 文件名太多了怎么办? 
        List<string> _tempFileNames = new List<string>();

        // 准备临时文件名
        string GetTempFileName()
        {
            Debug.Assert(string.IsNullOrEmpty(this.TempFileDir) == false, "");
            string strTempFileName = Path.Combine(this.TempFileDir, "~image_" + Guid.NewGuid().ToString());
            if (this._tempFileNames != null)
                _tempFileNames.Add(strTempFileName);
            return strTempFileName;
        }

        void DeleteTempFile(string strFileName)
        {
            if (this._tempFileNames == null)
                return;
            int index = this._tempFileNames.IndexOf(strFileName);
            if (index != -1)
            {
                this._tempFileNames.RemoveAt(index);
                try
                {
                    File.Delete(strFileName);
                }
                catch
                {

                }
            }
        }

        // 删除全部临时文件
        public void DeleteTempFiles()
        {
            if (this._tempFileNames == null)
                return;

            foreach (string strFileName in this._tempFileNames)
            {
                if (File.Exists(strFileName) == true)
                {
                    try
                    {
                        File.Delete(strFileName);
                    }
                    catch
                    {
                    }
                }
            }

            this._tempFileNames.Clear();
        }

    }

    // 追踪图像和行对象的关系
    public class TraceObject
    {
        public string ServerUrl = "";
        public string UserName = "";

        public string ObjectPath = "";    // 对象路径
        public object Tag = null;    // 关联的数据，用于定位发起的对象

        public string FileName = "";    // [in][out] 存储下载图像内容的文件名。如果调用前给出这个文件名，功能就使用它；否则就自动创建一个临时文件
    }

    class MyWebClient : WebClient
    {
        public int Timeout = -1;

        HttpWebRequest _request = null;

        protected override WebRequest GetWebRequest(Uri address)
        {
            _request = (HttpWebRequest)base.GetWebRequest(address);

#if NO
            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
#endif
            if (this.Timeout != -1)
                _request.Timeout = this.Timeout;
            return _request;
        }

        public void Cancel()
        {
            if (this._request != null)
                this._request.Abort();
        }
    }

    /// <summary>
    /// 完成事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetObjectCompleteEventHandler(object sender,
    GetObjectCompleteEventArgs e);

    /// <summary>
    /// 完成事件的参数
    /// </summary>
    public class GetObjectCompleteEventArgs : EventArgs
    {
        public TraceObject TraceObject = null;
        public string ErrorInfo = "";   // 错误信息
    }
}
