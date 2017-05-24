using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Script;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 系统参数配置 对话框
    /// </summary>
    internal partial class CfgDlg : Form
    {
        /// <summary>
        /// 配置参数变化的事件
        /// </summary>
        public event ParamChangedEventHandler ParamChanged = null;

        public ApplicationInfo ap = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        bool m_bServerCfgChanged = false; // 服务器配置信息修改过

        public CfgDlg()
        {
            InitializeComponent();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                GuiState.SetUiState(controls, value);
            }
        }

        private void CfgDlg_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null
                && !(Control.ModifierKeys == Keys.Control))
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            // *** 服务器

            // serverurl
            this.textBox_server_dp2LibraryServerUrl.Text =
                ap.GetString("config",
                "circulation_server_url",
                "http://localhost:8001/dp2library");

            // author number GCAT serverurl
            this.textBox_server_authorNumber_gcatUrl.Text =
                ap.GetString("config",
                "gcat_server_url",
                "http://dp2003.com/dp2library/");  // "http://dp2003.com/gcatserver/" // "http://dp2003.com/dp2libraryws/gcat.asmx"

            // pinyin serverurl
            this.textBox_server_pinyin_gcatUrl.Text =
                ap.GetString("config",
                "pinyin_server_url",
                "http://dp2003.com/dp2library/");   // "http://dp2003.com/gcatserver/"

            // 绿色安装包
            this.textBox_server_greenPackage.Text =
                ap.GetString("config",
                "green_package_server_url",
                "");

            // dp2MServer URL
            this.textBox_message_dp2MServerUrl.Text =
                ap.GetString("config",
                "im_server_url",
                default_dp2mserver_url);

            // *** 缺省账户

            // 用户名
            this.textBox_defaultAccount_userName.Text =
                ap.GetString(
                "default_account",
                "username",
                "");

            this.checkBox_defaulAccount_savePasswordShort.Checked =
                ap.GetBoolean(
                "default_account",
                "savepassword_short",
                false);
            this.checkBox_defaulAccount_savePasswordLong.Checked =
    ap.GetBoolean(
    "default_account",
    "savepassword_long",
    false);

            if (this.checkBox_defaulAccount_savePasswordShort.Checked == true
                || this.checkBox_defaulAccount_savePasswordLong.Checked == true)
            {
                string strPassword = ap.GetString(
        "default_account",
        "password",
        "");
                strPassword = Program.MainForm.DecryptPasssword(strPassword);
                this.textBox_defaultAccount_password.Text = strPassword;
            }

            this.checkBox_defaultAccount_isReader.Checked =
                ap.GetBoolean(
                "default_account",
                "isreader",
                false);
            this.textBox_defaultAccount_location.Text =
                ap.GetString(
                "default_account",
                "location",
                "");
            this.checkBox_defaultAccount_occurPerStart.Checked = ap.GetBoolean(
                "default_account",
                "occur_per_start",
                true);

            // *** charging
            this.checkBox_charging_force.Checked = ap.GetBoolean(
                    "charging_form",
                    "force",
                    false);
            this.numericUpDown_charging_infoDlgOpacity.Value = ap.GetInt(
                "charging_form",
                "info_dlg_opacity",
                100);
            this.checkBox_charging_verifyBarcode.Checked = ap.GetBoolean(
                "charging_form",
                "verify_barcode",
                false);

            this.checkBox_charging_doubleItemInputAsEnd.Checked = ap.GetBoolean(
                "charging_form",
                "doubleItemInputAsEnd",
                false);

            this.comboBox_charging_displayFormat.Text =
    ap.GetString("charging_form",
    "display_format",
    "HTML");

            this.checkBox_charging_autoUppercaseBarcode.Checked =
                ap.GetBoolean(
                "charging_form",
                "auto_toupper_barcode",
                false);

            this.checkBox_charging_greenInfoDlgNotOccur.Checked =
                ap.GetBoolean(
                "charging_form",
                "green_infodlg_not_occur",
                false);

            this.checkBox_charging_stopFillingWhenCloseInfoDlg.Checked =
    ap.GetBoolean(
    "charging_form",
    "stop_filling_when_close_infodlg",
    true);

            this.checkBox_charging_noBiblioAndItem.Checked =
                ap.GetBoolean(
                "charging_form",
                "no_biblio_and_item_info",
                false);

            this.checkBox_charging_autoSwitchReaderBarcode.Checked =
                ap.GetBoolean(
                "charging_form",
                "auto_switch_reader_barcode",
                false);

            // 自动清除输入框中内容
            // 2008/9/26
            this.checkBox_charging_autoClearTextbox.Checked = ap.GetBoolean(
                "charging_form",
                "autoClearTextbox",
                true);

            // 启用读者密码验证
            this.checkBox_charging_veifyReaderPassword.Checked = ap.GetBoolean(
                "charging_form",
                "verify_reader_password",
                false);

            // 朗读读者姓名
            this.checkBox_charging_speakNameWhenLoadReaderRecord.Checked = ap.GetBoolean(
                "charging_form",
                "speak_reader_name",
                false);

            // 证条码号输入框允许输入汉字
            this.checkBox_charging_patronBarcodeAllowHanzi.Checked = ap.GetBoolean(
                "charging_form",
                "patron_barcode_allow_hanzi",
                false);

            // 读者信息中不显示借阅历史
            this.checkBox_charging_noBorrowHistory.Checked = ap.GetBoolean(
                "charging_form",
                "no_borrow_history",
                true);
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.20") < 0)
                this.checkBox_charging_noBorrowHistory.Enabled = false;

            // 启用 ISBN 借书还书功能
            this.checkBox_charging_isbnBorrow.Checked = ap.GetBoolean(
                "charging_form",
                "isbn_borrow",
                true);

            // 自动操作唯一事项
            this.checkBox_charging_autoOperItemDialogSingleItem.Checked = ap.GetBoolean(
                "charging_form",
                "auto_oper_single_item",
                false);

            // *** 快捷出纳

            this.comboBox_quickCharging_displayFormat.Text =
ap.GetString("quickcharging_form",
"display_format",
"HTML");
            this.comboBox_quickCharging_displayStyle.Text =
ap.GetString("quickcharging_form",
"display_style",
"light");

            // 验证条码号
            this.checkBox_quickCharging_verifyBarcode.Checked = ap.GetBoolean(
    "quickcharging_form",
    "verify_barcode",
    false);
            // 读者信息中不显示借阅历史
            this.checkBox_quickCharging_noBorrowHistory.Checked = ap.GetBoolean(
                "quickcharging_form",
                "no_borrow_history",
                true);
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.20") < 0)
                this.checkBox_quickCharging_noBorrowHistory.Enabled = false;

            // 朗读读者姓名
            this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Checked = ap.GetBoolean(
                "quickcharging_form",
                "speak_reader_name",
                false);

            // 朗读书名
            this.checkBox_quickCharging_speakBookTitle.Checked = ap.GetBoolean(
                "quickcharging_form",
                "speak_book_title",
                false);

            // 朗读状态
            this.comboBox_quickCharging_stateSpeak.Text = ap.GetString("quickcharging_form",
    "state_speak",
    "[不朗读]");

            // 启用 ISBN 借书还书功能
            this.checkBox_quickCharging_isbnBorrow.Checked = ap.GetBoolean(
                "quickcharging_form",
                "isbn_borrow",
                true);

            // 自动操作唯一事项
            this.checkBox_quickCharging_autoOperItemDialogSingleItem.Checked = ap.GetBoolean(
                "quickcharging_form",
                "auto_oper_single_item",
                false);

            // 日志记载操作耗时
            this.checkBox_quickCharging_logOperTime.Checked = ap.GetBoolean(
                "quickcharging_form",
                "log_opertime",
                true);

            // *** 种册窗
            this.checkBox_itemManagement_verifyItemBarcode.Checked = ap.GetBoolean(
                "entity_form",
                "verify_item_barcode",
                false);

            this.checkBox_itemManagement_cataloging.Checked = ap.GetBoolean(
                "entity_form",
                "cataloging",
                true);  // 2007/12/2 修改为 true

            this.checkBox_itemManagement_searchDupWhenSaving.Checked = ap.GetBoolean(
                "entity_form",
                "search_dup_when_saving",
                false);

            this.checkBox_itemManagement_verifyDataWhenSaving.Checked = ap.GetBoolean(
    "entity_form",
    "verify_data_when_saving",
    false);

            this.checkBox_itemManagement_showQueryPanel.Checked = ap.GetBoolean(
"entityform",
"queryPanel_visibie",
true);
            this.checkBox_itemManagement_showItemQuickInputPanel.Checked = ap.GetBoolean(
"entityform",
"itemQuickInputPanel_visibie",
true);

            // 副本书目记录显示为只读状态
            this.checkBox_itemManagement_linkedRecordReadonly.Checked = ap.GetBoolean(
"entityform",
"linkedRecordReadonly",
true);

            // 显示其他分馆的册记录
            this.checkBox_itemManagement_displayOtherLibraryItem.Checked = ap.GetBoolean(
"entityform",
"displayOtherLibraryItem",
false);

            // 自动限定paste进入的图像宽度
            this.textBox_itemManagement_maxPicWidth.Text = Program.MainForm.AppInfo.GetString(
    "entityform",
    "paste_pic_maxwidth",
    "-1");

            // ui 外观

            // 停靠
            this.comboBox_ui_fixedPanelDock.Text = Program.MainForm.panel_fixed.Dock.ToString();

            this.checkBox_ui_hideFixedPanel.Checked = ap.GetBoolean(
                "MainForm",
                "hide_fixed_panel",
                false);

            this.checkBox_ui_fixedPanelAnimationEnabled.Checked = ap.GetBoolean(
                "MainForm",
                "fixed_panel_animation",
                false);

            this.textBox_ui_defaultFont.Text = ap.GetString(
    "Global",
    "default_font",
    "");

            // *** 入馆登记
            // passgate
            this.numericUpDown_passgate_maxListItemsCount.Value = ap.GetInt(
                "passgate_form",
                "max_list_items_count",
                1000);

            // 检索
            // search
            this.checkBox_search_useExistDetailWindow.Checked = ap.GetBoolean(
                "all_search_form",
                "load_to_exist_detailwindow",
                true);

            this.numericUpDown_search_maxBiblioResultCount.Value = ap.GetInt(
                "biblio_search_form",
                "max_result_count",
                -1);

            this.checkBox_search_hideBiblioMatchStyle.Checked = ap.GetBoolean(
                "biblio_search_form",
                "hide_matchstyle",
                false);

            // 2008/1/20 
            this.checkBox_search_biblioPushFilling.Checked = ap.GetBoolean(
                "biblio_search_form",
                "push_filling_browse",
                false);

            this.numericUpDown_search_maxReaderResultCount.Value = ap.GetInt(
                "reader_search_form",
                "max_result_count",
                -1);

            this.checkBox_search_hideReaderMatchStyle.Checked = ap.GetBoolean(
                "reader_search_form",
                "hide_matchstyle",
                false);

            // 2008/1/20 
            this.checkBox_search_readerPushFilling.Checked = ap.GetBoolean(
                "reader_search_form",
                "push_filling_browse",
                false);

            // ---
            this.numericUpDown_search_maxItemResultCount.Value = ap.GetInt(
                "item_search_form",
                "max_result_count",
                -1);

            // 2008/11/21 
            this.checkBox_search_hideItemMatchStyleAndDbName.Checked = ap.GetBoolean(
                "item_search_form",
                "hide_matchstyle_and_dbname",
                false);

            // 2008/1/20 
            this.checkBox_search_itemPushFilling.Checked = ap.GetBoolean(
                "item_search_form",
                "push_filling_browse",
                false);

            // --- order
            this.numericUpDown_search_maxOrderResultCount.Value = ap.GetInt(
    "order_search_form",
    "max_result_count",
    -1);

            this.checkBox_search_hideOrderMatchStyleAndDbName.Checked = ap.GetBoolean(
                "order_search_form",
                "hide_matchstyle_and_dbname",
                false);

            this.checkBox_search_orderPushFilling.Checked = ap.GetBoolean(
                "order_search_form",
                "push_filling_browse",
                false);

            // --- issue
            this.numericUpDown_search_maxIssueResultCount.Value = ap.GetInt(
    "issue_search_form",
    "max_result_count",
    -1);

            this.checkBox_search_hideIssueMatchStyleAndDbName.Checked = ap.GetBoolean(
                "issue_search_form",
                "hide_matchstyle_and_dbname",
                true);

            this.checkBox_search_issuePushFilling.Checked = ap.GetBoolean(
                "issue_search_form",
                "push_filling_browse",
                false);

            // --- comment
            this.numericUpDown_search_maxCommentResultCount.Value = ap.GetInt(
    "comment_search_form",
    "max_result_count",
    -1);

            this.checkBox_search_hideCommentMatchStyleAndDbName.Checked = ap.GetBoolean(
                "comment_search_form",
                "hide_matchstyle_and_dbname",
                false);

            this.checkBox_search_commentPushFilling.Checked = ap.GetBoolean(
                "comment_search_form",
                "push_filling_browse",
                false);

            // 凭条打印
            this.comboBox_print_prnPort.Text =
                ap.GetString("charging_print",
                "prnPort",
                "LPT1");

            this.checkBox_print_pausePrint.Checked = ap.GetBoolean(
                "charging_print",
                "pausePrint",
                false);

            this.textBox_print_projectName.Text = ap.GetString(
                "charging_print",
                "projectName",
                "");

            //
            this.label_print_projectNameMessage.Text = "";

            // amerce
            this.comboBox_amerce_interface.Text =
                ap.GetString("config",
                "amerce_interface",
                "<无>");

            // 交费窗布局
            this.comboBox_amerce_layout.Text =
    ap.GetString("amerce_form",
    "layout",
    "左右分布");


            // accept
            this.checkBox_accept_singleClickLoadDetail.Checked =
                ap.GetBoolean(
                "accept_form",
                "single_click_load_detail",
                false);

            // *** 读卡器

            // 身份证读卡器URL
            this.textBox_cardReader_idcardReaderUrl.Text =
    ap.GetString("cardreader",
    "idcardReaderUrl",
    "");  // 常用值 "ipc://IdcardChannel/IdcardServer"

            // *** 指纹

            // 指纹阅读器URL
            this.textBox_fingerprint_readerUrl.Text =
                ap.GetString("fingerprint",
                "fingerPrintReaderUrl",
                "");    // 常用值 "ipc://FingerprintChannel/FingerprintServer"

            // 指纹代理帐户 用户名
            this.textBox_fingerprint_userName.Text =
    ap.GetString("fingerprint",
    "userName",
    "");
            // 指纹代理帐户 密码
            {
                string strPassword = ap.GetString("fingerprint",
                "password",
                "");
                strPassword = Program.MainForm.DecryptPasssword(strPassword);
                this.textBox_fingerprint_password.Text = strPassword;
            }

            // *** 读者

            // 自动重试 当出现读卡对话框时
            this.checkBox_patron_autoRetryReaderCard.Checked =
                ap.GetBoolean(
                "reader_info_form",
                "autoretry_readcarddialog",
                true);

            // 出现 用身份证号设置条码号 对话框
            this.checkBox_patron_displaySetReaderBarcodeDialog.Checked =
                ap.GetBoolean(
                "reader_info_form",
                "display_setreaderbarcode_dialog",
                true);

            // 校验输入的条码号
            this.checkBox_patron_verifyBarcode.Checked = ap.GetBoolean(
    "reader_info_form",
    "verify_barcode",
    false);

            // 在读者窗范围内自动关闭 身份证读卡器 键盘仿真(&S)
            this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Checked = ap.GetBoolean(
    "reader_info_form",
    "disable_idcardreader_sendkey",
    true);

            // 日志
            // 显示读者借阅历史
            this.checkBox_operLog_displayReaderBorrowHistory.Checked =
                ap.GetBoolean(
                "operlog_form",
                "display_reader_borrow_history",
                true);
            // 显示册借阅历史
            this.checkBox_operLog_displayItemBorrowHistory.Checked =
                ap.GetBoolean(
                "operlog_form",
                "display_item_borrow_history",
                true);
            // 自动缓存日志文件
            this.checkBox_operLog_autoCache.Checked =
                ap.GetBoolean(
                "global",
                "auto_cache_operlogfile",
                true);

            // 日志详细级别
            this.comboBox_operLog_level.Text =
                ap.GetString(
                "operlog_form",
                "level",
                "1 -- 简略");

            // 全局
            // 浏览器控件允许脚本错误对话框(&S)
            this.checkBox_global_displayScriptErrorDialog.Checked =
                ap.GetBoolean(
                "global",
                "display_webbrowsecontrol_scripterror_dialog",
                false);

            // 加拼音时自动选择多音字
            this.checkBox_global_autoSelPinyin.Checked =
                ap.GetBoolean(
                "global",
                "auto_select_pinyin",
                false);

            // 保存封面扫描的原始图像
            this.checkBox_global_saveOriginCoverImage.Checked =
                ap.GetBoolean(
                "global",
                "save_orign_cover_image",
                false);

            // 将键盘输入的条码号自动转为大写
            this.checkBox_global_upperInputBarcode.Checked =
                ap.GetBoolean(
                "global",
                "upper_input_barcode",
                true);

            // *** 标签打印
            // 从何处获取索取号
            this.comboBox_labelPrint_accessNoSource.Text = ap.GetString(
                "labelprint",
                "accessNo_source",
                "从册记录");

            // *** 消息

            // 共享书目数据
            _disableShareBiblioChangedEvent++;
            this.checkBox_message_shareBiblio.Checked = ap.GetBoolean(
                "message",
                "share_biblio",
                false);
            _disableShareBiblioChangedEvent--;

            this.textBox_message_userName.Text =
    ap.GetString(
    "message",
    "username",
    "");
            {
                string strPassword = ap.GetString(
        "message",
        "password",
        "");
                strPassword = Program.MainForm.DecryptPasssword(strPassword);
                this.textBox_message_password.Text = strPassword;
            }

            checkBox_charging_isbnBorrow_CheckedChanged(this, null);
            checkBox_quickCharging_isbnBorrow_CheckedChanged(this, null);

            this.m_bServerCfgChanged = false;
        }

        private void CfgDlg_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void CfgDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        void FireParamChanged(string strSection,
            string strEntry,
            object value)
        {
            if (this.ParamChanged != null)
            {
                ParamChangedEventArgs e = new ParamChangedEventArgs();
                e.Section = strSection;
                e.Entry = strEntry;
                e.Value = value;
                this.ParamChanged(this, e);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // serverurl
            ap.SetString("config",
                "circulation_server_url",
                this.textBox_server_dp2LibraryServerUrl.Text);

            // author number GCAT serverurl
            ap.SetString("config",
                "gcat_server_url",
                this.textBox_server_authorNumber_gcatUrl.Text);

            // pinyin serverurl
            ap.SetString("config",
                "pinyin_server_url",
                this.textBox_server_pinyin_gcatUrl.Text);

            // 绿色安装包
            ap.SetString("config",
                "green_package_server_url",
                this.textBox_server_greenPackage.Text);

            // dp2MServer URL
            ap.SetString("config",
                "im_server_url",
                this.textBox_message_dp2MServerUrl.Text);

            // default account
            ap.SetString(
                "default_account",
                "username",
                this.textBox_defaultAccount_userName.Text);

            ap.SetBoolean(
                "default_account",
                "savepassword_short",
                this.checkBox_defaulAccount_savePasswordShort.Checked);

            ap.SetBoolean(
    "default_account",
    "savepassword_long",
    this.checkBox_defaulAccount_savePasswordLong.Checked);

            if (this.checkBox_defaulAccount_savePasswordShort.Checked == true
                || this.checkBox_defaulAccount_savePasswordLong.Checked == true)
            {
                string strPassword = Program.MainForm.EncryptPassword(this.textBox_defaultAccount_password.Text);
                ap.SetString(
                    "default_account",
                    "password",
                    strPassword);
            }
            else
            {
                ap.SetString(
    "default_account",
    "password",
    "");
            }

            ap.SetBoolean(
                "default_account",
                "isreader",
                this.checkBox_defaultAccount_isReader.Checked);
            ap.SetString(
                "default_account",
                "location",
                this.textBox_defaultAccount_location.Text);
            ap.SetBoolean(
                "default_account",
                "occur_per_start",
                this.checkBox_defaultAccount_occurPerStart.Checked);

            // charging
            ap.SetBoolean(
                "charging_form",
                "force",
                this.checkBox_charging_force.Checked);
            ap.SetInt(
                "charging_form",
                "info_dlg_opacity",
                (int)this.numericUpDown_charging_infoDlgOpacity.Value);
            ap.SetBoolean(
                "charging_form",
                "verify_barcode",
                this.checkBox_charging_verifyBarcode.Checked);

            ap.SetBoolean(
               "charging_form",
                "doubleItemInputAsEnd",
                this.checkBox_charging_doubleItemInputAsEnd.Checked);

            ap.SetString("charging_form",
                "display_format",
                this.comboBox_charging_displayFormat.Text);

            ap.SetBoolean(
                "charging_form",
                "auto_toupper_barcode",
                this.checkBox_charging_autoUppercaseBarcode.Checked);

            ap.SetBoolean(
    "charging_form",
    "green_infodlg_not_occur",
    this.checkBox_charging_greenInfoDlgNotOccur.Checked);


            ap.SetBoolean(
"charging_form",
"stop_filling_when_close_infodlg",
this.checkBox_charging_stopFillingWhenCloseInfoDlg.Checked);

            ap.SetBoolean(
                "charging_form",
                "no_biblio_and_item_info",
                this.checkBox_charging_noBiblioAndItem.Checked);


            ap.SetBoolean(
                "charging_form",
                "auto_switch_reader_barcode",
                this.checkBox_charging_autoSwitchReaderBarcode.Checked);

            // 自动清除输入框中内容
            // 2008/9/26
            ap.SetBoolean(
                "charging_form",
                "autoClearTextbox",
                this.checkBox_charging_autoClearTextbox.Checked);


            // 启用读者密码验证
            ap.SetBoolean(
                "charging_form",
                "verify_reader_password",
                this.checkBox_charging_veifyReaderPassword.Checked);

            // 朗读读者姓名
            ap.SetBoolean(
    "charging_form",
    "speak_reader_name",
    this.checkBox_charging_speakNameWhenLoadReaderRecord.Checked);

            // 证条码号输入框允许输入汉字
            ap.SetBoolean(
                "charging_form",
                "patron_barcode_allow_hanzi",
                this.checkBox_charging_patronBarcodeAllowHanzi.Checked);

            // 读者信息中不显示借阅历史
            ap.SetBoolean(
                "charging_form",
                "no_borrow_history",
                this.checkBox_charging_noBorrowHistory.Checked);

            // 启用 ISBN 借书还书功能
            ap.SetBoolean(
               "charging_form",
               "isbn_borrow",
               this.checkBox_charging_isbnBorrow.Checked);

            // 自动操作唯一事项
            ap.SetBoolean(
                "charging_form",
                "auto_oper_single_item",
                this.checkBox_charging_autoOperItemDialogSingleItem.Checked);

            // *** 快捷出纳

            ap.SetString("quickcharging_form",
                "display_format",
                this.comboBox_quickCharging_displayFormat.Text);

            ap.SetString("quickcharging_form",
    "display_style",
    this.comboBox_quickCharging_displayStyle.Text);

            // 验证条码号
            ap.SetBoolean(
    "quickcharging_form",
    "verify_barcode",
    this.checkBox_quickCharging_verifyBarcode.Checked);

            // 读者信息中不显示借阅历史
            ap.SetBoolean(
                "quickcharging_form",
                "no_borrow_history",
                this.checkBox_quickCharging_noBorrowHistory.Checked);

            // 朗读读者姓名
            ap.SetBoolean(
                "quickcharging_form",
                "speak_reader_name",
                this.checkBox_quickCharging_speakNameWhenLoadReaderRecord.Checked);

            // 朗读书名
            ap.SetBoolean(
                "quickcharging_form",
                "speak_book_title",
                this.checkBox_quickCharging_speakBookTitle.Checked);

            // 朗读状态
            ap.SetString("quickcharging_form",
    "state_speak",
    this.comboBox_quickCharging_stateSpeak.Text);


            // 启用 ISBN 借书还书功能
            ap.SetBoolean(
                "quickcharging_form",
                "isbn_borrow",
                this.checkBox_quickCharging_isbnBorrow.Checked);

            // 自动操作唯一事项
            ap.SetBoolean(
                "quickcharging_form",
                "auto_oper_single_item",
                this.checkBox_quickCharging_autoOperItemDialogSingleItem.Checked);

            // 日志记载操作耗时
            ap.SetBoolean(
                "quickcharging_form",
                "log_opertime",
                this.checkBox_quickCharging_logOperTime.Checked);

            // *** 种册窗
            ap.SetBoolean(
                "entity_form",
                "verify_item_barcode",
                this.checkBox_itemManagement_verifyItemBarcode.Checked);

            ap.SetBoolean(
                "entity_form",
                "cataloging",
                this.checkBox_itemManagement_cataloging.Checked);

            ap.SetBoolean(
                "entity_form",
                "search_dup_when_saving",
                this.checkBox_itemManagement_searchDupWhenSaving.Checked);

            ap.SetBoolean(
"entity_form",
"verify_data_when_saving",
this.checkBox_itemManagement_verifyDataWhenSaving.Checked);

            ap.SetBoolean(
"entityform",
"queryPanel_visibie",
this.checkBox_itemManagement_showQueryPanel.Checked);
            ap.SetBoolean(
"entityform",
"itemQuickInputPanel_visibie",
this.checkBox_itemManagement_showItemQuickInputPanel.Checked);

            // 副本书目记录显示为只读状态
            ap.SetBoolean(
"entityform",
"linkedRecordReadonly",
this.checkBox_itemManagement_linkedRecordReadonly.Checked);

            // 自动限定paste进入的图像宽度
            ap.SetString(
    "entityform",
    "paste_pic_maxwidth",
    this.textBox_itemManagement_maxPicWidth.Text);

            // 显示其他分馆的册记录
            ap.SetBoolean(
"entityform",
"displayOtherLibraryItem",
this.checkBox_itemManagement_displayOtherLibraryItem.Checked);

            // ui
            ap.SetBoolean(
                "MainForm",
                "hide_fixed_panel",
                this.checkBox_ui_hideFixedPanel.Checked);

            ap.SetBoolean(
    "MainForm",
    "fixed_panel_animation",
    this.checkBox_ui_fixedPanelAnimationEnabled.Checked);

            ap.SetString(
                "Global",
                "default_font",
                this.textBox_ui_defaultFont.Text);

            // passgate
            // 入馆登记
            ap.SetInt(
                "passgate_form",
                "max_list_items_count",
                (int)this.numericUpDown_passgate_maxListItemsCount.Value);

            // search
            ap.SetBoolean(
                "all_search_form",
                "load_to_exist_detailwindow",
                this.checkBox_search_useExistDetailWindow.Checked);

            ap.SetInt(
                "biblio_search_form",
                "max_result_count",
                (int)this.numericUpDown_search_maxBiblioResultCount.Value);

            ap.SetBoolean(
                "biblio_search_form",
                "hide_matchstyle",
                this.checkBox_search_hideBiblioMatchStyle.Checked);

            // 2008/1/20 
            ap.SetBoolean(
                "biblio_search_form",
                "push_filling_browse",
                this.checkBox_search_biblioPushFilling.Checked);


            ap.SetInt(
                "reader_search_form",
                "max_result_count",
                (int)this.numericUpDown_search_maxReaderResultCount.Value);

            ap.SetBoolean(
                "reader_search_form",
                "hide_matchstyle",
                this.checkBox_search_hideReaderMatchStyle.Checked);

            // 2008/1/20 
            ap.SetBoolean(
                "reader_search_form",
                "push_filling_browse",
                this.checkBox_search_readerPushFilling.Checked);

            // --- search
            ap.SetInt(
                "item_search_form",
                "max_result_count",
                (int)this.numericUpDown_search_maxItemResultCount.Value);

            // 2008/11/21 
            ap.SetBoolean(
                "item_search_form",
                "hide_matchstyle_and_dbname",
                this.checkBox_search_hideItemMatchStyleAndDbName.Checked);

            // 2008/1/20 
            ap.SetBoolean(
                "item_search_form",
                "push_filling_browse",
                this.checkBox_search_itemPushFilling.Checked);


            // --- order
            ap.SetInt(
   "order_search_form",
   "max_result_count",
   (int)this.numericUpDown_search_maxOrderResultCount.Value);

            ap.SetBoolean(
                "order_search_form",
                "hide_matchstyle_and_dbname",
                this.checkBox_search_hideOrderMatchStyleAndDbName.Checked);

            ap.SetBoolean(
                "order_search_form",
                "push_filling_browse",
                this.checkBox_search_orderPushFilling.Checked);

            // --- issue
            ap.SetInt(
    "issue_search_form",
    "max_result_count",
    (int)this.numericUpDown_search_maxIssueResultCount.Value);

            ap.SetBoolean(
                "issue_search_form",
                "hide_matchstyle_and_dbname",
                this.checkBox_search_hideIssueMatchStyleAndDbName.Checked);

            ap.SetBoolean(
                "issue_search_form",
                "push_filling_browse",
                this.checkBox_search_issuePushFilling.Checked);

            // --- comment
            ap.SetInt(
    "comment_search_form",
    "max_result_count",
    (int)this.numericUpDown_search_maxCommentResultCount.Value);

            ap.SetBoolean(
                "comment_search_form",
                "hide_matchstyle_and_dbname",
                this.checkBox_search_hideCommentMatchStyleAndDbName.Checked);

            ap.SetBoolean(
                "comment_search_form",
                "push_filling_browse",
                this.checkBox_search_commentPushFilling.Checked);


            // 凭条打印
            ap.SetString("charging_print",
                "prnPort",
                this.comboBox_print_prnPort.Text);

            ap.SetBoolean(
                "charging_print",
                "pausePrint",
                this.checkBox_print_pausePrint.Checked);

            ap.SetString(
                "charging_print",
                "projectName",
                this.textBox_print_projectName.Text);

            // amerce
            ap.SetString("config",
                "amerce_interface",
                this.comboBox_amerce_interface.Text);

            // 交费窗布局
            ap.SetString("amerce_form",
    "layout",
    this.comboBox_amerce_layout.Text);

            // accept
            ap.SetBoolean(
                "accept_form",
                "single_click_load_detail",
                this.checkBox_accept_singleClickLoadDetail.Checked);

            // *** 读卡器

            // 身份证读卡器URL
            ap.SetString("cardreader",
                "idcardReaderUrl",
                this.textBox_cardReader_idcardReaderUrl.Text);

            // ** 指纹
            // 指纹阅读器URL
            ap.SetString("fingerprint",
                "fingerPrintReaderUrl",
                this.textBox_fingerprint_readerUrl.Text);

            // 指纹代理帐户 用户名
            ap.SetString("fingerprint",
                "userName",
                this.textBox_fingerprint_userName.Text);
            // 指纹代理帐户 密码
            {
                string strPassword = Program.MainForm.EncryptPassword(this.textBox_fingerprint_password.Text);
                ap.SetString(
                    "fingerprint",
                "password",
                    strPassword);
            }

            // *** 读者
            // 自动重试 当出现读卡对话框时
            ap.SetBoolean(
                "reader_info_form",
                "autoretry_readcarddialog",
                this.checkBox_patron_autoRetryReaderCard.Checked);

            // 出现 用身份证号设置条码号 对话框
            ap.SetBoolean(
                "reader_info_form",
                "display_setreaderbarcode_dialog",
                this.checkBox_patron_displaySetReaderBarcodeDialog.Checked);

            // 校验输入的条码号
            ap.SetBoolean(
    "reader_info_form",
    "verify_barcode",
    this.checkBox_patron_verifyBarcode.Checked);

            // 在读者窗范围内自动关闭 身份证读卡器 键盘仿真(&S)
            ap.GetBoolean(
    "reader_info_form",
    "disable_idcardreader_sendkey",
    this.checkBox_patron_disableIdcardReaderKeyboardSimulation.Checked);

            // 日志
            // 显示读者借阅历史
            ap.SetBoolean(
                "operlog_form",
                "display_reader_borrow_history",
                this.checkBox_operLog_displayReaderBorrowHistory.Checked);
            // 显示册借阅历史
            ap.SetBoolean(
                "operlog_form",
                "display_item_borrow_history",
                this.checkBox_operLog_displayItemBorrowHistory.Checked);
            // 自动缓存日志文件
            ap.SetBoolean(
                "global",
                "auto_cache_operlogfile",
                this.checkBox_operLog_autoCache.Checked);
            // 日志详细级别
            ap.SetString(
                "operlog_form",
                "level",
                this.comboBox_operLog_level.Text);

            // 全局
            // 浏览器控件允许脚本错误对话框(&S)
            ap.SetBoolean(
                "global",
                "display_webbrowsecontrol_scripterror_dialog",
                this.checkBox_global_displayScriptErrorDialog.Checked);

            // 加拼音时自动选择多音字
            ap.SetBoolean(
                "global",
                "auto_select_pinyin",
                this.checkBox_global_autoSelPinyin.Checked);

            // 保存封面扫描的原始图像
            ap.SetBoolean(
                "global",
                "save_orign_cover_image",
                this.checkBox_global_saveOriginCoverImage.Checked);
            
            // 将键盘输入的条码号自动转为大写
            ap.SetBoolean(
    "global",
    "upper_input_barcode",
    this.checkBox_global_upperInputBarcode.Checked);

            // *** 标签打印
            // 从何处获取索取号
            ap.SetString(
                "labelprint",
                "accessNo_source",
                this.comboBox_labelPrint_accessNoSource.Text);

            // *** 消息

            // 共享书目数据
            ap.SetBoolean(
                "message",
                "share_biblio",
                this.checkBox_message_shareBiblio.Checked);

            ap.SetString(
    "message",
    "username",
    this.textBox_message_userName.Text);

            {
                string strPassword = Program.MainForm.EncryptPassword(this.textBox_message_password.Text);
                ap.SetString(
                    "message",
                    "password",
                    strPassword);
            }

            if (m_bServerCfgChanged == true
                && Program.MainForm != null)
            {
                // 重新获得各种库名、列表
                Program.MainForm.StartPrepareNames(false, false);
            }

            Program.MainForm.FixedPanelAnimationEnabled = this.checkBox_ui_fixedPanelAnimationEnabled.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_clearValueTableCache_Click(object sender, EventArgs e)
        {
            Program.MainForm.ClearValueTableCache();
            MessageBox.Show(this, "OK");
        }

        // 重新获得书目库(公共)检索途径列表
        private void button_reloadBiblioDbFromInfos_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            Program.MainForm.GetDbFromInfos();

            MessageBox.Show(this, "OK");

            this.Enabled = true;

        }

        private void comboBox_ui_fixedPanelDock_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strDock = this.comboBox_ui_fixedPanelDock.Text;

            if (strDock == "Top")
            {
                Program.MainForm.panel_fixed.Dock = DockStyle.Top;
                Program.MainForm.panel_fixed.Size = new Size(Program.MainForm.panel_fixed.Width,
                    Program.MainForm.Size.Height / 3);
                Program.MainForm.splitter_fixed.Dock = DockStyle.Top;
            }
            else if (strDock == "Bottom")
            {
                Program.MainForm.panel_fixed.Dock = DockStyle.Bottom;
                Program.MainForm.panel_fixed.Size = new Size(Program.MainForm.panel_fixed.Width,
                    Program.MainForm.Size.Height / 3);
                Program.MainForm.splitter_fixed.Dock = DockStyle.Bottom;
            }
            else if (strDock == "Left")
            {
                Program.MainForm.panel_fixed.Dock = DockStyle.Left;
                Program.MainForm.panel_fixed.Size = new Size(Program.MainForm.Size.Width / 3,
                    Program.MainForm.panel_fixed.Size.Height);
                Program.MainForm.splitter_fixed.Dock = DockStyle.Left;
            }
            else if (strDock == "Right")
            {
                Program.MainForm.panel_fixed.Dock = DockStyle.Right;
                Program.MainForm.panel_fixed.Size = new Size(Program.MainForm.Size.Width / 3,
                    Program.MainForm.panel_fixed.Size.Height);
                Program.MainForm.splitter_fixed.Dock = DockStyle.Right;
            }
            else
            {
                // 缺省为右
                Program.MainForm.panel_fixed.Dock = DockStyle.Right;
                Program.MainForm.panel_fixed.Size = new Size(Program.MainForm.Size.Width / 3,
                    Program.MainForm.panel_fixed.Size.Height);
                Program.MainForm.splitter_fixed.Dock = DockStyle.Right;
            }
        }

        private void checkBox_ui_hideFixedPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_ui_hideFixedPanel.Checked == true)
            {
                /*
                Program.MainForm.panel_fixed.Visible = false;
                Program.MainForm.splitter_fixed.Visible = false;
                 * */
                Program.MainForm.PanelFixedVisible = false;
            }
            else
            {
                /*
                Program.MainForm.panel_fixed.Visible = true;
                Program.MainForm.splitter_fixed.Visible = true;
                 * */
                Program.MainForm.PanelFixedVisible = true;
            }
        }

        private void checkBox_defaulAccount_savePasswordLong_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_defaulAccount_savePasswordLong.Checked == true)
                this.checkBox_defaulAccount_savePasswordShort.Checked = true;
        }

        // 重新获得书目库名列表
        // 2007/5/27 
        private void button_reloadBiblioDbNames_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            Program.MainForm.InitialBiblioDbProperties();
            MessageBox.Show(this, "OK");

            this.Enabled = true;

        }

        private void button_reloadReaderDbProperties_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            // Program.MainForm.GetReaderDbNames();
            Program.MainForm.InitialReaderDbProperties();
            MessageBox.Show(this, "OK");

            this.Enabled = true;
        }

        // 重新获得实体库列表
        private void button_reloadUtilDbProperties_Click(object sender, EventArgs e)
        {
            this.Enabled = false;

            Program.MainForm.GetUtilDbProperties();
            MessageBox.Show(this, "OK");

            this.Enabled = true;
        }

        private void button_downloadPinyinXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            Program.MainForm.DownloadDataFile("pinyin.xml", out strError);
            MessageBox.Show(this, strError);
        }

        private void buttondownloadIsbnXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            Program.MainForm.DownloadDataFile("rangemessage.xml", out strError);   // 
            MessageBox.Show(this, strError);
        }

        private void MenuItem_print_editCharingPrintCs_Click(object sender, EventArgs e)
        {
            string strFileName = Path.Combine(Program.MainForm.DataDir, "charging_print.cs");
            System.Diagnostics.Process.Start("notepad.exe", strFileName);
        }

        private void MenuItem_print_editCharingPrintCsRef_Click(object sender, EventArgs e)
        {
            string strFileName = Path.Combine(Program.MainForm.DataDir, "charging_print.cs.ref");
            System.Diagnostics.Process.Start("notepad.exe", strFileName);
        }

        // 打印方案管理
        private void button_print_projectManage_Click(object sender, EventArgs e)
        {
            Program.MainForm.OperHistory.OnProjectManager(this);
        }

        private void textBox_print_projectName_TextChanged(object sender, EventArgs e)
        {
            label_print_projectNameMessage.Text = "方案名设定后，需要重新启动dp2circulation程序，才能发生作用。";
        }

        private void button_print_findProject_Click(object sender, EventArgs e)
        {
            // 出现对话框，询问Project名字
            GetProjectNameDlg dlg = new GetProjectNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.scriptManager = Program.MainForm.OperHistory.ScriptManager;
            dlg.ProjectName = this.textBox_print_projectName.Text;
            dlg.NoneProject = false;

            Program.MainForm.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_print_projectName.Text = dlg.ProjectName;
        }

        private void checkBox_charging_noBiblioAndItem_CheckedChanged(object sender, EventArgs e)
        {
            this.FireParamChanged(
                "charging_form",
                "no_biblio_and_item_info",
                (object)this.checkBox_charging_noBiblioAndItem.Checked);
        }

        private void textBox_server_circulationServerUrl_TextChanged(object sender, EventArgs e)
        {
            this.m_bServerCfgChanged = true;
        }

        private void button_ui_getDefaultFont_Click(object sender, EventArgs e)
        {
            Font font = null;
            if (String.IsNullOrEmpty(this.textBox_ui_defaultFont.Text) == false)
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                font = (Font)converter.ConvertFromString(this.textBox_ui_defaultFont.Text);
            }
            else
            {
                font = Control.DefaultFont;
            }

            FontDialog dlg = new FontDialog();
            dlg.ShowColor = false;
            dlg.Font = font;
            dlg.ShowApply = false;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                this.textBox_ui_defaultFont.Text = converter.ConvertToString(dlg.Font);
            }

        }

        private void comboBox_ui_fixedPanelDock_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_ui_fixedPanelDock.Invalidate();
        }

        private void button_operLog_clearCacheDirectory_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strCacheDir = Program.MainForm.OperLogCacheDir; //  PathUtil.MergePath(Program.MainForm.DataDir, "operlogcache");
            int nRet = Global.DeleteDataDir(
                this,
                strCacheDir,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            PathUtil.TryCreateDir(strCacheDir);  // 重新创建目录

            MessageBox.Show(this, "日志文件本地缓存目录 " + strCacheDir + " 已经被清空");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_fingerprint_defaultValue_Click(object sender, EventArgs e)
        {
            string strDefaultValue = "ipc://FingerprintChannel/FingerprintServer";

            DialogResult result = MessageBox.Show(this,
    "确实要将 指纹阅读器接口URL 的值设置为常用值\r\n \"" + strDefaultValue + "\" ? ",
    "CfgDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            this.textBox_fingerprint_readerUrl.Text = strDefaultValue;
        }

        private void button_fingerprint_clearLocalCacheFiles_Click(object sender, EventArgs e)
        {
            string strDir = Program.MainForm.FingerPrintCacheDir;  // PathUtil.MergePath(Program.MainForm.DataDir, "fingerprintcache");
            DialogResult result = MessageBox.Show(this,
"确实要删除文件夹 " + strDir + " (包括其中的的全部文件) ? ",
"CfgDlg",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            string strError = "";
            try
            {
                Directory.Delete(strDir, true);
            }
            catch (DirectoryNotFoundException)
            {
                strError = "本次操作前，文件夹 '" + strDir + "' 已经被删除";
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "删除文件夹 '" + strDir + "' 时出错: " + ex.Message;
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_cardReader_setIdcardUrlDefaultValue_Click(object sender, EventArgs e)
        {
            string strDefaultValue = "ipc://IdcardChannel/IdcardServer";

            DialogResult result = MessageBox.Show(this,
    "确实要将 身份证读卡器接口URL 的值设置为常用值\r\n \"" + strDefaultValue + "\" ? ",
    "CfgDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            this.textBox_cardReader_idcardReaderUrl.Text = strDefaultValue;
        }

        private void textBox_fingerprint_userName_TextChanged(object sender, EventArgs e)
        {
            // 如果用户名为空，则密码也要为空。因为单独有密码字符串无用，还容易引起猜测
            if (this.textBox_fingerprint_userName.Text == "")
                this.textBox_fingerprint_password.Text = "";
        }

        private void toolStripButton_server_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_server_dp2LibraryServerUrl.Text != ServerDlg.HnbUrl)
            {
                this.textBox_server_dp2LibraryServerUrl.Text = ServerDlg.HnbUrl;

                this.textBox_defaultAccount_userName.Text = "";
                this.textBox_defaultAccount_password.Text = "";
            }
        }

        private void toolStripButton_server_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_server_dp2LibraryServerUrl.Text != "net.pipe://localhost/dp2library/xe")
            {

                this.textBox_server_dp2LibraryServerUrl.Text = "net.pipe://localhost/dp2library/xe";

                this.textBox_defaultAccount_userName.Text = "supervisor";
                this.textBox_defaultAccount_password.Text = "";
            }
        }

        private void checkBox_charging_isbnBorrow_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox_charging_selectItemDialog.Enabled = this.checkBox_charging_isbnBorrow.Checked;
        }

        private void checkBox_quickCharging_isbnBorrow_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox_quickCharging_selectItemDialog.Enabled = this.checkBox_quickCharging_isbnBorrow.Checked;

        }

        int _disableShareBiblioChangedEvent = 0;

        private void checkBox_message_shareBiblio_CheckedChanged(object sender, EventArgs e)
        {
            if (_disableShareBiblioChangedEvent == 0 && this.checkBox_message_shareBiblio.Checked == true)
            {
                DialogResult result = MessageBox.Show(this,
    "确实要共享书目数据?\r\n\r\n共享书目数据将允许他人检索和获取您的全部书目记录。如果您不同意使用这个功能，请点“否”",
    "CfgDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    _disableShareBiblioChangedEvent++;
                    this.checkBox_message_shareBiblio.Checked = false;
                    _disableShareBiblioChangedEvent--;
                }
                else
                {
                    Debug.Assert(result == System.Windows.Forms.DialogResult.Yes, "");

                    // 2016/9/28
                    if (string.IsNullOrEmpty(this.textBox_message_dp2MServerUrl.Text) == true)
                    {
                        this.textBox_message_dp2MServerUrl.Text = default_dp2mserver_url;
                        this.textBox_message_userName.Text = "";
                        this.textBox_message_password.Text = "";
                    }
                }
            }
        }

        const string default_dp2mserver_url = "http://dp2003.com:8083/dp2MServer";

        private void button_message_setDefaultUrl_Click(object sender, EventArgs e)
        {
            this.textBox_message_dp2MServerUrl.Text = default_dp2mserver_url;
        }


    }

    // 调用数据加工模块
    /// <summary>
    /// 配置参数变化的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ParamChangedEventHandler(object sender,
        ParamChangedEventArgs e);

    /// <summary>
    /// 配置参数变化事件的参数
    /// </summary>
    public class ParamChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 小节标题
        /// </summary>
        public string Section = "";
        /// <summary>
        /// 事项标题
        /// </summary>
        public string Entry = "";
        /// <summary>
        /// 参数值
        /// </summary>
        public object Value = null;
    }

}