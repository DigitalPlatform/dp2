using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 询问要创建报表的时间范围，频率类型，哪些名称的对话框
    /// </summary>
    public partial class CreateWhatsReportDialog : Form
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public CreateWhatsReportDialog()
        {
            InitializeComponent();
        }

        private void CreateWhatsReportDialog_Load(object sender, EventArgs e)
        {

        }

        private void CreateWhatsReportDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

#if NO
            if (string.IsNullOrEmpty(this.checkedComboBox_createFreq.Text) == true)
            {
                strError = "请选定至少一个频率";
                goto ERROR1;
            }
#endif

            if (string.IsNullOrEmpty(this.textBox_dateRange.Text) == true)
            {
                strError = "请指定时间范围";
                goto ERROR1;
            }

            if (this.SelectedReportsNames.Count == 0)
            {
                strError = "请勾选至少一个报表名";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }



        /// <summary>
        /// 日期范围
        /// </summary>
        public string DateRange
        {
            get
            {
                return this.textBox_dateRange.Text;
            }
            set
            {
                this.textBox_dateRange.Text = value;
            }
        }

        private void checkedComboBox_createFreq_DropDown(object sender, EventArgs e)
        {
            if (this.checkedComboBox_createFreq.Items.Count > 0)
                return;

            this.checkedComboBox_createFreq.Items.Add("day\t日");
            this.checkedComboBox_createFreq.Items.Add("month\t月");
            this.checkedComboBox_createFreq.Items.Add("year\t年");
            this.checkedComboBox_createFreq.Items.Add("free\t自由");
        }

        /// <summary>
        /// 创建频率
        /// </summary>
        public string Frequency
        {
            get
            {
                return this.checkedComboBox_createFreq.Text;
            }
            set
            {
                this.checkedComboBox_createFreq.Text = value;
            }
        }

        const int COLUMN_REPORT_NAME = 0;
        const int COLUMN_REPORT_FREQ = 1;
        const int COLUMN_REPORT_TYPE = 2;

        /// <summary>
        /// 装载报表列表
        /// </summary>
        /// <param name="nodeLibrary"></param>
        public void LoadReportList(XmlNode nodeLibrary)
        {
            this.listView_reports.Items.Clear();

            if (nodeLibrary == null)
                return;

            XmlNodeList nodes = nodeLibrary.SelectNodes("reports/report");
            if (nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strFreq = DomUtil.GetAttr(node, "frequency");

                    string strType = DomUtil.GetAttr(node, "type");
                    //string strCfgFile = DomUtil.GetAttr(node, "cfgFile");
                    //string strNameTable = DomUtil.GetAttr(node, "nameTable");

                    ListViewItem item = new ListViewItem();
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_NAME, strName);
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_FREQ, strFreq);
                    ListViewUtil.ChangeItemText(item, COLUMN_REPORT_TYPE, strType);

                    this.listView_reports.Items.Add(item);
                }
            }
        }

#if NO
        // 
        /// <summary>
        /// 获得和设置 显示出来的全部报表名
        /// </summary>
        public List<string> ReportsNames
        {
            get
            {
                List<string> results = new List<string>();
                for (int i = 0; i < this.checkedListBox_reports.Items.Count; i++)
                {
                    results.Add(this.checkedListBox_reports.Items[i].ToString());
                }

                return results;
            }
            set
            {
                this.checkedListBox_reports.Items.Clear();
                foreach (string s in value)
                {
                    this.checkedListBox_reports.Items.Add(s, true);
                }
            }

        }
#endif

        // 
        /// <summary>
        /// 获得当前选定的那些报表名
        /// </summary>
        public List<string> SelectedReportsNames
        {
            get
            {
                List<string> results = new List<string>();
                foreach (ListViewItem item in this.listView_reports.CheckedItems)
                {
                    results.Add(ListViewUtil.GetItemText(item, COLUMN_REPORT_NAME));
                }

                return results;
            }
            set
            {
                ListViewUtil.ClearChecked(this.listView_reports);

                if (value != null)
                {
                    foreach (string s in value)
                    {
                        ListViewItem item = ListViewUtil.FindItem(this.listView_reports,
                            s, COLUMN_REPORT_NAME);
                        if (item != null)
                            item.Checked = true;
                    }
                }
            }
        }

        private void toolStripButton_selectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView_reports.Items)
            {
                item.Checked = true;
            }
        }

        private void toolStripButton_clearAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.ClearChecked(this.listView_reports);
        }

        private void listView_reports_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;
            if (item.Checked == true)
                item.Font = new Font(this.Font, FontStyle.Bold);
            else
                item.Font = this.Font;

            this.toolStripLabel_info.Text = "当前选定了 " + this.listView_reports.CheckedItems.Count + " 个报表";
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_reports);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_reports);
                GuiState.SetUiState(controls, value);
            }
        }

        // 
        /// <summary>
        /// 获得一个报表的频率
        /// </summary>
        /// <param name="strReportName">报表名</param>
        /// <returns></returns>
        public List<string> GetReportFreq(string strReportName)
        {
            ListViewItem item = ListViewUtil.FindItem(this.listView_reports, strReportName, COLUMN_REPORT_NAME);
            if (item == null)
                return new List<string>();
            return StringUtil.SplitList(ListViewUtil.GetItemText(item, COLUMN_REPORT_FREQ));
        }
    }
}
