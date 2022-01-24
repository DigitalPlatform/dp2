using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Web;
using System.Collections;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Core;
using System.Threading.Tasks;

namespace dp2Circulation
{
    /// <summary>
    /// 实用工具窗
    /// </summary>
    public partial class UtilityForm : MyForm
    {
        // public MainForm MainForm = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UtilityForm()
        {
            InitializeComponent();
        }

        private void UtilityForm_Load(object sender, EventArgs e)
        {
            FillEncodingList(this.comboBox_xmlFile_encoding,
                false);
            FillEncodingList(this.comboBox_worToIso_encoding,
    false);

            this.UiState = Program.MainForm.AppInfo.GetString(
                "utilityform",
                "ui_state",
                "");

#if NO
            this.textBox_serverFilePath.Text = Program.MainForm.AppInfo.GetString(
                "utilityform",
                "server_file_path",
                "");
            this.textBox_clientFilePath.Text = Program.MainForm.AppInfo.GetString(
    "utilityform",
    "client_file_path",
    "");
#endif

            FillSystemInfo();
        }

        private void UtilityForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_webCamera != null)
            {
                m_webCamera.CloseWebcam();
                m_webCamera = null;
            }
        }

        private void UtilityForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            Program.MainForm.AppInfo.SetString(
    "utilityform",
    "server_file_path",
    this.textBox_serverFilePath.Text);
            Program.MainForm.AppInfo.SetString(
    "utilityform",
    "client_file_path",
    this.textBox_clientFilePath.Text);
#endif
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
        "utilityform",
        "ui_state",
        this.UiState);
            }
        }

        private void button_sjhm_getOriginInfo_Click(object sender, EventArgs e)
        {
            this.textBox_sjhm_result.Text = "";

            string strError = "";

            string strOutputHanzi = "";
            List<string> sjhms = null;
            // 把字符串中的汉字转换为四角号码
            // parameters:
            //      bLocal  是否从本地获取四角号码
            //      strOutputText   [out]去除各种符号后的汉字字符串
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            int nRet = HanziTextToSjhm(
            this.textBox_sjhm_source.Text,
            out strOutputHanzi,
            out sjhms,
            out strError);
            if (nRet == -1)
            {
                this.textBox_sjhm_result.Text = strError;
                return;
            }

            string strText = "";
            for (int i = 0; i < sjhms.Count; i++)
            {
                string strOne = sjhms[i];
                char ch = strOutputHanzi[i];

                strText += ch.ToString() + "[" + strOne + "]\r\n";
            }

            this.textBox_sjhm_result.Text = strText;
        }

        // 把字符串中的汉字转换为四角号码
        // parameters:
        //      strOutputText   [out]去除各种符号后的汉字字符串
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        int HanziTextToSjhm(
            string strText,
            out string strOutputText,
            out List<string> sjhms,
            out string strError)
        {
            strError = "";
            strOutputText = "";
            sjhms = new List<string>();

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                {
                    continue;
                }

                // 汉字
                string strHanzi = "";
                strHanzi += ch;


                string strResultSjhm = "";

                int nRet = 0;


                nRet = Program.MainForm.LoadQuickSjhm(true, out strError);
                if (nRet == -1)
                    return -1;
                nRet = Program.MainForm.QuickSjhm.GetSjhm(
                    strHanzi,
                    out strResultSjhm,
                    out strError);


                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceled
                    return 0;
                }

                Debug.Assert(strResultSjhm != "", "");

                strResultSjhm = strResultSjhm.Trim();
                sjhms.Add(strResultSjhm);

                strOutputText += strHanzi;
            }

            return 1;   // 正常结束
        }

        bool m_bExceedMode = false;

        private void button_xmlEditor_load_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的XML文件名";
            dlg.FileName = this.textBox_xmlEditor_xmlFilename.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_xmlEditor_xmlFilename.Text = dlg.FileName;

            string strError = "";
            string strContent = "";
            Encoding encoding = null;
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(this.textBox_xmlEditor_xmlFilename.Text,
                1024 * 1024, // 1024K
                out strContent,
                out encoding,
                out strError);
            if (nRet == 1 || nRet == 2)
            {
                bool bExceed = nRet == 2;
                string strXml = "";
                if (this.checkBox_xmlEditor_indent.Checked == true)
                {
                    nRet = DomUtil.GetIndentXml(strContent,
                        true,
                        out strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        if (bExceed == false)
                            MessageBox.Show(this, strError);
                        strXml = strContent;
                    }
                }
                else
                    strXml = strContent;

                this.textBox_xmlEditor_content.Text =
                    (bExceed == true ? "文件尺寸太大，下面只显示了开头部分...\r\n" : "") + strXml;
                if (bExceed == true)
                    MessageBox.Show(this, "文件尺寸太大，下面只显示了开头部分");

                this.m_bExceedMode = bExceed;
            }
            else
            {
                this.textBox_xmlEditor_content.Text = "";
                this.m_bExceedMode = false;
            }

            if (encoding == null)
                this.comboBox_xmlFile_encoding.Text = "";
            else
                this.comboBox_xmlFile_encoding.Text = encoding.BodyName;

            SetExceedMode();
        }

        void SetExceedMode()
        {
            if (this.m_bExceedMode == true)
            {
                this.checkBox_xmlEditor_indent.Enabled = false;
                this.button_xmlEditor_save.Enabled = false;
                this.textBox_xmlEditor_content.ReadOnly = true;
            }
            else
            {
                this.checkBox_xmlEditor_indent.Enabled = true;
                this.button_xmlEditor_save.Enabled = true;
                this.textBox_xmlEditor_content.ReadOnly = false;
            }
        }

        private void checkBox_xmlEditor_indent_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_xmlEditor_indent.Checked == true)
            {
                string strError = "";
                string strXml = "";
                int nRet = DomUtil.GetIndentXml(this.textBox_xmlEditor_content.Text,
                    true,
                    out strXml,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                this.textBox_xmlEditor_content.Text = strXml;
            }
        }



        private void button_xmlEditor_save_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的XML文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_xmlEditor_xmlFilename.Text;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_xmlEditor_xmlFilename.Text = dlg.FileName;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.textBox_xmlEditor_content.Text);
            }
            catch (Exception ex)
            {
                strError = "XML格式不合法: " + ex.Message;
                goto ERROR1;
            }

            /*
            string strEncoding = DomUtil.GetDomEncodingString(dom);
            Encoding encoding = Encoding.UTF8;
            if (string.IsNullOrEmpty(strEncoding) == false)
            {
                try
                {
                    encoding = Encoding.GetEncoding(strEncoding);
                }
                catch
                {
                    encoding = Encoding.UTF8;
                }
            }
             * */

            Encoding encoding = Encoding.UTF8;
            if (string.IsNullOrEmpty(this.comboBox_xmlFile_encoding.Text) == false)
            {
                try
                {
                    encoding = Encoding.GetEncoding(this.comboBox_xmlFile_encoding.Text);
                }
                catch
                {
                    encoding = Encoding.UTF8;
                }
            }


            try
            {
                using (XmlTextWriter w = new XmlTextWriter(this.textBox_xmlEditor_xmlFilename.Text,
                    encoding))
                {
                    if (this.checkBox_xmlEditor_indent.Checked == true)
                    {
                        w.Formatting = Formatting.Indented;
                        w.Indentation = 4;
                    }

                    dom.WriteTo(w);
                }
            }
            catch (Exception ex)
            {
                strError = "XML文件保存过程中出错: " + ex.Message;
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /*public*/
        static void FillEncodingList(ComboBox list,
            bool bHasMarc8)
        {
            list.Items.Clear();

            List<string> encodings = GetEncodingList(bHasMarc8);
            for (int i = 0; i < encodings.Count; i++)
            {
                list.Items.Add(encodings[i]);
            }
        }

        // 列出encoding名列表
        // 需要把gb2312 utf-8等常用的提前
        /*public*/
        static List<string> GetEncodingList(bool bHasMarc8)
        {
            List<string> result = new List<string>();

            EncodingInfo[] infos = Encoding.GetEncodings();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].GetEncoding().Equals(Encoding.GetEncoding(936)) == true)
                    result.Insert(0, infos[i].Name);
                else if (infos[i].GetEncoding().Equals(Encoding.UTF8) == true)
                    result.Insert(0, infos[i].Name);
                else
                    result.Add(infos[i].Name);
            }

            if (bHasMarc8 == true)
                result.Add("MARC-8");

            return result;
        }

        private void comboBox_xmlFile_encoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strEncoding = this.comboBox_xmlFile_encoding.Text;

            string strError = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.textBox_xmlEditor_content.Text);
            }
            catch (Exception ex)
            {
                strError = "XML格式不合法: " + ex.Message;
                goto ERROR1;
            }

            if (DomUtil.GetDomEncodingString(dom) != this.comboBox_xmlFile_encoding.Text)
            {
                bool bRet = DomUtil.SetDomEncodingString(dom, this.comboBox_xmlFile_encoding.Text);
                if (bRet == true)
                    this.textBox_xmlEditor_content.Text = DomUtil.GetIndentXml(dom);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        WebCamera m_webCamera = null;
        // bool m_bStopCamera = true;

        private void button_webCamera_start_Click(object sender, EventArgs e)
        {
            if (m_webCamera == null)
            {
                m_webCamera = new WebCamera(this.panel_camera_preview.Handle,
                    panel_camera_preview.Width, panel_camera_preview.Height);
                m_webCamera.StartWebCam();
                //m_bStopCamera = false;
                this.button_webCamera_stop.Enabled = true;
                this.button_webCamera_copyToClipboard.Enabled = true;
            }
        }

        private void button_webCamera_stop_Click(object sender, EventArgs e)
        {
            if (m_webCamera != null)
            {
                m_webCamera.CloseWebcam();
                m_webCamera = null;
                //m_bStopCamera = true;
                this.button_webCamera_stop.Enabled = false;
                this.button_webCamera_start.Enabled = true;
                this.button_webCamera_copyToClipboard.Enabled = false;
            }
        }

        private void button_webCamera_copyToClipboard_Click(object sender, EventArgs e)
        {
            if (m_webCamera != null)
            {
                m_webCamera.Capture();
            }
        }

        /// <summary>
        /// 激活“摄像头”属性页
        /// </summary>
        public void ActivateWebCameraPage()
        {
            this.tabControl_main.SelectedTab = this.tabPage_webCamera;
        }

        private void button_currency_sum_Click(object sender, EventArgs e)
        {
            List<string> prices = new List<string>(this.textBox_currency_source.Lines);
            this.textBox_currency_target.Text = PriceUtil.TotalPrice(prices);
        }

        private void toolStripButton_textLines_sub_Click(object sender, EventArgs e)
        {
            this.textBox_textLines_target.Text = "";

            List<string> left = new List<string>(this.textBox_textLines_source1.Lines);
            List<string> right = new List<string>(this.textBox_textLines_source2.Lines);

            left.Sort();
            StringUtil.RemoveDup(ref left, true);
            right.Sort();
            StringUtil.RemoveDup(ref right, true);

            string strDebugInfo = "";
            string strError = "";

            List<string> targetLeft = new List<string>();
            List<string> targetMiddle = null;
            List<string> targetRight = null;

            int nRet = StringUtil.LogicOper("SUB",
    left,
    right,
    ref targetLeft,
    ref targetMiddle,
    ref targetRight,
    false,
    out strDebugInfo,
    out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            this.textBox_textLines_target.Text = StringUtil.MakePathList(targetLeft, "\r\n");
        }

        private void toolStripButton_textLines_and_Click(object sender, EventArgs e)
        {
            this.textBox_textLines_target.Text = "";

            List<string> left = new List<string>(this.textBox_textLines_source1.Lines);
            List<string> right = new List<string>(this.textBox_textLines_source2.Lines);

            left.Sort();
            StringUtil.RemoveDup(ref left, true);
            right.Sort();
            StringUtil.RemoveDup(ref right, true);

            string strDebugInfo = "";
            string strError = "";

            List<string> targetLeft = null;
            List<string> targetMiddle = new List<string>();
            List<string> targetRight = null;

            int nRet = StringUtil.LogicOper("AND",
    left,
    right,
    ref targetLeft,
    ref targetMiddle,
    ref targetRight,
    false,
    out strDebugInfo,
    out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            this.textBox_textLines_target.Text = StringUtil.MakePathList(targetMiddle, "\r\n");

        }

        private void toolStripButton_textLines_sort1_Click(object sender, EventArgs e)
        {
            this.textBox_textLines_target.Text = "";

            List<string> left = new List<string>(this.textBox_textLines_source1.Lines);

            left.Sort();
            // StringUtil.RemoveDup(ref left);
            this.textBox_textLines_target.Text = StringUtil.MakePathList(left, "\r\n");

        }

        private void toolStripButton_textLines_sort1removedup_Click(object sender, EventArgs e)
        {
            this.textBox_textLines_target.Text = "";

            List<string> left = new List<string>(this.textBox_textLines_source1.Lines);

            left.Sort();
            StringUtil.RemoveDup(ref left, true);
            this.textBox_textLines_target.Text = StringUtil.MakePathList(left, "\r\n");

        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.Invoke((Action)(() =>
            {
                this.tabControl_main.Enabled = bEnable;
            }));
        }

        // 评估网络速度
        private void button_health_speedTest_Click(object sender, EventArgs e)
        {
            string strError = "";
            //int nRet = 0;
            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在测算网络速度 ...");
            stop.BeginLoop();

            try
            {
                string strVersion = "";
                string strUID = "";

                // 第一次可能要登录，先不计算时间
                long lRet = this.Channel.GetVersion(
    this.stop,
    out strVersion,
    out strUID,
    out strError);
                if (lRet == -1)
                    goto ERROR1;

                DateTime start = DateTime.Now;
                // 循环 n 次
                for (int i = 0; i < 10; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }
                    lRet = this.Channel.GetVersion(
this.stop,
out strVersion,
out strUID,
out strError);
                    if (lRet == -1)
                        goto ERROR1;
                }
                TimeSpan delta = DateTime.Now - start;

                double m_secs = delta.TotalMilliseconds;    // 221

                int score = (int)(((double)50 * 10 / m_secs) * (double)100);
                this.label_health_message.Text = $"{score.ToString()}\r\n\r\n以每次通讯 50 毫秒为基准，满分 100\r\n服务器: {Program.MainForm.LibraryServerUrl}\r\n检测时间: {DateTime.Now.ToString()}";
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return;
        ERROR1:
            this.label_health_message.Text = "";
            MessageBox.Show(this, strError);
        }

        void ConvertISBN(string strAction)
        {
            string strError = "";

            int nRet = Program.MainForm.LoadIsbnSplitter(true, out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            EnableControls(false);
            try
            {
                StringBuilder text = new StringBuilder(4096);
                foreach (string strText in this.textBox_isbn_text.Lines)
                {
                    if (text.Length > 0)
                        text.Append("\r\n");

                    if (string.IsNullOrEmpty(strText) == true)
                    {
                        // text.Append("\r\n");
                        continue;
                    }

                    if (strText[0] == '?')
                    {
                        text.Append(strText);
                        continue;
                    }

                    string strTarget = "";
                    nRet = Program.MainForm.IsbnSplitter.IsbnInsertHyphen(
                       strText,
                       strAction,   // "force10",
                        out strTarget,
                        out strError);
                    if (nRet == -1)
                    {
                        text.Append("? " + strText + " " + strError);
                    }
#if NO
                    if (this.toolStripButton_isbn_hyphen.Checked == false)
                        strTarget = strTarget.Replace("-", "");
#endif
                    text.Append(strTarget);
                }

                this.textBox_isbn_text.Text = text.ToString();
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void toolStripButton_isbn_to10_Click(object sender, EventArgs e)
        {
            ConvertISBN("force10");
        }

        private void toolStripButton_isbn_to13_Click(object sender, EventArgs e)
        {
            ConvertISBN("force13");
        }

        private void button_findClientFilePath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要上传的文件";
            dlg.FileName = this.textBox_clientFilePath.Text;
            dlg.Filter = "所有文件 All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_clientFilePath.Text = dlg.FileName;
        }

        // 探测一个服务器文件是否存在
        // 返回其时间戳
        // return:
        //      -1  出错
        //      0   不存在
        //      1   存在
        public int ServerFileExists(string strServerFilePath,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;

            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.Initial("正在探索文件信息 ...");
            this.stop.BeginLoop();
            try
            {
                byte[] baContent = null;
                string strMetadata = "";
                string strOutputResPath = "";
                long lRet = this.Channel.GetRes(
                   this.stop,
                   strServerFilePath,
                   0,
                   0,
                   "timestamp",
                   out baContent,
                   out strMetadata,
                   out strOutputResPath,
                   out baOutputTimestamp,
                   out strError);
                if (lRet == -1)
                {
                    if (this.Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                        return 0;
                    return -1;
                }
                return 1;
            }
            finally
            {
                this.stop.EndLoop();
                this.stop.OnStop -= new StopEventHandler(this.DoStop);
                this.stop.Initial("");
            }
        }

        // 上传文件到到 dp2lbrary 服务器
        // parameters:
        //      timestamp   时间戳。如果为 null，函数会自动根据文件信息得到一个时间戳
        //      bRetryOverwiteExisting   是否自动在时间戳不一致的情况下覆盖已经存在的服务器文件。== true，表示当发现时间戳不一致的时候，自动用返回的时间戳重试覆盖
        // return:
        //		-1	出错
        //		0   上传文件成功
        public int UploadFile(
            string strClientFilePath,
            string strServerFilePath,
            string strStyle,
            byte[] timestamp,
            bool bRetryOverwiteExisting,
            out string strError)
        {
            strError = "";

            StopStyle old_stop_style = StopStyle.None;

            if (this.stop != null)
            {
                old_stop_style = this.stop.Style;
                this.stop.Style = StopStyle.EnableHalfStop;

                this.stop.OnStop += new StopEventHandler(this.DoStop);
                this.stop.Initial("正在上传文件 ...");
                this.stop.BeginLoop();
            }

            try
            {
                string strResPath = strServerFilePath;

#if NO
                string strMime = API.MimeTypeFrom(ResObjectDlg.ReadFirst256Bytes(strClientFilePath),
"");
#endif
                string strMime = PathUtil.MimeTypeFrom(strClientFilePath);

                // 检测文件尺寸
                FileInfo fi = new FileInfo(strClientFilePath);
                if (fi.Exists == false)
                {
                    strError = "文件 '" + strClientFilePath + "' 不存在...";
                    return -1;
                }

                string[] ranges = null;

                if (fi.Length == 0)
                {
                    // 空文件
                    ranges = new string[1];
                    ranges[0] = "";
                }
                else
                {
                    string strRange = "";
                    strRange = "0-" + Convert.ToString(fi.Length - 1);

                    // 按照100K作为一个chunk
                    // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                    ranges = RangeList.ChunkRange(strRange,
                        this.Channel.UploadResChunkSize // 500 * 1024
                        );
                }

                if (timestamp == null)
                    timestamp = FileUtil.GetFileTimestamp(strClientFilePath);

                byte[] output_timestamp = null;

                // REDOWHOLESAVE:
                string strWarning = "";

                for (int j = 0; j < ranges.Length; j++)
                {
                    // REDOSINGLESAVE:

                    Application.DoEvents();	// 出让界面控制权

                    if (this.stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strWaiting = "";
                    if (j == ranges.Length - 1)
                        strWaiting = " 请耐心等待...";

                    string strPercent = "";
                    RangeList rl = new RangeList(ranges[j]);
                    if (rl.Count >= 1)
                    {
                        double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    if (this.stop != null)
                        this.stop.SetMessage("正在上载 " + ranges[j] + "/"
                            + Convert.ToString(fi.Length)
                            + " " + strPercent + " " + strClientFilePath + strWarning + strWaiting);
                    int nRedoCount = 0;
                REDO:
                    long lRet = this.Channel.SaveResObject(
                        this.stop,
                        strResPath,
                        strClientFilePath,
                        strClientFilePath,
                        strMime,
                        ranges[j],
                        // j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                        timestamp,
                        strStyle,
                        out output_timestamp,
                        out strError);
                    timestamp = output_timestamp;

                    strWarning = "";

                    if (lRet == -1)
                    {
                        // 如果是第一个 chunk，自动用返回的时间戳重试一次覆盖
                        if (bRetryOverwiteExisting == true
                            && j == 0
                            && this.Channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch
                            && nRedoCount == 0)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        goto ERROR1;
                    }
                }

                return 0;
            ERROR1:
                return -1;
            }
            finally
            {
                if (this.stop != null)
                {
                    this.stop.EndLoop();
                    this.stop.OnStop -= new StopEventHandler(this.DoStop);
                    this.stop.Initial("上传文件完成");
                    this.stop.Style = old_stop_style;
                }
            }
        }

        private void button_upload_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_serverFilePath.Text) == true)
            {
                strError = "尚未指定服务器端文件路径";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_clientFilePath.Text) == true)
            {
                strError = "尚未指定前端文件路径";
                goto ERROR1;
            }

            // 获得最后修改时间参数
            FileInfo fi = new FileInfo(this.textBox_clientFilePath.Text);
            string strParam = "last_write_time:" + ByteArray.GetHexTimeStampString(BitConverter.GetBytes(fi.LastWriteTimeUtc.Ticks));

            this.EnableControls(false);

            try
            {
                byte[] baOutputTimestamp = null;

                // return:
                //      -1  出错
                //      0   不存在
                //      1   存在
                int nRet = ServerFileExists(this.textBox_serverFilePath.Text,
                    out baOutputTimestamp,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                {
                    DialogResult result = MessageBox.Show(this,
"服务器端文件 '" + this.textBox_serverFilePath.Text + "' 已经存在。\r\n\r\n请问是否要覆盖它?",
"UtilityForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                        return;
                }

                // 上传文件到到 dp2lbrary 服务器
                // return:
                //		-1	出错
                //		0   上传文件成功
                nRet = UploadFile(
                    this.textBox_clientFilePath.Text,
                    this.textBox_serverFilePath.Text,
                    strParam, // "ignorechecktimestamp",
                    baOutputTimestamp,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_download_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_serverFilePath.Text) == true)
            {
                strError = "尚未指定服务器端文件路径";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_clientFilePath.Text) == true)
            {
                strError = "尚未指定前端文件路径";
                goto ERROR1;
            }

            StopStyle old_stop_style = StopStyle.None;
            old_stop_style = this.stop.Style;
            this.stop.Style = StopStyle.EnableHalfStop;

            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.Initial("正在下载文件 ...");
            this.stop.BeginLoop();

            this.EnableControls(false);

            try
            {
                string strMetaData = "";
                byte[] baOutputTimeStamp = null;
                string strOutputPath = "";
                // parameters:
                //		strOutputFileName	输出文件名。可以为null。如果调用前文件已经存在, 会被覆盖。
                // return:
                //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                //		0	成功
                long lRet = this.Channel.GetRes(
                    this.stop,
                    this.textBox_serverFilePath.Text,
                    this.textBox_clientFilePath.Text,
                    // "metadata",
                    "content,data,metadata,timestamp,outputpath,gzip",  // 2017/10/7 增加 gzip
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                // 根据返回的时间戳设置文件最后修改时间
                FileUtil.SetFileLastWriteTimeByTimestamp(this.textBox_clientFilePath.Text, baOutputTimeStamp);
            }
            finally
            {
                this.stop.EndLoop();
                this.stop.OnStop -= new StopEventHandler(this.DoStop);
                this.stop.Initial("下载文件完成");
                this.stop.Style = old_stop_style;

                this.EnableControls(true);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void FillSystemInfo()
        {
            this.textBox_systemInfo_mac.Text = StringUtil.MakePathList(SerialCodeForm.GetMacAddress());
        }

        private void button_worToIso_findWorFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定即将被转换的 MARC 工作单文件名";
            dlg.FileName = this.textBox_worToIso_worFilename.Text;

            dlg.Filter = "MARC 工作单文件 (*.wor;*.txt)|*.wor;*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_worToIso_worFilename.Text = dlg.FileName;
        }

        private void comboBox_worToIso_encoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayFirstWorksheetRecord();
        }

        private void textBox_worToIso_worFilename_TextChanged(object sender, EventArgs e)
        {
            DisplayFirstWorksheetRecord();
        }

        private void button_worToIso_convert_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(this.textBox_worToIso_worFilename.Text) == true)
            {
                strError = "尚未指定工作单文件名";
                goto ERROR1;
            }

            Encoding encoding = MarcUtil.GetEncoding(this.comboBox_worToIso_encoding.Text);
            if (encoding == null)
                encoding = Encoding.GetEncoding(936);

            Encoding preferredEncoding = Encoding.UTF8;

            OpenMarcFileDlg dlg = new OpenMarcFileDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.Text = "请指定目标 ISO2709 文件名";
            dlg.IsOutput = true;
            dlg.FileName = "";
            // dlg.CrLf = false;
            dlg.CrLfVisible = false;   // 2020/3/9
            dlg.AddG01Visible = false;
            dlg.RemoveField998Visible = false;
            //dlg.RemoveField998 = m_mainForm.LastRemoveField998;
            dlg.EncodingListItems = Global.GetEncodingList(false);
            dlg.MarcSyntax = "<自动>";    // strPreferedMarcSyntax;
            dlg.EnableMarcSyntax = false;

            Program.MainForm.AppInfo.LinkFormState(dlg, "OpenMarcFileDlg_forOutput_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool unimarc_modify_100 = dlg.UnimarcModify100;

            Encoding targetEncoding = null;

            nRet = Global.GetEncoding(dlg.EncodingName,
                out targetEncoding,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            bool bExist = File.Exists(dlg.FileName);
            bool bAppend = false;

            if (bExist == true)
            {
                DialogResult result = MessageBox.Show(this,
        "文件 '" + dlg.FileName + "' 已存在，是否以追加方式写入记录?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)放弃",
        "UtilityForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    bAppend = true;

                if (result == DialogResult.No)
                    bAppend = false;

                if (result == DialogResult.Cancel)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }
            }


            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在转换文件格式 ...");
            stop.BeginLoop();

            try
            {
                using (TextReader reader = new StreamReader(this.textBox_worToIso_worFilename.Text, encoding))
                using (Stream target = File.Open(dlg.FileName,
                         FileMode.OpenOrCreate))
                {
                    if (bAppend == false)
                        target.SetLength(0);
                    else
                        target.Seek(0, SeekOrigin.End);

                    for (int i = 0; ; i++)
                    {
                        stop.SetMessage("正在转换 " + (i + 1).ToString());

                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                        string strMARC = "";
                        // return:
                        //	-2	MARC格式错
                        //	-1	出错
                        //	0	正确
                        //	1	结束(当前返回的记录有效)
                        //	2	结束(当前返回的记录无效)
                        nRet = MarcUtil.ReadWorksheetRecord(reader,
                out strMARC,
                out strError);
                        if (nRet == -1 || nRet == -2)
                            goto ERROR1;
                        if (nRet == 2)
                            break;

                        if (dlg.Mode880 == true && dlg.MarcSyntax == "usmarc")
                        {
                            MarcRecord record = new MarcRecord(strMARC);
                            MarcQuery.To880(record);
                            strMARC = record.Text;
                        }

                        byte[] baTarget = null;

                        // 将MARC机内格式转换为ISO2709格式
                        // parameters:
                        //      strSourceMARC   [in]机内格式MARC记录。
                        //      strMarcSyntax   [in]为"unimarc"或"usmarc"
                        //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
                        //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
                        // return:
                        //      -1  出错
                        //      0   成功
                        nRet = MarcUtil.CvtJineiToISO2709(
                            strMARC,
                            dlg.MarcSyntax,
                            targetEncoding,
                            unimarc_modify_100 ? "unimarc_100" : "",
                            out baTarget,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        target.Write(baTarget, 0,
                            baTarget.Length);

                        if (dlg.CrLf == true)
                        {
                            byte[] baCrLf = targetEncoding.GetBytes("\r\n");
                            target.Write(baCrLf, 0,
                                baCrLf.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "转换过程出现异常: " + ex.Message;
                goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }
            MessageBox.Show(this, "转换完成。记录已写入文件 " + dlg.FileName + " 中");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void DisplayFirstWorksheetRecord()
        {
            if (string.IsNullOrEmpty(this.textBox_worToIso_worFilename.Text) == true)
            {
                this.textBox_worToIso_preview.Text = "";
                return;
            }

            this.textBox_worToIso_preview.Text = MarcUtil.ReaderFirstWorksheetRecord(this.textBox_worToIso_worFilename.Text,
                this.comboBox_worToIso_encoding.Text);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);

                controls.Add(this.textBox_serverFilePath);
                controls.Add(this.textBox_clientFilePath);


                controls.Add(this.textBox_worToIso_worFilename);
                controls.Add(this.comboBox_worToIso_encoding);

                controls.Add(this.textBox_biblioRecPath);
                controls.Add(this.textBox_biblioTableStyle);

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);

                controls.Add(this.textBox_serverFilePath);
                controls.Add(this.textBox_clientFilePath);


                controls.Add(this.textBox_worToIso_worFilename);
                controls.Add(this.comboBox_worToIso_encoding);

                controls.Add(this.textBox_biblioRecPath);
                controls.Add(this.textBox_biblioTableStyle);

                GuiState.SetUiState(controls, value);
            }
        }

        private void toolStripButton_isbn_removeHyphen_Click(object sender, EventArgs e)
        {
            StringBuilder text = new StringBuilder(4096);
            foreach (string strText in this.textBox_isbn_text.Lines)
            {
                if (string.IsNullOrEmpty(strText) == true)
                {
                    text.Append(strText + "\r\n");
                    continue;
                }

                if (strText[0] == '?')
                {
                    text.Append(strText + "\r\n");
                    continue;
                }

                text.Append(strText.Replace("-", "") + "\r\n");
            }

            this.textBox_isbn_text.Text = text.ToString();
        }

        private void toolStripButton_xmlEditor_replaceControlChar_Click(object sender, EventArgs e)
        {
            this.textBox_xmlEditor_content.Text = DomUtil.ReplaceControlCharsButCrLf(this.textBox_xmlEditor_content.Text, '*');
        }

        private void toolStripButton_xmlEditor_htmlEncode_Click(object sender, EventArgs e)
        {
            this.textBox_xmlEditor_content.Text = HttpUtility.HtmlEncode(this.textBox_xmlEditor_content.Text);
        }

        private void toolStripButton_xmlEditor_htmlDecode_Click(object sender, EventArgs e)
        {
            this.textBox_xmlEditor_content.Text = HttpUtility.HtmlDecode(this.textBox_xmlEditor_content.Text);
        }

        private void button_systemInfo_getClientIP_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strValue = "";

            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.83") < 0)
            {
                strError = "当前连接的 dp2library 版本必须在 2.83 以上才能使用本功能 (但它是 " + Program.MainForm.ServerVersion + ")";
                goto ERROR1;
            }

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在测算网络速度 ...");
            stop.BeginLoop();

            try
            {
                long lRet = this.Channel.GetSystemParameter(stop,
                    "utility",
                    "getClientIP",
                    out strValue,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // InputDlg.GetInput(this, "本机 IP 地址", "本机 IP 地址", strValue, this.Font);
                bool bTemp = false;
                MessageDlg.Show(this,
    "本机 IP 地址为:\r\n" + strValue,
    "dp2Circulation",
    MessageBoxButtons.OK,
    MessageBoxDefaultButton.Button1,
    ref bTemp);

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 取 GCAT 著者号
        private void toolStripButton_textLines_getAuthorNumber_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.textBox_textLines_target.Text = "";
            this.textBox_textLines_source2.Text = "";

            string strGcatWebServiceUrl = Program.MainForm.GcatServerUrl;   // "http://dp2003.com/dp2libraryws/gcat.asmx";

            if (string.IsNullOrEmpty(strGcatWebServiceUrl))
            {
                strError = "尚未配置 GCAT 服务器地址";
                goto ERROR1;
            }

            StringBuilder result1 = new StringBuilder();
            StringBuilder result2 = new StringBuilder();

            this.EnableControls(false);

            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.Initial("正在取著者号 ...");
            this.stop.BeginLoop();
            try
            {
                int count = this.textBox_textLines_source1.Lines.Length;
                stop.SetProgressRange(0, count);
                int i = 0;

                foreach (string line in this.textBox_textLines_source1.Lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        i++;
                        continue;
                    }
                    string strLine = line.Trim();
                    if (string.IsNullOrEmpty(strLine))
                    {
                        i++;
                        continue;
                    }

#if NO
                    Hashtable question_table = (Hashtable)Program.MainForm.ParamTable["question_table"];
                    if (question_table == null)
                        question_table = new Hashtable();
#endif
                    Hashtable question_table = new Hashtable();

                    stop.SetMessage("正在取著者号 '" + strLine + "' ...");

                    string strDebugInfo = "";
                    string strAuthorNumber = "";
                    // return:
                    //      -4  著者字符串没有检索命中
                    //      -2  strID验证失败
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    long nRet = BiblioItemsHost.GetAuthorNumber(
                        ref question_table,
                        this.stop,
                        this,
                        strGcatWebServiceUrl,
                        strLine,
                        "",
                        true,	// bSelectPinyin
                        true,	// bSelectEntry
                        false,	// bOutputDebugInfo
                        out strAuthorNumber,
                        out strDebugInfo,
                        out strError);
#if NO
                    Program.MainForm.ParamTable["question_table"] = question_table;
#endif

                    if (nRet == 0)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        result1.Append(strAuthorNumber + "\r\n");
                        result2.Append(strLine + "\t" + strAuthorNumber + "\r\n");
                    }
                    else
                    {
                        result1.Append("error: " + strError + "\r\n");
                        result2.Append(strLine + "\terror: " + strError + "\r\n");
                    }

                    i++;
                }

                this.textBox_textLines_source2.Text = result1.ToString();
                this.textBox_textLines_target.Text = result2.ToString();
                return;
            }
            finally
            {
                this.stop.EndLoop();
                this.stop.OnStop -= new StopEventHandler(this.DoStop);
                this.stop.Initial("");

                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_getBiblioTable_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.textBox_biblioTableXml.Text = "";

            this.EnableControls(false);

            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.Initial("正在获取书目 table 格式 ...");
            this.stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();
            try
            {
                List<string> formats = new List<string> { "table:" + this.textBox_biblioTableStyle.Text.Replace(",", "|") };
                string[] results = null;
                byte[] timestamp = null;

                long lRet = channel.GetBiblioInfos(
                    stop,
                    this.textBox_biblioRecPath.Text,
                    "",
                    formats.ToArray(),
                    out results,
                    out timestamp,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                        strError = "书目记录 '" + this.textBox_biblioRecPath.Text + "' 不存在";

                    goto ERROR1;
                }
                else
                {
                    Debug.Assert(results != null && results.Length == formats.Count, "results必须包含 " + formats.Count + " 个元素");
                    this.textBox_biblioTableXml.Text = DomUtil.GetIndentXml(results[0]).Replace("\"", "\'");
                }

                return;
            }
            finally
            {
                this.ReturnChannel(channel);

                this.stop.EndLoop();
                this.stop.OnStop -= new StopEventHandler(this.DoStop);
                this.stop.Initial("");

                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // UID 从十六进制转换为十进制
        private void toolStripButton_uidToDecimal_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.textBox_textLines_target.Text = "";
            this.textBox_textLines_source2.Text = "";

            StringBuilder error = new StringBuilder();
            StringBuilder result2 = new StringBuilder();

            this.EnableControls(false);

            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.Initial("正在转换 UID ...");
            this.stop.BeginLoop();
            try
            {
                foreach (string line in this.textBox_textLines_source1.Lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        result2.AppendLine();
                        continue;
                    }
                    string strLine = line.Trim();
                    if (string.IsNullOrEmpty(strLine))
                    {
                        result2.AppendLine();
                        continue;
                    }

                    // 十六进制 6DB28CAF；十进制 2945233517 
                    try
                    {
                        string result = BindCardNumberDialog.HexToDecimal(strLine);
                        result2.AppendLine(result);
                    }
                    catch (Exception ex)
                    {
                        result2.AppendLine($"!!! 字符串 '{strLine}' 格式不正确: {ex.Message}");
                    }
                }

                this.textBox_textLines_source2.Text = error.ToString();
                this.textBox_textLines_target.Text = result2.ToString();
                return;
            }
            finally
            {
                this.stop.EndLoop();
                this.stop.OnStop -= new StopEventHandler(this.DoStop);
                this.stop.Initial("");

                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_uidToHex_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.textBox_textLines_target.Text = "";
            this.textBox_textLines_source2.Text = "";

            StringBuilder error = new StringBuilder();
            StringBuilder result2 = new StringBuilder();

            this.EnableControls(false);

            this.stop.OnStop += new StopEventHandler(this.DoStop);
            this.stop.Initial("正在转换 UID ...");
            this.stop.BeginLoop();
            try
            {
                foreach (string line in this.textBox_textLines_source1.Lines)
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        result2.AppendLine();
                        continue;
                    }
                    string strLine = line.Trim();
                    if (string.IsNullOrEmpty(strLine))
                    {
                        result2.AppendLine();
                        continue;
                    }

                    // 十进制 2945233517；十六进制 6DB28CAF
                    try
                    {
                        string result = BindCardNumberDialog.DecimalToHex(strLine);
                        result2.AppendLine(result);
                    }
                    catch (Exception ex)
                    {
                        result2.AppendLine($"!!! 字符串 '{strLine}' 格式不正确: {ex.Message}");
                    }
                }

                this.textBox_textLines_source2.Text = error.ToString();
                this.textBox_textLines_target.Text = result2.ToString();
                return;
            }
            finally
            {
                this.stop.EndLoop();
                this.stop.OnStop -= new StopEventHandler(this.DoStop);
                this.stop.Initial("");

                this.EnableControls(true);
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 测试当前用户登录
        private void button_health_tryLogin_Click(object sender, EventArgs e)
        {
            string strError = "";
            EnableControls(false);

            this.label_health_message.Text = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在测试当前用户登录 ...");
            stop.BeginLoop();

            this.ShowMessage("正在测试当前用户登录");

            try
            {
                // 先登出一次
                long lRet = this.Channel.Logout(out strError);

                lRet = this.Channel.GetSystemParameter(this.stop,
                    "system",
                    "biblioDbGroup",
                    out string strValue,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                this.label_health_message.Text = $"登录成功";
            }
            finally
            {
                this.ClearMessage();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            return;
        ERROR1:
            this.label_health_message.Text = strError;
            MessageBox.Show(this, strError);
        }

        public void Health()
        {
            this.tabControl_main.SelectedTab = this.tabPage_health;
            button_health_tryLogin_Click(this, new EventArgs());
        }

        private void button_addCrLf_getSourceFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定即将被转换的 ISO2709 文件名";
            dlg.FileName = this.textBox_addCrLf_sourceFilename.Text;

            dlg.Filter = "ISO2709 文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_addCrLf_sourceFilename.Text = dlg.FileName;
            this.textBox_addCrLf_targetFilename.Text = Path.Combine(Path.GetDirectoryName(dlg.FileName), Path.GetFileNameWithoutExtension(dlg.FileName) + "_crlf" + Path.GetExtension(dlg.FileName));
        }

        private void button_addCrLf_getTargetFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定即将创建的 ISO2709 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.textBox_addCrLf_targetFilename.Text;

            dlg.Filter = "ISO2709 文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_addCrLf_targetFilename.Text = dlg.FileName;
        }

        private void button_addCrLf_begin_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.button_addCrLf_begin.Enabled = false;
            try
            {
                var source_filename = this.textBox_addCrLf_sourceFilename.Text;
                var target_filename = this.textBox_addCrLf_targetFilename.Text;

                if (source_filename == target_filename)
                {
                    strError = "源文件和目标文件不能是同一个文件";
                    goto ERROR1;
                }

                // 先检查源文件中是否已经有了回车换行符号
                using (var stream_in = File.OpenRead(source_filename))
                {
                    while (true)
                    {
                        int nRet = stream_in.ReadByte();
                        if (nRet == -1)
                            break;
                        if ((char)nRet == '\r' || (char)nRet == '\n')
                        {
                            strError = $"源文件 {source_filename} 中已经有回车换行符号了，不能重复添加。放弃处理";
                            goto ERROR1;
                        }
                    }
                }

                using (var stream_in = File.OpenRead(source_filename))
                using (var stream_out = File.OpenWrite(target_filename))
                {
                    stream_out.SetLength(0);
                    while (true)
                    {
                        int nRet = stream_in.ReadByte();
                        if (nRet == -1)
                            break;
                        if ((char)nRet == 29)
                        {
                            stream_out.WriteByte((byte)nRet);
                            stream_out.WriteByte((byte)'\r');
                            stream_out.WriteByte((byte)'\n');
                        }
                        else
                            stream_out.WriteByte((byte)nRet);
                    }
                }

                MessageBox.Show(this, $"转换完成。目标文件 {target_filename} 中已经被添加了回车换行符号");
                return;
            }
            catch (Exception ex)
            {
                strError = $"转换出现异常: {ex.Message}";
                goto ERROR1;
            }
            finally
            {
                this.button_addCrLf_begin.Enabled = true;
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 取书目摘要
        private void toolStrip_textLines_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.textBox_textLines_target.Text = "";
            this.textBox_textLines_source2.Text = "";

            _ = Task.Run(() =>
            {
                string strError = "";

                StringBuilder result1 = new StringBuilder();
                StringBuilder result2 = new StringBuilder();

                this.EnableControls(false);

                this.stop.OnStop += new StopEventHandler(this.DoStop);
                this.stop.Initial("正在取书目摘要 ...");
                this.stop.BeginLoop();

                var channel = this.GetChannel();
                try
                {
                    int count = this.textBox_textLines_source1.Lines.Length;
                    stop.SetProgressRange(0, count);
                    int i = 0;
                    foreach (string line in this.textBox_textLines_source1.Lines)
                    {
                        // Application.DoEvents(); // 出让界面控制权
                        if (this.stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        stop.SetProgressValue(i);

                        if (string.IsNullOrEmpty(line))
                        {
                            result1.AppendLine();
                            result2.AppendLine();
                            i++;
                            continue;
                        }
                        string strLine = line.Trim();
                        if (string.IsNullOrEmpty(strLine))
                        {
                            result2.AppendLine();
                            i++;
                            continue;
                        }

                        string recpath = null;
                        string barcode = strLine;
                        if (strLine.IndexOf("/") != -1)
                        {
                            recpath = strLine;
                            barcode = null;
                        }

                        stop.SetMessage("正在取书目摘要 '" + strLine + "' ...");

                        long lRet = channel.GetBiblioSummary(stop,
                            barcode,
                            recpath,
                            null,
                            out _,
                            out string summary,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                        result1.AppendLine(summary);
                        result2.AppendLine(strLine + "\t" + summary);

                        i++;
                    }

                    this.Invoke((Action)(() =>
                    {
                        this.textBox_textLines_source2.Text = result1.ToString();
                        this.textBox_textLines_target.Text = result2.ToString();
                    }));
                    return;
                }
                finally
                {
                    this.ReturnChannel(channel);

                    this.stop.EndLoop();
                    this.stop.OnStop -= new StopEventHandler(this.DoStop);
                    this.stop.Initial("");

                    this.EnableControls(true);
                }
            ERROR1:
                ShowMessageBox(strError);
            });
        }
    }
}
