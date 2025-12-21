using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DigitalPlatform.Text;
using LibraryStudio.Forms;

namespace DigitalPlatform.Marc
{
    // 
    /// <summary>
    /// 字段集合
    /// </summary>
    public class FieldCollection : IList<Field>
    {
        // 编辑器的 DOM 对象
        internal DomRecord _domRecord = null;

        public FieldCollection(DomRecord domRecord)
        {
            _domRecord = domRecord;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="comparer">排序接口</param>
        public void Sort(IComparer comparer)
        {

        }

#if REMOVED
        // 摘要:
        //     获取或设置指定索引处的元素。
        //
        // 参数:
        //   index:
        //     要获得或设置的元素从零开始的索引。
        //
        // 返回结果:
        //     指定索引处的元素。
        //
        // 异常:
        //   System.ArgumentOutOfRangeException:
        //     index 小于 0。- 或 -index 等于或大于 System.Collections.Generic.List<T>.Count。
        /// <summary>
        /// 获取或设置指定索引处的 Field 对象。
        /// </summary>
        /// <param name="nIndex">要获得或设置的元素从零开始的索引。</param>
        /// <returns>指定索引处的 Field 对象。</returns>
        public new Field this[int nIndex]
        {
            get
            {
                if (nIndex < 0 || nIndex >= this.Count)
                {
                    Debug.Assert(false, "下标越界");
                    throw new Exception("下标越界。");
                }
                return (Field)base[nIndex];
            }
            set
            {
                base[nIndex] = value;
            }
        }
#endif

        /// <summary>
        /// 获取指定字段名的若干个字段中的某一个
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="nIndex">符合字段名的若干字段中的第几个</param>
        /// <returns>Field 对象</returns>
        public Field this[string strFieldName,
            int nIndex]
        {
            get
            {
                return this.GetOneField(strFieldName, nIndex);
            }
        }

        /// <summary>
        /// 获取指定字段名的若干个字段中的某一个
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="nIndex">符合字段名的若干字段中的第几个</param>
        /// <param name="strIndicatorMatch">字段指示符筛选条件。缺省为 "**"</param>
        /// <returns>Field 对象</returns>
        public Field GetOneField(string strFieldName,
            int nIndex,
            string strIndicatorMatch = "**")
        {
            var fields = this.GetFields(strFieldName, strIndicatorMatch);
            if (nIndex < 0 || nIndex >= fields.Count)
                return null;

            return fields[nIndex];
        }

        /// <summary>
        /// 获得符合条件的若干个字段对象
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strIndicatorMatch">字段指示符筛选条件。缺省为 "**"</param>
        /// <returns>字段对象集合</returns>
        public List<Field> GetFields(string strFieldName,
            string strIndicatorMatch = "**")
        {
            var fields = new List<Field>();
            foreach (Field field in this)
            {
                if (field.Name == strFieldName)
                {
                    if (strIndicatorMatch != "**"
                        && string.IsNullOrEmpty(field.Indicator) == false
                        && MarcUtil.MatchIndicator(strIndicatorMatch, field.Indicator) == false)
                        continue;
                    // 这里要特别注意
                    fields.Add(field);
                }
            }
            return fields;
        }

        // 取指定名称第一个字段的第一个子字段
        /// <summary>
        /// 取指定字段名的第一个字段的第一个子字段
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <returns>子字段值</returns>
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName)
        {
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
                return "";
            Subfield subfield = field.Subfields.GetSubfield(strSubfieldName, 0);
            if (subfield == null)
                return "";

            return subfield.Value;
        }

        // 2011/8/9
        // 取指定名称第一个字段的第一个子字段。还能依据字段的指示符进行筛选
        /// <summary>
        /// 取指定名称第一个字段的第一个子字段。还能依据字段的指示符进行筛选
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <param name="strIndicatorMatch">字段指示符筛选条件。缺省为 "**"</param>
        /// <returns>子字段值</returns>
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch)
        {
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
                return "";
            Subfield subfield = field.Subfields.GetSubfield(strSubfieldName, 0);
            if (subfield == null)
                return "";

            return subfield.Value;
        }

        // 2011/8/10
        // 取指定名称字段的子字段。还能依据字段的指示符进行筛选
        // parameters:
        //      strFieldName    3字符的字段名
        //      strSubfieldName 可以为一个或者多个字符。每个字符代表一个子字段名
        /// <summary>
        /// 取指定名称字段的子字段。还能依据字段的指示符进行筛选
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名。可以为一个或者多个字符。每个字符代表一个子字段名</param>
        /// <param name="strIndicatorMatch">字段指示符筛选条件。缺省为 "**"</param>
        /// <returns>子字段值数组</returns>
        public List<string> GetSubfields(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch)
        {
            List<string> results = new List<string>();
            var fields = this.GetFields(strFieldName, strIndicatorMatch);
            foreach (Field field in fields)
            {
                foreach (Subfield subfield in field.Subfields)
                {
                    if (strSubfieldName.IndexOf(subfield.Name) != -1)
                        results.Add(subfield.Value);
                }
            }
            return results;
        }

        public Field SetFirstSubfield(string strFieldName,
    string strSubfieldName,
    string strSubfieldValue)
        {
            return SetFirstSubfield(strFieldName,
            strSubfieldName,
            strSubfieldValue,
            out _);
        }

        /// <summary>
        /// 设置指定字段名的第一个字段内的指定子字段名的第一个子字段的值。
        /// 如果不存在这样的字段和子字段，则次第创建
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <param name="strSubfieldValue">子字段值</param>
        /// <param name="old_field"></param>
        public Field SetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strSubfieldValue,
            out Field old_field)
        {
            old_field = null;
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
            {
                field = this.Add(strFieldName,
                    "  ",
                    "",
                    true);
            }
            else
            {
                old_field = field.Clone();
            }

            if (field == null)
                throw new Exception("不可能的情况");

            Subfield subfield = field.Subfields.GetSubfield(strSubfieldName, 0);
            if (subfield != null)
                subfield.Value = strSubfieldValue;
            else
            {
                subfield = new Subfield();
                subfield.m_strName = strSubfieldName;
                subfield.m_strValue = strSubfieldValue;
                field.Subfields.Add(subfield, true);
            }

            return field;
        }

        internal MarcEditor MarcEditor
        {
            get
            {
                return this._domRecord.GetControl() as MarcEditor;
            }
        }

        public int Count => _domRecord.FieldCount;

        bool ICollection<Field>.IsReadOnly => true;

        public Field this[int index]
        {
            get
            {
                if (index < 0 || index >= _domRecord.FieldCount)
                {
                    throw new ArgumentException("index 下标越界");
                }
                var source = _domRecord.GetField(index);
                return new Field(this, source);
            }
            set
            {
                // 替换一个字段
                _domRecord.GetField(index).Text = value.Text;
            }
        }

        // 获得当前输入焦点所在的字段
        public Field GetFocusedField()
        {
            var field = _domRecord.LocateField(_domRecord.CaretOffset);
            if (field == null)
                return null;
            return new Field(this, field);
        }

        //--------------------------追加字段-------------------

        // 追加一个新字段，只供内部使用，
        // 因为该函数只处理内存对象，不涉及界面的事情
        // 操作历史: 无
        // parameters:
        //		strName	新字段名称
        //		strIndicator	新字段指示符
        //		strValue	新字段值 子字段指示符已经转换为内部形态
        //		bFireTextChanged	是否触发TextChanged事件
        //		bInOrder	是否按序加到指定位置 true加到指定位置，false加到末尾
        // return:
        //		void
        internal Field AddInternal(string strName,
            string strIndicator,
            string strValue,
            bool bFireTextChanged,
            bool bInOrder,
            out int nOutputPosition)
        {
            nOutputPosition = -1;

            nOutputPosition = _domRecord.FieldCount;
            if (bInOrder == false)
            {

            }
            else
            {
                //先定位，再用insert
                int nPosition = this.GetPosition(strName);
                nOutputPosition = nPosition;
            }

            _domRecord.InsertField(nOutputPosition,
    strName,
    strIndicator,
    strValue);

            /*
            if (bFireTextChanged == true)
            {
                // 文档发生改变
                this.MarcEditor.FireTextChanged();
            }
            */
            return this[nOutputPosition];
        }


        // 对内
        // 追加一个新字段，供外部使用
        // 该函数不仅处理内存对象，还处理界面的事情
        // 操作历史: 无
        // parameters:
        //		strName	新字段名称
        //		strIndicator	新字段指示符
        //		strValue	新字段值(里面可能包含正规的字段指示符ASCII31)
        /// <summary>
        /// 新增一个字段对象
        /// </summary>
        /// <param name="strName">字段名</param>
        /// <param name="strIndicator">字段指示符</param>
        /// <param name="strValue">字段内容</param>
        /// <param name="bInOrder">是否要按照字段名顺序插入到适当位置</param>
        /// <returns>新增的字段对象</returns>
        public Field Add(string strName,
            string strIndicator,
            string strValue,
            bool bInOrder)
        {
            // strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            var field = this.AddInternal(strName,
                strIndicator,
                strValue,
                true,
                bInOrder,
                out _);

            // 界面失效区域

            // 根据字段类型，设焦点位置
            if (field.Name == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.Name) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            return field;
        }

        // 根据字段名进行定位
        private int GetPosition(string strFieldName)
        {
            if (this.Count == 0)
                return 0;

            int nBaseIndex = 0;
            for (int i = 0; i < this.Count; i++)
            {
                Field field = this[i];
                if (String.Compare(field.Name, strFieldName) <= 0)  // < 2009/7/3 changed
                {
                    nBaseIndex = i;
                }
            }

            return nBaseIndex;
        }

        //--------------------------前插字段-------------------

        // 根据机内格式的表示多个字段的字符串，给nIndex位置前新增多个字段
        // 操作历史: 无
        // parameters:
        //		nIndex	位置
        //		strFieldMarc	marc字符串
        //		nNewFieldCount	out参数，新增了几个字段
        internal void InsertInternal(int nIndex,
            string strFieldsMarc,
            out int nNewFieldsCount)
        {
            nNewFieldsCount = 0;

            // 先找到有几个字段
            // strFieldsMarc = strFieldsMarc?.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);
            List<string> fields = Record.GetFields(strFieldsMarc, out _);
            if (fields == null || fields.Count == 0)
                return;

            nNewFieldsCount = fields.Count;

            this.InsertInternal(nIndex, fields);
        }

        public void InsertFields(int nIndex,
            List<string> fields)
        {
            InsertInternal(nIndex, fields);
        }

        // 操作历史: 无
        internal void InsertInternal(int nIndex,
            List<string> fields)
        {
            if (fields == null || fields.Count == 0)
                return;

            foreach (var text in fields)
            {
                _domRecord.InsertField(nIndex, text);
                nIndex++;
            }

            // 文档发生变化
            // this.MarcEditor.FireTextChanged();
        }

        // 前插一个新字段，可供内部或外部使用，
        // 该函数不仅处理内存对象，还处理界面的事情
        // 操作历史: 无
        // parameters:
        //		nIndex	参考的位置
        //		strName	新字段名称
        //		strIndicator	新字段指示符
        //		strValue	新字段值(里面可能包含正规的字段指示符ASCII31)
        // 说明:函数内部调InsertField(nIndex,field)版本
        /// <summary>
        /// 在指定位置插入一个新的字段对象
        /// </summary>
        /// <param name="nIndex">插入位置</param>
        /// <param name="strName">字段名</param>
        /// <param name="strIndicator">字段指示符</param>
        /// <param name="strValue">字段内容</param>
        /// <returns>新增的字段对象</returns>
        public Field Insert(int nIndex,
            string strName,
            string strIndicator,
            string strValue)
        {
            // strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            Field field = new Field(this,
                strName.PadLeft(3, '*'),
                strIndicator,
                strValue,
                nIndex == 0 ? true : false);

            this.Insert(nIndex,
                field);
            return this[nIndex];
        }

        // 前插一个字段
        // 操作历史: 无
        /// <summary>
        /// 插入一个字段对象
        /// </summary>
        /// <param name="nIndex">位置</param>
        /// <param name="field">字段对象</param>
        public void Insert(int nIndex,
            Field field)
        {
            // Debug.Assert(nIndex > this.Count, "nIndex参数不合法");

            _domRecord.InsertField(nIndex,
                field.Text);    // TODO: 注意检查 field.Text 末尾是否有字段结束符
            _domRecord.GetFieldOffsRange(nIndex,
                1,
                out int start,
                out int end);
            _domRecord.Select(start, end, end);
        }

        //--------------------------后插字段-------------------

        // 后插一个新字段，可供内部或外部使用，
        // 该函数不仅处理内存对象，还处理界面的事情
        // 操作历史: 无
        // parameters:
        //		nIndex	参考的位置
        //		strName	新字段名称
        //		strIndicator	新字段指示符
        //		strValue	新字段值(里面可能包含正规的字段指示符ASCII31)
        // 注意: 本函数自动解决指示符的定位问题
        /// <summary>
        /// 插入一个新的字段对象，在参考位置的后面
        /// </summary>
        /// <param name="nIndex">插入位置。字段对象将插入到这个位置的后面</param>
        /// <param name="strName">字段名</param>
        /// <param name="strIndicator">字段指示符</param>
        /// <param name="strValue">字段内容</param>
        /// <returns>新插入的字段对象</returns>
        public Field InsertAfter(int nIndex,
            string strName,
            string strIndicator,
            string strValue)
        {
            // strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            Field field = new Field(this,
                strName,
                strIndicator,
                strValue);

            this.InsertAfter(nIndex,
                field);

            return this[nIndex+1];
        }

        // 后插字段
        // 操作历史: 无
        /// <summary>
        /// 插入一个新的字段对象，在参考位置的后面
        /// </summary>
        /// <param name="nIndex">插入位置。字段对象将插入到这个位置的后面</param>
        /// <param name="field">字段对象</param>
        public void InsertAfter(int nIndex,
            Field field)
        {
            this.Insert(nIndex + 1,
                field);
        }

        //--------------------------删除字段-------------------


        // 删除一个字段，可供内部或外部使用，
        // 该函数不仅处理内存对象，还处理界面的事情
        // 操作历史: 无
        // parameters:
        //		nFieldIndex	字段索引号
        /// <summary>
        /// 在指定位置删除一个字段对象
        /// </summary>
        /// <param name="nFieldIndex">位置</param>
        public void RemoveAt(int nFieldIndex)
        {
            _domRecord.DeleteField(nFieldIndex);
        }


        #region 仅操作集合，不触发任何事件

        /*
        internal void _removeAt(int field_index)
        {
            this.RemoveAt(field_index);
        }

        internal void _insert(int field_index, Field field)
        {
            this.Insert(field_index, field);
        }
        */

        public int IndexOf(Field item)
        {
            return item.Index;
        }


        public void Add(Field item)
        {
            _domRecord.InsertField(_domRecord.FieldCount,
                item.Name,
                item.Indicator,
                item.Value);
        }

        public void Clear()
        {
            _domRecord.Clear();
        }

        public bool Contains(Field item)
        {
            if (item.IsDeleted)
                return false;
            var index = item.Index;
            if (index >= 0 && index < this.Count)
                return true;
            return false;
        }

        void ICollection<Field>.CopyTo(Field[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Field item)
        {
            var ret = Contains(item);
            if (ret == false)
                return false;
            RemoveAt(item.Index);
            return true;
        }

        IEnumerator<Field> IEnumerable<Field>.GetEnumerator()
        {
            foreach (var field in _domRecord)
            {
                yield return new Field(this, field);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var field in _domRecord)
            {
                yield return new Field(this, field);
            }
        }

        #endregion
    }

    /// <summary>
    /// 字段排序接口
    /// </summary>
    internal class FieldComparer : IComparer<Field>
    {
        int IComparer<Field>.Compare(Field x, Field y)
        {
            if (x.Name == y.Name)
                return 0;

            if (x.IsHeader)
                return -1;
            if (y.IsHeader)
                return 1;

            // 把符号'-'替换为'/'，这样就比'0'还小
            return string.Compare(x.Name.Replace("-", "/"), y.Name.Replace("-", "/"));
        }
    }
}
