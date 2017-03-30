using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

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
#if NO
        public string Name { get; set; }
        public string Barcode { get; set; }
        public string Department { get; set; }

        public ReaderSheetItem(string strName, string strDepartment, string strBarcode)
        {
            this.Name = strName;
            this.Department = strDepartment;
            this.Barcode = strBarcode;
        }
#endif
        public string Xml { get; set; }

        public ReaderSheetItem(string strXml)
        {
            this.Xml = strXml;
        }

        public void Output(StreamWriter sw, string strSheetDefName)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            string strLibraryName = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");

            string strReaderType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            string strExpire = DomUtil.GetElementText(dom.DocumentElement, "expire");
            if (string.IsNullOrEmpty(strExpire) == false)
                strExpire = DateTimeUtil.LocalDate(strExpire);

            if (strSheetDefName.ToLower().IndexOf("card") != -1)
            {
                sw.WriteLine(strBarcode);
                sw.WriteLine(strLibraryName);
                sw.WriteLine(strDepartment);
                sw.WriteLine(strName);
                sw.WriteLine("读者类型: " + strReaderType);
                sw.WriteLine("失效期: " + strExpire);

                sw.WriteLine(strBarcode);
                sw.WriteLine("***");
                return;
            }

            sw.WriteLine(strName);
            sw.WriteLine(strDepartment);
            sw.WriteLine(strBarcode);
            sw.WriteLine("***");
        }
    }

    public class ReaderSheetInfo
    {
        // 区分 Sheet 的依据
        public string Group { get; set; }

        public List<ReaderSheetItem> Items { get; set; }

        public ReaderSheetInfo(string strGroup)
        {
            this.Group = strGroup;
        }

#if NO
        public void AddItem(string strName, string strDepartment, string strBarcode)
        {
            ReaderSheetItem item = new ReaderSheetItem(strName, strDepartment, strBarcode);
            if (this.Items == null)
                this.Items = new List<ReaderSheetItem>();
            this.Items.Add(item);
        }
#endif
        public void AddItem(string strXml)
        {
            ReaderSheetItem item = new ReaderSheetItem(strXml);
            if (this.Items == null)
                this.Items = new List<ReaderSheetItem>();
            this.Items.Add(item);
        }

        public void Output(StreamWriter sw, string strSheetDefName)
        {
            foreach(ReaderSheetItem item in Items)
            {
                item.Output(sw, strSheetDefName);
            }
        }

    }

    public class ReaderSheetCollection : List<ReaderSheetInfo>
    {
#if NO
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
#endif
        public void AddItem(string strGroup, string strXml)
        {
            ReaderSheetInfo info = this.Find(strGroup);
            if (info == null)
            {
                info = new ReaderSheetInfo(strGroup);
                this.Add(info);
            }

            info.AddItem(strXml);
        }

        public ReaderSheetInfo Find(string strGroup)
        {
            foreach (ReaderSheetInfo info in this)
            {
                if (info.Group == strGroup)
                    return info;
            }

            return null;
        }

        // 判断一个元素是否为集合的最后一个元素
        public bool IsTail(ReaderSheetInfo info)
        {
            if (this.Count == 0)
                return false;
            if (info == this[this.Count - 1])
                return true;
            return false;
        }
    }
}
