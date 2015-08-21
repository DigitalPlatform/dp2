using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;

using DigitalPlatform.Text;
using System.IO;
using DigitalPlatform.GUI;

namespace DigitalPlatform.Marc
{
	/// <summary>
	/// 打开或者保存 ISO2709 文件的通用对话框
	/// </summary>
    public class OpenMarcFileDlg : System.Windows.Forms.Form
    {
        /// <summary>
        /// 获得编码方式的事件
        /// </summary>
        public event GetEncodingEventHandler GetEncoding = null;

        bool m_bIsOutput = false;
        /// <summary>
        /// 是否为输出文件方式。当保存文件的时候应该为 true
        /// </summary>
        public bool IsOutput
        {
            get
            {
                return this.m_bIsOutput;
            }
            set
            {
                this.m_bIsOutput = value;
                if (value == true)
                {
                    // 改为用EnableMarcSyntax成员控制
                    // this.comboBox_marcSyntax.Enabled = false;
                    this.checkBox_880.Text = "转换为 880 模式(&C)";
                }
                else
                {
                    this.checkBox_880.Text = "转换为平行模式(&C)";

                    this.checkBox_crLf.Visible = false;
                    this.checkBox_addG01Field.Visible = false;
                    this.checkBox_removeField998.Visible = false;
                }
            }
        }
        // public bool DisableOutputMarcSyntax = true; 

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_filename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_encoding;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_marcSyntax;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_findFileName;
        private System.Windows.Forms.CheckBox checkBox_crLf;
        private Label label_encodingComment;
        private CheckBox checkBox_addG01Field;
        private ComboBox comboBox_catalogingRule;
        private Label label_catalogingRule;
        private WebBrowser webBrowser1;
        private CheckBox checkBox_removeField998;
        private CheckBox checkBox_880;
        private Panel panel_main;
        private IContainer components;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OpenMarcFileDlg()
        {
            InitializeComponent();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OpenMarcFileDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_filename = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_encoding = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_marcSyntax = new System.Windows.Forms.ComboBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_findFileName = new System.Windows.Forms.Button();
            this.checkBox_crLf = new System.Windows.Forms.CheckBox();
            this.label_encodingComment = new System.Windows.Forms.Label();
            this.checkBox_addG01Field = new System.Windows.Forms.CheckBox();
            this.comboBox_catalogingRule = new System.Windows.Forms.ComboBox();
            this.label_catalogingRule = new System.Windows.Forms.Label();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.checkBox_removeField998 = new System.Windows.Forms.CheckBox();
            this.checkBox_880 = new System.Windows.Forms.CheckBox();
            this.panel_main = new System.Windows.Forms.Panel();
            this.panel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "MARC (ISO2709格式) 文件名(&F):";
            // 
            // textBox_filename
            // 
            this.textBox_filename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_filename.Location = new System.Drawing.Point(0, 15);
            this.textBox_filename.Name = "textBox_filename";
            this.textBox_filename.Size = new System.Drawing.Size(462, 21);
            this.textBox_filename.TabIndex = 1;
            this.textBox_filename.TextChanged += new System.EventHandler(this.textBox_filename_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "编码方式(&E):";
            // 
            // comboBox_encoding
            // 
            this.comboBox_encoding.Items.AddRange(new object[] {
            "GB2312",
            "UTF-8",
            "UTF-16"});
            this.comboBox_encoding.Location = new System.Drawing.Point(94, 72);
            this.comboBox_encoding.Name = "comboBox_encoding";
            this.comboBox_encoding.Size = new System.Drawing.Size(168, 20);
            this.comboBox_encoding.TabIndex = 6;
            this.comboBox_encoding.Text = "GB2312";
            this.comboBox_encoding.TextChanged += new System.EventHandler(this.comboBox_encoding_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "MARC格式(&S):";
            // 
            // comboBox_marcSyntax
            // 
            this.comboBox_marcSyntax.Items.AddRange(new object[] {
            "UNIMARC",
            "USMARC",
            "<自动>"});
            this.comboBox_marcSyntax.Location = new System.Drawing.Point(94, 49);
            this.comboBox_marcSyntax.Name = "comboBox_marcSyntax";
            this.comboBox_marcSyntax.Size = new System.Drawing.Size(168, 20);
            this.comboBox_marcSyntax.TabIndex = 4;
            this.comboBox_marcSyntax.Text = "UNIMARC";
            this.comboBox_marcSyntax.TextChanged += new System.EventHandler(this.comboBox_marcSyntax_TextChanged);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(349, 306);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 14;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(430, 306);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 15;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_findFileName
            // 
            this.button_findFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findFileName.Location = new System.Drawing.Point(461, 13);
            this.button_findFileName.Name = "button_findFileName";
            this.button_findFileName.Size = new System.Drawing.Size(33, 22);
            this.button_findFileName.TabIndex = 2;
            this.button_findFileName.Text = "...";
            this.button_findFileName.Click += new System.EventHandler(this.button_findFileName_Click);
            // 
            // checkBox_crLf
            // 
            this.checkBox_crLf.AutoSize = true;
            this.checkBox_crLf.Location = new System.Drawing.Point(0, 198);
            this.checkBox_crLf.Name = "checkBox_crLf";
            this.checkBox_crLf.Size = new System.Drawing.Size(210, 16);
            this.checkBox_crLf.TabIndex = 12;
            this.checkBox_crLf.Text = "在每条记录后添加回车换行符号(&C)";
            // 
            // label_encodingComment
            // 
            this.label_encodingComment.Location = new System.Drawing.Point(92, 92);
            this.label_encodingComment.Name = "label_encodingComment";
            this.label_encodingComment.Size = new System.Drawing.Size(170, 22);
            this.label_encodingComment.TabIndex = 7;
            // 
            // checkBox_addG01Field
            // 
            this.checkBox_addG01Field.AutoSize = true;
            this.checkBox_addG01Field.Location = new System.Drawing.Point(0, 220);
            this.checkBox_addG01Field.Name = "checkBox_addG01Field";
            this.checkBox_addG01Field.Size = new System.Drawing.Size(108, 16);
            this.checkBox_addG01Field.TabIndex = 13;
            this.checkBox_addG01Field.Text = "加入-01字段(&G)";
            this.checkBox_addG01Field.UseVisualStyleBackColor = true;
            // 
            // comboBox_catalogingRule
            // 
            this.comboBox_catalogingRule.Items.AddRange(new object[] {
            "<无限制>",
            "NLC",
            "CALIS"});
            this.comboBox_catalogingRule.Location = new System.Drawing.Point(94, 117);
            this.comboBox_catalogingRule.Name = "comboBox_catalogingRule";
            this.comboBox_catalogingRule.Size = new System.Drawing.Size(168, 20);
            this.comboBox_catalogingRule.TabIndex = 9;
            this.comboBox_catalogingRule.Text = "<无限制>";
            this.comboBox_catalogingRule.Visible = false;
            // 
            // label_catalogingRule
            // 
            this.label_catalogingRule.AutoSize = true;
            this.label_catalogingRule.Location = new System.Drawing.Point(0, 120);
            this.label_catalogingRule.Name = "label_catalogingRule";
            this.label_catalogingRule.Size = new System.Drawing.Size(77, 12);
            this.label_catalogingRule.TabIndex = 8;
            this.label_catalogingRule.Text = "编目规则(&R):";
            this.label_catalogingRule.Visible = false;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser1.Location = new System.Drawing.Point(282, 49);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(212, 236);
            this.webBrowser1.TabIndex = 13;
            // 
            // checkBox_removeField998
            // 
            this.checkBox_removeField998.AutoSize = true;
            this.checkBox_removeField998.Location = new System.Drawing.Point(0, 154);
            this.checkBox_removeField998.Name = "checkBox_removeField998";
            this.checkBox_removeField998.Size = new System.Drawing.Size(120, 16);
            this.checkBox_removeField998.TabIndex = 10;
            this.checkBox_removeField998.Text = "删除 998 字段(&R)";
            this.checkBox_removeField998.UseVisualStyleBackColor = true;
            // 
            // checkBox_880
            // 
            this.checkBox_880.AutoSize = true;
            this.checkBox_880.Location = new System.Drawing.Point(0, 176);
            this.checkBox_880.Name = "checkBox_880";
            this.checkBox_880.Size = new System.Drawing.Size(132, 16);
            this.checkBox_880.TabIndex = 11;
            this.checkBox_880.Text = "转换为 998 模式(&C)";
            this.checkBox_880.UseVisualStyleBackColor = true;
            this.checkBox_880.CheckedChanged += new System.EventHandler(this.checkBox_880_CheckedChanged);
            // 
            // panel_main
            // 
            this.panel_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_main.Controls.Add(this.textBox_filename);
            this.panel_main.Controls.Add(this.checkBox_880);
            this.panel_main.Controls.Add(this.label1);
            this.panel_main.Controls.Add(this.checkBox_removeField998);
            this.panel_main.Controls.Add(this.button_findFileName);
            this.panel_main.Controls.Add(this.webBrowser1);
            this.panel_main.Controls.Add(this.checkBox_crLf);
            this.panel_main.Controls.Add(this.comboBox_catalogingRule);
            this.panel_main.Controls.Add(this.label2);
            this.panel_main.Controls.Add(this.label_catalogingRule);
            this.panel_main.Controls.Add(this.comboBox_encoding);
            this.panel_main.Controls.Add(this.checkBox_addG01Field);
            this.panel_main.Controls.Add(this.label3);
            this.panel_main.Controls.Add(this.label_encodingComment);
            this.panel_main.Controls.Add(this.comboBox_marcSyntax);
            this.panel_main.Location = new System.Drawing.Point(12, 12);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(494, 288);
            this.panel_main.TabIndex = 16;
            // 
            // OpenMarcFileDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(518, 341);
            this.Controls.Add(this.panel_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OpenMarcFileDlg";
            this.ShowInTaskbar = false;
            this.Text = "请指定MARC文件名";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OpenMarcFileDlg_FormClosed);
            this.Load += new System.EventHandler(this.OpenMarcFileDlg_Load);
            this.panel_main.ResumeLayout(false);
            this.panel_main.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(this.FileName) == true)
            {
                MessageBox.Show(this, "尚未指定 MARC 文件名");
                return;
            }

            if (String.IsNullOrEmpty(this.comboBox_encoding.Text) == true)
            {
                MessageBox.Show(this, "尚未指定编码方式");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void OpenMarcFileDlg_Load(object sender, System.EventArgs e)
        {
#if NO
            if (IsOutput == true)
            {
                // 改为用EnableMarcSyntax成员控制
                // this.comboBox_marcSyntax.Enabled = false;
            }
            else
            {
                this.checkBox_crLf.Visible = false;
                this.checkBox_addG01Field.Visible = false;
                this.checkBox_removeField998.Visible = false;
            }
#endif

            this.BeginInvoke(new Delegate_Initial(_initial));

            comboBox_marcSyntax_TextChanged(this, null);
        }

        private void OpenMarcFileDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            HideMessageTip();
        }

        /*public*/ delegate void Delegate_Initial();

        void _initial()
        {
            if (this.IsOutput == false
                && string.IsNullOrEmpty(this.FileName) == false)
            {
                HideMessageTip();
                ShowMessageTip();
            }
        }

        private void button_findFileName_Click(object sender, System.EventArgs e)
        {
            if (this.IsOutput == true)
            {
                // 获得输出文件名
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.Title = "请指定要输出的 MARC(ISO2709格式) 文件名";
                dlg.CreatePrompt = false;
                dlg.OverwritePrompt = false;
                dlg.FileName = this.textBox_filename.Text;

                dlg.Filter = "MARC (ISO2709) 文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";

                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                this.textBox_filename.Text = dlg.FileName;

            }
            else
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要导入的 MARC(ISO2709格式) 文件名";
                dlg.FileName = this.textBox_filename.Text;

                dlg.Filter = "ISO2709 文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                this.textBox_filename.Text = dlg.FileName;
            }
        }

        /// <summary>
        /// 获取或设置文件名全路径
        /// </summary>
        public string FileName
        {
            get
            {
                return this.textBox_filename.Text;
            }
            set
            {
                this.textBox_filename.Text = value;

                // 自动显示文件的第一条
                DisplayFirstRecord(value, this.Encoding);
            }
        }

        /// <summary>
        /// 获取或设置 MARC 格式。为 "unimarc" "usmarc" 之一
        /// </summary>
        public string MarcSyntax
        {
            get
            {
                return this.comboBox_marcSyntax.Text.ToLower();
            }
            set
            {
                this.comboBox_marcSyntax.Text = value.ToUpper();
            }
        }

        /// <summary>
        /// 获得编码方式
        /// </summary>
        public Encoding Encoding
        {
            get
            {
                // 2014/3/10
                if (string.IsNullOrEmpty(this.comboBox_encoding.Text) == true)
                    return null;

                if (StringUtil.IsNumber(this.comboBox_encoding.Text) == true)
                    return Encoding.GetEncoding(Convert.ToInt32(this.comboBox_encoding.Text));

                if (this.GetEncoding != null)
                {
                    GetEncodingEventArgs e = new GetEncodingEventArgs();
                    e.EncodingName = this.comboBox_encoding.Text;

                    this.GetEncoding(this, e);

                    if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                        throw new Exception(e.ErrorInfo);

                    return e.Encoding;
                }
                else
                    return Encoding.GetEncoding(this.comboBox_encoding.Text);
            }
        }

        /// <summary>
        /// 获得或设置编码方式名
        /// </summary>
        public string EncodingName
        {
            get
            {
                return this.comboBox_encoding.Text;
            }
            set
            {
                this.comboBox_encoding.Text = value;

                // 自动显示文件的第一条
                try
                {
                    DisplayFirstRecord(this.FileName, this.Encoding);
                }
                catch
                {
                    this.ClearHtml();
                }
            }
        }

        /// <summary>
        /// “在每条记录后添加回车换行符号” checkbox 是否可见
        /// </summary>
        public bool CrLfVisible
        {
            get
            {
                return this.checkBox_crLf.Visible;
            }
            set
            {
                this.checkBox_crLf.Visible = value;
            }
        }

        /// <summary>
        /// 输出文件时，是否要在记录之间加入回车换行符号。
        /// 加入回车换行符号这一做法是北图发行 MARC 数据磁盘时候的一种做法，但并不是标准的做法
        /// </summary>
        public bool CrLf
        {
            get
            {
                return this.checkBox_crLf.Checked;
            }
            set
            {
                this.checkBox_crLf.Checked = value;
            }
        }

        /// <summary>
        /// “加入 -01 字段” checkbox 是否可见
        /// </summary>
        public bool AddG01Visible
        {
            get
            {
                return this.checkBox_addG01Field.Visible;
            }
            set
            {
                this.checkBox_addG01Field.Visible = value;
            }
        }

        // 2008/11/10
        /// <summary>
        /// 输出文件时，是否加入 -01 字段
        /// </summary>
        public bool AddG01
        {
            get
            {
                return this.checkBox_addG01Field.Checked;
            }
            set
            {
                this.checkBox_addG01Field.Checked = value;
            }
        }

        /// <summary>
        /// “删除 998 字段” checkbox 是否可见
        /// </summary>
        public bool RemoveField998Visible
        {
            get
            {
                return this.checkBox_removeField998.Visible;
            }
            set
            {
                this.checkBox_removeField998.Visible = value;
            }
        }

        /// <summary>
        /// 输出文件时，是否要从 MARC 记录中删除 998 字段
        /// </summary>
        public bool RemoveField998
        {
            get
            {
                return this.checkBox_removeField998.Checked;
            }
            set
            {
                this.checkBox_removeField998.Checked = value;
            }
        }

        /// <summary>
        /// “编目规则” combobox 及标签是否可见
        /// </summary>
        public bool RuleVisible
        {
            get
            {
                return this.comboBox_catalogingRule.Visible;
            }
            set
            {
                this.comboBox_catalogingRule.Visible = value;
                this.label_catalogingRule.Visible = value;
            }
        }

        /// <summary>
        /// 编目规则
        /// </summary>
        public string Rule
        {
            get
            {
                return this.comboBox_catalogingRule.Text;
            }
            set
            {
                this.comboBox_catalogingRule.Text = value;
            }
        }

        /// <summary>
        /// 所有可用的编码方式
        /// </summary>
        public List<string> EncodingListItems
        {
            get
            {
                List<string> result = new List<string>();
                for (int i = 0; i < this.comboBox_encoding.Items.Count; i++)
                {
                    result.Add((string)this.comboBox_encoding.Items[i]);
                }

                return result;
            }
            set
            {
                this.comboBox_encoding.Items.Clear();
                for (int i = 0; i < value.Count; i++)
                {
                    this.comboBox_encoding.Items.Add(value[i]);
                }
            }

        }

        // 是否需要输出的时候允许用户选择marcsyntax list。
        // 如果设置为禁止，这就意味着程序会自动设置好其中的值(虽然为disable状态)，用户也能看到，但是不能选择。
        // dp2可以识别marc syntax，而dt1000/dp1不能。后者需要Enable这个list，以便让用户加以选择
        /// <summary>
        /// “MARC 格式” ComboBox 的 Enabled 状态
        /// </summary>
        public bool EnableMarcSyntax
        {
            get
            {
                return this.comboBox_marcSyntax.Enabled;
            }
            set
            {
                this.comboBox_marcSyntax.Enabled = value;
                this.comboBox_marcSyntax.Select(0, 0);
            }
        }

        // 编码方式注。可以用来注明原始的编码方式是什么。
        /// <summary>
        /// 编码方式注释文字
        /// </summary>
        public string EncodingComment
        {
            get
            {
                return this.label_encodingComment.Text;
            }
            set
            {
                this.label_encodingComment.Text = value;
            }
        }

        void DisplayFirstRecord(string strFileName,
            Encoding encoding)
        {
            if (string.IsNullOrEmpty(strFileName) == true)
                goto ERROR1;
            if (File.Exists(strFileName) == false)
                goto ERROR1;
            if (encoding == null)
                goto ERROR1;

            string strMARC = "";
            string strError = "";
                    // return:
        //      -1  出错
        //      0   正常
            int nRet = LoadFirstRecord(strFileName,
            encoding,
            out strMARC,
            out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.Mode880 == true
                && (this.comboBox_marcSyntax.Text == "USMARC" || this.comboBox_marcSyntax.Text == "<自动>"))
            {
                if (this.IsOutput == false)
                {
                    MarcRecord temp = new MarcRecord(strMARC);
                    MarcQuery.ToParallel(temp);
                    strMARC = temp.Text;
                }
            }

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

            string strHtml = "<html>" +
    strHead +
    "<body>" +
    MarcUtil.GetHtmlOfMarc(strMARC, false) +
    "</body></html>";

            AppendHtml(this.webBrowser1, strHtml, true);

            if (this.IsOutput == false && this.Visible == true)
            {
                HideMessageTip();
                ShowMessageTip();
            }
            return;
        ERROR1:
            ClearHtml();
        }

        void ClearHtml()
        {
            this.webBrowser1.DocumentText = "<html><body></body></html>";
        }

        /*public*/ static void AppendHtml(WebBrowser webBrowser,
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

        private MessageBalloon m_firstUseBalloon = null;
        bool m_bBalloonDisplayed = false;   // 是否已经显示过一次了

        /*public*/ void ShowMessageTip()
        {
            if (m_bBalloonDisplayed == true)
                return;

            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.webBrowser1;
            m_firstUseBalloon.Title = "如何确定 MARC 文件的编码方式?";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "\r\n这里显示了当前文件第一条 MARC 记录的内容，请仔细观察汉字内容部分是否正确。\r\n\r\n如果出现了乱码，可重选其他编码方式试试";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();

            m_bBalloonDisplayed = true;
        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;
        }

        // 从 ISO2709 文件中读出第一条记录。返回的是机内格式
        // return:
        //      -1  出错
        //      0   正常
        /*public*/ static int LoadFirstRecord(string strMarcFileName,
            Encoding encoding,
            out string strMARC,
            out string strError)
        {
            strError = "";
            strMARC = "";

            try
            {
                using (Stream stream = File.Open(strMarcFileName,
    FileMode.Open,
    FileAccess.Read,
    FileShare.ReadWrite))
                {
                    // 从ISO2709文件中读入一条MARC记录
                    // return:
                    //	-2	MARC格式错
                    //	-1	出错
                    //	0	正确
                    //	1	结束(当前返回的记录有效)
                    //	2	结束(当前返回的记录无效)
                    int nRet = MarcUtil.ReadMarcRecord(stream,
                        encoding,
                        true,	// bRemoveEndCrLf,
                        true,	// bForce,
                        out strMARC,
                        out strError);
                    if (nRet == 0 || nRet == 1)
                        return 0;   // 正常

                    strError = "读入MARC记录时出错: " + strError;
                    return -1;
                }
            }
            catch (Exception ex)
            {
                strError = "异常: " + ex.Message;
                return -1;
            }
        }

        private void textBox_filename_TextChanged(object sender, EventArgs e)
        {
            // 自动显示文件的第一条
            DisplayFirstRecord(this.FileName, this.Encoding);
        }

        private void comboBox_encoding_TextChanged(object sender, EventArgs e)
        {
            // 自动显示文件的第一条
            DisplayFirstRecord(this.FileName, this.Encoding);
        }

        private void comboBox_marcSyntax_TextChanged(object sender, EventArgs e)
        {
            if (this.comboBox_marcSyntax.Text == "USMARC"
                || this.comboBox_marcSyntax.Text == "<自动>")
                this.checkBox_880.Visible = this.m_bMode880Visible;
            else
                this.checkBox_880.Visible = false;
        }

        public bool Mode880
        {
            get
            {
                if (this.m_bMode880Visible == false)
                    return false;
                return this.checkBox_880.Checked;
            }
            set
            {
                this.checkBox_880.Checked = value;

            }
        }

        // 880 checkbox 是否要显示？除了这个变量，还要看 MarcSyntax
        bool m_bMode880Visible = true;
        public bool Mode880Visible
        {
            get
            {
                return this.m_bMode880Visible;
            }
            set
            {
                this.m_bMode880Visible = value;

                comboBox_marcSyntax_TextChanged(this, null);
            }
        }

        private void checkBox_880_CheckedChanged(object sender, EventArgs e)
        {
            // 自动显示文件的第一条
            DisplayFirstRecord(this.FileName, this.Encoding);
        }

        public Panel MainPanel
        {
            get
            {
                return this.panel_main;
            }
        }
    }

    /// <summary>
    /// 获得编码方式的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetEncodingEventHandler(object sender,
GetEncodingEventArgs e);

    /// <summary>
    /// 获得编码方式事件的参数
    /// </summary>
    public class GetEncodingEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 编码方式名
        /// </summary>
        public string EncodingName = "";    // [in]

        /// <summary>
        /// [out] 返回编码方式 Encoding 对象
        /// </summary>
        public Encoding Encoding = null;    // [out]

        /// <summary>
        /// [out] 返回出错信息。如果为空则表示没有错误
        /// </summary>
        public string ErrorInfo = "";   // [out]
    }
}
