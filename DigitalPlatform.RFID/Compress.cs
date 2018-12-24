using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    public class Compress
    {

        public static CompactionScheme AutoSelectCompressMethod(string text)
        {
            if (CheckInteger(text, false))
                return CompactionScheme.Integer;
            if (CheckNumeric(text, false))
                return CompactionScheme.Numeric;
            if (CheckBit5(text, false))
                return CompactionScheme.FivebitCode;
            if (CheckBit6(text, false))
                return CompactionScheme.SixBitCode;
            if (CheckBit7(text, false))
                return CompactionScheme.SevenBitCode;
            return CompactionScheme.Null;
        }

        #region Integer

        public const UInt64 MaxInteger = 9999999999999999999;   // 19 位

        // 整型数
        public static byte[] IntegerCompress(string text)
        {
            // 检查
            CheckInteger(text);

#if NO
            List<byte> results = new List<byte>();
            BigInteger bigInteger = BigInteger.Parse(text);
            for (; ; )
            {
                BigInteger current = bigInteger % 256;
                results.Insert(0, (byte)current);
                bigInteger = bigInteger / 256;
                if (bigInteger == 0)
                    break;
            }

            return results.ToArray();
#endif

            UInt64 v = Convert.ToUInt64(text);

            return TrimLeft(ReverseBytes(BitConverter.GetBytes(v)));
        }

        // 反转 bytes
        public static byte[] ReverseBytes(byte[] bytes)
        {
            List<byte> results = new List<byte>(bytes);
            results.Reverse();
            return results.ToArray();
        }

        // 去掉左边的连续全 0 的 byte
        public static byte[] TrimLeft(byte[] source)
        {
            bool flag = true;
            List<byte> results = new List<byte>();
            foreach (byte b in source)
            {
                if (b == 0 && flag)
                    continue;
                results.Add(b);
                flag = false;
            }

            return results.ToArray();
        }


        public static string IntegerExtract(byte[] data)
        {
            BigInteger v = 0;
            foreach (byte b in data)
            {
                v = v << 8;
                v += b;
            }

            return v.ToString();
            // return BitConverter.ToInt64(data, 0).ToString();
        }

        // 检查字符串是否符合整型压缩的要求
        static bool CheckInteger(string text, bool throwException = true)
        {
            // 首字符不能为 '0'
            if (text[0] == '0')
            {
                if (throwException)
                    throw new ArgumentException("整型数压缩内容首字符不允许为 '0'");
                return false;
            }

            if (text.Length > 19)
            {
                if (throwException)
                    throw new ArgumentException("整型数压缩内容长度不应超过 19 字符");
                return false;
            }

            if (text.Length < 2)
            {
                if (throwException)
                    throw new ArgumentException("整型数压缩内容长度不应少于 2 字符");
                return false;
            }

            foreach (char ch in text)
            {
                if (ch < '0' || ch > '9')
                {
                    if (throwException)
                        throw new ArgumentException($"用于整型数压缩的内容中出现了非数字字符 '{ch}'");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region numeric

        // 数字
        public static byte[] NumericCompress(string text)
        {
            // 检查
            CheckNumeric(text);

            List<byte> bytes = new List<byte>();
            byte temp = 0;
            int i = 0;
            foreach (char ch in text)
            {
                var v = ch - '0';   // Convert.ToInt16(ch);
                if ((i % 2) == 0)
                {
                    temp = (byte)((v << 4) & 0xf0);
                    // 最后一次。单元测试注意此种边界情况
                    if (i >= text.Length - 1)
                    {
                        temp += (byte)0xf;
                        bytes.Add(temp);
                    }
                }
                else
                {
                    temp += (byte)v;
                    bytes.Add(temp);
                }
                i++;
            }

            return bytes.ToArray();
        }

        public static string NumericExtract(byte[] data)
        {
            // TODO: 检查补齐字符是否连续，是否从右端开始
            StringBuilder result = new StringBuilder();
            foreach (byte b in data)
            {
                short left = (short)((b >> 4) & 0xf);
                if (left == 0xf)    // 遇到补齐的字符
                    break;
                result.Append(left).ToString();
                short right = (short)(b & 0xf);
                if (right == 0xf)
                    break;
                result.Append(right.ToString());
            }
            return result.ToString();
        }

        // 检查字符串是否符合数字压缩的要求
        static bool CheckNumeric(string text, bool throwException = true)
        {
            foreach (char ch in text)
            {
                if (ch < '0' || ch > '9')
                {
                    if (throwException)
                        throw new ArgumentException($"用于数字压缩的内容中出现了非法字符 '{ch}'");
                    return false;
                }
            }

            if (text.Length < 2)
            {
                if (throwException)
                    throw new ArgumentException("数字压缩内容长度不应少于 2 字符");
                return false;
            }

            return true;
        }

        #endregion

        #region bit5

        // 整型数
        public static byte[] Bit5Compress(string text)
        {
            // 检查
            CheckBit5(text);

            BitPackage package = new BitPackage();
            foreach (char ch in text)
            {
#if DEBUG
                int prefix = (ch >> 5) & 0x7;
                Debug.Assert(prefix == 2);
#endif
                int start = ch << 3;
                for (int i = 0; i < 5; i++)
                {
                    package.Add((start & 0x80) == 0x80 ? true : false);
                    start = start << 1;
                }
            }

            package.Flush();
            return package.Bytes;
        }

#if NO
        // 整型数
        public static byte[] Bit5Compress(string text)
        {
            // 检查
            CheckBit5(text);

            List<bool> bit_array = new List<bool>();
            foreach (char ch in text)
            {
#if DEBUG
                int prefix = (ch >> 5) & 0x7;
                Debug.Assert(prefix == 2);
#endif
                int start = ch << 3;
                for (int i = 0; i < 5; i++)
                {
                    bit_array.Add((start & 0x80) == 0x80 ? true : false);
                    start = start << 1;
                }
            }

            List<byte> results = new List<byte>();

            byte b = 0;
            byte flag = 0x80;
            for (int i = 0; i < bit_array.Count; i++)
            {
                if (bit_array[i] == true)
                    b |= flag;
                flag = (byte)(flag >> 1);
                if (flag == 1)
                {
                    flag = 0x80;
                    results.Add(b);
                }
                else
                { 
                    // 最后一个元素
                    if (i >= bit_array.Count - 1)
                        results.Add(b);
                }
            }

            return results.ToArray();
        }
#endif
        static bool CheckBit5(string text, bool throwException = true)
        {
            if (text.Length < 3)
            {
                if (throwException)
                    throw new ArgumentException("5 bit 压缩内容长度不应少于 3 字符");
                return false;
            }

            // A-_ 之间是合法字符
            foreach (char ch in text)
            {
                if (ch < 0x41 || ch > 0x5f)
                {
                    if (throwException)
                        throw new ArgumentException($"用于 5 bit 压缩的内容字符出现了 0x41 和 0x5f 之外的字符 '{ch}'");
                    return false;
                }
            }

            return true;
        }

        public static string Bit5Extract(byte[] data)
        {
            BitExtract extract = new BitExtract(data);
            StringBuilder result = new StringBuilder();
            while (true)
            {
                List<bool> bits = extract.GetBits(5);
                if (bits.Count < 5)
                    break;
                if (BitExtract.IsZero(bits))
                    break;
                char ch = (char)(BitExtract.ToByte(bits, 5) | 0x40);
                result.Append(ch);
            }
            return result.ToString();
        }


        // 将 bit 聚集为 byte
        class BitPackage
        {
            // 掩码
            byte _mask = 0x80;

            // 当前 1 bit 位于 _current 的偏移
            // 0 表示在最左端; 7 表示在最右端
            int _index = 0;

            public int Index
            {
                get
                {
                    return _index;
                }
            }

            // 当前正在加工中的 byte
            byte _current = 0;

            public byte CurrentByte
            {
                get
                {
                    return _current;
                }
                set
                {
                    _current = value;
                }
            }

            List<byte> _bytes = new List<byte>();

            public byte[] Bytes
            {
                get
                {
                    return _bytes.ToArray();
                }
            }

            public void Clear()
            {
                _bytes = new List<byte>();
                _mask = 0x80;
                _current = 0;
            }

            public void AddRange(List<bool> bits)
            {
                foreach (bool bit in bits)
                {
                    Add(bit);
                }
            }

            public void AddRange(string bits)
            {
                foreach (char bit in bits)
                {
                    Add(bit == '0' ? false : true);
                }
            }

            // 加入一个 bit
            public void Add(bool bit)
            {
                if (bit)
                    _current |= _mask;

                _mask >>= 1;
                _index++;
                if (_mask == 0)
                {
                    _bytes.Add(_current);
                    _mask = 0x80;
                    _current = 0;
                    _index = 0;
                }
            }

            // 将当前 byte 加入结果数组
            public void Flush()
            {
                if (_mask != 0x80)
                {
                    _bytes.Add(_current);

                    _mask = 0x80;
                    _current = 0;
                    _index = 0;
                }
            }
        }

        // 从 byte 流中提取指定数目的 bit 构成 byte
        class BitExtract
        {
            // 上次提取剩余的 bits
            List<bool> _cache = new List<bool>();

            // 剩余的 bytes
            List<byte> _bytes = new List<byte>();

            public BitExtract(byte[] bytes)
            {
                _bytes = new List<byte>(bytes);
            }

            // 从头提取出 count 个 bit
            public List<bool> GetBits(int bits)
            {
                // 从 bytes 中提取 8 个 bit
                if (_cache.Count < bits)
                {
                    int delta = bits - _cache.Count;
                    int bytes = delta / 8;
                    if ((delta % 8) > 0)
                        bytes++;
                    EnsureCache(bytes);
                }

                List<bool> results = new List<bool>();
                for (int i = 0; i < bits; i++)
                {
                    if (_cache.Count == 0)
                        break;  // cache 中不够那么多
                    results.Add(_cache[0]);
                    _cache.RemoveAt(0);
                }

                return results;
            }

            public string GetBitsString(int bits)
            {
                List<bool> result = GetBits(bits);
                StringBuilder text = new StringBuilder();
                foreach (bool b in result)
                {
                    text.Append(b ? "1" : "0");
                }

                return text.ToString();
            }

            // 当前 bits 个数
            public int Count
            {
                get
                {
                    return _cache.Count + (_bytes.Count * 8);
                }
            }

            // 给 cache 中补充 bytes 个字节 (8 * bytes 个 bit)
            void EnsureCache(int bytes)
            {
                int need = bytes;
                while (need > 0)
                {
                    if (_bytes.Count == 0)
                        return; // 再也没有了
                    int start = _bytes[0];
                    _bytes.RemoveAt(0);
                    for (int i = 0; i < 8; i++)
                    {
                        _cache.Add((start & 0x80) == 0x80 ? true : false);
                        start = start << 1;
                    }
                    need--;
                }
            }

            // 把 bits 转换为 byte
            // parameters:
            //      count   最多多少个 bit
            public static byte ToByte(List<bool> bits, int count)
            {
                byte result = 0;
                int i = 0;
                foreach (bool b in bits)
                {
                    result <<= 1;
                    if (i >= count)
                        break;
                    if (b)
                        result |= 0x01;
                    i++;
                }

                return result;
            }

            public static List<bool> ToList(string bits)
            {
                List<bool> results = new List<bool>();

                foreach (char c in bits)
                {
                    results.Add(c == '1' ? true : false);
                }
                return results;
            }

            public static byte ToByte(string bits, int count)
            {
                return ToByte(ToList(bits), count);
            }

            // 是否全为 0
            public static bool IsZero(List<bool> bits)
            {
                foreach (bool b in bits)
                {
                    if (b == true)
                        return false;
                }

                return true;
            }
        }

        #endregion

        #region bit6

        // 整型数
        public static byte[] Bit6Compress(string text)
        {
            // 检查
            CheckBit6(text);

            BitPackage package = new BitPackage();
            foreach (char ch in text)
            {
#if DEBUG
                int prefix = (ch >> 6) & 0x3;
                Debug.Assert(prefix == 0x0 || prefix == 0x1);
#endif
                int start = ch << 2;
                for (int i = 0; i < 6; i++)
                {
                    package.Add((start & 0x80) == 0x80 ? true : false);
                    start = start << 1;
                }
            }

            // 尾部 bits 补齐 BIN 100000
            if (package.Index != 0)
                package.Add(true);

            package.Flush();
            return package.Bytes;
        }

        static bool CheckBit6(string text, bool throwException = true)
        {
            if (text.Length < 4)
            {
                if (throwException)
                    throw new ArgumentException("用于 6 bit 压缩的内容长度不应少于 4 字符");
                return false;
            }

            // 空-_ 之间为合法字符
            foreach (char ch in text)
            {
                if (ch < 0x20 || ch > 0x5f)
                {
                    if (throwException)
                        throw new ArgumentException($"用于 6 bit 压缩的内容字符出现了在 0x20 和 0x5f 之外的字符 '{ch}'");
                    return false;
                }
            }

            // 尾部不能有一个或者多个 x20 字符
            int count = 0;
            for (int i = text.Length - 1; i >= 0; i--)
            {
                char ch = text[i];
                if (ch == 0x20)
                    count++;
                else
                    break;
            }
            if (count > 0)
            {
                if (throwException)
                    throw new ArgumentException("用于 6 bit 压缩的内容字符的尾部不应出现 0x20 字符");
                return false;
            }

            return true;
        }

        public static string Bit6Extract(byte[] data)
        {
            BitExtract extract = new BitExtract(data);
            StringBuilder result = new StringBuilder();
            while (true)
            {
                List<bool> bits = extract.GetBits(6);
                if (bits.Count < 6)
                    break;

                char ch = (char)BitExtract.ToByte(bits, 6);

                // 如果正好末尾是 6 bits 100000，需丢弃
                // 注意测试这种情形, 应正确丢弃
                if (extract.Count == 0 && ch == 0x20)
                    break;

                if ((ch & 0x20) == 0x00)
                    ch = (char)(ch | 0x40);
                result.Append(ch);
            }
            return result.ToString();
        }

        #endregion

        #region bit7

        // 整型数
        public static byte[] Bit7Compress(string text)
        {
            // 检查
            CheckBit7(text);

            BitPackage package = new BitPackage();
            foreach (char ch in text)
            {
#if DEBUG
                int prefix = (ch >> 7) & 0x1;
                Debug.Assert(prefix == 0x0);
#endif
                int start = ch << 1;
                for (int i = 0; i < 7; i++)
                {
                    package.Add((start & 0x80) == 0x80 ? true : false);
                    start = start << 1;
                }
            }

            // 尾部 bits 补齐 BIN 1111111
            if (package.Index != 0)
            {
                int count = 8 - package.Index;
                for (int i = 0; i < count; i++)
                {
                    package.Add(true);
                }
            }

            package.Flush();
            return package.Bytes;
        }

        static bool CheckBit7(string text, bool throwException = true)
        {
            if (text.Length < 8)
            {
                if (throwException)
                    throw new ArgumentException("用于 7 bit 压缩的内容长度不应少于 8 字符");
                return false;
            }

            foreach (char ch in text)
            {
                if (ch > 0x7e)
                {
                    if (throwException)
                        throw new ArgumentException("用于 7 bit 压缩的内容字符应该在 x00 和 x7e 之间");
                    return false;
                }
            }

            return true;
        }

        public static string Bit7Extract(byte[] data)
        {
            BitExtract extract = new BitExtract(data);
            StringBuilder result = new StringBuilder();
            while (true)
            {
                List<bool> bits = extract.GetBits(7);
                if (bits.Count < 7)
                    break;

                char ch = (char)BitExtract.ToByte(bits, 7);

                // 正好末尾是 7 bits 1111111，需要丢弃
                // TODO: 注意测试这种情况
                if (extract.Count == 0 && ch == 0x7f)
                    break;

                result.Append(ch);
            }

            return result.ToString();
        }

        #endregion

        #region ISIL

#if NO

        public static byte[] IsilCompress(string text)
        {
            // 检查
            CheckIsil(text);

            BitPackage package = new BitPackage();
            char current_charset = 'u'; // u(大写) l(小写) d(数字)
            for (int index = 0; index < text.Length; index++)
            {
                char ch = text[index];

                if (ch == '-')
                {
                    if (current_charset == 'u' || current_charset == 'l')
                        package.AddRange("00000");
                    else
                    {
                        Debug.Assert(current_charset == 'd');
                        package.AddRange("1010");
                    }
                    continue;
                }

                if (ch == ':')
                {
                    if (current_charset == 'u')
                        package.AddRange("11011");
                    else if (current_charset == 'd')
                        package.AddRange("1011");
                    else
                    {
                        Debug.Assert(current_charset == 'l');
                        // TODO: 切换到 upper 或者 digit。最好预测一下，下一个字符，以便切换最优化
                        package.AddRange(GetSwitchBits(current_charset, 'u'));
                        current_charset = 'u';
                        package.AddRange("11011");
                    }
                    continue;
                }

                if (ch == '/')
                {
                    if (current_charset == 'l')
                        package.AddRange("11011");
                    else
                    {
                        Debug.Assert(current_charset == 'd' || current_charset == 'u');
                        // 切换到 lower
                        package.AddRange(GetSwitchBits(current_charset, 'l'));
                        current_charset = 'l';
                        package.AddRange("11011");
                    }
                    continue;
                }

                if (ch >= 'A' && ch <= 'Z')
                {
                    if (current_charset != 'u')
                    {
                        package.AddRange(GetSwitchBits(current_charset, 'u'));
                        current_charset = 'u';
                    }
                    AddToPackage(ch, current_charset, package);
                    continue;
                }

                if (ch >= 'a' && ch <= 'z')
                {
                    if (current_charset != 'l')
                    {
                        package.AddRange(GetSwitchBits(current_charset, 'l'));
                        current_charset = 'l';
                    }
                    AddToPackage(ch, current_charset, package);
                    continue;
                }

                if (ch >= '0' && ch <= '9')
                {
                    if (current_charset != 'd')
                    {
                        package.AddRange(GetSwitchBits(current_charset, 'd'));
                        current_charset = 'd';
                    }
                    AddToPackage(ch, current_charset, package);
                    continue;
                }

                throw new Exception($"出现了超出范围的字符 '{ch}'");
            }

            // 尾部 bits 补齐 BIN 1111111
            if (package.Index != 0)
            {
                int count = 8 - package.Index;
                for (int i = 0; i < count; i++)
                {
                    package.Add(true);
                }
            }

            package.Flush();
            return package.Bytes;
        }

#endif

        public static byte[] IsilCompress(string text,
            StringBuilder debugInfo = null)
        {
            // 检查
            CheckIsil(text);

            BitPackage package = new BitPackage();
            char prev_charset = 'u'; // u(大写) l(小写) d(数字)
            for (int index = 0; index < text.Length; index++)
            {
                char ch = text[index];

                if (debugInfo != null)
                    debugInfo.Append($"{ch}");

                char next_ch = (char)0;
                if (index + 1 <= text.Length - 1)
                    next_ch = text[index + 1];

                // 获得当前字符可能处于的字符集
                string current_charsets = GetCharsets(ch);

                string action = "";
                string targets = "";
                // 如果当前字符的字符集和 prev_charset 不同
                // 就必须 switch 或者 shift
                if (current_charsets.IndexOf(prev_charset) == -1)
                {
                    // 1) 如果 next_ch 在 prev_charset 以内，则用 shift
                    if (next_ch == 0
                        || GetCharsets(next_ch).IndexOf(prev_charset) != -1)
                    {
                        action = "shift";
                        targets = current_charsets[0].ToString();
                    }
                    else
                    {
                        action = "switch";
                        targets = GetCharsets(ch);
                    }

                    if (debugInfo != null)
                        debugInfo.Append($" {action}:{prev_charset}-{targets}");

                    string output = "";
                    if (action == "switch")
                    {
                        output = GetSwitchBits(prev_charset, targets[0]);
                        package.AddRange(output);
                        prev_charset = targets[0];
                        output += "," + AddToPackage(ch, prev_charset, package);
                    }
                    else
                    {
                        Debug.Assert(action == "shift");
                        output = GetShiftBits(prev_charset, targets[0]);
                        package.AddRange(output);
                        output += "," + AddToPackage(ch, targets[0], package);
                    }

                    if (debugInfo != null)
                        debugInfo.Append($" output:{output}\r\n");
                }
                else
                {

                    // 直接输出字符
                    string output = AddToPackage(ch, prev_charset, package);
                    if (debugInfo != null)
                        debugInfo.Append($" output:{output}\r\n");
                }
            }

            // 尾部 bits 补齐 BIN 1111111
            if (package.Index != 0)
            {
                int count = 8 - package.Index;
                for (int i = 0; i < count; i++)
                {
                    package.Add(true);
                }
            }

            package.Flush();
            return package.Bytes;
        }

        // 获得一个字符可能处于的字符集。可能不止一个
        static string GetCharsets(char ch)
        {
            if (ch == '-')
                return "uld";
            if (ch == ':')
                return "ud";
            if (ch == '/')
                return "l";
            if (ch >= 'a' && ch <= 'z')
                return "l";
            if (ch >= 'A' && ch <= 'Z')
                return "u";
            if (ch >= '0' && ch <= '9')
                return "d";
            throw new Exception($"出现非法字符 '{ch}'");
        }

        // 检测两个字符是否可以处于同一个字符集
        static bool InSameCharset(char ch1, char ch2, out string charsets)
        {
            charsets = "";
            string charsets1 = GetCharsets(ch1);
            string charsets2 = GetCharsets(ch2);
            foreach (char charset1 in charsets1)
            {
                foreach (char charset2 in charsets2)
                {
                    if (charset1 == charset2
                        && charsets.IndexOf(charset1) == -1)
                        charsets += charset1.ToString();
                }
            }

            if (string.IsNullOrEmpty(charsets) == false)
                return true;

            return false;
        }

        // 获得 switch bits
        static string GetSwitchBits(char prev_charset, char next_charset)
        {
            if (prev_charset == next_charset)
                throw new ArgumentException("prev_charset 和 next_charset 不应该相同");

            if (prev_charset == 'u')
            {
                // upper --> lower
                if (next_charset == 'l')
                    return "11100";
                // upper --> digit
                if (next_charset == 'd')
                    return "11110";
            }

            if (prev_charset == 'l')
            {
                // lower --> upper
                if (next_charset == 'u')
                    return "11100";
                // lower --> digit
                if (next_charset == 'd')
                    return "11110";
            }

            if (prev_charset == 'd')
            {
                // digit --> upper
                if (next_charset == 'u')
                    return "1100";
                // digit --> lower
                if (next_charset == 'l')
                    return "1110";
            }

            throw new Exception($"出现了不可能的组合 {prev_charset} --> {next_charset}");
        }

        // 获得 shift bits
        static string GetShiftBits(char prev_charset, char next_charset)
        {
            if (prev_charset == next_charset)
                throw new ArgumentException("prev_charset 和 next_charset 不应该相同");

            if (prev_charset == 'u')
            {
                // upper --> lower
                if (next_charset == 'l')
                    return "11101";
                // upper --> digit
                if (next_charset == 'd')
                    return "11111";
            }

            if (prev_charset == 'l')
            {
                // lower --> upper
                if (next_charset == 'u')
                    return "11101";
                // lower --> digit
                if (next_charset == 'd')
                    return "11111";
            }

            if (prev_charset == 'd')
            {
                // digit --> upper
                if (next_charset == 'u')
                    return "1101";
                // digit --> lower
                if (next_charset == 'l')
                    return "1111";
            }

            throw new Exception($"出现了不可能的组合 {prev_charset} --> {next_charset}");
        }

        static string AddToPackage(char ch, char charset, BitPackage package)
        {
            string bits = "";
            if (charset == 'u')
                bits = GetUpperSetBits(ch);
            else if (charset == 'l')
                bits = GetLowerSetBits(ch);
            else if (charset == 'd')
                bits = GetDigiSetBits(ch);
            else
                throw new ArgumentException($"无法识别的 charset '{charset}'");
            package.AddRange(bits);
            return bits;
        }

        static string GetUpperSetBits(char ch)
        {
            if (ch == '-')
                return "00000";
            if (ch == ':')
                return "11011";
            if (ch > 'Z' || ch < 'A')
                throw new ArgumentException($"大写字符集中出现非法字符 '{ch}'");
            int value = ch - 'A' + 1;
            return Convert.ToString(value, 2).PadLeft(5, '0');
        }

        static string GetLowerSetBits(char ch)
        {
            if (ch == '-')
                return "00000";
            if (ch == '/')
                return "11011";
            if (ch > 'z' || ch < 'a')
                throw new ArgumentException($"小写字符集中出现非法字符 '{ch}'");
            int value = ch - 'a' + 1;
            return Convert.ToString(value, 2).PadLeft(5, '0');
        }

        static string GetDigiSetBits(char ch)
        {
            if (ch == '-')
                return "1010";
            if (ch == ':')
                return "1011";
            if (ch > '9' || ch < '0')
                throw new ArgumentException($"数字字符集中出现非法字符 '{ch}'");
            int value = ch - '0';
            return Convert.ToString(value, 2).PadLeft(4, '0');
        }

        static List<bool> MakeBits(string value)
        {
            List<bool> results = new List<bool>();
            foreach (char ch in value)
            {
                results.Add(ch == '0' ? false : true);
            }
            return results;
        }


        // TODO: 检查下一个字符是否在连续的平面中

        static string[] valid_isil_ranges = new string[] {
            "-",
            "AZ",
            ":",
            "az",
            "/",
            "09",
        };

        // 是否为合法的 ISIL 字符
        static bool IsValidIsilChar(char ch)
        {
            foreach (string range in valid_isil_ranges)
            {
                if (range.Length == 1)
                {
                    if (ch == range[0])
                        return true;
                }

                if (range.Length == 2)
                {
                    if (ch >= range[0] && ch <= range[1])
                        return true;
                }
            }

            return false;
        }

        static bool CheckIsil(string text, bool throwException = true)
        {

            foreach (char ch in text)
            {
                if (IsValidIsilChar(ch) == false)
                {
                    if (throwException)
                        throw new ArgumentException($"ISIL 内容中出现了不合法的字符 '{ch.ToString()}'");
                    return false;
                }
            }

            return true;
        }

        public static string IsilExtract(byte[] data)
        {
            char save_charset = 'u';
            char current_charset = 'u';
            BitExtract extract = new BitExtract(data);
            StringBuilder result = new StringBuilder();
            while (true)
            {
                int bit_count = char.ToLower(current_charset) == 'd' ? 4 : 5;
                string bits = extract.GetBitsString(bit_count);
                if (bits.Length < bit_count)
                    break;

                // 切换命令
                if (current_charset == 'u' || current_charset == 'U')
                {
                    // 小写锁定
                    if (bits == "11100")
                    {
                        current_charset = 'l';
                        continue;
                    }

                    // 小写 shift
                    if (bits == "11101")
                    {
                        save_charset = current_charset;
                        current_charset = 'L';
                        continue;
                    }

                    // 数字锁定
                    if (bits == "11110")
                    {
                        current_charset = 'd';
                        continue;
                    }

                    // 数字 shift
                    if (bits == "11111")
                    {
                        save_charset = current_charset;
                        current_charset = 'D';
                        continue;
                    }
                }

                // 切换命令
                if (current_charset == 'l' || current_charset == 'L')
                {
                    // 大写锁定
                    if (bits == "11100")
                    {
                        current_charset = 'u';
                        continue;
                    }

                    // 大写 shift
                    if (bits == "11101")
                    {
                        save_charset = current_charset;
                        current_charset = 'U';
                        continue;
                    }

                    // 数字锁定
                    if (bits == "11110")
                    {
                        current_charset = 'd';
                        continue;
                    }

                    // 数字 shift
                    if (bits == "11111")
                    {
                        save_charset = current_charset;
                        current_charset = 'D';
                        continue;
                    }
                }

                // 切换命令
                if (current_charset == 'd' || current_charset == 'D')
                {
                    // 大写锁定
                    if (bits == "1100")
                    {
                        current_charset = 'u';
                        continue;
                    }

                    // 大写 shift
                    if (bits == "1101")
                    {
                        save_charset = current_charset;
                        current_charset = 'U';
                        continue;
                    }

                    // 小写锁定
                    if (bits == "1110")
                    {
                        current_charset = 'l';
                        continue;
                    }

                    // 小写 shift
                    if (bits == "1111")
                    {
                        save_charset = current_charset;
                        current_charset = 'L';
                        continue;
                    }
                }

                char ch = (char)BitExtract.ToByte(bits, bit_count);

                if (current_charset == 'u' || current_charset == 'U')
                {
                    if (bits == "00000")
                        result.Append('-');
                    else if (bits == "11011")
                        result.Append(':');
                    else if (ch >= 0x01 && ch <= 0x1a)
                    {
                        result.Append((char)(ch - 1 + 'A'));
                    }
                    else
                        throw new Exception("error 1");

                    if (current_charset == 'U')
                        current_charset = save_charset;
                    continue;
                }

                if (current_charset == 'l' || current_charset == 'L')
                {
                    if (bits == "00000")
                        result.Append('-');
                    else if (bits == "11011")
                        result.Append('/');
                    else if (ch >= 0x01 && ch <= 0x1a)
                    {
                        result.Append((char)(ch - 1 + 'a'));
                    }
                    else
                        throw new Exception("error 2");

                    if (current_charset == 'L')
                        current_charset = save_charset;
                    continue;
                }

                if (current_charset == 'd' || current_charset == 'D')
                {
                    if (bits == "1010")
                        result.Append('-');
                    else if (bits == "1011")
                        result.Append(':');
                    else if (ch >= 0 && ch <= 0x09)
                    {
                        result.Append((char)('0' + ch));
                        continue;
                    }
                    else
                        throw new Exception("error 3");

                    if (current_charset == 'D')
                        current_charset = save_charset;
                    continue;
                }

                break;
            }

            return result.ToString();
        }

        #endregion
    }


}
