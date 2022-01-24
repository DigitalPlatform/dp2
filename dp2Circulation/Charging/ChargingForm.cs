using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 出纳操作窗口
    /// </summary>
    public partial class ChargingForm : MyForm, IProtectFocus, IChargingForm
    {
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
        WebExternalHost m_webExternalHost_itemInfo = new WebExternalHost();
        WebExternalHost m_webExternalHost_biblioInfo = new WebExternalHost();

        string m_strCurrentBarcode = "";
        string m_strCurrentReaderBarcode = "";

        FuncState m_funcstate = FuncState.Borrow;

        DisplayState m_displaystate = DisplayState.TEXT;

        // 同一个读者连续借阅成功后所累积的册条码号集合
        List<string> oneReaderItemBarcodes = new List<string>();

        const int WM_SWITCH_FOCUS = API.WM_USER + 200;
        // const int WM_LOADSIZE = API.WM_USER + 201;
        const int WM_ENABLE_EDIT = API.WM_USER + 202;

        // 消息WM_SWITCH_FOCUS的wparam参数值
        /// <summary>
        /// 焦点位置下标：读者证条码号输入域
        /// </summary>
        public const int READER_BARCODE = 0;
        /// <summary>
        /// 焦点位置下标：读者证密码输入域
        /// </summary>
        public const int READER_PASSWORD = 1;
        /// <summary>
        /// 焦点位置下标：册条码号输入域
        /// </summary>
        public const int ITEM_BARCODE = 2;

        // string FastInputText = "";  // CharingInfoDlg返回的快捷输入内容

        Hashtable m_textTable = new Hashtable();
        int m_nTextNumber = 0;

        // 记载连续输入的册条码号
        List<BarcodeAndTime> m_itemBarcodes = new List<BarcodeAndTime>();

        /// <summary>
        /// 当前活动的读者证条码号
        /// </summary>
        public string ActiveReaderBarcode
        {
            get
            {
                return this.toolStripDropDownButton_readerBarcodeNavigate.Text;
            }
            set
            {
                this.toolStripDropDownButton_readerBarcodeNavigate.Text = value;

                if (String.IsNullOrEmpty(value) == true)
                {
                    this.toolStripMenuItem_naviToAmerceForm.Enabled = false;
                    this.toolStripMenuItem_naviToReaderInfoForm.Enabled = false;
                    this.toolStripMenuItem_naviToActivateForm_old.Enabled = false;
                    this.toolStripMenuItem_openReaderManageForm.Enabled = false;
                    this.toolStripMenuItem_naviToActivateForm_new.Enabled = false;
                }
                else
                {
                    this.toolStripMenuItem_naviToAmerceForm.Enabled = true;
                    this.toolStripMenuItem_naviToReaderInfoForm.Enabled = true;
                    this.toolStripMenuItem_naviToActivateForm_old.Enabled = true;
                    this.toolStripMenuItem_openReaderManageForm.Enabled = true;
                    this.toolStripMenuItem_naviToActivateForm_new.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 当前活动的册条码号
        /// </summary>
        public string ActiveItemBarcode
        {
            get
            {
                return this.toolStripDropDownButton_itemBarcodeNavigate.Text;
            }
            set
            {
                this.toolStripDropDownButton_itemBarcodeNavigate.Text = value;

                if (String.IsNullOrEmpty(value) == true)
                {
                    this.toolStripMenuItem_openEntityForm.Enabled = false;
                    this.toolStripMenuItem_openItemInfoForm.Enabled = false;
                }
                else
                {
                    this.toolStripMenuItem_openEntityForm.Enabled = true;
                    this.toolStripMenuItem_openItemInfoForm.Enabled = true;
                }
            }
        }

        // 
        /// <summary>
        /// 显示读者信息的格式。为 text html 之一
        /// </summary>
        public string PatronRenderFormat
        {
            get
            {
                if (this.DisplayState == DisplayState.TEXT)
                    return "text";

                if (this.NoBorrowHistory == true
                    && StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.21") >= 0)
                    return "html:noborrowhistory";

                return "html";
            }
        }

        /// <summary>
        /// 显示书目、册信息的格式。为 text html 之一
        /// </summary>
        public string RenderFormat
        {
            get
            {
                if (this.DisplayState == DisplayState.TEXT)
                    return "text";
                return "html";
            }
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        public DisplayState DisplayState
        {
            get
            {
                return this.m_displaystate;
            }
            set
            {
                this.m_displaystate = value;

                if (this.m_displaystate == DisplayState.TEXT)
                {
                    this.webBrowser_reader.Visible = false;
                    this.webBrowser_biblio.Visible = false;
                    this.webBrowser_item.Visible = false;

                    this.textBox_readerInfo.Visible = true;
                    this.textBox_biblioInfo.Visible = true;
                    this.textBox_itemInfo.Visible = true;

                    this.tableLayoutPanel_readerInfo.RowStyles[2].SizeType = SizeType.Percent;
                    this.tableLayoutPanel_readerInfo.RowStyles[2].Height = 1.0F;

                    this.tableLayoutPanel_biblioInfo.RowStyles[2].SizeType = SizeType.Percent;
                    this.tableLayoutPanel_biblioInfo.RowStyles[2].Height = 1.0F;

                    this.tableLayoutPanel_itemInfo.RowStyles[2].SizeType = SizeType.Percent;
                    this.tableLayoutPanel_itemInfo.RowStyles[2].Height = 1.0F;

                }
                if (this.m_displaystate == DisplayState.HTML)
                {
                    this.textBox_readerInfo.Visible = false;
                    this.textBox_biblioInfo.Visible = false;
                    this.textBox_itemInfo.Visible = false;

                    this.webBrowser_reader.Visible = true;
                    this.webBrowser_biblio.Visible = true;
                    this.webBrowser_item.Visible = true;

                    this.tableLayoutPanel_readerInfo.RowStyles[1].SizeType = SizeType.Percent;
                    this.tableLayoutPanel_readerInfo.RowStyles[1].Height = 1.0F;

                    this.tableLayoutPanel_biblioInfo.RowStyles[1].SizeType = SizeType.Percent;
                    this.tableLayoutPanel_biblioInfo.RowStyles[1].Height = 1.0F;

                    this.tableLayoutPanel_itemInfo.RowStyles[1].SizeType = SizeType.Percent;
                    this.tableLayoutPanel_itemInfo.RowStyles[1].Height = 1.0F;
                }
            }
        }

        /// <summary>
        /// 是否要强制操作[暂未使用]
        /// </summary>
        public bool Force
        {
            get
            {
                return false;   // 2008/10/29 
                /*
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "force",
                    false);
                 * */
            }
            set
            {

                Program.MainForm.AppInfo.SetBoolean(
                    "charging_form",
                    "force",
                    value);
            }
        }


        // 
        // 2008/9/26
        /// <summary>
        /// 是否自动清除输入框中内容
        /// </summary>
        public bool AutoClearTextbox
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "autoClearTextbox",
                    true);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "charging_form",
                    "autoClearTextbox",
                    value);
            }
        }

        /// <summary>
        /// 是否 不显示书目和册信息
        /// </summary>
        public bool NoBiblioAndItemInfo
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "no_biblio_and_item_info",
                    false);
            }

        }

        /// <summary>
        /// 是否 不显示绿色提示对话框
        /// </summary>
        public bool GreenDisable
        {
            get
            {
                return
                Program.MainForm.AppInfo.GetBoolean(
                "charging_form",
                "green_infodlg_not_occur",
                false);
            }
        }

        /// <summary>
        /// 是否自动校验输入的条码号
        /// </summary>
        public bool NeedVerifyBarcode
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "verify_barcode",
                    false);
            }
        }

        /// <summary>
        /// 信息对话框的不透明度
        /// </summary>
        public double InfoDlgOpacity
        {
            get
            {
                return (double)Program.MainForm.AppInfo.GetInt(
                    "charging_form",
                    "info_dlg_opacity",
                    100) / (double)100;
            }
        }

        //
        bool AutoSwitchReaderBarcode
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "auto_switch_reader_barcode",
                    false);
            }

        }



        bool DoubleItemInputAsEnd
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "doubleItemInputAsEnd",
                    false);
            }

        }

        /// <summary>
        /// 改变布局方式
        /// </summary>
        /// <param name="bNoBiblioAndItemInfo">是否 不要显示书目和册信息部分</param>
        public void ChangeLayout(bool bNoBiblioAndItemInfo)
        {
            if (bNoBiblioAndItemInfo == true)
            {
                // 把operation表格控件，从biblioanditem移动到readerinfo表格控件内
                this.tableLayoutPanel_readerInfo.Controls.Add(this.tableLayoutPanel_operation,
                    0, 4);
                this.tableLayoutPanel_biblioAndItem.Controls.Remove(this.tableLayoutPanel_operation);

                // 把readerinfo表格控件(从main的panel1)提升到最高层
                this.panel_main.Controls.Add(this.tableLayoutPanel_readerInfo); // Dock本来就是Fill

                this.splitContainer_main.Panel1.Controls.Remove(this.tableLayoutPanel_readerInfo);

                /*
                // 把readerinfo的Dock方式修改为Fill
                this.tableLayoutPanel_readerInfo.Dock = DockStyle.Fill;
                 * */

                // 隐藏splitContainer_main
                this.splitContainer_main.Visible = false;
            }
            else
            {
                // 把operation表格控件，从readerinfo表格控件内移动到biblioanditem
                this.tableLayoutPanel_biblioAndItem.Controls.Add(this.tableLayoutPanel_operation,
                    0, 3);
                this.tableLayoutPanel_readerInfo.Controls.Remove(this.tableLayoutPanel_operation);

                // 把readerinfo表格控件从最高层移动到main的panel1
                this.splitContainer_main.Panel1.Controls.Add(this.tableLayoutPanel_readerInfo);
                this.panel_main.Controls.Remove(this.tableLayoutPanel_readerInfo);

                /*
                // 把readerinfo的Dock方式修改为Fill
                this.tableLayoutPanel_readerInfo.Dock = DockStyle.Fill;
                 * */

                // 显示splitContainer_main
                this.splitContainer_main.Visible = true;
            }
        }

        private void ChargingForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            if (this.NoBiblioAndItemInfo == true)
            {
                ChangeLayout(true);
            }

            Program.MainForm.AppInfo.LoadMdiLayout += new EventHandler(AppInfo_LoadMdiLayout);
            Program.MainForm.AppInfo.SaveMdiLayout += new EventHandler(AppInfo_SaveMdiLayout);

            // LoadSize();

            this.FuncState = this.FuncState;    // 使"操作"按钮文字显示正确

            string strDisplayFormat =
                Program.MainForm.AppInfo.GetString(
                "charging_form",
                "display_format",
                "HTML");
            if (strDisplayFormat == "HTML")
                this.DisplayState = DisplayState.HTML;
            else
                this.DisplayState = DisplayState.TEXT;

            // webbrowser
            this.m_webExternalHost_readerInfo.Initial(// Program.MainForm, 
                this.webBrowser_reader);
            this.webBrowser_reader.ObjectForScripting = this.m_webExternalHost_readerInfo;

            // 2009/10/18 
            this.m_webExternalHost_itemInfo.Initial(// Program.MainForm,
                this.webBrowser_item);
            this.webBrowser_item.ObjectForScripting = this.m_webExternalHost_itemInfo;

            this.m_webExternalHost_biblioInfo.Initial(// Program.MainForm, 
                this.webBrowser_biblio);
            this.webBrowser_biblio.ObjectForScripting = this.m_webExternalHost_biblioInfo;

#if NO
            // this.VerifyReaderPassword = this.VerifyReaderPassword;  // 使"校验读者密码"按钮文字状态显示正确
            this.VerifyReaderPassword = Program.MainForm.AppInfo.GetBoolean(
                "charging_form",
                "verify_reader_password",
                false);
#endif
            if (this.VerifyReaderPassword == true)
            {
                this.label_verifyReaderPassword.Visible = true;
                this.textBox_readerPassword.Visible = true;
                this.button_verifyReaderPassword.Visible = true;
            }
            else
            {
                this.label_verifyReaderPassword.Visible = false;
                this.textBox_readerPassword.Visible = false;
                this.button_verifyReaderPassword.Visible = false;
            }

            if (this.PatronBarcodeAllowHanzi == false)
                this.textBox_readerBarcode.ImeMode = System.Windows.Forms.ImeMode.Disable;

            this.SwitchFocus(READER_BARCODE, null);

#if NO
            // 窗口打开时初始化
            this.m_bSuppressScriptErrors = !Program.MainForm.DisplayScriptErrorDialog;
#endif

            // API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            SetReaderRenderString("(空)");
            SetBiblioRenderString("(空)");
            SetItemRenderString("(空)");
        }

        void AppInfo_SaveMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            if (Program.MainForm != null)
            {
                // 分割条位置
                // 保存splitContainer_main的状态
                Program.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "chargingform_state",
                    "splitContainer_main");
                // 保存splitContainer_biblioAndItem的状态
                Program.MainForm.SaveSplitterPos(
                    this.splitContainer_biblioAndItem,
                    "chargingform_state",
                    "splitContainer_biblioAndItem");
            }
        }

        void AppInfo_LoadMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            try
            {
                /*
                // 获得splitContainer_main的状态
                int nValue = MainForm.AppInfo.GetInt(
                "chargingform_state",
                "splitContainer_main",
                -1);
                if (nValue != -1)
                    this.splitContainer_main.SplitterDistance = nValue;

                // 获得splitContainer_biblioAndItem的状态
                nValue = MainForm.AppInfo.GetInt(
                "chargingform_state",
                "splitContainer_biblioAndItem",
                -1);
                if (nValue != -1)
                    this.splitContainer_biblioAndItem.SplitterDistance = nValue;
                 * */

                // 获得splitContainer_main的状态
                Program.MainForm.LoadSplitterPos(
                    this.splitContainer_main,
                    "chargingform_state",
                    "splitContainer_main");

                // 获得splitContainer_biblioAndItem的状态
                Program.MainForm.LoadSplitterPos(
                    this.splitContainer_biblioAndItem,
                    "chargingform_state",
                    "splitContainer_biblioAndItem");
            }
            catch
            {
            }
        }

        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_webExternalHost_readerInfo.ChannelInUse || this.m_webExternalHost_itemInfo.ChannelInUse || this.m_webExternalHost_biblioInfo.ChannelInUse;
        }

        private void ChargingForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ChargingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.commander.Destroy();

            if (this.m_webExternalHost_readerInfo != null)
            {
                this.m_webExternalHost_readerInfo.Destroy();
            }
            if (this.m_webExternalHost_itemInfo != null)
            {
                this.m_webExternalHost_itemInfo.Destroy();
            }
            if (this.m_webExternalHost_biblioInfo != null)
            {
                this.m_webExternalHost_biblioInfo.Destroy();
            }

            if (this.Channel != null)
                this.Channel.Close();   // TODO: 最好限制一个时间，超过这个时间则Abort()

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                // SaveSize();

                Program.MainForm.AppInfo.LoadMdiLayout -= new EventHandler(AppInfo_LoadMdiLayout);
                Program.MainForm.AppInfo.SaveMdiLayout -= new EventHandler(AppInfo_SaveMdiLayout);
            }
        }

        /// <summary>
        /// 同一个读者连续借阅成功后所累积的册条码号集合
        /// </summary>
        public string[] OneReaderItemBarcodes
        {
            get
            {
                if (this.oneReaderItemBarcodes == null)
                    return null;
                string[] result = new string[this.oneReaderItemBarcodes.Count];
                for (int i = 0; i < this.oneReaderItemBarcodes.Count; i++)
                {
                    result[i] = this.oneReaderItemBarcodes[i];
                }
                return result;
            }
        }

        // 带有焦点切换功能的
        /// <summary>
        /// 功能类型。设置时带有焦点切换功能
        /// </summary>
        public FuncState SmartFuncState
        {
            get
            {
                return m_funcstate;
            }
            set
            {
                /*
                FuncState old_funcstate = this.m_funcstate;

                this.FuncState = value;

                // 清除webbrowser
                SetReaderRenderString("(空)");

                SetBiblioRenderString("(空)");
                SetItemRenderString("(空)");

                // 切换为不同的功能的时候，定位焦点
                if (old_funcstate != this.m_funcstate)
                {
                    if (this.m_funcstate != FuncState.Return)
                    {
                        this.textBox_readerBarcode.SelectAll();
                        this.textBox_itemBarcode.SelectAll();

                        // this.textBox_readerBarcode.Focus();
                        this.SwitchFocus(READER_BARCODE, null);
                    }
                    else
                    {
                        this.textBox_itemBarcode.SelectAll();

                        // this.textBox_itemBarcode.Focus();
                        this.SwitchFocus(ITEM_BARCODE, null);
                    }
                }
                else // 重复设置为同样功能，当作清除功能
                {
                        this.textBox_readerBarcode.Text = "";
                        this.textBox_itemBarcode.Text = "";

                    if (this.m_funcstate != FuncState.Return)
                    {
                        // this.textBox_readerBarcode.Focus();
                        this.SwitchFocus(READER_BARCODE, null);
                    }
                    else
                    {
                        // this.textBox_itemBarcode.Focus();
                        this.SwitchFocus(ITEM_BARCODE, null);
                    }
                }

                */

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
            // 2011/12/6
            this.m_webExternalHost_itemInfo.StopPrevious();
            this.webBrowser_reader.Stop();
            this.m_webExternalHost_readerInfo.StopPrevious();
            this.webBrowser_item.Stop();
            this.m_webExternalHost_biblioInfo.StopPrevious();
            this.webBrowser_biblio.Stop();

            FuncState old_funcstate = this.m_funcstate;

            this.FuncState = value;

            // 清除webbrowser
            if (bClearInfoWindow == true)
            {
                SetReaderRenderString("(空)");

                SetBiblioRenderString("(空)");
                SetItemRenderString("(空)");
            }

            // 切换为不同的功能的时候，定位焦点
            if (old_funcstate != this.m_funcstate)
            {
                // 2008/9/26 
                if (this.AutoClearTextbox == true)
                {
                    this.textBox_readerBarcode.Text = "";
                    this.textBox_readerPassword.Text = "";
                    this.textBox_itemBarcode.Text = "";
                }

                if (this.m_funcstate != FuncState.Return)
                {
                    this.textBox_readerBarcode.SelectAll();
                    this.textBox_itemBarcode.SelectAll();

                    // this.textBox_readerBarcode.Focus();
                    this.SwitchFocus(READER_BARCODE, null);
                }
                else
                {
                    this.textBox_itemBarcode.SelectAll();

                    // this.textBox_itemBarcode.Focus();
                    this.SwitchFocus(ITEM_BARCODE, null);
                }
            }
            else // 重复设置为同样功能，当作清除功能
            {
                // 2008/9/26 
                if (this.AutoClearTextbox == true)
                {
                    this.textBox_readerBarcode.Text = "";
                    this.textBox_readerPassword.Text = "";
                    this.textBox_itemBarcode.Text = "";
                }
                else
                {
                    if (bDupAsClear == true)
                    {
                        this.textBox_readerBarcode.Text = "";
                        this.textBox_readerPassword.Text = "";
                        this.textBox_itemBarcode.Text = "";
                    }
                }

                if (this.m_funcstate != FuncState.Return)
                {
                    // this.textBox_readerBarcode.Focus();
                    this.SwitchFocus(READER_BARCODE, null);
                }
                else
                {
                    // this.textBox_itemBarcode.Focus();
                    this.SwitchFocus(ITEM_BARCODE, null);
                }
            }
        }

        FuncState FuncState
        {
            get
            {
                return m_funcstate;
            }
            set
            {
                // 清除记忆的册条码号
                this.m_itemBarcodes.Clear();

                this.m_funcstate = value;

                this.toolStripMenuItem_borrow.Checked = false;
                this.toolStripMenuItem_return.Checked = false;
                this.toolStripMenuItem_verifyReturn.Checked = false;
                this.toolStripMenuItem_renew.Checked = false;
                this.toolStripMenuItem_verifyRenew.Checked = false;
                this.toolStripMenuItem_lost.Checked = false;

                // 2008/9/26 
                if (this.AutoClearTextbox == true)
                {
                    this.textBox_readerBarcode.Text = "";
                    this.textBox_readerPassword.Text = "";
                    this.textBox_itemBarcode.Text = "";
                }

                if (m_funcstate == FuncState.Borrow)
                {
                    this.button_itemAction.Text = "借";
                    this.toolStripMenuItem_borrow.Checked = true;
                    // this.textBox_readerBarcode.Enabled = true;
                    EnableEdit(READER_BARCODE, true);
                }
                if (m_funcstate == FuncState.Return)
                {
                    this.button_itemAction.Text = "还";
                    this.toolStripMenuItem_return.Checked = true;
                    this.textBox_readerBarcode.Text = "";
                    // this.textBox_readerBarcode.Enabled = false;
                    EnableEdit(READER_BARCODE, false);
                }
                if (m_funcstate == FuncState.VerifyReturn)
                {
                    this.button_itemAction.Text = "验证还";
                    this.toolStripMenuItem_verifyReturn.Checked = true;
                    // this.textBox_readerBarcode.Enabled = true;
                    EnableEdit(READER_BARCODE, true);
                }
                // 2015/12/29
                if (m_funcstate == FuncState.Renew)
                {
                    this.button_itemAction.Text = "续借";
                    this.toolStripMenuItem_renew.Checked = true;
                    this.textBox_readerBarcode.Text = "";
                    EnableEdit(READER_BARCODE, false);
                }
                if (m_funcstate == FuncState.VerifyRenew)
                {
                    this.button_itemAction.Text = "验证续借";
                    this.toolStripMenuItem_verifyRenew.Checked = true;
                    // this.textBox_readerBarcode.Enabled = true;
                    EnableEdit(READER_BARCODE, true);
                }
                if (m_funcstate == FuncState.Lost)
                {
                    this.button_itemAction.Text = "丢失";
                    this.toolStripMenuItem_lost.Checked = true;
                    // this.textBox_readerBarcode.Enabled = true;
                    EnableEdit(READER_BARCODE, true);
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChargingForm()
        {
            InitializeComponent();

            // 让浏览器控件具备可以分辨首次非停止的能力
            Global.PrepareStop(this.webBrowser_biblio);
            Global.PrepareStop(this.webBrowser_item);
            Global.PrepareStop(this.webBrowser_reader);
        }

        /// <summary>
        /// 当前读者证条码号
        /// </summary>
        public string CurrentReaderBarcode
        {
            get
            {
                return m_strCurrentReaderBarcode;
            }
            set
            {
                m_strCurrentBarcode = value;

                // 搜索出记录，显示在窗口中
                string strError = "";
                // return:
                //      -1  error
                //      0   没有找到
                //      1   成功
                //      2   放弃
                int nRet = LoadReaderRecord(ref m_strCurrentBarcode,
                    out strError);
                if (nRet == -1)
                {
                    SetReaderRenderString(
                        "装载读者记录发生错误: " + strError);
                }
            }
        }

        /// <summary>
        /// 重新装载读者记录
        /// </summary>
        public void Reload()
        {
            string strBarcode = this.textBox_readerBarcode.Text;

            if (string.IsNullOrEmpty(strBarcode) == true)
                strBarcode = m_strCurrentBarcode;

            if (string.IsNullOrEmpty(strBarcode) == true)
                return;


            // 搜索出记录，显示在窗口中
            string strError = "";
            // return:
            //      -1  error
            //      0   没有找到
            //      1   成功
            //      2   放弃
            int nRet = LoadReaderRecord(ref strBarcode,
                out strError);
            if (nRet == -1)
            {
                SetReaderRenderString(
                    "装载读者记录发生错误: " + strError);
            }

            if (strBarcode != this.textBox_readerBarcode.Text)
                this.textBox_readerBarcode.Text = strBarcode;
        }


        void SetReaderRenderString(string strText)
        {
            // NewExternal();

            if (this.DisplayState == DisplayState.TEXT)
                this.textBox_readerInfo.Text = strText;
            else
            {
                m_webExternalHost_readerInfo.StopPrevious();

                if (strText == "(空)")
                {
                    Global.ClearHtmlPage(this.webBrowser_reader,
                        Program.MainForm.DataDir);
                    return;
                }

                // 2012/1/13
                Global.StopWebBrowser(this.webBrowser_reader);

                // PathUtil.CreateDirIfNeed(Program.MainForm.DataDir + "\\servermapped");

                string strTempFilename = Path.Combine(Program.MainForm.DataDir, "~charging_temp_reader.html");
                using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
                {
                    sw.Write(strText);
                }
                this.webBrowser_reader.Navigate(strTempFilename);
            }
        }

        void SetItemRenderString(string strText)
        {
            if (this.DisplayState == DisplayState.TEXT)
                this.textBox_itemInfo.Text = strText;
            else
            {
                // 2011/12/6
                this.m_webExternalHost_itemInfo.StopPrevious();

                if (strText == "(空)")
                {
                    Global.ClearHtmlPage(this.webBrowser_item,
                        Program.MainForm.DataDir);
                    return;
                }

                // 2012/1/13
                Global.StopWebBrowser(this.webBrowser_item);

                // PathUtil.CreateDirIfNeed(Program.MainForm.DataDir + "\\servermapped");
                string strTempFilename = Program.MainForm.DataDir + "\\~charging_temp_item.html";
                using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
                {
                    sw.Write(strText);
                }
                this.webBrowser_item.Navigate(strTempFilename);
            }
        }

        void SetBiblioRenderString(string strText)
        {
            if (this.DisplayState == DisplayState.TEXT)
                this.textBox_biblioInfo.Text = strText;
            else
            {
                // 2011/12/6
                this.m_webExternalHost_biblioInfo.StopPrevious();
                this.webBrowser_biblio.Stop();

                if (strText == "(空)")
                {
                    Global.ClearHtmlPage(this.webBrowser_biblio,
                        Program.MainForm.DataDir);
                    return;
                }

                // 2012/1/13
                Global.StopWebBrowser(this.webBrowser_biblio);

                // PathUtil.CreateDirIfNeed(Program.MainForm.DataDir + "\\servermapped");
                string strTempFilename = Program.MainForm.DataDir + "\\~charging_temp_biblio.html";
                using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
                {
                    sw.Write(strText);
                }
                this.webBrowser_biblio.Navigate(strTempFilename);
            }
        }

        // 将字符串中的宏 %datadir% 替换为实际的值
        string ReplaceMacro(string strText)
        {
            strText = strText.Replace("%mappeddir%", PathUtil.MergePath(Program.MainForm.DataDir, "servermapped"));
            return strText.Replace("%datadir%", Program.MainForm.DataDir);
        }

        /// <summary>
        /// 是否 朗读读者姓名，当装载读者记录的时候。缺省为 false
        /// </summary>
        public bool VoiceName
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "speak_reader_name",
                    false);
            }
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

        // 装入读者记录
        // return:
        //      -1  error
        //      0   没有找到
        //      1   成功
        //      2   放弃
        int LoadReaderRecord(ref string strBarcode,
            out string strError)
        {
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在初始化浏览器组件 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            EnableControls(false);

            try
            {

                SetReaderRenderString("(空)");

                SetBiblioRenderString("(空)");
                SetItemRenderString("(空)");

                string strStyle = this.PatronRenderFormat;

                if (this.VoiceName == true)
                    strStyle += ",summary";

                stop.SetMessage("正在装入读者记录 " + strBarcode + " ...");

                string[] results = null;
                byte[] baTimestamp = null;
                string strRecPath = "";
                long lRet = Channel.GetReaderInfo(
                    stop,
                    GetRequestPatronBarcode(strBarcode),
                    strStyle,   // this.RenderFormat, // "html",
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    if (StringUtil.IsIdcardNumber(strBarcode) == true)
                        SetReaderRenderString("证条码号(或身份证号)为 '" + strBarcode + "' 的读者记录没有找到 ...");
                    else
                        SetReaderRenderString("证条码号为 '" + strBarcode + "' 的读者记录没有找到 ...");
                    return 0;   // not found
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


                if (lRet > 1)
                {
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
                        // Program.MainForm,
                        StringUtil.SplitList(strRecPath),
                        "请选择一个读者记录",
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    // TODO: 保存窗口内的尺寸状态
                    Program.MainForm.AppInfo.LinkFormState(dlg, "ChargingForm_SelectPatronDialog_state");
                    dlg.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                        return 2;

                    strBarcode = dlg.SelectedBarcode;
                    strResult = dlg.SelectedHtml;
                }

                SetReaderRenderString(ReplaceMacro(strResult));

                this.m_strCurrentBarcode = strBarcode;  // 2011/6/24

                if (this.VoiceName == true && results.Length >= 2)
                {
                    string strName = results[1];
                    Program.MainForm.Speak(strName);
                }
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
            ERROR1:
            return -1;
        }

        // 装入册记录
        // parameters:
        //      strBarcode  册条码号。可以为"@path:"引导，表示使用记录路径
        int LoadItemAndBiblioRecord(string strBarcode,
            out string strError)
        {
            SetBiblioRenderString("(空)");
            SetItemRenderString("(空)");

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入册记录 " + strBarcode + " ...");
            stop.BeginLoop();

            try
            {
                string strItemText = "";
                string strBiblioText = "";

                long lRet = Channel.GetItemInfo(
                    stop,
                    strBarcode,
                    this.RenderFormat, // "html",
                    out strItemText,
                    this.RenderFormat, // "html",
                    out strBiblioText,
                    out strError);
                if (lRet == 0)
                    return 0;   // not found
                if (lRet == -1)
                    goto ERROR1;

                SetItemRenderString(ReplaceMacro(strItemText));

                SetBiblioRenderString(ReplaceMacro(strBiblioText));

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

        /// <summary>
        /// 从浏览器控件中获得 HTML 字符串
        /// </summary>
        /// <param name="webBrowser">浏览器控件</param>
        /// <returns>HTML 字符串</returns>
        public static string GetHtmlString(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
                return "";

            HtmlElement item = doc.All["html"];
            if (item == null)
                return "";

            return item.OuterHtml;
        }

#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        private void textBox_readerBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_loadReader;

            Program.MainForm.EnterPatronIdEdit(InputType.ALL);
        }

        private void textBox_readerBarcode_Leave(object sender, EventArgs e)
        {
            Program.MainForm.LeavePatronIdEdit();
        }

        private void textBox_readerPassword_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_verifyReaderPassword;
        }

        private void textBox_itemBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_itemAction;

            Program.MainForm.EnterPatronIdEdit(InputType.ALL);
        }

        private void textBox_itemBarcode_Leave(object sender, EventArgs e)
        {
            Program.MainForm.LeavePatronIdEdit();
        }

        private void button_loadReader_Click(object sender, EventArgs e)
        {
            this.button_loadReader.Enabled = false; // BUG 2009/6/2
            this.textBox_readerBarcode.Enabled = false;   // 2009/10/20 

            this.m_webExternalHost_readerInfo.StopPrevious();
            this.webBrowser_reader.Stop();

            // 2009/10/20 
            this.m_webExternalHost_itemInfo.StopPrevious();
            this.webBrowser_item.Stop();
            this.m_webExternalHost_biblioInfo.StopPrevious();
            this.webBrowser_biblio.Stop();

            this.commander.AddMessage(WM_LOAD_READER);
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
        /// <summary>
        /// 装载读者记录
        /// </summary>
        public void DoLoadReader()
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(Program.MainForm != null, "Program.MainForm == null");
            Debug.Assert(this.Channel != null, "this.Channel == null");

            // 2008/9/26 
            if (this.AutoClearTextbox == true)
            {
                this.textBox_readerPassword.Text = "";
                this.textBox_itemBarcode.Text = "";
            }

            if (this.textBox_readerBarcode.Text == "")
            {
                strError = "尚未输入读者证条码号";
                goto ERROR1;
            }

            if (this.AutoToUpper == true)
                this.textBox_readerBarcode.Text = this.textBox_readerBarcode.Text.ToUpper();

            this.ActiveReaderBarcode = this.textBox_readerBarcode.Text;

            // 清除记忆的册条码号
            Debug.Assert(this.m_itemBarcodes != null, "this.m_itemBarcodes == null");
            this.m_itemBarcodes.Clear();

            // 2017/1/4
            // 变换条码号
            // return:
            //      -1  出错
            //      0   不需要进行变换
            //      1   需要进行变换
            nRet = Program.MainForm.NeedTransformBarcode(
                Program.MainForm.FocusLibraryCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                string strText = this.textBox_readerBarcode.Text;

                nRet = Program.MainForm.TransformBarcode(
                    Program.MainForm.FocusLibraryCode,
                    ref strText,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_readerBarcode.Text = strText;
            }

            if (this.NeedVerifyBarcode == true
                && StringUtil.IsIdcardNumber(this.textBox_readerBarcode.Text) == false
                && IsName(this.textBox_readerBarcode.Text) == false)
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
                    this.textBox_readerBarcode.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    string strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        "您输入的条码 " + this.textBox_readerBarcode.Text + " 格式不正确(" + strError + ")。请重新输入。",
                        InfoColor.Red,
                        "装载读者记录",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    this.SwitchFocus(READER_BARCODE, strFastInputText);
                    return;
                }

                // 实际输入的是册条码号
                if (nRet == 2)
                {
                    string strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        "您输入的条码号 " + this.textBox_readerBarcode.Text + " 是册条码号。请输入读者证条码号。",
                        InfoColor.Red,
                        "装载读者记录",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    this.SwitchFocus(READER_BARCODE, strFastInputText);
                    return;
                }

                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2)
                    MessageBox.Show(this, "警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
            }

            // this.oneReaderItemBarcodes.Clear();
            this.oneReaderItemBarcodes = new List<string>();    // 2014/12/22

            this.m_strCurrentBarcode = this.textBox_readerBarcode.Text;

            // 搜索出记录，显示在窗口中
            // return:
            //      -1  error
            //      0   没有找到
            //      1   成功
            //      2   放弃
            nRet = LoadReaderRecord(ref m_strCurrentBarcode,
                out strError);
            if (this.m_strCurrentBarcode != this.textBox_readerBarcode.Text)
            {
                this.textBox_readerBarcode.Text = this.m_strCurrentBarcode;
                this.ActiveReaderBarcode = this.textBox_readerBarcode.Text;
            }

            if (nRet == -1)
            {
                string strFastInputText = ChargingInfoDlg.Show(
                    this.CharingInfoHost,
                    strError,
                    InfoColor.Red,
                    "装载读者记录",
                    this.InfoDlgOpacity,
                    Program.MainForm.DefaultFont);
                /*
                SetReaderRenderString(this.webBrowser_reader,
                    "装载读者记录发生错误: " + strError);
                 * */
                // 输入焦点仍然回到读者证条码号输入域
                /*
                this.textBox_readerBarcode.SelectAll();
                this.textBox_readerBarcode.Focus();
                 * */
                this.SwitchFocus(READER_BARCODE, strFastInputText);
            }
            else if (nRet == 0)
            {
                if (this.Channel.ErrorCode == ErrorCode.IdcardNumberNotFound)
                    strError = "读者身份证号(或证条码号) " + this.textBox_readerBarcode.Text + " 不存在。\r\n\r\n如果是使用身份证第一次借书，请先用身份证快速创建读者记录";
                else
                    strError = "读者证条码号 " + this.textBox_readerBarcode.Text + " 不存在。";
                string strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        strError,
                        InfoColor.Red,
                        "装载读者记录",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);

                // 输入焦点仍然回到读者证条码号输入域
                /*
                this.textBox_readerBarcode.SelectAll();
                this.textBox_readerBarcode.Focus();
                 * */
                this.SwitchFocus(READER_BARCODE, strFastInputText);
            }
            else if (nRet == 2)
            {
                // 放弃装入 
                return;
            }
            else
            {
                Debug.Assert(nRet == 1, "");
                // 转移输入焦点
                /*
                if (this.textBox_readerPassword.Enabled == true)
                {
                    this.textBox_readerPassword.SelectAll();
                    this.textBox_readerPassword.Focus();
                }
                else
                {
                    this.textBox_itemBarcode.SelectAll();
                    this.textBox_itemBarcode.Focus();
                }
                */
                if (this.textBox_readerPassword.Visible == true // 2011/12/5
                    && this.textBox_readerPassword.Enabled == true)
                {
                    this.SwitchFocus(READER_PASSWORD, null);
                }
                else
                {
                    this.SwitchFocus(ITEM_BARCODE, null);
                }

                Debug.Assert(Program.MainForm.OperHistory != null, "Program.MainForm.OperHistory == null");

                // 触发操作历史动作
                Program.MainForm.OperHistory.ReaderBarcodeScaned(
                    this.textBox_readerBarcode.Text);
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
            if (this.FuncState == FuncState.Return)
                this.SwitchFocus(ITEM_BARCODE, "");
            else
                this.SwitchFocus(READER_BARCODE, "");
            return;
        }

        // 
        /// <summary>
        /// 打印借还凭条
        /// </summary>
        public void Print()
        {
            // 触发历史动作
            Program.MainForm.OperHistory.Print();
        }

        /*
        // 测试打印借还凭条
        public void TestPrint()
        {
            // 触发历史动作
            Program.MainForm.OperHistory.TestPrint();
        }*/

        void EnableEdit(int target,
            bool bEnable)
        {
            API.PostMessage(this.Handle, WM_ENABLE_EDIT,
                target, bEnable == true ? 1 : 0);
        }

        void SwitchFocus(int target,
            string strFastInput)
        {
            // 提防hashtable越来越大
            if (this.m_textTable.Count > 5)
            {
                Debug.Assert(false, "");
                this.m_textTable.Clear();
            }

            int nNumber = -1;   // -1表示不需要传递字符串参数

            // 如果需要传递字符串参数
            if (String.IsNullOrEmpty(strFastInput) == false)
            {
                string strNumber = this.m_nTextNumber.ToString();
                nNumber = this.m_nTextNumber;
                this.m_nTextNumber++;
                if (this.m_nTextNumber == -1)   // 避开-1
                    this.m_nTextNumber++;

                this.m_textTable[strNumber] = strFastInput;
            }

            API.PostMessage(this.Handle, WM_SWITCH_FOCUS,
                target, nNumber);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOAD_READER:
                    {
                        if (this.m_webExternalHost_readerInfo.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            DoLoadReader();
                        }
                    }
                    return;
                case WM_LOAD_ITEM:
                    {
                        if (this.m_webExternalHost_itemInfo.CanCallNew(
                            this.commander,
                            m.Msg) == true)
                        {
                            DoItemAction();
                        }
                    }
                    return;
                /*
            case WM_LOADSIZE:
                LoadSize();
                return;
                 * */
                case WM_ENABLE_EDIT:
                    {
                        int nOn = (int)m.LParam;
                        if ((int)m.WParam == READER_BARCODE)
                        {
                            if (nOn == 1)
                            {
                                if (this.textBox_readerBarcode.Enabled == false)
                                {
                                    this.textBox_readerBarcode.Enabled = true;
                                    this.button_loadReader.Enabled = true;
                                }
                            }
                            else
                            {
                                if (this.textBox_readerBarcode.Enabled == true)
                                {
                                    this.textBox_readerBarcode.Enabled = false;
                                    this.button_loadReader.Enabled = false;
                                }
                            }
                        }
                        if ((int)m.WParam == ITEM_BARCODE)
                        {
                            if (nOn == 1)
                            {
                                if (this.textBox_itemBarcode.Enabled == false)
                                    this.textBox_itemBarcode.Enabled = true;
                            }
                            else
                            {
                                if (this.textBox_itemBarcode.Enabled == true)
                                    this.textBox_itemBarcode.Enabled = false;
                            }
                        }
                    }
                    return;

                case WM_SWITCH_FOCUS:
                    {
                        string strFastInputText = "";
                        int nNumber = (int)m.LParam;

                        if (nNumber != -1)
                        {
                            string strNumber = nNumber.ToString();
                            strFastInputText = (string)this.m_textTable[strNumber];
                            this.m_textTable.Remove(strNumber);
                        }

                        if (String.IsNullOrEmpty(strFastInputText) == false)
                        {
                            if ((int)m.WParam == READER_BARCODE)
                            {
                                if (this.FuncState == FuncState.Return)
                                    this.FuncState = FuncState.Borrow;

                                this.textBox_readerBarcode.Text = strFastInputText;

                                // 2009/6/2 
                                if (this.button_loadReader.Enabled == false)
                                    this.button_loadReader.Enabled = true;
                                /*
                                // 2009/11/8 
                                if (this.textBox_readerBarcode.Enabled == false)
                                    this.textBox_readerBarcode.Enabled = true;
                                 * */

                                this.button_loadReader_Click(this, null);
                            }
                            if ((int)m.WParam == READER_PASSWORD)
                            {
                                this.textBox_readerPassword.Text = strFastInputText;

                                // 2009/6/2 
                                if (this.button_verifyReaderPassword.Enabled == false)
                                    this.button_verifyReaderPassword.Enabled = true;
                                /*
                                // 2009/11/8 
                                if (this.textBox_readerPassword.Enabled == false)
                                    this.textBox_readerPassword.Enabled = true;
                                 * */

                                this.button_verifyReaderPassword_Click(this, null);
                            }
                            if ((int)m.WParam == ITEM_BARCODE)
                            {
                                this.textBox_itemBarcode.Text = strFastInputText;

                                // 2009/6/2 
                                if (this.button_itemAction.Enabled == false)
                                    this.button_itemAction.Enabled = true;
                                /*
                                // 2009/11/8 
                                if (this.textBox_itemBarcode.Enabled == false)
                                    this.textBox_itemBarcode.Enabled = true;
                                 * */

                                this.button_itemAction_Click(this, null);
                            }

                            return;
                        }

                        if ((int)m.WParam == READER_BARCODE)
                        {
                            // 2009/6/2 
                            if (this.button_loadReader.Enabled == false)
                                this.button_loadReader.Enabled = true;
                            // 2009/11/8 
                            if (this.textBox_readerBarcode.Enabled == false)
                                this.textBox_readerBarcode.Enabled = true;

                            /*
                            // ???
                            if (this.FuncState == FuncState.Return)
                                this.FuncState = FuncState.Borrow;
                             * */
                            this.textBox_readerBarcode.SelectAll();
                            this.textBox_readerBarcode.Focus();
                        }

                        if ((int)m.WParam == READER_PASSWORD)
                        {
                            // 2009/6/2 
                            if (this.button_verifyReaderPassword.Enabled == false)
                                this.button_verifyReaderPassword.Enabled = true;
                            // 2009/11/8 
                            if (this.textBox_readerPassword.Enabled == false)
                                this.textBox_readerPassword.Enabled = true;

                            this.textBox_readerPassword.SelectAll();
                            this.textBox_readerPassword.Focus();
                        }

                        if ((int)m.WParam == ITEM_BARCODE)
                        {
                            // 2009/6/2 
                            if (this.button_itemAction.Enabled == false)
                                this.button_itemAction.Enabled = true;
                            // 2009/11/8 
                            if (this.textBox_itemBarcode.Enabled == false)
                                this.textBox_itemBarcode.Enabled = true;

                            this.textBox_itemBarcode.SelectAll();
                            this.textBox_itemBarcode.Focus();
                        }

                        return;
                    }
                    // break;
            }
            base.DefWndProc(ref m);
        }

        //
        /// <summary>
        ///  清除书目和实体两个浏览器控件中的内容
        /// </summary>
        public void ClearItemAndBiblioControl()
        {
            SetBiblioRenderString("(空)");
            SetItemRenderString("(空)");
        }

#if NOOOOOOOOOOO
        void NewExternal()
        {
            if (this.m_webExternalHost != null)
            {
                /*
                if (this.m_webExternalHost.IsInLoop == false)
                    return; // 如果不在循环中，则继续用这个对象
                 * */

                this.m_webExternalHost.Destroy();
                this.webBrowser_reader.ObjectForScripting = null;
            }

            this.m_webExternalHost = new WebExternalHost();
            this.m_webExternalHost.Initial(Program.MainForm);
            this.webBrowser_reader.ObjectForScripting = this.m_webExternalHost;
        }
#endif

        private void button_itemAction_Click(object sender, EventArgs e)
        {
            this.button_itemAction.Enabled = false;
            this.textBox_itemBarcode.Enabled = false;   // 2009/10/20 

            this.m_webExternalHost_itemInfo.StopPrevious();
            this.webBrowser_item.Stop();

            // 2009/10/20 
            this.m_webExternalHost_readerInfo.StopPrevious();
            this.webBrowser_reader.Stop();
            this.m_webExternalHost_biblioInfo.StopPrevious();
            this.webBrowser_biblio.Stop();

            this.commander.AddMessage(WM_LOAD_ITEM);
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
            else if (func == dp2Circulation.FuncState.VerifyRenew)
            {
                dlg.FunctionType = "renew";
                dlg.Text = "请选择要续借的册";
            }
            else if (func == dp2Circulation.FuncState.Return || func == dp2Circulation.FuncState.Lost)
            {
                dlg.FunctionType = "return";
                dlg.Text = "请选择要还回的册";
            }
            else if (func == dp2Circulation.FuncState.VerifyReturn || func == dp2Circulation.FuncState.VerifyLost)
            {
                dlg.FunctionType = "return";
                dlg.VerifyBorrower = this.textBox_readerBarcode.Text;
                dlg.Text = "请选择要(验证)还回的册";
            }

            dlg.AutoOperSingleItem = this.AutoOperSingleItem;
            dlg.AutoSearch = true;
            dlg.MainForm = Program.MainForm;
            dlg.From = "ISBN";
            dlg.QueryWord = strText;

            dlg.UiState = Program.MainForm.AppInfo.GetString(
        "ChargingForm",
        "SelectItemDialog_uiState",
        "");

            // TODO: 保存窗口内的尺寸状态
            Program.MainForm.AppInfo.LinkFormState(dlg, "ChargingForm_SelectItemDialog_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
"ChargingForm",
"SelectItemDialog_uiState",
dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return 0;

            Debug.Assert(string.IsNullOrEmpty(dlg.SelectedItemBarcode) == false, "");
            strItemBarcode = dlg.SelectedItemBarcode;
            return 1;
        }


        /// <summary>
        /// 执行动作按钮代表的动作
        /// </summary>
        /// <returns>-1: 出错; 0: 功能没有执行; 1: 功能已经执行</returns>
        public int DoItemAction()
        {
            string strError = "";
            int nRet = 0;

            DateTime start_time = DateTime.Now;

#if NO
            // 如果册条码号为空，在这里回车可以被当作切换到读者证条码号输入域？
            if (this.textBox_itemBarcode.Text == "")
            {
                MessageBox.Show(this, "尚未输入册条码号");
                this.SwitchFocus(ITEM_BARCODE, null);
                return -1;
            }
#endif

            if (this.AutoToUpper == true)
                this.textBox_itemBarcode.Text = this.textBox_itemBarcode.Text.ToUpper();

            this.ActiveReaderBarcode = this.textBox_readerBarcode.Text;

            BarcodeAndTime barcodetime = null;

            if (this.DoubleItemInputAsEnd == true)
            {
                // 取出上次输入的最后一个条码，和目前输入的条码比较，看是否一样。
                if (this.m_itemBarcodes.Count > 0)
                {
                    string strLastItemBarcode = this.m_itemBarcodes[m_itemBarcodes.Count - 1].Barcode;
                    TimeSpan delta = DateTime.Now - this.m_itemBarcodes[m_itemBarcodes.Count - 1].Time;
                    // MessageBox.Show(this, delta.TotalMilliseconds.ToString());
                    if (strLastItemBarcode == this.textBox_itemBarcode.Text
                        && delta.TotalMilliseconds < 5000) // 5秒以内
                    {
                        // 清除册条码号输入域
                        this.textBox_itemBarcode.Text = "";

                        // 如果当前在“还”状态，需要修改为“验证还”，避免读者证条码号输入域为diable而无法切换焦点过去
                        if (this.FuncState == FuncState.Return)
                            this.FuncState = FuncState.VerifyReturn;


                        // 清除读者证条码号输入域
                        this.textBox_readerBarcode.Text = "请输入下一个读者的证条码号...";
                        this.SwitchFocus(READER_BARCODE, null);
                        return 0;
                    }
                }
                barcodetime = new BarcodeAndTime();
                barcodetime.Barcode = this.textBox_itemBarcode.Text;
                barcodetime.Time = DateTime.Now;

                this.m_itemBarcodes.Add(barcodetime);
                // 仅仅保持一个条码就可以了
                while (this.m_itemBarcodes.Count > 1)
                    this.m_itemBarcodes.RemoveAt(0);
            }

            string strFastInputText = "";

            string strTemp = this.textBox_itemBarcode.Text;
            if ((this.UseIsbnBorrow == true && QuickChargingForm.IsISBN(ref strTemp) == true)
                || strTemp.ToLower() == "?b"
                || string.IsNullOrEmpty(strTemp) == true)
            {
                string strItemBarcode = "";
                // return:
                //      -1  error
                //      0   放弃
                //      1   成功
                nRet = SelectOneItem(this.FuncState,
                    strTemp.ToLower() == "?b" ? "" : strTemp,
                    out strItemBarcode,
                    out strError);
                if (nRet == -1)
                {
                    strError = "选择册记录的过程中出错: " + strError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        "已取消选择册记录。注意操作并未执行",
                        InfoColor.Red,
                        "扫入册条码",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    this.SwitchFocus(ITEM_BARCODE, strFastInputText);
                    return -1;
                }

                this.textBox_itemBarcode.Text = strItemBarcode;
            }

            this.ActiveItemBarcode = this.textBox_itemBarcode.Text;

            // 2017/1/4
            // 变换条码号
            // return:
            //      -1  出错
            //      0   不需要进行变换
            //      1   需要进行变换
            nRet = Program.MainForm.NeedTransformBarcode(
                Program.MainForm.FocusLibraryCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                string strText = this.textBox_itemBarcode.Text;

                nRet = Program.MainForm.TransformBarcode(
                    Program.MainForm.FocusLibraryCode,
                    ref strText,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_itemBarcode.Text = strText;
            }

            if (this.NeedVerifyBarcode == true)
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
                    this.textBox_itemBarcode.Text,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        "您输入的条码 " + this.textBox_itemBarcode.Text + " 格式不正确(" + strError + ")。请重新输入。",
                        InfoColor.Red,
                        "扫入册条码",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    this.SwitchFocus(ITEM_BARCODE, strFastInputText);
                    return -1;
                }

                // 发现实际输入的是读者证条码号
                if (nRet == 1)
                {
                    // 2008/1/2 
                    if (this.AutoSwitchReaderBarcode == true)
                    {
                        string strItemBarcode = this.textBox_itemBarcode.Text;
                        this.textBox_itemBarcode.Text = "";

                        // 如果当前在“还”或者“验证还”状态，需要修改为“借”?
                        if (this.FuncState == FuncState.Return
                            || this.FuncState == FuncState.VerifyReturn)
                            this.FuncState = FuncState.Borrow;
                        else
                            this.FuncState = FuncState.VerifyReturn;

                        // 直接跨越而去执行借阅功能
                        this.textBox_readerBarcode.Text = strItemBarcode;
                        this.button_loadReader_Click(null, null);

                        // this.SwitchFocus(READER_BARCODE, strItemBarcode);
                        return 0;
                    }

                    // 刻板式，要求必须类型匹配
                    strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        "您输入的条码号 " + this.textBox_itemBarcode.Text + " 是读者证条码号。请输入册条码号。",
                        InfoColor.Red,
                        "扫入册条码",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    this.SwitchFocus(ITEM_BARCODE, strFastInputText);
                    return -1;
                }

                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2)
                    MessageBox.Show(this, "警告：前端开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");
            }

            EnableControls(false);

            try
            {
                if (this.NoBiblioAndItemInfo == false)
                {
                    // 借阅操作前装入册记录
                    nRet = LoadItemAndBiblioRecord(this.textBox_itemBarcode.Text,
                        out strError);
                    if (nRet == 0)
                    {
                        strError = "册条码号 '" + this.textBox_itemBarcode.Text + "' 没有找到";
                        goto ERROR1;
                    }

                    if (nRet == -1)
                        goto ERROR1;
                }

                long lRet = 0;

                // 借/续借
                if (this.FuncState == FuncState.Borrow
                    || this.FuncState == FuncState.Renew
                    || this.FuncState == FuncState.VerifyRenew)
                {
                    string strOperName = "";

                    if (this.FuncState == FuncState.Borrow)
                    {
                        strOperName = "借阅";
                    }
                    else if (this.FuncState == FuncState.VerifyRenew)
                    {
                        strOperName = "续借";
                    }

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在进行" + strOperName + "操作: " + this.textBox_readerBarcode.Text
                    + " " + strOperName + " " + this.textBox_itemBarcode.Text + " ...");
                    stop.BeginLoop();

                    if (this.NoBiblioAndItemInfo == false)
                    {
                        // 清除书目和实体信息
                        SetBiblioRenderString("(空)");
                        SetItemRenderString("(空)");
                    }

                    try
                    {
                        string strReaderRecord = "";
                        string strConfirmItemRecPath = null;

                        bool bRenew = false;
                        if (this.FuncState == FuncState.VerifyRenew
                            || this.FuncState == dp2Circulation.FuncState.Renew)
                            bRenew = true;

                        REDO:
                        string[] aDupPath = null;
                        string[] item_records = null;
                        string[] reader_records = null;
                        string[] biblio_records = null;
                        string strOutputReaderBarcode = "";

                        BorrowInfo borrow_info = null;

                        // item返回的格式
                        string strItemReturnFormats = "";
                        // 2008/5/9 有必要才返回item信息
                        if (this.NoBiblioAndItemInfo == false)
                            strItemReturnFormats = this.RenderFormat;
                        if (Program.MainForm.ChargingNeedReturnItemXml == true)
                        {
                            if (String.IsNullOrEmpty(strItemReturnFormats) == false)
                                strItemReturnFormats += ",";
                            strItemReturnFormats += "xml";
                        }

                        // biblio返回的格式
                        string strBiblioReturnFormats = "";
                        if (this.NoBiblioAndItemInfo == false)
                            strBiblioReturnFormats = this.RenderFormat;

                        string strStyle = "reader";
                        if (this.NoBiblioAndItemInfo == false)
                            strStyle += ",item,biblio";
                        else if (Program.MainForm.ChargingNeedReturnItemXml)
                            strStyle += ",item";

                        //if (Program.MainForm.TestMode == true)
                        //    strStyle += ",testmode";

                        lRet = Channel.Borrow(
                            stop,
                            bRenew,
                            this.FuncState == dp2Circulation.FuncState.Renew ? "" : this.textBox_readerBarcode.Text,
                            this.textBox_itemBarcode.Text,
                            strConfirmItemRecPath,
                            this.Force,
                            this.OneReaderItemBarcodes,
                            strStyle,   //this.NoBiblioAndItemInfo == false ? "reader,item,biblio" : "reader",
                            strItemReturnFormats, // this.RenderFormat, // "html",
                            out item_records,   // strItemRecord,
                            this.PatronRenderFormat + ",xml", // "html",
                            out reader_records, // strReaderRecord,
                            strBiblioReturnFormats,
                            out biblio_records,
                            out aDupPath,
                            out strOutputReaderBarcode,
                            out borrow_info,
                            out strError);

                        if (reader_records != null && reader_records.Length > 0)
                            strReaderRecord = reader_records[0];

                        // 刷新读者信息
                        if (String.IsNullOrEmpty(strReaderRecord) == false)
                            SetReaderRenderString(ReplaceMacro(strReaderRecord));

                        // 显示书目和实体信息
                        if (this.NoBiblioAndItemInfo == false)
                        {
                            string strItemRecord = "";
                            if (item_records != null && item_records.Length > 0)
                                strItemRecord = item_records[0];
                            if (String.IsNullOrEmpty(strItemRecord) == false)
                                this.SetItemRenderString(ReplaceMacro(strItemRecord));

                            string strBiblioRecord = "";
                            if (biblio_records != null && biblio_records.Length > 0)
                                strBiblioRecord = biblio_records[0];
                            if (String.IsNullOrEmpty(strBiblioRecord) == false)
                                this.SetBiblioRenderString(ReplaceMacro(strBiblioRecord));
                        }

                        string strItemXml = "";
                        if (Program.MainForm.ChargingNeedReturnItemXml == true
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
                        {
                            // 清除记忆的册条码号
                            this.m_itemBarcodes.Clear();

                            if (Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.ItemBarcodeDup)
                            {
                                // Program.MainForm.PrepareSearch();
                                LibraryChannel channel = Program.MainForm.GetChannel();
                                try
                                {
                                    ItemBarcodeDupDlg dupdlg = new ItemBarcodeDupDlg();
                                    MainForm.SetControlFont(dupdlg, this.Font, false);
                                    string strErrorNew = "";
                                    nRet = dupdlg.Initial(
                                        // Program.MainForm,
                                        aDupPath,
                                        "因册条码号发生重复，" + strOperName + "操作被拒绝。\r\n\r\n可根据下面列出的详细信息，选择适当的册记录，重试操作。\r\n\r\n原始出错信息:\r\n" + strError,
                                        channel,    // Program.MainForm.Channel,
                                        Program.MainForm.Stop,
                                        out strErrorNew);
                                    if (nRet == -1)
                                    {
                                        // 初始化对话框失败
                                        MessageBox.Show(this, strErrorNew);
                                        goto ERROR1;
                                    }

                                    Program.MainForm.AppInfo.LinkFormState(dupdlg, "ChargingForm_dupdlg_state");
                                    dupdlg.ShowDialog(this);
                                    Program.MainForm.AppInfo.UnlinkFormState(dupdlg);

                                    if (dupdlg.DialogResult == DialogResult.Cancel)
                                        goto ERROR1;

                                    strConfirmItemRecPath = dupdlg.SelectedRecPath;

                                    goto REDO;
                                }
                                finally
                                {
                                    Program.MainForm.ReturnChannel(channel);
                                    // Program.MainForm.EndSearch();
                                }
                            }

                            goto ERROR1;
                        }

                        /*
                         * 属于多余的操作? 2008/5/9 去除
                        if (String.IsNullOrEmpty(strConfirmItemRecPath) == false
                            && this.NoBiblioAndItemInfo == false)
                        {
                            // 借阅操作后装入准确的册记录
                            string strError_1 = "";
                            int nRet_1 = LoadItemAndBiblioRecord("@path:" + strConfirmItemRecPath,
                                out strError_1);
                            if (nRet == -1)
                            {
                                strError_1 = "册记录 '" + strConfirmItemRecPath + "' 没有找到";
                                MessageBox.Show(this, strError);
                            }
                        }
                        */

                        DateTime end_time = DateTime.Now;

                        string strReaderSummary = "";
                        if (reader_records != null && reader_records.Length > 1)
                        {
                            /*
                            // 2012/1/5
                            // 加入缓存
                            Program.MainForm.SetReaderXmlCache(strOutputReaderBarcode,
                                "",
                                reader_records[1]);
                             * */
                            strReaderSummary = Global.GetReaderSummary(reader_records[1]);
                        }

                        Program.MainForm.OperHistory.BorrowAsync(
                            this,
                            bRenew,
                            strOutputReaderBarcode,
                            this.textBox_itemBarcode.Text,
                            strConfirmItemRecPath,
                            strReaderSummary,
                            strItemXml,
                            borrow_info,
                            start_time,
                            end_time);

                    }
                    finally
                    {
                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                    }

                    // 累积同一读者借阅成功的册条码号
                    this.oneReaderItemBarcodes.Add(this.textBox_itemBarcode.Text);

                    if (lRet == 1)
                    {
                        // 有重复册条码号
                        strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                            strError.Replace("\r\n", "\r\n\r\n"),
                            InfoColor.Yellow,
                            strOperName,    // "caption",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    }
                    else
                    {
                        if (this.GreenDisable == false)
                        {
                            strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                                strOperName + "成功",
                                InfoColor.Green,
                                strOperName,    // "caption",
                            this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                        }
                    }
                }
                else if (this.FuncState == FuncState.Return
                    || this.FuncState == FuncState.VerifyReturn
                    || this.FuncState == FuncState.Lost)
                {
                    string strAction = "";
                    string strLocation = "";    // 还回的册的馆藏地点

                    if (this.FuncState == FuncState.Return)
                        strAction = "return";
                    if (this.FuncState == FuncState.VerifyReturn)
                        strAction = "return";
                    if (this.FuncState == FuncState.Lost)
                        strAction = "lost";

                    Debug.Assert(strAction != "", "");

                    string strOperName = "";

                    if (this.FuncState == FuncState.Return)
                    {
                        strOperName = "还书";
                    }
                    else if (this.FuncState == FuncState.VerifyReturn)
                    {
                        strOperName = "验证还书";
                    }
                    else
                    {
                        strOperName = "丢失";
                    }

                    // 还书
                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在进行 " + strOperName + " 操作: " + this.textBox_readerBarcode.Text
                    + " 还 " + this.textBox_itemBarcode.Text + " ...");
                    stop.BeginLoop();

                    if (this.NoBiblioAndItemInfo == false)
                    {
                        // 清除书目和实体信息
                        SetBiblioRenderString("(空)");
                        SetItemRenderString("(空)");
                    }

                    try
                    {
                        string strReaderRecord = "";
                        string strConfirmItemRecPath = null;
                        string strOutputReaderBarcode = "";

                        REDO:
                        string[] aDupPath = null;
                        string[] item_records = null;
                        string[] reader_records = null;
                        string[] biblio_records = null;

                        ReturnInfo return_info = null;

                        // item返回的格式 2008/5/9
                        string strItemReturnFormats = "";
                        if (this.NoBiblioAndItemInfo == false)
                            strItemReturnFormats = this.RenderFormat;
                        if (Program.MainForm.ChargingNeedReturnItemXml == true)
                        {
                            if (String.IsNullOrEmpty(strItemReturnFormats) == false)
                                strItemReturnFormats += ",";
                            strItemReturnFormats += "xml";
                        }

                        // biblio返回的格式
                        string strBiblioReturnFormats = "";
                        if (this.NoBiblioAndItemInfo == false)
                            strBiblioReturnFormats = this.RenderFormat;

                        string strStyle = "reader";
                        if (this.NoBiblioAndItemInfo == false)
                            strStyle += ",item,biblio";
                        else if (Program.MainForm.ChargingNeedReturnItemXml)
                            strStyle += ",item";

                        //if (Program.MainForm.TestMode == true)
                        //    strStyle += ",testmode";

                        lRet = Channel.Return(
                            stop,
                            strAction,
                            this.textBox_readerBarcode.Text,
                            this.textBox_itemBarcode.Text,
                            strConfirmItemRecPath,
                            this.Force,
                            strStyle,   // this.NoBiblioAndItemInfo == false ? "reader,item,biblio" : "reader",
                            strItemReturnFormats,
                            out item_records,
                            this.PatronRenderFormat + ",xml", // "html",
                            out reader_records,
                            strBiblioReturnFormats,
                            out biblio_records,
                            out aDupPath,
                            out strOutputReaderBarcode,
                            out return_info,
                            out strError);
                        if (lRet == -1)
                        {
                            // 清除记忆的册条码号
                            this.m_itemBarcodes.Clear();

                            if (Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.ItemBarcodeDup)
                            {
                                // Program.MainForm.PrepareSearch();
                                LibraryChannel channel = Program.MainForm.GetChannel();
                                try
                                {
                                    ItemBarcodeDupDlg dupdlg = new ItemBarcodeDupDlg();
                                    MainForm.SetControlFont(dupdlg, this.Font, false);
                                    string strErrorNew = "";
                                    nRet = dupdlg.Initial(
                                        // Program.MainForm,
                                        aDupPath,
                                        "因册条码号发生重复，还回操作被拒绝。\r\n\r\n可根据下面列出的详细信息，选择适当的册记录，重试操作。\r\n\r\n原始出错信息:\r\n" + strError,
                                        channel,    // Program.MainForm.Channel,
                                        Program.MainForm.Stop,
                                        out strErrorNew);
                                    if (nRet == -1)
                                    {
                                        // 初始化对话框失败
                                        MessageBox.Show(this, strErrorNew);
                                        goto ERROR1;
                                    }

                                    Program.MainForm.AppInfo.LinkFormState(dupdlg, "ChargingForm_dupdlg_state");
                                    dupdlg.ShowDialog(this);
                                    Program.MainForm.AppInfo.UnlinkFormState(dupdlg);

                                    if (dupdlg.DialogResult == DialogResult.Cancel)
                                        goto ERROR1;

                                    strConfirmItemRecPath = dupdlg.SelectedRecPath;

                                    goto REDO;
                                }
                                finally
                                {
                                    Program.MainForm.ReturnChannel(channel);
                                    // Program.MainForm.EndSearch();
                                }
                            }

                            goto ERROR1;
                        }

                        if (return_info != null)
                        {
                            strLocation = StringUtil.GetPureLocation(return_info.Location);
                        }

                        // 确定还书的读者证条码号
                        this.ActiveReaderBarcode = strOutputReaderBarcode;

                        if (reader_records != null && reader_records.Length > 0)
                            strReaderRecord = reader_records[0];

                        string strReaderSummary = "";
                        if (reader_records != null && reader_records.Length > 1)
                        {
                            /*
                            // 2012/1/5
                            // 加入缓存
                            Program.MainForm.SetReaderXmlCache(strOutputReaderBarcode,
                                "",
                                reader_records[1]);
                             * */
                            strReaderSummary = Global.GetReaderSummary(reader_records[1]);
                        }

                        // 刷新读者信息
                        SetReaderRenderString(ReplaceMacro(strReaderRecord));

                        // 显示书目和实体信息
                        if (this.NoBiblioAndItemInfo == false)
                        {
                            string strItemRecord = "";
                            if (item_records != null && item_records.Length > 0)
                                strItemRecord = item_records[0];
                            if (String.IsNullOrEmpty(strItemRecord) == false)
                                SetItemRenderString(ReplaceMacro(strItemRecord));

                            string strBiblioRecord = "";
                            if (biblio_records != null && biblio_records.Length > 0)
                                strBiblioRecord = biblio_records[0];
                            if (String.IsNullOrEmpty(strBiblioRecord) == false)
                                this.SetBiblioRenderString(ReplaceMacro(strBiblioRecord));

                            /*
                            {
                                // 以前的方法，要多用一次API

                                string strError_1 = "";
                                int nRet_1 = 0;

                                if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                                {
                                    nRet_1 = LoadItemAndBiblioRecord("@path:" + strConfirmItemRecPath,
                                         out strError_1);
                                }
                                else if (aDupPath != null && aDupPath.Length == 1)
                                {
                                    nRet_1 = LoadItemAndBiblioRecord("@path:" + aDupPath[0],
                                         out strError_1);
                                }
                                else
                                {
                                    nRet_1 = LoadItemAndBiblioRecord(this.textBox_itemBarcode.Text,
                                         out strError_1);
                                }

                                if (nRet_1 == -1)
                                    MessageBox.Show(this, strError_1);
                            }
                             * */
                        }

                        string strItemXml = "";
                        if (Program.MainForm.ChargingNeedReturnItemXml == true
                            && item_records != null)
                        {
                            if (item_records.Length > 0)
                            {
                                // xml总是在最后一个
                                strItemXml = item_records[item_records.Length - 1];
                            }
                        }

                        DateTime end_time = DateTime.Now;

                        Program.MainForm.OperHistory.ReturnAsync(
                            this,
                            strAction,  // this.FuncState == FuncState.Lost,
                            strOutputReaderBarcode, // this.textBox_readerBarcode.Text,
                            this.textBox_itemBarcode.Text,
                            strConfirmItemRecPath,
                            strReaderSummary,
                            strItemXml,
                            return_info,
                            start_time,
                            end_time);

                    }
                    finally
                    {
                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                    }

                    if (lRet == 1)
                    {
                        // 超期情况/放入预约架/有重复册条码号
                        strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                            strError.Replace("\r\n", "\r\n\r\n"),
                            InfoColor.Yellow,
                            strOperName,    // "caption",
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                    }
                    else
                    {
                        if (this.GreenDisable == false)
                        {
                            string strText = "还书成功";
                            if (string.IsNullOrEmpty(strLocation) == false)
                                strText += "\r\n\r\n馆藏地: " + strLocation;
                            strFastInputText = ChargingInfoDlg.Show(
                                this.CharingInfoHost,
                                strText,
                                InfoColor.Green,
                                strOperName,    // "caption",
                            this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                        }
                    }
                } // endif if 还书
                else
                {
                    strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                        "暂不支持",
                        InfoColor.Red,
                        "caption",  // 
                        this.InfoDlgOpacity,
                        Program.MainForm.DefaultFont);
                }

            }
            finally
            {
                EnableControls(true);
            }

            // 焦点回到册条码号输入域
            this.SwitchFocus(ITEM_BARCODE, strFastInputText);
            return 1;
            ERROR1:
            strFastInputText = ChargingInfoDlg.Show(
                this.CharingInfoHost,
                strError,
                InfoColor.Red,
                "caption",
                this.InfoDlgOpacity,
                Program.MainForm.DefaultFont);
            EnableControls(true);

            // 焦点回到册条码号textbox
            /*
            this.textBox_itemBarcode.SelectAll();
            this.textBox_itemBarcode.Focus();
             * */
            this.SwitchFocus(ITEM_BARCODE, strFastInputText);
            return -1;
        }

        /*
        public const int READER_BARCODE = 0;
        public const int READER_PASSWORD = 1;
        public const int ITEM_BARCODE = 2;
         * */
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
                Program.MainForm.DefaultFont);

            this.SwitchFocus(nTarget, strFastInputText);
        }

        // 按钮上的右鼠标popupmenu: 借
        private void toolStripMenuItem_borrow_Click(object sender, EventArgs e)
        {
            // this.FuncState = FuncState.Borrow;
            SmartSetFuncState(FuncState.Borrow,
                false,
                false);
        }

        private void toolStripMenuItem_return_Click(object sender, EventArgs e)
        {
            // this.FuncState = FuncState.Return;
            SmartSetFuncState(FuncState.Return,
                false,
                false);

        }

        private void toolStripMenuItem_verifyReturn_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.VerifyReturn;
        }

        private void toolStripMenuItem_verifyRenew_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.VerifyRenew;
        }

        private void toolStripMenuItem_renew_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Renew;
        }

        private void toolStripMenuItem_lost_Click(object sender, EventArgs e)
        {
            this.FuncState = FuncState.Lost;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_itemBarcode.Enabled = bEnable;
            this.button_itemAction.Enabled = bEnable;
            // this.checkBox_force.Enabled = bEnable;

#if NO
            if (this.VerifyReaderPassword == true)
            {
                this.textBox_readerPassword.Enabled = bEnable;
                this.button_verifyReaderPassword.Enabled = bEnable;
            }
#endif
            if (this.textBox_readerPassword.Visible == true)
            {
                this.textBox_readerPassword.Enabled = bEnable;
                this.button_verifyReaderPassword.Enabled = bEnable;
            }

            if (this.FuncState == FuncState.Return)
            {
                this.textBox_readerBarcode.Enabled = false;
                this.button_loadReader.Enabled = false;
            }
            else
            {
                this.textBox_readerBarcode.Enabled = bEnable;
                this.button_loadReader.Enabled = bEnable;
            }

        }

        /// <summary>
        /// 册条码输入域内容
        /// </summary>
        public string ItemBarcode
        {
            get
            {
                return this.textBox_itemBarcode.Text;
            }
            set
            {
                this.textBox_itemBarcode.Text = value;
            }
        }

        /// <summary>
        /// 读者证条码号输入域内容
        /// </summary>
        public string ReaderBarcode
        {
            get
            {
                return this.textBox_readerBarcode.Text;
            }
            set
            {
                this.textBox_readerBarcode.Text = value;
            }
        }

        private void button_verifyReaderPassword_Click(object sender, EventArgs e)
        {
            string strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在校验读者密码 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            this.EnableControls(false);

            try
            {
                long lRet = Channel.VerifyReaderPassword(
                    stop,
                    this.textBox_readerBarcode.Text,
                    this.textBox_readerPassword.Text,
                    out strError);
                if (lRet == 0)
                {
                    goto ERROR1;
                }
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                this.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            /*
            this.textBox_itemBarcode.SelectAll();
            this.textBox_itemBarcode.Focus();
             * */
            // 校验正确
            this.SwitchFocus(ITEM_BARCODE, null);
            return;
            ERROR1:
            // 校验发现/发生错误
            string strFastInputText = ChargingInfoDlg.Show(
                        this.CharingInfoHost,
                strError,
                InfoColor.Red,
                "验证读者证密码",
                this.InfoDlgOpacity,
                true,
                        Program.MainForm.DefaultFont);
            // 焦点重新定位到密码输入域
            /*
            this.textBox_readerPassword.Focus();
            this.textBox_readerPassword.SelectAll();
             * */
            this.SwitchFocus(READER_PASSWORD, strFastInputText);
        }

#if NO
        // 界面上 是否需要 校验读者密码
        bool VerifyReaderPassword
        {
            get
            {
                return m_bVerifyReaderPassword;
            }
            set
            {
                this.m_bVerifyReaderPassword = value;
                this.MenuItem_verifyReaderPassword.Checked = value;

                if (m_bVerifyReaderPassword == false)
                {
                    this.textBox_readerPassword.Enabled = false;
                    this.button_verifyReaderPassword.Enabled = false;
                }
                else
                {
                    this.textBox_readerPassword.Enabled = true;
                    this.button_verifyReaderPassword.Enabled = true;
                }
            }
        }
#endif
        bool VerifyReaderPassword
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "verify_reader_password",
                    false);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "charging_form",
                    "verify_reader_password",
                    value);
            }
        }

        /// <summary>
        /// 自动操作唯一事项
        /// </summary>
        public bool AutoOperSingleItem
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
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
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "isbn_borrow",
                    true);
            }
        }


        /// <summary>
        /// 读者信息中不显示借阅历史
        /// </summary>
        public bool NoBorrowHistory
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "no_borrow_history",
                    true);
            }
            set
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "charging_form",
                    "no_borrow_history",
                    value);
            }
        }

#if NO
        private void MenuItem_verifyReaderPassword_Click(object sender, EventArgs e)
        {
            if (this.VerifyReaderPassword == true)
            {
                this.VerifyReaderPassword = false;
            }
            else
            {
                this.VerifyReaderPassword = true;
            }
        }
#endif

        /*
        private void button_testCookie_Click(object sender, EventArgs e)
        {
            string strURL = "http://localhost";
            Uri UriURL = new Uri(strURL);
            string strCookie = RetrieveIECookiesForUrl(UriURL.AbsoluteUri);

            MessageBox.Show(this, strCookie);
        }

        private static string RetrieveIECookiesForUrl(string url)
        {
            url = "";
            StringBuilder cookieHeader = new StringBuilder(new String(' ',
    256), 256);
            int datasize = cookieHeader.Length;
            if (!API.InternetGetCookie(url, null, cookieHeader, ref datasize))
            {
                if (datasize < 0)
                    return String.Empty;
                cookieHeader = new StringBuilder(datasize); // resize with new datasize 
                API.InternetGetCookie(url, null, cookieHeader, ref datasize);
            }
            return cookieHeader.ToString(); 

        }
         * */



        // 修改窗口标题
        void UpdateWindowTitle()
        {
            this.Text = "出纳 " + this.textBox_readerBarcode.Text + " " + this.textBox_itemBarcode.Text;
        }

        private void textBox_readerBarcode_TextChanged(object sender, EventArgs e)
        {
            // 清除记忆的册条码号
            this.m_itemBarcodes.Clear();

            this.UpdateWindowTitle();

        }

        private void ChargingForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

            Program.MainForm.toolButton_refresh.Enabled = true;
        }

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
            if (this.m_webExternalHost_itemInfo != null)
                this.m_webExternalHost_itemInfo.StopPrevious();
            this.webBrowser_item.Stop();
            if (this.m_webExternalHost_biblioInfo != null)
                this.m_webExternalHost_biblioInfo.StopPrevious();
            this.webBrowser_biblio.Stop();
        }

        /// <summary>
        /// 是否要自动把输入的小写字符转换为大写
        /// </summary>
        public bool AutoToUpper
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                "charging_form",
                "auto_toupper_barcode",
                false);
            }
        }

        #region 读者证条码号快速导航菜单功能

        private void toolStripMenuItem_naviToAmerceForm_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveReaderBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的读者证条码号");
                return;
            }

            AmerceForm form = Program.MainForm.EnsureAmerceForm();
            Global.Activate(form);

            form.LoadReader(this.ActiveReaderBarcode, true);
        }

        private void toolStripMenuItem_naviToReaderInfoForm_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveReaderBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的读者证条码号");
                return;
            }

            ReaderInfoForm form = Program.MainForm.EnsureReaderInfoForm();
            Global.Activate(form);

            form.LoadRecord(this.ActiveReaderBarcode,
                false);
        }

        private void toolStripMenuItem_naviToActivateForm_old_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveReaderBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的读者证条码号");
                return;
            }

            ActivateForm form = Program.MainForm.EnsureActivateForm();
            Global.Activate(form);

            form.LoadOldRecord(this.ActiveReaderBarcode);
        }

        private void toolStripMenuItem_naviToActivateForm_new_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveReaderBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的读者证条码号");
                return;
            }

            ActivateForm form = Program.MainForm.EnsureActivateForm();
            Global.Activate(form);

            form.LoadNewRecord(this.ActiveReaderBarcode);
        }

        private void toolStripMenuItem_openReaderManageForm_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveReaderBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的读者证条码号");
                return;
            }

            ReaderManageForm form = Program.MainForm.EnsureReaderManageForm();
            Global.Activate(form);

            form.LoadRecord(this.ActiveReaderBarcode);
        }

        #endregion

        #region 册条码号快速导航菜单功能


        private void toolStripMenuItem_openEntityForm_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveItemBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的册条码号");
                return;
            }

            EntityForm form = Program.MainForm.EnsureEntityForm();
            Global.Activate(form);

            form.LoadItemByBarcode(this.ActiveItemBarcode, false);
        }

        private void toolStripMenuItem_openItemInfoForm_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.ActiveItemBarcode) == true)
            {
                MessageBox.Show(this, "当前还没有活动的册条码号");
                return;
            }

            ItemInfoForm form = Program.MainForm.EnsureItemInfoForm();
            Global.Activate(form);

            form.LoadRecord(this.ActiveItemBarcode);
        }

        #endregion

        private void webBrowser_reader_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        private void ChargingForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void ChargingForm_DragDrop(object sender, DragEventArgs e)
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
                strError = "出纳窗只允许拖入一个记录";
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
                string[] parts = strFirstLine.Split(new char[] { '\t' });
                string strReaderBarcode = "";
                if (parts.Length >= 2)
                    strReaderBarcode = parts[1].Trim();

                if (String.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    // this.CurrentReaderBarcode = strReaderBarcode;

                    this.textBox_readerBarcode.Text = strReaderBarcode;
                    button_loadReader_Click(this, null);
                }
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

        private void textBox_itemBarcode_TextChanged(object sender, EventArgs e)
        {
            // 修改窗口标题
            this.UpdateWindowTitle();
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            if (keyData == Keys.F5)
            {
                this.Reload();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void webBrowser_biblio_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        private void webBrowser_item_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (Program.MainForm.SuppressScriptErrors == true)
                e.Handled = true;
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

        /// <summary>
        /// 是否要在关闭信息对话框的时候自动停止填充
        /// </summary>
        public bool StopFillingWhenCloseInfoDlg
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
    "charging_form",
    "stop_filling_when_close_infodlg",
    true);
            }
        }

        /// <summary>
        /// 证条码号输入框是否允许输入汉字
        /// </summary>
        public bool PatronBarcodeAllowHanzi
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "charging_form",
                    "patron_barcode_allow_hanzi",
                    false);
            }
        }




    }

    /// <summary>
    /// 功能类型
    /// </summary>
    public enum FuncState
    {
        /// <summary>
        /// 自动。既可以借书，也可以还书
        /// </summary>
        Auto = 0,   // 既可以借书，也可以还书
        /// <summary>
        /// 借书
        /// </summary>
        Borrow = 1, // 借书
        /// <summary>
        /// 还书(不验证)
        /// </summary>
        Return = 2, // 还书(不验证)
        /// <summary>
        /// 还书(要验证)
        /// </summary>
        VerifyReturn = 3, // 还书(要验证)
        /// <summary>
        /// 续借(不验证)
        /// </summary>
        Renew = 4,  // 续借
        /// <summary>
        /// 续借(要验证)
        /// </summary>
        VerifyRenew = 5,  // 续借
        /// <summary>
        /// 丢失声明
        /// </summary>
        Lost = 6,   // 丢失

        /// <summary>
        /// 验证丢失
        /// </summary>
        VerifyLost = 7, // 验证丢失
        /// <summary>
        /// 装载读者信息
        /// </summary>
        LoadPatronInfo = 8, // 装载读者信息
        /// <summary>
        /// 同一读者继续借
        /// </summary>
        ContinueBorrow = 9, // 同一读者继续借书
        /// <summary>
        /// 盘点图书
        /// </summary>
        InventoryBook = 10, // 盘点图书 2015/8/16

        /// <summary>
        /// 读过
        /// </summary>
        Read = 11,  // 读过 2016/1/8

        /// <summary>
        /// 配书
        /// </summary>
        Boxing = 12,    // 配书 2016/12/3

        /// <summary>
        /// 移交
        /// </summary>
        Transfer = 13,  // 移交 2017/1/12

        /// <summary>
        /// 特殊借阅
        /// </summary>
        SpecialBorrow = 14, // 2021/8/26

        SpecialRenew = 15,  // 2021/8/26
    }

    /*public*/
    class BarcodeAndTime
    {
        public string Barcode = "";
        public DateTime Time = DateTime.Now;
    }

    /// <summary>
    /// 显示状态
    /// </summary>
    public enum DisplayState
    {
        /// <summary>
        /// HTML 格式
        /// </summary>
        HTML = 0, // html格式
        /// <summary>
        /// 纯文本格式
        /// </summary>
        TEXT = 1,   // 纯文本格式
    }

    /// <summary>
    /// 出纳信息的宿主
    /// </summary>
    internal class ChargingInfoHost
    {
        /// <summary>
        /// ApplicationInfo
        /// </summary>
        public IApplicationInfo ap = null;
        /// <summary>
        /// 宿主窗口
        /// </summary>
        public IWin32Window window = null;

        // 
        /// <summary>
        /// 停止获取摘要的动作
        /// </summary>
        public event EventHandler StopGettingSummary = null;

        /// <summary>
        /// 响应停止获取摘要的动作
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        public void OnStopGettingSummary(object sender, EventArgs e)
        {
            if (this.StopGettingSummary != null)
                this.StopGettingSummary(sender, e);
        }

    }
}