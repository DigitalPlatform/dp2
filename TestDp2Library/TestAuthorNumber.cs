using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Text;

namespace TestDp2Library
{
    [TestClass]
    public class TestAuthorNumber
    {
        [TestMethod]
        public void Test_GetSubRange_1()
        {
            string gcat_xml = @"<i h='陈' p='CHEN2' >
        <r n='A' v='C350' f='10' />
        <r n='BA-BING' v='C360' f='10' />
        <r n='BO-BU' v='C370' f='10' />
        <r n='C' v='C380' f='10' />
        <r n='D' v='C390' f='10' />
        <r n='E' v='C400' f='10' />
        <r n='F' v='C410' f='10' />
        <r n='G' v='C420' f='10' />
        <r n='HA-HENG' v='C430' f='10' />
        <r n='HO-HUO' v='C440' f='10' />
        <r n='JA-JIA' v='C450' f='10' />
        <r n='JIAN-JIE' v='C460' f='10' />
        <r n='JIN-JUN' v='C470' f='10' />
        <r n='K' v='C480' f='10' />
        <r n='LA-LIA' v='C490' f='10' />
        <r n='LIAN-LUO' v='C500' f='10' />
        <r n='M' v='C510' f='10' />
        <r n='N-P' v='C520' f='10' />
        <r n='Q' v='C530' f='10' />
        <r n='R' v='C540' f='10' />
        <r n='SA-SENG' v='C550' f='10' />
        <r n='SHA-SUO' v='C560' f='10' />
        <r n='T' v='C570' f='10' />
        <r n='W' v='C580' f='10' />
        <r n='X-XIU' v='C590' f='10' />
        <r n='XU-XUN' v='C600' f='10' />
        <r n='Y-YE' v='C610' f='10' />
        <r n='YI-YOU' v='C620' f='10' />
        <r n='YU-YUN' v='C630' f='10' />
        <r n='ZA-ZENG' v='C640' f='10' />
        <r n='ZHA-ZO' v='C650' f='10' />
        <r n='ZU-ZUO' v='C660' f='10' />
    </i>";

            string case_xml = @"<root>

        <r n='A' v='C350' f='10' />
            <r n='B' v='' f='' />
        <r n='BA-BING' v='C360' f='10' />
            <r n='BANG' v='C360' f='10' />

        <r n='BO-BU' v='C370' f='10' />
        <r n='C' v='C380' f='10' />
            <r n='CANG' v='C380' f='10' />
        <r n='D' v='C390' f='10' />
            <r n='DONG' v='C390' f='10' />

        <r n='E' v='C400' f='10' />
        <r n='F' v='C410' f='10' />
        <r n='G' v='C420' f='10' />
            <r n='GZ' v='C420' f='10' />

            <r n='H' v='' f='' />
        <r n='HA-HENG' v='C430' f='10' />
        <r n='HO-HUO' v='C440' f='10' />
        <r n='JA-JIA' v='C450' f='10' />
            <r n='JIAM' v='C450' f='10' />

        <r n='JIAN-JIE' v='C460' f='10' />
        <r n='JIN-JUN' v='C470' f='10' />
        <r n='K' v='C480' f='10' />
        <r n='LA-LIA' v='C490' f='10' />
        <r n='LIAN-LUO' v='C500' f='10' />
        <r n='M' v='C510' f='10' />
        <r n='N-P' v='C520' f='10' />
        <r n='Q' v='C530' f='10' />
        <r n='R' v='C540' f='10' />
        <r n='SA-SENG' v='C550' f='10' />
        <r n='SHA-SUO' v='C560' f='10' />
        <r n='T' v='C570' f='10' />
        <r n='W' v='C580' f='10' />
        <r n='X-XIU' v='C590' f='10' />
        <r n='XU-XUN' v='C600' f='10' />
        <r n='Y-YE' v='C610' f='10' />
        <r n='YI-YOU' v='C620' f='10' />
        <r n='YU-YUN' v='C630' f='10' />
            <r n='YU-YUNG' v='C630' f='10' />

        <r n='ZA-ZENG' v='C640' f='10' />
        <r n='ZHA-ZO' v='C650' f='10' />
            <r n='ZONG' v='C650' f='10' />

        <r n='ZU-ZUO' v='C660' f='10' />
</root>";

            TestCase(gcat_xml, case_xml);
        }

        void TestCase(string gcat_xml, string case_xml)
        {
            XmlDocument gcat_dom = new XmlDocument();
            gcat_dom.LoadXml(gcat_xml);

            XmlDocument case_dom = new XmlDocument();
            case_dom.LoadXml(case_xml);

            XmlNodeList nodes = case_dom.DocumentElement.SelectNodes("r");
            foreach (XmlElement node in nodes)
            {
                string strName = node.GetAttribute("n");
                string strValue = node.GetAttribute("v");
                string strFufen = node.GetAttribute("f");

                List<string> parts = StringUtil.ParseTwoPart(strName, "-");
                if (string.IsNullOrEmpty(parts[1]))
                    parts.RemoveAt(1);

                foreach (string strPinyin in parts)
                {
                    //if (string.IsNullOrEmpty(strPinyin))
                    //    continue;

                    // parameters:
                    //		strPinyin	一个汉字的拼音。如果==""，表示找第一个r元素
                    // return:
                    //		-1	出错
                    //		0	没有找到
                    //		1	找到
                    int nRet = LibraryApplication.GetSubRange(gcat_dom,
                        strPinyin,
                        false, // bool bOutputDebugInfo,
                        out string strOutputValue,
                        out string strOutputFufen,
                        out string strDebugInfo,
                        out string strError);
                    if (nRet == -1)
                        throw new Exception($"拼音 {strPinyin} 获得范围时出错: {strError} ");

                    if (nRet == 0)
                    {
                        strOutputFufen = "";
                        strOutputValue = "";
                    }

                    // 比较返回结果
                    if (strOutputValue != strValue)
                        throw new Exception($"拼音 {strPinyin} 获得范围时出错: 返回value='{strOutputValue}' 和期望值 '{strValue}' 不同 ");
                    if (strOutputFufen != strFufen)
                        throw new Exception($"拼音 {strPinyin} 获得范围时出错: 返回fufen='{strOutputFufen}' 和期望值 '{strFufen}' 不同 ");
                }
            }

        }

        [TestMethod]
        public void Test_GetSubRange_2()
        {
            string gcat_xml = @"<i h='北京图书馆' p='BEI3' >
        <r n='' v='B618' f='0' />
        <r n='J' v='B619' f='3' />
        <r n='T' v='B622' f='3' />
        <r n='Z' v='B625' f='3' />
    </i>";

            string case_xml = @"<i h='北京图书馆' p='BEI3' >
        <r n='' v='B618' f='0' />
        <r n='A' v='B619' f='3' />
        <r n='A-J' v='B619' f='3' />
        <r n='K-T' v='B622' f='3' />
        <r n='W-Z' v='B625' f='3' />
    </i>";

            TestCase(gcat_xml, case_xml);
        }

    }

    class TestInfo
    {
        public string Hanzi { get; set; }
        public string Pinyin { get; set; }
        public string Number { get; set; }
    }

}
