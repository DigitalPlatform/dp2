using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using DigitalPlatform;

namespace DigitalPlatform.Range
{
    /// <summary>
    /// RangeItem是RangeList集合的成员，表示一个连续的范围
    /// </summary>
    public class RangeItem : IComparable
    {
        public long lStart = 0;	// 如果为-1，表示从尾端长度为lLength的一段，因为总长度未知，所以lStart未知，但是-1表明了这种状态
        public long lLength = 0;	// 如果为-1，表示从lStart开始一直到末尾。
        #region IComparable Members

        // 如果this小于obj，返回<0的值
        public int CompareTo(object obj)
        {
            RangeItem item = (RangeItem)obj;
            /*
			if (this.lStart == item.lStart)
				return (int)(this.lLength - item.lLength);
			return (int)(this.lStart - item.lStart);
             * */

            // 2012/8/26 修改
            if (this.lStart == item.lStart)
            {
                long lDelta = this.lLength - item.lLength;
                if (lDelta == 0)
                    return 0;
                if (lDelta < 0)
                    return -1;
                return 1;
            }
            {
                long lDelta = this.lStart - item.lStart;
                if (lDelta == 0)
                    return 0;
                if (lDelta < 0)
                    return -1;
                return 1;
            }
        }

        #endregion


        public RangeItem()
        {
        }

        public RangeItem(RangeItem item)
        {
            lStart = item.lStart;
            lLength = item.lLength;
        }

        // 拼接为表示范围的字符串
        public string GetContentRangeString()
        {
            Debug.Assert(this.lStart >= 0, "");
            Debug.Assert(this.lStart + this.lLength - 1 >= 0, "");

            if (lLength == 1)
                return Convert.ToString(lStart);

            return Convert.ToString(lStart) + "-" + Convert.ToString(lStart + lLength - 1);
        }
    }

    /// <summary>
    /// 表示范围的类
    /// </summary>
    public class RangeList : List<RangeItem>
    {
        public string delimeters = ",";	// 分隔符。可以列出多个
        public string contentRange = "";	// 保存范围字符串

        // 构造函数
        public RangeList(string strContentRange)
        {
            BuildItems(strContentRange);
        }

        public RangeList()
        {

        }


        public RangeList(string strContentRange,
            string delemParam)
        {
            delimeters = delemParam;

            BuildItems(strContentRange);
        }

        // 创建内容事项
        public void BuildItems(string strContentRange)
        {
            long lStart = 0;
            long lLength = 0;
            string[] split = null;

            char[] delimChars = delimeters.ToCharArray();

            split = strContentRange.Split(delimChars);

            for (int i = 0; i < split.Length; i++)
            {
                if (split[i] == "")
                    continue;
                // 根据-拆分
                int nRet = split[i].IndexOf("-");
                if (nRet == -1)
                {
                    lStart = 0; //  Convert.ToInt64(split[i]);

                    if (Int64.TryParse(split[i], out lStart) == false)
                        throw new Exception("用字符串 '" + strContentRange + "' 构造RangeList时出错：数字 '" + split[i].ToString() + "' 格式不正确");

                    lLength = 1;
                }
                else
                {
                    string left = split[i].Substring(0, nRet);
                    string right = split[i].Substring(nRet + 1);
                    left = left.Trim();
                    right = right.Trim();

                    if (left == "")
                    {
                        lStart = -1;
                        try
                        {
                            lLength = Convert.ToInt64(right);
                        }
                        catch
                        {
                            throw new Exception("用字符串 '" + strContentRange + "' 构造RangeList时出错：数字 '" + right + "' 格式不正确");
                        }
                        goto CONTINUE;
                    }
                    else
                    {
                        try
                        {
                            lStart = Convert.ToInt64(left);
                        }
                        catch
                        {
                            throw new Exception("用字符串 '" + strContentRange + "' 构造RangeList时出错：数字 '" + left + "' 格式不正确");
                        }

                    }

                    if (right == "")
                    {
                        lLength = -1;
                        // 此时lStart不能为-1
                        goto CONTINUE;
                    }
                    else
                    {
                        long lEnd = 0;  // Convert.ToInt64(right);
                        if (Int64.TryParse(right, out lEnd) == false)
                            throw new Exception("用字符串 '" + strContentRange + "' 构造RangeList时出错：数字 '" + right.ToString() + "' 格式不正确");
                        if (lStart > lEnd)
                        {
                            // TODO: 纠正超过 MaxValue 的情况
                            lLength = (lStart - lEnd) + 1;
                            lStart = lEnd;
                        }
                        else
                        {
                            // TODO: 纠正超过 MaxValue 的情况
                            lLength = (lEnd - lStart) + 1;
                        }
                    }

                }
            CONTINUE:
                RangeItem item = new RangeItem();
                item.lStart = lStart;
                item.lLength = lLength;
                this.Add(item);
            }

            contentRange = strContentRange;	// 保存起来
        }

        // 拼接为表示范围的字符串
        public string GetContentRangeString()
        {
            string strResult = "";

            for (int i = 0; i < Count; i++)
            {
                RangeItem item = (RangeItem)this[i];
                if (i != 0)
                    strResult += ",";
                strResult += item.GetContentRangeString();
            }

            return strResult;
        }

        // 拼接为表示范围的字符串
        public string GetContentRangeString(int nStart, int nCount)
        {
            string strResult = "";

            for (int i = nStart; i < this.Count && i < nStart + nCount; i++)
            {
                RangeItem item = (RangeItem)this[i];
                if (strResult != "")
                    strResult += ",";
                strResult += item.GetContentRangeString();
            }

            return strResult;
        }

        // 获得最大边界
        // 所谓最大边界，是范围中出现的最大数字。是包含了这个数。
        public long max()
        {
            long lValue = 0;
            for (int i = 0; i < Count; i++)
            {
                RangeItem item = (RangeItem)this[i];
                if (item.lLength == -1)
                    return -1;	// 表示不确定，相当于无穷大
                if (item.lStart + item.lLength + -1 > lValue)
                    lValue = item.lStart + item.lLength - 1;
            }

            return lValue;
        }

        // 获得最小边界
        // 所谓最小边界，是范围中出现的最小数字。包含这个数字。
        public long min()
        {
            long lValue = 0;
            bool bFirst = true;
            for (int i = 0; i < Count; i++)
            {
                RangeItem item = (RangeItem)this[i];
                if (bFirst == true)
                {
                    lValue = item.lStart;
                    bFirst = false;
                }
                else
                {
                    if (item.lStart < lValue)
                        lValue = item.lStart;
                }
            }

            return lValue;
        }


        // bIsOrdered	true表示RangeList是排序过的，算法更优化
        public bool IsInRange(long lNumber,
            bool bIsOrdered)
        {
            for (int i = 0; i < this.Count; i++)
            {
                RangeItem item = (RangeItem)this[i];
                if (item.lLength == -1)
                {
                    if (lNumber >= item.lStart)
                        return true;
                }
                else if (item.lStart <= lNumber && item.lStart + item.lLength > lNumber)   // BUG!!! item.lStart + item.lLength >= lNumber
                    return true;
                if (bIsOrdered == true)
                {
                    if (item.lStart > lNumber)
                        break;
                }
            }
            return false;
        }

        // 合并重叠的事项
        // 要求事先排序。否则不能保证运算正确性。
        public int Merge()
        {
            for (int i = 0; i < this.Count; i++)
            {
                RangeItem item1 = (RangeItem)this[i];
                if (item1.lLength == 0)
                {
                    this.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int j = i + 1; j < this.Count; j++)
                {
                    RangeItem item2 = (RangeItem)this[j];

                    if (item2.lStart == item1.lStart + item1.lLength)
                    {
                        // 紧邻
                        item1.lLength += item2.lLength;
                        this.RemoveAt(j);
                        j--;
                        continue;
                    }
                    else if (item2.lStart >= item1.lStart
                        && item2.lStart <= item1.lStart + item1.lLength - 1)
                    {
                        // 有重叠
                        long end1 = item1.lStart + item1.lLength;
                        long end2 = item2.lStart + item2.lLength;
                        if (end1 <= end2)
                            item1.lLength = end2 - item1.lStart;
                        else
                            item1.lLength = end1 - item1.lStart;

                        // item1.lLength = item2.lStart + item2.lLength - item1.lStart;
                        this.RemoveAt(j);
                        j--;
                        continue;
                    }
                    else
                    {
                        break;	// 没有重叠
                    }

                }

            }
            return 0;

        }

        public static void CrossOper(RangeList source1,
            RangeList source2,
            RangeList targetLeft,
            RangeList targetMiddle,
            RangeList targetRight)
        {
            int i, j;

            RangeItem item1 = null;	// 左边队列
            RangeItem item2 = null;	// 右边队列

            RangeItem item = null;	// 临时

            bool bFinished1 = false;
            bool bFinished2 = false;

            for (i = 0, j = 0; ; )
            {
                // 取队列1
                if (item1 == null && bFinished1 == false
                    && i < source1.Count)
                {
                    item1 = (RangeItem)source1[i];
                    if (item1 == null)
                    {
                        throw (new ArgumentException("source1数组中位置" + Convert.ToString(i) + "(从0开始计数)包含空元素..."));
                    }
                    i++;
                }

                // 取队列2
                if (item2 == null && bFinished2 == false
                    && j < source2.Count)
                {
                    item2 = (RangeItem)source2[j];
                    if (item2 == null)
                    {
                        throw (new ArgumentException("source2数组中位置" + Convert.ToString(j) + "(从0开始计数)包含空元素..."));
                    }
                    j++;
                }

                if (item1 == null && item2 == null)
                    break;	// 全部处理完成了

                // 比较两个Item
                if (item1 != null && item2 != null)
                {
                    // item1完全小于item2
                    if (item1.lStart + item1.lLength <= item2.lStart)
                    {
                        item = new RangeItem(item1);
                        if (targetLeft != null)
                            targetLeft.Add(item);
                        item1 = null;	// 为队列1取下一个作准备
                        continue;
                    }
                    // item2完全小于item1
                    if (item2.lStart + item2.lLength <= item1.lStart)
                    {
                        item = new RangeItem(item2);
                        if (targetRight != null)
                            targetRight.Add(item);
                        item2 = null;	// 为队列2取下一个作准备
                        continue;
                    }
                    // item1和item2部分重叠

                    // item1在前
                    if (item1.lStart <= item2.lStart)
                    {
                        // |item1     |
                        //        |item2      |
                        // |  A   | B |   C   |

                        // item1 A部分去targetLeft
                        if (item1.lStart != item2.lStart)
                        {
                            item = new RangeItem();
                            item.lStart = item1.lStart;
                            item.lLength = item2.lStart - item1.lStart;
                            if (targetLeft != null)
                                targetLeft.Add(item);
                        }

                        // item1和item2重叠的部分B，去targetMiddle
                        item = new RangeItem();
                        item.lStart = item2.lStart;

                        /*
                        long end1 = item1.lStart + item1.lLength;
                        long end2 = item2.lStart + item2.lLength;

                        if (end1 <= end2) 
                        {
                            item.lLength = end1 - item.lStart;
                        }
                        else 
                        {
                            item.lLength = end2 - item.lStart;
                        }
                        if (targetMiddle != null)
                            targetMiddle.Add(item);

                        // item2和Item2不重叠的C部分，留下来做下次循环

                        if (end1 <= end2) 
                        {
                            if (end1 == end2) 
                            {
                                item1 = null;
                                item2 = null;
                                continue;
                            }
                            item2.lStart = end1;
                            item2.lLength = end2 - end1;
                            item1 = null;
                            continue;
                        }
                        else 
                        {
                            item1.lStart = end2;
                            item1.lLength = end1 - end2;
                            item2 = null;
                            continue;
                        }
                        */


                    } // item1在前

                    // item2在前
                    else // if (item1.lStart > item2.lStart) 
                    {
                        // |item2     |
                        //        |item1      |
                        // |  A   | B |   C   |

                        // item2 A部分去targetRight
                        item = new RangeItem();
                        item.lStart = item2.lStart;
                        item.lLength = item1.lStart - item2.lStart;
                        if (targetRight != null)
                            targetRight.Add(item);

                        // item1和item2重叠的部分B，去targetMiddle
                        item = new RangeItem();
                        item.lStart = item1.lStart;
                        /*
                        long end1 = item1.lStart + item1.lLength;
                        long end2 = item2.lStart + item2.lLength;

                        if (end1 <= end2) 
                        {
                            item.lLength = end1 - item.lStart;
                        }
                        else 
                        {
                            item.lLength = end2 - item.lStart;
                        }
                        if (targetMiddle != null)
                            targetMiddle.Add(item);

                        // item2和Item2不重叠的C部分，留下来做下次循环

                        if (end1 <= end2) 
                        {
                            if (end1 == end2) 
                            {
                                item1 = null;
                                item2 = null;
                                continue;
                            }
                            item2.lStart = end1;
                            item2.lLength = end2 - end1;
                            item1 = null;
                            continue;
                        }
                        else 
                        {
                            item1.lStart = end2;
                            item1.lLength = end1 - end2;
                            item2 = null;
                            continue;
                        }
                        */


                    } // item2在前

                    if (true)
                    { // C部分
                        long end1 = item1.lStart + item1.lLength;
                        long end2 = item2.lStart + item2.lLength;

                        if (end1 <= end2)
                        {
                            item.lLength = end1 - item.lStart;
                        }
                        else
                        {
                            item.lLength = end2 - item.lStart;
                        }
                        if (targetMiddle != null)
                            targetMiddle.Add(item);

                        // item2和Item2不重叠的C部分，留下来做下次循环

                        if (end1 <= end2)
                        {
                            if (end1 == end2)
                            {
                                item1 = null;
                                item2 = null;
                                continue;
                            }
                            item2.lStart = end1;
                            item2.lLength = end2 - end1;
                            item1 = null;
                            continue;
                        }
                        else
                        {
                            item1.lStart = end2;
                            item1.lLength = end1 - end2;
                            item2 = null;
                            continue;
                        }
                    } // -- C部分


                    // continue;
                } // -- 比较两个Item

                // 只有Item1非空
                if (item1 != null)
                {
                    if (targetLeft != null)
                        targetLeft.Add(item1);
                    item1 = null;
                    continue;
                }
                // 只有Item2非空
                if (item2 != null)
                {
                    if (targetRight != null)
                        targetRight.Add(item2);
                    item2 = null;
                    continue;
                }
            }
        }

        // 将strRange1中表示的范围减去strRange2的范围，返回
        public static string Sub(string strRange1, string strRange2)
        {
            RangeList rl1 = new RangeList(strRange1);
            RangeList rl2 = new RangeList(strRange2);

            RangeList result = new RangeList();
            RangeList.CrossOper(rl1,
                rl2,
                result,
                null,
                null);
            return result.GetContentRangeString();
        }

        // 返回范围中包含数字个数
        public static long GetNumberCount(string strRange)
        {
            RangeList rl = new RangeList(strRange);
            long lTotal = 0;
            for (int i = 0; i < rl.Count; i++)
            {
                RangeItem item = (RangeItem)rl[i];
                lTotal += item.lLength;
            }

            return lTotal;
        }

        // 把一个contentrange字符串按照分块尺寸切割为多个contentrange字符串
        // 原理：
        // 按照数字的个数来切割。和数字本身的值无关。
        // 计算把每个连续的段落包含的数字个数，凑够了chunksize就输出字符串。如果不够，
        // 则把多个段落一起输出为一个字符串。
        public static string[] ChunkRange(string strRange, long lChunkSize)
        {
            if (lChunkSize <= 0)
                throw (new ArgumentException("RangeList.ChunkRange(string strRange, long lChunkSize): lChunkSize参数必须大于0"));

            string[] result = null;

            // 空范围 2006/6/27
            if (String.IsNullOrEmpty(strRange) == true)
            {
                result = new string[1];
                result[0] = strRange;
                return result;
            }


            RangeList rl = new RangeList(strRange);

            ArrayList aText = new ArrayList();

            long lCurSize = 0;
            int nStartIndex = 0;

            for (int i = 0; i < rl.Count; i++)
            {
                RangeItem item = (RangeItem)rl[i];
                lCurSize += item.lLength;
                if (lCurSize >= lChunkSize)
                {
                    string strText = "";
                    // 从nStart到i之间转换为一个字符串
                    if (nStartIndex < i)
                    {
                        strText += rl.GetContentRangeString(nStartIndex, i - nStartIndex);
                        strText += ",";
                    }

                    long lDelta = lCurSize - lChunkSize;
                    // i所在位置chunk点左边的转换为一个字符串
                    strText += Convert.ToString(item.lStart) + "-"
                        + Convert.ToString(item.lStart + item.lLength - 1 - lDelta);
                    // 余下的部分重新写入i位置item 
                    if (lDelta > 0)
                    {
                        nStartIndex = i;
                        long lUsed = item.lLength - lDelta;
                        item.lStart += lUsed;
                        item.lLength -= lUsed;
                        i--;
                    }
                    else
                    {
                        nStartIndex = i + 1;
                    }
                    aText.Add(strText);
                    lCurSize = 0;
                    continue;
                }

            }

            // 最后一次
            if (nStartIndex < rl.Count)
            {
                string strText = "";
                // 从nStart到i之间转换为一个字符串
                strText += rl.GetContentRangeString(nStartIndex, rl.Count - nStartIndex);
                aText.Add(strText);
            }

            if (aText.Count > 0)
            {
                result = new string[aText.Count];
                for (int j = 0; j < aText.Count; j++)
                {
                    result[j] = (string)aText[j];
                }
            }
            else // 确保数组有至少一个元素
            {
                result = new string[1];
                result[0] = strRange;
            }

            return result;
        }

        // 合并两个contentrange字符串为一个新串
        // parameters:
        //		strS1	第一个范围字符串
        //		strS2	第二个范围字符串
        //		lWholeLength	大文件的尺寸。用来检测本次合并后的字符串是否已经完全覆盖整个文件范围
        //		strResult	out参数，返回合并后的字符串
        // return
        //		-1	出错 
        //		0	还有未覆盖的部分 
        //		1	本次已经完全覆盖
        public static int MergeContentRangeString(string strS1,
            string strS2,
            long lWholeLength,
            out string strResult,
            out string strError)
        {
            strError = "";

            RangeList rl1 = new RangeList(strS1);

            RangeList rl2 = new RangeList(strS2);

            // 组合两个RangeList
            rl1.AddRange(rl2);

            // 排序
            rl1.Sort();

            // 合并事项
            rl1.Merge();

            // 调试用!
            // Debug.Assert(rl1.Count == 1, "");

            // 返回合并后的contentrange字符串
            strResult = rl1.GetContentRangeString();

            if (rl1.Count == 1)
            {
                RangeItem item = (RangeItem)rl1[0];

                if (item.lLength > lWholeLength)
                {
                    strError = "唯一一个事项的长度 " + item.lLength.ToString() + " 居然大于整体长度 " + lWholeLength.ToString();
                    return -1;	// 唯一一个事项的长度居然超过检测的长度，通常表明有输入参数错误
                }

                if (item.lStart == 0
                    && item.lLength == lWholeLength)
                    return 1;	// 表示完全覆盖
            }

            return 0;	// 还有未覆盖的部分
        }

        // 将源文件中指定的片断内容复制到目标文件中
        // 当strContentRange的值为""时，表示复制整个文件
        // 返回值：-1 出错 其他 复制的总尺寸
        public static long CopyFragment(
            string strSourceFileName,
            string strContentRange,
            string strTargetFileName,
            out string strErrorInfo)
        {
            long lTotalBytes = 0;
            strErrorInfo = "";

            FileInfo fi = new FileInfo(strSourceFileName);
            if (fi.Length == 0)
                return 0;
            // 表示范围的字符串为空，恰恰表示要包含全部范围
            if (strContentRange == "")
            {
                strContentRange = "0-" + Convert.ToString(fi.Length - 1);
            }

            // 创建RangeList，便于理解范围字符串
            RangeList rl = new RangeList(strContentRange);


            // 检查strContentRange指出的最大最小边界和源文件中实际情况是否矛盾
            long lMax = rl.max();
            if (fi.Length <= lMax)
            {
                strErrorInfo = "文件" + strSourceFileName + "文件尺寸比范围" + strContentRange + "中定义的最大边界"
                    + Convert.ToString(lMax) + "小...";
                return -1;
            }

            long lMin = rl.min();
            if (fi.Length <= lMin)
            {
                strErrorInfo = "文件" + strSourceFileName + "文件尺寸比范围" + strContentRange + "中定义的最小边界"
                    + Convert.ToString(lMax) + "小...";
                return -1;
            }

            using (FileStream fileTarget = File.Create(strTargetFileName))
            using (FileStream fileSource = File.Open(strSourceFileName,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // 循环，复制每个连续片断
                for (int i = 0; i < rl.Count; i++)
                {
                    RangeItem ri = (RangeItem)rl[i];

                    fileSource.Seek(ri.lStart, SeekOrigin.Begin);
                    DumpStream(fileSource, fileTarget, ri.lLength, true);

                    lTotalBytes += ri.lLength;
                }
            }

            return lTotalBytes;
        }

        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            long lLength,
            bool bFlush)
        {
            long lWrited = 0;
            long lThisRead = 0;
            int nChunkSize = 8192;
            byte[] bytes = new byte[nChunkSize];
            while (true)
            {
                long lLeft = lLength - lWrited;
                if (lLeft > nChunkSize)
                    lThisRead = nChunkSize;
                else
                    lThisRead = lLeft;
                long n = streamSource.Read(bytes, 0, (int)lThisRead);

                if (n != 0) // 2005/6/8
                {
                    streamTarget.Write(bytes, 0, (int)n);
                }

                if (bFlush == true)
                    streamTarget.Flush();


                //if (n<nChunkSize)
                //	break;
                if (n <= 0)
                    break;

                lWrited += n;
            }

            return lWrited;
        }

        // 将源文件中指定的片断内容复制到目标文件中
        // 当strContentRange的值为""时，表示复制整个文件
        // 返回值：-1 出错 其他 复制的总尺寸
        public static long CopyFragment(
            string strSourceFileName,
            string strContentRange,
            out byte[] baResult,
            out string strErrorInfo)
        {
            baResult = null;
            strErrorInfo = "";
            FileInfo fi = new FileInfo(strSourceFileName);
            if (fi.Length == 0)
                return 0;
            using (FileStream fileSource = File.Open(
                strSourceFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                long lRet = CopyFragment(fileSource,
                    fi.Length,
                    strContentRange,
                    out baResult,
                    out strErrorInfo);
                return lRet;
            }
        }

        // 包装后的版本，兼容以前的代码
        public static long CopyFragment(
    Stream fileSource,
    long lTotalLength,
    string strContentRange,
    out byte[] baResult,
    out string strErrorInfo)
        {
            long lFileStart = fileSource.Position;
            return CopyFragment(
    fileSource,
    lTotalLength,
    strContentRange,
    lFileStart,
    out baResult,
    out strErrorInfo);
        }

        // 注意，本函数会改变文件当前指针位置，算法有缺陷
        // 将源文件中指定的片断内容复制到目标文件中
        // 当strContentRange的值为""时，表示复制整个文件
        // parameters:
        //      lFileStart  fileSource 中打算操作的片段的起始位置。strContentRange 中的数字，是从这个起始位置以后计算的
        // 返回值：-1 出错 其他 复制的总尺寸
        public static long CopyFragment(
            Stream fileSource,
            long lTotalLength,
            string strContentRange,
            long lFileStart,
            out byte[] baResult,
            out string strErrorInfo)
        {
            long lTotalBytes = 0;
            strErrorInfo = "";
            baResult = null;

            /*
            FileInfo fi = new FileInfo(strSourceFileName);
            if (fi.Length == 0)
                return 0;
            */

            // long lFileStart = fileSource.Position;

            // 表示范围的字符串为空，恰恰表示要包含全部范围
            if (strContentRange == "")
            {
                if (lTotalLength == 0) // 2005/6/24
                {
                    baResult = new byte[0];
                    return 0;
                }

                strContentRange = "0-" + Convert.ToString(lTotalLength - 1);
            }

            // 创建RangeList，便于理解范围字符串
            RangeList rl = new RangeList(strContentRange);

            // 检查strContentRange指出的最大最小边界和源文件中实际情况是否矛盾
            long lMax = rl.max();
            if (lTotalLength <= lMax)
            {
                strErrorInfo = "文件尺寸比范围" + strContentRange + "中定义的最大边界"
                    + Convert.ToString(lMax) + "小...";
                return -1;
            }

            long lMin = rl.min();
            if (lTotalLength <= lMin)
            {
                strErrorInfo = "文件尺寸比范围" + strContentRange + "中定义的最小边界"
                    + Convert.ToString(lMax) + "小...";
                return -1;
            }

            /*
            FileStream fileSource = File.Open(
                strSourceFileName,
                FileMode.Open,
                FileAccess.Read, 
                FileShare.ReadWrite);
            */

            //			int nStart = 0;

            // 循环，复制每个连续片断
            for (int i = 0, nStart = 0; i < rl.Count; i++)
            {
                RangeItem ri = (RangeItem)rl[i];

                // TODO: 这里是性能瓶颈。应该是 SeekOrigin.Current 才好
                // fileSource.Seek(ri.lStart + lFileStart, SeekOrigin.Begin);
                FastSeek(fileSource, ri.lStart + lFileStart);

                baResult = ByteArray.EnsureSize(baResult, nStart + (int)ri.lLength);
                nStart += fileSource.Read(baResult, nStart, (int)ri.lLength);

                lTotalBytes += ri.lLength;
            }

            // fileSource.Close();

            return lTotalBytes;
        }

        // 2017/9/16 修改后版本
        // 将源文件中指定的片断内容复制到目标文件中
        // 当strContentRange的值为""时，表示复制整个文件
        // 返回值：-1 出错 其他 复制的总尺寸
        public static long CopyFragmentNew(
            Stream fileSource,
            long lTotalLength,
            string strContentRange,
            out byte[] baResult,
            out string strErrorInfo)
        {
            long lTotalBytes = 0;
            strErrorInfo = "";
            baResult = null;

            // long lFileStart = fileSource.Position;

            // 表示范围的字符串为空，恰恰表示要包含全部范围
            if (string.IsNullOrEmpty(strContentRange) == true)
            {
                if (lTotalLength == 0)
                {
                    baResult = new byte[0];
                    return 0;
                }

                strContentRange = "0-" + Convert.ToString(lTotalLength - 1);
            }

            // 创建RangeList，便于理解范围字符串
            RangeList rl = new RangeList(strContentRange);

            // 检查strContentRange指出的最大最小边界和源文件中实际情况是否矛盾
            long lMax = rl.max();
            if (lTotalLength <= lMax)
            {
                strErrorInfo = "文件尺寸比范围" + strContentRange + "中定义的最大边界"
                    + Convert.ToString(lMax) + "小...";
                return -1;
            }

            long lMin = rl.min();
            if (lTotalLength <= lMin)
            {
                strErrorInfo = "文件尺寸比范围" + strContentRange + "中定义的最小边界"
                    + Convert.ToString(lMax) + "小...";
                return -1;
            }

            // 循环，复制每个连续片断
            for (int i = 0, nStart = 0; i < rl.Count; i++)
            {
                RangeItem ri = (RangeItem)rl[i];

                FastSeek(fileSource, ri.lStart);
                baResult = ByteArray.EnsureSize(baResult, nStart + (int)ri.lLength);
                nStart += fileSource.Read(baResult, nStart, (int)ri.lLength);

                lTotalBytes += ri.lLength;
            }

            return lTotalBytes;
        }

        public static void FastSeek(Stream stream, long lOffset)
        {
            long delta1 = lOffset - stream.Position;
#if NO
            if (delta1 < 0)
                delta1 = -delta1;
#endif

            if (Math.Abs(delta1) < lOffset)
            {
                stream.Seek(delta1, SeekOrigin.Current);
                Debug.Assert(stream.Position == lOffset, "");
            }
            else
                stream.Seek(lOffset, SeekOrigin.Begin);
        }

    } // end of class RangeList
}
