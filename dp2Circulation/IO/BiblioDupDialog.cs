using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Collections;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    public partial class BiblioDupDialog : Form
    {
        public string OriginXml { get; set; }

        public string OriginRecPath { get; set; }

        public string DupBiblioRecPathList { get; set; }

        public BiblioDupDialog()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_browse.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

        }

        // 外来数据的浏览列标题的对照表。MARC 格式名 --> 列标题字符串
        Hashtable _browseTitleTable = new Hashtable();

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(e.DbName);
            if (temp != null)
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_browse.Tag;
            prop.ClearCache();
        }


        private void BiblioDupDialog_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(FillBrowseList));
        }

        void FillBrowseList()
        {
            this.listView_browse.Items.Clear();

            List<string> recpaths = StringUtil.SplitList(this.DupBiblioRecPathList);

            LibraryChannel channel = Program.MainForm.GetChannel();

            try
            {
                // 获得书目记录
                BrowseLoader loader = new BrowseLoader();
                loader.Channel = channel;
                // loader.Stop = this.Progress;
                loader.Format = "id,xml,cols,timestamp";
                loader.RecPaths = recpaths;

                int i = 0;
                foreach (DigitalPlatform.LibraryClient.localhost.Record biblio_item in loader)
                {
                    ListViewItem item = null;

                    if (biblio_item.RecordBody != null
                        && biblio_item.RecordBody.Result != null
                        && biblio_item.RecordBody.Result.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.NoError)
                    {
                        item = Global.AppendNewLine(
                            this.listView_browse,
                            biblio_item.Path,
                            new string[] { biblio_item.RecordBody.Result.ErrorString });
                        item.Tag = null;
                    }
                    else
                    {
                        item = Global.AppendNewLine(
                            this.listView_browse,
                            biblio_item.Path,
                            biblio_item.Cols);
                        ItemTag tag = new ItemTag();
                        tag.RecPath = biblio_item.Path;
                        tag.Xml = biblio_item.RecordBody.Xml;
                        tag.Timestamp = biblio_item.RecordBody.Timestamp;
                        item.Tag = tag;
                    }

                    i++;
                }

                if (this.listView_browse.Items.Count > 0)
                    ListViewUtil.SelectLine(this.listView_browse.Items[0], true);
            }
            finally
            {
                Program.MainForm.ReturnChannel(channel);
            }

            if (this.AutoSelectMode == true)
            {
                SortItems();
                if (this.listView_browse.Items.Count > 0)
                {
                    ListViewUtil.SelectLine(this.listView_browse.Items[0], true);

                    if (this.MergeStyle == dp2Circulation.MergeStyle.None)
                    {
                        // 自动选择保留目标书目记录的方式
                        this.MergeStyle = MergeStyle.ReserveTargetBiblio;
                    }

                    if ((this.MergeStyle & dp2Circulation.MergeStyle.ReserveTargetBiblio) == dp2Circulation.MergeStyle.ReserveTargetBiblio)
                        this.Action = "mergeTo";    // useTargetBiblio
                    else
                        this.Action = "mergeToUseSourceBiblio";
                    this.Close();
                }
            }
        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSelectedIndexChanged(this.listView_browse,
0,
null);

            if (this.listView_browse.SelectedItems.Count == 1)
            {
                this.SelectedRecPath = ListViewUtil.GetItemText(this.listView_browse.SelectedItems[0], 0);
                ItemTag tag = (ItemTag)this.listView_browse.SelectedItems[0].Tag;
                if (tag != null)
                    this.SelectedTimestamp = tag.Timestamp;
                else
                    this.SelectedTimestamp = null;

                DisplaySelectedRecord(this.listView_browse.SelectedItems[0]);
            }
            else
            {
                this.SelectedRecPath = "";
                this.SelectedTimestamp = null;

                // Global.ClearHtmlPage(this.webBrowser1, this.TempDir);
                DisplaySelectedRecord(null);
            }
        }

        // 显示选择的目标记录、和源记录。源记录显示在左边
        void DisplaySelectedRecord(ListViewItem item)
        {
            string strError = "";
            int nRet = 0;

            string strXmlOrigin = this.OriginXml;
            string strXmlTarget = "";

            if (item != null)
            {
                ItemTag tag = (ItemTag)item.Tag;
                if (tag != null)
                    strXmlTarget = tag.Xml;
            }

            string strRecPathOrigin = this.OriginRecPath;
            string strRecPathTarget = "";

            if (item != null)
                strRecPathTarget = ListViewUtil.GetItemText(item, 0);

            string strOriginMARC = "";
            string strOriginFragmentXml = "";
            if (string.IsNullOrEmpty(strXmlOrigin) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXmlOrigin,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strOriginMARC,
                    out strOriginFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    goto ERROR1;
                }
            }

            string strTargetMARC = "";
            string strTargetFragmentXml = "";
            if (string.IsNullOrEmpty(strXmlTarget) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXmlTarget,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strTargetMARC,
                    out strTargetFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML转换到MARC记录时出错: " + strError;
                    goto ERROR1;
                }
            }
            else
                strTargetMARC = "";

            string strHtml2 = "";
#if NO
            if (string.IsNullOrEmpty(strOriginMARC) == false
                && string.IsNullOrEmpty(strTargetMARC) == false)
#endif
            {
                string strTargetTitle = "目标: " + strRecPathTarget;

                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                nRet = MarcDiff.DiffHtml(
                    "源",
                    strOriginMARC,
                    strOriginFragmentXml,
                    "",
                    strTargetTitle,
                    strTargetMARC,
                    strTargetFragmentXml,
                    "",
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            {
                string strHtml = "<html>" +
    this.MarcHtmlHead +
    "<body>" +
    strHtml2 +
    "</body></html>";

                this.webBrowser1.Stop();
                Global.SetHtmlString(this.webBrowser1,
        strHtml,
        this.TempDir,
        "temp_biblio_dup_html");
            }
            return;
        ERROR1:
            {
                string strHtml = "<html>" +
this.MarcHtmlHead +
"<body>" +
HttpUtility.HtmlEncode(strError) +
"</body></html>";

                this.webBrowser1.Stop();
                Global.SetHtmlString(this.webBrowser1,
        strHtml,
        this.TempDir,
        "temp_biblio_dup_html");
            }
        }

#if NO
        void DisplayMarc(string strOldMARC,
    string strNewMARC)
        {
            string strError = "";

            string strHtml2 = "";

            if (string.IsNullOrEmpty(strOldMARC) == false
   && string.IsNullOrEmpty(strNewMARC) == true)
            {
                strHtml2 = MarcUtil.GetHtmlOfMarc(strOldMARC,
                    null,
                    null,
                    false);
            }
            else
            {
                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                int nRet = MarcDiff.DiffHtml(
                    strOldMARC,
                    null,
                    null,
                    strNewMARC,
                    null,
                    null,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                {
                }
            }

            string strHtml = "<html>" +
this.MarcHtmlHead +
"<body>" +
strHtml2 +
"</body></html>";

            this.webBrowser1.Stop();
            Global.SetHtmlString(this.webBrowser1,
    strHtml,
    this.TempDir,
    "temp_html");
        }
#endif

        public string TempDir
        {
            get;
            set;
        }

        public string MarcHtmlHead
        {
            get;
            set;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_browse);
                controls.Add(this.splitContainer1);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_browse);
                controls.Add(this.splitContainer1);
                GuiState.SetUiState(controls, value);
            }

        }

        // 选中的目标记录路径
        public string SelectedRecPath { get; set; }

        public byte[] SelectedTimestamp { get; set; }

        public string Action { get; set; }

        public MergeStyle MergeStyle = MergeStyle.None;

        private void toolStripButton_mergeTo_Click(object sender, EventArgs e)
        {
            // TODO: 出现合并对话框，选择保留源和目标中哪一条书目记录
            GetMergeStyleDialog merge_dlg = new GetMergeStyleDialog();
            MainForm.SetControlFont(merge_dlg, this.Font, false);
            merge_dlg.EnableSubRecord = false;
            merge_dlg.SourceRecPath = this.OriginRecPath;
            merge_dlg.TargetRecPath = this.SelectedRecPath;
            merge_dlg.MessageText = "请指定源书目记录(左侧)和目标书目记录(右侧)合并的方法";

            merge_dlg.UiState = Program.MainForm.AppInfo.GetString(
"BiblioDupDialog",
"GetMergeStyleDialog_uiState",
"");
            {
                MergeStyle style = merge_dlg.GetMergeStyle();
                // 去掉和 Subrecord 有关的 bit
                style = (style & (dp2Circulation.MergeStyle.ReserveSourceBiblio | dp2Circulation.MergeStyle.ReserveTargetBiblio));
                // 设置为 合并下级记录
                style |= dp2Circulation.MergeStyle.CombineSubrecord;
                merge_dlg.SetMergeStyle(style);
            }
            Program.MainForm.AppInfo.LinkFormState(merge_dlg, "entityform_GetMergeStyleDialog_state");
            merge_dlg.ShowDialog(this);
            Program.MainForm.AppInfo.SetString(
"BiblioDupDialog",
"GetMergeStyleDialog_uiState",
merge_dlg.UiState);

            if (merge_dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.MergeStyle = merge_dlg.GetMergeStyle();

            if ((this.MergeStyle & dp2Circulation.MergeStyle.ReserveTargetBiblio) == dp2Circulation.MergeStyle.ReserveTargetBiblio)
                this.Action = "mergeTo";    // useTargetBiblio
            else
                this.Action = "mergeToUseSourceBiblio";
            this.Close();
        }

        private void toolStripButton_createNew_Click(object sender, EventArgs e)
        {
            this.Action = "createNew";
            this.Close();
        }

        private void toolStripButton_skip_Click(object sender, EventArgs e)
        {
            this.Action = "skip";
            this.Close();
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            this.Action = "stop";
            this.Close();
        }

        private void BiblioDupDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Action) == true || this.Action == "stop")
            {
                // 警告中断处理
                DialogResult result = MessageBox.Show(this,
    "确实要中断全部处理? ",
    "BiblioDupDialog",
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

        // 进入自动选择状态
        private void toolStripButton_autoSelect_Click(object sender, EventArgs e)
        {

        }

        // 是否处在 AutoSelect 模式
        public bool AutoSelectMode
        {
            get
            {
                return this.toolStripButton_autoSelect.Checked;
            }
            set
            {
                this.toolStripButton_autoSelect.Checked = value;
            }
        }

        public MergeRegistry AutoMergeRegistry { get; set; }

        public void SortItems()
        {
            if (this.listView_browse.Items.Count <= 1)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_browse.Items)
            {
                items.Add(item);
            }

            this.AutoMergeRegistry.Sort(ref items);

            this.listView_browse.Items.Clear();
            foreach (ListViewItem item in items)
            {
                this.listView_browse.Items.Add(item);
            }
        }
    }

    public class ItemTag
    {
        public string RecPath { get; set; }
        public string Xml { get; set; }
        public byte[] Timestamp { get; set; }
    }
}
