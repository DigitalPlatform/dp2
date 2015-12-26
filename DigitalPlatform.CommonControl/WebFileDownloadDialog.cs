using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;   // for WebClient class
using System.Net.Cache;
using System.Threading;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;

namespace DigitalPlatform.CommonControl
{
    public partial class WebFileDownloadDialog : Form
    {
        const int WM_RUN = API.WM_USER + 200;

        public AutoResetEvent eventComplete = new AutoResetEvent(false);
        public AsyncCompletedEventArgs e = null;
        public MyWebClient webClient = null;
        public string Url = "";
        public string OutputFilename = "";
        public string TempFielname = "";
        public string IfModifySince = "";
        public string LastModified = "";
        public string ErrorInfo = "";
        public bool NotModified = false;

        public WebFileDownloadDialog()
        {
            InitializeComponent();
        }

        private void WebFileDownloadDialog_Load(object sender, EventArgs e)
        {
            API.PostMessage(this.Handle, WM_RUN, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_RUN:
                    {
                        this.MessageText = "正在下载Web文件 " + this.Url;

                        if (string.IsNullOrEmpty(this.TempFielname) == true)
                            this.TempFielname = Path.GetTempFileName();

                        MyWebClient webClient = new MyWebClient();

                        if (string.IsNullOrEmpty(this.IfModifySince) == false)
                            webClient.IfModifiedSince = this.IfModifySince;

                        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
                        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                        try
                        {


                            webClient.DownloadFileAsync(new Uri(this.Url, UriKind.Absolute),
                                this.TempFielname, null);
                            while (true)
                            {
                                Application.DoEvents();
                                if (eventComplete.WaitOne(100) == true)
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {

                            // File.Delete(this.OutputFielname);

                            this.ErrorInfo = "下载 " + this.Url + " 时发生错误 :" + ex.Message;
                            this.Close();
                            return;
                        }
                        finally
                        {
                            if (webClient.ResponseHeaders != null)
                                this.LastModified = webClient.ResponseHeaders["Last-Modified"];
                            webClient.CancelAsync();
                            webClient.Dispose();
                        }

                        if (this.e == null
    || this.e.Cancelled == true)
                        {
                            try
                            {
                                File.Delete(this.TempFielname);
                            }
                            catch
                            {
                            }

                            this.ErrorInfo = "下载 " + this.Url + " 被取消";
                        }

                        if (this.e != null)
                        {
                            if (e.Error != null)
                            {
                                try
                                {
                                    File.Delete(this.TempFielname);
                                }
                                catch
                                {
                                }
                                // TODO: 准备处理304响应
                                var webException = this.e.Error as WebException;
                                if (null != webException)
                                {
                                    var httWebResponse = webException.Response as HttpWebResponse;
                                    if (null != httWebResponse)
                                    {
                                        if (HttpStatusCode.NotModified == httWebResponse.StatusCode)
                                        {
                                            this.NotModified = true;
                                        }
                                    }
                                }

                                this.ErrorInfo = "下载 " + this.Url + " 过程发生错误: " + ExceptionUtil.GetExceptionMessage(this.e.Error);
                            }
                            else
                            {
                                File.Copy(this.TempFielname, this.OutputFilename, true);
                                try
                                {
                                    File.Delete(this.TempFielname);
                                }
                                catch
                                {
                                }
                            }
                        }

                        this.Close();
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.e = e;
            if (this.eventComplete != null)
                this.eventComplete.Set();
        }



        private void button_cancel_Click(object sender, EventArgs e)
        {
            if (webClient != null)
                webClient.CancelAsync();

            eventComplete.Set();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void WebFileDownloadDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            eventComplete.Set();
        }

        public ProgressBar ProgressBar
        {
            get
            {
                return this.progressBar1;
            }
        }

        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        // 包装后的版本
        public static int DownloadWebFile(
    IWin32Window owner,
    string strUrl,
    string strLocalFileName,
    string strTempFilename,
    out string strError)
        {
            string strIfModifySince = "";
            string strLastModified = "";
            return DownloadWebFile(owner,
                strUrl,
                strLocalFileName,
                strTempFilename,
                strIfModifySince,
                out strLastModified,
                out strError);
        }

        // parameters:
        //      strLastModified 最后修改时间。RFC1123格式。只下载这个时间以后的文件。如果为空，表示不限制时间
        // return:
        //      -1  出错
        //      0   没有更新
        //      1   已经下载
        public static int DownloadWebFile(
            IWin32Window owner,
            string strUrl,
            string strLocalFileName,
            string strTempFilename,
            string strIfModifySince,
            out string strLastModified,
            out string strError)
        {
            strError = "";
            strLastModified = "";

            WebFileDownloadDialog dlg = new WebFileDownloadDialog();
            if (GuiUtil.GetDefaultFont() != null)
                dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Url = strUrl;
            dlg.OutputFilename = strLocalFileName;
            dlg.TempFielname = strTempFilename;
            dlg.IfModifySince = strIfModifySince;
            dlg.MessageText = "正在下载Web文件 " + strUrl;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(owner);

            if (dlg.NotModified == true)
                return 0;

            if (string.IsNullOrEmpty(dlg.ErrorInfo) == false)
            {
                strError = dlg.ErrorInfo;
                return -1;
            }

            strLastModified = dlg.LastModified;

            return 1;
        }




    }

    public class MyWebClient : WebClient
    {
        public string IfModifiedSince = "";
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            if (String.IsNullOrEmpty(this.IfModifiedSince) == false)
                request.IfModifiedSince = DateTimeUtil.FromRfc1123DateTimeString(this.IfModifiedSince);
            else
                request.IfModifiedSince = DateTime.MinValue;

            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

            return request;
        }

    }
}
