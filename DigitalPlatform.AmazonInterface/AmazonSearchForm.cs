using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;

namespace DigitalPlatform.AmazonInterface
{
    public partial class AmazonSearchForm : Form
    {
        FloatingMessageForm _floatingMessage = null;

        public bool FloatingMode = false;

        public string TempFileDir = "";
        public string ServerUrl = "webservices.amazon.cn";

        public int RetryCount = 4;

        public bool AutoSearch = false;

        public AmazonSearchForm()
        {
            InitializeComponent();
            this.CreateBrowseColumns();
        }

        Font _idFont = null;
        Font _titleFont = null;

        private void AmazonSearchForm_Load(object sender, EventArgs e)
        {
            FillFrom();

            this._idFont = new System.Drawing.Font(this.Font.Name, this.Font.Size * 2, FontStyle.Bold);
            this._titleFont = new System.Drawing.Font(this.Font.Name, this.Font.Size * (float)1.2, FontStyle.Bold);

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.AutoHide = false;
                _floatingMessage.Show(this);
                this._floatingMessage.OnResizeOrMove();

            }

            BeginThread();

            if (this.AutoSearch == true
    && string.IsNullOrEmpty(this.textBox_queryWord.Text) == false)
            {
                this.BeginInvoke(new Action<object, EventArgs>(button_search_Click), this, new EventArgs());
            }
        }

        private void AmazonSearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.eventClose.Set();
        }

        private void AmazonSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_floatingMessage != null)
                _floatingMessage.Close();

            StopThread();
            DeleteTempFiles();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.dpTable_items);
                controls.Add(this.comboBox_from);
                controls.Add(this.textBox_queryWord);

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.dpTable_items);
                controls.Add(this.comboBox_from);
                controls.Add(this.textBox_queryWord);
                GuiState.SetUiState(controls, value);
            }
        }

        public void SetFloatMessage(string strColor,
string strText)
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string, string>(SetFloatMessage), strColor, strText);
                return;
            }

            if (strColor == "waiting")
                this._floatingMessage.RectColor = Color.FromArgb(80, 80, 80);
            else
                this._floatingMessage.RectColor = Color.Purple;

            this._floatingMessage.Text = strText;
        }

        public string QueryWord
        {
            get
            {
                return this.textBox_queryWord.Text;
            }
            set
            {
                this.textBox_queryWord.Text = value;
            }
        }

        public string From
        {
            get
            {
                return this.comboBox_from.Text;
            }
            set
            {
                this.comboBox_from.Text = value;
            }
        }

        static string[] froms = new string[] {
            "题名\ttitle",
            "著者\tauthor",
            "出版者\tpublisher",
            "出版日期\tpubdate",
            "主题词\tsubject",
            "关键词\tkeywords",
            "语言\tlanguage",
            "装订\tbinding",
            "ISBN\tISBN",
            "EISBN\tEISBN",
            "ASIN\tASIN"};
        void FillFrom()
        {
            this.comboBox_from.Items.Clear();
            foreach (string s in froms)
            {
                this.comboBox_from.Items.Add(s);
            }
        }

        public string GetFromRight()
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(this.comboBox_from.Text,
                "\t",
                out strLeft,
                out strRight);
            if (string.IsNullOrEmpty(strRight) == false)
                return strRight;

            // 从 Items 中寻找
            string strText = GetLineText(this.comboBox_from, strLeft);
            if (strText == null)
                return null;

            StringUtil.ParseTwoPart(strText,
    "\t",
    out strLeft,
    out strRight);
            if (string.IsNullOrEmpty(strRight) == false)
                return strRight;
            return strRight;
        }

        // 根据左侧文字匹配 Items 中整行文字，如果有匹配的行，返回整行文字
        static string GetLineText(TabComboBox combobox,
            string strLeft)
        {
            foreach (string s in combobox.Items)
            {
                if (StringUtil.HasHead(s, strLeft + "\t") == true)
                    return s;
            }

            return null;
        }

        const int COLUMN_IMAGE = 1; // 2

        void ClearList()
        {
#if NO
            // 将 Image 对象释放
            foreach (DpRow row in this.dpTable_items.Rows)
            {
                Image image = row[COLUMN_IMAGE].Image;
                row[COLUMN_IMAGE].Image = null;
                if (image != null)
                    image.Dispose();
            }
#endif
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

            this.dpTable_items.Rows.Clear();
            DeleteTempFiles();
        }

        AmazonSearch _search = null;

        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.ClearList();

            // line.BiblioSummary = "正在针对 " + account.ServerName + " \r\n检索 " + line.BiblioBarcode + " ...";

            this.button_stop.Enabled = true;
            this.button_search.Enabled = false;
            try
            {
                string strFrom = this.GetFromRight();

#if NO
                if (_search != null)
                    _search.Dispose();

                _search = new AmazonSearch();

                // search.MainForm = this.MainForm;
                _search.TempFileDir = this.TempFileDir;
                _search.Idle -= new EventHandler(search_Idle);
                _search.Idle += new EventHandler(search_Idle);
#endif

                if (_search == null)
                {
                    _search = new AmazonSearch();
                    _search.TempFileDir = this.TempFileDir;
                    _search.Idle -= new EventHandler(search_Idle);
                    _search.Idle += new EventHandler(search_Idle);
                }


                // 多行检索中的一行检索
                int nRedoCount = 0;
            REDO:
                int nRet = _search.Search(
                    this.ServerUrl,
                    this.textBox_queryWord.Text.Replace("-", ""),
                    strFrom,    // "ISBN",
                    "[default]",
                    true,
                    out strError);
                if (nRet == -1)
                {
                    if (_search.Exception != null && _search.Exception is WebException)
                    {
                        WebException e1 = _search.Exception as WebException;
                        if (e1.Status == WebExceptionStatus.ProtocolError)
                        {
                            // 重做
                            if (nRedoCount < this.RetryCount)
                            {
                                nRedoCount++;
                                _search.Delay();
                                goto REDO;
                            }

#if NO
                        // 询问是否重做
                        DialogResult result = MessageBox.Show(this,
"检索 '" + strLine + "' 时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 跳过这一行继续检索后面的行； Cancel: 中断整个检索操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Retry)
                        {
                                _search.Delay(true);
                            goto REDO;
                        }
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        goto CONTINUE;
#endif
                            strError = "nRedoCount ["+nRedoCount.ToString()+"] " + strError;
                            goto ERROR1;
                        }
                    }
                    goto ERROR1;
                }

                nRet = _search.LoadBrowseLines(appendBrowseLine,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (_search.HasNextBatch() == true)
                {
                    AddNextCmdLine();
                }
            }
            finally
            {
                this.button_stop.Enabled = false;
                this.button_search.Enabled = true;
            }

            if (_search.HitCount == 0)
            {
                MessageBox.Show(this, "没有命中");
            } 
            return;

        ERROR1:
            strError = "针对服务器 '" + this.ServerUrl + "' 检索出错: " + strError;
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 检索
        /// </summary>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1表示出错，其他命中记录的数目</returns>
        public int DoSearch(out string strError)
        {
            strError = "";

            this.ClearList();

            // line.BiblioSummary = "正在针对 " + account.ServerName + " \r\n检索 " + line.BiblioBarcode + " ...";

            this.button_stop.Enabled = true;
            this.button_search.Enabled = false;
            try
            {
                string strFrom = this.GetFromRight();

#if NO
                if (_search != null)
                    _search.Dispose();

                _search = new AmazonSearch();
                // search.MainForm = this.MainForm;
                _search.TempFileDir = this.TempFileDir;
                _search.Idle -= new EventHandler(search_Idle);
                _search.Idle += new EventHandler(search_Idle);
#endif
                if (_search == null)
                {
                    _search = new AmazonSearch();
                    _search.TempFileDir = this.TempFileDir;
                    _search.Idle -= new EventHandler(search_Idle);
                    _search.Idle += new EventHandler(search_Idle);
                }


                // 多行检索中的一行检索
                int nRedoCount = 0;
            REDO:
                int nRet = _search.Search(
                    this.ServerUrl,
                    this.textBox_queryWord.Text.Replace("-", ""),
                    strFrom,    // "ISBN",
                    "[default]",
                    true,
                    out strError);
                if (nRet == -1)
                {
                    if (_search.Exception != null && _search.Exception is WebException)
                    {
                        WebException e1 = _search.Exception as WebException;
                        if (e1.Status == WebExceptionStatus.ProtocolError)
                        {
                            // 重做
                            if (nRedoCount < this.RetryCount)
                            {
                                nRedoCount++;
                                _search.Delay();
                                goto REDO;
                            }

#if NO
                        // 询问是否重做
                        DialogResult result = MessageBox.Show(this,
"检索 '" + strLine + "' 时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 跳过这一行继续检索后面的行； Cancel: 中断整个检索操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Retry)
                        {
                                _search.Delay();
                            goto REDO;
                        }
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        goto CONTINUE;
#endif
                            strError = "nRedoCount [" + nRedoCount.ToString() + "] " + strError;
                            goto ERROR1;
                        }
                    }
                    goto ERROR1;
                }

                nRet = _search.LoadBrowseLines(appendBrowseLine,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 如果有命中的，自动选定第一行
                if (dpTable_items.Rows.Count > 0)
                    dpTable_items.Rows[0].Select(SelectAction.On);

                if (_search.HasNextBatch() == true)
                {
                    AddNextCmdLine();
                }
            }
            finally
            {
                this.button_stop.Enabled = false;
                this.button_search.Enabled = true;
            }


            return _search.HitCount;
        ERROR1:
            strError = "针对服务器 '" + this.ServerUrl + "' 检索出错: " + strError;
            return -1;
        }

        void AddNextCmdLine()
        {
            DpRow row = new DpRow();

            DpCell cell = new DpCell();
            cell.Text = "";
            row.Add(cell);

            cell = new DpCell();
            cell.Text = "";
            row.Add(cell);

            cell = new DpCell();
            cell.Text = "共命中 "+_search.HitCount.ToString()+" 条。双击调入下一批命中记录 ...";
            row.Add(cell);

            this.dpTable_items.Rows.Add(row);
        }

        void search_Idle(object sender, EventArgs e)
        {
            Application.DoEvents();

            // TODO: 检测中断
        }

        // 获得图像 URL
        //             
        // parameters:
        // return:
        //      name(string) --> ImageInfo 对照表
        //                  name = "SmallImage",  "MediumImage", "LargeImage"};
        public Hashtable GetImageUrls(string strXml)
        {
            Hashtable table = null;
            string strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", AmazonSearch.NAMESPACE);

            int nRet = AmazonSearch.GetImageUrl(dom.DocumentElement,
                nsmgr,
                out table,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return table;
        ERROR1:
            throw new Exception(strError);
        }

        // 针对亚马逊服务器检索，装入一个浏览行的回调函数
        int appendBrowseLine(string strRecPath,
    string strRecord,
    object param,
    out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strRecord);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("amazon", AmazonSearch.NAMESPACE);

            List<string> cols = null;
            string strASIN = "";
            string strCoverImageUrl = "";
            int nRet = AmazonSearch.ParseItemXml(dom.DocumentElement,
                nsmgr,
                out strASIN,
                out strCoverImageUrl,
                out cols,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            string strMARC = "";
            // 将亚马逊 XML 格式转换为 UNIMARC 格式
            nRet = AmazonSearch.AmazonXmlToUNIMARC(dom.DocumentElement,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;
#endif

            AmazonBiblioInfo info = new AmazonBiblioInfo();
            info.Xml = dom.DocumentElement.OuterXml;
            info.Timestamp = null;
            info.ASIN = strASIN + "@" + this.ServerUrl;
            info.MarcSyntax = "amazon"; // 表示 XML 尚未转换
            info.ImageUrl = strCoverImageUrl;
            this.AddBiblioBrowseLine(
                info.ASIN,
                StringUtil.MakePathList(cols, "\t"),
                info);

            return 0;
        }

        // 加入一个浏览行
        public void AddBiblioBrowseLine(
            string strBiblioRecPath,
            string strBrowseText,
            AmazonBiblioInfo info)
        {
            if (this.dpTable_items.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string, string, AmazonBiblioInfo>(AddBiblioBrowseLine),
                    strBiblioRecPath,
                    strBrowseText,
                    info);
                return;
            }

            // 检查已经存在的最后一行是否为命令行
            if (this.dpTable_items.Rows.Count > 0)
            {
                DpRow tail = this.dpTable_items.Rows[this.dpTable_items.Rows.Count - 1];
                if (string.IsNullOrEmpty(tail[0].Text) == true)
                    this.dpTable_items.Rows.RemoveAt(this.dpTable_items.Rows.Count - 1);
            }

            List<string> columns = StringUtil.SplitList(strBrowseText, '\t');
            DpRow row = new DpRow();
            row.LineAlignment = StringAlignment.Center;

            DpCell cell = new DpCell();
            cell.Text = (this.dpTable_items.Rows.Count + 1).ToString();
            cell.Font = this._idFont;
            cell.ForeColor = SystemColors.GrayText;
            row.Add(cell);

#if NO
            cell = new DpCell();
            cell.Text = strBiblioRecPath;
            row.Add(cell);
#endif

            cell = new DpCell();
            // cell.Text = info.ImageUrl;
            // cell.Text = "正在加载图片 ...";
            row.Add(cell);

            int i = 0;
            foreach (string s in columns)
            {
                cell = new DpCell();
                cell.Text = s;
                if (i == 0)
                    cell.Font = this._titleFont;

                row.Add(cell);
                i++;
            }

            row.Tag = info;
            // row[COLUMN_IMAGE].OwnerDraw = true;

            this.dpTable_items.Rows.Add(row);

#if NO
            if (string.IsNullOrEmpty(info.ImageUrl) == false)
            {
                TraceImage trace = new TraceImage();
                trace.ImageUrl = info.ImageUrl;
                trace.Row = row;
                lock (_trace_images)
                {
                    _trace_images.Add(trace);
                }
            }
#endif
            AddTraceItem(row);
        }

        void AddTraceItem(DpRow row)
        {
            AmazonBiblioInfo info = row.Tag as AmazonBiblioInfo;
            if (info != null && string.IsNullOrEmpty(info.ImageUrl) == false)
            {
                TraceImage trace = new TraceImage();
                trace.ImageUrl = info.ImageUrl;
                trace.Row = row;
                lock (_trace_images)
                {
                    _trace_images.Add(trace);
                }
            }
        }

        // 创建浏览栏目标题
        void CreateBrowseColumns()
        {
            if (this.dpTable_items.Columns.Count > 1)
                return;

            List<string> columns = new List<string>() { "封面", "书名", "作者", "出版者", "出版日期" };
            foreach (string s in columns)
            {
                DpColumn column = new DpColumn();
                column.Text = s;
                column.Width = 120;
                this.dpTable_items.Columns.Add(column);
            }
        }

        ImageThread _fillImageThread = null;

        public void BeginThread()
        {
            if (this._fillImageThread == null)
            {
                this._fillImageThread = new ImageThread();
                this._fillImageThread.Container = this;
                this._fillImageThread.BeginThread();
            }
        }

        public void ActivateThread()
        {
            if (this._fillImageThread != null)
                this._fillImageThread.Activate();

        }

        public void StopThread()
        {
            this._fillImageThread.StopThread(true);
            this._fillImageThread = null;
        }

        class ImageThread : ThreadBase
        {
            internal ReaderWriterLock m_lock = new ReaderWriterLock();
            internal static int m_nLockTimeout = 5000;	// 5000=5秒

            public AmazonSearchForm Container = null;

            public void LockForWrite()
            {
                this.m_lock.AcquireWriterLock(m_nLockTimeout);
            }

            public void UnlockForWrite()
            {
                this.m_lock.ReleaseWriterLock();
            }

            // 工作线程每一轮循环的实质性工作
            public override void Worker()
            {
                this.m_lock.AcquireWriterLock(m_nLockTimeout);
                try
                {
                    if (this.Stopped == true)
                        return;

                    this.Container.DownloadImages();

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

        }

        // 存储图像和行的对应关系，好在图像获取完成后，加入到显示
        List<TraceImage> _trace_images = new List<TraceImage>();

        MyWebClient _webClient = null;

        // 下载图像文件。因为在线程中使用，所以使用同步的版本就行
        void DownloadImages()
        {
            TraceImage info = null;
            lock (_trace_images)
            {
                if (_trace_images.Count == 0)
                    return;

                info = _trace_images[0];
                _trace_images.RemoveAt(0);
            }

            info.FileName = GetTempFileName();

            string strError = "";
            if (_webClient == null)
            {
                _webClient = new MyWebClient();
                _webClient.Timeout = 5000;
                _webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
            }

            try
            {
                _webClient.DownloadFile(new Uri(info.ImageUrl, UriKind.Absolute), info.FileName);
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
                            SetErrorText(info, strError);
                            goto END1;
                        }
                    }
                }

                strError = ex.Message;
                SetErrorText(info, strError);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                SetErrorText(info, strError);
#if NO
                info.Row[COLUMN_IMAGE].Text = strError;
                info.Row[COLUMN_IMAGE].OwnerDraw = false;
#endif
            }

            if (this.IsDisposed == true || this.Visible == false
    || info == null)
                return;

            // _info.Row[COLUMN_IMAGE].Image = Image.FromFile(_info.FileName);
            SetImage(info.Row[COLUMN_IMAGE], info.FileName);
            END1:
            this.ActivateThread();
        }

        void SetErrorText(TraceImage info, string strError)
        {
            if (this.dpTable_items.InvokeRequired)
            {
                this.dpTable_items.Invoke(new Action<TraceImage, string>(SetErrorText), info, strError);
                return;
            }

            info.Row[COLUMN_IMAGE].Text = strError;
            info.Row[COLUMN_IMAGE].OwnerDraw = false;
        }

#if NO
        TraceImage _info = null;    // 事件之间传递信息
        void DownloadImages()
        {
            if (_trace_images.Count == 0)
                return;

            _info = _trace_images[0];
            _trace_images.RemoveAt(0);

            _info.FileName = GetTempFileName();

            string strError = "";
            if (_webClient == null)
                _webClient = new WebClient();

            _webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_downloadFileCompleted);
            try
            {
                _webClient.DownloadFileAsync(new Uri(_info.ImageUrl, UriKind.Absolute),
                    _info.FileName, null);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                _info.Row[COLUMN_IMAGE].Text = strError;
            }
        }

                // AsyncCompletedEventArgs _end_e = null;

        void webClient_downloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // _end_e = e;
            if (this.IsDisposed == true || this.Visible == false
                || _info == null)
                return;

            if (e.Cancelled == true)
            {
                _info.Row[COLUMN_IMAGE].OwnerDraw = false;
                _info.Row[COLUMN_IMAGE].Text = "cancelled";
            }
            else if (e.Error != null)
            {
                _info.Row[COLUMN_IMAGE].OwnerDraw = false;
                _info.Row[COLUMN_IMAGE].Text = e.Error.Message;
            }
            else
            {
                // _info.Row[COLUMN_IMAGE].Image = Image.FromFile(_info.FileName);
                SetImage(_info.Row[COLUMN_IMAGE], _info.FileName);
            }

            // _info = null;
            this.ActivateThread();
        }

#endif

#if NO
        string _tempFileName = "";

        // 准备临时文件名
        string GetTempFileName()
        {
            // 如果以前有临时文件名，就直接沿用
            if (string.IsNullOrEmpty(this._tempFileName) == true)
            {
                // this.TempFilename = Path.Combine(this.MainForm.DataDir, "~webclient_response_" + Guid.NewGuid().ToString());
                Debug.Assert(string.IsNullOrEmpty(this.TempFileDir) == false, "");
                this._tempFileName = Path.Combine(this.TempFileDir, "~image_" + Guid.NewGuid().ToString());
            }

            try
            {
                File.Delete(this._tempFileName);
            }
            catch
            {
            }

            return _tempFileName;
        }
#endif
        List<string> _tempFileNames = new List<string>();

        // 准备临时文件名
        string GetTempFileName()
        {
            Debug.Assert(string.IsNullOrEmpty(this.TempFileDir) == false, "");
            string strTempFileName = Path.Combine(this.TempFileDir, "~image_" + Guid.NewGuid().ToString());
            _tempFileNames.Add(strTempFileName);
            return strTempFileName;
        }

        // 删除全部临时文件
        void DeleteTempFiles()
        {
            List<string> undeleted = new List<string>();
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
                        undeleted.Add(strFileName);
                    }
                }
            }

            this._tempFileNames.Clear();

            // 残留的文件也许后面就有机会删除成功了
            if (undeleted.Count > 0)
                this._tempFileNames = undeleted;
        }


#if NO
        void SetImage(DpCell cell, string strFileName)
        {
            if (this.dpTable_items.InvokeRequired)
            {
                this.Invoke(new Action<DpCell, string>(SetImage), cell, strFileName);
                return;
            }

            Image image = Image.FromFile(strFileName);
            cell.Image = image;
            cell.Text = "";
        }
#endif

        void SetImage(DpCell cell, string strFileName)
        {
            if (this.dpTable_items.InvokeRequired)
            {
                this.Invoke(new Action<DpCell, string>(SetImage), cell, strFileName);
                return;
            }
            cell.OwnerDraw = true;
            ImageCellInfo info = new ImageCellInfo();
            info.FileName = strFileName;
            cell.Tag = info;
            cell.Text = ""; // 迫使刷新
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            if (_search != null)
                _search.CancelSearch();
        }

        private void dpTable_items_PaintRegion(object sender, PaintRegionArgs e)
        {
            if (e.Action == "query")
            {
                // 测算图像高度
                DpCell cell = e.Item as DpCell;
                // DpRow row = cell.Container;
                ImageCellInfo cell_info = (ImageCellInfo)cell.Tag; 
                // string strFileName = (string)cell.Tag;
                if (cell_info == null || string.IsNullOrEmpty(cell_info.FileName) == true)
                    e.Height = 0;
                else
                {
                    try
                    {
                        using (Stream s = File.Open(cell_info.FileName, FileMode.Open))
                        {
                            Image image = Image.FromStream(s);
                            e.Height = image.Height;
                            image.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        // throw new Exception("read image error:" + ex.Message);
                        if (cell_info.RetryCount < 5)
                        {
                            cell_info.RetryCount++;
                            DpRow row = cell.Container;
                            AddTraceItem(row);
                        }
                        else
                        {

                            e.Height = 0;
                            cell.OwnerDraw = false;
                            cell.Text = "read image error";
                        }
                        return;
                    }

                    if (this.dpTable_items.MaxTextHeight < e.Height)
                        this.dpTable_items.MaxTextHeight = e.Height;
                }
                return;
            }

            {
                Debug.Assert(e.Action == "paint", "");

                DpCell cell = e.Item as DpCell;

                ImageCellInfo cell_info = (ImageCellInfo)cell.Tag;
                // string strFileName = (string)cell.Tag;
                if (cell_info != null && string.IsNullOrEmpty(cell_info.FileName) == false)
                {
                    try
                    {
                        using (Stream s = File.Open(cell_info.FileName, FileMode.Open))
                        {
                            Image image = Image.FromStream(s);

                            // 绘制图像
                            e.pe.Graphics.DrawImage(image,
                                (float)e.X,
                                (float)e.Y);

                            image.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        // throw new Exception("read image error:" + ex.Message);
                        if (cell_info.RetryCount < 5)
                        {
                            cell_info.RetryCount++;
                            DpRow row = cell.Container;
                            AddTraceItem(row);
                        }
                        else
                        {
                            cell.OwnerDraw = false;
                            cell.Text = "read image error";
                        }
                    }
                }
                else
                {
                    // 绘制文字“正在加载”
                }

            }
        }

        class ImageCellInfo
        {
            public string FileName = "";
            public int RetryCount = 0;
        }

        private void dpTable_items_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedRecord();
        }

        void OpenSelectedRecord()
        {
            if (this.dpTable_items.SelectedRows.Count == 1)
            {
                DpRow row = this.dpTable_items.SelectedRows[0];
                if (string.IsNullOrEmpty(row[0].Text) == true)
                {
                    LoadNextBatch();
                }
                else
                {
                    button_OK_Click(this, new EventArgs());
                }
            }
        }

        AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        AutoResetEvent eventSelected = new AutoResetEvent(false);	// 


        // 等待选择一个事项
        // return:
        //      true    已经选择
        //      false   点了“取消”
        public bool WaitSelect()
        {
            WaitHandle[] events = new WaitHandle[2];

            events[0] = eventClose;
            events[1] = eventSelected;

            while (true)
            {
                if (this.IsDisposed == true)
                    break;

                Application.DoEvents();

                int index = 0;
                try
                {
                    index = WaitHandle.WaitAny(events, 100, false);
                }
                catch (System.Threading.ThreadAbortException /*ex*/)
                {
                    return false;
                }

                if (index == WaitHandle.WaitTimeout)
                {
                    // 超时
                    continue;
                }
                else if (index == 0)
                {
                    return false;
                }
                else
                {
                    // 得到激活信号
                    eventSelected.Reset();
                    return true;
                }
            }

            return false;
        }

        void LoadNextBatch()
        {
            string strError = "";

            this.button_stop.Enabled = true;
            this.button_search.Enabled = false;
            try
            {
                int nRedoCount = 0;
            REDO:
                int nRet = _search.NextBatch(this.ServerUrl,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == -1)
                {
                    if (_search.Exception != null && _search.Exception is WebException)
                    {
                        WebException e1 = _search.Exception as WebException;
                        if (e1.Status == WebExceptionStatus.ProtocolError)
                        {
                            // 重做
                            if (nRedoCount < this.RetryCount)
                            {
                                nRedoCount++;
                                _search.Delay();
                                goto REDO;
                            }

#if NO
                        // 询问是否重做
                        DialogResult result = MessageBox.Show(this,
"检索 '" + strLine + "' 时发生错误:\r\n\r\n" + strError + "\r\n\r\n是否重试?\r\n\r\n(Yes: 重试; No: 跳过这一行继续检索后面的行； Cancel: 中断整个检索操作",
"AmazonSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Retry)
                        {
                                _search.Delay();
                            goto REDO;
                        }
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        goto CONTINUE;
#endif
                            goto ERROR1;
                        }
                    }
                    goto ERROR1;
                }


                nRet = _search.LoadBrowseLines(appendBrowseLine,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (_search.HasNextBatch() == true)
                {
                    AddNextCmdLine();
                }
            }
            finally
            {
                this.button_stop.Enabled = false;
                this.button_search.Enabled = true;
            }
            return;
        ERROR1:
            strError = "针对服务器 '" + this.ServerUrl + "' 检索出错: " + strError;
            MessageBox.Show(this, strError);
        }

        private void dpTable_items_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("打开");
            menuItem.Click += new System.EventHandler(this.menu_openSelectedRecord_Click);
            if (this.dpTable_items.SelectedRows.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("装入下一批记录(&N)");
            menuItem.Click += new System.EventHandler(this.menu_nextBatch_Click);
            if (_search == null || _search.HasNextBatch() == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.dpTable_items, new Point(e.X, e.Y));		
        }

        void menu_nextBatch_Click(object sender, EventArgs e)
        {
            LoadNextBatch();
        }

        void menu_openSelectedRecord_Click(object sender, EventArgs e)
        {
            OpenSelectedRecord();
        }

        private void dpTable_items_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dpTable_items.SelectedRows.Count > 0)
                this.button_OK.Enabled = true;
            else
                this.button_OK.Enabled = false;

        }

        // 当前选择的行
        public AmazonBiblioInfo SelectedItem
        {
            get
            {
                if (this.dpTable_items.SelectedRows.Count == 0)
                    return null;
                return this.dpTable_items.SelectedRows[0].Tag as AmazonBiblioInfo;
            }
        }


        private void button_Cancel_Click(object sender, EventArgs e)
        {
            if (this.FloatingMode == false)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
            }
            else
            {
                // this.WindowState = FormWindowState.Minimized;
                this.eventClose.Set();
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.dpTable_items.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "请选择一个书目事项");
                return;
            }
            if (this.FloatingMode == false)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            else
            {
                this.eventSelected.Set();
            }
        }

        private void dpTable_items_Click(object sender, EventArgs e)
        {
            SetFloatMessage("", "");    // 清除浮动显示
        }
    }

    /// <summary>
    /// 在内存中缓存一条书目信息。能够表示新旧记录的修改关系
    /// </summary>
    public class AmazonBiblioInfo
    {
        /// <summary>
        /// 记录路径
        /// </summary>
        public string ASIN = "";
        /// <summary>
        /// 记录 XML
        /// </summary>
        public string Xml = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        /// <summary>
        /// MARC 格式类型
        /// </summary>
        public string MarcSyntax = "";

        /// <summary>
        /// 封面图像 URL
        /// </summary>
        public string ImageUrl = "";
    }

    // 追踪图像和行对象的关系
    class TraceImage
    {
        public string ImageUrl = "";    // 图像 URL
        public DpRow Row = null;    // 行
        public string FileName = "";    // 存储下载图像内容的临时文件名
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
}
