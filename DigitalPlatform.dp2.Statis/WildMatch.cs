using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DigitalPlatform.dp2.Statis
{

    public class ModeCell
    {
        internal int m_nType = 0;	// 单元类型
        internal string m_strValue = "";	// 值
        // 当单元类型为MC_CHAR时，m_strValue中为一个字符
        // 当单元类型为MC_STRING时，m_strValue中为多个字符
        // 当单元类型为MC_CHARLIST时，m_strValue中为字符可能值的列举
        //		列举为双字符成组出现，如果双字符首尾相同，实际上表示了单字符

        internal int m_nValue = 0;	// 值。一般用于MC_MULTI类型单元存放后面连续的固定长度匹配内容总宽度
        internal int m_nStartOffs = 0;	// 开始匹配的偏移
        internal bool m_bMatch = false;		// 本单元是否匹配成功
        internal int m_nStyle = 0;		// 本单元风格

    }

    /// <summary>
    /// 通配符
    /// </summary>
    public class WildMatch
    {
        string m_strMode = "";
        string m_strModeHead = "";
        bool m_bCaseSensitive = false;
        List<ModeCell> m_ModeArray = null;

        public char CHAR_MULTI = '%';
        public char CHAR_SINGLE = '_';
        public char CHAR_QUOTELEFT = '[';
        public char CHAR_QUOTERIGHT = ']';

        const int MC_MULTI = 1;	// 任意0-多个字符
        const int MC_SINGLE = 2;	// 任意单个字符	
        const int MC_CHAR = 3;	// 单个字符
        const int MC_STRING = 4;	// 多个字符
        const int MC_CHARLIST = 5;	// 字符可能性列举

        const int MC_STYLE_RIGHTMOST_MULTI = 0x01;	// 最右边一个MS_MULTI类型单元


        // 构造函数
        // parameters:
        //      strWildCharDef  通配字符的定义。如果为null，表示用缺省的，相当于"%_[]"
        //              顺次需要定义 任意字符 单个字符 字符列举的左括号 字符列举的右括号
        public WildMatch(string strPattern,
            string strWildCharDef)
        {
            if (String.IsNullOrEmpty(strWildCharDef) == false)
            {
                if (strWildCharDef.Length >= 1)
                    this.CHAR_MULTI = strWildCharDef[0];
                if (strWildCharDef.Length >= 2)
                    this.CHAR_SINGLE = strWildCharDef[1];
                if (strWildCharDef.Length >= 3)
                    this.CHAR_QUOTELEFT = strWildCharDef[2];
                if (strWildCharDef.Length >= 4)
                    this.CHAR_QUOTERIGHT = strWildCharDef[3];
            }

            MakeMatchArray(strPattern);
        }

        /*

        public static int[] GetNext(string a)
        {
            int[] next = new int[a.Length];

            next[0] = 0;
            int i = 0;
            int j = 0;
            while (i < next.Length - 1)
            {
                if (j == 0 || a[i] == a[j - 1])
                {
                    ++i;
                    ++j;
                    if (a[i] != a[j - 1])
                        next[i] = j;
                    else
                        next[i] = next[j - 1];
                }
                else
                    j = next[j - 1];
            }

            return next;
        }

        public static int KMP(string zc, string ppc)
        {
            int[] next = Program.GetNext(ppc);
            int i = 0; int j = 0;
            while (i < zc.Length && j < ppc.Length)
            {
                if (j == 0 || zc[i] == ppc[j - 1])
                {
                    ++i;
                    ++j;
                }
                else
                    j = next[j - 1];
            }
            if (j >= ppc.Length)
                return i - ppc.Length + 1;//匹配的话返回startindex
            else
                return -1;//不匹配的话返回-1
        }
         * */

        static string GetModeHead(string strMode)
        {
            string strModeHead = "";

            if (strMode.Length != 0)
            {
                char chFirst;

                chFirst = strMode[0];

                if (chFirst == '_'
                    || chFirst == '%'
                    || chFirst == '[')
                {
                    // 空
                }
                else
                {
                    int nRet;
                    // 截取前面纯净的一截
                    nRet = strMode.IndexOfAny(new char[] { '_', '%', '[' });
                    if (nRet != -1)
                    {
                        strModeHead = strMode.Substring(0, nRet);
                    }
                    else
                    {
                        strModeHead = strMode;
                    }

                }
            }
            return strModeHead;	// 空
        }


        // 创建匹配模式数组
        // return:
        //		-1	error
        //		0	suceed
        int MakeMatchArray(string strMode)
        {
            // LPTSTR p;
            ModeCell cell = null;
            bool bInQuote = false;
            char chMin = (char)0;
            char chMax = (char)0;
            bool bMinFited = false;

            // 记忆
            this.m_strMode = strMode;

            this.m_strModeHead = GetModeHead(strMode);

            if (this.m_ModeArray == null)
                this.m_ModeArray = new List<ModeCell>();
            else
                this.m_ModeArray.Clear();

            for (int i = 0; i < strMode.Length; i++)
            {
                char ch = strMode[i];

                if (ch == CHAR_MULTI)
                {
                    cell = new ModeCell();
                    m_ModeArray.Add(cell);
                    cell.m_nType = MC_MULTI;
                    cell = null;
                    continue;
                }

                if (ch == CHAR_SINGLE)
                {
                    cell = new ModeCell();
                    m_ModeArray.Add(cell);
                    cell.m_nType = MC_SINGLE;
                    cell = null;
                    continue;
                }

                if (ch == CHAR_QUOTERIGHT)
                {
                    if (bInQuote == false)
                        return -1;
                    Debug.Assert(cell != null, "");
                    Debug.Assert(cell.m_nType == MC_CHARLIST, "");
                    if (chMin != (char)0)
                    {
                        chMax = chMin;
                        cell.m_strValue += chMin;
                        cell.m_strValue += chMax;
                        chMin = (char)0;
                        chMax = (char)0;
                        bMinFited = false;
                    }
                    // 收尾
                    bInQuote = false;
                    cell = null;
                    continue;
                }

                if (ch == CHAR_QUOTELEFT)
                {
                    cell = new ModeCell();
                    m_ModeArray.Add(cell);
                    cell.m_nType = MC_CHARLIST;
                    bInQuote = true;
                    continue;
                }

                if (bInQuote == true)
                {
                    Debug.Assert(cell != null, "");
                    Debug.Assert(cell.m_nType == MC_CHARLIST, "");

                    if (chMin != (char)0 && bMinFited == false)
                    {
                        if (ch == '-')
                        {
                            bMinFited = true;
                            continue;
                        }
                        else
                        {
                            // 填写前一个
                            chMax = chMin;
                            cell.m_strValue += chMin;
                            cell.m_strValue += chMax;
                            chMin = (char)0;
                            chMax = (char)0;
                            bMinFited = false;
                            // 填写后一个(当前这一个)
                            cell.m_strValue += ch;
                            cell.m_strValue += ch;
                            continue;
                        }
                    }

                    if (chMin != (char)0 && bMinFited == true)
                    {
                        chMax = ch;
                        // min > max怎么办?
                        // min > max怎么办?
                        if (chMin > chMax)
                        {
                            char chTemp = chMin;
                            chMin = chMax;
                            chMax = chTemp;
                        }
                        cell.m_strValue += chMin;
                        cell.m_strValue += chMax;
                        chMin = (char)0;
                        chMax = (char)0;
                        bMinFited = false;
                        continue;
                    }
                    if (chMin == (char)0)
                    {
                        Debug.Assert(bMinFited == false, "");
                        chMin = ch;
                        continue;
                    }

                }

                Debug.Assert(bInQuote == false, "");

                if (cell != null)
                {
                    Debug.Assert(cell.m_nType == MC_STRING, "");
                }
                else
                {
                    cell = new ModeCell();
                    m_ModeArray.Add(cell);
                    cell.m_nType = MC_STRING;
                }
                cell.m_strValue += ch;

            }


            // 需要将连续的MC_MULTI类型单元归并?

            return 0;
        }

        // 清除数组中的临时变量， 计算出一些值
        // 
        void InitialArrayVars()
        {
            int nRightMostMulti = -1;	// 最右边一个MS_MULTI单元
            int nFixWidth = 0;
            ModeCell lastMultiCell = null;

            ModeCell cell = null;

            for (int i = 0; i < this.m_ModeArray.Count; i++)
            {
                cell = m_ModeArray[i];
                cell.m_nStartOffs = 0;
                cell.m_bMatch = false;
                cell.m_nStyle = 0;

                if (cell.m_nType == MC_MULTI)
                {
                    if (lastMultiCell != null)
                    {
                        if (nFixWidth == 0)
                        {
                            // 紧挨着的两个MC_MULTI要去重
                            Debug.Assert(i != 0, "");
                            m_ModeArray.RemoveAt(i);
                            i--;
                            cell = m_ModeArray[i];
                            Debug.Assert(lastMultiCell == cell, "");
                        }
                        lastMultiCell.m_nValue = nFixWidth;
                    }
                    nFixWidth = 0;

                    nRightMostMulti = i;

                    lastMultiCell = cell;
                    continue;
                }
                /*
    #define MC_SINGLE		2	// 任意单个字符	
    #define MC_CHAR			3	// 单个字符
    #define MC_STRING		4	// 多个字符
    #define MC_CHARLIST		5	// 字符可能性列举
                */
                if (cell.m_nType == MC_SINGLE
                    || cell.m_nType == MC_CHAR
                    || cell.m_nType == MC_CHARLIST)
                    nFixWidth += 1;
                else if (cell.m_nType == MC_STRING)
                    nFixWidth += cell.m_strValue.Length;
                else
                {
                    Debug.Assert(false, "");	// 不可能出现的类型
                }

            }

            if (lastMultiCell != null)
                lastMultiCell.m_nValue = nFixWidth;

            if (nRightMostMulti != -1)
            {
                cell = m_ModeArray[nRightMostMulti];
                cell.m_nStyle = MC_STYLE_RIGHTMOST_MULTI;
            }
        }

        // 对一个字符串进行匹配
        // parameters:
        //		strResult	匹配上的字符串局部内容
        // return:
        //		-1	not match
        //		其他	首次匹配的位置
        public int Match(string strString,
                out string strResult)
        {
            int nStartOffs = 0;
            ModeCell cell = null;
            ModeCell firstcell = null;
            int nRet;
            int nWidth;
            int nUsedIdx;

            strResult = "";

            InitialArrayVars();

            if (m_ModeArray.Count == 0)
                return 0;

            for (int i = 0; i < this.m_ModeArray.Count; )
            {
                cell = m_ModeArray[i];

                if (i == 0)
                    firstcell = cell;

                if (cell.m_nType == MC_MULTI)
                {
                    if (i == this.m_ModeArray.Count - 1)	// 当前单元已经是最后一个单元
                        goto END1;
                    if ((cell.m_nStyle & MC_STYLE_RIGHTMOST_MULTI) != 0)
                    {
                        int nTempOffs;
                        nTempOffs = strString.Length - cell.m_nValue;
                        if (nTempOffs < nStartOffs)
                            return -1;
                        nStartOffs = nTempOffs;
                    }

                    cell.m_nStartOffs = nStartOffs;
                    // 匹配固定长度的一段内容
                    // return:
                    //		-1	error
                    //		0	not match
                    //		1	match
                    //		2	cell index out of range
                    //		3	比较的区域已经越过字符串最右边
                    nRet = MatchFixedLength(i + 1,
                        strString,
                        nStartOffs,
                        out nWidth,
                        out nUsedIdx);
                    if (nRet == -1)
                    {
                        Debug.Assert(false, "");
                        return -1;
                    }
                    if (nRet == 0)
                    { // not match
                        nStartOffs += 1;
                        cell.m_nStartOffs = nStartOffs;
                        continue;
                    }
                    if (nRet == 1)
                    { // match
                        nStartOffs += nWidth;
                        //cell.m_nStartOffs;
                        i += nUsedIdx + 1;
                        continue;	// 跳过已经比较过的距离，继续比较后面单元
                    }
                    if (nRet == 2)	// 当前单元已经是最后一个单元
                        goto END1;

                    if (nRet == 3)
                    {
                        return -1;
                    }


                }

                // 直接遇到非MC_MULTI单元
                nRet = MatchFixedLength(i,
                    strString,
                    nStartOffs,
                    out nWidth,
                    out nUsedIdx);
                if (nRet == -1)
                {
                    Debug.Assert(false, "");
                    return -1;
                }
                if (nRet == 0)
                { // not match
                    return -1;
                }
                if (nRet == 1)
                { // match
                    nStartOffs += nWidth;
                    i += nUsedIdx;
                    continue;	// 跳过已经比较过的距离，继续比较后面单元
                }
                if (nRet == 3)
                {
                    return -1;
                }
            }

            if (nStartOffs < strString.Length)
                return -1;

        END1:
            if (firstcell.m_nType == MC_MULTI)
                return firstcell.m_nStartOffs;	// found, match pos
            return 0;	// found, match pos
        }

        // 忽略大小写的比较
        static int memicmp(string s1,
            int start1,
            string s2,
            int start2,
            int len)
        {
            if (start1 + len > s1.Length
                || start2 + len > s2.Length)
            {
                Debug.Assert(false, "memicmp()给出的start+len超过了某一字符串的尾部");
            }

            for (int i = 0; i < len; i++)
            {
                char ch1 = char.ToLower(s1[start1 + i]);
                char ch2 = char.ToLower(s2[start2 + i]);


                int delta = ch1 - ch2;
                if (delta != 0)
                    return delta;
            }

            return 0;
        }

        // 比较
        static int memcmp(string s1,
            int start1,
            string s2, 
            int start2,
            int len)
        {
            if (start1 + len > s1.Length
                || start2 + len > s2.Length)
            {
                Debug.Assert(false, "memicmp()给出的start+len超过了某一字符串的尾部");
            }


            for(int i=0;i<len;i++)
            {
                char ch1 = s1[start1+i];
                char ch2 = s2[start2+i];

                int delta = ch1 - ch2;
                if (delta != 0)
                    return delta;
            }

            return 0;
        }

        // 匹配固定长度的一段内容
        // parameters:
        //		nCellIdx	开始的_CModeCell单元下标
        //		pszString	被检测的字符串
        //		bRightMost	是否为最右一个单元，需要特殊匹配(右对齐)
        //		nWidth		从开始的_CModeCell单元直到MC_MUITL单元之间，固定长部分的总长度
        //		nUsedIdx	连续固定区域单元的个数
        // return:
        //		-1	error
        //		0	not match
        //		1	match
        //		2	cell index out of range
        //		3	比较的区域已经越过字符串最右边
        int MatchFixedLength(int nCellIdx,
            string strString,
            int start,
            out int nWidth,
            out int nUsedIdx)
        {
            nWidth = 0;
            nUsedIdx = 0;

            ModeCell cell = null;
            int nOffs = 0;
            int nPartLen = 0;
            int nStringLen;

            nStringLen = strString.Length - start;

            int nMax = m_ModeArray.Count;
            if (nCellIdx >= nMax)
                return 2;

            for (int i = nCellIdx; i < this.m_ModeArray.Count; i++, nUsedIdx++)
            {
                cell = m_ModeArray[i];

                if (cell.m_nType == MC_MULTI)
                {
                    nWidth = nOffs;
                    return 1;
                }

                if (cell.m_nType == MC_STRING)
                {
                    nPartLen = cell.m_strValue.Length;
                    Debug.Assert(nPartLen != 0, "");

                    if (nOffs + nPartLen > nStringLen)
                        return 3;

                    if (m_bCaseSensitive)
                    {
                        if (memcmp(cell.m_strValue, 0,
                            strString, start + nOffs,
                            nPartLen)
                            == 0)
                        {
                            nOffs += nPartLen;
                            continue;
                        }
                    }
                    else
                    {
                        if (memicmp(cell.m_strValue, 0,
                            strString, start + nOffs,
                            nPartLen) == 0)
                        {
                            nOffs += nPartLen;
                            continue;
                        }
                    }
                    return 0;
                }
                else if (cell.m_nType == MC_CHARLIST)
                {
                    char chStart;
                    char chEnd;
                    char chChar = strString[start + nOffs];
                    bool bFound = false;
                    nPartLen = 1;

                    if (nOffs + nPartLen > nStringLen)
                        return 3;	// not match
                    int nCount = cell.m_strValue.Length;

                    Debug.Assert(nCount % 2 == 0, "");	// 必须是偶数
                    nCount = nCount / 2;
                    for (int j = 0; j < nCount; j++)
                    {
                        chStart = cell.m_strValue[j * 2];
                        chEnd = cell.m_strValue[j * 2 + 1];
                        if (chChar < chStart
                            || chChar > chEnd)
                            continue;	// not match
                        else
                        {
                            bFound = true;
                            break;
                        }
                    }
                    if (bFound == false)
                        return 0;

                    nOffs += nPartLen;
                }
                else if (cell.m_nType == MC_SINGLE)
                {
                    nPartLen = 1;

                    if (nOffs + nPartLen > nStringLen)
                        return 3;	// not match
                    nOffs += nPartLen;
                    continue;
                }
                else
                {
                    Debug.Assert(false, "");
                }


            }


            nWidth = nOffs;
            return 1;	// match
        }

    }



}
