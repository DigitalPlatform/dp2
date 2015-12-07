using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// MARC 基本节点
    /// </summary>
    public class MarcNode 
    {
        /// <summary>
        /// 父节点
        /// </summary>
        public MarcNode Parent = null;

        /// <summary>
        /// 节点类型
        /// </summary>
        public NodeType NodeType = NodeType.None;

        /// <summary>
        /// 子节点集合
        /// </summary>
        public ChildNodeList ChildNodes = new ChildNodeList();

        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcNode 对象
        /// </summary>
        public MarcNode()
        {
            this.Parent = null;
            this.ChildNodes.owner = this;
        }

        /// <summary>
        /// 初始化一个 MarcNode对象，并设置好其 Parent 成员
        /// </summary>
        /// <param name="parent">上级 MarcNode 对象</param>
        public MarcNode(MarcNode parent)
        {
            this.Parent = parent;
            this.ChildNodes.owner = this;
        }

        #endregion

        // Name
        internal string m_strName = "";
        /// <summary>
        /// 节点的名字
        /// </summary>
        public virtual string Name
        {
            get
            {
                return this.m_strName;
            }
            set
            {
                this.m_strName = value;
            }
        }

        // Indicator
        internal string m_strIndicator = "";
        /// <summary>
        /// 节点的指示符
        /// </summary>
        public virtual string Indicator
        {
            get
            {
                return this.m_strIndicator;
            }
            set
            {
                this.m_strIndicator = value;
            }
        }

        /// <summary>
        /// 指示符的第一个字符
        /// </summary>
        public virtual char Indicator1
        {
            get
            {
                if (string.IsNullOrEmpty(m_strIndicator) == true)
                    return (char)0;
                return this.m_strIndicator[0];
            }
            set
            {
                // 没有动作。需要派生类实现
            }
        }

        /// <summary>
        /// 指示符的第二个字符
        /// </summary>
        public virtual char Indicator2
        {
            get
            {
                if (string.IsNullOrEmpty(m_strIndicator) == true)
                    return (char)0;
                if (m_strIndicator.Length < 2)
                    return (char)0;
                return this.m_strIndicator[1];
            }
            set
            {
                // 没有动作。需要派生类实现
            }
        }

        // Content
        // 这个是缺省的实现方式，可以直接用于没有下级的纯内容节点
        internal string m_strContent = "";
        /// <summary>
        /// 节点的正文内容
        /// </summary>
        public virtual string Content
        {
            get
            {
                return this.m_strContent;
            }
            set
            {
                this.m_strContent = value;
            }
        }

        // Text 用于构造MARC机内格式字符串的表示当前节点部分的字符串
        //
        /// <summary>
        /// 节点的全部文字，MARC 机内格式表现形态
        /// </summary>
        public virtual string Text
        {
            get
            {
                return this.Name + this.Indicator + this.Content;
            }
            set
            {
                this.Content = value;   // 这是个草率的实现，需要具体节点重载本函数
            }
        }

        /// <summary>
        /// 创建一个新的节点对象，从当前对象复制出全部内容
        /// </summary>
        /// <returns>新的节点对象</returns>
        public virtual MarcNode clone()
        {
            throw new Exception("not implemented");
        }


        // 看一个字段名是否是控制字段。所谓控制字段没有指示符概念
        // parameters:
        //		strFieldName	字段名
        // return:
        //		true	是控制字段
        //		false	不是控制字段
        /// <summary>
        /// 检测一个字段名是否为控制字段(的字段名)
        /// </summary>
        /// <param name="strFieldName">要检测的字段名</param>
        /// <returns>true表示是控制字段，false表示不是控制字段</returns>
        public static bool isControlFieldName(string strFieldName)
        {
            if (String.Compare(strFieldName, "hdr", true) == 0)
                return true;

            if (String.Compare(strFieldName, "###", true) == 0)
                return true;

            if (
                (
                String.Compare(strFieldName, "001") >= 0
                && String.Compare(strFieldName, "009") <= 0
                )

                || String.Compare(strFieldName, "-01") == 0
                )
                return true;

            return false;
        }

        /// <summary>
        /// 输出当前对象的全部子对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public virtual string dumpChildren()
        {
            StringBuilder strResult = new StringBuilder(4096);
            for (int i = 0; i < this.ChildNodes.count; i++)
            {
                MarcNode child = this.ChildNodes[i];
                strResult.Append( child.dump() );
            }

            return strResult.ToString();
        }

        /// <summary>
        /// 输出当前对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public virtual string dump()
        {
            // 一般实现
            return this.Name + this.Indicator 
                + dumpChildren();
        }

        /// <summary>
        /// 获得根节点
        /// </summary>
        /// <returns>根节点</returns>
        public MarcNode getRootNode()
        {
            MarcNode node = this;
            while (node.Parent != null)
                node = node.Parent;

            Debug.Assert(node.Parent == null, "");
            return node;
        }

        // 根
        /// <summary>
        /// 根节点
        /// </summary>
        public MarcNode Root
        {
            get
            {
                MarcNode node = this;
                while (node.Parent != null)
                {
                    node = node.Parent;
                }

#if DEBUG
                if (node != this)
                {
                    Debug.Assert(node.NodeType == Marc.NodeType.Record || node.NodeType == Marc.NodeType.None, "");
                }
#endif
                return node;
            }
        }

        // 
        /// <summary>
        /// 获得表示当前对象的位置的路径。用于比较节点之间的位置关系
        /// </summary>
        /// <returns>路径字符串</returns>
        public string getPath()
        {
            MarcNode parent = this.Parent;
            if (parent == null)
                return "0";
            int index = parent.ChildNodes.indexOf(this);
            if (index == -1)
                throw new Exception("在父节点的 ChildNodes 中没有找到自己");

            string strParentPath = this.Parent.getPath();
            return strParentPath + "/" + index.ToString();
        }

        // 内容是否为空?
        /// <summary>
        /// 检测节点内容是否为空
        /// </summary>
        public bool isEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(this.Content) == true)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// 将当前节点从父节点摘除。但依然保留对当前节点对下级的拥有关系
        /// </summary>
        /// <returns>已经被摘除的当前节点</returns>
        public MarcNode detach()
        {
            MarcNode parent = this.Parent;
            if (parent == null)
                return this; // 自己是根节点，或者先前已经被摘除
            int index = parent.ChildNodes.indexOf(this);
            if (index == -1)
            {
                throw new Exception("parent的ChildNodes中居然没有找到自己");
                return this;
            }

            parent.ChildNodes.removeAt(index);
            this.Parent = null;
            return this;
        }

        /// <summary>
        /// 将指定节点插入到当前节点的前面兄弟位置
        /// </summary>
        /// <param name="source">要插入的节点</param>
        /// <returns>当前节点</returns>
        public MarcNode before(MarcNode source)
        {
            MarcNode parent = this.Parent;
            // 自己是根节点，无法具有兄弟
            if (parent == null)
                throw new Exception("无法在根节点同级插入新节点");

            int index = parent.ChildNodes.indexOf(this);
            if (index == -1)
            {
                throw new Exception("parent的ChildNodes中居然没有找到自己");
            }

            // 进行类型检查，同级只能插入相同类型的元素
            if (this.NodeType != source.NodeType)
                throw new Exception("无法在节点同级插入不同类型的新节点。this.NodeTYpe=" + this.NodeType.ToString() + ", source.NodeType=" + source.NodeType.ToString());

            source.detach();
            parent.ChildNodes.insert(index, source);
            source.Parent = this.Parent;
            return this;
        }

        // 把source插入到this的后面。返回this
        /// <summary>
        /// 将指定节点插入到当前节点的后面兄弟位置
        /// </summary>
        /// <param name="source">要插入的节点</param>
        /// <returns>当前节点</returns>
        public MarcNode after(MarcNode source)
        {
            MarcNode parent = this.Parent;
            // 自己是根节点，无法具有兄弟
            if (parent == null)
                throw new Exception("无法在根节点同级插入新节点");

            int index = parent.ChildNodes.indexOf(this);
            if (index == -1)
            {
                throw new Exception("parent的ChildNodes中居然没有找到自己");
            }

            // 进行类型检查，同级只能插入相同类型的元素
            if (this.NodeType != source.NodeType)
                throw new Exception("无法在节点同级插入不同类型的新节点。this.NodeTYpe="+this.NodeType.ToString()+", source.NodeType="+source.NodeType.ToString());

            source.detach();
            parent.ChildNodes.insert(index+1, source);
            source.Parent = this.Parent;
            return this;
        }

        // 把strText构造的新对象插入到this的后面。返回this
        /// <summary>
        /// 用指定的字符串构造出新的节点，插入到当前节点的后面兄弟位置
        /// </summary>
        /// <param name="strText">用于构造新节点的字符串</param>
        /// <returns>当前节点</returns>
        public MarcNode after(string strText)
        {
            MarcNodeList targets = new MarcNodeList(this);

            targets.after(strText);
            return this;
        }

        // 把source插入到this的下级末尾位置。返回this
        /// <summary>
        /// 将指定节点追加到当前节点的子节点尾部
        /// </summary>
        /// <param name="source">要追加的节点</param>
        /// <returns>当前节点</returns>
        public MarcNode append(MarcNode source)
        {
            source.detach();
            this.ChildNodes.add(source);
            source.Parent = this;
            return this;
        }

        // 把strText构造的新对象插入到this的下级末尾位置。返回this
        /// <summary>
        /// 用指定的字符串构造出新的节点，追加到当前节点的子节点末尾
        /// </summary>
        /// <param name="strText">用于构造新节点的字符串</param>
        /// <returns>当前节点</returns>
        public MarcNode append(string strText)
        {
            MarcNodeList targets = new MarcNodeList(this);

            targets.append(strText);
            return this;
        }

        // this 插入到 target 儿子的末尾
        /// <summary>
        /// 将当前节点追加到指定(目标)节点的子节点末尾
        /// </summary>
        /// <param name="target">目标节点</param>
        /// <returns>当前节点</returns>
        public MarcNode appendTo(MarcNode target)
        {
            this.detach();
            target.ChildNodes.add(this);
            this.Parent = target;
            return this;
        }

        // 把source插入到this的下级开头位置。返回this
        /// <summary>
        /// 将指定的(源)节点插入到当前节点的子节点开头位置
        /// </summary>
        /// <param name="source">源节点</param>
        /// <returns>当前节点</returns>
        public MarcNode prepend(MarcNode source)
        {
            source.detach();
            this.ChildNodes.insert(0, source);
            source.Parent = this;
            return this;
        }

        // 把strText构造的新对象插入到this的下级开头位置。返回this
        /// <summary>
        /// 用指定的字符串构造出新节点，插入到当前节点的子节点开头
        /// </summary>
        /// <param name="strText">用于构造新节点的字符串</param>
        /// <returns>当前节点</returns>
        public MarcNode prepend(string strText)
        {
            MarcNodeList targets = new MarcNodeList(this);

            targets.prepend(strText);
            return this;
        }

        // this 插入到 target 的儿子的第一个
        /// <summary>
        /// 将当前节点插入到指定的(目标)节点的子节点的开头
        /// </summary>
        /// <param name="target">目标节点</param>
        /// <returns>当前节点</returns>
        public MarcNode prependTo(MarcNode target)
        {
            this.detach();
            target.ChildNodes.insert(0, this);
            this.Parent = target;
            return this;
        }

#if NO
        public virtual MarcNavigator CreateNavigator()
        {
            return new MarcNavigator(this);
        }
#endif

        public MarcNodeList select(string strXPath)
        {
            return select(strXPath, -1);
        }

        // 针对DOM树进行 XPath 筛选
        // parameters:
        //      nMaxCount    至多选择开头这么多个元素。-1表示不限制
        /// <summary>
        /// 用 XPath 字符串选择节点
        /// </summary>
        /// <param name="strXPath">XPath 字符串</param>
        /// <param name="nMaxCount">限制命中的最多节点数。-1表示不限制</param>
        /// <returns>被选中的节点集合</returns>
        public MarcNodeList select(string strXPath, int nMaxCount/* = -1*/)
        {
            MarcNodeList results = new MarcNodeList();

            MarcNavigator nav = new MarcNavigator(this);  // 出发点在当前对象

            XPathNodeIterator ni = nav.Select(strXPath);
            while (ni.MoveNext() && (nMaxCount == -1 || results.count < nMaxCount))
            {
                NaviItem item = ((MarcNavigator)ni.Current).Item;
                if (item.Type != NaviItemType.Element)
                {
                    // if (bSkipNoneElement == false)
                        throw new Exception("xpath '"+strXPath+"' 命中了非元素类型的节点，这是不允许的");
                    continue;
                }
                results.add(item.MarcNode);
            }
            return results;
        }

        /*
        public MarcNode SelectSingleNode(string strXpath)
        {
            MarcNavigator nav = new MarcNavigator(this);
            XPathNodeIterator ni = nav.Select(strXpath);
            ni.MoveNext();
            return ((MarcNavigator)ni.Current).Item.MarcNode;
        }
         * */

#if NO
        public MarcNodeList SelectNodes(string strPath)
        {
            string strFirstPart = GetFirstPart(ref strPath);

            if (strFirstPart == "/")
            {
                /*
                if (this.Parent == null)
                    return this.SelectNodes(strPath);
                 * */

                return GetRootNode().SelectNodes(strPath);
            }

            if (strFirstPart == "..")
            {
                return this.Parent.SelectNodes(strPath);
            }

            if (strFirstPart == ".")
            {
                return this.SelectNodes(strPath);
            }

            // tagname[@attrname='']
            string strTagName = "";
            string strCondition = "";

            int nRet = strFirstPart.IndexOf("[");
            if (nRet == -1)
                strTagName = strFirstPart;
            else
            {
                strCondition = strFirstPart.Substring(nRet + 1);
                if (strCondition.Length > 0)
                {
                    // 去掉末尾的']'
                    if (strCondition[strCondition.Length - 1] == ']')
                        strCondition.Substring(0, strCondition.Length - 1);
                }
                strTagName = strFirstPart.Substring(0, nRet);
            }

            MarcNodeList results = new MarcNodeList(null);

            for (int i = 0; i < this.ChildNodes.Count; i++)
            {
                MarcNode node = this.ChildNodes[i];
                Debug.Assert(node.Parent != null, "");
                if (strTagName == "*" || node.Name == strTagName)
                {
                    if (results.Parent == null)
                        results.Parent = node.Parent;
                    results.Add(node);
                }
            }


            if (String.IsNullOrEmpty(strPath) == true)
            {
                // 到了path的末级。用strFirstPart筛选对象
                return results;
            }

            return results.SelectNodes(strPath);
        }

                // 获得路径的第一部分
        static string GetFirstPart(ref string strPath)
        {
            if (String.IsNullOrEmpty(strPath) == true)
                return "";

            if (strPath[0] == '/')
            {
                strPath = strPath.Substring(1);
                return "/";
            }

            string strResult = "";
            int nRet = strPath.IndexOf("/");
            if (nRet == -1)
            {
                strResult = strPath;
                strPath = "";
                return strResult;
            }

            strResult = strPath.Substring(0, nRet);
            strPath = strPath.Substring(nRet + 1);
            return strResult;
        }
#endif

        // 删除自己
        // 但是this.Parent指针还是没有清除
        /// <summary>
        /// 从父节点(的子节点集合中)将当前节点移走。注意，本操作并不修改当前节点的 Parent 成员，也就是说 Parent 成员依然指向父节点
        /// </summary>
        /// <returns>当前节点</returns>
        public MarcNode remove()
        {
            if (this.Parent != null)
            {
                this.Parent.ChildNodes.remove(this);
                return this;
            }

            return null;    // biaoshi zhaobudao , yejiu wucong shanchu
        }

        #region 访问各种位置

        /// <summary>
        /// 当前节点的第一个子节点
        /// </summary>
        public MarcNode FirstChild
        {
            get
            {
                if (this.ChildNodes.count == 0)
                    return null;
                return this.ChildNodes[0];
            }
        }

        /// <summary>
        /// 当前节点的最后一个子节点
        /// </summary>
        public MarcNode LastChild
        {
            get
            {
                if (this.ChildNodes.count == 0)
                    return null;
                return this.ChildNodes[this.ChildNodes.count - 1];
            }
        }

        #endregion
    }


    // MARC 记录
    /// <summary>
    /// MARC 记录节点
    /// </summary>
    public class MarcRecord : MarcNode
    {
        // 存储头标区 24 字符
        /// <summary>
        /// MARC记录的头标区，一共24个字符
        /// </summary>
        public MarcHeader Header = new MarcHeader();

        /// <summary>
        /// 嵌套字段的定义。
        /// 缺省为 空，表示不使用嵌套字段。
        /// 这是一个列举字段名的逗号间隔的列表('*'为通配符)，或者 '@' 字符后面携带一个正则表达式
        /// </summary>
        public string OuterFieldDef
        {
            get;
            set;
        }

        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcRecord 对象
        /// </summary>
        public MarcRecord()
        {
            this.Parent = null;
            this.NodeType = NodeType.Record;
        }

        // 通过传递过来的MARC记录构造整个一棵树
        /// <summary>
        /// 初始化一个 MarcRecord 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strRecord">表示一条完整的 MARC 记录的 MARC 机内格式字符串</param>
        /// <param name="strOuterFieldDef">嵌套字段的定义。缺省为 null，表示不使用嵌套字段。这是一个列举字段名的逗号间隔的列表('*'为通配符)，或者 '@' 字符后面携带一个正则表达式</param>
        public MarcRecord(string strRecord,
            string strOuterFieldDef = null)
        {
            this.NodeType = NodeType.Record;

            this.OuterFieldDef = strOuterFieldDef;
            this.Content = strRecord;
        }

        #endregion

        // 根节点的 Name Indicator 都为空，内容记载在 Content
        /// <summary>
        /// 当前节点的全部文字。表现了一条完整的 MARC 记录
        /// </summary>
        public override string Text
        {
            get
            {
                return this.Content;
            }
            set
            {
                this.Content = value;
            }
        }

        /// <summary>
        /// 当前节点的正文内容。对于 MarcRoecrd 节点来说，它等同于 Text 成员值
        /// </summary>
        public override string Content
        {
            get
            {
                StringBuilder result = new StringBuilder(4096);
                result.Append(this.Header.ToString());
                // 合成下级元素
                for (int i = 0; i < this.ChildNodes.count; i++)
                {
                    MarcNode node = this.ChildNodes[i];

                    if (node.NodeType != Marc.NodeType.Field)
                        throw new Exception("根下级出现了不是 Field 类型的节点 ("+node.NodeType.ToString()+")");
                    /*
                    if (i != 0 && node.Name != "hdr")
                        throw new Exception("MarcField同级第一个位置必须是Name为'hdr'的表示头标区的MarcField（而现在Name为 '" + node.Name + "'）");
                     * */
                    MarcField field = (MarcField)node;

                    result.Append(field.Text);
                }
                return result.ToString();
            }
            set
            {
                setContent(value);
            }
        }

        void setContent(string strValue)
        {
            // 拆分为字段
            this.ChildNodes.clearAndDetach();
            this.m_strContent = "";

            if (String.IsNullOrEmpty(strValue) == true)
                return;

            // 整理尾部字符
            char tail = strValue[strValue.Length - 1];
            if (tail == 29)
            {
                strValue = strValue.Substring(0, strValue.Length - 1);

                if (String.IsNullOrEmpty(strValue) == true)
                    return;
            }

            this.Header[0, Math.Min(strValue.Length, 24)] = strValue;

            if (strValue.Length <= 24)
            {
                // 只有头标区，没有任何字段
                return;
            }
            this.ChildNodes.add(MarcQuery.createFields(
                // this,
                strValue.Substring(24),
                this.OuterFieldDef));
        }
#if NO
        void SetContent(string strValue)
        {
            // 拆分为字段
            this.ChildNodes.Clear();
            this.m_strContent = "";

            if (String.IsNullOrEmpty(strValue) == true)
                return;

            // 整理尾部字符
            char tail = strValue[strValue.Length - 1];
            if (tail == 29)
                strValue = strValue.Substring(0, strValue.Length - 1);

            if (String.IsNullOrEmpty(strValue) == true)
                return;

            tail = strValue[strValue.Length - 1];
            if (tail != 30)
                strValue += (char)30;

            StringBuilder field_text = new StringBuilder(4096);
            MarcField field = null;
            for (int i = 0; i < strValue.Length; i++)
            {
                char ch = strValue[i];
                if (ch == 30 || ch == 29)
                {
                    // 上一个字段结束。创建一个字段的时机
                    string strText = field_text.ToString();

                    if (this.ChildNodes.Count == 0)
                    {
                        // 创建第一个字段，也就是头标区
                        string strHeader = "";  // 头标区
                        string strRest = "";    // 余下的部分
                        // 长度不足 24 字符
                        if (string.IsNullOrEmpty(strText) == true
                            || strText.Length < 24)
                        {
                            // 需要补齐 24 字符
                            strHeader = strText.PadRight(24, '?');
                        }
                        else
                        {
                            // 长度大于或者等于 24 字符
                            strHeader = strText.Substring(0, 24);
                            strRest = strText.Substring(24);
                        }

                        // header
                        Debug.Assert(strHeader.Length == 24, "");
                        field = new MarcField(this);
                        field.IsHeader = true;
                        field.Text = strHeader;
                        this.ChildNodes.Add(field);
                        Debug.Assert(field.Parent == this, "");

                        // 余下的部分再创建一个字段
                        if (string.IsNullOrEmpty(strRest) == false)
                        {
                            // 如果长度不足 3 字符，补齐?
                            if (strRest.Length < 3)
                                strRest = strRest.PadRight(3, '?');
                            field = new MarcField(this);
                            field.Text = strRest;
                            this.ChildNodes.Add(field);
                            Debug.Assert(field.Parent == this, "");
                        }

                        field_text.Clear();
                        continue;
                    }

                    // 创建头标区以后的普通字段
                    field = new MarcField(this);

                    // 如果长度不足 3 字符，补齐?
                    if (strText.Length < 3)
                        strText = strText.PadRight(3, '?');
                    field = new MarcField(this);
                    field.Text = strText;
                    this.ChildNodes.Add(field);
                    Debug.Assert(field.Parent == this, "");

                    field_text.Clear();
                }
                else
                {
                    field_text.Append(ch);
                }
            }

        }
#endif

        // 常用名。等同于ChildNodes
        /// <summary>
        /// 当前节点下属的全部字段节点。本属性相当于 CHildNodes 的别名
        /// </summary>
        public MarcNodeList Fields
        {
            get
            {
                return this.ChildNodes;
            }
            set
            {
                this.ChildNodes.clearAndDetach();
                this.ChildNodes.add(value);
            }
        }

        // fangbian diaoyong
        /// <summary>
        /// 在下级节点末尾追加一个字段节点
        /// </summary>
        /// <param name="field">字段节点</param>
        public void add(MarcField field)
        {
            this.ChildNodes.add(field);
        }

        /// <summary>
        /// 设置指定名字的第一个字段的指示符和内容。如果没有这个字段，则创建一个
        /// </summary>
        /// <param name="strFieldName">字段名。3字符</param>
        /// <param name="strIndicator">要设置的指示符。如果不想修改指定字段的指示符，则可以将本参数设置为 null；否则应当是一个 2 字符的字符串。如果要操作的字段是控制字段，则本参数不会被使用，可设置为 null</param>
        /// <param name="strContent">要设置的字段内容。如果是非控制字段，本参数中一般需要按照 MARC 格式要求包含子字段符号(MarcQuery.SUBFLD)，以指定子字段名等</param>
        /// <param name="strNewIndicator">如果指定的字段不存在，则需要创建，创建的时候将采用本参数作为字段指示符的值。控制字段(因为没有指示符所以)不使用本参数。如果本参数设置为 null，但函数执行过程中确实遇到了需要创建新字段的情况，则函数会自动采用两个空格作为新字段的指示符</param>
        public void setFirstField(
            string strFieldName,
            string strIndicator,
            string strContent,
            string strNewIndicator = null)
        {
            // 检查参数
            if (string.IsNullOrEmpty(strFieldName) == true || strFieldName.Length != 3)
                throw new ArgumentException("strFieldName不能为空，应是 3 字符内容", "strFieldName");
            if (string.IsNullOrEmpty(strIndicator) == false && strIndicator.Length != 2)
                throw new ArgumentException("strIndicator若不是空，应是 2 字符内容", "strIndicator");
            if (string.IsNullOrEmpty(strNewIndicator) == false && strNewIndicator.Length != 2)
                throw new ArgumentException("strNewIndicator若不是空，应是 2 字符内容", "strNewIndicator");

            MarcNodeList fields = this.select("field[@name='" + strFieldName + "']");
            if (fields.count == 0)
            {
                if (isControlFieldName(strFieldName) == true)
                {
                    this.ChildNodes.insertSequence(new MarcField(strFieldName, strContent));
                }
                else
                {
                    if (string.IsNullOrEmpty(strNewIndicator) == true)
                        strNewIndicator = "  ";
                    this.ChildNodes.insertSequence(new MarcField(strFieldName, strNewIndicator, strContent));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(strIndicator) == false)
                    fields[0].Indicator = strIndicator;

                fields[0].Content = strContent;
            }
        }


        /// <summary>
        /// 设置指定名字的第一个字段和第一个子字段的值。如果没有这个字段，则创建一个；如果没有这个子字段，则创建一个
        /// </summary>
        /// <param name="strFieldName">字段名。3字符</param>
        /// <param name="strSubfieldName">子字段名。1字符</param>
        /// <param name="strContent">要设置的子字段内容</param>
        /// <param name="strNewIndicator">如果指定的字段不存在，则需要创建，创建的时候将采用本参数作为字段指示符的值</param>
        public void setFirstSubfield(
            string strFieldName,
            string strSubfieldName,
            string strContent,
            string strNewIndicator = "  ")
        {
            // 检查参数
            if (string.IsNullOrEmpty(strFieldName) == true || strFieldName.Length != 3)
                throw new ArgumentException("strFieldName不能为空，应是 3 字符内容", "strFieldName");
            if (string.IsNullOrEmpty(strSubfieldName) == true || strSubfieldName.Length != 1)
                throw new ArgumentException("strSubfieldName不能为空，应是 1 字符内容", "strSubfieldName");
            if (string.IsNullOrEmpty(strNewIndicator) == true || strNewIndicator.Length != 2)
                throw new ArgumentException("strNewIndicator不能为空，应是 2 字符内容", "strNewIndicator");

            MarcNodeList fields = this.select("field[@name='" + strFieldName + "']");
            if (fields.count == 0)
            {
                this.ChildNodes.insertSequence(new MarcField(strFieldName, strNewIndicator, MarcQuery.SUBFLD + strSubfieldName + strContent));
            }
            else
            {
                MarcNodeList subfields = fields[0].select("subfield[@name='"+strSubfieldName+"']");
                if (subfields.count == 0)
                {
                    fields[0].ChildNodes.insertSequence(new MarcSubfield("a", strContent));
                }
                else
                {
                    subfields[0].Content = strContent;
                }
            }
        }

        /// <summary>
        /// 输出当前对象的全部子对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public override string dumpChildren()
        {
            StringBuilder strResult = new StringBuilder(4096);
            foreach (MarcNode node in this.ChildNodes)
            {
                if (strResult.Length > 0)
                    strResult.Append("\r\n");
                strResult.Append(node.dump());
            }

            return strResult.ToString();
        }

        /// <summary>
        /// 输出当前对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public override string dump()
        {
            return this.Header + "\r\n" + dumpChildren();
        }

        /// <summary>
        /// 创建一个新的 MarcRecord 节点对象，从当前对象复制出全部内容和下级节点
        /// </summary>
        /// <returns>新的节点对象</returns>
        public override MarcNode clone()
        {
            MarcNode new_node = new MarcRecord();
            new_node.Text = this.Text;
            new_node.Parent = null; // 尚未和任何对象连接
            return new_node;
        }
    }

    /// <summary>
    /// MARC 外围字段节点
    /// </summary>
    public class MarcOuterField : MarcField
    {
        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcOuterField 对象
        /// </summary>
        public MarcOuterField()
        {
            this.Parent = null;
            this.NodeType = NodeType.Field;
            Debug.Assert(this.ChildNodes.owner == this, "");

            this.Text = DefaultFieldName;
        }

        // 使用一个字符串构造
        /// <summary>
        /// 初始化一个 MarcOuterField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strText">表示一个完整的 MARC 字段的 MARC 机内格式字符串</param>
        public MarcOuterField(string strText)
        {
            this.Parent = null;
            this.NodeType = NodeType.Field;
            Debug.Assert(this.ChildNodes.owner == this, "");

            this.Text = strText;
        }

        // 使用两个或者三个字符串构造
        // 创建001等控制字段的时候，可以只使用前面两个参数，这时候第二参数表示内容部分
        // 如果在创建001等控制字段的时候，一共使用了三个参数，则 strIndicator + strContent 一起当作字段内容
        /// <summary>
        /// 初始化一个 MarcOuterField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="strContent">字段正文</param>
        public MarcOuterField(string strName,
            string strIndicator,
            string strContent = "")
        {
            this.NodeType = NodeType.Field;

            if (String.IsNullOrEmpty(strName) == true)
                this.m_strName = DefaultFieldName;
            else
            {
                if (strName.Length != 3)
                    throw new Exception("Field的Name必须为3字符");
                this.m_strName = strName;
            }

            if (isControlFieldName(strName) == true)
            {
                // strIndicator 和 strContent 连接在一起当作内容
                // 因为这个缘故，可以省略第三个参数
                this.Content = strIndicator + strContent;
                return;
            }
            this.m_strIndicator = strIndicator;
            this.Content = strContent;
        }

        // parameters:
        //      subfields   字符串数组，指定了一系列要创建的子字段文字。每个字符串型态需要这样 “axxx” 表示子字段名为a，内容为xxx。注意，不需要包含子字段符号 SUBFLD
        /// <summary>
        /// 初始化一个 MarcOuterField 对象，并根据指定的字符串设置好全部内容和下级对象
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="fields">表示若干内嵌字段的字符串数组。每个字符串开始就是字段名</param>
        public MarcOuterField(string strName,
            string strIndicator,
            List<string> fields)
        {
            string[] temp = new string[fields.Count];
            fields.CopyTo(temp);

            newMarcField(strName,
                strIndicator,
                temp);
        }

        // 另一字符串数组版本
        /// <summary>
        /// 初始化一个 MarcOuterField 对象，并根据指定的字符串设置好全部内容和下级对象
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="fields">表示若干内嵌字段的字符串数组。每个字符串开始就是字段名</param>
        public MarcOuterField(string strName,
            string strIndicator,
            string[] fields)
        {
            newMarcField(strName, strIndicator, fields);
        }

        void newMarcField(string strName,
            string strIndicator,
            string [] fields)
        {
            this.NodeType = NodeType.Field;

            if (String.IsNullOrEmpty(strName) == true)
                this.m_strName = DefaultFieldName;
            else
            {
                if (strName.Length != 3)
                    throw new Exception("Field的Name必须为3字符");
                this.m_strName = strName;
            }

            if (isControlFieldName(strName) == true)
            {
                throw new ArgumentException("外围字段不能使用控制字段名", "strName");
            }

            this.m_strIndicator = strIndicator;

            StringBuilder content = new StringBuilder(4096);
            foreach (string s in fields)
            {
                content.Append(MarcQuery.SUBFLD);
                content.Append("1");
                content.Append(s);
            }

            this.Content = content.ToString();
        }

        // 使用一个字符串构造，指定了后面参数中所使用的(代用)子字段符号
        /// <summary>
        /// 初始化一个 MarcOuterField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="chSubfield">子字段符号的代用字符。在 strText 参数中可以用这个代用字符来表示子字段符号 (ASCII 31)</param>
        /// <param name="strText">表示一个完整的 MARC 字段的 MARC 机内格式字符串</param>
        public MarcOuterField(char chSubfield, string strText)
        {
            this.Parent = null;
            this.NodeType = NodeType.Field;
            Debug.Assert(this.ChildNodes.owner == this, "");

            this.Text = strText.Replace(chSubfield, MarcQuery.SUBFLD[0]);
        }

        #endregion

        /// <summary>
        /// 当前节点的全部文字。表现了一个完整的 MARC 外围字段
        /// </summary>
        public override string Text
        {
            get
            {
#if NO
                // 头标区
                if (this.IsHeader == true)
                {
                    Debug.Assert(string.IsNullOrEmpty(this.Name) == true, "");
                    Debug.Assert(string.IsNullOrEmpty(this.Indicator) == true, "");
                    return this.Content;
                }
#endif

                return this.Name + this.Indicator + this.Content + MarcQuery.FLDEND;
            }
            set
            {
                setFieldText(value);
            }
        }

        // 将机内格式的字符串设置到字段
        // 最后一个字符可以是 30 (表示字段结束)，也可以没有这个字符
        void setFieldText(string strText)
        {
            // 去掉末尾的 30 字符
            if (strText != null && strText.Length >= 1)
            {
                if (strText[strText.Length - 1] == (char)30)
                    strText = strText.Substring(0, strText.Length - 1);
            }

            if (string.IsNullOrEmpty(strText) == true)
                throw new Exception("字段 Text 不能设置为空");

            if (strText.Length < 3)
                throw new Exception("字段 Text 不能设置为小于 3 字符");

            string strFieldName = strText.Substring(0, 3);
            strText = strText.Substring(3); // 剩余部分

            this.m_strName = strFieldName;
            if (MarcNode.isControlFieldName(strFieldName) == true)
            {
                throw new Exception("MARC 外围字段的字段名不能使用控制字段名 '"+strFieldName+"'");
            }
            else
            {
                // 普通字段

                // 剩下的内容为空
                if (string.IsNullOrEmpty(strText) == true)
                {
                    this.Indicator = DefaultIndicator;
                    return;
                }

                // 还剩下一个字符
                if (strText.Length < 2)
                {
                    Debug.Assert(strText.Length == 1, "");
                    this.Indicator = strText + new string(MarcQuery.DefaultChar, 1);
                    return;
                }

                // 剩下两个字符以上
                this.m_strIndicator = strText.Substring(0, 2);
                this.Content = strText.Substring(2);
            }
        }

        /// <summary>
        /// 字段正文。即字段指示符以后的全部内容
        /// </summary>
        public override string Content
        {
            get
            {
                StringBuilder result = new StringBuilder(4096);
                result.Append(this.m_strContent);   // 第一个子字段符号以前的内容
                // 合成下级元素
                for (int i = 0; i < this.ChildNodes.count; i++)
                {
                    MarcNode node = this.ChildNodes[i];
                    result.Append(node.Text);
                    // strResult += new string((char)31, 1) + node.Name + node.Content;
                }
                return result.ToString();
            }
            set
            {
                // 拆分为子字段
                this.ChildNodes.clearAndDetach();
                this.m_strContent = "";

                string strLeadingString = "";
                MarcNodeList inner_fields = MarcQuery.createInnerFields(
                    value, out strLeadingString);
                this.ChildNodes.add(inner_fields);
                this.m_strContent = strLeadingString;
            }
        }

    }

    /// <summary>
    /// MARC 内嵌字段节点
    /// </summary>
    public class MarcInnerField : MarcField
    {
        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcInnerField 对象
        /// </summary>
        public MarcInnerField() : base()
        {
        }

        // 使用一个字符串构造
        /// <summary>
        /// 初始化一个 MarcInnerField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strText">表示一个完整的 MARC 字段的 MARC 机内格式字符串</param>
        public MarcInnerField(string strText) : base(strText)
        {
        }

        /// <summary>
        /// 初始化一个 MarcInnerField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="strContent">字段正文</param>
        public MarcInnerField(string strName,
            string strIndicator,
            string strContent = "") : base(strName, strIndicator, strContent)
        {
        }

        // parameters:
        //      subfields   字符串数组，指定了一系列要创建的子字段文字。每个字符串型态需要这样 “axxx” 表示子字段名为a，内容为xxx。注意，不需要包含子字段符号 SUBFLD
        /// <summary>
        /// 初始化一个 MarcInnerField 对象，并根据指定的字符串设置好全部内容和下级对象
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="subfields">表示若干子字段的字符串数组。每个字符串的第一字符为子字段名，其余为子字段内容</param>
        public MarcInnerField(string strName,
            string strIndicator,
            List<string> subfields) : base (strName, strIndicator, subfields)
        {
        }

        // 另一字符串数组版本
        /// <summary>
        /// 初始化一个 MarcInnerField 对象，并根据指定的字符串设置好全部内容和下级对象
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="subfields">表示若干子字段的字符串数组。每个字符串的第一字符为子字段名，其余为子字段内容</param>
        public MarcInnerField(string strName,
            string strIndicator,
            string[] subfields) : base (strName, strIndicator, subfields)
        {
        }

        // 使用一个字符串构造，指定了后面参数中所使用的(代用)子字段符号
        /// <summary>
        /// 初始化一个 MarcInnerField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="chSubfield">子字段符号的代用字符。在 strText 参数中可以用这个代用字符来表示子字段符号 (ASCII 31)</param>
        /// <param name="strText">表示一个完整的 MARC 字段的 MARC 机内格式字符串</param>
        public MarcInnerField(char chSubfield, string strText) : base (chSubfield, strText)
        {
        }

        #endregion


        /// <summary>
        /// 当前节点的全部文字。表现了一个完整的 MARC 内嵌字段
        /// </summary>
        public override string Text
        {
            get
            {
#if NO
                // 头标区
                if (this.IsHeader == true)
                {
                    Debug.Assert(string.IsNullOrEmpty(this.Name) == true, "");
                    Debug.Assert(string.IsNullOrEmpty(this.Indicator) == true, "");
                    return this.Content;
                }
#endif

                // 普通字段
                return MarcQuery.SUBFLD + "1" + this.Name + this.Indicator + this.Content;
            }
            set
            {
#if NO
                if (this.IsHeader == true)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        throw new Exception("头标区内容不能设置为空");
                    if (value.Length != 24)
                        throw new Exception("头标区内容只能设置为24字符");

                    this.Content = value;
                    return;
                }
#endif

                setFieldText(value);
            }
        }

        // 将机内格式的字符串设置到字段
        // $1200  $axxx$bxxx
        void setFieldText(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                throw new Exception("内嵌字段 Text 不能设置为空");
            if (strText[0] == MarcQuery.SUBFLD[0])
            {
                if (strText.Length < 5)
                    throw new Exception("内嵌字段 Text 不能设置为小于 5 字符");
                if (strText[0] != MarcQuery.SUBFLD[0])
                    throw new Exception("内嵌字段 Text 第一字符必须是子字段符号");
                if (strText[1] != '1')
                    throw new Exception("内嵌字段 Text 第二字符必须为 '1'");

                // 去掉头部的 $1  字符
                strText = strText.Substring(2);
                if (strText.Length < 3)
                    throw new Exception("字段 Text 剥离前二字符后，不能设置为小于 3 字符");
            }
            else
            {
                if (strText.Length < 3)
                    throw new Exception("内嵌字段 Text 不能设置为小于 3 字符");
            }

            string strFieldName = strText.Substring(0, 3);
            strText = strText.Substring(3); // 剩余部分

            this.m_strName = strFieldName;
            if (MarcNode.isControlFieldName(strFieldName) == true)
            {
                // 控制字段
                this.m_strIndicator = "";

                // 剩下的内容为空
                if (string.IsNullOrEmpty(strText) == true)
                {
                    this.Content = "";
                    return;
                }

                this.m_strContent = strText;    // 不要用this.Content，因为会惊扰到重新创建下级的机制
            }
            else
            {
                // 普通字段

                // 剩下的内容为空
                if (string.IsNullOrEmpty(strText) == true)
                {
                    this.Indicator = DefaultIndicator;
                    return;
                }

                // 还剩下一个字符
                if (strText.Length < 2)
                {
                    Debug.Assert(strText.Length == 1, "");
                    this.Indicator = strText + new string(MarcQuery.DefaultChar, 1);
                    return;
                }

                // 剩下两个字符以上
                this.m_strIndicator = strText.Substring(0, 2);
                this.Content = strText.Substring(2);
            }
        }

        /// <summary>
        /// 输出当前对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public override string dump()
        {
            return "$1" + this.Name + this.Indicator + this.m_strContent
                + dumpChildren();
        }
    }

    // MARC 字段
    /// <summary>
    /// MARC 字段节点
    /// </summary>
    public class MarcField : MarcNode
    {
        // 缺省的字段名
        /// <summary>
        /// 缺省的字段名。当没有指定字段名的时候，会自动用这个值来填充
        /// </summary>
        public static string DefaultFieldName
        {
            get
            {
                return new string(MarcQuery.DefaultChar, 3);
            }
        }

        /// <summary>
        /// 缺省的字段指示符值。当没有明确指定指示符值的时候，会自动用这个值来填充
        /// </summary>
        public static string DefaultIndicator
        {
            get
            {
                return new string(MarcQuery.DefaultChar, 2);
            }
        }

        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcField 对象
        /// </summary>
        public MarcField()
        {
            this.Parent = null;
            this.NodeType = NodeType.Field;
            Debug.Assert(this.ChildNodes.owner == this, "");

            this.Text = DefaultFieldName;
        }

        // 使用一个字符串构造
        /// <summary>
        /// 初始化一个 MarcField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strText">表示一个完整的 MARC 字段的 MARC 机内格式字符串</param>
        public MarcField(string strText)
        {
            this.Parent = null;
            this.NodeType = NodeType.Field;
            Debug.Assert(this.ChildNodes.owner == this, "");

            this.Text = strText;
        }

        // 使用两个或者三个字符串构造
        // 创建001等控制字段的时候，可以只使用前面两个参数，这时候第二参数表示内容部分
        // 如果在创建001等控制字段的时候，一共使用了三个参数，则 strIndicator + strContent 一起当作字段内容
        /// <summary>
        /// 初始化一个 MarcField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="strContent">字段正文</param>
        public MarcField(string strName,
            string strIndicator,
            string strContent = "")
        {
            this.NodeType = NodeType.Field;

            if (String.IsNullOrEmpty(strName) == true)
                this.m_strName = DefaultFieldName;
            else
            {
                if (strName.Length != 3)
                    throw new Exception("Field的Name必须为3字符");
                this.m_strName = strName;
            }

            if (isControlFieldName(strName) == true)
            {
                // strIndicator 和 strContent 连接在一起当作内容
                // 因为这个缘故，可以省略第三个参数
                this.Content = strIndicator + strContent;
                return;
            }
            this.m_strIndicator = strIndicator;
            this.Content = strContent;
        }

        // parameters:
        //      subfields   字符串数组，指定了一系列要创建的子字段文字。每个字符串型态需要这样 “axxx” 表示子字段名为a，内容为xxx。注意，不需要包含子字段符号 SUBFLD
        /// <summary>
        /// 初始化一个 MarcField 对象，并根据指定的字符串设置好全部内容和下级对象
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="subfields">表示若干子字段的字符串数组。每个字符串的第一字符为子字段名，其余为子字段内容</param>
        public MarcField(string strName,
            string strIndicator,
            List<string> subfields)
        {
            string[] temp = new string[subfields.Count];
            subfields.CopyTo(temp);

            newMarcField(strName,
                strIndicator,
                temp);
        }

        // 另一字符串数组版本
        /// <summary>
        /// 初始化一个 MarcField 对象，并根据指定的字符串设置好全部内容和下级对象
        /// </summary>
        /// <param name="strName">字段名。3字符的字符串</param>
        /// <param name="strIndicator">字段指示符。为2字符的字符串，或者空字符串</param>
        /// <param name="subfields">表示若干子字段的字符串数组。每个字符串的第一字符为子字段名，其余为子字段内容</param>
        public MarcField(string strName,
            string strIndicator,
            string[] subfields)
        {
            newMarcField(strName, strIndicator, subfields);
        }

        void newMarcField(string strName,
            string strIndicator,
            string [] subfields)
        {
            this.NodeType = NodeType.Field;

            if (String.IsNullOrEmpty(strName) == true)
                this.m_strName = DefaultFieldName;
            else
            {
                if (strName.Length != 3)
                    throw new Exception("Field的Name必须为3字符");
                this.m_strName = strName;
            }

            if (isControlFieldName(strName) == true)
            {
                throw new ArgumentException("不能用构造函数 NewMarcField(string strName, string strIndicator, string [] subfields) 创建控制字段", "strName");
            }

            this.m_strIndicator = strIndicator;

            StringBuilder content = new StringBuilder(4096);
            foreach (string s in subfields)
            {
                content.Append(MarcQuery.SUBFLD);
                content.Append(s);
            }

            this.Content = content.ToString();
        }

        // 使用一个字符串构造，指定了后面参数中所使用的(代用)子字段符号
        /// <summary>
        /// 初始化一个 MarcField 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="chSubfield">子字段符号的代用字符。在 strText 参数中可以用这个代用字符来表示子字段符号 (ASCII 31)</param>
        /// <param name="strText">表示一个完整的 MARC 字段的 MARC 机内格式字符串</param>
        public MarcField(char chSubfield, string strText)
        {
            this.Parent = null;
            this.NodeType = NodeType.Field;
            Debug.Assert(this.ChildNodes.owner == this, "");

            this.Text = strText.Replace(chSubfield, MarcQuery.SUBFLD[0]);
        }

        #endregion

        /// <summary>
        /// 当前节点的全部文字。表现了一个完整的 MARC 字段
        /// </summary>
        public override string Text
        {
            get
            {
#if NO
                // 头标区
                if (this.IsHeader == true)
                {
                    Debug.Assert(string.IsNullOrEmpty(this.Name) == true, "");
                    Debug.Assert(string.IsNullOrEmpty(this.Indicator) == true, "");
                    return this.Content;
                }
#endif

                // 普通字段
                return this.Name + this.Indicator + this.Content + MarcQuery.FLDEND;
            }
            set
            {
#if NO
                if (this.IsHeader == true)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        throw new Exception("头标区内容不能设置为空");
                    if (value.Length != 24)
                        throw new Exception("头标区内容只能设置为24字符");

                    this.Content = value;
                    return;
                }
#endif

                setFieldText(value);
            }
        }

        // 将机内格式的字符串设置到字段
        // 最后一个字符可以是 30 (表示字段结束)，也可以没有这个字符
        void setFieldText(string strText)
        {
            // 去掉末尾的 30 字符
            if (strText != null && strText.Length >= 1)
            {
                if (strText[strText.Length - 1] == (char)30)
                    strText = strText.Substring(0, strText.Length - 1);
            }

            if (string.IsNullOrEmpty(strText) == true)
                throw new Exception("字段 Text 不能设置为空");

            if (strText.Length < 3)
                throw new Exception("字段 Text 不能设置为小于 3 字符");

            string strFieldName = strText.Substring(0, 3);
            strText = strText.Substring(3); // 剩余部分

            this.m_strName = strFieldName;
            if (MarcNode.isControlFieldName(strFieldName) == true)
            {
                // 控制字段
                this.m_strIndicator = "";

                // 剩下的内容为空
                if (string.IsNullOrEmpty(strText) == true)
                {
                    this.Content = "";
                    return;
                }

                this.m_strContent = strText;    // 不要用this.Content，因为会惊扰到重新创建下级的机制
            }
            else
            {
                // 普通字段

                // 剩下的内容为空
                if (string.IsNullOrEmpty(strText) == true)
                {
                    this.Indicator = DefaultIndicator;
                    return;
                }

                // 还剩下一个字符
                if (strText.Length < 2)
                {
                    Debug.Assert(strText.Length == 1, "");
                    this.Indicator = strText + new string(MarcQuery.DefaultChar, 1);
                    return;
                }

                // 剩下两个字符以上
                this.m_strIndicator = strText.Substring(0, 2);
                this.Content = strText.Substring(2);
            }
        }

        /// <summary>
        /// 字段名
        /// </summary>
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true
                    || value.Length != 3)
                    throw new ArgumentException("MarcField 的 Name 属性只允许用 3 个字符来设置", "Name");

                string strOldName = this.Name;
                bool bOldIsControlField = isControlFieldName(strOldName);

                bool bNewIsControlField = isControlFieldName(value);

                string strOldContent = "";
                // 预先存储内容字符串
                if (bOldIsControlField != bNewIsControlField)
                {
                    if (bOldIsControlField == false)
                        strOldContent = this.m_strContent + this.Content;
                    else
                        strOldContent = this.Content;
                }

                base.Name = value;
                // 如果从控制字段转换为普通字段(或者反之)，Indicator要妥善处理
                if (bOldIsControlField != bNewIsControlField)
                {
                    if (bOldIsControlField == false)
                    {
                        // 200 --> 001，重构下级，重构时 Indicator归入m_strContent
                        Debug.Assert(this.m_strIndicator.Length == 2, "");
                        this.ChildNodes.clearAndDetach();
                        this.Content = this.m_strIndicator + strOldContent;
                        this.m_strIndicator = "";
                    }
                    else
                    {
                        // 001 --> 200，重构下级，重构时 Indicator不算在m_strContent内
                        Debug.Assert(this.m_strIndicator.Length == 0, "");
                        this.ChildNodes.clearAndDetach();
                        this.m_strIndicator = m_strContent.Substring(0, 2);
                        this.Content = strOldContent.Substring(2);
                    }
                }
            }
        }

        // 内容字符串。不包括指示符部分，不包括字段结束符
        /// <summary>
        /// 字段正文。即字段指示符以后的全部内容
        /// </summary>
        public override string Content
        {
            get
            {
                StringBuilder result = new StringBuilder(4096);
                result.Append( this.m_strContent );   // 第一个子字段符号以前的内容
                // 合成下级元素
                for (int i = 0; i < this.ChildNodes.count; i++)
                {
                    MarcNode node = this.ChildNodes[i];
                    result.Append(node.Text);
                    // strResult += new string((char)31, 1) + node.Name + node.Content;
                }
                return result.ToString();
            }
            set
            {
                // 拆分为子字段
                this.ChildNodes.clearAndDetach();
                this.m_strContent = "";

                if (isControlFieldName(this.Name) == true)
                {
                    // 需要检查内容里面的 31字符？其实也不必检查，因为来回修改字段名的时候，可能内容中会包含 31 字符，属于正常情况
                    this.m_strContent = value;
                    return;
                }

#if NO
                List<string> segments = new List<string>();
                StringBuilder prefix = new StringBuilder(); // 第一个 31 出现以前的一段文字
                StringBuilder segment = new StringBuilder(); // 正在追加的内容段落

                for (int i = 0; i < value.Length; i++)
                {
                    char ch = value[i];
                    if (ch == 31)
                    {
                        // 如果先前有累积的，推走
                        if (segment.Length > 0)
                        {
                            segments.Add(segment.ToString());
                            segment.Clear();
                        }

                        segment.Append(ch);
                    }
                    else
                    {
                        if (segment.Length > 0 || segments.Count > 0)
                            segment.Append(ch);
                        else
                            prefix.Append(ch);// 第一个子字段符号以前的内容放在这里
                    }
                }

                if (segment.Length > 0)
                {
                    segments.Add(segment.ToString());
                    segment.Clear();
                }

                if (prefix.Length > 0)
                    this.m_strContent = prefix.ToString();
                foreach (string s in segments)
                {
                    MarcSubfield subfield = new MarcSubfield(this);
                    if (s.Length < 2)
                        subfield.Text = MarcNode.SUBFLD + "?";  // TODO: 或者可以忽略?
                    else
                        subfield.Text = s;
                    this.ChildNodes.Add(subfield);
                    Debug.Assert(subfield.Parent == this, "");
                }
#endif

                string strLeadingString = "";
                MarcNodeList subfields = MarcQuery.createSubfields(
                    // this,
                    value, out strLeadingString);
                this.ChildNodes.add(subfields);
                this.m_strContent = strLeadingString;
            }
        }

        /// <summary>
        /// 字段正文前导字符串。字段指示符以后，第一个子字段符号以前的一段特殊内容。控制字段不具备这个部分
        /// </summary>
        public string Leading
        {
            get
            {
                // 控制字段没有leading内容
                if (this.IsControlField == true)
                    return "";

                return this.m_strContent;
            }
            set
            {
                if (this.IsControlField == true)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        return;
                    throw new Exception("控制字段没有不能设置(非空的) Leading 内容");
                }
                this.m_strContent = value;
            }
        }

        internal void ensureIndicatorChars()
        {
            if (string.IsNullOrEmpty(this.m_strIndicator) == true
    || this.m_strIndicator.Length < 2)
                this.m_strIndicator.PadRight(2, MarcQuery.DefaultChar);
        }

        /// <summary>
        /// 字段指示符的第一个字符
        /// </summary>
        public override char Indicator1
        {
            get
            {
                if (this.IsControlField == true)
                    return (char)0;
                ensureIndicatorChars();
                return this.m_strIndicator[0];
            }
            set
            {
                if (this.IsControlField == true)
                    return;
                ensureIndicatorChars();
                this.m_strIndicator = new string(value, 1) + this.m_strIndicator[1];
            }
        }

        /// <summary>
        /// 字段指示符的第二个字符
        /// </summary>
        public override char Indicator2
        {
            get
            {
                if (this.IsControlField == true)
                    return (char)0;
                ensureIndicatorChars();
                return this.m_strIndicator[1];
            }
            set
            {
                if (this.IsControlField == true)
                    return;
                ensureIndicatorChars();
                this.m_strIndicator = new string(this.m_strIndicator[0],1) + value;
            }
        }

        /// <summary>
        /// 当前字段节点是否为控制字段
        /// </summary>
        public bool IsControlField
        {
            get
            {
                return isControlFieldName(this.Name);
            }
        }

        // 常用名。等同于ChildNodes
        /// <summary>
        /// 当前字段节点的下级节点集合。相当于 ChildNodes 的别名
        /// </summary>
        public MarcNodeList Subfields
        {
            get
            {
                return this.ChildNodes;
            }
            set
            {
                this.ChildNodes.clearAndDetach();
                this.ChildNodes.add(value);
            }
        }

        // fangbian diaoyong
        /// <summary>
        /// 在下级节点末尾追加一个子字段节点
        /// </summary>
        /// <param name="subfield">字段节点</param>
        public void add(MarcSubfield subfield)
        {
            this.ChildNodes.add(subfield);

        }

        /// <summary>
        /// 输出当前对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public override string dump()
        {
            return this.Name + this.Indicator + this.m_strContent
                + dumpChildren();
        }

        /// <summary>
        /// 创建一个新的 MarcField 节点对象，从当前对象复制出全部内容和下级节点
        /// </summary>
        /// <returns>新的节点对象</returns>
        public override MarcNode clone()
        {
            MarcNode new_node = new MarcField();
            new_node.Text = this.Text;
            new_node.Parent = null; // 尚未和任何对象连接
            return new_node;
        }
    }

    // MARC 子字段
    /// <summary>
    /// MARC 子字段节点
    /// </summary>
    public class MarcSubfield : MarcNode
    {
        /// <summary>
        /// 缺省的子字段名。当没有指定子字段名的时候，会自动用这个值来填充
        /// </summary>
        public static string DefaultFieldName
        {
            get
            {
                return new string(MarcQuery.DefaultChar, 1);
            }
        }

        #region 构造函数

        /// <summary>
        /// 初始化一个 MarcSubfield 对象
        /// </summary>
        public MarcSubfield()
        {
            this.Parent = null;
            this.NodeType = NodeType.Subfield;
            this.Name = DefaultFieldName;
        }


        // 使用一个字符串构造
        // parameters:
        //      strText 可以为 SUBFLED + "aAAA" 形态，也可以为 "aAAA"形态
        /// <summary>
        /// 初始化一个 MarcSubfield 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strText">表示一个完整的 MARC 子字段的 MARC 机内格式字符串。第一字符可以为 ASCII 31，也可以为子字段名字符</param>
        public MarcSubfield(string strText)
        {
            this.NodeType = NodeType.Subfield;

            string strName = "";
            string strContent = "";
            if (string.IsNullOrEmpty(strText) == false)
            {
                if (strText[0] == (char)31)
                {
                    if (strText.Length > 1)
                    {
                        strName = strText.Substring(1, 1);
                        strContent = strText.Substring(2);
                    }
                }
                else
                {
                    strName = strText.Substring(0, 1);
                    strContent = strText.Substring(1);
                }
            }

            if (String.IsNullOrEmpty(strName) == true)
                this.Name = DefaultFieldName;
            else
            {
                if (strName.Length != 1)
                    throw new Exception("Subfield的Name必须为1字符");
                if (strName[0] == (char)31)
                    throw new Exception("子字段名不允许包含 ASCII 31 字符");

                this.Name = strName;
            }
            if (strContent.IndexOf((char)31) != -1)
                throw new Exception("子字段内容字符串中不允许包含 ASCII 31 字符");

            this.Content = strContent;
        }

        // 使用两个字符串构造
        /// <summary>
        /// 初始化一个 MarcSubfield 对象，并根据指定的字符串设置好全部内容
        /// </summary>
        /// <param name="strName">子字段名。一个字符</param>
        /// <param name="strContent">子字段正文</param>
        public MarcSubfield(string strName,
            string strContent)
        {
            this.NodeType = NodeType.Subfield;

            if (String.IsNullOrEmpty(strName) == true)
                this.Name = DefaultFieldName;
            else
            {
                if (strName.Length != 1)
                    throw new Exception("Subfield的Name必须为1字符");
                if (strName[0] == (char)31)
                    throw new Exception("子字段名不允许包含 ASCII 31 字符");

                this.Name = strName;
            }
            if (strContent.IndexOf((char)31) != -1)
                throw new Exception("子字段内容字符串中不允许包含 ASCII 31 字符");

            this.Content = strContent;
        }

        #endregion

        // 至少2字符
        /// <summary>
        /// 当前节点的全部文字。表现了一个完整的 MARC 子字段
        /// </summary>
        public override string Text
        {
            get
            {
                return MarcQuery.SUBFLD + this.Name + this.Content;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                    throw new Exception("子字段的 Text 不能设置为空");
                if (value.Length <= 1)
                    throw new Exception("子字段的 Text 不能设置为 1 字符的内容。至少要 2 字符，并且第一个字符必须为ASCII 31");
                if (value[0] != (char)31)
                    throw new Exception("子字段的 Text 第一个字符必须设置为ASCII 31");

                Debug.Assert(value.Length >= 2, "");
                this.Name = value.Substring(1, 1);
                this.Content = value.Substring(2);
            }
        }

        /// <summary>
        /// 子字段名。一字符
        /// </summary>
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true
                    || value.Length != 1)
                    throw new ArgumentException("MarcSubfield 的 Name 属性只允许用 1 个字符来设置", "Name");

                base.Name = value;
            }
        }

        /// <summary>
        /// 子字段正文。即子字段名以后的全部内容
        /// </summary>
        public override string Content
        {
            get
            {
                return this.m_strContent;
            }
            set
            {
                this.ChildNodes.clearAndDetach();
                this.m_strContent = value;
            }
        }



        /// <summary>
        /// 输出当前对象的调试用字符串
        /// </summary>
        /// <returns>表示内容的字符串</returns>
        public override string dump()
        {
            Debug.Assert(string.IsNullOrEmpty(this.Indicator) == true, "");
            return "$" + this.Name + this.Content;
        }

        /// <summary>
        /// 创建一个新的 MarcSubfield 节点对象，从当前对象复制出全部内容
        /// </summary>
        /// <returns>新的节点对象</returns>
        public override MarcNode clone()
        {
            MarcNode new_node = new MarcSubfield();
            new_node.Text = this.Text;
            new_node.Parent = null; // 尚未和任何对象连接
            return new_node;
        }
    }

    /// <summary>
    /// 专用于存储下级节点的集合类
    /// <para></para>
    /// </summary>
    /// <remarks>
    /// 继承 MarcNodeList 类而来，完善了 add() 方法，能自动把每个元素的 Parent 成员设置好
    /// </remarks>
    public class ChildNodeList : MarcNodeList
    {
        internal MarcNode owner = null;

        // 追加
        // 对node先要摘除
        /// <summary>
        /// 在当前集合末尾追加一个节点元素
        /// </summary>
        /// <param name="node">要追加的节点</param>
        public new void add(MarcNode node)
        {
            node.detach();
            base.add(node);

            Debug.Assert(owner != null, "");
            node.Parent = owner;
        }

        // 检查加入，不去摘除 node 原来的关系，也不自动修改 node.Parent
        internal void baseAdd(MarcNode node)
        {
            base.add(node);
        }

        // 追加
        /// <summary>
        /// 在当前集合末尾追加若干节点元素
        /// </summary>
        /// <param name="list">要追加的若干节点元素</param>
        public new void add(MarcNodeList list)
        {
            base.add(list);
            Debug.Assert(owner != null, "");
            foreach (MarcNode node in list)
            {
                node.Parent = owner;
            }
        }

        /// <summary>
        /// 向当前集合中添加一个节点元素，按节点名字顺序决定加入的位置
        /// </summary>
        /// <param name="node">要加入的节点</param>
        /// <param name="style">如何加入</param>
        /// <param name="comparer">用于比较大小的接口</param>
        public override void insertSequence(MarcNode node,
    InsertSequenceStyle style = InsertSequenceStyle.PreferHead,
    IComparer<MarcNode> comparer = null)
        {
            base.insertSequence(node, style, comparer);
            node.Parent = owner;
        }

        /// <summary>
        /// 清除当前集合，并把集合中的元素全部摘除
        /// </summary>
        public new void clear()
        {
            clearAndDetach();
        }

        // 清除集合，并把原先的每个元素的Parent清空。
        // 主要用于ChildNodes摘除关系
        internal void clearAndDetach()
        {
            foreach (MarcNode node in this)
            {
                node.Parent = null;
            }
            base.clear();
        }
    }

    /// <summary>
    /// 节点类型
    /// </summary>
    public enum NodeType
    {
        /// <summary>
        /// 尚未确定
        /// </summary>
        None = 0,
        /// <summary>
        /// 记录
        /// </summary>
        Record = 1,
        /// <summary>
        /// 字段
        /// </summary>
        Field = 2,
        /// <summary>
        /// 子字段
        /// </summary>
        Subfield = 3,
    }

    /// <summary>
    /// dump()方法的操作风格
    /// </summary>
    [Flags]
    public enum DumpStyle
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 包含行号
        /// </summary>
        LineNumber = 0x01,
    }

    /// <summary>
    /// MARC 记录的头标区
    /// </summary>
    public class MarcHeader
    {
        string m_strContent = "";

        /// <summary>
        /// 头标区所含字符数。固定为 24
        /// </summary>
        public static int FixedLength = 24;

        #region 各种功能位

        // 记录长度
        public string reclen
        {
            get
            {
                return this[0, 5];
            }
            set
            {
                this[0, 5] = value;
            }
        }

        public string status
        {
            get
            {
                return this[5, 1];
            }
            set
            {
                this[5, 1] = value;
            }
        }

        public string type
        {
            get
            {
                return this[6, 1];
            }
            set
            {
                this[6, 1] = value;
            }
        }

        public string level
        {
            get
            {
                return this[7, 1];
            }
            set
            {
                this[7, 1] = value;
            }
        }

        public string control
        {
            get
            {
                return this[8, 1];
            }
            set
            {
                this[8, 1] = value;
            }
        }

        public string reserve
        {
            get
            {
                return this[9, 1];
            }
            set
            {
                this[9, 1] = value;
            }
        }

        // 字段指示符长度
        public string indicount
        {
            get
            {
                return this[10, 1];
            }
            set
            {
                this[10, 1] = value;
            }
        }

        // 子字段标识符长度
        public string subfldcodecount
        {
            get
            {
                return this[11, 1];
            }
            set
            {
                this[11, 1] = value;
            }
        }

        // 数据基地址
        public string baseaddr
        {
            get
            {
                return this[12, 5];
            }
            set
            {
                this[12, 5] = value;
            }
        }

        public string res1
        {
            get
            {
                return this[17, 3];
            }
            set
            {
                this[17, 3] = value;
            }
        }

        // 目次区中字段长度部分
        public string lenoffld
        {
            get
            {
                return this[20, 1];
            }
            set
            {
                this[20, 1] = value;
            }
        }

        // 目次区中字段起始位置部分
        public string startposoffld
        {
            get
            {
                return this[21, 1];
            }
            set
            {
                this[21, 1] = value;
            }
        }

        // 实现者定义部分
        public string impdef
        {
            get
            {
                return this[22, 1];
            }
            set
            {
                this[22, 1] = value;
            }
        }

        public string res2
        {
            get
            {
                return this[23, 1];
            }
            set
            {
                this[23, 1] = value;
            }
        }

        #endregion

        // 2015/5/31
        // 按照UNIMARC惯例强制填充ISO2709头标区
        public void ForceUNIMARCHeader()
        {
            indicount = "2";
            subfldcodecount = "2";
            lenoffld = "4";   // 目次区中字段长度部分
            startposoffld = "5"; // 目次区中字段起始位置部分
        }

        /// <summary>
        /// 获取或设置头标区中任意一段长度的子字符串
        /// </summary>
        /// <param name="nStart">开始位置</param>
        /// <param name="nLength">长度。如果为-1，表示尽可能多。本参数可以缺省，缺省值为1</param>
        /// <returns>nStart 和 nLength 参数所表示范围的字符串</returns>
        public string this [int nStart, int nLength = 1]
        {
            get
            {
                if (nStart < 0 || nStart >= FixedLength)
                    throw new ArgumentException("nStart的取值范围应该是大于或等于 0，小于 " + FixedLength);
                if (nLength == -1)
                    nLength = FixedLength; 
                if (nStart + nLength > FixedLength)
                    throw new ArgumentException("nStart + nLength 应该小于或等于 " + FixedLength);
                if (nLength == 0)
                    return "";

                EnsureFixedLength();

                return this.m_strContent.Substring(nStart, nLength);
            }
            set
            {
                if (nStart < 0 || nStart >= FixedLength)
                    throw new ArgumentException("nStart的取值范围应该是大于或等于 0，小于 " + FixedLength);
                if (nLength == -1)
                    nLength = FixedLength;
                if (nStart + nLength > FixedLength)
                    throw new ArgumentException("nStart + nLength 应该小于或等于 " + FixedLength);
                if (value == null)
                {
                    if (nLength == 0)
                        return;
                    throw new ArgumentException("value 不应为 null");
                }
                if (value.Length < nLength)
                    throw new ArgumentException("value 的字符数不足 nLength 参数所指定的字符数 " + nLength);

                EnsureFixedLength();

                string strLeft = this.m_strContent.Substring(0, nStart);
                string strRight = this.m_strContent.Substring(nStart + nLength);
                this.m_strContent = strLeft + value.Substring(0, nLength) + strRight;
            }
        }

        // 确保内容为固定字符数
        void EnsureFixedLength()
        {
            if (this.m_strContent.Length < FixedLength)
                this.m_strContent = this.m_strContent.PadRight(FixedLength, MarcQuery.DefaultChar);
            else if (this.m_strContent.Length > FixedLength)
                this.m_strContent = this.m_strContent.Substring(0, FixedLength);
        }

        /// <summary>
        /// 获得表示整个头标区内容的字符串
        /// </summary>
        /// <returns>表示整个头标区内容的字符串</returns>
        public override string ToString()
        {
            EnsureFixedLength();
            return this.m_strContent;
        }

        // TODO: 设置头标区为UNIMARC或者MARC21缺省值的功能。和文献类型等参数有关，可能需要一个可变参数的函数
    }


}
