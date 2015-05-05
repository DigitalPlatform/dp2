using System;
using System.Diagnostics;
using System.Text;

namespace DigitalPlatform.Marc
{
	// ISO2709ANSIHEADER结构定义
	// ISO2709头标区结构
	// charset: 按照ANSI字符集存储，尺寸固定，适用于DBCS/UTF-8/MARC-8情形
	public class MarcHeaderStruct
	{
		byte[] reclen	= new byte[5];				// 记录长度
		byte[] status	= new byte[1];
		byte[] type		= new byte[1];
		byte[] level	= new byte[1];
		byte[] control	= new byte[1];
		byte[] reserve	= new byte[1];
		byte[] indicount	= new byte[1];			// 字段指示符长度
		byte[] subfldcodecount	= new byte[1];	// 子字段标识符长度
		byte[] baseaddr	= new byte[5];			// 数据基地址
		byte[] res1		= new byte[3];
		byte[] lenoffld	= new byte[1];			// 目次区中字段长度部分
		byte[] startposoffld	= new byte[1];		// 目次区中字段起始位置部分
		byte[] impdef	= new byte[1];				// 实现者定义部分
		byte[] res2		= new byte[1];

		// 按照UNIMARC惯例强制填充ISO2709头标区
		public int ForceUNIMARCHeader()
		{
			indicount[0] = (byte)'2';
			subfldcodecount[0] = (byte)'2';
			lenoffld[0] = (byte)'4';   // 目次区中字段长度部分
			startposoffld[0] = (byte)'5'; // 目次区中字段起始位置部分

			return 0;
		}


		public static string StringValue(byte[] baValue)
		{
			Encoding encoding = Encoding.UTF8;
			return encoding.GetString(baValue);
		}

		public static int IntValue(byte[] baValue)
		{
			Encoding encoding = Encoding.UTF8;
			return Convert.ToInt32(encoding.GetString(baValue));
		}

		public static int IntValue(byte[] baValue, 
			int nStart,
			int nLength)
		{
			Encoding encoding = Encoding.UTF8;
			byte[] baTemp = new byte[nLength];
			Array.Copy(baValue, nStart, baTemp, 0, nLength);
			return Convert.ToInt32(encoding.GetString(baTemp));
		}

		// 记录长度
		public int RecLength
		{
			get
			{
				return IntValue(reclen);
			}
			set 
			{
				string strText = Convert.ToString(value);
				strText = strText.PadLeft(reclen.Length, '0');
				reclen = Encoding.UTF8.GetBytes(strText);
			}
		}

		// 记录长度 字符串
		public string RecLengthString
		{
			get
			{
				return StringValue(reclen);
			}
		}

		// 数据基地址
		public int BaseAddress
		{
			get
			{
				return IntValue(baseaddr);
			}
			set 
			{
				string strText = Convert.ToString(value);
				strText = strText.PadLeft(baseaddr.Length, '0');
				baseaddr = Encoding.UTF8.GetBytes(strText);
			}
		}

		// 数据基地址 字符串
		public string BaseAddressString
		{
			get
			{
				return StringValue(baseaddr);
			}
		}

		// 目次区中表示字段长度要占用的字符数
		public int WidthOfFieldLength
		{
			get
			{
				return IntValue(lenoffld);
			}
		}

		// 字符串：目次区中表示字段长度要占用的字符数
		public string WidthOfFieldLengthString
		{
			get
			{
				return StringValue(lenoffld);
			}
		}

		public int WidthOfStartPositionOfField
		{
			get
			{
				return IntValue(startposoffld);
			}
		}
			
		// string版本
		public string WidthOfStartPositionOfFieldString
		{
			get
			{
				return StringValue(startposoffld);
			}
		}

        public MarcHeaderStruct(Encoding encoding,
            byte[] baRecord)
        {
            if (baRecord.Length < 24)
            {
                throw (new ArgumentException("baRecord中字节数少于24"));
            }

            bool bUcs2 = false;

            if (encoding != null
                && encoding.Equals(Encoding.Unicode) == true)
                bUcs2 = true;

            if (bUcs2 == true)
            {
                // 先把baRecord转换为ANSI类型的缓冲区
                string strRecord = encoding.GetString(baRecord);

                baRecord = Encoding.ASCII.GetBytes(strRecord);
            }

            Array.Copy(baRecord,
                0,
                reclen, 0,
                5);
            Array.Copy(baRecord,
                5,
                status, 0,
                1);
            Array.Copy(baRecord,
                5 + 1,
                type, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1,
                level, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1,
                control, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1,
                reserve, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1,
                indicount, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1,
                subfldcodecount, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1 + 1,
                baseaddr, 0,
                5);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5,
                res1, 0,
                3);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3,
                lenoffld, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3 + 1,
                startposoffld, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3 + 1 + 1,
                impdef, 0,
                1);
            Array.Copy(baRecord,
                5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3 + 1 + 1 + 1,
                res2, 0,
                1);
        }

		public MarcHeaderStruct(byte[] baRecord)
		{
			if (baRecord.Length < 24) 
			{
				throw(new Exception("baRecord中字节数少于24"));
			}

            bool bUcs2 = false;
            if (baRecord[0] == 0
                || baRecord[1] == 0)
            {
                bUcs2 = true;
            }

            if (bUcs2 == true)
            {
                throw new Exception("应用构造函数的应外一个版本，才能支持UCS2编码方式");
            }

			Array.Copy(baRecord,
				0,
				reclen,	0,
				5);
			Array.Copy(baRecord,
				5,
				status, 0,
				1);
			Array.Copy(baRecord,
				5 + 1,
				type, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1,
				level, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1,
				control, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1,
				reserve, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1,
				indicount, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1,
				subfldcodecount, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1 + 1,
				baseaddr, 0,
				5);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5,
				res1, 0,
				3);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3,
				lenoffld, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3 + 1,
				startposoffld, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3 + 1 + 1,
				impdef, 0,
				1);
			Array.Copy(baRecord,
				5 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 5 + 3 + 1 + 1 + 1,
				res2, 0,
				1);
		}

		public byte[] GetBytes()
		{
			byte [] baResult = null;

			baResult = ByteArray.Add(baResult, reclen);	// 5
			baResult = ByteArray.Add(baResult, status);	// 1
			baResult = ByteArray.Add(baResult, type);	// 1
			baResult = ByteArray.Add(baResult, level);	// 1
			baResult = ByteArray.Add(baResult, control);	// 1
			baResult = ByteArray.Add(baResult, reserve);	// 1
			baResult = ByteArray.Add(baResult, indicount);	// 1
			baResult = ByteArray.Add(baResult, subfldcodecount);	// 1
			baResult = ByteArray.Add(baResult, baseaddr);	// 5
			baResult = ByteArray.Add(baResult, res1);	// 3
			baResult = ByteArray.Add(baResult, lenoffld);	// 1
			baResult = ByteArray.Add(baResult, startposoffld);	// 1
			baResult = ByteArray.Add(baResult, impdef);	// 1
			baResult = ByteArray.Add(baResult, res2);	// 1

			Debug.Assert(baResult.Length == 24, "头标区内容必须为24字符");
			if (baResult.Length != 24)
				throw(new Exception("MarcHeader.GetBytes() error"));

            // 2014/5/9
            // 防范头标区出现 0 字符
            for (int i = 0; i < baResult.Length; i++)
            {
                if (baResult[i] == 0)
                    baResult[i] = (byte)'*';
            }

			return baResult;
		}

	}

}
