using DigitalPlatform.Marc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 书目记录合并策略
    /// </summary>
    public class MergeRegistry
    {
        // 数据库名顺序列表
        public List<string> DbNames = new List<string>();

        // 要求 ListViewItem 的 .Tag 为 ItemTag 类型
        public void Sort(ref List<ListViewItem> items)
        {
            items.Sort((item1, item2) =>
            {
                ItemTag info1 = (ItemTag)item1.Tag;
                ItemTag info2 = (ItemTag)item2.Tag;

                // 先按照数据库名字排序
                int index1 = this.DbNames.IndexOf(Global.GetDbName(info1.RecPath));
                if (index1 == -1)
                    index1 = this.DbNames.Count + 1;
                int index2 = this.DbNames.IndexOf(Global.GetDbName(info2.RecPath));
                if (index2 == -1)
                    index2 = this.DbNames.Count + 1;
                if (index1 != index2)
                    return index1 - index2;

                // 再观察 606 690 丰富程度
                int nRet = Compare6XX(info1.Xml, info2.Xml);
                if (nRet != 0)
                    return nRet;

                // 再观察 MARC 记录长度
                nRet = CompareMarcLength(info1.Xml, info2.Xml);
                if (nRet != 0)
                    return nRet;

                return nRet;
            });
        }

        // 比较 606 690 丰富程度。数值小于 0 表示 strXml1 比 strXml2 丰富
        static int Compare6XX(string strXml1, string strXml2)
        {
            int nCount1 = Count6XX(strXml1);
            int nCount2 = Count6XX(strXml2);
            return nCount2 - nCount1;
        }

        static int CompareMarcLength(string strXml1, string strXml2)
        {
            return GetMarcLength(strXml2) - GetMarcLength(strXml1);
        }

        static int Count6XX(string strXml)
        {
            string strError = "";

            string strMarc = "";
            string strOutMarcSyntax = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(strXml,
                true,   // 2013/1/12 修改为true
                "", // strMarcSyntax
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                return 0;

            int nCount = 0;
            if (strOutMarcSyntax == "unimarc")
            {
                MarcRecord record = new MarcRecord(strMarc);
                nCount += record.select("field[@name='606']").count;
                nCount += record.select("field[@name='690']").count;
                return nCount;
            }
            if (strOutMarcSyntax == "usmarc")
            {
                return 0;
            }
            return 0;
        }

        static int GetMarcLength(string strXml)
        {
            string strError = "";

            string strMarc = "";
            string strOutMarcSyntax = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            int nRet = MarcUtil.Xml2Marc(strXml,
                true,   // 2013/1/12 修改为true
                "", // strMarcSyntax
                out strOutMarcSyntax,
                out strMarc,
                out strError);
            if (nRet == -1)
                return 0;

            MarcRecord record = new MarcRecord(strMarc);

            return record.Text.Length;
        }

    }
}
