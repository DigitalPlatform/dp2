using System;
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
using System.Threading;
using System.Linq;

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
using Microsoft.CodeAnalysis.Operations;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Script;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
// using dp2Circulation.Reader;

namespace dp2Circulation
{
    /// <summary>
    /// 读者信息管理窗口
    /// </summary>
    public partial class ReaderInfoForm : MyForm
    {
        // 和 form_closed 关联
        // CancellationTokenSource _cancel = new CancellationTokenSource();

        int m_nChannelInUse = 0; // >0表示通道正在被使用

        Commander commander = null;

        const int WM_NEXT_RECORD = API.WM_USER + 200;   // 当前读者记录向后翻页(同读者库内下一个 ID 的读者记录)
        const int WM_PREV_RECORD = API.WM_USER + 201;
        const int WM_LOAD_RECORD = API.WM_USER + 202;
        const int WM_DELETE_RECORD = API.WM_USER + 203;
        const int WM_FORCE_DELETE_RECORD = API.WM_USER + 204;
        const int WM_HIRE = API.WM_USER + 205;
        const int WM_SAVETO = API.WM_USER + 206;
        const int WM_SAVE_RECORD = API.WM_USER + 207;
        const int WM_SAVE_RECORD_BARCODE = API.WM_USER + 208;

        // 2020/5/26
        const int WM_SAVE_RECORD_STATE = API.WM_USER + 210;

        const int WM_SAVE_RECORD_FORCE = API.WM_USER + 220;
        const int WM_FOREGIFT = API.WM_USER + 221;
        const int WM_RETURN_FOREGIFT = API.WM_USER + 222;
        const int WM_SET_FOCUS = API.WM_USER + 223;

        const int WM_NEXT_RECORD_RESULTSET = API.WM_USER + 230; // 在命中结果集中向后翻页
        const int WM_PREV_RECORD_RESULTSET = API.WM_USER + 231;

        WebExternalHost m_webExternalHost = new WebExternalHost();

        WebExternalHost m_chargingInterface = new WebExternalHost();

        string m_strSetAction = "new";  // new / change 之一

        SelectedTemplateCollection selected_templates = new SelectedTemplateCollection();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderInfoForm()
        {
            this.UseLooping = true;

            InitializeComponent();
        }

        /// <summary>
        /// 当前读者证条码号
        /// </summary>
        public string ReaderBarcode
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.toolStripTextBox_barcode.Text;  //  this.textBox_readerBarcode.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.toolStripTextBox_barcode.Text = value; //  this.textBox_readerBarcode.Text = value;
                });
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

            // testing
            // this.readerEditControl1.SetEditable("name", "name");

            this.readerEditControl1.GetValueTable += new GetValueTableEventHandler(readerEditControl1_GetValueTable);
            this.readerEditControl1.VerifyContent += ReaderEditControl1_VerifyContent;
            this.readerEditControl1.Initializing = false;   // 如果没有此句，一开始在空模板上修改就不会变色

            //
            this.binaryResControl1.ContentChanged -= new ContentChangedEventHandler(binaryResControl1_ContentChanged);
            this.binaryResControl1.ContentChanged += new ContentChangedEventHandler(binaryResControl1_ContentChanged);

            this.binaryResControl1.GetChannel -= binaryResControl1_GetChannel;
            this.binaryResControl1.GetChannel += binaryResControl1_GetChannel;

            this.binaryResControl1.ReturnChannel -= binaryResControl1_ReturnChannel;
            this.binaryResControl1.ReturnChannel += binaryResControl1_ReturnChannel;

            // this.binaryResControl1.Channel = this.Channel;
            // this.binaryResControl1.Stop = null; // this._stop;

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

#if NEWFINGER
            if (string.IsNullOrEmpty(Program.MainForm.PalmprintReaderUrl))
            {
                this.toolStripSplitButton_registerPalmprint.Visible = false;
                this.toolStripSplitButton_registerFingerprint.Visible = false;
            }
            else
            {
                if (Program.MainForm.IsFingerprint())
                {
                    this.toolStripSplitButton_registerPalmprint.Visible = false;
                    this.toolStripSplitButton_registerFingerprint.Visible = true;
                }
                else
                {
                    this.toolStripSplitButton_registerPalmprint.Visible = true;
                    this.toolStripSplitButton_registerFingerprint.Visible = false;
                }
            }
#endif

            API.PostMessage(this.Handle, WM_SET_FOCUS, 0, 0);

#if SUPPORT_OLD_STOP
            this.Channel = null;    // testing 2022/6/14
#endif
        }

        private void ReaderEditControl1_VerifyContent(object sender, VerifyEditEventArgs e)
        {
            if (e.EditName == "email")
            {
                var errors = PropertyTableDialog.VerifyString(
    "email,weixinid",
    e.Text);
                if (errors.Count > 0)
                    e.ErrorInfo = StringUtil.MakePathList(errors, "; ");
            }
        }

        void binaryResControl1_ReturnChannel(object sender, ReturnChannelEventArgs e)
        {
            OnReturnChannel(sender, e);
        }

        void binaryResControl1_GetChannel(object sender, GetChannelEventArgs e)
        {
            OnGetChannel(sender, e);
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
                {
                    this.readerEditControl1.Changed = value;
                    if (value == false)
                        ClearImportantFields();
                }
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
#if !NEWFINGER
                // TODO: 增加一个参数，表示不需要显示“已取消操作”提示对话框
                var task = CancelReadFingerprintString();
#endif
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
                    }
                }));
                if (e.Cancel == true)   // 2022/6/10
                    return;
            }

        }

        private void ReaderInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            // 2022/6/10
            _cancel.Cancel();
            */

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
            this.readerEditControl1.VerifyContent -= ReaderEditControl1_VerifyContent;

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

        // (为了兼容以前的 public API。即将弃用。线程模型不理想)
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
            bool bForceLoad = false)
        {
            var task = LoadRecordAsync(
strBarcode,
bForceLoad);
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
            }
            return task.Result;
        }

        public Task<int> LoadRecordAsync(string strBarcode,
            bool bForceLoad = false)
        {
            return Task.Factory.StartNew(() =>
            {
                return _loadRecord(strBarcode, bForceLoad);
            },
this.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        int _loadRecord(string strBarcode,
            bool bForceLoad = false)
        {
            string strError = "";
            int nRet = 0;

            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = this.TryGet(() =>
                {
                    return MessageBox.Show(this,
"当前有信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要根据证条码号重新装载内容? ",
"ReaderInfoForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                });
                if (result != DialogResult.Yes)
                    return 0;   // cancelled
            }

            using (var looping = Looping(
                out LibraryChannel channel,
                "正在装载读者记录 ...",
                "disableControl"))
            {

                this.readerEditControl1.Clear();
                this.binaryResControl1.Clear();

                ClearReaderHtmlPage();

                this.ClearBorrowHistoryPage();
                this.ClearQrCodePage();

                byte[] baTimestamp = null;
                string strOutputRecPath = "";
                int nRedoCount = 0;

            REDO:
                looping.Progress.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                List<string> formats = new List<string>() { "xml", "html" };
                if (Control.ModifierKeys == Keys.Control)
                    formats = new List<string>() { "xml" };
                // 2021/7/21
                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.61") >= 0)
                    formats.Add("structure");

                long lRet = channel.GetReaderInfo(
                    looping.Progress,
                    strBarcode,
                    StringUtil.MakePathList(formats),
                    out string[] results,
                    out strOutputRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (lRet == 0)
                    goto ERROR1;

                if (lRet > 1)
                {
                    // 如果重试后依然发生重复
                    if (nRedoCount > 0)
                    {
                        if (bForceLoad == true)
                        {
                            strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，但仍装入其中第一条读者记录。\r\n\r\n这是一个严重错误，请系统管理员尽快排除。";
                            this.MessageBoxShow(strError);    // 警告后继续装入第一条 
                        }
                        else
                        {
                            strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，放弃装入读者记录。\r\n\r\n注意这是一个严重错误，请系统管理员尽快排除。";
                            goto ERROR1;    // 当出错处理
                        }
                    }

                    // -1   error
                    // 1    redo
                    // 0    return
                    var ret = this.TryGet(() =>
                    {
                        SelectPatronDialog dlg = new SelectPatronDialog();

                        dlg.Overflow = StringUtil.SplitList(strOutputRecPath).Count < lRet;
                        nRet = dlg.Initial(
                            // Program.MainForm,
                            StringUtil.SplitList(strOutputRecPath),
                            "请选择一个读者记录",
                            out strError);
                        if (nRet == -1)
                            return -1;  // goto ERROR1;

                        Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_SelectPatronDialog_state");
                        dlg.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(dlg);

                        if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            return 0;

                        strBarcode = "@path:" + dlg.SelectedRecPath;   // 2015/11/16 // .SelectedBarcode;
                        nRedoCount++;
                        return 1;   // goto REDO;
                    });
                    if (ret == 0)
                        return 0;
                    if (ret == 1)
                        goto REDO;
                    return -1;
                }

                this.ReaderBarcode = strBarcode;

                if (results == null || results.Length < 1)
                {
                    strError = "返回的 results 不正常。";
                    goto ERROR1;
                }

                string strXml = "";
                string strHtml = "";
                strXml = GetValue(formats, results, "xml");
                strHtml = GetValue(formats, results, "html");
                string strStructure = GetValue(formats, results, "structure");

                nRet = this.readerEditControl1.SetData(
                    strXml,
                    strOutputRecPath,
                    baTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (string.IsNullOrEmpty(strStructure) == false)
                {
                    ParseStructure(strStructure,
    out string visibleFields,
    out string writeableFields);

                    this.readerEditControl1.SetEditable(visibleFields, writeableFields);
                }

                // 接着装入对象资源
                {
                    nRet = this.binaryResControl1.LoadObject(
                        looping.Progress,
                        channel,
                        strOutputRecPath,    // 2008/11/2 changed
                        strXml,
                        Program.MainForm.ServerVersion,
                        out strError);
                    if (nRet == -1)
                    {
                        this.MessageBoxShow(strError);
                        // return -1;
                    }
                }
                Global.SetXmlToWebbrowser(this.webBrowser_xml,
                    Program.MainForm.DataDir,
                    "xml",
                    strXml);

                this.m_strSetAction = "change";

                this.SetReaderHtmlString(strHtml);
            }

            this.TryInvoke(() =>
            {
                tabControl_readerInfo_SelectedIndexChanged(this, new EventArgs());
            });
            return 1;
        ERROR1:
            this.MessageBoxShow(strError);
            return -1;
        }

        // 根据格式名字找到值
        static string GetValue(List<string> formats,
            string[] results,
            string format)
        {
            if (results == null || results.Length == 0)
                return null;
            int index = formats.IndexOf(format);
            if (index == -1)
                return null;    // not found
            Debug.Assert(index >= 0);
            if (index >= results.Length)
                return null;
            return results[index];
        }

        void ParseStructure(string xml,
            out string visibleFields,
            out string writeableFields)
        {
            visibleFields = "";
            writeableFields = "";
            if (string.IsNullOrEmpty(xml))
                return;
            /*
    * <structure 
    * visibleFields="name,namePinyin,state,?comment"
    * writeableFields="name,namePinyin" />
    * 
    * */
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            visibleFields = dom.DocumentElement?.GetAttribute("visibleFields");
            writeableFields = dom.DocumentElement?.GetAttribute("writeableFields");
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
            this.m_webExternalHost.SetHtmlString(strHtml,
                "readerinfoform_reader");
        }

        // (为了兼容以前的 public API。即将弃用。线程模型不理想)
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
            var task = LoadRecordByRecPathAsync(
strRecPath,
strPrevNextStyle);
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
            }
            return task.Result;
        }

        public Task<int> LoadRecordByRecPathAsync(string strRecPath,
            string strPrevNextStyle = "")
        {
            return Task.Factory.StartNew(() =>
            {
                return _loadRecordByRecPath(strRecPath, strPrevNextStyle);
            },
this.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        int _loadRecordByRecPath(string strRecPath,
            string strPrevNextStyle = "")
        {
            string strError = "";

            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = this.TryGet(() =>
                {
                    return MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要根据记录路径重新装载内容? ",
    "ReaderInfoForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                });
                if (result != DialogResult.Yes)
                    return 0;   // cancelled
            }

            bool bPrevNext = false;

            if (String.IsNullOrEmpty(strPrevNextStyle) == false)
            {
                strRecPath += "$" + strPrevNextStyle.ToLower();
                bPrevNext = true;
            }

            var looping = Looping(out LibraryChannel channel,
                $"正在装入读者记录 {strRecPath} {strPrevNextStyle}...",
                "disableControl");

            if (bPrevNext == false)
            {
                ClearReaderHtmlPage();

                this.readerEditControl1.Clear();
                this.binaryResControl1.Clear();
            }

            this.ClearBorrowHistoryPage();
            this.ClearQrCodePage();
            try
            {
                // looping.Progress.SetMessage("正在装入读者记录 " + strRecPath + " ...");

                List<string> formats = new List<string>() { "xml", "html" };
                // 2021/7/21
                if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.61") >= 0)
                    formats.Add("structure");
                long lRet = channel.GetReaderInfo(
                    looping.Progress,
                    "@path:" + strRecPath,
                    StringUtil.MakePathList(formats),   // "xml,html",
                    out string[] results,
                    out string strOutputRecPath,
                    out byte[] baTimestamp,
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

                if (results == null || results.Length < 1)
                {
                    strError = "返回的results不正常。";
                    goto ERROR1;
                }

                string strXml = "";
                string strHtml = "";
                strXml = GetValue(formats, results, "xml");
                strHtml = GetValue(formats, results, "html");
                string strStructure = GetValue(formats, results, "structure");

                int nRet = this.readerEditControl1.SetData(
                    strXml,
                    strOutputRecPath,   // strRecPath,
                    baTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (string.IsNullOrEmpty(strStructure) == false)
                {
                    ParseStructure(strStructure,
    out string visibleFields,
    out string writeableFields);

                    this.readerEditControl1.SetEditable(visibleFields, writeableFields);
                }

                // 接着装入对象资源
                {
                    this.binaryResControl1.Clear();
                    nRet = this.binaryResControl1.LoadObject(
                        looping.Progress,
                        channel,
                        strOutputRecPath,    // 2008/11/2 changed
                        strXml,
                        Program.MainForm.ServerVersion,
                        out strError);
                    if (nRet == -1)
                    {
                        this.MessageBoxShow(strError);
                        // return -1;
                    }
                }

                this.ReaderBarcode = this.readerEditControl1.Barcode;

                Global.SetXmlToWebbrowser(this.webBrowser_xml,
Program.MainForm.DataDir,
"xml",
strXml);

                this.m_strSetAction = "change";

                this.SetReaderHtmlString(strHtml);
            }
            finally
            {
                looping.Dispose();
            }

            this.TryInvoke(() =>
            {
                tabControl_readerInfo_SelectedIndexChanged(this, new EventArgs());
            });
            return 1;
        ERROR1:
            this.MessageBoxShow(strError);
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

        void EnableBioSendKey(bool bEnable)
        {
            /*
            if (string.IsNullOrEmpty(FingerprintManager.Url) == false)
            {
                var result = FingerprintManager.GetState(bEnable ? "continueCapture" : "disableCatpure");
                if (result.Value == -1)
                    Program.MainForm?.OperHistory?.AppendHtml($"<div class='debug error'>{ HttpUtility.HtmlEncode($"禁用或者启动掌纹捕捉时出错: {result.ErrorInfo}") }</div>");
                else
                {
                }
            }
            */
            if (bEnable)
                MainForm.EnablePalmSendKey();
            else
                MainForm.DisablePalmSendKey();
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
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */

            SetMenuItemState();

            if (this.DisableIdcardReaderSendkey == true)
            {
                EnableSendKey(false);
            }

            if (this.DisableBioSendkey)
                EnableBioSendKey(false);

            // Debug.WriteLine("Activated");
        }

        private void ReaderInfoForm_Deactivate(object sender, EventArgs e)
        {
            if (this.DisableIdcardReaderSendkey == true)
            {
                EnableSendKey(true);
            }

            if (this.DisableBioSendkey)
                EnableBioSendKey(true);

            // Debug.WriteLine("DeActivated");
        }

        public override void UpdateEnable(bool bEnable)
        {
            // this.textBox_readerBarcode.Enabled = bEnable;
            // this.button_load.Enabled = bEnable;
            this.toolStrip_load.Enabled = bEnable;

            this.readerEditControl1.Enabled = bEnable;

            if (bEnable == false)
                this.toolStripSplitButton_delete.Enabled = bEnable;
            else
            {
                if (string.IsNullOrEmpty(this.readerEditControl1.RecPath) == false)
                    this.toolStripSplitButton_delete.Enabled = true;  // 只有具备明确的路径的记录，才能被删除
                else
                    this.toolStripSplitButton_delete.Enabled = false;
            }

            EnableToolStripExclude(bEnable,
                new ToolStripItem[] { this.toolStripButton_stopSummaryLoop, this.toolStripSplitButton_delete });
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
                this.toolStripTextBox_barcode.Focus();
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

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存配置文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在保存配置文件 ...");

            try
            {
                looping.Progress.SetMessage("正在保存配置文件 " + strCfgFilePath + " ...");

                byte[] output_timestamp = null;
                string strOutputPath = "";

                long lRet = channel.WriteRes(
                    looping.Progress,
                    strCfgFilePath,
                    strContent,
                    true,
                    "", // style
                    baTimestamp,
                    out output_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);

                this.ReturnChannel(channel);
            }

            return 1;
        ERROR1:
            return -1;
        }

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
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                int nRet = this.GetCfgFileContent(strReaderDbName + "/cfgs/template",
                    out string strContent,
                    out byte[] baTimestamp,
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
                tempdlg.CheckNameExist = false; // 按OK按钮时不警告"名字不存在",这样允许新建一个模板
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

                if (tempdlg.Changed == false)   // 没有必要保存回去
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
                ClearImportantFields();

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

                this.readerEditControl1.RecPath = dbname_dlg.DbName + "/?"; // 为了追加保存
                this.readerEditControl1.Changed = false;
                ClearImportantFields();

                // 下载配置文件
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.GetCfgFileContent(strReaderDbName + "/cfgs/template",
                    out string strContent,
                    out byte[] baCfgOutputTimestamp,
                    out strError);
                if (nRet == 0)
                {
                    MessageBox.Show(this, strError + "\r\n\r\n将改用位于本地的 “选项/读者信息缺省值” 来刷新记录");

                    // 如果template文件不存在，则找本地配置的模板
                    string strNewDefault = Program.MainForm.AppInfo.GetString(
    "readerinfoform_optiondlg",
    "newreader_default",
    "<root />");
                    /*
                    nRet = this.readerEditControl1.SetData(strNewDefault,
                         "",
                         null,
                         out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                    */
                    // 2021/7/23
                    nRet = LoadStructure(
                        strNewDefault,
                        "",
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

                /*
                nRet = this.readerEditControl1.SetData(
        select_temp_dlg.SelectedRecordXml,
        dbname_dlg.DbName + "/?",
        null,
        out strError);
                if (nRet == -1)
                    goto ERROR1;
                */
                // 2021/7/23
                nRet = LoadStructure(
                    select_temp_dlg.SelectedRecordXml,
                    dbname_dlg.DbName + "/?",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;


                Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "xml",
    select_temp_dlg.SelectedRecordXml);

                this.m_strSetAction = "new";

#if NO
                Global.ClearHtmlPage(this.webBrowser_readerInfo,
                    Program.MainForm.DataDir);
#endif
                ClearReaderHtmlPage();

                this.readerEditControl1.Changed = false;
                ClearImportantFields();
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

        // 2021/7/23
        int LoadStructure(string strDefaultXml,
            string strRecPath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 旧版本
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.61") < 0)
            {
                nRet = this.readerEditControl1.SetData(strDefaultXml,
     strRecPath,
     null,
     out strError);
                if (nRet == -1)
                    return -1;
                return 0;
            }

            List<string> formats = new List<string>() { "xml" };
            // if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.61") >= 0)
            formats.Add("structure");

            LibraryChannel channel = this.GetChannel();

            try
            {
                long lRet = channel.GetReaderInfo(
                    null,   // _stop,
                    string.IsNullOrEmpty(strDefaultXml) ? "<root />" : strDefaultXml,
                    StringUtil.MakePathList(formats),
                    out string[] results,
                    out string _,
                    out byte[] _,
                    out strError);
                if (lRet == -1)
                    return -1;

                string strXml = "";
                strXml = GetValue(formats, results, "xml");
                string strStructure = GetValue(formats, results, "structure");

                nRet = this.readerEditControl1.SetData(
                    strXml,
                    strRecPath,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (string.IsNullOrEmpty(strStructure) == false)
                {
                    ParseStructure(strStructure,
    out string visibleFields,
    out string writeableFields);

                    this.readerEditControl1.SetEditable(visibleFields, writeableFields);
                }

                return 0;
            }
            finally
            {
                this.ReturnChannel(channel);
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
                /*
                int nRet = this.readerEditControl1.SetData(strNewDefault,
                     "",
                     null,
                     out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return -1;
                }
                */
                // 2021/7/23
                int nRet = LoadStructure(
                    strNewDefault,
                    "",
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
                ClearImportantFields();
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
            this.Invoke((Action)(() =>
            {
                toolStripTextBox_barcode.Enabled = bEnable;
                this.toolStrip1.Enabled = bEnable;
            }));
        }

        // 除了 stop 按钮外，Enable/Disable 其他所有按钮
        void EnableToolStripExclude(bool bEnable,
            ToolStripItem[] excludes)
        {
            List<ToolStripItem> all = new List<ToolStripItem>(this.toolStrip1.Items.Cast<ToolStripItem>());
            foreach (var item in excludes)
            {
                all.Remove(item);
            }

            foreach (var item in all)
            {
                item.Enabled = bEnable;
            }
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

        // 为了兼容以前脚本里面的调用。但注意发现这个函数容易引起死锁！
        public int SaveRecord(string strStyle = "displaysuccess,verifybarcode")
        {
            var task = SaveRecordAsync(strStyle);
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
            }
            return task.Result;
        }

        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 保存记录
        /// </summary>
        /// <param name="strStyle">风格。为 displaysuccess/verifybarcode/changereaderbarcode/changestate/changereaderforce 之一或者组合。缺省值为 displaysuccess,verifybarcode</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public async Task<int> SaveRecordAsync(string strStyle = "displaysuccess,verifybarcode")
        {
            string strError = "";
            int nRet = 0;

            try
            {
                /*
                bool bControlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;

                if (bControlPressed == false
                    && string.IsNullOrEmpty(this.readerEditControl1.Barcode) == true)
                {
                    // 注: 这样做主要是对证条码号为空的情况制造一点操作麻烦，避免工作人员出现误操作
                    strError = "尚未输入证条码号。\r\n\r\n可改为按住 Ctrl 键使用本命令，按照读者记录路径进行保存";
                    goto ERROR1;
                }
                */

                // 是否强制修改册条码号
                bool bChangeReaderBarcode = StringUtil.IsInList("changereaderbarcode", strStyle);
                bool bChangeState = StringUtil.IsInList("changestate", strStyle);
                bool bChangeReaderForce = StringUtil.IsInList("changereaderforce", strStyle);
                if (bChangeReaderBarcode && bChangeReaderForce)
                {
                    strError = "style 不应同时包含 changereaderbarcode 和 changerecordforce";
                    goto ERROR1;
                }

                if (bChangeReaderForce)
                {
                    var result = this.TryGet(() =>
                    {
                        return MessageBox.Show(this,
                        "您确实要强制修改当前读者记录？\r\n\r\n警告：当读者有在借信息的情况下，强制修改保存功能，*** 不会自动修改 *** 相关在借册记录，会造成借阅信息关联错误。若只是想在修改证条码号以后保存记录，请改用“保存(强制修改证条码号)功能”",
                        "谨慎使用“保存(强制修改)”功能",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    });
                    if (result == DialogResult.No)
                        return 0;
                }

                // TODO: 保存时候的选项

                // 当 this.readerEditControl1.RecPath 为空的时候，需要出现对话框，让用户可以选择目标库
                string strTargetRecPath = this.readerEditControl1.RecPath;
                if (string.IsNullOrEmpty(this.readerEditControl1.RecPath) == true)
                {
                    var ret = this.TryGet(() =>
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
                        return 1;
                    });
                    if (ret == 0)
                        return 0;
                }

                bool bVerifyBarcode = StringUtil.IsInList("verifybarcode", strStyle);

                // 校验证条码号
                if ((this.NeedVerifyBarcode == true || bVerifyBarcode)
                    && StringUtil.IsIdcardNumber(this.readerEditControl1.Barcode) == false
                    && bChangeReaderForce == false/* 2020/10/31 */)
                {
                    nRet = DoVerifyPatronBarcode(strTargetRecPath,
                        bVerifyBarcode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#if REMOVED
                    var library_code = Program.MainForm.GetReaderDbLibraryCode(Global.GetDbName(strTargetRecPath));

                    // 形式校验条码号
                    // return:
                    //      -2  服务器没有配置校验方法，无法校验
                    //      -1  error
                    //      0   不是合法的条码号
                    //      1   是合法的读者证条码号
                    //      2   是合法的册条码号
                    nRet = VerifyBarcode(
                        library_code,
                        // Program.MainForm.FocusLibraryCode, // 是否可以根据读者库的馆代码？或者现在已经有了服务器校验功能，这里已经没有必要校验了? // this.Channel.LibraryCodeList,
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
                        MessageBoxShow("警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
                    }
#endif
                }


                var looping = Looping(out LibraryChannel channel,
                    "正在保存读者记录 " + this.readerEditControl1.Barcode + " ...",
                    "disableControl");
                try
                {
                    string strNewXml = "";
                    this.Invoke((Action)(() =>
                    {
                        nRet = GetReaderXml(
                true,
                false,
                out strNewXml,
                out strError);
                    }));
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
                        if (bChangeState)
                        {
                            strError = "changestate 和 changereaderbarcode 不应同时具备";
                            goto ERROR1;
                        }
                        if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.51") < 0)
                        {
                            strError = "需要 dp2library 版本在 2.51 以上才能实现强制修改册条码号的功能。当前 dp2library 版本为 " + Program.MainForm.ServerVersion;
                            goto ERROR1;
                        }
                        strAction = "changereaderbarcode";
                    }
                    else if (strAction == "change" && bChangeState) // 2020/5/28
                    {
                        strAction = "changestate";
                    }

                    if (strAction == "change" && bChangeReaderForce)
                    {
                        strAction = "forcechange";
                    }

                    // 调试
                    // MessageBoxShow("1 this.m_strSetAction='"+this.m_strSetAction+"'");

                    long lRet = channel.SetReaderInfo(
                        looping.Progress,
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
                            var ret = this.TryGet(() =>
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
                                        this.MessageBoxShow(strError);
                                    }
                                    this.MessageBoxShow("请注意重新保存记录");
                                    return -1;
                                }
                                return 0;
                            });
                            if (ret == -1)
                                return -1;
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
                        this.MessageBoxShow(strError);

                        if (channel.ErrorCode == ErrorCode.PartialDenied)
                        {
                            // 提醒重新装载?
                            this.MessageBoxShow("请重新装载记录, 检查哪些字段内容修改被拒绝。");
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
                            looping.Progress,
                            channel,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            this.MessageBoxShow(strError);
                        }
                        if (nRet >= 1)
                        {
                            // 重新获得时间戳
                            lRet = channel.GetReaderInfo(
                                looping.Progress,
                                "@path:" + strSavedPath,
                                "", // "xml,html",
                                out string[] results,
                                out string strOutputPath,
                                out baNewTimestamp,
                                out strError);
                            if (lRet == -1 || lRet == 0)
                            {
                                this.MessageBoxShow(strError);
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
                        Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "xml",
    strSavedXml);
                        // 2007/11/12
                        this.m_strSetAction = "change";

                        // 装载记录到HTML
                        {

                            // string strBarcode = this.readerEditControl1.Barcode;

                            looping.Progress.SetMessage($"正在装入读者记录 {strSavedPath} 的 HTML ...");

                            int nRedoCount = 0;
                        REDO_LOAD_HTML:
                            string[] results = null;
                            lRet = channel.GetReaderInfo(
                                looping.Progress,
                                // strBarcode,
                                "@path:" + strSavedPath,
                                "html",
                                out results,
                                out string strOutputRecPath,
                                out byte[] baTimestamp,
                                out strError);
                            if (lRet == -1)
                            {
                                // 以读者身份保存自己的读者记录后，dp2library 为了清除以前缓存的登录信息，强制释放了通道。所以这里需要能重试操作
                                if (channel.ErrorCode == ErrorCode.ChannelReleased
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
                                // strError = "条码 " + strBarcode + " 命中记录 " + lRet.ToString() + " 条，注意这是一个严重错误，请系统管理员尽快排除。";
                                strError = $"路径 '{strSavedPath}' 命中记录 {lRet.ToString()} 条，注意这是一个严重错误，请系统管理员尽快排除。";
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
                        var result = await FaceNotifyTask("faceChanged");
                        if (result.Value == -1)
                            warnings.Add(result.ErrorInfo);
                    }

#if !NEWFINGER
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

#endif

                    if (warnings.Count > 0)
                    {
                        string warning = $"虽然读者记录已经保存成功，但通知人脸中心和指纹中心刷新时发生了错误: {StringUtil.MakePathList(warnings, "; ")}";
                        this.ShowMessage(warning, "yellow", true);
                    }

                    if (StringUtil.IsInList("displaysuccess", strStyle) == true)
                        Program.MainForm.StatusBarMessage = "读者记录保存成功";
                    return 1;
                }
                finally
                {
                    looping.Dispose();
                }

            }
            catch (Exception ex)
            {
                MainForm.WriteErrorLog($"SaveRecordAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                strError = $"SaveRecordAsync() 出现异常: {ex.Message}。详情已写入错误日志";
                goto ERROR1;
            }

        ERROR1:
            this.MessageBoxShow(strError);
            Program.MainForm.StatusBarMessage = strError;
            return -1;
        }

        int DoVerifyPatronBarcode(string strTargetRecPath,
            bool bVerifyBarcode,
            out string strError)
        {
            strError = "";

            var library_code = Program.MainForm.GetReaderDbLibraryCode(Global.GetDbName(strTargetRecPath));
            if (library_code == null)
                library_code = Program.MainForm.FocusLibraryCode;

            // 形式校验条码号
            // return:
            //      -2  服务器没有配置校验方法，无法校验
            //      -1  error
            //      0   不是合法的条码号
            //      1   是合法的读者证条码号
            //      2   是合法的册条码号
            int nRet = VerifyBarcode(
                library_code,
                // Program.MainForm.FocusLibraryCode, // 是否可以根据读者库的馆代码？或者现在已经有了服务器校验功能，这里已经没有必要校验了? // this.Channel.LibraryCodeList,
                this.readerEditControl1.Barcode,
                out strError);
            if (nRet == -1)
                return -1;

            // 输入的条码格式不合法
            if (nRet == 0)
            {
                strError = "您输入的证条码 " + this.readerEditControl1.Barcode + " 格式不正确(" + strError + ")。";
                return -1;
            }

            // 实际输入的是册条码号
            if (nRet == 2)
            {
                strError = "您输入的条码号 " + this.readerEditControl1.Barcode + " 是册条码号。请输入读者证条码号。";
                return -1;
            }

            // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
            if (nRet == -2
                && (this.NeedVerifyBarcode == true && bVerifyBarcode == false))
            {
                this.MessageBoxShow("警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
            }

            return 0;
        }

        /*
        void MessageBoxShow(string text)
        {
            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, text);
            }));
        }
        */

        // 另存
        private void toolStripButton_saveTo_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_SAVETO);
        }

        static string GetOuterXml(XmlDocument domTarget,
    string element_name)
        {
            XmlNodeList nodes = null;
            if (element_name.Contains(":"))
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);
                element_name = element_name.Replace(DpNs.dprms, "dprms");
                nodes = domTarget.DocumentElement.SelectNodes("//" + element_name, nsmgr);   // "//dprms:file"
            }
            else
                nodes = domTarget.DocumentElement.SelectNodes(element_name);

            if (nodes.Count == 0)
                return null;

            List<string> oldOuterXmls = new List<string>();
            foreach (XmlElement element in nodes)
            {
                oldOuterXmls.Add(element.OuterXml);
            }

            /*
            // TODO: 是否要排序?
            if (oldOuterXmls.Count > 0)
                oldOuterXmls.Sort();
            */

            return StringUtil.MakePathList(oldOuterXmls, "\r\n");
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

            // 2021/7/26
            // 保留此时的 dprms:file 元素值，这是上次 SetData() 时候的值，代表从服务器获取过来时候的值
            string old_files = GetOuterXml(dom, "http://dp2003.com/dprms:file");

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
                nRet = this.binaryResControl1.AddFileFragments(dom,
            out strError);
                if (nRet == -1)
                    return -1;
            }

            // 如果 dprms:file 元素相比从服务器获取来的原始记录发生了变化，则需要在后继提交的读者 XML 记录根元素 importantFields 属性中标注它为重要元素
            string new_files = GetOuterXml(dom, "http://dp2003.com/dprms:file");
            if (old_files != new_files)
                AddImportantField("http://dp2003.com/dprms:file");

            // 如果没有 refID 元素，需要给添加一个
            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID) == true)
                DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());

            // 2021/7/22
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.65") >= 0
                && _importantFields.Count > 0)
                dom.DocumentElement.SetAttribute("importantFields", StringUtil.MakePathList(_importantFields));

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

        public Task SaveToAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                _saveTo();
            },
this.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        void _saveTo()
        {
            string strError = "";
            int nRet = 0;
            bool bReserveFieldsCleared = false;

            if (this.readerEditControl1.Barcode == "")
            {
                strError = "尚未输入证条码号";
                goto ERROR1;
            }

            // 出现对话框，让用户可以选择目标库
            ReaderSaveToDialog saveto_dlg = null;

            var dialog_result = this.TryGet(() =>
            {
                saveto_dlg = new ReaderSaveToDialog();
                MainForm.SetControlFont(saveto_dlg, this.Font, false);
                saveto_dlg.Text = "新增一条读者记录";
                saveto_dlg.MessageText = "请选择要保存的目标记录位置\r\n(记录ID为 ? 表示追加保存到数据库末尾)";
                // saveto_dlg.MainForm = Program.MainForm;
                saveto_dlg.RecPath = this.readerEditControl1.RecPath;
                saveto_dlg.RecID = "?";

                Program.MainForm.AppInfo.LinkFormState(saveto_dlg, "readerinfoform_savetodialog_state");
                saveto_dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(saveto_dlg);
                return saveto_dlg.DialogResult;
            });

            if (dialog_result == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 校验证条码号
            if (this.NeedVerifyBarcode == true
                && StringUtil.IsIdcardNumber(this.readerEditControl1.Barcode) == false)
            {
                nRet = DoVerifyPatronBarcode(saveto_dlg.RecPath,
    false,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
#if REMOVED
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
#endif
            }


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

            var looping = Looping(out LibraryChannel channel,
                "正在保存读者记录 " + this.readerEditControl1.Barcode + " ...",
                "disableControl");
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

                long lRet = channel.SetReaderInfo(
                    looping.Progress,
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
                        CompareReaderForm dlg = null;

                        var dialog_result1 = this.TryGet(() =>
                        {
                            dlg = new CompareReaderForm();
                            dlg.Initial(
                                //Program.MainForm,
                                this.readerEditControl1.RecPath,
                                strExistingXml,
                                baNewTimestamp,
                                strNewXml,
                                this.readerEditControl1.Timestamp,
                                "数据库中的记录在编辑期间发生了改变。请仔细核对，并重新修改窗口中的未保存记录，按确定按钮后可重试保存。");

                            dlg.StartPosition = FormStartPosition.CenterScreen;
                            return dlg.ShowDialog(this);
                        });
                        if (dialog_result1 == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                this.MessageBoxShow( strError);
                            }
                            this.MessageBoxShow("请注意重新保存记录");
                            return;
                        }
                    }

                    goto ERROR1;
                }

                if (lRet == 1)
                {
                    // 部分字段被拒绝
                    this.MessageBoxShow(strError);

                    if (channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        // 提醒重新装载?
                        this.MessageBoxShow("请重新装载记录, 检查哪些字段内容修改被拒绝。");
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
                        looping.Progress,
                        channel,
                        Program.MainForm.ServerVersion,
                        out strError);
                    if (nRet == -1)
                    {
                        this.MessageBoxShow( strError);
                    }
                    if (nRet >= 1)
                    {
                        // 重新获得时间戳
                        lRet = channel.GetReaderInfo(
                            looping.Progress,
                            "@path:" + strSavedPath,
                            "", // "xml,html",
                            out string[] results,
                            out string strOutputPath,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                        {
                            this.MessageBoxShow( strError);
                        }
                    }

                    // 重新装载记录到编辑器
                    nRet = this.readerEditControl1.SetData(strSavedXml,
                        strSavedPath,
                        baNewTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "xml",
    strSavedXml);
                    // 2007/11/12
                    this.m_strSetAction = "change";

                    // 接着装入对象资源
                    {
                        nRet = this.binaryResControl1.LoadObject(
                            looping.Progress,
                            channel,
                            strSavedPath,    // 2008/11/2 changed
                            strSavedXml,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            this.MessageBoxShow(strError);
                            // return -1;
                        }
                    }

                    // 2011/11/23
                    // 装载记录到HTML
                    {
                        string strBarcode = this.readerEditControl1.Barcode;

                        looping.Progress.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                        string[] results = null;
                        lRet = channel.GetReaderInfo(
                            looping.Progress,
                            strBarcode,
                            "html",
                            out results,
                            out string strOutputRecPath,
                            out byte[] baTimestamp,
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
                looping.Dispose();
            }

            if (bReserveFieldsCleared == true)
                this.MessageBoxShow("另存成功。新记录的密码为初始状态，显示名尚未设置。");
            else
                this.MessageBoxShow("另存成功。");
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

#if REMOVED
        // 删除记录
        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_DELETE_RECORD);
        }
#endif

        public Task DeleteRecordAsync(string style = "")
        {
            return Task.Factory.StartNew(() =>
            {
                _deleteRecord(style);
            },
this.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        // 删除记录
        // parameters:
        //      style   force 表示强制删除。否则就是普通删除
        void _deleteRecord(string style = "")
        {
            string strError = "";

            bool bForceDelete = false;
            string strActionName = "删除";
            // 如果同时按下control键，表示强制按照记录路径删除
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control
                || StringUtil.IsInList("force", style))
            {
                bForceDelete = true;
                strActionName = "强制删除";
            }

            string strRecPath = null;
            string strText = $"确实要{strActionName}证条码号为 '" + this.readerEditControl1.Barcode + "' 的读者记录 ? ";

            if (string.IsNullOrEmpty(this.readerEditControl1.Barcode) == true)
            {
                //strError = "尚未输入证条码号，无法删除。\r\n\r\n可改为按住 Ctrl 键使用本命令，按照读者记录路径进行删除";
                //goto ERROR1;
                strRecPath = this.readerEditControl1.RecPath;
                strText = $"确实要{strActionName}证条码号为 '" + this.readerEditControl1.Barcode + "' 并且记录路径为 '" + strRecPath + "' 的读者记录 ? ";
            }

            if (bForceDelete)
                strText += "\r\n\r\n警告：当读者有在借信息的情况下，强制删除功能在删除读者记录时 *** 不会修改 *** 相关在借册记录，会造成借阅信息关联错误。正常情况下应该先将该读者的在借册全部执行还书，然后再删除读者记录。请慎重操作";

            DialogResult result = this.TryGet(() =>
            {
                return MessageBox.Show(this,
                strText,
                "ReaderInfoForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            });
            if (result != DialogResult.Yes)
                return;

            var looping = Looping(out LibraryChannel channel,
                "正在删除读者记录 " + this.readerEditControl1.Barcode + " ...",
                "disableControl");
            try
            {
                int nRet = this.readerEditControl1.GetData(
                    out string strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // string strOldBarcode = this.readerEditControl1.Barcode;

                ErrorCodeValue kernel_errorcode;

                byte[] baNewTimestamp = null;
                string strExistingXml = "";
                string strSavedXml = "";
                string strSavedPath = "";

                long lRet = channel.SetReaderInfo(
                    looping.Progress,
                    bForceDelete ? "forcedelete" : "delete",
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
                        CompareReaderForm dlg = null;
                        var dialog_result = this.TryGet(() =>
                        {
                            dlg = new CompareReaderForm();
                            dlg.Initial(
                                //Program.MainForm,
                                this.readerEditControl1.RecPath,
                                strExistingXml,
                                baNewTimestamp,
                                strNewXml,
                                this.readerEditControl1.Timestamp,
                                "数据库中的记录在编辑期间发生了改变。请仔细核对，若还想继续删除，按‘确定’按钮后可重试删除。如果不想删除了，请按‘取消’按钮");

                            dlg.StartPosition = FormStartPosition.CenterScreen;
                            return dlg.ShowDialog(this);
                        });
                        if (dialog_result == DialogResult.OK)
                        {
                            nRet = this.readerEditControl1.SetData(dlg.UnsavedXml,
                                dlg.RecPath,
                                dlg.UnsavedTimestamp,
                                out strError);
                            if (nRet == -1)
                            {
                                this.MessageBoxShow(strError);
                            }
                            this.MessageBoxShow("请注意读者记录此时***并未***删除。\r\n\r\n如要删除记录，请按‘删除’按钮重新提交删除请求。");
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
                    this.MessageBoxShow(strError);
                }

                this.readerEditControl1.Changed = false;
                ClearImportantFields();

#if !NEWFINGER
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
#endif
            }
            finally
            {
                looping.Dispose();
            }

            this.MessageBoxShow("删除成功。\r\n\r\n您会发现编辑窗口中还留着读者记录内容，但请不要担心，数据库里的读者记录已经被删除了。\r\n\r\n如果您这时后悔了，还可以按“保存按钮”把读者记录原样保存回去。");
            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        // 选项
        private void toolStripButton_option_Click(object sender, EventArgs e)
        {
            ReaderInfoFormOptionDlg dlg = new ReaderInfoFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            // dlg.MainForm = Program.MainForm;
            dlg.ShowDialog(this);
        }

        public Task HireAsync()
        {
            return Task.Factory.StartNew(async () =>
            {
                await _hireAsync();
            },
this.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        async Task _hireAsync()
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
                nRet = DoVerifyPatronBarcode(this.readerEditControl1.RecPath,
    false,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
#if REMOVED
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
#endif
            }

            var looping = Looping(out LibraryChannel channel,
                "正在创建读者记录 " + this.readerEditControl1.Barcode + " 的 租金交费请求 ...",
                "disableControl");
            try
            {
                string strReaderBarcode = this.readerEditControl1.Barcode;
                string strAction = "hire";

                long lRet = channel.Hire(
                    looping.Progress,
                    strAction,
                    strReaderBarcode,
                    out string strOutputrReaderXml,
                    out string strOutputID,
                    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }
            }
            finally
            {
                looping.Dispose();
            }

            // 重新装载窗口内容
            await LoadRecordAsync(this.readerEditControl1.Barcode,
                false);

            this.MessageBoxShow("创建租金交费请求 成功");
            return;
        ERROR1:
            this.MessageBoxShow(strError);
            return;
        }

        // 前一条读者记录
        private void toolStripButton_prev_Click(object sender, EventArgs e)
        {
            bool resultSet = !(Control.ModifierKeys == Keys.Control);

            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(resultSet ? WM_PREV_RECORD_RESULTSET : WM_PREV_RECORD);
        }

        // 后一条读者记录
        private void toolStripButton_next_Click(object sender, EventArgs e)
        {
            bool resultSet = !(Control.ModifierKeys == Keys.Control);

            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(resultSet ? WM_NEXT_RECORD_RESULTSET : WM_NEXT_RECORD);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            int msg = m.Msg;

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
                            _ = this.LoadRecordAsync(
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
                case WM_FORCE_DELETE_RECORD:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            _ = this.DeleteRecordAsync(msg == WM_FORCE_DELETE_RECORD ? "force" : "");
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_PREV_RECORD:
                case WM_NEXT_RECORD:
                case WM_PREV_RECORD_RESULTSET:
                case WM_NEXT_RECORD_RESULTSET:
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
                        string direction = (msg == WM_NEXT_RECORD || msg == WM_NEXT_RECORD_RESULTSET ? "next" : "prev");
                        bool resultSet = (msg == WM_PREV_RECORD_RESULTSET || msg == WM_NEXT_RECORD_RESULTSET);

                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            if (resultSet)
                            {
                                string strRecPath = GetPrevNextRecPath(direction);
                                if (string.IsNullOrEmpty(strRecPath))
                                {
                                    this.ShowMessage(direction == "next" ? "无法向后翻动" : "无法向前翻动", "yellow", true);
                                    return;
                                }
                                this.ClearMessage();
                                _ = LoadRecordByRecPathAsync(strRecPath, "");
                            }
                            else
                                _ = LoadRecordByRecPathAsync(this.readerEditControl1.RecPath, direction);
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
#if REMOVED
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
#endif
                case WM_HIRE:
                    EnableToolStrip(false);
                    try
                    {
                        if (this.m_webExternalHost.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            _ = this.HireAsync();
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
                            _ = this.ForegiftAsync("foregift");
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
                            _ = this.ForegiftAsync("return");
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
                            _ = this.SaveToAsync();
                        }
                    }
                    finally
                    {
                        EnableToolStrip(true);
                    }
                    return;
                case WM_SAVE_RECORD:
                    _ = Task.Run(async () =>
                    {
                        EnableToolStrip(false);
                        try
                        {
                            if (this.m_webExternalHost.CanCallNew(
                                this.commander,
                                msg) == true)
                            {
                                await this.SaveRecordAsync("displaysuccess");  // ,verifybarcode
                            }
                        }
                        finally
                        {
                            EnableToolStrip(true);
                        }
                    });
                    return;
                case WM_SAVE_RECORD_BARCODE:
                    _ = Task.Run(async () =>
                    {
                        EnableToolStrip(false);
                        try
                        {
                            if (this.m_webExternalHost.CanCallNew(
                                this.commander,
                                msg) == true)
                            {
                                await this.SaveRecordAsync("displaysuccess,changereaderbarcode");  // verifybarcode,
                            }
                        }
                        finally
                        {
                            EnableToolStrip(true);
                        }
                    });
                    return;
                case WM_SAVE_RECORD_STATE:
                    _ = Task.Run(async () =>
                    {
                        EnableToolStrip(false);
                        try
                        {
                            if (this.m_webExternalHost.CanCallNew(
                                this.commander,
                                msg) == true)
                            {
                                await this.SaveRecordAsync("displaysuccess,changestate");
                            }
                        }
                        finally
                        {
                            EnableToolStrip(true);
                        }
                    });
                    return;
                case WM_SAVE_RECORD_FORCE:
                    _ = Task.Run(async () =>
                    {
                        EnableToolStrip(false);
                        try
                        {
                            if (this.m_webExternalHost.CanCallNew(
                                this.commander,
                                msg) == true)
                            {
                                await this.SaveRecordAsync("displaysuccess,changereaderforce");
                            }
                        }
                        finally
                        {
                            EnableToolStrip(true);
                        }
                    });
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

        // (为了兼容以前的 public API。即将弃用。线程模型不理想)
        // parameters:
        //      strAction   为foregift和return之一
        void Foregift(string strAction)
        {
            var task = ForegiftAsync(strAction);
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
            }
        }

        public Task ForegiftAsync(string strAction)
        {
            return Task.Factory.StartNew(async () =>
            {
                await _foregiftAsync(strAction);
            },
this.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        async Task _foregiftAsync(string strAction)
        {
            if (this.ReaderXmlChanged == true
                || this.ObjectChanged == true)
            {
                this.MessageBoxShow("当前有信息被修改后尚未保存。必须先保存后，才能进行创建押金的操作。");
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
                nRet = DoVerifyPatronBarcode(this.readerEditControl1.RecPath,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#if REMOVED
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
#endif
            }

            string strActionName = "押金交费";

            if (strAction == "return")
                strActionName = "押金退费";

            var looping = Looping(out LibraryChannel channel,
                "正在创建读者记录 " + this.readerEditControl1.Barcode + " 的" + strActionName + "记录 ...",
                "disableControl");
            try
            {
                string strReaderBarcode = this.readerEditControl1.Barcode;

                Debug.Assert(strAction == "foregift" || strAction == "return", "");

                long lRet = channel.Foregift(
                    looping.Progress,
                    strAction,
                    strReaderBarcode,
                    out string strOutputrReaderXml,
                    out string strOutputID,
                    out strError);
                if (lRet == -1)
                {
                    goto ERROR1;
                }
            }
            finally
            {
                looping.Dispose();
            }

            // 重新装载窗口内容
            var ret = await LoadRecordAsync(this.readerEditControl1.Barcode,
                false);
            if (ret == 1)
                this.MessageBoxShow("创建" + strActionName + "记录成功");
            return;
        ERROR1:
            this.MessageBoxShow(strError);
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
                strError += "。无法创建证件照片";
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

            bool control = (Control.ModifierKeys == Keys.Control);

            if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl)
                || control)
            {
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
            }
            else
            {
                // 利用人脸中心捕获读者照片
                try
                {
                    // 注： new CameraPhotoDialog() 可能会抛出异常
                    using (PatronPhotoDialog dlg = new PatronPhotoDialog())
                    {
                        // MainForm.SetControlFont(dlg, this.Font, false);
                        dlg.Font = this.Font;

                        Program.MainForm.AppInfo.LinkFormState(dlg, "CameraPhotoDialog_state");
                        dlg.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(dlg);

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
                catch (Exception ex)
                {

                }
                finally
                {
                }


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


        /// <summary>
        /// 标记删除当前记录的证件照片对象或人脸照片对象
        /// </summary>
        public int ClearCardPhoto(string usage = "cardphoto")
        {
            List<ListViewItem> items = this.binaryResControl1.FindItemByUsage(usage);
            if (items.Count > 0)
                return this.binaryResControl1.MaskDelete(items);
            return 0;
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
        /// 设置当前记录的证件或人脸识别用照片对象
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

        /// <summary>
        /// 是否在读者窗范围内自动关闭 掌纹、指纹 键盘仿真
        /// </summary>
        public bool DisableBioSendkey
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
    "reader_info_form",
    "disable_bio_sendkey",
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

                LibraryChannel channel = this.GetChannel();

                /*
                _stop.OnStop += new StopEventHandler(this.DoStop);
                _stop.Initial("正在生成 HTML 预览 ...");
                _stop.BeginLoop();
                */
                var looping = BeginLoop(this.DoStop, "正在生成 HTML 预览 ...");

                EnableControls(false);

                try
                {
                    byte[] baTimestamp = null;
                    string strOutputRecPath = "";

                    string strBarcode = strReaderXml;

                    string[] results = null;
                    long lRet = channel.GetReaderInfo(
                        looping.Progress,
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

                    /*
                    _stop.EndLoop();
                    _stop.OnStop -= new StopEventHandler(this.DoStop);
                    _stop.Initial("");
                    */
                    EndLoop(looping);

                    this.ReturnChannel(channel);
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

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在移动读者记录 " + this.readerEditControl1.RecPath + " 到 " + saveto_dlg.RecPath + "...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在移动读者记录 " + this.readerEditControl1.RecPath + " 到 " + saveto_dlg.RecPath + "...");

            EnableControls(false);
            try
            {
                strTargetRecPath = saveto_dlg.RecPath;

                byte[] target_timestamp = null;
                long lRet = channel.MoveReaderInfo(
    looping.Progress,
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

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);

                this.ReturnChannel(channel);
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

#if !NEWFINGER

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

#endif
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

        #endregion

        #region 掌纹登记功能

        // 局部更新掌纹信息高速缓存
        // return:
        //      -2  remoting服务器连接失败。驱动程序尚未启动
        //      -1  出错
        //      0   成功
        int UpdatePalmprintCache(
            string strBarcode,
            string strFingerprint,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(Program.MainForm.PalmprintReaderUrl) == true)
            {
                strError = $"尚未配置 {Program.MainForm.GetPalmName()}阅读器URL 系统参数，无法更新{Program.MainForm.GetPalmName()}高速缓存";
                return -1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.PalmprintReaderUrl,
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

        async Task<GetFingerprintStringResult> CancelReadPalmprintString()
        {
            string strError = "";
            GetFingerprintStringResult result = new GetFingerprintStringResult();

            if (string.IsNullOrEmpty(Program.MainForm.PalmprintReaderUrl) == true)
            {
                strError = $"尚未配置 {Program.MainForm.GetPalmName()}阅读器URL 系统参数，无法读取{Program.MainForm.GetPalmName()}信息";
                goto ERROR1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.PalmprintReaderUrl,
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
                    strError = "针对 " + Program.MainForm.PalmprintReaderUrl + " 的 GetFingerprintString() 操作失败: " + ex.Message;
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
        async Task<GetFingerprintStringResult> ReadPalmprintString(string strExcludeBarcodes)
        {
            string strError = "";
            GetFingerprintStringResult result = new GetFingerprintStringResult();

            if (string.IsNullOrEmpty(Program.MainForm.PalmprintReaderUrl) == true)
            {
                strError = $"尚未配置 {Program.MainForm.GetPalmName()}阅读器URL 系统参数，无法读取{Program.MainForm.GetPalmName()}信息";
                goto ERROR1;
            }

            FingerprintChannel channel = StartFingerprintChannel(
                Program.MainForm.PalmprintReaderUrl,
                out strError);
            if (channel == null)
                goto ERROR1;

            _inFingerprintCall++;
            try
            {
                return await GetFingerprintString(channel, strExcludeBarcodes);
            }
            catch (Exception ex)
            {
                strError = "针对 " + Program.MainForm.PalmprintReaderUrl + " 的 GetFingerprintString() 操作失败: " + ex.Message;
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

        #endregion

        private async void toolStripButton_registerFingerprint_Click(object sender, EventArgs e)
        {
            bool bPractice = (Control.ModifierKeys == Keys.Control);

#if NEWFINGER
            await registerPalmprintAsync(bPractice);

#else
            string strError = "";

            this.ShowMessage("等待扫描指纹 ...");
            this.EnableControls(false);
            // Program.MainForm.StatusBarMessage = "等待扫描指纹...";
            try
            {
                NormalResult getstate_result = await FingerprintGetState(Program.MainForm.FingerprintReaderUrl, "");
                if (getstate_result.Value == -1)
                {
                    strError = $"指纹中心当前状态不正确：{getstate_result.ErrorInfo}";
                    goto ERROR1;
                }

                getstate_result = await FingerprintGetState(Program.MainForm.FingerprintReaderUrl, "getLibraryServerUID");
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
                    AddImportantField("fingerprint");
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
#endif
        }

        private void toolStripMenuItem_clearFingerprint_Click(object sender, EventArgs e)
        {
            this.readerEditControl1.FingerprintFeatureVersion = "";
            this.readerEditControl1.FingerprintFeature = "";
            this.readerEditControl1.Changed = true;
            AddImportantField("fingerprint");
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
            PropertyDlg dlg = new PropertyDlg();
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

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在加好友 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在加好友 ...");

            EnableControls(false);
            try
            {
                // Result.Value -1出错 0请求成功(注意，并不代表对方同意) 1:请求前已经是好友关系了，没有必要重复请求 2:已经成功添加
                lRet = channel.SetFriends(
    looping.Progress,
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

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);

                this.ReturnChannel(channel);
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

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在导出读者信息到 Excel 文件 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在导出读者信息到 Excel 文件 ...");

            EnableControls(false);

            try
            {
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
                    looping.Progress,
                    false,
                    true,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;
                return;
            }
            finally
            {
                EnableControls(true);

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);

            }

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

            LibraryChannel channel = this.GetChannel();

            /*
            if (_stop != null)
            {
                _stop.OnStop += new StopEventHandler(this.DoStop);
                _stop.Initial("正在装载借阅历史 ...");
                _stop.BeginLoop();
            }
            */
            var looping = BeginLoop(this.DoStop, "正在装载借阅历史 ...");

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
#if SUPPORT_OLD_STOP
                this.ChannelDoEvents = true;
#endif
                // this.Channel.Idle += Channel_Idle;  // 防止控制权出让给正在获取摘要的读者信息 HTML 页面
                try
                {
                    lRet = channel.LoadChargingHistory(
                        looping.Progress,
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
#if SUPPORT_OLD_STOP
                    this.ChannelDoEvents = true;
#endif
                }

                FillBorrowHistoryPage(total_results, nPageNo * _itemsPerPage, (int)lRet);
                return 0;
            }
            finally
            {
                EnableControls(true);

                /*
                if (_stop != null)
                {
                    _stop.EndLoop();
                    _stop.OnStop -= new StopEventHandler(this.DoStop);
                    _stop.Initial("");
                }
                */
                EndLoop(looping);

                this.ReturnChannel(channel);
            }
        }


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

            this.TryInvoke(() =>
            {
                ImageUtil.SetImage(this.pictureBox_qrCode, null);   // 2016/12/28
                this.textBox_pqr.Text = "";
            });
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
            this.TryInvoke(() =>
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
            });
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

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获得读者二维码 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, "正在获得读者二维码 ...");

            LibraryChannel channel = this.GetChannel();
            try
            {

                // 读入读者记录
                long lRet = channel.VerifyReaderPassword(looping.Progress,
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

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);
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

        List<string> _importantFields = new List<string>();
        void AddImportantField(string name)
        {
            if (_importantFields.IndexOf(name) == -1)
                _importantFields.Add(name);
        }

        void ClearImportantFields()
        {
            _importantFields.Clear();
        }

        // 根据文件登记人脸(用于人脸识别)
        // 可用于 C# 脚本调用
        public async Task<NormalResult> RegisterFaceAsync(string strFaceFileName)
        {
            string strError = "";
            this.ShowMessage("等待登记人脸 ...");
            this.EnableControls(false);
            try
            {
                // 检查 FaceCenter 所连的 dp2library 服务器是否和 dp2circulation 所连的一致
                NormalResult getstate_result = await FaceGetStateAsync("getLibraryServerUID");
                if (getstate_result.Value == -1)
                {
                    // strError = getstate_result.ErrorInfo;
                    return getstate_result;
                }
                else if (getstate_result.ErrorCode != Program.MainForm.ServerUID)
                {
                    strError = $"人脸中心所连接的 dp2library 服务器 UID {getstate_result.ErrorCode} 和内务当前所连接的 UID {Program.MainForm.ServerUID} 不同。无法进行人脸登记";
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }

                byte[] bytes = null;
                using (Stream stream = File.OpenRead(strFaceFileName))
                {
                    bytes = new byte[stream.Length];
                    await stream.ReadAsync(bytes, 0, bytes.Length);
                }

                GetFeatureStringResult feature_result = await ReadFeatureString(bytes, "", "");
                if (feature_result.Value == -1)
                {
                    /*
                    if (feature_result.ErrorCode == "getFeatureFail")
                    {
                        // 无法提取特征的情况，输出报错信息到操作历史，然后继续循环
                        // Program.MainForm.OperHistory.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode($"无法提取人脸特征 {feature_result.ErrorInfo}") + "</div>");
                        return feature_result;
                    }
                    */
                    return feature_result;
                }

                this.readerEditControl1.FaceFeature = feature_result.FeatureString;
                this.readerEditControl1.FaceFeatureVersion = feature_result.Version;
                this.readerEditControl1.Changed = true;
                AddImportantField("face");

                // AddImportantField("face");

                var savePhoto = Program.MainForm.SavePhotoWhileRegisterFace;

                // TODO: 如果尺寸符合要求，则直接用返回的 jpeg 上载
                // 设置人脸照片对象
                if (savePhoto)
                {
                    using (Image image = FromBytes(/*feature_result.ImageData*/bytes))
                    using (Image image1 = new Bitmap(image))
                    {
                        // 自动缩小图像
                        int nRet = SetCardPhoto(image1,
                            "face",
                            out string strShrinkComment,
                            out strError);
                        if (nRet == -1)
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError,
                                ErrorCode = "setCardPhotoError"
                            };
                    }
                }
                else
                {
                    // 清除 usage 为 "face" 的对象
                    ClearCardPhoto("face");
                }
                return new NormalResult();

            }
            finally
            {
                this.EnableControls(true);
                this.ClearMessage();
            }
        }

        // 登记人脸。用于人脸识别
        private async void toolStripSplitButton_registerFace_ButtonClick(object sender, EventArgs e)
        {
            string strError = "";
            this.ShowMessage("等待登记人脸 ...");
            this.EnableControls(false);
            try
            {
                var version_result = await GetFaceVersionAsync();
                if (version_result.Value == -1)
                {
                    strError = version_result.ErrorInfo;
                    if (version_result.ErrorCode == "RequestError")
                        strError += " 可能是因为 人脸中心(FaceCenter) 模块没有启动";
                    goto ERROR1;
                }

                string version = version_result.ErrorCode;
                // 要返回人脸特征
                if (StringUtil.CompareVersion(version, "1.5.12") < 0)
                {
                    strError = $"人脸登记功能必须和 facecenter 1.5.12 或以上版本配套使用(但当前 facecenter 为 {version} 版)";
                    goto ERROR1;
                }

                // 2021/10/14
                // 检查 FaceCenter 所连的 dp2library 服务器是否和 dp2circulation 所连的一致
                NormalResult getstate_result = await FaceGetStateAsync("getLibraryServerUID");
                if (getstate_result.Value == -1)
                {
                    strError = getstate_result.ErrorInfo;
                    goto ERROR1;
                }
                else if (getstate_result.ErrorCode != Program.MainForm.ServerUID)
                {
                    strError = $"人脸中心所连接的 dp2library 服务器 UID {getstate_result.ErrorCode} 和内务当前所连接的 UID {Program.MainForm.ServerUID} 不同。无法进行人脸登记";
                    goto ERROR1;
                }

                var savePhoto = Program.MainForm.SavePhotoWhileRegisterFace;
                string style = "ui,confirmPicture,searchDup";
                if (savePhoto)
                    style += ",returnImage";
                REDO:
                GetFeatureStringResult result = await ReadFeatureString(
                    null,
                    this.readerEditControl1.Barcode,
                    style);

                if (result.ErrorCode == "alreadyExist")
                    result.ErrorInfo = $"登记人脸被拒绝: {result.ErrorInfo}";

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
                    string log_info = "";
                    if (result.ErrorCode == "alreadyExist")
                    {
                        // string version = await GetFaceVersionAsync();

                        // 要返回人脸特征
                        if (StringUtil.CompareVersion(version, "1.5.12") >= 0
                            && result.ImageData != null)
                        {
                            // 记载到本地 log 中
                            string barcode = this.readerEditControl1.Barcode;
                            string filename = Path.Combine(MainForm.UserLogDir, $"face_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_{barcode}.png");
                            using (Image image = FromBytes(result.ImageData))
                            {
                                image.Save(filename, ImageFormat.Png);
                            }
                            MainForm.WriteInfoLog($"为读者 '{barcode}' 进行人脸登记过程中发现 {result.ErrorInfo}。\r\n读者 '{barcode}' 的人脸特征为:\r\n{result.FeatureString}\r\n其人脸图片已存入文件 {filename}");
                            log_info = "。\r\n当前读者人脸信息(未登记)已写入错误日志";
                        }
                    }

                    strError = result.ErrorInfo + log_info;
                    goto ERROR1;
                }

                this.readerEditControl1.FaceFeature = result.FeatureString;
                this.readerEditControl1.FaceFeatureVersion = result.Version;
                this.readerEditControl1.Changed = true;
                AddImportantField("face");

                // 2021/7/22
                // AddImportantField("face");

                // TODO: 如果尺寸符合要求，则直接用返回的 jpeg 上载
                // 设置人脸照片对象
                if (savePhoto)
                {
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
                else
                {
                    // 清除 usage 为 "face" 的对象
                    ClearCardPhoto("face");
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
            this.ShowMessage(strError, "red", true);
            ShowMessageBox(strError);
        }

        // 获得人脸接口版本号
        async Task<NormalResult> GetFaceVersionAsync()
        {
            return await FaceGetStateAsync("getVersion");
        }

        public static Image FromBytes(byte[] bytes)
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
            string strError = "";

            string library_code = Program.MainForm.GetReaderDbLibraryCode(Global.GetDbName(this.ReaderEditControl.RecPath));
            int ret = this.readerEditControl1.GetData(out string xml,
                out strError);
            if (ret == -1)
            {
                goto ERROR1;
            }
            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                strError = $"读者 XML 装入 DOM 出现异常: {ex.Message}";
                goto ERROR1;
            }
            using (RfidPatronCardDialog dlg = new RfidPatronCardDialog())
            {
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.SetData(this.ReaderEditControl,
                    library_code,
                    readerdom,
                    out string strWarning);
                if (string.IsNullOrEmpty(strWarning) == false)
                {
                    // 警告，询问是否继续创建标签
                    DialogResult result = MessageBox.Show(this,
    strWarning + "\r\n\r\n是否继续创建读者卡?\r\n\r\n(Yes: 继续; No: 放弃)",
    "ReaderInfoForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
                Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_RfidPatronCardDialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripMenuItem_bindCardNumber_Click(object sender, EventArgs e)
        {
            using (BindCardNumberDialog dlg = new BindCardNumberDialog())
            {
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.Numbers = this.readerEditControl1.CardNumber;
                Program.MainForm.AppInfo.LinkFormState(dlg, "ReaderInfoForm_BindCardNumberDialog_state");
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                    return;
                this.readerEditControl1.CardNumber = dlg.Numbers;
            }
        }

        // 清除人脸特征和图片
        private void toolStripMenuItem_clearFaceFeature_Click(object sender, EventArgs e)
        {
            this.readerEditControl1.FaceFeatureVersion = "";
            this.readerEditControl1.FaceFeature = "";
            this.readerEditControl1.Changed = true;
            AddImportantField("face");

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
#if NEWFINGER
            await registerPalmprintAsync(false);
#else
            await registerFingerprint(false);
#endif
        }

        private async void ToolStripMenuItem_fingerprintPracticeMode_Click(object sender, EventArgs e)
        {
#if NEWFINGER
            await registerPalmprintAsync(true);
#else
            await registerFingerprint(true);
#endif
        }

#if !NEWFINGER
        async Task registerFingerprint(bool bPractice)
        {
            string strError = "";

            this.ShowMessage("等待扫描指纹 ...");
            this.EnableControls(false);
            // Program.MainForm.StatusBarMessage = "等待扫描指纹...";
            try
            {
                NormalResult getstate_result = await FingerprintGetState(Program.MainForm.FingerprintReaderUrl, "");
                if (getstate_result.Value == -1)
                {
                    strError = $"指纹中心当前状态不正确：{getstate_result.ErrorInfo}";
                    goto ERROR1;
                }

                getstate_result = await FingerprintGetState(Program.MainForm.FingerprintReaderUrl, "getLibraryServerUID");
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
                    AddImportantField("fingerprint");
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
#endif

        private void ToolStripMenuItem_saveChangeState_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_SAVE_RECORD_STATE);  // 只修改读者记录状态

        }

        // 2020/12/29
        // 登记掌纹
        private async void toolStripSplitButton_registerPalmprint_ButtonClick(object sender, EventArgs e)
        {
            await registerPalmprintAsync(false);
        }

        async Task registerPalmprintAsync(bool bPractice)
        {
            string strError = "";
            string caption = Program.MainForm.GetPalmName();

            using (CancellationTokenSource cancel = CancellationTokenSource.CreateLinkedTokenSource(_cancel.Token))
            {
                var token = cancel.Token;

                RegisterPalmprintDialog dlg = new RegisterPalmprintDialog();
                dlg.Text = $"登记{caption}";
                dlg.FormClosed += (s, e) =>
                {
                    {
                        if (MainForm.AppInfo != null)
                            MainForm.AppInfo.UnlinkFormState(dlg);
                    }
                    cancel.Cancel();
                    _ = Task.Run(async () =>
                    {
                        await CancelReadPalmprintString();
                    });
                };
                token.Register(() =>
                {
                    this.Invoke((Action)(() =>
                    {
                        dlg.Close();
                    }));
                });
                // dlg.StartPosition = FormStartPosition.CenterScreen;
                MainForm.AppInfo.LinkFormState(dlg, "RegisterPalmprintDialog_state");
                dlg.Show(this);
                dlg.Message = $"等待扫入{caption} ...";

                FingerprintManager.Touched += PalmprintManager_Touched;

                this.ShowMessage($"等待扫描{caption} ...");
                this.EnableControls(false);

                var task = Task.Run(async () =>
                {
                    // 暂停掌纹图像对话框的显示
                    MainForm.PausePalmprintDisplay();
                    try
                    {
                        while (token.IsCancellationRequested == false)
                        {
                            // 2021/11/1
                            if (FingerprintManager.Pause == true)
                            {
                                dlg.DisplayError("暂停显示", Color.DarkGray);
                                await Task.Delay(TimeSpan.FromSeconds(1), token);
                                continue;
                            }

                            var result = FingerprintManager.GetImage("wait:1000,rect");
                            if (result.ImageData == null)
                            {
                                Thread.Sleep(50);
                                continue;
                            }

                            var image = FromBytes(result.ImageData);
                            if (string.IsNullOrEmpty(result.Text) == false)
                                Charging.PalmprintForm.PaintLines(image, result.Text);
                            dlg.Invoke(new Action(() =>
                            {
                                dlg.Image = image;
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        // 写入错误日志
                        MainForm.WriteErrorLog($"显示{caption}图像出现异常: {ExceptionUtil.GetDebugText(ex)}");
                    }
                    finally
                    {
                        // 恢复掌纹图像对话框的显示
                        MainForm.ContinuePalmprintDisplay();
                    }
                });

                try
                {
                    NormalResult getstate_result = await FingerprintGetState(Program.MainForm.PalmprintReaderUrl, "");
                    if (getstate_result.Value == -1)
                    {
                        strError = $"{caption}中心当前状态不正确：{getstate_result.ErrorInfo}";
                        goto ERROR1;
                    }

                    // 2022/6/9
                    strError = Program.MainForm.CheckPalmCenterVersion();
                    if (strError != null)
                    {
                        goto ERROR1;
                    }

                    getstate_result = await FingerprintGetState(Program.MainForm.PalmprintReaderUrl, "getLibraryServerUID");
                    if (getstate_result.Value == -1)
                    {
                        strError = getstate_result.ErrorInfo;
                        goto ERROR1;
                    }
                    else if (getstate_result.ErrorCode != null &&
                        getstate_result.ErrorCode != Program.MainForm.ServerUID)
                    {
                        strError = $"{caption}中心所连接的 dp2library 服务器 UID {getstate_result.ErrorCode} 和内务当前所连接的 UID {Program.MainForm.ServerUID} 不同。无法进行{caption}登记";
                        goto ERROR1;
                    }

                    // 暂停识别掌纹
                    getstate_result = await FingerprintGetState(Program.MainForm.PalmprintReaderUrl, "pauseCapture");
                    if (getstate_result.Value == -1)
                    {
                        strError = getstate_result.ErrorInfo;
                        goto ERROR1;
                    }

                // TODO: 练习模式需要判断版本 2.2 以上

                REDO:
                    string exclude = this.readerEditControl1.Barcode;
                    if (bPractice)
                        exclude = "!practice";
#if NEWFINGER
                    if (Program.MainForm.IsFingerprint())
                        exclude += ",!disableUI";
#endif
                    GetFingerprintStringResult result = await ReadPalmprintString(
                        exclude);
                    /*
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
                    */

                    if (result.Value == -1 || result.Value == 0)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }

                    this.Invoke((Action)(() =>
                    {
                        dlg.CancelButtonText = "关闭";
                        dlg.ColorMode = "green";
                    }));

#if NO
                strFingerprint = "12345";   // test
                strVersion = "test-version";
#endif

                    if (bPractice == false)
                    {
                        if (Program.MainForm.IsFingerprint() == false)
                        {
                            this.readerEditControl1.PalmprintFeature = result.Fingerprint;   // strFingerprint;
                            this.readerEditControl1.PalmprintFeatureVersion = result.Version;    // strVersion;
                            this.readerEditControl1.Changed = true;
                            AddImportantField("palmprint");
                        }
                        else
                        {
                            this.readerEditControl1.FingerprintFeature = result.Fingerprint;
                            this.readerEditControl1.FingerprintFeatureVersion = result.Version;
                            this.readerEditControl1.Changed = true;
                            AddImportantField("fingerprint");
                        }
                    }

                    try
                    {
                        // await Task.Delay(TimeSpan.FromSeconds(5), _cancel.Token);
                        await Task.Delay(TimeSpan.FromSeconds(5), cancel.Token);
                    }
                    catch
                    {

                    }
                }
                finally
                {
                    {
                        // 恢复识别掌纹
                        var getstate_result = await FingerprintGetState(Program.MainForm.PalmprintReaderUrl, "continueCapture");
                        if (getstate_result.Value == -1)
                        {
                            MainForm.WriteErrorLog($"registerPalmprintAsync() 中恢复识别{caption}时出错: {getstate_result.ErrorInfo}");
                        }
                    }

                    try
                    {
                        this.EnableControls(true);
                    }
                    catch
                    {

                    }
                    this.ClearMessage();

                    FingerprintManager.Touched -= PalmprintManager_Touched;

                    cancel.Cancel();
                    dlg.Close();
                }

                // 显示获取掌纹中途的信息
                void PalmprintManager_Touched(object sender, TouchedEventArgs e)
                {
                    // this.ShowMessage(e.Message);

                    // 此处不接受除提示外的其他消息
                    if (e.Quality != -1)
                        return;

                    string type = "";
                    var text = e.Message;
                    if (text.Contains(":"))
                    {
                        var parts = StringUtil.ParseTwoPart(text, ":");
                        type = parts[0];
                        text = parts[1];
                    }

                    if (type == "register")
                    {
                        dlg.Invoke((Action)(() =>
                        {
#if REMOVED
                    // 2021/10/28
                    if (e.Message.StartsWith("!image")
                        && e.Result != null && e.Result.ImageData != null)
                    {
                        // 图像消息
                        try
                        {
                            ParseWidthHeight(e.Message.Substring("!image:".Length),
        out int width,
        out int height);
                            var bmp = ToGrayBitmap(e.Result.ImageData, width, height);
                            dlg.Image = bmp;
                        }
                        catch (Exception ex)
                        {
                            dlg.Message = "异常: " + ex.Message;
                        }
                    }
                    else
#endif
                            dlg.Message = text; //  e.Message;
                        }));
                    }
                }
            }

            // Program.MainForm.Speak("掌纹信息获取成功");
            Program.MainForm.StatusBarMessage = $"{caption}信息获取成功";
            return;
        ERROR1:
            Program.MainForm.StatusBarMessage = strError;
            this.ShowMessage(strError, "red", true);
        }

        static void ParseWidthHeight(string text,
            out int width,
            out int height)
        {
            var parts = StringUtil.ParseTwoPart(text, "*");
            if (Int32.TryParse(parts[0], out width) == false)
                throw new ArgumentException($"长宽字符串 '{text}' 格式不正确。星号左侧应该是数字");
            if (Int32.TryParse(parts[1], out height) == false)
                throw new ArgumentException($"长宽字符串 '{text}' 格式不正确。星号右侧应该是数字");
        }

#if REMOVED
        public static Bitmap ToGrayBitmap(byte[] rawValues, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// 获取图像参数
            int stride = bmpData.Stride;  // 扫描线的宽度
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置
            int scanBytes = stride * height;   // 用stride宽度，表示这是内存区域的大小

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组
            int posScan = 0, posReal = 0;   // 分别设置两个位置指针，指向源数组和目标数组
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存
            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描
                for (int y = 0; y < width; y++)
                {
                    pixelValues[posScan++] = rawValues[posReal++];
                }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域

            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = tempPalette;
            //// 算法到此结束，返回结果
            return bmp;
        }

#endif
        private void toolStripMenuItem_clearPalmprint_Click(object sender, EventArgs e)
        {
            this.readerEditControl1.PalmprintFeatureVersion = "";
            this.readerEditControl1.PalmprintFeature = "";
            this.readerEditControl1.Changed = true;
            AddImportantField("palmprint");
        }


        #region 将 dt1000 读者 MARC 记录转换为 dp2 的 XML 格式

        public static int ConvertDt1000ReaderMarcToXml(MarcRecord record,
            string path,
            string timestamp,
            out XmlDocument dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            List<string> warnings = new List<string>();

            /*
            var g01 = record.select("field[@name='-01']").FirstContent;
            var parts = StringUtil.ParseTwoPart(g01, "|");
            string path = ToDp2Path(parts[0]);
            string timestamp = parts[1];
            */
            /*
            ParseDt1000G01(record,
    out string path,
    out string timestamp);
            */

            dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 2021/12/17
            // 给根元素设置几个参数
            if (path != null)
                DomUtil.SetAttr(dom.DocumentElement, "path", DpNs.dprms, path);
            if (timestamp != null)
                DomUtil.SetAttr(dom.DocumentElement, "timestamp", DpNs.dprms, timestamp);

            // 读者证条码
            string strBarcode = "";

            // 以字段/子字段名从记录中得到第一个子字段内容。
            // parameters:
            //		strMARC	机内格式MARC记录
            //		strFieldName	字段名。内容为字符
            //		strSubfieldName	子字段名。内容为1字符
            // return:
            //		""	空字符串。表示没有找到指定的字段或子字段。
            //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
            strBarcode = record.select("field[@name='100']/subfield[@name='a']").FirstContent;

            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                warnings.Add("MARC记录中缺乏100$a读者证条码号");
            }
            else
            {
                strBarcode = strBarcode.ToUpper();
            }

            if (string.IsNullOrEmpty(strBarcode) == false)
                DomUtil.SetElementText(dom.DocumentElement, "barcode", strBarcode);


            // 证号
            // 2008/10/14 new add
            string strCardNumber = "";

            strCardNumber = record.select("field[@name='100']/subfield[@name='b']").FirstContent;
            if (String.IsNullOrEmpty(strCardNumber) == false)
                DomUtil.SetElementText(dom.DocumentElement, "cardNumber", strCardNumber);

            // 密码
            string strPassword = record.select("field[@name='080']/subfield[@name='a']").FirstContent;
            if (String.IsNullOrEmpty(strPassword) == true)
            {
                // 2021/12/17
                // 如果读者记录原来没有密码，则产生随机密码。对读者记录起到保护作用
                strPassword = Guid.NewGuid().ToString();
            }

            {
                try
                {
                    // TODO: 用最新 hash 算法
                    strPassword = Cryptography.GetSHA1(strPassword);
                }
                catch
                {
                    strError = "将密码明文转换为SHA1时发生错误";
                    return -1;
                }

                DomUtil.SetElementText(dom.DocumentElement, "password", strPassword);
            }

            // 读者类型
            string strReaderType = record.select("field[@name='110']/subfield[@name='a']").FirstContent;

            DomUtil.SetElementText(dom.DocumentElement, "readerType", strReaderType);

            /*
            // 发证日期
            DomUtil.SetElementText(dom.DocumentElement, "createDate", strCreateDate);
             * */

            // 失效期
            string strExpireDate = record.select("field[@name='110']/subfield[@name='d']").FirstContent;

            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                string strToday = DateTimeUtil.DateTimeToString8(DateTime.Now);

                // 2009/2/26 new add
                // 兼容4/6字符形态
                if (strExpireDate.Length == 4)
                {
                    strExpireDate = strExpireDate + "0101";
                }
                else if (strExpireDate.Length == 6)
                {
                    strExpireDate = strExpireDate + "01";
                }

                if (strExpireDate.Length != 8)
                {
                    warnings.Add("110$d中的失效期  '" + strExpireDate + "' 应为8字符。升级程序自动以 " + strToday + " 充当失效期");
                    strExpireDate = strToday;   // 2008/8/26 new add
                }

                Debug.Assert(strExpireDate.Length == 8, "");

                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strExpireDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        warnings.Add("MARC数据中110$d日期字符串转换格式为rfc1123时发生错误: " + strError);
                        strExpireDate = strToday;   // 2008/8/26 new add

                        // 2008/10/28 new add
                        nRet = DateTimeUtil.Date8toRfc1123(strExpireDate,
                            out strTarget,
                            out strError);
                        Debug.Assert(nRet != -1, "");
                    }

                    strExpireDate = strTarget;
                }

                DomUtil.SetElementText(dom.DocumentElement, "expireDate", strExpireDate);
            }

            // 押金
            // 2008/11/13 new add
            string strForegift = record.select("field[@name='110']/subfield[@name='e']").FirstContent;

            if (String.IsNullOrEmpty(strForegift) == false)
            {
                long foregift = 0;
                try
                {
                    foregift = Convert.ToInt64(strForegift);
                }
                catch (Exception /*ex*/)
                {
                    warnings.Add("MARC数据中110$e押金字符串 '" + strForegift + "' 格式错误");
                    strForegift = "";
                    goto SKIP_COMPUTE_FOREGIFT;
                }

                double new_foregift = (double)foregift / (double)100;
                strForegift = "CNY" + new_foregift.ToString();
            }

        SKIP_COMPUTE_FOREGIFT:
            if (String.IsNullOrEmpty(strForegift) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "foregift", strForegift);
            }

            // 停借原因
            string strState = record.select("field[@name='982']/subfield[@name='b']").FirstContent;

            if (String.IsNullOrEmpty(strState) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "state", strState);
            }

            // 姓名
            string strName = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
            if (String.IsNullOrEmpty(strName) == true)
            {
                warnings.Add("MARC记录中缺乏200$a读者姓名");
            }

            DomUtil.SetElementText(dom.DocumentElement, "name", strName);

            // 姓名拼音
            string strNamePinyin = record.select("field[@name='200']/subfield[@name='A']").FirstContent;

            if (String.IsNullOrEmpty(strNamePinyin) == false)
            {
                DomUtil.SetElementText(dom.DocumentElement, "namePinyin", strNamePinyin);
            }

            // 性别
            string strGender = record.select("field[@name='200']/subfield[@name='b']").FirstContent;

            DomUtil.SetElementText(dom.DocumentElement, "gender", strGender);

            /*
            // 生日
            // 2008/10/14 new add 未证实
            string strBirthday = "";
            strBirthday = MarcUtil.GetFirstSubfield(strMARC,
                "200",
                "c");

            DomUtil.SetElementText(dom.DocumentElement, "birthday", strBirthday);
             * */

            // 身份证号

            // 单位
            string strDepartment = record.select("field[@name='300']/subfield[@name='a']").FirstContent;

            DomUtil.SetElementText(dom.DocumentElement, "department", strDepartment);

            // 地址
            string strAddress = record.select("field[@name='400']/subfield[@name='b']").FirstContent;

            DomUtil.SetElementText(dom.DocumentElement, "address", strAddress);

            // 邮政编码
            string strZipCode = record.select("field[@name='400']/subfield[@name='a']").FirstContent;
            if (string.IsNullOrEmpty(strZipCode) == false)
                DomUtil.SetElementText(dom.DocumentElement, "zipcode", strZipCode);

            // 电话
            string strTel = record.select("field[@name='300']/subfield[@name='b']").FirstContent;
            if (string.IsNullOrEmpty(strTel) == false)
                DomUtil.SetElementText(dom.DocumentElement, "tel", strTel);

            // email

            // 所借阅的各册
            string strField986 = "";
            string strNextFieldName = "";
            // 从记录中得到一个字段
            // parameters:
            //		strMARC		机内格式MARC记录
            //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
            //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
            //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
            //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
            //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = MarcUtil.GetField(record.Text,
    "986",
    0,
    out strField986,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得986字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeBorrows = dom.CreateElement("borrows");
                nodeBorrows = dom.DocumentElement.AppendChild(nodeBorrows);

                string strWarningParam = "";
                nRet = CreateBorrowsNode(nodeBorrows,
                    strField986,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据986字段内容创建<borrows>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    warnings.Add(strWarningParam);
            }


            string strField988 = "";
            // 违约金记录
            nRet = MarcUtil.GetField(record.Text,
    "988",
    0,
    out strField988,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得988字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeOverdues = dom.CreateElement("overdues");
                nodeOverdues = dom.DocumentElement.AppendChild(nodeOverdues);

                string strWarningParam = "";
                nRet = CreateOverduesNode(nodeOverdues,
                    strField988,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据988字段内容创建<overdues>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    warnings.Add(strWarningParam);

            }


            string strField984 = "";
            // 预约信息
            nRet = MarcUtil.GetField(record.Text,
    "984",
    0,
    out strField984,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得984字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeReservations = dom.CreateElement("reservations");
                nodeReservations = dom.DocumentElement.AppendChild(nodeReservations);

                string strWarningParam = "";
                nRet = CreateReservationsNode(nodeReservations,
                    strField984,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据984字段内容创建<reservations>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    warnings.Add(strWarningParam);

            }

            string strField989 = "";
            // 借阅历史
            nRet = MarcUtil.GetField(record.Text,
    "989",
    0,
    out strField989,
    out strNextFieldName);
            if (nRet == -1)
            {
                strError = "从MARC记录中获得989字段时出错";
                return -1;
            }
            if (nRet == 1)
            {
                XmlNode nodeBorrowHistory = dom.CreateElement("borrowHistory");
                nodeBorrowHistory = dom.DocumentElement.AppendChild(nodeBorrowHistory);

                string strWarningParam = "";
                nRet = CreateBorrowHistoryNode(nodeBorrowHistory,
                    strField989,
                    out strWarningParam,
                    out strError);
                if (nRet == -1)
                {
                    strError = "根据989字段内容创建<borrowHistory>节点时出错: " + strError;
                    return -1;
                }

                if (String.IsNullOrEmpty(strWarningParam) == false)
                    warnings.Add(strWarningParam);

            }

            // 遮盖MARC记录中的808$a内容
            strPassword = record.select("field[@name='080']/subfield[@name='a']").FirstContent;
            if (String.IsNullOrEmpty(strPassword) == false)
            {
                record.setFirstSubfield("080", "a", "********");
            }

            // 保留原始记录供参考
            {
                // 删除 997 字段
                record.select("field[@name='997']").detach();

                string strPlainText = record.Text.Replace(MarcUtil.SUBFLD, '$');
                strPlainText = strPlainText.Replace(new String(MarcUtil.FLDEND, 1), "#\r\n");
                if (strPlainText.Length > 24)
                    strPlainText = strPlainText.Insert(24, "\r\n");

                DomUtil.SetElementText(dom.DocumentElement, "originMARC", strPlainText);
            }

            if (warnings.Count > 0)
            {
                strError = StringUtil.MakePathList(warnings, "; ");
                DomUtil.SetElementText(dom.DocumentElement, "comment", strError);
            }

            // 2021/12/17
            DomUtil.RemoveEmptyElements(dom.DocumentElement);
            return 0;
        }

        // 从 dt1000 MARC 记录中的若干 -01 字段中选择一个来源数据库
        // /132.147.160.100/图书总库/ctlno/0000001
        public static int SelectDt1000G01Source(
            Form owner,
            MarcRecord record,
            out string path,
            out string timestamp)
        {
            path = "";
            timestamp = "";

            // 来源。去掉后面 /ctlno/xxxxx 部分的
            List<string> sources = new List<string>();
            List<string> timestamps = new List<string>();

            // 原始路径
            // List<string> paths = new List<string>();

            var fields = record.select("field[@name='-01']");
            foreach (MarcField field in fields)
            {
                var g01 = field.Content;
                var parts = StringUtil.ParseTwoPart(g01, "|");
                path = parts[0];
                timestamp = parts[1];

                // paths.Add(path);

                // path 去掉后面两截 /ctlno/0000001
                {
                    int pos = path.LastIndexOf("/ctlno/");
                    if (pos != -1)
                        path = path.Substring(0, pos);
                    sources.Add(path);
                }

                timestamps.Add(timestamp);
            }

            if (timestamps.Count == 0)
                return -1;

            int index = 0;
            if (timestamps.Count > 1)
            {
                bool temp = false;
                var result = SelectDlg.GetSelect(
                    owner,
                    "dt1000 数据来源",
                    "请选择数据来源",
                    sources.ToArray(),
                    0,
                    null,
                    ref temp,
                    owner.Font);
                if (result == null)
                    return 0;
                index = sources.IndexOf(result);
            }

            path = sources[index];
            timestamp = timestamps[index];
            return 1;
        }

        // 根据 source 定位并获得 -01 字段中的记录路径
        public static int GetDt1000G01Path(
    MarcRecord record,
    string source,
    out string path,
    out string timestamp)
        {
            path = "";
            timestamp = "";

            var fields = record.select("field[@name='-01']");
            foreach (MarcField field in fields)
            {
                var g01 = field.Content;
                var parts = StringUtil.ParseTwoPart(g01, "|");
                path = parts[0];
                timestamp = parts[1];

                if (path.StartsWith(source + "/"))
                {
                    path = ToDp2Path(path);
                    return 1;
                }
            }

            path = null;
            timestamp = null;
            return 0;
        }

        // 将 dt1000 的记录路径转换为 dp2 形态
        // /132.147.160.100/图书总库/ctlno/0000001
        static string ToDp2Path(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            if (path.StartsWith("/"))
                path = path.Substring(1);

            if (path.IndexOf("/ctlno/") != -1)
                path = path.Replace("/ctlno/", "/");

            // 132.147.160.100/图书总库/0000001 --> 图书总库/0000001
            if (Count(path) == 2)
            {
                int index = path.IndexOf("/");
                if (index != -1)
                    path = path.Substring(index + 1);
            }

            return path;
        }

        static int Count(string path)
        {
            int count = 0;
            foreach (var ch in path.ToCharArray())
            {
                if (ch == '/')
                    count++;
            }

            return count;
        }

        public static string HtmlEncode(string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        // 创建<borrows>节点的下级内容
        static int CreateBorrowsNode(XmlNode nodeBorrows,
            string strField986,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField986,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);

                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                /*
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    false,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "986字段中 册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                XmlNode nodeBorrow = nodeBorrows.OwnerDocument.CreateElement("borrow");
                nodeBorrow = nodeBorrows.AppendChild(nodeBorrow);

                DomUtil.SetAttr(nodeBorrow, "barcode", strBarcode);

                // borrowDate属性
                // 第一次借书日期
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strWarning += "986$t子字段内容 '" + strBorrowDate + "' 的长度不是8字符; ";
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "986字段中$t日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "borrowDate", strBorrowDate);
                }

                // no属性
                // 从什么数字开始计数？
                string strNo = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "y",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strNo = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strNo) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "no", strNo);
                }




                // borrowPeriod属性

                // 根据应还日期计算出来?

                // 应还日期
                string strReturnDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "v",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strReturnDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    if (strReturnDate.Length != 8)
                    {
                        strWarning += "986$v子字段内容 '" + strReturnDate + "' 的长度不是8字符; ";
                    }
                }
                else
                {
                    if (strBorrowDate != "")
                    {
                        strWarning += "986字段中子字段组 " + Convert.ToString(g + 1) + " 有 $t 子字段内容而没有 $v 子字段内容, 不正常; ";
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strReturnDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "986字段中$v日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strReturnDate = "";
                    }
                    else
                    {
                        strReturnDate = strTarget;
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false
                    && String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    // 计算差额天数
                    DateTime timestart = DateTimeUtil.FromRfc1123DateTimeString(strBorrowDate);
                    DateTime timeend = DateTimeUtil.FromRfc1123DateTimeString(strReturnDate);

                    TimeSpan delta = timeend - timestart;

                    string strBorrowPeriod = Convert.ToString(delta.TotalDays) + "day";
                    DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);
                }

                // 续借的日期
                if (strNo != "")
                {
                    string strRenewDate = "";
                    nRet = MarcUtil.GetSubfield(strGroup,
                        ItemType.Group,
                        "x",
                        0,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (strSubfield.Length >= 1)
                    {
                        strRenewDate = strSubfield.Substring(1);

                        if (strRenewDate.Length != 8)
                        {
                            strWarning += "986$x子字段内容 '" + strRenewDate + "' 的长度不是8字符; ";
                        }
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false)
                    {
                        string strTarget = "";
                        nRet = DateTimeUtil.Date8toRfc1123(strRenewDate,
                            out strTarget,
                            out strError);
                        if (nRet == -1)
                        {
                            strWarning += "986字段中$x日期字符串转换格式为rfc1123时发生错误: " + strError;
                            strRenewDate = "";
                        }
                        else
                        {
                            strRenewDate = strTarget;
                        }
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false)
                    {
                        DomUtil.SetAttr(nodeBorrow, "borrowDate", strRenewDate);
                    }

                    if (String.IsNullOrEmpty(strRenewDate) == false
    && String.IsNullOrEmpty(strReturnDate) == false)    // && String.IsNullOrEmpty(strBorrowDate) == false
                    {
                        // 重新计算差额天数
                        DateTime timestart = DateTimeUtil.FromRfc1123DateTimeString(strRenewDate);
                        DateTime timeend = DateTimeUtil.FromRfc1123DateTimeString(strReturnDate);

                        TimeSpan delta = timeend - timestart;

                        string strBorrowPeriod = Convert.ToString(delta.TotalDays) + "day";
                        DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);
                    }
                }
            }

            return 0;
        }

        // 创建<overdues>节点的下级内容
        static int CreateOverduesNode(XmlNode nodeOverdues,
            string strField988,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField988,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                /*
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    false,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;


                if (nRet != 0)
                {
                    strWarning += "988字段中 册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                string strCompleteDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "d",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strCompleteDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strCompleteDate) == false)
                    continue; // 如果已经交了罚金，这个子字段组就忽略了

                XmlNode nodeOverdue = nodeOverdues.OwnerDocument.CreateElement("overdue");
                nodeOverdue = nodeOverdues.AppendChild(nodeOverdue);

                DomUtil.SetAttr(nodeOverdue, "barcode", strBarcode);

                // borrowDate属性
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "e",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strWarning += "988$e子字段内容 '" + strBorrowDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "988字段中$e日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "borrowDate", strBorrowDate);
                }

                // returnDate属性
                string strReturnDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "t",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strReturnDate = strSubfield.Substring(1);

                    if (strReturnDate.Length != 8)
                    {
                        strWarning += "988$t子字段内容 '" + strReturnDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strReturnDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "988字段中$t日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strReturnDate = "";
                    }
                    else
                    {
                        strReturnDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strReturnDate) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "returnDate", strReturnDate);  // 2006/12/29 changed
                }

                // borrowPeriod未知
                //   DomUtil.SetAttr(nodeOverdue, "borrowPeriod", strBorrowPeriod);

                // price和type属性是为兼容dt1000数据而设立的属性
                // 而over超期天数属性就空缺了

                // price属性
                string strPrice = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "c",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strPrice = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strPrice) == false)
                {
                    // 是否需要转换为带货币单位的, 带小数部分的字符串?
                    if (StringUtil.IsPureNumber(strPrice) == true)
                    {
                        // 只有纯数字才作

                        long lOldPrice = 0;

                        try
                        {
                            lOldPrice = Convert.ToInt64(strPrice);
                        }
                        catch
                        {
                            strWarning += "价格字符串 '' 格式不正确，应当为纯数字。";
                            goto SKIP_11;
                        }

                        // 转换为元
                        double dPrice = ((double)lOldPrice) / 100;

                        strPrice = "CNY" + dPrice.ToString();
                    }

                SKIP_11:

                    DomUtil.SetAttr(nodeOverdue, "price", strPrice);
                }

                // type属性
                string strType = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "b",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strType = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strType) == false)
                {
                    DomUtil.SetAttr(nodeOverdue, "type", strType);
                }

                // 2007/9/27 new add
                DomUtil.SetAttr(nodeOverdue, "id", "upgrade-" + Guid.NewGuid().ToString()/*this.GetOverdueID()*/);   // 2008/2/8 new add "upgrade-"
            }

            return 0;
        }


        // 创建<reservations>节点的下级内容
        // 待做内容：
        // 1)如果实体库已经存在，这里需要增加相关操作实体库的代码。
        // 也可以专门用一个读者记录和实体记录对照修改的阶段，来处理相互的关系
        // 2)暂时没有处理已到的预约册的信息升级功能，而是丢弃了这些信息
        static int CreateReservationsNode(XmlNode nodeReservations,
            string strField984,
            // int nEntityBarcodeLength,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField984,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                /*
                // return:
                //      -1  error
                //      0   OK
                //      1   Invalid
                nRet = VerifyBarcode(
                    false,
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet != 0)
                {
                    strWarning += "984字段中 册条码 '" + strBarcode + "' 不合法 -- " + strError + "; ";
                }
                 * */
                strBarcode = strBarcode.ToUpper();  // 2008/10/24 new add

                string strArriveDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "c",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strArriveDate = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strArriveDate) == false)
                    continue; // 如果已经到书，这个子字段组就忽略了

                XmlNode nodeRequest = nodeReservations.OwnerDocument.CreateElement("request");
                nodeRequest = nodeReservations.AppendChild(nodeRequest);

                DomUtil.SetAttr(nodeRequest, "items", strBarcode);

                // requestDate属性
                string strRequestDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
    ItemType.Group,
    "b",
    0,
    out strSubfield,
    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strRequestDate = strSubfield.Substring(1);

                    if (strRequestDate.Length != 8)
                    {
                        strWarning += "984$b子字段内容 '" + strRequestDate + "' 的长度不是8字符; ";
                    }
                }

                if (String.IsNullOrEmpty(strRequestDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strRequestDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "984字段中$b日期字符串转换格式为rfc1123时发生错误: " + strError;
                        strRequestDate = "";
                    }
                    else
                    {
                        strRequestDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strRequestDate) == false)
                {
                    DomUtil.SetAttr(nodeRequest, "requestDate", strRequestDate);
                }

            }

            return 0;
        }

        // 创建<borrowHistory>节点的下级内容
        static int CreateBorrowHistoryNode(XmlNode nodeBorrowHistory,
            string strField989,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            int nRet = 0;

            XmlNode nodePrev = null;    // 插入参考节点
                                        // 进行子字段组循环
            for (int g = 0; ; g++)
            {
                string strGroup = "";
                // 从字段中得到子字段组
                // parameters:
                //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
                //		nIndex	子字段组序号。从0开始计数。
                //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
                // return:
                //		-1	出错
                //		0	所指定的子字段组没有找到
                //		1	找到。找到的子字段组返回在strGroup参数中
                nRet = MarcUtil.GetGroup(strField989,
                    g,
                    out strGroup);
                if (nRet == -1)
                {
                    strError = "从MARC记录字段中获得子字段组 " + Convert.ToString(g) + " 时出错";
                    return -1;
                }

                if (nRet == 0)
                    break;

                string strSubfield = "";
                string strNextSubfieldName = "";

                string strBarcode = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "a",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBarcode = strSubfield.Substring(1);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;


                strBarcode = strBarcode.ToUpper();

                XmlNode nodeBorrow = nodeBorrowHistory.OwnerDocument.CreateElement("borrow");

                // If refChild is a null reference (Nothing in Visual Basic), insert newChild at the end of the list of child nodes
                nodeBorrow = nodeBorrowHistory.InsertBefore(nodeBorrow, nodePrev);
                nodePrev = nodeBorrow;

                // 删除超过100个的子节点
                if (nodeBorrowHistory.ChildNodes.Count > 100)
                {
                    XmlNode temp = nodeBorrowHistory.ChildNodes[nodeBorrowHistory.ChildNodes.Count - 1];
                    nodeBorrowHistory.RemoveChild(temp);
                }

                DomUtil.SetAttr(nodeBorrow, "barcode", strBarcode);

                // borrowDate属性
                string strBorrowDate = "";
                nRet = MarcUtil.GetSubfield(strGroup,
                    ItemType.Group,
                    "t",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (strSubfield.Length >= 1)
                {
                    strBorrowDate = strSubfield.Substring(1);

                    if (strBorrowDate.Length != 8)
                    {
                        strBorrowDate = "";
                    }
                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    string strTarget = "";
                    nRet = DateTimeUtil.Date8toRfc1123(strBorrowDate,
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        strBorrowDate = "";
                    }
                    else
                    {
                        strBorrowDate = strTarget;
                    }

                }

                if (String.IsNullOrEmpty(strBorrowDate) == false)
                {
                    DomUtil.SetAttr(nodeBorrow, "borrowDate", strBorrowDate);
                }
            }

            /*
            // delete more than 100
            if (nodeBorrowHistory.ChildNodes.Count > 100)
            {
                XmlNodeList nodes = nodeBorrowHistory.SelectNodes("borrow");
                for (int i = 100; i < nodes.Count; i++)
                {
                    nodeBorrowHistory.RemoveChild(nodes[i]);
                }
            }*/

            return 0;
        }


        #endregion

        private async void toolStripMenuItem_registerFaceByFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "请指定人脸图像文件名";
            // dlg.FileName = this.textBox_filename.Text;

            dlg.Filter = "图像文件 (*.bmp;*.jpg;*.gif;*.png)|*.bmp;*.jpg;*.gif;*.png|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            var result = await RegisterFaceAsync(dlg.FileName);
            if (result.Value == -1)
                ShowMessageBox(result.ErrorInfo);
        }

        // 立刻发出超期通知
        private void toolStripMenuItem_notifyOverdue_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.120") < 0)
            {
                strError = $"触发超期通知功能必须和 dp2library 3.120 或以上版本配套使用";
                goto ERROR1;
            }

            string message_type_list = "mq";
            using (GetMessageTypeDialog dlg = new GetMessageTypeDialog())
            {
                dlg.Font = this.Font;
                dlg.TypeList = Program.MainForm.AppInfo.GetString("readerinfoform", "messageTypeList", "mq");
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                message_type_list = dlg.TypeList;
                Program.MainForm.AppInfo.SetString("readerinfoform", "messageTypeList", dlg.TypeList);
            }

            string strRecPath = this.readerEditControl1.RecPath;

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial($"正在针对读者记录 {strRecPath} 触发超期通知 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, $"正在针对读者记录 {strRecPath} 触发超期通知 ...");

            EnableControls(false);

            try
            {
                long lRet = channel.SetReaderInfo(
    looping.Progress,
    "notifyOverdue",
    strRecPath,
    $"bodytypes:{message_type_list.Replace(",", "|")}", // strNewXml,
    "",
    null,
    out string strExistingXml,
    out string strSavedXml,
    out string strSavedPath,
    out byte[] baNewTimestamp,
    out ErrorCodeValue kernel_errorcode,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                EnableControls(true);

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);

                this.ReturnChannel(channel);
            }
            MessageBox.Show(this, $"触发超期通知成功。{strError}");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 立即发出召回通知
        private void toolStripMenuItem_notifyRecall_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.120") < 0)
            {
                strError = $"触发召回通知功能必须和 dp2library 3.120 或以上版本配套使用";
                goto ERROR1;
            }

            var reason = InputDlg.GetInput(this,
                "请输入召回事由描述",
                "事由描述:",
                "毕业手续需要",
                this.Font);
            if (reason == null)
                return;

            string message_type_list = "mq";
            using (GetMessageTypeDialog dlg = new GetMessageTypeDialog())
            {
                dlg.Font = this.Font;
                dlg.TypeList = Program.MainForm.AppInfo.GetString("readerinfoform", "messageTypeList", "mq");
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                message_type_list = dlg.TypeList;
                Program.MainForm.AppInfo.SetString("readerinfoform", "messageTypeList", dlg.TypeList);
            }

            string strRecPath = this.readerEditControl1.RecPath;

            LibraryChannel channel = this.GetChannel();

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial($"正在针对读者记录 {strRecPath} 触发召回通知 ...");
            _stop.BeginLoop();
            */
            var looping = BeginLoop(this.DoStop, $"正在针对读者记录 {strRecPath} 触发召回通知 ...");

            EnableControls(false);

            try
            {
                long lRet = channel.SetReaderInfo(
    looping.Progress,
    "notifyRecall",
    strRecPath,
    $"bodytypes:{message_type_list.Replace(",", "|")},reason:{StringUtil.EscapeString(reason, ":,")}", // strNewXml,
    "",
    null,
    out string strExistingXml,
    out string strSavedXml,
    out string strSavedPath,
    out byte[] baNewTimestamp,
    out ErrorCodeValue kernel_errorcode,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                EnableControls(true);

                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
                EndLoop(looping);

                this.ReturnChannel(channel);
            }
            MessageBox.Show(this, $"触发召回成功。{strError}");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        string GetPrevNextRecPath(string strStyle)
        {
            var form = Program.MainForm.GetTopChildWindow<ReaderSearchForm>();
            if (form == null)
                return "";

            // REDO:
            ListViewItem item = ReaderSearchForm.MoveSelectedItem(form.ListViewRecords, strStyle);
            if (item == null)
                return "";
            string text = ListViewUtil.GetItemText(item, 0);
            /*
            // 遇到 Z39.50 命令行，要跳过去
            if (ReaderSearchForm.IsCmdLine(text))
                goto REDO;
            */
            return text;
        }

        private void readerEditControl1_EditEmail(object sender, EventArgs e)
        {
            using (PropertyTableDialog dlg = new PropertyTableDialog())
            {
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.Text = "当前读者的 Email 等";
                dlg.PropertyNameList = new List<string> {
                    "email",
                    "weixinid",
                    "" // 空字符串表示允许默认参数名。那么此处默认 email (第一个元素)
                };
                dlg.PropertyString = this.readerEditControl1.Email;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.OK)
                    return;
                this.readerEditControl1.Email = dlg.PropertyString;
            }
        }

        private void readerEditControl1_EditCardNumber(object sender, EventArgs e)
        {
            toolStripMenuItem_bindCardNumber_Click(sender, e);
        }

        // 普通删除记录
        private void toolStripSplitButton_delete_ButtonClick(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_DELETE_RECORD);
        }

        // 强制删除记录
        private void ToolStripMenuItem_forceDelete_Click(object sender, EventArgs e)
        {
            EnableToolStrip(false);

            this.m_webExternalHost.StopPrevious();
            this.webBrowser_readerInfo.Stop();

            this.commander.AddMessage(WM_FORCE_DELETE_RECORD);
        }
    }
}