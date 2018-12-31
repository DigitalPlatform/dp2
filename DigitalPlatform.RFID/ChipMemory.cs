using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // 创建一个元素
        public Element NewElement(ElementOID oid, string content)
        {
            // 查重
            {
                foreach (Element element in this._elements)
                {
                    if (element.OID == oid)
                        throw new Exception($"名为 {Element.GetOidName(oid)}, OID 为 {element.OID} 的元素已经存在，无法重复创建");
                }
            }

            {
                Element element = new Element(-1)
                {
                    OID = oid,
                    Text = content,
                };

                _elements.Add(element);
                // 注：此处不对 elements 排序。最后需要的时候(比如组装的阶段)再排序
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
        public static LogicChip From(byte[] data)
        {
            LogicChip chip = new LogicChip();
            chip.Parse(data);
            return chip;
        }

        // 解析 data 内容，初始化本对象
        public void Parse(byte[] data)
        {
            this._isNew = false;
            this._elements.Clear();

            int start = 0;
            while (start < data.Length)
            {
                if (data[start] == 0)
                    break;  // 有时候用 0 填充了余下的全部 bytes
                Element element = Element.Parse(data, start, out int bytes);
                this._elements.Add(element);
                start += bytes;
            }

            this._changed = false;
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
        public void Sort(int total_bytes, int block_size)
        {
            SetContentParameter();
            if (this.IsNew == true)
            {
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

                    // 最后比较 OID。OID 值小的靠前
                    delta = a.OID - b.OID;
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

                    CompactionScheme compact_method = CompactionScheme.Null;
                    if (element.OID == ElementOID.ContentParameter)
                        compact_method = CompactionScheme.OctectString;
                    else if (element.OID == ElementOID.OwnerInstitution
                        || element.OID == ElementOID.IllBorrowingInstitution)
                        compact_method = CompactionScheme.ISIL;
                    element.OriginData = Element.Compact((int)element.OID,
                        element.Text,
                        compact_method,
                        false);
                    //if (start != element.StartOffs)
                    //    throw new Exception($"element {element.ToString()} 的 StartOffs {element.StartOffs} 不符合预期值 {start}");

                    // 如果是最后一个 element(并且它是 WillLock 类型)
                    // 需要把尾部对齐 block 边界
                    if (i == this._elements.Count - 1 && element.WillLock)
                    {
                        int result = AdjustCount(block_size, element.OriginData.Length);
                        int delta = result - element.OriginData.Length;

                        if (delta != 0)
                            element.OriginData = Element.AdjustPaddingBytes(element.OriginData, delta);
                    }

                    start += element.OriginData.Length;
                    prev_type = current_type;
                    prev_element = element;
                    i++;
                }

            }
            else
            {
                // 改写状态。Locked 的元素不能动。只能在余下空挡位置，见缝插针组合排列非 Lock 元素
                // 组合算法是：每当得到一个区间尺寸，就去查询余下的元素中尺寸组合起来最适合的那些元素
                // 可以用一种利用率指标来评价组合好坏。也就是余下的空挡越小，越好
                List<int> elements = GetFreeElements();
                if (elements.Count == 0)
                    return; // 没有必要进行排序

                bool bRet = GetFreeSegments(total_bytes,
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

                // 安排元素顺序
                SetElementsPos(
                    block_size,
                    layouts[0],
                    free_segments,
                    anchor_list);
            }
        }

        #region Sort() 的下级函数

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

        static void AdjustPadding(
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

            if (total < segment_volume)
            {
                int delta = segment_volume - total;
                Debug.Assert(delta > 0);
                Element element = elements[elements.Count - 1];
                // TODO: 注意调整可能导致溢出
                element.OriginData = Element.AdjustPaddingBytes(element.OriginData, delta);
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
                    return -1;

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
        static Element FindElementByCount(ref List<Element> elements, int count)
        {
            Element found = null;
            foreach (Element element in elements)
            {
                if (count < 0 && element.Locked)
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
        //      total_bytes 这是整个芯片的 byte 容量
        //      results 返回数值集合。每个元素是一个数值，表示 segment 内可用空间 bytes 数
        //      anchor_list 便于后期插入操作的锚定列表。里面是所有 locked 类型的元素，还有 null 对象。null 对象位置代表可用的空白区间
        // return:
        //      true    成功
        //      false   失败
        bool GetFreeSegments(int total_bytes,
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
                results.Add(total_bytes);   // 没有任何 locked elements。全部都是可用空间
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
            if (start < total_bytes)
            {
                results.Add(total_bytes - start);
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
            List<int[]> all = PermutationAndCombination<int>.GetPermutation(elements.ToArray());
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

        // 根据当前的所有元素，设置 Content parameter 元素内容
        public void SetContentParameter()
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

            content_parameter.Content = Compact.TrimRight(Compact.ReverseBytes(BitConverter.GetBytes(value)));
            content_parameter.Text = Element.GetHexString(content_parameter.Content);
        }

        // 打包为 byte[] 形态
        // TODO: 对于修改的情形，要避开已经 lock 的元素，对元素进行空间布局
        public byte[] GetBytes(int total_bytes, int block_size
            // bool alignment
            )
        {
            // 先对 elements 排序。确保 PII 和 Content Parameter 元素 index 在前两个
            this.Sort(total_bytes, block_size);

            int start = 0;
            List<byte> results = new List<byte>();
            foreach (Element element in this._elements)
            {
#if NO
                CompactionScheme compact_method = CompactionScheme.Null;
                if (element.OID == ElementOID.ContentParameter)
                    compact_method = CompactionScheme.OctectString;
                else if (element.OID == ElementOID.OwnerInstitution
                    || element.OID == ElementOID.IllBorrowingInstitution)
                    compact_method = CompactionScheme.ISIL;
                var bytes = Element.Compact((int)element.OID,
                    element.Text,
                    compact_method,
                    false);
#endif
                if (element.Locked)
                {
                    if (start != element.StartOffs)
                        throw new Exception($"element {element.ToString()} 的 StartOffs {element.StartOffs} 不符合预期值 {start}");
                }
                var bytes = element.OriginData;
                results.AddRange(bytes);
                start += bytes.Length;
            }

            return results.ToArray();
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

}
