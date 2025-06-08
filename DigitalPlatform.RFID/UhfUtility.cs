using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

using static DigitalPlatform.RFID.LogicChip;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// 超高频标签实用函数
    /// </summary>
    public static class UhfUtility
    {
        // 2023/11/2
        // 用 new_bytes 覆盖 old_bytes 内容，清除 new_bytes 末尾延伸出来的较长部分的内容为 0
        // return:
        //      返回一个足以实现覆盖效果的 byte []。也就是说，不会造成覆盖后最后末尾还残留了一些脏内容
        public static byte[] OverwriteBank(byte[] old_bytes,
            byte[] new_bytes,
            bool force_even_bytes = false)
        {
            if (old_bytes == null || old_bytes.Length == 0)
                return new_bytes;

            if (new_bytes == null)
                new_bytes = new byte[0];

            // new_bytes 内容较长
            if (old_bytes.Length <= new_bytes.Length)
                return new_bytes;

            // 探测延展部分是否包含非 0 的 bytes
            int last_none_zero_offset = -1;    // 最后非 0 byte 的偏移
            for (int i = new_bytes.Length; i < old_bytes.Length; i++)
            {
                if (old_bytes[i] != 0)
                    last_none_zero_offset = i;
            }

            // 延展部分 没有发现非 0 byte
            if (last_none_zero_offset == -1)
                return new_bytes;   // 沿用 new_bytes。这样写入内容更少，效率高

            Debug.Assert(last_none_zero_offset >= 0);

            // 在 new_bytes 末尾延展若干 0 byte，确保实现清空覆盖效果
            var results = new List<byte>(new_bytes);
            for (int i = new_bytes.Length; i <= last_none_zero_offset; i++)
            {
                results.Add(0);
            }
            Debug.Assert(results.Count <= Math.Max(old_bytes.Length, new_bytes.Length));

            // 确保偶数个 bytes
            if (force_even_bytes && (results.Count % 2) != 0)
                results.Add(0);

            return results.ToArray();
        }

        public static bool IsBlankTag(byte[] epc_bank,
            byte[] user_bank)
        {
            return IsBlankEpcBank(epc_bank, user_bank);

            /*
            if (user_bank != null && user_bank.Length > 0)
            {
                bool not_empty = false;
                foreach (byte b in user_bank)
                {
                    if (b != 0)
                    {
                        not_empty = true;
                        break;
                    }
                }

                if (not_empty == true)
                    return false;
                return true;
            }
            else
            {
                // 标签制造的时候不存在 User Bank，那么需要判断 EPC bank

                // TODO: EPC 空白的标志是什么？
            }

            return true;
            */
        }

        public static bool IsBlankEpcBank(byte[] epc_bank,
            byte[] user_bank = null)
        {
            // 彻底空白的 EPC
            if (epc_bank.Length == 4
    && epc_bank[2] == 0 && epc_bank[3] == 0)
                return true;

            bool is_user_bank_empty = false;
            if (user_bank == null)
                is_user_bank_empty = true;
            else
            {
                if (user_bank.Where(o => o != 0).Any())
                    is_user_bank_empty = false;
                else
                    is_user_bank_empty = true;
            }

            var pc = ParsePC(epc_bank, 2);

            // 标签厂家出厂时内容特征
            // 3000 --> PC Word --> (二进制) 0011 0000 0000 0000
            // --> Length=6word UMI=0 XPC=0 Toggle=0 attribute=0x00
            if (pc.LengthIndicator == 6
                && (pc.UMI == false || (pc.UMI == true && is_user_bank_empty == true))
                && pc.XPC == false && pc.AFI == 0 && pc.ISO == false
                && epc_bank[4] == 0xe2/* && epc_bank[5] == 0*/)
                return true;

            return false;
        }

        // 判断标签内容是否采用了 ISO28560-4 (UHF 国标)编码格式
        public static bool IsISO285604Format(byte[] epc_bank,
            byte[] user_bank)
        {
            var pc = ParsePC(epc_bank, 2);
            if ((pc.AFI == 0xc2 || pc.AFI == 0x07)
                && pc.ISO == true)
                return true;
            else
                return false;
            /*
            if (user_bank != null && pc.UMI == false)
                return false;
            if (pc.ISO == false)
                return false;

            if (pc.UMI == true)
            {
                if (user_bank != null && user_bank.Length >= 1)
                {
                    // 检查 User Bank 第一 byte，DSFID 是否为 0x06
                    if (user_bank[0] != 0x06)
                        return false;
                }
            }
            */
            return true;
        }

        // 根据指定的 PC 创建空的 EPC Bank 内容
        // 注: 不包含最开始的 CRC word
        public static byte[] BuildBlankEpcBank()
        {
            List<byte> bytes = new List<byte>();
            {
                ProtocolControlWord pc = new ProtocolControlWord();
                pc.UMI = false;
                pc.XPC = false;
                pc.ISO = false;
                pc.AFI = 0;
                pc.LengthIndicator = 6; // 载荷为 6
                bytes.AddRange(UhfUtility.EncodePC(pc));
            }

            {
                List<byte> payload = new List<byte>();
                payload.Add(0xe2);
                payload.Add(0);

                var guid = Guid.NewGuid().ToByteArray();
                for (int i = 0; i < 10; i++)
                {
                    payload.Add(guid[i]);
                }
                while (payload.Count < 12)
                {
                    payload.Add(0);
                }
                Debug.Assert(payload.Count == 12);
                bytes.AddRange(payload);
            }

            return bytes.ToArray();
        }

        #region 编码

        // 根据 LogicChip 对象构造标签内容
        // parameters:
        //      umi     PC(协议控制字)中 UMI 是否为 on?
        //      uii_include_oi  是否要把 OI 装配到 UII 字符串中？
        //      style   如果包含 "afi_eas_on" 表示 AFI 用 0x07(否则用 0xc2)
        //              如果包含 "oi_in_userbank" 表示把 OI 写入 User Bank, 并且 EPC 中的 UII 里面不包含 OI 部分。否则会在 EPC 中 UII 中包含 OI 部分，而 UserBank 中不会有 OI 元素
        public static BuildTagResult BuildTag(LogicChip chip_param,
            bool umi,   // 2025/6/7
            bool uii_include_oi = true,
            string style = "")
        {
            // 2024/1/2
            var chip = chip_param.Clone();

            var oiInUserBank = StringUtil.IsInList("oi_in_userbank", style);

            string pii = chip.FindElement(ElementOID.PII)?.Text;
            string oi = chip.FindElement(ElementOID.OI)?.Text;
            string aoi = chip.FindElement(ElementOID.AOI)?.Text;

            string uii = "";
            if (oiInUserBank)
            {
                uii = pii;
            }
            else
            {
                if (string.IsNullOrEmpty(oi) == false)
                {
                    uii = oi + "." + pii;
                    chip.RemoveElement(ElementOID.OI);
                }
                else if (string.IsNullOrEmpty(aoi) == false)
                {
                    uii = aoi + "." + pii;
                    chip.RemoveElement(ElementOID.AOI);
                }
                else
                    uii = pii;
            }

            chip.RemoveElement(ElementOID.PII);

            byte[] user_bank = null;
            if (chip.Elements.Count > 0)
                user_bank = EncodeUserBank(chip,
                4 * 52,
                4,
                true,
                out string block_map);

            byte afi = 0xc2;
            if (StringUtil.IsInList("afi_eas_on", style) == true)
                afi = 0x07;

            // 注意返回的 bytes 不包含校验码 1 word 部分(这部分一般是由读写器驱动自动计算和补充写入)
            var epc_payload = EncodeEpcBank(uii, umi, afi);

            return new BuildTagResult
            {
                EpcBank = epc_payload,
                UserBank = user_bank
            };
        }


        // 2020/12/15
        // 编码 MB01 也就是 EPC Bank
        // 注意返回的 bytes 不包含校验码 1 word 部分(这部分一般是由读写器驱动自动计算和补充写入)
        public static byte[] EncodeEpcBank(string uii, 
            bool umi = true,    // 2025/6/7
            byte afi = 0xc2)
        {
            var uii_bytes = EncodeUII(uii, true);

            ProtocolControlWord pc_info = new ProtocolControlWord();
            pc_info.UMI = umi;
            pc_info.XPC = false;
            pc_info.ISO = true;
            pc_info.AFI = afi;
            pc_info.LengthIndicator = uii_bytes.Length / 2;
            var pc_bytes = UhfUtility.EncodePC(pc_info);

            List<byte> results = new List<byte>(pc_bytes);
            results.AddRange(uii_bytes);
            return results.ToArray();
        }

        // 2020/12/15
        // 编码 MB11 也就是 User Bank
        // parameters:
        //      max_bytes   最大容量字节数。注意不包含 head 字节
        public static byte[] EncodeUserBank(LogicChip chip,
            int max_bytes,
            int block_size,
            bool word,
            out string block_map)
        {
            block_map = "";

            // 注意 chip 里面不应该包含 PII 元素
            if (chip.FindElement(ElementOID.PII) != null)
                throw new ArgumentException($"chip 的 Elements 中不应包含 PII 元素");

            var bytes = chip.GetBytes(max_bytes,
                block_size,
                GetBytesStyle.ReserveSequence,
                out block_map);
            var head = new byte[] { 0x06 };
            List<byte> results = new List<byte>(head);
            results.AddRange(bytes);

            // 补齐偶数字节
            if (word && (results.Count % 2) != 0)
                results.Add(0);

            return results.ToArray();
        }

        // 将字符翻译为 URN40 对照表中的数值
        static int GetUrn40Decimal(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A' + 1;
            }

            if (c == '-')
                return 27;
            if (c == '.')
                return 28;
            if (c == ':')
                return 29;
            if (c >= '0' && c <= '9')
                return c - '0' + 30;

            return 0;   // 表示 c 不在对照表中
        }

        static List<int> GetUrn40Decimal(string text)
        {
            List<int> results = new List<int>();
            foreach (var c in text)
            {
                results.Add(GetUrn40Decimal(c));
            }

            return results;
        }

        // 为编码 URN 40 需要的一个段落
        public class Segment
        {
            // 段类型
            // 为 digit table utf8-one-byte utf8-two-byte utf8-triple-byte 之一
            public string Type { get; set; }
            // 段内文字
            public string Text { get; set; }
        }

        // (为编码 URN 40)把文字切割为若干个段落
        public static List<Segment> SplitSegment(string text)
        {
            List<Segment> results = new List<Segment>();

            List<char> buffer = new List<char>();   // 缓冲区
            string buffer_type = "";    // 为 digit table utf8-one-byte utf8-two-byte utf8-triple-byte 之一
            foreach (char c in text)
            {
                int d = GetUrn40Decimal(c);
                if (d != 0)
                {
                    // *** 表内字符

                    if (char.IsDigit(c) == true)
                    {
                        if (buffer_type != "digit"
                            && buffer.Count > 0)
                        {
                            results.Add(new Segment
                            {
                                Type = buffer_type,
                                Text = new string(buffer.ToArray())
                            });

                            buffer.Clear();
                        }

                        if (c == '0' && buffer.Count == 0)
                        {
                            // 2025/2/28
                            // digit 段落的开头不允许是 '0'
                            // 后面还要继续判断和处理
                        }
                        else
                        {
                            buffer.Add(c);
                            buffer_type = "digit";
                            continue;
                        }
                    }

                    if (char.IsDigit(c) == false)
                    {
                        if (buffer_type != "table"
                            && buffer.Count > 0)
                        {
                            results.Add(new Segment
                            {
                                Type = buffer_type,
                                Text = new string(buffer.ToArray())
                            });

                            buffer.Clear();
                        }
                        buffer.Add(c);
                        buffer_type = "table";
                    }
                    else
                    {
                        // throw new Exception("不可能到达这里");
                        // 2025/2/28
                        buffer.Add(c);
                        buffer_type = "table";
                    }
                }
                else
                {
                    // *** 非表内字符

                    int count = GetUtf8ByteCount(c);
                    if (count == 1)
                    {
                        if (buffer_type != "utf8-one-byte"
                            && buffer.Count > 0)
                        {
                            results.Add(new Segment
                            {
                                Type = buffer_type,
                                Text = new string(buffer.ToArray())
                            });

                            buffer.Clear();
                        }
                        buffer.Add(c);
                        buffer_type = "utf8-one-byte";
                    }
                    else if (count == 2)
                    {
                        if (buffer_type != "utf8-two-byte"
                            && buffer.Count > 0)
                        {
                            results.Add(new Segment
                            {
                                Type = buffer_type,
                                Text = new string(buffer.ToArray())
                            });

                            buffer.Clear();
                        }
                        buffer.Add(c);
                        buffer_type = "utf8-two-byte";
                    }
                    else if (count == 3)
                    {
                        if (buffer_type != "utf8-triple-byte"
                            && buffer.Count > 0)
                        {
                            results.Add(new Segment
                            {
                                Type = buffer_type,
                                Text = new string(buffer.ToArray())
                            });

                            buffer.Clear();
                        }
                        buffer.Add(c);
                        buffer_type = "utf8-triple-byte";
                    }
                    else
                    {
                        throw new Exception($"无法编码字符 '{c}'，因为它是 UTF-8 {count} bytes 形态");
                    }
                }
            }

            if (buffer.Count > 0)
            {
                results.Add(new Segment
                {
                    Type = buffer_type,
                    Text = new string(buffer.ToArray())
                });
            }

            if (results.Count <= 1 && results[0].Type != "digit")
                return results;

            // 把前面有连续 '0' 的 digit 段落拆分为多个段落

            // 把不足 9 字符的 digit 段落和前后的 table 段落合并
            List<Segment> merged = new List<Segment>();

            Segment prev = null;
            foreach (var segment in results)
            {
                if (segment.Type == "digit"
&& segment.Text.Length < 9)
                    segment.Type = "table";

                if (prev != null)
                {
                    if (segment.Type == "table"
    && prev.Type == "table")
                    {
                        prev = new Segment
                        {
                            Type = "table",
                            Text = prev.Text + segment.Text
                        };
                        continue;
                    }
                    merged.Add(prev);
                    prev = null;
                }
                prev = segment;
            }
            if (prev != null)
                merged.Add(prev);

            // 如果段落为小于 9 字符的 digit 类型，要改为 table 类型
            foreach (var segment in merged)
            {
                if (segment.Type == "digit"
                    && segment.Text.Length < 9)
                    segment.Type = "table";
            }

            return merged;
        }

        public static int GetUtf8ByteCount(char c)
        {
            return Encoding.UTF8.GetByteCount(new char[] { c });
        }

        // 编码 UII
        // parameters:
        //      word    是否要自动补齐偶数字节数
        public static byte[] EncodeUII(string text, bool word = true)
        {
            List<byte> results = new List<byte>();

            var segments = SplitSegment(text);
            foreach (var segment in segments)
            {
                if (segment.Type == "digit")
                    results.AddRange(EncodeLongNumericString(segment.Text));
                else if (segment.Type == "table")
                    results.AddRange(
                        EncodeTableDecimals(GetUrn40Decimal(segment.Text))
                        );
                else if (segment.Type == "utf8-one-byte")
                {
                    foreach (var c in segment.Text)
                    {
                        // ISO 646 输出
                        results.Add(0xfc);
                        results.Add((byte)c);
                    }
                }
                else if (segment.Type == "utf8-two-byte")
                {
                    foreach (var c in segment.Text)
                    {
                        results.Add(0xfd);
                        results.AddRange(Encoding.UTF8.GetBytes(new char[] { c }));
                    }
                }
                else if (segment.Type == "utf8-triple-byte")
                {
                    foreach (var c in segment.Text)
                    {
                        results.Add(0xfe);
                        results.AddRange(Encoding.UTF8.GetBytes(new char[] { c }));
                    }
                }
                else
                    throw new Exception($"无法识别的 segment type '{segment.Type}'");
            }

            // 补齐偶数字节
            if (word && (results.Count % 2) != 0)
                results.Add(0);

            return results.ToArray();
        }

        // 将 (URN Code 40) 数值每三个编码为两个 bytes
        public static byte[] EncodeTableDecimals(List<int> chars)
        {
            // 补足到 3 的整倍数
            int delta = 3 - (chars.Count % 3);
            if (delta < 3)
            {
                for (int i = 0; i < delta; i++)
                {
                    chars.Add((char)0);
                }
            }

            List<byte> results = new List<byte>();
            int offs = 0;
            while (offs < chars.Count)
            {
                var c1 = chars[offs++];
                var c2 = chars[offs++];
                var c3 = chars[offs++];

                results.AddRange(EncodeTriple(c1, c2, c3));
            }

            return results.ToArray();
        }

        // 编码范围在 'A'-'9' 内的三个字符为两个 bytes
        public static byte[] EncodeTriple(int c1, int c2, int c3)
        {
            List<byte> results = new List<byte>();
            uint v = (1600 * (uint)c1) + (40 * (uint)c2) + (uint)c3 + 1;
            if (v > UInt16.MaxValue)
                throw new Exception($"{v} 溢出 16 bit 整数范围");
            return Compact.ReverseBytes(BitConverter.GetBytes((UInt16)v));
        }

        // 以 long numeric string 方式编码一个数字字符串
        // 算法可参考：
        // https://www.ipc.be/~/media/documents/public/operations/rfid/ipc%20rfid%20standard%20for%20test%20letters.pdf?la=en
        public static byte[] EncodeLongNumericString(string text)
        {
            if (text.Length < 9)
                throw new ArgumentException($"用于编码的数字字符串不应短于 9 字符(但现在是 {text.Length} 字符)");

            byte[] bytes = Compact.IntegerCompact(text);
            if (bytes.Length < 4)
                throw new ArgumentException($"数字字符串编码后不应短于 4 bytes(但现在是 {bytes.Length} bytes)");

            byte second = (byte)((((text.Length - 9) << 4) & (byte)0xf0) | ((bytes.Length - 4) & 0x0f));

            List<byte> results = new List<byte>();
            results.Add(0xfb);
            results.Add(second);
            results.AddRange(bytes);

            return results.ToArray();
        }

        #endregion

        #region 解码

        public class ParseGbResult : NormalResult
        {
            public ProtocolControlWord PC { get; set; }
            public string UII { get; set; }

            // 逻辑标签内容
            public LogicChip LogicChip { get; set; }

            // 返回更安全的 UII。意思是如果 User Bank 中有机构代码，必要时会取它和 PII 一起构造 UII
            public string SafetyUII
            {
                get
                {
                    if (UII != null && UII.Contains("."))
                        return UII;
                    if (LogicChip == null)
                        return UII;
                    var oi = LogicChip.FindElement(ElementOID.OI)?.Text;
                    if (string.IsNullOrEmpty(oi))
                        oi = LogicChip.FindElement(ElementOID.AOI)?.Text;

                    if (string.IsNullOrEmpty(oi) == false)
                        return oi + "." + UII;
                    return UII;
                }
            }

            // 2025/2/27
            // 获得解释文字
            public string GetDescription(byte[] epc_bank, byte[] user_bank)
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine("=== EPC Bank ===");
                text.AppendLine($"Hex(十六进制内容):\t{ByteArray.GetHexTimeStampString(epc_bank)?.ToUpper()}");
                text.AppendLine($"PC(协议控制字):\t{this.PC.ToString()}");
                text.AppendLine("=== User Bank ===");
                text.AppendLine($"Hex(十六进制内容):\t{ByteArray.GetHexTimeStampString(user_bank)?.ToUpper()}");
                if (this.LogicChip != null)
                {
                    foreach (var element in this.LogicChip.Elements)
                    {
                        text.AppendLine($"{(int)element.OID} {element.OID.ToString()}:\t'{element.Text}' (hex:{ByteArray.GetHexTimeStampString(element.Content)?.ToUpper()})");
                    }
                }
                return text.ToString();
            }

        }

        // 解析一个 UHF 国标标签全部内容
        // 注意，返回的 LogicChip 里面并没有 OI 和 PII 元素。如果需要，可以通过调用 AddPiiOi() 函数添加
        public static ParseGbResult ParseTag(byte[] epc_bank,
            byte[] user_bank,
            int block_size,
            string block_map = "")
        {
            var epc_result = ParseEpcBank(epc_bank);
            if (epc_result.Value == -1)
                return new ParseGbResult
                {
                    Value = -1,
                    ErrorInfo = epc_result.ErrorInfo,
                    ErrorCode = epc_result.ErrorCode
                };

            LogicChip chip = null;
            if (user_bank != null)
            {
                var user_result = ParseUserBank(user_bank,
                    block_size,
                    block_map);
                if (user_result.Value == -1)
                    return new ParseGbResult
                    {
                        Value = -1,
                        ErrorInfo = user_result.ErrorInfo,
                        ErrorCode = user_result.ErrorCode
                    };
                chip = user_result.LogicChip;
            }

            return new ParseGbResult
            {
                PC = epc_result.PC,
                UII = epc_result.UII,
                LogicChip = chip,   // chip 如果为空，表示没有 User Bank
            };
        }

        // 为 chip 中添加(从 UII 中得来的) PII 和 OI 元素
        public static void AddPiiOi(string uii, LogicChip chip)
        {
            if (string.IsNullOrEmpty(uii))
                return;

            var pii = GetPiiPart(uii);
            var oi = GetOiPart(uii, false);

            if (string.IsNullOrEmpty(pii) == false)
                chip.SetElement(ElementOID.PII, pii);
            if (string.IsNullOrEmpty(oi) == false)
                chip.SetElement(ElementOID.OI, oi);
        }

        // 获得 oi.pii 的 oi 部分
        public static string GetOiPart(string oi_pii, bool return_null)
        {
            // 2023/11/6
            if (oi_pii == null)
            {
                if (return_null)
                    return null;
                return "";
            }

            if (oi_pii.IndexOf(".") == -1)
            {
                if (return_null)
                    return null;
                return "";
            }
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[0];
        }

        // 获得 oi.pii 的 pii 部分
        public static string GetPiiPart(string oi_pii)
        {
            // 2023/11/6
            if (oi_pii == null)
                return null;
            if (oi_pii.IndexOf(".") == -1)
                return oi_pii;
            var parts = StringUtil.ParseTwoPart(oi_pii, ".");
            return parts[1];
        }


        // 解析 MB01 数据结构
        // parameters:
        //      data    MB01 包含的数据。注意开头 2 bytes 是校验码，从 0x10 bits 偏移位置开始是协议控制字(PC)
        public static ParseEpcBankResult ParseEpcBank(byte[] data)
        {
            if (data.Length <= 4)
                return new ParseEpcBankResult
                {
                    Value = -1,
                    ErrorInfo = $"data 长度不足",
                    ErrorCode = "lengthError",
                };

            var pc = ParsePC(data, 2);
            if (pc.ISO == true)
            {
                if (pc.AFI != 0xc2 && pc.AFI != 0x07)
                    return new ParseEpcBankResult
                    {
                        Value = -1,
                        ErrorInfo = $"(PC.ISO=true, PC.AFI={Element.GetHexString((byte)pc.AFI)}) 目前仅支持 AFI 为 0xc2 或 0x07 的图书馆应用家族标签",
                        ErrorCode = "notSupportAFI",
                    };
            }
            else
            {
                return new ParseEpcBankResult
                {
                    Value = -1,
                    ErrorInfo = "(PC.ISO=false) 目前暂不支持 GC1/EPC 的 MB01(EPC Bank)",
                    ErrorCode = "notSupportISO",
                };
            }

            // 载荷部分长度
            int payloadLength = data.Length - 4;
            if (payloadLength < pc.LengthIndicator * 2)
                throw new Exception($"data 中载荷部分长度(byte 数) {payloadLength} 小于 PC.LengthIndicator 中的 word 数 {pc.LengthIndicator}");

            ParseEpcBankResult result = new ParseEpcBankResult();
            result.PC = pc;
            result.UII = DecodeUII(data,
                4,
                Math.Min(pc.LengthIndicator * 2, payloadLength));
            return result;
        }

        // 解析 User Bank。注意 data 第一 byte 是 DSFID，第二 byte 开始才是数据
        public static ParseUserBankResult ParseUserBank(byte[] data,
            int block_size,
            string block_map = "")
        {
            if (data.Length <= 1)
                return new ParseUserBankResult
                {
                    Value = -1,
                    ErrorInfo = "data 长度不足"
                };
            try
            {
                // 解码 User Bank，和解码高频标签的 Bytes 一样
                List<byte> temp = new List<byte>(data);
                temp.RemoveAt(0);

                var chip = LogicChip.From(temp.ToArray(),
                    block_size,
                    block_map);

                return new ParseUserBankResult { LogicChip = chip };
            }
            catch (Exception ex)
            {
                return new ParseUserBankResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message
                };
            }
        }

        // 解析协议控制字(Protocol Control Word)
        public static ProtocolControlWord ParsePC(byte[] data, int start)
        {
            if (data.Length - start < 2)
                throw new ArgumentException($"data 内应包含至少 {(start + 2)} bytes(但现在只有 {data.Length})", nameof(data));

            ProtocolControlWord result = new ProtocolControlWord();
            // 0x0 1 2 3 4 bits
            result.LengthIndicator = (data[start] >> 3) & 0x1f;
            result.UMI = (data[start] & 0x4) != 0;
            result.XPC = (data[start] & 0x2) != 0;
            result.ISO = (data[start] & 0x1) != 0;
            result.AFI = data[start + 1];
            return result;
        }

        // 观察 EPC Bank 中的 UMI 标志位
        public static bool GetUMI(byte[] data, int start)
        {
            if (data.Length < 2)
                return false;

            // 0x0 1 2 3 4 bits
            // result.LengthIndicator = (data[start] >> 3) & 0x1f;
            return (data[start] & 0x4) != 0;
            // result.XPC = (data[start] & 0x2) != 0;
            // result.ISO = (data[start] & 0x1) != 0;
            // result.AFI = data[start + 1];
        }

        public static byte[] EncodePC(ProtocolControlWord pc_info)
        {
            List<byte> results = new List<byte>();

            int value = (pc_info.LengthIndicator << 3) & 0xf8;
            value += ((pc_info.UMI ? 1 : 0) << 2) & 0x04;
            value += ((pc_info.XPC ? 1 : 0) << 1) & 0x02;
            value += (pc_info.ISO ? 1 : 0);
            results.Add((byte)value);
            results.Add((byte)pc_info.AFI);
            return results.ToArray();
        }

        // 解码 UII
        public static string DecodeUII(byte[] data, int start, int length)
        {
            int offs = start;
            StringBuilder result = new StringBuilder();
            while (offs - start < length)
            {
                byte b = data[offs];
                if (b <= 0xfa)
                {
                    // 16 bit 按照 URN40 解释为 3 个字符
                    result.Append(UrnCode40_DecodeOneWord(data, offs));
                    offs += 2;
                }
                else if (b == 0xfb)
                {
                    // long numeric string
                    result.Append(DecodeLongNumericString(data, start, out int used));
                    offs += used;
                }
                else if (b == 0xfc)
                {
                    // next byte is ISO/IEC 646 IRV character
                    result.Append((char)(data[offs + 1]));
                    offs += 2;
                }
                else if (b == 0xfd)
                {
                    string temp = Encoding.UTF8.GetString(data, offs + 1, 2);
                    result.Append(temp);
                    offs += 3;
                }
                else if (b == 0xfe)
                {
                    string temp = Encoding.UTF8.GetString(data, offs + 1, 3);
                    result.Append(temp);
                    offs += 4;
                }
            }

            return result.ToString();
        }

        // 将 URN40 对照表中的数值翻译为字符
        static char GetUrn40Char(uint d)
        {
            if (d >= 1 && d <= 26)
            {
                return (char)((uint)'A' + (d - 1));
            }

            if (d == 27)
                return '-';
            if (d == 28)
                return '.';
            if (d == 29)
                return ':';

            if (d >= 30 && d <= 39)
                return (char)((uint)'0' + (d - 30));

            // 表示 d 不在对照表中
            throw new ArgumentException($"值 {d} 不在 URN40 表中");
        }


        // 解析一个 Word(=2 bytes)
        static string UrnCode40_DecodeOneWord(byte[] data,
            int start)
        {
            if (start >= data.Length - 1)
                return "";

            uint v = ((((uint)data[start]) << 8) & 0xff00) + ((uint)data[start + 1] & 0x00ff);
            // 2020/8/17
            if (v == 0)
                return "";
            /*
            byte[] word = new byte[2];
            word[0] = data[start + 0];
            word[1] = data[start + 1];
            uint v = BitConverter.ToUInt16(word, 0);
            */
            v--;
            uint c1 = v / 1600;
            uint rest = (v % 1600);
            uint c2 = rest / 40;
            uint c3 = (rest % 40);
            List<char> results = new List<char>();
            if (c1 != 0)
                results.Add(GetUrn40Char(c1));
            if (c2 != 0)
                results.Add(GetUrn40Char(c2));
            if (c3 != 0)
                results.Add(GetUrn40Char(c3));
            return new string(results.ToArray());
        }

        // 解析 Long Numeric String
        // 注意 start 位置的 byte 应该是 0xfb
        // parameters:
        //      used_bytes  返回用掉的 bytes 数
        public static string DecodeLongNumericString(byte[] data,
            int start,
            out int used_bytes)
        {
            used_bytes = 0;
            byte lead = data[start];
            if (lead != 0xfb)
                throw new ArgumentException("开始的第一个 byte 必须为 0xfb", $"{nameof(data)}, {nameof(start)}");
            byte second = data[start + 1];
            // 解码后的数字应有的字符数
            int decimal_length = ((second >> 4) & 0x0f) + 9;
            // 编码后的 bytes 数
            int encoded_bytes = (second & 0x0f) + 4;

            if (encoded_bytes > 19)
                throw new Exception($"Long Numeric String 的编码后 bytes 数({encoded_bytes})不合法，应小于等于 19");

            string result = "";
            {
                byte[] bytes = new byte[encoded_bytes];
                Array.Copy(data, start + 2, bytes, 0, encoded_bytes);
                result = Compact.IntegerExtract(bytes);
            }

            // 补齐前方的 '0'
            result.PadLeft(decimal_length, '0');
            used_bytes = encoded_bytes + 2;
            return result;
        }

        #endregion

        // 根据 EPC 内容 bytes 构造包括 CRC bytes 的全部 hex string
        public static string EpcBankHex(byte[] epc_content)
        {
            var crc_bytes = CRC16.crc16x25(epc_content);
            Debug.Assert(crc_bytes.Length == 2);
            var crc_hex = ByteArray.GetHexTimeStampString(crc_bytes).ToUpper();
            return crc_hex + Element.GetHexString(epc_content);
        }
    }

    public class ParseEpcBankResult : NormalResult
    {
        // Protocol Control Word
        public ProtocolControlWord PC { get; set; }
        // 
        public string UII { get; set; }

        public override string ToString()
        {
            return $"UII='{UII}',PC='{PC.ToString()}',{base.ToString()}";
        }
    }

    public class ParseUserBankResult : NormalResult
    {
        public LogicChip LogicChip { get; set; }
    }

    // 协议控制字(PC)之解析结构
    public class ProtocolControlWord
    {
        // Length Indicator
        public int LengthIndicator { get; set; }
        // User Memory Indicator
        public bool UMI { get; set; }
        // XPC 是否存在？
        public bool XPC { get; set; }
        // 是 ISO AFI 还是 GS1/EPC ?
        public bool ISO { get; set; }
        // 图书馆应用的 AFI 应该为 0xc2
        public byte AFI { get; set; }

        public override string ToString()
        {
            return $"LengthIndicator={LengthIndicator}, UMI={UMI}, XPC={XPC}, ISO={ISO}, AFI={Element.GetHexString((byte)AFI)}";
        }
    }

    public class BuildTagResult : NormalResult
    {
        public byte[] EpcBank { get; set; }
        public byte[] UserBank { get; set; }
    }

    // XTID 结构解析
    // 这个 URL 是 GS1 提供的 Decoder
    // https://www.gs1.org/services/tid-decoder
    // https://www.gs1.org/services/epc-encoderdecoder
    public class ExtendedTagIdentification
    {
        // XTID 指示符
        public bool XtidIndicator { get; set; }

        // 安全指示符
        public bool SecurityIndicator { get; set; }

        // 文件指示符
        public bool FileIndicator { get; set; }

        //  Mask 设计者 ID
        public int MaskDesignerID { get; set; }

        // 标签型号
        public int TagModelNumber { get; set; }

        // 注: 此前的 Member 为 Short Tag Identification 结构

        // byte [] 形态的 SerialNumber
        public byte[] SerialNumber { get; set; }

        // STID-URI 字符串
        public string StidUri { get; set; }

        // UInt64 形态的 SerialNumber
        public UInt64 SerialNumberInteger
        {
            get
            {
                if (SerialNumber == null)
                    return 0;
                List<byte> results = new List<byte>(SerialNumber);
                while (results.Count < 8)
                {
                    results.Insert(0, 0);
                }
                return BitConverter.ToUInt64(Compact.ReverseBytes(results.ToArray()), 0);
            }
        }


        public static ExtendedTagIdentification Parse(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                throw new ArgumentException($"bytes 至少要有 4 bytes");

            if (bytes[0] != 0xe2)
                throw new ArgumentException("第一 byte 不是 0xE2，表明这不是合法的 TID 内容");

            var result = new ExtendedTagIdentification();
            result.XtidIndicator = (bytes[1] & 0x80) != 0;
            result.SecurityIndicator = (bytes[1] & 0x40) != 0;
            result.FileIndicator = (bytes[1] & 0x20) != 0;
            /*
            result.MDID = (((int)(bytes[1]) << 5) & 0x1f0)
                + ((bytes[2] >> 4) & 0x0f);
            */
            result.MaskDesignerID = (((int)bytes[2] >> 4) & 0x0f)
    + (((int)bytes[1] << 4) & 0xff0);
            // GS1 EPC Tag data standard: page 119, 16.3.2
            // 3) Consider bits b8...b19 as an 12 bit unsigned integer. This is the Tag Mask Designer ID (MDID).

            result.TagModelNumber = (((int)bytes[2] << 8) & 0xf00)
                + ((int)bytes[3] & 0xff);

            // 从此开始是 Extended 部分特有结构
            if (result.XtidIndicator == true)
            {
                int v = ((int)bytes[4] >> 5) & 0x07;

                if (v == 0)
                    throw new ArgumentException("byte offset 4(从零开始计算) 之前 3 bit 值为零，不是合法的 Serial Number Segment 内容");

                int l = 48 + 16 * (v - 1);

                List<byte> numbers = new List<byte>();
                // 从 6 开始
                for (int i = 0; i < l / 8; i++)
                {
                    byte b = bytes[6 + i];
                    numbers.Add(b);
                }

                result.SerialNumber = numbers.ToArray();
                // urn:epc:stid:xFFF.x040.x123456789ABC
                result.StidUri = $"urn:epc:stid:x{Hex(result.MaskDesignerID, 3)}.x{Hex(result.TagModelNumber, 3)}.x{Hex(result.SerialNumber)}";
            }
            return result;
        }

        static string Hex(int v, int length)
        {
            return Convert.ToString(v, 16).ToUpper().PadLeft(length, '0');
        }

        static string Hex(byte[] bytes)
        {
            return ByteArray.GetHexTimeStampString(bytes).ToUpper();
        }
    }

}
