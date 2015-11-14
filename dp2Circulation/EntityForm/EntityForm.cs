// #define _TEST_PINYIN

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MarcDom;
using DigitalPlatform.GcatClient;

using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.GcatClient.gcat_new_ws;
using DigitalPlatform.CommonDialog;
using DigitalPlatform.MessageClient;

namespace dp2Circulation
{
    /// <summary>
    /// 种册窗
    /// </summary>
    public partial class EntityForm : MyForm
    {
        GenerateData _genData = null;

        // 模板界面的内容版本号
        int _templateVersion = 0;

        // MARC 界面的内容版本号
        int _marcEditorVersion = 0;

        CommentViewerForm m_commentViewer = null;

        WebExternalHost m_webExternalHost_biblio = new WebExternalHost();
        // WebExternalHost m_webExternalHost_comment = new WebExternalHost();

        // 存储书目和<dprms:file>以外的其它XML片断
        XmlDocument domXmlFragment = null;

        VerifyViewerForm m_verifyViewer = null;

        List<PendingLoadRequest> m_listPendingLoadRequest = new List<PendingLoadRequest>();

        SelectedTemplateCollection selected_templates = new SelectedTemplateCollection();

        int m_nChannelInUse = 0; // >0表示通道正在被使用

        /// <summary>
        /// 是否正在装载记录的中途
        /// </summary>
        public bool IsLoading
        {
            get
            {
                if (this.m_nChannelInUse > 0)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// 目标记录路径
        /// </summary>
        public string TargetRecPath
        {
            get
            {
                return this.orderControl1.TargetRecPath;
            }
            set
            {
                this.orderControl1.TargetRecPath = value;
                this.issueControl1.TargetRecPath = value;
            }
        }

        /// <summary>
        /// 是否为验收模式
        /// </summary>
        public bool AcceptMode
        {
            get
            {
                return this._bAcceptMode;
            }
            set
            {
                this._bAcceptMode = value;
#if ACCEPT_MODE
                this.SupressSizeSetting = value;
#endif
            }
        }

        bool _bAcceptMode = false; // 是否为验收模式 如果是，则一切设施都设置为便利批处理验收的状态；如果否，为普通状态

        MacroUtil m_macroutil = new MacroUtil();   // 宏处理器

        bool m_bDeletedMode = false;    // 是否处在刚删除后残留了书目和实体信息，但是不让编辑的特殊状态

        string m_strOriginBiblioXml = ""; // 最初从数据库或模板中调入的XML书目数据

        string BiblioOriginPath = "";   // 书目记录在数据库中的原始路径

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;
#endif

        // BookItemCollection bookitems = null;

        // string m_strBiblioRecPath = ""; // 本窗口中的种记录路径

        // BiblioDbFromInfo[] DbFromInfos = null;

        BrowseSearchResultForm browseWindow = null; 

        int m_nInSearching = 0;
        // string m_strTempBiblioRecPath = "";

        RegisterType m_registerType = RegisterType.Register;

        // const int WM_PREPARE = API.WM_USER + 200;
        const int WM_SWITCH_FOCUS = API.WM_USER + 201;
        // const int WM_LOADLAYOUT = API.WM_USER + 202;
        const int WM_SEARCH_DUP = API.WM_USER + 203;
        const int WM_VERIFY_DATA = API.WM_USER + 204;
        const int WM_FILL_MARCEDITOR_SCRIPT_MENU = API.WM_USER + 205;

        // 消息WM_SWITCH_FOCUS的wparam参数值
        const int BIBLIO_SEARCHTEXT = 0;
        const int ITEM_BARCODE = 1;
        const int MARC_EDITOR = 2;
        const int ITEM_LIST = 3;    // 册列表
        const int ORDER_LIST = 4;   // 订购列表
        const int ISSUE_LIST = 5;   // 期列表
        const int COMMENT_LIST = 6;   // 评注列表

        /// <summary>
        /// 书目记录时间戳
        /// </summary>
        public byte[] BiblioTimestamp = null;

        // 
        /// <summary>
        /// 当前记录的书目库名。
        /// 主要给C#二次开发脚本用
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return Global.GetDbName(this.BiblioRecPath);
            }
        }

        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string BiblioRecPath
        {
            get
            {
                return this.textBox_biblioRecPath.Text;
            }
            set
            {
                string strOldDbName = Global.GetDbName(this.BiblioRecPath);
                string strNewDbName = Global.GetDbName(value);


                this.textBox_biblioRecPath.Text = value;

                if (this.entityControl1 != null)
                {
                    this.entityControl1.BiblioRecPath = value;
                }

                if (this.issueControl1 != null)
                {
                    this.issueControl1.BiblioRecPath = value;
                }

                if (this.orderControl1 != null)
                {
                    this.orderControl1.BiblioRecPath = value;
                }

                if (this.binaryResControl1 != null)
                {
                    this.binaryResControl1.BiblioRecPath = value;
                }

                if (this.commentControl1 != null)
                {
                    this.commentControl1.BiblioRecPath = value;
                }

                // 刷新窗口标题
                this.Text = "种册 " + value;

                // 迫使获得新的配置文件
                if (strOldDbName != strNewDbName)
                {
                    this.m_marcEditor.MarcDefDom = null;  
                    this.m_marcEditor.Invalidate();   // TODO: ??
                }

                // 显示Ctrl+A菜单
                if (this.MainForm.PanelFixedVisible == true)
                    this._genData.AutoGenerate(this.m_marcEditor,
                        new GenerateDataEventArgs(),
                        GetBiblioRecPathOrSyntax(),
                        true);
            }
        }

        // 2009/2/3 
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// 册记录列表控件
        /// </summary>
        public EntityControl EntityControl
        {
            get
            {
                return this.entityControl1;
            }
        }

        /// <summary>
        /// 期记录列表控件
        /// </summary>
        public IssueControl IssueControl
        {
            get
            {
                return this.issueControl1;
            }
        }

        /// <summary>
        /// 订购记录列表控件
        /// </summary>
        public OrderControl OrderControl
        {
            get
            {
                return this.orderControl1;
            }
        }

        /// <summary>
        /// 评注记录列表控件
        /// </summary>
        public CommentControl CommentControl
        {
            get
            {
                return this.commentControl1;
            }
        }

        /// <summary>
        /// 对象资源列表控件
        /// </summary>
        public BinaryResControl BinaryResControl
        {
            get
            {
                return this.binaryResControl1;
            }
        }

        /// <summary>
        /// MARC 编辑器
        /// </summary>
        public DigitalPlatform.Marc.MarcEditor MarcEditor
        {
            get
            {
                return m_marcEditor;
            }
        }

        // 获得当前窗口的 MARC 字符串
        // 本功能内会促使两个编辑器同步，然后再获得最新字符串
        public string GetMarc()
        {
            SynchronizeMarc();
            return this.m_marcEditor.Marc;
        }

        // 设置当前窗口的 MARC 字符串
        // 本功能会设置两个编辑器的 MARC 字符串
        public void SetMarc(string strMarc)
        {
            this.m_marcEditor.Marc = strMarc;
            this.easyMarcControl1.SetMarc(strMarc);
            this._marcEditorVersion = 0;
            this._templateVersion = 0;
        }

        public void SynchronizeMarc()
        {
            if (this._marcEditorVersion < this._templateVersion)
            {
                this.m_marcEditor.Marc = this.easyMarcControl1.GetMarc();
            }
            if (this._marcEditorVersion > this._templateVersion)
            {
                this.easyMarcControl1.SetMarc(this.m_marcEditor.Marc);
            }
            this._marcEditorVersion = 0;
            this._templateVersion = 0;
        }

        public void SetMarcChanged(bool bChanged)
        {
            this.m_marcEditor.Changed = bChanged;
            this.easyMarcControl1.Changed = bChanged;
        }

        public bool GetMarcChanged()
        {
            // SynchronizeMarc();
            return this.m_marcEditor.Changed || this.easyMarcControl1.Changed;
        }

        // public bool m_bRemoveDeletedItem = false;   // 在删除册事项时, 是否从视觉上抹除这些事项(实际上内存里面还保留有即将提交的事项)?

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityForm()
        {
            InitializeComponent();
        }

        void EnableItemsPage(bool bEnable)
        {
            EnablePage(bEnable,
                this.tabControl_itemAndIssue,
                this.tabPage_item);

            // 2014/9/5
            // 禁止或者允许册条码号输入域
            this.textBox_itemBarcode.Enabled = bEnable;
            this.button_register.Enabled = bEnable;
        }

        void EnableObjectsPage(bool bEnable)
        {
            EnablePage(bEnable,
                this.tabControl_itemAndIssue,
                this.tabPage_object);
        }

        void EnableIssuesPage(bool bEnable)
        {
            EnablePage(bEnable,
                this.tabControl_itemAndIssue,
                this.tabPage_issue);
        }

        void EnableOrdersPage(bool bEnable)
        {
            EnablePage(bEnable,
                this.tabControl_itemAndIssue,
                this.tabPage_order);
        }

        void EnableCommentsPage(bool bEnable)
        {
            EnablePage(bEnable,
                this.tabControl_itemAndIssue,
                this.tabPage_comment);
        }

        bool ItemsPageVisible
        {
            get
            {
                return this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_item) != -1;
            }
        }

        void EnablePage(bool bEnable,
            TabControl container,
            TabPage page)
        {
            if (bEnable == true)
            {
                if (container.TabPages.IndexOf(page) == -1)
                {
                    container.TabPages.Add(page);
                    this.RemoveFreeControl(page);
                }
            }
            else
            {
                if (container.TabPages.IndexOf(page) != -1)
                {
                    container.TabPages.Remove(page);
                    this.AddFreeControl(page);
                }
            }
        }

        // AppDomain m_scriptDomain = null;

        private void EntityForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            // 2015/5/27
            this._genData = new GenerateData(this, null);
            this._genData.SynchronizeMarcEvent += _genData_SynchronizeMarcEvent;

            // m_scriptDomain = AppDomain.CreateDomain("script");
#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);

                        // 2012/10/3
            this.Channel.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

            Stop = new DigitalPlatform.Stop();
            Stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.m_webExternalHost_biblio.Initial(this.MainForm, this.webBrowser_biblioRecord);
            this.webBrowser_biblioRecord.ObjectForScripting = this.m_webExternalHost_biblio;

            // this.m_webExternalHost_comment.Initial(this.MainForm);

            this.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);

            // LoadLayout0();
            if (this.AcceptMode == false)
            {
#if NO
                // 设置窗口尺寸状态
                MainForm.AppInfo.LoadMdiChildFormStates(this,
                    "mdi_form_state");
#endif
            }
            else
            {
                Form form = this;
                FormWindowState savestate = form.WindowState;
                bool bStateChanged = false;
                if (form.WindowState != FormWindowState.Normal)
                {
                    form.WindowState = FormWindowState.Normal;
                    bStateChanged = true;
                }

                AppInfo_LoadMdiSize(this, null);

                if (bStateChanged == true)
                    form.WindowState = savestate;
            }

            if (this.AcceptMode == true)
            {
#if ACCEPT_MODE
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
#endif
            }


            // 如果为禁止编目功能
            if (this.Cataloging == false)
            {
                this.tabControl_biblioInfo.TabPages.Remove(this.tabPage_marc);
                this.AddFreeControl(this.tabPage_marc); // 2015/11/7
                this.toolStrip_marcEditor.Enabled = false;

                // 对象管理功能也被禁止
                this.tabControl_itemAndIssue.TabPages.Remove(this.tabPage_object);
                this.AddFreeControl(this.tabPage_object);   // 2015/11/7
                this.binaryResControl1.Enabled = false;
            }


            this.MainForm.FillBiblioFromList(this.comboBox_from);

            // 恢复上次退出时保留的检索途径
            string strFrom = this.MainForm.AppInfo.GetString(
            "entityform",
            "search_from",
            "");
            if (String.IsNullOrEmpty(strFrom) == false)
                this.comboBox_from.Text = strFrom;

            this.checkedComboBox_biblioDbNames.Text = this.MainForm.AppInfo.GetString(
                "entityform",
                "search_dbnames",
                "<全部>");

            this.comboBox_matchStyle.Text = this.MainForm.AppInfo.GetString(
                "entityform",
                "search_matchstyle",
                "前方一致");


            /*
            // 2008/6/25 
            this.checkBox_autoDetectQueryBarcode.Checked = this.MainForm.AppInfo.GetBoolean(
                "entityform",
                "auto_detect_query_barcode",
                true);
             * */

            this.checkBox_autoSavePrev.Checked = this.MainForm.AppInfo.GetBoolean(
                "entityform",
                "auto_save_prev",
                true);

            this.BiblioChanged = false;

            // 保存当前活动的属性页名字，因为后面可能要清除有关page
            this.m_strUsedActiveItemPage = GetActiveItemPageName();

            // 初始化册控件
            this.entityControl1.GetMacroValue -= new GetMacroValueHandler(issueControl1_GetMacroValue);
            this.entityControl1.GetMacroValue += new GetMacroValueHandler(issueControl1_GetMacroValue);

            this.entityControl1.ContentChanged -= new ContentChangedEventHandler(issueControl1_ContentChanged);
            this.entityControl1.ContentChanged += new ContentChangedEventHandler(issueControl1_ContentChanged);

            this.entityControl1.GetParameterValue -= new GetParameterValueHandler(entityControl1_GetParameterValue);
            this.entityControl1.GetParameterValue += new GetParameterValueHandler(entityControl1_GetParameterValue);

            this.entityControl1.VerifyBarcode -= new VerifyBarcodeHandler(entityControl1_VerifyBarcode);
            this.entityControl1.VerifyBarcode += new VerifyBarcodeHandler(entityControl1_VerifyBarcode);

            this.entityControl1.EnableControlsEvent -= new EnableControlsHandler(entityControl1_EnableControls);
            this.entityControl1.EnableControlsEvent += new EnableControlsHandler(entityControl1_EnableControls);

            this.entityControl1.LoadRecord -= new LoadRecordHandler(entityControl1_LoadRecord111);
            this.entityControl1.LoadRecord += new LoadRecordHandler(entityControl1_LoadRecord111);

            // 2009/2/24 
            this.entityControl1.GenerateData -= new GenerateDataEventHandler(entityControl1_GenerateData);
            this.entityControl1.GenerateData += new GenerateDataEventHandler(entityControl1_GenerateData);

            /*
            // 2009/2/24 
            this.entityControl1.GenerateAccessNo -= new GenerateDataEventHandler(entityControl1_GenerateAccessNo);
            this.entityControl1.GenerateAccessNo += new GenerateDataEventHandler(entityControl1_GenerateAccessNo); 
             * */

            this.entityControl1.ShowMessage -= entityControl1_ShowMessage;
            this.entityControl1.ShowMessage += entityControl1_ShowMessage;

            this.entityControl1.Channel = this.Channel;
            this.entityControl1.Stop = this.Progress;
            this.entityControl1.MainForm = this.MainForm;

            this.EnableItemsPage(false);


            // 初始化期控件

            // 2008/12/27 
            this.issueControl1.GenerateEntity -= new GenerateEntityEventHandler(issueControl1_GenerateEntity);
            this.issueControl1.GenerateEntity += new GenerateEntityEventHandler(issueControl1_GenerateEntity);

            // 2008/12/24 
            this.issueControl1.GetOrderInfo -= new GetOrderInfoEventHandler(issueControl1_GetOrderInfo);
            this.issueControl1.GetOrderInfo += new GetOrderInfoEventHandler(issueControl1_GetOrderInfo);

            this.issueControl1.GetItemInfo -= new GetItemInfoEventHandler(issueControl1_GetItemInfo);
            this.issueControl1.GetItemInfo += new GetItemInfoEventHandler(issueControl1_GetItemInfo);

            this.issueControl1.GetMacroValue -= new GetMacroValueHandler(issueControl1_GetMacroValue);
            this.issueControl1.GetMacroValue += new GetMacroValueHandler(issueControl1_GetMacroValue);

            this.issueControl1.ContentChanged -= new ContentChangedEventHandler(issueControl1_ContentChanged);
            this.issueControl1.ContentChanged += new ContentChangedEventHandler(issueControl1_ContentChanged);

            this.issueControl1.EnableControlsEvent -= new EnableControlsHandler(entityControl1_EnableControls);
            this.issueControl1.EnableControlsEvent += new EnableControlsHandler(entityControl1_EnableControls);

            this.issueControl1.ChangeItem -= new ChangeItemEventHandler(issueControl1_ChangeItem);
            this.issueControl1.ChangeItem += new ChangeItemEventHandler(issueControl1_ChangeItem);

            // 2010/4/27
            this.issueControl1.LoadRecord -= new LoadRecordHandler(entityControl1_LoadRecord111);
            this.issueControl1.LoadRecord += new LoadRecordHandler(entityControl1_LoadRecord111);

            // 2012/9/22
            this.issueControl1.GenerateData -= new GenerateDataEventHandler(entityControl1_GenerateData);
            this.issueControl1.GenerateData += new GenerateDataEventHandler(entityControl1_GenerateData);

            this.issueControl1.Channel = this.Channel;
            this.issueControl1.Stop = this.Progress;
            this.issueControl1.MainForm = this.MainForm;

            this.EnableIssuesPage(false);

            // 2010/4/27
            this.issueControl1.InputItemsBarcode = this.MainForm.AppInfo.GetBoolean(
                "entity_form",
                "issueControl_input_item_barcode",
                true);
            // 2011/9/8
            this.issueControl1.SetProcessingState = this.MainForm.AppInfo.GetBoolean(
                "entity_form",
                "issueControl_set_processing_state",
                true);
            // 2012/5/7
            this.issueControl1.CreateCallNumber = this.MainForm.AppInfo.GetBoolean(
                "entity_form",
                "create_callnumber",
                false);

            // 初始化采购控件
            this.orderControl1.GetMacroValue -= new GetMacroValueHandler(issueControl1_GetMacroValue);
            this.orderControl1.GetMacroValue += new GetMacroValueHandler(issueControl1_GetMacroValue);

            this.orderControl1.ContentChanged -= new ContentChangedEventHandler(issueControl1_ContentChanged);
            this.orderControl1.ContentChanged += new ContentChangedEventHandler(issueControl1_ContentChanged);

            this.orderControl1.EnableControlsEvent -= new EnableControlsHandler(entityControl1_EnableControls);
            this.orderControl1.EnableControlsEvent += new EnableControlsHandler(entityControl1_EnableControls);

            this.orderControl1.GenerateEntity -= new GenerateEntityEventHandler(orderControl1_GenerateEntity);
            this.orderControl1.GenerateEntity += new GenerateEntityEventHandler(orderControl1_GenerateEntity);

            // 2008/11/4 
            this.orderControl1.OpenTargetRecord -= new OpenTargetRecordEventHandler(orderControl1_OpenTargetRecord);
            this.orderControl1.OpenTargetRecord += new OpenTargetRecordEventHandler(orderControl1_OpenTargetRecord);

            this.orderControl1.HilightTargetItem -= new HilightTargetItemsEventHandler(orderControl1_HilightTargetItem);
            this.orderControl1.HilightTargetItem += new HilightTargetItemsEventHandler(orderControl1_HilightTargetItem);

            // 2009/11/8 
            this.orderControl1.SetTargetRecPath -= new SetTargetRecPathEventHandler(orderControl1_SetTargetRecPath);
            this.orderControl1.SetTargetRecPath += new SetTargetRecPathEventHandler(orderControl1_SetTargetRecPath);

            // 2009/11/23 
            this.orderControl1.LoadRecord -= new LoadRecordHandler(entityControl1_LoadRecord111);
            this.orderControl1.LoadRecord += new LoadRecordHandler(entityControl1_LoadRecord111);

            this.orderControl1.VerifyLibraryCode -= new VerifyLibraryCodeEventHandler(orderControl1_VerifyLibraryCode);
            this.orderControl1.VerifyLibraryCode += new VerifyLibraryCodeEventHandler(orderControl1_VerifyLibraryCode);

            this.orderControl1.Channel = this.Channel;
            this.orderControl1.Stop = this.Progress;
            this.orderControl1.MainForm = this.MainForm;

            this.EnableOrdersPage(false);

            // 初始化评注控件
            this.commentControl1.GetMacroValue -= new GetMacroValueHandler(issueControl1_GetMacroValue);
            this.commentControl1.GetMacroValue += new GetMacroValueHandler(issueControl1_GetMacroValue);

            this.commentControl1.ContentChanged -= new ContentChangedEventHandler(issueControl1_ContentChanged);
            this.commentControl1.ContentChanged += new ContentChangedEventHandler(issueControl1_ContentChanged);

            this.commentControl1.EnableControlsEvent -= new EnableControlsHandler(entityControl1_EnableControls);
            this.commentControl1.EnableControlsEvent += new EnableControlsHandler(entityControl1_EnableControls);

            /*
            this.commentControl1.GenerateEntity -= new GenerateEntityEventHandler(orderControl1_GenerateEntity);
            this.commentControl1.GenerateEntity += new GenerateEntityEventHandler(orderControl1_GenerateEntity);

            this.commentControl1.OpenTargetRecord -= new OpenTargetRecordEventHandler(orderControl1_OpenTargetRecord);
            this.commentControl1.OpenTargetRecord += new OpenTargetRecordEventHandler(orderControl1_OpenTargetRecord);

            this.commentControl1.HilightTargetItem -= new HilightTargetItemsEventHandler(orderControl1_HilightTargetItem);
            this.commentControl1.HilightTargetItem += new HilightTargetItemsEventHandler(orderControl1_HilightTargetItem);

            this.commentControl1.SetTargetRecPath -= new SetTargetRecPathEventHandler(orderControl1_SetTargetRecPath);
            this.commentControl1.SetTargetRecPath += new SetTargetRecPathEventHandler(orderControl1_SetTargetRecPath);

            */
            this.commentControl1.LoadRecord -= new LoadRecordHandler(entityControl1_LoadRecord111);
            this.commentControl1.LoadRecord += new LoadRecordHandler(entityControl1_LoadRecord111);

            this.CommentControl.AddSubject -= new AddSubjectEventHandler(CommentControl_AddSubject);
            this.CommentControl.AddSubject += new AddSubjectEventHandler(CommentControl_AddSubject);

            this.commentControl1.Channel = this.Channel;
            this.commentControl1.Stop = this.Progress;
            this.commentControl1.MainForm = this.MainForm;
            // this.commentControl1.WebExternalHost = this.m_webExternalHost_comment;

            this.EnableCommentsPage(false);

            // 初始化对象控件
            if (this.Cataloging == true)
            {

                this.binaryResControl1.ContentChanged -= new ContentChangedEventHandler(binaryResControl1_ContentChanged);
                this.binaryResControl1.ContentChanged += new ContentChangedEventHandler(binaryResControl1_ContentChanged);

                this.binaryResControl1.Channel = this.Channel;
                this.binaryResControl1.Stop = this.Progress;

                this.binaryResControl1.RightsCfgFileName = Path.Combine(this.MainForm.UserDir, "objectrights.xml");

                this.m_macroutil.ParseOneMacro -= new ParseOneMacroEventHandler(m_macroutil_ParseOneMacro);
                this.m_macroutil.ParseOneMacro += new ParseOneMacroEventHandler(m_macroutil_ParseOneMacro);

                // 2009/2/24 
                this.binaryResControl1.GenerateData -= new GenerateDataEventHandler(entityControl1_GenerateData);
                this.binaryResControl1.GenerateData += new GenerateDataEventHandler(entityControl1_GenerateData);

                this.binaryResControl1.TempDir = this.MainForm.UserTempDir;

                LoadFontToMarcEditor();

                this.m_marcEditor.AppInfo = this.MainForm.AppInfo;    // 2009/9/18 
            }



            if (this.AcceptMode == true)
            {
                this.flowLayoutPanel_query.Visible = false;
            }
            else
            {
                this.flowLayoutPanel_query.Visible = this.MainForm.AppInfo.GetBoolean(
"entityform",
"queryPanel_visibie",
true);
            }

            this.panel_itemQuickInput.Visible = this.MainForm.AppInfo.GetBoolean(
"entityform",
"itemQuickInputPanel_visibie",
true);

            // 
            this.EnableControls(true);  // 促使“保存”按钮状态被设置

            // API.PostMessage(this.Handle, WM_LOADLAYOUT, 0, 0);


            // 2008/11/2 
            // RegisterType
            {
                string strRegisterType = this.MainForm.AppInfo.GetString("entity_form",
                    "register_type",
                    "");
                if (String.IsNullOrEmpty(strRegisterType) == false)
                {
                    try
                    {
                        this.RegisterType = (RegisterType)Enum.Parse(typeof(RegisterType), strRegisterType, true);
                    }
                    catch
                    {
                    }
                }
            }

            string strSelectedTemplates = this.MainForm.AppInfo.GetString(
                "entity_form",
                "selected_templates",
                "");
            if (String.IsNullOrEmpty(strSelectedTemplates) == false)
            {
                selected_templates.Build(strSelectedTemplates);
            }

        }

        void entityControl1_ShowMessage(object sender, ShowMessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message) == false)
                this.ShowMessage(e.Message, e.Color, e.ClickClose);
            else
                this.ClearMessage();
        }

        void _genData_SynchronizeMarcEvent(object sender, EventArgs e)
        {
            this.SynchronizeMarc();
        }

        /// <summary>
        /// 重载 MyForm 类型的 OnMyFormLoad() 函数
        /// </summary>
        public override void OnMyFormLoad()
        {
            base.OnMyFormLoad();

            // 2013/6/23 移动到这里
            this.Channel.AfterLogin -= new AfterLoginEventHandle(__Channel_AfterLogin);
            this.Channel.AfterLogin += new AfterLoginEventHandle(__Channel_AfterLogin);
        }

        // 添加自由词，前半部分工作
        void CommentControl_AddSubject(object sender, AddSubjectEventArgs e)
        {
            string strError = "";

            // 根据书目库名获得MARC格式语法名
            // return:
            //      null    没有找到指定的书目库名
            string strMarcSyntax = MainForm.GetBiblioSyntax(this.BiblioDbName);
            if (strMarcSyntax == null)
            {
                strError = "书目库名 '" + this.BiblioDbName + "' 居然没有找到";
                goto ERROR1;
            }

            List<string> reserve_subjects = null;
            List<string> exist_subjects = null;

            int nRet = ItemInfoForm.GetSubjectInfo(this.GetMarc(),  // this.m_marcEditor.Marc,
                strMarcSyntax,
                out reserve_subjects,
                out exist_subjects,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            AddSubjectDialog dlg = new AddSubjectDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ReserveSubjects = reserve_subjects;
            dlg.ExistSubjects = exist_subjects;
            dlg.HiddenNewSubjects = e.HiddenSubjects;
            dlg.NewSubjects = e.NewSubjects;

            this.MainForm.AppInfo.LinkFormState(dlg, "entityform_addsubjectdialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                e.Canceled = true;
                return;
            }

            List<string> subjects = new List<string>();
            subjects.AddRange(dlg.ExistSubjects);
            subjects.AddRange(dlg.NewSubjects);

            StringUtil.RemoveDupNoSort(ref subjects);   // 去重
            StringUtil.RemoveBlank(ref subjects);   // 去掉空元素

            string strMARC = this.GetMarc();    //  this.m_marcEditor.Marc;
            // 修改指示符1为空的那些 610 字段
            // parameters:
            //      strSubject  可以修改的自由词的总和。包括以前存在的和本次添加的
            nRet = ItemInfoForm.ChangeSubject(ref strMARC,
                strMarcSyntax,
                subjects,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            // this.m_marcEditor.Marc = strMARC;
            // this.m_marcEditor.Changed = true;
            this.SetMarc(strMARC);
            this.SetMarcChanged(true);
            return;
        ERROR1:
            e.ErrorInfo = strError;
            if (e.ShowErrorBox == true)
                MessageBox.Show(this, strError);
            e.Canceled = true;
        }

        void orderControl1_VerifyLibraryCode(object sender, VerifyLibraryCodeEventArgs e)
        {
            if (this.Channel == null)
                return;
            if (Global.IsGlobalUser(this.Channel.LibraryCodeList) == true)
                return; // 全局用户不做检查

            List<string> librarycodes = Global.FromLibraryCodeList(e.LibraryCode);

            List<string> outof_librarycodes = new List<string>();
            foreach (string strLibraryCode in librarycodes)
            {
                if (StringUtil.IsInList(strLibraryCode, this.Channel.LibraryCodeList) == false)
                    outof_librarycodes.Add(strLibraryCode);
            }

            if (outof_librarycodes.Count > 0)
            {
                StringUtil.RemoveDupNoSort(ref outof_librarycodes);

                e.ErrorInfo = "馆代码 '"+StringUtil.MakePathList(outof_librarycodes)+"' 不在当前用户的管辖范围 '"+this.Channel.LibraryCodeList+"' 内";
                return;
            }
        }

        void AppInfo_SaveMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 分割条位置
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_recordAndItems,
                    "entity_form",
                    "main_splitter_pos");

                // 当前活动的HTML/MARC page
                string strActivePage = "";

                if (this.tabControl_biblioInfo.SelectedTab == this.tabPage_marc)
                    strActivePage = "marc";
                else if (this.tabControl_biblioInfo.SelectedTab == this.tabPage_html)
                    strActivePage = "html";
                else if (this.tabControl_biblioInfo.SelectedTab == this.tabPage_template)
                    strActivePage = "template";

                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "active_page",
                    strActivePage);

                // 当前活动的册/期/采购/对象 page
                string strActiveItemIssuePage = GetActiveItemPageName();

                // 

                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "active_item_issue_page",
                    strActiveItemIssuePage);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.entityControl1.ListView);
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "item_list_column_width",
                    strWidths);

                strWidths = ListViewUtil.GetColumnWidthListString(this.orderControl1.ListView);
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "order_list_column_width",
                    strWidths);

                strWidths = ListViewUtil.GetColumnWidthListString(this.commentControl1.ListView);
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "comment_list_column_width",
                    strWidths);

                strWidths = ListViewUtil.GetColumnWidthListString(this.issueControl1.ListView);
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "issue_list_column_width",
                    strWidths);

                strWidths = ListViewUtil.GetColumnWidthListString(this.binaryResControl1.ListView);
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "object_list_column_width",
                    strWidths);
            }
        }

        string GetActiveItemPageName()
        {
            string strActiveItemIssuePage = "";

            if (this.tabControl_itemAndIssue.SelectedTab == this.tabPage_item)
                strActiveItemIssuePage = "item";
            else if (this.tabControl_itemAndIssue.SelectedTab == this.tabPage_issue)
                strActiveItemIssuePage = "issue";
            else if (this.tabControl_itemAndIssue.SelectedTab == this.tabPage_object)
                strActiveItemIssuePage = "object";
            else if (this.tabControl_itemAndIssue.SelectedTab == this.tabPage_order)
                strActiveItemIssuePage = "order";

            return strActiveItemIssuePage;
        }

        void AppInfo_LoadMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            // *********** 原来LoadLayout0()的部分

            // 当前活动的HTML/MARC page
            string strActivePage = this.MainForm.AppInfo.GetString(
                "entity_form",
                "active_page",
                "");

            if (String.IsNullOrEmpty(strActivePage) == false)
            {
                if (strActivePage == "marc")
                    this.tabControl_biblioInfo.SelectedTab = this.tabPage_marc;
                else if (strActivePage == "html")
                    this.tabControl_biblioInfo.SelectedTab = this.tabPage_html;
                else if (strActivePage == "template")
                    this.tabControl_biblioInfo.SelectedTab = this.tabPage_template;
            }

            string strActiveItemIssuePage = this.MainForm.AppInfo.GetString(
"entity_form",
"active_item_issue_page",
"");
            if (LoadActiveItemIssuePage(strActiveItemIssuePage) == false)
                this.m_strUsedActiveItemPage = strActiveItemIssuePage;

            // *********** 原来LoadLayout()的部分

            this.MainForm.LoadSplitterPos(
    this.splitContainer_recordAndItems,
    "entity_form",
    "main_splitter_pos");

            string strWidths = this.MainForm.AppInfo.GetString(
                "entity_form",
                "item_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.entityControl1.ListView,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
                "entity_form",
                "order_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.orderControl1.ListView,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
    "entity_form",
    "comment_list_column_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.commentControl1.ListView,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
                "entity_form",
                "issue_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.issueControl1.ListView,
                    strWidths,
                    true);
            }

            strWidths = this.MainForm.AppInfo.GetString(
                "entity_form",
                "object_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.binaryResControl1.ListView,
                    strWidths,
                    true);
            }
        }

        void orderControl1_SetTargetRecPath(object sender, SetTargetRecPathEventArgs e)
        {
            if (e.TargetRecPath == this.BiblioRecPath)
                return;

            // 如果本来就是这个值
            string strOldTargetRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");
            if (strOldTargetRecPath == e.TargetRecPath)
                return;

            if (this.LinkedRecordReadonly == true)
            {
                this.m_marcEditor.Record.Fields.SetFirstSubfield("998", "t", e.TargetRecPath);
                if (String.IsNullOrEmpty(e.TargetRecPath) == false)
                    this.m_marcEditor.ReadOnly = true;
                else
                    this.m_marcEditor.ReadOnly = false;
            }
        }

        /// <summary>
        /// 是否要把 副本书目记录显示为只读状态
        /// </summary>
        public bool LinkedRecordReadonly
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
"entityform",
"linkedRecordReadonly",
true);
            }
        }

        void entityControl1_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this._genData.AutoGenerate(sender, 
                e,
                GetBiblioRecPathOrSyntax());
        }

        /*
        void entityControl1_GenerateAccessNo(object sender, GenerateDataEventArgs e)
        {
            CreateCallNumber(sender, e);
        }*/

        void issueControl1_GenerateEntity(object sender, GenerateEntityEventArgs e)
        {
            orderControl1_GenerateEntity(sender, e);
        }

        // 期控件要获得册信息
        void issueControl1_GetItemInfo(object sender, GetItemInfoEventArgs e)
        {
            string strError = "";

            // 2010/3/26
            this.entityControl1.Items.SetRefID();

            List<string> XmlRecords = null;
            // 根据出版时间，匹配“时间范围”符合的册记录
            int nRet = this.entityControl1.GetItemInfoByPublishTime(e.PublishTime,
                out XmlRecords,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            e.ItemXmls = XmlRecords;
        }

        // 期控件要获得订购信息
        void issueControl1_GetOrderInfo(object sender,
            GetOrderInfoEventArgs e)
        {
            string strError = "";
            List<string> XmlRecords = null;
            // 根据出版时间，匹配“时间范围”符合的订购记录
            int nRet = this.orderControl1.GetOrderInfoByPublishTime(e.PublishTime,
                e.LibraryCodeList,
                out XmlRecords,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            e.OrderXmls = XmlRecords;
        }

        void orderControl1_HilightTargetItem(object sender, HilightTargetItemsEventArgs e)
        {
            this.ActivateItemsPage();
            this.SelectItemsByBatchNo(e.BatchNo, true);
        }

        void orderControl1_OpenTargetRecord(object sender, OpenTargetRecordEventArgs e)
        {
            // 新打开一个EntityForm
            EntityForm form = null;
            int nRet = 0;

            if (e.TargetRecPath == this.BiblioRecPath)
            {
                // 如果涉及当前记录，不必新开EntityForm窗口了，仅仅激活items page就可以了
                form = this;
                Global.Activate(form);
            }
            else
            {
                form = new EntityForm();
                form.MdiParent = this.MdiParent;
                form.MainForm = this.MainForm;
                form.Show();

                nRet = form.LoadRecordOld(e.TargetRecPath, 
                    "",
                    false);
                if (nRet != 1)
                {
                    e.ErrorInfo = "目标书目记录 " + e.TargetRecPath + " 装载失败";
                    return;
                }

            }

            form.ActivateItemsPage();
            form.SelectItemsByBatchNo(e.BatchNo, true);
        }

        // 选定(加亮)items事项中符合指定批次号的那些行
        void SelectItemsByBatchNo(string strAcceptBatchNo,
            bool bClearOthersHilight)
        {
            this.entityControl1.SelectItemsByBatchNo(strAcceptBatchNo,
                bClearOthersHilight);
        }

        // 激活items page
        /// <summary>
        /// 激活册属性页
        /// </summary>
        /// <returns>当前是否存在此属性页</returns>
        public bool ActivateItemsPage()
        {
            if (this.tabControl_itemAndIssue.Contains(this.tabPage_item) == true)
            {
                this.tabControl_itemAndIssue.SelectedTab = this.tabPage_item;
                return true;
            }

            return false;   // not found
        }

        // 激活orders page
        /// <summary>
        /// 激活订购属性页
        /// </summary>
        /// <returns>当前是否存在此属性页</returns>
        public bool ActivateOrdersPage()
        {
            if (this.tabControl_itemAndIssue.Contains(this.tabPage_order) == true)
            {
                this.tabControl_itemAndIssue.SelectedTab = this.tabPage_order;
                return true;
            }

            return false;   // not found
        }

        // 激活comments page
        /// <summary>
        /// 激活评注属性页
        /// </summary>
        /// <returns>当前是否存在此属性页</returns>
        public bool ActivateCommentsPage()
        {
            if (this.tabControl_itemAndIssue.Contains(this.tabPage_comment) == true)
            {
                this.tabControl_itemAndIssue.SelectedTab = this.tabPage_comment;
                return true;
            }

            return false;   // not found
        }

        // 激活issues page
        /// <summary>
        /// 激活期属性页
        /// </summary>
        /// <returns>当前是否存在此属性页</returns>
        public bool ActivateIssuesPage()
        {
            if (this.tabControl_itemAndIssue.Contains(this.tabPage_issue) == true)
            {
                this.tabControl_itemAndIssue.SelectedTab = this.tabPage_issue;
                return true;
            }

            return false;   // not found
        }

        // 2009/12/16 
        // 修改册对象
        void issueControl1_ChangeItem(object sender,
            ChangeItemEventArgs e)
        {
            string strError = "";

            List<InputBookItem> bookitems = new List<InputBookItem>();

            // 创建实体记录
            for (int i = 0; i < e.DataList.Count; i++)
            {
                ChangeItemData data = e.DataList[i];

                BookItem bookitem = null;
                // 外部调用，设置一个实体记录。
                // 具体动作有：new change delete neworchange
                int nRet = this.entityControl1.DoSetEntity(
                    true,
                    data.Action,
                    data.RefID,
                    data.Xml,
                    out bookitem,
                    out strError);
                if (nRet == -1)
                {
                    data.ErrorInfo = strError;
                }
                else if (nRet == 1)
                {
                    data.WarningInfo = strError;
                    data.WarningInfo += "\r\n\r\n不过上述包含重复册条码号的记录已经被成功创建或修改。请留意稍后去消除册条码号重复";
                } 

                if (data.Action == "new"
                    || (bookitem != null && bookitem.ItemDisplayState == ItemDisplayState.New))
                {
                    if (String.IsNullOrEmpty(bookitem.Barcode) == true)
                    {
                        InputBookItem input_bookitem = new InputBookItem();
                        input_bookitem.Sequence = data.Sequence;
                        input_bookitem.BookItem = bookitem;
                        bookitems.Add(input_bookitem);
                    }
                }
            }

            if (bookitems.Count > 0
    && e.InputItemBarcode == true)  // 2009/1/15 
            {
                // 要求输入若干条码
                InputItemBarcodeDialog item_barcode_dlg = new InputItemBarcodeDialog();
                MainForm.SetControlFont(item_barcode_dlg, this.Font, false);

                item_barcode_dlg.AppInfo = this.MainForm.AppInfo;
                item_barcode_dlg.SeriesMode = e.SeriesMode; // 2008/12/27 

                item_barcode_dlg.DetectBarcodeDup -= new DetectBarcodeDupHandler(item_barcode_dlg_DetectBarcodeDup);
                item_barcode_dlg.DetectBarcodeDup += new DetectBarcodeDupHandler(item_barcode_dlg_DetectBarcodeDup);

                item_barcode_dlg.VerifyBarcode -= new VerifyBarcodeHandler(item_barcode_dlg_VerifyBarcode);
                item_barcode_dlg.VerifyBarcode += new VerifyBarcodeHandler(item_barcode_dlg_VerifyBarcode);

                item_barcode_dlg.EntityControl = this.entityControl1;
                item_barcode_dlg.BookItems = bookitems;

                this.MainForm.AppInfo.LinkFormState(item_barcode_dlg, "entityform_inputitembarcodedlg_state");
                item_barcode_dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(item_barcode_dlg);


                if (item_barcode_dlg.DialogResult != DialogResult.OK)
                {
                }
            }

            // TODO: 是否要创建索取号?
            // 为新的册记录创建索取号
            if (e.CreateCallNumber == true && bookitems.Count > 0)
            {
                // 选定新的册记录事项
                List<BookItem> items = new List<BookItem>();
                foreach (InputBookItem input_item in bookitems)
                {
                    items.Add(input_item.BookItem);
                }
                // 在listview中选定指定的事项
                int nRet = this.EntityControl.SelectItems(
                   true,
                   items);
                if (nRet < items.Count)
                {
                    e.ErrorInfo = "SetlectItems()未能选定要求的全部事项";
                    this.ActivateItemsPage();
                    return;
                }

                // 为当前选定的事项创建索取号
                // return:
                //      -1  出错
                //      0   放弃处理
                //      1   已经处理
                nRet = this.EntityControl.CreateCallNumber(
                    false,
                    out strError);
                if (nRet == -1)
                {
                    /*
                    e.ErrorInfo = "创建索取号时发生错误: " + strError;
                    this.ActivateItemsPage();
                    return;
                     * */
                    // 警告性质
                    // 2012/9/1
                    this.ActivateItemsPage();
                    MessageBox.Show(this, "警告：创建索取号时发生错误: " + strError);
                }
            }

        }

        // 验收的时候自动创建实体记录
        void orderControl1_GenerateEntity(object sender, 
            GenerateEntityEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strTargetRecPath = "";
            if (sender is OrderControl)
            {
                Debug.Assert(e.SeriesMode == false, "");
                strTargetRecPath = this.orderControl1.TargetRecPath;
            }
            else if (sender is IssueControl)
            {
                Debug.Assert(e.SeriesMode == true, "");
                strTargetRecPath = this.issueControl1.TargetRecPath;
            }
            else if (sender is IssueManageControl)
            {
                Debug.Assert(e.SeriesMode == true, "");
                strTargetRecPath = this.issueControl1.TargetRecPath;
            }
            else
            {
                Debug.Assert(false, "");
                strTargetRecPath = this.orderControl1.TargetRecPath;
            }


            EntityForm form = null;

            // 4) 这里的路径为空，表示需要通过菜单选择目标库，然后处理方法同3)
            if (String.IsNullOrEmpty(strTargetRecPath) == true)
            {
                string strBiblioRecPath = "";

                if (e.SeriesMode == false)
                {
                    // 图书。
                    
                    // TODO: 如果为工作库，当对话框打开后，缺省选定源库名? 这样会方便了脱离验收窗的实体窗独立验收操作

                    // 根据书目库名获得MARC格式语法名
                    // return:
                    //      null    没有找到指定的书目库名
                    string strCurSyntax = MainForm.GetBiblioSyntax(this.BiblioDbName);
                    if (strCurSyntax == null)
                    {
                        e.ErrorInfo = "书目库名 '" + this.BiblioDbName + "' 居然没有找到";
                        return;
                    }

                    // TODO: 如果可选列表为一个库名，那就最好不必让用户选了?

                    // 获得一个目标库名
                    GetAcceptTargetDbNameDlg dlg = new GetAcceptTargetDbNameDlg();
                    MainForm.SetControlFont(dlg, this.Font, false);
                    dlg.AutoFinish = true;
                    dlg.SeriesMode = e.SeriesMode;
                    dlg.MainForm = this.MainForm;
                    dlg.DbName = this.BiblioDbName;
                    // 根据当前所在的库的marc syntax限制一下目标库的范围
                    dlg.MarcSyntax = strCurSyntax;

                    dlg.ShowDialog(this);

                    if (dlg.DialogResult != DialogResult.OK)
                    {
                        e.ErrorInfo = "放弃创建实体记录";
                        return;
                    }

                    // 如果目标库名和当前窗口的书目记录路径中的库名相同，则意味着目标记录就是当前记录，而不必新建记录
                    if (dlg.DbName == this.BiblioDbName)
                    {
                        strBiblioRecPath = this.BiblioRecPath;
                    }
                    else
                    {
                        strBiblioRecPath = dlg.DbName + "/?";
                    }
                }
                else
                {
                    // 2009/11/9 
                    // 期刊。禁止验收到其他记录的能力。直接验收到源记录。
                    strBiblioRecPath = this.BiblioRecPath;
                }

                // 新打开一个EntityForm
                if (strBiblioRecPath == this.BiblioRecPath)
                {
                    // 如果涉及当前记录，不必新开EntityForm窗口了
                    form = this;
                }
                else
                {
                    form = new EntityForm();
                    form.MdiParent = this.MdiParent;
                    form.MainForm = this.MainForm;
                    form.Show();

                    // 设置MARC记录
                    // ??? e.BiblioRecord 
                    form.m_marcEditor.Marc = this.GetMarc();    //  this.m_marcEditor.Marc;

                    form.BiblioRecPath = strBiblioRecPath;
                }

                form.EnableItemsPage(true);

                // TODO: 在创建实体记录过程中，是否允许立即输入册条码号?
                // 输入册条码号的同时，要醒目显示条码所对应的馆藏地点，以便工作人员分类摆放图书
                // 也建议dp2Circulation提供一个通过扫册条码快速观察馆藏地点的功能窗口

                goto CREATE_ENTITY;
            }

            // 3) 这里的路径仅有库名部分，表示种记录不存在，需要根据当前记录的MARC来创建；
            /*
            string strID = Global.GetID(this.TargetRecPath);
            if (String.IsNullOrEmpty(strID) == true
                || strID == "?")
             * */
            if (Global.IsAppendRecPath(this.TargetRecPath) == true)   // 2008/12/3 
            {
                string strDbName = Global.GetDbName(strTargetRecPath);

                // 路径全为空的情况已经走到前面的分支内了，不会走到这里
                Debug.Assert(String.IsNullOrEmpty(strDbName) == false, "路径全为空的情况已经走到前面的分支内了，不会走到这里");

                // TODO: 需要检查一下strDbName中的数据库名是否确实为目标库

                string strBiblioRecPath = "";

                // 如果目标库名和当前窗口的书目记录路径中的库名相同，则意味着目标记录就是当前记录，而不必新建记录
                if (strDbName == this.BiblioDbName)
                {
                    strBiblioRecPath = this.BiblioRecPath;
                }
                else
                {
                    strBiblioRecPath = strDbName + "/?";
                }

                // 新打开一个EntityForm
                if (strBiblioRecPath == this.BiblioRecPath)
                {
                    // 如果涉及当前记录，不必新开EntityForm窗口了
                    form = this;
                }
                else
                {
                    form = new EntityForm();
                    form.MdiParent = this.MdiParent;
                    form.MainForm = this.MainForm;
                    form.Show();
                }

                // 设置MARC记录
                form.m_marcEditor.Marc = this.GetMarc();    //  this.m_marcEditor.Marc;
                form.BiblioRecPath = strBiblioRecPath;
                form.EnableItemsPage(true);


                goto CREATE_ENTITY;
            }

            // 1)这里的路径和当前记录路径一致，表明实体记录就创建在当前记录下；
            if (this.entityControl1.BiblioRecPath == strTargetRecPath)
            {

                // 不要求保存
                form = this;
                goto CREATE_ENTITY;
            }


            // 2) 目标记录路径和当前记录路径不一致，不过目标种记录已经存在，需要在它下面创建实体记录；

            {
                Debug.Assert(strTargetRecPath != this.BiblioRecPath, "新开窗口，必须不涉及当前书目记录");

                Debug.Assert(form == null, "");

                // 新打开一个EntityForm
                form = new EntityForm();
                form.MdiParent = this.MdiParent;
                form.MainForm = this.MainForm;
                form.Show();

                nRet = form.LoadRecordOld(strTargetRecPath,
                    "",
                    false);
                if (nRet != 1)
                {
                    e.ErrorInfo = "目标书目记录 " +strTargetRecPath+ " 装载失败";
                    return;
                }

                // items page自然会被显示出来

                goto CREATE_ENTITY;
            }

        CREATE_ENTITY:

            Debug.Assert(form != null, "");

            List<InputBookItem> bookitems = new List<InputBookItem>();

            // 创建实体记录
            for (int i = 0; i < e.DataList.Count; i++)
            {
                GenerateEntityData data = e.DataList[i];

                BookItem bookitem = null;
                // 外部调用，设置一个实体记录。
                // 具体动作有：new change delete
                nRet = form.entityControl1.DoSetEntity(
                    false,
                    data.Action,
                    data.RefID,
                    data.Xml,
                    out bookitem,
                    out strError);
                if (nRet == -1 || nRet == 1)
                {
                    Debug.Assert(nRet != 1, "");
                    data.ErrorInfo = strError;
                }

                if (data.Action == "new")
                {
                    InputBookItem input_bookitem = new InputBookItem();
                    input_bookitem.Sequence = data.Sequence;
                    input_bookitem.OtherPrices = data.OtherPrices;
                    input_bookitem.BookItem = bookitem;
                    bookitems.Add(input_bookitem);
                }
            }

            if (bookitems.Count > 0
                && e.InputItemBarcode == true)  // 2009/1/15 
            {
                // 要求输入若干条码
                InputItemBarcodeDialog item_barcode_dlg = new InputItemBarcodeDialog();
                MainForm.SetControlFont(item_barcode_dlg, this.Font, false);

                item_barcode_dlg.AppInfo = this.MainForm.AppInfo;
                item_barcode_dlg.SeriesMode = e.SeriesMode; // 2008/12/27 

                item_barcode_dlg.DetectBarcodeDup -= new DetectBarcodeDupHandler(item_barcode_dlg_DetectBarcodeDup);
                item_barcode_dlg.DetectBarcodeDup += new DetectBarcodeDupHandler(item_barcode_dlg_DetectBarcodeDup);

                item_barcode_dlg.VerifyBarcode -= new VerifyBarcodeHandler(item_barcode_dlg_VerifyBarcode);
                item_barcode_dlg.VerifyBarcode += new VerifyBarcodeHandler(item_barcode_dlg_VerifyBarcode);

                item_barcode_dlg.EntityControl = form.entityControl1;
                item_barcode_dlg.BookItems = bookitems;

                this.MainForm.AppInfo.LinkFormState(item_barcode_dlg, "entityform_inputitembarcodedlg_state");
                item_barcode_dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(item_barcode_dlg);


                if (item_barcode_dlg.DialogResult != DialogResult.OK)
                {
                }
            }

            // ??
            // 将最终保存后获得的书目记录路径记载到TargetRecPath中
            strTargetRecPath = form.BiblioRecPath;
            if (sender is OrderControl)
                this.orderControl1.TargetRecPath = strTargetRecPath;
            else if (sender is IssueControl)
                this.issueControl1.TargetRecPath = strTargetRecPath;
            else if (sender is IssueManageControl)
                this.issueControl1.TargetRecPath = strTargetRecPath;

            // 设置MARC记录
            if (String.IsNullOrEmpty(e.BiblioRecord) == false)
            {
                Debug.Assert(e.BiblioSyntax == "unimarc" 
                    || e.BiblioSyntax == "usmarc"
                    || e.BiblioSyntax == "marc"
                    || e.BiblioSyntax == "xml",
                    "");
                nRet = form.ImportRecordString(e.BiblioSyntax,
                    e.BiblioRecord,
                    out strError);
                if (nRet == -1)
                {
                    e.ErrorInfo = strError;
                    return;
                }
            }

            // 为新的册记录创建索取号
            if (e.CreateCallNumber == true && bookitems.Count > 0)
            {
                // 选定新的册记录事项
                List<BookItem> items = new List<BookItem>();
                foreach (InputBookItem input_item in bookitems)
                {
                    items.Add(input_item.BookItem);
                }
                // 在listview中选定指定的事项
                nRet = form.EntityControl.SelectItems(
                   true,
                   items);
                if (nRet < items.Count)
                {
                    e.ErrorInfo = "SetlectItems()未能选定要求的全部事项";
                    form.ActivateItemsPage();
                    return;
                }

                // 为当前选定的事项创建索取号
                // return:
                //      -1  出错
                //      0   放弃处理
                //      1   已经处理
                nRet = form.EntityControl.CreateCallNumber(
                    false,
                    out strError);
                if (nRet == -1)
                {
                    /*
                    e.ErrorInfo = "创建索取号时发生错误: " + strError;
                    form.ActivateItemsPage();
                    return;
                     * */
                    // 警告性质
                    // 2012/9/1
                    this.ActivateItemsPage();
                    MessageBox.Show(this, "警告：创建索取号时发生错误: " + strError);
                }
            }


            // 保存整个记录?
            if (this != form)
            {
                // 提交所有保存请求
                // return:
                //      -1  有错。此时不排除有些信息保存成功。
                //      0   成功。
                nRet = form.DoSaveAll();
                e.TargetRecPath = form.BiblioRecPath;

                if (form.HasCommentPage == true && form.CommentControl != null
                    && this.HasCommentPage == true && this.CommentControl != null
                    && this.CommentControl.Items.Count > 0)
                {
                    // 移动、归并评注记录
                    // 改变归属
                    // 即修改实体信息的<parent>元素内容，使指向另外一条书目记录
                    // parameters:
                    //      items   要改变归属的事项集合。如果为 null，表示全部改变归属
                    nRet = this.CommentControl.ChangeParent(null,
                        form.BiblioRecPath,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "警告：移动评注记录(" + this.BiblioRecPath + " --> " + form.BiblioRecPath + ")时发生错误: " + strError);

                    // 重新装载评注属性页
                    nRet = form.CommentControl.LoadItemRecords(form.BiblioRecPath,
                        "",
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, "警告：重新装载书目记录 " + form.BiblioRecPath + " 的下属评注记录时发生错误: " + strError);

                }
            }
            else
            {
                e.TargetRecPath = form.BiblioRecPath;
            }

            // 触发提示通知推荐过的读者
            if (form.HostObject != null)
            {
                AfterCreateItemsArgs e1 = new AfterCreateItemsArgs();
                e1.Case = "accept";
                // form.HostObject.AfterCreateItems(this, e1);
                form.HostObject.Invoke("AfterCreateItems", this, e1);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    MessageBox.Show(this, "验收中创建册记录的延续工作(AfterCreateItems)失败: " + strError + "\r\n\r\n但保存操作已经成功");
                }
            }

            // 2013/12/2 移动到这里
            if (this != form)
            {
                form.Close();
            }

            return;
        }

        /*
        // 导入新的MARC字符串，但是保持原来的998字段
        void ImportMarcString(string strMarc)
        {
            Field old_998 = null;

            // 保存当前记录的998字段
            old_998 = this.MarcEditor.Record.Fields.GetOneField("998", 0);

            this.MarcEditor.Marc = strMarc;

            if (old_998 != null)
            {
                // 恢复先前的998字段内容
                for (int i = 0; i < this.MarcEditor.Record.Fields.Count; i++)
                {
                    Field temp = this.MarcEditor.Record.Fields[i];
                    if (temp.Name == "998")
                    {
                        this.MarcEditor.Record.Fields.RemoveAt(i);
                        i--;
                    }
                }
                this.MarcEditor.Record.Fields.Insert(this.MarcEditor.Record.Fields.Count,
                    old_998.Name,
                    old_998.Indicator,
                    old_998.Value);
            }
        }
         * */

        // 导入新的MARC/XML字符串，但是保持原来的998字段
        int ImportRecordString(
            string strSyntax,
            string strRecord,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            Field old_998 = null;

            // 保存当前记录的998字段
            old_998 = this.m_marcEditor.Record.Fields.GetOneField("998", 0);

            if (strSyntax == "xml")
            {
                nRet = this.SetBiblioRecordToMarcEditor(strRecord, out strError);
            }
            else if (strSyntax == "marc" || strSyntax == "unimarc" || strSyntax == "usmarc")
            {
                // this.m_marcEditor.Marc = strRecord;
                this.SetMarc(strRecord);
            }

            if (nRet == -1)
                return -1;

            if (old_998 != null)
            {
                // 恢复先前的998字段内容
                for (int i = 0; i < this.m_marcEditor.Record.Fields.Count; i++)
                {
                    Field temp = this.m_marcEditor.Record.Fields[i];
                    if (temp.Name == "998")
                    {
                        this.m_marcEditor.Record.Fields.RemoveAt(i);
                        i--;
                    }
                }
                this.m_marcEditor.Record.Fields.Insert(this.m_marcEditor.Record.Fields.Count,
                    old_998.Name,
                    old_998.Indicator,
                    old_998.Value);
            }

            return 0;
        }

        // 条码输入对话框请求校验条码
        void item_barcode_dlg_VerifyBarcode(object sender, VerifyBarcodeEventArgs e)
        {
            string strError = "";
            e.Result = this.VerifyBarcode(
                this.Channel.LibraryCodeList,
                e.Barcode,
                out strError);
            e.ErrorInfo = strError;
        }

        void item_barcode_dlg_DetectBarcodeDup(object sender, DetectBarcodeDupEventArgs e)
        {
            int nRet = 0;
            string strError = "";

            // 对一批事项的条码查重
            // return:
            //      -1  出错
            //      0   不重复
            //      1   重复
            nRet = e.EntityControl.CheckBarcodeDup(
                e.BookItems,
                out strError);
            e.Result = nRet;
            e.ErrorInfo = strError;
        }

        void m_macroutil_ParseOneMacro(object sender, ParseOneMacroEventArgs e)
        {
            // string strError = "";
            string strName = Unquote(e.Macro);  // 去掉百分号

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

            if (strName == "username")
            {
                e.Value = this.Channel.UserName;
                return;
            }

            string strValue = "";
            string strError = "";
            // 从marceditor_macrotable.xml文件中解析宏
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = MacroUtil.GetFromLocalMacroTable(
                Path.Combine(this.MainForm.DataDir, "marceditor_macrotable.xml"),
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

            if (String.Compare(strFuncName, "IncSeed", true) == 0
                || String.Compare(strFuncName, "IncSeed+", true) == 0
                || String.Compare(strFuncName, "+IncSeed", true) == 0)
            {
                // 种次号库名, 指标名, 要填充到的位数
                string[] aParam = strParams.Split(new char[] { ',' });
                if (aParam.Length != 3 && aParam.Length != 2)
                {
                    strError = "IncSeed需要2或3个参数。";
                    goto ERROR1;
                }

                bool IncAfter = false;  // 是否为先取后加
                if (strFuncName[strFuncName.Length - 1] == '+')
                    IncAfter = true;

                string strZhongcihaoDbName = aParam[0].Trim();
                string strEntryName = aParam[1].Trim();
                strValue = "";

                long lRet = 0;
                if (e.Simulate == true)
                {
                    // parameters:
                    //      strZhongcihaoGroupName  @引导种次号库名 !引导线索书目库名 否则就是 种次号组名
                    lRet = Channel.GetZhongcihaoTailNumber(
    null,
    strZhongcihaoDbName,
    strEntryName,
    out strValue,
    out strError);
                    if (lRet == -1)
                        goto ERROR1; 
                    if (string.IsNullOrEmpty(strValue) == true)
                    {
                        strValue = "1";
                    }
                }
                else
                {
                    // parameters:
                    //      strZhongcihaoGroupName  @引导种次号库名 !引导线索书目库名 否则就是 种次号组名
                    lRet = this.Channel.SetZhongcihaoTailNumber(
    null,
    IncAfter == true ? "increase+" : "increase",
    strZhongcihaoDbName,
    strEntryName,
    "1",
    out strValue,
    out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }

                // 补足左方'0'
                if (aParam.Length == 3)
                {
                    int nWidth = 0;
                    try
                    {
                        nWidth = Convert.ToInt32(aParam[2]);
                    }
                    catch
                    {
                        strError = "第三参数应当为纯数字（表示补足的宽度）";
                        goto ERROR1;
                    }
                    e.Value = strValue.PadLeft(nWidth, '0');
                }
                else
                    e.Value = strValue;
                return;
            }

            e.Canceled = true;  // 不能解释处理
            return;
        ERROR1:
            e.Canceled = true;
            e.ErrorInfo = strError;
        }

        static string Unquote(string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            if (strValue[0] == '%')
                strValue = strValue.Substring(1);
            if (strValue.Length == 0)
                return "";
            if (strValue[strValue.Length - 1] == '%')
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }

        void binaryResControl1_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            SetSaveAllButtonState(true);
        }

        void entityControl1_LoadRecord111(object sender, LoadRecordEventArgs e)
        {
            e.Result = this.LoadRecordOld(e.BiblioRecPath,
                "",
                false);
        }

        void entityControl1_EnableControls(object sender, EnableControlsEventArgs e)
        {
            if (this.m_nInDisable == 0)
                this.EnableControls(e.bEnable);
        }

        // 册控件请求校验条码
        void entityControl1_VerifyBarcode(object sender, VerifyBarcodeEventArgs e)
        {
            string strError = "";
            e.Result = this.VerifyBarcode(
                this.Channel.LibraryCodeList,
                e.Barcode,
                out strError);
            e.ErrorInfo = strError;
        }

        // 册控件询问参数值
        void entityControl1_GetParameterValue(object sender, GetParameterValueEventArgs e)
        {
            if (e.Name == "NeedVerifyItemBarcode")
            {
                e.Value = this.NeedVerifyItemBarcode == true ? "true" : "false";
            }
        }

        void issueControl1_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            SetSaveAllButtonState(true);
        }

        void issueControl1_GetMacroValue(object sender, GetMacroValueEventArgs e)
        {
            e.MacroValue = this.GetMacroValue(e.MacroName);
        }

#if NOOOOOOOOOOOOO
        // 装载布局。不需要异步的部分
        void LoadLayout0()
        {
            if (this.AcceptMode == false)
            {
                // 设置窗口尺寸状态
                MainForm.AppInfo.LoadMdiChildFormStates(this,
                    "mdi_form_state");
            }



            // 当前活动的HTML/MARC page
            string strActivePage = this.MainForm.AppInfo.GetString(
                "entity_form",
                "active_page",
                "");

            if (String.IsNullOrEmpty(strActivePage) == false)
            {
                if (strActivePage == "marc")
                    this.tabControl_biblioInfo.SelectedTab = this.tabPage_marc;
                else if (strActivePage == "html")
                    this.tabControl_biblioInfo.SelectedTab = this.tabPage_html;
            }

            LoadActiveItemIssuePage();
        }
#endif

        string m_strUsedActiveItemPage = ""; // 是否延了设置属性页? 如果是，等装载记录的时候需要再次兑现

        bool LoadActiveItemIssuePage(string strActiveItemIssuePage)
        {

            if (this.AcceptMode == true)
            {
                // 按照优先顺序，激活order page / item page
                if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_issue) != -1)
                    this.tabControl_itemAndIssue.SelectedTab = this.tabPage_issue;
                else if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_order) != -1)
                    this.tabControl_itemAndIssue.SelectedTab = this.tabPage_order;
                else if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_item) != -1)
                    this.tabControl_itemAndIssue.SelectedTab = this.tabPage_item;
                else if (this.tabControl_itemAndIssue.TabPages.Count > 0)
                    this.tabControl_itemAndIssue.SelectedTab = this.tabControl_itemAndIssue.TabPages[this.tabControl_itemAndIssue.TabPages.Count - 1];    // 最靠后的一个page
                return true;
            }

            // 当前活动的册/期 page
            if (strActiveItemIssuePage == null)
            {
                strActiveItemIssuePage = this.MainForm.AppInfo.GetString(
        "entity_form",
        "active_item_issue_page",
        "");
            }

            if (String.IsNullOrEmpty(strActiveItemIssuePage) == false)
            {
                if (strActiveItemIssuePage == "item")
                {
                    if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_item) != -1)
                    {
                        this.tabControl_itemAndIssue.SelectedTab = this.tabPage_item;
                        return true;
                    }
                }
                else if (strActiveItemIssuePage == "order")
                {
                    if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_order) != -1)
                    {
                        this.tabControl_itemAndIssue.SelectedTab = this.tabPage_order;
                        return true;
                    }
                }
                else if (strActiveItemIssuePage == "issue")
                {
                    if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_issue) != -1)
                    {
                        this.tabControl_itemAndIssue.SelectedTab = this.tabPage_issue;
                        return true;
                    }
                }
                else if (strActiveItemIssuePage == "object")
                {
                    if (this.tabControl_itemAndIssue.TabPages.IndexOf(this.tabPage_object) != -1)
                    {
                        this.tabControl_itemAndIssue.SelectedTab = this.tabPage_object;
                        return true;
                    }
                }

            }

            if (this.tabControl_itemAndIssue.TabPages.Count > 0)
                this.tabControl_itemAndIssue.SelectedTab = this.tabControl_itemAndIssue.TabPages[this.tabControl_itemAndIssue.TabPages.Count-1];    // 最靠后的一个page

            return false;
        }

        // 获得当前有修改标志的部分的名称
        string GetCurrentChangedPartName()
        {
            string strPart = "";

            if (this.BiblioChanged == true)
                strPart += "书目信息";

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

            if (this.OrdersChanged == true)
            {
                if (strPart != "")
                    strPart += "和";
                strPart += "采购信息";
            }

            if (this.CommentsChanged == true)
            {
                if (strPart != "")
                    strPart += "和";
                strPart += "评注信息";
            } 
            
            return strPart;
        }

        private void EntityForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (Stop != null)
            {
                if (Stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
#endif
            if (this.browseWindow != null)
            {
            // 避免用 MyForm 的警告机制导致 MessageBox 被 TopMost 状态的小浏览窗遮住无法操作
                if (Progress != null && Progress.State == 0)    // 0 表示正在处理
                {
                    Progress.DoStop();
                    e.Cancel = true;
                    return;
                }
                CloseBrowseWindow();
                e.Cancel = true;
                this.ShowMessage("浏览小窗已经关闭。再关闭一次可关闭种册窗", "yellow", true);
                return;
            }

            if (this.EntitiesChanged == true
                || this.IssuesChanged == true
                || this.BiblioChanged == true
                || this.ObjectChanged == true
                || this.OrdersChanged == true
                || this.CommentsChanged == true)
            {

                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "EntityForm",
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

        private void EntityForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (Stop != null) // 脱离关联
            {
                Stop.Unregister();	// 和容器关联
                Stop = null;
            }
#endif

            /*
            if (m_scriptDomain != null)
            {
                AppDomain.Unload(m_scriptDomain);
                m_scriptDomain = null;
            }
             * */

            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Destroy();
            /*
            if (this.m_webExternalHost_comment != null)
                this.m_webExternalHost_comment.Destroy();
             * */

            if (this.m_verifyViewer != null)
                this.m_verifyViewer.Close();

            if (this.m_commentViewer != null)
                this.m_commentViewer.Close();

#if NO
            if (this.browseWindow != null)
                this.browseWindow.Close();
#endif
            CloseBrowseWindow();

            if (this._genData != null)
            {
                this._genData.SynchronizeMarcEvent -= _genData_SynchronizeMarcEvent;
                this._genData.Close();
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 保存检索途径
                this.MainForm.AppInfo.SetString(
                    "entityform",
                    "search_from",
                    this.comboBox_from.Text);

                this.MainForm.AppInfo.SetString(
                    "entityform",
                    "search_dbnames",
                    this.checkedComboBox_biblioDbNames.Text);

                this.MainForm.AppInfo.SetString(
                    "entityform",
                    "search_matchstyle",
                    this.comboBox_matchStyle.Text);

                /*
                // 2008/6/25 
                this.MainForm.AppInfo.SetBoolean(
                    "entityform",
                    "auto_detect_query_barcode",
                    this.checkBox_autoDetectQueryBarcode.Checked);
                 * */

                this.MainForm.AppInfo.SetBoolean(
                    "entityform",
                    "auto_save_prev",
                    this.checkBox_autoSavePrev.Checked);

                // 2008/11/2 
                // RegisterType
                this.MainForm.AppInfo.SetString("entity_form",
                    "register_type",
                    this.RegisterType.ToString());

                string strSelectedTemplates = selected_templates.Export();
                this.MainForm.AppInfo.SetString(
                    "entity_form",
                    "selected_templates",
                    strSelectedTemplates);

                // 2010/4/27
                this.MainForm.AppInfo.SetBoolean("entity_form",
                    "issueControl_input_item_barcode",
                    this.issueControl1.InputItemsBarcode);
                this.MainForm.AppInfo.SetBoolean(
        "entity_form",
        "issueControl_set_processing_state",
        this.issueControl1.SetProcessingState);
                // 2012/5/7
                this.MainForm.AppInfo.SetBoolean(
                    "entity_form",
                    "create_callnumber",
                    this.issueControl1.CreateCallNumber);

                // SaveLayout();
                if (this.AcceptMode == false)
                {
#if NO
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");
#endif
                }
                else
                {
                    Form form = this;
                    FormWindowState savestate = form.WindowState;
                    bool bStateChanged = false;
                    if (form.WindowState != FormWindowState.Normal)
                    {
                        form.WindowState = FormWindowState.Normal;
                        bStateChanged = true;
                    }

                    AppInfo_SaveMdiSize(this, null);

                    if (bStateChanged == true)
                        form.WindowState = savestate;
                }

                this.MainForm.AppInfo.LoadMdiSize -= new EventHandler(AppInfo_LoadMdiSize);
                this.MainForm.AppInfo.SaveMdiSize -= new EventHandler(AppInfo_SaveMdiSize);
            }

            if (this.easyMarcControl1 != null)
            {
                // 放在一起 Dispose 速度就快了！
                List<Control> controls = this.easyMarcControl1.Clear(false);
                foreach (Control control in controls)
                {
                    if (control != null)
                        control.Dispose();
                }

                //this.easyMarcControl1.Clear(true);
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
            }
        }

        void __Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            this.commentControl1.SetLibraryCodeFilter(this.Channel.LibraryCodeList);
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

        // 
        /// <summary>
        /// 实体信息是否被改变
        /// </summary>
        public bool EntitiesChanged
        {
            get
            {
                if (this.entityControl1 != null)
                    return this.entityControl1.Changed;

                return false;
            }
            set
            {
                if (this.entityControl1 != null)
                    this.entityControl1.Changed = value;
            }

        }

        // 
        /// <summary>
        /// 期信息是否被改变
        /// </summary>
        public bool IssuesChanged
        {
            get
            {
                if (this.issueControl1 != null)
                    return this.issueControl1.Changed;

                return false;
            }
            set
            {
                if (this.issueControl1 != null)
                    this.issueControl1.Changed = value;
            }
        }

        // 
        /// <summary>
        /// 采购信息是否被改变
        /// </summary>
        public bool OrdersChanged
        {
            get
            {
                if (this.orderControl1 != null)
                    return this.orderControl1.Changed;

                return false;
            }
            set
            {
                if (this.orderControl1 != null)
                    this.orderControl1.Changed = value;
            }
        }

        // 
        /// <summary>
        /// 评注信息是否被改变
        /// </summary>
        public bool CommentsChanged
        {
            get
            {
                if (this.commentControl1 != null)
                    return this.commentControl1.Changed;

                return false;
            }
            set
            {
                if (this.commentControl1 != null)
                    this.commentControl1.Changed = value;
            }
        }

        // 2015/8/12
        public string MarcSyntax
        {
            get;
            set;
        }

        // 
        /// <summary>
        /// 书目信息是否被改变
        /// </summary>
        public bool BiblioChanged
        {
            get
            {
                if (this.m_marcEditor != null)
                {
                    // 如果object id有所改变，那么即便MARC没有改变，那最后的合成XML也发生了改变
                    if (this.binaryResControl1 != null)
                    {
                        if (this.binaryResControl1.IsIdUsageChanged() == true)
                            return true;
                    }

                    // return this.m_marcEditor.Changed;
                    return this.GetMarcChanged();
                }

                return false;
            }
            set
            {
                if (this.Cataloging == false)
                {
                    if (value == true)
                    {
                        throw new Exception("当前不允许编目功能，因此不能对BiblioChanged设置true值");
                    }
                }

                if (this.m_marcEditor != null)
                {
                    // this.m_marcEditor.Changed = value;
                    this.SetMarcChanged(value);
                }

                // ****
                toolStripButton_marcEditor_save.Enabled = value;
            }
        }

        // return:
        //      -1  出错
        //      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel；或者“到头”“到尾”)
        //      1   成功装载
        //      2   通道被占用
        /// <summary>
        /// 可靠装载记录
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strPrevNextStyle">前后翻动风格</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel；或者“到头”“到尾”)
        ///      1   成功装载
        ///      2   通道被占用
        /// </returns>
        public int SafeLoadRecord(string strBiblioRecPath,
            string strPrevNextStyle)
        {
            string strError = "";
            int nRet = LoadRecord(strBiblioRecPath,
                strPrevNextStyle,
                true,
                false,
                out strError);
            if (nRet == 2)
            {
                this.AddToPendingList(strBiblioRecPath, strPrevNextStyle);
            }

            return nRet;
        }

        /// <summary>
        /// 重新装载当前记录
        /// </summary>
        public void Reload()
        {
            string strError = "";
            int nRet = this.LoadRecord(this.BiblioRecPath,
    "",
    true,
    true,
    out strError,
    true);
            if (nRet == -1 /*|| string.IsNullOrEmpty(strError) == false*/)
                MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 装载记录，从 BiblioInfo 对象
        /// </summary>
        /// <param name="info"></param>
        /// <param name="bSetFocus"></param>
        /// <param name="strTotalError"></param>
        /// <param name="bWarningNotSave"></param>
        /// <returns>
        ///      -1  出错
        ///      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel；或者“到头”“到尾”)
        ///      1   成功装载
        ///      2   通道被占用
        /// </returns>
        public int LoadRecord(BiblioInfo info,
            bool bSetFocus,
            out string strTotalError,
            bool bWarningNotSave = false)
        {
            strTotalError = "";

            int nRet = 0;
            if (this.EntitiesChanged == true
    || this.IssuesChanged == true
    || this.BiblioChanged == true
    || this.ObjectChanged == true
    || this.OrdersChanged == true
    || this.CommentsChanged == true)
            {
                if (this.checkBox_autoSavePrev.Checked == true
                    && bWarningNotSave == false)
                {
                    nRet = this.DoSaveAll();
                    if (nRet == -1)
                    {
                        // strTotalError = "当前记录尚未保存";  // 2014/7/8
                        return -1;
                    }
                }
                else
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装入新内容? ",
                        "EntityForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return 0;
                }
            }

            // 2012/7/25 移动到这里
            // 因为 LoadBiblioRecord() 会导致填充AutoGen菜单
            this._genData.ClearViewer();

            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();

            // 清空4个下属记录的控件
            this.entityControl1.ClearItems();
            this.textBox_itemBarcode.Text = "";

            this.issueControl1.ClearItems();
            this.orderControl1.ClearItems();
            this.commentControl1.ClearItems();
            this.binaryResControl1.Clear();

            this.EnableItemsPage(false);
            this.EnableIssuesPage(false);
            this.EnableOrdersPage(false);
            this.EnableCommentsPage(false);


            if (this.m_verifyViewer != null)
                this.m_verifyViewer.Clear();

            this.m_webExternalHost_biblio.SetHtmlString("(空白)", "entityform_error");

            string strXml = "";
            if (string.IsNullOrEmpty(info.NewXml) == false)
                strXml = info.NewXml;
            else
                strXml = info.OldXml;

            string strError = "";
            bool bError = false;

            // return:
            //      -1  error
            //      0   空的记录
            //      1   成功
            nRet = SetBiblioRecordToMarcEditor(strXml,
                out strError);
            if (nRet == -1)
            {
                if (String.IsNullOrEmpty(strTotalError) == false)
                    strTotalError += "\r\n";
                strTotalError += strError;

                bError = true;
            }

            this.BiblioTimestamp = info.Timestamp;
            this.BiblioRecPath = "";    // info.RecPath; 

            // 显示Ctrl+A菜单
            if (this.MainForm.PanelFixedVisible == true)
                this._genData.AutoGenerate(this.m_marcEditor,
                    new GenerateDataEventArgs(),
                    "format:" + this.MarcSyntax,
                    true);

            this.BiblioChanged = false;

            // 装载书目和<dprms:file>以外的其它XML片断
            if (string.IsNullOrEmpty(strXml) == false)
            {
                nRet = LoadXmlFragment(strXml,
                    out strError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strTotalError) == false)
                        strTotalError += "\r\n";
                    strTotalError += strError;

                    bError = true;
                }
            }

            this.DeletedMode = false;

            if (bError == true)
            {
                this.ShowMessage(strTotalError, "red", true);
                return -1;
            }

            this.ShowMessage("书目记录来自\r\n" + info.RecPath, "green", true);

            if (m_strFocusedPart == "marceditor"
                && bSetFocus == true)
            {
                SwitchFocus(MARC_EDITOR);
            }

            DoViewComment(false);
            return 1;
        }

        // 兼容以前习惯的版本
        // return:
        //      -1  出错。已经用MessageBox报错
        //      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel)
        //      1   成功装载
        //      2   通道被占用
        /// <summary>
        /// 装载记录。旧版本
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strPrevNextStyle">前后翻动风格</param>
        /// <param name="bCheckInUse">是否检查通道占用情况</param>
        /// <returns>
        ///      -1  出错。已经用MessageBox报错
        ///      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel)
        ///      1   成功装载
        ///      2   通道被占用
        /// </returns>
        public int LoadRecordOld(string strBiblioRecPath,
            string strPrevNextStyle,
            bool bCheckInUse)
        {
            string strError = "";
            int nRet = LoadRecord(strBiblioRecPath,
                strPrevNextStyle,
                bCheckInUse,
                true,
                out strError);
            if (nRet == -1 || nRet == 2)
            {
                if (String.IsNullOrEmpty(strError) == false)
                    MessageBox.Show(this, strError);
            }
            else
            {
                if (String.IsNullOrEmpty(strError) == false)
                    MessageBox.Show(this, strError);
            }

            return nRet;
        }

        // TODO: 有这样一种情况：虽然书目记录和下属的记录都不存在，但是窗口内容被改变了，已然不是以前的内容。如果这时保存记录，会有意外发生，例如本来就有的册信息被清空了。
        // 要想办法在这种情况下保持窗口内全部信息不变；或者，既然已经改变，索性把MARC窗内的记录全部清除，书目记录路径也清楚，避免误会
        // parameters:
        //      bWarningNotSave 是否警告尚未保存？如果==false，并且“自动保存”checkbox为true，会自动保存，不警告
        //      bSetFocus   装载完成后是否把焦点切换到MarcEditor上
        // return:
        //      -1  出错
        //      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel；或者“到头”“到尾”)
        //      1   成功装载
        //      2   通道被占用
        /// <summary>
        /// 装载记录
        /// </summary>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strPrevNextStyle">前后翻动风格</param>
        /// <param name="bCheckInUse">是否检查通道占用情况</param>
        /// <param name="bSetFocus">装载完成后是否把焦点切换到MarcEditor上</param>
        /// <param name="strTotalError">返回总的出错情况</param>
        /// <param name="bWarningNotSave">是否警告尚未保存？如果==false，并且“自动保存”checkbox为true，会自动保存，不警告</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel；或者“到头”“到尾”)
        ///      1   成功装载
        ///      2   通道被占用
        /// </returns>
        public int LoadRecord(string strBiblioRecPath,
            string strPrevNextStyle,
            bool bCheckInUse,
            bool bSetFocus,
            out string strTotalError,
            bool bWarningNotSave = false)
        {
            string strError = "";
            strTotalError = "";

            this.ShowMessage("正在装载书目记录 " + strBiblioRecPath + " " + strPrevNextStyle + " ...");
            this.m_nChannelInUse++;
            try
            {
                if (bCheckInUse == true)
                {
                    if (this.m_nChannelInUse > 1 || this.m_nInSearching > 0)
                    {
                        /*
                        this.m_nChannelInUse--;
                        MessageBox.Show(this, "通道已经被占用。请稍后重试");
                        return -1;
                         * */
                        strError = "通道已经被占用。请稍后重试";
                        return 2;   // 通道被占用
                    }
                }

                bool bMarcEditorContentChanged = false; // MARC编辑器内的内容可曾修改?
                bool bBiblioRecordExist = false;    // 书目记录是否存在?
                bool bSubrecordExist = false;   // 至少有一个从属的记录存在
                bool bSubrecordListCleared = false; // 子记录的list是否被清除了?

                string strOutputBiblioRecPath = "";

                lock (this.Channel)
                {
                    if (this.EntitiesChanged == true
                        || this.IssuesChanged == true
                        || this.BiblioChanged == true
                        || this.ObjectChanged == true
                        || this.OrdersChanged == true
                        || this.CommentsChanged == true)
                    {
                        // 2008/6/25 
                        if (this.checkBox_autoSavePrev.Checked == true
                            && bWarningNotSave == false)
                        {
                            int nRet = this.DoSaveAll();
                            if (nRet == -1)
                            {
                                // strTotalError = "当前记录尚未保存";  // 2014/7/8
                                return -1;
                            }
                        }
                        else
                        {
                            // 警告尚未保存
                            DialogResult result = MessageBox.Show(this,
                                "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装入新内容? ",
                                "EntityForm",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            if (result == DialogResult.No)
                                return 0;
                        }
                    }

                    EnableControls(false);
                    try
                    {
                        // 2012/7/25 移动到这里
                        // 因为 LoadBiblioRecord() 会导致填充AutoGen菜单
                        this._genData.ClearViewer();

                        if (this.m_commentViewer != null)
                            this.m_commentViewer.Clear();

                        string strXml = "";
                        int nRet = this.LoadBiblioRecord(strBiblioRecPath,
                            strPrevNextStyle,
                            false,
                            out strOutputBiblioRecPath,
                            out strXml,
                            out strError);
                        if (nRet == -1)
                        {
                            string strErrorText = "装载书目记录 '" + strBiblioRecPath + "' (style='" + strPrevNextStyle + "')时发生错误: " + strError;
#if NO
                            Global.SetHtmlString(this.webBrowser_biblioRecord,
                                strErrorText);
#endif
                            this.m_webExternalHost_biblio.SetHtmlString(strErrorText, "entityform_error");

                            // MessageBox.Show(this, strErrorText);
                            if (String.IsNullOrEmpty(strTotalError) == false)
                                strTotalError += "\r\n";
                            strTotalError += strErrorText;
                        }
                        else if (nRet == 0)
                        {
                            bBiblioRecordExist = false;
                            // 虽然种记录不存在，但是也继续装载册记录
                            // return 0;

                            string strText = "";

                            // 在不是前后翻看记录的情况下，要清空MARC窗，避免误会
                            if (String.IsNullOrEmpty(strPrevNextStyle) == true)
                            {
                                strText = "书目记录 '" + strBiblioRecPath + "' 没有找到...";

                                // 清空MARC窗，避免误会
                                // this.m_marcEditor.Marc = "012345678901234567890123";
                                this.SetMarc("012345678901234567890123");
                                bMarcEditorContentChanged = true;

                                // 如果书目记录不存在，则沿用strBiblioRecPath的路径
                                if (String.IsNullOrEmpty(strOutputBiblioRecPath) == true)
                                {
                                    strOutputBiblioRecPath = strBiblioRecPath;
                                }
                            }
                            else
                            {
                                if (strPrevNextStyle == "prev")
                                    strText = "到头";
                                else if (strPrevNextStyle == "next")
                                    strText = "到尾";

                                strText += "\r\n\r\n(窗口内的原记录没有被刷新)";

                                strOutputBiblioRecPath = "";    // 这时候继续装载下属记录也无法进行了，因为不知道书目记录的路径。TODO: 将来可以采用猜测法，把书目记录路径+1或者-1,直到遇到下一条记录
                                // MessageBox.Show(this, strText);

                                if (String.IsNullOrEmpty(strTotalError) == false)
                                    strTotalError += "\r\n";
                                strTotalError += strText;

                                return 0;   // 2008/11/2 
                            }

                            // MessageBox.Show(this, strText);
                            if (String.IsNullOrEmpty(strTotalError) == false)
                                strTotalError += "\r\n";
                            strTotalError += strText;
                        }
                        else
                        {
                            bBiblioRecordExist = true;
                        }

                        bool bError = false;

                        // 注：当bBiblioRecordExist==true时，LoadBiblioRecord()函数中已经设好了书目记录路径

                        strBiblioRecPath = null;    // 防止后面继续使用。因为prev/next风格时，strBiblioRecPath的路径并不是所获得的记录的路径

                        // 清空4个下属记录的控件
                        this.entityControl1.ClearItems();
                        this.textBox_itemBarcode.Text = ""; // 2009/1/5 

                        this.issueControl1.ClearItems();
                        this.orderControl1.ClearItems();   // 2008/11/2 
                        this.commentControl1.ClearItems();
                        this.binaryResControl1.Clear(); // 2008/11/2 
                        if (this.m_verifyViewer != null)
                            this.m_verifyViewer.Clear();
                        /*
                        if (this.m_genDataViewer != null)
                            this.m_genDataViewer.Clear();
                         * */

                        bSubrecordListCleared = true;

                        if (String.IsNullOrEmpty(strOutputBiblioRecPath) == false)
                        {
                            string strBiblioDbName = "";

                            /*
                            if (String.IsNullOrEmpty(strOutputBiblioRecPath) == false)
                                strBiblioDbName = Global.GetDbName(strOutputBiblioRecPath);
                            else
                            {
                                Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "");
                                strBiblioDbName = Global.GetDbName(strBiblioRecPath);
                            }
                             * */
                            strBiblioDbName = Global.GetDbName(strOutputBiblioRecPath);

                            // 接着装入相关的所有册
                            string strItemDbName = this.MainForm.GetItemDbName(strBiblioDbName);
                            if (String.IsNullOrEmpty(strItemDbName) == false) // 仅在当前书目库有对应的实体库时，才装入册记录
                            {
                                this.EnableItemsPage(true);

                                nRet = this.entityControl1.LoadItemRecords(strOutputBiblioRecPath,    // 2008/11/2 new changed
                                    // this.DisplayOtherLibraryItem,
                                    this.DisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (this.entityControl1.Channel.ErrorCode == ErrorCode.AccessDenied)
                                    {
                                        // 在 ListView 背景上显示报错信息，不要用 MessageBox 报错
                                        this.entityControl1.ErrorInfo = strError;
                                    }
                                    else
                                    {
                                        // MessageBox.Show(this, strError);
                                        if (String.IsNullOrEmpty(strTotalError) == false)
                                            strTotalError += "\r\n";
                                        strTotalError += strError;

                                        bError = true;
                                        // return -1;
                                    }
                                }

                                if (nRet == 1)
                                    bSubrecordExist = true;
                            }
                            else
                            {
                                this.EnableItemsPage(false);
                            }

                            // 接着装入相关的所有期
                            string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);
                            if (String.IsNullOrEmpty(strIssueDbName) == false) // 仅在当前书目库有对应的期库时，才装入期记录
                            {
                                this.EnableIssuesPage(true);

                                nRet = this.issueControl1.LoadItemRecords(strOutputBiblioRecPath,  // 2008/11/2 changed
                                    "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (this.issueControl1.Channel.ErrorCode == ErrorCode.AccessDenied)
                                    {
                                        // 在 ListView 背景上显示报错信息，不要用 MessageBox 报错
                                        this.issueControl1.ErrorInfo = strError;
                                    }
                                    else
                                    {
                                        // MessageBox.Show(this, strError);
                                        if (String.IsNullOrEmpty(strTotalError) == false)
                                            strTotalError += "\r\n";
                                        strTotalError += strError;

                                        bError = true;
                                        // return -1;
                                    }
                                }

                                if (nRet == 1)
                                    bSubrecordExist = true;
                            }
                            else
                            {
                                this.EnableIssuesPage(false);
                            }

                            // 接着装入相关的所有订购信息
                            string strOrderDbName = this.MainForm.GetOrderDbName(strBiblioDbName);
                            if (String.IsNullOrEmpty(strOrderDbName) == false) // 仅在当前书目库有对应的采购库时，才装入采购记录
                            {
                                if (String.IsNullOrEmpty(strIssueDbName) == false)
                                    this.orderControl1.SeriesMode = true;
                                else
                                    this.orderControl1.SeriesMode = false;

                                this.EnableOrdersPage(true);
                                nRet = this.orderControl1.LoadItemRecords(strOutputBiblioRecPath,  // 2008/11/2 changed
                                    "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (this.orderControl1.Channel.ErrorCode == ErrorCode.AccessDenied)
                                    {
                                        // 在 ListView 背景上显示报错信息，不要用 MessageBox 报错
                                        this.orderControl1.ErrorInfo = strError;
                                    }
                                    else
                                    {
                                        // MessageBox.Show(this, strError);
                                        if (String.IsNullOrEmpty(strTotalError) == false)
                                            strTotalError += "\r\n";
                                        strTotalError += strError;

                                        bError = true;
                                        // return -1;
                                    }
                                }

                                if (nRet == 1)
                                    bSubrecordExist = true;
                            }
                            else
                            {
                                this.EnableOrdersPage(false);
                            }

                            // 接着装入相关的所有评注信息
                            string strCommentDbName = this.MainForm.GetCommentDbName(strBiblioDbName);
                            if (String.IsNullOrEmpty(strCommentDbName) == false) // 仅在当前书目库有对应的采购库时，才装入采购记录
                            {
                                this.EnableCommentsPage(true);
                                nRet = this.commentControl1.LoadItemRecords(strOutputBiblioRecPath,
                                    "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (this.commentControl1.Channel.ErrorCode == ErrorCode.AccessDenied)
                                    {
                                        // 在 ListView 背景上显示报错信息，不要用 MessageBox 报错
                                        this.commentControl1.ErrorInfo = strError;
                                    }
                                    else
                                    {
                                        if (String.IsNullOrEmpty(strTotalError) == false)
                                            strTotalError += "\r\n";
                                        strTotalError += strError;

                                        bError = true;
                                    }
                                }


                                if (nRet == 1)
                                    bSubrecordExist = true;
                            }
                            else
                            {
                                this.EnableCommentsPage(false);
                            }

                            // 接着装入对象资源
                            {
                                nRet = this.binaryResControl1.LoadObject(strOutputBiblioRecPath,    // 2008/11/2 changed
                                    strXml,
                                    this.MainForm.ServerVersion,
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (this.binaryResControl1.Channel.ErrorCode == ErrorCode.AccessDenied)
                                    {
                                        // 在 ListView 背景上显示报错信息，不要用 MessageBox 报错
                                        this.binaryResControl1.ErrorInfo = strError;
                                    }
                                    else
                                    {
                                        // MessageBox.Show(this, strError);
                                        if (String.IsNullOrEmpty(strTotalError) == false)
                                            strTotalError += "\r\n";
                                        strTotalError += strError;

                                        bError = true;
                                        // return -1;
                                    }
                                }

                                if (nRet == 1)
                                    bSubrecordExist = true;
                            }

                            // 装载书目和<dprms:file>以外的其它XML片断
                            if (string.IsNullOrEmpty(strXml) == false)
                            {
                                nRet = LoadXmlFragment(strXml,
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (String.IsNullOrEmpty(strTotalError) == false)
                                        strTotalError += "\r\n";
                                    strTotalError += strError;

                                    bError = true;
                                }
                            }


                        } // end of if (String.IsNullOrEmpty(strOutputBiblioRecPath) == false)

                        if (string.IsNullOrEmpty(this.m_strUsedActiveItemPage) == false)
                        {
                            // 只要有实体库，即便当前书目记录没有下属的实体记录，也要显示册listview page
                            if (LoadActiveItemIssuePage(m_strUsedActiveItemPage) == true)
                                this.m_strUsedActiveItemPage = "";
                        }

                        if (bBiblioRecordExist == false && bSubrecordExist == true)
                            this.BiblioRecPath = strOutputBiblioRecPath;

                        if (bBiblioRecordExist == false
                            && bSubrecordExist == false
                            && bSubrecordListCleared == true)
                        {
                            if (bMarcEditorContentChanged == false)
                            {
                                this.m_marcEditor.Marc = "012345678901234567890123";
                                this.SetMarc("012345678901234567890123");
                                bMarcEditorContentChanged = true;
                            }

                            if (this.DeletedMode == false)
                                this.BiblioRecPath = "";    // 避免残余记录覆盖了不该覆盖的记录
                        }

                        // 2008/11/2 
                        if (bMarcEditorContentChanged == true)
                            this.BiblioChanged = false; // 避免后面自动保存时错误覆盖了不该覆盖的记录

                        // 2008/9/16 
                        this.DeletedMode = false;

                        if (bError == true)
                            return -1;

                        // 2013/11/13
                        if (bBiblioRecordExist == false
    && bSubrecordExist == false
    && bSubrecordListCleared == true)
                            return -1;

                        // 2008/11/26 
                        if (m_strFocusedPart == "marceditor"
                            && bSetFocus == true)
                        {
                            SwitchFocus(MARC_EDITOR);
                        }

                        DoViewComment(false);
                        return 1;
                    }
                    finally
                    {
                        EnableControls(true);
                    }
                }
            }
            finally
            {
                this.m_nChannelInUse--;
                this.ClearMessage();
            }
        }

        /// <summary>
        /// 当前记录是否具有评注属性页
        /// </summary>
        public bool HasCommentPage
        {
            get
            {
                if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
                    return false;

                string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);
                string strCommentDbName = this.MainForm.GetCommentDbName(strBiblioDbName);
                if (String.IsNullOrEmpty(strCommentDbName) == false) // 仅在当前书目库有对应的采购库时，才装入采购记录
                    return true;
                return false;
            }
        }

        // TODO: 拼音风格发生变化后，以前创建的拼音字符串是不是会引起问题?
        /// <summary>
        /// 获得当前记录已经选择过的多音字情况
        /// </summary>
        /// <returns>汉字字符串和拼音字符串的对照表</returns>
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

        /// <summary>
        /// 设置使用过的拼音信息
        /// 存储起来提供以后使用
        /// </summary>
        /// <param name="table">汉字字符串和拼音字符串的对照表</param>
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

        // 装载书目和<dprms:file>以外的其它XML片断
        int LoadXmlFragment(string strXml,
            out string strError)
        {
            strError = "";

            this.domXmlFragment = null;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            nsmgr.AddNamespace("unimarc", DpNs.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//unimarc:leader | //unimarc:controlfield | //unimarc:datafield | //usmarc:leader | //usmarc:controlfield | //usmarc:datafield | //dprms:file", nsmgr);
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            this.domXmlFragment = new XmlDocument();
            this.domXmlFragment.LoadXml("<root />");
            this.domXmlFragment.DocumentElement.InnerXml = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 检测特定位置书目记录是否已经存在
        // parameters:
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int DetectBiblioRecord(string strBiblioRecPath,
            out byte [] timestamp,
            out string strError)
        {
            strError = "";
            timestamp = null;

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在检测书目记录 " + strBiblioRecPath + " ...");
            Progress.BeginLoop();

            try
            {

                string[] formats = new string[1];
                formats[0] = "xml";

                string[] results = null;

                long lRet = Channel.GetBiblioInfos(
                    Progress,
                    strBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == 0)
                {
                    return 0;   // not found
                }
                if (lRet == -1)
                    return -1;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
            /*
        ERROR1:
            return -1;
             * */
        }



        // 装入书目记录
        // 本函数在后期修改this.BiblioRecPath。如果中间出错，则不修改
        // parameters:
        //      strDirectionStyle   prev/next/空
        //      bWarningNotSaved    是否警告装入前书目信息修改后尚未保存？
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LoadBiblioRecord(string strBiblioRecPath,
            string strDirectionStyle,
            bool bWarningNotSaved,
            out string strOutputBiblioRecPath,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strOutputBiblioRecPath = "";

            // 2008/6/24 
            if (String.IsNullOrEmpty(strDirectionStyle) == false)
            {
                if (strDirectionStyle != "prev"
                    && strDirectionStyle != "next")
                {
                    strError = "未知的strDirectionStyle参数值 '" + strDirectionStyle + "'";
                    return -1;
                }
            }

            if (bWarningNotSaved == true
                && this.Cataloging == true
                && this.BiblioChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
    "当前有编目信息被修改后尚未保存。若此时装载新内容，现有未保存信息将丢失。\r\n\r\n确实要装入新内容? ",
    "EntityForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    strError = "放弃装载书目记录";
                    return -1;
                }
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在初始化浏览器组件 ...");
            Progress.BeginLoop();

            // this.Update();   // 优化
            // this.MainForm.Update();

            try
            {
                if (String.IsNullOrEmpty(strDirectionStyle) == false)
                {
                    strBiblioRecPath += "$" + strDirectionStyle;
                }

#if NO
                Global.SetHtmlString(this.webBrowser_biblioRecord, "(空白)");
#endif
                this.m_webExternalHost_biblio.SetHtmlString("(空白)", "entityform_error");

                Progress.SetMessage("正在装入书目记录 " + strBiblioRecPath + " ...");

                bool bCataloging = this.Cataloging;

                /*
                long lRet = Channel.GetBiblioInfo(
                    stop,
                    strBiblioRecPath,
                    "html",
                    out strHtml,
                    out strError);
                 * */
                string[] formats = null;

                if (bCataloging == true)
                {
                    formats = new string[3];
                    formats[0] = "outputpath";
                    formats[1] = "html";
                    formats[2] = "xml";
                }
                else
                {
                    formats = new string[2];
                    formats[0] = "outputpath";
                    formats[1] = "html";
                }

                string[] results = null;
                byte[] baTimestamp = null;

                long lRet = Channel.GetBiblioInfos(
                    Progress,
                    strBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    // Global.SetHtmlString(this.webBrowser_biblioRecord, "路径为 '" + strBiblioRecPath + "' 的书目记录没有找到 ...");
                    this.m_webExternalHost_biblio.SetHtmlString("路径为 '" + strBiblioRecPath + "' 的书目记录没有找到 ...",
                        "entityform_error");
                    return 0;   // not found
                }

                string strHtml = "";

                bool bError = false;
                string strErrorText = "";

                if (results != null && results.Length >= 1)
                    strOutputBiblioRecPath = results[0];

                if (results != null && results.Length >= 2)
                    strHtml = results[1];

                if (lRet == -1)
                {
                    // 暂时不报错
                    bError = true;
                    strErrorText = strError;

                    // 有报错的情况下不刷新时间戳 2008/11/28 changed
                }
                else
                {
                    // 2014/11/5
                    if (string.IsNullOrEmpty(strError) == false)
                    {
                        bError = true;
                        strErrorText = strError;
                    }

                    // 没有报错时，要对results进行严格检查
                    if (results == null)
                    {
                        strError = "results == null";
                        goto ERROR1;
                    }
                    if (results.Length != formats.Length)
                    {
                        strError = "result.Length != formats.Length";
                        goto ERROR1;
                    }

                    // 没有报错的情况下才刷新时间戳 2008/11/28 changed
                    this.BiblioTimestamp = baTimestamp;
                }
#if NO
                Global.SetHtmlString(this.webBrowser_biblioRecord,
                    strHtml,
                    this.MainForm.DataDir,
                    "entityform_biblio");
#endif
                this.m_webExternalHost_biblio.SetHtmlString(strHtml,
                    "entityform_biblio");

                // 如果没有修改BiblioRecPath，就不能把MARC编辑器中的书目记录修改，否则因BiblioChanged已经为true，可能会导致后面在原有书目记录上作错误的自动保存的副作用
                this.BiblioRecPath = strOutputBiblioRecPath; // 2008/6/24 

                if (bCataloging == true)
                {
                    if (results != null && results.Length >= 3)
                        strXml = results[2];

                    if (bError == false)    // 2008/6/24 
                    {
                        // return:
                        //      -1  error
                        //      0   空的记录
                        //      1   成功
                        int nRet = SetBiblioRecordToMarcEditor(strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 2008/11/13 
                        if (nRet == 0)
                            MessageBox.Show(this, "警告：当前书目记录 '" + strOutputBiblioRecPath + "' 是一条空记录");

                        this.BiblioChanged = false;

                        // 2009/10/24 
                        // 根据998$t 兑现ReadOnly状态
                        string strTargetBiblioRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");
                        if (String.IsNullOrEmpty(strTargetBiblioRecPath) == false)
                        {
                            if (this.LinkedRecordReadonly == true)
                            {
                                // TODO: 装载目标记录，替代当前全部内容(除了998)
                                this.m_marcEditor.ReadOnly = true;
                            }
                        }
                        else
                        {
                            // 如果必要，恢复可编辑状态
                            if (this.m_marcEditor.ReadOnly != false)
                                this.m_marcEditor.ReadOnly = false;
                        }

                        // 注：非采购工作库，也可以设定目标记录路径
                        // TODO: 未来可以增加“终点库”角色，这样的库才是不能设定目标记录路径的
                        /*
                        // 根据当前库是不是采购工作库，决定“设置目标记录”按钮是否为Enabled
                        if (this.MainForm.IsOrderWorkDb(this.BiblioDbName) == true)
                            this.toolStripButton_setTargetRecord.Enabled = true;
                        else
                            this.toolStripButton_setTargetRecord.Enabled = false;
                         * */

                    }
                }

                if (bError == true)
                {
                    strError = strErrorText;
                    goto ERROR1;
                }
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 将XML格式的书目记录装入MARC窗中
        // return:
        //      -1  error
        //      0   空的记录
        //      1   成功
        int SetBiblioRecordToMarcEditor(string strXml,
            out string strError)
        {
            strError = "";

            string strMarcSyntax = "";
            string strOutMarcSyntax = "";
            string strMarc = "";

            // 保存XML数据
            this.m_strOriginBiblioXml = strXml;

            // 2008/11/13 
            if (String.IsNullOrEmpty(strXml) == true)
            {
                strMarc = "012345678901234567890123";
                // this.m_marcEditor.Marc = strMarc;
                this.SetMarc(strMarc);
                this.MarcSyntax = "";
                return 0;
            }
            else
            {

                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                int nRet = MarcUtil.Xml2Marc(strXml,
                    true,
                    strMarcSyntax,
                    out strOutMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    return -1;
                }
                // this.m_marcEditor.Marc = strMarc;
                this.MarcSyntax = strOutMarcSyntax;
                this.SetMarc(strMarc);
                return 1;
            }
        }

        // 清除编目有关信息
        void ClearBiblio()
        {
            // this.m_marcEditor.Marc = "012345678901234567890123";
            this.SetMarc("012345678901234567890123");
            this.BiblioChanged = false;

            this.MarcSyntax = "";   // 2015/8/12

            // Global.SetHtmlString(this.webBrowser_biblioRecord, "(空白)");
            this.m_webExternalHost_biblio.SetHtmlString("(空白)",
                "entityform_error");
        }

        #region 原来的entity处理相关代码

        /*
        // 清除实体有关信息
        void ClearItems()
        {
            this.listView_items.Items.Clear();
            this.bookitems = new BookItemCollection();
        }
         
        // 装入实体记录
        int LoadEntityRecords(string strBiblioRecPath,
            out string strError)
        {
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入册信息 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();


            try
            {
                // string strHtml = "";

                EntityInfo[] entities = null;

                long lRet = Channel.GetEntities(
                    stop,
                    strBiblioRecPath,
                    out entities,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                this.ClearItems();

                // Debug.Assert(false, "");

                if (lRet == 0)
                    return 0;

                Debug.Assert(entities != null, "");


                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i].ErrorCode != ErrorCodeValue.NoError)
                    {
                        strError = "路径为 '" + entities[i].OldRecPath + "' 的册记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
                        return -1;
                    }

                    // 剖析一个册的xml记录，取出有关信息放入listview中
                    BookItem bookitem = new BookItem();

                    int nRet = bookitem.SetData(entities[i].OldRecPath, // NewRecPath
                             entities[i].OldRecord,
                             entities[i].OldTimestamp,
                             out strError);
                    if (nRet == -1)
                        return -1;

                    if (entities[i].ErrorCode == ErrorCodeValue.NoError)
                        bookitem.Error = null;
                    else
                        bookitem.Error = entities[i];

                    this.bookitems.Add(bookitem);


                    bookitem.AddToListView(this.listView_items);
                }

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
         

        // 右鼠标键菜单
        private void listView_items_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool bHasBillioLoaded = false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
                bHasBillioLoaded = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newEntity_Click);
            if (bHasBillioLoaded == false)
                menuItem.Enabled = false;

            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // cut 剪切
            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // copy 复制
            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(ClipboardBookItemCollection)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            // paste 粘贴
            menuItem = new MenuItem("粘贴(&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteEntity_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 改变归属
            menuItem = new MenuItem("改变归属(&B)");
            menuItem.Click += new System.EventHandler(this.menu_changeParent_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("标记删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("撤销删除(&U)");
            menuItem.Click += new System.EventHandler(this.menu_undoDeleteEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_items, new Point(e.X, e.Y));
        }

         *         public int CountOfVisibleBookItems()
        {
            return this.listView_items.Items.Count;
        }

        public int IndexOfVisibleBookItems(BookItem bookitem)
        {
            for (int i = 0; i < this.listView_items.Items.Count; i++)
            {
                BookItem cur = (BookItem)this.listView_items.Items[i].Tag;

                if (cur == bookitem)
                    return i;
            }

            return -1;
        }

        public BookItem GetAtVisibleBookItems(int nIndex)
        {
            return (BookItem)this.listView_items.Items[nIndex].Tag;
        }

                 */

        #endregion


        // 改变 全部保存 按钮的状态
        void SetSaveAllButtonState(bool bEnable)
        {
            // 2011/11/8
            if (this.m_bDeletedMode == true)
            {
                this.button_save.Enabled = true;
                this.toolStripButton_saveAll.Enabled = true;
                return;
            }

            if (this.EntitiesChanged == true
                || this.IssuesChanged == true
                || this.BiblioChanged == true
                || this.ObjectChanged == true
                || this.OrdersChanged == true
                || this.CommentsChanged == true
                )
            {
                this.button_save.Enabled = bEnable;
                this.toolStripButton_saveAll.Enabled = bEnable;
            }
            else
            {
                this.button_save.Enabled = false;
                this.toolStripButton_saveAll.Enabled = false;
            }
        }

        int m_nInDisable = 0;

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_queryWord.Enabled = bEnable;

            if (this.ItemsPageVisible == false)
            {
                this.textBox_itemBarcode.Enabled = false;
                this.button_register.Enabled = false;
            }
            else
            {
                this.textBox_itemBarcode.Enabled = bEnable;
                this.button_register.Enabled = bEnable;
            }

            if (bEnable == false)
                this.button_save.Enabled = bEnable;
            else
                SetSaveAllButtonState(bEnable);

            this.button_search.Enabled = bEnable;
            this.toolStripButton_option.Enabled = bEnable;

            this.entityControl1.Enabled = (this.m_bDeletedMode == true) ? false : bEnable;
            this.issueControl1.Enabled = (this.m_bDeletedMode == true) ? false : bEnable;
            this.orderControl1.Enabled = (this.m_bDeletedMode == true) ? false : bEnable;
            this.commentControl1.Enabled = (this.m_bDeletedMode == true) ? false : bEnable;
            this.binaryResControl1.Enabled = (this.m_bDeletedMode == true) ? false : bEnable;

            this.comboBox_from.Enabled = bEnable;
            this.checkedComboBox_biblioDbNames.Enabled = bEnable;
            this.comboBox_matchStyle.Enabled = bEnable;

            // this.checkBox_autoDetectQueryBarcode.Enabled = bEnable;
            this.checkBox_autoSavePrev.Enabled = bEnable;

            this.textBox_biblioRecPath.Enabled = bEnable;

            this.toolStripButton_clear.Enabled = bEnable;

            if (this.toolStrip_marcEditor.Enabled != bEnable)
                this.toolStrip_marcEditor.Enabled = bEnable;

            bool bValue = (this.m_bDeletedMode == true) ? false : bEnable;  // 2012/3/19
            if (this.m_marcEditor.Enabled != bValue)
                this.m_marcEditor.Enabled = bValue;
        }

        // 获取书目记录的局部
        int GetBiblioPart(string strBiblioRecPath,
            string strBiblioXml,
            string strPartName,
            out string strResultValue,
            out string strError)
        {
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获取书目记录的局部 -- '" + strPartName + "'...");
            Progress.BeginLoop();
            try
            {
                // stop.SetMessage("正在装入书目记录 " + strBiblioRecPath + " ...");

                long lRet = Channel.GetBiblioInfo(
                    Progress,
                    strBiblioRecPath,
                    strBiblioXml,
                    strPartName,    // 包含'@'符号
                    out strResultValue,
                    out strError);
                return (int)lRet;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }
        }

        string GetMacroValue(string strMacroName)
        {
            // return strMacroName + "--";
            string strError = "";
            string strResultValue = "";
            int nRet = 0;

            // 2015/7/10
            // @accessNo 只能由前端处理
            if (strMacroName == "@accessNo")
                return strMacroName;

            // 2015/10/10
            if (strMacroName.IndexOf("%") != -1)
            {
                ParseOneMacroEventArgs e1 = new ParseOneMacroEventArgs();
                e1.Macro = strMacroName;
                e1.Simulate = false;
                m_macroutil_ParseOneMacro(this, e1);
                if (e1.Canceled == true)
                    goto CONTINUE;
                if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    return strMacroName + ":error:" + e1.ErrorInfo;
                }
                return e1.Value;
            }

         CONTINUE:
            // 书目记录XML格式
            string strXmlBody = "";

            if (Global.IsAppendRecPath(this.BiblioRecPath) == true
                || this.BiblioChanged == true)  // 2010/12/5 add
            {
                // 如果记录路径表明这是一条待存的新记录，那就需要准备好strXmlBody，以便获取宏的时候使用
                nRet = this.GetBiblioXml(
                    "", // 迫使从记录路径中看marc格式
                    true,   // 包含资源ID
                    out strXmlBody,
                    out strError);
                if (nRet == -1)
                    return strError;
            }

            // 获取书目记录的局部
            nRet = GetBiblioPart(this.BiblioRecPath,
                strXmlBody,
                strMacroName,
                out strResultValue,
                out strError);
            if (nRet == -1)
            {
                if (String.IsNullOrEmpty(strResultValue) == true)
                    return strMacroName + ":error:" + strError;

                return strResultValue;
            }

            return strResultValue;
        }

        // 全部保存
        private void button_save_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);
            this.m_nInDisable++;
            try
            {
                if (string.IsNullOrEmpty(this.BiblioRecPath) == true)
                    toolStripButton1_marcEditor_saveTo_Click(null, null);
                else
                    DoSaveAll();
            }
            finally
            {
                this.m_nInDisable--;
                this.EnableControls(true);
            }
        }


        // 提交所有保存请求
        // parameters:
        //      strStyle    风格。displaysuccess 显示最后的成功消息在框架窗口的状态条 verifydata 发送校验记录的消息(注意是否校验还要取决于配置状态)
        //                  searchdup 虽然对本函数没有作用，但是可以传递到下级函数SaveBiblioToDatabase()
        // return:
        //      -1  有错。此时不排除有些信息保存成功。
        //      0   成功。
        /// <summary>
        /// 全部保存
        /// </summary>
        /// <param name="strStyle">保存方式。由 displaysuccess / verifydata / searchdup 之一或者逗号间隔组合而成。displaysuccess 显示最后的成功消息在框架窗口的状态条; verifydata 保存成功后发送校验记录的消息(注意是否校验还要取决于配置状态); searchdup 保存成功后发送查重消息</param>
        /// <returns>-1: 有错。此时不排除有些信息保存成功。0: 成功。</returns>
        public int DoSaveAll(string strStyle = "displaysuccess,verifydata,searchdup")
        {
            bool bBiblioSaved = false;
            int nRet = 0;
            string strText = "";
            int nErrorCount = 0;

            bool bDisplaySuccess = StringUtil.IsInList("displaysuccess", strStyle);
            bool bVerifyData = StringUtil.IsInList("verifydata", strStyle);
            // bool bForceVerifyData = StringUtil.IsInList("forceverifydata", strStyle);

            bool bVerified = false;

            if (this.m_verifyViewer != null)
                this.m_verifyViewer.Clear();

            string strHtml = "";

            if (this.BiblioChanged == true
                || Global.IsAppendRecPath(this.BiblioRecPath) == true
                || this.m_bDeletedMode == true /* 2011/11/8 */)
            {
                // 2014/7/3
                if (bVerifyData == true
&& this.ForceVerifyData == true)
                {
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    e1.FocusedControl = this.m_marcEditor;

                    // 0: 没有发现校验错误; 1: 发现校验警告; 2: 发现校验错误
                    nRet = this.VerifyData(this, e1, true);
                    if (nRet == 2)
                    {
                        MessageBox.Show(this, "MARC 记录经校验发现有错，被拒绝保存。请修改 MARC 记录后重新保存");
                        return -1;
                    }

                    bVerified = true;
                }

                // 保存书目记录到数据库
                // return:
                //      -1  出错
                //      0   没有保存
                //      1   已经保存
                nRet = SaveBiblioToDatabase(true,
                    out strHtml,
                    strStyle);
                if (nRet == 1)
                {
                    bBiblioSaved = true;
                    strText += "书目信息";
                }
                if (nRet == -1)
                {
                    nErrorCount++;
                }
            }

            bool bOrdersSaved = false;
            // 提交订购保存请求
            // return:
            //      -1  出错
            //      0   没有必要保存
            //      1   保存成功
            nRet = this.orderControl1.DoSaveItems();
            if (nRet == 1)
            {
                bOrdersSaved = true;
                if (strText != "")
                    strText += " ";
                strText += "采购信息";
            }
            if (nRet == -1)
            {
                nErrorCount++;

                // 2013/1/18
                // 如果订购信息保存不成功，则不要继续保存后面的其他信息。这主要是为了订购验收环节考虑，避免在订购信息保存失败的情况下继续保存验收所创建的新的册信息
                return -1;
            }

            bool bIssuesSaved = false;
            bool bIssueError = false;
            // 提交期保存请求
            // return:
            //      -1  出错
            //      0   没有必要保存
            //      1   保存成功
            nRet = this.issueControl1.DoSaveItems();
            if (nRet == 1)
            {
                bIssuesSaved = true;
                if (strText != "")
                    strText += " ";
                strText += "期信息";
            }
            if (nRet == -1)
            {
                nErrorCount++;
                bIssueError = true;

                // 2013/1/18
                // 如果期信息保存不成功，则不要继续保存后面的其他信息。这主要是为了期刊验收环节考虑，避免在期信息保存失败的情况下继续保存验收所创建的新的册信息
                return -1;
            }

            bool bEntitiesSaved = false;

            // 注：在期刊记到后，如果期信息保存不成功，则不保存册信息。以免发生不一致
            if (bIssueError == false)
            {
                // 提交实体保存请求
                // return:
                //      -1  出错
                //      0   没有必要保存
                //      1   保存成功
                nRet = this.entityControl1.DoSaveItems();
                if (nRet == 1)
                {
                    bEntitiesSaved = true;
                    if (strText != "")
                        strText += " ";
                    strText += "册信息";
                }
                if (nRet == -1)
                {
                    nErrorCount++;
                }
            }

            bool bCommentsSaved = false;
            // 提交评注保存请求
            // return:
            //      -1  出错
            //      0   没有必要保存
            //      1   保存成功
            nRet = this.commentControl1.DoSaveItems();
            if (nRet == 1)
            {
                bCommentsSaved = true;
                if (strText != "")
                    strText += " ";
                strText += "评注信息";
            }
            if (nRet == -1)
            {
                nErrorCount++;
            }

            bool bObjectSaved = false;
            string strError = "";

            // 当允许编目功能的时候才能允许保存对象资源。否则会把书目记录摧毁为空记录
            if (this.Cataloging == true)
            {
                // 提交对象保存请求
                // return:
                //		-1	error
                //		>=0 实际上载的资源对象数
                nRet = this.binaryResControl1.Save(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "保存对象信息时出错: " + strError);
                    nErrorCount++;
                }

                if (nRet >= 1)
                {
                    bObjectSaved = true;
                    if (strText != "")
                        strText += " ";
                    strText += "对象信息";

                    /*
                    string strSavedBiblioRecPath = this.BiblioRecPath;

                    // 刷新书目记录的时间戳
                    string strOutputBiblioRecPath = "";
                    string strXml = "";
                    nRet = LoadBiblioRecord(this.BiblioRecPath,
                        "",
                        false,
                        out strOutputBiblioRecPath,
                        out strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        // 如果提取记录失败，并且原有书目记录路径被摧毁，需要恢复
                        if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
                        {
                            this.BiblioRecPath = strSavedBiblioRecPath;
                        }

                        MessageBox.Show(this, strError + "\r\n\r\n注意：当前窗口内的书目记录时间戳可能没有正确刷新，这将导致后继的保存书目记录操作出现时间戳不匹配报错");
                        nErrorCount++;
                    }
                    */
                }
            }

            if (string.IsNullOrEmpty(strHtml) == false)
            {
                this.m_webExternalHost_biblio.SetHtmlString(strHtml,
"entityform_biblio");
            }

            if (bDisplaySuccess == true)
            {
                if (bEntitiesSaved == true
                    || bBiblioSaved == true
                    || bIssuesSaved == true
                    || bOrdersSaved == true
                    || bObjectSaved == true
                    || bCommentsSaved == true)
                    this.MainForm.StatusBarMessage = strText + " 保存 成功";
            }

            if (nErrorCount > 0)
            {
                return -1;
            }

            // 保存成功后再校验 MARC 记录
            if (bVerifyData == true
                && this.AutoVerifyData == true
                && bVerified == false)
            {
                API.PostMessage(this.Handle, WM_VERIFY_DATA, 0, 0);
            }

            return 0;
        }



        /*
        void DisplayErrorInfo(EntityInfo[] errorinfos)
        {
            if (errorinfos == null)
                return;

            for (int i = 0; i < errorinfos.Length; i++)
            {
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                    continue;   // 越过一般信息

                XmlDocument dom = new XmlDocument();

                string strNewXml = errorinfos[i].NewRecord;
                string strOldXml = errorinfos[i].OldRecord;

                if (String.IsNullOrEmpty(strNewXml) == false)
                {
                    dom.LoadXml(strNewXml);
                }
                else if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    dom.LoadXml(strOldXml);
                }
                else {
                    // 找不到条码来定位
                    Debug.Assert(false, "找不到定位的条码");
                    // 是否单独显示出来?
                    continue;
                }

                string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

                if (String.IsNullOrEmpty(strBarcode) == true)
                {
                    Debug.Assert(false, "DOM中没有非空的<barcode>元素值");
                    continue;
                }

                BookItem bookitem = this.bookitems.GetItem(strBarcode);

                if (bookitem == null)
                {
                    Debug.Assert(false, "条码在bookitems中没有找到");
                    continue;
                }

                bookitem.ErrorInfo = errorinfos[i].ErrorInfo;
                bookitem.RefreshListView();
            }
        }
         */

        string GetBiblioQueryString()
        {
            string strText = this.textBox_queryWord.Text;
            int nRet = strText.IndexOf(';');
            if (nRet != -1)
            {
                strText = strText.Substring(0, nRet).Trim();
                this.textBox_queryWord.Text = strText;
            }

            /*
            if (this.checkBox_autoDetectQueryBarcode.Checked == true)
            {
                if (strText.Length == 13)
                {
                    string strHead = strText.Substring(0, 3);
                    if (strHead == "978")
                    {
                        this.textBox_queryWord.Text = strText + " ;自动用" + strText.Substring(3, 9) + "来检索";
                        return strText.Substring(3, 9);
                    }
                }
            }*/

            return strText;
        }

        /// <summary>
        /// 检索命中的最大记录数限制参数。-1 表示不限制
        /// </summary>
        public int MaxSearchResultCount
        {
            get
            {
                return (int)this.MainForm.AppInfo.GetInt(
                    "biblio_search_form",
                    "max_result_count",
                    -1);

            }
        }

#if NO
        internal new void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
#if NO
            if (sender == this.browseWindow)
                this._browseWindowSelected = true;
#endif
        }
#endif

        // bool _browseWindowSelected = false;     // 小浏览窗口是否被确定选择记录而关闭的
        bool _willCloseBrowseWindow = false;    // 是否要在检索结束后自动关闭浏览窗口(一般是因为中途 X 按钮被触发过了)
        // 进行检索
        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            bool bDisplayClickableError = false;

            _willCloseBrowseWindow = false;
            // _browseWindowSelected = false;

            ActivateBrowseWindow(false);

            this.browseWindow.RecordsList.Items.Clear();

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在检索 ...");
            Progress.BeginLoop();

            this.ShowMessage("正在检索 ...");

            this.browseWindow.stop = Progress;

            //this.button_search.Enabled = false;
            this.EnableControls(false);

            m_nInSearching++;

            try
            {
                if (this.comboBox_from.Text == "")
                {
                    strError = "尚未选定检索途径";
                    goto ERROR1;
                }
                string strFromStyle = "";

                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle(this.comboBox_from.Text);
                }
                catch (Exception ex)
                {
                    strError = "GetBiblioFromStyle() exception:" + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + this.comboBox_from.Text + "' 对应的style字符串";
                    goto ERROR1;
                }

                string strMatchStyle = BiblioSearchForm.GetCurrentMatchStyle(this.comboBox_matchStyle.Text);
                if (this.textBox_queryWord.Text == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.textBox_queryWord.Text = "";

                        // 专门检索空值
                        strMatchStyle = "exact";
                    }
                    else
                    {
                        // 为了在检索词为空的时候，检索出全部的记录
                        strMatchStyle = "left";
                    }
                }
                else
                {
                    // 2009/11/5 
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        goto ERROR1;
                    }
                }

                string strQueryWord = GetBiblioQueryString();

                bool bNeedShareSearch = false;
                if (this.SearchShareBiblio == true
    && this.MainForm != null && this.MainForm.MessageHub != null
    && this.MainForm.MessageHub.ShareBiblio == true)
                {
                    bNeedShareSearch = true;
                }

                if (bNeedShareSearch == true)
                {
                    // 开始检索共享书目
                    // return:
                    //      -1  出错
                    //      0   没有检索目标
                    //      1   成功启动检索
                    nRet = BeginSearchShareBiblio(
                        this.textBox_queryWord.Text,
                        strFromStyle,
                        strMatchStyle,
                        out strError);
                    if (nRet == -1)
                    {
                        // 显示错误信息
                        this.ShowMessage(strError, "red", true);
                        bDisplayClickableError = true;
                    }
                }

                string strQueryXml = "";
                long lRet = Channel.SearchBiblio(Progress,
                    this.checkedComboBox_biblioDbNames.Text,    // "<全部>",
                    strQueryWord,   // this.textBox_queryWord.Text,
                    this.MaxSearchResultCount,  // 1000
                    strFromStyle,
                    strMatchStyle,
                    this.Lang,
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // TODO: 最多检索1000条的限制，可以作为参数配置？在CfgDlg中

                long lHitCount = lRet;

                if (lHitCount == 0)
                {
                    // strError = "从途径 '" + strFromStyle + "' 检索 '" + strQueryWord + "' 没有命中";
                    // goto ERROR1;
                }
                else
                {
                    if (Progress != null && Progress.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    if (lHitCount > 1)
                        this.ShowBrowseWindow(-1);

                    // 从此位置以后，_willCloseBrowseWindow 如果变为 true 则表示要立即终止循环和处理

                    long lStart = 0;
                    long lPerCount = Math.Min(50, lHitCount);
                    DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if ((Progress != null && Progress.State != 0)
                            || _willCloseBrowseWindow)
                        {
                            // MessageBox.Show(this, "用户中断");
                            break;  // 已经装入的还在
                        }

                        Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                        lRet = Channel.GetSearchResult(
                            Progress,
                            null,   // strResultSetName
                            lStart,
                            lPerCount,
                            "id,cols",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                        {
                            if (this.browseWindow == null
                                || (Progress != null && Progress.State != 0)
                                || _willCloseBrowseWindow)
                            {
                                // MessageBox.Show(this, "用户中断");
                                break;
                            }

                            goto ERROR1;
                        }

                        if (lRet == 0)
                        {
                            MessageBox.Show(this, "未命中");
                            return;
                        }

                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            if (this.browseWindow == null)
                                break;
                            Global.AppendNewLine(
                                this.browseWindow.RecordsList,
                                searchresults[i].Path,
                                searchresults[i].Cols);
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;
                    }
                }

                if (bNeedShareSearch == true)
                {
                    this.ShowMessage("等待共享检索响应 ...");
                    // 结束检索共享书目
                    // return:
                    //      -1  出错
                    //      >=0 命中记录个数
                    nRet = EndSearchShareBiblio(out strError);
                    if (nRet == -1)
                    {
                        // 显示错误信息
                        this.ShowMessage(strError, "red", true);
                        bDisplayClickableError = true;
                    }
                    else
                    {
#if NO
                        if (_searchParam._searchCount > 0)
                        {
                            this.ShowMessage("共享书目命中 " + _searchParam._searchCount + " 条", "green");
                            this._floatingMessage.DelayClear(new TimeSpan(0, 0, 3));
                        }
#endif
                    }

                    lHitCount += _searchParam._searchCount;
                }

                if (this.browseWindow == null)
                    goto END1;

#if NO
                if ((Progress != null && Progress.State != 0)
    && _browseWindowSelected == false)
                {
                    // 双击后会走到这里
                    strError = "用户中断";
                    goto ERROR1;
                }

                if (_browseWindowSelected)
                {
                    this.SwitchFocus(MARC_EDITOR);
                    return;
                }
#endif

                if (lHitCount > 1)
                    this.ShowBrowseWindow(lHitCount);

                if (lHitCount == 1)
                {
                    this.browseWindow.LoadFirstDetail(true);    // 不会触发 closed 事件
                    this.CloseBrowseWindow();
                }
                else
                {
                    if (lHitCount == 0)
                    {
                        this.ShowMessage("未命中", "yellow", true);
                        bDisplayClickableError = true;
                    }
                }

#if NO
                if (lHitCount == 0)
                    this.label_message.Text = "未命中";
                else
                    this.label_message.Text = "检索共命中 " + lHitCount.ToString() + " 条书目记录，已全部装入";
#endif

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                if (bDisplayClickableError == false
    && this._floatingMessage.InDelay() == false)
                    this.ClearMessage();

                if (this.MainForm.MessageHub != null)
                    this.MainForm.MessageHub.SearchResponseEvent -= MessageHub_SearchResponseEvent;

                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.Style = StopStyle.None;

                // this.button_search.Enabled = true;
                this.EnableControls(true);

                m_nInSearching--;
            }

            if (_willCloseBrowseWindow == true)
                CloseBrowseWindow();

            END1:
            this.textBox_queryWord.SelectAll();

            // 焦点切换到条码textbox
            /*
            this.textBox_itemBarcode.SelectAll();
            this.textBox_itemBarcode.Focus(); 
             * */
            this.SwitchFocus(ITEM_BARCODE);
            return;
        ERROR1:
            CloseBrowseWindow();
            MessageBox.Show(this, strError);
            // 焦点仍回到种检索词
            /*
            this.textBox_queryWord.Focus();
            this.textBox_queryWord.SelectAll();
             * */
            this.SwitchFocus(BIBLIO_SEARCHTEXT);
        }

        // 开始检索共享书目
        // return:
        //      -1  出错
        //      0   没有检索目标
        //      1   成功启动检索
        int BeginSearchShareBiblio(
            string strQueryWord,
            string strFromStyle,
            string strMatchStyle,
            out string strError)
        {
            strError = "";

            this.browseWindow.BrowseTitleTable = this._browseTitleTable;

            string strSearchID = Guid.NewGuid().ToString();
            _searchParam = new SearchParam();
            _searchParam._searchID = strSearchID;
            _searchParam._searchComplete = false;
            _searchParam._searchCount = 0;
            this.MainForm.MessageHub.SearchResponseEvent += MessageHub_SearchResponseEvent;

            string strOutputSearchID = "";
            int nRet = this.MainForm.MessageHub.BeginSearchBiblio(
                strSearchID,
                "<全部>",
strQueryWord,
strFromStyle,
strMatchStyle,
"",
1000,
out strOutputSearchID,
out strError);
            if (nRet == -1)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                return -1;
            }
            if (nRet == 0)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                return 0;
            }

            return 1;
        }

        // 结束检索共享书目
        // return:
        //      -1  出错
        //      >=0 命中记录个数
        int EndSearchShareBiblio(out string strError)
        {
            strError = "";

            try
            {
                // 装入浏览记录
                TimeSpan timeout = new TimeSpan(0, 1, 0);
                DateTime start_time = DateTime.Now;
                while (_searchParam._searchComplete == false)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                    if (DateTime.Now - start_time > timeout)    // 超时
                        break;
                    if (this.Progress != null && this.Progress.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }

                return _searchParam._searchCount;
            }
            finally
            {
                _searchParam._searchID = "";
            }
        }

        class SearchParam
        {
            public string _searchID = "";
            public bool _searchComplete = false;
            public int _searchCount = 0;
        }

        SearchParam _searchParam = null;

        // 外来数据的浏览列标题的对照表。MARC 格式名 --> 列标题字符串
        Hashtable _browseTitleTable = new Hashtable();

        void MessageHub_SearchResponseEvent(object sender, SearchResponseEventArgs e)
        {
            if (e.SsearchID != _searchParam._searchID)
                return;
            if (e.ResultCount == -1 && e.Start == -1)
            {
                // 检索过程结束
                _searchParam._searchComplete = true;
                return;
            }
            string strError = "";

            if (e.ResultCount == -1)
            {
                strError = e.ErrorInfo;
                goto ERROR1;
            }

            // TODO: 注意来自共享网络的图书馆名不能和 servers.xml 中的名字冲突。另外需要检查，不同的 UID，图书馆名字不能相同，如果发生冲突，则需要给分配 ..1 ..2 这样的编号以示区别
            // 需要一直保存一个 UID 到图书馆命的对照表在内存备用
            // TODO: 来自共享网络的记录，图标或 @ 后面的名字应该有明显的形态区别
            foreach (BiblioRecord record in e.Records)
            {
                string strXml = record.Data;

                string strMarcSyntax = "";
                string strBrowseText = "";
                string strColumnTitles = "";
                int nRet = BuildBrowseText(strXml,
out strBrowseText,
out strMarcSyntax,
out strColumnTitles,
out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strRecPath = record.RecPath + "@" + (string.IsNullOrEmpty(record.LibraryName) == false ? record.LibraryName : record.LibraryUID);

#if NO
                string strDbName = ListViewProperty.GetDbName(strRecPath);
                _browseTitleTable[strDbName] = strColumnTitles;
#endif
                _browseTitleTable[strMarcSyntax] = strColumnTitles;

                // 将书目记录放入 m_biblioTable
                {
                    BiblioInfo info = new BiblioInfo();
                    info.OldXml = strXml;
                    info.RecPath = strRecPath;
                    info.Timestamp = ByteArray.GetTimeStampByteArray(record.Timestamp);
                    info.Format = strMarcSyntax;
                    this.browseWindow.BiblioTable[strRecPath] = info;
                }

                List<string> column_list = StringUtil.SplitList(strBrowseText, '\t');
                string[] cols = new string[column_list.Count];
                column_list.CopyTo(cols);

                ListViewItem item = null;
                this.Invoke((Action)(() =>
                {
                    item = Global.AppendNewLine(
    this.browseWindow.RecordsList,
    strRecPath,
    cols);
                }
                ));

                if (item != null)
                    item.BackColor = Color.LightGreen;

#if NO
                RegisterBiblioInfo info = new RegisterBiblioInfo();
                info.OldXml = strXml;   // strMARC;
                info.Timestamp = ByteArray.GetTimeStampByteArray(record.Timestamp);
                info.RecPath = record.RecPath + "@" + (string.IsNullOrEmpty(record.LibraryName) == false ? record.LibraryName : record.LibraryUID);
                info.MarcSyntax = strMarcSyntax;
#endif
                _searchParam._searchCount++;
            }

            return;
        ERROR1:
            // 加入一个文本行
            {
                string[] cols = new string[1];
                cols[0] = strError;
                this.Invoke((Action)(() =>
                {

                    ListViewItem item = Global.AppendNewLine(
        this.browseWindow.RecordsList,
        "error",
        cols);
                }
    ));
            }
        }

        void CloseBrowseWindow()
        {
            if (this.browseWindow != null)
            {
                this.browseWindow.Close();
                this.browseWindow = null;
            }
        }

        // 显示 浏览小窗口
        // parameters:
        //      lHitCount   要显示出来的命中数。如果为 -1，则不显示命中数
        void ShowBrowseWindow(long lHitCount)
        {
#if NO
            try
            {
#endif
            // 2015/10/10
            if (browseWindow == null || browseWindow.IsDisposed == true)
                return;

                if (this.browseWindow.Visible == false)
                    this.MainForm.AppInfo.LinkFormState(this.browseWindow, "browseWindow_state");

            // 再观察一段 2015/9/8
                this.browseWindow.Visible = true;

                // 2014/7/8
                if (this.browseWindow.WindowState == FormWindowState.Minimized)
                    this.browseWindow.WindowState = FormWindowState.Normal;

                if (lHitCount != -1)
                    this.browseWindow.Text = "命中 " + lHitCount.ToString() + " 条书目记录。请从中选择一条";
        
#if NO
        }
            catch(System.ObjectDisposedException)
            {

            }
#endif
        }

        void ActivateBrowseWindow(bool bShow)
        {
            if (this.browseWindow == null
                || (this.browseWindow != null && this.browseWindow.IsDisposed == true))
            {
                this.browseWindow = new BrowseSearchResultForm();
                MainForm.SetControlFont(this.browseWindow, this.MainForm.DefaultFont);

                this.browseWindow.MainForm = this.MainForm; // 2009/2/17 
                this.browseWindow.Text = "命中多条种记录。请从中选择一条";
                this.browseWindow.FormClosing -= browseWindow_FormClosing;
                this.browseWindow.FormClosing += browseWindow_FormClosing;
                this.browseWindow.FormClosed -= new FormClosedEventHandler(browseWindow_FormClosed);
                this.browseWindow.FormClosed += new FormClosedEventHandler(browseWindow_FormClosed);
                // this.browseWindow.MdiParent = this.MainForm;
                if (bShow == true)
                {
                    this.MainForm.AppInfo.LinkFormState(this.browseWindow, "browseWindow_state");
                    this.browseWindow.Show();
                }

                this.browseWindow.OpenDetail -= new OpenDetailEventHandler(browseWindow_OpenDetail);
                this.browseWindow.OpenDetail += new OpenDetailEventHandler(browseWindow_OpenDetail);
            }
            else
            {
                if (this.browseWindow.Visible == false
                    && bShow == true)
                {
                    /*
                    this.MainForm.AppInfo.LinkFormState(this.browseWindow, "browseWindow_state");
                    this.browseWindow.Visible = true;
                     * */
                    ShowBrowseWindow(-1);
                }

                this.browseWindow.BringToFront();
                this.browseWindow.RecordsList.Items.Clear();
            }
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ObjectDisposedException
Message: 无法访问已释放的对象。
对象名:“BrowseSearchResultForm”。
Stack:
在 System.Windows.Forms.Control.CreateHandle()
在 System.Windows.Forms.Form.CreateHandle()
在 System.Windows.Forms.Control.get_Handle()
在 System.Windows.Forms.Control.SetVisibleCore(Boolean value)
在 System.Windows.Forms.Form.SetVisibleCore(Boolean value)
在 System.Windows.Forms.Control.set_Visible(Boolean value)
在 dp2Circulation.EntityForm.ShowBrowseWindow(Int64 lHitCount)
在 dp2Circulation.EntityForm.button_search_Click(Object sender, EventArgs e)
在 System.Windows.Forms.Control.OnClick(EventArgs e)
在 System.Windows.Forms.Button.OnClick(EventArgs e)
在 System.Windows.Forms.Button.OnMouseUp(MouseEventArgs mevent)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ButtonBase.WndProc(Message& m)
在 System.Windows.Forms.Button.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.4.5712.38964, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/24 14:12:46 (Mon, 24 Aug 2015 14:12:46 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
         * */
        void browseWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (stop != null && stop.State == 0 && e.CloseReason == CloseReason.UserClosing)    // 0 表示正在处理
            {
                // 如果是在检索中途关闭小窗口，则需要先设置 stop 为停止状态，不能直接关闭小窗口
                stop.DoStop();
                e.Cancel = true;
                // 通知检索循环结束后关闭小窗口
                _willCloseBrowseWindow = true;
            }
        }

        void browseWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (browseWindow != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(browseWindow);
                this.browseWindow = null;
            }
        }

        /*
        // 装入种
        void browseWindow_OpenDetail(object sender, OpenDetailEventArgs e)
        {
            if (e.Paths.Length == 0)
                return;

            string strBiblioRecPath = e.Paths[0];

            // 这里有一个重入的问题
            Debug.Assert(m_nInSearching == 0, "");


            this.LoadRecord(strBiblioRecPath);
        }
         */

        void browseWindow_OpenDetail(object sender, OpenDetailEventArgs e)
        {
            int nRet = 0;

            if ((e.Paths == null || e.Paths.Length == 0)
                && e.BiblioInfos != null && e.BiblioInfos.Count > 0)
            {

                BiblioInfo info = e.BiblioInfos[0];
                Debug.Assert(info != null, "");
                string strError = "";
                nRet = this.LoadRecord(info,
                    true,
                    out strError);
                if (nRet != 1)
                    MessageBox.Show(this, strError);
                return;
            }

            if (e.Paths.Length == 0)
                return;

            string strBiblioRecPath = e.Paths[0];

            // 这里有一个重入的问题
            // TODO: 其实这里检查已经没有必要了。因为新的LoadRecord()函数已经检查了m_nInSearching
            if (m_nInSearching > 0)
            {
                /*
                this.m_strTempBiblioRecPath = strBiblioRecPath;
                API.PostMessage(this.Handle, WM_PREPARE, 0, 0);
                 * */
                this.AddToPendingList(strBiblioRecPath, "");
                return;
            }


            nRet = this.LoadRecordOld(strBiblioRecPath, "", true);
            // 2009/11/6 
            if (nRet == 2)
            {
                this.AddToPendingList(strBiblioRecPath, "");
                return;
            }
        }

        // 
        /// <summary>
        /// 保存时是否自动查重
        /// </summary>
        public bool AutoSearchDup
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
    "entity_form",
    "search_dup_when_saving",
    false);
            }
        }

        // 
        /// <summary>
        /// 保存时是否自动校验数据。自动校验如果发现数据有错，既然保存
        /// </summary>
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

        /// <summary>
        /// 保存时是否强制校验数据。校验时如果发现数据有错，会拒绝保存
        /// </summary>
        public bool ForceVerifyData
        {
            get
            {
#if NO
                return this.MainForm.AppInfo.GetBoolean(
    "entity_form",
    "verify_data_when_saving",
    false);
#endif
                if (this.Channel == null)
                    return false;
                return StringUtil.IsInList("client_forceverifydata", this.Channel.Rights);
            }
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SEARCH_DUP:
                    this.SearchDup();
                    return;
                case WM_FILL_MARCEDITOR_SCRIPT_MENU:
                    // 显示Ctrl+A菜单
                    if (this.MainForm.PanelFixedVisible == true)
                        this._genData.AutoGenerate(this.m_marcEditor,
                            new GenerateDataEventArgs(),
                            GetBiblioRecPathOrSyntax(),
                            true);
                    return;
                case WM_VERIFY_DATA:
                    {
                        GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                        e1.FocusedControl = this.m_marcEditor;

                        this.VerifyData(this, e1, true);
                        return;
                    }
                case WM_SWITCH_FOCUS:
                    {
                        if ((int)m.WParam == BIBLIO_SEARCHTEXT)
                        {
                            this.textBox_queryWord.SelectAll();
                            this.textBox_queryWord.Focus();
                        }

                        else if ((int)m.WParam == ITEM_BARCODE)
                        {
                            this.textBox_itemBarcode.SelectAll();
                            this.textBox_itemBarcode.Focus();
                        }
                        else if ((int)m.WParam == ITEM_LIST)
                        {
                            bool bFound = this.ActivateItemsPage();
                            if (bFound == true)
                                this.entityControl1.Focus();
                        }
                        else if ((int)m.WParam == ORDER_LIST)
                        {
                            bool bFound = this.ActivateOrdersPage();
                            if (bFound == true)
                                this.orderControl1.Focus();
                        }
                        else if ((int)m.WParam == COMMENT_LIST)
                        {
                            bool bFound = this.ActivateCommentsPage();
                            if (bFound == true)
                                this.commentControl1.Focus();
                        }
                        else if ((int)m.WParam == ISSUE_LIST)
                        {
                            bool bFound = this.ActivateIssuesPage();
                            if (bFound == true)
                                this.issueControl1.Focus();
                        }
                        else if ((int)m.WParam == MARC_EDITOR)
                        {
                            if (this.m_marcEditor.FocusedFieldIndex == -1)
                                this.m_marcEditor.FocusedFieldIndex = 0;

                            if (this.m_marcEditor.Focused == false)
                                this.m_marcEditor.Focus();
                        }

                        return;
                    }
                // break;

            }
            base.DefWndProc(ref m);
        }

        // 种检索词textbox被触碰
        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;
        }

        private void textBox_queryWord_Leave(object sender, EventArgs e)
        {
            // 2008/12/15 
            this.AcceptButton = null;
        }

        private void textBox_itemBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_register;
            m_strFocusedPart = "itembarcode";
        }

        private void textBox_itemBarcode_Leave(object sender, EventArgs e)
        {
            // 2008/12/9 
            this.AcceptButton = null;
        }

        /// <summary>
        /// 是否要校验册条码号
        /// </summary>
        public bool NeedVerifyItemBarcode
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "entity_form",
                    "verify_item_barcode",
                    false);
            }
        }

#if NO
        // 形式校验条码号
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="strBarcode">要校验的条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-2  服务器没有配置校验方法，无法校验</para>
        /// <para>-1  出错</para>
        /// <para>0   不是合法的条码号</para>
        /// <para>1   是合法的读者证条码号</para>
        /// <para>2   是合法的册条码号</para>
        /// </returns>
        public int VerifyBarcode(
            string strBarcode,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在校验条码 ...");
            Progress.BeginLoop();

            /*
            this.Update();
            this.MainForm.Update();
             * */

            try
            {
                long lRet = Channel.VerifyBarcode(
                    Progress,
                    strBarcode,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.NotFound)
                        return -2;
                    goto ERROR1;
                }
                return (int)lRet;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                EnableControls(true);
            }
        ERROR1:
            return -1;
        }
#endif

        // 册登记
        private void button_register_Click(object sender, EventArgs e)
        {
            this.DoRegisterEntity();
        }

        // 2006/12/3 
        // 根据册条码号 装载一个册，连带装入种
        // parameters:
        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册条码号，装载书目记录
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <param name="bAutoSavePrev">是否自动保存窗口中先前的修改</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有找到</para>
        /// <para>1   找到</para>
        /// </returns>
        public int LoadItemByBarcode(string strItemBarcode,
            bool bAutoSavePrev)
        {
            if (string.IsNullOrEmpty(strItemBarcode) == true)
            {
                MessageBox.Show(this, "请先输入一个册条码号才能进行检索");
                return -1;
            }
            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {

                int nRet = 0;
                // TODO: 外部调用时，要能自动把items page激活

                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                {
                    if (bAutoSavePrev == false)
                    {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                            "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。\r\n\r\n在装入新的实体信息以前，是否先保存这些修改? ",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show(this, "放弃装入册事项 (册条码号为 '" + strItemBarcode + "' )");
                            return -1;
                        }
                        if (result == DialogResult.Yes)
                        {
                            /*
                            // 保存当前册信息
                            nRet = this.entityControl1.DoSaveEntities();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作
                             * */
                            nRet = this.DoSaveAll();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作

                        }
                    }
                    else
                    {
                        /*
                        // 保存当前册信息
                        nRet = this.entityControl1.DoSaveEntities();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                         * */
                        nRet = this.DoSaveAll();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                    }
                }


                // 2006/12/30 
                if (strItemBarcode != this.textBox_itemBarcode.Text)
                    this.textBox_itemBarcode.Text = strItemBarcode;

                // 注：如果所装入的item从属于和当前种不同的种，如果当前书目数据被修改过，会警告是否(破坏性)装入，但是书目数据不会被保存。这是一个问题。
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.entityControl1.DoSearchEntity(this.textBox_itemBarcode.Text);

                // 焦点切换到条码输入域
                // this.SwitchFocus(ITEM_BARCODE);

                this.SwitchFocus(ITEM_LIST);

                return nRet;
            }
            finally
            {
                this.m_nChannelInUse--;
            }
        }

        // 2008/11/2 
        // 根据册记录路径 装载一个册，连带装入种
        // parameters:
        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册记录路径，装入书目记录
        /// </summary>
        /// <param name="strItemRecPath">册记录路径</param>
        /// <param name="bAutoSavePrev">是否自动保存窗口中先前的修改</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有找到</para>
        /// <para>1   找到</para>
        /// </returns>
        public int LoadItemByRecPath(string strItemRecPath,
            bool bAutoSavePrev)
        {
            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {

                int nRet = 0;

                // TODO: 外部调用时，要能自动把items page激活

                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                {
                    if (bAutoSavePrev == false)
                    {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                            "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。\r\n\r\n在装入新的实体信息以前，是否先保存这些修改? ",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show(this, "放弃装入册事项 (册记录路径为 '" + strItemRecPath + "' )");
                            return -1;
                        }
                        if (result == DialogResult.Yes)
                        {
                            nRet = this.DoSaveAll();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作

                        }
                    }
                    else
                    {
                        nRet = this.DoSaveAll();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                    }
                }

                /*
                if (strItemBarcode != this.textBox_itemBarcode.Text)
                    this.textBox_itemBarcode.Text = strItemBarcode;
                 * */

                string strItemBarcode = "";
                BookItem result_item = null;

                // 注：如果所装入的item从属于和当前种不同的种，如果当前书目数据被修改过，会警告是否(破坏性)装入，但是书目数据不会被保存。这是一个问题。
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.entityControl1.DoSearchItemByRecPath(strItemRecPath,
                    out result_item,
                    false);
                if (result_item != null)
                    strItemBarcode = result_item.Barcode;

                if (strItemBarcode != this.textBox_itemBarcode.Text)
                    this.textBox_itemBarcode.Text = strItemBarcode;

                // 焦点切换到条码输入域
                // this.SwitchFocus(ITEM_BARCODE);
                this.SwitchFocus(ITEM_LIST);

                return nRet;
            }
            finally
            {
                this.m_nChannelInUse--;
            }
        }

        // 2010/2/26 
        // 根据册记录参考ID 装载一个册，连带装入种
        // parameters:
        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册记录参考 ID，转股书目记录
        /// </summary>
        /// <param name="strItemRefID">册记录的参考 ID</param>
        /// <param name="bAutoSavePrev">是否自动保存窗口中先前的修改</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有找到</para>
        /// <para>1   找到</para>
        /// </returns>
        public int LoadItemByRefID(string strItemRefID,
            bool bAutoSavePrev)
        {
            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {

                int nRet = 0;

                // TODO: 外部调用时，要能自动把items page激活

                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                {
                    if (bAutoSavePrev == false)
                    {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                            "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。\r\n\r\n在装入新的实体信息以前，是否先保存这些修改? ",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show(this, "放弃装入册事项 (册记录路径为 '" + strItemRefID + "' )");
                            return -1;
                        }
                        if (result == DialogResult.Yes)
                        {
                            nRet = this.DoSaveAll();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作

                        }
                    }
                    else
                    {
                        nRet = this.DoSaveAll();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                    }
                }

                string strItemBarcode = "";
                BookItem result_item = null;
                // 注：如果所装入的item从属于和当前种不同的种，如果当前书目数据被修改过，会警告是否(破坏性)装入，但是书目数据不会被保存。这是一个问题。
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.entityControl1.DoSearchItemByRefID(strItemRefID,
                    out result_item);
                if (result_item != null)
                    strItemBarcode = result_item.Barcode;

                if (strItemBarcode != this.textBox_itemBarcode.Text)
                    this.textBox_itemBarcode.Text = strItemBarcode;

                // 焦点切换到条码输入域
                // this.SwitchFocus(ITEM_BARCODE);
                this.SwitchFocus(ITEM_LIST);

                return nRet;
            }
            finally
            {
                this.m_nChannelInUse--;
            }
        }

        // 2011/6/30 
        // 根据评注记录路径 装载一个评注记录，连带装入种
        // parameters:
        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据评注记录路径，装入书目记录
        /// </summary>
        /// <param name="strCommentRecPath">评注记录路径</param>
        /// <param name="bAutoSavePrev">是否自动保存窗口中先前的修改</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有找到</para>
        /// <para>1   找到</para>
        /// </returns>
        public int LoadCommentByRecPath(string strCommentRecPath,
            bool bAutoSavePrev)
        {
            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {

                int nRet = 0;

                // TODO: 外部调用时，要能自动把items page激活

                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                {
                    if (bAutoSavePrev == false)
                    {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                            "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。\r\n\r\n在装入新的实体信息以前，是否先保存这些修改? ",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show(this, "放弃装入评注事项 (评注记录路径为 '" + strCommentRecPath + "' )");
                            return -1;
                        }
                        if (result == DialogResult.Yes)
                        {
                            nRet = this.DoSaveAll();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作

                        }
                    }
                    else
                    {
                        nRet = this.DoSaveAll();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                    }
                }

                CommentItem result_item = null;
                // 注：如果所装入的item从属于和当前种不同的种，如果当前书目数据被修改过，会警告是否(破坏性)装入，但是书目数据不会被保存。这是一个问题。
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.commentControl1.DoSearchItemByRecPath(strCommentRecPath,
                    out result_item);

                this.SwitchFocus(COMMENT_LIST);

                return nRet;
            }
            finally
            {
                this.m_nChannelInUse--;
            }
        }

        // 2009/11/23 
        // 根据订购册记录路径 装载一个订购记录，连带装入种
        // parameters:
        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据订购册记录路径，装入书目记录
        /// </summary>
        /// <param name="strOrderRecPath">订购记录路径</param>
        /// <param name="bAutoSavePrev">是否自动保存窗口中先前的修改</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有找到</para>
        /// <para>1   找到</para>
        /// </returns>
        public int LoadOrderByRecPath(string strOrderRecPath,
            bool bAutoSavePrev)
        {
            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {

                int nRet = 0;

                // TODO: 外部调用时，要能自动把items page激活

                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                {
                    if (bAutoSavePrev == false)
                    {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                            "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。\r\n\r\n在装入新的实体信息以前，是否先保存这些修改? ",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show(this, "放弃装入订购事项 (订购记录路径为 '" + strOrderRecPath + "' )");
                            return -1;
                        }
                        if (result == DialogResult.Yes)
                        {
                            nRet = this.DoSaveAll();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作

                        }
                    }
                    else
                    {
                        nRet = this.DoSaveAll();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                    }
                }

                /*
                if (strItemBarcode != this.textBox_itemBarcode.Text)
                    this.textBox_itemBarcode.Text = strItemBarcode;
                 * */

                OrderItem result_item = null;
                // 注：如果所装入的item从属于和当前种不同的种，如果当前书目数据被修改过，会警告是否(破坏性)装入，但是书目数据不会被保存。这是一个问题。
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.orderControl1.DoSearchItemByRecPath(strOrderRecPath,
                    out result_item);

                this.SwitchFocus(ORDER_LIST);

                return nRet;
            }
            finally
            {
                this.m_nChannelInUse--;
            }
        }

        // 2010/4/27
        // 根据期记录路径 装载一个期记录，连带装入种
        // parameters:
        //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据期记录路径，装载书目记录
        /// </summary>
        /// <param name="strIssueRecPath">期记录路径</param>
        /// <param name="bAutoSavePrev">是否自动保存窗口中先前的修改</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有找到</para>
        /// <para>1   找到</para>
        /// </returns>
        public int LoadIssueByRecPath(string strIssueRecPath,
            bool bAutoSavePrev)
        {
            this.m_nChannelInUse++;
            if (this.m_nChannelInUse > 1)
            {
                this.m_nChannelInUse--;
                MessageBox.Show(this, "通道已经被占用。请稍后重试");
                return -1;
            }
            try
            {

                int nRet = 0;

                // TODO: 外部调用时，要能自动把items page激活

                if (this.EntitiesChanged == true
                    || this.IssuesChanged == true
                    || this.BiblioChanged == true
                    || this.ObjectChanged == true
                    || this.OrdersChanged == true
                    || this.CommentsChanged == true)
                {
                    if (bAutoSavePrev == false)
                    {
                        // 警告尚未保存
                        DialogResult result = MessageBox.Show(this,
                            "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。\r\n\r\n在装入新的实体信息以前，是否先保存这些修改? ",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show(this, "放弃装入册事项 (册记录路径为 '" + strIssueRecPath + "' )");
                            return -1;
                        }
                        if (result == DialogResult.Yes)
                        {
                            nRet = this.DoSaveAll();
                            if (nRet == -1)
                                return -1; // 放弃进一步操作

                        }
                    }
                    else
                    {
                        nRet = this.DoSaveAll();
                        if (nRet == -1)
                            return -1; // 放弃进一步操作
                    }
                }

                /*
                if (strItemBarcode != this.textBox_itemBarcode.Text)
                    this.textBox_itemBarcode.Text = strItemBarcode;
                 * */

                IssueItem result_item = null;
                // 注：如果所装入的item从属于和当前种不同的种，如果当前书目数据被修改过，会警告是否(破坏性)装入，但是书目数据不会被保存。这是一个问题。
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = this.issueControl1.DoSearchItemByRecPath(strIssueRecPath,
                    out result_item);

                this.SwitchFocus(ISSUE_LIST);

                return nRet;
            }
            finally
            {
                this.m_nChannelInUse--;
            }
        }

        private void toolStripMenuItem_SearchOnly_Click(object sender, EventArgs e)
        {
            this.RegisterType = RegisterType.SearchOnly;
        }

        private void toolStripMenuItem_quickRegister_Click(object sender, EventArgs e)
        {
            this.RegisterType = RegisterType.QuickRegister;
        }

        private void toolStripMenuItem_register_Click(object sender, EventArgs e)
        {
            this.RegisterType = RegisterType.Register;
        }

        /// <summary>
        /// 登记按钮动作类型
        /// </summary>
        public RegisterType RegisterType
        {
            get
            {
                return m_registerType;
            }
            set
            {
                m_registerType = value;

                this.toolStripMenuItem_SearchOnly.Checked = false;
                this.toolStripMenuItem_quickRegister.Checked = false;
                this.toolStripMenuItem_register.Checked = false;


                if (m_registerType == RegisterType.SearchOnly)
                {
                    this.button_register.Text = "检索";
                    this.toolStripMenuItem_SearchOnly.Checked = true;
                }
                if (m_registerType == RegisterType.QuickRegister)
                {
                    this.button_register.Text = "快速登记";
                    this.toolStripMenuItem_quickRegister.Checked = true;
                }
                if (m_registerType == RegisterType.Register)
                {
                    this.button_register.Text = "登记";
                    this.toolStripMenuItem_register.Checked = true;
                }

            }
        }

        private void button_option_Click(object sender, EventArgs e)
        {
            EntityFormOptionDlg dlg = new EntityFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        // 清除书目、期、册、采购、对象信息窗口
        private void button_clear_Click(object sender, EventArgs e)
        {
            Clear(true);
        }

        /// <summary>
        /// 清除当前窗口内容
        /// </summary>
        /// <param name="bWarningNotSave">是否警告尚未保存的修改</param>
        /// <returns>true: 已经清除; false: 放弃清除</returns>
        public bool Clear(bool bWarningNotSave)
        {
            if (bWarningNotSave == true)
            {
                if (this.EntitiesChanged == true
        || this.IssuesChanged == true
        || this.OrdersChanged == true
        || this.CommentsChanged == true
        || this.BiblioChanged == true
        || this.ObjectChanged == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。若此时清除，现有未保存信息将丢失。\r\n\r\n确实要清除内容? ",
                        "EntityForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return false;   // canceled
                }
            }

            /*
            this.listView_items.Items.Clear();
            this.bookitems = null;
             * */
            this.entityControl1.ClearItems();

            this.issueControl1.ClearItems();

            this.orderControl1.ClearItems();

            this.commentControl1.ClearItems();

            this.TargetRecPath = "";

            this.ClearBiblio();

            // this.m_strTempBiblioRecPath = "";
            lock (this.m_listPendingLoadRequest)
            {
                this.m_listPendingLoadRequest.Clear();  // 2009/11/6 
            }

            if (this.m_verifyViewer != null)
                this.m_verifyViewer.Clear();

            if (this._genData != null)
                this._genData.ClearViewer();

            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();

            return true;    // cleared
        }

        private void EntityForm_Activated(object sender, EventArgs e)
        {
            // 2009/1/15 
            if (this.AcceptMode == true)
            {
#if ACCEPT_MODE
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
#endif
            }

            this.MainForm.stopManager.Active(this.Progress);

            this.MainForm.SetMenuItemState();

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = true;
            this.MainForm.MenuItem_logout.Enabled = true;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = true;

            this.MainForm.toolButton_refresh.Enabled = true;

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

        }


        void SwitchFocus(int target)
        {
            API.PostMessage(this.Handle,
                WM_SWITCH_FOCUS,
                target,
                0);
        }

        // 是否允许编目功能
        bool Cataloging
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(
                    "entity_form",
                    "cataloging",
                    true);  // 2007/12/2 修改为 true
            }
        }

        // 装载书目模板
        // return:
        //      -1  error
        //      0   放弃
        //      1   成功装载
        /// <summary>
        /// 装载书目模板
        /// </summary>
        /// <param name="bAutoSave">是否自动保存窗口内先前的修改</param>
        /// <returns>
        /// <para>-1: 出错</para>
        /// <para>0: 放弃</para>
        /// <para>1: 成功装载</para>
        /// </returns>
        public int LoadBiblioTemplate(bool bAutoSave = true)
        {
            int nRet = 0;

            // 按住 Shift 使用本功能，可重新出现对话框
            bool bShift = (Control.ModifierKeys == Keys.Shift);

            if (this.BiblioChanged == true
                || this.EntitiesChanged == true
                || this.IssuesChanged == true
                || this.OrdersChanged == true
                || this.CommentsChanged == true
                || this.ObjectChanged == true)
            {
                // 2008/6/25
                if (this.checkBox_autoSavePrev.Checked == true
                    && bAutoSave == true)
                {
                    nRet = this.DoSaveAll();
                    if (nRet == -1)
                        return -1;
                }
                else
                {

                    DialogResult result = MessageBox.Show(this,
                        "装载编目模板前,发现当前窗口中已有 " + GetCurrentChangedPartName() + " 修改后未来得及保存。是否要继续装载编目模板到窗口中(这样将丢失先前修改的内容)?\r\n\r\n(是)继续装载编目模板 (否)不装载编目模板",
                        "EntityForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        MessageBox.Show(this, "装载编目模板操作被放弃...");
                        return 0;
                    }
                }
            }

            string strSelectedDbName = this.MainForm.AppInfo.GetString(
                "entity_form",
                "selected_dbname_for_loadtemplate",
                "");

            SelectedTemplate selected = this.selected_templates.Find(strSelectedDbName);

            GetDbNameDlg dbname_dlg = new GetDbNameDlg();
            MainForm.SetControlFont(dbname_dlg, this.Font, false);
            if (selected != null)
            {
                dbname_dlg.NotAsk = selected.NotAskDbName;
                dbname_dlg.AutoClose = (bShift == true ? false : selected.NotAskDbName);
            }

            dbname_dlg.EnableNotAsk = true;
            dbname_dlg.DbName = strSelectedDbName;
            dbname_dlg.MainForm = this.MainForm;

            dbname_dlg.Text = "装载书目模板 -- 请选择目标编目库名";
            //  dbname_dlg.StartPosition = FormStartPosition.CenterScreen;

            this.MainForm.AppInfo.LinkFormState(dbname_dlg, "entityform_load_template_GetBiblioDbNameDlg_state");
            dbname_dlg.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(dbname_dlg);

            if (dbname_dlg.DialogResult != DialogResult.OK)
                return 0;

            string strBiblioDbName = dbname_dlg.DbName;
            // 记忆
            this.MainForm.AppInfo.SetString(
                "entity_form",
                "selected_dbname_for_loadtemplate",
                strBiblioDbName);

            selected = this.selected_templates.Find(strBiblioDbName);

            this.BiblioRecPath = dbname_dlg.DbName + "/?";	// 为了追加保存

            // 下载配置文件
            string strContent = "";
            string strError = "";

            // string strCfgFilePath = respath.Path + "/cfgs/template";
            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetCfgFileContent(strBiblioDbName,
                "template",
                out strContent,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                this.BiblioTimestamp = null;
                goto ERROR1;
            }

            // MessageBox.Show(this, strContent);

            SelectRecordTemplateDlg select_temp_dlg = new SelectRecordTemplateDlg();
            MainForm.SetControlFont(select_temp_dlg, this.Font, false);

            select_temp_dlg.Text = "请选择新书目记录模板 -- 来自书目库 '" + strBiblioDbName + "'";
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
            select_temp_dlg.EnableNotAsk = true;    // 2015/5/11

            nRet = select_temp_dlg.Initial(
                true, // true 表示也允许删除  // false,
                strContent,
                out strError);
            if (nRet == -1)
            {
                strError = "装载配置文件 '" + "template" + "' 发生错误: " + strError;
                goto ERROR1;
            }

            this.MainForm.AppInfo.LinkFormState(select_temp_dlg, "entityform_load_template_SelectTemplateDlg_state");
            select_temp_dlg.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(select_temp_dlg);

            if (select_temp_dlg.DialogResult != DialogResult.OK)
                return 0;

            if (select_temp_dlg.Changed == true)
            {
                // return:
                //      -1  出错
                //      0   没有必要保存
                //      1   成功保存
                nRet = SaveTemplateChange(select_temp_dlg,
                    strBiblioDbName,
                    baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.MainForm.StatusBarMessage = "修改模板成功。";
                return 1;
            }

            // 记忆本次的选择，下次就不用再进入本对话框了
            this.selected_templates.Set(strBiblioDbName,
                dbname_dlg.NotAsk,
                select_temp_dlg.SelectedName,
                select_temp_dlg.NotAsk);

            this.BiblioTimestamp = null;
            // this.m_strMetaData = "";	// 记忆XML记录的元数据

            this.BiblioOriginPath = ""; // 保存从数据库中来的原始path

            // this.TimeStamp = baTimeStamp;

            // this.Text = respath.ReverseFullPath; // 窗口标题

            // return:
            //      -1  error
            //      0   空的记录
            //      1   成功
            nRet = SetBiblioRecordToMarcEditor(select_temp_dlg.SelectedRecordXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

#if NO
            Global.SetHtmlString(this.webBrowser_biblioRecord,
                "(空白)");
#endif
            this.m_webExternalHost_biblio.SetHtmlString("(空白)",
    "entityform_error");

            // 对象tabpage清空 2009/1/5 
            this.binaryResControl1.Clear();

            // 册tabpage是否显示
            string strItemDbName = this.MainForm.GetItemDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strItemDbName) == false)
            {
                this.EnableItemsPage(true);
            }
            else
            {
                this.EnableItemsPage(false);
            }

            this.entityControl1.ClearItems();

            // 期tabpage是否显示
            string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strIssueDbName) == false)
            {
                this.EnableIssuesPage(true);
            }
            else
            {
                this.EnableIssuesPage(false);
            }

            this.issueControl1.ClearItems();

            // 订购tabpage是否显示
            string strOrderDbName = this.MainForm.GetOrderDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strOrderDbName) == false) // 仅在当前书目库有对应的采购库时，才装入采购记录
            {
                if (String.IsNullOrEmpty(strIssueDbName) == false)
                    this.orderControl1.SeriesMode = true;
                else
                    this.orderControl1.SeriesMode = false;

                this.EnableOrdersPage(true);
            }
            else
            {
                this.EnableOrdersPage(false);
            }

            this.orderControl1.ClearItems();

            // 评注tabpage是否显示
            string strCommentDbName = this.MainForm.GetCommentDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strCommentDbName) == false)
            {
                this.EnableCommentsPage(true);
            }
            else
            {
                this.EnableCommentsPage(false);
            }

            this.commentControl1.ClearItems();

            // 2007/11/5 
            this.DeletedMode = false;

            this.BiblioChanged = false;

            // ****
            this.toolStripButton_marcEditor_save.Enabled = true;

            // 用模板的时候，无论如何ReadOnly都是false
            if (this.m_marcEditor.ReadOnly == true)
                this.m_marcEditor.ReadOnly = false;

            // 2008/11/30 
            SwitchFocus(MARC_EDITOR);
            if (dbname_dlg.NotAsk == true || select_temp_dlg.NotAsk == true)
            {
                this.MainForm.StatusBarMessage = "自动从书目库 " + strBiblioDbName + " 中装入名为 " + select_temp_dlg.SelectedName + " 的新书目记录模板。如要重新出现装载对话框，请按住Shift键再点“装载书目模板”按钮...";
            }
            return 1;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        /*
        // 从路径中析出数据库名
        static string GetDbName(string strPath)
        {
            int nRet = strPath.IndexOf("/");
            if (nRet == -1)
                return strPath;
            return strPath.Substring(0, nRet);
        }*/

        // 获得当前记录的MARC格式
        // 2009/3/4 
        // return:
        //      null    因为当前记录路径为空，无法获得MARC格式
        //      其他    MARC格式。为"unimarc" "usnmarc" 之一
        /// <summary>
        /// 获得当前记录的MARC格式
        /// </summary>
        /// <returns>
        /// <para>null: 因为当前记录路径为空，无法获得MARC格式</para>
        /// <para>其他: MARC格式。为"unimarc" "usnmarc" 之一</para>
        /// </returns>
        public string GetCurrentMarcSyntax()
        {
            string strMarcSyntax = "";

            // 获得库名，根据库名得到marc syntax
            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            if (String.IsNullOrEmpty(strBiblioDbName) == false)
                strMarcSyntax = MainForm.GetBiblioSyntax(strBiblioDbName);
            else
                return null;    // 无法得到，因为当前没有书目库路径

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            return strMarcSyntax;
        }

        // 获得书目记录的XML格式
        // parameters:
        //      strBiblioDbName 书目库名。用来辅助决定要创建的XML记录的marcsyntax。如果此参数==null，表示会从this.BiblioRecPath中去取书目库名
        //      bIncludeFileID  是否要根据当前rescontrol内容合成<dprms:file>元素?
        int GetBiblioXml(
            string strBiblioDbName,
            bool bIncludeFileID,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strMarcSyntax = "";

            // 获得库名，根据库名得到marc syntax
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
                strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            if (String.IsNullOrEmpty(strBiblioDbName) == false)
                strMarcSyntax = MainForm.GetBiblioSyntax(strBiblioDbName);

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            // 2008/5/16 changed
            string strMARC = this.GetMarc();    //  this.m_marcEditor.Marc;
            XmlDocument domMarc = null;
            int nRet = MarcUtil.Marc2Xml(strMARC,
                strMarcSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 因为domMarc是根据MARC记录合成的，所以里面没有残留的<dprms:file>元素，也就没有(创建新的id前)清除的需要

            Debug.Assert(domMarc != null, "");

            // 合成<dprms:file>元素
            if (this.binaryResControl1 != null
                && bIncludeFileID == true)  // 2008/12/3 
            {
#if NO
                List<string> ids = this.binaryResControl1.GetIds();
                List<string> usages = this.binaryResControl1.GetUsages();

                Debug.Assert(ids.Count == usages.Count, "");

                for (int i = 0; i < ids.Count; i++)
                {
                    string strID = ids[i];
                    if (String.IsNullOrEmpty(strID) == true)
                        continue;

                    string strUsage = usages[i];

                    XmlNode node = domMarc.CreateElement("dprms",
                        "file",
                        DpNs.dprms);
                    domMarc.DocumentElement.AppendChild(node);
                    DomUtil.SetAttr(node, "id", strID);
                    if (string.IsNullOrEmpty(strUsage) == false)
                        DomUtil.SetAttr(node, "usage", strUsage);
                }
#endif
                // 在 XmlDocument 对象中添加 <file> 元素。新元素加入在根之下
                nRet = this.binaryResControl1.AddFileFragments(ref domMarc,
            out strError);
                if (nRet == -1)
                    return -1;
            }

            // 合成其它XML片断
            if (domXmlFragment != null
                && string.IsNullOrEmpty(domXmlFragment.DocumentElement.InnerXml) == false)
            {
                XmlDocumentFragment fragment = domMarc.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = domXmlFragment.DocumentElement.InnerXml;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                domMarc.DocumentElement.AppendChild(fragment);
            }

            strXml = domMarc.OuterXml;
            return 0;
        }

        // 把当前书目记录复制到目标位置
        // parameters:
        //      strAction   动作。为"onlycopybiblio" "onlymovebiblio"之一。增加 copy / move
        /// <summary>
        /// 把当前书目记录复制到目标位置
        /// </summary>
        /// <param name="strAction">动作。为 copy / move / onlycopybiblio / onlymovebiblio 之一</param>
        /// <param name="strTargetBiblioRecPath">目标书目记录路径</param>
        /// <param name="strMergeStyle">合并方式</param>
        /// <param name="strXml">返回书目记录 XML</param>
        /// <param name="strOutputBiblioRecPath">实际写入的书目记录路径</param>
        /// <param name="baOutputTimestamp">目标记录的最新时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int CopyBiblio(
            string strAction,
            string strTargetBiblioRecPath,
            string strMergeStyle,
            out string strXml,  // 顺便返回书目记录XML格式
            out string strOutputBiblioRecPath,
            out byte [] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            strXml = "";
            baOutputTimestamp = null;
            strOutputBiblioRecPath = "";
            int nRet = 0;

            string strOldMarc = this.GetMarc();    //  this.m_marcEditor.Marc;
            bool bOldChanged = this.GetMarcChanged();   //  this.m_marcEditor.Changed;

            try
            {

                // 2011/11/28
                // 保存前的准备工作
                {
                    // 初始化 dp2circulation_marc_autogen.cs 的 Assembly，并new DetailHost对象
                    // return:
                    //      -1  error
                    //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                    //      1   重新(或者首次)初始化了Assembly
                    nRet = this._genData.InitialAutogenAssembly(strTargetBiblioRecPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (this._genData.DetailHostObj != null)
                    {
                        BeforeSaveRecordEventArgs e = new BeforeSaveRecordEventArgs();
                        // this._genData.DetailHostObj.BeforeSaveRecord(this.m_marcEditor, e);
                        this._genData.DetailHostObj.Invoke("BeforeSaveRecord", this.m_marcEditor, e);
                        if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                        {
                            MessageBox.Show(this, "保存前的准备工作失败: " + e.ErrorInfo + "\r\n\r\n但保存操作仍将继续");
                        }
                    }
                }

                // 获得书目记录XML格式
                strXml = "";
                nRet = this.GetBiblioXml(
                    "", // 迫使从记录路径中看marc格式
                    true,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;

            }
            finally
            {
                // 复原当前窗口的记录
                if (this.GetMarc() /*this.m_marcEditor.Marc*/ != strOldMarc)
                {
                    // this.m_marcEditor.Marc = strOldMarc;
                    this.SetMarc(strOldMarc);
                }
                if (this.GetMarcChanged() /*this.m_marcEditor.Changed*/ != bOldChanged)
                {
                    // this.m_marcEditor.Changed = bOldChanged;
                    this.SetMarcChanged(bOldChanged);
                }
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在复制书目记录 ...");
            Progress.BeginLoop();

            try
            {
                string strOutputBiblio = "";

                // result.Value:
                //      -1  出错
                //      0   成功，没有警告信息。
                //      1   成功，有警告信息。警告信息在 result.ErrorInfo 中
                long lRet = this.Channel.CopyBiblioInfo(
                    this.Progress,
                    strAction,
                    this.BiblioRecPath,
                    "xml",
                    null,
                    this.BiblioTimestamp,
                    strTargetBiblioRecPath,
                    null,   // strXml,
                    strMergeStyle,
                    out strOutputBiblio,
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 1)
                {
                    // 有警告
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 0;
        }

        // 2013/12/2
        /// <summary>
        /// 获得脚本对象
        /// </summary>
        public IDetailHost HostObject
        {
            get
            {
                string strError = "";
                // 初始化 dp2circulation_marc_autogen.cs 的 Assembly，并new DetailHost对象
                // return:
                //      -1  error
                //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                //      1   重新(或者首次)初始化了Assembly
                int nRet = this._genData.InitialAutogenAssembly(this.BiblioRecPath, // null,
                    out strError);
                /*
                if (nRet == -1)
                    throw new Exception(strError);
                 * */
                return this._genData.DetailHostObj;
            }
        }

        // 保存书目记录到数据库
        // parameters:
        //      bIncludeFileID  (书目记录XML)是否要根据当前rescontrol内容合成<dprms:file>元素?
        // return:
        //      -1  出错
        //      0   没有保存
        //      1   已经保存
        /// <summary>
        /// 保存书目记录到数据库
        /// </summary>
        /// <param name="bIncludeFileID">(书目记录XML)是否要根据当前对象控件内容合成&lt;dprms:file&gt;元素?</param>
        /// <param name="strHtml">返回新记录的 OPAC 格式内容</param>
        /// <param name="strStyle">风格。由 displaysuccess / searchdup 之一或者逗号间隔组合而成。displaysuccess 显示最后的成功消息在框架窗口的状态条; searchdup 保存成功后发送查重消息</param>
        /// <returns>
        /// <para>-1  出错</para>
        /// <para>0   没有保存</para>
        /// <para>1   已经保存</para>
        /// </returns>
        public int SaveBiblioToDatabase(bool bIncludeFileID,
            out string strHtml,
            string strStyle = "displaysuccess,searchdup")
        {
            string strError = "";
            strHtml = "";
            int nRet = 0;

            bool bDisplaySuccess = StringUtil.IsInList("displaysuccess", strStyle);
            bool bSearchDup = StringUtil.IsInList("searchdup", strStyle);

            if (this.Cataloging == false)
            {
                strError = "当前不允许编目功能，因此也不允许保存书目信息的功能";
                return -1;
            }

            // 如果刚才在删除后模式，现在取消这个模式 2007/10/15
            if (this.DeletedMode == true)
            {
                // TODO: 除了册信息，也要考虑期、采购信息
                int nEntityCount = this.entityControl1.ItemCount;

                if (nEntityCount > 0)
                {
                    DialogResult result = MessageBox.Show(this,
"如果您用本功能将刚删除的书目记录保存回数据库，那么书目记录下属的 "
+ nEntityCount.ToString()
+ " 条实体记录将不会被保存回实体库。\r\n\r\n如果要在保存书目数据的同时也完整保存这些被删除的实体记录，请先在种册窗工具条上选择“.../使能编辑保存”功能，然后再使用“全部保存”按钮"
+ "\r\n\r\n是否要在不保存实体记录的情况下单独保存书目记录？ (Yes 是 / No 放弃单独保存书目记录的操作)",
"EntityForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        strError = "放弃保存书目记录";
                        goto ERROR1;
                    }
                }
            }

            string strTargetPath = this.BiblioRecPath;
            if (string.IsNullOrEmpty(strTargetPath) == true)
            {
                // 需要询问保存的路径
                BiblioSaveToDlg dlg = new BiblioSaveToDlg();
                MainForm.SetControlFont(dlg, this.Font, false);

                dlg.MainForm = this.MainForm;
                dlg.Text = "仅保存书目记录";
                dlg.MessageText = "请指定新书目记录要保存到的位置";
                dlg.EnableCopyChildRecords = false;

                dlg.BuildLink = false;

                dlg.CopyChildRecords = false;

                {
                    string strMarcSyntax = this.GetCurrentMarcSyntax();
                    if (string.IsNullOrEmpty(strMarcSyntax) == true)
                        strMarcSyntax = this.MarcSyntax;    // 外来数据的 MARC 格式

                    dlg.MarcSyntax = strMarcSyntax;
                }

                dlg.CurrentBiblioRecPath = this.BiblioRecPath;
                this.MainForm.AppInfo.LinkFormState(dlg, "entityform_BiblioSaveToDlg_state");
                dlg.ShowDialog(this);
                // this.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult != DialogResult.OK)
                    return 0;

                strTargetPath = dlg.RecPath;
            }

            // 保存前的准备工作
            {
                // 初始化 dp2circulation_marc_autogen.cs 的 Assembly，并new DetailHost对象
                // return:
                //      -1  error
                //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                //      1   重新(或者首次)初始化了Assembly
                nRet = this._genData.InitialAutogenAssembly(strTargetPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (this._genData.DetailHostObj != null)
                {
                    BeforeSaveRecordEventArgs e = new BeforeSaveRecordEventArgs();
                    // this._genData.DetailHostObj.BeforeSaveRecord(this.m_marcEditor, e);
                    this._genData.DetailHostObj.Invoke("BeforeSaveRecord", this.m_marcEditor, e);
                    if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                    {
                        MessageBox.Show(this, "保存前的准备工作失败: " + e.ErrorInfo + "\r\n\r\n但保存操作仍将继续");
                    }
                }
            }

            // 获得书目记录XML格式
            string strXmlBody = "";
            nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                bIncludeFileID,
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            bool bPartialDenied = false;
            string strOutputPath = "";
            byte[] baNewTimestamp = null;
            string strWarning = "";
            nRet = SaveXmlBiblioRecordToDatabase(strTargetPath,
                this.DeletedMode == true,
                strXmlBody,
                this.BiblioTimestamp,
                out strOutputPath,
                out baNewTimestamp,
                out strWarning,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (string.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);
            if (Channel.ErrorCode == ErrorCode.PartialDenied)
                bPartialDenied = true;

            this.BiblioTimestamp = baNewTimestamp;
            this.BiblioRecPath = strOutputPath;
            this.BiblioOriginPath = strOutputPath;

            this.BiblioChanged = false;

            // 如果刚才在删除后模式，现在取消这个模式 2007/10/15
            if (this.DeletedMode == true)
            {
                this.DeletedMode = false;

                // 重新装载实体记录，以便反映其listview变空的事实
                // 接着装入相关的所有册
                nRet = this.entityControl1.LoadItemRecords(
                    this.BiblioRecPath,
                    // this.DisplayOtherLibraryItem,
                    this.DisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 清除ReadOnly状态，如果998$t已经消失
            if (this.m_marcEditor.ReadOnly == true)
            {
                string strTargetBiblioRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");
                if (String.IsNullOrEmpty(strTargetBiblioRecPath) == true)
                    this.m_marcEditor.ReadOnly = false;
            }

            if (bDisplaySuccess == true)
            {
                this.MainForm.StatusBarMessage = "书目记录 '" + this.BiblioRecPath + "' 保存成功";
                // MessageBox.Show(this, "书目记录保存成功。");
            }

            if (bSearchDup == true)
            {
                if (this.AutoSearchDup == true)
                    API.PostMessage(this.Handle, WM_SEARCH_DUP, 0, 0);
            }

            // if (bPartialDenied == true)
            {
                // 获得实际保存的书目记录
                string[] results = null;
                string[] formats = null;
                if (bPartialDenied == true)
                {
                    formats = new string[2];
                    formats[0] = "html";
                    formats[1] = "xml";
                }
                else
                {
                    formats = new string[1];
                    formats[0] = "html";
                }
                long lRet = Channel.GetBiblioInfos(
    Progress,
    strOutputPath,
    "",
    formats,
    out results,
    out baNewTimestamp,
    out strError);
                if (lRet == 0)
                {
                    strError = "重新装载时，路径为 '" + strOutputPath + "' 的书目记录没有找到 ...";
                    goto ERROR1;
                }
                if (results == null)
                {
                    strError = "重新装载书目记录时出错: result == null";
                    goto ERROR1;
                }

                {
                    // 重新显示 OPAC 书目信息
                    // TODO: 需要在对象保存完以后发出这个指令
                    Debug.Assert(results.Length >= 1, "");
                    if (results.Length > 0)
                    {
                        strHtml = results[0];
#if NO
                        this.m_webExternalHost_biblio.SetHtmlString(strHtml,
                            "entityform_biblio");
#endif
                    }
                }

                DoViewComment(false);   // 重新显示固定面板区的属性 XML 2015/7/11

                if (bPartialDenied == true)
                {
                    if (results.Length < 2)
                    {
                        strError = "重新装载书目记录时出错: result.Length["+results.Length.ToString()+"] 小于 2";
                        goto ERROR1;
                    }
                    PartialDeniedDialog dlg = new PartialDeniedDialog();

                    MainForm.SetControlFont(dlg, this.Font, false);
                    dlg.SavingXml = strXmlBody;
                    Debug.Assert(results.Length >= 2, "");
                    dlg.SavedXml = results[1];
                    dlg.MainForm = this.MainForm;

                    this.MainForm.AppInfo.LinkFormState(dlg, "PartialDeniedDialog_state");
                    dlg.ShowDialog(this);
                    this.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (dlg.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        string strOutputBiblioRecPath = "";
                        string strXml = "";
                        // 将实际保存的记录装入 MARC 编辑器
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   found
                        nRet = LoadBiblioRecord(strOutputPath,
                            "",
                            false,
                            out strOutputBiblioRecPath,
                            out strXml,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "重新装载书目记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }
                }
            }
            return 1;

        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        /// <summary>
        /// 当前是否处在删除后的状态
        /// </summary>
        public bool DeletedMode
        {
            get
            {
                return this.m_bDeletedMode;
            }
            set
            {
                this.m_bDeletedMode = value;

                this.SetSaveAllButtonState(true);
                this.EnableControls(true);  // 2009/11/11 
            }
        }

        // 
        /// <summary>
        /// 删除书目记录 和 下属的册、期、订购、对象记录
        /// </summary>
        public void DeleteBiblioFromDatabase()
        {
            string strError = "";

            List<string> subRecord_warnings = new List<string>();

            int nEntityCount = this.entityControl1.ItemCount;
            if (nEntityCount != 0)
                subRecord_warnings.Add(nEntityCount.ToString() + " 个册记录");

            int nIssueCount = this.issueControl1.ItemCount;
            if (nIssueCount != 0)
                subRecord_warnings.Add(nIssueCount.ToString() + " 个期记录");

            int nOrderCount = this.orderControl1.ItemCount;
            if (nOrderCount != 0)
                subRecord_warnings.Add(nOrderCount.ToString() + " 个采购记录");

            int nCommentCount = this.commentControl1.ItemCount;
            if (nCommentCount != 0)
                subRecord_warnings.Add(nCommentCount.ToString() + " 个评注记录");

            if (subRecord_warnings.Count > 0)
            {
                // 检查前端权限
                if (StringUtil.IsInList("client_deletebibliosubrecords", this.Channel.Rights) == false)
                {
                    strError = "书目记录 " + this.BiblioRecPath + " 包含下属的 "
                        + StringUtil.MakePathList(subRecord_warnings, "、")
                        + "，但当前用户并不具备 client_deletebibliosubrecords 权限，因此无法进行删除书目记录的操作";
                    goto ERROR1;
                }
            }

            string strChangedWarning = "";

            // 先检查实体listview中是否有new change deleted事项，如果有，警告
            if (this.EntitiesChanged == true
                || this.IssuesChanged == true
                || this.OrdersChanged == true
                || this.CommentsChanged == true
                || this.ObjectChanged == true
                || this.BiblioChanged == true)
            {

                strChangedWarning = "当前有 "
                    + GetCurrentChangedPartName()
                + " 被修改过。\r\n\r\n";
            }

            string strText = strChangedWarning;

            strText += "确实要删除书目记录 " +this.BiblioRecPath + " ";

            if (subRecord_warnings.Count > 0)
                strText += "和下属的 " + StringUtil.MakePathList(subRecord_warnings, "、");
#if NO
            int nEntityCount = this.entityControl1.ItemCount;
            if (nEntityCount != 0)
                strText += "和下属的 " + nEntityCount.ToString() + " 个册记录";

            int nIssueCount = this.issueControl1.ItemCount;
            if (nIssueCount != 0)
                strText += "和下属的 " + nIssueCount.ToString() + " 个期记录";

            int nOrderCount = this.orderControl1.ItemCount;
            if (nOrderCount != 0)
                strText += "和下属的 " + nOrderCount.ToString() + " 个采购记录";

            int nCommentCount = this.commentControl1.ItemCount;
            if (nCommentCount != 0)
                strText += "和下属的 " + nCommentCount.ToString() + " 个评注记录";
#endif

            int nObjectCount = this.binaryResControl1.ObjectCount;
            if (nObjectCount != 0)
                strText += " 和从属的 " + nObjectCount.ToString() + " 个对象";

            strText += " ?";

            // 警告删除
            DialogResult result = MessageBox.Show(this,
                strText,
                "EntityForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            int nRet = DeleteBiblioRecordFromDatabase(this.BiblioRecPath,
                "delete",
                this.BiblioTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // this.BiblioTimestamp = null;
            // this.textBox_biblioRecPath.Text = strOutputPath;
            // this.BiblioOriginPath = strOutputPath;


            this.BiblioChanged = false; // 避免关闭窗口时警告有修改内容
            this.DeletedMode = true;



            string strMessage = "书目记录 '" + this.BiblioRecPath + "' ";

            if (nEntityCount != 0)
            {
                // 册信息残留在listview中，还可以保存回去，所以不清除

                strMessage += "和 下属的实体记录 ";
            }

            if (nIssueCount != 0)
            {
                // 期信息残留在listview中，还可以保存回去，所以不清除

                strMessage += "和 下属的期记录 ";
            }

            if (nOrderCount != 0)
            {
                // 采购信息残留在listview中，还可以保存回去，所以不清除

                strMessage += "和 下属的采购记录 ";
            }

            if (nCommentCount != 0)
            {
                // 评注信息残留在listview中，还可以保存回去，所以不清除

                strMessage += "和 下属的评注记录 ";
            }

            if (nObjectCount != 0)
            {
                // 对象信息无法保存回去，所以清除
                this.binaryResControl1.Clear();

                strMessage += "和 从属的对象 ";
            }

            strMessage += "删除成功";

            this.MainForm.StatusBarMessage = strMessage;

            this.SetSaveAllButtonState(true);
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }


        // 
        /// <summary>
        /// 保存当前窗口内记录到模板配置文件
        /// </summary>
        public void SaveBiblioToTemplate()
        {
            // 获得路径行中已经有的书目库名
            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            GetDbNameDlg dlg = new GetDbNameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.DbName = strBiblioDbName;
            dlg.MainForm = this.MainForm;
            dlg.Text = "请选择目标编目库名";
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            strBiblioDbName = dlg.DbName;


            // 下载模板配置文件
            string strContent = "";
            string strError = "";

            // string strCfgFilePath = respath.Path + "/cfgs/template";
            byte [] baTimestamp = null;

            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetCfgFileContent(strBiblioDbName,
                "template",
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
                true,   // 允许修改
                strContent, 
                out strError);
            if (nRet == -1)
                goto ERROR1;


            tempdlg.Text = "请选择要修改的模板记录";
            tempdlg.CheckNameExist = false;	// 按OK按钮时不警告"名字不存在",这样允许新建一个模板
            //tempdlg.ap = this.MainForm.applicationInfo;
            //tempdlg.ApCfgTitle = "detailform_selecttemplatedlg";
            tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return;

            // return:
            //      -1  出错
            //      0   没有必要保存
            //      1   成功保存
            nRet = SaveTemplateChange(tempdlg,
                strBiblioDbName,
                baTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

#if NO
            // 获得书目记录XML格式
            string strXmlBody = "";
            nRet = this.GetBiblioXml(
                strBiblioDbName,
                false,  // 不要包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 修改配置文件内容
            if (tempdlg.textBox_name.Text != "")
            {
                // 替换或者追加一个记录
                nRet = tempdlg.ReplaceRecord(tempdlg.textBox_name.Text,
                    strXmlBody,
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
            nRet = SaveCfgFile(strBiblioDbName,
                "template",
                strOutputXml,
                baTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            this.MainForm.StatusBarMessage = "修改模板成功。";
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   成功保存
        int SaveTemplateChange(SelectRecordTemplateDlg tempdlg,
            string strBiblioDbName,
            byte[] baTimestamp,
            out string strError)
        {
            strError = "";

            if (tempdlg.Changed == false    // DOM 内容没有变化
                && tempdlg.textBox_name.Text == "")	// 没有选定要保存的模板名
                return 0;


            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                strBiblioDbName,
                false,  // 不要包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 修改配置文件内容
            if (tempdlg.textBox_name.Text != "")
            {
                // 替换或者追加一个记录
                nRet = tempdlg.ReplaceRecord(tempdlg.textBox_name.Text,
                    strXmlBody,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            string strOutputXml = tempdlg.OutputXml;

            // Debug.Assert(false, "");
            nRet = SaveCfgFile(strBiblioDbName,
                "template",
                strOutputXml,
                baTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 1;
        ERROR1:
            return -1;
        }

        // 保存XML格式的书目记录到数据库
        // parameters:
        //      bResave 是否为删除后重新保存的模式。在这种模式下，使用 strAction == "new"
        int SaveXmlBiblioRecordToDatabase(string strPath,
            bool bResave,
            string strXml,
            byte[] baTimestamp,
            out string strOutputPath,
            out byte[] baNewTimestamp,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";
            baNewTimestamp = null;
            strOutputPath = "";

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在保存书目记录 ...");
            Progress.BeginLoop();

            try
            {
                string strAction = "change";

                if (Global.IsAppendRecPath(strPath) == true || bResave == true)
                    strAction = "new";

                /*
                if (String.IsNullOrEmpty(strPath) == true)
                    strAction = "new";
                else
                {
                    string strRecordID = Global.GetRecordID(strPath);
                    if (String.IsNullOrEmpty(strRecordID) == true
                        || strRecordID == "?")
                        strAction = "new";
                }
                */
                REDO:
                long lRet = Channel.SetBiblioInfo(
                    Progress,
                    strAction,
                    strPath,
                    "xml",
                    strXml,
                    baTimestamp,
                    "",
                    out strOutputPath,
                    out baNewTimestamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存书目记录 '" + strPath + "' 时出错: " + strError;
                    if (strAction == "change" && Channel.ErrorCode == ErrorCode.NotFound)
                    {
                        strError = "保存书目记录 '" + strPath + "' 时出错: 原记录已经不存在";
                        DialogResult result = MessageBox.Show(this,
strError + "\r\n\r\n请问是否改为重新创建此记录?",
"EntityForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            strAction = "new";
                            // TODO: 此时也需要进一步保存下属的册记录等。而且对象资源不可能恢复了
                            goto REDO;
                        }
                    }

                    goto ERROR1;
                }
                if (Channel.ErrorCode == ErrorCode.PartialDenied)
                {
                    strWarning = "书目记录 '" + strPath + "' 保存成功，但所提交的字段部分被拒绝 ("+strError+")。请留意刷新窗口，检查实际保存的效果";
                }
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 从数据库中删除书目记录
        int DeleteBiblioRecordFromDatabase(string strPath,
            string strAction,
            byte [] baTimestamp,
            out string strError)
        {
            strError = "";

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在删除书目记录 ...");
            Progress.BeginLoop();

            try
            {
                string strOutputPath = "";
                byte[] baNewTimestamp = null;

                long lRet = Channel.SetBiblioInfo(
                    Progress,
                    strAction,  // "delete",
                    strPath,
                    "xml",
                    "", // strXml,
                    baTimestamp,
                    "",
                    out strOutputPath,
                    out baNewTimestamp,
                    out strError);
                if (lRet == -1)
                {
                    // 删除失败时也不要忘记了更新时间戳
                    // 这样如果遇到时间戳不匹配，下次重试删除即可?
                    if (baNewTimestamp != null)
                        this.BiblioTimestamp = baNewTimestamp;
                    goto ERROR1;
                }

            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 工具条按钮：装载模板
        private void toolStripButton_marcEditor_loadTemplate_Click(object sender, EventArgs e)
        {
            this.toolStrip_marcEditor.Enabled = false;
            try
            {
                LoadBiblioTemplate(true);
            }
            finally
            {
                this.toolStrip_marcEditor.Enabled = true;
            }
        }

        // 工具条按钮：保存到模板
        private void toolStripButton_marcEditor_saveTemplate_Click(object sender, EventArgs e)
        {
            this.toolStrip_marcEditor.Enabled = false;
            try
            {
                SaveBiblioToTemplate();
            }
            finally
            {
                this.toolStrip_marcEditor.Enabled = true;
            }

        }

        // 工具条按钮: 保存记录到数据库
        private void toolStripButton_marcEditor_save_Click(object sender, EventArgs e)
        {
            this.toolStrip_marcEditor.Enabled = false;
            try
            {
                // 2014/7/3
                bool bVerifyed = false;

                if (this.m_verifyViewer != null)
                    this.m_verifyViewer.Clear();

                if (this.ForceVerifyData == true)
                {
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    e1.FocusedControl = this.m_marcEditor;

                    // 0: 没有发现校验错误; 1: 发现校验警告; 2: 发现校验错误
                    int nRet = this.VerifyData(this, e1, true);
                    if (nRet == 2)
                    {
                        MessageBox.Show(this, "MARC 记录经校验发现有错，被拒绝保存。请修改 MARC 记录后重新保存");
                        return;
                    }
                    bVerifyed = true;
                }

                string strHtml = "";
                SaveBiblioToDatabase(true, out strHtml);
                this.m_webExternalHost_biblio.SetHtmlString(strHtml,
    "entityform_biblio");

                if (this.AutoVerifyData == true
                    && bVerifyed == false)
                    API.PostMessage(this.Handle, WM_VERIFY_DATA, 0, 0);
            }
            finally
            {
                this.toolStrip_marcEditor.Enabled = true;
            }
        }

        // 工具条按钮：从数据库删除书目记录
        private void toolStripButton_marcEditor_delete_Click(object sender, EventArgs e)
        {
            this.toolStrip_marcEditor.Enabled = false;
            try
            {
                DeleteBiblioFromDatabase();
            }
            finally
            {
                this.toolStrip_marcEditor.Enabled = true;
            }
        }

        // 工具条按钮、菜单：查看当前XML数据
        private void MenuItem_marcEditor_viewXml_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";

            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,   // 包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "当前XML数据";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strXmlBody;

            //dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "entityform_xmlviewer_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            if (this.MainForm.CanDisplayItemProperty() == true)
                DoViewComment(false);   // 显示在固定面板
            else
                DoViewComment(true);

        }

        // 工具条按钮、菜单：查看最初调入的XML数据
        private void MenuItem_marcEditor_viewOriginXml_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.m_strOriginBiblioXml) == true)
            {
                strError = "暂不具备原始XML数据";
                goto ERROR1;
            }

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "最初调入的XML数据";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = this.m_strOriginBiblioXml;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog();   // ?? this
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // MARC编辑器内文字改变
        private void MarcEditor_TextChanged(object sender, EventArgs e)
        {
            // ****
            this.toolStripButton_marcEditor_save.Enabled = true;

            this.SetSaveAllButtonState(true);

            this._marcEditorVersion++;
        }

        private void easyMarcControl_TextChanged(object sender, EventArgs e)
        {
            // ****
            this.toolStripButton_marcEditor_save.Enabled = true;

            this.SetSaveAllButtonState(true);

            this._templateVersion++;
        }

        // TODO: 本功能已经随着“选项”按钮移动到工具条而废弃
        // 使能记录删除后的“全部保存”按钮
        private void ToolStripMenuItem_enableSaveAllButtonAfterRecordDeleted_Click(object sender, EventArgs e)
        {
            if (this.DeletedMode == false)
            {
                MessageBox.Show(this, "已经在普通模式");
                return;
            }

            this.entityControl1.ChangeAllItemToNewState();
            this.issueControl1.ChangeAllItemToNewState();
            this.orderControl1.ChangeAllItemToNewState();
            this.commentControl1.ChangeAllItemToNewState();

            // 将MarcEditor修改标记变为true
            // this.m_marcEditor.Changed = true; // 这一句决定了使能后如果立即关闭EntityForm窗口，是否会警告(书目)内容丢失
            this.SetMarcChanged(true);

            this.DeletedMode = false;
            // this.SetSaveAllButtonState(true);
            // this.EnableControls(true);  // 2009/11/11 
        }

        // marc编辑窗要从外部获得配置文件内容
        private void MarcEditor_GetConfigFile(object sender, DigitalPlatform.Marc.GetConfigFileEventArgs e)
        {
            Debug.Assert(false, "改造后不要用这个事件接口");

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                e.ErrorInfo = "记录路径为空，无法获得配置文件";
                return;
            }

            // 下载配置文件

            // 得到干净的文件名
            string strCfgFileName = e.Path;
            int nRet = strCfgFileName.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFileName = strCfgFileName.Substring(0, nRet);
            }

            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            string strContent = "";
            string strError = "";

            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetCfgFileContent(strBiblioDbName,
                strCfgFileName,
                out strContent,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                e.ErrorInfo = "获得配置文件 '" + strCfgFileName + "' 时出错：" + strError;
            }
            else
            {
                byte[] baContent = StringUtil.GetUtf8Bytes(strContent, true);
                MemoryStream stream = new MemoryStream(baContent);
                e.Stream = stream;
            }
        }

        private void MarcEditor_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            // Debug.Assert(false, "");
            bool bRemote = string.IsNullOrEmpty(this.MarcSyntax) == false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true
                && bRemote == false)
            {
                e.ErrorInfo = "MarcSyntax 为空，并且记录路径为空，无法获得配置文件 '"+e.Path+"'";
                this.ShowMessage(e.ErrorInfo, "red", true);
                return;
            }

            // 得到干净的文件名
            string strCfgFileName = e.Path;
            int nRet = strCfgFileName.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFileName = strCfgFileName.Substring(0, nRet);
            }

            // 根据 MarcSyntax 取得配置文件
            if (bRemote && string.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                string strFileName = Path.Combine(this.MainForm.DataDir, this.MarcSyntax + "_cfgs/" + strCfgFileName);

                // 在cache中寻找
                e.XmlDocument = this.MainForm.DomCache.FindObject(strFileName);
                if (e.XmlDocument != null)
                    return;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "配置文件 '" + strFileName + "' 装入 XMLDOM 时出错: " + ex.Message;
                    this.ShowMessage(e.ErrorInfo, "red", true);
                    return;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strFileName, dom);  // 保存到缓存
                return;
            }

            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            string strCfgFilePath = strBiblioDbName + "/cfgs/" + strCfgFileName;

            // 在cache中寻找
            e.XmlDocument = this.MainForm.DomCache.FindObject(strCfgFilePath);
            if (e.XmlDocument != null)
                return;

            // 下载配置文件
            string strContent = "";
            string strError = "";

            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = GetCfgFileContent(strCfgFilePath,
                out strContent,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                e.ErrorInfo = "获得配置文件 '" + strCfgFilePath + "' 时出错：" + strError;
            }
            else
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strContent);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "配置文件 '" + strCfgFilePath + "' 装入XMLDUM时出错: " + ex.Message;
                    this.ShowMessage(e.ErrorInfo, "red", true);
                    return;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strCfgFilePath, dom);  // 保存到缓存
            }
        }

        private void MarcEditor_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this._genData.AutoGenerate(sender, 
                e,
                GetBiblioRecPathOrSyntax());
        }

        private void MarcEditor_VerifyData(object sender, GenerateDataEventArgs e)
        {
            // this.VerifyData(sender, e);
            this.VerifyData(sender, e, false);
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

#if NO
        // MARC格式校验
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm BindingForm
        /// <summary>
        /// MARC格式校验
        /// </summary>
        /// <param name="sender">从何处启动?</param>
        /// <param name="e">GenerateDataEventArgs对象，表示动作参数</param>
        public void VerifyData(object sender, 
            GenerateDataEventArgs e)
        {
            VerifyData(sender, e, false);
        }
#endif

        // MARC格式校验
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm BindingForm
        /// <summary>
        /// MARC格式校验
        /// </summary>
        /// <param name="sender">从何处启动?</param>
        /// <param name="e">GenerateDataEventArgs对象，表示动作参数</param>
        /// <param name="bAutoVerify">是否自动校验。自动校验的时候，如果没有发现错误，则不出现最后的对话框</param>
        /// <returns>0: 没有发现校验错误; 1: 发现校验警告; 2: 发现校验错误</returns>
        public int VerifyData(object sender,
            GenerateDataEventArgs e,
            bool bAutoVerify)
        {
            // 库名部分路径
            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            string strError = "";
            string strCode = "";
            string strRef = "";
            string strOutputFilename = "";

            // Debug.Assert(false, "");
            this.m_strVerifyResult = "正在校验...";
            // 自动校验的时候，如果没有发现错误，则不出现最后的对话框
            if (bAutoVerify == false)
            {
                // 如果固定面板隐藏，就打开窗口
                DoViewVerifyResult(this.MainForm.PanelFixedVisible == false ? true : false);

                // 2011/8/17
                if (this.MainForm.PanelFixedVisible == true)
                    MainForm.ActivateVerifyResultPage();
            }

            VerifyHost host = new VerifyHost();
            host.DetailForm = this;

            string strCfgFileName = "dp2circulation_marc_verify.fltx";

            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetCfgFile(strBiblioDbName,
                strCfgFileName,
                out strOutputFilename,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                // .cs 和 .cs.ref
                strCfgFileName = "dp2circulation_marc_verify.cs";
                nRet = GetCfgFileContent(strBiblioDbName,
    strCfgFileName,
    out strCode,
    out baCfgOutputTimestamp,
    out strError);
                if (nRet == 0)
                {
                    strError = "服务器上没有定义路径为 '" + strBiblioDbName + "/" + strCfgFileName + "' 的配置文件(或.fltx配置文件)，数据校验无法进行";
                    goto ERROR1;
                } 
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;
                strCfgFileName = "dp2circulation_marc_verify.cs.ref";
                nRet = GetCfgFileContent(strBiblioDbName,
                    strCfgFileName,
                    out strRef,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == 0)
                {
                    strError = "服务器上没有定义路径为 '" + strBiblioDbName + "/" + strCfgFileName + "' 的配置文件，虽然定义了.cs配置文件。数据校验无法进行";
                    goto ERROR1;
                } 
                if (nRet == -1 || nRet == 0)
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

                nRet = this.PrepareMarcFilter(
                    host,
                    strOutputFilename,
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
                        this.GetMarc(),    //  this.m_marcEditor.Marc,
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

            bool bVerifyFail = false;
            if (string.IsNullOrEmpty(host.ResultString) == true)
            {
                if (this.m_verifyViewer != null)
                {
                    this.m_verifyViewer.ResultString = "经过校验没有发现任何错误。";
                }
            }
            else
            {
                if (bAutoVerify == true)
                {
                    // 延迟打开窗口
                    DoViewVerifyResult(this.MainForm.PanelFixedVisible == false ? true : false);
                }
                this.m_verifyViewer.ResultString = host.ResultString;
                this.MainForm.ActivateVerifyResultPage();   // 2014/7/3
                bVerifyFail = true;
            }

            this.SetSaveAllButtonState(true);   // 2009/3/29 
            return bVerifyFail == true ? 2: 0;
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

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",
									Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe"
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
                "dp2Circulation.VerifyHost");
            if (entryClassType == null)
            {

                strError = "dp2Circulation.VerifyHost派生类没有找到";
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

        /*public*/ int PrepareMarcFilter(
            VerifyHost host,
            string strFilterFileName,
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
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = "EntityForm filter.Load() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

#if NO
            string[] saAddRef1 = {
										 this.BinDir + "\\digitalplatform.marcdom.dll",
										 this.BinDir + "\\digitalplatform.marckernel.dll",
										 this.BinDir + "\\digitalplatform.libraryserver.dll",
										 this.BinDir + "\\digitalplatform.dll",
										 this.BinDir + "\\digitalplatform.Text.dll",
										 this.BinDir + "\\digitalplatform.IO.dll",
										 this.BinDir + "\\digitalplatform.Xml.dll",
										 };
#endif
            string[] saAddRef1 = {
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",
									Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe"
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


#if NO
        // 自动加工数据
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm
        public void AutoGenerate(object sender, GenerateDataEventArgs e)
        {
            // 库名部分路径
            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            string strError = "";
            string strCode = "";
            string strRef = "";

            // Debug.Assert(false, "");

            string strCfgFileName = "dp2circulation_marc_autogen.cs";   // 原来叫dp2_autogen.cs 2007/12/10修改为dp2circulation_marc_autogen.cs

            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetCfgFileContent(strBiblioDbName,
                strCfgFileName,
                out strCode,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            strCfgFileName = "dp2circulation_marc_autogen.cs.ref";
            nRet = GetCfgFileContent(strBiblioDbName,
                strCfgFileName,
                out strRef,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            try
            {
                // 执行代码
                nRet = RunCsScript(
                    sender,
                    e,
                    strCode,
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

            this.SetSaveAllButtonState(true);   // 2009/3/29 
            return;
       ERROR1:
            MessageBox.Show(this, strError);
        }

#endif

        int RunCsScript(
            object sender,
            GenerateDataEventArgs e,
            string strCode,
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
                                    // 2011/3/4 增加
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",

									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.xmleditor.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",
									Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe"
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

            // 得到Assembly中Host派生类Type
            Type entryClassType = ScriptManager.GetDerivedClassType(
                assembly,
                "dp2Circulation.Host");
            if (entryClassType == null)
            {
                /*
                strError = "dp2Circulation.Host派生类没有找到";
                return -1;
                 * */

                entryClassType = ScriptManager.GetDerivedClassType(
                    assembly,
                    "dp2Circulation.DetailHost");
                if (entryClassType == null)
                {
                    strError = "dp2Circulation.Host的派生类和dp2Circulation.DetailHost的派生类都没有找到";
                    return -1;
                }

                // new一个DetailHost派生对象
                DetailHost hostObj = (DetailHost)entryClassType.InvokeMember(null,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                    null);

                if (hostObj == null)
                {
                    strError = "new DetailHost的派生类对象时失败";
                    return -1;
                }

                // 为DetailHost派生类设置参数
                hostObj.DetailForm = this;
                hostObj.Assembly = assembly;

                // hostObj.Main(sender, e);

                // 2009/2/27 
                hostObj.Invoke(String.IsNullOrEmpty(e.ScriptEntry) == true ? "Main" : e.ScriptEntry,
                    sender,
                    e);

                return 0;
            }
            else
            {
                // 为了兼容，保留以往的调用方式

                // new一个Host派生对象
                Host hostObj = (Host)entryClassType.InvokeMember(null,
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

                HostEventArgs e1 = new HostEventArgs();
                e1.e = e;   // 2009/2/24 

                /*
                nRet = this.Flush(out strError);
                if (nRet == -1)
                    return -1;
                 * */


                hostObj.Main(sender, e1);

                /*
                nRet = this.Flush(out strError);
                if (nRet == -1)
                    return -1;
                 * */
            }

            return 0;
        }

#if NO
        /// <summary>
        /// 开始循环
        /// </summary>
        /// <param name="strMessage">要显示在状态行的信息</param>
        public void BeginLoop(string strMessage)
        {
            EnableControls(false);

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial(strMessage);
            Progress.BeginLoop();

            this.Update();
            this.MainForm.Update();
        }

        /// <summary>
        /// 结束循环
        /// </summary>
        public void EndLoop()
        {
            Progress.EndLoop();
            Progress.OnStop -= new StopEventHandler(this.DoStop);
            Progress.Initial("");

            EnableControls(true);
        }
#endif

#if NO
        // 查重
        private void toolStripButton_searchDup_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,   // 包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            DupForm form = new DupForm();

            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;

            form.ProjectName = "<默认>";
            form.XmlRecord = strXmlBody;
            form.RecordPath = this.BiblioRecPath;

            form.AutoBeginSearch = true;

            form.Show();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

        // 查重
        int SearchDup()
        {
            string strError = "";

            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,   // 包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            bool bExistDupForm = this.MainForm.GetTopChildWindow<DupForm>() != null;

            DupForm form = this.MainForm.EnsureDupForm();
            Debug.Assert(form != null, "");

            /*
            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;
             * */

            form.ProjectName = "<默认>";
            form.XmlRecord = strXmlBody;
            form.RecordPath = this.BiblioRecPath;

            /*
            form.AutoBeginSearch = true;
            form.Show();
            form.WaitSearchFinish();
             * */
            Global.Activate(form);
            nRet = form.DoSearch(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(form, strError);
                return -1;
            }

            if (form.GetDupCount() == 0)
            {
                if (bExistDupForm == true)
                {
                    // 把查重窗压到下面
                    this.Activate();
                }
                else
                {
                    // 关掉查重窗
                    form.Close();
                }
                return 0;
            }

            MessageBox.Show(form, "保存记录时经自动查重，发现重复记录");
            return 1;
        ERROR1:
            this.Activate();
            MessageBox.Show(this, strError);
            return -1;
        }

        // 
        /// <summary>
        /// 获得出版社相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str210">返回 210 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int GetPublisherInfo(string strPublisherNumber,
            out string str210,
            out string strError)
        {
            strError = "";
            str210 = "";

            string strDbName = this.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获得出版社信息 ...");
            Progress.BeginLoop();

            try
            {
                string strAction = "";

                long lRet = Channel.GetUtilInfo(
                    Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    strPublisherNumber,
                    "v210",
                    out str210,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }


            return 1;
        }

        // 
        /// <summary>
        /// 设置出版社相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str210">210 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int SetPublisherInfo(string strPublisherNumber,
            string str210,
            out string strError)
        {
            strError = "";

            string strDbName = this.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在设置出版社信息 ...");
            Progress.BeginLoop();

            try
            {
                string strAction = "";

                long lRet = Channel.SetUtilInfo(
                    Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    "r",
                    "i",
                    "v210",
                    strPublisherNumber,
                    str210,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

        }

        // 
        /// <summary>
        /// 获得102相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str102">返回 102 字符春</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
        public int Get102Info(string strPublisherNumber,
            out string str102,
            out string strError)
        {
            strError = "";
            str102 = "";

            string strDbName = this.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获得102信息 ...");
            Progress.BeginLoop();

            try
            {
                string strAction = "";

                long lRet = Channel.GetUtilInfo(
                    Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    strPublisherNumber,
                    "v102",
                    out str102,
                    out strError);
                if (lRet == -1)
                    return -1;

                if (lRet == 0)
                    return 0;

            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }


            return 1;
        }

        // 
        /// <summary>
        /// 设置102相关信息
        /// </summary>
        /// <param name="strPublisherNumber">出版社号码</param>
        /// <param name="str102">102 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int Set102Info(string strPublisherNumber,
            string str102,
            out string strError)
        {
            strError = "";

            string strDbName = this.MainForm.GetUtilDbName("publisher");

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                strError = "尚未定义publisher类型的实用库名";
                return -1;
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在设置102信息 ...");
            Progress.BeginLoop();

            try
            {
                string strAction = "";

                long lRet = Channel.SetUtilInfo(
                    Progress,
                    strAction,
                    strDbName,
                    "ISBN",
                    "r",
                    "i",
                    "v102",
                    strPublisherNumber,
                    str102,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

        }

        // (复制)另存书目记录为。注：包括下属的册、订购、期、评注记录和对象资源
        private void toolStripButton1_marcEditor_saveTo_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.MainForm.ServerVersion < 2.39)
            {
                strError = "本功能需要配合 dp2library 2.39 或以上版本才能使用";
                goto ERROR1;
            }

            string strTargetRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");
            if (string.IsNullOrEmpty(strTargetRecPath) == false)
            {
                DialogResult result = MessageBox.Show(this,
    "当前窗口内的记录原本是从 '" + strTargetRecPath + "' 复制过来的。是否要复制回原有位置？\r\n\r\nYes: 是; No: 否，继续进行普通复制操作; Cancel: 放弃本次操作",
    "EntityForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // strTargetRecPath会发生作用
                }

                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strTargetRecPath = "";
                }
            }


            bool bSaveAs = false;   // 源记录ID就是'?'，追加方式。这意味着数据库中没有源记录

            // 源记录就是 ？
            if (Global.IsAppendRecPath(this.BiblioRecPath) == true)
            {
                bSaveAs = true;
            }

            MergeStyle merge_style = MergeStyle.CombinSubrecord | MergeStyle.ReserveSourceBiblio;

            BiblioSaveToDlg dlg = new BiblioSaveToDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MainForm = this.MainForm;
            // dlg.RecPath = this.BiblioRecPath;
            if (string.IsNullOrEmpty(strTargetRecPath) == false)
                dlg.RecPath = strTargetRecPath;
            else
            {
                dlg.RecPath = this.MainForm.AppInfo.GetString(
                    "entity_form",
                    "save_to_used_path",
                    this.BiblioRecPath);
                dlg.RecID = "?";
            }

            if (bSaveAs == false)
                dlg.MessageText = "(注：本功能*可选择*是否复制书目记录下属的册、期、订购、实体记录和对象资源)\r\n\r\n将当前窗口中的书目记录 " + this.BiblioRecPath + " 复制到:";
            else
            {
                dlg.Text = "保存新书目记录到特定位置";
                dlg.MessageText = "注：\r\n1) 当前执行的是保存而不是复制操作(因为数据库里面还没有这条记录);\r\n2) 书目记录下属的册、期、订购、实体记录和对象资源会被一并保存";
                dlg.EnableCopyChildRecords = false;
            }

            if (string.IsNullOrEmpty(strTargetRecPath) == false)
                dlg.BuildLink = false;
            else
            {
                if (bSaveAs == false)
                    dlg.BuildLink = this.MainForm.AppInfo.GetBoolean(
                        "entity_form",
                        "when_save_to_build_link",
                        true);
                else
                    dlg.BuildLink = false;
            }

            if (bSaveAs == false)
                dlg.CopyChildRecords = this.MainForm.AppInfo.GetBoolean(
                    "entity_form",
                    "when_save_to_copy_child_records",
                    false);
            else
                dlg.CopyChildRecords = true;

            {
                string strMarcSyntax = this.GetCurrentMarcSyntax();
                if (string.IsNullOrEmpty(strMarcSyntax) == true)
                    strMarcSyntax = this.MarcSyntax;    // 外来数据的 MARC 格式

                dlg.MarcSyntax = strMarcSyntax;
            }

            dlg.CurrentBiblioRecPath = this.BiblioRecPath;
            this.MainForm.AppInfo.LinkFormState(dlg, "entityform_BiblioSaveToDlg_state");
            dlg.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (this.BiblioRecPath == dlg.RecPath)
            {
                strError = "要保存到的位置 '" + dlg.RecPath + "' 和当前记录本来的位置 '" + this.BiblioRecPath + "' 相同，复制操作被拒绝。若确实要这样保存记录，请直接使用保存功能。";
                goto ERROR1;
            }

            if (bSaveAs == false)
            {
                this.MainForm.AppInfo.SetBoolean(
                    "entity_form",
                    "when_save_to_build_link",
                    dlg.BuildLink);
                this.MainForm.AppInfo.SetBoolean(
                    "entity_form",
                    "when_save_to_copy_child_records",
                    dlg.CopyChildRecords);
            }
            this.MainForm.AppInfo.SetString(
    "entity_form",
    "save_to_used_path",
    dlg.RecPath);

            // 源记录就是 ？
            if (bSaveAs == true)
            {
                this.BiblioRecPath = dlg.RecPath;

                // 提交所有保存请求
                // return:
                //      -1  有错。此时不排除有些信息保存成功。
                //      0   成功。
                nRet = DoSaveAll();
                if (nRet == -1)
                {
                    strError = "保存操作出错";
                    goto ERROR1;
                }

                return;
            }

            // if (dlg.CopyChildRecords == true)
            {
                // 如果当前记录没有保存，则先保存
                if (this.EntitiesChanged == true
        || this.IssuesChanged == true
        || this.BiblioChanged == true
        || this.ObjectChanged == true
        || this.OrdersChanged == true
        || this.CommentsChanged == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。复制操作前必须先保存当前记录。\r\n\r\n请问要立即保存么？",
                        "EntityForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.OK)
                    {
                        // 提交所有保存请求
                        // return:
                        //      -1  有错。此时不排除有些信息保存成功。
                        //      0   成功。
                        nRet = DoSaveAll();
                        if (nRet == -1)
                        {
                            strError = "因为保存操作出错，所以后续的复制操作被放弃";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "复制操作被放弃";
                        goto ERROR1;
                    }
                }
            }

            // 看看要另存的位置，记录是否已经存在?
            // TODO：　需要改造为合并，或者覆盖。覆盖是先删除目标位置的记录。
            if (dlg.RecID != "?")
            {
                byte[] timestamp = null;

                // 检测特定位置书目记录是否已经存在
                // parameters:
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = DetectBiblioRecord(dlg.RecPath,
                    out timestamp,
                    out strError);
                if (nRet == 1)
                {
                    if (dlg.RecPath != strTargetRecPath)
                    {
#if NO
                        // 提醒覆盖？
                        DialogResult result = MessageBox.Show(this,
                            "书目记录 " + dlg.RecPath + " 已经存在。\r\n\r\n要用当前窗口中的书目记录覆盖此记录么? ",
                            "EntityForm",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                            return;
#endif
                        GetMergeStyleDialog merge_dlg = new GetMergeStyleDialog();
                        MainForm.SetControlFont(merge_dlg, this.Font, false);
                        merge_dlg.SourceRecPath = this.BiblioRecPath;
                        merge_dlg.TargetRecPath = dlg.RecPath;
                        merge_dlg.MessageText = "目标书目记录 " + dlg.RecPath + " 已经存在。\r\n\r\n请指定当前窗口中的书目记录(源)和此目标记录合并的方法";

                        merge_dlg.UiState = this.MainForm.AppInfo.GetString(
        "entity_form",
        "GetMergeStyleDialog_copy_uiState",
        "");
                        merge_dlg.EnableSubRecord = dlg.CopyChildRecords;

                        this.MainForm.AppInfo.LinkFormState(merge_dlg, "entityform_GetMergeStyleDialog_copy_state");
                        merge_dlg.ShowDialog(this);
                        this.MainForm.AppInfo.UnlinkFormState(merge_dlg);
                        this.MainForm.AppInfo.SetString(
"entity_form",
"GetMergeStyleDialog_copy_uiState",
merge_dlg.UiState);

                        if (merge_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            return;

                        merge_style = merge_dlg.GetMergeStyle();

                    }

                    // this.BiblioTimestamp = timestamp;   // 为了顺利覆盖

                    // TODO: 预先检查操作者权限，确保删除书目记录和下级记录都能成功，否则就警告

#if NO
                    // 删除目标位置的书目记录，但保留其下属的实体等记录
                    nRet = DeleteBiblioRecordFromDatabase(dlg.RecPath,
                        "onlydeletebiblio",
                        timestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#endif
                    if ((merge_style & MergeStyle.OverwriteSubrecord) != 0)
                    {
                        // 删除目标记录整个，或者删除目标位置的下级记录
                        // TODO: 测试的时候，注意不用下述调用而测试保留目标书目记录中对象的可能性
                        nRet = DeleteBiblioRecordFromDatabase(dlg.RecPath,
                            (merge_style & MergeStyle.ReserveSourceBiblio) != 0 ? "delete" : "onlydeletesubrecord",
                            timestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            if ((merge_style & MergeStyle.ReserveSourceBiblio) != 0)
                                strError = "删除目标位置的书目记录 '" + dlg.RecPath + "' 时出错: " + strError;
                            else
                                strError = "删除目标位置的书目记录 '" + dlg.RecPath + "' 的全部子记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }

                }
            }

            string strOutputBiblioRecPath = "";
            byte[] baOutputTimestamp = null;
            string strXml = "";

            string strOldBiblioRecPath = this.BiblioRecPath;
            string strOldMarc = this.GetMarc();    //  this.m_marcEditor.Marc;
            bool bOldChanged = this.GetMarcChanged();   //  this.m_marcEditor.Changed;

            try
            {

                // 保存原来的记录路径
                bool bOldReadOnly = this.m_marcEditor.ReadOnly;
                Field old_998 = null;
                // bool bOldChanged = this.BiblioChanged;

                if (dlg.BuildLink == true)
                {
                    nRet = this.MainForm.CheckBuildLinkCondition(
                        dlg.RecPath,    // 即将创建/保存的记录
                        strOldBiblioRecPath,    // 保存前的记录
                        false,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        // 
                        strError = "无法为记录 '" + this.BiblioRecPath + "' 建立指向 '" + strOldBiblioRecPath + "' 的目标关系：" + strError;
                        MessageBox.Show(this, strError);
                    }
                    else
                    {
                        // 保存当前记录的998字段
                        old_998 = this.m_marcEditor.Record.Fields.GetOneField("998", 0);

                        this.m_marcEditor.Record.Fields.SetFirstSubfield("998", "t", strOldBiblioRecPath);
                        /*
                        if (bOldReadOnly == false)
                            this.MarcEditor.ReadOnly = true;
                        */
                    }
                }
                else
                {
                    // 保存当前记录的998字段
                    old_998 = this.m_marcEditor.Record.Fields.GetOneField("998", 0);

                    // 清除可能存在的998$t
                    if (old_998 != null)
                    {
                        SubfieldCollection subfields = old_998.Subfields;
                        Subfield old_t = subfields["t"];
                        if (old_t != null)
                        {
                            old_998.Subfields = subfields.Remove(old_t);
                            // 如果998内一个子字段也没有了，是否这个字段要删除?
                        }
                        else
                            old_998 = null; // 表示(既然没有删除$t，就)不用恢复
                    }
                }

                string strMergeStyle = "";
                if ((merge_style & MergeStyle.ReserveSourceBiblio) != 0)
                    strMergeStyle = "reserve_source";
                else
                    strMergeStyle = "reserve_target";

                if ((merge_style & MergeStyle.MissingSourceSubrecord) != 0)
                    strMergeStyle += ",missing_source_subrecord";
                else if ((merge_style & MergeStyle.OverwriteSubrecord) != 0)
                {
                    // dp2library 尚未实现这个功能，不过本函数前面已经用 SetBiblioInfo() API 主动删除了目标位置下属的子记录，效果是一样的。(当然，这样实现起来原子性不是那么好)
                    // strMergeStyle += ",overwrite_target_subrecord";
                }

                if (dlg.CopyChildRecords == false)
                {
                    nRet = CopyBiblio(
        "onlycopybiblio",
        dlg.RecPath,
        strMergeStyle,
        out strXml,
        out strOutputBiblioRecPath,
        out baOutputTimestamp,
        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
                else
                {
                    nRet = CopyBiblio(
                        "copy",
                        dlg.RecPath,
                        strMergeStyle,
                        out strXml,
                        out strOutputBiblioRecPath,
                        out baOutputTimestamp,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }

            }
            finally
            {
#if NO
                // 复原当前窗口的记录
                if (this.m_marcEditor.Marc != strOldMarc)
                    this.m_marcEditor.Marc = strOldMarc;
                if (this.m_marcEditor.Changed != bOldChanged)
                    this.m_marcEditor.Changed = bOldChanged;
#endif
                if (this.GetMarc() /*this.m_marcEditor.Marc*/ != strOldMarc)
                {
                    // this.m_marcEditor.Marc = strOldMarc;
                    this.SetMarc(strOldMarc);
                }
                if (this.GetMarcChanged() /*this.m_marcEditor.Changed*/ != bOldChanged)
                {
                    // this.m_marcEditor.Changed = bOldChanged;
                    this.SetMarcChanged(bOldChanged);
                }
            }

            if (nRet == -1)
            {
                this.BiblioRecPath = strOldBiblioRecPath;

#if NO
                if (old_998 != null)
                {
                    // 恢复先前的998字段内容
                    for (int i = 0; i < this.MarcEditor.Record.Fields.Count; i++)
                    {
                        Field temp = this.MarcEditor.Record.Fields[i];
                        if (temp.Name == "998")
                        {
                            this.MarcEditor.Record.Fields.RemoveAt(i);
                            i--;
                        }
                    }
                    if (old_998 != null)
                    {
                        this.MarcEditor.Record.Fields.Insert(this.MarcEditor.Record.Fields.Count,
                            old_998.Name,
                            old_998.Indicator,
                            old_998.Value);
                    }
                    // 恢复操作前的ReadOnly
                    if (this.MarcEditor.ReadOnly != bOldReadOnly)
                        this.MarcEditor.ReadOnly = bOldReadOnly;

                    if (this.BiblioChanged != bOldChanged)
                        this.BiblioChanged = bOldChanged;
                }
#endif
                return;
            }

            // TODO: 询问是否要立即装载目标记录到当前窗口，还是装入新的一个种册窗，还是不装入？
            {
                DialogResult result = MessageBox.Show(this,
        "复制操作已经成功。\r\n\r\n请问是否立即将目标记录 '" + strOutputBiblioRecPath + "' 装入一个新的种册窗以便进行观察? \r\n\r\n是(Yes): 装入一个新的种册窗；\r\n否(No): 装入当前窗口；\r\n取消(Cancel): 不装入目标记录到任何窗口",
        "EntityForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    EntityForm form = new EntityForm();
                    form.MdiParent = this.MainForm;
                    form.MainForm = this.MainForm;
                    form.Show();
                    Debug.Assert(form != null, "");

                    form.LoadRecordOld(strOutputBiblioRecPath, "", true);
                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            // 将目标记录装入当前窗口
            this.LoadRecordOld(strOutputBiblioRecPath, "", false);
#if NO
            // 将目标记录装入当前窗口
            this.BiblioTimestamp = baOutputTimestamp;
            this.BiblioRecPath = strOutputBiblioRecPath;


            string strBiblioDbName = Global.GetDbName(this.BiblioRecPath);

            // bool bError = false;

            // 装载新记录的entities部分

            // 接着装入相关的所有册
            string strItemDbName = this.MainForm.GetItemDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strItemDbName) == false) // 仅在当前书目库有对应的实体库时，才装入册记录
            {
                this.EnableItemsPage(true);

                nRet = this.entityControl1.LoadEntityRecords(this.BiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    // bError = true;
                }
            }
            else
            {
                this.EnableItemsPage(false);
                this.entityControl1.ClearEntities();
            }

            // 接着装入相关的所有期
            string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strIssueDbName) == false) // 仅在当前书目库有对应的期库时，才装入期记录
            {
                this.EnableIssuesPage(true);
                nRet = this.issueControl1.LoadIssueRecords(this.BiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    // bError = true;
                }
            }
            else
            {
                this.EnableIssuesPage(false);
                this.issueControl1.ClearIssues();
            }
            // 接着装入相关的所有订购信息
            string strOrderDbName = this.MainForm.GetOrderDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strOrderDbName) == false) // 仅在当前书目库有对应的采购库时，才装入采购记录
            {
                if (String.IsNullOrEmpty(strIssueDbName) == false)
                    this.orderControl1.SeriesMode = true;
                else
                    this.orderControl1.SeriesMode = false;

                this.EnableOrdersPage(true);
                nRet = this.orderControl1.LoadOrderRecords(this.BiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    // bError = true;
                }
            }
            else
            {
                this.EnableOrdersPage(false);
                this.orderControl1.ClearOrders();
            }

            // 接着装入相关的所有评注信息
            string strCommentDbName = this.MainForm.GetCommentDbName(strBiblioDbName);
            if (String.IsNullOrEmpty(strCommentDbName) == false) // 仅在当前书目库有对应的评注库时，才装入评注记录
            {
                this.EnableCommentsPage(true);
                nRet = this.commentControl1.LoadCommentRecords(this.BiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    // bError = true;
                }
            }
            else
            {
                this.EnableCommentsPage(false);
                this.commentControl1.ClearComments();
            }

            // 接着装入对象资源
            if (dlg.CopyChildRecords == true)
            {
                nRet = this.binaryResControl1.LoadObject(this.BiblioRecPath,    // 2008/11/2 changed
                    strXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    // bError = true;
                    // return -1;
                }
            }

            // 装载书目和<dprms:file>以外的其它XML片断
            {
                nRet = LoadXmlFragment(strXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
            }

            /*
            // 没有对象资源
            if (this.binaryResControl1 != null)
            {
                this.binaryResControl1.Clear();
            }
             * */

            // TODO: 装载HTML?
            Global.SetHtmlString(this.webBrowser_biblioRecord, "(空白)");   // 暂时刷新为空白
#endif

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 册登记
        /// </summary>
        public void DoRegisterEntity()
        {
            int nRet = 0;
            string strError = "";

            // TODO: 要解决EnableControls调用多层嵌套的问题。另外焦点转移是否还正确？
            this.EnableControls(false);
            try
            {

                // 看看输入的条码是否为ISBN条码
                if (IsISBnBarcode(this.textBox_itemBarcode.Text) == true)
                {
                    // 保存当前册信息
                    nRet = this.entityControl1.DoSaveItems();
                    if (nRet == -1)
                        return; // 放弃进一步操作

                    // 转而触发新种检索操作
                    this.textBox_queryWord.Text = this.textBox_itemBarcode.Text;
                    this.textBox_itemBarcode.Text = "";

                    this.button_search_Click(null, null);
                    return;
                }

                // 检查册条码号形式是否合法
                if (NeedVerifyItemBarcode == true
                    && string.IsNullOrEmpty(this.textBox_itemBarcode.Text) == false)    // 2009/11/24 空的字符串不进行检查
                {
                    // 形式校验条码号
                    // return:
                    //      -2  服务器没有配置校验方法，无法校验
                    //      -1  error
                    //      0   不是合法的条码号
                    //      1   是合法的读者证条码号
                    //      2   是合法的册条码号
                    nRet = VerifyBarcode(
                        this.Channel.LibraryCodeList,
                        this.textBox_itemBarcode.Text,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 输入的条码号格式不合法
                    if (nRet == 0)
                    {
                        strError = "您输入的条码号 " + this.textBox_itemBarcode.Text + " 格式不正确(" + strError + ")。请重新输入。";
                        goto ERROR1;
                    }

                    // 实际输入的是读者证条码号
                    if (nRet == 1)
                    {
                        strError = "您输入的条码号 " + this.textBox_itemBarcode.Text + " 是读者证条码号。请输入册条码号。";
                        goto ERROR1;
                    }

                    // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                    if (nRet == -2)
                        MessageBox.Show(this, "警告：前端开启了校验条码号功能，但是服务器端缺乏相应的脚本函数，无法校验条码号。\r\n\r\n若要避免出现此警告对话框，请关闭前端校验功能");

                }

                ActivateItemsPage();

                if (this.RegisterType == RegisterType.Register)
                {
                    // 登记
                    this.entityControl1.DoNewEntity(this.textBox_itemBarcode.Text);

                    this.SwitchFocus(ITEM_BARCODE);
                }
                else if (this.RegisterType == RegisterType.QuickRegister)
                {
                    // 快速登记
                    nRet = this.entityControl1.DoQuickNewEntity(this.textBox_itemBarcode.Text);
                    if (nRet != -1)
                    {
                        /*
                        this.textBox_itemBarcode.SelectAll();
                        this.textBox_itemBarcode.Focus();
                         * */
                        this.SwitchFocus(ITEM_BARCODE);
                    }

                }
                else if (this.RegisterType == RegisterType.SearchOnly)
                {
                    // 只检索
                    //this.EnableControls(false);
                    LoadItemByBarcode(this.textBox_itemBarcode.Text,
                        this.checkBox_autoSavePrev.Checked);
                    //this.EnableControls(true);
                }
            }
            finally
            {
                this.EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        static bool IsISBnBarcode(string strText)
        {
            if (strText.Length == 13)
            {
                string strHead = strText.Substring(0, 3);
                if (strHead == "978" || strHead == "979")
                    return true;
            }

            return false;
        }

        void LoadFontToMarcEditor()
        {
            string strFontString = MainForm.AppInfo.GetString(
                "marceditor",
                "fontstring",
                "");  // "Arial Unicode MS, 12pt"

            if (String.IsNullOrEmpty(strFontString) == false)
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                this.m_marcEditor.Font = (Font)converter.ConvertFromString(strFontString);
            }

            string strFontColor = MainForm.AppInfo.GetString(
                "marceditor",
                "fontcolor",
                "");

            if (String.IsNullOrEmpty(strFontColor) == false)
            {
                // Create the ColorConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

                this.m_marcEditor.ContentTextColor = (Color)converter.ConvertFromString(strFontColor);
            }
        }

        void SaveFontForMarcEditor()
        {
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                string strFontString = converter.ConvertToString(this.m_marcEditor.Font);

                MainForm.AppInfo.SetString(
                    "marceditor",
                    "fontstring",
                    strFontString);
            }

            {
                // Create the ColorConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

                string strFontColor = converter.ConvertToString(this.m_marcEditor.ContentTextColor);

                MainForm.AppInfo.SetString(
                    "marceditor",
                    "fontcolor",
                    strFontColor);
            }
        }

        /// <summary>
        /// 恢复缺省字体
        /// </summary>
        public new void RestoreDefaultFont()
        {
            if (this.MainForm != null)
            {
                Size oldsize = this.Size;
                if (this.MainForm.DefaultFont == null)
                {
                    MainForm.SetControlFont(this, Control.DefaultFont);
                    this.m_marcEditor.Font = Control.DefaultFont;
                }
                else
                {
                    MainForm.SetControlFont(this, this.MainForm.DefaultFont);
                    this.m_marcEditor.Font = this.MainForm.DefaultFont;
                }
                this.Size = oldsize;

                // 保存到配置文件
                SaveFontForMarcEditor();
            }
        }

        // 
        /// <summary>
        /// 设置字体
        /// </summary>
        public void SetFont()
        {
            if (this.m_marcEditor.Focused)
                SetMarcEditFont();
            else
            {
                MessageBox.Show(this, "如果要设置 MARC编辑器 的字体，请将输入焦点置于 MARC编辑器 上再使用本功能。\r\n\r\n如果要设置窗口内其它部分的字体，请使用主菜单的“参数配置”命令，在随后出现的对话框中选择“外观”属性页，设置“缺省字体”");
            }
        }

        /// <summary>
        /// 设置 MARC 编辑器的字体
        /// </summary>
        public void SetMarcEditFont()
        {
            FontDialog dlg = new FontDialog();
            dlg.ShowColor = true;
            dlg.Color = this.m_marcEditor.ContentTextColor;
            dlg.Font = this.m_marcEditor.Font;
            dlg.ShowApply = true;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            dlg.Apply -= new EventHandler(dlgMarcEditFont_Apply);
            dlg.Apply += new EventHandler(dlgMarcEditFont_Apply);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            this.m_marcEditor.Font = dlg.Font;
            this.m_marcEditor.ContentTextColor = dlg.Color;

            // 保存到配置文件
            SaveFontForMarcEditor();
        }

        void dlgMarcEditFont_Apply(object sender, EventArgs e)
        {
            FontDialog dlg = (FontDialog)sender;

            this.m_marcEditor.Font = dlg.Font;
            this.m_marcEditor.ContentTextColor = dlg.Color;

            // 保存到配置文件
            SaveFontForMarcEditor();
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            string strError = "";
            long nRet = this.Channel.Logout(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

        }

        private void toolStripButton_prev_Click(object sender, EventArgs e)
        {
            // TODO: 可以改进为调用Safe...，这样就不必在意Disable按钮来防止重入了
            this.LoadRecordOld(this.BiblioRecPath, "prev", true);
        }

        private void toolStripButton_next_Click(object sender, EventArgs e)
        {
            this.LoadRecordOld(this.BiblioRecPath, "next", true);
        }

        string m_strFocusedPart = "";

        private void EntityForm_Leave(object sender, EventArgs e)
        {
        }

        private void EntityForm_Enter(object sender, EventArgs e)
        {
            /*
            // 2008/11/26 
            if (m_strFocusedPart == "marceditor")
            {
                if (this.MarcEditor.FocusedFieldIndex == -1)
                    this.MarcEditor.FocusedFieldIndex = 0;

                this.MarcEditor.Focus();
            }
            else if (m_strFocusedPart == "itembarcode")
            {
                this.textBox_itemBarcode.Focus();
            }
            */
        }

        private void MarcEditor_Enter(object sender, EventArgs e)
        {
            m_strFocusedPart = "marceditor";

            // API.PostMessage(this.Handle, WM_FILL_MARCEDITOR_SCRIPT_MENU, 0, 0);
            if (this.MainForm.PanelFixedVisible == true)
                this._genData.AutoGenerate(this.m_marcEditor,
                    new GenerateDataEventArgs(),
                    GetBiblioRecPathOrSyntax(),
                    true);

            Debug.WriteLine("MarcEditor Enter");
        }

        private void MarcEditor_Leave(object sender, EventArgs e)
        {
            Debug.WriteLine("MarcEditor Leave");
        }

        private void tabPage_marc_Enter(object sender, EventArgs e)
        {
            /*
            SwitchFocus(MARC_EDITOR);
             * */
        }

        private void MarcEditor_ControlLetterKeyPress(object sender, ControlLetterKeyPressEventArgs e)
        {
            if (e.KeyData == Keys.T)
            {
                e.Handled = true;
                this.LoadBiblioTemplate(true);
                return;
            }
            if (e.KeyData == Keys.D)
            {
                e.Handled = true;
                this.ToolStripMenuItem_searchDupInExistWindow_Click(this, e);
                return;
            }

        }

        private void EntityForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;

        }

        private void EntityForm_DragDrop(object sender, DragEventArgs e)
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
                strError = "种册窗只允许拖入一个记录";
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

            // 判断它是书目记录路径，还是实体记录路径？
            string strDbName = Global.GetDbName(strRecPath);

            if (this.MainForm.IsBiblioDbName(strDbName) == true)
            {
                this.LoadRecordOld(strRecPath,
                    "",
                    true);
            }
            else if (this.MainForm.IsItemDbName(strDbName) == true)
            {
                this.LoadItemByRecPath(strRecPath,
                    this.checkBox_autoSavePrev.Checked);
            }
            else
            {
                strError = "记录路径 '" + strRecPath + "' 中的数据库名既不是书目库名，也不是实体库名...";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }






        #region 期 相关功能









        #endregion

        /// <summary>
        /// 获得对象资源信息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        public void GetResInfo(object sender, GetResInfoEventArgs e)
        {
            List<ResInfo> resinfos = new List<ResInfo>();
            for (int i = 0; i < this.binaryResControl1.ListView.Items.Count; i++)
            {
                ListViewItem item = this.binaryResControl1.ListView.Items[i];

                string strID = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_ID);
                // 当 e.ID 为空的时候表示希望获得所有的行的信息。否则就是获得特定 id 的行的信息
                if (String.IsNullOrEmpty(e.ID) == true
                    || strID == e.ID)
                {
                    ResInfo resinfo = new ResInfo();
                    resinfo.ID = strID;
                    resinfo.LocalPath = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_LOCALPATH);
                    resinfo.Mime = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_MIME);
                    try
                    {
                        resinfo.Size = Convert.ToInt64(ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_SIZE));
                    }
                    catch
                    {
                    }
                    resinfo.Usage = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_USAGE);
                    resinfo.Rights = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_RIGHTS);
                    resinfos.Add(resinfo);
                }
            }

            e.Results = resinfos;
        }

        // return:
        //      false   没有找到指定 ID 的对象
        //      true    找到并修改了 rights 值
        public bool ChangeObjectRigths(string strID, string strRights)
        {
            List<ListViewItem> items = this.binaryResControl1.FindItemByID(strID);
            if (items.Count == 0)
                return false;
            foreach(ListViewItem item in items)
            {
                this.binaryResControl1.ChangeObjectRights(item, strRights);
            }
            return true;
        }

        // 设置目标记录
        // 空表示清除
        private void toolStripButton_setTargetRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strTargetBiblioRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");

        REDO:
            strTargetBiblioRecPath = InputDlg.GetInput(
            this,
            "请指定目标记录路径",
            "目标记录路径(格式'书目库名/ID'): \r\n\r\n[注：如果设置为空，表示清除目标记录路径]",
            strTargetBiblioRecPath,
            this.MainForm.DefaultFont);
            if (strTargetBiblioRecPath == null)
                return;

            if (strTargetBiblioRecPath == "")
                goto SET;

            // parameters:
            //      strSourceRecPath    记录ID不能为问号
            //      strTargetRecPath    记录ID可以为问号，当bCheckTargetWenhao==false
            // return:
            //      -1  出错
            //      0   不适合建立目标关系 (这种情况是没有什么错，但是不适合建立)
            //      1   适合建立目标关系
            int nRet = this.MainForm.CheckBuildLinkCondition(this.BiblioRecPath,
                    strTargetBiblioRecPath,
                    true,
                    out strError);
            if (nRet == -1 || nRet == 0)
            {
                MessageBox.Show(this, strError);
                goto REDO;
            }

            /*

            TODO: 改用统一函数检查

            // TODO: 最好检查一下这个路径的格式。合法的书目库名可以在MainForm中找到

            // 检查是不是书目库名。MARC格式是否和当前数据库一致。不能是当前记录自己
            string strDbName = Global.GetDbName(strTargetBiblioRecPath);
            string strRecordID = Global.GetRecordID(strTargetBiblioRecPath);

            if (String.IsNullOrEmpty(strDbName) == true
                || String.IsNullOrEmpty(strRecordID) == true)
            {
                strError = "'"+strTargetBiblioRecPath+"' 不是合法的记录路径";
                goto ERROR1;
            }

            // 根据书目库名获得MARC格式语法名
            // return:
            //      null    没有找到指定的书目库名
            string strCurrentSyntax = this.MainForm.GetBiblioSyntax(this.BiblioDbName);
            if (String.IsNullOrEmpty(strCurrentSyntax) == true)
                strCurrentSyntax = "unimarc";
            string strCurrentIssueDbName = MainForm.GetIssueDbName(this.BiblioDbName);


            bool bFound = false;
            for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
            {
                BiblioDbProperty prop = this.MainForm.BiblioDbProperties[i];

                if (prop.DbName == strDbName)
                {
                    bFound = true;

                    string strTempSyntax = prop.Syntax;
                    if (String.IsNullOrEmpty(strTempSyntax) == true)
                        strTempSyntax = "unimarc";

                    if (strTempSyntax != strCurrentSyntax)
                    {
                        strError = "拟设置的目标记录因其书目数据格式为 '"+strTempSyntax+"'，与当前记录的书目数据格式 '"+strCurrentSyntax+"' 不一致，因此操作被拒绝";
                        goto ERROR1;
                    }

                    if (String.IsNullOrEmpty(prop.IssueDbName)
                        != String.IsNullOrEmpty(strCurrentIssueDbName))
                    {
                        strError = "拟设置的目标记录因其书目库 '"+strDbName+"' 文献类型(期刊还是图书)和当前记录的书目库 '"+this.BiblioDbName+"' 不一致，因此操作被拒绝";
                        goto ERROR1;
                    }
                }
            }

            if (bFound == false)
            {
                strError = "'"+strDbName+"' 不是合法的书目库名";
                goto ERROR1;
            }

            if (strRecordID == "?")
            {
                strError = "记录ID不能为问号";
                goto ERROR1;
            }

            if (Global.IsPureNumber(strRecordID) == false)
            {
                strError = "记录ID部分必须为纯数字";
                goto ERROR1;
            }

            if (strDbName == this.BiblioDbName)
            {
                strError = "目标记录和当前记录不能属于同一个书目库";
                goto ERROR1;
                // 注：这样就不用检查目标是否本记录了
            }
            */

            bool bReplaceMarc = true;
            // 警告：当前记录会被目标记录完全替代
            DialogResult result = MessageBox.Show(this,
                "当前MARC编辑器内的内容将被来自目标记录的内容完全取代。\r\n\r\n确实要取代? \r\n\r\n是(Yes): 取代；\r\n否(No): 不取代，但是继续设置目标记录路径的操作；\r\n取消(Cancel): 不取代，并且放弃设置目标记录路径的操作",
                "EntityForm",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
                return;
            if (result == DialogResult.Yes)
                bReplaceMarc = true;
            else
                bReplaceMarc = false;

            if (bReplaceMarc == true)
            {
                // 保存当前记录的998字段
                Field old_998 = this.m_marcEditor.Record.Fields.GetOneField("998", 0);

                // 装入目标书目记录
                // 本函数不修改this.BiblioRecPath，因为这是当前记录的路径
                // parameters:
                // return:
                //      -1  error
                //      0   not found, strError中有出错信息
                //      1   found
                nRet = LoadTargetBiblioRecord(strTargetBiblioRecPath,
                    out strError);
                if (nRet == 0 || nRet == -1)
                    goto ERROR1;

                // 恢复先前的998字段内容
                for (int i = 0; i < this.m_marcEditor.Record.Fields.Count; i++)
                {
                    Field temp = this.m_marcEditor.Record.Fields[i];
                    if (temp.Name == "998")
                    {
                        this.m_marcEditor.Record.Fields.RemoveAt(i);
                        i--;
                    }
                }
                if (old_998 != null)
                {
                    this.m_marcEditor.Record.Fields.Insert(this.m_marcEditor.Record.Fields.Count,
                        old_998.Name,
                        old_998.Indicator,
                        old_998.Value);
                }
            }

        SET:

            if (String.IsNullOrEmpty(strTargetBiblioRecPath) == false)
                this.m_marcEditor.Record.Fields.SetFirstSubfield("998", "t", strTargetBiblioRecPath);
            else
            {
                this.Remove998t();
            }

        if (this.LinkedRecordReadonly == true)
        {
            if (String.IsNullOrEmpty(strTargetBiblioRecPath) == true)
                this.m_marcEditor.ReadOnly = false;
            else
                this.m_marcEditor.ReadOnly = true;
        }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      shifou fashengle gaibian
        bool Remove998t()
        {
            Field field_998 = this.m_marcEditor.Record.Fields.GetOneField("998", 0);
            if (field_998 == null)
                return false;
            SubfieldCollection subfields = field_998.Subfields;

            bool bChanged = false;
            while (true)
            {
                Subfield subfield = subfields["t"];
                if (subfield != null)
                {
                    subfields.Remove(subfield);
                    bChanged = true;
                }
                else
                    break;
            }

            if (bChanged == true)
                field_998.Subfields = subfields;

            return bChanged;
        }

        // 装入目标书目记录
        // 本函数不修改this.BiblioRecPath，因为这是当前记录的路径
        // parameters:
        // return:
        //      -1  error
        //      0   not found, strError中有出错信息
        //      1   found
        int LoadTargetBiblioRecord(string strTargetBiblioRecPath,
            out string strError)
        {
            strError = "";
            string strXml = "";
            string strOutputTargetBiblioRecPath = "";

            if (String.IsNullOrEmpty(strTargetBiblioRecPath) == true)
            {
                strError = "strTargetBiblioRecPath参数值不能为空";
                goto ERROR1;
            }

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在装入目标记录 '" + strTargetBiblioRecPath + "' ...");
            Progress.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                // Global.SetHtmlString(this.webBrowser_biblioRecord, "(空白)");
                this.m_webExternalHost_biblio.SetHtmlString("(空白)",
                    "entityform_error"); 
                
                Progress.SetMessage("正在装入目标记录 " + strTargetBiblioRecPath + " ...");

                bool bCataloging = this.Cataloging;

                string[] formats = null;

                if (bCataloging == true)
                {
                    formats = new string[3];
                    formats[0] = "outputpath";
                    formats[1] = "html";
                    formats[2] = "xml";
                }
                else
                {
                    formats = new string[2];
                    formats[0] = "outputpath";
                    formats[1] = "html";
                }

                string[] results = null;
                byte[] baTimestamp = null;

                long lRet = Channel.GetBiblioInfos(
                    Progress,
                    strTargetBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    strError = "路径为 '" + strTargetBiblioRecPath + "' 的书目记录没有找到 ...";
                    return 0;   // not found
                }

                string strHtml = "";

                if (results != null && results.Length >= 1)
                    strOutputTargetBiblioRecPath = results[0];

                if (results != null && results.Length >= 2)
                    strHtml = results[1];

                if (lRet == -1)
                {
                    return -1;
                }
                else
                {
                    // 没有报错时，要对results进行严格检查
                    if (results == null)
                    {
                        strError = "results == null";
                        goto ERROR1;
                    }
                    if (results.Length != formats.Length)
                    {
                        strError = "result.Length != formats.Length";
                        goto ERROR1;
                    }

                    // 没有报错的情况下才刷新时间戳
                    // this.BiblioTimestamp = baTimestamp;
                }

#if NO
                Global.SetHtmlString(this.webBrowser_biblioRecord,
                    strHtml,
                    this.MainForm.DataDir,
                    "entityform_biblio");
#endif
                this.m_webExternalHost_biblio.SetHtmlString(strHtml,
                    "entityform_biblio");

                if (bCataloging == true)
                {
                    if (results != null && results.Length >= 3)
                        strXml = results[2];

                    {
                        // return:
                        //      -1  error
                        //      0   空的记录
                        //      1   成功
                        int nRet = SetBiblioRecordToMarcEditor(strXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 2008/11/13 
                        if (nRet == 0)
                            MessageBox.Show(this, "警告：目标记录 '" + strOutputTargetBiblioRecPath + "' 是一条空记录");

                        this.BiblioChanged = true;
                    }
                }
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 装入目标记录
        private void ToolStripMenuItem_loadTargetBiblioRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strTargetBiblioRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");

            if (String.IsNullOrEmpty(strTargetBiblioRecPath) == true)
            {
                strError = "当前记录不具备目标记录";
                goto ERROR1;
            }

            // return:
            //      -1  出错。已经用MessageBox报错
            //      0   没有装载(例如发现窗口内的记录没有保存，出现警告对话框后，操作者选择了Cancel)
            //      1   成功装载
            //      2   通道被占用
            LoadRecordOld(strTargetBiblioRecPath,
                "",
                true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*public*/ void AddToPendingList(string strBiblioRecPath,
            string strPrevNextStyle)
        {
            PendingLoadRequest request = new PendingLoadRequest();
            request.RecPath = strBiblioRecPath;
            request.PrevNextStyle = strPrevNextStyle;
            lock (this.m_listPendingLoadRequest)
            {
                this.m_listPendingLoadRequest.Clear();
                this.m_listPendingLoadRequest.Add(request);
            }
            this.timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (this.m_listPendingLoadRequest)
            {
                if (this.m_listPendingLoadRequest.Count == 0)
                {
                    this.timer1.Stop();
                    return;
                }
            }
            string strError = "";
            PendingLoadRequest request = null;
            lock (this.m_listPendingLoadRequest)
            {
                request = this.m_listPendingLoadRequest[0];
            }
            int nRet = this.LoadRecord(request.RecPath,
                request.PrevNextStyle,
                true,
                false,
                out strError);
            if (nRet == 2)
            {
            }
            else
            {
                lock (this.m_listPendingLoadRequest)
                {
                    this.m_listPendingLoadRequest.Remove(request);
                }
            }

        }

        // 导出所有信息到XML文件
        private void ToolStripMenuItem_exportAllInfoToXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的XML文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlDocument domBiblio = new XmlDocument();
            try
            {
                domBiblio.LoadXml(strXmlBody);
            }
            catch (Exception ex)
            {
                strError = "biblio xml load to dom error: " + ex.Message;
                goto ERROR1;
            }

            XmlNode nodeBiblio = dom.CreateElement("dprms", "biblio", DpNs.dprms);
            dom.DocumentElement.AppendChild(nodeBiblio);
            nodeBiblio.InnerXml = domBiblio.DocumentElement.OuterXml;   // <unimarc:record>或者<usmarc:record>成为<dprms:biblio>的下级

            // 册
            string strItemXml = "";
            nRet = this.entityControl1.Items.BuildXml(
                out strItemXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            XmlDocument domItems = new XmlDocument();
            try
            {
                domItems.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "items xml load to dom error: " + ex.Message;
                goto ERROR1;
            }

            XmlNode nodeItems = dom.CreateElement("dprms", "itemCollection", DpNs.dprms);
            dom.DocumentElement.AppendChild(nodeItems);
            nodeItems.InnerXml = domItems.DocumentElement.InnerXml;

            // 期
            // TODO: 是否要根据出版物类型，决定是否创建<dprms:issues>元素
            string strIssueXml = "";
            nRet = this.issueControl1.Items.BuildXml(
                out strIssueXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            XmlDocument domIssues = new XmlDocument();
            try
            {
                domIssues.LoadXml(strIssueXml);
            }
            catch (Exception ex)
            {
                strError = "issues xml load to dom error: " + ex.Message;
                goto ERROR1;
            }

            XmlNode nodeIssues = dom.CreateElement("dprms", "issueCollection", DpNs.dprms);
            dom.DocumentElement.AppendChild(nodeIssues);
            nodeIssues.InnerXml = domIssues.DocumentElement.InnerXml;

            // 订购
            string strOrderXml = "";
            nRet = this.orderControl1.Items.BuildXml(
                out strOrderXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            XmlDocument domOrders = new XmlDocument();
            try
            {
                domOrders.LoadXml(strOrderXml);
            }
            catch (Exception ex)
            {
                strError = "orders xml load to dom error: " + ex.Message;
                goto ERROR1;
            }

            XmlNode nodeOrders = dom.CreateElement("dprms", "orderCollection", DpNs.dprms);
            dom.DocumentElement.AppendChild(nodeOrders);
            nodeOrders.InnerXml = domOrders.DocumentElement.InnerXml;

            // 评注
            string strCommentXml = "";
            nRet = this.commentControl1.Items.BuildXml(
                out strCommentXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            XmlDocument domComments = new XmlDocument();
            try
            {
                domComments.LoadXml(strCommentXml);
            }
            catch (Exception ex)
            {
                strError = "comments xml load to dom error: " + ex.Message;
                goto ERROR1;
            }

            XmlNode nodeComments = dom.CreateElement("dprms", "commentCollection", DpNs.dprms);
            dom.DocumentElement.AppendChild(nodeComments);
            nodeComments.InnerXml = domComments.DocumentElement.InnerXml;

            try
            {
                dom.Save(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "保存XML文件 '"+dlg.FileName+"' 时出错: " + ex.Message;
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从XML文件中导入全部信息
        private void StripMenuItem_importFromXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bRefreshRefID = true;
            if (Control.ModifierKeys == Keys.Control)
                bRefreshRefID = false;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "请指定要打开的XML文件名";
            dlg.FileName = "";
            // dlg.InitialDirectory = 
            dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(dlg.FileName);
            }
            catch (Exception ex)
            {
                strError = "XML文件 "+dlg.FileName+" 装载失败: " + ex.Message;
                goto ERROR1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            // 书目
            XmlNode node = dom.DocumentElement.SelectSingleNode("dprms:biblio", nsmgr);
            if (node != null)
            {
                // return:
                //      -1  error
                //      0   空的记录
                //      1   成功
                nRet = SetBiblioRecordToMarcEditor(node.OuterXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                Global.ClearHtmlPage(this.webBrowser_biblioRecord, this.MainForm.DataDir);
            }

            // 册
            Hashtable item_refid_change_table = null;
            node = dom.DocumentElement.SelectSingleNode("dprms:itemCollection", nsmgr);
            this.entityControl1.ClearItems();
            if (node != null)
            {
                nRet = this.entityControl1.Items.ImportFromXml(node,
                    this.entityControl1.ListView,
                    bRefreshRefID,
                    out item_refid_change_table,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.entityControl1.BiblioRecPath = this.BiblioRecPath;
            }

            Hashtable order_refid_change_table = new Hashtable();

            // 订购
            node = dom.DocumentElement.SelectSingleNode("dprms:orderCollection", nsmgr);
            this.orderControl1.ClearItems();
            if (node != null)
            {
                // parameters:
                //       changed_refids  累加修改过的 refid 对照表。 原来的 --> 新的
                nRet = this.orderControl1.Items.ImportFromXml(node,
                    this.orderControl1.ListView,
                    bRefreshRefID,
                    ref order_refid_change_table,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.orderControl1.BiblioRecPath = this.BiblioRecPath;
            }

            // 期
            node = dom.DocumentElement.SelectSingleNode("dprms:issueCollection", nsmgr);
            this.issueControl1.ClearItems();
            if (node != null)
            {
                nRet = this.issueControl1.Items.ImportFromXml(node,
                    this.issueControl1.ListView,
                    order_refid_change_table,
                    bRefreshRefID,
                    item_refid_change_table,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.issueControl1.BiblioRecPath = this.BiblioRecPath;
            }

            // 评注
            node = dom.DocumentElement.SelectSingleNode("dprms:commentCollection", nsmgr);
            this.commentControl1.ClearItems();
            if (node != null)
            {
                nRet = this.commentControl1.Items.ImportFromXml(node,
                    this.commentControl1.ListView,
                    bRefreshRefID,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.commentControl1.BiblioRecPath = this.BiblioRecPath;
            }

            SetSaveAllButtonState(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void ToolStripMenuItem_viewMarcJidaoData_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            TestJidaoForm dlg = new TestJidaoForm();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.MARC = this.GetMarc();  // this.m_marcEditor.Marc;
            this.MainForm.AppInfo.LinkFormState(dlg, "entityform_testJidaoForm_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            int nCount = dlg.Xmls.Count;

            if (nCount == 0)
            {
                strError = "没有创建任何期记录";
                goto ERROR1;
            }

            List<string> xmls = dlg.Xmls;
            // 移除publishtime重复的事项
            // return:
            //      -1  出错
            //      0   没有移除的
            //      >0  移除的个数
            nRet = this.issueControl1.RemoveDupPublishTime(ref xmls,    // 2013/9/21 修改过
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == nCount)
            {
                Debug.Assert(dlg.Xmls.Count == 0, "");
                strError = "升级后创建的 " + nCount + " 个期记录当前全部已经存在。放弃新增。";
                goto ERROR1;
            }

            dlg.Xmls = xmls;

            if (nRet > 0)
            {
                DialogResult result = MessageBox.Show(this,
    "升级后创建的 "+nCount+" 个期记录中有以下已经存在：\r\n" + strError + "\r\n\r\n这些重复的期不能加入期记录列表。\r\n\r\n请问是否继续接受其余 "
    +dlg.Xmls.Count.ToString()+" 个期记录? ",
    "EntityForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }


            // 根据期XML数据，创建或者修改期对象
            // TODO: 循环中出错时，要继续做下去，最后再报错？
            // return:
            //      -1  error
            //      0   succeed
            nRet = this.issueControl1.ChangeIssues(dlg.Xmls,
            out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_option_Click(object sender, EventArgs e)
        {
            EntityFormOptionDlg dlg = new EntityFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        private void toolStripButton_clear_Click(object sender, EventArgs e)
        {
            Clear(true);
        }

        private void toolStripButton_saveAll_Click(object sender, EventArgs e)
        {
            button_save_Click(sender, e);
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
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
                this.DoSaveAll();
                return true;
            }

            if (keyData == Keys.F3)
            {
                this.toolStripButton1_marcEditor_saveTo_Click(this, null);
                return true;
            }

            if (keyData == Keys.F4)
            {
                this.toolStrip_marcEditor.Enabled = false;
                try
                {
                    LoadBiblioTemplate(true);
                }
                finally
                {
                    this.toolStrip_marcEditor.Enabled = true;
                }
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

        string m_strVerifyResult = "";

        void DoViewVerifyResult(bool bOpenWindow)
        {
            // string strError = "";

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
                MainForm.SetControlFont(m_verifyViewer, this.Font, false);

                // m_viewer.MainForm = this.MainForm;  // 必须是第一句
                m_verifyViewer.Text = "校验结果";
                m_verifyViewer.ResultString = this.m_strVerifyResult;

                m_verifyViewer.DoDockEvent -= new DoDockEventHandler(m_viewer_DoDockEvent);
                m_verifyViewer.DoDockEvent += new DoDockEventHandler(m_viewer_DoDockEvent);

                m_verifyViewer.FormClosed -= new FormClosedEventHandler(m_viewer_FormClosed);
                m_verifyViewer.FormClosed += new FormClosedEventHandler(m_viewer_FormClosed);

                m_verifyViewer.Locate -= new LocateEventHandler(m_viewer_Locate);
                m_verifyViewer.Locate += new LocateEventHandler(m_viewer_Locate);

            }

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
            /*
        ERROR1:
            MessageBox.Show(this, "DoViewVerifyResult() 出错: " + strError);
             * */
        }

        void m_viewer_DoDockEvent(object sender, DoDockEventArgs e)
        {
            if (this.MainForm.CurrentVerifyResultControl != m_verifyViewer.ResultControl)
            {
                this.MainForm.CurrentVerifyResultControl = m_verifyViewer.ResultControl;
                // 防止内存泄漏
                this.m_verifyViewer.AddFreeControl(m_verifyViewer.ResultControl);
            }

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

            Field field = this.m_marcEditor.Record.Fields[strFieldName, nFieldIndex];
            if (field == null)
            {
                strError = "当前MARC编辑器中不存在 名为 '"+strFieldName+"' 位置为 "+nFieldIndex.ToString()+" 的字段";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strSubfieldName) == true)
            {
                // 字段名
                if (nCharPos == -1)
                {
                    this.m_marcEditor.SetActiveField(field, 2);
                }
                // 字段指示符
                else if (nCharPos == -2)
                {
                    this.m_marcEditor.SetActiveField(field, 1);
                }
                else
                {
                    this.m_marcEditor.FocusedField = field;
                    this.m_marcEditor.SelectCurEdit(nCharPos, 0);
                }
                this.m_marcEditor.EnsureVisible();
                return;
            }

            this.m_marcEditor.FocusedField = field;
            this.m_marcEditor.EnsureVisible();
            
            Subfield subfield = field.Subfields[strSubfieldName, nSubfieldIndex];
            if (subfield == null)
            {
                strError = "当前MARC编辑器中不存在 名为 '" + strSubfieldName + "' 位置为 " + nSubfieldIndex.ToString() + " 的子字段";
                goto ERROR1;
            }

            this.m_marcEditor.SelectCurEdit(subfield.Offset + 2, 0);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void m_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_verifyViewer != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(m_verifyViewer);

                // this.m_verifyViewer = null;
                CloseVerifyViewer();
            }
        }

        void CloseVerifyViewer()
        {
            if (m_verifyViewer != null)
            {
                if (this.MainForm != null
                    && this.MainForm.CurrentVerifyResultControl == m_verifyViewer.ResultControl)
                {
                    // 避免多重拥有。方便后面的 Dispose()
                    this.MainForm.CurrentVerifyResultControl = null;
                }

                this.m_verifyViewer.DisposeFreeControls();
                this.m_verifyViewer = null;
            }
        }

        private void toolStripButton_verifyData_Click(object sender, EventArgs e)
        {
            GenerateDataEventArgs e1 = new GenerateDataEventArgs();
            e1.FocusedControl = this.m_marcEditor;
            this.VerifyData(this, e1, false);
        }

        /// <summary>
        /// 检索面板是否可见
        /// </summary>
        public bool QueryPanelVisibie
        {
            get
            {
                return this.flowLayoutPanel_query.Visible;
            }
            set
            {
                this.flowLayoutPanel_query.Visible = value;
                this.MainForm.AppInfo.SetBoolean(
"entityform",
"queryPanel_visibie",
value);
            }
        }

        private void toolStripButton_hideSearchPanel_Click(object sender, EventArgs e)
        {
            this.QueryPanelVisibie = false;
        }

        private void toolStripButton_hideItemQuickImput_Click(object sender, EventArgs e)
        {
            this.ItemQuickInputPanelVisibie = false;
        }

        /// <summary>
        /// 快速输入册面板部分是否可见
        /// </summary>
        public bool ItemQuickInputPanelVisibie
        {
            get
            {
                return this.panel_itemQuickInput.Visible;
            }
            set
            {
                this.panel_itemQuickInput.Visible = value;
                this.MainForm.AppInfo.SetBoolean(
"entityform",
"itemQuickInputPanel_visibie",
value);
            }
        }

        private void entityControl1_Enter(object sender, EventArgs e)
        {
            // 显示Ctrl+A菜单
            if (this.MainForm.PanelFixedVisible == true)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.entityControl1.ListView;
                e1.ScriptEntry = "";    // 启动Ctrl+A菜单
                this._genData.AutoGenerate(this.entityControl1,
                    e1,
                    GetBiblioRecPathOrSyntax(),
                    true);
            }
        }

        string GetBiblioRecPathOrSyntax()
        {
            if (string.IsNullOrEmpty(this.BiblioRecPath) == false)
                return this.BiblioRecPath;
            if (string.IsNullOrEmpty(this.MarcSyntax) == false)
                return "format:" + this.MarcSyntax;
            return "";
        }

        private void entityControl1_Leave(object sender, EventArgs e)
        {
            /*
            // 清理Ctrl+A菜单
            if (this.MainForm.PanelFixedVisible == true)
            {
                if (this.m_genDataViewer != null)
                    this.m_genDataViewer.Clear();
            }
             * */
        }

        private void MarcEditor_SelectedFieldChanged(object sender, EventArgs e)
        {
            this._genData.RefreshViewerState();
        }

        private void binaryResControl1_Enter(object sender, EventArgs e)
        {
            // 显示Ctrl+A菜单
            if (this.MainForm.PanelFixedVisible == true)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.binaryResControl1.ListView;
                e1.ScriptEntry = "";    // 启动Ctrl+A菜单
                this._genData.AutoGenerate(this.binaryResControl1, 
                    e1,
                    GetBiblioRecPathOrSyntax(),
                    true);
            }
        }

        private void MarcEditor_GetTemplateDef(object sender, GetTemplateDefEventArgs e)
        {
            if (this._genData.DetailHostObj == null)
            {
                int nRet = 0;
                string strError = "";

                // 初始化 dp2circulation_marc_autogen.cs 的 Assembly，并new DetailHost对象
                // return:
                //      -1  error
                //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                //      1   重新(或者首次)初始化了Assembly
                nRet = this._genData.InitialAutogenAssembly(this.BiblioRecPath, // null,
                    out strError);
                if (nRet == -1)
                {
                    e.ErrorInfo = strError;
                    return;
                }
                if (nRet == 0)
                {
                    if (this._genData.DetailHostObj == null)
                    {
                        e.Canceled = true;
                        return; // 库名不具备，无法初始化
                    }
                }
                Debug.Assert(this._genData.DetailHostObj != null, "");
            }

            Debug.Assert(this._genData.DetailHostObj != null, "");

            // 如果脚本里面没有相应的回调函数
            if (this._genData.DetailHostObj.GetType().GetMethod("GetTemplateDef",
                BindingFlags.DeclaredOnly |
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod
                ) == null)
            {
                e.Canceled = true;
                return;
            }

            /*
            dynamic o = this.m_detailHostObj;
            try
            {
                o.GetTemplateDef(sender, e);
            }
            catch (Exception ex)
            {
                e.ErrorInfo = ex.Message;
                return;
            }
             * */
            // 有两个参数的成员函数
            Type classType = _genData.DetailHostObj.GetType();
            try
            {
                classType.InvokeMember("GetTemplateDef",
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.InvokeMethod
                    ,
                    null,
                    this._genData.DetailHostObj,
                    new object[] { sender, e });
            }
            catch (Exception ex)
            {
                e.ErrorInfo = GetExceptionMessage(ex) + "\r\n\r\n" +  ExceptionUtil.GetDebugText(ex);  // GetExceptionMessage(ex);
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

        // 使能记录删除后的“全部保存”按钮
        private void ToolStripMenuItem_enableSaveAllButton_Click(object sender, EventArgs e)
        {
            if (this.DeletedMode == false)
            {
                MessageBox.Show(this, "已经在普通模式");
                return;
            }

            this.entityControl1.ChangeAllItemToNewState();
            this.issueControl1.ChangeAllItemToNewState();
            this.orderControl1.ChangeAllItemToNewState();
            this.commentControl1.ChangeAllItemToNewState();

            // 将MarcEditor修改标记变为true
            // this.m_marcEditor.Changed = true; // 这一句决定了使能后如果立即关闭EntityForm窗口，是否会警告(书目)内容丢失
            this.SetMarcChanged(true);

            this.DeletedMode = false;
        }

        // 移动书目记录
        private void toolStripButton_marcEditor_moveTo_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.MainForm.ServerVersion < 2.39)
            {
                strError = "本功能需要配合 dp2library 2.39 或以上版本才能使用";
                goto ERROR1;
            }

            string strTargetRecPath = this.m_marcEditor.Record.Fields.GetFirstSubfield("998", "t");
            if (string.IsNullOrEmpty(strTargetRecPath) == false)
            {
                DialogResult result = MessageBox.Show(this,
    "当前窗口内的记录原本是从 '" + strTargetRecPath + "' 复制过来的。是否要移动回原有位置？\r\n\r\nYes: 是; No: 否，继续进行普通移动操作; Cancel: 放弃本次操作",
    "EntityForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // strTargetRecPath会发生作用
                }

                if (result == System.Windows.Forms.DialogResult.No)
                {
                    strTargetRecPath = "";
                }
            }

            // 源记录就是 ？
            if (Global.IsAppendRecPath(this.BiblioRecPath) == true)
            {
                strError = "源记录尚未建立，无法执行移动操作";
                goto ERROR1;
            }

            // string strMergeStyle = "";
            MergeStyle merge_style = MergeStyle.CombinSubrecord | MergeStyle.ReserveSourceBiblio;

            BiblioSaveToDlg dlg = new BiblioSaveToDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "移动书目记录到 ...";
            dlg.MainForm = this.MainForm;
            if (string.IsNullOrEmpty(strTargetRecPath) == false)
                dlg.RecPath = strTargetRecPath;
            else
            {
                dlg.RecPath = this.MainForm.AppInfo.GetString(
                    "entity_form",
                    "move_to_used_path",
                    this.BiblioRecPath);
                dlg.RecID = "?";
            }

            dlg.MessageText = "将当前窗口中的书目记录 " + this.BiblioRecPath + " (连同下属的册、期、订购、实体记录和对象资源)移动到:";
            dlg.CopyChildRecords = true;
            dlg.EnableCopyChildRecords = false;

            dlg.BuildLink = false;

            {
                string strMarcSyntax = this.GetCurrentMarcSyntax();
                if (string.IsNullOrEmpty(strMarcSyntax) == true)
                    strMarcSyntax = this.MarcSyntax;    // 外来数据的 MARC 格式

                dlg.MarcSyntax = strMarcSyntax;
            }

            // dlg.CurrentBiblioRecPath = this.BiblioRecPath;
            this.MainForm.AppInfo.LinkFormState(dlg, "entityform_BiblioMoveToDlg_state");
            dlg.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (this.BiblioRecPath == dlg.RecPath)
            {
                strError = "要移动到的位置 '" + dlg.RecPath + "' 和当前记录本来的位置 '" + this.BiblioRecPath + "' 相同，移动操作被拒绝。若确实要这样保存记录，请直接使用保存功能。";
                goto ERROR1;
            }

            this.MainForm.AppInfo.SetString(
    "entity_form",
    "move_to_used_path",
    dlg.RecPath);

            // if (dlg.CopyChildRecords == true)
            {
                // 如果当前记录没有保存，则先保存
                if (this.EntitiesChanged == true
        || this.IssuesChanged == true
        || this.BiblioChanged == true
        || this.ObjectChanged == true
        || this.OrdersChanged == true
        || this.CommentsChanged == true)
                {
                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(this,
                        "当前窗口内有 " + GetCurrentChangedPartName() + " 被修改后尚未保存。移动操作前必须先保存当前记录。\r\n\r\n请问要立即保存么？",
                        "EntityForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.OK)
                    {
                        // 提交所有保存请求
                        // return:
                        //      -1  有错。此时不排除有些信息保存成功。
                        //      0   成功。
                        nRet = DoSaveAll();
                        if (nRet == -1)
                        {
                            strError = "因为保存操作出错，所以后续的移动操作被放弃";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "移动操作被放弃";
                        goto ERROR1;
                    }
                }
            }

            // 看看要另存的位置，记录是否已经存在?
            if (dlg.RecID != "?")
            {
                byte[] timestamp = null;

                // 检测特定位置书目记录是否已经存在
                // parameters:
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = DetectBiblioRecord(dlg.RecPath,
                    out timestamp,
                    out strError);
                if (nRet == 1)
                {
                    //bool bOverwrite = false;
                    if (dlg.RecPath != strTargetRecPath)    // 移动回998$t情况就不询问是否覆盖了，直接选用归并方式
                    {
#if NO
                        // TODO: 用专用对话框实现
                        // 提醒覆盖？
                        DialogResult result = MessageBox.Show(this,
                            "目标书目记录 " + dlg.RecPath + " 已经存在。\r\n\r\n要用当前窗口中的书目记录(连同数字对象和下属的子记录)覆盖此记录，还是归并到此记录? \r\n\r\nYes: 覆盖; No: 归并; Cancel: 放弃本次移动操作",
                            "EntityForm",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Cancel)
                            return;
                        if (result == System.Windows.Forms.DialogResult.Yes)
                            bOverwrite = true;
                        else
                            bOverwrite = false;
#endif
                        GetMergeStyleDialog merge_dlg = new GetMergeStyleDialog();
                        MainForm.SetControlFont(merge_dlg, this.Font, false);
                        merge_dlg.SourceRecPath = this.BiblioRecPath;
                        merge_dlg.TargetRecPath = dlg.RecPath;
                        merge_dlg.MessageText = "目标书目记录 " + dlg.RecPath + " 已经存在。\r\n\r\n请指定当前窗口中的书目记录(源)和此目标记录合并的方法";

                        merge_dlg.UiState = this.MainForm.AppInfo.GetString(
        "entity_form",
        "GetMergeStyleDialog_uiState",
        "");
                        this.MainForm.AppInfo.LinkFormState(merge_dlg, "entityform_GetMergeStyleDialog_state");
                        merge_dlg.ShowDialog(this);
                        this.MainForm.AppInfo.UnlinkFormState(merge_dlg);
                        this.MainForm.AppInfo.SetString(
"entity_form",
"GetMergeStyleDialog_uiState",
merge_dlg.UiState);

                        if (merge_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            return;

                        merge_style = merge_dlg.GetMergeStyle();
                    }

                    // this.BiblioTimestamp = timestamp;   // 为了顺利覆盖

                    // TODO: 预先检查操作者权限，确保删除书目记录和下级记录都能成功，否则就警告

#if NO
                    if (bOverwrite == true)
                    {
                        // 删除目标位置的书目记录
                        // 如果为归并模式，则保留其下属的实体等记录
                        nRet = DeleteBiblioRecordFromDatabase(dlg.RecPath,
                            bOverwrite == true ? "delete" : "onlydeletebiblio",
                            timestamp,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
#endif
                    if ((merge_style & MergeStyle.OverwriteSubrecord)!= 0)
                    {
                        // 删除目标记录整个，或者删除目标位置的下级记录
                        // TODO: 测试的时候，注意不用下述调用而测试保留目标书目记录中对象的可能性
                        nRet = DeleteBiblioRecordFromDatabase(dlg.RecPath,
                            (merge_style & MergeStyle.ReserveSourceBiblio)!= 0 ? "delete" : "onlydeletesubrecord",
                            timestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            if ((merge_style & MergeStyle.ReserveSourceBiblio) != 0)
                                strError = "删除目标位置的书目记录 '"+dlg.RecPath+"' 时出错: " + strError;
                            else
                                strError = "删除目标位置的书目记录 '" + dlg.RecPath + "' 的全部子记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }
                }
            }

            string strOutputBiblioRecPath = "";
            byte[] baOutputTimestamp = null;
            string strXml = "";

            string strOldBiblioRecPath = this.BiblioRecPath;
            string strOldMarc = this.GetMarc(); //  this.m_marcEditor.Marc;
            bool bOldChanged = this.GetMarcChanged();   // this.m_marcEditor.Changed;

            try
            {
                // 保存原来的记录路径
                bool bOldReadOnly = this.m_marcEditor.ReadOnly;
                Field old_998 = null;

                string strDlgTargetDbName = Global.GetDbName(dlg.RecPath);
                string str998TargetDbName = Global.GetDbName(strTargetRecPath);

                // 如果移动目标和strTargetRecPath同数据库，则要去掉记录中可能存在的998$t
                if (strDlgTargetDbName == str998TargetDbName)
                {
                    // 保存当前记录的998字段
                    old_998 = this.m_marcEditor.Record.Fields.GetOneField("998", 0);

                    // 清除可能存在的998$t
                    if (old_998 != null)
                    {
                        SubfieldCollection subfields = old_998.Subfields;
                        Subfield old_t = subfields["t"];
                        if (old_t != null)
                        {
                            old_998.Subfields = subfields.Remove(old_t);
                            // 如果998内一个子字段也没有了，是否这个字段要删除?
                        }
                        else
                            old_998 = null; // 表示(既然没有删除$t，就)不用恢复
                    }
                }

                string strMergeStyle = "";
                if ((merge_style & MergeStyle.ReserveSourceBiblio) != 0)
                    strMergeStyle = "reserve_source";
                else
                    strMergeStyle = "reserve_target";

                if ((merge_style & MergeStyle.MissingSourceSubrecord) != 0)
                    strMergeStyle += ",missing_source_subrecord";
                else if ((merge_style & MergeStyle.OverwriteSubrecord) != 0)
                {
                    // dp2library 尚未实现这个功能，不过本函数前面已经用 SetBiblioInfo() API 主动删除了目标位置下属的子记录，效果是一样的。(当然，这样实现起来原子性不是那么好)
                    // strMergeStyle += ",overwrite_target_subrecord";
                }
                // combine 情况时缺省的，不用声明

                nRet = CopyBiblio(
                    "move",
                    dlg.RecPath,
                    strMergeStyle,
                    out strXml,
                    out strOutputBiblioRecPath,
                    out baOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);

            }
            finally
            {
#if NO
                // 复原当前窗口的记录
                if (this.m_marcEditor.Marc != strOldMarc)
                    this.m_marcEditor.Marc = strOldMarc;
                if (this.m_marcEditor.Changed != bOldChanged)
                    this.m_marcEditor.Changed = bOldChanged;
#endif
                if (this.GetMarc() /*this.m_marcEditor.Marc*/ != strOldMarc)
                {
                    // this.m_marcEditor.Marc = strOldMarc;
                    this.SetMarc(strOldMarc);
                }
                if (this.GetMarcChanged() /*this.m_marcEditor.Changed*/ != bOldChanged)
                {
                    // this.m_marcEditor.Changed = bOldChanged;
                    this.SetMarcChanged(bOldChanged);
                }
            }

            if (nRet == -1)
            {
                this.BiblioRecPath = strOldBiblioRecPath;
                return;
            }

            // 将目标记录装入当前窗口
            this.LoadRecordOld(strOutputBiblioRecPath, "", false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripSplitButton_searchDup_ButtonClick(object sender, EventArgs e)
        {
            ToolStripMenuItem_searchDupInExistWindow_Click(sender, e);
        }

        private void ToolStripMenuItem_searchDupInExistWindow_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,   // 包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            DupForm form = this.MainForm.GetTopChildWindow<DupForm>();
            if (form == null)
            {
                form = new DupForm();

                form.MainForm = this.MainForm;
                form.MdiParent = this.MainForm;

                form.ProjectName = "<默认>";
                form.XmlRecord = strXmlBody;
                form.RecordPath = this.BiblioRecPath;

                form.AutoBeginSearch = true;

                form.Show();
            }
            else
            {
                form.Activate();
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = this.WindowState;

                form.ProjectName = "<默认>";
                form.XmlRecord = strXmlBody;
                form.RecordPath = this.BiblioRecPath;

                form.BeginSearch();
            }


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void ToolStripMenuItem_searchDupInNewWindow_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 获得书目记录XML格式
            string strXmlBody = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,   // 包含资源ID
                out strXmlBody,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            DupForm form = new DupForm();

            form.MainForm = this.MainForm;
            form.MdiParent = this.MainForm;

            form.ProjectName = "<默认>";
            form.XmlRecord = strXmlBody;
            form.RecordPath = this.BiblioRecPath;

            form.AutoBeginSearch = true;

            form.Show();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(this.MainForm.DataDir, "operloghtml.css");

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

        void DoViewComment(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
            }

            string strMARC = this.GetMarc();    // this.m_marcEditor.Marc;

            // 获得书目记录XML格式
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                true,   // 包含资源ID
                out strXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strFragmentXml = "";
            nRet = MarcUtil.LoadXmlFragment(strXml,
                out strFragmentXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.m_strActiveCatalogingRules != "<不过滤>")
            {
                // 按照编目规则过滤
                // 获得一个特定风格的 MARC 记录
                // parameters:
                //      strStyle    要匹配的style值。如果为null，表示任何44值都匹配，实际上效果是去除$4并返回全部字段内容
                // return:
                //      0   没有实质性修改
                //      1   有实质性修改
                nRet = MarcUtil.GetMappedRecord(ref strMARC,
                    this.m_strActiveCatalogingRules);
            }

            // 2015/1/3
            string strImageFragment = BiblioSearchForm.GetImageHtmlFragment(
this.BiblioRecPath,
strMARC);

            strHtml = MarcUtil.GetHtmlOfMarc(strMARC,
                strFragmentXml,
                strImageFragment,
                false);
            string strFilterTitle = "";
            if (this.m_strActiveCatalogingRules != "<不过滤>")
            {
                if (string.IsNullOrEmpty(this.m_strActiveCatalogingRules) == true)
                    strFilterTitle = "过滤掉命令字符(保留全部编目规则)";
                else
                    strFilterTitle = "按编目规则 '" + this.m_strActiveCatalogingRules + "' 过滤";

                strFilterTitle = "<div class='cataloging_rule_title'>" + strFilterTitle + "</div>";
            }

            // TODO: 如果有改变，则显示先后对照?
            strHtml = "<html>" +
    GetHeadString() +
    "<body>" +
    strFilterTitle + 
    strHtml +
    GetTimestampHtml(this.BiblioTimestamp) +
    "</body></html>";
            bool bNew = false;
            if (this.m_commentViewer == null
                || (bOpenWindow == true && this.m_commentViewer.Visible == false))
            {
                m_commentViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_commentViewer, this.Font, false);
                bNew = true;
            }

            m_commentViewer.MainForm = this.MainForm;  // 必须是第一句

            if (bNew == true)
                m_commentViewer.InitialWebBrowser();

            m_commentViewer.Text = "MARC内容 '" + this.BiblioRecPath + "'";
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = strXml;
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // this.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    this.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_commentViewer.WindowState == FormWindowState.Minimized)
                        m_commentViewer.WindowState = FormWindowState.Normal;
                    m_commentViewer.Activate();
                }
            }
            else
            {
                if (m_commentViewer.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewComment() 出错: " + strError);
        }

        void marc_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        internal static string GetTimestampHtml(byte [] timestamp)
        {
            return "<p>书目记录时间戳: " + ByteArray.GetHexTimeStampString(timestamp) + "</p>";
        }

        List<string> GetExistCatalogingRules()
        {
            return Global.GetExistCatalogingRules(this.GetMarc()/*this.m_marcEditor.Marc*/);

        }

        // 当前活动的编目规则
        //      "" 表示每个编目规则都允许，但显示的时候会过滤掉 $* 和 {cr:...}
        //      "<不过滤>" 表示不过滤
        string m_strActiveCatalogingRules = "<不过滤>";

        private void toolStripDropDownButton_marcEditor_someFunc_DropDownOpening(object sender, EventArgs e)
        {
            this.toolStripMenuItem_marcEditor_setActiveCatalogingRule.DropDownItems.Clear();
            List<string> catalogrules = GetExistCatalogingRules();
            catalogrules.Insert(0, "<不过滤>");
            catalogrules.Insert(1, "");
            bool bFound = false;
            foreach(string s in catalogrules)
            {
                string strName = s;
                if (string.IsNullOrEmpty(s) == true)
                    strName = "<全部>";
                ToolStripMenuItem submenu = new ToolStripMenuItem(); 
                submenu.Text = strName;
                if (s == m_strActiveCatalogingRules)
                {
                    submenu.Checked = true;
                    bFound = true;
                }
                submenu.Click += new EventHandler(catalogingrule_submenu_Click);
                submenu.Tag = s;

                this.toolStripMenuItem_marcEditor_setActiveCatalogingRule.DropDownItems.Add(submenu);
            }

            if (bFound == false)
            {
                ToolStripMenuItem submenu = new ToolStripMenuItem();
                submenu.Text = this.m_strActiveCatalogingRules;
                submenu.Checked = true;
                submenu.Click += new EventHandler(catalogingrule_submenu_Click);
                submenu.Tag = this.m_strActiveCatalogingRules;

                this.toolStripMenuItem_marcEditor_setActiveCatalogingRule.DropDownItems.Add(submenu);
            }

        }

        void catalogingrule_submenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuitem = (ToolStripMenuItem)sender;
            this.m_strActiveCatalogingRules = (string)menuitem.Tag;

            DoViewComment(false);
        }

        /// <summary>
        /// 是否显示其他分馆的册记录
        /// </summary>
        public bool DisplayOtherLibraryItem
        {
            get
            {
                // 显示其他分馆的册记录
                return this.MainForm.AppInfo.GetBoolean(
    "entityform",
    "displayOtherLibraryItem",
    false);
            }
        }

#if NO
        // 
        /// <summary>
        /// 设置当前记录的图片对象
        /// </summary>
        /// <param name="image">Image 对象</param>
        /// <param name="strUsage">用途字符串</param>
        /// <param name="strShrinkComment">返回缩放提示字符串</param>
        /// <param name="strID">返回实际使用的对象 ID</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>0: 成功; -1: 出错</returns>
        public int SetImageObject(Image image,
            string strUsage,
            out string strShrinkComment,
            out string strID,
            out string strError)
        {
            strError = "";
            strShrinkComment = "";
            strID = "";
            int nRet = 0;

            // 自动缩小图像
            string strMaxWidth = this.MainForm.AppInfo.GetString(
    "entityform",
    "paste_pic_maxwidth",
    "-1");
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

            string strTempFilePath = FileUtil.NewTempFileName(this.MainForm.DataDir,
                "~temp_make_pic_",
                ".png");

            image.Save(strTempFilePath, System.Drawing.Imaging.ImageFormat.Png);
            image.Dispose();
            image = null;

            ListViewItem item = null;
            List<ListViewItem> items = this.binaryResControl1.FindItemByUsage(strUsage);
            if (items.Count == 0)
            {
                nRet = this.binaryResControl1.AppendNewItem(
                    strTempFilePath,
                    strUsage,
                    out item,
                    out strError);
            }
            else
            {
                item = items[0];
                nRet = this.binaryResControl1.ChangeObjectFile(item,
                    strTempFilePath,
                    strUsage,
                    out strError);
            }
            if (nRet == -1)
                goto ERROR1;

            strID = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_ID);

            return 0;
        ERROR1:
            return -1;
        }

#endif

        /// <summary>
        /// 设置当前记录的图片对象
        /// </summary>
        /// <param name="image">Image 对象</param>
        /// <param name="strUsage">用途字符串</param>
        /// <param name="strID">返回实际使用的对象 ID</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>0: 成功; -1: 出错</returns>
        public int SetImageObject(Image image,
            string strUsage,
            out string strID,
            out string strError)
        {
            strError = "";
            strID = "";
            int nRet = 0;

            string strTempFilePath = FileUtil.NewTempFileName(this.MainForm.DataDir,
                "~temp_make_pic_",
                ".png");

            image.Save(strTempFilePath, System.Drawing.Imaging.ImageFormat.Png);
            //image.Dispose();
            //image = null;

            ListViewItem item = null;
            List<ListViewItem> items = this.binaryResControl1.FindItemByUsage(strUsage);
            if (items.Count == 0)
            {
                nRet = this.binaryResControl1.AppendNewItem(
                    strTempFilePath,
                    strUsage,
                    "", // rights
                    out item,
                    out strError);
            }
            else
            {
                item = items[0];

                nRet = this.binaryResControl1.ChangeObjectFile(item,
                    strTempFilePath,
                    strUsage,
                    out strError);
            }
            if (nRet == -1)
                goto ERROR1;

            strID = ListViewUtil.GetItemText(item, BinaryResControl.COLUMN_ID);

            return 0;
        ERROR1:
            return -1;
        }

        // return:
        //      -1  出错
        //      其他  实际删除的个数
        public int DeleteImageObject(string strID)
        {
            List<ListViewItem> items = this.binaryResControl1.FindItemByID(strID);
            if (items.Count > 0)
                return this.binaryResControl1.MaskDelete(items);
            return 0;
        }
#if NO
        static bool IsAnImage(string filename)
        {
            try
            {
                Image newImage = Image.FromFile(filename);
            }
            catch (OutOfMemoryException ex)
            {
                // Image.FromFile will throw this if file is invalid.
                return false;
            }
            return true;
        }
#endif

        // 从剪贴板插入封面图像
        private void ToolStripMenuItem_insertCoverImageFromClipboard_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            List<ListViewItem> deleted_items = this.binaryResControl1.FindAllMaskDeleteItem();
            if (deleted_items.Count > 0)
            {
                strError = "当前有标记删除的对象尚未提交保存。请先提交这些保存后，再进行插入封面图像的操作";
                goto ERROR1;
            }

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

            CreateCoverImageDialog dlg = null;
            try
            {
                dlg = new CreateCoverImageDialog();

                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.OriginImage = image;
                this.MainForm.AppInfo.LinkFormState(dlg, "entityform_CreateCoverImageDialog_state");
                dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(dlg);
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

            this.SynchronizeMarc();

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
                nRet = SetImageObject(type.Image,
                    strType,    // "coverimage",
                    // out strShrinkComment,
                    out strID,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

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

                field_856.IndicatorAndValue = ("72$3Cover Image$" + DetailHost.LinkSubfieldName + "uri:" + strID + "$xtype:"+strType+";size:"+strSize+"$2dp2res").Replace('$', (char)31);
            }

            if (this.tabControl_biblioInfo.SelectedTab == this.tabPage_template)
                this.SynchronizeMarc();

            MessageBox.Show(this, "封面图像和856字段已经成功创建。\r\n"
                // + strShrinkComment
                + "\r\n\r\n(但因当前记录还未保存，图像数据尚未提交到服务器)\r\n\r\n注意稍后保存当前记录。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void checkedComboBox_dbName_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_biblioDbNames.Items.Count > 0)
                return;

            this.checkedComboBox_biblioDbNames.Items.Add("<全部>");

            if (this.MainForm.BiblioDbProperties != null)
            {
                for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                {
                    BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];
                    this.checkedComboBox_biblioDbNames.Items.Add(property.DbName);
                }
            }
        }

        private void checkedComboBox_dbName_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListView list = e.Item.ListView;

            if (e.Item.Text == "<全部>" || e.Item.Text.ToLower() == "<all>")
            {
                if (e.Item.Checked == true)
                {
                    // 如果当前勾选了“全部”，则清除其余全部事项的勾选
                    for (int i = 0; i < list.Items.Count; i++)
                    {
                        ListViewItem item = list.Items[i];
                        if (item.Text == "<全部>" || item.Text.ToLower() == "<all>")
                            continue;
                        if (item.Checked != false)
                            item.Checked = false;
                    }
                }
            }
            else
            {
                if (e.Item.Checked == true)
                {
                    // 如果勾选的不是“全部”，则要清除“全部”上可能的勾选
                    for (int i = 0; i < list.Items.Count; i++)
                    {
                        ListViewItem item = list.Items[i];
                        if (item.Text == "<全部>" || item.Text.ToLower() == "<all>")
                        {
                            if (item.Checked != false)
                                item.Checked = false;
                        }
                    }
                }
            }

        }

        private void MenuItem_marcEditor_getKeys_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 获得书目记录XML格式
            string strBiblioXml = "";
            int nRet = this.GetBiblioXml(
                "", // 迫使从记录路径中看marc格式
                false,
                out strBiblioXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strResultXml = "";
            nRet = GetKeys(this.BiblioRecPath,
                strBiblioXml,
                out strResultXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            XmlViewerForm dlg = new XmlViewerForm();

            dlg.Text = "书目记录的检索点";
            dlg.MainForm = this.MainForm;
            dlg.XmlString = strResultXml;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this); 
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int GetKeys(string strBiblioRecPath,
            string strBiblioXml,
            out string strResultXml,
            out string strError)
        {
            strError = "";
            strResultXml = "";

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在获得书目记录 " + strBiblioRecPath + " 的检索点 ...");
            Progress.BeginLoop();

            try
            {

                string[] formats = new string[1];
                formats[0] = "keys";

                string[] results = null;
                byte[] timestamp = null;
                long lRet = Channel.GetBiblioInfos(
                    Progress,
                    strBiblioRecPath,
                    strBiblioXml,
                    formats,
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (results != null && results.Length > 0)
                    strResultXml = results[0];
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
        }

        bool _readOnly = false;
        public bool ReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                this._readOnly = value;
                this.tableLayoutPanel_main.Enabled = !value;
            }
        }

        private void tabControl_biblioInfo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_biblioInfo.SelectedTab == this.tabPage_marc
                || this.tabControl_biblioInfo.SelectedTab == this.tabPage_template)
                SynchronizeMarc();
        }

        private void MenuItem_marcEditor_editMacroTable_Click(object sender, EventArgs e)
        {
            MacroTableDialog dlg = new MacroTableDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.XmlFileName = Path.Combine(this.MainForm.DataDir, "marceditor_macrotable.xml");

            this.MainForm.AppInfo.LinkFormState(dlg, "entityform_MacroTableDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
        }

        private void toolStripSplitButton_insertCoverImage_ButtonClick(object sender, EventArgs e)
        {
            ToolStripMenuItem_insertCoverImageFromClipboard_Click(sender, e);
        }

        private void ToolStripMenuItem_removeCoverImage_Click(object sender, EventArgs e)
        {
            bool bChanged = false;
            MarcRecord record = new MarcRecord(this.GetMarc());
            MarcNodeList subfields = record.select("field[@name='856']/subfield[@name='x']");

            foreach (MarcSubfield subfield in subfields)
            {
                string x = subfield.Content;
                if (string.IsNullOrEmpty(x) == true)
                    continue;
                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strType = (string)table["type"];
                if (string.IsNullOrEmpty(strType) == true)
                    continue;
                if (StringUtil.HasHead(strType, "FrontCover") == true)
                {
                    string u = subfield.Parent.select("subfield[@name='u']").FirstContent;

                    subfield.Parent.detach();
                    bChanged = true;

                    DeleteImageObject(GetImageID(u));
                }
            }

            if (bChanged == true)
                this.SetMarc(record.Text);
            else
                MessageBox.Show(this, "没有发现封面图像的 856 字段");
        }

        static string GetImageID(string strUri)
        {
            if (StringUtil.HasHead(strUri, "http:") == true)
                return null;
            if (StringUtil.HasHead(strUri, "uri:") == true)
                return strUri.Substring(4).Trim();
            return strUri;
        }
    }

    /// <summary>
    /// 册登记按钮的动作类型
    /// </summary>
    public enum RegisterType
    {
        /// <summary>
        /// 只检索
        /// </summary>
        SearchOnly = 0,   // 只检索
        /// <summary>
        /// 快速登记
        /// </summary>
        QuickRegister = 1, // 快速登记
        /// <summary>
        /// 登记
        /// </summary>
        Register = 2, // 登记
    }

#if NO
    // 加四角号码时的处理规则
    public enum SjHmStyle
    {
        None = 0,	// 不做任何改变
    }
#endif

    class PendingLoadRequest
    {
        public string RecPath = "";
        public string PrevNextStyle = "";
    }

    /// <summary>
    /// 校验数据的宿主类
    /// </summary>
    public class VerifyHost
    {
        /// <summary>
        /// 种册窗
        /// </summary>
        public EntityForm DetailForm = null;

        /// <summary>
        /// 结果字符串
        /// </summary>
        public string ResultString = "";

        /// <summary>
        /// 脚本编译后的 Assembly
        /// </summary>
        public Assembly Assembly = null;

        /// <summary>
        /// 调用一个功能函数
        /// </summary>
        /// <param name="strFuncName">功能名称</param>
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

        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void Main(object sender, HostEventArgs e)
        {

        }
    }

    /// <summary>
    /// 用于数据校验的 FilterDocument 派生类(MARC 过滤器文档类)
    /// </summary>
    public class VerifyFilterDocument : FilterDocument
    {
        /// <summary>
        /// 宿主对象
        /// </summary>
        public VerifyHost FilterHost = null;
    }
}