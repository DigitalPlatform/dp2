using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// MarcNode 集合
    /// </summary>
    public class MarcNodeList : IEnumerable
    {
        List<MarcNode> m_list = new List<MarcNode>();

        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcNodeList 对象
        /// </summary>
        public MarcNodeList()
        {
        }

        // 用一个元素构造出数组
        /// <summary>
        /// 初始化一个 MarcNodeList 对象，并填入一个 MarcNode 对象
        /// </summary>
        /// <param name="node">要填入的 MarcNode 对象。若本参数为 null，则初始化一个空的集合对象</param>
        public MarcNodeList(MarcNode node)
        {
            // 如果使用null，可以构造出一个空的集合。这是为了方便使用，尽量不要报错
            // 例如 new MarcNodeList(node.FirstChild) 如果 FirstChild 为空可以构造出一个空的集合，如果后面利用这个集合继续操作也无害
            if (node == null)
                return;
            this.add(node);
        }

        #endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_list.GetEnumerator();
        }

#if NO
        void ForEach(Action<MarcNode> action)
        {
            this.m_list.ForEach(action);
        }
#endif
        /// <summary>
        /// 获得 List&lt;MarcNode&gt;接口
        /// </summary>
        public List<MarcNode> List
        {
            get
            {
                return this.m_list;
            }
        }

        /*
        public List<MarcNode>.Enumerator GetEnumberator()
        {
            return this.m_list.GetEnumerator();
        }
         * */
        /// <summary>
        /// 当前集合中的元素个数
        /// </summary>
        public int count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        // 照顾习惯，也提供这个名字
        /// <summary>
        /// 当前集合中的元素个数
        /// </summary>
        public int length
        {
            get
            {
                return this.m_list.Count;
            }
        }

        /// <summary>
        /// 根据下标存取集合的元素
        /// </summary>
        /// <param name="index">下标数字。从0开始计算</param>
        /// <returns>集合元素</returns>
        public MarcNode this[int index]
        {
            get
            {
                return this.m_list[index];
            }
            set
            {
                this.m_list[index] = value;
            }
        }

        /// <summary>
        /// 获得一个元素在集合中的下标
        /// </summary>
        /// <param name="node">要探测的元素</param>
        /// <returns>下标。-1表示没有找到这个元素</returns>
        public int indexOf(MarcNode node)
        {
            return this.m_list.IndexOf(node);
        }


        // 复制出一个新的集合
        // 注意，不但数组(框架)被复制，其中每个元素都重新复制
        /// <summary>
        /// 根据当前集合复制出一个新的集合对象。本方法采取的是否深度复制策略，即新集合中的每个元素都是复制品
        /// </summary>
        /// <returns>新的集合</returns>
        public MarcNodeList clone()
        {
            MarcNodeList result = new MarcNodeList();
            foreach (MarcNode node in this)
            {
                result.add(node.clone());
            }

            return result;
        }

        // 从数组中移走全部对象
        // 注意，本函数并不修改所移走的对象的Parent成员。也就是说移走的对象并未完全被detach
        // 返回移出的nodes
        /// <summary>
        /// 从当前集合中移走全部元素。注意，本函数并不修改所移走的元素的 Parent 成员。也就是说移走的元素并未完全被摘除
        /// </summary>
        /// <returns>已经被移走的全部元素</returns>
        public MarcNodeList remove()
        {
            MarcNodeList results = new MarcNodeList();

            for (int i = 0; i < this.count; i++)
            {
                MarcNode node = this[i];
                MarcNode temp = node.remove();
                if (temp != null)
                    results.add(node);
            }

            return results;
        }

        // 追加一个元素
        // 返回增补对象后的当前集合
        /// <summary>
        /// 在当前集合末尾添加一个元素
        /// </summary>
        /// <param name="node">要添加的元素</param>
        /// <returns>添加元素后的当前集合</returns>
        public MarcNodeList add(MarcNode node)
        {
            this.m_list.Add(node);
            return this;
        }

        // 追加若干元素
        // 返回增补对象后的当前集合
        /// <summary>
        /// 在当前集合末尾添加若干元素
        /// </summary>
        /// <param name="nodes">要添加的若干元素</param>
        /// <returns>添加元素后的当前集合</returns>
        public MarcNodeList add(MarcNodeList nodes)
        {
            // this.m_list.AddRange(nodes);
            foreach (MarcNode node in nodes)
            {
                this.m_list.Add(node);
            }

            return this;
        }

        // 从指定位置移走一个对象
        // 返回包含移走的对象的集合
        /// <summary>
        /// 从指定下标位置移走一个元素
        /// </summary>
        /// <param name="index">要移走对象的下标</param>
        /// <returns>被移走的一个元素(所构成的新集合)</returns>
        public MarcNodeList removeAt(int index)
        {
            MarcNodeList results = new MarcNodeList(this.m_list[index]);
            this.m_list.RemoveAt(index);
            return results;
        }

        // 移走一个对象
        // 返回包含移走的对象的集合
        /// <summary>
        /// 从当前集合中移走指定的元素
        /// </summary>
        /// <param name="node">要移走的元素</param>
        /// <returns>被移走的一个元素(所构成的新集合)</returns>
        public MarcNodeList remove(MarcNode node)
        {
            bool bRet = this.m_list.Remove(node);   // TODO: 查阅一下返回值的含义
            if (bRet == true)
                return new MarcNodeList(node);
            return new MarcNodeList();
        }

        /// <summary>
        /// 在当前集合指定的下标位置插入一个元素
        /// </summary>
        /// <param name="index">插入点的下标</param>
        /// <param name="node">要插入的元素</param>
        /// <returns>插入了新元素后的当前集合</returns>
        public MarcNodeList insert(int index, MarcNode node)
        {
            this.m_list.Insert(index, node);
            return this;
        }

        /// <summary>
        /// 清除当前集合中的全部元素
        /// </summary>
        public void clear()
        {
            this.m_list.Clear();
        }

        // 插入一个适当的顺序位置
        // 规则是，插入位置的前一个元素比要插入的对象小，后一个元素比要插入的对象大
        // 所谓“小”和“大”的规则，也就是排序规则，可以自定义
        /// <summary>
        /// 向当前集合中添加一个节点元素，按节点名字顺序决定加入的位置
        /// </summary>
        /// <param name="node">要加入的节点</param>
        /// <param name="style">如何加入</param>
        /// <param name="comparer">用于比较大小的接口</param>
        public virtual void insertSequence(MarcNode node, 
            InsertSequenceStyle style = InsertSequenceStyle.PreferHead,
            IComparer<MarcNode> comparer = null)
        {
            if (comparer == null)
                comparer = new MarcNodeComparer();

            // 寻找插入位置
            List<int> values = new List<int>(); // 累积每个比较结果数字
            int nInsertPos = -1;
            int i = 0;
            foreach (MarcNode current in this)
            {
                int nBigThanCurrent = 0;   // 相当于node和当前对象相减
                
                nBigThanCurrent = comparer.Compare(node, current);
                if (nBigThanCurrent < 0)
                {
                    nInsertPos = i;
                    break;
                }
                if (nBigThanCurrent == 0)
                {
                    if ((style & InsertSequenceStyle.PreferHead) != 0)
                    {
                        nInsertPos = i;
                        break;
                    }
                }

                // 刚刚遇到过相等的一段，但在当前位置结束了相等 (或者开始变大，或者开始变小)
                if (nBigThanCurrent != 0 && values.Count > 0 && values[values.Count - 1] == 0)
                {
                    if ((style & InsertSequenceStyle.PreferTail) != 0)
                    {
                        nInsertPos = i - 1;
                        break;
                    }
                }

                values.Add(nBigThanCurrent);
                i++;
            }

            if (nInsertPos == -1)
            {
                this.m_list.Add(node);
                return;
            }

            this.m_list.Insert(nInsertPos, node);
        }

        /// <summary>
        /// 向当前集合中添加一个节点元素，按节点名字顺序决定加入的位置。寻找位置的算法是从集合尾部向开头寻找
        /// </summary>
        /// <param name="node">要加入的节点</param>
        /// <param name="style">如何加入</param>
        /// <param name="comparer">用于比较大小的接口</param>
        public virtual void insertSequenceReverse(MarcNode node,
    InsertSequenceStyle style = InsertSequenceStyle.PreferHead,
    IComparer<MarcNode> comparer = null)
        {
            if (comparer == null)
                comparer = new MarcNodeComparer();

            // 寻找插入位置
            List<int> values = new List<int>(); // 累积每个比较结果数字
            int nInsertPos = -1;
            for (int i = this.count - 1 ;i>=0; i--)
            {
                MarcNode current = this[i];

                int nBigThanCurrent = 0;   // 相当于node和当前对象相减

                nBigThanCurrent = comparer.Compare(node, current) * -1;
                if (nBigThanCurrent < 0)
                {
                    nInsertPos = i + 1;
                    break;
                }
                if (nBigThanCurrent == 0)
                {
                    if ((style & InsertSequenceStyle.PreferTail) != 0)
                    {
                        nInsertPos = i;
                        break;
                    }
                }

                // 刚刚遇到过相等的一段，但在当前位置结束了相等 (或者开始变大，或者开始变小)
                if (nBigThanCurrent != 0 && values.Count > 0 && values[values.Count - 1] == 0)
                {
                    if ((style & InsertSequenceStyle.PreferHead) != 0)
                    {
                        nInsertPos = i;
                        break;
                    }
                }

                values.Add(nBigThanCurrent);
            }

            if (nInsertPos == -1)
            {
                this.m_list.Insert(0, node);
                return;
            }

            this.m_list.Insert(nInsertPos, node);
        }

        class MarcNodeComparer : IComparer<MarcNode>
        {
            int IComparer<MarcNode>.Compare(MarcNode x, MarcNode y)
            {
                // 如果名字字符串中出现了字符 '-'，需要特殊的比较方式
                if (x.Name.IndexOf("-") != -1 || y.Name.IndexOf("-") != -1)
                    return CompareFieldName(x.Name, y.Name);
                return string.Compare(x.Name, y.Name);
            }

            // 字段名字符串比较大小
            // "-01"理解为比"001"更小
            public static int CompareFieldName(string s1, string s2)
            {
                s1 = s1.Replace("-", "/");
                s2 = s2.Replace("-", "/");
                return string.CompareOrdinal(s1, s2);
            }
        }

        /*
        //
        // 摘要:
        //     使用默认比较器对整个 System.Collections.Generic.List<T> 中的元素进行排序。
        //
        // 异常:
        //   System.InvalidOperationException:
        //     默认比较器 System.Collections.Generic.Comparer<T>.Default 找不到 T 类型的 System.IComparable<T>
        //     泛型接口或 System.IComparable 接口的实现。
        public void Sort();
        //
        // 摘要:
        //     使用指定的 System.Comparison<T> 对整个 System.Collections.Generic.List<T> 中的元素进行排序。
        //
        // 参数:
        //   comparison:
        //     比较元素时要使用的 System.Comparison<T>。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     comparison 为 null。
        //
        //   System.ArgumentException:
        //     comparison 的实现导致排序时出现错误。例如，将某个项与其自身进行比较时，comparison 可能不返回 0。
        public void Sort(Comparison<T> comparison);
        //
        // 摘要:
        //     使用指定的比较器对整个 System.Collections.Generic.List<T> 中的元素进行排序。
        //
        // 参数:
        //   comparer:
        //     比较元素时要使用的 System.Collections.Generic.IComparer<T> 实现，或者为 null，表示使用默认比较器 System.Collections.Generic.Comparer<T>.Default。
        //
        // 异常:
        //   System.InvalidOperationException:
        //     comparer 为 null，且默认比较器 System.Collections.Generic.Comparer<T>.Default 找不到
        //     T 类型的 System.IComparable<T> 泛型接口或 System.IComparable 接口的实现。
        //
        //   System.ArgumentException:
        //     comparer 的实现导致排序时出现错误。例如，将某个项与其自身进行比较时，comparer 可能不返回 0。
        public void Sort(IComparer<T> comparer);
        //
        // 摘要:
        //     使用指定的比较器对 System.Collections.Generic.List<T> 中某个范围内的元素进行排序。
        //
        // 参数:
        //   index:
        //     要排序的范围的从零开始的起始索引。
        //
        //   count:
        //     要排序的范围的长度。
        //
        //   comparer:
        //     比较元素时要使用的 System.Collections.Generic.IComparer<T> 实现，或者为 null，表示使用默认比较器 System.Collections.Generic.Comparer<T>.Default。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     index 小于 0。- 或 -count 小于 0。
        //
        //   System.ArgumentException:
        //     index 和 count 未指定 System.Collections.Generic.List<T> 中的有效范围。- 或 -comparer
        //     的实现导致排序时出现错误。例如，将某个项与其自身进行比较时，comparer 可能不返回 0。
        //
        //   System.InvalidOperationException:
        //     comparer 为 null，且默认比较器 System.Collections.Generic.Comparer<T>.Default 找不到
        //     T 类型的 System.IComparable<T> 泛型接口或 System.IComparable 接口的实现。
        public void Sort(int index, int count, IComparer<T> comparer);
         * */

        /// <summary>
        /// 对集合进行排序。按照节点名升序排序
        /// </summary>
        /// <returns>排序后的当前集合</returns>
        public MarcNodeList sort()
        {
            this.m_list.Sort(new MarcNodeComparer());
            return this;
        }

        /// <summary>
        /// 对集合进行排序
        /// </summary>
        /// <param name="comparer">排序接口</param>
        /// <returns>排序后的当前集合</returns>
        public MarcNodeList sort(IComparer<MarcNode> comparer)
        {
            this.m_list.Sort(comparer);
            return this;
        }

        // 2017/2/23
        /// <summary>
        /// 对集合进行排序
        /// </summary>
        /// <param name="comparison">排序接口</param>
        /// <returns>排序后的当前集合</returns>
        public MarcNodeList sort(Comparison<MarcNode> comparison)
        {
            this.m_list.Sort(comparison);
            return this;
        }

#region 获得元素

        // 返回第一个元素
        /// <summary>
        /// 获得当前集合的第一个元素
        /// </summary>
        /// <returns>当前集合的第一个元素(所构成的一个新集合)</returns>
        public MarcNodeList first()
        {
            MarcNodeList results = new MarcNodeList();
            if (this.count > 0)
                results.add(this[0]);

            return results;
        }

        // 返回最后一个元素
        /// <summary>
        /// 获得当前集合的最后一个元素
        /// </summary>
        /// <returns>当前集合的最后一个元素(所构成的一个新集合)</returns>
        public MarcNodeList last()
        {
            MarcNodeList results = new MarcNodeList();
            if (this.count > 0)
                results.add(this[this.count - 1]);

            return results;
        }

        // 返回某个元素
        /// <summary>
        /// 获得指定下标位置的元素
        /// </summary>
        /// <param name="index">下标</param>
        /// <returns>指定的元素(所构成的一个新集合)</returns>
        public MarcNodeList getAt(int index)
        {
            MarcNodeList results = new MarcNodeList();
            if (index < 0 || index >= this.count)
                throw new ArgumentException("index值 " + index.ToString() + " 超出许可范围", "index");

            results.add(this[index]);

            return results;
        }

#if NO
        // 返回一段元素
        public MarcNodeList getAt(int start, int length)
        {
            MarcNodeList results = new MarcNodeList();
            if (length == 0)
                return results;

            if (start < 0 || start >= this.count)
                throw new Exception("index值 " + start.ToString() + " 超出许可范围");
            if (start + length - 1 < 0 || start + length - 1 >= this.count)
                throw new Exception("index + length 值 " + (start + length).ToString() + " 超出许可范围");

            for (int i = 0; i < length; i++)
            {
                results.add(this[start + i]);
            }

            return results;
        }
#endif

        // 返回某些元素
        // parameters:
        //      length  要取得的个数。如果为-1，表示从index一直取到末尾
        /// <summary>
        /// 获得当前集合中一段范围的元素
        /// </summary>
        /// <param name="index">起点下标</param>
        /// <param name="length">个数</param>
        /// <returns>指定的元素(所构成的一个新集合)</returns>
        public MarcNodeList getAt(int index, int length)
        {
            MarcNodeList results = new MarcNodeList();
            if (length == 0)
                return results;
            if (index < 0 || index >= this.count)
                throw new ArgumentException("index值 " + index.ToString() + " 超出许可范围", "index");

            if (length == -1)
                length = this.count - index;

            if (index + length < 0 || index + length - 1 >= this.count)
                throw new ArgumentException("index+length值 " + (index + length).ToString() + " 超出许可范围");

            for (int i = index; i < index + length; i++)
            {
                results.add(this[i]);
            }

            return results;
        }

#endregion

#region 获得各种值

        // 第一个元素的 Name
        /// <summary>
        /// 获得当前集合中第一个元素的 Name 值
        /// </summary>
        public string FirstName
        {
            get
            {
                if (this.count == 0)
                    return null;

                return this[0].Name;
            }
        }

        // 第一个元素的 Indicator
        /// <summary>
        /// 获得当前集合中第一个元素的 Indicator 值
        /// </summary>
        public string FirstIndicator
        {
            get
            {
                if (this.count == 0)
                    return null;

                return this[0].Indicator;
            }
        }

        // 第一个元素的 Indicator1
        /// <summary>
        /// 获得当前集合中中第一个元素的 Indicator1 值
        /// </summary>
        public char FirstIndicator1
        {
            get
            {
                if (this.count == 0)
                    return (char)0;

                return this[0].Indicator1;
            }
        }

        // 第一个元素的 Indicator2
        /// <summary>
        /// 获得当前集合的第一个元素的 Indicator2 值
        /// </summary>
        public char FirstIndicator2
        {
            get
            {
                if (this.count == 0)
                    return (char)0;

                return this[0].Indicator2;
            }
        }

        // 第一个元素的Content
        /// <summary>
        /// 获得当前集合中第一个元素的 Content 值
        /// </summary>
        public string FirstContent
        {
            get
            {
                if (this.count == 0)
                    return null;

                return this[0].Content;
            }
        }

        // 2016/12/14
        /// <summary>
        /// 获得当前集合中全部节点的 Content 值拼接起来的字符串
        /// </summary>
        public List<string> Contents
        {
            get
            {
                List<string> results = new List<string>();
                foreach(MarcNode node in this)
                {
                    results.Add(node.Content);
                }

                return results;
            }
        }

        // 第一个元素的 Text
        /// <summary>
        /// 获得当前集合第一个元素的 Text 值
        /// </summary>
        public string FirstText
        {
            get
            {
                if (this.count == 0)
                    return null;

                return this[0].Text;
            }
        }

        // 所有元素的 Text 连接起来的一个字符串
        // 这种字符串可以用来一次创建若干对象 (假如都是 字段 或者 子字段 的话)
        /// <summary>
        /// 获得当前集合中全部元素的 Text 值所连接起来的一个字符串值
        /// </summary>
        public string AllText
        {
            get
            {
                if (this.count == 0)
                    return null;

                StringBuilder text = new StringBuilder(4096);
                foreach (MarcNode node in this)
                {
                    text.Append(node.Text);
                }
                return text.ToString();
            }
        }

        // 第一个元素的 Leading
        /// <summary>
        /// 获得当前集合中第一个 MarcField 类型元素的 Leading 值。注意，只有 MarcField 类型的节点才有 Leading 值。
        /// </summary>
        public string FirstLeading
        {
            get
            {
                if (this.count == 0)
                    return null;
                MarcNode node = this[0];
                if (node is MarcField)
                    return ((MarcField)node).Leading;
                return "";
            }
        }

        // 获得所有元素的第一个下级元素
        /// <summary>
        /// 获得当前集合中所有元素的第一个下级元素。注意这是一个集合
        /// </summary>
        public MarcNodeList FirstChild
        {
            get
            {
                MarcNodeList results = new MarcNodeList();
                foreach (MarcNode node in this)
                {
                    MarcNode child = node.FirstChild;
                    if (child != null)
                        results.add(child);
                }

                return results;
            }
        }

        // 获得所有元素的最后一个下级元素
        /// <summary>
        /// 获得当前集合中所有元素的最后一个下级元素。注意这是一个集合
        /// </summary>
        public MarcNodeList LastChild
        {
            get
            {
                MarcNodeList results = new MarcNodeList();
                foreach (MarcNode node in this)
                {
                    MarcNode child = node.LastChild;
                    if (child != null)
                        results.add(child);
                }

                return results;
            }
        }

#endregion

#region 成批修改成员

        /// <summary>
        /// 修改当前集合中的每个元素的 Name 值
        /// </summary>
        public string Name
        {
            set
            {
                foreach (MarcNode node in this)
                {
                    node.Name = value;  // TODO: 修改name的长度不合适了是否高报错?
                }
            }
        }

        /// <summary>
        /// 修改当前集合中每个元素的 Indicator 值
        /// </summary>
        public string Indicator
        {
            set
            {
                foreach (MarcNode node in this)
                {
                    node.Indicator = value;  // TODO: 修改indicator的长度不合适了是否报错?
                }
            }
        }

        /// <summary>
        /// 修改当前集合中每个元素的 Indicator1 值
        /// </summary>
        public char Indicator1
        {
            set
            {
                foreach (MarcNode node in this)
                {
                    node.Indicator1 = value;
                }
            }
        }

        /// <summary>
        /// 修改当前集合中每个元素的 Indicator2 值
        /// </summary>
        public char Indicator2
        {
            set
            {
                foreach (MarcNode node in this)
                {
                    node.Indicator2 = value;
                }
            }
        }

        /// <summary>
        /// 修改当前集合中每个元素的 Content 值
        /// </summary>
        public string Content
        {
            set
            {
                foreach (MarcNode node in this)
                {
                    node.Content = value;
                }
            }
        }

        /// <summary>
        /// 修改当前集合中每个元素的 Text 值
        /// </summary>
        public string Text
        {
            set
            {
                foreach (MarcNode node in this)
                {
                    node.Text = value;
                }
            }
        }

        /// <summary>
        /// 修改当前集合中每个 MarcField 类型的元素的 Leading 值
        /// </summary>
        public string Leading
        {
#if NO
            get
            {
                if (this.Count == 0)
                    return null;
                MarcNode node = this[0];
                if (node is MarcField)
                    return ((MarcField)node).Leading;
                return "";
            }
#endif
            set
            {
                foreach (MarcNode node in this)
                {
                    if (node is MarcField)
                    {
                        ((MarcField)node).Leading = value;
                    }
                }
            }
        }

        /// <summary>
        /// 在当前集合中每个元素的 DOM 位置前面插入指定的源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList before(MarcNodeList source_nodes)
        {
            MarcQuery.insertBefore(source_nodes, this);
            return this;
        }

        /// <summary>
        /// 在当前集合中每个元素的 DOM 位置前面插入新元素，新元素由指定的源字符串构造
        /// </summary>
        /// <param name="strSourceText">源字符串</param>
        /// <returns>当前集合</returns>
        public MarcNodeList before(string strSourceText)
        {
            if (this.count == 0 || string.IsNullOrEmpty(strSourceText))
                return this;

            if (this[0].NodeType == NodeType.None)
                throw new ArgumentException("不允许在 None 对象的后面添加任何对象");
            if (this[0].NodeType == NodeType.Record)
                throw new ArgumentException("不允许在 记录 对象的后面添加任何对象");

            MarcNodeList source_nodes = null;
            string strLeading = "";
            if (this[0].NodeType == NodeType.Field)
                source_nodes = MarcQuery.createFields(strSourceText);
            else
            {
                source_nodes = MarcQuery.createSubfields(strSourceText, out strLeading);
                // leading要追加到目标集合的最后一个元素的Content末尾
                if (string.IsNullOrEmpty(strLeading) == false)
                    this.last()[0].Content += strLeading;
            }

            MarcQuery.insertBefore(source_nodes, this);
            return this;
        }

        // 在当前集合中每个元素的DOM位置后面插入源集合内的元素
        // 参见 MarcQuery::insertAfter() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 在当前集合中每个元素的 DOM 位置后面插入指定的源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList after(MarcNodeList source_nodes)
        {
            MarcQuery.insertAfter(source_nodes, this);
            return this;
        }

        // 在当前集合中每个元素的DOM位置后面插入新元素
        // 参见 MarcQuery::insertAfter() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 在当前集合中每个元素的 DOM 位置后面插入新元素，新元素由指定的源字符串构造
        /// </summary>
        /// <param name="strSourceText">源字符串</param>
        /// <returns>当前集合</returns>
        public MarcNodeList after(string strSourceText)
        {
            if (this.count == 0 || string.IsNullOrEmpty(strSourceText))
                return this;

            if (this[0].NodeType == NodeType.None)
                throw new ArgumentException("不允许在 None 对象的后面添加任何对象");
            if (this[0].NodeType == NodeType.Record)
                throw new ArgumentException("不允许在 记录 对象的后面添加任何对象");

            MarcNodeList source_nodes = null;
            string strLeading = "";
            if (this[0].NodeType == NodeType.Field)
                source_nodes = MarcQuery.createFields(strSourceText);
            else
            {
                source_nodes = MarcQuery.createSubfields(strSourceText, out strLeading);
                // leading要追加到目标集合的最后一个元素的Content末尾
                if (string.IsNullOrEmpty(strLeading) == false)
                    this.last()[0].Content += strLeading;
            }

            MarcQuery.insertAfter(source_nodes, this);
            return this;
        }

        /// <summary>
        /// 将当前集合中的每个元素插入到指定的目标集合的每个元素的 DOM 位置前面
        /// </summary>
        /// <param name="target_nodes">目标集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList insertBefore(MarcNodeList target_nodes)
        {
            MarcQuery.insertBefore(this, target_nodes);
            return this;
        }

        // 把当前集合中的每个元素插入到目标集合的每个元素的DOM位置后面
        // 参见 MarcQuery::insertAfter() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 将当前集合中的每个元素插入到指定的目标集合的每个元素的 DOM 位置后面
        /// </summary>
        /// <param name="target_nodes">目标集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList insertAfter(MarcNodeList target_nodes)
        {
            MarcQuery.insertAfter(this, target_nodes);
            return this;
        }

        // 在当前集合中每个元素的DOM位置 下级末尾 插入源集合内的元素
        // 参见 MarcQuery::append() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 在当前集合中每个元素的 DOM 位置的下级末尾 追加来自源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList append(MarcNodeList source_nodes)
        {
            MarcQuery.append(source_nodes, this);
            return this;
        }

        // 在当前集合中每个元素的DOM位置 下级末尾 插入新元素
        // 参见 MarcQuery::append() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 在当前集合中每个元素的 DOM 位置的下级末尾 追加根据源字符串构造的新元素
        /// </summary>
        /// <param name="strSourceText">源字符串</param>
        /// <returns>当前集合</returns>
        public MarcNodeList append(string strSourceText)
        {
            if (this.count == 0 || string.IsNullOrEmpty(strSourceText))
                return this;

            if (this[0].NodeType == NodeType.None)
                throw new ArgumentException("不允许在 None 节点的 下级末尾 添加任何节点");
            if (this[0].NodeType == NodeType.Subfield)
                throw new ArgumentException("不允许在 子字段 节点的 下级末尾添加任何节点。因为子字段本身就是末级节点，不允许出现下级节点");

            MarcNodeList source_nodes = null;
            string strLeading = "";
            if (this[0].NodeType == NodeType.Record)
                source_nodes = MarcQuery.createFields(strSourceText);
            else if (this[0].NodeType == NodeType.Field)
            {
                source_nodes = MarcQuery.createSubfields(strSourceText, out strLeading);
                // leading要追加到目标集合的最后一个元素的Content末尾
                if (string.IsNullOrEmpty(strLeading) == false)
                    this.last()[0].Content += strLeading;
            }
            else
            {
                throw new Exception("未知的对象类型 '" + this[0].NodeType.ToString() + "'");
            }

            MarcQuery.append(source_nodes, this);
            return this;
        }

        /// <summary>
        /// 将当前集合内的每个元素追加到指定的目标集合的 DOM 位置下级末尾
        /// </summary>
        /// <param name="target_nodes">目标集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList appendTo(MarcNodeList target_nodes)
        {
            MarcQuery.append(this, target_nodes);
            return this;
        }

        // 在当前集合中每个元素的DOM位置 下级开头 插入源集合内的元素
        // 参见 MarcQuery::prepend() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 在当前集合的每个元素的 DOM 位置的下级开头插入源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList prepend(MarcNodeList source_nodes)
        {
            MarcQuery.prepend(source_nodes, this);
            return this;
        }

        // 在当前集合中每个元素的DOM位置 下级开头 插入新元素
        // 参见 MarcQuery::prepend() 函数的注释
        // 最后返回当前集合
        /// <summary>
        /// 在当前集合的每个元素的 DOM 位置的下级开头插入根据指定的源字符串构造的新元素
        /// </summary>
        /// <param name="strSourceText">源字符串</param>
        /// <returns>当前集合</returns>
        public MarcNodeList prepend(string strSourceText)
        {
            if (this.count == 0 || string.IsNullOrEmpty(strSourceText))
                return this;

            if (this[0].NodeType == NodeType.None)
                throw new ArgumentException("不允许在 None 节点的 下级开头 添加任何节点");
            if (this[0].NodeType == NodeType.Subfield)
                throw new ArgumentException("不允许在 子字段 节点的 下级开头 添加任何节点。因为子字段本身就是末级节点，不允许出现下级节点");

            MarcNodeList source_nodes = null;
            string strLeading = "";
            if (this[0].NodeType == NodeType.Record)
                source_nodes = MarcQuery.createFields(strSourceText);
            else if (this[0].NodeType == NodeType.Field)
            {
                source_nodes = MarcQuery.createSubfields(strSourceText, out strLeading);
                // leading要追加到目标集合的最后一个元素的Content末尾
                if (string.IsNullOrEmpty(strLeading) == false)
                    this.last()[0].Content += strLeading;
            }
            else
            {
                throw new Exception("未知的对象类型 '" + this[0].NodeType.ToString() + "'");
            }

            MarcQuery.prepend(source_nodes, this);
            return this;
        }

        /// <summary>
        /// 将当前集合内的全部元素插入到指定的目标集合的 DOM 位置下级开头
        /// </summary>
        /// <param name="target_nodes">目标集合</param>
        /// <returns>当前集合</returns>
        public MarcNodeList prependTo(MarcNodeList target_nodes)
        {
            MarcQuery.prepend(this, target_nodes);
            return this;
        }

        // 把当前集合内的每个元素从DOM位置摘除
        // 最后返回当前集合
        // 注：摘除下来的局部的DOM树，也应该可以对它进行select()的操作。只是根元素不一定是 MarcRoot 了
        /// <summary>
        /// 把当前集合内的每个元素从 DOM 位置摘除
        /// </summary>
        /// <returns>当前集合</returns>
        public MarcNodeList detach()
        {
            foreach (MarcNode node in this)
            {
                node.detach();
            }

            return this;
        }

#endregion

#region 筛选

        // 对一批互相没有树重叠关系的对象进行筛选
        static MarcNodeList simpleSelect(MarcNodeList source,
            string strXPath,
            int nMaxCount = -1)
        {
            // 准备一个模拟的记录节点
            // MarcRecord record = new MarcRecord();
            MarcNode temp_root = new MarcNode();
            temp_root.Name = "temp";

            // 保存下集合中所有的Parent指针
            List<MarcNode> parents = new List<MarcNode>();
            foreach (MarcNode node in source)
            {
                // 保存指针
                parents.Add(node.Parent);

                // 建立父子关系，但原来位置的ChidNodes并不摘除
                temp_root.ChildNodes.baseAdd(node);
                node.Parent = temp_root;
                // Debug.Assert(node.Parent == temp_root, "");
            }
            Debug.Assert(parents.Count == source.count, "");

            try
            {
                MarcNodeList results = new MarcNodeList();

                MarcNavigator nav = new MarcNavigator(temp_root);  // 出发点在模拟的记录节点上

                XPathNodeIterator ni = nav.Select(strXPath);
                while (ni.MoveNext() && (nMaxCount == -1 || results.count < nMaxCount))
                {
                    NaviItem item = ((MarcNavigator)ni.Current).Item;
                    if (item.Type != NaviItemType.Element)
                    {
                        throw new Exception("xpath '" + strXPath + "' 命中了非元素类型的节点，这是不允许的");
                        continue;
                    }
                    if (results.indexOf(item.MarcNode) == -1)   // 不重复的才加入
                        results.add(item.MarcNode);
                }
                return results;
            }
            finally
            {
                // 恢复原先的 Parent 指针
                Debug.Assert(parents.Count == source.count, "");
                for (int i = 0; i < source.count; i++)
                {
                    source[i].Parent = parents[i];
                }
            }
        }

        // 观察两个 MarcNode 之间是否有重叠关系
        static bool isCross(MarcNode node1, MarcNode node2)
        {
            MarcNode current = node1;
            while (current != null)
            {
                if (current == node2)
                    return true;
                current = current.Parent;
            }
            current = node2;
            while (current != null)
            {
                if (current == node1)
                    return true;
                current = current.Parent;
            }

            return false;
        }

        // 观察 node 是否和集合中的任何一个节点有重叠关系
        static bool isCross(MarcNode node, MarcNodeList list)
        {
            foreach (MarcNode current in list)
            {
                if (isCross(current, node) == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 针对当前集合中的全部元素进行 XPath 选择
        /// </summary>
        /// <param name="strXPath">XPath字符串</param>
        /// <returns>选中的元素所构成的新集合</returns>
        public MarcNodeList select(string strXPath)
        {
            return select(strXPath, -1);
        }

        // 针对集合中的元素进行 XPath 筛选
        // 本函数不怕同一批元素之间有重叠关系。所采用的策略是一批一批单独筛选，然后把输出结果合成
        // parameters:
        //      nMaxCount    至多选择开头这么多个元素。-1表示不限制
        /// <summary>
        /// 针对当前集合中的全部元素进行 XPath 选择
        /// </summary>
        /// <param name="strXPath">XPath字符串</param>
        /// <param name="nMaxCount">限制命中的最大元素个数。如果为 -1，表示不限制</param>
        /// <returns>选中的元素所构成的新集合</returns>
        public MarcNodeList select(string strXPath, int nMaxCount/* = -1*/)
        {
            // 把当前集合分割为每段内部确保互相不重叠
            List<MarcNodeList> lists = new List<MarcNodeList>();

            {
                MarcNodeList segment = new MarcNodeList();  // 当前累积的段落
                foreach (MarcNode node in this)
                {
                    if (segment.count > 0)
                    {
                        if (isCross(node, segment) == true)
                        {
                            // 推走
                            lists.Add(segment);
                            segment = new MarcNodeList();
                        }
                    }
                    segment.add(node);
                }

                // 最后剩下的
                if (segment.count > 0)
                    lists.Add(segment);
            }


            MarcNodeList results = new MarcNodeList();
            foreach (MarcNodeList segment in lists)
            {
                // 对一批互相没有树重叠关系的对象进行筛选
                MarcNodeList temp = simpleSelect(segment,
                    strXPath,
                    -1);
                foreach (MarcNode node in temp)
                {
                    if (results.indexOf(node) == -1)
                        results.add(node);
                }

                if (nMaxCount != -1 && results.count >= nMaxCount)
                {
                    if (results.count > nMaxCount)
                        return results.getAt(0, nMaxCount);
                    break;
                }
            }
            return results;
        }

#endregion

        /// <summary>
        /// 输出当前对象的调试用字符串
        /// </summary>
        /// <param name="style">输出的风格</param>
        /// <returns>表示内容的字符串</returns>
        public string dump(DumpStyle style = DumpStyle.None)
        {
            StringBuilder result = new StringBuilder(4096);
            int i = 0;
            foreach (MarcNode node in this)
            {
                if (result.Length > 0)
                    result.Append("\r\n");
                result.Append(
                    ((style & DumpStyle.LineNumber) != 0 ? (i + 1).ToString() + ") " : "")
                    + node.dump());
                i++;
            }

            return result.ToString();
        }

#if NO
                public string Dump()
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                if (i > 0)
                    strResult += "\r\n";
                strResult += this[i].Dump();
            }

            return strResult;
        }

#endif

#if NO
        // 清除集合，并把原先的每个元素的Parent清空。
        // 主要用于ChildNodes摘除关系
        internal void ClearAndDetach()
        {
            foreach (MarcNode node in this)
            {
                node.Parent = null;
            }
            base.Clear();
        }
#endif
    }

    /// <summary>
    /// 插入数组操作时的细节特性
    /// </summary>
    [Flags]
    public enum InsertSequenceStyle
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0x00,
        /// <summary>
        /// 如果名字相等，倾向于插入前方
        /// </summary>
        PreferHead = 0x01,  // 如果名字相等，倾向于插入前方
        /// <summary>
        /// 如果名字相等，倾向于插入后方
        /// </summary>
        PreferTail = 0x02,  // 如果名字相等，倾向于插入后方
    }
}
