using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DigitalPlatform.Text
{
    /// <summary>
    /// 能保持原序的参数分析类
    /// </summary>
    public class ParamList
    {
        class Parameter
        {
            public string Name = "";
            public string Value = "";
        }

        List<Parameter> _table = new List<Parameter>();

        void Set(string strName, string strValue)
        {
            Parameter parameter = Find(strName);
            if (parameter == null)
            {
                parameter = new Parameter();
                parameter.Name = strName;
                this._table.Add(parameter);
            }
            parameter.Value = strValue;
        }

        Parameter Find(string strName)
        {
            foreach (Parameter parameter in this._table)
            {
                if (parameter.Name == strName)
                    return parameter;
            }

            return null;
        }

        public string this[string strName]
        {
            get
            {
                return GetValue(strName);
            }
            set
            {
                this.Set(strName, value);
            }
        }

        public string GetValue(string strName)
        {
            Parameter parameter = Find(strName);
            if (parameter == null)
                return null;
            return parameter.Value;
        }

        // 删除一个条目
        public bool Remove(string strName)
        {
            Parameter parameter = Find(strName);
            if (parameter == null)
                return false;   // 本来就不存在
            this._table.Remove(parameter);
            return true;
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static ParamList Build(string strText,
            char chSegChar,
            char chEqualChar,
            string strDecodeStyle = "")
        {
            ParamList list = new ParamList();

            if (string.IsNullOrEmpty(strText) == true)
                return list;

            string[] parts = strText.Split(new char[] { chSegChar });   // ','
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                string strName = "";
                string strValue = "";
                int nRet = strPart.IndexOf(chEqualChar);    // '='
                if (nRet == -1)
                {
                    strName = strPart;
                    strValue = "";
                }
                else
                {
                    strName = strPart.Substring(0, nRet).Trim();
                    strValue = strPart.Substring(nRet + 1).Trim();
                }

                if (String.IsNullOrEmpty(strName) == true
                    && String.IsNullOrEmpty(strValue) == true)
                    continue;

                if (strDecodeStyle == "url")
                    strValue = HttpUtility.UrlDecode(strValue);

                list.Set(strName, strValue);
            }

            return list;
        }

        // 按照指定的 key 名字集合顺序和个数输出
        public string ToString(
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (Parameter parameter in this._table)
            {
                if (result.Length > 0)
                    result.Append(chSegChar);
                string strValue = parameter.Value;

                if (strEncodeStyle == "url")
                    result.Append(parameter.Name + new string(chEqualChar, 1) + HttpUtility.UrlEncode(strValue));
                else
                    result.Append(parameter.Name + new string(chEqualChar, 1) + strValue);
            }

            return result.ToString();
        }

    }
}
