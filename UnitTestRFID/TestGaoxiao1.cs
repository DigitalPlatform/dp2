using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;
using System.Diagnostics;

namespace UnitTestRFID
{
    /// <summary>
    /// 测试高校联盟 UHF 标签编码解码功能中的 EncodeUserElementContent() 函数
    /// </summary>
    [TestClass]
    public class TestGaoxiao1
    {
        // 对机构代码 "60000" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_oi_1()
        {
            // 所属馆标识 Owner Library
            /*
取值方式：2 字节整型数。参照中华人民共和国教育部行业标准 JY/T1001-2012，《教育管理信
息-教育管理基础代码》中的《中国高等院校代码表》取值，以 5 位数字所代表的高等院校代
码来标识所属馆。其中，针对首位数字为 9（军事院校，例如：90001 代表国防大学）的情况，
在存放时，需要将 9 变成为 6，然后以 2 字节整型数存储，在读取后，需要将 6 变成为 9，恢
复成原始代码。
* */
            try
            {
                var result = GaoxiaoUtility.EncodeUserElementContent(3, "60000");

                throw new Exception("前面应抛出 ArgumentException 异常才对");

                byte[] correct = new byte[] { 0x60, 0xea };
                // Assert.IsTrue(result.SequenceEqual(correct));
                Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
            }
            catch (ArgumentException)
            {

            }
        }

        // 对机构代码 "90000" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_oi_2()
        {
            // 所属馆标识 Owner Library
            /*
取值方式：2 字节整型数。参照中华人民共和国教育部行业标准 JY/T1001-2012，《教育管理信
息-教育管理基础代码》中的《中国高等院校代码表》取值，以 5 位数字所代表的高等院校代
码来标识所属馆。其中，针对首位数字为 9（军事院校，例如：90001 代表国防大学）的情况，
在存放时，需要将 9 变成为 6，然后以 2 字节整型数存储，在读取后，需要将 6 变成为 9，恢
复成原始代码。
* */
            var result = GaoxiaoUtility.EncodeUserElementContent(3, "90000");

            byte[] correct = new byte[] { 0xea, 0x60, };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对机构代码 "00001" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_oi_3()
        {
            // 所属馆标识 Owner Library
            var result = GaoxiaoUtility.EncodeUserElementContent(3, "00001");
            byte[] correct = new byte[] { 0x00, 0x01, };
            // Assert.IsTrue(result.SequenceEqual(correct));
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对卷册 "1/3" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_si_1()
        {
            // 卷册信息 Set Information
            /*
取值方式：总 4 字节定长字段，其中：高 2 字节存放卷册总数（最多 65536 卷册），低 2 字节
存放卷册序号（1 - 65536）。
            * */
            var result = GaoxiaoUtility.EncodeUserElementContent(4, "1/3");
            byte[] correct = new byte[] { 0x00, 0x03, 0x00, 0x01, };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对馆藏类别 "1.2" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_tou_1()
        {
            // 馆藏类别与状态 type of usage
            // 总 2 字节定长字段，其中：高字节存放馆藏类别(主限定标识)，低字节存放馆藏状态(次限定标识)
            var result = GaoxiaoUtility.EncodeUserElementContent(5, "1.2");
            byte[] correct = new byte[] { 0x01, 0x02,  };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对馆藏类别 "1" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_tou_2()
        {
            // 馆藏类别与状态 type of usage
            // 总 2 字节定长字段，其中：高字节存放馆藏类别(主限定标识)，低字节存放馆藏状态(次限定标识)
            var result = GaoxiaoUtility.EncodeUserElementContent(5, "1");
            byte[] correct = new byte[] { 0x01, 0x00,  };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对馆藏位置 "english" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_il_1()
        {
            // 6: 馆藏位置 Item Location
            string text = "english";
            var result = GaoxiaoUtility.EncodeUserElementContent(6, text);
            byte[] correct = Encoding.UTF8.GetBytes(text);
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对馆藏位置 "中文" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_il_2()
        {
            // 6: 馆藏位置 Item Location
            string text = "中文";
            var result = GaoxiaoUtility.EncodeUserElementContent(6, text);
            byte[] correct = Encoding.UTF8.GetBytes(text);
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对机构代码 "60000" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_oi_1()
        {
            byte[] source = new byte[] { 0xea, 0x60, };
            var result = GaoxiaoUtility.DecodeUserElementContent(3, source);
            Assert.AreEqual("90000", result);
        }

        // 对机构代码 "000001" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_oi_2()
        {
            byte[] source = new byte[] { 0x00, 0x01, };
            var result = GaoxiaoUtility.DecodeUserElementContent(3, source);
            Assert.AreEqual("00001", result);
        }

        // 对卷册信息 "1/3" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_si_1()
        {
            byte[] source = new byte[] { 0x00, 0x03, 0x00, 0x01, };
            var result = GaoxiaoUtility.DecodeUserElementContent(4, source);
            Assert.AreEqual("1/3", result);
        }

        // 对馆藏类别 "1.2" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_tou_1()
        {
            byte[] source = new byte[] { 0x01, 0x02,  };
            var result = GaoxiaoUtility.DecodeUserElementContent(5, source);
            Assert.AreEqual("1.2", result);
        }

        // 测试编码后再解码是否能完全还原
        [TestMethod]
        public void Test_EncodeAndDecode_1()
        {
            List<GaoxiaoUserElement> elements = new List<GaoxiaoUserElement>();
            elements.Add(new GaoxiaoUserElement
            {
                OID = 3,
                Content = "50000"
            });
            elements.Add(new GaoxiaoUserElement
            {
                OID = 4,
                Content = "1/3"
            });
            elements.Add(new GaoxiaoUserElement
            {
                OID = 5,
                Content = "100.200"
            });
            // 6: 馆藏位置 Item Location
            elements.Add(new GaoxiaoUserElement
            {
                OID = 6,
                Content = "中文/english"
            });

            var bytes = GaoxiaoUtility.EncodeUserBank(elements, false);
            var results = GaoxiaoUtility.DecodeUserBank(bytes);

            // 判断是否完全还原
            Assert.AreEqual(results.Count, elements.Count);
            for (int i = 0; i < results.Count; i++)
            {
                Assert.IsTrue(results[i].Equal(elements[i]));
            }
        }


        // 上海交大提供样品标签(1)
        /*
RFU 0000000000000000
EPC C15734001703000398130803F4040000
TID E2003412012CFE000199E4340706012870055FFBFFFFDC50
USR 0C0228081004000100010000000000000000000000000000000000000000
         * */
        [TestMethod]
        public void test_decode_tag_1()
        {
            var epc_bank = Element.FromHexString("C15734001703000398130803F4040000");
            var user_bank = Element.FromHexString("0C0228081004000100010000000000000000000000000000000000000000");
            var result = GaoxiaoUtility.ParseTag(epc_bank, user_bank);

            Assert.AreEqual(true, result.PC.UMI);
            Assert.AreEqual(false, result.PC.XPC);
            Assert.AreEqual(false, result.PC.ISO);
            Assert.AreEqual(0, result.PC.AFI);

            Assert.AreEqual(3, result.LogicChip.Elements.Count);
            Assert.AreEqual("32103179", result.LogicChip.FindElement(ElementOID.PII)?.Text);
            Assert.AreEqual("10248", result.LogicChip.FindElement(ElementOID.OI)?.Text);
            Assert.AreEqual("1/1", result.LogicChip.FindElement(ElementOID.SetInformation)?.Text);

            Assert.AreEqual("32103179", result.EpcInfo.PII);
            Assert.AreEqual(2, result.EpcInfo.ContentParameters.Length);
            Assert.AreEqual(3, result.EpcInfo.ContentParameters[0]);
            Assert.AreEqual(4, result.EpcInfo.ContentParameters[1]);

            // Debug.WriteLine(result.LogicChip.Elements.ToString());
        }

        // 上海交大提供样品标签(2)
        /*
RFU 0000000000000000
EPC B16E34001703000378130803F4040000
TID E20034120139FE000199E1750819013370055FFBFFFFDC50
USR 0C0228081004000100010000000000000000000000000000000000000000
         * */
        [TestMethod]
        public void test_decode_tag_2()
        {
            var epc_bank = Element.FromHexString("B16E34001703000378130803F4040000");
            var user_bank = Element.FromHexString("0C0228081004000100010000000000000000000000000000000000000000");
            var result = GaoxiaoUtility.ParseTag(epc_bank, user_bank);

            Assert.AreEqual(true, result.PC.UMI);
            Assert.AreEqual(false, result.PC.XPC);
            Assert.AreEqual(false, result.PC.ISO);
            Assert.AreEqual(0, result.PC.AFI);

            Assert.AreEqual(3, result.LogicChip.Elements.Count);
            Assert.AreEqual("32103177", result.LogicChip.FindElement(ElementOID.PII)?.Text);
            Assert.AreEqual("10248", result.LogicChip.FindElement(ElementOID.OI)?.Text);
            Assert.AreEqual("1/1", result.LogicChip.FindElement(ElementOID.SetInformation)?.Text);

            Assert.AreEqual("32103177", result.EpcInfo.PII);
            Assert.AreEqual(2, result.EpcInfo.ContentParameters.Length);
            Assert.AreEqual(3, result.EpcInfo.ContentParameters[0]);
            Assert.AreEqual(4, result.EpcInfo.ContentParameters[1]);

            // Debug.WriteLine(result.LogicChip.Elements.ToString());
        }

        // 解码高校 Content Parameter
        // 格式文档上的例子 00a1
        [TestMethod]
        public void test_gaoxiao_contentParameter_1()
        {
            var bytes = new byte[] { 0x00, 0xa1 };
            var results = GaoxiaoUtility.DecodeContentParameter(bytes);
            Assert.AreEqual(3, results.Length);

            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(12, results[1]);
            Assert.AreEqual(15, results[2]);
        }

        // 测试往复编码解码 高校联盟的 Content Parameter 元素内容
        [TestMethod]
        public void test_gaoxiao_contentParameter_2()
        {
            var oid_list = new int[] { 3, 4, 30, 31 };
            var bytes = GaoxiaoUtility.EncodeContentParameter(oid_list);
            Assert.AreEqual(2, bytes.Length);

            var results = GaoxiaoUtility.DecodeContentParameter(bytes);
            Assert.AreEqual(oid_list.Length, results.Length);

            for (int i = 0; i < oid_list.Length; i++)
            {
                Assert.AreEqual(oid_list[i], results[i]);
            }
        }

        // 往复编码解码 Set Information 元素内容部分
        [TestMethod]
        public void test_gaoxiao_userElement_1()
        {
            var bytes = GaoxiaoUtility.EncodeUserElementContent(4, "1/3");
            Assert.AreEqual(4, bytes.Length);

            var result = GaoxiaoUtility.DecodeUserElementContent(4, bytes);
            Assert.AreEqual("1/3", result);
        }

        // 往复编码解码 Type of Usage 元素内容部分
        [TestMethod]
        public void test_gaoxiao_typeofUsage_1()
        {
            var bytes = GaoxiaoUtility.EncodeUserElementContent(5, "10.20");
            Assert.AreEqual(2, bytes.Length);

            var result = GaoxiaoUtility.DecodeUserElementContent(5, bytes);
            Assert.AreEqual("10.20", result);
        }
    }
}
