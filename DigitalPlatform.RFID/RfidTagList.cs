using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DigitalPlatform.Text;
using static DigitalPlatform.RFID.GaoxiaoUtility;
using static DigitalPlatform.RFID.RfidTagList;

namespace DigitalPlatform.RFID
{

    // 存储 Tag 的数据结构。可以动态表现当前读卡器上的所有标签
    // 原名为 TagList，为避免和 System.Diagnostics.TagList 名字冲突，改为 RfidTagList
    public static class RfidTagList
    {
        static object _sync_books = new object();
        static List<TagAndData> _books = new List<TagAndData>();

        static object _sync_patrons = new object();
        static List<TagAndData> _patrons = new List<TagAndData>();

        public static List<TagAndData> Books
        {
            get
            {
                lock (_sync_books)
                {
                    List<TagAndData> results = new List<TagAndData>();
                    results.AddRange(_books);
                    return results;
                }
            }
        }

        public static List<TagAndData> Patrons
        {
            get
            {
                lock (_sync_patrons)
                {
                    List<TagAndData> results = new List<TagAndData>();
                    results.AddRange(_patrons);
                    return results;
                }
            }
        }


        /*
        static bool _dataReady = false;
        public static bool DataReady
        {
            get
            {
                return _dataReady;
            }
            set
            {
                _dataReady = value;
            }
        }
        */

        static void ClearTagInfo(string uid)
        {
            lock (_sync_books)
            {
                foreach (TagAndData data in _books)
                {
                    if (data.OneTag == null)
                        continue;
                    if (string.IsNullOrEmpty(uid)
                        || data.OneTag.UID == uid)
                        data.OneTag.TagInfo = null;
                }
            }

            ClearTypeTable(uid);
        }

        static void ClearTypeTable(string uid)
        {
            lock (_sync_typeTable)
            {
                if (string.IsNullOrEmpty(uid))
                    _typeTable.Clear();
                else
                    _typeTable.Remove(uid);
            }
        }

        // 从缓存中尝试查找 TypeOfUsage
        static string FindUsageFromTypeTable(string uid)
        {
            lock (_sync_typeTable)
            {
                return (string)_typeTable[uid];
            }
        }

        static TagAndData FindBookTag(string uid)
        {
            lock (_sync_books)
            {
                foreach (TagAndData tag in _books)
                {
                    if (tag.OneTag.UID == uid)
                        return tag;
                }
                return null;
            }
        }

        static TagAndData FindPatronTag(string uid)
        {
            lock (_sync_patrons)
            {
                foreach (TagAndData tag in _patrons)
                {
                    if (tag.OneTag.UID == uid)
                        return tag;
                }
                return null;
            }
        }

        public delegate void delegate_notifyChanged(
            List<TagAndData> new_books,
            List<TagAndData> changed_books,
            List<TagAndData> removed_books,
            List<TagAndData> new_patrons,
            List<TagAndData> changed_patrons,
            List<TagAndData> removed_patrons);
        public delegate void delegate_setError(string type, string error);

        // 2020/4/10
        // 判断标签是图书标签还是读者卡类型？
        // return:
        //      ""  不确定类型
        //      "patron"    读者卡
        //      "book"      图书标签
        public delegate string delegate_detectType(OneTag tag);

        // TODO: 维持一个 UID --> typeOfUsage 的对照表，加快对图书和读者类型标签的分离判断过程
        // UID --> typeOfUsage string
        static Hashtable _typeTable = new Hashtable();
        static object _sync_typeTable = new object();

        // TODO: 可以把 readerNameList 先 Parse 到一个结构来加快 match 速度
        static bool InRange(TagAndData data, string readerNameList)
        {
            // 匹配读卡器名字
            var ret = Reader.MatchReaderName(readerNameList, data.OneTag.ReaderName, out string antenna_list);
            if (ret == false)
                return false;
            var list = GetAntennaList(antenna_list);
            // 2019/12/19
            // 如果列表为空，表示不指定天线编号，所以任何 data 都是匹配命中的
            // RL8600 默认天线编号为 0；M201 默认天线编号为 1。列表为空的情况表示不指定天线编号有助于处理好这两种不同做法的读写器
            if (list.Count == 0)
                return true;
            if (list.IndexOf(data.OneTag.AntennaID) != -1)
                return true;
            return false;   // ret is bug!!!
        }

        // 把天线列表字符串变换为 List<uint> 类型
        // 注意 list 为空时候返回一个空集合
        static List<uint> GetAntennaList(string list)
        {
            if (string.IsNullOrEmpty(list) == true)
                return new List<uint>();    // 字符串是空，返回的列表就是空
            string[] numbers = list.Split(new char[] { '|', ',' });
            List<uint> bytes = new List<uint>();
            foreach (string number in numbers)
            {
                bytes.Add(Convert.ToUInt32(number));
            }

            return bytes;
        }

        // 2023/11/12
        // 根据 RSSI 过滤信号强度低于阈值的 tag
        public static List<OneTag> FilterByRSSI(List<OneTag> tags,
            byte rssi)
        {
            if (tags == null || tags.Count == 0)
                return tags;
            List<OneTag> results = new List<OneTag>();
            foreach (OneTag tag in tags)
            {
                if (tag.Protocol != InventoryInfo.ISO18000P6C)
                {
                    results.Add(tag);
                    continue;
                }
                if (tag.RSSI < rssi)
                    continue;
                results.Add(tag);
            }
            return results;
        }

        // 2023/11/16
        // 超高频“只读入 EPC”的加速模式
        public static bool OnlyReadEPC { get; set; }

        // parameters:
        //      readerNameList  list中包含的内容的读卡器名(列表)。注意 list 中包含的标签，可能并不是全部读卡器的标签。对没有包含在其中的标签，本函数需要注意跳过(维持现状)，不要当作被删除处理
        // 异常：
        //      可能抛出 TagInfoException
        public static void Refresh(BaseChannel<IRfid> channel,
            string readerNameList,
            List<OneTag> list,
            delegate_notifyChanged notifyChanged,
            delegate_setError setError/*,
            delegate_detectType detectType = null*/)
        {
            try
            {
                setError?.Invoke("rfid", null);

                List<TagAndData> new_books = new List<TagAndData>();
                List<TagAndData> new_patrons = new List<TagAndData>();

                List<TagAndData> error_books = new List<TagAndData>();
                List<TagAndData> error_patrons = new List<TagAndData>();

                // 从当前列表中发现已有的图书。用于交叉运算
                List<TagAndData> found_books = new List<TagAndData>();
                // 从当前列表中发现已有的读者。用于交叉运算
                List<TagAndData> found_patrons = new List<TagAndData>();

                // 即便是发现已经存在 UID 的标签，也要再判断一下 Antenna 是否不同。如果有不同，要进行变化通知
                // 从当前列表中发现(除了 UID) 内容有变化的图书。这些图书也会进入 found_books 集合
                List<TagAndData> changed_books = new List<TagAndData>();

                foreach (OneTag tag in list)
                {
                    // 检查以前的列表中是否已经有了
                    var book = FindBookTag(tag.UID);
                    if (book != null)
                    {
                        found_books.Add(book);
                        if (book.OneTag.AntennaID != tag.AntennaID
                            || book.OneTag.ReaderName != tag.ReaderName    // // 2020/10/17
                            /*|| book.OneTag.RSSI != tag.RSSI*/)    // 2023/11/12 RSSI 变动很频繁，如果 update 信号给到内务快捷出纳窗会造成触发条码输入的结果
                        {
                            // 修改 AntennaID
                            book.OneTag.AntennaID = tag.AntennaID;
                            // 修改 ReaderName
                            book.OneTag.ReaderName = tag.ReaderName;
                            // 修改 RSSI
                            // book.OneTag.RSSI = tag.RSSI;
                            changed_books.Add(book);

                            // 只清理缓存
                            _tagTable.Remove(book.OneTag.UID);
                        }

                        if (string.IsNullOrEmpty(book.Error) == false)
                            error_books.Add(book);
                        continue;
                    }
                    var patron = FindPatronTag(tag.UID);
                    if (patron != null)
                    {
                        found_patrons.Add(patron);
                        if (string.IsNullOrEmpty(patron.Error) == false)
                            error_patrons.Add(patron);
                        continue;
                    }

                    // ISO14443A 的一律当作读者证卡
                    if (tag.Protocol == InventoryInfo.ISO14443A)
                    {
                        patron = new TagAndData { OneTag = tag, Type = "patron" };
                        new_patrons.Add(patron);
                    }
                    else
                    {
                        // 根据缓存的 typeOfUsage 来判断
                        // string typeOfUsage = (string)_typeTable[tag.UID];
                        string typeOfUsage = FindUsageFromTypeTable(tag.UID);

                        if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                        {
                            // TODO: 这里有个问题就是 TagInfo 为 null。需要专门获得一次 TagInfo
                            patron = new TagAndData { OneTag = tag, Type = "patron" };
                            new_patrons.Add(patron);
                            found_books.Remove(patron);
                        }
                        else
                        {
                            // ISO15693 的则先添加到 _books 中。等类型判断完成，有可能还要调整到 _patrons 中
                            book = new TagAndData { OneTag = tag };
                            new_books.Add(book);
                        }
                    }
                }

                List<TagAndData> remove_books = new List<TagAndData>();
                List<TagAndData> remove_patrons = new List<TagAndData>();

                // 交叉运算
                // 注意对那些在 readerNameList 以外的标签不要当作 removed 处理
                foreach (TagAndData book in _books)
                {
                    if (InRange(book, readerNameList) == false)
                        continue;
                    if (found_books.IndexOf(book) == -1)
                        remove_books.Add(book);
                }

                foreach (TagAndData patron in _patrons)
                {
                    if (InRange(patron, readerNameList) == false)
                        continue;

                    if (found_patrons.IndexOf(patron) == -1)
                        remove_patrons.Add(patron);
                }

                bool array_changed = false;

                // 兑现添加
                lock (_sync_books)
                {
                    foreach (TagAndData book in new_books)
                    {
                        if (_books.IndexOf(book) == -1)
                        {
                            _books.Add(book);
                            array_changed = true;
                        }
                    }
                    // 兑现删除
                    foreach (TagAndData book in remove_books)
                    {
                        _books.Remove(book);
                        array_changed = true;
                    }
                }

                lock (_sync_patrons)
                {
                    foreach (TagAndData patron in new_patrons)
                    {
                        if (_patrons.IndexOf(patron) == -1)
                        {
                            _patrons.Add(patron);
                            array_changed = true;
                        }
                    }
                    foreach (TagAndData patron in remove_patrons)
                    {
                        _patrons.Remove(patron);
                        array_changed = true;
                    }
                }

                // 通知一次变化
                if (array_changed
                    || changed_books.Count > 0
                    || new_books.Count > 0 || remove_books.Count > 0
                    || new_patrons.Count > 0 || remove_patrons.Count > 0)    // 2019/8/15 优化
                    notifyChanged(new_books, changed_books, remove_books,
                        new_patrons, null, remove_patrons);

                // 需要获得 Tag 详细信息的。注意还应当包含以前出错的 Tag
                //List<TagAndData> news = new_books;
                // news.AddRange(error_books);

                List<TagAndData> news = new List<TagAndData>();
                news.AddRange(_books);
                news.AddRange(new_patrons);

                new_books = new List<TagAndData>();
                remove_books = new List<TagAndData>();
                new_patrons = new List<TagAndData>();
                remove_patrons = new List<TagAndData>();

                // .TagInfo 是否发生过填充
                bool taginfo_changed = false;
                {
                    List<TagAndData> update_books = new List<TagAndData>();
                    List<TagAndData> update_patrons = new List<TagAndData>();

                    // 逐个获得新发现的 ISO15693/ISO18000P6C 标签的详细数据，用于判断图书/读者类型
                    foreach (TagAndData data in news)
                    {
                        OneTag tag = data.OneTag;
                        if (tag == null)
                            continue;
                        if (tag.TagInfo != null && data.Error == null)
                            continue;
                        if (tag.Protocol == InventoryInfo.ISO14443A)
                            continue;
                        {
                            /*
                            // TODO
                            GetTagInfoResult gettaginfo_result = null;
                            if (tag.InventoryInfo == null)
                                gettaginfo_result = GetTagInfo(channel, tag.UID);
                            else
                                gettaginfo_result = GetTagInfo(channel, tag.InventoryInfo);
                                */
                            // 自动重试一次
                            GetTagInfoResult gettaginfo_result = null;

                            // 2023/11/16
                            // “只读入 EPC”加速模式，直接用 UID 合成 TagInfo 结构
                            if (OnlyReadEPC && tag.Protocol == InventoryInfo.ISO18000P6C)
                            {
                                gettaginfo_result = new GetTagInfoResult();
                                // TODO: 对于疑似非图书类型的标签，是不是还是要走向真正获取一次 User Bank 的分支？
                                gettaginfo_result.TagInfo = new TagInfo
                                {
                                    Protocol = tag.Protocol,
                                    ReaderName = tag.ReaderName,
                                    AntennaID = tag.AntennaID,
                                    UID = tag.UID,
                                    Bytes = null,
                                };
                            }
                            else
                            {
                                int i = 0;
                                for (i = 0; i < 2; i++)
                                {
                                    gettaginfo_result = GetTagInfo(channel, tag.ReaderName, tag.UID, tag.AntennaID, tag.Protocol);
                                    if (gettaginfo_result.Value != -1)
                                        break;
                                }
                            }

                            if (gettaginfo_result.Value == -1)
                            {
                                setError?.Invoke("rfid", gettaginfo_result.ErrorInfo);
                                // TODO: 是否直接在标签上显示错误文字?
                                data.Error = gettaginfo_result.ErrorInfo;
                                if (data.Type == "patron")
                                    update_patrons.Add(data);
                                else
                                    update_books.Add(data);
                                continue;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(data.Error) == false)
                                {
                                    data.Error = null;
                                    if (data.Type == "patron")
                                        update_patrons.Add(data);
                                    else
                                        update_books.Add(data);
                                }
                            }

                            TagInfo info = gettaginfo_result.TagInfo;

                            // 记下来。避免以后重复再次去获取了
                            if (tag.TagInfo == null)
                            {
                                tag.TagInfo = info;
                                taginfo_changed = true;

                                // 2019/8/25
                                if (data.Type == "patron")
                                {
                                    if (update_patrons.IndexOf(data) == -1)
                                        update_patrons.Add(data);
                                }
                                else
                                {
                                    /*
                                    if (update_books.IndexOf(data) == -1)
                                        update_books.Add(data);
                                        */
                                }
                            }

                            // 对于不确定类型的标签(data.Type 为空的)，再次确定类型以便分离 ISO15693 图书标签和读者卡标签
                            if (string.IsNullOrEmpty(data.Type))
                            {
                                LogicChip chip = null;
                                string typeOfUsage = "";

                                if (tag.Protocol == InventoryInfo.ISO18000P6C)
                                {
                                    // UHF
                                    try
                                    {
                                        var chip_info = GetUhfChipInfo(info);
                                        // 2024/4/25
                                        // chip_info.ErrorInfo 中可能有报错信息
                                        if (string.IsNullOrEmpty(chip_info.ErrorInfo) == false)
                                        {
                                            setError?.Invoke("rfid", chip_info.ErrorInfo);
                                            data.Error = chip_info.ErrorInfo;
                                        }
                                        typeOfUsage = chip_info.Chip?.FindElement(ElementOID.TypeOfUsage)?.Text;
                                    }
                                    catch
                                    {
                                        typeOfUsage = "";
                                    }
                                }
                                else
                                {
                                    // HF
                                    try
                                    {
                                        // 观察 typeOfUsage 元素
                                        // Exception:
                                        //      可能会抛出异常 ArgumentException TagDataException
                                        chip = LogicChip.From(info.Bytes,
                    (int)info.BlockSize,
                    "");
                                        typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                                    }
                                    catch (TagDataException ex)
                                    {
                                        // throw new TagInfoException(ex.Message, info);

                                        // 解析错误的标签，当作图书标签处理
                                        typeOfUsage = "";
                                    }
                                }

                                if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                                {
                                    // 需要调整到 _patrons 中
                                    data.Type = "patron";
                                    // 删除列表? 同时添加列表
                                    remove_books.Add(data);
                                    update_books.Remove(data);  // 容错
                                    new_patrons.Add(data);
                                }
                                else
                                {
                                    data.Type = "book";
                                    update_books.Add(data);
                                    update_patrons.Remove(data);
                                }

                                // 保存到缓存
                                if (typeOfUsage != null)
                                {
                                    lock (_sync_typeTable)
                                    {
                                        if (_typeTable.Count > 1000)
                                            _typeTable.Clear();
                                        _typeTable[data.OneTag.UID] = typeOfUsage;
                                    }
                                }
                            }
                        }
                    } // end of foreach

                    array_changed = false;
                    // 再次兑现添加和删除
                    // 兑现添加
                    lock (_sync_books)
                    {
                        foreach (TagAndData book in new_books)
                        {
                            _books.Add(book);
                            array_changed = true;
                        }
                        // 兑现删除
                        foreach (TagAndData book in remove_books)
                        {
                            _books.Remove(book);
                            array_changed = true;
                        }
                    }

                    lock (_sync_patrons)
                    {
                        foreach (TagAndData patron in new_patrons)
                        {
                            _patrons.Add(patron);
                            array_changed = true;
                        }
                        foreach (TagAndData patron in remove_patrons)
                        {
                            _patrons.Remove(patron);
                            array_changed = true;
                        }
                    }

                    // 再次通知变化
                    if (array_changed == true
                        || taginfo_changed == true
                        || new_books.Count > 0
                        || update_books.Count > 0
                        || remove_books.Count > 0
                        || new_patrons.Count > 0
                        || update_patrons.Count > 0
                        || remove_patrons.Count > 0)    // 2019/8/15 优化
                        notifyChanged(new_books, update_books, remove_books,
                            new_patrons, update_patrons, remove_patrons);
                }
            }
            finally
            {
                // _dataReady = true;
            }
        }

        public class ChipInfo
        {
            public LogicChip Chip { get; set; }

            // 注意这里是 OI 或者 AOI 合并到一个字段
            public string OI { get; set; }

            public string PII { get; set; }

            public string UII { get; set; }

            public string UhfProtocol { get; set; }

            // (当 taginfo.Byte 为 null 时)根据 UMI 和 Content Parameters 判断是否具备 OI 元素
            public bool ContainOiElement { get; set; }

            public string ErrorInfo { get; set; }
        }

        public static ChipInfo GetChipInfo(TagInfo taginfo,
            string style = "convertValueToGB,ensureChip")   // 2025/9/30 增加了 ensureChip 缺省值
        {
            if (taginfo.Protocol == InventoryInfo.ISO15693)
            {
                return GetHfChipInfo(taginfo, style);
            }
            else if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
            {
                return GetUhfChipInfo(taginfo, style);
            }

            throw new ArgumentException($"GetChipInfo() 无法识别的 RFID 协议 '{taginfo.Protocol}'");
        }

        public static ChipInfo GetHfChipInfo(TagInfo taginfo,
            string style = "")
        {
            ChipInfo result = new ChipInfo();

            try
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                result.Chip = LogicChip.From(taginfo.Bytes,
        (int)taginfo.BlockSize,
        ""  // taginfo.LockStatus
        );
                result.UhfProtocol = null;

                result.PII = result.Chip?.FindElement(ElementOID.PII)?.Text;

                string oi = result.Chip?.FindElement(ElementOID.OI)?.Text;
                if (string.IsNullOrEmpty(oi))
                    oi = result.Chip?.FindElement(ElementOID.AOI)?.Text;
                result.OI = oi;

                // 构造 UII
                if (string.IsNullOrEmpty(oi))
                    result.UII = result.PII;
                else
                    result.UII = oi + "." + result.PII;

                return result;
            }
            catch (Exception ex)
            {
                result.ErrorInfo = ex.Message;
                return result;
            }
        }

        // 注1: taginfo.EAS 在调用后可能被修改
        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
        // parameters:
        //      style   解析高校联盟 UHF 格式时的 style。缺省为 "convertValueToGB"
        //              dontCheckUMI 表示不检查 PC UMI 标志位。这常用于解析一些缺失 User Bank 内容的畸形标签内容
        //              ensureChip 确保返回前创建 LogicChip 对象
        public static ChipInfo GetUhfChipInfo(TagInfo taginfo,
            string style = "convertValueToGB,ensureChip")
        {
            ChipInfo result = new ChipInfo();

            var epc_bank = Element.FromHexString(taginfo.UID);

            if (UhfUtility.IsBlankTag(epc_bank, taginfo.Bytes) == true)
            {
                // 空白标签
                result.UII = null;
            }
            else
            {
                var isGB = UhfUtility.IsISO285604Format(epc_bank, taginfo.Bytes);
                if (isGB)
                {
                    // *** 国标 UHF
                    var parse_result = UhfUtility.ParseTag(epc_bank,
        taginfo.Bytes,
        4,
        "");
                    if (parse_result.Value == -1)
                    {
                        // throw new Exception(parse_result.ErrorInfo);
                        result.ErrorInfo = parse_result.ErrorInfo;
                        return result;
                    }
                    result.Chip = parse_result.LogicChip;

                    // TODO: result.ContainOiElement

                    // pc.AFI = enable ? 0x07 : 0xc2;
                    taginfo.EAS = parse_result.PC.AFI == 0x07;
                    taginfo.AFI = (byte)parse_result.PC.AFI;    // 2025/6/7
                    result.UhfProtocol = "gb";
                    result.UII = parse_result.SafetyUII;
                    result.PII = GetPiiPart(parse_result.SafetyUII);
                    result.OI = GetOiPart(parse_result.SafetyUII, false);

                    // 2023/12/22
                    // 为 chip 中添加 PII 和 OI 元素
                    if (parse_result.LogicChip != null)
                        UhfUtility.AddPiiOi(parse_result.SafetyUII, parse_result.LogicChip);
                }
                else
                {
                    // *** 高校联盟 UHF
                    var parse_result = GaoxiaoUtility.ParseTag(
        epc_bank,
        taginfo.Bytes,
        style/*没有包含 checkUMI，表示允许“望湖洞庭”通过*/);  // "convertValueToGB"
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
                    result.Chip = parse_result.LogicChip;

                    result.ContainOiElement = parse_result.PC == null ? false
                        : ParseGaoxiaoResult.ContainUserBankOiElement(
                        parse_result.PC.UMI,
                        parse_result.EpcInfo?.ContentParameters);
                    // TODO: 如果 Chip 为 null，需要 new 一个，并且把 PII 等内容放到其 Elements 中，以便上层可以正常使用

                    taginfo.EAS = parse_result.EpcInfo == null ? false : !parse_result.EpcInfo.Lending;
                    taginfo.AFI = (byte)parse_result.PC.AFI;    // 2025/6/7
                    result.UhfProtocol = "gxlm";

                    result.PII = GetPiiPart(parse_result.EpcInfo?.PII);

                    // 从 User Bank 中取得 OI
                    string oi = result.Chip?.FindGaoxiaoOI();
                    /*
                    string oi = result.Chip?.FindElement(ElementOID.OI)?.Text;
                    if (string.IsNullOrEmpty(oi))
                        oi = result.Chip?.FindElement((ElementOID)27)?.Text;    // 注: 高校联盟没有 AOI 字段，只有 27 字段
                    */
                    result.OI = oi;   // GetOiPart(parse_result.EpcInfo.PII, false);

                    // 构造 UII
                    if (string.IsNullOrEmpty(oi))
                        result.UII = result.PII;
                    else
                        result.UII = oi + "." + result.PII;
                }
            }

            // 是否要确保返回前创建 LogicChip 对象
            var ensure_chip = StringUtil.IsInList("ensureChip", style);

            if (result.Chip == null && ensure_chip)
            {
                result.Chip = new LogicChip();
                if (string.IsNullOrEmpty(result.PII) == false)
                    result.Chip.SetElement(ElementOID.PII, result.PII, false);
                if (string.IsNullOrEmpty(result.OI) == false)
                {
                    // TODO: 把不是 ISIL 形态的放入 AOI 元素?
                    result.Chip.SetElement(ElementOID.OI, result.OI, false);
                }
            }
            return result;
        }

        // 2023/11/20
        // 根据 EPC Bank 获知 UHF 标签的 EAS 状态
        // parameters:
        //      gb_afi [out] 返回国标格式的 AFI。注意，针对高校联盟格式会返回 0
        public static bool GetUhfEas(string uid,
            out int gb_afi)
        {
            gb_afi = 0;
            var epc_bank = Element.FromHexString(uid);

            if (UhfUtility.IsBlankTag(epc_bank, null) == true)
            {
                // 空白标签
                return false;
            }
            else
            {
                var isGB = UhfUtility.IsISO285604Format(epc_bank, null);
                if (isGB)
                {
                    // *** 国标 UHF
                    var pc = UhfUtility.ParsePC(epc_bank, 2);
                    gb_afi = pc.AFI;
                    if (pc.AFI == 0x07)
                        return true;
                    return false;
                }
                else
                {
                    // var use_gxlm_pii = StringUtil.IsInList("gxlm_pii", style);
                    // *** 高校联盟 UHF
                    var parse_result = GaoxiaoUtility.ParseTag(
        epc_bank,
        null,
        "");
                    if (parse_result.Value == -1)
                    {
                        return false;
                    }
                    if (parse_result.EpcInfo == null)
                        return false;
                    return !parse_result.EpcInfo.Lending;
                }
            }
        }

        public static string GetHfUii(TagInfo taginfo)
        {
            try
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                var chip = LogicChip.From(taginfo.Bytes,
        (int)taginfo.BlockSize,
        ""  // taginfo.LockStatus
        );

                var pii = chip?.FindElement(ElementOID.PII)?.Text;

                string oi = chip?.FindElement(ElementOID.OI)?.Text;
                if (string.IsNullOrEmpty(oi))
                    oi = chip?.FindElement(ElementOID.AOI)?.Text;

                // 构造 UII
                if (string.IsNullOrEmpty(oi))
                    return pii;
                else
                    return oi + "." + pii;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // 获得 UHF 标签的 UII 内容
        // parameters:
        //      style   风格。如果为 gxlm_pii ，表示不需要返回 oi 部分(因而也不需要解析 .Bytes 部分)
        // return:
        //      返回 UII 字符串。如果标签解析出错，或者是空白标签，则返回 "uid:xxxx" 形态
        public static string GetUhfUii(string uid,
            byte[] user_bank)
        {
            /*
            string uid = taginfo.UID;
            */
            var epc_bank = Element.FromHexString(uid);

            if (UhfUtility.IsBlankTag(epc_bank, null) == true)
            {
                // 空白标签
                return "uid:" + uid;
            }
            else
            {
                var isGB = UhfUtility.IsISO285604Format(epc_bank, null);
                if (isGB)
                {
                    // *** 国标 UHF
                    var parse_result = UhfUtility.ParseTag(epc_bank,
        null,
        4);
                    if (parse_result.Value == -1)
                        return "uid:" + uid;

                    return parse_result.SafetyUII;
                }
                else
                {
                    // var use_gxlm_pii = StringUtil.IsInList("gxlm_pii", style);
                    // *** 高校联盟 UHF
                    var parse_result = GaoxiaoUtility.ParseTag(
        epc_bank,
        user_bank,  // use_gxlm_pii ? null : taginfo.Bytes,
        "");  // "convertValueToGB"
                    if (parse_result.Value == -1)
                    {
                        return "uid:" + uid;
                    }

                    string pii = GetPiiPart(parse_result.EpcInfo?.PII);

                    if (user_bank == null)
                        return pii;
                    /*
                    if (use_gxlm_pii)
                        return pii;
                    */

                    // 从 User Bank 中取得 OI
                    string oi = parse_result.LogicChip?.FindGaoxiaoOI();
                    /*
                    string oi = parse_result.LogicChip?.FindElement(ElementOID.OI)?.Text;
                    if (string.IsNullOrEmpty(oi))
                        oi = parse_result.LogicChip?.FindElement((ElementOID)27)?.Text;    // 注: 高校联盟没有 AOI 字段，只有 27 字段
                    */
                    // 构造 UII
                    if (string.IsNullOrEmpty(oi))
                        return pii;
                    else
                        return oi + "." + pii;
                }
            }
        }


#if REMOVED
        // 获得 UHF 标签的 UII 内容
        // parameters:
        //      style   风格。如果为 gxlm_pii ，表示不需要返回 oi 部分(因而也不需要解析 .Bytes 部分)
        // return:
        //      返回 UII 字符串。如果标签解析出错，或者是空白标签，则返回 "uid:xxxx" 形态
        public static string GetUhfUii(TagInfo taginfo,
            string style)
        {
            string uid = taginfo.UID;
            var epc_bank = Element.FromHexString(uid);

            if (UhfUtility.IsBlankTag(epc_bank, null) == true)
            {
                // 空白标签
                return "uid:" + uid;
            }
            else
            {
                var isGB = UhfUtility.IsISO285604Format(epc_bank, null);
                if (isGB)
                {
                    // *** 国标 UHF
                    var parse_result = UhfUtility.ParseTag(epc_bank,
        null,
        4);
                    if (parse_result.Value == -1)
                        return "uid:" + uid;

                    return parse_result.UII;
                }
                else
                {
                    var use_gxlm_pii = StringUtil.IsInList("gxlm_pii", style);
                    // *** 高校联盟 UHF
                    var parse_result = GaoxiaoUtility.ParseTag(
        epc_bank,
        use_gxlm_pii ? null : taginfo.Bytes,
        "");  // "convertValueToGB"
                    if (parse_result.Value == -1)
                    {
                        return "uid:" + uid;
                    }

                    string pii = GetPiiPart(parse_result.EpcInfo?.PII);

                    if (use_gxlm_pii)
                        return pii;

                    // 从 User Bank 中取得 OI
                    string oi = parse_result.LogicChip?.FindGaoxiaoOI();
                    /*
                    string oi = parse_result.LogicChip?.FindElement(ElementOID.OI)?.Text;
                    if (string.IsNullOrEmpty(oi))
                        oi = parse_result.LogicChip?.FindElement((ElementOID)27)?.Text;    // 注: 高校联盟没有 AOI 字段，只有 27 字段
                    */
                    // 构造 UII
                    if (string.IsNullOrEmpty(oi))
                        return pii;
                    else
                        return oi + "." + pii;
                }
            }
        }

#endif

        // 返回 null/"blank"/"gb"/"gxlm"，
        // 分别表示 "无法判断"/"空白超高频标签"/"国标"/"高校联盟"
        public static string GetUhfProtocol(TagInfo taginfo)
        {
            if (taginfo == null)
                return null;
            if (taginfo.Protocol == InventoryInfo.ISO18000P6C)
            {
                var epc_bank = Element.FromHexString(taginfo.UID);
                if (UhfUtility.IsBlankTag(epc_bank, taginfo.Bytes) == true)
                    return "blank"; // 空白超高频标签内容
                var isGB = UhfUtility.IsISO285604Format(epc_bank, taginfo.Bytes);
                if (isGB)
                    return "gb";
                else
                    return "gxlm";
            }
            return null;    // 表示不是超高频标签
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

        // 当 EAS 修改后，主动修改 this.Books 中的相关信息
        // parameters:
        //      old_uid 原先的 UID
        //      changed_uid 修改后的 UID (对于 UHF 标签)
        //                  "on" "off" 之一(对于 HF 标签)
        // return:
        //      返回修改后的 OneTag 对象
        public static OneTag ChangeUID(
            BaseChannel<IRfid> channel,
            string old_uid,
            string changed_uid)
        {
            if (string.IsNullOrEmpty(old_uid))
                throw new ArgumentException("old_uid 参数值不应为空");

            List<OneTag> tags = new List<OneTag>();
            var news = Books;
            // int count = 0;
            foreach (TagAndData data in news)
            {
                OneTag tag = data.OneTag;
                if (tag == null)
                    continue;
                //if (tag.TagInfo != null && data.Error == null)
                //    continue;
                if (tag.Protocol == InventoryInfo.ISO14443A)
                    continue;

                if (tag.UID != old_uid)
                    continue;

                if (tag.Protocol == InventoryInfo.ISO18000P6C)
                {
                    if (string.IsNullOrEmpty(changed_uid))
                        throw new ArgumentException("changed_uid 参数值不应为空(针对 UHF 标签)");

                    if (changed_uid == "on" || changed_uid == "off")
                        throw new ArgumentException($"changed_uid 参数值不应为 '{changed_uid}' (针对 UHF 标签时)(on off 只能用于 HF 标签)");

                    tag.UID = changed_uid;
                    if (tag.TagInfo != null)
                    {
                        // 脱钩
                        tag.TagInfo = tag.TagInfo.Clone();

                        // 然后再修改
                        tag.TagInfo.UID = changed_uid;
                        tag.TagInfo.EAS = GetUhfEas(changed_uid, out int afi);
                        if (afi != 0)
                            tag.TagInfo.AFI = (byte)afi;
                    }
                    // count++;

                    // !!!
                    // 2023/11/24
                    // 清掉 _tagTable 中的事项，但不清除 Books 中的 .TagInfo
                    // 这样避免下轮 _tagTable 中的事项被原 UID 关联用到
                    RfidTagList.ClearTagTable(old_uid, false);
                    tags.Add(tag);
                }
                else if (tag.Protocol == InventoryInfo.ISO15693)
                {
                    // ClearTagTable(tag.UID);

                    if (changed_uid != "on" && changed_uid != "off")
                        throw new ArgumentException($"针对 HF 标签，changed_uid 参数值应当为 'on' 'off' 之一");

                    if (tag.TagInfo != null)
                    {
                        // 脱钩
                        tag.TagInfo = tag.TagInfo.Clone();

                        var eas = changed_uid == "on" ? true : false;
                        ChangeTagInfoEas(tag.TagInfo, eas);
                    }

                    // !!!
                    // 2023/11/24
                    // 清掉 _tagTable 中的事项，但不清除 Books 中的 .TagInfo
                    // 这样避免下轮 _tagTable 中的事项被原 UID 关联用到
                    RfidTagList.ClearTagTable(old_uid, false);

#if REMOVED
                    tag.TagInfo = null;
                    // 尝试重新获取 TagInfo。
                    // TODO: 也可以优化为直接修改内存对象，而不去访问读写器重新获取标签信息

                    ClearTagTable(tag.UID);

                    // 自动重试一次
                    GetTagInfoResult gettaginfo_result = null;
                    for (int i = 0; i < 2; i++)
                    {
                        gettaginfo_result = GetTagInfo(channel, tag.ReaderName, tag.UID, tag.AntennaID, tag.Protocol);
                        if (gettaginfo_result.Value != -1)
                            break;
                    }

                    if (gettaginfo_result.Value == -1)
                    {
                        data.Error = gettaginfo_result.ErrorInfo;
                        continue;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(data.Error) == false)
                        {
                            data.Error = null;
                        }
                    }

                    TagInfo info = gettaginfo_result.TagInfo;
                    tag.TagInfo = info;
#endif

                    // count++;
                    tags.Add(tag);
                }
            } // end of foreach

            if (tags.Count == 0)
                return null;
            return tags[0];
        }

        // 2023/12/6
        // 清除 _tagTable 中缓存的信息
        public static void ClearTagTableByDatas(List<TagAndData> datas)
        {
            if (datas == null || datas.Count == 0)
                return;
            foreach (var data in datas)
            {
                ClearTagTable(data.OneTag.UID, false);
            }
        }


        static bool ChangeTagInfoEas(TagInfo tag_info,
            bool eas)
        {
            if (tag_info == null)
                return false;
            if (tag_info.EAS == eas)
                return false;

            tag_info.EAS = eas;
            // 然后修改 AFI
            if (tag_info.AFI == 0xc2 || tag_info.AFI == 0x07)
            {
                tag_info.AFI = (eas == true ? (byte)0x07 : (byte)0xc2);
            }

            return true;
        }

        // 根据 EPC 信息把 TagInfo::EAS 设置到位
        public static bool SetTagInfoEAS(TagInfo tag_info)
        {
            // 把 tag_info.EAS 设置到位
            if (tag_info.Protocol == InventoryInfo.ISO18000P6C)
            {
                try
                {
                    // UHF 和 HF 标签不一样，需要专门把 TagInfo.EAS 设置到位
                    var chip_info = RfidTagList.GetUhfChipInfo(tag_info);
                }
                catch
                {
                }
            }

            return tag_info.EAS;
        }


        // 填充 Books 和 Patrons 每个元素的 .TagInfo
        public static int FillTagInfo(BaseChannel<IRfid> channel)
        {
            var news = Books;
            news.AddRange(Patrons);
            int count = 0;
            foreach (TagAndData data in news)
            {
                OneTag tag = data.OneTag;
                if (tag == null)
                    continue;
                if (tag.TagInfo != null && data.Error == null)
                    continue;
                if (tag.Protocol == InventoryInfo.ISO14443A)
                    continue;

                // TODO: 根据 OnlyReadEPC，不要填充 ISO18000P6C 协议的 TagInfo 的 Bytes
                if (tag.TagInfo == null)
                {
                    // 自动重试一次
                    GetTagInfoResult gettaginfo_result = null;
                    for (int i = 0; i < 2; i++)
                    {
                        gettaginfo_result = GetTagInfo(channel, tag.ReaderName, tag.UID, tag.AntennaID, tag.Protocol);
                        if (gettaginfo_result.Value != -1)
                            break;
                    }

                    if (gettaginfo_result.Value == -1)
                    {
                        data.Error = gettaginfo_result.ErrorInfo;
                        continue;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(data.Error) == false)
                        {
                            data.Error = null;
                        }
                    }

                    TagInfo info = gettaginfo_result.TagInfo;
                    tag.TagInfo = info;
                    count++;
                }
            } // end of foreach
            return count;
        }

#if NO
        // (中间版本)
        // parameters:
        //      readerNameList  list中包含的内容的读卡器名(列表)。注意 list 中包含的标签，可能并不是全部读卡器的标签。对没有包含在其中的标签，本函数需要注意跳过(维持现状)，不要当作被删除处理
        // 异常：
        //      可能抛出 TagInfoException
        public static void Refresh(BaseChannel<IRfid> channel,
            string readerNameList,
            List<OneTag> list,
            delegate_notifyChanged notifyChanged,
            delegate_setError setError,
            delegate_detectType detectType = null)
        {
            try
            {
                setError?.Invoke("rfid", null);

                List<TagAndData> new_books = new List<TagAndData>();
                List<TagAndData> new_patrons = new List<TagAndData>();

                List<TagAndData> error_books = new List<TagAndData>();
                List<TagAndData> error_patrons = new List<TagAndData>();

                // 从当前列表中发现已有的图书。用于交叉运算
                List<TagAndData> found_books = new List<TagAndData>();
                // 从当前列表中发现已有的读者。用于交叉运算
                List<TagAndData> found_patrons = new List<TagAndData>();

                // 即便是发现已经存在 UID 的标签，也要再判断一下 Antenna 是否不同。如果有不同，要进行变化通知
                // 从当前列表中发现(除了 UID) 内容有变化的图书。这些图书也会进入 found_books 集合
                List<TagAndData> changed_books = new List<TagAndData>();

                foreach (OneTag tag in list)
                {
                    // 检查以前的列表中是否已经有了
                    var book = FindBookTag(tag.UID);
                    if (book != null)
                    {
                        found_books.Add(book);
                        if (book.OneTag.AntennaID != tag.AntennaID)
                        {
                            // 修改 AntennaID
                            book.OneTag.AntennaID = tag.AntennaID;
                            changed_books.Add(book);
                        }

                        if (string.IsNullOrEmpty(book.Error) == false)
                            error_books.Add(book);
                        continue;
                    }
                    var patron = FindPatronTag(tag.UID);
                    if (patron != null)
                    {
                        found_patrons.Add(patron);
                        if (string.IsNullOrEmpty(patron.Error) == false)
                            error_patrons.Add(patron);
                        continue;
                    }

                    // 原来的行为
                    if (detectType == null)
                    {
                        // ISO14443A 的一律当作读者证卡
                        if (tag.Protocol == InventoryInfo.ISO14443A)
                        {
                            patron = new TagAndData { OneTag = tag, Type = "patron" };
                            new_patrons.Add(patron);
                        }
                        else
                        {
                            // 根据缓存的 typeOfUsage 来判断
                            string typeOfUsage = (string)_typeTable[tag.UID];
                            if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                            {
                                patron = new TagAndData { OneTag = tag, Type = "patron" };
                                new_patrons.Add(patron);
                                found_books.Remove(patron);
                            }
                            else
                            {
                                // ISO15693 的则先添加到 _books 中。等类型判断完成，有可能还要调整到 _patrons 中
                                book = new TagAndData { OneTag = tag };
                                new_books.Add(book);
                            }
                        }
                    }
                    else
                    {
                        // *** 按照定制函数来进行类别分析
                        string type = detectType(tag);
                        if (type == "patron")
                        {
                            var new_data = new TagAndData
                            {
                                OneTag = tag,
                                Type = "patron"
                            };
                            new_patrons.Add(new_data);
                            // TODO: 按照 UID 移走
                            found_books.Remove(new_data);
                        }
                        else
                        {
                            // "book" or ""
                            var new_data = new TagAndData
                            {
                                OneTag = tag,
                                Type = ""
                            };
                            new_books.Add(new_data);
                            // TODO: 按照 UID 移走
                            found_patrons.Remove(new_data);
                        }
                    }
                }

                List<TagAndData> remove_books = new List<TagAndData>();
                List<TagAndData> remove_patrons = new List<TagAndData>();

                // 交叉运算
                // 注意对那些在 readerNameList 以外的标签不要当作 removed 处理
                foreach (TagAndData book in _books)
                {
                    if (InRange(book, readerNameList) == false)
                        continue;
                    if (found_books.IndexOf(book) == -1)
                        remove_books.Add(book);
                }

                foreach (TagAndData patron in _patrons)
                {
                    if (InRange(patron, readerNameList) == false)
                        continue;

                    if (found_patrons.IndexOf(patron) == -1)
                        remove_patrons.Add(patron);
                }

                bool array_changed = false;

                // 兑现添加
                lock (_sync_books)
                {
                    foreach (TagAndData book in new_books)
                    {
                        if (_books.IndexOf(book) == -1)
                        {
                            _books.Add(book);
                            array_changed = true;
                        }
                    }
                    // 兑现删除
                    foreach (TagAndData book in remove_books)
                    {
                        _books.Remove(book);
                        array_changed = true;
                    }
                }

                lock (_sync_patrons)
                {
                    foreach (TagAndData patron in new_patrons)
                    {
                        if (_patrons.IndexOf(patron) == -1)
                        {
                            _patrons.Add(patron);
                            array_changed = true;
                        }
                    }
                    foreach (TagAndData patron in remove_patrons)
                    {
                        _patrons.Remove(patron);
                        array_changed = true;
                    }
                }

                // 通知一次变化
                if (array_changed
                    || changed_books.Count > 0
                    || new_books.Count > 0 || remove_books.Count > 0
                    || new_patrons.Count > 0 || remove_patrons.Count > 0)    // 2019/8/15 优化
                    notifyChanged(new_books, changed_books, remove_books,
                        new_patrons, null, remove_patrons);

                // 需要获得 Tag 详细信息的。注意还应当包含以前出错的 Tag
                //List<TagAndData> news = new_books;
                // news.AddRange(error_books);

                List<TagAndData> news = new List<TagAndData>();
                news.AddRange(_books);
                news.AddRange(new_patrons);

                new_books = new List<TagAndData>();
                remove_books = new List<TagAndData>();
                new_patrons = new List<TagAndData>();
                remove_patrons = new List<TagAndData>();

                // .TagInfo 是否发生过填充
                bool taginfo_changed = false;
                {
                    List<TagAndData> update_books = new List<TagAndData>();
                    List<TagAndData> update_patrons = new List<TagAndData>();

                    // 逐个获得新发现的 ISO15693 标签的详细数据，用于判断图书/读者类型
                    foreach (TagAndData data in news)
                    {
                        OneTag tag = data.OneTag;
                        if (tag == null)
                            continue;
                        if (tag.TagInfo != null && data.Error == null)
                            continue;
                        if (tag.Protocol == InventoryInfo.ISO14443A)
                            continue;
                        {
                            /*
                            // TODO
                            GetTagInfoResult gettaginfo_result = null;
                            if (tag.InventoryInfo == null)
                                gettaginfo_result = GetTagInfo(channel, tag.UID);
                            else
                                gettaginfo_result = GetTagInfo(channel, tag.InventoryInfo);
                                */
                            // 自动重试一次
                            GetTagInfoResult gettaginfo_result = null;
                            for (int i = 0; i < 2; i++)
                            {
                                gettaginfo_result = GetTagInfo(channel, tag.ReaderName, tag.UID, tag.AntennaID);
                                if (gettaginfo_result.Value != -1)
                                    break;
                            }

                            if (gettaginfo_result.Value == -1)
                            {
                                setError?.Invoke("rfid", gettaginfo_result.ErrorInfo);
                                // TODO: 是否直接在标签上显示错误文字?
                                data.Error = gettaginfo_result.ErrorInfo;
                                if (data.Type == "patron")
                                    update_patrons.Add(data);
                                else
                                    update_books.Add(data);
                                continue;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(data.Error) == false)
                                {
                                    data.Error = null;
                                    if (data.Type == "patron")
                                        update_patrons.Add(data);
                                    else
                                        update_books.Add(data);
                                }
                            }

                            TagInfo info = gettaginfo_result.TagInfo;

                            // 记下来。避免以后重复再次去获取了
                            if (tag.TagInfo == null)
                            {
                                tag.TagInfo = info;
                                taginfo_changed = true;

                                // 2019/8/25
                                if (data.Type == "patron")
                                {
                                    if (update_patrons.IndexOf(data) == -1)
                                        update_patrons.Add(data);
                                }
                                else
                                {
                                    /*
                                    if (update_books.IndexOf(data) == -1)
                                        update_books.Add(data);
                                        */
                                }
                            }

                            // 对于不确定类型的标签(data.Type 为空的)，再次确定类型以便分离 ISO15693 图书标签和读者卡标签
                            if (string.IsNullOrEmpty(data.Type))
                            {
                                // 原来的行为
                                if (detectType == null)
                                {
                                    LogicChip chip = null;
                                    string typeOfUsage = "";
                                    try
                                    {
                                        // 观察 typeOfUsage 元素
                                        // Exception:
                                        //      可能会抛出异常 ArgumentException TagDataException
                                        chip = LogicChip.From(info.Bytes,
                    (int)info.BlockSize,
                    "");
                                        typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                                    }
                                    catch (TagDataException ex)
                                    {
                                        // throw new TagInfoException(ex.Message, info);

                                        // 解析错误的标签，当作图书标签处理
                                        typeOfUsage = "";
                                    }


                                    if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                                    {
                                        // 需要调整到 _patrons 中
                                        data.Type = "patron";
                                        // 删除列表? 同时添加列表
                                        remove_books.Add(data);
                                        update_books.Remove(data);  // 容错
                                        new_patrons.Add(data);
                                    }
                                    else
                                    {
                                        data.Type = "book";
                                        update_books.Add(data);
                                        update_patrons.Remove(data);
                                    }

                                    // 保存到缓存
                                    if (typeOfUsage != null)
                                    {
                                        if (_typeTable.Count > 1000)
                                            _typeTable.Clear();
                                        _typeTable[data.OneTag.UID] = typeOfUsage;
                                    }
                                }
                                else
                                {
                                    // *** 用定制的函数判断
                                    string new_type = detectType(tag);

                                    // 重新分离 ISO15693 图书标签和读者卡标签
                                    if (data.Type != new_type)
                                    {
                                        if (new_type == "patron")
                                        {
                                            // book --> patron
                                            // 需要调整到 _patrons 中
                                            data.Type = "patron";
                                            // 删除列表? 同时添加列表
                                            remove_books.Add(data);
                                            update_books.Remove(data);  // 容错
                                            //new_patrons.Add(data);
                                            update_patrons.Add(data);
                                        }

                                        if (new_type == "book")
                                        {
                                            // patron --> book
                                            data.Type = "book";
                                            /*
                                            update_books.Add(data);
                                            update_patrons.Remove(data);
                                            */
                                            remove_patrons.Add(data);
                                            update_patrons.Remove(data);
                                            update_books.Add(data);
                                        }

                                    }
                                }
                            }

                        }
                    } // end of foreach

                    array_changed = false;
                    // 再次兑现添加和删除
                    // 兑现添加
                    lock (_sync_books)
                    {
                        foreach (TagAndData book in new_books)
                        {
                            _books.Add(book);
                            array_changed = true;
                        }
                        // 兑现删除
                        foreach (TagAndData book in remove_books)
                        {
                            _books.Remove(book);
                            array_changed = true;
                        }
                    }

                    lock (_sync_patrons)
                    {
                        foreach (TagAndData patron in new_patrons)
                        {
                            _patrons.Add(patron);
                            array_changed = true;
                        }
                        foreach (TagAndData patron in remove_patrons)
                        {
                            _patrons.Remove(patron);
                            array_changed = true;
                        }
                    }

                    // 再次通知变化
                    if (array_changed == true
                        || taginfo_changed == true
                        || new_books.Count > 0
                        || update_books.Count > 0
                        || remove_books.Count > 0
                        || new_patrons.Count > 0
                        || update_patrons.Count > 0
                        || remove_patrons.Count > 0)    // 2019/8/15 优化
                        notifyChanged(new_books, update_books, remove_books,
                            new_patrons, update_patrons, remove_patrons);
                }
            }
            finally
            {
                _dataReady = true;
            }
        }

#endif

        static bool _useTagTable = true;

        /// <summary>
        /// 是否要启用 _tagTable ?
        /// 如果不启用，则每次都是直接从读写器 GetTagInfo() 获得标签详细信息
        /// </summary>
        public static bool UseTagTable
        {
            get
            {
                return _useTagTable;
            }
            set
            {
                _useTagTable = value;
                _tagTable.Clear();
            }
        }


        // uid --> TagInfo
        static Hashtable _tagTable = new Hashtable();

        // parameters:
        //      clear_tag_info  是否也要连带清除 Books 集合中的 .TagInfo 为 null?

        public static void ClearTagTable(string uid,
            bool clear_tag_info = true)
        {
            if (string.IsNullOrEmpty(uid))
            {
                _tagTable.Clear();
                // 要把 books 集合中相关 uid 的 TagInfo 设置为 null，迫使后面重新从 RfidCenter 获取
                if (clear_tag_info)
                    ClearTagInfo(null);
            }
            else
            {
                _tagTable.Remove(uid);
                // 要把 books 集合中相关 uid 的 TagInfo 设置为 null，迫使后面重新从 RfidCenter 获取
                if (clear_tag_info)
                    ClearTagInfo(uid);
            }
        }

        public static void SetTagInfoEAS(TagInfo tagInfo, bool enable)
        {
            tagInfo.AFI = enable ? (byte)0x07 : (byte)0xc2;
            tagInfo.EAS = enable;
        }

        // 修改和 EAS 有关的内存数据
        public static bool SetEasData(string uid, bool enable)
        {
            _tagTable.Remove(uid);

            // 找到对应事项，修改 EAS 和 AFI
            var data = FindBookTag(uid);
            if (data == null)
                return false;
            if (data.OneTag != null && data.OneTag.TagInfo != null)
            {
                SetTagInfoEAS(data.OneTag.TagInfo, enable);
                return true;
            }
            return false;
        }

        public static int TagTableCount
        {
            get
            {
                return _tagTable.Count;
            }
        }

        // 从缓存中获取标签信息
        static GetTagInfoResult GetTagInfo(BaseChannel<IRfid> channel,
            string reader_name,
            string uid,
            uint antenna,
            string protocol)
        {
            // 2019/5/21
            if (channel.Started == false)
                return new GetTagInfoResult
                {
                    Value = -1,
                    ErrorInfo = "RFID 通道尚未启动"
                };

            // testing
            // Thread.Sleep(1000);

            TagInfo info = null;

            if (_useTagTable)
                info = (TagInfo)_tagTable[uid];

            // 2020/10/17
            // 检查 reader_name 和 antenna
            /*
            if (info != null)
            {
                if (info.ReaderName != reader_name || info.AntennaID != antenna)
                {
                    info = null;
                    _tagTable.Remove(uid);
                }
            }
            */

            // 直接利用原先的 TagInfo，把 readername 和 antennaid 改成所需要的就可以了
            if (info != null)
            {
                if (info.UID != uid)
                {
                    // 2023/11/24
                    // 发现 key 和 info.UID 扭曲的缓存事项，丢弃
                    info = null;
                    _tagTable.Remove(uid);
                }
                else if (info.Protocol != protocol)
                {
                    info = null;
                    _tagTable.Remove(uid);
                }
                else if (info.ReaderName != reader_name || info.AntennaID != antenna)
                {
                    info.ReaderName = reader_name;
                    info.AntennaID = antenna;
                }
            }

            if (info == null)
            {
                // 注: 对于 UHF 标签，Driver 程序已经做到了从 UID 中解析出 EPC 中的 PC.UMI，这样如果标签本来就没有 User Bank 内容，则只需要用 UID 构造出 TagInfo 对象即可，不必从读写器去真的请求读标签了
                var result = channel.Object.GetTagInfo(reader_name, uid, antenna);
                if (result.Value == -1)
                    return result;
                info = result.TagInfo;
                if (info != null && _useTagTable)
                {
                    if (_tagTable.Count > 1000)
                        _tagTable.Clear();
                    _tagTable[uid] = info;
                }
            }

            return new GetTagInfoResult { TagInfo = info };
        }

#if NOT_USE
        // 2019/9/25
        // 从缓存中获取标签信息
        static GetTagInfoResult GetTagInfo(BaseChannel<IRfid> channel,
            InventoryInfo inventory_info)
        {
            // 2019/5/21
            if (channel.Started == false)
                return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };

            TagInfo info = (TagInfo)_tagTable[inventory_info.UID];
            if (info == null)
            {
                var result = channel.Object.GetTagInfo("*", inventory_info);
                if (result.Value == -1)
                    return result;
                info = result.TagInfo;
                if (info != null)
                {
                    if (_tagTable.Count > 1000)
                        _tagTable.Clear();
                    _tagTable[inventory_info.UID] = info;
                }
            }

            return new GetTagInfoResult { TagInfo = info };
        }

#endif

        #region 探测因为 EAS 变化引起的 EPC 变化

        public class DataChange
        {
            public TagAndData OldData { get; set; }
            public TagAndData NewData { get; set; }
        }

        // 识别 EPC 变动
        public static List<DataChange> DetectEpcChange(ref List<TagAndData> add_datas,
            ref List<TagAndData> remove_datas)
        {
            var list_add = MakeList(add_datas);
            var list_remove = MakeList(remove_datas);
            if (list_add.Count == 0 || list_remove.Count == 0)
                return new List<DataChange>();

            var updates = new List<DataChange>();
            foreach (var item_add in list_add)
            {
                foreach (var item_remove in list_remove)
                {
                    if (WildIsEqual(item_add.UII, item_remove.UII))
                    {
                        add_datas.Remove(item_add.Data);
                        remove_datas.Remove(item_remove.Data);
                        updates.Add(new DataChange
                        {
                            OldData = item_remove.Data,
                            NewData = item_add.Data,
                        });
                    }
                }
            }

            return updates;

            // TODO: 还可把 OI 和 PII 分离以后单独比较
            bool WildIsEqual(string uii1, string uii2)
            {
                if (uii1 == null)
                    uii1 = "";
                if (uii2 == null)
                    uii2 = "";
                if (uii1 == uii2)
                    return true;
                var dot1 = uii1.Contains(".");
                var dot2 = uii2.Contains(".");
                if (dot1 == dot2)
                    return uii1 == uii2;
                if (dot1 == true)
                {
                    return uii1 == uii2 || uii1.EndsWith("." + uii2);
                }

                if (dot2 == true)
                {
                    return uii1 == uii2 || uii2.EndsWith("." + uii1);
                }

                return false;
            }

            List<UiiAndTag> MakeList(List<TagAndData> datas)
            {
                var list = new List<UiiAndTag>();
                foreach (var data in datas)
                {
                    if (data.OneTag.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        var uii = RfidTagList.GetUhfUii(data.OneTag.UID, data.OneTag.TagInfo?.Bytes);
                        list.Add(new UiiAndTag
                        {
                            UII = uii,
                            Data = data
                        });
                    }
                }
                return list;
            }
        }
        class UiiAndTag
        {
            public string UII { get; set; }
            public TagAndData Data { get; set; }
        }

        #endregion
    }

    public class TagAndData
    {
        public string Type { get; set; }    // patron/book/(null) 其中 (null) 表示无法判断类型

        public OneTag OneTag { get; set; }

        // public LogicChip LogicChip { get; set; }

        public string Error { get; set; }   // 错误信息

        public override string ToString()
        {
            return $"Type={Type},OneTag=[{OneTag.GetDescription()}],Error={Error}";
        }
    }

    // 2020/4/9
    // RFID 标签信息异常
    [Serializable]
    public class TagInfoException : Exception
    {
        public TagInfo TagInfo { get; set; }
        public TagInfoException(string s, TagInfo tagInfo)
            : base(s)
        {
            TagInfo = tagInfo;
        }
    }
}
