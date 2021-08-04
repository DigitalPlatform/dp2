// #define TEST

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.DTLP;
using DigitalPlatform.Drawing;  // ColorUtil
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.UnionCatalogClient;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.Core;

namespace dp2Catalog
{
    public partial class MarcDetailForm : MyForm
    {
        SelectedTemplateCollection selected_templates = new SelectedTemplateCollection();

        MacroUtil m_macroutil = new MacroUtil();   // 宏处理器

        // 存储书目和<dprms:file>以外的其它XML片断
        XmlDocument domXmlFragment = null;

        VerifyViewerForm m_verifyViewer = null;

        public LoginInfo LoginInfo = new LoginInfo();

        LinkMarcFile linkMarcFile = null;   // 如果不为null，表示在连接状态

        const int WM_LOADSIZE = API.WM_USER + 201;
        const int WM_VERIFY_DATA = API.WM_USER + 204;
        const int WM_FILL_MARCEDITOR_SCRIPT_MENU = API.WM_USER + 205;

        // public MainForm MainForm = null;

        // public DigitalPlatform.Stop stop = null;

        public ISearchForm LinkedSearchForm = null;

        public long RecordVersion = 0; // 当前记录的修改后版本号。0 表示尚未修改过

        // 从Z39.50服务器检索过来的原始记录
        DigitalPlatform.OldZ3950.Record m_currentRecord = null;
        public DigitalPlatform.OldZ3950.Record CurrentRecord
        {
            get
            {
                return this.m_currentRecord;
            }
            set
            {
                this.m_currentRecord = value;

                // 显示Ctrl+A菜单
                if (this.MainForm.PanelFixedVisible == true
                    && value != null)   // 2013/6/5
                    this.AutoGenerate(this.MarcEditor,
                        new GenerateDataEventArgs(),
                    true);
            }
        }

        Encoding CurrentEncoding = Encoding.GetEncoding(936);

        public bool UseAutoDetectedMarcSyntaxOID = false;

        string m_strAutoDetectedMarcSyntaxOID = "";
        private string AutoDetectedMarcSyntaxOID
        {
            get
            {
                return this.m_strAutoDetectedMarcSyntaxOID;
            }
            set
            {
                this.m_strAutoDetectedMarcSyntaxOID = value;
            }
        }

        byte[] CurrentTimestamp = null;

        // 用于保存记录的路径
        public string SavePath
        {
            get
            {
                return this.textBox_savePath.Text;
            }
            set
            {
                this.textBox_savePath.Text = value;

                // 显示Ctrl+A菜单
                if (this.MainForm.PanelFixedVisible == true)
                    this.AutoGenerate(this.MarcEditor,
                        new GenerateDataEventArgs(),
                    true);
            }
        }

        // (C#脚本使用)
        // 书目记录路径 例如 "中文图书/1"
        public string ServerName
        {
            get
            {
                string strError = "";
                string strProtocol = "";
                string strPath = "";
                int nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    // throw new Exception(strError);
                    return "";
                }

                // TODO: 要区分不同的protocol进行正确处理

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return strServerName;
            }
        }

        // (C#脚本使用)
        // 书目记录路径 例如 "中文图书/1"
        public string BiblioRecPath
        {
            get
            {
                string strError = "";
                string strProtocol = "";
                string strPath = "";
                int nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    // throw new Exception(strError);
                    return "";
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return strLocalPath;
            }
        }

        // (C#脚本使用)
        // 书目库名
        public string BiblioDbName
        {
            get
            {
                string strError = "";
                string strProtocol = "";
                string strPath = "";
                int nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    // throw new Exception(strError);
                    return "";
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                return strBiblioDbName;
            }
        }

        public MarcDetailForm()
        {
            InitializeComponent();

            this.MarcEditor.Changed = false;    // 因为设计态的this.MarcEditor.Marc被设置了一次，changed修改了
            this.RecordVersion = 0;
        }

        private void MarcDetailForm_Load(object sender, EventArgs e)
        {
#if NO
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            Global.FillEncodingList(this.comboBox_originDataEncoding,
                true);
            /*
            // 补充MARC8
            this.comboBox_originDataEncoding.Items.Add("MARC-8");
             * */

            this.MarcEditor.AppInfo = this.MainForm.AppInfo;
            LoadFontToMarcEditor();
            this.MarcEditor.UiState = Program.MainForm.AppInfo.GetString("marcDetialForm", "marcEditorState", "");

            this.m_macroutil.ParseOneMacro -= new ParseOneMacroEventHandler(m_macroutil_ParseOneMacro);
            this.m_macroutil.ParseOneMacro += new ParseOneMacroEventHandler(m_macroutil_ParseOneMacro);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);

            string strSelectedTemplates = this.MainForm.AppInfo.GetString(
    "marcdetailform",
    "selected_templates",
    "");
            if (String.IsNullOrEmpty(strSelectedTemplates) == false)
            {
                selected_templates.Build(strSelectedTemplates);
            }

#if NO
            this.m_strPinyinGcatID = this.MainForm.AppInfo.GetString("entity_form", "gcat_pinyin_api_id", "");
            this.m_bSavePinyinGcatID = this.MainForm.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", false);
#endif
        }


        private void MarcDetailForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if ((stop != null && stop.State == 0)    // 0 表示正在处理
                || _processing > 0)
            {
                MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。或等待长时操作完成");
                e.Cancel = true;
                return;
            }
#endif

            if (/*this.EntitiesChanged == true
                || this.IssuesChanged == true
                || this.ObjectChanged == true
                || */this.BiblioChanged == true
                )
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
    "MarcDetailForm",
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

        // 对象信息是否被改变
        public bool ObjectChanged
        {
            get
            {
                /*
                if (this.binaryResControl1 != null)
                    return this.binaryResControl1.Changed;

                return false;
                 * */
                return false;
            }
            set
            {
                /*
                if (this.binaryResControl1 != null)
                    this.binaryResControl1.Changed = value;
                 * */
            }
        }

        // 书目信息是否被改变
        public bool BiblioChanged
        {
            get
            {
                if (this.MarcEditor != null)
                {
                    /*
                    // 如果object id有所改变，那么即便MARC没有改变，那最后的合成XML也发生了改变
                    if (this.binaryResControl1 != null)
                    {
                        if (this.binaryResControl1.IsIdChanged() == true)
                            return true;
                    }
                    */

                    return this.MarcEditor.Changed;
                }

                return false;
            }
            set
            {
                if (this.MarcEditor != null)
                    this.MarcEditor.Changed = value;

                if (value == false)
                {
                    this.RecordVersion = 0;
                }
            }
        }

        // 获得当前有修改标志的部分的名称
        string GetCurrentChangedPartName()
        {
            string strPart = "";

            if (this.BiblioChanged == true)
                strPart += "书目信息";

            /*
            if (this.EntitiesChanged == true)
            {
                if (strPart != "")
                    strPart += "和";
                strPart += "册信息";
            }

            if (this.IssuesChanged == true)
            {
                if (strPart != "")
                    strPart += "和";
                strPart += "期信息";
            }

            if (this.ObjectChanged == true)
            {
                if (strPart != "")
                    strPart += "和";
                strPart += "对象信息";
            }
             * */

            return strPart;
        }

        private void MarcDetailForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            Program.MainForm.AppInfo.SetString("marcDetialForm", "marcEditorState", this.MarcEditor.UiState);

            SaveSize();

            if (this.m_genDataViewer != null)
                this.m_genDataViewer.Close();

            if (this.m_verifyViewer != null)
                this.m_verifyViewer.Close();

            if (this.linkMarcFile != null)
                this.linkMarcFile.Close();

#if NO
            if (this.m_bSavePinyinGcatID == false)
                this.m_strPinyinGcatID = "";
            this.MainForm.AppInfo.SetString("entity_form", "gcat_pinyin_api_id", this.m_strPinyinGcatID);
            this.MainForm.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", this.m_bSavePinyinGcatID);
#endif
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                string strSelectedTemplates = selected_templates.Export();
                this.MainForm.AppInfo.SetString(
                    "marcdetailform",
                    "selected_templates",
                    strSelectedTemplates);
            }
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_FILL_MARCEDITOR_SCRIPT_MENU:
                    // 显示Ctrl+A菜单
                    if (this.MainForm.PanelFixedVisible == true)
                        this.AutoGenerate(this.MarcEditor,
                            new GenerateDataEventArgs(),
                            true);
                    return;
                case WM_LOADSIZE:
                    LoadSize();
                    return;
                case WM_VERIFY_DATA:
                    {
                        GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                        e1.FocusedControl = this.MarcEditor;

                        this.VerifyData(this, e1, null, true);
                        return;
                    }
            }
            base.DefWndProc(ref m);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.splitContainer_originDataMain);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.splitContainer_originDataMain);
                GuiState.SetUiState(controls, value);
            }
        }


        public void LoadSize()
        {
            if (this.SuppressSizeSetting == false)
            {
                // 设置窗口尺寸状态
                MainForm.AppInfo.LoadMdiChildFormStates(this,
                    "mdi_form_state",
                    SizeStyle.All,
                    MainForm.DefaultMdiWindowWidth,
                    MainForm.DefaultMdiWindowHeight);

#if NO
            // 获得splitContainer_originDataMain的状态
            int nValue = MainForm.AppInfo.GetInt(
            "marcdetailform",
            "splitContainer_originDataMain",
            -1);
            if (nValue != -1)
                this.splitContainer_originDataMain.SplitterDistance = nValue;
#endif
            }

            this.UiState = MainForm.AppInfo.GetString(
            "marcdetailform",
            "ui_state",
            "");

        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                if (this.SuppressSizeSetting == false)
                {
                    MainForm.AppInfo.SaveMdiChildFormStates(this,
                        "mdi_form_state");

#if NO
            // 保存splitContainer_originDataMain的状态
            MainForm.AppInfo.SetInt(
                "marcdetailform",
                "splitContainer_originDataMain",
                this.splitContainer_originDataMain.SplitterDistance);
#endif
                }

                MainForm.AppInfo.SetString(
    "marcdetailform",
    "ui_state",
    this.UiState);
            }
        }

        // 分析路径
        public static int ParsePath(string strPath,
            out string strProtocol,
            out string strResultsetName,
            out string strIndex,
            out string strError)
        {
            strError = "";
            strProtocol = "";
            strResultsetName = "";
            strIndex = "";

            int nRet = strPath.IndexOf(":");
            if (nRet == -1)
            {
                strError = "缺乏':'";
                return -1;
            }

            strProtocol = strPath.Substring(0, nRet);
            // 去掉":"
            strPath = strPath.Substring(nRet + 1);

            nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
            {
                strError = "缺乏/";
                return -1;
            }

            strResultsetName = strPath.Substring(0, nRet);
            strIndex = strPath.Substring(nRet + 1);

            return 0;
        }

        // 刷新关联的检索窗中可能缓存的记录。用于保存记录之后。
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshCachedRecord(
            string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.LinkedSearchForm == null)
            {
                strError = "没有关联的检索窗";
                goto ERROR1;
            }
            else
            {
                if (this.LinkedSearchForm.IsValid() == false)
                {
                    strError = "相关的Z39.50检索窗已经销毁，没有必要刷新";
                    return 0;
                }
            }

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 如果保存后协议名 和 原先连接的检索窗 没有发生变化
            if (strProtocol == this.LinkedSearchForm.CurrentProtocol)
            {
                nRet = this.LinkedSearchForm.RefreshOneRecord(
                    "path:" + strPath,
                    strAction,
                    out strError);
                return nRet;
            }

            // 如果协议名发生了变化
            strPath = this.textBox_tempRecPath.Text;
            if (String.IsNullOrEmpty(strPath) == true)
            {
                strError = "路径为空";
                return 0;
            }


            // 分离出各个部分
            strProtocol = "";
            string strResultsetName = "";
            string strIndex = "";

            nRet = ParsePath(strPath,
                out strProtocol,
                out strResultsetName,
                out strIndex,
                out strError);
            if (nRet == -1)
            {
                strError = "解析路径 '" + strPath + "' 字符串过程中发生错误: " + strError;
                goto ERROR1;
            }

            if (strProtocol != this.LinkedSearchForm.CurrentProtocol)
            {
                strError = "检索窗的协议已经发生改变";
                goto ERROR1;
            }

            if (strResultsetName != this.LinkedSearchForm.CurrentResultsetPath)
            {
                strError = "结果集已经发生改变";
                goto ERROR1;
            }

            int index = 0;

            index = Convert.ToInt32(strIndex) - 1;

            nRet = this.LinkedSearchForm.RefreshOneRecord(
                "index:" + index.ToString(),
                strAction,
                out strError);
            return nRet;
            ERROR1:
            return -1;
        }

        public void Reload()
        {
            if (String.IsNullOrEmpty(this.SavePath) == true)
                LoadRecord("current", true);
            else
                LoadRecordByPath("current", true);
        }



        // 根据数据库索引号位置装载记录
        // parameters:
        //      bReload 是否确保从数据库装载
        public int LoadRecordByPath(string strDirection,
            bool bReload = false)
        {
            string strError = "";
            int nRet = 0;

            if (strDirection == "prev")
            {
            }
            else if (strDirection == "next")
            {
            }
            else if (strDirection == "current")
            {
            }
            else
            {
                strError = "不能识别的strDirection参数值 '" + strDirection + "'";
                goto ERROR1;
            }

            if (this.BiblioChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                    "MarcDetailForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "记录路径为空，无法进行定位";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strProtocol == "dp2library")
            {
                dp2SearchForm dp2_searchform = null;

                if (this.LinkedSearchForm == null
                    || !(this.LinkedSearchForm is dp2SearchForm))
                {
                    dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行LoadRecord()";
                        goto ERROR1;
                    }
                }
                else
                {
                    dp2_searchform = (dp2SearchForm)this.LinkedSearchForm;
                }

                return LoadDp2Record(dp2_searchform,
                    strPath,
                    strDirection,
                    true,
                    bReload);
            }
            else if (strProtocol == "dtlp")
            {
                DtlpSearchForm dtlp_searchform = null;

                if (this.LinkedSearchForm == null
                    || !(this.LinkedSearchForm is DtlpSearchForm))
                {
                    dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法进行LoadRecord()";
                        goto ERROR1;
                    }
                }
                else
                {
                    dtlp_searchform = (DtlpSearchForm)this.LinkedSearchForm;
                }

                return LoadAmazonRecord(dtlp_searchform,
                    strPath,
                    strDirection,
                    true);
            }
            else if (strProtocol == "amazon")
            {
                AmazonSearchForm amazon_searchform = null;

                if (this.LinkedSearchForm == null
                    || !(this.LinkedSearchForm is AmazonSearchForm))
                {
                    amazon_searchform = this.GetAmazonSearchForm();

                    if (amazon_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法进行LoadRecord()";
                        goto ERROR1;
                    }
                }
                else
                {
                    amazon_searchform = (AmazonSearchForm)this.LinkedSearchForm;
                }

                return LoadAmazonRecord(amazon_searchform,
                    strPath,
                    strDirection,
                    true);
            }
            else
            {
                strError = "LoadRecordByPath()目前不支持 " + strProtocol + " 协议";
                goto ERROR1;
            }

            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

#if NO
        // 装载MARC记录，根据记录路径
        // parameters:
        //      strPath 路径。例如 "localhost/图书总库/ctlno/1"
        public int LoadDtlpRecord(DtlpSearchForm dtlp_searchform,
            string strPath,
            string strDirection,
            bool bLoadResObject)
        {
            string strError = "";

            if (dtlp_searchform == null)
            {
                strError = "dtlp_searchform参数不能为空";
                goto ERROR1;
            }

            Debug.Assert(dtlp_searchform.CurrentProtocol == dtlp_searchform.CurrentProtocol.ToLower(), "协议名应当采用小写");

            if (dtlp_searchform.CurrentProtocol != "dtlp")
            {
                strError = "所提供的检索窗不是dtlp协议";
                goto ERROR1;
            }

            DigitalPlatform.Z3950.Record record = null;
            Encoding currentEncoding = null;

            this.CurrentRecord = null;

            byte[] baTimestamp = null;
            string strOutStyle = "";

            string strOutputPath = "";
            string strMARC;

            long lVersion = 0;
            LoginInfo logininfo = null;
            string strXmlFragment = "";

            int nRet = dtlp_searchform.GetOneRecord(
                "marc",
                // strPath,
                // strDirection,
                0,  // test
                "path:" + strPath + ",direction:" + strDirection,
                "",
                out strOutputPath,
                out strMARC,
                out strXmlFragment,
                out strOutStyle,
                out baTimestamp,
                out lVersion,
                out record,
                out currentEncoding,
                out logininfo,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.domXmlFragment = null;

            this.CurrentTimestamp = baTimestamp;
            this.SavePath = dtlp_searchform.CurrentProtocol + ":" + strOutputPath;
            this.CurrentEncoding = currentEncoding;


            /*
            // 接着装入对象资源
            if (bLoadResObject == true)
            {
                this.binaryResControl1.Channel = dp2_searchform.GetChannel(dp2_searchform.GetServerUrl(strServerName));
                nRet = this.binaryResControl1.LoadObject(strLocalPath,
                    strRecordXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return -1;
                }
            }*/
            // 装入MARC编辑器
            this.MarcEditor.Marc = strMARC;

            this.CurrentRecord = record;
            if (this.m_currentRecord != null)
            {
                // 装入二进制编辑器
                this.binaryEditor_originData.SetData(
                    this.m_currentRecord.m_baRecord);

                // 装入ISO2709文本
                nRet = this.Set2709OriginText(this.m_currentRecord.m_baRecord,
                    this.CurrentEncoding,
                    out strError);
                if (nRet == -1)
                {
                    this.textBox_originData.Text = strError;
                }

                // 数据库名
                this.textBox_originDatabaseName.Text = this.m_currentRecord.m_strDBName;

                // Marc syntax OID
                this.textBox_originMarcSyntaxOID.Text = this.m_currentRecord.m_strSyntaxOID;

                // 让确定的OID起作用 2008/3/25
                if (String.IsNullOrEmpty(this.m_currentRecord.m_strSyntaxOID) == false)
                    this.AutoDetectedMarcSyntaxOID = "";
            }
            else
            {
                byte[] baMARC = this.CurrentEncoding.GetBytes(strMARC);
                // 装入二进制编辑器
                this.binaryEditor_originData.SetData(
                    baMARC);

                // 装入ISO2709文本
                nRet = this.Set2709OriginText(baMARC,
                    this.CurrentEncoding,
                    out strError);
                if (nRet == -1)
                {
                    this.textBox_originData.Text = strError;
                }
            }


            // 构造路径
            /*
            string strPath = searchform.CurrentProtocol + ":"
                + searchform.CurrentResultsetPath
                + "/" + (index + 1).ToString();

            this.textBox_tempRecPath.Text = strPath;
             * */
            this.textBox_tempRecPath.Text = "";


            this.MarcEditor.MarcDefDom = null; // 强制刷新字段名提示
            this.MarcEditor.RefreshNameCaption();

            this.BiblioChanged = false;

            if (this.MarcEditor.FocusedFieldIndex == -1)
                this.MarcEditor.FocusedFieldIndex = 0;

            this.MarcEditor.Focus();
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

#endif

        // 装载MARC记录，根据记录路径
        // parameters:
        //      strPath 路径。例如 "图书总库/1@本地服务器"
        //      bReload 是否确保从数据库装载
        public int LoadDp2Record(dp2SearchForm dp2_searchform,
            string strPath,
            string strDirection,
            bool bLoadResObject,
            bool bReload = false)
        {
            string strError = "";

            if (dp2_searchform == null)
            {
                strError = "dp2_searchform参数不能为空";
                goto ERROR1;
            }

            if (dp2_searchform.CurrentProtocol != "dp2library")
            {
                strError = "所提供的检索窗不是 dp2library 协议";
                goto ERROR1;
            }

            _processing++;
            try
            {

                DigitalPlatform.OldZ3950.Record record = null;
                Encoding currentEncoding = null;

                this.CurrentRecord = null;

                byte[] baTimestamp = null;
                string strOutStyle = "";

                string strSavePath = "";
                string strMARC;

                long lVersion = 0;
                LoginInfo logininfo = null;
                string strXmlFragment = "";

                int nRet = dp2_searchform.GetOneRecord(
                    //true,
                    "marc",
                    //strPath,
                    //strDirection,
                    0,  // test
                    "path:" + strPath + ",direction:" + strDirection,
                    bReload == true ? "reload" : "",
                    out strSavePath,
                    out strMARC,
                    out strXmlFragment,
                    out strOutStyle,
                    out baTimestamp,
                    out lVersion,
                    out record,
                    out currentEncoding,
                    out logininfo,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = this.LoadXmlFragment(strXmlFragment,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.CurrentTimestamp = baTimestamp;
                // this.SavePath = dp2_searchform.CurrentProtocol + ":" + strOutputPath;
                this.SavePath = strSavePath;
                this.CurrentEncoding = currentEncoding;

#if TEST
                // 测试：休眠一段时间，然后出让控制权
                Thread.Sleep(3000);
                Application.DoEvents();
#endif

                // 装入MARC编辑器
                this.MarcEditor.Marc = strMARC;

                this.m_nDisableInitialAssembly++;
                this.CurrentRecord = record;
                this.m_nDisableInitialAssembly--;

                if (this.m_currentRecord != null)
                {
                    // 装入二进制编辑器
                    this.binaryEditor_originData.SetData(
                        this.m_currentRecord.m_baRecord);

                    // 装入ISO2709文本
                    nRet = this.Set2709OriginText(this.m_currentRecord.m_baRecord,
                        this.CurrentEncoding,
                        out strError);
                    if (nRet == -1)
                    {
                        this.textBox_originData.Text = strError;
                    }

                    // 数据库名
                    this.textBox_originDatabaseName.Text = this.m_currentRecord.m_strDBName;

                    // Marc syntax OID
                    this.textBox_originMarcSyntaxOID.Text = this.m_currentRecord.m_strSyntaxOID;

                    // 2014/5/18
                    if (this.UseAutoDetectedMarcSyntaxOID == true)
                    {
                        this.AutoDetectedMarcSyntaxOID = this.m_currentRecord.AutoDetectedSyntaxOID;
                        if (string.IsNullOrEmpty(this.AutoDetectedMarcSyntaxOID) == false)
                            this.textBox_originMarcSyntaxOID.Text = this.AutoDetectedMarcSyntaxOID;
                    }

#if NO
                // 让确定的OID起作用 2008/3/25
                if (String.IsNullOrEmpty(this.m_currentRecord.m_strSyntaxOID) == false)
                    this.AutoDetectedMarcSyntaxOID = "";
#endif
                }
                else
                {
                    byte[] baMARC = this.CurrentEncoding.GetBytes(strMARC);
                    // 装入二进制编辑器
                    this.binaryEditor_originData.SetData(
                        baMARC);

                    // 装入ISO2709文本
                    nRet = this.Set2709OriginText(baMARC,
                        this.CurrentEncoding,
                        out strError);
                    if (nRet == -1)
                    {
                        this.textBox_originData.Text = strError;
                    }
                }

                DisplayHtml(strMARC, this.textBox_originMarcSyntaxOID.Text);

                // 构造路径
                /*
                string strPath = searchform.CurrentProtocol + ":"
                    + searchform.CurrentResultsetPath
                    + "/" + (index + 1).ToString();

                this.textBox_tempRecPath.Text = strPath;
                 * */
                if (strDirection != "current")  // 2013/9/18
                    this.textBox_tempRecPath.Text = "";


                this.MarcEditor.MarcDefDom = null; // 强制刷新字段名提示
                this.MarcEditor.RefreshNameCaption();

                this.BiblioChanged = false;

                if (this.MarcEditor.FocusedFieldIndex == -1)
                    this.MarcEditor.FocusedFieldIndex = 0;

                this.MarcEditor.Focus();
                return 0;
            }
            finally
            {
                _processing--;
            }
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 装载MARC记录，根据记录路径
        // parameters:
        //      strPath 路径。例如 "localhost/图书总库/ctlno/1"
        public int LoadAmazonRecord(ISearchForm searchform,
            string strPath,
            string strDirection,
            bool bLoadResObject)
        {
            string strError = "";

            if (searchform == null)
            {
                strError = "searchform 参数不能为空";
                goto ERROR1;
            }

            Debug.Assert(searchform.CurrentProtocol == searchform.CurrentProtocol.ToLower(), "协议名应当采用小写");
#if NO
            if (dtlp_searchform.CurrentProtocol != "amazon")
            {
                strError = "所提供的检索窗不是dtlp协议";
                goto ERROR1;
            }
#endif

            DigitalPlatform.OldZ3950.Record record = null;
            Encoding currentEncoding = null;

            this.CurrentRecord = null;

            byte[] baTimestamp = null;
            string strOutStyle = "";

            string strSavePath = "";
            string strMARC;

            long lVersion = 0;
            LoginInfo logininfo = null;
            string strXmlFragment = "";

            int nRet = searchform.GetOneRecord(
                "marc",
                //strPath,
                //strDirection,
                0,  // test
                "path:" + strPath + ",direction:" + strDirection,
                "",
                out strSavePath,
                out strMARC,
                                out strXmlFragment,
                out strOutStyle,
                out baTimestamp,
                                out lVersion,
                out record,
                out currentEncoding,
                                out logininfo,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.domXmlFragment = null;

            this.CurrentTimestamp = baTimestamp;
            // this.SavePath = searchform.CurrentProtocol + ":" + strOutputPath;
            this.SavePath = strSavePath;
            this.CurrentEncoding = currentEncoding;

            /*
            // 接着装入对象资源
            if (bLoadResObject == true)
            {
                this.binaryResControl1.Channel = dp2_searchform.GetChannel(dp2_searchform.GetServerUrl(strServerName));
                nRet = this.binaryResControl1.LoadObject(strLocalPath,
                    strRecordXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return -1;
                }
            }*/
            // 装入MARC编辑器
            this.MarcEditor.Marc = strMARC;


            this.CurrentRecord = record;
            if (this.m_currentRecord != null)
            {
                // 装入二进制编辑器
                this.binaryEditor_originData.SetData(
                    this.m_currentRecord.m_baRecord);

                // 装入ISO2709文本
                nRet = this.Set2709OriginText(this.m_currentRecord.m_baRecord,
                    this.CurrentEncoding,
                    out strError);
                if (nRet == -1)
                {
                    this.textBox_originData.Text = strError;
                }

                // 数据库名
                this.textBox_originDatabaseName.Text = this.m_currentRecord.m_strDBName;

                // Marc syntax OID
                this.textBox_originMarcSyntaxOID.Text = this.m_currentRecord.m_strSyntaxOID;

                // 让确定的OID起作用 2008/3/25
                if (String.IsNullOrEmpty(this.m_currentRecord.m_strSyntaxOID) == false)
                    this.AutoDetectedMarcSyntaxOID = "";
            }
            else
            {
                byte[] baMARC = this.CurrentEncoding.GetBytes(strMARC);
                // 装入二进制编辑器
                this.binaryEditor_originData.SetData(
                    baMARC);

                // 装入ISO2709文本
                nRet = this.Set2709OriginText(baMARC,
                    this.CurrentEncoding,
                    out strError);
                if (nRet == -1)
                {
                    this.textBox_originData.Text = strError;
                }
            }

            DisplayHtml(strMARC, this.textBox_originMarcSyntaxOID.Text);

            this.textBox_tempRecPath.Text = "";

            this.MarcEditor.MarcDefDom = null; // 强制刷新字段名提示
            this.MarcEditor.RefreshNameCaption();

            this.BiblioChanged = false;

            if (this.MarcEditor.FocusedFieldIndex == -1)
                this.MarcEditor.FocusedFieldIndex = 0;

            this.MarcEditor.Focus();
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        public Hashtable GetSelectedPinyin()
        {
            Hashtable result = new Hashtable();
            if (this.domXmlFragment == null)
                return result;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList nodes = this.domXmlFragment.DocumentElement.SelectNodes("dprms:selectedPinyin/dprms:entry", nsmgr);
            foreach (XmlNode node in nodes)
            {
                result[node.InnerText] = DomUtil.GetAttr(node, "pinyin");
            }

            return result;
        }

        public void SetSelectedPinyin(Hashtable table)
        {
            if (this.domXmlFragment == null)
            {
                this.domXmlFragment = new XmlDocument();
                this.domXmlFragment.LoadXml("<root />");
            }
            bool bChanged = false;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNode root = this.domXmlFragment.DocumentElement.SelectSingleNode("dprms:selectedPinyin", nsmgr);
            if (root == null)
            {
                root = this.domXmlFragment.CreateElement("dprms:selectedPinyin", DpNs.dprms);
                this.domXmlFragment.DocumentElement.AppendChild(root);
                bChanged = true;
            }
            else
            {
                if (String.IsNullOrEmpty(root.InnerXml) == false)
                {
                    root.InnerXml = ""; // 清除原来的全部下级元素
                    bChanged = true;
                }
            }

            if (table == null)
            {
                if (bChanged == true)
                    this.BiblioChanged = true;
                return;
            }

            foreach (string key in table.Keys)
            {
                // key为汉字
                XmlNode node = this.domXmlFragment.CreateElement("dprms:entry", DpNs.dprms);
                root.AppendChild(node);
                node.InnerText = key;
                DomUtil.SetAttr(node, "pinyin", (string)table[key]);
                bChanged = true;
            }

            if (bChanged == true)
                this.BiblioChanged = true;
        }

        // 装载书目以外的其它XML片断
        int LoadXmlFragment(string strXmlFragment,
            out string strError)
        {
            strError = "";

            this.domXmlFragment = null;

            if (string.IsNullOrEmpty(strXmlFragment) == true)
                return 0;

            this.domXmlFragment = new XmlDocument();
            this.domXmlFragment.LoadXml("<root />");

            try
            {
                this.domXmlFragment.DocumentElement.InnerXml = strXmlFragment;
            }
            catch (Exception ex)
            {
                strError = "装入XML Fragment到InnerXml时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // parameters:
        //      bForceFullElementSet    是否强制用Full元素集。如果为false，表示无所谓，也就是说按照当前的元素集(有可能是Full，也有可能是Brief)
        //      bReload                 是否确保从数据库装载
        public int LoadRecord(string strDirection,
            bool bForceFullElementSet = false,
            bool bReload = false)
        {
            string strError = "";
            int nRet = 0;

            string strChangedWarning = "";

            if (this.ObjectChanged == true
                || this.BiblioChanged == true)
            {
                strChangedWarning = "当前有 "
                    + GetCurrentChangedPartName()
                + " 被修改过。\r\n\r\n";

                string strText = strChangedWarning;

                strText += "确实要装入新的书目记录 ?";

                // 警告新装入
                DialogResult result = MessageBox.Show(this,
                    strText,
                    "MarcDetailForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            this._processing++;
            try
            {
                // 当处于连接MARC文件状态时
                if (this.linkMarcFile != null)
                {
                    string strMarc = "";
                    byte[] baRecord = null;

                    if (strDirection == "next")
                    {
                        //	    2	结束(当前返回的记录无效)
                        nRet = this.linkMarcFile.NextRecord(out strMarc,
                            out baRecord,
                            out strError);
                        if (nRet == 2)
                        {
                            strError = "到尾";
                            goto ERROR1;
                        }
                    }
                    else if (strDirection == "prev")
                    {
                        nRet = this.linkMarcFile.PrevRecord(out strMarc,
                            out baRecord,
                            out strError);
                        if (nRet == 1)
                        {
                            strError = "到头";
                            goto ERROR1;
                        }
                    }
                    else if (strDirection == "current")
                    {
                        nRet = this.linkMarcFile.CurrentRecord(out strMarc,
                            out baRecord,
                            out strError);
                        if (nRet == 1)
                        {
                            strError = "??";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "不能识别的strDirection参数值 '" + strDirection + "'";
                        goto ERROR1;
                    }
                    if (nRet == -1)
                        goto ERROR1;

                    LoadLinkedMarcRecord(strMarc, baRecord);
                    return 0;
                }

                if (this.LinkedSearchForm == null)
                {
                    strError = "没有关联的检索窗";
                    goto ERROR1;
                }

                string strPath = this.textBox_tempRecPath.Text;
                if (String.IsNullOrEmpty(strPath) == true)
                {
                    strError = "路径为空";
                    goto ERROR1;
                }

                // 分离出各个部分
                string strProtocol = "";
                string strResultsetName = "";
                string strIndex = "";

                nRet = ParsePath(strPath,
                    out strProtocol,
                    out strResultsetName,
                    out strIndex,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + strPath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }

                if (strProtocol != this.LinkedSearchForm.CurrentProtocol)
                {
                    strError = "检索窗的协议已经发生改变";
                    goto ERROR1;
                }

                if (strResultsetName != this.LinkedSearchForm.CurrentResultsetPath)
                {
                    strError = "结果集已经发生改变";
                    goto ERROR1;
                }

                int index = 0;

                index = Convert.ToInt32(strIndex) - 1;

                REDO:
                if (strDirection == "prev")
                {
                    index--;
                    if (index < 0)
                    {
                        strError = "到头";
                        goto ERROR1;
                    }
                }
                else if (strDirection == "current")
                {
                }
                else if (strDirection == "next")
                {
                    index++;
                }
                else
                {
                    strError = "不能识别的strDirection参数值 '" + strDirection + "'";
                    goto ERROR1;
                }

                if (this.LinkedSearchForm.IsValid() == false)
                {
                    strError = "连接的检索窗已经失效，无法载入记录 " + strPath;
                    goto ERROR1;
                }

                // return:
                //      -1  出错
                //      0   成功
                //      2   需要跳过
                nRet = LoadRecord(this.LinkedSearchForm, index, bForceFullElementSet, bReload);  // 
                if (nRet == 2)
                {
                    if (strDirection == "current")
                    {
                        strError = "当前位置 " + index.ToString() + " 是需要跳过的位置";
                        goto ERROR1;
                    }
                    goto REDO;
                }
                return nRet;
            }
            finally
            {
                this._processing--;
            }
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        int Set2709OriginText(byte[] baOrigin,
            Encoding encoding,
            out string strError)
        {
            strError = "";

            this.label_originDataWarning.Text = "";

            if (encoding == null)
            {
                int nRet = this.MainForm.GetEncoding(this.comboBox_originDataEncoding.Text,
                    out encoding,
                    out strError);
                if (nRet == -1)
                    return -1;

                /*
                // 2007/7/24 add
                if (encoding == null)
                    encoding = Encoding.UTF8;
                 * */
            }
            else
            {
                this.comboBox_originDataEncoding.Text = GetEncodingForm.GetEncodingName(this.CurrentEncoding);

            }

            this.textBox_originData.Text = (baOrigin == null ? "" : encoding.GetString(baOrigin));

            return 0;
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2catalog 
发送者 xxxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ObjectDisposedException
Message: 无法访问已释放的对象。
对象名:“MarcEditor”。
Stack:
在 System.Windows.Forms.Control.CreateHandle()
在 System.Windows.Forms.Control.get_Handle()
在 DigitalPlatform.Marc.Field.CalculateHeight(Graphics g, Boolean bIgnoreEdit)
在 DigitalPlatform.Marc.FieldCollection.AddInternal(String strName, String strIndicator, String strValue, Boolean bFireTextChanged, Boolean bInOrder, Int32& nOutputPosition)
在 DigitalPlatform.Marc.Record.SetMarc(String strMarc, Boolean bCheckMarcDef, String& strError)
在 DigitalPlatform.Marc.MarcEditor.set_Marc(String value)
在 dp2Catalog.MarcDetailForm.LoadRecord(ISearchForm searchform, Int32 index, Boolean bForceFullElementSet, Boolean bReload)
在 dp2Catalog.dp2SearchForm.LoadDetail(Int32 index, Boolean bOpenNew)
在 dp2Catalog.dp2SearchForm.listView_browse_DoubleClick(Object sender, EventArgs e)
在 System.Windows.Forms.ListView.WndProc(Message& m)
在 DigitalPlatform.GUI.ListViewNF.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Catalog 版本: dp2Catalog, Version=2.4.5698.23777, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1 
操作时间 2015/8/10 13:48:50 (Mon, 10 Aug 2015 13:48:50 +0800) 
前端地址 xxxx 经由 http://dp2003.com/dp2library 
         * */
        // 从检索窗装载MARC记录
        // parameters:
        //      bForceFullElementSet    是否强制用Full元素集。如果为false，表示无所谓，也就是说按照当前的元素集(有可能是Full，也有可能是Brief)
        //      bReload 是否确保从数据库装载
        // return:
        //      -1  出错
        //      0   成功
        //      2   需要跳过
        public int LoadRecord(ISearchForm searchform,
            int index,
            bool bForceFullElementSet = false,
            bool bReload = false)
        {
            string strError = "";

            if (searchform == null)
                throw new ArgumentException("searchform 参数不应为 null", "searchform");


            this._processing++;
            this.stop.BeginLoop();  // 在这里启用 stop，可以防止在装载的中途 Form 被关闭、造成 MarcEditor 设置 MARC 字符串过程抛出异常
            this.EnableControls(false);
            try
            {
#if TEST
                // 测试：休眠一段时间，然后出让控制权
                Thread.Sleep(3000);
                Application.DoEvents();
#endif
                string strMARC = "";

                this.LinkedSearchForm = searchform;
                // this.SavePath = "";  // 2011/5/5 去除

                DigitalPlatform.OldZ3950.Record record = null;
                Encoding currentEncoding = null;

                this.CurrentRecord = null;
                string strSavePath = "";
                byte[] baTimestamp = null;

                this.m_nDisableInitialAssembly++;   // 防止多次初始化Assembly
                try
                {
                    string strOutStyle = "";
                    LoginInfo logininfo = null;
                    string strXmlFragment = "";
                    long lVersion = 0;

                    string strParameters = "hilight_browse_line";
                    if (bForceFullElementSet == true)
                        strParameters += ",force_full";
                    if (bReload == true)
                        strParameters += ",reload";

                    // 获得一条MARC/XML记录
                    // return:
                    //      -1  error
                    //      0   suceed
                    //      1   为诊断记录
                    //      2   分割条，需要跳过这条记录
                    int nRet = searchform.GetOneRecord(
                        "marc",
                        index,  // 即将废止
                        "index:" + index.ToString(),
                        strParameters,  // true,
                        out strSavePath,
                        out strMARC,
                        out strXmlFragment,
                        out strOutStyle,
                        out baTimestamp,
                        out lVersion,
                        out record,
                        out currentEncoding,
                        out logininfo,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 2)
                        return 2;

                    nRet = this.LoadXmlFragment(strXmlFragment,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    this.LoginInfo = logininfo;

                    if (strOutStyle != "marc")
                    {
                        strError = "所获取的记录不是marc格式";
                        goto ERROR1;
                    }

                    this.RecordVersion = lVersion;

                    this.CurrentRecord = record;
                    if (this.m_currentRecord != null)
                    {
                        // 装入二进制编辑器
                        this.binaryEditor_originData.SetData(
                            this.m_currentRecord.m_baRecord);

                        // 装入ISO2709文本
                        nRet = this.Set2709OriginText(this.m_currentRecord.m_baRecord,
                            this.CurrentEncoding,
                            out strError);
                        if (nRet == -1)
                        {
                            this.textBox_originData.Text = strError;
                        }

                        // 数据库名
                        this.textBox_originDatabaseName.Text = this.m_currentRecord.m_strDBName;

                        // Marc syntax OID
                        this.textBox_originMarcSyntaxOID.Text = this.m_currentRecord.m_strSyntaxOID;

                        // 2014/5/18
                        if (this.UseAutoDetectedMarcSyntaxOID == true)
                        {
                            this.AutoDetectedMarcSyntaxOID = this.m_currentRecord.AutoDetectedSyntaxOID;
                            if (string.IsNullOrEmpty(this.AutoDetectedMarcSyntaxOID) == false)
                                this.textBox_originMarcSyntaxOID.Text = this.AutoDetectedMarcSyntaxOID;
                        }

#if NO
                    // 让确定的OID起作用 2008/3/25
                    if (String.IsNullOrEmpty(this.m_currentRecord.m_strSyntaxOID) == false)
                        this.AutoDetectedMarcSyntaxOID = "";
#endif
                    }
                    else
                    {
                        byte[] baMARC = this.CurrentEncoding.GetBytes(strMARC);
                        // 装入二进制编辑器
                        this.binaryEditor_originData.SetData(
                            baMARC);

                        // 装入ISO2709文本
                        nRet = this.Set2709OriginText(baMARC,
                            this.CurrentEncoding,
                            out strError);
                        if (nRet == -1)
                        {
                            this.textBox_originData.Text = strError;
                        }
                    }
                }
                finally
                {
                    this.m_nDisableInitialAssembly--;
                }

                this.SavePath = strSavePath;
                this.CurrentTimestamp = baTimestamp;
                this.CurrentEncoding = currentEncoding;

                // 装入MARC编辑器
                this.MarcEditor.Marc = strMARC;

                DisplayHtml(strMARC, this.textBox_originMarcSyntaxOID.Text);

                // 构造路径

                string strPath = searchform.CurrentProtocol + ":"
                    + searchform.CurrentResultsetPath
                    + "/" + (index + 1).ToString();

                this.textBox_tempRecPath.Text = strPath;

                this.MarcEditor.MarcDefDom = null; // 强制刷新字段名提示
                this.MarcEditor.RefreshNameCaption();

                this.BiblioChanged = false;

                if (this.MarcEditor.FocusedFieldIndex == -1)
                    this.MarcEditor.FocusedFieldIndex = 0;

                this.MarcEditor.Focus();
                return 0;
            }
            finally
            {
                this.stop.EndLoop();
                this.EnableControls(true);
                this._processing--;
            }
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        private void MarcDetailForm_Activated(object sender, EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            // 菜单
            MainForm.MenuItem_saveOriginRecordToIso2709.Enabled = true;
            MainForm.MenuItem_font.Enabled = true;
            MainForm.MenuItem_saveToTemplate.Enabled = true;
            MainForm.MenuItem_viewAccessPoint.Enabled = true;


            // 工具条按钮
            MainForm.toolButton_search.Enabled = false;
            MainForm.toolButton_prev.Enabled = true;
            MainForm.toolButton_next.Enabled = true;
            MainForm.toolButton_nextBatch.Enabled = false;

            MainForm.toolButton_getAllRecords.Enabled = false;
            MainForm.toolButton_saveTo.Enabled = true;
            MainForm.toolButton_saveToDB.Enabled = true;
            MainForm.toolButton_save.Enabled = true;
            MainForm.toolButton_delete.Enabled = true;

            MainForm.toolButton_loadTemplate.Enabled = true;

            MainForm.toolButton_dup.Enabled = true;
            MainForm.toolButton_verify.Enabled = true;
            MainForm.toolButton_refresh.Enabled = true;
            MainForm.toolButton_loadFullRecord.Enabled = true;

            if (this.m_verifyViewer != null)
            {
                if (m_verifyViewer.Docked == true
                    && this.MainForm.CurrentVerifyResultControl != m_verifyViewer.ResultControl)
                    this.MainForm.CurrentVerifyResultControl = m_verifyViewer.ResultControl;
            }
            else
            {
                this.MainForm.CurrentVerifyResultControl = null;
            }

            MarcDetailForm exist_fixed = this.MainForm.FixedMarcDetailForm;

            if (this.Fixed == false && exist_fixed != null)
                MainForm.toolStripButton_copyToFixed.Enabled = true;
            else
                MainForm.toolStripButton_copyToFixed.Enabled = false;

            SyncRecord();
        }

        // 同步 MARC 记录
        void SyncRecord()
        {
            string strError = "";
            int nRet = 0;

            string strMarcSyntax = "";
            string strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
            if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
            {
                /*
                strError = "当前MARC syntax OID为空，无法判断MARC具体格式";
                goto ERROR1;
                 * */
                return;
            }

            if (strMarcSyntaxOID == "1.2.840.10003.5.1")
                strMarcSyntax = "unimarc";
            if (strMarcSyntaxOID == "1.2.840.10003.5.10")
                strMarcSyntax = "usmarc";

            if (this.LinkedSearchForm != null
                && this.LinkedSearchForm is dp2SearchForm)
            {
                long lVersion = this.RecordVersion;
                string strMARC = this.MarcEditor.Marc;

                string strProtocol = "";
                string strPath = "";
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // return:
                //      -1  出错
                //      0   没有必要更新
                //      1   已经更新到 检索窗
                //      2   需要从 strMARC 中取出内容更新到记录窗
                nRet = this.LinkedSearchForm.SyncOneRecord(
                    strPath,
                    ref lVersion,
                    ref strMarcSyntax,
                    ref strMARC,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 2)
                {
                    this.MarcEditor.Marc = strMARC;
                    this.RecordVersion = lVersion;

                    DisplayHtml(strMARC, strMarcSyntaxOID);
                }
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void comboBox_originDataEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strError = "";

            if (this.m_currentRecord == null)
            {
                this.textBox_originData.Text = "缺乏当前记录";
                this.label_originDataWarning.Text = "";
                return;
            }

            byte[] baOriginData = this.m_currentRecord.m_baRecord;    // this.binaryEditor_originData.GetData(100*1024);

            // 装入ISO2709文本
            int nRet = this.Set2709OriginText(baOriginData,
                null,   // 当前编码方式
                out strError);
            if (nRet == -1)
            {
                this.textBox_originData.Text = strError;
            }
        }

        private void comboBox_originDataEncoding_TextChanged(object sender, EventArgs e)
        {
            comboBox_originDataEncoding_SelectedIndexChanged(this, null);
        }

        public void SaveRecordToWorksheet()
        {
            string strError = "";
            int nRet = 0;

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的工作单文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = MainForm.LastWorksheetFileName;
            dlg.Filter = "工作单文件 (*.wor)|*.wor|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "ZSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性


            MainForm.LastWorksheetFileName = dlg.FileName;

            StreamWriter sw = null;

            try
            {
                // 创建文件
                sw = new StreamWriter(MainForm.LastWorksheetFileName,
                    bAppend,	// append
                    System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + MainForm.LastWorksheetFileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                List<string> lines = null;
                // 将机内格式变换为工作单格式                    // return:
                //      -1  出错
                //      0   成功
                nRet = MarcUtil.CvtJineiToWorksheet(
                    this.MarcEditor.Marc,
                    -1,
                    out lines,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }

                if (bAppend == true)
                    MainForm.MessageText =
                        "1条记录成功追加到文件 " + MainForm.LastWorksheetFileName + " 尾部";
                else
                    MainForm.MessageText =
                        "1条记录成功保存到新文件 " + MainForm.LastWorksheetFileName + " 尾部";

            }
            catch (Exception ex)
            {
                strError = "写入文件 " + MainForm.LastWorksheetFileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                sw.Close();
            }

            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 将原始记录保存到ISO2709文件
        // TODO: 如果记录发生了修改，则不再保存原始记录，而要保存MARC编辑器中的记录？
        public void SaveOriginRecordToIso2709()
        {
            string strError = "";
            int nRet = 0;

            byte[] baTarget = null; // 如果已经是ISO2709格式，则存储在这里
            string strMarc = "";    // 如果是MARC机内格式，存储在这里

            // 如果不是从Z39.50协议过来的记录，或者记录在MARC编辑窗中已经修改过
            if (this.m_currentRecord == null
                || (this.m_currentRecord != null && this.m_currentRecord.m_baRecord == null)    // 2008/4/14
                || this.MarcEditor.Changed == true)
            {
                // strError = "没有当前记录";
                // goto ERROR1;
                // 从MARC编辑器中取记录
                strMarc = this.MarcEditor.Marc;
                baTarget = null;
            }
            else
            {
                strMarc = "";
                baTarget = this.m_currentRecord.m_baRecord;

            }

            Encoding preferredEncoding = this.CurrentEncoding;

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = true;
            dlg.GetEncoding -= new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.GetEncoding += new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.FileName = MainForm.LastIso2709FileName;
            // dlg.CrLf = MainForm.LastCrLfIso2709;
            dlg.CrLfVisible = false;   // 2020/3/9
            dlg.RemoveField998 = MainForm.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(true);
            dlg.EncodingName =
                (String.IsNullOrEmpty(MainForm.LastEncodingName) == true ? GetEncodingForm.GetEncodingName(preferredEncoding) : MainForm.LastEncodingName);
            dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool unimarc_modify_100 = dlg.UnimarcModify100;

            Encoding targetEncoding = null;

            if (dlg.EncodingName == "MARC-8"
    && preferredEncoding.Equals(this.MainForm.Marc8Encoding) == false)
            {
                strError = "保存操作无法进行。只有在记录的原始编码方式为 MARC-8 时，才能使用这个编码方式保存记录。";
                goto ERROR1;
            }

            nRet = this.MainForm.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            string strLastFileName = MainForm.LastIso2709FileName;
            string strLastEncodingName = MainForm.LastEncodingName;


            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "ZSearchForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }

            // 检查同一个文件连续存时候的编码方式一致性
            if (strLastFileName == dlg.FileName
                && bAppend == true)
            {
                if (strLastEncodingName != ""
                    && strLastEncodingName != dlg.EncodingName)
                {
                    DialogResult result = MessageBox.Show(this,
                        "文件 '" + dlg.FileName + "' 已在先前已经用 " + strLastEncodingName + " 编码方式存储了记录，现在又以不同的编码方式 " + dlg.EncodingName + " 追加记录，这样会造成同一文件中存在不同编码方式的记录，可能会令它无法被正确读取。\r\n\r\n是否继续? (是)追加  (否)放弃操作",
                        "ZSearchForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃处理...";
                        goto ERROR1;
                    }
                }
            }

            MainForm.LastIso2709FileName = dlg.FileName;
            MainForm.LastCrLfIso2709 = dlg.CrLf;
            MainForm.LastEncodingName = dlg.EncodingName;
            MainForm.LastRemoveField998 = dlg.RemoveField998;

            Stream s = null;
            try
            {
                s = File.Open(MainForm.LastIso2709FileName,
                     FileMode.OpenOrCreate);
                if (bAppend == false)
                    s.SetLength(0);
                else
                    s.Seek(0, SeekOrigin.End);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 " + MainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                string strMarcSyntax = "";

                // 当源和目标编码不同的时候，才需要获得MARC语法参数
                if (this.CurrentEncoding.Equals(targetEncoding) == false
                    || strMarc != "")
                {
                    string strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                    if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                    {
                        strError = "当前MARC syntax OID为空，无法判断MARC具体格式";
                        goto ERROR1;
                    }

                    if (strMarcSyntaxOID == "1.2.840.10003.5.1")
                        strMarcSyntax = "unimarc";
                    if (strMarcSyntaxOID == "1.2.840.10003.5.10")
                        strMarcSyntax = "usmarc";
                }

                if (strMarc != "")
                {
                    Debug.Assert(strMarcSyntax != "", "");

                    if (dlg.RemoveField998 == true)
                    {
                        MarcRecord temp = new MarcRecord(strMarc);
                        temp.select("field[@name='998']").detach();
                        temp.select("field[@name='997']").detach();
                        strMarc = temp.Text;
                    }

                    if (dlg.Mode880 == true && strMarcSyntax == "usmarc")
                    {
                        MarcRecord temp = new MarcRecord(strMarc);
                        MarcQuery.To880(temp);
                        strMarc = temp.Text;
                    }

                    // 将MARC机内格式转换为ISO2709格式
                    // parameters:
                    //      strSourceMARC   [in]机内格式MARC记录。
                    //      strMarcSyntax   [in]为"unimarc"或"usmarc"
                    //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
                    //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = MarcUtil.CvtJineiToISO2709(
                        strMarc,
                        strMarcSyntax,
                        targetEncoding,
                        unimarc_modify_100 ? "unimarc_100" : "",
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else if (this.CurrentEncoding.Equals(targetEncoding) == true)
                {
                    // source和target编码方式相同，不用转换
                    // baTarget = this.CurrentRecord.m_baRecord;
                    Debug.Assert(strMarcSyntax == "", "");

                    // 规范化 ISO2709 物理记录
                    // 主要是检查里面的记录结束符是否正确，去掉多余的记录结束符
                    baTarget = MarcUtil.CononicalizeIso2709Bytes(targetEncoding,
                        baTarget);
                }
                else
                {
                    // baTarget = this.CurrentRecord.m_baRecord;

                    Debug.Assert(strMarcSyntax != "", "");

                    nRet = ZSearchForm.ChangeIso2709Encoding(
                        this.CurrentEncoding,
                        baTarget,
                        targetEncoding,
                        strMarcSyntax,
                        out baTarget,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                s.Seek(0, SeekOrigin.End);

                s.Write(baTarget, 0,
                    baTarget.Length);

                if (dlg.CrLf == true)
                {
                    byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                    s.Write(baCrLf, 0,
                        baCrLf.Length);
                }

                if (bAppend == true)
                    MainForm.MessageText =
                        "1条记录成功追加到文件 " + MainForm.LastIso2709FileName + " 尾部";
                else
                    MainForm.MessageText =
                        "1条记录成功保存到新文件 " + MainForm.LastIso2709FileName + " 尾部";
            }
            catch (Exception ex)
            {
                strError = "写入文件 " + MainForm.LastIso2709FileName + " 失败，原因: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                s.Close();
            }
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void dlg_GetEncoding(object sender, GetEncodingEventArgs e)
        {
            string strError = "";
            Encoding encoding = null;
            int nRet = this.MainForm.GetEncoding(e.EncodingName,
                out encoding,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }
            e.Encoding = encoding;
        }

        // 获得当前记录的MARC格式OID
        string GetCurrentMarcSyntaxOID(out string strError)
        {
            strError = "";

            string strMarcSyntaxOID = "";

            if (String.IsNullOrEmpty(this.AutoDetectedMarcSyntaxOID) == false)
            {
                // 采纳自动识别的结果
                strMarcSyntaxOID = this.AutoDetectedMarcSyntaxOID;
            }
            else
            {
                if (this.m_currentRecord == null)
                {
                    strError = "没有当前记录信息(从而无法得知MARC格式)";
                    return null;
                }

                strMarcSyntaxOID = this.m_currentRecord.m_strSyntaxOID;

                // 2008/1/8
                if (strMarcSyntaxOID == "1.2.840.10003.5.109.10")
                    strMarcSyntaxOID = "1.2.840.10003.5.10";    // MARCXML当作USMARC处理
            }

            return strMarcSyntaxOID;
        }

        private void MarcEditor_GetConfigFile(object sender, DigitalPlatform.Marc.GetConfigFileEventArgs e)
        {
            int nRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(this.SavePath) == false)
            {
                string strProtocol = "";
                string strPath = "";

                // 分离出各个部分
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + this.SavePath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }

                if (strProtocol != "dp2library")
                    goto OTHER;

                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法获取配置文件";
                    goto ERROR1;
                }

                string strServerName = "";
                string strLocalPath = "";

                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                if (string.Compare(strServerName, "mem", true) == 0
                || string.Compare(strServerName, "file", true) == 0)
                    goto OTHER;

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                string strSyntax = "";
                // 获得一个数据库的数据syntax
                // parameters:
                //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetDbSyntax(
                    null,
                    strServerName,
                    strBiblioDbName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取书目库 '" + strBiblioDbName + "的数据格式时发生错误: " + strError;
                    goto ERROR1;
                }

                /*
                string strDefFilename = "";

                if (strSyntax == "unimarc"
                    || strSyntax == "usmarc")
                    strDefFilename = "marcdef";
                else
                {
                    strError = "所选书目库 '" + strBiblioDbName + "' 不是MARC格式的数据库";
                    goto ERROR1;
                }*/

                // 得到干净的文件名
                string strCfgFileName = e.Path;
                nRet = strCfgFileName.IndexOf("#");
                if (nRet != -1)
                {
                    strCfgFileName = strCfgFileName.Substring(0, nRet);
                }

                // 然后获得cfgs/template配置文件
                string strCfgFilePath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                string strCode = "";
                byte[] baCfgOutputTimestamp = null;
                nRet = dp2_searchform.GetCfgFile(
                    true,
                    strCfgFilePath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                e.Stream = new MemoryStream(Encoding.UTF8.GetBytes(strCode));
                return;
            }

            OTHER:
            {
                string strCfgFileName = e.Path;
                nRet = strCfgFileName.IndexOf("#");
                if (nRet != -1)
                {
                    strCfgFileName = strCfgFileName.Substring(0, nRet);
                }

                string strMarcSyntaxOID = "";

                strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                {
                    e.ErrorInfo = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                    return;
                }

                string strPath = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;

                try
                {
                    Stream s = File.OpenRead(strPath);

                    e.Stream = s;
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "文件  " + strPath + " 打开失败: " + ex.Message;
                }
            }
            return;
            ERROR1:
            e.ErrorInfo = strError;
        }

        private void MarcEditor_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            int nRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(this.SavePath) == false)
            {
                string strProtocol = "";
                string strPath = "";

                // 分离出各个部分
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + this.SavePath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }

                if (strProtocol != "dp2library")
                    goto OTHER;

                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法获取配置文件";
                    goto ERROR1;
                }

                string strServerName = "";
                string strLocalPath = "";

                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                if (string.Compare(strServerName, "mem", true) == 0
                || string.Compare(strServerName, "file", true) == 0)
                    goto OTHER;

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                string strSyntax = "";
                // 获得一个数据库的数据syntax
                // parameters:
                //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetDbSyntax(
                    null,
                    strServerName,
                    strBiblioDbName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取书目库 '" + strBiblioDbName + "的数据格式时发生错误: " + strError;
                    goto ERROR1;
                }

                /*
                string strDefFilename = "";

                if (strSyntax == "unimarc"
                    || strSyntax == "usmarc")
                    strDefFilename = "marcdef";
                else
                {
                    strError = "所选书目库 '" + strBiblioDbName + "' 不是MARC格式的数据库";
                    goto ERROR1;
                }*/

                // 得到干净的文件名
                string strCfgFileName = e.Path;
                nRet = strCfgFileName.IndexOf("#");
                if (nRet != -1)
                {
                    strCfgFileName = strCfgFileName.Substring(0, nRet);
                }

                // 然后获得cfgs/template配置文件
                string strCfgFilePath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                e.XmlDocument = this.MainForm.DomCache.FindObject(strCfgFilePath);
                if (e.XmlDocument != null)
                    return;

                string strCode = "";
                byte[] baCfgOutputTimestamp = null;
                nRet = dp2_searchform.GetCfgFile(
                    true,
                    strCfgFilePath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strCode);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "装载配置文件 '" + strCfgFilePath + "' 到 XMLDOM 的过程中出现错误: " + ex.Message;
                    return;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strCfgFilePath, dom);
                return;
            }

            OTHER:
            {
                string strCfgFileName = e.Path;
                nRet = strCfgFileName.IndexOf("#");
                if (nRet != -1)
                {
                    strCfgFileName = strCfgFileName.Substring(0, nRet);
                }


                string strMarcSyntaxOID = "";

                strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                {
                    e.ErrorInfo = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                    return;
                }


                string strPath = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;

                e.XmlDocument = this.MainForm.DomCache.FindObject(strPath);
                if (e.XmlDocument != null)
                    return;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strPath);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "装载配置文件 '" + strPath + "' 到 XMLDOM 的过程中出现错误: " + ex.Message;
                    return;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strPath, dom);
            }

            return;
            ERROR1:
            e.ErrorInfo = strError;
        }

        // 把全路径修改为追加形态的路径。也就是把记录id修改为?
        public static int ChangePathToAppendStyle(string strOriginPath,
            out string strOutputPath,
            out string strError)
        {
            strOutputPath = "";
            strError = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(strOriginPath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol == "dtlp")
            {
                nRet = strPath.IndexOf("/");
                if (nRet != -1)
                    strOutputPath = strProtocol + ":" + strPath.Substring(0, nRet) + "/?";
                else
                    strOutputPath = strProtocol + ":" + strPath;

                return 0;
            }
            else if (strProtocol == "dp2library")
            {
                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                strOutputPath = strProtocol + ":" + strBiblioDbName + "/?" + "@" + strServerName;
                return 0;
            }
            else if (strProtocol == "unioncatalog")
            {
                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                strOutputPath = strProtocol + ":" + strBiblioDbName + "/?" + "@" + strServerName;
                return 0;
            }
            else
            {
                strError = "不能识别的协议名 '" + strProtocol + "'";
                return -1;
            }
        }

        public int LinkMarcFile()
        {
            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.IsOutput = false;
            dlg.GetEncoding -= new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.GetEncoding += new GetEncodingEventHandler(dlg_GetEncoding);
            dlg.FileName = this.MainForm.LinkedMarcFileName;
            // dlg.CrLf = MainForm.LastCrLfIso2709;
            dlg.EncodingListItems = Global.GetEncodingList(true);
            // dlg.EncodingName = ""; GetEncodingForm.GetEncodingName(preferredEncoding);
            // dlg.EncodingComment = "注: 原始编码方式为 " + GetEncodingForm.GetEncodingName(preferredEncoding);
            // dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = true;

            if (String.IsNullOrEmpty(this.MainForm.LinkedEncodingName) == false)
                dlg.EncodingName = this.MainForm.LinkedEncodingName;
            if (String.IsNullOrEmpty(this.MainForm.LinkedMarcSyntax) == false)
                dlg.MarcSyntax = this.MainForm.LinkedMarcSyntax;

            this.MainForm.AppInfo.LinkFormState(dlg, "OpenMarcFileDlg_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            // 储存用过的文件名
            // 2009/9/21
            this.MainForm.LinkedMarcFileName = dlg.FileName;
            this.MainForm.LinkedEncodingName = dlg.EncodingName;
            this.MainForm.LinkedMarcSyntax = dlg.MarcSyntax;

            string strError = "";

            this.linkMarcFile = new LinkMarcFile();
            // return:
            //      -1  error
            //      0   succeed
            int nRet = this.linkMarcFile.Open(dlg.FileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.linkMarcFile.Encoding = dlg.Encoding;
            this.linkMarcFile.MarcSyntax = dlg.MarcSyntax;

            string strMarc = "";
            byte[] baRecord = null;
            //	    2	结束(当前返回的记录无效)
            nRet = this.linkMarcFile.NextRecord(out strMarc,
                out baRecord,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            // 2013/5/26
            if (nRet == 2)
                goto ERROR1;

            if (this.linkMarcFile.MarcSyntax == "<自动>"
            || this.linkMarcFile.MarcSyntax.ToLower() == "<auto>")
            {
                // 自动识别MARC格式
                string strOutMarcSyntax = "";
                // 探测记录的MARC格式 unimarc / usmarc / reader
                // return:
                //      0   没有探测出来。strMarcSyntax为空
                //      1   探测出来了
                nRet = MarcUtil.DetectMarcSyntax(strMarc,
                    out strOutMarcSyntax);
                this.linkMarcFile.MarcSyntax = strOutMarcSyntax;    // 有可能为空，表示探测不出来
                if (String.IsNullOrEmpty(this.linkMarcFile.MarcSyntax) == true)
                {
                    MessageBox.Show(this, "软件无法确定此MARC文件的MARC格式");
                }
            }

            if (dlg.Mode880 == true && linkMarcFile.MarcSyntax == "usmarc")
            {
                MarcRecord temp = new MarcRecord(strMarc);
                MarcQuery.ToParallel(temp);
                strMarc = temp.Text;
            }
            LoadLinkedMarcRecord(strMarc, baRecord);
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        void LoadLinkedMarcRecord(string strMarc,
            byte[] baRecord)
        {
            int nRet = 0;
            string strError = "";

            this.CurrentTimestamp = null;
            this.textBox_tempRecPath.Text = "marcfile:" + this.linkMarcFile.FileName + ":" + this.linkMarcFile.CurrentIndex.ToString();
            this.SavePath = "";
            this.CurrentEncoding = this.linkMarcFile.Encoding;

            string strMarcSyntax = this.linkMarcFile.MarcSyntax.ToLower();

            // 2009/9/21
            if (strMarcSyntax == "unimarc")
                this.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.1";   // UNIMARC
            else if (strMarcSyntax == "usmarc")
            {
                this.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.10";   // USMARC
                // this.MarcEditor.Lang = "en";
            }

            // 装入MARC编辑器
            this.MarcEditor.Marc = strMarc;


            this.CurrentRecord = null;

            DigitalPlatform.OldZ3950.Record record = new DigitalPlatform.OldZ3950.Record();
            if (strMarcSyntax == "unimarc" || strMarcSyntax == "")
                record.m_strSyntaxOID = "1.2.840.10003.5.1";
            else if (strMarcSyntax == "usmarc")
                record.m_strSyntaxOID = "1.2.840.10003.5.10";
            else if (strMarcSyntax == "dt1000reader")
                record.m_strSyntaxOID = "1.2.840.10003.5.dt1000reader";
            else
            {
                strError = "未知的MARC syntax '" + strMarcSyntax + "'";
                goto ERROR1;
            }

            record.m_baRecord = baRecord;

            this.CurrentRecord = record;

            DisplayHtml(strMarc, record.m_strSyntaxOID);

            {
                if (this.CurrentEncoding.Equals(this.MainForm.Marc8Encoding) == true)
                {
                }
                else
                {
                    if (baRecord == null)
                    {
                        baRecord = this.CurrentEncoding.GetBytes(strMarc);
                    }
                }

                if (baRecord != null)
                {
                    // 装入二进制编辑器
                    this.binaryEditor_originData.SetData(
                        baRecord);
                }
                else
                {
                    // TODO: 是否要清除原有内容?
                }

                this.label_originDataWarning.Text = "";
                if (baRecord != null)
                {
                    // 装入ISO2709文本
                    nRet = this.Set2709OriginText(baRecord,
                        this.CurrentEncoding,
                        out strError);
                    if (nRet == -1)
                    {
                        this.textBox_originData.Text = strError;
                        this.label_originDataWarning.Text = "";
                    }
                }
            }

            this.MarcEditor.MarcDefDom = null; // 强制刷新字段名提示
            this.MarcEditor.RefreshNameCaption();

            this.BiblioChanged = false;

            if (this.MarcEditor.FocusedFieldIndex == -1)
                this.MarcEditor.FocusedFieldIndex = 0;

            this.MarcEditor.Focus();
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 查重
        // parameters:
        //      strSender   触发命令的来源 "toolbar" "ctrl_d"
        public int SearchDup(string strSender)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(strSender == "toolbar" || strSender == "ctrl_d", "");

            string strStartPath = this.SavePath;

            // 检查当前通讯协议
            string strProtocol = "";
            string strPath = "";

            if (String.IsNullOrEmpty(strStartPath) == false)
            {
                nRet = Global.ParsePath(strStartPath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (strProtocol != "dp2library"
                    && strProtocol != "dtlp")
                    strStartPath = "";  // 迫使重新选择起点路径
            }
            else
            {
                // 选择协议
                SelectProtocolDialog protocol_dlg = new SelectProtocolDialog();
                GuiUtil.SetControlFont(protocol_dlg, this.Font);

                protocol_dlg.Protocols = new List<string>();
                protocol_dlg.Protocols.Add("dp2library");
                protocol_dlg.Protocols.Add("dtlp");
                protocol_dlg.StartPosition = FormStartPosition.CenterScreen;

                protocol_dlg.ShowDialog(this);

                if (protocol_dlg.DialogResult != DialogResult.OK)
                    return 0;

                strProtocol = protocol_dlg.SelectedProtocol;
            }

            this.EnableControls(false);
            try
            {

                // dtlp协议的查重
                if (strProtocol.ToLower() == "dtlp")
                {
                    // TODO: 如果起始路径为空，需要从系统配置中得到一个缺省的起始路径

                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法进行查重";
                        goto ERROR1;
                    }

                    // 打开查重窗口
                    DtlpDupForm form = new DtlpDupForm();

                    // form.MainForm = this.MainForm;
                    form.MdiParent = this.MainForm;
                    form.LoadDetail -= new LoadDetailEventHandler(dtlpdupform_LoadDetail);
                    form.LoadDetail += new LoadDetailEventHandler(dtlpdupform_LoadDetail);

                    string strCfgFilename = this.MainForm.DataDir + "\\dtlp_dup.xml";
                    nRet = form.Initial(strCfgFilename,
                        this.MainForm.stopManager,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        // 配置文件不存在对于编辑配置文件的界面无大碍，但是对查重却影响深远 -- 无法查重
                        strError = "配置文件 " + strCfgFilename + " 不存在。请利用主菜单“帮助/系统参数配置”命令，出现对话框后选“DTLP协议”属性页，按“查重方案配置”进行配置。";
                        goto ERROR1;
                    }


                    form.RecordPath = strPath;
                    form.ProjectName = "{default}"; // "<默认>"
                    form.MarcRecord = this.MarcEditor.Marc;
                    form.DtlpChannel = dtlp_searchform.DtlpChannel;

                    form.AutoBeginSearch = true;

                    form.Show();

                    return 0;
                }
                else if (strProtocol.ToLower() == "dp2library")
                {
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行查重";
                        goto ERROR1;
                    }
                    if (String.IsNullOrEmpty(strStartPath) == true)
                    {
                        /*
                        strError = "当前记录路径为空，无法进行查重";
                        goto ERROR1;
                         * */


                        string strDefaultStartPath = this.MainForm.DefaultSearchDupStartPath;

                        // 如果缺省起点路径定义为空，或者按下Control键强制要求出现对话框
                        if (String.IsNullOrEmpty(strDefaultStartPath) == true
                            || (Control.ModifierKeys == Keys.Control && strSender == "toolbar"))
                        {
                            // 变为正装形态
                            if (String.IsNullOrEmpty(strDefaultStartPath) == false)
                                strDefaultStartPath = Global.GetForwardStyleDp2Path(strDefaultStartPath);

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
                            dlg.Path = strDefaultStartPath;  // 采用遗留的上次用过的路径

                            this.MainForm.AppInfo.LinkFormState(dlg,
                                "searchdup_selectstartpath_dialog_state");

                            dlg.ShowDialog(this);

                            this.MainForm.AppInfo.UnlinkFormState(dlg);

                            if (dlg.DialogResult != DialogResult.OK)
                                return 0;

                            strDefaultStartPath = Global.GetBackStyleDp2Path(dlg.Path + "/?");

                            // 重新设置到系统参数中
                            this.MainForm.DefaultSearchDupStartPath = strDefaultStartPath;
                        }

                        // strProtocol = "dp2library";
                        strPath = strDefaultStartPath;
                    }


                    //// 
                    /*
                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行查重";
                        goto ERROR1;
                    }*/

                    // 将strPath解析为server url和local path两个部分
                    string strServerName = "";
                    string strPurePath = "";
                    dp2SearchForm.ParseRecPath(strPath,
                        out strServerName,
                        out strPurePath);

                    /*
                    // 获得server url

                    string strServerUrl = dp2_searchform.GetServerUrl(strServerName);
                    if (strServerUrl == null)
                    {
                        strError = "未能找到名为 '" + strServerName + "' 的服务器";
                        goto ERROR1;
                    }
                    */

                    string strDbName = dp2SearchForm.GetDbName(strPurePath);
                    string strSyntax = "";


                    // 获得一个数据库的数据syntax
                    // parameters:
                    //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                    //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = dp2_searchform.GetDbSyntax(null, // this.stop, bug!!!
                        strServerName,
                        strDbName,
                        out strSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (String.IsNullOrEmpty(strSyntax) == true)
                        strSyntax = "unimarc";

                    string strXml = "";
                    // 获得书目记录的XML格式
                    // parameters:
                    //      strMarcSyntax 要创建的XML记录的marcsyntax。
                    /*
                    nRet = dp2SearchForm.GetBiblioXml(
                        strSyntax,
                        this.MarcEditor.Marc,
                        out strXml,
                        out strError);
                     * */
                    // 2008/5/16 changed
                    nRet = MarcUtil.Marc2Xml(
    this.MarcEditor.Marc,
    strSyntax,
    out strXml,
    out strError);

                    if (nRet == -1)
                        goto ERROR1;

                    // 打开查重窗口
                    dp2DupForm form = new dp2DupForm();

                    form.MainForm = this.MainForm;
                    form.MdiParent = this.MainForm;

                    form.LibraryServerName = strServerName;
                    form.ProjectName = "<默认>";
                    form.XmlRecord = strXml;
                    form.RecordPath = strPurePath;

                    form.AutoBeginSearch = true;

                    form.Show();

                    return 0;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持 Z39.50 协议的查重";
                    goto ERROR1;
                }
                else if (strProtocol.ToLower() == "amazon")
                {
                    strError = "目前暂不支持 amazon 协议的查重";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }
            }
            finally
            {
                this.EnableControls(true);
            }

            ERROR1:
            MessageBox.Show(this, strError);
            return -1;

        }

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

        void dtlpdupform_LoadDetail(object sender, LoadDetailEventArgs e)
        {
            string strError = "";
            DtlpSearchForm dtlp_searchform = null;

            /*
            // 要看看this合法不合法？
            if (this.IsValid() == false)
            {
                strError = "依存的MarcDetailForm已经销毁";
                MessageBox.Show(ForegroundWindow.Instance, strError);
                return;
            }*/

            if (this.LinkedSearchForm == null
                || this.LinkedSearchForm.IsValid() == false
                || !(this.LinkedSearchForm is DtlpSearchForm))
            {
                dtlp_searchform = this.GetDtlpSearchForm();

                if (dtlp_searchform == null)
                {
                    strError = "没有连接的或者打开的DTLP检索窗，无法进行LoadRecord()";
                    goto ERROR1;
                }
            }
            else
            {
                dtlp_searchform = (DtlpSearchForm)this.LinkedSearchForm;
            }

            MarcDetailForm detail = new MarcDetailForm();

            detail.MdiParent = this.MainForm;   // 这里不能用this.MdiParent。如果this已经关闭，this.MainForm还可以使用
            detail.MainForm = this.MainForm;
            detail.Show();
            int nRet = detail.LoadAmazonRecord(dtlp_searchform,
                e.RecordPath,
                "current",
                true);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }


        // 保存记录
        // parameters:
        //      strStyle    "saveas"另存  其他为普通保存
        public int SaveRecord(string strStyle = "save")
        {
            string strError = "";
            int nRet = 0;

            _processing++;
            try
            {
#if TEST
                // 测试：休眠一段时间，然后出让控制权
                Thread.Sleep(3000);
                Application.DoEvents();
#endif

                string strLastSavePath = MainForm.LastSavePath;
                if (String.IsNullOrEmpty(strLastSavePath) == false)
                {
                    string strOutputPath = "";
                    nRet = ChangePathToAppendStyle(strLastSavePath,
                        out strOutputPath,
                        out strError);
                    if (nRet == -1)
                    {
                        MainForm.LastSavePath = ""; // 避免下次继续出错 2011/3/4
                        goto ERROR1;
                    }
                    strLastSavePath = strOutputPath;
                }

                string strCurrentUserName = "";
                string strSavePath = this.SavePath == "" ? strLastSavePath : this.SavePath;

                if (strStyle == "save"
                    && string.IsNullOrEmpty(this.SavePath) == false
                    && (Control.ModifierKeys & Keys.Control) == 0)
                {
                    // 2011/8/8
                    // 保存时如果已经有了路径，就不用打开对话框了
                }
                else
                {
                    SaveRecordDlg dlg = new SaveRecordDlg();
                    GuiUtil.SetControlFont(dlg, this.Font);

                    dlg.MainForm = this.MainForm;
                    dlg.GetDtlpSearchParam += new GetDtlpSearchParamEventHandle(dlg_GetDtlpSearchParam);
                    dlg.GetDp2SearchParam += new GetDp2SearchParamEventHandle(dlg_GetDp2SearchParam);
                    if (strStyle == "save")
                        dlg.RecPath = this.SavePath == "" ? strLastSavePath : this.SavePath;
                    else
                    {
                        dlg.RecPath = strLastSavePath;  // 2011/6/19
                        dlg.Text = "另存记录";
                    }

                    // dlg.StartPosition = FormStartPosition.CenterScreen;
                    this.MainForm.AppInfo.LinkFormState(dlg, "SaveRecordDlg_state");
                    dlg.UiState = this.MainForm.AppInfo.GetString("MarcDetailForm", "SaveRecordDlg_uiState", "");
                    dlg.ShowDialog(this);
                    this.MainForm.AppInfo.SetString("MarcDetailForm", "SaveRecordDlg_uiState", dlg.UiState);

                    if (dlg.DialogResult != DialogResult.OK)
                        return 0;

                    MainForm.LastSavePath = dlg.RecPath;

                    strSavePath = dlg.RecPath;
                    strCurrentUserName = dlg.CurrentUserName;
                }


                /*
                if (String.IsNullOrEmpty(this.SavePath) == true)
                {
                    strError = "缺乏保存路径";
                    goto ERROR1;
                }
                 * */

                string strProtocol = "";
                string strPath = "";
                nRet = Global.ParsePath(strSavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.stop.BeginLoop();

                this.EnableControls(false);
                try
                {
                    // dtlp协议的记录保存
                    if (strProtocol.ToLower() == "dtlp")
                    {
                        DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                        if (dtlp_searchform == null)
                        {
                            strError = "没有连接的或者打开的DTLP检索窗，无法保存记录";
                            goto ERROR1;
                        }

                        /*
                        string strOutPath = "";
                        nRet = DtlpChannel.CanonicalizeWritePath(strPath,
                            out strOutPath,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        strPath = strOutPath;
                         * */
                        string strOutputPath = "";
                        byte[] baOutputTimestamp = null;
                        nRet = dtlp_searchform.SaveMarcRecord(
                            strPath,
                            this.MarcEditor.Marc,
                            this.CurrentTimestamp,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // TODO: 时间戳冲突?

                        this.SavePath = strProtocol + ":" + strOutputPath;
                        this.CurrentTimestamp = baOutputTimestamp;

                        this.BiblioChanged = false;

                        // 是否刷新MARC记录？
                        //AutoCloseMessageBox.Show(this, "保存成功");
                        // MessageBox.Show(this, "保存成功");
                        return 0;
                    }
                    else if (strProtocol.ToLower() == "dp2library")
                    {
                        dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                        if (dp2_searchform == null)
                        {
                            strError = "没有连接的或者打开的dp2检索窗，无法保存记录";
                            goto ERROR1;
                        }

#if NO
                    // 迫使登录一次
                    if (string.IsNullOrEmpty(strCurrentUserName) == true
                        && string.IsNullOrEmpty(this.CurrentUserName) == true)
                    {
                        string strServerName = "";
                        string strLocalPath = "";

                        // 解析记录路径。
                        // 记录路径为如下形态 "中文图书/1 @服务器"
                        dp2SearchForm.ParseRecPath(strPath,
                            out strServerName,
                            out strLocalPath);

                        string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);
                        string strSyntax = "";
                        nRet = dp2_searchform.GetDbSyntax(
    null,
    strServerName,
    strBiblioDbName,
    out strSyntax,
    out strError);
                    }
#endif

                        // 保存前的准备工作
                        {
                            // 初始化 dp2catalog_marc_autogen.cs 的 Assembly，并new MarcDetailHost对象
                            // return:
                            //      -2  清除了Assembly
                            //      -1  error
                            //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly (可能本来就是空)
                            //      1   重新(或者首次)初始化了Assembly
                            nRet = InitialAutogenAssembly(out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (this.m_detailHostObj != null)
                            {
                                // 模拟出this.SavePath 2011/11/22
                                string strOldSavePath = this.textBox_savePath.Text;
                                this.textBox_savePath.Text = strSavePath;
                                try
                                {
                                    BeforeSaveRecordEventArgs e = new BeforeSaveRecordEventArgs();
                                    e.CurrentUserName = strCurrentUserName;
                                    this.m_detailHostObj.BeforeSaveRecord(this.MarcEditor, e);
                                    if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                                    {
                                        MessageBox.Show(this, "保存前的准备工作失败: " + e.ErrorInfo + "\r\n\r\n但保存操作仍将继续");
                                    }
                                }
                                finally
                                {
                                    // 恢复this.SavePath
                                    this.textBox_savePath.Text = strOldSavePath;
                                }
                            }
                        }

                        byte[] baTimestamp = this.CurrentTimestamp;
                        string strMARC = this.MarcEditor.Marc;
                        string strFragment = "";
                        if (this.domXmlFragment != null
                            && this.domXmlFragment.DocumentElement != null)
                            strFragment = this.domXmlFragment.DocumentElement.InnerXml;

                        // 2014/5/12
                        string strMarcSyntax = "";
                        if (this.CurrentRecord != null)
                            strMarcSyntax = GetMarcSyntax(this.CurrentRecord.m_strSyntaxOID);

                        // 2014/5/18
                        if (string.IsNullOrEmpty(this.AutoDetectedMarcSyntaxOID) == false)
                            strMarcSyntax = GetMarcSyntax(this.AutoDetectedMarcSyntaxOID);

                        string strComment = "";
                        bool bOverwrite = false;

                        if (string.IsNullOrEmpty(this.SavePath) == false)
                        {
                            string strTempProtocol = "";
                            string strTempPath = "";
                            nRet = Global.ParsePath(this.SavePath,
                                out strTempProtocol,
                                out strTempPath,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            string strServerName = "";
                            string strPurePath = "";

                            dp2SearchForm.ParseRecPath(strTempPath,
    out strServerName,
    out strPurePath);

                            if (dp2SearchForm.IsAppendRecPath(strPurePath) == false)
                            {
                                string strServerUrl = dp2_searchform.GetServerUrl(strServerName);
                                strComment = "copy from " + strPurePath + "@" + strServerUrl;
                            }
                        }
                        else if (string.IsNullOrEmpty(this.textBox_tempRecPath.Text) == false)
                        {
                            strComment = "copy from " + this.textBox_tempRecPath.Text;
                        }

                        string strRights = "";
                        // 判断是否追加
                        {
                            string strServerName = "";
                            string strPurePath = "";

                            dp2SearchForm.ParseRecPath(strPath,
    out strServerName,
    out strPurePath);
                            if (dp2SearchForm.IsAppendRecPath(strPurePath) == false)
                                bOverwrite = true;

                            nRet = dp2_searchform.GetChannelRights(
                                    strServerName,
                                    out strRights,
                                    out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        bool bForceWverifyData = StringUtil.IsInList("client_forceverifydata", strRights);

                        bool bVerifyed = false;
                        if (bForceWverifyData == true)
                        {
                            GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                            e1.FocusedControl = this.MarcEditor;

                            // 0: 没有发现校验错误; 1: 发现校验警告; 2: 发现校验错误
                            nRet = this.VerifyData(this, e1, strSavePath, true);
                            if (nRet == 2)
                            {
                                strError = "MARC 记录经校验发现有错，被拒绝保存。请修改 MARC 记录后重新保存";
                                goto ERROR1;
                            }
                            bVerifyed = true;
                        }

                        REDO_SAVE_DP2:
                        string strOutputPath = "";
                        byte[] baOutputTimestamp = null;
                        // return:
                        //      -2  timestamp mismatch
                        //      -1  error
                        //      0   succeed
                        nRet = dp2_searchform.SaveMarcRecord(
                            true,
                            strPath,
                            strMARC,
                            strMarcSyntax,
                            baTimestamp,
                            strFragment,
                            strComment,
                            out strOutputPath,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == -2)
                        {
                            // 时间戳冲突了

                            // 装载目标记录
                            DigitalPlatform.OldZ3950.Record record = null;
                            Encoding currentEncoding = null;
                            byte[] baTargetTimestamp = null;
                            string strOutStyle = "";
                            string strTargetMARC = "";
                            string strError1 = "";

                            string strOutputSavePath = "";
                            long lVersion = 0;
                            LoginInfo logininfo = null;
                            string strXmlFragment = "";

                            nRet = dp2_searchform.GetOneRecord(
                                // true,
                                "marc",
                                //strPath,    // 不能有问号?
                                //"", // strDirection,
                                0,
                                "path:" + strPath,
                                "",
                                out strOutputSavePath,
                                out strTargetMARC,
                                out strXmlFragment,
                                out strOutStyle,
                                out baTargetTimestamp,
                                out lVersion,
                                out record,
                                out currentEncoding,
                                out logininfo,
                                out strError1);
                            if (nRet == -1)
                            {
                                strError = "保存记录时发生错误: " + strError + "，在重装入目标记录的时候又发生错误: " + strError1;
                                goto ERROR1;
                            }

                            nRet = this.LoadXmlFragment(strXmlFragment,
    out strError1);
                            if (nRet == -1)
                            {
                                strError1 = "保存记录时发生错误: " + strError + "，在重装入目标记录的时候又发生错误: " + strError1;
                                goto ERROR1;
                            }

                            // TODO: 检查源和目标的MARC格式是否一致？是否前面检查过了？
                            TwoBiblioDialog two_biblio_dlg = new TwoBiblioDialog();
                            GuiUtil.SetControlFont(two_biblio_dlg, this.Font);

                            two_biblio_dlg.Text = "覆盖书目记录";
                            two_biblio_dlg.MessageText = "即将被覆盖的目标记录和源内容不同。\r\n\r\n请问是否确定要用源内容覆盖目标记录?";
                            two_biblio_dlg.LabelSourceText = "源";
                            two_biblio_dlg.LabelTargetText = "目标 " + strPath;
                            two_biblio_dlg.MarcSource = strMARC;
                            two_biblio_dlg.MarcTarget = strTargetMARC;
                            two_biblio_dlg.ReadOnlyTarget = true;   // 初始时目标MARC编辑器不让进行修改

                            this.MainForm.AppInfo.LinkFormState(two_biblio_dlg, "TwoBiblioDialog_state");
                            two_biblio_dlg.ShowDialog(this);
                            this.MainForm.AppInfo.UnlinkFormState(two_biblio_dlg);

                            if (two_biblio_dlg.DialogResult == DialogResult.Cancel)
                            {
                                strError = "放弃保存";
                                goto ERROR1;
                                // return 0;   // 全部放弃
                            }

                            if (two_biblio_dlg.DialogResult == DialogResult.No)
                            {
                                strError = "放弃保存";
                                goto ERROR1;
                            }

                            if (two_biblio_dlg.EditTarget == false)
                                strMARC = two_biblio_dlg.MarcSource;
                            else
                                strMARC = two_biblio_dlg.MarcTarget;

                            baTimestamp = baTargetTimestamp;
                            goto REDO_SAVE_DP2;
                        }

                        this.SavePath = dp2_searchform.CurrentProtocol + ":" + strOutputPath;
                        this.CurrentTimestamp = baOutputTimestamp;

                        this.BiblioChanged = false;

                        this.MarcEditor.ClearMarcDefDom();
                        this.MarcEditor.RefreshNameCaption();

                        // 是否刷新MARC记录？
                        // MessageBox.Show(this, "保存成功");

                        if (bOverwrite == true
                            && this.LinkedSearchForm != null)
                        {
                            // return:
                            //      -2  不支持
                            //      -1  error
                            //      0   相关窗口已经销毁，没有必要刷新
                            //      1   已经刷新
                            //      2   在结果集中没有找到要刷新的记录
                            nRet = RefreshCachedRecord("refresh",
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(this, "记录保存已经成功，但刷新相关结果集内记录时出错: " + strError);
                        }

                        if (this.AutoVerifyData == true
                            && bVerifyed == false)
                        {
                            // API.PostMessage(this.Handle, WM_VERIFY_DATA, 0, 0);

                            GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                            e1.FocusedControl = this.MarcEditor;

                            // 0: 没有发现校验错误; 1: 发现校验警告; 2: 发现校验错误
                            nRet = this.VerifyData(this, e1, strSavePath, true);
                            if (nRet == 2)
                            {
                                strError = "MARC 记录经校验发现有错。记录已经保存。请修改 MARC 记录后重新保存";
                                MessageBox.Show(this, strError);
                            }
                        }

                        return 0;
                    }
                    else if (strProtocol.ToLower() == "unioncatalog")
                    {
                        string strServerName = "";
                        string strPurePath = "";
                        dp2SearchForm.ParseRecPath(strPath,
                            out strServerName,
                            out strPurePath);
                        if (String.IsNullOrEmpty(strServerName) == true)
                        {
                            strError = "路径不合法: 缺乏服务器名部分";
                            goto ERROR1;
                        }
                        if (String.IsNullOrEmpty(strPurePath) == true)
                        {
                            strError = "路径不合法：缺乏纯路径部分";
                            goto ERROR1;
                        }

                        byte[] baTimestamp = this.CurrentTimestamp;
                        string strMARC = this.MarcEditor.Marc;
                        string strMarcSyntax = "";

                        string strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                        if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                        {
                            strError = "当前MARC syntax OID为空，无法判断MARC具体格式";
                            goto ERROR1;
                        }

                        if (strMarcSyntaxOID == "1.2.840.10003.5.1")
                            strMarcSyntax = "unimarc";
                        if (strMarcSyntaxOID == "1.2.840.10003.5.10")
                            strMarcSyntax = "usmarc";

                        string strXml = "";

                        nRet = MarcUtil.Marc2Xml(
                            strMARC,
                            strMarcSyntax,
                            out strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        string strXml1 = "";
                        // 将机内使用的marcxml格式转化为marcxchange格式
                        nRet = MarcUtil.MarcXmlToXChange(strXml,
                            null,
                            out strXml1,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // TODO: 是否可以直接使用Z39.50属性对话框中的用户名和密码? 登录失败后才出现登录对话框
                        if (this.LoginInfo == null)
                            this.LoginInfo = new dp2Catalog.LoginInfo();

                        bool bRedo = false;
                        REDO_LOGIN:
                        if (string.IsNullOrEmpty(this.LoginInfo.UserName) == true
                            || bRedo == true)
                        {
                            LoginDlg login_dlg = new LoginDlg();
                            GuiUtil.SetControlFont(login_dlg, this.Font);

                            if (bRedo == true)
                                login_dlg.Comment = strError + "\r\n\r\n请重新登录";
                            else
                                login_dlg.Comment = "请指定用户名和密码";
                            login_dlg.UserName = this.LoginInfo.UserName;
                            login_dlg.Password = this.LoginInfo.Password;
                            login_dlg.SavePassword = true;
                            login_dlg.ServerUrl = strServerName;
                            login_dlg.StartPosition = FormStartPosition.CenterScreen;
                            login_dlg.ShowDialog(this);

                            if (login_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            {
                                strError = "放弃保存";
                                goto ERROR1;
                            }

                            this.LoginInfo.UserName = login_dlg.UserName;
                            this.LoginInfo.Password = login_dlg.Password;
                            strServerName = login_dlg.ServerUrl;
                        }

                        if (this.LoginInfo.UserName.IndexOf("/") != -1)
                        {
                            strError = "用户名中不能出现字符 '/'";
                            goto ERROR1;
                        }

                        string strOutputTimestamp = "";
                        string strOutputRecPath = "";
                        // parameters:
                        //      strAction   动作。为"new" "change" "delete" "onlydeletebiblio"之一。"delete"在删除书目记录的同时，会自动删除下属的实体记录。不过要求实体均未被借出才能删除。
                        // return:
                        //      -2  登录不成功
                        //      -1  出错
                        //      0   成功
                        nRet = UnionCatalog.UpdateRecord(
                            null,
                            strServerName,
                            this.LoginInfo.UserName + "/" + this.LoginInfo.Password,
                            dp2SearchForm.IsAppendRecPath(strPurePath) == true ? "new" : "change",
                            strPurePath,
                            "marcxchange",
                            strXml1,
                            ByteArray.GetHexTimeStampString(baTimestamp),
                            out strOutputRecPath,
                            out strOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == -2)
                        {
                            bRedo = true;
                            goto REDO_LOGIN;
                        }

                        this.CurrentTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);
                        this.SavePath = strProtocol + ":" + strOutputRecPath + "@" + strServerName;

                        this.BiblioChanged = false;

                        this.MarcEditor.ClearMarcDefDom();
                        this.MarcEditor.RefreshNameCaption();

                        // 是否刷新MARC记录？
                        // MessageBox.Show(this, "保存成功");

                        if (dp2SearchForm.IsAppendRecPath(strPurePath) == false
                            && this.LinkedSearchForm != null
                            && this.LinkedSearchForm is ZSearchForm)
                        {
                            nRet = RefreshCachedRecord("refresh",
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(this, "记录保存已经成功，但刷新相关结果集内记录时出错: " + strError);
                        }

                        return 0;
                    }
                    else if (strProtocol.ToLower() == "z3950")
                    {
                        strError = "目前暂不支持 Z39.50 协议的保存操作";
                        goto ERROR1;
                    }
                    else if (strProtocol.ToLower() == "amazon")
                    {
                        strError = "目前暂不支持 amazon 协议的保存操作";
                        goto ERROR1;
                    }
                    else
                    {
                        strError = "无法识别的协议名 '" + strProtocol + "'";
                        goto ERROR1;
                    }
                }
                finally
                {
                    this.stop.EndLoop();

                    this.EnableControls(true);
                }
            }
            finally
            {
                _processing--;
            }
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = null;

            if (this.LinkedSearchForm != null
                && this.LinkedSearchForm is DtlpSearchForm)
            {
                dtlp_searchform = (DtlpSearchForm)this.LinkedSearchForm;
            }
            else
            {
                dtlp_searchform = this.MainForm.TopDtlpSearchForm;

                if (dtlp_searchform == null)
                {
                    // 新开一个dtlp检索窗
                    FormWindowState old_state = this.WindowState;

                    dtlp_searchform = new DtlpSearchForm();
                    dtlp_searchform.MainForm = this.MainForm;
                    dtlp_searchform.MdiParent = this.MainForm;
                    dtlp_searchform.WindowState = FormWindowState.Minimized;
                    dtlp_searchform.Show();

                    this.WindowState = old_state;
                    this.Activate();

                    // 需要等待初始化操作彻底完成
                    dtlp_searchform.WaitLoadFinish();

                }
            }

            return dtlp_searchform;
        }

        dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            if (this.LinkedSearchForm != null
                && this.LinkedSearchForm.IsValid() == true   // 2008/3/17
                && this.LinkedSearchForm is dp2SearchForm)
            {
                dp2_searchform = (dp2SearchForm)this.LinkedSearchForm;
            }
            else
            {
                dp2_searchform = this.MainForm.TopDp2SearchForm;

                if (dp2_searchform == null)
                {
                    // 新开一个dp2检索窗
                    FormWindowState old_state = this.WindowState;

                    dp2_searchform = new dp2SearchForm();
                    dp2_searchform.MainForm = this.MainForm;
                    dp2_searchform.MdiParent = this.MainForm;
                    dp2_searchform.WindowState = FormWindowState.Minimized;
                    dp2_searchform.Show();

                    // 2008/3/17
                    this.WindowState = old_state;
                    this.Activate();

                    // 需要等待初始化操作彻底完成
                    dp2_searchform.WaitLoadFinish();
                }
            }

            return dp2_searchform;
        }

        AmazonSearchForm GetAmazonSearchForm()
        {
            AmazonSearchForm searchform = null;

            if (this.LinkedSearchForm != null
                && this.LinkedSearchForm.IsValid() == true   // 2008/3/17
                && this.LinkedSearchForm is AmazonSearchForm)
            {
                searchform = (AmazonSearchForm)this.LinkedSearchForm;
            }
            else
            {
                searchform = this.MainForm.TopAmazonSearchForm;

                if (searchform == null)
                {
                    // 新开一个dp2检索窗
                    FormWindowState old_state = this.WindowState;

                    searchform = new AmazonSearchForm();
                    searchform.MainForm = this.MainForm;
                    searchform.MdiParent = this.MainForm;
                    searchform.WindowState = FormWindowState.Minimized;
                    searchform.Show();

                    // 2008/3/17
                    this.WindowState = old_state;
                    this.Activate();

                    // 需要等待初始化操作彻底完成
                    // dp2_searchform.WaitLoadFinish();
                }
            }

            return searchform;
        }


        void dlg_GetDp2SearchParam(object sender, GetDp2SearchParamEventArgs e)
        {
            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

#if OLD_CHANNEL
            e.dp2Channels = dp2_searchform.Channels;
#endif
            e.MainForm = this.MainForm;
        }

        void dlg_GetDtlpSearchParam(object sender, GetDtlpSearchParamEventArgs e)
        {
            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            e.DtlpChannels = dtlp_searchform.DtlpChannels;
            e.DtlpChannel = dtlp_searchform.DtlpChannel;
        }

        public int GetAccessPoint()
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "缺乏保存路径";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.EnableControls(false);
            try
            {
                // dtlp协议
                if (strProtocol.ToLower() == "dtlp")
                {
                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法观察检索点";
                        goto ERROR1;
                    }

                    List<string> results = null;

                    // string strOutputPath = "";
                    // byte[] baOutputTimestamp = null;
                    nRet = dtlp_searchform.GetAccessPoint(
                        strPath,
                        this.MarcEditor.Marc,
                        out results,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    ViewAccessPointForm form = new ViewAccessPointForm();

                    form.MdiParent = this.MdiParent;
                    form.AccessPoints = results;
                    form.Show();

                    /*
                    string strText = "";

                    if (results != null)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            strText += results[i] + "\r\n";
                        }
                    }

                    MessageBox.Show(this, strText);
                     * */
                    return 1;
                }
                // dp2library协议
                else if (strProtocol.ToLower() == "dp2library")
                {

                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                        goto ERROR1;
                    }


                    return 1;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持 Z39.50 协议的观察检索点的操作";
                    goto ERROR1;
                }
                else if (strProtocol.ToLower() == "amazon")
                {
                    strError = "目前暂不支持 amazon 协议的观察检索点的操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }

            }
            finally
            {
                this.EnableControls(true);
            }

            // return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 删除记录
        // TODO: 需要增加对dp2和UnionCatalog协议的删除功能
        public int DeleteRecord()
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "缺乏保存路径";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strChangedWarning = "";

            if (this.ObjectChanged == true
                || this.BiblioChanged == true)
            {
                strChangedWarning = "当前有 "
                    + GetCurrentChangedPartName()
                // strChangedWarning
                + " 被修改过。\r\n\r\n";
            }

            string strText = strChangedWarning;

            strText += "确实要删除书目记录 \r\n" + strPath + " ";

            /*
            int nObjectCount = this.binaryResControl1.ObjectCount;
            if (nObjectCount != 0)
                strText += "和从属的 " + nObjectCount.ToString() + " 个对象";
             * */

            strText += " ?";

            // 警告删除
            DialogResult result = MessageBox.Show(this,
                strText,
                "MarcDetailForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return 0;
            }

            this.stop.BeginLoop();

            this.EnableControls(false);
            try
            {

                // dtlp协议的记录删除
                if (strProtocol.ToLower() == "dtlp")
                {
                    DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

                    if (dtlp_searchform == null)
                    {
                        strError = "没有连接的或者打开的DTLP检索窗，无法保存记录";
                        goto ERROR1;
                    }

                    // string strOutputPath = "";
                    // byte[] baOutputTimestamp = null;
                    nRet = dtlp_searchform.DeleteMarcRecord(
                        strPath,
                        this.CurrentTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    MessageBox.Show(this, "删除成功");
                    return 1;
                }
                // dp2library协议的记录删除
                else if (strProtocol.ToLower() == "dp2library")
                {

                    dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                    if (dp2_searchform == null)
                    {
                        strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                        goto ERROR1;
                    }



                    // string strOutputPath = "";
                    byte[] baOutputTimestamp = null;
                    // 删除一条MARC/XML记录
                    // parameters:
                    //      strSavePath 内容为"中文图书/1@本地服务器"。没有协议名部分。
                    // return:
                    //      -1  error
                    //      0   suceed
                    nRet = dp2_searchform.DeleteOneRecord(
                        strPath,
                        this.CurrentTimestamp,
                        out baOutputTimestamp,
                        out strError);
                    this.CurrentTimestamp = baOutputTimestamp;  // 即便发生错误，也要更新时间戳，以便后面继续删除
                    if (nRet == -1)
                        goto ERROR1;

                    this.ObjectChanged = false;
                    this.BiblioChanged = false;

                    MessageBox.Show(this, "删除成功");
                    // TODO: ZSearchForm中的记录是否也要清除?

                    if (this.LinkedSearchForm != null
    && this.LinkedSearchForm is ZSearchForm)
                    {
                        nRet = RefreshCachedRecord("delete",
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, "记录删除已经成功，但刷新相关结果集内记录时出错: " + strError);
                    }

                    return 1;
                }
                else if (strProtocol.ToLower() == "unioncatalog")
                {
                    string strServerName = "";
                    string strPurePath = "";
                    dp2SearchForm.ParseRecPath(strPath,
                        out strServerName,
                        out strPurePath);
                    if (String.IsNullOrEmpty(strServerName) == true)
                    {
                        strError = "路径不合法: 缺乏服务器名部分";
                        goto ERROR1;
                    }
                    if (String.IsNullOrEmpty(strPurePath) == true)
                    {
                        strError = "路径不合法：缺乏纯路径部分";
                        goto ERROR1;
                    }

                    byte[] baTimestamp = this.CurrentTimestamp;

                    // TODO: 是否可以直接使用Z39.50属性对话框中的用户名和密码? 登录失败后才出现登录对话框
                    if (this.LoginInfo == null)
                        this.LoginInfo = new dp2Catalog.LoginInfo();

                    bool bRedo = false;
                    REDO_LOGIN:
                    if (string.IsNullOrEmpty(this.LoginInfo.UserName) == true
                        || bRedo == true)
                    {
                        LoginDlg login_dlg = new LoginDlg();
                        GuiUtil.SetControlFont(login_dlg, this.Font);
                        if (bRedo == true)
                            login_dlg.Comment = strError + "\r\n\r\n请重新登录";
                        else
                            login_dlg.Comment = "请指定用户名和密码";
                        login_dlg.UserName = this.LoginInfo.UserName;
                        login_dlg.Password = this.LoginInfo.Password;
                        login_dlg.SavePassword = true;
                        login_dlg.ServerUrl = strServerName;
                        login_dlg.StartPosition = FormStartPosition.CenterScreen;
                        login_dlg.ShowDialog(this);

                        if (login_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            strError = "放弃保存";
                            goto ERROR1;
                        }

                        this.LoginInfo.UserName = login_dlg.UserName;
                        this.LoginInfo.Password = login_dlg.Password;
                        strServerName = login_dlg.ServerUrl;
                    }

                    if (this.LoginInfo.UserName.IndexOf("/") != -1)
                    {
                        strError = "用户名中不能出现字符 '/'";
                        goto ERROR1;
                    }

                    string strOutputTimestamp = "";
                    string strOutputRecPath = "";
                    // parameters:
                    //      strAction   动作。为"new" "change" "delete" "onlydeletebiblio"之一。"delete"在删除书目记录的同时，会自动删除下属的实体记录。不过要求实体均未被借出才能删除。
                    // return:
                    //      -2  登录不成功
                    //      -1  出错
                    //      0   成功
                    nRet = UnionCatalog.UpdateRecord(
                        null,
                        strServerName,
                        this.LoginInfo.UserName + "/" + this.LoginInfo.Password,
                        "delete",
                        strPurePath,
                        "", // format
                        null,
                        ByteArray.GetHexTimeStampString(baTimestamp),
                        out strOutputRecPath,
                        out strOutputTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == -2)
                    {
                        bRedo = true;
                        goto REDO_LOGIN;
                    }

                    this.CurrentTimestamp = ByteArray.GetTimeStampByteArray(strOutputTimestamp);

                    this.BiblioChanged = false;
                    this.ObjectChanged = false;

                    MessageBox.Show(this, "删除成功");

                    if (this.LinkedSearchForm != null
    && this.LinkedSearchForm is ZSearchForm)
                    {
                        nRet = RefreshCachedRecord("delete",
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, "记录删除已经成功，但刷新相关结果集内记录时出错: " + strError);
                    }


                    return 0;
                }
                else if (strProtocol.ToLower() == "z3950")
                {
                    strError = "目前暂不支持 Z39.50 协议的删除操作";
                    goto ERROR1;
                }
                else if (strProtocol.ToLower() == "amazon")
                {
                    strError = "目前暂不支持 amazon 协议的删除操作";
                    goto ERROR1;
                }
                else
                {
                    strError = "无法识别的协议名 '" + strProtocol + "'";
                    goto ERROR1;
                }

            }
            finally
            {
                this.stop.EndLoop();

                this.EnableControls(true);
            }

            // return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        public void EnableControls(bool bEnable)
        {
            this.textBox_tempRecPath.Enabled = bEnable;
            this.MarcEditor.Enabled = bEnable;
        }

        void LoadFontToMarcEditor()
        {
            string strFaceName = MainForm.AppInfo.GetString("marceditor",
                "fontface",
                GuiUtil.GetDefaultEditorFontName());  // "Arial Unicode MS"
            float fFontSize = (float)Convert.ToDouble(MainForm.AppInfo.GetString("marceditor",
                "fontsize",
                "12.0"));

            string strColor = MainForm.AppInfo.GetString("marceditor",
                "fontcolor",
                "");

            string strStyle = MainForm.AppInfo.GetString("marceditor",
                "fontstyle",
                "");
            FontStyle style = FontStyle.Regular;
            if (String.IsNullOrEmpty(strStyle) == false)
            {
                style = (FontStyle)Enum.Parse(typeof(FontStyle), strStyle, true);
            }

            if (String.IsNullOrEmpty(strColor) == false)
            {
                this.MarcEditor.ContentTextColor = ColorUtil.String2Color(strColor);
            }

            this.MarcEditor.Font = new Font(strFaceName, fFontSize, style);

            this.MarcEditor.EnterAsAutoGenerate = MainForm.AppInfo.GetBoolean(
                "marceditor",
                "EnterAsAutoGenerate",
                false);

        }

        void SaveFontForMarcEditor()
        {
            MainForm.AppInfo.SetString("marceditor",
                "fontface",
                this.MarcEditor.Font.FontFamily.Name);
            MainForm.AppInfo.SetString("marceditor",
                "fontsize",
                Convert.ToString(this.MarcEditor.Font.Size));

            //            string strStyle = Enum.GetName(typeof(FontStyle), this.MarcEditor.Font.Style);
            string strStyle = this.MarcEditor.Font.Style.ToString();


            MainForm.AppInfo.SetString("marceditor",
                "fontstyle",
                strStyle);


            MainForm.AppInfo.SetString("marceditor",
                "fontcolor",
                this.MarcEditor.ContentTextColor != DigitalPlatform.Marc.MarcEditor.DefaultBackColor ? ColorUtil.Color2String(this.MarcEditor.ContentTextColor) : "");
        }

        // 设置字体
        public void SetFont()
        {
            FontDialog dlg = new FontDialog();

            dlg.ShowColor = true;
            dlg.Color = this.MarcEditor.ContentTextColor;
            dlg.Font = this.MarcEditor.Font;
            dlg.ShowApply = true;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            dlg.Apply -= new EventHandler(dlgFont_Apply);
            dlg.Apply += new EventHandler(dlgFont_Apply);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            this.MarcEditor.Font = dlg.Font;
            this.MarcEditor.ContentTextColor = dlg.Color;

            // 保存到配置文件
            SaveFontForMarcEditor();
        }

        void dlgFont_Apply(object sender, EventArgs e)
        {
            FontDialog dlg = (FontDialog)sender;

            this.MarcEditor.Font = dlg.Font;
            this.MarcEditor.ContentTextColor = dlg.Color;

            // 保存到配置文件
            SaveFontForMarcEditor();
        }

        #region 为汉字加拼音相关功能



#if NO
                // 把字符串中的汉字转换为四角号码
        // parameters:
        //      bLocal  是否从本地获取四角号码
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        public int HanziTextToSjhm(
            bool bLocal,
            string strText,
            out List<string> sjhms,
            out string strError)
        {
            strError = "";
            sjhms = new List<string>();

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";


            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                {
                    continue;
                }

                // 汉字
                string strHanzi = "";
                strHanzi += ch;


                string strResultSjhm = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.MainForm.LoadQuickSjhm(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.MainForm.QuickSjhm.GetSjhm(
                        strHanzi,
                        out strResultSjhm,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取四角号码");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceled
                    return 0;
                }

                Debug.Assert(strResultSjhm != "", "");

                strResultSjhm = strResultSjhm.Trim();
                sjhms.Add(strResultSjhm);
            }

            return 1;   // 正常结束
        }

        GcatServiceClient m_gcatClient = null;
        string m_strPinyinGcatID = "";
        bool m_bSavePinyinGcatID = false;

        // 汉字字符串转换为拼音
        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        public int SmartHanziTextToPinyin(
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            Stop new_stop = new DigitalPlatform.Stop();
            new_stop.Register(MainForm.stopManager, true);	// 和容器关联
            new_stop.OnStop += new StopEventHandler(new_stop_OnStop);
            new_stop.Initial("正在获得 '" + strText + "' 的拼音信息 (从服务器 " + this.MainForm.PinyinServerUrl + ")...");
            new_stop.BeginLoop();

            m_gcatClient = null;
            try
            {

                m_gcatClient = GcatNew.CreateChannel(this.MainForm.PinyinServerUrl);

            REDO_GETPINYIN:
                int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字
                string strPinyinXml = "";
                // return:
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                int nRet = GcatNew.GetPinyin(
                    new_stop,
                    m_gcatClient,
                    m_strPinyinGcatID,
                    strText,
                    out strPinyinXml,
                    out strError);
                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
    "从服务器 '" + this.MainForm.PinyinServerUrl + "' 获取拼音的过程出错:\r\n" + strError + "\r\n\r\n是否要临时改为使用本机加拼音功能? \r\n\r\n(注：临时改用本机拼音的状态在程序退出时不会保留。如果要永久改用本机拼音方式，请使用主菜单的“参数配置”命令，将“服务器”属性页的“拼音服务器URL”内容清空)",
    "EntityForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        this.MainForm.ForceUseLocalPinyinFunc = true;
                        strError = "将改用本机拼音，请重新操作一次。(本次操作出错: " + strError + ")";
                        return -1;
                    }
                    strError = " " + strError;
                    return -1;
                }

                if (nRet == -2)
                {
                    IdLoginDialog login_dlg = new IdLoginDialog();
                    login_dlg.Text = "获得拼音 -- "
                        + ((string.IsNullOrEmpty(this.m_strPinyinGcatID) == true) ? "请输入ID" : strError);
                    login_dlg.ID = this.m_strPinyinGcatID;
                    login_dlg.SaveID = this.m_bSavePinyinGcatID;
                    login_dlg.StartPosition = FormStartPosition.CenterScreen;
                    if (login_dlg.ShowDialog(this) == DialogResult.Cancel)
                    {
                        return 0;
                    }

                    this.m_strPinyinGcatID = login_dlg.ID;
                    this.m_bSavePinyinGcatID = login_dlg.SaveID;
                    goto REDO_GETPINYIN;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strPinyinXml);
                }
                catch (Exception ex)
                {
                    strError = "strPinyinXml装载到XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                foreach (XmlNode nodeWord in dom.DocumentElement.ChildNodes)
                {
                    if (nodeWord.NodeType == XmlNodeType.Text)
                    {
                        SelPinyinDlg.AppendText(ref strPinyin, nodeWord.InnerText);
                        nStatus = 0;
                        continue;
                    }

                    if (nodeWord.NodeType != XmlNodeType.Element)
                        continue;

                    string strWordPinyin = DomUtil.GetAttr(nodeWord, "p");
                    if (string.IsNullOrEmpty(strWordPinyin) == false)
                        strWordPinyin = strWordPinyin.Trim();

                    // 目前只取多套读音的第一套
                    nRet = strWordPinyin.IndexOf(";");
                    if (nRet != -1)
                        strWordPinyin = strWordPinyin.Substring(0, nRet).Trim();

                    string[] pinyin_parts = strWordPinyin.Split(new char[] { ' ' });
                    int index = 0;
                    // 让选择多音字
                    foreach (XmlNode nodeChar in nodeWord.ChildNodes)
                    {
                        if (nodeChar.NodeType == XmlNodeType.Text)
                        {
                            SelPinyinDlg.AppendText(ref strPinyin, nodeChar.InnerText);
                            nStatus = 0;
                            continue;
                        }

                        string strHanzi = nodeChar.InnerText;
                        string strCharPinyins = DomUtil.GetAttr(nodeChar, "p");

                        if (String.IsNullOrEmpty(strCharPinyins) == true)
                        {
                            strPinyin += strHanzi;
                            nStatus = 0;
                            index++;
                            continue;
                        }

                        if (strCharPinyins.IndexOf(";") == -1)
                        {
                            DomUtil.SetAttr(nodeChar, "sel", strCharPinyins);
                            SelPinyinDlg.AppendPinyin(ref strPinyin,
                                SelPinyinDlg.ConvertSinglePinyinByStyle(
                                    strCharPinyins,
                                    style)
                                    );
                            nStatus = 2;
                            index++;
                            continue;
                        }

#if _TEST_PINYIN
                        // 调试！
                        string[] parts = strCharPinyins.Split(new char[] {';'});
                        {
                            DomUtil.SetAttr(nodeChar, "sel", parts[0]);
                            AppendPinyin(ref strPinyin, parts[0]);
                            nStatus = 2;
                            index++;
                            continue;
                        }
#endif


                        string strSampleText = "";
                        int nOffs = -1;
                        SelPinyinDlg.GetOffs(dom.DocumentElement,
                            nodeChar,
                out strSampleText,
                out nOffs);

                        {	// 如果是多个拼音
                            SelPinyinDlg dlg = new SelPinyinDlg();
                            float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            GuiUtil.SetControlFont(dlg, this.Font, false);
                            // 维持字体的原有大小比例关系
                            dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            // 这个对话框比较特殊 GuiUtil.SetControlFont(dlg, this.Font, false);

                            dlg.Text = "请选择汉字 '" + strHanzi + "' 的拼音 (来自服务器 " + this.MainForm.PinyinServerUrl + ")";
                            dlg.SampleText = strSampleText;
                            dlg.Offset = nOffs;
                            dlg.Pinyins = strCharPinyins;
                            if (index < pinyin_parts.Length)
                                dlg.ActivePinyin = pinyin_parts[index];
                            dlg.Hanzi = strHanzi;

                            MainForm.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                            dlg.ShowDialog(this);

                            MainForm.AppInfo.UnlinkFormState(dlg);

                            Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                            if (dlg.DialogResult == DialogResult.Abort)
                            {
                                return 0;   // 用户希望整个中断
                            }

                            DomUtil.SetAttr(nodeChar, "sel", dlg.ResultPinyin);

                            if (dlg.DialogResult == DialogResult.Cancel)
                            {
                                SelPinyinDlg.AppendText(ref strPinyin, strHanzi);
                                nStatus = 2;
                            }
                            else if (dlg.DialogResult == DialogResult.OK)
                            {
                                SelPinyinDlg.AppendPinyin(ref strPinyin,
                                    SelPinyinDlg.ConvertSinglePinyinByStyle(
                                    dlg.ResultPinyin,
                                    style)
                                    );
                                nStatus = 2;
                            }
                            else
                            {
                                Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                            }

                            index++;
                        }

                    }
                }

#if _TEST_PINYIN
#else
                // return:
                //      -2  strID验证失败
                //      -1  出错
                //      0   成功
                nRet = GcatNew.SetPinyin(
                    new_stop,
                    m_gcatClient,
                    "",
                    dom.DocumentElement.OuterXml,
                    out strError);
                if (nRet == -1)
                    return -1;
#endif

                return 1;
            }
            finally
            {
                new_stop.EndLoop();
                new_stop.OnStop -= new StopEventHandler(new_stop_OnStop);
                new_stop.Initial("");
                new_stop.Unregister();
                if (m_gcatClient != null)
                {
                    m_gcatClient.Close();
                    m_gcatClient = null;
                }
            }
        }


        void new_stop_OnStop(object sender, StopEventArgs e)
        {
            if (this.m_gcatClient != null)
            {
                this.m_gcatClient.Abort();
            }
        }
#endif

#if NO
        // 把字符串中的汉字和拼音分离
        // parameters:
        //      bLocal  是否从本地获取拼音
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        public int HanziTextToPinyin(
            bool bLocal,
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";


            string strHanzi;
            int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字


            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                strHanzi = "";

                if (ch >= 0 && ch <= 128)
                {
                    if (nStatus == 2)
                        strPinyin += " ";

                    strPinyin += ch;

                    if (ch == ' ')
                        nStatus = 1;
                    else
                        nStatus = 0;

                    continue;
                }
                else
                {	// 汉字
                    strHanzi += ch;
                }

                // 汉字前面出现了英文或者汉字，中间间隔空格
                if (nStatus == 2 || nStatus == 0)
                    strPinyin += " ";


                // 看看是否特殊符号
                if (strSpecialChars.IndexOf(strHanzi) != -1)
                {
                    strPinyin += strHanzi;	// 放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }


                // 获得拼音
                string strResultPinyin = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.MainForm.LoadQuickPinyin(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.MainForm.QuickPinyin.GetPinyin(
                        strHanzi,
                        out strResultPinyin,
                        out strError);
                }
                else
                {
                    throw new Exception("暂不支持从拼音库中获取拼音");
                    /*
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                     * */
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceld
                    strPinyin += strHanzi;	// 只好将汉字放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// 如果是多个拼音
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    // GuiUtil.SetControlFont(dlg, this.Font);
                    float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    GuiUtil.SetControlFont(dlg, this.Font, false);
                    // 维持字体的原有大小比例关系
                    dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);

                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    MainForm.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                    dlg.ShowDialog(this);

                    MainForm.AppInfo.UnlinkFormState(dlg);

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        strPinyin += strHanzi;
                    }
                    else if (dlg.DialogResult == DialogResult.OK)
                    {
                        strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                            dlg.ResultPinyin,
                            style);
                    }
                    else if (dlg.DialogResult == DialogResult.Abort)
                    {
                        return 0;   // 用户希望整个中断
                    }
                    else
                    {
                        Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                    }
                }
                else
                {
                    // 单个拼音

                    strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                        strResultPinyin,
                        style);
                }
                nStatus = 2;
            }

            return 1;   // 正常结束
        }
#endif

        #endregion

        private void MarcEditor_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this.AutoGenerate(sender, e);
        }

        private void MarcEditor_ControlLetterKeyPress(object sender,
            ControlLetterKeyPressEventArgs e)
        {
            // Ctrl + D 查重
            if (e.KeyData == (Keys.D | Keys.Control))
            {
                this.SearchDup("ctrl_d");
                e.Handled = true;
                return;
            }
        }

#if NO
        // 自动加工数据
        public void AutoGenerate()
        {
            string strError = "";
            string strCode = "";
            string strRef = "";


            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                strError = "缺乏保存路径";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // dtlp协议的自动创建数据
            if (strProtocol.ToLower() == "dtlp")
            {
                strError = "暂不支持来自DTLP协议的数据自动创建功能";
                goto ERROR1;
            }

            // dp2library协议的自动创建数据
            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                    goto ERROR1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                string strCfgFileName = "dp2catalog_marc_autogen.cs";

                string strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                strCfgFileName = "dp2catalog_marc_autogen.cs.ref";

                strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strRef,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }

            try
            {
                // 执行代码
                nRet = RunScript(strCode,
                    strRef,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = "执行脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

#if NO
        int RunScript(string strCode,
            string strRef,
            out string strError)
        {
            strError = "";
            string[] saRef = null;
            int nRet;
            // string strWarning = "";

            nRet = ScriptManager.GetRefsFromXml(strRef,
                out saRef,
                out strError);
            if (nRet == -1)
                return -1;

            // 2007/12/4
            ScriptManager.RemoveRefsBinDirMacro(ref saRef);

            string[] saAddRef = {
                                    // 2011/5/4 增加
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",

									//Environment.CurrentDirectory + "\\digitalplatform.xmleditor.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2catalog.exe"
								};

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            /*
            Assembly assembly = ScriptManager.CreateAssembly(
                strCode,
                saRef,
                null,	// strLibPaths,
                null,	// strOutputFile,
                out strError,
                out strWarning);
            if (assembly == null)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strError;
                return -1;
            }*/
            Assembly assembly = null;
            string strErrorInfo = "";
            string strWarningInfo = "";
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                null,   // strLibPaths,
                out assembly,
                out strErrorInfo,
                out strWarningInfo);
            if (nRet == -1)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strErrorInfo;
                return -1;
            }

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Catalog.MarcDetailHost");
            if (entryClassType == null)
            {
                strError = "dp2Catalog.MarcDetailHost派生类没有找到";
                return -1;
            }

            // new一个MarcDetailHost派生对象
            MarcDetailHost hostObj = (MarcDetailHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            if (hostObj == null)
            {
                strError = "new Host派生类对象失败";
                return -1;
            }

            // 为Host派生类设置参数
            hostObj.DetailForm = this;
            hostObj.Assembly = assembly;

            HostEventArgs e = new HostEventArgs();

            /*
            nRet = this.Flush(out strError);
            if (nRet == -1)
                return -1;
             * */


            hostObj.Main(null, e);

            /*
            nRet = this.Flush(out strError);
            if (nRet == -1)
                return -1;
             * */

            return 0;
        }

#endif


        #region 最新的创建数据脚本功能

        Assembly m_autogenDataAssembly = null;
        string m_strAutogenDataCfgFilename = "";    // 自动创建数据的.cs文件路径，全路径，包括库名部分
        object m_autogenSender = null;
        MarcDetailHost m_detailHostObj = null;
        GenerateDataForm m_genDataViewer = null;

        // 是否为新的风格
        bool AutoGenNewStyle
        {
            get
            {
                if (this.m_detailHostObj == null)
                    return false;

                if (this.m_detailHostObj.GetType().GetMethod("CreateMenu") != null)
                    return true;
                return false;
            }
        }

        int m_nDisableInitialAssembly = 0;

        // 初始化 dp2catalog_marc_autogen.cs 的 Assembly，并new MarcDetailHost对象
        // return:
        //      -2  清除了Assembly
        //      -1  error
        //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly (可能本来就是空)
        //      1   重新(或者首次)初始化了Assembly
        public int InitialAutogenAssembly(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (m_nDisableInitialAssembly > 0)
                return 0;

            bool bAssemblyReloaded = false;

            string strAutogenDataCfgFilename = "";
            string strAutogenDataCfgRefFilename = "";
            string strProtocol = "";
            dp2SearchForm dp2_searchform = null;

            if (String.IsNullOrEmpty(this.SavePath) == true)
            {
                if (this.CurrentRecord == null)
                {
                    this.m_autogenDataAssembly = null;
                    this.m_detailHostObj = null;
                    strError = "SavePath和CurrentRecord都为空，Assembly被清除";
                    return -2;
                }

                string strCfgFileName = "dp2catalog_marc_autogen.cs";

                string strMarcSyntaxOID = "";

                strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                {
                    strError = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                    goto ERROR1;
                }

                strAutogenDataCfgFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;
                strAutogenDataCfgRefFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName + ".ref";

                strProtocol = "localfile";
                goto BEGIN;
            }

            string strPath = "";
            nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // dtlp协议的自动创建数据
            if (strProtocol.ToLower() == "dtlp")
            {
                // TODO: 建立数据库名映射到MarcSyntac的机制

                /*
                strError = "暂不支持来自DTLP协议的数据自动创建功能";
                goto ERROR1;
                 * */

                if (this.CurrentRecord == null)
                {
                    this.m_autogenDataAssembly = null;
                    this.m_detailHostObj = null;
                    strError = "CurrentRecord为空，Assembly被清除";
                    return -2;
                }

                string strCfgFileName = "dp2catalog_marc_autogen.cs";

                string strMarcSyntaxOID = "";

                strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                {
                    strError = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                    goto ERROR1;
                }


                strAutogenDataCfgFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;
                strAutogenDataCfgRefFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName + ".ref";

                strProtocol = "localfile";
                goto BEGIN;
            }
            // dp2library协议的自动创建数据
            else if (strProtocol.ToLower() == "dp2library")
            {
                dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                    goto ERROR1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strCfgFileName = "dp2catalog_marc_autogen.cs";

                if (string.Compare(strServerName, "mem", true) == 0
                || string.Compare(strServerName, "file", true) == 0)
                {
                    string strMarcSyntaxOID = "";

                    strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                    if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                    {
                        strError = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                        goto ERROR1;
                    }

                    strAutogenDataCfgFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;
                    strAutogenDataCfgRefFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName + ".ref";

                    strProtocol = "localfile";
                    goto BEGIN;
                }

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                strAutogenDataCfgFilename = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;
                strAutogenDataCfgRefFilename = strBiblioDbName + "/cfgs/" + strCfgFileName + ".ref@" + strServerName;
            }
            // amazon 协议的自动创建数据
            else if (strProtocol.ToLower() == "amazon")
            {
                if (this.CurrentRecord == null)
                {
                    this.m_autogenDataAssembly = null;
                    this.m_detailHostObj = null;
                    strError = "CurrentRecord为空，Assembly被清除";
                    return -2;
                }

                string strCfgFileName = "dp2catalog_marc_autogen.cs";

                string strMarcSyntaxOID = "";

                strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                {
                    strError = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                    goto ERROR1;
                }


                strAutogenDataCfgFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;
                strAutogenDataCfgRefFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName + ".ref";

                strProtocol = "localfile";
                goto BEGIN;
            }
            else if (strProtocol.ToLower() == "sru")
            {
                if (this.CurrentRecord == null)
                {
                    this.m_autogenDataAssembly = null;
                    this.m_detailHostObj = null;
                    strError = "CurrentRecord为空，Assembly被清除";
                    return -2;
                }

                string strCfgFileName = "dp2catalog_marc_autogen.cs";

                string strMarcSyntaxOID = "";

                strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
                if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
                {
                    strError = "因为: " + strError + "，无法获得配置文件 '" + strCfgFileName + "'";
                    goto ERROR1;
                }


                strAutogenDataCfgFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName;
                strAutogenDataCfgRefFilename = this.MainForm.DataDir + "\\" + strMarcSyntaxOID.Replace(".", "_") + "\\" + strCfgFileName + ".ref";

                strProtocol = "localfile";
                goto BEGIN;
            }
            else
            {
                strError = "暂不支持来自 '" + strProtocol + "'  协议的数据自动创建功能";
                goto ERROR1;
            }

            BEGIN:
            // 如果必要，重新准备Assembly
            if (m_autogenDataAssembly == null
                || m_strAutogenDataCfgFilename != strAutogenDataCfgFilename)
            {
                this.m_autogenDataAssembly = this.MainForm.AssemblyCache.FindObject(strAutogenDataCfgFilename);

                if (this.m_detailHostObj != null)
                {
                    this.m_detailHostObj.Dispose();
                    this.m_detailHostObj = null;
                }

                // 如果Cache中没有现成的Assembly
                if (this.m_autogenDataAssembly == null)
                {
                    string strCode = "";
                    string strRef = "";

                    byte[] baCfgOutputTimestamp = null;

                    if (strProtocol.ToLower() == "dp2library")
                    {
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   found
                        nRet = dp2_searchform.GetCfgFile(strAutogenDataCfgFilename,
                            out strCode,
                            out baCfgOutputTimestamp,
                            out strError);
                        if (nRet == -1 || nRet == 0)
                            goto ERROR1;
                        nRet = dp2_searchform.GetCfgFile(strAutogenDataCfgRefFilename,
out strRef,
out baCfgOutputTimestamp,
out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else if (strProtocol.ToLower() == "localfile")
                    {
                        if (File.Exists(strAutogenDataCfgFilename) == false)
                        {
                            /*
                            if (bOnlyFillMenu == true)
                                return;
                             * */
                            strError = "配置文件 '" + strAutogenDataCfgFilename + "' 不存在...";
                            goto ERROR1;
                        }
                        if (File.Exists(strAutogenDataCfgRefFilename) == false)
                        {

                            strError = "配置文件 '" + strAutogenDataCfgRefFilename + "' 不存在(但是其配套的.cs文件已经存在)...";
                            goto ERROR1;
                        }
                        try
                        {
                            Encoding encoding = FileUtil.DetectTextFileEncoding(strAutogenDataCfgFilename);
                            using (StreamReader sr = new StreamReader(strAutogenDataCfgFilename, encoding))
                            {
                                strCode = sr.ReadToEnd();
                            }
                            encoding = FileUtil.DetectTextFileEncoding(strAutogenDataCfgRefFilename);
                            using (StreamReader sr = new StreamReader(strAutogenDataCfgRefFilename, encoding))
                            {
                                strRef = sr.ReadToEnd();
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = ExceptionUtil.GetAutoText(ex);
                            goto ERROR1;
                        }
                    }

                    try
                    {
                        // 准备Assembly
                        Assembly assembly = null;
                        nRet = GetCsScriptAssembly(
                            strCode,
                            strRef,
                            out assembly,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "编译脚本文件 '" + strAutogenDataCfgFilename + "' 时出错：" + strError;
                            goto ERROR1;
                        }
                        // 记忆到缓存
                        this.MainForm.AssemblyCache.SetObject(strAutogenDataCfgFilename, assembly);

                        this.m_autogenDataAssembly = assembly;

                        bAssemblyReloaded = true;
                    }
                    catch (Exception ex)
                    {
                        strError = "准备脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                        goto ERROR1;
                    }
                }

                bAssemblyReloaded = true;

                m_strAutogenDataCfgFilename = strAutogenDataCfgFilename;

                // 至此，Assembly已经纯备好了
                Debug.Assert(this.m_autogenDataAssembly != null, "");
            }

            Debug.Assert(this.m_autogenDataAssembly != null, "");

            // 准备 host 对象
            if (this.m_detailHostObj == null
                || bAssemblyReloaded == true)
            {
                try
                {
                    MarcDetailHost host = null;
                    nRet = NewHostObject(
                        out host,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "执行脚本文件 '" + m_strAutogenDataCfgFilename + "' 时出错：" + strError;
                        goto ERROR1;
                    }
                    if (this.m_detailHostObj != null)
                    {
                        this.m_detailHostObj.Dispose();
                        this.m_detailHostObj = null;
                    }
                    this.m_detailHostObj = host;
                }
                catch (Exception ex)
                {
                    strError = "准备脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
            }

            Debug.Assert(this.m_detailHostObj != null, "");

            if (bAssemblyReloaded == true)
                return 1;
            return 0;
            ERROR1:
            return -1;
        }

        /*
发生未捕获的界面线程异常: 
Type: System.ObjectDisposedException
Message: 无法访问已释放的对象。
对象名:“MarcDetailForm”。
Stack:
在 System.Windows.Forms.Control.CreateHandle()
在 System.Windows.Forms.Form.CreateHandle()
在 System.Windows.Forms.Control.get_Handle()
在 System.Windows.Forms.Control.GetSafeHandle(IWin32Window window)
在 System.Windows.Forms.MessageBox.ShowCore(IWin32Window owner, String text, String caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, Boolean showHelp)
在 System.Windows.Forms.MessageBox.Show(IWin32Window owner, String text)
在 dp2Catalog.MarcDetailForm.AutoGenerate(Object sender, GenerateDataEventArgs e, Boolean bOnlyFillMenu)
在 dp2Catalog.MarcDetailForm.MarcEditor_GenerateData(Object sender, GenerateDataEventArgs e)
在 DigitalPlatform.Marc.MarcEditor.OnGenerateData(GenerateDataEventArgs e)
在 DigitalPlatform.Marc.MyEdit.ProcessDialogKey(Keys keyData)
在 System.Windows.Forms.Control.PreProcessMessage(Message& msg)
在 System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message& msg)
在 System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG& msg)
         * */
        // 自动加工数据
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm
        public void AutoGenerate(object sender,
            GenerateDataEventArgs e,
            bool bOnlyFillMenu = false)
        {
            int nRet = 0;

            string strError = "";

            this._processing++;
            try
            {
                bool bAssemblyReloaded = false;

                // 初始化 dp2catalog_marc_autogen.cs 的 Assembly，并new MarcDetailHost对象
                // return:
                //      -2  清除了Assembly
                //      -1  error
                //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                //      1   重新(或者首次)初始化了Assembly
                nRet = InitialAutogenAssembly(out strError);
                if (nRet == -2)
                {
                    if (bOnlyFillMenu == true)
                        return;
                }
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    if (this.m_detailHostObj == null)
                        return; // 库名不具备，无法初始化
                }
                if (nRet == 1)
                    bAssemblyReloaded = true;

                Debug.Assert(this.m_detailHostObj != null, "");

                if (this.AutoGenNewStyle == true)
                {
                    DisplayAutoGenMenuWindow(this.MainForm.PanelFixedVisible == false ? true : false);
                    if (bOnlyFillMenu == false)
                    {
                        if (this.MainForm.PanelFixedVisible == true)
                            MainForm.ActivateGenerateDataPage();
                    }

                    if (this.m_genDataViewer != null)
                    {
                        this.m_genDataViewer.sender = sender;
                        this.m_genDataViewer.e = e;
                    }

                    // 清除残留菜单事项
                    if (m_autogenSender != sender
                        || bAssemblyReloaded == true)
                    {
                        if (this.m_genDataViewer != null
                            && this.m_genDataViewer.Count > 0)
                            this.m_genDataViewer.Clear();
                    }
                }
                else // 旧的风格
                {
                    if (this.m_genDataViewer != null)
                    {
                        this.m_genDataViewer.Close();
                        this.m_genDataViewer = null;
                    }

                    if (this.Focused == true || this.MarcEditor.Focused)
                        this.MainForm.CurrentGenerateDataControl = null;

                    // 如果意图仅仅为填充菜单
                    if (bOnlyFillMenu == true)
                        return;
                }

                try
                {
                    // 旧的风格
                    if (this.AutoGenNewStyle == false)
                    {
                        this.m_detailHostObj.Invoke(String.IsNullOrEmpty(e.ScriptEntry) == true ? "Main" : e.ScriptEntry,
    sender,
    e);
                        // this.SetSaveAllButtonState(true);
                        return;
                    }

                    // 初始化菜单
                    try
                    {
                        if (this.m_genDataViewer != null)
                        {
                            if (this.m_genDataViewer.Count == 0)
                            {
                                dynamic o = this.m_detailHostObj;
                                o.CreateMenu(sender, e);

                                this.m_genDataViewer.Actions = this.m_detailHostObj.ScriptActions;
                            }

                            // 根据当前插入符位置刷新加亮事项
                            this.m_genDataViewer.RefreshState();
                        }

                        if (String.IsNullOrEmpty(e.ScriptEntry) == false)
                        {
                            this.m_detailHostObj.Invoke(e.ScriptEntry,
                                sender,
                                e);
                        }
                        else
                        {
                            if (this.MainForm.PanelFixedVisible == true
                                && bOnlyFillMenu == false
                                && this.MainForm.CurrentGenerateDataControl != null)
                            {
                                TableLayoutPanel table = (TableLayoutPanel)this.MainForm.CurrentGenerateDataControl;
                                for (int i = 0; i < table.Controls.Count; i++)
                                {
                                    Control control = table.Controls[i];
                                    if (control is DpTable)
                                    {
                                        control.Focus();
                                        break;
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        /*
                        // 被迫改用旧的风格
                        this.m_detailHostObj.Invoke(String.IsNullOrEmpty(e.ScriptEntry) == true ? "Main" : e.ScriptEntry,
        sender,
        e);
                        this.SetSaveAllButtonState(true);
                        return;
                         * */
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    strError = "执行脚本文件 '" + m_strAutogenDataCfgFilename + "' 过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }

                this.m_autogenSender = sender;  // 记忆最近一次的调用发起者

                if (bOnlyFillMenu == false
                    && this.m_genDataViewer != null)
                    this.m_genDataViewer.TryAutoRun();

                return;
            }
            finally
            {
                this._processing--;
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void DisplayAutoGenMenuWindow(bool bOpenWindow)
        {
            string strError = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_genDataViewer == null || m_genDataViewer.Visible == false))
                    return;
            }


            if (this.m_genDataViewer == null
                || (bOpenWindow == true && this.m_genDataViewer.Visible == false))
            {
                m_genDataViewer = new GenerateDataForm();

                m_genDataViewer.AutoRun = this.MainForm.AppInfo.GetBoolean("detailform", "gen_auto_run", false);
                // GuiUtil.SetControlFont(m_genDataViewer, this.Font, false);

                {	// 恢复列宽度
                    string strWidths = this.MainForm.AppInfo.GetString(
                                   "gen_data_dlg",
                                    "column_width",
                                   "");
                    if (String.IsNullOrEmpty(strWidths) == false)
                    {
                        DpTable.SetColumnHeaderWidth(m_genDataViewer.ActionTable,
                            strWidths,
                            true);
                    }
                }

                // m_genDataViewer.MainForm = this.MainForm;  // 必须是第一句
                m_genDataViewer.Text = "创建数据";

                m_genDataViewer.DoDockEvent -= new DoDockEventHandler(m_genDataViewer_DoDockEvent);
                m_genDataViewer.DoDockEvent += new DoDockEventHandler(m_genDataViewer_DoDockEvent);

                m_genDataViewer.SetMenu -= new RefreshMenuEventHandler(m_genDataViewer_SetMenu);
                m_genDataViewer.SetMenu += new RefreshMenuEventHandler(m_genDataViewer_SetMenu);

                m_genDataViewer.TriggerAction -= new TriggerActionEventHandler(m_genDataViewer_TriggerAction);
                m_genDataViewer.TriggerAction += new TriggerActionEventHandler(m_genDataViewer_TriggerAction);

                m_genDataViewer.MyFormClosed -= new EventHandler(m_genDataViewer_MyFormClosed);
                m_genDataViewer.MyFormClosed += new EventHandler(m_genDataViewer_MyFormClosed);

                m_genDataViewer.FormClosed -= new FormClosedEventHandler(m_genDataViewer_FormClosed);
                m_genDataViewer.FormClosed += new FormClosedEventHandler(m_genDataViewer_FormClosed);

            }


            if (bOpenWindow == true)
            {
                if (m_genDataViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_genDataViewer, "autogen_viewer_state");
                    m_genDataViewer.Show(this);
                    m_genDataViewer.Activate();

                    this.MainForm.CurrentGenerateDataControl = null;
                }
                else
                {
                    if (m_genDataViewer.WindowState == FormWindowState.Minimized)
                        m_genDataViewer.WindowState = FormWindowState.Normal;
                    m_genDataViewer.Activate();
                }
            }
            else
            {
                if (m_genDataViewer.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentGenerateDataControl != m_genDataViewer.Table)
                        m_genDataViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }

            if (this.m_genDataViewer != null)
                this.m_genDataViewer.CloseWhenComplete = bOpenWindow;

            return;
            ERROR1:
            MessageBox.Show(this, "DisplayAutoGenMenu() 出错: " + strError);
        }

        void m_genDataViewer_DoDockEvent(object sender, DoDockEventArgs e)
        {
            if (this.MainForm.CurrentGenerateDataControl != m_genDataViewer.Table)
                this.MainForm.CurrentGenerateDataControl = m_genDataViewer.Table;

            if (e.ShowFixedPanel == true
                && this.MainForm.PanelFixedVisible == false)
                this.MainForm.PanelFixedVisible = true;

            /*
            this.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", m_genDataViewer.AutoRun);

            {	// 保存列宽度
                string strWidths = DpTable.GetColumnWidthListString(m_genDataViewer.ActionTable);
                this.MainForm.AppInfo.SetString(
                    "gen_data_dlg",
                    "column_width",
                    strWidths);
            }
             * */

            m_genDataViewer.Docked = true;
            m_genDataViewer.Visible = false;
        }

        void m_genDataViewer_SetMenu(object sender, RefreshMenuEventArgs e)
        {
            if (e.Actions == null || this.m_detailHostObj == null)
                return;

            Type classType = m_detailHostObj.GetType();

            foreach (ScriptAction action in e.Actions)
            {
                string strFuncName = action.ScriptEntry + "_setMenu";
                if (string.IsNullOrEmpty(strFuncName) == true)
                    continue;

                DigitalPlatform.Script.SetMenuEventArgs e1 = new DigitalPlatform.Script.SetMenuEventArgs();
                e1.Action = action;
                e1.sender = e.sender;
                e1.e = e.e;

                classType = m_detailHostObj.GetType();
                while (classType != null)
                {
                    try
                    {
                        // 有两个参数的成员函数
                        classType.InvokeMember(strFuncName,
                            BindingFlags.DeclaredOnly |
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod
                            ,
                            null,
                            this.m_detailHostObj,
                            new object[] { sender, e1 });
                        break;
                    }
                    catch (System.MissingMethodException/*ex*/)
                    {
                        classType = classType.BaseType;
                        if (classType == null)
                            break;
                    }
                }
            }
        }

        void m_genDataViewer_TriggerAction(object sender, TriggerActionArgs e)
        {
            string strError = "";
            if (this.m_detailHostObj != null)
            {
                if (this.IsDisposed == true)
                {
                    if (this.m_genDataViewer != null)
                    {
                        this.m_genDataViewer.Clear();
                        this.m_genDataViewer.Close();
                        this.m_genDataViewer = null;
                        return;
                    }
                }
                if (String.IsNullOrEmpty(e.EntryName) == false)
                {
                    try
                    {
                        this.m_detailHostObj.Invoke(e.EntryName,
                            e.sender,
                            e.e);
                    }
                    catch (Exception ex)
                    {
                        // 2015/8/24
                        strError = "MARC记录窗的记录 '" + this.SavePath + "' 在执行创建数据脚本的时候出现异常: " + ExceptionUtil.GetDebugText(ex)
                            + "\r\n\r\n建议检查此书目记录相关的 dp2catalog_marc_autogen.cs 配置文件，试着刷新相关书目库定义，或者与数字平台的工程师取得联系";
                        goto ERROR1;
                    }
                }

                if (this.m_genDataViewer != null)
                    this.m_genDataViewer.RefreshState();
            }
            return;
            ERROR1:
            // MessageBox.Show(this, strError);
            {
                bool bSendReport = true;
                DialogResult result = MessageDlg.Show(this,
        "dp2Catalog 发生异常:\r\n\r\n" + strError,
        "dp2Catalog 发生异常",
        MessageBoxButtons.OK,
        MessageBoxDefaultButton.Button1,
        ref bSendReport,
        new string[] { "确定" },
        "将信息发送给开发者");
                // 发送异常报告
                if (bSendReport)
                    Program.CrashReport(strError);
            }
        }

        void m_genDataViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_genDataViewer != null)
            {
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                {
                    this.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", m_genDataViewer.AutoRun);

                    {	// 保存列宽度
                        string strWidths = DpTable.GetColumnWidthListString(m_genDataViewer.ActionTable);
                        this.MainForm.AppInfo.SetString(
                            "gen_data_dlg",
                            "column_width",
                            strWidths);
                    }

                    this.MainForm.AppInfo.UnlinkFormState(m_genDataViewer);
                }
                this.m_genDataViewer = null;
            }
        }

        void m_genDataViewer_MyFormClosed(object sender, EventArgs e)
        {
            if (m_genDataViewer != null)
            {
                this.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", m_genDataViewer.AutoRun);

                {	// 保存列宽度
                    string strWidths = DpTable.GetColumnWidthListString(m_genDataViewer.ActionTable);
                    this.MainForm.AppInfo.SetString(
                        "gen_data_dlg",
                        "column_width",
                        strWidths);
                }

                this.MainForm.AppInfo.UnlinkFormState(m_genDataViewer);
                this.m_genDataViewer = null;
            }
        }

        int NewHostObject(
            out MarcDetailHost hostObj,
            out string strError)
        {
            strError = "";
            hostObj = null;

            Type entryClassType = ScriptManager.GetDerivedClassType(
    this.m_autogenDataAssembly,
    "dp2Catalog.MarcDetailHost");
            if (entryClassType == null)
            {
                strError = "dp2Catalog.MarcDetailHost的派生类都没有找到";
                return -1;
            }

            // new一个MarcDetailHost派生对象
            hostObj = (MarcDetailHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            if (hostObj == null)
            {
                strError = "new MarcDetailHost的派生类对象时失败";
                return -1;
            }

            // 为DetailHost派生类设置参数
            hostObj.DetailForm = this;
            hostObj.Assembly = this.m_autogenDataAssembly;
            return 0;
        }

        int GetCsScriptAssembly(
    string strCode,
    string strRef,
            out Assembly assembly,
    out string strError)
        {
            strError = "";
            assembly = null;

            int nRet;

            // 2018/8/26
            // 为了兼容以前代码，对 using 部分进行修改
            strCode = ScriptManager.ModifyCode(strCode);

            nRet = ScriptManager.GetRefsFromXml(strRef,
                out string[] saRef,
                out strError);
            if (nRet == -1)
                return -1;

            ScriptManager.RemoveRefsBinDirMacro(ref saRef);

            string[] saAddRef = {
                                    // 2011/3/4 增加
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.script.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.xmleditor.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    //Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2catalog.exe"
                                };

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            string strErrorInfo = "";
            string strWarningInfo = "";
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                null,   // strLibPaths,
                out assembly,
                out strErrorInfo,
                out strWarningInfo);
            if (nRet == -1)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strErrorInfo;
                return -1;
            }

            return 0;
        }


        #endregion


        // 获得出版社相关信息
        public int GetPublisherInfo(
            string strPublisherNumber,
            out string str210,
            out string strError)
        {
            strError = "";
            str210 = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用GetPublisherInfo()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.GetPublisherInfo(
                    strServerName,
                    strPublisherNumber,
                    out str210,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 设置出版社相关信息
        public int SetPublisherInfo(
            string strPublisherNumber,
            string str210,
            out string strError)
        {
            strError = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用SetPublisherInfo()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.SetPublisherInfo(
                    strServerName,
                    strPublisherNumber,
                    str210,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 获得102相关信息
        public int Get102Info(
            string strPublisherNumber,
            out string str102,
            out string strError)
        {
            strError = "";
            str102 = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用Get102Info()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.Get102Info(
                    strServerName,
                    strPublisherNumber,
                    out str102,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        // 设置102相关信息
        public int Set102Info(
            string strPublisherNumber,
            string str102,
            out string strError)
        {
            strError = "";

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(this.SavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                return -1;

            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法继续调用Set102Info()";
                    return -1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                return dp2_searchform.Set102Info(
                    strServerName,
                    strPublisherNumber,
                    str102,
                    out strError);
            }

            strError = "无法识别的协议名 '" + strProtocol + "'";
            return -1;
        }

        public int LoadTemplate()
        {
            int nRet = 0;

            if (this.BiblioChanged == true
                || this.ObjectChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                    "MarcDetailForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return 0;
            }

            // 跟据不同的协议，调用不同的装载模板功能
            string strProtocol = "";
            string strPath = "";
            string strError = "";

            if (String.IsNullOrEmpty(this.SavePath) == false)
            {
                // 分离出各个部分
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + this.SavePath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }
            }
            else
            {
                // 选择协议
                SelectProtocolDialog protocol_dlg = new SelectProtocolDialog();
                GuiUtil.SetControlFont(protocol_dlg, this.Font);

                protocol_dlg.Protocols = new List<string>();
                protocol_dlg.Protocols.Add("dp2library");
                protocol_dlg.Protocols.Add("dtlp");
                protocol_dlg.StartPosition = FormStartPosition.CenterScreen;

                protocol_dlg.ShowDialog(this);

                if (protocol_dlg.DialogResult != DialogResult.OK)
                    return 0;

                strProtocol = protocol_dlg.SelectedProtocol;
            }


            if (strProtocol == "dp2library")
            {
                return LoadDp2libraryTemplate(strPath);
            }
            else if (strProtocol == "dtlp")
            {
                return LoadDtlpTemplate(strPath);
            }
            else
            {
                return LoadDp2libraryTemplate("");
            }

            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // DTLP协议下 装载模板
        // parameters:
        //      strPath DTLP协议内路径。例如 localhost/中文图书/ctlno/0000001
        public int LoadDtlpTemplate(string strPath)
        {
            int nRet = 0;
            string strError = "";

            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            if (dtlp_searchform == null)
            {
                strError = "没有连接的或者打开的DTLP检索窗，无法装载模板";
                goto ERROR1;
            }


            string strServerAddr = "";
            string strDbName = "";
            string strNumber = "";

            // 解析保存路径
            // return:
            //      -1  出错
            //      0   成功
            nRet = DtlpChannel.ParseWritePath(strPath,
                out strServerAddr,
                out strDbName,
                out strNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strStartPath = "";

            if (String.IsNullOrEmpty(strServerAddr) == false
                && String.IsNullOrEmpty(strDbName) == false)
                strStartPath = strServerAddr + "/" + strDbName;
            else if (String.IsNullOrEmpty(strServerAddr) == false)
                strStartPath = strServerAddr;

            GetDtlpResDialog dlg = new GetDtlpResDialog();
            GuiUtil.SetControlFont(dlg, this.Font);


            dlg.Text = "请选择目标数据库";
            dlg.Initial(dtlp_searchform.DtlpChannels,
                dtlp_searchform.DtlpChannel);
            dlg.EnabledIndices = new int[] { DtlpChannel.TypeStdbase };
            dlg.Path = strStartPath;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            // 获得default.cfg配置文件
            string strCfgPath = dlg.Path + "/cfgs/default.cfg";
            string strContent = "";

            Cursor.Current = Cursors.WaitCursor;
            nRet = dtlp_searchform.DtlpChannel.GetCfgFile(strCfgPath,
                out strContent,
                out strError);
            Cursor.Current = Cursors.Default;

            if (nRet == -1)
                goto ERROR1;

            // 选择模板
            SelectRecordTemplateDialog tempdlg = new SelectRecordTemplateDialog();
            GuiUtil.SetControlFont(tempdlg, this.Font);

            tempdlg.Content = strContent;
            tempdlg.StartPosition = FormStartPosition.CenterScreen;

            tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return 0;

            this.SavePath = "DTLP" + ":" + dlg.Path + "/ctlno/?";

            // 自动识别MARC格式
            string strOutMarcSyntax = "";
            // 探测记录的MARC格式 unimarc / usmarc / reader
            nRet = MarcUtil.DetectMarcSyntax(tempdlg.SelectedRecordMarc,
                out strOutMarcSyntax);
            if (strOutMarcSyntax == "")
                strOutMarcSyntax = "unimarc";

            if (strOutMarcSyntax == "unimarc" || strOutMarcSyntax == "")
                this.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.1";
            else if (strOutMarcSyntax == "usmarc")
                this.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.10";
            else if (strOutMarcSyntax == "dt1000reader")
                this.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.dt1000reader";
            else
            {
                /*
                strError = "未知的MARC syntax '" + strOutMarcSyntax + "'";
                goto ERROR1;
                 * */
                // TODO: 可以出现菜单选择
            }

            this.MarcEditor.ClearMarcDefDom();
            this.MarcEditor.Marc = tempdlg.SelectedRecordMarc;
            this.CurrentTimestamp = null;

            this.ObjectChanged = false;
            this.BiblioChanged = false;

            DisplayHtml(tempdlg.SelectedRecordMarc, this.AutoDetectedMarcSyntaxOID);

            this.LinkedSearchForm = null;  // 切断和原来关联的检索窗的联系。这样就没法前后翻页了
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 将形态为 “本地服务器/中文图书”这样的路径转换为“中文图书@本地服务器”
        static string CanonicalizePath(string strPath)
        {
            string[] parts = strPath.Split(new char[] { '/' });
            if (parts.Length < 2)
                return "";

            return parts[1] + "@" + parts[0];
        }

        // dp2library协议下 装载模板
        // parameters:
        //      strPath dp2library协议内路径。例如 中文图书/1@本地服务器
        public int LoadDp2libraryTemplate(string strPath)
        {
            try
            {
                string strError = "";
                int nRet = 0;

                // 按住 Shift 使用本功能，可重新出现对话框
                bool bShift = (Control.ModifierKeys == Keys.Shift);

                /*
                if (this.BiblioChanged == true
                    || this.ObjectChanged == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装载新内容? ",
                        "MarcDetailForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return 0;
                }*/


                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法装载模板";
                    goto ERROR1;
                }

                string strSelectedDbName = this.MainForm.AppInfo.GetString(
         "entity_form",
         "selected_dbname_for_loadtemplate",
         "");
                SelectedTemplate selected = this.selected_templates.Find(strSelectedDbName);


                string strServerName = "";
                string strLocalPath = "";

                string strBiblioDbName = "";

                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(string.IsNullOrEmpty(strSelectedDbName) == false ? strSelectedDbName : strPath,
                    out strServerName,
                    out strLocalPath);

                strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);


                /*
                if (this.LinkedSearchForm != null
                    && strProtocol != this.LinkedSearchForm.CurrentProtocol)
                {
                    strError = "检索窗的协议已经发生改变";
                    goto ERROR1;
                }*/

                string strStartPath = "";

                if (String.IsNullOrEmpty(strServerName) == false
                    && String.IsNullOrEmpty(strBiblioDbName) == false)
                    strStartPath = strServerName + "/" + strBiblioDbName;
                else if (String.IsNullOrEmpty(strServerName) == false)
                    strStartPath = strServerName;

                GetDp2ResDlg dbname_dlg = new GetDp2ResDlg();
                GuiUtil.SetControlFont(dbname_dlg, this.Font);
                if (selected != null)
                {
                    dbname_dlg.NotAsk = selected.NotAskDbName;
                    dbname_dlg.AutoClose = (bShift == true ? false : selected.NotAskDbName);
                }
                dbname_dlg.EnableNotAsk = true;

                dbname_dlg.Text = "装载书目模板 -- 请选择目标数据库";
#if OLD_CHANNEL
                dbname_dlg.dp2Channels = dp2_searchform.Channels;
#endif
                dbname_dlg.ChannelManager = Program.MainForm;

                dbname_dlg.Servers = this.MainForm.Servers;
                dbname_dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_DB };
                dbname_dlg.Path = strStartPath;

                if (this.IsValid() == false)
                    return -1;
                dbname_dlg.ShowDialog(this);    ////


                if (dbname_dlg.DialogResult != DialogResult.OK)
                    return 0;

                // 记忆
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "selected_dbname_for_loadtemplate",
                    CanonicalizePath(dbname_dlg.Path));

                selected = this.selected_templates.Find(CanonicalizePath(dbname_dlg.Path));   // 

                // 将目标路径拆分为两个部分
                nRet = dbname_dlg.Path.IndexOf("/");
                if (nRet == -1)
                {
                    Debug.Assert(false, "");
                    strServerName = dbname_dlg.Path;
                    strBiblioDbName = "";
                    strError = "所选择目标(数据库)路径 '" + dbname_dlg.Path + "' 格式不正确";
                    goto ERROR1;
                }
                else
                {
                    strServerName = dbname_dlg.Path.Substring(0, nRet);
                    strBiblioDbName = dbname_dlg.Path.Substring(nRet + 1);

                    // 检查所选数据库的syntax，必须为marc

                    string strSyntax = "";
                    // 获得一个数据库的数据syntax
                    // parameters:
                    //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                    //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = dp2_searchform.GetDbSyntax(
                        null,
                        strServerName,
                        strBiblioDbName,
                        out strSyntax,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取书目库 '" + strBiblioDbName + "的数据格式时发生错误: " + strError;
                        goto ERROR1;
                    }

                    if (strSyntax != "unimarc"
                        && strSyntax != "usmarc")
                    {
                        strError = "所选书目库 '" + strBiblioDbName + "' 不是MARC格式的数据库";
                        goto ERROR1;
                    }
                }


                // 然后获得cfgs/template配置文件
                string strCfgFilePath = strBiblioDbName + "/cfgs/template" + "@" + strServerName;

                string strCode = "";
                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgFilePath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                SelectRecordTemplateDlg temp_dlg = new SelectRecordTemplateDlg();
                GuiUtil.SetControlFont(temp_dlg, this.Font);

                temp_dlg.Text = "请选择新记录模板 -- " + dbname_dlg.Path;

                string strSelectedTemplateName = "";
                bool bNotAskTemplateName = false;
                if (selected != null)
                {
                    strSelectedTemplateName = selected.TemplateName;
                    bNotAskTemplateName = selected.NotAskTemplateName;
                }

                temp_dlg.SelectedName = strSelectedTemplateName;
                temp_dlg.AutoClose = (bShift == true ? false : bNotAskTemplateName);
                temp_dlg.NotAsk = bNotAskTemplateName;
                temp_dlg.EnableNotAsk = true;    // 2015/5/11

                nRet = temp_dlg.Initial(
                    false,  // true 表示也允许删除
                    strCode,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载配置文件 '" + strCfgFilePath + "' 发生错误: " + strError;
                    goto ERROR1;
                }

                temp_dlg.ap = this.MainForm.AppInfo;
                temp_dlg.ApCfgTitle = "marcdetailform_selecttemplatedlg";
                if (this.IsValid() == false)
                    return -1;
                temp_dlg.ShowDialog(this);  ////


                if (temp_dlg.DialogResult != DialogResult.OK)
                    return 0;

                // 记忆本次的选择，下次就不用再进入本对话框了
                this.selected_templates.Set(CanonicalizePath(dbname_dlg.Path),
                    dbname_dlg.NotAsk,
                    temp_dlg.SelectedName,
                    temp_dlg.NotAsk);

                string strMarcSyntax = "";
                string strOutMarcSyntax = "";
                string strRecord = "";

                // 从数据记录中获得MARC格式
                nRet = MarcUtil.Xml2Marc(temp_dlg.SelectedRecordXml,
                    true,
                    strMarcSyntax,
                    out strOutMarcSyntax,
                    out strRecord,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    goto ERROR1;
                }
                this.SavePath = "dp2library" + ":" + strBiblioDbName + "/?" + "@" + strServerName;

                if (this.IsValid() == false)
                    return -1;

                this.MarcEditor.ClearMarcDefDom();
                this.MarcEditor.Marc = strRecord;   ////
                this.CurrentTimestamp = baCfgOutputTimestamp;

                this.ObjectChanged = false;
                this.BiblioChanged = false;

                DisplayHtml(strRecord, GetSyntaxOID(strOutMarcSyntax));

                // 2020/2/17
                // 设置 MarcSyntax OID，以便让保存到文件功能可以感知到 MARC 格式类型
                this.m_currentRecord = new DigitalPlatform.OldZ3950.Record();
                this.m_currentRecord.m_strSyntaxOID = GetSyntaxOID(strOutMarcSyntax);

                this.LinkedSearchForm = null;  // 切断和原来关联的检索窗的联系。这样就没法前后翻页了
                return 0;
                ERROR1:
                MessageBox.Show(this, strError);
                return -1;
            }
            catch (System.ObjectDisposedException)
            {
                return -1;
            }
        }

        // 保存到模板
        public int SaveToTemplate()
        {
            string strError = "";
            int nRet = 0;


            // 跟据不同的协议，调用不同的装载模板功能
            string strProtocol = "";
            string strPath = "";

            if (String.IsNullOrEmpty(this.SavePath) == false)
            {
                // 分离出各个部分
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "解析路径 '" + this.SavePath + "' 字符串过程中发生错误: " + strError;
                    goto ERROR1;
                }
            }
            else
            {
                strProtocol = "dp2library";
            }


            if (strProtocol == "dp2library")
            {
                return SaveToDp2libraryTemplate(strPath);
            }
            else if (strProtocol == "dtlp")
            {
                return SaveToDtlpTemplate(strPath);
            }
            else
            {
                return SaveToDp2libraryTemplate("");
            }

            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 保存到 DTLP协议 模板
        // parameters:
        //      strPath DTLP协议内路径。例如 localhost/中文图书/ctlno/0000001
        public int SaveToDtlpTemplate(string strPath)
        {
            int nRet = 0;
            string strError = "";

            DtlpSearchForm dtlp_searchform = this.GetDtlpSearchForm();

            if (dtlp_searchform == null)
            {
                strError = "没有连接的或者打开的DTLP检索窗，无法保存模板";
                goto ERROR1;
            }


            string strServerAddr = "";
            string strDbName = "";
            string strNumber = "";

            // 解析保存路径
            // return:
            //      -1  出错
            //      0   成功
            nRet = DtlpChannel.ParseWritePath(strPath,
                out strServerAddr,
                out strDbName,
                out strNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strStartPath = strServerAddr + "/" + strDbName;

            GetDtlpResDialog dlg = new GetDtlpResDialog();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请选择目标数据库";
            dlg.Initial(dtlp_searchform.DtlpChannels,
                dtlp_searchform.DtlpChannel);
            dlg.EnabledIndices = new int[] { DtlpChannel.TypeStdbase };
            dlg.Path = strStartPath;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            // 获得default.cfg配置文件
            string strCfgPath = dlg.Path + "/cfgs/default.cfg";
            string strContent = "";

            Cursor.Current = Cursors.WaitCursor;
            nRet = dtlp_searchform.DtlpChannel.GetCfgFile(strCfgPath,
                out strContent,
                out strError);
            Cursor.Current = Cursors.Default;

            if (nRet == -1)
                goto ERROR1;

            // 选择模板
            SelectRecordTemplateDialog tempdlg = new SelectRecordTemplateDialog();
            GuiUtil.SetControlFont(tempdlg, this.Font);

            tempdlg.LoadMode = false;
            tempdlg.Content = strContent;
            tempdlg.SelectedRecordMarc = this.MarcEditor.Marc;
            tempdlg.StartPosition = FormStartPosition.CenterScreen;

            tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return 0;

            if (tempdlg.Changed == false)
                return 0;

            Cursor.Current = Cursors.WaitCursor;
            nRet = dtlp_searchform.DtlpChannel.WriteCfgFile(strCfgPath,
                tempdlg.Content,
                out strError);
            Cursor.Current = Cursors.Default;

            if (nRet == -1)
                goto ERROR1;


            MessageBox.Show(this, "修改模板 '" + strCfgPath + "' 成功");
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 保存到dp2library模板
        public int SaveToDp2libraryTemplate(string strPath)
        {
            string strError = "";
            int nRet = 0;

            dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

            if (dp2_searchform == null)
            {
                strError = "没有连接的或者打开的dp2检索窗，无法保存当前内容到模板";
                goto ERROR1;
            }

            string strServerName = "";
            string strLocalPath = "";

            string strBiblioDbName = "";

            // 解析记录路径。
            // 记录路径为如下形态 "中文图书/1 @服务器"
            dp2SearchForm.ParseRecPath(strPath,
                out strServerName,
                out strLocalPath);
            strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

            string strStartPath = "";

            if (String.IsNullOrEmpty(strServerName) == false
                && String.IsNullOrEmpty(strBiblioDbName) == false)
                strStartPath = strServerName + "/" + strBiblioDbName;
            else if (String.IsNullOrEmpty(strServerName) == false)
                strStartPath = strServerName;

            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = "请选择目标数据库";
#if OLD_CHANNEL
            dlg.dp2Channels = dp2_searchform.Channels;
#endif
            dlg.ChannelManager = Program.MainForm;

            dlg.Servers = this.MainForm.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_DB };
            dlg.Path = strStartPath;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            string strSyntax = "";

            nRet = dlg.Path.IndexOf("/");
            if (nRet == -1)
            {
                strServerName = dlg.Path;
                strBiblioDbName = "";
                strError = "未选择目标数据库";
                goto ERROR1;
            }
            else
            {
                strServerName = dlg.Path.Substring(0, nRet);
                strBiblioDbName = dlg.Path.Substring(nRet + 1);

                // 检查所选数据库的syntax，必须为dc

                // 获得一个数据库的数据syntax
                // parameters:
                //      stop    如果!=null，表示使用这个stop，它已经OnStop +=
                //              如果==null，表示会自动使用this.stop，并自动OnStop+=
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetDbSyntax(
                    null,
                    strServerName,
                    strBiblioDbName,
                    out strSyntax,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取书目库 '" + strBiblioDbName + "的数据格式时发生错误: " + strError;
                    goto ERROR1;
                }

                if (strSyntax != "unimarc"
                    && strSyntax != "usmarc")
                {
                    strError = "所选书目库 '" + strBiblioDbName + "' 不是MARC格式的数据库";
                    goto ERROR1;
                }
            }


            // 然后获得cfgs/template配置文件
            string strCfgFilePath = strBiblioDbName + "/cfgs/template" + "@" + strServerName;

            string strCode = "";
            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = dp2_searchform.GetCfgFile(strCfgFilePath,
                out strCode,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            SelectRecordTemplateDlg tempdlg = new SelectRecordTemplateDlg();
            GuiUtil.SetControlFont(tempdlg, this.Font);
            nRet = tempdlg.Initial(true,
                strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            tempdlg.Text = "请选择要修改的模板记录";
            tempdlg.CheckNameExist = false;	// 按OK按钮时不警告"名字不存在",这样允许新建一个模板
            tempdlg.ap = this.MainForm.AppInfo;
            tempdlg.ApCfgTitle = "marcdetailform_selecttemplatedlg";
            tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return 0;

            // 修改配置文件内容
            if (tempdlg.textBox_name.Text != "")
            {
                string strXml = "";
                /*
                nRet = dp2SearchForm.GetBiblioXml(
                    strSyntax,
                    this.MarcEditor.Marc,
                    out strXml,
                    out strError);
                 * */
                // 2008/5/16 changed
                nRet = MarcUtil.Marc2Xml(
    this.MarcEditor.Marc,
    strSyntax,
    out strXml,
    out strError);

                if (nRet == -1)
                    goto ERROR1;

                // 替换或者追加一个记录
                nRet = tempdlg.ReplaceRecord(tempdlg.textBox_name.Text,
                    strXml,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }
            }

            if (tempdlg.Changed == false)	// 没有必要保存回去
                return 0;

            string strOutputXml = tempdlg.OutputXml;

            nRet = dp2_searchform.SaveCfgFile(
                strCfgFilePath,
                strOutputXml,
                baCfgOutputTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "修改模板 '" + strCfgFilePath + "' 成功");
            return 0;
            ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_originData)
            {
                // 一旦发生改变，提示信息就不可逆。除非重新装载原始内容
                if (this.MarcEditor.Changed == true)
                    this.label_originDataWarning.Text = "警告：MARC编辑器中的记录已发生改变，和这里的原始数据不同了...";
            }
        }

        protected override bool ProcessDialogKey(
Keys keyData)
        {
            /*
            if (keyData == Keys.Enter)
            {
                this.button_OK_Click(this, null);
                return true;
            }*/

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            if (keyData == Keys.F2)
            {
                this.SaveRecord();
                return true;
            }

            if (keyData == Keys.F3)
            {
                this.SaveRecord("saveas");
                return true;
            }

            if (keyData == Keys.F4)
            {
                this.LoadTemplate();
                return true;
            }

            if (keyData == Keys.F5)
            {
                this.Reload();
                return true;
            }

            // return false;
            return base.ProcessDialogKey(keyData);
        }

        private void MarcEditor_VerifyData(object sender, GenerateDataEventArgs e)
        {
            this.VerifyData(sender, e);
        }

        public void VerifyData()
        {
            GenerateDataEventArgs e1 = new GenerateDataEventArgs();
            e1.FocusedControl = this.MarcEditor;
            this.VerifyData(this, e1, null, false);
        }

        // MARC格式校验
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm
        public void VerifyData(object sender,
            GenerateDataEventArgs e)
        {
            VerifyData(sender, e, null, false);
        }

        // MARC格式校验
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm
        /// <summary>
        /// MARC格式校验
        /// </summary>
        /// <param name="sender">从何处启动?</param>
        /// <param name="e">GenerateDataEventArgs对象，表示动作参数</param>
        /// <param name="bAutoVerify">是否自动校验。自动校验的时候，如果没有发现错误，则不出现最后的对话框</param>
        /// <returns>0: 没有发现校验错误; 1: 发现校验警告; 2: 发现校验错误</returns>
        public int VerifyData(object sender,
            GenerateDataEventArgs e,
            string strSavePath,
            bool bAutoVerify)
        {
            string strError = "";
            string strCode = "";
            string strRef = "";


            if (string.IsNullOrEmpty(strSavePath) == true)
                strSavePath = this.SavePath;

            if (String.IsNullOrEmpty(strSavePath) == true)
            {
                strError = "缺乏保存路径";
                goto ERROR1;
            }

            string strProtocol = "";
            string strPath = "";
            int nRet = Global.ParsePath(strSavePath,
                out strProtocol,
                out strPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            // dtlp协议的自动创建数据
            if (strProtocol.ToLower() == "dtlp")
            {
                strError = "暂不支持来自DTLP协议的数据自动创建功能";
                goto ERROR1;
            }

            // Debug.Assert(false, "");
            this.m_strVerifyResult = "正在校验...";
            // 自动校验的时候，如果没有发现错误，则不出现最后的对话框
            if (bAutoVerify == false)
            {
                // 如果固定面板隐藏，就打开窗口
                DoViewVerifyResult(this.MainForm.PanelFixedVisible == false ? true : false);
            }

            VerifyHost host = new VerifyHost();
            host.DetailForm = this;

            // dp2library协议的自动创建数据
            if (strProtocol.ToLower() == "dp2library")
            {
                dp2SearchForm dp2_searchform = this.GetDp2SearchForm();

                if (dp2_searchform == null)
                {
                    strError = "没有连接的或者打开的dp2检索窗，无法进行数据创建";
                    goto ERROR1;
                }

                string strServerName = "";
                string strLocalPath = "";
                // 解析记录路径。
                // 记录路径为如下形态 "中文图书/1 @服务器"
                dp2SearchForm.ParseRecPath(strPath,
                    out strServerName,
                    out strLocalPath);

                string strBiblioDbName = dp2SearchForm.GetDbName(strLocalPath);

                string strCfgFileName = "dp2catalog_marc_verify.fltx";

                string strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = dp2_searchform.GetCfgFile(strCfgPath,
                    out strCode,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strCfgFileName = "dp2catalog_marc_verify.cs";

                    strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                    baCfgOutputTimestamp = null;
                    nRet = dp2_searchform.GetCfgFile(strCfgPath,
                        out strCode,
                        out baCfgOutputTimestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        strError = "服务器上没有定义路径为 '" + strCfgPath + "' 的配置文件(或.fltx配置文件)，数据校验无法进行";
                        goto ERROR1;
                    }
                    if (nRet == -1)
                        goto ERROR1;

                    strCfgFileName = "dp2catalog_marc_verify.cs.ref";

                    strCfgPath = strBiblioDbName + "/cfgs/" + strCfgFileName + "@" + strServerName;

                    nRet = dp2_searchform.GetCfgFile(strCfgPath,
                        out strRef,
                        out baCfgOutputTimestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        strError = "服务器上没有定义路径为 '" + strCfgPath + "' 的配置文件，虽然定义了.cs配置文件。数据校验无法进行";
                        goto ERROR1;
                    }
                    if (nRet == -1)
                        goto ERROR1;

                    try
                    {
                        // 执行代码
                        nRet = RunVerifyCsScript(
                            sender,
                            e,
                            strCode,
                            strRef,
                            out host,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    catch (Exception ex)
                    {
                        strError = "执行脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                        goto ERROR1;
                    }
                }
                else
                {
                    VerifyFilterDocument filter = null;

                    nRet = this.PrepareVerifyMarcFilter(
                        host,
                        strCode,
                        out filter,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "编译文件 '" + strCfgFileName + "' 的过程中出错:\r\n" + strError;
                        goto ERROR1;
                    }

                    try
                    {

                        nRet = filter.DoRecord(null,
                            this.MarcEditor.Marc,
                            0,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                    }
                    catch (Exception ex)
                    {
                        strError = "filter.DoRecord error: " + ExceptionUtil.GetDebugText(ex);
                        goto ERROR1;
                    }
                }
            }

            bool bVerifyFail = false;
            if (string.IsNullOrEmpty(host.ResultString) == true)
            {
                if (this.m_verifyViewer != null)
                    this.m_verifyViewer.ResultString = "经过校验没有发现任何错误。";
            }
            else
            {
                if (bAutoVerify == true)
                {
                    // 延迟打开窗口
                    DoViewVerifyResult(this.MainForm.PanelFixedVisible == false ? true : false);
                }
                this.m_verifyViewer.ResultString = host.ResultString;
                this.MainForm.ActivateVerifyResultPage();   // 2014/7/13
                bVerifyFail = true;
            }

            return bVerifyFail == true ? 2 : 0;
            ERROR1:
            MessageBox.Show(this, strError);
            if (this.m_verifyViewer != null)
                this.m_verifyViewer.ResultString = strError;
            return 0;
        }

        int RunVerifyCsScript(
    object sender,
    GenerateDataEventArgs e,
    string strCode,
    string strRef,
            out VerifyHost hostObj,
    out string strError)
        {
            strError = "";
            string[] saRef = null;
            int nRet;
            hostObj = null;

            nRet = ScriptManager.GetRefsFromXml(strRef,
                out saRef,
                out strError);
            if (nRet == -1)
                return -1;

            // 2007/12/4
            ScriptManager.RemoveRefsBinDirMacro(ref saRef);

            string[] saAddRef = {
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\dp2catalog.exe"
                                };

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            Assembly assembly = null;
            string strErrorInfo = "";
            string strWarningInfo = "";
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                null,   // strLibPaths,
                out assembly,
                out strErrorInfo,
                out strWarningInfo);
            if (nRet == -1)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strErrorInfo;
                return -1;
            }

            // 得到Assembly中VerifyHost派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Catalog.VerifyHost");
            if (entryClassType == null)
            {

                strError = "dp2Catalog.VerifyHost派生类没有找到";
                return -1;
            }

            {
                // new一个VerifyHost派生对象
                hostObj = (VerifyHost)entryClassType.InvokeMember(null,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                    null);

                if (hostObj == null)
                {
                    strError = "new VerifyHost派生类对象失败";
                    return -1;
                }

                // 为Host派生类设置参数
                hostObj.DetailForm = this;
                hostObj.Assembly = assembly;

                HostEventArgs e1 = new HostEventArgs();
                e1.e = e;   // 2009/2/24

                hostObj.Main(sender, e1);
            }

            return 0;
        }

        public int PrepareVerifyMarcFilter(
    VerifyHost host,
    string strContent,
    out VerifyFilterDocument filter,
    out string strError)
        {
            strError = "";

            // 新创建
            // string strFilterFileContent = "";

            filter = new VerifyFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "VerifyHost Host = null;";

            filter.strPreInitial = " VerifyFilterDocument doc = (VerifyFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "VerifyHost" + ")doc.FilterHost;\r\n";

            try
            {
                filter.LoadContent(strContent);
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            // c#代码

            int nRet = filter.BuildScriptFile(out string strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string[] saAddRef1 = {
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\dp2catalog.exe"
                                };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
            // 本函数内对saRef不再进行宏替换
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                strLibPaths,
                out assembly,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                // MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;
            return 0;
            ERROR1:
            return -1;
        }

        string m_strVerifyResult = "";

        void DoViewVerifyResult(bool bOpenWindow)
        {
            string strError = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_verifyViewer == null || m_verifyViewer.Visible == false))
                    return;
            }


            if (this.m_verifyViewer == null
                || (bOpenWindow == true && this.m_verifyViewer.Visible == false))
            {
                m_verifyViewer = new VerifyViewerForm();
                // GuiUtil.SetControlFont(m_viewer, this.Font, false);
            }

            // m_viewer.MainForm = this.MainForm;  // 必须是第一句
            m_verifyViewer.Text = "校验结果";
            m_verifyViewer.ResultString = this.m_strVerifyResult;

            m_verifyViewer.DoDockEvent -= new DoDockEventHandler(m_viewer_DoDockEvent);
            m_verifyViewer.DoDockEvent += new DoDockEventHandler(m_viewer_DoDockEvent);

            m_verifyViewer.FormClosed -= new FormClosedEventHandler(m_viewer_FormClosed);
            m_verifyViewer.FormClosed += new FormClosedEventHandler(m_viewer_FormClosed);

            m_verifyViewer.Locate -= new LocateEventHandler(m_viewer_Locate);
            m_verifyViewer.Locate += new LocateEventHandler(m_viewer_Locate);

            if (bOpenWindow == true)
            {
                if (m_verifyViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_verifyViewer, "verify_viewer_state");
                    m_verifyViewer.Show(this);
                    m_verifyViewer.Activate();

                    this.MainForm.CurrentVerifyResultControl = null;
                }
                else
                {
                    if (m_verifyViewer.WindowState == FormWindowState.Minimized)
                        m_verifyViewer.WindowState = FormWindowState.Normal;
                    m_verifyViewer.Activate();
                }
            }
            else
            {
                if (m_verifyViewer.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentVerifyResultControl != m_verifyViewer.ResultControl)
                        m_verifyViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
            ERROR1:
            MessageBox.Show(this, "DoViewVerifyResult() 出错: " + strError);
        }

        void m_viewer_DoDockEvent(object sender, DoDockEventArgs e)
        {
            if (this.MainForm.CurrentVerifyResultControl != m_verifyViewer.ResultControl)
                this.MainForm.CurrentVerifyResultControl = m_verifyViewer.ResultControl;

            if (e.ShowFixedPanel == true
                && this.MainForm.PanelFixedVisible == false)
                this.MainForm.PanelFixedVisible = true;

            m_verifyViewer.Docked = true;
            m_verifyViewer.Visible = false;
        }

        void m_viewer_Locate(object sender, LocateEventArgs e)
        {
            string strError = "";

            string[] parts = e.Location.Split(new char[] { ',' });
            string strFieldName = "";
            int nFieldIndex = 0;
            string strSubfieldName = "";
            int nSubfieldIndex = 0;

            int nCharPos = 0;
            int nRet = 0;

            if (parts.Length == 0)
                return;
            if (parts.Length >= 1)
            {
                string strValue = parts[0].Trim();
                nRet = strValue.IndexOf("#");
                if (nRet == -1)
                    strFieldName = strValue;
                else
                {
                    strFieldName = strValue.Substring(0, nRet);
                    string strNumber = strValue.Substring(nRet + 1);
                    if (string.IsNullOrEmpty(strNumber) == false)
                    {
                        try
                        {
                            nFieldIndex = Convert.ToInt32(strNumber);
                        }
                        catch
                        {
                            strError = "字段位置 '" + strNumber + "' 格式不正确...";
                            goto ERROR1;
                        }
                        nFieldIndex--;
                    }
                }
            }

            if (parts.Length >= 2)
            {
                string strValue = parts[1].Trim();
                nRet = strValue.IndexOf("#");
                if (nRet == -1)
                    strSubfieldName = strValue;
                else
                {
                    strSubfieldName = strValue.Substring(0, nRet);
                    string strNumber = strValue.Substring(nRet + 1);
                    if (string.IsNullOrEmpty(strNumber) == false)
                    {
                        try
                        {
                            nSubfieldIndex = Convert.ToInt32(strNumber);
                        }
                        catch
                        {
                            strError = "子字段位置 '" + strNumber + "' 格式不正确...";
                            goto ERROR1;
                        }
                        nSubfieldIndex--;
                    }
                }
            }


            if (parts.Length >= 3)
            {
                string strValue = parts[2].Trim();
                if (string.IsNullOrEmpty(strValue) == false)
                {
                    try
                    {
                        nCharPos = Convert.ToInt32(strValue);
                    }
                    catch
                    {
                        strError = "字符位置 '" + strValue + "' 格式不正确...";
                        goto ERROR1;
                    }
                    if (nCharPos > 0)
                        nCharPos--;
                }
            }

            Field field = this.MarcEditor.Record.Fields[strFieldName, nFieldIndex];
            if (field == null)
            {
                strError = "当前MARC编辑器中不存在 名为 '" + strFieldName + "' 位置为 " + nFieldIndex.ToString() + " 的字段";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strSubfieldName) == true)
            {
                // 字段名
                if (nCharPos == -1)
                {
                    this.MarcEditor.SetActiveField(field, 2);
                }
                // 字段指示符
                else if (nCharPos == -2)
                {
                    this.MarcEditor.SetActiveField(field, 1);
                }
                else
                {
                    this.MarcEditor.FocusedField = field;
                    this.MarcEditor.SelectCurEdit(nCharPos, 0);
                }
                this.MarcEditor.EnsureVisible();
                return;
            }

            this.MarcEditor.FocusedField = field;
            this.MarcEditor.EnsureVisible();

            Subfield subfield = field.Subfields[strSubfieldName, nSubfieldIndex];
            if (subfield == null)
            {
                strError = "当前MARC编辑器中不存在 名为 '" + strSubfieldName + "' 位置为 " + nSubfieldIndex.ToString() + " 的子字段";
                goto ERROR1;
            }

            this.MarcEditor.SelectCurEdit(subfield.Offset + 2, 0);
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        void m_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_verifyViewer != null)
            {
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                    this.MainForm.AppInfo.UnlinkFormState(m_verifyViewer);

                this.m_verifyViewer = null;
            }
        }

        // 保存时自动查重
        public bool AutoVerifyData
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "entity_form",
                    "verify_data_when_saving",
                    false);
            }
        }

        private void MarcEditor_ParseMacro(object sender, ParseMacroEventArgs e)
        {
            string strResult = "";
            string strError = "";

            // 借助于MacroUtil进行处理
            int nRet = m_macroutil.Parse(
                e.Simulate,
                e.Macro,
                out strResult,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            e.Value = strResult;
        }


        void m_macroutil_ParseOneMacro(object sender, ParseOneMacroEventArgs e)
        {
            string strError = "";
            string strName = StringUtil.Unquote(e.Macro, "%%");  // 去掉百分号

            // 函数名：
            string strFuncName = "";
            string strParams = "";

            int nRet = strName.IndexOf(":");
            if (nRet == -1)
            {
                strFuncName = strName.Trim();
            }
            else
            {
                strFuncName = strName.Substring(0, nRet).Trim();
                strParams = strName.Substring(nRet + 1).Trim();
            }

            if (strName == "username"
                && String.IsNullOrEmpty(this.SavePath) == false)
            {
                e.Value = this.CurrentUserName;
                return;
            }

            string strValue = "";
            // 从marceditor_macrotable.xml文件中解析宏
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = MacroUtil.GetFromLocalMacroTable(PathUtil.MergePath(this.MainForm.UserDir, "marceditor_macrotable.xml"),
                strName,
                e.Simulate,
                out strValue,
                out strError);
            if (nRet == -1)
            {
                e.Canceled = true;
                e.ErrorInfo = strError;
                return;
            }

            if (nRet == 1)
            {
                e.Value = strValue;
                return;
            }

            ERROR1:
            e.Canceled = true;  // 不能解释处理
            return;
        }

#if NO
        static string Unquote(string strValue)
        {
            if (strValue.Length == 0)
                return "";
            if (strValue[0] == '%')
                strValue = strValue.Substring(1);
            if (strValue.Length == 0)
                return "";
            if (strValue[strValue.Length - 1] == '%')
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }
#endif

        public string CurrentUserName
        {
            get
            {
                string strError = "";
                int nRet = 0;
                string strProtocol = "";
                string strPath = "";
                nRet = Global.ParsePath(this.SavePath,
                    out strProtocol,
                    out strPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (strProtocol == "dp2library")
                {
                    dp2SearchForm dp2_searchform = null;

                    if (this.LinkedSearchForm is dp2SearchForm)
                        dp2_searchform = (dp2SearchForm)this.LinkedSearchForm;
                    else
                    {
                        dp2_searchform = this.GetDp2SearchForm();
                        if (dp2_searchform == null)
                        {
                            strError = "没有连接的或者打开的dp2检索窗";
                            goto ERROR1;
                        }
                    }

#if OLD_CHANNEL
                    if (dp2_searchform.Channel == null)
                    {
                        string strServerName = "";
                        string strLocalPath = "";

                        // 解析记录路径。
                        // 记录路径为如下形态 "中文图书/1 @服务器"
                        dp2SearchForm.ParseRecPath(strPath,
                            out strServerName,
                            out strLocalPath);

                        nRet = dp2_searchform.ForceLogin(
    null,
    strServerName,
    out strError);
                    }

                    if (dp2_searchform.Channel != null)
                        return dp2_searchform.Channel.UserName;
                    return "";
#endif
                    if (string.IsNullOrEmpty(dp2_searchform.CurrentUserName))
                    {
                        string strServerName = "";
                        string strLocalPath = "";

                        // 解析记录路径。
                        // 记录路径为如下形态 "中文图书/1 @服务器"
                        dp2SearchForm.ParseRecPath(strPath,
                            out strServerName,
                            out strLocalPath);

                        nRet = dp2_searchform.ForceLogin(
    null,
    strServerName,
    out strError);
                    }

                    return dp2_searchform.CurrentUserName;
                }

                return "";
                ERROR1:
                // throw new Exception(strError);
                return null;
            }
        }

        private void MarcEditor_Enter(object sender, EventArgs e)
        {
            API.PostMessage(this.Handle, WM_FILL_MARCEDITOR_SCRIPT_MENU, 0, 0);
        }

        private void MarcEditor_GetTemplateDef(object sender, GetTemplateDefEventArgs e)
        {
            if (this.m_detailHostObj == null)
            {
                int nRet = 0;
                string strError = "";

                // 初始化 dp2catalog_marc_autogen.cs 的 Assembly，并new MarcDetailHost对象
                // return:
                //      -2  清除了Assembly
                //      -1  error
                //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                //      1   重新(或者首次)初始化了Assembly
                nRet = InitialAutogenAssembly(out strError);
                if (nRet == -1 || nRet == -2)
                {
                    e.ErrorInfo = strError;
                    return;
                }
                if (nRet == 0)
                {
                    if (this.m_detailHostObj == null)
                    {
                        e.Canceled = true;
                        return; // 库名不具备，无法初始化
                    }
                }
                Debug.Assert(this.m_detailHostObj != null, "");
            }

            // 如果脚本里面没有相应的回调函数
            if (this.m_detailHostObj.GetType().GetMethod("GetTemplateDef",
                BindingFlags.DeclaredOnly |
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod
                ) == null)
            {
                e.Canceled = true;
                return;
            }

            // 有两个参数的成员函数
            Type classType = m_detailHostObj.GetType();
            try
            {
                classType.InvokeMember("GetTemplateDef",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.InvokeMethod
                    ,
                    null,
                    this.m_detailHostObj,
                    new object[] { sender, e });
            }
            catch (Exception ex)
            {
                e.ErrorInfo = GetExceptionMessage(ex) + "\r\n\r\n" + ExceptionUtil.GetDebugText(ex);  // GetExceptionMessage(ex);
                return;
            }
        }

        static string GetExceptionMessage(Exception ex)
        {
            string strResult = ex.GetType().ToString() + ":" + ex.Message;
            while (ex != null)
            {
                if (ex.InnerException != null)
                    strResult += "\r\n" + ex.InnerException.GetType().ToString() + ": " + ex.InnerException.Message;

                ex = ex.InnerException;
            }

            return strResult;
        }

        private void MarcEditor_TextChanged(object sender, EventArgs e)
        {
            this.RecordVersion = DateTime.Now.Ticks;
        }

        private void MarcDetailForm_Deactivate(object sender, EventArgs e)
        {
            SyncRecord();
        }

        // 根据 MARC 格式字符串获得 OID 字符串
        public static string GetSyntaxOID(string strMarcSyntax)
        {
            if (strMarcSyntax == "unimarc" || strMarcSyntax == "")
                return "1.2.840.10003.5.1";
            else if (strMarcSyntax == "usmarc")
                return "1.2.840.10003.5.10";
            else if (strMarcSyntax == "dt1000reader")
                return "1.2.840.10003.5.dt1000reader";
            else
                return "";
        }

        // 根据 OID 获得 MARC 格式字符串
        public static string GetMarcSyntax(string strOID)
        {
            if (strOID == "1.2.840.10003.5.1")
                return "unimarc";
            if (strOID == "1.2.840.10003.5.10")
                return "usmarc";

            return null;
        }

        void DisplayHtml(string strMARC, string strSytaxOID)
        {
            string strError = "";
            string strHtmlString = "";
            // return:
            //      -1  出错
            //      0   .fltx 文件没有找到
            //      1   成功
            int nRet = this.MainForm.BuildMarcHtmlText(
                strSytaxOID,
                strMARC,
                out strHtmlString,
                out strError);
            if (nRet == -1)
                strHtmlString = strError.Replace("\r\n", "<br/>");
            if (nRet == 0)
            {
                // TODO: 清除
                return;
            }

            Global.SetHtmlString(this.webBrowser_html,
    strHtmlString,
    this.MainForm.DataDir,
    "marcdetailform_biblio");
        }

        #region MARC21 --> HTML

        static string GetMaterialType(MarcRecord record)
        {
            if ("at".IndexOf(record.Header[6]) != -1
                && "acdm".IndexOf(record.Header[7]) != -1)
                return "Book";  // Books

            if (record.Header[6] == "m")
                return "Computer Files";

            if ("df".IndexOf(record.Header[6]) != -1)
                return "Map";  // Maps

            if ("cdij".IndexOf(record.Header[6]) != -1)
                return "Music";  // Music

            if ("a".IndexOf(record.Header[6]) != -1
    && "bis".IndexOf(record.Header[7]) != -1)
                return "Periodical or Newspaper";  // Continuing Resources

            if ("gkor".IndexOf(record.Header[6]) != -1)
                return "Visual Material";  // Visual Materials

            if (record.Header[6] == "p")
                return "Mixed Material";    // Mixed Materials

            return "";
        }

        // 直接串联每个子字段的内容
        static string ConcatSubfields(MarcNodeList nodes)
        {
            StringBuilder text = new StringBuilder(4096);
            foreach (MarcNode node in nodes)
            {
                text.Append(node.Content + " ");
            }

            return text.ToString().Trim();
        }
        // 组合构造若干个普通字段内容
        // parameters:
        //      strSubfieldNameList 筛选的子字段名列表。如果为 null，表示不筛选
        static string BuildFields(MarcNodeList fields,
            string strSubfieldNameList = null)
        {
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {

                    StringBuilder temp = new StringBuilder(4096);
                    foreach (MarcNode subfield in nodes)
                    {
                        if (strSubfieldNameList != null)
                        {
                            if (strSubfieldNameList.IndexOf(subfield.Name) == -1)
                                continue;
                        }
                        temp.Append(subfield.Content + " ");
                    }

                    if (temp.Length > 0)
                    {
                        if (i > 0)
                            text.Append("|");
                        text.Append(temp.ToString().Trim());
                        i++;
                    }
                }
            }

            return text.ToString().Trim();
        }

        // 组合构造若干个主题字段内容
        static string BuildSubjects(MarcNodeList fields)
        {
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    if (i > 0)
                        text.Append("|");

                    bool bPrevContent = false;  // 前一个子字段是除了 x y z 以外的子字段
                    StringBuilder temp = new StringBuilder(4096);
                    foreach (MarcNode subfield in nodes)
                    {
                        if (subfield.Name == "2")
                            continue;   // 不使用 $2

                        if (subfield.Name == "x"
                            || subfield.Name == "y"
                            || subfield.Name == "z"
                            || subfield.Name == "v")
                        {
                            temp.Append("--");
                            temp.Append(subfield.Content);
                            bPrevContent = false;
                        }
                        else
                        {
                            if (bPrevContent == true)
                                temp.Append(" ");
                            temp.Append(subfield.Content);
                            bPrevContent = true;
                        }
                    }

                    text.Append(temp.ToString().Trim());
                    i++;
                }
            }

            return text.ToString().Trim();
        }

        // 组合构造若干个856字段内容
        static string BuildLinks(MarcNodeList fields)
        {
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (MarcNode field in fields)
            {
                MarcNodeList nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    string u = "";
                    MarcNodeList single = nodes.select("subfield[@name='u']");
                    if (single.count > 0)
                    {
                        u = single[0].Content;
                    }

                    string z = "";
                    single = nodes.select("subfield[@name='z']");
                    if (single.count > 0)
                    {
                        z = single[0].Content;
                    }

                    string t3 = "";
                    single = nodes.select("subfield[@name='3']");
                    if (single.count > 0)
                    {
                        t3 = single[0].Content;
                    }

                    if (i > 0)
                        text.Append("|");

                    StringBuilder temp = new StringBuilder(4096);

                    if (string.IsNullOrEmpty(t3) == false)
                        temp.Append(t3 + ": <|");

                    temp.Append("url:" + u);
                    temp.Append(" text:" + u);
                    if (string.IsNullOrEmpty(z) == false)
                        temp.Append("|>  " + z);

                    text.Append(temp.ToString().Trim());
                    i++;
                }
            }

            return text.ToString().Trim();
        }

        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "bibliohtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    //"<script type='text/javascript' src='%datadir%/jquery.js'></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";
        }

        int BuildMarc21Html(string strMARC,
            out string strHtmlString,
            out string strError)
        {
            strError = "";
            strHtmlString = "";

            StringBuilder text = new StringBuilder();

            List<NameValueLine> results = null;
            int nRet = ScriptMarc21(strMARC,
            out results,
            out strError);
            if (nRet == -1)
                return -1;

            text.Append("<html>" + GetHeadString(false) + "<body><table class='biblio'>");
            foreach (NameValueLine line in results)
            {
                text.Append("<tr class='content'>");

                text.Append("<td class='name'>" + HttpUtility.HtmlEncode(line.Name) + "</td>");
                text.Append("<td class='value'>" + HttpUtility.HtmlEncode(line.Value) + "</td>");

                text.Append("</tr>");
            }
            text.Append("</table></body></html>");
            strHtmlString = text.ToString();
            return 0;
        }

        static int ScriptMarc21(string strMARC,
            out List<NameValueLine> results,
            out string strError)
        {
            strError = "";
            results = new List<NameValueLine>();

            MarcRecord record = new MarcRecord(strMARC);

            if (record.ChildNodes.count == 0)
                return 0;

            // LC control no.
            MarcNodeList nodes = record.select("field[@name='010']/subfield[@name='a']");
            if (nodes.count > 0)
            {
                results.Add(new NameValueLine("LC control no.", nodes[0].Content.Trim()));
            }

            // Type of material
            results.Add(new NameValueLine("Type of material", GetMaterialType(record)));

            // Personal name
            MarcNodeList fields = record.select("field[@name='100']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new NameValueLine("Personal name", ConcatSubfields(nodes)));
                }
            }

            // Corporate name
            fields = record.select("field[@name='110']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Corporate name", BuildFields(fields)));
            }

            // Uniform title
            fields = record.select("field[@name='240']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Uniform title", BuildFields(fields)));
            }

            // Main title
            fields = record.select("field[@name='245']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Main title", BuildFields(fields)));
            }
#if NO
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new OneLine("Main title", ConcatSubfields(nodes)));
                }
            }
#endif

            // Portion of title
            fields = record.select("field[@name='246' and @indicator2='0']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Portion of title", BuildFields(fields)));
            }

            // Spine title
            fields = record.select("field[@name='246' and @indicator2='8']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Spine title", BuildFields(fields)));
            }

            // Edition
            fields = record.select("field[@name='250']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Edition", BuildFields(fields)));
            }

            // Published/Created
            fields = record.select("field[@name='260']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new NameValueLine("Published/Created", ConcatSubfields(nodes)));
                }
            }

            // Related names
            fields = record.select("field[@name='700' or @name='710' or @name='711']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Related names", BuildFields(fields)));
            }

            // Related titles
            fields = record.select("field[@name='730' or @name='740']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Related titles", BuildFields(fields)));
            }

            // Description
            fields = record.select("field[@name='300' or @name='362']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Description", BuildFields(fields)));
            }

            // ISBN
            fields = record.select("field[@name='020']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("ISBN", BuildFields(fields)));
            }

            // Current frequency
            fields = record.select("field[@name='310']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Current frequency", BuildFields(fields)));
            }

            // Former title
            fields = record.select("field[@name='247']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Former title", BuildFields(fields)));
            }

            // Former frequency
            fields = record.select("field[@name='321']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Former frequency", BuildFields(fields)));
            }

            // Continues
            fields = record.select("field[@name='780']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Continues", BuildFields(fields)));
            }

            // ISSN
            MarcNodeList subfields = record.select("field[@name='022']/subfield[@name='a']");
            if (subfields.count > 0)
            {
                results.Add(new NameValueLine("ISSN", ConcatSubfields(subfields)));
            }

            // Linking ISSN
            subfields = record.select("field[@name='022']/subfield[@name='l']");
            if (subfields.count > 0)
            {
                results.Add(new NameValueLine("Linking ISSN", ConcatSubfields(subfields)));
            }

            // Invalid LCCN
            subfields = record.select("field[@name='010']/subfield[@name='z']");
            if (subfields.count > 0)
            {
                results.Add(new NameValueLine("Invalid LCCN", ConcatSubfields(subfields)));
            }

            // Contents
            fields = record.select("field[@name='505' and @indicator1='0']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Contents", BuildFields(fields)));
            }

            // Partial contents
            fields = record.select("field[@name='505' and @indicator1='2']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Partial contents", BuildFields(fields)));
            }

            // Computer file info
            fields = record.select("field[@name='538']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Computer file info", BuildFields(fields)));
            }

            // Notes
            fields = record.select("field[@name='500'  or @name='501' or @name='504' or @name='561' or @name='583' or @name='588' or @name='590']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Notes", BuildFields(fields)));
            }

            // References
            fields = record.select("field[@name='510']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("References", BuildFields(fields)));
            }

            // Additional formats
            fields = record.select("field[@name='530' or @name='533' or @name='776']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Additional formats", BuildFields(fields)));
            }



            // Subjects
            fields = record.select("field[@name='600' or @name='610' or @name='630' or @name='650' or @name='651']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Subjects", BuildSubjects(fields)));
            }

            // Form/Genre
            fields = record.select("field[@name='655']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Form/Genre", BuildSubjects(fields)));
            }

            // Series
            fields = record.select("field[@name='440' or @name='490' or @name='830']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Series", BuildFields(fields)));
            }


            // LC classification
            fields = record.select("field[@name='050']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("LC classification", BuildFields(fields)));
            }
#if NO
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield");
                if (nodes.count > 0)
                {
                    results.Add(new OneLine("LC classification", ConcatSubfields(nodes)));
                }
            }
#endif

            // NLM class no.
            fields = record.select("field[@name='060']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("NLM class no.", BuildFields(fields)));
            }


            // Dewey class no.
            // 不要 $2
            fields = record.select("field[@name='082']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Dewey class no.", BuildFields(fields, "a")));
            }


            // NAL class no.
            fields = record.select("field[@name='070']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("NAL class no.", BuildFields(fields)));
            }

            // National bib no.
            fields = record.select("field[@name='015']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("National bib no.", BuildFields(fields, "a")));
            }

            // National bib agency no.
            fields = record.select("field[@name='016']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("National bib agency no.", BuildFields(fields, "a")));
            }

            // LC copy
            fields = record.select("field[@name='051']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("LC copy", BuildFields(fields)));
            }

            // Other system no.
            fields = record.select("field[@name='035'][subfield[@name='a']]");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Other system no.", BuildFields(fields, "a")));
            }
#if NO
            fields = record.select("field[@name='035']");
            foreach (MarcNode field in fields)
            {
                nodes = field.select("subfield[@name='a']");
                if (nodes.count > 0)
                {
                    results.Add(new OneLine("Other system no.", ConcatSubfields(nodes)));
                }
            }
#endif

            // Reproduction no./Source
            fields = record.select("field[@name='037']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Reproduction no./Source", BuildFields(fields)));
            }

            // Geographic area code
            fields = record.select("field[@name='043']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Geographic area code", BuildFields(fields)));
            }

            // Quality code
            fields = record.select("field[@name='042']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Quality code", BuildFields(fields)));
            }

            // Links
            fields = record.select("field[@name='856'or @name='859']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Links", BuildLinks(fields)));
            }

            // Content type
            fields = record.select("field[@name='336']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Content type", BuildFields(fields, "a")));
            }

            // Media type
            fields = record.select("field[@name='337']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Media type", BuildFields(fields, "a")));
            }

            // Carrier type
            fields = record.select("field[@name='338']");
            if (fields.count > 0)
            {
                results.Add(new NameValueLine("Carrier type", BuildFields(fields, "a")));
            }

            return 0;
        }



        #endregion

        public bool GetQueryContent(out string strUse,
            out string strWord)
        {
            strUse = "";
            strWord = "";

            string strError = "";
            string strMarcSyntax = "";
            string strMarcSyntaxOID = this.GetCurrentMarcSyntaxOID(out strError);
            if (String.IsNullOrEmpty(strMarcSyntaxOID) == true)
            {
                /*
                strError = "当前MARC syntax OID为空，无法判断MARC具体格式";
                goto ERROR1;
                 * */
                return false;
            }

            if (strMarcSyntaxOID == "1.2.840.10003.5.1")
                strMarcSyntax = "unimarc";
            if (strMarcSyntaxOID == "1.2.840.10003.5.10")
                strMarcSyntax = "usmarc";

            string strMARC = this.MarcEditor.Marc;
            MarcRecord record = new MarcRecord(strMARC);

            if (strMarcSyntax == "unimarc")
            {
                strWord = record.select("field[@name='010']/subfield[@name='a']").FirstContent;
                if (string.IsNullOrEmpty(strWord) == true)
                {
                    strWord = record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                    strUse = "title";
                }
                else
                    strUse = "ISBN";
                if (string.IsNullOrEmpty(strWord) == true)
                    return false;

                return true;
            }
            if (strMarcSyntax == "usmarc")
            {
                strWord = record.select("field[@name='020']/subfield[@name='a']").FirstContent;
                if (string.IsNullOrEmpty(strWord) == true)
                {
                    strWord = record.select("field[@name='245']/subfield[@name='a']").FirstContent;
                    strUse = "title";
                }
                else
                    strUse = "ISBN";
                if (string.IsNullOrEmpty(strWord) == true)
                    return false;

                return true;
            }
            return false;
        }

        public void CopyMarcToFixed()
        {
            MarcDetailForm exist_fixed = this.MainForm.FixedMarcDetailForm;
            if (exist_fixed == null)
            {
                MessageBox.Show(this, "固定记录窗不存在，无法进行复制操作");
                return;
            }

            string strMARC = this.MarcEditor.Marc;
            // TODO: 需要检查 MARC 格式是否一致
            exist_fixed.MarcEditor.Marc = strMARC;
            exist_fixed.ShowMessage("MARC 编辑器中已成功复制了新内容", "green", true);
        }
    }

    public class VerifyHost
    {
        public MarcDetailForm DetailForm = null;
        public string ResultString = "";
        public Assembly Assembly = null;

        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // 调用成员函数
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod
                ,
                null,
                this,
                null);
        }

        public virtual void Main(object sender, HostEventArgs e)
        {

        }
    }

    public class VerifyFilterDocument : FilterDocument
    {
        public VerifyHost FilterHost = null;
    }

    public class NameValueLine
    {
        public string Name = "";
        public string Value = "";

        public NameValueLine()
        {
        }

        public NameValueLine(string strName, string strValue)
        {
            Name = strName;
            Value = strValue;
        }
    }

}