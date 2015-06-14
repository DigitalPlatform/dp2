using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using System.IO;
using System.Threading;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using System.Diagnostics;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.Script;

namespace dp2Circulation
{
    /// <summary>
    /// 快捷出纳窗
    /// </summary>
    public partial class QuickChargingForm : MyForm, IProtectFocus, IChargingForm
    {
        /// <summary>
        /// 借书、还书完成的事件
        /// </summary>
        public event BorrowCompleteEventHandler BorrowComplete = null;

        /// <summary>
        /// IProtectFocus 接口要求的函数
        /// </summary>
        /// <param name="pfAllow">是否允许</param>
        public void AllowFocusChange(ref bool pfAllow)
        {
            pfAllow = false;
        }

        Commander commander = null;

        const int WM_LOAD_READER = API.WM_USER + 300;
        const int WM_LOAD_ITEM = API.WM_USER + 301;

        WebExternalHost m_webExternalHost_readerInfo = new WebExternalHost();

        TaskList _taskList = new TaskList();

        internal ExternalChannel _summaryChannel = new ExternalChannel();
        internal ExternalChannel _barcodeChannel = new ExternalChannel();

        FloatingMessageForm _floatingMessage = null;

        PatronCardStyle _cardStyle = new PatronCardStyle();

        /// <summary>
        /// 构造函数
        /// </summary>
        public QuickChargingForm()
        {
            InitializeComponent();

            this.dpTable_tasks.ImageList = this.imageList_progress;

#if NO
            // 黑色调
            this.dpTable_tasks.BackColor = Color.Black;
            this.dpTable_tasks.ForeColor = Color.LightGray;
            this.dpTable_tasks.Font = new Font(this.dpTable_tasks.Font, FontStyle.Bold);

            // 深蓝
            this.dpTable_tasks.BackColor = Color.DarkBlue;
            this.dpTable_tasks.ForeColor = Color.LightGray;
            this.dpTable_tasks.Font = new Font(this.dpTable_tasks.Font, FontStyle.Bold);
#endif

            _cardStyle.PhtoMaxWidth = 50;

            this.webBrowser_reader.ScriptErrorsSuppressed = true;
        }

        PatronCardControl _cardControl = null;

        private void QuickChargingForm_Load(object sender, EventArgs e)
        {
            this.Channel.Idle += new IdleEventHandler(Channel_Idle);

            if (this.DisplayFormat == "卡片")
            {
                _cardControl = new PatronCardControl();
                _cardControl.Dock = DockStyle.Fill;

                this.splitContainer_main.Panel1.Controls.Remove(this.webBrowser_reader);
                this.splitContainer_main.Panel1.Controls.Add(_cardControl);
            }

            // webbrowser
            this.m_webExternalHost_readerInfo.Initial(this.MainForm, this.webBrowser_reader);
            this.m_webExternalHost_readerInfo.OutputDebugInfo += new OutputDebugInfoEventHandler(m_webExternalHost_readerInfo_OutputDebugInfo);
            // this.m_webExternalHost_readerInfo.WebBrowser = this.webBrowser_reader;  //
            this.webBrowser_reader.ObjectForScripting = this.m_webExternalHost_readerInfo;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            this._summaryChannel.Initial(this.MainForm);
            this._barcodeChannel.Initial(this.MainForm);

            if (this.DisplayFormat == "HTML")
            {
                SetReaderHtmlString("(空)");
            }

            this.FuncState = this.FuncState;

            this._taskList.Channel = this.Channel;
            this._taskList.stop = this.stop;
            this._taskList.Container = this;
            this._taskList.BeginThread();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                // _floatingMessage.TopMost = true;
                // _floatingMessage.Text = "正在处理，请不要让读者离开 ...";
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.Show(this);
            }

            this.MainForm.Move += new EventHandler(MainForm_Move);

            this.toolStripButton_enableHanzi.Checked = this.MainForm.AppInfo.GetBoolean(
                "quickchargingform",
                "eanble_hanzi",
                false);
            this.toolStripButton_upperInput.Checked = this.MainForm.AppInfo.GetBoolean(
                "quickchargingform",
                "upper_input",
                true);
        }

        void m_webExternalHost_readerInfo_OutputDebugInfo(object sender, OutputDebugInfoEventArgs e)
        {
            if (_floatingMessage != null)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string>(AppendFloatingMessage), "\r\n" + e.Text);
            }
        }

        void AppendFloatingMessage(string strText)
        {
            this._floatingMessage.Text += strText;
        }

        void MainForm_Move(object sender, EventArgs e)
        {
            this._floatingMessage.OnResizeOrMove();
        }

        void Channel_Idle(object sender, IdleEventArgs e)
        {
            // 被专门的线程使用，因而不需要出让控制权
            e.bDoEvents = false;
        }

        private void QuickChargingForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void QuickChargingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null)
                this.MainForm.Move -= new EventHandler(MainForm_Move);

            this.commander.Destroy();

            if (this.m_webExternalHost_readerInfo != null)
            {
                this.m_webExternalHost_readerInfo.OutputDebugInfo -= new OutputDebugInfoEventHandler(m_webExternalHost_readerInfo_OutputDebugInfo);
                this.m_webExternalHost_readerInfo.Destroy();
            }

            this._taskList.Close();

            this._summaryChannel.Close();
            this._barcodeChannel.Close();

            if (_floatingMessage != null)
                _floatingMessage.Close();

            if (_patronSummaryForm != null)
                _patronSummaryForm.Close();

            this.Channel.Idle -= new IdleEventHandler(Channel_Idle);

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetBoolean(
                    "quickchargingform",
                    "eanble_hanzi",
                    this.toolStripButton_enableHanzi.Checked);
                this.MainForm.AppInfo.SetBoolean(
                    "quickchargingform",
                    "upper_input",
                    this.toolStripButton_upperInput.Checked);
            }
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost_readerInfo.ChannelInUse;
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            if (keyData == Keys.Enter) 
            {
                // MessageBox.Show(this, "test");
                AsyncDoAction(this.FuncState, GetUpperCase(this.textBox_input.Text));
                return true;
            }

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            // return false;
            return base.ProcessDialogKey(keyData);
        }

        delegate int Delegate_SelectOnePatron(long lRet,
            string strRecPath,
            out string strBarcode,
            out string strResult,
            out string strError);

        // return:
        //      -1  error
        //      0   放弃
        //      1   成功
        internal int SelectOnePatron(long lRet,
            string strRecPath,
            out string strBarcode,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";
            strBarcode = "";

            if (lRet <= 1)
            {
                strError = "没有必要调用 SelectMultiPatron()";
                return -1;
            }

            if (this.InvokeRequired)
            {
                Delegate_SelectOnePatron d = new Delegate_SelectOnePatron(SelectOnePatron);
                object[] args = new object[5];
                args[0] = lRet;
                args[1] = strRecPath;
                args[2] = strBarcode;
                args[3] = strResult;
                args[4] = strError;
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strBarcode = (string)args[2];
                strResult = (string)args[3];
                strError = (string)args[4];
                return result;
            }

            /*
            strError = "读者证条码号 '" + strBarcode + "' 命中 " + lRet.ToString() + " 条读者记录。这是一个严重错误，请系统管理员尽快排除。\r\n\r\n(当前窗口中显示的是其中的第一个记录)";
            goto ERROR1;
             * */
            SelectPatronDialog dlg = new SelectPatronDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.NoBorrowHistory = this.NoBorrowHistory;
            dlg.ColorBarVisible = false;
            dlg.MessageVisible = false;
            dlg.Overflow = StringUtil.SplitList(strRecPath).Count < lRet;
            int nRet = dlg.Initial(
                this.MainForm,
                this.Channel,
                this.stop,
                StringUtil.SplitList(strRecPath),
                "请选择一个读者记录",
                out strError);
            if (nRet == -1)
                return -1;
            // TODO: 保存窗口内的尺寸状态
            this.MainForm.AppInfo.LinkFormState(dlg, "QuickChargingForm_SelectPatronDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return 0;

            strBarcode = dlg.SelectedBarcode;
            strResult = dlg.SelectedHtml;

            return 1;
        }


        //
        delegate int Delegate_SelectOneItem(
            FuncState func,
            string strText,
    out string strItemBarcode,
    out string strError);

        // return:
        //      -1  error
        //      0   放弃
        //      1   成功
        internal int SelectOneItem(
            FuncState func,
            string strText,
            out string strItemBarcode,
            out string strError)
        {
            strError = "";
            strItemBarcode = "";

            if (this.InvokeRequired)
            {
                Delegate_SelectOneItem d = new Delegate_SelectOneItem(SelectOneItem);
                object[] args = new object[4];
                args[0] = func;
                args[1] = strText;
                args[2] = strItemBarcode;
                args[3] = strError;
                int result = (int)this.Invoke(d, args);

                // 取出out参数值
                strItemBarcode = (string)args[2];
                strError = (string)args[3];
                return result;
            }

            SelectItemDialog dlg = new SelectItemDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            if (func == dp2Circulation.FuncState.Borrow
                || func == dp2Circulation.FuncState.ContinueBorrow)
            {
                dlg.FunctionType = "borrow";
                dlg.Text = "请选择要借阅的册";
            }
            else if (func == dp2Circulation.FuncState.Renew)
            {
                dlg.FunctionType = "renew";
                dlg.Text = "请选择要续借的册";
            }
            else if (func == dp2Circulation.FuncState.VerifyRenew)
            {
                dlg.FunctionType = "renew";
                dlg.VerifyBorrower = this._taskList.CurrentReaderBarcode;
                dlg.Text = "请选择要(验证)续借的册";
            }
            else if (func == dp2Circulation.FuncState.Return || func == dp2Circulation.FuncState.Lost)
            {
                dlg.FunctionType = "return";
                dlg.Text = "请选择要还回的册";
            }
            else if (func == dp2Circulation.FuncState.VerifyReturn || func == dp2Circulation.FuncState.VerifyLost)
            {
                dlg.FunctionType = "return";
                dlg.VerifyBorrower = this._taskList.CurrentReaderBarcode;
                dlg.Text = "请选择要(验证)还回的册";
            }

            dlg.AutoOperSingleItem = this.AutoOperSingleItem;
            dlg.AutoSearch = true;
            dlg.MainForm = this.MainForm;
            dlg.From = "ISBN";
            dlg.QueryWord = strText;

            string strUiState = this.MainForm.AppInfo.GetString(
        "QuickChargingForm",
        "SelectItemDialog_uiState",
        "");
            dlg.UiState = strUiState;

            if (string.IsNullOrEmpty(strUiState) == false
                || this.MainForm.PanelFixedVisible == true)
                this.MainForm.AppInfo.LinkFormState(dlg, "QuickChargingForm_SelectItemDialog_state");
            else
            {
                dlg.Size = this.MainForm.panel_fixed.Size;
                dlg.StartPosition = FormStartPosition.Manual;
                dlg.Location = this.MainForm.PointToScreen(this.MainForm.panel_fixed.Location);
            }

            dlg.ShowDialog(this);

            if (string.IsNullOrEmpty(strUiState) == false
                || this.MainForm.PanelFixedVisible == true)
                this.MainForm.AppInfo.UnlinkFormState(dlg);

            this.MainForm.AppInfo.SetString(
"QuickChargingForm",
"SelectItemDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return 0;

            Debug.Assert(string.IsNullOrEmpty(dlg.SelectedItemBarcode) == false, "");
            strItemBarcode = dlg.SelectedItemBarcode;
            return 1;
        }

        delegate void Delegate_FillItemSummary(string strItemBarcode,
            string strConfirmItemRecPath,
            ChargingTask task);
        internal void AsynFillItemSummary(string strItemBarcode, 
            string strConfirmItemRecPath,
            ChargingTask task)
        {
            if (this.InvokeRequired)
            {
                Delegate_FillItemSummary d = new Delegate_FillItemSummary(AsynFillItemSummary);
                this.BeginInvoke(d, 
                    new object[] { 
                        strItemBarcode,
                        strConfirmItemRecPath,
                        task }
                    );
                return;
            }

            DpRow row = FindTaskLine(task);
            if (row == null)
                return;

            string strError = "";
            string strSummary = "";
            int nRet = GetBiblioSummary(strItemBarcode,
                strConfirmItemRecPath,
                out strSummary,
                out strError);
            if (nRet == -1)
                strSummary = strError;

            task.ItemSummary = strSummary;
            DisplayTask("refresh", task);

            if (this.SpeakBookTitle == true && nRet != -1
                && string.IsNullOrEmpty(strSummary) == false)
            {
                string strTitle = "";
                nRet = strSummary.IndexOf("/");
                if (nRet != -1)
                    strTitle = strSummary.Substring(0, nRet).Trim();
                else
                    strTitle = strSummary.Trim();

                this.MainForm.Speak(strTitle);
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_input.Enabled = bEnable;
        }

        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="strBarcode">要校验的条码号</param>
        /// <param name="strLibraryCodeList">馆代码列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-2  服务器没有配置校验方法，无法校验</para>
        /// <para>-1  出错</para>
        /// <para>0   不是合法的条码号</para>
        /// <para>1   是合法的读者证条码号</para>
        /// <para>2   是合法的册条码号</para>
        /// </returns>
        public override int VerifyBarcode(
            string strLibraryCodeList,
            string strBarcode,
            out string strError)
        {
            strError = "";

            // 2014/5/4
            if (StringUtil.HasHead(strBarcode, "PQR:") == true)
            {
                strError = "这是读者证号二维码";
                return 1;
            }

            this._barcodeChannel.PrepareSearch("正在验证条码号 " + strBarcode + "...");
            try
            {
                // TODO: 使用回调函数，以决定是否 disable textbox
                return this.MainForm.VerifyBarcode(
                    this._barcodeChannel.stop,
                    this._barcodeChannel.Channel,
                    strLibraryCodeList,
                    strBarcode,
                    EnableControls,
                    out strError);
            }
            finally
            {
                this._barcodeChannel.EndSearch();
            }
        }


        /// <summary>
        /// 获得书目摘要
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="strConfirmItemRecPath">用于确认的册记录路径。可以为空</param>
        /// <param name="strSummary">书目摘要</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError中；0: 没有找到; 1: 找到了</returns>
        public int GetBiblioSummary(string strItemBarcode,
    string strConfirmItemRecPath,
    out string strSummary,
    out string strError)
        {
            string strBiblioRecPath = "";
            int nRet = this.MainForm.GetCachedBiblioSummary(strItemBarcode,
strConfirmItemRecPath,
out strSummary,
out strError);
            if (nRet == -1 || nRet == 1)
                return nRet;

            Debug.Assert(nRet == 0, "");

            this._summaryChannel.PrepareSearch("正在获取书目摘要 ...");
            try
            {
                long lRet = this._summaryChannel.Channel.GetBiblioSummary(
                    this._summaryChannel.stop,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    null,
                    out strBiblioRecPath,
                    out strSummary,
                    out strError);
                if (lRet == -1)
                {
                    return -1;
                }
                else
                {
                    this.MainForm.SetBiblioSummaryCache(strItemBarcode,
                         strConfirmItemRecPath,
                         strSummary);
                }

                return (int)lRet;
            }
            finally
            {
                this._summaryChannel.EndSearch();
            }
        }

        public bool IsCardMode
        {
            get
            {
                if (_cardControl != null)
                    return true;
                return false;
            }
        }

        delegate void Delegate_SetReaderCardString(string strText);
        public void SetReaderCardString(string strText)
        {
            if (this.InvokeRequired)
            {
                Delegate_SetReaderCardString d = new Delegate_SetReaderCardString(SetReaderCardString);
                this.BeginInvoke(d, new object[] { strText });
                return;
            }

                if (string.IsNullOrEmpty(strText) == false
                    && strText[0] != '<')
                    _cardControl.Text = strText;
                else
                {
                    try
                    {
                        _cardControl.Xml = strText;
                    }
                    catch (Exception ex)
                    {
                        _cardControl.Text = ex.Message;
                    }
                }
        }

        delegate void Delegate_SetReaderHtmlString(string strHtml);
        /// <summary>
        /// 显示读者 HTML 字符串
        /// </summary>
        /// <param name="strHtml">HTML 字符串</param>
        public void SetReaderHtmlString(string strHtml)
        {
            if (this.InvokeRequired)
            {
                Delegate_SetReaderHtmlString d = new Delegate_SetReaderHtmlString(_setReaderHtmlString);
                this.BeginInvoke(d, new object[] { strHtml });
            }
            else
            {
                _setReaderHtmlString(strHtml);
            }
        }

        void _setReaderHtmlString(string strHtml)
        {
#if NO
            m_webExternalHost_readerInfo.StopPrevious();

            if (strText == "(空)")
            {
                Global.ClearHtmlPage(this.webBrowser_reader,
                    this.MainForm.DataDir);
                return;
            }

            Global.StopWebBrowser(this.webBrowser_reader);

            string strTempFilename = this.MainForm.DataDir + "\\~charging_temp_reader.html";
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strText);
            }

            int nRedoCount = 0;
        REDO:
            try
            {
                this.webBrowser_reader.Navigate(strTempFilename);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }
            }
#endif
            int nRedoCount = 0;
        REDO:
            try
            {
                if (strHtml == "(空)")
                    this.m_webExternalHost_readerInfo.ClearHtmlPage();
                else
                    this.m_webExternalHost_readerInfo.SetHtmlString(strHtml, "reader_html");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }
            }
        }

        delegate void Delegate_SetReaderTextString(string strText);
        /// <summary>
        /// 显示读者文本字符串
        /// </summary>
        /// <param name="strText">文本字符串</param>
        public void SetReaderTextString(string strText)
        {
            if (this.InvokeRequired)
            {
                Delegate_SetReaderTextString d = new Delegate_SetReaderTextString(SetReaderTextString);
                this.BeginInvoke(d, new object[] { strText });
                return;
            }

            int nRedoCount = 0;
        REDO:
            try
            {
                this.m_webExternalHost_readerInfo.SetTextString(strText, "reader_text" );
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }
            }
        }

        #region IChargingForm 接口相关

        // 2008/10/31 
        ChargingInfoHost m_chargingInfoHost = null;

        /// <summary>
        /// 获得 ChargingInfoHost 对象
        /// </summary>
        internal ChargingInfoHost CharingInfoHost
        {
            get
            {
                if (this.m_chargingInfoHost == null)
                {
                    m_chargingInfoHost = new ChargingInfoHost();
                    m_chargingInfoHost.ap = MainForm.AppInfo;
                    m_chargingInfoHost.window = this;
                    if (this.StopFillingWhenCloseInfoDlg == true)
                    {
                        m_chargingInfoHost.StopGettingSummary -= new EventHandler(m_chargingInfoHost_StopGettingSummary);
                        m_chargingInfoHost.StopGettingSummary += new EventHandler(m_chargingInfoHost_StopGettingSummary);
                    }
                }
                else
                {
                    if (this.StopFillingWhenCloseInfoDlg == false)
                    {
                        m_chargingInfoHost.StopGettingSummary -= new EventHandler(m_chargingInfoHost_StopGettingSummary);
                    }
                    else
                    {
                        m_chargingInfoHost.StopGettingSummary -= new EventHandler(m_chargingInfoHost_StopGettingSummary);
                        m_chargingInfoHost.StopGettingSummary += new EventHandler(m_chargingInfoHost_StopGettingSummary);
                    }
                }

                return m_chargingInfoHost;
            }
        }

        void m_chargingInfoHost_StopGettingSummary(object sender, EventArgs e)
        {
            if (this.m_webExternalHost_readerInfo != null)
                this.m_webExternalHost_readerInfo.StopPrevious();
            this.webBrowser_reader.Stop();
        }
        // 信息对话框的不透明度
        public double InfoDlgOpacity
        {
            get
            {
                return (double)this.MainForm.AppInfo.GetInt(
                    "charging_form",
                    "info_dlg_opacity",
                    100) / (double)100;
            }
        }

        /// <summary>
        /// 是否要在关闭信息对话框的时候自动停止填充
        /// </summary>
        public bool StopFillingWhenCloseInfoDlg
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
    "charging_form",
    "stop_filling_when_close_infodlg",
    true);
            }
        }

        /// <summary>
        /// 自动操作唯一事项
        /// </summary>
        public bool AutoOperSingleItem
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "auto_oper_single_item",
                    false);
            }
        }

        /// <summary>
        /// 是否启用 ISBN 借书还书功能
        /// </summary>
        public bool UseIsbnBorrow
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "isbn_borrow",
                    true);
            }
        }

        /// <summary>
        /// 显示快速操作对话框
        /// </summary>
        /// <param name="color">信息颜色</param>
        /// <param name="strCaption">对话框标题文字</param>
        /// <param name="strMessage">消息内容文字</param>
        /// <param name="nTarget">对话框关闭后要切换去的位置。为 READER_BARCODE READER_PASSWORD ITEM_BARCODE 之一</param>
        public void FastMessageBox(InfoColor color,
            string strCaption,
            string strMessage,
            int nTarget)
        {
            string strFastInputText = ChargingInfoDlg.Show(
                this.CharingInfoHost,
                strMessage,
                color,
                strCaption,
                this.InfoDlgOpacity,
                this.MainForm.DefaultFont);

            // this.SwitchFocus(nTarget, strFastInputText);
            if (string.IsNullOrEmpty(strFastInputText) == false)
            {
                this.textBox_input.Text = strFastInputText;
                AsyncDoAction(this._funcstate, strFastInputText);
            }
        }

        #endregion

        delegate void Delegate_DisplayTask(string strAction, 
            ChargingTask task);
        //
        // 在显示列表中操作一个 Task 行
        // parameters:
        //      strAction   add remove refresh refresh_and_visible
        internal void DisplayTask(string strAction,
            ChargingTask task)
        {
            if (this.InvokeRequired)
            {
                Delegate_DisplayTask d = new Delegate_DisplayTask(_displayTask);
                this.Invoke(d, new object[] { strAction, task });
            }
            else
            {
                _displayTask(strAction, task);
            }
        }

        void _displayTask(string strAction, ChargingTask task)
        {
            if (strAction == "add")
            {
                DpRow line = new DpRow();
                line.Style = DpRowStyle.HorzGrid;
                line.BackColor = SystemColors.Window;   //  Color.FromArgb(254, 254, 254);
                task.RefreshDisplay(line);

                line.Tag = task;
                this.dpTable_tasks.Rows.Add(line);
                if (this._bScrollBarTouched == false)
                {
                    // TODO: 应该分为两种情况  希望看到最末事项的，和看中间的。信号是触动卷滚条到底部；拖动卷滚条到中部
                    this.dpTable_tasks.FocusedItem = line;
                    line.EnsureVisible();
                }
            }
            else if (strAction == "remove")
            {
                DpRow line = FindTaskLine(task);
                if (line != null)
                    this.dpTable_tasks.Rows.Remove(line);
                else
                {
                    // Debug.Assert(false, "");
                }
            }
            else if (strAction == "refresh"
                || strAction == "refresh_and_visible")
            {
                DpRow line = FindTaskLine(task);
                if (line != null)
                {
                    // 刷新显示
                    task.RefreshDisplay(line);

                    if (task.Action == "load_reader_info" && string.IsNullOrEmpty(task.ReaderXml) == false)
                        task.RefreshPatronCardDisplay(line);

                    if (this._bScrollBarTouched == false)
                    {
                        // 如果刷新的对象是 Focus 对象，则确保显示在视野范围内
                        // TODO: 当发现中途人触动了控件时，这个功能要禁用，以免对人的操作发生干扰
                        if (this.dpTable_tasks.FocusedItem == line)
                            line.EnsureVisible();
                        else
                        {
                            if (strAction == "refresh_and_visible")
                                line.EnsureVisible();
                        }
                    }

                }

            }

            // 刷新读者摘要窗口
            if (this._patronSummaryForm != null
                && this._patronSummaryForm.Visible
                && strAction == "add")
                this._patronSummaryForm.OnTaskStateChanged(strAction, task);
        }

        // 创建读者摘要
        internal List<PatronSummary> BuildPatronSummary(ChargingTask exclude_task)
        {
            List<PatronSummary> results = new List<PatronSummary>();
            PatronSummary current_summary = null;
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task == exclude_task)
                    continue;

                if (task.Action == "load_reader_info")
                {
                    // 为前一个读者
                    if (current_summary != null)
                        current_summary.RefreshColorList();

                    current_summary = new PatronSummary();
                    current_summary.Name = task.ReaderName;
                    current_summary.Barcode = task.ReaderBarcode;
                    current_summary.Tasks.Add(task);
                    results.Add(current_summary);

                    continue;
                }

                if (current_summary != null)
                    current_summary.Tasks.Add(task);
            }

            if (current_summary != null)
                current_summary.RefreshColorList();
            return results;
        }

        // 是否为姓名
        // 包含一个以上汉字，或者 ~ 开头的任意文字
        static bool IsName(string strText)
        {
            if (StringUtil.ContainHanzi(strText) == true)
                return true;
            if (StringUtil.HasHead(strText, "~") == true)
                return true;
            return false;
        }

        bool __bLoadReaderInfo = false;   // true
        /// <summary>
        /// 后面是否需要输入 证条码号。 false 表示需要输入册条码号
        /// </summary>
        public bool WillLoadReaderInfo
        {
            get
            {
                return __bLoadReaderInfo;
            }
            set
            {
                bool bChanged = false;
                if (__bLoadReaderInfo != value)
                    bChanged = true;

                __bLoadReaderInfo = value;

                if (bChanged == true)
                {
                    /*
                    if (value == true)
                        this.MainForm.EnterPatronIdEdit(InputType.PQR);
                    else
                        this.MainForm.LeavePatronIdEdit();
                     * */
                    SetInputMessage(value);

                    // EnterOrLeavePQR(value);
                    EnterOrLeavePQR(true, InputType.ALL);
                }

            }
        }

        delegate void Delegate_EnterOrLeavePQR(bool bEnter, InputType input_type);
        // 进入或离开 PQR 状态
        void EnterOrLeavePQR(bool bEnter, InputType input_type = InputType.ALL)
        {
            if (this.InvokeRequired == true)
            {
                Delegate_EnterOrLeavePQR d = new Delegate_EnterOrLeavePQR(EnterOrLeavePQR);
                this.BeginInvoke(d, new object[] { bEnter, input_type });
                return;
            }
            if (bEnter == true)
                this.MainForm.EnterPatronIdEdit(input_type);
            else
                this.MainForm.LeavePatronIdEdit();
        }

        delegate void Delegate_DoAction(FuncState func,
            string strText,
            string strTaskID);
        // //
        /// <summary>
        /// 执行一个出纳动作。
        /// 由于这是异步执行，不能立即返回操作结果，需要后面主动去查询
        /// </summary>
        /// <param name="func">出纳功能</param>
        /// <param name="strText">字符串。可能是证条码号，也可能是册条码号</param>
        /// <param name="strTaskID">任务 ID，用于管理和查询任务状态</param>
        public void AsyncDoAction(FuncState func,
            string strText,
            string strTaskID = "")
        {
            Delegate_DoAction d = new Delegate_DoAction(_doAction);
            this.BeginInvoke(d, new object[] { func, strText, strTaskID });
        }

        // 盘算是否为 ISBN 字符串
        // 如果用 ISBN 作为前缀，返回的时候 strTextParam 中会去掉前缀部分。这样便于用于对话框检索
        public static bool IsISBN(ref string strTextParam)
        {
            string strText = strTextParam;

            if (string.IsNullOrEmpty(strText) == true)
                return false;
            strText = strText.Replace("-", "").ToUpper();
            if (string.IsNullOrEmpty(strText) == true)
                return false;

            if (StringUtil.HasHead(strText, "ISBN") == true)
            {
                strText = strText.Substring("ISBN".Length).Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    return false;
                strTextParam = strText;
                return true;
            }

            // 2015/5/8
            if (strText.ToUpper().EndsWith("ISBN") == true)
            {
                strText = strText.Substring(0, strText.Length - "ISBN".Length).Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    return false;
                strTextParam = strText;
                return true;
            }

            string strError = "";
            // return:
            //      -1  出错
            //      0   校验正确
            //      1   校验不正确。提示信息在strError中
            int nRet = IsbnSplitter.VerifyISBN(strText,
                out strError);
            if (nRet == 0)
                return true;

#if NO
            if (strText.Length == 13)
            {
                if (IsbnSplitter.IsIsbn13(strText) == true)
                    return true;
            }
#endif

            return false;
        }

        // parameters:
        //      strTaskID   任务 ID，用于管理和查询任务状态
        void _doAction(FuncState func,
            string strText,
            string strTaskID)
        {
            if (string.IsNullOrEmpty(strText) == true)
            {
                MessageBox.Show(this, "请输入适当的条码号");
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }

            // 中国中间(温和)停止过，则需要重新启动线程
            if (this._taskList.Stopped == true)
                this._taskList.BeginThread();

            // m_webExternalHost_readerInfo.StopPrevious();

            if ((this.UseIsbnBorrow == true && IsISBN(ref strText) == true)
                || strText.ToLower() == "?b")
            {
                string strItemBarcode = "";
                string strError = "";
                // return:
                //      -1  error
                //      0   放弃
                //      1   成功
                int nRet = SelectOneItem(func,
                    strText.ToLower() == "?b" ? "" : strText,
                    out strItemBarcode,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "选择册记录的过程中出错: " + strError);
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                    return;
                }
                if (nRet == 0)
                {
                    MessageBox.Show(this, "已取消选择册记录。注意操作并未执行");
                    this.textBox_input.SelectAll();
                    this.textBox_input.Focus();
                    return;
                }

                strText = strItemBarcode;
            }

            // 检查条码号，如果是读者证条码号，则 func = LoadPatronInfo
            if (this.NeedVerifyBarcode == true)
            {
                if (StringUtil.IsIdcardNumber(strText) == true
                    || IsName(strText) == true)
                {
                    WillLoadReaderInfo = true;
                }
                else
                {
                    string strError = "";
                    // 形式校验条码号
                    // return:
                    //      -2  服务器没有配置校验方法，无法校验
                    //      -1  error
                    //      0   不是合法的条码号
                    //      1   是合法的读者证条码号
                    //      2   是合法的册条码号
                    int nRet = VerifyBarcode(
                        this.Channel.LibraryCodeList,
                        strText,
                        out strError);
                    if (nRet == -2)
                    {
                        MessageBox.Show(this, "服务器没有配置条码号验证脚本，无法使用验证条码号功能。请在前端参数配置对话框的“快捷出纳”属性页中清除“校验输入的条码号”事项");
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus();
                        return;
                    }
                    if (nRet == -1)
                    {
                        // TODO: 语音提示
                        // TODO: 红色对话框
                        MessageBox.Show(this, strError);
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus();
                        return;
                    }
                    if (nRet == 0)
                    {
                        // TODO: 语音提示
                        // TODO: 红色对话框
                        MessageBox.Show(this, "'"+strText+"' 不是合法的条码号");
                        this.textBox_input.SelectAll();
                        this.textBox_input.Focus();
                        return;
                    }
                    // 有可能 验证条码号的时候因为 EnableControls 丢失了焦点
                    this.textBox_input.Focus();

                    if (nRet == 1)
                        WillLoadReaderInfo = true;
                    else
                    {
                        Debug.Assert(nRet == 2, "");
                        if (this.WillLoadReaderInfo == true)
                        {
                            // TODO: 语音提示
                            MessageBox.Show(this, "这里需要输入 证 条码号，而您输入的 '"+strText+"' 是一个 册 条码号。\r\n\r\n请重新输入");
                            this.textBox_input.SelectAll();
                            return;
                        }
                    }
                }
            }

            if (WillLoadReaderInfo == true)
            {
                func = FuncState.LoadPatronInfo;
                // _bLoadReaderInfo = false;
            }

            ChargingTask task = new ChargingTask();
            task.ID = strTaskID;
            if (func == FuncState.LoadPatronInfo)
            {
                task.ReaderBarcode = strText;
                task.Action = "load_reader_info";
            }
            else if (func == dp2Circulation.FuncState.Borrow)
            {
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = strText;
                task.Action = "borrow";
            }
            else if (func == dp2Circulation.FuncState.ContinueBorrow)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    // TODO: 这里直接出现对话框搜集读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    return;
                } 
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = strText;
                task.Action = "borrow";
            }
            else if (func == dp2Circulation.FuncState.Renew)
            {
                // task.ReaderBarcode = "";
                task.ItemBarcode = strText;
                task.Action = "renew";
            }
            else if (func == dp2Circulation.FuncState.VerifyRenew)
            {
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = strText;
                task.Action = "verify_renew";
            }
            else if (func == dp2Circulation.FuncState.Return)
            {
                task.ItemBarcode = strText;
                task.Action = "return";
            }
            else if (func == dp2Circulation.FuncState.VerifyReturn)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    // TODO: 这里直接出现对话框搜集读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    return;
                }
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = strText;
                task.Action = "verify_return";
            }
            else if (func == dp2Circulation.FuncState.Lost)
            {
                task.ItemBarcode = strText;
                task.Action = "lost";
            }
            else if (func == dp2Circulation.FuncState.VerifyLost)
            {
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                {
                    WillLoadReaderInfo = true;
                    // 提示请输入读者证条码号
                    MessageBox.Show(this, "请先输入读者证条码号，然后再输入册条码号");
                    this.textBox_input.SelectAll();
                    return;
                }
                task.ReaderBarcode = this._taskList.CurrentReaderBarcode;
                task.ItemBarcode = strText;
                task.Action = "verify_lost";
            }

            this.textBox_input.SelectAll();
            
            try
            {
                this._taskList.AddTask(task);
            }
            catch (LockException ex)
            {
                Delegate_DoAction d = new Delegate_DoAction(_doAction);
                this.BeginInvoke(d, new object[] { func, strText, strTaskID });
            }
        }

        DpRow FindTaskLine(ChargingTask task)
        {
            foreach (DpRow line in this.dpTable_tasks.Rows)
            {
                if (line.Tag == task)
                    return line;
            }
            return null;
        }

        private void webBrowser_reader_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (this.MainForm.SuppressScriptErrors == true)
                e.Handled = true;
        }


        private void toolStripMenuItem_loadPatronInfo_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.LoadPatronInfo;
        }

        private void toolStripMenuItem_borrow_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Borrow,
    true,
    false);
        }

        // 统一读者继续借书。不清除现有窗口内容
        private void toolStripMenuItem_continueBorrow_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.ContinueBorrow,
false,
false);
        }

        private void toolStripMenuItem_return_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Return,
    true,
    false);
        }

        private void toolStripMenuItem_renew_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Renew,
true,
false);
        }

        private void toolStripMenuItem_verifyRenew_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.VerifyRenew,
true,
false);
        }

        private void toolStripMenuItem_lost_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.Lost,
true,
false);
        }

        private void toolStripMenuItem_verifyReturn_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.VerifyReturn,
true,
false);
        }

        private void toolStripMenuItem_verifyLost_Click(object sender, EventArgs e)
        {
            SmartSetFuncState(FuncState.VerifyLost,
true,
false);
        }

        #region 各种配置参数

        public string DisplayFormat
        {
            get
            {
                return this.MainForm.AppInfo.GetString("quickcharging_form",
                    "display_format",
                    "HTML");
            }
        }

        /// <summary>
        /// 显示读者信息的格式。为 text html 之一
        /// </summary>
        public string PatronRenderFormat
        {
            get
            {
                if (_cardControl != null)
                {
                    if (this.NoBorrowHistory == true && this.MainForm.Version >= 2.25)
                        return "xml:noborrowhistory";

                    return "xml";
                }
                if (this.NoBorrowHistory == true && this.MainForm.Version >= 2.21)
                    return "html:noborrowhistory";

                return "html";

            }
        }

        // 读者信息中不显示借阅历史
        public bool NoBorrowHistory
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "no_borrow_history",
                    true);
            }
            set
            {
                this.MainForm.AppInfo.SetBoolean(
                    "quickcharging_form",
                    "no_borrow_history",
                    value);
            }
        }

        /// <summary>
        /// 是否自动清除输入框中内容
        /// </summary>
        public bool AutoClearTextbox
        {
            get
            {
                return true;
#if NO
                return this.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "autoClearTextbox",
                    true);
#endif
            }
        }

        /// <summary>
        /// 是否自动校验输入的条码号
        /// </summary>
        public bool NeedVerifyBarcode
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "verify_barcode",
                    false);
            }
        }

        /// <summary>
        /// 显示书目、册信息的格式。为 text html 之一
        /// </summary>
        public string RenderFormat
        {
            get
            {
                return "html";
            }
        }


        /// <summary>
        /// 是否要朗读读者姓名
        /// </summary>
        public bool SpeakPatronName
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "speak_reader_name",
                    false);
            }
        }

        /// <summary>
        /// 是否要朗读书名
        /// </summary>
        public bool SpeakBookTitle
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "quickcharging_form",
                    "speak_book_title",
                    false);
            }
        }

        #endregion


        FuncState _funcstate = FuncState.Borrow;

        FuncState FuncState
        {
            get
            {
                return _funcstate;
            }
            set
            {

                this._funcstate = value;
                WillLoadReaderInfo = true;
                this._bScrollBarTouched = false;

                this.MainForm.ClearQrLastText();

                this.toolStripMenuItem_borrow.Checked = false;
                this.toolStripMenuItem_return.Checked = false;
                this.toolStripMenuItem_verifyReturn.Checked = false;
                this.toolStripMenuItem_renew.Checked = false;
                this.toolStripMenuItem_verifyRenew.Checked = false;
                this.toolStripMenuItem_lost.Checked = false;
                this.toolStripMenuItem_verifyLost.Checked = false;
                this.toolStripMenuItem_loadPatronInfo.Checked = false;
                this.toolStripMenuItem_continueBorrow.Checked = false;

                if (this.AutoClearTextbox == true)
                {
                    this.textBox_input.Text = "";
                }

                if (_funcstate == FuncState.Borrow)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[0];
                    this.toolStripMenuItem_borrow.Checked = true;
                }
                if (_funcstate == FuncState.ContinueBorrow)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[0];
                    this.toolStripMenuItem_continueBorrow.Checked = true;
                    WillLoadReaderInfo = false;
                }
                if (_funcstate == FuncState.Return)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[1];
                    this.toolStripMenuItem_return.Checked = true;

                    WillLoadReaderInfo = false;
                }
                if (_funcstate == FuncState.VerifyReturn)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[1];
                    this.toolStripMenuItem_verifyReturn.Checked = true;
                }
                if (_funcstate == FuncState.Renew)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[2];
                    this.toolStripMenuItem_renew.Checked = true;

                    WillLoadReaderInfo = false;
                }
                if (_funcstate == FuncState.VerifyRenew)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[2];
                    this.toolStripMenuItem_verifyRenew.Checked = true;
                }
                if (_funcstate == FuncState.Lost)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[3];
                    this.toolStripMenuItem_lost.Checked = true;

                    WillLoadReaderInfo = false;
                }
                if (_funcstate == FuncState.VerifyLost)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[3];
                    this.toolStripMenuItem_verifyLost.Checked = true;
                }
                if (_funcstate == FuncState.LoadPatronInfo)
                {
                    this.pictureBox1.Image = this.imageList_func_large.Images[4];
                    this.toolStripMenuItem_loadPatronInfo.Checked = true;
                }

                // SetInputMessage();
            }
        }

        // 清除任务列表
        // 本函数只能被界面线程调用
        // parameters:
        //      rows    要清除的显示行。如果为 null，表示希望全部清除
        bool ClearTaskByRows(List<DpRow> rows, bool bWarning = true)
        {
            if (rows == null)
            {
                rows = new List<DpRow>();
                foreach (DpRow row in this.dpTable_tasks.Rows)
                {
                    rows.Add(row);
                }
            }

            List<ChargingTask> tasks = new List<ChargingTask>();    // 希望清除的任务
            List<ChargingTask> warning_tasks = new List<ChargingTask>();    // 需要警告的任务
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                tasks.Add(task);
                if (task.Compeleted == false)
                {
                    warning_tasks.Add(task);
                }
            }

            if (warning_tasks.Count == 0 && tasks.Count == 0)
                return true;

            if (bWarning == true)
            {
                if (warning_tasks.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
            "当前有 " + warning_tasks.Count.ToString() + " 个任务尚未完成，如果要继续，必须取消这些任务。\r\n\r\n是否要继续?\r\n\r\n(是) 继续，全部任务被清除; (否) 放弃操作",
            "QuickChargingForm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        this._taskList.stop.DoStop();
                    }
                    else
                        return false;   // 放弃清除
                }
            }

            this._taskList.ClearTasks(tasks);
            foreach (DpRow row in rows)
            {
                this.dpTable_tasks.Rows.Remove(row);
            }

            // this._taskList.Clear();
            // this.dpTable_tasks.Rows.Clear();

            this._taskList.CurrentReaderBarcode = "";
            SetColorList();
            return true;    // 清除并继续
        }

        delegate DialogResult Delegate_AskContinue(string strText);
        internal DialogResult AskContinue(string strText)
        {
            Delegate_AskContinue d = new Delegate_AskContinue(_askContinue);
            return (DialogResult)this.Invoke(d, new object[] { strText });
        }

        internal DialogResult _askContinue(string strText)
        {
            return MessageBox.Show(this,
                strText,
                "QuickChargingForm",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
        }

        delegate void Delegate_ClearTaskList(List<ChargingTask> tasks);
        // 清除任务列表显示
        // 注意，并不负责删除 _taskList 中的元素
        internal void ClearTaskList(List<ChargingTask> tasks)
        {
            if (this.InvokeRequired)
            {
                Delegate_ClearTaskList d = new Delegate_ClearTaskList(ClearTaskList);
                this.Invoke(d, new object[] { tasks });
                return;
            }

            if (tasks == null)
                this.dpTable_tasks.Rows.Clear();
            else
            {
                for(int i = 0; i<this.dpTable_tasks.Rows.Count ; i++)
                {
                    DpRow row = this.dpTable_tasks.Rows[i];
                    if (tasks.IndexOf((ChargingTask)row.Tag) != -1)
                    {
                        // this.dpTable_tasks.Rows.RemoveAt(i);
                        this.dpTable_tasks.Rows.Remove(row);
                        i--;
                    }
                }
            }

            this._bScrollBarTouched = false;
        }

        delegate void Delegate_DisplayCurrentReaderBarcode(string strReaderBarcode);
        /// <summary>
        /// 显示当前证条码号到工具条上
        /// </summary>
        /// <param name="strReaderBarcode">读者证条码号</param>
        void DisplayCurrentReaderBarcode(string strReaderBarcode)
        {
            if (this.InvokeRequired)
            {
                Delegate_DisplayCurrentReaderBarcode d = new Delegate_DisplayCurrentReaderBarcode(DisplayCurrentReaderBarcode);
                this.BeginInvoke(d, new object[] { strReaderBarcode });
                return;
            }

            this.toolStripLabel_currentPatron.Text = strReaderBarcode;
        }


        delegate void Delegate_SetInputMessage(bool bReaderBarcode);
        // 刷新 输入号码类型的标签显示
        void SetInputMessage(bool bReaderBarcode)
        {
            if (this.InvokeRequired)
            {
                Delegate_SetInputMessage d = new Delegate_SetInputMessage(SetInputMessage);
                this.BeginInvoke(d, new object[] { bReaderBarcode });
                return;
            }

            if (bReaderBarcode == true)
            {
                this.label_barcode_type.ImageIndex = 1;
                this.label_input_message.Text = "证 条码号";
            }
            else
            {
                this.label_barcode_type.ImageIndex = 0;
                this.label_input_message.Text = "册 条码号";
            }
        }

        delegate void Delegate_SetColorList();
        // 显示色条
        internal void SetColorList()
        {
            if (this.InvokeRequired)
            {
                Delegate_SetColorList d = new Delegate_SetColorList(SetColorList);
                this.BeginInvoke(d);
                return;
            }

            int nWaitingCount = 0;
            StringBuilder text = new StringBuilder(256);
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if ((task.State == "begin" || string.IsNullOrEmpty(task.State) == true)
                    && task.Action != "load_reader_info")
                    nWaitingCount++;

                if (task.Action == "load_reader_info")
                    continue;   // 装载读者的动作不计算在颜色显示内

                char color = 'W';   // 缺省为白色
                if (string.IsNullOrEmpty(task.Color) == false)
                    color = Char.ToUpper(task.Color[0]);
                text.Append(color);
            }

            this.colorSummaryControl1.ColorList = text.ToString();

            // TODO: 是否延迟显示，避免反复出现和隐藏
            if (nWaitingCount > 0)
            {
                string strState = "";
                if (this._taskList.Stopped == true)
                    strState = "已暂停任务处理。\r\n";
                this._floatingMessage.Text = strState + "有 " + nWaitingCount.ToString() + " 个任务尚未完成 ...";
            }
            else
            {
                if (this._taskList.Stopped == true)
                    this._floatingMessage.Text = "已暂停任务处理。";
                else
                    this._floatingMessage.Text = "";
            }

            // 刷新读者摘要窗口
            if (this._patronSummaryForm != null
                && this._patronSummaryForm.Visible)
            {
                List<PatronSummary> summaries = BuildPatronSummary(null);
                _patronSummaryForm.PatronSummaries = summaries;
                _patronSummaryForm.FillList();
                if (summaries.Count == 0)
                {
                    _patronSummaryForm.Close();
                    _patronSummaryForm = null;
                }
            }

        }

        // 当前读者证条码号已经成功设置
        internal void CurrentReaderBarcodeChanged(string strReaderBarcode)
        {
            // 在装载读者记录的时候，不改变应输入条码号类型的提示
            if (this.FuncState != dp2Circulation.FuncState.LoadPatronInfo)
            {
                if (string.IsNullOrEmpty(strReaderBarcode) == false)
                    this.WillLoadReaderInfo = false;
                else
                    this.WillLoadReaderInfo = true;

                // SetInputMessage();
            }

            // 显示到 ToolStrip 上
            DisplayCurrentReaderBarcode(strReaderBarcode);
        }

        /// <summary>
        /// 功能类型。设置时带有焦点切换功能
        /// </summary>
        public FuncState SmartFuncState
        {
            get
            {
                return _funcstate;
            }
            set
            {

                SmartSetFuncState(value,
                    true,
                    true);
            }
        }

        // 智能设置功能名。
        // parameters:
        //      bClearInfoWindow    切换中是否清除信息窗内容
        //      bDupAsClear 是否把重复的设置动作当作清除输入域内容来理解
        void SmartSetFuncState(FuncState value,
            bool bClearInfoWindow,
            bool bDupAsClear)
        {
            this.webBrowser_reader.Stop();
            this.m_webExternalHost_readerInfo.StopPrevious();

            // 清除 webbrowser 和任务列表
            if (bClearInfoWindow == true)
            {
                if (ClearTaskByRows(null, true) == false)
                    return;
                if (this.IsCardMode == true)
                    SetReaderCardString("");
                else
                    SetReaderHtmlString("(空)");
            }

            FuncState old_funcstate = this._funcstate;

            this.FuncState = value;

            // 同一读者借的附加判断
            if (value == dp2Circulation.FuncState.ContinueBorrow)
            {
                // TODO: 警告一下，让操作者知道需要先输入读者证条码号
                // 或者将功能菜单 disabled

                // 如果当前证条码号为空，则只好让操作者先输入读者证条码号，本功能就退化为普通借书功能了
                if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                    this.WillLoadReaderInfo = true;
            }

            // 切换为不同的功能的时候，定位焦点
            if (old_funcstate != this._funcstate)
            {

                if (this.AutoClearTextbox == true)
                {
                    this.textBox_input.Text = "";
                }

                if (this._funcstate != FuncState.Return)
                {
                    this.textBox_input.SelectAll();
                }
                else
                {
                    this.textBox_input.SelectAll();
                }
            }
            else // 重复设置为同样功能，当作清除功能
            {

                if (this.AutoClearTextbox == true)
                {
                    this.textBox_input.Text = "";
                }
                else
                {
                    if (bDupAsClear == true)
                    {
                        this.textBox_input.Text = "";
                    }
                }

                // focus input 
                this.textBox_input.Focus();
            }
        }

        private void QuickChargingForm_Move(object sender, EventArgs e)
        {
            // OnResizeOrMove();
        }

        private void QuickChargingForm_SizeChanged(object sender, EventArgs e)
        {
            // OnResizeOrMove();
        }

#if NO
        void OnResizeOrMove()
        {
            if (this._floatingMessage != null)
            {
                Rectangle rect = this.ClientRectangle;  //  new Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);

                Rectangle screen_rect = this.RectangleToScreen(rect);

                this._floatingMessage.Location = new Point(screen_rect.X,
                    screen_rect.Y);

                this._floatingMessage.Size = new Size(screen_rect.Width, screen_rect.Height);
            }
        }
#endif

        // 查询任务状态
        // 如果列表中同样 ID 的任务超过一个，则只给出第一个的状态
        // return:
        //      null    指定的任务没有找到
        //      其他      任务状态字符串
        public string GetTaskState(string strTaskID)
        {
            foreach (DpRow row in this.dpTable_tasks.Rows)
            {
                row.Selected = false;
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task.ID == strTaskID)
                    return task.State;
            }

            return null;
        }

        // 将指定的任务行滚入可见范围，并设为焦点状态
        // parameters:
        //      index   事项的 index，是排出了颜色为 "" 的 Task
        bool EnsureVisibleLine(int index)
        {
            int i = 0;
            bool bFound = false;
            foreach(DpRow row in this.dpTable_tasks.Rows)
            {
                row.Selected = false;
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (string.IsNullOrEmpty(task.Color) == true)
                    continue;
                if (i == index)
                {
                    this.dpTable_tasks.FocusedItem = row;
                    row.Selected = true; 
                    row.EnsureVisible();
                    bFound = true;
                }
                i ++;
            }

            return bFound;
        }

        private void colorSummaryControl1_Click(object sender, EventArgs e)
        {
            Point pt = Control.MousePosition;
            pt = this.colorSummaryControl1.PointToClient(pt);
            int index = this.colorSummaryControl1.HitTest(pt.X, pt.Y);
            // MessageBox.Show(this, index.ToString());
            EnsureVisibleLine(index);
        }

        private void dpTable_tasks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;
            ToolStripSeparator menuSepItem = null;

            DpRow selected_row = null;
            ChargingTask selected_task = null;
            if (this.dpTable_tasks.SelectedRows.Count > 0)
            {
                selected_row = this.dpTable_tasks.SelectedRows[0];
                selected_task = (ChargingTask)selected_row.Tag;
            }


            // 
            menuItem = new ToolStripMenuItem("打开到 读者窗(&R)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToReaderInfoForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 交费窗(&A)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToAmerceForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 激活窗[源] (&S)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToActivateForm_old_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 停借窗(&M)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToReaderManageForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 激活窗[目标] (&T)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ReaderBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToActivateForm_old_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 册窗(&I)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ItemBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToItemInfoForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 种册窗(&E)");
            if (this.dpTable_tasks.SelectedRows.Count > 0
                && selected_task != null && String.IsNullOrEmpty(selected_task.ItemBarcode) == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToEntityForm_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("复制 [" + this.dpTable_tasks.SelectedRows.Count.ToString() + "] (&D)");
            if (this.dpTable_tasks.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_copy_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("删除任务 [" + this.dpTable_tasks.SelectedRows.Count.ToString() + "] (&D)");
            if (this.dpTable_tasks.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_deleteTask_Click);
            contextMenu.Items.Add(menuItem);

#if NO
            // 
            menuItem = new ToolStripMenuItem("test");
            menuItem.Click += new EventHandler(menuItem_test_Click);
            contextMenu.Items.Add(menuItem);

#endif
            // 
            menuItem = new ToolStripMenuItem("test change state");
            menuItem.Click += new EventHandler(menuItem_test_change_state_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.dpTable_tasks, e.Location);

        }

        // 获得一个 DpRow 行的用于 Copy 的文本
        static string GetRowText(DpRow row)
        {
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (DpCell cell in row)
            {
                // 跳过第一列
                if (i > 0)
                {
                    if (text.Length > 0)
                        text.Append("\t");
                    text.Append(cell.Text);
                }

                i++;
            }

            return text.ToString();
        }

        // TODO: 增加双格式功能，可以 paste 到任务列表中。任务还可以重新提交执行
        void menuItem_copy_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
#if NO
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                strTotal.Append(task.ErrorInfo + "\r\n");
#endif
                strTotal.Append(GetRowText(row) + "\r\n");

            }

            Clipboard.SetDataObject(strTotal.ToString(), true);

            this.Cursor = oldCursor;
        }

        void menuItem_test_change_state_Click(object sender, EventArgs e)
        {
            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                task.State = "begin";
                this.DisplayTask("refresh", task);
            }
        }

        // TODO: 可以成批获得摘要
        void menuItem_test_Click(object sender, EventArgs e)
        {
            HtmlDocument doc = this.webBrowser_reader.Document;
            HtmlElementCollection col = doc.GetElementsByTagName("td");
            string strResult = "";
            List<HtmlElement> nodes = new List<HtmlElement>();
            foreach (HtmlElement ele in col)
            {
                string strClass = ele.GetAttribute("className");
                strResult += ele.OuterHtml;
                if (strClass.IndexOf("pending") != -1)
                    nodes.Add(ele);
            }

            m_webExternalHost_readerInfo.IsInLoop = true;
            foreach (HtmlElement ele in nodes)
            {

                string strText = ele.InnerText.Trim();
                string strLeft = "";
                string strRight = "";
                if (strText.IndexOf(":") != -1)
                {
                    StringUtil.ParseTwoPart(strText,
                        ":",
                        out strLeft,
                        out strRight);
                }
                else
                    strRight = strText;

                ele.InnerHtml = "<img src='./servermapped/images/ajax-loader.gif'></img>";

                if (strLeft == "P")
                    ele.InnerHtml = m_webExternalHost_readerInfo.GetPatronSummary(strRight);
                else
                    ele.InnerHtml = "<div class='wide'><div>" + m_webExternalHost_readerInfo.GetSummary(strRight, false);

                string strClass = ele.GetAttribute("className");
                if (string.IsNullOrEmpty(strClass) == false)
                {
                    strClass = strClass.Replace("pending", "");
                    ele.SetAttribute("className", strClass);
                }
            }

            // MessageBox.Show(this, strResult);
        }

        // 删除选定的任务
        // 如果有没有完成的任务，则需要统一中断(等待完成)，然后再删除任务
        void menuItem_deleteTask_Click(object sender, EventArgs e)
        {
            int nErrorCount = 0;
            int nNotCompleteCount = 0;
            int nYellowCount = 0;

            List<DpRow> rows = new List<DpRow>();
            foreach (DpRow row in this.dpTable_tasks.SelectedRows)
            {
                ChargingTask task = (ChargingTask)row.Tag;
                if (task == null)
                    continue;
                if (task.State == "error")
                    nErrorCount++;
                if ((string.IsNullOrEmpty(task.State) == true && task.Action != "load_reader_info")
                    || task.State == "begin")
                    nNotCompleteCount++;
                if (task.Color == "yellow")
                    nYellowCount++;

                rows.Add(row);
            }

            if (rows.Count == 0)
            {
                MessageBox.Show(this, "当前没有任何任务可以清除");
                return;
            }

            if (nErrorCount + nNotCompleteCount + nYellowCount != 0)
            {
                string strText = "";
                if (nNotCompleteCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "; ";
                    strText += nNotCompleteCount.ToString() + " 个未完成的事项";
                }
                if (nErrorCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "; ";
                    strText += nErrorCount.ToString() + " 个发生错误的(红色)事项";
                }
                if (nYellowCount > 0)
                {
                    if (string.IsNullOrEmpty(strText) == false)
                        strText += "; ";
                    strText += nYellowCount.ToString() + " 个需要进一步处理的(黄色)事项";
                }

                DialogResult result = MessageBox.Show(this,
"当前有 " + strText + "。\r\n\r\n确实要清除选定的 "+rows.Count.ToString()+" 个事项?",
"QuickChargingForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
            }

            ClearTaskByRows(rows, false);
        }

        // 打开到 读者窗
        void menuItem_loadToReaderInfoForm_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("readerinfo_form");
        }

        // 打开到 交费窗
        void menuItem_loadToAmerceForm_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("amerce_form");

        }

        // 打开到 激活窗 (源)
        void menuItem_loadToActivateForm_old_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("activate_form_old");
        }

        // 打开到 激活窗 (目标)
        void menuItem_loadToActivateForm_new_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("activate_form_new");
        }

        // 打开到 停借窗
        void menuItem_loadToReaderManageForm_Click(object sender, EventArgs e)
        {
            LoadToPatronTypeForm("readermanage_form");
        }

        // 装入读者相关类型的窗口
        void LoadToPatronTypeForm(string strType)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            DpRow selected_row = null;
            ChargingTask selected_task = null;

            selected_row = this.dpTable_tasks.SelectedRows[0];
            selected_task = (ChargingTask)selected_row.Tag;

            if (string.IsNullOrEmpty(selected_task.ReaderBarcode) == true)
            {
                strError = "所选定的任务事项不具备证条码号信息";
                goto ERROR1;
            }

            if (strType == "readerinfo_form")
            {
                ReaderInfoForm form = this.MainForm.EnsureReaderInfoForm();
                Global.Activate(form);

                form.LoadRecord(selected_task.ReaderBarcode,
                    false);
            }
            if (strType == "amerce_form")
            {
                AmerceForm form = this.MainForm.EnsureAmerceForm();
                Global.Activate(form);

                form.LoadReader(selected_task.ReaderBarcode, true);
            }
            if (strType == "activate_form_old")
            {
                ActivateForm form = this.MainForm.EnsureActivateForm();
                Global.Activate(form);

                form.LoadOldRecord(selected_task.ReaderBarcode);
            }
            if (strType == "activate_form_new")
            {
                ActivateForm form = this.MainForm.EnsureActivateForm();
                Global.Activate(form);

                form.LoadNewRecord(selected_task.ReaderBarcode);
            }
            if (strType == "readermanage_form")
            {
                ReaderManageForm form = this.MainForm.EnsureReaderManageForm();
                Global.Activate(form);

                form.LoadRecord(selected_task.ReaderBarcode);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打开到 册窗
        void menuItem_loadToItemInfoForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            DpRow selected_row = null;
            ChargingTask selected_task = null;

            selected_row = this.dpTable_tasks.SelectedRows[0];
            selected_task = (ChargingTask)selected_row.Tag;

            if (string.IsNullOrEmpty(selected_task.ItemBarcode) == true)
            {
                strError = "所选定的任务事项不具备册条码号信息";
                goto ERROR1;
            }

            ItemInfoForm form = this.MainForm.EnsureItemInfoForm();
            Global.Activate(form);

            form.LoadRecord(selected_task.ItemBarcode);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打开到 种册窗
        void menuItem_loadToEntityForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_tasks.SelectedRows.Count == 0)
            {
                strError = "尚未选定要操作的任务事项";
                goto ERROR1;
            }

            DpRow selected_row = null;
            ChargingTask selected_task = null;

            selected_row = this.dpTable_tasks.SelectedRows[0];
            selected_task = (ChargingTask)selected_row.Tag;

            if (string.IsNullOrEmpty(selected_task.ItemBarcode) == true)
            {
                strError = "所选定的任务事项不具备册条码号信息";
                goto ERROR1;
            }

            EntityForm form = this.MainForm.EnsureEntityForm();
            Global.Activate(form);

            form.LoadItemByBarcode(selected_task.ItemBarcode, false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void QuickChargingForm_Activated(object sender, EventArgs e)
        {
#if NO
            if (_floatingMessage != null)
            {
                try
                {
                    _floatingMessage.Show();
                }
                catch
                {
                }
            }
#endif

            this.textBox_input.Focus();
        }

        private void QuickChargingForm_Deactivate(object sender, EventArgs e)
        {
#if NO
            if (_floatingMessage != null)
            {
                try
                {
                    _floatingMessage.Hide();
                }
                catch
                {
                }
            }
#endif
        }

        private void textBox_input_Enter(object sender, EventArgs e)
        {
#if NO
            if (__bLoadReaderInfo == true)
            {
                EnterOrLeavePQR(__bLoadReaderInfo);
            }
#endif
            // 扫入 3 种条码均可
            EnterOrLeavePQR(true, InputType.ALL);
        }

        private void textBox_input_Leave(object sender, EventArgs e)
        {
            EnterOrLeavePQR(false);
        }

        private void QuickChargingForm_Enter(object sender, EventArgs e)
        {

        }

        bool _bScrollBarTouched = false;    // 当前一轮操作中，任务列表的卷滚条是否被触动过。如果被触动过，则刷新显示和加入新对象的过程，均不要自动卷动内容

        private void dpTable_tasks_ScrollBarTouched(object sender, ScrollBarTouchedArgs e)
        {
            this._bScrollBarTouched = true;
        }

        private void dpTable_tasks_Click(object sender, EventArgs e)
        {
            this._bScrollBarTouched = true;
        }


        private void dpTable_tasks_PaintRegion(object sender, PaintRegionArgs e)
        {
            if (e.Action == "query")
            {
                e.Height = 100;
                DpCell cell = e.Item as DpCell;
                DpRow row = cell.Container;
                ChargingTask task = (ChargingTask)row.Tag;

                PatronCardInfo info = new PatronCardInfo();

                string strError = "";

                int nRet = info.SetData(task.ReaderXml, out strError);
                if (nRet == -1)
                {
                    e.Height = 0;
                    return;
                }
                info.Layout(Graphics.FromHwnd(this.dpTable_tasks.Handle),
_cardStyle,
e.Width,
e.Height);
                cell.Tag = info;
                return;
            }

            {
                Debug.Assert(e.Action == "paint", "");
                PatronCardInfo info = ((DpCell)e.Item).Tag as PatronCardInfo;
                if (info != null)
                {
                    Debug.Assert(info != null, "");

                    info.Paint(e.pe.Graphics,
                        e.X,
                        e.Y,
                        this._cardStyle);
                }
            }
        }

        private void contextMenuStrip_selectFunc_Opening(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(this._taskList.CurrentReaderBarcode) == true)
                this.toolStripMenuItem_continueBorrow.Enabled = false;
            else
                this.toolStripMenuItem_continueBorrow.Enabled = true;
        }

        PatronSummaryForm _patronSummaryForm = null;

        delegate void Delegate_DisplayReaderSummary(ChargingTask exclude_task, string strText);
        internal void DisplayReaderSummary(ChargingTask exclude_task,
            string strText)
        {
            if (this.InvokeRequired)
            {
                Delegate_DisplayReaderSummary d = new Delegate_DisplayReaderSummary(DisplayReaderSummary);
                this.BeginInvoke(d,
                    new object[] { exclude_task, strText }
                    );
                return;
            }

            List<PatronSummary> summaries = BuildPatronSummary(exclude_task);

            if (_patronSummaryForm == null)
            {
                _patronSummaryForm = new PatronSummaryForm();
                _patronSummaryForm.FormClosed -= new FormClosedEventHandler(_patronSummaryForm_FormClosed);
                _patronSummaryForm.FormClosed += new FormClosedEventHandler(_patronSummaryForm_FormClosed);
                // _patronSummaryForm.Show(this);
            }

            _patronSummaryForm.PatronSummaries = summaries;
            _patronSummaryForm.Font = this.Font;


            if (_patronSummaryForm.Visible == false)
            {
                this.MainForm.AppInfo.LinkFormState(this._patronSummaryForm, "_patronSummaryForm_state");
                _patronSummaryForm.Show(this);
            }
            else
                _patronSummaryForm.FillList();

            _patronSummaryForm.Comment = strText;
            _patronSummaryForm.ShowComment();
        }

        void _patronSummaryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_patronSummaryForm != null
                && this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(_patronSummaryForm);
                this._patronSummaryForm = null;
            }
        }

        private void toolStripButton_openPatronSummaryWindow_Click(object sender, EventArgs e)
        {
            List<PatronSummary> summaries = BuildPatronSummary(null);

            _patronSummaryForm = new PatronSummaryForm();
            _patronSummaryForm.PatronSummaries = summaries;
            _patronSummaryForm.Font = this.Font;
            _patronSummaryForm.Show(this);
        }

        /// <summary>
        /// 打印借还凭条
        /// </summary>
        public void Print()
        {
            // 触发历史动作
            this.MainForm.OperHistory.Print();
        }

#if NO
        private void toolStripButton_enableHanzi_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_enableHanzi.Checked == false)
                this.toolStripButton_enableHanzi.Checked = true;
            else
                this.toolStripButton_enableHanzi.Checked = false;
        }
#endif

        private void toolStripButton_enableHanzi_CheckedChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_enableHanzi.Checked == true)
            {
                this.toolStripButton_enableHanzi.Text = "中";
                this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            }
            else
            {
                this.toolStripButton_enableHanzi.Text = "英";
                this.textBox_input.ImeMode = System.Windows.Forms.ImeMode.Disable;
            }
        }

        internal void TriggerBorrowComplete(BorrowCompleteEventArgs e)
        {
            if (this.BorrowComplete != null)
                this.BorrowComplete(this, e);
        }

        private void toolStripButton_selectItem_Click(object sender, EventArgs e)
        {
            string strItemBarcode = "";
            string strError = "";
            // return:
            //      -1  error
            //      0   放弃
            //      1   成功
            int nRet = SelectOneItem(this._funcstate,
                "",
                out strItemBarcode,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "选择册记录的过程中出错: " + strError);
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }
            if (nRet == 0)
            {
                MessageBox.Show(this, "已取消选择册记录。注意操作并未执行");
                this.textBox_input.SelectAll();
                this.textBox_input.Focus();
                return;
            }

            this.textBox_input.Text = strItemBarcode;
            AsyncDoAction(this.FuncState, GetUpperCase(this.textBox_input.Text));
        }

        string GetUpperCase(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;
            if (this.toolStripButton_upperInput.Checked == true)
                return strText.ToUpper();
            return strText;
        }

        private void toolStripButton_upperInput_CheckedChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_upperInput.Checked == true)
                this.toolStripButton_upperInput.Text = "A";
            else
                this.toolStripButton_upperInput.Text = "a";
        }

    }

    /// <summary>
    /// 借书还书操作完成事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void BorrowCompleteEventHandler(object sender,
        BorrowCompleteEventArgs e);

    /// <summary>
    /// 借书还书操作完成事件的参数
    /// </summary>
    public class BorrowCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// 动作
        /// </summary>
        public string Action = "";
        /// <summary>
        /// 册条码号
        /// </summary>
        public string ItemBarcode = "";
        /// <summary>
        /// 读者证条码号
        /// </summary>
        public string ReaderBarcode = "";
    }

    // 任务列表
    class TaskList : ThreadBase
    {
        public QuickChargingForm Container = null;

        List<ChargingTask> _tasks = new List<ChargingTask>();

        string _strCurrentReaderBarcode = "";
        public string CurrentReaderBarcode
        {
            get
            {
                return this._strCurrentReaderBarcode;
            }
            set
            {
                this._strCurrentReaderBarcode = value;

                if (this.Container != null)
                    this.Container.CurrentReaderBarcodeChanged(this.CurrentReaderBarcode);  // 通知 读者记录已经成功装载
            }
        }

        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = null;
        public DigitalPlatform.Stop stop = null;

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

#if NO

        bool m_bStopThread = true;
        internal Thread _thread = null;

        internal AutoResetEvent eventClose = new AutoResetEvent(false);	// true : initial state is signaled 
        internal AutoResetEvent eventActive = new AutoResetEvent(false);	// 激活信号
        // internal AutoResetEvent eventFinished = new AutoResetEvent(false);	// true : initial state is signaled 

        public int PerTime = 1000;   // 1 秒 5 * 60 * 1000;	// 5 分钟
#endif

        public override void StopThread(bool bForce)
        {
            // this.Clear();

            base.StopThread(bForce);
        }

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            int nOldCount = 0;
            List<ChargingTask> tasks = new List<ChargingTask>();
            List<ChargingTask> remove_tasks = new List<ChargingTask>();
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new LockException("锁定尝试中超时");
            try
            {
                nOldCount = this._tasks.Count;
                foreach (ChargingTask task in this._tasks)
                {
                    if (task.State == "")
                    {
                        tasks.Add(task);
                    }

#if NO
                    if (task.State == "finish"
                        && task.Action == "load_reader_info"
                        && task.ReaderBarcode != this.CurrentReaderBarcode)
                        remove_tasks.Add(task);
#endif
                }
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }

            if (tasks.Count > 0)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("进行一轮任务处理...");
                stop.BeginLoop();
                try
                {

                    foreach (ChargingTask task in tasks)
                    {
                        if (this.Stopped == true)
                        {
                            this.Container.SetColorList();  // 促使“任务已经暂停”显示出来
                            return;
                        }

                        if (stop != null && stop.State != 0)
                        {
                            this.Stopped = true;
                            this.Container.SetColorList();  // 促使“任务已经暂停”显示出来
                            return;
                        }

                        // bool bStop = false;
                        // 执行任务
                        if (task.Action == "load_reader_info")
                        {
                            LoadReaderInfo(task);
                        }
                        else if (task.Action == "borrow"
                            || task.Action == "renew"
                            || task.Action == "verify_renew")
                        {
                            Borrow(task);
                        }
                        else if (task.Action == "return"
                            || task.Action == "verify_return"
                            || task.Action == "lost"
                            || task.Action == "verify_lost")
                        {
                            Return(task);
                        }

                        stop.SetMessage("");

#if NO
                    if (bStop == true)
                    {
                        this.m_bStopThread = true;
                        this.Container.SetColorList();  // 促使“任务已经暂停”显示出来
                        return;
                    }
#endif
                    }

                }
                finally
                {

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }

            bool bChanged = false;
            if (remove_tasks.Count > 0)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
                try
                {
                    if (this._tasks.Count != nOldCount)
                        bChanged = true;

                    foreach (ChargingTask task in remove_tasks)
                    {
                        RemoveTask(task, false);
                    }
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }
            }
            /*
            if (bChanged == true)
                this.Activate();
             * */
        }



        // 获得可以发送给服务器的证条码号字符串
        // 去掉前面的 ~
        static string GetRequestPatronBarcode(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            if (strText[0] == '~')
                return strText.Substring(1);

            return strText;
        }

        // 将字符串中的宏 %datadir% 替换为实际的值
        string ReplaceMacro(string strText)
        {
            strText = strText.Replace("%mappeddir%", PathUtil.MergePath(this.Container.MainForm.DataDir, "servermapped"));
            return strText.Replace("%datadir%", this.Container.MainForm.DataDir);
        }

        internal void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        // 装载读者信息
        // return:
        //      false   正常
        //      true    需要停止后继的需要通道的操作
        void LoadReaderInfo(ChargingTask task)
        {
            task.State = "begin";
            task.Color = "purple";    // "light";
            this.Container.DisplayTask("refresh", task);
            this.Container.SetColorList();

#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("装入读者信息 " +task.ReaderBarcode+ "...");
            stop.BeginLoop();
            try
            {
#endif
            stop.SetMessage("装入读者信息 " + task.ReaderBarcode + "...");

                string strError = "";

                if (this.Container.IsCardMode == true)
                    this.Container.SetReaderCardString("");
                else
                    this.Container.SetReaderHtmlString("(空)");

                string strStyle = this.Container.PatronRenderFormat;
                if (this.Container.SpeakPatronName == true)
                    strStyle += ",summary";
                strStyle += ",xml";
            if (this.Container.MainForm.Version >= 2.25)
            strStyle += ":noborrowhistory";
#if NO
            if (this.VoiceName == true)
                strStyle += ",summary";
#endif

                stop.SetMessage("正在装入读者记录 " + task.ReaderBarcode + " ...");

                string[] results = null;
                byte[] baTimestamp = null;
                string strRecPath = "";
                long lRet = this.Channel.GetReaderInfo(
                    stop,
                    GetRequestPatronBarcode(task.ReaderBarcode),
                    strStyle,   // this.RenderFormat, // "html",
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);

                task.ErrorInfo = strError;

                if (lRet == 0)
                {
                     
                    if (StringUtil.IsIdcardNumber(task.ReaderBarcode) == true)
                        task.ErrorInfo = ("证条码号(或身份证号)为 '" + task.ReaderBarcode + "' 的读者记录没有找到 ...");
                    else
                        task.ErrorInfo = ("证条码号为 '" + task.ReaderBarcode + "' 的读者记录没有找到 ...");

                    goto ERROR1;   // not found
                }
                if (lRet == -1)
                    goto ERROR1;

                if (results == null || results.Length == 0)
                {
                    strError = "返回的results不正常。";
                    goto ERROR1;
                }
                string strResult = "";
                strResult = results[0];
                string strReaderXml = results[results.Length - 1];

                if (lRet > 1)
                {
                    string strBarcode = "";
                    // return:
                    //      -1  error
                    //      0   放弃
                    //      1   成功
                    int nRet = this.Container.SelectOnePatron(lRet,
                        strRecPath,
                        out strBarcode,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                    {
                        task.ErrorInfo = strError;
                        goto ERROR1;
                    }
                    if (nRet == 0)
                    {
                        strError = "放弃装入读者记录";
                        task.ErrorInfo = strError;
                        goto ERROR1;
                    }

                    if (task.ReaderBarcode != strBarcode)
                    {
                        task.ReaderBarcode = strBarcode;

                        // TODO: 此时 task.ReaderXml 需要另行获得
                        strReaderXml = "";

                        strStyle = "xml";
            if (this.Container.MainForm.Version >= 2.25)
            strStyle += ":noborrowhistory";
                        results = null;

                        lRet = this.Channel.GetReaderInfo(
    stop,
    strBarcode,
    strStyle,   // this.RenderFormat, // "html",
    out results,
    out strRecPath,
    out baTimestamp,
    out strError);
                        if (lRet == 1 && results != null && results.Length >= 1)
                            strReaderXml = results[0];
                    }
                }
                else
                {
                    // 检查用户输入的 barcode 是否和读者记录里面的 barcode 吻合
                }

                task.ReaderXml = strReaderXml;

                if (string.IsNullOrEmpty(strReaderXml) == false)
                {
                    task.ReaderName = Global.GetReaderSummary(strReaderXml);
                }

                if (this.Container.IsCardMode == true)
                    this.Container.SetReaderCardString(strReaderXml);
                else 
                    this.Container.SetReaderHtmlString(ReplaceMacro(strResult));

                // 如果切换了读者
                // if (this.CurrentReaderBarcode != task.ReaderBarcode)
                {
                    // 删除除了本任务以外的其他任务
                    // TODO: 需要检查这些任务中是否有尚未完成的
                    List<ChargingTask> tasks = new List<ChargingTask>();
                    tasks.AddRange(this._tasks);
                    tasks.Remove(task);

                    int nCount = CountNotFinishTasks(tasks);
                    if (nCount > 0)
                    {
#if NO
                        if (this.Container.AskContinue("当前有 " + nCount.ToString()+ " 个任务尚未完成。\r\n\r\n是否清除这些任务并继续?") == DialogResult.Cancel)
                        {
                            strError = "装入读者记录的操作被中断";
                            task.ErrorInfo = strError;
                            goto ERROR1;
                        }
#endif
                        this.Container.DisplayReaderSummary(task, "前面读者有 " + nCount.ToString()+ " 个任务尚未完成，或有提示需要进一步处理。\r\n点击此处查看摘要信息");
                    }
                    else
                        this.Container.ClearTaskList(tasks);
                }

                this.CurrentReaderBarcode = task.ReaderBarcode; // 会自动显示出来

                if (this.Container.SpeakPatronName == true && results.Length >= 2)
                {
                    string strName = results[1];
                    this.Container.MainForm.Speak(strName);
                }



                // this.m_strCurrentBarcode = strBarcode;
                task.State = "finish";
                task.Color = "";
                // 兑现显示
                this.Container.DisplayTask("refresh", task);
                this.Container.SetColorList();
                // this.Container.CurrentReaderBarcodeChanged(this.CurrentReaderBarcode);  // 通知 读者记录已经成功装载
#if NO
                if (stop != null && stop.State != 0)
                    return true;
                return false;
#endif 
            return;
            ERROR1:
                task.State = "error";
                task.Color = "red";
                this.CurrentReaderBarcode = ""; // 及时清除上下文,避免后面错误借到先前的读者名下
                if (this.Container.IsCardMode == true)
                    this.Container.SetReaderCardString(task.ErrorInfo);
                else
                    this.Container.SetReaderTextString(task.ErrorInfo);
                // 兑现显示
                this.Container.DisplayTask("refresh", task);
                this.Container.SetColorList();
                this.Stopped = true;  // 全部任务停止处理。这是因为装载读者的操作比较重要，是后继操作的前提
#if NO
                if (stop != null && stop.State != 0)
                    return true;
                return false;
#endif
                return;
#if NO
            }
            finally
            {

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
#endif
        }

        // 统计若干任务中，有多少个处于未完成的状态。和黄色 红色状态
        static int CountNotFinishTasks(List<ChargingTask> tasks)
        {
            int nCount = 0;
            foreach (ChargingTask task in tasks)
            {
                if (task.State == "begin" || task.Color == "black"
                    || task.Color == "yellow" || task.Color == "red")
                    nCount++;
            }

            return nCount;
        }

        string GetPostFix()
        {
            if (this.Container.MainForm.Version >= 2.24)
                return ":noborrowhistory";
            return "";
        }

        // 借书
        void Borrow(ChargingTask task)
        {
            DateTime start_time = DateTime.Now;

            task.State = "begin";
            task.Color = "purple";  //  "light";
            this.Container.DisplayTask("refresh", task);
            this.Container.SetColorList();

            string strOperText = task.ReaderBarcode + " 借 " + task.ItemBarcode;

#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strOperText + " ...");
            stop.BeginLoop();
            try
            {
#endif
            stop.SetMessage(strOperText + " ...");

            string strError = "";

            string strReaderRecord = "";
            string strConfirmItemRecPath = null;

            bool bRenew = false;
            if (task.Action == "renew" || task.Action == "verify_renew")
                bRenew = true;

            if (task.Action == "renew")
                task.ReaderBarcode = "";
            else
            {
                if (string.IsNullOrEmpty(task.ReaderBarcode) == true)
                {
                    strError = "证条码号为空，无法进行借书操作";
                    task.ErrorInfo = strError;
                    goto ERROR1;
                }
            }

        REDO:
            string[] aDupPath = null;
            string[] item_records = null;
            string[] reader_records = null;
            string[] biblio_records = null;
            string strOutputReaderBarcode = "";

            BorrowInfo borrow_info = null;

            // item返回的格式
            string strItemReturnFormats = "";

            if (this.Container.MainForm.ChargingNeedReturnItemXml == true)
            {
                if (String.IsNullOrEmpty(strItemReturnFormats) == false)
                    strItemReturnFormats += ",";
                strItemReturnFormats += "xml" + GetPostFix();
            }

            // biblio返回的格式
            string strBiblioReturnFormats = "";

            // 读者返回格式
            string strReaderFormatList = "";
            bool bName = false; // 是否直接取得读者姓名，而不要获得读者 XML
            if (this.Container.MainForm.Version >= 2.24)
            {
                strReaderFormatList = this.Container.PatronRenderFormat + ",summary";
                bName = true;
            }
            else
                strReaderFormatList = this.Container.PatronRenderFormat + ",xml" + GetPostFix();

            string strStyle = "reader";

            if (this.Container.MainForm.ChargingNeedReturnItemXml)
                strStyle += ",item";

            //if (this.Container.MainForm.TestMode == true)
            //    strStyle += ",testmode";

            long lRet = Channel.Borrow(
stop,
bRenew,
task.ReaderBarcode,
task.ItemBarcode,
strConfirmItemRecPath,
false,
null,   // this.OneReaderItemBarcodes,
strStyle,
strItemReturnFormats,
out item_records,
strReaderFormatList,    // this.Container.PatronRenderFormat + ",xml" + GetPostFix(),
out reader_records,
strBiblioReturnFormats,
out biblio_records,
out aDupPath,
out strOutputReaderBarcode,
out borrow_info,
out strError);
            task.ErrorInfo = strError;

            if (reader_records != null && reader_records.Length > 0)
                strReaderRecord = reader_records[0];

            // 刷新读者信息
            if (this.Container.IsCardMode == true)
            {
                if (String.IsNullOrEmpty(strReaderRecord) == false)
                    this.Container.SetReaderCardString(strReaderRecord);
            }
            else
            {
                if (String.IsNullOrEmpty(strReaderRecord) == false)
                    this.Container.SetReaderHtmlString(ReplaceMacro(strReaderRecord));
            }

            string strItemXml = "";
            if (this.Container.MainForm.ChargingNeedReturnItemXml == true
                && item_records != null)
            {
                Debug.Assert(item_records != null, "");

                if (item_records.Length > 0)
                {
                    // xml总是在最后一个
                    strItemXml = item_records[item_records.Length - 1];
                }
            }

            if (lRet == -1)
                goto ERROR1;

            DateTime end_time = DateTime.Now;

            string strReaderSummary = "";
            if (reader_records != null && reader_records.Length > 1)
            {
                if (bName == false)
                    strReaderSummary = Global.GetReaderSummary(reader_records[1]);
                else
                    strReaderSummary = reader_records[1];
            }

#if NO
                string strBiblioSummary = "";
                if (biblio_records != null && biblio_records.Length > 1)
                    strBiblioSummary = biblio_records[1];
#endif

            task.ReaderName = strReaderSummary;
            // task.ItemSummary = strBiblioSummary;
            this.Container.AsynFillItemSummary(task.ItemBarcode,
                strConfirmItemRecPath,
                task);

            this.Container.MainForm.OperHistory.BorrowAsync(
this.Container,
bRenew,
strOutputReaderBarcode,
task.ItemBarcode,
strConfirmItemRecPath,
strReaderSummary,
strItemXml,
borrow_info,
start_time,
end_time);

            /*
            lRet = 1;
            task.ErrorInfo = "asdf a asdf asdf as df asdf as f a df asdf a sdf a sdf asd f asdf a sdf as df";
            */

            if (lRet == 1)
            {
                // 黄色状态
                task.Color = "yellow";
            }
            else
            {
                // 绿色状态
                task.Color = "green";
            }

            // this.m_strCurrentBarcode = strBarcode;
            task.State = "finish";
            // 兑现显示
            this.Container.DisplayTask("refresh_and_visible", task);
            this.Container.SetColorList();
#if NO
                if (stop != null && stop.State != 0)
                    return true;
                return false;
#endif

            {
                BorrowCompleteEventArgs e1 = new BorrowCompleteEventArgs();
                e1.Action = task.Action;
                e1.ItemBarcode = task.ItemBarcode;
                e1.ReaderBarcode = strOutputReaderBarcode;
                this.Container.TriggerBorrowComplete(e1);
            }
            return;
        ERROR1:
            task.State = "error";
            task.Color = "red";
            // this.Container.SetReaderRenderString(strError);
            // 兑现显示
            this.Container.DisplayTask("refresh_and_visible", task);
            this.Container.SetColorList();
#if NO
                if (stop != null && stop.State != 0)
                    return true;
                return false;
#endif
            return;

#if NO
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
#endif

        }

        // 还书
        void Return(ChargingTask task)
        {
            DateTime start_time = DateTime.Now;

            task.State = "begin";
            task.Color = "purple";  //  "light";
            this.Container.DisplayTask("refresh", task);
            this.Container.SetColorList();

            string strOperText = task.ReaderBarcode + " 还 " + task.ItemBarcode;

#if NO
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strOperText + " ...");
            stop.BeginLoop();
            try
            {
#endif
            stop.SetMessage(strOperText + " ...");

                string strError = "";

                string strReaderRecord = "";
                string strConfirmItemRecPath = null;

                string strAction = task.Action;
                string strReaderBarcode = task.ReaderBarcode;
                if (task.Action == "verify_return")
                {
                    if (string.IsNullOrEmpty(strReaderBarcode) == true)
                    {
                        strError = "尚未输入读者证条码号";
                        task.ErrorInfo = strError;
                        goto ERROR1;
                    }
                    strAction = "return";
                }
                else if (task.Action == "verify_lost")
                {
                    if (string.IsNullOrEmpty(strReaderBarcode) == true)
                    {
                        strError = "尚未输入读者证条码号";
                        task.ErrorInfo = strError;
                        goto ERROR1;
                    }
                    strAction = "lost";
                }
                else
                {
                    strReaderBarcode = "";
                }
            REDO:
                string[] aDupPath = null;
                string[] item_records = null;
                string[] reader_records = null;
                string[] biblio_records = null;
                string strOutputReaderBarcode = "";

                ReturnInfo return_info = null;

                // item返回的格式
                string strItemReturnFormats = "";

                if (this.Container.MainForm.ChargingNeedReturnItemXml == true)
                {
                    if (String.IsNullOrEmpty(strItemReturnFormats) == false)
                        strItemReturnFormats += ",";
                    strItemReturnFormats += "xml" + GetPostFix();
                }

                // biblio返回的格式
                string strBiblioReturnFormats = "";

                // 读者返回格式
                string strReaderFormatList = "";
                bool bName = false; // 是否直接取得读者姓名，而不要获得读者 XML
                if (this.Container.MainForm.Version >= 2.24)
                {
                    strReaderFormatList = this.Container.PatronRenderFormat + ",summary";
                    bName = true;
                }
                else
                    strReaderFormatList = this.Container.PatronRenderFormat + ",xml" + GetPostFix();



                string strStyle = "reader";

                if (this.Container.MainForm.ChargingNeedReturnItemXml)
                    strStyle += ",item";

                //if (this.Container.MainForm.TestMode == true)
                //    strStyle += ",testmode";

                long lRet = Channel.Return(
                    stop,
                    strAction,
                    strReaderBarcode,
                    task.ItemBarcode,
                    strConfirmItemRecPath,
                    false,
                    strStyle,   // this.NoBiblioAndItemInfo == false ? "reader,item,biblio" : "reader",
                    strItemReturnFormats,
                    out item_records,
                    strReaderFormatList,    // this.Container.PatronRenderFormat + ",xml" + GetPostFix(), // "html",
                    out reader_records,
                    strBiblioReturnFormats,
                    out biblio_records,
                    out aDupPath,
                    out strOutputReaderBarcode,
                    out return_info,
                    out strError);

                if (lRet != 0)
                    task.ErrorInfo = strError;

#if NO
                if (return_info != null)
                {
                    strLocation = StringUtil.GetPureLocation(return_info.Location);
                }
#endif

                if (reader_records != null && reader_records.Length > 0)
                    strReaderRecord = reader_records[0];

                // 刷新读者信息
                if (this.Container.IsCardMode == true)
                {
                    if (String.IsNullOrEmpty(strReaderRecord) == false)
                        this.Container.SetReaderCardString(strReaderRecord);
                }
                else
                {
                    if (String.IsNullOrEmpty(strReaderRecord) == false)
                        this.Container.SetReaderHtmlString(ReplaceMacro(strReaderRecord));
                }

                string strItemXml = "";
                if (this.Container.MainForm.ChargingNeedReturnItemXml == true
                    && item_records != null)
                {
                    Debug.Assert(item_records != null, "");

                    if (item_records.Length > 0)
                    {
                        // xml总是在最后一个
                        strItemXml = item_records[item_records.Length - 1];
                    }
                }

                if (lRet == -1)
                    goto ERROR1;

                string strReaderSummary = "";
                if (reader_records != null && reader_records.Length > 1)
                {
                    if (bName == false)
                        strReaderSummary = Global.GetReaderSummary(reader_records[1]);
                    else
                        strReaderSummary = reader_records[1];
                }

#if NO
                string strBiblioSummary = "";
                if (biblio_records != null && biblio_records.Length > 1)
                    strBiblioSummary = biblio_records[1];
#endif

                task.ReaderName = strReaderSummary;
                // task.ItemSummary = strBiblioSummary;
                this.Container.AsynFillItemSummary(task.ItemBarcode,
                    strConfirmItemRecPath,
                    task);

                if (string.IsNullOrEmpty(task.ReaderBarcode) == true)
                    task.ReaderBarcode = strOutputReaderBarcode;

                DateTime end_time = DateTime.Now;

                this.Container.MainForm.OperHistory.ReturnAsync(
                    this.Container,
                    task.Action == "lost" || task.Action == "verify_lost",
                    strOutputReaderBarcode, // this.textBox_readerBarcode.Text,
                    task.ItemBarcode,
                    strConfirmItemRecPath,
                    strReaderSummary,
                    strItemXml,
                    return_info,
                    start_time,
                    end_time);

                if (lRet == 1)
                {
                    // 黄色状态
                    task.Color = "yellow";
                }
                else
                {
                    // 绿色状态
                    task.Color = "green";
                }

                // this.m_strCurrentBarcode = strBarcode;
                task.State = "finish";
                // 兑现显示
                this.Container.DisplayTask("refresh_and_visible", task);
                this.Container.SetColorList();
#if NO
                if (stop != null && stop.State != 0)
                    return true;
                return false;
#endif
                {
                    BorrowCompleteEventArgs e1 = new BorrowCompleteEventArgs();
                    e1.Action = task.Action;
                    e1.ItemBarcode = task.ItemBarcode;
                    e1.ReaderBarcode = strOutputReaderBarcode;
                    this.Container.TriggerBorrowComplete(e1);
                }
                return;

            ERROR1:
                task.State = "error";
                task.Color = "red";
                // this.Container.SetReaderRenderString(strError);
                // 兑现显示
                this.Container.DisplayTask("refresh_and_visible", task);
                this.Container.SetColorList();
#if NO
                if (stop != null && stop.State != 0)
                    return true;
                return false;
#endif
                return;
#if NO
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
#endif
        }


        public void Close()
        {
            if (stop != null)
                stop.DoStop();
            this.eventClose.Set();
            Stopped = true;
        }

        public void Clear()
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                this._tasks.Clear();
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

        // 清除指定的那些任务
        public void ClearTasks(List<ChargingTask> tasks)
        {
            if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                foreach (ChargingTask task in tasks)
                {
                    this._tasks.Remove(task);
                }
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }
        }

#if NO
        public bool Stopped
        {
            get
            {
                return m_bStopThread;
            }
        }

        void StopThread(bool bForce)
        {
            // 如果以前在做，立即停止
            if (stop != null)
                stop.DoStop();

            m_bStopThread = true;
            this.eventClose.Set();

            if (bForce == true)
            {
                if (this._thread != null)
                {
                    if (!this._thread.Join(2000))
                        this._thread.Abort();
                    this._thread = null;
                }
            }
        }

        public void BeginThread()
        {
            // 如果以前在做，立即停止
            StopThread(true);

            this.eventActive.Set();
            this.eventClose.Reset(); 

            this._thread = new Thread(new ThreadStart(this.ThreadMain));
            this._thread.Start();
        }

        void ThreadMain()
        {
            m_bStopThread = false;

            try
            {

                WaitHandle[] events = new WaitHandle[2];

                events[0] = eventClose;
                events[1] = eventActive;

                while (m_bStopThread == false)
                {
                    int index = 0;
                    try
                    {
                        index = WaitHandle.WaitAny(events, PerTime, false);
                    }
                    catch (System.Threading.ThreadAbortException /*ex*/)
                    {
                        break;
                    }

                    if (index == WaitHandle.WaitTimeout)
                    {
                        // 超时
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();

                    }
                    else if (index == 0)
                    {
                        break;
                    }
                    else
                    {
                        // 得到激活信号
                        eventActive.Reset();
                        Worker();
                        eventActive.Reset();    // 2013/11/23 只让堵住的时候发挥作用
                    }
                }

                return;
            }
            finally
            {
                m_bStopThread = true;
                _thread = null;
            }
        }


        public void Activate()
        {
            eventActive.Set();
        }

#endif

        // 加入一个任务到列表中
        public void AddTask(ChargingTask task)
        {
            task.Color = "black";   // 表示等待处理
            if (this.m_lock.TryEnterWriteLock(500) == false)   // m_nLockTimeout
                throw new LockException("锁定尝试中超时");
            try
            {
                this._tasks.Add(task);
            }
            finally
            {
                this.m_lock.ExitWriteLock();
            }

            // 触发任务开始执行
            this.Activate();

            // 兑现显示
            this.Container.DisplayTask("add", task);
            this.Container.SetColorList();
        }

        public void RemoveTask(ChargingTask task, bool bLock = true)
        {
            if (bLock == true)
            {
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new LockException("锁定尝试中超时");
            }
            try
            {
                this._tasks.Remove(task);
            }
            finally
            {
                if (bLock == true)
                    this.m_lock.ExitWriteLock();
            } 
            
            // 兑现显示
            this.Container.DisplayTask("remove", task);
            this.Container.SetColorList();
        }
    }

    // 一个出纳任务
    class ChargingTask
    {
        public string ID = "";  // 任务 ID。这是在任务之间唯一的一个字符串，用于查询和定位任务
        public string ReaderBarcode = "";
        public string ItemBarcode = "";
        public string Action = "";  // load_reader_info borrow return lost renew
        public string State = "";   // 空 / begin / finish / error
        public string Color = "";   // 颜色
        public string ErrorInfo = "";   // 出错信息

        public string ReaderName = "";  // 读者姓名
        public string ItemSummary = ""; // 册的书目摘要

        public string ReaderXml = "";   // 读者记录 XML

        const int IMAGEINDEX_WAITING = 0;
        const int IMAGEINDEX_FINISH = 1;
        const int IMAGEINDEX_ERROR = 2;
        const int IMAGEINDEX_INFORMATION = 3;

        public void RefreshPatronCardDisplay(DpRow row)
        {
            if (row.Count < 3)
                return;

            DpCell cell = row[2];
            if (this.Action == "load_reader_info")
                cell.OwnerDraw = true;

            cell.Relayout();
        }

        public void RefreshDisplay(DpRow row)
        {
            // 初始化列
            if (row.Count == 0)
            {
                // 色条
                DpCell cell = new DpCell();
                row.Add(cell);

                // 状态
                cell = new DpCell();
                cell.ImageIndex = -1;
                row.Add(cell);

                // 内容
                cell = new DpCell();
                row.Add(cell);
            }

            bool bStateText = false;

            // 状态
            // row[0].Text = this.State;
            DpCell state_cell = row[1];
            if (this.State == "begin")
            {
                if (bStateText == true)
                    state_cell.Text = "请求中";
                state_cell.ImageIndex = IMAGEINDEX_WAITING;
            }
            else if (this.State == "error")
            {
                if (bStateText == true)
                    state_cell.Text = "出错";
                state_cell.ImageIndex = IMAGEINDEX_ERROR;
            }
            else if (this.State == "finish")
            {
                if (bStateText == true)
                    state_cell.Text = "完成";
                state_cell.ImageIndex = IMAGEINDEX_FINISH;
            }
            else
            {
                if (bStateText == true)
                    state_cell.Text = "未处理";
                state_cell.ImageIndex = -1;
            }

            string strText = "";
            // 内容
            if (this.Action == "load_reader_info")
                strText = "装载读者信息 " + this.ReaderBarcode;
            else if (this.Action == "borrow")
            {
                strText = GetOperText("借");
            }
            else if (this.Action == "return")
            {
                strText = GetOperText("还");
            }
            else if (this.Action == "verify_return")
            {
                strText = GetOperText("(验证)还");
            }
            else if (this.Action == "lost")
            {
                strText = GetOperText("丢失");
            }
            else if (this.Action == "verify_lost")
            {
                strText = GetOperText("(验证)丢失");
            }
            else if (this.Action == "renew")
            {
                strText = GetOperText("续借");
            }
            else if (this.Action == "verify_renew")
            {
                strText = GetOperText("(验证)续借");
            }

            if (string.IsNullOrEmpty(this.ErrorInfo) == false)
                strText += "\r\n===\r\n" + this.ErrorInfo;

            row[2].Text = strText;

            row.BackColor = SystemColors.Window;
            row.ForeColor = SystemColors.WindowText;

            DpCell color_cell = row[0];
            // row.BackColor = System.Drawing.Color.Transparent;
            if (this.Color == "red")
            {
                color_cell.BackColor = System.Drawing.Color.Red;
                // row.ForeColor = System.Drawing.Color.White;
            }
            else if (this.Color == "green")
            {
                color_cell.BackColor = System.Drawing.Color.Green;
                // row.ForeColor = System.Drawing.Color.White;
            }
            else if (this.Color == "yellow")
            {
                row.BackColor = System.Drawing.Color.Yellow;
                color_cell.BackColor = System.Drawing.Color.Transparent;
                // row.ForeColor = System.Drawing.Color.Black;
            }
            else if (this.Color == "light")
            {
                color_cell.BackColor = System.Drawing.Color.LightGray;
            }
            else if (this.Color == "purple")
            {
                color_cell.BackColor = System.Drawing.Color.Purple;
            }
            else if (this.Color == "black")
            {
                color_cell.BackColor = System.Drawing.Color.Purple;
                row.BackColor = System.Drawing.Color.Black;
                row.ForeColor = System.Drawing.Color.LightGray;
            }
            else
            {
                // color_cell.BackColor = System.Drawing.Color.Transparent;
                color_cell.BackColor = System.Drawing.Color.White;
            }
        }

        string GetOperText(string strOperName)
        {
            string strSummary = "";
            if (string.IsNullOrEmpty(this.ItemSummary) == false)
                strSummary = "\r\n---\r\n" + this.ItemSummary;
            if (string.IsNullOrEmpty(this.ReaderBarcode) == false)
                return this.ReaderBarcode + " " + this.ReaderName + " " + strOperName + " " + this.ItemBarcode + strSummary;
            else
                return strOperName + " " + this.ItemBarcode + strSummary;

        }

        // 任务是否完成
        // error finish 都是完成状态
        public bool Compeleted
        {
            get
            {
                if (this.State == "error" || this.State == "finish")
                    return true;
                return false;
            }
        }
    }

    // 额外的通道
    public class ExternalChannel
    {
        public MainForm MainForm = null;

        public DigitalPlatform.Stop stop = null;

        public LibraryChannel Channel = new LibraryChannel();

        bool _doEvents = false;

        public void Initial(MainForm main_form,
            bool bDoEvents = false)
        {
            this._doEvents = bDoEvents;
            this.MainForm = main_form;

            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            this.Channel.Idle -= new IdleEventHandler(Channel_Idle);
            this.Channel.Idle += new IdleEventHandler(Channel_Idle);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            return;
        }

        void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            MainForm.Channel_AfterLogin(this, e);
        }

        void Channel_Idle(object sender, IdleEventArgs e)
        {
            e.bDoEvents = this._doEvents;
        }

        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            MainForm.Channel_BeforeLogin(this, e);
        }

        public void Close()
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            if (this.Channel != null)
            {
                this.Channel.Close();
                this.Channel = null;
            }
        }

        public void PrepareSearch(string strText)
        {
            if (stop != null)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial(strText);
                stop.BeginLoop();
            }
        }

        public void EndSearch()
        {
            if (stop != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
    }


}
