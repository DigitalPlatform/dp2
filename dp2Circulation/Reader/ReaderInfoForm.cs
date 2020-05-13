﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Web;
using System.Threading.Tasks;
using System.Drawing.Imaging;

using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Drawing;
using DigitalPlatform.Interfaces;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Script;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 读者信息管理窗口
    /// </summary>
    public partial class ReaderInfoForm : MyForm
    {
        int m_nChannelInUse = 0; // >0表示通道正在被使用

        Commander commander = null;

        const int WM_NEXT_RECORD = API.WM_USER + 200;
        const int WM_PREV_RECORD = API.WM_USER + 201;
        const int WM_LOAD_RECORD = API.WM_USER + 202;
        const int WM_DELETE_RECORD = API.WM_USER + 203;
        const int WM_HIRE = API.WM_USER + 204;
        const int WM_SAVETO = API.WM_USER + 205;
        const int WM_SAVE_RECORD = API.WM_USER + 206;
        const int WM_SAVE_RECORD_BARCODE = API.WM_USER + 207;
        const int WM_SAVE_RECORD_FORCE = API.WM_USER + 208;
        const int WM_FOREGIFT = API.WM_USER + 210;
        const int WM_RETURN_FOREGIFT = API.WM_USER + 211;
        const int WM_SET_FOCUS = API.WM_USER + 212;

        WebExternalHost m_webExternalHost = new WebExternalHost();

        WebExternalHost m_chargingInterface = new WebExternalHost();

        string m_strSetAction = "new";  // new / change 之一

        SelectedTemplateCollection selected_templates = new SelectedTemplateCollection();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderInfoForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 当前读者证条码号
        /// </summary>
        public string ReaderBarcode
        {
            get
            {
                return this.toolStripTextBox_barcode.Text;  //  this.textBox_readerBarcode.Text;
            }
            set
            {
                this.toolStripTextBox_barcode.Text = value; //  this.textBox_readerBarcode.Text = value;
            }
        }

        // 外部使用
        /// <summary>
        /// 读者信息编辑控件
        /// </summary>
        public ReaderEditControl ReaderEditControl
        {
            get
            {
                return this.readerEditControl1;
            }
        }

        private void ReaderInfoForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);

#if NO
                // 窗口打开时初始化
                this.m_bSuppressScriptErrors = !Program.MainForm.DisplayScriptErrorDialog;
#endif
            }

#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");

            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.readerEditControl1.SetReadOnly("librarian");

            this.readerEditControl1.GetValueTable += new GetValueTableEventHandler(readerEditControl1_GetValueTable);

            this.readerEditControl1.Initializing = false;   // 如果没有此句，一开始在空模板上修改就不会变色

            //
            this.binaryResControl1.ContentChanged -= new ContentChangedEventHandler(binaryResControl1_ContentChanged);
            this.binaryResControl1.ContentChanged += new ContentChangedEventHandler(binaryResControl1_ContentChanged);

            this.binaryResControl1.GetChannel -= binaryResControl1_GetChannel;
            this.binaryResControl1.GetChannel += binaryResControl1_GetChannel;

            this.binaryResControl1.ReturnChannel -= binaryResControl1_ReturnChannel;
            this.binaryResControl1.ReturnChannel += binaryResControl1_ReturnChannel;

            // this.binaryResControl1.Channel = this.Channel;
            this.binaryResControl1.Stop = this.stop;

            // webBrowser_readerInfo
            this.m_webExternalHost.Initial(// Program.MainForm, 
                this.webBrowser_readerInfo);
            this.m_webExternalHost.GetLocalPath -= new GetLocalFilePathEventHandler(m_webExternalHost_GetLocalPath);
            this.m_webExternalHost.GetLocalPath += new GetLocalFilePathEventHandler(m_webExternalHost_GetLocalPath);
            this.webBrowser_readerInfo.ObjectForScripting = this.m_webExternalHost;

            // webBrowser_borrowHistory
            this.m_chargingInterface.Initial(// Program.MainForm, 
                this.webBrowser_borrowHistory);
            this.m_chargingInterface.GetLocalPath -= new GetLocalFilePathEventHandler(m_webExternalHost_GetLocalPath);
            this.m_chargingInterface.GetLocalPath += new GetLocalFilePathEventHandler(m_webExternalHost_GetLocalPath);
            this.m_chargingInterface.CallFunc += m_chargingInterface_CallFunc;
            this.webBrowser_borrowHistory.ObjectForScripting = this.m_chargingInterface;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            string strSelectedTemplates = Program.MainForm.AppInfo.GetString(
    "readerinfo_form",
    "selected_templates",
    "");
            if (String.IsNullOrEmpty(strSelectedTemplates) == false)
            {
                selected_templates.Build(strSelectedTemplates);
            }

            LoadExternalFields();

            ClearBorrowHistoryPage();
            ClearQrCodePage();

            API.PostMessage(this.Handle, WM_SET_FOCUS, 0, 0);
        }

        void binaryResControl1_ReturnChannel(object sender, ReturnChannelEventArgs e)
        {
            this.stop.EndLoop();
            this.stop.OnStop -= new StopEventHandler(this.DoStop);
            this.ReturnChannel(e.Channel);
        }

        void binaryResControl1_GetChannel(object sender, GetChannelEventArgs e)
        {
            e.Channel = this.GetChannel();
            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.BeginLoop();
        }

        void LoadExternalFields()
        {
            string strError = "";
            // 从配置文件装载字段配置，初始化这些字段
            string strFileName = Path.Combine(Program.MainForm.UserDir, "patron_extend.xml");
            if (File.Exists(strFileName) == true)
            {
                int nRet = this.readerEditControl1.LoadConfig(strFileName,
                    out strError);
                if (nRet == -1)
                    this.ShowMessage(strError, "red", true);
            }
        }

        void m_webExternalHost_GetLocalPath(object sender, GetLocalFilePathEventArgs e)
        {
            if (e.Name == "PatronCardPhoto")
            {
                List<ListViewItem> items = this.binaryResControl1.FindItemByUsage("cardphoto");
                if (items.Count > 0)
                {
                    string strError = "";
                    string strLocalPath = "";
                    // return:
                    //      -1  出错
                    //      0   不属于修改或者创建后尚未上载的情况
                    //      1   成功
                    int nRet = this.binaryResControl1.GetUnuploadFilePath(items[0],
            out strLocalPath,
            out strError);
                    e.LocalFilePath = strLocalPath;
                    // 注：本地路径""表示这种类型的对象有，但是没有本地文件。也就是说已经上载，需要从服务器找
                }
                else
                {
                    e.LocalFilePath = null; // null表示根本没有这种类型的对象
                }
            }
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost.ChannelInUse;
        }

        void binaryResControl1_ContentChanged(object sender, ContentChangedEventArgs e)
        {
        }

        public bool Changed
        {
            get
            {
                if (this.ReaderXmlChanged)
                    return true;
                if (this.ObjectChanged)
                    return true;
                return false;
            }
            set
            {
                this.ReaderXmlChanged = value;
                this.ObjectChanged = value;
            }
        }

        // 
        /// <summary>
        /// 对象信息是否被改变
        /// </summary>
        public bool ObjectChanged
        {
            get
            {
                if (this.binaryResControl1 != null)
                    return this.binaryResControl1.Changed;

                return false;
            }
            set
            {
                if (this.binaryResControl1 != null)
                    this.binaryResControl1.Changed = value;
            }
        }

        /// <summary>
        /// 读者记录 XML 是否被改变
        /// </summary>
        public bool ReaderXmlChanged
        {
            get
            {
                if (this.readerEditControl1 != null)
                {
                    // 如果object id有所改变，那么即便XML记录没有改变，那最后的合成XML也发生了改变
                    if (this.binaryResControl1 != null)
                    {
                        if (this.binaryResControl1.IsIdUsageChanged() == true)
                            return true;
                    }

                    return this.readerEditControl1.Changed;
                }

                return false;
            }
            set
            {
                if (this.readerEditControl1 != null)
                    this.readerEditControl1.Changed = value;
            }
        }

        /*
        // 2008/10/28
        void NewExternal()
        {
            if (this.m_webExternalHost != null)
            {
                this.m_webExternalHost.Close();
                this.webBrowser_readerInfo.ObjectForScripting = null;
            }

            this.m_webExternalHost = new WebExternalHost();
            this.m_webExternalHost.Initial(Program.MainForm);
            this.webBrowser_readerInfo.ObjectForScripting = this.m_webExternalHost;
        }*/

        void readerEditControl1_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void ReaderInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
#endif
            if (_inFingerprintCall > 0)
            {
                // TODO: 增加一个参数，表示不需要显示“已取消操作”提示对话框
                var task = CancelReadFingerprintString();
                /*
                try
                {
                    GetFingerprintStringResult result = await CancelReadFingerprintString();
                    if (result.Value == -1)
                    {
                        ShowMessageBox(result.ErrorInfo);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessageBox(ex.Message);
                }
                */
            }


            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                this.Invoke((Action)(() =>
                {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                "ReaderInfoForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }));
            }

        }

        private void ReaderInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.commander.Destroy();

            if (this.m_webExternalHost != null)
                this.m_webExternalHost.Destroy();

            if (this.m_chargingInterface != null)
                this.m_chargingInterface.Destroy();
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                string strSelectedTemplates = selected_templates.Export();
                Program.MainForm.AppInfo.SetString(
                    "readerinfo_form",
                    "selected_templates",
                    strSelectedTemplates);
            }

            this.readerEditControl1.GetValueTable -= new GetValueTableEventHandler(readerEditControl1_GetValueTable);

#if NO
            MainForm.AppInfo.SaveMdiChildFormStates(this,
"mdi_form_state");
#endif

        }

        /*
        public string Path
        {
            get
            {
                return m_strPath;
            }
            set
            {
                m_strPath = value;
            }
        }
         */

#if NO
        void SetXmlToWebbrowser(WebBrowser webbrowser,
            string strXml)
        {
            string strTargetFileName = MainForm.DataDir + "\\xml.xml";

            /*
            StreamWriter sw = new StreamWriter(strTargetFileName,
                false,	// append
                System.Text.Encoding.UTF8);
            sw.Write(strXml);
            sw.Close();
             * */
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strTargetFileName = MainForm.DataDir + "\\xml.txt";
                StreamWriter sw = new StreamWriter(strTargetFileName,
    false,	// append
    System.Text.Encoding.UTF8);
                sw.Write("XML内容装入DOM时出错: " + ex.Message + "\r\n\r\n" + strXml);
                sw.Close();
                webbrowser.Navigate(strTargetFileName);

                return;
            }

            dom.Save(strTargetFileName);
            webbrowser.Navigate(strTargetFileName);
        }
#endif

        public void AsyncLoadRecord(string strBarcode)
        {
            this.toolStripTextBox_barcode.Text = strBarcode;

            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_LOAD_RECORD);
        }

        // 根据读者证条码号，装入读者记录
        // parameters:
        //      bForceLoad  在发生重条码的情况下是否强行装入第一条
        /// <summary>
        /// 根据读者证条码号，装入读者记录
        /// </summary>
        /// <param name="strBarcode">读者证条码号</param>
        /// <param name="bForceLoad">在发生重条码的情况下是否强行装入第一条</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int LoadRecord(string strBarcode,
            bool bForceLoad)
        {
            string strError = "";
            int nRet = 0;

#if NO
            // 2013/12/4
            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();
#endif

            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
"当前有信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要根据证条码号重新装载内容? ",
"ReaderInfoForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return 0;   // cancelled
            }

            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                strError = "通道已经被占用。请稍后重试";
                goto ERROR1;
            }
            try
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在装载读者记录 ...");
                stop.BeginLoop();

                EnableControls(false);

                this.readerEditControl1.Clear();
#if NO
                Global.ClearHtmlPage(this.webBrowser_readerInfo,
                    Program.MainForm.DataDir);
#endif
                ClearReaderHtmlPage();
                this.binaryResControl1.Clear();

                this.ClearBorrowHistoryPage();
                this.ClearQrCodePage();

                try
                {
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";
                    int nRedoCount = 0;

                    REDO:
                    stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        strBarcode,
                        "xml,html",
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                        goto ERROR1;

                    if (lRet > 1)
                    {
#if NO
                        if (bForceLoad == true)
                        {
                            strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，但仍装入其中第一条读者记录。\r\n\r\n这是一个严重错误，请系统管理员尽快排除。";
                            MessageBox.Show(this, strError);    // 警告后继续装入第一条 
                        }
                        else
                        {
                            strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                            goto ERROR1;    // 当出错处理
                        }
#endif
                        // 如果重试后依然发生重复
                        if (nRedoCount > 0)
                        {
                            if (bForceLoad == true)
                            {
                                strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，但仍装入其中第一条读者记录。\r\n\r\n这是一个严重错误，请系统管理员尽快排除。";
                                MessageBox.Show(this, strError);    // 警告后继续装入第一条 
                            }
                            else
                            {
                                strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                                goto ERROR1;    // 当出错处理
                            }
                        }

                        SelectPatronDialog dlg = new SelectPatronDialog();

                        dlg.Overflow = StringUtil.SplitList(strOutputRecPath).Count < lRet;
                        nRet = dlg.Initial(
                            // Program.MainForm,
                            StringUtil.SplitList(strOutputRecPath),
                            "请选择一个读者记录",
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_SelectPatronDialog_state");
                        dlg.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(dlg);

                        if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            return 0;

                        strBarcode = "@path:" + dlg.SelectedRecPath;   // 2015/11/16 // .SelectedBarcode;
                        nRedoCount++;
                        goto REDO;
                    }

                    this.ReaderBarcode = strBarcode;

                    /*
                    this.RecPath = strRecPath;

                    this.Timestamp = baTimestamp;

                    // 保存刚获得的记录
                    this.OldRecord = strXml;
                     */


                    if (results == null || results.Length < 2)
                    {
                        strError = "返回的results不正常。";
                        goto ERROR1;
                    }

                    string strXml = "";
                    string strHtml = "";
                    strXml = results[0];
                    strHtml = results[1];

                    nRet = this.readerEditControl1.SetData(
                        strXml,
                        strOutputRecPath,
                        baTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 接着装入对象资源
                    {
                        nRet = this.binaryResControl1.LoadObject(
                            this.Channel,
                            strOutputRecPath,    // 2008/11/2 changed
                            strXml,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                            // return -1;
                        }
                    }

                    /*
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strXml);
                     * */
                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
                        Program.MainForm.DataDir,
                        "xml",
                        strXml);

                    this.m_strSetAction = "change";

                    /*
                    lRet = Channel.GetReaderInfo(
                        stop,
                        strBarcode,
                        "html",
                        out strHtml,
                        out strRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        ChargingForm.SetHtmlString(this.webBrowser_readerInfo,
    "装载读者记录发生错误: " + strError);

                    }
                    else
                    {
                        ChargingForm.SetHtmlString(this.webBrowser_readerInfo,
                            strHtml,
        Program.MainForm.DataDir,
        "readerinfoform_reader");
                    }
                     * */

#if NO
                    // 2013/12/21
                    this.m_webExternalHost.StopPrevious();
                    this.webBrowser_readerInfo.Stop();

                    Global.SetHtmlString(this.webBrowser_readerInfo,
        strHtml,
        Program.MainForm.DataDir,
        "readerinfoform_reader");
#endif
                    this.SetReaderHtmlString(strHtml);

                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
            finally
            {
                this.m_nChannelInUse--;
            }

            tabControl_readerInfo_SelectedIndexChanged(this, new EventArgs());
            return 1;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        void ClearReaderHtmlPage()
        {
            // 2013/12/21
            this.m_webExternalHost.StopPrevious();
            // this.webBrowser_readerInfo.Stop();

            Global.ClearHtmlPage(this.webBrowser_readerInfo,
    Program.MainForm.DataDir);

            this.m_chargingInterface.StopPrevious();
        }

        void SetReaderHtmlString(string strHtml)
        {
#if NO
            // 2013/12/21
            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            Global.SetHtmlString(this.webBrowser_readerInfo,
strHtml,
Program.MainForm.DataDir,
"readerinfoform_reader");
#endif
            this.m_webExternalHost.SetHtmlString(strHtml,
                "readerinfoform_reader");
        }

        // 根据读者记录路径，装入读者记录
        // parameters:
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 根据读者记录路径，装入读者记录
        /// </summary>
        /// <param name="strRecPath">读者记录路径</param>
        /// <param name="strPrevNextStyle">方向</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int LoadRecordByRecPath(string strRecPath,
            string strPrevNextStyle = "")
        {
            string strError = "";

#if NO
            // 2013/12/4
            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();
#endif

            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
"当前有信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要根据记录路径重新装载内容? ",
"ReaderInfoForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return 0;   // cancelled

            }

            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                strError = "通道已经被占用。请稍后重试";
                goto ERROR1;
            }
            try
            {
                bool bPrevNext = false;

                if (String.IsNullOrEmpty(strPrevNextStyle) == false)
                {
                    strRecPath += "$" + strPrevNextStyle.ToLower();
                    bPrevNext = true;
                }

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();

                EnableControls(false);

                // NewExternal();

                if (bPrevNext == false)
                {
                    this.readerEditControl1.Clear();
#if NO
                    Global.ClearHtmlPage(this.webBrowser_readerInfo,
                        Program.MainForm.DataDir);
#endif
                    ClearReaderHtmlPage();

                    this.binaryResControl1.Clear();
                }

                this.ClearBorrowHistoryPage();
                this.ClearQrCodePage();
                try
                {
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    stop.SetMessage("正在装入读者记录 " + strRecPath + " ...");

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        "@path:" + strRecPath,
                        "xml,html",
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (bPrevNext == true)
                        {
                            strError += "\r\n\r\n新记录没有装载，窗口中还保留了装载前的记录";
                        }
                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        if (bPrevNext == true)
                        {
                            strError += "\r\n\r\n新记录没有装载，窗口中还保留了装载前的记录";
                        }
                        goto ERROR1;
                    }

                    if (lRet > 1)   // 不可能发生吧?
                    {
                        strError = "记录路径 " + strRecPath + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                        goto ERROR1;
                    }

                    if (results == null || results.Length < 2)
                    {
                        strError = "返回的results不正常。";
                        goto ERROR1;
                    }

                    string strXml = "";
                    string strHtml = "";
                    strXml = results[0];
                    strHtml = results[1];

                    int nRet = this.readerEditControl1.SetData(
                        strXml,
                        strOutputRecPath,   // strRecPath,
                        baTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 接着装入对象资源
                    {
                        this.binaryResControl1.Clear();
                        nRet = this.binaryResControl1.LoadObject(
                            this.Channel,
                            strOutputRecPath,    // 2008/11/2 changed
                            strXml,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                            // return -1;
                        }
                    }

                    this.ReaderBarcode = this.readerEditControl1.Barcode;

                    /*
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strXml);
                     * */
                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "xml",
    strXml);

                    this.m_strSetAction = "change";

#if NO
                    // 2013/12/21
                    this.m_webExternalHost.StopPrevious();
                    this.webBrowser_readerInfo.Stop();

                    Global.SetHtmlString(this.webBrowser_readerInfo,
        strHtml,
        Program.MainForm.DataDir,
        "readerinfoform_reader");
#endif
                    this.SetReaderHtmlString(strHtml);
                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
            finally
            {
                this.m_nChannelInUse--;
            }

            tabControl_readerInfo_SelectedIndexChanged(this, new EventArgs());
            return 1;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        /*public*/
        void SetMenuItemState()
        {
            // 菜单

            // 工具条按钮

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }

        void EnableSendKey(bool bEnable)
        {
            // 2014/10/12
            if (Program.MainForm == null)
                return;

            if (string.IsNullOrEmpty(Program.MainForm.IdcardReaderUrl) == true)
                return;

            int nRet = 0;
            string strError = "";
            try
            {
                nRet = StartIdcardChannel(
                    Program.MainForm.IdcardReaderUrl,
                    out strError);
                if (nRet == -1)
                    return;

                if (m_idcardObj.SendKeyEnabled != bEnable)
                    m_idcardObj.SendKeyEnabled = bEnable;
            }
            catch
            {
                return;
            }
            finally
            {
                try
                {
                    EndIdcardChannel();
                }
                catch
                {
                }
            }
        }

        private void ReaderInfoForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this.stop);

            SetMenuItemState();

            if (this.DisableIdcardReaderSendkey == true)
            {
                EnableSendKey(false);
            }
            // Debug.WriteLine("Activated");
        }

        private void ReaderInfoForm_Deactivate(object sender, EventArgs e)
        {
            if (this.DisableIdcardReaderSendkey == true)
            {
                EnableSendKey(true);
            }

            // Debug.WriteLine("DeActivated");
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // this.textBox_readerBarcode.Enabled = bEnable;
            // this.button_load.Enabled = bEnable;
            this.toolStrip_load.Enabled = bEnable;


            this.readerEditControl1.Enabled = bEnable;

            if (bEnable == false)
                this.toolStripButton_delete.Enabled = bEnable;
            else
            {
                if (this.readerEditControl1.RecPath != "")
                    this.toolStripButton_delete.Enabled = true;  // 只有具备明确的路径的记录，才能被删除
                else
                    this.toolStripButton_delete.Enabled = false;
            }

            this.toolStripButton_loadFromIdcard.Enabled = bEnable;

            this.toolStripDropDownButton_loadBlank.Enabled = bEnable;
            this.toolStripButton_loadBlank.Enabled = bEnable;

            this.toolStripButton_webCamera.Enabled = bEnable;
            this.toolStripButton_pasteCardPhoto.Enabled = bEnable;

            this.toolStripButton_registerFingerprint.Enabled = bEnable;
            this.toolStripButton_createMoneyRecord.Enabled = bEnable;

            this.toolStripButton_saveTo.Enabled = bEnable;
            //this.toolStripButton_save.Enabled = bEnable;
            this.toolStripSplitButton_save.Enabled = bEnable;

            this.toolStripButton_clearOutofReservationCount.Enabled = bEnable;

            this.toolStripButton_option.Enabled = bEnable;

            this.toolStripDropDownButton_otherFunc.Enabled = bEnable;

            // 2008/10/28
            this.toolStripButton_next.Enabled = bEnable;
            this.toolStripButton_prev.Enabled = bEnable;
        }

        private void toolStripTextBox_barcode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // 回车
                case Keys.Enter:
                case Keys.LineFeed:
                    // toolStripTextBox_barcode.Enabled = false;
                    // toolStripTextBox_barcode.SelectAll();   //
                    toolStripButton_load_Click(sender, new EventArgs());
                    //e.Handled = true;
                    //e.SuppressKeyPress = true;
                    break;
            }
        }

        private void toolStripButton_load_Click(object sender, EventArgs e)
        {
            if (this.toolStripTextBox_barcode.Text == "")
            {
                MessageBox.Show(this, "尚未指定读者证条码号");
                return;
            }

            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_LOAD_RECORD);
        }

#if NO
        private void button_load_Click(object sender, EventArgs e)
        {
            if (this.textBox_readerBarcode.Text == "")
            {
                MessageBox.Show(this, "尚未指定读者证条码号");
                return;
            }

            this.toolStrip1.Enabled = false;

            this.m_webExternalHost.StopPrevious();
                    this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_LOAD_RECORD);
        }

        private void textBox_readerBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_load;
        }

                private void textBox_readerBarcode_TextChanged(object sender, EventArgs e)
        {
            this.UpdateWindowTitle();
        }
#endif

        private void toolStripTextBox_barcode_TextChanged(object sender, EventArgs e)
        {
            this.UpdateWindowTitle();
        }

        void UpdateWindowTitle()
        {
            this.Text = "读者 " + this.toolStripTextBox_barcode.Text; // this.textBox_readerBarcode.Text;
        }

        // 保存配置文件
        int SaveCfgFile(string strCfgFilePath,
            string strContent,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存配置文件 ...");
            stop.BeginLoop();

            try
            {
                stop.SetMessage("正在保存配置文件 " + strCfgFilePath + " ...");

                byte[] output_timestamp = null;
                string strOutputPath = "";

                long lRet = Channel.WriteRes(
                    stop,
                    strCfgFilePath,
                    strContent,
                    true,
                    "",	// style
                    baTimestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
            ERROR1:
            return -1;
        }

#if NO
        int m_nInGetCfgFile = 0;    // 防止GetCfgFile()函数重入


        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetCfgFileContent(string strCfgFilePath,
            out string strContent,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            baOutputTimestamp = null;
            strError = "";
            strContent = "";

            if (m_nInGetCfgFile > 0)
            {
                strError = "GetCfgFile() 重入了";
                return -1;
            }


            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在下载配置文件 ...");
            stop.BeginLoop();

            m_nInGetCfgFile++;

            try
            {
                stop.SetMessage("正在下载配置文件 " + strCfgFilePath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                long lRet = Channel.GetRes(stop,
                    MainForm.cfgCache,
                    strCfgFilePath,
                    strStyle,
                    null,
                    out strContent,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    goto ERROR1;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }

#endif

        // 
        /// <summary>
        /// 保存当前窗口内记录到模板配置文件
        /// </summary>
        public void SaveReaderToTemplate()
        {
            string strError = "";
            this.EnableControls(false);
            try
            {
                // 获得路径行中已经有的读者库名
                string strReaderDbName = Global.GetDbName(this.readerEditControl1.RecPath);

                GetDbNameDlg dlg = new GetDbNameDlg();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.DbType = "reader";
                dlg.DbName = strReaderDbName;
                // dlg.MainForm = Program.MainForm;
                dlg.Text = "请选择目标读者库名";
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                strReaderDbName = dlg.DbName;

                // 下载模板配置文件
                string strContent = "";
                byte[] baTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                int nRet = GetCfgFileContent(strReaderDbName + "/cfgs/template",
                    out strContent,
                    out baTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    goto ERROR1;
                }

                SelectRecordTemplateDlg tempdlg = new SelectRecordTemplateDlg();
                MainForm.SetControlFont(tempdlg, this.Font, false);
                nRet = tempdlg.Initial(
                    true,
                    strContent, out strError);
                if (nRet == -1)
                    goto ERROR1;

                tempdlg.Text = "请选择要修改的模板记录";
                tempdlg.CheckNameExist = false;	// 按OK按钮时不警告"名字不存在",这样允许新建一个模板
                //tempdlg.ap = Program.MainForm.applicationInfo;
                //tempdlg.ApCfgTitle = "detailform_selecttemplatedlg";
                tempdlg.ShowDialog(this);

                if (tempdlg.DialogResult != DialogResult.OK)
                    return;

                string strNewXml = "";
                nRet = this.readerEditControl1.GetData(
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // 需要消除password/displayName元素内容
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strNewXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载XML到DOM出错: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.DeleteElement(dom.DocumentElement, "password");
                    DomUtil.DeleteElement(dom.DocumentElement, "displayName");
                    DomUtil.DeleteElement(dom.DocumentElement, "refID");

                    strNewXml = dom.OuterXml;
                }
#endif
                nRet = ClearReserveFields(
            ref strNewXml,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 修改配置文件内容
                if (tempdlg.textBox_name.Text != "")
                {
                    // 替换或者追加一个记录
                    nRet = tempdlg.ReplaceRecord(tempdlg.textBox_name.Text,
                        strNewXml,
                        out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                }

                if (tempdlg.Changed == false)	// 没有必要保存回去
                    return;

                string strOutputXml = tempdlg.OutputXml;

                // Debug.Assert(false, "");
                nRet = SaveCfgFile(strReaderDbName + "/cfgs/template",
                    strOutputXml,
                    baTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                Program.MainForm.StatusBarMessage = "修改模板成功。";
                return;
            }
            finally
            {
                this.EnableControls(true);
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 装载读者记录模板
        // return:
        //      -1  error
        //      0   放弃
        //      1   成功装载
        /// <summary>
        /// 装载读者记录模板
        /// </summary>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int LoadReaderTemplateFromServer()
        {
            this.EnableControls(false);

            try
            {

                int nRet = 0;
                string strError = "";

                bool bShift = (Control.ModifierKeys == Keys.Shift);

                if (this.ReaderXmlChanged == true
                    || this.ObjectChanged == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
        "当前有信息被修改后尚未保存。若此时若创建新读者信息，现有未保存信息将丢失。\r\n\r\n确实要创建新读者信息? ",
        "ReaderInfoForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                        return 0;
                }

                this.binaryResControl1.Clear();
                this.ObjectChanged = false; // 2013/10/17

                nRet = this.readerEditControl1.SetData("<root />",
         "",
         null,
         out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.readerEditControl1.Changed = false;

                string strSelectedDbName = Program.MainForm.AppInfo.GetString(
                    "readerinfo_form",
                    "selected_dbname_for_loadtemplate",
                    "");

                SelectedTemplate selected = this.selected_templates.Find(strSelectedDbName);

                GetDbNameDlg dbname_dlg = new GetDbNameDlg();
                MainForm.SetControlFont(dbname_dlg, this.Font, false);
                dbname_dlg.DbType = "reader";
                if (selected != null)
                {
                    dbname_dlg.NotAsk = selected.NotAskDbName;
                    dbname_dlg.AutoClose = (bShift == true ? false : selected.NotAskDbName);
                }

                dbname_dlg.EnableNotAsk = true;
                dbname_dlg.DbName = strSelectedDbName;
                // dbname_dlg.MainForm = Program.MainForm;

                dbname_dlg.Text = "装载读者记录模板 -- 请选择目标读者库名";
                //  dbname_dlg.StartPosition = FormStartPosition.CenterScreen;

                Program.MainForm.AppInfo.LinkFormState(dbname_dlg, "readerinfoformm_load_template_GetBiblioDbNameDlg_state");
                dbname_dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dbname_dlg);

                if (dbname_dlg.DialogResult != DialogResult.OK)
                    return 0;

                string strReaderDbName = dbname_dlg.DbName;
                // 记忆
                Program.MainForm.AppInfo.SetString(
                    "readerinfo_form",
                    "selected_dbname_for_loadtemplate",
                    strReaderDbName);

                selected = this.selected_templates.Find(strReaderDbName);

                this.readerEditControl1.RecPath = dbname_dlg.DbName + "/?";	// 为了追加保存
                this.readerEditControl1.Changed = false;

                // 下载配置文件
                string strContent = "";

                // string strCfgFilePath = respath.Path + "/cfgs/template";
                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetCfgFileContent(strReaderDbName + "/cfgs/template",
                    out strContent,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == 0)
                {
                    MessageBox.Show(this, strError + "\r\n\r\n将改用位于本地的 “选项/读者信息缺省值” 来刷新记录");

                    // 如果template文件不存在，则找本地配置的模板
                    string strNewDefault = Program.MainForm.AppInfo.GetString(
    "readerinfoform_optiondlg",
    "newreader_default",
    "<root />");
                    nRet = this.readerEditControl1.SetData(strNewDefault,
                         "",
                         null,
                         out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);

                    // this.ClearCardPhoto();
                    this.binaryResControl1.Clear();
                    this.ObjectChanged = false; // 2013/10/17

#if NO
                    Global.ClearHtmlPage(this.webBrowser_readerInfo,
                        Program.MainForm.DataDir);
#endif
                    ClearReaderHtmlPage();


                    /*
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strNewDefault);
                     * */
                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "xml",
    strNewDefault);

                    this.m_strSetAction = "new";
                    // this.m_strLoadSource = "local";
                    return -1;
                }
                if (nRet == -1 || nRet == 0)
                {
                    this.readerEditControl1.Timestamp = null;
                    goto ERROR1;
                }

                // MessageBox.Show(this, strContent);

                SelectRecordTemplateDlg select_temp_dlg = new SelectRecordTemplateDlg();
                MainForm.SetControlFont(select_temp_dlg, this.Font, false);

                select_temp_dlg.Text = "请选择新读者记录模板 -- 来自库 '" + strReaderDbName + "'";
                string strSelectedTemplateName = "";
                bool bNotAskTemplateName = false;
                if (selected != null)
                {
                    strSelectedTemplateName = selected.TemplateName;
                    bNotAskTemplateName = selected.NotAskTemplateName;
                }

                select_temp_dlg.SelectedName = strSelectedTemplateName;
                select_temp_dlg.AutoClose = (bShift == true ? false : bNotAskTemplateName);
                select_temp_dlg.NotAsk = bNotAskTemplateName;

                nRet = select_temp_dlg.Initial(
                    false,
                    strContent,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载配置文件 '" + "template" + "' 发生错误: " + strError;
                    goto ERROR1;
                }

                Program.MainForm.AppInfo.LinkFormState(select_temp_dlg, "readerinfoform_load_template_SelectTemplateDlg_state");
                select_temp_dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(select_temp_dlg);

                if (select_temp_dlg.DialogResult != DialogResult.OK)
                    return 0;

                // 记忆本次的选择，下次就不用再进入本对话框了
                this.selected_templates.Set(strReaderDbName,
                    dbname_dlg.NotAsk,
                    select_temp_dlg.SelectedName,
                    select_temp_dlg.NotAsk);

                this.readerEditControl1.Timestamp = null;

                // this.BiblioOriginPath = ""; // 保存从数据库中来的原始path


                nRet = this.readerEditControl1.SetData(
        select_temp_dlg.SelectedRecordXml,
        dbname_dlg.DbName + "/?",
        null,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                /*
                this.SetXmlToWebbrowser(this.webBrowser_xml,
                    select_temp_dlg.SelectedRecordXml);
                 * */
                Global.SetXmlToWebbrowser(this.webBrowser_xml,
Program.MainForm.DataDir,
"xml",
select_temp_dlg.SelectedRecordXml);

                this.m_strSetAction = "new";
                // this.m_strLoadSource = "server";

#if NO
                Global.ClearHtmlPage(this.webBrowser_readerInfo,
                    Program.MainForm.DataDir);
#endif
                ClearReaderHtmlPage();

                this.readerEditControl1.Changed = false;
                return 1;
                ERROR1:
                MessageBox.Show(this, strError);
                return -1;
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        // 装载一条空白记录[从本地]
        // return:
        //      -1  error
        //      0   放弃
        //      1   成功装载
        /// <summary>
        /// 从本地装载一条空白记录
        /// </summary>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int LoadReaderTemplateFromLocal()
        {
            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时若创建新读者信息，现有未保存信息将丢失。\r\n\r\n确实要创建新读者信息? ",
    "ReaderInfoForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return 0;
            }

            this.EnableControls(false);

            try
            {
                string strError = "";

                string strNewDefault = Program.MainForm.AppInfo.GetString(
        "readerinfoform_optiondlg",
        "newreader_default",
        "<root />");
                int nRet = this.readerEditControl1.SetData(strNewDefault,
                     "",
                     null,
                     out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return -1;
                }

                // this.ClearCardPhoto();
                this.binaryResControl1.Clear();

#if NO
                Global.ClearHtmlPage(this.webBrowser_readerInfo,
                    Program.MainForm.DataDir);
#endif
                ClearReaderHtmlPage();

                /*
                this.SetXmlToWebbrowser(this.webBrowser_xml,
                    strNewDefault);
                 * */
                Global.SetXmlToWebbrowser(this.webBrowser_xml,
Program.MainForm.DataDir,
"xml",
strNewDefault);

                this.m_strSetAction = "new";
                // this.m_strLoadSource = "local";

                this.readerEditControl1.Changed = false; // 2013/10/17
                this.ObjectChanged = false; // 2013/10/17
                return 1;
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        void EnableToolStrip(bool bEnable)
        {
            toolStripTextBox_barcode.Enabled = bEnable;
            this.toolStrip1.Enabled = bEnable;
        }

#if REMOVED
        // 保存
        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            if (Control.ModifierKeys == Keys.Control)
                this.commander.AddMessage(WM_SAVE_RECORD_BARCODE);  // 能在读者尚有外借信息的情况下强行修改证条码号
            else
                this.commander.AddMessage(WM_SAVE_RECORD);
        }
#endif

#if NO
        // 形式校验条码号
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        int VerifyBarcode(
            string strBarcode,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在校验条码 ...");
            stop.BeginLoop();

            /*
            this.Update();
            Program.MainForm.Update();
             * */

            try
            {
                long lRet = Channel.VerifyBarcode(
                    stop,
                    strBarcode,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        return -2;
                    goto ERROR1;
                }
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
        ERROR1:
            return -1;
        }
#endif

        // 
        /// <summary>
        /// 是否校验输入的条码号
        /// </summary>
        public bool NeedVerifyBarcode
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "reader_info_form",
                    "verify_barcode",
                    false);
            }
        }

        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 保存记录
        /// </summary>
        /// <param name="strStyle">风格。为 displaysuccess/verifybarcode/changereaderbarcode/changereaderforce 之一或者组合。缺省值为 displaysuccess,verifybarcode</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int SaveRecord(string strStyle = "displaysuccess,verifybarcode")
        {
            string strError = "";
            int nRet = 0;

            bool bControlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            
            if (bControlPressed == false 
                && string.IsNullOrEmpty(this.readerEditControl1.Barcode) == true)
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            // 是否强制修改册条码号
            bool bChangeReaderBarcode = StringUtil.IsInList("changereaderbarcode", strStyle);
            bool bChangeReaderForce = StringUtil.IsInList("changereaderforce", strStyle);
            if (bChangeReaderBarcode && bChangeReaderForce)
            {
                strError = "style 不应同时包含 changereaderbarcode 和 changerecordforce";
                goto ERROR1;
            }

            if (bChangeReaderForce)
            {
                var result = MessageBox.Show(this,
                    "您确实要强制修改当前读者记录？\r\n\r\n警告：当读者有在借记录的情况下，强制修改保存功能，*** 不会自动修改*** 这些在借册记录，会造成借阅信息关联错误。若只是想在修改证条码号以后保存记录，请改用“保存(强制修改证条码号)功能”",
                    "谨慎使用“保存(强制修改)”功能", 
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            bool bVerifyBarcode = StringUtil.IsInList("verifybarcode", strStyle);

            // 校验证条码号
            if ((this.NeedVerifyBarcode == true || bVerifyBarcode)
                && StringUtil.IsIdcardNumber(this.readerEditControl1.Barcode) == false)
            {
                // 形式校验条码号
                // return:
                //      -2  服务器没有配置校验方法，无法校验
                //      -1  error
                //      0   不是合法的条码号
                //      1   是合法的读者证条码号
                //      2   是合法的册条码号
                nRet = VerifyBarcode(
                    Program.MainForm.FocusLibraryCode, // 是否可以根据读者库的馆代码？或者现在已经有了服务器校验功能，这里已经没有必要校验了? // this.Channel.LibraryCodeList,
                    this.readerEditControl1.Barcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    strError = "您输入的证条码 " + this.readerEditControl1.Barcode + " 格式不正确(" + strError + ")。";
                    goto ERROR1;
                }

                // 实际输入的是册条码号
                if (nRet == 2)
                {
                    strError = "您输入的条码号 " + this.readerEditControl1.Barcode + " 是册条码号。请输入读者证条码号。";
                    goto ERROR1;
                }

                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2
                    && (this.NeedVerifyBarcode == true && bVerifyBarcode == false))
                {
                    MessageBox.Show(this, "警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
                }
            }

            // TODO: 保存时候的选项

            // 当 this.readerEditControl1.RecPath 为空的时候，需要出现对话框，让用户可以选择目标库
            string strTargetRecPath = this.readerEditControl1.RecPath;
            if (string.IsNullOrEmpty(this.readerEditControl1.RecPath) == true)
            {
                // 出现对话框，让用户可以选择目标库
                ReaderSaveToDialog saveto_dlg = new ReaderSaveToDialog();
                MainForm.SetControlFont(saveto_dlg, this.Font, false);
                saveto_dlg.MessageText = "请选择记录位置";
                // saveto_dlg.MainForm = Program.MainForm;
                saveto_dlg.RecPath = this.readerEditControl1.RecPath;
                saveto_dlg.RecID = "?";

                Program.MainForm.AppInfo.LinkFormState(saveto_dlg, "readerinfoform_savetodialog_state");
                saveto_dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(saveto_dlg);

                if (saveto_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return 0;

                strTargetRecPath = saveto_dlg.RecPath;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者记录 " + this.readerEditControl1.Barcode + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                nRet = GetReaderXml(
            true,
            false,
            out string strNewXml,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strAction = this.m_strSetAction;

                // 如果特意选定过要保存的位置
                if (string.IsNullOrEmpty(strTargetRecPath) == false
                    && Global.IsAppendRecPath(strTargetRecPath) == false // 2015/11/16 增加的此句，消除 Bug
                    && strAction == "new")
                    strAction = "change";

                if (strAction == "change" && bChangeReaderBarcode)
                {
                    if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.51") < 0)
                    {
                        strError = "需要 dp2library 版本在 2.51 以上才能实现强制修改册条码号的功能。当前 dp2library 版本为 " + Program.MainForm.ServerVersion;
                        goto ERROR1;
                    }
                    strAction = "changereaderbarcode";
                }

                if (strAction == "change" && bChangeReaderForce)
                {
                    strAction = "forcechange";
                }

                // 调试
                // MessageBox.Show(this, "1 this.m_strSetAction='"+this.m_strSetAction+"'");

                long lRet = Channel.SetReaderInfo(
                    stop,
                    strAction,  // this.m_strSetAction,
                    strTargetRecPath,
                    strNewXml,
                    // 2007/11/5 changed
                    this.m_strSetAction != "new" ? this.readerEditControl1.OldRecord : null,
                    this.m_strSetAction != "new" ? this.readerEditControl1.Timestamp : null,
                    out string strExistingXml,
                    out string strSavedXml,
                    out string strSavedPath,
                    out byte[] baNewTimestamp,
                    out ErrorCodeValue kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            //Program.MainForm,
                            this.readerEditControl1.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            this.readerEditControl1.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意重新保存记录");
                            return -1;
                        }
                    }

                    goto ERROR1;
                }

                /*
                this.Timestamp = baNewTimestamp;
                this.OldRecord = strSavedXml;
                this.RecPath = strSavedPath;
                 */

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                    MessageBox.Show(this, strError);

                    if (Channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        // 提醒重新装载?
                        MessageBox.Show(this, "请重新装载记录, 检查哪些字段内容修改被拒绝。");
                    }
                }
                else
                {
                    this.binaryResControl1.BiblioRecPath = strSavedPath;
                    // 提交对象保存请求
                    // return:
                    //		-1	error
                    //		>=0 实际上载的资源对象数
                    nRet = this.binaryResControl1.Save(
                        this.Channel,
                        Program.MainForm.ServerVersion,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                    }
                    if (nRet >= 1)
                    {
                        // 重新获得时间戳
                        lRet = Channel.GetReaderInfo(
                            stop,
                            "@path:" + strSavedPath,
                            "", // "xml,html",
                            out string[] results,
                            out string strOutputPath,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            MessageBox.Show(this, strError);
                        }
                    }

                    // 重新装载记录到编辑器
                    nRet = this.readerEditControl1.SetData(strSavedXml,
                        strSavedPath,
                        baNewTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 刷新XML显示
                    /*
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strSavedXml);
                     * */
                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
Program.MainForm.DataDir,
"xml",
strSavedXml);
                    // 2007/11/12
                    this.m_strSetAction = "change";

                    // 装载记录到HTML
                    {

                        string strBarcode = this.readerEditControl1.Barcode;

                        stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                        int nRedoCount = 0;
                        REDO_LOAD_HTML:
                        string[] results = null;
                        lRet = Channel.GetReaderInfo(
                            stop,
                            strBarcode,
                            "html",
                            out results,
                            out string strOutputRecPath,
                            out byte[] baTimestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            // 以读者身份保存自己的读者记录后，dp2library 为了清除以前缓存的登录信息，强制释放了通道。所以这里需要能重试操作
                            if (Channel.ErrorCode == ErrorCode.ChannelReleased
                                && nRedoCount < 10)
                            {
                                nRedoCount++;
                                goto REDO_LOAD_HTML;
                            }
                            strError = "保存记录已经成功，但在刷新HTML显示的时候发生错误: " + strError;
                            // Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                            this.m_webExternalHost.SetTextString(strError);
                            goto ERROR1;
                        }

                        if (lRet == 0)
                        {
                            strError = "保存记录已经成功，但在刷新HTML显示的时候发生错误: " + strError;
                            // Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                            this.m_webExternalHost.SetTextString(strError);
                            goto ERROR1;
                        }

                        if (lRet > 1)
                        {
                            strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，注意这是一个严重错误，请系统管理员尽快排除。";
                            strError = "保存记录已经成功，但在刷新HTML显示的时候发生错误: " + strError;
                            // Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                            this.m_webExternalHost.SetTextString(strError);
                            goto ERROR1;    // 当出错处理
                        }

                        string strHtml = results[0];

#if NO
                        Global.SetHtmlString(this.webBrowser_readerInfo,
                            strHtml,
        Program.MainForm.DataDir,
        "readerinfoform_reader");
#endif
                        this.SetReaderHtmlString(strHtml);
                    }
                }

                List<string> warnings = new List<string>();

                // 通知人脸中心获取最新变化信息
                // TODO: 对比新旧记录，如果 face 元素变化了，或者册条码号变化了，才请求立即人脸缓存
                if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl) == false
    && string.IsNullOrEmpty(this.readerEditControl1.Barcode) == false)
                {
                    var result = FaceNotifyTask("faceChanged").Result;
                    if (result.Value == -1)
                        warnings.Add(result.ErrorInfo);
                }

                // TODO: 对比新旧记录，如果指纹信息变化了，或者册条码号变化了，才请求立即刷新指纹缓存
                // 更新指纹高速缓存
                if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == false
                && string.IsNullOrEmpty(this.readerEditControl1.Barcode) == false)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      0   成功
                    nRet = UpdateFingerprintCache(
                         this.readerEditControl1.Barcode,
                         this.readerEditControl1.FingerprintFeature,
                         out strError);
                    if (nRet == -1)
                    {
                        // strError = "虽然读者记录已经保存成功，但更新指纹缓存时发生了错误: " + strError;
                        // goto ERROR1;
                        warnings.Add(strError);
                    }
                    // -2 故意不报错。因为用户可能配置了URL，但是当前驱动程序并没有启动
                }

                if (warnings.Count > 0)
                {
                    string warning = $"虽然读者记录已经保存成功，但通知人脸中心和指纹中心刷新时发生了错误: {StringUtil.MakePathList(warnings, "; ")}";
                    this.ShowMessage(warning, "yellow", true);
                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            if (StringUtil.IsInList("displaysuccess", strStyle) == true)
                Program.MainForm.StatusBarMessage = "读者记录保存成功";
            // MessageBox.Show(this, "保存成功");
            return 1;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 另存
        private void toolStripButton_saveTo_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_SAVETO);
        }

        // 获得读者记录的XML格式
        // parameters:
        //      bIncludeFileID  是否要根据当前rescontrol内容合成<dprms:file>元素?
        //      bClearFileID    是否要清除以前的<dprms:file>元素
        int GetReaderXml(
            bool bIncludeFileID,
            bool bClearFileID,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strNewXml = "";
            int nRet = this.readerEditControl1.GetData(
                out strNewXml,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装入DOM时出错: " + ex.Message;
                return -1;
            }

            Debug.Assert(dom != null, "");

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            if (bClearFileID == true
                || (this.binaryResControl1 != null && bIncludeFileID == true)
                )
            {
                // 2011/10/13
                // 清除以前的<dprms:file>元素
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
                foreach (XmlNode node in nodes)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

            // 合成<dprms:file>元素
            if (this.binaryResControl1 != null
                && bIncludeFileID == true)  // 2008/12/3
            {
                // 在 XmlDocument 对象中添加 <file> 元素。新元素加入在根之下
                nRet = this.binaryResControl1.AddFileFragments(ref dom,
            out strError);
                if (nRet == -1)
                    return -1;
            }

            // 如果没有 refID 元素，需要给添加一个
            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID) == true)
                DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());

            strXml = dom.OuterXml;
            return 0;
        }

        // 清除一些保留字段的内容
        static int ClearReserveFields(
            ref string strNewXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML到DOM出错: " + ex.Message;
                return -1;
            }
            DomUtil.DeleteElement(dom.DocumentElement, "refID");
            DomUtil.DeleteElement(dom.DocumentElement, "password");
            DomUtil.DeleteElement(dom.DocumentElement, "displayName");
            // 2014/11/14
            DomUtil.DeleteElement(dom.DocumentElement, "fingerprint");
            DomUtil.DeleteElement(dom.DocumentElement, "hire");
            DomUtil.DeleteElement(dom.DocumentElement, "foregift");
            // DomUtil.DeleteElement(dom.DocumentElement, "personalLibrary");
            DomUtil.DeleteElement(dom.DocumentElement, "friends");

            // 2019/8/1
            DomUtil.DeleteElement(dom.DocumentElement, "face");

#if NO
            // 清除<dprms:file>元素
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }
#endif

            strNewXml = dom.OuterXml;
            return 0;
        }

        void SaveTo()
        {
            string strError = "";
            int nRet = 0;
            bool bReserveFieldsCleared = false;

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            // 校验证条码号
            if (this.NeedVerifyBarcode == true
                && StringUtil.IsIdcardNumber(this.readerEditControl1.Barcode) == false)
            {
                // 形式校验条码号
                // return:
                //      -2  服务器没有配置校验方法，无法校验
                //      -1  error
                //      0   不是合法的条码号
                //      1   是合法的读者证条码号
                //      2   是合法的册条码号
                nRet = VerifyBarcode(
                    Program.MainForm.FocusLibraryCode, // this.Channel.LibraryCodeList,
                    this.readerEditControl1.Barcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    strError = "您输入的证条码号 " + this.readerEditControl1.Barcode + " 格式不正确(" + strError + ")。";
                    goto ERROR1;
                }

                // 实际输入的是册条码号
                if (nRet == 2)
                {
                    strError = "您输入的条码号 " + this.readerEditControl1.Barcode + " 是册条码号。请输入读者证条码号。";
                    goto ERROR1;
                }

                /*
                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2)
                    MessageBox.Show(this, "警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
                 * */
            }

            // 出现对话框，让用户可以选择目标库
            ReaderSaveToDialog saveto_dlg = new ReaderSaveToDialog();
            MainForm.SetControlFont(saveto_dlg, this.Font, false);
            saveto_dlg.Text = "新增一条读者记录";
            saveto_dlg.MessageText = "请选择要保存的目标记录位置\r\n(记录ID为 ? 表示追加保存到数据库末尾)";
            // saveto_dlg.MainForm = Program.MainForm;
            saveto_dlg.RecPath = this.readerEditControl1.RecPath;
            saveto_dlg.RecID = "?";

            Program.MainForm.AppInfo.LinkFormState(saveto_dlg, "readerinfoform_savetodialog_state");
            saveto_dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(saveto_dlg);

            if (saveto_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            bool bIdChanged = false;    // 目标路径是否发生了变化

            if (saveto_dlg.RecID == "?")
                this.m_strSetAction = "new";
            else
            {
                this.m_strSetAction = "change";

                // 检查目标记录路径是否发生了变化
                if (saveto_dlg.RecPath != this.readerEditControl1.RecPath)
                    bIdChanged = true;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者记录 " + this.readerEditControl1.Barcode + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strNewXml = "";

                if (this.m_strSetAction == "new")
                    nRet = GetReaderXml(
                        false,  // 不创建<dprms:file>元素
                        true,   // 清除<dprms:file>元素
                        out strNewXml,
                        out strError);
                else
                    nRet = GetReaderXml(
                        true,  // 创建<dprms:file>元素
                        false,
                        out strNewXml,
                        out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 需要消除password/displayName元素内容
                if (this.m_strSetAction == "new")
                {
                    // 清除一些保留字段的内容
                    nRet = ClearReserveFields(
            ref strNewXml,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    bReserveFieldsCleared = true;
                }

                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                // 调试
                // MessageBox.Show(this, "2 this.m_strSetAction='" + this.m_strSetAction + "'");

                long lRet = Channel.SetReaderInfo(
                    stop,
                    this.m_strSetAction,
                    saveto_dlg.RecPath, // this.readerEditControl1.RecPath,
                    strNewXml,
                    this.m_strSetAction != "new" && bIdChanged == false ? this.readerEditControl1.OldRecord : null,
                    this.m_strSetAction != "new" && bIdChanged == false ? this.readerEditControl1.Timestamp : null,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            //Program.MainForm,
                            this.readerEditControl1.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            this.readerEditControl1.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意重新保存记录");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                /*
                this.Timestamp = baNewTimestamp;
                this.OldRecord = strSavedXml;
                this.RecPath = strSavedPath;
                 */

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                    MessageBox.Show(this, strError);

                    if (Channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        // 提醒重新装载?
                        MessageBox.Show(this, "请重新装载记录, 检查哪些字段内容修改被拒绝。");
                    }
                }
                else
                {
                    this.binaryResControl1.BiblioRecPath = strSavedPath;
                    // 提交对象保存请求
                    // return:
                    //		-1	error
                    //		>=0 实际上载的资源对象数
                    nRet = this.binaryResControl1.Save(
                        this.Channel,
                        Program.MainForm.ServerVersion,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                    }
                    if (nRet >= 1)
                    {
                        // 重新获得时间戳
                        string[] results = null;
                        string strOutputPath = "";
                        lRet = Channel.GetReaderInfo(
                            stop,
                            "@path:" + strSavedPath,
                            "", // "xml,html",
                            out results,
                            out strOutputPath,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            MessageBox.Show(this, strError);
                        }
                    }

                    // 重新装载记录到编辑器
                    nRet = this.readerEditControl1.SetData(strSavedXml,
                        strSavedPath,
                        baNewTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    /*
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strSavedXml);
                     * */
                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
Program.MainForm.DataDir,
"xml",
strSavedXml);
                    // 2007/11/12
                    this.m_strSetAction = "change";

                    // 接着装入对象资源
                    {
                        nRet = this.binaryResControl1.LoadObject(
                            this.Channel,
                            strSavedPath,    // 2008/11/2 changed
                            strSavedXml,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                            // return -1;
                        }
                    }

                    // 2011/11/23
                    // 装载记录到HTML
                    {
                        byte[] baTimestamp = null;
                        string strOutputRecPath = "";

                        string strBarcode = this.readerEditControl1.Barcode;

                        stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                        string[] results = null;
                        lRet = Channel.GetReaderInfo(
                            stop,
                            strBarcode,
                            "html",
                            out results,
                            out strOutputRecPath,
                            out baTimestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "保存记录已经成功，但在刷新HTML显示的时候发生错误: " + strError;
                            // Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                            this.m_webExternalHost.SetTextString(strError);
                            goto ERROR1;
                        }

                        if (lRet == 0)
                        {
                            strError = "保存记录已经成功，但在刷新HTML显示的时候发生错误: " + strError;
                            Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                            goto ERROR1;
                        }

                        if (lRet > 1)
                        {
                            strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，注意这是一个严重错误，请系统管理员尽快排除。";
                            strError = "保存记录已经成功，但在刷新HTML显示的时候发生错误: " + strError;
                            // Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                            this.m_webExternalHost.SetTextString(strError);
                            goto ERROR1;    // 当出错处理
                        }

                        string strHtml = results[0];

#if NO
                        Global.SetHtmlString(this.webBrowser_readerInfo,
                            strHtml,
        Program.MainForm.DataDir,
        "readerinfoform_reader");
#endif
                        this.SetReaderHtmlString(strHtml);
                    }

                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            if (bReserveFieldsCleared == true)
                MessageBox.Show(this, "另存成功。新记录的密码为初始状态，显示名尚未设置。");
            else
                MessageBox.Show(this, "另存成功。");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 删除记录
        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_DELETE_RECORD);

        }

        // 删除记录
        void DeleteRecord()
        {
            string strError = "";

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号，无法删除";
                goto ERROR1;
            }

            // bool bForceDelete = false;
            string strRecPath = null;
            string strText = "确实要删除证条码号为 '" + this.readerEditControl1.Barcode + "' 的读者记录 ? ";

            // 如果同时按下control键，表示强制按照记录路径删除
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // bForceDelete = true;
                strRecPath = this.readerEditControl1.RecPath;
                strText = "确实要删除证条码号为 '" + this.readerEditControl1.Barcode + "' 并且记录路径为 '" + strRecPath + "' 的读者记录 ? ";
            }

            DialogResult result = MessageBox.Show(this,
                strText,
                "ReaderInfoForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除读者记录 " + this.readerEditControl1.Barcode + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {

                string strNewXml = "";
                int nRet = this.readerEditControl1.GetData(
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strOldBarcode = this.readerEditControl1.Barcode;

                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                long lRet = Channel.SetReaderInfo(
                    stop,
                    "delete",
                    strRecPath,   // this.readerEditControl1.RecPath,
                    "", // strNewXml,
                    this.readerEditControl1.OldRecord,
                    this.readerEditControl1.Timestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            //Program.MainForm,
                            this.readerEditControl1.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            this.readerEditControl1.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，若还想继续删除，按‘确定’按钮后可重试删除。如果不想删除了，请按‘取消’按钮");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意读者记录此时***并未***删除。\r\n\r\n如要删除记录，请按‘删除’按钮重新提交删除请求。");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                // 保留删除过的窗口，一旦需要，还可以重新保存回去
                this.m_strSetAction = "new";

                nRet = this.readerEditControl1.SetData(strExistingXml,
                    null,
                    null,
                    out strError);
                if (nRet == -1)
                {
                    strError = "删除操作后的SetData()操作失败: " + strError;
                    MessageBox.Show(this, strError);
                }

                this.readerEditControl1.Changed = false;

                // 更新指纹高速缓存
                if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == false
                    && string.IsNullOrEmpty(this.readerEditControl1.Barcode) == false)
                {
                    // return:
                    //      -2  remoting服务器连接失败。驱动程序尚未启动
                    //      -1  出错
                    //      0   成功
                    nRet = UpdateFingerprintCache(
                         strOldBarcode,
                         "",
                         out strError);
                    if (nRet == -1)
                    {
                        strError = "虽然读者记录已经删除成功，但更新指纹缓存时发生了错误: " + strError;
                        goto ERROR1;
                    }
                    // -2 故意不报错。因为用户可能配置了URL，但是当前接口程序(zkfingerprint.exe)并没有启动
                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            MessageBox.Show(this, "删除成功。\r\n\r\n您会发现编辑窗口中还留着读者记录内容，但请不要担心，数据库里的读者记录已经被删除了。\r\n\r\n如果您这时后悔了，还可以按“保存按钮”把读者记录原样保存回去。");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;

        }

#if NO
        #region delete

        // 删除
        private void button_delete_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号，无法删除";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除证条码号为 '" + this.readerEditControl1.Barcode + "' 的读者记录 ? ",
"ReaderInfoForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在删除读者记录 " + this.readerEditControl1.Barcode + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {

                string strNewXml = "";
                int nRet = this.readerEditControl1.GetData(
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;


                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                long lRet = Channel.SetReaderInfo(
                    stop,
                    "delete",
                    null,   // this.readerEditControl1.RecPath,
                    "", // strNewXml,
                    this.readerEditControl1.OldRecord,
                    this.readerEditControl1.Timestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            Program.MainForm,
                            this.readerEditControl1.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            this.readerEditControl1.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，若还想继续删除，按‘确定’按钮后可重试删除。如果不想删除了，请按‘取消’按钮");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意读者记录此时***并未***删除。\r\n\r\n如要删除记录，请按‘删除’按钮重新提交删除请求。");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                // 保留删除过的窗口，一旦需要，还可以重新保存回去
                this.m_strSetAction = "new";

                nRet = this.readerEditControl1.SetData(strExistingXml,
                    null,
                    null,
                    out strError);
                if (nRet == -1)
                {
                    strError = "删除操作后的SetDate()操作失败: " + strError;
                    MessageBox.Show(this, strError);
                }

                this.readerEditControl1.Changed = false;

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            MessageBox.Show(this, "删除成功。\r\n\r\n您会发现编辑窗口中还留着读者记录内容，但请不要担心，数据库里的读者记录已经被删除了。\r\n\r\n如果您这时后悔了，还可以按“保存按钮”把读者记录原样保存回去。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }


        // 另存
        private void button_saveTo_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者记录 " + this.readerEditControl1.Barcode + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strNewXml = "";
                int nRet = this.readerEditControl1.GetData(
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 需要消除password元素内容。
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strNewXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "装载XML到DOM出错: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.SetElementText(dom.DocumentElement,
                        "password", "");
                    strNewXml = dom.OuterXml;
                }

                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                long lRet = Channel.SetReaderInfo(
                    stop,
                    "new",  // this.m_strSetAction,
                    "", // this.readerEditControl1.RecPath,
                    strNewXml,
                    "", // this.readerEditControl1.OldRecord,
                    null,   // this.readerEditControl1.Timestamp,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            Program.MainForm,
                            this.readerEditControl1.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            this.readerEditControl1.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意重新保存记录");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                /*
                this.Timestamp = baNewTimestamp;
                this.OldRecord = strSavedXml;
                this.RecPath = strSavedPath;
                 */

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                    MessageBox.Show(this, strError);

                    if (Channel.ErrorCode == ErrorCode.ChangePartDenied)
                    {
                        // 提醒重新装载?
                        MessageBox.Show(this, "请重新装载记录, 检查哪些字段内容修改被拒绝。");
                    }
                }
                else
                {
                    // 重新装载记录到编辑器
                    nRet = this.readerEditControl1.SetData(strSavedXml,
                        strSavedPath,
                        baNewTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // 
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strSavedXml);

                    // 2007/11/12
                    this.m_strSetAction = "change";
                }

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            MessageBox.Show(this, "另存成功。新记录的密码尚未设置。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 保存
        private void button_save_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存读者记录 " + this.readerEditControl1.Barcode + " ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strNewXml = "";
                int nRet = this.readerEditControl1.GetData(
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                long lRet = Channel.SetReaderInfo(
                    stop,
                    this.m_strSetAction,
                    this.readerEditControl1.RecPath,
                    strNewXml,
                    this.m_strSetAction != "new" ? this.readerEditControl1.OldRecord : null,
                    this.m_strSetAction != "new" ? this.readerEditControl1.Timestamp : null,
                    out strExistingXml,
                    out strSavedXml,
                    out strSavedPath,
                    out baNewTimestamp,
                    out kernel_errorcode,
                    out strError);
                if (lRet == -1)
                {
                    // Debug.Assert(false, "");

                    if (kernel_errorcode == ErrorCodeValue.TimestampMismatch)
                    {
                        CompareReaderForm dlg = new CompareReaderForm();
                        dlg.Initial(
                            Program.MainForm,
                            this.readerEditControl1.RecPath,
                            strExistingXml,
                            baNewTimestamp,
                            strNewXml,
                            this.readerEditControl1.Timestamp,
                            "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                        dlg.StartPosition = FormStartPosition.CenterScreen;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                MessageBox.Show(this, strError);
                            }
                            MessageBox.Show(this, "请注意重新保存记录");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                /*
                this.Timestamp = baNewTimestamp;
                this.OldRecord = strSavedXml;
                this.RecPath = strSavedPath;
                 */

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                    MessageBox.Show(this, strError);

                    if (Channel.ErrorCode == ErrorCode.ChangePartDenied)
                    {
                        // 提醒重新装载?
                        MessageBox.Show(this, "请重新装载记录, 检查哪些字段内容修改被拒绝。");
                    }
                }
                else
                {
                    // 重新装载记录到编辑器
                    nRet = this.readerEditControl1.SetData(strSavedXml,
                        strSavedPath,
                        baNewTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // 
                    this.SetXmlToWebbrowser(this.webBrowser_xml,
                        strSavedXml);

                    // 2007/11/12
                    this.m_strSetAction = "change";

                }

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            MessageBox.Show(this, "保存成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 装载一条空白记录
        private void button_loadBlank_Click(object sender, EventArgs e)
        {
            if (this.readerEditControl1.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时若创建新读者信息，现有未保存信息将丢失。\r\n\r\n确实要创建新读者信息? ",
    "ReaderInfoForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
            }

            this.EnableControls(false);

            try
            {
                string strError = "";

                string strNewDefault = Program.MainForm.AppInfo.GetString(
        "readerinfoform_optiondlg",
        "newreader_default",
        "<root />");
                int nRet = this.readerEditControl1.SetData(strNewDefault,
                     "",
                     null,
                     out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                Global.ClearHtmlPage(this.webBrowser_readerInfo,
                    Program.MainForm.DataDir);
                // 
                this.SetXmlToWebbrowser(this.webBrowser_xml,
                    strNewDefault);


                this.m_strSetAction = "new";
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        // 选项
        private void button_option_Click(object sender, EventArgs e)
        {
            ReaderInfoFormOptionDlg dlg = new ReaderInfoFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.MainForm = Program.MainForm;
            dlg.ShowDialog(this);
        }

        #endregion

#endif

        // 选项
        private void toolStripButton_option_Click(object sender, EventArgs e)
        {
            ReaderInfoFormOptionDlg dlg = new ReaderInfoFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            // dlg.MainForm = Program.MainForm;
            dlg.ShowDialog(this);
        }


        void Hire()
        {
            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                MessageBox.Show(this, "当前有信息被修改后尚未保存。必须先保存后，才能进行创建租金的操作。");
                return;
            }

            string strError = "";
            int nRet = 0;

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            // 校验证条码号
            if (this.NeedVerifyBarcode == true
                && StringUtil.IsIdcardNumber(this.readerEditControl1.Barcode) == false)
            {
                // 形式校验条码号
                // return:
                //      -2  服务器没有配置校验方法，无法校验
                //      -1  error
                //      0   不是合法的条码号
                //      1   是合法的读者证条码号
                //      2   是合法的册条码号
                nRet = VerifyBarcode(
                    Program.MainForm.FocusLibraryCode, // this.Channel.LibraryCodeList,
                    this.readerEditControl1.Barcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    strError = "您输入的证条码号 " + this.readerEditControl1.Barcode + " 格式不正确(" + strError + ")。";
                    goto ERROR1;
                }

                // 实际输入的是册条码号
                if (nRet == 2)
                {
                    strError = "您输入的条码号 " + this.readerEditControl1.Barcode + " 是册条码号。请输入读者证条码号。";
                    goto ERROR1;
                }

                /*
                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2)
                    MessageBox.Show(this, "警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
                 * */
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建读者记录 " + this.readerEditControl1.Barcode + " 的 租金交费请求 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strReaderBarcode = this.readerEditControl1.Barcode;
                string strAction = "hire";

                string strOutputrReaderXml = "";
                string strOutputID = "";

                long lRet = Channel.Hire(
                    stop,
                    strAction,
                    strReaderBarcode,
                    out strOutputrReaderXml,
                    out strOutputID,
                    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }


            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            // 重新装载窗口内容
            LoadRecord(this.readerEditControl1.Barcode,
                false);

            MessageBox.Show(this, "创建租金交费请求 成功");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;

        }

        // 前一条读者记录
        private void toolStripButton_prev_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_PREV_RECORD);
        }

        // 后一条读者记录
        private void toolStripButton_next_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_NEXT_RECORD);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SET_FOCUS:
                    this.toolStripTextBox_barcode.Focus();
                    return;
                case WM_LOAD_RECORD:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.LoadRecord(
                                this.toolStripTextBox_barcode.Text,
                                // this.textBox_readerBarcode.Text,
                                false);
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_DELETE_RECORD:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.DeleteRecord();
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_NEXT_RECORD:
                    EnableToolStrip(false);
                    try
                    {
                        /*
                        Debug.Assert(this.m_webExternalHost.IsInLoop == false, "启动前发现上一次循环尚未停止");

                        if (this.m_webExternalHost.ChannelInUse == true)
                        {
                            // 缓兵之计
                            this.m_webExternalHost.Stop();
                            // Thread.Sleep(100);
                            this.commander.AddMessage(WM_NEXT_RECORD);
                            return;
                        }


                        Debug.Assert(this.m_webExternalHost.ChannelInUse == false, "启动前发现通道还未释放");
                         * */

                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            LoadRecordByRecPath(this.readerEditControl1.RecPath, "next");
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;

                case WM_PREV_RECORD:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            LoadRecordByRecPath(this.readerEditControl1.RecPath, "prev");
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_HIRE:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.Hire();
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_FOREGIFT:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.Foregift("foregift");
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_RETURN_FOREGIFT:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.Foregift("return");
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_SAVETO:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.SaveTo();
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_SAVE_RECORD:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.SaveRecord("displaysuccess");  // ,verifybarcode
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_SAVE_RECORD_BARCODE:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.SaveRecord("displaysuccess,changereaderbarcode");  // verifybarcode,
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_SAVE_RECORD_FORCE:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            this.SaveRecord("displaysuccess,changereaderforce");
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void toolStripButton_stopSummaryLoop_Click(object sender, EventArgs e)
        {
            // this.m_webExternalHost.IsInLoop = false;
            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

        }


        // parameters:
        //      strAction   为foregift和return之一
        void Foregift(string strAction)
        {
            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                MessageBox.Show(this, "当前有信息被修改后尚未保存。必须先保存后，才能进行创建押金的操作。");
                return;
            }

            string strError = "";
            int nRet = 0;


            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            // 校验证条码号
            if (this.NeedVerifyBarcode == true
                && StringUtil.IsIdcardNumber(this.readerEditControl1.Barcode) == false)
            {
                // 形式校验条码号
                // return:
                //      -2  服务器没有配置校验方法，无法校验
                //      -1  error
                //      0   不是合法的条码号
                //      1   是合法的读者证条码号
                //      2   是合法的册条码号
                nRet = VerifyBarcode(
                    Program.MainForm.FocusLibraryCode, // this.Channel.LibraryCodeList,
                    this.readerEditControl1.Barcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    strError = "您输入的证条码号 " + this.readerEditControl1.Barcode + " 格式不正确(" + strError + ")。";
                    goto ERROR1;
                }

                // 实际输入的是册条码号
                if (nRet == 2)
                {
                    strError = "您输入的条码号 " + this.readerEditControl1.Barcode + " 是册条码号。请输入读者证条码号。";
                    goto ERROR1;
                }

                /*
                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2)
                    MessageBox.Show(this, "警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
                 * */
            }

            string strActionName = "押金交费";

            if (strAction == "return")
                strActionName = "押金退费";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建读者记录 " + this.readerEditControl1.Barcode + " 的" + strActionName + "记录 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strReaderBarcode = this.readerEditControl1.Barcode;

                string strOutputrReaderXml = "";
                string strOutputID = "";

                Debug.Assert(strAction == "foregift" || strAction == "return", "");

                long lRet = Channel.Foregift(
                    stop,
                    strAction,
                    strReaderBarcode,
                    out strOutputrReaderXml,
                    out strOutputID,
                    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            // 重新装载窗口内容
            LoadRecord(this.readerEditControl1.Barcode,
                false);

            MessageBox.Show(this, "创建" + strActionName + "记录成功");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;

        }

        // 创建租金交费请求
        private void ToolStripMenuItem_hire_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_HIRE);
        }

        /*
        // old
        private void toolStripButton_hire_Click(object sender, EventArgs e)
        {
        }*/

        // 创建押金交费请求
        private void ToolStripMenuItem_foregift_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_FOREGIFT);
        }

        /*
        // old
        private void toolStripButton_foregift_Click(object sender, EventArgs e)
        {
        }*/

        // 创建押金退费请求
        private void ToolStripMenuItem_returnForegift_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_RETURN_FOREGIFT);
        }

        private void ReaderInfoForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void ReaderInfoForm_DragDrop(object sender, DragEventArgs e)
        {
            string strError = "";

            string strWhole = (String)e.Data.GetData("Text");

            string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1)
            {
                strError = "连一行也不存在";
                goto ERROR1;
            }

            if (lines.Length > 1)
            {
                strError = "读者窗只允许拖入一个记录";
                goto ERROR1;
            }

            string strFirstLine = lines[0].Trim();

            // 取得recpath
            string strRecPath = "";
            int nRet = strFirstLine.IndexOf("\t");
            if (nRet == -1)
                strRecPath = strFirstLine;
            else
                strRecPath = strFirstLine.Substring(0, nRet).Trim();

            // 判断它是不是读者记录路径
            string strDbName = Global.GetDbName(strRecPath);

            if (Program.MainForm.IsReaderDbName(strDbName) == true)
            {
                this.LoadRecordByRecPath(strRecPath,
                    "");
            }
            else
            {
                strError = "记录路径 '" + strRecPath + "' 中的数据库名不是读者库名...";
                goto ERROR1;
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_clearOutofReservationCount_Click(object sender, EventArgs e)
        {
            bool bRet = this.readerEditControl1.ClearOutofReservationCount();
            if (bRet == true)
            {
                MessageBox.Show(this, "当前记录的 预约到书未取次数 已经被清除为0。注意保存当前记录。");
            }
        }

        private void toolStripButton_saveTemplate_Click(object sender, EventArgs e)
        {
            SaveReaderToTemplate();
        }

        // *** 此函数已经废止
        // (从剪贴板)粘贴证件照(1)
        private void toolStripButton_pasteCardPhoto_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 从剪贴板中取得图像对象
            List<Image> images = ImageUtil.GetImagesFromClipboard(out strError);
            if (images == null)
            {
                strError = "。无法创建证件照片";
                goto ERROR1;
            }
            Image image = images[0];
#if NO
            Image image = null;
            IDataObject obj1 = Clipboard.GetDataObject();
            if (obj1.GetDataPresent(typeof(Bitmap)))
            {
                image = (Image)obj1.GetData(typeof(Bitmap));
            }
            else if (obj1.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])obj1.GetData(DataFormats.FileDrop);

                try
                {
                    image = Image.FromFile(files[0]);
                }
                catch (OutOfMemoryException)
                {
                    strError = "当前 Windows 剪贴板中的第一个文件不是图像文件。无法创建证件照片";
                    goto ERROR1;
                }
            }
            else
            {
                strError = "当前 Windows 剪贴板中没有图形对象。无法创建证件照片";
                goto ERROR1;
            }
#endif

            string strShrinkComment = "";
            using (image)
            {
                // 自动缩小图像
                nRet = SetCardPhoto(image,
                    "cardphoto",
                out strShrinkComment,
                out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 切换到对象属性页，以便操作者能看到刚刚创建的对象行
            this.tabControl_readerInfo.SelectedTab = this.tabPage_objects;

            MessageBox.Show(this, "证件照片已经成功创建。\r\n"
                + strShrinkComment
                + "\r\n\r\n(但因当前读者记录还未保存，图像数据尚未提交到服务器)\r\n\r\n注意稍后保存当前读者记录。");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_webCamera_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strShrinkComment = "";

#if NO
            UtilityForm form = Program.MainForm.EnsureUtilityForm();
            form.Activate();
            form.ActivateWebCameraPage();
#endif
            Program.MainForm.DisableCamera();
            try
            {
                // 注： new CameraPhotoDialog() 可能会抛出异常
                using (CameraPhotoDialog dlg = new CameraPhotoDialog())
                {
                    // MainForm.SetControlFont(dlg, this.Font, false);
                    dlg.Font = this.Font;

                    dlg.CurrentCamera = Program.MainForm.AppInfo.GetString(
                        "readerinfoform",
                        "current_camera",
                        "");

                    Program.MainForm.AppInfo.LinkFormState(dlg, "CameraPhotoDialog_state");
                    dlg.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(dlg);

                    Program.MainForm.AppInfo.SetString(
                        "readerinfoform",
                        "current_camera",
                        dlg.CurrentCamera);

                    if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                        return;

                    int nRet = 0;

                    Image image = dlg.Image;

                    using (image)
                    {
                        // 自动缩小图像
                        nRet = SetCardPhoto(image,
                    "cardphoto",
                        out strShrinkComment,
                        out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }
            }
            finally
            {
                Application.DoEvents();

                Program.MainForm.EnableCamera();
            }

            // 切换到对象属性页，以便操作者能看到刚刚创建的对象行
            this.tabControl_readerInfo.SelectedTab = this.tabPage_objects;  // 会导致输入焦点变化，读者窗停止捕捉摄像

            MessageBox.Show(this, "证件照片已经成功创建。\r\n"
                + strShrinkComment
                + "\r\n\r\n(但因当前读者记录还未保存，图像数据尚未提交到服务器)\r\n\r\n注意稍后保存当前读者记录。");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 装载一条空白记录 从本地
        private void toolStripMenuItem_loadBlankFromLocal_Click(object sender, EventArgs e)
        {
            LoadReaderTemplateFromLocal();
            this.toolStripButton_loadBlank.Image = this.toolStripMenuItem_loadBlankFromLocal.Image;
            this.toolStripButton_loadBlank.Text = this.toolStripMenuItem_loadBlankFromLocal.Text;
        }

        // 装载一条空白记录 从服务器
        private void ToolStripMenuItem_loadBlankFromServer_Click(object sender, EventArgs e)
        {
            LoadReaderTemplateFromServer();
            this.toolStripButton_loadBlank.Image = this.ToolStripMenuItem_loadBlankFromServer.Image;
            this.toolStripButton_loadBlank.Text = this.ToolStripMenuItem_loadBlankFromServer.Text;
        }

        // 会变化的命令
        private void toolStripButton_loadBlank_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_loadBlank.Text == this.toolStripMenuItem_loadBlankFromLocal.Text)
            {
                LoadReaderTemplateFromLocal();
            }
            else
            {
                LoadReaderTemplateFromServer();
            }
        }


        IpcClientChannel m_idcardChannel = new IpcClientChannel();
        IIdcard m_idcardObj = null;

        int StartIdcardChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            //Register the channel with ChannelServices.
            if (this.m_idcardChannel == null)
                this.m_idcardChannel = new IpcClientChannel();

            ChannelServices.RegisterChannel(this.m_idcardChannel, false);

            try
            {
                m_idcardObj = (IIdcard)Activator.GetObject(typeof(IIdcard),
                    strUrl);
                if (m_idcardObj == null)
                {
                    strError = "无法连接到服务器 " + strUrl;
                    return -1;
                }
            }
            finally
            {
            }

            return 0;
        }

        void EndIdcardChannel()
        {
            if (this.m_idcardChannel != null)
            {
                ChannelServices.UnregisterChannel(this.m_idcardChannel);
                this.m_idcardChannel = null;
            }
        }

        // parameters:
        //      strSelection    身份证字段选择列表。缺省值为 "name,gender,nation,dateOfBirth,address,idcardnumber,agency,validaterange,photo"
        //      bSetCreateDate  是否设置 发证日期 字段内容
        static int BuildReaderXml(string strIdcardXml,
            string strSelection,
            bool bSetReaderBarcode,
            bool bSetCreateDate,
            ref string strReaderXml,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strReaderXml) == true)
                strReaderXml = "<root />";

            XmlDocument domSource = new XmlDocument();
            try
            {
                domSource.LoadXml(strIdcardXml);
            }
            catch (Exception ex)
            {
                strError = "身份证信息 XML 装入 DOM 失败: " + ex.Message;
                return -1;
            }

            XmlDocument domTarget = new XmlDocument();
            try
            {
                domTarget.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "原有读者XML装入DOM失败: " + ex.Message;
                return -1;
            }

            // 身份证号
            if (StringUtil.IsInList("idcardnumber", strSelection) == true)
            {
                string strID = DomUtil.GetElementText(domSource.DocumentElement,
                    "id");

                if (bSetReaderBarcode == true)
                {
                    // 读者证号
                    DomUtil.SetElementText(domTarget.DocumentElement,
                        "barcode", strID);
                }

                DomUtil.SetElementText(domTarget.DocumentElement,
                    "idCardNumber", strID);
            }

            // 姓名
            if (StringUtil.IsInList("name", strSelection) == true)
            {
                string strName = DomUtil.GetElementText(domSource.DocumentElement,
        "name");
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "name", strName);
            }

            // 性别
            if (StringUtil.IsInList("gender", strSelection) == true)
            {
                string strGender = DomUtil.GetElementText(domSource.DocumentElement,
        "gender");
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "gender", strGender);
            }

            // 民族
            if (StringUtil.IsInList("nation", strSelection) == true)
            {
                string strNation = DomUtil.GetElementText(domSource.DocumentElement,
    "nation");
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "nation", strNation);
            }

            // 出生日期
            if (StringUtil.IsInList("dateOfBirth", strSelection) == true)
            {
                string strDateOfBirth = DomUtil.GetElementText(domSource.DocumentElement,
    "dateOfBirth");
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "dateOfBirth", strDateOfBirth);
            }

            // 家庭地址
            if (StringUtil.IsInList("address", strSelection) == true)
            {
                string strAddress = DomUtil.GetElementText(domSource.DocumentElement,
    "address");
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "address", strAddress);
            }

            // 发证日期
            string strCreateDate = DomUtil.GetElementText(domSource.DocumentElement,
"createDate");

            // 失效日期
            string strExpireDate = DomUtil.GetElementText(domSource.DocumentElement,
"expireDate");
            if (StringUtil.IsInList("validaterange", strSelection) == true)
            {
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "expireDate", strExpireDate);
            }

            // 发证机关
            string strAgency = DomUtil.GetElementText(domSource.DocumentElement,
"agency");
            string strComment = "";

            if (StringUtil.IsInList("agency", strSelection) == true)
            {
                strComment += "本记录根据身份证信息创建。身份证签发机关: " + strAgency + "; ";
            }
            if (StringUtil.IsInList("validaterange", strSelection) == true)
            {
                strComment += "有效期限: " + DateTimeUtil.LocalDate(strCreateDate)
                    + " - "
                    + DateTimeUtil.LocalDate(strExpireDate);
            }

            if (string.IsNullOrEmpty(strComment) == false)
            {
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "comment", strComment);
            }

            // 读者记录的创建日期算作今天
            if (bSetCreateDate == true)
            {
                DomUtil.SetElementText(domTarget.DocumentElement,
                    "createDate", DateTimeUtil.Rfc1123DateTimeStringEx(DateTime.Now));
            }

            // TODO: 是否警告已经失效的身份证件?

            strReaderXml = domTarget.DocumentElement.OuterXml;
            return 0;
        }

        // 
        /// <summary>
        /// 标记删除当前记录的证件照片对象
        /// </summary>
        public void ClearCardPhoto()
        {
            List<ListViewItem> items = this.binaryResControl1.FindItemByUsage("cardphoto");

            this.binaryResControl1.MaskDelete(items);
        }

        /// <summary>
        /// 当前窗口中是否已经有了用途为 "cardphoto" 的对象资源
        /// </summary>
        public bool HasCardPhoto
        {
            get
            {
                List<ListViewItem> items = this.binaryResControl1.FindItemByUsage("cardphoto");
                if (items.Count > 0)
                {
                    // 观察是否有至少一个尺寸为 0 以外的行
                    foreach (ListViewItem item in items)
                    {
                        string strSizeString = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_SIZE);
                        if (string.IsNullOrEmpty(strSizeString) == false)
                        {
                            long v = 0;
                            if (long.TryParse(strSizeString, out v) == false)
                                continue;
                            if (v > 0)
                                return true;
                        }
                    }
                    return false;
                }
                return false;
            }
        }

        // 
        /// <summary>
        /// 设置当前记录的证件照片对象
        /// </summary>
        /// <param name="image">腿片对象</param>
        /// <param name="object_type">对象的类型，cardphoto/face 之一</param>
        /// <param name="strShrinkComment">返回缩放注释</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int SetCardPhoto(Image image,
            string object_type,
            out string strShrinkComment,
            out string strError)
        {
            strError = "";
            strShrinkComment = "";
            int nRet = 0;

            ImageFormat format = ImageFormat.Jpeg;

            // 自动缩小图像
            string strMaxWidth = Program.MainForm.AppInfo.GetString(
    "readerinfoform_optiondlg",
    $"{object_type}_maxwidth",
    object_type == "cardphoto" ? "120" : "640");
            int nMaxWidth = -1;
            Int32.TryParse(strMaxWidth,
                out nMaxWidth);
            if (nMaxWidth != -1)
            {
                int nOldWidth = image.Width;
                // 缩小图像
                // parameters:
                //		nNewWidth0	宽度(0表示不变化)
                //		nNewHeight0	高度
                //      bRatio  是否保持纵横比例
                // return:
                //      -1  出错
                //      0   没有必要缩放(objBitmap未处理)
                //      1   已经缩放
                nRet = DigitalPlatform.Drawing.GraphicsUtil.ShrinkPic(ref image,
                    nMaxWidth,
                    0,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nOldWidth != image.Width)
                {
                    strShrinkComment = "图像宽度被从 " + nOldWidth.ToString() + " 像素缩小到 " + image.Width.ToString() + " 像素";
                }
            }

            string strTempFilePath = FileUtil.NewTempFileName(Program.MainForm.DataDir,
                "~temp_make_cardphoto_",
                ".png");

            image.Save(strTempFilePath,
                format);
            image.Dispose();
            image = null;

            List<ListViewItem> items = this.binaryResControl1.FindItemByUsage(object_type); // "cardphoto"
            if (items.Count == 0)
            {
                ListViewItem item = null;
                nRet = this.binaryResControl1.AppendNewItem(
    strTempFilePath,
    object_type,   // "cardphoto",
    "",
    out item,
    out strError);
            }
            else
            {
                nRet = this.binaryResControl1.ChangeObjectFile(items[0],
     strTempFilePath,
     object_type,  // "cardphoto",
             out strError);
            }
            if (nRet == -1)
                goto ERROR1;

            return 0;
            ERROR1:
            return -1;
        }

        /// <summary>
        /// 是否显示“是否用身份证号当作证条码号”按钮
        /// </summary>
        public string DisplaySetReaderBarcodeDialogButton
        {
            get
            {
                return Program.MainForm.AppInfo.GetString(
    "reader_info_form",
    "display_setreaderbarcode_dialog_button",
    "no");
            }
            set
            {
                Program.MainForm.AppInfo.SetString(
    "reader_info_form",
    "display_setreaderbarcode_dialog_button",
    value);
            }
        }

        /// <summary>
        /// 是否显示“是否用身份证号当作证条码号”对话框
        /// </summary>
        public bool DisplaySetReaderBarcodeDialog
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
    "reader_info_form",
    "display_setreaderbarcode_dialog",
    true);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
    "reader_info_form",
    "display_setreaderbarcode_dialog",
    value);
            }
        }

        // 
        /// <summary>
        /// 身份证字段选择列表
        /// </summary>
        public string IdcardFieldSelection
        {
            get
            {
                return Program.MainForm.AppInfo.GetString(
    "readerinfoform_optiondlg",
    "idcardfield_filter_list",
    "name,gender,nation,dateOfBirth,address,idcardnumber,agency,validaterange,photo");

            }
        }

        // 
        /// <summary>
        /// 当出现读卡对话框时是否自动重试
        /// </summary>
        public bool AutoRetryReaderCard
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                "reader_info_form",
                "autoretry_readcarddialog",
                true);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                "reader_info_form",
                "autoretry_readcarddialog",
                value);
            }
        }

        // 在读者窗范围内自动关闭 身份证读卡器 键盘仿真(&S)
        /// <summary>
        /// 是否在读者窗范围内自动关闭 身份证读卡器 键盘仿真
        /// </summary>
        public bool DisableIdcardReaderSendkey
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
    "reader_info_form",
    "disable_idcardreader_sendkey",
    true);
            }
        }

        // string m_strLoadSource = "";   // 从什么渠道装载的空白记录信息? local server idcard

        string m_strIdcardXml = "";
        byte[] m_baPhoto = null;

        // parameters:
        //      bClear  操作前是否清除编辑器原有的全部内容
        // return:
        //      -1  出错
        //      0   放弃装载
        //      1   成功
        int LoadFromIdcard(bool bClear,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(Program.MainForm.IdcardReaderUrl) == true)
            {
                strError = "尚未配置 身份证读卡器URL 系统参数，无法读取身份证卡";
                return -1;
            }

            if (string.IsNullOrEmpty(this.IdcardFieldSelection) == true)
            {
                MessageBox.Show(this, "提示：您配置的身份证字段选用参数中不包括任何字段，所以导入操作没有实际意义。(您可在读者窗的“选项”对话框中修改身份证字段选用参数)");
            }

            this.EnableControls(false);
            bool bOldSendKeyEnabled = true;
            Image image = null;
            try
            {
                int nRet = StartIdcardChannel(
                    Program.MainForm.IdcardReaderUrl,
                    out strError);
                if (nRet == -1)
                    return -1;

                try
                {
                    try
                    {
                        bOldSendKeyEnabled = m_idcardObj.SendKeyEnabled;
                        m_idcardObj.SendKeyEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        strError = "针对 " + Program.MainForm.IdcardReaderUrl + " 操作失败: " + ex.Message;
                        return -1;
                    }

                    // 警告尚未保存
                    // 在禁止驻留程序 SendKey 以后才出现对话框较好
                    if (this.ReaderXmlChanged == true
    || this.ObjectChanged == true)
                    {
                        DialogResult result = MessageBox.Show(this,
            "当前有信息被修改后尚未保存。若此时若创建新读者信息，现有未保存信息将丢失。\r\n\r\n确实要创建新读者信息? ",
            "ReaderInfoForm",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                            return 0;
                    }

                    m_strIdcardXml = "";
                    m_baPhoto = null;

                    REDO:
                    try
                    {
                        // prameters:
                        //      strStyle 如何获取数据。all/xml/photo 的一个或者多个的组合
                        // return:
                        //      -1  出错
                        //      0   成功
                        //      1   重复读入未拿走的卡号
                        nRet = m_idcardObj.ReadCard("all",
                            out m_strIdcardXml,
                            out m_baPhoto,
                            out strError);
                    }
                    catch (Exception ex)
                    {
                        strError = "针对 " + Program.MainForm.IdcardReaderUrl + " 操作失败: " + ex.Message;
                        return -1;
                    }

                    if (nRet == -1)
                    {
                        /*
                        // 固定间隔重新探测一下
                        DialogResult result = MessageBox.Show(this,
"请把身份证放到读卡器上，并保持到操作完成...",
"ReaderInfoForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Asterisk,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Retry)
                            goto REDO;
                        strError = "放弃读卡";
                         * */

                        PlaceIdcardDialog dlg = new PlaceIdcardDialog();
                        MainForm.SetControlFont(dlg, this.Font, false);
                        dlg.AutoRetry = this.AutoRetryReaderCard;
                        dlg.ReadCard -= new ReadCardEventHandler(dlg_ReadCard);
                        dlg.ReadCard += new ReadCardEventHandler(dlg_ReadCard);
                        Program.MainForm.AppInfo.LinkFormState(dlg, "PlaceIdcardDialog_state");
                        dlg.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(dlg);

                        this.AutoRetryReaderCard = dlg.AutoRetry;

                        if (dlg.DialogResult == System.Windows.Forms.DialogResult.Retry)
                            goto REDO;
                        if (dlg.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            Debug.Assert(string.IsNullOrEmpty(m_strIdcardXml) == false, "");
                        }
                        else
                        {
                            Debug.Assert(dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel, "");
                            strError = "放弃读卡";
                            return 0;
                        }
                    }

                    Console.Beep(); // 表示读取成功

                    // string strLocalTempPhotoFilename = Program.MainForm.DataDir + "/~current_unsaved_patron_photo.png";
                    if (m_baPhoto != null
                    && StringUtil.IsInList("photo", this.IdcardFieldSelection) == true)
                    {
                        using (MemoryStream s = new MemoryStream(m_baPhoto))
                        {
                            Debug.Assert(image == null, "");
                            image = new Bitmap(s);
                        }

                        // image.Save(strLocalTempPhotoFilename, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    else
                    {
                        // File.Delete(strLocalTempPhotoFilename);
                    }
                    m_baPhoto = null;   // 释放空间
                }
                finally
                {
                    try
                    {
                        m_idcardObj.SendKeyEnabled = bOldSendKeyEnabled;
                    }
                    catch
                    {
                    }

                    EndIdcardChannel();
                }

                bool bSetReaderBarcode = false;
                if (StringUtil.IsInList("idcardnumber", this.IdcardFieldSelection) == true)
                {
                    if (this.DisplaySetReaderBarcodeDialog == true)
                    {
                        SetReaderBarcodeNumberDialog dlg = new SetReaderBarcodeNumberDialog();
                        MainForm.SetControlFont(dlg, this.Font, false);

                        dlg.DontAsk = !this.DisplaySetReaderBarcodeDialog;
                        dlg.InitialSelect = this.DisplaySetReaderBarcodeDialogButton;
                        Program.MainForm.AppInfo.LinkFormState(dlg, "readerinfoformm_SetReaderBarcodeNumberDialog_state");
                        dlg.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(dlg);

                        this.DisplaySetReaderBarcodeDialog = !dlg.DontAsk;
                        this.DisplaySetReaderBarcodeDialogButton = (dlg.DialogResult == System.Windows.Forms.DialogResult.Yes ? "yes" : "no");

                        bSetReaderBarcode = (dlg.DialogResult == System.Windows.Forms.DialogResult.Yes);
                    }
                    else
                    {
                        bSetReaderBarcode = (this.DisplaySetReaderBarcodeDialogButton == "yes");
                    }
                }

                string strReaderXml = "";
                if (bClear == false)
                {
                    nRet = this.readerEditControl1.GetData(
                        out strReaderXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取编辑器中现有XML时出错：" + strError;
                        return -1;
                    }
                }

                nRet = BuildReaderXml(m_strIdcardXml,
                    this.IdcardFieldSelection,
                    bSetReaderBarcode,
                    bClear,
                    ref strReaderXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                nRet = this.readerEditControl1.SetData(strReaderXml,
                    bClear == true ? "" : this.readerEditControl1.RecPath,    // 2013/6/17 如果不清除以前的内容，则也保留以前的路径
                    bClear == true ? null : this.readerEditControl1.Timestamp,  // 2013/6/27
                    out strError);
                if (nRet == -1)
                {
                    return -1;
                }

                if (StringUtil.IsInList("photo", this.IdcardFieldSelection) == true)
                {
                    // this.binaryResControl1.Clear();

                    if (image != null)
                    {
                        nRet = SetCardPhoto(image,
                    "cardphoto",
        out string strShrinkComment,
        out strError);
                        if (nRet == -1)
                            return -1;
                        image.Dispose();
                        image = null;
                    }
                }

#if NO
                Global.ClearHtmlPage(this.webBrowser_readerInfo,
                    Program.MainForm.DataDir);
#endif
                ClearReaderHtmlPage();

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在生成 HTML 预览 ...");
                stop.BeginLoop();

                EnableControls(false);

                try
                {
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    string strBarcode = strReaderXml;

                    string[] results = null;
                    long lRet = Channel.GetReaderInfo(
                        stop,
                        strBarcode,
                        "html",
                        out results,
                        out strOutputRecPath,
                        out baTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "创建读者记录 HTML 预览时发生错误: " + strError;
                        // Global.SetHtmlString(this.webBrowser_readerInfo, strError);
                        this.m_webExternalHost.SetTextString(strError);
                    }
                    else
                    {
                        string strHtml = results[0];

#if NO
                        // 2013/12/21
                        this.m_webExternalHost.StopPrevious();
                        this.webBrowser_readerInfo.Stop();

                        Global.SetHtmlString(this.webBrowser_readerInfo,
                            strHtml,
                            Program.MainForm.DataDir,
                            "readerinfoform_reader");
#endif
                        this.SetReaderHtmlString(strHtml);
                    }
                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }

                /*
                this.SetXmlToWebbrowser(this.webBrowser_xml,
                    strReaderXml);
                 * */
                Global.SetXmlToWebbrowser(this.webBrowser_xml,
Program.MainForm.DataDir,
"xml",
strReaderXml);
                if (bClear == false) // 2013/6/19
                {
                    if (Global.IsAppendRecPath(this.readerEditControl1.RecPath) == true)
                        this.m_strSetAction = "new";
                    else
                        this.m_strSetAction = "change";
                }
                else
                    this.m_strSetAction = "new";

                // this.m_strLoadSource = "idcard";
                return 1;
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
                this.EnableControls(true);
            }
        }

        void dlg_ReadCard(object sender, ReadCardEventArgs e)
        {
            try
            {
                string strError = "";

                string strTempXml = ""; // 2013/10/17
                // prameters:
                //      strStyle 如何获取数据。all/xml/photo 的一个或者多个的组合
                // return:
                //      -1  出错
                //      0   成功
                //      1   重复读入未拿走的卡号
                int nRet = m_idcardObj.ReadCard("all",
                    out strTempXml,
                    out m_baPhoto,
                    out strError);
                if (nRet != -1)
                {
                    e.Done = true;
                    Debug.Assert(string.IsNullOrEmpty(strTempXml) == false, "");
                    m_strIdcardXml = strTempXml;
                }
            }
            catch (Exception /*ex*/)
            {
            }
        }

        // 从身份证装入
        private void toolStripButton_loadFromIdcard_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 如果按住Control键使用这个功能，就表示不清除先前的内容
            bool bControl = Control.ModifierKeys == Keys.Control;

            int nRet = LoadFromIdcard(!bControl, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        private void ReaderInfoForm_Enter(object sender, EventArgs e)
        {

        }

        private void ReaderInfoForm_Leave(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem_moveRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;
            string strTargetRecPath = "";

            if (string.IsNullOrEmpty(this.readerEditControl1.RecPath) == true)
            {
                strError = "当前记录的路径为空，无法进行移动操作";
                goto ERROR1;
            }

            if (this.ReaderXmlChanged == true
    || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
"当前有信息被修改后尚未保存。若此时进行移动操作，现有未保存信息将丢失。\r\n\r\n确实要进行移动操作? ",
"ReaderInfoForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;   // cancelled

            }

            // 出现对话框，让用户可以选择目标库
            ReaderSaveToDialog saveto_dlg = new ReaderSaveToDialog();
            MainForm.SetControlFont(saveto_dlg, this.Font, false);
            saveto_dlg.Text = "移动读者记录";
            saveto_dlg.MessageText = "请选择要移动去的目标记录位置";
            // saveto_dlg.MainForm = Program.MainForm;
            saveto_dlg.RecPath = this.readerEditControl1.RecPath;
            saveto_dlg.RecID = "?";

            Program.MainForm.AppInfo.LinkFormState(saveto_dlg, "readerinfoform_movetodialog_state");
            saveto_dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(saveto_dlg);

            if (saveto_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在移动读者记录 " + this.readerEditControl1.RecPath + " 到 " + saveto_dlg.RecPath + "...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                strTargetRecPath = saveto_dlg.RecPath;

                byte[] target_timestamp = null;
                long lRet = Channel.MoveReaderInfo(
    stop,
    this.readerEditControl1.RecPath,
    ref strTargetRecPath,
    out target_timestamp,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            // 重新装载窗口内容
            Debug.Assert(string.IsNullOrEmpty(strTargetRecPath) == false, "");
            LoadRecordByRecPath(strTargetRecPath,
                "");

            MessageBox.Show(this, "移动成功。");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

#if NO
        bool m_bSuppressScriptErrors = true;
        public bool SuppressScriptErrors
        {
            get
            {
                return this.m_bSuppressScriptErrors;
            }
            set
            {
                this.m_bSuppressScriptErrors = value;
            }
        }
#endif

        private void webBrowser_readerInfo_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (Program.MainForm.SuppressScriptErrors == true)
                e.Handled = true;
        }

        private void readerEditControl1_GetLibraryCode(object sender, GetLibraryCodeEventArgs e)
        {
            e.LibraryCode = Program.MainForm.GetReaderDbLibraryCode(e.DbName);
        }


        #region 指纹登记功能

        // 局部更新指纹信息高速缓存
        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        int UpdateFingerprintCache(
            string strBarcode,
            string strFingerprint,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == true)
            {
                strError = "尚未配置 指纹阅读器URL 系统参数，无法更新指纹高速缓存";
                return -1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.FingerprintReaderUrl,
                out strError);
            if (channel == null)
                return -1;
            _inFingerprintCall++;
            try
            {
                List<FingerprintItem> items = new List<FingerprintItem>();

                FingerprintItem item = new FingerprintItem();
                item.ReaderBarcode = strBarcode;
                item.FingerprintString = strFingerprint;
                items.Add(item);

                // return:
                //      -2  remoting服务器连接失败。驱动程序尚未启动
                //      -1  出错
                //      0   成功
                int nRet = AddItems(
                    channel,
                    items,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == -2)
                    return -2;
            }
            finally
            {
                _inFingerprintCall--;
                EndFingerprintChannel(channel);
            }

            return 0;
        }

        async Task<GetFingerprintStringResult> CancelReadFingerprintString()
        {
            string strError = "";
            GetFingerprintStringResult result = new GetFingerprintStringResult();

            if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == true)
            {
                strError = "尚未配置 指纹阅读器URL 系统参数，无法读取指纹信息";
                goto ERROR1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.FingerprintReaderUrl,
                out strError);
            if (channel == null)
                goto ERROR1;

            _inFingerprintCall++;
            try
            {
                try
                {
                    return await Task.Factory.StartNew<GetFingerprintStringResult>(
                        () =>
                        {
                            GetFingerprintStringResult temp_result = new GetFingerprintStringResult();
                            try
                            {
                                temp_result.Value = channel.Object.CancelGetFingerprintString();
                                if (temp_result.Value == -1)
                                    temp_result.ErrorInfo = "API cancel return error";
                                return temp_result;
                            }
                            catch (RemotingException ex)
                            {
                                temp_result.ErrorInfo = ex.Message;
                                temp_result.Value = 0;  // 让调主认为没有出错
                                return temp_result;
                            }
                            catch (Exception ex)
                            {
                                temp_result.ErrorInfo = ex.Message;
                                temp_result.Value = -1;
                                return temp_result;
                            }
                        });
                }
                catch (Exception ex)
                {
                    strError = "针对 " + Program.MainForm.FingerprintReaderUrl + " 的 GetFingerprintString() 操作失败: " + ex.Message;
                    goto ERROR1;
                }
            }
            finally
            {
                _inFingerprintCall--;
                EndFingerprintChannel(channel);
            }
            ERROR1:
            result.ErrorInfo = strError;
            result.Value = -1;
            return result;
        }

        // return:
        //      -1  error
        //      0   放弃输入
        //      1   成功输入
        async Task<GetFingerprintStringResult> ReadFingerprintString(string strExcludeBarcodes)
        {
            string strError = "";
            GetFingerprintStringResult result = new GetFingerprintStringResult();

            if (string.IsNullOrEmpty(Program.MainForm.FingerprintReaderUrl) == true)
            {
                strError = "尚未配置 指纹阅读器URL 系统参数，无法读取指纹信息";
                goto ERROR1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.FingerprintReaderUrl,
                out strError);
            if (channel == null)
                goto ERROR1;

            _inFingerprintCall++;
            try
            {
#if NO
                    // 获得一个指纹特征字符串
                    // return:
                    //      -1  error
                    //      0   放弃输入
                    //      1   成功输入
                    nRet = m_fingerPrintObj.GetFingerprintString(out strFingerprint,
                        out strVersion,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    return nRet;
#endif
                return await GetFingerprintString(channel, strExcludeBarcodes);
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.FingerprintReaderUrl + " 的 GetFingerprintString() 操作失败: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                _inFingerprintCall--;
                EndFingerprintChannel(channel);
            }
            ERROR1:
            result.ErrorInfo = strError;
            result.Value = -1;
            return result;
        }

        class GetFingerprintStringResult
        {
            public string Fingerprint { get; set; }
            public string Version { get; set; }

            public int Value { get; set; }
            public string ErrorInfo { get; set; }
        }

        Task<GetFingerprintStringResult> GetFingerprintString(FingerprintChannel channel,
            string strExcludeBarcodes)
        {
            return Task.Factory.StartNew<GetFingerprintStringResult>(
    () =>
    {
        return CallGetFingerprintString(channel, strExcludeBarcodes);
    });
        }

        GetFingerprintStringResult CallGetFingerprintString(FingerprintChannel channel,
            string strExcludeBarcodes)
        {
            GetFingerprintStringResult result = new GetFingerprintStringResult();
            try
            {
                // 先尝试 2.0 版本
                try
                {
                    int nRet = channel.Object.GetFingerprintString(
                        strExcludeBarcodes,
                        out string strFingerprint,
                        out string strVersion,
                        out string strError);
                    result.Fingerprint = strFingerprint;
                    result.Version = strVersion;
                    result.ErrorInfo = strError;
                    result.Value = nRet;
                    if (nRet == -1)
                    {
                        if (strVersion != "[not support]")
                            return result;
                    }
                    else
                        return result;
                }
                catch (System.Runtime.Remoting.RemotingException)
                {
                }

                // 然后尝试用 V1.0 调用方式
                {
                    // 获得一个指纹特征字符串
                    // return:
                    //      -1  error
                    //      0   放弃输入
                    //      1   成功输入
                    int nRet = channel.Object.GetFingerprintString(out string strFingerprint,
                    out string strVersion,
                    out string strError);

                    result.Fingerprint = strFingerprint;
                    result.Version = strVersion;
                    result.ErrorInfo = strError;
                    result.Value = nRet;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.ErrorInfo = "GetFingerprintString() 异常: " + ex.Message;
                result.Value = -1;
                return result;
            }
        }

        public class GetVersionResult : NormalResult
        {
            public string CfgInfo { get; set; }
            public string Version { get; set; }
        }

        public static GetVersionResult CallGetVersion(FingerprintChannel channel)
        {
            GetVersionResult result = new GetVersionResult();
            try
            {
                // 获得一个指纹特征字符串
                // return:
                //      -1  error
                //      0   放弃输入
                //      1   成功输入
                int nRet = channel.Object.GetVersion(out string strVersion,
                    out string strCfgInfo,
                    out string strError);

                result.CfgInfo = strCfgInfo;
                result.Version = strVersion;
                result.ErrorInfo = strError;
                result.Value = nRet;

                // 2019/2/19
                channel.Version = result.Version;
                channel.CfgInfo = result.CfgInfo;

                return result;
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
                result.CfgInfo = "";
                result.Version = "1.0";
                result.ErrorInfo = "";
                result.Value = 0;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorInfo = "CallGetVersion() 异常: " + ex.Message;
                result.Value = -1;
                return result;
            }
        }

        #endregion

        private async void toolStripButton_registerFingerprint_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bPractice = (Control.ModifierKeys == Keys.Control);

            this.ShowMessage("等待扫描指纹 ...");
            this.EnableControls(false);
            // Program.MainForm.StatusBarMessage = "等待扫描指纹...";
            try
            {
                NormalResult getstate_result = await FingerprintGetState("");
                if (getstate_result.Value == -1)
                {
                    strError = $"指纹中心当前状态不正确：{getstate_result.ErrorInfo}";
                    goto ERROR1;
                }

                getstate_result = await FingerprintGetState("getLibraryServerUID");
                if (getstate_result.Value == -1)
                {
                    strError = getstate_result.ErrorInfo;
                    goto ERROR1;
                }
                else if (getstate_result.ErrorCode != null &&
                    getstate_result.ErrorCode != Program.MainForm.ServerUID)
                {
                    strError = $"指纹中心所连接的 dp2library 服务器 UID {getstate_result.ErrorCode} 和内务当前所连接的 UID {Program.MainForm.ServerUID} 不同。无法进行指纹登记";
                    goto ERROR1;
                }

                REDO:
                GetFingerprintStringResult result = await ReadFingerprintString(
                    bPractice == true ? "!practice" : this.readerEditControl1.Barcode);
                if (result.Value == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
result.ErrorInfo + "\r\n\r\n是否重试?",
"ReaderInfoForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO;
                }

                if (result.Value == -1 || result.Value == 0)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

#if NO
                strFingerprint = "12345";   // test
                strVersion = "test-version";
#endif

                if (bPractice == false)
                {
                    this.readerEditControl1.FingerprintFeature = result.Fingerprint;   // strFingerprint;
                    this.readerEditControl1.FingerprintFeatureVersion = result.Version;    // strVersion;
                    this.readerEditControl1.Changed = true;
                }
            }
            finally
            {
                this.EnableControls(true);
                this.ClearMessage();
            }

            // MessageBox.Show(this, strFingerprint);
            Program.MainForm.StatusBarMessage = "指纹信息获取成功";
            return;
            ERROR1:
            Program.MainForm.StatusBarMessage = strError;
            ShowMessageBox(strError);
        }

        private void toolStripMenuItem_clearFingerprint_Click(object sender, EventArgs e)
        {
            this.readerEditControl1.FingerprintFeatureVersion = "";
            this.readerEditControl1.FingerprintFeature = "";
            this.readerEditControl1.Changed = true;
        }

        // 导出在借册条码号到文本文件
        private void ToolStripMenuItem_exportBorrowingBarcode_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的条码号文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            // dlg.FileName = this.ExportTextFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "文本文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            bool bAppend = true;

            if (File.Exists(dlg.FileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文本文件 '" + dlg.FileName + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃输出)",
                    "ReaderInfoForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
            }

            string strNewXml = "";
            int nRet = this.readerEditControl1.GetData(
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML到DOM出错: " + ex.Message;
                goto ERROR1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");

            using (StreamWriter sw = new StreamWriter(dlg.FileName, bAppend, Encoding.UTF8))
            {
                foreach (XmlElement node in nodes)
                {
                    string strBarcode = node.GetAttribute("barcode");
                    if (string.IsNullOrEmpty(strBarcode) == false)
                        sw.WriteLine(strBarcode);
                }
            }

            Program.MainForm.StatusBarMessage = "导出成功。";
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripTextBox_barcode_Enter(object sender, EventArgs e)
        {
            if (m_nChannelInUse > 0)
                return;
            Program.MainForm.EnterPatronIdEdit(InputType.PQR);

            // 2013/5/25
            // 禁止身份证读卡器键盘仿真的时候，证条码号输入域例外
            if (this.DisableIdcardReaderSendkey == true)
            {
                EnableSendKey(true);
            }

            // Debug.WriteLine("Barcode textbox focued");

        }

        private void toolStripTextBox_barcode_Leave(object sender, EventArgs e)
        {
            // 2014/10/12
            if (Program.MainForm == null)
                return;

            Program.MainForm.LeavePatronIdEdit();

            // 2013/5/25
            // 禁止身份证读卡器键盘仿真的时候，证条码号输入域例外
            if (this.DisableIdcardReaderSendkey == true)
            {
                EnableSendKey(false);
            }
            // Debug.WriteLine("Barcode textbox leave");
        }

        private void readerEditControl1_CreatePinyin(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strHanzi = this.readerEditControl1.NameString;
            if (string.IsNullOrEmpty(strHanzi) == true)
            {
                strError = "尚未输入读者姓名，因此无法创建姓名拼音";
                goto ERROR1;
            }

            this.EnableControls(false);
            try
            {
                string strPinyin = "";
                // return:
                //      -1  出错
                //      0   用户中断选择
                //      1   成功
                nRet = Program.MainForm.GetPinyin(
                    this,
                    strHanzi,
                    PinyinStyle.None,
                    false,
                    out strPinyin,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.readerEditControl1.NamePinyin = strPinyin;
            }
            finally
            {
                this.EnableControls(true);
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 注：此功能较旧，其菜单已经被隐藏
        // 导出到 Excel 文件
        private void toolStripMenuItem_exportExcel_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strNewXml = "";
            nRet = this.readerEditControl1.GetData(
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "装载XML到DOM出错: " + ex.Message;
                goto ERROR1;
            }


            // 构造一个特定的文件名
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

            string strFileName = strName + "_" + strBarcode + ".xlsx";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = strFileName;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.EnableControls(false);
            try
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");

                ExcelDocument doc = ExcelDocument.Create(dlg.FileName);
                try
                {
                    doc.NewSheet("Sheet1");

                    int nColIndex = 0;
                    int _lineIndex = 0;

                    // 姓名
                    List<CellData> cells = new List<CellData>();
                    cells.Add(new CellData(nColIndex++, "姓名"));
                    cells.Add(new CellData(nColIndex++, strName));
                    doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                    _lineIndex++;

                    // 证条码号
                    nColIndex = 0;
                    cells = new List<CellData>();
                    cells.Add(new CellData(nColIndex++, "证条码号"));
                    cells.Add(new CellData(nColIndex++, strBarcode));
                    doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                    _lineIndex++;

                    // 空行
                    _lineIndex++;

                    // 标题 在借册
                    // TODO: 最好跨越多栏
                    nColIndex = 0;
                    cells = new List<CellData>();
                    cells.Add(new CellData(nColIndex++, "在借册(" + nodes.Count.ToString() + ")"));
                    doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                    _lineIndex++;

                    // 表格栏目标题行
                    nColIndex = 0;
                    cells = new List<CellData>();
                    cells.Add(new CellData(nColIndex++, "册条码号"));
                    cells.Add(new CellData(nColIndex++, "书目摘要"));
                    cells.Add(new CellData(nColIndex++, "借阅时间"));
                    cells.Add(new CellData(nColIndex++, "借阅期限"));

                    doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                    _lineIndex++;


                    foreach (XmlElement node in nodes)
                    {
                        nColIndex = 0;
                        cells = new List<CellData>();

                        string strItemBarcode = node.GetAttribute("barcode");
                        string strConfirmItemRecPath = node.GetAttribute("recPath");
                        string strBorrowDate = node.GetAttribute("borrowDate");
                        string strBorrowPeriod = node.GetAttribute("borrowPeriod");
                        string strSummary = "";
                        nRet = Program.MainForm.GetBiblioSummary(strItemBarcode,
                            strConfirmItemRecPath,
                            true,
                            out strSummary,
                            out strError);
                        if (nRet == -1)
                            strSummary = strError;

                        cells.Add(new CellData(nColIndex++, strItemBarcode));
                        cells.Add(new CellData(nColIndex++, strSummary));
                        cells.Add(new CellData(nColIndex++, DateTimeUtil.LocalTime(strBorrowDate)));
                        cells.Add(new CellData(nColIndex++, strBorrowPeriod));

                        doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                        _lineIndex++;
                    }

                    // 空行
                    _lineIndex++;
                    // create time
                    {
                        _lineIndex++;
                        cells = new List<CellData>();
                        cells.Add(new CellData(0, "本文件创建时间"));
                        cells.Add(new CellData(1, DateTime.Now.ToString()));
                        doc.WriteExcelLine(_lineIndex, cells);

                        _lineIndex++;
                    }

                }
                finally
                {
                    doc.SaveWorksheet();
                    doc.Close();
                }
            }
            finally
            {
                this.EnableControls(true);
            }

            Program.MainForm.StatusBarMessage = "导出成功。";
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// MDI子窗口被通知事件发生
        /// </summary>
        /// <param name="e">事件类型</param>
        public override void OnNotify(ParamChangedEventArgs e)
        {
            if (e.Section == "valueTableCacheCleared")
            {
                this.readerEditControl1.OnValueTableCacheCleared();
            }
        }

        private void readerEditControl1_EditRights(object sender, EventArgs e)
        {
            DigitalPlatform.CommonControl.PropertyDlg dlg = new DigitalPlatform.CommonControl.PropertyDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            string strPatronRightsCfgFileName = Path.Combine(Program.MainForm.UserDir, "patronrights.xml");

            string strRightsCfgFileName = Path.Combine(Program.MainForm.UserDir, "objectrights.xml");

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Text = "当前读者的权限";
            dlg.PropertyString = this.readerEditControl1.Rights;
            if (File.Exists(strPatronRightsCfgFileName) == true
                && Control.ModifierKeys != Keys.Control)
                dlg.CfgFileName = strPatronRightsCfgFileName;   // 优先用读者权限定义配置文件
            else
                dlg.CfgFileName = Path.Combine(Program.MainForm.DataDir, "userrightsdef.xml");
            if (File.Exists(strRightsCfgFileName) == true)
                dlg.CfgFileName += "," + strRightsCfgFileName;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.readerEditControl1.Rights = dlg.PropertyString;
        }

        // 加好友
        private void toolStripButton_addFriends_Click(object sender, EventArgs e)
        {
            // 因为可能要刷新窗口，因此要求操作前修改已经保存
            if (this.ReaderXmlChanged == true
    || this.ObjectChanged == true)
            {
                MessageBox.Show(this, "当前有信息被修改后尚未保存。必须先保存后，才能进行加好友的操作。");
                return;
            }

            AddFriendsDialog dlg = new AddFriendsDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_AddFriendsDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            string strError = "";
            long lRet = 0;


            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在加好友 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                // Result.Value -1出错 0请求成功(注意，并不代表对方同意) 1:请求前已经是好友关系了，没有必要重复请求 2:已经成功添加
                lRet = Channel.SetFriends(
    stop,
    "request",
    dlg.ReaderBarcode,
    dlg.Comment,
    "",
    out strError);
                if (lRet == -1 || lRet == 1)
                {
                    goto ERROR1;
                }

                if (lRet == 0)
                    strError = "请求已经发出，正等待对方同意";
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            if (lRet == 2)
            {
                // TODO: 需要立即刷新窗口，兑现 firends 字段的更新显示
                MessageBox.Show(this, "好友字段已经被修改，请注意重新装载读者记录");
            }
            Program.MainForm.StatusBarMessage = strError;
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripMenuItem_exportDetailToExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strNewXml = "";
            int nRet = this.readerEditControl1.GetData(
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            List<string> xmls = new List<string>();
            xmls.Add(strNewXml);

            // 创建读者详情 Excel 文件。这是便于被外部调用的版本，只需要提供读者 XML 记录即可
            // return:
            //      -1  出错
            //      0   用户中断
            //      1   成功
            nRet = ReaderSearchForm.CreateReaderDetailExcelFile(xmls,
                Program.MainForm.GetBiblioSummary,
                null,
                false,
                true,
                out strError);
            if (nRet != 1)
                goto ERROR1;
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void LoadBorrowHistory(string action)
        {
            string strError = "";
            int nPageNo = 0;
            if (action == "load")
                nPageNo = 0;
            else if (action == "loadAll")
                nPageNo = -1;
            else if (action == "prevPage")
            {
                nPageNo = _currentPageNo - 1;
                if (nPageNo < 0)
                {
                    strError = "已经到头";
                    goto ERROR1;
                }
            }
            else if (action == "nextPage")
            {
                nPageNo = _currentPageNo + 1;
                if (nPageNo > _pageCount - 1)
                {
                    strError = "已经到尾";
                    goto ERROR1;
                }
            }
            else if (action == "firstPage")
                nPageNo = 0;
            else if (action == "tailPage")
            {
                if (_pageCount <= 0)
                {
                    strError = "没有尾页";
                    goto ERROR1;
                }
                nPageNo = _pageCount - 1;
            }

            string strBarcode = this.toolStripTextBox_barcode.Text;
            int nRet = LoadBorrowHistory(
                string.IsNullOrEmpty(strBarcode) == true ? "!all" : strBarcode,
                nPageNo,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
            ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        static int _itemsPerPage = 10;

        // parameters:
        //      nPageNo 页号。如果为 -1，表示希望从头获取全部内容
        int LoadBorrowHistory(
            string strBarcode,
            int nPageNo,
            out string strError)
        {
            strError = "";

            if (stop != null)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在装载借阅历史 ...");
                stop.BeginLoop();
            }

            EnableControls(false);
            try
            {
                List<ChargingItemWrapper> total_results = new List<ChargingItemWrapper>();

                int nLength = 0;
                if (nPageNo == -1)
                {
                    nPageNo = 0;
                    nLength = -1;
                }
                else
                {
                    nLength = _itemsPerPage;
                }

                long lRet = 0;
                this.ChannelDoEvents = true;
                // this.Channel.Idle += Channel_Idle;  // 防止控制权出让给正在获取摘要的读者信息 HTML 页面
                try
                {
                    lRet = this.Channel.LoadChargingHistory(
                        stop,
                        strBarcode,
                        "return,lost,read",
                        nPageNo,
                        nLength,
                        out total_results,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    _currentPageNo = nPageNo;
                }
                finally
                {
                    // this.Channel.Idle -= Channel_Idle;
                    this.ChannelDoEvents = true;
                }

                FillBorrowHistoryPage(total_results, nPageNo * _itemsPerPage, (int)lRet);
                return 0;
            }
            finally
            {
                EnableControls(true);

                if (stop != null)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
        }

#if NO
        // parameters:
        //      nPageNo 页号。如果为 -1，表示希望从头获取全部内容
        int LoadBorrowHistory(
            string strBarcode,
            int nPageNo,
            out string strError)
        {
            strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装载借阅历史 ...");
            stop.BeginLoop();

            EnableControls(false);
            try
            {
                List<ChargingItemWrapper> total_results = new List<ChargingItemWrapper>();
                long lHitCount = 0;

                long lLength = 0;
                long lStart = 0;
                if (nPageNo == -1)
                    lLength = -1;
                else
                {
                    lStart = nPageNo * _itemsPerPage;
                    lLength = _itemsPerPage;
                }
                int nGeted = 0;
                this.Channel.Idle += Channel_Idle;  // 防止控制权出让给正在获取摘要的读者信息 HTML 页面
                try
                {
                    for (; ; )
                    {
                        ChargingItemWrapper[] results = null;
                        long lRet = this.Channel.SearchCharging(
                            stop,
                            strBarcode,
                            "~",
                            "return,lost",
                            "descending",
                            lStart + nGeted,
                            lLength,
                            out results,
                            out strError);
                        if (lRet == -1)
                            return -1;
                        if (results.Length == 0)
                            break;
                        total_results.AddRange(results);
                        lHitCount = lRet;

                        // 修正 lLength
                        if (lLength != -1 && lHitCount < lStart + nGeted + lLength)
                            lLength -= lStart + nGeted + lLength - lHitCount;

                        if (total_results.Count >= lHitCount - lStart)
                            break;

                        nGeted += results.Length;
                        if (lLength != -1)
                            lLength -= results.Length;
                    }

                }
                finally
                {
                    this.Channel.Idle -= Channel_Idle;
                }

                FillBorrowHistoryPage(total_results, (int)lStart, (int)lHitCount);
                return 0;
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }
#endif

#if NO
        void Channel_Idle(object sender, IdleEventArgs e)
        {
            // e.bDoEvents = false;
        }
#endif

        void m_chargingInterface_CallFunc(object sender, EventArgs e)
        {
            string name = sender as string;
            this.BeginInvoke(new Action<string>(LoadBorrowHistory), name);
        }

        void ClearBorrowHistoryPage()
        {
            ClearHtml();

            string strItemLink = "<a href='javascript:void(0);' onclick=\"window.external.Call('load');\">" + HttpUtility.HtmlEncode("装载") + "</a>";

            AppendHtml("<html><body>");
            AppendHtml(strItemLink);
            AppendHtml("</body></html>");

            _borrowHistoryLoaded = false;
        }

        void ClearQrCodePage()
        {
            _qrCodeLoaded = false;

            ImageUtil.SetImage(this.pictureBox_qrCode, null);   // 2016/12/28
            this.textBox_pqr.Text = "";
        }

        int _currentPageNo = 0;
        int _pageCount = 0;

        static string MakeAnchor(string name, string caption, bool enabled)
        {
            if (enabled)
                return "<a href='javascript:void(0);' onclick=\"window.external.Call('" + name + "');\">" + HttpUtility.HtmlEncode(caption) + "</a>";
            return HttpUtility.HtmlEncode(caption);
        }

        public static string GetOperTypeName(string strAction)
        {
            if (strAction == "return")
                return "借过";
            if (strAction == "lost")
                return "借过(丢失)";
            if (strAction == "read")
                return "读过";
            if (strAction == "boxing")
                return "配书";
            return strAction;
        }

        void FillBorrowHistoryPage(List<ChargingItemWrapper> items,
            int nStart,
            int nTotalCount)
        {
            this.ClearMessage();

            StringBuilder text = new StringBuilder();

            _currentPageNo = nStart / _itemsPerPage;
            _pageCount = nTotalCount / _itemsPerPage;
            if ((nTotalCount % _itemsPerPage) > 0)
                _pageCount++;

            // string strBinDir = Environment.CurrentDirectory;
            string strBinDir = Program.MainForm.UserDir;    // 2017/2/23

            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "default\\charginghistory.css");
            string strSummaryJs = Path.Combine(Program.MainForm.DataDir, "getsummary.js");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strScriptHead = "<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-1.4.4.min.js\"></script>"
                + "<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-ui-1.8.7.min.js\"></script>"
                + "<script type='text/javascript' charset='UTF-8' src='" + strSummaryJs + "'></script>";
            string strStyle = @"<style type='text/css'>
</style>";
            text.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                + strLink
                + strScriptHead.Replace("%bindir%", strBinDir)
                + strStyle
                + "</head><body>");

            string strFirstPageLink = MakeAnchor("firstPage", "首页", _currentPageNo > 0);
            string strPrevPageLink = MakeAnchor("prevPage", "前页", _currentPageNo > 0);
            string strNextPageLink = MakeAnchor("nextPage", "后页", _currentPageNo < _pageCount - 1);
            string strTailPageLink = MakeAnchor("tailPage", "末页", _currentPageNo != _pageCount - 1 && _pageCount > 0);
            string strLoadAllLink = MakeAnchor("loadAll", "装载全部", _pageCount > 1);

            string strPages = (_currentPageNo + 1) + "/" + _pageCount + "&nbsp;";
            if (items.Count > _itemsPerPage)
                strPages = "(全部)";

            text.Append(strPages
                + strFirstPageLink + "&nbsp;" + strPrevPageLink + "&nbsp;" + strNextPageLink + "&nbsp;" + strTailPageLink + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
                + strLoadAllLink);

            text.Append("<table>");
            text.Append("<tr>");
            text.Append("<td class='nowrap'>序号</td>");
            text.Append("<td class='nowrap'>类型</td>");
            text.Append("<td class='nowrap'>册条码号</td>");
            text.Append("<td class='nowrap'>书目摘要</td>");
            text.Append("<td class='nowrap'>卷册</td>");
            text.Append("<td class='nowrap'>期限</td>");
            text.Append("<td class='nowrap'>借阅操作者</td>");
            text.Append("<td class='nowrap'>借阅操作时间</td>");
            text.Append("<td class='nowrap'>还回操作者</td>");
            text.Append("<td class='nowrap'>还回操作时间</td>");
            text.Append("</tr>");

            foreach (ChargingItemWrapper wrapper in items)
            {
                ChargingItem item = wrapper.Item;
                text.Append("<tr class='" + HttpUtility.HtmlEncode(item.Action) + "'>");
                text.Append("<td>" + (nStart + 1).ToString() + "</td>");
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(GetOperTypeName(item.Action)) + "</td>");

                string strItemBarcode = item.ItemBarcode;
                if (string.IsNullOrEmpty(strItemBarcode) == true
                    && string.IsNullOrEmpty(item.BiblioRecPath) == false)
                    strItemBarcode = "@biblioRecPath:" + item.BiblioRecPath;

                if (string.IsNullOrEmpty(strItemBarcode) == false
                    && (strItemBarcode.StartsWith("@refID:") == true || strItemBarcode.StartsWith("@biblioRecPath:") == true))
                    text.Append("<td>" + HttpUtility.HtmlEncode(strItemBarcode) + "</td>");
                else
                    text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(strItemBarcode) + "</td>");

                text.Append("<td class='summary pending'>BC:" + HttpUtility.HtmlEncode(strItemBarcode) + "</td>");

                string strVolume = item.Volume;
                if (string.IsNullOrEmpty(strVolume))
                {
                    if (wrapper.RelatedItem != null)
                        strVolume = wrapper.RelatedItem.Volume;
                }
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(strVolume) + "</td>");

                string strPeriod = "";
                if (wrapper.RelatedItem != null)
                    strPeriod = wrapper.RelatedItem.Period;
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(strPeriod) + "</td>");

                string strBorrowOperator = "";
                string strBorrowTime = "";
                if (wrapper.RelatedItem != null)
                {
                    strBorrowOperator = wrapper.RelatedItem.Operator;
                    strBorrowTime = wrapper.RelatedItem.OperTime;
                }
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(strBorrowOperator) + "</td>");
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(strBorrowTime) + "</td>");

                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(item.Operator) + "</td>");
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(item.OperTime) + "</td>");

                text.Append("</tr>");
                nStart++;
            }
            text.Append("</table>");
            text.Append("</body></html>");

            this.m_chargingInterface.SetHtmlString(text.ToString(),
    "readerinfoform_charginghis");
        }

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "default\\charginghistory.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = this.webBrowser_borrowHistory.Document;

                if (doc == null)
                {
                    this.webBrowser_borrowHistory.Navigate("about:blank");
                    doc = this.webBrowser_borrowHistory.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser_borrowHistory,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser_borrowHistory,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser_borrowHistory.Document.Window.ScrollTo(0,
                this.webBrowser_borrowHistory.Document.Body.ScrollRectangle.Height);
        }

        bool _borrowHistoryLoaded = false;
        bool _qrCodeLoaded = false;

        private void tabControl_readerInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_readerInfo.SelectedTab == this.tabPage_borrowHistory)
            {
                if (_borrowHistoryLoaded == false)
                {
                    this.BeginInvoke(new Action<string>(LoadBorrowHistory), "load");
                    _borrowHistoryLoaded = true;
                }
            }

            if (this.tabControl_readerInfo.SelectedTab == this.tabPage_qrCode)
            {
                if (_qrCodeLoaded == false)
                {
                    this.BeginInvoke(new Action(LoadQrCode));
                    _qrCodeLoaded = true;
                }
            }
        }

        void LoadQrCode()
        {
            string strError = "";
            string strCode = "";

            ImageUtil.SetImage(this.pictureBox_qrCode, null);   // 2016/12/28
            this.textBox_pqr.Text = "";

            if (string.IsNullOrEmpty(this.ReaderBarcode) == true)
                return;

            int nRet = GetPatronTempId(
                this.ReaderBarcode,
                out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_pqr.Text = strCode;

            string strCharset = "ISO-8859-1";
            bool bDisableECI = false;

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                // Options = new EncodingOptions
                Options = new QrCodeEncodingOptions
                {
                    Height = 400,
                    Width = 400,
                    DisableECI = bDisableECI,
                    ErrorCorrection = ErrorCorrectionLevel.L,
                    CharacterSet = strCharset // "UTF-8"
                }
            };

#if NO
            using (var bitmap = writer.Write(strCode))
            {
                this.pictureBox_qrCode.Image = bitmap;
            }
#endif
            ImageUtil.SetImage(this.pictureBox_qrCode, writer.Write(strCode));  // 2016/12/28
            return;
            ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        // 获得读者证号二维码字符串
        public int GetPatronTempId(
            string strReaderBarcode,
            out string strCode,
            out string strError)
        {
            strError = "";
            strCode = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得读者二维码 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
            try
            {

                // 读入读者记录
                long lRet = channel.VerifyReaderPassword(stop,
                    "!getpatrontempid:" + strReaderBarcode,
                    null,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    strError = "获得读者证号二维码时发生错误: " + strError;
                    return -1;
                }

                strCode = strError;
                return 0;
            }
            finally
            {
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }

        // 编辑读者记录 XML
        private void toolStripMenuItem_editXML_Click(object sender, EventArgs e)
        {
            int nRet = this.readerEditControl1.GetData(out string strXml,
                out string strError);
            if (nRet == -1)
                goto ERROR1;
            strXml = DomUtil.GetIndentXml(strXml);
            string result = EditDialog.GetInput(this,
                "readerInfoForm",
                "读者记录 XML",
                strXml,
                this.Font);
            if (result == null)
                return;

            nRet = this.readerEditControl1.SetData(result,
                this.readerEditControl1.RecPath,
                this.readerEditControl1.Timestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "xml",
    result);

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 登记人脸。用于人脸识别
        private async void toolStripSplitButton_registerFace_ButtonClick(object sender, EventArgs e)
        {
            string strError = "";
            this.ShowMessage("等待登记人脸 ...");
            this.EnableControls(false);
            try
            {
                REDO:
                GetFeatureStringResult result = await ReadFeatureString(
                    null,
                    this.readerEditControl1.Barcode,
                    "ui,confirmPicture,returnImage");
                if (result.Value == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
result.ErrorInfo + "\r\n\r\n是否重试?",
"ReaderInfoForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO;
                }

                if (result.Value == -1 || result.Value == 0)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

                this.readerEditControl1.FaceFeature = result.FeatureString;
                this.readerEditControl1.FaceFeatureVersion = result.Version;
                this.readerEditControl1.Changed = true;

                // TODO: 如果尺寸符合要求，则直接用返回的 jpeg 上载
                // 设置人脸照片对象
                using (Image image = FromBytes(result.ImageData))
                using (Image image1 = new Bitmap(image))
                {
                    // 自动缩小图像
                    int nRet = SetCardPhoto(image1,
                        "face",
                        out string strShrinkComment,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
            }
            finally
            {
                this.EnableControls(true);
                this.ClearMessage();
            }

            // MessageBox.Show(this, strFingerprint);
            Program.MainForm.StatusBarMessage = "人脸信息获取成功";
            // TODO: 记住保存记录时通知 facecenter DoReplication
            return;
            ERROR1:
            Program.MainForm.StatusBarMessage = strError;
            ShowMessageBox(strError);
        }

        static Image FromBytes(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Image.FromStream(stream);
            }
        }

        // (从剪贴板)粘贴证件照(注意证件照是用途为 cardphoto 的对象，和人脸识别无关)
        private void ToolStripMenuItem_pasteCardPhoto_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 从剪贴板中取得图像对象
            List<Image> images = ImageUtil.GetImagesFromClipboard(out strError);
            if (images == null)
            {
                if (string.IsNullOrEmpty(strError) == true)
                    strError = "当前剪贴板为空";
                strError = $"{strError}。无法创建证件照片";
                goto ERROR1;
            }
            Image image = images[0];

            string strShrinkComment = "";
            using (image)
            {
                // 自动缩小图像
                nRet = SetCardPhoto(image,
                    "cardphoto",
                out strShrinkComment,
                out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 切换到对象属性页，以便操作者能看到刚刚创建的对象行
            this.tabControl_readerInfo.SelectedTab = this.tabPage_objects;

            MessageBox.Show(this, "证件照片已经成功创建。\r\n"
                + strShrinkComment
                + "\r\n\r\n(但因当前读者记录还未保存，图像数据尚未提交到服务器)\r\n\r\n注意稍后保存当前读者记录。");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 创建 15693 读者卡
        private void toolStripMenuItem_createRfidCard_Click(object sender, EventArgs e)
        {
            string library_code = Program.MainForm.GetReaderDbLibraryCode(Global.GetDbName(this.ReaderEditControl.RecPath));
            RfidPatronCardDialog dlg = new RfidPatronCardDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.SetData(this.ReaderEditControl, library_code);
            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_RfidPatronCardDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);
        }

        private void toolStripMenuItem_bindCardNumber_Click(object sender, EventArgs e)
        {
            BindCardNumberDialog dlg = new BindCardNumberDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Numbers = this.readerEditControl1.CardNumber;
            Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_BindCardNumberDialog_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;
            this.readerEditControl1.CardNumber = dlg.Numbers;
        }

        // 清除人脸特征和图片
        private void toolStripMenuItem_clearFaceFeature_Click(object sender, EventArgs e)
        {
            this.readerEditControl1.FaceFeatureVersion = "";
            this.readerEditControl1.FaceFeature = "";
            this.readerEditControl1.Changed = true;

            // 标记删除 usage 为 "face" 的对象
            List<ListViewItem> items = this.binaryResControl1.FindItemByUsage("face");
            if (items.Count > 0)
                this.binaryResControl1.MaskDelete(items);

            // TODO: 注意保存记录的时候通知 facecenter 及时同步刷新信息
            // 可能需要建立一种标志，表示人脸相关信息修改过
            MessageBox.Show(this, "人脸特征信息和图片已经清除。但读者记录尚未保存");
        }

        // 一般保存
        private void toolStripSplitButton_save_ButtonClick(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_SAVE_RECORD);
        }

        // 能修改册条码号的保存
        private void ToolStripMenuItem_saveChangeBarcode_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_SAVE_RECORD_BARCODE);  // 能在读者尚有外借信息的情况下强行修改证条码号
        }

        // 强制保存所有内容，包括 borrows 元素。一定要小心使用本功能
        private void ToolStripMenuItem_saveForce_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_SAVE_RECORD_FORCE);
        }

        private async void toolStripSplitButton_registerFingerprint_ButtonClick(object sender, EventArgs e)
        {
            await registerFingerprint(false);
        }

        private async void ToolStripMenuItem_fingerprintPracticeMode_Click(object sender, EventArgs e)
        {
            await registerFingerprint(true);
        }

        async Task registerFingerprint(bool bPractice)
        {
            string strError = "";

            this.ShowMessage("等待扫描指纹 ...");
            this.EnableControls(false);
            // Program.MainForm.StatusBarMessage = "等待扫描指纹...";
            try
            {
                NormalResult getstate_result = await FingerprintGetState("");
                if (getstate_result.Value == -1)
                {
                    strError = $"指纹中心当前状态不正确：{getstate_result.ErrorInfo}";
                    goto ERROR1;
                }

                getstate_result = await FingerprintGetState("getLibraryServerUID");
                if (getstate_result.Value == -1)
                {
                    strError = getstate_result.ErrorInfo;
                    goto ERROR1;
                }
                else if (getstate_result.ErrorCode != null &&
                    getstate_result.ErrorCode != Program.MainForm.ServerUID)
                {
                    strError = $"指纹中心所连接的 dp2library 服务器 UID {getstate_result.ErrorCode} 和内务当前所连接的 UID {Program.MainForm.ServerUID} 不同。无法进行指纹登记";
                    goto ERROR1;
                }

                // TODO: 练习模式需要判断版本 2.2 以上

                REDO:
                GetFingerprintStringResult result = await ReadFingerprintString(
                    bPractice == true ? "!practice" : this.readerEditControl1.Barcode);
                if (result.Value == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
result.ErrorInfo + "\r\n\r\n是否重试?",
"ReaderInfoForm",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO;
                }

                if (result.Value == -1 || result.Value == 0)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }

#if NO
                strFingerprint = "12345";   // test
                strVersion = "test-version";
#endif

                if (bPractice == false)
                {
                    this.readerEditControl1.FingerprintFeature = result.Fingerprint;   // strFingerprint;
                    this.readerEditControl1.FingerprintFeatureVersion = result.Version;    // strVersion;
                    this.readerEditControl1.Changed = true;
                }
            }
            finally
            {
                this.EnableControls(true);
                this.ClearMessage();
            }

            // MessageBox.Show(this, strFingerprint);
            Program.MainForm.StatusBarMessage = "指纹信息获取成功";
            return;
            ERROR1:
            Program.MainForm.StatusBarMessage = strError;
            this.ShowMessage(strError, "red", true);
            // ShowMessageBox(strError);
        }

    }
}