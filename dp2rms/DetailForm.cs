using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Text;

using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Marc;
using DigitalPlatform.Library;
using DigitalPlatform.Drawing;
using DigitalPlatform.CommonControl;


namespace dp2rms
{
    /// <summary>
    /// Summary description for DetailForm.
    /// </summary>
    public class DetailForm : System.Windows.Forms.Form
    {
        public Hashtable ParamTable = new Hashtable();

        public string MarcSyntax = "";

        public string Lang = "zh";

        RmsChannel channel = null;	// 临时使用的channel对象

        public AutoResetEvent eventClose = new AutoResetEvent(false);
        public RmsChannelCollection Channels = new RmsChannelCollection();	// 拥有
        DigitalPlatform.Stop stop = null;

        byte[] TimeStamp = null;	// 当前记录的时间戳
        string m_strMetaData = "";	// 当前记录的元数据

        string strDatabaseOriginPath = ""; // 刚从数据库调入时候的全路径

        // ViewAccessPointForm accessPointWindow = null;

        private System.Windows.Forms.TextBox textBox_recPath;
        private System.Windows.Forms.Button button_findRecPath;
        private System.Windows.Forms.TabControl tabControl_bottom;
        private System.Windows.Forms.TabPage tabPage_resFiles;
        private ResFileList listView_resFiles;

        public DigitalPlatform.Xml.XmlEditor XmlEditor;
        private System.Windows.Forms.TabControl tabControl_record;
        private System.Windows.Forms.TabPage tabPage_xml;
        private System.Windows.Forms.TabPage tabPage_marc;
        public MarcEditor MarcEditor;

        int m_nChangeTextNest = 0;

        ArrayList m_queueTextChanged = new ArrayList();

        MacroUtil m_macroutil = new MacroUtil();   // 宏处理器

        SeedManager m_seedmanager = new SeedManager();

        // int m_nTimeStampXml = 0;
        // int m_nTimeStampMarc = 0;
        private System.Windows.Forms.Timer timer_crossRefresh;
        private System.Windows.Forms.TabPage tabPage_xmlText;
        private System.Windows.Forms.TextBox textBox_xmlPureText;
        private TableLayoutPanel tableLayoutPanel_recpath;
        private SplitContainer splitContainer_main;
        private System.ComponentModel.IContainer components;

        public DetailForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

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
                this.eventClose.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailForm));
            this.textBox_recPath = new System.Windows.Forms.TextBox();
            this.button_findRecPath = new System.Windows.Forms.Button();
            this.timer_crossRefresh = new System.Windows.Forms.Timer(this.components);
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_record = new System.Windows.Forms.TabControl();
            this.tabPage_xml = new System.Windows.Forms.TabPage();
            this.XmlEditor = new DigitalPlatform.Xml.XmlEditor();
            this.tabPage_marc = new System.Windows.Forms.TabPage();
            this.MarcEditor = new DigitalPlatform.Marc.MarcEditor();
            this.tabPage_xmlText = new System.Windows.Forms.TabPage();
            this.textBox_xmlPureText = new System.Windows.Forms.TextBox();
            this.tabControl_bottom = new System.Windows.Forms.TabControl();
            this.tabPage_resFiles = new System.Windows.Forms.TabPage();
            this.listView_resFiles = new ResFileList();
            this.tableLayoutPanel_recpath = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_record.SuspendLayout();
            this.tabPage_xml.SuspendLayout();
            this.tabPage_marc.SuspendLayout();
            this.tabPage_xmlText.SuspendLayout();
            this.tabControl_bottom.SuspendLayout();
            this.tabPage_resFiles.SuspendLayout();
            this.tableLayoutPanel_recpath.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_recPath
            // 
            this.textBox_recPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_recPath.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_recPath.Location = new System.Drawing.Point(0, 3);
            this.textBox_recPath.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.textBox_recPath.Name = "textBox_recPath";
            this.textBox_recPath.Size = new System.Drawing.Size(524, 21);
            this.textBox_recPath.TabIndex = 0;
            this.textBox_recPath.TextChanged += new System.EventHandler(this.textBox_recPath_TextChanged);
            // 
            // button_findRecPath
            // 
            this.button_findRecPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findRecPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button_findRecPath.Location = new System.Drawing.Point(530, 3);
            this.button_findRecPath.Margin = new System.Windows.Forms.Padding(3, 3, 1, 3);
            this.button_findRecPath.Name = "button_findRecPath";
            this.button_findRecPath.Size = new System.Drawing.Size(33, 20);
            this.button_findRecPath.TabIndex = 1;
            this.button_findRecPath.Text = "...";
            this.button_findRecPath.Click += new System.EventHandler(this.button_findRecPath_Click);
            // 
            // timer_crossRefresh
            // 
            this.timer_crossRefresh.Interval = 500;
            this.timer_crossRefresh.Tick += new System.EventHandler(this.timer_crossRefresh_Tick);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 33);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_record);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tabControl_bottom);
            this.splitContainer_main.Size = new System.Drawing.Size(564, 285);
            this.splitContainer_main.SplitterDistance = 209;
            this.splitContainer_main.TabIndex = 4;
            // 
            // tabControl_record
            // 
            this.tabControl_record.Controls.Add(this.tabPage_xml);
            this.tabControl_record.Controls.Add(this.tabPage_marc);
            this.tabControl_record.Controls.Add(this.tabPage_xmlText);
            this.tabControl_record.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_record.Location = new System.Drawing.Point(0, 0);
            this.tabControl_record.Name = "tabControl_record";
            this.tabControl_record.SelectedIndex = 0;
            this.tabControl_record.Size = new System.Drawing.Size(564, 209);
            this.tabControl_record.TabIndex = 1;
            this.tabControl_record.SelectedIndexChanged += new System.EventHandler(this.tabControl_record_SelectedIndexChanged);
            // 
            // tabPage_xml
            // 
            this.tabPage_xml.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_xml.Controls.Add(this.XmlEditor);
            this.tabPage_xml.Location = new System.Drawing.Point(4, 22);
            this.tabPage_xml.Name = "tabPage_xml";
            this.tabPage_xml.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_xml.Size = new System.Drawing.Size(556, 183);
            this.tabPage_xml.TabIndex = 0;
            this.tabPage_xml.Text = "XML";
            this.tabPage_xml.UseVisualStyleBackColor = true;
            // 
            // XmlEditor
            // 
            this.XmlEditor.ActiveItem = null;
            this.XmlEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.XmlEditor.Changed = true;
            this.XmlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XmlEditor.DocumentOrgX = 0;
            this.XmlEditor.DocumentOrgY = 0;
            this.XmlEditor.LayoutStyle = DigitalPlatform.Xml.LayoutStyle.Horizontal;
            this.XmlEditor.Location = new System.Drawing.Point(4, 4);
            this.XmlEditor.Name = "XmlEditor";
            this.XmlEditor.Size = new System.Drawing.Size(548, 175);
            this.XmlEditor.TabIndex = 0;
            this.XmlEditor.Text = "XmlEditor";
            this.XmlEditor.Xml = "";
            this.XmlEditor.TextChanged += new System.EventHandler(this.xmlEditor_TextChanged);
            // 
            // tabPage_marc
            // 
            this.tabPage_marc.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_marc.Controls.Add(this.MarcEditor);
            this.tabPage_marc.Location = new System.Drawing.Point(4, 22);
            this.tabPage_marc.Name = "tabPage_marc";
            this.tabPage_marc.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_marc.Size = new System.Drawing.Size(556, 183);
            this.tabPage_marc.TabIndex = 1;
            this.tabPage_marc.Text = "MARC";
            this.tabPage_marc.UseVisualStyleBackColor = true;
            // 
            // MarcEditor
            // 
            this.MarcEditor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MarcEditor.CaptionFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MarcEditor.Changed = true;
            this.MarcEditor.ContentBackColor = System.Drawing.SystemColors.Window;
            this.MarcEditor.ContentTextColor = System.Drawing.SystemColors.WindowText;
            this.MarcEditor.CurrentImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MarcEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MarcEditor.DocumentOrgX = 0;
            this.MarcEditor.DocumentOrgY = 0;
            this.MarcEditor.FieldNameCaptionWidth = 100;
            this.MarcEditor.FixedSizeFont = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Bold);
            this.MarcEditor.FocusedField = null;
            this.MarcEditor.FocusedFieldIndex = -1;
            this.MarcEditor.HorzGridColor = System.Drawing.Color.LightGray;
            this.MarcEditor.IndicatorBackColor = System.Drawing.SystemColors.Window;
            this.MarcEditor.IndicatorBackColorDisabled = System.Drawing.SystemColors.Control;
            this.MarcEditor.IndicatorTextColor = System.Drawing.Color.Green;
            this.MarcEditor.Location = new System.Drawing.Point(4, 4);
            this.MarcEditor.Marc = "????????????????????????";
            this.MarcEditor.MarcDefDom = null;
            this.MarcEditor.Name = "MarcEditor";
            this.MarcEditor.NameBackColor = System.Drawing.SystemColors.Window;
            this.MarcEditor.NameCaptionBackColor = System.Drawing.SystemColors.Info;
            this.MarcEditor.NameCaptionTextColor = System.Drawing.SystemColors.InfoText;
            this.MarcEditor.NameTextColor = System.Drawing.Color.Blue;
            this.MarcEditor.ReadOnly = false;
            this.MarcEditor.SelectionStart = -1;
            this.MarcEditor.Size = new System.Drawing.Size(548, 175);
            this.MarcEditor.TabIndex = 0;
            this.MarcEditor.VertGridColor = System.Drawing.Color.LightGray;
            this.MarcEditor.GetConfigFile += new DigitalPlatform.Marc.GetConfigFileEventHandle(this.MarcEditor_GetConfigFile);
            this.MarcEditor.GetConfigDom += new DigitalPlatform.Marc.GetConfigDomEventHandle(this.MarcEditor_GetConfigDom);
            this.MarcEditor.TextChanged += new System.EventHandler(this.MarcEditor_TextChanged);
            // 
            // tabPage_xmlText
            // 
            this.tabPage_xmlText.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_xmlText.Controls.Add(this.textBox_xmlPureText);
            this.tabPage_xmlText.Location = new System.Drawing.Point(4, 22);
            this.tabPage_xmlText.Name = "tabPage_xmlText";
            this.tabPage_xmlText.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_xmlText.Size = new System.Drawing.Size(556, 183);
            this.tabPage_xmlText.TabIndex = 2;
            this.tabPage_xmlText.Text = "XML纯文本";
            this.tabPage_xmlText.UseVisualStyleBackColor = true;
            // 
            // textBox_xmlPureText
            // 
            this.textBox_xmlPureText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_xmlPureText.HideSelection = false;
            this.textBox_xmlPureText.Location = new System.Drawing.Point(4, 4);
            this.textBox_xmlPureText.MaxLength = 2000000000;
            this.textBox_xmlPureText.Multiline = true;
            this.textBox_xmlPureText.Name = "textBox_xmlPureText";
            this.textBox_xmlPureText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_xmlPureText.Size = new System.Drawing.Size(548, 175);
            this.textBox_xmlPureText.TabIndex = 0;
            this.textBox_xmlPureText.TextChanged += new System.EventHandler(this.textBox_xmlPureText_TextChanged);
            // 
            // tabControl_bottom
            // 
            this.tabControl_bottom.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl_bottom.Controls.Add(this.tabPage_resFiles);
            this.tabControl_bottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_bottom.Location = new System.Drawing.Point(0, 0);
            this.tabControl_bottom.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.tabControl_bottom.Multiline = true;
            this.tabControl_bottom.Name = "tabControl_bottom";
            this.tabControl_bottom.SelectedIndex = 0;
            this.tabControl_bottom.Size = new System.Drawing.Size(564, 72);
            this.tabControl_bottom.TabIndex = 0;
            // 
            // tabPage_resFiles
            // 
            this.tabPage_resFiles.Controls.Add(this.listView_resFiles);
            this.tabPage_resFiles.Location = new System.Drawing.Point(4, 4);
            this.tabPage_resFiles.Name = "tabPage_resFiles";
            this.tabPage_resFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_resFiles.Size = new System.Drawing.Size(556, 46);
            this.tabPage_resFiles.TabIndex = 0;
            this.tabPage_resFiles.Text = "资源文件";
            this.tabPage_resFiles.UseVisualStyleBackColor = true;
            // 
            // listView_resFiles
            // 
            this.listView_resFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView_resFiles.Changed = false;
            this.listView_resFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_resFiles.FullRowSelect = true;
            this.listView_resFiles.HideSelection = false;
            this.listView_resFiles.Location = new System.Drawing.Point(3, 3);
            this.listView_resFiles.Name = "listView_resFiles";
            this.listView_resFiles.Size = new System.Drawing.Size(550, 40);
            this.listView_resFiles.TabIndex = 0;
            this.listView_resFiles.UseCompatibleStateImageBehavior = false;
            this.listView_resFiles.View = System.Windows.Forms.View.Details;
            // 
            // tableLayoutPanel_recpath
            // 
            this.tableLayoutPanel_recpath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel_recpath.AutoSize = true;
            this.tableLayoutPanel_recpath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel_recpath.ColumnCount = 2;
            this.tableLayoutPanel_recpath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_recpath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_recpath.Controls.Add(this.button_findRecPath, 1, 0);
            this.tableLayoutPanel_recpath.Controls.Add(this.textBox_recPath, 0, 0);
            this.tableLayoutPanel_recpath.Location = new System.Drawing.Point(0, 4);
            this.tableLayoutPanel_recpath.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_recpath.Name = "tableLayoutPanel_recpath";
            this.tableLayoutPanel_recpath.RowCount = 1;
            this.tableLayoutPanel_recpath.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_recpath.Size = new System.Drawing.Size(564, 27);
            this.tableLayoutPanel_recpath.TabIndex = 4;
            // 
            // DetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 326);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.tableLayoutPanel_recpath);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DetailForm";
            this.Text = "详细窗";
            this.Activated += new System.EventHandler(this.DetailForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DetailForm_Closing);
            this.Closed += new System.EventHandler(this.DetailForm_Closed);
            this.Load += new System.EventHandler(this.DetailForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_record.ResumeLayout(false);
            this.tabPage_xml.ResumeLayout(false);
            this.tabPage_marc.ResumeLayout(false);
            this.tabPage_xmlText.ResumeLayout(false);
            this.tabPage_xmlText.PerformLayout();
            this.tabControl_bottom.ResumeLayout(false);
            this.tabPage_resFiles.ResumeLayout(false);
            this.tableLayoutPanel_recpath.ResumeLayout(false);
            this.tableLayoutPanel_recpath.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public MainForm MainForm
        {
            get
            {
                return (MainForm)this.MdiParent;
            }
        }

        private void DetailForm_Load(object sender, System.EventArgs e)
        {
            this.MainForm.AppInfo.LoadMdiSize += new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize += new EventHandler(AppInfo_SaveMdiSize);

            // 设置窗口尺寸状态
            if (MainForm.AppInfo != null)
            {
                MainForm.AppInfo.LoadMdiChildFormStates(this,
                    "mdi_form_state");

                // 恢复tab状态
                this.tabControl_record.SelectedIndex = MainForm.AppInfo.GetInt(
                    "detailform",
                    "tab_state",
                    0);

                // this.MarcEditor.Font = new Font("Fixedsys", 12);
                LoadFontToMarcEditor();
            }

            stop = new DigitalPlatform.Stop();

            stop.Register(MainForm.stopManager, true);	// 和容器关联

            this.Channels.AskAccountInfo += new AskAccountInfoEventHandle(MainForm.Servers.OnAskAccountInfo);
            /*
			this.Channels.procAskAccountInfo = 
				new Delegate_AskAccountInfo(MainForm.Servers.AskAccountInfo);
             */


            XmlEditor.Xml = "<root/>";	// ?????

            this.XmlEditor.GenerateData += new GenerateDataEventHandler(XmlEditor_GenerateData);

            this.Changed = false;

            this.listView_resFiles.procDownloadFiles = new Delegate_DownloadFiles(this.DownloadFiles);
            this.listView_resFiles.procDownloadOneMetaData = new Delegate_DownloadOneMetaData(this.DownloadOneFileMetaData);

            this.listView_resFiles.editor = this.XmlEditor;

            // 最好当确知库有marcdef配置文件时,才打开时钟.
            if (timer_crossRefresh.Enabled == false)
                timer_crossRefresh.Enabled = true;

            this.MarcEditor.GenerateData += new GenerateDataEventHandler(MarcEditor_GenerateData);
            this.MarcEditor.ParseMacro += new ParseMacroEventHandler(MarcEditor_ParseMacro);

            this.m_macroutil.ParseOneMacro += new ParseOneMacroEventHandler(m_macroutil_ParseOneMacro);
        }

        void XmlEditor_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this.AutoGenerate();
        }

#if NO
        static string Unquote(string strValue)
        {
            if (strValue.Length == 0)
                return "";
            if (strValue[0] == '%')
                strValue = strValue.Substring(1);
            if (strValue.Length == 0)
                return "";
            if (strValue[strValue.Length - 1] == '%')
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }
#endif

        // 解析每一个宏
        void m_macroutil_ParseOneMacro(object sender, ParseOneMacroEventArgs e)
        {
            string strError = "";
            string strName = StringUtil.Unquote(e.Macro, "%%");  // 去掉百分号

            // 函数名：
            string strFuncName = "";
            string strParams = "";

            int nRet = strName.IndexOf(":");
            if (nRet == -1)
            {
                strFuncName = strName.Trim();
            }
            else
            {
                strFuncName = strName.Substring(0, nRet).Trim();
                strParams = strName.Substring(nRet + 1).Trim();
            }

            if (String.Compare(strFuncName, "IncSeed", true) == 0)
            {
                string[] aParam = strParams.Split(new char[] { ',' });
                if (aParam.Length != 3 && aParam.Length != 2)
                {
                    strError = "IncSeed需要2或3个参数。";
                    goto ERROR1;
                }

                ResPath respath = new ResPath(textBox_recPath.Text);


                m_seedmanager.Initial(this.MainForm.SearchPanel,
                    respath.Url,
                    aParam[0].Trim());

                string strValue = "";

                if (e.Simulate == true)
                {
                    nRet = m_seedmanager.GetSeed(
                        aParam[1].Trim(),
                        out strValue,
                        out strError);
                    if (nRet == 0)
                    {
                        nRet = 1;
                        strValue = "1";
                    }
                }
                else
                {
                    nRet = m_seedmanager.IncSeed(
                        aParam[1].Trim(),
                        "1",
                        out strValue,
                        out strError);
                }
                if (nRet == -1)
                    goto ERROR1;

                // 补足左方'0'
                if (aParam.Length == 3)
                {
                    int nWidth = 0;
                    try
                    {
                        nWidth = Convert.ToInt32(aParam[2]);
                    }
                    catch
                    {
                        strError = "第三参数应当为纯数字（表示补足的宽度）";
                        goto ERROR1;
                    }
                    e.Value = strValue.PadLeft(nWidth, '0');
                }
                else
                    e.Value = strValue;
                return;
            }

            e.Canceled = true;  // 不能解释处理
            return;

        ERROR1:
            e.ErrorInfo = strError;
        }

        // MARC编辑器解决不了的宏, 求助于这里
        void MarcEditor_ParseMacro(object sender, ParseMacroEventArgs e)
        {
            string strResult = "";
            string strError = "";

            // 借助于MacroUtil进行处理
            int nRet = m_macroutil.Parse(
                e.Simulate,
                e.Macro,
                out strResult,
                out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            e.Value = strResult;
        }

        void MarcEditor_GenerateData(object sender, GenerateDataEventArgs e)
        {
            this.AutoGenerate();
        }

        private void DetailForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0 || stop.State == 1)
                {
                    this.channel.Abort();
                    e.Cancel = true;
                    return;
                }
            }

            if (this.Changed == true)
            {

                DialogResult result = MessageBox.Show(this,
                    "当前窗口中内容修改后未保存。是否要放弃保存并关闭窗口?\r\n\r\n(是)放弃保存并关闭窗口 (否)不关闭窗口",
                    "dp2rms",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

        }

        bool m_bPureXmlChanged = false;

        bool Changed
        {
            get
            {
                if (XmlEditor.Changed == true
                    || listView_resFiles.Changed == true)
                    return true;

                if (this.MarcEditor != null)
                {
                    if (this.MarcEditor.Changed == true)
                        return true;
                }

                if (this.m_bPureXmlChanged == true)
                    return true;

                return false;
            }

            set
            {
                XmlEditor.Changed = value;
                listView_resFiles.Changed = value;
                if (this.MarcEditor != null)
                {
                    this.MarcEditor.Changed = value;
                }
                this.m_bPureXmlChanged = value;
            }
        }

        private void DetailForm_Closed(object sender, System.EventArgs e)
        {
            eventClose.Set();

            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联

                // MainForm.stopManager.Remove(stop);
                stop = null;
            }

            /*
			if (accessPointWindow != null)
			{
				accessPointWindow.Close();
				accessPointWindow = null;
			}
             */


            this.Channels.AskAccountInfo -= new AskAccountInfoEventHandle(MainForm.Servers.OnAskAccountInfo);

            if (MainForm.AppInfo != null)
            {
                // 记忆窗口一般状态
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                "mdi_form_state");

                // 记忆tab状态
                MainForm.AppInfo.SetInt("detailform",
                "tab_state",
                this.tabControl_record.SelectedIndex);

                // 记忆MARC编辑器状态
                MainForm.AppInfo.SetBoolean(
                    "marceditor",
                    "EnterAsAutoGenerate",
                    this.MarcEditor.EnterAsAutoGenerate);


            }

            this.MarcEditor.GenerateData -= new GenerateDataEventHandler(MarcEditor_GenerateData);
            this.MarcEditor.ParseMacro -= new ParseMacroEventHandler(MarcEditor_ParseMacro);

            // MacroUtil需要事件支撑
            this.m_macroutil.ParseOneMacro -= new ParseOneMacroEventHandler(m_macroutil_ParseOneMacro);

            this.XmlEditor.GenerateData -= new GenerateDataEventHandler(XmlEditor_GenerateData);

            this.MainForm.AppInfo.LoadMdiSize -= new EventHandler(AppInfo_LoadMdiSize);
            this.MainForm.AppInfo.SaveMdiSize -= new EventHandler(AppInfo_SaveMdiSize);

        }

        public void AppInfo_LoadMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            try
            {
                // 获得splitContainer_main的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_main,
                    "detailform",
                    "splitContainer_main");
            }
            catch
            {
            }
        }

        void AppInfo_SaveMdiSize(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 分割条位置
                // 保存splitContainer_main的状态
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "detailform",
                    "splitContainer_main");
            }
        }


        public void LoadTemplate()
        {
            if (this.Changed == true)
            {

                DialogResult result = MessageBox.Show(this,
                    "装载模板前,发现当前窗口中已有内容修改后未来得及保存。是否要继续装载模板到窗口中(这样将丢失先前修改的内容)?\r\n\r\n(是)继续装载模板 (否)不装载模板",
                    "dp2rms",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    MessageBox.Show(this, "装载模板操作被放弃...");
                    return;
                }
            }

            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标数据库";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = textBox_recPath.Text;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            textBox_recPath.Text = dlg.Path + "/?";	// 为了追加保存

            // 下载配置文件
            ResPath respath = new ResPath(dlg.Path);


            // 使用Channel

            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                string strContent;
                string strError;

                string strCfgFilePath = respath.Path + "/cfgs/template";

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strCfgFilePath);
                stop.BeginLoop();

                byte[] baTimeStamp = null;
                string strMetaData;
                string strOutputPath;

                long lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strCfgFilePath,
                    // this.eventClose,
                    out strContent,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    this.TimeStamp = null;
                    MessageBox.Show(this, strError);
                    return;
                }
                else
                {
                    // MessageBox.Show(this, strContent);
                    SelectRecordTemplateDlg tempdlg = new SelectRecordTemplateDlg();
                    tempdlg.Font = GuiUtil.GetDefaultFont();

                    int nRet = tempdlg.Initial(strContent, out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, "装载配置文件 '" + strCfgFilePath + "' 发生错误: " + strError);
                        return;
                    }

                    tempdlg.ap = this.MainForm.AppInfo;
                    tempdlg.ApCfgTitle = "detailform_selecttemplatedlg";
                    tempdlg.ShowDialog(this);

                    if (tempdlg.DialogResult != DialogResult.OK)
                        return;


                    this.TimeStamp = null;
                    this.m_strMetaData = "";	// 记忆XML记录的元数据

                    this.strDatabaseOriginPath = ""; // 保存从数据库中来的原始path

                    nRet = this.SetRecordToControls(tempdlg.SelectedRecordXml,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }


                    this.TimeStamp = baTimeStamp;

                    this.Text = respath.ReverseFullPath;

                }

            }
            finally
            {
                channel = channelSave;
            }

        }

        private void button_findRecPath_Click(object sender, System.EventArgs e)
        {
            LoadTemplate();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        private void DetailForm_Activated(object sender, System.EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            // 菜单
            MainForm.MenuItem_properties.Enabled = true;
            MainForm.MenuItem_viewAccessPoint.Enabled = true;
            MainForm.MenuItem_dup.Enabled = true;
            MainForm.MenuItem_save.Enabled = true;
            MainForm.MenuItem_saveas.Enabled = true;
            MainForm.MenuItem_saveasToDB.Enabled = true;
            MainForm.MenuItem_saveToTemplate.Enabled = true;
            MainForm.MenuItem_autoGenerate.Enabled = true;

            if (this.tabControl_record.SelectedTab == this.tabPage_marc)
                MainForm.MenuItem_font.Enabled = true;  // ??
            else
                MainForm.MenuItem_font.Enabled = false;  // ??

            // 工具条按钮
            MainForm.toolBarButton_save.Enabled = true;
            MainForm.toolBarButton_refresh.Enabled = true;
            MainForm.toolBarButton_loadTemplate.Enabled = true;

            MainForm.toolBarButton_prev.Enabled = true;
            MainForm.toolBarButton_next.Enabled = true;

            SetDeleteToolButton();
        }

        public void LoadRecord(string strRecordPath,
            string strExtStyle)
        {
            string strError = "";
            int nRet = LoadRecord(strRecordPath,
                strExtStyle,
                out strError);
            if (nRet != 0)
                MessageBox.Show(this, strError);

        }

        // 装载记录
        // 把strRecordPath表示的记录装载到窗口中，并且在窗口第一行
        // 路径内容设置好
        // parameters:
        //		strRecordPath	记录路径。如果==null，表示直接用textBox_recPath中当前的内容作为路径
        //		strExtStyle	如果为null，表示获取strRecordPath或textbox表示的记录。如果为"next"或"prev"，
        //					则表示取其后或前一条记录
        // return:
        //		-2	放弃
        //		-1	出错
        //		0	正常
        //		1	到头或者到尾
        public int LoadRecord(string strRecordPath,
            string strExtStyle,
            out string strError)
        {
            strError = "";

            EnableControlsInLoading(true);

            try
            {

                if (this.Changed == true)
                {

                    DialogResult result = MessageBox.Show(this,
                        "装载新内容前, 发现当前窗口中已有内容修改后未来得及保存。是否要继续装载新内容到窗口中(这样将丢失先前修改过的内容)?\r\n\r\n(是)继续装载新内容 (否)不装载新内容",
                        "dp2rms",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        strError = "装载新内容操作被放弃...";
                        return -2;
                    }
                }

                if (strRecordPath != null)
                    textBox_recPath.Text = strRecordPath;

                ResPath respath = new ResPath(textBox_recPath.Text);

                this.Text = respath.ReverseFullPath;


                string strContent;
                string strMetaData;
                // string strError;
                byte[] baTimeStamp = null;
                string strOutputPath;


                // 使用Channel
                RmsChannel channelSave = channel;

                channel = Channels.GetChannel(respath.Url);
                Debug.Assert(channel != null, "Channels.GetChannel 异常");

                try
                {

                    string strStyle = "content,data,metadata,timestamp,outputpath,withresmetadata";	// 

                    if (strExtStyle != null && strExtStyle != "")
                    {
                        strStyle += "," + strExtStyle;
                    }

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在装载记录" + respath.FullPath);
                    stop.BeginLoop();



                    long lRet = channel.GetRes(respath.Path,
                        strStyle,
                        // this.eventClose,
                        out strContent,
                        out strMetaData,
                        out baTimeStamp,
                        out strOutputPath,
                        out strError);


                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    this.TimeStamp = baTimeStamp;	// 设置时间戳很重要。即便xml不合法，也应设置好时间戳，否则窗口无法进行正常删除。

                    this.strDatabaseOriginPath = respath.Url + "?" + strOutputPath; // 保存从数据库中来的原始path

                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFoundSubRes)
                        {
                            // 下级资源不存在, 警告一下就行了
                            MessageBox.Show(this, strError);
                            goto CONTINUELOAD;
                        }
                        else if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            if (strExtStyle == "prev")
                                strError = "到头";
                            else if (strExtStyle == "next")
                                strError = "到尾";
                            return 1;
                        }
                        else
                        {
                            // this.TimeStamp = null;
                            strError = "从路径 '" + respath.Path + "' 获取记录时出错: " + strError;
                            return -1;
                        }
                    }

                }
                finally
                {
                    channel = channelSave;
                }

            CONTINUELOAD:

                respath.Path = strOutputPath;
                textBox_recPath.Text = respath.FullPath;

                //string strTemp = ByteArray.GetHexTimeStampString(baTimeStamp);

                this.m_strMetaData = strMetaData;	// 记忆XML记录的元数据

                int nRet = SetRecordToControls(strContent,
                    out strError);
                if (nRet == -1)
                    return -1;


                return 0;
            }
            finally
            {
                EnableControlsInLoading(false);
            }
        }

        // 将XML记录装入各个控件
        int SetRecordToControls(string strContent,
            out string strError)
        {
            strError = "";

            // 统一控件时间戳
            //this.m_nTimeStampMarc = 0;
            //this.m_nTimeStampXml = 0;
            m_queueTextChanged.Clear();

            // 清除MARC格式的缓存
            this.MarcSyntax = "";


            listView_resFiles.Initial(XmlEditor);	// 把资源listview和XmlEditor勾连上

            try
            {
                XmlEditor.BeginUpdate();

                XmlEditor.Xml = strContent;

                XmlEditor.EndUpdate();

                this.Changed = false;	// xml内容的变化,会导致ResFileList也变化,所以消除bChanged on标志,应当从总体入手

                /*
                // 测试
                XmlDocument dom = new XmlDocument();
                dom.PreserveWhitespace = true;  // 在意空白符号
                dom.LoadXml(strContent);
                 * */
            }
            catch (Exception ex)
            {
                // 装入普通textbox
                this.textBox_xmlPureText.Text = strContent;

                if (strContent.Length < 4096)
                    strError = "装载XML记录内容 '" + strContent + "' 到DOM时出错: " + ex.Message;
                else
                    strError = "装载XML记录到DOM时出错: " + ex.Message;
                return -1;
            }

            // 规整缩进
            if (SetPlainTextXml(strContent, out strError) == -1)
                return -1;

            this.Flush();

            this.Changed = false;

            return 0;
        }

        // 向XML文本编辑器设置内容。将规整格式
        int SetPlainTextXml(string strXml,
            out string strError)
        {
            strError = "";
            string strOutXml = "";
            int nRet = DomUtil.GetIndentXml(strXml,
                out strOutXml,
                out strError);
            if (nRet == -1)
            {
                strError = "将XML字符串规整格式时出错: " + strError;
                return -1;
            }
            this.textBox_xmlPureText.Text = strOutXml;
            return 0;
        }

        // 从marcdef配置文件中获得marc格式定义
        // return:
        //		-1	出错
        //		0	没有找到
        //		1	找到
        int GetMarcSyntax(out string strMarcSyntax,
            out string strError)
        {
            strError = "";

            if (this.MarcSyntax != "")
            {
                // 如果需要改变窗口的MARC格式，需要清除this.MarcSyntax
                strMarcSyntax = this.MarcSyntax;
                return 1;
            }

            strMarcSyntax = "";
            int nRet = 0;
            Stream s = null;

            ResPath respath = new ResPath(textBox_recPath.Text);

            nRet = GetMarcDefCfgFile(respath.Url,
                ResPath.GetDbName(respath.Path),
                out s,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;


            s.Seek(0, SeekOrigin.Begin);

            // 从marcdef配置文件中得到marc格式字符串
            // return:
            //		-1	出错
            //		0	没有找到
            //		1	找到
            nRet = MarcUtil.GetMarcSyntaxFromCfgFile(s,
                out strMarcSyntax,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
                this.MarcSyntax = strMarcSyntax;
            else
            {
                this.MarcSyntax = "";
            }


            return nRet;
        }

        // 保存记录到另一数据库
        // parameters:
        public void SaveAsRecord()
        {
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标数据库";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = textBox_recPath.Text;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            SaveRecord(dlg.Path + "/?");
        }

        // 保存记录到模板配置文件
        // parameters:
        public void SaveToTemplate()
        {
            // 选择目标数据库
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标数据库";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = textBox_recPath.Text;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            // dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;


            // 下载模板配置文件
            ResPath respath = new ResPath(dlg.Path);

            string strError;
            string strContent;
            byte[] baTimeStamp = null;
            string strMetaData;
            string strOutputPath;

            string strCfgFilePath = respath.Path + "/cfgs/template";

            long lRet = 0;

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strCfgFilePath);
                stop.BeginLoop();



                lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strCfgFilePath,
                    out strContent,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (lRet == -1)
                {
                    this.TimeStamp = null;
                    MessageBox.Show(this, strError);
                    return;
                }

            }
            finally
            {
                channel = channelSave;
            }

            SelectRecordTemplateDlg tempdlg = new SelectRecordTemplateDlg();
            tempdlg.Font = GuiUtil.GetDefaultFont();

            int nRet = tempdlg.Initial(strContent, out strError);
            if (nRet == -1)
                goto ERROR1;


            tempdlg.Text = "请选择要修改的模板记录";
            tempdlg.CheckNameExist = false;	// 按OK按钮时不警告"名字不存在",这样允许新建一个模板
            tempdlg.ap = this.MainForm.AppInfo;
            tempdlg.ApCfgTitle = "detailform_selecttemplatedlg";
            tempdlg.ShowDialog(this);

            if (tempdlg.DialogResult != DialogResult.OK)
                return;

            string strXmlBody = "";
            bool bHasUploadedFile = false;


            nRet = GetXmlRecord(out strXmlBody,
                out bHasUploadedFile,
                out strError);
            if (nRet == -1)
                goto ERROR1;



            // 修改配置文件内容
            if (tempdlg.textBox_name.Text != "")
            {
                // 替换或者追加一个记录
                nRet = tempdlg.ReplaceRecord(tempdlg.textBox_name.Text,
                    strXmlBody,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }
            }

            if (tempdlg.Changed == false)	// 没有必要保存回去
                return;

            string strOutputXml = tempdlg.OutputXml;


            // 使用Channel
            channelSave = channel;

            // 重新获得一次channel, 是因为前面GetXmlRecord()函数有可能摧毁这个变量
            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {


                // 存回配置文件
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在保存配置文件 " + strCfgFilePath);
                stop.BeginLoop();

                byte[] baOutputTimeStamp = null;
                // string strOutputPath = "";

                EnableControlsInLoading(true);

                lRet = channel.DoSaveTextRes(strCfgFilePath,
                    strOutputXml,
                    true,	// bInlucdePreamble
                    "",	// style
                    baTimeStamp,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);

                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    strError = "保存配置文件" + strCfgFilePath + "失败，原因: " + strError;
                    goto ERROR1;
                }
            }
            finally
            {
                channel = channelSave;
            }

            MessageBox.Show(this, "修改模板配置文件成功。");

            return;

        ERROR1:
            MessageBox.Show(this, strError);

        }


        // 保存记录
        // parameters:
        //		strRecordPath	记录路径。如果==null，表示直接用textBox_recPath中当前的内容作为路径
        public void SaveRecord(string strRecordPath)
        {
            if (strRecordPath != null)
                textBox_recPath.Text = strRecordPath;

            if (textBox_recPath.Text == "")
            {
                MessageBox.Show(this, "路径不能为空");
                return;
            }

            ResPath respath = new ResPath(textBox_recPath.Text);

            Uri uri = null;
            try
            {
                uri = new Uri(respath.Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "路径错误: " + ex.Message);
                return;
            }
            // 保存到文件
            if (uri.IsFile)
            {
                MessageBox.Show(this, "暂时不支持保存到文件");
                return;
            }


            string strError;

            string strXml = "";
            bool bHasUploadedFile = false;

            int nRet = GetXmlRecord(out strXml,
                out bHasUploadedFile,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }


            byte[] baOutputTimeStamp = null;
            string strOutputPath = "";
            long lRet = 0;

            int nUploadCount = 0;

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在保存记录 " + respath.FullPath);
                stop.BeginLoop();


                EnableControlsInLoading(true);

                //string strTemp = ByteArray.GetHexTimeStampString(this.TimeStamp);

                if (String.IsNullOrEmpty(this.strDatabaseOriginPath) == false
                    && bHasUploadedFile == true
                    && respath.FullPath != this.strDatabaseOriginPath)
                {
                    ResPath respath_old = new ResPath(this.strDatabaseOriginPath);

                    if (respath_old.Url != respath.Url)
                    {
                        MessageBox.Show(this, "目前暂不支持跨服务器情况下的资源复制。本记录中原有的已上载资源，在另存到目标库的时丢失（为空），请注意保存完后手动上载。");
                        goto SKIPCOPYRECORD;
                    }
                    // 复制记录
                    // return:
                    //		-1	出错。错误信息在strError中
                    //		0或者其他		成功
                    nRet = channel.DoCopyRecord(respath_old.Path,
                        respath.Path,
                        false,  // bool bDeleteOriginRecord,
                        out baOutputTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, "复制资源时发生错误: " + strError);
                    }
                    else
                    {
                        // 为继续保存最新XML记录作准备
                        respath.Path = strOutputPath;   // ?形式路径其实已经确定
                        this.TimeStamp = baOutputTimeStamp;
                    }
                }
            SKIPCOPYRECORD:

                lRet = channel.DoSaveTextRes(respath.Path,
                    strXml,
                    false,	// bInlucdePreamble
                    "",	// style
                    this.TimeStamp,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);


                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    MessageBox.Show(this, "保存记录失败，原因: " + strError);
                    return;
                }

                //


                this.TimeStamp = baOutputTimeStamp;
                respath.Path = strOutputPath;
                textBox_recPath.Text = respath.FullPath;

                ////
                this.strDatabaseOriginPath = respath.Url + "?" + strOutputPath; // 保存从数据库中来的原始path

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在保存资源 " + respath.FullPath);
                stop.BeginLoop();

                EnableControlsInLoading(true);
                Debug.Assert(channel != null, "");
                // 保存对象资源,循环对象列表就可以了
                nUploadCount = this.listView_resFiles.DoUpload(
                    respath.Path,
                    channel,
                    stop,
                    out strError);
                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

            }
            finally
            {
                channel = channelSave;
            }

            if (nUploadCount == -1)
            {
                MessageBox.Show(this, "XML记录保存成功, 但保存资源失败，原因: " + strError);
                return;
            }

            if (nUploadCount > 0)
            {
                // 使用Channel
                channelSave = channel;

                channel = Channels.GetChannel(respath.Url);
                Debug.Assert(channel != null, "Channels.GetChannel 异常");


                // 需要重新获得时间戳
                string strStyle = "timestamp,metadata";	// withresmetadata
                string strMetaData = "";
                string strContent = "";

                try
                {
                    lRet = channel.GetRes(respath.Path,
                        strStyle,
                        out strContent,
                        out strMetaData,
                        out baOutputTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        MessageBox.Show(this, "重新获得时间戳 '" + respath.FullPath + "' 失败。原因 : " + strError);
                        return;
                    }
                }
                finally
                {
                    channel = channelSave;
                }
                this.TimeStamp = baOutputTimeStamp;	// 设置时间戳很重要。即便xml不合法，也应设置好时间戳，否则窗口无法进行正常删除。
                this.m_strMetaData = strMetaData;	// 记忆XML记录的元数据



            }

            this.Changed = false;

            MessageBox.Show(this, "保存记录 '" + respath.FullPath + "' 成功。");
        }

        // 删除记录
        // parameters:
        //		strRecordPath	记录路径。如果==null，表示直接用textBox_recPath中当前的内容作为路径
        public void DeleteRecord(string strRecordPath)
        {
            if (strRecordPath != null)
                textBox_recPath.Text = strRecordPath;

            if (textBox_recPath.Text == "")
            {
                MessageBox.Show(this, "路径不能为空");
                return;
            }

            ResPath respath = new ResPath(textBox_recPath.Text);

            Uri uri = null;
            try
            {
                uri = new Uri(respath.Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "路径错误: " + ex.Message);
                return;
            }			// 保存到文件
            if (uri.IsFile)
            {
                MessageBox.Show(this, "暂时不支持删除文件");
                return;
            }


            string strText = "你确实要删除位于服务器 '" + respath.Url + "' 上的记录 '" + respath.Path + "' 吗?";

            DialogResult msgResult = MessageBox.Show(this,
                strText,
                "dp2rms",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (msgResult != DialogResult.OK)
            {
                MessageBox.Show(this, "删除操作被放弃...");
                return;
            }

            string strError;
            byte[] baOutputTimeStamp = null;

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在删除记录 " + respath.FullPath);
                stop.BeginLoop();


                EnableControlsInLoading(true);

                long lRet = channel.DoDeleteRes(respath.Path,
                    this.TimeStamp,
                    out baOutputTimeStamp,
                    out strError);

                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (lRet == -1)
                {
                    MessageBox.Show(this, "删除记录 '" + respath.Path + "' 失败，原因: " + strError);
                    return;
                }

            }
            finally
            {
                channel = channelSave;
            }



            // 如果删除成功,原来时间戳遗留在this.TimeStamp中,也无害

            MessageBox.Show(this, "删除记录 '" + respath.FullPath + "' 成功。");

        }


        // 观察检索点
        // parameters:
        //		strRecordPath	记录路径。如果==null，表示直接用textBox_recPath中当前的内容作为路径
        public void ViewAccessPoint(string strRecordPath)
        {
            if (strRecordPath == null || strRecordPath == "")
                strRecordPath = textBox_recPath.Text;

            if (strRecordPath == "")
            {
                MessageBox.Show(this, "必须指定好路径后, 才能模拟创建检索点");
                return;
            }

            ResPath respath = new ResPath(strRecordPath);

            string strError;

            string strXml = "";

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {
                bool bHasUploadedFile = false;

                int nRet = GetXmlRecord(out strXml,
                    out bHasUploadedFile,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            finally
            {
                channel = channelSave;
            }

            ViewAccessPointForm accessPointWindow = MainForm.TopViewAccessPointForm;

            if (accessPointWindow == null)
            {
                accessPointWindow = new ViewAccessPointForm();
                // accessPointWindow.StartPosition = FormStartPosition.CenterScreen;
                accessPointWindow.Show();
                accessPointWindow.MdiParent = MainForm; // MDI子窗口
            }
            else
                accessPointWindow.Activate();

            /*
        else 
        {
            accessPointWindow.Focus();
            if (accessPointWindow.WindowState == FormWindowState.Minimized) 
            {
                accessPointWindow.WindowState = FormWindowState.Normal;
            }
        }
             */

            /*
			if (accessPointWindow.Visible == false) 
			{
				try // Close()过的窗口
				{
					accessPointWindow.Show();
				}
				catch (System.ObjectDisposedException)
				{
					accessPointWindow = new ViewAccessPointForm();
					accessPointWindow.StartPosition = FormStartPosition.CenterScreen;
					accessPointWindow.Show();
				}
			}
             */


            // 使用Channel
            channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获取检索点 " + respath.FullPath);
                stop.BeginLoop();


                EnableControlsInLoading(true);

                long lRet = channel.DoGetKeys(
                    respath.Path,
                    strXml,
                    "zh",
                    // "",
                    accessPointWindow,
                    stop,
                    out strError);

                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (lRet == -1)
                {
                    MessageBox.Show(this, "获取检索点失败，原因: " + strError);
                    return;
                }

                // 设置标题
                // ResPath respath = new ResPath(this.textBox_recPath.Text);
                accessPointWindow.Text = "观察检索点: " + respath.ReverseFullPath;


            }
            finally
            {
                channel = channelSave;
            }




        }


        // 启动查重
        // parameters:
        //		strRecordPath	记录路径。如果==null，表示直接用textBox_recPath中当前的内容作为路径
        public void SearchDup(string strRecordPath)
        {
            if (strRecordPath == null || strRecordPath == "")
                strRecordPath = textBox_recPath.Text;

            if (strRecordPath == "")
            {
                MessageBox.Show(this, "必须指定好路径后, 才能进行查重");
                return;
            }

            ResPath respath = new ResPath(strRecordPath);

            string strError;

            string strXml = "";
            bool bHasUploadedFile = false;

            int nRet = GetXmlRecord(out strXml,
                out bHasUploadedFile,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }


            // 最好激活当前已经存在的查重mdi窗口

            DupDlg dlg = new DupDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            // dlg.TopMost = true;
            dlg.OpenDetail -= new OpenDetailEventHandler(this.MainForm.OpenDetailCallBack);
            dlg.OpenDetail += new OpenDetailEventHandler(this.MainForm.OpenDetailCallBack);
            dlg.Closed -= new EventHandler(dupdlg_Closed);
            dlg.Closed += new EventHandler(dupdlg_Closed);

            SearchPanel searchpanel = new SearchPanel();
            searchpanel.Initial(this.MainForm.Servers,
                this.MainForm.cfgCache);

            searchpanel.ap = this.MainForm.AppInfo;
            searchpanel.ApCfgTitle = "detailform_dupdlg";

            string strDbFullName = respath.Url + "?" + ResPath.GetDbName(respath.Path);
            // 获得上次遗留的缺省查重方案名
            string strDupProjectName = GetUsedDefaultDupProject(strDbFullName);

            dlg.Initial(searchpanel,
                respath.Url,
                true);
            dlg.ProjectName = strDupProjectName;
            dlg.RecordFullPath = strRecordPath;
            dlg.Record = strXml;
            dlg.MdiParent = this.MainForm;	// MDI化
            dlg.Show();



            // this.MainForm.SetFirstMdiWindowState();
        }

        // 获得使用过的方案名
        public string GetUsedDefaultDupProject(string strOriginDbFullPath)
        {
            string strEntry = AttrNameEncode(strOriginDbFullPath);
            // 获得上次遗留的缺省查重方案名
            string strDupProjectName = this.MainForm.AppInfo.GetString(
                "origin_project_name",
                strEntry,
                "{default}");
            return strDupProjectName;
        }

        // 设置使用过的方案名
        public void SetUsedDefaultDupProject(string strOriginDbFullPath,
            string strDupProjectName)
        {
            string strEntry = AttrNameEncode(strOriginDbFullPath);
            try
            {
                this.MainForm.AppInfo.SetString(
                    "origin_project_name",
                    strEntry,
                    strDupProjectName);
            }
            catch
            {
            }
        }


        string AttrNameEncode(string strText)
        {
            strText = strText.Replace("/", "_");
            strText = strText.Replace("+", "_");
            strText = strText.Replace("?", "_");
            strText = strText.Replace(":", "_");
            strText = strText.Replace(" ", "_");

            return strText;
        }

        private void dupdlg_Closed(object sender, EventArgs e)
        {
            DupDlg dlg = (DupDlg)sender;
            string strDbFullName = dlg.OriginDbFullPath;

            this.SetUsedDefaultDupProject(strDbFullName, dlg.ProjectName);
        }


        public string PropertiesText
        {
            get
            {
                return "记录路径:\t" + this.textBox_recPath.Text + "\r\n"
                    + "时间戳:\t" + (this.TimeStamp == null ? "(空)" : ByteArray.GetHexTimeStampString(this.TimeStamp)) + "\r\n";
            }
        }

        // 在装载记录的时期, disable某些界面元素
        void EnableControlsInLoading(bool bLoading)
        {
            if (bLoading == true)
            {
                textBox_recPath.Enabled = false;
                button_findRecPath.Enabled = false;
                // XmlEditor.Enabled = false;	// 需要优化绘制过程
                listView_resFiles.Enabled = false;

                if (MainForm.ActiveMdiChild == this)
                {
                    // 工具条按钮
                    MainForm.toolBarButton_save.Enabled = false;
                    MainForm.toolBarButton_refresh.Enabled = false;
                    MainForm.toolBarButton_loadTemplate.Enabled = false;

                    MainForm.toolBarButton_prev.Enabled = false;
                    MainForm.toolBarButton_next.Enabled = false;
                }

            }
            else
            {
                textBox_recPath.Enabled = true;
                button_findRecPath.Enabled = true;
                XmlEditor.Enabled = true;
                listView_resFiles.Enabled = true;

                if (MainForm.ActiveMdiChild == this)
                {
                    // 工具条按钮
                    MainForm.toolBarButton_save.Enabled = true;
                    MainForm.toolBarButton_refresh.Enabled = true;
                    MainForm.toolBarButton_loadTemplate.Enabled = true;

                    MainForm.toolBarButton_prev.Enabled = true;
                    MainForm.toolBarButton_next.Enabled = true;
                }
            }



        }

        // 记录路径发生任何修改
        private void textBox_recPath_TextChanged(object sender, System.EventArgs e)
        {
            SetDeleteToolButton();
        }

        void SetDeleteToolButton()
        {
            if (this == MainForm.ActiveMdiChild)
            {
                if (textBox_recPath.Text != "")
                {
                    MainForm.toolBarButton_delete.Enabled = true;
                }
                else
                {
                    MainForm.toolBarButton_delete.Enabled = false;
                }
            }
        }


        // 回调函数
        public void DownloadFiles()
        {
            string[] ids = this.listView_resFiles.GetSelectedDownloadIds();
            string strError;

            for (int i = 0; i < ids.Length; i++)
            {
                int nRet = DownloadOneFile(ids[i],
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }

            MessageBox.Show(this, "下载资源文件完成 ...");
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        int DownloadOneFile(string strID,
            out string strError)
        {

            strError = "";
            ResPath respath = new ResPath(textBox_recPath.Text);
            string strResPath = respath.Path + "/object/" + strID;

            strResPath = strResPath.Replace(":", "/");


            string strLocalPath = this.listView_resFiles.GetLocalFileName(strID);

            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的本地文件名";
            dlg.CreatePrompt = false;
            dlg.FileName = strLocalPath == "" ? strID + ".res" : strLocalPath;
            dlg.InitialDirectory = Environment.CurrentDirectory;
            // dlg.Filter = "projects files (outer*.xml)|outer*.xml|All files (*.*)|*.*" ;

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                strError = "放弃";
                return -1;
            }


            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载资源文件 " + strResPath);
                stop.BeginLoop();

                byte[] baOutputTimeStamp = null;

                EnableControlsInLoading(true);

                string strMetaData;
                string strOutputPath = "";

                long lRet = channel.GetRes(strResPath,
                    dlg.FileName,
                    stop,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);

                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    MessageBox.Show(this, "下载资源文件失败，原因: " + strError);
                    goto ERROR1;
                }

            }
            finally
            {
                channel = channelSave;
            }
            return 0;

        ERROR1:
            return -1;

        }

        // 回调函数
        int DownloadOneFileMetaData(string strID,
            out string strResultXml,
            out byte[] timestamp,
            out string strError)
        {
            timestamp = null;
            strError = "";
            ResPath respath = new ResPath(textBox_recPath.Text);
            string strResPath = respath.Path + "/object/" + strID;

            strResPath = strResPath.Replace(":", "/");

            // 使用Channel

            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载资源文件的元数据 " + strResPath);
                stop.BeginLoop();

                byte[] baOutputTimeStamp = null;
                string strOutputPath = "";

                EnableControlsInLoading(true);

                // 只得到metadata
                long lRet = channel.GetRes(strResPath,
                    (Stream)null,
                    stop,
                    "metadata,timestamp,outputpath",
                    null,
                    out strResultXml,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);

                EnableControlsInLoading(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    MessageBox.Show(this, "下载资源文件元数据失败，原因: " + strError);
                    goto ERROR1;
                }

                timestamp = baOutputTimeStamp;

            }
            finally
            {
                channel = channelSave;
            }
            return 0;

        ERROR1:
            return -1;
        }

        // 获得Xml记录
        // 本函数有修改this.channel的危险
        // parameters:
        //      bHasFile    是否具有已上载的资源文件?
        int GetXmlRecord(out string strXml,
            out bool bHasUploadedFile,
            out string strError)
        {
            bHasUploadedFile = false;
            strXml = "";

            int nRet = 0;

            if (!(Control.ModifierKeys == Keys.Control))
            {
                // 确保控件内容同步
                nRet = this.Flush(out strError);
                if (nRet == -1)
                    return -1;
            }


            // 从XML数据中移除工作用的临时属性
            nRet = ResFileList.RemoveWorkingAttrs(XmlEditor.Xml,
                out strXml,
                out bHasUploadedFile,
                out strError);
            if (nRet == -1)
            {
                strError = "RemoveWorkingAttrs()失败，原因: " + strError;
                return -1;
            }

            return 0;
        }

        // 保存到备份格式
        public void SaveToBackup(string strOutputFileName)
        {
            string strError;

            // 询问文件名
            if (strOutputFileName == null)
            {
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要保存的备份文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = false;
                dlg.FileName = MainForm.UsedBackupFileName;  // "*.dp2bak";
                dlg.InitialDirectory = Environment.CurrentDirectory;
                dlg.Filter = "backup files (*.dp2bak)|*.dp2bak|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                strOutputFileName = dlg.FileName;
                MainForm.UsedBackupFileName = strOutputFileName;    // 记忆下来
            }

            bool bOverwrite = false;

            if (File.Exists(strOutputFileName) == true)
            {
                OverwriteOrAppendBackupFileDlg dlg = new OverwriteOrAppendBackupFileDlg();
                dlg.Font = GuiUtil.GetDefaultFont();

                dlg.Text = "文件已经存在，是否覆盖?";
                dlg.Message = "文件 " + strOutputFileName + " 已经存在，是追加还是覆盖？";
                dlg.ShowDialog(this);

                if (dlg.DialogResult == DialogResult.Yes)
                {
                    // 追加
                    bOverwrite = false;
                }
                else if (dlg.DialogResult == DialogResult.No)
                {
                    // 覆盖
                    bOverwrite = true;
                }
                return; // 放弃
            }

            // 打开文件

            FileStream fileTarget = File.Open(
                strOutputFileName,
                FileMode.OpenOrCreate,	// 原来是Open，后来修改为OpenOrCreate。这样对临时文件被系统管理员手动意外删除(但是xml文件中仍然记载了任务)的情况能够适应。否则会抛出FileNotFoundException异常
                FileAccess.Write,
                FileShare.ReadWrite);

            if (bOverwrite == true)
                fileTarget.SetLength(0);

            try
            {

                fileTarget.Seek(0, SeekOrigin.End);	// 具有追加的能力

                long lStart = fileTarget.Position;	// 记忆起始位置

                byte[] length = new byte[8];

                fileTarget.Write(length, 0, 8);	// 临时写点数据,占据记录总长度位置

                bool bHasUploadedFile = false;


                // 获得Xml记录
                string strXmlBody;
                int nRet = GetXmlRecord(out strXmlBody,
                    out bHasUploadedFile,
                    out strError);
                if (nRet == -1)
                {
                    fileTarget.SetLength(lStart);	// 把本次追加写入的全部去掉
                    goto ERROR1;
                }

                ResPath respath = new ResPath(textBox_recPath.Text);

                // 向backup文件中保存第一个 res
                StringUtil.ChangeMetaData(ref this.m_strMetaData, // ResFileList
                    null,
                    null,
                    null,
                    null,
                    respath.FullPath,
                    ByteArray.GetHexTimeStampString(this.TimeStamp));   // 任延华加 2005/6/11

                long lRet = Backup.WriteFirstResToBackupFile(
                    fileTarget,
                    this.m_strMetaData,
                    strXmlBody);

                // 使用Channel
                RmsChannel channelSave = channel;

                channel = Channels.GetChannel(respath.Url);
                Debug.Assert(channel != null, "Channels.GetChannel 异常");

                try
                {

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在保存记录 " + respath.FullPath + "到备份文件 " + strOutputFileName);
                    stop.BeginLoop();

                    EnableControlsInLoading(true);

                    nRet = this.listView_resFiles.DoSaveResToBackupFile(
                        fileTarget,
                        respath.Path,
                        channel,
                        stop,
                        out strError);

                    EnableControlsInLoading(false);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    if (nRet == -1)
                    {
                        fileTarget.SetLength(lStart);	// 把本次追加写入的全部去掉
                        strError = "保存记录失败，原因: " + strError;
                        goto ERROR1;
                    }
                }
                finally
                {
                    channel = channelSave;
                }

                // 写入总长度
                long lTotalLength = fileTarget.Position - lStart - 8;
                byte[] data = BitConverter.GetBytes(lTotalLength);

                fileTarget.Seek(lStart - fileTarget.Position, SeekOrigin.Current);
                Debug.Assert(fileTarget.Position == lStart, "");

                // fileTarget.Seek(lStart, SeekOrigin.Begin);   // 性能
                fileTarget.Write(data, 0, 8);
            }
            finally
            {
                fileTarget.Close();
                fileTarget = null;
            }

            string strText = "";

            if (bOverwrite == false)
                strText = "追加保存备份文件完成...";
            else
                strText = "覆盖保存备份文件完成 ...";

            MessageBox.Show(this, strText);
            return;


        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 自动加工数据
        public void AutoGenerate()
        {
            // 库名部分路径
            ResPath respath = new ResPath(textBox_recPath.Text);
            respath.MakeDbName();

            string strError;
            string strCode;
            string strRef;

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {
                string strCfgPath = respath.Path + "/cfgs/autoGenerate.cs";

                string strCfgRefPath = respath.Path + "/cfgs/autoGenerate.cs.ref";

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strCfgPath);
                stop.BeginLoop();

                byte[] baTimeStamp = null;
                string strMetaData;
                string strOutputPath;

                long lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strCfgPath,
                    out strCode,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (lRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }


                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strCfgRefPath);
                stop.BeginLoop();

                lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strCfgRefPath,
                    out strRef,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

            }
            finally
            {
                channel = channelSave;
            }



            // 执行代码
            int nRet = RunScript(strCode,
                strRef,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

        }

        int RunScript(string strCode,
            string strRef,
            out string strError)
        {
            strError = "";
            string[] saRef = null;
            int nRet;
            string strWarning = "";

            nRet = Script.GetRefs(strRef,
                out saRef,
                out strError);
            if (nRet == -1)
                return -1;

            string[] saAddRef = {
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\digitalplatform.xmleditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.rms.dll",
									Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
									Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2rms.exe"
								};

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            Assembly assembly = Script.CreateAssembly(
                strCode,
                saRef,
                null,	// strLibPaths,
                null,	// strOutputFile,
                out strError,
                out strWarning);
            if (assembly == null)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strError;
                return -1;
            }

            // 得到Assembly中Host派生类Type
            Type entryClassType = Script.GetDerivedClassType(
                assembly,
                "dp2rms.Host");

            // new一个Host派生对象
            Host hostObj = (Host)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            // 为Host派生类设置参数
            hostObj.DetailForm = this;
            hostObj.Assembly = assembly;

            HostEventArgs e = new HostEventArgs();

            nRet = this.Flush(out strError);
            if (nRet == -1)
                return -1;


            hostObj.Main(null, e);

            nRet = this.Flush(out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        /*
        public char[] GetSpecialChars()
        {
            string strChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";
            char[] result = new char[strChars.Length];

            for(int i=0;i<result.Length;i++)
            {
                result[i] = strChars[i];
            }

            return result;
        }
        */

        // 兼容以前版本
        public int HanziTextToPinyin(
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            return HanziTextToPinyin(
                false,
                strText,
                style,
                out strPinyin,
                out strError);
        }


        // 把字符串中的汉字和拼音分离
        // parameters:
        //      bLocal  是否从本地获取拼音
        // return:
        //      -1  出错
        //      0   用户希望中断
        //      1   正常
        public int HanziTextToPinyin(
            bool bLocal,
            string strText,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strPinyin = "";

            string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";


            string strHanzi;
            int nStatus = -1;	// 前面一个字符的类型 -1:前面没有字符 0:普通英文字母 1:空格 2:汉字


            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                strHanzi = "";

                if (ch >= 0 && ch <= 128)
                {
                    if (nStatus == 2)
                        strPinyin += " ";

                    strPinyin += ch;

                    if (ch == ' ')
                        nStatus = 1;
                    else
                        nStatus = 0;

                    continue;
                }
                else
                {	// 汉字
                    strHanzi += ch;
                }

                // 汉字前面出现了英文或者汉字，中间间隔空格
                if (nStatus == 2 || nStatus == 0)
                    strPinyin += " ";


                // 看看是否特殊符号
                if (strSpecialChars.IndexOf(strHanzi) != -1)
                {
                    strPinyin += strHanzi;	// 放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }


                // 获得拼音
                string strResultPinyin = "";

                int nRet = 0;

                if (bLocal == true)
                {
                    nRet = this.MainForm.LoadQuickPinyin(true, out strError);
                    if (nRet == -1)
                        return -1;
                    nRet = this.MainForm.QuickPinyin.GetPinyin(
                        strHanzi,
                        out strResultPinyin,
                        out strError);
                }
                else
                {
                    nRet = GetOnePinyin(strHanzi,
                         out strResultPinyin,
                         out strError);
                }
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {	// canceld
                    strPinyin += strHanzi;	// 只好将汉字放在本应是拼音的位置
                    nStatus = 2;
                    continue;
                }

                Debug.Assert(strResultPinyin != "", "");

                strResultPinyin = strResultPinyin.Trim();
                if (strResultPinyin.IndexOf(";", 0) != -1)
                {	// 如果是多个拼音
                    SelPinyinDlg dlg = new SelPinyinDlg();
                    dlg.Font = GuiUtil.GetDefaultFont();

                    dlg.SampleText = strText;
                    dlg.Offset = i;
                    dlg.Pinyins = strResultPinyin;
                    dlg.Hanzi = strHanzi;

                    MainForm.AppInfo.LinkFormState(dlg, "SelPinyinDlg_state");

                    dlg.ShowDialog(this);

                    MainForm.AppInfo.UnlinkFormState(dlg);

                    Debug.Assert(DialogResult.Cancel != DialogResult.Abort, "推断");

                    if (dlg.DialogResult == DialogResult.Cancel)
                    {
                        strPinyin += strHanzi;
                    }
                    else if (dlg.DialogResult == DialogResult.OK)
                    {
                        strPinyin += ConvertSinglePinyinByStyle(
                            dlg.ResultPinyin,
                            style);
                    }
                    else if (dlg.DialogResult == DialogResult.Abort)
                    {
                        return 0;   // 用户希望整个中断
                    }
                    else
                    {
                        Debug.Assert(false, "SelPinyinDlg返回时出现意外的DialogResult值");
                    }
                }
                else
                {
                    // 单个拼音

                    strPinyin += ConvertSinglePinyinByStyle(
                        strResultPinyin,
                        style);
                }
                nStatus = 2;
            }

            return 1;   // 正常结束
        }

        // 获得一个汉字的拼音
        // 所获得的拼音, 是一个分号间隔的字符串, 表示对应于这个汉字的多音
        // return:
        //		-1	error
        //		1	found
        //		0	not found
        int GetOnePinyin(string strOneHanzi,
            out string strPinyin,
            out string strError)
        {
            strPinyin = "";
            strError = "";

            // 拼音库路径
            string strPinyinDbPath = MainForm.AppInfo.GetString("pinyin",
                "pinyin_db_path",
                "");

            if (String.IsNullOrEmpty(strPinyinDbPath) == true)
            {
                strError = "拼音库路径尚未配置。请先用菜单“帮助／系统参数设置”功能配置适当的拼音库路径。";
                return -1;
            }

            ResPath respath = new ResPath(strPinyinDbPath);

            string strDbName = respath.Path;

            // 2007/4/5 改造 加上了 GetXmlStringSimple()
            string strQueryXml = "<target list='" + strDbName + ":" + "汉字'><item><word>"
                + StringUtil.GetXmlStringSimple(strOneHanzi)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>chi</lang></target>";

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索拼音 '" + strOneHanzi + "'");
                stop.BeginLoop();

                try
                {
                    long nRet = channel.DoSearch(strQueryXml,
                        "default",
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "检索拼音库时出错: " + strError;
                        return -1;
                    }
                    if (nRet == 0)
                        return 0;	// not found

                    List<string> aPath = null;
                    nRet = channel.DoGetSearchResult(
                        "default",
                        1,
                        this.Lang,
                        stop,
                        out aPath,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "检索拼音库获取检索结果时出错: " + strError;
                        return -1;
                    }

                    if (aPath.Count == 0)
                    {
                        strError = "检索拼音库获取的检索结果为空";
                        return -1;
                    }

                    string strStyle = "content,data";

                    string strContent;
                    string strMetaData;
                    byte[] baTimeStamp;
                    string strOutputPath;

                    nRet = channel.GetRes((string)aPath[0],
                        strStyle,
                        // this.eventClose,
                        out strContent,
                        out strMetaData,
                        out baTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取拼音记录体时出错: " + strError;
                        return -1;
                    }

                    // 取出拼音字符串
                    XmlDocument dom = new XmlDocument();


                    try
                    {
                        dom.LoadXml(strContent);
                    }
                    catch (Exception ex)
                    {
                        strError = "汉字 '" + strOneHanzi + "' 所获取的拼音记录 " + strContent + " XML数据装载出错: " + ex.Message;
                        return -1;
                    }

                    strPinyin = DomUtil.GetAttr(dom.DocumentElement, "p");

                    return 1;

                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }

            }
            finally
            {
                channel = channelSave;
            }

        }

        // return:
        //		-1	出错
        //		0	没有找到
        //		1	找到
        int GetMarcDefCfgFile(string strUrl,
            string strDbName,
            out Stream s,
            out string strError)
        {
            strError = "";
            s = null;

            if (String.IsNullOrEmpty(strUrl) == true)
            {
                /*
                strError = "URL为空";
                goto ERROR1;
                 */
                return 0;
            }

            string strPath = strDbName + "/cfgs/marcdef";

            // 使用Channel
            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(strUrl);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            try
            {

                string strContent;
                // string strError;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strPath);
                stop.BeginLoop();

                byte[] baTimeStamp = null;
                string strMetaData;
                string strOutputPath;

                long lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strPath,
                    out strContent,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        return 0;

                    strError = "获得配置文件 '" + strPath + "' 时出错：" + strError;
                    goto ERROR1;
                }
                else
                {
                    byte[] baContent = StringUtil.GetUtf8Bytes(strContent, true);
                    MemoryStream stream = new MemoryStream(baContent);
                    s = stream;
                }

            }
            finally
            {
                channel = channelSave;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 保证控件内容同步
        int CrossRefreshControls(out string strError)
        {
            strError = "";
            int nRet = 0;

            this.m_nChangeTextNest++;

            try
            {

                if (this.m_queueTextChanged.Count == 0)	// m_nTimeStampMarc == m_nTimeStampXml
                    return 0;

                TextChangedInfo tail = (TextChangedInfo)this.m_queueTextChanged[this.m_queueTextChanged.Count - 1];

                if (tail.ControlChanged == this.MarcEditor)	// m_nTimeStampMarc > m_nTimeStampXml
                {
                    // MARC控件中内容更新一些. 需要刷新到xml控件中
                    MemoryStream s = new MemoryStream();

                    MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8);

                    string strMarcSyntax = "";
                    nRet = GetMarcSyntax(out strMarcSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 在当前没有定义MARC语法的情况下，默认unimarc
                    if (nRet == 0 && strMarcSyntax == "")
                        strMarcSyntax = "unimarc";

                    /*
                    if (strMarcSyntax == "unimarc")
                    {
                        writer.MarcNameSpaceUri = DpNs.unimarcxml;
                        writer.MarcPrefix = strMarcSyntax;
                    }
                    else if (strMarcSyntax == "usmarc")
                    {
                        writer.MarcNameSpaceUri = Ns.usmarcxml;
                        writer.MarcPrefix = strMarcSyntax;
                    }
                    else 
                    {
                        writer.MarcNameSpaceUri = DpNs.unimarcxml;
                        writer.MarcPrefix = "unimarc";
                    }

                    string strMARC = this.MarcEditor.Marc;
                    string strDebug = strMARC.Replace((char)Record.FLDEND, '#');
                    nRet = writer.WriteRecord(strMARC,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    writer.Flush();
                    s.Flush();
					
                    s.Seek(0, SeekOrigin.Begin);

                    XmlDocument domMarc = new XmlDocument();
                    try 
                    {
                        domMarc.Load(s);
                    }
                    catch (Exception ex)
                    {

                        strError = ex.Message;
                        goto ERROR1;
                    }
                    finally 
                    {
                        //File.Delete(strTempFileName);
                        s.Close();

                    }
                     * */
                    string strMARC = this.MarcEditor.Marc;
                    XmlDocument domMarc = null;
                    nRet = MarcUtil.Marc2Xml(strMARC,
                        strMarcSyntax,
                        out domMarc,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // this.XmlEditor.Xml = domMarc.DocumentElement.OuterXml;

                    Cursor oldcursor = this.Cursor;
                    this.Cursor = Cursors.WaitCursor;

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(this.XmlEditor.Xml);

                    XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
                    mngr.AddNamespace("unimarc", DpNs.unimarcxml);
                    mngr.AddNamespace("usmarc", Ns.usmarcxml);
                    mngr.AddNamespace("dprms", DpNs.dprms);

                    // 保护unimarc usmarc名字空间以外的元素
                    // XmlNodeList filenodes = dom.DocumentElement.SelectNodes("child::*[not(unimarc:controlfield) AND not(unimarc:datafield) AND not(unimarc:leader)]", mngr);    // "//dprms:file"
                    List<XmlNode> filenodes = new List<XmlNode>();
                    for (int i = 0; i < dom.DocumentElement.ChildNodes.Count; i++)
                    {
                        XmlNode node = dom.DocumentElement.ChildNodes[i];
                        if (node.NamespaceURI == DpNs.unimarcxml
                            || node.NamespaceURI == Ns.usmarcxml)
                        {
                            continue;
                        }

                        filenodes.Add(node);
                    }

                    XmlElement marcroot = null;
                    XmlNodeList recordItems = dom.DocumentElement.SelectNodes("//" + strMarcSyntax + ":record",
                        mngr);
                    if (recordItems.Count == 0)
                    {
                        marcroot = dom.CreateElement(strMarcSyntax,
                            "record",
                            MarcUtil.GetMarcURI(strMarcSyntax));
                        marcroot = (XmlElement)dom.DocumentElement.AppendChild(marcroot);
                    }
                    else
                    {
                        marcroot = (XmlElement)recordItems[0];

#if NO
                        // 如果MARC根就是文档根,并且已经有<dprms:file>元素
                        if (marcroot == dom.DocumentElement && filenodes.Count > 0)
                        {
                            // 保存全部<file>元素
                            // List<XmlNode> fs = new List<XmlNode>();

                            // 修改文档根?
                            dom.LoadXml("<root/>");

                            marcroot = dom.CreateElement(strMarcSyntax,
                                "record",
                                MarcUtil.GetMarcURI(strMarcSyntax));
                            marcroot = (XmlElement)dom.DocumentElement.AppendChild(marcroot);

                            // 插入先前保存的<dprms:file>元素
                            for (int i = 0; i < filenodes.Count; i++)
                            {
                                dom.DocumentElement.AppendChild(filenodes[i]);
                            }
                        }
#endif
                    }


                    try
                    {
                        marcroot.InnerXml = domMarc.DocumentElement.InnerXml;
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        goto ERROR1;
                    }

                    // 插入先前保存的<dprms:file>元素
                    for (int i = 0; i < filenodes.Count; i++)
                    {
                        dom.DocumentElement.AppendChild(filenodes[i]);
                    }

                    this.XmlEditor.Xml = dom.DocumentElement.OuterXml;

                    // 需要规整缩进
                    if (SetPlainTextXml(dom.DocumentElement.OuterXml, out strError) == -1)
                        goto ERROR1;

                    this.Cursor = oldcursor;

                    /*
                        XmlNamespaceManager mngr = new XmlNamespaceManager(new NameTable());
                        mngr.AddNamespace("unimarc", DpNs.unimarcxml);
                        mngr.AddNamespace("usmarc", Ns.usmarcxml);
			
                        ElementItem marcroot = null;
                        ItemList recordItems = this.XmlEditor.DocumentElement.SelectItems("//"+strMarcSyntax+":record",
                            mngr);
                        if (recordItems.Count == 0)
                        {
                            marcroot = this.XmlEditor.CreateElementItem(strMarcSyntax,
                                "record",
                                MarcUtil.GetMarcURI(strMarcSyntax));
                            this.XmlEditor.DocumentElement.AppendChild(marcroot);
                        }
                        else 
                        {
                            marcroot = (ElementItem)recordItems[0];
                        }

                        try 
                        {
                            marcroot.OuterXml = domMarc.DocumentElement.OuterXml;
                        }
                        catch (Exception ex)
                        {
                            strError = ex.Message;
                            goto ERROR1;
                        }

                        */


                    // 统一控件时间戳
                    //this.m_nTimeStampMarc = 0;
                    //this.m_nTimeStampXml = 0;

                }

                if (tail.ControlChanged == this.XmlEditor)	// m_nTimeStampXml > m_nTimeStampMarc
                {
                    // xml控件中内容更新一些. 需要刷新到MARC控件中
                    // 从xml控件到marc控件
                    string strMarc = "";
                    string strMarcSyntax = "";

                    nRet = GetMarcSyntax(out strMarcSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strOutMarcSyntax = "";

                    string strXml = this.XmlEditor.Xml;
                    if (strXml != "")
                    {

                        nRet = MarcUtil.Xml2Marc(strXml,
                            true,
                            strMarcSyntax,
                            out strOutMarcSyntax,
                            out strMarc,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "XML转换到MARC记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }

                    try
                    {
                        this.MarcEditor.Marc = strMarc;
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }

                    // 需要规整缩进
                    if (SetPlainTextXml(strXml, out strError) == -1)
                        return -1;

                    // 统一控件时间戳
                    //this.m_nTimeStampMarc = 0;
                    //this.m_nTimeStampXml = 0;

                }

                if (tail.ControlChanged == this.textBox_xmlPureText)
                {
                    // textbox控件中内容更新一些. 需要刷新到MARC控件中
                    // 从textbox控件到marc控件
                    string strMarc = "";
                    string strMarcSyntax = "";

                    nRet = GetMarcSyntax(out strMarcSyntax,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    string strOutMarcSyntax = "";

                    string strXml = this.textBox_xmlPureText.Text;
                    if (string.IsNullOrEmpty(strXml) == false)
                    {
                        nRet = MarcUtil.Xml2Marc(strXml,
                            true,
                            strMarcSyntax,
                            out strOutMarcSyntax,
                            out strMarc,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "XML转换到MARC记录时出错: " + strError;
                            goto ERROR1;
                        }
                    }

                    try
                    {
                        this.MarcEditor.Marc = strMarc;
                    }
                    catch (Exception ex)
                    {
                        strError = ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }

                    this.XmlEditor.Xml = strXml;
                }

                this.m_queueTextChanged.Clear();

                return 0;
            ERROR1:
                return -1;
            }
            finally
            {
                this.m_nChangeTextNest--;
            }
        }

        private void tabControl_record_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            // 菜单有所变化
            if (this.tabControl_record.SelectedTab == this.tabPage_marc)
                MainForm.MenuItem_font.Enabled = true;  // ??
            else
                MainForm.MenuItem_font.Enabled = false;  // ??


            string strError = "";
            int nRet = 0;

            //nRet = CrossRefreshControls(out strError);
            nRet = PutTextChangedInfoToQueue(null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void MarcEditor_TextChanged(object sender, System.EventArgs e)
        {
            if (this.m_nChangeTextNest != 0)
                return;

            // m_nTimeStampMarc ++;


            // 当前xmleditor处于看不见的状态, 注意及时刷新marc
            string strError = "";
            int nRet = 0;

            nRet = PutTextChangedInfoToQueue(this.MarcEditor,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 如果其他窗口为可见，立即兑现刷新 2006/5/30 add
            if (this.tabControl_record.SelectedTab != this.tabPage_marc)
            {
                nRet = CrossRefreshControls(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }


            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
            // }
        }

        private void xmlEditor_TextChanged(object sender, System.EventArgs e)
        {
            if (this.m_nChangeTextNest != 0)
                return;

            // m_nTimeStampXml ++;

            /*
            if (this.tabControl_record.SelectedTab == this.tabPage_xml)
            {
            }
            else 
            {
            */
            // 当前xmleditor处于看不见的状态, 注意及时刷新marc
            string strError = "";
            int nRet = 0;

            nRet = PutTextChangedInfoToQueue(this.XmlEditor,
                    out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            // 如果其他窗口为可见，立即兑现刷新 2006/5/30 add
            if (this.tabControl_record.SelectedTab != this.tabPage_xml
                && String.IsNullOrEmpty(this.textBox_recPath.Text) == false)
            {
                nRet = CrossRefreshControls(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
             **/


            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
            //}
        }

        int PutTextChangedInfoToQueue(object controlChanged,
            out string strError)
        {
            strError = "";

            if (controlChanged == null && this.m_queueTextChanged.Count == 0)
            {
                this.m_queueTextChanged.Clear();
                return 0;
            }


            TextChangedInfo info = new TextChangedInfo();
            info.ControlChanged = controlChanged;

            // 看插入前, 队列尾部最后一个的方向是否相同
            if (this.m_queueTextChanged.Count > 0)
            {
                TextChangedInfo tail = (TextChangedInfo)this.m_queueTextChanged[this.m_queueTextChanged.Count - 1];
                if (tail.ControlChanged != controlChanged)
                {
                    int nRet = 0;

                    nRet = CrossRefreshControls(out strError);
                    if (nRet == -1)
                    {
                        this.m_queueTextChanged.Clear();
                        this.m_queueTextChanged.Add(info);
                        return -1;
                    }

                    if (controlChanged == null)
                    {
                        this.m_queueTextChanged.Clear();
                        return 0;
                    }


                }
            }

            this.m_queueTextChanged.Clear();
            if (controlChanged != null)
            {
                this.m_queueTextChanged.Add(info);
            }
            return 0;
        }

        public int Flush(out string strError)
        {
            strError = "";
            if (this.tabControl_record.SelectedTab == this.tabPage_marc)
            {
                this.MarcEditor.Flush();
            }
            else if (this.tabControl_record.SelectedTab == this.tabPage_xml)
            {
                this.XmlEditor.Flush();
            }

            int nRet = 0;

            nRet = PutTextChangedInfoToQueue(null,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        public void Flush()
        {

            string strError = "";
            int nRet = 0;

            nRet = this.Flush(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void timer_crossRefresh_Tick(object sender, System.EventArgs e)
        {
            /*
			 Flush();   // 打开
             */
        }

        public class TextChangedInfo
        {
            public object ControlChanged = null;
        }


        static string ConvertSinglePinyinByStyle(string strPinyin,
            PinyinStyle style)
        {
            if (style == PinyinStyle.None)
                return strPinyin;
            if (style == PinyinStyle.Upper)
                return strPinyin.ToUpper();
            if (style == PinyinStyle.Lower)
                return strPinyin.ToLower();
            if (style == PinyinStyle.UpperFirst)
            {
                if (strPinyin.Length > 1)
                {
                    return strPinyin.Substring(0, 1).ToUpper() + strPinyin.Substring(1).ToLower();
                }

                return strPinyin;
            }

            Debug.Assert(false, "未定义的拼音风格");
            return strPinyin;
        }


        // 当前窗口中记录的路径
        public string RecPath
        {
            get
            {
                return textBox_recPath.Text;
            }
            set
            {
                textBox_recPath.Text = value;
            }
        }

        // 设置字体
        public void SetFont()
        {
            FontDialog dlg = new FontDialog();

            dlg.ShowColor = true;
            dlg.Color = this.MarcEditor.ContentTextColor;
            dlg.Font = this.MarcEditor.Font;
            dlg.ShowApply = true;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            dlg.Apply += new EventHandler(dlgFont_Apply);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            this.MarcEditor.Font = dlg.Font;
            this.MarcEditor.ContentTextColor = dlg.Color;

            // 保存到配置文件
            SaveFontForMarcEditor();
        }

        void dlgFont_Apply(object sender, EventArgs e)
        {
            FontDialog dlg = (FontDialog)sender;

            this.MarcEditor.Font = dlg.Font;
            this.MarcEditor.ContentTextColor = dlg.Color;

            // 保存到配置文件
            SaveFontForMarcEditor();
        }

        void LoadFontToMarcEditor()
        {
            string strFaceName = MainForm.AppInfo.GetString("marceditor",
                "fontface",
                "Verdana");
            float fFontSize = (float)Convert.ToDouble(MainForm.AppInfo.GetString("marceditor",
                "fontsize",
                "12.0"));

            string strColor = MainForm.AppInfo.GetString("marceditor",
                "fontcolor",
                "");

            string strStyle = MainForm.AppInfo.GetString("marceditor",
                "fontstyle",
                "");
            FontStyle style = FontStyle.Regular;
            if (String.IsNullOrEmpty(strStyle) == false)
            {
                style = (FontStyle)Enum.Parse(typeof(FontStyle), strStyle, true);
            }

            if (String.IsNullOrEmpty(strColor) == false)
            {
                this.MarcEditor.ContentTextColor = ColorUtil.String2Color(strColor);
            }

            this.MarcEditor.Font = new Font(strFaceName, fFontSize, style);

            this.MarcEditor.EnterAsAutoGenerate = MainForm.AppInfo.GetBoolean(
                "marceditor",
                "EnterAsAutoGenerate",
                false);

        }

        void SaveFontForMarcEditor()
        {
            MainForm.AppInfo.SetString("marceditor",
                "fontface",
                this.MarcEditor.Font.FontFamily.Name);
            MainForm.AppInfo.SetString("marceditor",
                "fontsize",
                Convert.ToString(this.MarcEditor.Font.Size));

            //            string strStyle = Enum.GetName(typeof(FontStyle), this.MarcEditor.Font.Style);
            string strStyle = this.MarcEditor.Font.Style.ToString();


            MainForm.AppInfo.SetString("marceditor",
                "fontstyle",
                strStyle);


            MainForm.AppInfo.SetString("marceditor",
                "fontcolor",
                this.MarcEditor.ContentTextColor != MarcEditor.DefaultBackColor ? ColorUtil.Color2String(this.MarcEditor.ContentTextColor) : "");


        }

        int m_nInGetCfgFile = 0;    // 防止GetCfgFile()函数重入 2008/3/6

        // marc编辑窗要从外部获得配置文件内容
        private void MarcEditor_GetConfigFile(object sender,
            DigitalPlatform.Marc.GetConfigFileEventArgs e)
        {

            if (m_nInGetCfgFile > 0)
            {
                e.ErrorInfo = "MarcEditor_GetConfigFile() 重入了";
                return;
            }

            if (String.IsNullOrEmpty(textBox_recPath.Text))
            {
                e.ErrorInfo = "URL为空";
                return;
            }

            // 下载配置文件
            ResPath respath = new ResPath(textBox_recPath.Text);

            string strCfgFileName = e.Path;
            int nRet = strCfgFileName.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFileName = strCfgFileName.Substring(0, nRet);
            }

            string strPath = ResPath.GetDbName(respath.Path) + "/cfgs/" + strCfgFileName;

            // 使用Channel

            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            m_nInGetCfgFile++;

            try
            {

                string strContent;
                string strError;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strPath);
                stop.BeginLoop();

                byte[] baTimeStamp = null;
                string strMetaData;
                string strOutputPath;

                long lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strPath,
                    out strContent,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");


                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        e.ErrorInfo = "";
                        return;
                    }


                    e.ErrorInfo = "获得配置文件 '" + strPath + "' 时出错：" + strError;
                    return;
                }
                else
                {
                    byte[] baContent = StringUtil.GetUtf8Bytes(strContent, true);
                    MemoryStream stream = new MemoryStream(baContent);
                    e.Stream = stream;
                }


            }
            finally
            {
                channel = channelSave;

                m_nInGetCfgFile--;
            }
        }

        private void MarcEditor_GetConfigDom(object sender, GetConfigDomEventArgs e)
        {
            if (String.IsNullOrEmpty(textBox_recPath.Text) == true)
            {
                e.ErrorInfo = "记录路径为空，无法获得配置文件 '" + e.Path + "'";
                return;
            }
            ResPath respath = new ResPath(textBox_recPath.Text);

            // 得到干净的文件名
            string strCfgFileName = e.Path;
            int nRet = strCfgFileName.IndexOf("#");
            if (nRet != -1)
            {
                strCfgFileName = strCfgFileName.Substring(0, nRet);
            }

            string strPath = ResPath.GetDbName(respath.Path) + "/cfgs/" + strCfgFileName;

            // 在cache中寻找
            e.XmlDocument = this.MainForm.DomCache.FindObject(strPath);
            if (e.XmlDocument != null)
                return;

            // 使用Channel

            RmsChannel channelSave = channel;

            channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel 异常");

            m_nInGetCfgFile++;

            try
            {
                string strContent;
                string strError;

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在下载文件" + strPath);
                stop.BeginLoop();

                byte[] baTimeStamp = null;
                string strMetaData;
                string strOutputPath;

                long lRet = channel.GetRes(
                    MainForm.cfgCache,
                    strPath,
                    out strContent,
                    out strMetaData,
                    out baTimeStamp,
                    out strOutputPath,
                    out strError);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        e.ErrorInfo = "";
                        return;
                    }


                    e.ErrorInfo = "获得配置文件 '" + strPath + "' 时出错：" + strError;
                    return;
                }


                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strContent);
                }
                catch (Exception ex)
                {
                    e.ErrorInfo = "配置文件 '" + strPath + "' 装入XMLDUM时出错: " + ex.Message;
                    return;
                }
                e.XmlDocument = dom;
                this.MainForm.DomCache.SetObject(strPath, dom);  // 保存到缓存
            }
            finally
            {
                channel = channelSave;

                m_nInGetCfgFile--;
            }
        }

        private void textBox_xmlPureText_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nChangeTextNest != 0)
                return;

            string strError = "";
            int nRet = 0;

            nRet = PutTextChangedInfoToQueue(this.textBox_xmlPureText,
                    out strError);
            if (nRet == -1)
                goto ERROR1;

            this.m_bPureXmlChanged = true;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;

        }

    }


    // 加拼音时的大小写风格
    public enum PinyinStyle
    {
        None = 0,	// 不做任何改变
        Upper = 1,	// 全部大写
        Lower = 2,	// 全部小写
        UpperFirst = 3,	// 首字母大写,其它小写
    }
}
