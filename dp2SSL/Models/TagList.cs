using DigitalPlatform.RFID;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL.Models
{
    public class TagAndData
    {
        public string Type { get; set; }    // patron/book/(null) 其中 (null) 表示无法判断类型

        public OneTag OneTag { get; set; }

        public LogicChip LogicChip { get; set; }
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

        static TagAndData FindBookTag(string uid)
        {
            foreach (TagAndData tag in _books)
            {
                if (tag.OneTag.UID == uid)
                    return tag;
            }
            return null;
        }

        static TagAndData FindPatronTag(string uid)
        {
            foreach (TagAndData tag in _patrons)
            {
                if (tag.OneTag.UID == uid)
                    return tag;
            }
            return null;
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
            List<TagAndData> new_books = new List<TagAndData>();
            List<TagAndData> new_patrons = new List<TagAndData>();

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
                    continue;
                }
                var patron = FindPatronTag(tag.UID);
                if (patron != null)
                {
                    found_patrons.Add(patron);
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
                    _books.Add(patron);
                }
                foreach (TagAndData patron in remove_patrons)
                {
                    _patrons.Remove(patron);
                }
            }

            // 通知一次变化
            notifyChanged(new_books, null, remove_books,
                new_patrons, null, remove_patrons);

            new_books = new List<TagAndData>();
            remove_books = new List<TagAndData>();
            new_patrons = new List<TagAndData>();
            remove_patrons = new List<TagAndData>();

            {
                List<TagAndData> update_books = new List<TagAndData>();
                List<TagAndData> update_patrons = new List<TagAndData>();

                // 逐个获得新发现的 ISO15693 标签的详细数据，用于判断图书/读者类型
                foreach (TagAndData data in new_books)
                {
                    OneTag tag = data.OneTag;
                    {
                        var gettaginfo_result = GetTagInfo(channel, tag.UID);
                        if (gettaginfo_result.Value == -1)
                        {
                            setError?.Invoke("current", gettaginfo_result.ErrorInfo);
                            // TODO: 是否直接在标签上显示错误文字?
                            continue;
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
                            new_patrons.Add(data);
                        }
                        else
                        {
                            data.Type = "book";
                            update_books.Add(data);
                        }
                    }

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
                            _books.Add(patron);
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
        }

        // uid --> TagInfo
        static Hashtable _tagTable = new Hashtable();

        public static void ClearTagTable(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                _tagTable.Clear();
            else
                _tagTable.Remove(uid);
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
