using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Web;
using System.IO;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.CommonDialog;
using DigitalPlatform.Drawing;

namespace dp2Circulation
{
    /// <summary>
    /// 册窗 / 订购窗 / 期窗 / 评注窗
    /// </summary>
    public partial class ItemInfoForm : MyForm
    {
        // 
        /// <summary>
        /// 数据库类型
        /// </summary>
        string m_strDbType = "item";  // comment order issue

        /// <summary>
        /// 数据库类型。为 item / order / issue / comment 之一
        /// </summary>
        public string DbType
        {
            get
            {
                return this.m_strDbType;
            }
            set
            {
                this.m_strDbType = value;

                if (this.m_strDbType == "comment")
                    this.toolStripButton_addSubject.Visible = true;
                else
                    this.toolStripButton_addSubject.Visible = false;

                this.Text = this.DbTypeCaption;
                this.comboBox_from.Items.Clear();   // 促使更换
            }
        }

        /// <summary>
        /// 当前已经装载的记录路径
        /// </summary>
        public string ItemRecPath { get; set; } // 当前已经装载的册记录路径

        /// <summary>
        /// 当前已装载的书目记录路径
        /// </summary>
        public string BiblioRecPath { get; set; }   // 当前已装载的书目记录路径

#if NO
        string _xml = "";

        /// <summary>
        /// 记录 XML
        /// </summary>
        public string Xml
        {
            get
            {
                return this._xml;
            }
            set
            {
                this._xml = value;

                this.ItemXmlChanged = true;
                /*
SetXmlToWebbrowser(this.webBrowser_itemXml,
    strItemText);
 * */
                // 把 XML 字符串装入一个Web浏览器控件
                // 这个函数能够适应"<root ... />"这样的没有prolog的XML内容
                Global.SetXmlToWebbrowser(this.webBrowser_itemXml,
                    Program.MainForm.DataDir,
                    "xml",
                    value);

                SetItemRefID(value);
            }
        }
#endif
        /// <summary>
        /// 记录 XML
        /// </summary>
        public string Xml
        {
            get
            {
                return this.textBox_editor.Text;
            }
            set
            {
                // 2020/8/23
                if (string.IsNullOrEmpty(value) == false)
                {
                    int nRet = DomUtil.GetIndentXml(value,
        true,
        out string strXml,
        out string strError);
                    if (nRet != -1)
                        value = strXml;
                }

                this.textBox_editor.Text = value;

                this.ItemXmlChanged = true;
                /*
SetXmlToWebbrowser(this.webBrowser_itemXml,
    strItemText);
 * */
                // 把 XML 字符串装入一个Web浏览器控件
                // 这个函数能够适应"<root ... />"这样的没有prolog的XML内容
                Global.SetXmlToWebbrowser(this.webBrowser_itemXml,
                    Program.MainForm.DataDir,
                    "xml",
                    value);

                SetItemRefID(value);
            }
        }

        bool _itemXmlChanged = false;
        public bool ItemXmlChanged
        {
            get
            {
                return _itemXmlChanged;
            }
            set
            {
                _itemXmlChanged = value;

                this.toolStripButton_save.Enabled = _itemXmlChanged || this.ObjectChanged;
            }
        }

        /// <summary>
        /// 记录时间戳
        /// </summary>
        public byte[] Timestamp { get; set; }


        const int WM_LOAD_RECORD = API.WM_USER + 200;
        const int WM_PREV_RECORD = API.WM_USER + 201;
        const int WM_NEXT_RECORD = API.WM_USER + 202;

        Commander commander = null;
        WebExternalHost m_webExternalHost_item = new WebExternalHost();
        WebExternalHost m_chargingInterface = new WebExternalHost();
        WebExternalHost m_webExternalHost_biblio = new WebExternalHost();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ItemInfoForm()
        {
            this.UseLooping = true; // 2022/11/2

            InitializeComponent();
        }

        private void ItemInfoForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
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
            {
                //
                this.binaryResControl1.ContentChanged -= new ContentChangedEventHandler(binaryResControl1_ContentChanged);
                this.binaryResControl1.ContentChanged += new ContentChangedEventHandler(binaryResControl1_ContentChanged);

                this.binaryResControl1.GetChannel -= binaryResControl1_GetChannel;
                this.binaryResControl1.GetChannel += binaryResControl1_GetChannel;

                this.binaryResControl1.ReturnChannel -= binaryResControl1_ReturnChannel;
                this.binaryResControl1.ReturnChannel += binaryResControl1_ReturnChannel;

                // this.binaryResControl1.Stop = this._stop;
            }

            // webbrowser
            this.m_webExternalHost_item.Initial(// Program.MainForm, 
                this.webBrowser_itemHTML);
            this.webBrowser_itemHTML.ObjectForScripting = this.m_webExternalHost_item;

            this.m_chargingInterface.Initial(// Program.MainForm, 
                this.webBrowser_borrowHistory);
            this.m_chargingInterface.CallFunc += m_chargingInterface_CallFunc;
            this.webBrowser_borrowHistory.ObjectForScripting = this.m_chargingInterface;

            this.m_webExternalHost_biblio.Initial(// Program.MainForm,
                this.webBrowser_biblio);
            this.webBrowser_biblio.ObjectForScripting = this.m_webExternalHost_biblio;

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            this.Text = this.DbTypeCaption;

            ClearBorrowHistoryPage();
        }

        void binaryResControl1_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            // this.toolStripButton_save.Enabled = e.CurrentChanged;
            this.toolStripButton_save.Enabled = _itemXmlChanged || e.CurrentChanged;
        }

        void binaryResControl1_ReturnChannel(object sender, ReturnChannelEventArgs e)
        {
            /*
            this._stop.EndLoop();
            this._stop.OnStop -= new StopEventHandler(this.DoStop);
            this.ReturnChannel(e.Channel);
            */
            OnReturnChannel(sender, e);
        }

        void binaryResControl1_GetChannel(object sender, GetChannelEventArgs e)
        {
            /*
            e.Channel = this.GetChannel();
            this._stop.OnStop += new StopEventHandler(this.DoStop);
            this._stop.BeginLoop();
            */
            OnGetChannel(sender, e);
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

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost_item.ChannelInUse || this.m_webExternalHost_biblio.ChannelInUse;
        }

        private void ItemInfoForm_FormClosing(object sender, FormClosingEventArgs e)
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
            if (this.ItemXmlChanged == true ||
                this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有信息被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "ItemInfoForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void ItemInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.commander.Destroy();

            if (this.m_webExternalHost_item != null)
                this.m_webExternalHost_item.Destroy();
            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Destroy();
            if (this.m_chargingInterface != null)
                this.m_chargingInterface.Destroy();
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
            MainForm.AppInfo.SaveMdiChildFormStates(this,
   "mdi_form_state");
#endif
        }

        /*
        void SetXmlToWebbrowser(WebBrowser webbrowser,
            string strXml)
        {
            string strTargetFileName = MainForm.DataDir + "\\xml.xml";

            StreamWriter sw = new StreamWriter(strTargetFileName,
                false,	// append
                System.Text.Encoding.UTF8);
            sw.Write(strXml);
            sw.Close();

            webbrowser.Navigate(strTargetFileName);
        }
         * */

        /// <summary>
        /// 重新装载当前记录
        /// </summary>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int Reload()
        {
            return LoadRecordByRecPath(this.ItemRecPath, "");
        }

        // 
        /// <summary>
        /// 根据册条码号，装入册记录和书目记录
        /// 本方式只能当 DbType 为 "item" 时调用
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int LoadRecord(string strItemBarcode)
        {
            Debug.Assert(this.m_strDbType == "item", "");

            string strError = "";

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在装载册信息 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在装载册信息 ...",
                "disableControl");

            Global.ClearHtmlPage(this.webBrowser_itemHTML,
                Program.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_itemXml,
                Program.MainForm.DataDir);
            Global.ClearHtmlPage(this.webBrowser_biblio,
                Program.MainForm.DataDir);

            ClearBorrowHistoryPage();
            SetItemRefID("");

            // this.textBox_message.Text = "";
            this.toolStripLabel_message.Text = "";

            looping.Progress.SetMessage("正在装入册记录 " + strItemBarcode + " ...");
            try
            {
                string strItemText = "";
                string strBiblioText = "";

                string strItemRecPath = "";
                string strBiblioRecPath = "";

                byte[] item_timestamp = null;

                long lRet = channel.GetItemInfo(
                    looping.Progress,
                    strItemBarcode,
                    "html",
                    out strItemText,
                    out strItemRecPath,
                    out item_timestamp,
                    "html",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1 || lRet == 0)
                    goto ERROR1;

                this.ItemRecPath = strItemRecPath;    // 2009/10/18
                this.BiblioRecPath = strBiblioRecPath;  // 2013/3/4

                if (lRet > 1)
                {
                    this.textBox_queryWord.Text = strItemBarcode;
                    this.comboBox_from.Text = "册条码";

                    strError = "册条码号 '" + strItemBarcode + "' 检索命中" + lRet.ToString() + " 条册记录，它们的路径如下：" + strItemRecPath + "；装入操作被放弃。\r\n\r\n这是一个严重的错误，请尽快联系系统管理员解决此问题。\r\n\r\n如要装入其中的任何一条，请采用记录路径方式装入。";
                    goto ERROR1;
                }

#if NO
                Global.SetHtmlString(this.webBrowser_itemHTML,
                    strItemText,
                    Program.MainForm.DataDir,
                    "iteminfoform_item");
#endif
                this.m_webExternalHost_item.SetHtmlString(strItemText,
                    "iteminfoform_item");

                if (String.IsNullOrEmpty(strBiblioText) == true)
                    Global.SetHtmlString(this.webBrowser_biblio,
                        "(书目记录 '" + strBiblioRecPath + "' 不存在)");
                else
                {
#if NO
                    Global.SetHtmlString(this.webBrowser_biblio,
                        strBiblioText,
                        Program.MainForm.DataDir,
                        "iteminfoform_biblio");
#endif
                    this.m_webExternalHost_biblio.SetHtmlString(strBiblioText,
                        "iteminfoform_biblio");
                }

                // this.textBox_message.Text = "册记录路径: " + strItemRecPath + " ；其从属的种(书目)记录路径: " + strBiblioRecPath;
                this.toolStripLabel_message.Text = this.DbTypeCaption + "记录路径: " + strItemRecPath + " ；其从属的种(书目)记录路径: " + strBiblioRecPath;

                this.textBox_queryWord.Text = strItemBarcode;
                this.comboBox_from.Text = "册条码号";

                // 最后获得item xml
                lRet = channel.GetItemInfo(
                    looping.Progress,
                    strItemBarcode,
                    "xml",
                    out strItemText,
                    out strItemRecPath,
                    out item_timestamp,
                    null,   // "html",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    Global.SetHtmlString(this.webBrowser_itemXml,
                        HttpUtility.HtmlEncode(strError));
                }
                else
                {
                    this.Xml = strItemText;
                    this.Timestamp = item_timestamp;

                    this.ItemXmlChanged = false;

                    // 接着装入对象资源
                    {
                        this.binaryResControl1.Clear();
                        int nRet = this.binaryResControl1.LoadObject(
                            looping.Progress,
                            channel,
                            strItemRecPath,
                            this.Xml,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }
                    }
                }
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */

                this.textBox_queryWord.SelectAll();
                this.textBox_queryWord.Focus();
            }

            tabControl_item_SelectedIndexChanged(this, new EventArgs());
            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        /// <summary>
        /// 数据库类型的显示名称
        /// </summary>
        public string DbTypeCaption
        {
            get
            {
                if (this.m_strDbType == "item")
                    return "册";
                else if (this.m_strDbType == "comment")
                    return "评注";
                else if (this.m_strDbType == "order")
                    return "订购";
                else if (this.m_strDbType == "issue")
                    return "期";
                else
                    throw new Exception("未知的DbType '" + this.m_strDbType + "'");
            }
        }

        // 
        /// <summary>
        /// 根据册/订购/期/评注记录路径，装入事项记录和书目记录
        /// </summary>
        /// <param name="strItemRecPath">事项记录路径</param>
        /// <param name="strPrevNextStyle">前后翻动风格</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int LoadRecordByRecPath(string strItemRecPath,
            string strPrevNextStyle)
        {
            string strError = "";

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在初始化浏览器组件 ...");
            _stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                null,
                "disableControl");

            bool bPrevNext = false;

            string strRecPath = strItemRecPath;

            // 2009/10/18
            if (String.IsNullOrEmpty(strPrevNextStyle) == false)
            {
                strRecPath += "$" + strPrevNextStyle.ToLower();
                bPrevNext = true;
            }

            if (bPrevNext == false)
            {
                Global.ClearHtmlPage(this.webBrowser_itemHTML,
                    Program.MainForm.DataDir);
                Global.ClearHtmlPage(this.webBrowser_itemXml,
                    Program.MainForm.DataDir);
                Global.ClearHtmlPage(this.webBrowser_biblio,
                    Program.MainForm.DataDir);

                // this.textBox_message.Text = "";
                this.toolStripLabel_message.Text = "";
            }

            ClearBorrowHistoryPage();
            SetItemRefID("");

            looping.Progress.SetMessage("正在装入" + this.DbTypeCaption + "记录 " + strItemRecPath + " ...");
            try
            {
                string strItemText = "";
                string strBiblioText = "";

                string strOutputItemRecPath = "";
                string strBiblioRecPath = "";

                byte[] item_timestamp = null;

                string strBarcode = "@path:" + strRecPath;

                long lRet = 0;

                if (this.m_strDbType == "item")
                    lRet = channel.GetItemInfo(
                         looping.Progress,
                         strBarcode,
                         "html",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "html",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else if (this.m_strDbType == "comment")
                    lRet = channel.GetCommentInfo(
                         looping.Progress,
                         strBarcode,    // "@path:" + strItemRecPath,
                                        // "",
                         "html",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "html",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else if (this.m_strDbType == "order")
                    lRet = channel.GetOrderInfo(
                         looping.Progress,
                         strBarcode,    // "@path:" + strItemRecPath,
                                        // "",
                         "html",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "html",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else if (this.m_strDbType == "issue")
                    lRet = channel.GetIssueInfo(
                         looping.Progress,
                         strBarcode,    // "@path:" + strItemRecPath,
                                        // "",
                         "html",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "html",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else if (this.m_strDbType == "arrive")
                {

                }
                else
                    throw new Exception("未知的DbType '" + this.m_strDbType + "'");

                if (lRet == -1 || lRet == 0)
                {
                    if (bPrevNext == true
                        && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                    {
                        strError += "\r\n\r\n新记录没有装载，窗口中还保留了装载前的记录";
                        goto ERROR1;
                    }


                    this.ItemRecPath = strOutputItemRecPath;    // 2011/9/5
                    this.BiblioRecPath = strBiblioRecPath;  // 2013/3/4

                    this.m_webExternalHost_item.SetHtmlString(strError,
    "iteminfoform_item");
                }
                else
                {
                    this.ItemRecPath = strOutputItemRecPath;    // 2009/10/18
                    this.BiblioRecPath = strBiblioRecPath;  // 2013/3/4

                    this.m_webExternalHost_item.SetHtmlString(strItemText,
                        "iteminfoform_item");
                }

                if (String.IsNullOrEmpty(strBiblioText) == true)
                    Global.SetHtmlString(this.webBrowser_biblio,
                        "(书目记录 '" + strBiblioRecPath + "' 不存在)");
                else
                {
                    this.m_webExternalHost_biblio.SetHtmlString(strBiblioText,
                        "iteminfoform_biblio");
                }

                // this.textBox_message.Text = "册记录路径: " + strOutputItemRecPath + " ；其从属的种(书目)记录路径: " + strBiblioRecPath;
                this.toolStripLabel_message.Text = this.DbTypeCaption + "记录路径: " + strOutputItemRecPath + " ；其从属的种(书目)记录路径: " + strBiblioRecPath;
                this.textBox_queryWord.Text = this.ItemRecPath; // strItemRecPath;
                this.comboBox_from.Text = this.DbTypeCaption + "记录路径";

                // 最后获得item xml
                if (this.m_strDbType == "item")
                    lRet = channel.GetItemInfo(
                        looping.Progress,
                        "@path:" + strOutputItemRecPath, // strBarcode,
                        "xml",
                        out strItemText,
                        out strItemRecPath,
                        out item_timestamp,
                        null,   // "html",
                        out strBiblioText,
                        out strBiblioRecPath,
                        out strError);
                else if (this.m_strDbType == "comment")
                    lRet = channel.GetCommentInfo(
                         looping.Progress,
                         "@path:" + strOutputItemRecPath,
                         // "",
                         "xml",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else if (this.m_strDbType == "order")
                    lRet = channel.GetOrderInfo(
                         looping.Progress,
                         "@path:" + strOutputItemRecPath,
                         // "",
                         "xml",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else if (this.m_strDbType == "issue")
                    lRet = channel.GetIssueInfo(
                         looping.Progress,
                         "@path:" + strOutputItemRecPath,
                         // "",
                         "xml",
                         out strItemText,
                         out strOutputItemRecPath,
                         out item_timestamp,
                         "",
                         out strBiblioText,
                         out strBiblioRecPath,
                         out strError);
                else
                    throw new Exception("未知的DbType '" + this.m_strDbType + "'");

                if (lRet == -1 || lRet == 0)
                {
                    Global.SetHtmlString(this.webBrowser_itemXml,
                        HttpUtility.HtmlEncode(strError));
                }
                else
                {
                    this.Xml = strItemText;
                    this.Timestamp = item_timestamp;

                    this.ItemXmlChanged = false;
#if NO
                    /*
                    SetXmlToWebbrowser(this.webBrowser_itemXml,
                        strItemText);
                     * */
                    // 把 XML 字符串装入一个Web浏览器控件
                    // 这个函数能够适应"<root ... />"这样的没有prolog的XML内容
                    Global.SetXmlToWebbrowser(this.webBrowser_itemXml,
                        Program.MainForm.DataDir,
                        "xml",
                        strItemText);

                    SetItemRefID(strItemText);
#endif

                    // 接着装入对象资源
                    {
                        this.binaryResControl1.Clear();
                        int nRet = this.binaryResControl1.LoadObject(
                            looping.Progress,
                            channel,
                            strOutputItemRecPath,
                            this.Xml,
                            Program.MainForm.ServerVersion,
                            out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, strError);
                        }
                    }
                }
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */
            }

            tabControl_item_SelectedIndexChanged(this, new EventArgs());
            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // parameters:
        //      strStyle    force/空
        void SaveRecord(string strStyle)
        {
            string strError = "";

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存册记录 " + this.ItemRecPath + " ...");
            _stop.BeginLoop();

            EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在保存册记录 " + this.ItemRecPath + " ...",
                "disableControl");
            try
            {
                int nRet = SaveItemInfo(
                    looping.Progress,
                    channel,
                    strStyle,
                    this.DbType,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 保存对象
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
                    goto ERROR1;

                // 重新装载记录
                nRet = LoadRecordByRecPath(this.ItemRecPath, "");
                return;
            }
            finally
            {
                looping.Dispose();
                /*
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得记录的XML格式
        // parameters:
        //      bIncludeFileID  是否要根据当前rescontrol内容合成<dprms:file>元素?
        //      bClearFileID    是否要清除以前的<dprms:file>元素
        int GetXml(
            bool bIncludeFileID,
            bool bClearFileID,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.Xml);
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

        public int SaveItemInfo(
            Stop stop,
            LibraryChannel channel,
            string strStyle,
            string strDbType,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<EntityInfo> entityArray = new List<EntityInfo>();

            {
                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(this._refID) == true)
                {
                    this._refID = Guid.NewGuid().ToString();
                }

                info.RefID = this._refID;

                nRet = GetXml(true,
            false,
            out string strXml,
            out strError);
                if (nRet == -1)
                    return -1;

                info.OldRecPath = this.ItemRecPath;
                if (StringUtil.IsInList("force", strStyle) == true)
                {
                    info.Action = "forcechange";
                    info.Style = "nocheckdup";
                }
                else
                    info.Action = "change";
                info.NewRecPath = this.ItemRecPath;

                info.NewRecord = strXml;
                info.NewTimestamp = null;

                info.OldRecord = this.Xml;
                info.OldTimestamp = this.Timestamp;

                entityArray.Add(info);
            }

            // 复制到目标
            EntityInfo[] entities = entityArray.ToArray();
            EntityInfo[] errorinfos = null;

            long lRet = 0;

            if (strDbType == "item")
                lRet = channel.SetEntities(
                     stop,
                     this.BiblioRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else if (strDbType == "order")
                lRet = channel.SetOrders(
                     stop,
                     this.BiblioRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else if (strDbType == "issue")
                lRet = channel.SetIssues(
                     stop,
                     this.BiblioRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else if (strDbType == "comment")
                lRet = channel.SetComments(
                     stop,
                     this.BiblioRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else
            {
                strError = "未知的 strDbType '" + strDbType + "'";
                return -1;
            }
            if (lRet == -1)
                return -1;

            if (errorinfos == null)
                return 0;

            strError = "";
            foreach (EntityInfo error in errorinfos)
            {
                if (String.IsNullOrEmpty(error.RefID) == true)
                {
                    strError = "服务器返回的EntityInfo结构中RefID为空";
                    return -1;
                }

                this.Timestamp = error.NewTimestamp;

                // 正常信息处理
                if (error.ErrorCode == ErrorCodeValue.NoError)
                    continue;

                strError += error.RefID + "在提交保存过程中发生错误 -- " + error.ErrorInfo + "\r\n";
            }

            if (String.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }

        string _itemBarcode = "";
        string _refID = "";

        // 设置当前记录的唯一标识
        void SetItemRefID(string strXml)
        {
            _itemBarcode = "";
            _refID = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return; // 起到清除的作用

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
                _itemBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                _refID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            }
            catch
            {

            }
        }
#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        void SetMenuItemState()
        {
            // 菜单

            // 工具条按钮

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

            Program.MainForm.toolButton_refresh.Enabled = true;
        }

        private void ItemInfoForm_Activated(object sender, EventArgs e)
        {
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */

            SetMenuItemState();
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOAD_RECORD:
                    this.toolStrip1.Enabled = false;
                    try
                    {
                        if (this.m_webExternalHost_item.CanCallNew(
                            this.commander,
                            m.Msg) == true
                            && this.m_webExternalHost_biblio.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            DoLoadRecord();
                        }
                    }
                    finally
                    {
                        this.toolStrip1.Enabled = true;
                    }
                    return;
                case WM_PREV_RECORD:
                    this.toolStrip1.Enabled = false;
                    try
                    {
                        if (this.m_webExternalHost_item.CanCallNew(
                            this.commander,
                            m.Msg) == true
                            && this.m_webExternalHost_biblio.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            LoadRecordByRecPath(this.ItemRecPath, "prev");
                        }
                    }
                    finally
                    {
                        this.toolStrip1.Enabled = true;
                    }
                    return;
                case WM_NEXT_RECORD:
                    this.toolStrip1.Enabled = false;
                    try
                    {
                        if (this.m_webExternalHost_item.CanCallNew(
                            this.commander,
                            m.Msg) == true
                            && this.m_webExternalHost_biblio.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            LoadRecordByRecPath(this.ItemRecPath, "next");
                        }
                    }
                    finally
                    {
                        this.toolStrip1.Enabled = true;
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void button_load_Click(object sender, EventArgs e)
        {
            if (this.textBox_queryWord.Text == "")
            {
                MessageBox.Show(this, "尚未输入检索词");
                return;
            }

            this.toolStrip1.Enabled = false;
            this.button_load.Enabled = false;

            this.m_webExternalHost_item.StopPrevious();
            this.webBrowser_itemHTML.Stop();

            this.m_webExternalHost_biblio.StopPrevious();
            this.webBrowser_biblio.Stop();

            this.commander.AddMessage(WM_LOAD_RECORD);
        }

        private void DoLoadRecord()
        {
            string strError;
            if (this.textBox_queryWord.Text == "")
            {
                strError = "尚未输入检索词";
                goto ERROR1;
            }

            if (this.comboBox_from.Text == "册条码"
                || this.comboBox_from.Text == "册条码号")
            {
                if (this.m_strDbType != "item")
                {
                    strError = "只能当DbType为item时才能使用 册条码号 检索途径";
                    goto ERROR1;
                }
                int nRet = this.textBox_queryWord.Text.IndexOf("/");
                if (nRet != -1)
                {
                    strError = "您输入的检索词似乎为一个记录路径，而不是册条码号";
                    MessageBox.Show(this, strError);
                }

                LoadRecord(this.textBox_queryWord.Text);
            }
            else if (this.comboBox_from.Text == this.DbTypeCaption + "记录路径")
            {
                int nRet = this.textBox_queryWord.Text.IndexOf("/");
                if (nRet == -1)
                {
                    strError = "您输入的检索词似乎为一个册条码号，而不是" + this.DbTypeCaption + "记录路径";
                    MessageBox.Show(this, strError);
                }

                // LoadRecord("@path:" + this.textBox_queryWord.Text);
                LoadRecordByRecPath(this.textBox_queryWord.Text, "");
            }
            else
            {
                strError = "无法识别的检索途径 '" + this.comboBox_from.Text + "'";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                this.comboBox_from.Enabled = bEnable;
                this.textBox_queryWord.Enabled = bEnable;
                this.button_load.Enabled = bEnable;
                this.toolStrip1.Enabled = bEnable;  // 避免使用工具条上的命令按钮
            }));
        }

        private void toolStripButton_prevRecord_Click(object sender, EventArgs e)
        {
            this.toolStrip1.Enabled = false;
            this.button_load.Enabled = false;

            this.m_webExternalHost_item.StopPrevious();
            this.webBrowser_itemHTML.Stop();

            this.m_webExternalHost_biblio.StopPrevious();
            this.webBrowser_biblio.Stop();

            this.commander.AddMessage(WM_PREV_RECORD);
        }

        private void toolStripButton_nextRecord_Click(object sender, EventArgs e)
        {
            this.toolStrip1.Enabled = false;
            this.button_load.Enabled = false;

            this.m_webExternalHost_item.StopPrevious();
            this.webBrowser_itemHTML.Stop();

            this.m_webExternalHost_biblio.StopPrevious();
            this.webBrowser_biblio.Stop();

            this.commander.AddMessage(WM_NEXT_RECORD);

        }

        private void comboBox_from_DropDown(object sender, EventArgs e)
        {
            this.comboBox_from.Items.Clear();

            if (this.m_strDbType == "item")
                this.comboBox_from.Items.Add("册条码号");

            this.comboBox_from.Items.Add(this.DbTypeCaption + "记录路径");
        }

        // 增添自由词
        private void toolStripButton_addSubject_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获取书目记录 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取书目记录 ...",
                "disableControl");

            try
            {
                List<string> reserve_subjects = null;
                List<string> exist_subjects = null;
                byte[] biblio_timestamp = null;
                string strBiblioXml = "";

                nRet = GetExistSubject(
                    looping.Progress,
                    channel,
                    this.BiblioRecPath,
                    out strBiblioXml,
                    out reserve_subjects,
                    out exist_subjects,
                    out biblio_timestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strCommentState = "";
                string strNewSubject = "";
                byte[] item_timestamp = null;
                nRet = GetCommentContent(
                    looping.Progress,
                    channel,
                    this.ItemRecPath,
            out strNewSubject,
            out strCommentState,
            out item_timestamp,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                AddSubjectDialog dlg = new AddSubjectDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.ReserveSubjects = reserve_subjects;
                dlg.ExistSubjects = exist_subjects;
                dlg.HiddenNewSubjects = StringUtil.SplitList(strNewSubject.Replace("\\r", "\n"), '\n');
                if (StringUtil.IsInList("已处理", strCommentState) == false)
                    dlg.NewSubjects = dlg.HiddenNewSubjects;

                Program.MainForm.AppInfo.LinkFormState(dlg, "iteminfoform_addsubjectdialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;

                List<string> subjects = new List<string>();
                subjects.AddRange(dlg.ExistSubjects);
                subjects.AddRange(dlg.NewSubjects);

                StringUtil.RemoveDupNoSort(ref subjects);   // 去重
                StringUtil.RemoveBlank(ref subjects);   // 去掉空元素

                // 修改指示符1为空的那些 610 字段
                // parameters:
                //      strSubject  可以修改的自由词的总和。包括以前存在的和本次添加的
                nRet = ChangeSubject(ref strBiblioXml,
                    subjects,
                    out strError);

                // 保存书目记录
                byte[] output_timestamp = null;
                string strOutputBiblioRecPath = "";
                long lRet = channel.SetBiblioInfo(
                    looping.Progress,
                    "change",
                    this.BiblioRecPath,
                    "xml",
                    strBiblioXml,
                    biblio_timestamp,
                    "",
                    out strOutputBiblioRecPath,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 修改评注记录状态
                // return:
                //       -1  出错
                //      0   没有发生修改
                //      1   发生了修改
                nRet = ChangeCommentState(
                    looping.Progress,
                    channel,
                    this.BiblioRecPath,
                    this.ItemRecPath,
                    "已处理",
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */
            }

            // 重新装载内容
            this.Reload();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改评注的状态
        // return:
        //       -1  出错
        //      0   没有发生修改
        //      1   发生了修改
        /// <summary>
        /// 修改评注记录的状态字段
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="channel"></param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strCommentRecPath">评注记录路径</param>
        /// <param name="strAddList">要在状态字符串中加入的子串列表</param>
        /// <param name="strRemoveList">要在状态字符串中删除的子串列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///       -1  出错
        ///      0   没有发生修改
        ///      1   发生了修改
        /// </returns>
        public int ChangeCommentState(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strCommentRecPath,
            string strAddList,
            string strRemoveList,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strCommentRecPath) == true)
            {
                strError = "CommentRecPath为空";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                strError = "strBiblioRecPath为空";
                goto ERROR1;
            }

            // 获得旧记录
            string strOldXml = "";
            // byte[] timestamp = ByteArray.GetTimeStampByteArray(this.Timestamp);

            string strOutputPath = "";
            byte[] comment_timestamp = null;
            string strBiblio = "";
            string strTempBiblioRecPath = "";
            long lRet = channel.GetCommentInfo(
stop,
"@path:" + strCommentRecPath,
"xml", // strResultType
out strOldXml,
out strOutputPath,
out comment_timestamp,
"recpath",  // strBiblioType
out strBiblio,
out strTempBiblioRecPath,
out strError);
            if (lRet == -1)
            {
                strError = "获得原有评注记录 '" + strCommentRecPath + "' 时出错: " + strError;
                goto ERROR1;
            }

#if NO
            if (ByteArray.Compare(comment_timestamp, timestamp) != 0)
            {
                strError = "修改被拒绝。因为记录 '" + strCommentRecPath + "' 在保存前已经被其他人修改过。请重新装载";
                goto ERROR1;
            }
#endif


            XmlDocument dom = new XmlDocument();
            if (String.IsNullOrEmpty(strOldXml) == false)
            {
                try
                {
                    dom.LoadXml(strOldXml);
                }
                catch (Exception ex)
                {
                    strError = "装载记录XML进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }
            }
            else
                dom.LoadXml("<root/>");

            // 仅仅修改状态
            {
                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");
                string strOldState = strState;

                Global.ModifyStateString(ref strState,
    strAddList,
    strRemoveList);

                if (strState == strOldState)
                    return 0;   // 没有必要修改

                DomUtil.SetElementText(dom.DocumentElement,
                    "state", strState);

                // 在<operations>中写入适当条目
                string strComment = "'" + strOldState + "' --> '" + strState + "'";
                nRet = Global.SetOperation(
                    ref dom,
                    "stateModified",
                    channel.UserName,
                    strComment,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            string strNewCommentRecPath = "";
            string strNewXml = "";
            byte[] baNewTimestamp = null;

            {
                strNewCommentRecPath = strCommentRecPath;

                // 覆盖
                nRet = ChangeCommentInfo(
                    stop,
                    channel,
                    strBiblioRecPath,
                    strCommentRecPath,
                    strOldXml,
                    dom.DocumentElement.OuterXml,
                    comment_timestamp,
                    out strNewXml,
                    out baNewTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 
        /// <summary>
        /// 修改一个评注记录
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="channel"></param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strCommentRecPath">评注记录路径</param>
        /// <param name="strOldXml">评注记录修改前的 XML</param>
        /// <param name="strCommentXml">评注记录要修改成的 XML</param>
        /// <param name="timestamp">修改前的时间戳</param>
        /// <param name="strNewXml">返回实际保存成功的评注记录 XML</param>
        /// <param name="baNewTimestamp">返回修改后的时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int ChangeCommentInfo(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            string strCommentRecPath,
            string strOldXml,
            string strCommentXml,
            byte[] timestamp,
            out string strNewXml,
            out byte[] baNewTimestamp,
            out string strError)
        {
            strError = "";
            strNewXml = "";
            baNewTimestamp = null;

            EntityInfo info = new EntityInfo();
            info.RefID = Guid.NewGuid().ToString();

            string strTargetBiblioRecID = Global.GetRecordID(strBiblioRecPath);

            XmlDocument comment_dom = new XmlDocument();
            try
            {
                comment_dom.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(comment_dom.DocumentElement,
                "parent", strTargetBiblioRecID);

            info.Action = "change";
            info.OldRecPath = strCommentRecPath;
            info.NewRecPath = strCommentRecPath;
            info.OldRecord = strOldXml;
            info.OldTimestamp = timestamp;
            info.NewRecord = comment_dom.OuterXml;
            info.NewTimestamp = null;

            // 
            EntityInfo[] comments = new EntityInfo[1];
            comments[0] = info;

            EntityInfo[] errorinfos = null;

            long lRet = channel.SetComments(
                stop,
                strBiblioRecPath,
                comments,
                out errorinfos,
                out strError);
            if (lRet == -1)
            {
                return -1;
            }

            if (errorinfos != null && errorinfos.Length > 0)
            {
                int nErrorCount = 0;
                for (int i = 0; i < errorinfos.Length; i++)
                {
                    EntityInfo error = errorinfos[i];
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        if (String.IsNullOrEmpty(strError) == false)
                            strError += "; ";
                        strError += errorinfos[0].ErrorInfo;
                        nErrorCount++;
                    }
                    else
                    {
                        // strNewCommentRecPath = error.NewRecPath;
                        strNewXml = error.NewRecord;
                        baNewTimestamp = error.NewTimestamp;
                    }
                }
                if (nErrorCount > 0)
                {
                    return -1;
                }
            }

            return 1;
        }

        /// <summary>
        /// 获得评注记录内容
        /// 请参考 dp2Library API GetCommentInfo() 的详细信息
        /// </summary>
        /// <param name="stop"></param>
        /// <param name="channel"></param>
        /// <param name="strCommentRecPath">评注记录路径</param>
        /// <param name="strContent">返回内容</param>
        /// <param name="strState">返回状态</param>
        /// <param name="item_timestamp">返回时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        int GetCommentContent(
            Stop stop,
            LibraryChannel channel,
            string strCommentRecPath,
            out string strContent,
            out string strState,
            out byte[] item_timestamp,
            out string strError)
        {
            strError = "";
            strContent = "";
            strState = "";
            item_timestamp = null;

            string strCommentXml = "";
            string strOutputItemRecPath = "";
            string strBiblioText = "";
            string strBiblioRecPath = "";
            long lRet = channel.GetCommentInfo(
     stop,
     "@path:" + strCommentRecPath,
     // "",
     "xml",
     out strCommentXml,
     out strOutputItemRecPath,
     out item_timestamp,
     null,
     out strBiblioText,
     out strBiblioRecPath,
     out strError);
            if (lRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时间出错: " + ex.Message;
                return -1;
            }

            strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            strContent = DomUtil.GetElementText(dom.DocumentElement, "content");
            return 0;
        }

        // 修改指示符1为空的那些 610 字段
        // parameters:
        //      subjects  可以修改的自由词的总和。包括以前存在的和本次添加的
        static int ChangeSubject(ref string strBiblioXml,
            List<string> subjects,
            out string strError)
        {
            strError = "";

            // 对主题词去重

            string strMARC = "";
            string strMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            nRet = ChangeSubject(ref strMARC,
                strMarcSyntax,
                subjects,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = MarcUtil.Marc2XmlEx(strMARC,
                strMarcSyntax,
                ref strBiblioXml,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        /// <summary>
        /// 根据提供的主题词字符串 修改 MARC 记录中的 610 或 653 字段
        /// </summary>
        /// <param name="strMARC">要操作的 MARC 记录字符串。机内格式</param>
        /// <param name="strMarcSyntax">MARC 格式</param>
        /// <param name="subjects">主题词字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int ChangeSubject(ref string strMARC,
            string strMarcSyntax,
            List<string> subjects,
            out string strError)
        {
            strError = "";

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList nodes = null;
            if (strMarcSyntax == "unimarc")
                nodes = record.select("field[@name='610' and @indicator1=' ']");
            else if (strMarcSyntax == "usmarc")
                nodes = record.select("field[@name='653' and @indicator1=' ']");
            else
            {
                strError = "未知的 MARC 格式类型 '" + strMarcSyntax + "'";
                return -1;
            }

            if (subjects == null || subjects.Count == 0)
            {
                // 删除那些可以删除的 610 字段
                foreach (MarcNode node in nodes)
                {
                    MarcNodeList subfields = node.select("subfield[@name='a']");
                    if (subfields.count == node.ChildNodes.count)
                    {
                        // 如果除了 $a 以外没有其他任何子字段，则字段可以删除
                        node.detach();
                    }
                }
            }
            else
            {

                MarcNode field610 = null;

                // 只留下一个 610 字段
                if (nodes.count > 1)
                {
                    int nCount = nodes.count;
                    foreach (MarcNode node in nodes)
                    {
                        MarcNodeList subfields = node.select("subfield[@name='a']");
                        if (subfields.count == node.ChildNodes.count)
                        {
                            // 如果除了 $a 以外没有其他任何子字段，则字段可以删除
                            node.detach();
                            nCount--;
                        }

                        if (nCount <= 1)
                            break;
                    }

                    // 重新选定
                    if (strMarcSyntax == "unimarc")
                        nodes = record.select("field[@name='610' and @indicator1=' ']");
                    else if (strMarcSyntax == "usmarc")
                        nodes = record.select("field[@name='653' and @indicator1=' ']");

                    field610 = nodes[0];
                }
                else if (nodes.count == 0)
                {
                    // 创建一个新的 610 字段
                    if (strMarcSyntax == "unimarc")
                        field610 = new MarcField("610", "  ");
                    else if (strMarcSyntax == "usmarc")
                        field610 = new MarcField("653", "  ");

                    record.ChildNodes.insertSequence(field610);
                }
                else
                {
                    Debug.Assert(nodes.count == 1, "");
                    field610 = nodes[0];
                }

                // 删除全部 $a 子字段
                field610.select("subfield[@name='a']").detach();


                // 添加若干个 $a 子字段
                Debug.Assert(subjects.Count > 0, "");
                MarcNodeList source = new MarcNodeList();
                for (int i = 0; i < subjects.Count; i++)
                {
                    source.add(new MarcSubfield("a", subjects[i]));
                }
                // 寻找适当位置插入
                field610.ChildNodes.insertSequence(source[0]);
                if (source.count > 1)
                {
                    // 在刚插入的对象后面插入其余的对象
                    MarcNodeList list = new MarcNodeList(source[0]);
                    source.removeAt(0); // 排除刚插入的一个
                    list.after(source);
                }
            }

            strMARC = record.Text;
            return 0;
        }

        // parameters:
        //      reserve_subjects   保留的自由词。指指示符1为 0/1/2 的自由词。这些自由词不让对话框修改(可以在 MARC 编辑器修改)
        //      subjects          让修改的自由词。指示符1为 空。这些自由词让对话框修改
        int GetExistSubject(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            out string strBiblioXml,
            out List<string> reserve_subjects,
            out List<string> subjects,
            out byte[] timestamp,
            out string strError)
        {
            strError = "";
            reserve_subjects = new List<string>();
            subjects = new List<string>();
            timestamp = null;
            strBiblioXml = "";

            string[] results = null;

            // 获得书目记录
            long lRet = channel.GetBiblioInfos(
                stop,
                strBiblioRecPath,
                "",
                new string[] { "xml" },   // formats
                out results,
                out timestamp,
                out strError);
            if (lRet == 0)
                return -1;
            if (lRet == -1)
                return -1;

            if (results == null || results.Length == 0)
            {
                strError = "results error";
                return -1;
            }

            strBiblioXml = results[0];

            string strMARC = "";
            string strMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            int nRet = MarcUtil.Xml2Marc(strBiblioXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML转换到MARC记录时出错: " + strError;
                return -1;
            }

            nRet = GetSubjectInfo(strMARC,
                strMarcSyntax,
                out reserve_subjects,
                out subjects,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        /// <summary>
        /// 从 MARC 字符串中获得主题词信息
        /// </summary>
        /// <param name="strMARC">MARC 字符串。机内格式</param>
        /// <param name="strMarcSyntax">MARC 格式</param>
        /// <param name="reserve_subjects">返回要保留的主题词集合。字段指示符1 不为空的</param>
        /// <param name="subjects">返回主题词集合。字段指示符1 为空的</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public static int GetSubjectInfo(string strMARC,
            string strMarcSyntax,
            out List<string> reserve_subjects,
            out List<string> subjects,
            out string strError)
        {
            strError = "";
            reserve_subjects = new List<string>();
            subjects = new List<string>();

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList nodes = null;
            if (strMarcSyntax == "unimarc")
                nodes = record.select("field[@name='610']/subfield[@name='a']");
            else if (strMarcSyntax == "usmarc")
                nodes = record.select("field[@name='653']/subfield[@name='a']");
            else
            {
                strError = "未知的 MARC 格式类型 '" + strMarcSyntax + "'";
                return -1;
            }

            foreach (MarcNode node in nodes)
            {
                if (string.IsNullOrEmpty(node.Content.Trim()) == true)
                    continue;

                Debug.Assert(node.NodeType == NodeType.Subfield, "");

                if (node.Parent.Indicator1 == ' ')
                    subjects.Add(node.Content.Trim());
                else
                    reserve_subjects.Add(node.Content.Trim());
            }

            return 0;
        }

        void m_chargingInterface_CallFunc(object sender, EventArgs e)
        {
            if (this.DbType != "item")
                return;

            string name = sender as string;
            this.BeginInvoke(new Action<string>(LoadBorrowHistory), name);
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

            string strItemRefID = GetItemRefID();

            if (string.IsNullOrEmpty(strItemRefID) == true)
            {
                strError = "strItemRefID 为空";
                goto ERROR1;
            }

            int nRet = LoadBorrowHistory(strItemRefID,
     nPageNo,
     out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        string GetItemRefID()
        {
            if (string.IsNullOrEmpty(this._itemBarcode) == false)
                return "@itemBarcode:" + this._itemBarcode;
            return "@itemRefID:" + this._refID;
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

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在装载借阅历史 ...");
            _stop.BeginLoop();

            EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在装载借阅历史 ...",
                "disableControl");
            try
            {
                long lRet = 0;
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

#if SUPPORT_OLD_STOP
                this.ChannelDoEvents = false;
#endif
                // this.Channel.Idle += Channel_Idle;  // 防止控制权出让给正在获取摘要的读者信息 HTML 页面
                try
                {
                    lRet = channel.LoadChargingHistory(looping.Progress,
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
                looping.Dispose();
                /*
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }
        }

#if NO
        void Channel_Idle(object sender, IdleEventArgs e)
        {
            // e.bDoEvents = false;

        }
#endif

        void ClearBorrowHistoryPage()
        {
            ClearHtml();

            string strItemLink = "<a href='javascript:void(0);' onclick=\"window.external.Call('load');\">" + HttpUtility.HtmlEncode("装载") + "</a>";

            AppendHtml("<html><body>");
            AppendHtml(strItemLink);
            AppendHtml("</body></html>");

            _borrowHistoryLoaded = false;
        }

        int _currentPageNo = 0;
        int _pageCount = 0;

        static string MakeAnchor(string name, string caption, bool enabled)
        {
            if (enabled)
                return "<a href='javascript:void(0);' onclick=\"window.external.Call('" + name + "');\">" + HttpUtility.HtmlEncode(caption) + "</a>";
            return HttpUtility.HtmlEncode(caption);
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
            text.Append("<td class='nowrap'>证条码号</td>");
            text.Append("<td class='nowrap'>姓名</td>");
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
                text.Append("<td class='nowrap'>" + (nStart + 1).ToString() + "</td>");
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(ReaderInfoForm.GetOperTypeName(item.Action)) + "</td>");

                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(item.PatronBarcode) + "</td>");
                text.Append("<td class='summary pending nowrap'>P:" + HttpUtility.HtmlEncode(item.PatronBarcode) + "</td>");

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

        private void tabControl_item_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_item.SelectedTab == this.tabPage_borrowHistory)
            {
                if (_borrowHistoryLoaded == false)
                {
                    this.BeginInvoke(new Action<string>(LoadBorrowHistory), "load");
                    _borrowHistoryLoaded = true;
                }
            }
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            SaveRecord(Control.ModifierKeys == Keys.Control ? "force" : "");
        }

        private void ToolStripMenuItem_insertCoverImageFromClipboard_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.tabControl_item.SelectedTab != this.tabPage_object)
                this.tabControl_item.SelectedTab = this.tabPage_object;

            List<ListViewItem> deleted_items = this.binaryResControl1.FindAllMaskDeleteItem();
            if (deleted_items.Count > 0)
            {
                strError = "当前有标记删除的对象尚未提交保存。请先提交这些保存后，再进行插入封面图像的操作";
                goto ERROR1;
            }

            // 从剪贴板中取得图像对象
            List<Image> images = ImageUtil.GetImagesFromClipboard(out strError);
            if (images == null)
            {
                strError += "。无法创建封面图像";
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
                    strError = "当前 Windows 剪贴板中的第一个文件不是图像文件。无法创建封面图像";
                    goto ERROR1;
                }
            }
            else
            {
                strError = "当前 Windows 剪贴板中没有图形对象。无法创建封面图像";
                goto ERROR1;
            }
#endif

            CreateCoverImageDialog dlg = null;
            try
            {
                dlg = new CreateCoverImageDialog();

                MainForm.SetControlFont(dlg, this.Font, false);
                ImageInfo info = new ImageInfo();
                info.Image = image;
                dlg.ImageInfo = info;
                Program.MainForm.AppInfo.LinkFormState(dlg, "entityform_CreateCoverImageDialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }
            }

            foreach (ImageType type in dlg.ResultImages)
            {
                if (type.Image == null)
                {
                    continue;
                }

                string strType = "FrontCover." + type.TypeName;
                string strSize = type.Image.Width.ToString() + "X" + type.Image.Height.ToString() + "px";

                // string strShrinkComment = "";
                string strID = "";
                nRet = EntityForm.SetImageObject(
                    this.binaryResControl1,
                    type.Image,
                    strType,    // "coverimage",
                                // out strShrinkComment,
                    out strID,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                Field field_856 = null;
                List<Field> fields = DetailHost.Find856ByResID(this.m_marcEditor,
                        strID);
                if (fields.Count == 1)
                {
                    field_856 = fields[0];
                    // TODO: 出现对话框
                }
                else if (fields.Count > 1)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前 MARC 编辑器中已经存在 " + fields.Count.ToString() + " 个 856 字段其 $" + DetailHost.LinkSubfieldName + " 子字段关联了对象 ID '" + strID + "' ，是否要编辑其中的第一个 856 字段?\r\n\r\n(注：可改在 MARC 编辑器中选中一个具体的 856 字段进行编辑)\r\n\r\n(OK: 编辑其中的第一个 856 字段; Cancel: 取消操作",
                        "EntityForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        return;
                    field_856 = fields[0];
                    // TODO: 出现对话框
                }
                else
                    field_856 = this.m_marcEditor.Record.Fields.Add("856", "  ", "", true);

                field_856.IndicatorAndValue = ("72$3Cover Image$" + DetailHost.LinkSubfieldName + "uri:" + strID + "$xtype:" + strType + ";size:" + strSize + "$2dp2res").Replace('$', (char)31);
#endif
            }

            MessageBox.Show(this, "封面图像已经成功创建。\r\n"
                + "\r\n\r\n(但因当前记录还未保存，图像数据尚未提交到服务器)\r\n\r\n注意稍后保存当前记录。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void ToolStripMenuItem_clearCoverImage_Click(object sender, EventArgs e)
        {

            if (this.tabControl_item.SelectedTab != this.tabPage_object)
                this.tabControl_item.SelectedTab = this.tabPage_object;

            bool bChanged = false;
#if NO
            string[] types = new string[] {
                "FrontCover.SmallImage",
                "FrontCover.MediumImage",
                "FrontCover.LargeImage"};
            foreach (string type in types)
            {
                // TODO: 最好支持 CoverImage.* 通配符匹配。或者用正则表达式
                List<ListViewItem> items = this.binaryResControl1.FindItemByUsage(type);
                if (items.Count > 0)
                {
                    this.binaryResControl1.MaskDelete(items);
                    bChanged = true;
                }
            }
#endif
            bChanged = this.binaryResControl1.MaskDeleteCoverImageObject();

            if (bChanged == true)
                MessageBox.Show(this, "封面图像对象已经成功标记删除。\r\n"
                    + "\r\n\r\n(但因当前记录还未保存，删除动作尚未提交到服务器)\r\n\r\n注意稍后保存当前记录。");
            else
                MessageBox.Show(this, "没有发现封面图像对象");
        }

        private void toolStripSplitButton_insertCoverImage_ButtonClick(object sender, EventArgs e)
        {
            ToolStripMenuItem_insertCoverImageFromClipboard_Click(sender, e);
        }

        private void ToolStripMenuItem_pasteXmlRecord_Click(object sender, EventArgs e)
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
            {
                MessageBox.Show(this, "剪贴板中没有内容");
                return;
            }

            this.Xml = (string)ido.GetData(DataFormats.UnicodeText);

        }

        private void textBox_editor_TextChanged(object sender, EventArgs e)
        {
            this.ItemXmlChanged = true;
        }

        // 规整 XML
        private void ToolStripMenuItem_edit_indentXml_Click(object sender, EventArgs e)
        {
            int nRet = DomUtil.GetIndentXml(this.textBox_editor.Text,
                true,
                out string strXml,
                out string strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            this.textBox_editor.Text = strXml;
        }

        // 删除 XML 中的空元素
        private void ToolStripMenuItem_edit_removeEmptyElements_Click(object sender, EventArgs e)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.textBox_editor.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "XML 格式错误: " + ex.Message);
                return;
            }

            DomUtil.RemoveEmptyElements(dom.DocumentElement, false);
            if (dom.DocumentElement != null)
            {
                int nRet = DomUtil.GetIndentXml(dom.OuterXml,
    true,
    out string strXml,
    out string strError);
                if (nRet == -1)
                    this.textBox_editor.Text = dom.OuterXml;
                else
                    this.textBox_editor.Text = strXml;
            }
            else
                this.textBox_editor.Text = "";
        }

    }
}