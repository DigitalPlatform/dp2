using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;

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

            byte[] correct = new byte[] { 0x60, 0xea };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对机构代码 "00001" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_oi_3()
        {
            // 所属馆标识 Owner Library
            var result = GaoxiaoUtility.EncodeUserElementContent(3, "00001");
            byte[] correct = new byte[] { 0x01, 0x00 };
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
            byte[] correct = new byte[] { 0x01, 0x00, 0x03, 0x00 };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对馆藏类别 "1.2" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_tou_1()
        {
            // 馆藏类别与状态 type of usage
            // 总 2 字节定长字段，其中：高字节存放馆藏类别(主限定标识)，低字节存放馆藏状态(次限定标识)
            var result = GaoxiaoUtility.EncodeUserElementContent(5, "1.2");
            byte[] correct = new byte[] { 0x02, 0x01 };
            Assert.AreEqual(Element.GetHexString(correct), Element.GetHexString(result));
        }

        // 对馆藏类别 "1" 编码
        [TestMethod]
        public void Test_EncodeUserElementContent_tou_2()
        {
            // 馆藏类别与状态 type of usage
            // 总 2 字节定长字段，其中：高字节存放馆藏类别(主限定标识)，低字节存放馆藏状态(次限定标识)
            var result = GaoxiaoUtility.EncodeUserElementContent(5, "1");
            byte[] correct = new byte[] { 0x00, 0x01 };
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
            byte[] source = new byte[] { 0x60, 0xea };
            var result = GaoxiaoUtility.DecodeUserElementContent(3, source);
            Assert.AreEqual("90000", result);
        }

        // 对机构代码 "000001" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_oi_2()
        {
            byte[] source = new byte[] { 0x01, 0x00 };
            var result = GaoxiaoUtility.DecodeUserElementContent(3, source);
            Assert.AreEqual("00001", result);
        }

        // 对卷册信息 "1/3" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_si_1()
        {
            byte[] source = new byte[] { 0x01, 0x00, 0x03, 0x00 };
            var result = GaoxiaoUtility.DecodeUserElementContent(4, source);
            Assert.AreEqual("1/3", result);
        }

        // 对馆藏类别 "1.2" 解码
        [TestMethod]
        public void Test_DecodeUserElementContent_tou_1()
        {
            byte[] source = new byte[] { 0x02, 0x01 };
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
    }
}
