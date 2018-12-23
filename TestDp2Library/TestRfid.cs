using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.RFID;

namespace TestDp2Library
{
    [TestClass]
    public class TestRfid
    {
        #region integer

        [TestMethod]
        public void Test_rfid_1()
        {
            TestInteger("999999",
                MakeBytes(999999));

            // ‭DE0B6B3A763FFFF‬

            TestInteger("10", new byte[] { (byte)10 });
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
            Debug.Assert(Compress.MaxInteger < UInt64.MaxValue);
            for (UInt64 i = Compress.MaxInteger; i > Compress.MaxInteger - 65535; i--)
            {
                TestInteger(i.ToString(), MakeBytes(i));
            }
        }

        void TestInteger(string text, byte[] correct)
        {
            byte[] result = Compress.IntegerCompress(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compress.IntegerExtract(result));
        }

        // 构造用于判断结果的 byte []
        byte[] MakeBytes(UInt64 v)
        {
            return Compress.TrimLeft(Compress.ReverseBytes(BitConverter.GetBytes(v)));
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
            byte[] result = Compress.DigitCompress(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compress.DigitExtract(result));
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
            byte[] result = Compress.Bit5Compress(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compress.Bit5Extract(result));
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

        void TestBit6(string text, byte[] correct)
        {
            byte[] result = Compress.Bit6Compress(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compress.Bit6Extract(result));
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
            byte[] result = Compress.Bit7Compress(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compress.Bit7Extract(result));
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

        void TestIsilProcess(string text, string process_info)
        {
            StringBuilder debugInfo = new StringBuilder();
            Compress.IsilCompress(text, debugInfo);

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
        }

        void TestIsil(string text, byte[] correct)
        {
            byte[] result = Compress.IsilCompress(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compress.IsilExtract(result));
        }


        #endregion
    }
}
