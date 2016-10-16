using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DigitalPlatform.Marc
{
    // 
    /// <summary>
    /// 子字段集合
    /// </summary>
    public class SubfieldCollection : ArrayList
    {
        /// <summary>
        /// 容器。 Field 对象
        /// </summary>
        public Field Container = null;

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
        //     index 小于零。- 或 -index 等于或大于 System.Collections.ArrayList.Count。
        /// <summary>
        /// 获取或设置指定索引处的 Subfield 元素。
        /// </summary>
        /// <param name="index">要获得或设置的元素从零开始的索引。</param>
        /// <returns>指定索引处的 Subfield 元素。</returns>
        public new Subfield this[int index]
        {
            get
            {
                return (Subfield)base[index];
            }
            set
            {
                base[index] = value;
                Flush();
            }

        }

        /// <summary>
        /// 获取或设置指定 Name 的 Subfield 元素。
        /// 获取的时候，如果有同名的子字段，则返回第一个。
        /// 设置的时候，如果尚不存在这个名字的子字段对象，则追加一个子字段对象；如果存在多个同名的子字段对象，则替换第一个
        /// </summary>
        /// <param name="strName">子字段名。一个字符的字符串</param>
        /// <returns>指定 Name 的 Subfield 元素。</returns>
        public Subfield this[string strName]
        {
            get
            {
                return GetSubfield(strName, 0);
            }
            set
            {
                Subfield subfield = GetSubfield(strName, 0);
                if (subfield == null)
                {
                    // throw(new Exception("未找到"));
                    this.Add(value);
                    return;
                }
                int i = this.IndexOf(subfield);
                base[i] = value;
                Flush();
            }
        }

        /// <summary>
        /// 获取或设置指定 Name 的 Subfield 元素。还能指定在同名子字段中是哪一个。
        /// 设置的时候，如果尚不存在这个名字的指定重复位置的子字段对象，则追加一个子字段对象
        /// </summary>
        /// <param name="strName">子字段名。一个字符的字符串</param>
        /// <param name="nDupIndex">在同名的子字段中的索引，从 0 开始计算</param>
        /// <returns>指定 Name 的 Subfield 元素。</returns>
        public Subfield this[string strName, int nDupIndex]
        {
            get
            {
                return GetSubfield(strName, nDupIndex);
            }
            set
            {
                Subfield subfield = GetSubfield(strName, nDupIndex);
                if (subfield == null)
                {
                    //throw(new Exception("未找到"));
                    this.Add(value);
                    return;
                }
                int i = this.IndexOf(subfield);
                base[i] = value;
                Flush();
            }

        }

        // parameters:
        //      strName 子字段名
        //      nDupIndex   第几个
        /// <summary>
        /// 根据子字段名和重复位置获取一个 Subfield 对象
        /// </summary>
        /// <param name="strName">子字段名。一个字符的字符串</param>
        /// <param name="nDupIndex">在同名的子字段中的索引，从 0 开始计算</param>
        /// <returns>Subfield 元素。</returns>
        public Subfield GetSubfield(string strName,
            int nDupIndex)
        {
            int nDup = 0;
            for (int i = 0; i < this.Count; i++)
            {
                Subfield subfield = this[i];
                if (subfield.Name == strName)
                {
                    if (nDupIndex == nDup)
                        return subfield;
                    nDup++;
                }

            }

            return null;
        }

        /// <summary>
        /// 根据一个 Field 对象的内容创建一个新的 SubfieldCollection 对象
        /// </summary>
        /// <param name="container">Field 对象，也被当作要创建的 SubfieldCollection 对象的容器</param>
        /// <returns>新创建的 SubfieldCollection 对象</returns>
        public static SubfieldCollection BuildSubfields(Field container)
        {
            SubfieldCollection subfields = new SubfieldCollection();

            string strField = container.Text;

            for (int i = 0; ; i++)
            {
                string strSubfield = "";
                string strNextSubfieldName = "";

                // 从字段或子字段组中得到一个子字段
                // parameters:
                //		strText		字段内容，或者子字段组内容。
                //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
                //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
                //					形式为'a'这样的。
                //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
                //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
                //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                int nRet = MarcUtil.GetSubfield(
                    strField,
                    ItemType.Field,
                    null,
                    i,
                    out strSubfield,
                    out strNextSubfieldName);
                if (nRet == -1 || nRet == 0)
                    break;

                Subfield subfield = new Subfield();
                subfield.Text = strSubfield;
                subfield.Container = subfields;

                subfields.Add(subfield);
            }

            subfields.Container = container;
            return subfields;
        }

        /// <summary>
        /// 将 Subfield 对象对象添加到 当前集合 的结尾处。
        /// </summary>
        /// <param name="subfield">要添加的 Subfield 对象</param>
        public void Add(Subfield subfield)
        {
            base.Add(subfield);

            Flush();
        }

        // 根据子字段名找到偏移量
        /// <summary>
        /// 根据子字段名找到它在集合中的偏移量
        /// </summary>
        /// <param name="strSubfieldName">子字段名</param>
        /// <returns>从 0 开始计算的偏移量。如果不存在，则返回 this.Count 值</returns>
        public int GetPosition(string strSubfieldName)
        {
            int nPosition = 0;
            // Debug.Assert(false, "");
            for (int i = 0; i < this.Count; i++)
            {
                nPosition = i;

                Subfield subfield = this[i];
                if (String.Compare(subfield.Name, strSubfieldName) > 0)
                    return nPosition;
            }

            return this.Count;  // 2007/7/10 changed
            // return nPosition;
        }

        // 需要做一个根据字符偏移量定位到子字段的函数 


        /// <summary>
        /// 将 Subfield 对象对象添加到 当前集合 的结尾处；或者按照子字段名字母顺序插入。
        /// </summary>
        /// <param name="subfield">要插入的 Subfield 对象</param>
        /// <param name="bInOrder">是否按照子字段名字母顺序插入。如果为 false，则表示追加到集合末尾</param>
        public void Add(Subfield subfield,
            bool bInOrder)
        {
            if (bInOrder == true)
            {
                int nPosition = GetPosition(subfield.Name);
                base.Insert(nPosition, subfield);
            }
            else
            {
                base.Add(subfield);
            }

            this.Flush();
        }

        // 注：返回collection对象是为了方便set回Field.Subfields
        /// <summary>
        /// 从当前集合中移出一个 Subfield 对象
        /// </summary>
        /// <param name="subfield">要移出的 Subfield 对象</param>
        /// <returns>顺便返回当前集合对象</returns>
        public SubfieldCollection Remove(Subfield subfield)
        {
            base.Remove(subfield);

            Flush();

            return this;
        }

        /// <summary>
        /// 移除 当前集合 的指定索引处的 Subfield 元素。
        /// </summary>
        /// <param name="index">要移除的对象的索引</param>
        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);

            Flush();
        }

        /// <summary>
        /// 将 Subfield 元素插入 当前集合的 的指定索引处。
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="subfield">Subfield 对象</param>
        public void Insert(int index, Subfield subfield)
        {
            base.Insert(index, subfield);

            Flush();
        }

        /// <summary>
        /// 将集合元素的情况兑现到容器对象的 Value 成员中
        /// </summary>
        public void Flush()
        {
            if (this.Container == null)
                return;

            string strValue = "";
            for (int i = 0; i < this.Count; i++)
            {
                Subfield subfield = (Subfield)this[i];
                strValue += new string(Record.SUBFLD, 1) + subfield.Name + subfield.Value;
            }

            this.Container.Value = strValue;
        }
    }
}
