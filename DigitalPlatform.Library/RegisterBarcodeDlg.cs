using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Web;
using System.Reflection;

using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 册条码号登录对话框
    /// </summary>
    public partial class RegisterBarcodeDlg : Form
    {
        // Assembly AssemblyFilter = null;
        MyFilterDocument filter = null;
        ScriptManager scriptManager = new ScriptManager();

        /// <summary>
        /// 结果字符串
        /// </summary>
        public string ResultString = "";


        BrowseSearchResultDlg browseWindow = null;

        /// <summary>
        /// 
        /// </summary>
        public SearchPanel SearchPanel = null;

        string m_strServerUrl = "";
        string m_strBiblioDbName = "";
        string m_strItemDbName = "";

        string m_strBiblioRecPath = "";

        /// <summary>
        /// 语言代码
        /// </summary>
        public string Lang = "zh";

        /// <summary>
        /// 册事项集合
        /// </summary>
        public BookItemCollection Items = null;

        bool m_bSearchOnly = false; // 是否只为册检索服务、不支持册登录功能

        XmlDocument dom = null; // cfgs/global配置文件

        /// <summary>
        /// 打开详细窗
        /// </summary>
        public event OpenDetailEventHandler OpenDetail = null;


        /// <summary>
        /// 是否只检索不登录
        /// </summary>
        public bool SearchOnly
        {
            get
            {
                return m_bSearchOnly;
            }
            set
            {
                m_bSearchOnly = value;
                if (m_bSearchOnly == true)
                {
                    this.button_register.Text = "检索(&S)";
                    this.button_save.Enabled = false;
                    this.Text = "典藏册检索";
                }
                else
                {
                    this.button_register.Text = "登记(&R)";
                    this.button_save.Enabled = true;
                    this.Text = "典藏册登录";
                }
            }
        }

        /// <summary>
        /// 服务器URL
        /// </summary>
        public string ServerUrl
        {
            get
            {
                return this.m_strServerUrl;
            }
            set
            {
                this.m_strServerUrl = value;
                UpdateTargetInfo();
            }
        }

        /// <summary>
        /// 书目库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return this.m_strBiblioDbName;
            }
            set
            {
                this.m_strBiblioDbName = value;
                UpdateTargetInfo();
            }
        }

        /// <summary>
        /// 实体库名
        /// </summary>
        public string ItemDbName
        {
            get
            {
                return this.m_strItemDbName;
            }
            set
            {
                this.m_strItemDbName = value;
                UpdateTargetInfo();
            }
        }

        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string BiblioRecPath
        {
            get
            {
                return this.m_strBiblioRecPath;
            }
            set
            {
                this.m_strBiblioRecPath = value;
                this.label_biblioRecPath.Text = "种记录路径: " + value;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public RegisterBarcodeDlg()
        {
            InitializeComponent();
        }

        private void RegisterBarcodeDlg_Load(object sender, EventArgs e)
        {
            UpdateTargetInfo();

            FillFromList();

        }

        private void button_target_Click(object sender, EventArgs e)
        {
            GetLinkDbDlg dlg = new GetLinkDbDlg();

            dlg.SearchPanel = this.SearchPanel;
            dlg.ServerUrl = this.ServerUrl;
            dlg.BiblioDbName = this.BiblioDbName;
            dlg.ItemDbName = this.ItemDbName;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.ServerUrl = dlg.ServerUrl;
            this.BiblioDbName = dlg.BiblioDbName;
            this.ItemDbName = dlg.ItemDbName;

            FillFromList();
        }

        // 保存
        private void button_save_Click(object sender, EventArgs e)
        {
            string strError = "";

            EnableControls(false);
            int nRet = this.SaveItems(out strError);
            EnableControls(true);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "册信息保存成功。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;

        }

        // 更新目标有关的显示信息
        void UpdateTargetInfo()
        {
            this.label_target.Text = "服务器: " + this.ServerUrl + "\r\n书目库: " + this.BiblioDbName + ";    实体库: " + this.ItemDbName;
        }

        // 填充from列表
        void FillFromList()
        {
            this.comboBox_from.Items.Clear();


            if (this.ServerUrl == "")
                return;

            if (this.BiblioDbName == "")
                return;


            string strOldSelectedItem = this.comboBox_from.Text;
            this.comboBox_from.Text = "";
            RmsChannel channel = this.SearchPanel.Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            string[] items = null;

            string strError = "";

            long lRet = channel.DoDir(this.BiblioDbName,
                this.Lang,
                null,   // 不需要列出全部语言的名字
                ResTree.RESTYPE_FROM,
                out items,
                out strError);

            if (lRet == -1)
            {
                strError = "列 '" + this.BiblioDbName + "' 库的检索途径时发生错误: " + strError;
                goto ERROR1;
            }

            bool bFoundOldItem = false;
            for (int i = 0; i < items.Length; i++)
            {
                if (strOldSelectedItem == items[i])
                    bFoundOldItem = true;
                this.comboBox_from.Items.Add(items[i]);
            }

            if (bFoundOldItem == true)
                this.comboBox_from.Text = strOldSelectedItem;
            else
            {
                if (this.comboBox_from.Items.Count > 0)
                    this.comboBox_from.Text = (string)this.comboBox_from.Items[0];
            }

            return;

        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        static bool IsISBnBarcode(string strText)
        {
            if (strText.Length == 13)
            {
                string strHead = strText.Substring(0, 3);
                if (strHead == "978")
                    return true;
            }

            return false;
        }

        string GetQueryString()
        {
            string strText = this.textBox_queryWord.Text;
            int nRet = strText.IndexOf(';');
            if (nRet != -1)
            {
                strText = strText.Substring(0, nRet).Trim();
                this.textBox_queryWord.Text = strText;
            }

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
            }

            return strText;
        }

        /// <summary>
        /// 检索出书目数据
        /// </summary>
        /// <param name="strError"></param>
        /// <returns></returns>
        public long SearchBiblio(out string strError)
        {
            this.SearchPanel.BeginLoop("正在检索 " + this.textBox_queryWord.Text + " ...");
            try
            {
                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.BiblioDbName + ":" + this.comboBox_from.Text)        // 2007/9/14
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(GetQueryString())
                    + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + this.Lang + "</lang></target>";


                ActivateBrowseWindow();

                long lRet = 0;

                this.SearchPanel.BrowseRecord += new BrowseRecordEventHandler(SearchPanel_BrowseRecord);

                try
                {
                    // 检索
                    lRet = this.SearchPanel.SearchAndBrowse(
                        this.ServerUrl,
                        strQueryXml,
                        true,
                        out strError);
                }
                finally
                {
                    this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(SearchPanel_BrowseRecord);
                }


                if (lRet == 1 && this.browseWindow != null)
                {
                    this.browseWindow.LoadFirstDetail(true);
                }

                if (lRet == 0)
                {
                    strError = "没有命中。";
                }


                if (lRet == 0 || lRet == -1)
                {
                    this.browseWindow.Close();
                    this.browseWindow = null;

                }


                return lRet;

            }
            finally
            {
                this.SearchPanel.EndLoop();
            }

        }

        void ActivateBrowseWindow()
        {
            if (this.browseWindow == null
                || (this.browseWindow != null && this.browseWindow.IsDisposed == true))
            {
                this.browseWindow = new BrowseSearchResultDlg();
                this.browseWindow.Text = "命中多条种记录。请从中选择一条";
                this.browseWindow.Show();

                this.browseWindow.OpenDetail -= new OpenDetailEventHandler(browseWindow_OpenDetail);
                this.browseWindow.OpenDetail += new OpenDetailEventHandler(browseWindow_OpenDetail);
            }
            else
            {
                this.browseWindow.BringToFront();
                this.browseWindow.RecordsList.Items.Clear();
            }
        }

        void SearchPanel_BrowseRecord(object sender, BrowseRecordEventArgs e)
        {
            this.browseWindow.NewLine(e.FullPath,
                e.Cols);
        }

        // 装入详细记录
        void browseWindow_OpenDetail(object sender, OpenDetailEventArgs e)
        {
            if (e.Paths.Length == 0)
                return;

            ResPath respath = new ResPath(e.Paths[0]);

            string strError = "";
            int nRet = LoadBiblioRecord(
                respath.Path,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        int LoadBiblioRecord(
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            this.BiblioRecPath = strBiblioRecPath;

            string strMarcXml = "";
            byte[] baTimeStamp = null;

            int nRet = this.SearchPanel.GetRecord(
                this.ServerUrl,
                this.BiblioRecPath,
                out strMarcXml,
                out baTimeStamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 转换为MARC格式

            string strOutMarcSyntax = "";
            string strMarc = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strMarcXml,
                true,
                "", // this.CurMarcSyntax,
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.filter == null)
            {
                string strCfgFilePath = this.BiblioDbName + "/cfgs/html.fltx";
                string strContent = "";
                // 获得配置文件
                // return:
                //		-1	error
                //		0	not found
                //		1	found
                nRet = this.SearchPanel.GetCfgFile(
                    this.ServerUrl,
                    strCfgFilePath,
                    out strContent,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "在服务器 " + this.ServerUrl + " 上没有找到配置文件 '" + strCfgFilePath + "' ，因此只能以MARC工作单格式显示书目信息...";
                    string strText = strMarc.Replace((char)31, '^');
                    strText = strText.Replace(new string((char)30, 1), "<br/>");
                    this.HtmlString = strText;
                    goto ERROR1;
                }


                MyFilterDocument tempfilter = null;

                nRet = PrepareMarcFilter(
                    strContent,
                    //Environment.CurrentDirectory + "\\marc.fltx",
                    out tempfilter,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.filter = tempfilter;
            }

            this.ResultString = "";

            // 触发filter中的Record相关动作
            nRet = this.filter.DoRecord(
                null,
                strMarc,
                0,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.HtmlString = this.ResultString;

            // 装载册信息
            nRet = LoadItems(this.BiblioRecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;

        ERROR1:
            return -1;
        }

        // 清除以前残余的信息
        void Clear()
        {
            this.HtmlString = "(blank)";
            this.listView_items.Items.Clear();
            this.BiblioRecPath = "";
        }

        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 清除以前残余的信息
            /*
            this.HtmlString = "(blank)";
            this.listView_items.Items.Clear();
            this.BiblioRecPath = "";
             */
            this.Clear();

            EnableControls(false);
            long nRet = SearchBiblio(out strError);
            EnableControls(true);
            if (nRet == 0 || nRet == -1)
                goto ERROR1;

            this.textBox_queryWord.SelectAll();

            this.textBox_itemBarcode.Focus();   // 焦点切换到条码号textbox
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            this.textBox_queryWord.Focus();
            this.textBox_queryWord.SelectAll();
            return;
        }

        /// <summary>
        /// HTML字符串
        /// </summary>
        public string HtmlString
        {
            get
            {
                HtmlDocument doc = this.webBrowser_record.Document;

                if (doc == null)
                    return "";

                HtmlElement item = doc.All["html"];
                if (item == null)
                    return "";

                return item.OuterHtml;

            }
            set
            {
                // this.webBrowser_record.Navigate("about:blank");


                HtmlDocument doc = this.webBrowser_record.Document;

                if (doc == null)
                {
                    this.webBrowser_record.Navigate("about:blank");
                    doc = this.webBrowser_record.Document;
                }

                doc = doc.OpenNew(true);
                doc.Write(value);
            }
        }


        int PrepareMarcFilter(
            string strFilterFileContent,
            out MyFilterDocument filter,
            out string strError)
        {
            filter = new MyFilterDocument();

            filter.HostForm = this;
            filter.strOtherDef = "RegisterBarcodeDlg HostForm = null;";

            filter.strPreInitial = " MyFilterDocument doc = (MyFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " HostForm = ("
                + "RegisterBarcodeDlg" + ")doc.HostForm;\r\n";

            // filter.Load(strFilterFileName);
            filter.LoadContent(strFilterFileContent);

            string strCode = "";    // c#代码

            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string[] saAddRef1 = {
										 Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.library.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
										 Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
										 Environment.CurrentDirectory + "\\dp2rms.exe",
										 /*strMainCsDllName*/ };

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
                MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assembly;

            return 0;
        ERROR1:
            return -1;
        }

        private void textBox_queryWord_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_search;
        }

        private void textBox_itemBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_register;
        }

        private void button_register_Click(object sender, EventArgs e)
        {
            if (m_bSearchOnly == true)
            {
                this.DoSearchFromBarcode(true);
            }
            else
            {
                this.DoRegisterBarcode();
            }
        }

        // 通过册条码号检索察看相关种册信息
        // 是否可以优化为：先看看当前窗口中是否有了要检索的条码号？不过这也等于放弃了查重功能。
        void DoSearchFromBarcode(bool bDetectDup)
        {
            string strError = "";
            // string strBiblioRecPath = "";
            // string strItemRecPath = "";


            EnableControls(false);

            try
            {

                this.Clear();

                List<DoublePath> paths = null;

                int nRet = GetLinkInfoFromBarcode(
                    this.textBox_itemBarcode.Text,
                    true,
                    out paths,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    goto NOTFOUND;
                }

                DoublePath dpath = null;

                if (nRet > 1)
                {
                    // MessageBox.Show(this, "册条码号 " + this.textBox_itemBarcode.Text + "　出现重复现象，请及时解决。");
                    SelectDupItemRecordDlg dlg = new SelectDupItemRecordDlg();
                    dlg.MessageText = "册条码号 " + this.textBox_itemBarcode.Text + "　出现重复现象，这将会导致业务功能出现故障。\r\n\r\n请选择当前希望观察的一条册记录。";
                    dlg.Paths = paths;
                    dlg.ShowDialog(this);

                    if (dlg.DialogResult != DialogResult.OK)
                        return; // 放弃操作

                    dpath = dlg.SelectedDoublePath;
                }
                else
                {
                    dpath = paths[0];
                }

                this.BiblioDbName = ResPath.GetDbName(dpath.BiblioRecPath);
                this.ItemDbName = ResPath.GetDbName(dpath.ItemRecPath);

                nRet = LoadBiblioRecord(
                    dpath.BiblioRecPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 突出显示所命中的册信息行
                ListViewItem listitem = this.GetListViewItem(
                    this.textBox_itemBarcode.Text,
                    dpath.ItemRecPath);

                if (listitem == null)
                {
                    strError = "册条码号为 '" + this.textBox_itemBarcode.Text
                        + "' 册记录路径为 '" + dpath.ItemRecPath + "' 的ListViewItem居然不存在 ...";
                    goto ERROR1;
                }

                listitem.Selected = true;
                listitem.Focused = true;
                this.listView_items.EnsureVisible(this.listView_items.Items.IndexOf(listitem));

            }
            finally
            {
                EnableControls(true);
            }



            this.textBox_itemBarcode.SelectAll();
            this.textBox_itemBarcode.Focus();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        NOTFOUND:
            MessageBox.Show(this, "册条码号 " + this.textBox_itemBarcode.Text + " 没有找到对应的记录。");
            this.textBox_itemBarcode.SelectAll();
            this.textBox_itemBarcode.Focus();
            return;
        }

        // 获得cfgs/global配置文件
        int GetGlobalCfgFile(out string strError)
        {
            strError = "";

            if (this.dom != null)
                return 0;	// 优化

            if (this.ServerUrl == "")
            {
                strError = "尚未指定服务器URL";
                return -1;
            }

            string strCfgFilePath = "cfgs/global";
            XmlDocument tempdom = null;
            // 获得配置文件
            // return:
            //		-1	error
            //		0	not found
            //		1	found
            int nRet = this.SearchPanel.GetCfgFile(
                this.ServerUrl,
                strCfgFilePath,
                out tempdom,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "配置文件 '" + strCfgFilePath + "' 没有找到...";
                return -1;
            }

            this.dom = tempdom;

            return 0;
        }


        // 根据册条码号在一系列可能的实体库中检索出册信息，
        // 然后提取出有关种的信息
        int GetLinkInfoFromBarcode(string strBarcode,
            bool bDetectDup,
            out List<DoublePath> paths,
            out string strError)
        {
            strError = "";
            // strBiblioRecPath = "";
            // strItemRecPath = "";

            paths = new List<DoublePath>();

            string strBiblioRecPath = "";
            string strItemRecPath = "";

            // 获得cfgs/global配置文件
            int nRet = GetGlobalCfgFile(out strError);
            if (nRet == -1)
                return -1;

            // 列出所有<dblink>配置事项
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//dblink");
            if (nodes.Count == 0)
            {
                strError = "cfgs/global配置文件中，尚未配置任何<dblink>元素。";
                return -1;
            }


            this.SearchPanel.BeginLoop("正在检索 " + strBarcode + " 所对应的册信息...");
            try
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strBiblioDbName = DomUtil.GetAttr(node, "bibliodb");
                    string strItemDbName = DomUtil.GetAttr(node, "itemdb");

                    // 2007/4/5 改造 加上了 GetXmlStringSimple()
                    string strQueryXml = "<target list='"
                        + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "册条码")       // 2007/9/14
                        + "'><item><word>"
                        + StringUtil.GetXmlStringSimple(strBarcode)
                        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + this.Lang + "</lang></target>";

                    // strItemRecPath = "";

                    nRet = this.SearchPanel.SearchMultiPath(
                        this.ServerUrl,
                        strQueryXml,
                        1000,
                        out List<string> aPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        continue;

                    for (int j = 0; j < aPath.Count; j++)
                    {
                        strItemRecPath = aPath[j];

                        XmlDocument tempdom = null;
                        byte[] baTimestamp = null;
                        // 提取册记录
                        nRet = this.SearchPanel.GetRecord(
                            this.ServerUrl,
                            strItemRecPath,
                            out tempdom,
                            out baTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "提取册记录 " + strItemRecPath + " 时发生错误：" + strError;
                            return -1;
                        }

                        strBiblioRecPath = strBiblioDbName + "/" + DomUtil.GetElementText(tempdom.DocumentElement, "parent");

                        DoublePath dpath = new DoublePath();
                        dpath.ItemRecPath = strItemRecPath;
                        dpath.BiblioRecPath = strBiblioRecPath;

                        paths.Add(dpath);
                    }

                    // 如果不需要查重，则遇到命中后尽快退出循环
                    if (bDetectDup == false && paths.Count >= 1)
                        return paths.Count;
                }

                return paths.Count;
            }
            finally
            {
                this.SearchPanel.EndLoop();
            }
        }

        // 册登录
        // 根据输入的册条码号新增一册信息，或者定位到已经存在的行
        void DoRegisterBarcode()
        {
            string strError = "";
            int nRet;

            if (this.textBox_itemBarcode.Text == "")
            {
                strError = "尚未输入册条码号";
                goto ERROR1;
            }

            // 看看输入的条码号是否为ISBN条码号
            if (IsISBnBarcode(this.textBox_itemBarcode.Text) == true)
            {
                // 保存当前册信息
                EnableControls(false);
                nRet = this.SaveItems(out strError);
                EnableControls(true);
                if (nRet == -1)
                    goto ERROR1;

                // 转而触发新种检索操作
                this.textBox_queryWord.Text = this.textBox_itemBarcode.Text;
                this.textBox_itemBarcode.Text = "";

                this.button_search_Click(null, null);
                return;
            }


            if (this.Items == null)
                this.Items = new BookItemCollection();

            // 种内查重
            BookItem item = this.Items.GetItem(this.textBox_itemBarcode.Text);

            ListViewItem listitem = null;

            // 如果该册条码号的事项已经存在
            if (item != null)
            {
                listitem = this.GetListViewItem(this.textBox_itemBarcode.Text,
                    null);

                if (listitem == null)
                {
                    strError = "册条码号为 '" + this.textBox_itemBarcode.Text + "'的BookItem内存事项存在，但是没有对应的ListViewItem ...";
                    goto ERROR1;
                }

                UnselectListViewItems();
                listitem.Selected = true;
                listitem.Focused = true;
                this.listView_items.EnsureVisible(this.listView_items.Items.IndexOf(listitem));
                goto END1;
            }

            List<DoublePath> paths = null;

            // 种外全面查重
            nRet = GetLinkInfoFromBarcode(
                this.textBox_itemBarcode.Text,
                true,
                out paths,
                out strError);
            if (nRet == -1)
            {
                strError = "册条码号查重操作出现错误：" + strError;
                goto ERROR1;
            }

            if (nRet > 0)
            {
                // MessageBox.Show(this, "册条码号 " + this.textBox_itemBarcode.Text + "　出现重复现象，请及时解决。");
                SelectDupItemRecordDlg dlg = new SelectDupItemRecordDlg();
                dlg.MessageText = "册条码号 " + this.textBox_itemBarcode.Text + " 已经被登录过了，情况如下。\r\n\r\n如果想详细观察，请选择当前希望观察的一条册记录；否则请按“取消”按钮。";
                dlg.Paths = paths;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return; // 放弃操作

                if (this.BiblioRecPath != dlg.SelectedDoublePath.BiblioRecPath)
                {
                    MessageBox.Show(this, "请注意软件即将自动装入新种 " + dlg.SelectedDoublePath.BiblioRecPath + " 到窗口中，如稍候您想继续对原种 " + this.BiblioRecPath + " 进行册登录，请切记重新装入原种后再行册登录 ...");
                }

                // 先保存本种
                nRet = this.SaveItems(out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_queryWord.Text = "";   // qingchu yuanlai de jiansuoci , bimian wuhui 

                DoublePath dpath = dlg.SelectedDoublePath;

                this.BiblioDbName = ResPath.GetDbName(dpath.BiblioRecPath);
                this.ItemDbName = ResPath.GetDbName(dpath.ItemRecPath);

                nRet = LoadBiblioRecord(
                    dpath.BiblioRecPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 突出显示所命中的册信息行
                listitem = this.GetListViewItem(
                    this.textBox_itemBarcode.Text,
                    dpath.ItemRecPath);

                if (listitem == null)
                {
                    strError = "册条码号为 '" + this.textBox_itemBarcode.Text
                        + "' 册记录路径为 '" + dpath.ItemRecPath + "' 的ListViewItem居然不存在 ...";
                    goto ERROR1;
                }

                listitem.Selected = true;
                listitem.Focused = true;
                this.listView_items.EnsureVisible(this.listView_items.Items.IndexOf(listitem));

                this.textBox_itemBarcode.SelectAll();
                this.textBox_itemBarcode.Focus();
                return;
            }


            item = new BookItem();

            item.Barcode = this.textBox_itemBarcode.Text;
            item.Changed = true;    // 表示这是新事项

            this.Items.Add(item);

            listitem = item.AddToListView(this.listView_items);

            // 加上选择标记
            UnselectListViewItems();
            listitem.Focused = true;
            this.listView_items.EnsureVisible(this.listView_items.Items.IndexOf(listitem));

        END1:
            this.textBox_itemBarcode.SelectAll();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 在listview中根据册条码号定位一个item事项
        // parameters:
        //      strItemRecPath  册记录路径。如果==null，则表示本参数在定位中不起作用
        ListViewItem GetListViewItem(string strBarcode,
            string strItemRecPath)
        {
            int nColumnIndex = this.listView_items.Columns.IndexOf(columnHeader_recpath);

            for (int i = 0; i < this.listView_items.Items.Count; i++)
            {
                if (strBarcode == this.listView_items.Items[i].Text)
                {
                    if (String.IsNullOrEmpty(strItemRecPath) == true)
                        return this.listView_items.Items[i];

                    if (strItemRecPath == this.listView_items.Items[i].SubItems[nColumnIndex].Text)
                        return this.listView_items.Items[i];
                }
            }

            return null;
        }

        void UnselectListViewItems()
        {
            for (int i = 0; i < this.listView_items.Items.Count; i++)
            {
                this.listView_items.Items[i].Selected = false;
            }

        }

        int LoadItems(string strBiblioRecPath,
            out string strError)
        {
            strError = "";

            this.listView_items.Items.Clear();

            if (String.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                /*
                strError = "strBiblioRecPath参数不能为空";
                return -1;
                 */
                return 0;
            }

            if (this.Items == null)
                this.Items = new BookItemCollection();
            else
                this.Items.Clear();

            // 检索出所有关联到种记录id的册记录
            long lRet = SearchItems(ResPath.GetRecordId(strBiblioRecPath),
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        /// <summary>
        /// 检索出册数据
        /// </summary>
        /// <param name="strBiblioRecId"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public long SearchItems(string strBiblioRecId,
            out string strError)
        {
            this.SearchPanel.BeginLoop("正在检索所有从属于 " + strBiblioRecId + " 的册记录 ...");
            try
            {
                string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(this.ItemDbName + ":" + "父记录")       // 2007/9/14
                    + "'><item><word>"
                    + strBiblioRecId
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + this.Lang + "</lang></target>";

                // 检索
                long lRet = 0;

                this.SearchPanel.BrowseRecord += new BrowseRecordEventHandler(SearchItems_BrowseRecord);

                try
                {
                    lRet = this.SearchPanel.SearchAndBrowse(
                         this.ServerUrl,
                         strQueryXml,
                         false,
                         out strError);
                }
                finally
                {
                    this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(SearchItems_BrowseRecord);
                }

                return lRet;

            }
            finally
            {
                this.SearchPanel.EndLoop();
            }

        }

        // 检索册信息过程中，处理结果集中每条记录的回调函数
        void SearchItems_BrowseRecord(object sender, BrowseRecordEventArgs e)
        {
            ResPath respath = new ResPath(e.FullPath);

            XmlDocument tempdom = null;
            byte[] baTimeStamp = null;
            string strError = "";

            int nRet = this.SearchPanel.GetRecord(
                respath.Url,
                respath.Path,
                out tempdom,
                out baTimeStamp,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            BookItem item = new BookItem(respath.Path, tempdom);

            item.Timestamp = baTimeStamp;
            this.Items.Add(item);

            item.AddToListView(this.listView_items);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存册信息
        // (如何删除册信息还是一个棘手的问题)
        int SaveItems(out string strError)
        {
            strError = "";

            if (this.Items == null)
            {
                strError = "Items尚未初始化";
                return -1;
            }

            for (int i = 0; i < this.Items.Count; i++)
            {
                BookItem item = this.Items[i];

                // 跳过没有修改过的事项
                if (item.Changed == false)
                    continue;

                // 新事项
                if (item.RecPath == "")
                    item.RecPath = this.ItemDbName + "/?";

                if (item.Parent == "")
                {
                    if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
                    {
                        strError = "因BiblioRecPath成员为空，无法构造册信息。";
                        return -1;
                    }
                    item.Parent = ResPath.GetRecordId(this.BiblioRecPath);
                }

                string strXml = "";

                int nRet = item.BuildRecord(
                    out strXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "第 " + Convert.ToString(i + 1) + " 行构造册记录时出错: " + strError;
                    return -1;
                }

                byte[] baOutputTimestamp = null;

                nRet = this.SearchPanel.SaveRecord(
                    this.ServerUrl,
                    item.RecPath,
                    strXml,
                    item.Timestamp,
                    true,
                    out baOutputTimestamp,
                    out strError);
                if (nRet == -1)
                {
                    strError = "第 " + Convert.ToString(i + 1) + " 行保存册记录时出错: " + strError;
                    return -1;
                }
                item.Timestamp = baOutputTimestamp;
                item.Changed = false;
                // 事项颜色会发生变化
                item.RefreshItemColor();

            }

            return 0;
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_itemBarcode.Enabled = bEnable;
            this.textBox_queryWord.Enabled = bEnable;
            this.comboBox_from.Enabled = bEnable;
            this.checkBox_autoDetectQueryBarcode.Enabled = bEnable;
            this.button_register.Enabled = bEnable;
            this.button_save.Enabled = bEnable;
            this.button_search.Enabled = bEnable;
            this.listView_items.Enabled = bEnable;
        }

        private void RegisterBarcodeDlg_Activated(object sender, EventArgs e)
        {

            if (this.browseWindow == null
                || (this.browseWindow != null && this.browseWindow.IsDisposed == true))
            {
            }
            else
            {
                this.browseWindow.BringToFront();
            }
        }

        private void RegisterBarcodeDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Items != null)
            {
                if (this.Items.Changed == true)
                {
                    DialogResult result = MessageBox.Show(this,
    "当前有册信息被修改后尚未保存。\r\n\r\n确实要关闭窗口? ",
    "RegitsterBarcodeDlg",
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
        }

        private void RegisterBarcodeDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 书目记录路径的标签双击
        private void label_biblioRecPath_DoubleClick(object sender, EventArgs e)
        {
            if (this.OpenDetail == null)
                return;

            string[] paths = new string[1];
            paths[0] = ServerUrl + "?" + BiblioRecPath;

            OpenDetailEventArgs args = new OpenDetailEventArgs();
            args.Paths = paths;
            args.OpenNew = true;

            this.label_biblioRecPath.Enabled = false;
            this.OpenDetail(this, args);
            this.label_biblioRecPath.Enabled = true;
        }
    }

    /// <summary>
    /// MARC过滤器特定版本
    /// </summary>
    public class MyFilterDocument : FilterDocument
    {
        /// <summary>
        /// 宿主对话框
        /// </summary>
        public RegisterBarcodeDlg HostForm = null;
    }


}