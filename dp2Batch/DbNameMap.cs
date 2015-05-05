using System;
using System.Collections;
using System.Diagnostics;

namespace dp2Batch1 // 本类已经被废止
{
	/// <summary>
	/// backup文件中的数据库名 和 本次拟转入的目标库名之间的对照表
	/// </summary>
	public class DbNameMap
	{
		Hashtable m_hashtable = new Hashtable();
		ArrayList m_list = new ArrayList();

		public DbNameMap()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// 构造map对象。
		// parameters:
		//		strDbPaths	分号间隔的路径。将创建为Origin和Target相同的overwrite事项。
		//					格式为 dbpathorigin-dbpathtarget|style;...
		//					可以省略为dbpath1;dbpath2;... 每个path既当origin也当target用
		public static DbNameMap Build(string strDbPaths,
            out string strError)
		{
            strError = "";
			DbNameMap map = new DbNameMap();

			string[] aPath = strDbPaths.Split(new char[]{';'});

			for(int i=0;i<aPath.Length;i++)
			{
				string strLine = aPath[i].Trim();
				if (strLine == "")
					continue;
				string strOrigin = "";
				string strTarget = "";
				string strStyle = "";

				int nRet = strLine.IndexOf("|");
				if (nRet == -1)
					strStyle = "overwrite";
				else 
				{
					strStyle = strLine.Substring(nRet + 1).Trim().ToLower();
					strLine = strLine.Substring(0, nRet).Trim();
				}

				nRet = strLine.IndexOf("-");
				if (nRet == -1)
				{
					strOrigin = strLine;
					strTarget = strLine;
				}
				else 
				{
					strOrigin = strLine.Substring(0, nRet).Trim();
					strTarget = strLine.Substring(nRet + 1).Trim();
				}

				if (map.NewItem(strOrigin, strTarget, strStyle, out strError) == null)
                    return null;
			}

			return map;
		}

		// 将全部内容转换为字符串型态
		// parameters:
		//		bCompact	是否为紧凑型态
		public string ToString(bool bCompact)
		{
			string strResult = "";
			for(int i=0;i<this.m_list.Count;i++)
			{
				DbNameMapItem item = (DbNameMapItem)this.m_list[i];

				if (strResult != "")
					strResult += ";";

				if (bCompact == true)
				{
					if (item.Origin == item.Target)
					{
						if (item.Style == "overwrite")
							strResult += item.Origin;
						else
							strResult += item.Origin + "|" + item.Style;
					}
					else 
					{
						if (item.Style == "overwrite")
							strResult += item.Origin + "-" + item.Target;
						else
							strResult += item.Origin + "-" + item.Target + "|" + item.Style;

					}

				}
				else
					strResult += item.Origin + "-" + item.Target + "|" + item.Style;
			}

			return strResult;
		}

		public DbNameMapItem NewItem(string strOrigin,
			string strTarget,
			string strStyle,
            out string strError)
		{
			return NewItem(strOrigin,
				strTarget,
				strStyle,
                -1,
                out strError);
		}

		// 加入新项
		// 需要查重
		public DbNameMapItem NewItem(string strOrigin,
			string strTarget,
			string strStyle,
			int nInsertPos,
            out string strError)
		{
            strError = "";

            Debug.Assert(strStyle.IndexOf("--") == -1, "");

			DbNameMapItem item  = new DbNameMapItem();
			item.Origin = strOrigin;
			item.Target = strTarget;
			item.Style = strStyle;

			if (nInsertPos == -1)
				this.m_list.Add(item);
			else 
				this.m_list.Insert(nInsertPos, item);

            // 2010/2/25 new add
            if (this.m_hashtable.ContainsKey(strOrigin.ToUpper()) == true)
            {
                strError = "已有名为 '" + strOrigin + "' 的事项，不能重复加入";
                return null;
            }

			this.m_hashtable.Add(strOrigin.ToUpper(), item);

			return item;
		}

		public void Clear()
		{
			this.m_list.Clear();
			this.m_hashtable.Clear();
		}

		public int Count
		{
			get 
			{
				return m_list.Count;
			}
		}

		public DbNameMapItem this[int nIndex]
		{
			get 
			{
				return (DbNameMapItem)this.m_list[nIndex];
			}
			set 
			{
				DbNameMapItem olditem = (DbNameMapItem)this.m_list[nIndex];
				this.m_hashtable.Remove(olditem.Origin.ToUpper());

				this.m_list[nIndex] = value;
				this.m_hashtable.Add(value.Origin.ToUpper(), value);
			}
		}

		// 以Origin字符串作为key搜索
		public DbNameMapItem this[string strOrigin]
		{
			get 
			{
				return (DbNameMapItem)this.m_hashtable[strOrigin.ToUpper()];
			}
			set 
			{
				DbNameMapItem olditem = (DbNameMapItem)this.m_hashtable[strOrigin.ToUpper()];
				int nOldIndex = -1;
				if (olditem != null)
				{
					nOldIndex = this.m_list.IndexOf(olditem);
					this.m_hashtable.Remove(olditem.Origin.ToUpper());
					this.m_list.Remove(olditem);
				}

				if (nOldIndex != -1)
					this.m_list[nOldIndex] = value;
				else
					this.m_list.Add(value);

				this.m_hashtable.Add(value.Origin.ToUpper(), value);
			}
		}

		public DbNameMap Clone()
		{
			/*
			DbNameMap result = new DbNameMap();

			foreach( DictionaryEntry entry in this)
			{
				// int i =0;
				

				DbNameMapItem olditem = (DbNameMapItem)this[entry.Key];

				DbNameMapItem newitem = new DbNameMapItem();

				newitem.strOrigin = olditem.strOrigin;
				newitem.strStyle = olditem.strStyle;
				newitem.strTarget = olditem.strTarget;

				result.Add(entry.Key, newitem);
			}
			*/
			DbNameMap result = new DbNameMap();

			for(int i=0;i<this.m_list.Count;i++)
			{
				DbNameMapItem olditem = (DbNameMapItem)this.m_list[i];

                string strError = "";
				if (result.NewItem(olditem.Origin, olditem.Target, olditem.Style, out strError) == null)
                    throw new Exception(strError);
			}
			return result;
		}

		// 根据源路径匹配适合的事项
		public DbNameMapItem MatchItem(string strOrigin)
		{
			for(int i=0;i<this.Count;i++)
			{
				DbNameMapItem item = this[i];

				if (strOrigin == "{null}" || strOrigin == "" || strOrigin == null)
				{
					if (item.Origin == "{null}")
						return item;
					if (item.Origin == "*")
					{
						// 本来这就算匹配上了,但是还需要仔细看看strTarget是否为"*"
						if (item.Target != "*")
							return item;
					}
					continue;
				}

				if (item.Origin == "*")
					return item;

				if (String.Compare(item.Origin, strOrigin, true) == 0)
					return item;
				
			}


			return null;
		}


	}

	public class DbNameMapItem
	{
		public string Origin = "";	// 源头
		public string Target = "";	// 目的

		public string Style;	// 风格。比方说"overwrite"或者"append"

	}
}
