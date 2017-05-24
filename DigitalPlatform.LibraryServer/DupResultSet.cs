using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    /*
    [Serializable()]
    public class DupLine
    {
        public string Path = "";
        public int Weight = 0;

        public DupLine(string strPath,
            int nWeight)
        {
            this.Path = strPath;
            this.Weight = nWeight;
        }
    }*/

    // 行对象
    public class DupLineItem : Item
    {
        int m_nLength = 0;
        byte[] m_buffer = null;

        public string Path = "";
        public int Weight = 0;
        public int Threshold = 0;

        /*
        public DupLineItem(string strPath,
            int nWeight)
        {
            this.Weight = nWeight;
            this.Path = strPath;

        }
         * */

        public override void BuildBuffer()
        {
            //
            byte[] baWeight = BitConverter.GetBytes((Int32)this.Weight);
            Debug.Assert(baWeight.Length == 4, "");

            //
            byte[] baThreshold = BitConverter.GetBytes((Int32)this.Threshold);
            Debug.Assert(baThreshold.Length == 4, "");

            //
            byte[] baPath = Encoding.UTF8.GetBytes(this.Path);
            int nPathBytes = baPath.Length;

            // 
            byte[] baPathLength = BitConverter.GetBytes((Int32)nPathBytes);
            Debug.Assert(baPathLength.Length == 4, "");

            this.Length = 4/*weight*/ + 4/*threshold*/ + 4/*length of path content */ + nPathBytes;


            m_buffer = new byte[this.Length];
            Array.Copy(baWeight, m_buffer, baWeight.Length);
            Array.Copy(baThreshold, 0, m_buffer, 4, 4);
            Array.Copy(baPathLength, 0, m_buffer, 4 + 4, 4);
            Array.Copy(baPath, 0, m_buffer, 4 + 4 + 4, nPathBytes);
        }

        /*
        public DupLineItem FileLine
        {
            get
            {
                return m_line;
            }
            set
            {
                m_line = value;

                this.m_strLineKey = m_line.Text;
                byte[] baKey = Encoding.UTF8.GetBytes(this.m_strLineKey);
                int nKeyBytes = baKey.Length;

                // 初始化二进制内容
                MemoryStream s = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, m_line);

                this.Length = (int)s.Length + 4 + nKeyBytes;	// 算上了length所占bytes

                m_buffer = new byte[(int)s.Length];
                s.Seek(0, SeekOrigin.Begin);
                s.Read(m_buffer, 0, m_buffer.Length);
                s.Close();
            }
        }
         * */

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

            // 读入Weight
            byte[] weightbuffer = new byte[4];
            stream.Read(weightbuffer, 0, 4);
            this.Weight = BitConverter.ToInt32(weightbuffer, 0);

            // 读入Threshold
            byte[] shresholdbuffer = new byte[4];
            stream.Read(shresholdbuffer, 0, 4);
            this.Threshold = BitConverter.ToInt32(shresholdbuffer, 0);

            // 读入path length
            byte[] lengthbuffer = new byte[4];
            stream.Read(lengthbuffer, 0, 4);
            int nPathLength = BitConverter.ToInt32(lengthbuffer, 0);

            // 读入path content
            if (nPathLength > 0)
            {
                byte[] pathbuffer = new byte[nPathLength];
                stream.Read(pathbuffer, 0, nPathLength);

                this.Path = Encoding.UTF8.GetString(pathbuffer);
            }
            else
                this.Path = "";
        }


        public override void ReadCompareData(Stream stream)
        {
            if (this.Length == 0)
                throw new Exception("length尚未初始化");

            // 读入Weight
            byte[] weightbuffer = new byte[4];
            stream.Read(weightbuffer, 0, 4);
            this.Weight = BitConverter.ToInt32(weightbuffer, 0);

            // 读入Threshold
            byte[] shresholdbuffer = new byte[4];
            stream.Read(shresholdbuffer, 0, 4);
            this.Threshold = BitConverter.ToInt32(shresholdbuffer, 0);

            // 读入path length
            byte[] lengthbuffer = new byte[4];
            stream.Read(lengthbuffer, 0, 4);
            int nPathLength = BitConverter.ToInt32(lengthbuffer, 0);

            // 读入path content
            if (nPathLength > 0)
            {
                byte[] pathbuffer = new byte[nPathLength];
                stream.Read(pathbuffer, 0, nPathLength);

                this.Path = Encoding.UTF8.GetString(pathbuffer);
            }
            else
                this.Path = "";
        }

        public override void WriteData(Stream stream)
        {
            if (m_buffer == null)
                BuildBuffer();

            if (m_buffer == null)
            {
                throw (new Exception("m_buffer尚未初始化"));
            }

            // 写入Length个bytes的内容
            stream.Write(m_buffer, 0, this.Length);
        }

        // 实现IComparable接口的CompareTo()方法,
        // obj: An object to compare with this instance
        // 返回值 A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
        // Less than zero: This instance is less than obj.
        // Zero: This instance is equal to obj.
        // Greater than zero: This instance is greater than obj.
        // 异常: ArgumentException,obj is not the same type as this instance.
        public override int CompareTo(object obj)
        {
            DupLineItem item = (DupLineItem)obj;

            // 小在前
            return String.Compare(this.Path, item.Path);
        }

        // 按照权值排序
        public int CompareWeightTo(object obj)
        {
            DupLineItem item = (DupLineItem)obj;

            int delta = this.Weight - item.Weight;
            if (delta != 0)
                return -1 * delta;    // 大在前

            // 如权值相同，再按照路径排序
            // 小在前
            return String.Compare(this.Path, item.Path);
        }

        // 按照差额排序。所谓差额就是权值和阈值的差额
        public int CompareOverThresholdTo(object obj)
        {
            DupLineItem item = (DupLineItem)obj;

            int over1 = this.Weight - this.Threshold;
            int over2 = item.Weight - item.Threshold;

            int delta = over1 - over2;
            if (delta != 0)
                return -1 * delta;    // 大在前

            // 如差额相同，再按照路径排序
            // 小在前
            return String.Compare(this.Path, item.Path);
        }
    }

    /// <summary>
    /// 用于查重的结果集文件对象
    /// 主要特征是，每个事项都包含一个权值整数字段
    /// </summary>
    public class DupResultSet : ItemFileBase
    {
        // 2017/4/14
        // 辅助记忆是否排序过
        public bool Sorted { get; set; }

        // 排序风格
        public DupResultSetSortStyle SortStyle = DupResultSetSortStyle.Path;

        public DupResultSet()
        {

        }

        public override Item NewItem()
        {
            return new DupLineItem();
        }

        public string Dump()
        {
            return "";
        }

        // 使得可以按照多种风格排序
        public override int Compare(long lPtr1, long lPtr2)
        {
            if (lPtr1 < 0 && lPtr2 < 0)
                return 0;
            else if (lPtr1 >= 0 && lPtr2 < 0)
                return 1;
            else if (lPtr1 < 0 && lPtr2 >= 0)
                return -1;

            DupLineItem item1 = (DupLineItem)GetCompareItemByOffset(lPtr1);
            DupLineItem item2 = (DupLineItem)GetCompareItemByOffset(lPtr2);

            if (this.SortStyle == DupResultSetSortStyle.Path)
                return item1.CompareTo(item2);
            else if (this.SortStyle == DupResultSetSortStyle.Weight)
                return item1.CompareWeightTo(item2);
            else if (this.SortStyle == DupResultSetSortStyle.OverThreshold)
                return item1.CompareOverThresholdTo(item2);
            else
            {
                Debug.Assert(false, "invalid sort style");
                return 0;
            }
        }

        // 功能: 合并两个数组
        // parameters:
        //		strStyle	运算风格 OR , AND , SUB
        //		sourceLeft	源左边结果集
        //		sourceRight	源右边结果集
        //		targetLeft	目标左边结果集
        //		targetMiddle	目标中间结果集
        //		targetRight	目标右边结果集
        //		bOutputDebugInfo	是否输出处理信息
        //		strDebugInfo	处理信息
        // return
        //		-1	出错
        //		0	成功
        public static int Merge(string strStyle,
            DupResultSet sourceLeft,
            DupResultSet sourceRight,
            DupResultSet targetLeft,
            DupResultSet targetMiddle,
            DupResultSet targetRight,
            bool bOutputDebugInfo,
            out string strDebugInfo,
            out string strError)
        {
            strDebugInfo = "";
            strError = "";

            if (sourceLeft.m_streamSmall == null)
            {
                throw new Exception("sourceLeft结果集对象未建索引");
            }

            if (sourceRight.m_streamSmall == null)
            {
                throw new Exception("sourceRight结果集对象未建索引");
            }


            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "strStyle值:" + strStyle + "<br/>";
                strDebugInfo += "sourceLeft结果集:" + sourceLeft.Dump() + "<br/>";
                strDebugInfo += "sourceRight结果集:" + sourceRight.Dump() + "<br/>";
            }

            if (String.Compare(strStyle, "OR", true) == 0)
            {
                if (targetLeft != null || targetRight != null)
                {
                    Exception ex = new Exception("DpResultSetManager::Merge()中是不是参数用错了?当strStyle参数值为\"OR\"时，targetLeft参数和targetRight无效，值应为null");
                    throw (ex);
                }
            }

            DupLineItem dpRecordLeft;
            DupLineItem dpRecordRight;
            int i = 0;
            int j = 0;
            int ret;
            while (true)
            {
                dpRecordLeft = null;
                dpRecordRight = null;
                if (i >= sourceLeft.Count)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "i大于等于sourceLeft的个数，将i改为-1<br/>";
                    }
                    i = -1;
                }
                else if (i != -1)
                {
                    try
                    {
                        dpRecordLeft = (DupLineItem)sourceLeft[i];
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取出sourceLeft集合中第" + Convert.ToString(i) + "个元素，Path为" + dpRecordLeft.Path + "<br/>";
                        }
                    }
                    catch (Exception e)
                    {
                        Exception ex = new Exception("取SourceLeft集合出错：i=" + Convert.ToString(i) + "----Count=" + Convert.ToString(sourceLeft.Count) + ", internel error :" + e.Message + "<br/>");
                        throw (ex);
                    }
                }
                if (j >= sourceRight.Count)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "j大于等于sourceRight的个数，将j改为-1<br/>";
                    }
                    j = -1;
                }
                else if (j != -1)
                {
                    try
                    {
                        dpRecordRight = (DupLineItem)sourceRight[j];
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取出sourceRight集合中第" + Convert.ToString(j) + "个元素，Path为" + dpRecordRight.Path + "<br/>";
                        }
                    }
                    catch
                    {
                        Exception ex = new Exception("j=" + Convert.ToString(j) + "----Count=" + Convert.ToString(sourceLeft.Count) + sourceRight.GetHashCode() + "<br/>");
                        throw (ex);
                    }
                }
                if (i == -1 && j == -1)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "i,j都等于-1跳出<br/>";
                    }
                    break;
                }

                if (dpRecordLeft == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "dpRecordLeft为null，设ret等于1<br/>";
                    }
                    ret = 1;
                }
                else if (dpRecordRight == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "dpRecordRight为null，设ret等于-1<br/>";
                    }
                    ret = -1;
                }
                else
                {
                    ret = dpRecordLeft.CompareTo(dpRecordRight);  //MyCompareTo(oldOneKey); //改CompareTO
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "dpRecordLeft与dpRecordRight均不为null，比较两条记录得到ret等于" + Convert.ToString(ret) + "<br/>";
                    }
                }

                if (String.Compare(strStyle, "OR", true) == 0
                    && targetMiddle != null)
                {
                    if (ret == 0)
                    {
                        // 左右任意取一个就可以，但是要加上权值 2007/7/2
                        dpRecordLeft.Weight += dpRecordRight.Weight;

                        targetMiddle.Add(dpRecordLeft);
                        i++;
                        j++;
                    }
                    else if (ret < 0)
                    {
                        targetMiddle.Add(dpRecordLeft);
                        i++;
                    }
                    else if (ret > 0)
                    {
                        targetMiddle.Add(dpRecordRight);
                        j++;
                    }
                    continue;
                }

                if (ret == 0 && targetMiddle != null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret等于0,加到targetMiddle里面<br/>";
                    }

                    // 左右任意取一个就可以，但是要加上权值 2007/7/2
                    dpRecordLeft.Weight += dpRecordRight.Weight;

                    targetMiddle.Add(dpRecordLeft);
                    i++;
                    j++;
                }

                if (ret < 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret小于0,加到targetLeft里面<br/>";
                    }

                    if (targetLeft != null && dpRecordLeft != null)
                        targetLeft.Add(dpRecordLeft);
                    i++;
                }

                if (ret > 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret大于0,加到targetRight里面<br/>";
                    }

                    if (targetRight != null && dpRecordRight != null)
                        targetRight.Add(dpRecordRight);

                    j++;
                }
            }
            return 0;
        }
    }

    public enum DupResultSetSortStyle
    {
        Path = 0,   // 按照路径排序
        Weight = 1, // 按照权值排序。如果权值相同，则按路径排序
        OverThreshold = 2,  // 按照权值和阈值的差额来排序。如果这个差额相同，则按路径排序
    }
}
