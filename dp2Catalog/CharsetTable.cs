using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace dp2Catalog
{
    /// <summary>
    /// 字符集码表
    /// </summary>
    public class CharsetTable : ItemFileBase
    {
        public CharsetTable()
		{

		}


		public override Item NewItem()
		{
			return new CharsetItem();
		}

        // 二分法
        // 根据给出的Key得到Value
        // return:
        //      -1  not found
        public int Search(string strKeyParam,
            out string strValue)
        {
            strValue = "";

            int k;	// 区间左
            int m;	// 区间右
            int j = -1;	// 区间中
            string strKey;
            int nComp;

            k = 0;
            m = (int)this.Count - 1;
            while (k <= m)
            {
                j = (k + m) / 2;
                // 取得j位置的值

                CharsetItem item = (CharsetItem)this[j];

                strKey = item.Name;

                nComp = String.Compare(strKey, strKeyParam);
                if (nComp == 0)
                {
                    strValue = item.Value;
                    break;
                }

                if (nComp > 0)
                {	// strKeyParam较小
                    m = j - 1;
                }
                else
                {
                    k = j + 1;
                }

            }

            if (k > m)
                return -1;	// not found

            return j;
        }

        // 将EACC字符缓冲区中的字符转换为Unicode字符
        // paramters:
        //		pszEACC	预先装了EACC字符的缓冲区(中间不能有非EACC字符)
        //		nEACCBytes		字节数。注意应当为3的倍数
        //		pUnicode	用于存放结果Unicode字符的缓冲区
        //		nMaxUnicodeBytes	缓冲区最大尺寸
        // return:
        //		-1	失败
        //		其它，转换成的Unicode字符数
        public int EACCToUnicode(string strEACC,
            out string strUnicode,
            out string strError)
        {

            strUnicode = "";
            strError = "";

            int nMax;
            char ch0;
            int nValue;
            int i;
            int nRet;
            char ch1;
            char ch2;
            string strPart;
            string strValue;
            // int t = 0;


            int nEACCBytes = strEACC.Length;
            if (nEACCBytes % 3 != 0)
            {
                strError = "参加转换的字节数应当为3的倍数(但是为 " + nEACCBytes.ToString() + ")";
                return -1;
            }

            nMax = nEACCBytes / 3;

            for (i = 0; i < nMax; i++)
            {


                ch0 = strEACC[i * 3];
                ch1 = strEACC[(i * 3) + 1];
                ch2 = strEACC[(i * 3) + 2];

                string strEACCPart = Convert.ToString((int)ch0, 16).PadLeft(2, '0');
                strEACCPart += Convert.ToString((int)ch1, 16).PadLeft(2, '0');
                strEACCPart += Convert.ToString((int)ch2, 16).PadLeft(2, '0');

                if (strEACCPart == "212321")
                {
                    strValue = "3000";
                    goto SKIP1;
                }

                nRet = Search(strEACCPart.ToUpper(),
                    out strValue);
                if (nRet == -1)
                {
                    // strUnicode += '*';
                    strUnicode += "{"+strEACCPart+"}";
                    continue;
                }

            SKIP1:
                strPart = strValue.Substring(0, 2);
                ch1 = (char)Convert.ToInt32(strPart, 16);

                strPart = strValue.Substring(2, 2);
                ch2 = (char)Convert.ToInt32(strPart, 16);

                nValue = 0xff00 & (((Int32)(ch1)) << 8);
                nValue += 0x00ff & ch2;
                strUnicode += (char)nValue;
            }

            return strUnicode.Length;
        }

        /*
        public int Text_e2u(string strSource,
            out string strTarget)
        {
            strTarget = "";

            int nEscCount = 0;
            bool bInEsc = false;
            bool bInCJK = false;
            bool bInMultiple = false;
            string strPart = "";

            string strError = "";

            for (int i = 0; i < strSource.Length; )
            {
                // char ch = strSource[i];
                if (strSource[i] == 0x1b && nEscCount == 0)
                {	// escape code
                    bInEsc = true;
                    nEscCount = 1;

                    i++;

                    // 看看strPart中是否有积累的内容
                    if (strPart != "")
                    {
                        if ((strPart.Length % 3) != 0)
                        {
                            strTarget += strPart;
                            goto CONTINUE1;
                        }
                        Debug.Assert((strPart.Length % 3) == 0, "");

                        string strTemp = "";
                        // strPart中必须存放非Unicode字符串
                        int nRet = EACCToUnicode(strPart,
                            out strTemp,
                            out strError);
                        if (nRet == -1)
                        {
                            strTarget += "[EACCToUnicode error:"
                                + strError + "][" + strPart + "]";

                            goto CONTINUE1;
                        }
                        strTarget += strTemp;

                    }
                CONTINUE1:
                    strPart = "";
                    continue;
                }

                if (bInEsc && nEscCount == 1)
                {
                    if (strSource[i] == 0x28 || strSource[i] == 0x2c)
                        bInMultiple = false;
                    else if (strSource[i] == 0x24)
                        bInMultiple = true;
                }

                if (bInEsc && nEscCount == 2)
                {
                    if (strSource[i] == 0x24 && bInMultiple == true)
                    {	// 国会图书馆$$1情况
                        i++;
                        continue;	// nEscCount不变，继续处理
                    }
                    if (strSource[i] == 0x28 && bInMultiple == false)
                    { // 国会图书馆((B情况
                        i++;
                        continue;
                    }
                    if (strSource[i] == 0x31)
                        bInCJK = true;
                    else
                        bInCJK = false;
                    bInEsc = false;
                    nEscCount = 0;
                    i++;
                    continue;
                }


                if (bInEsc)
                    nEscCount++;

                if (bInEsc == false)
                {
                    if (bInCJK == true)
                    {
                        strPart += strSource[i];
                    }
                    else
                        strTarget += strSource[i];
                }


                i++;
            }



            // 看看strPart中是否有积累的内容
            if (strPart != "")
            {
                if ((strPart.Length % 3) != 0)
                {
                    strTarget += strPart;
                    return 0;
                }
                Debug.Assert((strPart.Length % 3) == 0, "");

                string strTemp = "";

                // strPart中必须存放非Unicode字符串
                int nRet = EACCToUnicode(strPart,
                    out strTemp,
                    out strError);
                if (nRet == -1)
                {
                    strTarget += "[EACCToUnicode error:"
                        + strError + "][" + strPart + "]";
                    return 0;
                }
                strTarget += strTemp;

            }

            return 0;
        }
         * */




    }


    public class CharsetItem : Item
    {
        int m_nLength = 0;
        byte[] m_buffer = null;

        public string Name
        {
            get
            {
                string strValue = this.Content;
                int nRet = strValue.IndexOf('\t');
                if (nRet == -1)
                    return strValue;
                return strValue.Substring(0, nRet);
            }
        }

        public string Value
        {
            get
            {
                string strValue = this.Content;
                int nRet = strValue.IndexOf('\t');
                if (nRet == -1)
                    return null;
                return strValue.Substring(nRet+1);
            }
        }

        public string Content
        {
            get
            {
                return Encoding.UTF8.GetString(this.m_buffer);
            }
            set
            {
                m_buffer = Encoding.UTF8.GetBytes(value);
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
        }


        public override void ReadCompareData(Stream stream)
        {
            if (this.Length == 0)
                throw new Exception("length尚未初始化");


            // 读入Length个bytes的内容
            m_buffer = new byte[this.Length];
            stream.Read(m_buffer, 0, m_buffer.Length);
        }

        public override void WriteData(Stream stream)
        {
            if (m_buffer == null)
            {
                throw (new Exception("m_buffer尚未初始化"));
            }


            // 写入Length个bytes的内容
            stream.Write(m_buffer, 0, this.Length);
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
            CharsetItem item = (CharsetItem)obj;

            return String.Compare(this.Name, item.Name);
        }
    }
}
