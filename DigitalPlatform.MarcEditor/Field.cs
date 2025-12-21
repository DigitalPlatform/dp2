using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;

using LibraryStudio.Forms;


namespace DigitalPlatform.Marc
{
    // 字段
    /// <summary>
    /// 字段对象
    /// </summary>
	public class Field
    {
        public static List<Field> Clone(IEnumerable<Field> fields)
        {
            var results = new List<Field>();
            foreach (var field in fields)
            {
                results.Add(field.Clone());
            }

            return results;
        }

        private string m_strName = "";
        private string m_strIndicator = "";
        private string m_strValue = "";
        private bool m_bIsHeader = false;

        public Field Clone()
        {
            var result = new Field
            {
                _domField = null,   // 表示这是临时克隆的
                container = this.container,
                m_strName = this.Name,
                m_strIndicator = this.Indicator,
                m_strValue = this.Value,
                // PureHeight = PureHeight,
                Selected = Selected,
            };
            return result;
        }

        /// <summary>
        /// 容器，也就是当前字段对象所从属的字段对象数组
        /// </summary>
		internal FieldCollection container = null;

        /*
        internal string m_strNameCaption = "字段说明";
        */

        // internal int PureHeight = 20;

        /// <summary>
        /// 当前对象是否处在选中状态
        /// </summary>
		public bool Selected = false;

        DomField _domField = null;

        /// <summary>
        /// 构造函数
        /// </summary>
		public Field()
        {
        }

        public Field(FieldCollection field_collection,
            DomField domField)
        {
            this.container = field_collection;
            this._domField = domField;
        }

        // 构造克隆状态的对象
        public Field(FieldCollection field_collection,
            string name,
            string indicator,
            string content,
            bool is_header = false)
        {
            this.container = field_collection;
            this._domField = null;
            m_strName = name;
            m_strIndicator = indicator;
            m_strValue = content;
            m_bIsHeader = is_header;
        }

        public int Index
        {
            get
            {
                if (_domField == null)
                    return -1;
                return _domField.Index;
            }
        }

        public bool IsDeleted
        {
            get
            {
                if (_domField == null)
                    return false;
                return _domField.IsDeleted;
            }
        }

        public bool Equal(Field field)
        {
            if (_domField == null 
                && field._domField == null)
            {
                return this.Text == field.Text;
            }

            if (_domField == null || field._domField == null)
                throw new InvalidOperationException("非克隆和克隆对象无法直接比较");

            if (this.Index == field.Index)
                return true;
            return false;
        }

        // 字段名
        /// <summary>
        /// 获取或设置字段名
        /// </summary>
        public string Name
        {
            get
            {
                if (_domField == null)
                {
                    if (m_bIsHeader)
                        return "###";
                    return m_strName;
                }
                if (_domField.IsHeader)
                    return "###";
                return _domField.Name;
            }
            set
            {
                if (_domField == null)
                    m_strName = value;
                else
                    _domField.Name = value;
            }
        }

        // 字段指示符
        /// <summary>
        /// 获取或设置字段指示符
        /// </summary>
        public string Indicator
        {
            get
            {
                if (_domField == null)
                    return m_strIndicator;
                return _domField.Indicator;
            }
            set
            {
                if (_domField == null)
                    m_strIndicator = value;
                else
                    _domField.Indicator = value;
            }
        }

#if REMOVED
        // 字段值。get没有替换^符号
        internal string ValueKernel
        {
            get
            {
                return this.m_strValue;
            }
        }
#endif

        /// <summary>
        /// 获取或设置 字段指示符联合字段内容
        /// </summary>
        public string IndicatorAndValue
        {
            get
            {
                if (_domField == null)
                    return m_strIndicator + m_strValue;
                return _domField.Indicator + _domField.Content;
            }
            set
            {
                if (Record.IsControlFieldName(this.Name) == true)
                {
                    this.Value = value;
                    return;
                }

                if (value.Length >= 2)
                {
                    this.Indicator = value.Substring(0, 2);
                    this.Value = value.Substring(2);
                }
                else
                {
                    this.Indicator = value.PadRight(2, ' ');    // 填充空白
                    this.Value = "";
                }
            }
        }

        // 字段值。get替换了^符号
        /// <summary>
        /// 获取或设置 字段值，也就是字段内容
        /// </summary>
        public string Value
        {
            get
            {
                if (_domField == null)
                    return m_strValue;
                return _domField.Content;
            }
            set
            {
                if (_domField == null)
                    m_strValue = value;
                else
                    _domField.Content = value;
            }
        }

#if REMOVED
        // 本字段总高度
        /// <summary>
        /// 获取本字段的显示区域的高度
        /// </summary>
        public int TotalHeight
        {
            get
            {
                return _domField.GetMarcField().GetPixelHeight();
            }
        }
#endif

        /// <summary>
        /// 获取或设置本字段的 MARC 字符串 (机内格式)
        /// </summary>
        public string Text
        {
            get
            {
                if (_domField == null)
                    return m_strName + m_strIndicator + m_strValue;
                return _domField.Text;
            }
            set
            {
                if (_domField == null)
                    _setCloneText(value);
                else
                    _domField.Text = value;
            }
        }

        // 设置克隆对象的内容
        void _setCloneText(string text)
        {
            if (_domField != null)
                throw new InvalidOperationException("必须是克隆对象才能使用 _setCloneText()");

            LibraryStudio.Forms.MarcField.ParseFieldParts(text,
                m_bIsHeader,
                true,
                out string name,
                out string indicator,
                out string content);

            m_strName = name;
            m_strIndicator = indicator;
            m_strValue = content;
        }

        public bool IsHeader
        {
            get
            {
                if (_domField == null)
                    return m_bIsHeader;
                return _domField.IsHeader;
            }
        }

        public bool IsControlField
        {
            get
            {
                if (_domField == null)
                    return MarcField.isControlFieldName(m_strName);
                return _domField.IsControlField;
            }
        }

        // 获得一个区域的，相对于字段头部的 offs 位置
        // 1: 字段名 2: 指示符 3:字段内容
        public int GetRegionCaretPos(int region)
        {
            if (_domField == null)
                return 0;
            if (region == 0 || region == 1)
                return 0;
            if (region == 2)
                return 3;
            if (region == 3)
            {
                if (this.IsControlField)
                    return 3;
                return 5;
            }
            return 0;
        }

        // 获得字段的MARC格式
        // parameters:
        //      bAddFLDEND  是否加字段结束符
        // return:
        //      字段的MARC字符串
        /// <summary>
        /// 获取本字段的 MARC 字符串 (机内格式)
        /// </summary>
        /// <param name="bAddFLDEND">是否包含字段结束符</param>
        /// <returns>MARC 字符串</returns>
		public string GetFieldMarc(bool bAddFLDEND)
        {
            if (this.IsHeader)
            {
                if (_domField == null)
                    return m_strValue;
                return _domField.Text;
            }

            if (_domField == null)
            {
                var value = m_strName + m_strIndicator + m_strValue;
                if (bAddFLDEND && value.Last() != Record.FLDEND)
                    value += Record.FLDEND;
                return value;
            }

            {
                var value = _domField.Text;
                if (bAddFLDEND && value.Last() != Record.FLDEND)
                    value += Record.FLDEND;

                // 需要把 Name Indicator 中不足的字符填足?
                if (this.IsControlField && value.Length < 3)
                    value = value.PadRight(3, ' ');
                else
                    value = value.PadRight(5, ' ');

                return value;
            }
        }

        /// <summary>
        /// 设置本字段的 MARC 字符串 (机内格式)
        /// </summary>
        /// <param name="strFieldMarc">本字段的 MARC 字符串</param>
        public void SetFieldMarc(string strFieldMarc)
        {
            SetFieldMarc(strFieldMarc, true);
        }

        /// <summary>
        /// 设置本字段的 MARC 字符串 (机内格式)
        /// </summary>
        /// <param name="strFieldMarc">本字段的 MARC 字符串</param>
        /// <param name="bFlushEdit">是否自动刷新小 Edit</param>
		internal void SetFieldMarc(string strFieldMarc,
            bool bFlushEdit)
        {
            if (this.container == null)
                return;

            if (this.IsHeader)
            {
                _domField.Content = strFieldMarc;
                return;
            }


            if (strFieldMarc.Length < 3)
                strFieldMarc = strFieldMarc + new string(' ', 3 - strFieldMarc.Length);

            string strName = "";
            string strIndicator = "";
            string strValue = "";

            strName = strFieldMarc.Substring(0, 3);
            if (Record.IsControlFieldName(strName) == true)
            {
                strIndicator = "";
                strValue = strFieldMarc.Substring(3);
            }
            else
            {
                if (strFieldMarc.Length < 5)
                    strFieldMarc = strFieldMarc + new string(' ', 5 - strFieldMarc.Length);

                strIndicator = strFieldMarc.Substring(3, 2);

                strValue = strFieldMarc.Substring(5);
            }

            this.Text = strName + strIndicator + strValue;
            // this.container.MarcEditor.FireTextChanged();
        }

        // 获得反相颜色
        static Color ReverseColor(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }

        // 子字段集合
        // 通过get得到的集合，remove其中的subfield对象，field中不能兑现。
        // 需要set那个remove后的集合回来，才能兑现
        /// <summary>
        /// 获取或设置子字段对象集合
        /// </summary>
		public SubfieldCollection Subfields
        {
            get
            {
                if (this.IsHeader)
                    return null;
                return SubfieldCollection.BuildSubfields(this);
            }
            set
            {
                if (value != null)
                {
                    value.Container = this;

                    value.Flush();  // Flush()中必定作了针对this的事情
                }
            }
        }

    }
}
