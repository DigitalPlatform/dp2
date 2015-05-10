using System;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform.Text;


namespace DigitalPlatform.Xml
{
	// 元素节点
	public class ElementItem : ElementAttrBase
	{

		internal ItemList attrs = new ItemList(); // 属性集合, 处理普通属性节点，也包含名字空间节点   
		internal ItemList children = new ItemList(); // 儿子集合,除了元素节点、注释等等，也可包含文本节点

		internal ExpandStyle m_childrenExpand = ExpandStyle.None; // 儿子元素集合图像状态
		internal ExpandStyle m_attrsExpand = ExpandStyle.None; // 属性集合图像状态

		internal int m_bWantAttrsInitial = -1;  // -1 未赋值 0 不用改变 1希望改变
		internal int m_bWantChildInitial = -1;  // -1 未赋值 0 不用改变 1希望改变

		internal int m_xmlAttrsTimestamp = 0;  // 属性xml编辑框时间戳
		internal int m_objAttrsTimestamp = 0;  // 内存对象的时间戳

		internal int m_xmlChildrenTimestamp = 0; // 儿子xml编辑框时间戳
		internal int m_objChildrenTimestmap = 0; // 内存对象的时间戳

		public bool IsEmpty = false; // 一个节点是否是<aa/>格式,只有元素节点是才有意思


		internal ElementItem(XmlEditor document)
		{
			this.m_document = document;
			this.m_paraValue1 = null;
		}


		// 初始化数据，从node中获取
		public virtual void InitialData(XmlNode node)
		{
			this.Name = node.Name;
			this.Prefix = node.Prefix;

			if (node.NodeType == XmlNodeType.Element)
			{
				XmlElement elementNode = (XmlElement)node;
				this.IsEmpty = elementNode.IsEmpty;
			}
			this.LocalName = node.LocalName;
			this.m_strTempURI = node.NamespaceURI;
		}

		public void SetStyle(ElementInitialStyle style)
		{

			// 本对象旧的状态
			ExpandStyle oldChildExpand = this.m_childrenExpand;
			ExpandStyle oldAttrsExpand = this.m_attrsExpand;

			// 参数传进的新状态
			ElementInitialStyle elementStyle = style;
			ExpandStyle newAttrsExpand = elementStyle.attrsExpandStyle;
			ExpandStyle newChildExpand = elementStyle.childrenExpandStyle;

			// 先设成不用改变状态
			this.m_bWantAttrsInitial = 0;
			this.m_bWantChildInitial = 0;

			// 设属性是否需要改变状态
			if (elementStyle.bReinitial == false)	// 首次初始化
			{
				this.m_bWantAttrsInitial = 1;
			}
			else 
			{
				if (newAttrsExpand != oldAttrsExpand) 
					this.m_bWantAttrsInitial = 1;
				else
					this.m_bWantAttrsInitial = 0;
			}

			// 设儿子状态
			if (elementStyle.bReinitial == false)	// 首次初始化
			{
				this.m_bWantChildInitial = 1;
			}
			else
			{
				if (newChildExpand != oldChildExpand)
					this.m_bWantChildInitial = 1;
				else
					this.m_bWantChildInitial = 0;
			}
		}

		// 用node初始化本对象和下级
		// return:
		//		-1  出错
		//		-2  中途cancel
		//		0   成功
		public override int Initial(XmlNode node, 
			ItemAllocator allocator,
			object style,
            bool bConnect)
		{
			// this.m_bConnected = true;
            this.m_bConnected = bConnect;   // new change

			if (!(style is ElementInitialStyle))
			{
				Debug.Assert(false,"style必须为ElementInitialStyle类型");
				return -1;
			}

			if ((node.NodeType != XmlNodeType.Element)
				&& node.NodeType != XmlNodeType.Document)
			{
				Debug.Assert(false, "'" + node.NodeType.ToString() + "'不是合适的节点类型");
				return 0;
			}

			// 设状态
			this.SetStyle((ElementInitialStyle)style);

			// 参数传进的新状态
			ElementInitialStyle elementStyle = (ElementInitialStyle)style;
			ExpandStyle newAttrsExpand = elementStyle.attrsExpandStyle;
			ExpandStyle newChildExpand = elementStyle.childrenExpandStyle;

			// 初始化数据，从node中获取
			this.InitialData(node);

			// 处理属性
			if (this.m_bWantAttrsInitial == 1) 
			{
				if (node.Attributes == null
					|| node.Attributes.Count == 0)
				{
					this.m_attrsExpand = ExpandStyle.None;
					goto SKIPATTRS;
				}

				// 本次希望属性展开
				if (newAttrsExpand == ExpandStyle.Expand)
				{
					this.ClearAttrs();
					foreach(XmlNode attrXmlNode in node.Attributes )
					{
						AttrItem attr = null;

						attr = (AttrItem)allocator.newItem(attrXmlNode,
							this.m_document);
						if (attr != null) 
						{
							int nRet = attr.Initial(attrXmlNode, 
								allocator,
								null,
                                true);
							if (nRet == -1)
								return -1;

							this.AppendAttrInternal(attr,false,true);


						}

						attr.m_bConnected = true;
					}

					this.m_objAttrsTimestamp ++ ;
					this.m_attrsExpand = ExpandStyle.Expand;
				}
				else if (newAttrsExpand == ExpandStyle.Collapse)
				{
					// 注,收缩时不要把属性毁掉

					// 本次希望属性收缩
					this.m_attrsExpand = ExpandStyle.Collapse;
				}
				else
				{
					this.m_attrsExpand = ExpandStyle.None;
				}
			}

			SKIPATTRS:

				// 处理下级
				if (this.m_bWantChildInitial == 1)
				{
					if (node.ChildNodes.Count == 0)  //没有null的情况
					{
						this.m_childrenExpand = ExpandStyle.None;
						goto SKIPCHILDREN;
					}

					// 希望下级展开
					if (newChildExpand == ExpandStyle.Expand)
					{
						if (elementStyle.bReinitial == false
							|| this.m_xmlChildrenTimestamp > this.m_objChildrenTimestmap)
						{
							this.ClearChildren();

							// 处理下级
							foreach(XmlNode child in node.ChildNodes) 
							{
								Item item;
								item = allocator.newItem(child,
									this.m_document);

								if (item == null)
									continue;


								ElementInitialStyle childStyle = 
									new ElementInitialStyle();
								childStyle.attrsExpandStyle = ExpandStyle.Expand;
								childStyle.childrenExpandStyle = ExpandStyle.Expand;
								childStyle.bReinitial = false;
								int nRet = item.Initial(child,
									allocator,
									childStyle,
                                    bConnect);  // 继承外面传入的参数
								if (nRet == -2)
									return -2;
								if (nRet <= -1)
								{
									return nRet;
								}

								// 这里不在AppendChildInternal里做flush的原因是
								// Initial()阶段表示只修改的一次，不是每个元素修改父亲一次
								this.AppendChildInternal(item,false,true);

								//item.m_bConnected = true;
							}
							this.m_objChildrenTimestmap ++;
						}
						this.m_childrenExpand = ExpandStyle.Expand;
					
					}
					else if (newChildExpand == ExpandStyle.Collapse)
					{
						this.m_childrenExpand = ExpandStyle.Collapse;
					}
					else
					{
						this.m_childrenExpand = ExpandStyle.None;
					}
				}

			SKIPCHILDREN:
				/*
							// end
							ItemCreatedEventArgs args = new ItemCreatedEventArgs();
							args.item = this;
							args.bInitial = true;
							this.m_document.fireItemCreated(this,args);
				*/

				return 0;
		}



		// 完成重载基类的InitialVisual()
		public override void InitialVisual()
		{
			bool bHasVisual = false;
			Label label = this.GetLable();

			if (label == null)
			{
				// 加Label
				label = new Label();
				label.container = this;
				label.Text = this.Name;
				this.AddChildVisual(label);
			}
			else
			{
				label.Text = this.Name;
				bHasVisual = true;
			}

			Box boxTotal = null;
			if (bHasVisual == false)
			{
				// 定义一个总框
				boxTotal = new Box ();
				boxTotal.Name = "BoxTotal";
				boxTotal.container = this;
				this.AddChildVisual (boxTotal);

				// 外面总框的layoutStyle样式为竖排
				boxTotal.LayoutStyle = LayoutStyle.Vertical;
			}
			else
			{
				boxTotal = this.GetBoxTotal();
				if (boxTotal == null)
				{
					Debug.Assert(false,"有Lable对象,不可能没有BoxTotal对象");
					throw new Exception("有Lable对象,不可能没有BoxTotal对象");
				}
			}

			///
			if (boxTotal != null)
				InitialVisualSpecial(boxTotal);
			///

			/*
						if (bHasLable == false)
						{
							//如果boxTotal只有一个box，则设为横排
							if (boxTotal.childrenVisual != null && boxTotal.childrenVisual .Count == 1)
								boxTotal.LayoutStyle = LayoutStyle.Horizontal ;

							Comment comment = new Comment ();
							comment.container = this;
							this.AddChildVisual(comment);
						}
			*/	
		
			this.m_strTempURI = null;

		}


		public override void InitialVisualSpecial(Box boxTotal)
		{
			boxTotal.LayoutStyle = LayoutStyle.Vertical;
			if (this.m_bWantAttrsInitial == 1)
			{
				// 找到旧的BoxAttributes,删掉
				Box oldBoxAttributes = this.GetBoxAttributes(boxTotal);
				if (oldBoxAttributes != null)
					boxTotal.childrenVisual.Remove(oldBoxAttributes);

				if (this.m_attrsExpand != ExpandStyle.None)
				{
					if (this.m_attrsExpand == ExpandStyle.Expand
						&& this.attrs.Count == 0)
					{
						goto SKIPATTRS;
					}
					//定义包含展开按钮和属性的大框
					Box boxAttrs = new Box ();
					boxAttrs.Name = "BoxAttributes";
					boxAttrs.LayoutStyle = LayoutStyle.Horizontal;
					boxAttrs.container = boxTotal;

					//定义展开按钮
					ExpandHandle expandAttributesHandle = new ExpandHandle ();
					expandAttributesHandle.Name = "ExpandAttributes";
					expandAttributesHandle.container = boxAttrs;
					boxAttrs.AddChildVisual(expandAttributesHandle);

					//定义属性框
					Attributes attributes = new Attributes();
					attributes.container = boxAttrs;
					attributes.LayoutStyle = LayoutStyle.Vertical; //一般子元素都是竖排，横排只需修改这里
					boxAttrs.AddChildVisual(attributes);

					// 属性大框加到总大框里
					// 属性大框永远排在总大框的第一位
					if (boxTotal.childrenVisual == null)
						boxTotal.childrenVisual = new ArrayList();
					boxTotal.childrenVisual.Insert(0,boxAttrs);//.AddChildVisual (boxAttrs);

					//属性框初始化
					attributes.InitialVisual();   //这句话很重要
				}
			}

			SKIPATTRS:

				if (this.m_bWantChildInitial == 1)
				{
					// 找到旧的BoxContent,删掉
					Box oldBoxContent = this.GetBoxContent(boxTotal);
					if (oldBoxContent != null)
						boxTotal.childrenVisual.Remove(oldBoxContent);


					if (this.m_childrenExpand != ExpandStyle.None)
					{
						if (this.m_childrenExpand == ExpandStyle.Expand
							&& this.children.Count == 0)
						{
							return;
						}
						Box boxContent = new Box ();
						boxContent.Name = "BoxContent";
						boxContent.container = boxTotal;
						boxContent.LayoutStyle = LayoutStyle.Horizontal;

						ExpandHandle expandContentHandle = new ExpandHandle();
						expandContentHandle.Name = "ExpandContent";
						expandContentHandle.container = boxContent;
						boxContent.AddChildVisual(expandContentHandle);

						Content content = new Content();
						content.container = boxContent;
						content.LayoutStyle = LayoutStyle.Vertical  ;//Vertical ;
						boxContent.AddChildVisual(content);

						boxTotal.AddChildVisual(boxContent);
					
						//调入contna的初始化visual函数
						content.InitialVisual();
					}
				}
		}


	
		// 重建attributes内部结构
		public void AttributesReInitial()
		{
			Attributes attributes = this.GetAttributes();
			if (attributes == null)
			{
				this.InitialVisual();
				return;
			}
			else
			{
				attributes.InitialVisual();
			}
		}


		// 重建content内部结构
		public void ContentReInitial()
		{
			Content content = this.GetContent();
			if (content == null)
			{
				this.InitialVisual();
				return;
			}
			else
			{
				content.InitialVisual();		
			}
		}


		public Box GetBoxAttributes(Box boxTotal)
		{
			if (boxTotal.childrenVisual == null)
				return null;

			Box boxAttributes = null;
			for(int i=0;i<boxTotal.childrenVisual.Count;i++)
			{
				Visual visual = (Visual)boxTotal.childrenVisual[i];
				if (visual.Name == "BoxAttributes")
				{
					boxAttributes = (Box)visual;
					break;
				}
			}
			return boxAttributes;
		}

		public Box GetBoxContent(Box boxTotal)
		{
			if (boxTotal.childrenVisual == null)
				return null;

			Box boxContent = null;
			for(int i=0;i<boxTotal.childrenVisual.Count;i++)
			{
				Visual visual = (Visual)boxTotal.childrenVisual[i];
				if (visual.Name == "BoxContent")
				{
					boxContent = (Box)visual;
					break;
				}
			}
			return boxContent;
		}

		public Content GetContent()
		{
			Box boxTotal = this.GetBoxTotal();
			if (boxTotal == null)
				return null;

			Box boxContent = this.GetBoxContent(boxTotal);
			if (boxContent == null)
				return null;

			foreach(Visual visual in boxContent.childrenVisual )
			{
				if (visual is Content )
					return (Content)visual;
			}
			return null;
		}

		
		// 从box里找到Content
		public Attributes GetAttributes()
		{
			Box boxTotal = this.GetBoxTotal();
			if (boxTotal == null)
				return null;

			Box boxAttributes = this.GetBoxAttributes(boxTotal);
			if (boxAttributes == null)
				return null;

			foreach(Visual visual in boxAttributes.childrenVisual )
			{
				if (visual is Attributes )
					return (Attributes)visual;
			}
			return null;
		}

		
		//得到attributes关闭时的text
		public Text GetAttributesText()
		{
			Box boxTotal = this.GetBoxTotal();
			if (boxTotal == null)
				return null;

			Box boxAttributes = this.GetBoxAttributes(boxTotal);
			if (boxAttributes == null)
				return null;

			foreach(Visual visual in boxAttributes.childrenVisual )
			{
				if (visual is Text )
					return (Text)visual;				
			}
			return null;
		}

		
		// 得到content关闭时的text
		public Text GetContentText()
		{
			Box boxTotal = this.GetBoxTotal();
			if (boxTotal == null)
				return null;

			Box boxContent = this.GetBoxContent(boxTotal);
			if (boxContent == null)
				return null;

			foreach(Visual visual in boxContent.childrenVisual )
			{
				if (visual is Text )
					return (Text)visual;				
			}
			return null;
		}


		public ItemList Children
		{
			get
			{
				return this.children;
			}
		}

		public ItemList Attributes
		{
			get
			{
				return this.attrs;
			}
		}

		public override string NamespaceURI 
		{
			get 
			{
				if (m_strTempURI != null)
					return m_strTempURI;	// 在插入前或摘除后起作用

				string strURI = "";
				AttrItem namespaceAttr = null;
				// 根据一个前缀字符串, 从起点元素开始查找, 看这个前缀字符串是在哪里定义的URI。
				// 也就是要找到xmlns:???=???这样的属性对象，返回在namespaceAttr参数中。
				// 本来从返回的namespaceAttr参数中可以找到命中URI信息，但是为了使用起来方便，
				// 本函数也直接在strURI参数中返回了命中的URI
				// parameters:
				//		startItem	起点element对象
				//		strPrefix	要查找的前缀字符串
				//		strURI		[out]返回的URI
				//		namespaceAttr	[out]返回的AttrItem节点对象
				// return:
				//		ture	找到(strURI和namespaceAttr中有返回值)
				//		false	没有找到
				bool bRet = ItemUtil.LocateNamespaceByPrefix(
					this,
					this.Prefix,
					out strURI,
					out namespaceAttr);
				if (bRet == false) 
				{
					if (this.Prefix == "")
						return "";
					return null;
				}
				return strURI;
			}
		}

		public ExpandStyle AttrsExpand
		{
			get
			{
				return this.m_attrsExpand;
			}
			set
			{
				this.m_attrsExpand = value;
			}
		}

		public ExpandStyle ChildrenExpand
		{
			get
			{
				return this.m_childrenExpand;
			}
			set
			{
				this.m_childrenExpand = value;
			}
		}


		public static XmlNamespaceManager GatherOuterNamespaces(
			ElementItem element,
			NameTable nt)
		{
			XmlNamespaceManager nsColl = new XmlNamespaceManager(nt);

			ElementItem current = element;
			string strName = element.Name;
		
			while(true)
			{
				if (current == null
					|| current == current.m_document.VirtualRoot)
				{
					break;
				}

				nsColl.PushScope();

				foreach(AttrItem attr in current.attrs)
				{
					if (attr.IsNamespace == false)
						continue;

					nsColl.AddNamespace(attr.LocalName, attr.GetValue());	// 只要prefix重就不加入
				}

				current = (ElementItem)current.parent;
			}

			return nsColl;
		}



		// 根据xml编辑框中的内容创建属性
		void BuildAttrs()
		{
			//this.SetFreshValue();

			Debug.Assert(m_attrsExpand == ExpandStyle.Collapse, "不是闭合状态不要用本函数");

			NameTable nt = new NameTable();

			// 创建名字空间
			XmlNamespaceManager nsmgr = GatherOuterNamespaces(
				this,
				nt);

			XmlParserContext context = new XmlParserContext(nt,
				nsmgr,
				null,
				XmlSpace.None);


			string strAttrsXml = "";
			// 2.得到attributes关闭时的text
			Text oText = this.GetAttributesText();
			if (oText != null) 
			{
				strAttrsXml = oText.Text.Trim();

				if (strAttrsXml != "")
					strAttrsXml = " " + strAttrsXml;
			}
			else 
			{
				Debug.Assert(false, "必须有text对象");
			}

			string strFragmentXml = "<" + this.Name + strAttrsXml + "/>";

            /*
            TextReader tr = new StringReader(strFragmentXml);
            XmlReaderSettings settings = new XmlReaderSettings();
            settings..XmlResolver = resolver;

            XmlReader reader = XmlReader.Create(tr, settings,
                context);
             * */

			// 过一段时间做XmlSchema校验
			XmlValidatingReader reader =
				new XmlValidatingReader(strFragmentXml,
				XmlNodeType.Element,
				context);
			
			// 不根据schema进行校验
			reader.ValidationType = ValidationType.None; 

			this.attrs = new ItemList();

			while(true)
			{
				if (reader.Read() == false)
					break;

				if (reader.MoveToFirstAttribute() == false)
					goto CONTINUE;

				while(true)
				{
					AttrItem attr = this.m_document.CreateAttrItem(reader.Name);
					attr.Initial(reader);
					this.AppendAttrInternal(attr,false,true);
					if (reader.MoveToNextAttribute() == false)
						break;
				}

			CONTINUE:
				break;
			}
		}


		// 根据xml编辑框中的内容创建后代
		void BuildDescendant()
		{
			// 采用加虚根的方法

			//this.SetFreshValue();

			Debug.Assert(this.m_childrenExpand == ExpandStyle.Collapse, "不是闭合状态不要用本函数");

			string strAdditionalNsString = "";

			string strInnerXml = "";
			// 2.得到attributes关闭时的text
			Text oText = this.GetContentText();
			if (oText != null) 
			{
				strInnerXml = oText.Text.Trim();
			}
			else 
			{
				Debug.Assert(false, "必须有text对象");
			}

			NamespaceItemCollection nsColl = NamespaceItemCollection.GatherOuterNamespaces(
				(ElementItem)this);

			if (nsColl.Count > 0)
			{
				strAdditionalNsString = nsColl.MakeAttrString();
			}

			string strXml = "";
			if (this == this.m_document.VirtualRoot)
			{
				strXml = strInnerXml;
			}
			else
			{
				strXml = "<root "+ strAdditionalNsString + " >\r\n" + strInnerXml + "</root>";
			}

			this.ClearChildren();

			this.children = new ItemList();


			XmlDocument dom = new XmlDocument();
			dom.LoadXml(strXml);

			XmlNode root = null;
			if (this == this.m_document.VirtualRoot)
			{
				root = dom;
			}
			else
			{
				root = dom.DocumentElement;
			}

			foreach(XmlNode node in root.ChildNodes)
			{
				Item item = this.m_document.allocator.newItem(node,
					this.m_document);

			
				ElementInitialStyle style = new ElementInitialStyle();
				style.attrsExpandStyle = ExpandStyle.Expand;
				style.childrenExpandStyle = ExpandStyle.Expand;
				style.bReinitial = false;

				item.Initial(node,
					this.m_document.allocator,
					style,
                    true);

				// 这个函数是为Flush服务的函数，所以不应再使用时间戳加大
				this.AppendChildInternal(item,false,true);
			}

		}


		// 命令空间列表
		public ItemList NamespaceList
		{
			get
			{
				ItemList namespaceList = new ItemList();
				
				ElementItem item = this;
				while(item != null)
				{
					for(int i=item.attrs.Count-1;i>=0;i--)
					{
						if (((AttrItem)item.attrs[i]).IsNamespace == true)
							namespaceList.Add(item.attrs[i]);
					}
					item = (ElementItem)item.parent;
				}

				return namespaceList;
			}
		}


		// 属性列表
		// 用于名字空间的属性节点不计算在内
		public ItemList PureAttrList
		{
			get
			{
				ItemList attrList = new ItemList();
				for(int i=0;i<this.attrs.Count;i++)
				{
					AttrItem attr = (AttrItem)this.attrs[i];

					if (attr.IsNamespace == false)
						attrList.Add(attr);
				}
				return attrList;
			}
			
		}


		public string AttrsXml
		{
			get
			{
				//SetFreshValue();
				return GetAttrsXml();
			}
		}



		// 优化xml
		public void YouHua()
		{
			this.YouHuaOneLevel();

			// 目的是把"收缩条"也去掉
			this.InitialVisual();

			int nWidth , nHeight;
			this.Layout(this.Rect.X,
				this.Rect.Y,
				this.Rect.Width,
				0,   //设为0，主要是高度变化
				this.m_document.nTimeStampSeed++,
				out nWidth,
				out nHeight,
				LayoutMember.Layout | LayoutMember.Up);


			//设为当前值
			if (ItemUtil.IsBelong(this.m_document.m_selectedItem,
				this) == true)
			{
				this.m_document.SetCurText(this,null);
				this.m_document.SetActiveItem(this);
			}
			else
			{
				// 没有改变curText，但需要重设，其实如果在上方时可以优化掉
				this.m_document.SetEditPos();
			}

			this.m_document.AfterDocumentChanged(ScrollBarMember.Both);
			this.m_document.Invalidate();

			// 文档发生变化了
			this.m_document.FireTextChanged();

			this.Flush();
			return;

		}


		// 优化本层的名字空间
		public void YouHuaOneLevel()
		{
			NamespaceItemCollection namespaceItems = new NamespaceItemCollection();
	
			ItemList parentNamespaces = null;
			if (this.parent != null)
			{
				parentNamespaces = ((ElementItem)this.parent).NamespaceList;
			}

			if (this.attrs != null
				&& parentNamespaces != null)
			{
				ArrayList aNamespaceAttr = new ArrayList();
				foreach(AttrItem attr in this.attrs)
				{
					if (attr.IsNamespace == true)
					{
						//namespaceItems.Add(attr.Name,attr.Value);
							
						foreach(Item parentNamespace in parentNamespaces)
						{
                            if (parentNamespace.Name == attr.Name
                                && parentNamespace.Value != attr.Value)
                            {
                                break;
                            }

							if (parentNamespace.Name == attr.Name
								&& parentNamespace.Value == attr.Value)
							{
								aNamespaceAttr.Add(attr);
							}
						}
					}
				}

				foreach(AttrItem namespaceAttr in aNamespaceAttr)
				{
					this.RemoveAttrInternal(namespaceAttr,true);   // 要改成Internal
				}
			}

			if (this.children != null)
			{
				foreach(Item item in this.children)
				{
					if (item is ElementItem)
					{
						ElementItem element = (ElementItem)item;
						element.YouHuaOneLevel();
					}
				}
			}
		}


		// 清空儿子，里面会调ItemDeleted事件,且会对ElementItem类型的儿子进行调归
		public void ClearChildren()
		{
			for(int i=0;i<this.children.Count;i++)
			{
				Item child = this.children[i];
				this.RemoveChildInternal(child,false);
			}
			this.m_objChildrenTimestmap ++;
		}


		// 清除属性，里面会调ItemDeleted事件
		public void ClearAttrs()
		{
			foreach(AttrItem attr in this.attrs)
			{
				string strXPath = this.GetXPath();
				////////////////////////////////////////////////
				// ItemDeleted
				///////////////////////////////////////////////
				ItemDeletedEventArgs args = 
					new ItemDeletedEventArgs();
				args.item = attr;
				args.XPath = strXPath;

				// 每次按off算,外面需要时设为on
				args.RiseAttrsEvents = false;
				args.RecursiveChildEvents = false;
				this.m_document.fireItemDeleted(this.m_document,args);
			}

			this.attrs.Clear();
		}

		// 如果祖先是收缩态要对祖先进行Flush()
		public void FlushAncestor()
		{
			ElementItem element = this.parent;
			while(true)
			{
				if (element == null)
					break;
				if (element.m_attrsExpand == ExpandStyle.Collapse
					|| element.m_childrenExpand == ExpandStyle.Collapse)
				{
					element.Flush();
				}
				element = element.parent;
			}
		}



		public void Flush()
		{
			///////////////////////////////////
			//1. 对属性进行Flush
			////////////////////////////////////

			// 内存对象 大于 xml编辑框，则需要把内容对象的内容对现到xml编辑框
			if (this.m_objAttrsTimestamp >= this.m_xmlAttrsTimestamp)
			{
				Text text = this.GetAttributesText();
				if (text != null)
				{
					text.Text = GetAttrsXmlWithoutFlush();
					if (this.m_document.m_curText == text)
						this.m_document.VisualTextToEditControl();
				}
			}
				// 内存对象 小于 xml编辑框,则需要根据xml编辑框的内容重建内存对象
			else if (this.m_objAttrsTimestamp < this.m_xmlAttrsTimestamp)
			{
				// 重建内存结构
				BuildAttrs();
			}

			this.m_xmlAttrsTimestamp = 0;
			this.m_objAttrsTimestamp = 0;



			///////////////////////////////////
			//2. 对儿子进行Flush
			////////////////////////////////////
			
			// 内存对象 大于 xml编辑框，则需要把内容对象的内容对现到xml编辑框
			if (this.m_objChildrenTimestmap >= this.m_xmlChildrenTimestamp)
			{
				Text text = this.GetContentText();
				if (text != null)
				{
					text.Text = this.GetInnerXml(null);
					if (this.m_document.m_curText == text)
						this.m_document.VisualTextToEditControl();
				}
			}
				// 内存对象 小于 xml编辑框,则需要根据xml编辑框的内容重建内存对象
			else if (this.m_objChildrenTimestmap < this.m_xmlChildrenTimestamp)
			{
				// 重建内存结构
				this.BuildDescendant();
			}

			this.m_objChildrenTimestmap  = 0;
			this.m_xmlChildrenTimestamp = 0;


			// 失效
			this.m_document.Invalidate(this.Rect);

			// 影响上级
			this.FlushAncestor();
		}
	

		// 追加子元素节点 
		// 最好判断一下即将插入的元素对象类型
		// 必须是ElementItem 或者 TextItem
		internal void AppendChildInternal(Item item,
			bool bAddObjTimestamp,
			bool bInitial)
		{
			Debug.Assert(item != null,"item参数不能为null");

			Debug.Assert(!(item is AttrItem),"item参数不能为AttrItem类型");
			
			item.parent = this;
			this.children.Add(item);

			// 检查NamespaceURI是否存在
			if (item is ElementItem
				&& bInitial == false)
			{
				ElementItem element = (ElementItem)item;
				string strError;
				int nRet = element.ProcessElementNsURI(bInitial,
					out strError);
				if (nRet == -1)
				{
					this.RemoveChildInternal(item,true);
					throw(new PrefixNotDefineException(strError));
				}
			}

			if (this.m_bConnected == true)
			{
                /*
                if (item is ElementItem)
                    FireItemCreatedTree((ElementItem)item);  // 递归触发事件
                else
                {*/
                    // end 调事件
                    ItemCreatedEventArgs endArgs = new ItemCreatedEventArgs();
                    endArgs.item = item;
                    item.m_bConnected = true;

                    this.m_document.fireItemCreated(this.m_document, endArgs);
                //}

				if (item is ElementItem)
				{
					ElementItem elem = (ElementItem)item;
					elem.SendAttrsCreatedEvent();  // 连带处理属性和儿子
				}
			}

			if (bAddObjTimestamp == true)
				this.m_objChildrenTimestmap ++;
		}

        /*
        static void FireItemCreatedTree(ElementItem element)
        {
            ItemCreatedEventArgs endArgs = new ItemCreatedEventArgs();
            endArgs.item = element;
            element.m_bConnected = true;

            element.m_document.fireItemCreated(element.m_document, endArgs);

        
            // 递归
            for (int i = 0; i < element.children.Count; i++)
            {
                Item item = element.Children[i];
                if (!(item is ElementItem))
                    continue;
                FireItemCreatedTree((ElementItem)item);
            }
        
        }
        */


        // 追加属性节点
        public AttrItem AppendAttrInternal(AttrItem attr,
			bool bAddObjTimestamp,
			bool bInitial)
		{
			// 去掉原同名的属性节点
			AttrItem oldAttr = this.GetAttrItem(attr.Name);
			if (oldAttr != null)
				this.RemoveAttrInternal(oldAttr,false);  //可以和追加节点视作一次

			attr.parent = this;
			this.attrs.Add(attr);

			if (bInitial == false)
			{
				string strError;
				int nRet = this.ProcessAttrNsURI(attr,
					bInitial,
					out strError);
				if (nRet == -1)
				{
					this.RemoveAttrInternal(attr,true);
					throw(new PrefixNotDefineException(strError));
				}
			}

			// 发ItemCreated消息
			if (this.m_bConnected == true)
			{
				// ItemCreated事件
				ItemCreatedEventArgs endArgs = new ItemCreatedEventArgs();
				endArgs.item = attr;
				endArgs.bInitial = false;
				this.m_document.fireItemCreated(this.m_document,endArgs);	

				attr.m_bConnected = true;
			}

			if (bAddObjTimestamp == true)
				this.m_objAttrsTimestamp ++;

			return attr;
		}
		
		// 删儿子 item可以是ElementItem 或 TextItem
		internal void RemoveChildInternal(Item item,
			bool bAddObjTimestamp)
		{
			Debug.Assert(item != null,"RemoveChild() item不能为null,调用出错");

			// 当前节点 不属于儿子集合
			int nIndex = this.children.IndexOf(item);
			if (nIndex == -1)
			{
				Debug.Assert(false,"RemoveChild() item不属于儿子集合，调用出错");
				return;
			}

			////////////////////////////////////////////////
			// BeforeItemDelete
			///////////////////////////////////////////////
			string strXPath = item.GetXPath();  // 先得到Xpath,否则删除后就没有了
			BeforeItemDeleteEventArgs beforeArgs = 
				new BeforeItemDeleteEventArgs();
			beforeArgs.item = item;
			this.m_document.fireBeforeItemDelete(this.m_document,beforeArgs);


			// 把一些有用的初值设好，例如NamespaceURi,Value
			if (item is ElementItem)
			{
				// 注意递归下级
				this.SetNamespaceURI((ElementAttrBase)item);  

				// 递归下级设value???????????
			}
			else
			{
				// 把临时参数设好，目的是在一个元素被删除后，还可以继续使用它的Value属性
				item.m_paraValue1 = item.GetValue();
			}

			if (ItemUtil.IsBelong(item,this.m_document.m_selectedItem))
			{
				this.m_document.SetActiveItem(null);
				this.m_document.SetCurText(null,null);
			}

			// 做Remove()操作
			this.children.Remove(item);

			////////////////////////////////////////////////
			// ItemDeleted
			///////////////////////////////////////////////
			if (item is ElementItem)
			{
				ElementItem element = (ElementItem)item;
				
				element.FireTreeRemoveEvents(strXPath);
			}
			else
			{
				ItemDeletedEventArgs args = 
					new ItemDeletedEventArgs();
				args.item = item;
				args.XPath = strXPath;

				// 每次按off算,外面需要时设为on
				args.RiseAttrsEvents = false;
				args.RecursiveChildEvents = false;
				this.m_document.fireItemDeleted(this.m_document,args);
			}

			if (bAddObjTimestamp == true)
				this.m_objChildrenTimestmap ++;
		}

		
		// 删属性
		internal void RemoveAttrInternal(AttrItem attr,
			bool bAddObjTimestamp)
		{
			Debug.Assert(attr != null,"RemoveAttr() attr不能为null,调用出错");

			// 当前节点 不属于儿子集合
			int nIndex = this.attrs.IndexOf(attr);
			if (nIndex == -1)
			{
				Debug.Assert(false,"RemoveChild() attr不属于属性集合，调入出错");
				return;
			}

			////////////////////////////////////////////////
			// BeforeItemDelete
			///////////////////////////////////////////////
			string strXPath = attr.GetXPath();  // 先得到Xpath,否则删除后就没有了
			BeforeItemDeleteEventArgs beforeArgs = 
				new BeforeItemDeleteEventArgs();
			beforeArgs.item = attr;
			this.m_document.fireBeforeItemDelete(this.m_document,beforeArgs);


			// 把一些有用的初值设好，例如NamespaceURi
			this.SetNamespaceURI((ElementAttrBase)attr);  
			attr.m_paraValue1 = attr.GetValue();


			// 进行Remove操作
			this.attrs.Remove(attr);


			////////////////////////////////////////////////
			// ItemDeleted
			///////////////////////////////////////////////
			ItemDeletedEventArgs args = 
				new ItemDeletedEventArgs();
			args.item = attr;
			args.XPath = strXPath;

			// 每次按off算,外面需要时设为on
			args.RiseAttrsEvents = false;
			args.RecursiveChildEvents = false;
			this.m_document.fireItemDeleted(this.m_document,args);

			if (bAddObjTimestamp == true)
				this.m_objAttrsTimestamp ++;
		}

        // 插入儿子事项（属性或元素）
        // paramters:
        //      referenceItem   插入位置参考元素。将插入到此元素之前
        // Exception:
        //      可能会抛出PrefixNotDefineException异常
        internal void InsertChildInternal(Item referenceItem,
			Item newItem,
			bool bAddObjTimestamp,
			bool bInitial)
		{
			int nIndex = -1;
			nIndex = this.children.IndexOf (referenceItem);
			if (nIndex == -1)
			{
				Debug.Assert (false,"Insert()时，startItem竟然不在children里");
				return;
			}
			this.InsertChildInternal(nIndex,
				newItem,
				bAddObjTimestamp,
				bInitial); 
		}

		// 插入属性或者下级元素(按整数序号)
        // Exception:
        //      可能会抛出PrefixNotDefineException异常
        internal void InsertChildInternal(int nIndex,
			Item newItem,
			bool bAddObjTimestamp,
			bool bInitial)
		{
			newItem.parent = this;
			this.children.Insert(nIndex,newItem);

			// 检查NamespaceURI是否存在
			if (newItem is ElementItem && bInitial == false)
			{
				string strError;
                int nRet = ((ElementItem)newItem).ProcessElementNsURI( // old : this
					bInitial,
					out strError);
				if (nRet == -1)
				{
					this.RemoveChildInternal(newItem,true);
					throw(new PrefixNotDefineException(strError));
				}
			}

			if (this.m_bConnected == true)
			{
                /*
				// end 调事件
                if (newItem is ElementItem)
                    FireItemCreatedTree((ElementItem)newItem);  // 递归触发事件
                else
                {*/

                    ItemCreatedEventArgs args = new ItemCreatedEventArgs();
                    args.item = newItem;
                    //args.bInitial = false;

                    newItem.m_bConnected = true;
                    this.m_document.fireItemCreated(this.m_document, args);
                //}

				if (newItem is ElementItem)
				{
					ElementItem elem = (ElementItem)newItem;
					elem.SendAttrsCreatedEvent();
				}
			}

			if (bAddObjTimestamp == true)
				this.m_objChildrenTimestmap ++;
		}

		// 插入属性或者下级元素(按节点指针)
		internal void InsertAttrInternal(AttrItem startAttr,
			AttrItem newAttr,
			bool bAddObjTimestamp,
			bool bInitial)
		{
			int nIndex = this.attrs.IndexOf (startAttr);
			if (nIndex == -1)
			{
				Debug.Assert (false,"Insert时，startItem竟然不在children里");
				return;
			}
			this.InsertAttrInternal(nIndex,
				newAttr,
				bAddObjTimestamp,
				bInitial);
		}

		// 插入属性或者下级元素(按整数序号)
		internal void InsertAttrInternal(int nIndex,
			AttrItem newAttr,
			bool bAddObjTimestamp,
			bool bInitial)
		{
			newAttr.parent = this;

			this.attrs.Insert(nIndex,newAttr);

			if (bInitial == false)
			{
				string strError;
				int nRet = this.ProcessAttrNsURI(newAttr,
					bInitial,
					out strError);
				if (nRet == -1)
				{
					this.RemoveAttrInternal(newAttr,true);
					throw(new PrefixNotDefineException(strError));
				}
			}
			// 发ItemCreated()
			if (this.m_bConnected == true)
			{
				// ItemCreated事件
				ItemCreatedEventArgs endArgs = new ItemCreatedEventArgs();
				endArgs.item = newAttr;
				//endArgs.bInitial = false;
				this.m_document.fireItemCreated(this.m_document,endArgs);	
				newAttr.m_bConnected = true;
			}
			
			if (bAddObjTimestamp == true)
				this.m_objAttrsTimestamp ++;  
		}


		// 取一个属性节点
		public AttrItem GetAttrItem(string strAttrName)
		{
			if (this.attrs == null)
				return null;

			foreach(AttrItem attr in this.attrs)
			{
				if (attr.Name == strAttrName)
					return attr;
			}
			return null;
		}

		// 得到一个属性节点的值
		public string GetAttrValue(string strAttrName)
		{
			AttrItem attr = this.GetAttrItem(strAttrName);
			if (attr != null)
				return attr.GetValue();

			return "";
		}

		// 设置一个属性节点的值
		// 如果指定的属性不存在，则新创建属性
		public void SetAttrValue(string strAttrName,
			string strNewAttrValue)
		{
			if (strNewAttrValue == null)
				strNewAttrValue = "";

			AttrItem attr = this.GetAttrItem(strAttrName);
			if (attr == null)
			{
				attr = this.m_document.CreateAttrItem(strAttrName);
				attr.SetValue(strNewAttrValue);
				this.AppendAttrInternal(attr,true,false);
			}
			else
			{
				string strOldValue = attr.GetValue();

				// 修改值
				if (strOldValue != strNewAttrValue)
				{
					// 需要改
					attr.SetValue(strNewAttrValue);
					this.m_objAttrsTimestamp ++;

					// 触发事件
					////////////////////////////////////////////////////
					// ItemAttrChanged
					////////////////////////////////////////////////////
					ItemChangedEventArgs args = 
						new ItemChangedEventArgs();
					args.item = attr;
					args.NewValue = strNewAttrValue;
					args.OldValue = strOldValue;
					this.m_document.fireItemChanged(this.m_document,args);
				}
			}

			this.AfterAttrCreateOrChange(attr);
		}


		// 根据命令空间找一个属性
		public virtual string GetAttribute(string strLocalName,
			string strNamespaceURI)
		{
			if (this.attrs == null)
				return "";

			for(int i=0;i<this.attrs.Count;i++)
			{
				AttrItem attr = (AttrItem)this.attrs[i];
				if (attr.Name == strLocalName
					&& attr.NamespaceURI == strNamespaceURI)
				{
					return attr.GetValue();
				}
			}

			return "";
		}

		
		#region 关于优化名字空间的函数

		public enum GetPrefixStyle
		{
			ElementNameUsed = 1,
			AttributesUsed = 2,
			Defined = 4,
			All = 7,
		}

		// 获得element本层用过的或者定义的prefix。之一，或者组合。
		// parameters:
		//		style	如何取
		public Hashtable GetPrefix(GetPrefixStyle style)
		{
			Hashtable aPrefix = new Hashtable();
			
			// 元素名的前缀
			if ((style & GetPrefixStyle.ElementNameUsed) == GetPrefixStyle.ElementNameUsed) 
			{
				aPrefix.Add(this.Prefix, null);
			}

			for(int i=0; i<this.attrs.Count; i++)
			{
				AttrItem attr = (AttrItem)this.attrs[i];

				bool bProcess = false;
				string strPrefix = "";

				if ((style & GetPrefixStyle.Defined) == GetPrefixStyle.Defined) 
				{
					if (attr.IsNamespace == true) 
					{
						strPrefix = attr.LocalName;
						bProcess = true;
					}
				}
				if (bProcess == false
					&& (style & GetPrefixStyle.AttributesUsed) == GetPrefixStyle.AttributesUsed) 
				{
					if (attr.IsNamespace == false) 
					{
						strPrefix = attr.Prefix;
						bProcess = true;
					}
				}

				if (bProcess == false)
					continue;

				if (aPrefix.Contains(strPrefix) == false)
					aPrefix.Add(strPrefix, null);

			}


			return aPrefix;
		}

		// 获得element本层用过的prefix
		// parameters:
		//		bRemoveDefined	是否移走本层已经定义的前缀
		public Hashtable GetUsedPrefix(bool bRemoveDefined)
		{
			Hashtable aDefinedPrefix = null;
			
			if (bRemoveDefined == true) 
			{
				aDefinedPrefix = new Hashtable();

				for(int j=0; j<this.attrs.Count; j++)
				{
					AttrItem attr = (AttrItem)this.attrs[j];

					Debug.Assert(attr.LocalName != null, "");

					if (attr.IsNamespace == false)
						continue;

/*
					if (attr.LocalName == "xml")
						continue;
*/
					if (aDefinedPrefix.Contains(attr.LocalName) == false)
						aDefinedPrefix.Add(attr.LocalName, null);
					else 
					{
						Debug.Assert(false, "在一个元素层次中，不可能出现重复定义的名字空间前缀");
					}

				}

				if (aDefinedPrefix.Count == 0)
					aDefinedPrefix = null;
			}

			Hashtable aPrefix = new Hashtable();


			// 元素名的前缀
			if (aDefinedPrefix != null) // 看看是否属于本层次已定义的
			{
				if (aDefinedPrefix.Contains(this.Prefix) == false)
					aPrefix.Add(this.Prefix, null);
			}
			else 
			{
				aPrefix.Add(this.Prefix, null);
			}

			for(int i=0; i<this.attrs.Count; i++)
			{
				AttrItem attr = (AttrItem)this.attrs[i];

				if (attr.IsNamespace == true)
					continue;


				if (attr.Prefix == "") 
					continue;

				// 看看是否属于本层次已定义的
				if (aDefinedPrefix != null)
				{
					if (aDefinedPrefix.Contains(attr.Prefix) == true)
						continue;
				}			

				if (aPrefix.Contains(attr.Prefix) == false)
					aPrefix.Add(attr.Prefix, null);
			}


			/*
			ArrayList aResult = new ArrayList();

			aResult.AddRange(aPrefix.Keys);
			*/

			return aPrefix;
		}


		#endregion


		public override string GetValue()
		{
			Debug.Assert(false,"本对象不能使用该函数");
			return null;
		}

		public override void SetValue(string strValue)
		{
			Debug.Assert(false,"本对象不能使用该函数");
		}

		internal override Text GetVisualText()
		{
			Debug.Assert(false,"本对象不能使用该函数");
			return null;
		}


		#region InnerXml 与 OuterXml


		public void SetText(string strValue)
		{
			this.ClearChildren();

			TextItem text = this.m_document.CreateTextItem();
			text.SetValue(strValue);

			this.AppendChild(text);
		}


		public string GetText()
		{
			return this.GetInnerText(false);
		}


		// 获得所有的文本节点的值
		// parameter:
		//		bRecursion	是否对下级进行递归
		public string GetInnerText(bool bRecursion)
		{
			string strInnerText = "";
			foreach(Item item in this.children)
			{
				if (item is ElementItem)
				{
					if (bRecursion == true)
						strInnerText += ((ElementItem)item).GetInnerText(bRecursion);
				}
				if (item is TextItem)
					strInnerText += item.Value;
			}
			return strInnerText;
		}



		// 得到InnerXml属性
		// parameters:
		//		FragmentRoot	是否加入外部名字空间信息，如果要加入，片段根元素是什么。
		//				如果==null，所有的节点都不带额外的名称空间信息，如果!=null，所有层如果必要都可能会带上额外的名字空间信息
		public string GetInnerXml(ElementItem FragmentRoot)
		{
			string strContent = "";
			// 通过递归儿子获得strContent
			for(int i=0; i<this.children.Count;i++)
			{
				Item child = (Item)this.children[i];

				if (child is ElementAttrBase) 
				{
					strContent += child.GetOuterXml(FragmentRoot != null ? (ElementItem)child : null);
				}
				else 
				{
					strContent += child.GetOuterXml(null);
				}
			}
			return strContent;
		}

		internal string GetAttrsXmlWithoutFlush()
		{
			string strAttrXml = "";

			// 内存对象 大于 xml编辑框
			if (this.m_objAttrsTimestamp >= this.m_xmlAttrsTimestamp)
			{
				for(int i=0; i<this.attrs.Count;i++)
				{
					AttrItem attr = (AttrItem)this.attrs[i];

					strAttrXml += " " + attr.GetOuterXml(null);
				}

				return strAttrXml;
			}
			else
			{
				// 2.得到attributes关闭时的text
				Text oText = this.GetAttributesText();
				if (oText != null) 
				{
					strAttrXml = oText.Text.Trim();

					if (strAttrXml != "")
						strAttrXml = " " + strAttrXml;

					return strAttrXml;
				}
				else 
				{
					Debug.Assert(false, "必须有text对象");
					return "";
				}
			}
		}

		// 获得本对象所包含的属性XML字符串
		// 如果属性不存在，返回""；如果存在内容，有前空而没有后空
		internal string GetAttrsXml()
		{
			string strAttrXml = "";

			for(int i=0; i<this.attrs.Count;i++)
			{
				AttrItem attr = (AttrItem)this.attrs[i];
				strAttrXml += " " + attr.GetOuterXml(null);
			}
			return strAttrXml;
		}

		// 无论是展开还是收缩状态都能获得额外名字空间的GetOuterXml()特殊版本，适用于展开收缩的情况
		// 注意：所产生的字符串多了一个根，不能随便用
		public string GetOuterXmlSpecial()
		{
			Debug.Assert (this != this.m_document.VirtualRoot,
				"不要这样调用,应分情况调用虚根的GetInnerXml()或者GetOuterXml()");

			string strAdditionalNsString = "";
			string strXml = this.GetOuterXml(null);

			NamespaceItemCollection nsColl = NamespaceItemCollection.GatherOuterNamespaces(
				(ElementItem)this.parent);

			if (nsColl.Count > 0)
			{
				strAdditionalNsString = nsColl.MakeAttrString();
			}

			return "<root "+ strAdditionalNsString + " >\r\n" + strXml + "</root>";
		}


		// parameters:
		//		FragmentRoot	如果要加入额外名字空间的话，片段的顶部element对象。
		//			如果==null，表示不必加入额外的名字空间信息
		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			int i;
			string strOuterXml = "";

			string strContent = "";
			string strAttrXml = "";

			// 通过递归儿子获得strContent
			for(i=0; i<this.children.Count;i++)
			{
				Item child = (Item)this.children[i];
				strContent += child.GetOuterXml(FragmentRoot);
			}

			if (this == this.m_document.VirtualRoot) 
			{
				return strContent;
			}

			string strAdditional = "";
			if (FragmentRoot != null)  //需要加额外的名字空间
			{
				// 本层需要加入的额外名字空间属性
				ArrayList aPrefix = null;
				ItemUtil.GetUndefinedPrefix(this,
					FragmentRoot,
					out aPrefix);

				for(i=0;i<aPrefix.Count;i++)
				{
					string strPrefix = (string)aPrefix[i];
					if (strPrefix == "xml")
						continue;

					string strURI = "";
					AttrItem foundAttr = null;

					bool bRet = ItemUtil.LocateNamespaceByPrefix(this,	// 可以优化为FragmentRoot的父亲
						strPrefix,
						out strURI,
						out foundAttr);
					if (bRet == false)
					{
						if (strPrefix != "")
						{
							throw(new Exception("前缀" +strPrefix+ "没有找到定义位置"));
						}
						else
							continue;
					}

					if (strPrefix != "")
						strAdditional += " xmlns:" + strPrefix + "='" + StringUtil.GetXmlStringSimple(strURI) + "'";
					else
						strAdditional += " xmlns='" + StringUtil.GetXmlStringSimple(strURI) + "'";

				}
			}

			// 似乎可以优化，改用GetAttrsXml()?
			for(i=0; i<this.attrs.Count;i++)
			{
				AttrItem attr = (AttrItem)this.attrs[i];

				strAttrXml += " " + attr.GetOuterXml(FragmentRoot);
			}

			if (strAdditional != "")
				strAttrXml += strAdditional;


			//if (strAttrXml != "")
			//	strAttrXml += " ";

			Debug.Assert(this.Name != "", "ElementItem的Name不应为空");

			Debug.Assert(this != this.m_document.VirtualRoot, "前面已经处理了,不可能走到这里");	// 前面已经处理了,不可能走到这里

			strOuterXml = "<" + this.Name + strAttrXml + ">" + strContent + "</" + this.Name + ">";
			return strOuterXml;
		}


		// 得到InnerXml属性
		// 内部不要调此函数
		public string InnerXml
		{
			get
			{
				return this.GetInnerXml(this);
			}
		}

		// 内部绝对不能调此属性,因为会有多余的名字空间信息
		public override string OuterXml 
		{
			get
			{
				return this.GetOuterXml(this);
			}
			set 
			{
				string strError = "";
				int nRet = this.m_document.PasteOverwrite(value,
					this,
					false,
					out strError);
				if (nRet == -1)
				{
					throw(new Exception(strError));
				}
			}
		}


		#endregion


		#region 收缩展开部分

		// expandAttrs	    属性状态
		// expandChildren	儿子状态
		public void ExpandAttrsOrChildren(ExpandStyle expandAttrs,
			ExpandStyle expandChildren,
			bool bChangeDisplay)
		{
			bool bOldChanged = this.m_document.m_bChanged;

			//设光标为等待状态
			Cursor cursorSave = this.m_document.Cursor;
			if (bChangeDisplay == true) 
			{
				this.m_document.Cursor = Cursors.WaitCursor;
			}

			ElementInitialStyle style = new ElementInitialStyle();
			style.childrenExpandStyle = expandChildren;
			style.attrsExpandStyle = expandAttrs;
			style.bReinitial = true;

			string strXml = "";
			if (this == this.m_document.VirtualRoot)
			{
				strXml = this.GetOuterXml(null);
			}
			else
			{
				strXml = this.GetOuterXmlSpecial();
			}


			XmlDocument dom = new XmlDocument ();
			try
			{
				dom.LoadXml(strXml);
			}
			catch(Exception ex)
			{
				throw(new Exception("ExpandChild()内部错误：" + ex.Message));
			}
			
			if (this == this.m_document.VirtualRoot)
			{
				// 初始化item层次，注意使用根下的第一个元素
				this.Initial(dom,
					this.m_document.allocator,
					style,
                    true);
			}
			else 
			{
				Debug.Assert(dom.DocumentElement.ChildNodes.Count == 1, "特殊的xml字符串doc根必须有且只有一个儿子");

				// 初始化item层次，注意使用根下的第一个元素
				this.Initial(dom.DocumentElement.ChildNodes[0],
					this.m_document.allocator,
					style,
                    true);
			}


			// 重新初始化Visual层次
			this.InitialVisual();


			// 5.item重新layout,这里还用原来的rect,意思是item尺寸不变，主要修改里面的尺寸
			int nRetWidth,nRetHeight;
			this.Layout (this.Rect.X,
				this.Rect.Y,
				this.Rect.Width,
				0,
				this.m_document.nTimeStampSeed,
				out nRetWidth,
				out nRetHeight,
				LayoutMember.Layout);

			this.Layout(this.Rect.X,
				this.Rect.Y,
				this.Rect.Width,  
				this.Rect.Height,
				this.m_document.nTimeStampSeed ++,
				out nRetWidth,
				out nRetHeight,
				LayoutMember.EnLargeHeight | LayoutMember.Up  );


			this.m_document.nTimeStampSeed++;


			/*
						if (this.SelectedItem != null) 
						{
							// 如果CurItem1是element的属性之一
							if (this.SelectedItem is AttrItem
								&& this.SelectedItem.parent == element)
							{
								if (expandAttrs == ExpandStyle.Collapse)
								{
									this.SelectedItem = element;
								}
							}
							else
							{
								if (ItemUtil.IsBelong(this.SelectedItem,
									element))
								{
									this.SelectedItem = element;
								}
							}
						}
			*/			
			if (bChangeDisplay == true) 
			{
				//layout后，文档尺寸发生变化，所以调此函数
				this.m_document.AfterDocumentChanged(ScrollBarMember.Both);

				// 这儿为什么要这样做呢，我也不太清楚
				this.m_document.DocumentOrgX = this.m_document.DocumentOrgX;
				this.m_document.DocumentOrgY = this.m_document.DocumentOrgY;
				
				//改回原来光标的状态
				this.m_document.Cursor = cursorSave;

				this.m_document.m_bChanged = bOldChanged;

				this.m_document.Invalidate();	
			}

			//this.SelectedItem = element;

			this.m_childrenExpand = expandChildren;
			this.m_attrsExpand = expandAttrs;

			this.Flush();
		}

		#endregion

		#region 属性部分
		

		// parameter:
		//		element 被处理的属性节点
		// return:
		//		-1	error
		//		0	successed
		internal int ProcessAttrNsURI(AttrItem attr,
			bool bInitial,
			out string strError)
		{
			strError = "";

			// 不带名字名间 或缺省名字空间
			if (attr.Prefix == null
				|| attr.Prefix == "")
			{
				return 0;
			}

			string strSaveURI = attr.NamespaceURI;

			attr.m_strTempURI = null;

			string strUpDefineURI = attr.NamespaceURI;
			if (strUpDefineURI != null)
				return 0;

			if (bInitial == true)
				return 0;

			attr.m_strTempURI = strSaveURI;

			// 说明已经定义好了，不是临时变量
			if (attr.m_strTempURI == null)
			{
				strError = "未找到前缀'" + attr.Prefix + "'对应的URI";
				return 0;
			}

			AttrItem attrNs = this.m_document.CreateAttrItem("xmlns:" + attr.Prefix);
			attrNs.SetValue(attr.m_strTempURI);
			attrNs.IsNamespace = true;
			attr.parent.AppendAttrInternal(attrNs,true,false);

			// 把参数置空
			attr.m_strTempURI = null;
			return 0;
		}

		
		// 功能: 给一个元素向前插入一个同级元素或者属性
		// parameter:
		//		newItem     要插入的新Item
		//		refChild    参考位置的元素
		//		strError    out参数，返回出错信息
		// return:
        //      -1  出错
        //      0   成功
		public int InsertAttr(AttrItem startAttr,
			AttrItem newAttr, 
			out string strError)
		{
			Cursor oldCursor = this.m_document.Cursor;
			this.m_document.Cursor =  Cursors.WaitCursor;
			try
			{
				strError = "";
				if (startAttr == null)
				{
					Debug.Assert(false,"InsertAttr()时，传入的startAttr为null");
					strError = "InsertAttr()时，传入的startAttr为null";
					return -1;
				}
				if (newAttr == null)
				{
					Debug.Assert(false,"InsertAttr()时，传入的newAttr为null");
					strError = "InsertAttr()时，传入的newAttr为null";
					return -1;
				}


				// 1.调InsertAttr()函数，把父亲关系建好
				this.InsertAttrInternal(startAttr,
					newAttr,
					true,
					false);




				this.AfterAttrCreateOrChange(newAttr);

				return 0;
			}
			finally
			{
				this.m_document.Cursor = oldCursor;
			}
		}


		// 改变属性:上级增加了属性，或者修改的属性值调此函数，做残余的事情
		public void AfterAttrCreateOrChange(AttrItem attr)
		{
			Cursor oldCursor = this.m_document.Cursor;
			this.m_document.Cursor =  Cursors.WaitCursor;
			try
			{
				// 设父亲状态
				this.m_bWantAttrsInitial = 1;
				if (this.attrs.Count > 0
					&& this.AttrsExpand == ExpandStyle.None)
				{
					this.AttrsExpand = ExpandStyle.Expand;
				}
			
				// 重新初始化Attributes区域
				this.AttributesReInitial();



				int nWidth, nHeight;
				this.Layout(this.Rect.X,
					this.Rect.Y,
					this.Rect.Width,
					0,
					this.m_document.nTimeStampSeed++,
					out nWidth,
					out nHeight,
					LayoutMember.Layout | LayoutMember.Up );

				// 把新插入的属性变为当前活动的对象
				// this.m_document.SetActiveItem(attr);
				// this.m_document.SetCurText(attr);

				// 设卷滚条
				this.m_document.AfterDocumentChanged(ScrollBarMember.Both);
				this.m_document.Invalidate();  //??多大范围
			
				// 文档发生变化了
				this.m_document.FireTextChanged();

				this.Flush();
			}
			finally
			{
				this.m_document.Cursor = oldCursor;
			}
		}

		

		// 追加属性
		public int AppendAttr(AttrItem attr,
			out string strError)
		{
			Cursor oldCursor = this.m_document.Cursor;
			this.m_document.Cursor =  Cursors.WaitCursor;
			try
			{
				strError = "";
				this.AppendAttrInternal(attr,true,false);

				this.AfterAttrCreateOrChange(attr);
				return 0;
			}
			finally
			{
				this.m_document.Cursor = oldCursor;
			}
		}

		#endregion

		#region 儿子部分

		
		// parameter:
		//		bInitial	是否处理Initial阶段
		// return:
		//		-1	出错
		//		0	成功
		internal int ProcessElementNsURI(bool bInitial,
			out string strError)
		{
			strError = "";

			// 不带名字名间 或缺省名字空间
			if (this.Prefix == null
				|| this.Prefix == "")
			{
				return 0;
			}

			string strSaveURI = this.NamespaceURI;
			
			this.m_strTempURI = null;

			string strUpDefineURI = this.NamespaceURI;
			// 说明上级已经存在这个prefix对应的URI，不用创建名字空间属性节点了
			if (strUpDefineURI != null)
				return 0;

			if (bInitial == true)
				return 0;

			this.m_strTempURI = strSaveURI;
			// 说明已经定义好了，不是临时变量
			if (this.m_strTempURI == null)
			{
				strError = "没有指定'" + this.Prefix + "'前缀对应的URI";
				return -1;
			}


			AttrItem attrNs = this.m_document.CreateAttrItem("xmlns:" + this.Prefix);
			attrNs.SetValue(this.m_strTempURI);
			attrNs.IsNamespace = true;

			this.AppendAttrInternal(attrNs,true,false);

			// m_strTempURI参数置null
			this.m_strTempURI = null;

			return 0;
		}


		// 自动处理了PrefixNotDefineException异常的AppendChild版本
		public void AutoAppendChild(Item newChildItem)
		{
			REDO:
				try 
				{
					this.AppendChild(newChildItem);
				}
				catch (PrefixNotDefineException ex)
				{
					AddNsDefineDlg dlg = new AddNsDefineDlg();

					if (!(newChildItem is ElementItem)) 
					{
						Debug.Assert(false, "");
						throw ex;
					}

					dlg.textBox_prefix.Text = ((ElementItem)newChildItem).Prefix;
					dlg.label_message.Text = "在插入元素 '" + newChildItem.Name + "' 过程中，发现前缀 '"
						+ dlg.textBox_prefix.Text + "' 未定义。请给出此前缀的定义。";
					dlg.ShowDialog(this.m_document);

					if (dlg.DialogResult != DialogResult.OK) 
					{
						MessageBox.Show(this.m_document, "放弃插入元素");
						return;
					}

					// 此处将来也可以开发在父亲或祖先元素中增加名字空间定义的可能。

					((ElementItem)newChildItem).Prefix = dlg.textBox_prefix.Text;
					((ElementItem)newChildItem).m_strTempURI = dlg.textBox_uri.Text;
					goto REDO;
				}

			// 其他异常还会继续抛出
		}

		// 追加下级
		public void AppendChild(Item newChildItem)
		{
			Cursor oldCursor = this.m_document.Cursor;
			this.m_document.Cursor =  Cursors.WaitCursor;
			try
			{
				if (newChildItem == null)
				{
					Debug.Assert(false,"newChildItem 不能为 null");
					throw new Exception("newChildItem 不能为 null");
				}


				// 使内存对象时间戳加大
				this.AppendChildInternal(newChildItem,true,false);




				// 设父亲的儿子区域状态
				this.m_bWantChildInitial = 1;
				if (this.children.Count > 0
					&& this.ChildrenExpand == ExpandStyle.None)
				{
					this.m_childrenExpand = ExpandStyle.Expand;
				}


				// 父亲对Content部分重做IntialVisual()
				this.ContentReInitial();

				int nWidth , nHeight;
				this.Layout(this.Rect.X,
					this.Rect.Y,
					this.Rect.Width,
					0,
					this.m_document.nTimeStampSeed++,
					out nWidth,
					out nHeight,
					LayoutMember.Layout | LayoutMember.Up);
		
				this.m_document.AfterDocumentChanged(ScrollBarMember.Both );
				this.m_document.Invalidate();

				// 文档发生变化
				this.m_document.FireTextChanged();

				this.Flush();
			}
			finally
			{
				this.m_document.Cursor = oldCursor;
			}
		}


		public void SendAttrsCreatedEvent()
		{
			foreach(AttrItem attr in this.attrs)
			{
				if (attr.m_bConnected == true)
					continue;

				ItemCreatedEventArgs endArgs = new ItemCreatedEventArgs();
				endArgs.item = attr;
				endArgs.bInitial = false;
				this.m_document.fireItemCreated(this.m_document,endArgs);

				attr.m_bConnected = true;
			}
		}

		// 给一个元素追加一个下级元素节点
		// parameter:
		//		newChildItem	必须是ElementItem 或者 TextItem
		//		bCallBeforeDelegate 是否调BeforeDelegate
		// return:
        //      -1  出错
        //      -2  放弃
        //      0   成功
		public int AppendChild(Item newChildItem,
			out string strError)
		{
			strError = "";
			try
			{
				this.AppendChild(newChildItem);
			}
			catch(Exception ex)
			{
				strError = ex.Message;
				return -1;
			}
			return 0;
		}



		// 功能: 给一个元素向前插入一个同级元素
		// parameter:
        //		referenceChild    参考位置的元素
		//		newItem     要插入的新Item
		//		strError    out参数，返回出错信息
		// return:
        //      -1  出错
		//		0   成功
        // Exception:
        //      可能会抛出PrefixNotDefineException异常
		public int InsertChild(Item referenceChild,
			Item newChild, //任何儿子类型
			out string strError)
		{
			Cursor oldCursor = this.m_document.Cursor;
			this.m_document.Cursor =  Cursors.WaitCursor;
			try
			{
				strError = "";
				if (referenceChild == null)
				{
					Debug.Assert(false,"InsertChild()时，传入的startChild为null");
					strError = "InsertChild()时，传入的startChild为null";
					return -1;
				}
				if (newChild == null)
				{
					Debug.Assert(false,"InsertChild()时，传入的newChild为null");
					strError = "InsertChild()时，传入的newChild为null";
					return -1;
				}

				// 是虚根节点没法再插入同级
				if (referenceChild == this.m_document.VirtualRoot)
				{
					strError = "虚根元素不能插入同级元素";
					return -1;
				}

				// 是根节点没法再插入同级
				if (referenceChild == this.m_document.docRoot)
				{
					strError = "根元素不能插入同级元素";
					return -1;
				}


				// 1.调InsertAttr()函数，把父亲关系建好
				// 使内存对象加大

                // !!! 这里可能会抛出PrefixNotDefineException异常
                    this.InsertChildInternal(referenceChild,
                        newChild,
                        true,
                        false);

				// 插入同级，必然是展开的状态，否则选中不了当前节点，所以不用设父亲的状态了



				// 2.父亲重建Visual关系
				this.ContentReInitial();

				// 4.调Layout()
				int nWidth,nHeight;
				this.Layout(this.Rect.X,
					this.Rect.Y,
					this.Rect.Width,  //插入节点，不改变宽度
					0,
					this.m_document.nTimeStampSeed++,
					out nWidth,
					out nHeight,
					LayoutMember.Layout  | LayoutMember.Up); //影响上级

				this.m_document.AfterDocumentChanged (ScrollBarMember.Both);
				this.m_document.Invalidate ();

				// 文档发生改变
				this.m_document.FireTextChanged();

				this.Flush();
				return 0;
			}
			finally
			{
				this.m_document.Cursor = oldCursor;
			}
		}

		#endregion

		#region 删除前缀

		// 根据前缀检索一个名字空间是否可以删除
		public bool CoundDeleteNs(string strPrefix)
		{
			bool bDefinded = this.CheckDefindedByAncestor(strPrefix);
			if (bDefinded == true)
				return true;

			bool bUsing = this.CheckUseNs(strPrefix);
			if (bUsing == false)
				return true;

			return false;

		}

		// 检查一个前缀是否在上级做了定义
		public bool CheckDefindedByAncestor(string strPrefix)
		{
			ElementItem curElement = this.parent;
			while(true)
			{
				if (curElement == null)
					break;

				foreach(AttrItem attr in curElement.attrs)
				{
					if (attr.LocalName == strPrefix)
						return true;
				}
				curElement = curElement.parent;
			}
			return false;

		}

		// 检索自己及属性及下级使用使用这个前缀
		public bool CheckUseNs(string strPrefix)
		{
			if (this.Prefix == strPrefix)
				return true;

			foreach(AttrItem attr in this.attrs)
			{
				if (attr.Prefix == strPrefix)
					return true;
			}
			foreach(Item child in this.children)
			{
				if (!(child is ElementItem))
					continue;
				
				bool bUser = ((ElementItem)child).CheckUseNs(strPrefix);
				if (bUser == true)
					return true;
			}
			return false;
		}

		#endregion

		#region 删除一个节点
		
		// parameter:
		//		item	要删除的节点
		//		bForceDelete	当为名字空间，且正在被用时，是否强行删除节点
		// return
		//		false	未删除
		//		true	删除了
		public bool Remove(Item item,
			bool bForceDelete)
		{

			if (!(item is AttrItem))
				return this.Remove(item);

			AttrItem attr = (AttrItem)item;
			if (attr.IsNamespace == false)
				return this.Remove(item);

			bool bCoundDelete = this.CoundDeleteNs(attr.LocalName);
			if (bCoundDelete == true)
				return this.Remove(item);

			if (bForceDelete == true)
				return this.Remove(item);

			return false;

		}

		// 删除一个下级节点
		internal bool RemoveChild(Item item)
		{
			if (item == null)
			{
				Debug.Assert(false,"RemoveChild(),item参数不能为null");
				return false;
			}
			if (item is AttrItem)
			{
				Debug.Assert(false,"此处item不能为AttrItem类型");
				return false;
			}
			// 此时item不可能是虚根，调用错误
			if (item == this.m_document.VirtualRoot)  ///???????????
			{
				Debug.Assert(false,"此时item不可能是虚根，调用错误");
				return false;
			}


			// 当前活动节点是不是包含在被删除的节点内，
			// 如果是，则从item附近找到一个相领的节点。最后把活动节点 , curText , edit设正确
			bool bBeLong = false;
			Item hereAboutItem = null;
			if (ItemUtil.IsBelong(this.m_document.m_selectedItem,
				item) == true)
			{
				hereAboutItem = ItemUtil.GetNearItem(item,
					MoveMember.Auto);
				bBeLong = true;
			}


			// 内存对象到位
			this.RemoveChildInternal(item,true);

			// 视图对象到位
			if (this.children.Count == 0)
			{
				// 目的是把"收缩条"也去掉
				this.InitialVisual();
			}
			else
			{
				this.ContentReInitial();
			}

			int nWidth , nHeight;
			this.Layout(this.Rect.X,
				this.Rect.Y,
				this.Rect.Width,
				0,   //设为0，主要是高度变化
				this.m_document.nTimeStampSeed++,
				out nWidth,
				out nHeight,
				LayoutMember.Layout | LayoutMember.Up);


			//设为当前值
			if (bBeLong == true)
			{
				this.m_document.SetCurText(hereAboutItem,null);
				this.m_document.SetActiveItem(hereAboutItem);
			}
			else
			{
				// 没有改变curText，但需要重设，其实如果在上方时可以优化掉
				this.m_document.SetEditPos();
			}

			this.m_document.AfterDocumentChanged(ScrollBarMember.Both);
			this.m_document.Invalidate();

			// 文档发生变化
			this.m_document.FireTextChanged();

			this.Flush();
			return true;
		}

		internal bool RemoveAttr(AttrItem attr)
		{
			if (attr == null)
			{
				Debug.Assert(false,"RemoveAttr() attr参数不能为null");
				return false;
			}
			if (this == this.m_document.VirtualRoot)
			{
				Debug.Assert(false,"this不能为虚根");
				return false;
			}
	
			// 当前活动节点是不是就是要删除的节点
			// 如果是，则从attr附近找到一个相领的节点。最后把活动节点 , curText , edit设正确
			bool bBeLong = false;
			Item hereAboutItem = null;
			if (this.m_document.m_selectedItem == attr)
			{
				hereAboutItem = ItemUtil.GetNearItem(attr,MoveMember.Auto);
				bBeLong = true;
			}

				
			this.RemoveAttrInternal(attr,true);


			if (this.attrs.Count == 0)
			{
				// 目的是为了把"收缩条"也去掉
				this.InitialVisual();
			}
			else
			{
				this.AttributesReInitial();
			}

			int nWidth , nHeight;
			this.Layout(this.Rect.X,
				this.Rect.Y,
				this.Rect.Width,
				0,   //设为0，主要是高度变化
				this.m_document.nTimeStampSeed++,
				out nWidth,
				out nHeight,
				LayoutMember.Layout | LayoutMember.Up );


			//设为当前值
			if (bBeLong == true)
			{
				this.m_document.SetCurText(hereAboutItem,null);
				this.m_document.SetActiveItem(hereAboutItem);
			}
			else
			{
				this.m_document.SetEditPos ();
			}


			this.m_document.AfterDocumentChanged(ScrollBarMember.Both);
			this.m_document.Invalidate();

			// 文档发生变化
			this.m_document.FireTextChanged();

			this.Flush();

			return true;
		}


		// 删除一个指定的元素 可以是 ElemtnItem ,AttrItem ,TextItem
		public bool Remove(Item item)
		{
			Cursor oldCursor = this.m_document.Cursor;
			this.m_document.Cursor =  Cursors.WaitCursor;
			try
			{
				if (item == null)
				{
					Debug.Assert(false,"Remove() item参数不能为null");
					return false;
				}

				if (item is AttrItem)
					return this.RemoveAttr((AttrItem)item);
				else
					return this.RemoveChild(item);
			}
			finally
			{
				this.m_document.Cursor = oldCursor;
			}
		}


		// 因为NamespaceURI是通过是一个属性，是通过上级得到的，
		// 本函数的目的是在删除item时，把它的名字空间的临用参数设好，以便在脱关系时还可以继续使用
		internal void SetNamespaceURI(ElementAttrBase item)
		{
			item.m_strTempURI = item.NamespaceURI;

			if (item is ElementItem)
			{
				ElementItem element = (ElementItem)item;
				
				foreach(AttrItem attr in element.attrs)
				{
					this.SetNamespaceURI(attr);
				}

				foreach(Item child in element.children)
				{
					if (!(child is ElementItem))
						continue;
					this.SetNamespaceURI((ElementItem)child);
				}
			}
		}


		// 应该在删除前把事件发了
		public void FireTreeRemoveEvents(string strXPath)
		{
			// 发自己的ItemDeleted事件

			ItemDeletedEventArgs args = 
				new ItemDeletedEventArgs();
			args.item = this;
			args.XPath = strXPath;

			// 每次按off算,外面需要时设为on
			//args.RiseAttrsEvents = false;
			//args.RecursiveChildEvents = false;
			args.RiseAttrsEvents = true;
			args.RecursiveChildEvents = true;
			this.m_document.fireItemDeleted(this.m_document,args);


			if (args.RiseAttrsEvents == true)
			{
				for(int i=0;i<this.attrs.Count;i++)
				{
					AttrItem attr = (AttrItem)this.attrs[i];

					ItemDeletedEventArgs argsAttr = 
						new ItemDeletedEventArgs();
					argsAttr.item = attr;
					argsAttr.XPath = strXPath + "/@" + attr.Name + "";

					// 每次按off算,外面需要时设为on
					//argsAttr.RiseAttrsEvents = false;
					//argsAttr.RecursiveChildEvents = false;
					
					argsAttr.RiseAttrsEvents = true;
					argsAttr.RecursiveChildEvents = true;

					this.m_document.fireItemDeleted(this.m_document,argsAttr);
				}
			}
			if (args.RecursiveChildEvents == true)
			{
				for(int i=0;i<this.children.Count;i++)
				{
					Item child = this.children[i];
					if (child is ElementItem)
					{
						ElementItem element = (ElementItem)this.children[i];

						string strPartXpath = ItemUtil.GetPartXpath(this,
							element);

						string strChildXPath = strXPath + "/" + strPartXpath;

						element.FireTreeRemoveEvents(strChildXPath);

					}
					else
					{
						ItemDeletedEventArgs argsChild = 
							new ItemDeletedEventArgs();
						argsChild.item = child;
						argsChild.XPath = strXPath;

						// 每次按off算,外面需要时设为on
						//argsChild.RiseAttrsEvents = false;
						//argsChild.RecursiveChildEvents = false;

						
						argsChild.RiseAttrsEvents = true;
						argsChild.RecursiveChildEvents = true;
						this.m_document.fireItemDeleted(this.m_document,argsChild);
					}

				}

			}



		}
		#endregion

	}

    public class PrefixNotDefineException : Exception
    {
        public PrefixNotDefineException(string strMessage)
            : base(strMessage)
        { }
    }

}
