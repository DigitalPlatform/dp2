using System;
using System.Diagnostics;
using DigitalPlatform.RFID;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using DigitalPlatform;
using System.Text;
using System.Collections.Generic;

namespace UnitTestProject2
{
    [TestClass]
    public class TestRfid2
    {

        #region integer



        // 占2个字节
        [TestMethod]
        public void Test_integer_5()
        {
            TestInteger("256", new byte[] { 0x01, 0x00 });
        }

        // 占3个字节
        [TestMethod]
        public void Test_integer_6()
        {
            TestInteger("65536", new byte[] { 0x01, 0x00, 0x00 });
        }

        // 占4个字节
        [TestMethod]
        public void Test_integer_7()
        {
            TestInteger("4294967296", new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00});
        }

        // int64的最后一个数
        [TestMethod]
        public void Test_integer_8()
        {
            TestInteger("9223372036854775807", new byte[] { 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF});
        }

        
        // uint64最后一个数
        [TestMethod]
        public void Test_integer_9()
        {
            TestInteger("9999999999999999999", new byte[] { 0x8a, 0xc7, 0x23, 0x04, 0x89, 0xe7, 0xff, 0xff });
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

        #region 检查是否在范围


        // 不在范围的数字，小于10
        [TestMethod]
        public void Check_integer_1()
        {
            CheckInteger("1");
        }

        // 不在范围的数字,首字符为"0"
        [TestMethod]
        public void Check_integer_2()
        {
            CheckInteger("01");
        }

        // 不在范围的数字,超过19位
        [TestMethod]
        public void Check_integer_3()
        {
            CheckInteger("100000000000000000000");
        }

        // 不在范围的数字,负数
        [TestMethod]
        public void Check_integer_4()
        {
            CheckInteger("-1");
        }

        // 不在范围的数字,字母
        [TestMethod]
        public void Check_integer_5()
        {
            CheckInteger("A");
        }

        // 不在范围的数字,汉字
        [TestMethod]
        public void Check_integer_6()
        {
            CheckInteger("中国");
        }

        // 不在范围的数字,符号
        [TestMethod]
        public void Check_integer_7()
        {
            CheckInteger("/");
        }


        void CheckInteger(string text)
        {
            bool bRet = Compact.CheckInteger(text,false);

            Assert.IsTrue(bRet==false);
        }


        #endregion



        #endregion


        #region numeric




        [TestMethod]
        public void Test_numeric_4()
        {
            TestNumeric("99", new byte[] { 0x99 });
        }


        [TestMethod]
        public void Test_numeric_5()
        {
            TestNumeric("091", new byte[] { 0x09, 0x1f });
        }

        [TestMethod]
        public void Test_numeric_6()
        {
            TestNumeric("256", new byte[] { 0x25, 0x6f });
        }

        [TestMethod]
        public void Test_numeric_7()
        {
            TestNumeric("1203", new byte[] { 0x12, 0x03 });
        }

        [TestMethod]
        public void Test_numeric_8()
        {
            TestNumeric("65536", new byte[] { 0x65, 0x53, 0x6f });
        }

        [TestMethod]
        public void Test_numeric_9()
        {
            TestNumeric("1234567", new byte[] {0x12,0x34,0x56,0x7f });
        }

        [TestMethod]
        public void Test_numeric_10()
        {
            TestNumeric("10000000000000000000", new byte[] {0x10 ,0,0,0,0,0,0,0,0,0});
        }

        [TestMethod]
        public void Test_numeric_11()
        {
            TestNumeric("100000000000000000001", new byte[] {0x10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x1f });
        }

        [TestMethod]
        public void Test_numeric_12()
        {
            TestNumeric("8999999999999999999999", new byte[] { 0x89, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99, 0x99 });
        }

        void TestNumeric(string text, byte[] correct)
        {
            byte[] result = Compact.NumericCompact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.NumericExtract(result));
        }


        #region 检查是否在范围

        // 不在范围的数字,符号
        [TestMethod]
        public void Check_Numeric_1()
        {
            CheckNumeric("0");
        }

        // 不在范围的数字,符号
        [TestMethod]
        public void Check_Numeric_2()
        {
            CheckNumeric("9");
        }

        // 不在范围的数字,符号
        [TestMethod]
        public void Check_Numeric_3()
        {
            CheckNumeric("-11");
        }

        // 不在范围的数字,符号
        [TestMethod]
        public void Check_Numeric_4()
        {
            CheckNumeric("A");
        }


        // 不在范围的数字,符号
        [TestMethod]
        public void Check_Numeric_5()
        {
            CheckNumeric("中国");
        }

        // 不在范围的数字,符号
        [TestMethod]
        public void Check_Numeric_6()
        {
            CheckNumeric("/");
        }

        void CheckNumeric(string text)
        {
            bool bRet = Compact.CheckNumeric(text, false);

            Assert.IsTrue(bRet == false);
        }


        #endregion



        #endregion


        #region bit5




        [TestMethod]
        public void Test_bit5_3()
        {
            /*
            AHQZ
            41-48-51-5a
            01000001-01001000-01010001-01011010
            00001-01000-10001-11010
            00001010-00100011-1010
            00001010-00100011-10100000
            0a-23-a0
            0x0a,0x23,0xa0
             */
            TestBit5("AHQZ", new byte[] { 0x0a, 0x23, 0xa0 });
        }

        [TestMethod]
        public void Test_bit5_4()
        {
            /*
            ZYXWV
            5a-59-58-57-56
            01011010-01011001-01011000-01010111-01010110
            11010-11001-11000-10111-10110
            11010110-01110001-01111011-0
            11010110-01110001-01111011-00000000
            d6-71-7b-00
            0xd6,0x71,0x7b,0x00
             */
            TestBit5("ZYXWV", new byte[] { 0xd6, 0x71, 0x7b, 0x00 });
        }

        [TestMethod]
        public void Test_bit5_5()
        {
            /*
            UTSRQPO
            55-54-53-52-51-50-4f
            01010101-01010100-01010011-01010010-01010001-01010000-01001111
            10101-10100-10011-10010-10001-10000-01111
            10101101-00100111-00101000-11000001-111
            10101101-00100111-00101000-11000001-11100000
            ad-27-28-c1-e0
            0xad,0x27,0x28,0xc1,0xe0
             */
            TestBit5("UTSRQPO", new byte[] { 0xad, 0x27, 0x28, 0xc1, 0xe0 });
        }

        [TestMethod]
        public void Test_bit5_6()
        {
            /*
            ABCDEFGHIGKLMNOPQRSTUVWXYZ
            41-42-43-44-45-46-47-48-49-47-4b-4c-4d-4e-4f-50-51-52-53-54-55-56-57-58-59-5a
            01000001-01000010-01000011-01000100-01000101-01000110-01000111-01001000-01001001-01000111-01001011-01001100-01001101-01001110-01001111-01010000-01010001-01010010-01010011-01010100-01010101-01010110-01010111-01011000-01011001-01011010
            00001-00010-00011-00100-00101-00110-00111-01000-01001-00111-01011-01100-01101-01110-01111-10000-10001-10010-10011-10100-10101-10110-10111-11000-11001-11010
            00001000-10000110-01000010-10011000-11101000-01001001-11010110-11000110-10111001-11110000-10001100-10100111-01001010-11011010-11111000-11001110-10
            00001000-10000110-01000010-10011000-11101000-01001001-11010110-11000110-10111001-11110000-10001100-10100111-01001010-11011010-11111000-11001110-10000000
            08-86-42-98-e8-49-d6-c6-b9-f0-8c-a7-4a-da-f8-ce-80
            0x08,0x86,0x42,0x98,0xe8,0x49,0xd6,0xc6,0xb9,0xf0,0x8c,0xa7,0x4a,0xda,0xf8,0xce,0x80
             */
            TestBit5("ABCDEFGHIGKLMNOPQRSTUVWXYZ", new byte[] { 0x08, 0x86, 0x42, 0x98, 0xe8, 0x49, 0xd6, 0xc6, 0xb9, 0xf0, 0x8c, 0xa7, 0x4a, 0xda, 0xf8, 0xce, 0x80 });
        }

        [TestMethod]
        public void Test_bit5_7()
        {
            /*
            [\]^_
            5b-5c-5d-5e-5f
            01011011-01011100-01011101-01011110-01011111
            11011-11100-11101-11110-11111
            11011111-00111011-11101111-1
            11011111-00111011-11101111-10000000
            df-3b-ef-80
            0xdf,0x3b,0xef,0x80
             */
            TestBit5(@"[\]^_", new byte[] { 0xdf, 0x3b, 0xef, 0x80 });
        }

        [TestMethod]
        public void Test_bit5_8()
        {
            /*
            ABCDEFGHIGKLMNOPQRSTUVWXYZ[\]^_
            41-42-43-44-45-46-47-48-49-47-4b-4c-4d-4e-4f-50-51-52-53-54-55-56-57-58-59-5a-5b-5c-5d-5e-5f
            01000001-01000010-01000011-01000100-01000101-01000110-01000111-01001000-01001001-01000111-01001011-01001100-01001101-01001110-01001111-01010000-01010001-01010010-01010011-01010100-01010101-01010110-01010111-01011000-01011001-01011010-01011011-01011100-01011101-01011110-01011111
            00001-00010-00011-00100-00101-00110-00111-01000-01001-00111-01011-01100-01101-01110-01111-10000-10001-10010-10011-10100-10101-10110-10111-11000-11001-11010-11011-11100-11101-11110-11111
            00001000-10000110-01000010-10011000-11101000-01001001-11010110-11000110-10111001-11110000-10001100-10100111-01001010-11011010-11111000-11001110-10110111-11001110-11111011-111
            00001000-10000110-01000010-10011000-11101000-01001001-11010110-11000110-10111001-11110000-10001100-10100111-01001010-11011010-11111000-11001110-10110111-11001110-11111011-11100000
            08-86-42-98-e8-49-d6-c6-b9-f0-8c-a7-4a-da-f8-ce-b7-ce-fb-e0
            0x08,0x86,0x42,0x98,0xe8,0x49,0xd6,0xc6,0xb9,0xf0,0x8c,0xa7,0x4a,0xda,0xf8,0xce,0xb7,0xce,0xfb,0xe0
             */
            TestBit5(@"ABCDEFGHIGKLMNOPQRSTUVWXYZ[\]^_", new byte[] { 0x08, 0x86, 0x42, 0x98, 0xe8, 0x49, 0xd6, 0xc6, 0xb9, 0xf0, 0x8c, 0xa7, 0x4a, 0xda, 0xf8, 0xce, 0xb7, 0xce, 0xfb, 0xe0 });
        }



        [TestMethod]
        public void Test_bit5_9()
        {
            /*
            C_D
            43-5f-44
            01000011-01011111-01000100
            00011-11111-00100
            00011111-1100100
            00011111-11001000
            1f-c8
            0x1f,0xc8
             */
            TestBit5("C_D", new byte[] { 0x1f, 0xc8 });
        }

        [TestMethod]
        public void Test_bit5_10()
        {
            /*
            E_F^
            45-5f-46-5e
            01000101-01011111-01000110-01011110
            00101-11111-00110-11110
            00101111-11001101-1110
            00101111-11001101-11100000
            2f-cd-e0
            0x2f,0xcd,0xe0
            */
            TestBit5("E_F^", new byte[] { 0x2f, 0xcd, 0xe0 });
        }


        [TestMethod]
        public void Test_bit5_11()
        {
            /*
           _G]H^
           5f-47-5d-48-5e
           01011111-01000111-01011101-01001000-01011110
           11111-00111-11101-01000-11110
           11111001-11111010-10001111-0
           11111001-11111010-10001111-00000000
           f9-fa-8f-00
           0xf9,0xfa,0x8f,0x00  
           */
            TestBit5("_G]H^", new byte[] { 0xf9, 0xfa, 0x8f, 0x00 });
        }



        void TestBit5(string text, byte[] correct)
        {
            byte[] result = Compact.Bit5Compact(text);

            Assert.IsTrue(result.SequenceEqual(correct));

            Assert.AreEqual(text, Compact.Bit5Extract(result));
        }


        #region 检查是否在范围



        [TestMethod]
        public void Check_Bit5_1()
        {
            //1位长度
            CheckBit5("A");
        }

        [TestMethod]
        public void Check_Bit5_2()
        {
            // 2位长度
            CheckBit5("^B");
        }

        [TestMethod]
        public void Check_Bit5_3()
        {
            // 小写
            CheckBit5("abc");
        }

        [TestMethod]
        public void Check_Bit5_4()
        {
            // 数字
            CheckBit5("123");
        }

        [TestMethod]
        public void Check_Bit5_5()
        {
            // 汉字
            CheckBit5("中国");
        }

        [TestMethod]
        public void Check_Bit5_6()
        {
            // 不在范围的字符
            CheckBit5("/?*");
        }

        [TestMethod]
        public void Check_Bit5_7()
        {
            // 不在范围
            CheckBit5("-AB");
        }


        void CheckBit5(string text)
        {
            bool bRet = Compact.CheckBit5(text, false);

            Assert.IsTrue(bRet == false);
        }

        #endregion


        #endregion


        #region bit6



        [TestMethod]
        public void Test_bit6_4()
        {
            /*
            ABC12345
            41-42-43-31-32-33-34-35
            01000001-01000010-01000011-00110001-00110010-00110011-00110100-00110101
            000001-000010-000011-110001-110010-110011-110100-110101
            00000100-00100000-11110001-11001011-00111101-00110101
            00000100-00100000-11110001-11001011-00111101-00110101
            04-20-f1-cb-3d-35
            0x04,0x20,0xf1,0xcb,0x3d,0x35
             */
            TestBit6("ABC12345", new byte[] {
                0x04,0x20,0xf1,0xcb,0x3d,0x35
            });
        }

        [TestMethod]
        public void Test_bit6_5()
        {
            /*
            K825.6=76/Z780
            4b-38-32-35-2e-36-3d-37-36-2f-5a-37-38-30
            01001011-00111000-00110010-00110101-00101110-00110110-00111101-00110111-00110110-00101111-01011010-00110111-00111000-00110000
            001011-111000-110010-110101-101110-110110-111101-110111-110110-101111-011010-110111-111000-110000
            00101111-10001100-10110101-10111011-01101111-01110111-11011010-11110110-10110111-11100011-0000
            00101111-10001100-10110101-10111011-01101111-01110111-11011010-11110110-10110111-11100011-00001000
            2f-8c-b5-bb-6f-77-da-f6-b7-e3-08
            0x2f,0x8c,0xb5,0xbb,0x6f,0x77,0xda,0xf6,0xb7,0xe3,0x08
             */
            TestBit6("K825.6=76/Z780", new byte[] {
                0x2f,0x8c,0xb5,0xbb,0x6f,0x77,0xda,0xf6,0xb7,0xe3,0x08
            });
        }

        [TestMethod]
        public void Test_bit6_6()
        {
            /*
            TP393/H637
            54-50-33-39-33-2f-48-36-33-37
            01010100-01010000-00110011-00111001-00110011-00101111-01001000-00110110-00110011-00110111
            010100-010000-110011-111001-110011-101111-001000-110110-110011-110111
            01010001-00001100-11111001-11001110-11110010-00110110-11001111-0111
            01010001-00001100-11111001-11001110-11110010-00110110-11001111-01111000
            51-0c-f9-ce-f2-36-cf-78
            0x51,0x0c,0xf9,0xce,0xf2,0x36,0xcf,0x78
             */
            TestBit6("TP393/H637", new byte[] {
                0x51,0x0c,0xf9,0xce,0xf2,0x36,0xcf,0x78
            });
        }

        [TestMethod]
        public void Test_bit6_7()
        {
            /*
            I563.85/H022
            49-35-36-33-2e-38-35-2f-48-30-32-32
            01001001-00110101-00110110-00110011-00101110-00111000-00110101-00101111-01001000-00110000-00110010-00110010
            001001-110101-110110-110011-101110-111000-110101-101111-001000-110000-110010-110010
            00100111-01011101-10110011-10111011-10001101-01101111-00100011-00001100-10110010
            00100111-01011101-10110011-10111011-10001101-01101111-00100011-00001100-10110010
            27-5d-b3-bb-8d-6f-23-0c-b2
            0x27,0x5d,0xb3,0xbb,0x8d,0x6f,0x23,0x0c,0xb2
             */
            TestBit6("I563.85/H022", new byte[] {
                0x27,0x5d,0xb3,0xbb,0x8d,0x6f,0x23,0x0c,0xb2
            });
        }

        [TestMethod]
        public void Test_bit6_8()
        {
            /*
            I17(198.4)/Y498
            49-31-37-28-31-39-38-2e-34-29-2f-59-34-39-38
            01001001-00110001-00110111-00101000-00110001-00111001-00111000-00101110-00110100-00101001-00101111-01011001-00110100-00111001-00111000
            001001-110001-110111-101000-110001-111001-111000-101110-110100-101001-101111-011001-110100-111001-111000
            00100111-00011101-11101000-11000111-10011110-00101110-11010010-10011011-11011001-11010011-10011110-00
            00100111-00011101-11101000-11000111-10011110-00101110-11010010-10011011-11011001-11010011-10011110-00100000
            27-1d-e8-c7-9e-2e-d2-9b-d9-d3-9e-20
            0x27,0x1d,0xe8,0xc7,0x9e,0x2e,0xd2,0x9b,0xd9,0xd3,0x9e,0x20
             */
            TestBit6("I17(198.4)/Y498", new byte[] {
                0x27,0x1d,0xe8,0xc7,0x9e,0x2e,0xd2,0x9b,0xd9,0xd3,0x9e,0x20
            });
        }

        [TestMethod]
        public void Test_bit6_9()
        {
            /*
            I712.88/B303
            49-37-31-32-2e-38-38-2f-42-33-30-33
            01001001-00110111-00110001-00110010-00101110-00111000-00111000-00101111-01000010-00110011-00110000-00110011
            001001-110111-110001-110010-101110-111000-111000-101111-000010-110011-110000-110011
            00100111-01111100-01110010-10111011-10001110-00101111-00001011-00111100-00110011
            00100111-01111100-01110010-10111011-10001110-00101111-00001011-00111100-00110011
            27-7c-72-bb-8e-2f-0b-3c-33
            0x27,0x7c,0x72,0xbb,0x8e,0x2f,0x0b,0x3c,0x33
             */
            TestBit6("I712.88/B303", new byte[] {
               0x27,0x7c,0x72,0xbb,0x8e,0x2f,0x0b,0x3c,0x33
            });
        }

        [TestMethod]
        public void Test_bit6_10()
        {
            /*
            J205.1/F933
            4a-32-30-35-2e-31-2f-46-39-33-33
            01001010-00110010-00110000-00110101-00101110-00110001-00101111-01000110-00111001-00110011-00110011
            001010-110010-110000-110101-101110-110001-101111-000110-111001-110011-110011
            00101011-00101100-00110101-10111011-00011011-11000110-11100111-00111100-11
            00101011-00101100-00110101-10111011-00011011-11000110-11100111-00111100-11100000
            2b-2c-35-bb-1b-c6-e7-3c-e0
            0x2b,0x2c,0x35,0xbb,0x1b,0xc6,0xe7,0x3c,0xe0
             */
            TestBit6("J205.1/F933", new byte[] {
                0x2b,0x2c,0x35,0xbb,0x1b,0xc6,0xe7,0x3c,0xe0
            });
        }

        [TestMethod]
        public void Test_bit6_11()
        {
            /*
            I222.742/Z134
            49-32-32-32-2e-37-34-32-2f-5a-31-33-34
            01001001-00110010-00110010-00110010-00101110-00110111-00110100-00110010-00101111-01011010-00110001-00110011-00110100
            001001-110010-110010-110010-101110-110111-110100-110010-101111-011010-110001-110011-110100
            00100111-00101100-10110010-10111011-01111101-00110010-10111101-10101100-01110011-110100
            00100111-00101100-10110010-10111011-01111101-00110010-10111101-10101100-01110011-11010010
            27-2c-b2-bb-7d-32-bd-ac-73-d2
            0x27,0x2c,0xb2,0xbb,0x7d,0x32,0xbd,0xac,0x73,0xd2
             */
            TestBit6("I222.742/Z134", new byte[] {
                0x27,0x2c,0xb2,0xbb,0x7d,0x32,0xbd,0xac,0x73,0xd2
            });
        }

        [TestMethod]
        public void Test_bit6_12()
        {
            /*
            B822.9-49/K170
            42-38-32-32-2e-39-2d-34-39-2f-4b-31-37-30
            01000010-00111000-00110010-00110010-00101110-00111001-00101101-00110100-00111001-00101111-01001011-00110001-00110111-00110000
            000010-111000-110010-110010-101110-111001-101101-110100-111001-101111-001011-110001-110111-110000
            00001011-10001100-10110010-10111011-10011011-01110100-11100110-11110010-11110001-11011111-0000
            00001011-10001100-10110010-10111011-10011011-01110100-11100110-11110010-11110001-11011111-00001000
            0b-8c-b2-bb-9b-74-e6-f2-f1-df-08
            0x0b,0x8c,0xb2,0xbb,0x9b,0x74,0xe6,0xf2,0xf1,0xdf,0x08
             */
            TestBit6("B822.9-49/K170", new byte[] {
                0x0b,0x8c,0xb2,0xbb,0x9b,0x74,0xe6,0xf2,0xf1,0xdf,0x08
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
        public void Test_bit7_3()
        {
            /*
            abcdefgh
            61-62-63-64-65-66-67-68
            01100001-01100010-01100011-01100100-01100101-01100110-01100111-01101000
            1100001-1100010-1100011-1100100-1100101-1100110-1100111-1101000
            11000011-10001011-00011110-01001100-10111001-10110011-11101000
            11000011-10001011-00011110-01001100-10111001-10110011-11101000
            c3-8b-1e-4c-b9-b3-e8
            0xc3,0x8b,0x1e,0x4c,0xb9,0xb3,0xe8
             */
            TestBit7("abcdefgh", new byte[] {
                0xc3,0x8b,0x1e,0x4c,0xb9,0xb3,0xe8
            });
        }

        [TestMethod]
        public void Test_bit7_4()
        {
            /*
            abcdefgh123
            61-62-63-64-65-66-67-68-31-32-33
            01100001-01100010-01100011-01100100-01100101-01100110-01100111-01101000-00110001-00110010-00110011
            1100001-1100010-1100011-1100100-1100101-1100110-1100111-1101000-0110001-0110010-0110011
            11000011-10001011-00011110-01001100-10111001-10110011-11101000-01100010-11001001-10011
            11000011-10001011-00011110-01001100-10111001-10110011-11101000-01100010-11001001-10011111
            c3-8b-1e-4c-b9-b3-e8-62-c9-9f
            0xc3,0x8b,0x1e,0x4c,0xb9,0xb3,0xe8,0x62,0xc9,0x9f
             */
            TestBit7("abcdefgh123", new byte[] {
                0xc3,0x8b,0x1e,0x4c,0xb9,0xb3,0xe8,0x62,0xc9,0x9f
            });
        }

        [TestMethod]
        public void Test_bit7_5()
        {
            /*
            hijk9876{/?}~
            68-69-6a-6b-39-38-37-36-7b-2f-3f-7d-7e
            01101000-01101001-01101010-01101011-00111001-00111000-00110111-00110110-01111011-00101111-00111111-01111101-01111110
            1101000-1101001-1101010-1101011-0111001-0111000-0110111-0110110-1111011-0101111-0111111-1111101-1111110
            11010001-10100111-01010110-10110111-00101110-00011011-10110110-11110110-10111101-11111111-11011111-110
            11010001-10100111-01010110-10110111-00101110-00011011-10110110-11110110-10111101-11111111-11011111-11011111
            d1-a7-56-b7-2e-1b-b6-f6-bd-ff-df-df
            0xd1,0xa7,0x56,0xb7,0x2e,0x1b,0xb6,0xf6,0xbd,0xff,0xdf,0xdf
             */
            TestBit7("hijk9876{/?}~", new byte[] {
                0xd1,0xa7,0x56,0xb7,0x2e,0x1b,0xb6,0xf6,0xbd,0xff,0xdf,0xdf
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
        public void Test_isil_process_4()
        {
            TestIsilProcess("-", @"- output:00000");
            TestIsilProcess("1", @"1 shift:u-d output:11111,0001");
            TestIsilProcess("2", @"2 shift:u-d output:11111,0010");
            TestIsilProcess("9", @"9 shift:u-d output:11111,1001");
            TestIsilProcess(":", @": output:11011");
        }


        [TestMethod]
        public void Test_isil_process_5()
        {
            TestIsilProcess("CN-110108-1-NLC",
                @"C output:00011
N output:01110
- output:00000
1 switch:u-d output:11110,0001
1 output:0001
0 output:0000
1 output:0001
0 output:0000
8 output:1000
- output:1010
1 output:0001
- output:1010
N switch:d-u output:1100,01110
L output:01100
C output:00011");
        }

        [TestMethod]
        public void Test_isil_process_6()
        {
            TestIsilProcess("C-YBL",
                @"C output:00011
- output:00000
Y output:11001
B output:00010
L output:01100");
        }


        [TestMethod]
        public void Test_isil_process_7()
        {
            TestIsilProcess("CN-120104-C-YBL", 
                @"C output:00011
N output:01110
- output:00000
1 switch:u-d output:11110,0001
2 output:0010
0 output:0000
1 output:0001
0 output:0000
4 output:0100
- output:1010
C shift:d-u output:1101,00011
- output:1010
Y switch:d-u output:1100,11001
B output:00010
L output:01100");
        }

        [TestMethod]
        public void Test_isil_process_list_1()
        {
            List<string> strList = new List<string>();
            strList.Add(@"C output:00011
N output:01110
- output:00000
1 switch:u-d output:11110,0001
2 output:0010
0 output:0000
1 output:0001
0 output:0000
4 output:0100
- output:1010
C switch:d-u output:1100,00011
- output:00000
Y output:11001
B output:00010
L output:01100");

            strList.Add(@"C output:00011
N output:01110
- output:00000
1 switch:u-d output:11110,0001
2 output:0010
0 output:0000
1 output:0001
0 output:0000
4 output:0100
- output:1010
C shift:d-u output:1101,00011
- output:1010
Y switch:d-u output:1100,11001
B output:00010
L output:01100");

            TestIsilProcessList("CN-120104-C-YBL",strList);
        }




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

        // 基于TestIsilProcess，只要list中的其中一项匹配，则通过
        void TestIsilProcessList(string text, List<string> processInfoList)
        {
            StringBuilder debugInfo = new StringBuilder();
            Compact.IsilCompact(text, debugInfo);

            // 去掉末尾的回行
            bool bRet = false;
            for (int i = 0; i < processInfoList.Count; i++)
            {
                string process_info = processInfoList[i];
                process_info = process_info.TrimEnd(new char[] { '\r', '\n' });
                string result = debugInfo.ToString().TrimEnd(new char[] { '\r', '\n' });

                 bRet= String.Equals(process_info, result);
                if (bRet==true)
                {
                    break;
                }
            }

            Assert.AreEqual(bRet,true);
        }



        
        [TestMethod]
        public void Test_isil_3()
        {
            TestIsil("CN-110108-1-NLC", new byte[] {
                0x1b,0x81,0xe1,0x10,0x10,0x8a,0x1a,0xc7,0x30,0x7f
            });
        }


        
        [TestMethod]
        public void Test_isil_4()
        {
            TestIsil("CN-120104-C-YBL", new byte[] {
                0x1b,0x81,0xe1,0x20,0x10,0x4a,0xd1,0xd6,0x64,0x4c    // -C-YBL 按shift
                //0x1b,0x81,0xe1,0x20,0x10,0x4a,0xc1,0x83,0x22,0x67   // // -C-YBL 按switch
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
        public void Test_autoSelect_3()
        {
            Assert.AreEqual(
                CompactionScheme.SixBitCode,
                Compact.AutoSelectCompactMethod("a bcde"));
        }

        [TestMethod]
        public void Test_autoSelect_4()
        {
            Assert.AreEqual(
                CompactionScheme.OctectString,
                Compact.AutoSelectCompactMethod("BA"));  // 拓抽标签中写的OMF元素值为BA
        }

        #endregion


    }
}
