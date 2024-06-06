using System;
using System.Windows.Forms;

using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using static DigitalPlatform.Marc.MarcEditor;


namespace dp2Circulation
{
    public partial class VerifyMarcResultDialog : Form
    {
        /// <summary>
        /// 原始记录。显示在左侧
        /// </summary>
        public string OriginMarc = "";

        /// <summary>
        /// 修改后的记录。显示在右侧
        /// </summary>
        public string ChangedMarc = "";


        public VerifyMarcResultDialog()
        {
            InitializeComponent();
        }

        private void VerifyMarcResultDialog_Load(object sender, EventArgs e)
        {
            if (this.DesignMode)
                return;
            this.Display();
        }

        private void VerifyMarcResultDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 接受修改后的记录
        private void button_acceptChangedMarc_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // 取消(也就是说不接受修改后的记录)
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void Display()
        {
            string strError = "";
            string strHtml2 = "";

            int nRet = GetHtml(
    this.OriginMarc,
    this.ChangedMarc,
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
            // 设计态
            if (this.DesignMode)
                return "";


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

#if REMOVED
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
                    _leftTitle,
                    strOldMARC,
                    strOldFragmentXml,
                    "",
                    _rightTitle,
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
#endif
        int GetHtml(
string strOldMARC,
string strNewMARC,
out string strHtml2,
out string strError)
        {
            strError = "";
            strHtml2 = "";
            int nRet = 0;

            string strOldFragmentXml = "";
            string strNewFragmentXml = "";

            if (string.IsNullOrEmpty(strOldMARC) == false
                && string.IsNullOrEmpty(strNewMARC) == false)
            {
                // 创建展示两个 MARC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功
                nRet = MarcDiff.DiffHtml(
                    _leftTitle,
                    strOldMARC,
                    strOldFragmentXml,
                    "",
                    _rightTitle,
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


        internal string _leftTitle = "原始记录";
        internal string _rightTitle = "修改后的记录";

        private void toolStripButton_copyLeftToClipboard_Click(object sender, EventArgs e)
        {
            TextToClipboardFormat(this.OriginMarc);
        }

        private void toolStripButton_copyRightToClipboard_Click(object sender, EventArgs e)
        {
            TextToClipboardFormat(this.ChangedMarc);
        }

        public static void TextToClipboardFormat(string strText)
        {
            /*
#if BIDI_SUPPORT
            strText = RemoveBidi(strText);
#endif
            */

            // Make a DataObject.
            DataObject data_object = new DataObject();

            // Add the data in various formats.
            // 普通格式
            data_object.SetData(DataFormats.UnicodeText, strText
                .Replace((char)Record.SUBFLD, '$')
                .Replace((char)Record.FLDEND, '#')
                .Replace((char)Record.RECEND, '*'));
            // 专用格式
            data_object.SetData(new MarcEditorData(strText));

            // Place the data in the Clipboard.
            StringUtil.RunClipboard(() =>
            {
                Clipboard.SetDataObject(data_object);
            });
        }

    }
}
