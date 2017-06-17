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

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    public partial class ItemClassStatisDialog : Form
    {
        public string ClassListFileName { get; set; }

        public ItemClassStatisDialog()
        {
            InitializeComponent();
        }

        public bool OverwritePrompt
        {
            get;
            set;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.comboBox_classType.Text) == true)
            {
                strError = "尚未指定分类法";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_outputExcelFileName.Text) == true)
            {
                strError = "尚未指定输出文件名";
                goto ERROR1;
            }

            string strOutputFileName = "";
            // return:
            //      -1  出错
            //      0   文件名不合法
            //      1   文件名合法
            int nRet = ExportPatronExcelDialog.CheckExcelFileName(this.textBox_outputExcelFileName.Text,
                true,
                out strOutputFileName,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            this.textBox_outputExcelFileName.Text = strOutputFileName;

            // 提醒覆盖文件
            if (this.OverwritePrompt == true
                && File.Exists(this.FileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文件 '" + this.FileName + "' 已经存在。继续操作将覆盖此文件。\r\n\r\n请问是否要覆盖此文件? (OK 覆盖；Cancel 放弃操作)",
                    "导出读者详情",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
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

        private void button_getOutputExcelFileName_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_outputExcelFileName.Text;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_outputExcelFileName.Text = dlg.FileName;
        }

        public string FileName
        {
            get
            {
                return this.textBox_outputExcelFileName.Text;
            }
            set
            {
                this.textBox_outputExcelFileName.Text = value;
            }
        }

        public bool OutputPrice
        {
            get
            {
                return this.checkBox_price.Checked;
            }
            set
            {
                this.checkBox_price.Checked = value;
            }
        }

        public string ClassType
        {
            get
            {
                return this.comboBox_classType.Text;
            }
            set
            {
                this.comboBox_classType.Text = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputExcelFileName);
                controls.Add(this.comboBox_classType);
                controls.Add(this.checkBox_price);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_outputExcelFileName);
                controls.Add(this.comboBox_classType);
                controls.Add(this.checkBox_price);
                GuiState.SetUiState(controls, value);
            }
        }

        public static List<string> LoadClassList(string strFileName, string strClassType)
        {
            List<string> results = new List<string>();
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (FileNotFoundException)
            {
                return results;
            }
            catch (DirectoryNotFoundException)
            {
                return results;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("class[@name='" + strClassType + "']/item");
            foreach (XmlElement item in nodes)
            {
                string strLine = item.InnerText.Trim();
                results.Add(strLine);
            }

            return results;
        }

        public static void SaveClassList(string strFileName,
            string strClassType,
            List<string> list)
        {
            PathUtil.TryCreateDir(Path.GetDirectoryName(strFileName));

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (FileNotFoundException)
            {
                dom.LoadXml("<root />");
            }
            catch (DirectoryNotFoundException)
            {
                dom.LoadXml("<root />");
            }

            XmlElement root = dom.DocumentElement.SelectSingleNode("class[@name='" + strClassType + "']") as XmlElement;
            if (root != null)
                root.InnerXml = "";
            else
            {
                root = dom.CreateElement("class");
                dom.DocumentElement.AppendChild(root);
                root.SetAttribute("name", strClassType);
            }

            foreach (string s in list)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                string strLine = s.Trim();
                if (string.IsNullOrEmpty(strLine))
                    continue;
                XmlElement item = dom.CreateElement("item");
                root.AppendChild(item);
                item.InnerText = strLine;
            }

            dom.Save(strFileName);
        }

        private void ItemClassStatisDialog_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboBox_classType.Text) == false)
            {
                LoadClassList();
            }
        }

        string _currentClassType = "";
        void LoadClassList()
        {
            if (string.IsNullOrEmpty(_currentClassType) == false)
            {
                this.textBox_classTitle.Text = StringUtil.MakePathList(LoadClassList(this.ClassListFileName, _currentClassType), "\r\n");
            }
        }

        void SaveClassList()
        {
            if (string.IsNullOrEmpty(_currentClassType) == false)
            {
                SaveClassList(this.ClassListFileName, _currentClassType, this.textBox_classTitle.Lines.ToList());
            }
        }

        private void comboBox_classType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveClassList();

            this.textBox_classTitle.Text = "";
            this._currentClassType = this.comboBox_classType.Text;
            LoadClassList();

            this.textBox_classTitle.Enabled = string.IsNullOrEmpty(this.comboBox_classType.Text) == false;
            this.label_classTitle.Text = (string.IsNullOrEmpty(this._currentClassType) ? "" : (this.comboBox_classType.Text + " "))
                + "类目栏内容:";
        }

        private void ItemClassStatisDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveClassList();
        }

        private void button_setCommonValue_Click(object sender, EventArgs e)
        {
            switch (this._currentClassType)
            {
                case "中图法":
                    this.textBox_classTitle.Text = @"A
B
C
D
E
F
G
H
I
J
K
L
M
N
O
P
Q
R
S
T
TP
U
V
X
Z";
                    break;
                case "科图法":
                    this.textBox_classTitle.Text = @"1
2
3
4
5
6
7
8
9";
                    break;
                case "人大法":
                    this.textBox_classTitle.Text = @"1
2
3
4
5
6
7
8
9";
                    break;
            }
        }

        public List<string> ClassList
        {
            get
            {
                return this.textBox_classTitle.Lines.ToList();
            }
        }

        // 在类目表中进行查找，获得匹配的事项。
        // 可以返回多项
        public static List<string> GetClassHead(List<string> class_list,
            string strClassText)
        {
            List<string> results = new List<string>();

            if (string.IsNullOrEmpty(strClassText) == true)
                return results;

            if (class_list.Count == 0)
            {
                results.Add(strClassText.Substring(0, 1));
                return results;
            }

            foreach (string strEntry in class_list)
            {
                if (strClassText.Length < strEntry.Length)
                    continue;

                if (strClassText.StartsWith(strEntry))
                    results.Add(strEntry);

#if NO
                string strHead = strClassText.Substring(0, strEntry.Length);
                if (strHead == strEntry)
                {
                    results.Add(strEntry);
                }
#endif
            }

#if NO
            if (results.Count == 0)
            {
                results.Add("其它");
            }
#endif

            return results;
        }

        // 去掉价格字符串中的 "(...)" 部分
        // TODO: 检查小数点后的位数，多于 2 位的要删除
        // return:
        //      false   没有发生修改
        //      true    发生了修改
        public static bool CorrectPrice(ref string strText)
        {
            strText = strText.Trim();

            //2017/6/17
            strText.Replace("￥","CNY");

            strText = StringUtil.ToDBC(strText);

            int nStart = strText.IndexOf("(");
            if (nStart == -1)
                return false;

            // 右边剩余部分
            string strRight = strText.Substring(nStart + 1);

            strText = strText.Substring(0, nStart).Trim();
            int nEnd = strRight.IndexOf(")");
            if (nEnd == -1)
                return true;

            string strFragment = strRight.Substring(0, nEnd).Trim();
            strText += strRight.Substring(nEnd + 1).Trim();

            // 判断是否为 全5册 情况
            if (string.IsNullOrEmpty(strFragment) == false)
            {
                string strNumber = StringUtil.Unquote(strFragment, "全册");
                if (strNumber != strFragment)
                {
                    int v = 0;
                    if (StringUtil.IsPureNumber(strNumber) && Int32.TryParse(strNumber, out v))
                    {
                        strText += "/" + strNumber;
                        // strError = "被变换为每册平均价格形态";
                    }
                }
            }

            return true;
        }

    }
}
