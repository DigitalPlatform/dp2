using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;   // for WebClient class
using System.IO;
using System.Xml;

using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Deployment.Application;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Script;

using DigitalPlatform.Library;
using DigitalPlatform.Core;

namespace dp2rms
{
    public partial class MainForm : Form
    {
        public ObjectCache<XmlDocument> DomCache = new ObjectCache<XmlDocument>();

        // 先前用过的备份文件名
        public string UsedBackupFileName = "*.dp2bak";

        public string DataDir = "";

        // 为C#脚本所准备
        public Hashtable ParamTable = new Hashtable();

        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

        public ServerCollection Servers = null;

        public CfgCache cfgCache = new CfgCache();

        // bool	m_bFirstMdiOpened = false;

        //保存界面信息
        public ApplicationInfo AppInfo = new ApplicationInfo("dp2rms.xml");

        public QuickPinyin QuickPinyin = null;
        public IsbnSplitter IsbnSplitter = null;

        public MainForm()
        {
            InitializeComponent();
        }

        public int LoadIsbnSplitter(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            string strFileName = Path.Combine(this.DataDir, "rangemessage.xml");

            // 优化
            if (this.IsbnSplitter != null)
                return 0;

            REDO:

            try
            {
                this.IsbnSplitter = new IsbnSplitter(strFileName); // "isbn.xml"
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地isbn规则文件 " + strFileName + " 时发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile(Path.GetFileName(strFileName),
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
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
                strError = "装载本地isbn规则文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }


        public int LoadQuickPinyin(bool bAutoDownload,
            out string strError)
        {
            strError = "";

            // 优化
            if (this.QuickPinyin != null)
                return 0;

            REDO:

            try
            {
                this.QuickPinyin = new QuickPinyin(this.DataDir + "\\pinyin.xml");
            }
            catch (FileNotFoundException ex)
            {
                strError = "装载本地拼音文件发生错误 :" + ex.Message;

                if (bAutoDownload == true)
                {
                    string strError1 = "";
                    int nRet = this.DownloadDataFile("pinyin.xml",
                        out strError1);
                    if (nRet == -1)
                    {
                        strError = strError + "\r\n自动下载文件。\r\n" + strError1;
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
                strError = "装载本地拼音文件发生错误 :" + ex.Message;
                return -1;
            }

            return 1;
        }

        private void MainFormNew_Load(object sender, EventArgs e)
        {
            Searching(false);   // 隐藏searching ToolStripLabel

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


            this.SetMenuItemState(); //任延华加 2006/1/5
            this.toolBarButton_stop.Enabled = false; //任延华加 2006/1/5


            // 从文件中装载创建一个ServerCollection对象
            // parameters:
            //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
            //							如果==true，函数直接返回一个新的空ServerCollection对象
            // Exception:
            //			FileNotFoundException	文件没找到
            //			SerializationException	版本迁移时容易出现
            try
            {

                Servers = ServerCollection.Load(this.DataDir
                    + "\\servers.bin",
                    true);
                Servers.ownerForm = this;
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(this, ex.Message);
                Servers = new ServerCollection();
                // 设置文件名，以便本次运行结束时覆盖旧文件
                Servers.FileName = this.DataDir
                    + "\\servers.bin";

            }

            this.Servers.ServerChanged += new ServerChangedEventHandle(Servers_ServerChanged);

            string strError = "";
            int nRet = cfgCache.Load(this.DataDir
                + "\\cfgcache.xml",
                out strError);
            if (nRet == -1)
            {
                if (IsFirstRun == false)
                    MessageBox.Show(strError + "\r\n\r\n程序稍后会尝试自动创建这个文件");
            }


            cfgCache.TempDir = this.DataDir
                + "\\cfgcache";
            cfgCache.InstantSave = true;


            // 设置窗口尺寸状态
            if (AppInfo != null)
            {
                SetFirstDefaultFont();

                MainForm.SetControlFont(this, this.DefaultFont);

                AppInfo.LoadFormStates(this,
                    "mainformstate");
            }


            stopManager.Initial(toolBarButton_stop,
                (object)this.toolStripStatusLabel_main,
                (object)this.toolStripProgressBar_main);

            // 存在上次遗留的窗口
            int nLastSearchWindow = this.AppInfo.GetInt(
                "main_form",
                "last_search_window",
                1);
            if (nLastSearchWindow == 1)
            {
                MenuItem_openSearch_Click(null, null);	// 打开一个检索窗
            }

            if (IsFirstRun == true && this.Servers.Count == 0)
            {
                MessageBox.Show(this, "欢迎您安装使用dp2rms -- 资源管理。");
                ManageServers(true);
                ManagePreference();
            }

        }

        public string StatusLabel
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

        void Servers_ServerChanged(object sender, ServerChangedEventArgs e)
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child is SearchForm)
                {
                    SearchForm searchform = (SearchForm)child;
                    searchform.RefreshResTree();
                }

            }
        }

        private void MainFormNew_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void MainFormNew_FormClosed(object sender, FormClosedEventArgs e)
        {

            this.Servers.ServerChanged -= new ServerChangedEventHandle(Servers_ServerChanged);

            // 保存到文件
            // parameters:
            //		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
            Servers.Save(null);
            Servers = null;

            string strError;
            int nRet = cfgCache.Save(null, out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            // 保存窗口尺寸状态
            if (AppInfo != null)
            {

                // 只要存在Search窗口
                this.AppInfo.SetInt(
                    "main_form",
                    "last_search_window",
                    this.TopSearchForm != null ? 1 : 0);


                /*
                // MDI子窗口是否最大化
                if (this.ActiveMdiChild != null) 
                {
                    this.applicationInfo.SetString(
                        "mdiwindows", "window_state", 
                        Enum.GetName(typeof(FormWindowState),
                        this.ActiveMdiChild.WindowState));
                }
                */

                AppInfo.SaveFormStates(this,
                    "mainformstate");
            }

            //记住save,保存信息XML文件
            AppInfo.Save();
            AppInfo = null;	// 避免后面再用这个对象		

        }

        // 打开...
        private void MenuItem_open_Click(object sender, EventArgs e)
        {

        }

        // 打开检索窗
        private void MenuItem_openSearch_Click(object sender, EventArgs e)
        {
            SearchForm child = new SearchForm();

            child.MdiParent = this;

            // child.Height = 20;
            child.Show();
            //SetFirstMdiWindowState();
        }

        // 打开新详细窗[空白]
        private void MenuItem_openDetail_Click(object sender, EventArgs e)
        {
            DetailForm child = new DetailForm();

            child.MdiParent = this;

            child.Show();
            //SetFirstMdiWindowState();

        }

        // 有关MDI子窗口排列的菜单命令
        private void MenuItem_openDetailWithTemplate_Click(object sender, EventArgs e)
        {
            DetailForm child = new DetailForm();

            child.MdiParent = this;

            child.Show();
            //SetFirstMdiWindowState();


            child.LoadTemplate();
        }

        // 保存
        private void MenuItem_save_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                ((DetailForm)this.ActiveMdiChild).SaveRecord(null);
            }

        }

        // 另存为
        private void MenuItem_saveas_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                // ((DetailForm)this.ActiveMdiChild).SaveRecord(null);
                ((DetailForm)this.ActiveMdiChild).SaveToBackup(null);

            }
        }

        // 另存到数据库
        private void MenuItem_saveasToDB_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                ((DetailForm)this.ActiveMdiChild).SaveAsRecord();
            }

        }

        // 属性
        private void MenuItem_properties_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                DetailForm detail = (DetailForm)this.ActiveMdiChild;

                RecordPropertiesDlg dlg = new RecordPropertiesDlg();
                MainForm.SetControlFont(dlg, this.DefaultFont);

                dlg.textBox_content.Text = detail.PropertiesText;
                dlg.ShowDialog(this);
            }
        }

        // 服务器管理
        private void MenuItem_serversManagement_Click(object sender, EventArgs e)
        {
            ManageServers(false);
        }

        void ManageServers(bool bFirstRun)
        {
            ServersDlg dlg = new ServersDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            string strWidths = this.AppInfo.GetString(
"serversdlg",
"list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(dlg.ListView,
                    strWidths,
                    true);
            }

            ServerCollection newServers = Servers.Dup();

            if (bFirstRun == true)
                dlg.Text = "首次运行: 请指定服务器参数";
            dlg.Servers = newServers;
            this.AppInfo.LinkFormState(dlg, "serversdlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);

            strWidths = ListViewUtil.GetColumnWidthListString(dlg.ListView);
            this.AppInfo.SetString(
                "serversdlg",
                "list_column_width",
                strWidths);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // this.Servers = newServers;
            this.Servers.Import(newServers);
        }

        // 退出
        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void Searching(bool bSearching)
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));

            if (bSearching == true)
            {
                this.toolStripLabel_searching.Image = ((System.Drawing.Image)(resources.GetObject("toolStripLabel_searching.Image")));
                this.toolStripLabel_searching.ToolTipText = "正在检索...";
            }
            else
            {
                this.toolStripLabel_searching.Image = null;
                this.toolStripLabel_searching.ToolTipText = "";
            }
        }

        // 观察检索点
        private void MenuItem_viewAccessPoint_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                DetailForm detail = (DetailForm)this.ActiveMdiChild;

                detail.ViewAccessPoint(null);
            }
        }

        // 保存到模板
        private void MenuItem_saveToTemplate_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                DetailForm detail = (DetailForm)this.ActiveMdiChild;

                detail.SaveToTemplate();
            }
        }

        // 自动创建数据
        private void MenuItem_autoGenerate_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                DetailForm detail = (DetailForm)this.ActiveMdiChild;

                detail.AutoGenerate();
            }

        }

        // 查重
        private void MenuItem_dup_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                DetailForm detail = (DetailForm)this.ActiveMdiChild;

                detail.SearchDup(null);
            }
        }

        // 清除配置文件本地缓存
        private void MenuItem_clearCfgCache_Click(object sender, EventArgs e)
        {
            cfgCache.Clear();

            // 为简单起见，清除DOM缓存的功能也在这里调用了
            this.DomCache.Clear();
        }

        // 管理种次号
        private void MenuItem_manageZhongcihao_Click(object sender, EventArgs e)
        {
            ZhongcihaoDlg dlg = new ZhongcihaoDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            SearchPanel searchpanel = new SearchPanel();
            searchpanel.Initial(this.Servers,
                this.cfgCache);

            searchpanel.ap = this.AppInfo;
            searchpanel.ApCfgTitle = "mainform_zhongcihaodlg";

            // dlg.TopMost = true;
            dlg.OpenDetail -= new OpenDetailEventHandler(this.OpenDetailCallBack);
            dlg.OpenDetail += new OpenDetailEventHandler(this.OpenDetailCallBack);

            // 获得上次遗留的URL
            string strLastUrl = this.AppInfo.GetString(
                "zhongcihao",
                "url",
                "http://dp2003.com/dp2kernel");

            dlg.Closed -= new EventHandler(zhongcihaodlg_Closed);
            dlg.Closed += new EventHandler(zhongcihaodlg_Closed);


            this.AppInfo.LinkFormState(dlg, "zhongcihaodlg_state");

            dlg.Initial(searchpanel,
                strLastUrl,
                "中文书目",
                "",
                false);
            dlg.Show();
            dlg.MdiParent = this;
        }

        // 回调函数，保存使用过的Url
        private void zhongcihaodlg_Closed(object sender,
            EventArgs e)
        {
            ZhongcihaoDlg dlg = (ZhongcihaoDlg)sender;
            try
            {
                this.AppInfo.SetString(
                    "zhongcihao",
                    "url",
                    dlg.ServerUrl);
                // this.applicationInfo.UnlinkFormState(dlg);


            }
            catch
            {
            }
        }

        // 公用的SearchPanel
        public SearchPanel SearchPanel
        {
            get
            {
                SearchPanel searchpanel = new SearchPanel();
                searchpanel.Initial(this.Servers,
                    this.cfgCache);

                searchpanel.ap = this.AppInfo;
                searchpanel.ApCfgTitle = "mainform_searchpanel";

                return searchpanel;
            }
        }

        // 分类主题对照
        private void MenuItem_class2Subject_Click(object sender, EventArgs e)
        {
            Class2SubjectDlg dlg = new Class2SubjectDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            // dlg.TopMost = true;
            dlg.CopySubject += new CopySubjectEventHandler(this.CopySubject);

            SearchPanel searchpanel = new SearchPanel();
            searchpanel.Initial(this.Servers,
                this.cfgCache);

            searchpanel.ap = this.AppInfo;
            searchpanel.ApCfgTitle = "mainform_class2subjectdlg";

            // 获得上次遗留的URL
            string strLastUrl = this.AppInfo.GetString(
                "class2subject",
                "url",
                "http://dp2003.com/dp2kernel");

            dlg.Closed -= new EventHandler(class2subjectdlg_Closed);
            dlg.Closed += new EventHandler(class2subjectdlg_Closed);

            this.AppInfo.LinkFormState(dlg, "class2subjectdlg_state");

            dlg.Initial(searchpanel,
                strLastUrl, // "http://dp2003.com/rmsws/rmsws.asmx",
                "分类主题对照");
            dlg.CssUrl = Environment.CurrentDirectory + "\\class2subject.css";
            dlg.Show();
            dlg.MdiParent = this;
        }

        // 回调函数，保存使用过的Url
        private void class2subjectdlg_Closed(object sender,
            EventArgs e)
        {
            Class2SubjectDlg dlg = (Class2SubjectDlg)sender;
            try
            {
                this.AppInfo.SetString(
                    "class2subject",
                    "url",
                    dlg.ServerUrl);
                // this.applicationInfo.UnlinkFormState(dlg);

            }
            catch
            {
            }
        }

        /*
        private void MenuItem_tileHorizontal_Click(object sender, EventArgs e)
        {

        }

        private void MenuItem_tileVertical_Click(object sender, EventArgs e)
        {

        }

        private void MenuItem_cascade_Click(object sender, EventArgs e)
        {

        }

        private void MenuItem_arrangeIcons_Click(object sender, EventArgs e)
        {

        }
        */

        // 有关MDI子窗口排列的菜单命令
        private void MenuItem_mdi_arrange_Click(object sender, System.EventArgs e)
        {
            // 平铺 水平方式
            if (sender == MenuItem_tileHorizontal)
                this.LayoutMdi(MdiLayout.TileHorizontal);

            if (sender == MenuItem_tileVertical)
                this.LayoutMdi(MdiLayout.TileVertical);

            if (sender == MenuItem_cascade)
                this.LayoutMdi(MdiLayout.Cascade);

            if (sender == MenuItem_arrangeIcons)
                this.LayoutMdi(MdiLayout.ArrangeIcons);

        }

        // 版权
        private void MenuItem_copyright_Click(object sender, EventArgs e)
        {
            CopyrightDlg dlg = new CopyrightDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

        }

        // 系统参数配置
        private void MenuItem_preference_Click(object sender, EventArgs e)
        {
            ManagePreference();
        }

        void ManagePreference()
        {
            PreferenceDlg dlg = new PreferenceDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.ap = this.AppInfo;
            dlg.MainForm = this;
            dlg.ShowDialog(this);
        }

        // 设置字体
        private void MenuItem_font_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is DetailForm)
            {
                DetailForm detail = (DetailForm)this.ActiveMdiChild;

                detail.SetFont();
            }
        }

        public void SetMenuItemState()
        {
            // 菜单
            MenuItem_properties.Enabled = false;
            MenuItem_viewAccessPoint.Enabled = false;
            MenuItem_dup.Enabled = false;

            MenuItem_save.Enabled = false;
            MenuItem_saveas.Enabled = false;
            MenuItem_saveasToDB.Enabled = false;
            MenuItem_saveToTemplate.Enabled = false;
            MenuItem_autoGenerate.Enabled = false;

            MenuItem_font.Enabled = false;

            // 工具条按钮
            toolBarButton_save.Enabled = false;
            toolBarButton_refresh.Enabled = false;
            toolBarButton_delete.Enabled = false;
            toolBarButton_prev.Enabled = false;
            toolBarButton_next.Enabled = false;
            toolBarButton_loadTemplate.Enabled = false;
            toolBarButton_search.Enabled = false;
            ToolStripMenuItem_searchKeyID.Enabled = false;
        }

        private void MainFormNew_MdiChildActivate(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild == null)
            {
                SetMenuItemState();
            }
        }

        // 当前顶层的DetailForm
        public DetailForm TopDetailForm
        {
            get
            {
                return (DetailForm)GetTopChildWindow(typeof(DetailForm));
            }

        }

        // 当前顶层的SearchForm
        public SearchForm TopSearchForm
        {
            get
            {
                return (SearchForm)GetTopChildWindow(typeof(SearchForm));
            }

        }

        // 当前顶层的ViewAccessPointForm
        public ViewAccessPointForm TopViewAccessPointForm
        {
            get
            {
                return (ViewAccessPointForm)GetTopChildWindow(typeof(ViewAccessPointForm));
            }

        }

        // 得到特定类型的顶层MDI窗口
        Form GetTopChildWindow(Type type)
        {
            if (ActiveMdiChild == null)
                return null;

            // 得到顶层的MDI Child
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

        public void OpenDetailCallBack(object sender, OpenDetailEventArgs e)
        {
            // ZhongcihaoDlg dlg = (ZhongcihaoDlg)sender;

            for (int i = 0; i < e.Paths.Length; i++)
            {
                DetailForm child = null;

                if (!(Control.ModifierKeys == Keys.Control))
                    child = this.TopDetailForm;

                if (child == null)
                {
                    child = new DetailForm();
                    child.MdiParent = this;
                    child.Show();
                }
                else
                {
                    child.Activate();
                }


                child.LoadRecord(e.Paths[i], null);
            }

        }

        private void toolStrip_main_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == toolBarButton_stop)
                stopManager.DoStopActive();

            if (e.ClickedItem == toolBarButton_save)
            {
                if (this.ActiveMdiChild is DetailForm)
                {
                    ((DetailForm)this.ActiveMdiChild).SaveRecord(null);
                }
            }

            if (e.ClickedItem == toolBarButton_refresh)
            {
                if (this.ActiveMdiChild is DetailForm)
                {
                    ((DetailForm)this.ActiveMdiChild).LoadRecord(null, null);
                }

            }

            if (e.ClickedItem == toolBarButton_loadTemplate)
            {
                if (this.ActiveMdiChild is DetailForm)
                {
                    ((DetailForm)this.ActiveMdiChild).LoadTemplate();
                }

            }

            if (e.ClickedItem == toolBarButton_delete)
            {
                if (this.ActiveMdiChild is DetailForm)
                {
                    ((DetailForm)this.ActiveMdiChild).DeleteRecord(null);
                }

            }

            if (e.ClickedItem == toolBarButton_prev)
            {
                if (this.ActiveMdiChild is DetailForm)
                {
                    DetailForm detail = (DetailForm)this.ActiveMdiChild;
                    detail.LoadRecord(null, "prev");
                }

            }
            if (e.ClickedItem == toolBarButton_next)
            {
                if (this.ActiveMdiChild is DetailForm)
                {
                    DetailForm detail = (DetailForm)this.ActiveMdiChild;
                    detail.LoadRecord(null, "next");
                }
            }

            if (e.ClickedItem == this.toolBarButton_search)
            {
                if (this.ActiveMdiChild is SearchForm)
                {
                    ((SearchForm)this.ActiveMdiChild).DoSearch(false);
                }
            }
        }


        // 回调 复制主题词。复制到剪贴板
        void CopySubject(object sender, CopySubjectEventArgs e)
        {
            Clipboard.SetDataObject(e.Subject);
            /*
            Class2SubjectDlg dlg = (Class2SubjectDlg)sender;
            dlg.DialogResult = DialogResult.OK;
            dlg.Close();
            */
        }

        private void MenuItem_registerItems_Click(object sender, EventArgs e)
        {
            RegisterBarcodeDlg dlg = new RegisterBarcodeDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            // 装载上次运行的遗留参数
            dlg.ServerUrl = this.AppInfo.GetString(
                "registerbarcode",
                "serverurl",
                "");
            dlg.BiblioDbName = this.AppInfo.GetString(
                "registerbarcode",
                "bibliodbname",
                "");
            dlg.ItemDbName = this.AppInfo.GetString(
                "registerbarcode",
                "itemdbname",
                "");
            dlg.SearchPanel = this.SearchPanel;
            dlg.SearchOnly = false;

            dlg.OpenDetail -= new OpenDetailEventHandler(this.OpenDetailCallBack);
            dlg.OpenDetail += new OpenDetailEventHandler(this.OpenDetailCallBack);


            dlg.ShowDialog();

            // 记忆本次的参数
            this.AppInfo.SetString(
                "registerbarcode",
                "serverurl",
                dlg.ServerUrl);
            this.AppInfo.SetString(
                "registerbarcode",
                "bibliodbname",
                dlg.BiblioDbName);
            this.AppInfo.SetString(
                "registerbarcode",
                "itemdbname",
                dlg.ItemDbName);
        }

        private void MenuItem_itemSearch_Click(object sender, EventArgs e)
        {
            RegisterBarcodeDlg dlg = new RegisterBarcodeDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            // 装载上次运行的遗留参数
            dlg.ServerUrl = this.AppInfo.GetString(
                "registerbarcode",
                "serverurl",
                "");
            dlg.BiblioDbName = this.AppInfo.GetString(
                "registerbarcode",
                "bibliodbname",
                "");
            dlg.ItemDbName = this.AppInfo.GetString(
                "registerbarcode",
                "itemdbname",
                "");
            dlg.SearchPanel = this.SearchPanel;
            dlg.SearchOnly = true;

            dlg.OpenDetail -= new OpenDetailEventHandler(this.OpenDetailCallBack);
            dlg.OpenDetail += new OpenDetailEventHandler(this.OpenDetailCallBack);


            dlg.ShowDialog();

            // 记忆本次的参数
            this.AppInfo.SetString(
                "registerbarcode",
                "serverurl",
                dlg.ServerUrl);
            this.AppInfo.SetString(
                "registerbarcode",
                "bibliodbname",
                dlg.BiblioDbName);
            this.AppInfo.SetString(
                "registerbarcode",
                "itemdbname",
                dlg.ItemDbName);
        }

        private void MenuItem_downloadPinyinXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            DownloadDataFile("pinyin.xml", out strError);
            MessageBox.Show(this, strError);
        }

        // 下载数据文件
        public int DownloadDataFile(string strFileName,
            out string strError)
        {
            strError = "";

            WebClient webClient = new WebClient();

            string strUrl = "http://dp2003.com/dp2rms/" + strFileName;
            string strLocalFileName = this.DataDir + "\\" + strFileName;
            try
            {
                webClient.DownloadFile(strUrl,
                    strLocalFileName);
            }
            catch (Exception ex)
            {
                strError = "下载" + strFileName + "文件发生错误 :" + ex.Message;
                return -1;
            }

            strError = "下载" + strFileName + "文件成功 :\r\n" + strUrl + " --> " + strLocalFileName;
            return 0;
        }

        private void MenuItem_downloadIsbnXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            DownloadDataFile("rangemessage.xml", out strError);   // "isbn.xml"
            MessageBox.Show(this, strError);

        }

        private void MenuItem_isbnAddHypen_Click(object sender, EventArgs e)
        {
            IsbnHyphenDlg dlg = new IsbnHyphenDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.MainForm = this;
            dlg.ShowDialog(this);
        }

        private void ToolStripMenuItem_searchKeyID_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is SearchForm)
            {
                ((SearchForm)this.ActiveMdiChild).DoSearch(true);
            }
        }



        void SetFirstDefaultFont()
        {
            if (this.DefaultFont != null)
                return;

            try
            {
                FontFamily family = new FontFamily("微软雅黑");
            }
            catch
            {
                return;
            }
            this.DefaultFontString = "微软雅黑, 9pt";
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
                    return null;

                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strDefaultFontString);
            }
        }

        // parameters:
        //      bForce  是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置
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
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                Font subfont = sub.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    sub.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);


                    // sub.Font = new Font(font, subfont.Style);
                }

                if (sub is ToolStrip)
                {
                    ChangeDifferentFaceFont((ToolStrip)sub, font);
                }

                // 递归
                ChangeDifferentFaceFont(sub, font);
            }
        }

        static void ChangeDifferentFaceFont(ToolStrip tool,
    Font font)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            for (int i = 0; i < tool.Items.Count; i++)
            {
                ToolStripItem item = tool.Items[i];

                Font subfont = item.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    // item.Font = new Font(font, subfont.Style);
                    item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
                }
            }
        }

        // 保存分割条位置
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

        // 获得并设置分割条位置
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
    }
}