using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace dp2Catalog
{
    public partial class AmazonQueryControl : UserControl
    {
        string m_strLang = "zh-CN";
        public string Lang
        {
            get
            {
                return this.m_strLang;
            }
            set
            {
                this.m_strLang = value;
                if (this.amazonSearchParametersControl1 != null)
                    this.amazonSearchParametersControl1.Lang = value;
            }
        }

        public IDictionary<string, string> ParameterTable
        {
            get
            {
                return BuildParameterTable();
            }
            set
            {
                if (value.ContainsKey("SearchIndex") == true)
                    this.SetSearchIndex(value["SearchIndex"]);
                this.amazonSearchParametersControl1.RestoreValues(value);
            }
        }

        public AmazonQueryControl()
        {
            InitializeComponent();
            this.comboBox_searchIndex.RemoveRightPartAtTextBox = false;

        }

        public void BuildParametTable(ref IDictionary<string, string> table)
        {
            if (table == null)
                table = new Dictionary<string, string>();
            table["SearchIndex"] = this.GetSearchIndex();

            // ParameterTable temp = null;
            this.amazonSearchParametersControl1.BuildParameterTable(ref table);
        }

        // 构造参数表
        IDictionary<string, string> BuildParameterTable()
        {
            IDictionary<string, string> table = new ParameterTable();
            // TODO: 是否需要检查 当前 SearchIndex 值为空
            table.Add("SearchIndex", this.GetSearchIndex());

            // ParameterTable temp = null;
            this.amazonSearchParametersControl1.BuildParameterTable(ref table);
            /*
            foreach (string s in temp.Keys)
            {
                table.Add(s, temp[s]);
            }
             * */

            return table;
        }

        public string GetSearchIndex()
        {
            string strLeft = "";
            string strRight = "";
            ParseLeftRight(this.comboBox_searchIndex.Text,
                out strLeft,
                out strRight);
            return strRight;
        }

        // 设置纯粹的 SearchIndex 值。注意，strValue 内容不包括 caption 部分
        public void SetSearchIndex(string strValue)
        {
            // 从配置文件中获得一个 SearchIndex 名称对应的 Caption
            string strCaption = this.amazonSearchParametersControl1.GetSearchIndexCaption(
                strValue,
                this.Lang);
            this.comboBox_searchIndex.Text = strCaption + "\t" + strValue;
        }

        public int Initial(string strCfgFileName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFileName);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            this.amazonSearchParametersControl1.Lang = this.Lang; 
            this.amazonSearchParametersControl1.CfgDom = dom;

            // 填充 SearchIndex 列表
            this.comboBox_searchIndex.Items.Clear();
            List<string> searchIndexNames = this.amazonSearchParametersControl1.GetSearchIndexNames(this.Lang);
            foreach (string s in searchIndexNames)
            {
                this.comboBox_searchIndex.Items.Add(s);
            }

            if (string.IsNullOrEmpty(this.comboBox_searchIndex.Text) == true
                && searchIndexNames.Count > 0)
            {
                this.comboBox_searchIndex.Text = searchIndexNames[0];
                SetParameters();
            }
            else if (string.IsNullOrEmpty(this.comboBox_searchIndex.Text) == false)
            {
                SetParameters();
            }


            return 0;
        }

        private void comboBox_searchIndex_TextChanged(object sender, EventArgs e)
        {
            if (this.amazonSearchParametersControl1.CfgDom != null
                && this.amazonSearchParametersControl1.SearchIndex != this.comboBox_searchIndex.Text)
            {
                SetParameters();
            }
        }

        void SetParameters()
        {
            try
            {
                string strLeft = "";
                string strRight = "";
                ParseLeftRight(this.comboBox_searchIndex.Text,
                    out strLeft,
                    out strRight);
                this.amazonSearchParametersControl1.SearchIndex = strRight;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        // 剖析出左右两个部分
        public static void ParseLeftRight(string strText,
            out string strLeft,
            out string strRight,
            string strSep = "\t")
        {
            int nRet = strText.IndexOf(strSep);
            if (nRet != -1)
            {
                strLeft = strText.Substring(0, nRet).Trim();
                strRight = strText.Substring(nRet + 1).Trim();
            }
            else
            {
                strLeft = strText;
                strRight = "";
            }
        }


        public static string GetRight(string s)
        {
            string strLeft = "";
            string strRight = "";
            AmazonQueryControl.ParseLeftRight(s,
                out strLeft,
                out strRight);

            return strRight;
        }

        public static string GetLeft(string s)
        {
            string strLeft = "";
            string strRight = "";
            AmazonQueryControl.ParseLeftRight(s,
                out strLeft,
                out strRight);

            return strLeft;
        }
    }

    public class ParameterTable : Dictionary<string, string>
    {
        public string Dump()
        {
            StringBuilder text = new StringBuilder(4096);
            foreach (string key in this.Keys)
            {
                text.Append(key + "="+ this[key] + "\r\n");
            }

            return text.ToString();
        }
    }
}
