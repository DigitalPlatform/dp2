﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.Marc;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 书目记录覆盖保存时候，显示原有记录内容和新内容的对话框
    /// </summary>
    public partial class OverwriteBiblioDialog : Form
    {
        public string TargetPosition { get; set; }
        public string SourcePosition { get; set; }

        /// <summary>
        /// 新记录 XML
        /// </summary>
        public string NewXml { get; set; }

        /// <summary>
        /// 原有记录 XML
        /// </summary>
        public string ExistingXml { get; set; }

        public OverwriteBiblioDialog()
        {
            InitializeComponent();
        }

        private void OverwriteBiblioDialog_Load(object sender, EventArgs e)
        {
            this.Display();
        }

        private void button_overwrite_Click(object sender, EventArgs e)
        {
            this.Action = "overwrite";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.Action = "cancel";
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        void Display()
        {
            string strError = "";
            string strHtml2 = "";

            int nRet = GetXmlHtml(
    this.ExistingXml,
    this.NewXml,
    out strHtml2,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            string strHtml = "<html>" +
GetHeadString(false) +
"<body>" +
strHtml2 +
"</body></html>";

            lock (this)
            {
                this.webBrowser1.Stop();
                // Debug.Assert(Program.MainForm != null, "");
                Global.SetHtmlString(this.webBrowser1,
                    strHtml,
                    Program.MainForm.DataDir,
                    "partial_denied_viewer_html");
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = PathUtil.MergePath(Program.MainForm.DataDir, "operloghtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    //"<script type='text/javascript' src='%datadir%/jquery.js'></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";
        }

        int GetXmlHtml(
    string strXml1,
    string strXml2,
    out string strHtml2,
    out string strError)
        {
            strError = "";
            strHtml2 = "";
            int nRet = 0;

            string strOldMARC = "";
            string strOldFragmentXml = "";
            if (string.IsNullOrEmpty(strXml1) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml1,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strOldMARC,
                    out strOldFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML 转换到 MARC 记录时出错: " + strError;
                    return -1;
                }
            }

            string strNewMARC = "";
            string strNewFragmentXml = "";
            if (string.IsNullOrEmpty(strXml2) == false)
            {
                string strOutMarcSyntax = "";
                // 将XML格式转换为MARC格式
                // 自动从数据记录中获得MARC语法
                nRet = MarcUtil.Xml2Marc(strXml2,
                    MarcUtil.Xml2MarcStyle.Warning | MarcUtil.Xml2MarcStyle.OutputFragmentXml,
                    "",
                    out strOutMarcSyntax,
                    out strNewMARC,
                    out strNewFragmentXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "XML 转换到 MARC 记录时出错: " + strError;
                    return -1;
                }

            }

            if (string.IsNullOrEmpty(strOldMARC) == false
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                nRet = MarcDiff.DiffHtml(
                    $"原有记录 {TargetPosition}",
                    strOldMARC,
                    strOldFragmentXml,
                    "",
                    $"新记录 {SourcePosition}",
                    strNewMARC,
                    strNewFragmentXml,
                    "",
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else if (string.IsNullOrEmpty(strOldMARC) == false
    && string.IsNullOrEmpty(strNewMARC) == true)
            {
                strHtml2 = MarcUtil.GetHtmlOfMarc(strOldMARC,
                    strOldFragmentXml,
                    "",
                    false);
            }
            else if (string.IsNullOrEmpty(strOldMARC) == true
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                strHtml2 = MarcUtil.GetHtmlOfMarc(strNewMARC,
                    strNewFragmentXml,
                    "",
                    false);
            }

            return 0;
        }

        /*
        internal string _leftTitle = "原有记录";
        internal string _rightTitle = "新记录";
        */

        public string Action { get; set; }

        private void button_skip_Click(object sender, EventArgs e)
        {
            this.Action = "skip";
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public bool DontAsk
        {
            get
            {
                return this.checkBox_dontAsk.Checked;
            }
            set
            {
                this.checkBox_dontAsk.Checked = value;
            }
        }
    }
}
