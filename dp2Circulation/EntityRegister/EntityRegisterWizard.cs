
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.AmazonInterface;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.CommonControl;
using DigitalPlatform.EasyMarc;
using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.MessageClient;

namespace dp2Circulation
{
    /// <summary>
    /// 册登记向导窗口。适合初学者使用的册登记窗口
    /// </summary>
    public partial class EntityRegisterWizard : MyForm, IBiblioItemsWindow
    {
        GenerateData _genData = null;

        // FloatingMessageForm _floatingMessage = null;

        EntityRegisterBase _base = new EntityRegisterBase();

        BiblioAndEntities _biblio = null;

        public EntityRegisterWizard()
        {
            InitializeComponent();

            // 当册记录区在右边竖放的时候，尽量避免出现水平卷滚条
            this.flowLayoutPanel1.Padding = new Padding(0, 0, SystemInformation.VerticalScrollBarWidth + 2, 0);

            this.dpTable_browseLines.ImageList = this.imageList_progress;
            CreateBrowseColumns();

            // Add any initialization after the InitializeComponent() call.
            this.NativeTabControl1 = new NativeTabControl();
            this.NativeTabControl1.AssignHandle(this.tabControl_main.Handle);

            _biblio = new BiblioAndEntities(this,
                easyMarcControl1, 
                flowLayoutPanel1);
            _biblio.GetValueTable += _biblio_GetValueTable;
            _biblio.DeleteEntity += _biblio_DeleteEntity;
            _biblio.LoadEntities += _biblio_LoadEntities;
            _biblio.GetEntityDefault += _biblio_GetEntityDefault;
            _biblio.GenerateData += _biblio_GenerateData;
            _biblio.VerifyBarcode += _biblio_VerifyBarcode;
            _biblio.EntitySelectionChanged += _biblio_EntitySelectionChanged;

            this._imageManager = new ImageManager();
            this._imageManager.ChannelPool = this._base._channelPool;
            this._imageManager.GetObjectComplete += new GetObjectCompleteEventHandler(_imageManager_GetObjectComplete);
            this._imageManager.BeginThread();

        }

        void _imageManager_GetObjectComplete(object sender, GetObjectCompleteEventArgs e)
        {
            EventFilter filter = e.TraceObject.Tag as EventFilter;
            this.AsyncGetImageComplete(filter.Row,
                filter.BiblioRecPath,
                e.TraceObject != null ? e.TraceObject.FileName : null,
                e.ErrorInfo);
        }

        // 册记录编辑控件中，选择发生了变化。sender 为发出消息的 EntityEditControl，或者加号按钮 PlusButton
        void _biblio_EntitySelectionChanged(object sender, EventArgs e)
        {
            if (this._keyboardForm == null
|| this._inWizardControl > 0)
                return;
            else
                this.BeginInvoke(new Action<object, EventArgs>(OnEntitySelectionChanged), sender, e);
        }

        void OnEntitySelectionChanged(object sender, EventArgs e)
        {
            if (this._keyboardForm == null
    || this._inWizardControl > 0)
                return;

            if (sender is PlusButton)
            {
                this._keyboardForm.SetCurrentEntityLine(null, null);
                SetKeyboardFormStep(KeyboardForm.Step.EditEntity, "dont_hilight");
                return;
            }

            EntityEditControl control = sender as EntityEditControl;   // this._biblio.GetFocusedEditControl();
            if (control != null)
            {
                EditLine line = GetFocuedEntityLine(control);
                this._keyboardForm.SetCurrentEntityLine(control, line);
                SetKeyboardFormStep(KeyboardForm.Step.EditEntity, "dont_hilight");
                return;
            }

            this._keyboardForm.SetCurrentEntityLine(null, null);
            SetKeyboardFormStep(KeyboardForm.Step.EditEntity, "dont_hilight");

        }

        void _biblio_VerifyBarcode(object sender, VerifyBarcodeEventArgs e)
        {
            string strError = "";
            e.Result = this.VerifyBarcode(
                this.Channel.LibraryCodeList,
                e.Barcode,
                out strError);
            e.ErrorInfo = strError;
        }

        public override void EnableControls(bool bEnable)
        {
            this.tabControl_main.Enabled = bEnable;
            this.toolStrip1.Enabled = bEnable;
        }

        void _biblio_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this._genData.AutoGenerate(sender, e, this.BiblioRecPath);
        }

        void _biblio_GetEntityDefault(object sender, GetDefaultItemEventArgs e)
        {
            // 获得册登记缺省值。快速册登记
            e.Xml = this.MainForm.AppInfo.GetString(
"entityform_optiondlg",
"quickRegister_default",
"<root />");
#if NO
            string strQuickDefault = this.MainForm.AppInfo.GetString(
    "entityform_optiondlg",
    "quickRegister_default",
    "<root />");
#endif
        }

        void _biblio_LoadEntities(object sender, EventArgs e)
        {
            string strError = "";
            // 将一条书目记录下属的若干册记录装入列表
            // return:
            //      -2  用户中断
            //      -1  出错
            //      >=0 装入的册记录条数
            int nRet = LoadBiblioSubItems(out strError);
            if (nRet == -1)
            {
                // this.ShowMessage(strError, "red");
                MessageBox.Show(this, strError);
            }

            _biblio.ScrollPlusIntoView();
        }

        void _biblio_DeleteEntity(object sender, DeleteItemEventArgs e)
        {
            DeleteEntity(e.Control);
        }

        void _biblio_GetValueTable(object sender, GetValueTableEventArgs e)
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

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.comboBox_from);
                controls.Add(this.textBox_queryWord);
                controls.Add(this.splitContainer_biblioAndItems);
                //controls.Add(this.textBox_settings_importantFields);
                // 此处的缺省值会被忽略
                controls.Add(new ControlWrapper(this.checkBox_settings_needBookType, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needLocation, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needAccessNo, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needPrice, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needItemBarcode, false));
                controls.Add(new ControlWrapper(this.checkBox_settings_needBatchNo, false));
                controls.Add(new ControlWrapper(this.checkBox_settings_keyboardWizard, false));
                controls.Add(new ControlWrapper(this.comboBox_settings_colorStyle, 0));
                controls.Add(this.dpTable_browseLines);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.comboBox_from);
                controls.Add(this.textBox_queryWord);
                controls.Add(this.splitContainer_biblioAndItems);
                //controls.Add(new ControlWrapper(this.textBox_settings_importantFields, "010,200,210,215,686,69*,7**".Replace(",", "\r\n")));
                controls.Add(new ControlWrapper(this.checkBox_settings_needBookType, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needLocation, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needAccessNo, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needPrice, true));
                controls.Add(new ControlWrapper(this.checkBox_settings_needItemBarcode, false));
                controls.Add(new ControlWrapper(this.checkBox_settings_needBatchNo, false));
                controls.Add(new ControlWrapper(this.checkBox_settings_keyboardWizard, false));
                controls.Add(new ControlWrapper(this.comboBox_settings_colorStyle, 0));
                controls.Add(this.dpTable_browseLines);
                GuiState.SetUiState(controls, value);
            }
        }


        private void EntityRegisterWizard_Load(object sender, EventArgs e)
        {
            _biblio.MainForm = this.MainForm;
            _base.MainForm = this.MainForm;

            if (this._imageManager != null)
                this._imageManager.TempFileDir = this.MainForm.UserTempDir;

            this._originTitle = this.Text;

            SetTitle();
            SetButtonState();

            LoadServerXml();

#if NO
            {
                _floatingMessage = new FloatingMessageForm(this, true);
                // _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);

                // _floatingMessage.Text = "test";
                //_floatingMessage.Clicked += _floatingMessage_Clicked;
            }
#endif

#if NO
            this.MainForm.Move += new EventHandler(MainForm_Move);
#endif
            this.MainForm.Activated += MainForm_Activated;
            this.MainForm.Deactivate += MainForm_Deactivate;

            // this.MainForm.MessageFilter += MainForm_MessageFilter;

            {
                this.UiState = this.MainForm.AppInfo.GetString("entityRegisterWizard", "uistate", "");
                // 缺省值 检索途径
                if (string.IsNullOrEmpty(this.comboBox_from.Text) == true
                    && this.comboBox_from.Items.Count > 0)
                    this.comboBox_from.Text = this.comboBox_from.Items[0] as string;
#if NO
                // 缺省值 书目重要字段
                if (string.IsNullOrEmpty(this.textBox_settings_importantFields.Text) == true)
                    this.textBox_settings_importantFields.Text = "010,200,210,215,686,69*,7**".Replace(",", "\r\n");
#endif
            }

            SetControlsColor(this._colorStyle);

            this._genData = new GenerateData(this, this);
            this._genData.ScriptFileName = "dp2circulation_marc_autogen_2.cs";
            this._genData.DetailHostType = typeof(BiblioItemsHost);

            // 刚打开窗口，设定输入焦点
            this.BeginInvoke(new Action(SetStartFocus));
        }

        void MainForm_Deactivate(object sender, EventArgs e)
        {
            Debug.WriteLine("MainForm_Deactivate");
            if (this._keyboardForm != null)
                this._keyboardForm.SetPanelState("transparent");
        }

        void MainForm_Activated(object sender, EventArgs e)
        {
            Debug.WriteLine("MainForm_Activated");
            if (this._keyboardForm != null)
                this._keyboardForm.SetPanelState("display");
        }

        void SetStartFocus()
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_searchBiblio)
            {
                this.textBox_queryWord.SelectAll();
                this.textBox_queryWord.Focus();
            }
        }

        string _colorStyle = "dark";
        public string ColorStyle
        {
            get
            {
                return _colorStyle;
            }
            set
            {
                if (_colorStyle != value)
                {
                    _colorStyle = value;
                    SetControlsColor(value);
                }
            }
        }

        void SetControlsColor(string strStyle)
        {
            if (strStyle == "dark")
            {
                this.BackColor = Color.DimGray;
                this.ForeColor = Color.White;

                this.toolStrip1.BackColor = Color.FromArgb(70, 70, 70); // 50
            }
            else if (strStyle == "light")
            {
                this.BackColor = SystemColors.Window;
                this.ForeColor = SystemColors.WindowText;

                this.toolStrip1.BackColor = this.BackColor;
            }

            foreach (TabPage page in this.tabControl_main.TabPages)
            {
                page.BackColor = this.BackColor;
                page.ForeColor = this.ForeColor;
            }

            this.toolStrip1.ForeColor = this.ForeColor;
            foreach (ToolStripItem item in this.toolStrip1.Items)
            {
                item.BackColor = this.toolStrip1.BackColor;
                item.ForeColor = this.ForeColor;
            }

            {
                this.button_settings_entityDefault.BackColor = this.BackColor;
                this.button_settings_entityDefault.ForeColor = this.ForeColor;

                this.button_settings_bilbioDefault.BackColor = this.BackColor;
                this.button_settings_bilbioDefault.ForeColor = this.ForeColor;

                //this.textBox_settings_importantFields.BackColor = this.BackColor;
                //this.textBox_settings_importantFields.ForeColor = this.ForeColor;

                this.comboBox_settings_colorStyle.BackColor = this.BackColor;
                this.comboBox_settings_colorStyle.ForeColor = this.ForeColor;

                this.textBox_queryWord.BackColor = this.BackColor;
                this.textBox_queryWord.ForeColor = this.ForeColor;

                this.button_settings_reCreateServersXml.BackColor = this.BackColor;
                this.button_settings_reCreateServersXml.ForeColor = this.ForeColor;


                this.comboBox_from.BackColor = this.BackColor;
                this.comboBox_from.ForeColor = this.ForeColor;

                this.button_search.BackColor = this.BackColor;
                this.button_search.ForeColor = this.ForeColor;

                this.flowLayoutPanel1.BackColor = this.BackColor;
                this.flowLayoutPanel1.ForeColor = this.ForeColor;

            }

            {
                this.dpTable_browseLines.BackColor = this.BackColor;
                this.dpTable_browseLines.ForeColor = this.ForeColor;

                this.dpTable_browseLines.ColumnsBackColor = this.BackColor;
                this.dpTable_browseLines.ColumnsForeColor = this.ForeColor;

                if (strStyle == "dark")
                {
                    this.dpTable_browseLines.DocumentShadowColor = Color.Black;
                }
                else if (strStyle == "light")
                {
                    this.dpTable_browseLines.DocumentShadowColor = Color.DarkGray;
                }
            }

            {
                this.easyMarcControl1.BackColor = this.BackColor;
                this.easyMarcControl1.ForeColor = this.ForeColor;
                this.easyMarcControl1.SetColorStyle(strStyle);
            }

            this.splitContainer_biblioAndItems.BackColor = this.BackColor;

            _biblio.SetEntityColorStyle(strStyle);

            if (this._keyboardForm != null)
                this._keyboardForm.SetColorStyle(strStyle);
        }


#if NO
        void _floatingMessage_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "";
        }
#endif

#if NO
        void MainForm_MessageFilter(object sender, MessageFilterEventArgs e)
        {
            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "";
        }
#endif

#if NO
        void MainForm_Move(object sender, EventArgs e)
        {
            this._floatingMessage.OnResizeOrMove();
        }
#endif

        private void EntityRegisterWizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 提示保存修改
            string strText = _biblio.GetChangedWarningText();
            if (string.IsNullOrEmpty(strText) == false)
            {
                DialogResult result = MessageBox.Show(this.Owner,
                    strText + "。\r\n\r\n是否保存这些修改?\r\n\r\n是：保存修改; \r\n否: 不保存修改(修改将丢失); \r\n取消: 取消'关闭窗口'操作)",
"册登记",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // return:
                    //      -1      保存过程出错
                    //      0       没有必要保存(例如没有发生过修改)
                    //      1       保存成功
                    int nRet = SaveBiblioAndItems();
                    if (nRet == -1)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

        }

        private void EntityRegisterWizard_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_searchParam != null)
            {
                try
                {
                    _searchParam._searchComplete = true;
                }
                catch
                {

                }
            }

            if (this._genData != null)
            {
                this._genData.Close();
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
                    "entityRegisterWizard", 
                    "uistate",
                    this.UiState);

                // this.MainForm.MessageFilter -= MainForm_MessageFilter;
#if NO
                this.MainForm.Move -= new EventHandler(MainForm_Move);
#endif
                this.MainForm.Activated -= MainForm_Activated;
                this.MainForm.Deactivate -= MainForm_Deactivate;

            }

            CloseKeyboardForm();

#if NO
            if (_floatingMessage != null)
                _floatingMessage.Close();
#endif
        }

        #region IBiblioItemsWindow 接口要求

        public string GetMarc()
        {
            if (this._biblio != null)
                return this._biblio.GetMarc();
            return null;
        }

        public void SetMarc(string strMARC)
        {
            if (this._biblio != null)
                this._biblio.SetMarc(strMARC);
        }

        public string MarcSyntax
        {
            get
            {
                if (this._biblio != null)
                    return this._biblio.MarcSyntax;
                return null;
            }
        }

        public string BiblioRecPath
        {
            get
            {
                return this._biblio.BiblioRecPath;
            }
        }

        public Form Form
        {
            get
            {
                return this;
            }
        }

        #endregion

        void LoadServerXml()
        {
            string strError = "";
            // _base.ServersDom = null;    // test

            // TODO: 目录名和当前用户相关
            // 当前登录的主要服务器不同，则需要的 xml 配置文件是不同的。应当存储在各自的目录中
            string strFileName = Path.Combine(this.MainForm.ServerCfgDir, ReportForm.GetValidPathString(this.MainForm.GetCurrentUserName()) + "\\servers.xml");

            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strFileName));

            if (File.Exists(strFileName) == false
                || MainForm.GetServersCfgFileVersion(strFileName) < MainForm.SERVERSXML_VERSION)
            {
                // 创建 servers.xml 配置文件
                int nRet = this.MainForm.BuildServersCfgFile(strFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.ShowMessage("配置文件 " + strFileName + " 创建成功", "green", true);
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "文件 '" + strFileName + "' 装入 XMLDOM 时出错: " + ex.Message;
                goto ERROR1;
            }

            // TODO: 是否在文件不存在的情况下，给出缺省的几个 server ?

            _base.ServersDom = dom;
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        string _originTitle = "";

        void SetTitle()
        {
            this.Text = this._originTitle + " - " + this.tabControl_main.SelectedTab.Text;
        }

        void SetButtonState()
        {
            if (this.tabControl_main.SelectedIndex == 0)
                this.toolStripButton_prev.Enabled = false;
            else
                this.toolStripButton_prev.Enabled = true;

            if (this.tabControl_main.SelectedIndex >= this.tabControl_main.TabPages.Count - 1)
                this.toolStripButton_next.Enabled = false;
            else
            {
                this.toolStripButton_next.Enabled = true;
            }

            if (this.tabControl_main.SelectedIndex == 0)
                this.toolStripButton_start.Enabled = false;
            else
                this.toolStripButton_start.Enabled = true;

            if (this.tabControl_main.SelectedTab == this.tabPage_biblioAndItems)
            {
                this.toolStripButton_save.Enabled = true;
                this.toolStripButton_delete.Enabled = true;
                this.toolStripLabel_biblioSource.Visible = true;
            }
            else
            {
                this.toolStripButton_save.Enabled = false;
                this.toolStripButton_delete.Enabled = false;
                this.toolStripLabel_biblioSource.Visible = false;
            }
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTitle();
            SetButtonState();

            if (this.tabControl_main.SelectedTab == tabPage_settings)
            {
                if (this._keyboardForm == null
    || this._inWizardControl > 0)
                { 
                }
                else
                    SetKeyboardFormStep(KeyboardForm.Step.None, "dont_hilight");
            }
        }

        private void tabControl_main_DrawItem(object sender, DrawItemEventArgs e)
        {
            // DrawTabControlTabs(tabControl_main, e, null);

            // ChangeTabColor(sender, e);
        }

        private NativeTabControl NativeTabControl1;

        // http://stackoverflow.com/questions/2567172/c-sharp-tabcontrol-border-controls
        private class NativeTabControl : NativeWindow
        {

            protected override void WndProc(ref Message m)
            {
                if ((m.Msg == TCM_ADJUSTRECT))
                {
                    RECT rc = (RECT)m.GetLParam(typeof(RECT));
                    //Adjust these values to suit, dependant upon Appearance
                    rc.Left -= 3;
                    rc.Right += 3;
                    rc.Top -= 3;
                    rc.Bottom += 3;
                    Marshal.StructureToPtr(rc, m.LParam, true);
                }

                base.WndProc(ref m);
            }

            private const Int32 TCM_FIRST = 0x1300;
            private const Int32 TCM_ADJUSTRECT = (TCM_FIRST + 40);
            private struct RECT
            {
                public Int32 Left;
                public Int32 Top;
                public Int32 Right;
                public Int32 Bottom;
            }

        }

        // 检索
        private void button_search_Click(object sender, EventArgs e)
        {
            DoSearch(this.textBox_queryWord.Text,
                this.comboBox_from.Text);
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

        // parameters:
        //      bAutoFocus  是否要自动设置控件输入焦点?
        public void DoSearch(string strQueryWord, 
            string strFrom,
            bool bAutoSetFocus = true)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strQueryWord) == true)
            {
                strError = "尚未输入检索词";
                goto ERROR1;
            }

            if (_base.ServersDom == null)
            {
                strError = "_base.ServersDom 为空";
                goto ERROR1;
            }

            string strTotalError = "";

            this.ClearList();
            this.ClearMessage();

            if (bAutoSetFocus == false)
                _inWizardControl++;

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            // this.Progress.Initial("进行一轮任务处理...");
            this.Progress.BeginLoop();
            try
            {
                int nHitCount = 0;

                //line.SetBiblioSearchState("searching");
                this.ShowMessage("正在检索 " + strQueryWord + " ...", "progress", false);

                XmlNodeList servers = _base.ServersDom.DocumentElement.SelectNodes("server");
                foreach (XmlElement server in servers)
                {
                    Application.DoEvents();
                    if (this.Progress != null && this.Progress.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }
                    AccountInfo account = EntityRegisterBase.GetAccountInfo(server);
                    if (account == null)
                    {
                        strError = "GetAccountInfo() error. server='"+server.OuterXml+"'";
                        goto ERROR1;
                    }
                    Debug.Assert(account != null, "");
                    _base.CurrentAccount = account;

                    if (account.ServerType == "dp2library")
                    {
                        nRet = SearchLineDp2library(
                            strQueryWord,
                            strFrom,
                            account,
                            bAutoSetFocus,
                            out strError);
                        if (nRet == -1)
                            strTotalError += strError + "\r\n";
                        else
                            nHitCount += nRet;
                    }
                    else if (account.ServerType == "amazon")
                    {
                        nRet = SearchLineAmazon(
                            strQueryWord,
                            strFrom,
                            account,
                            bAutoSetFocus,
                            out strError);
                        if (nRet == -1)
                            strTotalError += strError + "\r\n";
                        else
                            nHitCount += nRet;
                    }
                }

                // 检索共享网络
                if (this.MainForm != null && this.MainForm.MessageHub != null
                    && this.MainForm.MessageHub.ShareBiblio == true)
                {
                    nRet = SearchLineMessage(
                        strQueryWord,
                        strFrom,
                        bAutoSetFocus,
                        out strError);
                    if (nRet == -1)
                        strTotalError += strError + "\r\n";
                    else
                        nHitCount += nRet;
                }

                if (string.IsNullOrEmpty(strTotalError) == false)
                    this.ShowMessage(strError, "red", true);
                else if (nHitCount == 0)
                    this.ShowMessage("没有命中", "yellow", true);
#if NO
                // line.SetBiblioSearchState(nHitCount.ToString());

                // 
                if (nHitCount == 1)
                {
                    // TODO: 如果有报错的行，是否就不要自动模拟双击了？ 假如这种情况是因为红泥巴服务器持续无法访问引起的，需要有配置办法可以临时禁用这个数据源

                    // 模拟双击
                    int index = line._biblioRegister.GetFirstRecordIndex();
                    if (index == -1)
                    {
                        strError = "获得第一个浏览记录 index 时出错";
                        goto ERROR1;
                    }
                    nRet = line._biblioRegister.SelectBiblio(index,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                else
                {
                    if (nHitCount > 1)
                        line.SetDisplayMode("select");
                    else
                    {
                        // 进入详细状态，可以新创建一条记录
                        line.AddBiblioBrowseLine(BiblioRegisterControl.TYPE_INFO,
                            "没有命中书目记录。双击本行新创建书目记录",
                            "",
                            BuildBlankBiblioInfo(line.BiblioBarcode));
                        line.SetDisplayMode("select");
                    }
                }
#endif
            }
            finally
            {
                if (this.Progress != null)
                {
                    this.Progress.EndLoop();
                    this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                    // this.Progress.Initial("");
                }

                if (bAutoSetFocus == false)
                    _inWizardControl--;
            }

#if NO
            if (string.IsNullOrEmpty(strTotalError) == false)
            {
                // DisplayFloatErrorText(strTotalError);

                line._biblioRegister.BarColor = "R";   // 红色，需引起注意
                this.SetColorList();
            }
            else
            {
                line._biblioRegister.BarColor = "Y";   // 黄色表示等待选择?
                this.SetColorList();
            }
#endif
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
            MessageBox.Show(this, strError);
        }

        #region 针对亚马逊服务器的检索

        // return:
        //      -1  出错
        //      >=0 命中的记录数
        int SearchLineAmazon(
            string strQueryWord,
            string strFrom,
            AccountInfo account,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";

            if (strFrom == "书名" || strFrom == "题名")
                strFrom = "title";
            else if (strFrom == "作者" || strFrom == "著者" || strFrom == "责任者")
                strFrom = "author";
            else if (strFrom == "出版社" || strFrom == "出版者")
                strFrom = "publisher";
            else if (strFrom == "出版日期")
                strFrom = "pubdate";
            else if (strFrom == "主题词")
                strFrom = "subject";
            else if (strFrom == "关键词")
                strFrom = "keywords";
            else if (strFrom == "语言")
                strFrom = "language";
            else if (strFrom == "装订")
                strFrom = "binding";

/* 还可以使用:
            "ISBN",
            "EISBN",
            "ASIN"
*/

            this.ShowMessage("正在针对 " + account.ServerName + " \r\n检索 " + strQueryWord + " ...",
                "progress", false);

            AmazonSearch search = new AmazonSearch();
            // search.MainForm = this.MainForm;
            search.TempFileDir = this.MainForm.UserTempDir;

            search.Timeout = 5 * 1000;
            search.Idle += search_Idle;
            try
            {

                // 多行检索中的一行检索
                int nRedoCount = 0;
            REDO:
                int nRet = search.Search(
                    account.ServerUrl,
                    strQueryWord.Replace("-", ""),
                    strFrom,    // "ISBN",
                    "[default]",
                    true,
                    out strError);
                if (nRet == -1)
                {
                    if (search.Exception != null && search.Exception is WebException)
                    {
                        WebException e = search.Exception as WebException;
                        if (e.Status == WebExceptionStatus.ProtocolError)
                        {
                            // 重做
                            if (nRedoCount < 2)
                            {
                                nRedoCount++;
                                Thread.Sleep(1000);
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
                            Thread.Sleep(1000);
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

                nRet = search.LoadBrowseLines(appendBrowseLine,
                    null,   // line,
                    bAutoSetFocus,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                return nRet;
            }
            finally
            {
                search.Idle -= search_Idle;
                this.ClearMessage();
            }

        ERROR1:
            strError = "针对服务器 '" + account.ServerName + "' 检索出错: " + strError;
            AddBiblioBrowseLine(strError, TYPE_ERROR, bAutoSetFocus);
            return -1;
        }

        void search_Idle(object sender, EventArgs e)
        {
            Application.DoEvents(); // 等待过程中出让界面控制权
        }

        // 针对亚马逊服务器检索，装入一个浏览行的回调函数
        int appendBrowseLine(string strRecPath,
            string strRecord,
            object param,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";

            // RegisterLine line = param as RegisterLine;

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

            string strMARC = "";
            // 将亚马逊 XML 格式转换为 UNIMARC 格式
            nRet = AmazonSearch.AmazonXmlToUNIMARC(dom.DocumentElement,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            string strXml = "";
            nRet = MarcUtil.Marc2Xml(strMARC,
    "unimarc",
    out strXml,
    out strError);
            if (nRet == -1)
                return -1;

            RegisterBiblioInfo info = new RegisterBiblioInfo();
            info.OldXml = strXml;   //  strMARC;
            info.Timestamp = null;
            info.RecPath = strASIN + "@" + _base.CurrentAccount.ServerName;
            info.MarcSyntax = "unimarc";
            AddBiblioBrowseLine(
                TYPE_AMAZON,  // -1,
                info.RecPath,
                StringUtil.MakePathList(cols, "\t"),
                info,
                bAutoSetFocus);

            return 0;
        }


        #endregion

        #region 针对 dp2MServer 共享网络的检索

        // return:
        //      -1  出错
        //      >=0 命中的记录数
        int SearchLineMessage(
            string strQueryWord,
            string strFrom,
            // AccountInfo account,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFromStyle = "";

            if (string.IsNullOrEmpty(strFrom) == true)
                strFrom = "ISBN";

            if (strFrom == "书名" || strFrom == "题名")
                strFromStyle = "title";
            else if (strFrom == "作者" || strFrom == "著者" || strFrom == "责任者")
                strFromStyle = "contributor";
            else if (strFrom == "出版社" || strFrom == "出版者")
                strFromStyle = "publisher";
            else if (strFrom == "出版日期")
                strFromStyle = "publishtime";
            else if (strFrom == "主题词")
                strFromStyle = "subject";

            if (string.IsNullOrEmpty(strFromStyle) == true)
            {
                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle(strFrom);
                }
                catch (Exception ex)
                {
                    strError = "EntityRegisterWizard GetBiblioFromStyle() exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + strFrom + "' 对应的 style 字符串";
                    goto ERROR1;
                }
            }

            string strMatchStyle = "left";  // BiblioSearchForm.GetCurrentMatchStyle(this.comboBox_matchStyle.Text);
            if (string.IsNullOrEmpty(strQueryWord) == true)
            {
                if (strMatchStyle == "null")
                {
                    strQueryWord = "";

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
                if (strMatchStyle == "null")
                {
                    strError = "检索空值的时候，请保持检索词为空";
                    goto ERROR1;
                }
            }

            this.ShowMessage("正在针对 共享网络\r\n检索 " + strQueryWord + " ...",
                "progress", false);

                string strSearchID = Guid.NewGuid().ToString();
                _searchParam = new SearchParam();
                _searchParam._searchID = strSearchID;
                _searchParam._autoSetFocus = bAutoSetFocus;
                _searchParam._searchComplete = false;
                _searchParam._searchCount = 0;
                this.MainForm.MessageHub.SearchResponseEvent += MessageHub_SearchResponseEvent;

            try
            {
                string strOutputSearchID = "";
                nRet = this.MainForm.MessageHub.BeginSearchBiblio(
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
                    goto ERROR1;
                if (nRet == 0)
                    return 0;

                // 装入浏览记录
                {
                    TimeSpan timeout = new TimeSpan(0,1,0);
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
                            goto ERROR1;
                        }
                    }
                }

                return _searchParam._searchCount;
            }
            finally
            {
                this.ClearMessage();
                {
                    this.MainForm.MessageHub.SearchResponseEvent -= MessageHub_SearchResponseEvent;
                    _searchParam._searchID = "";
                }
            }

        ERROR1:
            strError = "针对 共享网络 检索出错: " + strError;
            AddBiblioBrowseLine(strError, TYPE_ERROR, bAutoSetFocus);
            return -1;
        }

        class SearchParam
        {
            public string _searchID = "";
            public bool _autoSetFocus = false;
            public bool _searchComplete = false;
            public int _searchCount = 0;
        }

        SearchParam _searchParam = null;

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
            foreach(BiblioRecord record in e.Records)
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

                int image_index = -1;
                image_index = TYPE_CLOUD;

                RegisterBiblioInfo info = new RegisterBiblioInfo();
                info.OldXml = strXml;   // strMARC;
                info.Timestamp = ByteArray.GetTimeStampByteArray(record.Timestamp);
                info.RecPath = record.RecPath + "@" + (string.IsNullOrEmpty(record.LibraryName) == false? record.LibraryName : record.LibraryUID);
                info.MarcSyntax = strMarcSyntax;
                AddBiblioBrowseLine(
                    image_index,    // -1,
                    info.RecPath,
                    strBrowseText,
                    info,
                    _searchParam._autoSetFocus);
                _searchParam._searchCount++;
            }

            return;
        ERROR1:
            // 加入一个文本行
            AddBiblioBrowseLine(
                TYPE_ERROR,
                "", // item.RecPath,
                strError,
                null,
                false);
        }


        #endregion


        #region 针对 dp2library 服务器的检索

        // LibraryChannel _channel = null;

        // 正在使用中的 LibraryChannel。当正在进行检索的时候，双击浏览列表装入详细记录，可能会新开 Channel，同时使用的 Channel 多于一个
        List<LibraryChannel> _channels = new List<LibraryChannel>();

        new void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();

            if (this._channels != null)
            {
                lock (this._channels)
                {
                    foreach (LibraryChannel channel in this._channels)
                    {
                        if (channel != null)
                            channel.Abort();
                    }
                }
            }
        }

        // 针对 dp2library 服务器进行检索
        // parameters:
        //  
        // return:
        //      -1  出错
        //      >=0 命中的记录数
        int SearchLineDp2library(
            string strQueryWord,
            string strFrom,
            AccountInfo account,
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFromStyle = "";

            if (string.IsNullOrEmpty(strFrom) == true)
                strFrom = "ISBN";

            if (strFrom == "书名" || strFrom == "题名")
                strFromStyle = "title";
            else if (strFrom == "作者" || strFrom == "著者" || strFrom == "责任者")
                strFromStyle = "contributor";
            else if (strFrom == "出版社" || strFrom == "出版者")
                strFromStyle = "publisher";
            else if (strFrom == "出版日期")
                strFromStyle = "publishtime";
            else if (strFrom == "主题词")
                strFromStyle = "subject";

            if (string.IsNullOrEmpty(strFromStyle) == true)
            {
                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle(strFrom);
                }
                catch (Exception ex)
                {
                    strError = "EntityRegisterWizard GetBiblioFromStyle() {D6B49099-CB3F-4B76-84EC-7548981B2E47} exception: " + ex.Message;
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + strFrom + "' 对应的 style 字符串";
                    goto ERROR1;
                }
            }

            LibraryChannel current_channel = _base.GetChannel(account.ServerUrl, account.UserName);
            current_channel.Timeout = new TimeSpan(0, 0, 5);   // 超时值为 5 秒
            current_channel.Idle += _channel_Idle;
            lock (this._channels)
            {
                this._channels.Add(current_channel);
            }

            try
            {
                string strMatchStyle = "left";  // BiblioSearchForm.GetCurrentMatchStyle(this.comboBox_matchStyle.Text);
                if (string.IsNullOrEmpty(strQueryWord) == true)
                {
                    if (strMatchStyle == "null")
                    {
                        strQueryWord = "";

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
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        goto ERROR1;
                    }
                }

                ServerInfo server_info = null;

                //if (line != null)
                //    line.BiblioSummary = "正在获取服务器 " + account.ServerName + " 的配置信息 ...";
                this.ShowMessage("正在获取服务器 " + account.ServerName + " 的配置信息 ...", 
                    "progress", false);

                // 准备服务器信息
                nRet = _base.GetServerInfo(
                    current_channel,
                    account,
                    out server_info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;    // 可以不报错 ?

                this.ShowMessage("正在针对 " + account.ServerName + " \r\n检索 " + strQueryWord + " ...",
                    "progress", false);

                string strQueryXml = "";
                long lRet = current_channel.SearchBiblio(Progress,
                    server_info == null ? "<全部>" : server_info.GetBiblioDbNames(),    // "<全部>",
                    strQueryWord,   // this.textBox_queryWord.Text,
                    1000,
                    strFromStyle,
                    strMatchStyle,
                    this.Lang,
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 '" + account.ServerName + "' 检索出错: " + strError;
                    goto ERROR1;
                }
                if (lRet == 0)
                {
                    strError = "没有命中";
                    return 0;
                }

                // 装入浏览格式
                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                string strStyle = "id";

                List<string> biblio_recpaths = new List<string>();
                // 装入浏览格式
                for (; ; )
                {
                    if (this.Progress != null && this.Progress.State != 0)
                    {
                        break;
                    }

                    lRet = current_channel.GetSearchResult(
                        this.Progress,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                        goto ERROR1;
                    }

                    if (lRet == 0)
                        break;

                    // 处理浏览结果

                    foreach (DigitalPlatform.CirculationClient.localhost.Record searchresult in searchresults)
                    {
                        biblio_recpaths.Add(searchresult.Path);
                    }

                    {
                        // 获得书目记录
                        BiblioLoader loader = new BiblioLoader();
                        loader.Channel = current_channel;
                        loader.Stop = this.Progress;
                        loader.Format = "xml";
                        loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp;
                        loader.RecPaths = biblio_recpaths;

                        try
                        {
                            int i = 0;
                            foreach (BiblioItem item in loader)
                            {
                                if (string.IsNullOrEmpty(item.ErrorInfo) == false)
                                {
                                    AddBiblioBrowseLine(
                                        TYPE_ERROR,
                                        item.RecPath,
                                        item.ErrorInfo,
                                        null,
                                        false);
                                    i++;
                                    continue;
                                }

                                string strXml = item.Content;

                                string strMarcSyntax = "";
                                string strBrowseText = "";
                                string strColumnTitles = "";
#if NO
                                string strMARC = "";
                                // 将XML格式转换为MARC格式
                                // 自动从数据记录中获得MARC语法
                                nRet = MarcUtil.Xml2Marc(strXml,    // info.OldXml,
                                    true,
                                    null,
                                    out strMarcSyntax,
                                    out strMARC,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "XML转换到MARC记录时出错: " + strError;
                                    goto ERROR1;
                                }

                                nRet = BuildMarcBrowseText(
                                    strMarcSyntax,
                                    strMARC,
                                    out strBrowseText,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "MARC记录转换到浏览格式时出错: " + strError;
                                    goto ERROR1;
                                }
#endif
                                nRet = BuildBrowseText(strXml,
            out strBrowseText,
            out strMarcSyntax,
            out strColumnTitles,
            out strError);
                                if (nRet == -1)
                                    goto ERROR1;

                                int image_index = -1;
                                if (account.IsLocalServer == false)
                                    image_index = TYPE_CLOUD;

                                RegisterBiblioInfo info = new RegisterBiblioInfo();
                                info.OldXml = strXml;   // strMARC;
                                info.Timestamp = item.Timestamp;
                                info.RecPath = item.RecPath + "@" + account.ServerName;
                                info.MarcSyntax = strMarcSyntax;
                                AddBiblioBrowseLine(
                                    image_index,    // -1,
                                    info.RecPath,
                                    strBrowseText,
                                    info,
                                    bAutoSetFocus);
                                i++;
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "EntityRegisterWizard {414094F8-91E3-4C94-9AD6-71804FD93B67} exception: " + ExceptionUtil.GetAutoText(ex);
                            goto ERROR1;
                        }

                        // lIndex += biblio_recpaths.Count;
                        biblio_recpaths.Clear();
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                return (int)lHitCount;
            }
            finally
            {
                lock(this._channels)
                {
                    this._channels.Remove(current_channel);
                }

                current_channel.Idle -= _channel_Idle;
                _base.ReturnChannel(current_channel);
                current_channel = null;
                this.ClearMessage();
            }

        ERROR1:
            AddBiblioBrowseLine(strError, TYPE_ERROR, bAutoSetFocus);
            return -1;
        }

        void _channel_Idle(object sender, IdleEventArgs e)
        {
            e.bDoEvents = true;
        }

        #endregion

        #region 浏览行相关

        public const int TYPE_ERROR = 2;
        public const int TYPE_INFO = 3;
        public const int TYPE_CLOUD = 4;
        public const int TYPE_AMAZON = 5;

        public void AddBiblioBrowseLine(string strText,
    int nType,
            bool bAutoSetFocus)
        {
            // this._biblioRegister.AddBiblioBrowseLine(strText, nType);
            this.AddBiblioBrowseLine(
    nType,
    strText,
    "",
    null,
    bAutoSetFocus);
        }

        // 加入一个浏览行
        public void AddBiblioBrowseLine(
            int nType,
            string strBiblioRecPath,
            string strBrowseText,
            RegisterBiblioInfo info,
            bool bAutoSetFocus)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<int, string, string, RegisterBiblioInfo, bool>(AddBiblioBrowseLine),
                    nType,
                    strBiblioRecPath,
                    strBrowseText,
                    info,
                    bAutoSetFocus);
                return;
            }

            List<string> columns = StringUtil.SplitList(strBrowseText, '\t');
            DpRow row = new DpRow();

            // 序号
            DpCell cell = new DpCell();
            cell.Alignment = DpTextAlignment.Far;
            cell.Font = new Font(this.Font.FontFamily, this.Font.Size * 2);
            cell.Text = (this.dpTable_browseLines.Rows.Count + 1).ToString();
            {
                cell.ImageIndex = nType;
                if (nType == TYPE_ERROR)
                    cell.BackColor = Color.Red;
                else if (nType == TYPE_INFO)
                    cell.BackColor = Color.Yellow;
            }
            row.Add(cell);

            // 记录路径
            cell = new DpCell();
            cell.Text = strBiblioRecPath;
            row.Add(cell);

            // 封面
            cell = new DpCell();
            cell.Text = "";
            row.Add(cell);

            foreach (string s in columns)
            {
                cell = new DpCell();
                cell.Text = s;
                row.Add(cell);
            }

            row.Tag = info;
            this.dpTable_browseLines.Rows.Add(row);

            // 当插入第一行的时候，顺便选中它
            if (this.dpTable_browseLines.Rows.Count == 1)
            {
                if (bAutoSetFocus)
                    this.dpTable_browseLines.Focus();
                row.Selected = true;
                this.dpTable_browseLines.FocusedItem = row;
            }

            PrepareCoverImage(row);
        }

        // 替换浏览列内容
        public void ChangeBiblioBrowseLine(
            DpRow row,
            string strBrowseText,
            RegisterBiblioInfo info)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<DpRow, string, RegisterBiblioInfo>(ChangeBiblioBrowseLine),
                    row,
                    strBrowseText,
                    info);
                return;
            }

            List<string> columns = StringUtil.SplitList(strBrowseText, '\t');


            // 0: index
            // 1: recpath

            int index = COLUMN_IMAGE + 1;
            foreach (string s in columns)
            {
                if (index >= row.Count)
                    break;
                row[index].Text = s;
                index++;
            }

            row.Tag = info;
            // PrepareCoverImage(row);
        }

        // 创建浏览栏目标题
        void CreateBrowseColumns()
        {
            if (this.dpTable_browseLines.Columns.Count > 2)
                return;

            List<string> columns = new List<string>() { "封面", "书名", "作者", "出版者", "出版日期" };
            foreach (string s in columns)
            {
                DpColumn column = new DpColumn();
                column.Text = s;
                column.Width = 120;
                this.dpTable_browseLines.Columns.Add(column);
            }
        }

        const int COLUMN_NO = 0;
        const int COLUMN_RECPATH = 1;
        const int COLUMN_IMAGE = 2;

        // 获得检索命中列表中的事项数。其中可能包含错误信息行
        public int ResultListCount
        {
            get
            {
                return this.dpTable_browseLines.Rows.Count;
            }
        }

        // 获得检索命中结果数。不包含错误信息行
        public int SearchResultCount
        {
            get
            {
                int count = 0;
                foreach(DpRow row in this.dpTable_browseLines.Rows)
                {
                    DpCell cell = row[0];
                    if (cell.Tag != null)
                    {
                        int nType = (int)(cell.Tag);
                        if (nType == TYPE_ERROR || nType == TYPE_INFO)
                            continue;
                    }
                    count++;
                }
                return count;
            }
        }

        // 获得检索结果列表中的错误信息数
        public int SearchResultErrorCount
        {
            get
            {
                int count = 0;
                foreach (DpRow row in this.dpTable_browseLines.Rows)
                {
                    DpCell cell = row[0];
                    if (cell.Tag != null)
                    {
                        int nType = (int)(cell.Tag);
                        if (nType == TYPE_ERROR)
                            count++;
                    }
                }
                return count;
            }
        }

        public void ClearList()
        {
            if (this._imageManager != null)
                this._imageManager.ClearList();

            if (this.dpTable_browseLines == null)
                return;

            foreach (DpRow row in this.dpTable_browseLines.Rows)
            {
                RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                if (info != null)
                {
                    if (string.IsNullOrEmpty(info.CoverImageFileName) == false)
                    {
                        try
                        {
                            File.Delete(info.CoverImageFileName);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            this.dpTable_browseLines.Rows.Clear();
        }

        // 更新浏览行中缓冲的书目信息
        // parameters:
        //      new_info    要更新的 RegisterBiblioInfo 对象。如果为 null，表示要删除此浏览行
        void RefreshBrowseLineBiblio(string strRecPath,
            RegisterBiblioInfo new_info)
        {
#if DEBUG
            if (new_info != null)
            {
                Debug.Assert(strRecPath == new_info.RecPath, "");
            }
#endif

            foreach (DpRow row in this.dpTable_browseLines.Rows)
            {
                RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                if (info == null)
                    continue;
                if (info.RecPath == strRecPath)
                {
                    if (new_info == null)
                    {
                        this.dpTable_browseLines.Rows.Remove(row);
                        // TODO: 需要修改后面的浏览行的序号
                        return;
                    }
#if NO
                    info.OldXml = "";
                    info.NewXml = "";
                    info.Timestamp = null;
                    // 仅保留 RecPath 供重新装载用
#endif
                    RegisterBiblioInfo dup = new RegisterBiblioInfo(new_info);
                    //dup.CoverImageFileName = info.CoverImageFileName;
                    //dup.CoverImageRquested = info.CoverImageRquested;
                    // row.Tag = dup;

                    // XML 转换为 MARC，为了构造浏览格式的需要
                    string strError = "";
                    string strBrowseText = "";
                    string strMarcSyntax = "";
                    string strColumnTitles = "";

                    int nRet = BuildBrowseText(dup.OldXml,
out strBrowseText,
out strMarcSyntax,
out strColumnTitles,
out strError);
                    if (nRet == -1)
                        strBrowseText = strError;

                    dup.MarcSyntax = strMarcSyntax;


#if NO
                    // 刷新浏览列
                    string strBrowseText = "";
                    int nRet = BuildMarcBrowseText(
                        dup.MarcSyntax,
                        dup.OldXml,
                        out strBrowseText,
                        out strError);
                    if (nRet == -1)
                    {
                        strBrowseText = "MARC记录转换到浏览格式时出错: " + strError;
                    }
#endif
                    // 替换浏览列内容
                    ChangeBiblioBrowseLine(
                        row,
                        strBrowseText,
                        dup);
                }
            }

        }

        // public event AsyncGetImageEventHandler AsyncGetImage = null;
        // bool CoverImageRequested = false; // 如果为 true ,表示已经请求了异步获取图像，不要重复请求

        // 准备特定浏览行的封面图像
        // parameters:
        //      bRetry  是否为重试？如果为 true，表示即便 info.CoverImageRquested == true 也要重做
        void PrepareCoverImage(DpRow row, 
            bool bRetry = false)
        {
            Debug.Assert(row != null, "");

            RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
            if (info == null)
                return;

            if (string.IsNullOrEmpty(info.CoverImageFileName) == false)
                return;

            if (string.IsNullOrEmpty(info.OldXml) == true)
                return;

            string strUrl = "";
            {
                string strError = "";
                string strMARC = "";
                string strMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                int nRet = MarcUtil.Xml2Marc(info.OldXml,
                    true,
                    null,
                    out strMarcSyntax,
                    out strMARC,
                    out strError);
                if (nRet == -1)
                {
                    Debug.Assert(false, "");
                    return;
                }

                if (string.IsNullOrEmpty(strMARC) == true)
                    return;

                strUrl = ScriptUtil.GetCoverImageUrl(strMARC);
                if (string.IsNullOrEmpty(strUrl) == true)
                    return;
            }

#if NO
            if (StringUtil.HasHead(strUrl, "http:") == true)
                return;
#endif

            if (info != null
                && bRetry == false
                && info.CoverImageRquested == true)
                return;

            // 通过 dp2library 协议获得图像文件
            // if (this.AsyncGetImage != null)
            {
                AsyncGetImageEventArgs e = new AsyncGetImageEventArgs();
                e.RecPath = row[1].Text;
                e.ObjectPath = strUrl;
                e.FileName = "";
                e.Row = row;
                this.AsyncGetImageFile(this, e);
                // 修改状态，表示已经发出请求
                if (row != null && info != null)
                {
                    info.CoverImageRquested = true;
                }
            }
        }

#if NO
        public override MainForm MainForm
        {
            get
            {
                return base.MainForm;
            }
            set
            {
                base.MainForm = value;
                if (value != null && this._imageManager != null)
                {
                    this._imageManager.TempFileDir = value.UserTempDir;
                }
            }
        }
#endif

        internal ImageManager _imageManager = null;

        internal void AsyncGetImageFile(object sender,
    AsyncGetImageEventArgs e)
        {
            string strError = "";
            if (String.IsNullOrEmpty(e.RecPath) == true)
            {
                strError = "e.RecPath 为空，无法获得图像文件";
                goto ERROR1;
            }

            if (StringUtil.HasHead(e.ObjectPath, "http:") == true)
            {
                EventFilter filter = new EventFilter();
                // filter.BiblioRegister = biblioRegister;
                filter.Row = e.Row;
                filter.BiblioRecPath = e.RecPath;

                this._imageManager.AsyncGetObjectFile(
                    "",
                    "",
                    e.ObjectPath,
                    e.FileName,
                    filter);
            }
            else
            {
                string strPath = "";
                string strServerName = "";
                StringUtil.ParseTwoPart(e.RecPath, "@", out strPath, out strServerName);

                AccountInfo account = _base.GetAccountInfo(strServerName, false);
                if (account == null)
                {
                    strError = "e.RecPath 中 '" + e.RecPath + "' 服务器名 '" + strServerName + "' 没有配置";
                    goto ERROR1;
                }
                string strServerUrl = account.ServerUrl;
                string strUserName = account.UserName;

                if (EntityRegisterBase.IsDot(strServerUrl) == true)
                    strServerUrl = this.MainForm.LibraryServerUrl;
                if (EntityRegisterBase.IsDot(strUserName) == true)
                    strUserName = this.MainForm.DefaultUserName;

                EventFilter filter = new EventFilter();
                // filter.BiblioRegister = biblioRegister;
                filter.Row = e.Row;
                filter.BiblioRecPath = e.RecPath;


                // string strObjectPath = strPath + "/object/" + e.ObjectPath;
                string strObjectPath = ScriptUtil.MakeObjectUrl(strPath, e.ObjectPath);
                this._imageManager.AsyncGetObjectFile(strServerUrl,
                    strUserName,
                    strObjectPath,
                    e.FileName,
                    filter);
            }

            return;
        ERROR1:
            this.AsyncGetImageComplete(e.Row,
                "",
                null,
                strError);
        }

        // 设置图像文件
        public void AsyncGetImageComplete(DpRow row,
            string strBiblioRecPath,
            string strFileName,
            string strErrorInfo)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                this.Invoke(new Action<DpRow, string, string, string>(AsyncGetImageComplete),
                    row, strBiblioRecPath, strFileName, strErrorInfo);
                return;
            }

            // row[COLUMN_IMAGE].Text = strFileName;

            if (string.IsNullOrEmpty(strErrorInfo) == false)
                row[COLUMN_IMAGE].Text = "!" + strErrorInfo;
            else
                SetImage(row[COLUMN_IMAGE], strFileName);

#if NO
            // 已经选定了书目记录的情况
            if (row == null)
            {
                if (string.IsNullOrEmpty(strErrorInfo) == false)
                {
                    // 如何报错?
                    // row 为空
                    // this.pictureBox1.Image = null;
                    return;
                }

                // 给相关浏览行设定
                row = FindRowByRecPath(this.BiblioRecPath + "@" + this.ServerName);
                if (row != null)
                {
                    RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                    if (info != null)
                        info.CoverImageFileName = strFileName;
                }

                if (strBiblioRecPath == this.BiblioRecPath
                    && this.DisplayMode == "detail")
                {
                    this.ImageFileName = strFileName;
                    this.pictureBox1.LoadAsync(strFileName);
                }
                return;
            }

            {
                RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
                if (info != null)
                    info.CoverImageFileName = strFileName;

                if (this.DisplayMode == "select"
                    && this.dpTable_browseLines.SelectedRows.Count == 1
                    && this.dpTable_browseLines.SelectedRows[0] == row)
                    this.pictureBox1.LoadAsync(strFileName);
            }
#endif
        }

        // 事件筛选
        class EventFilter
        {
            // public BiblioRegisterControl BiblioRegister = null;
            public DpRow Row = null;    // 浏览行对象
            public string BiblioRecPath = "";   // 书目记录路径
        }

        #endregion

        int SetBiblio(RegisterBiblioInfo info, 
            bool bAutoSetFocus,
            out string strError)
        {
            strError = "";

            string strStartText = info.RecPath;

            string strStartServerName = "";
            string strStartRecPath = "";
            StringUtil.ParseTwoPart(info.RecPath, "@", out strStartRecPath, out strStartServerName);

            // 给书目记录一个本地路径，用于后面可能发生的保存操作
            string strServerName = "";
            string strBiblioRecPath = "";
            bool bAllowCopy = true;

            AccountInfo _currentAccount = _base.GetAccountInfo(strStartServerName, false);
            if (_currentAccount == null)
            {
                //strError = "服务器名 '" + strStartServerName + "' 没有配置";
                //return -1;
            }
            else
            {
                if (_currentAccount.IsLocalServer == false)
                    bAllowCopy = true;
                else
                    bAllowCopy = false;
            }

            // 根据书目记录的路径，匹配适当的目标
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = GetTargetInfo(
                bAllowCopy,
                strStartServerName,
                strStartRecPath,
                out strServerName,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                // strError = "来自服务器 '" + this._biblio.ServerName + "' 的书目记录 '" + this._biblio.BiblioRecPath + "' 没有找到匹配的保存目标";
                strServerName = strStartServerName;
                strBiblioRecPath = strStartRecPath;
            }

            info.RecPath = strBiblioRecPath + "@" + strServerName;

            this.toolStripLabel_biblioSource.Text = strStartText + " --> " + info.RecPath;

            OnHiddenFieldsChanged(info.MarcSyntax);

            nRet = this._biblio.SetBiblio(info, bAutoSetFocus, out strError);
            if (nRet == -1)
                return -1;
            {
                if (this._genData != null
    && this.MainForm.PanelFixedVisible == true
    && this._biblio != null)
                    this._genData.AutoGenerate(this.easyMarcControl1,
                        new GenerateDataEventArgs(),
                        this._biblio.BiblioRecPath,
                        true);
            }

            return 0;
        }

        private void dpTable_browseLines_DoubleClick(object sender, EventArgs e)
        {
            EditSelectedBrowseLine();
        }

        // 选定一个浏览行
        // parameters:
        //      bClearBefore    是否在选择前清除以前的选择标记
        //      index   从 0 开始计数的，行下标
        public bool SelectBrowseLine(bool bClearBefore,
            int index)
        {
            if (index < 0)
                throw new ArgumentException("index 值不能小于 0", "index");
            if (index >= this.dpTable_browseLines.Rows.Count)
                return false;
            if (bClearBefore)
            {
                foreach (DpRow row in this.dpTable_browseLines.Rows)
                {
                    if (row.Selected == true)
                        row.Selected = false;
                }
            }

            {
                DpRow row = this.dpTable_browseLines.Rows[index];
                row.Selected = true;
                this.dpTable_browseLines.FocusedItem = row;
            }
            return true;
        }

        // 将当前选定的浏览行装入编辑器
        // parameters:
        //      bAutoSetFocus   是否自动设定键盘输入焦点？如果为 true，表示这是用户主动操作，而不是向导引导的流程。此种情况下需要在操作中主动影响向导窗口的当前页面
        public void EditSelectedBrowseLine(bool bAutoSetFocus = true)
        {
            string strError = "";
            int nRet = 0;

            this.ClearMessage();

            if (this.dpTable_browseLines.Rows.Count == 0)
            {
                strError = "请先进行检索，并选定命中结果中要编辑的一行";
                goto ERROR1;
            }
            if (this.dpTable_browseLines.SelectedRows.Count == 0)
            {
                strError = "请选择要编辑的一行";
                goto ERROR1;
            }

            RegisterBiblioInfo info = this.dpTable_browseLines.SelectedRows[0].Tag as RegisterBiblioInfo;
            if (info == null)
            {
                strError = "这是提示信息行";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(info.OldXml) == true && string.IsNullOrEmpty(info.NewXml) == true)
            {
                strError = "此记录已经被更新，请重新检索";
                goto ERROR1;
            }

            // 此语句 .SelectedTab = ... 要自动 Focus 到新 page 的第一个子控件上
            // http://stackoverflow.com/questions/4044711/select-tab-page-in-tabcontrol-without-stealing-focus
            if (bAutoSetFocus == false)
                this.tabControl_main.Enabled = false;

            this.tabControl_main.SelectedTab = this.tabPage_biblioAndItems;

            if (bAutoSetFocus == false)
                this.tabControl_main.Enabled = true;

            // return:
            //      false   后续操作可以进行
            //      true    出现错误，需要人工干预，或者操作者选择了取消，后续操作不能进行
            if (WarningSaveChange() == true)
                return;

            // 清除窗口内容
            _biblio.Clear();

            nRet = SetBiblio(info, bAutoSetFocus, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (bAutoSetFocus)
            {
                this.easyMarcControl1.SelectFirstItem();
#if NO
                // TODO: 是否需要一个特殊的 step 参数，定位到 MarcEdit 但不向后找空字段
                KeyboardForm.Step result_step = SetKeyboardFormStep(dp2Circulation.KeyboardForm.Step.EditBiblio);
                if (result_step == KeyboardForm.Step.EditEntity)
                {
                    // 要将输入焦点切换到册记录区域。如果当前一个册也没有，则要切换 focus 到加号上面
                    this.GetEntityPlusButton().Focus();
                }
#endif
            }
            return;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
            {
                // MessageBox.Show(this, strError);
                this.ShowMessage(strError, "red", true);
            }
        }

        KeyboardForm.Step SetKeyboardFormStep(
            KeyboardForm.Step step,
            string strStyle = "")
        {
            if (this._keyboardForm != null)
            {
                this._keyboardForm.SetStep(step, strStyle);
                return this._keyboardForm.GetStep();
            }

            return dp2Circulation.KeyboardForm.Step.None;
        }

        // return:
        //      false   后续操作可以进行
        //      true    出现错误，需要人工干预，或者操作者选择了取消，后续操作不能进行
        bool WarningSaveChange()
        {
            // 提示保存修改
            string strText = _biblio.GetChangedWarningText();
            if (string.IsNullOrEmpty(strText) == false)
            {
                DialogResult result = MessageBox.Show(this.Owner,
                    strText + "。\r\n\r\n是否保存这些修改?\r\n\r\n是：保存修改; \r\n否: 不保存修改(修改将丢失); \r\n取消: 取消'重新开始'操作)",
"册登记",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return true;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // return:
                    //      -1      保存过程出错
                    //      0       没有必要保存(例如没有发生过修改)
                    //      1       保存成功
                    int nRet = SaveBiblioAndItems();
                    if (nRet == -1)
                        return true;
                }
            }

            return false;
        }

        private void easyMarcControl1_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            // if (GetConfigDom != null)
            {
                if (string.IsNullOrEmpty(this._biblio.BiblioRecPath) == true
                    && string.IsNullOrEmpty(this._biblio.ServerName) == true)
                {
                    // e.Path = e.Path;
                }
                else
                {
                    // e.Path = Global.GetDbName(this.BiblioRecPath) + "/cfgs/" + e.Path + "@" + this.ServerName;
                    e.Path = e.Path + ":" + this._biblio.BiblioRecPath + "@" + this._biblio.ServerName;
                }

                // GetConfigDom(this, e);
                MarcEditor_GetConfigDom(sender, e);
            }
        }

        // 从 marcdef 中获得 MARC 格式定义
        static string GetMarcSyntax(XmlDocument marcdef_dom)
        {
            XmlNode node = marcdef_dom.DocumentElement.SelectSingleNode("//MARCSyntax");
            if (node == null)
                return null;

            return node.InnerText.Trim().ToLower();
        }

        public void MarcEditor_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            // Debug.Assert(false, "");

            // 路径中应该包含 @服务器名

            if (String.IsNullOrEmpty(e.Path) == true)
            {
                e.ErrorInfo = "e.Path 为空，无法获得配置文件";
                goto ERROR1;
            }

            // e.Path 是这样的形态 marcdef:中文图书/1@当前服务器 。冒号后面部分可以缺省，表示未知服务器的记录(没有路径和服务器名)

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(e.Path, "@", out strPath, out strServerName);

            //
            string strPureFileName = "";
            string strBiblioRecPath = "";
            StringUtil.ParseTwoPart(strPath, ":", out strPureFileName, out strBiblioRecPath);

            string strServerType = _base.GetServerType(strServerName);
            if (strServerType == "amazon" || strServerName == "!unknown")
            {
                // TODO: 如何知道 MARC 记录是什么具体的 MARC 格式?
                // 可能需要在服务器信息中增加一个首选的 MARC 格式属性
                string strFileName = Path.Combine(this.MainForm.DataDir, "unimarc_cfgs/" + strPath);

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
                    goto ERROR1;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strFileName, dom);  // 保存到缓存
                return;
            }

            AccountInfo account = _base.GetAccountInfo(strServerName, false);   // false 可以不影响可能正在进行的检索长操作，因为长操作可能要用到 this._base._currentAccount
            if (account == null)
            {
                e.ErrorInfo = "e.Path 中 '" + e.Path + "' 服务器名 '" + strServerName + "' 没有配置";
                goto ERROR1;
            }

            Debug.Assert(strServerType == "dp2library", "");


            // string strBiblioDbName = Global.GetDbName(this._biblio.BiblioRecPath);
            string strBiblioDbName = Global.GetDbName(strBiblioRecPath);

            // 得到干净的文件名
            string strCfgFilePath = strBiblioDbName + "/cfgs/" + strPureFileName;    // e.Path;
            int nRet = strCfgFilePath.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFilePath = strCfgFilePath.Substring(0, nRet);
            }

            // 在cache中寻找
            e.XmlDocument = this.MainForm.DomCache.FindObject(strCfgFilePath);
            if (e.XmlDocument == null)
            {

                // TODO: 可以通过服务器名，得到 url username 等配置参数

                // 下载配置文件
                string strContent = "";
                string strError = "";

                byte[] baCfgOutputTimestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetCfgFileContent(
                    account.ServerUrl,
                    account.UserName,
                    strCfgFilePath,
                    out strContent,
                    out baCfgOutputTimestamp,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    e.ErrorInfo = "获得配置文件 '" + strCfgFilePath + "' 时出错：" + strError;
                    goto ERROR1;
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
                        goto ERROR1;
                    }
                    e.XmlDocument = dom;
                    this.MainForm.DomCache.SetObject(strCfgFilePath, dom);  // 保存到缓存
                }
            }

            // 只使用属于数据库的 marcdef 文件中的 MarcSyntax 信息，然后使用安装目录中的 marcdef 配置文件
            {
                string strFileName = "";

                string strMarcSyntax = GetMarcSyntax(e.XmlDocument);
                if (strMarcSyntax == "usmarc")
                    strFileName = Path.Combine(this.MainForm.DataDir, "usmarc_cfgs/" + strPureFileName);
                else
                    strFileName = Path.Combine(this.MainForm.DataDir, "unimarc_cfgs/" + strPureFileName);

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
                    goto ERROR1;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strFileName, dom);  // 保存到缓存
            }
            return;
        ERROR1:
            this.ShowMessage(e.ErrorInfo, "red", true);
        }

        int m_nInGetCfgFile = 0;

        // 获得配置文件
        // parameters:
        //      
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetCfgFileContent(
            string strServerUrl,
            string strUserName,
            string strCfgFilePath,
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

            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在下载配置文件 ...");
            Progress.BeginLoop();

            m_nInGetCfgFile++;

            LibraryChannel channel = _base.GetChannel(strServerUrl, strUserName);

            try
            {
                Progress.SetMessage("正在下载配置文件 " + strCfgFilePath + " ...");
                string strMetaData = "";
                string strOutputPath = "";

                string strStyle = "content,data,metadata,timestamp,outputpath";

                // TODO: 应该按照 URL 区分
                long lRet = channel.GetRes(Progress,
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
                    if (channel.ErrorCode == ErrorCode.NotFound)
                        return 0;

                    goto ERROR1;
                }
            }
            finally
            {
                _base.ReturnChannel(channel);

                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");

                m_nInGetCfgFile--;
            }

            return 1;
        ERROR1:
            return -1;
        }

        #region 册记录相关

        // 将一条书目记录下属的若干册记录装入列表
        // return:
        //      -2  用户中断
        //      -1  出错
        //      >=0 装入的册记录条数
        int LoadBiblioSubItems(
            out string strError)
        {
            strError = "";
            int nRet = 0;

            _biblio.ClearEntityEditControls("normal");

            string strBiblioRecPath = this._biblio.BiblioRecPath;
            if (string.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                // 册信息部分显示为空
                // this._biblio.TrySetBlank("none");
                this._biblio.AddPlus();
                return 0;
            }

            AccountInfo _currentAccount = _base.GetAccountInfo(this._biblio.ServerName);
            if (_currentAccount == null)
            {
                strError = "服务器名 '" + this._biblio.ServerName + "' 没有配置";
                return -1;
            }

            // 如果不是本地服务器，则不需要装入册记录
            if (_currentAccount.IsLocalServer == false)
            {
                _base.CurrentAccount = null;
                // 册信息部分显示为空
                // this._biblio.TrySetBlank("none");
                this._biblio.AddPlus();
                return 0;
            }

            LibraryChannel current_channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);
            if (current_channel == null)
            {
                strError = "GetChannel() error";
                return -1;
            }
            lock(this._channels)
            {
                this._channels.Add(current_channel);
            }

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            //this.Progress.Initial("正在装入书目记录 '" + strBiblioRecPath + "' 下属的册记录 ...");
            this.Progress.BeginLoop();
            try
            {
                int nCount = 0;

                long lPerCount = 100; // 每批获得多少个
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                for (; ; )
                {
                    if (Progress.State != 0)
                    {
                        strError = "用户中断";
                        return -2;
                    }

                    EntityInfo[] entities = null;

                    long lRet = current_channel.GetEntities(
             Progress,
             strBiblioRecPath,
             lStart,
             lCount,
             "",  // bDisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
             "zh",
             out entities,
             out strError);
                    if (lRet == -1)
                        return -1;

                    lResultCount = lRet;

                    if (lRet == 0)
                    {
                        // 册信息部分显示为空
                        // this._biblio.TrySetBlank("none");
                        // return nCount;
                        goto END1;
                    }

                    Debug.Assert(entities != null, "");

                    foreach (EntityInfo entity in entities)
                    {
                        // string strXml = entity.OldRecord;
                        if (entity.ErrorCode != ErrorCodeValue.NoError)
                        {
                            // TODO: 显示错误信息
                            continue;
                        }

                        EntityEditControl edit_control = null;
                        // 添加一个新的册对象
                        nRet = this._biblio.NewEntity(entity.OldRecPath,
                            entity.OldTimestamp,
                            entity.OldRecord,
                            false,  // 不必滚入视野
                            false,  // 不改变 Focus
                            out edit_control,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        nCount++;
                    }

                    lStart += entities.Length;
                    if (lStart >= lResultCount)
                        break;

                    if (lCount == -1)
                        lCount = lPerCount;

                    if (lStart + lCount > lResultCount)
                        lCount = lResultCount - lStart;
                }

                END1:
                this._biblio.AddPlus();
                return nCount;
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");

                lock(this._channels)
                {
                    this._channels.Remove(current_channel);
                }

                _base.ReturnChannel(current_channel);
                current_channel = null;
                _currentAccount = null;
            }
        }

        void DeleteEntity(EntityEditControl edit)
        {
            string strError = "";
            int nRet = 0;

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            this.Progress.BeginLoop();
            try
            {
                List<EntityEditControl> controls = new List<EntityEditControl>();
                controls.Add(edit);

                // line.SetDisplayMode("summary");

                // 删除下属的册记录
                {
                    EntityInfo[] entities = null;
                    // 构造用于保存的实体信息数组
                    nRet = this._biblio.BuildSaveEntities(
                        "delete",
                        controls,
                        out entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 分批进行保存
                    // return:
                    //      -2  部分成功，部分失败
                    //      -1  出错
                    //      0   保存成功，没有错误和警告
                    nRet = SaveEntities(
                        entities,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        goto ERROR1;

                    // 兑现视觉删除
                    this._biblio.RemoveEditControl(edit);

                    // line._biblioRegister.EntitiesChanged = false;
                }

                return;
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");
            }
        ERROR1:
            BiblioRegisterControl.SetEditErrorInfo(edit, strError);
            //line._biblioRegister.BarColor = "R";   // 红色
            //this.SetColorList();
        this.ShowMessage(strError, "red", true);
        }


        // 分批进行保存
        // return:
        //      -2  部分成功，部分失败
        //      -1  出错
        //      0   保存成功，没有错误和警告
        int SaveEntities(
            EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            //int nRet = 0;

            bool bWarning = false;
            EntityInfo[] errorinfos = null;
            string strWarning = "";

            // 确定目标服务器 目标书目库
            AccountInfo _currentAccount = _base.GetAccountInfo(this._biblio.ServerName);
            if (_currentAccount == null)
            {
                strError = "' 服务器名 '" + this._biblio.ServerName + "' 没有配置";
                return -1;
            }

            LibraryChannel current_channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);
            if (current_channel == null)
            {
                strError = "GetChannel() error";
                return -1;
            }
            lock (this._channels)
            {
                this._channels.Add(current_channel);
            }
            try
            {

                string strBiblioRecPath = this._biblio.BiblioRecPath;

                int nBatch = 100;
                for (int i = 0; i < (entities.Length / nBatch) + ((entities.Length % nBatch) != 0 ? 1 : 0); i++)
                {
                    int nCurrentCount = Math.Min(nBatch, entities.Length - i * nBatch);
                    EntityInfo[] current = GetPart(entities, i * nBatch, nCurrentCount);

                    long lRet = current_channel.SetEntities(
         Progress,
         strBiblioRecPath,
         entities,
         out errorinfos,
         out strError);
                    if (lRet == -1)
                        return -1;

                    // 把出错的事项和需要更新状态的事项兑现到显示、内存
                    string strError1 = "";
                    if (this._biblio.RefreshOperResult(errorinfos, out strError1) == true)
                    {
                        bWarning = true;
                        strWarning += " " + strError1;
                    }

                    if (lRet == -1)
                        return -1;
                }

                if (string.IsNullOrEmpty(strWarning) == false)
                    strError += " " + strWarning;

                if (bWarning == true)
                    return -2;

                // line._biblioRegister.EntitiesChanged = false;    // 所有册都保存成功了
                return 0;
            }
            finally
            {
                lock (this._channels)
                {
                    this._channels.Remove(current_channel);
                }

                _base.ReturnChannel(current_channel);
                current_channel = null;
                _currentAccount = null;
            }
        }

        static EntityInfo[] GetPart(EntityInfo[] source,
int nStart,
int nCount)
        {
            EntityInfo[] result = new EntityInfo[nCount];
            for (int i = 0; i < nCount; i++)
            {
                result[i] = source[i + nStart];
            }
            return result;
        }

        public EntityEditControl AddNewEntity(string strBarcode,
            bool bAutoSetFocus = true)
        {
            string strError = "";
            EntityEditControl control = null;
            int nRet = this._biblio.AddNewEntity(strBarcode, 
                bAutoSetFocus,
                out control,
                out strError);
            if (nRet == -1)
                this.ShowMessage(strError, "red", true);
            return control;
        }

        public PlusButton GetEntityPlusButton()
        {
            if (this._biblio == null)
                return null;
            return this._biblio.GetPlusButton();
        }

        public void EnsurePlusButtonVisible()
        {
            PlusButton button = GetEntityPlusButton();
            if (button != null)
                this.flowLayoutPanel1.ScrollControlIntoView(button);
        }

        #endregion

        #region 保存书目记录

        // 保存书目记录和下属的册记录
        // return:
        //      -1      保存过程出错
        //      0       没有必要保存(例如没有发生过修改)
        //      1       保存成功
        int SaveBiblioAndItems()
        {
            string strError = "";
            int nRet = 0;

            if (_biblio.BiblioChanged)
            {
                List<BiblioError> errors = null;
                // return:
                //      -1  检查过程出错。错误信息在 strError 中。和返回 1 的区别是，这里是某些因素导致无法检查了，而不是因为册记录格式有错
                //      0   正确
                //      1   有错。错误信息在 errors 中
                nRet = _biblio.VerifyBiblio(
                    "",
                    out errors,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    // 观察报错的字段是否有隐藏状态的。如果有，则需要把它们显示出来，以便操作者观察修改
                    {
#if NO
                        List<string> important_fieldnames = null; //  StringUtil.SplitList(this.textBox_settings_importantFields.Text.Replace("\r\n", ","));
                        if (_biblio.MarcSyntax == "unimarc")
                            important_fieldnames = StringUtil.SplitList(this.UnimarcBiblioImportantFields);
                        else
                            important_fieldnames = StringUtil.SplitList(this.Marc21BiblioImportantFields);

                        List<string> hidden_fieldnames = BiblioError.GetOutOfRangeFieldNames(errors, important_fieldnames);
                        if (hidden_fieldnames.Count > 0)
                            this.easyMarcControl1.HideFields(hidden_fieldnames, false); // null, false 可显示全部隐藏字段
#endif
                        List<string> names = BiblioError.GetFieldNames(errors);
                        if (names.Count > 0)
                            this.easyMarcControl1.HideFields(names, true); // null, false 可显示全部隐藏字段
                    }

                    bool bTemp = false;
                    // TODO: 如果保持窗口修改后的尺寸位置?
                    MessageDialog.Show(this,
                        "书目记录格式不正确",
                        BiblioError.GetListString(errors, "\r\n"),
                        null,
                        ref bTemp);

                    strError = "书目记录没有被保存。请修改相关字段后重新提交保存";
                    goto ERROR1;
                }

            }

            {
                // 补全册记录信息
                nRet = _biblio.CompleteEntities(
    out strError);
                if (nRet == -1)
                    goto ERROR1;

                List<string> verify_styles = new List<string>();

                if (this.checkBox_settings_needBookType.Checked == true)
                    verify_styles.Add("need_booktype");
                if (this.checkBox_settings_needLocation.Checked == true)
                    verify_styles.Add("need_location");
                if (this.checkBox_settings_needAccessNo.Checked == true)
                    verify_styles.Add("need_accessno");
                if (this.checkBox_settings_needPrice.Checked == true)
                    verify_styles.Add("need_price");
                if (this.checkBox_settings_needItemBarcode.Checked == true)
                    verify_styles.Add("need_barcode");
                if (this.checkBox_settings_needBatchNo.Checked == true)
                    verify_styles.Add("need_batchno");

                List<string> errors = null;

                // 检查册记录的格式是否正确
                // parameters:
                //      errors  返回册记录出错信息。每个元素返回一个错误信息，顺次对应于每个有错的册记录。文字中有说明，是那个册记录出错
                // return:
                //      -1  检查过程出错。错误信息在 strError 中。和返回 1 的区别是，这里是某些因素导致无法检查了，而不是因为册记录格式有错
                //      0   正确
                //      1   有错。错误信息在 errors 中
                nRet = _biblio.VerifyEntities(
                    StringUtil.MakePathList(verify_styles),
                    out errors,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    bool bTemp = false;
                    MessageDialog.Show(this,
                        "册记录格式不正确",
                        StringUtil.MakePathList(errors, "\r\n"),
                        null,
                        ref bTemp);
                    strError = "记录没有被保存。请修改相关记录后重新提交保存";
                    goto ERROR1;
                }
            }

            bool bBiblioSaved = false;
            bool bEntitySaved = false;

            this.ShowMessage("正在保存书目和册记录", "progress", false);

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            this.Progress.BeginLoop();
            try
            {
                string strCancelSaveBiblio = "";
                string strWarning = "";

                AccountInfo _currentAccount = _base.GetAccountInfo(this._biblio.ServerName);
                if (_currentAccount == null)
                {
                    strError = "服务器名 '" + this._biblio.ServerName + "' 没有配置";
                    goto ERROR1;
                }

                // line.SetBiblioSearchState("searching");
                if (this._biblio.BiblioChanged == true
                    || Global.IsAppendRecPath(this._biblio.BiblioRecPath) == true
                    || _currentAccount.IsLocalServer == false)
                {
                    // TODO: 确定目标 dp2library 服务器 目标书目库
                    string strServerName = "";
                    string strBiblioRecPath = "";
                    // 根据书目记录的路径，匹配适当的目标
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetTargetInfo(
                        _currentAccount.IsLocalServer == false ? true : false,
                        this._biblio.ServerName,
                        this._biblio.BiblioRecPath,
                        out strServerName,
                        out strBiblioRecPath,
                        out strError);
#if NO
                    if (nRet != 1)
                    {
                        strError = "line (servername='" + line._biblioRegister.ServerName + "' bibliorecpath='" + line._biblioRegister.BiblioRecPath + "') 没有找到匹配的保存目标";
                        goto ERROR1;
                    }
#endif
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "来自服务器 '" + this._biblio.ServerName + "' 的书目记录 '" + this._biblio.BiblioRecPath + "' 没有找到匹配的保存目标";
                        bool bAppend = Global.IsAppendRecPath(this._biblio.BiblioRecPath);
                        if (bAppend == true || _currentAccount.IsLocalServer == false)
                            goto ERROR1;

                        // 虽然书目记录无法保存，但继续寻求保存册记录
                        strCancelSaveBiblio = strError;
                        goto SAVE_ENTITIES;
                    }

                    // if nRet == 0 并且 书目记录路径不是追加型的
                    // 虽然无法兑现修改后保存,但是否可以依然保存实体记录?

                    string strXml = "";
                    nRet = this._biblio.GetBiblioXml(
                        out strXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strOutputPath = "";
                    byte[] baNewTimestamp = null;
                    // return:
                    //      -1  出错
                    //      0   有警告，在 strWarning 中。一般是因为部分保存成功
                    //      1   成功
                    nRet = SaveXmlBiblioRecordToDatabase(
                        strServerName,
                        strBiblioRecPath,
                        strXml,
                        this._biblio.Timestamp,
                        out strOutputPath,
                        out baNewTimestamp,
                        out strWarning,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        // strWarning 中为警告信息
                        // TODO: 出现对照窗口，允许进一步修改
                    }
                    this._biblio.ServerName = strServerName;
                    this._biblio.BiblioRecPath = strOutputPath;
                    this._biblio.Timestamp = baNewTimestamp;

                    if (string.IsNullOrEmpty(strWarning) == true)
                        this._biblio.BiblioChanged = false;

                    this.toolStripLabel_biblioSource.Text = this._biblio.BiblioRecPath + "@" + this._biblio.ServerName;

                    bBiblioSaved = true;

                    // 刷新浏览列表中的书目信息
                    {
                        strXml = "";
                        if (string.IsNullOrEmpty(strWarning) == true)
                        {
                            nRet = this._biblio.GetBiblioXml(out strXml, out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        RegisterBiblioInfo info = new RegisterBiblioInfo(
                            _biblio.BiblioRecPath + "@" + _biblio.ServerName,
                            strXml,
                            "",
                            _biblio.Timestamp,
                            _biblio.MarcSyntax);
                        this.RefreshBrowseLineBiblio(info.RecPath,
                            info);
                    }

                    this.ShowMessage("书目记录 " + strOutputPath + " 保存成功", "green", true);
                }

                // line.SetDisplayMode("summary");

                SAVE_ENTITIES:
                // 保存下属的册记录
                {
                    EntityInfo[] entities = null;
                    // 构造用于保存的实体信息数组
                    nRet = this._biblio.BuildSaveEntities(
                        "change",
                        null,
                        out entities,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (entities.Length > 0)
                    {
                        this.ShowMessage("正在保存 " + entities.Length + " 个册记录", "progress", false);
                        // 分批进行保存
                        // return:
                        //      -2  部分成功，部分失败
                        //      -1  出错
                        //      0   保存成功，没有错误和警告
                        nRet = SaveEntities(
                            entities,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                            goto ERROR1;
                        this._biblio.EntitiesChanged = false;
                        bEntitySaved = true;
                    }
                    else
                    {
                        this._biblio.EntitiesChanged = false;
                        // line._biblioRegister.BarColor = "G";   // 绿色

                    }
                }


                if (string.IsNullOrEmpty(strCancelSaveBiblio) == false)
                {
                    if (bEntitySaved == false)
                        strError = "书目记录无法保存\r\n(" + strCancelSaveBiblio + ")";
                    else
                        strError = "书目记录无法保存，但册记录保存成功\r\n(" + strCancelSaveBiblio + ")";
                    goto ERROR1;
                }
                else if (string.IsNullOrEmpty(strWarning) == false)
                {
                    if (bEntitySaved == false)
                        this.ShowMessage(strWarning, "yellow", true);
                    else
                        this.ShowMessage(strWarning + "，但册记录保存成功", "yellow", true);
                }
                else if (bBiblioSaved == false && bEntitySaved == false)
                {
                    this.ShowMessage("没有可保存的信息", "yellow", true);
                    return 0;
                }
                else
                {
                    this.ShowMessage("保存成功", "green", true);
                }
                return 1;
            }
            finally
            {
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");
            }
        ERROR1:
            this.ShowMessage(strError, "red", true);
            return -1;
        }

        // 保存XML格式的书目记录到数据库
        // parameters:
        // return:
        //      -1  出错
        //      0   有警告，在 strWarning 中。一般是因为部分保存成功
        //      1   成功
        int SaveXmlBiblioRecordToDatabase(
            string strServerName,
            string strPath,
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

            AccountInfo _currentAccount = _base.GetAccountInfo(strServerName);
            if (_currentAccount == null)
            {
                strError = "服务器名 '" + strServerName + "' 没有配置";
                return -1;
            }
            LibraryChannel current_channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);
            if (current_channel == null)
            {
                strError = "GetChannel() error";
                return -1;
            }
            lock (this._channels)
            {
                this._channels.Add(current_channel);
            }

            try
            {
                string strAction = "change";

                if (Global.IsAppendRecPath(strPath) == true)
                    strAction = "new";

            //REDO:
                long lRet = current_channel.SetBiblioInfo(
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
                    goto ERROR1;
                }
                if (current_channel.ErrorCode == ErrorCode.PartialDenied)
                {
                    strWarning = "书目记录 '" + strPath + "' 保存成功，但所提交的字段部分被拒绝 (" + strError + ")。请留意刷新窗口，检查实际保存的效果";
                    return 0;
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                lock (this._channels)
                {
                    this._channels.Remove(current_channel);
                }

                _base.ReturnChannel(current_channel);
                current_channel = null;
                // _currentAccount = null;
            }
        }

        // 判断一个 server 是否适合写入
        bool IsWritable(XmlElement server,
            string strEditBiblioRecPath)
        {
            string strBiblioDbName = Global.GetDbName(strEditBiblioRecPath);
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                return false;

            XmlNodeList databases = server.SelectNodes("database[@name='" + strBiblioDbName + "']");
            foreach (XmlElement database in databases)
            {
                bool bIsTarget = DomUtil.GetBooleanParam(database, "isTarget", false);
                if (bIsTarget == true)
                {
                    string strAccess = database.GetAttribute("access");
                    bool bAppend = Global.IsAppendRecPath(strEditBiblioRecPath);
                    if (bAppend == true && StringUtil.IsInList("append", strAccess) == true)
                        return true;
                    if (bAppend == false 
                        && (StringUtil.IsInList("overwrite", strAccess) == true || StringUtil.IsInList("partial_overwrite", strAccess) == true)
                        )
                        return true;
                }
            }

            return false;
        }

        // 判断一个 server 是否适合删除记录
        bool IsDeleteable(XmlElement server,
            string strEditBiblioRecPath)
        {
            bool bAppend = Global.IsAppendRecPath(strEditBiblioRecPath);
            if (bAppend == true)
                return false;   // 追加形态的路径不适合用于参数操作

            string strBiblioDbName = Global.GetDbName(strEditBiblioRecPath);
            if (string.IsNullOrEmpty(strBiblioDbName) == true)
                return false;

            XmlNodeList databases = server.SelectNodes("database[@name='" + strBiblioDbName + "']");
            foreach (XmlElement database in databases)
            {
                bool bIsTarget = DomUtil.GetBooleanParam(database, "isTarget", false);
                if (bIsTarget == true)
                {
                    string strAccess = database.GetAttribute("access");
                    if (StringUtil.IsInList("delete", strAccess) == true)
                        return true;
                }
            }

            return false;
        }

        // 为保存书目记录的需要，根据书目记录的路径，匹配适当的目标
        // parameters:
        //      bAllowCopyTo    是否允许书目记录复制到其他库？这发生在原始库不让 overwrite 的时候
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetTargetInfo(
            bool bAllowCopyTo,
            string strEditServerName,
            string strEditBiblioRecPath,
            out string strServerName,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strServerName = "";
            strBiblioRecPath = "";

            // string strEditServerUrl = "";
            //string strEditServerName = this._biblio.ServerName;
            //string strEditBiblioRecPath = this._biblio.BiblioRecPath;

            if (string.IsNullOrEmpty(strEditServerName) == false
                && string.IsNullOrEmpty(strEditBiblioRecPath) == false)
            {
                // 验证 edit 中的书目库名，是否是可以写入的 ?
                XmlElement server = (XmlElement)_base.ServersDom.DocumentElement.SelectSingleNode("server[@name='" + strEditServerName + "']");
                if (server != null)
                {
                    if (IsWritable(server, strEditBiblioRecPath) == true)
                    {
                        strServerName = strEditServerName;
                        strBiblioRecPath = strEditBiblioRecPath;
                        return 1;
                    }

                    if (bAllowCopyTo == false)
                        return 0;
                }
            }

            if (_base.ServersDom == null || _base.ServersDom.DocumentElement == null)
            {
                strError = "_base.ServersDom == null || _base.ServersDom.DocumentElement == null";
                return -1;
            }

            // 此后都是寻找可以追加写入的
            // 获得第一个可以写入的服务器名
            XmlNodeList servers = _base.ServersDom.DocumentElement.SelectNodes("server");
            foreach (XmlElement server in servers)
            {
                XmlNodeList databases = server.SelectNodes("database");
                foreach (XmlElement database in databases)
                {
                    string strDatabaseName = database.GetAttribute("name");
                    if (string.IsNullOrEmpty(strDatabaseName) == true)
                        continue;
                    bool bIsTarget = DomUtil.GetBooleanParam(database, "isTarget", false);
                    if (bIsTarget == false)
                        continue;

                    string strAccess = database.GetAttribute("access");
                    if (StringUtil.IsInList("append", strAccess) == false)
                        continue;
                    strServerName = server.GetAttribute("name");
                    strBiblioRecPath = strDatabaseName + "/?";
                    return 1;
                }
            }

            return 0;
        }

        #endregion

        #region 删除书目记录

        // return:
        //      -1  出错
        //      0   不允许删除
        //      1   允许删除
        int BiblioCanDelete(
    out string strServerName,
    out string strBiblioRecPath)
        {
            strServerName = "";
            strBiblioRecPath = "";

            // _currentAccount.IsLocalServer == true ?

            string strEditServerName = this._biblio.ServerName;
            string strEditBiblioRecPath = this._biblio.BiblioRecPath;

            if (string.IsNullOrEmpty(strEditServerName) == false
                && string.IsNullOrEmpty(strEditBiblioRecPath) == false)
            {
                // 验证 edit 中的书目库名，是否是可以写入的 ?
                XmlElement server = (XmlElement)_base.ServersDom.DocumentElement.SelectSingleNode("server[@name='" + strEditServerName + "']");
                if (server != null)
                {
                    if (IsDeleteable(server, strEditBiblioRecPath) == true)
                    {
                        strServerName = strEditServerName;
                        strBiblioRecPath = strEditBiblioRecPath;
                        return 1;
                    }

                    return 0;
                }
                return 0;
            }

            return 0;
        }

        // 从数据库中删除书目记录
        int DeleteBiblioRecord()
        {
            string strError = "";
            int nRet = 0;

            Progress.OnStop += new StopEventHandler(this.DoStop);
            this.ShowMessage("正在删除书目记录 ...", "progress", false);
            Progress.BeginLoop();

            try
            {
                AccountInfo _currentAccount = _base.GetAccountInfo(this._biblio.ServerName);
                if (_currentAccount == null)
                {
                    strError = "服务器名 '" + this._biblio.ServerName + "' 没有配置";
                    goto ERROR1;
                }

                // TODO: 确定目标 dp2library 服务器 目标书目库
                string strServerName = "";
                string strBiblioRecPath = "";

                // return:
                //      -1  出错
                //      0   不允许删除
                //      1   允许删除
                nRet = BiblioCanDelete(
                    out strServerName,
                    out strBiblioRecPath);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "来自服务器 '" + this._biblio.ServerName + "' 的书目记录 '" + this._biblio.BiblioRecPath + "' 不允许删除";
                    goto ERROR1;
                }

                byte[] baNewTimestamp = null;
                nRet = DeleteBiblioRecordFromDatabase(
                    strServerName,
                    strBiblioRecPath,
                    this._biblio.Timestamp,
                    out baNewTimestamp,
                    out strError);
                if (nRet == -1)
                {
                    // 删除失败时也不要忘记了更新时间戳
                    // 这样如果遇到时间戳不匹配，下次重试删除即可?
                    if (baNewTimestamp != null)
                        _biblio.Timestamp = baNewTimestamp;
                    goto ERROR1;
                }

                _biblio.BiblioChanged = false;
                _biblio.EntitiesChanged = false;

                this.RefreshBrowseLineBiblio(_biblio.BiblioRecPath + "@" + _biblio.ServerName, null);

                this.ShowMessage("书目记录 " + strBiblioRecPath + "@" + strServerName + " 删除成功", "green", true);
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
            }

            return 1;
        ERROR1:
            this.ShowMessage(strError, "red", true);
            // MessageBox.Show(this, strError);
            return -1;
        }

        int DeleteBiblioRecordFromDatabase(
    string strServerName,
    string strPath,
    // string strXml,
    byte[] baTimestamp,
    out byte[] baNewTimestamp,
    out string strError)
        {
            strError = "";
            baNewTimestamp = null;

            AccountInfo _currentAccount = _base.GetAccountInfo(strServerName);
            if (_currentAccount == null)
            {
                strError = "服务器名 '" + strServerName + "' 没有配置";
                return -1;
            }
            LibraryChannel current_channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);
            if (current_channel == null)
            {
                strError = "GetChannel() error";
                return -1;
            }
            lock (this._channels)
            {
                this._channels.Add(current_channel);
            }

            try
            {
                string strAction = "delete";

                if (Global.IsAppendRecPath(strPath) == true)
                {
                    strError = "路径 '"+strPath+"' 不能用于删除操作";
                    return -1;
                }

                string strOutputPath = "";
            //REDO:
                long lRet = current_channel.SetBiblioInfo(
                    Progress,
                    strAction,
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
                    strError = "删除书目记录 '" + strPath + "' 时出错: " + strError;
                    goto ERROR1;
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                lock (this._channels)
                {
                    this._channels.Remove(current_channel);
                }

                _base.ReturnChannel(current_channel);
                current_channel = null;
                // _currentAccount = null;
            }
        }

        #endregion

        private void flowLayoutPanel1_SizeChanged(object sender, EventArgs e)
        {
            // 当窗口较小，垂直卷动看到(册记录显示区)下部内容的时候，最大化窗口，则会留下卷滚条的残余图像。
            // 这一行语句能消除这个问题
            //this.flowLayoutPanel1.Update();
            //this.flowLayoutPanel1.Invalidate();
        }

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {
            // return:
            //      -1      保存过程出错
            //      0       成功
            ReStart();
        }

        // parameters:
        //      bAutoSave   是否在操作前自动保存修改?
        // return:
        //      -1      保存过程出错
        //      0       成功
        public int ReStart(bool bAutoSave = true)
        {
#if NO
            // 提示保存修改
            string strText = _biblio.GetChangedWarningText();
            if (string.IsNullOrEmpty(strText) == false)
            {
                DialogResult result = MessageBox.Show(this.Owner,
                    strText + "。\r\n\r\n是否保存这些修改?\r\n\r\n是：保存修改; \r\n否: 不保存修改(修改将丢失); \r\n取消: 取消'重新开始'操作)",
"册登记",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // return:
                    //      -1      保存过程出错
                    //      0       没有必要保存(例如没有发生过修改)
                    //      1       保存成功
                    int nRet = SaveBiblioAndItems();
                    if (nRet == -1)
                        return;
                }
            }
#endif
            this.ClearMessage();

            if (bAutoSave)
            {
                string strText = _biblio.GetChangedWarningText();
                if (string.IsNullOrEmpty(strText) == false)
                {
                    // return:
                    //      -1      保存过程出错
                    //      0       没有必要保存(例如没有发生过修改)
                    //      1       保存成功
                    int nRet = SaveBiblioAndItems();
                    if (nRet == -1)
                        return -1;
                }
            }

            // return:
            //      false   后续操作可以进行
            //      true    出现错误，需要人工干预，或者操作者选择了取消，后续操作不能进行
            if (WarningSaveChange() == true)
                return -1;

            // 清除窗口内容
            this._inWizardControl++;    // 2015/7/6 避免因为清除 EasyMarcControl 内容而引起对 OnMarcSelectionChanged() KeyboardForm 的波动
            _biblio.Clear();
            this._inWizardControl--;

            this.textBox_queryWord.Text = "";
            this.dpTable_browseLines.Rows.Clear();

            this.tabControl_main.SelectedTab = this.tabPage_searchBiblio;

            this.textBox_queryWord.Focus();
            return 0;
        }

        // 获得一个控件的边框参数。屏幕坐标
        public Rectangle GetRect(string strName)
        {
            Control control = null;
            switch (strName)
            {
                case "QueryWord":
                    control = this.textBox_queryWord;
                    break;
                case "ResultList":
                    control = this.dpTable_browseLines;
                    break;
                case "MarcEdit":
                    control = this.easyMarcControl1;
                    break;
                case "Entities":
                    control = this.flowLayoutPanel1;
                    break;
                case "FirstEntity":
                    control = this.flowLayoutPanel1;
                    break;
            }
            if (control != null)
                // return control.RectangleToScreen(new Rectangle(new Point(0,0), control.Size));
                return control.Parent.RectangleToScreen(control.Bounds);
            return
                new Rectangle(0, 0, 0, 0);
        }

        private void toolStripButton_prev_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex > 0)
            {
                this.tabControl_main.SelectedIndex--;
            }

        }

        private void toolStripButton_next_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex < this.tabControl_main.TabPages.Count - 1)
            {
                if (this.tabControl_main.SelectedTab == this.tabPage_searchBiblio)
                {
                    dpTable_browseLines_DoubleClick(sender, e);
                    return;
                }

                this.tabControl_main.SelectedIndex++;
            }
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            this.ClearMessage();

            SaveBiblioAndItems();
        }

                // 装入一条空白书目记录
        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            NewBiblio();
        }

        // 装入一条空白书目记录
        // return:
        //      -1      出错
        //      0       成功
        public int NewBiblio(bool bAutoSetFocus = true)
        {
            string strError = "";
            int nRet = 0;

            this.ClearMessage();

            if (_base.ServersDom == null || _base.ServersDom.DocumentElement == null)
            {
                strError = "servers.xml 配置文件尚未初始化";
                goto ERROR1;
            }

            string strRecord = "";
            strRecord = this.UnimarcBiblioDefault;
            if (string.IsNullOrEmpty(strRecord) == false)
            {
                if (StringUtil.HasHead(strRecord, "hdr") == true
                    || StringUtil.HasHead(strRecord, "###") == true)
                {
                    strRecord = strRecord.Substring(3);
                    // 去掉第一个回车
                    nRet = strRecord.IndexOf("\r\n");
                    if (nRet != -1)
                        strRecord = strRecord.Remove(nRet, 2);
                }
                else
                {
                    // 加上头标区
                    strRecord = "?????nam0 22?????3i 45  " + strRecord;
                }
                strRecord = strRecord.Replace("$", new string((char)31, 1))
                    .Replace("\r\n", new string((char)30, 1));
            }

            RegisterBiblioInfo info = null;
            if (this.tabControl_main.SelectedTab == this.tabPage_searchBiblio)
                info = BuildBlankBiblioInfo(
                     strRecord,
                     this.comboBox_from.Text,
                     this.textBox_queryWord.Text);
            else
                info = BuildBlankBiblioInfo(
                    strRecord,
                    "",
                    "");
            if (bAutoSetFocus == false)
                this.tabControl_main.Enabled = false;

            // 如果当前不在书目 page，要自动切换到位
            this.tabControl_main.SelectedTab = this.tabPage_biblioAndItems;

            if (bAutoSetFocus == false)
                this.tabControl_main.Enabled = true;

            // return:
            //      false   后续操作可以进行
            //      true    出现错误，需要人工干预，或者操作者选择了取消，后续操作不能进行
            if (WarningSaveChange() == true)
                return -1;

            // 清除窗口内容
            _biblio.Clear();

            nRet = SetBiblio(info, bAutoSetFocus, out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;
        ERROR1:
            if (bAutoSetFocus)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    MessageBox.Show(this, strError);
            }
            else
            {
                this.ShowMessage(strError, "ref", true);
            }
            return -1;
        }

        public EasyLine GetNextBiblioLine(EasyLine ref_line)
        {
            return this.easyMarcControl1.GetNextEditableLine(ref_line);
        }

        public EasyLine GetPrevBiblioLine(EasyLine ref_line)
        {
            return this.easyMarcControl1.GetPrevEditableLine(ref_line);
        }

        public static EditLine GetNextEntityLine(EntityEditControl control, EditLine ref_line)
        {
            return control.GetNextEditableLine(ref_line);
        }

        public static EditLine GetFocuedEntityLine(EntityEditControl control)
        {
            return control.GetFocuedLine();
        }

        // 将一个册记录控件或下级控件滚入视野
        public void EnsureEntityVisible(Control control)
        {
            this.flowLayoutPanel1.ScrollControlIntoView(control);
        }

        // 构造一条空白书目记录
        RegisterBiblioInfo BuildBlankBiblioInfo(
            string strRecord,
            string strFrom,
            string strValue)
        {
#if NO
            // 获得一个可以保存新书目记录的服务器地址和书目库名
            string strServerName = "";
            string strBiblioDbName = "";
            // 寻找一个可以创建新书目记录的数据库信息
            // return:
            //      false 没有找到
            //      ture 找到
            GetTargetDatabaseInfo(out strServerName,
                out strBiblioDbName);
#endif

            // 装入一条空白书目记录
            RegisterBiblioInfo info = new RegisterBiblioInfo();

#if NO
            if (string.IsNullOrEmpty(strBiblioDbName) == false)
                info.RecPath = strBiblioDbName + "/?@" + strServerName; 
#endif

            string strISBN = "";
            string strTitle = "";
            string strAuthor = "";
            string strPublisher = "";

            strFrom = strFrom.ToLower();

            if (strFrom == "isbn")
                strISBN = strValue;
            if (strFrom == "书名" || strFrom == "题名")
                strTitle = strValue;
            if (strFrom == "作者" || strFrom == "著者" || strFrom == "责任者")
                strAuthor = strValue;
            if (strFrom == "出版者" || strFrom == "出版社")
                strPublisher = strValue;

            info.MarcSyntax = "unimarc";
            MarcRecord record = new MarcRecord(strRecord);
            if (string.IsNullOrEmpty(strRecord) == true)
            {
                record.add(new MarcField('$', "010  $a" + strISBN + "$dCNY??"));
                record.add(new MarcField('$', "2001 $a"+strTitle+"$f"+strAuthor));
                record.add(new MarcField('$', "210  $a$c"+strPublisher+"$d"));
                record.add(new MarcField('$', "215  $a$d??cm"));
                record.add(new MarcField('$', "690  $a"));
                record.add(new MarcField('$', "701  $a" + strAuthor));

                // record.Header.ForceUNIMARCHeader();
                record.Header[0, 24] = "?????nam0 22?????3i 45  ";
            }
            else
            {
                record.setFirstSubfield("010", "a", strISBN);
                record.setFirstSubfield("200", "a", strTitle);
                record.setFirstSubfield("200", "f", strAuthor);
                record.setFirstSubfield("210", "c", strPublisher);
                record.setFirstSubfield("701", "a", strAuthor);
#if NO
                if (record.select("field[@name='010']").count == 0)
                    record.ChildNodes.insertSequence(new MarcField('$', "010  $a" + strISBN + "$dCNY??"));
                else if (record.select("field[@name='010']/subfield[@name='a']").count == 0)
                    (record.select("field[@name='010']")[0] as MarcField).ChildNodes.insertSequence(new MarcSubfield("a", strISBN));
                else
                    record.select("field[@name='010']/subfield[@name='a']")[0].Content = strISBN;
#endif
            }

            // info.OldXml = record.Text;

            string strError = "";
            string strXml = "";
            int nRet = MarcUtil.Marc2Xml(record.Text,
                info.MarcSyntax,
                out strXml,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            info.OldXml = strXml;
            return info;
        }

        private void splitContainer_biblioAndItems_DoubleClick(object sender, EventArgs e)
        {
            if (this.splitContainer_biblioAndItems.Orientation == Orientation.Horizontal)
            {
                this.splitContainer_biblioAndItems.Orientation = Orientation.Vertical;
                this.splitContainer_biblioAndItems.SplitterDistance = this.splitContainer_biblioAndItems.Width / 2;
            }
            else
            {
                this.splitContainer_biblioAndItems.Orientation = Orientation.Horizontal;
                this.splitContainer_biblioAndItems.SplitterDistance = this.splitContainer_biblioAndItems.Height / 2;
            }
        }

#if NO
        private void textBox_settings_importantFields_TextChanged(object sender, EventArgs e)
        {
            if (this._biblio != null)
            {
                this._biblio.HideFieldNames = StringUtil.SplitList(this.textBox_settings_importantFields.Text.Replace("\r\n", ","));
                this._biblio.HideFieldNames.Insert(0, "rvs");
            }
        }
#endif

        void OnHiddenFieldsChanged(string strMarcSyntax = null)
        {
            if (this._biblio != null)
            {
                if (strMarcSyntax == null)
                    strMarcSyntax = this._biblio.MarcSyntax;

                List<string> hidden_fieldnames = null;
                if (strMarcSyntax == "unimarc")
                    hidden_fieldnames = StringUtil.SplitList(this.UnimarcBiblioHiddenFields);
                else
                    hidden_fieldnames = StringUtil.SplitList(this.Marc21BiblioHiddenFields);

                // hidden_fieldnames.Insert(0, "rvs");

                this._biblio.HideFieldNames = hidden_fieldnames;
            }
        }

        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;

            if (_inWizardControl == 0)
            {
                SetKeyboardFormStep(KeyboardForm.Step.PrepareSearchBiblio, "dont_clear,dont_hilight");
            }
        }

        private void textBox_queryWord_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            if (keyData == Keys.Enter && this.dpTable_browseLines.Focused)
            {
            }

            if (keyData == Keys.Escape)
            {

            }

            return base.ProcessDialogKey(keyData);
        }

        private void dpTable_browseLines_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (int)Keys.Enter
                && this.dpTable_browseLines.Focused)
            {
                this.dpTable_browseLines_DoubleClick(this, e);
                return;
            }
        }

        private void button_settings_entityDefault_Click(object sender, EventArgs e)
        {
            EntityFormOptionDlg dlg = new EntityFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.DisplayStyle = "quick_entity";
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        private void easyMarcControl1_Enter(object sender, EventArgs e)
        {
            if (this._genData != null
                && this.MainForm.PanelFixedVisible == true
                && this._biblio != null)
                this._genData.AutoGenerate(this.easyMarcControl1,
                    new GenerateDataEventArgs(),
                    this._biblio.BiblioRecPath,
                    true);

            if (this._keyboardForm == null
    || this._inWizardControl > 0)
                return;
            else
                this.BeginInvoke(new Action<object, EventArgs>(OnMarcSelectionChanged), sender, e);
        }

        private void easyMarcControl1_Leave(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Enter(object sender, EventArgs e)
        {
#if NO
            // 找到拥有输入焦点的那个 EntityEditControl
            foreach(Control control in this.flowLayoutPanel1.Controls)
            {
                if (control.ContainsFocus == true
                    && control is EntityEditControl)
                {
                    if (this._genData != null
                        && this.MainForm.PanelFixedVisible == true
                        && this._biblio != null)
                    {
                        GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                        e1.FocusedControl = control;
                        this._genData.AutoGenerate(_biblio, // control,
                            e1,
                            this._biblio.BiblioRecPath,
                            true);
                    }
                    return;

                }
            }
#endif
            if (_biblio != null)
            {
                EntityEditControl edit = _biblio.GetFocusedEditControl();
                if (edit != null)
                {
                    if (this._genData != null
        && this.MainForm.PanelFixedVisible == true)
                    {
                        GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                        e1.FocusedControl = edit;
                        this._genData.AutoGenerate(_biblio, // control,
                            e1,
                            this._biblio.BiblioRecPath,
                            true);
                    }
                }
            }
        }

        private void flowLayoutPanel1_Leave(object sender, EventArgs e)
        {

        }

        // 删除书目记录
        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            this.ClearMessage();

            DialogResult result = MessageBox.Show(this.Owner,
    "确实要删除当前书目记录?\r\n\r\n删除书目记录会一并删除其下属的册记录和期、订购、评注记录。删除后记录将无法复原，请小心确认操作\r\n\r\n是：删除; \r\n否: 放弃删除",
"册登记",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
            {
                this.ShowMessage("已放弃删除", "yellow", true);
                return;
            }

            this.DeleteBiblioRecord();
        }

        #region 键盘输入面板

        KeyboardForm _keyboardForm = null;

        private void checkBox_settings_keyboardWizard_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_settings_keyboardWizard.Checked == true)
            {
                OpenKeyboardForm(this.FloatingKeyboardForm);
            }
            else
            {
                CloseKeyboardForm();
            }
        }

        bool FloatingKeyboardForm
        {
            get
            {
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                    return this.MainForm.AppInfo.GetBoolean("entityRegisterWizard", "keyboardFormFloating", true);
                return true;            
            }
            set
            {
                if (this.MainForm != null && this.MainForm.AppInfo != null)
                    this.MainForm.AppInfo.SetBoolean("entityRegisterWizard", "keyboardFormFloating", value);
            }
        }
        // parameters:
        //      bFloatingWindow 是否打开为浮动的对话框？ false 表示停靠在固定面板区
        void OpenKeyboardForm(bool bFloatingWindow)
        {
            if (this._keyboardForm == null
                || (bFloatingWindow == true && this._keyboardForm.Visible == false))
            {
                CloseKeyboardForm();

                this._keyboardForm = new KeyboardForm();
                this._keyboardForm.FormClosed += _keyboardForm_FormClosed;
                this._keyboardForm.DoDockEvent += _keyboardForm_DoDockEvent;
                GuiUtil.AutoSetDefaultFont(this._keyboardForm);
                this._keyboardForm.Text = "向导";
                this._keyboardForm.BaseForm = this;

                // this._keyboardForm.Show(this.MainForm);
            }
            // this.easyMarcControl1.HideSelection = false;    // 当 EasyMarcControl 不拥有输入焦点时也能看到蓝色选定字段的标记

            if (bFloatingWindow == true)
            {
                if (_keyboardForm.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(_keyboardForm, "keyboardform_state");

                    _keyboardForm.Show(this.MainForm);
                    _keyboardForm.Activate();

                    if (this._keyboardForm != null)
                        this._keyboardForm.SetColorStyle(this.ColorStyle);

                    this.MainForm.CurrentAcceptControl = null;
                }
                else
                {
                    if (_keyboardForm.WindowState == FormWindowState.Minimized)
                        _keyboardForm.WindowState = FormWindowState.Normal;
                    _keyboardForm.Activate();
                }
            }
            else
            {
                if (_keyboardForm.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentAcceptControl != _keyboardForm.Table)
                    {
                        _keyboardForm.DoDock(true); // false 不会自动显示FixedPanel
                        _keyboardForm.Initialize(); // 没有 .Show() 的就用 .Initialize()
                    }
                }
            }

            this.checkBox_settings_keyboardWizard.Checked = true;
        }

        void _keyboardForm_DoDockEvent(object sender, DoDockEventArgs e)
        {
            if (this._keyboardForm.Docked == false)
            {
                if (this.MainForm.CurrentAcceptControl != this._keyboardForm.Table)
                    this.MainForm.CurrentAcceptControl = this._keyboardForm.Table;

                if (e.ShowFixedPanel == true)
                {
                    if (this.MainForm.PanelFixedVisible == false)
                        this.MainForm.PanelFixedVisible = true;
                    // 把 acceptpage 翻出来
                    this.MainForm.ActivateAcceptPage();
                }

                this._keyboardForm.Docked = true;
                this._keyboardForm.Visible = false;

                this.FloatingKeyboardForm = false;

                if (this._keyboardForm != null)
                    this._keyboardForm.SetColorStyle(this.ColorStyle);
            }
            else
            {
                this.OpenKeyboardForm(true);
                this.FloatingKeyboardForm = true;
            }


        }

        void _keyboardForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (this._keyboardForm != null)
                this.MainForm.AppInfo.UnlinkFormState(_keyboardForm);
#endif

            this.checkBox_settings_keyboardWizard.Checked = false;
        }

        void CloseKeyboardForm()
        {
            if (this._keyboardForm != null)
            {
                if (this.MainForm.CurrentAcceptControl == _keyboardForm.Table)
                    this.MainForm.CurrentAcceptControl = null;

                this._keyboardForm.Close();
                this._keyboardForm = null;
            }
            // this.easyMarcControl1.HideSelection = true;
        }

        #endregion

        private void EntityRegisterWizard_Move(object sender, EventArgs e)
        {
            if (this._keyboardForm != null)
                this._keyboardForm.UpdateRectTarget();
        }

        private void EntityRegisterWizard_Resize(object sender, EventArgs e)
        {
            if (this._keyboardForm != null)
                this._keyboardForm.UpdateRectTarget();
        }

        private void EntityRegisterWizard_Enter(object sender, EventArgs e)
        {
            Debug.WriteLine("EntityRegisterWizard_Enter");
#if NO
            if (this._keyboardForm != null)
                this._keyboardForm.SetPanelState("form");
#endif
        }

        private void EntityRegisterWizard_Leave(object sender, EventArgs e)
        {
            Debug.WriteLine("EntityRegisterWizard_Leave");
#if NO
            if (this._keyboardForm != null)
                this._keyboardForm.SetPanelState("transparent");
#endif
        }

        private void EntityRegisterWizard_Activated(object sender, EventArgs e)
        {
            Debug.WriteLine("EntityRegisterWizard_Activated");

        }

        private void EntityRegisterWizard_Deactivate(object sender, EventArgs e)
        {
            Debug.WriteLine("EntityRegisterWizard_Deactivate");

        }

        int _inWizardControl = 0;

        // 阻止动作连带传递到 KeyboardForm
        public void DisableWizardControl()
        {
            _inWizardControl++;
        }

        // 允许动作连带传递到 KeyboardForm
        public void EnableWizardControl()
        {
            _inWizardControl--;
        }

        private void dpTable_browseLines_Enter(object sender, EventArgs e)
        {
            if (_inWizardControl == 0)
            {
                if (this.dpTable_browseLines.Rows.Count > 0)
                    SetKeyboardFormStep(KeyboardForm.Step.SelectSearchResult);
            }
        }

        // 本函数也有可能被本类 easyMarcControl1_Enter 调用
        private void easyMarcControl1_SelectionChanged(object sender, EventArgs e)
        {
            if (this._keyboardForm == null
    || this._inWizardControl > 0)
                return;
            else
                this.BeginInvoke(new Action<object, EventArgs>(OnMarcSelectionChanged), sender, e);
        }

        void OnMarcSelectionChanged(object sender, EventArgs e)
        {
            if (this._keyboardForm == null
                || this._inWizardControl > 0)
                return;
            // TODO: 为了提高运行速度，下面功能可以用 BeginInvoke 实现
            List<EasyLine> lines = this.easyMarcControl1.SelectedItems;
            if (lines.Count == 1)
            {
                this._keyboardForm.SetCurrentBiblioLine(lines[0]);
                SetKeyboardFormStep(KeyboardForm.Step.EditBiblio, "dont_hilight");
            }
            else
            {
                this._keyboardForm.SetCurrentBiblioLine(null);
                SetKeyboardFormStep(KeyboardForm.Step.EditBiblio, "dont_hilight");
            }
        }

        private void comboBox_settings_colorStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ColorStyle = this.comboBox_settings_colorStyle.Text;
        }

        private void dpTable_browseLines_PaintRegion(object sender, PaintRegionArgs e)
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
                    catch (Exception)
                    {
                        // throw new Exception("read image error:" + ex.Message);
                        if (cell_info.RetryCount < 5)
                        {
                            cell_info.RetryCount++;
                            DpRow row = cell.Container;
                            PrepareCoverImage(row, true);
                        }
                        else
                        {

                            e.Height = 0;
                            cell.OwnerDraw = false;
                            cell.Text = "read image error";
                        }
                        return;
                    }

                    if (this.dpTable_browseLines.MaxTextHeight < e.Height)
                        this.dpTable_browseLines.MaxTextHeight = e.Height;
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
                    catch (Exception)
                    {
                        // throw new Exception("read image error:" + ex.Message);
                        if (cell_info.RetryCount < 5)
                        {
                            cell_info.RetryCount++;
                            DpRow row = cell.Container;
                            PrepareCoverImage(row, true);
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

        void SetImage(DpCell cell, string strFileName)
        {
            if (this.dpTable_browseLines.InvokeRequired)
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

        // 探测当前用户对一个书目库的访问权限
        public static int DetectAccess(LibraryChannel channel,
            Stop stop,
            string strBiblioDbName,
            string strMarcSyntax,
            out string strBiblioAccess,
            out string strEntityAccess,
            out string strError)
        {
            strError = "";
            strBiblioAccess = "";
            strEntityAccess = "";

            // 样本记录
            MarcRecord record = new MarcRecord();
            record.add(new MarcField('$', "001???????"));
            record.add(new MarcField('$', "200$a书名$f作者"));
            record.add(new MarcField('$', "701$a作者"));

            string strXml = "";
            int nRet = MarcUtil.Marc2Xml(record.Text,
                strMarcSyntax,
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> biblio_access_list = new List<string>();
            List<string> entity_access_list = new List<string>();

            // 模拟书目追加写入
            string strAction = "simulate_new";
            string strPath = strBiblioDbName + "/?";
            string strOutputPath = "";
            byte[] baNewTimestamp = null;
            long lRet = channel.SetBiblioInfo(
    stop,
    strAction,
    strPath,
    "xml",
    strXml,
    null,   // baTimestamp,
    "",
    out strOutputPath,
    out baNewTimestamp,
    out strError);
            if (lRet == -1)
            {
                // 注意检查部分允许写入的报错
                if (channel.ErrorCode != ErrorCode.AccessDenied)
                    return -1;
            }
            else
                biblio_access_list.Add("append");

            // 模拟书目覆盖写入
            strAction = "simulate_change";
            strPath = strBiblioDbName + "/0";
            lRet = channel.SetBiblioInfo(
stop,
strAction,
strPath,
"xml",
strXml,
null,   // baTimestamp,
"",
out strOutputPath,
out baNewTimestamp,
out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode != ErrorCode.AccessDenied)
                    return -1;
            }
            else
            {
                // 注意检查部分允许写入的报错
                if (channel.ErrorCode == ErrorCode.PartialDenied)
                    biblio_access_list.Add("partial_overwrite");
                else
                    biblio_access_list.Add("overwrite");
            }

            // 模拟书目删除
            strAction = "simulate_delete";
            strPath = strBiblioDbName + "/0";
            lRet = channel.SetBiblioInfo(
stop,
strAction,
strPath,
"xml",
strXml,
null,   // baTimestamp,
"",
out strOutputPath,
out baNewTimestamp,
out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode != ErrorCode.AccessDenied)
                    return -1;
            }
            else
            {
                // 注意检查部分允许写入的报错
                if (channel.ErrorCode == ErrorCode.PartialDenied)
                    biblio_access_list.Add("partial_delete");
                else
                    biblio_access_list.Add("delete");
            }

            strBiblioAccess = StringUtil.MakePathList(biblio_access_list);

            // 模拟实体追加写入
            EntityInfo info = new EntityInfo();
            {
                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml("<root />");

                info.Style = "simulate";
                info.RefID = Guid.NewGuid().ToString();

                DomUtil.SetElementText(item_dom.DocumentElement,
                    "parent", "0");

                info.OldRecPath = "";
                info.OldRecord = "";
                info.OldTimestamp = null;

                info.Action = "new";
                info.NewRecPath = "";

                info.NewRecord = item_dom.DocumentElement.OuterXml;
                info.NewTimestamp = null;
            }

            EntityInfo[] entities = new EntityInfo[1];
            entities[0] = info;

            EntityInfo[] errorinfos = null;

            lRet = channel.SetEntities(
     stop,
     strPath,
     entities,
     out errorinfos,
     out strError);
            if (lRet == -1)
            {
                List<string> normal_errors = new List<string>();
                List<string> accessdenied_errors = new List<string>();
                GetErrorInfo(errorinfos, out normal_errors, out accessdenied_errors);
                if (normal_errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(normal_errors, " ; ");
                    return -1;
                }
            }
            else
            {
                entity_access_list.Add("append");
            }

            // 模拟实体覆盖写入
            info.Action = "change";
            lRet = channel.SetEntities(
stop,
strPath,
entities,
out errorinfos,
out strError);
            if (lRet == -1)
            {
                List<string> normal_errors = new List<string>();
                List<string> accessdenied_errors = new List<string>();
                GetErrorInfo(errorinfos, out normal_errors, out accessdenied_errors);
                if (normal_errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(normal_errors, " ; ");
                    return -1;
                }
            }
            else
            {
                entity_access_list.Add("overwrite");
            }

            // 模拟实体删除操作
            info.Action = "delete";

            // 是否要提供旧记录？
            info.OldRecPath = "";
            info.OldRecord = "";
            info.OldTimestamp = null;

            info.NewRecPath = "";
            info.NewRecord = "";
            info.NewTimestamp = null;

            lRet = channel.SetEntities(
stop,
strPath,
entities,
out errorinfos,
out strError);
            if (lRet == -1)
            {
                List<string> normal_errors = new List<string>();
                List<string> accessdenied_errors = new List<string>();
                GetErrorInfo(errorinfos, out normal_errors, out accessdenied_errors);
                if (normal_errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(normal_errors, " ; ");
                    return -1;
                }
            }
            else
            {
                entity_access_list.Add("delete");
            }

            strEntityAccess = StringUtil.MakePathList(entity_access_list);
            return 0;
        }

        static int GetErrorInfo(EntityInfo[] errorinfos,
            out List<string> normal_errors,
            out List<string> accessdenied_errors)
        {
            normal_errors = new List<string>();
            accessdenied_errors = new List<string>();

            if (errorinfos == null)
                return 0;

            int nCount = 0;
            foreach (EntityInfo info in errorinfos)
            {
                if (String.IsNullOrEmpty(info.RefID) == true)
                {
                    normal_errors.Add("服务器返回的 EntityInfo 结构中 RefID 为空");
                    nCount++;
                    return nCount;
                }

                // 正常信息处理
                if (info.ErrorCode == ErrorCodeValue.NoError)
                    continue;

                if (info.ErrorCode == ErrorCodeValue.AccessDenied)
                {
                    accessdenied_errors.Add(info.ErrorInfo);
                    nCount++;
                }
                else
                {
                    normal_errors.Add(info.RefID + "在提交保存过程中发生错误 -- " + info.ErrorInfo);
                    nCount++;
                }
            }

            return nCount;
        }

        private void button_setting_reCreateServersXml_Click(object sender, EventArgs e)
        {
            // _base.ServersDom = null;    // test

            this.ClearMessage();
            this.EnableControls(false);
            try
            {
                string strError = "";
                // string strFileName = Path.Combine(this.MainForm.ServerCfgDir, "servers.xml");
                // 当前登录的主要服务器不同，则需要的 xml 配置文件是不同的。应当存储在各自的目录中
                string strFileName = Path.Combine(this.MainForm.ServerCfgDir, ReportForm.GetValidPathString(this.MainForm.GetCurrentUserName()) + "\\servers.xml");
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strFileName));

                // 创建 servers.xml 配置文件
                int nRet = this.MainForm.BuildServersCfgFile(strFileName,
                    out strError);
                if (nRet == -1)
                {
                    // MessageBox.Show(this, strError);
                    this.ShowMessage(strError, "red", true);
                    return;
                }

                this.ShowMessage("配置文件 " + strFileName + " 创建成功", "green", true);

                // 重新加载
                LoadServerXml();
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        public string UnimarcBiblioDefault
        {
            get
            {
                return this.MainForm.AppInfo.GetString("entityRegisterWizard", 
                    "unimarcBiblioDefault",
                    "010  $a$dCNY??\r\n2001 $a$f\r\n210  $a$c$d\r\n215  $a??页$d??cm\r\n690  $a\r\n701  $a");
            }
            set
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard",
                    "unimarcBiblioDefault",
                    value);
            }
        }

        public string Marc21BiblioDefault
        {
            get
            {
                return this.MainForm.AppInfo.GetString("entityRegisterWizard",
                    "marc21BiblioDefault",
                    "010  $a$d\r\n200  $a$f\r\n210\r\n215\r\n690\r\n701  $a");
            }
            set
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard",
                    "marc21BiblioDefault",
                    value);
            }
        }

#if NO
        public string UnimarcBiblioImportantFields
        {
            get
            {
                return this.MainForm.AppInfo.GetString("entityRegisterWizard",
                    "unimarcImportantFields",
                    "010,200,210,215,686,69*,7**");
            }
            set
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard",
                    "unimarcImportantFields",
                    value);
                OnImportantFieldsChanged();
            }
        }

        public string Marc21BiblioImportantFields
        {
            get
            {
                return this.MainForm.AppInfo.GetString("entityRegisterWizard",
                    "marc21ImportantFields",
                    "010,200,210,215,686,69*,7**");
            }
            set
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard",
                    "marc21ImportantFields",
                    value);
                OnImportantFieldsChanged();
            }
        }
#endif
        public string UnimarcBiblioHiddenFields
        {
            get
            {
                return this.MainForm.AppInfo.GetString("entityRegisterWizard",
                    "unimarcHiddenFields",
                    "00*,1**,856,9**");
            }
            set
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard",
                    "unimarcHiddenFields",
                    value);
                OnHiddenFieldsChanged();
            }
        }

        public string Marc21BiblioHiddenFields
        {
            get
            {
                return this.MainForm.AppInfo.GetString("entityRegisterWizard",
                    "marc21HiddenFields",
                    "00*,856");
            }
            set
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard",
                    "marc21HiddenFields",
                    value);
                OnHiddenFieldsChanged();
            }
        }

        private void button_settings_bilbioDefault_Click(object sender, EventArgs e)
        {
            //controls.Add(new ControlWrapper(this.textBox_settings_importantFields, "010,200,210,215,686,69*,7**".Replace(",", "\r\n")));

            MarcDefaultDialog dlg = new MarcDefaultDialog();
            // GuiUtil.AutoSetDefaultFont(dlg);

            dlg.UnimarcDefault = this.UnimarcBiblioDefault;
            dlg.UnimarcHiddenFields = this.UnimarcBiblioHiddenFields.Replace(",","\r\n");
            dlg.Marc21Default = this.Marc21BiblioDefault;
            dlg.Marc21HiddenFields = this.Marc21BiblioHiddenFields.Replace(",", "\r\n");

            dlg.StartPosition = FormStartPosition.CenterScreen;
            this.MainForm.AppInfo.LinkFormState(dlg, "entityRegisterWizard_biblioDefault");
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.UnimarcBiblioDefault = dlg.UnimarcDefault;
            this.UnimarcBiblioHiddenFields = dlg.UnimarcHiddenFields.Replace("\r\n", ",");
            this.Marc21BiblioDefault = dlg.Marc21Default;
            this.Marc21BiblioHiddenFields = dlg.Marc21HiddenFields.Replace("\r\n", ",");

        }
#if NO
        void RegisterMouseClick(Control parent)
        {
            parent.MouseClick += parent_MouseClick;
            foreach(Control child in parent.Controls)
            {
                child.MouseClick += parent_MouseClick;
                RegisterMouseClick(child);
            }

            if (parent is SplitContainer)
            {
                SplitContainer split = parent as SplitContainer;
                RegisterMouseClick(split.Panel1);
                RegisterMouseClick(split.Panel2);
            }
        }

        void parent_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;
            if (string.IsNullOrEmpty(this._floatingMessage.Text) == false)
                this._floatingMessage.Text = "";
        }
#endif

    }
}
