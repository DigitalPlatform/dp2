using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.DTLP;
using System.IO;
using System.Xml;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform;

namespace dp2Catalog
{
    public partial class SystemCfgForm : Form
    {
        public string Lang = "zh-CN";

        public event ParamChangedEventHandler ParamChanged = null;

        public MainForm MainForm = null;

        int m_nPreventFire = 0;

        string m_strOldDp2searchformLayout = "";

        public SystemCfgForm()
        {
            InitializeComponent();

            this.tabComboBox_amazon_defaultServer.RemoveRightPartAtTextBox = false;
        }

        private void SystemCfgForm_Load(object sender, EventArgs e)
        {
            this.m_nPreventFire++;

            try
            {
                // 初始化值
                textBox_dtlpDefaultUserName.Text = MainForm.AppInfo.GetString(
                    "preference",
                    "defaultUserName",
                    "public");
                textBox_dtlpDefaultPassword.Text = MainForm.AppInfo.GetString(
                    "preference",
                    "defaultPassword",
                    "");
                checkBox_dtlpSavePassword.Checked = Convert.ToBoolean(
                    MainForm.AppInfo.GetInt(
                    "preference",
                    "savePassword",
                    0)
                    );



                // 查重

                // 备选的起点路径
                this.textBox_dp2library_searchDup_defaultStartPath.Text = MainForm.AppInfo.GetString(
                    "searchdup",
                    "defaultStartPath",
                    "");

                // dp2检索窗
                this.comboBox_dp2SearchForm_layout.Text = MainForm.AppInfo.GetString(
                    "dp2searchform",
                    "layout",
                    "资源树最大");
                this.m_strOldDp2searchformLayout = this.comboBox_dp2SearchForm_layout.Text;

                checkBox_dtlpSavePassword_CheckedChanged(null, null);

                this.checkBox_dp2SearchForm_useExistDetailWindow.Checked = MainForm.AppInfo.GetBoolean(
    "all_search_form",
    "load_to_exist_detailwindow",
    true);

                this.numericUpDown_dp2library_searchMaxCount.Value = MainForm.AppInfo.GetInt(
    "dp2library",
    "search_max_count",
    1000);

                // 亚马逊检索窗
                this.checkBox_amazon_alwaysUseFullElementSet.Checked = MainForm.AppInfo.GetBoolean(
"amazon_search_form",
"always_use_full_elementset",
true);

                string strError = "";
                int nRet = FillAmazonDefaultServerList(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

                this.tabComboBox_amazon_defaultServer.Text = MainForm.AppInfo.GetString(
"amazon_search_form",
"default_server",
"中国\twebservices.amazon.cn");

                // 创建 856 字段
                this.checkBox_amazon_create856.Checked = MainForm.AppInfo.GetBoolean(
"amazon_search_form",
"create_856",
true);

                // ui
                // 停靠
                this.comboBox_ui_fixedPanelDock.Text = this.MainForm.panel_fixed.Dock.ToString();

                this.checkBox_ui_hideFixedPanel.Checked = MainForm.AppInfo.GetBoolean(
                    "MainForm",
                    "hide_fixed_panel",
                    false);

                this.textBox_ui_defaultFont.Text = MainForm.AppInfo.GetString(
"Global",
"default_font",
"");

                // MarcDetailForm
                this.checkBox_marcDetailForm_verifyDataWhenSaving.Checked = MainForm.AppInfo.GetBoolean(
"entity_form",
"verify_data_when_saving",
false);

                // *** 服务器
                // author number GCAT serverurl
                this.textBox_server_authorNumber_gcatUrl.Text =
                    MainForm.AppInfo.GetString("config",
                    "gcat_server_url",
                    "http://dp2003.com/dp2library/");  // "http://dp2003.com/dp2libraryws/gcat.asmx"

                // pinyin serverurl
                this.textBox_server_pinyin_gcatUrl.Text =
                    MainForm.AppInfo.GetString("config",
                    "pinyin_server_url",
                    "http://dp2003.com/dp2library/");


                // *** 全局
                // 加拼音时自动选择多音字
                this.checkBox_global_autoSelPinyin.Checked =
                    MainForm.AppInfo.GetBoolean(
                    "global",
                    "auto_select_pinyin",
                    false);
            }
            finally
            {
                this.m_nPreventFire--;
            }
        }

        // 可能会抛出异常
        public static List<string> ListAmazonServers(string strDataDir,
            string strLang)
        {
            List<string> results = new List<string>();

            string strCfgFileName = Path.Combine(strDataDir, "amazon/amazon.xml");
            XmlDocument dom = new XmlDocument();
            dom.Load(strCfgFileName);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("endpoints/endpoint");
            foreach (XmlNode node in nodes)
            {
                var element = node as XmlElement;
                string strHost = element.GetAttribute("host");
                string strCaption = DomUtil.GetCaption(strLang, node);
                results.Add(strCaption + "\t" + strHost);
            }

            return results;
        }

        int FillAmazonDefaultServerList(out string strError)
        {
            strError = "";

            this.tabComboBox_amazon_defaultServer.Items.Clear();
            try
            {
                List<string> results = ListAmazonServers(this.MainForm.DataDir, this.Lang);
                foreach (string s in results)
                {
                    this.tabComboBox_amazon_defaultServer.Items.Add(s);
                }
                return 0;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
        }

        private void SystemCfgForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SystemCfgForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void checkBox_dtlpSavePassword_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_dtlpSavePassword.Checked == true)
                this.textBox_dtlpDefaultPassword.Enabled = true;
            else
                this.textBox_dtlpDefaultPassword.Enabled = false;

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 保存值
            MainForm.AppInfo.SetString(
                "preference",
                "defaultUserName",
                textBox_dtlpDefaultUserName.Text);
            MainForm.AppInfo.SetString(
                "preference",
                "defaultPassword",
                textBox_dtlpDefaultPassword.Text);
            MainForm.AppInfo.SetInt(
                "preference",
                "savePassword",
                Convert.ToInt16(checkBox_dtlpSavePassword.Checked)
                );


            // 查重

            // 备选的起点路径
            MainForm.AppInfo.SetString(
                "searchdup",
                "defaultStartPath",
                this.textBox_dp2library_searchDup_defaultStartPath.Text);

            // dp2检索窗
            MainForm.AppInfo.SetString(
                "dp2searchform",
                "layout",
                this.comboBox_dp2SearchForm_layout.Text);

            MainForm.AppInfo.SetBoolean(
"all_search_form",
"load_to_exist_detailwindow",
this.checkBox_dp2SearchForm_useExistDetailWindow.Checked);

            MainForm.AppInfo.SetInt(
    "dp2library",
    "search_max_count",
    (int)this.numericUpDown_dp2library_searchMaxCount.Value);

            // 亚马逊检索窗
            MainForm.AppInfo.SetBoolean(
"amazon_search_form",
"always_use_full_elementset",
this.checkBox_amazon_alwaysUseFullElementSet.Checked);

            MainForm.AppInfo.SetString(
"amazon_search_form",
"default_server",
this.tabComboBox_amazon_defaultServer.Text);

            // 创建 856 字段
            MainForm.AppInfo.SetBoolean(
"amazon_search_form",
"create_856",
this.checkBox_amazon_create856.Checked);

            // ui
            MainForm.AppInfo.SetBoolean(
                "MainForm",
                "hide_fixed_panel",
                this.checkBox_ui_hideFixedPanel.Checked);

            MainForm.AppInfo.SetString(
    "Global",
    "default_font",
    this.textBox_ui_defaultFont.Text);

            // MarcDetailForm
            MainForm.AppInfo.SetBoolean(
"entity_form",
"verify_data_when_saving",
this.checkBox_marcDetailForm_verifyDataWhenSaving.Checked);

            // *** 服务器
            // author number GCAT serverurl
            MainForm.AppInfo.SetString("config",
                "gcat_server_url",
                this.textBox_server_authorNumber_gcatUrl.Text);

            // pinyin serverurl
            MainForm.AppInfo.SetString("config",
                "pinyin_server_url",
                this.textBox_server_pinyin_gcatUrl.Text);

            // *** 全局
            // 加拼音时自动选择多音字
            MainForm.AppInfo.SetBoolean(
                "global",
                "auto_select_pinyin",
                this.checkBox_global_autoSelPinyin.Checked);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // 还原中途曾经被改动过的
            if (this.comboBox_dp2SearchForm_layout.Text != this.m_strOldDp2searchformLayout)
                this.FireParamChanged(
"dp2searchform",
"layout",
(object)this.m_strOldDp2searchformLayout);

            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void button_hostManage_Click(object sender, EventArgs e)
        {
            DigitalPlatform.DTLP.HostManageDlg dlg
    = new DigitalPlatform.DTLP.HostManageDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.applicationInfo = MainForm.AppInfo;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.OK)
            {
                MessageBox.Show(this, "服务器地址修改后，需要重新打开相关窗口，新的配置才能反映在资源树中 ...");
            }

        }

        private void button_dp2library_serverManage_Click(object sender, EventArgs e)
        {
            this.MainForm.ManageServers(false);
        }

        private void button_dtlp_dupCfg_Click(object sender, EventArgs e)
        {
            string strError = "";

            DtlpSearchForm dtlp_searchform = this.MainForm.GetDtlpSearchForm();

            if (dtlp_searchform == null)
            {
                strError = "无法获得打开的DTLP检索窗，无法配置查重";
                goto ERROR1;
            }


            DupCfgDialog dlg = new DupCfgDialog();
            GuiUtil.SetControlFont(dlg, this.Font);

            int nRet = dlg.Initial(this.MainForm.DataDir + "\\dtlp_dup.xml",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            dlg.DtlpChannels = dtlp_searchform.DtlpChannels;
            dlg.DtlpChannel = dtlp_searchform.DtlpChannel;

            // dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg,
                "dupcfg_dialogdialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得缺省查重起点路径
        private void button_dp2library_searchDup_findDefaultStartPath_Click(object sender, EventArgs e)
        {
            string strError = "";

            dp2SearchForm dp2_searchform = this.MainForm.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                strError = "无法获得打开的 dp2检索窗，无法获得 dp2library 数据库名";
                goto ERROR1;
            }

            string strDefaultStartPath = Global.GetForwardStyleDp2Path(this.textBox_dp2library_searchDup_defaultStartPath.Text);

            // 临时指定一个dp2library服务器和数据库
            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请指定一个 dp2library 数据库，以作为模拟的查重起点";
#if OLD_CHANNEL
            dlg.dp2Channels = dp2_searchform.Channels;
#endif
            dlg.ChannelManager = Program.MainForm;

            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_DB };
            dlg.Path = strDefaultStartPath;

            this.MainForm.AppInfo.LinkFormState(dlg,
                "searchdup_selectstartpath_dialog_state");

            dlg.ShowDialog(this);

            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            strDefaultStartPath = Global.GetBackStyleDp2Path(dlg.Path + "/?");


            this.textBox_dp2library_searchDup_defaultStartPath.Text = strDefaultStartPath;
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void comboBox_dp2SearchForm_layout_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_dp2SearchForm_layout.Invalidate();
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

        private void comboBox_dp2SearchForm_layout_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nPreventFire > 0)
                return;

            this.FireParamChanged(
"dp2searchform",
"layout",
(object)this.comboBox_dp2SearchForm_layout.Text);

        }

        private void comboBox_ui_fixedPanelDock_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strDock = this.comboBox_ui_fixedPanelDock.Text;

            if (strDock == "Top")
            {
                this.MainForm.panel_fixed.Dock = DockStyle.Top;
                this.MainForm.panel_fixed.Size = new Size(this.MainForm.panel_fixed.Width,
                    this.MainForm.Size.Height / 3);
                this.MainForm.splitter_fixed.Dock = DockStyle.Top;
            }
            else if (strDock == "Bottom")
            {
                this.MainForm.panel_fixed.Dock = DockStyle.Bottom;
                this.MainForm.panel_fixed.Size = new Size(this.MainForm.panel_fixed.Width,
                    this.MainForm.Size.Height / 3);
                this.MainForm.splitter_fixed.Dock = DockStyle.Bottom;
            }
            else if (strDock == "Left")
            {
                this.MainForm.panel_fixed.Dock = DockStyle.Left;
                this.MainForm.panel_fixed.Size = new Size(this.MainForm.Size.Width / 3,
                    this.MainForm.panel_fixed.Size.Height);
                this.MainForm.splitter_fixed.Dock = DockStyle.Left;
            }
            else if (strDock == "Right")
            {
                this.MainForm.panel_fixed.Dock = DockStyle.Right;
                this.MainForm.panel_fixed.Size = new Size(this.MainForm.Size.Width / 3,
                    this.MainForm.panel_fixed.Size.Height);
                this.MainForm.splitter_fixed.Dock = DockStyle.Right;
            }
            else
            {
                // 缺省为右
                this.MainForm.panel_fixed.Dock = DockStyle.Right;
                this.MainForm.panel_fixed.Size = new Size(this.MainForm.Size.Width / 3,
                    this.MainForm.panel_fixed.Size.Height);
                this.MainForm.splitter_fixed.Dock = DockStyle.Right;
            }

        }

        private void comboBox_ui_fixedPanelDock_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_ui_fixedPanelDock.Invalidate();
        }

        private void checkBox_ui_hideFixedPanel_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_ui_hideFixedPanel.Checked == true)
            {
                this.MainForm.PanelFixedVisible = false;
            }
            else
            {
                this.MainForm.PanelFixedVisible = true;
            }
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

        private void tabComboBox_amazon_defaultServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.FireParamChanged(
"amazon_search_form",
"default_server",
(object)this.tabComboBox_amazon_defaultServer.Text);
        }

    }

    // 调用数据加工模块
    public delegate void ParamChangedEventHandler(object sender,
        ParamChangedEventArgs e);

    public class ParamChangedEventArgs : EventArgs
    {
        public string Section = "";
        public string Entry = "";
        public object Value = null;
    }
}