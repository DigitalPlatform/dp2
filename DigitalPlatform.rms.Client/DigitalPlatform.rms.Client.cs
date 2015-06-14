using System;
using System.Windows .Forms ;
using System.Drawing ;
using System.Xml ;
using System.Collections ;
using System.Threading ;
using System.Net ;
using System.IO ;

using DigitalPlatform.Xml  ;
using DigitalPlatform.Text  ;
using DigitalPlatform.IO ;
using DigitalPlatform.Range  ;
using DigitalPlatform.rms ;

namespace DigitalPlatform.rms.Client
{

	public class FileItem
	{
		public string strClientPath = null;
		public string strItemPath = null;
		public string strFileNo = null;
	}

	//服务器列表
	public class HostList:ArrayList
	{
		public string m_strFileName;	// XML文件名

		public ReaderWriterLock m_lock = new ReaderWriterLock();
		public static int m_nLockTimeout = 5000;	// 5000=5秒

		public HostList(string strFileName)
		{
			m_strFileName = strFileName;
			Load(strFileName);
		}

		// 根据hosturl找到Host对象
		public HostItem GetHost(string strHostUrl)
		{
			// 对本list要进行读取的操作
			m_lock.AcquireReaderLock(m_nLockTimeout);

			try 
			{
				for(int i=0;i<this.Count;i++) 
				{
					HostItem obj = (HostItem)this[i];
					if (obj.m_strHostURL == strHostUrl)
						return obj;
				}
			}
			finally 
			{
				m_lock.ReleaseReaderLock();
			}
			return null;	// not found
		}


		// 根据hosturl找到Host对象
		public HostItem NewHost(string strHostUrl)
		{
			if (strHostUrl == "")
				return null;
			// 对本list要进行读取的操作
			m_lock.AcquireWriterLock(m_nLockTimeout);

			try 
			{
				for(int i=0;i<this.Count;i++) 
				{
					HostItem obj = (HostItem)this[i];
					if (obj.m_strHostURL == strHostUrl)
						return obj;	// 已经有了
				}

				HostItem newhost = new HostItem();
				newhost.m_strHostURL = strHostUrl;
				this.Add(newhost);

				return newhost;
			}
			finally 
			{
				m_lock.ReleaseWriterLock();
			}
		}


		public HostList()
		{}

		//根据根节点，创建HostList
		public static HostList CreateBy(XmlNode nodeRoot)
		{
			HostList hostlistObj = new HostList();
			XmlNodeList nodes = nodeRoot.SelectNodes("host");

			// 对list要进行插入元素的操作
			hostlistObj.m_lock.AcquireWriterLock(m_nLockTimeout);

			try 
			{
				for(int i=0;i<nodes.Count;i++) 
				{
					HostItem hostObj = HostItem.CreateBy(nodes[i]);
					hostlistObj.Add(hostObj);
				}
			}
			finally 
			{
				hostlistObj.m_lock.ReleaseWriterLock();
			}

			return hostlistObj;
		}

		
		public string GetXml()
		{
			string strCode = "";

			strCode = "<root";
			strCode += " >";
			// 关心下级

			// 对本list要进行读取的操作
			m_lock.AcquireReaderLock(m_nLockTimeout);
			try 
			{
				for(int i=0;i<this.Count;i++) 
				{
					strCode += ((HostItem)this[i]).GetXml();
				}
			}
			finally
			{
				m_lock.ReleaseReaderLock();
			}

			strCode += "</root>";
			return strCode;
		}


		~HostList()
		{
			if (m_strFileName != "") 
			{				
				Save(m_strFileName);
			}
		}


		//从XML文件中装载全部信息
		//strFileName: XmlL文件名
		public void Load(string strFileName)
		{
			XmlDocument dom = new XmlDocument();
			dom.Load(strFileName);
			// 从<root>下面的<user>元素初始化User对象
			CreateBy(dom.DocumentElement);
		}


		//供外部调的保存到文件的函数
		public void Save()
		{
			this.Save(this.m_strFileName );
		}

		// 保存全部信息到xml文件
		private void Save(string strFileName)
		{
			if (strFileName == null)
				return;
			if (strFileName == "")
				return ;
			string strCode = "";

			strCode = "<?xml version='1.0' encoding='utf-8' ?>"
				+ GetXml();

			StreamWriter sw = new StreamWriter(strFileName, 
				false,	// overwrite
				System.Text.Encoding.UTF8);
			sw.Write(strCode);
			sw.Close();
		}
	}


	// 服务器对象
	public class HostItem
	{
		public string m_strHostURL;
		public CookieContainer Cookies = new System.Net.CookieContainer();
		
		//根据node创建本对象
		public static HostItem CreateBy (XmlNode node)
		{
			HostItem newHost = new HostItem ();
			newHost.m_strHostURL = DomUtil.GetAttr (node,"name");
			return newHost;
		}

		
		//得到AttrXml字符串
		public string GetAttrXml()
		{
			string strCode;
			strCode = " name=\"" + m_strHostURL + "\"";
			return strCode;
		}


		//得到Xml
		public string GetXml()
		{
			string strCode = "";

			strCode = "<host";
			strCode += GetAttrXml();
			strCode += " >";
			strCode += "</host>";
			return strCode;
		}
	}


	// 查询式对象
	public class QueryClient
	{
	
		// 中间格式检索式 转换为 dprms系统正规的XML检索式
		// 所谓中间格式，就是 target|query word
		public static string ProcessQuery2Xml(
			string strQuery,
			string strLanguage)
		{
			if (strQuery == "")
				return "";

			string strTarget = "";
			string strAllWord = "";
			int nPosition = strQuery.IndexOf("|");
			if (nPosition >= 0)
			{
				strAllWord = strQuery.Substring(0,nPosition);
				strTarget = strQuery.Substring(nPosition+1);
			}
			else
			{
				strTarget = strQuery;
			}
	
			string[] aWord;
			aWord = strAllWord.Split(new Char [] {' '});
	
			if (aWord == null)
				aWord[0] = strAllWord;
	
			string strXml = "";
			string strWord;
			string strMatch;
			string strRelation;
			string strDataType;	
			foreach(string strOneWord in aWord)
			{
				if (strXml != "")
					strXml += "<operator value='OR'/>";
				string strID1;
				string strID2;
				SplitRangeID(strOneWord,out strID1, out strID2);
				if (StringUtil.IsNum(strID1)==true 
					&& StringUtil.IsNum(strID2) && strOneWord!="")
				{
					strWord = strOneWord;
					strMatch = "exact";
                    strRelation = "range";  // 2012/3/29
					strDataType = "number";
				}
				else
				{
					string strOperatorTemp;
					string strRealText;
				
					int ret;
					ret = GetPartCondition(strOneWord, out strOperatorTemp,out strRealText);
				
					if (ret == 0 && strOneWord!="")
					{
						strWord = strRealText;
						strMatch = "exact";
						strRelation = strOperatorTemp;
						if(StringUtil.IsNum(strRealText) == true)
							strDataType = "number";					
						else
							strDataType = "string";
					}
					else
					{
						strWord = strOneWord;
						strMatch = "left";
						strRelation = "=";
						strDataType = "string";					
					}
				}

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
				strXml += "<item><word>"
                    +StringUtil.GetXmlStringSimple(strWord)+
					"</word><match>"+strMatch+
					"</match><relation>"+strRelation+
					"</relation><dataType>"+strDataType+
					"</dataType></item>";
			}
			if (strLanguage == "")
				MessageBox.Show ("语言已选中，怎么为空呢？");
			strXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strTarget)     // 2007/9/14
                +"'>"+strXml+"<lang>"+strLanguage+"</lang></target>";
			return strXml;
		}

		
		//将"***-***"拆分成两部分
		public static int SplitRangeID(string strRange ,
			out string strID1, 
			out string strID2)
		{
			int nPosition;
			nPosition = strRange.IndexOf("-");
			strID1 = "";
			strID2 = "";
			if (nPosition > 0)
			{
				strID1 = strRange.Substring(0,nPosition).Trim();
				strID2 = strRange.Substring(nPosition+1).Trim();
				if (strID2 == "")
					strID2 = "9999999999";
			}
			if (nPosition == 0)
			{
				strID1 = "0";
				strID2 = strRange.Substring(1).Trim();
			}
			if (nPosition < 0)
			{
				strID1 = strRange.Trim();
				strID2 = strRange.Trim();
			}
			return 0;
		}


		// 根据表示式，得到操作符和值
		// return:
		//		0	有关系操作符
		//		-1	无关系操作符				
		public static int GetPartCondition(string strText,
			out string strOperator,
			out string strRealText)
		{
			strText = strText.Trim();
			strOperator = "=";
			strRealText = strText;
			int nPosition;
			nPosition = strText.IndexOf(">=");
			if(nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+2);

				strOperator = ">=";
				return 0;
			}
			nPosition = strText.IndexOf("<=");
			if(nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+2);
				strOperator = "<=";
				return 0;
			}
			nPosition = strText.IndexOf("<>");
			if(nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+2);
				strOperator = "<>";
				return 0;
			}

			nPosition = strText.IndexOf("><");
			if(nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+2);
				strOperator = "<>";
				return 0;
			}
			nPosition = strText.IndexOf("!=");
			if(nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+2);
				strOperator = "<>";
				return 0;
			}
			nPosition = strText.IndexOf(">");
			int nPosition2 = strText.IndexOf(">=");
			if(nPosition2<0 && nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+1);
				strOperator = ">";
				return 0;
			}
			nPosition = strText.IndexOf("<");
			nPosition2 = strText.IndexOf("<=");
			if(nPosition2<0 && nPosition >= 0)
			{
				strRealText = strText.Substring(nPosition+1);
				strOperator = "<";
				return 0;
			}
			return -1;
		}


		//将各项组合成XML的一项
		public static int combination(
			string strTarget,
			string strWord,
			string strMatch,
			string strRelation,
			string strDataType,
			string strOperator,
			ref string strXml)
		{
			if (strXml!="")  // && (i != nLine-1) && (i != 0))
			{
				strXml += "<operator value='"+strOperator+"'/>";
			}
			strXml += "<target list='"+
                StringUtil.GetXmlStringSimple(strTarget)     // 2007/9/14
                + "'>" + 
				"<item>" +
				"<word>" + StringUtil.GetXmlStringSimple(strWord) 
                +"</word>"+
				"<match>" + strMatch +"</match>"+
				"<relation>" + strRelation +"</relation>"+
				"<dataType>" + strDataType +"</dataType>" +
				"</item>" +
				"</target>";
			return 0;
		}
	}



	public class ClientUtil
	{
/*
		public static int DaBagFileAdded(string strXmlText,
			ArrayList aFileName,
			Stream target,
			out string strInfo)
		{
			strInfo = "";

			XmlDocument dom = new XmlDocument ();
			try
			{
				dom.LoadXml(strXmlText);
			}
			catch(Exception ex )
			{
				strInfo += "不合法的XML\r\n"+ex.Message ;
				return -1;
			}

			XmlNodeList listFile = dom.SelectNodes("//file");
			

			//得到最大的文件号
			int nMaxFileNo = 0;
			foreach(XmlNode node in listFile)
			{
				string strFileNo = DomUtil.GetNodeText (node);
				int nPosition = strFileNo.IndexOf (".");
				if (nPosition > 0)
				{
					int nTempNo = Convert.ToInt32 (strFileNo.Substring (0,nPosition));
					if (nTempNo > nMaxFileNo)
						nMaxFileNo = nTempNo;
				}
			}

			//加大一号
			nMaxFileNo ++;

			//拼出每个文件的ID，并保存到xml里
			ArrayList aFileID = new ArrayList ();
			for(int i=0;i<aFileName.Count ;i++)
			{
				string strFileName = (string)aFileName[i];
				string strExtention = Path.GetExtension (strFileName);
				string strFileID = Convert.ToString (nMaxFileNo++)+strExtention;
				//先给xml里设好
				XmlNode nodeFile = listFile[i];
				DomUtil.SetNodeText (nodeFile,strFileID);
				//加到aFileID数组里，在打包时用到
				aFileID.Add (strFileID);
			}

			long lTotalLength = 0;  //内容总长度，不包括开头的8个字节
			//长度字节数组
			byte[] bufferLength = new byte[8];

			//记住写总长度的位置
			long lPositon = target.Position ;

			//1.开头空出8字节，最后写总长度*****************
			target.Write(bufferLength,0,8);

			//2.写XMl文件*******************
			MemoryStream ms = new MemoryStream ();
			dom.Save (ms);

			//将字符串转换成字符数组
			//byte[] bufferXmlText = System.Text.Encoding.UTF8.GetBytes(strXmlText);
			
			//算出XML文件的字节数
			long lXmlLength = ms.Length  ;//(long)bufferXmlText.Length;
			bufferLength =	System.BitConverter.GetBytes(lXmlLength);
			
			target.Write(bufferLength,0,8);
			lTotalLength += 8;

			//target.Write (bufferXmlText,0,lXmlLength);
			ms.Seek (0,SeekOrigin.Begin );
			StreamUtil.DumpStream (ms,target);
			lTotalLength += lXmlLength ;

			//3.写文件
			long lFileLengthTotal = 0;  //全部文件的长度,也可以继续用lTotalLength，但新申请一个变量出问题的情况更小
			for(int i=0;i<aFileName.Count ;i++)
			{
				FileStream streamFile = File.Open ((string)aFileName[i],FileMode.Open);
				WriteFile(streamFile,
					(string)aFileID[i],
					target,
					ref lFileLengthTotal);
				streamFile.Close ();
			}
			lTotalLength += lFileLengthTotal;

			//4.写总长度
			bufferLength = System.BitConverter.GetBytes(lTotalLength);
			target.Seek (lPositon,SeekOrigin.Begin);
			target.Write (bufferLength,0,8);

			//将指针移到最后
			target.Seek (0,SeekOrigin.End);
			return 0;
		}
*/



		//将一条记录及包含的多个资源文件打包
		//应保证外部把target的位置定好
		//0:成功
		//-1:出错
		public static int DaBag(string strXmlText,
			ArrayList aFileItem,
			Stream target,
			out string strInfo)
		{
			strInfo = "";

			XmlDocument dom = new XmlDocument ();
			try
			{
				dom.LoadXml(strXmlText);
			}
			catch(Exception ex )
			{
				strInfo += "不合法的XML\r\n"+ex.Message ;
				return -1;
			}

			long lTotalLength = 0;  //内容总长度，不包括开头的8个字节
			//长度字节数组
			byte[] bufferLength = new byte[8];

			//记住写总长度的位置
			long lPositon = target.Position ;

			//1.开头空出8字节，最后写总长度*****************
			target.Write(bufferLength,0,8);

			//2.写XMl文件*******************
			MemoryStream ms = new MemoryStream ();
			dom.Save (ms);

			//将字符串转换成字符数组
			//byte[] bufferXmlText = System.Text.Encoding.UTF8.GetBytes(strXmlText);
			
			//算出XML文件的字节数
			long lXmlLength = ms.Length  ;//(long)bufferXmlText.Length;
			bufferLength =	System.BitConverter.GetBytes(lXmlLength);
			
			target.Write(bufferLength,0,8);
			lTotalLength += 8;

			//target.Write (bufferXmlText,0,lXmlLength);
			ms.Seek (0,SeekOrigin.Begin );
			StreamUtil.DumpStream (ms,target);
			lTotalLength += lXmlLength ;

			//3.写文件
			long lFileLengthTotal = 0;  //全部文件的长度,也可以继续用lTotalLength，但新申请一个变量出问题的情况更小
			
			FileItem fileItem = null;
			for(int i=0;i<aFileItem.Count ;i++)
			{
				fileItem = (FileItem)aFileItem[i];
				//MessageBox.Show (fileItem.strClientPath + " --- " + fileItem.strItemPath + " --- " + fileItem.strFileNo  );
			
				FileStream streamFile = File.Open (fileItem.strClientPath ,FileMode.Open);
				WriteFile(streamFile,
					fileItem.strFileNo ,
					target,
					ref lFileLengthTotal);
				streamFile.Close ();
			}

			lTotalLength += lFileLengthTotal;

			//4.写总长度
			bufferLength = System.BitConverter.GetBytes(lTotalLength);
			target.Seek (lPositon,SeekOrigin.Begin);
			target.Write (bufferLength,0,8);

			//将指针移到最后
			target.Seek (0,SeekOrigin.End);
			return 0;
		}



		//写子文件数据	注.外部保证把位置移好
		//source: 二进制流
		//strID: 记录ID
		//target: 目标流
		//lFileLengthTotal: 文件总长度
		//0: 正常得到文件内容 -1:文件名为空
		public static int WriteFile(Stream source,
			string strID,
			Stream target,
			ref long lFileLengthTotal)
		{
			long lTotalLength = 0;  //总长度
			//长度字节数组
			byte[] bufferLength = new byte[8];

			//记住写总长度的位置
			long lPosition = target.Position ;

			//1.开头空出8字节，最后写总长度*****************
			target.Write(bufferLength,0,8);

			//2.先写名称字符串的长度;
			//将字符串转换成字符数组
			byte[] bufferID = System.Text.Encoding.UTF8 .GetBytes(strID);
			bufferLength = System.BitConverter.GetBytes((long)bufferID.Length);
			target.Write (bufferLength,0,8);
			lTotalLength += 8;

			//3.写名称字符串
			target.Write (bufferID,
				0,
				bufferID.Length );
			lTotalLength += bufferID.Length;

			//4.写二进制文件
			bufferLength = System.BitConverter.GetBytes(source.Length);
			//二进制文件的长度;
			target.Write (bufferLength,0,8);
			lTotalLength += 8;
			//写二进制文件内容
			source.Seek (0,SeekOrigin.Begin);
			StreamUtil.DumpStream (source,
				target);
			lTotalLength += source.Length ;


			//5.返回开头写总长度
			bufferLength =	System.BitConverter.GetBytes(lTotalLength);
			target.Seek (lPosition,SeekOrigin.Begin);
			target.Write (bufferLength,0,8);

			//将指针移到最后
			target.Seek (0,SeekOrigin.End);
			lFileLengthTotal += (lTotalLength+8);
			return 0;
		}



		/*
		//将一个包文件分成范围集合
		public static int SplitFile2FragmentList(string strClientFilePath,
			FragmentList fragmentList)
		{
			FileInfo fi = new FileInfo(strClientFilePath);
			string[] aRange = null;
			string strErrorInfo;

			if (fi.Length == 0)
				return -1;

			int nPackageMaxSize = 500*1024;
			if (fi.Length > nPackageMaxSize) 
			{
				string strRangeWhole = "0-" + Convert.ToString(fi.Length-1);
				aRange = RangeList.ChunkRange(strRangeWhole, nPackageMaxSize);
			}

			if (aRange != null) 
			{
				for(long i=0; i<aRange.Length; i++) 
				{
					string strContentRange = aRange[i];

					FragmentItem fragmentItem = fragmentList.newItem(
						strClientFilePath,
						strContentRange,
						false,    //是否立即创建临时文件? true:表示立即复制临时文件;false:不复制,建议不要立刻复制，等到发包时再复制
						out strErrorInfo);

					if (fragmentItem == null)
						return -1 ;
				}
			}
			else 
			{
				// 文件尺寸没有超过包尺寸限制
				FragmentItem fragmentItem = fragmentList.newItem(
					strClientFilePath,
					"",	// 空字符串表示文件中全部内容进入
					false,
					out strErrorInfo);
				if (fragmentItem == null)
					return -1;
			}
			return 0;
		}
		*/

	}

}
