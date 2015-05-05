using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// Provides a cursor model for navigating and editing MARC data.
    /// </summary>
    class MarcNavigator : XPathNavigator
    {
        private NameTable m_nametable = null;
        internal NavigatorState m_navigatorState = null;

		/// <summary>
		/// 初始化一个 MarcNavigator 类的实例。
		/// </summary>
		/// <param name="node">出发点的 MarcNode 对象</param>
		public MarcNavigator(MarcNode node)
		{
#if NO
			//StreamUtil.WriteText("I:\\debug.txt","进到 构造函数XmlEditorNavigator(editor)里\r\n");

			Debug.Assert(node != null,"item不能为null");

            this.m_navigatorState = new NavigatorState();
            this.m_navigatorState.CurItem = new NaviItem(node, NaviItemType.Element);
            this.m_navigatorState.DocRoot = new NaviItem(node.Root, NaviItemType.Element);
            this.m_navigatorState.VirtualRoot = this.m_navigatorState.DocRoot;

            this.m_nametable = new NameTable();
            this.m_nametable.Add(String.Empty);
#endif
            Debug.Assert(node != null, "node不能为null");
            NaviItem item = new NaviItem(node, NaviItemType.Element);
            Initial(item);
		}

        /// <summary>
        /// 初始化一个 MarcNavigator 类的实例。
        /// </summary>
        /// <param name="item">出发点的 NaviItem 对象</param>
        public MarcNavigator(NaviItem item)
        {
            //StreamUtil.WriteText("I:\\debug.txt","进到 构造函数XmlEditorNavigator(editor)里\r\n");

            Initial(item);
        }

        void Initial(NaviItem item)
        {
            Debug.Assert(item != null, "item不能为null");

            this.m_navigatorState = new NavigatorState();
            this.m_navigatorState.CurItem = item;
            this.m_navigatorState.DocRoot = item.MarcNode.Root;
            // this.m_navigatorState.VirtualRoot = new NaviItem(item.MarcNode.Root, NaviItemType.VirtualRoot);

            this.m_nametable = new NameTable();
            this.m_nametable.Add(String.Empty);
        }

        /// <summary>
        /// 初始化一个 MarcNavigator 类的实例。
        /// </summary>
        /// <param name="navigator">参考的 MarcNavigator 对象</param>
        public MarcNavigator(MarcNavigator navigator) 
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 构造函数XmlEditorNavigator(navigator)里\r\n");

            this.m_navigatorState = new NavigatorState(navigator.m_navigatorState);
            this.m_nametable = (NameTable)navigator.NameTable;
		}

        /// <summary>
        /// 当前节点，代表当前光标位置
        /// </summary>
		public NaviItem Item
		{
			get
			{
				return this.m_navigatorState.CurItem;
			}
		}

        #region 重载的属性

        /// <summary>
        /// 当前节点的 XPathNodeType 类型
        /// </summary>
        public override XPathNodeType NodeType
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 NodeType属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                    return XPathNodeType.Root;	// 虚拟根

#if NO
                if (this.m_navigatorState.CurItem is AttrItem
                    && ((AttrItem)this.m_navigatorState.CurItem).IsNamespace == true)
                    return XPathNodeType.Namespace;
#endif
                if (this.m_navigatorState.CurItem.Type == NaviItemType.Element)
                    return XPathNodeType.Element;
                if (this.m_navigatorState.CurItem.Type == NaviItemType.Attribute)
                    return XPathNodeType.Attribute;
                if (this.m_navigatorState.CurItem.Type == NaviItemType.Text)
                    return XPathNodeType.Text;

                return XPathNodeType.All;
            }
        }

        /// <summary>
        /// 获得当前节点的不包括名字空间前缀的名字
        /// </summary>
        public override string LocalName
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 LocalName属性\r\n");

                // LocalName不包含前缀
                string strName = this.Name;
                int nIndex = strName.IndexOf(":");
                if (nIndex >= 0)
                {
                    strName = strName.Substring(nIndex + 1);
                }

                this.m_nametable.Add(strName);
                return this.m_nametable.Get(strName);
            }
        }

        /// <summary>
        /// 获得当前节点的 qualified 名字，也就是包含名字空间前缀的名字
        /// </summary>
        public override string Name
        {
            get
            {

                //StreamUtil.WriteText("I:\\debug.txt","进到 Name属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                    return "";	// 虚根

                // 包含前缀
                return this.m_navigatorState.CurItem.Name;
            }
        }

        /// <summary>
        /// 获得当前节点的名字空间 URI
        /// </summary>
        public override string NamespaceURI
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 NamespaceURI属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                // 实在没有
                return this.m_nametable.Get(String.Empty);
            }
        }

        /// <summary>
        /// 获得当前节点所关联的名字空间前缀
        /// </summary>
        public override string Prefix
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 Prefix属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                return this.m_nametable.Get(String.Empty);
            }
        }

        /// <summary>
        /// 获得当前节点的字符串值
        /// </summary>
        public override string Value
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 Value属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                    return "";
                    // return this.m_navigatorState.DocRoot.Value; // 虚根的内容正好相当于文档根的内容

                return this.m_navigatorState.CurItem.Value;
            }
        }

        /// <summary>
        /// 获得当前节点的 base URI
        /// </summary>
        public override String BaseURI
        {
            get
            {

                //StreamUtil.WriteText("I:\\debug.txt","进到 BaseURI属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                return "";
            }
        }

        /// <summary>
        /// 当前节点是否为一个没有结束标记的空元素
        /// </summary>
        public override bool IsEmptyElement
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 IsEmptyElement属性\r\n");

                if (this.m_navigatorState.CurItem.Type == NaviItemType.Element)
                {
                    return this.m_navigatorState.CurItem.MarcNode.isEmpty;
                }
                return false;
            }
        }

#if NO
        public override string XmlLang
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 XmlLang属性\r\n");

                string strLang = this.GetAttribute("lang",
                        "http://www.w3.org/2000/xmlns");
                return strLang;

            }
        }
#endif

        /// <summary>
        /// 获得 MarcNavigator 的 XmlNameTable
        /// </summary>
        public override XmlNameTable NameTable
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 NameTable属性\r\n");

                return this.m_nametable;
            }
        }

#if NO
        public override bool HasAttributes
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt","进到 HasAttributes属性\r\n");

                if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                    return false;

                return this.m_navigatorState.CurItem.HasAttributes;

                // 注:虚根也不算element
            }
        }
#endif


        #endregion

        #region 针对属性节点

#if NO
        public override string GetAttribute(string localName,
            string namespaceURI)
        {

            //StreamUtil.WriteText("I:\\debug.txt","进到 GetAttribute()\r\n");

            if (HasAttributes == false)
                return String.Empty;

            NaviItem current = this.m_navigatorState.CurItem;
            if (current.Type != NaviItemType.Element)
                return String.Empty;

            string strAttr = current.GetAttrValue(localName);
            if (string.IsNullOrEmpty(strAttr) == false)
                return strAttr;

            return String.Empty;
        }
#endif

        // 移动到最近的一个element,通常是上级
        void MoveToElement()
        {
            //StreamUtil.WriteText("I:\\debug.txt","进到 MoveToElement()\r\n");

            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
            {
                Debug.Assert(false, "");
                return;
            }

            NaviItem current = this.m_navigatorState.CurItem;
            if (current.Type == NaviItemType.Element)
                return;
            if (current.Type == NaviItemType.Attribute
                || current.Type == NaviItemType.Text)
            {
                Debug.Assert(current.MarcNode != null, "");
                this.m_navigatorState.CurItem = new NaviItem(current.MarcNode, NaviItemType.Element);
                return;
            }
            Debug.Assert(false, "");
        }

#if NO
        public override bool MoveToAttribute(string localName,
            string namespaceURI)
        {
            //StreamUtil.WriteText("I:\\debug.txt","进到 MoveToAttribute()\r\n");

            NaviItem current = this.m_navigatorState.CurItem;

            Debug.Assert(current.MarcNode != null, "");

            // 检查是否有这个属性名
            if (current.ExistAttr(localName) == false)
                return false;

            this.m_navigatorState.CurItem = new NaviItem(current.MarcNode, NaviItemType.Attribute);
            this.m_navigatorState.CurItem.AttrName = localName;

            return true;
        }
#endif

        /*
         * 
If the XPathNavigator is not currently positioned on an element, this method returns false and the position of the XPathNavigator does not change.
After a successful call to MoveToFirstAttribute, the LocalName, NamespaceURI and Prefix properties reflect the values of the attribute. When the XPathNavigator is positioned on an attribute, the methods MoveToNext, MoveToPrevious, and MoveToFirst are not applicable. These methods always return false and do not change the position of the XPathNavigator. Rather, you can call MoveToNextAttribute to move to the next attribute node.
After the XPathNavigator is positioned on an attribute, you can call MoveToParent to move to the owner element.
         * 
         * */
        /// <summary>
        /// 移动到第一个属性节点
        /// </summary>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToFirstAttribute()
        {
            ////Debug.WriteLine("MoveToFirstAttribute");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());

            if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                return false;

            Debug.Assert(this.m_navigatorState.CurItem.Type == NaviItemType.Element, "必须是元素节点");

            NaviItem current = this.m_navigatorState.CurItem;
            string strAttrName = current.FirstAttrName;
            if (strAttrName == null)
                return false;

            this.m_navigatorState.CurItem.AttrName = strAttrName;
            this.m_navigatorState.CurItem.Type = NaviItemType.Attribute;
            ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
            return true;
        }

        /*
         * 
If the XPathNavigator is not currently positioned on an attribute, this method returns false and the position of the XPathNavigator does not change.
When the XPathNavigator is positioned on an attribute, the methods MoveToNext, MoveToPrevious, and MoveToFirst methods are not applicable. These methods always return false and do not change the position of the XPathNavigator.
After the XPathNavigator is positioned on an attribute, you can call MoveToParent to move to the owner element.
         * 
         * */
        /// <summary>
        /// 移动到下一个属性节点
        /// </summary>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToNextAttribute()
        {
            ////Debug.WriteLine("MoveToNextAttribute");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());


            if (this.m_navigatorState.CurItem.Type != NaviItemType.Attribute)
                return false;

            Debug.Assert(this.m_navigatorState.CurItem.Type == NaviItemType.Attribute, "必须是属性节点");

            string strNextAttrName = this.m_navigatorState.CurItem.GetNextAttrName(this.m_navigatorState.CurItem.AttrName);
            if (strNextAttrName == null)
                return false;

            this.m_navigatorState.CurItem.AttrName = strNextAttrName;
            this.m_navigatorState.CurItem.Type = NaviItemType.Attribute;
            ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
            return true;
        }

        /// <summary>
        /// 移动到一个 ID 匹配要求值的属性节点
        /// </summary>
        /// <param name="id">想要移动到达的节点的 ID 值</param>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToId(string id)
        {
            ////Debug.WriteLine("MoveToId");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());

            return false;
        }
        #endregion

        #region 针对命名空间节点

#if NO
        public override string GetNamespace(string localname)
        {
            //StreamUtil.WriteText("I:\\debug.txt","GetNamespace() !");
            return String.Empty;
        }

        public override bool MoveToNamespace(string Namespace)
        {
            //StreamUtil.WriteText("I:\\debug.txt","进到 MoveToNamespace()\r\n");
            return false;
        }
#endif

        /// <summary>
        /// 移动到当前节点的第一个名字空间节点
        /// </summary>
        /// <param name="namespaceScope">一个表示名字空间 scope 的 XPathNamespaceScope 值</param>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            ////Debug.WriteLine("MoveToFirstNamespace");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());

            return false;
        }

        /// <summary>
        /// 移动到下一个名字空间节点
        /// </summary>
        /// <param name="namespaceScope">一个表示名字空间 scope 的 XPathNamespaceScope 值</param>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            ////Debug.WriteLine("MoveToNextNamespace");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            return false;
        }

        #endregion

        #region 针对普通节点,如element comment...的移动等操作

#if NO
        public override void MoveToRoot()
        {
            //StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToRoot() \r\n");
            this.m_navigatorState.CurItem = this.m_navigatorState.VirtualRoot;

            Debug.Assert(this.m_navigatorState.CurItem != null, "");
        }
#endif

        // Root     Root nodes do not have parents.
        // Element  Element or Root node.
        // Attribute    Element node.
        // Text     Element node.
        // Namespace    Element node.
        /// <summary>
        /// 移动到当前节点的父节点
        /// </summary>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToParent()
        {
            ////Debug.WriteLine("MoveToParent");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());

            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                return false;

            // 如果是属性节点，本函数的意思是移动到这个属性节点所从属的元素节点的parent?
            if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
            {
                MoveToElement();
                ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
                return true;
            }

            Debug.Assert(this.m_navigatorState.CurItem.Type == NaviItemType.Element, "");

            if (this.m_navigatorState.CurItem.MarcNode.Parent == null)
            {
                // 移动到虚拟根
                this.m_navigatorState.CurItem = new NaviItem(null, NaviItemType.VirtualRoot);
                return true;
            }

            this.m_navigatorState.CurItem.MarcNode = this.m_navigatorState.CurItem.MarcNode.Parent;
            ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
            return true;
        }

        // Root and Element are the only two XPathNodeTYpe values that have children. 
        // This property always returns false for all other XPathNodeType node types.
        /// <summary>
        /// 移动到当前节点的第一个子节点
        /// </summary>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToFirstChild()
        {
            ////Debug.WriteLine("MoveToFirstChild");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            // 虚根
            if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
            {
                if (this.m_navigatorState.DocRoot == null)
                {
                    Debug.Assert(false,
                        "DocRoot不能为null");
                    return false;
                }

                //StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirstChild() 当前是虚根 \r\n");

                this.m_navigatorState.CurItem = new NaviItem(this.m_navigatorState.DocRoot, NaviItemType.Element);
                ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
                return true;
            }

            //StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirstChild() \r\n");

            // Element 以外的都返回false
            if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                return false;

            if (this.m_navigatorState.CurItem.MarcNode.ChildNodes.count == 0)
                return false;

            this.m_navigatorState.CurItem.MarcNode = this.m_navigatorState.CurItem.MarcNode.ChildNodes[0];
            ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
            return true;
        }

#if NO
        public override bool MoveToFirst()
        {
            //StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirst() \r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                return false;
            if (this.m_navigatorState.CurItem == this.m_navigatorState.DocRoot)
                return false;

            if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                MoveToElement();
#if NO

            if (this.m_navigatorState.CurItem.Type == NaviItemType.Text)
                return false;

            // ???
            if (this.m_navigatorState.CurItem.Type == NaviItemType.Attribute)
            {
                Debug.Assert(false, "");
                string strAttrName = this.m_navigatorState.CurItem.FirstAttrName;
                if (strAttrName == null)
                    return false;
                this.m_navigatorState.CurItem.AttrName = strAttrName;
                return true;
            }
#endif

            MarcNode parent = this.m_navigatorState.CurItem.MarcNode.Parent;
            if (parent == null)
                return true;

            this.m_navigatorState.CurItem.MarcNode = parent.ChildNodes[0];
            return true;
        }
#endif

        // return true if the XPathNavigator is successful moving to the previous sibling node; otherwise , false if there is no previous sibling node or if the XPathNavigator is currently positioned on an attribute node.
        /// <summary>
        /// 移动到当前节点的前一个兄弟节点
        /// </summary>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToPrevious()
        {
            ////Debug.WriteLine("MoveToPrevious");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            // 已在虚根上
            if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                return false;

            // Attribute 节点返回 false。Text节点因为不会有兄弟，所以也返回 false
            if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                return false;

            MarcNode parent = this.m_navigatorState.CurItem.MarcNode.Parent;
            if (parent == null)
                return false;

            int nIndex = parent.ChildNodes.indexOf(this.m_navigatorState.CurItem.MarcNode);
            if (nIndex - 1 < 0)
                return false;	// 已经到兄弟开头

            this.m_navigatorState.CurItem.MarcNode = parent.ChildNodes[nIndex - 1];
            ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
            return true;
        }

        /// <summary>
        /// 移动到当前节点的下一个兄弟节点
        /// </summary>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveToNext()
        {
            ////Debug.WriteLine("MoveToNext");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            // 已在虚根上
            if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                return false;

            // Attribute 节点返回 false。Text节点因为不会有兄弟，所以也返回 false
            if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                return false;

            MarcNode parent = this.m_navigatorState.CurItem.MarcNode.Parent;
            if (parent == null)
                return false;

            int nIndex = parent.ChildNodes.indexOf(this.m_navigatorState.CurItem.MarcNode);
            if (nIndex == -1)
                throw new Exception("MarcNode在parent的ChildNodes中没有找到自己");

            if (nIndex >= parent.ChildNodes.count - 1)
                return false;	// 已经到兄弟末尾

            this.m_navigatorState.CurItem.MarcNode = parent.ChildNodes[nIndex + 1];
            ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
            return true;
        }

#if NO
        public override bool HasChildren
        {
            get
            {
                //StreamUtil.WriteText("I:\\debug.txt",this.Name + "--HasChildren() \r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem.Type != NaviItemType.Element)
                    return false;

                if (this.m_navigatorState.CurItem.MarcNode.ChildNodes.Count == 0)
                    return false;

                return true;
            }
        }
#endif


        #endregion

        #region 针对XPathNavigator对象

        /// <summary>
        /// 创建一个新的，和当前对象当前位置一样的 MarcNavigator 对象
        /// </summary>
        /// <returns>新的 MarcNavigator 对象</returns>
        public override XPathNavigator Clone()
        {
            ////Debug.WriteLine("Clone");
            return new MarcNavigator(this);
        }

        /// <summary>
        /// 移动当前位置到和指定的 XPathNavigator 对象一样的位置
        /// </summary>
        /// <param name="other">参考的 XPathNavigator 对象</param>
        /// <returns>如果移动成功，返回 true；否则返回 false。当返回 false 时，表示当前位置没有发生变动。</returns>
        public override bool MoveTo(XPathNavigator other)
        {
            ////Debug.WriteLine("MoveTo");
            ////Debug.WriteLine("*** Current " + this.m_navigatorState.Dump());
            if (other is MarcNavigator)
            {
                this.m_navigatorState = new NavigatorState(((MarcNavigator)other).m_navigatorState);
                ////Debug.WriteLine("*** Changed " + this.m_navigatorState.Dump());
                return true;
            }
            return false;
        }

        // 是否是同样的位置
        // parameters:
        //      other   XPathNavigator对象
        // return:
        /// <summary>
        /// 检测当前对象的位置和指定的 XPathNavigator 对象的位置是否相同
        /// </summary>
        /// <param name="other">要对比的 XPathNavigator 对象</param>
        /// <returns>是否相同。true表示相同，false表示不相同</returns>
        public override bool IsSamePosition(XPathNavigator other)
        {
            ////Debug.WriteLine("IsSamePosition");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");
            if (other is MarcNavigator)
            {
                if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                {
                    return (((MarcNavigator)other).m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot);
                }
                else
                {
                    if (this.m_navigatorState.CurItem.Type == NaviItemType.VirtualRoot)
                        return false;
                    return (this.m_navigatorState.CurItem.GetPath() == ((MarcNavigator)other).m_navigatorState.CurItem.GetPath());
                }
            }
            return false;
        }

        #endregion


        // *********************
		// This class keeps track of the state the navigator is in.
		internal class NavigatorState
		{
            public NaviItem CurItem = null;
            public MarcNode DocRoot = null;
            // public NaviItem VirtualRoot = null;

			public NavigatorState()
			{}

            public NavigatorState(NaviItem item)
			{
				CurItem = item;
				//VirtualRoot = new NaviItem(item.MarcNode.Root, NaviItemType.VirtualRoot);
				//Debug.Assert(VirtualRoot != null, "VirtualRoot不能为null");
                DocRoot = item.MarcNode.Root;
				Debug.Assert(DocRoot != null, "DocRoot不能为null");
			}

            // 复制构造函数
			public NavigatorState(NavigatorState NavState)
			{
				this.CurItem = new NaviItem(NavState.CurItem);  // 复制。位置参数都需要复制，因为复制的对象里面的这些状态以后可能被修改
				this.DocRoot = NavState.DocRoot;    // 引用。因为都是针对的同一个文档，文档本身并不需要复制
				// this.VirtualRoot = NavState.VirtualRoot;
			}

            public string Dump()
            {
                if (this.CurItem.Type == NaviItemType.VirtualRoot)
                    return "VirtualRoot";
                if (this.CurItem.Type == NaviItemType.Element)
                    return "Element" + ", MarcNode.Name=" + this.CurItem.MarcNode.Name;
                if (this.CurItem.Type == NaviItemType.Attribute)
                    return "Attribute" + ", AttrName="+this.CurItem.AttrName+", MarcNode.Name=" + this.CurItem.MarcNode.Name;
                if (this.CurItem.Type == NaviItemType.Text)
                    return "Text" + ", Text=" + this.CurItem.Value + ", MarcNode.Name=" + this.CurItem.MarcNode.Name;
                if (this.CurItem.Type == NaviItemType.None)
                    return "None";

                return "????";
            }
		}


    }
}
