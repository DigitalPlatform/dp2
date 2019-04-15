using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// 用于从 ISO2709 文件读入 MARC 记录的枚举器
    /// </summary>
    public class MarcLoader : IEnumerable
    {
        // 默认为 GB2312 编码方式
        private Encoding _encoding = Encoding.GetEncoding(936);

        /// <summary>
        /// 编码方式。
        /// </summary>
        public Encoding Encoding { get => _encoding; set => _encoding = value; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 事项类型。
        /// 空或者 "iso2709" 表示为 byte [] 类型，为 ISO2709 原始形态；"marc" 表示为 MARC 机内格式 string
        /// </summary>
        public string ItemType { get; set; }

        /// <summary>
        /// 用于显示进度的回调函数
        /// </summary>
        public Delegate_setProgress SetProgress { get; set; }

        /// <summary>
        /// 用于显示进度的回调函数类型
        /// </summary>
        /// <param name="totalLength">总长度</param>
        /// <param name="current">当前位置</param>
        public delegate void Delegate_setProgress(long totalLength, long current);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="itemType">事项类型。iso2709/marc 之一</param>
        /// <param name="func_setProgress">用于显示进度的回调函数</param>
        public MarcLoader(string filename,
            Encoding encoding,
            string itemType,
            Delegate_setProgress func_setProgress)
        {
            this.FileName = filename;
            this.Encoding = encoding;
            this.ItemType = itemType;
            this.SetProgress = func_setProgress;
        }

        /// <summary>
        /// 获得枚举器
        /// </summary>
        /// <returns>返回 IEnumberator 对象</returns>
        public IEnumerator GetEnumerator()
        {
            using (Stream s = File.OpenRead(this.FileName))
            {
                if (this.SetProgress != null)
                    this.SetProgress(s.Length, 0);

                for (; ; )
                {
                    // return:
                    // -1	出错
                    //	0	正确
                    //	1	结束(当前返回的记录有效)
                    //	2	结束(当前返回的记录无效)
                    int nRet = ReadMarcRecord(s,
                        this.Encoding,
                        true,
                        out byte[] baRecord,
                        out string strError);

                    if (this.SetProgress != null)
                        this.SetProgress(s.Length, s.Position);

                    if (nRet == -1)
                        throw new Exception(strError);
                    if (nRet == 2)
                        break;

                    if (string.IsNullOrEmpty(this.ItemType) || this.ItemType == "iso2709")
                        yield return baRecord;
                    else if (this.ItemType == "marc")
                    {
                        // 把byte[]类型的MARC记录转换为机内格式
                        // return:
                        //		-2	MARC格式错
                        //		-1	一般错误
                        //		0	正常
                        int nRet0 = ConvertIso2709ToMarcString(baRecord,
                            this.Encoding,
                            true,
                            out string strMarc,
                            out strError);
                        if (nRet0 == -1 || nRet0 == -2)
                            throw new Exception(strError);
                        yield return strMarc;
                    }
                    else
                        throw new Exception("未知的 ItemType '" + this.ItemType + "'");
                    if (nRet == 1)
                        break;
                }

                if (this.SetProgress != null)
                    this.SetProgress(s.Length, s.Length);
            }
        }

        // 从ISO2709文件中读入一条MARC记录
        // 增加了对UCS2编码方式的支持
        // parameters:
        //      encoding    编码方式。如果为null，表示为ansi类编码方式
        // return:
        // -1	出错
        //	0	正确
        //	1	结束(当前返回的记录有效)
        //	2	结束(当前返回的记录无效)
        public static int ReadMarcRecord(Stream s,
                Encoding encoding,
                bool bRemoveEndCrLf,
                out byte[] baRecord,
                out string strError)
        {
            strError = "";

            int nMaxBytes = 100000;

            int nRet = -1;
            int i = 0;
            List<byte> baTemp = new List<byte>();

            bool bUcs2 = false;

            if (encoding != null
                && encoding.Equals(Encoding.Unicode) == true)
            {
                bUcs2 = true;
                nMaxBytes = 2 * nMaxBytes;
            }

            // TODO: 如果是文件开头，要检查头三个 bytes 是不是 UTF-8 的 BOM
            bool bIsFirstRecord = s.Position == 0;

            for (i = 0; i < nMaxBytes; i++)
            {
                nRet = s.ReadByte();
                if (nRet == -1)
                {
                    nRet = 1;
                    break;
                }
                baTemp.Add((byte)nRet);


                if (bUcs2 == false)
                {
                    if (nRet == 29)
                    {
                        nRet = 0;
                        break;
                    }
                }
                else
                {
                    // 2007/7/11 add
                    // 如果为UCS2

                    // 如果缓冲区为偶数bytes
                    if ((i % 2) == 1)
                    {
                        byte b1 = baTemp[i - 1];
                        byte b2 = baTemp[i];

                        if (
                            (b1 == 29 && b2 == 0)
                            || (b1 == 0 && b2 == 29)
                            )
                        {
                            nRet = 0;
                            break;
                        }
                    }
                }
            }

            if (i >= nMaxBytes)
            {
                strError = "ISO2709记录尺寸(根据记录结束符测得)超过 " + nMaxBytes.ToString() + ", 被认为是不合法的记录";
                nRet = -1;	// 记录太大，或者文件不是ISO2709格式
            }

            // 2018/3/8
            // 检查 UTF-8 文件头部的 BOM
            if (bIsFirstRecord == true && baTemp.Count >= 3)
            {
                // ef bb bf
                if (baTemp[0] == 0xef && baTemp[1] == 0xbb && baTemp[2] == 0xbf)
                    baTemp.RemoveRange(0, 3); // 删除开头的 BOM
            }

            if (bRemoveEndCrLf)
            {
                // 看看开头的byte
                if (bUcs2 == true)
                {
                    for (i = 0; i < baTemp.Count / 2; i++)
                    {
                        byte b1 = (byte)baTemp[i * 2];
                        byte b2 = (byte)baTemp[i * 2 + 1];

                        if ((b2 == 0 && (b1 == 0x0d || b1 == 0x0a))
                            || ((b1 == 0) && (b2 == 0x0d || b2 == 0x0a))
                            || (b1 == 0xff && b2 == 0xfe)
                            || (b1 == 0xfe && b2 == 0xff)
                            )
                        {
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (i > 0)
                        baTemp.RemoveRange(0, i * 2); // 删除开头连续的CR LF
                }
                else
                {
                    // 不是UCS2
                    for (i = 0; i < baTemp.Count; i++)
                    {
                        byte b = (byte)baTemp[i];
                        if (b == 0x0d || b == 0x0a)
                        {
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (i > 0)
                        baTemp.RemoveRange(0, i); // 删除开头连续的CR LF
                }
            }

            baRecord = baTemp.ToArray();

            if (baRecord.Length == 0)
                return 2;

            return nRet;
        }

        #region ISO2709 --> 机内格式

        /// <summary>
        /// 字段结束符
        /// </summary>
        public const char FLDEND = (char)30;    // 字段结束符

        /// <summary>
        /// 记录结束符
        /// </summary>
        public const char RECEND = (char)29;    // 记录结束符

        /// <summary>
        /// 子字段符号
        /// </summary>
        public const char SUBFLD = (char)31;    // 子字段指示符

        // 把byte[]类型的 ISO2709 记录转换为 MARC 机内格式字符串
        // return:
        //		-2	MARC格式错
        //		-1	一般错误
        //		0	正常
        public static int ConvertIso2709ToMarcString(byte[] baRecord,
            Encoding encoding,
            bool bForce,
            out string strMarc,
            out string strError)
        {
            strError = "";
            strMarc = "";
            int nRet = 0;

            bool bUcs2 = false;

            if (encoding.Equals(Encoding.Unicode) == true)
                bUcs2 = true;

            List<byte[]> aField = new List<byte[]>();
            if (bForce == true
                || bUcs2 == true)
            {
                nRet = ForceCvt2709ToFieldArray(
                    ref encoding,
                    baRecord,
                    out aField,
                    out strError);

                Debug.Assert(nRet != -2, "");

                /*
                if (bUcs2 == true)
                {
                    // 转换后，编码方式已经变为UTF8
                    Debug.Assert(encoding.Equals(Encoding.UTF8), "");
                }
                 * */
            }
            else
            {
                nRet = Cvt2709ToFieldArray(
                    encoding,
                    baRecord,
                    out aField,
                    out strError);
            }

            if (nRet == -1)
                return -1;

            if (nRet == -2)  //marc出错
                return -2;

            string[] saField = null;
            GetMarcRecordString(aField,
                encoding,
                out saField);

            if (saField.Length > 0)
            {
                string strHeader = saField[0];

                if (strHeader.Length > 24)
                    strHeader = strHeader.Substring(0, 24);
                else
                    strHeader = saField[0].PadRight(24, '*');

                StringBuilder text = new StringBuilder(1024);
                text.Append(strHeader);
                for (int i = 1; i < saField.Length; i++)
                {
                    text.Append(saField[i] + new string(FLDEND, 1));
                }

                strMarc = text.ToString().Replace("\r", "*").Replace("\n", "*");
                return 0;
            }

            return 0;
        }


        // 将ISO2709格式记录转换为字段数组
        // aResult的每个元素为byte[]型，内容是一个字段。第一个元素是头标区，一定是24bytes
        // return:
        //	-1	一般性错误
        //	-2	MARC格式错误
        static int Cvt2709ToFieldArray(
            Encoding encoding,  // 2007/7/11
            byte[] s,
            out List<byte[]> aResult,   // out
            out string strErrorInfo)
        {
            strErrorInfo = "";
            aResult = new List<byte[]>();

            int maxbytes = 2000000;	// 约2000K，防止攻击

            if (encoding.Equals(Encoding.Unicode) == true)
                throw new Exception("UCS2编码方式应当使用 ForceCvt2709ToFieldArray()，而不是 Cvt2709ToFieldArray()");

            MarcHeaderStruct header = new MarcHeaderStruct(encoding, s);

            {
                // 输出头标区
                byte[] tarray = null;
                tarray = new byte[24];
                Array.Copy(s, 0, tarray, 0, 24);

                // 2014/5/9
                // 防范头标区出现 0 字符
                for (int j = 0; j < tarray.Length; j++)
                {
                    if (tarray[j] == 0)
                        tarray[j] = (byte)'*';
                }

                aResult.Add(tarray);
            }

            int somaxlen;
            int reclen, baseaddr, lenoffld, startposoffld;
            int len, startpos;
            // char *dirp;
            int offs = 0;
            int t = 0;
            int i;

            somaxlen = s.Length;
            try
            {
                reclen = header.RecLength;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区开始5字符 '" + header.RecLengthString + "' 不是纯数字 :" + ex.Message;
                // throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }
            if (reclen > somaxlen)
            {
                strErrorInfo = "头标区头5字符表示的记录长度"
                    + Convert.ToString(reclen)
                    + "大于源缓冲区整个内容的长度"
                    + Convert.ToString(somaxlen);
                goto ERROR2;
            }
            if (reclen < 24)
            {
                strErrorInfo = "头标区头5字符表示的记录长度"
                    + Convert.ToString(reclen)
                    + "小于24";
                goto ERROR2;
            }

            if (s[reclen - 1] != RECEND)
            {
                strErrorInfo = "头标区声称的结束位置不是MARC记录结束符";
                goto ERROR2;  // 结束符不正确
            }

            for (i = 0; i < reclen - 1; i++)
            {
                if (s[i] == RECEND)
                {
                    strErrorInfo = "记录内容中不能有记录结束符";
                    goto ERROR2;
                }
            }

            try
            {
                baseaddr = header.BaseAddress;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区数据基地址5字符 '" + header.BaseAddressString + " '不是纯数字 :" + ex.Message;
                //throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }

            if (baseaddr > somaxlen)
            {
                strErrorInfo = "数据基地址值 "
                    + Convert.ToString(baseaddr)
                    + " 已经超出源缓冲区整个内容的长度 "
                    + Convert.ToString(somaxlen);
                goto ERROR2;
            }
            if (baseaddr <= 24)
            {
                strErrorInfo = "数据基地址值 "
                    + Convert.ToString(baseaddr)
                    + " 小于24";
                goto ERROR2;  // 数据基地址太小
            }
            if (s[baseaddr - 1] != FLDEND)
            {
                strErrorInfo = "没有在目次区尾部位置" + Convert.ToString(baseaddr) + "找到FLDEND符号";
                goto ERROR2;  // 
            }

            try
            {
                lenoffld = header.WidthOfFieldLength;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区目次区字段长度1字符 '" + header.WidthOfFieldLengthString + " '不是纯数字 :" + ex.Message;
                //throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }

            try
            {
                startposoffld = header.WidthOfStartPositionOfField;
            }
            catch (FormatException ex)
            {
                strErrorInfo = "头标区目次区字段起始位置1字符 '" + header.WidthOfStartPositionOfFieldString + " '不是纯数字 :" + ex.Message;
                // throw(new MarcException(strErrorInfo));
                goto ERROR2;
            }


            if (lenoffld <= 0 || lenoffld > 30)
            {
                strErrorInfo = "目次区中字段长度值占用字符数 "
                    + Convert.ToString(lenoffld)
                    + " 不正确，应在1和29之间...";
                goto ERROR2;
            }

            if (lenoffld != 4)
            {	// 2001/5/15
                strErrorInfo = "目次区中字段长度值占用字符数 "
                    + Convert.ToString(lenoffld)
                    + " 不正确，应为4...";
                goto ERROR2;
            }

            lenoffld = 4;
            if (startposoffld <= 0 || startposoffld > 30)
            {
                strErrorInfo = "目次区中字段起始位置值占用字符数 "
                    + Convert.ToString(startposoffld)
                    + " 不正确，应在1到29之间...";
                goto ERROR2;
            }

            startposoffld = 5;

            // 开始处理目次区
            // dirp = (char *)sopp;
            t = 24;
            offs = 24;
            MyByteList baField = null;
            for (i = 0; ; i++)
            {
                if (s[offs] == FLDEND)
                    break;  // 目次区结束

                // 将字段名装入目标
                if (offs + 3 >= baseaddr)
                    break;
                if (t + 3 >= maxbytes)
                    break;

                baField = new MyByteList();
                baField.AddRange(s, offs, 3);
                t += 3;


                // 得到字段长度
                offs += 3;
                if (offs + lenoffld >= baseaddr)
                    break;
                len = MarcHeaderStruct.IntValue(s, offs, lenoffld);

                // 得到字段内容开始地址
                offs += lenoffld;
                if (offs + startposoffld >= baseaddr)
                    break;
                startpos = MarcHeaderStruct.IntValue(s, offs, startposoffld);

                offs += startposoffld;
                if (offs >= baseaddr)
                    break;

                // 将字段内容装入目标
                if (t + len >= maxbytes)
                    break;
                if (s[baseaddr + startpos - 1] != FLDEND)
                {
                    // errnoiso2709 = ERROR_BADFLDCONTENT;
                    strErrorInfo = "缺乏字段结束符";
                    goto ERROR2;
                }

                if (s[baseaddr + startpos + len - 1] != FLDEND)
                {
                    //errnoiso2709 = ERROR_BADFLDCONTENT;
                    strErrorInfo = "缺乏字段结束符";
                    goto ERROR2;
                }

                baField.AddRange(s, baseaddr + startpos, len == 0 ? len : len - 1);
                t += len;

                aResult.Add(baField.GetByteArray());
                baField = null;
            }

            if (t + 1 >= maxbytes)
            {
                // errnoiso2709 = ERROR_TARGETBUFFEROVERFLOW;
                strErrorInfo = "记录太大";
                goto ERROR2;  // 目标空间不够
            }

            Debug.Assert(t != -2, "");
            return t;
            //ERROR1:
            //	return -1;	// 一般性错误
            ERROR2:
            // 调试用
            Debug.Assert(false, "");
            return -2;	// MARC格式错误
        }

        // 强制将ISO2709格式记录转换为字段数组
        // 本函数采用的算法是将目次区的地址和长度忽略，只取3字符的字段名
        // aResult的每个元素为byte[]型，内容是一个字段。第一个元素是头标区，一定是24bytes
        // return:
        //	-1	一般性错误
        //	-2	MARC格式错误
        static int ForceCvt2709ToFieldArray(
            ref Encoding encoding,  // 2007/7/11 函数内可能发生变化
            byte[] s,
            out List<byte[]> aResult,
            out string strErrorInfo)
        {
            strErrorInfo = "";
            aResult = new List<byte[]>();

            Debug.Assert(s != null, "");

            List<MyByteList> results = new List<MyByteList>();

            bool bUcs2 = false;
            if (encoding.Equals(Encoding.Unicode) == true)
                bUcs2 = true;

            if (bUcs2 == true)
            {
                string strRecord = encoding.GetString(s);

                // 变换成UTF-8编码方式处理
                s = Encoding.UTF8.GetBytes(strRecord);
                encoding = Encoding.UTF8;
            }

            MarcHeaderStruct header = null;
            try
            {
                header = new MarcHeaderStruct(encoding, s);
            }
            catch (ArgumentException)
            {
                // 不足 24 字符的，给与宽容
                header = new MarcHeaderStruct(Encoding.ASCII, Encoding.ASCII.GetBytes("012345678901234567890123"));
            }
            header.ForceUNIMARCHeader();	// 强制将某些位置设置为默认值

            results.Add(header.GetByteList());

            int somaxlen;
            int offs;
            int i, j;

            somaxlen = s.Length;

            // 开始处理目次区
            offs = 24;
            MyByteList baField = null;
            bool bFound = false;
            for (i = 0; ; i++)
            {
                bFound = false;
                for (j = offs; j < offs + 3 + 4 + 5; j++)
                {
                    if (j >= somaxlen)
                        break;
                    if (s[j] == FLDEND)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (j >= somaxlen)
                {
                    offs = j;
                    break;
                }

                if (bFound == true)
                {
                    if (j <= offs + 3)
                    {
                        offs = j + 1;
                        break;
                    }
                }

                // 将字段名装入目标
                baField = new MyByteList();
                baField.AddRange(s, offs, 3);

                results.Add(baField);
                baField = null;
                // 得到字段内容开始地址
                offs += 3;
                offs += 4;
                offs += 5;

                if (bFound == true)
                {
                    offs = j + 1;
                    break;
                }

            }

            if (offs >= somaxlen)
                return 0;

            int nFieldNumber = 1;
            baField = null;
            // 加入对应的字段内容
            for (; offs < somaxlen; offs++)
            {
                byte c = s[offs];
                if (c == RECEND)
                    break;
                if (c == FLDEND)
                {
                    nFieldNumber++;
                    baField = null;
                }
                else
                {
                    if (baField == null)
                    {
                        // 确保下标不越界
                        while (nFieldNumber >= results.Count)
                        {
                            MyByteList temp = new MyByteList();
                            temp.Add((byte)'?');
                            temp.Add((byte)'?');
                            temp.Add((byte)'?');
                            results.Add(temp);
                        }
                        baField = results[nFieldNumber];
                    }

                    baField.Add(c);
                }
            }

            aResult = new List<byte[]>();
            foreach (MyByteList list in results)
            {
                aResult.Add(list.GetByteArray());
            }

            return 0;
            //		ERROR1:
            //			return -1;	// 一般性错误
            //		ERROR2:
            //			return -2;	// MARC格式错误
        }

        // 把 [byte []] 变换为 string []
        // aSourceField:	MARC字段数组。注意ArrayList每个元素要求为byte[]类型
        static int GetMarcRecordString(List<byte[]> aSourceField,
            Encoding encoding,
            out string[] saTarget)
        {
            saTarget = new string[aSourceField.Count];
            for (int j = 0; j < aSourceField.Count; j++)
            {
                saTarget[j] = encoding.GetString((byte[])aSourceField[j]);
            }

            return 0;
        }

        #endregion
    }

    // ISO2709ANSIHEADER结构定义
    // ISO2709头标区结构
    // charset: 按照ANSI字符集存储，尺寸固定，适用于DBCS/UTF-8/MARC-8情形
    internal class MarcHeaderStruct
    {
        byte[] reclen = new byte[5];                // 记录长度
        byte[] status = new byte[1];
        byte[] type = new byte[1];
        byte[] level = new byte[1];
        byte[] control = new byte[1];
        byte[] reserve = new byte[1];
        byte[] indicount = new byte[1];         // 字段指示符长度
        byte[] subfldcodecount = new byte[1];   // 子字段标识符长度
        byte[] baseaddr = new byte[5];          // 数据基地址
        byte[] res1 = new byte[3];
        byte[] lenoffld = new byte[1];          // 目次区中字段长度部分
        byte[] startposoffld = new byte[1];     // 目次区中字段起始位置部分
        byte[] impdef = new byte[1];                // 实现者定义部分
        byte[] res2 = new byte[1];

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
                throw (new Exception("baRecord中字节数少于24"));
            }

            bool bUcs2 = false;
            if (baRecord[0] == 0
                || baRecord[1] == 0)
            {
                bUcs2 = true;
            }

            if (bUcs2 == true)
            {
                throw new Exception("应用构造函数的另外一个版本，才能支持UCS2编码方式");
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

#if NO
        public byte[] GetBytes()
        {
            byte[] baResult = null;

            baResult = ByteArray.Add(baResult, reclen); // 5
            baResult = ByteArray.Add(baResult, status); // 1
            baResult = ByteArray.Add(baResult, type);   // 1
            baResult = ByteArray.Add(baResult, level);  // 1
            baResult = ByteArray.Add(baResult, control);    // 1
            baResult = ByteArray.Add(baResult, reserve);    // 1
            baResult = ByteArray.Add(baResult, indicount);  // 1
            baResult = ByteArray.Add(baResult, subfldcodecount);    // 1
            baResult = ByteArray.Add(baResult, baseaddr);   // 5
            baResult = ByteArray.Add(baResult, res1);   // 3
            baResult = ByteArray.Add(baResult, lenoffld);   // 1
            baResult = ByteArray.Add(baResult, startposoffld);  // 1
            baResult = ByteArray.Add(baResult, impdef); // 1
            baResult = ByteArray.Add(baResult, res2);   // 1

            Debug.Assert(baResult.Length == 24, "头标区内容必须为24字符");
            if (baResult.Length != 24)
                throw (new Exception("MarcHeader.GetBytes() error"));

            // 2014/5/9
            // 防范头标区出现 0 字符
            for (int i = 0; i < baResult.Length; i++)
            {
                if (baResult[i] == 0)
                    baResult[i] = (byte)'*';
            }

            return baResult;
        }
#endif
        // 2015/5/10
        public MyByteList GetByteList()
        {
            MyByteList list = new MyByteList(24);

            list.AddRange(reclen);	// 5
            list.AddRange(status);	// 1
            list.AddRange(type);	// 1
            list.AddRange(level);	// 1
            list.AddRange(control);	// 1
            list.AddRange(reserve);	// 1
            list.AddRange(indicount);	// 1
            list.AddRange(subfldcodecount);	// 1
            list.AddRange(baseaddr);	// 5
            list.AddRange(res1);	// 3
            list.AddRange(lenoffld);	// 1
            list.AddRange(startposoffld);	// 1
            list.AddRange(impdef);	// 1
            list.AddRange(res2);	// 1

            Debug.Assert(list.Count == 24, "头标区内容必须为24字符");
            if (list.Count != 24)
                throw (new Exception("MarcHeader.GetBytes() error"));

            // 2014/5/9
            // 防范头标区出现 0 字符
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == 0)
                    list[i] = (byte)'*';
            }

            return list;
        }
    }

    /// <summary>
    /// byte数组，可以动态扩展
    /// </summary>
    class MyByteList : List<byte>
    {
        public MyByteList()
            : base()
        {
        }

        public MyByteList(int capacity)
            : base(capacity)
        {
        }

        public void AddRange(byte[] baSource)
        {
            base.AddRange(baSource);
        }

        public int AddRange(byte[] baSource,
            int nStart,
            int nLength)
        {
            int nCount = 0;
            for (int i = nStart; i < baSource.Length && i < nStart + nLength; i++)
            {
                this.Add(baSource[i]);
                nCount++;
            }

            return nCount;
        }

        public byte[] GetByteArray()
        {
            return base.ToArray();
        }
    }

}
