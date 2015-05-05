using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.Marc
{
    // 子字段
    /// <summary>
    /// 子字段对象
    /// </summary>
    public class Subfield
    {
        internal string m_strName = "";
        internal string m_strValue = "";

        /// <summary>
        /// 容器。
        /// SubfieldCollection 对象
        /// </summary>
        public SubfieldCollection Container = null;

        // 在字段内容中的偏移量
        /// <summary>
        /// 返回一个偏移量：当前子字段第一字符，在所从属的字段内容中的偏移量
        /// </summary>
        public int Offset
        {
            get
            {
                if (this.Container == null)
                    return -1;
                int v = 0;
                for (int i = 0; i < Container.Count; i++)
                {
                    Subfield subfield = Container[i];
                    if (subfield == this)
                    {
                        return v;
                    }

                    v += 1 + subfield.Name.Length + subfield.Value.Length;
                }

                return -1;  // 没有找到自己
            }
        }

        /// <summary>
        /// 获得或设置子字段名
        /// </summary>
        public string Name
        {
            get
            {
                return m_strName;
            }
            set
            {
                if (m_strName == value)
                    return;

                m_strName = value;
                if (this.Container != null)
                    this.Container.Flush();
            }
        }

        /// <summary>
        /// 获得或者设置当前子字段的 Value 值。也就是子字段内容部分，不包含子字段名
        /// </summary>
        public string Value
        {
            get
            {
                return m_strValue;
            }
            set
            {

                if (m_strValue == value)
                    return;

                m_strValue = value;
                if (this.Container != null)
                    this.Container.Flush();

            }
        }

        /// <summary>
        /// 获取或设置当前子字段的 Text 值。也就是 Name + Value 的字符串
        /// </summary>
        public string Text
        {
            get
            {
                return this.Name + this.Value;
            }

            set
            {
                if (value.Length == 0)
                {
                    if (this.m_strName == "" && this.m_strValue == "")
                        return;

                    this.m_strName = "";
                    this.m_strValue = "";

                    if (this.Container != null)
                        this.Container.Flush();

                    return;
                }
                this.m_strName = value[0].ToString();
                this.m_strValue = value.Substring(1);

                if (this.Container != null)
                    this.Container.Flush();
            }
        }

    }

}
