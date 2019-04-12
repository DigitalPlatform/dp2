using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Core;

namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// Summary description for CfgFileEditDlg.
    /// </summary>
    public class CfgFileEditDlg : System.Windows.Forms.Form
    {
        public NoHasSelTextBox textBox_content;

        public DatabaseObject Obj = null;

        public ApplicationInfo applicationInfo = null;
        public DigitalPlatform.StopManager stopManager = null;
        RmsChannel channel = null;
        public ServerCollection Servers = null;	// 引用
        public RmsChannelCollection Channels = null;

        public string LocalPath = "";	// 本地物理路径

        byte[] TimeStamp = null;
        MemoryStream Stream = null;

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        public System.Windows.Forms.TextBox textBox_path;
        private System.Windows.Forms.Button button_format;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_export;
        private System.Windows.Forms.Button button_import;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_mime;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox textBox_serverUrl;
        private System.Windows.Forms.CheckBox checkBox_autoCreate;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public CfgFileEditDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            textBox_content.DisableEmSetSelMsg = false;

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.Channels != null)
                    this.Channels.Dispose();

            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CfgFileEditDlg));
            this.textBox_content = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.textBox_path = new System.Windows.Forms.TextBox();
            this.button_format = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.button_export = new System.Windows.Forms.Button();
            this.button_import = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_mime = new System.Windows.Forms.TextBox();
            this.textBox_serverUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox_autoCreate = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox_content
            // 
            this.textBox_content.AcceptsReturn = true;
            this.textBox_content.AcceptsTab = true;
            this.textBox_content.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_content.HideSelection = false;
            this.textBox_content.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_content.Location = new System.Drawing.Point(9, 82);
            this.textBox_content.MaxLength = 2000000000;
            this.textBox_content.Multiline = true;
            this.textBox_content.Name = "textBox_content";
            this.textBox_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_content.Size = new System.Drawing.Size(546, 237);
            this.textBox_content.TabIndex = 4;
            this.textBox_content.TextChanged += new System.EventHandler(this.textBox_content_TextChanged);
            this.textBox_content.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox_content_KeyUp);
            this.textBox_content.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBox_content_MouseDown);
            this.textBox_content.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox_content_MouseUp);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(480, 356);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 24);
            this.button_OK.TabIndex = 9;
            this.button_OK.Text = "保存";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(480, 384);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 10;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // textBox_path
            // 
            this.textBox_path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_path.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_path.Location = new System.Drawing.Point(83, 33);
            this.textBox_path.Name = "textBox_path";
            this.textBox_path.Size = new System.Drawing.Size(472, 21);
            this.textBox_path.TabIndex = 1;
            // 
            // button_format
            // 
            this.button_format.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_format.Location = new System.Drawing.Point(104, 356);
            this.button_format.Name = "button_format";
            this.button_format.Size = new System.Drawing.Size(120, 23);
            this.button_format.TabIndex = 8;
            this.button_format.Text = "整理XML格式(&F)";
            this.button_format.Click += new System.EventHandler(this.button_format_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(8, 328);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(544, 22);
            this.label_message.TabIndex = 5;
            // 
            // button_export
            // 
            this.button_export.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_export.Location = new System.Drawing.Point(9, 356);
            this.button_export.Name = "button_export";
            this.button_export.Size = new System.Drawing.Size(75, 23);
            this.button_export.TabIndex = 6;
            this.button_export.Text = "导出(&E)...";
            this.button_export.Click += new System.EventHandler(this.button_export_Click);
            // 
            // button_import
            // 
            this.button_import.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_import.Location = new System.Drawing.Point(9, 384);
            this.button_import.Name = "button_import";
            this.button_import.Size = new System.Drawing.Size(75, 22);
            this.button_import.TabIndex = 7;
            this.button_import.Text = "导入(&I)...";
            this.button_import.Click += new System.EventHandler(this.button_import_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "路径(&P):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "&MIME:";
            // 
            // textBox_mime
            // 
            this.textBox_mime.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_mime.Location = new System.Drawing.Point(83, 58);
            this.textBox_mime.Name = "textBox_mime";
            this.textBox_mime.Size = new System.Drawing.Size(192, 21);
            this.textBox_mime.TabIndex = 3;
            this.textBox_mime.TextChanged += new System.EventHandler(this.textBox_mime_TextChanged);
            // 
            // textBox_serverUrl
            // 
            this.textBox_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverUrl.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_serverUrl.Location = new System.Drawing.Point(83, 9);
            this.textBox_serverUrl.Name = "textBox_serverUrl";
            this.textBox_serverUrl.Size = new System.Drawing.Size(472, 21);
            this.textBox_serverUrl.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "服务器(&U):";
            // 
            // checkBox_autoCreate
            // 
            this.checkBox_autoCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_autoCreate.AutoSize = true;
            this.checkBox_autoCreate.Location = new System.Drawing.Point(104, 387);
            this.checkBox_autoCreate.Name = "checkBox_autoCreate";
            this.checkBox_autoCreate.Size = new System.Drawing.Size(138, 16);
            this.checkBox_autoCreate.TabIndex = 13;
            this.checkBox_autoCreate.Text = "自动创建中间对象(&A)";
            // 
            // CfgFileEditDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(564, 416);
            this.Controls.Add(this.checkBox_autoCreate);
            this.Controls.Add(this.textBox_serverUrl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_mime);
            this.Controls.Add(this.textBox_path);
            this.Controls.Add(this.textBox_content);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_import);
            this.Controls.Add(this.button_export);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_format);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CfgFileEditDlg";
            this.ShowInTaskbar = false;
            this.Text = "编辑配置文件";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.CfgFileEditDlg_Closing);
            this.Closed += new System.EventHandler(this.CfgFileEditDlg_Closed);
            this.Load += new System.EventHandler(this.CfgFileEditDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public string ServerUrl
        {
            get
            {
                return this.textBox_serverUrl.Text;
            }
            set
            {
                this.textBox_serverUrl.Text = value;
            }
        }


        public string Path
        {
            get
            {
                return textBox_path.Text;
            }
            set
            {
                textBox_path.Text = value;
            }
        }

        public void Initial(DatabaseObject objFile,
            string strPath)
        {
            this.Obj = objFile;
            this.Path = strPath;
            // this.textBox_path.Text = this.Path;

        }

        public void Initial(ServerCollection servers,
            RmsChannelCollection channels,
            DigitalPlatform.StopManager stopManager,
            string serverUrl,
            string strCfgFilePath)
        {
            this.Servers = servers;
            this.Channels = channels;
            this.stopManager = stopManager;
            this.ServerUrl = serverUrl;
            this.Path = strCfgFilePath;
            // this.textBox_path.Text = this.ServerUrl + "?" + this.Path;

        }

        public string Mime
        {
            get
            {
                return this.textBox_mime.Text;
            }
            set
            {
                this.textBox_mime.Text = value;
            }
        }

        static string ConvertCrLf(string strText)
        {
            strText = strText.Replace("\r\n", "\r");
            strText = strText.Replace("\n", "\r");
            return strText.Replace("\r", "\r\n");
        }

        private void CfgFileEditDlg_Load(object sender, System.EventArgs e)
        {
            button_export.Enabled = false;

            MemoryStream stream = null;
            string strMetaData;
            string strError = "";
            string strMime = "";

            Hashtable values = null;

            if (Obj != null)
            {
                if (this.Obj.Content != null)
                {
                    stream = new MemoryStream(this.Obj.Content);
                    this.Stream = stream;
                    this.Stream.Seek(0, SeekOrigin.Begin);

                    button_export.Enabled = true;
                }

                this.TimeStamp = this.Obj.TimeStamp;

                strMetaData = this.Obj.Metadata;

                // 观察mime
                // 取metadata
                values = StringUtil.ParseMetaDataXml(strMetaData,
                    out strError);
                if (values == null)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
                strMime = (string)values["mimetype"];
                if (strMime == null || strMime == "")
                    strMime = "text";
                this.Mime = strMime;

                this.LocalPath = (string)values["localpath"];
                if (this.LocalPath == null)
                    this.LocalPath = "";

                this.textBox_content.Text = "";

                // string strFirstPart = StringUtil.GetFirstPartPath(ref strMime);
                if (this.IsText == true)
                {
                    if (this.Stream != null)
                    {
                        this.Stream.Seek(0, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(this.Stream, Encoding.UTF8))
                        {
                            this.textBox_content.Text = ConvertCrLf(sr.ReadToEnd());
                        }
                    }
                }
                else
                {
                }

                //////

                button_OK.Enabled = false;

                this.textBox_content.SelectionStart = 0;
                this.textBox_content.SelectionLength = 0;
                return;
            }

            this.channel = Channels.GetChannel(this.ServerUrl);
            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载配置文件: " + this.Path);

                stop.BeginLoop();

            }

            // string strContent = "";
            byte[] baTimeStamp = null;
            string strOutputPath;

            string strStyle = "content,data,metadata,timestamp,outputpath";
            //			string strStyle = "attachment,data,metadata,timestamp,outputpath";

            stream = new MemoryStream();

            long lRet = channel.GetRes(
                this.Path,
                stream,
                stop,	// stop,
                strStyle,
                null,	// byte [] input_timestamp,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);

            /*
            long lRet = channel.GetRes((
                this.Path,
                out strContent,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            */

            if (stopManager != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

            }

            if (lRet == -1)
            {
                MessageBox.Show(this, strError);
                goto FINISH;
            }

            this.Stream = stream;
            this.Stream.Seek(0, SeekOrigin.Begin);

            button_export.Enabled = true;


            this.TimeStamp = baTimeStamp;

            // 观察mime
            // 取metadata
            values = StringUtil.ParseMetaDataXml(strMetaData,
                out strError);
            if (values == null)
            {
                MessageBox.Show(this, strError);
                goto FINISH;
            }
            strMime = (string)values["mimetype"];
            if (strMime == null || strMime == "")
                strMime = "text";
            this.Mime = strMime;

            this.LocalPath = (string)values["localpath"];
            if (this.LocalPath == null)
                this.LocalPath = "";

            // string strFirstPart = StringUtil.GetFirstPartPath(ref strMime);
            if (this.IsText == true)
            {
                this.Stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(this.Stream, Encoding.UTF8))
                {
                    this.textBox_content.Text = ConvertCrLf(sr.ReadToEnd());
                }
                // 注意，此后 this.Stream 被关闭
            }
            else
            {
                //this.textBox_content.Text = "<二进制内容无法直接编辑>";
                //this.textBox_content.ReadOnly = true;
                //this.button_format.Enabled = false;
            }

            //////

            button_OK.Enabled = false;
        FINISH:
            if (stopManager != null && stop != null)
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }

            this.channel = null;

            this.textBox_content.SelectionStart = 0;
            this.textBox_content.SelectionLength = 0;
        }

        // mime是否为text开头
        bool IsText
        {
            get
            {
                string strMime = this.Mime;
                string strFirstPart = StringUtil.GetFirstPartPath(ref strMime);
                if (strFirstPart.ToLower() == "text")
                    return true;

                return false;
            }
        }

        // 去掉字符串中的单个0x0a(而不是0x0d 0x0a)
        public static string RemoveSingle0a(string strText)
        {
            string strResult = strText.Replace("\n", "");
            return strResult.Replace("\r", "\r\n");
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            string strMetaData = "";

            // TODO: 一旦遇到问题，可以放开注释试验
            // this.textBox_content.Text = RemoveSingle0a(this.textBox_content.Text);


            if (this.Obj != null)
            {
                if (this.IsText == true)
                {
                    byte[] baContent = StringUtil.GetUtf8Bytes(this.textBox_content.Text, true);
                    this.Obj.Content = baContent;
                }
                else
                {
                    if (this.Stream != null)
                    {
                        this.Obj.Content = new byte[this.Stream.Length];
                        this.Stream.Seek(0, SeekOrigin.Begin);
                        this.Stream.Read(this.Obj.Content,
                            0, (int)this.Stream.Length);
                    }
                    else
                    {
                        this.Obj.Content = null;
                    }
                }

                StringUtil.ChangeMetaData(ref strMetaData,
                    null,	// string strID,
                    this.LocalPath,	// string strLocalPath,
                    this.Mime,	// string strMimeType,
                    null,	// string strLastModified,
                    null,	// string strPath,
                    null);	// string strTimestamp)
                this.Obj.Metadata = strMetaData;

                this.Obj.Changed = true;

                this.DialogResult = DialogResult.OK;
                this.Close();

                return;
            }

            this.channel = Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在保存配置文件: " + this.Path);

                stop.BeginLoop();

            }

            if (this.IsText == true)
            {
                // 更新stream对象内容
                byte[] baContent = StringUtil.GetUtf8Bytes(this.textBox_content.Text, true);
                this.Stream = new MemoryStream(baContent);  // !!! 什么时候释放?
                /*
                this.Stream.SetLength(0);
                StreamWriter sw = new StreamWriter(this.Stream, Encoding.UTF8);
                sw.Write(this.textBox_content.Text);
                */
            }

            if (this.Stream != null)
                this.Stream.Seek(0, SeekOrigin.Begin);

            // 保存配置文件
            string strError = "";
            byte[] baOutputTimestamp = null;
            string strOutputPath = "";
            string strStyle = "";

            if (this.checkBox_autoCreate.Checked == true)
            {
                if (strStyle != "")
                    strStyle += ",";
                strStyle += "autocreatedir";
            }


            StringUtil.ChangeMetaData(ref strMetaData,
                null,	// string strID,
                this.LocalPath,	// string strLocalPath,
                this.Mime,	// string strMimeType,
                null,	// string strLastModified,
                null,	// string strPath,
                null);	// string strTimestamp)

            string strRange = "";
            if (this.Stream != null && this.Stream.Length != 0)
            {
                Debug.Assert(this.Stream.Length != 0, "test");
                strRange = "0-" + Convert.ToString(this.Stream.Length - 1);
            }
            long lRet = channel.DoSaveResObject(this.Path,
                this.Stream,
                (this.Stream != null && this.Stream.Length != 0) ? this.Stream.Length : 0,
                strStyle,
                strMetaData,	// strMetadata,
                strRange,
                true,
                this.TimeStamp,	// timestamp,
                out baOutputTimestamp,
                out strOutputPath,
                out strError);

            /*
            // 保存配置文件
            byte[] baOutputTimeStamp = null;
            string strOutputPath = "";
            string strError = "";


            long lRet = channel.DoSaveTextRes(this.Path,
                this.textBox_content.Text,
                true,	// bInlucdePreamble
                "",	// style
                this.TimeStamp,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            */

            if (stopManager != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            if (lRet == -1)
            {
                MessageBox.Show(this, strError);
                goto FINISH;
            }

            this.TimeStamp = baOutputTimestamp;

            MessageBox.Show(this, "配置文件 '" + this.Path + "' 保存成功");


            /////////////
        FINISH:

            if (stopManager != null && stop != null)
            {

                stop.Unregister();	// 和容器关联
                stop = null;
            }

            this.channel = null;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_format_Click(object sender, System.EventArgs e)
        {
            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(textBox_content.Text);
            }
            catch (XmlException ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));

                textBox_content.Focus();
                //textBox_content.DisableEmSetSelMsg = false;
                API.SetEditCurrentCaretPos(
                    textBox_content,
                    ex.LinePosition - 1,
                    ex.LineNumber - 1,
                    true);
                //textBox_content.DisableEmSetSelMsg = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            using (MemoryStream m = new MemoryStream())
            using (XmlTextWriter w = new XmlTextWriter(m, Encoding.UTF8))
            {
                w.Formatting = Formatting.Indented;
                w.Indentation = 4;
                dom.Save(w);
                w.Flush();

                m.Seek(0, SeekOrigin.Begin);

                using (StreamReader sr = new StreamReader(m, Encoding.UTF8))
                {
                    textBox_content.Text = ConvertCrLf(sr.ReadToEnd());
                }

                // w.Close();
                //			m.Close();
            }

            MessageBox.Show(this, "整理完毕。");
        }

        private void textBox_content_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            DisplayCurLineNo();
        }

        private void textBox_content_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DisplayCurLineNo();
        }

        void DisplayCurLineNo()
        {
            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                textBox_content,
                out x,
                out y);
            label_message.Text = "Ln " + Convert.ToString(y + 1) + "   Ch " + (x >= 0 ? Convert.ToString(x + 1) : "?");

        }

        private void CfgFileEditDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK
                && API.GetEditModify(textBox_content) == true)
            {

                DialogResult msgResult = MessageBox.Show(this,
                    "配置文件内容已经被修改, 尚未保存。\r\n是否要关闭窗口并放弃修改内容?",
                    "CfgFileEditDlg",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (msgResult == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

        }

        private void CfgFileEditDlg_Closed(object sender, System.EventArgs e)
        {
            // 2015/11/23
            if (this.Stream != null)
                this.Stream.Close();
        }

        private void textBox_content_TextChanged(object sender, System.EventArgs e)
        {
            button_OK.Enabled = true;
        }


        // 让正文编辑器获得tab键, 当作内容输入
        protected override bool ProcessTabKey(
            bool forward)
        {
            if (this.textBox_content.Focused == true)
            {
                return false;
            }

            return base.ProcessTabKey(forward);
        }

        private void button_export_Click(object sender, System.EventArgs e)
        {
            if (this.Stream == null)
            {
                MessageBox.Show(this, "尚未装载任何内容");
                return;
            }

            // 询问文件全路径
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.CreatePrompt = false;
            dlg.FileName = this.LocalPath;
            // dlg.FileName = "outer_projects.xml";
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (Stream s = File.Create(dlg.FileName))
            {
                this.Stream.Seek(0, SeekOrigin.Begin);
                StreamUtil.DumpStream(this.Stream, s);
            }
        }

        private void button_import_Click(object sender, System.EventArgs e)
        {
            OpenCfgFileDlg dlg = new OpenCfgFileDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.Mime = dlg.textBox_mime.Text;
            this.LocalPath = dlg.textBox_localPath.Text;
            using (Stream s = File.OpenRead(this.LocalPath))
            {
                if (s.Length > 1024 * 1024)
                {
                    MessageBox.Show(this, "配置文件尺寸不能大于1M");
                    return;
                }

                byte[] buffer = new byte[s.Length];
                s.Read(buffer, 0, (int)s.Length);

                this.Stream = new MemoryStream(buffer);
            }

            //
            if (this.IsText == true)
            {
                if (this.Stream != null)
                {
                    this.Stream.Seek(0, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(this.Stream, Encoding.UTF8))
                    {
                        this.textBox_content.Text = sr.ReadToEnd();
                    }
                }
            }

            button_OK.Enabled = true;
        }

        // 回调函数
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        private void textBox_mime_TextChanged(object sender, System.EventArgs e)
        {
            if (this.IsText == true)
            {
                this.textBox_content.ReadOnly = false;
                this.button_format.Enabled = true;
            }
            else
            {
                this.textBox_content.ReadOnly = true;
                this.button_format.Enabled = false;
                this.textBox_content.Text = "<二进制内容无法直接编辑>";
            }
        }

        private void textBox_content_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }


    }
}
