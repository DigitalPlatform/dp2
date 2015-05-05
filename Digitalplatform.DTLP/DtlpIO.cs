using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.DTLP
{
	/// <summary>
	/// 负责dtlp记录批量出入的接口类
	/// </summary>
	public class DtlpIO
	{
        DtlpChannelArray Channels = null;
		DtlpChannel channel = null;	// DTLP通道
		// public IWin32Window owner = null;

		public string	m_strStartNumber = "";
		public string	m_strEndNumber = "";
		string	m_strNextNumber = "";			// 即将处理的下一条记录的记录号
		public string	m_strCurNumber = "";			// 已经处理完的当前记录的记录号

		public string	m_strDBPath = "";

		public string	m_strRecord = "";
		public byte[]	m_baTimeStamp = new byte[9];

		public string	m_strPath = "";	// 当前记录路径

		public int ErrorNo = 0;

		public DtlpIO()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// 初始化本对象
		// 从外部给的lChannel，在使用完后一定要设置为-1，否则会导致多次Destroy()
		public int Initial(DtlpChannelArray channels,
			string strDBPath,
			string strStartNumber,
			string strEndNumber)
		{
            Debug.Assert(channels != null, "channels参数不能为null");

            this.Channels = channels;
			this.channel = this.Channels.CreateChannel(0);

			// Debug.Assert(channel != null, "channel参数不能为null");


			/*
			pConnect->m_strDefUserName = strDefUserName;
			pConnect->m_strDefPassword = strDefPassword;
			*/

			Debug.Assert(strStartNumber != "",
				"strStartNumber参数不能为空");
			m_strStartNumber = strStartNumber;

			Debug.Assert(strEndNumber != "",
				"strEndNumber参数不能为空");
			m_strEndNumber = strEndNumber;

			Debug.Assert(strDBPath != "",
				"strDBPath参数不能为空");
			m_strDBPath = strDBPath;			// 数据库路径

			return 0;
		}

		// 得到下一条记录
		// return:
		//		-1	出错
		//		0	继续
		//		1	到达末尾(超过m_strEndNumber)
		//		2	没有找到记录
		public int NextRecord(ref int nRecCount,
            out string strError)
		{
            strError = "";

			string strPath;
			string strNumber;
			string strNextNumber;
			byte[] baPackage = null;
		
			int nSearchStyle = DtlpChannel.XX_STYLE;
			int nRet;
			int nErrorNo;
			int nDirStyle = 0;	// 方向风格
			byte[] baMARC = null;

            if (this.channel == null)
                this.channel = this.Channels.CreateChannel(0);

			Debug.Assert(m_strStartNumber != "",
				"m_strStartNumber值不能为空");
			Debug.Assert(m_strEndNumber != "",
				"m_strEndNumber值不能为空");

			// 首次进入本函数
			if (nRecCount == -1) 
			{
				strNumber = m_strStartNumber;

				nDirStyle = 0;
				nRecCount = 0;
			}
			else 
			{
				strNumber = m_strNextNumber;

				if (Convert.ToInt64(m_strStartNumber) <= Convert.ToInt64(m_strEndNumber)) 
				{
					nDirStyle = DtlpChannel.NEXT_RECORD;
					if (Convert.ToInt64(strNumber) >= Convert.ToInt64(m_strEndNumber))
						return 1;	// 结束
				}
				else 
				{
					nDirStyle = DtlpChannel.PREV_RECORD;
					if (Convert.ToInt64(strNumber) <= Convert.ToInt64(m_strEndNumber))
						return 1;	// 结束
				}
			}


			strPath = m_strDBPath;
			strPath += "/ctlno/";
			strPath += strNumber;

	//		REDO:
				nRet = channel.Search(strPath,
					nSearchStyle | nDirStyle,
					out baPackage);
			if (nRet == -1) 
			{
				nErrorNo  = channel.GetLastErrno();
				if (nErrorNo == DtlpChannel.GL_NOTLOGIN) 
				{
					// 重新登录的事情，已经被Search()接管了
				}
				else 
				{
					if (nErrorNo == DtlpChannel.GL_NOTEXIST) 
					{
						// 增量号码
						Int64 n64Number = Convert.ToInt64(strNumber);
						string strVersion;

						// GetDTLPVersion(m_strDBPath, strVersion);	// 可以将库名再去除，效果更好
						strVersion = "0.9";
				
						if (n64Number+1<9999999
							&& strVersion == "0.9") 
						{
							//strNumber.Format(_T("%I0764d"), n64Number+1);
							// 确保7位数字
							strNumber = String.Format("{0:D7}", n64Number+1);   // "{0,7:D}" BUG!!! 左边实际上是空格 2009/2/26
						}
						else 
						{
							// strNumber.Format(_T("%I64d"), n64Number+1);
                            strNumber = String.Format("{0:D7}", n64Number + 1);  // "{0,7:D}" BUG!!! 左边实际上是空格 2009/2/26
						}


						m_strCurNumber = strNumber;
						goto NOTFOUND;
					}

					this.ErrorNo = nErrorNo;

					if (nErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
						this.channel = null;

					// 得到字符串即可
                    /*
					channel.ErrorBox(owner, "DtlpIO",
						"检索发生错误\nSearch() error");
                     * */
                    strError = "检索发生错误\nSearch() error: \r\n" + channel.GetErrorDescription();
					goto ERROR1;
				}

				m_strPath = strPath;
	
				goto ERROR1;
			}


			/// 
			Package package = new Package();

			package.LoadPackage(baPackage, channel.GetPathEncoding(strPath));
			nRet = package.Parse(PackageFormat.Binary);
			if (nRet == -1) 
			{
				Debug.Assert(false, "Package::Parse() error");
                strError = "Package::Parse() error";
				goto ERROR1;
			}

			nRet = package.GetFirstBin(out baMARC);
			if (nRet == -1 || nRet == 0) 
			{
				Debug.Assert(false, "Package::GetFirstBin() error");
                strError = "Package::GetFirstBin() error";
				goto ERROR1;
			}

			if (baMARC.Length >= 9) 
			{
				Array.Copy(baMARC, 0, m_baTimeStamp, 0, 9);

				byte [] baBody = new byte[baMARC.Length - 9];
				Array.Copy(baMARC, 9, baBody, 0, baMARC.Length - 9);
				baMARC = baBody;

				//baMARC.RemoveAt(0, 9);	// 时间戳
			}
			else 
			{
				// 记录有问题，放入一个空记录?
			}

			// ---????????? 编写一个在byte[]末尾追加东西的函数?

			// baMARC = ByteArray.Add(baMARC, (byte)0);
			// baMARC.Add(0);
			// baMARC.Add(0);

			m_strRecord = channel.GetPathEncoding(strPath).GetString(baMARC);

			strPath = package.GetFirstPath();

			nRet = GetCtlNoFromPath(strPath,
				out strNextNumber);
			if (nRet == -1) 
			{
				Debug.Assert(false, "GetCtlNoFromPath() return error ...");
				strError = "GetCtlNoFromPath() error ...";
				goto ERROR1;
			}

			m_strNextNumber = strNextNumber;

			if (nRecCount == 0)	// 首次
				m_strCurNumber = strNumber;
			else
				m_strCurNumber = strNextNumber;

			m_strPath = strPath;	// new add

			nRecCount ++;
			return 0;
			ERROR1:
				return -1;
			NOTFOUND:
				return 2;
		}

        // 增量当前号码，以便继续获取后面的记录
        // 2009/2/26
        public void IncreaseNextNumber()
        {
            // 增量号码
            string strNumber = this.m_strNextNumber;

            Int64 n64Number = Convert.ToInt64(strNumber);
            string strVersion;

            // GetDTLPVersion(m_strDBPath, strVersion);	// 可以将库名再去除，效果更好
            strVersion = "0.9";

            if (n64Number + 1 < 9999999
                && strVersion == "0.9")
            {
                //strNumber.Format(_T("%I0764d"), n64Number+1);
                // 确保7位数字
                strNumber = String.Format("{0:D7}", n64Number + 1);
            }
            else
            {
                // strNumber.Format(_T("%I64d"), n64Number+1);
                strNumber = String.Format("{0:D7}", n64Number + 1);
            }

            this.m_strNextNumber = strNumber;
        }

		// 得到从右面开始的第一个'/'以右的部分
		// return:
		//		-1	error
		//		其他 strRightPart的长度
		static int GetCtlNoFromPath(string strPath,
			out string strRightPart)
		{
			Debug.Assert(strPath != null, "strPath参数不能为null");

			string strTemp = strPath;
			int nRet;
	
			strRightPart = "";
			nRet = strTemp.LastIndexOf('/');
			if (nRet == -1)
				return -1;

			strTemp = strTemp.Substring(nRet+1);

			strRightPart = strTemp;

			return strRightPart.Length;
		}


		// 校准转出范围的首尾号
		// return:
		//		-1	出错
		//		0	没有改变首尾号
		//		1	校准后改变了首尾号
		//		2	书目库中没有记录
		public int VerifyRange(out string strError)
		{
            strError = "";

			string strPath;
			byte[] baPackage;

			string strMinNumber;
			string strMaxNumber;
			int nRet;
			int style = DtlpChannel.JH_STYLE;
			int nErrorNo;

			bool bChanged = false;
			string strVersion;

            if (this.channel == null)
                this.channel = this.Channels.CreateChannel(0);

			Debug.Assert(m_strStartNumber != "",
				"m_strStartNumber值不能为空");
			Debug.Assert(m_strEndNumber != "",
				"m_strEndNumber值不能为空");

			// 校准最大号
			//REDO1:
				strPath = m_strDBPath;

			// GetDTLPVersion(m_strDBPath, strVersion);	// 可以将库名再去除，效果更好
			strVersion = "0.9";

			if (strVersion == "0.9")
				strPath += "/ctlno/9999999";	// 7位
			else
				strPath += "/ctlno/9999999999";	// 10位

			nRet = channel.Search(strPath,
				style | DtlpChannel.PREV_RECORD,
				out baPackage);
			if (nRet <= 0) 
			{
				nErrorNo  = channel.GetLastErrno();
				this.ErrorNo = nErrorNo;

				if (nErrorNo == DtlpChannel.GL_NOTEXIST)
					return 2;

				if (nErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
					this.channel = null;

                /*
				channel.ErrorBox(owner,
                    "dp1Batch",
                    "校准最大号时发生错误\nSearch() style=PREV_RECORD error");
                 * */
                strError = "校准最大号时发生错误\nSearch() style=PREV_RECORD error: \r\n" + channel.GetErrorDescription();
				goto ERROR1;
			}

			/// 
			Package package = new Package();

			package.LoadPackage(baPackage, channel.GetPathEncoding(strPath));
			nRet = package.Parse(PackageFormat.Binary);
			if (nRet == -1) 
			{
                strError = "Package::Parse(PackageFormat.Binary) error";
				Debug.Assert(false, strError);
				goto ERROR1;
			}

			strPath = package.GetFirstPath();

			nRet = GetCtlNoFromPath(strPath,
				out strMaxNumber);
			if (nRet == -1) 
			{
                strError = "GetCtlNoFromPath() error ...";
				Debug.Assert(false, strError);
				goto ERROR1;
			}

            // 2007/8/18 new add
            if (strVersion == "0.9")
                strMaxNumber = strMaxNumber.PadLeft(7, '0');	// 7位
            else
                strMaxNumber = strMaxNumber.PadLeft(10, '0');	// 10位


			if (Convert.ToInt64(m_strStartNumber) <= Convert.ToInt64(m_strEndNumber)) 
			{
				if (Convert.ToInt64(m_strEndNumber) > Convert.ToInt64(strMaxNumber)) 
				{
					m_strEndNumber = strMaxNumber;
					bChanged = true;
				}
			}
			else 
			{
				if (Convert.ToInt64(m_strStartNumber) > Convert.ToInt64(strMaxNumber)) 
				{
					m_strStartNumber = strMaxNumber;
					bChanged = true;
				}
			}


			// 校准最小号
			//REDO2:
				strPath = m_strDBPath;
			strPath += "/ctlno/0000000000";	// -

			nRet = channel.Search(strPath,
				style | DtlpChannel.NEXT_RECORD,
				out baPackage);
			if (nRet <= 0) 
			{
				nErrorNo = channel.GetLastErrno();

				this.ErrorNo = nErrorNo;

				if (nErrorNo == DtlpChannel.GL_NOTEXIST)
					return 2;
				if (nErrorNo == DtlpChannel.GL_INVALIDCHANNEL)
					this.channel = null;

                /*
				channel.ErrorBox(owner,
					"Batch",
					"校准最小号时发生错误\nSearch() style=NEXT_RECORD error");
                 * */
                strError = "校准最小号时发生错误\nSearch() style=NEXT_RECORD error: \r\n" + channel.GetErrorDescription();
				goto ERROR1;

			}

			/// 
			package.LoadPackage(baPackage, channel.GetPathEncoding(strPath));
			nRet = package.Parse(PackageFormat.Binary);
			if (nRet == -1) 
			{
                strError = "Package::Parse(PackageFormat.Binary) error";
				Debug.Assert(false, strError);
				goto ERROR1;
			}

			strPath = package.GetFirstPath();

			nRet = GetCtlNoFromPath(strPath,
				out strMinNumber);
			if (nRet == -1) 
			{
                strError = "GetCtlNoFromPath() error ...";
				Debug.Assert(false, strError);
				goto ERROR1;
			}

            // 2007/8/18 new add
            if (strVersion == "0.9")
                strMinNumber = strMinNumber.PadLeft(7, '0');	// 7位
            else
                strMinNumber = strMinNumber.PadLeft(10, '0');	// 10位


			if (Convert.ToInt64(m_strStartNumber) <= Convert.ToInt64(m_strEndNumber)) 
			{
				if (Convert.ToInt64(m_strStartNumber) < Convert.ToInt64(strMinNumber)) 
				{
					m_strStartNumber = strMinNumber;
					bChanged = true;
				}
			}
			else 
			{
				if (Convert.ToInt64(m_strEndNumber) < Convert.ToInt64(strMinNumber)) 
				{
					m_strEndNumber = strMinNumber;
					bChanged = true;
				}
			}

			if (bChanged == true)
				return 1;
			else
				return 0;

			ERROR1:
				return -1;
		}

	}
}
