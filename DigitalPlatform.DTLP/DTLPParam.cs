using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Net;
using DigitalPlatform.Core;

namespace DigitalPlatform.DTLP
{

	// 一个参数对象
	public class Param
	{

		public int	m_nStyle = 0;
		public Int32	m_lValue = 0;
		public byte []	m_baValue = null;

		public const int STYLE_INT = 0;	// 16位整数
		public const int STYLE_LONG = 1;	// 32位整数
		public const int STYLE_BUFF = 2;	// 缓冲区
		public const int STYLE_STRING = 3;	// 字符串
		public const int STYLE_END = -1;	// 结束

		public Param() 
		{

		}

        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
		~Param() 
		{
		}

	}


	/// <summary>
	/// 参数数组
	/// </summary>
	public class DTLPParam : ArrayList
	{
		public DTLPParam()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
		~DTLPParam()
		{
		}

		void RemoveAll()
		{
			this.Clear();
		}

		
		// define a Parameter's Style
		public int DefPara(int nStyle)
		{
			Param param = null;
			if ( nStyle != Param.STYLE_BUFF
				&& nStyle != Param.STYLE_INT
				&& nStyle != Param.STYLE_LONG ) 
			{
				Debug.Assert(false, "style error");
				return -1;  // style error
			}
	
			param = new Param();
			if (param == null) 
			{
				Debug.Assert(false, "new Param object fail ...");
				return -1;
			}
			this.Add(param);

			param.m_nStyle = nStyle;
	
			return 0;
		}

		// 从字符数组中指定位置开始取4byte转换为32位整数
		public static Int32 GetInt32(byte [] source,
			int nStartIndex)
		{
			/*
			char [] dest = new char[4];

			Array.Copy(
					 source,
					 nIndex,
					 dest,
					 0,
					 4
					 );
			*/

			return BitConverter.ToInt32(source, nStartIndex);

		}


		/*
		* Convert a Package to a Para Table
		*
		* Prefered Usage:
		* BeginPara()
		*   DefPara()
		*   DefPara()
		*   DefPara()
		*   DefPara()
		*   ...
		* PackageToPara()
		* EndPara()
		*
		* return : Parameter Count
		*/
		public int PackageToPara(byte [] baPackage,
			ref int lErrno,
			out int nFuncNum)
		{
			//char *buff=NULL;
			int lParaCount;
			//		char far *pp;
			Int16 i;
			int lParaLen;
			int lParaValue;
			Int16 nParaValue;
			//	char far *lpTarget;
			Param param = null;
	
			Debug.Assert(baPackage != null, "baPackage参数不能为空");
			//		buff = (char *)pPackage;
	
			// 0-3     4-7      8-11       12-15 16...
			// rec_len func_num para_count errno para's
	

			// 取得参数总数
			lParaCount = BitConverter.ToInt32(baPackage, 8);
			lParaCount = IPAddress.NetworkToHostOrder(lParaCount);


			// 取得功能号
			nFuncNum = BitConverter.ToInt32(baPackage, 4);
			nFuncNum = IPAddress.NetworkToHostOrder(nFuncNum);


			// 取得错误码
			lErrno = BitConverter.ToInt32(baPackage, 12);
			lErrno = IPAddress.NetworkToHostOrder(lErrno);
	
	
			/*
			// 修正参数个数 2005/4/14
			if (lParaCount != this.Count) 
			{
				lParaCount = this.Count;
			}
			*/
	
	
			int nOffs = 16;
			//pp=buff+16;
	
			for(i=0;i<lParaCount;i++) 
			{
				param = (Param)this[i];
				if (param == null) 
				{
					Debug.Assert(false, "Param数组中出现空元素");
					continue;
				}

				// 取得参数尺寸
				lParaLen = BitConverter.ToInt32(baPackage, nOffs);
				lParaLen = IPAddress.NetworkToHostOrder(lParaLen);
		
				if (param.m_nStyle == Param.STYLE_INT) // 16位整数
				{
					if (lParaLen != 2)  // para length incorrect
						goto ERROR1;
					// 取出参数值,2byte 16位整数
					nParaValue = BitConverter.ToInt16(baPackage, nOffs + 4);
					nParaValue = IPAddress.NetworkToHostOrder((Int16)nParaValue);
					param.m_lValue = (Int32)nParaValue;
					param.m_baValue = null;
					nOffs += 4+2;
				}
				else if (param.m_nStyle == Param.STYLE_LONG) // 32位整数
				{
					if (lParaLen != 4L) // para length incorrect
						goto ERROR1;
					// 取出参数值,4byte 32位整数
					lParaValue = BitConverter.ToInt32(baPackage, nOffs + 4);
					lParaValue = IPAddress.NetworkToHostOrder((Int32)lParaValue);
					param.m_lValue = lParaValue;
					param.m_baValue = null;
					nOffs += 4+4;
				}
				else if (param.m_nStyle == Param.STYLE_BUFF) // 缓冲区
				{

					param.m_lValue = lParaLen;	// 存储缓冲区尺寸
					if (param.m_baValue != null) 
					{
						param.m_baValue = null;
					}

					param.m_baValue = new byte [Math.Max(lParaLen, 4096)];	// 取整块，避免内存碎片?
					if (param.m_baValue == null) 
					{
						Debug.Assert(false, "分配缓冲区失败");
						goto ERROR1;
					}

					Array.Copy(baPackage, nOffs+4, param.m_baValue, 0 , lParaLen);
					nOffs  += 4 + lParaLen;
				}
				else 
				{
					Debug.Assert(false, "未知的Param风格");
				}
		
			}
	
			return lParaCount;
	
			ERROR1:
				return -1;
		}


		// Int32
		//*  Add an integer to ParaTable
		public int ParaInt(Int32 nValue)
		{
			Param param = new Param();
			if (param == null) 
			{
				Debug.Assert(false, "new Param object fail");
				return -1;
			}
			this.Add(param);

			param.m_nStyle = Param.STYLE_INT;
			param.m_lValue = nValue;
			return 2;
		}


		// Int32
		//  Add a long to ParaTable
		public int ParaLong(Int32 lValue)
		{
			Param param = new Param();
			this.Add(param);

			param.m_nStyle = Param.STYLE_LONG;
			param.m_lValue = lValue;
			return 4;
		}


		//  Add a Buffer to ParaTable
		public int ParaBuff(byte [] baBuffer,
			int lLen)
        {
			Debug.Assert(baBuffer != null, "baBuffer参数不能为空");
			Debug.Assert(lLen >= 0, "lLen参数值不能为0或负数");

			Param param = new Param();
			this.Add(param);

			param.m_nStyle = Param.STYLE_BUFF;
			param.m_lValue = lLen;

			param.m_baValue = new byte [Math.Max(lLen, 4096)];
            if (param.m_baValue == null) 
			{
				Debug.Assert(false, "分配缓冲区失败");
                return -1;
			}

			Array.Copy(baBuffer, 0, param.m_baValue, 0, lLen);
			return lLen;
		}


		public static Encoding GetEncoding(int nCharset)
		{
			if (nCharset == DtlpChannel.CHARSET_DBCS)
				return Encoding.GetEncoding(936);
			if (nCharset == DtlpChannel.CHARSET_UTF8)
				return Encoding.UTF8;
			Debug.Assert(false, "不支持的编码方式");
			return null;
		}

		// Add a Buffer (string) to ParaTable
		public int ParaString(string strBuffer,
			int nCharset)
		{
			Debug.Assert(strBuffer!=null, "strBuffer参数不能为空");

			// 这里负责把字符串翻译为特定编码

			byte[] buffer = GetEncoding(nCharset).GetBytes(strBuffer);
				// GB-2312

			// 为字符串末尾增加一个0字符
			buffer = ByteArray.EnsureSize(buffer, buffer.Length + 1);
			buffer[buffer.Length -1] = 0;

			/*
			byte[] buffer = Encoding.Convert(
				Encoding.Unicode,
				Encoding.GetEncoding(936),	// GB-2312
				strBuffer.ToCharArray);
			*/

			return ParaBuff(buffer, buffer.Length);
		}


		// 特殊版本
		// Add a Buffer (string) to ParaTable
		// 将路径和next字符串用"||"间隔拼接起来
		public int ParaPathString(string strBuffer,
			int nCharset,
			byte [] baNext)
		{
			Debug.Assert(strBuffer!=null, "strBuffer参数不能为空");
			// 谁负责把字符串翻译为Ansi字符集？

			strBuffer += "||";
			byte[] buffer = GetEncoding(nCharset).GetBytes(strBuffer);
			// GB-2312

			if (baNext != null) 
			{
				int nOldLength = buffer.Length;
				buffer = ByteArray.EnsureSize(buffer, buffer.Length + baNext.Length);

				Array.Copy(baNext,0,buffer,nOldLength,baNext.Length);
			}

			// 为字符串末尾增加一个0字符
			buffer = ByteArray.EnsureSize(buffer, buffer.Length + 1);
			buffer[buffer.Length -1] = 0;

			return ParaBuff(buffer, buffer.Length);
		}


		// 将long val值放入pp指向的缓冲区的4字节
		// ArrayList版本
		static int addr_long(ArrayList aPackage,
			Int32 val)
		{
			val= IPAddress.HostToNetworkOrder(val);

			// 追加在数组末尾
			aPackage.AddRange(BitConverter.GetBytes(val));
			return 0;
		}

		static int addr_long(ref byte[] aPackage,
			int nStart,
			Int32 val)
		{
			val= IPAddress.HostToNetworkOrder(val);

			byte[] baVal = BitConverter.GetBytes(val);

			// 确保尺寸足够
			aPackage = ByteArray.EnsureSize(aPackage, nStart + baVal.Length);

			Array.Copy(baVal, 0, aPackage, nStart, baVal.Length);
			return 0;
		}

		// 将int val值放入pp指向的缓冲区2字节
		// ArrayList版本
		static int addr_int(ArrayList aPackage,
			Int16 val)
		{
			val=IPAddress.HostToNetworkOrder(val);

			// 追加在数组末尾
			aPackage.AddRange(BitConverter.GetBytes(val));
			return 0;
		}

		static int addr_int(ref byte[] aPackage,
			int nStart,
			Int16 val)
		{
			val=IPAddress.HostToNetworkOrder(val);

			byte[] baVal = BitConverter.GetBytes(val);

			// 确保尺寸足够
			aPackage = ByteArray.EnsureSize(aPackage, nStart + baVal.Length);

			Array.Copy(baVal, 0, aPackage, nStart, baVal.Length);

			return 0;
		}

		// 将buffer val值放入pp指向的缓冲区n字节
		// ArrayList版本
		static int addr_buff(ArrayList aPackage,
			byte []baBuffer,
			int nLen)
		{
			// 追加在数组末尾
			if (nLen == baBuffer.Length)
				aPackage.AddRange(baBuffer);
			else 
			{
				char [] buffer = new char [nLen];
				Array.Copy(baBuffer, 0, buffer, 0 , nLen);
				aPackage.AddRange(buffer);
			}

			return 0;
		}

		static int addr_buff(ref byte[] aPackage,
			int nStart,
			byte []baBuffer,
			int nLen)
		{

			// 确保尺寸足够
			aPackage = ByteArray.EnsureSize(aPackage, nStart + nLen);

			Array.Copy(baBuffer, 0, aPackage, nStart, nLen);
			return 0;
		}

		/*
		* Convert a Para Table to a Package
		*
		* Prefered Usage:
		* BeginPara()
		*   ParaLong()
		*   ParaLong()
		*   ParaBuff()
		*   ParaInt()
		*   ...
		* ParaToPackage()
		* EndPara()
		*
		*  return : Package Length
		*/
		public int ParaToPackage(int lFuncNum,
			int lErrno,
			out byte []aTarget)
		{
			int i;                 // 循环计数
			int par_count = 0;     // 参数总计数
			int whole_len;        // 通讯包的总长度
			int slen;
			Param param = null;
	
			aTarget = null;
			if (this.Count == 0)
				return -1;

			// byte[] aTarget = null;
			//ArrayList aTarget = new ArrayList();
	
			//sbuff = pPackage;
	
			// 0-3     4-7      8-11       12-15  16...
			// rec_len func_num para_count errno  para's
	
			//pp=sbuff+16;

			//baPackage.SetSize(sizeof(long)*4, CHUNK_SIZE);
			// aTarget.AddRange(new char[4*4]);	// 占住4*4个byte

			aTarget = ByteArray.EnsureSize(aTarget, 4*4);	// 占住4*4个byte
	
			for(i=0;i<this.Count;i++) {
				param = (Param)this[i];
				if (param == null) {
					Debug.Assert(false, "Param数组中出现空元素");
					continue;
				}
				if (param.m_nStyle == Param.STYLE_INT) {
                    addr_long(ref aTarget, aTarget.Length, 2);
					addr_int(ref aTarget, aTarget.Length, (Int16)(param.m_lValue));
				}
				else if (param.m_nStyle == Param.STYLE_LONG) {
					addr_long(ref aTarget, aTarget.Length, 4);
					addr_long(ref aTarget, aTarget.Length, (Int32)param.m_lValue);
				}
				else if (param.m_nStyle == Param.STYLE_BUFF) {
					addr_long(ref aTarget, aTarget.Length, param.m_lValue);  // length
					addr_buff(ref aTarget, aTarget.Length, param.m_baValue,
						param.m_lValue);
				}
				else {
					Debug.Assert(false, "未知的参数类型");	// 未知的参数类型
				}
		
			}
	
			// 计算通讯包的总长度
			whole_len = aTarget.Length;
	
			// 反馈给调用者
			slen = whole_len;
			whole_len = IPAddress.HostToNetworkOrder((Int32)whole_len);

			// 设置整个包的长度
			Array.Copy(BitConverter.GetBytes((Int32)whole_len),
				0,
				aTarget,
				0, 
				4);


			// 设置功能号
			lFuncNum = IPAddress.HostToNetworkOrder((Int32)lFuncNum);
			Array.Copy(BitConverter.GetBytes((Int32)lFuncNum),
				0,
				aTarget,
				4, 
				4);

	
			// 设置参数个数
			par_count = this.Count;
			par_count = IPAddress.HostToNetworkOrder((Int32)par_count);
			Array.Copy(BitConverter.GetBytes((Int32)par_count),
				0,
				aTarget,
				8, 
				4);

			// 设置错误码
			lErrno=IPAddress.HostToNetworkOrder((Int32)lErrno);
			Array.Copy(BitConverter.GetBytes((Int32)lErrno),
				0,
				aTarget,
				12, 
				4);

			// 最终复制给输出参数
			/*
			baPackage = new byte[aTarget.Count];
			aTarget.CopyTo(baPackage);
			*/
	
			return slen;
		}

		public byte [] baValue(int nIndex)
		{
			Debug.Assert(nIndex >= 0 && nIndex < this.Count,
				"nIndex参数非法");

			return ((Param)this[nIndex]).m_baValue;
		}

		public Int32 lValue(int nIndex)
		{
				Debug.Assert(nIndex >= 0 && nIndex < this.Count,
					"nIndex参数非法");

				return ((Param)this[nIndex]).m_lValue;
		}



	}
}
