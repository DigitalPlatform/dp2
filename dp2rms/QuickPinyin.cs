using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;

namespace dp2rms
{
    /// <summary>
    /// 快速产生拼音
    /// </summary>
    public class QuickPinyin
    {
        XmlDocument dom = null;

        public QuickPinyin(string strFileName)
        {
            dom = new XmlDocument();
            dom.Load(strFileName);
        }

        // 获得拼音
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int GetPinyin(string strHanzi,
            out string strPinyins,
            out string strError)
        {
            strPinyins = "";
            strError = "";

            if (dom == null)
            {
                strError = "尚未装载拼音文件内容";
                return -1;
            }

            XmlNode node = dom.DocumentElement.SelectSingleNode("p[@h='"+strHanzi+"']");
            if (node == null)
                return 0;
            strPinyins = DomUtil.GetAttr(node, "p");
            return 1;
        }
    }
}
