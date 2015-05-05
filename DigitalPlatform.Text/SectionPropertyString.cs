using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DigitalPlatform.Text.SectionPropertyString
{
    public class Item
    {
        public string Value;
        public bool Defined = false;

        public string PureValue
        {
            get
            {
                if (String.IsNullOrEmpty(this.Value))
                    return this.Value;
                if (this.Value[0] == '-' || this.Value[0] == '+')
                    return this.Value.Substring(1);

                return this.Value;
            }
        }

        public bool Enabled
        {
            get
            {
                if (String.IsNullOrEmpty(this.Value))
                    return false;
                if (this.Value[0] == '-')
                    return false;
                return true;
            }

            set
            {
                if (String.IsNullOrEmpty(this.Value))
                    return;
                if (value == true)
                {
                    if (this.Value[0] == '-')
                        this.Value = "+" + this.Value.Substring(1);
                }
                else
                {
                    if (this.Value[0] == '-')
                        return;

                    if (this.Value[0] == '+')
                        this.Value = "-" + this.Value.Substring(1);
                    else
                        this.Value = "-" + this.Value;
                }
            }

        }
    }

    // 一个小节
    public class Section : List<Item>
    {
        public string Name = "";
        public string DefaultSectionName = "";

        public Section()
        {
        }

        public Section(string strDefaultSectionName,
            string strSection)
        {
            Build(strDefaultSectionName,
                strSection);
        }

        public string Value
        {
            get
            {
                if (this.Count == 0)
                    return "";

                string strResult = "";
                for (int i = 0; i < this.Count; i++)
                {
                    if (strResult != "")
                        strResult += ",";
                    strResult += this[i].Value;
                }

                return strResult;
            }

            set
            {
                this.BuildValue(value);
            }
        }

        // 不影响Name，只更新属性值列表
        public void BuildValue(string strSectionValue)
        {
            this.Clear();

            int nRet = strSectionValue.IndexOf(":");
            if (nRet != -1)
                throw(new Exception("BuildValue()的strSectionValue参数中不能包含冒号(是否应使用Build()方法?)"));

    
            // 根据逗号拆分
            string[] values = strSectionValue.Split(new char[] { ',' });

            for (int j = 0; j < values.Length; j++)
            {
                string strValue = values[j];
                if (String.IsNullOrEmpty(strValue) == true)
                    continue;

                Item item = new Item();
                item.Value = strValue;
                item.Defined = false;

                this.Add(item);
            }
        }


        public void Build(string strDefaultSectionName,
            string strSection)
        {
            this.Clear();

            DefaultSectionName = strDefaultSectionName;

            // 析出名字
            string strValues = "";
            int nRet = strSection.IndexOf(":");
            if (nRet == -1)
            {
                this.Name = strDefaultSectionName;
                strValues = strSection;
            }
            else
            {
                this.Name = strSection.Substring(0, nRet).Trim();
                strValues = strSection.Substring(nRet + 1).Trim();
            }

            // 根据逗号拆分
            string[] values = strValues.Split(new char[] { ',' });

            for (int j = 0; j < values.Length; j++)
            {
                string strValue = values[j];
                if (String.IsNullOrEmpty(strValue) == true)
                    continue;

                Item item = new Item();
                item.Value = strValue;
                item.Defined = false;

                this.Add(item);
            }
        }

        public ItemState GetItemState(string strPureValue)
        {
            for (int i = 0; i < this.Count; i++)
            {
                Item item = this[i];
                if (String.Compare(item.PureValue, strPureValue) == 0)
                {
                    if (item.Enabled == true)
                        return ItemState.On;
                    return ItemState.Off;
                }
            }

            return ItemState.NotExist;
        }

        // 新增一个事项。如果strValue和已经存在的值匹配，则返回已经存在的事项，不创建新事项
        public Item NewItem(string strPureValue,
            ItemState itemstate)
        {
            Debug.Assert(itemstate != ItemState.NotExist, "创建新对象不能使用NotExist状态");
            Item item = null;
            for (int i = 0; i < this.Count; i++)
            {
                if (String.Compare(this[i].PureValue, strPureValue) == 0)
                {
                    item = this[i];
                    goto FOUND;
                }
            }

            item = new Item();
            this.Add(item);
            FOUND:
            item.Value = strPureValue;
            item.Enabled = (itemstate == ItemState.On ? true : false);

            return item;
        }

        public override string ToString()
        {
            if (this.Count == 0)
                return "";

            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                if (strResult != "")
                    strResult += ",";
                strResult += this[i].Value;
            }

            return (String.IsNullOrEmpty(this.Name) == true ? this.DefaultSectionName : this.Name)
                + ":" + strResult;
        }
    }

    // 整个配置字符串
    public class PropertyCollection : List<Section>
    {
        public PropertyCollection()
        {
         
        }
        public PropertyCollection(string strDefaultSectionName,
            string strPropertyString,
            DelimiterFormat format)
        {
            Build(strDefaultSectionName,
                strPropertyString,
                format);
        }

        public void Build(string strDefaultSectionName,
            string strPropertyString,
            DelimiterFormat format)
        {
            this.Clear();

            // 根据分号拆分
            string[] sections = null;
            
            if (format == DelimiterFormat.Semicolon)
                sections = strPropertyString.Split(new char[] { ';' });
            else if (format == DelimiterFormat.CrLf)
            {
                sections = strPropertyString.Replace("\r\n", "\r").Split(new char[] { '\r' });
            }
            else if (format == DelimiterFormat.Mix)
            {
                sections = strPropertyString.Replace("\r\n", "\r").Split(new char[] { '\r',';' });
            }
            else
            {
                throw new Exception("不支持的SeperatorFormat类型: " + format.ToString());
            }


            for (int i = 0; i < sections.Length; i++)
            {
                string strSection = sections[i].Trim();

                if (String.IsNullOrEmpty(strSection))
                    continue;

                Section section = new Section(strDefaultSectionName, strSection);

                this.Add(section);
            }
        }

        public Section this[string strCategory]
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (String.Compare(this[i].Name, strCategory, true) == 0)
                        return this[i];
                }

                return null;
            }
            set
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (String.Compare(this[i].Name, strCategory, true) == 0)
                    {
                        this[i] = value;
                        return;
                    }
                }

                this.Add(value);
            }
        }

        public override string ToString()
        {
            return this.ToString(DelimiterFormat.Semicolon);
        }
 
        // 输出全部字符串
        public string ToString(DelimiterFormat format)
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                string strSection = this[i].ToString();
                if (strSection == "")
                    continue;

                if (strResult != "")
                {
                    if (format == DelimiterFormat.Semicolon)
                        strResult += ";";
                    else if (format == DelimiterFormat.CrLf)
                        strResult += "\r\n";
                    else if (format == DelimiterFormat.Mix)
                        strResult += ";\r\n";
                    else
                    {
                        throw new Exception("不支持的SeperatorFormat类型: " + format.ToString());
                    }
                }

                strResult += strSection;
            }

            return strResult;
        }

        // 按照类目输出局部字符串
        public string ToString(DelimiterFormat format,
            string strCategory)
        {
            if (String.IsNullOrEmpty(strCategory))
                strCategory = "*";

            if (strCategory == "*")
                return this.ToString(format);

            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                // 选择目录
                if (String.Compare(this[i].Name, strCategory, true) != 0)
                    continue;

                string strSection = this[i].ToString();
                if (strSection == "")
                    continue;

                if (strResult != "")
                {
                    if (format == DelimiterFormat.Semicolon)
                        strResult += ";";
                    else if (format == DelimiterFormat.CrLf)
                        strResult += "\r\n";
                    else if (format == DelimiterFormat.Mix)
                        strResult += ";\r\n";
                    else
                    {
                        throw new Exception("不支持的SeperatorFormat类型: " + format.ToString());
                    }
                } 
                
                strResult += strSection;
            }

            return strResult;
        }

        public ItemState GetItemState(string strCategory,
            string strValue)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (String.Compare(this[i].Name, strCategory, true) == 0)
                    return this[i].GetItemState(strValue);

            }

            return ItemState.NotExist;
        }

        // 新创建一个事项。会自动查重，不会创建重复的section和item
        public Item NewItem(string strCategory,
            string strPureValue,
            ItemState itemstate)
        {
            Debug.Assert(itemstate != ItemState.NotExist, "创建新对象不能使用NotExist状态");

            Section section = null;
            for (int i = 0; i < this.Count; i++)
            {
                // 选择目录
                if (String.Compare(this[i].Name, strCategory, true) == 0)
                {
                    section = this[i];
                    goto FOUNDSECTION;
                }
            }

            section = new Section();
            section.Name = strCategory;
            this.Add(section);

        FOUNDSECTION:
            return section.NewItem(strPureValue, itemstate);
        }

        // 新创建一个Section。会自动查重，不会创建重复的section
        public Section NewSection(
            string strDefaultSectionName,
            string strCategory,
            string strSection)
        {
            Section section = null;
            for (int i = 0; i < this.Count; i++)
            {
                // 选择目录
                if (String.Compare(this[i].Name, strCategory, true) == 0)
                {
                    section = this[i];
                    goto FOUNDSECTION;
                }
            }

            section = new Section();
            section.Name = strCategory;
            this.Add(section);

        FOUNDSECTION:
            section.Build(strDefaultSectionName, strSection);
            return section;
        }

    }

    // 小节之间分隔符的格式
    public enum DelimiterFormat
    {
        Semicolon = 0,  // 分号间隔
        CrLf = 1,   // 回车换行符号间隔
        Mix = 2,    // 对于输出：两者皆有，即先分号然后紧跟回车换行；对于输入：仅有其中一种符号或者两种符号混合，都能识别
    }

    public enum ItemState
    {
        NotExist = 0,
        On = 1,
        Off = 2,
    }
    // 增补菜单
    public delegate void IsDefinedEventHandle(object sender,
    IsDefinedEventArgs e);

    public class IsDefinedEventArgs : EventArgs
    {
        public Item Item = null;
        public Section Section = null;
        public PropertyCollection PropertyCollection = null;
    }
}
