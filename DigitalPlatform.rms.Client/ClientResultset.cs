using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Threading;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.rms.Client
{

	/// <summary>
	/// 结果集容器
	/// </summary>
	public class ClientResultsetCollection
	{
		public ReaderWriterLock m_lock = new ReaderWriterLock();
		public static int m_nLockTimeout = 5000;	// 5000=5秒


		public Hashtable hashtable = new Hashtable();

		public ClientResultsetCollection()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public ClientResultset NewResultset(string strName)
		{
			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{
				ClientResultset resultset = (ClientResultset)hashtable[strName];

				if (resultset == null)
				{
					resultset = new ClientResultset();
					resultset.Name = strName;

					hashtable.Add(strName, resultset);

					resultset.File.Open(true);
				}

				return resultset;

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}

		public ClientResultset GetResultset(string strName)
		{
			// 加读锁
			this.m_lock.AcquireReaderLock(m_nLockTimeout);
			try 
			{

				ClientResultset resultset = (ClientResultset)hashtable[strName];

				return resultset;
			}
			finally
			{
				this.m_lock.ReleaseReaderLock();
			}
		}

		// 功能: 合并结果集
		// parameters:
		//		strStyle	运算风格 OR , AND , SUB
		//		sourceLeft	源,左边结果集
		//		sourceRight	源,右边结果集
		//		targetLeft	目标,左边结果集
		//		targetMiddle	目标,中间结果集
		//		targetRight	目标,右边结果集
		//		strDebugInfo	处理信息
		// return:
		//		-1	失败
		//		0	成功
		public static int Merge(string strStyle,
			ClientResultset sourceLeft,
			ClientResultset sourceRight,
			ClientResultset targetLeft,
			ClientResultset targetMiddle,
			ClientResultset targetRight,
			bool bOutputDebugInfo,
			out string strDebugInfo,
			out string strError)
		{
			strDebugInfo = "";
			strError = "";
			if (bOutputDebugInfo == true)
			{
				strDebugInfo += "strStyle值:"+strStyle+"\r\n";
				strDebugInfo += "sourceLeft结果集:\r\n"+sourceLeft.Dump()+"\r\n";
				strDebugInfo += "sourceRight结果集:\r\n"+sourceRight.Dump()+"\r\n";
			}

			if (String.Compare(strStyle,"OR",true) == 0)
			{
				if (targetLeft!=null || targetRight!=null)
				{
					Exception ex = new Exception("当strStyle参数值为\"OR\"时，targetLeft参数和targetRight无效，值应为null");
					throw(ex);
				}
			}

			if (sourceLeft != null)
			{
				if (sourceLeft.File.HasIndexed == false)
				{
					strError = "为了提高运行速度, sourceLeft应在调用Merge()前确保indexed";
					return -1;
				}
			}

			if (sourceRight != null)
			{
				if (sourceRight.File.HasIndexed == false)
				{
					strError = "为了提高运行速度, sourceRight应在调用Merge()前确保indexed";
					return -1;
				}
			}

			ClientRecordItem dpRecordLeft;
			ClientRecordItem dpRecordRight;
			int i = 0;   
			int j = 0;
			int ret;
			while (true)
			{
				dpRecordLeft = null;
				dpRecordRight = null;
				if (i >= sourceLeft.Count)
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "i大于等于sourceLeft的个数，将i改为-1\r\n";
					i = -1;
				}
				else if (i != -1)
				{
					try
					{
						dpRecordLeft = (ClientRecordItem)sourceLeft.File[i];
						if (bOutputDebugInfo == true)
							strDebugInfo += "取出sourceLeft集合中第 "+Convert.ToString(i)+" 个元素，Path为 '"+dpRecordLeft.Path+"' \r\n";
					}
					catch
					{
						Exception ex = new Exception("sourceLeft取元素异常: i="+Convert.ToString(i)+"----Count="+Convert.ToString(sourceLeft.Count)+"");
						throw(ex);
					}
				}
				if (j >= sourceRight.Count)
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "j大于等于sourceRight的个数，将j改为-1\r\n";
					j = -1;
				}
				else if (j != -1)
				{
					try
					{
						dpRecordRight = (ClientRecordItem)sourceRight.File[j];
						if (bOutputDebugInfo == true)
							strDebugInfo += "取出sourceRight集合中第 "+Convert.ToString(j)+" 个元素，Path为 '"+dpRecordRight.Path+"'\r\n";
					}
					catch
					{
						Exception ex = new Exception("sourceRight取元素异常: j="+Convert.ToString(j)+"----Count="+Convert.ToString(sourceLeft.Count)+sourceRight.GetHashCode()+"<br/>");
						throw(ex);
					}
				}
				if (i == -1 && j == -1)
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "i,j都等于-1跳出\r\n";
					break;
				}

				if (dpRecordLeft == null)
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "dpRecordLeft为null，设ret等于1\r\n";
					ret = 1;
				}
				else if (dpRecordRight == null)
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "dpRecordRight为null，设ret等于-1\r\n";
					ret = -1;
				}
				else
				{
					ret = dpRecordLeft.CompareTo(dpRecordRight);  //MyCompareTo(oldOneKey); //改CompareTO
					if (bOutputDebugInfo == true)
						strDebugInfo += "dpRecordLeft与dpRecordRight均不为null，比较两条记录得到ret等于"+Convert.ToString(ret)+"\r\n";
				}


				if (String.Compare(strStyle,"OR",true) == 0 
					&& targetMiddle != null)
				{
					if (ret == 0) 
					{
						targetMiddle.File.Add(dpRecordLeft);
						i++;
						j++;
					}
					else if (ret<0) 
					{
						targetMiddle.File.Add(dpRecordLeft);
						i++;
					}
					else if (ret>0)
					{
						targetMiddle.File.Add(dpRecordRight);
						j++;
					}
					continue;
				}

				if (ret == 0 && targetMiddle != null) 
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "ret等于0,加到targetMiddle里面\r\n";
					targetMiddle.File.Add(dpRecordLeft);
					i++;
					j++;
				}

				if (ret<0) 
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "ret小于0,加到targetLeft里面\r\n";

					if (targetLeft != null && dpRecordLeft != null)
						targetLeft.File.Add(dpRecordLeft);
					i++;
				}

				if (ret>0 )
				{
					if (bOutputDebugInfo == true)
						strDebugInfo += "ret大于0,加到targetRight里面\r\n";

					if (targetRight != null && dpRecordRight != null)
						targetRight.File.Add(dpRecordRight);

					j++;
				}
			}
			return 0;
		}


	}


	/// <summary>
	/// 前端结果集
	/// </summary>
	public class ClientResultset
	{
		public string Name = "";

		public ClientResultsetFile File = new ClientResultsetFile();

		public ReaderWriterLock m_lock = new ReaderWriterLock();
		public static int m_nLockTimeout = 5000;	// 5000=5秒

		public long Count
		{
			get 
			{
				return File.Count;
			}
		}

		// 检索文章
		// return:
		//		-1	error
		//		其他 命中数
		public int Search(
			string strServerUrl,
			string strQueryXml,
			RmsChannelCollection Channels,
			string strLang,
			out string strError)

		{
			strError = "";

			string strMessage = "";

			// 加写锁
			this.m_lock.AcquireWriterLock(m_nLockTimeout);
			try 
			{

				this.File.Clear();	// 清空集合


				//if (page.Response.IsClientConnected == false)	// 灵敏中断
				//	return -1;

				RmsChannel channel = Channels.GetChannel(strServerUrl);
				Debug.Assert(channel != null, "Channels.GetChannel 异常");

				strMessage += "--- begin search ...\r\n";

				DateTime time = DateTime.Now;

				//if (page.Response.IsClientConnected == false)	// 灵敏中断
				//	return -1;

				long nRet = channel.DoSearch(strQueryXml,
                    "default",
                    "", // strOuputStyle
					out strError);
				if (nRet == -1) 
				{
					strError = "检索时出错: " + strError;
					return -1;
				}


				TimeSpan delta = DateTime.Now - time;
				strMessage += "search end. time="+delta.ToString()+"\r\n";

				if (nRet == 0)
					return 0;	// not found

				long lTotalCount = nRet;

				//if (page.Response.IsClientConnected == false)	// 灵敏中断
				//	return -1;

				strMessage += "--- begin get search result ...\r\n";

				time = DateTime.Now;

				long lStart = 0;
				long lPerCount = Math.Min(lTotalCount, 1000);


				for(;;)
				{
					//if (page.Response.IsClientConnected == false)	// 灵敏中断
					//	return -1;


					List<string> aPath = null;
					lPerCount = Math.Min((lTotalCount - lStart), 1000);

					nRet = channel.DoGetSearchResult(
                        "default",
                        lStart,
						lPerCount,
						strLang,
						null,	// stop,
						out aPath,
						out strError);
					if (nRet == -1) 
					{
						strError = "检索库时出错: " + strError;
						return -1;
					}


					delta = DateTime.Now - time;
					strMessage += "get search result end. time="+delta.ToString()+"\r\n";


					if (aPath.Count == 0)
					{
						strError = "检索库时获取的检索结果为空";
						return -1;
					}

					//if (page.Response.IsClientConnected == false)	// 灵敏中断
					//	return -1;

					strMessage += "--- begin build storage ...\r\n";

					time = DateTime.Now;

					int i;
					// 加入新行对象。新行对象中，只初始化了m_strRecID参数
					for(i=0;i<aPath.Count;i++)
					{
						ClientRecordItem item = new ClientRecordItem();
						item.Path = (string)aPath[i];
						this.File.Add(item);

						if ((i % 100) == 0)
						{
							strMessage += "process " + Convert.ToString(i)+ "\r\n";
						}

					}

					delta = DateTime.Now - time;
					strMessage += "build storage end. time="+delta.ToString()+"\r\n";

					lStart += aPath.Count;
					if (lStart >= lTotalCount)
						break;

				}


				return 1;

			}
			finally
			{
				this.m_lock.ReleaseWriterLock();
			}
		}


		//功能:列出集合中的所有项
		//返回值: 返回集合成员组成的表格字符串
		public string Dump()
		{
			string strText = "";

			foreach(ClientRecordItem eachRecord in this.File)
			{
				strText += eachRecord.Path + "\r\n";
			}
			return strText;
		}

	}


	public class ClientRecordItem :  DigitalPlatform.IO.Item
	{
		int m_nLength = 0;
		string m_strPath = null;	

		byte [] m_buffer = null;


		public string Path
		{
			get 
			{
				return m_strPath;
			}
			set
			{
				m_strPath = value;

				m_buffer = Encoding.Unicode.GetBytes(m_strPath);;

				this.Length = m_buffer.Length;
			}
		}

		public override int Length
		{
			get 
			{
				return m_nLength;
			}
			set 
			{
				m_nLength = value;
			}
		}

		public override void ReadData(Stream stream)
		{
			if (this.Length == 0)
				throw new Exception("length尚未初始化");


			// 读入Length个bytes的内容
			m_buffer = new byte[this.Length];
			stream.Read(m_buffer, 0, m_buffer.Length);

			// 还原内存对象
			m_strPath = Encoding.Unicode.GetString(m_buffer);
		}


		public override void ReadCompareData(Stream stream)
		{
			if (this.Length == 0)
				throw new Exception("length尚未初始化");


			ReadData(stream);
		}

		public override void WriteData(Stream stream)
		{
			if (m_strPath == null)
			{
				throw(new Exception("m_strPath尚未初始化"));
			}

			if (m_buffer == null)
			{
				throw(new Exception("m_buffer尚未初始化"));
			}


			// 写入Length个bytes的内容
			stream.Write(m_buffer,0, this.Length);
		}

		// 实现IComparable接口的CompareTo()方法,
		// 根据ID比较两个对象的大小，以便排序，
		// 按右对齐方式比较
		// obj: An object to compare with this instance
		// 返回值 A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
		// Less than zero: This instance is less than obj.
		// Zero: This instance is equal to obj.
		// Greater than zero: This instance is greater than obj.
		// 异常: ArgumentException,obj is not the same type as this instance.
		public override int CompareTo(object obj)
		{
			ClientRecordItem item = (ClientRecordItem)obj;

			return String.Compare(this.Path, item.Path, true);
		}
	}



	/// <summary>
	/// 一个结果集的磁盘物理存储结构
	/// </summary>
	public class ClientResultsetFile : ItemFileBase
	{

		public ClientResultsetFile()
		{

		}


		public override  DigitalPlatform.IO.Item NewItem()
		{
			return new ClientRecordItem();
		}

	}




}
