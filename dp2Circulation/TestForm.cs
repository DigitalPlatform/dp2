using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;   // for WebClient class
using System.Xml;
using System.Linq;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Interfaces;
using DigitalPlatform.EasyMarc;
using DigitalPlatform.AmazonInterface;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    internal partial class TestForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        public TestForm()
        {
            InitializeComponent();

        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            this.textBox_diskSpace_tempFileName.Text = this.MainForm.AppInfo.GetString(
                "TestForm",
                "diskspace_tempfilename",
                "");
            this.textBox_diskSpace_size.Text = this.MainForm.AppInfo.GetString(
                "TestForm",
                "diskspace_size",
                "");
            DispFreeSpace();

            this.checkBox_fromEditControl_hasCaptionsTitleLine.Checked = this.fromEditControl1.HasCaptionsTitleLine;

            // CheckedComboBox page
            this.textBox_checkedComboBox_listValues.Text = this.MainForm.AppInfo.GetString(
                "TestForm",
                "checkedcombobox_listvalues",
                "");

            this.textBox_serverFilePath.Text = this.MainForm.AppInfo.GetString(
                "TestForm",
                "ftp_server_path",
                "");
            this.textBox_clientFilePath.Text = this.MainForm.AppInfo.GetString(
                "TestForm",
                "ftp_client_path",
                "");

            this.UiState = this.MainForm.AppInfo.GetString(
                "TestForm",
                "ui_state",
                "");

            // this.entityRegisterControl1.Font = this.Font;

        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
                    "TestForm",
                    "diskspace_tempfilename",
                    this.textBox_diskSpace_tempFileName.Text);
                this.MainForm.AppInfo.SetString(
                    "TestForm",
                    "diskspace_size",
                    this.textBox_diskSpace_size.Text);

                // CheckedComboBox page
                this.MainForm.AppInfo.SetString(
                    "TestForm",
                    "checkedcombobox_listvalues",
                    this.textBox_checkedComboBox_listValues.Text);

                this.MainForm.AppInfo.SetString(
        "TestForm",
        "ftp_server_path",
        this.textBox_serverFilePath.Text);
                this.MainForm.AppInfo.SetString(
                    "TestForm",
                    "ftp_client_path",
                    this.textBox_clientFilePath.Text);

                this.MainForm.AppInfo.SetString(
        "TestForm",
        "ui_state",
        this.UiState);
            }
        }

        // 可增加显示磁盘空间的功能
        private void button_diskSpace_writeTempFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            string strText = "";

            if (this.textBox_diskSpace_size.Text == "")
            {
                strError = "尚未指定临时文件尺寸";
                goto ERROR1;
            }

            if (this.textBox_diskSpace_tempFileName.Text == "")
            {
                strError = "尚未指定临时文件名";
                goto ERROR1;
            }

            long lSize = 0;
            string strPrefix = "";
            string strUnit = "";

            nRet = ParseSizeText(this.textBox_diskSpace_size.Text,
                out lSize,
                out strPrefix,
                out strUnit,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strUnit = strUnit.ToUpper();

            if (strUnit == "M")
                lSize = lSize * 1024 * 1024;
            else if (strUnit == "K")
                lSize = lSize * 1024;
            else if (strUnit == "")
            {
                // lSize 不变
            }
            else
            {
                strError = "未知的尺寸单位 '" + strUnit + "'";
                goto ERROR1;
            }

            EnableControls(false);
            try
            {
                Stream s = File.Open(
    this.textBox_diskSpace_tempFileName.Text,
    FileMode.OpenOrCreate,
    FileAccess.ReadWrite,
    FileShare.ReadWrite);

                using (s)
                {
                    if (strPrefix == "+")
                    {
                        strText = "文件尺寸从 " + s.Length.ToString() + " 扩大 " + lSize.ToString() + "，到 " + (lSize + s.Length).ToString() + "。";
                        s.SetLength(lSize + s.Length);
                    }
                    else if (strPrefix == "-")
                    {
                        strText = "文件尺寸从 " + s.Length.ToString() + " 缩小 " + lSize.ToString() + "，到 " + (s.Length - lSize).ToString() + "。";
                        s.SetLength(Math.Max(s.Length - lSize, 0));
                    }
                    else if (strPrefix == "")
                    {
                        strText = "设置文件尺寸为 " + lSize.ToString() + " 。";
                        s.SetLength(lSize);
                    }
                    else
                    {
                        strError = "未知的前缀 '" + strPrefix + "'";
                        goto ERROR1;
                    }
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);
            }

            DispFreeSpace();
            MessageBox.Show(this, strText);
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void button_diskSpace_deleteTempFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_diskSpace_tempFileName.Text == "")
            {
                strError = "尚未指定临时文件名";
                goto ERROR1;
            }
            EnableControls(false);
            try
            {

                File.Delete(this.textBox_diskSpace_tempFileName.Text);
            }
            finally
            {
                EnableControls(true);
            }
            DispFreeSpace();
            MessageBox.Show(this, "OK");
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 解析尺寸字符串
        // 尺寸字符串的格式为：前缀 + 数字 + 单位
        // 其中，前缀为 + 或 - , 单位为 M或 K
        static int ParseSizeText(string strSize,
            out long lSize,
            out string strPrefix,
            out string strUnit,
            out string strError)
        {
            lSize = 0;
            strPrefix = "";
            strUnit = "";
            strError = "";

            bool bInDigit = false;
            bool bInPrefix = true;

            string strDigit = "";

            for (int i = 0; i < strSize.Length; i++)
            {
                char c = strSize[i];
                if (c >= '0' && c <= '9')
                {
                    bInDigit = true;
                    bInPrefix = false;
                }

                if (bInPrefix == true)
                    strPrefix += c;
                else
                {
                    if (bInDigit == true)
                        strDigit += c;
                    else
                        strUnit += c;
                }
            }

            if (strDigit == "")
                strDigit = "0";
            lSize = Convert.ToInt64(strDigit);
            return 0;
        }

        void DispFreeSpace()
        {
            if (this.textBox_diskSpace_tempFileName.Text.Length < 2)
            {
                this.textBox_diskSpace_freeSpace.Text = "";
                return;
            }

            string strDriver = this.textBox_diskSpace_tempFileName.Text.Substring(0, 2);
            try
            {
                DriveInfo info = new DriveInfo(strDriver);

                this.textBox_diskSpace_freeSpace.Text = strDriver + "剩余空间: " + info.AvailableFreeSpace.ToString();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.tabControl_main.Enabled = bEnable;
        }

        // 创建事件日志目录
        private void button_createEventLogDir_Click(object sender, EventArgs e)
        {
            if (this.textBox_evenLogDirName.Text == "")
            {
                MessageBox.Show(this, "尚未指定日志目录名");
                return;
            }

            // Create the source, if it does not already exist.
            if (!EventLog.SourceExists(this.textBox_evenLogDirName.Text))
            {
                EventLog.CreateEventSource(this.textBox_evenLogDirName.Text, "DigitalPlatform");
            }


            EventLog Log = new EventLog();
            Log.Source = this.textBox_evenLogDirName.Text;
            Log.WriteEntry(this.textBox_evenLogDirName.Text + "目录创建成功。",
                EventLogEntryType.Information);

            MessageBox.Show(this, "OK");
        }

        private void button_testExceptionMessage_Click(object sender, EventArgs e)
        {
            try
            {
                TestForm form = null;

                form.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetDebugText(ex));
            }
        }

        private void button_goUrl_Click(object sender, EventArgs e)
        {
            this.extendedWebBrowser1.Navigate(this.textBox_url.Text);
        }

        private void extendedWebBrowser1_BeforeNavigate(object sender, BeforeNavigateArgs e)
        {
            // Debug.Assert(false, "");

            int i = 0;
            i++;
        }

        private void button_webClient_go_Click(object sender, EventArgs e)
        {
            this.textBox_webClient_headers.Text = "";

            WebClient webClient = new WebClient();

            try
            {
                webClient.DownloadFile(this.textBox_webClient_url.Text,
                    this.MainForm.DataDir + "\\temp.temp");

                foreach (string key in webClient.ResponseHeaders.AllKeys)
                {
                    this.textBox_webClient_headers.Text += key + ":" + webClient.ResponseHeaders[key] + "\r\n";
                }

                MessageBox.Show(this, "OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void textBox_locationEditControl_count_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int nCount = Convert.ToInt32(this.textBox_locationEditControl_count.Text);
                this.locationEditControl1.Count = nCount;
            }
            catch
            {
                return;
            }
        }

        private void button_locationEditControl_setToControl_Click(object sender, EventArgs e)
        {
            this.locationEditControl1.Value = this.textBox_locationEditControl_locationString.Text;
        }

        private void button_locationEditControl_getFromControl_Click(object sender, EventArgs e)
        {
            this.textBox_locationEditControl_locationString.Text = this.locationEditControl1.Value;
        }

        // 将XML片段设置给控件
        private void button_captionEditControl_set_Click(object sender, EventArgs e)
        {
            try
            {
                this.captionEditControl1.Xml = this.textBox_captionEditControl_xml.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        // 从控件中获得XML片段
        private void button_captionEditControl_get_Click(object sender, EventArgs e)
        {
            try
            {
                this.textBox_captionEditControl_xml.Text = this.captionEditControl1.Xml;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void button_fromEditControl_set_Click(object sender, EventArgs e)
        {
            try
            {
                this.fromEditControl1.Xml = this.textBox_fromEditControl_xml.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void button_fromEditControl_get_Click(object sender, EventArgs e)
        {
            try
            {
                this.textBox_fromEditControl_xml.Text = this.fromEditControl1.Xml;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }

        }

        // FromEditControl的captions部分是否有单独的标题条?
        private void checkBox_fromEditControl_hasCaptionsTitleLine_CheckedChanged(object sender, EventArgs e)
        {
            this.fromEditControl1.HasCaptionsTitleLine = this.checkBox_fromEditControl_hasCaptionsTitleLine.Checked;
        }

        private void button_testDouble_Click(object sender, EventArgs e)
        {
            /*
            double x = 3198.85D;
            double y = 6.80D;
             * */

            double x = Convert.ToDouble("3198.85");
            double y = Convert.ToDouble("6.80");


            MessageBox.Show(this, "x=" + x.ToString() + " y=" + y.ToString() + "=" + (x + y).ToString());
        }

        private void button_checkedComboBox_setList_Click(object sender, EventArgs e)
        {
            this.checkedComboBox1.Items.Clear();

            for (int i = 0; i < this.textBox_checkedComboBox_listValues.Lines.Length; i++)
            {
                this.checkedComboBox1.Items.Add(this.textBox_checkedComboBox_listValues.Lines[i]);
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {

        }

        private void button_times_getUTime_Click(object sender, EventArgs e)
        {
            this.textBox_times_utime.Text = DateTime.Now.ToString("u");
        }



        private void button_times_getRfc1123Time_Click(object sender, EventArgs e)
        {

            this.textBox_times_rfc1123Time.Text = DateTimeUtil.Rfc1123DateTimeStringEx(
                DateTime.Now);
        }

        private void button_times_parseRfc1123Time_Click(object sender, EventArgs e)
        {
            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(this.textBox_times_rfc1123Time.Text);
            MessageBox.Show(this, "GMT time: " + time.ToString() + "  LocalTime: " + time.ToLocalTime().ToString());
        }



        private void button_string_buidRangeString_Click(object sender, EventArgs e)
        {
            string[] parts = this.textBox_string_numberList.Text.Split(new char[] { ',' });
            List<String> numbers = new List<string>();
            numbers.AddRange(parts);
            MessageBox.Show(this, Global.BuildNumberRangeString(numbers));
        }

        private void button_font_createFont_Click(object sender, EventArgs e)
        {
            System.Drawing.FontFamily family = new System.Drawing.FontFamily(this.textBox_fontName.Text);
            MessageBox.Show(this, family.ToString());
        }

        private void button_gcatClient_getNumber_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strDebugInfo = "";
            string strNumber = "";

            this.textBox_gcatClient_debugInfo.Text = "";

            // return:
            //      -1  error
            //      0   canceled
            //      1   succeed
            int nRet = GcatNew.GetNumber(
                null,
                this,
                this.textBox_gcatClient_url.Text,   // "http://localhost/gcatserver/",
                "",
                this.textBox_gcatClient_author.Text,
                true,
                true,
                true,
                out strNumber,
                out strDebugInfo,
                out strError);
            if (nRet != 1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, strNumber);

            this.textBox_gcatClient_debugInfo.Text = strDebugInfo;
        }

        private void button_font_htmlInputDialog_Click(object sender, EventArgs e)
        {
            HtmlInputDialog dlg = new HtmlInputDialog();
            dlg.Text = "指定统计特性";
            dlg.Url = "f:\\temp\\input.html";
            dlg.Size = new Size(700, 500);
            dlg.ShowDialog(this);
        }

        // marcxchange --> marcxml
        private void button_marcFormat_convertXtoK_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strTarget = "";
            this.textBox_marcFormat_targetXml.Text = "";

            int nRet = MarcUtil.MarcXChangeToXml(this.textBox_marcFormat_sourceXml.Text,
            out strTarget,
            out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                string strXml = "";
                nRet = DomUtil.GetIndentXml(strTarget,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                else
                    this.textBox_marcFormat_targetXml.Text = strXml;
            }
        }

        // marcxml --> marcxchange
        private void button_marcFormat_convertKtoX_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strTarget = "";
            this.textBox_marcFormat_targetXml.Text = "";

            // 将机内使用的marcxml格式转化为marcxchange格式
            int nRet = MarcUtil.MarcXmlToXChange(this.textBox_marcFormat_sourceXml.Text,
                null,
            out strTarget,
            out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
            {
                string strXml = "";
                nRet = DomUtil.GetIndentXml(strTarget,
                    out strXml,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                else
                    this.textBox_marcFormat_targetXml.Text = strXml;
            }

        }

        private void button_dpTable_fill_Click(object sender, EventArgs e)
        {
            if (this.dpTable1.Columns.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    DpColumn cell = new DpColumn();
                    cell.Text = "column " + i.ToString();
                    cell.Width = 100;
                    this.dpTable1.Columns.Add(cell);
                }
            }

            string strImageFileName = Path.Combine(this.MainForm.DataDir, "ajax-loader.gif");
            Image image = Image.FromFile(strImageFileName);

            for (int i = 0; i < 10; i++)
            {
                DpRow line = new DpRow();
                line.ForeColor = System.Drawing.Color.Yellow;
                for (int j = 0; j < 10; j++)
                {
                    DpCell cell = new DpCell();
                    if (j == 0)
                        cell.Image = image;
                    cell.Text = "asdf asd fa sdfa sdf asd fa sdf" + i.ToString() + " " + j.ToString();
                    if (j == 5)
                    {
                        cell.BackColor = System.Drawing.Color.Green;
                        cell.ForeColor = System.Drawing.Color.White;
                        cell.Font = new System.Drawing.Font(this.dpTable1.Font, FontStyle.Bold);

                    }
                    if (i == 2)
                        cell.Alignment = DpTextAlignment.InheritLine;
                    line.Add(cell);
                }

                if (i == 2)
                {
                    line.BackColor = System.Drawing.Color.Red;
                    line.Font = new System.Drawing.Font(this.dpTable1.Font, FontStyle.Italic);
                    line.Alignment = StringAlignment.Center;
                }

                this.dpTable1.Rows.Add(line);
            }

            /*
            {
                DpRow line = new DpRow();
                line.Style = DpRowStyle.Seperator;
                line.BackColor = Color.Blue;
                line.ForeColor = Color.White;
                this.dpTable1.Rows.Add(line);

            }
             * */
        }

        private void button_dpTable_change_Click(object sender, EventArgs e)
        {
            this.dpTable1.Rows[1].Font = new System.Drawing.Font("Arial", 18, GraphicsUnit.Point);
        }

        private void button_gcatClient_getPinyin_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strPinyinXml = "";

            this.textBox_gcatClient_debugInfo.Text = "";

            // return:
            //      -1  error
            //      0   canceled
            //      1   succeed
            int nRet = GcatNew.GetPinyin(
                null,
                this.textBox_gcatClient_url.Text,   // "http://localhost/gcatserver/",
                "",
                this.textBox_gcatClient_hanzi.Text,
                out strPinyinXml,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                MessageBox.Show(this, strPinyinXml);
        }

        private void button_xml_getXmlFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的XML文件名";
            dlg.FileName = this.textBox_xml_xmlFilename.Text;
            // dlg.InitialDirectory = 
            dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_xml_xmlFilename.Text = dlg.FileName;
        }

        private void button_xml_loadToDom_Click(object sender, EventArgs e)
        {
            XmlDocument dom = new XmlDocument();
            dom.Load(this.textBox_xml_xmlFilename.Text);

            this.textBox_xml_content.Text = dom.OuterXml;
        }

        WebCamera wc = null;

        private void button_start_Click(object sender, EventArgs e)
        {
            if (wc == null)
            {
                wc = new WebCamera(this.panel_camera_preview.Handle,
                    panel_camera_preview.Width, panel_camera_preview.Height);
                wc.StartWebCam();
            }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            if (wc != null)
            {
                wc.CloseWebcam();
                wc = null;
            }
        }

        private void button_camera_capture_Click(object sender, EventArgs e)
        {
            if (wc != null)
            {
                this.label_image.Image = wc.Capture();
            }
        }

        private void button_idcardReader_read_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            /*
            IServerFactory obj = (IServerFactory)Activator.GetObject(typeof(IServerFactory),
    "ipc://IdcardChannel/ServerFactory");

            m_idcardObj = obj.GetInterface();
 */
            this.pictureBox_idCard.Image = null;


            nRet = StartChannel(out strError);
            if (nRet == -1)
                goto ERROR1;

            try
            {
                string strXml = "";
                // Image image = null;
                byte[] baPhoto = null;
                nRet = m_idcardObj.ReadCard("",
                    out strXml,
                    out baPhoto,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (baPhoto != null)
                {
                    using (MemoryStream s = new MemoryStream(baPhoto))
                    {
                        this.pictureBox_idCard.Image = new Bitmap(s);
                    }
                }

                MessageBox.Show(this, strXml);
            }
            finally
            {
                EndChannel();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        IpcClientChannel m_idcardChannel = new IpcClientChannel();
        IIdcard m_idcardObj = null;

        int StartChannel(out string strError)
        {
            strError = "";

            //Register the channel with ChannelServices.
            ChannelServices.RegisterChannel(m_idcardChannel, false);

            try
            {
                m_idcardObj = (IIdcard)Activator.GetObject(typeof(IIdcard),
                    this.textBox_idcardReader_serverUrl.Text);
                if (m_idcardObj == null)
                {
                    strError = "could not locate Idcard Server";
                    return -1;
                }
            }
            finally
            {
            }

            return 0;
        }

        void EndChannel()
        {
            ChannelServices.UnregisterChannel(m_idcardChannel);
        }

        private void button_idcardReader_messageBeep_Click(object sender, EventArgs e)
        {
            Console.Beep();
        }

        private void button_cutter_convertTextToXml_Click(object sender, EventArgs e)
        {
            // string strError = "";

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的文本文件名";
            dlg.FileName = "";
            // dlg.InitialDirectory = 
            dlg.Filter = "文本文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string strSourceFilename = dlg.FileName;


            SaveFileDialog save_dlg = new SaveFileDialog();

            save_dlg.Title = "请指定要创建的XML文件名";
            save_dlg.CreatePrompt = false;
            save_dlg.OverwritePrompt = false;
            save_dlg.FileName = "";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            save_dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";

            save_dlg.RestoreDirectory = true;

            if (save_dlg.ShowDialog() != DialogResult.OK)
                return;

            string strTargetFilename = save_dlg.FileName;

            using (StreamReader sr = new StreamReader(strSourceFilename))
            using (XmlTextWriter writer = new XmlTextWriter(strTargetFilename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();

                writer.WriteStartElement("collection");

                for (; ; )
                {
                    string strLine = sr.ReadLine();
                    if (strLine == null)
                        break;
                    strLine = strLine.Trim();

                    if (string.IsNullOrEmpty(strLine) == true)
                        continue;

                    string strNumber = "";
                    string strText = "";
                    int nRet = strLine.IndexOf(" ");
                    if (nRet == -1)
                        continue;
                    strNumber = strLine.Substring(0, nRet).Trim();
                    strText = strLine.Substring(nRet + 1).Trim();

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<item />");
                    DomUtil.SetAttr(dom.DocumentElement, "n", strNumber);
                    DomUtil.SetAttr(dom.DocumentElement, "t", strText);

                    dom.DocumentElement.WriteTo(writer);
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            MessageBox.Show(this, "OK");
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }



        private void button_cutter_getEntry_Click(object sender, EventArgs e)
        {
            string strError = "";
            this.textBox_cutter_resultString.Text = "";

            int nRet = this.MainForm.LoadQuickCutter(true, out strError);
            if (nRet == -1)
                goto ERROR1;

            string strText = "";
            string strNumber = "";
            nRet = this.MainForm.QuickCutter.GetEntry(this.textBox_cutter_authorString.Text,
                out strText,
                out strNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_cutter_resultString.Text = strText + " " + strNumber;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_cutter_verify_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = this.MainForm.LoadQuickCutter(true, out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = this.MainForm.QuickCutter.Verify(out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "OK");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_cutter_exchange_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = this.MainForm.LoadQuickCutter(true, out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = this.MainForm.QuickCutter.Exchange(out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "OK");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // .InnerText和XPath text()哪个快 ?
        private void button_test_innerTextAndXPath_Click(object sender, EventArgs e)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root><test>test string</test></root>");

            XmlNode node = dom.DocumentElement.SelectSingleNode("test");

            DateTime start_time = DateTime.Now;

            for (int i = 0; i < 10000; i++)
            {
                string strText = node.InnerText; // GetInnerText(node);   // node.InnerText;
            }

            TimeSpan delta1 = DateTime.Now - start_time;


            start_time = DateTime.Now;

            for (int i = 0; i < 10000; i++)
            {
                string strText = GetNodeText(node);
            }

            TimeSpan delta2 = DateTime.Now - start_time;

            MessageBox.Show(this, ".InnertText 耗费时间 " + delta1.ToString() + "， GetNodeText() 耗费时间" + delta2.ToString());
        }

        public static string GetNodeText(XmlNode node)
        {
            Debug.Assert(node != null, "");

            XmlNode nodeText = node.SelectSingleNode("text()");
            if (nodeText == null)
                return "";
            else
                return nodeText.Value;
        }

        public static string GetInnerText(XmlNode node)
        {
            Debug.Assert(node != null, "");

            return node.InnerText;
        }

        private void button_testMessageDialog_Click(object sender, EventArgs e)
        {
            bool bHideMessageBox = false;
            /*
            DialogResult result = MessageDialog.Show(this,
    "是否要升级统计方案 ?",
    MessageBoxButtons.YesNoCancel,
    MessageBoxDefaultButton.Button2,
    "以后不再提示，按本次的选择处理",
    ref bHideMessageBox);
             * */
            DialogResult result = MessageDialog.Show(this,
"是否要升级统计方案 ?",
"以后不再提示，按本次的选择处理",
ref bHideMessageBox);

            MessageBox.Show(this, "result=" + result.ToString() + ", bHideMessageBox=" + bHideMessageBox.ToString());
        }

        private void button_times_parseFreeTime_Click_3(object sender, EventArgs e)
        {
            DateTime time = DateTimeUtil.ParseFreeTimeString(this.textBox_times_freeTime.Text);
            MessageBox.Show(this, "time: " + time.ToString());
        }

        private void button_string_CompareAccessNo_Click(object sender, EventArgs e)
        {
            this.textBox_string_accessNo1_ascii.Text = GetAsciiExplainString(this.textBox_string_accessNo1.Text);
            this.textBox_string_accessNo2_ascii.Text = GetAsciiExplainString(this.textBox_string_accessNo2.Text);

            int nRet = StringUtil.CompareAccessNo(this.textBox_string_accessNo1.Text,
                this.textBox_string_accessNo2.Text);
            MessageBox.Show(this, "nRet = " + nRet.ToString());
        }

        static string GetAsciiExplainString(string strText)
        {
            string strResult = "";

            foreach (char ch in strText)
            {
                strResult += new string(ch, 1) + " [" + ((UInt32)ch) + "] ";
            }

            return strResult;
        }

        private void button_excel_test_Click(object sender, EventArgs e)
        {
            string strFileName = "f:\\temp\\test.xlsx";
            CreateSpreadsheetWorkbook(strFileName);
        }

        public static void CreateSpreadsheetWorkbook(string filepath)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.

            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "mySheet" };
            sheets.Append(sheet);

            UpdateValue(workbookpart, "mySheet2", "A1", "text 1", 0, true);

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        #region text excel

        public static bool UpdateValue(
            WorkbookPart wbPart,
            string sheetName, string addressName, string value,
                                UInt32Value styleIndex, bool isString)
        {
            // Assume failure.
            bool updated = false;

            Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where(
                (s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(wbPart.GetPartById(sheet.Id))).Worksheet;
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.
                    int stringIndex = InsertSharedStringItem(wbPart, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                // Save the worksheet.
                ws.Save();
                updated = true;
            }

            return updated;
        }

        // Given the main workbook part, and a text value, insert the text into 
        // the shared string table. Create the table if necessary. If the value 
        // already exists, return its index. If it doesn't exist, insert it and 
        // return its new index.
        private static int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = wbPart
                .GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // If the shared string table is missing, something's wrong.
            // Just return the index that you found in the cell.
            // Otherwise, look up the correct text in the table.
            if (stringTablePart == null)
            {
                // Create it.
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
                stringTablePart.SharedStringTable = stringTable;
            }

            // Iterate through all the items in the SharedStringTable. 
            // If the text already exists, return its index.
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTable.Save();
            }

            return index;
        }

        // Given a Worksheet and an address (like "AZ254"), either return a 
        // cell reference, or create the cell reference and return it.
        private static DocumentFormat.OpenXml.Spreadsheet.Cell InsertCellInWorksheet(Worksheet ws,
            string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            DocumentFormat.OpenXml.Spreadsheet.Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        // Add a cell with the specified address to a row.
        private static DocumentFormat.OpenXml.Spreadsheet.Cell CreateCell(Row row, String address)
        {
            DocumentFormat.OpenXml.Spreadsheet.Cell cellResult;
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
            {
                if (string.Compare(cell.CellReference.Value, address, true) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        // Return the row at the specified rowIndex located within
        // the sheet data passed in via wsData. If the row does not
        // exist, create it.
        private static Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
            Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        // Given an Excel address such as E5 or AB128, GetRowIndex
        // parses the address and returns the row index.
        private static UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }
        #endregion

        private void button_encoding_detectEncoding_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的记录路径文件名";
            // dlg.FileName = this.RecPathFilePath;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            Encoding encoding = FileUtil.DetectTextFileEncoding(dlg.FileName);

            MessageBox.Show(this, encoding.ToString());
        }

        void DoStop(object sender, StopEventArgs e)
        {
#if NO
            if (this.Channel != null)
                this.Channel.Abort();
#endif
        }

        void Channel_BeforeLogin(object sender, DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            MainForm.Channel_BeforeLogin(sender, e);    // 2015/11/8
        }

        Stop _stop = null;
        private void button_test_channelAttack_Click(object sender, EventArgs e)
        {

            _stop = new DigitalPlatform.Stop();
            _stop.Register(this.MainForm.stopManager, true);	// 和容器关联

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.Initial("正在测试耗费通道 ...");
            _stop.BeginLoop();

            this.button_test_channelAttack.Enabled = false;
            this.numericUpDown_test_tryChannelCount.Enabled = false;
            try
            {
                for (int i = 0; i < this.numericUpDown_test_tryChannelCount.Value; i++)
                {
                    Application.DoEvents();

                    if (_stop != null && _stop.State != 0)
                        break;

                    LibraryChannel channel = new LibraryChannel();
                    channel.Url = this.MainForm.LibraryServerUrl;

                    channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
                    channel.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);

                    string strValue = "";
                    string strError = "";
                    long lRet = channel.GetSystemParameter(_stop,
                        "library",
                        "name",
                        out strValue,
                        out strError);
#if NO
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.OutofSession)
                            break;
                    }
#endif

                    _stop.SetMessage(i.ToString());
                }
            }
            finally
            {
                this.numericUpDown_test_tryChannelCount.Enabled = true;
                this.button_test_channelAttack.Enabled = true;

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                if (_stop != null) // 脱离关联
                {
                    _stop.Unregister();	// 和容器关联
                    _stop = null;
                }
            }
        }

        private void button_patronCardControl_setData_Click(object sender, EventArgs e)
        {
            this.patronCardControl1.Xml = @"<root>
<name>名字</name>
<barcode>123456789</barcode>
<department>adf adf ads fasdf asdf </department>
</root>";
        }

        private void button_javascript_run_Click(object sender, EventArgs e)
        {

        }

        private void button_ftp_findClientFilePath_Click(object sender, EventArgs e)
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

        private void button_ftp_upload_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                string strError = "";

                // 上传文件
                // 自动创建所需的目录
                // 不会抛出异常
                int nRet = FtpUploadDialog.SafeUploadFile(ref this._dirTable,
                        this.textBox_clientFilePath.Text,
                        "ftp://localhost",
                        this.textBox_serverFilePath.Text,
                        "Administrator",
                        "",
                        out strError);
                if (nRet == -1)
                    goto ERROR1;
                return;
            ERROR1:
                MessageBox.Show(this, strError);
            }
            finally
            {
                EnableControls(true);
            }
        }



        Hashtable _dirTable = new Hashtable();

        private void button_ftp_createDir_Click(object sender, EventArgs e)
        {
            FtpUploadDialog.FtpCreateDir(
                ref _dirTable,
                "ftp://localhost",
                Path.GetDirectoryName(this.textBox_serverFilePath.Text),
                "Administrator",
                "");
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            SelectItemDialog dlg = new SelectItemDialog();

            dlg.MainForm = this.MainForm;
            dlg.ShowDialog(this);
        }

        private void button_marcTemplate_addLine_Click(object sender, EventArgs e)
        {
#if NO
            EasyLine line = this.easyMarcControl1.InsertNewLine(0);
            line.Content = "test";
#endif
            this.easyMarcControl1.SetMarc(this.textBox_marcTemplate_marc.Text);
            //List<string> field_names = new List<string> {"001", "100"};
            //this.easyMarcControl1.HideFields(field_names, true);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_marcTemplate_marc);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_marcTemplate_marc);
                GuiState.SetUiState(controls, value);
            }
        }

        private void easyMarcControl1_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {

            string strFileName = Path.Combine(this.MainForm.DataDir, "marcdef");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                e.ErrorInfo = "配置文件 '" + strFileName + "' 装入XMLDUM时出错: " + ex.Message;
                return;
            }
            e.XmlDocument = dom;
        }

        private void button_marcTemplate_getMarc_Click(object sender, EventArgs e)
        {
            this.textBox_marcTemplate_marc.Text = this.easyMarcControl1.GetMarc();
        }

        private void button_entitiesControl_addLine_Click(object sender, EventArgs e)
        {
#if NO
            RegisterLine line = new RegisterLine(this.entitiesControl1);
            this.entitiesControl1.InsertNewLine(0, line, true);
#endif
        }

        private void button_entityRegisterControl_addLine_Click(object sender, EventArgs e)
        {
#if NO
            // this.entityRegisterControl1.SetMarc(this.textBox_marcTemplate_marc.Text);

            string strError = "";
            // 添加一个新的册对象
            int nRet = this.entityRegisterControl1.NewEntity(
                "0000001",
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
#endif
        }

        private void button_amazonSearch_openDialog_Click(object sender, EventArgs e)
        {
            AmazonSearchForm dlg = new AmazonSearchForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.TempFileDir = this.MainForm.UserTempDir;
            dlg.AutoSearch = true;

            dlg.UiState = this.MainForm.AppInfo.GetString(
"TestForm",
"AmazonSearchForm_uiState",
"");
            //dlg.QueryWord = "7-02-003343-1";
            //dlg.From = "isbn";
            // TODO: 保存窗口内的尺寸状态
            this.MainForm.AppInfo.LinkFormState(dlg, "TestForm_AmazonSearchForm_state");

            dlg.ShowDialog(this);

            this.MainForm.AppInfo.UnlinkFormState(dlg);

            this.MainForm.AppInfo.SetString(
"TestForm",
"AmazonSearchForm_uiState",
dlg.UiState);

        }

        // 检查 dp2libraryXE 是否已经安装
        private void MenuItem_group1_detectDp2libraryXEInstalled_Click(object sender, EventArgs e)
        {

        }

        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V2/dp2内务 V2"
        public static bool IsProductInstalled(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
            if (File.Exists(shortcutPath))
                return true;
            return false;
        }

        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V2/dp2内务 V2"
        public static string GetShortcutFilePath(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
        }

        private void button_test_detectInstallation_Click(object sender, EventArgs e)
        {
#if NO
            bool bRet = IsProductInstalled("DigitalPlatform/dp2 V2/dp2Library XE");
            MessageBox.Show(this, bRet.ToString());
#endif
            Process.Start(GetShortcutFilePath("DigitalPlatform/dp2 V2/dp2Library XE"));
        }

        private void button_testGetMergeStyleDialog_Click(object sender, EventArgs e)
        {
            GetMergeStyleDialog dlg = new GetMergeStyleDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ShowDialog(this);
        }

        private void button_test_loginAttack_Click(object sender, EventArgs e)
        {
            LibraryChannel channel = new LibraryChannel();
            channel.Url = this.MainForm.LibraryServerUrl;

            channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            channel.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);


            _stop = new DigitalPlatform.Stop();
            _stop.Register(this.MainForm.stopManager, true);	// 和容器关联

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Style = StopStyle.EnableHalfStop;
            _stop.Initial("正在试探密码 ...");
            _stop.BeginLoop();

            this.button_test_loginAttack.Enabled = false;
            this.numericUpDown_test_tryChannelCount.Enabled = false;
            try
            {
                for (int i = 0; i < this.numericUpDown_test_tryChannelCount.Value; i++)
                {
                    Application.DoEvents();

                    if (_stop != null && _stop.State != 0)
                        break;


                    string strUserName = "supervisor";
                    string strPassword = i.ToString();

                    string strRights = "";
                    string strLibraryCode = "";
                    string strOutputUserName = "";
                    string strError = "";
                    long lRet = channel.Login(
                        strUserName,
                        strPassword,
                        "",
                        out strOutputUserName,
                        out strRights,
                        out strLibraryCode,
                        out strError);
#if NO
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.OutofSession)
                            break;
                    }
#endif

                    _stop.SetMessage(i.ToString() + " username=" + strUserName + " password=" + strPassword + " lRet = " + lRet.ToString() + " " + strError);
                }
            }
            finally
            {
                this.numericUpDown_test_tryChannelCount.Enabled = true;
                this.button_test_loginAttack.Enabled = true;

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                if (_stop != null) // 脱离关联
                {
                    _stop.Unregister();	// 和容器关联
                    _stop = null;
                }
            }

        }

        private void button_testThrow_Click(object sender, EventArgs e)
        {
            throw new Exception("test throw exception");
        }

        private void button_openWindowsUpdateDialog_Click(object sender, EventArgs e)
        {
            WindowsUpdateDialog dlg = new WindowsUpdateDialog();
            dlg.ShowDialog(this);
        }

        private void button_testRelationDialog_Click(object sender, EventArgs e)
        {
            RelationDialog dlg = new RelationDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ProcSearchDictionary = SearchDictionary;
            dlg.Show(this);
        }

        int SearchDictionary(
            LibraryChannel channel,
            Stop stop,
            string strDbName,
            string strKey,
            string strMatchStyle,
            int nMaxCount,
            ref List<string> results,
            out string strError)
        {
            return this.MainForm.SearchDictionary(
                channel,
            stop,
            strDbName,
            strKey,
            strMatchStyle,
            nMaxCount,
            ref results,
            out strError);
        }

    }
}