using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Threading;

using System.Deployment.Application;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Net;   // for WebClient class

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;   // QuickPinyin IsbnSplitter
using DigitalPlatform.CommonControl;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.GcatClient.gcat_new_ws;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;

namespace dp2Catalog
{
    public partial class MainForm : Form
    {
        // MarcFilter���󻺳��
        public FilterCollection Filters = new FilterCollection();

        public OperHistory OperHistory = null;

        const int WM_PREPARE = API.WM_USER + 200;
        /*
        public string UsedUcUserName = "";
        public string UserdUcPassword = "";
        public bool UserdUcSavePassword = false;
         * */
        public ObjectCache<Assembly> AssemblyCache = new ObjectCache<Assembly>();
        public ObjectCache<XmlDocument> DomCache = new ObjectCache<XmlDocument>();

        // ΪC#�ű���׼��
        public Hashtable ParamTable = new Hashtable();

        public QuickPinyin QuickPinyin = null;
        public IsbnSplitter IsbnSplitter = null;
        public QuickSjhm QuickSjhm = null;

        public CfgCache cfgCache = new CfgCache();

        // dp2library��������Ϣ(���ݿ�syntax��)
        public dp2ServerInfoCollection ServerInfos = new dp2ServerInfoCollection();

        // dp2library����������(ȱʡ�û���/�����)
        public dp2ServerCollection Servers = null;

        public CharsetTable EaccCharsetTable = null;
        public Marc8Encoding Marc8Encoding = null;

        public string DataDir = "";

        /// <summary>
        /// �û�Ŀ¼
        /// </summary>
        public string UserDir = "";

        public string UserTempDir = "";

        //���������Ϣ
        public ApplicationInfo AppInfo = new ApplicationInfo("dp2catalog.xml");

        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();
        public DigitalPlatform.Stop Stop = null;

        public FromCollection Froms = new FromCollection();

        // ʹ�ù�������MARC�ļ���
        public string LinkedMarcFileName = "";
        public string LinkedEncodingName = "";
        public string LinkedMarcSyntax = "";

        // Ϊ�˱���ISO2709�ļ�����ļ�������
        public string LastIso2709FileName = "";
        public bool LastCrLfIso2709 = false;
        public bool LastRemoveField998 = false;
        public string LastEncodingName = "";

        public string LastSavePath = "";

        // ���ʹ�ù��Ĺ������ļ���
        public string LastWorksheetFileName = "";

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            this.SetTitle();

#if SN
#else
            this.MenuItem_resetSerialCode.Visible = false;
#endif

            // ��ʼ������Ŀ¼
            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                // MessageBox.Show(this, "no network");
                DataDir = Environment.CurrentDirectory;
            }

            {
                // 2015/5/8
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2Catalog_v2");
                PathUtil.CreateDirIfNeed(this.UserDir);

                this.UserTempDir = Path.Combine(this.UserDir, "temp");
                PathUtil.CreateDirIfNeed(this.UserTempDir);

                string strOldFileName = Path.Combine(this.DataDir, "zserver.xml");
                string strNewFileName = Path.Combine(this.UserDir, "zserver.xml");
                if (File.Exists(strNewFileName) == false)
                {
                    try
                    {
                        if (File.Exists(strOldFileName) == true)
                        {
                            // ������ 2.4 �������ԭ������Ŀ¼�е� zserver.xml �ļ��ƶ�����
                            File.Copy(strOldFileName, strNewFileName, true);
                            File.Delete(strOldFileName);    // ɾ��Դ�ļ��������û�������ĸ��ļ�������
                        }
                        else
                        {
                            // �հ�װ�õ�ʱ���û�Ŀ¼�л�û���ļ������Ǵ� default_zserver.xml �и��ƹ���
                            string strDefaultFileName = Path.Combine(this.DataDir, "default_zserver.xml");
                            Debug.Assert(File.Exists(strDefaultFileName) == true, "");
                            File.Copy(strDefaultFileName, strNewFileName, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "���� zserver.xml �ļ�ʱ����: " + ex.Message);
                    }
                }

            }

            // ���ô��ڳߴ�״̬
            if (AppInfo != null)
            {
                // �״����У��������á�΢���źڡ�����
                if (this.IsFirstRun == true)
                {
                    SetFirstDefaultFont();
                }

                GuiUtil.SetControlFont(this, this.DefaultFont);

                AppInfo.LoadFormStates(this,
                    "mainformstate",
                    FormWindowState.Maximized);
            }

            InitialFixedPanel();

            // Stop��ʼ��
            stopManager.Initial(this.toolButton_stop,
                (object)this.toolStripStatusLabel_main,
                (object)this.toolStripProgressBar_main);

            // stopManager.LinkReversButton(this.toolButton_search);
            List<object> reverse_buttons = new List<object>();
            reverse_buttons.Add(this.toolButton_search);
            reverse_buttons.Add(this.toolButton_nextBatch);
            reverse_buttons.Add(this.toolButton_getAllRecords);
            stopManager.LinkReverseButtons(reverse_buttons);

            // �˵�״̬
            this.SetMenuItemState();

            // cfgcache
            nRet = cfgCache.Load(this.DataDir
                + "\\cfgcache.xml",
                out strError);
            if (nRet == -1)
            {
                if (IsFirstRun == false)
                    MessageBox.Show(strError);
            }

            cfgCache.TempDir = this.DataDir
                + "\\cfgcache";
            cfgCache.InstantSave = true;

            // Z39.50 froms
            nRet = LoadFroms(this.DataDir + "\\bib1use.xml", out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            // MARC-8�ַ���
            this.EaccCharsetTable = new CharsetTable();
            try
            {
                this.EaccCharsetTable.Attach(this.DataDir + "\\eacc_charsettable",
                    this.DataDir + "\\eacc_charsettable.index");
                this.EaccCharsetTable.ReadOnly = true;  // ����Close()��ʱ��ɾ���ļ�

                this.Marc8Encoding = new Marc8Encoding(this.EaccCharsetTable,
                    this.DataDir + "\\asciicodetables.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "װ��EACC����ļ�ʱ��������: " + ex.Message);
            }


            // ���ļ���װ�ش���һ��dp2ServerCollection����
            // parameters:
            //		bIgnorFileNotFound	�Ƿ��׳�FileNotFoundException�쳣��
            //							���==true������ֱ�ӷ���һ���µĿ�ServerCollection����
            // Exception:
            //			FileNotFoundException	�ļ�û�ҵ�
            //			SerializationException	�汾Ǩ��ʱ���׳���
            try
            {

                Servers = dp2ServerCollection.Load(this.DataDir
                    + "\\servers.bin",
                    true);
                Servers.ownerForm = this;
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(this, ex.Message);
                Servers = new dp2ServerCollection();
                // �����ļ������Ա㱾�����н���ʱ���Ǿ��ļ�
                Servers.FileName = this.DataDir
                    + "\\servers.bin";
            }

            this.Servers.ServerChanged += new dp2ServerChangedEventHandle(Servers_ServerChanged);

            if (IsFirstRun == true && this.Servers.Count == 0)
            {
#if NO
                MessageBox.Show(this, "��ӭ����װʹ��dp2Catalog -- ��Ŀǰ��");

                // ��ʾ����dp2libraaryws������
                ManageServers(true);
                // ManagePreference();
#endif

                FirstRunDialog first_dialog = new FirstRunDialog();
                GuiUtil.SetControlFont(first_dialog, this.DefaultFont);
                first_dialog.MainForm = this;
                first_dialog.StartPosition = FormStartPosition.CenterScreen;
                if (first_dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                {
                    Application.Exit();
                    return;
                }

                // �״�д�� ����ģʽ ��Ϣ
                this.AppInfo.SetString("main_form", "last_mode", first_dialog.Mode);
                if (first_dialog.Mode == "test" || first_dialog.Mode == "community")
                {
                    this.AppInfo.SetString("sn", "sn", first_dialog.Mode);
                    this.AppInfo.Save();
                }

                if (first_dialog.ServerType == "[��ʱ��ʹ���κη�����]")
                {
                }
                else
                {
                    dp2ServerCollection newServers = Servers.Dup();
                    dp2Server server = newServers.NewServer(-1);
                    server.Name = first_dialog.ServerName;  // first_dialog.ServerType;
                    if (string.IsNullOrEmpty(server.Name) == true)
                        server.Name = "������";
                    server.DefaultPassword = first_dialog.Password;
                    server.Url = first_dialog.ServerUrl;
                    server.DefaultUserName = first_dialog.UserName;
                    server.SavePassword = true;

                    Servers.Changed = true;
                    this.Servers.Import(newServers);
                }

                // ���zserver.xml�Ƿ��Ѿ����ڣ�
                // һ����������ò��������ʽ��
                string strServerXmlPath = Path.Combine(this.UserDir, "zserver.xml");
                if (FileUtil.FileExist(strServerXmlPath) == false)
                {
                    // ���������ļ�zserver.xml
                    nRet = DownloadUserFile("zserver.xml",
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
            }

            nRet = this.LoadIsbnSplitter(true, out strError);
            if (nRet == -1)
            {
                strError = "װ��ISBN������ʱ���ִ���: " + strError;
                MessageBox.Show(this, strError);
            }


            this.LastSavePath = this.AppInfo.GetString(
                "main_form",
                "last_saved_path",
                "");

            StartPrepareNames(true);

            this.m_strPinyinGcatID = this.AppInfo.GetString("entity_form", "gcat_pinyin_api_id", "");
            this.m_bSavePinyinGcatID = this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", false);
        }

        public void StartPrepareNames(bool bFullInitial)
        {
            if (bFullInitial == true)
                API.PostMessage(this.Handle, WM_PREPARE, 1, 0);
            else
                API.PostMessage(this.Handle, WM_PREPARE, 0, 0);
        }

        void RestoreLastOpenedMdiWindow(string strOpenedMdiWindow)
        {

            // ȱʡ��һ��Z search form
            if (String.IsNullOrEmpty(strOpenedMdiWindow) == true)
            {
                if (this.Servers.Count == 0)
                    strOpenedMdiWindow = "dp2Catalog.ZSearchForm";  // ,dp2Catalog.dp2SearchForm
                else
                {
                    string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
                    if (string.IsNullOrEmpty(strSerialCode) == false)
                        strOpenedMdiWindow = "dp2Catalog.dp2SearchForm";
                }
            }

            string[] types = strOpenedMdiWindow.Split(new char[] {','});
            for (int i = 0; i < types.Length; i++)
            {
                string strType = types[i];
                if (String.IsNullOrEmpty(strType) == true)
                    continue;

                if (strType == "dp2Catalog.ZSearchForm")
                    this.MenuItem_openZSearchForm_Click(this, null);	// ��һ��Z39.50������
                else if (strType == "dp2Catalog.dp2SearchForm")
                    this.MenuItem_openDp2SearchForm_Click(this, null);	// ��һ��dp2������
                else if (strType == "dp2Catalog.DtlpSearchForm")
                    this.MenuItem_openDtlpSearchForm_Click(this, null);	// ��һ��dp2������
                else if (strType == "dp2Catalog.ZBatchSearchForm")
                    this.MenuItem_openZBatchSearchForm_Click(this, null);	// ��һ��ZBatchSearchForm������
                else if (strType == "dp2Catalog.AmazonSearchForm")
                    this.MenuItem_openAmazonSearchForm_Click(this, null);	// ��һ��AmaxonSearchForm������
                else
                    continue;

                // this.AppInfo.FirstMdiOpened = true; // ������ִ򿪳�Minimized״̬�Ĵ���
            }

            // ��洰��
            MenuItem_openAdvertiseForm_Click(this, null);

            // װ��MDI�Ӵ���״̬
            this.AppInfo.LoadFormMdiChildStates(this,
                "mainformstate");
        }

        public bool IsFirstRun
        {
            get
            {
                try
                {
                    if (ApplicationDeployment.CurrentDeployment.IsFirstRun == true)
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }

            }
        }

        void Servers_ServerChanged(object sender, dp2ServerChangedEventArgs e)
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child is dp2SearchForm)
                {
                    dp2SearchForm searchform = (dp2SearchForm)child;
                    searchform.RefreshResTree();
                }

            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ��ǰ��ر�MDI�Ӵ��ڵ�ʱ���Ѿ���������ֹ�رյ����������Ͳ����ٴ�ѯ����
            if (e.CloseReason == CloseReason.UserClosing && e.Cancel == true)
                return;

            if (e.CloseReason != CloseReason.ApplicationExitCall)
            {
                // ����ر�
                DialogResult result = MessageBox.Show(this,
                    "ȷʵҪ�˳� dp2Catalog -- dp2��Ŀǰ�� ? ",
                    "dp2Catalog",
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Servers.ServerChanged -= new dp2ServerChangedEventHandle(Servers_ServerChanged);

            // ���浽�ļ�
            // parameters:
            //		strFileName	�ļ��������==null,��ʾʹ��װ��ʱ������Ǹ��ļ���
            Servers.Save(null);
            Servers = null;


            // ���洰�ڳߴ�״̬
            if (AppInfo != null)
            {
                this.AppInfo.SetString(
                    "main_form",
                    "last_saved_path",
                    this.LastSavePath);

                string strOpenedMdiWindow = GuiUtil.GetOpenedMdiWindowString(this);
                this.AppInfo.SetString(
                    "main_form",
                    "last_opened_mdi_window",
                    strOpenedMdiWindow);

                /*
                // ����Z3950 Search����
                this.AppInfo.SetInt(
                    "main_form",
                    "last_zsearch_window",
                    this.TopZSearchForm != null ? 1 : 0);

                // ����dp2 Search����
                this.AppInfo.SetInt(
                    "main_form",
                    "last_dp2_search_window",
                    this.TopDp2SearchForm != null ? 1 : 0);

                // ����DTLP Search����
                this.AppInfo.SetInt(
                    "main_form",
                    "last_dtlp_search_window",
                    this.TopDtlpSearchForm != null ? 1 : 0);
                 * */

                FinishFixedPanel();

                AppInfo.SaveFormStates(this,
                    "mainformstate");

                if (this.m_bSavePinyinGcatID == false)
                    this.m_strPinyinGcatID = "";
                this.AppInfo.SetString("entity_form", "gcat_pinyin_api_id", this.m_strPinyinGcatID);
                this.AppInfo.GetBoolean("entity_form", "gcat_pinyin_api_saveid", this.m_bSavePinyinGcatID);
            }

            // cfgcache
            string strError;
            int nRet = cfgCache.Save(null, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);


            //��סsave,������ϢXML�ļ�
            AppInfo.Save();
            AppInfo = null;	// ������������������		
        }

        void InitialFixedPanel()
        {
            string strDock = this.AppInfo.GetString(
                "MainForm",
                "fixedpanel_dock",
                "right");
            int nFixedHeight = this.AppInfo.GetInt(
                "MainForm",
                "fixedpanel_height",
                100);
            int nFixedWidth = this.AppInfo.GetInt(
                "MainForm",
                "fixedpanel_width",
                -1);
            // �״δ򿪴���
            if (nFixedWidth == -1)
                nFixedWidth = this.Width / 3;

            if (strDock == "bottom")
            {
                this.panel_fixed.Dock = DockStyle.Bottom;
                this.panel_fixed.Size = new Size(this.panel_fixed.Width,
                    nFixedHeight);
            }
            else if (strDock == "top")
            {
                this.panel_fixed.Dock = DockStyle.Top;
                this.panel_fixed.Size = new Size(this.panel_fixed.Width,
                    nFixedHeight);
            }
            else if (strDock == "left")
            {
                this.panel_fixed.Dock = DockStyle.Left;
                this.panel_fixed.Size = new Size(nFixedWidth,
                    this.panel_fixed.Size.Height);
            }
            else if (strDock == "right")
            {
                this.panel_fixed.Dock = DockStyle.Right;
                this.panel_fixed.Size = new Size(nFixedWidth,
                    this.panel_fixed.Size.Height);
            }

            this.splitter_fixed.Dock = this.panel_fixed.Dock;

            bool bHide = this.AppInfo.GetBoolean(
                "MainForm",
                "hide_fixed_panel",
                false);
            if (bHide == true)
            {
                this.panel_fixed.Visible = false;
                this.splitter_fixed.Visible = false;
            }

            try
            {
                this.tabControl_panelFixed.SelectedIndex = this.AppInfo.GetInt(
                    "MainForm",
                    "active_fixed_panel_page",
                    0);
            }
            catch
            {
            }
        }

        void FinishFixedPanel()
        {
            string strDock = "right";
            if (this.panel_fixed.Dock == DockStyle.Bottom)
                strDock = "bottom";
            else if (this.panel_fixed.Dock == DockStyle.Left)
                strDock = "left";
            else if (this.panel_fixed.Dock == DockStyle.Right)
                strDock = "right";
            else if (this.panel_fixed.Dock == DockStyle.Top)
                strDock = "top";

            this.AppInfo.SetString(
                "MainForm",
                "fixedpanel_dock",
                strDock);
            this.AppInfo.SetInt(
                "MainForm",
                "fixedpanel_height",
                this.panel_fixed.Size.Height);
            this.AppInfo.SetInt(
                "MainForm",
                "fixedpanel_width",
                this.panel_fixed.Size.Width);

            this.AppInfo.SetInt(
    "MainForm",
    "active_fixed_panel_page",
    this.tabControl_panelFixed.SelectedIndex);
        }

        /*
        // ����һ�����ھ���ǲ���MDI�Ӵ��ڵľ����
        // ����ǣ��򷵻ظ�MDI�Ӵ��ڵ�Form����
        Form IsMdiChildren(IntPtr hwnd)
        {
            for (int i = 0; i < this.MdiChildren.Length; i++)
            {
                if (hwnd == this.MdiChildren[i].Handle)
                {
                    return this.MdiChildren[i];
                }
            }
            return null;    // not found
        }

        // ����һ����ʾ��ǰ�򿪵�MDI�Ӵ��ڵ��ַ���
        string GetOpenedMdiWindowString()
        {
            if (this.ActiveMdiChild == null)
                return null;

            // �õ������MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return null;

            // �ҵ���ײ����Ӵ���
            IntPtr hwndFirst = API.GetWindow(hwnd, API.GW_HWNDLAST);

            // ˳�εõ��ַ���
            string strResult = "";
            hwnd = hwndFirst;
            for (; ; )
            {
                if (hwnd == IntPtr.Zero)
                    break;

                Form temp = IsMdiChildren(hwnd);
                if (temp != null)
                {
                    // ��С���ı�����
                    if (temp.WindowState != FormWindowState.Minimized)
                        strResult += temp.GetType().ToString() + ",";
                }

                hwnd = API.GetWindow(hwnd, API.GW_HWNDPREV);
            }

            return strResult;
        }
         * */

        public void SetMenuItemState()
        {
            // �˵�
            this.MenuItem_saveOriginRecordToIso2709.Enabled = false;
            this.MenuItem_saveOriginRecordToWorksheet.Enabled = false;
            this.MenuItem_font.Enabled = false;
            this.MenuItem_saveToTemplate.Enabled = false;
            this.MenuItem_viewAccessPoint.Enabled = false;

            /*
            // ��������ť
            this.ToolStripMenuItem_loadReaderInfo.Enabled = true;
            this.ToolStripMenuItem_loadReaderInfo.Enabled = true;
             * */
            this.toolButton_nextBatch.Enabled = false;
            this.toolButton_prev.Enabled = false;
            this.toolButton_next.Enabled = false;

            this.toolButton_getAllRecords.Enabled = false;
            this.toolButton_saveTo.Enabled = false;
            this.toolButton_saveToDB.Enabled = false;
            this.toolButton_save.Enabled = false;
            this.toolButton_delete.Enabled = false;

            this.toolButton_refresh.Enabled = false;
            this.toolButton_loadTemplate.Enabled = false;

            this.toolButton_dup.Enabled = false;
            this.toolButton_verify.Enabled = false;
            this.toolButton_loadFullRecord.Enabled = false;
        }



        // װ�ؼ���;����Ϣ
        int LoadFroms(string strFileName,
            out string strError)
        {
            strError = "";
            this.Froms = new FromCollection();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "װ���ļ� " +strFileName+ " ��XMLDOMʱ����: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                Bib1Use from = new Bib1Use();
                from.Name = DomUtil.GetAttr(node, "name");
                from.UniName = DomUtil.GetAttr(node, "uni_name");
                from.Value = DomUtil.GetAttr(node, "value");
                from.Comment = DomUtil.GetAttr(node, "comment");

                this.Froms.Add(from);
            }


            return 0;
        }

        // ��ü���;���б�
        public string[] GetFromList()
        {
            string[] result = new string[this.Froms.Count];

            for (int i = 0; i < this.Froms.Count; i++)
            {
                Bib1Use from = this.Froms[i];
                result[i] = from.Name + " - " + from.Comment;
            }

            return result;
        }

        // ����ָ���λ��
        public void SaveSplitterPos(SplitContainer container,
            string strSection,
            string strEntry)
        {
            float fValue = (float)container.SplitterDistance /
                (
                container.Orientation == Orientation.Horizontal ?
                (float)container.Height
                :
                (float)container.Width
                )
                ;
            this.AppInfo.SetFloat(
                strSection,
                strEntry,
                fValue);

        }

        // ��ò����÷ָ���λ��
        public void LoadSplitterPos(SplitContainer container,
            string strSection,
            string strEntry)
        {
            float fValue = this.AppInfo.GetFloat(
                strSection,
                strEntry,
                (float)0);
            if (fValue == 0)
                return;

            try
            {
                container.SplitterDistance = (int)Math.Ceiling(
                (
                container.Orientation == Orientation.Horizontal ?
                (float)container.Height
                :
                (float)container.Width
                )
                * fValue);
            }
            catch
            {
            }
        }

        #region ��ť�¼�

        // ����
        private void toolButton_search_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm zsearchform = (ZSearchForm)this.ActiveMdiChild;
                    // zsearchform.DoSearchOneServer();
                    zsearchform.DoSearch();
                }
                else if (this.ActiveMdiChild is DtlpSearchForm)
                {
                    DtlpSearchForm dtlpsearchform = (DtlpSearchForm)this.ActiveMdiChild;
                    dtlpsearchform.DoSearch();
                }
                else if (this.ActiveMdiChild is dp2SearchForm)
                {
                    dp2SearchForm dp2searchform = (dp2SearchForm)this.ActiveMdiChild;
                    dp2searchform.DoSearch();
                }
                else if (this.ActiveMdiChild is dp2DupForm)
                {
                    dp2DupForm dupform = (dp2DupForm)this.ActiveMdiChild;
                    dupform.DoSearchDup();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    AmazonSearchForm form = (AmazonSearchForm)this.ActiveMdiChild;
                    form.DoSearch();
                }

            }
            finally
            {
                save.RestoreAll();
            }


        }

        private void toolButton_stop_Click(object sender, EventArgs e)
        {
            stopManager.DoStopActive();

        }

        private void toolButton_prev_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("prev");
                    else
                        detail.LoadRecord("prev");
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("prev");
                    else
                        detail.LoadRecord("prev");
                }
                else if (this.ActiveMdiChild is XmlDetailForm)
                {
                    XmlDetailForm detail = (XmlDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        // detail.LoadRecordByPath("next");
                        detail.LoadRecord("prev");
                    }
                    else
                        detail.LoadRecord("prev");
                }
            }
            finally
            {
                save.RestoreAll();
            }

        }

        private void toolButton_next_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("next");
                    else
                        detail.LoadRecord("next");
                }
                else if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                        detail.LoadRecordByPath("next");
                    else
                        detail.LoadRecord("next");
                }
                else if (this.ActiveMdiChild is XmlDetailForm)
                {
                    XmlDetailForm detail = (XmlDetailForm)this.ActiveMdiChild;
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        // detail.LoadRecordByPath("next");
                        detail.LoadRecord("next");
                    }
                    else
                        detail.LoadRecord("next");
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // ���»�õ�ǰ��¼����Ӧ��reload����
        private void toolButton_refresh_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.Reload();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.Reload();
                }
                if (this.ActiveMdiChild is dp2SearchForm)
                {
                    dp2SearchForm detail = (dp2SearchForm)this.ActiveMdiChild;
                    detail.Reload();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_loadFullRecord_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.LoadRecord("current", true);
                }
                else if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                    search.ReloadFullElementSet();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    var search = this.ActiveMdiChild as AmazonSearchForm;
                    search.ReloadFullElementSet();
                }
                else if (this.ActiveMdiChild is XmlDetailForm)
                {
                    var detail = this.ActiveMdiChild as XmlDetailForm;
                    detail.LoadRecord("current", true);
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_nextBatch_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;

                    search.NextBatch();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    AmazonSearchForm search = (AmazonSearchForm)this.ActiveMdiChild;

                    search.NextBatch();
                }
            }
            finally
            {
                save.RestoreAll();
            }

        }

        private void toolButton_getAllRecords_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                    search.GetAllRecords();
                }
                else if (this.ActiveMdiChild is AmazonSearchForm)
                {
                    AmazonSearchForm search = (AmazonSearchForm)this.ActiveMdiChild;
                    search.GetAllRecords();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_saveTo_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {
                MenuItem_saveOriginRecordToIso2709_Click(this, null);
            }
            finally
            {
                save.RestoreAll();
            } 
        }

        private void toolButton_save_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.SaveRecord();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.SaveRecord();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // ��浽���ݿ�
        private void toolButton_saveToDB_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.SaveRecord("saveas");
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.SaveRecord();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // װ�ؼ�¼ģ��
        private void toolButton_loadTemplate_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.LoadTemplate();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.LoadTemplate();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        // ���浽��¼ģ��
        private void MenuItem_saveToTemplate_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.SaveToTemplate();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.SaveToTemplate();
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void MenuItem_viewAccessPoint_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.GetAccessPoint();
                }

            }
            finally
            {
                save.RestoreAll();
            }
        }

        private void toolButton_delete_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                    detail.DeleteRecord();
                }
                if (this.ActiveMdiChild is DcForm)
                {
                    DcForm detail = (DcForm)this.ActiveMdiChild;
                    detail.DeleteRecord();
                }
                if (this.ActiveMdiChild is dp2SearchForm)
                {
                    dp2SearchForm detail = (dp2SearchForm)this.ActiveMdiChild;
                    detail.DeleteSelectedRecords();
                }
            }
            finally
            {
                save.RestoreAll();
            }

        }

        #endregion


        // ��ǰ�����SearchForm
        public ZSearchForm TopZSearchForm
        {
            get
            {
                return (ZSearchForm)GetTopChildWindow(typeof(ZSearchForm));
            }
        }

        // ��ǰ�����DtlpSearchForm
        public DtlpSearchForm TopDtlpSearchForm
        {
            get
            {
                return (DtlpSearchForm)GetTopChildWindow(typeof(DtlpSearchForm));
            }

        }

        // ��ǰ����� dp2SearchForm
        public dp2SearchForm TopDp2SearchForm
        {
            get
            {
                return (dp2SearchForm)GetTopChildWindow(typeof(dp2SearchForm));
            }
        }

        // ��ǰ����� AmazonSearchForm
        public AmazonSearchForm TopAmazonSearchForm
        {
            get
            {
                return (AmazonSearchForm)GetTopChildWindow(typeof(AmazonSearchForm));
            }
        }

        // ��ǰ�����MarcDetailForm
        public MarcDetailForm TopMarcDetailForm
        {
            get
            {
                return (MarcDetailForm)GetTopChildWindow(typeof(MarcDetailForm));
            }
        }


        // ��ǰ�����DcForm
        public DcForm TopDcForm
        {
            get
            {
                return (DcForm)GetTopChildWindow(typeof(DcForm));
            }
        }

        // �õ��ض����͵Ķ���MDI����
        Form GetTopChildWindow(Type type)
        {
            if (ActiveMdiChild == null)
                return null;

            // �õ������MDI Child
            IntPtr hwnd = this.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return null;

            for (; ; )
            {
                if (hwnd == IntPtr.Zero)
                    break;

                Form child = null;
                for (int j = 0; j < this.MdiChildren.Length; j++)
                {
                    if (hwnd == this.MdiChildren[j].Handle)
                    {
                        child = this.MdiChildren[j];
                        goto FOUND;
                    }
                }

                goto CONTINUE;
            FOUND:

                if (child.GetType().Equals(type) == true)
                    return child;

            CONTINUE:
                hwnd = API.GetWindow(hwnd, API.GW_HWNDNEXT);
            }

            return null;
        }

        #region �˵��¼�

        // ��Z39.50������
        private void MenuItem_openZSearchForm_Click(object sender, EventArgs e)
        {
            ZSearchForm form = new ZSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // ��MARC��¼��
        private void MenuItem_openMarcDetailForm_Click(object sender, EventArgs e)
        {
            MarcDetailForm form = new MarcDetailForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // ����ģ��
        private void MenuItem_openMarcDetailFormEx_Click(object sender, EventArgs e)
        {
            MarcDetailForm form = new MarcDetailForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
            form.LoadTemplate();
        }

        // ��XML��¼��
        private void MenuItem_loadXmlDetailForm_Click(object sender, EventArgs e)
        {
            XmlDetailForm form = new XmlDetailForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        private void MenuItem_openBerDebugForm_Click(object sender, EventArgs e)
        {
            BerDebugForm form = new BerDebugForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();

        }

        private void MenuItem_saveOriginRecordToWorksheet_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                detail.SaveRecordToWorksheet();
            }
            else if (this.ActiveMdiChild is ZSearchForm)
            {
                ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                search.menuItem_saveOriginRecordToWorksheet_Click(this, null);
            }
            else if (this.ActiveMdiChild is dp2SearchForm)
            {
                dp2SearchForm search = (dp2SearchForm)this.ActiveMdiChild;
                search.SaveOriginRecordToWorksheet();
            }
        }

        private void MenuItem_saveOriginRecordToIso2709_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                detail.SaveOriginRecordToIso2709();
            }
            else if (this.ActiveMdiChild is ZSearchForm)
            {
                ZSearchForm search = (ZSearchForm)this.ActiveMdiChild;
                search.menuItem_saveOriginRecordToIso2709_Click(this, null);
            }
            else if (this.ActiveMdiChild is dp2SearchForm)
            {
                dp2SearchForm search = (dp2SearchForm)this.ActiveMdiChild;
                search.SaveOriginRecordToIso2709();
            }
            
        }

        private void MenuItem_openZhongcihaoForm_Click(object sender, EventArgs e)
        {
            ZhongcihaoForm form = new ZhongcihaoForm();

            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // �й�MDI�Ӵ������еĲ˵�����
        private void MenuItem_mdi_arrange_Click(object sender, System.EventArgs e)
        {
            // ƽ�� ˮƽ��ʽ
            if (sender == MenuItem_tileHorizontal)
                this.LayoutMdi(MdiLayout.TileHorizontal);

            if (sender == MenuItem_tileVertical)
                this.LayoutMdi(MdiLayout.TileVertical);

            if (sender == MenuItem_cascade)
                this.LayoutMdi(MdiLayout.Cascade);

            if (sender == MenuItem_arrangeIcons)
                this.LayoutMdi(MdiLayout.ArrangeIcons);

        }

        // ��Ȩ
        private void MenuItem_copyright_Click(object sender, EventArgs e)
        {
            CopyrightDlg dlg = new CopyrightDlg();
            GuiUtil.SetControlFont(dlg, this.DefaultFont);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

        }

        // DtlpЭ�������
        private void MenuItem_openDtlpSearchForm_Click(object sender, EventArgs e)
        {
            DtlpSearchForm form = new DtlpSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // dp2libraryЭ�������
        private void MenuItem_openDp2SearchForm_Click(object sender, EventArgs e)
        {
#if NO
            if (this.TestMode == true)
            {
                MessageBox.Show(this, "dp2 ��������Ҫ���������к�(��ʽģʽ)����ʹ��");
                return;
            }
#endif


#if SN
            // ������к�
            // DateTime start_day = new DateTime(2014, 11, 15);    // 2014/11/15 �Ժ�ǿ���������кŹ���
            // if (DateTime.Now >= start_day || this.IsExistsSerialNumberStatusFile() == true)
            {
                // ���û�Ŀ¼��д��һ�������ļ�����ʾ���кŹ����Ѿ�����
                this.WriteSerialNumberStatusFile();

                string strError = "";
                int nRet = this.VerifySerialCode("dp2 ��������Ҫ���������кŲ���ʹ��",
                    "",
                    false,
                    out strError);
                if (nRet == -1)
                {
#if NO
                    MessageBox.Show(this, "dp2 ��������Ҫ���������кŲ���ʹ��");
                    return;
#endif
                }
            }

#endif

            dp2SearchForm form = new dp2SearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // OAI-PMHЭ�������
        private void MenuItem_openOaiSearchForm_Click(object sender, EventArgs e)
        {
            OaiSearchForm form = new OaiSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();

        }


        private void MenuItem_cfg_Click(object sender, EventArgs e)
        {
            string strOldDefaultFontString = this.DefaultFontString;

            SystemCfgForm dlg = new SystemCfgForm();
            GuiUtil.SetControlFont(dlg, this.DefaultFont);

            dlg.ParamChanged += new ParamChangedEventHandler(CfgDlg_ParamChanged);
            dlg.MainForm = this;
            // dlg.StartPosition = FormStartPosition.CenterScreen;

            this.AppInfo.LinkFormState(dlg,
                "systemcfg_dialog_state");

            dlg.ShowDialog(this);

            this.AppInfo.UnlinkFormState(dlg);
            dlg.ParamChanged -= new ParamChangedEventHandler(CfgDlg_ParamChanged);

            // ȱʡ���巢���˱仯
            if (strOldDefaultFontString != this.DefaultFontString)
            {
                Size oldsize = this.Size;

                GuiUtil.SetControlFont(this, this.DefaultFont, true);

                foreach (Form child in this.MdiChildren)
                {
                    oldsize = child.Size;

                    GuiUtil.SetControlFont(child, this.DefaultFont, true);
                }
            }

            // ����ѷ ȱʡ�������仯
            {
                // ������ǰ�򿪵�����AmazonSearchForm
                List<Form> forms = GetChildWindows(typeof(AmazonSearchForm));
                foreach (Form child in forms)
                {
                    AmazonSearchForm form = (AmazonSearchForm)child;
                    // �ð�ť������ʾ����
                    form.RefreshUI();
                }
            }
        }

        void CfgDlg_ParamChanged(object sender, ParamChangedEventArgs e)
        {
            if (e.Section == "dp2searchform"
                && e.Entry == "layout")
            {
                // ������ǰ�򿪵�����dp2SearchForm
                List<Form> forms = GetChildWindows(typeof(dp2SearchForm));
                foreach (Form child in forms)
                {
                    dp2SearchForm form = (dp2SearchForm)child;

                    if (form.SetLayout((string)e.Value) == true)
                        form.AppInfo_LoadMdiSize(form, null);
                }
            }

        }

        // �õ��ض����͵�MDI����
        List<Form> GetChildWindows(Type type)
        {
            List<Form> results = new List<Form>();

            foreach (Form child in this.MdiChildren)
            {
                if (child.GetType().Equals(type) == true)
                    results.Add(child);
            }

            return results;
        }

        private void MenuItem_openDtlpLogForm_Click(object sender, EventArgs e)
        {
            DtlpLogForm form = new DtlpLogForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        private void MenuItem_openEaccForm_Click(object sender, EventArgs e)
        {
            EaccForm form = new EaccForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();

        }

        // ��DC��¼��
        private void MenuItem_openDcForm_Click(object sender, EventArgs e)
        {
            DcForm form = new DcForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // ��DC��¼�� ��ģ��
        private void MenuItem_openDcFormEx_Click(object sender, EventArgs e)
        {
            DcForm form = new DcForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
            form.LoadTemplate();
        }

        // ������Ŀ¼�ļ���
        private void MenuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void MenuItem_font_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;

                detail.SetFont();
            }
            if (this.ActiveMdiChild is DcForm)
            {
                DcForm detail = (DcForm)this.ActiveMdiChild;

                detail.SetFont();
            }

        }

        private void MenuItem_openTestForm_Click(object sender, EventArgs e)
        {
            TestForm form = new TestForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        #endregion


        public ToolStripProgressBar ToolStripProgressBar
        {
            get
            {
                return this.toolStripProgressBar_main;
            }
        }

        // ��ֹ����ֹͣ��ť�����������ť��
        // ���Ҫ�ָ�����EanbleStateCollection��RestoreAll()������
        public EnableStateCollection DisableToolButtons()
        {
            // ����ԭ����״̬
            EnableStateCollection results = new EnableStateCollection();

            results.Push(this.toolButton_getAllRecords);

            results.Push(this.toolButton_next);
            results.Push(this.toolButton_nextBatch);
            results.Push(this.toolButton_prev);
            results.Push(this.toolButton_save);
            results.Push(this.toolButton_saveTo);
            results.Push(this.toolButton_saveToDB);
            results.Push(this.toolButton_search);
            results.Push(this.toolButton_dup);
            results.Push(this.toolButton_verify);

            // this.toolButton_stop
            return results;
        }

        // ���Encoding���󡣱�����֧��MARC-8������
        // return:
        //      -1  error
        //      0   succeed
        public int GetEncoding(string strName,
            out Encoding encoding,
            out string strError)
        {
            strError = "";
            encoding = null;

            try
            {

                if (StringUtil.IsNumber(strName) == true)
                {
                    try
                    {
                        Int32 nCodePage = Convert.ToInt32(strName);
                        encoding = Encoding.GetEncoding(nCodePage);
                    }
                    catch (Exception ex)
                    {
                        strError = "������뷽ʽ���̳���: " + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    if (strName.ToLower() == "eacc"
                        || strName.ToLower() == "marc-8")
                        encoding = this.Marc8Encoding;
                    else
                        encoding = Encoding.GetEncoding(strName);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return 0;
        }


        public string MessageText
        {
            get
            {
                return this.toolStripStatusLabel_main.Text;
            }
            set
            {
                this.toolStripStatusLabel_main.Text = value;
            }
        }

        // ����������ȱʡ�ʻ�����
        public void ManageServers(bool bFirstRun)
        {
            ServersDlg dlg = new ServersDlg();
            GuiUtil.SetControlFont(dlg, this.DefaultFont);

            dp2ServerCollection newServers = Servers.Dup();

            if (bFirstRun == true)
            {
                dlg.Text = "�״�����: ���� dp2library ������Ŀ��";
                dlg.FirstRun = true;
            }
            dlg.Servers = newServers;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // this.Servers = newServers;
            this.Servers.Import(newServers);
        }

        // ���������ļ�
        public int DownloadDataFile(string strFileName,
            out string strError)
        {
            strError = "";

            string strUrl = "http://dp2003.com/dp2Catalog/" + strFileName;
            string strLocalFileName = this.DataDir + "\\" + strFileName;
            string strTempFileName = this.DataDir + "\\~temp_download_webfile";

            int nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
                strUrl,
                strLocalFileName,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;
            strError = "����" + strFileName + "�ļ��ɹ� :\r\n" + strUrl + " --> " + strLocalFileName;
            return 0;
        }

        // 2015/5/8
        // �����û��ļ�
        public int DownloadUserFile(string strFileName,
            out string strError)
        {
            strError = "";

            string strUrl = "http://dp2003.com/dp2Catalog/" + strFileName;
            string strLocalFileName = Path.Combine(this.UserDir, strFileName);
            string strTempFileName = Path.Combine(this.UserDir, "~temp_download_webfile");

            int nRet = WebFileDownloadDialog.DownloadWebFile(
                this,
                strUrl,
                strLocalFileName,
                strTempFileName,
                out strError);
            if (nRet == -1)
                return -1;
            strError = "����" + strFileName + "�ļ��ɹ� :\r\n" + strUrl + " --> " + strLocalFileName;
            return 0;
        }


        public int LoadQuickSjhm(bool bAutoDownload,
out string strError)
        {
            strError = "";

            // �Ż�
            if (this.QuickSjhm != null)
                return 0;

        REDO:

            try
            {
                this.QuickSjhm = new QuickSjhm(this.DataDir + "\\sjhm.xml");
            }
            catch (FileNotFoundException ex)
            {
                strError = "װ�ر����ĽǺ����ļ��������� :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("sjhm.xml",
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n�Զ������ļ���\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "װ�ر����ĽǺ����ļ��������� :" + ex.Message;
                return -1;
            }

            return 1;
        }


        public int LoadQuickPinyin(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            // �Ż�
            if (this.QuickPinyin != null)
                return 0;

        REDO:

            try
            {
                this.QuickPinyin = new QuickPinyin(this.DataDir + "\\pinyin.xml");
            }
            catch (FileNotFoundException ex)
            {
                strError = "װ�ر���ƴ���ļ��������� :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("pinyin.xml",
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n�Զ������ļ���\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "װ�ر���ƴ���ļ��������� :" + ex.Message;
                return -1;
            }

            return 1;
        }

        public int LoadIsbnSplitter(bool bAutoDownload,
    out string strError)
        {
            strError = "";

            // �Ż�
            if (this.IsbnSplitter != null)
                return 0;

        REDO:

            try
            {
                this.IsbnSplitter = new IsbnSplitter(this.DataDir + "\\rangemessage.xml");  // "\\isbn.xml"
            }
            catch (FileNotFoundException ex)
            {
                strError = "װ�ر��� isbn �����ļ� rangemessage.xml �������� :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("rangemessage.xml",    // "isbn.xml"
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n�Զ������ļ���\r\n" + strError1;
                        return -1;
                    }
                    goto REDO;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "װ�ر��� isbn �����ļ��������� :" + ex.Message;
                return -1;
            }

            return 1;
        }

        // (C#�ű�ʹ��)
        // ��ISBN����ȡ�ó�����Ų���
        // �����������Զ���Ӧ��978ǰ׺������ISBN��
        // ISBN�����޺��ʱ���������Զ��ȼӺ��Ȼ����ȡ�ó������
        // parameters:
        //      strPublisherNumber  ��������롣������978-����
        public int GetPublisherNumber(string strISBN,
            out string strPublisherNumber,
            out string strError)
        {
            strPublisherNumber = "";
            strError = "";

            int nRet = strISBN.IndexOf("-");
            if (nRet == -1)
            {

                nRet = this.LoadIsbnSplitter(true, out strError);
                if (nRet == -1)
                {
                    strError = "��ȡ�������ǰ������ISBN����û�к�ܣ��ڼ����ܵĹ����У����ִ���: " + strError;
                    return -1;
                }

                string strResult = "";

                nRet = this.IsbnSplitter.IsbnInsertHyphen(strISBN,
                    "force10",  // ���ڳ���������ISBN��������978ǰ׺
                    out strResult,
                    out strError);
                if (nRet == -1)
                {
                    strError = "��ȡ�������ǰ������ISBN����û�к�ܣ��ڼ����ܵĹ����У����ִ���: " + strError;
                    return -1;
                }

                strISBN = strResult;
            }

            return Global.GetPublisherNumber(strISBN,
                out strPublisherNumber,
                out strError);
        }

        // ����
        private void toolButton_dup_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is MarcDetailForm)
                {
                    MarcDetailForm marcform = (MarcDetailForm)this.ActiveMdiChild;
                    marcform.SearchDup("toolbar");
                }
                else if (this.ActiveMdiChild is DcForm)
                {
                    DcForm dcform = (DcForm)this.ActiveMdiChild;
                    dcform.SearchDup("toolbar");
                }
            }
            finally
            {
                save.RestoreAll();
            }
        }


        private void toolButton_verify_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                MarcDetailForm marcform = (MarcDetailForm)this.ActiveMdiChild;
                marcform.VerifyData();
            }
            else if (this.ActiveMdiChild is DcForm)
            {
                DcForm dcform = (DcForm)this.ActiveMdiChild;
                // dcform.VerifyData();
            }
        }
        

        // ����MARC�ļ�
        private void MenuItem_linkMarcFile_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is MarcDetailForm)
            {
                // �����ǰ����ΪMARC��¼������ֱ������֮
                MarcDetailForm detail = (MarcDetailForm)this.ActiveMdiChild;
                detail.LinkMarcFile();
            }
            else
            {
                // �����ǰ���ڲ���MARC��¼�������¿�һ��
                MarcDetailForm form = new MarcDetailForm();

                form.MdiParent = this;

                form.MainForm = this;
                form.Show();

                form.LinkMarcFile();
            }
        }

        // ���޸����봰
        private void MenuItem_changePassword_Click(object sender, EventArgs e)
        {
            ChangePasswordForm form = new ChangePasswordForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // ȱʡ��MDI�Ӵ��ڿ��
        public static int DefaultMdiWindowWidth
        {
            get
            {
                return (int)((double)SystemInformation.WorkingArea.Width * 0.8);
            }
        }

        // ȱʡ��MDI�Ӵ��ڸ߶�
        public static int DefaultMdiWindowHeight
        {
            get
            {
                return (int)((double)SystemInformation.WorkingArea.Height * 0.8);
            }
        }

        // ��������ļ����ػ���
        private void MenuItem_clearCfgCache_Click(object sender, EventArgs e)
        {
            cfgCache.ClearCfgCache();

            this.AssemblyCache.Clear(); // ˳��Ҳ���Assembly����
            this.DomCache.Clear();
        }

        public AmazonSearchForm GetAmazonSearchForm()
        {
            AmazonSearchForm searchform = null;

            searchform = this.TopAmazonSearchForm;

            if (searchform == null)
            {
                // �¿�һ�� Amazon ������
                FormWindowState old_state = this.WindowState;

                searchform = new AmazonSearchForm();
                searchform.MainForm = this;
                searchform.MdiParent = this;
                searchform.WindowState = FormWindowState.Minimized;
                searchform.Show();

                // 2008/3/17 
                this.WindowState = old_state;
                this.Activate();

                // ��Ҫ�ȴ���ʼ�������������
                // searchform.WaitLoadFinish();
            }

            return searchform;
        }


        public dp2SearchForm GetDp2SearchForm()
        {
            dp2SearchForm dp2_searchform = null;

            dp2_searchform = this.TopDp2SearchForm;

            if (dp2_searchform == null)
            {
                // �¿�һ��dp2������
                FormWindowState old_state = this.WindowState;

                dp2_searchform = new dp2SearchForm();
                dp2_searchform.MainForm = this;
                dp2_searchform.MdiParent = this;
                dp2_searchform.WindowState = FormWindowState.Minimized;
                dp2_searchform.Show();

                // 2008/3/17 
                this.WindowState = old_state;
                this.Activate();

                // ��Ҫ�ȴ���ʼ�������������
                dp2_searchform.WaitLoadFinish();
            }

            return dp2_searchform;
        }

        public DtlpSearchForm GetDtlpSearchForm()
        {
            DtlpSearchForm dtlp_searchform = this.TopDtlpSearchForm;

            dtlp_searchform = this.TopDtlpSearchForm;

            if (dtlp_searchform == null)
            {
                // �¿�һ��dtlp������
                FormWindowState old_state = this.WindowState;

                dtlp_searchform = new DtlpSearchForm();
                dtlp_searchform.MainForm = this;
                dtlp_searchform.MdiParent = this;
                dtlp_searchform.WindowState = FormWindowState.Minimized;
                dtlp_searchform.Show();

                this.WindowState = old_state;
                this.Activate();

                // ��Ҫ�ȴ���ʼ�������������
                dtlp_searchform.WaitLoadFinish();

            }

            return dtlp_searchform;
        }

        public string DefaultSearchDupStartPath
        {
            get
            {
                // ��ѡ�����·��
                return this.AppInfo.GetString(
                    "searchdup",
                    "defaultStartPath",
                    "");
            }
            set
            {
                this.AppInfo.SetString(
                    "searchdup",
                    "defaultStartPath",
                    value);
            }
        }

        public string StatusBarMessage
        {
            get
            {
                return toolStripStatusLabel_main.Text;
            }
            set
            {
                toolStripStatusLabel_main.Text = value;
            }
        }

        private void toolStripButton_testSearch_Click(object sender, EventArgs e)
        {
            EnableStateCollection save = DisableToolButtons();
            try
            {

                if (this.ActiveMdiChild is ZSearchForm)
                {
                    ZSearchForm zsearchform = (ZSearchForm)this.ActiveMdiChild;
                    zsearchform.DoTestSearch();
                }
                else if (this.ActiveMdiChild is DtlpSearchForm)
                {

                }
                else if (this.ActiveMdiChild is dp2SearchForm)
                {
                }
                else if (this.ActiveMdiChild is dp2DupForm)
                {
                }

            }
            finally
            {
                save.RestoreAll();
            }
        }

        // ��ǰ�Ƿ�߱���ʾitem property������
        public bool CanDisplayItemProperty()
        {
            if (this.PanelFixedVisible == false)
                return false;
            if (this.tabControl_panelFixed.SelectedTab != this.tabPage_property)
                return false;

            return true;
        }

        public void ActivatePropertyPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
        }

        public Control CurrentPropertyControl
        {
            get
            {
                if (this.tabPage_property.Controls.Count == 0)
                    return null;
                return this.tabPage_property.Controls[0];
            }
            set
            {
                // ���ԭ�пؼ�
                while (this.tabPage_property.Controls.Count > 0)
                    this.tabPage_property.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_property.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_property;
                }
            }
        }

        public void ActivateVerifyResultPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
        }

        public Control CurrentVerifyResultControl
        {
            get
            {
                if (this.tabPage_verifyResult.Controls.Count == 0)
                    return null;
                return this.tabPage_verifyResult.Controls[0];
            }
            set
            {
                // ���ԭ�пؼ�
                while (this.tabPage_verifyResult.Controls.Count > 0)
                    this.tabPage_verifyResult.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_verifyResult.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_verifyResult;
                }
            }
        }

        public void ActivateGenerateDataPage()
        {
            this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;
        }

        // 
        public Control CurrentGenerateDataControl
        {
            get
            {
                if (this.tabPage_generateData.Controls.Count == 0)
                    return null;
                return this.tabPage_generateData.Controls[0];
            }
            set
            {
                // ���ԭ�пؼ�
                while (this.tabPage_generateData.Controls.Count > 0)
                    this.tabPage_generateData.Controls.RemoveAt(0);

                if (value != null)
                {
                    this.tabPage_generateData.Controls.Add(value);
                    // this.tabControl_panelFixed.SelectedTab = this.tabPage_generateData;
                }
            }
        }


        public bool PanelFixedVisible
        {
            get
            {
                return this.panel_fixed.Visible;
            }
            set
            {
                this.panel_fixed.Visible = value;
                this.splitter_fixed.Visible = value;
            }
        }

        private void toolStripButton_close_Click(object sender, EventArgs e)
        {
            this.PanelFixedVisible = false;
        }

        public string DefaultFontString
        {
            get
            {
                return this.AppInfo.GetString(
                    "Global",
                    "default_font",
                    "");
            }
            set
            {
                this.AppInfo.SetString(
                    "Global",
                    "default_font",
                    value);
            }
        }

        new public Font DefaultFont
        {
            get
            {
                string strDefaultFontString = this.DefaultFontString;
                if (String.IsNullOrEmpty(strDefaultFontString) == true)
                {
                    return GuiUtil.GetDefaultFont();    // 2015/5/8
                    // return null;
                }

                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strDefaultFontString);
            }
        }

        void SetFirstDefaultFont()
        {
            if (this.DefaultFont != null)
                return;

            try
            {
                FontFamily family = new FontFamily("΢���ź�");
            }
            catch
            {
                return;
            }
            this.DefaultFontString = "΢���ź�, 9pt";
        }

#if NO
        // parameters:
        //      bForce  �Ƿ�ǿ�����á�ǿ��������ָDefaultFont == null ��ʱ��ҲҪ����Control.DefaultFont������
        public static void SetControlFont(Control control,
            Font font,
            bool bForce = false)
        {
            if (font == null)
            {
                if (bForce == false)
                    return;
                font = Control.DefaultFont;
            }
            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            { }
            else
                control.Font = font;

            ChangeDifferentFaceFont(control, font);
        }

        static void ChangeDifferentFaceFont(Control parent,
            Font font)
        {
            // �޸������¼��ؼ������壬�����������һ���Ļ�
            foreach (Control sub in parent.Controls)
            {
                Font subfont = sub.Font;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    sub.Font = new Font(font, subfont.Style);
                }

                if (sub is ToolStrip)
                {
                    ChangeDifferentFaceFont((ToolStrip)sub, font);
                }

                // �ݹ�
                ChangeDifferentFaceFont(sub, font);
            }
        }

        static void ChangeDifferentFaceFont(ToolStrip tool,
    Font font)
        {
            // �޸�������������壬�����������һ���Ļ�
            for (int i = 0; i < tool.Items.Count; i++)
            {
                ToolStripItem item = tool.Items[i];

                Font subfont = item.Font;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    item.Font = new Font(font, subfont.Style);
                }
            }
        }
#endif

        /// <summary>
        /// ��ƴ��ʱ�Զ�ѡ�������
        /// </summary>
        public bool AutoSelPinyin
        {
            get
            {
                return this.AppInfo.GetBoolean(
                    "global",
                    "auto_select_pinyin",
                    false);
            }
        }

        // GCATͨ�ú������ߺ���� WebService URL
        public string GcatServerUrl
        {
            get
            {
                return this.AppInfo.GetString("config",
                    "gcat_server_url",
                    "http://dp2003.com/gcatserver/");
            }
        }

        public string PinyinServerUrl
        {
            get
            {
                return this.AppInfo.GetString("config",
                    "pinyin_server_url",
                    "http://dp2003.com/gcatserver/");
            }
        }

        // �Ƿ�Ҫ��ʱ���ñ���ƴ������״̬�˳�ʱ���ᱻ����
        public bool ForceUseLocalPinyinFunc = false;

        private void MenuItem_openProgramFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void MenuItem_openZBatchSearchForm_Click(object sender, EventArgs e)
        {
            ZBatchSearchForm form = new ZBatchSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        private void MenuItem_openAmazonSearchForm_Click(object sender, EventArgs e)
        {
            AmazonSearchForm form = new AmazonSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // wparam == 1����ʾ����ȫ���ܵĳ�ʼ������������ǳ�ʼ�����ֲ���
                case WM_PREPARE:
                    {
                        int nRet = 0;

                        // �Ƚ�ֹ����
                        if (m.WParam.ToInt32() == 1)
                            EnableControls(false);

                        try
                        {
                            string strError = "";

#if NO
                            // ������кš��������ʱ��Ҫ����ֲ�Ʒ����
                            DateTime start_day = new DateTime(2014, 11, 15);    // 2014/11/15 �Ժ�ǿ���������кŹ���
                            if (DateTime.Now >= start_day || IsExistsSerialNumberStatusFile() == true)
                            {
                                // ���û�Ŀ¼��д��һ�������ļ�����ʾ���кŹ����Ѿ�����
                                WriteSerialNumberStatusFile();

                                nRet = this.VerifySerialCode("", out strError);
                                if (nRet == -1)
                                {
                                    MessageBox.Show(this, "dp2Catalog ��Ҫ���������кŲ���ʹ��");
                                    Application.Exit();
                                    return;
                                }
                            }
#endif

                            Stop = new DigitalPlatform.Stop();
                            Stop.Register(stopManager, true);	// ����������
                            Stop.SetMessage("����ɾ����ǰ��������ʱ�ļ�...");

                            DeleteAllTempFiles(this.DataDir);

                            Stop.SetMessage("");
                            if (Stop != null) // �������
                            {
                                Stop.Unregister();	// ����������
                                Stop = null;
                            }

                            // ��ʼ����ʷ���󣬰���C#�ű�
                            if (this.OperHistory == null)
                            {
                                this.OperHistory = new OperHistory();
                                nRet = this.OperHistory.Initial(this,
                                    this.webBrowser_history,
                                    out strError);
                                if (nRet == -1)
                                    MessageBox.Show(this, strError);
                            }
                        }
                        finally
                        {
                            // Ȼ����ɽ���
                            if (m.WParam.ToInt32() == 1)
                                EnableControls(true);

                        }

                        if (m.WParam.ToInt32() == 1)
                        {
                            // �ָ��ϴ������Ĵ���
                            string strOpenedMdiWindow = this.AppInfo.GetString(
                                "main_form",
                                "last_opened_mdi_window",
                                "");

                            RestoreLastOpenedMdiWindow(strOpenedMdiWindow);
                        }
                        return;
                    }

                // break;

            }
            base.DefWndProc(ref m);
        }

        // ɾ������Ŀ¼��ȫ����ʱ�ļ�
        // �����������ʱ�����
        void DeleteAllTempFiles(string strDataDir)
        {
            // ���ÿ���Ȩ
            Application.DoEvents();

            DirectoryInfo di = new DirectoryInfo(strDataDir);

            if (string.IsNullOrEmpty(di.Name) == false
                && di.Name[0] == '~')
            {
                try
                {
                    di.Delete(true);
                }
                catch
                {
                    // goto DELETE_FILES;
                }

                return;
            }

        DELETE_FILES:
            FileInfo[] fis = di.GetFiles();
            for (int i = 0; i < fis.Length; i++)
            {
                string strFileName = fis[i].Name;
                if (strFileName.Length > 0
                    && strFileName[0] == '~')
                {
                    Stop.SetMessage("����ɾ�� " + fis[i].FullName);
                    try
                    {
                        File.Delete(fis[i].FullName);
                    }
                    catch
                    {
                    }
                }
            }

            // �����¼�Ŀ¼���ݹ�
            DirectoryInfo[] dis = di.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                DeleteAllTempFiles(dis[i].FullName);
            }
        }

        void EnableControls(bool bEnable)
        {
            this.menuStrip_main.Enabled = bEnable;
        }

        private void MenuItem_openZBatchSearchForm1_Click(object sender, EventArgs e)
        {
            ZBatchSearchForm form = new ZBatchSearchForm();

            form.MdiParent = this;

            form.MainForm = this;
            form.Show();
        }

        // ���ַ����еĺ���ת��Ϊ�ĽǺ���
        // parameters:
        //      bLocal  �Ƿ�ӱ��ػ�ȡ�ĽǺ���
        // return:
        //      -1  ����
        //      0   �û�ϣ���ж�
        //      1   ����
        public int HanziTextToSjhm(
            bool bLocal,
            string strText,
            out List<string> sjhms,
            out string strError)
        {
            strError = "";
            sjhms = new List<string>();

            // string strSpecialChars = "���������������������������������ۣݡ����������������ܣ�������������";


            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // �����Ƿ��������
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                {
                    continue;
                }

                // ����
                string strHanzi = "";
                strHanzi += ch;


                string strResultSjhm = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.LoadQuickSjhm(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.QuickSjhm.GetSjhm(
                        strHanzi,
                        out strResultSjhm,
                        out strError);
                }
                else
                {
                    throw new Exception("�ݲ�֧�ִ�ƴ�����л�ȡ�ĽǺ���");
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

            return 1;   // ��������
        }

#if NO
        // ��װ��汾
        public int GetPinyin(IWin32Window owner,
    string strHanzi,
    PinyinStyle style,
    bool bAutoSel,
    out string strPinyin,
    out string strError)
        {
            return GetPinyin(owner,
                strHanzi,
                style,
                (bAutoSel ? "auto" : ""),
                out strPinyin,
                out strError);
        }
#endif

        // �����ַ���ת��Ϊƴ��
        // ��������ᰴ�յ�ǰ���ã��Զ�����ʹ���²�ļ�ƴ������
        // return:
        //      -1  ����
        //      0   �û�ϣ���ж�
        //      1   ����
        /// <summary>
        /// �����ַ���ת��Ϊƴ��
        /// </summary>
        /// <param name="owner">���ں����� MessageBox �ͶԻ��� ����������</param>
        /// <param name="strHanzi">�����ַ���</param>
        /// <param name="style">ת��Ϊƴ���ķ��</param>
        /// <param name="bAutoSel">�Ƿ��Զ�ѡ�������</param>
        /// <param name="strPinyin">����ƴ���ַ���</param>
        /// <param name="strError">���س�����Ϣ</param>
        /// <returns>-1: ����; 0: �û�ϣ���ж�; 1: ����; 2: ����ַ�������û���ҵ�ƴ���ĺ���</returns>
        public int GetPinyin(IWin32Window owner,
            string strHanzi,
            PinyinStyle style,
            string strDuoyinStyle,  // bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            // return:
            //      -1  ����
            //      0   �û�ϣ���ж�
            //      1   ����
            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
               || this.ForceUseLocalPinyinFunc == true)
            {
                return this.HanziTextToPinyin(
                    owner,
                    true,	// ���أ�����
                    strHanzi,
                    style,
                    strDuoyinStyle,
                    out strPinyin,
                    out strError);
            }
            else
            {
                // �����ַ���ת��Ϊƴ��
                // ����������Ѿ�MessageBox������strError��һ�ַ���Ϊ�ո�
                // return:
                //      -1  ����
                //      0   �û�ϣ���ж�
                //      1   ����
                return this.SmartHanziTextToPinyin(
                    owner,
                    strHanzi,
                    style,
                    strDuoyinStyle,
                    out strPinyin,
                    out strError);
            }
        }

        // ���ַ����еĺ��ֺ�ƴ������
        // parameters:
        //      bLocal  �Ƿ�ӱ��ػ�ȡƴ��
        // return:
        //      -1  ����
        //      0   �û�ϣ���ж�
        //      1   ����
        public int HanziTextToPinyin(
            IWin32Window owner,
            bool bLocal,
            string strText,
            PinyinStyle style,
            string strDuoyinStyle,  // 2014/10/20
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            bool bAuto = StringUtil.IsInList("auto", strDuoyinStyle);
            bool bFirst = StringUtil.IsInList("first", strDuoyinStyle);

            // string strSpecialChars = "���������������������������������ۣݡ����������������ܣ�������������";

            string strHanzi;
            int nStatus = -1;	// ǰ��һ���ַ������� -1:ǰ��û���ַ� 0:��ͨӢ����ĸ 1:�ո� 2:����

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
                {	// ����
                    strHanzi += ch;
                }

                // ����ǰ�������Ӣ�Ļ��ߺ��֣��м����ո�
                if (nStatus == 2 || nStatus == 0)
                    strPinyin += " ";


                // �����Ƿ��������
                if (StringUtil.SpecialChars.IndexOf(strHanzi) != -1)
                {
                    strPinyin += strHanzi;	// ���ڱ�Ӧ��ƴ����λ��
                    nStatus = 2;
                    continue;
                }


                // ���ƴ��
                string strResultPinyin = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.LoadQuickPinyin(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.QuickPinyin.GetPinyin(
                        strHanzi,
                        out strResultPinyin,
                        out strError);
                }
                else
                {
                    throw new Exception("�ݲ�֧�ִ�ƴ�����л�ȡƴ��");
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
                    strPinyin += strHanzi;	// ֻ�ý����ַ��ڱ�Ӧ��ƴ����λ��
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// ����Ƕ��ƴ��
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    // GuiUtil.SetControlFont(dlg, this.Font);
                    float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                    GuiUtil.SetControlFont(dlg, this.Font, false);
                    // ά�������ԭ�д�С������ϵ
                    dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                    dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);

                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    if (bFirst == true
    && string.IsNullOrEmpty(dlg.Pinyins) == false)
                    {
                        dlg.ResultPinyin = SelPinyinDlg.GetFirstPinyin(dlg.Pinyins);
                        dlg.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                        dlg.ShowDialog(owner);

                        this.AppInfo.UnlinkFormState(dlg);
                    }

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "�ƶ�");

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
                        return 0;   // �û�ϣ�������ж�
                    }
                    else
                    {
                        Debug.Assert(false, "SelPinyinDlg����ʱ���������DialogResultֵ");
                    }
                }
                else
                {
                    // ����ƴ��

                    strPinyin += SelPinyinDlg.ConvertSinglePinyinByStyle(
                        strResultPinyin,
                        style);
                }
                nStatus = 2;
            }

            return 1;   // ��������
        }

        GcatServiceClient m_gcatClient = null;
        string m_strPinyinGcatID = "";
        bool m_bSavePinyinGcatID = false;

        // ��װ��İ汾
        public int SmartHanziTextToPinyin(
    IWin32Window owner,
    string strText,
    PinyinStyle style,
    bool bAutoSel,
    out string strPinyin,
    out string strError)
        {
            return SmartHanziTextToPinyin(owner, 
                strText, 
                style, 
               ( bAutoSel ? "auto" : "" ),
                out strPinyin,
                out strError);
        }

        // �����ַ���ת��Ϊƴ��
        // ����������Ѿ�MessageBox������strError��һ�ַ���Ϊ�ո�
        /// <summary>
        /// �����ַ���ת��Ϊƴ�������ܷ�ʽ
        /// </summary>
        /// <param name="owner">���ں����� MessageBox �ͶԻ��� ����������</param>
        /// <param name="strText">�����ַ���</param>
        /// <param name="style">ת��Ϊƴ���ķ��</param>
        /// <param name="strDuoyinStyle">�Ƿ��Զ�ѡ������֡�auto/first ��һ��������ϡ����Ϊ auto,first ��ʾ���Ȱ�������ƴ��ѡ��û������ƴ���ģ�ѡ���һ��</param>
        /// <param name="strPinyin">����ƴ���ַ���</param>
        /// <param name="strError"></param>
        /// <returns>-1: ����; 0: �û�ϣ���ж�; 1: ����; 2: ����ַ�������û���ҵ�ƴ���ĺ���</returns>
        public int SmartHanziTextToPinyin(
            IWin32Window owner,
            string strText,
            PinyinStyle style,
            string strDuoyinStyle,  // bool bAutoSel,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            bool bAuto = StringUtil.IsInList("auto", strDuoyinStyle);
            bool bFirst = StringUtil.IsInList("first", strDuoyinStyle);

            bool bNotFoundPinyin = false;   // �Ƿ���ֹ�û���ҵ�ƴ����ֻ�ܰѺ��ַ������ַ��������

            Stop new_stop = new DigitalPlatform.Stop();
            new_stop.Register(this.stopManager, true);	// ����������
            new_stop.OnStop += new StopEventHandler(new_stop_OnStop);
            new_stop.Initial("���ڻ�� '" + strText + "' ��ƴ����Ϣ (�ӷ����� " + this.PinyinServerUrl + ")...");
            new_stop.BeginLoop();

            m_gcatClient = null;
            try
            {

                m_gcatClient = GcatNew.CreateChannel(this.PinyinServerUrl);

            REDO_GETPINYIN:
                int nStatus = -1;	// ǰ��һ���ַ������� -1:ǰ��û���ַ� 0:��ͨӢ����ĸ 1:�ո� 2:����
                string strPinyinXml = "";
                // return:
                //      -2  strID��֤ʧ��
                //      -1  ����
                //      0   �ɹ�
                int nRet = GcatNew.GetPinyin(
                    new_stop,
                    m_gcatClient,
                    m_strPinyinGcatID,
                    strText,
                    out strPinyinXml,
                    out strError);
                if (nRet == -1)
                {
                    if (new_stop != null && new_stop.State != 0)
                        return 0;

                    DialogResult result = MessageBox.Show(this,
    "�ӷ����� '" + this.PinyinServerUrl + "' ��ȡƴ���Ĺ��̳���:\r\n" + strError + "\r\n\r\n�Ƿ�Ҫ��ʱ��Ϊʹ�ñ�����ƴ������? \r\n\r\n(ע����ʱ���ñ���ƴ����״̬�ڳ����˳�ʱ���ᱣ�������Ҫ���ø��ñ���ƴ����ʽ����ʹ�����˵��ġ��������á��������������������ҳ�ġ�ƴ��������URL���������)",
    "MainForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        this.ForceUseLocalPinyinFunc = true;
                        strError = "�����ñ���ƴ���������²���һ�Ρ�(���β�������: " + strError + ")";
                        return -1;
                    }
                    strError = " " + strError;
                    return -1;
                }

                if (nRet == -2)
                {
                    IdLoginDialog login_dlg = new IdLoginDialog();
                    GuiUtil.SetControlFont(login_dlg, this.Font);
                    login_dlg.Text = "���ƴ�� -- "
                        + ((string.IsNullOrEmpty(this.m_strPinyinGcatID) == true) ? "������ID" : strError);
                    login_dlg.ID = this.m_strPinyinGcatID;
                    login_dlg.SaveID = this.m_bSavePinyinGcatID;
                    login_dlg.StartPosition = FormStartPosition.CenterScreen;
                    if (login_dlg.ShowDialog(owner) == DialogResult.Cancel)
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
                    strError = "strPinyinXmlװ�ص�XMLDOMʱ����: " + ex.Message;
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

                    // Ŀǰֻȡ���׶����ĵ�һ��
                    nRet = strWordPinyin.IndexOf(";");
                    if (nRet != -1)
                        strWordPinyin = strWordPinyin.Substring(0, nRet).Trim();

                    string[] pinyin_parts = strWordPinyin.Split(new char[] { ' ' });
                    int index = 0;
                    // ��ѡ�������
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
                        // ���ԣ�
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

                        {	// ����Ƕ��ƴ��
                            SelPinyinDlg dlg = new SelPinyinDlg();
                            float ratio_single = dlg.listBox_multiPinyin.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            float ratio_sample = dlg.textBox_sampleText.Font.SizeInPoints / dlg.Font.SizeInPoints;
                            GuiUtil.SetControlFont(dlg, this.Font, false);
                            // ά�������ԭ�д�С������ϵ
                            dlg.listBox_multiPinyin.Font = new Font(dlg.Font.FontFamily, ratio_single * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            dlg.textBox_sampleText.Font = new Font(dlg.Font.FontFamily, ratio_sample * dlg.Font.SizeInPoints, GraphicsUnit.Point);
                            // ����Ի���Ƚ����� GuiUtil.SetControlFont(dlg, this.Font, false);

                            dlg.Text = "��ѡ���� '" + strHanzi + "' ��ƴ�� (���Է����� " + this.PinyinServerUrl + ")";
                            dlg.SampleText = strSampleText;
                            dlg.Offset = nOffs;
                            dlg.Pinyins = strCharPinyins;
                            if (index < pinyin_parts.Length)
                                dlg.ActivePinyin = pinyin_parts[index];
                            dlg.Hanzi = strHanzi;

                            if (bAuto == true
                                && string.IsNullOrEmpty(dlg.ActivePinyin) == false)
                            {
                                dlg.ResultPinyin = dlg.ActivePinyin;
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else if (bFirst == true
                                && string.IsNullOrEmpty(dlg.Pinyins) == false)
                            {
                                dlg.ResultPinyin = SelPinyinDlg.GetFirstPinyin(dlg.Pinyins);
                                dlg.DialogResult = DialogResult.OK;
                            }
                            else
                            {
                                this.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                                dlg.ShowDialog(owner);

                                this.AppInfo.UnlinkFormState(dlg);
                            }

                            Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "�ƶ�");

                            if (dlg.DialogResult == DialogResult.Abort)
                            {
                                return 0;   // �û�ϣ�������ж�
                            }

                            DomUtil.SetAttr(nodeChar, "sel", dlg.ResultPinyin);

                            if (dlg.DialogResult == DialogResult.Cancel)
                            {
                                SelPinyinDlg.AppendText(ref strPinyin, strHanzi);
                                nStatus = 2;
                                bNotFoundPinyin = true;
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
                                Debug.Assert(false, "SelPinyinDlg����ʱ���������DialogResultֵ");
                            }

                            index++;
                        }

                    }
                }

#if _TEST_PINYIN
#else
                // 2014/10/22
                // ɾ�� word �µ� Text �ڵ�
                XmlNodeList text_nodes = dom.DocumentElement.SelectNodes("word/text()");
                foreach (XmlNode node in text_nodes)
                {
                    Debug.Assert(node.NodeType == XmlNodeType.Text, "");
                    node.ParentNode.RemoveChild(node);
                }

                // 2013/9/17
                // ��û��p���Ե�<char>Ԫ��ȥ�����Ա��ϴ�
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//char");
                foreach (XmlNode node in nodes)
                {
                    string strP = DomUtil.GetAttr(node, "p");
                    string strSelValue = DomUtil.GetAttr(node, "sel");  // 2013/9/13

                    if (string.IsNullOrEmpty(strP) == true
                        || string.IsNullOrEmpty(strSelValue) == true)
                    {
                        XmlNode parent = node.ParentNode;
                        parent.RemoveChild(node);

                        // �ѿյ�<word>Ԫ��ɾ��
                        if (parent.Name == "word"
                            && parent.ChildNodes.Count == 0
                            && parent.ParentNode != null)
                        {
                            parent.ParentNode.RemoveChild(parent);
                        }
                    }

                    // TODO: һ��ƴ����û������ѡ��ģ��Ƿ�Ͳ������ˣ�
                    // ע�⣬ǰ�˸����´�����ƴ���������أ�ֻ�ǵ���ԭ���ӷ����������ģ�����������
                }

                if (dom.DocumentElement.ChildNodes.Count > 0)
                {

                    // return:
                    //      -2  strID��֤ʧ��
                    //      -1  ����
                    //      0   �ɹ�
                    nRet = GcatNew.SetPinyin(
                        new_stop,
                        m_gcatClient,
                        "",
                        dom.DocumentElement.OuterXml,
                        out strError);
                    if (nRet == -1)
                    {
                        if (new_stop != null && new_stop.State != 0)
                            return 0;

                        return -1;
                    }
                }
#endif

                if (bNotFoundPinyin == false)
                    return 1;   // ��������

                return 2;   // ����ַ�������û���ҵ�ƴ���ĺ���
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

        // parameters:
        //      strIndicator    �ֶ�ָʾ���������null���ã����ʾ����ָʾ������ɸѡ
        // return:
        //      0   û���ҵ�ƥ�����������
        //      >=1 �ҵ��������ҵ��������������
        public static int GetPinyinCfgLine(XmlDocument cfg_dom,
            string strFieldName,
            string strIndicator,
            out List<PinyinCfgItem> cfg_items)
        {
            cfg_items = new List<PinyinCfgItem>();

            XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                PinyinCfgItem item = new PinyinCfgItem(node);

                if (item.FieldName != strFieldName)
                    continue;

                if (string.IsNullOrEmpty(item.IndicatorMatchCase) == false
                    && string.IsNullOrEmpty(strIndicator) == false)
                {
                    if (MarcUtil.MatchIndicator(item.IndicatorMatchCase, strIndicator) == false)
                        continue;
                }

                cfg_items.Add(item);
            }

            return cfg_items.Count;
        }

        // Ϊһ�� MARC ��¼��ƴ��
        // return:
        //      -1  ���������жϵ����
        //      0   ����
        /// <summary>
        /// Ϊ MarcRecord �����ڵļ�¼��ƴ��
        /// </summary>
        /// <param name="record">MARC ��¼����</param>
        /// <param name="strCfgXml">ƴ������ XML</param>
        /// <param name="style">���</param>
        /// <param name="strPrefix">ǰ׺�ַ�����ȱʡΪ�� [��������ʱδ����]</param>
        /// <param name="strDuoyinStyle">�Ƿ��Զ�ѡ������֡�auto/first ֮һ�������</param>
        /// <returns>-1: ���������жϵ����; 0: ����</returns>
        public int AddPinyin(
            MarcRecord record,
            string strCfgXml,
            PinyinStyle style = PinyinStyle.None,
            string strPrefix = "",
            string strDuoyinStyle=""
            /*bool bAutoSel = false*/)
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXmlװ�ص�XMLDOMʱ����: " + ex.Message;
                goto ERROR1;
            }

            // PinyinStyle style = PinyinStyle.None;	// �������޸�ƴ����Сд���
            MarcNodeList fields = record.select("field");

            foreach (MarcField field in fields)
            {
                List<PinyinCfgItem> cfg_items = null;
                int nRet = GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                string strHanzi = "";

                foreach (PinyinCfgItem item in cfg_items)
                {
                    for (int k = 0; k < item.From.Length; k++)
                    {
                        if (item.From.Length != item.To.Length)
                        {
                            strError = "�������� fieldname='" + item.FieldName + "' from='" + item.From + "' to='" + item.To + "' ����from��to����ֵ���ַ�������";
                            goto ERROR1;
                        }

                        string from = new string(item.From[k], 1);
                        string to = new string(item.To[k], 1);

                        // ɾ���Ѿ����ڵ�Ŀ�����ֶ�
                        field.select("subfield[@name='" + to + "']").detach();

                        MarcNodeList subfields = field.select("subfield[@name='" + from + "']");

                        foreach (MarcSubfield subfield in subfields)
                        {
                            strHanzi = subfield.Content;

                            if (MarcDetailHost.ContainHanzi(strHanzi) == false)
                                continue;

                            string strPinyin;
#if NO
                            // ���ַ����еĺ��ֺ�ƴ������
                            // return:
                            //      -1  ����
                            //      0   �û�ϣ���ж�
                            //      1   ����
                            if (string.IsNullOrEmpty(this.PinyinServerUrl) == true
                               || this.ForceUseLocalPinyinFunc == true)
                            {
                                nRet = this.HanziTextToPinyin(
                                    this,
                                    true,	// ���أ�����
                                    strHanzi,
                                    style,
                                    out strPinyin,
                                    out strError);
                            }
                            else
                            {
                                // �����ַ���ת��Ϊƴ��
                                // ����������Ѿ�MessageBox������strError��һ�ַ���Ϊ�ո�
                                // return:
                                //      -1  ����
                                //      0   �û�ϣ���ж�
                                //      1   ����
                                nRet = this.SmartHanziTextToPinyin(
                                    this,
                                    strHanzi,
                                    style,
                                    bAutoSel,
                                    out strPinyin,
                                    out strError);
                            }
#endif
                            nRet = this.GetPinyin(this,
                                strHanzi,
                                style,
                                strDuoyinStyle,   // bAutoSel,
                                out strPinyin,
                                out strError);

                            if (nRet == -1)
                            {
                                goto ERROR1;
                            }
                            if (nRet == 0)
                            {
                                strError = "�û��жϡ�ƴ�����ֶ����ݿ��ܲ�������";
                                goto ERROR1;
                            }

                            subfield.after(MarcQuery.SUBFLD + to + strPinyin);
                        }
                    }
                }
            }

            return 0;
        ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
            {
                if (strError[0] != ' ')
                    MessageBox.Show(this, strError);
            }
            return -1;
        }

        public void RemovePinyin(
            MarcRecord record,
            string strCfgXml)
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXmlװ�ص�XMLDOMʱ����: " + ex.Message;
                goto ERROR1;
            }

            MarcNodeList fields = record.select("field");

            foreach (MarcField field in fields)
            {

                List<PinyinCfgItem> cfg_items = null;
                int nRet = GetPinyinCfgLine(
                    cfg_dom,
                    field.Name,
                    field.Indicator,    // TODO: ���Բ�����ָʾ�������������ɾ������Ѱ��Χ
                    out cfg_items);
                if (nRet <= 0)
                    continue;

                foreach (PinyinCfgItem item in cfg_items)
                {
                    foreach (char ch in item.To)
                    {
                        string to = new string(ch, 1);

                        // ɾ���Ѿ����ڵ�Ŀ�����ֶ�
                        field.select("subfield[@name='" + to + "']").detach();
                    }
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // ���� MARC ��ʽ��¼�� HTML ��ʽ
        // paramters:
        //      strMARC MARC���ڸ�ʽ
        // return:
        //      -1  ����
        //      0   .fltx �ļ�û���ҵ�
        //      1   �ɹ�
        public int BuildMarcHtmlText(
            string strSytaxOID,
            string strMARC,
            out string strHtmlString,
            out string strError)
        {
            strHtmlString = "";
            strError = "";
            int nRet = 0;

#if NO
            nRet = BuildMarc21Html(strMARC,
            out strHtmlString,
            out strError);
            if (nRet == -1)
                return -1;
            return 1;
#endif

            FilterHost host = new FilterHost();
            host.ID = "";
            host.MainForm = this;

            BrowseFilterDocument filter = null;

            string strFilterFileName = Path.Combine(this.DataDir, strSytaxOID.Replace(".", "_") + "\\marc_html.fltx");

            nRet = this.PrepareMarcFilter(
                host,
                strFilterFileName,
                out filter,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
                return 0;

            try
            {
                nRet = filter.DoRecord(null,
        strMARC,
        0,
        out strError);
                if (nRet == -1)
                    goto ERROR1;

                strHtmlString = host.ResultString;
            }
            finally
            {
                // �黹����
                this.Filters.SetFilter(strFilterFileName, filter);
            }

            return 1;
        ERROR1:
            // ���û��棬��Ϊ���ܳ����˱������
            // TODO: ��ȷ���ֱ������
            this.Filters.ClearFilter(strFilterFileName);
            return -1;
        }

        // return:
        //      -1  ����
        //      0   .fltx �ļ�û���ҵ�
        //      1   �ɹ�
        public int PrepareMarcFilter(
FilterHost host,
string strFilterFileName,
out BrowseFilterDocument filter,
out string strError)
        {
            strError = "";

            // �����Ƿ����ֳɿ��õĶ���
            filter = (BrowseFilterDocument)this.Filters.GetFilter(strFilterFileName);

            if (filter != null)
            {
                filter.FilterHost = host;
                return 1;
            }

            // �´���
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
            catch (FileNotFoundException ex)
            {
                strError = ex.Message;
                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            string strCode = "";    // c#����

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strBinDir = Environment.CurrentDirectory;

            string[] saAddRef1 = {
					strBinDir + "\\digitalplatform.marcdom.dll",
					strBinDir + "\\digitalplatform.marckernel.dll",
					strBinDir + "\\digitalplatform.marcquery.dll",
					strBinDir + "\\digitalplatform.dll",
					strBinDir + "\\digitalplatform.Text.dll",
					strBinDir + "\\digitalplatform.IO.dll",
					strBinDir + "\\digitalplatform.Xml.dll",
					strBinDir + "\\dp2catalog.exe" };

            Assembly assembly = null;
            string strWarning = "";
            string strLibPaths = "";

            string[] saRef2 = filter.GetRefs();

            string[] saRef = new string[saRef2.Length + saAddRef1.Length];
            Array.Copy(saRef2, saRef, saRef2.Length);
            Array.Copy(saAddRef1, 0, saRef, saRef2.Length, saAddRef1.Length);

            // ����Script��Assembly
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
            return 1;
        ERROR1:
            return -1;
        }

        #region ���кŻ���

#if SN
        internal void WriteSerialNumberStatusFile()
        {
            try
            {
                string strFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dp2catalog_status");
                if (File.Exists(strFileName) == true)
                    return;
                using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
                {
                    sw.Write(DateTimeUtil.DateTimeToString8(DateTime.Now));
                }

                File.SetAttributes(strFileName, FileAttributes.Hidden);
            }
            catch
            {
            }
        }

        internal bool IsExistsSerialNumberStatusFile()
        {
            try
            {
                string strFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dp2catalog_status");
                return File.Exists(strFileName);
            }
            catch
            {
            }
            return true;    // ��������쳣�������д��ļ�
        }

#endif



        bool _testMode = false;  // true: ���к�Ϊ�յ�ʱ��Ҳ���� ����ģʽ����������ģʽ��ǰ�˾��޷��ͱ�׼��� dp2library ����ʹ���ˣ�ֻ�ܺ� dp2libraryXE ����������ģʽ����ʹ��

        public bool TestMode
        {
            get
            {
                return this._testMode;
            }
            set
            {
                this._testMode = value;
                SetTitle();
            }
        }

        bool _communityMode = false;

        public bool CommunityMode
        {
            get
            {
                return this._communityMode;
            }
            set
            {
                this._communityMode = value;
                SetTitle();
            }
        }

        void SetTitle()
        {
            if (this.TestMode == true)
                this.Text = "dp2Catalog V2 -- ��Ŀ [����ģʽ]";
            else if (this.CommunityMode == true)
                this.Text = "dp2Catalog V2 -- ��Ŀ [������]";
            else
                this.Text = "dp2Catalog V2 -- ��Ŀ [רҵ��]";
        }

#if SN
        // �������ַ���ƥ�����к�
        bool MatchLocalString(string strSerialNumber)
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            foreach (string mac in macs)
            {
                string strLocalString = GetEnvironmentString(mac);
                string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                    return true;
            }

            // 2014/12/19
            if (DateTime.Now.Month == 12)
            {
                foreach (string mac in macs)
                {
                    string strLocalString = GetEnvironmentString(mac, true);
                    string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                    if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                        return true;
                }

            }

            return false;
        }

        // parameters:
        //      strRequirFuncList   Ҫ�����߱��Ĺ����б����ż�����ַ���
        //      bReinput    ������кŲ�����Ҫ���Ƿ�ֱ�ӳ��ֶԻ������û������������к�
        // return:
        //      -1  ����
        //      0   ��ȷ
        internal int VerifySerialCode(
            string strTitle,
            string strRequirFuncList,
            bool bReinput,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 2014/11/15
            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");

            // �״�����
            if (string.IsNullOrEmpty(strSerialCode) == true)
            {
            }

        REDO_VERIFY:
            if (strSerialCode == "test")
            {
                if (string.IsNullOrEmpty(strRequirFuncList) == true)
                {
                    this.TestMode = true;
                    this.CommunityMode = false;
                    // ����д�� ����ģʽ ��Ϣ����ֹ�û�����
                    // С�Ͱ�û�ж�Ӧ������ģʽ
                    this.AppInfo.SetString("main_form", "last_mode", "test");
                    return 0;
                }
            }
            else if (strSerialCode == "community")
            {
                if (string.IsNullOrEmpty(strRequirFuncList) == true)
                {
                    this.TestMode = false;
                    this.CommunityMode = true;
                    this.AppInfo.SetString("main_form", "last_mode", "community");
                    return 0;
                }
            }
            else
            {
                this.TestMode = false;
                this.AppInfo.SetString("main_form", "last_mode", "standard");
            }

            //string strLocalString = GetEnvironmentString();

            //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");

        if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false ||
                // strSha1 != GetCheckCode(strSerialCode) 
                MatchLocalString(strSerialCode) == false
                || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (bReinput == false)
                {
                    strError = "���к���Ч";
                    return -1;
                }

                if (String.IsNullOrEmpty(strSerialCode) == false)
                    MessageBox.Show(this, "���к���Ч������������");
                else if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false)
                    MessageBox.Show(this, "���к��� function ������Ч������������");

                // �����������кŶԻ���
                nRet = ResetSerialCode(
                    strTitle,
                    false,
                    strSerialCode,
                    GetEnvironmentString(strFirstMac));
                if (nRet == 0)
                {
                    strError = "����";
                    return -1;
                }
                strSerialCode = this.AppInfo.GetString("sn", "sn", "");
                goto REDO_VERIFY;
            }
            return 0;
        }

        // return:
        //      false   ������
        //      true    ����
        bool CheckFunction(string strEnvString,
            string strFuncList)
        {
            Hashtable table = StringUtil.ParseParameters(strEnvString);
            string strFuncValue = (string)table["function"];
            string[] parts = strFuncList.Split(new char[] { ',' });
            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part) == true)
                    continue;
                if (StringUtil.IsInList(part, strFuncValue) == false)
                    return false;
            }

            return true;
        }

        // parameters:
        //      bServer     �Ƿ�ΪС�ͷ������汾�������С�ͷ������汾���� net.tcp Э��� dp2library host��������ǵ����汾���� net.pipe �� dp2library host
        string GetEnvironmentString(string strMAC,
            bool bNextYear = false)
        {
            Hashtable table = new Hashtable();
            table["mac"] = strMAC;  //  SerialCodeForm.GetMacAddress();
            if (bNextYear == false)
                table["time"] = SerialCodeForm.GetTimeRange();
            else
                table["time"] = SerialCodeForm.GetNextYearTimeRange();

            table["product"] = "dp2catalog";

            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
            // �� strSerialCode �е���չ�����趨�� table ��
            SerialCodeForm.SetExtParams(ref table, strSerialCode);
#if NO
            if (string.IsNullOrEmpty(strSerialCode) == false)
            {
                string strExtParam = GetExtParams(strSerialCode);
                if (string.IsNullOrEmpty(strExtParam) == false)
                {
                    Hashtable ext_table = StringUtil.ParseParameters(strExtParam);
                    string function = (string)ext_table["function"];
                    if (string.IsNullOrEmpty(function) == false)
                        table["function"] = function;
                }
            }
#endif

            return StringUtil.BuildParameterString(table);
        }



        // ��� xxx|||xxxx ����߲���
        static string GetCheckCode(string strSerialCode)
        {
            string strSN = "";
            string strExtParam = "";
            StringUtil.ParseTwoPart(strSerialCode,
                "|||",
                out strSN,
                out strExtParam);

            return strSN;
        }

        // ��� xxx|||xxxx ���ұ߲���
        static string GetExtParams(string strSerialCode)
        {
            string strSN = "";
            string strExtParam = "";
            StringUtil.ParseTwoPart(strSerialCode,
                "|||",
                out strSN,
                out strExtParam);

            return strExtParam;
        }

#if NO
        static string GetTimeRange()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString().PadLeft(4, '0');
        }
#endif

        string CopyrightKey = "dp2catalog_sn_key";

        // return:
        //      0   Cancel
        //      1   OK
        int ResetSerialCode(
            string strTitle,
            bool bAllowSetBlank,
            string strOldSerialCode,
            string strOriginCode)
        {
            Hashtable ext_table = StringUtil.ParseParameters(strOriginCode);
            string strMAC = (string)ext_table["mac"];
            if (string.IsNullOrEmpty(strMAC) == true)
                strOriginCode = "!error";
            else 
                strOriginCode = Cryptography.Encrypt(strOriginCode,
                this.CopyrightKey);
            SerialCodeForm dlg = new SerialCodeForm();
            dlg.Text = strTitle;
            dlg.Font = this.Font;
            dlg.SerialCode = strOldSerialCode;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.OriginCode = strOriginCode;

        REDO:
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            if (string.IsNullOrEmpty(dlg.SerialCode) == true)
            {
                if (bAllowSetBlank == true)
                {
                    DialogResult result = MessageBox.Show(this,
        "ȷʵҪ�����к�����Ϊ��?\r\n\r\n(һ�������к�����Ϊ�գ�dp2Catalog ���Զ��˳����´�������Ҫ�����������к�)",
        "dp2Catalog",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        return 0;
                    }
                }
                else
                {
                    MessageBox.Show(this, "���кŲ�����Ϊ�ա�����������");
                    goto REDO;
                }
            }

            this.AppInfo.SetString("sn", "sn", dlg.SerialCode);
            this.AppInfo.Save();

            return 1;
        }

        // �����к��л�� expire= ����ֵ
        // ����ֵΪ MAC ��ַ���б��м�ָ��� '|'
        internal string GetExpireParam()
        {
            string strSerialCode = this.AppInfo.GetString("sn", "sn", "");
            if (string.IsNullOrEmpty(strSerialCode) == false)
            {
                string strExtParam = GetExtParams(strSerialCode);
                if (string.IsNullOrEmpty(strExtParam) == false)
                {
                    Hashtable ext_table = StringUtil.ParseParameters(strExtParam);
                    return (string)ext_table["expire"];
                }
            }

            return "";
        }

#endif

        #endregion

        private void MenuItem_resetSerialCode_Click(object sender, EventArgs e)
        {
#if SN
            string strError = "";
            int nRet = 0;

            // 2014/11/15
            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            string strRequirFuncList = "";  // ��Ϊ����������ͨ�õ����кţ�����������ĸ����ܣ����Զ����ú����кŵĹ��ܲ�����顣ֻ�еȵ��õ����幦�ܵ�ʱ�򣬲��ܷ������к��Ƿ�������幦�ܵ� function = ... ����

            string strSerialCode = "";
        REDO_VERIFY:
            if (strSerialCode == "test")
            {
                this.TestMode = true;
                this.CommunityMode = false;
                // ����д�� ����ģʽ ��Ϣ����ֹ�û�����
                // С�Ͱ�û�ж�Ӧ������ģʽ
                this.AppInfo.SetString("main_form", "last_mode", "test");
                return;
            }
            else if (strSerialCode == "community")
            {
                this.TestMode = false;
                this.CommunityMode = true;
                this.AppInfo.SetString("main_form", "last_mode", "community");
                return;
            }
            else
            {
                this.TestMode = false;
                this.CommunityMode = false;
                this.AppInfo.SetString("main_form", "last_mode", "standard");
            }

            //string strLocalString = GetEnvironmentString();

            //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");

            if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false ||
                // strSha1 != GetCheckCode(strSerialCode) 
                MatchLocalString(strSerialCode) == false
                || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (String.IsNullOrEmpty(strSerialCode) == false)
                    MessageBox.Show(this, "���к���Ч������������");
                else if (CheckFunction(GetEnvironmentString(""), strRequirFuncList) == false)
                    MessageBox.Show(this, "���к��� function ������Ч������������");


                // �����������кŶԻ���
                nRet = ResetSerialCode(
                    "�����������к�",
                    true,
                    strSerialCode,
                    GetEnvironmentString(strFirstMac));
                if (nRet == 0)
                {
                    strError = "����";
                    goto ERROR1;
                }
                strSerialCode = this.AppInfo.GetString("sn", "sn", "");
                if (string.IsNullOrEmpty(strSerialCode) == true)
                {
                    Application.Exit();
                    return;
                }

                this.AppInfo.Save();
                goto REDO_VERIFY;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        private void MenuItem_openAdvertiseForm_Click(object sender, EventArgs e)
        {
            AdvertiseForm form = new AdvertiseForm();
            form.MdiParent = this;
            form.MainForm = this;
            form.Show();
        }

        private void MenuItem_openUserFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

    }

    public class EnableState
    {
        public object Control = null;
        public bool Enabled = false;
    }

    public class EnableStateCollection : List<EnableState>
    {
        // ���룬��Disable�ؼ�
        public void Push(object obj)
        {
            EnableState state = new EnableState();
            state.Control = obj;

            ToolStripItem item = null;
            Control control = null;

            if (obj is Control)
            {
                control = (Control)obj;
                state.Enabled = control.Enabled;

                control.Enabled = false;
            }
            else if (obj is ToolStripItem)
            {
                item = (ToolStripItem)obj;
                state.Enabled = item.Enabled;

                item.Enabled = false;
            }
            else
            {
                throw new Exception("obj���ͱ���ΪControl ToolStripItem֮һ");
            }

            this.Add(state);
        }



        public void RestoreAll()
        {
            for (int i = 0; i < this.Count; i++)
            {
                EnableState state = this[i];

                ToolStripItem item = null;
                Control control = null;

                if (state.Control is Control)
                {
                    control = (Control)state.Control;
                    control.Enabled = state.Enabled;
                }
                else if (state.Control is ToolStripItem)
                {
                    item = (ToolStripItem)state.Control;
                    item.Enabled = state.Enabled;
                }
                else
                {
                    throw new Exception("state.Control���ͱ���ΪControl ToolStripItem֮һ");
                }

            }

            this.Clear(); // �ú����
        }
    }

    //
    /*
<item name = "ISSN" value=8 uni_name = "Identifier-ISSN" />
     * */
    public class Bib1Use
    {
        public string Name = "";
        public string Value = "";
        public string UniName = "";
        public string Comment = "";
    }

    public class FromCollection : List<Bib1Use>
    {
        public string GetValue(string strName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name.Trim() == strName.Trim())
                    return this[i].Value;
            }

            return null;    // not found
        }

    }

}