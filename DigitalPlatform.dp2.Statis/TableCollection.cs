using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.dp2.Statis
{
    // 用名字管理的 Table
    public class TableCollection
    {
        // 名字 --> Table 对象
        Hashtable _tables = new Hashtable();

#if NO
        // 名字 --> hints
        Hashtable _hints = new Hashtable();
#endif

        // 获得指定前缀的若干名字
        public List<string> GetNames(string strPrefix)
        {
            List<string> results = new List<string>();
            foreach (string key in _tables.Keys)
            {
                if (StringUtil.HasHead(key, strPrefix) == true)
                    results.Add(key);
            }

            return results;
        }

        // 获得一个表格。如果已经有了，就利用现成的；如果还没有，就自动创建一个
        public Table GetTable(string strName)
        {
            Table table = (Table)_tables[strName];
            if (table != null)
                return table;

            table = new Table(0);
            _tables[strName] = table;

            return table;
        }

        /// <summary>
        /// 写入一个单元的值
        /// </summary>
        /// <param name="strTableName">Table 名字</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="value">值</param>
        public void SetValue(
            string strTableName,
            string strEntry,
            int nColumn,
            object value)
        {

            Table table = GetTable(strTableName);
            table.SetValue(strEntry,
                nColumn, value);
        }

        static void Inc(Table table,
    string strEntry,
    int nColumn,
    string strPrice)
        {
            Line line = table.EnsureLine(strEntry);
            string strOldValue = (string)line[nColumn];
            if (string.IsNullOrEmpty(strOldValue) == true)
            {
                line.SetValue(nColumn, strPrice);
                return;
            }

            // 连接两个价格字符串
            string strPrices = PriceUtil.JoinPriceString(strOldValue,
                    strPrice);

            string strError = "";
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

            string strResult = "";
            nRet = PriceUtil.TotalPrice(prices,
out strResult,
out strError);
            if (nRet == -1)
                throw new Exception(strError);

            line.SetValue(nColumn, strResult);
        }

        /// <summary>
        /// 增量一个单元的金额
        /// </summary>
        /// <param name="strTableName">Table 名字</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="strPrice">金额字符串</param>
        public void IncPrice(
            string strTableName,
            string strEntry,
            int nColumn,
            string strPrice)
        {
            if (string.IsNullOrEmpty(strPrice) == true)
                return;

            Table table = GetTable(strTableName);
            Inc(table, strEntry, nColumn, strPrice);
        }

        /// <summary>
        /// 增量一个单元的整数值
        /// </summary>
        /// <param name="strTableName">Table 名字</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="createValue">创建值</param>
        /// <param name="incValue">增量值</param>
        public void IncValue(
            string strTableName,
            string strEntry,
            int nColumn,
            Int64 createValue,
            Int64 incValue)
        {
            Table table = GetTable(strTableName);
            table.IncValue(strEntry,
                nColumn, createValue, incValue);
        }
    }
}
