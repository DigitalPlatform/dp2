using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.Marc
{
    public enum ItemType
    {
        Filter = 0,
        Record = 1,
        Field = 2,
        Subfield = 3,
        Group = 4,
        Char = 5,
    }

    /// <summary>
    /// 一些有关MARC的实用函数
    /// </summary>
    public class MarcUtil
    {
        public const char FLDEND = (char)30;	// 字段结束符
        public const char RECEND = (char)29;	// 记录结束符
        public const char SUBFLD = (char)31;	// 子字段指示符

        public const int FLDNAME_LEN = 3;       // 字段名长度
        public const int MAX_MARCREC_LEN = 100000;   // MARC记录的最大长度

        // 包装后的版本
        public static string GetHtmlOfMarc(string strMARC, bool bSubfieldReturn)
        {
            return GetHtmlOfMarc(strMARC, "", "", bSubfieldReturn);
        }

        static string GetFragmentHtml(string strFragmentXml)
        {
            if (string.IsNullOrEmpty(strFragmentXml) == true)
                return "";

            strFragmentXml = MarcDiff.GetIndentInnerXml(strFragmentXml);    // 不包含根节点
            return GetPlanTextHtml(strFragmentXml);
        }

        static string GetPlanTextHtml(string strOldFragmentXml)
        {
            string strLineClass = "datafield";
            StringBuilder strResult = new StringBuilder(4096);

            strResult.Append("\r\n<tr class='" + strLineClass + "'>");

            // 
            string[] lines = HttpUtility.HtmlEncode(strOldFragmentXml).Replace("\r\n", "\n").Split(new char[] { '\n' });
            StringBuilder result = new StringBuilder(4096);
            foreach (string line in lines)
            {
                if (result.Length > 0)
                    result.Append("<br/>");
                result.Append(MarcDiff.ReplaceLeadingTab(line));
            }

            strResult.Append("\r\n<td class='content' colspan='3'>" + result + "</td>");

            strResult.Append("\r\n</tr>");

            return strResult.ToString();
        }

        // 2013/6/11
        // 获得 MARC 记录的 HTML 格式字符串
        public static string GetHtmlOfXml(string strFragmentXml,
            bool bSubfieldReturn)
        {
            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);

            if (string.IsNullOrEmpty(strFragmentXml) == false)
            {
                strResult.Append(GetFragmentHtml(strFragmentXml));
            }

            strResult.Append("\r\n</table>");
            return strResult.ToString();
        }

        static string GetImageHtml(string strImageFragment)
        {
            if (string.IsNullOrEmpty(strImageFragment) == true)
                return "";

            string strLineClass = "datafield";
            StringBuilder strResult = new StringBuilder(4096);

            strResult.Append("\r\n<tr class='" + strLineClass + "'>");

            strResult.Append("\r\n<td class='content' colspan='3'>"    //  
                + strImageFragment
                + "</td>");

            strResult.Append("\r\n</tr>");

            return strResult.ToString();
        }


        // 2013/2/16
        // 获得 MARC 记录的 HTML 格式字符串
        public static string GetHtmlOfMarc(string strMARC,
            string strFragmentXml,
            string strCoverImageFragment,
            bool bSubfieldReturn)
        {
            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);

            for (int i = 0; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";

                int nRet = MarcUtil.GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet != 1)
                    break;

                string strLineClass = "";
                string strFieldName = "";
                string strIndicatior = "";
                string strContent = "";
                if (i != 0)
                {
                    // 取字段名
                    if (strField.Length < 3)
                    {
                        strFieldName = strField;
                        strField = "";
                    }
                    else
                    {
                        strFieldName = strField.Substring(0, 3);
                        strField = strField.Substring(3);
                    }

                    // 取指示符
                    if (IsControlFieldName(strFieldName) == true)
                    {
                        strLineClass = "controlfield";
                        strField = strField.Replace(' ', '_');
                    }
                    else
                    {
                        if (strField.Length < 2)
                        {
                            strIndicatior = strField;
                            strField = "";
                        }
                        else
                        {
                            strIndicatior = strField.Substring(0, 2);
                            strField = strField.Substring(2);
                        }
                        strIndicatior = strIndicatior.Replace(' ', '_');

                        strLineClass = "datafield";

                        // 1XX字段有定长内容
                        if (strFieldName.Length >= 1 && strFieldName[0] == '1')
                        {
                            strField = strField.Replace(' ', '_');
                            strLineClass += " fixedlengthsubfield";
                        }
                    }
                }
                else
                {
                    strLineClass = "header";
                    strField = strField.Replace(' ', '_');
                }

                /*
                strContent = strField.Replace(new string((char)31,1),
                    "<span>|</span>");
                 * */
                strContent = GetHtmlFieldContent(strField,
                    bSubfieldReturn);

                // 
                strResult.Append("\r\n<tr class='" + strLineClass + "'><td class='fieldname'>" + strFieldName + "</td>"
                    + "<td class='indicator'>" + strIndicatior + "</td>"
                    + "<td class='content'>" + strContent + "</td></tr>");

                if (i == 0)
                    strResult.Append(GetImageHtml(strCoverImageFragment));
            }

            if (string.IsNullOrEmpty(strFragmentXml) == false)
            {
                strResult.Append(GetFragmentHtml(strFragmentXml));
            }

            strResult.Append("\r\n</table>");

            return strResult.ToString();
        }

        public static string GetHtmlFieldContent(string strContent,
            bool bSubfieldReturn)
        {
            const string SubFieldChar = "‡";
            const string FieldEndChar = "¶";

            StringBuilder result = new StringBuilder(4096);
            for (int i = 0; i < strContent.Length; i++)
            {
                char ch = strContent[i];
                if (ch == (char)31)
                {
                    if (result.Length > 0)
                    {
                        if (bSubfieldReturn == true)
                            result.Append("<br/>");
                        else
                            result.Append(" "); // 为了显示时候可以折行
                    }

                    result.Append("<span class='subfield'>");
                    result.Append((char)0x200e);
                    result.Append(SubFieldChar);
                    if (i < strContent.Length - 1)
                    {
                        result.Append(strContent[i + 1]);
                        i++;
                    }
                    else
                        result.Append(SubFieldChar);

                    // 2022/1/6
                    // 为 $9 后面加一个空格。解决 Unicode bidi 问题
                    if (result.Length > 0 && char.IsDigit(result[result.Length - 1]))
                        result.Append(' ');
                    result.Append("</span>");
                    continue;
                }
                result.Append(ch);
            }

            result.Append("<span class='fieldend'>" + FieldEndChar + "</span>");

            return result.ToString();
        }

        public static bool MatchIndicator(string strMatchCase,
    string strIndicator)
        {
            if (string.IsNullOrEmpty(strMatchCase) == true)
            {
                Debug.Assert(false, "strMatch内容不能为空");
                return false;
            }
            /*
            if (strMatch.Length != 2)
            {
                Debug.Assert(false, "strMatch长度只能为2 (strMatch='"+strMatch+"')");
                return false;
            }
             * */

            if (string.IsNullOrEmpty(strIndicator) == true)
            {
                Debug.Assert(false, "strIndicator内容不能为空");
                return true;
            }
            if (strIndicator.Length != 2)
            {
                Debug.Assert(false, "strIndicator长度只能为2 (strIndicator='" + strIndicator + "')");
                return false;
            }

            if (strMatchCase == "**")
                return true;

            // Regular expression
            if (strMatchCase.Length >= 1
                && strMatchCase[0] == '@')
            {
                if (StringUtil.RegexCompare(strMatchCase.Substring(1),
                    RegexOptions.None,
                    strIndicator) == true)
                    return true;
                return false;
            }

            if (strMatchCase[0] != '*')
            {
                if (strMatchCase[0] != strIndicator[0])
                    return false;
            }

            if (strMatchCase[1] != '*')
            {
                if (strMatchCase[1] != strIndicator[1])
                    return false;
            }

            return true;
        }

        // 把byte[]类型的MARC记录转换为机内格式
        // return:
        //		-2	MARC格式错
        //		-1	一般错误
        //		0	正常
        public static int ConvertByteArrayToMarcRecord(byte[] baRecord,
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
                nRet = MarcUtil.ForceCvt2709ToFieldArray(
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
                //???
                nRet = MarcUtil.Cvt2709ToFieldArray(
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

                strMarc = text.ToString().Replace("\r", "*").Replace("\n", "*");    // 2012/3/16
                return 0;
            }

            return 0;
        }


        // 将ISO2709格式记录转换为字段数组
        // aResult的每个元素为byte[]型，内容是一个字段。第一个元素是头标区，一定是24bytes
        // return:
        //	-1	一般性错误
        //	-2	MARC格式错误
        public static int Cvt2709ToFieldArray(
            Encoding encoding,  // 2007/7/11
            byte[] s,
            out List<byte[]> aResult,   // out
            out string strErrorInfo)
        {
            strErrorInfo = "";
            aResult = new List<byte[]>();

            // const char *sopp;
            int maxbytes = 2000000;	// 约2000K，防止攻击

            // const byte RECEND = 29;
            // const byte FLDEND = 30;
            // const byte SUBFLD = 31;

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
            // char temp[30];

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
                /*
                baTarget.SetSize(t+3, CHUNK_SIZE);
                memcpy((char *)baTarget.GetData()+t,
                    dirp+offs,
                    3);
                t+=3;
                */
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

                /*
                baTarget.SetSize(t+len, CHUNK_SIZE);
                memcpy((char *)baTarget.GetData()+t,
                    sopp+baseaddr+startpos,
                    len);
                t += len;
                */
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

            /*
            baField.Add((char)RECEND);
            t ++;
            */

            /*
            baTarget.SetSize(t+1, CHUNK_SIZE);
            *((char *)baTarget.GetData() + t++) = RECEND;
            if (t+1>=maxbytes) 
            {
                errnoiso2709 = ERROR_TARGETBUFFEROVERFLOW;
                goto ERROR1;  // 目标空间不够
            }
            */

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
        public static int ForceCvt2709ToFieldArray(
            ref Encoding encoding,  // 2007/7/11 函数内可能发生变化
            byte[] s,
            out List<byte[]> aResult,
            out string strErrorInfo)
        {
            strErrorInfo = "";
            aResult = new List<byte[]>();

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
            header.ForceUNIMARCHeader();	// 强制将某些位置设置为缺省值

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

        // 从ISO2709文件中读入一条MARC记录
        // return:
        //	-2	MARC格式错
        //	-1	出错
        //	0	正确
        //	1	结束(当前返回的记录有效)
        //	2	结束(当前返回的记录无效)
        public static int ReadMarcRecord(Stream s,
            Encoding encoding,
            bool bRemoveEndCrLf,
            bool bForce,
            out string strMARC,
            out string strError)
        {
            strMARC = "";
            strError = "";

            byte[] baRecord = null;
            // return:
            // -1	出错
            //	0	正确
            //	1	结束(当前返回的记录有效)
            //	2	结束(当前返回的记录无效)
            int nRet = ReadMarcRecord(s,
                encoding,
                bRemoveEndCrLf,
                out baRecord,
                out strError);
            if (nRet != 0 && nRet != 1)
                return nRet;

            // return:
            //		-2	MARC格式错
            //		-1	一般错误
            //		0	正常
            int nRet1 = ConvertByteArrayToMarcRecord(baRecord,
                encoding,
                bForce,
                out strMARC,
                out strError);
            if (nRet1 == 0)
                return nRet;

            return nRet1;
        }

        // 从ISO2709文件中读入一条MARC记录
        // 要返回 byte []
        // return:
        //	-2	MARC格式错
        //	-1	出错
        //	0	正确
        //	1	结束(当前返回的记录有效)
        //	2	结束(当前返回的记录无效)
        public static int ReadMarcRecord(Stream s,
            Encoding encoding,
            bool bRemoveEndCrLf,
            bool bForce,
            out string strMARC,
            out byte[] baRecord,
            out string strError)
        {
            strMARC = "";
            strError = "";

            baRecord = null;
            // return:
            // -1	出错
            //	0	正确
            //	1	结束(当前返回的记录有效)
            //	2	结束(当前返回的记录无效)
            int nRet = ReadMarcRecord(s,
                encoding,
                bRemoveEndCrLf,
                out baRecord,
                out strError);
            if (nRet != 0 && nRet != 1)
                return nRet;

            // return:
            //		-2	MARC格式错
            //		-1	一般错误
            //		0	正常
            int nRet1 = ConvertByteArrayToMarcRecord(baRecord,
                encoding,
                bForce,
                out strMARC,
                out strError);
            if (nRet1 == 0)
                return nRet;

            return nRet1;
        }

        // 2013/11/23
        // 规范化 ISO2709 物理记录
        // 主要是检查里面的记录结束符是否正确，去掉多余的记录结束符
        public static byte[] CononicalizeIso2709Bytes(Encoding encoding,
            byte[] baRecord)
        {
            if (baRecord == null || baRecord.Length == 0)
                return baRecord;

            if (encoding.Equals(Encoding.Unicode) == true
                || encoding.Equals(Encoding.UTF32) == true)
            {
                return baRecord;    // 暂不作检查
            }

            // 检查中间的记录结束符
            for (int i = 0; i < baRecord.Length - 1; i++)
            {
                if (baRecord[i] == 29)
                    baRecord[i] = (byte)'*';
            }

            // 检查记录结束符
            if (baRecord[baRecord.Length - 1] != 29)
                baRecord = ByteArray.Add(baRecord, (byte)29);

            return baRecord;
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

            // 2007/7/23
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


            //       int i = 0;

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

            baRecord = new byte[baTemp.Count];
            baTemp.CopyTo(baRecord);    // 2015/5/10
#if NO
			for(i=0;i<baTemp.Count;i++) 
			{
				baRecord[i] = (byte)baTemp[i];
			}
#endif

            if (baRecord.Length == 0)
                return 2;

            return nRet;
        }

        // 将MARC记录转换为字段(字符串)数组。
        public static int ConvertMarcToFieldArray(string strMARC,
            out string[] saField,
            out string strError)
        {
            strError = "";
            saField = null;

            string[] aDataField = null;

            string strLeader = "";

            if (strMARC.Length < 24)
            {
                strLeader = strMARC.PadRight(24, '*');
                saField = new string[1];
                saField[0] = strLeader;
                return 0;
            }
            else
            {
                strLeader = strMARC.Substring(0, 24);
            }

            aDataField = strMARC.Substring(24).Split(new char[] { (char)FLDEND });

            int i;
            List<string> temp = new List<string>(500);

            for (i = 1; i < aDataField.Length; i++)
            {
                string strField = aDataField[i - 1];
                if (strField.Length == 0)
                    continue;
                if (strField.Length < 3)
                    strField = strField.PadRight(3, '*');

                temp.Add(strField);
            }

            // 2012/11/3 修改
            temp.Insert(0, strLeader);
            saField = new string[temp.Count];
            temp.CopyTo(saField);
#if NO
			saField = new string [temp.Count+1];

			saField[0] = strLeader;
			for(i=1;i<saField.Length;i++)
			{
				saField[i] = (string)temp[i-1];
			}
#endif

            return 0;
        }

#if NO
		// strField:	最好是纯粹的字段内容，即不包括字段名和指示符部分
		public static int GetSubfield(string strField,
			out string[] aSubfield)
		{
			ArrayList aTemp = new ArrayList();
			int i;

			string[] aSplit = strField.Split(new char[]{(char)31});
			if (aSplit == null) 
			{
				aSubfield = null;
				return -1;
			}
			for(i=0;i<aSplit.Length;i++) 
			{
				if (aSplit[i].Length < 1)
					continue;
				aTemp.Add(aSplit[i]);
			}

			aSubfield = new string[aTemp.Count];
			for(i=0;i<aTemp.Count;i++) 
			{
				aSubfield[i] = (string)aTemp[i];
			}

			return 0;
		}
#endif


        public static string GetMarcURI(string strMarcSyntax)
        {
            if (strMarcSyntax == "usmarc")
                return Ns.usmarcxml;

            return DpNs.unimarcxml;
        }

#if NO
        // TODO: 保留domMarc中除了MARC相关结构以外的其它内容
        // 2010/11/15
        // 将MARC记录转换为xml格式
        // parameters:
        //      strMarcSyntax   MARC格式．为 unimarc/usmarc之一，缺省为unimarc
        public static int Marc2Xml(string strMARC,
            string strMarcSyntax,
            ref XmlDocument domMarc,
            out string strError)
        {
            strError = "";
            domMarc = null;

            // MARC控件中内容更新一些. 需要刷新到xml控件中
            MemoryStream s = new MemoryStream();

            MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8);

            if (strMarcSyntax == "unimarc")
            {
                writer.MarcNameSpaceUri = DpNs.unimarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else if (strMarcSyntax == "usmarc")
            {
                writer.MarcNameSpaceUri = Ns.usmarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else
            {
                writer.MarcNameSpaceUri = DpNs.unimarcxml;
                writer.MarcPrefix = "unimarc";
            }

            int nRet = writer.WriteRecord(strMARC,
                out strError);
            if (nRet == -1)
                return -1; ;

            writer.Flush();
            s.Flush();

            s.Seek(0, SeekOrigin.Begin);

            domMarc = new XmlDocument();
            try
            {
                domMarc.Load(s);
            }
            catch (Exception ex)
            {
                strError = "Marc2Xml()中XML数据装入DOM时出错: " + ex.Message;
                return -1;
            }

            s.Close();
            return 0;
        }
#endif

        // 包装后的版本
        public static int LoadXmlFragment(string strXml,
    out string strXmlFragment,
    out string strError)
        {
            strXmlFragment = "";
            XmlDocument domXmlFragment = null;
            int nRet = LoadXmlFragment(strXml, out domXmlFragment, out strError);
            if (nRet == -1)
                return -1;
            if (domXmlFragment != null && domXmlFragment.DocumentElement != null)
                strXmlFragment = domXmlFragment.DocumentElement.OuterXml;
            return nRet;
        }

        // 装载书目以外的其它XML片断
        static int LoadXmlFragment(string strXml,
            out XmlDocument domXmlFragment,
            out string strError)
        {
            strError = "";

            domXmlFragment = null;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            nsmgr.AddNamespace("unimarc", DpNs.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//unimarc:leader | //unimarc:controlfield | //unimarc:datafield | //usmarc:leader | //usmarc:controlfield | //usmarc:datafield", nsmgr); // | //dprms:file
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            domXmlFragment = new XmlDocument();
            domXmlFragment.LoadXml("<root />");
            domXmlFragment.DocumentElement.InnerXml = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 2013/3/5
        // 将 MARC 格式转换为 MARCXML 格式，替换已有的 XML 中的相关部分，保留其他部分
        public static int Marc2XmlEx(string strMARC,
            string strMarcSyntax,
            ref string strXml,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml) == true)
            {
                return Marc2Xml(strMARC, strMarcSyntax, out strXml, out strError);
            }

            XmlDocument domXmlFragment = null;

            // 装载书目以外的其它XML片断
            int nRet = LoadXmlFragment(strXml,
    out domXmlFragment,
    out strError);
            if (nRet == -1)
                return -1;

            XmlDocument domMarc = null;
            nRet = MarcUtil.Marc2Xml(strMARC,
                strMarcSyntax,
                out domMarc,
                out strError);
            if (nRet == -1)
                return -1;

            // 合成其它XML片断
            if (domXmlFragment != null
                && string.IsNullOrEmpty(domXmlFragment.DocumentElement.InnerXml) == false)
            {
                XmlDocumentFragment fragment = domMarc.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = domXmlFragment.DocumentElement.InnerXml;
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                domMarc.DocumentElement.AppendChild(fragment);
            }
            strXml = domMarc.OuterXml;
            return 0;
        }

        // 2008/5/16
        // 将MARC记录转换为xml格式
        // parameters:
        //      strMarcSyntax   MARC格式．为 unimarc/usmarc之一，缺省为unimarc
        public static int Marc2Xml(string strMARC,
            string strMarcSyntax,
            out XmlDocument domMarc,
            out string strError)
        {
            strError = "";
            domMarc = null;

            // MARC控件中内容更新一些. 需要刷新到xml控件中
            using (MemoryStream s = new MemoryStream())
            using (MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8))
            {
                if (strMarcSyntax == "unimarc")
                {
                    writer.MarcNameSpaceUri = DpNs.unimarcxml;
                    writer.MarcPrefix = strMarcSyntax;
                }
                else if (strMarcSyntax == "usmarc")
                {
                    writer.MarcNameSpaceUri = Ns.usmarcxml;
                    writer.MarcPrefix = strMarcSyntax;
                }
                else
                {
                    writer.MarcNameSpaceUri = DpNs.unimarcxml;
                    writer.MarcPrefix = "unimarc";
                }

                int nRet = writer.WriteRecord(strMARC,
                    out strError);
                if (nRet == -1)
                    return -1; ;

                writer.Flush();
                s.Flush();

                s.Seek(0, SeekOrigin.Begin);

                domMarc = new XmlDocument();
                try
                {
                    domMarc.Load(s);
                }
                catch (Exception ex)
                {
                    strError = "Marc2Xml()中XML数据装入DOM时出错: " + ex.Message;
                    return -1;
                }

                return 0;
            }
        }

#if NO
        // 因为使用了 XmlDocument 而速度较慢
		// 将MARC记录转换为xml格式
        // parameters:
        //      strMarcSyntax   MARC格式．为 unimarc/usmarc之一，缺省为unimarc
		public static int Marc2Xml(string strMARC,
			string strMarcSyntax,
			out string strXml,
			out string strError)
		{
			strError = "";
			strXml = "";

			// MARC控件中内容更新一些. 需要刷新到xml控件中
			MemoryStream s = new MemoryStream();

			MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8);

			if (strMarcSyntax == "unimarc")
			{
				writer.MarcNameSpaceUri = DpNs.unimarcxml;
				writer.MarcPrefix = strMarcSyntax;
			}
			else if (strMarcSyntax == "usmarc")
			{
				writer.MarcNameSpaceUri = Ns.usmarcxml;
				writer.MarcPrefix = strMarcSyntax;
			}
			else 
			{
				writer.MarcNameSpaceUri = DpNs.unimarcxml;
				writer.MarcPrefix = "unimarc";
			}

			int nRet = writer.WriteRecord(strMARC,
				out strError);
			if (nRet == -1)
				return -1;;

			writer.Flush();
			s.Flush();
					
			s.Seek(0, SeekOrigin.Begin);

			XmlDocument domMarc = new XmlDocument();
			try 
			{
				domMarc.Load(s);
			}
			catch (Exception ex)
			{
                strError = "Marc2Xml()中XML数据装入DOM时出错: " + ex.Message;
				return -1;
			}

			strXml = domMarc.OuterXml;

			s.Close();

			return 0;
		}

#endif

        // 获得 MARCXML 字符串的 MARC 格式类型
        // return:
        //      -1  出错
        //      0   无法探测
        //      1   成功探测
        public static int GetMarcSyntax(string strXml,
out string strMarcSyntax,
out string strError)
        {
            strError = "";
            strMarcSyntax = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;  // 在意空白符号
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "加载 XML 到 DOM 时出错: " + ex.Message;
                return -1;
            }

            return GetMarcSyntax(dom,
out strMarcSyntax,
out strError);
        }

        // 获得 MARCXML 字符串的 MARC 格式类型
        // return:
        //      -1  出错
        //      0   无法探测
        //      1   成功探测
        public static int GetMarcSyntax(XmlDocument dom,
    out string strMarcSyntax,
    out string strError)
        {
            strError = "";
            strMarcSyntax = "";

            // 取MARC根
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            XmlElement root = null;
            {
                // '//'保证了无论MARC的根在何处，都可以正常取出。
                root = dom.DocumentElement.SelectSingleNode("//unimarc:record", nsmgr) as XmlElement;
                if (root == null)
                {
                    root = dom.DocumentElement.SelectSingleNode("//usmarc:record", nsmgr) as XmlElement;

                    if (root == null)
                        return 0;

                    strMarcSyntax = "usmarc";
                }
                else
                    strMarcSyntax = "unimarc";
            }

            return 1;
        }

        // 将MARC记录转换为xml格式
        // 2015/5/10 改进了函数性能，采用 StringWriter 获取字符串结果
        // parameters:
        //      strMarcSyntax   MARC格式．为 unimarc/usmarc之一，缺省为unimarc
        public static int Marc2Xml(string strMARC,
            string strMarcSyntax,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            // MARC控件中内容更新一些. 需要刷新到xml控件中
            using (StringWriter s = new StringWriter())
            using (MarcXmlWriter writer = new MarcXmlWriter(s))
            {
                if (strMarcSyntax == "unimarc")
                {
                    writer.MarcNameSpaceUri = DpNs.unimarcxml;
                    writer.MarcPrefix = strMarcSyntax;
                }
                else if (strMarcSyntax == "usmarc")
                {
                    writer.MarcNameSpaceUri = Ns.usmarcxml;
                    writer.MarcPrefix = strMarcSyntax;
                }
                else
                {
                    writer.MarcNameSpaceUri = DpNs.unimarcxml;
                    writer.MarcPrefix = "unimarc";
                }

                int nRet = writer.WriteRecord(strMARC,
                    out strError);
                if (nRet == -1)
                    return -1;

                writer.Flush();

                strXml = s.ToString();
                return 0;
            }
        }

        // 包装以后的版本
        public static int Xml2Marc(XmlDocument dom,
            bool bWarning,
            string strMarcSyntax,
            out string strOutMarcSyntax,
            out string strMARC,
            out string strError)
        {
            // Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");
            string strFragmentXml = "";
            return Xml2Marc(dom,
                bWarning ? Xml2MarcStyle.Warning : Xml2MarcStyle.None,
                strMarcSyntax,
                out strOutMarcSyntax,
                out strMARC,
                out strFragmentXml,
                out strError);
        }

        // 包装以后的版本
        public static int Xml2Marc(string strXml,
            bool bWarning,
            string strMarcSyntax,
            out string strOutMarcSyntax,
            out string strMARC,
            out string strError)
        {
            // Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");
            string strFragmentXml = "";
            return Xml2Marc(strXml,
                bWarning ? Xml2MarcStyle.Warning : Xml2MarcStyle.None,
                strMarcSyntax,
                out strOutMarcSyntax,
                out strMARC,
                out strFragmentXml,
                out strError);
        }

        [Flags]
        public enum Xml2MarcStyle
        {
            None = 0,
            Warning = 0x1,
            OutputFragmentXml = 0x02,
        }

        // 将MARCXML格式的xml记录转换为marc机内格式字符串
        // 注意，如果strXml内容为空，本函数会报错。最好在进入函数前进行判断。
        // parameters:
        //		bWarning	        ==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
        //		strMarcSyntax	    指示marc语法,如果==""，则自动识别
        //		strOutMarcSyntax	[out] 返回记录的 MARC 格式。如果 strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
        //      strFragmentXml      [out] 返回删除 <leader> <controlfield> <datafield> 以后的 XML 代码。注意，包含 <record> 元素
        public static int Xml2Marc(string strXml,
            Xml2MarcStyle style,
            string strMarcSyntax,
            out string strOutMarcSyntax,
            out string strMARC,
            out string strFragmentXml,
            out string strError)
        {
            strMARC = "";
            strError = "";
            strOutMarcSyntax = "";
            strFragmentXml = "";

            // 2013/9/25
            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            Debug.Assert(string.IsNullOrEmpty(strXml) == false, "");

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;  // 在意空白符号
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "Xml2Marc() strXml 加载 XML 到 DOM 时出错: " + ex.Message;
                return -1;
            }

            return Xml2Marc(dom,
    style,
    strMarcSyntax,
    out strOutMarcSyntax,
    out strMARC,
    out strFragmentXml,
    out strError);
        }

        public static int Xml2Marc(XmlDocument dom,
    Xml2MarcStyle style,
    string strMarcSyntax,
    out string strOutMarcSyntax,
    out string strMARC,
    out string strFragmentXml,
    out string strError)
        {
            strMARC = "";
            strError = "";
            strOutMarcSyntax = "";
            strFragmentXml = "";

            if (dom.DocumentElement == null)
                return 0;

            bool bWarning = (style & Xml2MarcStyle.Warning) != 0;
            bool bOutputFragmentXml = (style & Xml2MarcStyle.OutputFragmentXml) != 0;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);

            // 取 MARC 根元素兼探测 MARC 格式类型
            XmlNode root = null;
            if (string.IsNullOrEmpty(strMarcSyntax) == true)
            {
                // '//'保证了无论MARC的根在何处，都可以正常取出。
                root = dom.DocumentElement.SelectSingleNode("//unimarc:record", nsmgr);
                if (root == null)
                {
                    root = dom.DocumentElement.SelectSingleNode("//usmarc:record", nsmgr);
                    if (root == null)
                    {
                        // TODO: 是否要去除所有 MARC 相关元素
                        if (bOutputFragmentXml)
                            strFragmentXml = dom.DocumentElement.OuterXml;
                        return 0;
                    }

                    strMarcSyntax = "usmarc";
                }
                else
                {
                    strMarcSyntax = "unimarc";
                }
            }
            else
            {
                // 2012/1/8
                if (strMarcSyntax != null)
                    strMarcSyntax = strMarcSyntax.ToLower();

                if (strMarcSyntax != "unimarc"
                    && strMarcSyntax != "usmarc")
                {
                    strError = "无法识别 MARC格式 '" + strMarcSyntax + "' 。目前仅支持 unimarc 和 usmarc 两种格式";
                    return -1;
                }

                root = dom.DocumentElement.SelectSingleNode("//" + strMarcSyntax + ":record", nsmgr);
                if (root == null)
                {
                    // TODO: 是否要去除所有 MARC 相关元素
                    if (bOutputFragmentXml)
                        strFragmentXml = dom.DocumentElement.OuterXml;
                    return 0;
                }
            }

            StringBuilder strMarc = new StringBuilder(4096);

            strOutMarcSyntax = strMarcSyntax;

            XmlNode leader = root.SelectSingleNode(strMarcSyntax + ":leader", nsmgr);
            if (leader == null)
            {
                strError += "缺<" + strMarcSyntax + ":leader>元素\r\n";
                if (bWarning == false)
                    return -1;
                else
                    strMarc.Append("012345678901234567890123");
            }
            else // 正常情况
            {
                // string strLeader = DomUtil.GetNodeText(leader);
                // GetNodeText()会自动Trim()，会导致头标区内容末尾丢失字符
                string strLeader = leader.InnerText;
                if (strLeader.Length != 24)
                {
                    strError += "<" + strMarcSyntax + ":leader>元素内容应为24字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strLeader.Length < 24)
                            strLeader = strLeader.PadRight(24, ' ');
                        else
                            strLeader = strLeader.Substring(0, 24);
                    }
                }

                strMarc.Append(strLeader);

                // 从 DOM 中删除 leader 元素
                if (bOutputFragmentXml)
                    leader.ParentNode.RemoveChild(leader);
            }

            int i = 0;

            // 固定长字段
            XmlNodeList controlfields = root.SelectNodes(strMarcSyntax + ":controlfield", nsmgr);
            for (i = 0; i < controlfields.Count; i++)
            {
                XmlNode field = controlfields[i];
                string strTag = DomUtil.GetAttr(field, "tag");
                if (strTag.Length != 3)
                {
                    strError += "<" + strMarcSyntax + ":controlfield>元素的tag属性值'" + strTag + "'应当为3字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strTag.Length < 3)
                            strTag = strTag.PadRight(3, '*');
                        else
                            strTag = strTag.Substring(0, 3);
                    }
                }

                string strContent = DomUtil.GetNodeText(field);

                strMarc.Append(strTag + strContent + new string(MarcUtil.FLDEND, 1));

                // 从 DOM 中删除
                if (bOutputFragmentXml)
                    field.ParentNode.RemoveChild(field);
            }

            // 可变长字段
            XmlNodeList datafields = root.SelectNodes(strMarcSyntax + ":datafield", nsmgr);
            for (i = 0; i < datafields.Count; i++)
            {
                XmlNode field = datafields[i];
                string strTag = DomUtil.GetAttr(field, "tag");
                if (strTag.Length != 3)
                {
                    strError += "<" + strMarcSyntax + ":datafield>元素的tag属性值'" + strTag + "'应当为3字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strTag.Length < 3)
                            strTag = strTag.PadRight(3, '*');
                        else
                            strTag = strTag.Substring(0, 3);
                    }
                }

                string strInd1 = DomUtil.GetAttr(field, "ind1");
                if (strInd1.Length != 1)
                {
                    strError += "<" + strMarcSyntax + ":datalfield>元素的ind1属性值'" + strInd1 + "'应当为1字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strInd1.Length < 1)
                            strInd1 = '*'.ToString();
                        else
                            strInd1 = strInd1[0].ToString();
                    }
                }

                string strInd2 = DomUtil.GetAttr(field, "ind2");
                if (strInd2.Length != 1)
                {
                    strError += "<" + strMarcSyntax + ":datalfield>元素的indi2属性值'" + strInd2 + "'应当为1字符\r\n";
                    if (bWarning == false)
                        return -1;
                    else
                    {
                        if (strInd2.Length < 1)
                            strInd2 = '*'.ToString();
                        else
                            strInd2 = strInd2[0].ToString();
                    }
                }

                // string strContent = DomUtil.GetNodeText(field);
                XmlNodeList subfields = field.SelectNodes(strMarcSyntax + ":subfield", nsmgr);
                StringBuilder strContent = new StringBuilder(4096);
                for (int j = 0; j < subfields.Count; j++)
                {
                    XmlNode subfield = subfields[j];

                    XmlAttribute attr = subfield.Attributes["code"];
#if NO
					string strCode = DomUtil.GetAttr(subfield, "code");
					if (strCode.Length != 1)
					{
						strError += "<"+strMarcSyntax+":subfield>元素的code属性值'"+strCode+"'应当为1字符\r\n";
						if (bWarning == false)
							return -1;
						else 
						{
							if (strCode.Length < 1)
								strCode = '*'.ToString();
							else
								strCode = strCode[0].ToString();
						}
					}

                    string strSubfieldContent = DomUtil.GetNodeText(subfield);

					strContent += new string(MarcUtil.SUBFLD,1) + strCode + strSubfieldContent;

#endif
                    if (attr == null)
                    {
                        // 前导纯文本
                        strContent.Append(DomUtil.GetNodeText(subfield));
                        continue;   //  goto CONTINUE; BUG!!!
                    }

                    string strCode = attr.Value;
                    if (strCode.Length != 1)
                    {
                        strError += "<" + strMarcSyntax + ":subfield>元素的 code 属性值 '" + strCode + "' 应当为1字符\r\n";
                        if (bWarning == false)
                            return -1;
                        else
                        {
                            if (strCode.Length < 1)
                                strCode = "";   // '*'.ToString();
                            else
                                strCode = strCode[0].ToString();
                        }
                    }

                    string strSubfieldContent = DomUtil.GetNodeText(subfield);
                    strContent.Append(new string(MarcUtil.SUBFLD, 1) + strCode + strSubfieldContent);
                }

                strMarc.Append(strTag + strInd1 + strInd2 + strContent + new string(MarcUtil.FLDEND, 1));

            CONTINUE:
                // 从 DOM 中删除
                if (bOutputFragmentXml)
                    field.ParentNode.RemoveChild(field);
            }

            strMARC = strMarc.ToString();
            if (bOutputFragmentXml)
                strFragmentXml = dom.DocumentElement.OuterXml;

            return 0;
        }


        // 将marcxchange格式转化为机内使用的marcxml格式
        public static int MarcXChangeToXml(string strSource,
            out string strTarget,
            out string strError)
        {
            strError = "";
            strTarget = "";

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;  // 在意空白符号
            try
            {
                dom.LoadXml(strSource);
            }
            catch (Exception ex)
            {
                strError = "源加载到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            XmlDocument target_dom = new XmlDocument();
            string strMarcNs = "";
            string strPrefix = "marc";

            string strMarcXUri = "info:lc/xmlns/marcxchange-v1";

            // 取MARC根
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);
            nsmgr.AddNamespace("marcxchange", strMarcXUri);

            XmlNode root = null;

            // '//'保证了无论MARC的根在何处，都可以正常取出。
            root = dom.DocumentElement.SelectSingleNode("//marcxchange:record", nsmgr);
            if (root == null)
            {
                strError = "源中不存在<record>元素";
                return -1;
            }

            string strFormat = DomUtil.GetAttr(root, "format");
            if (strFormat != null)
                strFormat = strFormat.ToLower();
            if (strFormat == "unimarc")
            {
                strMarcNs = Ns.unimarcxml;
                strPrefix = "unimarc";
                target_dom.AppendChild(target_dom.CreateElement(strPrefix + ":record", strMarcNs));

            }
            else if (strFormat == "marc21")
            {
                strMarcNs = Ns.usmarcxml;
                strPrefix = "usmarc";
                target_dom.AppendChild(target_dom.CreateElement(strPrefix + ":record", strMarcNs));
            }
            else
            {
                strError = "源的<record>元素的format属性值目前仅支持UNIMARC和MARC21";
                return -1;
            }

            foreach (XmlNode field in root.ChildNodes)
            {
                if (field.NodeType != XmlNodeType.Element)
                    continue;
                XmlNode target_field = null;
                if (field.LocalName == "leader" && field.NamespaceURI == strMarcXUri)
                {
                    target_field = target_dom.CreateElement(strPrefix + ":leader", strMarcNs);
                    target_dom.DocumentElement.AppendChild(target_field);
                    target_field.InnerText = field.InnerText;
                    continue;
                }

                if (field.LocalName == "controlfield" && field.NamespaceURI == strMarcXUri)
                {
                    target_field = target_dom.CreateElement(strPrefix + ":controlfield", strMarcNs);
                    target_dom.DocumentElement.AppendChild(target_field);
                    target_field.InnerText = field.InnerText;
                    foreach (XmlAttribute attr in field.Attributes)
                    {
                        // target_field.Attributes.Append(attr);
                        DomUtil.SetAttr(target_field, attr.Name, attr.Value);
                    }
                    continue;
                }

                if (field.LocalName == "datafield" && field.NamespaceURI == strMarcXUri)
                {
                    target_field = target_dom.CreateElement(strPrefix + ":datafield", strMarcNs);
                    target_dom.DocumentElement.AppendChild(target_field);
                    foreach (XmlAttribute attr in field.Attributes)
                    {
                        // target_field.Attributes.Append(attr);
                        DomUtil.SetAttr(target_field, attr.Name, attr.Value);
                    }

                    foreach (XmlNode child in field.ChildNodes)
                    {
                        if (child.LocalName == "subfield" && child.NamespaceURI == strMarcXUri)
                        {
                            XmlNode target_subfield = target_dom.CreateElement(strPrefix + ":subfield", strMarcNs);
                            target_field.AppendChild(target_subfield);
                            target_subfield.InnerText = child.InnerText;
                            foreach (XmlAttribute attr in child.Attributes)
                            {
                                DomUtil.SetAttr(target_subfield, attr.Name, attr.Value);
                            }
                        }
                    }
                    continue;
                }
            }

            strTarget = target_dom.OuterXml;

            return 0;
        }

        // 将机内使用的marcxml格式转化为marcxchange格式
        public static int MarcXmlToXChange(string strSource,
            string strType,
            out string strTarget,
            out string strError)
        {
            strError = "";
            strTarget = "";

            if (String.IsNullOrEmpty(strType) == true)
                strType = "Bibliographic";

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;  // 在意空白符号
            try
            {
                dom.LoadXml(strSource);
            }
            catch (Exception ex)
            {
                strError = "源加载到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            XmlDocument target_dom = new XmlDocument();
            string strMarcNs = "";
            string strPrefix = "marc";
            string strFormat = "";

            string strMarcXUri = "info:lc/xmlns/marcxchange-v1";

            // 取MARC根
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("unimarc", Ns.unimarcxml);
            nsmgr.AddNamespace("usmarc", Ns.usmarcxml);
            nsmgr.AddNamespace("marcxchange", strMarcXUri);

            // 
            XmlNode root = null;

            // '//'保证了无论MARC的根在何处，都可以正常取出。
            root = dom.DocumentElement.SelectSingleNode("//unimarc:record", nsmgr);
            if (root == null)
            {
                root = dom.DocumentElement.SelectSingleNode("//usmarc:record", nsmgr);

                if (root == null)
                {
                    strError = "源中unimarc和unimarc的<record>元素均不存在";
                    return -1;
                }

                strMarcNs = Ns.usmarcxml;
                strPrefix = "usmarc";
                strFormat = "MARC21";
            }
            else
            {
                strMarcNs = Ns.unimarcxml;
                strPrefix = "unimarc";
                strFormat = "UNIMARC";
            }

            target_dom.AppendChild(target_dom.CreateElement("marc:record", strMarcXUri));
            DomUtil.SetAttr(target_dom.DocumentElement,
                "format",
                strFormat);
            DomUtil.SetAttr(target_dom.DocumentElement,
    "type",
    strType);

            foreach (XmlNode source_field in root.ChildNodes)
            {
                if (source_field.NodeType != XmlNodeType.Element)
                    continue;
                XmlNode target_field = null;
                if (source_field.LocalName == "leader" && source_field.NamespaceURI == strMarcNs)
                {
                    target_field = target_dom.CreateElement("marc:leader", strMarcXUri);
                    target_dom.DocumentElement.AppendChild(target_field);
                    target_field.InnerText = source_field.InnerText;
                    continue;
                }

                if (source_field.LocalName == "controlfield" && source_field.NamespaceURI == strMarcNs)
                {
                    target_field = target_dom.CreateElement("marc:controlfield", strMarcXUri);
                    target_dom.DocumentElement.AppendChild(target_field);
                    target_field.InnerText = source_field.InnerText;
                    foreach (XmlAttribute attr in source_field.Attributes)
                    {
                        // target_field.Attributes.Append(attr);
                        DomUtil.SetAttr(target_field, attr.Name, attr.Value);
                    }
                    continue;
                }

                if (source_field.LocalName == "datafield" && source_field.NamespaceURI == strMarcNs)
                {
                    target_field = target_dom.CreateElement("marc:datafield", strMarcXUri);
                    target_dom.DocumentElement.AppendChild(target_field);
                    foreach (XmlAttribute attr in source_field.Attributes)
                    {
                        // target_field.Attributes.Append(attr);
                        DomUtil.SetAttr(target_field, attr.Name, attr.Value);
                    }

                    foreach (XmlNode child in source_field.ChildNodes)
                    {
                        if (child.LocalName == "subfield" && child.NamespaceURI == strMarcNs)
                        {
                            XmlNode target_subfield = target_dom.CreateElement("marc:subfield", strMarcXUri);
                            target_field.AppendChild(target_subfield);
                            target_subfield.InnerText = child.InnerText;
                            foreach (XmlAttribute attr in child.Attributes)
                            {
                                DomUtil.SetAttr(target_subfield, attr.Name, attr.Value);
                            }
                        }
                    }
                    continue;
                }
            }

            strTarget = target_dom.OuterXml;

            return 0;
        }

        // 从marcdef配置文件中得到marc格式字符串
        // 用Stream的原因，是为了提高执行速度
        // return:
        //		-1	出错
        //		0	没有找到
        //		1	找到
        public static int GetMarcSyntaxFromCfgFile(Stream s,
            out string strMarcSyntax,
            out string strError)
        {
            strError = "";
            strMarcSyntax = "";
            try
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "MARCSyntax")
                            {
                                strMarcSyntax = reader.ReadString().ToLower();
                                return 1;
                            }
                        }
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }
        }

        #region 处理MARC记录各种内部结构的静态函数

        // 包装后的版本 2010/11/15
        // 替换字段内的第一个子字段的内容
        // return:
        //		-1	出错
        //		0	指定的子字段没有找到，因此将新内容插入到适当地方了。
        //		1	找到了指定的字段，并且也成功用新内容替换掉了。
        public static int ReplaceSubfieldContent(ref string strField,
            string strSubfieldName,
            string strContent)
        {
            Debug.Assert(strSubfieldName.Length == 1, "");

            // return:
            //		-1	出错
            //		0	指定的子字段没有找到，因此将strSubfieldzhogn的内容插入到适当地方了。
            //		1	找到了指定的字段，并且也成功用strSubfield内容替换掉了。
            return ReplaceSubfield(
                ref strField,
                strSubfieldName,
                0,
                strSubfieldName + strContent);
        }

        // 包装后的版本 2010/11/15
        // 获得MARC记录中的第一个字段
        // return:
        //		null	出错
        //      ""      没有找到
        //		其他    字段名 + [字段指示符 +] 字段内容
        public static string GetField(string strMARC,
            string strFieldName)
        {
            int nRet = 0;

            string strField = "";
            string strNextFieldName = "";

            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = GetField(strMARC,
            strFieldName,
            0,
            out strField,
            out strNextFieldName);
            if (nRet == -1)
                return null;
            if (nRet == 0)
                return "";

            return strField;
        }

        // 包装后版本 2010/11/15
        // 获得字段内的第一个子字段的内容
        public static string GetSubfieldContent(string strField,
            string strSubfieldName)
        {
            string strSubfield = "";
            string strNextSubfieldName = "";
            // return:
            //		-1	出错
            //		0	所指定的子字段没有找到
            //		1	找到。找到的子字段返回在strSubfield参数中
            int nRet = GetSubfield(strField,
                ItemType.Field,
            strSubfieldName,
            0,
            out strSubfield,
            out strNextSubfieldName);
            if (nRet == -1)
                return null;
            if (nRet == 0)
                return "";
            if (String.IsNullOrEmpty(strSubfield) == true)
                return "";

            return strSubfield.Substring(1);
        }

        // 2009/11/25
        // 替换第一个子字段
        // parameters:
        //      strMARC MARC记录
        //      strFieldName    字段名
        //      strSubfieldName 子字段名。一字符
        //      strValue    子字段内容。如果为null，表示要删除这个子字段
        static public void SetFirstSubfield(ref string strMARC,
            string strFieldName,
            string strSubfieldName,
            string strValue)
        {
            string strField = "";
            string strNextFieldName = "";

            // return:
            //		-1	error
            //		0	not found
            //		1	found
            int nRet = GetField(strMARC,
                strFieldName,
                0,
                out strField,
                out strNextFieldName);
            if (nRet != 1)
            {
                if (strValue == null)
                    return; // 正好连字段都不存在，就返回

                strField = strFieldName + "  ";
            }

            //		strSubfield	要替换成的新子字段。注意，其中第一字符为子字段名，后面为子字段内容
            // return:
            //		-1	出错
            //		0	指定的子字段没有找到，因此将strSubfieldzhogn的内容插入到适当地方了。
            //		1	找到了指定的字段，并且也成功用strSubfield内容替换掉了。
            nRet = ReplaceSubfield(
                ref strField,
                strSubfieldName,
                0,
                strValue == null ? null : strSubfieldName + strValue);

            //		strField	要替换成的新字段内容。包括字段名、必要的字段指示符、字段内容。这意味着，不但可以替换一个字段的内容，也可以替换它的字段名和指示符部分。
            // return:
            //		-1	出错
            //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
            //		1	找到了指定的字段，并且也成功用strField替换掉了。
            nRet = ReplaceField(
                ref strMARC,
                strFieldName,
                0,
                strField);
        }

        // 以字段/子字段名从记录中得到第一个子字段内容。
        // parameters:
        //		strMARC	机内格式MARC记录
        //		strFieldName	字段名。内容为字符
        //		strSubfieldName	子字段名。内容为1字符
        // return:
        //		""	空字符串。表示没有找到指定的字段或子字段。
        //		其他	子字段内容。注意，这是子字段纯内容，其中并不包含子字段名。
        static public string GetFirstSubfield(string strMARC,
            string strFieldName,
            string strSubfieldName)
        {
            string strField = "";
            string strNextFieldName = "";


            // return:
            //		-1	error
            //		0	not found
            //		1	found
            int nRet = GetField(strMARC,
                strFieldName,
                0,
                out strField,
                out strNextFieldName);

            if (nRet != 1)
                return "";

            string strSubfield = "";
            string strNextSubfieldName = "";

            // return:
            //		-1	error
            //		0	not found
            //		1	found
            nRet = GetSubfield(strField,
                ItemType.Field,
                strSubfieldName,
                0,
                out strSubfield,
                out strNextSubfieldName);
            if (strSubfield.Length < 1)
                return "";

            return strSubfield.Substring(1);
        }

        // 看一个字段名是否是控制字段。所谓控制字段没有指示符概念
        // parameters:
        //		strFieldName	字段名
        // return:
        //		true	是控制字段
        //		false	不是控制字段
        public static bool IsControlFieldName(string strFieldName)
        {
#if NO
            if (String.Compare(strFieldName, "hdr", true) == 0)
                return true;

            if (String.Compare(strFieldName, "###", true) == 0)
                return true;

                        if (
                (
                String.Compare(strFieldName, "001") >= 0
                && String.Compare(strFieldName, "009") <= 0
                )

                || String.Compare(strFieldName, "-01") == 0
                )
                return true;
#endif
            if (strFieldName == null || strFieldName.Length < 3)
                throw new ArgumentException("strFieldName 参数值应当是 3 字符的字符串");

            if (strFieldName[0] == '0' && strFieldName[1] == '0')
                return true;
            if (strFieldName == "hdr" || strFieldName == "###" || strFieldName == "-01")
                return true;
            return false;
        }

        public static List<string> GetFields(
            string strMARC,
            string strFieldName,
            string strIndicatorMatch = "**")
        {
            List<string> results = new List<string>();
            for (int i = 0; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                int nRet = GetField(strMARC,
                    strFieldName,
                    i,
            out strField,
            out strNextFieldName);
                if (nRet != 1)
                    break;
                if (string.IsNullOrEmpty(strField) == true)
                    continue;
                if (strField.Length < 3)
                    continue;
                string strIndicator = "";

                if (IsControlFieldName(strField.Substring(0, 3)) == true)
                {
                }
                else
                {
                    if (strField.Length >= 5)
                        strIndicator = strField.Substring(3, 2);
                    else
                        strIndicator = strField.Substring(3, 1);
                }

                if (MarcUtil.MatchIndicator(strIndicatorMatch, strIndicator) == true)
                {
                    results.Add(strField);
                }

            }

            return results;
        }

        // 取指定名称字段的子字段。还能依据字段的指示符进行筛选
        // parameters:
        //      strFieldName    3字符的字段名
        //      strSubfieldName 可以为一个或者多个字符。每个字符代表一个子字段名
        // return:
        //      字符串数组。每个元素为子字段内容，不包含子字段名
        public static List<string> GetSubfields(
            string strMARC,
            string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch = "**")
        {
            List<string> results = new List<string>();
            List<string> fields = GetFields(strMARC,
                strFieldName,
                strIndicatorMatch);
            foreach (string strField in fields)
            {

                for (int i = 0; ; i++)
                {
                    string strSubfield = "";
                    string strNextSubfieldName = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    int nRet = GetSubfield(strField,
                        ItemType.Field,
                    strSubfieldName,
                    i,
                    out strSubfield,
                    out strNextSubfieldName);
                    if (nRet != 1)
                        break;
                    if (string.IsNullOrEmpty(strSubfield) == true)
                        continue;
                    strSubfield = strSubfield.Substring(1); // 去掉子字段名
                    if (string.IsNullOrEmpty(strSubfield) == true)
                        continue;
                    results.Add(strSubfield);
                }
            }
            return results;
        }


        // 单元测试!

        // 从记录中得到一个字段
        // parameters:
        //		strMARC		机内格式MARC记录
        //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
        //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个则表示头标区)
        //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
        //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
        //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
        // return:
        //		-1	出错
        //		0	所指定的字段没有找到
        //		1	找到。找到的字段返回在strField参数中
        public static int GetField(string strMARC,
            string strFieldName,
            int nIndex,
            out string strField,
            out string strNextFieldName)
        {
            //			LPTSTR p;
            int nFoundCount = 0;
            int nChars = 0;
            string strCurFldName;


            strField = null;
            strNextFieldName = null;


            if (strMARC == null)
            {
                Debug.Assert(false, "strMARC参数不能为null");
                return -1;
            }


            if (strMARC.Length < 24)
                return -1;

            if (strFieldName != null)
            {
                if (strFieldName.Length != 3)
                {
                    Debug.Assert(false, "字段名长度必须为3");	// 字段名必须为3字符
                    return -1;
                }
            }
            else
            {
                // 表示不关心字段名，依靠nIndex来定位字段
            }

            strField = "";

            char ch;

            // 循环，找特定的字段

            // p = (LPTSTR)pszMARC;
            for (int i = 0; i < strMARC.Length;)
            {
                ch = strMARC[i];

                if (ch == RECEND)
                    break;

                // 设置m_strItemName
                if (i == 0)
                {

                    if ((nIndex == 0 && strFieldName == null)	// 头标区
                        ||
                        (strFieldName != null
                        && String.Compare("hdr", strFieldName, true) == 0) // 有字段名要求，并且要求头标区
                        )
                    {
                        strField = strMARC.Substring(0, 24);

                        // 取strNextFieldName
                        strNextFieldName = GetNextFldName(strMARC,
                            24);
                        return 1;	// found
                    }
                    nChars = 24;

                    if (strFieldName == null
                        || (strFieldName != null
                        && "hdr" == strFieldName)
                        )
                    {
                        nFoundCount++;
                    }

                }
                else
                {
                    nChars = DetectFldLens(strMARC, i);
                    if (nChars < 3 + 1)
                    {
                        strCurFldName = "???";	// ???
                        goto SKIP;
                    }
                    Debug.Assert(nChars >= 3 + 1, "");
                    strCurFldName = strMARC.Substring(i, 3);
                    if (strFieldName == null
                        || (strFieldName != null
                        && strCurFldName == strFieldName)
                        )
                    {
                        if (nIndex == nFoundCount)
                        {
                            strField = strMARC.Substring(i, nChars - 1);	// 不包含字段结束符

                            // 取strNextFieldName
                            strNextFieldName = GetNextFldName(strMARC,
                                i + nChars);

                            /*
                            if (i+nChars < strMARC.Length
                                && strMARC[i+nChars] != RECEND
                                && DetectFldLens(strMARC, i+nChars) >= 3 ) 
                            {
                                strNextFieldName = strMARC.Substring(i+nChars, 3);
                                for(int j=0;j<strNextFieldName.Length;j++) 
                                {
                                    char ch0 = strNextFieldName[j];
                                    if (ch0 == RECEND
                                        || ch0 == SUBFLD 
                                        || ch0 == FLDEND)
                                        strNextFieldName = strNextFieldName.Insert(j, "?").Remove(j+1, 1);

                                }
                            }
                            else
                                strNextFieldName = "";
                            */

                            return 1;	// found
                        }
                        nFoundCount++;
                    }
                }

            SKIP:
                i += nChars;
            }
            return 0;	// not found
        }

        // nStart需正好为字段名字符位置
        // 本函数为函数GetField()服务
        static string GetNextFldName(string strMARC,
            int nStart)
        {
            string strNextFieldName = "";

            if (nStart < strMARC.Length
                && strMARC[nStart] != RECEND
                && DetectFldLens(strMARC, nStart) >= 3)
            {
                strNextFieldName = strMARC.Substring(nStart, 3);
                for (int j = 0; j < strNextFieldName.Length; j++)
                {
                    char ch0 = strNextFieldName[j];
                    if (ch0 == RECEND
                        || ch0 == SUBFLD
                        || ch0 == FLDEND)
                        strNextFieldName = strNextFieldName.Insert(j, "?").Remove(j + 1, 1);

                }
            }
            else
                strNextFieldName = "";

            return strNextFieldName;
        }

        // 根据字段开始位置探测整个字段长度
        // 返回字段的长度
        // 这是包含记录结束符号在内的长度
        public static int DetectFldLens(string strText,
            int nStart)
        {
            Debug.Assert(strText != null, "strText参数不能为null");
            int nChars = 0;
            // LPTSTR p = (LPTSTR)pFldStart;
            for (; nStart < strText.Length; nStart++)
            {
                if (strText[nStart] == FLDEND)
                {
                    nChars++;
                    break;
                }
                nChars++;
            }

            return nChars;
        }

        // 根据子字段开始位置探测整个子字段长度
        // nStart位置是这个子字段的子字段符号位置
        // 返回子字段的长度(字符数，不是byte数)
        public static int DetectSubFldLens(string strField,
            int nStart)
        {
            Debug.Assert(strField != null, "strField参数不能为null");
            Debug.Assert(strField[nStart] == SUBFLD, "nStart位置必须是一个子字段符号");
            Debug.Assert(nStart < strField.Length, "nStart不能越过strField长度");
            int nChars = 0;
            nStart++;
            nChars++;
            for (; nStart < strField.Length; nStart++)
            {
                if (strField[nStart] == SUBFLD)
                {
                    break;
                }
                nChars++;
            }

            return nChars;
        }


        // 得到一个嵌套记录中的字段
        // parameters:
        //		strMARC		字段中嵌套的MARC记录。其实，这本是一个MARC字段，其中用$1隔开各个嵌套字段。例如UNIMARC的410字段就是这样。
        //		strFieldName	字段名。如果本参数==null，表示想获取任意字段中的第nIndex个
        //		nIndex		同名字段中的第几个。从0开始计算(任意字段中，序号0个不表示头标区了，因为MARC嵌套记录中无法定义头标区)
        //		strField	[out]输出字段。包括字段名、必要的字段指示符、字段内容。不包含字段结束符。
        //					注意头标区当作一个字段返回，此时strField中不包含字段名，其中一开始就是头标区内容
        //		strNextFieldName	[out]顺便返回所找到的字段其下一个字段的名字
        // return:
        //		-1	出错
        //		0	所指定的字段没有找到
        //		1	找到。找到的字段返回在strField参数中
        public static int GetNestedField(string strMARC,
            string strFieldName,
            int nIndex,
            out string strField,
            out string strNextFieldName)
        {
            // LPTSTR p;
            int nFoundCount = 0;
            int nChars = 0;
            string strFldName = "";

            strField = "";
            strNextFieldName = "";

            if (strMARC == null)
            {
                Debug.Assert(false, "strMARC参数不能为null");
                return -1;
            }

            if (strMARC.Length < 5)
                return -1;

            if (strFieldName != null)
            {
                if (strFieldName.Length != 3)
                {
                    Debug.Assert(false, "字段名长度必须为3");	// 字段名必须为3字符
                    return -1;
                }
            }
            else
            {
                // 表示不关心字段名，依靠nIndex来定位字段
            }

            strField = "";

            // 循环，找特定的字段
            //p = (LPTSTR)pszMARC + 5;
            int nStart = 5;

            // 找到第一个'$1'符号
            for (; nStart < strMARC.Length;)
            // *p&&*p!=FLDEND;) 
            {
                if (strMARC[nStart] == FLDEND)
                    break;
                if (strMARC[nStart] == SUBFLD
                    && strMARC[nStart + 1] == '1')
                    goto FOUND;
                nStart++;
            }
            return 0;

        FOUND:

            for (int i = nStart; i < strMARC.Length;)
            {
                if (strMARC[i] == FLDEND)
                    break;

                nChars = DetectNestedFldLens(strMARC, i);
                if (nChars < 2 + 3 + 1)
                {
                    strFldName = "???";
                    goto SKIP;
                }
                Debug.Assert(nChars >= 2 + 3 + 1, "error");
                strFldName = strMARC.Substring(i + 2, 3);

                if (strFieldName == null
                    || (strFieldName != null
                    && strFldName == strFieldName))
                {
                    if (nIndex == nFoundCount)
                    {
                        strField = strMARC.Substring(i + 2, nChars - 2);

                        if (i + nChars < strMARC.Length
                            && strMARC[i + nChars] != RECEND
                            && DetectFldLens(strMARC, i + nChars) >= 2 + 3)
                            strNextFieldName = strMARC.Substring(i + nChars + 2, 3);
                        else
                            strNextFieldName = "";

                        return 1;	// found
                    }
                    nFoundCount++;
                }

            SKIP:
                i += nChars;
            }
            return 0;	// not found
        }

        // 嵌套字段：根据字段开始位置探测整个字段长度
        // 返回字段的长度(字符数，不是byte数)
        static int DetectNestedFldLens(string strMARC, int nStart)
        {
            Debug.Assert(strMARC != null, "strMARC参数不能为null");

            if (nStart >= strMARC.Length)
            {
                Debug.Assert(false, "nStart参数值超出strMARC内容长度范围");
                return 0;
            }

            if (strMARC[nStart] != SUBFLD || strMARC[nStart + 1] != '1')
            {
                Debug.Assert(false, "必须用$1开头的位置调用本函数");
            }

            int i = nStart + 1;
            for (; i < strMARC.Length; i++)
            {
                if (strMARC[i] == SUBFLD
                    && strMARC[i + 1] == '1')
                    break;
            }

            return i - nStart;
        }


        // 从字段中得到子字段组
        // parameters:
        //		strField	字段。其中包括字段名、必要的指示符，字段内容。也就是调用GetField()函数所得到的字段。
        //		nIndex	子字段组序号。从0开始计数。
        //		strGroup	[out]得到的子字段组。其中包括若干子字段内容。
        // return:
        //		-1	出错
        //		0	所指定的子字段组没有找到
        //		1	找到。找到的子字段组返回在strGroup参数中
        public static int GetGroup(string strField,
            int nIndex,
            out string strGroup)
        {
            Debug.Assert(strField != null, "strField参数不能为null");

            Debug.Assert(nIndex >= 0, "nIndex参数必须>=0");

            strGroup = "";

            int nLen = strField.Length;
            if (nLen <= 5)
            {
                return 0;
            }

            // LPCTSTR lpStart,lpStartSave;
            // LPTSTR pp;
            //int l;
            string strZzd = "a";

            // char zzd[3];

            /*
            zzd[0]=SUBFLD;
            zzd[1]='a';
            zzd[2]=0;
            */
            strZzd = strZzd.Insert(0, new String(SUBFLD, 1));

            // lpStart = pszField;

            // 找到起点
            int nStart = 5;
            int nPos;
            for (int i = 0; ; i++)
            {
                nPos = strField.IndexOf(strZzd, nStart);
                if (nPos == -1)
                    return 0;

                /*
                pp = _tcsstr(lpStart,zzd);
                if (pp==NULL) 
                {
                    return 0; // not found
                }
                */

                if (i >= nIndex)
                {
                    nStart = nPos;
                    break;
                }
                nStart = nPos + 1;
                // lpStart = pp + 1;
            }

            //lpStart = pp;
            //lpStartSave = pp;
            //lpStart ++;
            int nStartSave = nStart;
            nStart++;

            nPos = strField.IndexOf(strZzd, nStart);
            if (nPos == -1)
            {
                // 后方没有子字段了
                strGroup = strField.Substring(nStartSave);
                return 1;
            }
            else
            {
                strGroup = strField.Substring(nStartSave, nPos - nStartSave);
                return 1;
            }
            /*
            pp = _tcsstr(lpStart,zzd);
            if (pp == NULL) 
            {	// 后方没有子字段了
                l = _tcslen(lpStartSave);
                MemCpyToString(lpStartSave, l, strGroup);
                return strGroup.GetLength();
            }
            else 
            {
                l = pp - lpStartSave;	// 注意，是字符数
                MemCpyToString(lpStartSave, l, strGroup);
                return strGroup.GetLength();
            }
            */

            //	ASSERT(0);
            //   return 0;
        }

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
        public static int GetSubfield(string strText,
            ItemType textType,
            string strSubfieldName,
            int nIndex,
            out string strSubfield,
            out string strNextSubfieldName)
        {

            Debug.Assert(textType == ItemType.Field
                || textType == ItemType.Group,
                "textType只能为 ItemType.Field/ItemType.Group之一");
            // LPCTSTR p;
            // int nTextLen;
            int nFoundCount = 0;
            int i;

            strSubfield = "";
            strNextSubfieldName = "";

            Debug.Assert(strText != null, "strText参数不能为null");
            //nFieldLen = strText.Length;

            if (textType == ItemType.Field)
            {
#if NO
                if (strText.Length < 3)
                    return -1;	// 字段内容长度不足3字符

                if (strText.Length == 3)
                    return 0;	// 不存在任何子字段内容
#endif
                if (strText.Length < 3)
                    return -1;	// 字段内容长度不足3字符

                if (strText.Length <= 5)
                    return 0;	// 不存在任何子字段内容

                // 2016/10/16
                strText = strText.Substring(5);
            }
            if (textType == ItemType.Group)
            {
                if (strText.Length < 2)
                    return -1;	// 字段内容长度不足3字符

                if (strText.Length == 1)
                    return 0;	// 不存在任何子字段内容
            }

            if (strSubfieldName != null)
            {
                if (strSubfieldName.Length != 1)
                {
                    Debug.Assert(false, "strSubfieldName参数的值若不是null，则必须为1字符");
                    return -1;
                }
            }

            // p = pszField + 3;
            // 找到第一个子字段符号
            for (i = 0; i < strText.Length; i++)
            {
                if (strText[i] == SUBFLD)
                    goto FOUND;
            }
            return 0;

        FOUND:
            // 匹配
            for (; i < strText.Length; i++)
            {
                if (strText[i] == SUBFLD)
                {
                    if (i + 1 >= strText.Length)
                        return 0;	// not found

                    if (strSubfieldName == null
                        ||
                        (strSubfieldName != null
                        && strText[i + 1] == strSubfieldName[0])
                        )
                    {
                        if (nFoundCount == nIndex)
                        {
                            int nChars = DetectSubFldLens(strText, i);
                            strSubfield = strText.Substring(i + 1, nChars - 1);

                            // 取下一个子字段名
                            if (i + nChars < strText.Length
                                && strText[i + nChars] == SUBFLD
                                && DetectFldLens(strText, i + nChars) >= 2)
                            {
                                strNextSubfieldName = strText.Substring(i + nChars + 1, 1);
                            }
                            else
                                strNextSubfieldName = "";

                            return 1;
                        }

                        nFoundCount++;
                    }

                }

            }

            return 0;
        }

        // 根据偏移位置获得所在的子字段名
        public static char SubfieldNameByOffs(string strText,
            int nPos)
        {
            int i = 0;
            // 找到第一个子字段符号
            for (i = 0; i < strText.Length; i++)
            {
                if (strText[i] == SUBFLD)
                    goto FOUND;
            }
            return (char)0;    // not found

        FOUND:

            if (nPos < i)
                return (char)0;    // not found 在第一个子字段出现以前的位置

            // 匹配
            for (; i < strText.Length; i++)
            {
                if (strText[i] == SUBFLD)
                {
                    if (i + 1 >= strText.Length)
                        return (char)0;	// not found

                    int nChars = DetectSubFldLens(strText, i);

                    // strSubfield = strText.Substring(i + 1, nChars - 1);

                    if (nPos >= i && nPos <= i + nChars)
                        return strText[i + 1];
                }

            }

            return (char)0;
        }


        // 替换记录中的字段内容。
        // 先在记录中找同名字段(第nIndex个)，如果找到，则替换；如果没有找到，
        // 则在顺序位置插入一个新字段。
        // parameters:
        //		strMARC		[in][out]MARC记录。
        //		strFieldName	要替换的字段的名。如果为null或者""，则表示所有字段中序号为nIndex中的那个被替换
        //		nIndex		要替换的字段的所在序号。如果为-1，将始终为在记录中追加新字段内容。
        //		strField	要替换成的新字段内容。包括字段名、必要的字段指示符、字段内容。这意味着，不但可以替换一个字段的内容，也可以替换它的字段名和指示符部分。
        // return:
        //		-1	出错
        //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
        //		1	找到了指定的字段，并且也成功用strField替换掉了。
        public static int ReplaceField(
            ref string strMARC,
            string strFieldName,
            int nIndex,
            string strField)
        {
            int nInsertOffs = 24;
            int nStartOffs = 24;
            int nLen = 0;
            int nChars = 0;
            string strFldName;
            int nFoundCount = 0;
            bool bFound = false;

            if (strMARC.Length < 24)
                return -1;

            // Debug.Assert(strFieldName != null, "");

            /*
            if (strFieldName.Length != 3) 
            {
                Debug.Assert(false, "strFieldName参数内容必须为3字符");
                return -1;
            }
            */
            if (strFieldName != null)
            {
                if (strFieldName.Length != 3)
                {
                    Debug.Assert(false, "字段名长度必须为3");	// 字段名必须为3字符
                    return -1;
                }
            }
            else
            {
                // 表示不关心字段名，依靠nIndex来定位字段
            }

            bool bIsHeader = false;

            if (strFieldName == null || strFieldName == "")
            {
                if (nIndex == 0)
                    bIsHeader = true;
            }
            else
            {
                if (strFieldName == "hdr")
                    bIsHeader = true;
            }

            // 检查strField参数正确性
            if (bIsHeader == true)
            {
                if (strField == null
                    || strField == "")
                {
                    Debug.Assert(false, "头标区内容只能替换，不能删除");
                    return -1;
                }

                if (strField.Length != 24)
                {
                    Debug.Assert(false, "strField中用来替换头标区的内容，必须为24字符");
                    return -1;
                }
            }


            // 看看给出的字段内容最后是否有字段结束符，如果没有，则追加一个。
            // 头标区内容不作此处理
            if (strField != null && strField.Length > 0
                && bIsHeader == false)
            {
                if (strField[strField.Length - 1] != FLDEND)
                    strField += FLDEND;
            }

            bool bInsertOffsOK = false;

            // 循环，找特定的字段
            //p = (LPTSTR)(LPCTSTR)strMARC;
            for (int i = 0; i < strMARC.Length;)
            {
                if (strMARC[i] == RECEND)
                    break;


                if (i == 0)
                {
                    if ((nIndex == 0 && strFieldName == null)	// 头标区
                        ||
                        (strFieldName != null
                        && String.Compare("hdr", strFieldName, true) == 0) // 有字段名要求，并且要求头标区
                        )
                    {
                        if (String.IsNullOrEmpty(strField) == true)
                        {
                            Debug.Assert(false, "头标区内容只能替换，不能删除");
                            return -1;
                        }

                        if (strField.Length != 24)
                        {
                            Debug.Assert(false, "strField中用来替换头标区的内容，必须为24字符");
                            return -1;
                        }

                        strMARC = strMARC.Remove(0, 24);	// 删除原来内容

                        strMARC = strMARC.Insert(0, strField);	// 插入新的内容

                        return 1;	// found
                    }

                    nChars = 24;
                    strFldName = "hdr";

                    if (strFieldName == null
                        || (strFieldName != null
                        && "hdr" == strFieldName)
                        )
                    {
                        // 2012/7/28
                        if (nIndex != nFoundCount)
                        {
                            nFoundCount++;
                            goto CONTINUE;
                        }

                        nFoundCount++;
                    }
                }
                else
                {
                    nChars = DetectFldLens(strMARC, i);
                    if (nChars < 3 + 1)
                    {
                        strFldName = "???";
                        goto SKIP;
                    }
                    Debug.Assert(nChars >= 3 + 1, "探测到字段长度不能小于3+1");
                    strFldName = strMARC.Substring(i, 3);
                    // MemCpyToString(p, 3, strFldName);
                }

            SKIP:

                if (strFieldName == null
                    || (strFieldName != null
                    && strFldName == strFieldName)
                    )
                {
                    if (nIndex == nFoundCount)
                    {
                        nStartOffs = i;
                        nLen = nChars;
                        bFound = true;
                        goto FOUND;
                    }
                    nFoundCount++;
                }

                // 想办法留存将来要用的插入位置
                if (strFieldName != null && strFieldName != ""
                    && strFldName != "hdr"
                    && bInsertOffsOK == false)
                {
                    if (String.Compare(strFldName, strFieldName, false) > 0)
                    {
                        nInsertOffs = Math.Max(i, 24);
                        bInsertOffsOK = true;	// 以后不再设置
                    }
                    else
                    {
                        nInsertOffs = Math.Max(i + nChars, 24);
                    }
                }

            CONTINUE:
                i += nChars;
            }

            nStartOffs = nInsertOffs;
            nLen = 0;

            if (String.IsNullOrEmpty(strField) == true)	// 实际为删除要求
                return 0;

            FOUND:
            if (nLen > 0)
                strMARC = strMARC.Remove(nStartOffs, nLen);	// 删除原来内容

            if (String.IsNullOrEmpty(strField) == false)    // 2008/11/10
                strMARC = strMARC.Insert(nStartOffs, strField);	// 插入新的内容

            if (bFound == true)
                return 1;

            return 0;
        }

        // BUG: 如果初始的strField为“100”，当加入子字段a后，strField中忘记加入字段指示符
        // 替换字段中的子字段。
        // parameters:
        //		strField	[in,out]待替换的字段
        //		strSubfieldName	要替换的子字段的名，内容为1字符。如果==null，表示任意子字段
        //					形式为'a'这样的。
        //		nIndex		要替换的子字段所在序号。如果为-1，将始终为在字段中追加新子字段内容。
        //		strSubfield	要替换成的新子字段。注意，其中第一字符为子字段名，后面为子字段内容
        // return:
        //		-1	出错
        //		0	指定的子字段没有找到，因此将strSubfieldzhogn的内容插入到适当地方了。
        //		1	找到了指定的字段，并且也成功用strSubfield内容替换掉了。
        public static int ReplaceSubfield(
            ref string strField,
            string strSubfieldName,
            int nIndex,
            string strSubfield)
        {
            if (strField.Length <= 1)
                return -1;

            if (strSubfieldName != null)
            {
                if (strSubfieldName.Length != 1)
                {
                    Debug.Assert(false, "strSubfieldName参数的值若不是null，则必须为1字符");
                    return -1;
                }
            }

            if (nIndex < 0)
                goto APPEND;	// 追加新子字段

            int i = 0;
            int nFoundCount = 0;

            // 找到第一个子字段符号
            for (i = 0; i < strField.Length; i++)
            {
                if (strField[i] == SUBFLD)
                    goto FOUNDHEAD;
            }
            goto APPEND;
        FOUNDHEAD:
            // 匹配
            for (; i < strField.Length; i++)
            {
                if (strField[i] == SUBFLD)
                {
                    if (i + 1 >= strField.Length)
                        goto APPEND;	// not found

                    if (strSubfieldName == null
                        ||
                        (strSubfieldName != null
                        && strField[i + 1] == strSubfieldName[0])
                        )
                    {
                        if (nFoundCount == nIndex)
                        {
                            int nChars = DetectSubFldLens(strField, i);

                            // 去除原来的内容
                            strField = strField.Remove(i, nChars);
                            if (strSubfield != null
                                && strSubfield != "")
                            {
                                // 插入新内容
                                strField = strField.Insert(i, new string(SUBFLD, 1) + strSubfield);
                            }
                            return 1;
                        }

                        nFoundCount++;
                    }

                }

            } // end

        APPEND:
            strField += new string(SUBFLD, 1) + strSubfield;

            return 0;	// inserted
        }

        // 2011/1/10
        // 删除一个子字段
        // 其实原来的ReplaceSubfield()也可以当作删除来使用
        // return:
        //      -1  出错
        //      0   没有找到子字段
        //      1   找到并删除
        public static int DeleteSubfield(ref string strField,
            string strSubfieldName,
            int nIndex)
        {
            if (strField.Length <= 3)
                return 0;

            if (strSubfieldName != null)
            {
                if (strSubfieldName.Length != 1)
                {
                    Debug.Assert(false, "strSubfieldName参数的值若不是null，则必须为1字符");
                    return -1;
                }
            }

            if (nIndex < 0)
            {
                Debug.Assert(false, "nIndex应当>=0");
                return -1;
            }

            int nCount = 0;
            int nStart = -1;
            for (int i = 0; i < strField.Length; i++)
            {
                if (strField[i] == SUBFLD)
                {
                    if (strSubfieldName != null)
                    {
                        if (i + 1 >= strField.Length)
                            break;

                        if (strSubfieldName[0] != strField[i + 1])
                            continue;
                    }

                    if (nCount == nIndex)
                    {
                        nStart = i;
                        break;
                    }
                    nCount++;
                }
            }

            if (nStart == -1)
                return 0;

            int nLength = DetectSubFldLens(strField, nStart);

            Debug.Assert(nStart + nLength <= strField.Length, "");

            // 删除
            strField = strField.Remove(nStart, nLength);
            return 1;
        }

        // 2011/1/10
        // 插入子字段
        // parameters:
        //		strField	[in,out]待替换的字段
        //		strSubfieldName	要替换的子字段的名，内容为1字符。形式为'a'这样的。如果==null，表示任意子字段
        //		nIndex		要替换的子字段所在序号。如果为-1，将导致总是追加在字段末尾
        //      nDirection  -1 在命中的子字段前方插入；0 替换所命中的子字段；1 在命中的子字段后方插入
        //      strInsertContent    要插入的内容。第一字符应当为31，后面是子字段名和内容。当然，也允许特殊用法。
        // return:
        //      -1  出错
        //      0   没有找到子字段，直接在字段尾部插入内容
        //      1   找到了子字段，并在其附近进行了插入
        public static int InsertSubfield(ref string strField,
            string strSubfieldName,
            int nIndex,
            string strInsertContent,
            int nDirection)
        {
            if (strField.Length <= 3)
                return 0;

            if (strSubfieldName != null)
            {
                if (strSubfieldName.Length != 1)
                {
                    Debug.Assert(false, "strSubfieldName参数的值若不是null，则必须为1字符");
                    return -1;
                }
            }

            int nCount = 0;
            int nStart = -1;
            for (int i = 0; i < strField.Length; i++)
            {
                if (strField[i] == SUBFLD)
                {
                    if (strSubfieldName != null)
                    {
                        if (i + 1 >= strField.Length)
                            break;

                        if (strSubfieldName[0] != strField[i + 1])
                            continue;
                    }

                    if (nCount == nIndex)
                    {
                        nStart = i;
                        break;
                    }
                    nCount++;
                }
            }

            if (nStart == -1)
            {
                // 没有找到命中位置，则追加
                // TODO: 需要注意判断是否为控制字段，如果不是控制字段，要注意创建必要的字段指示符位置
                strField = strField + strInsertContent;
                return 0;
            }

            int nLength = DetectSubFldLens(strField, nStart);

            Debug.Assert(nStart + nLength <= strField.Length, "");

            // 去除原来的内容
            if (nDirection == 0)
                strField = strField.Remove(nStart, nLength);

            // 插入新内容
            if (nDirection == 0 || nDirection == -1)
            {
                strField = strField.Insert(nStart, strInsertContent);
            }
            else if (nDirection == 1)
            {
                strField = strField.Insert(nStart + nLength, strInsertContent);
            }
            else
            {
                Debug.Assert(false, "");
            }

            return 1;
        }

        // 获得一个特定风格的 MARC 记录
        // parameters:
        //      strStyle    要匹配的style值。如果为null，表示任何$*值都匹配，实际上效果是去除$*并返回全部字段内容
        // return:
        //      0   没有实质性修改
        //      1   有实质性修改

        static public int GetMappedRecord(ref string strMARC,
    string strStyle)
        {
            bool bChanged = false;

            MarcRecord record = new MarcRecord(strMARC);
            MarcRecord result = new MarcRecord(strMARC.PadRight(24, ' ').Substring(0, 24));

            foreach (MarcField field in record.ChildNodes)
            {
                string strContent = field.Content;

                string strBlank = strContent;   // .Trim();
                int nRet = strBlank.IndexOf((char)SUBFLD);
                if (nRet != -1)
                    strBlank = strBlank.Substring(0, nRet); // .Trim();

                string strCmd = StringUtil.GetLeadingCommand(strBlank);
                if (string.IsNullOrEmpty(strStyle) == false
                    && string.IsNullOrEmpty(strCmd) == false
                    && StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    string strRule = strCmd.Substring(3);
                    if (strRule != strStyle
                        && string.IsNullOrEmpty(strStyle) == false)
                    {
                        bChanged = true;
                        continue;
                    }
                }

                MarcField new_field = new MarcField(field.Text);

                MarcNodeList xings = new_field.select("subfield[@name='*']");
                if (xings.count > 0
                    && xings.FirstContent != strStyle && string.IsNullOrEmpty(strStyle) == false)
                {
                    bChanged = true;
                    continue;
                }

                if (xings.count > 0)
                {
                    xings.detach();
                    bChanged = true;
                }

                if (string.IsNullOrEmpty(strCmd) == false)
                {
                    new_field.Content = strContent.Substring(strCmd.Length + 2);
                    bChanged = true;
                }

                if (new_field.Name == "hdr" && string.IsNullOrEmpty(strStyle) == false)
                {
                    result.Header[0, 24] = new_field.Content.PadRight(24, ' ').Substring(0, 24);
                    bChanged = true;
                    continue;
                }

                FilterSubfields(new_field, strStyle);

                result.add(new_field);
            }

            strMARC = result.Text;
            if (bChanged == true)
                return 1;
            return 0;
        }

        // return:
        //      false   没有发生修改
        //      true    发生了修改
        static bool FilterSubfields(MarcField field, string strStyle)
        {
            if (field.IsControlField == true)
                return false;
            bool bChanged = false;
            MarcField result = new MarcField();
            //result.Name = field.Name;
            //result.Indicator = field.Indicator;
            foreach (MarcSubfield subfield in field.Subfields)
            {
                string strCmd = StringUtil.GetLeadingCommand(subfield.Content);
                if (string.IsNullOrEmpty(strStyle) == false
                    && string.IsNullOrEmpty(strCmd) == false)
                {
                    if (StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strRule = strCmd.Substring(3);
                        if (strRule != strStyle
                            && string.IsNullOrEmpty(strStyle) == false)
                        {
                            bChanged = true;
                            continue;
                        }
                    }
                    else
                        strCmd = null;  // 其他 xx: 命令不算
                }

                MarcSubfield new_subfield = new MarcSubfield(subfield.Name, subfield.Content);
                if (string.IsNullOrEmpty(strCmd) == false)
                {
                    new_subfield.Content = subfield.Content.Substring(strCmd.Length + 2);
                    bChanged = true;
                }

                result.add(new_subfield);
            }

            if (bChanged == true)
                field.Content = result.Content;
            return bChanged;
        }

#if NO
        // 获得一个特定风格的 MARC 记录
        // parameters:
        //      strStyle    要匹配的style值。如果为null，表示任何$*值都匹配，实际上效果是去除$*并返回全部字段内容
        // return:
        //      0   没有实质性修改
        //      1   有实质性修改
        static public int GetMappedRecord(ref string strMARC,
            string strStyle)
        {
            int nRet = 0;
            string strName = "*";
            bool bChanged = false;

            // 先处理 $* 子字段
            StringBuilder strResult = new StringBuilder(4096);
            for (int i = 0; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                nRet = GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet == 0)
                    break;

                if (i == 0)
                {
                    strResult.Append(strField);
                    continue;
                }

                if (string.IsNullOrEmpty(strField) == true)
                    continue;
                if (strField.Length < 3)
                    continue;

                {
                    string strFieldName = strField.Substring(0, 3);

                    // 字段名后(字段指示符后)和第一个子字段符号之间的空白片断
                    string strIndicator = "";
                    string strContent = "";
                    if (IsControlFieldName(strField.Substring(0, 3)) == true)
                    {
                        strContent = strField.Substring(3);
                    }
                    else
                    {
                        if (strField.Length >= 5)
                        {
                            strIndicator = strField.Substring(3, 2);
                            strContent = strField.Substring(3 + 2);
                        }
                        else
                            strIndicator = strField.Substring(3, 1);
                    }

                    string strBlank = strContent;   // .Trim();
                    nRet = strBlank.IndexOf((char)SUBFLD);
                    if (nRet != -1)
                        strBlank = strBlank.Substring(0, nRet); // .Trim();

                    string strCmd = StringUtil.GetLeadingCommand(strBlank);
                    if (string.IsNullOrEmpty(strStyle) == false
                        && string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strRule = strCmd.Substring(3);
                        if (strRule != strStyle)
                        {
                            bChanged = true;
                            continue;
                        }
                    }

                    // 后面还是要继续处理，但strField中去掉了 {...} 一段
                    if (string.IsNullOrEmpty(strCmd) == false)
                    {
                        strContent = strContent.Substring(strCmd.Length + 2);
                        strField = strFieldName + strIndicator + strContent;
                        bChanged = true;
                    }
                }

                //
                string strSubfield = "";
                string strNextSubfieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的子字段没有找到
                //		1	找到。找到的子字段返回在strSubfield参数中
                nRet = GetSubfield(strField,
                    ItemType.Field,
                    strName,    // "*",
                    0,
                    out strSubfield,
                    out strNextSubfieldName);
                if (nRet == 1)
                {
                    string strCurStyle = strSubfield.Substring(1);
                    if (string.IsNullOrEmpty(strStyle) == false
                        && strCurStyle != strStyle)
                    {
                        bChanged = true;
                        continue;
                    }

                    // 删除$*子字段
                    ReplaceSubfield(
            ref strField,
            strName,    // "*",
            0,
            null);
                    bChanged = true;
                }

                strResult.Append(strField + new string(FLDEND, 1));
            }

            if (bChanged == true)
            {
                strMARC = strResult.ToString();
            }

            // 删除里面不符合编目规则的子字段
            nRet = RemoveCmdSubfield(ref strMARC,
                strStyle);
            if (nRet == 1)
                bChanged = true;

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 删除里面不符合编目规则的子字段
        static int RemoveCmdSubfield(ref string strMARC,
    string strStyle)
        {
            bool bChanged = false;
            for (int i = 1; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                int nRet = GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet == 0)
                    break;

                // TODO: 没有子字段的字段内容部分，是否可以包含{...} ?
                bool bFieldChanged = false;
                for (int j = 0; ; j++)
                {
                    string strSubfield = "";
                    string strNextSubfieldName = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    nRet = GetSubfield(strField,
                        ItemType.Field,
                        null,
                        j,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (nRet != 1)
                        break;
                    if (strSubfield.Length <= 1)
                        continue;

                    string strSubfieldName = strSubfield.Substring(0, 1);
                    string strContent = strSubfield.Substring(1);

                    string strCmd = StringUtil.GetLeadingCommand(strContent);
                    if (string.IsNullOrEmpty(strCmd) == false
                        && StringUtil.HasHead(strCmd, "cr:") == true)
                    {
                        string strRule = strCmd.Substring(3);
                        if (strRule == strStyle
                            || string.IsNullOrEmpty(strStyle) == true)
                        {
                            // 保留这个子字段
                            // 但要删除前面 {...}部分
                            ReplaceSubfield(
                ref strField,
                null,    // "*",
                j,
                strSubfieldName + strContent.Substring(strCmd.Length + 2));
                            bFieldChanged = true;
                        }
                        else
                        {
                            // 删除这个子字段
                            ReplaceSubfield(
                                ref strField,
                                null,    // "*",
                                j,
                                null);
                            j--;
                            bFieldChanged = true;
                        }
                    }
                }

                if (bFieldChanged == true)
                {
                    ReplaceField(ref strMARC,
                        null,
                        i,
                        strField);
                    bChanged = true;
                }
            }

            if (bChanged == true)
                return 1;

            return 0;
        }
#endif

        // 删除空的字段
        // return:
        //      -1  error
        //      0   没有修改
        //      1   发生了修改
        public static int RemoveEmptyFields(ref string strMARC,
            out string strError)
        {
            strError = "";
            bool bChanged = false;

            for (int i = 1; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                int nRet = GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet == -1)
                {
                    strError = "GetField() error";
                    return -1;
                }
                if (nRet != 1)
                    break;
                if (string.IsNullOrEmpty(strField) == true)
                    continue;
                if (strField.Length < 3)
                    continue;

                string strIndicator = "";
                string strContent = "";

                if (IsControlFieldName(strField.Substring(0, 3)) == true)
                {
                    strContent = strField.Substring(3);
                }
                else
                {
                    if (strField.Length >= 5)
                    {
                        strIndicator = strField.Substring(3, 2);
                        strContent = strField.Substring(3 + 2);
                    }
                    else
                        strIndicator = strField.Substring(3, 1);
                }

                strContent = strContent.Trim();

                if (string.IsNullOrEmpty(strContent) == true)
                {
                    // return:
                    //		-1	出错
                    //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
                    //		1	找到了指定的字段，并且也成功用strField替换掉了。
                    nRet = ReplaceField(
                        ref strMARC,
                        null,
                        i,
                        null);
                    bChanged = true;
                    i--;
                }
            }

            if (bChanged == false)
                return 0;
            return 1;
        }

        // 删除空的子字段
        // return:
        //      -1  error
        //      0   没有修改
        //      1   发生了修改
        public static int RemoveEmptySubfields(ref string strMARC,
            out string strError)
        {
            strError = "";
            bool bChanged = false;

            for (int i = 1; ; i++)
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                int nRet = GetField(strMARC,
                    null,
                    i,
                    out strField,
                    out strNextFieldName);
                if (nRet == -1)
                {
                    strError = "GetField() error";
                    return -1;
                }
                if (nRet != 1)
                    break;
                if (string.IsNullOrEmpty(strField) == true)
                    continue;
                if (strField.Length < 3)
                    continue;

                bool bFieldChanged = false;
                for (int j = 0; ; j++)
                {
                    string strSubfield = "";
                    string strNextSubfieldName = "";
                    // return:
                    //		-1	出错
                    //		0	所指定的子字段没有找到
                    //		1	找到。找到的子字段返回在strSubfield参数中
                    nRet = GetSubfield(strField,
                        ItemType.Field,
                        null,
                        j,
                        out strSubfield,
                        out strNextSubfieldName);
                    if (nRet == -1)
                    {
                        strError = "GetSubfield() error";
                        return -1;
                    }
                    if (nRet != 1)
                        break;

                    // 删除空子字段
                    if (strSubfield.Length == 1)
                    {
                        // return:
                        //      -1  出错
                        //      0   没有找到子字段
                        //      1   找到并删除
                        nRet = DeleteSubfield(ref strField,
                            null,
                            j);
                        if (nRet == -1)
                        {
                            strError = "DeleteSubfield() error";
                            return -1;
                        }
                        bFieldChanged = true;
                        j--;
                    }
                }
#if NO
                string strIndicator = "";

                if (IsControlFieldName(strField.Substring(0, 3)) == true)
                {
                }
                else
                {
                    if (strField.Length >= 5)
                        strIndicator = strField.Substring(3, 2);
                    else
                        strIndicator = strField.Substring(3, 1);
                }

                if (MarcUtil.MatchIndicator(strIndicatorMatch, strIndicator) == true)
                {
                    results.Add(strField);
                }
#endif

                if (bFieldChanged == true)
                {
                    // return:
                    //		-1	出错
                    //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
                    //		1	找到了指定的字段，并且也成功用strField替换掉了。
                    nRet = ReplaceField(
                        ref strMARC,
                        null,
                        i,
                        strField);
                    bChanged = true;
                }
            }

            if (bChanged == false)
                return 0;
            return 1;
        }

        #endregion


        #region 处理MARC记录转换为ISO209任务的静态函数

        // 兼容原来的版本
        public static int ModifyOutputMARC111(
    string strMARC,
    string strMarcSyntax,
    Encoding encoding,
    out string strResult)
        {
            return ModifyOutputMARC(
            strMARC,
            strMarcSyntax,
            encoding,
            "unimarc_100",  // 默认的效果
            out strResult);
        }

        // 2017/4/7 改为用 MarcRecord 处理 100$a
        // 根据MARC格式类型和输出的编码方式要求，修改MARC记录的头标区或100字段。
        // parameters:
        //		strMarcSyntax   "unimarc" "usmarc"
        //      strStyle    修改方式。
        //                  unimarc_100 自动覆盖修改 100 字段内和字符集编码方式有关的几个字符位
        public static int ModifyOutputMARC(
            string strMARC,
            string strMarcSyntax,
            Encoding encoding,
            string strStyle,
            out string strResult)
        {
            strResult = strMARC;

            if (StringUtil.IsInList("unimarc_100", strStyle)
                && String.Compare(strMarcSyntax, "unimarc", true) == 0) // UNIMARC
            {
                /*
                In UNIMARC the information about enconding sets are stored in field 100, 
        position 26-29 & 30-33. The
        code for Unicode is "50" in positions 26-27 and the position 28-33 will 
        contain blanks.
                */
                // 将100字段中28开始的位置按照UTF-8编码特性强行置值。

                MarcRecord record = new MarcRecord(strMARC);
                bool bChanged = false;

                string strValue = record.select("field[@name='100']/subfield[@name='a']").FirstContent;
                if (strValue == null)
                    strValue = "";

                // 确保子字段内容长度为 36 字符。
                int nOldLength = strValue.Length;
                strValue = strValue.PadRight(36, ' ');
                if (strValue.Length != nOldLength)
                    bChanged = true;

                string strPart = strValue.Substring(26, 8);
                // 看看26-29是否已经符合要求
                if (encoding == Encoding.UTF8)
                {
                    if (strPart == "50      ")
                    { // 已经符合要求
                    }
                    else
                    {
                        strValue = strValue.Remove(26, 8);
                        strValue = strValue.Insert(26, "50      ");
                        bChanged = true;
                    }
                }
                else
                {
                    if (strPart == "50      ")
                    { // 需要改变
                        strValue = strValue.Remove(26, 8);
                        strValue = strValue.Insert(26, "0120    ");
                        bChanged = true;
                    }
                    else
                    {	// 不需要改变
                    }
                }

                if (bChanged == true)
                {
                    record.setFirstSubfield("100", "a", strValue, "  ");
                    strResult = record.Text;
                }
            }

            // 修改头标区
            if (String.Compare(strMarcSyntax, "unimarc", true) == 0)
            {
                // UNIMARC
                strResult = StringUtil.SetAt(strResult, 9, ' ');
            }
            else if (true/*nMARCType == 1*/)
            {
                // USMARC。所有非UNIMARC的都仿USMARC处理，因为不必使用100字段
                if (encoding == Encoding.UTF8)
                    strResult = StringUtil.SetAt(strResult, 9, 'a');	// UTF-8(UCS-2也仿此)
                else
                    strResult = StringUtil.SetAt(strResult, 9, ' ');	// # DBCS或者MARC-8 // 2007/8/8 change '#' to ' '
            }

            return 0;
        }

#if NO
        // 根据MARC格式类型和输出的编码方式要求，修改MARC记录的头标区或100字段。
        // parameters:
        //		strMarcSyntax   "unimarc" "usmarc"
        public static int ModifyOutputMARC(
            string strMARC,
            string strMarcSyntax,
            Encoding encoding,
            out string strResult)
        {
            strResult = strMARC;

            if (String.Compare(strMarcSyntax, "unimarc", true) == 0) // UNIMARC
            {
                /*
                In UNIMARC the information about enconding sets are stored in field 100, 
        position 26-29 & 30-33. The
        code for Unicode is "50" in positions 26-27 and the position 28-33 will 
        contain blanks.
                */
                // 将100字段中28开始的位置按照UTF-8编码特性强行置值。
                string strField;
                bool bChanged = false;
                int nRet;
                string strPart;
                string strNextFieldName;

                // 得到原来的100字段内容。
                nRet = GetField(strMARC,
                    "100",
                    0,
                    out strField,
                    out strNextFieldName);
                if (nRet != 1)
                {	// 100字段不存在
                    strField = "100  ";
                }
                // 确保字段长度(包括字段名和字段指示符)为3+2+2+36字符。
                int nOldLength = strField.Length;
                strField = strField.PadRight(3 + 2 + 2 + 36, ' ');
                //nRet = EnsureFieldSize(strField,
                //	3+2+2+36);
                if (strField.Length != nOldLength)
                    bChanged = true;

                strPart = strField.Substring(3 + 2 + 2 + 26, 8);
                // 看看26-29是否已经符合要求
                if (encoding == Encoding.UTF8)
                {
                    //MemCpyToString((LPCTSTR)strField + 3+2+2+26,
                    //	8,
                    //	strPart);
                    if (strPart == "50      ")
                    { // 已经符合要求
                    }
                    else
                    {
                        strField = strField.Remove(3 + 2 + 2 + 26, 8);
                        strField = strField.Insert(3 + 2 + 2 + 26, "50      ");
                        bChanged = true;
                    }
                }
                else
                {
                    if (strPart == "50      ")
                    { // 需要改变
                        strField = strField.Remove(3 + 2 + 2 + 26, 8);
                        strField = strField.Insert(3 + 2 + 2 + 26, "0120    ");
                        bChanged = true;
                    }
                    else
                    {	// 不需要改变
                    }
                }

                if (bChanged == true)
                {
                    ReplaceField(ref strResult,
                        "100",
                        0,
                        strField);
                }

            }

            // 修改头标区
            if (String.Compare(strMarcSyntax, "unimarc", true) == 0)
            { // UNIMARC
                strResult = StringUtil.SetAt(strResult, 9, ' ');
            }
            else if (true/*nMARCType == 1*/)
            { // USMARC。所有非UNIMARC的都仿USMARC处理，因为不必使用100字段
                if (encoding == Encoding.UTF8)
                    strResult = StringUtil.SetAt(strResult, 9, 'a');	// UTF-8(UCS-2也仿此)
                else
                    strResult = StringUtil.SetAt(strResult, 9, ' ');	// # DBCS或者MARC-8 // 2007/8/8 change '#' to ' '
            }

            return 0;
        }
#endif

        // 将机内格式记录构造为ISO2709格式记录。
        // parameters:
        //		baMARC		[in]机内格式记录。已经通过适当Encoding对象转换为ByteArray了
        //		baResult	[out]ISO2709格式记录。
        // return:
        //		-1	error
        //		0	succeed
        public static int BuildISO2709Record(byte[] baMARC,
            out byte[] baResult)
        {
            int nLen;
            byte[] baMuci = null;	// 目次区
            byte[] baBody = null;	// 数据区
            byte[] baFldName = null;
            string strFldLen;
            string strFldStart;
            byte[] baFldContent = null;
            int nStartPos;
            int nFldLen;
            int nFldStart;
            bool bEnd = false;
            int nPos;
            int nRecLen = 0;

            baResult = null;

            if (baMARC == null)
                return -1;
            if (baMARC.Length < 24)
                return -1;

            // 2018/3/8
            if (baMARC[0] == 0
    || baMARC[1] == 0)
            {
                throw new Exception("ISO2709 格式无法使用编码方式 UCS-2 (UTF-16)");
            }

            MarcHeaderStruct header = new MarcHeaderStruct(baMARC);

            /*
            ISO2709ANSIHEADER header;
            memcpy(&header,
                (LPCSTR)advstrMARC,
                sizeof(header));
            */

            nLen = baMARC.Length;

            for (nStartPos = 24, nFldStart = 0; ;)
            {
                nPos = ByteArray.IndexOf(baMARC, (byte)FLDEND, nStartPos);
                // nPos = FindCharInStringA((LPCSTR)advstrMARC, FLDEND, nStartPos);
                if (nPos == -1)
                {
                    nFldLen = nLen - nStartPos;
                    bEnd = true;
                }
                else
                {
                    nFldLen = nPos - nStartPos + 1;
                }
                if (nFldLen < 3)
                {
                    goto SKIP;
                }
                // strFldName = advstrMARC.MidA(nStartPos, 3);
                baFldName = new byte[3];
                Array.Copy(baMARC,
                    nStartPos,
                    baFldName, 0,
                    3);

                // advstrFldContent = advstrMARC.MidA(nStartPos + 3, nFldLen - 3);
                baFldContent = new byte[nFldLen - 3];
                Array.Copy(baMARC,
                    nStartPos + 3,
                    baFldContent, 0,
                    nFldLen - 3);

                //advstrFldLen.Format("%04d", nFldLen - 3);
                strFldLen = Convert.ToString(nFldLen - 3);
                strFldLen = strFldLen.PadLeft(4, '0');

                // advstrFldStart.Format("%05d", nFldStart);
                strFldStart = Convert.ToString(nFldStart);
                strFldStart = strFldStart.PadLeft(5, '0');

                nFldStart += nFldLen - 3;

                // advstrMuci += (LPCSTR)advstrFldName;
                baMuci = ByteArray.Add(baMuci, baFldName);
                // advstrMuci += (LPCSTR)advstrFldLen;
                baMuci = ByteArray.Add(baMuci, Encoding.UTF8.GetBytes(strFldLen));
                // advstrMuci += (LPCSTR)advstrFldStart;
                baMuci = ByteArray.Add(baMuci, Encoding.UTF8.GetBytes(strFldStart));

                baBody = ByteArray.Add(baBody, baFldContent);
            SKIP:
                if (bEnd)
                    break;
                nStartPos = nPos + 1;
            }


            nRecLen = baMuci.Length + 1
                + baBody.Length + 1 + 24;

            /*
            advstrText.Format(
                "%05d",
                nRecLen);

            memcpy(header.reclen,
                (LPCSTR)advstrText,
                advstrText.GetLengthA());
            */
            header.RecLength = nRecLen;


            /*
            advstrText.Format(
                "%05d",
                sizeof(header) + advstrMuci.GetLengthA() + 1);
            memcpy(header.baseaddr,
                (LPCSTR)advstrText,
                advstrText.GetLengthA());
            */
            header.BaseAddress = 24 + baMuci.Length + 1;

            // ForceUNIMARCHeader(&header);

            /*
            In USMARC format, leader postion 09, one character indicate the character coding scheme:

            09 - Character coding scheme
            Identifies the character coding scheme used in the record. 
            # - MARC-8
            a - UCS/Unicode
            (http://lcweb.loc.gov/marc/bibliographic/ecbdldrd.html)
            */



            //baTarget.SetSize(nRecLen);

            /*
            memcpy(baTarget.GetData(), 
                (char *)&header,
                sizeof(header));
            */
            baResult = ByteArray.Add(baResult, header.GetBytes());


            /*
            memcpy((char *)baTarget.GetData() + sizeof(header), 
                (LPCSTR)advstrMuci,
                advstrMuci.GetLengthA());
            */
            baResult = ByteArray.Add(baResult, baMuci);

            /*
            *((char *)baTarget.GetData() + sizeof(header) + advstrMuci.GetLengthA())
                = FLDEND;
            */
            baResult = ByteArray.Add(baResult, (byte)FLDEND);

            /*
            memcpy((char *)baTarget.GetData() + sizeof(header)+ advstrMuci.GetLengthA() + 1, 
                (LPCSTR)advstrBody,
                advstrBody.GetLengthA());
            */
            baResult = ByteArray.Add(baResult, baBody);

            /*
            *((char *)baTarget.GetData() + nRecLen - 1)
                = RECEND;
            */
            baResult = ByteArray.Add(baResult, (byte)RECEND);

            return 0;
        }

        // 探测记录的MARC格式 unimarc / usmarc / dt1000reader
        // return:
        //      0   没有探测出来。strMarcSyntax为空
        //      1   探测出来了
        public static int DetectMarcSyntax(string strMARC,
            out string strMarcSyntax)
        {
            strMarcSyntax = "";

            string strField = "";
            string strNextFieldName = "";
            // 看看是不是有245字段
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            int nRet = GetField(strMARC,
                "245",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == 1)
            {
                strMarcSyntax = "usmarc";
                return 1;
            }

            // 看看是不是有008字段
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = GetField(strMARC,
                "008",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == 1)
            {
                strMarcSyntax = "usmarc";
                return 1;
            }

            /*
                        看头标区的方法不可靠
                        // 看看头标区09位？

            In USMARC format, leader postion 09, one character indicate the character coding scheme:

            09 - Character coding scheme
            Identifies the character coding scheme used in the record. 
            # - MARC-8
            a - UCS/Unicode
            (http://lcweb.loc.gov/marc/bibliographic/ecbdldrd.html)
                        if (strMARC.Length >= 24 
                            && (strMARC[9] == ' ' || strMARC[9] == 'a' ))
                        {
                            strMarcSyntax = "usmarc";
                            return 1;
                        }
            */

            // 看看是不是有200字段
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = GetField(strMARC,
                "200",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == 1)
            {
#if NO
                // 看100$a内容长度，决定是不是读者格式?
                string str100_a = GetFirstSubfield(strMARC,
                    "100",
                    "a");
                if (str100_a.Length >= 36)
                {
                    strMarcSyntax = "unimarc";
                    return 1;
                }
#endif
                // 看801$a内容长度，决定是不是读者格式?
                string str801_a = GetFirstSubfield(strMARC,
                    "801",
                    "a");
                if (str801_a.Length > 0)
                {
                    strMarcSyntax = "unimarc";
                    return 1;
                }
                else
                {
                    strMarcSyntax = "dt1000reader";
                    return 1;
                }
            }

            /*
            // 看看是不是有905字段
            // return:
            //		-1	出错
            //		0	所指定的字段没有找到
            //		1	找到。找到的字段返回在strField参数中
            nRet = GetField(strMARC,
                "905",
                0,
                out strField,
                out strNextFieldName);
            if (nRet == 1)
            {
                strMarcSyntax = "unimarc";
                return 1;
            }
             * */

            return 0;
        }

        // 将一个太长的行切割为多行
        static List<string> BreakLine(string strText, int nWrapCol)
        {
            int max = nWrapCol;
            List<string> lines = new List<string>();
            while (strText.Length > 0)
            {
                int nCutLength = Math.Min(max, strText.Length);
                string strLine = strText.Substring(0, nCutLength);

                strLine = ReplaceTailBlank(strLine);

                if (max == nWrapCol)
                    lines.Add(strLine);
                else
                    lines.Add("     " + strLine);

                strText = strText.Substring(nCutLength);
                max = nWrapCol - 5;
            }

            /*
            if (strText.Length > 0)
            {
                if (max == nWrapCol)
                    lines.Add(strText);
                else
                    lines.Add("     " + strText);
            }
             * */

            return lines;
        }

        static string ReplaceTailBlank(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;

            int nCount = 0;
            for (int i = strText.Length - 1; i >= 0; i--)
            {
                char ch = strText[i];
                if (ch == ' ')
                    nCount++;
                else
                    break;
            }

            if (nCount == 0)
                return strText;

            return strText.Substring(0, strText.Length - nCount) + new string('_', nCount);
        }

        // 将机内格式变换为工作单格式
        // return:
        //      -1  出错
        //      0   成功
        public static int CvtJineiToWorksheet(
            string strSourceMARC,
            int nWrapCol,
            out List<string> lines,
            out string strError)
        {
            strError = "";
            lines = new List<string>();

            if (strSourceMARC.Length < 24)
            {
                strError = "源记录长度不足24字符";
                return -1;
            }

            if (nWrapCol < 0 && nWrapCol != -1)
            {
                strError = "nWarpCol参数值必须大于等于6，或者为-1";
                return -1;
            }

            if (nWrapCol >= 0 && nWrapCol < 6)
            {
                strError = "nWarpCol参数值必须大于等于6，或者为-1";
                return -1;
            }

            lines.Add(ReplaceTailBlank(strSourceMARC.Substring(0, 24)));
            strSourceMARC = strSourceMARC.Substring(24).Replace(new string(RECEND, 1), "").Replace(new string(SUBFLD, 1), "ǂ");
            string[] fields = strSourceMARC.Split(new char[] { (char)FLDEND });
            foreach (string field in fields)
            {
                if (string.IsNullOrEmpty(field) == true)
                    continue;
                if (nWrapCol != -1)
                    lines.AddRange(BreakLine(field, nWrapCol));
                else
                    lines.Add(ReplaceTailBlank(field));
            }

            lines.Add("***");
            return 0;
        }

        // 兼容原来的版本
        public static int CvtJineiToISO2709(
    string strSourceMARC,
    string strMarcSyntax,
    Encoding targetEncoding,
    out byte[] baResult,
    out string strError)
        {
            return CvtJineiToISO2709(
    strSourceMARC,
    strMarcSyntax,
    targetEncoding,
    "unimarc_100",
    out baResult,
    out strError);
        }

        // 将MARC机内格式转换为ISO2709格式
        // parameters:
        //      strSourceMARC   [in]机内格式MARC记录。
        //      strMarcSyntax   [in]为"unimarc"或"usmarc"
        //      targetEncoding  [in]输出ISO2709的编码方式。为UTF8、codepage-936等等
        //      baResult    [out]输出的ISO2709记录。编码方式受targetEncoding参数控制。注意，缓冲区末尾不包含0字符。
        // return:
        //      -1  出错
        //      0   成功
        public static int CvtJineiToISO2709(
            string strSourceMARC,
            string strMarcSyntax,
            Encoding targetEncoding,
            string strStyle,
            out byte[] baResult,
            out string strError)
        {
            baResult = null;

            if (strSourceMARC.Length < 24)
            {
                strError = "机内格式记录长度小于24字符";
                return -1;
            }

            // 2013/11/23
            // 疑问：MARC 机内格式字符串最后一个字符到底允许不允许为 MARC 结束符?
            // 替换记录中可能出现的 MARC 结束符。这个字符会破坏 ISO2709 文件的记录分段
            if (strSourceMARC.IndexOf(RECEND) != -1)
            {
#if NO
                StringBuilder text = new StringBuilder(4096);
                foreach (char ch in strSourceMARC)
                {
                    if (ch != RECEND)
                        text.Append(ch);
                    else
                        text.Append('*');
                }
                strSourceMARC = text.ToString();
#endif
                strSourceMARC = strSourceMARC.Replace(RECEND, '*');
            }

            ModifyOutputMARC(strSourceMARC,
                strMarcSyntax,
                targetEncoding,
                strStyle,
                out string strMARC);

            // 先转换字符集
            byte[] baMARC = targetEncoding.GetBytes(strMARC);

            BuildISO2709Record(baMARC,
                out baResult);

            strError = "";
            return 0;
        }

        #endregion

        #region 工作单文件、记录相关函数

        // 读出工作单文件的第一个记录的文本内容。注意，文本内容没有进行任何转换，还是工作单格式
        public static string ReaderFirstWorksheetRecord(string strFileName,
            string strEncodingName)
        {
            try
            {
                Encoding encoding = GetEncoding(strEncodingName);
                if (encoding == null)
                    encoding = Encoding.GetEncoding(936);
                StringBuilder text = new StringBuilder();
                using (TextReader reader = new StreamReader(strFileName, encoding))
                {
                    for (int i = 0; ; i++)
                    {
                        string line = reader.ReadLine();
                        if (line == null || i >= 100)
                            break;
                        text.Append(line + "\r\n");
                        if (line == "***")
                            break;
                    }
                }

                return text.ToString();
            }
            catch (FileNotFoundException)
            {
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static Encoding GetEncoding(string strEncodingName)
        {
            if (string.IsNullOrEmpty(strEncodingName) == true)
                return null;

            if (StringUtil.IsNumber(strEncodingName) == true)
                return Encoding.GetEncoding(Convert.ToInt32(strEncodingName));

            return Encoding.GetEncoding(strEncodingName);
        }

        // 2015/8/10
        // 从 MARC 工作单文件中顺次读一条 MARC 记录
        // return:
        //	-2	MARC格式错
        //	-1	出错
        //	0	正确
        //	1	结束(当前返回的记录有效)
        //	2	结束(当前返回的记录无效)
        public static int ReadWorksheetRecord(TextReader s,
            out string strMARC,
            out string strError)
        {
            strMARC = "";
            strError = "";

            int nMaxLines = 1000;

            List<string> lines = new List<string>();
            try
            {
                // TODO: 如果始终没有 *** 行怎么办？要防止内存溢出
                for (int i = 0; ; i++)
                {
                    if (i > nMaxLines)
                    {
                        strError = "一个工作单记录内文本行数超过了 " + nMaxLines.ToString() + " 行，可能您操作的不是工作单格式的文件 ...";
                        return -1;
                    }

                    string strLine = s.ReadLine();
                    if (strLine == null)
                    {
                        if (lines.Count == 0)
                            return 2;
                        break;
                    }

                    if (strLine == "***")
                        break;

#if NO
                    // 2018/11/19
                    {
                        // 遇到全是空格的行，也当作记录结束
                        if (string.IsNullOrEmpty(strLine.Trim())
                            && i > 0)
                            break;

                        if (string.IsNullOrEmpty(strLine.Trim()) && lines.Count == 0)
                            continue;
                    }
#endif

                    if (IsContinueLine(ref strLine) == false)
                    {
                        if (string.IsNullOrEmpty(strLine) == true)
                            continue;
                        lines.Add(strLine);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (lines.Count == 0)
                            lines.Add(strLine);
                        else
                            lines[lines.Count - 1] = lines[lines.Count - 1] + strLine;
                    }

                }
            }
            catch (Exception ex)
            {
                strError = "读入文件时发生异常: " + ex.Message;
                return -1;
            }

            StringBuilder text = new StringBuilder();
            {
                int i = 0;
                foreach (string line in lines)
                {
                    // 一行右侧连续的 '_' 字符要替换为空格
                    string strLine = ConvertTailBlanks(line);

                    if (i == 0)
                    {
                        // 确保 24 个字符
                        if (strLine.Length < 24)
                            strLine = strLine.PadRight(24, ' ');
                        else if (strLine.Length > 24)
                            strLine = strLine.Substring(0, 24);
                        text.Append(strLine);
                        i++;
                        continue;
                    }
                    // 注：符号 'ǂ' 是 MARC 编辑器的“复制工作单到剪贴板”功能所使用的子字段符号
                    text.Append(strLine.Replace('@', (char)31).Replace('ǂ', (char)31) + new string((char)30, 1));
                }
            }

            strMARC = text.ToString();
            return 0;
        }

        // 2020/7/9
        // 把一行末尾的连续 _ 字符替换为空格
        public static string ConvertTailBlanks(string line)
        {
            if (string.IsNullOrEmpty(line))
                return line;
            int length = line.Length;
            string temp = line.TrimEnd(new char[] { '_' });
            if (temp.Length == length)
                return line;
            return temp.PadRight(length, ' ');
        }

        // 是否为续行？
        // 续行的特征是前 5 个字符为空格
        // 如果为续行，则函数返回前会进行处理，去掉前 5 个字符
        static bool IsContinueLine(ref string strLine)
        {
            if (string.IsNullOrEmpty(strLine) == true)
                return false;
            if (strLine.Length < 5)
            {
                // TODO: 补齐 5 字符?
                return false;
            }

            if (strLine.Substring(0, 5) == "     ")
            {
                strLine = strLine.Substring(5);
                return true;
            }

            return false;
        }

        #endregion
    }


#if NO
	/// <summary>
	/// byte数组，可以动态扩展
	/// </summary>
	public class TempByteArray : ArrayList
	{
		public void AddRange(byte[] baSource)
		{
			for(int i=0;i<baSource.Length;i++) 
			{
				this.Add(baSource[i]);
			}
		}

		public int AddRange(byte[] baSource,
			int nStart,
			int nLength)
		{
			int nCount = 0;
			for(int i=nStart;i<baSource.Length && i<nStart+nLength;i++) 
			{
				this.Add(baSource[i]);
				nCount ++;
			}

			return nCount;
		}
			
		public byte[] GetByteArray()
		{
			byte[] result = new byte[this.Count];

			for(int i=0;i<this.Count;i++) 
			{
				result[i] = (byte)this[i];
			}

			return result;
		}
	}

#endif
    /// <summary>
    /// byte数组，可以动态扩展
    /// </summary>
    public class MyByteList : List<byte>
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
            byte[] result = new byte[this.Count];
            base.CopyTo(result);
            return result;
        }
    }

}

