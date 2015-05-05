using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace dp2Catalog
{
    public partial class AmazonSimpleQueryControl : UserControl
    {
        public AmazonSimpleQueryControl()
        {
            InitializeComponent();
        }

        public override bool Focused
        {
            get
            {
                if (this.textBox_word.Focused)
                    return true;
                if (this.tabComboBox_match.Focused)
                    return true;
                if (this.comboBox_from.Focused)
                    return true;
                return false;
                // return base.Focused;
            }
        }

        static string[] froms = new string[] {
            "题名\ttitle",
            "著者\tauthor",
            "出版者\tpublisher",
            "出版日期\tpubdate",
            "主题词\tsubject",
            "关键词\tkeywords",
            "语言\tlanguage",
            "装订\tbinding",
            "ISBN\tISBN",
            "EISBN\tEISBN",
            "ASIN\tASIN"};

        // 初始化
        public void Initial()
        {
            this.comboBox_from.Items.Clear();
            foreach (string s in froms)
            {
                this.comboBox_from.Items.Add(s);
            }

            if (string.IsNullOrEmpty(this.comboBox_from.Text) == true
                && froms != null && froms.Length > 0)
                this.comboBox_from.Text = froms[0];

            FillMatchStyles();
        }

        public string GetFromRight()
        {
            string strLeft = "";
            string strRight = "";
            AmazonQueryControl.ParseLeftRight(this.comboBox_from.Text,
                out strLeft,
                out strRight);
            if (string.IsNullOrEmpty(strRight) == false)
                return strRight;

            // 从 Items 中寻找
            string strText = GetLineText(this.comboBox_from, strLeft);
            if (strText == null)
                return null;

            AmazonQueryControl.ParseLeftRight(strText,
    out strLeft,
    out strRight);
            return strRight;
        }

        // 根据左侧文字匹配 Items 中整行文字，如果有匹配的行，返回整行文字
        static string GetLineText(TabComboBox combobox,
            string strLeft)
        {
            foreach (string s in combobox.Items)
            {
                if (StringUtil.HasHead(s, strLeft + "\t") == true)
                    return s;
            }

            return null;
        }

        public string GetMatchStyleRight()
        {
            string strLeft = "";
            string strRight = "";
            AmazonQueryControl.ParseLeftRight(this.tabComboBox_match.Text,
                out strLeft,
                out strRight);
            if (string.IsNullOrEmpty(strRight) == false)
                return strRight;
            // 从 Items 中寻找
            string strText = GetLineText(this.tabComboBox_match, strLeft);
            if (strText == null)
                return null;

            AmazonQueryControl.ParseLeftRight(strText,
    out strLeft,
    out strRight);
            return strRight;
        }

        void FillMatchStyles()
        {
            string[] matchs = null;

            // 保存已有的 Text 值
            string strOldText = this.tabComboBox_match.Text;

            this.tabComboBox_match.Items.Clear();

            string strFrom = this.GetFromRight();
            if (strFrom == "title")
            {
                matchs = new string[] {
            "默认\t[default]",
            "前方一致\t-begins",
            "单词前方一致\t-words-begin"};
            }
            else if (strFrom == "author")
            {
                matchs = new string[] {
            "默认\t[default]",
            "前方一致\t-begins",
            "精确一致\t-exact"};
            }
            else if (strFrom == "keywords")
            {
                matchs = new string[] {
            "默认\t[default]",
            "前方一致\t-begin"};
            }
            else if (strFrom == "subject")
            {
                matchs = new string[] {
            "默认\t[default]",
            "前方一致\t-begins",
            "单词前方一致\t-words-begin"};
            }
            else if (strFrom == "pubdate")
            {
                matchs = new string[] {
            "默认\t[default]",
            "以后\t:after",
            "正当\t:during"};
            }

            if (matchs == null)
            {
                matchs = new string[] {
            "默认\t[default]"};
            }

            foreach (string s in matchs)
            {
                this.tabComboBox_match.Items.Add(s);
            }

            if (string.IsNullOrEmpty(strOldText) == false)
            {
                string strLeft = AmazonQueryControl.GetLeft(strOldText);
                // 重新设置 Text 值
                string strFound = GetLineText(this.tabComboBox_match,
                    strLeft);
                if (string.IsNullOrEmpty(strFound) == false)
                {
                    if (strOldText == strFound)
                    {
                        // 不用修改
                    }
                    else
                    {
                        this.tabComboBox_match.Text = strLeft;  // 只要左边部分
                    }
                }
                else
                    this.tabComboBox_match.Text = "";   // Text 发现不在列表中，清空以避免问题
            }

            if (string.IsNullOrEmpty(this.tabComboBox_match.Text) == true
    && matchs != null && matchs.Length > 0)
                this.tabComboBox_match.Text = matchs[0];
        }

        // 构造检索式
        public int BuildQueryString(out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            if (string.IsNullOrEmpty(this.textBox_word.Text) == true)
            {
                strError = "尚未输入检索词";
                return -1;
            }

            string strFrom = this.GetFromRight();
            string strMatch = this.GetMatchStyleRight();

            if (strMatch == "[default]")
                strMatch = "";

            strText = strFrom + strMatch;
            if (strText.IndexOf(":") == -1)
                strText += ": ";
            
            strText += " " + this.textBox_word.Text;
            return 0;
        }

        private void comboBox_from_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillMatchStyles();
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        public string Word
        {
            get
            {
                return this.textBox_word.Text;
            }
            set
            {
                this.textBox_word.Text = value;
            }
        }

        public string From
        {
            get
            {
                return this.comboBox_from.Text;
            }
            set
            {
                this.comboBox_from.Text = value;
            }
        }

        public string MatchStyle
        {
            get
            {
                return this.tabComboBox_match.Text;
            }
            set
            {
                this.tabComboBox_match.Text = value;
            }
        }

        public string GetContentString(bool bIncludeWord)
        {
            return (bIncludeWord ? this.textBox_word.Text : "")
                + "^" + this.comboBox_from.Text + "^" + this.tabComboBox_match.Text;
        }

        public void SetContentString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return;

            string[] parts = strText.Split(new char[] {'^' });
            if (parts.Length > 0)
                this.textBox_word.Text = parts[0];
            if (parts.Length > 1)
                this.comboBox_from.Text = parts[1];
            if (parts.Length > 2)
                this.tabComboBox_match.Text = parts[2];
        }

        bool m_bWordVisible = true;
        public bool WordVisible
        {
            get
            {
                return this.m_bWordVisible;
            }
            set
            {
                this.m_bWordVisible = value;

                this.label_word.Visible = value;
                this.textBox_word.Visible = value;
            }
        }
    }
}
