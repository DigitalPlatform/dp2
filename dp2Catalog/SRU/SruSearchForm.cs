using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace dp2Catalog
{
    public partial class SruSearchForm : MyForm, ISearchForm
    {
        SruConfig _config = null;

        public SruSearchForm()
        {
            InitializeComponent();
        }

        // 下一批记录开始的第一条 ID
        string _numberOfRecords = "";   // 命中数
        string _nextRecordPosition = "";    // 下一批开始 ID
        string _useName = "";
        string _word = "";
        string _recordSchema = "";
        string _serverName = "";

        private async void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";
            EnableControls(false);
            try
            {
                _useName = this.comboBox_use.Text;
                _word = this.textBox_queryWord.Text;
                _serverName = this.comboBox_server.Text;
                _recordSchema = this.comboBox_format.Text;

                string url = await _config.BuildSearchUrl(_serverName,
        _word,
        _useName,
        _recordSchema,
        "1",
        null);
                var result = await WebClientEx.DownloadStringAsync(url);

                FillBrowseList(result.String, true);

                // string url = $"https://bnu.alma.exlibrisgroup.com/view/sru/86BNU_INST?version=1.2&operation=searchRetrieve&recordSchema=marcxml&query=alma.isbn={word}";
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*
    <record>
      <recordSchema>marcxml</recordSchema>
      <recordPacking>xml</recordPacking>
      <recordData>         * 
         * */
        class RecordData
        {
            public string ID { get; set; }  // 记录的唯一标识符
            public string Position { get; set; }    // 记录在当前结果集中的位置

            public string Schema { get; set; }  // marcxml 等
            public string Packing { get; set; } // xml 等
            public string Data { get; set; }    // recordData 元素的 OuterXml

            public string GetData(string format, out string syntax)
            {
                syntax = "";
                if (format == "xml" && this.Packing == "xml")
                    return this.Data;

                if (this.Packing == "xml")
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(this.Data);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());

                    // MARCXML --> MARC
                    if (this.Schema == "marcxml" && format == "marc")
                    {
                        nsmgr.AddNamespace("srw", "http://www.loc.gov/zing/srw/");
                        nsmgr.AddNamespace("marc21", "http://www.loc.gov/MARC21/slim");

                        string marcxml = dom.DocumentElement.SelectSingleNode("//marc21:record", nsmgr).OuterXml;

                        XmlDocument marcxml_dom = new XmlDocument();
                        marcxml_dom.LoadXml(marcxml);

                        int nRet = MarcUtil.Xml2Marc(marcxml_dom,
            true,
            null,
            out syntax,
            out string strMARC,
            out string strError);
                        if (nRet == -1)
                            throw new Exception(strError);
                        return strMARC;
                    }
                }
                return this.Data;
            }
        }

        // 填充浏览列表
        void FillBrowseList(string xml, bool clear_before = false)
        {
            if (clear_before)
                this.listView_browse.Items.Clear();

            // "http://www.loc.gov/MARC21/slim"
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            // 2021/8/4
            FillHitCount(dom);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("srw", "http://www.loc.gov/zing/srw/");
            nsmgr.AddNamespace("marc21", "http://www.loc.gov/MARC21/slim");

            nsmgr.AddNamespace("srw_dc", "info:srw/schema/1/dc-schema");
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

            /*
             *
             *  <recordIdentifier>9910523143603961</recordIdentifier>
                <recordPosition>1</recordPosition>
             * */
            XmlNodeList records = dom.DocumentElement.SelectNodes("//srw:record", nsmgr);
            foreach (XmlElement record in records)
            {
                string schema = record.SelectSingleNode("srw:recordSchema", nsmgr)?.InnerText;
                string packing = record.SelectSingleNode("srw:recordPacking", nsmgr)?.InnerText;

                string id = record.SelectSingleNode("srw:recordIdentifier", nsmgr)?.InnerText;
                string pos = record.SelectSingleNode("srw:recordPosition", nsmgr)?.InnerText;

                List<DigitalPlatform.Marc.NameValueLine> results = null;

                if (schema == "dc")
                {
                    string dcxml = record.SelectSingleNode("//srw_dc:dc", nsmgr).OuterXml;
                    int nRet = MarcTable.ScriptDC("",
                        dcxml,
                        "title,author,publication_area",
                        null,
                        out results,
                        out string strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                }
                else if (schema == "marcxml")
                {
                    string marcxml = record.SelectSingleNode("//marc21:record", nsmgr).OuterXml;

                    XmlDocument marcxml_dom = new XmlDocument();
                    marcxml_dom.LoadXml(marcxml);

                    int nRet = MarcUtil.Xml2Marc(marcxml_dom,
        true,
        null,
        out string marcSyntax,
        out string strMARC,
        out string strError);
                    if (nRet == -1)
                        throw new Exception(strError);


                    if (marcSyntax == "usmarc")
                        nRet = MarcTable.ScriptMarc21("",
                            strMARC,
                            "title,author,publication_area",
                            null,
                            out results,
                            out strError);
                    else if (marcSyntax == "unimarc")
                        nRet = MarcTable.ScriptUnimarc("",
                            strMARC,
                            "title,author,publication_area",
                            null,
                            out results,
                            out strError);
                    else
                        throw new Exception($"未知的 MARC 格式 '{marcSyntax}'");

                    if (nRet == -1)
                        throw new Exception(strError);
                }

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, pos);
                {
                    string data = record.SelectSingleNode("srw:recordData", nsmgr)?.OuterXml;

                    item.Tag = new RecordData
                    {
                        ID = id,
                        Position = pos,
                        Schema = schema,
                        Packing = packing,
                        Data = data
                    };
                }
                FillColumns(item, results);

                this.listView_browse.Items.Add(item);
            }

            this.textBox_resultInfo.Text = $"命中数: {_numberOfRecords}\r\n已装入: {this.listView_browse.Items.Count}";
        }


        /*
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
    <searchRetrieveResponse xmlns="http://www.loc.gov/zing/srw/">
        <version>1.2</version>
        <numberOfRecords>239392</numberOfRecords>
  ...
         * */
        void FillHitCount(XmlDocument dom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("srw", "http://www.loc.gov/zing/srw/");
            nsmgr.AddNamespace("marc21", "http://www.loc.gov/MARC21/slim");

            _numberOfRecords = dom.DocumentElement.SelectSingleNode("srw:numberOfRecords", nsmgr)?.InnerText;
            _nextRecordPosition = dom.DocumentElement.SelectSingleNode("srw:nextRecordPosition", nsmgr)?.InnerText;

            this.textBox_resultInfo.Text = $"命中数: {_numberOfRecords}";
        }

        void FillColumns(ListViewItem item,
            List<DigitalPlatform.Marc.NameValueLine> results)
        {
            string title = "";
            string author = "";
            string publisher = "";

            foreach (var line in results)
            {
                if (line.Type == "title")
                    title = line.Value;
                if (line.Type == "author")
                    author = line.Value;
                if (line.Type == "publication_area")
                    publisher = line.Value;
            }

            ListViewUtil.ChangeItemText(item, 1, title);
            ListViewUtil.ChangeItemText(item, 2, author);
            ListViewUtil.ChangeItemText(item, 3, publisher);
        }

        private void SruSearchForm_Load(object sender, EventArgs e)
        {
            LoadConfig();
            this.UiState = Program.MainForm.AppInfo.GetString(
"srusearchform",
"ui_state",
"");
            /*
            Task.Run(() =>
            {
            });
            */
            string[] formats = new string[] {
                "marcxml",
                "dc",
                "dcx",
                "mods",
                "unimarcxml",
                "kormarcxml",
                "cnmarcxml",
                "isohold",
            };

            this.comboBox_format.Items.AddRange(formats);
        }

        void LoadConfig()
        {
            try
            {
                string fileName = Path.Combine(Program.MainForm.UserDir, "sru\\sru.xml");
                _config = SruConfig.From(fileName);

                var servers = _config.ListTargets("*");
                this.Invoke((Action)(() =>
                {
                    this.comboBox_server.Items.Clear();
                    foreach (var server in servers)
                    {
                        this.comboBox_server.Items.Add(server.Name);
                    }
                }));
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show(this, $"装载 sru.xml 出现异常: {ex.Message}");
                }));
            }
        }

        private void comboBox_use_DropDown(object sender, EventArgs e)
        {
            /*
            if (this.comboBox_use.Items.Count == 0)
            {
                if (string.IsNullOrEmpty(this.comboBox_server.Text))
                    return;
                var targets = _config.ListTargets(this.comboBox_server.Text);
                if (targets.Count == 0)
                    return;
                var lines = await _config.ListUses(targets[0]);
                foreach (var line in lines)
                {
                    this.comboBox_use.Items.Add(line.Name == null ? line.Value : line.Name);
                }
            } */
        }

        async Task UpdateUseList()
        {
            this.comboBox_use.Items.Clear();

            if (string.IsNullOrEmpty(this.comboBox_server.Text))
                return;

            var targets = _config.ListTargets(this.comboBox_server.Text);
            if (targets.Count == 0)
                return;
            var lines = await _config.ListUses(targets[0]);
            // 2021/8/4
            // 按照 .Name 排序
            lines.Sort((a, b) => string.Compare(a.Name, b.Name));
            foreach (var line in lines)
            {
                this.comboBox_use.Items.Add(line.Name == null ? line.Value : line.Name);
            }
        }

        private void comboBox_server_SelectedIndexChanged(object sender, EventArgs e)
        {
            // this.comboBox_use.Items.Clear();

        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_queryWord);
                controls.Add(this.comboBox_server);
                controls.Add(this.comboBox_use);
                controls.Add(this.comboBox_format);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_queryWord);
                controls.Add(this.comboBox_server);
                controls.Add(this.comboBox_use);
                controls.Add(this.comboBox_format);
                GuiState.SetUiState(controls, value);
            }
        }

        private void SruSearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MainForm.AppInfo.SetString(
"srusearchform",
"ui_state",
this.UiState);
        }

        private async void comboBox_server_TextChanged(object sender, EventArgs e)
        {
            await UpdateUseList();
        }

        void EnableControls(bool enable)
        {
            this.textBox_queryWord.Enabled = enable;
            this.comboBox_use.Enabled = enable;
            this.comboBox_format.Enabled = enable;
            this.comboBox_server.Enabled = enable;
            this.listView_browse.Enabled = enable;

            this.button_search.Enabled = enable;
        }

        // 获得下一批结果集中数据
        // 启动后控制就立即返回
        // thread:
        //      界面线程
        // return:
        //      -1  error
        //      0   线程已经启动，但是没有等它结束
        //      1   线程已经结束
        public int NextBatch()
        {
            Int32.TryParse(_numberOfRecords, out int hitcount);
            Int32.TryParse(_nextRecordPosition, out int nextstart);

            if (this.listView_browse.Items.Count >= hitcount)
            {
                MessageBox.Show(this, "已经全部装入");
                return 1;
            }

            _ = GetNextBatch(_serverName,
                _word,
                _useName,
                _recordSchema,
                _nextRecordPosition);
            return 0;
        }

        async Task GetNextBatch(
            string serverName,
            string word,
            string use_name,
            string recordSchema,
            string nextRecordPosition)
        {
            string strError = "";
            EnableControls(false);
            try
            {
                string url = await _config.BuildSearchUrl(_serverName,
        word,
        use_name,
        recordSchema,
        nextRecordPosition,
        null);
                var result = await WebClientEx.DownloadStringAsync(url);

                FillBrowseList(result.String, false);
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_browse_DoubleClick(object sender, EventArgs e)
        {
            //this._processing++;
            try
            {
                int nIndex = -1;
                if (this.listView_browse.SelectedIndices.Count > 0)
                    nIndex = this.listView_browse.SelectedIndices[0];
                else
                {
                    if (this.listView_browse.FocusedItem == null)
                        return;
                    nIndex = this.listView_browse.Items.IndexOf(this.listView_browse.FocusedItem);
                }

                LoadDetail(nIndex,
                    LoadToExistDetailWindow == true ? "exist" : "new");
            }
            finally
            {
                //this._processing--;
            }
        }

        // parameters:
        //      strStyle    打开的风格 new/exist/fixed
        //      bOpendNew   是否打开新的详细窗
        void LoadDetail(int index,
            string strStyle = "new"
                // bool bOpenNew = true
                )
        {
            // 取出记录路径，析出书目库名，然后看这个书目库的syntax
            // 可能装入MARC和DC两种不同的窗口
            string strError = "";

#if NO
            // 防止重入
            if (m_bInSearching == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法装载记录。请稍后再试。";
                goto ERROR1;
            }
#endif

            string strSyntax = "";
            int nRet = GetOneRecordSyntax(index,
                out strSyntax,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strSyntax == "" // default = unimarc
                || strSyntax.ToLower() == "unimarc"
                || strSyntax.ToLower() == "usmarc")
            {

                MarcDetailForm form = null;
                MarcDetailForm exist_fixed = this.MainForm.FixedMarcDetailForm;

                if (strStyle == "exist")
                    form = this.MainForm.TopMarcDetailForm;
                else if (strStyle == "fixed")
                    form = exist_fixed;

                if (exist_fixed != null)
                    exist_fixed.Activate();

                if (form == null)
                {
                    form = new MarcDetailForm();

                    form.MdiParent = this.MainForm;
                    form.MainForm = this.MainForm;

                    if (strStyle == "fixed")
                    {
                        form.Fixed = true;
                        form.SuppressSizeSetting = true;
                        this.MainForm.SetMdiToNormal();
                    }
                    else
                    {
                        // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                        if (exist_fixed != null)
                        {
                            form.SuppressSizeSetting = true;
                            this.MainForm.SetMdiToNormal();
                        }
                    }
                    form.Show();
                    if (strStyle == "fixed")
                        this.MainForm.SetFixedPosition(form, "left");
                    else
                    {
                        // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                        if (exist_fixed != null)
                        {
                            this.MainForm.SetFixedPosition(form, "right");
                        }
                    }
                }
                else
                {
                    form.Activate();
                    if (strStyle == "fixed")
                    {
                        this.MainForm.SetMdiToNormal();
                    }
                    else
                    {
                        // 在已经有左侧窗口的情况下，普通窗口需要显示在右侧
                        if (exist_fixed != null)
                        {
                            this.MainForm.SetMdiToNormal();
                        }
                    }
                }

                // MARC Syntax OID
                // 需要建立数据库配置参数，从中得到MARC格式
                ////form.AutoDetectedMarcSyntaxOID = "1.2.840.10003.5.1";   // UNIMARC

                form.LoadRecord(this, index);
            }
            else if (strSyntax.ToLower() == "dc")
            {
                DcForm form = null;

                if (strStyle == "exist")
                    form = this.MainForm.TopDcForm;

                if (form == null)
                {
                    form = new DcForm();

                    form.MdiParent = this.MainForm;
                    form.MainForm = this.MainForm;

                    form.Show();
                }
                else
                    form.Activate();

                form.LoadRecord(this, index);
            }
            else
            {
                strError = "未知的syntax '" + strSyntax + "'";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int GetOneRecordSyntax(int index,
    out string strSyntax,
    out string strError)
        {
            strSyntax = "";
            strError = "";

            var data = this.listView_browse.Items[index]?.Tag as RecordData;
            if (data == null)
            {
                strError = ".tag == null";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(data.Data);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("srw", "http://www.loc.gov/zing/srw/");
            nsmgr.AddNamespace("marc21", "http://www.loc.gov/MARC21/slim");

            var record = dom.DocumentElement.SelectSingleNode("//marc21:record", nsmgr);
            if (record != null)
            {
                strSyntax = "usmarc";
                return 0;
            }
            strError = "无法识别数据 Syntax";
            return -1;
        }

        // 是否优先装入已经打开的详细窗?
        public bool LoadToExistDetailWindow
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "all_search_form",
                    "load_to_exist_detailwindow",
                    true);
            }
        }

        #region ISearchForm

        public string CurrentProtocol
        {
            get
            {
                return "sru";
            }
        }

        public string CurrentResultsetPath
        {
            get
            {
                /*
                string strServerName = "";
                string strServerUrl = "";
                string strDbName = "";
                string strFrom = "";
                string strFromStyle = "";

                string strError = "";

                int nRet = dp2ResTree.GetNodeInfo(this.dp2ResTree1.SelectedNode,
                    out strServerName,
                    out strServerUrl,
                    out strDbName,
                    out strFrom,
                    out strFromStyle,
                    out strError);
                if (nRet == -1)
                    return "";

                return strServerName
                    + "/" + strDbName
                    + "/" + strFrom
                    + "/" + this.textBox_simple_queryWord.Text
                    + "/default";
                */
                return "";
            }
        }


        // 获得一条MARC/XML记录
        // return:
        //      -1  error 包括not found
        //      0   found
        //      1   为诊断记录
        public int GetOneRecord(
            string strStyle,
            int nTest,
            string strPathParam,
            string strParameters,   // bool bHilightBrowseLine,
            out string strSavePath,
            out string strRecord,
            out string strXmlFragment,
            out string strOutStyle,
            out byte[] baTimestamp,
            out long lVersion,
            out DigitalPlatform.OldZ3950.Record record,
            out Encoding currrentEncoding,
            out LoginInfo logininfo,
            out string strError)
        {
            strXmlFragment = "";
            strRecord = "";
            record = null;
            strError = "";
            currrentEncoding = Encoding.UTF8;   //  this.CurrentEncoding;
            baTimestamp = null;
            strSavePath = "";
            strOutStyle = "marc";
            logininfo = new LoginInfo();
            lVersion = 0;

#if NO
            // 防止重入
            if (m_bInSearching == true)
            {
                strError = "当前窗口正在被一个未结束的长操作使用，无法获得记录。请稍后再试。";
                return -1;
            }
#endif

            if (strStyle != "marc" && strStyle != "xml")
            {
                strError = "dp2SearchForm只支持获取MARC格式记录和xml格式记录，不支持 '" + strStyle + "' 格式的记录";
                return -1;
            }
            int nRet = 0;

            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            if (index == -1)
            {
                string strOutputPath = "";
                nRet = InternalGetOneRecord(
                    true,
                    strStyle,
                    strPath,
                    strDirection,
                    strParameters,  // 2013/9/22
                    out strRecord,
                    out strXmlFragment,
                    out strOutputPath,
                    out strOutStyle,
                    out baTimestamp,
                    out record,
                    out currrentEncoding,
                    out strError);
                if (string.IsNullOrEmpty(strOutputPath) == false)
                    strSavePath = this.CurrentProtocol + ":" + strOutputPath;
                return nRet;
            }

#endif

            bool bHilightBrowseLine = StringUtil.IsInList("hilight_browse_line", strParameters);

            if (index >= this.listView_browse.Items.Count)
            {
                // 如果检索曾经中断过，这里可以触发继续检索
                strError = "越过结果集尾部";
                return -1;
            }

            ListViewItem curItem = this.listView_browse.Items[index];

            if (bHilightBrowseLine == true)
            {
                // 修改listview中事项的选定状态
                for (int i = 0; i < this.listView_browse.SelectedItems.Count; i++)
                {
                    this.listView_browse.SelectedItems[i].Selected = false;
                }

                curItem.Selected = true;
                curItem.EnsureVisible();
            }


            var data = curItem.Tag as RecordData;
            strRecord = data.GetData(strStyle, out string strOutMarcSyntax);

            record = new DigitalPlatform.OldZ3950.Record();
            if (strOutMarcSyntax == "unimarc" || strOutMarcSyntax == "")
                record.m_strSyntaxOID = "1.2.840.10003.5.1";
            else if (strOutMarcSyntax == "usmarc")
                record.m_strSyntaxOID = "1.2.840.10003.5.10";
            else if (strOutMarcSyntax == "dc")
                record.m_strSyntaxOID = "?";
            else
            {
                strError = "未知的MARC syntax '" + strOutMarcSyntax + "'";
                return -1;
            }

            strPath = data.ID + "@" + _serverName;
            strSavePath = this.CurrentProtocol + ":" + strPath;
            return 0;
#if NO
            strPath = curItem.Text;

            strSavePath = this.CurrentProtocol + ":" + strPath;

            {
                string strOutputPath = "";

                nRet = InternalGetOneRecord(
                    true,
                    strStyle,
                    strPath,
                    "",
                    strParameters,  // 2013/9/22
                    out strRecord,
                    out strXmlFragment,
                    out strOutputPath,
                    out strOutStyle,
                    out baTimestamp,
                    out record,
                    out currrentEncoding,
                    out strError);
                if (string.IsNullOrEmpty(strOutputPath) == false)
                    strSavePath = this.CurrentProtocol + ":" + strOutputPath;
                return nRet;
            }
#endif
        }



        // 刷新一条MARC记录
        // return:
        //      -2  不支持
        //      -1  error
        //      0   相关窗口已经销毁，没有必要刷新
        //      1   已经刷新
        //      2   在结果集中没有找到要刷新的记录
        public int RefreshOneRecord(
            string strPathParam,
            string strAction,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            int index = -1;
            string strPath = "";
            string strDirection = "";
            nRet = Global.ParsePathParam(strPathParam,
                out index,
                out strPath,
                out strDirection,
                out strError);
            if (nRet == -1)
                return -1;

            List<ListViewItem> items = new List<ListViewItem>();

            if (index == -1)
            {
                ListViewItem item = ListViewUtil.FindItem(this.listView_browse, strPath, 0);
                if (item == null)
                {
                    strError = "路径为 '" + strPath + "' 的事项在列表中没有找到";
                    return 2;
                }
                items.Add(item);
            }
            else
            {
                if (index >= this.listView_browse.Items.Count)
                {
                    strError = "index [" + index.ToString() + "] 越过结果集尾部";
                    return -1;
                }
                items.Add(this.listView_browse.Items[index]);
            }

            if (strAction == "refresh")
            {
                nRet = RefreshListViewLines(items,
        out strError);
                if (nRet == -1)
                    return -1;

                DoViewComment(false);
                return 1;
            }

#endif
            return 0;
        }

        // 同步一条 MARC/XML 记录
        // 如果 Lversion 比检索窗中的记录新，则用 strMARC 内容更新检索窗内的记录
        // 如果 lVersion 比检索窗中的记录旧(也就是说 Lverion 的值偏小)，那么从 strMARC 中取出记录更新到记录窗
        // parameters:
        //      lVersion    [in]记录窗的 Version [out] 检索窗的记录 Version
        // return:
        //      -1  出错
        //      0   没有必要更新
        //      1   已经更新到 检索窗
        //      2   需要从 strMARC 中取出内容更新到记录窗
        public int SyncOneRecord(string strPath,
            ref long lVersion,
            ref string strSyntax,
            ref string strMARC,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            BiblioInfo info = null;

            // 存储所获得书目记录 XML
            info = (BiblioInfo)this.m_biblioTable[strPath];
            if (info == null)
            {
                // 检索窗中内存尚未存储的情况，相当于 version = 0
                if (lVersion > 0)
                {
                    // 预先准备好 info 
                    // 找到 Item 行
                    ListViewItem item = ListViewUtil.FindItem(this.listView_browse, strPath, 0);
                    if (item == null)
                    {
                        strError = "路径为 '" + strPath + "' 的事项在列表中没有找到";
                        return -1;
                    }

                    nRet = GetBiblioInfo(
                        true,
                        item,
                        out info,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 继续向后执行
                }
                else
                    return 0;
            }

            if (info != null)
            {
                if (lVersion == info.NewVersion)
                    return 0;

                string strXml = "";
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    strXml = info.OldXml;
                else
                    strXml = info.NewXml;

                if (lVersion > info.NewVersion)
                {
                    // 来自 strMARC 的更新一点
                    info.NewVersion = lVersion;

                    if (strSyntax == "xml")
                        strXml = strMARC;
                    else
                    {
                        XmlDocument domMarc = new XmlDocument();
                        domMarc.LoadXml(strXml);

                        // TODO: 需要测试。看看是否可以保留以前的 file 元素
                        nRet = MarcUtil.Marc2Xml(strMARC,
                            strSyntax,
                            out domMarc,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        strXml = domMarc.OuterXml;
                    }
                }
                else
                {
                    // 来自 info 的更新一点
                    lVersion = info.NewVersion;

                    if (strSyntax == "xml")
                        strMARC = strXml;
                    else
                    {
                        // 将XML格式转换为MARC格式
                        // 自动从数据记录中获得MARC语法
                        nRet = MarcUtil.Xml2Marc(strXml,
                            true,
                            null,
                            out strSyntax,
                            out strMARC,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    return 2;
                }

                /*
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    info.OldXml = strXml;
                else
                    info.NewXml = strXml;
                 * */
                if (string.IsNullOrEmpty(info.NewXml) == true)
                    this.m_nChangedCount++;
                info.NewXml = strXml;

                DoViewComment(false);
                return 1;
            }

#endif
            return 0;
        }

        // 对象、窗口是否还有效?
        public bool IsValid()
        {
            if (this.IsDisposed == true)
                return false;

            return true;
        }

#endregion

    }


}
