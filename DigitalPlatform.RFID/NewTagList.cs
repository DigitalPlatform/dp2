using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.RFID
{
    // 新版本。可以取代老版本 TagList 类
    // 这个新版本不再区分 Book 和 Patron。把区分类型的事情留给应用程序。这样简化了本部分的程序代码，也提高了整体运行效率
    // **************
    // 存储 Tag 的数据结构。可以动态表现当前读卡器上的所有标签
    public class NewTagList
    {
        object _sync_tags = new object();
        List<TagAndData> _tags = new List<TagAndData>();

        public List<TagAndData> Tags
        {
            get
            {
                lock (_sync_tags)
                {
                    List<TagAndData> results = new List<TagAndData>();
                    results.AddRange(_tags);
                    return results;
                }
            }
        }

        public void AssertTagInfo()
        {
            int i = 0;
            foreach (var tag in Tags)
            {
                Debug.Assert(tag.OneTag.TagInfo != null, $"i={i} tag={tag.ToString()}");
                i++;
            }
        }

        // 清除 _tags 中的所有内容
        public void Clear()
        {
            lock (_sync_tags)
            {
                _tags.Clear();
            }
        }

        // 清除 _tags 中特定对象的 .TagInfo 属性
        void ClearTagInfo(string uid)
        {
            lock (_sync_tags)
            {
                foreach (TagAndData data in _tags)
                {
                    if (data.OneTag == null)
                        continue;
                    if (string.IsNullOrEmpty(uid)
                        || data.OneTag.UID == uid)
                        data.OneTag.TagInfo = null;
                }
            }
        }

#if REMOVED
        // 2020/12/14
        // 从 Tags 集合中删除一个对象。如果 uid 参数为 null，表示删除全部对象
        public void RemoveTag(string uid)
        {
            lock (_sync_tags)
            {
                var tags = new List<TagAndData>();
                foreach (TagAndData data in _tags)
                {
                    if (data.OneTag == null)
                        continue;

                    if (string.IsNullOrEmpty(uid)
                        || data.OneTag.UID == uid)
                        tags.Add(data);
                }

                foreach(var tag in tags)
                {
                    _tags.Remove(tag);
                    // TODO: 如何通知前端?
                }
            }
        }
#endif

        // 从当前在读卡器上的标签集合中查找一个标签信息
        public TagAndData FindTag(string uid)
        {
            lock (_sync_tags)
            {
                foreach (TagAndData tag in _tags)
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
            List<TagAndData> removed_books);
        public delegate void delegate_setError(string type, string error);
        public delegate GetTagInfoResult delegate_getTagInfo(string readerName, string uid, uint antennaID, string protocol);

        // TODO: 维持一个 UID --> typeOfUsage 的对照表，加快对图书和读者类型标签的分离判断过程
        // UID --> typeOfUsage string
        // static Hashtable _typeTable = new Hashtable();

        // TODO: 可以把 readerNameList 先 Parse 到一个结构来加快 match 速度
        static bool InRange(OneTag tag, string readerNameList)
        {
            // 匹配读卡器名字
            var ret = Reader.MatchReaderName(readerNameList, tag.ReaderName, out string antenna_list);
            if (ret == false)
                return false;
            var list = GetAntennaList(antenna_list);
            // 2019/12/19
            // 如果列表为空，表示不指定天线编号，所以任何 data 都是匹配命中的
            // RL8600 默认天线编号为 0；M201 默认天线编号为 1。列表为空的情况表示不指定天线编号有助于处理好这两种不同做法的读写器
            if (list.Count == 0)
                return true;
            if (list.IndexOf(tag.AntennaID) != -1)
                return true;
            return false;   // ret is bug!!!
        }

        /*
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
        */

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

        // parameters:
        //      readerNameList  list中包含的内容的读卡器名(列表)。注意 list 中包含的标签，可能并不是全部读卡器的标签。对没有包含在其中的标签，本函数需要注意跳过(维持现状)，不要当作被删除处理
        // 异常：
        //      
        public void Refresh(// BaseChannel<IRfid> channel,
            string readerNameList,
            List<OneTag> list_param,
            delegate_getTagInfo getTagInfo,
            delegate_notifyChanged notifyChanged,
            delegate_setError setError)
        {
            setError?.Invoke("rfid", null);

            // 对 list 里面的元素要按照天线号进行过滤
            // 注：这样过滤主要是为了防范 shelf.xml 对 M201 这样的没有多天线的配置了多个天线用于多个门，引起这里算法故障(在 Tags 中填充很多重复 UID 的对象)
            // 由于 RfidCenter 中 ListTags() 对于 M201 这样的读卡器没有严格过滤天线号，所以会有上述问题
            // 若加固了 RfidCenter 以后，这一段过滤代码可以省略，以便提高执行速度
            List<OneTag> list = new List<OneTag>();
            foreach (OneTag tag in list_param)
            {
                if (InRange(tag, readerNameList) == false)
                    continue;
                list.Add(tag);
            }

            List<TagAndData> new_books = new List<TagAndData>();

            List<TagAndData> error_books = new List<TagAndData>();

            // 从当前列表中发现已有的图书。用于交叉运算
            List<TagAndData> found_books = new List<TagAndData>();

            // 即便是发现已经存在 UID 的标签，也要再判断一下 Antenna 是否不同。如果有不同，要进行变化通知
            // 从当前列表中发现(除了 UID) 内容有变化的图书。这些图书也会进入 found_books 集合
            List<TagAndData> changed_books = new List<TagAndData>();

            foreach (OneTag tag in list)
            {
                // 检查以前的列表中是否已经有了
                var book = FindTag(tag.UID);

                /*
                // 2020/4/19 验证性做法：从一个读卡器变动到另一个读卡器，第一种顺序，瞬间集合里面可能会有相同 UID 的两个对象
                // 用 Test_transfer_test_1() 单元测试
                // 如果找到的对象是属于当前读卡器以外的范围，则当作没有找到处理
                if (book != null && InRange(book.OneTag, readerNameList) == false)
                    book = null;
                    */

                if (book != null)
                {
                    found_books.Add(book);
                    if (book.OneTag.AntennaID != tag.AntennaID
                        || book.OneTag.ReaderName != tag.ReaderName)
                    {
                        var onetag = book.OneTag;
                        // 修改 AntennaID
                        onetag.AntennaID = tag.AntennaID;
                        onetag.ReaderName = tag.ReaderName;
                        if (onetag.TagInfo != null)
                        {
                            /*
                            // TODO: 这里还有个做法，就是干脆把 .TagInfo 设置为 null。这会导致重新获取 TagInfo(所谓两阶段)
                            onetag.TagInfo = null;
                            ClearTagTable(onetag.UID);
                            */
                            onetag.TagInfo.AntennaID = tag.AntennaID;
                            onetag.TagInfo.ReaderName = tag.ReaderName;

                            // 只清理缓存
                            _tagTable.Remove(onetag.UID);
                        }
                        changed_books.Add(book);
                    }

                    if (string.IsNullOrEmpty(book.Error) == false)
                        error_books.Add(book);
                    continue;
                }
                // ISO15693 的则先添加到 _books 中。等类型判断完成，有可能还要调整到 _patrons 中
                book = new TagAndData { OneTag = tag };
                new_books.Add(book);

                // 2020/4/19
                // 对于新加入的标签，只清理缓存。防止以前残留的 cache 信息污染
                _tagTable.Remove(tag.UID);
            }

            List<TagAndData> remove_books = new List<TagAndData>();

            // 交叉运算
            // 注意对那些在 readerNameList 以外的标签不要当作 removed 处理
            foreach (TagAndData book in Tags/*_tags*/)
            {
                if (InRange(book.OneTag, readerNameList) == false)
                    continue;
                if (found_books.IndexOf(book) == -1)
                    remove_books.Add(book);
            }

            bool array_changed = false;

            // 兑现添加
            lock (_sync_tags)
            {
                foreach (TagAndData book in new_books)
                {
                    if (_tags.IndexOf(book) == -1)
                    {
                        _tags.Add(book);
                        array_changed = true;
                    }
                }
                // 兑现删除
                foreach (TagAndData book in remove_books)
                {
                    _tags.Remove(book);
                    array_changed = true;
                }
            }

            // 通知一次变化
            if (array_changed
                || changed_books.Count > 0
                || new_books.Count > 0 || remove_books.Count > 0)
                notifyChanged?.Invoke(new_books, changed_books, remove_books);

            List<TagAndData> news = new List<TagAndData>();
            lock (_sync_tags)
            {
                news.AddRange(_tags);
            }

            new_books = new List<TagAndData>();
            remove_books = new List<TagAndData>();

            // .TagInfo 是否发生过填充
            bool taginfo_changed = false;
            {
                List<TagAndData> update_books = new List<TagAndData>();

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

                    // 2020/12/16
                    // 根据 PC 判断是否需要读出 User Bank
                    if (tag.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // TODO: UID 字符串转换为 byte [] 速度慢。最好是专门取特定位置两个字符即可
                        var umi = UhfUtility.GetUMI(Element.FromHexString(tag.UID), 2);
                        if (umi == false)
                        {
                            var info = new TagInfo
                            {
                                // 2020/12/13
                                Protocol = tag.Protocol,
                                ReaderName = tag.ReaderName,
                                UID = tag.UID,
                                Bytes = null,
                                AntennaID = tag.AntennaID,
                            };

                            // 记下来。避免以后重复再次去获取了
                            if (tag.TagInfo == null && info != null)
                            {
                                tag.TagInfo = info;
                                taginfo_changed = true;

                                {
                                    if (update_books.IndexOf(data) == -1)
                                        update_books.Add(data);
                                }
                            }
                            continue;
                        }
                    }

                    if (getTagInfo != null)
                    {
                        // 自动重试一次
                        GetTagInfoResult gettaginfo_result = null;
                        for (int i = 0; i < 2; i++)
                        {
                            // gettaginfo_result = GetTagInfo(channel, tag.ReaderName, tag.UID, tag.AntennaID);
                            gettaginfo_result = GetTagInfo(getTagInfo, tag.ReaderName, tag.UID, tag.AntennaID, tag.Protocol);
                            if (gettaginfo_result.Value != -1)
                                break;
                        }

                        if (gettaginfo_result.Value == -1)
                        {
                            setError?.Invoke("rfid", gettaginfo_result.ErrorInfo);
                            // TODO: 是否直接在标签上显示错误文字?
                            data.Error = gettaginfo_result.ErrorInfo;
                            update_books.Add(data);
                            continue;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(data.Error) == false)
                            {
                                data.Error = null;
                                update_books.Add(data);
                            }
                        }

                        TagInfo info = gettaginfo_result.TagInfo;

                        // 记下来。避免以后重复再次去获取了
                        if (tag.TagInfo == null && info != null)
                        {
                            tag.TagInfo = info;
                            taginfo_changed = true;

                            {
                                if (update_books.IndexOf(data) == -1)
                                    update_books.Add(data);
                            }
                        }
                    }
                } // end of foreach

                array_changed = false;
                // 再次兑现添加和删除
                // 兑现添加
                lock (_sync_tags)
                {
                    foreach (TagAndData book in new_books)
                    {
                        _tags.Add(book);
                        array_changed = true;
                    }
                    // 兑现删除
                    foreach (TagAndData book in remove_books)
                    {
                        _tags.Remove(book);
                        array_changed = true;
                    }
                }

                // 再次通知变化
                if (array_changed == true
                    || taginfo_changed == true
                    || new_books.Count > 0
                    || update_books.Count > 0
                    || remove_books.Count > 0)
                    notifyChanged?.Invoke(new_books, update_books, remove_books);
            }
        }

        // 标签信息缓存
        // uid --> TagInfo
        Hashtable _tagTable = new Hashtable();

        public void ClearTagTable(string uid, bool clearTagInfo = true)
        {
            if (string.IsNullOrEmpty(uid))
            {
                _tagTable.Clear();

                if (clearTagInfo)
                {
                    // T要把 books 集合中相关 uid 的 TagInfo 设置为 null，迫使后面重新从 RfidCenter 获取
                    ClearTagInfo(null);
                }
            }
            else
            {
                _tagTable.Remove(uid);

                if (clearTagInfo)
                {
                    // 要把 books 集合中相关 uid 的 TagInfo 设置为 null，迫使后面重新从 RfidCenter 获取
                    ClearTagInfo(uid);
                }
            }
        }

        // 修改一个 tagInfo 中的 EAS 相关值
        public static void SetTagInfoEAS(TagInfo tagInfo, bool enable)
        {
            tagInfo.AFI = enable ? (byte)0x07 : (byte)0xc2;
            tagInfo.EAS = enable;
        }

        // 校验一个 tagInfo 中的 EAS 值是否符合。
        // return:
        //      false   不符合
        //      true    符合
        public static bool VerifyTagInfoEas(TagInfo tagInfo, bool enable)
        {
            var afi = enable ? (byte)0x07 : (byte)0xc2;
            if (afi != tagInfo.AFI)
                return false;
            if (tagInfo.EAS != enable)
                return false;
            return true;
        }

        // 修改和 EAS 有关的内存数据
        public bool SetEasData(string uid, bool enable)
        {
            _tagTable.Remove(uid);
            // 找到对应事项，修改 EAS 和 AFI
            var data = FindTag(uid);
            if (data == null)
                return false;
            if (data.OneTag != null && data.OneTag.TagInfo != null)
            {
                SetTagInfoEAS(data.OneTag.TagInfo, enable);
                return true;
            }
            return false;
        }

        public int TagTableCount
        {
            get
            {
                return _tagTable.Count;
            }
        }

        // 从缓存中获取标签信息
        GetTagInfoResult GetTagInfo(delegate_getTagInfo getTagInfo,
            string reader_name,
            string uid,
            uint antenna,
            string protocol)
        {
            //if (channel.Started == false)
            //    return new GetTagInfoResult { Value = -1, ErrorInfo = "RFID 通道尚未启动" };

            TagInfo info = (TagInfo)_tagTable[uid];

            // 2020/10/17
            // 检查 reader_name 和 antenna
            if (info != null)
            {
                if (info.ReaderName != reader_name || info.AntennaID != antenna)
                {
                    info = null;
                    _tagTable.Remove(uid);
                }
            }

            if (info == null)
            {
                var result = getTagInfo(reader_name, uid, antenna, protocol);
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
#endif


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

}
