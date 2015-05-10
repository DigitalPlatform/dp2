using System;
using System.Collections;
using System.Diagnostics;

namespace DigitalPlatform.Xml
{
	// 存放一些静态函数
	public class ItemUtil
	{
		static void RemoveDup(ref Hashtable target,
			Hashtable dup)
		{
			if (dup.Count == 0)
				return;

			ICollection array = target.Keys;

			int nOldCount = array.Count;

			ArrayList aWantRemove = new ArrayList();

			foreach(object strKey in array)
			{
				if (dup.Contains(strKey) == true) 
				{
					aWantRemove.Add(strKey);
				}
			}

			for(int i=0;i<aWantRemove.Count;i++)
			{
				target.Remove(aWantRemove[i]);
				// Debug.Assert(nOldCount == array.Count, "Hashtable.Keys不是只读的?");
			}

		}


		
		// 获得start独有的并且在top圈子以内未被定义的若干prefix
		// 注: start到top范围内被start以外元素使用过的prefix不包含在内
		// parameters:
		//		start	起点元素
		//		top		范围顶部元素。注意包含这个元素。
		public static void GetUndefinedPrefix(ElementItem start,
			ElementItem top,
			out ArrayList aResult)
		{
			aResult = null;
			

			Hashtable aPrefix = start.GetUsedPrefix(true);

			if (start == top)
				goto END1;

			ElementItem current = (ElementItem)start.parent;
			while(true)
			{
				if (current == null)
					break;

				Hashtable aCurrentLevelPrefix = current.GetPrefix(ElementItem.GetPrefixStyle.All);

				// 从aPrefix中去除和aCurrentLevelPrefix相交的部分
				RemoveDup(ref aPrefix,
					aCurrentLevelPrefix);

				if (current == top)
					break;

				current = (ElementItem)current.parent;
			}

			END1:

				aResult = new ArrayList();

			aResult.AddRange(aPrefix.Keys);
		}

		// 假定nsColl中的信息即将还原为xml的属性字符串，插入element节点的属性集合中，
		// 本函数为确保不会发生属性重名的错误，而特意对nsColl去重
		public static void RemoveAttributeDup(ElementItem element,
			ref NamespaceItemCollection nsColl)
		{

			if (nsColl.Count == 0)
				return;

			// 对基准元素的属性进行一次去重操作
			if (element.attrs != null)
			{
				for(int i=0; i<nsColl.Count; i++)
				{
					NamespaceItem item = (NamespaceItem)nsColl[i];

					string strAttrString = item.AttrName;

					bool bOccurNamespaceAttr = false;
					foreach(AttrItem attr in element.attrs)
					{
						if (attr.IsNamespace == false)
							continue;

						bOccurNamespaceAttr = true;

						if (attr.Name == strAttrString) 
						{
							nsColl.RemoveAt(i);
							i--;
							break;
						}
					
					}

					if (bOccurNamespaceAttr == false)
						break;	// 说明element.attrs集合中，全部是普通类型的属性，没有名字空间型的属性，那样就意味着不用去重了
				}

			}

		}

	

		// ture 找到
		public static bool LocateNamespaceByUri(ElementItem startItem,
			string strURI,
			out string strPrefix,
			out Item namespaceAttr)
		{
			strPrefix = "";
			namespaceAttr = null;

			ElementItem currentItem = startItem;
			while(true)
			{
				if (currentItem == null)
					break;

				if (currentItem.attrs != null)
				{
					foreach(AttrItem attr in currentItem.attrs)
					{
						if (attr.IsNamespace == false)
							continue;

						if (attr.GetValue() == strURI)
						{
							strPrefix = attr.Name;
							namespaceAttr = attr;
							return true;
						}
					}
				}
				currentItem = (ElementItem)currentItem.parent;
			}

			return false;

		}

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
		public static bool LocateNamespaceByPrefix(ElementItem startItem,
			string strPrefix,
			out string strURI,
			out AttrItem namespaceAttr)
		{
			strURI = "";
			namespaceAttr = null;

			/*
			Debug.Assert(strPrefix != "", "strPrefix参数不应当为空。前缀为空时，无需调用本函数就知道没有找到");
			if (strPrefix == "")
				return false;
			*/

			ElementItem currentItem = startItem;
			while(true)
			{
				if (currentItem == null)
					break;

				foreach(AttrItem attr in currentItem.attrs)
				{
					if (attr.IsNamespace == false)
						continue;

					string strLocalName = "";
					int nIndex = attr.Name.IndexOf(":");
					if (nIndex >= 0)
					{
						strLocalName = attr.Name.Substring(nIndex + 1);
					}
					else
					{
						Debug.Assert(attr.Name == "xmlns", "名字空间型的属性，如果name中无冒号，必然为xmlns。");
						if (attr.Name == "xmlns")
						{
							strLocalName = "";
						}
					}

					if (strLocalName == strPrefix)
					{
						strURI = attr.GetValue();
						namespaceAttr = attr;
						return true;
					}
				}

				currentItem = (ElementItem)currentItem.parent;
			}

			return false;

		}

        // parameters:
		//      descendant  假设的后代  低
		//      ancestor    假设的祖先    高
		public static bool IsAncestorOf(ElementItem descendant,
			ElementItem ancestor)
		{

			Item currentItem = descendant.parent;
			while(true)
			{
				if (currentItem == null)
					return false;

				if (currentItem == ancestor)
					return true;

				currentItem = currentItem.parent;
			}
			// return false;
		}

		// 判断一个低级节点是否是高级节点的下级
		public static bool IsBelong(Item lowerItem,Item higherItem)
		{
			Item thisItem = lowerItem;
			while(true)
			{
				if (thisItem == null)
					break;
				if (thisItem.Equals (higherItem) == true)
					return true;

				thisItem = thisItem.parent ;
			}
			return false;
		}

		
		public static string GetLocalName(string strName)
		{
			string strLocalName = "";
			bool bIsNamespace = false;

			if (strName.Length >= 5)
			{
				if (strName.Substring(0,5) == "xmlns")	// 大小写敏感
				{
					if (strName.Length == 5)	// 特殊属性，定义了无前缀字符串的名字空间
					{
						bIsNamespace = true;
						strLocalName = "";

						// 如果要和.net dom兼容的话
						//this.Prefix = "";	// 本应是"xmlns"，但是.net的dom为"";
						//this.LocalName = "xmlns";	// 本应是"",但是.net的dom为"xmlns";
					}
					else if (strName[5] == ':')	// 特殊属性，定义了有前缀字符串的名字空间
					{
						bIsNamespace = true;
						strLocalName = strName.Substring(6);
						if (strLocalName == "")
						{
							throw(new Exception("冒号后面应还有内容"));
						}
					}
					else // 普通属性
					{
						bIsNamespace = false;
					}
				}
			}

			// 处理普通属性
			if (bIsNamespace == false)
			{
				int nRet = strName.IndexOf(":");
				if (nRet == -1) 
				{
					strLocalName = strName;
				}
				else 
				{
					strLocalName = strName.Substring(nRet + 1);
				}
			}
			return strLocalName;
		}


		// 得到startItem的一个附近的节点
		// MoveMember.Auto      先下，再上，再父亲
		// MoveMember.Front     前方
		// MoveMember.Behind    方
		public static Item GetNearItem(Item startItem,
			MoveMember moveMember)
		{
			Item item = startItem;

			if (item == null)
				return null;

			if (item.parent == null)
				return null;

			int nRet = -1;
			ItemList aItem = null;
			// 如果item的parent对象是ElementItem，先试图找其儿子
			if (item.parent != null) 
			{
				aItem = item.parent.children;
				if (aItem != null)
					nRet = aItem.IndexOf(item);
				if (nRet == -1)
				{
					Debug.Assert(item.parent != null,
						"上面已经判断过了");

					aItem = ((ElementItem)item.parent).attrs ;
					if (aItem != null)
						nRet = aItem.IndexOf(item);
				}	
			
			}

			if (nRet == -1)
				return null;

			Debug.Assert(aItem != null, "上面已经判断过了nRet");

			if (moveMember == MoveMember.Auto)
			{
				if (aItem.Count > nRet + 1)  //返回下一个
					return aItem[nRet+1];
				else if (nRet > 0)
					return aItem[nRet -1];  // 返回上一个
			}

			if (moveMember == MoveMember.Front)
			{
				if (nRet > 0)
					return aItem[nRet - 1];
			}
			if (moveMember== MoveMember.Behind  )
			{
				if (aItem.Count  > nRet + 1)
					return aItem[nRet + 1];
			}

			return item.parent;   //返回父亲
		}

		// 由path得到Item
		// parameters:
		//		itemRoot	根item
		//		strPath	path
		//		item	out参数，返回item
		// return:
		//		-1	error
		//		0	succeed
		public static int Path2Item(ElementItem itemRoot,
			string strPath, 
			out Item item)
		{
			item = null;
			if (itemRoot == null)
				return -1;
			if (itemRoot.children == null)
				return -1;

			if (strPath == "")
				return -1;

			int nPosition = strPath.IndexOf ('/');

			string strLeft= "";
			string strRight = "";
			if (nPosition >= 0)
			{
				strLeft = strPath.Substring(0,nPosition);
				strRight = strPath.Substring(nPosition+1);
			}
			else
			{
				strLeft = strPath;
			}

			//得到序号
			int nIndex = getNo(strLeft);

			//得到名称
			nPosition = strLeft.IndexOf ("[");
			string strName = strLeft.Substring (0,nPosition);

			int i=0;
			foreach(Item child in itemRoot.children )
			{
				if (child.Name == strName)
				{
					//递归
					if (i == nIndex)
					{
						if (strRight == "")
						{
							item = child;
							break;
						}
						else 
						{
							if (!(child is ElementItem))
								return -1;	// text类型节点再也无法继续向下找儿子了
							Debug.Assert(child is ElementItem);
							return Path2Item((ElementItem)child,
								strRight,
								out item);
						}
					}
					else
						i++;
				}
			}
			return 0;
		}

		
		// 得到括在[]中的号码，转换为数值，并且减1
        // parameters:
		//      strText 传入的字符串
        // return:
		//      该函数返回的数值，注意比较序号，一定要用数值类型，不要用字符类型，自己原来就错用了字符类型
		private static int getNo(string strText)
		{
			//首先看"["在该字符串出现的位置
			int nPositionNo = strText.IndexOf("[");

			//如果instr返回值大于，则表示确实出现了，否则没有"]"或其它情况
			if (nPositionNo > 0)
			{
				//截掉strText从"["开始左边的字符，只剩右边
				strText = strText.Substring(nPositionNo+1);

				//然后再从剩下的字符串找"]"出现的位置
				nPositionNo = strText.IndexOf("]");

				//如果找到，则只保存"]"左边的内容，
				if (nPositionNo > 0)
					strText = strText.Substring(0,nPositionNo);  //nPositionNo-1);
				else
					return 0;

				//如果左右截完后，剩下空，则没有序号，函数返回0，跳出函数
				if (strText == "")
					return 0;


				//否则，strPath字符串变成了一个只有数字形式的字符串
				//使用cint()转换成数值，且减1，因为DOM中是从0开始的
				return System.Convert.ToInt32(strText)-1;
			}
			else
			{
				return 0;
			}
		}
		

		// 由item得到path
        // parameters:
		//      itemRoot    根item
		//      item        给定的item
		//      strPath     out参数，返回item的path
        // return:
        //      -1  出错
        //      0   成功
		public static int Item2Path(ElementItem itemRoot,
			Item item,
			out string strPath)
		{
			strPath = "";
			if (itemRoot == null)
				return -1;
			if (item == null)
				return -1;


			Item itemMyself;
			Item itemTemp;

			int nIndex;


			//当为属性节点时，加了属性path字符串
			string strAttr = "";
			if (item is AttrItem )  
			{
				strAttr = "/@" + item.Name;
				item = item.parent ;
			}

			while(item != null)
			{
				//与根节点相等
				if (item.Equals(itemRoot) == true)
					break;

				itemMyself = item;
				item = item.parent;

				if (item == null)
					break;
				
				itemTemp = null;
				if (item is ElementItem 
					&& ((ElementItem)item).children != null)
				{
					itemTemp = ((ElementItem)item).children[0];
				}

				nIndex = 1;

				while(itemTemp != null)
				{
					if (itemTemp.Equals(itemMyself) == true)
					{
						if (strPath != "")
							strPath = "/" + strPath;

						strPath = itemMyself.Name + "[" + System.Convert.ToString(nIndex) + "]" + strPath;
						
						break;
					}

					if (itemTemp.Name == itemMyself.Name)
						nIndex += 1;
					
					itemTemp = itemTemp.GetNextSibling();
				}
			}

			strPath = strPath + strAttr;

			if (strPath == "")
				return 0;
			else
				return 1;
		}

		public static string GetPartXpath(ElementItem parent,
			Item item)
		{
			string strPath = "";

			Item currentItem = null;
			if (parent.children != null)
			{
				currentItem = parent.children[0];
			}

			int nIndex = 1;

			while(currentItem != null)
			{
				if (currentItem == item)
				{
					strPath = item.Name + "[" + System.Convert.ToString(nIndex) + "]";
					break;
				}

				if (currentItem.Name == item.Name)
					nIndex += 1;
					
				currentItem = currentItem.GetNextSibling();
			}
			return strPath;
		}
	}

}
