using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform;
using System.Threading;
using DigitalPlatform.CirculationClient;
using System.IO;
using System.Diagnostics;

namespace dp2Circulation
{
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

        // 下载图像文件
        void DownloadImages()
        {
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

            LibraryChannel channel = this.ChannelPool.GetChannel(info.ServerUrl, info.UserName);

            byte[] baOutputTimeStamp = null;
            string strMetaData = "";
            string strTempOutputPath = "";
            string strError = "";

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
