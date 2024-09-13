using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// 源记录。显示在左侧
        /// </summary>
        public string SourceMarc = "";

        /// <summary>
        /// 目标记录的原有内容
        /// </summary>
        public string TargetOldMarc = "";

        /// <summary>
        /// 目标记录的修改后内容。显示在右侧
        /// </summary>
        public string TargetNewMarc = "";


        public VerifyMarcResultDialog()
        {
            InitializeComponent();
        }

        public void FocusToWebControl()
        {
            this.webBrowser1.Focus();
        }

        private void VerifyMarcResultDialog_Load(object sender, EventArgs e)
        {
            if (this.DesignMode)
                return;

            // index 大的在上
            _buttons.Add(this.toolStripButton_targetOld);
            _buttons.Add(this.toolStripButton_source);
            _buttons.Add(this.toolStripButton_targetNew);

            if (string.IsNullOrEmpty(this.TargetOldMarc))
                this.toolStripButton_targetOld.Visible = false;

            var strings = GetStrings();
            Display(strings[0].Title,
                strings[0].Text,
                strings[1].Title,
                strings[1].Text);
        }

        public string ButtonSourceCaption
        {
            get
            {
                return this.toolStripButton_source.Text;
            }
            set
            {
                this.toolStripButton_source.Text = value;
            }
        }

        public string ButtonTargetOldCaption
        {
            get
            {
                return this.toolStripButton_targetOld.Text;
            }
            set
            {
                this.toolStripButton_targetOld.Text = value;
            }
        }

        public string ButtonTargetNewCaption
        {
            get
            {
                return this.toolStripButton_targetNew.Text;
            }
            set
            {
                this.toolStripButton_targetNew.Text = value;
            }
        }

        List<ToolStripButton> _buttons = new List<ToolStripButton>();

        // 重新安排按钮按下状态
        void PressButton(ToolStripButton button)
        {
            _buttons.Remove(button);
            _buttons.Add(button);
        }

        class TitleAndText
        {
            public string Title { get; set; }
            public string Text { get; set; }
        }

        // 获得上层的两个按钮对应的字符串
        List<TitleAndText> GetStrings()
        {
            List<TitleAndText> results = new List<TitleAndText>();
            var buttons = new List<ToolStripButton>();
            buttons.AddRange(_buttons);
            buttons.RemoveAt(0);

            if (buttons.IndexOf(this.toolStripButton_source) != -1)
                results.Add(new TitleAndText
                {
                    Title = this.toolStripButton_source.Text,
                    Text = this.SourceMarc
                });
            if (buttons.IndexOf( this.toolStripButton_targetOld) != -1)
                results.Add(new TitleAndText
                {
                    Title = this.toolStripButton_targetOld.Text,
                    Text = this.TargetOldMarc
                });
            if (buttons.IndexOf( this.toolStripButton_targetNew) != -1)
                results.Add(new TitleAndText
                {
                    Title = this.toolStripButton_targetNew.Text,
                    Text = this.TargetNewMarc
                });

            int i = 0;
            foreach (var button in _buttons)
            {
                if (i > 0)
                {
                    button.Checked = true;
                    /*
                    if (button == this.toolStripButton_source)
                        results.Add(new TitleAndText
                        {
                            Title = this.toolStripButton_source.Text,
                            Text = this.SourceMarc
                        });
                    else if (button == this.toolStripButton_targetOld)
                        results.Add(new TitleAndText
                        {
                            Title = this.toolStripButton_targetOld.Text,
                            Text = this.TargetOldMarc
                        });
                    else if (button == this.toolStripButton_targetNew)
                        results.Add(new TitleAndText
                        {
                            Title = this.toolStripButton_targetNew.Text,
                            Text = this.TargetNewMarc
                        });
                    */
                }
                else
                    button.Checked = false;
                i++;
            }

            Debug.Assert(results.Count == 2);

            return results;
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

        void Display(
            string leftTitle,
            string left,
            string rightTitle,
            string right)
        {
            string strError = "";
            string strHtml2 = "";

            int nRet = GetHtml(
                leftTitle,
                left,
                rightTitle,
                right,
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
            string oldTitle,
string strOldMARC,
string newTitle,
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
                    oldTitle,
                    strOldMARC,
                    strOldFragmentXml,
                    "",
                    newTitle,
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

        //internal string _leftTitle = "原始记录";
        //internal string _rightTitle = "修改后的记录";

        private void toolStripButton_copyLeftToClipboard_Click(object sender, EventArgs e)
        {
            var strings = GetStrings();
            TextToClipboardFormat(strings[0].Text);
        }

        private void toolStripButton_copyRightToClipboard_Click(object sender, EventArgs e)
        {
            var strings = GetStrings();
            TextToClipboardFormat(strings[1].Text);
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

        private void toolStripButton_source_Click(object sender, EventArgs e)
        {
            PressButton(sender as ToolStripButton);
            var strings = GetStrings();
            Display(strings[0].Title,
                strings[0].Text,
                strings[1].Title,
                strings[1].Text);

            if (_buttons.IndexOf(this.toolStripButton_targetNew) == 0)
                this.button_acceptChangedMarc.Enabled = false;
            else
                this.button_acceptChangedMarc.Enabled = true;
        }
    }
}
