using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    // ISO/TS 28560-4:2014(E), page 11
    public enum ElementOID
    {
        // Unique item identifier
        UII = 0,
        UniqueItemIdentifier = 0,

        // Primary item identifier
        PII = 1,
        PrimaryItemIdentifier = 1,

        // Content parameter
        CP = 2,
        ContentParameter = 2,

        // Owner institution (ISIL)
        OI = 3,
        OwnerInstitution = 3,

        // Set infomation
        SI = 4,
        SetInformation = 4,

        // Type of usage
        TU = 5,
        TypeOfUsage = 5,

        // Shelf location
        SL = 6,
        ShelfLocation = 6,

        // ONIX media format
        OMF = 7,
        OnixMediaFormat = 7,

        // MARC media format
        MMF = 8,
        MarcMediaFormat = 8,

        // Supplier identifier
        SID = 9,
        SupplierIdentifier = 9,

        // Order number
        ON = 10,
        OrderNumber = 10,

        // ILL borrowing institution (ISIL)
        IBI = 11,
        IllBorrowingInstitution = 11,

        // ILL borrowing transaction number
        ITN = 12,
        IllBorrowingTransactionNumber = 12,

        // GS1 product identifier
        GPI = 13,
        Gs1ProductIndentifier = 13,

        // Alternative unique item identifier
        AUII = 14,
        AlternativeUniqueItemIdentifier = 14,

        // Local data A
        LDA = 15,
        LocalDataA = 15,

        // Local data B
        LDB = 16,
        LocalDataB = 16,

        // Title
        T = 17,
        Title = 17,

        // Product identifier local
        PIL = 18,
        ProductIdentifierLocal = 18,

        // Media format (other)
        MF = 19,
        MediaFormat = 19,

        // Supply chain stage
        SCS = 20,
        SupplyChainStage = 20,

        // Supplier invoice number
        SIN = 21,
        SupplierInvoiceNumber = 21,

        // Alternative item identifier
        AII = 22,
        AlternativeItemIdentifier = 22,

        // Alternative owner institution
        AOI = 23,
        AlternativeOwnerInstitution = 23,

        // Subsidiary of an owner institution
        SOI = 24,
        SubsidiaryOfAnOwnerInstitution = 24,

        // Alternative ILL borrowing institution
        AIBI = 25,
        AlternativeIllBorrowingInstitution = 25,

        // Local data C
        LDC = 26,
        LocalDataC = 26,

        // Not defined 1
        ND1 = 27,
        NotDefined1 = 27,

        // Not defined 2
        ND2 = 28,
        NotDefined2 = 28,

        // Not defined 3
        ND3 = 29,
        NotDefined3 = 29,

        // Not defined 4
        ND4 = 30,
        NotDefined4 = 30,

        // Not defined 5
        ND5 = 31,
        NotDefined5 = 31,

    }

    // 一个数据元素
    public class Element
    {
        // 本元素在整个芯片内存中的起点 byte 位置
        int _startOffs = -1;    // -1 表示尚未初始化
        public int StartOffs
        {
            get
            {
                return _startOffs;
            }
        }

        // *** 原始数据
        public byte[] OriginData { get; set; }

        // *** 根据原始数据解析出来的信息
        internal Precursor _precursor = null;
        internal int _paddings = 0;
        internal int _lengthOfData = 0;
        internal byte[] _compactedData = null;

        // 末尾填充的 byte 数
        public int Paddings
        {
            get
            {
                return _paddings;
            }
        }

        // Precursor 中 Offset bit 是否置位
        // true 表示 Offset bit 为 1
        public bool PrecursorOffset
        {
            get
            {
                if (_precursor == null)
                    return false;
                return _precursor.Offset;
            }
        }

        public string Name
        {
            get
            {
                ElementOID oid = (ElementOID)this.OID;
                return GetOidName(oid);
            }
        }

        ElementOID _oid = ElementOID.PII;
        public ElementOID OID
        {
            get
            {
                return _oid;
            }
            set
            {
                if (this.Locked)
                    throw new Exception("Locked 状态的 Element 不允许进行修改");
                _oid = value;
            }
        }

        string _text = "";
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (this.Locked)
                    throw new Exception("Locked 状态的 Element 不允许进行修改");
                _text = value;
            }
        }

        byte[] _content = null;
        public byte[] Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (this.Locked)
                    throw new Exception("Locked 状态的 Element 不允许进行修改");

                _content = value;
            }
        }

        // 对于已经存在于芯片内存上的元素，是否处于已锁定状态
        bool _locked = false;
        public bool Locked
        {
            get { return _locked; }
        }

        // 对于新创建的元素，是否打算在写入(芯片)时候锁定它
        bool _willLock = false;
        public bool WillLock
        {
            get
            {
                return _willLock;
            }
            set
            {
                _willLock = value;
            }
        }

        public Element(int start)
        {
            this._startOffs = start;
        }

        public Element Clone()
        {
            Element element = new Element(this.StartOffs);
            element.OriginData = new List<byte>(this.OriginData).ToArray();

            element._precursor = this._precursor;
            element._paddings = this._paddings;
            element._lengthOfData = this._lengthOfData;
            element._compactedData = this._compactedData;
            element._oid = this._oid;
            element._text = this._text;
            element._content = this._content;
            element._locked = this._locked;
            element._willLock = this._willLock;

            return element;
        }

        // 用于单元测试
        public void SetLocked(bool locked)
        {
            this._locked = locked;
        }

        // 得到一个 OID 类型的名字字符串
        public static string GetOidName(ElementOID oid)
        {
            return Enum.GetName(typeof(ElementOID), oid);
        }

        // 根据名字得到 OID 类型
        public static ElementOID GetOidByName(string name)
        {
            // 把 name 变为 OID
            if (Enum.TryParse<ElementOID>(name, out ElementOID oid) == false)
                throw new ArgumentException($"name '{name}' 不合法");
            return oid;
        }

        public static bool TryGetOidByName(string name, out ElementOID oid)
        {
            try
            {
                oid = GetOidByName(name);
                return true;
            }
            catch
            {
                oid = ElementOID.UII;
                return false;
            }
        }

        // 验证输入的元素文本是否合法
        // return:
        //      null    合法
        //      其他  返回不合法的文字解释
        public static string VerifyElementText(ElementOID oid, string text)
        {
            if (oid == ElementOID.PII)
                return VerifyPII(text);
            if (oid == ElementOID.OwnerInstitution)
                return VerifyOwnerInstitution(text);
            if (oid == ElementOID.SetInformation)
                return VerifySetInformation(text);
            if (oid == ElementOID.TypeOfUsage)
                return VerifyTypeOfUsage(text);
            if (oid == ElementOID.Gs1ProductIndentifier)
                return VerifyGs1ProductIndentifier(text);
            if (oid == ElementOID.MediaFormat)
                return VerifyMediaFormat(text);
            if (oid == ElementOID.SupplyChainStage)
                return VerifySupplyChainStage(text);
            if (oid == ElementOID.IllBorrowingInstitution)
                return VerifyIBI(text);
            return null;
        }

        #region 校验各种元素的输入文本合法性

        public static string VerifyPII(string text)
        {
            return null;
        }

        public static string VerifyOwnerInstitution(string text)
        {
            DigitalPlatform.RFID.Compact.CheckIsil(text);
            return null;
        }

        public static string VerifyIBI(string text)
        {
            DigitalPlatform.RFID.Compact.CheckIsil(text);
            return null;
        }

        // GB/T 35660.2-2017 page 9
        // 编码到芯片的时候，根据 text 自动选定编码方式。由于它形态的特点，一般会自动选定 Integer 编码方式
        // 可参见 GB/T 35660.2-2017 page 32 具体例子
        public static string VerifySetInformation(string text)
        {
            if (text.Length != 2 && text.Length != 4 && text.Length != 6)
                return "SetInformation 元素内容必须是 2 4 6 个数字字符";
            foreach (char ch in text)
            {
                if (ch < '0' || ch > '9')
                    return "SetInformation 元素内容不允许出现非数字字符";
            }
            return null;
        }

        // GB/T 35660.1-2017 page 21
        // 两个字符。分别为主次两个限定符。每个字符范围是 0-9 A-F。实际上意思是当作 0-16 的数值来理解
        // 编码到芯片的时候，要用 OctectString 方式编码为一个 byte
        public static string VerifyTypeOfUsage(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;    // 允许值为空
            if (text.Length != 2)
                return "TypeOfUsage(应用类别) 元素内容必须是 2 字符";
            foreach (char ch in text)
            {
                if ((ch >= '0' && ch <= '9')
                    || (ch >= 'A' && ch <= 'F'))
                {

                }
                else
                    return "TypeOfUsage(应用类别) 元素内容必须是 0-9 或 A-F 范围的字符";
            }
            return null;
        }

        public static string VerifyGs1ProductIndentifier(string text)
        {
            if (text.Length != 13)
                return "Gs1ProductIndentifier(GS1产品标识符) 元素内容必须是 13 字符";
            return null;
        }

        // 这里采用十六进制表示法
        // GB/T 35660.1-2017 page 10
        // 编码到芯片时候，是 OctectString 方式，编码为一个 byte
        public static string VerifyMediaFormat(string text)
        {
            if (text.Length != 2)
                return "MediaFormat(媒体格式(其他)) 元素内容必须是 2 字符";
            foreach (char ch in text)
            {
                if ((ch >= '0' && ch <= '9')
                    || (ch >= 'A' && ch <= 'F'))
                {

                }
                else
                    return "MediaFormat(媒体格式(其他)) 元素内容必须是 0-9 或 A-F 范围的字符";
            }
            return null;
        }

        // 这里采用十六进制表示法
        // GB/T 35660.1-2017 page 11
        // 编码到芯片时候，是 OctectString 方式，编码为一个 byte
        public static string VerifySupplyChainStage(string text)
        {
            if (text.Length != 2)
                return "SupplyChainStage(供应链阶段) 元素内容必须是 2 字符";
            foreach (char ch in text)
            {
                if ((ch >= '0' && ch <= '9')
                    || (ch >= 'A' && ch <= 'F'))
                {

                }
                else
                    return "SupplyChainStage(供应链阶段) 元素内容必须是 0-9 或 A-F 范围的字符";
            }
            return null;
        }

        #endregion

        // 根据 OID 和字符内容，构造一个 element 的原始数据
        // parameters:
        //      text    内容文字。如果是给 ISIL 类型的, 要明确用 compact_method 指明
        //      alignment   对齐 block 边界
        public static byte[] Compact(int oid,
            string text,
            CompactionScheme compact_method,
            bool alignment)
        {
            Precursor precursor = new Precursor();
            precursor.ObjectIdentifier = oid;

            // 注: SetInformation 的编码方式是根据字符串来自动选定的 (GB/T 35660.2-2017 page 32 例子)

            if (oid == (int)ElementOID.ContentParameter
                || oid == (int)ElementOID.TypeOfUsage
                || oid == (int)ElementOID.MediaFormat
                || oid == (int)ElementOID.SupplyChainStage)
                compact_method = CompactionScheme.OctectString;
            else if (oid == (int)ElementOID.OwnerInstitution
                || oid == (int)ElementOID.IllBorrowingInstitution)
                compact_method = CompactionScheme.ISIL;

            // 自动选定压缩方案
            if (compact_method == CompactionScheme.Null)
            {
                compact_method = RFID.Compact.AutoSelectCompactMethod((ElementOID)oid, text);
                if (compact_method == CompactionScheme.Null)
                    throw new Exception($"无法为字符串 '{text}' (oid='{(ElementOID)oid}') 自动选定压缩方案");
            }

            byte[] data = null;
            if (compact_method == CompactionScheme.Integer)
                data = RFID.Compact.IntegerCompact(text);
            else if (compact_method == CompactionScheme.Numeric)
                data = RFID.Compact.NumericCompact(text);
            else if (compact_method == CompactionScheme.FivebitCode)
                data = RFID.Compact.Bit5Compact(text);
            else if (compact_method == CompactionScheme.SixBitCode)
                data = RFID.Compact.Bit6Compact(text);
            else if (compact_method == CompactionScheme.Integer)
                data = RFID.Compact.IntegerCompact(text);
            else if (compact_method == CompactionScheme.SevenBitCode)
                data = RFID.Compact.Bit7Compact(text);
            else if (compact_method == CompactionScheme.OctectString)
                data = FromHexString(text);
            else if (compact_method == CompactionScheme.Utf8String)
                data = Encoding.UTF8.GetBytes(text);
            else if (compact_method == CompactionScheme.ISIL)
                data = RFID.Compact.IsilCompact(text);
            else if (compact_method == CompactionScheme.Base64)
                data = Convert.FromBase64String(text);

            if (oid == (int)ElementOID.ContentParameter)
                compact_method = 0;

            if (compact_method == CompactionScheme.ISIL)
                compact_method = (int)CompactionScheme.ApplicationDefined;

            precursor.CompactionCode = (int)compact_method;

            // 通过 data 计算出是否需要 padding bytes
            int total_bytes = 1 // precursor
                + 1 // length of data
                + data.Length;  // data
            if (precursor.ObjectIdentifier >= 15)
                total_bytes++;  // relative-OID 需要多占一个 byte

            int paddings = 0;
            if (alignment)
            {
                if ((total_bytes % 4) != 0)
                {
                    paddings = 4 - (total_bytes % 4);
                }
            }

            // 组装最终数据
            List<byte> result = new List<byte>();

            // OID 值是否越过 precursor 表达范围
            if (precursor.ObjectIdentifier >= 15)
                precursor.ObjectIdentifier = 0x0f;

            if (paddings > 0)
                precursor.Offset = true;
            result.Add(precursor.ToByte());

            if (precursor.ObjectIdentifier == 0x0f)
            {
                Debug.Assert(oid >= 15);
                result.Add((byte)(oid - 15));   // relative-OID
            }

            // padding length byte
            if (paddings > 0)
                result.Add((byte)(paddings - 1));

            result.Add((byte)data.Length);   // length of data

            result.AddRange(data);  // data

            for (int i = 0; i < paddings - 1; i++)
                result.Add((byte)0); //   padding bytes

            return result.ToArray();
        }

        // 根据 byte [] 建立一个 Element 对象
        // parameter:
        //      bytes   [out] 返回本次用掉的 byte 数
        public static Element Parse(byte[] data,
            int start,
            out int bytes)
        {
            bytes = 0;

            if (start >= data.Length)
                throw new ArgumentException($"start 值 {start} 不应越过 data length {data.Length}");

            int offset = start;

            Element element = new Element(start);

            if (data.Length - offset < 1)
                throw new Exception($"data 长度不足，从 {offset} 开始应至少为 1 bytes");

            element._precursor = new Precursor(data[offset]);

            offset++;

            // OID 为 1-14, 元素存储结构为 Precursor + Length of data + Compacted data
            if (element._precursor.ObjectIdentifier <= 14)
            {
                element.OID = (ElementOID)element._precursor.ObjectIdentifier;
            }
            else
            {
                // OID 为 15-127。元素存储结构为 Precursor + Relative-OID 15 to 127 + Length of data + Compacted data
                if (data.Length - offset < 3)
                    throw new Exception($"data 长度不足，从 {offset} 开始应至少为 3 bytes");

                element.OID = (ElementOID)(15 + data[offset]);
                offset++;
            }

            if (data.Length - offset < 2)
                throw new Exception($"data 长度不足，从 {offset} 开始应至少为 2 bytes");

            if (element._precursor.Offset == true)
            {
                // 填充字节数
                element._paddings = data[offset];
                offset++;
            }

            element._lengthOfData = data[offset];
            offset++;

            if (data.Length - offset < element._lengthOfData)
                throw new Exception($"data 长度不足，从 {offset} 开始应至少为 {element._lengthOfData} bytes");

            element._compactedData = new byte[element._lengthOfData];
            Array.Copy(data, offset, element._compactedData, 0, element._lengthOfData);

            bytes = offset - start + element._lengthOfData + element._paddings;

            element.OriginData = new byte[bytes];
            Array.Copy(data, start, element.OriginData, 0, bytes);

            element.Content = element._compactedData;

            // 解析出 Text
            if (element._precursor.CompactionCode == (int)CompactionScheme.ApplicationDefined)
            {
                if (element.OID == ElementOID.OwnerInstitution
                    || element.OID == ElementOID.IllBorrowingInstitution
                    )
                {
                    element.Text = RFID.Compact.IsilExtract(element._compactedData);
                }
                else
                {
                    // 只能用 Content 表示
                    // TODO: 此时 Text 里面放什么？是否要让 get Text 抛出异常引起注意?
                }
            }
            else
            {
                if (element._precursor.CompactionCode == (int)CompactionScheme.Integer)
                    element.Text = RFID.Compact.IntegerExtract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.Integer)
                    element.Text = RFID.Compact.IntegerExtract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.Numeric)
                    element.Text = RFID.Compact.NumericExtract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.FivebitCode)
                    element.Text = RFID.Compact.Bit5Extract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.SixBitCode)
                    element.Text = RFID.Compact.Bit6Extract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.SevenBitCode)
                    element.Text = RFID.Compact.Bit7Extract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.OctectString)
                {
                    // element.Text = Encoding.ASCII.GetString(element._compactedData); // GetHexString(element._compactedData);
                    if (element.OID == ElementOID.TypeOfUsage)
                        element.Text = GetHexString(element._compactedData);
                    else
                        element.Text = Encoding.ASCII.GetString(element._compactedData); // GetHexString(element._compactedData);
                }
                else if (element._precursor.CompactionCode == (int)CompactionScheme.Utf8String)
                    element.Text = Encoding.UTF8.GetString(element._compactedData);
                else
                    throw new Exception($"出现意料之外的 CompactScheme {element._precursor.CompactionCode}");
            }

            return element;
        }

        // 调整 padding bytes。
        // 如果 data 包含超过一个元素的内容，则第一个元素后面的内容操作后不会被损坏
        // 注：
        // 当 OID 为 1-14 时:
        // Precursor (+Padding length) + Length of data + Compacted data (+padding bytes)
        // 当 OID 为 15-127 时
        // Precursor (+Padding length) + Additioal OID value + Length of data + Compacted data (+padding bytes)
        // parameters:
        //      data    待加工的数据
        //      delta   变化数。可以是负数。表示增加或者减少这么多个 bytes 的 padding 字符
        // exception:
        //      可能会抛出 Exception 或 PaddingOverflowException
        public static byte[] AdjustPaddingBytes(byte[] data, int delta)
        {
            if (delta == 0)
                throw new ArgumentException("不允许以 delta 为 0 进行调用");

            Precursor precursor = new Precursor(data[0]);

            int padding_length_count = 0;   // “填充字节数”位，的字节个数。0 或 1
            int additional_oid_count = 0;   // 附加的 OID 字节个数。0 或 1
            int padding_count = 0;  // padding byte 个数。注意这个值没有包含 padding length byte 本身的 1
            int compacted_data_length = 0;
            int total_length = 0;

            if (precursor.Offset)
            {
                padding_length_count = 1;
                padding_count = data[1];
            }

            if (precursor.ObjectIdentifier >= 15)
                additional_oid_count = 1;

            compacted_data_length = data[1 + padding_length_count + additional_oid_count];

            total_length = 1 // (precursor) byte
                + padding_length_count // (padding length) byte
                + additional_oid_count // (additional oid value) byte
                + 1 // (length of data) byte
                + compacted_data_length
                + padding_count;

            if (data.Length < total_length)
                throw new Exception($"调用时给出的 data 长度为 {data.Length}, 不足 {total_length}。数据不完整");

            List<byte> result = new List<byte>();
            List<byte> more = new List<byte>();
            {
                int i = 0;
                // 确保没有多余的数据
                for (i = 0; i < total_length; i++)
                {
                    result.Add(data[i]);
                }

                // 多余的部分暂存起来
                for (; i < data.Length; i++)
                {
                    more.Add(data[i]);
                }
            }

            // 开始腾挪

            // *** padding 增多的情况
            if (delta > 0)
            {
                // 加工前已经有 padding 的情况
                if (precursor.Offset)
                {
                    // 在末尾增加 delta 个填充 byte
                    for (int i = 0; i < delta; i++)
                    {
                        result.Add(0);
                    }
                    // 检查 byte 值是否溢出
                    // 有办法预先知道多大的 delta 值会溢出
                    if (result[1] + delta > byte.MaxValue)
                        throw new PaddingOverflowException($"Padding Length 原值为 {result[1]}，加上 {delta} 以后发生了溢出",
                            byte.MaxValue - result[1]);
                    // 修改 Padding Length 位
                    result[1] = (byte)(result[1] + delta);
                    result.AddRange(more);
                    return result.ToArray();
                }

                // 加工前没有 padding 的情况
                // 1) 先插入一个 Padding length 位
                result.Insert(1, (byte)(delta - 1));
                if (delta - 1 > 0)
                {
                    for (int i = 0; i < delta - 1; i++)
                    {
                        result.Add(0);
                    }
                }
                // 2) Offset bit 设置为 1
                result[0] |= 0x80;

                result.AddRange(more);
                return result.ToArray();
            }

            // *** delta 减少的情况
            Debug.Assert(delta < 0);
            // 加工前已经有 padding 的情况
            if (precursor.Offset)
            {
                if (padding_count + delta < -1)
                    throw new Exception($"delta 值 {delta} 太小以至于超过可用范围");
                // 去掉尾部 padding 字节就可以满足的情况
                if (padding_count + delta >= 0)
                {
                    for (int i = 0; i < -delta; i++)
                    {
                        result.RemoveAt(result.Count - 1);
                    }
                }
                else
                {
                    Debug.Assert(padding_count + delta == -1);
                    for (int i = 0; i < -delta - 1; i++)
                    {
                        result.RemoveAt(result.Count - 1);
                    }
                    // 还要去掉 padding length 位
                    result.RemoveAt(1);
                    // Offset 变为 false
                    precursor.Offset = false;
                    result[0] = precursor.ToByte();
                }

                result.AddRange(more);
                return result.ToArray();
            }

            // 加工前没有 padding 的情况
            throw new PaddingOverflowException($"delta 值为 {delta} 但原始数据中并没有 padding 可以去除",
                0);
        }

        // 获得 element 用到的填充 byte 个数
        public static int GetPaddingCount(byte[] data)
        {
            Precursor precursor = new Precursor(data[0]);

            int padding_length_count = 0;   // “填充字节数”位，的字节个数。0 或 1
            int padding_count = 0;  // padding byte 个数。注意这个值没有包含 padding length byte 本身的 1

            if (precursor.Offset)
            {
                padding_length_count = 1;
                padding_count = data[1];
            }

            return padding_count + padding_length_count;
        }

        // 获得 element 占用的总 byte 数
        public static int GetTotalLength(byte[] data)
        {
            Precursor precursor = new Precursor(data[0]);

            int padding_length_count = 0;   // “填充字节数”位，的字节个数。0 或 1
            int additional_oid_count = 0;   // 附加的 OID 字节个数。0 或 1
            int padding_count = 0;  // padding byte 个数。注意这个值没有包含 padding length byte 本身的 1
            int compacted_data_length = 0;

            if (precursor.Offset)
            {
                padding_length_count = 1;
                padding_count = data[1];
            }

            if (precursor.ObjectIdentifier >= 15)
                additional_oid_count = 1;

            compacted_data_length = data[1 + padding_length_count + additional_oid_count];

            return 1 // (precursor) byte
                + padding_length_count // (padding length) byte
                + additional_oid_count // (additional oid value) byte
                + 1 // (length of data) byte
                + compacted_data_length
                + padding_count;
        }

        public override string ToString()
        {
            string compacted = "";
            if (this._precursor != null)
                compacted = Convert.ToString(this._precursor.CompactionCode, 2).PadLeft(3, '0');
            string bin = GetHexString(this.Content);
            if (this.OID == ElementOID.ContentParameter)
            {
                bin += "(" + GetContentParameterDesription(this.Content) + ")";
            }
            return $"OID={((int)this.OID)}({this.OID}),text='{this.Text}',content={bin},compacted={compacted}, OriginData={GetHexString(this.OriginData)}";
        }

        // 获得 Content Parameter 的解释文字
        public static string GetContentParameterDesription(byte[] data)
        {
            List<string> names = new List<string>();
            int index = 3;
            foreach (byte b in data)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (((b << i) & 0x80) == 0x80)
                    {
                        names.Add(((ElementOID)index).ToString());
                    }
                    index++;
                }
            }

            return string.Join(",", names.ToArray());
        }

        public static string GetHexString(byte value)
        {
            string strHex = Convert.ToString(value, 16).ToUpper();
            return strHex.PadLeft(2, '0');
        }

        // 得到用16进制字符串表示的 bin 内容
        public static string GetHexString(byte[] baTimeStamp, string format = "")
        {
            if (baTimeStamp == null)
                return "";
            if (string.IsNullOrEmpty(format))
            {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < baTimeStamp.Length; i++)
                {
                    //string strHex = String.Format("{0,2:X}",baTimeStamp[i]);
                    string strHex = Convert.ToString(baTimeStamp[i], 16).ToUpper();
                    text.Append(strHex.PadLeft(2, '0'));
                }

                return text.ToString();
            }

            // 每行四个 byte
            if (int.TryParse(format, out int bytes_per_line) == true)
            {
                if (bytes_per_line < 1)
                    throw new ArgumentException($"format 参数值应为 1 或以上的一个数字");

                StringBuilder text = new StringBuilder();
                for (int i = 0; i < baTimeStamp.Length; i++)
                {
                    string strHex = Convert.ToString(baTimeStamp[i], 16).ToUpper();
                    text.Append(strHex.PadLeft(2, '0'));
                    if ((i % bytes_per_line) < bytes_per_line - 1)
                        text.Append(" ");
                    if ((i % bytes_per_line) == bytes_per_line - 1)
                        text.Append("\r\n");
                }

                return text.ToString();
            }
            else
                throw new Exception($"未知的风格 '{format}'");
        }

        // 得到 byte[]类型 内容
        public static byte[] FromHexString(string strHexTimeStamp)
        {
            if (string.IsNullOrEmpty(strHexTimeStamp) == true)
                return null;

            strHexTimeStamp = strHexTimeStamp.Replace(" ", "").Replace("\r\n", "").ToUpper();

            byte[] result = new byte[strHexTimeStamp.Length / 2];

            for (int i = 0; i < strHexTimeStamp.Length / 2; i++)
            {
                string strHex = strHexTimeStamp.Substring(i * 2, 2);
                result[i] = Convert.ToByte(strHex, 16);
            }

            return result;
        }
    }

    public class Precursor
    {
        public byte OriginData { get; set; }

        // 1 bit
        public bool Offset { get; set; }
        // 3 bits
        public int CompactionCode { get; set; }
        // 4 bits
        public int ObjectIdentifier { get; set; }

        public Precursor()
        {

        }

        public Precursor(byte data)
        {
            this.OriginData = data;
            Parse(data);
        }

        void Parse(byte data)
        {
            // 第一个 bit
            this.Offset = ((data & 0x80) == 0x80);
            // 从第二个 bit 开始，一共三个 bit
            this.CompactionCode = (data >> 4) & 0x07;
            // 最后四个 bit
            this.ObjectIdentifier = data & 0x0f;
        }

#if NO
        // 构造一个 Precursor 对象
        public Precursor Compact(bool offset, int compaction_code, int oid)
        {
            Precursor result = new Precursor
            {
                Offset = offset,
                CompactionCode = compaction_code,
                ObjectIdentifier = oid
            };

            return result;
        }
#endif
        public byte ToByte()
        {
            int result = 0;
            // 第一个 bit
            if (this.Offset)
                result |= 0x80;

            // 从第二个 bit 开始，一共三个 bit
            result |= (this.CompactionCode << 4) & 0x70;

            // 最后四个 bit
            result |= this.ObjectIdentifier & 0x0f;

            return (byte)result;
        }
    }

    // 填充字节溢出异常
    public class PaddingOverflowException : Exception
    {
        public int MaxDelta { get; set; }

        public PaddingOverflowException(string message, int max_delta) : base(message)
        {
            this.MaxDelta = max_delta;
        }
    }
}
