using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

using DigitalPlatform.Text;

namespace DigitalPlatform.Marc
{
    // 
    /// <summary>
    /// 字段集合
    /// </summary>
    public class FieldCollection : List<Field>
    {
        /// <summary>
        /// 所从属的记录对象
        /// </summary>
        internal Record record = null;

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="comparer">排序接口</param>
        public void Sort(IComparer comparer)
        {

        }

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
            FieldCollection fields = this.GetFields(strFieldName, strIndicatorMatch);
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
        public FieldCollection GetFields(string strFieldName,
            string strIndicatorMatch = "**")
        {
            FieldCollection fields = new FieldCollection();
            foreach (Field field in this)
            {
                if (field.m_strName == strFieldName)
                {
                    if (strIndicatorMatch != "**"
                        && string.IsNullOrEmpty(field.m_strIndicator) == false
                        && MarcUtil.MatchIndicator(strIndicatorMatch, field.m_strIndicator) == false)
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
            FieldCollection fields = this.GetFields(strFieldName, strIndicatorMatch);
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

        /// <summary>
        /// 设置指定字段名的第一个字段内的指定子字段名的第一个子字段的值。
        /// 如果不存在这样的字段和子字段，则次第创建
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <param name="strSubfieldValue">子字段值</param>
        public void SetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strSubfieldValue)
        {
            Field field = this.GetOneField(strFieldName, 0);
            if (field == null)
            {
                field = this.Add(strFieldName,
                    "  ",
                    "",
                    true);
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
        }

        internal MarcEditor MarcEditor
        {
            get
            {
                return this.record.marcEditor;
            }
        }

        //--------------------------追加字段-------------------

        // 追加一个新字段，只供内部使用，
        // 因为该函数只处理内存对象，不涉及界面的事情
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

#if DEBUG
            if (strValue.IndexOf((char)31) != -1)
                Debug.Assert(false, "AddInternal()函数的strValue参数中不应包含ASCII 31");
#endif

            string strCaption = this.record.marcEditor.GetLabel(strName);

            Field field = new Field(this);
            field.m_strNameCaption = strCaption;
            field.m_strName = strName;
            field.m_strIndicator = strIndicator;
            field.m_strValue = strValue;
            if (this.Count == 0)
            {
                field.m_strName = "###";
                field.m_strIndicator = "";
                field.m_strValue = field.m_strValue.PadRight(24, '?');
            }

            field.CalculateHeight(null, false);
            if (bInOrder == false)
            {
                base.Add(field);
            }
            else
            {
                //先定位，再用insert
                int nPosition = this.GetPosition(field.Name);
                this.InsertInternal(nPosition + 1,
                    field);
                nOutputPosition = nPosition;
            }

            if (bFireTextChanged == true)
            {
                // 文档发生改变
                this.MarcEditor.FireTextChanged();
            }

            return field;
        }


        // 对内
        // 追加一个新字段，供外部使用
        // 该函数不仅处理内存对象，还处理界面的事情
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
            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            int nOutputPosition = -1;
            Field field = this.AddInternal(strName,
                strIndicator,
                strValue,
                true,
                bInOrder,
                out nOutputPosition);


            // 界面失效区域

            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            if (nOutputPosition == -1)
            {
                iRect.rect = this.MarcEditor.GetItemBounds(this.Count - 1,
                    1,
                    BoundsPortion.FieldAndBottom);
            }
            else
            {
                if (this.MarcEditor.FocusedFieldIndex > nOutputPosition)
                {
                    this.MarcEditor.SelectedFieldIndices[0] = (int)this.MarcEditor.SelectedFieldIndices[0] + 1;
                }
                iRect.rect = this.MarcEditor.GetItemBounds(nOutputPosition,
                    -1,
                    BoundsPortion.FieldAndBottom);
            }

            // 根据字段类型，设焦点位置
            if (field.m_strName == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.m_strName) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            //this.marcEditor.ActiveField(field,3);

            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);

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

        // 前插一个新字段，只供内部使用，
        // 因为该函数只处理内存对象，不涉及界面的事情
        // parameters:
        //		nIndex	参考的位置
        //		strName	新字段名称
        //		strIndicator	新字段指示符
        //		strValue	新字段值
        // return:
        //		void
        internal Field InsertInternal(int nIndex,
            Field field)
        {
            if (this.Count == 0)
                throw new Exception("当前 MARC 记录没有头标区。请先创建头标区");

            Debug.Assert(nIndex <= this.Count, "nIndex ["+nIndex.ToString()+"] 不合法");

            string strCaption = this.MarcEditor.GetLabel(field.m_strName);

            field.m_strNameCaption = strCaption;
            field.container = this;
            field.CalculateHeight(null, false);
            if (this.Count == 0)
            {
                field.m_strName = "###";
                field.m_strIndicator = "";
                field.m_strValue = field.m_strValue.PadRight(24, '?');
            }

            base.Insert(nIndex, field);

            if (this.MarcEditor.curEdit != null)
                this.MarcEditor.curEdit.ContentIsNull = true;    // 防止后面调用时送回内存 2009/7/3

            // 文档发生改变
            this.MarcEditor.FireTextChanged();

            return field;
        }

        // 根据通过的Marc字段串，给nIndex位置前新增多个字段
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
            strFieldsMarc = strFieldsMarc.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);
            List<string> fields = Record.GetFields(strFieldsMarc);
            if (fields == null || fields.Count == 0)
                return;

            nNewFieldsCount = fields.Count;

            this.InsertInternal(nIndex, fields);
        }

        internal void InsertInternal(int nIndex,
            List<string> fields)
        {
            if (fields == null || fields.Count == 0)
                return;

            // 2007/7/17
            // 把小edit控件隐藏
            this.MarcEditor.HideTextBox();

            // 2014/7/10
            if (this.MarcEditor.curEdit != null)
                this.MarcEditor.curEdit.ContentIsNull = true;    // 防止后面调用时送回内存

            // 如果插入的第一个元素涉及到头标区，要进行特殊的处理
            // 2009/3/5
            if (nIndex == 0)
            {
                Debug.Assert(fields.Count > 0, "");
                if (fields[0].Length > 24)
                {
                    string strValue = fields[0];
                    string strHeader = strValue.Substring(0, 24);
                    string strOther = strValue.Substring(24);
                    fields[0] = strHeader;
                    fields.Insert(1, strOther);
                }
            }

            // 把多个字段加进去一个临时数组里
            // List<Field> aField = new List<Field>();
            int nTempIndex = nIndex;
            for (int i = 0; i < fields.Count; i++)
            {
                Field field = new Field(this);
                base.Insert(nTempIndex, field);
                nTempIndex++;
                // aField.Add(field);
                field.SetFieldMarc(fields[i], false);

                // if (this.Count == 0 && i == 0)
                if (nTempIndex == 1)
                {
                    field.m_strName = "###";
                    field.m_strIndicator = "";
                    field.m_strValue = field.m_strValue.PadRight(24, '?');
                }
            }

            /*
            // 插入了记录中
            this.InsertRange(nIndex, aField);
             * */

            int nTailIndex = -1;
            if (this.MarcEditor.SelectedFieldIndices.Count > 0)
                nTailIndex = this.MarcEditor.SelectedFieldIndices[this.MarcEditor.SelectedFieldIndices.Count - 1];

            // 2007/7/17
            // 焦点字段下标也被推动
            if (nTailIndex != -1)
            {
                if (nIndex <= nTailIndex)
                {
                    // this.MarcEditor.FocusedFieldIndex += fields.Count;

                    // 2014/7/10
                    for (int i = 0; i < this.MarcEditor.SelectedFieldIndices.Count; i++)
                    {
                        this.MarcEditor.SelectedFieldIndices[i] += fields.Count;
                    }
                }
            }


            // 文档发生变化
            this.MarcEditor.FireTextChanged();

            // 2007/7/17
            // 插入后，小编辑器位置可能被推动？
            this.MarcEditor.SetEditPos();

#if NO
            // 把新内容赋到小edit控件里
            this.MarcEditor.ItemTextToEditControl();
#endif
        }

        // 前插一个新字段，可供内部或外部使用，
        // 该函数不仅处理内存对象，还处理界面的事情
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
            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            Field field = new Field(this);
            field.m_strName = strName.PadLeft(3, '*');
            field.m_strIndicator = strIndicator;
            field.m_strValue = strValue;

            this.Insert(nIndex,
                field);

            return field;
        }

        // 前插一个字段
        /// <summary>
        /// 插入一个字段对象
        /// </summary>
        /// <param name="nIndex">位置</param>
        /// <param name="field">字段对象</param>
        public /*override*/ new void Insert(int nIndex,
            Field field)
        {
            Debug.Assert(nIndex <= this.Count, "nIndex参数不合法");
            // Debug.Assert(oValue is Field, "必须为Field类型");
            // Field field = (Field)oValue;

            // 把内容还原，把当前焦点设为空，省得Active时把下标搞错
            this.MarcEditor.ClearSelectFieldIndices();

            this.InsertInternal(nIndex,
                field);

            // 根据字段类型，设焦点位置
            if (field.m_strName == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.m_strName) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            // 失效范围
            int nStartIndex = 0;
            if (nIndex > 0)
                nStartIndex = nIndex - 1;
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.MarcEditor.GetItemBounds(nStartIndex,
                -1,
                BoundsPortion.FieldAndBottom);
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);
        }

        //--------------------------后插字段-------------------

        // 后插一个新字段，可供内部或外部使用，
        // 该函数不仅处理内存对象，还处理界面的事情
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
            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

            Field field = new Field();
            field.m_strName = strName;
            field.m_strIndicator = strIndicator;
            field.m_strValue = strValue;

            this.InsertAfter(nIndex,
                field);

            return field;
        }

        // 后插字段
        /// <summary>
        /// 插入一个新的字段对象，在参考位置的后面
        /// </summary>
        /// <param name="nIndex">插入位置。字段对象将插入到这个位置的后面</param>
        /// <param name="field">字段对象</param>
        public void InsertAfter(int nIndex,
            Field field)
        {
            Debug.Assert(nIndex < this.Count, "InsertAfterField(),nIndex参数不合法");

            // 内存对象加一个
            this.InsertInternal(nIndex + 1,
                field);

            // 根据字段类型，设焦点位置
            if (field.m_strName == this.MarcEditor.DefaultFieldName)
                this.MarcEditor.SetActiveField(field, 1);
            else if (Record.IsControlFieldName(field.m_strName) == true)
                this.MarcEditor.SetActiveField(field, 3);
            else
                this.MarcEditor.SetActiveField(field, 2);

            // 失效从当前新增字段到末尾的区域
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.MarcEditor.GetItemBounds(nIndex + 1,
                -1,
                BoundsPortion.FieldAndBottom);
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Horz,
                iRect);
        }

        //--------------------------删除字段-------------------

        // 删除一个字段，只供内部使用，
        // 因为该函数只处理内存对象，不涉及界面的事情
        // parameters:
        //		nFieldIndex	字段索引号
        // return:
        //		void
        internal void RemoveAtInternal(int nFieldIndex)
        {
            base.RemoveAt(nFieldIndex);

            // 文档发生改变
            this.MarcEditor.FireTextChanged();
        }

        // 删除一个字段，可供内部或外部使用，
        // 该函数不仅处理内存对象，还处理界面的事情
        // parameters:
        //		nFieldIndex	字段索引号
        /// <summary>
        /// 在指定位置删除一个字段对象
        /// </summary>
        /// <param name="nFieldIndex">位置</param>
        public /*override*/ new void RemoveAt(int nFieldIndex)
        {
            this.MarcEditor.Flush();
            this.RemoveAtInternal(nFieldIndex);

            // 把小edit控件隐藏
            this.MarcEditor.SelectedFieldIndices.Remove(nFieldIndex);
            this.MarcEditor.HideTextBox();

            if (nFieldIndex < this.Count)
                this.MarcEditor.SetActiveField(nFieldIndex, this.MarcEditor.m_nFocusCol);

            // 应把失效区域计算出来，进行优化
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            //iRect.rect = 
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Vert,
                iRect);
        }

        /// <summary>
        /// 删除若干字段对象
        /// </summary>
        /// <param name="fieldIndices">位置下标数组</param>
        public void RemoveAt(int[] fieldIndices)
        {
            // 清除选中对象
            this.MarcEditor.ClearSelectFieldIndices();

            int nMixIndex = 1000;
            for (int i = 0; i < fieldIndices.Length; i++)
            {
                int nIndex = fieldIndices[i];
                if (nIndex < nMixIndex)
                    nMixIndex = nIndex;
                this[nIndex] = null;
            }

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] == null)
                {
                    this.RemoveAtInternal(i);
                    i--;
                }
            }

            if (nMixIndex < this.Count)
                this.MarcEditor.SetActiveField(nMixIndex, this.MarcEditor.m_nFocusCol);

            // 应把失效区域计算出来，进行优化
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = true;
            this.MarcEditor.AfterDocumentChanged(ScrollBarMember.Vert,
                iRect);
        }
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

            if (x.Name == "hdr")
                return -1;
            if (y.Name == "hdr")
                return 1;

            // 把符号'-'替换为'/'，这样就比'0'还小
            return string.Compare(x.Name.Replace("-", "/"), y.Name.Replace("-", "/"));
        }
    }
}
