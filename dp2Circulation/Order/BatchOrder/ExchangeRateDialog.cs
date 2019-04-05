using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 编辑汇率转换表的对话框
    /// </summary>
    public partial class ExchangeRateDialog : Form
    {
        // 汇率转换表配置文件。这是一个 XML 文件
        public string CfgFileName { get; set; }

        XmlDocument _dom = null;

        public bool Changed { get; set; }

        public ExchangeRateDialog()
        {
            InitializeComponent();
        }

        private void ExchangeRateDialog_Load(object sender, EventArgs e)
        {
            FillSellerList();

            DisplayFirstSeller();
        }

        void OpenXml()
        {
            if (this._dom == null)
            {
                _dom = new XmlDocument();
                try
                {
                    _dom.Load(this.CfgFileName);
                }
                catch (FileNotFoundException)
                {
                    _dom.LoadXml("<root />");
                }
            }
        }

        void CloseXml()
        {
            if (_dom != null)
            {
                if (this.Changed == true)
                {
                    _dom.Save(this.CfgFileName);
                    this.Changed = false;
                }
                _dom = null;
            }
        }

        void FillSellerList()
        {
            this.comboBox_seller.Items.Clear();

            OpenXml();
            XmlNodeList nodes = _dom.DocumentElement.SelectNodes("item");
            foreach (XmlElement item in nodes)
            {
                string strSeller = item.GetAttribute("seller");
                string strTable = item.GetAttribute("table");

                this.comboBox_seller.Items.Add(strSeller);
            }
        }

        void DisplayFirstSeller()
        {
            if (this.comboBox_seller.Items.Count > 0)
            {
                this.comboBox_seller.Text = (string)this.comboBox_seller.Items[0];
            }
        }

        // return:
        //      true    合法
        //      false   不合法。已经报错了
        bool VerifyData()
        {
            if (this._dom == null)
                return true;

            XmlNodeList nodes = _dom.DocumentElement.SelectNodes("item");
            foreach (XmlElement item in nodes)
            {
                string strSeller = item.GetAttribute("seller");
                string strTable = item.GetAttribute("table");

                // 验证数据是否合法
                try
                {
                    RateItem.ParseList(strTable);
                }
                catch (Exception ex)
                {
                    this.comboBox_seller.Text = strSeller;
                    MessageBox.Show(this, "渠道(书商) '" + strSeller + "' 的汇率表格式不合法: " + ex.Message);
                    return false;
                }
            }

            return true;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {

            // 如果最后的场景是即将添加，则自动添加
            if (this.button_add.Enabled == true)
            {
                button_add_Click(this, e);
            }

            StoreLastSeller();

            // 验证数据是否合法
            // return:
            //      true    合法
            //      false   不合法。已经报错了
            if (VerifyData() == false)
                return;

            // 保存到 XML 文件
            this.CloseXml();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string RateList
        {
            get
            {
                return this.textBox_rates.Text.Replace("\r\n", ";");
            }
            set
            {
                if (value == null)
                {
                    this.textBox_rates.Text = "";
                    return;
                }
                this.textBox_rates.Text = value.Replace(";", "\r\n");
            }
        }

        public class TableItem
        {
            public string Seller { get; set; }
            public string Table { get; set; }
        }

        public static TableItem FindTableItem(XmlDocument dom, string strSeller)
        {
            XmlElement node = dom.DocumentElement.SelectSingleNode("item[@seller='" + strSeller + "']") as XmlElement;
            if (node == null)
                return null;
            TableItem item = new TableItem();
            item.Seller = strSeller;
            item.Table = node.GetAttribute("table");
            return item;
        }

        string _lastSeller = null;

        // 把最近一个 seller 的修改兑现到 XmlDocument
        void StoreLastSeller()
        {
            if (_lastSeller == null)
                return;

#if NO
            XmlElement node = _dom.DocumentElement.SelectSingleNode("item[@seller='" + _lastSeller + "']") as XmlElement;
            if (node == null)
            {
#if NO
                node = _dom.CreateElement("item");
                _dom.DocumentElement.AppendChild(node);
                node.SetAttribute("seller", _lastSeller);
#endif
                _lastSeller = null;
                return;
            }

            string strOldValue = node.GetAttribute("table");
            string strNewValue = this.RateList;
            if (strOldValue != strNewValue)
            {
                node.SetAttribute("table", strNewValue);
                this.Changed = true;
            }
#endif
            if (AddSellerToDom(this._lastSeller, true) == false)
            {
                _lastSeller = null;
                return;
            }
        }

        private void comboBox_seller_TextChanged(object sender, EventArgs e)
        {
            // 先保存修改
            StoreLastSeller();

            this.BeginInvoke(new Action(RefreshAddDeleteButtons));
        }

        void RefreshAddDeleteButtons()
        {
            TableItem item = FindTableItem(this._dom, this.comboBox_seller.Text);
            if (item == null)
            {
                this.button_add.Enabled = true;
                this.button_delete.Enabled = false;
            }
            else
            {
                this.button_add.Enabled = false;
                this.button_delete.Enabled = true;
            }
        }

        private void comboBox_seller_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 先保存修改
            StoreLastSeller();

            this.RateList = "";

            TableItem item = FindTableItem(this._dom, this.comboBox_seller.Text);
            if (item != null)
            {
                this.RateList = item.Table;

                Debug.Assert(this.comboBox_seller.Text == item.Seller, "");
            }

            _lastSeller = this.comboBox_seller.Text;
        }

        bool AddSellerToDom(string strSeller, bool bReplace)
        {
            bool bInserted = false;
            XmlElement node = _dom.DocumentElement.SelectSingleNode("item[@seller='" + strSeller + "']") as XmlElement;
            if (node == null)
            {
                if (bReplace == true)
                    return false;

                node = _dom.CreateElement("item");
                _dom.DocumentElement.AppendChild(node);
                node.SetAttribute("seller", strSeller);
                bInserted = true;
            }

            string strOldValue = node.GetAttribute("table");
            string strNewValue = this.RateList;
            if (strOldValue != strNewValue)
            {
                node.SetAttribute("table", strNewValue);
                this.Changed = true;
            }

            if (bInserted)
                FillSellerList();
            return true;
        }

        private void button_add_Click(object sender, EventArgs e)
        {
#if NO
            XmlElement node = _dom.DocumentElement.SelectSingleNode("item[@seller='" + this.comboBox_seller.Text + "']") as XmlElement;
            if (node == null)
            {
                node = _dom.CreateElement("item");
                _dom.DocumentElement.AppendChild(node);
                node.SetAttribute("seller", this.comboBox_seller.Text);
            }

            string strOldValue = node.GetAttribute("table");
            string strNewValue = this.RateList;
            if (strOldValue != strNewValue)
            {
                node.SetAttribute("table", strNewValue);
                this.Changed = true;
            }
#endif
            AddSellerToDom(this.comboBox_seller.Text, false);

            this._lastSeller = this.comboBox_seller.Text;
            this.BeginInvoke(new Action(RefreshAddDeleteButtons));
        }

        private void button_delete_Click(object sender, EventArgs e)
        {
            XmlElement node = _dom.DocumentElement.SelectSingleNode("item[@seller='" + this.comboBox_seller.Text + "']") as XmlElement;
            if (node == null)
                return;

            node.ParentNode.RemoveChild(node);
            this.Changed = true;

            this._lastSeller = null;
            FillSellerList();

            // 切换为当前存在的第一个 seller
            DisplayFirstSeller();

            if (this.comboBox_seller.Items.Count == 0)
            {
                this.comboBox_seller.Text = "";
                this.RateList = "";
            }
        }
    }

    public class RateItem
    {
        public DoubleCurrencyItem Source { get; set; }
        public DoubleCurrencyItem Target { get; set; }

        // 进行汇率计算
        public CurrencyItem Exchange(CurrencyItem item)
        {
            CurrencyItem result = new CurrencyItem();
            result.Value = Convert.ToDecimal(Convert.ToDouble(item.Value) * this.Target.Value / this.Source.Value);
            result.Prefix = this.Target.Prefix;
            result.Postfix = this.Target.Postfix;
            return result;
        }

        // CNY1.00=USD0.22222
        public static RateItem Parse(string strText)
        {
            if (strText.IndexOf('=') == -1)
                throw new Exception("对照事项格式不正确，缺乏等号。'" + strText + "'");
            List<string> parts = StringUtil.ParseTwoPart(strText, "=");
            string strSource = parts[0].Trim();
            string strTarget = parts[1].Trim();

            RateItem result = new RateItem();
            result.Source = DoubleCurrencyItem.Parse(strSource);
            result.Target = DoubleCurrencyItem.Parse(strTarget);

            return result;
        }

        // 分号分隔。
        public static List<RateItem> ParseList(string strText)
        {
            List<RateItem> results = new List<RateItem>();

            string[] segments = strText.Split(new char[] { ';' });
            foreach (string segment in segments)
            {
                if (string.IsNullOrEmpty(segment))
                    continue;
                string strSegment = segment.Trim();
                if (string.IsNullOrEmpty(strSegment))
                    continue;
                results.Add(RateItem.Parse(strSegment));
            }

            return results;
        }

        public static RateItem FindBySource(List<RateItem> rate_table,
            string strPrefix,
            string strPostfix)
        {
            foreach (RateItem item in rate_table)
            {
                if (string.IsNullOrEmpty(item.Source.Prefix) == true
                    && string.IsNullOrEmpty(strPrefix) == true)
                {

                }
                else
                {
                    if (item.Source.Prefix != strPrefix)
                        continue;
                }

                if (string.IsNullOrEmpty(item.Source.Postfix) == true
    && string.IsNullOrEmpty(strPostfix) == true)
                {

                }
                else
                {

                    if (item.Source.Postfix != strPostfix)
                        continue;
                }

                return item;
            }

            return null;
        }

        // 将形如"-CNY123.4+USD10.55-20.3"的价格字符串计算汇率
        // parameters:
        public static string RatePrices(
            List<RateItem> rate_table,
            string strPrices)
        {
            string strError = "";

            strPrices = strPrices.Trim();

            if (String.IsNullOrEmpty(strPrices) == true)
                return "";

            List<string> prices = null;
            // 将形如"-123.4+10.55-20.3"的价格字符串切割为单个的价格字符串，并各自带上正负号
            // return:
            //      -1  error
            //      0   succeed
            int nRet = PriceUtil.SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            List<string> changed_prices = new List<string>();
            foreach (string price in prices)
            {
                CurrencyItem item = CurrencyItem.Parse(price);

                RateItem rate = FindBySource(rate_table, item.Prefix, item.Postfix);
                if (rate == null)
                {
                    changed_prices.Add(price);
                    continue;
                }

                CurrencyItem result = rate.Exchange(item);
                changed_prices.Add(result.ToString());
            }

            List<string> results = new List<string>();

            // 汇总价格
            // 货币单位不同的，互相独立
            // return:
            //      -1  error
            //      0   succeed
            nRet = PriceUtil.TotalPrice(changed_prices,
                out results,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

#if NO
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < results.Count; i++)
            {
                string strOnePrice = results[i];
                if (String.IsNullOrEmpty(strOnePrice) == true)
                    continue;
                if (strOnePrice[0] == '+')
                    text.Append("+" + strOnePrice.Substring(1));
                else if (strOnePrice[0] == '-')
                    text.Append("-" + strOnePrice.Substring(1));
                else
                    text.Append("+" + strOnePrice);    // 缺省为正数
            }

            return text.ToString().TrimStart(new char[] { '+' });
#endif
            return PriceUtil.JoinPriceString(results);
        }
    }

    /// <summary>
    /// 金额事项
    /// DoubleCurrencyItem 和 CurrentItem 的区别，是前者用 double 存储数字，后者用 decimal 存储。后者只允许小数点后最多两位精度
    /// 所以 DoubleCurrenyItem 适合表示汇率乘除法计算中的比例因子
    /// </summary>
    public class DoubleCurrencyItem
    {
        /// <summary>
        /// 前缀字符串
        /// </summary>
        public string Prefix = "";
        /// <summary>
        /// 后缀字符串
        /// </summary>
        public string Postfix = "";
        /// <summary>
        /// 数值
        /// </summary>
        public Double Value = 0;

        public static DoubleCurrencyItem Parse(string strText)
        {
            string strError = "";
            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            int nRet = PriceUtil.ParsePriceUnit(strText,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
            double value = 0;
            try
            {
                value = Convert.ToDouble(strValue);
            }
            catch
            {
                strError = "数字 '" + strValue + "' 格式不正确";
                throw new Exception(strError);
            }

            DoubleCurrencyItem item = new DoubleCurrencyItem();
            item.Prefix = strPrefix;
            item.Postfix = strPostfix;
            item.Value = value;

            return item;
        }
    }
}
