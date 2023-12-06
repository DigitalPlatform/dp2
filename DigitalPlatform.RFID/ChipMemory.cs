using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace DigitalPlatform.RFID
{
#if NO
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
#endif

    /// <summary>
    /// 模拟 RFID 芯片内的逻辑结构。便于执行整体压缩，解压缩的操作
    /// </summary>
    public class LogicChip
    {
        /*
        public const string ISO15693 = "ISO15693";
        public const string ISO14443A = "ISO14443A";
        public const string ISO18000P6C = "ISO18000P6C";
        * */
        // "ISO15693" "ISO14443A" "ISO18000P6C:gb" "ISO18000P6C:gxlm"
        public string Protocol { get; set; }

        // 默认的 GB 35660 DSFID 值
        public static byte DefaultDSFID = 0x06;

        // 默认的图书 AFI 值。归架状态
        public static byte DefaultBookAFI = 0x07;
        // 默认的读者 AFI 值。“借出”状态
        public static byte DefaultPatronAFI = 0xC2;

        // 默认的图书 EAS 值。归架状态
        public static bool DefaultBookEAS = true;
        // 默认的读者 EAS 值。Disabled
        public static bool DefaultPatronEAS = false;

        // 是否为“新创建”状态？
        // 这种状态，表明数据只在内存，还没有写入芯片
        bool _isNew = true;
        public bool IsNew
        {
            get
            {
                return _isNew;
            }
        }

        // 内容是否被修改过？
        // 指 IsNew 为 false 的，也就是从芯片中读出信息修改的时候，是否在内存中被修改过某些元素并且还没有来得及保存回芯片
        bool _changed = false;
        public bool Changed
        {
            get
            {
                return _changed;
            }
        }

        List<Element> _elements = new List<Element>();
        public List<Element> Elements
        {
            get
            {
                return _elements;
            }
        }

        // 用于单元测试
        public void SetIsNew(bool isNew)
        {
            this._isNew = isNew;
        }

        // 是否为空白内容？
        public virtual bool IsBlank()
        {
            if (this.Elements.Count == 0)
                return true;
            return false;
        }

        // 2023/11/30
        // 判断一个 HF 标签是否是空白标签
        public static bool IsBlankHfTag(byte[] bytes)
        {
            if (bytes == null)
                return true;
            if (bytes.Where(o => o != 0).Any())
                return false;
            return true;
        }

        // 查找一个元素
        public Element FindElement(ElementOID oid)
        {
            foreach (Element element in this._elements)
            {
                if (element.OID == oid)
                    return element;
            }

            return null;
        }

        // 修改一个元素的文本。如果不存在这个元素，会自动创建
        public Element SetElement(ElementOID oid,
            string content,
            bool verify = true)
        {
            Element element = FindElement(oid);
            if (element == null)
                return NewElement(oid, content, verify);

            // 2020/12/13
            if (verify)
            {
                string verify_result = Element.VerifyElementText(oid, content);
                if (verify_result != null)
                    throw new Exception($"修改元素值 oid={oid},content={content} 时数据不合法: {verify_result}");
            }

            element.Text = content;
            return element;
        }

        // 创建一个元素
        public Element NewElement(ElementOID oid,
            string content,
            bool verify = true)
        {
            // 查重
            {
                foreach (Element element in this._elements)
                {
                    if (element.OID == oid)
                        throw new Exception($"名为 {Element.GetOidName(oid)}, OID 为 {element.OID} 的元素已经存在，无法重复创建");
                }
            }

            if (verify)
            {
                string verify_result = Element.VerifyElementText(oid, content);
                if (verify_result != null)
                    throw new Exception($"创建元素 oid={oid},content={content} 时数据不合法: {verify_result}");
            }

            {
                Element element = new Element(-1)
                {
                    OID = oid,
                    Text = content,
                };

                if (element.OID == ElementOID.ContentParameter)
                {
                    // ContentParameter 的位置要特殊处理
                    if (_elements.Count > 0
                        && _elements[0].OID == ElementOID.PII)
                        _elements.Insert(1, element);
                    else
                        _elements.Insert(0, element);
                }
                else
                {
                    // TODO: 尽量按照 OID 序号顺序插入
                    // 注：此处不对 elements 排序。最后需要的时候(比如组装的阶段)再排序
                    _elements.Add(element);
                }

                return element;
            }
        }

        // return:
        //      非null    表示找到，并删除
        //      null   表示没有找到指定的元素
        public Element RemoveElement(ElementOID oid)
        {
            foreach (Element element in this._elements)
            {
                if (element.OID == oid)
                {
                    if (element.Locked)
                        throw new Exception("该元素被锁定，无法删除");

                    _elements.Remove(element);
                    return element;
                }
            }

            return null;
        }

        // 根据物理数据构造 (拆包)
        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        // parameters:
        //      block_map   每个 char 表示一个 block 的锁定状态。'l' 表示锁定, '.' 表示没有锁定
        public static LogicChip From(byte[] data,
            int block_size,
            string block_map = "")
        {
            LogicChip chip = new LogicChip();
            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            chip.Parse(data, block_size, block_map);
            return chip;
        }

        // 解析 data 内容，初始化本对象
        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        public void Parse(byte[] data,
            int block_size,
            string block_map)
        {
            this._isNew = false;
            this._elements.Clear();
            this.Protocol = InventoryInfo.ISO15693;

            // 2020/12/9
            if (block_size == 0)
                throw new ArgumentException($"block_size 不应为 0");

            int start = 0;
            while (start < data.Length)
            {
                if (data[start] == 0)
                {
                    break;
                    start++;
                    continue;  // 有时候用 0 填充了余下的部分 bytes
                }
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                Element element = Element.Parse(data, start, out int bytes);
                Debug.Assert(element.StartOffs == start);
                this._elements.Add(element);

                for (int i = start / block_size; i < (start + bytes) / block_size; i++)
                {
                    if (GetBlockStatus(block_map, i) == 'l')
                        element.SetLocked(true);
                }

                // 检查整个 element 占用的每个 block 的 Locked 状态是否一致
                for (int i = start / block_size; i < (start + bytes) / block_size; i++)
                {
                    char ch = GetBlockStatus(block_map, i);
                    if (element.Locked)
                    {
                        if (ch != 'l')
                            throw new Exception($"元素 {element.ToString()} 内发现锁定状态不一致的块");
                    }
                    else
                    {
                        if (ch != '.')
                            throw new Exception($"元素 {element.ToString()} 内发现锁定状态不一致的块");
                    }
                }

                start += bytes;
            }

            this._changed = false;
        }

        // 修改 Changed
        public void SetChanged(bool changed)
        {
            this._changed = changed;
        }


        // 对元素进行排序
        // 调用前，要确保每个元素都 Compact 好了，内容放入 OriginData 中了
        // 排序原则：
        // 1) PII 在第一个;
        // 2) Content Parameter 在第二个; 
        // 2.1) 如果元素里面至少有一个锁定元素，Content Parameter 元素要对齐 block 边界(便于锁定元素锁定)
        // 3) 其余拟锁定元素聚集在一起，最后给必要的 padding
        // 4) 所有非锁定的元素聚集在一起
        // 5) 锁定的元素，和非锁定元素区域内部，可以按照 OID 号码排序
        // 上述算法有个优势，就是所有锁定元素中间不一定要 block 边界对齐，这样可以节省一点空间
        // 但存在一个小问题： Content Parameter 要占用多少 byte? 如果以后元素数量增多，(因为它后面就是锁定区域)它无法变大怎么办？
        public void Sort(int max_bytes,
            int block_size,
            bool trim_cp_right,
            string style = "")
        {
            SetContentParameter(trim_cp_right);
            if (this.IsNew == true)
            {
                // 保留原来顺序
                // element --> index
                Hashtable table = new Hashtable();
                int index = 0;
                foreach (var element in this._elements)
                {
                    table[element] = index++;
                }

                // 2020/8/3
                bool reserve_sequence = StringUtil.IsInList("reserve_sequence", style);

                this._elements.Sort((a, b) =>
                {
                    // OID 为 1 的始终靠前
                    if ((int)a.OID == 1)
                    {
                        if (a.OID == b.OID)
                            return 0;
                        return -1;
                    }
                    if ((int)b.OID == 1)
                        return 1;

                    // 比较 WillLock。拟锁定的位置靠后
                    int lock_a = a.WillLock ? 1 : 0;
                    int lock_b = b.WillLock ? 1 : 0;
                    int delta = lock_a - lock_b;
                    if (delta != 0)
                        return delta;

                    // 最后比较 OID
                    if (reserve_sequence)
                    {
                        // 维持原始顺序
                        delta = (int)table[a] - (int)table[b];
                    }
                    else
                    {
                        // OID 值小的靠前
                        delta = a.OID - b.OID;
                    }
                    return delta;
                });

                // 更新 element.OriginData
                // 对 WillLock 切换的情形，和最后一个 WillLock 元素进行 padding 调整
                Element prev_element = null;
                string prev_type = "";   // free or will_lock
                string current_type = "";
                int start = 0;
                int i = 0;
                foreach (Element element in this._elements)
                {
                    if (element.WillLock == false)
                        current_type = "free";
                    else
                        current_type = "will_lock";

                    // 如果切换了类型(类型指“普通”还是 WillLock)
                    // 需要把 prev_element 尾部对齐 block 边界
                    if (current_type != prev_type
                        && prev_element != null)
                    {
                        int result = AdjustCount(block_size, start);
                        int delta = result - start;

                        // 调整 prev_element 的 padding
                        if (delta != 0)
                            prev_element.OriginData = Element.AdjustPaddingBytes(prev_element.OriginData, delta);

                        start = result;
                    }

#if NO
                    CompactionScheme compact_method = CompactionScheme.Null;
                    if (element.OID == ElementOID.ContentParameter)
                        compact_method = CompactionScheme.OctectString;
                    else if (element.OID == ElementOID.OwnerInstitution
                        || element.OID == ElementOID.IllBorrowingInstitution)
                        compact_method = CompactionScheme.ISIL;
#endif
                    element.OriginData = Element.Compact((int)element.OID,
                        element.Text,
                        element.CompactMethod,  // CompactionScheme.Null,
                        false);
                    //if (start != element.StartOffs)
                    //    throw new Exception($"element {element.ToString()} 的 StartOffs {element.StartOffs} 不符合预期值 {start}");


                    start += element.OriginData.Length;
                    prev_type = current_type;
                    prev_element = element;
                    i++;
                }

                // 如果是最后一个 element(并且它是 WillLock 类型)
                // 需要把尾部对齐 block 边界
                if (prev_element != null && prev_element.WillLock)
                {
                    int result = AdjustCount(block_size, start);
                    int delta = result - start;

                    // 调整 prev_element 的 padding
                    if (delta != 0)
                        prev_element.OriginData = Element.AdjustPaddingBytes(prev_element.OriginData, delta);

                    start = result;
                }
            }
            else
            {
                foreach (Element element in this._elements)
                {
                    if (element.Locked)
                        continue;

                    element.OriginData = Element.Compact((int)element.OID,
                        element.Text,
                        element.CompactMethod,  // CompactionScheme.Null,
                        false);
                }


                // 改写状态。Locked 的元素不能动。只能在余下空挡位置，见缝插针组合排列非 Lock 元素
                // 组合算法是：每当得到一个区间尺寸，就去查询余下的元素中尺寸组合起来最适合的那些元素
                // 可以用一种利用率指标来评价组合好坏。也就是余下的空挡越小，越好
                List<int> elements = GetFreeElements();
                if (elements.Count == 0)
                    return; // 没有必要进行排序

                bool bRet = GetFreeSegments(max_bytes,
                    out List<int> free_segments,
                    out List<object> anchor_list);

                if (bRet == false)
                    throw new Exception("当前没有任何自由空间可用于存储自由字段信息");

                // 对 n 个区间进行填充尝试。把每个可能的组合，都填充试验一轮
                // parameters:
                //      areas   空闲区间的列表。每个数字表示空闲的 byte 数
                //      elements    元素尺寸列表。每个数字表示元素占用的 byte 数
                List<List<OneArea>> layouts = GetPossibleLayouts(
                    block_size,
                    free_segments,
                    elements);
                if (layouts.Count == 0)
                    throw new Exception("没有找到可用的排列方式");

                // TODO: 如何优选 layouts? 目前能想到的，是把 ContentParameter 在第一个元素以后的优选出来

                // 安排元素顺序
                SetElementsPos(
                    block_size,
                    layouts[0],
                    free_segments,
                    anchor_list);
            }
        }

        #region Sort() 的下级函数

        // TODO: 单元测试，增加检查正确性的测试函数
        // parameters:
        //      layout  自由元素的布局方式。负数表示 WillLock 类型的元素
        void SetElementsPos(
            int block_size,
            List<OneArea> free_element_layout,
            List<int> free_segments,
            List<object> anchor_list)
        {
            List<Element> results = new List<Element>();

            // 1) 先列出所有 locked 状态的 element，和 free 状态的 element
            List<Element> locked_elements = new List<Element>();
            List<Element> free_elements = new List<Element>();
            foreach (Element element in this._elements)
            {
                if (element.Locked)
                    locked_elements.Add(element);
                else
                    free_elements.Add(element);
            }

            if (locked_elements.Count == 0)
            {
            }

            locked_elements.Sort((a, b) =>
            {
                return a.StartOffs - b.StartOffs;
            });

            // 2) 根据 free_element_layout 和 anchor_list 的指引，找到元素并逐个顺序排放
            // 遇到 0 的时候，就去找下一个独立的或者位置连续的多个 locked 元素插入。注意，可能不止一个 locked 元素

            int pos = -1;
            foreach (OneArea area in free_element_layout)
            {
                // TODO: 第一个 locked 元素，StartPos == 0 的，要无条件先插入 results
                if (area.Volume == 0)
                {
                    // 输出 locked 状态的一组 element
                    results.AddRange(GetNextGroup(anchor_list, ref pos));
                    continue;
                }

                List<Element> elements = new List<Element>();
                foreach (int count in area.Layout)
                {
                    // 寻找长度为 count 的元素
                    Element element = FindElementByCount(ref free_elements, count);
                    if (element == null)
                        throw new Exception($"长度为 {count} 的元素在(自由元素)列表中没有找到");
                    elements.Add(element);
                }
                // 每一段内的 elements 单独排序
                // 先把 WillLock 状态的靠到一边，然后按照 OID 从小到大排序
                Sort(elements);

                // 填充字节。调整 WillLock 结束点在 block 边界
                // area 末尾要调整到 volume 边界
                AdjustPadding(
                    IsTail(area, free_element_layout),
    block_size,
    area.Volume,
    elements);

                results.AddRange(elements);
            }

            Debug.Assert(free_elements.Count == 0);

            // 3) elements 赋给 this
            this._elements.Clear();
            this._elements.AddRange(results);
        }

        // 观察一个 OneArea 对象是否在数组中为非 0 Volume 的最后一个
        static bool IsTail(OneArea area, List<OneArea> areas)
        {
#if NO
            for (int i = areas.IndexOf(area) + 1; i < areas.Count; i++)
            {
                OneArea current = areas[i];
                if (current.Volume == 0)
                    continue;
                return false;
            }

            return true;
#endif
            return areas.IndexOf(area) == areas.Count - 1;
        }

        static void AdjustPadding(
            bool is_tail,
            int block_size,
            int segment_volume,
            List<Element> elements)
        {
            string prev_type = "";   // free or will_lock
            string current_type = "";
            int total = 0;
            foreach (Element element in elements)
            {
                if (element.WillLock)
                {
                    current_type = "will_lock";
                }
                else
                {
                    current_type = "free";
                }

                // 如果切换了类型(类型指“普通”还是 WillLock)，需要把 count 对齐 block 边界
                if (current_type != prev_type)
                {
                    int result = AdjustCount(block_size, total);
                    if (result != total)
                    {
                        int delta = result - total;
                        Debug.Assert(delta > 0);
                        // TODO: 注意调整可能导致溢出
                        element.OriginData = Element.AdjustPaddingBytes(element.OriginData, delta);
                    }
                    total = result;
                }

                total += element.OriginData.Length;
                prev_type = current_type;
            }
            if (elements.Count == 0)
                return;

            if (is_tail == false)
            {
                if (total < segment_volume)
                {
                    int delta = segment_volume - total;
                    Debug.Assert(delta > 0);
                    Debug.Assert(elements.Count > 0);
                    Element element = elements[elements.Count - 1];
                    // TODO: 注意调整可能导致溢出
                    element.OriginData = Element.AdjustPaddingBytes(element.OriginData, delta);
                }
            }
            else
            {

                Debug.Assert(elements.Count > 0);
                Element element = elements[elements.Count - 1];
                // 注：不管是不是 WillLock，最后一个 element 都调整到 block 边界
                // if (element.WillLock)
                {
                    int result = AdjustCount(block_size, total);
                    int delta = result - total;
                    if (delta > 0)
                        element.OriginData = Element.AdjustPaddingBytes(element.OriginData, delta);
                }
            }
        }

        static void Sort(List<Element> elements)
        {
            elements.Sort((a, b) =>
            {
                // OID 为 1 的始终靠前
                if ((int)a.OID == 1)
                {
                    if (a.OID == b.OID)
                        return 0;
                    return -1;
                }
                if ((int)b.OID == 1)
                    return 1;

                // 比较 WillLock。拟锁定的位置靠后
                int lock_a = a.WillLock ? 1 : 0;
                int lock_b = b.WillLock ? 1 : 0;
                int delta = lock_a - lock_b;
                if (delta != 0)
                    return delta;

                // 最后比较 OID。OID 值小的靠前
                delta = a.OID - b.OID;
                return delta;
            });

        }

        // 在 anchor_list 中找到下一组 elements。
        // 一组就是连续的一段 Element 对象。组之间用 null 对象隔开
        static List<Element> GetNextGroup(List<object> anchor_list, ref int pos)
        {
            List<Element> results = new List<Element>();
            pos++;
            int i = pos;
            for (i = pos; i < anchor_list.Count; i++)
            {
                object o = anchor_list[i];
                if (o == null)
                    break;
                Debug.Assert(o is Element);
                results.Add(o as Element);
            }

            pos = i;
            return results;
        }

        // 通过 原始尺寸 找到一个元素。返回前会在 elements 中去掉已经找到的元素
        static Element FindElementByCount(ref List<Element> elements,
            int count)
        {
            Element found = null;
            foreach (Element element in elements)
            {
                if (count < 0 && element.WillLock)
                {
                    if (element.OriginData.Length == -count)
                    {
                        found = element;
                        break;
                    }
                }
                else
                {
                    if (element.OriginData.Length == count)
                    {
                        found = element;
                        break;
                    }
                }
            }

            if (found == null)
                return null;
            elements.Remove(found);
            return found;
        }

        // 获得所有非锁定状态的元素尺寸。注意元素没有进行 block 对齐处理，是最小尺寸
        // return:
        //      返回数值集合。每个元素是一个数值，负数表示即将锁定的元素，正数表示普通元素
        List<int> GetFreeElements()
        {
            List<int> results = new List<int>();
            foreach (Element element in this._elements)
            {
                if (element.Locked)
                    continue;

                results.Add(element.WillLock ? -1 * element.OriginData.Length : element.OriginData.Length);
            }

            return results;
        }

        // TODO: 注意空间全部被锁定元素占满的情况处理
        // 获得当前没有锁定的区段的容量列表
        // 返回的数组中，每个整数表示一个区段长度(byte数)
        // parameters:
        //      max_bytes 这是整个芯片的 byte 容量
        //      results 返回数值集合。每个元素是一个数值，表示 segment 内可用空间 bytes 数
        //      anchor_list 便于后期插入操作的锚定列表。里面是所有 locked 类型的元素，还有 null 对象。null 对象位置代表可用的空白区间
        // return:
        //      true    成功
        //      false   失败
        bool GetFreeSegments(int max_bytes,
            out List<int> results,
            out List<object> anchor_list)
        {
            results = new List<int>();
            anchor_list = new List<object>();

            // 1) 先列出所有 locked 状态的 element
            List<Element> locked_elements = new List<Element>();
            foreach (Element element in this._elements)
            {
                if (element.Locked)
                    locked_elements.Add(element);
            }

            if (locked_elements.Count == 0)
            {
                results.Add(max_bytes);   // 没有任何 locked elements。全部都是可用空间
                return true;
            }

            // 2) 然后按照 element 的 start 位置排序
            locked_elements.Sort((a, b) =>
            {
                return a.StartOffs - b.StartOffs;
            });

            // 3) 最后计算出这些 element 之间的空白段落
            int start = 0;
            Element prev_element = null;
            foreach (Element element in locked_elements)
            {
                if (prev_element != null)
                {
                    if (results.Count == 0 || results[results.Count - 1] != 0)
                        results.Add(0); // 表示 fixed 位置
                    anchor_list.Add(prev_element);
                }
                int length = element.StartOffs - start;
                if (length > 0)
                {
                    anchor_list.Add(null);  // 空白区占位
                    results.Add(length);
                    length = 0;
                }
                start = element.StartOffs + element.OriginData.Length;
                prev_element = element;
            }

            if (prev_element != null)
            {
                if (results.Count == 0 || results[results.Count - 1] != 0)
                    results.Add(0); // 表示 fixed 位置
                anchor_list.Add(prev_element);
            }
            // 最后一段空白区
            if (start < max_bytes)
            {
                results.Add(max_bytes - start);
            }

            return true;
        }

        // 对 n 个区间进行填充尝试。把每个可能的组合，都填充试验一轮
        // parameters:
        //      segments   空闲区间的列表。每个数字表示空闲的 byte 数
        //      elements    元素尺寸列表。每个数字表示元素占用的 byte 数
        static List<List<OneArea>> GetPossibleLayouts(
            int block_size,
            List<int> segments,
            List<int> elements)
        {
            List<List<OneArea>> results = new List<List<OneArea>>();
            // 列出所有排列形态
            List<int[]> all = null;

            try
            {
                all = PermutationAndCombination<int>.GetPermutation(elements.ToArray());
            }
            catch
            {
                int i = 0;
                i++;
            }

            // 逐个进行检验
            foreach (int[] possible in all)
            {
                var result = TryPlacement(block_size,
                    segments,
                    new List<int>(possible));
                if (result != null)
                    results.Add(result);
            }

            return results;
        }

        class OneArea
        {
            // 本区域的 byte 容量。用于最后调整对齐
            public int Volume { get; set; }
            // 布局顺序
            List<int> _layout = new List<int>();
            public List<int> Layout
            {
                get
                {
                    return _layout;
                }
                set
                {
                    _layout = value;
                }
            }

#if NO
            public void Clear()
            {
                Layout.Clear();
            }

            public void Add(int value)
            {
                Layout.Add(value);
            }
#endif
        }

        // 将一组 elements 安放到 segments 中。
        // parameters:
        //      volume  可以安放元素的总容量。byte 数
        // return:
        //      非 null    匹配成功。返回加工后的内容。所谓加工，就是在分割点插入一个 0 元素
        //      null   匹配失败
        static List<OneArea> TryPlacement(
            int block_size,
            List<int> segments,
            List<int> elements)
        {
            List<OneArea> results = new List<OneArea>();
            int start = 0;
            foreach (int segment in segments)
            {
                if (segment == 0)
                {
                    results.Add(new OneArea());
                    continue;
                }
                OneArea one = TryPlacementInOneSegment(
block_size,
segment,
elements,
start);
                // 每一段后面都必定有一个 0 元素。注意段可能为空(因为段内空间太小没法放下哪怕一个元素)
                results.Add(one);
                // results.Add(0);
                start += one.Layout.Count;
            }

            // 最后还有无法安放的 element
            if (start < elements.Count)
                return null;

            return results;
        }

        // 尝试在一个 segment 中排列 elements
        // parameters:
        //      segment_volumn  segment 容量。byte 数
        //      bytes   [out] 返回本次用掉的 element 个数
        // return:
        //      空集合 排列失败。连第一个 element 也放不下
        //      其他 排列成功。返回用到的 elements。注意返回的元素个数可能会小于 elements.Count
        static OneArea TryPlacementInOneSegment(
    int block_size,
    int segment_volume,
    List<int> elements,
    int start)
        {
            OneArea results = new OneArea();
            results.Volume = segment_volume;

            int total = 0;
            string prev_type = "";   // free or will_lock
            string current_type = "";
            for (int i = start; i < elements.Count; i++)
            {
                int element = elements[i];
                int count = 0;
                // 如果 count 为负数，表示这是 WillLock 的元素
                if (element > 0)
                {
                    current_type = "free";
                    count = element;
                }
                else
                {
                    current_type = "will_lock";
                    count = -element;
                }

                // 如果切换了类型(类型指“普通”还是 WillLock)，需要把 count 对齐 block 边界
                if (current_type != prev_type)
                    total = AdjustCount(block_size, total);

                if (total == segment_volume
                    || (total < segment_volume && total + count > segment_volume))
                {
                    return results;
                }
                total += count;
                results.Layout.Add(element);
                prev_type = current_type;
            }

            if (total > segment_volume)
            {
                Debug.Assert(results.Layout.Count < elements.Count);
            }

            return results;
        }

        // 把 value 调整到 block 边界对齐
        // parameters:
        //      block_size  块尺寸。块中包含的 byte 数
        static int AdjustCount(int block_size, int value)
        {
            if ((value % block_size) == 0)
                return value;
            int result = block_size * ((value / block_size) + 1);
            Debug.Assert(result >= value);
            return result;
        }

        #endregion

        // 检查是否需要使用 Content parameter 字段
        bool NeedContentParameter()
        {
            foreach (Element element in this._elements)
            {
                int oid = (int)element.OID;
                if (oid >= 3)
                    return true;
            }
            return false;
        }

        // 根据当前的所有元素，设置 Content parameter 元素内容
        // parameters:
        //      trim_right  是否要截掉右侧多余的连续 0？截掉是一般做法。而不截掉，也就是保留足够 byte 数，足以应对以后元素增多以后的局面，保证 content parameter 本身耗用的空间和以前一致，有利于芯片以后修改内容时的布局。
        public void SetContentParameter(bool trim_right)
        {
            var content_parameter = this.FindElement(ElementOID.ContentParameter);
            if (content_parameter == null)
                content_parameter = this.NewElement(ElementOID.ContentParameter, null);

            UInt64 value = 0;
            foreach (Element element in this._elements)
            {
                int oid = (int)element.OID;
                if (oid >= 3)
                {
                    // TODO: 测试 0x80000000 >> 0 会不会有问题
                    value |= 0x8000000000000000 >> (oid - 3);
                }
            }

            if (value == 0)
            {
                this.RemoveElement(ElementOID.ContentParameter);
                return;
            }

            var bytes = Compact.ReverseBytes(BitConverter.GetBytes(value));
            if (trim_right)
                bytes = Compact.TrimRight(bytes);
            content_parameter.Content = bytes;
            content_parameter.Text = Element.GetHexString(content_parameter.Content);
        }

        static char NormalMapChar = '.';

        [Flags]
        public enum GetBytesStyle
        {
            None = 0x00,
            ContentParameterFullLength = 0x01,
            ReserveSequence = 0x02, // 2020/8/3 尽量保留元素的原始顺序
        }

        public void RemoveEmptyElements()
        {
            // 删除空元素
            for (int i = 0; i < this._elements.Count; i++)
            {
                Element element = this._elements[i];
                if (string.IsNullOrEmpty(element.Text)
                    && element.OID != ElementOID.ContentParameter
                    && element.Locked == false)
                {
                    this._elements.RemoveAt(i);
                    i--;
                }
            }
        }

        // 打包为 byte[] 形态
        // 注意，本函数执行后 this._elements 内各个元素的顺序可能会发生变化
        // 对于修改的情形，要避开已经 lock 的元素，对元素进行空间布局
        // parameters:
        //      block_map   [out]块地图。用 
        //                  字符 'l' 表示原来就是锁定状态的块
        //                  字符 'w' 表示需要新锁定的块
        //                  如果为全部 "...."，和 "" 是等同的。缺省的 char 是 '.'
        public byte[] GetBytes(int max_bytes,
            int block_size,
            GetBytesStyle style,
            out string block_map)
        {
            block_map = "";

            if ((max_bytes % block_size) != 0)
                throw new ArgumentException($"max_bytes({max_bytes}) 不是 block_size({block_size}) 的整倍数");

            // 删除空元素
            RemoveEmptyElements();
#if REMOVED
            for (int i = 0; i < this._elements.Count; i++)
            {
                Element element = this._elements[i];
                if (string.IsNullOrEmpty(element.Text)
                    && element.OID != ElementOID.ContentParameter
                    && element.Locked == false)
                {
                    this._elements.RemoveAt(i);
                    i--;
                }
            }
#endif

            // 先对 elements 排序。确保 PII 和 Content Parameter 元素 index 在前两个
            this.Sort(max_bytes,
                block_size,
                (style & GetBytesStyle.ContentParameterFullLength) == 0,
                (style & GetBytesStyle.ReserveSequence) != 0 ? "reserve_sequence" : "");

            List<char> map = new List<char>();
            int start = 0;
            List<byte> results = new List<byte>();
            foreach (Element element in this._elements)
            {
                if (element.Locked)
                {
                    if (start != element.StartOffs)
                        throw new Exception($"element {element.ToString()} 的 StartOffs {element.StartOffs} 不符合预期值 {start}");
                }

                var bytes = element.OriginData;
                results.AddRange(bytes);

                // 设置 block map
                if (element.WillLock || element.Locked)
                {
                    char ch = NormalMapChar;
                    if (element.Locked)
                        ch = 'l';
                    else if (element.WillLock)
                        ch = 'w';
                    for (int i = start / block_size; i < (start + bytes.Length) / block_size; i++)
                    {
                        SetBlockStatus(ref map, i, ch);
                    }
                }

                start += bytes.Length;
            }

            if (start > max_bytes)
                throw new Exception($"实际产生的 byte 数 {start} 超过限制数 {max_bytes}");

            block_map = new string(map.ToArray());
            return results.ToArray();
        }

        static void SetBlockStatus(ref List<char> map, int index, char ch)
        {
            while (map.Count < index + 1)
            {
                map.Add(NormalMapChar);
            }
            map[index] = ch;
        }

        public static char GetBlockStatus(string map, int index)
        {
            if (index >= map.Length)
                return '.';
            return map[index];
        }

        // 2022/4/22
        // 对 byte[] 内容执行清除。锁定的块不会被清除
        public static byte[] ClearBytes(byte[] bytes,
            uint block_size,
            uint max_block_count,
            string block_map)
        {
            List<byte> results = new List<byte>();
            for (int i = 0; i < max_block_count; i++)
            {
                var lock_char = GetBlockStatus(block_map, i);
                if (lock_char == 'l')
                    results.AddRange(GetRange(bytes, (uint)i * block_size, block_size));
                else
                {
                    for (int j = 0; j < block_size; j++)
                    {
                        results.Add(0);
                    }
                }
            }

            return results.ToArray();
        }

        static List<byte> GetRange(byte[] bytes,
            uint start,
            uint length,
            byte default_value = 0)
        {
            List<byte> results = new List<byte>();
            for (uint i = start; i < start + length; i++)
            {
                byte v;
                if (i < bytes.Length)
                    v = bytes[i];
                else
                    v = default_value;
                results.Add(v);
            }

            return results;
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
            StringBuilder text = new StringBuilder();
            int i = 1;
            foreach (Element element in this._elements)
            {
                text.Append($"{i}) {element.ToString()}\r\n");
                i++;
            }
            return text.ToString();
        }

        // 2023/11/16
        // 尝试从相关元素中寻找机构代码
        public string FindGaoxiaoOI()
        {
            string oi = null;
            // 2023/11/16
            if (string.IsNullOrEmpty(oi))
                oi = this.FindElement(ElementOID.OI)?.Text;
            if (string.IsNullOrEmpty(oi))
                oi = this.FindElement(ElementOID.AOI)?.Text;
            if (string.IsNullOrEmpty(oi))
                oi = this.FindElement((ElementOID)27)?.Text;
            return oi;
        }

        public string GetUII()
        {
            string pii = this.FindElement(ElementOID.PII)?.Text;
            string oi = this.FindGaoxiaoOI();
            if (string.IsNullOrEmpty(pii) == false
                && string.IsNullOrEmpty(oi) == false)
                return oi + "." + pii;
            return pii;
        }
    }

#if NO
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

#endif
}
