using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using static dp2Circulation.OperLogForm;

namespace dp2Circulation.OperLog
{
    public partial class SelectXmlDialog : Form
    {
        public const int COLUMN_NO = 0;
        public const int COLUMN_SOURCE = 1;
        public const int COLUMN_SIZE = 2;
        public const int COLUMN_ERRORINFO = 3;

        public List<RecoverBiblioItem> Xmls { get; set; }

        public RecoverBiblioItem SelectedXml { get; set; }

        public SelectXmlDialog()
        {
            InitializeComponent();
        }

        private void SelectXmlDialog_Load(object sender, EventArgs e)
        {
            FillXmls();
        }

        private void SelectXmlDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SelectXmlDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_browse.SelectedItems.Count != 1)
            {
                strError = "尚未选择 XML 记录";
                goto ERROR1;
            }

            var item = this.listView_browse.SelectedItems[0];
            this.SelectedXml = item.Tag as RecoverBiblioItem;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void FillXmls()
        {
            this.listView_browse.Items.Clear();
            if (this.Xmls == null || this.Xmls.Count == 0)
                return;
            int i = 0;
            foreach (var xml in this.Xmls)
            {
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_NO, (++i).ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_SOURCE, xml.Date + ":" + xml.Index.ToString().PadLeft(5, ' '));
                ListViewUtil.ChangeItemText(item, COLUMN_SIZE, xml.Xml == null ? "0" : xml.Xml?.Length.ToString());

                string errorInfo = "";
                if (IsValid(xml.Xml) == false)
                    errorInfo = "XML 结构不合法";
                ListViewUtil.ChangeItemText(item, COLUMN_ERRORINFO, errorInfo);
                item.Tag = xml;
                this.listView_browse.Items.Add(item);
            }

            // 默认选中最后一行
            if (this.listView_browse.Items.Count > 0)
            {
                int index = this.listView_browse.Items.Count - 1;
                var item = this.listView_browse.Items[index];
                item.Selected = true;
                item.EnsureVisible();
            }
        }

        public static bool IsValid(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return false;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_browse.SelectedItems.Count != 1)
            {
                ClearHtml();
                return;
            }

            var item = this.listView_browse.SelectedItems[0];

            var xml = item.Tag as RecoverBiblioItem;

            if (xml != null && IsValid(xml.Xml) == false)
            {
                if (xml.Xml == null)
                    xml.Xml = "";
                // webBrowser1.DocumentText = $"<html><body><div>{"错误: XML 结构不合法："}</div><div>{HttpUtility.HtmlEncode(xml).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>")}</div></body></html>";
                webBrowser1.DocumentText = $"<html><body><div>{"错误: XML 结构不合法："}</div><code>{HttpUtility.HtmlEncode(xml.Xml)}</code></body></html>";
            }
            else
                this.SetXmlToWebbrowser(this.webBrowser1, xml?.Xml);
        }

        void ClearHtml()
        {
            this.webBrowser1.DocumentText = "<html><body></body></html>";
        }

        void SetXmlToWebbrowser(WebBrowser webbrowser,
     string strXml)
        {
            string strTargetFileName = Path.Combine(Program.MainForm.DataDir, "xml.xml");

            using (StreamWriter sw = new StreamWriter(strTargetFileName,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strXml);
            }

            webbrowser.Navigate(strTargetFileName);
        }

        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var column = this.listView_browse.Columns[e.Column];
            if (column.Tag == null)
                column.Tag = true;
            bool asc = (bool)column.Tag;
            this.listView_browse.ListViewItemSorter = new ListViewItemComparer(e.Column, asc);
            this.listView_browse.Sort();
            column.Tag = !asc;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer1);
                controls.Add(this.listView_browse);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer1);
                controls.Add(this.listView_browse);
                GuiState.SetUiState(controls, value);
            }
        }
    }

    // Implements the manual sorting of items by columns.
    class ListViewItemComparer : IComparer
    {
        int _columnIndex = 0;
        bool _asc = true;

        public ListViewItemComparer(int column_index, bool asc)
        {
            _columnIndex = column_index;
            _asc = asc;
        }

        public int Compare(object x, object y)
        {
            var s1 = ((ListViewItem)x).SubItems[_columnIndex].Text;
            var s2 = ((ListViewItem)y).SubItems[_columnIndex].Text;
            int len1 = s1.Length;
            int len2 = s2.Length;
            int length = Math.Max(len1, len2);
            s1 = s1.PadLeft(length, ' ');
            s2 = s2.PadLeft(length, ' ');
            return (_asc == true ? 1 : -1) * String.Compare(s1, s2);
        }
    }
}
