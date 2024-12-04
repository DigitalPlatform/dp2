using System;
using System.Windows.Forms;

using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    public partial class MarcRecordDialog : Form
    {
        public MarcRecord MarcRecord { get; set; }

        public MarcRecordDialog()
        {
            InitializeComponent();
        }

        private void MarcRecordDialog_Load(object sender, EventArgs e)
        {
            if (this.MarcRecord != null)
                Display(this.MarcRecord);
        }

        private void MarcRecordDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public static DialogResult Show(
            MarcRecord record1,
            MarcRecord record2,
            string title = "MarcRecord",
            string style = "save_size")
        {
            MarcRecordComparerDialog dlg = new MarcRecordComparerDialog();

            string left_title = StringUtil.GetParameterByPrefix(style, "left_title");
            string right_title = StringUtil.GetParameterByPrefix(style, "right_title");
            MainForm.SetControlFont(dlg, Program.MainForm.Font, false);
            dlg.Text = title;
            if (left_title != null)
                dlg.LeftTitle = left_title;
            if (right_title != null)
                dlg.RightTitle = right_title;
            dlg.SavingMarc = record1?.Text;
            dlg.SavedMarc = record2?.Text;

            if (StringUtil.IsInList("save_size", style))
                Program.MainForm.AppInfo.LinkFormState(dlg, "MarcRecordDialog_two_state");
            dlg.ShowDialog(Program.MainForm);
            return dlg.DialogResult;
        }

        public static DialogResult Show(MarcRecord record,
    string title = "MarcRecord",
    string style = "save_size")
        {
            return Show(Program.MainForm,
            record,
            title,
            style);
        }

        public static DialogResult Show(IWin32Window owner,
            MarcRecord record,
            string title = "MarcRecord",
            string style = "save_size")
        {
            var dialog = new MarcRecordDialog();
            dialog.Text = title;
            dialog.MarcRecord = record;
            if (StringUtil.IsInList("save_size", style))
                Program.MainForm.AppInfo.LinkFormState(dialog, "MarcRecordDialog");
            return dialog.ShowDialog(owner);
        }

        void ClearHtml()
        {
            this.webBrowser1.DocumentText = "<html><body></body></html>";
        }

        /*public*/
        static void AppendHtml(WebBrowser webBrowser,
 string strHtml,
 bool bClear = false)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            if (bClear == true)
                doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        void Display(MarcRecord record)
        {
            string strHead = @"<head>
<style type='text/css'>
BODY {
	FONT-FAMILY: Microsoft YaHei, Verdana, 宋体;
	FONT-SIZE: 8pt;
}
TABLE.marc
{
    font-size: 8pt;
    width: auto;
}
TABLE.marc TD
{
   vertical-align:text-top;
}
TABLE.marc TR.header
{
    background-color: #eeeeee;
}
TABLE.marc TR.datafield
{
}
TABLE.marc TD.fieldname
{
    border: 0px;
    border-top: 1px;
    border-style: dotted;
    border-color: #cccccc;
}
TABLE.marc TD.fieldname, TABLE.marc TD.indicator, TABLE.marc TR.header TD.content, TABLE.marc SPAN
{
     font-family: Courier New, Tahoma, Arial, Helvetica, sans-serif;
     font-weight: bold;
}
TABLE.marc TD.indicator
{
    padding-left: 4px;
    padding-right: 4px;
    
    border: 0px;
    border-left: 1px;
    border-right: 1px;
    border-style: dotted;
    border-color: #eeeeee;
}
TABLE.marc SPAN.subfield
{
    margin: 2px;
    margin-left: 0px;
    line-height: 140%;
        
    border: 1px;
    border-style: solid;
    border-color: #cccccc;
    
    padding-top: 1px;
    padding-bottom: 1px;
    padding-left: 3px;
    padding-right: 3px;
    font-weight: bold;
    color: Blue;
    background-color: Yellow;
}
TABLE.marc SPAN.fieldend
{
    margin: 2px;
    margin-left: 4px;
    
    border: 1px;
    border-style: solid;
    border-color: #cccccc;
    
    padding-top: 1px;
    padding-bottom: 1px;
    padding-left: 3px;
    padding-right: 3px;
    font-weight: bold;
    color: White;
    background-color: #cccccc;
}
</style>
</head>";
            string strMARC = record.Text;

            string strHtml = "<html>" +
    strHead +
    "<body>" +
    MarcUtil.GetHtmlOfMarc(strMARC, false) +
    "</body></html>";

            AppendHtml(this.webBrowser1, strHtml, true);
            return;
        ERROR1:
            ClearHtml();
        }


#if REMOVED
        void Display()
        {
            string strError = "";
            string strHtml2 = "";

            int nRet = GetXmlHtml(
    this.SavingXml,
    this.SavedXml,
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

#endif
    }
}
