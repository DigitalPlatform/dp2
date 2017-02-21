using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class ExchangeRateDialog : Form
    {
        public ExchangeRateDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 验证数据是否合法
            try
            {
                RateItem.ParseList(this.RateList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }
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
