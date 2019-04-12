using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Runtime.Serialization;


namespace DigitalPlatform.rms
{
	public class rmsUtil
	{
#if NO
		// 将片断流(sourceStream)中全部内容根据contentrange字符串定义的位置
		// 还原复制到目标文件(strOriginFileName)中
		// 也就是说,contentrange字符串实际上定义的是从目标文件抽取到片断的规则
		// 当strContentRange的值为""时，表示复制整个文件
		// paramter:
		//		streamFragment:    片断流
		//		strContentRange:   片断流在文件中存在的位置
		//		strOriginFileName: 目标文件
		//		strError:          out 参数,return error info
		// return:
		//		-1  出错
		//		>=  实际复制的总尺寸
		public static long RestoreFragment(
			Stream streamFragment,
			string strContentRange,
			string strOriginFileName,
			out string strErrorInfo)
		{
			long lTotalBytes = 0;
			strErrorInfo = "";

			if (streamFragment.Length == 0)
				return 0;

			// 表示范围的字符串为空，恰恰表示要包含全部范围
			if (strContentRange == "") 
			{
				strContentRange = "0-" + Convert.ToString(streamFragment.Length - 1);
			}

			// 创建RangeList，便于理解范围字符串
			RangeList rl = new RangeList(strContentRange);

			FileStream fileOrigin = File.Open(
				strOriginFileName,
				FileMode.OpenOrCreate, // 原来是Open，后来修改为OpenOrCreate。这样对临时文件被系统管理员手动意外删除(但是xml文件中仍然记载了任务)的情况能够适应。否则会抛出FileNotFoundException异常
				FileAccess.Write,
				FileShare.ReadWrite);


			// 循环，复制每个连续片断
			for(int i=0; i<rl.Count; i++) 
			{
				RangeItem ri = (RangeItem)rl[i];

				fileOrigin.Seek(ri.lStart,SeekOrigin.Begin);
				StreamUtil.DumpStream(streamFragment, fileOrigin, ri.lLength);

				lTotalBytes += ri.lLength;
			}

			fileOrigin.Close();
  			return lTotalBytes;
		}
#endif

		// 得到传和的字符串，组合返回一个文件名字符串
		public static string makeFilePath(string strDir,
			string strPrefix,
			string strFileName)
		{
			string strResult = "";
			strPrefix = strPrefix.Replace("?","_");
			strResult = strDir + "~" + strPrefix + "_" + strFileName;
			return strResult;
		}
	}

#if NO
	//表示文件片断的类
	public class FragmentItem
	{
		public string strClientFilePath = "";	// 所从属的前端文件名
		public string strContentRange = "";		// 所对应的片断范围定义
		public string strTempFileName = "";		// 临时文件名

    // TODO: 容易造成 mem leak
		~FragmentItem()
		{
			DeleteTempFile();
		}


		// 删除临时文件
		public void DeleteTempFile()
		{
			if (strTempFileName != "") 
			{
				File.Delete(strTempFileName);
				strTempFileName = "";
			}
		}


		public long Copy(out string strErrorInfo)
		{
			if (strClientFilePath == "") 
			{
				strErrorInfo = "strClientFilePath参数为空...";
				return -1;
			}
			if (strTempFileName == "") 
			{
				// 获得临时文件名
				strTempFileName = Path.GetTempFileName();
			}

			//MessageBox.Show ("临时文件名:"+strTempFileName);
			return RangeList.CopyFragment(
				strClientFilePath,
				strContentRange,
				strTempFileName,
				out strErrorInfo);
		}


		//得到临时文件的长度
		public long GetTempFileLength()
		{
			if (strTempFileName == "")
				return -1;
			FileInfo fi = new FileInfo(strTempFileName);
			return fi.Length;
		}


		// 获得本片断的总尺寸
		public long lengthOf()
		{
			if (strContentRange == "") 
			{
				if (strClientFilePath == "")
					return -1;	// 表示非法值
				FileInfo fi = new FileInfo(strClientFilePath);
				return fi.Length;
			}
			return lengthOf(strContentRange);
		}


		// 把一个contentrange字符串翻译为总尺寸
		public static long lengthOf(string strContentRange)
		{
			long lTotalBytes = 0;

			// 创建RangeList，便于理解范围字符串
			RangeList rl = new RangeList(strContentRange);
			// 循环，复制每个连续片断
			for(int i=0; i<rl.Count; i++) 
			{
				RangeItem ri = (RangeItem)rl[i];

				lTotalBytes += ri.lLength;
			}
			return lTotalBytes;
		}
	} 

	//FragmentItem的集合
	public class FragmentList : ArrayList 
	{
		//创建一个新的FragmentItem对象，并加入集合
		//如果发现strClientFilePath和ContentRange参数和集合中已经存在的Item相同，则返回错误
		//strClientFilePath: 文件名
		//ContentRange: 范围
		//bCreateTempFile: 是否立即创建临时文件
		//strErrorInfo: 错误信息
		public FragmentItem newItem(string strClientFilePath,
			string strContentRange,
			bool bCreateTempFile,
			out string strErrorInfo)
		{
			strErrorInfo = "";

			FragmentItem fi = new FragmentItem();

			fi.strClientFilePath = strClientFilePath;
			fi.strContentRange = strContentRange;
			if (bCreateTempFile == true)
			{
				long ret = fi.Copy(out strErrorInfo);
				if (ret == -1)
					return null;
			}

			this.Add(fi);

			return fi;
		}
	} 

#endif

#if NO
	// FileNameHolder 里面的对象的类型为 FileNameItem
	public class FileNameHolder:ArrayList
	{
		//临时目录地址
		public string m_strDir;           

		//前缀，现在为sessionID + recordID
		public string m_strPrefix;
  
		//调试信息
		public string strFileNameHolderInfo = "";

		//公共Dir属性，临时目录地址
		public string Dir
		{
			get
			{
				return m_strDir;
			}
			set
			{
				m_strDir = value;
			}
		}

		//公共Prefix属性，表示前缀
		public string Prefix
		{
			get
			{
				return m_strPrefix;
			}
			set
			{
				m_strPrefix = value;
			}
		}

		//LeaveFiles调用Clear()函数清除所有对象，
		//那么在析构时就不会删除对象了，将所有权将给xmledit.
		public void LeaveFiles()
		{
			Clear();
		}

		//先物理删除所有的对象，再清空集合。
		//成功返回0
		public int DeleteAllFiles()
		{
			foreach(FileNameItem objFileName in this)
			{
				try
				{
					File.Delete(m_strDir + "~" + m_strPrefix + "_" + objFileName.FileName);
				}
				catch (Exception ex)
				{
					//Exception ex = new Exception("在DeleteAllFiles里删除了" + objFileName.FileName + "文件失败");
					throw(ex);
					//strFileNameHolderInfo +="在DeleteAllFiles里删除了"+objFileName.FileName+"文件失败";
				}
			}
			Clear();
			return 0;
		}

		//列出集合的中的所有项
		//表格字符串
		public string Dump()
		{
			string strResult = "";
			strResult += "<table border='1'><tr><td>文件名</td></tr>";

			foreach(FileNameItem objFileName in this)
			{
				strResult += "<tr><td>" + m_strDir + "~" + m_strPrefix + "_" + objFileName.FileName + "</td></tr>";
			}		
			strResult += "</table>";

			return strResult;
		}

    // TODO: 可能会造成 mem leak
		//析构函数删除所有对象
		~FileNameHolder()
		{
			DeleteAllFiles();
		}
	} //FileNameHolder类结束


	//FileNameHolder的成员类型
	public class FileNameItem
	{
		//文件名称
		private string m_strFileName;

		//文件类型
		private string m_strContentType;

		//构造函数包含一个参数:strFileName,表示对象文件名，赋值给m_strFileName
		//strFileName: 文件名
		public FileNameItem(string strFileName)
		{
			m_strFileName = strFileName;
		}

		//文件名
		public string FileName
		{
			get
			{
				return m_strFileName;
			}
		}

		//文件类型
		public string ContentType
		{
			get
			{
				return m_strContentType;
			}
			set
			{
				m_strContentType = value;
			}
		}
	} //FileNameItem类结束
#endif

	// 设计意图:为了处理"数据库名:记录ID"以及ID长度设计的DbPath类
	public class DbPath
	{
		//私有成员字段，存放数据库名称
		private string m_strName = "";
		
		//私有成员字段，存放记录ID
		private string m_strID = "";

#if NO
		//构造函数:将传入的逻辑ID拆分为两部分：数据库名和记录ID，分别赋值给m_strName和m_strID
		//strDpPath: 传入的组合形式
		public DbPath(string strDbPath)
		{
			int nPosition = strDbPath.LastIndexOf ("/"); //:
			//只不存在/号时，只有一个库名ID
			if (nPosition < 0)
			{
//				m_strID = strDbPath;
				this.m_strName = strDbPath;
				return;
			}
			m_strName = strDbPath.Substring(0,nPosition);
			m_strID = strDbPath.Substring(nPosition+1);
		}
#endif

        //构造函数:将传入的逻辑ID拆分为两部分：数据库名和记录ID，分别赋值给m_strName和m_strID
        //strDpPath: 传入的组合形式
        public DbPath(string strDbPath)
        {
            int nPosition = strDbPath.IndexOf("/"); // 2015/11/14
            //只不存在/号时，只有一个库名ID
            if (nPosition < 0)
            {
                //				m_strID = strDbPath;
                this.m_strName = strDbPath;
                return;
            }
            m_strName = strDbPath.Substring(0, nPosition);
            m_strID = strDbPath.Substring(nPosition + 1);
        }

		//公共Name属性，表示数据库名，提供给外部代码访问
		public string Name
		{
			get
			{
				return m_strName;
			}
			set
			{
				m_strName=value;
			}
		}

		//公共ID属性，表示记录ID，提供给外部代码访问
		public string ID
		{
			get
			{
				return m_strID;
			}
			set
			{
				m_strID = value;
			}
		}

		//公共Path属性，表示完整逻辑ID（包括库名和记录ID），只读
		public string Path
		{
			get
			{
				return m_strName + "/" + ID10; //:
			}
		}

		//公共ID10属性，返回一个10位长度的记录ID
		public string ID10
		{
			get
			{
				return this.m_strID.PadLeft(10,'0');
/*
				if (m_strID.Length < 10)
				{
					string strAdd = new string('0',10-m_strID.Length);
					return strAdd + m_strID;
				}
				
				return m_strID;
*/				
			}
		}


		// 确保 ID 字符串为 10 位数字形态
		public static string GetID10(string strID)
		{
			return strID.PadLeft(10, '0');	
		}

		// 获得去掉了前方 '0' 的短号码 ID 字符串
		public string CompressedID
		{
			get
			{
                return GetCompressedID(m_strID);

				// return m_strID.TrimStart(new char[]{'0'});
/*
				string strTemp = m_strID;
				while(strTemp.Substring(0,1) == "0")
				{
					strTemp = strTemp.Substring(1);
				}
				return strTemp;
*/				
			}
		}

		public static string GetCompressedID(string strID)
		{
			return strID.TrimStart(new char[]{'0'});
/*
			while(strID.Substring(0,1) == "0")
			{
				strID = strID.Substring(1);
			}
			return strID;
*/			
		}
	}

    public class LogicNameItem
    {
        [DataMember]
        public string Lang = "";
        [DataMember]
        public string Value = "";
    }
}
