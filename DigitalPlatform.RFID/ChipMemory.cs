using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    /// <summary>
    /// 模拟 RFID 芯片内存物理结构
    /// </summary>
    public class ChipMemory
    {
        List<Block> _blocks = new List<Block>();

        // 获得全部内容
        public byte[] GetBytes()
        {
            List<byte> results = new List<byte>();
            foreach (Block block in _blocks)
            {
                results.AddRange(block.Data);
            }

            return results.ToArray();
        }
    }

    /// <summary>
    /// 模拟 RFID 芯片内的逻辑结构。便于执行整体压缩，解压缩的操作
    /// </summary>
    public class LogicChip
    {
        List<Element> _elements = new List<Element>();

        public List<Element> Elements
        {
            get
            {
                return _elements;
            }
        }

        // 查找一个元素
        public Element FindElement(string name)
        {
            // 把 name 变为 OID
            if (Enum.TryParse<ElementOID>(name, out ElementOID oid) == false)
                throw new ArgumentException($"name '{name}' 不合法");

            foreach (Element element in this._elements)
            {
                if (element.OID == (int)oid)
                    return element;
            }

            return null;
        }

        public static string GetOidName(ElementOID oid)
        {
            return Enum.GetName(typeof(ElementOID), oid);
        }

        public Element NewElement(string name, string content)
        {
            // 把 name 变为 OID
            if (Enum.TryParse<ElementOID>(name, out ElementOID oid) == false)
                throw new ArgumentException($"name '{name}' 不合法");

            // 查重
            {
                foreach (Element element in this._elements)
                {
                    if (element.OID == (int)oid)
                        throw new Exception($"名字为 {name}, OID 为 {element.OID} 的元素已经存在，无法重复创建");
                }
            }

            {
                Element element = new Element
                {
                    OID = (int)oid,
                    Name = GetOidName(oid)
                };

                _elements.Add(element);
                // 注：此处不对 elements 排序。最后需要的时候(比如组装的阶段)再排序
                return element;
            }
        }

        // return:
        //      非null    表示找到，并删除
        //      null   表示没有找到指定的元素
        public Element RemoveElement(string name)
        {
            // 把 name 变为 OID
            if (Enum.TryParse<ElementOID>(name, out ElementOID oid) == false)
                throw new ArgumentException($"name '{name}' 不合法");

            foreach (Element element in this._elements)
            {
                if (element.OID == (int)oid)
                {
                    _elements.Remove(element);
                    return element;
                }
            }
            return null;
        }

        // 根据物理数据构造 (拆包)
        public static LogicChip From(ChipMemory memory)
        {
            byte[] bytes = memory.GetBytes();

            return null;
        }


        // 输出为物理数据格式 (打包)
        public ChipMemory ToChipMemory()
        {
            return null;
        }

        // 根据 XML 数据构造
        public static LogicChip FromXml(string xml)
        {
            return null;
        }

        // 输出为 XML 格式
        public string ToXml()
        {
            return "";
        }

        // 输出为便于观察的文本形态
        public override string ToString()
        {
            return "";
        }
    }

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
        localDataA = 15,

        // Local data B
        LDB = 16,
        localDataB = 16,

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
        AlternativeOwnerInstitution = 24,

        // Subsidiary of an owner institution
        SOI = 24,
        SubsidiaryOfAnOwnerInstitution = 24,

        // Alternative ILL borrowing institution
        AIBI = 25,
        AlternativeIllBorrowingInstitution = 26,

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

        public string Name { get; set; }
        public int OID { get; set; }
        public string Text { get; set; }
        public byte[] Content { get; set; }

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

            // 自动选定压缩方案
            if (compact_method == CompactionScheme.Null)
            {
                compact_method = Compress.AutoSelectCompressMethod(text);
                if (compact_method == CompactionScheme.Null)
                    throw new Exception($"无法为字符串 '{text}' 自动选定压缩方案");
            }

            if (compact_method == CompactionScheme.ISIL)
                precursor.CompactionCode = (int)CompactionScheme.ApplicationDefined;
            else
                precursor.CompactionCode = (int)compact_method;

            byte[] data = null;
            if (compact_method == CompactionScheme.Integer)
                data = Compress.IntegerCompress(text);
            else if (compact_method == CompactionScheme.Numeric)
                data = Compress.NumericCompress(text);
            else if (compact_method == CompactionScheme.FivebitCode)
                data = Compress.Bit5Compress(text);
            else if (compact_method == CompactionScheme.SixBitCode)
                data = Compress.Bit6Compress(text);
            else if (compact_method == CompactionScheme.Integer)
                data = Compress.IntegerCompress(text);
            else if (compact_method == CompactionScheme.SevenBitCode)
                data = Compress.Bit7Compress(text);
            else if (compact_method == CompactionScheme.OctectString)
                data = Encoding.ASCII.GetBytes(text);
            else if (compact_method == CompactionScheme.Utf8String)
                data = Encoding.UTF8.GetBytes(text);
            else if (compact_method == CompactionScheme.ISIL)
                data = Compress.IsilCompress(text);
            else if (compact_method == CompactionScheme.Base64)
                data = Convert.FromBase64String(text);

            // 通过 data 计算出是否需要 padding bytes
            int total_bytes = 1 // precursor
                + 1 // length of data
                + data.Length;  // data
            if (precursor.ObjectIdentifier >= 15)
                total_bytes++;  // relative-OID 需要多占一个 byte

            int paddings = 0;
            if ((total_bytes % 4) != 0)
            {
                paddings = 4 - (total_bytes % 4);
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

            Element element = new Element();

            if (data.Length - offset < 1)
                throw new Exception($"data 长度不足，从 {offset} 开始应至少为 1 bytes");

            element._precursor = new Precursor(data[offset]);

            offset++;

            // OID 为 1-14, 元素存储结构为 Precursor + Length of data + Compacted data
            if (element._precursor.ObjectIdentifier <= 14)
            {
                element.OID = element._precursor.ObjectIdentifier;
            }
            else
            {
                // OID 为 15-127。元素存储结构为 Precursor + Relative-OID 15 to 127 + Length of data + Compacted data
                if (data.Length - offset < 3)
                    throw new Exception($"data 长度不足，从 {offset} 开始应至少为 3 bytes");

                element.OID = 15 + data[offset];
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

            // 解析出 Text
            if (element._precursor.CompactionCode == (int)CompactionScheme.ApplicationDefined)
            {
                if (element.OID == (int)ElementOID.OwnerInstitution
                    || element.OID == (int)ElementOID.IllBorrowingInstitution
                    )
                {
                    element.Text = Compress.IsilExtract(element._compactedData);
                }
                else
                {
                    // 只能用 Content 表示
                    element.Content = element._compactedData;
                    // TODO: 此时 Text 里面放什么？是否要让 get Text 抛出异常引起注意?
                }
            }
            else
            {
                if (element._precursor.CompactionCode == (int)CompactionScheme.Integer)
                    element.Text = Compress.IntegerExtract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.Integer)
                    element.Text = Compress.IntegerExtract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.Numeric)
                    element.Text = Compress.NumericExtract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.FivebitCode)
                    element.Text = Compress.Bit5Extract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.SixBitCode)
                    element.Text = Compress.Bit6Extract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.SevenBitCode)
                    element.Text = Compress.Bit7Extract(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.OctectString)
                    element.Text = Encoding.ASCII.GetString(element._compactedData);
                else if (element._precursor.CompactionCode == (int)CompactionScheme.Utf8String)
                    element.Text = Encoding.UTF8.GetString(element._compactedData);
                else
                    throw new Exception($"出现意料之外的 CompactScheme {element._precursor.CompactionCode}");
            }

            return element;
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

    // 模拟一个块的结构
    public class Block
    {
        byte[] _data = null;

        public byte[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
    }

    // 数据压缩方案
    // ISO 28560-2:2014(E), page 17
    public enum CompactionScheme
    {
        ApplicationDefined = 0,
        Integer = 1,
        Numeric = 2,
        FivebitCode = 3,
        SixBitCode = 4,
        SevenBitCode = 5,
        OctectString = 6,
        Utf8String = 7,

        // 以下是扩展的几个值
        Null = -1,  // 表示希望自动选择
        ISIL = -2,  // 特殊地 ISIL 压缩方案
        Base64 = -3,    // base64 方式给出 byte[] 内容
    }
}
