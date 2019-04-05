using DigitalPlatform.Core;
using System;
using System.Collections;
using System.Diagnostics;

using System.Text;

namespace DigitalPlatform.DTLP
{

	// 通讯包中的一个单元
	public class Cell
	{
		public string	Path = null;	// m_strPath 前导部分和枚举部分的组合
		public string	Lead = null;	// m_strLead 前导部分
		public string	Content = null;	// m_strContent 枚举部分
		public byte []	ContentBytes = null;	// m_baContent
		// public int		m_nContentCharset;
		public Int32	Mask = 0;		// m_lMask
	}

	public enum PackageFormat 
	{
		String = 1,
		Binary = 2,
	}

	/// <summary>
	/// 处理通讯包的类
	/// </summary>
	public class Package : ArrayList
	{

		byte[]	m_baPackage;	// 通讯包

		public string	ContinueString = "";	// m_strNext
		public byte[]	ContinueBytes = null;	// m_baNext 为兼容dt1000/dt1500 ansi版本的内核，半个汉字问题

		Encoding	m_encoding	= Encoding.GetEncoding(936);

		public Package()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// 装载包内容
		public int LoadPackage(byte [] baPackage, 
			Encoding encoding)
		{
			Int32 lPackageLen;

			m_baPackage = null;

			if (baPackage == null) 
			{
				Debug.Assert(false,
					"baPackage参数不能为null");
				return -1;
			}

			if (baPackage.Length < 4) 
			{
				Debug.Assert(false,
					"baPackage内容尺寸小于4，不正确");
				return -1;
			}

			lPackageLen =  BitConverter.ToInt32(baPackage, 0);
			if (lPackageLen < 0) 
			{
				Debug.Assert(false,
					"baPackage内容尺寸小于0，不正确");
				m_baPackage = null;
				return -1;
			}

			if ( baPackage.Length < lPackageLen) 
			{
				// 通讯包格式存在严重错误，头部定义的长度大于实际长度
				Debug.Assert(false,
					"通讯包格式存在严重错误，头部定义的长度大于实际长度");
				m_baPackage = null;
				return -1;
			}

			m_baPackage = new byte [lPackageLen];
			Array.Copy(baPackage, 0, m_baPackage, 0, lPackageLen);

			m_encoding = encoding;

			return 0;
		}

		// 测算ansi字符集字符串的长度
		public static int strlen(byte [] baContent, int nOffs)
		{
			int nResult = 0;
			for(int i=nOffs;i<baContent.Length;i++) 
			{
				if (baContent[i] == 0)
					return nResult;
				nResult ++;
			}

			return nResult;
		}



		// 将通讯包转换为便于处理的行格式，放入m_LineArray中
		// 注意本函数需在LoadPackage()函数后使用。
		// 本函数把子包中的内容当作C语言字符串处理，将一个子包拆分为若干个字符串。
		// 返回-1表示失败。
		public int Parse(PackageFormat format)
		{
			Int32 lPackageLen;

			Int32 lBaoLen;
			Int32 lPathLen;
			int i;
			int wholelen,len;
			//char far *lpBao;
			//char far *lpPath;
			int j;
			Int32 lMask;
	
			// char *src = (char *)m_baPackage.GetData();

			Cell cell = null;
			int nOffs = 0;
			//byte [] baPath = null;
			//byte [] baBao = null;
			int nPathStart = -1;
			int nBaoStart = -1;
	
			this.Clear();
			ContinueString = "";
			ContinueBytes = null;

			lPackageLen =  BitConverter.ToInt32(m_baPackage, 0);

			Debug.Assert( lPackageLen == m_baPackage.Length,
				"包头部尺寸不正确");
	
//			pp = src + 4;
			nOffs += 4;
			Int32 lWholeLen = 4;
			for(i=0;;i++) 
			{
				lPathLen = BitConverter.ToInt32(m_baPackage, nOffs);

				Debug.Assert(lPathLen < 1000, "lPathLen不正确");

				// lpPath = pp + 4;
				nPathStart = nOffs + 4;
		
				nOffs += lPathLen;

				lWholeLen += lPathLen;

				if (lWholeLen >= lPackageLen)
					break;

				if (lWholeLen == lPackageLen) 
				{
					// 没有枚举部分
					lBaoLen = 0;
					lMask = 0;
				}
				else 
				{
					lBaoLen = BitConverter.ToInt32(m_baPackage, nOffs);
					lMask = BitConverter.ToInt32(m_baPackage, nOffs + 8);
				}

				if ((lMask & DtlpChannel.TypeBreakPoint) != 0) 
				{
					if (lBaoLen > 12) 
					{
						// lpBao = pp+12;

						len = strlen(m_baPackage, nOffs + 12) + 1;

						ContinueString = Encoding.GetEncoding(936).GetString(m_baPackage, nOffs+12, len-1);

						// 兼容模块
						ContinueBytes = new byte[len-1];
						Array.Copy(m_baPackage, nOffs+12, ContinueBytes, 0, len -1);

					}
					goto SKIP;
				}
		
				//lpBao = pp+12;  // 8
				if (lWholeLen == lPackageLen) 
					nBaoStart = -1;
				else 
					nBaoStart = nOffs + 12;

				// 将通讯包转换为以子包为单元的格式，放入m_LineArray中
				// 本函数不把子包中的内容当作C语言字符串处理。
				// 注意! pCell->ContentBytes中字符集未作转换(其它成员已经转换)。可能有以下几种形式：
				//	1)C语言字符串形式。DBCS/UTF8字符集。
				//	2)MARC记录。前面9字节为二进制内容，后面为C语言字符串。这样，就不能把整个ContentBytes
				//		当作一个字符串进行翻译，因为前面9字节中间可能包含0字符，将导致字符串终止。
				//		时间戳和MARC记录在一起的用法现在看来是一个大败笔。
				// 返回-1表示失败。

				if (format == PackageFormat.Binary) 
				{
					// lMask
					cell = new Cell();

					this.Add(cell);

					cell.Mask = lMask;
			
					if (lPathLen > 0 && m_baPackage[nPathStart] != 0) 
					{
						Debug.Assert(nPathStart!=-1,
							"nPathStart尚未初始化");

						Debug.Assert(strlen(m_baPackage, nPathStart) == lPathLen-4-1,
							"lPathLen值不正确");

						cell.Path += m_encoding.GetString(m_baPackage, nPathStart, lPathLen-4-1);
						cell.Lead += m_encoding.GetString(m_baPackage, nPathStart, lPathLen-4-1);

						/*
						// 最后一个字符不是'/'。即便为UTF8字符集，这里仍可以使用DBCS判断法。
						if (cell.Path.Length !=0
							&& cell.Path[Math.Max(0, cell.Path.Length-1)] != '/' )
							cell.Path += "/";
						*/

					}


					if (lWholeLen == lPackageLen) 
						break;

					if (lBaoLen <= 12)
						goto SKIP;

					// 枚举部分，当作一个整体
					Debug.Assert( lBaoLen >= 12 ,
						"lBaoLen不正确");

					if (lBaoLen > 12) // 正好== 12，不做
					{
						cell.ContentBytes  = new byte[lBaoLen - 12];

						Array.Copy(m_baPackage, nBaoStart, 
							cell.ContentBytes, 0, lBaoLen - 12);
					}

				}

				// nOffs += 12;

				if (lBaoLen <= 12)	
					goto SKIP;
		
				if (format == PackageFormat.String) 
				{
					for(len=0,wholelen=0,j=0;;j++) 
					{
			
						// lMask
						cell = new Cell();

						this.Add(cell);

						cell.Mask = lMask;
			
						if (lPathLen > 0 && m_baPackage[nPathStart] != 0) 
						{
							Debug.Assert(nPathStart!=-1,
								"nPathStart尚未初始化");

							Debug.Assert(strlen(m_baPackage, nPathStart) == lPathLen-4-1,
								"lPathLen值不正确");

							cell.Path += m_encoding.GetString(m_baPackage, nPathStart, lPathLen-4-1);
							cell.Lead += m_encoding.GetString(m_baPackage, nPathStart, lPathLen-4-1);

							// 最后一个字符不是'/'。即便为UTF8字符集，这里仍可以使用DBCS判断法。
							if (cell.Path.Length !=0
								&& cell.Path[Math.Max(0, cell.Path.Length-1)] != '/' )
								cell.Path += "/";

						}

						/*
						if (*lpPath) 
						{  // jia
							ASSERT(strlen(lpPath)==(unsigned int)lPathLen-4L-1L);
				
							if (nSrcCharset == CHARSET_UTF8) 
							{
								CAdvString advstrPath;
								advstrPath.SetString(lpPath,
									nSrcCharset == CHARSET_UTF8 ? _CHARSET_UTF8 : _CHARSET_DBCS);
								pCell->Path += (LPCTSTR)advstrPath; 
								pCell->Lead += (LPCTSTR)advstrPath;
							}
							else 
							{
								// DBCS时间优化，避免一次多余的复制
								pCell->Path += (LPCSTR)lpPath; 
								pCell->Lead += (LPCSTR)lpPath;
							}

				
							// 最后一个字符不是'/'。即便为UTF8字符集，这里仍可以使用DBCS判断法。
							if ( strlen(lpPath)!=0 && 
								*(lpPath+max(0,strlen(lpPath)-1))!='/' )
								pCell->Path += _T("/");
				
						}
						*/
			
						if (lBaoLen <= 12L) 
							break;



						len = strlen(m_baPackage, nBaoStart) + 1;

						cell.Path += m_encoding.GetString(m_baPackage, nBaoStart, len-1);
						cell.Content += m_encoding.GetString(m_baPackage, nBaoStart, len-1);


						wholelen += len;
						if (wholelen >= lBaoLen-12) // 8
							break;

						nBaoStart += len;
			
					}
				}


		
			SKIP:
				//pp+= lBaoLen;
				nOffs += lBaoLen;
		
				lWholeLen += lBaoLen;
				if (lWholeLen >= lPackageLen)
					break;
			}
	
			return i; // lTgtPos
		}



		// 得到包中第一个子包的内容部分
		// 注意需在函数Parse(PackageFormat.Binary)后使用，否则没有内容可处理。
		// return:
		//		-1	error
		//		0	not found
		//		1	found
        public int GetFirstBin(out byte[] baContent)
        {
            Cell cell;
            baContent = null;

            if (this.Count == 0)
                return 0;	// not found

            cell = (Cell)this[0];

            Debug.Assert(cell.ContentBytes != null, "");

            baContent = ByteArray.EnsureSize(baContent,
                cell.ContentBytes.Length);
            Array.Copy(cell.ContentBytes, 0, baContent, 0,
                cell.ContentBytes.Length);
            return 1;
		}


		public string GetFirstPath()
		{
            Cell cell;

            if (this.Count == 0)
                return "";

				cell = (Cell)this[0];
				return cell.Path;
		}

		public string GetFirstContent()
		{
			if (this.Count == 0)
				return "";	// not found
			Cell cell = (Cell)this[0];
			return cell.Content;
		}
	}

	//renyh edit
	//xietao edit
	// line3
}
