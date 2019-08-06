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

namespace dp2Catalog
{
    public partial class SruSearchForm : Form
    {
        SruConfig _config = null;

        public SruSearchForm()
        {
            InitializeComponent();
        }

        private async void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";
            try
            {
                string use_name = this.comboBox_use.Text;
                string word = this.textBox_queryWord.Text;

                string url = await _config.BuildSearchUrl(this.comboBox_server.Text,
        word,
        use_name);
                var result = await WebClientEx.DownloadStringAsync(url);

                FillBrowseList(result.String);

                // string url = $"https://bnu.alma.exlibrisgroup.com/view/sru/86BNU_INST?version=1.2&operation=searchRetrieve&recordSchema=marcxml&query=alma.isbn={word}";
                return;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            ERROR1:
            MessageBox.Show(this, strError);
        }

        // 填充浏览列表
        void FillBrowseList(string xml)
        {
            this.listView_browse.Items.Clear();

            // "http://www.loc.gov/MARC21/slim"
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("srw", "http://www.loc.gov/zing/srw/");
            nsmgr.AddNamespace("marc21", "http://www.loc.gov/MARC21/slim");

            /*
             *
             *  <recordIdentifier>9910523143603961</recordIdentifier>
                <recordPosition>1</recordPosition>
             * */
            XmlNodeList records = dom.DocumentElement.SelectNodes("//srw:record", nsmgr);
            foreach (XmlElement record in records)
            {
                string id = record.SelectSingleNode("srw:recordIdentifier", nsmgr)?.InnerText;
                string pos = record.SelectSingleNode("srw:recordPosition", nsmgr)?.InnerText;
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

                List<DigitalPlatform.Marc.NameValueLine> results = null;

                if (marcSyntax == "usmarc")
                    nRet = MarcTable.ScriptMarc21("",
                        strMARC,
                        "",
                        null,
                        out results,
                        out strError);
                else if (marcSyntax == "unimarc")
                    nRet = MarcTable.ScriptUnimarc("",
                        strMARC,
                        "",
                        null,
                        out results,
                        out strError);
                else
                    throw new Exception($"未知的 MARC 格式 '{marcSyntax}'");

                if (nRet == -1)
                    throw new Exception(strError);

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, pos);

                FillColumns(item, results);

                this.listView_browse.Items.Add(item);
            }
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
    }


}
