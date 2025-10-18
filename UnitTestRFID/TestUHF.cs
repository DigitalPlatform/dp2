using DigitalPlatform;
using DigitalPlatform.RFID;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DigitalPlatform.RFID.LogicChip;
using static DigitalPlatform.RFID.RfidTagList;

namespace UnitTestRFID
{

    [TestClass]
    public class TestUHF
    {
        [TestMethod]
        public void Test_encode_longNumericString()
        {
            // 以 long numeric string 方式编码一个数字字符串
            var bytes = UhfUtility.EncodeLongNumericString("12312345678");

            byte[] correct = Element.FromHexString(
@"FB 21 02 DD DF 7C 4E");

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }


        [TestMethod]
        public void Test_decode_longNumericString()
        {
            byte[] source = Element.FromHexString(
@"FB 21 02 DD DF 7C 4E");
            string result = UhfUtility.DecodeLongNumericString(source, 0, out int used);

            // 
            Assert.AreEqual("12312345678", result);
            Assert.AreEqual(source.Length, used);
        }

        // 测试编码 UII
        [TestMethod]
        public void Test_encode_uii_1()
        {
            TestEncodeUII("CH-", "141c");

            TestEncodeUII("000", "c04f");

            TestEncodeUII("134", "c70b");

            TestEncodeUII("-1.", "adb5");

            TestEncodeUII("123", "c6e2");

            TestEncodeUII("456", "da1d");

            TestEncodeUII("78.", "ed4d");

            TestEncodeUII("31", "d319");
        }

        static void TestEncodeUII(string uii, string hex)
        {
            var bytes = UhfUtility.EncodeUII(uii);

            byte[] correct = Element.FromHexString(hex);

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }

        // 测试编码 UII
        // ISO/TS 28560-4:2014(E) page 49
        [TestMethod]
        public void Test_encode_uii_2()
        {
            var bytes = UhfUtility.EncodeUII("CH-000134-1.12345678.31");

            byte[] correct = Element.FromHexString(
@"141c c04f c70b adb5 c6e2 da1d ed4d d319");

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }

        // page 17 of:
        // https://www.ipc.be/~/media/documents/public/operations/rfid/ipc%20rfid%20standard%20for%20test%20letters.pdf?la=en
        [TestMethod]
        public void Test_encode_uii_3()
        {
            var bytes = UhfUtility.EncodeUII("B.A12312345678", true);

            byte[] correct = Element.FromHexString(
@"10 E2 FB 21 02 DD DF 7C 4E 00");  // 注意补齐偶数，最后增加了一个 0 byte

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }

        // 测试解码 UII
        [TestMethod]
        public void Test_decode_uii_1()
        {
            TestDecodeUii("141c", "CH-");
            TestDecodeUii("c04f", "000");
            TestDecodeUii("c70b", "134");
            TestDecodeUii("adb5", "-1.");
            TestDecodeUii("c6e2", "123");
            TestDecodeUii("da1d", "456");
            TestDecodeUii("ed4d", "78.");
            TestDecodeUii("d319", "31");
        }

        static void TestDecodeUii(string hex, string text)
        {
            byte[] source = Element.FromHexString(hex);

            var result = UhfUtility.DecodeUII(source, 0, source.Length);

            // 
            Assert.AreEqual(text, result);
        }

        // 测试解码 UII
        // ISO/TS 28560-4:2014(E) page 49
        [TestMethod]
        public void Test_decode_uii_2()
        {
            byte[] source = Element.FromHexString(
@"141c c04f c70b adb5 c6e2 da1d ed4d d319");

            var result = UhfUtility.DecodeUII(source, 0, source.Length);

            // 
            Assert.AreEqual("CH-000134-1.12345678.31", result);
        }

        // 测试包含小写字母的情形
        [TestMethod]
        public void Test_decode_uii_3()
        {
            var bytes = UhfUtility.EncodeUII("aaaaa");
            var result = UhfUtility.DecodeUII(bytes, 0, bytes.Length);
            Assert.AreEqual("aaaaa", result);
        }

        // 9 字符才会形成 digit 类型
        [TestMethod]
        public void Test_splitSegment_01()
        {
            var segments = UhfUtility.SplitSegment("123456789");

            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual("digit", segments[0].Type);
            Assert.AreEqual("123456789", segments[0].Text);
        }

        // 8 字符只能形成 table 类型
        [TestMethod]
        public void Test_splitSegment_02()
        {
            var segments = UhfUtility.SplitSegment("12345678");

            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("12345678", segments[0].Text);
        }

        [TestMethod]
        public void Test_splitSegment_03()
        {
            var segments = UhfUtility.SplitSegment("AB.123456789");

            Assert.AreEqual(2, segments.Count);

            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("AB.", segments[0].Text);

            Assert.AreEqual("digit", segments[1].Type);
            Assert.AreEqual("123456789", segments[1].Text);
        }

        [TestMethod]
        public void Test_splitSegment_04()
        {
            var segments = UhfUtility.SplitSegment("123456789AB.");

            Assert.AreEqual(2, segments.Count);

            Assert.AreEqual("digit", segments[0].Type);
            Assert.AreEqual("123456789", segments[0].Text);

            Assert.AreEqual("table", segments[1].Type);
            Assert.AreEqual("AB.", segments[1].Text);
        }

        [TestMethod]
        public void Test_splitSegment_05()
        {
            var segments = UhfUtility.SplitSegment("1234/56789");

            Assert.AreEqual(3, segments.Count);

            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("1234", segments[0].Text);

            Assert.AreEqual("utf8-one-byte", segments[1].Type);
            Assert.AreEqual("/", segments[1].Text);

            Assert.AreEqual("table", segments[2].Type);
            Assert.AreEqual("56789", segments[2].Text);
        }

        // 含有 UTF-8 汉字字符
        [TestMethod]
        public void Test_splitSegment_06()
        {
            var segments = UhfUtility.SplitSegment("1234中国56789");

            Assert.AreEqual(3, segments.Count);

            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("1234", segments[0].Text);

            Assert.AreEqual("utf8-triple-byte", segments[1].Type);
            Assert.AreEqual("中国", segments[1].Text);

            Assert.AreEqual("table", segments[2].Type);
            Assert.AreEqual("56789", segments[2].Text);
        }

        [TestMethod]
        public void Test_splitSegment_07()
        {
            var segments = UhfUtility.SplitSegment("78.");

            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("78.", segments[0].Text);
        }

        // 2025/2/28
        [TestMethod]
        public void Test_splitSegment_08()
        {
            var segments = UhfUtility.SplitSegment("000387624");

            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("000387624", segments[0].Text);
        }

        // 2025/2/28
        [TestMethod]
        public void Test_splitSegment_09()
        {
            var segments = UhfUtility.SplitSegment("CN-0000001-ZG.000387624");

            Assert.AreEqual(1, segments.Count);
            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("CN-0000001-ZG.000387624", segments[0].Text);
        }

        // 2025/2/28
        [TestMethod]
        public void Test_splitSegment_10()
        {
            var segments = UhfUtility.SplitSegment("CN-0000001-ZG.387624000");

            Assert.AreEqual(2, segments.Count);
            Assert.AreEqual("table", segments[0].Type);
            Assert.AreEqual("CN-0000001-ZG.", segments[0].Text);
            Assert.AreEqual("digit", segments[1].Type);
            Assert.AreEqual("387624000", segments[1].Text);
        }

        // 

        // 编码 MB11 (User Bank)
        // ISO/TS 28560-4:2014(E) page 52
        [TestMethod]
        public void Test_encode_userbank_1()
        {
            LogicChip chip = new LogicChip();
            chip.NewElement(ElementOID.SetInformation, "1203");
            chip.NewElement(ElementOID.ShelfLocation, "QA268.L55");
            chip.NewElement(ElementOID.OwnerInstitution, "US-InU-Mu").CompactMethod = CompactionScheme.SevenBitCode;    // 如果让 GetBytes() 自动选择压缩方案，这个元素会被选择 ISIL 压缩方案
            Debug.Write(chip.ToString());

            var result = chip.GetBytes(4 * 9,
                4,
                GetBytesStyle.ReserveSequence,
                out string block_map);
            string result_string = Element.GetHexString(result, "4");
            byte[] correct = Element.FromHexString(
    @"02 01 D0 14 02
04B3 4607
441C b6E2
E335 D653
08AB 4D6C
9DD5 56CD
EB"
);
            Assert.IsTrue(result.SequenceEqual(correct));

            // Assert.AreEqual(block_map, "ww....www");

        }


        // 编码 MB11 (User Bank)
        // ISO/TS 28560-4:2014(E) page 52
        [TestMethod]
        public void Test_encode_userbank_2()
        {
            LogicChip chip = new LogicChip();
            chip.NewElement(ElementOID.SetInformation, "1203");
            chip.NewElement(ElementOID.ShelfLocation, "QA268.L55");
            chip.NewElement(ElementOID.OwnerInstitution, "US-InU-Mu").CompactMethod = CompactionScheme.SevenBitCode;    // 如果让 GetBytes() 自动选择压缩方案，这个元素会被选择 ISIL 压缩方案
            Debug.Write(chip.ToString());

            var result = UhfUtility.EncodeUserBank(chip,
                4 * 52,
                4,
                true,
                out string block_map);
            string result_string = Element.GetHexString(result, "4");
            byte[] correct = Element.FromHexString(
    @"06 02 01 D0 14 02
04B3 4607
441C b6E2
E335 D653
08AB 4D6C
9DD5 56CD
EB00"
);
            Assert.IsTrue(result.SequenceEqual(correct));

            // Assert.AreEqual(block_map, "ww....www");

        }

        [TestMethod]
        public void Test_decode_userbank_1()
        {
            byte[] userbank = Element.FromHexString(
@"06 02 01 D0 14 02
04B3 4607
441C b6E2
E335 D653
08AB 4D6C
9DD5 56CD
EB00"
);
            /*
            // 解码 User Bank，和解码高频标签的 Bytes 一样
            List<byte> temp = new List<byte>(userbank);
            temp.RemoveAt(0);

            var chip = LogicChip.From(temp.ToArray(),
                4);
            */
            var result = UhfUtility.ParseUserBank(userbank, 4);

            Assert.AreEqual(0, result.Value);

            var chip = result.LogicChip;

            Assert.AreEqual(4, chip.Elements.Count);    // 3 + 还有一个 Content Parameter 元素

            // 验证 Content Parameter
            var contentParameter = chip.FindElement(ElementOID.ContentParameter)?.Content;
            string description = Element.GetContentParameterDesription(contentParameter);
            Assert.AreEqual("OwnerInstitution,SetInformation,ShelfLocation", description);

            Assert.AreEqual("1203",
                chip.FindElement(ElementOID.SetInformation)?.Text);
            Assert.AreEqual("QA268.L55",
                chip.FindElement(ElementOID.ShelfLocation)?.Text);
            Assert.AreEqual("US-InU-Mu",
                chip.FindElement(ElementOID.OwnerInstitution)?.Text);
        }

#if REMOVED
        [TestMethod]
        public void Test_decode_userbank_2()
        {
            byte[] userbank = Element.FromHexString(
@"0c02d9941404000100012c0038000000000000000000000000000000000000000000000000000000000000000000000000000100000000000000000000000000");
            /*
            // 解码 User Bank，和解码高频标签的 Bytes 一样
            List<byte> temp = new List<byte>(userbank);
            temp.RemoveAt(0);

            var chip = LogicChip.From(temp.ToArray(),
                4);
            */
            var result = UhfUtility.ParseUserBank(userbank, 4);

            Assert.AreEqual(0, result.Value);

            /*
            var chip = result.LogicChip;

            Assert.AreEqual(4, chip.Elements.Count);    // 3 + 还有一个 Content Parameter 元素

            // 验证 Content Parameter
            var contentParameter = chip.FindElement(ElementOID.ContentParameter)?.Content;
            string description = Element.GetContentParameterDesription(contentParameter);
            Assert.AreEqual("OwnerInstitution,SetInformation,ShelfLocation", description);

            Assert.AreEqual("1203",
                chip.FindElement(ElementOID.SetInformation)?.Text);
            Assert.AreEqual("QA268.L55",
                chip.FindElement(ElementOID.ShelfLocation)?.Text);
            Assert.AreEqual("US-InU-Mu",
                chip.FindElement(ElementOID.OwnerInstitution)?.Text);
            */
        }
#endif

        // 编码 EPC bank
        [TestMethod]
        public void Test_encode_epcbank_1()
        {
            var bytes = UhfUtility.EncodeEpcBank("CH-000134-1.12345678.31");

            byte[] correct = Element.FromHexString(
@"45c2 141c c04f c70b adb5 c6e2 da1d ed4d d319");

            // 
            Assert.AreEqual(0, ByteArray.Compare(correct, bytes));
        }

        // 解码 EPC Bank
        [TestMethod]
        public void Test_decode_epcbank_1()
        {
            byte[] epc_bank = Element.FromHexString(
@"0000 45c2 141c c04f c70b adb5 c6e2 da1d ed4d d319");
            var result = UhfUtility.ParseEpcBank(epc_bank);

            Assert.AreEqual(0, result.Value);
            var pc = result.PC;
            Assert.AreEqual(true, pc.UMI);
            Assert.AreEqual(false, pc.XPC);
            Assert.AreEqual(true, pc.ISO);
            Assert.AreEqual(0xc2, pc.AFI);
            Assert.AreEqual((epc_bank.Length - 2/*校验码*/ - 2/*PC*/) / 2, pc.LengthIndicator);

            Assert.AreEqual("CH-000134-1.12345678.31", result.UII);
        }

        // 不明格式尝试
        [TestMethod]
        public void display_epc_1()
        {
            var crc = Element.FromHexString("C41E");

            // 协议控制字 Protocol Control Word
            var pc = UhfUtility.ParsePC(crc, 0);
            Debug.WriteLine(pc.ToString());

            var bytes = Element.FromHexString("300833B2DDD901400000000000000000");
            GaoxiaoEpcInfo info = GaoxiaoUtility.DecodeGaoxiaoEpcPayload(bytes, bytes.Length);
            Debug.WriteLine(info.ToString());
        }

        // 
        // 不明格式尝试
        // 厂家 02
        [TestMethod]
        public void display_epc_2()
        {
            var epc_bank = Element.FromHexString("A3623D07150CC6B7C697ADB45964B3CFC1C123EF");
            var result = UhfUtility.ParseTag(epc_bank,
                null,
                4);

            Debug.WriteLine(result.ToString());
        }

        // 
        // 不明格式尝试
        // 厂家 03
        /*
RFU 0000000000000000
EPC DAD53000170328087903000084560000
TID E2003412013D03000BB5808103160108700D5FFBFFFFDC50
USR 000879030000845600000C0228081004000100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
         * */
        [TestMethod]
        public void display_epc_3()
        {
            var epc_bank = Element.FromHexString("DAD53000170328087903000084560000");
            var user_bank = Element.FromHexString("000879030000845600000C0228081004000100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            var result = GaoxiaoUtility.ParseTag(epc_bank,
                user_bank,
                "");

            Debug.WriteLine(result.ToString());
        }

        // hh 厂家
        // 24E6 3400 8003 0053 3813 0058 B907 0000
        [TestMethod]
        public void display_epc_4()
        {
            var epc_bank = Element.FromHexString("24E634008003005338130058B9070000");
            var user_bank = Element.FromHexString("0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            var result = GaoxiaoUtility.ParseTag(epc_bank,
                user_bank,
                "");

            Debug.WriteLine(result.ToString());
        }

        [TestMethod]
        public void test_overwriteBank_01()
        {
            var old_bytes = new byte[] { 0, 0 };
            var new_bytes = new byte[] { 1, 1 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            Assert.AreEqual(2, result_bytes.Length);
            AssertBytes(result_bytes, new_bytes);
        }

        // old 延展部分多出来一些 0 byte。不影响 new
        [TestMethod]
        public void test_overwriteBank_02()
        {
            var old_bytes = new byte[] { 0, 0, 0 };
            var new_bytes = new byte[] { 1, 1 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            Assert.AreEqual(2, result_bytes.Length);
            AssertBytes(result_bytes, new_bytes);
        }

        // old 延展部分多出来一些非 0 byte。new 因此要延长一部分，以便可以有效覆盖这部分 old 内容
        [TestMethod]
        public void test_overwriteBank_03()
        {
            var old_bytes = new byte[] { 0, 0, 3 };
            var new_bytes = new byte[] { 1, 2 };
            var correct_bytes = new byte[] { 1, 2, 0 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            AssertBytes(correct_bytes, result_bytes);
        }

        // old 延展部分多出来一些非 0 byte。new 因此要延长一部分，以便可以有效覆盖这部分 old 内容
        [TestMethod]
        public void test_overwriteBank_04()
        {
            var old_bytes = new byte[] { 0, 0, 3, 0 };
            var new_bytes = new byte[] { 1, 2 };
            var correct_bytes = new byte[] { 1, 2, 0 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            AssertBytes(correct_bytes, result_bytes);
        }

        // old 延展部分多出来一些非 0 byte。new 因此要延长一部分，以便可以有效覆盖这部分 old 内容
        [TestMethod]
        public void test_overwriteBank_05()
        {
            var old_bytes = new byte[] { 0, 0, 0, 4 };
            var new_bytes = new byte[] { 1, 2 };
            var correct_bytes = new byte[] { 1, 2, 0, 0 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            AssertBytes(correct_bytes, result_bytes);
        }

        // 特殊情况测试
        [TestMethod]
        public void test_overwriteBank_11()
        {
            byte[] old_bytes = null;
            var new_bytes = new byte[] { 1, 2 };
            var correct_bytes = new byte[] { 1, 2 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            AssertBytes(correct_bytes, result_bytes);
        }

        [TestMethod]
        public void test_overwriteBank_12()
        {
            byte[] old_bytes = new byte[] { 1, 2 };
            byte[] new_bytes = null;
            var correct_bytes = new byte[] { 0, 0 };
            var result_bytes = UhfUtility.OverwriteBank(old_bytes, new_bytes);
            AssertBytes(correct_bytes, result_bytes);
        }

        void AssertBytes(byte[] bytes1, byte[] bytes2)
        {
            Assert.AreEqual(bytes1.Length, bytes2.Length);
            for (int i = 0; i < bytes1.Length; i++)
            {
                Assert.AreEqual(bytes1[i], bytes2[i]);
            }
        }

        [TestMethod]
        public void test_parseSTID_01()
        {
            // https://www.gs1.org/services/tid-decoder
            var bytes = ByteArray.GetTimeStampByteArray(
                "E2FFF0403C00123456789ABC1DD60000090400000904000400C0");
            var result = ExtendedTagIdentification.Parse(bytes);
            Assert.AreEqual(0xfff, result.MaskDesignerID);
            Assert.AreEqual(0x040, result.TagModelNumber);
            Assert.AreEqual(true, result.XtidIndicator);
            Assert.AreEqual(true, result.SecurityIndicator);
            Assert.AreEqual(true, result.FileIndicator);
            Assert.AreEqual("urn:epc:stid:xFFF.x040.x123456789ABC", result.StidUri);
            Assert.AreEqual((UInt64)0x123456789ABC, result.SerialNumberInteger);
        }

        [TestMethod]
        public void test_parseSTID_02()
        {
            var bytes = ByteArray.GetTimeStampByteArray(
                "e20034120137fd0009f15cdb04190135300d5ffbffffdc60");
            var result = ExtendedTagIdentification.Parse(bytes);
            Assert.AreEqual(0x003, result.MaskDesignerID);    // Alien Technology
            Assert.AreEqual(0x412, result.TagModelNumber);
            Assert.AreEqual(false, result.XtidIndicator);
            Assert.AreEqual(false, result.SecurityIndicator);
            Assert.AreEqual(false, result.FileIndicator);
            Assert.AreEqual(null, result.StidUri);
            Assert.AreEqual((UInt64)0x0, result.SerialNumberInteger);

        }

        [TestMethod]
        public void test_detect_01()
        {
            byte[] epc_bank = Element.FromHexString(
@"1303 2907 CD22 D385 114F C04F C0D1");

            var isGB = UhfUtility.IsISO285604Format(epc_bank, null);

            var parse_result = GaoxiaoUtility.ParseTag(
epc_bank,
null); 
            if (parse_result.Value == -1)
            {
                if (parse_result.ErrorCode == "parseEpcError"
                    || parse_result.ErrorCode == "parseError")
                {
                    return new ChipInfo
                    {
                        ErrorInfo = parse_result.ErrorInfo,
                        // TODO: 为 result 增加 ErrorCode
                    };
                }

                // throw new Exception(parse_result.ErrorInfo);
                result.ErrorInfo = parse_result.ErrorInfo;
                return result;
            }

        }
    }
}
