using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// 国内高校联盟标签实用函数
    /// 参考: http://www.cityu.edu.hk/lib/rfid_consortium/document.html
    /// </summary>
    public static class GaoxiaoUtility
    {
        // 判断标签内容是否采用了 高校联盟 编码格式
        // 疑问：可否认为凡是不符合国标格式的就是高校联盟格式？
        public static bool IsISO285604Format(byte[] epc_bank,
            byte[] user_bank)
        {
            if (user_bank != null && user_bank.Length >= 1)
            {
                // 检查 User Bank 特征？

            }

            /*
            {
                var pc = UhfUtility.ParsePC(epc_bank, 2);
                if (pc.AFI != 0xc2)
                    return false;
                if (user_bank != null && pc.UMI == false)
                    return false;
                if (pc.ISO == false)
                    return false;
            }
            */

            return true;
        }

        // TODO: Encode 和 Decode 往返 EPC Payload 单元测试

        // 编码高校联盟 EPC bank 的载荷部分。
        // return:
        //      返回 EPC 载荷字节数组。字节数会自动确保为偶数。注意返回的内容没有包含前 4 bytes(校验码和 PC)
        public static byte[] EncodeGaoxiaoEpcPayload(GaoxiaoEpcInfo info)
        {
            List<byte> result = new List<byte>();

            // *** 第一字节
            byte first = 0;
            // 安全位
            if (info.Lending)
                first = 0x80;
            // 预留位
            first |= (byte)((info.Reserve & 0x03) << 4);

            // 分拣信息
            first |= (byte)(info.Picking & 0xf);

            result.Add(first);

            // *** 第二字节
            byte second = 0;
            // 探测 PII 可用的编码方式
            second = (byte)((DetectEncodingType(info.PII) << 6) & 0xc0);

            // 数据模型版本
            // 值 0 代表第一版
            if (info.Version == 0)
                throw new Exception($"版本号应该从 1 开始");

            second |= (byte)((info.Version - 1) & 0x3f);
            result.Add(second);

            // *** EPC 的第 3-4 字节
            var two_bytes = EncodeContentParameter(info.ContentParameters);
            if (two_bytes.Length != 2)
                throw new Exception($"EncodeContentParameter() 编码的结果应该是 2 字节(但现在是 {two_bytes.Length} 字节)");
            result.AddRange(two_bytes);

            // *** EPC 的第 5-12/16/18 字节

            byte[] pii_bytes = null;
            if (info.EncodingType == 0)
                pii_bytes = EncodeIpc96bit(info.PII);
            else if (info.EncodingType == 1)
                pii_bytes = EncodeIpc128bit(info.PII);
            else if (info.EncodingType == 2)
                pii_bytes = Encoding.UTF8.GetBytes(info.PII);

            result.AddRange(pii_bytes);

            // 检查 result 的字节数是否为偶数，如果必要补齐偶数
            if ((result.Count % 2) != 0)
                result.Add(0);

            return result.ToArray();
        }

        // TODO: 注意添加单元测试
        // 检查 PII 文本适合用那种方式编码？
        static int DetectEncodingType(string text)
        {
            string error = VerifyIpc96bitString(text);
            if (error == null)
                return 0;
            error = VerifyIpc128bitString(text);
            if (error == null)
                return 1;
            return 3;   // TODO：注意检查 text 字符数是否超过最大限制
        }

        // 判断高校联盟格式的 EPC 载荷里面，图书是否处于外借状态
        // parameters:
        //      first_byte_of_epc_payload EPC 载荷的第一 byte
        public static bool IsLending(byte first_byte_of_epc_payload)
        {
            return (first_byte_of_epc_payload & 0x80) != 0;
        }

        // 解码 高校联盟 EPC 载荷部分。注意不包含 校验码 word 和 PC word
        public static GaoxiaoEpcInfo DecodeGaoxiaoEpcPayload(byte[] data,
            int length)
        {
            if (data.Length < length)
                throw new ArgumentException($"data 的字节数 ({data.Length}) 小于 length 参数值 {length}");

            GaoxiaoEpcInfo result = new GaoxiaoEpcInfo();

            if (length < 5)
                throw new ArgumentException($"data 的字节数 ({length}) 不应小于 5");
            /*
            if (data.Length < 1)
                throw new ArgumentException("data 的字节数不应小于 1");
            */

            byte first = data[0];
            // 安全位
            result.Lending = (first & 0x80) != 0;

            // 预留位
            result.Reserve = (first >> 4) & 0x03;

            // 分拣信息
            result.Picking = first & 0x0f;

            if (data.Length < 2)
                throw new ArgumentException("data 的字节数不应小于 2");
            byte second = data[1];

            // 编码方式
            result.EncodingType = (second >> 6) & 0x03;

            // 数据模型版本
            // 值 0 代表第一版
            result.Version = (second & 0x3f) + 1;

            byte[] content_parameter = new byte[2];
            content_parameter[0] = data[2];
            content_parameter[1] = data[3];

            // bit 高 --> bit 低
            // 31 30 29 28 27 26 24 16 , 15 14 12 11 6 5 4 3
            result.ContentParameters = DecodeContentParameter(content_parameter);

            // *** EPC 的第 5-12/16/18 字节
            byte[] rest = new byte[length - 4];
            Array.Copy(data, 4, rest, 0, length - 4);
            if (result.EncodingType == 0)
                result.PII = DecodeIpc96bit(rest);
            else if (result.EncodingType == 1)
                result.PII = DecodeIpc128bit(rest);
            else if (result.EncodingType == 2)
                result.PII = Encoding.UTF8.GetString(rest);
            else
                throw new Exception("目前暂不支持第四种 PII 编码方式");

            return result;
        }

        #region EPC 内 PII 编码解码

        // 解码 128 bit
        public static string DecodeIpc128bit(byte[] data)
        {
            int char_count = data[0] & 0x0f;
            // 获取压缩前文本字符数
            if (char_count < 1 || char_count > 14)
                throw new Exception($"从第一 byte 析出的原始字符数 {char_count} 超过合法范围 1-14");

            if (char_count < 12)
            {
                if (data.Length < char_count + 1)
                    throw new ArgumentException($"data 应为 {char_count + 1} 字节以上(但现在是 {data.Length} 字节) (1)");

                StringBuilder result = new StringBuilder();
                for (int i = 0; i < char_count; i++)
                {
                    result.Append((char)data[i + 1]);
                }

                return result.ToString();
            }

            if (data.Length < 12)
                throw new ArgumentException($"data 应为 12 字节以上(但现在是 {data.Length} 字节) (2)");

            char[] work_string = new char[15];
            for (int i = 0; i < work_string.Length; i++)
            {
                work_string[i] = '0';
            }

            // 将压缩码的低4字节取出
            UInt32 v1 = BitConverter.ToUInt32(data, 0);
            v1 >>= 4;
            // 还原最后1位字符，即校验位
            work_string[char_count - 1] = (char)(v1 & 0xFF);

            v1 >>= 8;
            // 解码第11至倒数第2个字符
            for (int i = char_count - 2; i >= 10; --i)
            {
                work_string[i] = (char)((v1 & 0x3F) + 0x30);
                v1 >>= 6;
            }

            // 将压缩码的中间4字节取出
            UInt32 v2 = BitConverter.ToUInt32(data, 4);
            // 解码第6-10个字符
            for (int i = 9; i >= 5; --i)
            {
                work_string[i] = (char)((v2 & 0x3F) + 0x30);
                v2 >>= 6;
            }

            // 将压缩码的高4字节取出
            UInt32 v3 = BitConverter.ToUInt32(data, 8);
            // 解码第1-5个字符
            for (int i = 4; i >= 0; --i)
            {
                work_string[i] = (char)((v3 & 0x3F) + 0x30);
                v3 >>= 6;
            }

            return new string(work_string.ToArray()).Substring(0, char_count);
        }

        // 验证 128 bit 编码的输入字符串的合法性
        public static string VerifyIpc128bitString(string text)
        {
            // 2023/11/6
            if (text == null)
                return $"字符串 '{text}' 长度 0 不符合 128-bit 编码的要求(应为 1-14 字符)";

            if (text.Length < 1 || text.Length > 14)
                return $"字符串 '{text}' 长度 {text.Length} 不符合 128-bit 编码的要求(应为 1-14 字符)";

            if (text.Length < 12) // 处理1-11字符长度的 text
            {
                return null;
            }

            int i = 0;
            foreach (char ch in text)
            {
                if (i == text.Length - 1)
                {
                    // 最后一个字符可以是任意 ASCII 字符
                }
                else
                {
                    if (char.IsDigit(ch) == false && char.IsUpper(ch) == false)
                        return $"128 bit 编码方式的字符串内字符 '{ch}' 不合法，应为数字或大写字母";
                }
                i++;
            }
            return null;
        }

        // 128 bit 编码
        // parameters:
        //      text    当字符数为 1-11：可选字符集为ISO/IEC646 IRV中的任意字符
        //              当字符数为 12-14： 第1至倒数第2位可选字符集为A-Z字母或0-9数字(小写字母需转换为大写再传入)
        //                  最后1位的可选字符集为ISO/IEC646 IRV中的任意字符
        // 返回的 bytes 长度：1) 当 text 小于 12 字符时，返回 bytes 长度为 text 字符数加一;
        // 2) 当 text 大于等于 12 字符(小于等于 14)时，返回 bytes 长度恒定为 12 
        public static byte[] EncodeIpc128bit(string text)
        {
            string error = VerifyIpc128bitString(text);
            if (error != null)
                throw new Exception(error);

            List<byte> result = new List<byte>();

            if (text.Length < 1 || text.Length > 14)
                throw new ArgumentException($"字符串 '{text}' 长度 {text.Length} 不符合 128-bit 编码的要求(应为 1-14 字符)");

            if (text.Length < 12) // 处理1-11字符长度的 text
            {
                result.Add((byte)text.Length);  // 存放原条码长度到低4bit
                foreach (var ch in text)
                {
                    result.Add((byte)ch);
                }
                return result.ToArray();
            }


            // 处理BarCode的第1-5个字符
            UInt32 v = 0;
            for (int i = 0; i < 5; ++i)
            {
                v = (v << 6) | ((UInt32)text[i] - 0x30);
            }
            byte[] compressed_1 = BitConverter.GetBytes(v); // offset 8

            // 处理BarCode的第6-10个字符
            UInt32 v2 = 0;
            for (int i = 0; i < 5; ++i)
            {
                v2 = (v2 << 6) | ((UInt32)text[i + 5] - 0x30);
            }
            byte[] compressed_2 = BitConverter.GetBytes(v2);    // offset 4


            // 处理BarCode的第11-倒数第二位字符
            UInt32 v3 = 0;
            for (int i = 0; i < text.Length - 11; ++i)
            {
                v3 = (v3 << 6) | ((UInt32)text[i + 10] - 0x30);
            }
            v3 = (v3 << 8) | ((UInt32)(text[text.Length - 1]));
            // 校验位
            v3 = (v3 << 4) | (UInt32)text.Length;
            byte[] compressed_3 = BitConverter.GetBytes(v3);    // offset 0

            result.AddRange(compressed_3);
            result.AddRange(compressed_2);
            result.AddRange(compressed_1);

            return result.ToArray();
        }

        // 解码 96 bit
        public static string DecodeIpc96bit(byte[] data)
        {
            int char_count = data[0] & 0x0f;
            // 获取压缩前文本字符数
            if (char_count < 1 || char_count > 14)
                throw new Exception($"从第一 byte 析出的原始字符数 {char_count} 超过合法范围 1-14");

            if (char_count < 8)
            {
                if (data.Length < char_count + 1)
                    throw new ArgumentException($"data 应为 {char_count + 1} 字节以上(但现在是 {data.Length} 字节) (1)");

                StringBuilder result = new StringBuilder();
                for (int i = 0; i < char_count; i++)
                {
                    result.Append((char)data[i + 1]);
                }

                return result.ToString();
            }

            if (data.Length < 8)
                throw new ArgumentException($"data 应为 8 字节以上(但现在是 {data.Length} 字节) (2)");

            char[] work_string = new char[15];
            for (int i = 0; i < work_string.Length; i++)
            {
                work_string[i] = '0';
            }

            // 前 4 bytes 变为整数值
            {
                UInt32 v = BitConverter.ToUInt32(data, 0);

                v >>= 4;

                // 还原最后1位字符，即校验位
                work_string[char_count - 1] = (char)(v & 0xFF);

                v >>= 8;

                // 解码最前面3位字母或数字
                for (int i = 2; i >= 0; --i)
                {
                    work_string[i] = (char)((v & 0x3F) + 0x30);
                    v >>= 6;
                }
                // 还原第四位数字的低2bit
                work_string[3] = (char)(v & 0x03);
            }

            // 将压缩码的前面(高)4字节取出
            {
                UInt32 v1 = BitConverter.ToUInt32(data, 4);

                // 取出第4位数字的低2bit，还原第4位数字
                work_string[3] = (char)
                    ((work_string[3] | ((char)(v1 & 0x03) << 2)) + 0x30);

                v1 >>= 2;
                // 还原BarCode从第5位起除最后1位的2-9位数字
                string number = v1.ToString();
                // _itoa(unWorkDat.uiDat, (char*)ucTmpStr, 10);
                // iDatLen = strlen((char*)ucTmpStr);
                for (int i = 0; i < number.Length; ++i)
                {
                    work_string[i + char_count - number.Length - 1] = number[i];
                }
            }

            return new string(work_string.ToArray()).Substring(0, char_count);
        }

        // 验证 96 bit 编码的输入字符串的合法性
        public static string VerifyIpc96bitString(string text)
        {
            // 2023/11/6
            if (text == null)
                return $"字符串 '{text}' 长度 0 不符合 96-bit 编码的要求(应为 1-14 字符)";

            if (text.Length < 1 || text.Length > 14)
                return $"字符串 '{text}' 长度 {text.Length} 不符合 96-bit 编码的要求(应为 1-14 字符)";

            if (text.Length < 8)
            {
                return null;
            }

            int i = 0;
            foreach (char ch in text)
            {
                if (i == text.Length - 1)
                {
                    // 最后一个字符可以是任意 ASCII 字符
                }
                else if (i >= 0 && i < 3)
                {
                    if (char.IsDigit(ch) == false && char.IsUpper(ch) == false)
                        return $"96 bit 编码方式的源字符串内字符(前三位内) '{ch}' 不合法，应为数字或大写字母";
                }
                else
                {
                    if (char.IsDigit(ch) == false)
                        return $"96 bit 编码方式的源字符串内字符 '{ch}' 不合法，应为数字";
                }
                i++;
            }
            return null;
        }


        // 编码 96 bit
        // parameters:
        //      text    如果为小于 8 字符的字符串，字符可以为任意 ASCII 字符
        //              如果为大于等于 8 字符(小于等于 14)的字符串，则前三位可以为大写字母和数字，最后一位可以为任意 ASCII 字符；其余字符必须为数字
        // return:
        // 返回的 bytes 长度：1) 当 text 小于 8 字符时，返回 bytes 长度为 text 字符数加一;
        // 2) 当 text 大于等于 8 字符(小于等于 14)时，返回 bytes 长度恒定为 8 
        public static byte[] EncodeIpc96bit(string text)
        {
            string error = VerifyIpc96bitString(text);
            if (error != null)
                throw new Exception(error);

            List<byte> result = new List<byte>();

            if (text.Length < 1 || text.Length > 14)
                throw new ArgumentException($"字符串 '{text}' 长度 {text.Length} 不符合 96-bit 编码的要求(应为 1-14 字符)");

            if (text.Length < 8)
            {
                result.Add((byte)text.Length);
                foreach (var ch in text)
                {
                    result.Add((byte)ch);
                }
                return result.ToArray();
            }

            // 压缩从第5位起除最后1位的2-9位数字
            string number = text.Substring(4, text.Length - 5);
            UInt32 v = Convert.ToUInt32(number);

            v <<= 2;
            // 将Barcode的第4位数字压缩后的高2bit存入
            v |= (((UInt32)text[3] - 0x30) & 0x0C) >> 2;

            byte[] compressed_1 = BitConverter.GetBytes(v);

            // 处理压缩后的低4字节
            {
                UInt32 v1 = 0;
                v1 = ((UInt32)text[3] - 0x30) & 0x03; // 压缩第4位数字压缩后的低2bit存入
                v1 = (v1 << 6)
                    | ((UInt32)text[0] - 0x30); // 压缩最前面3位字母或数字
                v1 = (v1 << 6) | ((UInt32)text[1] - 0x30);
                v1 = (v1 << 6) | ((UInt32)text[2] - 0x30);
                v1 = (v1 << 8) | ((UInt32)text[text.Length - 1]);   // 压缩BarCode最后1位字符，通常为校验码

                v1 = (v1 << 4) | (UInt32)text.Length;   // 记录原BarCode编码长度

                byte[] compressed_2 = BitConverter.GetBytes(v1);
                result.AddRange(compressed_2);
            }
            result.AddRange(compressed_1);
            return result.ToArray();
        }

#if NO
                        /*
                编码方式 1：针对 EPC 容量为 96bit
                适用情况：馆藏标识符总长度为 1-14 位的由字母和数字组成的字符串。当：
                * 馆藏标识符长度为 1-7：可选字符集为 ISO/IEC646 IRV 中的任意字符。
                * 馆藏标识符长度为 8-14：第 1 至 3 位字符的可选字符集为 A-Z 字母或 0-9 数字(如有小写
                字母需转换为大写字母再传入)，第 4 至倒数第 2 位的可选字符集为 0-9 数字，最后 1 位
                的可选字符集为 ISO/IEC646 IRV 中的任意字符。
                处理方式：参照附录 A 所给定的 96bit EPC 编码与解码算法进行处理。
                        * */

        /*+--------------------------------------------------------------------------------------+*/
        /*| iRFID_Encode96bit( ) |*/
        /*| |*/
        /*| Input: ucBarCode，输入时，是一个1-14Byte的无符号字符串 |*/
        /*| 由于结束符'\0'占用一个空间，最大需要开辟15Byte空间 |*/
        /*| ucBarCode字符数为1-7： |*/
        /*| 可选字符集为ISO/IEC646 IRV中的任意字符 |*/
        /*| ucBarCode字符数为8-14： |*/
        /*| 第1至3位字符的可选字符集为A-Z字母或0-9数字(小写字母需转换为大写再传入)|*/
        /*| 第4至倒数第2位的可选字符集为0-9数字 |*/
        /*| 最后1位的可选字符集为ISO/IEC646 IRV中的任意字符 |*/
        /*| Output: 输出时，ucBarCode是一个2-8Byte的无符号字符串 |*/
        /*| 返回值等于BarCode长度， -1代表输入条码长度不合法(输入条码长度介于1-14之间) |*/
        /*| |*/
        /*| 说明：用于压缩1-14位条码至2-8Byte |*/
        /*| ucBarCode字符数为1-7： |*/
        /*| 可选字符集为ISO/IEC646 IRV中的任意字符 |*/
        /*| ucBarCode字符数为8-14： |*/
        /*| 第1至3位字符的可选字符集为A-Z字母或0-9数字(小写字母需转换为大写再传入)|*/
        /*| 第4至倒数第2位的可选字符集为0-9数字 |*/
        /*| 最后1位的可选字符集为ISO/IEC646 IRV中的任意字符 |*/
        /*| |*/
        /*| Version 2.0 |*/
        /*| 完成日期：2012-11-16 |*/
        /*+--------------------------------------------------------------------------------------+*/
        int iRFID_Encode96bit(unsigned char* ucBarCode)
        {
            int i, iBarCodeLen;
            unsigned char ucCompressedBarCode[4], ucTmpChar;
            unsigned char* pucTmpDat;
            union
        {
                unsigned char ucStr[4];
                unsigned int uiDat;
            }
            unWorkDat;
            iBarCodeLen = strlen((char*)ucBarCode);
            if (iBarCodeLen < 1 || iBarCodeLen > 14) return (-1);
        if (iBarCodeLen < 8) // 处理1-7字符长度的BarCode
            {
                for (i = iBarCodeLen; i > 0; --i)
                {
                    ucBarCode[i] = ucBarCode[i - 1];
                }
                ucBarCode[0] = (unsigned char)iBarCodeLen;
                // 存放原条码长度到低4bit
                return (iBarCodeLen);
            }
            ucTmpChar = ucBarCode[iBarCodeLen - 1]; // 临时存放BarCode校验位
            ucBarCode[iBarCodeLen - 1] = 0; // 最后一位(即校验位)清零
            pucTmpDat = &ucBarCode[4]; // 处理压缩后的高4字节
            unWorkDat.uiDat = atoi((char*)pucTmpDat);// 压缩BarCode从第5位起除最后1位的2-9位数字
            unWorkDat.uiDat <<= 2;
            unWorkDat.uiDat |= (((unsigned int)ucBarCode[3] - 0x30 ) &0x0C )
>> 2; // 将Barcode的第4位数字压缩后的高2bit存入
            for (i = 0; i < 4; ++i) // 临时保存压缩后的高4字节
            {
                ucCompressedBarCode[i] = unWorkDat.ucStr[i];
            }
            // 处理压缩后的低4字节
            unWorkDat.uiDat = ((unsigned int)ucBarCode[3] - 0x30 )
&0x03; // 压缩第4位数字压缩后的低2bit存入
            unWorkDat.uiDat = (unWorkDat.uiDat << 6)
            | ((unsigned int)ucBarCode[0] - 0x30); // 压缩最前面3位字母或数字
            unWorkDat.uiDat = (unWorkDat.uiDat << 6) | ((unsigned int)ucBarCode[1] - 0x30);
            unWorkDat.uiDat = (unWorkDat.uiDat << 6) | ((unsigned int)ucBarCode[2] - 0x30);
            unWorkDat.uiDat = (unWorkDat.uiDat << 8) | ((unsigned int)ucTmpChar );
            // 压缩BarCode最后1位字符，通常为校验码
            unWorkDat.uiDat = (unWorkDat.uiDat << 4) | (unsigned int)iBarCodeLen;
            // 记录原BarCode编码长度
            for (i = 0; i < 4; ++i) // 构成输出压缩后的BarCode
            {
                ucBarCode[i] = unWorkDat.ucStr[i];
                ucBarCode[i + 4] = ucCompressedBarCode[i];
            }
            return (iBarCodeLen);
        }

        /*
编码方式 2：针对 EPC 容量为 128bit
适用情况：馆藏标识符总长度为 1-14 位字符。当：
* 馆藏标识符长度为 1-11：可选字符集为 ISO/IEC646 IRV 中的任意字符。
* 馆藏标识符长度为 12-14：第 1 至倒数第 2 位的可选字符集为 A-Z 字母或 0-9 数字(如有
小写字母需转换为大写字母再传入)，最后 1 位的可选字符集为 ISO/IEC646 IRV 中的任
意字符。
处理方式：参照附录 A 所给定的 128bit EPC 编码与解码算法进行处理。
        * */

        /*
编码方式 3：针对 EPC 容量为 144bit 或以上
适用情况：馆藏标识符总长度不大于 14 个字符，可选字符集为 ISO/IEC646 IRV 中的任意字
符。
处理方式：直接存取（不做任何编码、解码处理）。
        * */
#endif

        #endregion

        #region Content Parameters 演算

        static int[] offset_table = new int[] {
            3,4,5,6,11,12,14,15,16,24,26,27,28,29,30,31,
        };

        // 根据 offset 得到 OID
        static int GetOID(int offset)
        {
            if (offset < 0 || offset >= offset_table.Length)
                throw new ArgumentException($"offset 值 {offset} 超越合法范围(0-{offset_table.Length - 1})");
            return offset_table[offset];
        }

        // 解码两 byte 的 Content Parameter，返回 OID 列表
        public static int[] DecodeContentParameter(byte[] two_bytes)
        {
            List<int> results = new List<int>();
            // 低 byte (字节流中顺序靠后的才是低 byte)
            for (int i = 0; i < 8; i++)
            {
                bool on = ((two_bytes[1] >> i) & 0x01) != 0;
                if (on)
                    results.Add((byte)GetOID(i));
            }

            // 高 byte
            for (int i = 0; i < 8; i++)
            {
                bool on = ((two_bytes[0] >> i) & 0x01) != 0;
                if (on)
                    results.Add((byte)GetOID(i + 8));
            }
            return results.ToArray();
        }

        // 根据 OID 获得 bit 偏移量
        static int GetOffset(int oid)
        {
            for (int i = 0; i < offset_table.Length; i++)
            {
                if (oid == offset_table[i])
                    return i;
            }

            return -1;
        }

        // 把 OID 列表编码为 Content Parameter 的两 byte
        public static byte[] EncodeContentParameter(int[] oid_list)
        {
            UInt16 value = 0;
            foreach (var oid in oid_list)
            {
                var offset = GetOffset(oid);
                if (offset == -1)
                    throw new Exception($"OID {oid} 不允许出现在 Content Parameters 中");

                value |= (UInt16)(0x00000001 << offset);
            }

            return Compact.ReverseBytes(BitConverter.GetBytes(value));
        }

        #endregion

        #region USER 区的编码解码

        // 编码 User Bank
        public static byte[] EncodeUserBank(List<GaoxiaoUserElement> elements,
            bool tail_zero)
        {
            List<byte> results = new List<byte>();
            foreach (var element in elements)
            {
                // 检查 OID 是否在高校联盟允许的范围
                var index = FindOidOffset(element.OID);
                if (index == -1)
                    throw new ArgumentException($"高校联盟 UHF 标准不支持 OID 为 {element.OID} 的 User Bank 内容元素");

                var bytes = EncodeUserElementContent(element.OID, element.Content);
                int length = bytes.Length;
                // OID 第一字节的 6-bit
                // length 第一字节的后 2-bit + 第二字节的 8-bit
                byte first = (byte)(((element.OID << 2) & 0xfc) + ((length >> 8) & 0x03));
                byte second = (byte)(length & 0xff);

                results.Add(first);
                results.Add(second);
                results.AddRange(bytes);
            }

            // 补齐偶数字节数
            if ((results.Count % 2) != 0)
                results.Add(0);
            else if (tail_zero)
            {
                // 为了解析时候能终止，填充 0
                results.Add(0);
                results.Add(0);

                // TODO: 元素数也可以根据 ContentParameter 内容测算，可以不用特意填充 0
            }

            return results.ToArray();
        }

        // 解码 User Bank
        public static List<GaoxiaoUserElement> DecodeUserBank(byte[] data)
        {
            /*
            // testing
            if (data.Length > 0)
                data[0] = 0;
            */

            List<GaoxiaoUserElement> results = new List<GaoxiaoUserElement>();
            int start = 0;
            for (; ; )
            {
                if (start >= data.Length || data[start] == 0)
                    break;
                // OID 第一字节的 6-bit
                int oid = (data[start] >> 2) & 0x3f;
                // length 第一字节的后 2-bit + 第二字节的 8-bit
                int length = (data[start] & 0x03) << 8;
                length |= data[start + 1] & 0xff;

                if (start + length > data.Length)
                    throw new Exception($"start={start} 处的 element 长度 {length} 越界");

                List<byte> bytes = new List<byte>();
                for (int i = 0; i < length; i++)
                {
                    bytes.Add(data[start + 2 + i]);
                }

                results.Add(new GaoxiaoUserElement
                {
                    OID = oid,
                    Content = DecodeUserElementContent(oid, bytes.ToArray())
                });

                start += 2 + length;
            }

            return results;
        }

        public static string GetUserElementName(int oid)
        {
            switch(oid)
            {
                case 3:
                    return "所属馆标识 OwnerLibrary";
                case 4:
                    return "卷册信息 SetInformation";
                    case 5:
                    return "馆藏类别与状态 TypeOfUsage";
                case 6:
                    return "馆藏位置 ItemLocation";
                case 11:
                    return "馆际互借借入馆标识 ILL-BorrowingLibrary";
                case 12:
                    return "馆际互借事务号 ILL-BorrowingTransactionNumber";
                case 14:
                    return "备选的馆藏标识符(条码号) AlternativeItemIdentifier";
                case 15: 
                    return "临时馆藏位置 TemporaryItemLocation";
                case 16: 
                    return "主题 Subject";
                case 24: 
                    return "分馆标识 SubsidiaryOfAnOwnerLibrary";
                case 26: 
                    return "ISBN/ISSN";
                case 27:
                    return "备选项(数字平台计划用作 AOI)";
                case 28:
                case 29:
                case 30:
                case 31:
                    return "备选项";
            }

            return $"暂未命名";
        }

        public static byte[] EncodeUserElementContent(int oid, string content)
        {
            if (oid == 3)
            {
                // 所属馆标识 Owner Library
                /*
取值方式：2 字节整型数。参照中华人民共和国教育部行业标准 JY/T1001-2012，《教育管理信
息-教育管理基础代码》中的《中国高等院校代码表》取值，以 5 位数字所代表的高等院校代
码来标识所属馆。其中，针对首位数字为 9（军事院校，例如：90001 代表国防大学）的情况，
在存放时，需要将 9 变成为 6，然后以 2 字节整型数存储，在读取后，需要将 6 变成为 9，恢
复成原始代码。
* */
                if (string.IsNullOrEmpty(content) == false
                    && content[0] == '6')
                    throw new ArgumentException($"机构代码不允许用 '6' 开头，因为它被 '9' 挪用了");

                return GetLibraryCode("所属馆标识");
            }

            if (oid == 4)
            {
                // 卷册信息 Set Information
                /*
取值方式：总 4 字节定长字段，其中：高 2 字节存放卷册总数（最多 65536 卷册），低 2 字节
存放卷册序号（1 - 65536）。
                * */
                if (content.Contains("/") == false)
                    throw new ArgumentException($"卷册信息 '{content}' 格式不正确，应为 n/m 形态");
                var parts = StringUtil.ParseTwoPart(content, "/");
                string index_string = parts[0];
                string count_string = parts[1];

                if (UInt16.TryParse(index_string, out UInt16 index_value) == false)
                    throw new ArgumentException($"卷册信息 '{content}' 中的序号部分 '{index_string}'不合法。应该是 1-65536 范围内数字");
                if (UInt16.TryParse(count_string, out UInt16 count_value) == false)
                    throw new ArgumentException($"卷册信息 '{content}' 中的总数部分 '{index_string}'不合法。应该是 1-65536 范围内数字");

                var index_bytes = BitConverter.GetBytes(index_value);
                var count_bytes = BitConverter.GetBytes(count_value);

                index_bytes = Compact.ReverseBytes(index_bytes);
                count_bytes = Compact.ReverseBytes(count_bytes);

                List<byte> results = new List<byte>();
                results.AddRange(count_bytes);  // 总数为“高”byte，高即是字节流中靠前的意思
                results.AddRange(index_bytes);

                return results.ToArray();
            }

            if (oid == 5)
            {
                // 馆藏类别与状态 type of usage
                // 总 2 字节定长字段，其中：高字节存放馆藏类别(主限定标识)，低字节存放馆藏状态(次限定标识)
                var parts = StringUtil.ParseTwoPart(content, ".");
                string pimary_string = parts[0];
                string secondary_string = parts[1];

                if (UInt16.TryParse(pimary_string, out UInt16 pimary_value) == false)
                    throw new ArgumentException($"馆藏类别 '{content}' 中的左侧部分 '{pimary_string}'不合法。应该是 0-255 范围内数字");
                UInt16 secondary_value = 0;
                if (string.IsNullOrEmpty(secondary_string) == false)
                {
                    if (UInt16.TryParse(secondary_string, out secondary_value) == false)
                        throw new ArgumentException($"馆藏类别 '{content}' 中的右侧部分 '{secondary_string}'不合法。应该是 0-255 范围内数字");
                }

                List<byte> results = new List<byte>();
                results.Add((byte)pimary_value);
                results.Add((byte)secondary_value);

                Debug.Assert(results.Count == 2);
                return results.ToArray();
            }

            if (oid == 6 || oid == 12 || oid == 14 || oid == 15 || oid == 16 || oid == 24 || oid == 26)
            {
                // 6: 馆藏位置 Item Location
                // 12: 馆际互借事务号 ILL Borrowing Transaction Number
                // 14: 备选的馆藏标识符(条码号) Alternative Item Identifier
                // 15: 临时馆藏位置 Temporary Item Location
                // 16: 主题 Subject
                // 24: 分馆标识 Subsidiary of an Owner Library
                // 26: ISBN/ISSN
                return Encoding.UTF8.GetBytes(content);
            }

            if (oid == 11)
            {
                // 馆际互借借入馆标识 ILL Borrowing Library
                /*
取值方式：2 字节整型数。参照中华人民共和国教育部行业标准 JY/T1001-2012，《教育管理信
息-教育管理基础代码》中的《中国高等院校代码表》取值，以 5 位数字所代表的高等院校代
码来标识所属馆。其中，针对首位数字为 9（军事院校，例如：90001 代表国防大学）的情况，
在存放时，需要将 9 变成为 6，然后以 2 字节整型数存储，在读取后，需要将 6 变成为 9，恢
复成原始代码。
                * */
                return GetLibraryCode("馆际互借借入馆标识");
            }


            if (oid >= 27 && oid <= 31)
            {
                // 注：
                // 27: 备选项，数字平台计划用作 AOI

                // 保留的字段
                return Encoding.UTF8.GetBytes(content);
            }

            // 其他。暂时用 hex string 来表示
            return Element.FromHexString(content);

            byte[] GetLibraryCode(string name)
            {
                if (content.Length != 5)
                    throw new ArgumentException($"{name}应该是 5 位数字，但实际是 {content.Length} 位('{content}')");
                // 若第一字符为 '9'，需要变为 '6'
                if (content[0] == '9')
                    content = "6" + content.Substring(1);
                if (UInt16.TryParse(content, out UInt16 value) == false)
                    throw new ArgumentException($"{name} '{content}' 不合法。应该是 5 位数字");
                var result = BitConverter.GetBytes(value);
                Debug.Assert(result.Length == 2);
                // return result;
                return Compact.ReverseBytes(result);
            }
        }

        // 解码用户区元素
        public static string DecodeUserElementContent(int oid, byte[] data)
        {
            if (data.Length == 0)
                return "";

            if (oid == 3)
            {
                // 所属馆标识 Owner Library
                /*
取值方式：2 字节整型数。参照中华人民共和国教育部行业标准 JY/T1001-2012，《教育管理信
息-教育管理基础代码》中的《中国高等院校代码表》取值，以 5 位数字所代表的高等院校代
码来标识所属馆。其中，针对首位数字为 9（军事院校，例如：90001 代表国防大学）的情况，
在存放时，需要将 9 变成为 6，然后以 2 字节整型数存储，在读取后，需要将 6 变成为 9，恢
复成原始代码。
* */
                if (data.Length != 2)
                    throw new Exception($"OID 为 3 时，data 应为 2 字节(但现在为 {data.Length} 字节)");
#if REMOVED
                data = Compact.ReverseBytes(data);

                var result = BitConverter.ToUInt16(data, 0).ToString().PadLeft(5, '0');
                if (result[0] == '6')
                    return "9" + result.Substring(1);
                return result;
#endif
                return DecodeLibraryCode(data);
            }

            if (oid == 4)
            {
                // 卷册信息 Set Information
                /*
取值方式：总 4 字节定长字段，其中：高 2 字节存放卷册总数（最多 65536 卷册），低 2 字节
存放卷册序号（1 - 65536）。
                * */
                if (data.Length != 4)
                    throw new Exception($"OID 为 4 时，data 应为 4 字节(但现在为 {data.Length} 字节)");

                byte[] high = new byte[2];
                Array.Copy(data, high, 2);
                high = Compact.ReverseBytes(high);

                byte[] low = new byte[2];
                Array.Copy(data, 2, low, 0, 2);
                low = Compact.ReverseBytes(low);

                var no = BitConverter.ToUInt16(low, 0).ToString();
                var count = BitConverter.ToUInt16(high, 0).ToString();
                return no + "/" + count;
            }

            if (oid == 5)
            {
                // 馆藏类别与状态 type of usage
                // 总 2 字节定长字段，其中：高字节存放馆藏类别(主限定标识)，低字节存放馆藏状态(次限定标识)
                if (data.Length != 2)
                    throw new Exception($"OID 为 5 时，data 应为 2 字节(但现在为 {data.Length} 字节)");

                byte pimary = data[0];
                byte secondary = data[1];
                return ((int)pimary).ToString() + "." + ((int)secondary).ToString();
                /*
主限定标识(应用类别) 取值(数字) 次限定标识(馆藏状态) 取值(数字)
文献 0
    可外借 0
    不可外借 1
    剔旧 2
    处理中 3
    自定义 4-255
光盘 1
    可外借 0
    不可外借 1
    处理中 2
    自定义 3-255
架标/层标 2 自定义 0-255
证件 3 自定义 0-255
设备 4 自定义 0-255
预留 5-255 自定义 0-255
                * 
                 * */
            }

            if (oid == 6 || oid == 12 || oid == 14 || oid == 15 || oid == 16 || oid == 24 || oid == 26)
            {
                // 6: 馆藏位置 Item Location
                // 12: 馆际互借事务号 ILL Borrowing Transaction Number
                // 14: 备选的馆藏标识符 Alternative Item Identifier
                // 15: 临时馆藏位置 Temporary Item Location
                // 16: 主题 Subject
                // 24: 分馆标识 Subsidiary of an Owner Library
                // 26: ISBN/ISSN
                return Encoding.UTF8.GetString(data);
            }

            if (oid == 11)
            {
                // 馆际互借借入馆标识 ILL Borrowing Library
                /*
取值方式：2 字节整型数。参照中华人民共和国教育部行业标准 JY/T1001-2012，《教育管理信
息-教育管理基础代码》中的《中国高等院校代码表》取值，以 5 位数字所代表的高等院校代
码来标识所属馆。其中，针对首位数字为 9（军事院校，例如：90001 代表国防大学）的情况，
在存放时，需要将 9 变成为 6，然后以 2 字节整型数存储，在读取后，需要将 6 变成为 9，恢
复成原始代码。
                * */
                if (data.Length != 2)
                    throw new Exception($"OID 为 11 时，data 应为 2 字节(但现在为 {data.Length} 字节)");

#if REMOVED
                data = Compact.ReverseBytes(data);

                var result = BitConverter.ToUInt16(data, 0).ToString().PadLeft(5, '0');
                if (result[0] == '6')
                    return "9" + result.Substring(1);
                return result;
#endif
                return DecodeLibraryCode(data);
            }

            if (oid >= 27 && oid <= 31)
            {
                // 保留的字段
                return Encoding.UTF8.GetString(data);
            }

            // 其他。暂时用 hex string 来表示
            return Element.GetHexString(data);

            string DecodeLibraryCode(byte[] bytes)
            {
                if (bytes.Length != 2)
                    throw new Exception($"DecodeLibraryCode()，data 应为 2 字节(但现在为 {bytes.Length} 字节)");

                bytes = Compact.ReverseBytes(bytes);

                var result = BitConverter.ToUInt16(bytes, 0).ToString().PadLeft(5, '0');
                if (result[0] == '6')
                    return "9" + result.Substring(1);
                return result;
            }
        }

        #endregion

#if REMOVED
        // 校验 OI 元素内容是否符合高校联盟 OI 的格式
        public static string VerifyOI(LogicChip chip_param)
        {
            var oi = chip_param.FindElement(ElementOID.OI)?.Text;
            if (VerifyOI(oi) == false)
                return oi;
            return null;
        }
#endif

        // 根据 LogicChip 对象构造标签内容
        // 本函数实现注:
        // 1) 本函数构造的 result.UserBank 是 User Bank 内容，因此里面不会包含 PII ContentParameter 元素
        // 2) 调用前要协调好 UII(在 chip 的 PII 元素中) 和 OI(AOI)的关系。如果 UII 中已经包含了 OI 部分(即 xxx.xxx 格式)，那么 chip 中就不应该包含 OI 或者 AOI 元素了
        // 3) build_user_bank 为 false 的情况下，OI(AOI) 会被迫放到 result.EpcBank 的 UII 中返回
        // 4) 如果 chip 中同时包含了 PII 和 OI(AOI)，并且 build_user_bank == true，那么机构代码会放到 User Bank 中返回，EPC Bank 中只是不包含点的 PII。这是为了兼容高校联盟的做法(EPC 中不是 UII 形态)
        // 5) chip 中的 TypeOfUsage 元素内是通用值，在写入 UHF 高校联盟格式时要先映射转换为其私有值
        // parameters:
        //      build_user_bank 是否要构造 User Bank 内容。如果不构造的话，Content Parameter 中就不会包含任何 index 信息
        //      epc_info        要创建的 EPC Bank 的一些预定义值。
        //                      如果 epc_info.ContentParameters 不为 null，则表示直接用它进入 EPC
        //      style           处理风格。
        public static BuildTagResult BuildTag(LogicChip chip_param,
            bool build_user_bank,
            bool eas = true,
            GaoxiaoEpcInfo epc_info = null,
            string style = "")
        {
            // 2024/1/2
            var chip = chip_param.Clone();

            // 2023/11/7
            // 对于不写入 User Bank 的情况进行检查
            if (build_user_bank == false)
            {
                var tou = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                if (IsNormalBookTou(tou) == false)
                {
                    // 如果具有非 '1?' 的 TypeOfUsage，这表明不应缺乏 User Bank。
                    // 因为如果缺了 User Bank，则会被默认为图书类型，这样就令读出标签的人产生误会了
                    throw new ArgumentException($"又要写入 TypeOfUsage 元素(内容为 '{tou}')，又不让写入 User Bank，这种组合不被支持。因为这样会被读出时误当作图书类型标签");
                }
            }

            // 是否普通图书类型?
            bool IsNormalBookTou(string value)
            {
                /*
                if (string.IsNullOrEmpty(value)
    || value.StartsWith("1")
    || value.StartsWith("2")
    || value.StartsWith("7"))
                    return true;
                */
                // 注: 20 和 70 是特殊的图书，不被当作普通图书
                if (string.IsNullOrEmpty(value)
|| value.StartsWith("1"))
                    return true;
                return false;
            }

            // 2023/10/24
            // 把 AOI 变为 OI
            var element_aoi = chip.FindElement(ElementOID.AOI);
            var element_oi = chip.FindElement(ElementOID.OI);
            if (element_aoi != null && element_oi == null)
            {
                chip.SetElement(ElementOID.OI, element_aoi.Text, false);
                chip.RemoveElement(ElementOID.AOI);
            }

            List<GaoxiaoUserElement> user_elements = new List<GaoxiaoUserElement>();
            foreach (var element in chip.Elements)
            {
                // user bank 中不包含 PII 和 ContentParameter
                if (element.OID == ElementOID.PII
                    || element.OID == ElementOID.ContentParameter)
                    continue;

                // 检查 OI 元素内容是否符合高校 OI 的形态。如果不符合，则转为用备选的 27 元素
                if (element.OID == ElementOID.OI)
                {
                    if (string.IsNullOrEmpty(element.Text))
                        continue;
                    if (VerifyOI(element.Text) == false)
                        element.OID = (ElementOID)27;
                }

                // 2023/10/24
                // 高校联盟的 AOI 只能用 27
                if (element.OID == ElementOID.AOI)
                    element.OID = (ElementOID)27;

                var user_element = new GaoxiaoUserElement
                {
                    OID = (int)element.OID,
                    Content = element.Text,
                };

                // 2023/10/28
                // 要从国标元素内容映射到高校联盟形态
                if (user_element.OID == (int)ElementOID.TypeOfUsage)
                {
                    // 国标 --> 中立
                    var neutral = TypeOfUsage_GBToNeutral(user_element.Content);
                    // 中立 --> 高校
                    user_element.Content = TypeOfUsage_NeutralToGaoxiao(neutral);
                }

                // 2023/11/7
                // 要从国标元素内容映射到高校联盟形态
                if (user_element.OID == (int)ElementOID.SetInformation)
                {
                    // 国标 --> 中立
                    var neutral = SetInformation_GBToNeutral(user_element.Content);
                    // 中立 --> 高校
                    user_element.Content = SetInformation_NeutralToGaoxiao(neutral);
                }

                user_elements.Add(user_element);
            }

            byte[] user_bank = null;

            if (build_user_bank)
                user_bank = EncodeUserBank(user_elements, false);   // true

            if (epc_info == null)
                epc_info = new GaoxiaoEpcInfo();
            epc_info.Lending = !eas;
            // Version(5) Picking(1) Reserve(1)
            // epc_info.Version = 1;   // ?
            
            if (epc_info.ContentParameters == null)
            {
                if (build_user_bank)
                    epc_info.ContentParameters = BuildContentParameter(user_elements);
                else
                    epc_info.ContentParameters = new int[0];
            }
            epc_info.PII = chip.FindElement(ElementOID.PII)?.Text;

            // 重新获取一次
            element_aoi = chip.FindElement(ElementOID.AOI);
            element_oi = chip.FindElement(ElementOID.OI);

            string oi = element_oi?.Text;
            if (string.IsNullOrEmpty(oi))
                oi = element_aoi?.Text;

            // 2023/10/24
            // 如果不让写入 user_bank，但又想要写入机构代码，那就只能包含在 PII 之内了
            if (build_user_bank == false && string.IsNullOrEmpty(oi) == false)
            {
                if (epc_info.PII.Contains("."))
                    throw new ArgumentException($"chip 中的 PII 元素包含了点字符，然而 chip 同时又具备 OI 或者 AOI 元素，这是不允许的。只能二者选其一");
                epc_info.PII = oi + "." + epc_info.PII;

                throw new ArgumentException("又要写入 OI 字段，又不让写入 User Bank，这种组合不被支持。因为高校联盟格式的 EPC 册号码中不允许出现字符 '.'，所以机构代码无法以 UII 部件方式进入 EPC Bank");
            }

            // 编码高校联盟 EPC bank。注意返回的内容没有包含前 4 bytes(校验码和 PC)
            var epc_payload = EncodeGaoxiaoEpcPayload(epc_info);

            // 构造 PC (Protocal Control Word)
            var pc_info = new ProtocolControlWord();
            pc_info.UMI = build_user_bank;  // 2023/12/5 修改
            pc_info.XPC = false;
            pc_info.ISO = false;
            pc_info.AFI = 0;
            // 最后计算载荷的实际长度
            pc_info.LengthIndicator = epc_payload.Length / 2; // 这里是 word(一个 word 等于两个 bytes) 数
            var pc = UhfUtility.EncodePC(pc_info);

            List<byte> temp = new List<byte>();
            temp.AddRange(pc);
            temp.AddRange(epc_payload);
            epc_payload = temp.ToArray();

            return new BuildTagResult
            {
                EpcBank = epc_payload,
                UserBank = user_bank
            };
        }

        // 验证字符串是否符合高校联盟 OI 的形态要求
        public static bool VerifyOI(string oi)
        {
            if (oi == null)
                return false;
            if (oi.Length != 5)
                return false;
            foreach (var ch in oi)
            {
                if (char.IsDigit(ch) == false)
                    return false;
            }

            char first_char = oi[0];
            if (first_char == '8'
                || first_char == '7'
                || first_char == '6')
                return false;

            return true;
        }

        static int[] BuildContentParameter(List<GaoxiaoUserElement> user_elements)
        {
            int first_oid = offset_table[0];
            List<int> results = new List<int>();
            foreach (var element in user_elements)
            {
                int oid = element.OID;
                if (oid < first_oid)
                    continue;
                int index = FindOidOffset(oid);
                if (index >= 0)
                    results.Add(oid);
            }

            return results.ToArray();
        }

#if REMOVED
        static int[] BuildContentParameter(LogicChip chip)
        {
            List<int> results = new List<int>();
            foreach (var element in chip.Elements)
            {
                int oid = (int)element.OID;
                int index = FindOidOffset(oid);
                if (index == -1)
                    results.Add(oid);
            }

            return results.ToArray();
        }
#endif

        static int FindOidOffset(int oid)
        {
            int index = Array.IndexOf(offset_table, oid);
            return index;
        }

        public class ParseGaoxiaoResult : NormalResult
        {
            public ProtocolControlWord PC { get; set; }
            // 逻辑标签内容
            public LogicChip LogicChip { get; set; }
            // EPC 信息
            public GaoxiaoEpcInfo EpcInfo { get; set; }
            // user bank 解析出的若干元素
            public List<GaoxiaoUserElement> UserElements { get; set; }

            // 根据 UMI 和 Content Parameter 数组判断 User Bank 是否包含了 OI(或AOI,27) 元素
            public static bool ContainUserBankOiElement(
                bool umi,
                int[] content_parameters)
            {
                if (content_parameters == null)
                    return false;
                if (umi
                    && content_parameters != null
                    && (content_parameters.Contains((int)ElementOID.OI)
                    // || content_parameters.Contains((int)ElementOID.AOI)
                    || content_parameters.Contains(27))
                    )
                    return true;
                return false;
            }

            // 获得解释文字
            public string GetDescription(byte[] epc_bank, byte[] user_bank)
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine("=== EPC Bank ===");
                text.AppendLine($"Hex(十六进制内容):\t{ByteArray.GetHexTimeStampString(epc_bank)?.ToUpper()}");
                text.AppendLine($"PC(协议控制字):\t{this.PC.ToString()}");
                text.AppendLine($"高校联盟字段:\t{this.EpcInfo.ToString()}");
                text.AppendLine("=== User Bank ===");
                text.AppendLine($"Hex(十六进制内容):\t{ByteArray.GetHexTimeStampString(user_bank)?.ToUpper()}");
                if (this.UserElements != null)
                {
                    foreach (var element in this.UserElements)
                    {
                        text.AppendLine($"{element.OID} {GaoxiaoUtility.GetUserElementName(element.OID)}:\t'{element.Content}'");
                    }
                }
                return text.ToString();
            }
        }

        // 解析标签信息。
        // 根据 EPC 和 USR 两个 bank 的信息来进行解析
        // parameters:
        //      style   如果包含 "checkUMI"，表示要对 UMI=on 和 ContentParameters 的存在关系进行校验。否则缺省为不校验
        public static ParseGaoxiaoResult ParseTag(
            byte[] epc_bank,
            byte[] user_bank,
            string style = "")
        {
            if (StringUtil.IsInList("dontCheckUMI", style))
                throw new ArgumentException("style 参数值的 dontCheckUMI 用法已经废弃。请改用 checkUMI");

            bool checkUMI = StringUtil.IsInList("checkUMI", style);
            List<string> warnings = new List<string>();

            ProtocolControlWord pc = null;
            try
            {
                // 协议控制字 Protocol Control Word
                pc = UhfUtility.ParsePC(epc_bank, 2);

                /*
                if (pc.ISO == true)
                {
                    if (pc.AFI != 0xc2)
                        throw new Exception("目前仅支持 AFI 为 0xc2 的图书馆应用家族标签");
                }
                else
                {
                    throw new Exception("目前暂不支持 GC1/EPC 的 MB01");
                }
                */

                // 跳过 4 个 byte
                List<byte> bytes = new List<byte>(epc_bank);
                bytes.RemoveRange(0, 4);

                GaoxiaoEpcInfo epc_info = null;
                if (bytes.Count > 0)
                {
                    epc_info = DecodeGaoxiaoEpcPayload(bytes.ToArray(),
                        Math.Min(pc.LengthIndicator * 2, bytes.Count));
                    if (pc.UMI == false
                        && epc_info.ContentParameters.Length != 0)
                    {
                        if (pc.AFI == 0
                            && pc.ISO == false
                            && pc.UMI == false
                            && pc.XPC == false
                            && pc.LengthIndicator == 8)
                        {
                            return new ParseGaoxiaoResult
                            {
                                Value = 0,
                                ErrorInfo = "空白标签",
                                ErrorCode = "blank",
                                LogicChip = new LogicChip(),
                                EpcInfo = epc_info,
                                UserElements = new List<GaoxiaoUserElement>(),
                                PC = pc,
                            };
                        }

                        if (checkUMI == true)
                            return new ParseGaoxiaoResult
                            {
                                Value = -1,
                                ErrorInfo = "标签内容无法解析。EPC UMI 位和(高校联盟) ContentParameters 不符",
                                ErrorCode = "parseEpcError",
                                EpcInfo = epc_info,
                                UserElements = new List<GaoxiaoUserElement>(),
                                PC = pc,
                            };
                        else
                            warnings.Add("UMI 值 false 和 ContentParameter 有值矛盾了");
                    }
                }

                var convert_value_to_gb = StringUtil.IsInList("convertValueToGB", style);

                List<GaoxiaoUserElement> elements = null;
                LogicChip chip = new LogicChip();
                if (user_bank != null)
                {
                    elements = DecodeUserBank(user_bank);

                    if (epc_info != null)
                        chip.SetElement(ElementOID.PII, epc_info.PII);
                    foreach (var element in elements)
                    {
                        var oid = (ElementOID)element.OID;

                        if (convert_value_to_gb)
                        {
                            /*
                            if (oid == ElementOID.SetInformation)
                            {
                                // TODO: 将 1/1 规范化为国标形态
                                continue;
                            }
                            */
                            // 从高校值转换为国标值
                            if (oid == ElementOID.TypeOfUsage)
                            {
                                // 高校 --> 中立
                                var neutral = TypeOfUsage_GaoxiaoToNeutral(element.Content);
                                // 中立 --> 国标
                                element.Content = TypeOfUsage_NeutralToGB(neutral);
                            }

                            // 2023/11/7
                            // 从高校值转换为国标值
                            if (oid == ElementOID.SetInformation)
                            {
                                // 高校 --> 中立
                                var neutral = SetInformation_GaoxiaoToNeutral(element.Content);
                                // 中立 --> 国标
                                element.Content = SetInformation_NeutralToGB(neutral);
                            }

                            // 2023/11/28
                            // 将 OID 27 的元素转为国标 AOI
                            if ((int)oid == 27)
                                oid = ElementOID.AOI;
                        }

                        chip.SetElement(oid, element.Content, false);
                    }
                }
                else
                {
                    // 2023/10/24
                    if (epc_info != null)
                        chip.SetElement(ElementOID.PII, epc_info.PII);
                }

                var result = new ParseGaoxiaoResult
                {
                    LogicChip = chip,   // 如果 chip 为 null，表示没有 User Bank
                    EpcInfo = epc_info,
                    UserElements = elements, // 如果 elements 为 null，是因为没有 User Bank
                    PC = pc,
                };
                if (warnings.Count > 0)
                {
                    result.ErrorInfo = StringUtil.MakePathList(warnings, "; ");
                    result.ErrorCode = "warning";
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ParseGaoxiaoResult
                {
                    Value = -1,
                    ErrorInfo = $"标签内容无法解析。{ex.Message}",
                    ErrorCode = "parseError",
                    PC = pc,
                };
            }
        }

        public class ConvertResult : NormalResult
        {
            public List<GaoxiaoUserElement> Elements { get; set; }
        }

#if DEVELOPING

        // 将元素值从高校联盟标准转换为国标形态
        public static ConvertResult ToGB(List<GaoxiaoUserElement> elements)
        {
            List<GaoxiaoUserElement> results = new List<GaoxiaoUserElement>();
            foreach (var element in elements)
            {
                if (element.OID == (int)ElementOID.TypeOfUsage)
                {
                }

            }

            return new ConvertResult { Elements = results };
        }

#endif

        // TypeOfUsage: 高校 --> 中立
        static string TypeOfUsage_GaoxiaoToNeutral(string gaoxiao)
        {
            // xxx.xxx
            var parts = StringUtil.ParseTwoPart(gaoxiao, ".");
            string pimary = parts[0];
            string secondary = parts[1];
            string neutral = "gx:" + pimary;
            switch (pimary)
            {
                /*
* 附录 B：馆藏类别与状态参考代码
主限定标识(应用类别) 取值(数字) 
----次限定标识(馆藏状态) 取值(数字)
文献 0
----可外借 0
----不可外借 1
----剔旧 2
----处理中 3
----自定义 4-255
光盘 1
----可外借 0
----不可外借 1
----处理中 2
----自定义 3-255
架标/层标 2 
----自定义 0-255
证件 3 
----自定义 0-255
设备 4 
----自定义 0-255
预留 5-255 
----自定义 0-255
* */
                case "0":
                    if (secondary == "1")
                        neutral = "{非流通馆藏}";
                    else if (secondary == "2")
                        neutral = "{被剔除馆藏}";
                    else
                        neutral = "{馆藏}";
                    break;
                case "1":
                    neutral = "{光盘}";
                    break;
                case "2":
                    neutral = "{层架标}";
                    break;
                case "3":
                    neutral = "{读者证}";
                    break;
                case "4":
                    neutral = "{设备}";
                    break;
            }
            return neutral;
        }

        // TypeOfUsage: 中立 --> 高校
        static string TypeOfUsage_NeutralToGaoxiao(string neutral)
        {
            // 无法转换的值
            if (neutral.Contains(":"))
                return neutral;

            if (neutral.Contains("{") == false)
            {
                throw new ArgumentException($"neutral 参数值 '{neutral}' 中没有花括号，不符合中立格式 TypeOfUsage 值的形态要求");
            }

            // {xxx.xxx}
            string temp = StringUtil.Unquote(neutral, "{}");
            var parts = StringUtil.ParseTwoPart(temp, ".");
            string pimary = parts[0];
            string secondary = parts[1];
            string result = "neutral:" + pimary;
            switch (pimary)
            {

                /*
* 附录 B：馆藏类别与状态参考代码
主限定标识(应用类别) 取值(数字) 
----次限定标识(馆藏状态) 取值(数字)
文献 0
----可外借 0
----不可外借 1
----剔旧 2
----处理中 3
----自定义 4-255
光盘 1
----可外借 0
----不可外借 1
----处理中 2
----自定义 3-255
架标/层标 2 
----自定义 0-255
证件 3 
----自定义 0-255
设备 4 
----自定义 0-255
预留 5-255 
----自定义 0-255
* */
                case "馆藏":
                    result = "0.0";
                    break;
                case "非流通馆藏":
                    result = "0.1";
                    break;
                case "光盘":
                    result = "1";
                    break;
                case "层架标":
                    result = "2";
                    break;
                case "被剔除馆藏":
                    result = "0.2";
                    break;
                case "读者证":
                    result = "3";
                    break;
                case "设备":
                    result = "4";
                    break;
            }
            return result;
        }

        // TypeOfUsage: 中立 --> 国标
        static string TypeOfUsage_NeutralToGB(string neutral)
        {
            // 无法转换的值
            if (neutral.Contains(":"))
                return neutral;

            if (neutral.Contains("{") == false)
            {
                throw new ArgumentException($"neutral 参数值 '{neutral}' 中没有花括号，不符合中立格式 TypeOfUsage 值的形态要求");
            }

            // {xxx.xxx}
            string temp = StringUtil.Unquote(neutral, "{}");
            var parts = StringUtil.ParseTwoPart(temp, ".");
            string pimary = parts[0];
            string secondary = parts[1];
            string result = "neutral:" + pimary;
            switch (pimary)
            {
                case "馆藏":
                    result = "1";
                    break;
                case "非流通馆藏":
                    result = "2";
                    break;
                case "光盘":
                    result = "4";
                    break;
                case "层架标":
                    result = "3";
                    break;
                case "被剔除馆藏":
                    result = "7";
                    break;
                case "读者证":
                    result = "8";
                    break;
                case "设备":
                    result = "9";
                    break;
            }
            return result + "0";
        }

        // TypeOfUsage: 国标 --> 中立
        static string TypeOfUsage_GBToNeutral(string gb)
        {
            if (string.IsNullOrEmpty(gb))
                throw new ArgumentException($"参数 gb 的值不应为空");

            /* 国标 TypeOfUsage 值定义:
主限定符(HEX) 类别 次限定符(hex) 用途
-------------------------------------------------------
0 订购馆藏
		0 订购馆藏,没有特别说明
		1 订购馆藏,用于自动处理
		2 订购馆藏,用于手工处理
		3~F 用于未来使用
1 流通馆藏
		0 流通馆藏,没有特别说明
		1 流通馆藏,用于自动分拣
		2 流通馆藏,不能用于自动分拣
		3 流通馆藏,不能用于脱机事务
		4 流通馆藏,脱机不能归还
		5 流通馆藏,脱机无作业或不能归还
		6~F 用于未来使用
2 非流通馆藏
		0 非流通馆藏,没有特别说明
		1~F 用于未来使用
3~4 本地自定义
		0 本地应用,没有子类说明
		1~F 用于未来使用
5 用于未来使用
		0 未来应用,没有子类说明
		1~F 用于未来使用
6 没有标签用途信息
		0 未定义用途。当数据元素被锁定时,可以使用此值
		1~F 不用
7 被剔除馆藏
		0 被剔除馆藏,没有特别说明
		1 被剔除馆藏,用于出售
		2 被剔除馆藏,用于已出售的文献
		3 被剔除馆藏,要处理的文献
		4~F 用于未来使用
8 读者证
		0 读者证,没有特别说明
		1 读者证,成人证
		2 读者证,青少年证
		3 读者证,儿童证
		4~F 用于未来此子类使用
9 图书馆设备
		0 图书馆设备,没有特别说明
		1 个人计算机
		2 视频放映机
		3 投影仪
		4 白板
		5~F 用于未来使用
A~F 未来使用
		0 用于未来使用,未定义子类
		1~F 用于未来使用

             * 
             * */
            string primary = "";
            string secondary = "";

            if (gb.Length > 0)
                primary = gb.Substring(0, 1);
            if (gb.Length > 1)
                secondary = gb.Substring(1, 1);

            string neutral = "gb:" + gb;
            switch (primary)
            {
                case "1":
                    neutral = "{馆藏}";
                    break;
                case "2":
                    neutral = "{非流通馆藏}";
                    break;
                case "4":
                    neutral = "{光盘}";
                    break;
                case "3":
                    neutral = "{层架标}";
                    break;
                case "7":
                    neutral = "{被剔除馆藏}";
                    break;
                case "8":
                    neutral = "{读者证}";
                    break;
                case "9":
                    neutral = "{设备}";
                    break;
            }
            return neutral;
        }

        static string SetInformation_GBToNeutral(string gb)
        {
            var error = DigitalPlatform.RFID.Element.VerifySetInformation(gb);
            if (error != null)
                throw new ArgumentException($"SetInformation 元素内容 '{gb}' 不合法: {error}");
            // 中间位置加一个 '/'
            if ((gb.Length % 2) == 1)
                throw new ArgumentException($"SetInformation 元素内容 '{gb}' 不合法。应该为偶数字符数");
            int center_offs = gb.Length / 2;
            var text = gb.Insert(center_offs, "/");
            return "{" + text + "}";
        }

        static string SetInformation_NeutralToGB(string neutral)
        {
            var text = StringUtil.Unquote(neutral, "{}");
            var parts = StringUtil.ParseTwoPart(text, "/");
            int max_len = Math.Max(parts[0].Length, parts[1].Length);
            return parts[0].PadLeft(max_len, '0')
                + parts[1].PadLeft(max_len, '0');
        }

        static string SetInformation_GaoxiaoToNeutral(string gaoxiao)
        {
            return "{" + gaoxiao + "}";
        }

        static string SetInformation_NeutralToGaoxiao(string nuetral)
        {
            return StringUtil.Unquote(nuetral, "{}");
        }

        // 根据 EPC Bank 判断是不是“望湖洞庭”格式
        public static bool IsWhdt(byte[] epc_bank)
        {
            var parse_result = GaoxiaoUtility.ParseTag(epc_bank, null, ""); // 注意，没有包含 checkUMI 表示不要检查 UMI 和 ContentParameters 是否具备之间的关系
            if (parse_result.PC == null
                || parse_result.EpcInfo == null)
                return false;
            var pc = parse_result.PC;
            // 注: 望湖洞庭有一批标签没有 User Bank 内容，但 EPC 内容和先前的无异
            if (/*pc.UMI == true
                && */(pc.AFI == 0 || pc.AFI == 4)
                && pc.XPC == false
                && pc.ISO == false)
            {

            }
            else
                return false;

            var epc_info = parse_result.EpcInfo;

            // content parameter 16 24 28 30
            var cp = new int[] { 16, 24, 28, 30 };
            if (cp.SequenceEqual(epc_info.ContentParameters) == false)
                return false;

            if (epc_info.Reserve != 0)
                return false;
            /*
            if (epc_info.Picking != 1 && epc_info.Picking != 0)
                return false;
            if (epc_info.Version != 5 && epc_info.Version != 1)
                return false;
            */
            if (epc_info.EncodingType != 0)
                return false;
            return true;
        }



#if DEVELOPING
        // 将元素值从国标形态转换为高校标准
        public static ConvertResult ToGaoxiao(List<GaoxiaoUserElement> elements)
        {
            List<GaoxiaoUserElement> results = new List<GaoxiaoUserElement>();
            foreach (var element in elements)
            {

            }

            return new ConvertResult { Elements = results };
        }
#endif

#if DEVELOPING

        // 将元素值从高校联盟形态转换为中立形态
        public static List<GaoxiaoUserElement> ToNeutral(List<GaoxiaoUserElement> elements)
        {
            List<GaoxiaoUserElement> results = new List<GaoxiaoUserElement>();
            foreach (var element in elements)
            {
                if (element.OID == (int)(ElementOID.TypeOfUsage))
                {
                    // xxx.xxx
                    var parts = StringUtil.ParseTwoPart(element.Content, ".");
                    string pimary = parts[0];
                    string secondary = parts[1];
                    string neutral = "gx:" + pimary;
                    switch (pimary)
                    {

                        /*
 * 附录 B：馆藏类别与状态参考代码
主限定标识(应用类别) 取值(数字) 
----次限定标识(馆藏状态) 取值(数字)
文献 0
----可外借 0
----不可外借 1
----剔旧 2
----处理中 3
----自定义 4-255
光盘 1
----可外借 0
----不可外借 1
----处理中 2
----自定义 3-255
架标/层标 2 
----自定义 0-255
证件 3 
----自定义 0-255
设备 4 
----自定义 0-255
预留 5-255 
----自定义 0-255
* */
                        case "0":
                            neutral = "{馆藏}";
                            break;
                        case "1":
                            neutral = "{光盘}";
                            break;
                        case "2":
                            neutral = "{层架标}";
                            break;
                        case "3":
                            neutral = "{读者证}";
                            break;
                        case "4":
                            neutral = "{设备}";
                            break;
                    }
                    element.Content = neutral;
                }

                if (element.OID == (int)(ElementOID.SetInformation))
                {
                    // xx/xx
                    // 文字保持原样，外加 {}
                    if (string.IsNullOrEmpty(element.Content) == false)
                        element.Content = "{" + element.Content + "}";
                }
            }

            return results;
        }

        // 将元素值从中立形态转换为高校联盟形态
        public static List<GaoxiaoUserElement> FromNeutral(List<GaoxiaoUserElement> elements)
        {
            List<GaoxiaoUserElement> results = new List<GaoxiaoUserElement>();
            foreach (var element in elements)
            {
                // 将 AOI 转换到 27
                // 将不是 5 位数字的 OI 转换到 27

                if (element.OID == (int)(ElementOID.TypeOfUsage))
                {
                    if (element.Content.Contains("{") == false)
                    {

                    }
                    else
                    {
                        // {xxx.xxx}
                        string temp = StringUtil.Unquote(element.Content, "{}");
                        var parts = StringUtil.ParseTwoPart(temp, ".");
                        string pimary = parts[0];
                        string secondary = parts[1];
                        string neutral = "neutral:" + pimary;
                        switch (pimary)
                        {

                            /*
     * 附录 B：馆藏类别与状态参考代码
    主限定标识(应用类别) 取值(数字) 
    ----次限定标识(馆藏状态) 取值(数字)
    文献 0
    ----可外借 0
    ----不可外借 1
    ----剔旧 2
    ----处理中 3
    ----自定义 4-255
    光盘 1
    ----可外借 0
    ----不可外借 1
    ----处理中 2
    ----自定义 3-255
    架标/层标 2 
    ----自定义 0-255
    证件 3 
    ----自定义 0-255
    设备 4 
    ----自定义 0-255
    预留 5-255 
    ----自定义 0-255
    * */
                            case "馆藏":
                                neutral = "0";
                                break;
                            case "光盘":
                                neutral = "1";
                                break;
                            case "层架标":
                                neutral = "2";
                                break;
                            case "读者证":
                                neutral = "3";
                                break;
                            case "设备":
                                neutral = "4";
                                break;
                        }
                        element.Content = neutral;
                    }
                }

                if (element.OID == (int)(ElementOID.SetInformation))
                {
                    // xx/xx
                    element.Content = StringUtil.Unquote(element.Content, "{}");
                }
            }
            return results;
        }

#endif
    }

    // User Bank 内容元素(高校联盟标准)
    public class GaoxiaoUserElement
    {
        public int OID { get; set; }
        public string Content { get; set; }

        public bool Equal(GaoxiaoUserElement element)
        {
            if (this.OID != element.OID)
                return false;
            if (this.Content != element.Content)
                return false;
            return true;
        }
    }

    // 高校联盟 EPC 载荷信息结构
    public class GaoxiaoEpcInfo
    {
        // *** EPC 载荷的第 1 字节

        // 安全位：第 7bit（最高 bit）为安全位，有 0 和 1 两个取值，0 代表未出借状态（馆内存放），1 代表出借状态。
        public bool Lending { get; set; }

        // 预留位: 第 5-6bit 为预留位，例如可用于扩展分拣信息(初始化值为 00)。
        public int Reserve { get; set; }

        // 分拣信息: 第 0-4bit 为分拣信息，可用于标识馆藏分拣时所属的分拣箱号，目前共定义了 32 种不同的取值。
        public int Picking { get; set; }

        // *** EPC 载荷的第二字节

        // 编码方式
        /*
    6-7bit（最高 2bit）为编码方式，其中：
    00 编码方式 1，EPC 容量为 96bit (12Byte)
    01 编码方式 2，EPC 容量为 128bit (16Byte)
    10 编码方式 3，EPC 容量为 144bit 或以上 (≥18Byte)
    11 编码方式 4，保留（暂无定义）
        * */
        public int EncodingType { get; set; }

        // 数据模型版本
        //  0-5bit（低 6bit）为版本说明，可用于标识 64 种不同的版本信息(0-63)。
        public int Version { get; set; }

        // *** EPC 载荷的第 3-4 字节

        // 内容索引。OID 的数组
        public int[] ContentParameters { get; set; }

        // *** EPC 载荷的第 5-12/16/18 字节

        // 馆藏标识符
        public string PII { get; set; }


        public override string ToString()
        {
            return $"PII={PII},EncodingType={EncodingType},Version={Version},ContentParameters='{OidToString(ContentParameters)}',Lending={Lending},Reserve={Reserve},Picking={Picking}";
        }

        static string OidToString(int[] list)
        {
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (var v in list)
            {
                if (i > 0)
                    text.Append(",");
                text.Append($"{v}({GaoxiaoUtility.GetUserElementName(v)})");
                // text.Append(v);
                i++;
            }

            return text.ToString();
        }
    }
}
