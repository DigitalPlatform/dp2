using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class PrintReaderSheetDialog : Form
    {
        public PrintReaderSheetDialog()
        {
            InitializeComponent();
        }

        public bool GroupByDepartment
        {
            get
            {
                return this.checkBox_groupByDepartment.Checked;
            }
            set
            {
                this.checkBox_groupByDepartment.Checked = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkBox_groupByDepartment);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkBox_groupByDepartment);
                GuiState.SetUiState(controls, value);
            }
        }

    }

    public class ReaderSheetItem
    {
        public string Name { get; set; }
        public string Barcode { get; set; }
        public string Department { get; set; }

        public ReaderSheetItem(string strName, string strDepartment, string strBarcode)
        {
            this.Name = strName;
            this.Department = strDepartment;
            this.Barcode = strBarcode;
        }
    }

    public class ReaderSheetInfo
    {
        public string Department { get; set; }
        public List<ReaderSheetItem> Items { get; set; }

        public ReaderSheetInfo(string strDepartment)
        {
            this.Department = strDepartment;
        }

        public void AddItem(string strName, string strDepartment, string strBarcode)
        {
            ReaderSheetItem item = new ReaderSheetItem(strName, strDepartment, strBarcode);
            if (this.Items == null)
                this.Items = new List<ReaderSheetItem>();
            this.Items.Add(item);
        }
    }

    public class ReaderSheetCollection : List<ReaderSheetInfo>
    {
        public void AddItem(string strName, string strDepartment, string strBarcode)
        {
            ReaderSheetInfo info = this.Find(strDepartment);
            if (info == null)
            {
                info = new ReaderSheetInfo(strDepartment);
                this.Add(info);
            }

            info.AddItem(strName, strDepartment, strBarcode);
        }

        public ReaderSheetInfo Find(string strDepartment)
        {
            foreach (ReaderSheetInfo info in this)
            {
                if (info.Department == strDepartment)
                    return info;
            }

            return null;
        }
    }
}
