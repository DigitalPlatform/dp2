using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// 四角号码基础类
    /// </summary>
    public class QuickSjhm
    {
        XmlDocument dom = null;

        public QuickSjhm(string strFileName)
        {
            dom = new XmlDocument();
            dom.Load(strFileName);
        }

        // 获得四角号码
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetSjhm(string strHanzi,
            out string strSjhm,
            out string strError)
        {
            strSjhm = "";
            strError = "";

            if (dom == null)
            {
                strError = "尚未装载四角号码文件内容";
                return -1;
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("p[@h='" + strHanzi + "']");
            if (node == null)
                return 0;
            strSjhm = DomUtil.GetAttr(node, "s");
            return 1;
        }
    }
}
