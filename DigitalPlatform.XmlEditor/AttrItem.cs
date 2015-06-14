
using System;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Text;

namespace DigitalPlatform.Xml
{
	// 属性节点
	public class AttrItem : ElementAttrBase
	{
		internal AttrItem(XmlEditor document)
		{
			this.m_document = document;
		}


		// 初始化数据，从node中获取
		public virtual void InitialData(XmlNode node)
		{
			this.Name = node.Name;
			this.SetValue(node.Value);

			// Prefix和	NamespaceURI 两个成员在稍后初始化
			Prefix = null;
			this.m_strTempURI = node.NamespaceURI;
		}

		// parameters:
		//		style	初始化风格。暂未使用。
		public override void InitialVisualSpecial(Box boxTotal)
		{
			XmlText text = new XmlText ();
			text.Name = "TextOfAttrItem";
			text.container = boxTotal;
			Debug.Assert(this.m_paraValue1 != null,"m_paraValue是用来传递参数，不能为null");
			text.Text = this.GetValue();//this.m_paraValue;
			boxTotal.AddChildVisual(text);

			// 是否可以用SetValue()
			this.m_paraValue1 = null;

			if (this.IsNamespace == true)
				text.Editable = false;

			this.m_strTempURI = null;
		}




		// 用node初始化本对象和下级
		// parameters:
		//		style	暂不使用
		// return:
		//		-1  出错
		//		-2  中途cancel
		//		0   成功
		public override int Initial(XmlNode node, 
			ItemAllocator allocator,
			object style,
            bool bConnect)
		{
			// 初始化数据，从node中获取
			this.InitialData(node);

			// 搞清楚是不是namespace类型节点
			string strName = node.Name;
			if (strName.Length >= 5)
			{
				if (strName.Substring(0,5) == "xmlns")	// 大小写敏感
				{
					if (strName.Length == 5)	// 特殊属性，定义了无前缀字符串的名字空间
					{
						this.IsNamespace = true;
						this.Prefix = "xmlns";
						this.LocalName = "";

						// 如果要和.net dom兼容的话
						//this.Prefix = "";	// 本应是"xmlns"，但是.net的dom为"";
						//this.LocalName = "xmlns";	// 本应是"",但是.net的dom为"xmlns";
					}
					else if (strName[5] == ':')	// 特殊属性，定义了有前缀字符串的名字空间
					{
						this.IsNamespace = true;
						this.Prefix = "xmlns";
						this.LocalName = strName.Substring(6);
						if (this.LocalName == "")
						{
							throw(new Exception("冒号后面应还有内容"));
							// return -1;
						}
					}
					else // 普通属性
					{
						this.IsNamespace = false;
					}
				}
			}

			// 处理普通属性
			if (this.IsNamespace == false)
			{
				int nRet = strName.IndexOf(":");
				if (nRet == -1) 
				{
					this.Prefix = "";
					this.LocalName = strName;
				}
				else 
				{
					this.Prefix = strName.Substring(0, nRet);
					this.LocalName = strName.Substring(nRet + 1);
				}
			}

			return 0;
		}


		// 要求当前reader正好在一个属性上面
		public int Initial(XmlReader reader)
		{
			this.Name = reader.Name;
			this.SetValue(reader.Value);

			// Prefix和	NamespaceURI 两个成员在稍后初始化
			Prefix = null;

			// 搞清楚是不是namespace类型节点
			string strName = reader.Name;
			if (strName.Length >= 5)
			{
				if (strName.Substring(0,5) == "xmlns")	// 大小写敏感
				{
					if (strName.Length == 5)	// 特殊属性，定义了无前缀字符串的名字空间
					{
						this.IsNamespace = true;
						this.Prefix = "xmlns";
						this.LocalName = "";

						// 如果要和.net dom兼容的话
						//this.Prefix = "";	// 本应是"xmlns"，但是.net的dom为"";
						//this.LocalName = "xmlns";	// 本应是"",但是.net的dom为"xmlns";
					}
					else if (strName[5] == ':')	// 特殊属性，定义了有前缀字符串的名字空间
					{
						this.IsNamespace = true;
						this.Prefix = "xmlns";
						this.LocalName = strName.Substring(6);
						if (this.LocalName == "")
						{
							throw(new Exception("冒号后面应还有内容"));
							// return -1;
						}
					}
					else // 普通属性
					{
						this.IsNamespace = false;
					}
				}
			}

			// 处理普通属性
			if (this.IsNamespace == false)
			{
				int nRet = strName.IndexOf(":");
				if (nRet == -1) 
				{
					this.Prefix = "";
					this.LocalName = strName;
				}
				else 
				{
					this.Prefix = strName.Substring(0, nRet);
					this.LocalName = strName.Substring(nRet + 1);
				}
			}

			return 0;
		}


		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return this.Name + "='" + StringUtil.GetXmlStringSimple(this.GetValue()) + "'";
		}

		public override string NamespaceURI 
		{
			get 
			{
				if (m_strTempURI != null)
					return m_strTempURI;	// 在插入前或摘除后起作用

				string strURI = "";
				AttrItem namespaceAttr = null;

				// 对于属性无缺省的名字空间
				if (this.Prefix == "")
					return "";

				if (this.Prefix == "xml")
					return "http://www.w3.org/XML/1998/namespace";

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
					(ElementItem)this.parent,
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

	}


}
