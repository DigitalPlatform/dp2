using DigitalPlatform;
using DigitalPlatform.AmazonInterface;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
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

namespace dp2Circulation
{
    /// <summary>
    /// 册登记向导窗口。适合初学者使用的册登记窗口
    /// </summary>
    public partial class EntityRegisterWizard : MyForm, IBiblioItemsWindow
    {
        GenerateData _genData = null;

        FloatingMessageForm _floatingMessage = null;

        EntityRegisterBase _base = new EntityRegisterBase();

        BiblioAndEntities _biblio = null;

        public EntityRegisterWizard()
        {
            InitializeComponent();

            this.dpTable_browseLines.ImageList = this.imageList_progress;
            CreateBrowseColumns();

            // Add any initialization after the InitializeComponent() call.
            this.NativeTabControl1 = new NativeTabControl();
            this.NativeTabControl1.AssignHandle(this.tabControl_main.Handle);

            _biblio = new BiblioAndEntities(this,
                easyMarcControl1, 
                flowLayoutPanel1);
            _biblio.GetValueTable += _biblio_GetValueTable;
            _biblio.DeleteItem += _biblio_DeleteItem;
            _biblio.LoadEntities += _biblio_LoadEntities;
            _biblio.GetDefaultItem += _biblio_GetDefaultItem;
            _biblio.GenerateData += _biblio_GenerateData;
        }

        void _biblio_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this._genData.AutoGenerate(sender, e, this.BiblioRecPath);
        }

        void _biblio_GetDefaultItem(object sender, GetDefaultItemEventArgs e)
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

        void _biblio_DeleteItem(object sender, DeleteItemEventArgs e)
        {
            DeleteItem(e.Control);
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
                controls.Add(this.textBox_settings_importantFields);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.comboBox_from);
                controls.Add(this.textBox_queryWord);
                controls.Add(this.splitContainer_biblioAndItems);
                controls.Add(this.textBox_settings_importantFields);
                GuiState.SetUiState(controls, value);
            }
        }


        private void EntityRegisterWizard_Load(object sender, EventArgs e)
        {
            _biblio.MainForm = this.MainForm;
            _base.MainForm = this.MainForm;

            this._originTitle = this.Text;

            SetTitle();
            SetButtonState();

            LoadServerXml();

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

            this.MainForm.Move += new EventHandler(MainForm_Move);

            // this.MainForm.MessageFilter += MainForm_MessageFilter;

            SetControlsColor();

            this.UiState = this.MainForm.AppInfo.GetString("entityRegisterWizard", "uistate", "");
            // 缺省值 检索途径
            if (string.IsNullOrEmpty(this.comboBox_from.Text) == true
                && this.comboBox_from.Items.Count > 0)
                this.comboBox_from.Text = this.comboBox_from.Items[0] as string;
            // 缺省值 书目重要字段
            if (string.IsNullOrEmpty(this.textBox_settings_importantFields.Text) == true)
                this.textBox_settings_importantFields.Text = "010,200,210,215,686,69*,7**".Replace(",", "\r\n");

            this._genData = new GenerateData(this, this);
            this._genData.ScriptFileName = "dp2circulation_marc_autogen_2.cs";
            this._genData.DetailHostType = typeof(BiblioItemsHost);

        }

        void SetControlsColor()
        {
            this.button_settings_entityDefault.BackColor = this.BackColor;
            this.button_settings_entityDefault.ForeColor = this.ForeColor;

            this.textBox_settings_importantFields.BackColor = this.BackColor;
            this.textBox_settings_importantFields.ForeColor = this.ForeColor;


            this.textBox_queryWord.BackColor = this.BackColor;
            this.textBox_queryWord.ForeColor = this.ForeColor;

            this.comboBox_from.BackColor = this.BackColor;
            this.comboBox_from.ForeColor = this.ForeColor;

            this.button_search.BackColor = this.BackColor;
            this.button_search.ForeColor = this.ForeColor;
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

        void MainForm_Move(object sender, EventArgs e)
        {
            this._floatingMessage.OnResizeOrMove();
        }

        private void EntityRegisterWizard_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void EntityRegisterWizard_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this._genData != null)
            {
                this._genData.Close();
            }

            if (this.MainForm != null)
            {
                this.MainForm.AppInfo.SetString("entityRegisterWizard", "uistate", this.UiState);

                // this.MainForm.MessageFilter -= MainForm_MessageFilter;
                this.MainForm.Move -= new EventHandler(MainForm_Move);
            }

            if (_floatingMessage != null)
                _floatingMessage.Close();
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
            string strFileName = Path.Combine(this.MainForm.DataDir, "servers.xml");
            if (File.Exists(strFileName) == false
                || MainForm.GetServersCfgFileVersion(strFileName) < (double)0.01)
            {
                string strError = "";
                // 创建 servers.xml 配置文件
                int nRet = this.MainForm.BuildServersCfgFile(strFileName,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "文件 '" + strFileName + "' 装入XMLDOM 时出错: " + ex.Message);
                return;
            }

            // TODO: 是否在文件不存在的情况下，给出缺省的几个 server ?

            _base.ServersDom = dom;
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
                this.toolStripButton_save.Enabled = true;
            else
                this.toolStripButton_save.Enabled = false;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetTitle();
            SetButtonState();
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
            DoSearch(this.textBox_queryWord.Text, this.comboBox_from.Text);
        }

        void ShowMessage(string strMessage, 
            string strColor = "",
            bool bClickClose = false)
        {
            Color color = Color.FromArgb(80,80,80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        void ClearMessage()
        {
            this._floatingMessage.Text = "";
        }

        void DoSearch(string strQueryWord, string strFrom)
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
                    AccountInfo account = EntityRegisterBase.GetAccountInfo(server);
                    Debug.Assert(account != null, "");
                    _base.CurrentAccount = account;

                    if (account.ServerType == "dp2library")
                    {
                        nRet = SearchLineDp2library(
                            strQueryWord,
                            strFrom,
                            account,
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
                            out strError);
                        if (nRet == -1)
                            strTotalError += strError + "\r\n";
                        else
                            nHitCount += nRet;
                    }
                }

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
                this.Progress.EndLoop();
                this.Progress.OnStop -= new StopEventHandler(this.DoStop);
                // this.Progress.Initial("");
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
#if NO
            line.SetDisplayMode("summary");
            line.SetBiblioSearchState("error");
            line.BiblioSummary = strError;
            DisplayFloatErrorText(strError);

            line._biblioRegister.BarColor = "R";   // 红色，需引起注意
            this.SetColorList();
#endif
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
            out string strError)
        {
            strError = "";

            this.ShowMessage("正在针对 " + account.ServerName + " \r\n检索 " + strQueryWord + " ...",
                "progress", false);

            AmazonSearch search = new AmazonSearch();
            // search.MainForm = this.MainForm;
            search.TempFileDir = this.MainForm.UserTempDir;

            search.Timeout = 20 * 1000;
            search.Idle += search_Idle;
            try
            {

                // 多行检索中的一行检索
                int nRedoCount = 0;
            REDO:
                int nRet = search.Search(
                    account.ServerUrl,
                    strQueryWord.Replace("-", ""),
                    "ISBN",
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
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                return nRet;
            }
            finally
            {
                search.Idle -= search_Idle;
            }

        ERROR1:
            strError = "针对服务器 '" + account.ServerName + "' 检索出错: " + strError;
            AddBiblioBrowseLine(strError, TYPE_ERROR);
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

            RegisterBiblioInfo info = new RegisterBiblioInfo();
            info.OldXml = strMARC;
            info.Timestamp = null;
            info.RecPath = strASIN + "@" + _base.CurrentAccount.ServerName;
            info.MarcSyntax = "unimarc";
            AddBiblioBrowseLine(
                -1,
                info.RecPath,
                StringUtil.MakePathList(cols, "\t"),
                info);

            return 0;
        }


        #endregion

        #region 创建书目记录的浏览格式

        // 创建MARC格式记录的浏览格式
        // paramters:
        //      strMARC MARC机内格式
        public int BuildMarcBrowseText(
            string strMarcSyntax,
            string strMARC,
            out string strBrowseText,
            out string strError)
        {
            strBrowseText = "";
            strError = "";

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this.MainForm;

            BrowseFilterDocument filter = null;

            string strFilterFileName = Path.Combine(this.MainForm.DataDir, strMarcSyntax.Replace(".", "_") + "_cfgs\\marc_browse.fltx");

            int nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strBrowseText = host.ResultString;

            }
            finally
            {
                // 归还对象
                this.MainForm.Filters.SetFilter(strFilterFileName, filter);
            }

            return 0;
        ERROR1:
            return -1;
        }

        public int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // 看看是否有现成可用的对象
            filter = (BrowseFilterDocument)this.MainForm.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // 新创建
            // string strFilterFileContent = "";

            filter = new BrowseFilterDocument();

            filter.FilterHost = host;
            filter.strOtherDef = "FilterHost Host = null;";

            filter.strPreInitial = " BrowseFilterDocument doc = (BrowseFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = ("
                + "FilterHost" + ")doc.FilterHost;\r\n";

            // filter.Load(strFilterFileName);

            try
            {
                filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strBinDir = Environment.CurrentDirectory;

            string[] saAddRef1 = {
										 strBinDir + "\\digitalplatform.marcdom.dll",
										 // this.BinDir + "\\digitalplatform.marckernel.dll",
										 // this.BinDir + "\\digitalplatform.libraryserver.dll",
										 strBinDir + "\\digitalplatform.dll",
										 strBinDir + "\\digitalplatform.Text.dll",
										 strBinDir + "\\digitalplatform.IO.dll",
										 strBinDir + "\\digitalplatform.Xml.dll",
										 strBinDir + "\\dp2circulation.exe" };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // 创建Script的Assembly
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

        #endregion

        #region 针对 dp2library 服务器的检索

        LibraryChannel _channel = null;

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
            out string strError)
        {
            strError = "";
            int nRet = 0;

            _channel = _base.GetChannel(account.ServerUrl, account.UserName);
            _channel.Timeout = new TimeSpan(0, 0, 5);   // 超时值为 5 秒
            _channel.Idle += _channel_Idle;
            try
            {
                if (string.IsNullOrEmpty(strFrom) == true)
                    strFrom = "ISBN";
                string strFromStyle = "";
                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle(strFrom);
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + strFrom + "' 对应的style字符串";
                    goto ERROR1;
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

                ServerInfo server_info = null;

                //if (line != null)
                //    line.BiblioSummary = "正在获取服务器 " + account.ServerName + " 的配置信息 ...";
                this.ShowMessage("正在获取服务器 " + account.ServerName + " 的配置信息 ...", 
                    "progress", false);

                // 准备服务器信息
                nRet = _base.GetServerInfo(
                    _channel,
                    account,
                    out server_info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;    // 可以不报错 ?

                this.ShowMessage("正在针对 " + account.ServerName + " \r\n检索 " + strQueryWord + " ...",
                    "progress", false);

                string strQueryXml = "";
                long lRet = _channel.SearchBiblio(Progress,
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

                    lRet = _channel.GetSearchResult(
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
                        // 获得书目摘要
                        BiblioLoader loader = new BiblioLoader();
                        loader.Channel = _channel;
                        loader.Stop = this.Progress;
                        loader.Format = "xml";
                        loader.GetBiblioInfoStyle = GetBiblioInfoStyle.Timestamp;
                        loader.RecPaths = biblio_recpaths;

                        try
                        {
                            int i = 0;
                            foreach (BiblioItem item in loader)
                            {
                                string strXml = item.Content;

                                string strMARC = "";
                                string strMarcSyntax = "";
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

                                string strBrowseText = "";
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

                                RegisterBiblioInfo info = new RegisterBiblioInfo();
                                info.OldXml = strMARC;
                                info.Timestamp = item.Timestamp;
                                info.RecPath = item.RecPath + "@" + account.ServerName;
                                info.MarcSyntax = strMarcSyntax;
                                AddBiblioBrowseLine(
                                    -1,
                                    info.RecPath,
                                    strBrowseText,
                                    info);
                                i++;
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = ex.Message;
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
                _channel.Idle -= _channel_Idle;
                _base.ReturnChannel(_channel);
                _channel = null;
                this.ClearMessage();
            }

        ERROR1:
            AddBiblioBrowseLine(strError, TYPE_ERROR);
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

        public void AddBiblioBrowseLine(string strText,
    int nType)
        {
            // this._biblioRegister.AddBiblioBrowseLine(strText, nType);
            this.AddBiblioBrowseLine(
    nType,
    strText,
    "",
    null);
        }

        // 加入一个浏览行
        public void AddBiblioBrowseLine(
            int nType,
            string strBiblioRecPath,
            string strBrowseText,
            RegisterBiblioInfo info)
        {
            if (this.dpTable_browseLines.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<int, string, string, RegisterBiblioInfo>(AddBiblioBrowseLine),
                    nType,
                    strBiblioRecPath,
                    strBrowseText,
                    info);
                return;
            }

            List<string> columns = StringUtil.SplitList(strBrowseText, '\t');
            DpRow row = new DpRow();

            DpCell cell = new DpCell();
            cell.Text = (this.dpTable_browseLines.Rows.Count + 1).ToString();
            {
                cell.ImageIndex = nType;
                if (nType == TYPE_ERROR)
                    cell.BackColor = Color.Red;
                else if (nType == TYPE_INFO)
                    cell.BackColor = Color.Yellow;
            }
            row.Add(cell);

            cell = new DpCell();
            cell.Text = strBiblioRecPath;
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
                this.dpTable_browseLines.Focus();
                row.Selected = true;
                this.dpTable_browseLines.FocusedItem = row;
            }

            PrepareCoverImage(row);
        }

        // 创建浏览栏目标题
        void CreateBrowseColumns()
        {
            if (this.dpTable_browseLines.Columns.Count > 2)
                return;

            List<string> columns = new List<string>() { "书名", "作者", "出版者", "出版日期" };
            foreach (string s in columns)
            {
                DpColumn column = new DpColumn();
                column.Text = s;
                column.Width = 120;
                this.dpTable_browseLines.Columns.Add(column);
            }
        }

        public void ClearList()
        {
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

        public event AsyncGetImageEventHandler AsyncGetImage = null;
        bool CoverImageRequested = false; // 如果为 true ,表示已经请求了异步获取图像，不要重复请求

        // 准备特定浏览行的封面图像
        void PrepareCoverImage(DpRow row)
        {
            Debug.Assert(row != null, "");

            RegisterBiblioInfo info = row.Tag as RegisterBiblioInfo;
            if (info == null)
                return;

            if (string.IsNullOrEmpty(info.CoverImageFileName) == false)
                return;

            string strMARC = info.OldXml;
            if (string.IsNullOrEmpty(strMARC) == true)
                return;

            string strUrl = ScriptUtil.GetCoverImageUrl(strMARC);
            if (string.IsNullOrEmpty(strUrl) == true)
                return;

            if (StringUtil.HasHead(strUrl, "http:") == true)
                return;

            if (info != null && info.CoverImageRquested == true)
                return;

            // 通过 dp2library 协议获得图像文件
            if (this.AsyncGetImage != null)
            {
                AsyncGetImageEventArgs e = new AsyncGetImageEventArgs();
                e.RecPath = row[1].Text;
                e.ObjectPath = strUrl;
                e.FileName = "";
                e.Row = row;
                this.AsyncGetImage(this, e);
                // 修改状态，表示已经发出请求
                if (row != null)
                {
                    if (info != null)
                        info.CoverImageRquested = true;
                }
                else
                {
                    this.CoverImageRequested = true;
                }
            }
        }

        #endregion

        int SetBiblio(RegisterBiblioInfo info, out string strError)
        {
            int nRet = this._biblio.SetBiblio(info, out strError);
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
            string strError = "";
            if (this.dpTable_browseLines.SelectedRows.Count == 0)
            {
                strError = "请选择要装入的一行";
                goto ERROR1;
            }

            this.tabControl_main.SelectedTab = this.tabPage_biblioAndItems;

            RegisterBiblioInfo info = this.dpTable_browseLines.SelectedRows[0].Tag as RegisterBiblioInfo;
            if (info == null)
            {
                strError = "这是提示信息行";
                goto ERROR1;
            }

            int nRet = SetBiblio(info, out strError);
            if (nRet == -1)
                goto ERROR1;

            this.easyMarcControl1.SelectFirstItem();
            return;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
        }

        private void easyMarcControl1_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            // if (GetConfigDom != null)
            {
                if (string.IsNullOrEmpty(this._biblio.BiblioRecPath) == true
                    && string.IsNullOrEmpty(this._biblio.ServerName) == true)
                    e.Path = e.Path + "@!unknown";
                else
                {
                    // e.Path = Global.GetDbName(this.BiblioRecPath) + "/cfgs/" + e.Path + "@" + this.ServerName;
                    e.Path = e.Path + "@" + this._biblio.ServerName;
                }

                // GetConfigDom(this, e);
                MarcEditor_GetConfigDom(sender, e);
            }
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

            string strPath = "";
            string strServerName = "";
            StringUtil.ParseTwoPart(e.Path, "@", out strPath, out strServerName);

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

            AccountInfo account = _base.GetAccountInfo(strServerName);
            if (account == null)
            {
                e.ErrorInfo = "e.Path 中 '" + e.Path + "' 服务器名 '" + strServerName + "' 没有配置";
                goto ERROR1;
            }

            Debug.Assert(strServerType == "dp2library", "");

            //BiblioRegisterControl control = sender as BiblioRegisterControl;
            //string strBiblioDbName = Global.GetDbName(control.BiblioRecPath);
            string strBiblioDbName = Global.GetDbName(this._biblio.BiblioRecPath);

            // 得到干净的文件名
            string strCfgFilePath = strBiblioDbName + "/cfgs/" + strPath;    // e.Path;
            int nRet = strCfgFilePath.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFilePath = strCfgFilePath.Substring(0, nRet);
            }

            // 在cache中寻找
            e.XmlDocument = this.MainForm.DomCache.FindObject(strCfgFilePath);
            if (e.XmlDocument != null)
                return;

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

            _channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);

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

                    long lRet = _channel.GetEntities(
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

                        // 添加一个新的册对象
                        nRet = this._biblio.NewEntity(entity.OldRecPath,
                            entity.OldTimestamp,
                            entity.OldRecord,
                            false,  // 不必滚入视野
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

                _base.ReturnChannel(_channel);
                _channel = null;
                _currentAccount = null;
            }
        }

        void DeleteItem(EntityEditControl edit)
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
            int nRet = 0;

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

            _channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);
            try
            {

                string strBiblioRecPath = this._biblio.BiblioRecPath;

                int nBatch = 100;
                for (int i = 0; i < (entities.Length / nBatch) + ((entities.Length % nBatch) != 0 ? 1 : 0); i++)
                {
                    int nCurrentCount = Math.Min(nBatch, entities.Length - i * nBatch);
                    EntityInfo[] current = GetPart(entities, i * nBatch, nCurrentCount);

                    long lRet = _channel.SetEntities(
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
                _base.ReturnChannel(_channel);
                _channel = null;
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

        #endregion

        #region 保存书目记录

        // 保存书目记录和下属的册记录
        void SaveBiblioAndItems()
        {
            string strError = "";
            int nRet = 0;

            bool bBiblioSaved = false;

            this.ShowMessage("正在保存书目和册记录", "progress", false);

            this.Progress.OnStop += new StopEventHandler(this.DoStop);
            this.Progress.BeginLoop();
            try
            {
                string strCancelSaveBiblio = "";

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
                        out strServerName,
                        out strBiblioRecPath);
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
                    nRet = GetBiblioXml(
                        out strXml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strWarning = "";
                    string strOutputPath = "";
                    byte[] baNewTimestamp = null;
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
                    this._biblio.ServerName = strServerName;
                    this._biblio.BiblioRecPath = strOutputPath;
                    this._biblio.Timestamp = baNewTimestamp;
                    this._biblio.BiblioChanged = false;

                    bBiblioSaved = true;
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
                    }
                    else
                    {
                        this._biblio.EntitiesChanged = false;
                        // line._biblioRegister.BarColor = "G";   // 绿色
                        if (bBiblioSaved == false)
                            this.ShowMessage("没有可保存的信息", "yellow", true);
                        return;
                    }
                }

                if (string.IsNullOrEmpty(strCancelSaveBiblio) == false)
                {
                    this.ShowMessage("书目记录无法保存，但册记录保存成功\r\n(" + strCancelSaveBiblio + ")",
                        "red", true);
                }
                else
                {
                    this.ShowMessage("保存成功", "green", true);
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
            this.ShowMessage(strError, "red", true);
        }

        // 获得书目记录的XML格式
        // parameters:
        int GetBiblioXml(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            string strBiblioDbName = Global.GetDbName(this._biblio.BiblioRecPath);

            string strMarcSyntax = "";

            // TODO: 如何获得远程其他 dp2library 服务器的书目库的 syntax?
            // 获得库名，根据库名得到marc syntax
            if (String.IsNullOrEmpty(strBiblioDbName) == false)
                strMarcSyntax = MainForm.GetBiblioSyntax(strBiblioDbName);

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            // 2008/5/16 changed
            string strMARC = this._biblio.GetMarc();
            XmlDocument domMarc = null;
            int nRet = MarcUtil.Marc2Xml(strMARC,
                strMarcSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 因为domMarc是根据MARC记录合成的，所以里面没有残留的<dprms:file>元素，也就没有(创建新的id前)清除的需要

            Debug.Assert(domMarc != null, "");

#if NO
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
#endif

            strXml = domMarc.OuterXml;
            return 0;
        }

        // 保存XML格式的书目记录到数据库
        // parameters:
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
            _channel = _base.GetChannel(_currentAccount.ServerUrl, _currentAccount.UserName);

            try
            {
                string strAction = "change";

                if (Global.IsAppendRecPath(strPath) == true)
                    strAction = "new";

            REDO:
                long lRet = _channel.SetBiblioInfo(
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
                if (_channel.ErrorCode == ErrorCode.PartialDenied)
                {
                    strWarning = "书目记录 '" + strPath + "' 保存成功，但所提交的字段部分被拒绝 (" + strError + ")。请留意刷新窗口，检查实际保存的效果";
                }

                return 1;
            ERROR1:
                return -1;
            }
            finally
            {
                _base.ReturnChannel(_channel);
                this._channel = null;
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
                    if (bAppend == false && StringUtil.IsInList("overwrite", strAccess) == true)
                        return true;
                }
            }

            return false;
        }

        // 根据书目记录的路径，匹配适当的目标
        // parameters:
        //      bAllowCopyTo    是否允许书目记录复制到其他库？这发生在原始库不让 overwrite 的时候
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetTargetInfo(
            bool bAllowCopyTo,
            out string strServerName,
            out string strBiblioRecPath)
        {
            strServerName = "";
            strBiblioRecPath = "";

            // string strEditServerUrl = "";
            string strEditServerName = this._biblio.ServerName;
            string strEditBiblioRecPath = this._biblio.BiblioRecPath;

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

        private void flowLayoutPanel1_SizeChanged(object sender, EventArgs e)
        {
            // 当窗口较小，垂直卷动看到(册记录显示区)下部内容的时候，最大化窗口，则会留下卷滚条的残余图像。
            // 这一行语句能消除这个问题
            //this.flowLayoutPanel1.Update();
            //this.flowLayoutPanel1.Invalidate();
        }

        private void toolStripButton_start_Click(object sender, EventArgs e)
        {
            this.tabControl_main.SelectedTab = this.tabPage_searchBiblio;
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
                this.tabControl_main.SelectedIndex++;
            }
        }

        private void toolStripButton_save_Click(object sender, EventArgs e)
        {
            SaveBiblioAndItems();
        }

        // 装入一条空白书目记录
        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            string strError = "";

            RegisterBiblioInfo info = null;
            if (this.tabControl_main.SelectedTab == this.tabPage_searchBiblio)
                info = BuildBlankBiblioInfo(
                     "",
                     this.comboBox_from.Text,
                     this.textBox_queryWord.Text);
            else
                info = BuildBlankBiblioInfo(
     "",
     "",
     "");
            // 如果当前不在书目 page，要自动切换到位
            this.tabControl_main.SelectedTab = this.tabPage_biblioAndItems;

            int nRet = SetBiblio(info, out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this, strError);
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
            if (strFrom == "作者" || strFrom == "著者")
                strAuthor = strValue;
            if (strFrom == "出版者" || strFrom == "出版社")
                strPublisher = strValue;

            info.MarcSyntax = "unimarc";
            MarcRecord record = new MarcRecord(strRecord);
            if (string.IsNullOrEmpty(strRecord) == true)
            {
                record.add(new MarcField('$', "010  $a" + strISBN + "$dCNY??"));
                record.add(new MarcField('$', "2001 $a"+strTitle+"$f"));
                record.add(new MarcField('$', "210  $a$c"+strPublisher+"$d"));
                record.add(new MarcField('$', "215  $a$d??cm"));
                record.add(new MarcField('$', "690  $a"));
                record.add(new MarcField('$', "701  $a" + strAuthor));
            }
            else
            {
                record.setFirstSubfield("010", "a", strISBN);
                record.setFirstSubfield("200", "a", strTitle);
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
            info.OldXml = record.Text;

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

        private void textBox_settings_importantFields_TextChanged(object sender, EventArgs e)
        {
            if (this._biblio != null)
            {
                this._biblio.HideFieldNames = StringUtil.SplitList(this.textBox_settings_importantFields.Text.Replace("\r\n", ","));
                this._biblio.HideFieldNames.Insert(0, "rvs");
            }
        }

        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;
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
