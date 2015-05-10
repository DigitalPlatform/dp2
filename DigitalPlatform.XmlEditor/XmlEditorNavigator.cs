using System;
using System.Xml;
using System.Xml.XPath;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
	// 设计意图:
	// 有了这个类就可以通过XPath定位Item树中的节点
	public class XmlEditorNavigator : XPathNavigator
	{
		private NameTable m_nametable = null;
        internal NavigatorState m_navigatorState = null;



		// 注，测属性及其它非element节点,观察.net
		public XmlEditorNavigator(Item item)
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 构造函数XmlEditorNavigator(editor)里\r\n");

			Debug.Assert(item != null,"item不能为null");

			XmlEditor document = item.m_document;
			Debug.Assert(document != null,"document不能为null");

            this.m_navigatorState = new NavigatorState();
            this.m_navigatorState.CurItem = item;        //把当前节点设为虚根
            this.m_navigatorState.DocRoot = document.docRoot;
            this.m_navigatorState.VirtualRoot = document.VirtualRoot;

            this.m_nametable = new NameTable();
            this.m_nametable.Add(String.Empty);
		}

		public XmlEditorNavigator(XmlEditorNavigator navigator ) 
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 构造函数XmlEditorNavigator(navigator)里\r\n");

            this.m_navigatorState = new NavigatorState(navigator.m_navigatorState);
            this.m_nametable = (NameTable)navigator.NameTable;
		}

		public Item Item
		{
			get
			{
				return this.m_navigatorState.CurItem;
			}
		}

		#region 重载的属性

		public override XPathNodeType NodeType 
		{ 
			get 
			{
				//StreamUtil.WriteText("I:\\debug.txt","进到 NodeType属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
					return XPathNodeType.Root;	// 虚拟根

                if (this.m_navigatorState.CurItem is AttrItem
                    && ((AttrItem)this.m_navigatorState.CurItem).IsNamespace == true)
					return XPathNodeType.Namespace;
                if (this.m_navigatorState.CurItem is ElementItem)
					return XPathNodeType.Element;
                if (this.m_navigatorState.CurItem is AttrItem)
					return XPathNodeType.Attribute;
                if (this.m_navigatorState.CurItem is TextItem)
					return XPathNodeType.Text;

				return XPathNodeType.All;
			}
		}

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

		public override string Name 
		{ 
			get 
			{ 

				//StreamUtil.WriteText("I:\\debug.txt","进到 Name属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
					return "";	// 虚根
				// 包含前缀
                return this.m_navigatorState.CurItem.Name;
			}
		}

		public override string NamespaceURI 
		{
			get 
			{
				//StreamUtil.WriteText("I:\\debug.txt","进到 NamespaceURI属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

				// 虚根
                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
                    return this.m_nametable.Get(String.Empty);

                if (!(this.m_navigatorState.CurItem is ElementAttrBase))
                    return this.m_nametable.Get(String.Empty);

                ElementAttrBase element = (ElementAttrBase)this.m_navigatorState.CurItem;

				if (element.NamespaceURI != null)
				{
                    this.m_nametable.Add(element.NamespaceURI);
                    return this.m_nametable.Get(element.NamespaceURI);

				}

				// 实在没有
                return this.m_nametable.Get(String.Empty); 
			}
		}

		public override string Prefix 
		{
			get 
			{
				//StreamUtil.WriteText("I:\\debug.txt","进到 Prefix属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

				// 虚根
                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
                    return this.m_nametable.Get(String.Empty);

                if (!(this.m_navigatorState.CurItem is ElementAttrBase))
                    return this.m_nametable.Get(String.Empty);

                ElementAttrBase element = (ElementAttrBase)this.m_navigatorState.CurItem;


				if (element.Prefix != null)
				{
                    this.m_nametable.Add(element.Prefix);
                    return this.m_nametable.Get(element.Prefix);
				}

                return this.m_nametable.Get(String.Empty); 
			}
		}

		public override string Value
		{
			get 
			{ 
				//StreamUtil.WriteText("I:\\debug.txt","进到 Value属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
                    return this.m_navigatorState.DocRoot.GetValue(); // ???

                return this.m_navigatorState.CurItem.GetValue();
			}
		}

		public override String BaseURI 
		{
			get { 

				//StreamUtil.WriteText("I:\\debug.txt","进到 BaseURI属性\r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");


                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
					return "";

                if (!(this.m_navigatorState.CurItem is ElementAttrBase))
					return "";

                ElementAttrBase curItem = (ElementAttrBase)this.m_navigatorState.CurItem;

				return curItem.BaseURI;
			} 
		}

		public override bool IsEmptyElement
		{
			get 
			{
				//StreamUtil.WriteText("I:\\debug.txt","进到 IsEmptyElement属性\r\n");

                if (this.m_navigatorState.CurItem is ElementItem) 
				{
                    ElementItem element = (ElementItem)this.m_navigatorState.CurItem;

					return element.IsEmpty;
				}
				return false;
			}
		}

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

		public override XmlNameTable NameTable 
		{
			get 
			{ 
				//StreamUtil.WriteText("I:\\debug.txt","进到 NameTable属性\r\n");

                return this.m_nametable; 
			}
		}

		public override bool HasAttributes 
		{
			get
			{
				//StreamUtil.WriteText("I:\\debug.txt","进到 HasAttributes属性\r\n");

				if (!(this.m_navigatorState.CurItem is ElementItem))
					return false;

                ElementItem curItem = (ElementItem)this.m_navigatorState.CurItem;

				if (curItem.PureAttrList != null
					&& curItem.PureAttrList.Count > 0)
					return true;	// 有属性
				else
					return false;	// 注:虚根也不算element
			}
		}


		#endregion

		#region 针对属性节点

		public override string GetAttribute(string localName,
			string namespaceURI) 
		{

			//StreamUtil.WriteText("I:\\debug.txt","进到 GetAttribute()\r\n");

			if (HasAttributes == false)
				return String.Empty;

            ElementItem element = (ElementItem)this.m_navigatorState.CurItem;

			string strAttr = element.GetAttribute(localName,
				namespaceURI);
			if (strAttr != "")
				return strAttr;

			return String.Empty;
		}


		// 移动到最近的一个element,通常是上级
		void MoveToElement()
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToElement()\r\n");

			Debug.Assert(this.m_navigatorState.CurItem != null, "");

			if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
				return;

			if (!(this.m_navigatorState.CurItem is ElementItem)) 
			{
                this.m_navigatorState.CurItem = this.m_navigatorState.CurItem.parent;
			}
		}

		public override bool MoveToAttribute(string localName,
			string namespaceURI) 
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToAttribute()\r\n");


            if (!(this.m_navigatorState.CurItem is ElementItem))
				MoveToElement();

            if (!(this.m_navigatorState.CurItem is ElementItem)) 
			{
				return false;
			}

            ElementItem element = (ElementItem)this.m_navigatorState.CurItem;

			// namespace 增强???

			Item temp = element.GetAttrItem(localName);
			if (temp == null)
				return false;

            this.m_navigatorState.CurItem = temp;
			return true;
		}

		public override bool MoveToFirstAttribute() 
		{

			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToFirstAttribute()\r\n");

            if (!(this.m_navigatorState.CurItem is ElementItem))
				MoveToElement();

            Debug.Assert(this.m_navigatorState.CurItem is ElementItem, "必须是元素节点");

            ElementItem curItem = (ElementItem)this.m_navigatorState.CurItem;

			if (curItem.PureAttrList == null)
				return false;

			if (curItem.PureAttrList.Count == 0)
				return false;

            this.m_navigatorState.CurItem = curItem.PureAttrList[0];
			return true;
		}

		public override bool MoveToNextAttribute() 
		{

			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToNextAttribute()\r\n");


            if (!(this.m_navigatorState.CurItem is AttrItem))
				return false;

            Debug.Assert(this.m_navigatorState.CurItem is AttrItem, "必须是属性节点");

            ElementItem parent = (ElementItem)this.m_navigatorState.CurItem.parent;	// 属性的parent一定是ElementItem类型

			if (parent == null)
				return false;	// ???

            int nIndex = parent.PureAttrList.IndexOf(this.m_navigatorState.CurItem);
			if (nIndex + 1 >= parent.PureAttrList.Count)
				return false;	// 已经到兄弟末尾

            this.m_navigatorState.CurItem = parent.PureAttrList[nIndex + 1];

			return true;
		}


		public override bool MoveToId( string id ) 
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToId()\r\n");

			return false;
		}
		#endregion

		#region 针对命名空间节点

		public override string GetNamespace(string localname)
		{
			//StreamUtil.WriteText("I:\\debug.txt","GetNamespace() !");

            if (!(this.m_navigatorState.CurItem is ElementItem))
				return String.Empty;

            ElementItem element = (ElementItem)this.m_navigatorState.CurItem;

			ItemList namespaceList = element.NamespaceList;
			for(int i=0;i<namespaceList.Count;i++)
			{
				string strName = namespaceList[i].Name;
				int nIndex = strName.IndexOf(":");
				if (nIndex >= 0)
					strName = strName.Substring(nIndex + 1);

				if (strName == localname)
					return namespaceList[i].GetValue();
			}
			return String.Empty;
		}
		public override bool MoveToNamespace(string Namespace)
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToNamespace()\r\n");


            if (!(this.m_navigatorState.CurItem is ElementItem))
				return false;

            ElementItem element = (ElementItem)this.m_navigatorState.CurItem;


			ItemList namespaceList = element.NamespaceList;
			for(int i=0;i<namespaceList.Count;i++)
			{
				if (namespaceList[i].Name == Namespace)
				{
                    this.m_navigatorState.CurItem = namespaceList[i];
					return true;
				}
			}

			return false;
		}

		public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
		{

			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToFirstNamespace()\r\n");

            if (!(this.m_navigatorState.CurItem is ElementItem))
				return false;

            ElementItem element = (ElementItem)this.m_navigatorState.CurItem;

			ItemList namespaceList = element.NamespaceList;
			if (namespaceList.Count > 0)
			{
                this.m_navigatorState.CurItem = namespaceList[0];
				return true;
			}

			return false;
		}

		public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
		{
			//StreamUtil.WriteText("I:\\debug.txt","进到 MoveToNextNamespace()\r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");


            if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
				return false;

            if (!(this.m_navigatorState.CurItem is AttrItem))
				return false;

            AttrItem attr = (AttrItem)this.m_navigatorState.CurItem;

			if (attr.IsNamespace == false)
				return false;

			ElementItem element = (ElementItem)attr.parent;

			ItemList namespaceList = element.NamespaceList;
			if (namespaceList.Count > 0)
			{
                int nIndex = namespaceList.IndexOf(this.m_navigatorState.CurItem);
				if (nIndex == -1)
					return false;
				if (nIndex + 1 >= namespaceList.Count)
					return false;

                this.m_navigatorState.CurItem = namespaceList[nIndex + 1];
				return true;
			}

			return false;
		}
        

		#endregion

		#region 针对普通节点,如element comment...的移动等操作

		public override void MoveToRoot()
		{
			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToRoot() \r\n");
            this.m_navigatorState.CurItem = this.m_navigatorState.VirtualRoot;

            Debug.Assert(this.m_navigatorState.CurItem != null, "");

		}

		public override bool MoveToParent() 
		{
			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToParent() \r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
				return false;

            Item temp = this.m_navigatorState.CurItem.parent;
            this.m_navigatorState.CurItem = temp;
			return true;
		}

		public override bool MoveToFirstChild() 
		{

			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirstChild() \r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

			// 虚根
            if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot) 
			{
                if (this.m_navigatorState.DocRoot == null)
				{
					Debug.Assert(false,
                        "DocRoot不能为null");
					return false;
				}

				/*
				//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirstChild() 当前是虚根 \r\n");

				Debug.Assert(state.DocRoot != null,	"DocRoot不能为null");
				state.CurItem = state.DocRoot;

				return true;
				*/
			}

			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirstChild() \r\n");

            if (!(this.m_navigatorState.CurItem is ElementItem))
				return false;

            ElementItem parent = (ElementItem)this.m_navigatorState.CurItem;

			if (parent == null)
				return false;

			if (parent.children == null || parent.children.Count == 0) 
			{
				return false;
			}

            this.m_navigatorState.CurItem = parent.children[0];

			return true;
		}

		public override bool MoveToFirst() 
		{
			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToFirst() \r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");

            if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
				return true;

            if (this.m_navigatorState.CurItem is AttrItem)
				return false;

            ElementItem parent = (ElementItem)this.m_navigatorState.CurItem.parent; 

			Debug.Assert(parent != null, "除了虚根以外的其它任何element,都有parent");
			/*
			// 到虚根
			if (parent == null)
			{
				Debug.Assert(state.CurItem == state.DocRoot,"当前节点一定在实根上"); 
				return true;
			}
			*/

			if (parent.children == null || parent.children.Count == 0) 
			{
				Debug.Assert(false, "不太可能出现的情况....");
				return false;
			}

            this.m_navigatorState.CurItem = parent.children[0];

			return true;
		}

		public override bool MoveToPrevious() 
		{

			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToPrevious() \r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");


			// 已在虚根上
            if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
				return false;

            if (this.m_navigatorState.CurItem is AttrItem)
				return false;

            ElementItem parent = (ElementItem)this.m_navigatorState.CurItem.parent;
			
			Debug.Assert(parent != null, "除了虚根以外的其它任何element,都有parent");
			/*
			// 父亲是虚根，说明自己是实根
			if (parent == null)
			{
				return false;  // 实根是唯一的一个，没有兄弟，不能移到上一个
			}
			*/

			if (parent.children == null || parent.children.Count == 0) 
			{
				Debug.Assert(false, "不太可能出现的情况....");
				return false;
			}

            int nIndex = parent.children.IndexOf(this.m_navigatorState.CurItem);
			if (nIndex - 1 < 0)
				return false;	// 已经到兄弟开头

            this.m_navigatorState.CurItem = parent.children[nIndex - 1];

			return true;
		}

		public override bool MoveToNext() 
		{

			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToNext()1 \r\n");
            Debug.Assert(this.m_navigatorState.CurItem != null, "");


			// 已在虚根上
            if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
				return false;

            if (this.m_navigatorState.CurItem is AttrItem)
				return false;

            ElementItem parent = (ElementItem)this.m_navigatorState.CurItem.parent;

			Debug.Assert(parent != null, "除了虚根以外的其它任何element,都有parent");
			/*
			// 父亲是虚根，说明自己是实根
			if (parent == null)
			{
				//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--实根是唯一的一个，没有兄弟，不能移到下一个 \r\n");

				return false;  // 实根是唯一的一个，没有兄弟，不能移到下一个
			}
			*/

			//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--MoveToNext()2 \r\n");

			if (parent.children == null || parent.children.Count == 0) 
			{
				Debug.Assert(false, "不太可能出现的情况....");
				return false;
			}

            int nIndex = parent.children.IndexOf(this.m_navigatorState.CurItem);
			if (nIndex + 1 >= parent.children.Count)
				return false;	// 已经到兄弟末尾

            this.m_navigatorState.CurItem = parent.children[nIndex + 1];

			return true;
		}
        

		public override bool HasChildren 
		{
			get 
			{ 
				//StreamUtil.WriteText("I:\\debug.txt",this.Name + "--HasChildren() \r\n");
                Debug.Assert(this.m_navigatorState.CurItem != null, "");

				/*
				// 在虚根上
				if (state.CurItem == null) 
				{
					if (state.DocRoot != null)
						return true;
					else
					{
						Debug.Assert(false,"?在虚根上，没有实根是不可能的情况");
					}
					return false;
				}
				*/

                if (!(this.m_navigatorState.CurItem is ElementItem)) 
				{
                    Debug.Assert(!(this.m_navigatorState.CurItem is VirtualRootItem), 
						"好奇怪. 一个VirtualRootItem也应同时是一个ElementItem");
					return false;
				}

                ElementItem element = (ElementItem)this.m_navigatorState.CurItem;

				if (element.children == null)
					return false;

				if (element.children.Count == 0)
					return false;

				return true;
			}
		}
        
		#endregion

		#region 针对XPathNavigator对象

		public override XPathNavigator Clone() 
		{
			return new XmlEditorNavigator(this);
		}

		public override bool MoveTo( XPathNavigator other ) 
		{
			if (other is XmlEditorNavigator )
			{
                this.m_navigatorState = new NavigatorState(((XmlEditorNavigator)other).m_navigatorState);
				return true;
			}
			return false;
		}

        // 是否是同样的位置
        // parameters:
        //      other   XPathNavigator对象
        // return:
        public override bool IsSamePosition(XPathNavigator other)
        {
            Debug.Assert(this.m_navigatorState.CurItem != null, "");
            if (other is XmlEditorNavigator)
            {
                if (this.m_navigatorState.CurItem == this.m_navigatorState.VirtualRoot)
                {
                    return (((XmlEditorNavigator)other).m_navigatorState.CurItem == ((XmlEditorNavigator)other).m_navigatorState.VirtualRoot);
                }
                else
                {
                    if (((XmlEditorNavigator)other).m_navigatorState.CurItem == ((XmlEditorNavigator)other).m_navigatorState.VirtualRoot)
                        return false;
                    return (this.m_navigatorState.CurItem.GetXPath() == ((XmlEditorNavigator)other).m_navigatorState.CurItem.GetXPath());
                }
            }
            return false;
        }
    
		#endregion

		// *********************
		// This class keeps track of the state the navigator is in.
		internal class NavigatorState
		{
			public Item CurItem = null;
			public Item DocRoot = null;
			public Item VirtualRoot = null;

			public NavigatorState()
			{}

			public NavigatorState(Item item)
			{
				CurItem = item;
				VirtualRoot = item.m_document.VirtualRoot;
                Debug.Assert(VirtualRoot != null, "VirtualRoot不能为null");
				DocRoot = item.m_document.docRoot;
				Debug.Assert(DocRoot != null,"DocRoot不能为null");
			}

			public NavigatorState(NavigatorState NavState)
			{
				this.CurItem = NavState.CurItem;
				this.DocRoot = NavState.DocRoot;
				this.VirtualRoot = NavState.VirtualRoot;
			}
		}

	}
}
