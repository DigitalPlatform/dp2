using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// NaviItem对象的类型
    /// </summary>
    enum NaviItemType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 虚拟根
        /// </summary>
        VirtualRoot = 1,
        /// <summary>
        /// 元素
        /// </summary>
        Element = 2,
        /// <summary>
        /// 属性
        /// </summary>
        Attribute = 3,
        /// <summary>
        /// 文本
        /// </summary>
        Text = 4,
    }

    /// <summary>
    /// 专门用于 Navigation 的节点
    /// </summary>
    class NaviItem
    {
        /// <summary>
        /// 节点类型
        /// </summary>
        public NaviItemType Type = NaviItemType.None;

        /// <summary>
        /// 相关的 MarcNode 节点
        /// </summary>
        public MarcNode MarcNode = null;

        /// <summary>
        /// 当前属性名
        /// </summary>
        public string AttrName = "";

        /// <summary>
        /// 当前文本值
        /// </summary>
        public string Text = "";

        // 复制构造函数
        /// <summary>
        /// 初始化一个 NaviItem 对象
        /// </summary>
        /// <param name="other">复制时参考的对象</param>
        public NaviItem(NaviItem other)
        {
            this.Type = other.Type;
            this.MarcNode = other.MarcNode;
            this.AttrName = other.AttrName;
            this.Text = other.Text;
        }

        /// <summary>
        /// 初始化一个 NaviItem 对象
        /// </summary>
        /// <param name="node">要关联的 MarcNode 节点</param>
        /// <param name="type">要初始化的对象的类型</param>
        public NaviItem(MarcNode node, NaviItemType type)
        {
            this.MarcNode = node;
            this.Type = type;
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name
        {
            get
            {
#if NO
                if (this.Type == NaviItemType.Element)
                    return this.MarcNode.Name;
                else if (this.Type == NaviItemType.Attribute)
                    return this.AttrName;
                else if (this.Type == NaviItemType.Text)
                    return "";

                return "";
#endif
                if (this.Type == NaviItemType.Element)
                {
                    if (this.MarcNode.NodeType == NodeType.Record)
                        return "record";
                    if (this.MarcNode.NodeType == NodeType.Field)
                        return "field";
                    if (this.MarcNode.NodeType == NodeType.Subfield)
                        return "subfield";
                    if (this.MarcNode.NodeType == NodeType.None)
                        return "";  // 一般用作临时的根
                    return "";
                }
                else if (this.Type == NaviItemType.Attribute)
                    return this.AttrName;
                else if (this.Type == NaviItemType.Text)
                    return "";

                return "";
            }
        }

        /// <summary>
        /// 值
        /// </summary>
        public string Value
        {
            get
            {
                if (this.Type == NaviItemType.Element)
                    return this.MarcNode.Content;   // 不包含指示符部分
                else if (this.Type == NaviItemType.Attribute)
                    return this.GetAttrValue(this.AttrName);
                else if (this.Type == NaviItemType.Text)
                    return this.Text;

                return "";
            }
        }

        /// <summary>
        /// 是否具有属性?
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                if (this.Type == NaviItemType.Element)
                {
                    if (this.MarcNode.NodeType == NodeType.None)
                        return false;  // 一般用作临时的根
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 获得一个属性值
        /// </summary>
        /// <param name="strAttrName">属性名</param>
        /// <returns>属性值</returns>
        public string GetAttrValue(string strAttrName)
        {
            /*
            if (this.Type != NaviItemType.Element)
                return null;
             * */
            if (this.Type == NaviItemType.Element && this.MarcNode.NodeType == NodeType.None)
                return null;  // 一般用作临时的根

            if (strAttrName == "name")
                return this.MarcNode.Name;
            if (strAttrName == "content")
                return this.MarcNode.Content;
            if (strAttrName == "indicator")
                return this.MarcNode.Indicator;
            if (strAttrName == "indicator1" || strAttrName == "ind1")
                return new string(this.MarcNode.Indicator1, 1);
            if (strAttrName == "indicator2" || strAttrName == "ind2")
                return new string(this.MarcNode.Indicator2, 1);
            if (strAttrName == "leading")
            {
                if (this.MarcNode is MarcField)
                    return ((MarcField)this.MarcNode).Leading;
                return "";
            }

            // TODO: iscontrolfield ?

            return null;
        }

        static string[] attr_names = new string[] { "name",
            "indicator",
            "indicator1",
            "ind1",
            "indicator2",
            "ind2",
            "content",
            "leading",
        };

        /// <summary>
        /// 第一个属性值
        /// </summary>
        public string FirstAttrName
        {
            get
            {
                if (this.Type != NaviItemType.Element)
                    return null;
                if (this.Type == NaviItemType.Element && this.MarcNode.NodeType == NodeType.None)
                    return null;  // 一般用作临时的根

                return attr_names[0];
            }
        }

        /// <summary>
        /// 是否存在指定的属性
        /// </summary>
        /// <param name="strAttrName">属性名</param>
        /// <returns>true表示存在，false表示不存在</returns>
        public bool ExistAttr(string strAttrName)
        {
            if (this.Type == NaviItemType.Element && this.MarcNode.NodeType == NodeType.None)
                return false;  // 一般用作临时的根

            foreach (string strName in attr_names)
            {
                if (strAttrName == strName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获得下一个属性名
        /// </summary>
        /// <param name="strAttrName">当前属性名</param>
        /// <returns>下一个属性名</returns>
        public string GetNextAttrName(string strAttrName)
        {
            if (this.Type == NaviItemType.Element && this.MarcNode.NodeType == NodeType.None)
                return null;  // 一般用作临时的根

            int i = 0;
            foreach (string strName in attr_names)
            {
                if (strAttrName == strName)
                {
                    if (i >= attr_names.Length - 1)
                        return null;    // 没有了
                    return attr_names[i + 1];
                }

                i++;
            }

            return null;    // 没有找到strAttrName
        }

        /// <summary>
        /// 获得上一个属性名
        /// </summary>
        /// <param name="strAttrName">当前属性名</param>
        /// <returns>上一个属性名</returns>
        public string GetPrevAttrName(string strAttrName)
        {
            if (this.Type == NaviItemType.Element && this.MarcNode.NodeType == NodeType.None)
                return null;  // 一般用作临时的根

            int i = 0;
            foreach (string strName in attr_names)
            {
                if (strAttrName == strName)
                {
                    if (i <= 0)
                        return null;    // 没有了
                    return attr_names[i - 1];
                }

                i++;
            }

            return null;    // 没有找到strAttrName
        }

        /// <summary>
        /// 最后一个属性名
        /// </summary>
        public string LastAttrName
        {
            get
            {
                if (this.Type != NaviItemType.Element)
                    return null;
                if (this.Type == NaviItemType.Element && this.MarcNode.NodeType == NodeType.None)
                    return null;  // 一般用作临时的根

                return attr_names[0];
            }
        }

        /// <summary>
        /// 获得表示当前节点的 DOM 位置的路径字符串
        /// </summary>
        /// <returns>路径字符串</returns>
        public string GetPath()
        {
            if (this.Type == NaviItemType.Element)
            {
                return this.MarcNode.getPath();
            }
            if (this.Type == NaviItemType.Attribute)
            {
                return this.MarcNode.getPath() + "@" + this.AttrName;
            }
            if (this.Type == NaviItemType.Text)
            {
                return this.MarcNode.getPath() + "!text";
            }

            return null;
        }
    }
}
