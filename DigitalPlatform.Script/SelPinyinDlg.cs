using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// Summary description for SelPinyinDlg.
    /// </summary>
    public class SelPinyinDlg : System.Windows.Forms.Form
    {
        static string SelectedChar = "⁞";

        const int WM_FIRST_SETFOCUS = API.WM_USER + 200;

        public string SampleText = "";
        public int Offset = -1;	// Hanzi这个汉字在SampleText中所在的偏移
        public string Pinyins = "";

        public string ActivePinyin = "";    // 列表中应该被首先选择的拼音

        public string Hanzi = "";
        public string ResultPinyin = "";
        public TextBox textBox_sampleText;
        private System.Windows.Forms.Label label_largeHanzi;
        public ListBox listBox_multiPinyin;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private Button button_stop;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public SelPinyinDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelPinyinDlg));
            this.textBox_sampleText = new System.Windows.Forms.TextBox();
            this.label_largeHanzi = new System.Windows.Forms.Label();
            this.listBox_multiPinyin = new System.Windows.Forms.ListBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_sampleText
            // 
            this.textBox_sampleText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sampleText.Font = new System.Drawing.Font("宋体", 16F);
            this.textBox_sampleText.Location = new System.Drawing.Point(9, 9);
            this.textBox_sampleText.Name = "textBox_sampleText";
            this.textBox_sampleText.ReadOnly = true;
            this.textBox_sampleText.Size = new System.Drawing.Size(472, 32);
            this.textBox_sampleText.TabIndex = 0;
            this.textBox_sampleText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_largeHanzi
            // 
            this.label_largeHanzi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_largeHanzi.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_largeHanzi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_largeHanzi.Font = new System.Drawing.Font("微软雅黑", 90F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_largeHanzi.Location = new System.Drawing.Point(9, 44);
            this.label_largeHanzi.Name = "label_largeHanzi";
            this.label_largeHanzi.Size = new System.Drawing.Size(307, 170);
            this.label_largeHanzi.TabIndex = 1;
            this.label_largeHanzi.Text = "汉";
            this.label_largeHanzi.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_largeHanzi.SizeChanged += new System.EventHandler(this.label_largeHanzi_SizeChanged);
            // 
            // listBox_multiPinyin
            // 
            this.listBox_multiPinyin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox_multiPinyin.Font = new System.Drawing.Font("宋体", 18F);
            this.listBox_multiPinyin.IntegralHeight = false;
            this.listBox_multiPinyin.ItemHeight = 24;
            this.listBox_multiPinyin.Location = new System.Drawing.Point(322, 44);
            this.listBox_multiPinyin.Name = "listBox_multiPinyin";
            this.listBox_multiPinyin.Size = new System.Drawing.Size(156, 112);
            this.listBox_multiPinyin.TabIndex = 2;
            this.listBox_multiPinyin.DoubleClick += new System.EventHandler(this.listBox_multiPinyin_DoubleClick);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(403, 162);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 52);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(322, 191);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Location = new System.Drawing.Point(322, 162);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(75, 23);
            this.button_stop.TabIndex = 3;
            this.button_stop.Text = "停止(&S)";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // SelPinyinDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(490, 226);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.listBox_multiPinyin);
            this.Controls.Add(this.label_largeHanzi);
            this.Controls.Add(this.textBox_sampleText);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SelPinyinDlg";
            this.ShowInTaskbar = false;
            this.Text = "选择拼音";
            this.Load += new System.EventHandler(this.SelPinyinDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void SelPinyinDlg_Load(object sender, System.EventArgs e)
        {
            this.label_largeHanzi.Font = new Font("Arial",
                Math.Min(label_largeHanzi.Width,
                label_largeHanzi.Height) - 8,
                GraphicsUnit.Pixel);

            this.textBox_sampleText.Text = MakeShorterSampleText(this.SampleText,
                this.Offset);
            this.label_largeHanzi.Text = Hanzi;

            FillList();

            API.PostMessage(this.Handle, WM_FIRST_SETFOCUS, 0, 0);
            // this.listBox1.Focus();
        }

        void FillList()
        {
            listBox_multiPinyin.Items.Clear();

            if (Pinyins == "")
                return;

            int nActiveIndex = -1;
            string[] aPart = Pinyins.Split(new char[] { ';' });
            for (int i = 0; i < aPart.Length; i++)
            {
                string strOnePinyin = aPart[i].Trim();
                if (strOnePinyin == "")
                    continue;
                listBox_multiPinyin.Items.Add(strOnePinyin);
                if (strOnePinyin == this.ActivePinyin)
                    nActiveIndex = i;
            }

            if (nActiveIndex != -1)
            {
                listBox_multiPinyin.SelectedIndex = nActiveIndex;
                listBox_multiPinyin.Items[nActiveIndex] = SelectedChar + listBox_multiPinyin.Items[nActiveIndex];

                this.label_largeHanzi.BackColor = Color.LightGreen;
            }
            else
                listBox_multiPinyin.SelectedIndex = 0;
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            if (listBox_multiPinyin.SelectedIndex == -1)
            {
                MessageBox.Show(this, "尚未选择事项...");
                return;
            }

            this.ResultPinyin = ((string)listBox_multiPinyin.Items[listBox_multiPinyin.SelectedIndex]).Replace(SelectedChar, "");

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

#if NO
        public string FirstPinyin
        {
            get
            {
                if (this.listBox_multiPinyin.Items.Count == 0)
                    return "";

                return (listBox_multiPinyin.Items[0] as string).Replace(SelectedChar, "");

            }
        }
#endif

        // 获得一系列拼音的第一个
        // parameters:
        //      strPinyins  例如 ce;ci
        public static string GetFirstPinyin(string strPinyins)
        {
            if (string.IsNullOrEmpty(strPinyins) == true)
                return "";

            string[] aPart = strPinyins.Split(new char[] { ';' });
            return aPart[0].Trim();
        }

        private void label_largeHanzi_SizeChanged(object sender, System.EventArgs e)
        {
            float fFontSize = Math.Min(label_largeHanzi.Width,
                label_largeHanzi.Height - (label_largeHanzi.Height / 4));
            fFontSize = Math.Max(fFontSize, 8);
            this.label_largeHanzi.Font = new Font(this.Font.FontFamily,
                fFontSize,
                GraphicsUnit.Pixel);
        }

        string MakeShorterSampleText(string strText,
            int nOffset)
        {
            if (strText == "")
                return "";
            int nHalf = 20;

            if (nOffset == -1)
                return strText;

            int nLeft = nOffset;
            int nRight = strText.Length - nOffset - 1;
            int nCenter = nOffset;

            strText = strText.Insert(nCenter + 1, " › ");
            strText = strText.Insert(nCenter, " ‹ ");

            nCenter++;
            nLeft++;
            nRight++;

            bool bLeftTruncated = false;
            bool bRightTruncated = false;
            if (nLeft > nHalf)
            {
                strText = strText.Remove(0, nLeft - nHalf);
                nCenter -= nLeft - nHalf;
                bLeftTruncated = true;
            }

            if (nRight > nHalf)
            {
                strText = strText.Substring(0, nCenter + nHalf);
                bRightTruncated = true;
            }

            if (bLeftTruncated == true)
                strText = "... " + strText;
            if (bRightTruncated == true)
                strText = strText + " ...";

            return strText;
        }

        private void listBox_multiPinyin_DoubleClick(object sender, System.EventArgs e)
        {
            button_OK_Click(null, null);
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_FIRST_SETFOCUS:
                    this.listBox_multiPinyin.Focus();
                    return;
            }
            base.DefWndProc(ref m);
        }

        // 在字符串后面接续一个拼音
        public static void AppendPinyin(ref string strText,
            string strPinyin)
        {
            if (string.IsNullOrEmpty(strPinyin) == true)
                return;

            if (string.IsNullOrEmpty(strText) == true)
            {
                strText = strPinyin;
                return;
            }

            if (strText[strText.Length - 1] == ' ')
            {
                strText += strPinyin;
                return;
            }
            strText += " " + strPinyin;
        }

        // 在字符串后面接续一段未知的文字
        public static void AppendText(ref string strText,
            string strAppendText)
        {
            if (string.IsNullOrEmpty(strAppendText) == true)
                return;

            if (string.IsNullOrEmpty(strText) == true)
            {
                strText = strAppendText;
                return;
            }

            if (strText[strText.Length - 1] == ' ')
            {
                strText += strAppendText;
                return;
            }

            if (strAppendText[0] == ' ')
                strText += strAppendText;
            else
                strText += " " + strAppendText;
        }

        public static string ConvertSinglePinyinByStyle(string strPinyin,
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

        // 获得一段样本文字和焦点字符的定位
        public static void GetOffs(XmlNode root,
            XmlNode nodeFocusChar,
            out string strText,
            out int nOffs)
        {
            strText = "";
            nOffs = -1;
            foreach (XmlNode nodeWord in root.ChildNodes)
            {
                if (nodeWord.NodeType == XmlNodeType.Text)
                {
                    strText += nodeWord.InnerText;
                    continue;
                }

                if (nodeWord.NodeType != XmlNodeType.Element)
                    continue;

                // 让选择多音字
                foreach (XmlNode nodeChar in nodeWord.ChildNodes)
                {
                    if (nodeChar.NodeType == XmlNodeType.Text)
                    {
                        strText += nodeChar.InnerText;
                        continue;
                    }

                    if (nodeWord.NodeType != XmlNodeType.Element)
                        continue;

                    if (nodeChar == nodeFocusChar)
                        nOffs = strText.Length;
                    strText += nodeChar.InnerText;
                }
            }
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
