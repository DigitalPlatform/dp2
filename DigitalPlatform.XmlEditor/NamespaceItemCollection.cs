using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.Text;

namespace DigitalPlatform.Xml
{
	public class NamespaceItemCollection : ArrayList
	{
		// 构造一个连续的属性字符串
		// 如果集合为空，返回""；否则，处了正常内容外，左右自动加了一个空格
		public string MakeAttrString()
		{
			if (this.Count == 0)
				return "";
			string strResult = "";
			foreach(NamespaceItem item in this)
			{
				strResult += " " + item.OuterXml;
			}

			return strResult + " ";
		}

		// 分支版本。不适合外部直接调用。
		// 从指定位置向上(祖先)，所有元素的属性都是展开状态，这可以直接利用
		// 我们自己的DOM对象体系来搜集名字空间信息。
		// 收集一个元素节点以外(上方)的名字空间信息。
		// 包含element在内
		// parameters:
		//		element 基准元素
		// return:
		//		返回名字空间信息集合
		public static NamespaceItemCollection GatherOuterNamespacesByNativeDom(
			ElementItem element)
		{
			NamespaceItemCollection nsColl = new NamespaceItemCollection();

			ElementItem current = (ElementItem)element;
			while(true)
			{
				if (current == null)
					break;


				if (current.m_attrsExpand == ExpandStyle.Collapse) 
				{
					/*
					// 为了枚举本层属性，不得不展开属性数组对象，但是暂时没有作收缩回的功能
					current.GetControl().ExpandChild(current, 
						ExpandStyle.Expand,
						current.m_childrenExpand, 
						true);
					Debug.Assert( current.m_attrsExpand == ExpandStyle.Expand, 
						"展开后m_attrsExpand应当为true");
					*/
					Debug.Assert(false, "调用本函数的人，应当确保从要求位置向上祖先路径中，每个元素的属性集合都是处于展开状态。");
				}

				foreach(AttrItem attr in current.attrs)
				{
					if (attr.IsNamespace == false)
						continue;

					nsColl.Add(attr.LocalName, attr.GetValue(), true);	// 只要prefix重就不加入
				}

				current = (ElementItem)current.parent;
			}

			return nsColl;
		}


		// 分支版本。不适合外部直接调用。
		// 从指定位置向上(祖先)，有一个以上元素的属性是收缩状态，这样就没法
		// 利用我们自己的DOM对象体系来搜集名字空间信息，只能模拟一个XML局部字符串来借用
		// .net DOM来帮助搜集名字空间信息
		public static NamespaceItemCollection GatherOuterNamespacesByDotNetDom(
			ElementItem element)
		{
			string strXml = "";

			NamespaceItemCollection nsColl = new NamespaceItemCollection();

			ElementItem current = (ElementItem)element;
			while(true)
			{
				if (current == null 
					|| current is VirtualRootItem)
					break;

				strXml = "<" + current.Name + current.GetAttrsXml() + ">" + strXml + "</" + current.Name + ">";

				current = (ElementItem)current.parent;
			}

			if (strXml == "")
				return nsColl;

			XmlDocument dom  = new XmlDocument();

			try 
			{
				dom.LoadXml(strXml);
			}
			catch (Exception ex)
			{
				throw (new Exception("GatherOuterNamespacesByDotNetDom()加载模拟xml代码出错: " + ex.Message));
			}

			// 先确定起点
			XmlNode currentNode = dom.DocumentElement;
			while(true)
			{
				if (currentNode.ChildNodes.Count == 0)
					break;
				currentNode = currentNode.ChildNodes[0];
			}

			Debug.Assert(currentNode != null, "");

			// 开始搜集信息
			while(true)
			{
				if (currentNode == null)
					break;

				foreach(XmlAttribute attr in currentNode.Attributes)
				{
					if (attr.Prefix != "xmlns" && attr.LocalName != "xmlns")
						continue;

					if (attr.LocalName == "xmlns")
						nsColl.Add("", attr.Value, true);	// 只要prefix重就不加入
					else
						nsColl.Add(attr.LocalName, attr.Value, true);	// 只要prefix重就不加入
				}

				currentNode = currentNode.ParentNode;
				if (currentNode is XmlDocument)	// 这样就算到根以上了
					break;
			}

			return nsColl;
		}

		// 自动判断的版本。适合被外界调用。
		// 收集一个元素节点以外(上方)的名字空间信息。
		// 包含element在内
		// parameters:
		//		element 基准元素
		// return:
		//		返回名字空间信息集合
		public static NamespaceItemCollection GatherOuterNamespaces(
			ElementItem element)
		{
			bool bFound = false;	// 是否有一个以上的元素属性处于收缩状态
			ElementItem current = (ElementItem)element;
			while(true)
			{
				if (current == null)
					break;

				if (current.m_attrsExpand == ExpandStyle.Collapse) 
				{
					bFound = true;
					break;
				}

				current = (ElementItem)current.parent;
			}

			if (bFound == true)
				return GatherOuterNamespacesByDotNetDom(element);
			else
				return GatherOuterNamespacesByNativeDom(element);
		}

	

		// 加入一个新元素
		// 本函数将依照prefix和URI对已经有的所有元素查重，如果遇到重复的，不会加入
		// parameters:
		//		bCheckPrefixDup	是否对prefix也单独查重。如果==true，当prefix重，即便URI不重也不让加入
		// return:
		//		true	prefix和URI完全相同的元素已存在， 未加入新元素
		//		false	没有遇到重复的事项，新元素成功加入
		public bool Add(string strPrefix,
			string strURI,
			bool bCheckPrefixDup)
		{
			Debug.Assert(strPrefix != null,"strPrefix参数不能为null");

			Debug.Assert(strURI != "" && strURI != null,"strURI参数不能为null");

			for(int i=0;i<this.Count;i++)
			{
				NamespaceItem item = (NamespaceItem)(this[i]);
				if (bCheckPrefixDup == true
					&& item.Prefix == strPrefix)
					return true;

				if (item.Prefix == strPrefix
					&& item.URI == strURI)
					return true;
			}

			NamespaceItem newItem = new NamespaceItem();
			newItem.Prefix = strPrefix;
			newItem.URI = strURI;
			this.Add(newItem);
			return false;
		}

	}

	public class NamespaceItem
	{
		public string Prefix = "";
		public string URI = "";

		// 如果把名字空间信息还原为属性的话，属性名是什么?
		public string AttrName
		{
			get 
			{
				if (Prefix == "")   //xmlns
					return "xmlns";
				else
					return "xmlns:" + Prefix;
			}

		}

		public string AttrValue
		{
			get 
			{
				return URI;
			}
		}

		// 左右不含额外空格
		public string OuterXml
		{
			get 
			{
				return AttrName + "='" + StringUtil.GetXmlStringSimple(URI) + "'";
			}
		}
	}

}
