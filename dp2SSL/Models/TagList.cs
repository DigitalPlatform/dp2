using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.RFID;

namespace dp2SSL.Models
{
    public class TagAndData
    {
        public string Type { get; set; }    // patron/book/(null) 其中 (null) 表示无法判断类型

        public OneTag OneTag { get; set; }

        public LogicChip LogicChip { get; set; }

        public string Error { get; set; }   // 错误信息
    }

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

        public static void Refresh(BaseChannel<IRfid> channel,
            List<OneTag> list,
            delegate_notifyChanged notifyChanged,
            delegate_setError setError)
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

            foreach (OneTag tag in list)
            {
                // 检查以前的列表中是否已经有了
                var book = FindBookTag(tag.UID);
                if (book != null)
                {
                    found_books.Add(book);

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
                    // ISO15693 的则先添加到 _books 中。等类型判断完成，有可能还要调整到 _patrons 中
                    book = new TagAndData { OneTag = tag };
                    new_books.Add(book);
                }
            }

            List<TagAndData> remove_books = new List<TagAndData>();
            List<TagAndData> remove_patrons = new List<TagAndData>();

            // 交叉运算
            foreach (TagAndData book in _books)
            {
                if (found_books.IndexOf(book) == -1)
                    remove_books.Add(book);
            }

            foreach (TagAndData patron in _patrons)
            {
                if (found_patrons.IndexOf(patron) == -1)
                    remove_patrons.Add(patron);
            }

            // 兑现添加
            lock (_sync_books)
            {
                foreach (TagAndData book in new_books)
                {
                    _books.Add(book);
                }
                // 兑现删除
                foreach (TagAndData book in remove_books)
                {
                    _books.Remove(book);
                }
            }

            lock (_sync_patrons)
            {
                foreach (TagAndData patron in new_patrons)
                {
                    _patrons.Add(patron);
                }
                foreach (TagAndData patron in remove_patrons)
                {
                    _patrons.Remove(patron);
                }
            }

            // 通知一次变化
            notifyChanged(new_books, null, remove_books,
                new_patrons, null, remove_patrons);

            // 需要获得 Tag 详细信息的。注意还应当包含以前出错的 Tag
            //List<TagAndData> news = new_books;
            // news.AddRange(error_books);

            List<TagAndData> news = new List<TagAndData>();
            news.AddRange(_books);

            new_books = new List<TagAndData>();
            remove_books = new List<TagAndData>();
            new_patrons = new List<TagAndData>();
            remove_patrons = new List<TagAndData>();

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
                    {
                        var gettaginfo_result = GetTagInfo(channel, tag.UID);
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
                            tag.TagInfo = info;

                        // 观察 typeOfUsage 元素
                        var chip = LogicChip.From(info.Bytes,
    (int)info.BlockSize,
    "");

                        // 分离 ISO15693 图书标签和读者卡标签
                        string typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
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
                    }
                } // end of foreach

                // 再次兑现添加和删除
                // 兑现添加
                lock (_sync_books)
                {
                    foreach (TagAndData book in new_books)
                    {
                        _books.Add(book);
                    }
                    // 兑现删除
                    foreach (TagAndData book in remove_books)
                    {
                        _books.Remove(book);
                    }
                }

                lock (_sync_patrons)
                {
                    foreach (TagAndData patron in new_patrons)
                    {
                        _patrons.Add(patron);
                    }
                    foreach (TagAndData patron in remove_patrons)
                    {
                        _patrons.Remove(patron);
                    }
                }

                // 再次通知变化
                notifyChanged(new_books, update_books, remove_books,
                    new_patrons, update_patrons, remove_patrons);
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
            string uid)
        {
            // 2019/5/21
            if (channel.Started == false)
                return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };

            TagInfo info = (TagInfo)_tagTable[uid];
            if (info == null)
            {
                var result = channel.Object.GetTagInfo("*", uid);
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
    }
}
