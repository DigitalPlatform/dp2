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
using System.Windows.Forms;
using System.Xml;

using Newtonsoft.Json;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace TestReporting
{
    public partial class BuildReportDialog1 : Form
    {
        public string DataDir { get; set; }

        public Hashtable SelectedParamTable
        {
            get
            {
                return this._buildReportControl.GetValue();
            }
        }

        Dictionary<string, Hashtable> _uiStateTable = new Dictionary<string, Hashtable>();

        BuildReportControl _buildReportControl = new BuildReportControl();

        public BuildReportDialog1()
        {
            InitializeComponent();

            _buildReportControl.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(_buildReportControl);
            this.panel1.AutoScroll = true;
        }

        public string ReportType
        {
            get
            {
                // return this.comboBox_reportType.Text;
                return GetReportType();
            }
            set
            {
                this.comboBox_reportType.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            SaveCurrentControlState(_currentReportType);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            SaveCurrentControlState(_currentReportType);

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        string UiState0
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_reportType);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_reportType);
                GuiState.SetUiState(controls, value);

                comboBox_reportType_SelectedIndexChanged(this.comboBox_reportType, new EventArgs());
            }
        }

        string UiState1
        {
            get
            {
                return JsonConvert.SerializeObject(_uiStateTable);
            }
            set
            {
                _uiStateTable = JsonConvert.DeserializeObject<Dictionary<string, Hashtable>>(value);
                if (_uiStateTable == null)
                    _uiStateTable = new Dictionary<string, Hashtable>();
            }
        }

        public string UiState
        {
            get
            {
                return UiState0 + "`" + UiState1;
            }
            set
            {
                var parts = StringUtil.ParseTwoPart(value, "`");
                this.UiState1 = parts[1];
                this.UiState0 = parts[0];
            }
        }


        void FillReportList()
        {
            DirectoryInfo di = new DirectoryInfo(this.DataDir);
            var fis = di.GetFiles();
            foreach (var fi in fis)
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(fi.FullName);
                string typeName = DomUtil.GetElementText(dom.DocumentElement, "typeName").Replace(" ", "\t");
                this.comboBox_reportType.Items.Add(typeName);
            }
        }

        private void BuildReportDialog_Load(object sender, EventArgs e)
        {
            FillReportList();
        }

        // 保存当前控件状态
        void SaveCurrentControlState(string reportType)
        {
            _uiStateTable[reportType] = this._buildReportControl.GetValue();
        }

        // 装载当前控件状态。根据报表类型
        void LoadCurrentControlState(string reportType)
        {
            Hashtable table = null;

            if (_uiStateTable.ContainsKey(reportType) == true)
                table = _uiStateTable[reportType];
            if (table == null)
                this._buildReportControl.ClearValue();
            else
                this._buildReportControl.SetValue(table);
        }

        public string GetReportType()
        {
            return StringUtil.ParseTwoPart(this.comboBox_reportType.Text, "\t")[0];
        }

        string _currentReportType = "";

        private void comboBox_reportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 保存前一个
            SaveCurrentControlState(_currentReportType);

            string name = GetReportType();
            string filename = Path.Combine(this.DataDir, name + ".xml");

            try
            {
                XmlDocument dom = new XmlDocument();
                dom.Load(filename);

                XmlElement parameters = dom.DocumentElement.SelectSingleNode("parameters") as XmlElement;
                if (parameters == null)
                {
                    MessageBox.Show(this, $"文件 {filename} 内尚未定义 parameters 元素");
                    return;
                }
                this._buildReportControl.CreateChildren(parameters);

                _currentReportType = GetReportType();
                LoadCurrentControlState(_currentReportType);
            }
            catch (Exception ex)
            {
                // TODO: 如何报错?
                MessageBox.Show(this, ex.Message);
            }
        }
    }
}
