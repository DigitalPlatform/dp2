using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using DigitalPlatform.Xml;
using DigitalPlatform;

namespace dp2Catalog
{
    public partial class XmlDetailForm : Form
    {
        bool m_bDisplayOriginPage = true;   // 是否显示原始数据属性页
        public bool DisplayOriginPage
        {
            get
            {
                return this.m_bDisplayOriginPage;
            }
            set
            {
                this.m_bDisplayOriginPage = value;

                if (value == false)
                {
                    // TODO: 这里有内存泄漏，需要改进
                    if (this.tabControl_main.TabPages.IndexOf(this.tabPage_originData) != -1)
                        this.tabControl_main.TabPages.Remove(this.tabPage_originData);
                }
                else
                {
                    if (this.tabControl_main.TabPages.IndexOf(this.tabPage_originData) == -1)
                        this.tabControl_main.TabPages.Add(this.tabPage_originData);
                }
            }
        }

        const int WM_LOADSIZE = API.WM_USER + 201;
        const int WM_VERIFY_DATA = API.WM_USER + 204;
        const int WM_FILL_MARCEDITOR_SCRIPT_MENU = API.WM_USER + 205;

        public LoginInfo LoginInfo = new LoginInfo();
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;

        public ISearchForm LinkedSearchForm = null;

        DigitalPlatform.Z3950.Record CurrentRecord = null;

        Encoding CurrentEncoding = Encoding.GetEncoding(936);

        public string AutoDetectedMarcSyntaxOID = "";

        byte[] CurrentTimestamp = null;
        // 用于保存记录的路径
        public string SavePath
        {
            get
            {
                return this.textBox_savePath.Text;
            }
            set
            {
                this.textBox_savePath.Text = value;
            }

        }


        public XmlDetailForm()
        {
            InitializeComponent();
        }

        private void XmlDetailForm_Load(object sender, EventArgs e)
        {
            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联

            Global.FillEncodingList(this.comboBox_originDataEncoding,
                false);

            this.NeedIndentXml = this.MainForm.AppInfo.GetBoolean(
                "xmldetailform",
                "need_indent_xml",
                true);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        private void XmlDetailForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void XmlDetailForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            SaveSize();

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetBoolean(
        "xmldetailform",
        "need_indent_xml",
        this.NeedIndentXml);
            }
        }

        public int LoadRecord(string strDirection,
            bool bForceFull = false)
        {
            string strError = "";

            if (this.LinkedSearchForm == null)
            {
                strError = "没有关联的检索窗";
                goto ERROR1;
            }

            string strPath = this.textBox_tempRecPath.Text;
            if (String.IsNullOrEmpty(strPath) == true)
            {
                strError = "路径为空";
                goto ERROR1;
            }

            // 分离出各个部分
            string strProtocol = "";
            string strResultsetName = "";
            string strIndex = "";

            int nRet = MarcDetailForm.ParsePath(strPath,
                out strProtocol,
                out strResultsetName,
                out strIndex,
                out strError);
            if (nRet == -1)
            {
                strError = "解析路径 '" + strPath + "' 字符串过程中发生错误: " + strError;
                goto ERROR1;
            }


            if (strProtocol != this.LinkedSearchForm.CurrentProtocol)
            {
                strError = "检索窗的协议已经发生改变";
                goto ERROR1;
            }

            if (strResultsetName != this.LinkedSearchForm.CurrentResultsetPath)
            {
                strError = "结果集已经发生改变";
                goto ERROR1;
            }

            int index = 0;

            index = Convert.ToInt32(strIndex) - 1;


            if (strDirection == "prev")
            {
                index--;
                if (index < 0)
                {
                    strError = "到头";
                    goto ERROR1;
                }
            }
            else if (strDirection == "current")
            {
            }
            else if (strDirection == "next")
            {
                index++;
            }
            else
            {
                strError = "不能识别的strDirection参数值 '" + strDirection + "'";
                goto ERROR1;
            }

            return LoadRecord(this.LinkedSearchForm, index, bForceFull);
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 装载XML记录
        public int LoadRecord(ISearchForm searchform,
            int index,
            bool bForceFullElementSet = false)
        {
            string strError = "";
            string strMARC = "";

            this.LinkedSearchForm = searchform;
            this.SavePath = "";

            DigitalPlatform.Z3950.Record record = null;
            Encoding currentEncoding = null;

            this.CurrentRecord = null;

            byte[] baTimestamp = null;
            string strSavePath = "";
            string strOutStyle = "";
            LoginInfo logininfo = null;
            long lVersion = 0;

            string strXmlFragment = "";

            string strParameters = "hilight_browse_line";
            if (bForceFullElementSet == true)
                strParameters += ",force_full";

            int nRet = searchform.GetOneRecord(
                "xml",
                index,  // 即将废止
                "index:" + index.ToString(),
                strParameters, // true,
                out strSavePath,
                out strMARC,
                out strXmlFragment,
                out strOutStyle,
                out baTimestamp,
                out lVersion,
                out record,
                out currentEncoding,
                out logininfo,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            this.LoginInfo = logininfo;

            this.CurrentTimestamp = baTimestamp;
            this.SavePath = strSavePath;
            this.CurrentEncoding = currentEncoding;


            // 替换单个0x0a
            strMARC = strMARC.Replace("\r", "");
            strMARC = strMARC.Replace("\n", "\r\n");

            // 装入XML编辑器
            // this.textBox_xml.Text = strMARC;
            this.PlainText = strMARC;   // 能自动缩进
            this.textBox_xml.Select(0, 0);

            // 装入XML只读Web控件
            {
                string strTempFileName = MainForm.DataDir + "\\xml.xml";

                // SUTRS
                if (record.m_strSyntaxOID == "1.2.840.10003.5.101")
                    strTempFileName = MainForm.DataDir + "\\xml.txt";

                using (Stream stream = File.Create(strTempFileName))
                {
                    // 写入xml内容
                    byte[] buffer = Encoding.UTF8.GetBytes(strMARC);
                    stream.Write(buffer, 0, buffer.Length);
                }

                this.webBrowser_xml.Navigate(strTempFileName);
            }

            this.CurrentRecord = record;
            if (this.CurrentRecord != null && this.DisplayOriginPage == true)
            {
                // 装入二进制编辑器
                this.binaryEditor_originData.SetData(
                    this.CurrentRecord.m_baRecord);

                // 装入原始文本
                nRet = this.SetOriginText(this.CurrentRecord.m_baRecord,
                    this.CurrentEncoding,
                    out strError);
                if (nRet == -1)
                {
                    this.textBox_originData.Text = strError;
                }

                // 数据库名
                this.textBox_originDatabaseName.Text = this.CurrentRecord.m_strDBName;

                // record syntax OID
                this.textBox_originMarcSyntaxOID.Text = this.CurrentRecord.m_strSyntaxOID;
            }

            // 构造路径
            string strPath = searchform.CurrentProtocol + ":"
                + searchform.CurrentResultsetPath
                + "/" + (index + 1).ToString();

            this.textBox_tempRecPath.Text = strPath;
            this.textBox_xml.Focus();
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        int SetOriginText(byte[] baOrigin,
    Encoding encoding,
    out string strError)
        {
            strError = "";

            if (encoding == null)
            {
                int nRet = this.MainForm.GetEncoding(this.comboBox_originDataEncoding.Text,
                    out encoding,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                this.comboBox_originDataEncoding.Text = GetEncodingForm.GetEncodingName(this.CurrentEncoding);

            }

            this.textBox_originData.Text = encoding.GetString(baOrigin);

            return 0;
        }

        // 装入 XML 到 textbox 的时候需要缩进效果么?
        bool NeedIndentXml
        {
            get
            {
                return this.toolStripButton_indentXmlText.Checked;
            }
            set
            {
                this.toolStripButton_indentXmlText.Checked = value;
            }
        }

        public string PlainText
        {
            get
            {
                return this.textBox_xml.Text;
            }
            set
            {
                if (this.NeedIndentXml == false)
                {
                    this.textBox_xml.Text = value;
                    return;
                }
                string strError = "";
                string strOutXml = "";
                int nRet = DomUtil.GetIndentXml(value,
                    out strOutXml,
                    out strError);
                if (nRet == -1)
                {
                    // 可能并不是 XML
                    this.textBox_xml.Text = value;
                    return;
                }

                this.textBox_xml.Text = strOutXml;
            }
        }

        private void toolStripButton_indentXmlText_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_indentXmlText.Checked == true)
            {
                string strError = "";
                string strOutXml = "";
                int nRet = DomUtil.GetIndentXml(this.textBox_xml.Text,
                    out strOutXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                this.textBox_xml.Text = strOutXml;
            }
        }

        private void XmlDetailForm_Activated(object sender, EventArgs e)
        {
            MainForm.toolButton_prev.Enabled = true;
            MainForm.toolButton_next.Enabled = true;

            MainForm.toolButton_nextBatch.Enabled = false;
            MainForm.toolButton_getAllRecords.Enabled = false;

        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;

            }
            base.DefWndProc(ref m);
        }

        public void LoadSize()
        {
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state",
                MainForm.DefaultMdiWindowWidth,
                MainForm.DefaultMdiWindowHeight);


            // 获得splitContainer_originDataMain的状态
            int nValue = MainForm.AppInfo.GetInt(
            "xmldetailform",
            "splitContainer_originDataMain",
            -1);
            if (nValue != -1)
                this.splitContainer_originDataMain.SplitterDistance = nValue;

            try
            {
                this.tabControl_main.SelectedIndex = this.MainForm.AppInfo.GetInt(
                    "xmldetailform",
                    "active_page",
                    0);
            }
            catch
            {
            }
        }

        public void SaveSize()
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");

                // 保存splitContainer_originDataMain的状态
                MainForm.AppInfo.SetInt(
                    "xmldetailform",
                    "splitContainer_originDataMain",
                    this.splitContainer_originDataMain.SplitterDistance);

                this.MainForm.AppInfo.SetInt(
                    "xmldetailform",
                    "active_page",
                    this.tabControl_main.SelectedIndex);
            }
        }
    }
}