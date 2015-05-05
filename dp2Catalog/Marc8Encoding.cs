using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;

namespace dp2Catalog
{
    public class Marc8Encoding : Encoding
    {
        // http://www.loc.gov/marc/specifications/speccharmarc8.html
        // ASCII graphics are the default G0 set and ANSEL graphics are the default G1 set for MARC 21 records
        public int DefaultG0CodePage = 0x42;
        public int DefaultG1CodePage = 0x45;

        public int DefaultMultibyte = 0;
        // public int DefaultG = 0;

        public CharsetTable EaccCharsetTable = null;

        public List<CodePage> CodePages = null;


        public void BuildASCIiTables(string strXmlFileName)
        {
            XmlDocument dom = new XmlDocument();

            dom.Load(strXmlFileName);

            // <characterSet name="Basic Arabic" ISOcode="33">
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//characterSet");

            this.CodePages = new List<CodePage>();

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                CodePage page = new CodePage();
                page.Name = DomUtil.GetAttr(node, "name");
                page.Code = System.Convert.ToInt32(DomUtil.GetAttr(node, "ISOcode"), 16);

                LoadOneTable(node, ref page);

                this.CodePages.Add(page);
            }

        }

        void LoadOneTable(
            XmlNode root,
            ref CodePage page)
        {
            Debug.Assert(page != null, "");
            // <characterSet name="Basic Arabic" ISOcode="33">
            XmlNodeList nodes = root.SelectNodes("code");
            /*
             * <code>
				<marc>21</marc>
				<ucs>0021</ucs>
				<utf-8>21</utf-8>
				<name>EXCLAMATION MARK</name>
		    	</code>
             * */

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                CodeEntry entry = new CodeEntry();
                string strASCII = DomUtil.GetElementText(node, "marc");
                string strUnicode = DomUtil.GetElementText(node, "ucs");

                if (String.IsNullOrEmpty(strASCII) == true
                    || String.IsNullOrEmpty(strUnicode) == true)
                    continue;

                entry.ASCII = System.Convert.ToInt32(strASCII, 16);
                entry.Unicode = System.Convert.ToInt32(strUnicode, 16);
                page.Add(entry);
            }
        }

        // 构造函数
        public Marc8Encoding(CharsetTable eacc_charsettable,
            string strAsciiXmlFileName)
        {
            this.EaccCharsetTable = eacc_charsettable;
            this.EaccCharsetTable.ReadOnly = true;
            BuildASCIiTables(strAsciiXmlFileName);
        }

        CodePage FindPage(int iCode)
        {
            for (int i = 0; i < this.CodePages.Count; i++)
            {
                CodePage page = this.CodePages[i];
                if (page.Code == iCode)
                    return page;
            }

            return null;
        }

        int AsciiToUnicode(int iCodeG0,
            int iCodeG1,
            string strSource,
            out string strTarget,
            out string strError)
        {
            strTarget = "";
            strError = "";

            CodePage pageG0 = null;
            CodePage pageG1 = null;

            pageG0 = FindPage(iCodeG0);
            if (pageG0 == null)
            {
                strError = "不能识别的G0代码页 0x" + System.Convert.ToString(iCodeG0, 16);
                return -1;
            }

            pageG1 = FindPage(iCodeG1);
            if (pageG1 == null)
            {
                strError = "不能识别的G1代码页 0x" + System.Convert.ToString(iCodeG1, 16);
                return -1;
            }

            for (int i = 0; i < strSource.Length; i++)
            {
                byte b = (byte)strSource[i];
                int u = 0;

                if (b >= 0xa1 && b <= 0xfe)
                {
                    // 应用G1代码页
                    // b -= 0x80;
                    u = pageG1.GetUnicode(b);
                    strTarget += (char)u;

                }
                else if (b >= 0x21 && b <= 0x7e)
                {
                    // 应用G0代码页
                    u = pageG0.GetUnicode(b);
                    strTarget += (char)u;
                }
                else
                {
                    strTarget += strSource[i];
                    continue;
                }
            }

            return 0;
        }

        public int SetDefaultCodePage(string strField066Value)
        {
            // 注意每个子字段结束，都应复位一次状态？

            this.DefaultG0CodePage = 0x42;
            this.DefaultG1CodePage = 0x45;
            this.DefaultMultibyte = 0;


            return 0;

#if NOOOOOOOOO

            if (String.IsNullOrEmpty(strField066Value) == true)
            {
                return 0;
            }

            string str066a = "";
            string str066b = "";
            string str066c = "";

            // 得到$a $b $c
            string[] subs = strField066Value.Split(new char[] {(char)MarcUtil.SUBFLD });

            for (int i = 0; i < subs.Length; i++)
            {
                string strText = subs[i].TrimEnd();
                if (String.IsNullOrEmpty(strText) == true)
                    continue;
                if (strText[0] == 'a')
                    str066a = strText.Substring(1);
                if (strText[0] == 'b')
                    str066b = strText.Substring(1);
                if (strText[0] == 'c')
                    str066c = strText.Substring(1);

            }

            /*
             (3 for basic Arabic 
(4 for extended Arabic 
Beng for Bengali 
$1 for CJK 
(N for basic Cyrillic 
(Q for extended Cyrillic 
Deva for Devanagari 
(S for Greek 
(2 for Hebrew 
Taml for Tamil 
Thai for Thai
* 
             * */
            if (str066c == "(3")
            {
                this.DefaultCodePage = 0x33;
                this.DefaultMultibyte = 0;
                this.DefaultG = 1;
            }
            if (str066c == "(4")
            {
                this.DefaultCodePage = 0x34;
                this.DefaultMultibyte = 0;
                this.DefaultG = 0;
            }
            if (str066c == "(B")
            {
                this.DefaultCodePage = 0x42;
                this.DefaultMultibyte = 0;
                this.DefaultG = 0;
            }
            if (str066c == "$1")
            {
                this.DefaultCodePage = 0x31;
                this.DefaultMultibyte = 1;
                this.DefaultG = 1;
            }
            // 4E(hex) [ASCII graphic: N] = Basic Cyrillic 
            if (str066c == "(N")
            {
                this.DefaultCodePage = 0x4E;
                this.DefaultMultibyte = 0;
                this.DefaultG = 0;
            }
            // 51(hex) [ASCII graphic: Q] = Extended Cyrillic
            if (str066c == "(Q")
            {
                this.DefaultCodePage = 0x51;
                this.DefaultMultibyte = 0;
                this.DefaultG = 0;
            }
            // 53(hex) [ASCII graphic: S] = Basic Greek
            if (str066c == "(S")
            {
                this.DefaultCodePage = 0x53;
                this.DefaultMultibyte = 0;
                this.DefaultG = 0;
            }
            // 32(hex) [ASCII graphic: 2] = Basic Hebrew
            if (str066c == "(2")
            {
                this.DefaultCodePage = 0x32;
                this.DefaultMultibyte = 0;
                this.DefaultG = 0;
            }


            return 0;
#endif
        }

        // 将MARC-8字符串转换为Unicode字符串
        /*
(3 for basic Arabic 
(4 for extended Arabic 
Beng for Bengali 
$1 for CJK 
(N for basic Cyrillic 
(Q for extended Cyrillic 
Deva for Devanagari 
(S for Greek 
(2 for Hebrew 
Taml for Tamil 
Thai for Thai
         * * */
        public int Marc8_to_Unicode(
            byte[] baSource,
            out string strTarget)
        {
            int m = this.DefaultMultibyte;  // multiple ?
            int iCodeG0 = this.DefaultG0CodePage;
            int iCodeG1 = this.DefaultG1CodePage;

            strTarget = "";

            string strTemp = "";
            int nRet = 0;
            string strPart = "";

            string strError = "";

            for (int i = 0; i <= baSource.Length; )
            {
                // char ch = strSource[i];
                if (i >= baSource.Length || baSource[i] == 0x1b)
                {


                    // 将strPart中的处理掉
                    // 看看strPart中是否有积累的内容
                    if (strPart != "")
                    {
                        if (m == 0)
                        {
                            nRet = AsciiToUnicode(iCodeG0,
                                iCodeG1,
                                strPart,
                                out strTemp,
                                out strError);
                            if (nRet == -1)
                                strTemp = strError;

                            strTarget += strTemp;
                            strPart = "";
                        }
                        else // m==1
                        {
                            if ((strPart.Length % 3) != 0)
                            {
                                strTarget += strPart;
                                strPart = "";
                                continue;
                            }
                            Debug.Assert((strPart.Length % 3) == 0, "");

                            // strPart中必须存放非Unicode字符串
                            nRet = this.EaccCharsetTable.EACCToUnicode(strPart,
                                out strTemp,
                                out strError);
                            if (nRet == -1)
                            {
                                strTarget += "[EACCToUnicode error:"
                                    + strError + "][" + strPart + "]";
                                strPart = "";
                                goto CONTINUE;
                            }
                            strTarget += strTemp;

                            strPart = "";
                            goto CONTINUE;
                        }
                    }

                CONTINUE:
                    if (i >= baSource.Length)   // 最后一次
                        break;

                    // escape code


                    // 观察随后一位
                    /*
Technique 1: Greek Symbols, Subscript, and Superscript Characters
Three Greek symbols (alpha, beta, and gamma), fourteen subscript characters, and fourteen superscript characters have been placed in three separate character sets that are accessed by a locking escape sequence. The technique for accessing these characters is outside the framework specified in ANSI X3.41 or ISO 2022. These three special sets are designated as G0 sets in codes 21(hex) through 7E(hex) by means of a two-character sequence consisting of the Escape character and an ASCII graphic character. The specific escape sequences for the three special sets are:

ESCg (ASCII 1B(hex) 67(hex)) for the Greek symbol set
ESCb (ASCII 1B(hex) 62(hex)) for the Subscript set
ESCp (ASCII 1B(hex) 70(hex)) for the Superscript set

When one of these character sets is designated using the escape sequence, the escape is locking which means that all characters following the escape sequence are interpreted as being part of the newly designated character set until another escape sequence is encountered. This follow-on escape sequence may redesignate ASCII or designate another special character set as the G0 set. To redesignate ASCII, the following two-character escape sequence is used:

ESCs (ASCII 1B(hex) 73(hex)) for ASCII default character set
 * */
                    i++;
                    byte b1 = baSource[i];

                    if (b1 == 0x67)
                    {
                        iCodeG0 = 0x67;
                        i++;
                        continue;
                    }
                    if (b1 == 0x62)
                    {
                        iCodeG0 = 0x62;
                        i++;
                        continue;
                    }
                    if (b1 == 0x70)
                    {
                        iCodeG0 = 0x70;
                        i++;
                        continue;
                    }
                    if (b1 == 0x73)    // default
                    {
                        iCodeG0 = 0x73;
                        i++;
                        continue;
                    }

                    ///////
                    if (b1 == 0x28 || b1 == 0x2c)
                    {
                        // Technique 2
                        m = 0;
                        i++;
                        if (i >= baSource.Length)
                            break;  // 保险

                        iCodeG0 = baSource[i];
                        i++;
                        if (i >= baSource.Length)
                            break;  // 保险
                        continue;
                    }
                    else if (b1 == 0x29 || b1 == 0x2d)
                    {
                        // Technique 2
                        m = 0;
                        i++;
                        if (i >= baSource.Length)
                            break;  // 保险

                        iCodeG1 = baSource[i];
                        i++;
                        if (i >= baSource.Length)
                            break;  // 保险
                        continue;
                    }
                    else if (baSource[i] == 0x24)
                    {
                        // 还需要后一位综合判断
                        i++;
                        if (i >= baSource.Length)
                            break;  // 保险

                        byte b2 = baSource[i];


                        // 0x24 and next 0x29 
                        // 0x24 and next 0x2d
                        if (b2 == 0x29 || b2 == 0x2d)
                        {
                            m = 1;
                            i++;
                            if (i >= baSource.Length)
                                break;  // 保险

                            iCodeG1 = baSource[i];
                            i++;
                            if (i >= baSource.Length)
                                break;  // 保险
                            continue;
                        }
                        // 0x24 and next 0x2c
                        if (b2 == 0x2c)
                        {
                            m = 1;
                            i++;
                            if (i >= baSource.Length)
                                break;  // 保险

                            iCodeG0 = baSource[i];
                            i++;
                            if (i >= baSource.Length)
                                break;  // 保险
                            continue;
                        }

                        // 0x24 and next not 0x2c
                        m = 1;
                        // i就不++了
                        iCodeG0 = baSource[i];
                        i++;
                        if (i >= baSource.Length)
                            break;  // 保险
                        continue;
                    }


                    /*
                     * 
                    33(hex) [ASCII graphic: 3] = Basic Arabic
34(hex) [ASCII graphic: 4] = Extended Arabic
42(hex) [ASCII graphic: B] = Basic Latin (ASCII)
21 45(hex) [ASCII graphic: !E] = Extended Latin (ANSEL)
31(hex) [ASCII graphic: 1] = Chinese, Japanese, Korean (EACC)
4E(hex) [ASCII graphic: N] = Basic Cyrillic 
51(hex) [ASCII graphic: Q] = Extended Cyrillic
53(hex) [ASCII graphic: S] = Basic Greek
32(hex) [ASCII graphic: 2] = Basic Hebrew
                     * */

                    continue;
                }

                // 普通内容
                strPart += (char)baSource[i];
                i++;

            }


            return 0;
        }

        public override int GetMaxByteCount(
            int charCount)
        {
            return charCount*3;
        }

        public override int GetByteCount(
            char[] chars,
            int index,
            int count)
        {
            throw new Exception("Unicode to MARC-8 暂未实现");
            // return 0;
        }

        // 将内部字符串转换为EACC
        public override int GetBytes(
    char[] chars,
    int charIndex,
    int charCount,
    byte[] bytes,
    int byteIndex)
        {
            throw new Exception("Unicode to MARC-8 暂未实现");
        }

        public override int GetMaxCharCount(
    int byteCount)
        {
            return byteCount;
        }

        public override int GetCharCount(
    byte[] bytes,
    int index,
    int count)
        {
            byte[] baSource = new byte[count];
            Array.Copy(bytes, index, baSource, 0, count);

            string strTarget = null;
            this.Marc8_to_Unicode(
                baSource,
                out strTarget);

            return strTarget.Length;
        }

        // 将EACC编码转换为内部字符串
        public override int GetChars(
    byte[] bytes,
    int byteIndex,
    int byteCount,
    char[] chars,
    int charIndex)
        {
            byte [] baSource = new byte[byteCount];
            Array.Copy(bytes, byteIndex, baSource, 0, byteCount);

            string strTarget = null;
            this.Marc8_to_Unicode(
                baSource, 
                out strTarget);
            strTarget.CopyTo(0, chars, charIndex, strTarget.Length);
          
            return strTarget.Length;
        }

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

            List<byte[]> aField = new List<byte[]>();
            if (bForce == true)
            {
                nRet = MarcUtil.ForceCvt2709ToFieldArray(ref encoding,  //2007/7/16
                    baRecord,
                    aField,
                    out strError);
            }
            else
            {
                //???
                nRet = MarcUtil.Cvt2709ToFieldArray(
                    encoding,   // 2007/7/16
                    baRecord,
                    aField,
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

                strMarc = strHeader;
                for (int i = 1; i < saField.Length; i++)
                {
                    strMarc += saField[i] + new string(MarcUtil.FLDEND, 1);
                }

                return 0;
            }

            return 0;
        }

        // aSourceField:	MARC字段数组。注意ArrayList每个元素要求为byte[]类型
        static int GetMarcRecordString(List<byte[]> aSourceField,
            Encoding encoding,
            out string[] saTarget)
        {
            string strField066Value = "";

            // 提前得到066字段
            for (int j = 1; j < aSourceField.Count; j++)
            {
                string strField = Encoding.ASCII.GetString((byte[])aSourceField[j]);
                string strFieldName = "";
                if (strField.Length >= 5)
                    strFieldName = strField.Substring(0, 3);
                else
                    continue;

                if (strFieldName == "066")
                {
                    strField066Value = strField.Substring(5);
                    break;
                }
            }

            // 根据066字段内容提示Marc8Encoding切换状态
            saTarget = new string[aSourceField.Count];
            for (int j = 0; j < aSourceField.Count; j++)
            {
                if (encoding is Marc8Encoding)
                {
                    Marc8Encoding marc8 = (Marc8Encoding)encoding;
                    marc8.SetDefaultCodePage(strField066Value);
                    saTarget[j] = marc8.GetString((byte[])aSourceField[j]);
                }
                else
                {
                    // 一般编码方式
                    saTarget[j] = encoding.GetString((byte[])aSourceField[j]);
                }
            }

            return 0;
        }

    }

    // 一个代码事项
    public class CodeEntry
    {
        public int ASCII = 0;
        public int Unicode = 0;
    }

    // 一个代码页
    public class CodePage : List<CodeEntry>
    {
        public int Code = 0;
        public string Name = "";

        public int GetUnicode(byte s)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (s == this[i].ASCII)
                    return this[i].Unicode;
            }

            return 0;   // not found
        }
    }

}
