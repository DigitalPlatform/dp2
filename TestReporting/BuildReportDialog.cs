using System;
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

using DigitalPlatform.CommonControl;
using DigitalPlatform.Xml;

namespace TestReporting
{
    public partial class BuildReportDialog : Form
    {
        public string DataDir { get; set; }

        public BuildReportDialog()
        {
            InitializeComponent();

        }

        public string ReportType
        {
            get
            {
                return this.comboBox_reportType.Text;
            }
            set
            {
                this.comboBox_reportType.Text = value;
            }
        }

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

        public string LibraryCode
        {
            get
            {
                return this.textBox_libraryCode.Text;
            }
            set
            {
                this.textBox_libraryCode.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_reportType);
                controls.Add(this.textBox_dateRange);
                controls.Add(this.textBox_parameters);
                controls.Add(this.textBox_libraryCode);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_reportType);
                controls.Add(this.textBox_dateRange);
                controls.Add(this.textBox_parameters);
                controls.Add(this.textBox_libraryCode);
                GuiState.SetUiState(controls, value);
            }
        }

        void FillReportList()
        {
            DirectoryInfo di = new DirectoryInfo(this.DataDir);
            var fis = di.GetFiles();
            foreach(var fi in fis)
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

        public string [] Parameters
        {
            get
            {
                return this.textBox_parameters.Lines;
            }
            set
            {
                this.textBox_parameters.Lines = value;
            }
        }
    }
}
