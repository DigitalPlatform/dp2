using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;
using DigitalPlatform;

namespace TestDp2Library
{
    [TestClass]
    public class TestRfid
    {
        #region integer

        [TestMethod]
        public void Test_integer_1()
        {
            TestInteger("999999",
                MakeBytes(999999));

            // ‭DE0B6B3A763FFFF‬

        }

        [TestMethod]
        public void Test_integer_2()
        {
            TestInteger("10", new byte[] { (byte)10 });
        }

        [TestMethod]
        public void Test_integer_3()
        {
            // GB/T 35660.2-2017 page 31 例子
            TestInteger("123456789012",
                new byte[] { 0x1c, 0xbe, 0x99, 0x1a, 0x14 });
        }

        [TestMethod]
        public void Test_integer_4()
        {
            // GB/T 35660.2-2017 page 32 例子
            TestInteger("1203", new byte[] { 0x04, 0xb3 });
        }

        // 测试一些较小的数值
        [TestMethod]
        public void Test_loop_integer_1()
        {
            for (UInt64 i = 10; i < 65535; i++)
            {
                TestInteger(i.ToString(), MakeBytes(i));
            }
        }

        // 测试一些较大的数值
        [TestMethod]
        public void Test_loop_integer_2()
        {
            Debug.Assert(Compact.MaxInteger < UInt64.MaxValue);
            for (UInt64 i = Compact.MaxInteger; i > Compact.MaxInteger - 65535; i--)
            {
                TestInteger(i.ToString(), MakeBytes(i));
            }
        }

        void TestInteger(string text, byte[] correct)
        {
            byte[] result = Compact.IntegerCompact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.IntegerExtract(result));
        }

        // 构造用于判断结果的 byte []
        byte[] MakeBytes(UInt64 v)
        {
            return Compact.TrimLeft(Compact.ReverseBytes(BitConverter.GetBytes(v)));
        }

        #endregion

        #region digit

        [TestMethod]
        public void Test_digit_1()
        {
            TestDigit("00", new byte[] { 0 });
        }

        [TestMethod]
        public void Test_digit_2()
        {
            TestDigit("01", new byte[] { 1 });
        }

        [TestMethod]
        public void Test_digit_3()
        {
            TestDigit("010", new byte[] { 1, 0xf });
        }

        void TestDigit(string text, byte[] correct)
        {
            byte[] result = Compact.NumericCompact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.NumericExtract(result));
        }

        #endregion

        #region bit5

        [TestMethod]
        public void Test_bit5_1()
        {
            // 41, 42, 43 --> 0100 0001, 0100 0010, 0100 0011
            // 删除每个 byte 的前三个 bit，得到 bit 串: 0 0001, 0 0010, 0 0011
            // 按照 8 bit 边界排列: 0 0001 0 00, 10 0 0011 补 0 --> 0000 1000, 1000 0110
            // 0x08 0x86
            TestBit5("ABC", new byte[] { 0x8, 0x86 });
        }

        [TestMethod]
        public void Test_bit5_2()
        {
            // 5d, 5e, 5f --> 0101 1101, 0101 1110, 0101 1111
            // 删除每个 byte 的前三个 bit，得到 bit 串: 1 1101, 1 1110, 1 1111
            // 按照 8 bit 边界排列: 1 1101 1 11, 10 1 1111 补 0 --> 1110 1111, 1011 1110
            // 0xef 0xbe
            TestBit5("]^_", new byte[] { 0xef, 0xbe });
        }

        void TestBit5(string text, byte[] correct)
        {
            byte[] result = Compact.Bit5Compact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.Bit5Extract(result));
        }

        #endregion


        #region bit6

        [TestMethod]
        public void Test_bit6_1()
        {
            // GB/T 35660.2-2017 page 39 例子:
            // 41, 42, 43, 31, 32, 33, 34, 35 
            // 二进制表示: (A)0100 0001, (B)0100 0010, (C)0100 0011, (1)0011 0001, (2)0011 0010, (3)0011 0011, (4)0011 0100, (5)0011 0101, (6)0011, 0110
            // 删除每个 byte 的前 2个 bit，得到 bit 串:
            // 00 0001, 00 0010, 00 0011, 11 0001, 11 0010, 11 0011, 11 0100, 11 0101, 11 0110
            // 按照 8 bit 边界排列:
            // 00000100, 00100000, 11110001, 11001011, 00111101, 00110101, 110110 (补)10
            // 翻译为 hex 表示法：0x04, 0x20, 0xf1, 0xcb, 0x3d, 0x35, 0xda
            TestBit6("ABC123456", new byte[] {
                0x04, 0x20, 0xf1, 0xcb, 0x3d, 0x35, 0xda
            });
        }

        [TestMethod]
        public void Test_bit6_2()
        {
            // 最后补齐 6 bits 的情况
            // 41, 42, 43, 31, 32, 33, 34 
            // 二进制表示: (A)0100 0001, (B)0100 0010, (C)0100 0011, (1)0011 0001, (2)0011 0010, (3)0011 0011, (4)0011 0100
            // 删除每个 byte 的前 2个 bit，得到 bit 串:
            // 00 0001, 00 0010, 00 0011, 11 0001, 11 0010, 11 0011, 11 0100
            // 按照 8 bit 边界排列:
            // 00000100, 00100000, 11110001, 11001011, 00111101, 00 (补)100000
            // 翻译为 hex 表示法：0x04, 0x20, 0xf1, 0xcb, 0x3d, 0x20
            TestBit6("ABC1234", new byte[] {
                0x04, 0x20, 0xf1, 0xcb, 0x3d, 0x20
            });
        }

        [TestMethod]
        public void Test_bit6_3()
        {
            // GB/T 35660.2-2017 page 32 例子
            TestBit6("QA268.L55", new byte[] {
                0x44, 0x1c, 0xb6, 0xe2, 0xe3, 0x35, 0xd6
            });
        }

        void TestBit6(string text, byte[] correct)
        {
            byte[] result = Compact.Bit6Compact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.Bit6Extract(result));
        }

        #endregion


        #region bit7

        [TestMethod]
        public void Test_bit7_1()
        {
            // 最后正好对齐 8 bits 边界
            // 31, 32, 33, 34, 35, 36, 37, 38 
            // 二进制表示: (1)0011 0001, (2)0011 0010, (3)0011 0011, (4)0011 0100, (5)0011 0101, (6)0011, 0110, (7)0011 0111, (8)0011 1000
            // 删除每个 byte 的前 1 个 bit，得到 bit 串:
            // (1)011 0001, (2)011 0010, (3)011 0011, (4)011 0100, (5)011 0101, (6)011, 0110, (7)011 0111, (8)011 1000
            // 按照 8 bit 边界排列:
            // 0110 0010, 1100 1001, 1001 1011, 0100 0110, 1010 1101, 1001 1011, 1011 1000
            // 翻译为 hex 表示法：
            // 0x62,      0xc9,      0x9b,      0x46,      0xad,      0x9b,      0xb8
            TestBit7("12345678", new byte[] {
                0x62,0xc9,0x9b,0x46,0xad,0x9b,0xb8
            });
        }

        [TestMethod]
        public void Test_bit7_2()
        {
            // 测试最后有 7 bits 补齐的情况
            TestBit7("123456781234567", new byte[] {
                0x62,0xc9,0x9b,0x46,0xad,0x9b,0xb8,
                0x62,0xc9,0x9b,0x46,0xad,0x9b,0xff
            });
        }

        void TestBit7(string text, byte[] correct)
        {
            byte[] result = Compact.Bit7Compact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.Bit7Extract(result));
        }

        #endregion


        #region ISIL

        [TestMethod]
        public void Test_isil_process_1()
        {
            TestIsilProcess("DE-Heu1",
                @"D output:00100
E output:00101
- output:00000
H output:01000
e switch:u-l output:11100,00101
u output:10101
1 shift:l-d output:11111,0001");
        }

        [TestMethod]
        public void Test_isil_process_2()
        {
            TestIsilProcess("-",
@"- output:00000");

            TestIsilProcess("A",
                @"A output:00001");

            TestIsilProcess("B",
    @"B output:00010");

            TestIsilProcess("Z",
@"Z output:11010");

            TestIsilProcess(":",
@": output:11011");
        }

        [TestMethod]
        public void Test_isil_process_3()
        {
            TestIsilProcess("-",
@"- output:00000");

            TestIsilProcess("a",
                @"a shift:u-l output:11101,00001");

            TestIsilProcess("b",
    @"b shift:u-l output:11101,00010");

            TestIsilProcess("z",
@"z shift:u-l output:11101,11010");

            TestIsilProcess("/",
@"/ shift:u-l output:11101,11011");
        }

        // 新增
        [TestMethod]        public void Test_isil_process_6()        {            TestIsilProcess("CN-120104-C-YBL",                @"C output:00011N output:01110- output:000001 switch:u-d output:11110,00012 output:00100 output:00001 output:00010 output:00004 output:0100- output:1010C switch:d-u output:1100,00011- output:00000Y output:11001B output:00010L output:01100");        }        [TestMethod]        public void Test_isil_process_7()        {            TestIsilProcess("C-YBL",                @"C output:00011- output:00000Y output:11001B output:00010L output:01100");        }

        // 测试 ISIL 基本逻辑处理是否正确。
        // 注意，本测试并未验证 Compact 生成的 byte[] 是否正确，也未验证 Extract 部分功能
        void TestIsilProcess(string text, string process_info)
        {
            StringBuilder debugInfo = new StringBuilder();
            Compact.IsilCompact(text, debugInfo);

            // 去掉末尾的回行
            process_info = process_info.TrimEnd(new char[] { '\r', '\n' });
            string result = debugInfo.ToString().TrimEnd(new char[] { '\r', '\n' });
            Assert.AreEqual(result, process_info);
        }

        [TestMethod]
        public void Test_isil_1()
        {
            TestIsil("DE-Heu1", new byte[] {
                0x21,0x40,0x8e,0x16,0xbf,0x1f
            });
        }

        [TestMethod]
        public void Test_isil_2()
        {
            //StringBuilder debugInfo = new StringBuilder();
            //Compress.IsilCompress("US-InU-Mu", debugInfo);

            TestIsil("US-InU-Mu", new byte[] {
                0xac,0xc0,0x9e,0xba,0xa0,0x6f,0x6b
            });
        }

        void TestIsil(string text, byte[] correct)
        {
            byte[] result = Compact.IsilCompact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.IsilExtract(result));
        }

        #endregion

        #region AutoSelectCompactMethod()

        [TestMethod]
        public void Test_autoSelect_1()
        {
            // page 31 例子
            Assert.AreEqual(
                CompactionScheme.Integer,
                Compact.AutoSelectCompactMethod("123456789012"));
        }

        [TestMethod]
        public void Test_autoSelect_2()
        {
            // page 32 例子
            Assert.AreEqual(
                CompactionScheme.Integer,
                Compact.AutoSelectCompactMethod("1203"));
        }

        [TestMethod]
        public void Test_autoSelect_5()
        {
            Assert.AreEqual(
                CompactionScheme.SixBitCode,
                Compact.AutoSelectCompactMethod("QA268.L55"));
        }

        #endregion

        #region Element

        [TestMethod]
        public void Test_element_parse_1()
        {
            byte[] data = new byte[] {
                0x91, 0x00, 0x05, 0x1c,
                0xbe, 0x99, 0x1a, 0x14,
            };
            var element = Element.Parse(data, 0, out int bytes);
            Assert.AreEqual((int)element.OID, 1);
            Assert.AreEqual(element.Text, "123456789012");
            Assert.AreEqual(element.PrecursorOffset, true); // Precursor 后面会有 1 byte 的填充字符数
            Assert.AreEqual(element.Paddings, 0);   // 填充 byte 没有使用
        }

        [TestMethod]
        public void Test_element_compact_1()
        {
            byte[] result = Element.Compact(1,
                "123456789012",
                CompactionScheme.Null,
                true);
            byte[] correct = new byte[] {
                0x91, 0x00, 0x05, 0x1c,
                0xbe, 0x99, 0x1a, 0x14,
            };
            Assert.IsTrue(result.SequenceEqual(correct));
        }

        #endregion


        #region LogicChip

        [TestMethod]
        public void test_logicChip_1()
        {
            // zhiyan 1
            byte[] data = ByteArray.GetTimeStampByteArray("C102071100B0C30C30CA00000203A80008830203D6593F0000250110370210405F080599A713063F");
            LogicChip chip = LogicChip.From(data, 4);
            Debug.Write(chip.ToString());
        }

        [TestMethod]
        public void test_logicChip_2()
        {
            // zhiyan 2
            byte[] data = ByteArray.GetTimeStampByteArray("C102071100B0C30C30D600000203A80008830203D6593F0000250110370210405F080599A713063F");
            LogicChip chip = LogicChip.From(data, 4);

        }

        [TestMethod]
        public void test_logicChip_3()
        {
            // jiangxi jingyuan 1
            byte[] data = ByteArray.GetTimeStampByteArray("11030AA8AE0000000000000000000000000000000000000000000000000000000000000000000000");
            LogicChip chip = LogicChip.From(data, 4);
        }

        [TestMethod]
        public void test_logicChip_4()
        {
            // jiangxi jingyuan 2
            byte[] data = ByteArray.GetTimeStampByteArray("11030AA9770000000000000000000000000000000000000000000000000000000000000000000000");
            LogicChip chip = LogicChip.From(data, 4);
        }

        [TestMethod]
        public void test_logicChip_5()
        {
            // 
            byte[] data = ByteArray.GetTimeStampByteArray("C102071100B0C30C30C600000203A80008830203D6593F0000250110370210405F080599A713063F");
            LogicChip chip = LogicChip.From(data, 4);
        }

        [TestMethod]
        public void test_logicChip_6()
        {
            // ganchuang 1
            byte[] data = ByteArray.GetTimeStampByteArray("9101040142214B000201B80300650110660100670100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            LogicChip chip = LogicChip.From(data, 4);
            Debug.Write(chip.ToString());

        }

        [TestMethod]
        public void test_logicChip_7()
        {
            // ganchuang 2
            byte[] data = ByteArray.GetTimeStampByteArray("91020312D68700000201B80300650110660100670100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            LogicChip chip = LogicChip.From(data, 4);
            Debug.Write(chip.ToString());

        }

        #endregion

    }
}
