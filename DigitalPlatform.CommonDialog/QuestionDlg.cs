using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Generic;

using DigitalPlatform.Text;

namespace DigitalPlatform.CommonDialog
{

    /// <summary>
    /// 提问/回答 对话框。用于 GCAT 前端回答 API 返回的问题
    /// </summary>
    public class QuestionDlg : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label_messageTitle;
        private System.Windows.Forms.TextBox textBox_question;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_result;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private Label label_pinyin;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public QuestionDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuestionDlg));
            this.label_messageTitle = new System.Windows.Forms.Label();
            this.textBox_question = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_result = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label_pinyin = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_messageTitle
            // 
            this.label_messageTitle.AutoSize = true;
            this.label_messageTitle.Location = new System.Drawing.Point(12, 10);
            this.label_messageTitle.Name = "label_messageTitle";
            this.label_messageTitle.Size = new System.Drawing.Size(53, 18);
            this.label_messageTitle.TabIndex = 4;
            this.label_messageTitle.Text = "text:";
            // 
            // textBox_question
            // 
            this.textBox_question.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_question.Location = new System.Drawing.Point(15, 32);
            this.textBox_question.MaxLength = 0;
            this.textBox_question.Multiline = true;
            this.textBox_question.Name = "textBox_question";
            this.textBox_question.ReadOnly = true;
            this.textBox_question.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_question.Size = new System.Drawing.Size(470, 199);
            this.textBox_question.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 273);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "答案(&A):";
            // 
            // textBox_result
            // 
            this.textBox_result.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_result.Location = new System.Drawing.Point(147, 269);
            this.textBox_result.Name = "textBox_result";
            this.textBox_result.Size = new System.Drawing.Size(338, 28);
            this.textBox_result.TabIndex = 1;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(229, 305);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(123, 32);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(360, 305);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(125, 32);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label_pinyin
            // 
            this.label_pinyin.AutoSize = true;
            this.label_pinyin.Location = new System.Drawing.Point(12, 234);
            this.label_pinyin.Name = "label_pinyin";
            this.label_pinyin.Size = new System.Drawing.Size(0, 18);
            this.label_pinyin.TabIndex = 6;
            // 
            // QuestionDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 21);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(500, 353);
            this.Controls.Add(this.label_pinyin);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_result);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_question);
            this.Controls.Add(this.label_messageTitle);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QuestionDlg";
            this.ShowInTaskbar = false;
            this.Text = "创建著者号 - 请回答提问";
            this.Load += new System.EventHandler(this.QuestionDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            if (textBox_result.Text == "")
            {
                MessageBox.Show(this, "尚未输入答案");
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

        private void QuestionDlg_Load(object sender, System.EventArgs e)
        {
            this.AcceptButton = this.button_OK;
            this.textBox_result.Focus();

            if (string.IsNullOrEmpty(this.Xml) == false
                && this.HanziPinyinTable != null && this.HanziPinyinTable.Count > 0)
            {
                TryAutoAnswer();
            }
        }

        void TryAutoAnswer()
        {
            // 建立汉字和拼音对照表

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(this.Xml);

            XmlElement hanzi = dom.DocumentElement.SelectSingleNode("hanzi") as XmlElement;
            if (hanzi == null)
                return;

            string strHanzi = hanzi.InnerText.Trim();

            // 在 “汉字拼音对照表”中找到这个汉字
            HanziPinyin hanzi_pinyin = this.HanziPinyinTable.Find((o) =>
            {
                if (o.Hanzi == strHanzi)
                    return true;
                return false;
            });

            if (hanzi_pinyin == null)
                return;

            // 匹配汉字拼音
            List<int> match_results = new List<int>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("pinyin");
            int i = 1;
            foreach (XmlElement pinyin in nodes)
            {
                string strPinyin = pinyin.InnerText.Trim();
                if (IsEqual(hanzi_pinyin.Pinyin, strPinyin))
                    match_results.Add(i);
                i++;
            }

            if (match_results.Count != 1)
                return;

            this.textBox_result.Text = match_results[0].ToString();
            button_OK_Click(null, null);
        }

        static bool IsEqual(string pinyin1, string pinyin2)
        {
            pinyin1 = pinyin1.ToLower();
            pinyin2 = pinyin2.ToLower();
            return string.CompareOrdinal(pinyin1, pinyin2) == 0;
        }

        public string Xml { get; set; }

        public string Question
        {
            get
            {
                return this.textBox_question.Text;
            }
            set
            {
                this.textBox_question.Text = value;
            }
        }

        public string Result
        {
            get
            {
                return this.textBox_result.Text;
            }
            set
            {
                this.textBox_result.Text = value;
            }
        }

        public string MessageTitle
        {
            get
            {
                return this.label_messageTitle.Text;
            }
            set
            {
                this.label_messageTitle.Text = value;
            }
        }

#if NO
        public string Pinyin
        {
            get
            {
                return this.label_pinyin.Text;
            }
            set
            {
                this.label_pinyin.Text = value;
            }
        }
#endif
        public List<HanziPinyin> HanziPinyinTable { get; set; }

        public static List<HanziPinyin> BuildHanziPinyinTable(string hanzi, string pinyin)
        {
            hanzi = hanzi.Trim();
            pinyin = pinyin.Trim();

            List<HanziPinyin> results = new List<HanziPinyin>();
            List<string> pinyin_list = StringUtil.SplitList(pinyin, ' ');
            int i = 0;
            foreach (char ch in hanzi)
            {
                string one_pinyin = "";
                if (i < pinyin_list.Count)
                    one_pinyin = pinyin_list[i];
                else
                    break;
                results.Add(new HanziPinyin { Hanzi = new string(ch, 1), Pinyin = one_pinyin });
                i++;
            }

            return results;
        }
    }

    public class HanziPinyin
    {
        public string Hanzi { get; set; }
        public string Pinyin { get; set; }
    }
}
