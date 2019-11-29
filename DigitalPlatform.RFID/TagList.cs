using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{

    // 存储 Tag 的数据结构。可以动态表现当前读卡器上的所有标签
    public static class TagList
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

        // TODO: 维持一个 UID --> typeOfUsage 的对照表，加快对图书和读者类型标签的分离判断过程
        // UID --> typeOfUsage string
        static Hashtable _typeTable = new Hashtable();

        static bool InRange(TagAndData data, string readerNameList)
        {
            // 匹配读卡器名字
            return Reader.MatchReaderName(readerNameList, data.OneTag.ReaderName, out string antenna_list);
        }

        // parameters:
        //      readerNameList  list中包含的内容的读卡器名(列表)。注意 list 中包含的标签，可能并不是全部读卡器的标签。对没有包含在其中的标签，本函数需要注意跳过(维持现状)，不要当作被删除处理
        public static void Refresh(BaseChannel<IRfid> channel,
            string readerNameList,
            List<OneTag> list,
            delegate_notifyChanged notifyChanged,
            delegate_setError setError)
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

                            // 观察 typeOfUsage 元素
                            // Exception:
                            //      可能会抛出异常 ArgumentException TagDataException
                            var chip = LogicChip.From(info.Bytes,
        (int)info.BlockSize,
        "");

                            string typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;

                            // 分离 ISO15693 图书标签和读者卡标签
                            if (string.IsNullOrEmpty(data.Type))
                            {
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

        // uid --> TagInfo
        static Hashtable _tagTable = new Hashtable();

        public static void ClearTagTable(string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                _tagTable.Clear();
                // T要把 books 集合中相关 uid 的 TagInfo 设置为 null，迫使后面重新从 RfidCenter 获取
                ClearTagInfo(null);
            }
            else
            {
                _tagTable.Remove(uid);
                // 要把 books 集合中相关 uid 的 TagInfo 设置为 null，迫使后面重新从 RfidCenter 获取
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
            uint antenna)
        {
            // 2019/5/21
            if (channel.Started == false)
                return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };

            TagInfo info = (TagInfo)_tagTable[uid];
            if (info == null)
            {
                var result = channel.Object.GetTagInfo(reader_name, uid, antenna);
                if (result.Value == -1)
                    return result;
                info = result.TagInfo;
                if (info != null)
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

}
