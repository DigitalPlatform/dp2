using System;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using DigitalPlatform.Core;

namespace DigitalPlatform.DTLP
{
	/// <summary>
	/// 一个tcps通讯通道
	/// </summary>
	public class DtlpChannel
	{

		#region DTLP API Definition
		// --------------------------------
		// DTLP API 错误码
		public const int GL_INVALIDPATH      = 0x0001;
		public const int GL_OUTOFRANGE       = 0x0003;
		public const int GL_INVALIDCHANNEL   = 0x0005;

		public const int GL_HANGUP           = 0x0021;
		public const int GL_NORESPOND        = 0x0023;
		public const int GL_NEEDPASS         = 0x0025;

		public const int GL_ACCESSDENY       = 0x0033;
		public const int GL_RAP              = 0x0035;
		public const int GL_NOTEXIST         = 0x0037;
		public const int GL_NOMEM            = 0x0039;
		public const int GL_NOCHANNEL        = 0x003B;

		public const int GL_ERRSIGNATURE     = 0x003D;
		public const int GL_NOTLOGIN         = 0x003F;

		public const int GL_OVERFLOW         = 0x0041;
		public const int GL_CONNECT          = 0x0042;
		public const int GL_SEND             = 0x0043;
		public const int GL_RECV             = 0x0044;
		public const int GL_PARATOPACKAGE    = 0x0045;
		public const int GL_PACKAGETOPARA    = 0x0046;
		public const int GL_PACKAGENTOH      = 0x0047;     // 在解释包时ntoh发生错误
		public const int GL_REENTER          = 0x0048;
		public const int GL_INTR             = 0x0049;      // TCP/IP操作被中断


		//
		public const int ReservedPaths       = 3;
		/* newapi access attribute */
		public const int AttrIsleaf          = 0x00000001;
		public const int AttrSearch          = 0x00000002;
		public const int AttrWildChar        = 0x00000004;
		public const int AttrExtend          = 0x00000008;
		public const int AttrTcps            = 0x00000010;
		public const int AttrRdOnly          = 0x00001000;
		/* newapi type attribute   */
		public const int TypeStdbase         = 0x00010000;
		public const int TypeSmdbase         = 0x00020000;
		public const int TypeStdfile         = 0x00040000;
		public const int TypeCfgfile         = 0x00080000;
		public const int TypeFrom            = 0x00100000;
		public const int TypeKernel          = 0x00200000;
		public const int TypeHome            = 0x00400000;
		public const int TypeBreakPoint      = 0x00800000;
		public const int TypeCdbase          = (TypeStdbase | AttrRdOnly);

		public const int TypeServerTime      =	0x01000000;

		//
		public const int FUNC_CREATECHANNEL  = 0x2100;
		public const int FUNC_DESTROYCHANNEL = 0x2300;
		public const int FUNC_CHDIR          = 0x2500;
		public const int FUNC_DIR            = 0x2700;
		public const int FUNC_SEARCH         = 0x2900;
		public const int FUNC_WRITE          = 0x2B00;
		public const int FUNC_GETLASTERRNO   = 0x2D00;
		public const int FUNC_ACCMANAGEMENT  = 0xD100;

		/* ; dbnames                 0x3*00   */
		public const int FUNC_GETAVAILABLEDBS = 0x3100;
		public const int FUNC_DBINIT         = 0x3200;
		public const int FUNC_DBOPEN         = 0x3300;
		public const int FUNC_DBCLOSE        = 0x3500;
		public const int FUNC_GETFROM        = 0x3700;
		public const int FUNC_GETLASTNUMBER  = 0x3900;
		public const int FUNC_MODSUBASE      = 0x3B00;
		public const int FUNC_DELSUBASE      = 0x3D00;
		/* errno */
		public const int DB_NOTEXIST         = 0x3001;
		public const int DB_LOCKED           = 0x3003;
		public const int DB_CONFLICT         = 0x3005;

		/* ; simple dbase            0x4*00   */
		public const int FUNC_SMALLDBASE     = 0x4100;
		public const int FUNC_SMDBINIT       = 0x4300;
		public const int FUNC_SMWRITERECORD  = 0x4500;
		public const int FUNC_SMGETRECORD    = 0x4700;
		public const int FUNC_SMDELETERECORD = 0x4900;

		/* ; search                  0x5*00   */
		public const int FUNC_HITRECORDNUMS  = 0x5100;

		/* ; get records             0x7*00   */
		public const int FUNC_GETRECORDS     = 0x7100;
		public const int FUNC_GETNEXTRECORD  = 0x7300;
		/* errno  */
		public const int GR_PARTMISSING      = 0x7001;
		public const int GR_PARTLOCKED       = 0x7002;
		public const int GR_PARTRDDISABLE    = 0x7004;

		/* ; write records           0x9*00   */
		public const int FUNC_WRITERECORD    = 0x9100;
		/* errno  */
		public const int WR_ACCOUNTFULL      = 0x9001;
		public const int WR_WTDISABLE        = 0x9003;
		public const int WR_LOCKED           = 0x9005;

		/* ; lock records            0xA*00   */
		public const int FUNC_LOCKRECORD     = 0xA100;
		public const int FUNC_LOGICLOCK      = 0xA300;
		/* errno  */
		public const int LR_OVERFLOW         = 0xA001;
		public const int LR_CONFLICT         = 0xA003;
		public const int LR_NOTEXIST         = 0xA005;

		/* ; delete records          0xB*00   */
		public const int FUNC_DELETERECORD   = 0xB100;
		/* errno  */
		public const int DR_NOTEXIST         = 0xB001;
		public const int DR_LOCKED           = 0xB003;

		/* ; account management      0xD*00   */
		public const int FUNC_MANAGEMENT  = 0xD100;
		/* errno */
		public const int LG_BADNAME          = 0xD001;
		public const int LG_BADPASS          = 0xD003;
		public const int AS_NOTEXIST         = 0xD005;
		public const int AS_DUPLICATE        = 0xD007;
		public const int AS_FULL             = 0xD009;
		public const int AS_OVERDRAFT        = 0xD00B;

		/* ; file access             0xE*00   */
		public const int FUNC_OPENHOSTFILE   = 0xE100;
		public const int FUNC_CLOSEHOSTFILE  = 0xE300;
		public const int FUNC_GETHOSTFILE    = 0xE500;
		public const int FUNC_PUTHOSTFILE    = 0xE700;

		/* ; config file             0xF*00   */
		public const int FUNC_GETCONFIGNAME  = 0xF100;
		public const int FUNC_OPENCONFIG     = 0xF300;
		public const int FUNC_CLOSECONFIG    = 0xF500;
		public const int FUNC_GETENTRY       = 0xF700;
		public const int FUNC_PUTENTRY       = 0xF900;

		/* errno */
		public const int CS_NOTFOUND         = 0xF001;

		// ------------------------------------
		// 各种Search风格
		public const int JH_STYLE            = 0x0001;
		public const int Z3950_BRIEF_STYLE	 = 0x0002;	 
		public const int XX_STYLE            = 0x0003;
		public const int CTRLNO_STYLE        = 0x0005;
		public const int ISO_STYLE           = 0x0007;
		public const int WOR_STYLE           = 0x0009;
		public const int SEED_STYLE          = 0x000B;
		public const int KEY_STYLE           = 0x000D;
		public const int RIZHI_STYLE         = 0x000F;
		public const int SIGMSG_STYLE        = 0x0011;
		// 分离SearchStyle风格用的掩码
		public const int MASK_OF_STYLE		=	0x00FF;
		// 注:上述???_STYLE值的判断，要采用下面方式：
		// if ( (lStyle & MASK_OF_STYLE) == ???_STYLE) {...}
		// **不能**采用下面方式：
		// if (lStyle & ???_STYLE) {...}	// 这是错误的用法
		// -------------------------------------


		public const int PREV_RECORD         = 0x0100;
		public const int NEXT_RECORD         = 0x0200;
		public const int EXACT_RECORD        = 0x0400;
		public const int SAME_RECORD         = 0x0800;
		public const int FIRST_RECORD        = 0x1000;
		public const int LAST_RECORD         = 0x2000;
		public const int AMOUNT_RECORD       = 0x4000;
		public const int CONT_RECORD         = 0x8000;

		// ------------------------------------
		// 各种Write风格
		public const int APPEND_WRITE		=	0x0001;
		public const int REPLACE_WRITE		=	0x0003;
		public const int DELETE_WRITE		=	0x0005;
		public const int GETKEYS_WRITE		=	0x0007;
		public const int RIZHI_WRITE		=	0x0009;
		public const int REBUILD_WRITE		=	0x000B;
		public const int PATPASS_WRITE		=	0x000D;
		// 分离Write风格用的掩码
		public const int MASK_OF_WRITE		=	0x000F;
		// 注:上述???_WRITE值的判断，要采用下面方式：
		// if ( (lStyle & MASK_OF_WRITE) == ???_WRITE) {...}
		// **不能**采用下面方式：
		// if (lStyle & ???_WRITE) {...}	// 这是错误的用法
		// -------------------------------------

		public const int WRITE_NO_LOG		=	0x0100;	// (DTLP 1.0扩充) 不创建日志


		//
		#endregion

		public DtlpChannelArray Container = null;

		public bool PreferUTF8 = true;

		public const int CHARSET_DBCS = 0;
		public const int CHARSET_UTF8 = 1;
		//int	m_nDTLPCharset = CHARSET_DBCS;
		//bool m_bForceDBCS = true;

		public int	m_lUsrID = 0;

		int	    m_lErrno = 0;
		//ArrayList	m_baSendBuffer = null;// 发送请求用的缓冲区
		//ArrayList	m_baRecvBuffer = null;// 接收响应用的缓冲区

		// bool	m_bStop = false;			// 中断标记

		HostArray	m_HostArray = new HostArray();	// 主机数组,拥有

		int		m_nResponseFuncNum = 0;

		public int	m_nResultMaxLen = 60000;

		public DtlpChannel()
		{
		}

		public void InitialHostArray()
		{
			m_HostArray.InitialHostArray(this.Container.appInfo);
			m_HostArray.Container = this;
		}


		// 将通道准备就绪。
		// 如果主机事项没有，就创建并连接。如果尚未登录过，
		// 就进行登录。
		// 返回值：CHostEntry事项。从CHostEntry::m_lChannel中也可获得远程Channel
		//		NULL表示失败
		HostEntry PrepareChannel(string strHostName)
		{
			HostEntry	entry = null;
			int nErrorNo = 0;
			int nRet;

	
			// 寻找已有的事项
			entry = m_HostArray.MatchHostEntry(strHostName);
			if (entry == null) 
			{
				// 创建新事项并连接
				entry = new HostEntry();
				Debug.Assert(entry!=null, "new HostEntry Failed ...");
				if (entry == null)
					return null;
				m_HostArray.Add(entry);
				entry.Container = m_HostArray;

			}

			// 如果TCP/IP连接尚未建立
			if (entry.client == null) 
			{
				nRet = entry.ConnectSocket(strHostName,
					out nErrorNo);
				if (nRet == -1) 
				{
					// 需要建立出错码
					m_lErrno = GL_CONNECT; // 详细原因有域名不存在，connect()失败等
					return null;
				}
			}
			// 如果尚未向主机登录过
			if (entry.m_lChannel == -1L) 
			{
				int nHandle;
		
				nRet = entry.RmtCreateChannel(m_lUsrID);
				if (nRet == -1)
					return null;

				Debug.Assert(entry.m_lChannel != -1,
					"entry.m_lChannel值不正确");

				nHandle = nRet;

				// 字符集协商功能
				if (PreferUTF8 == true) 
				{
					byte [] baResult = null;
					string strPath = "";

					strPath = strHostName;
					// advstrPath += "/Initial/Charset/?";
					nRet = API_Management(strPath,
						"Initial",
						"Encoding/?",
						out baResult);
					if (nRet == -1) 
					{
						// 表明为DTLP 0.9，不支持字符集协商
						entry.m_nDTLPCharset = CHARSET_DBCS;
						// m_bForceDBCS = true;
					}
					else 
					{
						strPath = strHostName;
						// advstrPath += "/Initial/Charset/UTF8";
						nRet = API_Management(strPath,
							"Initial",
							"Encoding/UTF8",
							out baResult);
						if (nRet != -1) 
						{
							// 支持字符集切换为UTF8
							entry.m_nDTLPCharset = CHARSET_UTF8;
							// 从此以后就需要使用UTF8进行通讯了
							// m_bForceDBCS = false;

						}
					}
			
				}
				else 
				{
					// m_bForceDBCS = true;
				}




			}
	
			return entry;
		}



		// 得到错误码
		public int GetLastErrno()
		{
			return this.m_lErrno;
		}

		// 放弃正在进行的操作
		public bool Cancel()
		{
			// this.m_bStop = true;
	
			this.m_HostArray.CloseAllSockets();

			return true;
		}

		int GetLocalCharset()
		{
			if (IsHostActiveUTF8("") == true)
				return CHARSET_UTF8;
			return CHARSET_DBCS;
		}

		// 检测一个主机事项是不是utf-8编码
		public bool IsHostActiveUTF8(string strHostName)
		{
			if (strHostName == "") // 代表本级!
			{
				if (PreferUTF8 == true)
					return true;
				else 
					return false;
			}

			HostEntry	entry = null;
	
			// 寻找已有的事项
			entry = m_HostArray.MatchHostEntry(strHostName);
			if (entry == null) 
			{
				return false;	// not active
			}

			if (entry.m_nDTLPCharset == CHARSET_UTF8)
				return true;

			return false;
		}

		// 检测一个路径,发送/返回是否为utf-8编码
		// return:
		//		false	is not UTF8 (is DBCS)
		//		true	is UTF8
		public bool API_IsActiveUTF8(string strPath)
		{
			string strMyPath = null;
			string strOtherPath = null;

			//if (nHandle == 0 && nHandle == -1)
			//	return -1;

			SplitPath(strPath, out strMyPath, out strOtherPath);

			if (strMyPath == "") 
			{
				if (PreferUTF8)
					return true;
				return false;
			}

			return IsHostActiveUTF8(strMyPath);
		}

		// 把API_IsActiveUTF8()包装一下,便于使用
		public Encoding GetPathEncoding(string strPath)
		{
			if (API_IsActiveUTF8(strPath) == true)
				return Encoding.UTF8;
			return Encoding.GetEncoding(936);
		}


		// 出现汇报错误的对话框
		public void ErrorBox(IWin32Window owner,
			string strTitle,
			string strText)
		{
			int nErrorCode = GetLastErrno();

			string strError = GetErrorString(nErrorCode);

			// string strHex = String.Format("0X{0,8:X}",nErrorCode);
            string strHex = "0X" + Convert.ToString(nErrorCode, 16).PadLeft(8, '0');

			strError =	strText
				+ "\n----------------------------------\n错误码"
				+ strHex + ", 原因: "
				+ strError;

			MessageBox.Show(owner, strError, strTitle);
		}

        public string GetErrorDescription()
        {
            int nErrorCode = GetLastErrno();

            string strError = GetErrorString(nErrorCode);

            string strHex = "0X" + Convert.ToString(nErrorCode, 16).PadLeft(8, '0');

            return "错误码"
                + strHex + ", 原因: "
                + strError;
        }

		public string GetErrorString(int lErrno)
		{
			switch (lErrno) 
			{
				case GL_INTR:
					return "TCP/IP通讯中断 Communication Interrupted";
				case GL_INVALIDPATH:
					return "无效参数 Invalid Parameter";
				case GL_INVALIDCHANNEL:
					return "无效通道 Invalid Channel";
				case GL_REENTER:
					return "重入 Re-Enter";
				case GL_OUTOFRANGE:
					return "参数值超出有效范围 Parameter Out of Range";
				case GL_HANGUP:
					return "服务器已经挂起 Server Has Hang Up";
				case GL_NORESPOND:
					return "服务器没有响应 Server Not Respond ";
				case GL_ACCESSDENY:
					return "权限不够, 拒绝存取 Not Enough Right, Access Denied";
				case GL_RAP:
					return "需要反向求解 Need Reverse Path";
				case GL_NOTEXIST:
					return "不存在 Not Exist";
				case GL_NOMEM:
					return "返回参数预留空间不够 Not Enough Parameter Memory";
				case GL_NOCHANNEL:
					return "通道不够 Not Enough Channel";
				case GL_ERRSIGNATURE:
					return "Write()操作的时间戳不匹配 Error Write() Signature";
				case GL_NOTLOGIN:
					return "尚未登录 Not Login";
				case GL_OVERFLOW:
					return "溢出 Overflow";
				case GL_CONNECT:
					return "通讯连接失败 Connect Fail";
				case GL_SEND:
					return "通讯发送失败 Send Fail";
				case GL_RECV:
					return "通讯接收失败 Recieve Fail";
				case GL_PARATOPACKAGE:
					return "包转换失败 ParaToPackage Fail";
				case GL_NEEDPASS:
					return "需要鉴别信息 Need Authentication";
				case LG_BADNAME:
					return "错误的用户名 Bad Username";
				case LG_BADPASS:
					return "错误的口令字 Bad Password";
				case AS_NOTEXIST:
					return "帐户不存在 Account Not Exist";
				case AS_DUPLICATE:
					return "帐户重复 Account Duplicate";
				case AS_FULL:
					return "帐户已满 Account Full";
				case WR_WTDISABLE:
					return "不具备写权限 Not Allow Write";
				case DR_NOTEXIST:
					return "删除记录时原记录不存在 Record Not Exist";
				default:
				{
					return "未知错误 unknown error [" +  String.Format("0X{0,8:X}",lErrno) +"]";
				}
			}
		}

		// *** API ChDir
		public int API_ChDir(string strUserName,
			string strPassword,
			string strPath,
			//		int lPathLen, // 包含结尾的 0 字符
			out byte [] baResult)
		{
			// send:long Channel
			//      buff lpszUserName
			//      buff lpszPassword
			//      buff lpszPath
			//      long lPathLen
			//      long lResultMaxLen
			// recv:buff lpResult
			// return long
			baResult = null;
	
			DTLPParam param = new DTLPParam();
			int    lRet;
			int     nRet;
			string strMyPath = null;
			string strOtherPath = null;

			HostEntry entry = null;
			int nLen;
			int nErrorNo;
			byte [] baSendBuffer = null;
			byte [] baRecvBuffer = null;

			// int lResultMaxLen = 60000;
	
			SplitPath(strPath, out strMyPath, out strOtherPath);
	
			if (strMyPath == "") 
			{
		
				lRet = NullPackage(out baResult);
				return lRet;
			}
	
			entry = this.PrepareChannel(strMyPath);
	
			if (entry == null) 
			{ 
				//TRACE(_T("ChDir() CChannel::PrepareChannel(\"%hs\") return error\n"),
				//	(LPCSTR)strMyPath);
				return -1;
			}

			Debug.Assert(entry.m_lChannel!=-1,
				"m_lChannel不正确");
	
			param.Clear();
			param.ParaLong(entry.m_lChannel);
			param.ParaString(strUserName, entry.m_nDTLPCharset);
			param.ParaString(strPassword, entry.m_nDTLPCharset);

			int nDBCSLength = param.ParaString(strOtherPath, entry.m_nDTLPCharset);
			param.ParaLong(nDBCSLength);	// ParaString正好返回了这个尺寸

			param.ParaLong(m_nResultMaxLen);
		
			m_lErrno = 0;	//
			lRet = param.ParaToPackage(FUNC_CHDIR,
				m_lErrno,
				out baSendBuffer);
	
	
			// EndPara((LPPARATBL)&ParaTbl);
			if (lRet == -1) 
			{
				m_lErrno = GL_PARATOPACKAGE;
				//TRACE(_T("ChDir() Channel[%08x] ParaToPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				return -1; // error
			}
	
	
			nRet = entry.SendTcpPackage(baSendBuffer,
				lRet, 
				out nErrorNo);
			if (nRet < 0) 
			{
				this.m_lErrno = GL_SEND;
				//TRACE(_T("ChDir() Channel[%08x] CHostEntry::SendTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				return -1;
			}

			nRet = entry.RecvTcpPackage(out baRecvBuffer,
				out nLen,
				out nErrorNo);
			if (nRet < 0) 
			{
				//TRACE(_T("ChDir() Channel[%08x] CHostEntry::RecvTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				this.m_lErrno = nErrorNo;
				return -1;
			}
	
			param.Clear();
			param.DefPara(Param.STYLE_LONG);
			param.DefPara(Param.STYLE_BUFF);

			int nFuncNum = 0;
			try 
			{
				param.PackageToPara(baRecvBuffer,
					ref m_lErrno,
					out nFuncNum);
			}
			catch 
			{
				this.m_lErrno = GL_PACKAGETOPARA;
				return -1;
			}
			this.m_nResponseFuncNum = nFuncNum;
			lRet = param.lValue(0);

	
			//TRACE(_T("ChDir() Channel[%08x] Errno[%08x]\n"),
			//	Channel,
			//	lpChannel->m_lErrno);
	
			if (lRet != -1) 
			{
				if (param.baValue(1) == null) 
				{
					lRet = -1;
					goto WAI;
				}
				int lRet1;
				lRet1 = NtohLvalueInResult(param.baValue(1));
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_PACKAGENTOH;
					goto WAI;
				}
				lRet1 = AddCurPath(
					param.baValue(1),
					//param.lValue(0),
					strMyPath,
                    entry.m_nDTLPCharset,
					out baResult);
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_NOMEM;
				}
			}
			WAI:
	
				return lRet;
		}

		// *** 包装后的Dir，可以处理自动登录
		public int Dir(string strPath,
			out byte [] baResult)
		{
			int nRet = 0;

			REDO:
				nRet = API_Dir(strPath,
					out baResult);
			if (nRet == -1) 
			{
				int nErrorCode = this.GetLastErrno();
				if (nErrorCode == DtlpChannel.GL_NOTLOGIN) 
				{
					if (this.Container.HasAskAccountInfoEventHandler == false)
						return nRet;	// 无法获取账户信息，因此只好把错误上交

					// string strUserName, strPassword;
					// IWin32Window owner = null;
                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;

					// 获得缺省帐户信息
					int nProcRet = 0;
                INPUT:

                    // return:
                    //		2	already login succeed
                    //		1	dialog return OK
                    //		0	dialog return Cancel
                    //		-1	other error
                    /*
					nProcRet = this.Container.procAskAccountInfo(
						this, 
                        strPath, 
						out owner,
						out strUserName,
						out strPassword);
                     * */
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;

					if (nProcRet == 2)
						goto REDO;
					if (nProcRet == 1) 
					{
						byte[] baPackage = null;
						nRet = this.API_ChDir(e.UserName,   // strUserName, 
							e.Password, // strPassword,
							strPath,
							out baPackage);
						if (nRet > 0)
							goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,  // owner,
                                "Search()",
                                "Login fail ...");
						goto INPUT;
                        }
					}
					return nRet;	// 问题上交
				}
			}

			return nRet;
		}

		// *** 原始DTLP API 的Dir，没有作任何包装
		public int API_Dir(string strPath,
			out byte [] baResult)
		{
	
			// send:long Channel
			//      buff lpszPath
			//      long lPathLen
			//      long lResultMaxLen
			// recv:buff lpResult
			// return long

			baResult = null;

			DTLPParam param = new DTLPParam();
			int    lRet;
			int     nRet;

			string strMyPath = null;
			string strOtherPath = null;

			HostEntry entry = null;

			int nLen;
			int nErrorNo;
	
			byte [] baSendBuffer = null;
			byte [] baRecvBuffer = null;

			// int lResultMaxLen = 60000;

	
			SplitPath(strPath, out strMyPath, out strOtherPath);
	
			if (strMyPath == "") 
			{
				// 我还没有和远方连接,因此不知道别人的字符集.
				// 但是自己的字符集,是知道的.

				return this.LocalDir(GetLocalCharset(), out baResult);
			}
	
			entry = this.PrepareChannel(strMyPath);
			if (entry == null) 
			{ 
				return -1;
			}

			Debug.Assert(entry.m_lChannel!=-1,
				"m_lChannel不正确");
	
			param.Clear();
			param.ParaLong(entry.m_lChannel);
			int nPathLen = param.ParaString(strOtherPath, entry.m_nDTLPCharset);
			param.ParaLong(nPathLen);

			//param.ParaLong(lResultMaxLen-3*(strMyPath.GetLengthA()+1)); // new gai !!!!!
			param.ParaLong(m_nResultMaxLen);
	

			lRet = param.ParaToPackage(FUNC_DIR,
				m_lErrno,
				out baSendBuffer);
			if (lRet == -1) 
			{
				m_lErrno = GL_PARATOPACKAGE;
				return -1; // error
			}
	
			nRet = entry.SendTcpPackage(baSendBuffer,
				lRet, 
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = GL_SEND;
				//TRACE(_T("Dir() Channel[%08x] CHostEntry::SendTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				return -1;
			}

			nRet = entry.RecvTcpPackage(out baRecvBuffer,
				out nLen,
				out nErrorNo);
			if (nRet<0) 
			{
				//TRACE(_T("Dir() Channel[%08x] CHostEntry::RecvTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				this.m_lErrno = nErrorNo;
				return -1;
			}
	
			param.Clear();
			param.DefPara(Param.STYLE_LONG);
			param.DefPara(Param.STYLE_BUFF);
			int nFuncNum = 0;
			try 
			{
				param.PackageToPara(baRecvBuffer,
					ref m_lErrno,
					out nFuncNum);
			}
			catch 
			{
				this.m_lErrno = GL_PACKAGETOPARA;
				return -1;
			}	
			this.m_nResponseFuncNum = nFuncNum;
			lRet = param.lValue(0);
	
			if (lRet != -1) 
			{
				int lRet1;
				lRet1 = NtohLvalueInResult(param.baValue(1));
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_PACKAGENTOH;
					goto WAI;
				}
				lRet1 = AddCurPath(
					param.baValue(1),
					//param.lValue(0),
					strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_NOMEM;
				}
			}
			WAI:
	
				return lRet;
		}

		// *** 包装后的Search，可以处理自动登录
		public int Search(string strPath,
			int lStyle,
			out byte [] baResult)
		{
			int nRet = 0;

            int nRedoCount = 0;
			REDO:
				nRet = API_Search(strPath,
					lStyle,
					out baResult);
			if (nRet == -1) 
			{
				int nErrorCode = this.GetLastErrno();
				if (nErrorCode == DtlpChannel.GL_NOTLOGIN) 
				{
					if (this.Container.HasAskAccountInfoEventHandler == false)
						return nRet;	// 无法获取账户信息，因此只好把错误上交

                    // 2007/7/5
                    if (nRedoCount > 10)
                        return nRet;	// 无法获取账户信息，因此只好把错误上交


                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;
					// string strUserName, strPassword;
					// IWin32Window owner = null;
					// 获得缺省帐户信息
					int nProcRet = 0;
                INPUT:
                    // return:
                    //		2	already login succeed
                    //		1	dialog return OK
                    //		0	dialog return Cancel
                    //		-1	other error
                    /*
					nProcRet = this.Container.procAskAccountInfo(
						this, strPath, 
						out owner,
						out strUserName,
						out strPassword);
                     * */
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;

                    if (nProcRet == 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }
					if (nProcRet == 1) 
					{
						byte[] baPackage = null;
						nRet = this.API_ChDir(e.UserName,   // strUserName, 
							e.Password, // strPassword,
							strPath,
							out baPackage);
						if (nRet > 0)
							goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
						    goto INPUT;
                        }

					}
					return nRet;	// 问题上交
				}
			}

			return nRet;
		}

		// *** 包装后的Search，可以处理自动登录
		public int Search(string strPath,
			byte[] baNext,
			int lStyle,
			out byte [] baResult)
		{
			int nRet = 0;

			REDO:
				nRet = API_Search(strPath,
					baNext,
					lStyle,
					out baResult);
			if (nRet == -1) 
			{
				int nErrorCode = this.GetLastErrno();
				if (nErrorCode == DtlpChannel.GL_NOTLOGIN) 
				{
					if (this.Container.HasAskAccountInfoEventHandler == false)
						return nRet;	// 无法获取账户信息，因此只好把错误上交

                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;
					//string strUserName, strPassword;
					//IWin32Window owner = null;
					// 获得缺省帐户信息
					int nProcRet = 0;
                INPUT:
                    // return:
                    //		2	already login succeed
                    //		1	dialog return OK
                    //		0	dialog return Cancel
                    //		-1	other error
                    /*
					nProcRet = this.Container.procAskAccountInfo(
						this, strPath, 
						out owner,
						out strUserName,
						out strPassword);
                     * */
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;
					if (nProcRet == 2)
						goto REDO;
					if (nProcRet == 1) 
					{
						byte[] baPackage = null;
						nRet = this.API_ChDir(e.UserName,   // strUserName, 
							e.Password, // strPassword,
							strPath,
							out baPackage);
						if (nRet > 0)
							goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
						goto INPUT;
                        }
                        // 原来goto INPUT; 在这里
					}
					return nRet;	// 问题上交
				}
			}

			return nRet;
		}

		// *** 原始DTLP API 的Search，没有作任何包装
		public int API_Search(string strPath,
			int lStyle,
			out byte [] baResult)
		{
			// send:long Channel
			//      buff lpszPath
			//      long lPathLen
			//      long lResultMaxLen
			//      long lStyle
			// recv:buff lpResult
			// return long

			baResult = null;

			DTLPParam param = new DTLPParam();
			int    lRet;
			int     nRet;

			string strMyPath = null;
			string strOtherPath = null;

			HostEntry entry = null;
			
			int nLen;
			int nErrorNo;
	
	
			byte [] baSendBuffer = null;
			byte [] baRecvBuffer = null;

			// int lResultMaxLen = 60000;
	
			SplitPath(strPath, out strMyPath, out strOtherPath);
	
			if (strMyPath == "") 
			{
				return this.LocalDir(GetLocalCharset(), out baResult);
			}
	
	
			entry = this.PrepareChannel(strMyPath);
			if (entry == null) 
			{ 
				return -1;
			}
	
	
			if (strOtherPath == "") 
			{
				return this.SingleDir(strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
			}
	

			Debug.Assert(entry.m_lChannel!=-1,
				"m_lChannel不正确");
	
			param.Clear();
			param.ParaLong(entry.m_lChannel);
			int nPathLen = param.ParaString(strOtherPath, entry.m_nDTLPCharset);
			param.ParaLong(nPathLen);

			param.ParaLong(m_nResultMaxLen);
			param.ParaLong(lStyle);
	
			lRet = param.ParaToPackage(FUNC_SEARCH,
				m_lErrno,
				out baSendBuffer);
			if (lRet == -1) 
			{
				m_lErrno = GL_PARATOPACKAGE;
				return -1; // error
			}
	
			nRet = entry.SendTcpPackage(baSendBuffer,
				lRet, 
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = GL_SEND;
				//TRACE(_T("Dir() Channel[%08x] CHostEntry::SendTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				return -1;
			}

			nRet = entry.RecvTcpPackage(out baRecvBuffer,
				out nLen,
				out nErrorNo);
			if (nRet<0) 
			{
				//TRACE(_T("Dir() Channel[%08x] CHostEntry::RecvTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				this.m_lErrno = nErrorNo;
				return -1;
			}
	
			param.Clear();
			param.DefPara(Param.STYLE_LONG);
			param.DefPara(Param.STYLE_BUFF);
			int nFuncNum = 0;
			try 
			{
				param.PackageToPara(baRecvBuffer,
					ref m_lErrno,
					out nFuncNum);
			}
			catch 
			{
				this.m_lErrno = GL_PACKAGETOPARA;
				return -1;
			}			
			this.m_nResponseFuncNum = nFuncNum;
			lRet = param.lValue(0);
	
			if (lRet != -1) 
			{
				int lRet1;
				lRet1 = NtohLvalueInResult(param.baValue(1));
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_PACKAGENTOH;
					goto WAI;
				}
				lRet1 = AddCurPath(
					param.baValue(1),
					//param.lValue(0),
					strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_NOMEM;
				}
			}
			WAI:
	
				return lRet;
		}


		// *** 原始DTLP API 的Search，没有作任何包装
		// 特殊版本，给出byte[]类型的next字符串
		public int API_Search(string strPath,
			byte[] baNext,
			int lStyle,
			out byte [] baResult)
		{
			// send:long Channel
			//      buff lpszPath
			//      long lPathLen
			//      long lResultMaxLen
			//      long lStyle
			// recv:buff lpResult
			// return long

			baResult = null;

			DTLPParam param = new DTLPParam();
			int    lRet;
			int     nRet;

			string strMyPath = null;
			string strOtherPath = null;

			HostEntry entry = null;
			
			int nLen;
			int nErrorNo;
	
	
			byte [] baSendBuffer = null;
			byte [] baRecvBuffer = null;

			// int lResultMaxLen = 60000;
	
			SplitPath(strPath, out strMyPath, out strOtherPath);
	
			if (strMyPath == "") 
			{
				return this.LocalDir(GetLocalCharset(), out baResult);
			}
	
	
			entry = this.PrepareChannel(strMyPath);
			if (entry == null) 
			{ 
				return -1;
			}
	
	
			if (strOtherPath == "" 
				&& 
				(baNext == null || baNext.Length == 0) 
				)
			{
				return this.SingleDir(strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
			}
	

			Debug.Assert(entry.m_lChannel!=-1,
				"m_lChannel不正确");
	
			param.Clear();
			param.ParaLong(entry.m_lChannel);
			int nPathLen = param.ParaPathString(strOtherPath,
				entry.m_nDTLPCharset,
				baNext);
			param.ParaLong(nPathLen);

			param.ParaLong(m_nResultMaxLen);
			param.ParaLong(lStyle);
	
			lRet = param.ParaToPackage(FUNC_SEARCH,
				m_lErrno,
				out baSendBuffer);
			if (lRet == -1) 
			{
				m_lErrno = GL_PARATOPACKAGE;
				return -1; // error
			}
	
			nRet = entry.SendTcpPackage(baSendBuffer,
				lRet, 
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = GL_SEND;
				//TRACE(_T("Dir() Channel[%08x] CHostEntry::SendTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				return -1;
			}

			nRet = entry.RecvTcpPackage(out baRecvBuffer,
				out nLen,
				out nErrorNo);
			if (nRet<0) 
			{
				//TRACE(_T("Dir() Channel[%08x] CHostEntry::RecvTcpPackage() Errno[%08x]\n"),
				//	Channel,
				//	lpChannel->m_lErrno);
				this.m_lErrno = nErrorNo;
				return -1;
			}
	
			param.Clear();
			param.DefPara(Param.STYLE_LONG);
			param.DefPara(Param.STYLE_BUFF);
			int nFuncNum = 0;
			try 
			{
				param.PackageToPara(baRecvBuffer,
					ref m_lErrno,
					out nFuncNum);
			}
			catch 
			{
				this.m_lErrno = GL_PACKAGETOPARA;
				return -1;
			}			
			this.m_nResponseFuncNum = nFuncNum;
			lRet = param.lValue(0);
	
			if (lRet != -1) 
			{
				int lRet1;
				lRet1 = NtohLvalueInResult(param.baValue(1));
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_PACKAGENTOH;
					goto WAI;
				}
				lRet1 = AddCurPath(
					param.baValue(1),
					//param.lValue(0),
					strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_NOMEM;
				}
			}
			WAI:
	
				return lRet;
		}

		public int API_Management(string strPath,
			string strSource1,
//							 long  lSourceMaxLen1,
			string strSource2,
//							 long  lSourceMaxLen2,
			out byte [] baResult)
		{
			// send:long Channel
			//      buff lpszPath
			//      long lPathLen
			//      buff lpSource1
			//      long lSourceMaxLen1
			//      buff lpSource2
			//      long lSourceMaxLen2
			//      long lResultMaxLen
			// recv:buff lpResult
			// return long

			baResult = null;
	
			DTLPParam param = new DTLPParam();
			int    lRet;
			int     nRet;

			string strMyPath = null;
			string strOtherPath = null;

			HostEntry entry = null;

			int nLen;
			int nErrorNo;
	
			byte [] baSendBuffer = null;
			byte [] baRecvBuffer = null;
	
	
			SplitPath(strPath, out strMyPath, out strOtherPath);
	
			if (strMyPath == "") 
			{
				// 指出错误原因
				return -1;
			}
	
			entry = this.PrepareChannel(strMyPath);
			if (entry == null) 
			{ 
				return -1;
			}
	
			Debug.Assert(entry.m_lChannel!=-1,
				"m_lChannel不正确");

			param.Clear();
			param.ParaLong(entry.m_lChannel);

			int nPathLen = param.ParaString(strOtherPath, entry.m_nDTLPCharset);
			param.ParaLong(nPathLen);

			int nSource1Len = param.ParaString(strSource1, entry.m_nDTLPCharset);
			param.ParaLong(nSource1Len);

			int nSource2Len = param.ParaString(strSource2, entry.m_nDTLPCharset);
			param.ParaLong(nSource2Len);

			param.ParaLong(m_nResultMaxLen);
	
			lRet = param.ParaToPackage(FUNC_ACCMANAGEMENT,
				m_lErrno,
				out baSendBuffer);
			if (lRet == -1) 
			{
				m_lErrno = GL_PARATOPACKAGE;
				return -1; // error
			}
	
			nRet = entry.SendTcpPackage(baSendBuffer,
				lRet, 
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = GL_SEND;
				return -1;
			}

			nRet = entry.RecvTcpPackage(out baRecvBuffer,
				out nLen,
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = nErrorNo;
				return -1;
			}
	
			param.Clear();
			param.DefPara(Param.STYLE_LONG);
			param.DefPara(Param.STYLE_BUFF);
			int nFuncNum = 0;
			try 
			{
				param.PackageToPara(baRecvBuffer,
					ref m_lErrno,
					out nFuncNum);
			}
			catch 
			{
				this.m_lErrno = GL_PACKAGETOPARA;
				return -1;
			}			
			this.m_nResponseFuncNum = nFuncNum;
			lRet = param.lValue(0);
	
			if (lRet != -1) 
			{
				int lRet1;
				lRet1 = NtohLvalueInResult(param.baValue(1));
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_PACKAGENTOH;
					goto WAI;
				}
				lRet1 = AddCurPath(
					param.baValue(1),
					//param.lValue(0),
					strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_NOMEM;
				}
			}
			WAI:
				return lRet;
		}

		public int API_Write(string strPath,
			byte [] baBuffer,
			out byte[] baResult,
			int lStyle)
		{
			// send:long Channel
			//      buff lpszPath
			//      long lPathLen
			//      buff lpBuffer
			//      long lBufferLen
			//      long lResultMaxLen
			//      long lStyle
			// recv:buff lpResult
			// return long

			baResult = null;

	
			DTLPParam param = new DTLPParam();
			int    lRet;
			int     nRet;

			string strMyPath = null;
			string strOtherPath = null;

			HostEntry entry = null;

			int nLen;
			int nErrorNo;
	
			byte [] baSendBuffer = null;
			byte [] baRecvBuffer = null;
	
	
			SplitPath(strPath, out strMyPath, out strOtherPath);
	
			if (strMyPath == "") 
			{
				// 指出错误原因
				this.m_lErrno = GL_ACCESSDENY;
				return -1;
			}

	
			entry = this.PrepareChannel(strMyPath);
			if (entry == null) 
			{ 
				return -1;
			}

	
			if (strOtherPath == "") 
			{
				this.m_lErrno = GL_ACCESSDENY;
				return -1;
			}
	
			Debug.Assert(entry.m_lChannel!=-1,
				"m_lChannel不正确");

			param.Clear();
			param.ParaLong(entry.m_lChannel);

			int nPathLen = param.ParaString(strOtherPath, entry.m_nDTLPCharset);
			param.ParaLong(nPathLen);

			param.ParaBuff(baBuffer, baBuffer.Length);
			param.ParaLong(baBuffer.Length);

			param.ParaLong(m_nResultMaxLen);

			param.ParaLong(lStyle);

			lRet = param.ParaToPackage(FUNC_WRITE,
				m_lErrno,
				out baSendBuffer);
			if (lRet == -1) 
			{
				m_lErrno = GL_PARATOPACKAGE;
				return -1; // error
			}
	
	
			nRet = entry.SendTcpPackage(baSendBuffer,
				lRet, 
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = GL_SEND;
				return -1;
			}

			nRet = entry.RecvTcpPackage(out baRecvBuffer,
				out nLen,
				out nErrorNo);
			if (nRet<0) 
			{
				this.m_lErrno = nErrorNo;
				return -1;
			}
	
			param.Clear();
			param.DefPara(Param.STYLE_LONG);
			param.DefPara(Param.STYLE_BUFF);
			int nFuncNum = 0;
			try 
			{
				param.PackageToPara(baRecvBuffer,
					ref m_lErrno,
					out nFuncNum);
			}
			catch 
			{
				this.m_lErrno = GL_PACKAGETOPARA;
				return -1;
			}			
			this.m_nResponseFuncNum = nFuncNum;
			lRet = param.lValue(0);
	
			if (lRet != -1) 
			{
				int lRet1;
				lRet1 = NtohLvalueInResult(param.baValue(1));
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_PACKAGENTOH;
					goto WAI;
				}
				lRet1 = AddCurPath(
					param.baValue(1),
					//param.lValue(0),
					strMyPath,
					entry.m_nDTLPCharset,
					out baResult);
				if (lRet1 == -1) 
				{
					lRet = -1;
					m_lErrno = GL_NOMEM;
				}
			}
			WAI:
	
				return lRet;
		}

        public int DeleteMarcRecord(string strPath,
            byte[] baTimestamp,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            int nStyle = DELETE_WRITE;

            if (baTimestamp == null)
            {
                strError = "baTimeStamp参数不能为null";
                return -1;
            }

            if (baTimestamp.Length < 9)
            {
                strError = "baTimeStamp内容的长度不能小于9 bytes";
                return -1;
            }


            byte[] baResult = null;

        REDO:

            nRet = API_Write(strPath,
                baTimestamp,
                out baResult,
                nStyle);

            if (nRet == -1)
            {
                int nErrorCode = this.GetLastErrno();
                if (nErrorCode == DtlpChannel.GL_NOTLOGIN)
                {
                    if (this.Container.HasAskAccountInfoEventHandler == false)
                        return nRet;	// 无法获取账户信息，因此只好把错误上交


                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;
                    // 获得缺省帐户信息
                    int nProcRet = 0;
                INPUT:
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;
                    if (nProcRet == 2)
                        goto REDO;
                    if (nProcRet == 1)
                    {
                        byte[] baPackage = null;
                        nRet = this.API_ChDir(e.UserName,   // strUserName, 
                            e.Password, // strPassword,
                            strPath,
                            out baPackage);
                        if (nRet > 0)
                            goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
                        goto INPUT;
                        }
                    }
                    return nRet;	// 问题上交
                } // end if not login

                if (nErrorCode == GL_ERRSIGNATURE)
                {
                    // 时间戳不匹配
                }

                return nRet;
            }

            return nRet;
        }

        // 
		public int WriteMarcRecord(string strPath,
			int nStyle,
			string strRecord,
			byte[] baTimeStamp,
			out string strError)
		{
			int nRet = 0;
			strError = "";

			if (baTimeStamp == null) 
			{
				strError = "baTimeStamp参数不能为null";
				return -1;
			}

			if (baTimeStamp.Length < 9) 
			{
				strError = "baTimeStamp内容的长度不能小于9 bytes";
				return -1;
			}


			// 构造要写入的数据
			Encoding encoding = this.GetPathEncoding(strPath);

            CanonicalizeMARC(ref strRecord);

			byte[] baMARC = encoding.GetBytes(strRecord);

			byte[] baBuffer = null;

			baBuffer = ByteArray.EnsureSize(baBuffer, baMARC.Length + 9);
			Array.Copy(baTimeStamp,0, baBuffer, 0, 9);
			Array.Copy(baMARC, 0, baBuffer, 9, baMARC.Length);

			byte[] baResult = null;

			REDO:

			nRet = API_Write(strPath,
				baBuffer,
				out baResult,
				nStyle);

			if (nRet == -1) 
			{
				int nErrorCode = this.GetLastErrno();
				if (nErrorCode == DtlpChannel.GL_NOTLOGIN) 
				{
					if (this.Container.HasAskAccountInfoEventHandler == false)
						return nRet;	// 无法获取账户信息，因此只好把错误上交


                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;
					//string strUserName, strPassword;
					//IWin32Window owner = null;
					// 获得缺省帐户信息
					int nProcRet = 0;
                INPUT:
                    // return:
                    //		2	already login succeed
                    //		1	dialog return OK
                    //		0	dialog return Cancel
                    //		-1	other error
                    /*
					nProcRet = this.Container.procAskAccountInfo(
						this, strPath, 
						out owner,
						out strUserName,
						out strPassword);
                     * */
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;
					if (nProcRet == 2)
						goto REDO;
					if (nProcRet == 1) 
					{
						byte[] baPackage = null;
						nRet = this.API_ChDir(e.UserName,   // strUserName, 
							e.Password, // strPassword,
							strPath,
							out baPackage);
						if (nRet > 0)
							goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
						goto INPUT;
                        }
					}
					return nRet;	// 问题上交
				} // end if not login

				if (nErrorCode == GL_ERRSIGNATURE)
				{
					// 时间戳不匹配
				}

				return nRet;
			}

			return nRet;
		}

        // 把MARC记录正规化，以便保存到DTLP服务器
        // 检查记录末尾是否有30 29结束符，如果没有，给加上
        public static void CanonicalizeMARC(ref string strMARC)
        {
            if (strMARC.Length == 0)
            {
                strMARC = "012345678901234567890123001---" + new string(MarcUtil.FLDEND, 1) + new string(MarcUtil.RECEND, 1);
                return;
            }

            int nTail = strMARC.Length - 1;
            if (strMARC[nTail] != MarcUtil.RECEND)
            {
                if (strMARC[nTail] == MarcUtil.FLDEND)
                {
                    strMARC = strMARC + new string(MarcUtil.RECEND, 1);
                    return;
                }
                else
                {
                    strMARC = strMARC + new string(MarcUtil.FLDEND, 1) + new string(MarcUtil.RECEND, 1);
                    return;
                }
            }

            return;
        }

        // 另一版本
        public int WriteMarcRecord(string strPath,
            int nStyle,
            string strRecord,
            byte[] baTimeStamp,
            out string strOutputRecord,
            out string strOutputPath,
            out byte [] baOutputTimestamp,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            strOutputRecord = "";
            strOutputPath = "";
            baOutputTimestamp = null;

            if (baTimeStamp == null)
            {
                strError = "baTimeStamp参数不能为null";
                return -1;
            }

            if (baTimeStamp.Length < 9)
            {
                strError = "baTimeStamp内容的长度不能小于9 bytes";
                return -1;
            }


            // 构造要写入的数据
            Encoding encoding = this.GetPathEncoding(strPath);

            CanonicalizeMARC(ref strRecord);

            byte[] baMARC = encoding.GetBytes(strRecord);

            byte[] baBuffer = null;

            baBuffer = ByteArray.EnsureSize(baBuffer, baMARC.Length + 9);
            Array.Copy(baTimeStamp, 0, baBuffer, 0, 9);
            Array.Copy(baMARC, 0, baBuffer, 9, baMARC.Length);

            byte[] baResult = null;

        REDO:

            nRet = API_Write(strPath,
                baBuffer,
                out baResult,
                nStyle);

            if (nRet == -1)
            {
                int nErrorCode = this.GetLastErrno();
                if (nErrorCode == DtlpChannel.GL_NOTLOGIN)
                {
                    if (this.Container.HasAskAccountInfoEventHandler == false)
                        return nRet;	// 无法获取账户信息，因此只好把错误上交


                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;
                    //string strUserName, strPassword;
                    //IWin32Window owner = null;
                    // 获得缺省帐户信息
                    int nProcRet = 0;
                INPUT:
                    // return:
                    //		2	already login succeed
                    //		1	dialog return OK
                    //		0	dialog return Cancel
                    //		-1	other error
                    /*
                    nProcRet = this.Container.procAskAccountInfo(
                        this, strPath,
                        out owner,
                        out strUserName,
                        out strPassword);
                     * */
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;
                    if (nProcRet == 2)
                        goto REDO;
                    if (nProcRet == 1)
                    {
                        byte[] baPackage = null;
                        nRet = this.API_ChDir(e.UserName,   //strUserName,
                            e.Password, // strPassword,
                            strPath,
                            out baPackage);
                        if (nRet > 0)
                            goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
                        goto INPUT;
                        }
                    }
                    return nRet;	// 问题上交
                } // end if not login

                if (nErrorCode == GL_ERRSIGNATURE)
                {
                    // 时间戳不匹配
                }

                // 
                strError = "API_Write()出错:\r\n"
                    + "路径: " + strPath + "\r\n"
                    + "错误码: " + nErrorCode + "\r\n"
                    + "错误信息: " + GetErrorString(nErrorCode) + "\r\n";

                return nRet;
            }

            int nResult = nRet;

            // 分析服务器返回的记录
            Package package = new Package();
            package.LoadPackage(baResult,
                encoding);
            nRet = package.Parse(PackageFormat.Binary);
            if (nRet == -1)
            {
                strError = "Package::Parse() error";
                goto ERROR1;
            }

            byte[] content = null;
            nRet = package.GetFirstBin(out content);
            if (nRet == -1)
            {
                strError = "Package::GetFirstBin() error";
                goto ERROR1;
            }

            if (content == null
                || content.Length < 9)
            {
                strError = "content length < 9";
                goto ERROR1;
            }

            baOutputTimestamp = new byte[9];
            Array.Copy(content, baOutputTimestamp, 9);

            byte[] marc = new byte[content.Length - 9];
            Array.Copy(content,
                9,
                marc,
                0,
                content.Length - 9);

            strOutputRecord = encoding.GetString(marc);

            strOutputPath = package.GetFirstPath();

            return nResult;
        ERROR1:
            return -1;
        }

        // 获得一条记录的检索点
        public int GetAccessPoint(string strPath,
            string strRecord,
            out List<string> results,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            results = new List<string>();

            int nStyle = GETKEYS_WRITE;
            byte[] baTimeStamp = new byte[9];

            // 构造要写入的数据
            Encoding encoding = this.GetPathEncoding(strPath);

            CanonicalizeMARC(ref strRecord);

            byte[] baMARC = encoding.GetBytes(strRecord);

            byte[] baBuffer = null;

            baBuffer = ByteArray.EnsureSize(baBuffer, baMARC.Length + 9);
            Array.Copy(baTimeStamp, 0, baBuffer, 0, 9);
            Array.Copy(baMARC, 0, baBuffer, 9, baMARC.Length);

            byte[] baResult = null;

        REDO:
            nRet = API_Write(strPath,
                baBuffer,
                out baResult,
                nStyle);
            if (nRet == -1)
            {
                int nErrorCode = this.GetLastErrno();
                if (nErrorCode == DtlpChannel.GL_NOTLOGIN)
                {
                    if (this.Container.HasAskAccountInfoEventHandler == false)
                        return nRet;	// 无法获取账户信息，因此只好把错误上交


                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strPath;
                    //string strUserName, strPassword;
                    //IWin32Window owner = null;
                    // 获得缺省帐户信息
                    int nProcRet = 0;
                INPUT:
                    // return:
                    //		2	already login succeed
                    //		1	dialog return OK
                    //		0	dialog return Cancel
                    //		-1	other error
                    /*
                    nProcRet = this.Container.procAskAccountInfo(
                        this, strPath,
                        out owner,
                        out strUserName,
                        out strPassword);
                     * */
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;
                    if (nProcRet == 2)
                        goto REDO;
                    if (nProcRet == 1)
                    {
                        byte[] baPackage = null;
                        nRet = this.API_ChDir(e.UserName,   //strUserName,
                            e.Password, // strPassword,
                            strPath,
                            out baPackage);
                        if (nRet > 0)
                            goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
                            goto INPUT;
                        }
                    }
                    return nRet;	// 问题上交
                } // end if not login

                if (nErrorCode == GL_ERRSIGNATURE)
                {
                    // 时间戳不匹配
                }

                // 
                strError = "API_Write:\r\n"
                    + "路径: " + strPath + "\r\n"
                    + "错误码: " + nErrorCode + "\r\n"
                    + "错误信息: " + GetErrorString(nErrorCode) + "\r\n";
                return nRet;
            }

            int nResult = nRet;

            // 分析服务器返回的记录
            Package package = new Package();
            package.LoadPackage(baResult,
                encoding);
            nRet = package.Parse(PackageFormat.String);
            if (nRet == -1)
            {
                strError = "Package::Parse() error";
                goto ERROR1;
            }

            for (int i = 0; i < package.Count; i++)
            {
                Cell cell = (Cell)package[i];
                results.Add(cell.Content);
            }

            return nResult;
        ERROR1:
            return -1;
        }

		// 检索一条MARC记录
		// return:
		//		-1	error
		//		0	not found
		//		1	found
		public int GetRecord(
			string strPath,
			int nStyle,
			out string strRecPath,
			out string strRecord,
			out byte[] baTimeStamp,	// 至少9字符空间
			out string strError)
		{
			strRecPath = "";
			strRecord = "";
			baTimeStamp = null;
			strError = "";

			byte[] baPackage;
			byte[] baMARC;
	
			int nRet = this.Search(strPath,
				DtlpChannel.XX_STYLE | nStyle,
				out baPackage);

			if (nRet == -1L) 
			{
				int nErrorCode = this.GetLastErrno();

				if (nErrorCode == DtlpChannel.GL_NOTEXIST) 
					return 0;	// not found

				string strText = this.GetErrorString(nErrorCode);

				string strHex = String.Format("0X{0,8:X}",nErrorCode);

				strError = "错误码"
					+ strHex + ", 原因: "
					+ strText;
				goto ERROR1;
			}

			Package package = new Package();

			package.LoadPackage(baPackage, this.GetPathEncoding(strPath));
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
				baTimeStamp = new byte[9];
				Array.Copy(baMARC, 0, baTimeStamp, 0, 9);

				byte [] baBody = new byte[baMARC.Length - 9];
				Array.Copy(baMARC, 9, baBody, 0, baMARC.Length - 9);
				baMARC = baBody;
			}
			else 
			{
				// 记录有问题，放入一个空记录?
			}

			strRecord = GetPathEncoding(strPath).GetString(baMARC);	// ?????

			strRecPath = package.GetFirstPath();

            return 1;	// found
        ERROR1:
            return -1;
        }

        // 保存到(服务器端)配置文件
        // return:
        //      -1  出错
        //      0   成功
        public int WriteCfgFile(string strCfgFilePath,
            string strContent,
            out string strError)
        {
            strError = "";

            /*
            if (baTimeStamp == null)
            {
                strError = "baTimeStamp参数不能为null";
                return -1;
            }

            if (baTimeStamp.Length < 9)
            {
                strError = "baTimeStamp内容的长度不能小于9 bytes";
                return -1;
            }*/

            // 构造要写入的数据
            Encoding encoding = this.GetPathEncoding(strCfgFilePath);

            strContent = strContent.Replace("\r\n", "\r");

            byte[] baContent = encoding.GetBytes(strContent);

            for (int i = 0; i < baContent.Length; i++)
            {
                if (baContent[i] == (char)'\r')
                    baContent[i] = 0;
            }

            byte[] baResult = null;

            int nStyle = 0;

        REDO:

            int nRet = API_Write(strCfgFilePath,
                baContent,
                out baResult,
                nStyle);

            if (nRet == -1)
            {
                int nErrorCode = this.GetLastErrno();
                if (nErrorCode == DtlpChannel.GL_NOTLOGIN)
                {
                    if (this.Container.HasAskAccountInfoEventHandler == false)
                        return nRet;	// 无法获取账户信息，因此只好把错误上交


                    AskDtlpAccountInfoEventArgs e = new AskDtlpAccountInfoEventArgs();
                    e.Channel = this;
                    e.Path = strCfgFilePath;
                    //string strUserName, strPassword;
                    //IWin32Window owner = null;
                    // 获得缺省帐户信息
                    int nProcRet = 0;
                INPUT:
                    this.Container.CallAskAccountInfo(this, e);
                    nProcRet = e.Result;
                    if (nProcRet == 2)
                        goto REDO;
                    if (nProcRet == 1)
                    {
                        byte[] baPackage = null;
                        nRet = this.API_ChDir(e.UserName,   // strUserName, 
                            e.Password, // strPassword,
                            strCfgFilePath,
                            out baPackage);
                        if (nRet > 0)
                            goto REDO;
                        if (this.Container.GUI == true)
                        {
                            this.ErrorBox(e.Owner,
                                "Search()",
                                "Login fail ...");
                            goto INPUT;
                        }
                    }
                    return nRet;	// 问题上交
                } // end if not login

                if (nErrorCode == GL_ERRSIGNATURE)
                {
                    // 时间戳不匹配
                }

                string strText = this.GetErrorString(nErrorCode);
                string strHex = String.Format("0X{0,8:X}", nErrorCode);

                strError = "保存配置文件 '" + strCfgFilePath + " ' 时发生错误: "
                    + "错误码"
                    + strHex + ", 原因: "
                    + strText;
                return nRet;
            }

            return nRet;
        }

        public string GetErrorString()
        {
            int nErrorCode = this.GetLastErrno();

            string strText = this.GetErrorString(nErrorCode);
            string strHex = String.Format("0X{0,8:X}", nErrorCode);

            return "错误码 "
                + strHex + ", 原因: "
                + strText;
        }

        // 获得(服务器端)配置文件内容
        // return:
        //      -1  出错
        //      0   文件不存在
        //      1   成功
        public int GetCfgFile(string strCfgFilePath,
            out string strContent,
            out string strError)
        {
            strError = "";
            strContent = "";
            int nRet;
            byte[] baPackage = null;

            bool bFirst = true;

            byte[] baNext = null;
            int nStyle = DtlpChannel.XX_STYLE;

            byte[] baContent = null;

            Encoding encoding = this.GetPathEncoding(strCfgFilePath);


            for (; ; )
            {
                if (bFirst == true)
                {
                    nRet = this.Search(strCfgFilePath,
                        DtlpChannel.XX_STYLE,
                        out baPackage);
                }
                else
                {
                    nRet = this.Search(strCfgFilePath,
                        baNext,
                        DtlpChannel.XX_STYLE,
                        out baPackage);
                }

                if (nRet == -1)
                {
                    int nErrorCode = this.GetLastErrno();

                    if (nErrorCode == DtlpChannel.GL_NOTEXIST)
                        return 0;	// not found

                    string strText = this.GetErrorString(nErrorCode);

                    string strHex = String.Format("0X{0,8:X}", nErrorCode);

                    strError = "获取配置文件 '" + strCfgFilePath + " ' 时发生错误: "
                        +"错误码"
                        + strHex + ", 原因: "
                        + strText;
                    goto ERROR1;
                }

                Package package = new Package();
                package.LoadPackage(baPackage, encoding);
                package.Parse(PackageFormat.Binary);

                bFirst = false;

                byte[] baPart = null;
                package.GetFirstBin(out baPart);
                if (baContent == null)
                    baContent = baPart;
                else
                    baContent = ByteArray.Add(baContent, baPart);

                if (package.ContinueString != "")
                {
                    nStyle |= DtlpChannel.CONT_RECORD;
                    baNext = package.ContinueBytes;
                }
                else
                {
                    break;
                }
            }

            if (baContent != null)
            {
                for (int i = 0; i < baContent.Length; i++)
                {
                    if (baContent[i] == 0)
                        baContent[i] = (byte)'\r';
                }
                strContent = encoding.GetString(baContent).Replace("\r", "\r\n");
            }

            return 1;
        ERROR1:
            return -1;
        }

		public static void SetInt32Value(Int32 v,
			byte [] baBuffer,
			int offs)
		{
			byte [] va = BitConverter.GetBytes(v);

			for(int i =0; i<va.Length; i++)
			{
				baBuffer.SetValue(va[i], offs + i);
			}
		}


		// 将通讯包中所有network byteorder的整数转换为local byteorder
		public static int NtohLvalueInResult(byte [] baBuffer)
		{
			if (baBuffer == null)
				return -1;
			//  long packageLength;
			Int32 paraLength, pathLength;
			Int32 dataLength, maskLength, Mask;
	
			//  long rtnValue, myErrno;
			Int32 offs/*, len, Temp*/;
			Int32 lLength = 4;
	
			offs = 0;

			/* result package */
			paraLength = BitConverter.ToInt32(baBuffer, offs);
			paraLength = IPAddress.NetworkToHostOrder((Int32)paraLength);
			if(paraLength < lLength)
				return -1;
	
			SetInt32Value(paraLength, baBuffer, offs);

			if (paraLength == lLength)
				return 0;
			offs += lLength;
	
			while(true)
			{
				/* 1. pathlen + path */
				pathLength = BitConverter.ToInt32(baBuffer, offs);
				pathLength = IPAddress.NetworkToHostOrder((Int32)pathLength);
				if(pathLength + offs > paraLength) 
					break;
		
				SetInt32Value(pathLength, baBuffer, offs);
				offs += pathLength;
				if(offs >= paraLength) 
					break;
		
				/* 2. datalen + masklen + mask + data */
				dataLength = BitConverter.ToInt32(baBuffer, offs);
				dataLength = IPAddress.NetworkToHostOrder((Int32)dataLength);
				if(dataLength + offs > paraLength) 
				{ 
					//fnprintf(_T("tcps.log"), _T("\r\n*** NtoH Package Error"));
					return -1;  // jia
				}
		
				SetInt32Value(dataLength, baBuffer, offs);
	
				maskLength = BitConverter.ToInt32(baBuffer, offs + lLength);
				maskLength = IPAddress.NetworkToHostOrder((Int32)maskLength);
				SetInt32Value(maskLength, baBuffer, offs + lLength);

				Mask = BitConverter.ToInt32(baBuffer, offs + 2 * lLength);
				Mask = IPAddress.NetworkToHostOrder((Int32)Mask);
				SetInt32Value(Mask, baBuffer, offs + 2 * lLength);
	
				offs += dataLength;
				if(offs >= paraLength) 
					break;

			} /* each path */
	
			return 0;
		}

		// 似乎可以让调主来做?

		// 将返回包lpPackage中的所有路径加上lpCurPath这样一级，
		// 将结果放入lpResult中。
		static int AddCurPath(
			byte [] baPackage,
			// int	lPackageLen,
			string strCurPath,
			int nCharset,
			out byte [] baResult)
		{
			Int32 lPackageLen = 0;
			Int32 lPathLen;
			Int32 lBaoLen;
			int nCurPathLen;
			int s,t,i;

			baResult = null;
	
			if (baPackage == null)
				return -1;
	
			if (baPackage.Length == 0)
				return 0;
	
			lPackageLen = BitConverter.ToInt32(baPackage, 0);
			if (lPackageLen <= 0)
				return -1; 

			byte[] curPathBuffer = DTLPParam.GetEncoding(nCharset).GetBytes(strCurPath);

	
			nCurPathLen = curPathBuffer.Length;
			s = 4;
			t = 4;
	
			for(i=0; i<10; i++) // < 10?
			{ 
				// pathlen
				lPathLen = BitConverter.ToInt32(baPackage, s);	// *((long *)(lpPackage + s));
		
				if (lPathLen < 4) 
				{
					return -1;	// 原始包格式出错
				}

				s += 4;
				//*((long *)(lpResult+t)) = lPathLen +(long)nCurPathLen+1L;  // gai
				baResult = ByteArray.EnsureSize(baResult, t + 4);
				SetInt32Value(lPathLen + nCurPathLen + 1,
					baResult, t);

				t += 4;
		
				// path body

				//memcpy(lpResult + t,
				//	lpCurPath,
				//	nCurPathLen);
				baResult = ByteArray.EnsureSize( baResult, t + nCurPathLen);
				Array.Copy(curPathBuffer,
					0,
					baResult,
					t,
					nCurPathLen);

				t += nCurPathLen;
				//*(lpResult+t)='/';
				baResult = ByteArray.EnsureSize( baResult, t + 1);
				baResult.SetValue((byte)'/', t);
				t++;
		
				//memmove(lpResult + t,lpPackage + s,(int)lPathLen-4);
				baResult = ByteArray.EnsureSize( baResult, t + lPathLen-4);
				Array.Copy(baPackage,
					s,
					baResult,
					t,
					lPathLen-4);

                string strOldPath = Encoding.UTF8.GetString(baResult, t, lPathLen - 4);


				s +=(int)lPathLen-4;
				t +=(int)lPathLen-4;
		
		
				if (s >= (int)lPackageLen)
					break;
		
				// bao length
		
				//lBaoLen = *((long *)(lpPackage + s));
				lBaoLen = BitConverter.ToInt32(baPackage, s);
				s += 4;

				//*((long *)(lpResult+t)) = lBaoLen;
				baResult = ByteArray.EnsureSize( baResult, t + 4);	//
				SetInt32Value(lBaoLen,
					baResult, 
					t);

				t += 4;

				// bao body

				// memmove(lpResult + t,lpPackage + s,(int)lBaoLen-4);
				baResult = ByteArray.EnsureSize( baResult, t + lBaoLen - 4);
				Array.Copy(baPackage,
					s,
					baResult,
					t,
					lBaoLen-4);

				s +=(int)lBaoLen-4;
				t +=(int)lBaoLen-4;
				if (s >= (int)lPackageLen)
					break;
				//if (t >= (int)lResultMaxLen)
				//	break;
			}

			SetInt32Value((Int32)t,
				baResult, 
				0);

			// *((long *)(lpResult)) = (long)t;
			return t;
		}


		// 将路径切割为前后两个部分。
		public static void SplitPath(string strPath,
			out string strFirstPart,
			out string strOtherPart)
		{
			//string strTemp;
			int nRet;

			Debug.Assert(strPath != null, 
				"strPath参数不能为null");

			nRet = strPath.IndexOf("/",0);
			if (nRet == -1) 
			{
				strFirstPart = strPath;
				strOtherPart = "";
				return;
			}

			strFirstPart = strPath.Substring(0, nRet);
			strOtherPart = strPath.Substring(nRet+1);

		}

	
		// 将本级目录事项打包
		int LocalDir(
			int nCharset,
			out byte [] baResult)
		{
			int lPackageLen;
			int lBaoLen;
			int lPathLen;
			int lMask;
			int lMaskLen;

			// LPSTR lpBuffer;
			int cur=0;
	
			baResult = new byte [4096];
	
			if (baResult == null) 
			{
				m_lErrno = GL_OVERFLOW; // 内存不够
				return -1;
			}
	
			cur=4;
	
			// Path
			lPathLen = 5;
			Array.Copy(BitConverter.GetBytes((Int32)lPathLen), 
				0,
				baResult,
				cur,
				4);

			baResult = ByteArray.EnsureSize( baResult, cur + 4);

			cur += 4;

			// memmove(lpResult+cur,(char far *)"",1);
			baResult.SetValue((byte)0, cur);
	
			// Bao
			baResult = ByteArray.EnsureSize( baResult, cur + 1);

			cur+=1;

			byte [] baBuffer = null;

			lBaoLen = PackDirEntry(nCharset,
				out baBuffer);


			lBaoLen += 12;
			Array.Copy(BitConverter.GetBytes((Int32)lBaoLen), 
				0,
				baResult,
				cur,
				4);

			baResult = ByteArray.EnsureSize( baResult, cur + 4);

			cur += 4;

			lMaskLen = 8;

			Array.Copy(BitConverter.GetBytes((Int32)lMaskLen), 
				0,
				baResult,
				cur,
				4);

			baResult = ByteArray.EnsureSize( baResult, cur + 4);

			cur += 4;

			lMask = TypeKernel | AttrExtend;  // mask

			Array.Copy(BitConverter.GetBytes((Int32)lMask), 
				0,
				baResult,
				cur,
				4);

	
			baResult = ByteArray.EnsureSize( baResult, cur + 4);
			cur += 4;

			baResult = ByteArray.EnsureSize( baResult, cur+lBaoLen-12);

			if (baBuffer != null) 
			{
				Array.Copy(baBuffer, 
					0,
					baResult,
					cur,
					lBaoLen-12);
			}

			cur += (lBaoLen-12);
	
			// Package Len

			lPackageLen = (Int32)cur;

			Array.Copy(BitConverter.GetBytes((Int32)lPackageLen), 
				0,
				baResult,
				0,
				4);

			return lPackageLen;
		}

		int SingleDir(string strCurDir,
			int nCharset,
			out byte [] baResult)
		{
			int nCurDirLen;
			int t;

			baResult = null;

			byte[] buffer = DTLPParam.GetEncoding(nCharset).GetBytes(strCurDir);
			// 为字符串末尾增加一个0字符
			buffer = ByteArray.EnsureSize(buffer, buffer.Length + 1);
			buffer[buffer.Length -1] = 0;

			nCurDirLen = buffer.Length;
	
			t = 4;
			baResult = ByteArray.EnsureSize(baResult, t + 4);
			SetInt32Value(nCurDirLen+4,
				baResult, t);
			// *((long *)(lpResult + t))=(long)(nCurDirLen+1+4);
			t += 4;

			baResult = ByteArray.EnsureSize(baResult, t + nCurDirLen);
			Array.Copy(buffer, 0, baResult, t, nCurDirLen);
			// memmove(lpResult + t,(LPSTR)lpCurDir,nCurDirLen+1);
	
			t = 0;
			Int32 v = nCurDirLen+4  +4;
			SetInt32Value(v,
				baResult, t);
			// *((long *)(lpResult + t))=(long)(nCurDirLen+1+4  +4);
	
			return v;
		}

		int PackDirEntry(
			int nCharset,
			out byte [] baResult)
		{
			int i;
			// LPSTR pp;
			int cur = 0;
			int len;
			HostEntry entry = null;
			// CAdvString advstrText;

			baResult = null;
	
			for(i=0,cur=0;i<m_HostArray.Count;i++) 
			{
				entry = (HostEntry)m_HostArray[i];
				Debug.Assert(entry != null,
					"HostArray中出现了空的元素");

				if (entry.m_strHostName == "")
					continue;

				// Unicode --> GB
				// 假定输出不包含结尾的0字符
				byte[] buffer = 
					DTLPParam.GetEncoding(nCharset).GetBytes(entry.m_strHostName);
				// GB-2312

				/*
				byte[] buffer = Encoding.Convert(
					Encoding.Unicode,
					Encoding.GetEncoding(936),	// GB-2312
					entry.m_strHostName.ToCharArray());
				*/

		
				len = buffer.Length;
				if (len==0)
					continue;
		
				baResult = ByteArray.EnsureSize( baResult, cur+len+1);
				Array.Copy(buffer, 
					0,
					baResult,
					cur,
					len);

				// 结尾0
				baResult.SetValue((byte)0, baResult.Length - 1);
				cur += len+1;
			}
	
			return cur;
		}


		// 构造一个空的返回包
		static int NullPackage(out byte [] baResult)
		{
			int t = 0;
			Int32 l;

			baResult = null;

			t += 4;

			l = 4 + 1;   // path len

			baResult = ByteArray.EnsureSize( baResult, t + 4);
			Array.Copy(BitConverter.GetBytes((Int32)l), 
				0,
				baResult,
				t,
				4);
			t += 4;

			// path

			baResult = ByteArray.EnsureSize( baResult, t + 1);
			baResult.SetValue((byte)0, t);
			t += 1;


			l = 4 * 3;  // Bao Len

			Array.Copy(BitConverter.GetBytes((Int32)l), 
				0,
				baResult,
				t,
				4);
			t += 4;

			l = 4 * 2;   // Mask Len
			Array.Copy(BitConverter.GetBytes((Int32)l), 
				0,
				baResult,
				t,
				4);
			t += 4;

			l = AttrTcps | AttrExtend;   // Mask
			Array.Copy(BitConverter.GetBytes((Int32)l), 
				0,
				baResult,
				t,
				4);
			t += 4;

			l = (Int32)t;
			Array.Copy(BitConverter.GetBytes((Int32)l), 
				0,
				baResult,
				0,
				4);

			return t;
        }

        #region 和dt1000日志跟踪有关的实用函数

        // 将日志记录路径解析为日期、序号、偏移
        // 一个日志记录路径的例子为:
        // /ip/log/19991231/0@1234~5678
        // parameters:
        //		strLogPath		待解析的日志记录路径
        //		strDate			解析出的日期
        //		nRecID			解析出的记录号
        //		strOffset		解析出的记录偏移，例如1234~5678
        // return:
        //		-1		出错
        //		0		正确
        public static int ParseLogPath(string strLogPath,
            out string strDate,
            out int nRecID,
            out string strOffset,
            out string strError)
        {
            strError = "";
            strDate = "";
            nRecID = -1;
            strOffset = "";

            int nRet = 0;
            string strPath = "";

            strPath = strLogPath;

            nRet = strPath.LastIndexOf('@');
            if (nRet != -1)
            {
                strOffset = strPath.Substring(nRet + 1);
                strPath = strPath.Substring(0, nRet);
            }
            else
                strOffset = "";

            // number
            nRet = strPath.LastIndexOf('/');
            if (nRet != -1)
            {
                string strNumber;
                strNumber = strPath.Substring(nRet + 1);
                try
                {
                    nRecID = Convert.ToInt32(strNumber);
                }
                catch
                {
                    strError = "路径 '" + strLogPath + "' 中'" + strNumber + "'应当为纯数字";
                    return -1;
                }
                strPath = strPath.Substring(0, nRet);
            }
            else
            {
                nRecID = 0;
            }

            // date
            nRet = strPath.LastIndexOf('/');
            if (nRet != -1)
            {
                strDate = strPath.Substring(nRet + 1);
                Debug.Assert(strDate.Length == 8, "");
                strPath = strPath.Substring(0, nRet);
            }
            else
            {
                strDate = "";
            }

            return 0;
        }

        // 把dt1000工作单格式的日志记录，转换为MARC机内格式
        public static string GetDt1000LogRecord(byte[] baContent,
            Encoding encoding)
        {
            string strRecord = encoding.GetString(baContent);
            // int nRet = 0;

            // 将自然符号替换为ISO2709专用符号
            /*
            strRecord = strRecord.Replace("\r\n***\r\n",
                new string(MarcUtil.FLDEND, 1) + new string(MarcUtil.RECEND, 1));
            if (strRecord[strRecord.Length - 1] != MarcUtil.RECEND)
            {
                // 补救
                strRecord += new string(MarcUtil.RECEND, 1);
            }*/
            strRecord = strRecord.Replace("\r\n***\r\n",
                new string(MarcUtil.FLDEND, 1));



            strRecord = strRecord.Replace("\r\n", new string(MarcUtil.FLDEND, 1));
            if (strRecord.Length >= 25)
            {
                strRecord = strRecord.Remove(24, 1);	// 删除头标区后第一个FLDEND
            }

            /*
            // 如果倒数第二个字符不是FLDEND，则插入一个
            int nLen;
            nLen = strRecord.Length;
            if (nLen >= 2)
            {
                if (strRecord[nLen - 2] != MarcUtil.FLDEND)
                    strRecord = strRecord.Insert(nLen - 1, new string(MarcUtil.FLDEND, 1));
            }*/

            // 如果倒数第一个字符不是FLDEND，则插入一个
            if (strRecord[strRecord.Length - 1] != MarcUtil.FLDEND)
            {
                strRecord += new string(MarcUtil.FLDEND, 1);
            }

            return strRecord;
        }

        // 解析dt1000日志记录中的的要害参数
        /*
#define LOG_APPEND		0	// 表示追加记录
#define LOG_OVERWRITED	1	// 表示覆盖操作删除的旧记录
#define LOG_DELETE		2	// 表示删除记录
#define LOG_DESTROY_DB	12	// 表示初始化数据库
 * */
        // parameters:
        //      strOperPath .rz字段$a子字段内容，路径。不过，如果strOperCode为“12”时，strOperPath中返回的只是数据库名
        public static int ParseDt1000LogRecord(string strMARC,
            out string strOperCode,
            out string strOperComment,
            out string strOperPath,
            out string strError)
        {
            strError = "";

            strOperComment = "";
            strOperPath = "";
            strOperCode = "";

            string strField = "";
            string strNextFieldName = "";

            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            int nRet = MarcUtil.GetField(strMARC,
                ".rz",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == -1)
            {
                strError = "get field '.rz' failed...";
                goto ERROR1;
            }
            if (nRet == 0)
            {
                strError = "field '.rz' not found...";
                goto ERROR1;
            }


            if (strField.Length < 5)
            {
                strError = "'.rz'字段长度小于5...";
                goto ERROR1;
            }

            strOperCode = strField.Substring(3, 2);


            if (strOperCode == "00")
                strOperComment = "追加记录";
            else if (strOperCode == "01")
                strOperComment = "被覆盖的记录";
            else if (strOperCode == "02")
                strOperComment = "删除记录";
            else if (strOperCode == "12")
                strOperComment = "初始化数据库";
            else
            {
                strOperComment = "不能识别的操作类型 '" + strOperCode + "'";
                goto ERROR1;
            }

            string strSubfield = "";
            string strNextSubfieldName = "";

            // 获得$a子字段

            // 从字段或子字段组中得到一个子字段
            // parameters:
            //		strText		字段内容，或者子字段组内容。
            //		textType	表示strText中包含的是字段内容还是组内容。若为ItemType.Field，表示strText参数中为字段；若为ItemType.Group，表示strText参数中为子字段组。
            //		strSubfieldName	子字段名，内容为1位字符。如果==null，表示任意子字段
            //					形式为'a'这样的。
            //		nIndex			想要获得同名子字段中的第几个。从0开始计算。
            //		strSubfield		[out]输出子字段。子字段名(1字符)、子字段内容。
            //		strNextSubfieldName	[out]下一个子字段的名字，内容一个字符
            // return:
            //		-1	出错
            //		0	所指定的子字段没有找到
            //		1	找到。找到的子字段返回在strSubfield参数中

            nRet = MarcUtil.GetSubfield(strField,
                DigitalPlatform.Marc.ItemType.Field,
                "a",
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (nRet == -1)
            {
                strError = "获得.rz字段中的$a子字段时出错";
                goto ERROR1;
            }
            if (nRet == 0)
            {
                strError = ".rz字段中的$a子字段没有找到";
                goto ERROR1;
            }

            if (strSubfield.Length < 1)
            {
                strError = ".rz字段中的$a子字段内容为空";
                goto ERROR1;
            }

            string strContent = strSubfield.Substring(1);

            if (strOperCode == "12")
            {
                nRet = strContent.IndexOf("/");

                string strDbName = "";

                // 库名
                if (nRet == -1)
                {
                    // 这种情况是否要当作数据残缺处理？
                    strDbName = strContent;
                }
                else
                    strDbName = strContent.Substring(0, nRet);

                // '/'后面的数据库内部代号被放弃不用
                strOperPath = strDbName;
            }
            else
            {
                strOperPath = strContent;
            }

            return 0;
        ERROR1:

            return -1;
        }


        #endregion

        // 解析保存路径
        // return:
        //      -1  出错
        //      0   成功
        public static int ParseWritePath(string strPathParam,
            out string strServerAddr,
            out string strDbName,
            out string strNumber,
            out string strError)
        {
            strError = "";
            strServerAddr = "";
            strDbName = "";
            strNumber = "";

            string strPath = strPathParam;

            int nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strServerAddr = strPath;
                return 0;
            }

            strServerAddr = strPath.Substring(0, nRet);

            strPath = strPath.Substring(nRet + 1);

            // 库名
            nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strDbName = strPath;
                return 0;
            }

            strDbName = strPath.Substring(0, nRet);

            strPath = strPath.Substring(nRet + 1);


            string strTemp = "";

            // '记录索引号'汉字
            nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strTemp = strPath;
                return 0;
            }

            strTemp = strPath.Substring(0, nRet);

            if (strTemp != "ctlno" && strTemp != "记录索引号")
            {
                strError = "路径 '" + strPathParam + "' 格式不正确";
                return -1;
            }

            strPath = strPath.Substring(nRet + 1);

            // 号码
            nRet = strPath.IndexOf('/');
            if (nRet == -1)
            {
                strNumber = strPath;
                return 0;
            }

            strNumber = strPath.Substring(0, nRet);

            return 0;
        }

        // 正规化保存路径
        // return:
        //      -1  error
        //      0   为覆盖方式的路径
        //      1   为追加方式的路径
        public static int CanonicalizeWritePath(string strPath,
            out string strOutPath,
            out string strError)
        {
            strError = "";
            strOutPath = "";

            string strServerAddr = "";
            string strDbName = "";
            string strNumber = "";
            int nRet = ParseWritePath(strPath,
                out strServerAddr,
                out strDbName,
                out strNumber,
                out strError);
            if (nRet == -1)
                return -1;

            if (strServerAddr == "")
            {
                strError = "缺乏服务器名部分";
                return -1;
            }

            if (strDbName == "")
            {
                strError = "缺乏数据库名部分";
                return -1;
            }

            if (strNumber == "")
            {
                strNumber = "?";    // 表示追加
            }

            if (strNumber == "?")
            {
                // 为了避免dt1000/dt1500某个版本的保存后返回的路径还带有问号，所以要构造成下列没有问号形式的路径
                strOutPath = strServerAddr + "/" + strDbName;
                return 1;   // 表示为追加方式的路径
            }
            else
                strOutPath = strServerAddr + "/" + strDbName + "/记录索引号/" + strNumber;

            return 0;   // 表示为普通覆盖方式的路径
        }
    }



}
