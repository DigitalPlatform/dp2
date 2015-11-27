using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.rms
{
	//设计意图：
	//为了通过关键字检索记录，需要将一条记录的所有关键字提取出来存放到数据库keys表中.
	//记录与关键字是拥有的关系，即存记录时同时存关键字，删记录时同时删关键字
	//数据库中每个关键字包含三项内容：
	//keystring: 具体的文本
	//fromstring: 现在还没有用到
	//idstring: 对应的数据记录ID
	//根据数据记录和keys配置文件创建DpKeys集合
	//关键字配置文件包括四项:
	// <key>
	//   <xpath>/description/title</xpath>    :通过xpath从数据dom中找到具体内容存放到keystring
	//   <from>title</from>                   :直接提取内容存入fromstring
	//   <table name="title"/>                :属于哪个表
	// </key>
	// DpKeys从ArrayList继承，成员为DpKeys对象。用于处理关键字部分。
	public class KeyCollection : List<KeyItem> 
	{


		//设计意图:
		//覆盖记录时，根据新记录得到一些新的key，但原旧记录也存在一些旧的key，
		//可以用笨办法删除原旧记录所有的key，再增加新记录所有的key。
		// 
		//但新旧记录可能有一些重复key，更好的方法是让新旧记录的两组key进行比较，
		//结果分成三部分：
		//1.只在新记录出现的key
		//2.只在旧记录出现的key
		//3.重复的key。
		// 
		//这样在执行覆盖时，增加第一部分，删除第二部分，重复的保持不变，所以就节省了时间
		// 
		//注意调这个函数前，确保集合是排过序的
		// 
		//原来newKeys和oldKeys参数类型都是ref,但因为用户类都是引用类型，所以没必须用ref参数
		// parameters:
		//		newKeys	新记录的key集合
		//		oldKeys	旧记录的key集合
		// return:
		//		重复的key集合
		public static KeyCollection Merge(KeyCollection newKeys,
			KeyCollection oldKeys)
		{
			KeyCollection dupKeys = new KeyCollection();
			if (newKeys.Count == 0)
				return dupKeys;
			if (oldKeys.Count == 0)
				return dupKeys;

			KeyItem newOneKey;
			KeyItem oldOneKey;
			int i = 0;    //i,j等于-1时表示对应的集合结束
			int j = 0;
			int ret;

			//无条件循环，当有一个集合结束（下标变为-1）跳出循环
			while (true)
			{
				if (i >= newKeys.Count)
				{
					i = -1;
					//strInfo += "左结束<br/>";
				}

				if (j >= oldKeys.Count)
				{
					j = -1;
					//strInfo += "右结束<br/>";
				}

				//两个集合都没有结束时，执行比较，否则跳出循环（至少一个集合结束）
				if (i != -1 && j != -1)
				{
					newOneKey = (KeyItem)newKeys[i];
					oldOneKey = (KeyItem)oldKeys[j];

					ret = newOneKey.CompareTo(oldOneKey);  //MyCompareTo(oldOneKey); //改CompareTO

					//strInfo += "左-右,返回"+Convert.ToString(ret)+"<br/>";

					if (ret == 0)  //当等于0时,i,j不改变
					{
						newKeys.Remove(newOneKey);          //改为RemoveAt()
						oldKeys.Remove(oldOneKey);
						dupKeys.Add(oldOneKey);
					}


					//哪一个小，哪一个向下移动

					if (ret<0)  
						i++;

					if (ret>0)
						j++;

					//strInfo += "i=" + Convert.ToString(i) + "j=" + Convert.ToString(j) + "<br/>";
				}
				else
				{
					break;
				}
			}
			return dupKeys;
		}

		//列出集合中的所有项,调试使用
		//返回表格字符串
		public string Dump()
		{
			string strResult = "";

			foreach(KeyItem keyItem in this)
			{
				strResult += "SqlTableName=" + keyItem.SqlTableName + " Key=" + keyItem.Key + " FormValue=" + keyItem.FromValue + " RecordID=" + keyItem.RecordID + " Num=" + keyItem.Num + " KeyNoProcess=" + keyItem.KeyNoProcess + " FromName=" + keyItem.FromName + "\r\n";
			}
			return strResult;
		}

        // 对集合进行去重，去重之前先用DpKeys.Sort()进行排序。
        public void RemoveDup()
        {
            KeyItem prev = null;
            for (int i = 0; i < this.Count; i++)
            {
                KeyItem current = this[i];

                if (prev != null && current.CompareTo(prev) == 0)
                {
                    this.RemoveAt(i);
                    i--;
                    continue;
                }

                prev = current;
            }
        }

#if NO
		//对集合进行去重，去重之前先用DpKeys.Sort()进行排序。
		public void RemoveDup()
		{
			for(int i=0;i<this.Count;i++)
			{
				for(int j=i+1;j<this.Count;j++)
				{
					KeyItem Itemi = (KeyItem)this[i];
					KeyItem Itemj = (KeyItem)this[j];

					if(Itemi.CompareTo(Itemj) == 0)  //MyCompareTo(Itemj) == 0)  //改CompareTo
					{
						this.RemoveAt(j);
						j--;//??????
					}
					else
					{
						break;
					}
				}
			}
		}
#endif

	}


	//设计意图:表示单个key
	//继承IComparable接口
	public class KeyItem : IComparable<KeyItem>
	{
		public string SqlTableName;	// 对应的Sql Server表名
		public string Key;				// key	对应Sql Server表中的keystring字段
		public string FromValue;		// <from>的内容	对应Sql Server表中的fromstring字段
		public string RecordID;		// 记录ID	对应Sql Server表中的idstring字段
		public string Num;				// key的int类型，将存放一个专门的字段里，解决11>2的问题	对应Sql Server表中的keystringnum字段

		public string KeyNoProcess;	// 未处理的key
		public string FromName;		// 来源名，根据语言版本来确定


		
		// parameters:
		//		strSqlTableName	对应的SQL Server表名
		//		strKey			keystring字符串
		//		strFromValue	<from>中的值
		//		strRecordID		记录ID
		//		strNum			key的数字形式
		//		strKeyNoProcess	未处理的key
		//		strFromName		来源名，根据创建key的语言代码确定的
		// 说明:除strKeyNoProcess外，对每一项去前后空白
		public KeyItem(string strSqlTableName,
			string strKey,
			string strFromValue,
			string strRecordID, 
			string strNum,
			string strKeyNoProcess,
			string strFromName)
		{
			this.SqlTableName = strSqlTableName.Trim();
			
			// 这个字段写到库里
			this.Key = strKey.Trim().Replace("\n","");
			this.FromValue = strFromValue.Trim();
			this.RecordID = strRecordID.Trim();
			this.Num = strNum;

			this.KeyNoProcess = strKeyNoProcess;
			this.FromName = strFromName.Trim();
		}


		//隐式执行，可能直接通过DpKey的对象实例来访问
		//obj: 比较的对象
		//0表示相等，其它表示不等
        public int CompareTo(KeyItem keyItem)
		{
            // 2013/2/18
            // tablename
            int nRet = String.CompareOrdinal(this.SqlTableName, keyItem.SqlTableName);
            if (nRet != 0)
                return nRet;

			// 依Key排序
			nRet = String.Compare(this.Key,keyItem.Key);
            if (nRet != 0)
                return nRet;

			// 再依记录号排序
            nRet = String.Compare(this.RecordID, keyItem.RecordID);
            if (nRet != 0)
                return nRet;

            nRet = String.Compare(this.FromName, keyItem.FromName);
            if (nRet != 0)
                return nRet;


			// 最后依数字key排序
            nRet = String.Compare(this.Num, keyItem.Num);
            return nRet;
		}
	} 
}
