using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Diagnostics;

using System.Windows.Forms;
using System.Drawing;


namespace DigitalPlatform
{

    // byte[] 数组的实用函数集
    public class ByteArray
    {
        /*
        // 复制一个byte数组
        public static byte[] Dup(byte [] source)
        {
            if (source == null)
                return null;

            byte [] result = null;
            result = EnsureSize(result, source.Length);

            Array.Copy(source, 0, result, 0, source.Length);

            return result;
        }*/

        // 克隆一个字符数组
        public static byte[] GetCopy(byte[] baContent)
        {
            if (baContent == null)
                return null;
            byte[] baResult = new byte[baContent.Length];
            Array.Copy(baContent, 0, baResult, 0, baContent.Length);
            return baResult;
        }

        // 将byte[]转换为字符串，自动探测编码方式
        public static string ToString(byte[] baContent)
        {
            ArrayList encodings = new ArrayList();

            encodings.Add(Encoding.UTF8);
            encodings.Add(Encoding.Unicode);

            for (int i = 0; i < encodings.Count; i++)
            {
                Encoding encoding = (Encoding)encodings[i];

                byte[] Preamble = encoding.GetPreamble();

                if (baContent.Length < Preamble.Length)
                    continue;

                if (ByteArray.Compare(baContent, Preamble, Preamble.Length) == 0)
                    return encoding.GetString(baContent,
                        Preamble.Length,
                        baContent.Length - Preamble.Length);
            }

            // 缺省当作UTF8
            return Encoding.UTF8.GetString(baContent);
        }

        // byte[] 到 字符串
        public static string ToString(byte[] bytes,
            Encoding encoding)
        {
            int nIndex = 0;
            int nCount = bytes.Length;
            byte[] baPreamble = encoding.GetPreamble();
            if (baPreamble != null
                && baPreamble.Length != 0
                && bytes.Length >= baPreamble.Length)
            {
                byte[] temp = new byte[baPreamble.Length];
                Array.Copy(bytes,
                    0,
                    temp,
                    0,
                    temp.Length);

                bool bEqual = true;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] != baPreamble[i])
                    {
                        bEqual = false;
                        break;
                    }
                }

                if (bEqual == true)
                {
                    nIndex = temp.Length;
                    nCount = bytes.Length - temp.Length;
                }
            }

            return encoding.GetString(bytes,
                nIndex,
                nCount);
        }


        // 比较两个byte[]数组是否相等。
        // parameter:
        //		timestamp1: 第一个byte[]数组
        //		timestamp2: 第二个byte[]数组
        // return:
        //		0   相等
        //		大于或者小于0   不等。先比较长度。长度相等，再逐个字符相减。
        public static int Compare(
            byte[] bytes1,
            byte[] bytes2)
        {
            if (bytes1 == null && bytes2 == null)
                return 0;
            if (bytes1 == null)
                return -1;
            if (bytes2 == null)
                return 1;

            int nDelta = bytes1.Length - bytes2.Length;
            if (nDelta != 0)
                return nDelta;

            for (int i = 0; i < bytes1.Length; i++)
            {
                nDelta = bytes1[i] - bytes2[i];
                if (nDelta != 0)
                    return nDelta;
            }

            return 0;
        }

        // 比较两个byte数组的局部
        public static int Compare(
            byte[] bytes1,
            byte[] bytes2,
            int nLength)
        {
            if (bytes1.Length < nLength || bytes2.Length < nLength)
                return Compare(bytes1, bytes2, Math.Min(bytes1.Length, bytes2.Length));

            for (int i = 0; i < nLength; i++)
            {
                int nDelta = bytes1[i] - bytes2[i];
                if (nDelta != 0)
                    return nDelta;
            }

            return 0;
        }


        public static int IndexOf(byte[] source,
            byte v,
            int nStartPos)
        {
            for (int i = nStartPos; i < source.Length; i++)
            {
                if (source[i] == v)
                    return i;
            }
            return -1;
        }
        // 确保数组尺寸足够
        public static byte[] EnsureSize(byte[] source,
            int nSize)
        {
            if (source == null)
            {
                return new byte[nSize];
            }

            if (source.Length < nSize)
            {
                byte[] temp = new byte[nSize];
                Array.Copy(source,
                    0,
                    temp,
                    0,
                    source.Length);
                return temp;	// 尺寸不够，已经重新分配，并且继承了原有内容
            }

            return source;	// 尺寸足够
        }


        // 在缓冲区尾部追加一个字节
        public static byte[] Add(byte[] source,
            byte v)
        {
            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + 1);
            }
            else
            {
                nIndex = 0;
                source = EnsureSize(source, 1);
            }

            source[nIndex] = v;

            return source;
        }

        // 在缓冲区尾部追加若干字节
        public static byte[] Add(byte[] source,
            byte[] v)
        {
            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + v.Length);
            }
            else
            {
                // 2011/1/22
                if (v == null)
                    return null;
                nIndex = 0;
                source = EnsureSize(source, v.Length);
            }

            Array.Copy(v, 0, source, nIndex, v.Length);

            return source;
        }

        // 2011/9/12
        // 在缓冲区尾部追加若干字节
        public static byte[] Add(byte[] source,
            byte[] v,
            int nLength)
        {
            Debug.Assert(v.Length >= nLength, "");

            int nIndex = -1;
            if (source != null)
            {
                nIndex = source.Length;
                source = EnsureSize(source, source.Length + nLength);
            }
            else
            {
                if (v == null)
                    return null;
                nIndex = 0;
                source = EnsureSize(source, nLength);
            }

            Array.Copy(v, 0, source, nIndex, nLength);

            return source;
        }

        // 得到用16进制表示的时间戳字符串
        public static string GetHexTimeStampString(byte[] baTimeStamp)
        {
            if (baTimeStamp == null)
                return "";
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < baTimeStamp.Length; i++)
            {
                //string strHex = String.Format("{0,2:X}",baTimeStamp[i]);
                string strHex = Convert.ToString(baTimeStamp[i], 16);
                text.Append(strHex.PadLeft(2, '0'));
            }

            return text.ToString();
        }

        // 得到byte[]类型的时间戳
        public static byte[] GetTimeStampByteArray(string strHexTimeStamp)
        {
            if (string.IsNullOrEmpty(strHexTimeStamp) == true)
                return null;

            byte[] result = new byte[strHexTimeStamp.Length / 2];

            for (int i = 0; i < strHexTimeStamp.Length / 2; i++)
            {
                string strHex = strHexTimeStamp.Substring(i * 2, 2);
                result[i] = Convert.ToByte(strHex, 16);
            }

            return result;
        }
    }

    /*
    /// <summary>
    /// 一般性、全局函数
    /// </summary>
    public class General
    {
        public static long min(long a, long b)
        {
            return a < b ? a : b;
        }

        public static int min(int a, int b)
        {
            return a < b ? a : b;
        }

        public static long max(long a, long b)
        {
            return a > b ? a : b;
        }

        public static int max(int a, int b)
        {
            return a > b ? a : b;
        }
    }
    */

    // 编写者: 任延华
    public class ConvertUtil
    {

        // 用CopyTo()即可
        // 把包含string对象的ArrayList转换为string[]类型
        // parameters:
        //      nStartCol   开始的列号。一般为0
        public static string[] GetStringArray(
            int nStartCol,
            ArrayList aText)
        {
            string[] result = new string[aText.Count + nStartCol];
            for (int i = 0; i < aText.Count; i++)
            {
                result[i + nStartCol] = (string)aText[i];
            }
            return result;
        }

        //字符串到int32
        public static int S2Int32(string strText)
        {
            int nTemp = 0;
            try
            {
                nTemp = Convert.ToInt32(strText);
            }
            catch (Exception ex)
            {
                throw (new Exception("配置文件不合适的值:" + strText + "\r\n" + ex.Message));
            }
            return nTemp;
        }

        //字符串到int32带指定进制版本
        public static int S2Int32(string strText, int nBase)
        {
            int nTemp = 0;
            try
            {
                nTemp = Convert.ToInt32(strText, nBase);
            }
            catch (Exception ex)
            {
                throw (new Exception("配置文件不合适的值:" + strText + "\r\n" + ex.Message));
            }
            return nTemp;
        }

        // 检索范围是否合法,并返回真正能够取的长度
        // parameter:
        //		nStart          起始位置 不能小于0
        //		nNeedLength     需要的长度	不能小于-1，-1表示从nStart-(nTotalLength-1)
        //		lTotalLength    数据实际总长度 不能小于0
        //		nMaxLength      限制的最大长度	等于-1，表示不限制
        //		lOutputLength   out参数，返回的可以用的长度 2012/8/26 修改为long类型
        //		strError        out参数，返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        public static int GetRealLength(long lStart,
            int nNeedLength,
            long lTotalLength,
            int nMaxLength,
            out long lOutputLength,
            out string strError)
        {
            lOutputLength = 0;
            strError = "";

            // 起始值,或者总长度不合法
            if (lStart < 0
                || lTotalLength < 0)
            {
                strError = "范围错误:nStart < 0 或 nTotalLength <0 \r\n";
                return -1;
            }
            if (lStart != 0
                && lStart >= lTotalLength)
            {
                strError = "范围错误: 起始值 " + lStart.ToString() + " 大于总长度 " + lTotalLength.ToString() + "\r\n";
                return -1;
            }

            lOutputLength = nNeedLength;
            if (lOutputLength == 0)
            {
                return 0;
            }

            // 因为中间运算的时候lOutoutLength的值可能一度很大，所以用了long类型。但最后经过限制之后，不会超过int的范围

            if (lOutputLength == -1)  // 从开始到全部
                lOutputLength = lTotalLength - lStart;

            if (lStart + lOutputLength > lTotalLength)
                lOutputLength = lTotalLength - lStart;

            // 限制了最大长度
            if (nMaxLength != -1 && nMaxLength >= 0)
            {
                if (lOutputLength > nMaxLength)
                    lOutputLength = nMaxLength;

                Debug.Assert(lOutputLength < Int32.MaxValue && lOutputLength > Int32.MinValue, "");
            }

            return 0;
        }
    }

#if NO


	public class ArrayListUtil
	{
		// 功能: 合并两个字符串数组
		// parameter:
		//		sourceLeft: 源左边数组
		//		sourceRight: 源右边数组
		//		targetLeft: 目标左边数组
		//		targetMiddle: 目标中间数组
		//		targetRight: 目标右边数组
		// 出错抛出异常
		public static void MergeStringArray(ArrayList sourceLeft,
			ArrayList sourceRight,
			List<string> targetLeft,
			List<string> targetMiddle,
			List<string> targetRight)
		{
			int i = 0;   
			int j = 0;
			string strLeft;
			string strRight;
			int ret;
			while (true)
			{
				strLeft = null;
				strRight = null;
				if (i >= sourceLeft.Count)
				{
					i = -1;
				}
				else if (i != -1)
				{
					try
					{
						strLeft = (string)sourceLeft[i];
					}
					catch
					{
						Exception ex = new Exception("i="+Convert.ToString(i)+"----Count="+Convert.ToString(sourceLeft.Count)+"<br/>");
						throw(ex);
					}
				}
				if (j >= sourceRight.Count)
				{
					j = -1;
				}
				else if (j != -1)
				{
					try
					{
						strRight = (string)sourceRight[j];
					}
					catch
					{
						Exception ex = new Exception("j="+Convert.ToString(j)+"----Count="+Convert.ToString(sourceLeft.Count)+sourceRight.GetHashCode()+"<br/>");
						throw(ex);
					}
				}
				if (i == -1 && j == -1)
				{
					break;
				}

				if (strLeft == null)
				{
					ret = 1;
				}
				else if (strRight == null)
				{
					ret = -1;
				}
				else
				{
					ret = strLeft.CompareTo(strRight);  //MyCompareTo(oldOneKey); //改CompareTO
				}

				if (ret == 0 && targetMiddle != null) 
				{
					targetMiddle.Add(strLeft);
					i++;
					j++;
				}

				if (ret<0) 
				{
					if (targetLeft != null && strLeft != null)
						targetLeft.Add(strLeft);
					i++;
				}

				if (ret>0 )
				{
					if (targetRight != null && strRight != null)
						targetRight.Add(strRight);
					j++;
				}
			}
		}
	}
#endif

}



