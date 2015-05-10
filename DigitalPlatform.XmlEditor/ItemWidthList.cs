using System;
using System.Collections ;

namespace DigitalPlatform.Xml
{
	// 用来管数组中定义的宽度
	// 序号代表层次
	public class ItemWidthList : CollectionBase
	{
		public ItemWidth this[int index]
		{
			get 
			{
				return (ItemWidth)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}
		public void Add(ItemWidth width)
		{
			InnerList.Add(width);
		}

		// 取一个层的数组，如果没定义，则新创建一个
		public ItemWidth GetItemWidth(int nLevel)
		{
			while(nLevel >= this.Count )
			{
				this.Add (new ItemWidth ());
			}
			return this[nLevel];
		}
	}

	//意图：一个level的各类型区域的width
	public class ItemWidth:ArrayList
	{
		public ItemWidth()
		{  
			this.Add (new PartWidth ("Label",80,1,1));
			this.Add (new PartWidth ("Comment",0,1,1));
			this.Add (new PartWidth ("Text",400,0,0));

			this.Add (new PartWidth ("ExpandHandle",13,1,1));
			this.Add (new PartWidth ("Content",100,0,0));
			this.Add (new PartWidth ("Attributes",100,0,0));
			this.Add (new PartWidth ("Box",100,0,0));
		}

		// 取数组中取出指定区域的width
		public int GetValue(string strName)
		{
			foreach(PartWidth part in this)
			{
				if (part.strName == strName)
					return part.nWidth ;
			}
			return -1;
		}

		public PartWidth GetPartWidth(string strName)
		{
			foreach(PartWidth part in this)
			{
				if(part.strName == strName)
					return part;
			}
			return null;
		}


		// 设指定区域的width，如果已经存在，则修改其值，如果不存在，则新建一个
		public void  SetValue(string strName,int nValue)
		{
			PartWidth partWidth = null;
			foreach(PartWidth part in this)
			{
				if (part.strName == strName)
				{
					partWidth = part;
					break;
				}
			}
			if (partWidth == null)
			{
				partWidth = new PartWidth  (strName,nValue,0,0);
				this.Add (partWidth);
			}
			else 
			{
				partWidth.nWidth = nValue;
			}
		}

		// 增加指定区域的级别
		public void UpGradeNo(string strName)
		{
			foreach(PartWidth part in this)
			{
				if (part.strName == strName)
				{
					part.UpGradeNo ();
					break;
				}
			}
		}

		// 把指定区域的级别还原为缺省值
		public void BackDefaultGradeNo(string strName)
		{
			foreach(PartWidth part in this)
			{
				if (part.strName == strName)
				{
					part.BackDefaultGradeNo ();
					break;
				}
			}
		}

		
	}

	public class PartWidth
	{
		public string strName = "";   //名称
		public int nWidth = -1;       //宽度
  
		public int nDefaultGradeNo = 0; //缺省级别号
		public int nGradeNo = 0;        //级别号

		// 构造函数
		public PartWidth(string myName,
			int myWidth,
			int myDefaultGradeNo,
			int myGradeNo)
		{
			strName = myName;
			nWidth = myWidth;
			nDefaultGradeNo = myDefaultGradeNo;
			nGradeNo = myGradeNo;			
		}



		// 增加指定区域的级别
		public void UpGradeNo()
		{
			this.nGradeNo ++ ;
		}

		// 把指定区域的级别还原为缺省值
		public void BackDefaultGradeNo()
		{
			this.nGradeNo = this.nDefaultGradeNo ;
		}
	}
}
