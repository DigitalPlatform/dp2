using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections;
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
        public string CardPhotoPath { get; set; }

        public ReaderSheetItem(string strXml, string strCardPhotoPath)
        {
            this.Xml = strXml;
            this.CardPhotoPath = strCardPhotoPath;
        }

        /*
         * libraryName: 图书馆名字
         * barcode: 读者证条码号
         * department: 单位
         * name: 读者姓名
         * readerType: 读者类型
         * expire: 证失效期。本地时间格式，注意和 expireDate 元素内容不同
         * cardPhotoPath: 证件照片图像文件路径
         * */
        // parameters:
        //      strTemplate 模板。定义了如何输出读者信息中的每一行
        public void OutputByTemplate(StreamWriter sw, 
            string strTemplate)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            string strLibraryName = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");

            if (string.IsNullOrEmpty(strLibraryName))
                strLibraryName = Program.MainForm.LibraryName;

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            string strGender = DomUtil.GetElementText(dom.DocumentElement, "gender");

            string strReaderType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            string strExpire = DomUtil.GetElementText(dom.DocumentElement, "expireDate");
            if (string.IsNullOrEmpty(strExpire) == false)
                strExpire = DateTimeUtil.LocalDate(strExpire);

            Hashtable macro_table = new Hashtable();
            macro_table["%libraryName%"] = strLibraryName;
            macro_table["%barcode%"] = strBarcode;
            macro_table["%department%"] = strDepartment;
            macro_table["%name%"] = strName;
            macro_table["%gender%"] = strGender;
            macro_table["%readerType%"] = strReaderType;
            macro_table["%expire%"] = strExpire;
            macro_table["%cardPhotoPath%"] = this.CardPhotoPath;

            string strResult = StringUtil.MacroString(macro_table,
    strTemplate);
            sw.WriteLine(strResult);
            sw.WriteLine("***");
        }

        public void Output(StreamWriter sw, string strSheetDefName)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            string strLibraryName = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");

            // 2017/10/19
            if (string.IsNullOrEmpty(strLibraryName))
                strLibraryName = Program.MainForm.LibraryName;

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement, "department");
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");

            string strReaderType = DomUtil.GetElementText(dom.DocumentElement, "readerType");
            string strExpire = DomUtil.GetElementText(dom.DocumentElement, "expireDate");
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
                if (strSheetDefName.ToLower().IndexOf("photo") != -1)
                    sw.WriteLine(this.CardPhotoPath);
                sw.WriteLine("***");
                return;
            }

            sw.WriteLine(strName);
            sw.WriteLine(strDepartment);
            sw.WriteLine(strBarcode);
            if (strSheetDefName.ToLower().IndexOf("photo") != -1)
                sw.WriteLine(this.CardPhotoPath);
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
        public void AddItem(string strXml, string strCardPhotoPath)
        {
            ReaderSheetItem item = new ReaderSheetItem(strXml, strCardPhotoPath);
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

        public void OutputByTemplate(StreamWriter sw, string strTemplate)
        {
            foreach (ReaderSheetItem item in Items)
            {
                item.OutputByTemplate(sw, strTemplate);
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
        public void AddItem(string strGroup, string strXml, string strCardPhotoPath)
        {
            ReaderSheetInfo info = this.Find(strGroup);
            if (info == null)
            {
                info = new ReaderSheetInfo(strGroup);
                this.Add(info);
            }

            info.AddItem(strXml, strCardPhotoPath);
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
