using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.ResultSet
{
    //设计意图:
    //设计DpResultSetManager类，从ArrayList派生的集合，集合的成员为DpResultSet对象
    //即DpResultSet对象的容器
    public class DpResultSetManager : ArrayList
    {
        public string m_strDebugInfo = "";

        //功能: 从集中找到指定名称的DpResultSet
        //myResultSetNameDpResultSet的名称
        //如果找到返回相对的DpResultSet对象，如果没找到，返回null
        public DpResultSet GetResultSet(string myResultSetName)
        {
            DpResultSet foundSet = null;
            foreach (DpResultSet oneResultSet in this)
            {
                if (oneResultSet.Name == myResultSetName)
                    foundSet = oneResultSet;
            }
            return foundSet;
        }


        public const char OR = (char)0x01;
        public const char AND = (char)0x02;
        public const char FROM_LEAD = (char)0x03;
        public const char SPLIT = (char)0x04;

        static List<DpRecord> GetSames(DpResultSet resultset,
            int nStart,
            DpRecord start)
        {
            List<DpRecord> results = new List<DpRecord>();
            results.Add(start);
            for (int i = nStart + 1; i < results.Count; i++) // BUG !!!
            {
                DpRecord record = (DpRecord)resultset[i];
                if (record == null)
                    break;
                int ret = start.CompareTo(record);
                if (ret != 0)
                    break;
                results.Add(record);
            }

            return results;
        }

        static int OutputAND(List<DpRecord> left_sames,
            List<DpRecord> right_sames,
            DpResultSet target)
        {
            int nCount = 0;
            for (int i = 0; i < left_sames.Count; i++)
            {
                DpRecord left = left_sames[i];

                for (int j = 0; j < right_sames.Count; j++)
                {
                    DpRecord right = right_sames[j];

                    DpRecord temp_record = new DpRecord(left.ID);
                    temp_record.BrowseText = left.BrowseText
                        + new string(SPLIT, 1) + new string(AND, 1)
                        + right.BrowseText;
                    target.Add(temp_record);

                    nCount++;
                }
            }

            return nCount;
        }

        // 一边复制一边修改 每8byte 值
        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            int nChunkSize,
            long lDelta)
        {
            Debug.Assert(nChunkSize % 8 == 0, "");
            byte[] bytes = new byte[nChunkSize];
            long lLength = 0;
            while (true)
            {
                int n = streamSource.Read(bytes, 0, nChunkSize);

                if (n != 0)
                {
                    // 对缓冲区内的每个 8 byte 进行整值的增量
                    if ((n % 8) != 0)
                        throw new Exception("复制index文件过程中发现片断长度 " + n.ToString() + " 不是 8 的整倍数");
                    int nCount = n / 8;
                    for (int i = 0; i < nCount; i++)
                    {
                        Int64 v = System.BitConverter.ToInt64(bytes, i * 8);
                        if (v < 0)
                            continue;   // 已删除项
                        Array.Copy(BitConverter.GetBytes((Int64)(v + lDelta)),
                                0,
                                bytes,
                                i * 8,
                                8);
                    }

                    streamTarget.Write(bytes, 0, n);
                }

                if (n <= 0)
                    break;

                lLength += n;
            }

            return lLength;
        }

        static void AddIndexFile(Stream target, Stream source, long lDelta)
        {
            // 将source的目次项一边读入一边写入target的尾部
            if ((target.Length % 8) != 0)
            {
                throw new Exception("复制前目标index文件长度 " + target.Length.ToString() + " 不是 8 的整倍数");
            }
            long lLength = source.Length;
            if ((lLength % 8) != 0)
            {
                throw new Exception("复制前源index文件长度 " + lLength.ToString() + " 不是 8 的整倍数");
            }
            long lCount = lLength / 8;

            source.Seek(0, SeekOrigin.Begin);
            target.Seek(0, SeekOrigin.End);
            DumpStream(source,
                target,
                1024 * 8,
                lDelta);
        }

        // 观察两个结果集是否有交叉的部分
        // 调用前，两个结果集必须是有序的
        // return:
        //      -1  出错
        //      0   没有交叉部分
        //      1   有交叉部分
        public static int IsCross(DpResultSet sourceLeft,
            DpResultSet sourceRight,
            out string strError)
        {
            strError = "";

            if (sourceLeft.Count == 0 || sourceRight.Count == 0)
                return 0;

            if (sourceLeft.m_streamSmall == null)
            {
                throw new Exception("sourceLeft结果集对象未建索引");
            }

            if (sourceRight.m_streamSmall == null)
            {
                throw new Exception("sourceRight结果集对象未建索引");
            }

            if (sourceLeft.Sorted == false)
            {
                strError = "调用IsCross()前sourceLeft尚未排序";
                return -1;
            }
            if (sourceRight.Sorted == false)
            {
                strError = "调用IsCross()前sourceRight尚未排序";
                return -1;
            }

            DpRecord leftMin = (DpRecord)sourceLeft[0];
            DpRecord leftMax = (DpRecord)sourceLeft[sourceLeft.Count - 1];

            // 交换
            if (leftMin.CompareTo(leftMax) > 0)
            {
                DpRecord temp = leftMin;
                leftMin = leftMax;
                leftMax = temp;
            }

            DpRecord rightMin = (DpRecord)sourceRight[0];
            DpRecord rightMax = (DpRecord)sourceRight[sourceRight.Count - 1];

            // 交换
            if (rightMin.CompareTo(rightMax) > 0)
            {
                DpRecord temp = rightMin;
                rightMin = rightMax;
                rightMax = temp;
            }

            // rightMin rightMax
            //    leftMin  leftMax
            if (leftMin.CompareTo(rightMax) <= 0 && rightMin.CompareTo(leftMax) <= 0)
                return 1;

            // leftMin  leftMax
            //    rightMin rightMax
            if (rightMin.CompareTo(leftMax) <= 0 && leftMin.CompareTo(rightMax) <= 0)
                return 1;

            return 0;
        }

        // 把两个结果集简单前后拼接相加
        // 注意：函数执行过程，可能交换 left 和 right。也就是说返回后， left == right
        // TODO: 中途应当可以中断
        // TODO: 如果相加以前是有序的(并且顺序相同)，并且不交叉，可尽量保持相加后也依然有序。有可能把sourceRight返回到sourceLeft中
        public static int AddResults(ref DpResultSet sourceLeft,
            DpResultSet sourceRight,
            out string strError)
        {
            strError = "";

            bool bSorted = false;   // 是否合并后仍能维持排序?
            if (sourceLeft.Sorted == true && sourceRight.Sorted == true
                && sourceLeft.Count > 0 && sourceRight.Count > 0
                && sourceLeft.Asc == sourceRight.Asc)
            {
                DpRecord leftMin = (DpRecord)sourceLeft[0];
                DpRecord leftMax = (DpRecord)sourceLeft[sourceLeft.Count - 1];

                DpRecord rightMin = (DpRecord)sourceRight[0];
                DpRecord rightMax = (DpRecord)sourceRight[sourceRight.Count - 1];

                bool bExchanged = false;
                bool bCross = false;
                if (sourceLeft.Asc == 1)
                {
                    // 升序
                    if ((leftMin.CompareTo(rightMax) <= 0 && rightMin.CompareTo(leftMax) <= 0)
                        || (rightMin.CompareTo(leftMax) <= 0 && leftMin.CompareTo(rightMax) <= 0))
                        bCross = true;
                    else if (leftMin.CompareTo(rightMin) > 0)
                        bExchanged = true;
                }
                else
                {
                    // 降序
                    if ((leftMax.CompareTo(rightMin) <= 0 && rightMax.CompareTo(leftMin) <= 0)
                        || (rightMax.CompareTo(leftMin) <= 0 && leftMax.CompareTo(rightMin) <= 0))
                        bCross = true;
                    else if (leftMax.CompareTo(rightMax) < 0)
                        bExchanged = true;

                }

                if (bExchanged == true)
                {
                    DpResultSet temp = sourceLeft;
                    sourceLeft = sourceRight;
                    sourceRight = temp;
                }

                if (bCross == false)
                    bSorted = true;
                else
                    bSorted = false;
            }


            long lStart = -1;
            // 处理数据部分
            if (sourceLeft.m_bufferBig != null
    && sourceRight.m_bufferBig != null)
            {
                lStart = sourceLeft.m_bufferBig.Length;
                // 复制相加数据部分
                sourceLeft.m_bufferBig = ByteArray.Add(sourceLeft.m_bufferBig, sourceRight.m_bufferBig);
            }
            else
            {
                sourceLeft.WriteToDisk(true, true);
                sourceRight.WriteToDisk(true, true);

                if (sourceLeft.m_streamBig != null
                    && sourceRight.m_streamBig != null)
                {
                    lStart = sourceLeft.m_streamBig.Length;
                    sourceLeft.m_streamBig.Seek(0, SeekOrigin.End);
                    sourceRight.m_streamBig.Seek(0, SeekOrigin.Begin);

                    StreamUtil.DumpStream(sourceRight.m_streamBig,
                        sourceLeft.m_streamBig,
                        false);
                }
                else
                {
                    strError = "两个结果集中至少有一个 m_streamBig 没有打开";
                    return -1;
                }
            }

            // 处理索引部分
            if (sourceLeft.m_bufferSmall != null
                && sourceRight.m_bufferSmall != null)
            {
                if ((sourceLeft.m_bufferSmall.Length % 8) != 0)
                {
                    strError = "复制index缓冲区前发现目标缓冲区长度 " + sourceLeft.m_bufferSmall.Length.ToString() + " 不是 8 的整倍数";
                    return -1;
                }
                int nLength = sourceRight.m_bufferSmall.Length;
                if ((nLength % 8) != 0)
                {
                    strError = "复制index缓冲区前发现源缓冲区长度 " + nLength.ToString() + " 不是 8 的整倍数";
                    return -1;
                }
                // 复制相加索引部分
                sourceLeft.m_bufferSmall = ByteArray.Add(sourceLeft.m_bufferSmall, sourceRight.m_bufferSmall);
                Debug.Assert(sourceLeft.m_bufferSmall.Length == sourceRight.m_bufferSmall.Length + nLength, "");
                // 从lStart位置开始，为每个索引事项增量
                int nCount = nLength / 8;
                for (int i = 0; i < nCount; i++)
                {
                    Int64 v = System.BitConverter.ToInt64(sourceLeft.m_bufferSmall, (int)lStart + i * 8);
                    if (v < 0)
                        continue;   // 已删除项
                    Array.Copy(BitConverter.GetBytes((Int64)(v + lStart)),
                            0,
                            sourceLeft.m_bufferSmall,
                            lStart + i * 8,
                            8);
                }
            }
            else
            {
                sourceLeft.WriteToDisk(true, true);
                sourceRight.WriteToDisk(true, true);

                if (sourceLeft.m_streamSmall != null
                    && sourceRight.m_streamSmall != null)
                {
                    AddIndexFile(sourceLeft.m_streamSmall, sourceRight.m_streamSmall, lStart);
                }
                else
                {
                    sourceLeft.CloseSmallFile();
                    sourceRight.CloseSmallFile();
                }
            }

            sourceLeft.RefreshCount();
            if (bSorted == false)
                sourceLeft.Sorted = false;
            return 0;
        }

        public delegate bool QueryStop(object param);


        // TODO: 尽量用 offset 进行比较，延迟获得 record  2013/2/13

        // TODO: 加入中断机制。为了降低资源消耗，可以每100或者1000个单元检测一次中断
        // 功能: 合并两个数组
        // parameters:
        //		strLogicOper	运算风格 OR , AND , SUB
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
        public static int Merge(LogicOper logicoper,
            DpResultSet sourceLeft,
            DpResultSet sourceRight,
            string strOutputStyle,
            DpResultSet targetLeft,
            DpResultSet targetMiddle,
            DpResultSet targetRight,
            bool bOutputDebugInfo,
            QueryStop query_stop,
            object param,
            out string strDebugInfo,
            out string strError)
        {
            strDebugInfo = "";
            strError = "";

            DateTime start_time = DateTime.Now;

            // 2010/5/11
            if (sourceLeft.Asc != sourceRight.Asc)
            {
                strError = "sourceLeft.Asc 和 sourceRight.Asc 不一致";
                return -1;
            }

            bool bOutputKeyCount = StringUtil.IsInList("keycount", strOutputStyle);
            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

            int nAsc = sourceLeft.Asc;

            if (targetLeft != null)
                targetLeft.Asc = nAsc;
            if (targetMiddle != null)
                targetMiddle.Asc = nAsc;
            if (targetRight != null)
                targetRight.Asc = nAsc;

            // strLogicOper = strLogicOper.ToUpper();

            if (sourceLeft.m_streamSmall == null)
            {
                throw new Exception("sourceLeft结果集对象未建索引");
            }

            if (sourceRight.m_streamSmall == null)
            {
                throw new Exception("sourceRight结果集对象未建索引");
            }

            if (sourceLeft.Sorted == false)
            {
                strError = "调用Merge()前sourceLeft尚未排序";
                return -1;
            }
            if (sourceRight.Sorted == false)
            {
                strError = "调用Merge()前sourceRight尚未排序";
                return -1;
            }

            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "strStyle值:" + logicoper.ToString() + "<br/>";
                strDebugInfo += "sourceLeft结果集:" + sourceLeft.Dump() + "<br/>";
                strDebugInfo += "sourceRight结果集:" + sourceRight.Dump() + "<br/>";
            }

            if (logicoper == LogicOper.OR)
            {
                // OR操作不应使用targetLeft和targetRight参数
                if (targetLeft != null || targetRight != null)
                {
                    Exception ex = new Exception("DpResultSetManager::Merge()中是不是参数用错了?当strStyle参数值为\"OR\"时，targetLeft参数和targetRight无效，值应为null");
                    throw (ex);
                }
            }

            // 2010/10/2 add
            if (logicoper == LogicOper.SUB)
            {
                // SUB操作不应使用targetMiddle和targetRight参数
                if (targetMiddle != null || targetRight != null)
                {
                    Exception ex = new Exception("DpResultSetManager::Merge()中是不是参数用错了?当strStyle参数值为\"SUB\"时，targetMiddle参数和targetRight无效，值应为null");
                    throw (ex);
                }
            }

            DpRecord left = null;
            DpRecord right = null;

            DpRecord old_left = null;
            DpRecord old_right = null;
            int old_ret = 0;

            int m_nLoopCount = 0;

            int i = 0;
            int j = 0;
            int ret = 0;
            while (true)
            {
                if (m_nLoopCount++ % 1000 == 0)
                {
                    Thread.Sleep(1);
                    if (query_stop != null)
                    {
                        if (query_stop(param) == true)
                        {
                            strError = "用户中断";
                            return -1;
                        }
                    }
                }

                old_left = left;
                old_right = right;
                old_ret = ret;

                // 准备left right
                left = null;
                right = null;
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
                        left = (DpRecord)sourceLeft[i];
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取出sourceLeft集合中第" + Convert.ToString(i) + "个元素，ID为" + left.ID + "<br/>";
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
                        right = (DpRecord)sourceRight[j];
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取出sourceRight集合中第" + Convert.ToString(j) + "个元素，ID为" + right.ID + "<br/>";
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

                if (left == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "dpRecordLeft为null，设ret等于1<br/>";
                    }
                    ret = 1;
                }
                else if (right == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "dpRecordRight为null，设ret等于-1<br/>";
                    }
                    ret = -1;
                }
                else
                {
                    ret = nAsc * left.CompareTo(right);  //MyCompareTo(oldOneKey); //改CompareTO
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "dpRecordLeft与dpRecordRight均不为null，比较两条记录得到ret等于" + Convert.ToString(ret) + "<br/>";
                    }
                }



                if (logicoper == LogicOper.OR && targetMiddle != null)
                {
                    if (ret == 0)
                    {
                        if (bOutputKeyCount == false && bOutputKeyID == false)
                        {
                            // id值完全相同的时候，输出左边
                            targetMiddle.Add(left);
                        }
                        else if (bOutputKeyCount == true)
                        {
                            // 2008/11/21 changed
                            // id值完全相同的时候，输出一个复制于左边对象的新对象，其Index应该为原有两个的相加
                            DpRecord temp_record = new DpRecord(left.ID);
                            temp_record.Index = left.Index + right.Index;
                            targetMiddle.Add(temp_record);
                        }
                        else if (bOutputKeyID == true)
                        {
                            /*
                            // 2010/5/17
                            // id值完全相同的时候，输出一个复制于左边对象的新对象，其BrowseText应该为原有两个的串接
                            DpRecord temp_record = new DpRecord(dpRecordLeft.ID);
                            temp_record.BrowseText = dpRecordLeft.BrowseText + new string(OR, 1) + dpRecordRight.BrowseText;
                            targetMiddle.Add(temp_record);
                             * */
                            // 2010/5/17
                            // id值完全相同的时候，如果Keys不相同，则同时输出左边和右边对象
                            if (left.BrowseText != right.BrowseText)
                            {
                                targetMiddle.Add(left);
                                targetMiddle.Add(right);
                            }
                            else
                                targetMiddle.Add(left);
                        }

                        i++;
                        j++;
                    }
                    else if (ret < 0)
                    {
                        targetMiddle.Add(left);
                        i++;
                    }
                    else if (ret > 0)
                    {
                        targetMiddle.Add(right);
                        j++;
                    }
                    continue;
                }

                if (ret == 0)
                {
                    if (targetMiddle != null)
                    {
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "ret等于0,加到targetMiddle里面<br/>";
                        }

                        if (logicoper != LogicOper.AND)
                            targetMiddle.Add(left);
                        else
                        {
                            Debug.Assert(logicoper == LogicOper.AND, "");

                            if (bOutputKeyCount == false && bOutputKeyID == false)
                            {
                                // id值完全相同的时候，输出左边
                                targetMiddle.Add(left);
                            }
                            else if (bOutputKeyCount == true)
                            {
                                strError = "在keycount输出方式下，无法进行结果集之间的AND运算";
                                return -1;
                            }
                            else if (bOutputKeyID == true)
                            {
                                /*
                                // 2010/5/17
                                // id值完全相同的时候，其BrowseText应该为原有两个的串接
                                DpRecord temp_record = new DpRecord(left.ID);
                                temp_record.BrowseText = left.BrowseText
                                    + new string(SPLIT, 1) + new string(AND, 1)
                                    + right.BrowseText;
                                targetMiddle.Add(temp_record);
                                 * */

                                // 左边和右边相同的进入队列
                                List<DpRecord> left_sames = GetSames(sourceLeft,
                                    i,
                                    left);
                                Debug.Assert(left_sames.Count >= 1, "");
                                List<DpRecord> right_sames = GetSames(sourceRight,
                                    j,
                                    right);
                                Debug.Assert(right_sames.Count >= 1, "");
                                OutputAND(left_sames, right_sames, targetMiddle);

                                i += left_sames.Count;
                                j += right_sames.Count;
                                continue;
                            }
                        }

                    }   // endof if (targetMiddle != null)

                    // 2010/10/2 移动到这里
                    i++;
                    j++;
                }

                if (ret < 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret小于0,加到targetLeft里面<br/>";
                    }

                    if (targetLeft != null && left != null)
                        targetLeft.Add(left);
                    i++;
                }

                if (ret > 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret大于0,加到targetRight里面<br/>";
                    }

                    if (targetRight != null && right != null)
                        targetRight.Add(right);

                    j++;
                }
            }

            if (targetLeft != null)
                targetLeft.Sorted = true;
            if (targetMiddle != null)
                targetMiddle.Sorted = true;
            if (targetRight != null)
                targetRight.Sorted = true;

            TimeSpan delta = DateTime.Now - start_time;
            Debug.WriteLine("Merge() " + logicoper.ToString() + " 耗时 " + delta.ToString());

            return 0;
        }


    } //end of class DpResultSetManager


    // 结果集类	
    public class DpResultSet : IEnumerable, IDisposable
    {
        public bool ReadOnly { get; set; }

        public event GetTempFilenameEventHandler GetTempFilename = null;
        public string TempFileDir = ""; // 用于创建和存储临时文件的目录

        int m_nLoopCount = 0;
        public event IdleEventHandler Idle = null;

        object _param = null;
        public object Param
        {
            get
            {
                return _param;
            }
            set
            {
                this._param = value;
            }
        }

        public int Asc = 1; // 1 升序 -1 降序
        public bool Sorted = false; // 是否已经排过序

        //表示结果集的名称
        protected string m_strName;

        public string m_strQuery; //检索式XML字符串
        public int m_nStatus = 0; //0,尚未检索过;1,已经检索过;-1,检索失败

        //表示存放结果集的容器，为DpResultManager类型，这里DpResultSet与DpResultSetManager是包含的关系
        //包含关系:用一个成员字段包含对象实例，就可以实现包含关系，包含类可以完全控制对被包含类的成员的访问，
        public DpResultSetManager m_container;

        //大文件，大流
        public string m_strBigFileName = "";
        internal Stream m_streamBig = null;
        static int nBigBufferSize = 4 * 1024 * 1024;
        internal byte[] m_bufferBig = null;

        //小文件，小流
        public string m_strSmallFileName = "";
        public Stream m_streamSmall = null;
        static int nSmallBufferSize = 4096 * 100;
        internal byte[] m_bufferSmall = null;

        public long m_count = 0;

        bool bDirty = false; //初始值false,表示干净

        public DateTime CreateTime = DateTime.Now;
        public DateTime LastUsedTime = DateTime.Now;

        public void Touch()
        {
            this.LastUsedTime = DateTime.Now;
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);

#if NO
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
#endif
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // release managed resources if any
                }

                // release unmanaged resource
                this.Close();

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            disposed = true;
        }

#if NO
        ~DpResultSet()
        {
            Dispose(false);
        }
#endif

#if NO
        ~DpResultSet()
        {
            Close();
        }
#endif

        //构造函数，初始化m_strName和m_container
        //strName传入的结果集名字字符串
        //objResultSetManager传入的容器对象
        public DpResultSet(string strName,
            DpResultSetManager objResultSetManager)
        {
            m_strName = strName;
            m_container = objResultSetManager;
            Open(false);  //不建索引
        }

        public DpResultSet(bool bOpen,
            bool bCreateIndex)
        {
            if (bOpen == true)
            {
                Open(bCreateIndex);
            }
        }

        public DpResultSet(bool bCreateIndex)
        {
            Open(bCreateIndex);
        }


        public DpResultSet()
        {
            Open(false);
        }

        public DpResultSet(Delegate_getTempFileName procGetTempFileName)
        {
            Open(false, procGetTempFileName);
        }

        //对应m_strName，表示结果集名称，提供给外部代码使用
        public string Name
        {
            get
            {
                return m_strName;
            }
        }

        // 确保创建了索引
        public void EnsureCreateIndex()
        {
            if (this.m_streamSmall == null)
            {
                DateTime start_time = DateTime.Now;
                this.CreateSmallFile();
                TimeSpan delta = DateTime.Now - start_time;
                Debug.WriteLine("EnsureCreateIndex() 耗时 " + delta.ToString());
            }
            else
            {
                // Debug时可以校验一下index文件尺寸和Count的关系是否正确
            }
        }

        /// <summary>
        /// 结果集是否已经被关闭
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return this.m_streamBig == null;
            }
        }

        //功能:拷贝一个结集集到自己
        public int Copy(DpResultSet sourceResultSet)
        {
            // 如果就是自己
            if (sourceResultSet == this)
                return 0;

            this.Clear();

            if (sourceResultSet.Count == 0)
                goto END1;
            /*
            foreach (DpRecord record in sourceResultSet)
            {
                this.Add(record);
            }
             * */
            // 2007/1/1
            bool bFirst = true;
            long lPos = 0;
            DpRecord record = null;
            for (; ; )
            {
                if (bFirst == true)
                {
                    record = sourceResultSet.GetFirstRecord(
                        0,
                        false,
                        out lPos);
                }
                else
                {
                    // 取元素比[]操作速度快
                    record = sourceResultSet.GetNextRecord(
                        ref lPos);
                }

                bFirst = false;

                if (record == null)
                    break;

                this.Add(record);
            }

        END1:
            this.Sorted = sourceResultSet.Sorted;
            return 0;
        }

        //清空
        public void Clear()
        {
            if (m_streamBig != null && m_streamBig.Length > 0)
                m_streamBig.SetLength(0);
            if (m_streamSmall != null && m_streamSmall.Length > 0)
                m_streamSmall.SetLength(0);
            m_count = 0;

            // 2011/1/7 add
            this.m_bufferSmall = null;
            this.m_bufferBig = null;

            this.Sorted = false;
        }

        //记录数
        public long Count
        {
            get
            {
                return m_count;
            }
        }

        // 2010/10/11
        // 创建对象，为写入作准备
        public void Create(string strBigFilename,
            string strSmallFilename)
        {
            CloseAndDeleteBigFile("");
            CloseAndDeleteSmallFile("");

            File.Delete(strBigFilename);
            File.Delete(strSmallFilename);

            this.m_strBigFileName = strBigFilename;
            this.m_strSmallFileName = strSmallFilename;

            if (String.IsNullOrEmpty(this.m_strSmallFileName) == false)
                Open(true);
            else
                Open(false);
        }

        // 获得临时文件名
        string DoGetTempFilename()
        {
            if (string.IsNullOrEmpty(this.TempFileDir) == false)
            {
                Debug.Assert(string.IsNullOrEmpty(this.TempFileDir) == false, "");
                while (true)
                {
                    string strFilename = Path.Combine(this.TempFileDir, Guid.NewGuid().ToString());
                    if (File.Exists(strFilename) == false)
                    {
                        using (FileStream s = File.Create(strFilename))
                        {
                        }
                        return strFilename;
                    }
                }
            }

            if (this.GetTempFilename == null)
                return Path.GetTempFileName();

            GetTempFilenameEventArgs e = new GetTempFilenameEventArgs();
            this.GetTempFilename(this, e);
            if (String.IsNullOrEmpty(e.TempFilename) == true)
            {
                Debug.Assert(false, "虽然接管了事件，但是没有实质性动作");
                return Path.GetTempFileName();
            }

            Debug.Assert(string.IsNullOrEmpty(e.TempFilename) == false, "");
            return e.TempFilename;
        }

        public delegate string Delegate_getTempFileName();

        // 
        public void Open(bool bCreateIndex,
            Delegate_getTempFileName procGetTempFileName = null)
        {
            if (m_streamBig == null)
            {
                if (m_strBigFileName == "")
                {
                    // 优先使用临时的函数 创建临时文件
                    if (procGetTempFileName != null)
                        m_strBigFileName = procGetTempFileName();

                    if (string.IsNullOrEmpty(m_strBigFileName) == true)
                        m_strBigFileName = DoGetTempFilename();
                }
                this.m_streamBig = File.Open(m_strBigFileName,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);   // 2010/10/11 changed
            }
            if (bCreateIndex == true)
            {
                CreateIndex();
            }
        }

        //创建索引
        public void CreateIndex()
        {
            if (m_streamSmall != null)
            {
                m_streamSmall.Close();
                m_streamSmall = null;
                if (m_strSmallFileName != "")
                {
                    File.Delete(m_strSmallFileName);
                    m_strSmallFileName = "";
                }
            }
            if (m_streamSmall == null)
            {
                if (m_strSmallFileName == "")
                    m_strSmallFileName = DoGetTempFilename();

                m_streamSmall = File.Open(m_strSmallFileName,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);   // 2010/10/11 changed
            }
        }

        public void Flush()
        {
            if (m_streamBig != null)
            {
                if (string.IsNullOrEmpty(m_strBigFileName) == false)
                {
                    // 重新打开
                    m_streamBig.Close();
                    m_streamBig = File.Open(m_strBigFileName,
                        FileMode.Open,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite);
                }
                else
                {
                    // 刷缓冲区
                    m_streamBig.Flush();
                }
            }

            if (m_streamSmall != null)
            {
                if (string.IsNullOrEmpty(m_strSmallFileName) == false)
                {
                    // 重新打开
                    m_streamSmall.Close();
                    m_streamSmall = File.Open(m_strSmallFileName,
                       FileMode.Open,
                       FileAccess.ReadWrite,
                       FileShare.ReadWrite);
                }
                else
                {
                    // 刷缓冲区
                    m_streamSmall.Flush();
                }
            }
        }

        // 2016/5/13 编写，尚未测试
        // 克隆出一个新对象
        public DpResultSet Clone()
        {
            string filename_big = "";
            string filename_small = "";
            try
            {
                // 和原先文件在相同子目录创建新文件
                if (string.IsNullOrEmpty(this.m_strBigFileName) == false)
                {
                    filename_big = Path.Combine(Path.GetDirectoryName(this.m_strBigFileName), Guid.NewGuid().ToString());
                    File.Copy(this.m_strBigFileName, filename_big, false);
                }

                if (string.IsNullOrEmpty(this.m_strSmallFileName) == false
                    && string.IsNullOrEmpty(this.m_strBigFileName) == false)
                {
                    filename_small = Path.Combine(Path.GetDirectoryName(this.m_strSmallFileName), Guid.NewGuid().ToString());
                    File.Copy(this.m_strSmallFileName, filename_small, false);
                }

                DpResultSet result = new DpResultSet();
                if (string.IsNullOrEmpty(filename_big) == false
                    && string.IsNullOrEmpty(filename_small) == false)
                    result.Attach(filename_big, filename_small);
                else if (string.IsNullOrEmpty(filename_big) == false)
                    result.Attach(filename_big);
                return result;
            }
            catch (Exception)
            {
                if (string.IsNullOrEmpty(filename_big) == false)
                    File.Delete(filename_big);
                if (string.IsNullOrEmpty(filename_small) == false)
                    File.Delete(filename_small);
                throw;
            }
        }

        public void Close()
        {
#if NO // testing
            if (this.ReadOnly)
                throw new Exception("readonly close triggered");
#endif

            if (m_streamBig != null)
            {
                m_streamBig.Close();
                m_streamBig = null;
            }

            if (string.IsNullOrEmpty(m_strBigFileName) == false)
            {
                try
                {
                    File.Delete(m_strBigFileName);
                }
                catch
                {
                }
                m_strBigFileName = "";
            }

            if (m_streamSmall != null)
            {
                m_streamSmall.Close();
                m_streamSmall = null;
            }

            if (string.IsNullOrEmpty(m_strSmallFileName) == false)
            {
                try
                {
                    File.Delete(m_strSmallFileName);
                }
                catch
                {
                }
                m_strSmallFileName = "";
            }
        }

        public void CloseBigFile()
        {
            if (m_streamBig != null)
            {
                m_streamBig.Close();
                m_streamBig = null;
            }
            m_strBigFileName = "";
        }

        // parameters:
        //      strExcludeFileName  如果和这个文件名相同，则不要删除它
        public void CloseAndDeleteBigFile(string strExcludeFileName)
        {
            if (m_streamBig != null)
            {
                m_streamBig.Close();
                m_streamBig = null;
            }
            if (m_strBigFileName != "" && m_strSmallFileName != strExcludeFileName)
                File.Delete(m_strBigFileName);
            m_strBigFileName = "";
        }

        // 2010/10/11
        public void CloseSmallFile()
        {
            if (m_streamSmall != null)
            {
                m_streamSmall.Close();
                m_streamSmall = null;
            }
        }

        // parameters:
        //      strExcludeFileName  如果和这个文件名相同，则不要删除它
        public void CloseAndDeleteSmallFile(string strExcludeFileName)
        {
            if (m_streamSmall != null)
            {
                m_streamSmall.Close();
                m_streamSmall = null;
            }
            if (string.IsNullOrEmpty(m_strSmallFileName) == false
                && m_strSmallFileName != strExcludeFileName)
                File.Delete(m_strSmallFileName);
            // m_strBigFileName = ""; BUG!!!
            this.m_strSmallFileName = "";   // 2010/10/11
        }

        // 根据小文件的尺寸获得事项数
        public static long GetCount(string strSmallFilename)
        {
            try
            {
                FileInfo fi = new FileInfo(strSmallFilename);
                return fi.Length / 8;
            }
            catch
            {
                return -1;
            }
        }

        // 2010/10/11
        // 把文件挂接到结果集上
        // parameters:
        //      strBigFileName 大文件名称
        public void Attach(string strBigFileName,
            string strSmallFileName)
        {
            if (string.IsNullOrEmpty(strBigFileName))
                throw new ArgumentException("strBigFileName 参数值不应为空", "strBigFileName");
            if (string.IsNullOrEmpty(strSmallFileName))
                throw new ArgumentException("strSmallFileName 参数值不应为空", "strSmallFileName");

            CloseAndDeleteBigFile(strBigFileName);

            m_strBigFileName = strBigFileName;
            m_streamBig = File.Open(m_strBigFileName,
                FileMode.Open,
                FileAccess.ReadWrite,
                    FileShare.ReadWrite);

            CloseAndDeleteSmallFile(strSmallFileName);

            if (String.IsNullOrEmpty(strSmallFileName) == false)
            {
                this.m_strSmallFileName = strSmallFileName;

                m_streamSmall = File.Open(m_strSmallFileName,
                    FileMode.Open,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);

                this.m_count = m_streamSmall.Length / 8;  //m_count;
            }
            else
            {
                m_count = GetCount();
            }
        }

        // 把某个文件挂接到结果集上
        // parameters:
        //      strFileName 大文件名称
        public void Attach(string strFileName)
        {
            if (string.IsNullOrEmpty(strFileName))
                throw new ArgumentException("strFileName 参数值不应为空", "strFileName");

            CloseAndDeleteBigFile(strFileName);

            m_strBigFileName = strFileName;
            m_streamBig = File.Open(m_strBigFileName,
                FileMode.OpenOrCreate,  // ???
                FileAccess.ReadWrite,
                    FileShare.ReadWrite);   // 2010/10/11 changed

            m_count = GetCount();
        }

        public void RefreshCount()
        {
            if (this.m_streamSmall != null)
                this.m_count = m_streamSmall.Length / 8;
            else
                this.m_count = GetCount();
        }

        // 根据数据文件得到记录数
        public long GetCount()
        {
            if (m_streamBig.Position != 0)  // 2012/2/15 ***
                m_streamBig.Seek(0, SeekOrigin.Begin);

            int i = 0;
            long nLength;

            while (true)
            {
                //长度字节数组
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)   //表示文件到尾
                    break;

                nLength = System.BitConverter.ToInt32(bufferLength, 0);
                if (nLength < 0)  //删除项
                {
                    nLength = (int)GetRealValue(nLength);
                    goto CONTINUE;
                }
                i++;

            CONTINUE:
                m_streamBig.Seek(nLength, SeekOrigin.Current);
            }
            return i;
        }

        // 2010/10/11
        // 将数据文件和对象脱钩
        // parameters:
        // return:
        //	数据文件名
        public void Detach(out string strDataFileName,
            out string strIndexFileName)
        {
            strDataFileName = m_strBigFileName;
            CloseBigFile();

            m_strBigFileName = "";	// 避免析构函数去删除

            strIndexFileName = this.m_strSmallFileName;
            CloseSmallFile();

            this.m_strSmallFileName = "";	// 避免析构函数去删除
        }

        // Detach()
        public string Detach()
        {
            string strFileName = m_strBigFileName;
            CloseBigFile();
            CloseAndDeleteSmallFile("");
            return strFileName;
        }

        // 读入内存，为排序作准备
        void ReadToMemory()
        {
            if (this.m_streamBig != null &&
                this.m_streamBig.Length <= nBigBufferSize)
            {
                m_bufferBig = new byte[this.m_streamBig.Length];
                if (m_streamBig.Position != 0)  // 2012/2/15 ***
                    m_streamBig.Seek(0, SeekOrigin.Begin);
                m_streamBig.Read(m_bufferBig, 0, m_bufferBig.Length);
            }

            if (this.m_streamSmall != null &&
                this.m_streamSmall.Length <= nSmallBufferSize)
            {
                m_bufferSmall = new byte[this.m_streamSmall.Length];
                if (m_streamSmall.Position != 0)  // 2012/2/15 ***
                    m_streamSmall.Seek(0, SeekOrigin.Begin);
                m_streamSmall.Read(m_bufferSmall, 0, m_bufferSmall.Length);
            }
        }

        // 写回文件，为排序收尾
        internal void WriteToDisk(bool bWriteSmall,
            bool bWriteBig)
        {
            if (bWriteBig == true &&
                this.m_streamBig != null &&
                this.m_bufferBig != null)
            {
                if (m_streamBig.Position != 0)  // 2012/2/15 ***
                    m_streamBig.Seek(0, SeekOrigin.Begin);
                m_streamBig.Write(m_bufferBig, 0, m_bufferBig.Length);
            }

            if (bWriteSmall == true &&
                this.m_streamSmall != null &&
                this.m_bufferSmall != null)
            {
                if (m_streamSmall.Position != 0)  // 2012/2/15 ***
                    m_streamSmall.Seek(0, SeekOrigin.Begin);
                m_streamSmall.Write(m_bufferSmall, 0, m_bufferSmall.Length);
            }
        }

        //得到真正的位置或长度
        public long GetRealValue(long lPositionOrLength)
        {
            if (lPositionOrLength < 0)
            {
                lPositionOrLength = -lPositionOrLength;
                lPositionOrLength--;
            }
            return lPositionOrLength;
        }

        //得到删除标记使用的位置或长度
        public long GetDeletedValue(long lPositionOrLength)
        {
            if (lPositionOrLength >= 0)
            {
                lPositionOrLength++;
                lPositionOrLength = -lPositionOrLength;
            }
            return lPositionOrLength;
        }


        public long GetPhysicalCount()
        {
            Debug.Assert(this.m_streamSmall != null, "");

            return this.m_streamSmall.Length / 8;
        }

        // 调RemoveDup()之前，须先排序
        // 中途会触发Idle事件
        // 2013/2/13 优化
        public void RemoveDup()
        {
            if (this.Sorted == false)
                throw new Exception("调RemoveDup()之前，须先排序");

            if (this.Count <= 1)
                return;

            long physicalCount = GetPhysicalCount();

            m_nLoopCount = 0;

            long prevOffset = -1;

            for (int i = 0; i < physicalCount; i++)
            {
                if (m_nLoopCount++ % 1000 == 0)
                {
                    Thread.Sleep(1);
                    if (this.Idle != null)
                    {
                        IdleEventArgs e = new IdleEventArgs();
                        this.Idle(this, e);
                    }
                }

                long curOffset = GetBigOffsetBySmall(i);
                if (curOffset < 0)
                    continue;

                if (prevOffset != -1)
                {
                    if (Compare(curOffset, prevOffset) == 0)
                    {
                        RemoveAtPhysical(i);
                        m_count--;
                        bDirty = true;
                    }
                }

                prevOffset = curOffset;
            }

            // 为了以后快速访问，把索引文件中的删除标记的记录压缩掉
            this.CompressIndex();
        }

        //标记删除一条记录
        public void RemoveAtPhysical(int nIndex)
        {
            //以乘8的方式从小文件中得到大文件的偏移量
            long lBigOffset = GetBigOffsetBySmall(nIndex);
            lBigOffset = GetDeletedValue(lBigOffset);

            byte[] bufferBigOffset = new byte[8];
            bufferBigOffset = BitConverter.GetBytes((long)lBigOffset);

            if (m_streamSmall.Position != nIndex * 8)  // 2012/2/15 ***
                m_streamSmall.Seek(nIndex * 8, SeekOrigin.Begin);
            Debug.Assert(bufferBigOffset.Length == 8, "");
            m_streamSmall.Write(bufferBigOffset, 0, 8);
        }

#if NO
        //标记删除一条记录
        public void RemoveAt(int nIndex)
        {
            if (nIndex < 0 || nIndex >= m_count)
            {
                throw (new Exception("下标 " + Convert.ToString(nIndex) + " 越界(Count=" + Convert.ToString(m_count) + ")"));
            }

            int nRet = RemoveAtA(nIndex);
            if (nRet == -1)
            {
                throw (new Exception("设删除标记失败"));
            }


            //总记录数减一，bDirty设为true;
            m_count--;
            bDirty = true;
        }
#endif

        public int RemoveAtA(int nIndex)
        {
            int nRet = -1;
            if (m_streamSmall != null) //有小文件时
            {
                nRet = RemoveAtS(nIndex);
            }
            else  //小文件不存在时， 从大文件中删除
            {
                nRet = RemoveAtB(nIndex);
            }
            return nRet;
        }

        //定位小流
        public long LocateS(int nIndex)
        {
            long lPositionS = 0;
            if (bDirty == false)
            {
                lPositionS = nIndex * 8;
                if (lPositionS >= m_streamSmall.Length || nIndex < 0)
                {
                    throw (new Exception("下标越界..."));
                }
                if (m_streamSmall.Position != lPositionS)  // 2012/2/15 ***
                    m_streamSmall.Seek(lPositionS, SeekOrigin.Begin);
                return lPositionS;
            }
            else
            {
                if (m_streamSmall.Position != 0)  // 2012/2/15 ***
                    m_streamSmall.Seek(0, SeekOrigin.Begin);
                long lBigOffset;
                int i = 0;
                while (true)
                {
                    //读8个字节，得到位置
                    byte[] bufferBigOffset = new byte[8];
                    int n = m_streamSmall.Read(bufferBigOffset, 0, 8);
                    if (n < 8)   //表示文件到尾
                        break;
                    lBigOffset = System.BitConverter.ToInt64(bufferBigOffset, 0);

                    //为负数时跳过
                    if (lBigOffset < 0)
                    {
                        goto CONTINUE;
                    }

                    //表示按序号找到
                    if (i == nIndex)
                    {
                        if (m_streamSmall.Position != lPositionS)  // 2012/2/15 ***
                            m_streamSmall.Seek(lPositionS, SeekOrigin.Begin);
                        return lPositionS;
                    }
                    i++;

                CONTINUE:
                    lPositionS += 8;
                }
            }
            return -1;
        }

        //从小文件中删除
        public int RemoveAtS(int nIndex)
        {
            int nRet;

            //lBigOffset表示大文件的编移量，-1表示错误
            long lBigOffset = GetBigOffsetS(nIndex, false);
            if (lBigOffset == -1)
                return -1;

            lBigOffset = GetDeletedValue(lBigOffset);

            byte[] bufferBigOffset = new byte[8];
            bufferBigOffset = BitConverter.GetBytes((long)lBigOffset);

            nRet = (int)LocateS(nIndex);
            if (nRet == -1)
                return -1;
            Debug.Assert(bufferBigOffset.Length == 8, "");
            m_streamSmall.Write(bufferBigOffset, 0, 8);

            return 0;
        }

        //从大文件中删除
        public int RemoveAtB(int nIndex)
        {
            //得到大文件偏移量
            long lBigOffset = GetBigOffsetB(nIndex, false);
            if (lBigOffset == -1)
                return -1;

            if (lBigOffset >= m_streamBig.Length)
            {
                throw (new Exception("内部错误，位置大于总长度"));
                //return null;
            }

            if (m_streamBig.Position != lBigOffset)  // 2012/2/15 ***
                m_streamBig.Seek(lBigOffset, SeekOrigin.Begin);
            //长度字节数组
            byte[] bufferLength = new byte[4];
            int n = m_streamBig.Read(bufferLength, 0, 4);
            if (n < 4)   //表示文件到尾
            {
                throw (new Exception("内部错误:Read error"));
                //return null;
            }

            int nLength = System.BitConverter.ToInt32(bufferLength, 0);
            nLength = (int)GetDeletedValue(nLength);

            bufferLength = BitConverter.GetBytes((Int32)nLength);

            m_streamBig.Seek(-4, SeekOrigin.Current);

            Debug.Assert(bufferLength.Length == 4, "");
            m_streamBig.Write(bufferLength, 0, 4);

            return 0;
        }

        //自动返回大文件的编移量,小文件存在时，从小文件得到，不存在时，从大文件得到
        //bContainDeleted等于false，忽略已删除的记录，为true,不忽略
        //返回值
        //>=0:正常
        //-1:当bContainDeleted为false时:表示出错的情况，true时表示正常的负值
        public long GetBigOffsetA(long nIndex, bool bContainDeleted)
        {
            if (m_streamSmall != null)
            {
                return GetBigOffsetS(nIndex, bContainDeleted);
            }
            else
            {
                return GetBigOffsetB(nIndex, bContainDeleted);
            }
        }

        //根据小文件返回大文件的偏移量
        //返回值为大文件的长度
        //当bContainDeleted为false时-1:表示出错的情况，true时表示正常的负值
        public long GetBigOffsetS(long nIndex, bool bContainDeleted)
        {
            if (m_streamSmall == null)
            {
                throw (new Exception("小文件为null,GetBigOffsetAndSmallOffset()函数设计目的，只在小文件中查找。"));
            }

            long lBigOffset = 0;
            //干净
            if (bDirty == false)
            {
                if (nIndex * 8 >= m_streamSmall.Length || nIndex < 0)
                {
                    throw (new Exception("nIndex=" + Convert.ToString(nIndex) + "  m_streamSmall.Length=" + Convert.ToString(m_streamSmall.Length) + " 下标越界"));
                }
                //修改位置为负数
                lBigOffset = GetBigOffsetBySmall(nIndex);
                return lBigOffset;
            }
            else
            {
                if (m_streamSmall.Position != 0)  // 2012/2/15 ***
                    m_streamSmall.Seek(0, SeekOrigin.Begin);
                int i = 0;
                while (true)
                {
                    //读8个字节，得到位置
                    byte[] bufferBigOffset = new byte[8];
                    int n = m_streamSmall.Read(bufferBigOffset, 0, 8);
                    if (n < 8)   //表示文件到尾
                        break;
                    lBigOffset = System.BitConverter.ToInt32(bufferBigOffset, 0);

                    if (bContainDeleted == false)
                    {
                        //为负数时跳过
                        if (lBigOffset < 0)
                        {
                            continue;
                        }
                    }
                    //表示按序号找到
                    if (i == nIndex)
                    {
                        return lBigOffset;
                    }
                    i++;
                }
            }
            return -1;
        }

        // 从大文件中得到下一条记录的地址，不算被打删除标记的记录
        public long GetNextOffsetOfBigFile(long lPos)
        {
            if (m_streamBig.Position != lPos)  // 2012/2/15 ***
                m_streamBig.Seek(lPos, SeekOrigin.Begin);
            long lOffset = lPos;

            int nLength;
            while (true)
            {
                //读4个字节，得到长度
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)   //表示文件到尾
                    break;
                nLength = System.BitConverter.ToInt32(bufferLength, 0);

                // 表示已打删除标记的记录
                if (nLength < 0)
                {
                    //转换为实际的长度，再seek
                    long lTemp = GetRealValue(nLength);

                    m_streamBig.Seek(lTemp, SeekOrigin.Current);

                    lOffset += (4 + lTemp);
                    continue;
                }
                else
                {
                    return lOffset;
                }
            }

            return -1;
        }

        //根据大文件返回大文件的偏移量
        //返回值为大文件的长度
        //当bContainDeleted为false时-1:表示出错的情况，true时表示正常的负值
        public long GetBigOffsetB(long nIndex, bool bContainDeleted)
        {
            if (m_streamBig.Position != 0)  // 2012/2/15 ***
                m_streamBig.Seek(0, SeekOrigin.Begin);
            long lBigOffset = 0;

            int nLength;
            int i = 0;
            while (true)
            {
                //读4个字节，得到长度
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)   //表示文件到尾
                    break;
                nLength = System.BitConverter.ToInt32(bufferLength, 0);

                if (bContainDeleted == false)
                {
                    if (nLength < 0)
                    {
                        //转换为实际的长度，再seek
                        long lTemp = GetRealValue(nLength);
                        m_streamBig.Seek(lTemp, SeekOrigin.Current);

                        lBigOffset += (4 + lTemp);
                        continue;
                    }
                }

                if (i == nIndex)
                {
                    return lBigOffset;
                }
                else
                {
                    m_streamBig.Seek(nLength, SeekOrigin.Current);
                }

                lBigOffset += (4 + nLength);

                i++;
            }

            return -1;
        }

        //通过this[i]找记录
        public DpRecord this[long nIndex]
        {
            get
            {
                return GetRecord(nIndex, false);
            }
        }

        //record
        //0:结束
        //1:找到
        //-1:出错
        public DpRecord GetRecord(long nIndex,
            bool bContainDeleted)
        {
            DpRecord record = null;
            long lBigOffset;

            //自动返回大文件的编移量,小文件存在时，从小文件得到，不存在时，从大文件得到
            //bContainDeleted等于false，忽略已删除的记录，为true,不忽略
            //返回值
            //>=0:正常
            //-1:当bContainDeleted为false时:表示出错的情况，true时表示正常的负值
            lBigOffset = GetBigOffsetA(nIndex, bContainDeleted);

            //当bContainDeleted为false时即不包含已删除记录时，返回值-1，表示没找到
            if (bContainDeleted == false)
            {
                if (lBigOffset == -1)
                    return null;
            }
            record = GetRecordByOffset(lBigOffset);

            return record;
        }

        // 得到第一个记录,起到定位作用
        // return:
        //		null	文件结束
        //		其他	记录对象
        public DpRecord GetFirstRecord(long nIndex,
            bool bContainDeleted,
            out long lPos)
        {
            lPos = -1;
            DpRecord record = null;
            long lBigOffset;

            //自动返回大文件的编移量,小文件存在时，从小文件得到，不存在时，从大文件得到
            //bContainDeleted等于false，忽略已删除的记录，为true,不忽略
            //返回值
            //>=0:正常
            //-1:当bContainDeleted为false时:表示出错的情况，true时表示正常的负值
            lBigOffset = GetBigOffsetA(nIndex, bContainDeleted);

            //当bContainDeleted为false时即不包含已删除记录时，返回值-1，表示没找到
            if (bContainDeleted == false)
            {
                if (lBigOffset == -1)
                    return null;
            }
            record = GetRecordByOffset(lBigOffset);

            if (this.m_streamSmall != null)
            {
                lPos = nIndex + 1;
            }
            else
            {
                lPos = m_streamBig.Position;	// Seek (0, SeekOrigin.Current);
            }
            Debug.Assert(lPos > 0, "文件指针不正确");
            return record;
        }


        // 顺次得到下一条记录.第一次使用本函数之前必须有一次GetFirstRecord()调用
        // return:
        //		null	文件结束
        //		其他	记录对象
        // lPos当有小文件时，表示的是索引号，当无小文件时，为大文件的偏移量
        public DpRecord GetNextRecord(
            ref long lPos)
        {
            if (lPos < 0)
                return null;	// 可能是前面的GetFirstRecord()调用失败

            if (this.m_streamSmall == null)
            {
                if (lPos >= m_streamBig.Length)
                    return null;	// 结束
            }
            else
            {
                if (lPos >= this.Count)
                    return null;
            }

            DpRecord record = null;

            if (m_streamSmall != null)
            {
                // 当有小文件时，lPos参数表示是索引号
                long lDataPos = GetBigOffsetS(lPos, false);
                record = GetRecordByOffset(lDataPos);
                lPos++;
                return record;
            }

            // 不计算打了删除标记的记录
            {

                //读4个字节，得到长度
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)
                    return null;
                int nLength = System.BitConverter.ToInt32(bufferLength, 0);

                // 表示是删除的记录
                if (nLength < 0)
                {
                    throw new Exception("目前不可能出现这种情况");
                    /*
                    // 不包含被删除的记录
                    lPos = this.GetNextOffsetOfBigFile(lPos);
                    if (lPos == -1)
                        return null;
                        */
                }
            }

            record = GetRecordByOffset(lPos);
            lPos = m_streamBig.Position;	// Seek (0, SeekOrigin.Current);
            Debug.Assert(lPos > 0, "文件指针不正确");
            return record;
        }

        // 特殊版本
        public DpRecord GetRecordByOffsetEx(long lPos)
        {
            DpRecord record = null;

            if (lPos != m_streamBig.Position)
            {
                throw new Exception("error");
            }

            //长度字节数组
            byte[] bufferLength = new byte[4];
            int n = m_streamBig.Read(bufferLength, 0, 4);
            if (n < 4)   //表示文件到尾
            {
                throw (new Exception("内部错误:Read error"));
                //return null;
            }

            int nLength = System.BitConverter.ToInt32(bufferLength, 0);

            string strID = ReadString(ref nLength);

            string strIndex = "-1";
            Debug.Assert(strID != null, "");
            int nPosition = strID.IndexOf(",");
            if (nPosition >= 0)
            {
                strIndex = strID.Substring(nPosition + 1);
                strID = strID.Substring(0, nPosition);
            }

            //声明记录
            record = new DpRecord(strID);
            record.Index = Convert.ToInt32(strIndex);

            if (nLength <= 0)
                return record;

            /*
            record.m_strDom = ReadString(ref nLength);

            if (nLength <= 0)
                return record;
             * */

            record.BrowseText = ReadString(ref nLength);

            return record;
        }

        //根据4字节得到的长度，读出字符串，同时修改总长度
        string ReadStringFromMemory(long lOffset,
            ref int nMaxLength)
        {
            Debug.Assert(this.m_bufferBig != null, "");

            byte[] bufferLength = new byte[4];
            byte[] bufferText;
            int nLength;

            // int n;

            //读出length部分
            Array.Copy(this.m_bufferBig,
    lOffset,
    bufferLength,
    0,
    4);

            nLength = System.BitConverter.ToInt32(bufferLength, 0);
            bufferText = new byte[nLength];

            lOffset += 4;

            /*
            try
            {
             * */
            Array.Copy(this.m_bufferBig,
    lOffset,
    bufferText,
    0,
    nLength);
            /*
            }
            catch (Exception ex)
            {
                int k = 0;
                k++;
            }
             * */

            if (4 + nLength > nMaxLength)
            {
                throw (new Exception("当前小包的长度(4+" + nLength.ToString() + ")超出总限制长度 " + nMaxLength.ToString()));
            }

            nMaxLength = nMaxLength - (4 + nLength);

            return System.Text.Encoding.UTF8.GetString(bufferText);
        }

        //根据4字节得到的长度，读出字符串，同时修改总长度
        string ReadString(ref int nMaxLength)
        {
            byte[] bufferLength = new byte[4];
            byte[] bufferText;
            int nLength;

            int n;

            //读出ID
            n = m_streamBig.Read(bufferLength, 0, 4);
            if (n < 4)
            {
                throw (new Exception("内部错误:读出的长度小于4"));
            }

            nLength = System.BitConverter.ToInt32(bufferLength, 0);
            bufferText = new byte[nLength];

            m_streamBig.Read(bufferText, 0, nLength);

            if (4 + nLength > nMaxLength)
            {
                throw (new Exception("当前小包的长度(4+" + nLength.ToString() + ")超出总限制长度 " + nMaxLength.ToString()));
            }

            nMaxLength = nMaxLength - (4 + nLength);

            return System.Text.Encoding.UTF8.GetString(bufferText);
        }


        //GetRecordByOffset()不论正负数都可以找到记录，调用时，注意，如果不需要得到被删除的记录，自已做判断
        public DpRecord GetRecordByOffset(long lOffset)
        {
            DpRecord record = null;

            if (lOffset < 0)
            {
                lOffset = GetRealValue(lOffset);
            }

            if (lOffset >= m_streamBig.Length)
            {
                throw (new Exception("内部错误，位置大于总长度"));
                //return null;
            }

            // 如果大文件已经在内存
            if (this.m_bufferBig != null)
            {
                //长度字节数组
                byte[] bufferLength = new byte[4];
                /*
                try
                {
                 * */
                Array.Copy(this.m_bufferBig,
                    lOffset,
                    bufferLength,
                    0,
                    4);
                /*
                }
                catch (Exception ex)
                {
                    int i = 0;
                    i++;
                }
                 * */

                int nLength = System.BitConverter.ToInt32(bufferLength, 0);

                lOffset += 4;
                int nOldLength = nLength;
                string strID = ReadStringFromMemory(lOffset, ref nLength);
                int nDelta = nOldLength - nLength;

                string strIndex = "-1";
                int nPosition = strID.LastIndexOf(","); // IndexOf BUG!!!
                if (nPosition >= 0)
                {
                    strIndex = strID.Substring(nPosition + 1);
                    strID = strID.Substring(0, nPosition);
                }

                //声明记录
                record = new DpRecord(strID);

                record.Index = Convert.ToInt32(strIndex);   // 可能抛出异常


                if (nLength <= 0)
                    return record;

                lOffset += nDelta;
                record.BrowseText = ReadStringFromMemory(lOffset,
                    ref nLength);

                return record;
            }
            else
            {
                if (m_streamBig.Position != lOffset)    // 2012/2/15 ***
                    m_streamBig.Seek(lOffset, SeekOrigin.Begin);

                //长度字节数组
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)   //表示文件到尾
                {
                    throw (new Exception("内部错误:Read error"));
                    //return null;
                }

                int nLength = System.BitConverter.ToInt32(bufferLength, 0);


                string strID = ReadString(ref nLength);

                string strIndex = "-1";
                int nPosition = strID.LastIndexOf(","); // IndexOf BUG!!! 速度?
                if (nPosition >= 0)
                {
                    strIndex = strID.Substring(nPosition + 1);
                    strID = strID.Substring(0, nPosition);
                }

                //声明记录
                record = new DpRecord(strID);

                record.Index = Convert.ToInt32(strIndex);   // 可能抛出异常

                if (nLength <= 0)
                    return record;

                /*
                record.m_strDom = ReadString(ref nLength);
                if (nLength <= 0)
                    return record;
                 * */

                record.BrowseText = ReadString(ref nLength);

                return record;
            }
        }


        //用*8的方法算到小文件的位置，包含已删除的记录，并取出大文件的编移量
        public long GetBigOffsetBySmall(long nIndex)
        {
            if (m_streamSmall == null)
            {
                throw (new Exception("m_streamSmall对象为空"));
            }

            if (nIndex * 8 >= m_streamSmall.Length || nIndex < 0)
            {
                throw (new Exception("下标越界"));
            }

            byte[] bufferOffset = new byte[8];

            // 如果小文件已经在内存
            if (this.m_bufferSmall != null)
            {
                Array.Copy(this.m_bufferSmall,
                    nIndex * 8,
                    bufferOffset,
                    0,
                    8);
                return System.BitConverter.ToInt64(bufferOffset, 0);
            }

            if (m_streamSmall.Position != nIndex * 8)    // 2012/2/15 ***
                m_streamSmall.Seek(nIndex * 8, SeekOrigin.Begin);

            int n = m_streamSmall.Read(bufferOffset, 0, 8);
            if (n <= 0)
            {
                throw (new Exception("实际流的长度" + Convert.ToString(m_streamSmall.Length) + "\r\n"
                    + "希望Seek到的位置" + Convert.ToString(nIndex * 8) + "\r\n"
                    + "实际读的长度" + Convert.ToString(n)));
            }
            long lOffset = System.BitConverter.ToInt64(bufferOffset, 0);

            return lOffset;
        }

        public int CreateSmallFile()
        {
            int nLength;

            Debug.Assert(this.m_streamBig != null, "");

            CreateIndex();
            if (m_streamBig.Position != 0)  // 2012/2/15 ***
                m_streamBig.Seek(0, SeekOrigin.Begin);
            m_streamSmall.Seek(0, SeekOrigin.End);

            int i = 0;
            long lPosition = 0;
            int nDeleteCount = 0;
            for (i = 0; ; i++)
            {
                //长度字节数组
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)   //表示文件到尾
                    break;


                nLength = System.BitConverter.ToInt32(bufferLength, 0);
                if (nLength < 0)  //删除项
                {
                    nDeleteCount++;
                    nLength = (int)GetRealValue(nLength);
                    goto CONTINUE;
                }

                byte[] bufferOffset = new byte[8];
                bufferOffset = System.BitConverter.GetBytes((long)lPosition);
                Debug.Assert(bufferOffset.Length == 8, "");

                m_streamSmall.Write(bufferOffset, 0, 8);

            CONTINUE:

                m_streamBig.Seek(nLength, SeekOrigin.Current);
                lPosition += (4 + nLength);
            }

            return 0;
        }

        //排序
        public void Sort()
        {
            if (this.Count <= 1)
            {
                this.Sorted = true;
                Debug.WriteLine("QuickSort() 耗时 0 (优化)");
                return;
            }

            DateTime start_time = DateTime.Now;

            CreateSmallFile();

            TimeSpan delta = DateTime.Now - start_time;
            Debug.WriteLine("CreateSmallFile() 耗时 " + delta.ToString());

            start_time = DateTime.Now;

            ReadToMemory();

            try
            {

                // QuickSort();
                QuickSort1(0, this.Count - 1);
                WriteToDisk(true, false);

                this.Sorted = true;
            }
            finally
            {
                this.m_bufferSmall = null;
                this.m_bufferBig = null;
            }

            delta = DateTime.Now - start_time;
            Debug.WriteLine("QuickSort() 耗时 " + delta.ToString());
        }

        //功能:列出集合中的所有项
        //返回值: 返回集合成员组成的表格字符串
        public string Dump()
        {
            string strTable = "";

            strTable = "<table border='1'><tr><td>id</td></tr>";
            // TODO: foreach速度慢，注意改造为GetFirstRecord/GetNextRecord
            foreach (DpRecord eachRecord in this)
            {
                strTable += "<tr><td>" + eachRecord.ID + "</td></tr>";
            }
            strTable += "</table>";
            return strTable;
        }

        public string DumpAll()
        {
            if (m_streamSmall == null)
            {
                throw (new Exception("小文件不存在"));
            }
            string strResult = "";
            int nm_count = (int)(m_streamSmall.Length / 8);
            for (int i = 0; i < nm_count; i++)
            {
                //strResult += "地址:"+Convert.ToString (GetOffset(i))+"\r\n";
                DpRecord record = GetRecord(i, true);
                strResult += record.ID + "\r\n";
                // strResult += record.m_strDom + "\r\n";
                strResult += record.BrowseText + "\r\n\r\n";
            }
            return strResult;
        }

        public int CompressIndex()
        {
            if (this.m_streamSmall == null)
                return 0;

            if (this.bDirty == false)
                return 0;

            this.Compress(this.m_streamSmall);
            return 1;
        }

        /*
		private int Compress(Stream oStream)
		{
			if (oStream == null)
			{
				return -1;
			}
			long lDeletedStart = 0;  //删除块的起始位置
			long lDeletedEnd = 0;    //删除块的结束位置

			long lDeletedLength = 0;  //删除长度
			bool bDeleted = false;   //是否已出现删除块

			long lUseablePartLength = 0;    //后面正常块的长度
			bool bUseablePart = false;    //是否已出现正常块

			bool bEnd = false;
			long lValue = 0;

			oStream.Seek (0,SeekOrigin.Begin);
			while(true)
			{
				int nRet;
				byte[] bufferValue = new byte[8];
				nRet = oStream.Read(bufferValue,0,8);
				if (nRet != 8 && nRet != 0)  
				{
					throw(new Exception ("内部错误:读到的长度不等于8"));
					//break;
				}
				if (nRet == 0)//表示结束
				{
					if(bUseablePart == false)
						break;

					lValue = -1;
					bEnd = true;
					//break;
				}

				if (bEnd != true)
				{
					lValue = BitConverter.ToInt64(bufferValue,0);
				}

				if (lValue < 0)
				{
					if (bDeleted == true && bUseablePart == true)
					{
						lDeletedEnd = lDeletedStart + lDeletedLength;
						//调MovePart(lDeletedStart,lDeletedEnd,lUseablePartLength)

						StreamUtil.Move(oStream,
							lDeletedEnd,
							lUseablePartLength,
							lDeletedStart);

						//重新定位deleted的起始位置
						lDeletedStart = lUseablePartLength-lDeletedLength+lDeletedEnd;
						lDeletedEnd = lDeletedStart+lDeletedLength;

						oStream.Seek (lDeletedEnd+lUseablePartLength,SeekOrigin.Begin);
					}

					bDeleted = true;
					bUseablePart = false;
					lDeletedLength += 8;  //结束位置加8
				}
				else if (lValue >= 0)
				{
					//当出现过删除块时，又进入新的有用块时，前方的有用块不计，重新计算长度
					//|  useable  | ........ |  userable |
					//|  ........  | useable |
					if (bDeleted == true && bUseablePart == false)
					{
						lUseablePartLength = 0;
					}

					bUseablePart = true;
					lUseablePartLength += 8;
					
					if (bDeleted == false)
					{
						lDeletedStart += 8;  //当不存在删除块时，删除超始位置加8
					}
				}

				if (bEnd == true)
				{
					break;
				}
			}

			//只剩尾部的被删除记录
			if (bDeleted == true && bUseablePart == false)
			{
				//lDeletedEnd = lDeletedStart + lDeletedLength;
				oStream.SetLength(lDeletedStart);
			}

			bDirty = false;
			return 0;
		}
         */

        // 压缩索引
        private int Compress(Stream oStream)
        {
            if (oStream == null)
                return -1;

            int nRet;
            long lRestLength = 0;
            long lDeleted = 0;
            long lCount = 0;

            if (oStream.Position != 0)  // 2012/2/15 ***
                oStream.Seek(0, SeekOrigin.Begin);
            lCount = oStream.Length / 8;
            for (long i = 0; i < lCount; i++)
            {
                byte[] bufferValue = new byte[8];
                nRet = oStream.Read(bufferValue, 0, 8);
                if (nRet != 8 && nRet != 0)
                {
                    throw (new Exception("内部错误:读到的长度不等于8"));
                }

                long lValue = BitConverter.ToInt64(bufferValue, 0);

                if (nRet == 0)//表示结束
                {
                    break;
                }

                if (lValue < 0)
                {
                    // 表示需要删除此项目
                    lRestLength = oStream.Length - oStream.Position;

                    Debug.Assert(oStream.Position - 8 >= 0, "");

                    long lSavePosition = oStream.Position;

                    StreamUtil.Move(oStream,
                        oStream.Position,
                        lRestLength,
                        oStream.Position - 8);

                    if (oStream.Position != lSavePosition - 8)  // 2012/2/15 ***
                        oStream.Seek(lSavePosition - 8, SeekOrigin.Begin);

                    lDeleted++;
                }
            }

            if (lDeleted > 0)
            {
                oStream.SetLength((lCount - lDeleted) * 8);
            }

            bDirty = false;
            return 0;
        }

        //根据4字节得到的长度，读出字符串，同时修改总长度
        void WriteString(string strText, ref int nMaxLength)
        {
            byte[] bufferLength = new byte[4];
            byte[] bufferText;

            bufferText = Encoding.UTF8.GetBytes(strText);

            bufferLength = System.BitConverter.GetBytes((Int32)bufferText.Length);

            Debug.Assert(bufferLength.Length == 4, "");
            m_streamBig.Write(bufferLength, 0, 4);

            m_streamBig.Write(bufferText, 0, bufferText.Length);

            nMaxLength += 4;
            nMaxLength += bufferText.Length;
        }

        public void WriteBuffer(ByteList target,
            byte[] source,
            ref int lLength)
        {
            target.AddRange(source);
            //lLength += source.Length ;
        }

        public void WriteBuffer(ByteList target,
            string strSource,
            ref int lLength)
        {
            byte[] bufferLength = new byte[4];
            byte[] bufferText;
            bufferText = Encoding.UTF8.GetBytes(strSource);

            bufferLength = System.BitConverter.GetBytes((Int32)bufferText.Length);
            Debug.Assert(bufferLength.Length == 4, "");

            target.AddRange(bufferLength);
            target.AddRange(bufferText);

            lLength += (4 + bufferText.Length);
        }

        public void ReplaceBuffer(ByteList aBuffer,
            int lStart,
            byte[] buffer)
        {
            int lEnd = lStart + buffer.Length;
            for (int i = lStart; i < lEnd; i++)
            {
                aBuffer[i] = buffer[i - lStart];
            }
        }

        //确保流的指针放到恰当位置
        public virtual void Add(DpRecord record)  //virtual
        {
            m_streamBig.Seek(0, SeekOrigin.End);

            if (m_streamSmall != null)
            {
                m_streamSmall.Seek(0, SeekOrigin.End);
                long nPosition = m_streamBig.Position;

                byte[] bufferPosition = new byte[8];
                bufferPosition = System.BitConverter.GetBytes((long)nPosition);   // 原来缺乏(long), 是一个bug. 2006/10/1 修改

                Debug.Assert(bufferPosition.Length == 8, "");
                m_streamSmall.Write(bufferPosition, 0, 8);
            }

            ByteList aBuffer = new ByteList(4096);

            int nLength = 0;
            byte[] bufferLength = new byte[4];

            //占位4字节，后面写总长度
            //m_streamBig.Write(bufferLength,0,4);
            WriteBuffer(aBuffer, bufferLength, ref nLength);

            //写ID
            //WriteString(record.ID,ref nLength);
            WriteBuffer(aBuffer, record.ID + "," + Convert.ToString(record.Index), ref nLength);

            //写序号
            //WriteBuffer(aBuffer,"," + Convert.ToString (record.Index ),ref nLength);

            //写m_strDom
            //WriteString(record.m_strDom,ref nLength);

            // 写BrowseText
            // WriteString(record.BrowseText,ref nLength);
            if (String.IsNullOrEmpty(record.BrowseText) == false)
            {
                WriteBuffer(aBuffer, record.BrowseText, ref nLength);
            }

            //写总长度
            bufferLength = System.BitConverter.GetBytes((Int32)nLength);    // 4bytes!
            Debug.Assert(bufferLength.Length == 4, "");
            ReplaceBuffer(aBuffer, 0, bufferLength);

            //m_streamBig.Seek (-(nLength+4),SeekOrigin.Current  );
            byte[] bufferAll = new byte[aBuffer.Count];
            /*
            for (int i = 0; i < aBuffer.Count; i++)
            {
                bufferAll[i] = (byte)aBuffer[i];
            }
             * */
            // 2010/5/17
            aBuffer.CopyTo(bufferAll);

            m_streamBig.Write(bufferAll, 0, bufferAll.Length);
            m_count++;
        }

        // 2011/1/1
        // 在数据文件中直接搜索事项的起点偏移量。
        //	当然，这样速度很慢
        // return:
        //		-1	当bContainDeleted为false时-1表示出错的情况，bContainDeleted为true时表示正常的负值
        long GetDataOffsetFromDataFile(long nIndex, bool bContainDeleted)
        {
            if (m_streamBig.Position != 0)  // 2012/2/15 ***
                m_streamBig.Seek(0, SeekOrigin.Begin);
            long lBigOffset = 0;

            int nLength;
            int i = 0;
            while (true)
            {
                //读4个字节，得到长度
                byte[] bufferLength = new byte[4];
                int n = m_streamBig.Read(bufferLength, 0, 4);
                if (n < 4)   //表示文件到尾
                    break;
                nLength = System.BitConverter.ToInt32(bufferLength, 0);

                if (bContainDeleted == false)
                {
                    if (nLength < 0)
                    {
                        //转换为实际的长度，再seek
                        long lTemp = GetRealValue(nLength);
                        m_streamBig.Seek(lTemp, SeekOrigin.Current);

                        lBigOffset += (4 + lTemp);
                        continue;
                    }
                }

                if (i == nIndex)
                {
                    return lBigOffset;
                }
                else
                {
                    m_streamBig.Seek(nLength, SeekOrigin.Current);
                }

                lBigOffset += (4 + nLength);

                i++;
            }

            return -1;
        }

        // 2011/1/1
        // 自动选择从何处删除
        int RemoveAtAuto(int nIndex)
        {
            int nRet = -1;
            if (m_streamSmall != null) // 有索引文件时
            {
                // nRet = RemoveAtIndex(nIndex);
                nRet = CompressRemoveAtIndex(nIndex, 1);
            }
            else  // 索引文件不存在时， 从数据文件中删除
            {
                nRet = RemoveAtData(nIndex);
            }
            return nRet;
        }

        // 2011/1/1
        //从大文件中删除
        public int RemoveAtData(int nIndex)
        {
            //得到大文件偏移量
            long lBigOffset = GetDataOffsetFromDataFile(nIndex, false);
            if (lBigOffset == -1)
                return -1;

            if (lBigOffset >= m_streamBig.Length)
            {
                throw (new Exception("内部错误，位置大于总长度"));
                //return null;
            }

            if (m_streamBig.Position != lBigOffset)  // 2012/2/15 ***
                m_streamBig.Seek(lBigOffset, SeekOrigin.Begin);
            //长度字节数组
            byte[] bufferLength = new byte[4];
            int n = m_streamBig.Read(bufferLength, 0, 4);
            if (n < 4)   //表示文件到尾
            {
                throw (new Exception("内部错误:Read error"));
                //return null;
            }

            int nLength = System.BitConverter.ToInt32(bufferLength, 0);
            nLength = (int)GetDeletedValue(nLength);

            bufferLength = BitConverter.GetBytes((Int32)nLength);
            m_streamBig.Seek(-4, SeekOrigin.Current);
            Debug.Assert(bufferLength.Length == 4);
            m_streamBig.Write(bufferLength, 0, 4);

            return 0;
        }

        // 2011/1/1
        // 标记删除一条记录
        public void RemoveAt(int nIndex)
        {
            if (nIndex < 0 || nIndex >= m_count)
            {
                throw (new Exception("下标 " + Convert.ToString(nIndex) + " 越界(Count=" + Convert.ToString(m_count) + ")"));
            }
            int nRet = RemoveAtAuto(nIndex);
            if (nRet == -1)
            {
                throw (new Exception("RemoveAtAuto fail"));
            }

            m_count--;
            // bDirty = true;	// 表示已经有标记删除的事项了

        }

        // 2011/1/1
        //标记删除多条记录
        public void RemoveAt(int nIndex,
            int nCount)
        {

            if (nIndex < 0 || nIndex + nCount > m_count)
            {
                throw (new Exception("下标 " + Convert.ToString(nIndex) + " 越界(Count=" + Convert.ToString(m_count) + ")"));
            }

            int nRet = 0;
            if (m_streamSmall != null) // 有索引文件时
            {
                // nRet = RemoveAtIndex(nIndex);
                nRet = CompressRemoveAtIndex(nIndex, nCount);
            }
            else
            {
                throw (new Exception("暂时还没有编写"));
            }

            if (nRet == -1)
            {
                throw (new Exception("RemoveAtAuto fail"));
            }

            m_count -= nCount;
            // bDirty = true;	// 表示已经有标记删除的事项了
        }

        // 2011/1/1
        // 从索引文件中挤压式删除一个事项
        public int CompressRemoveAtIndex(int nIndex,
            int nCount)
        {
            if (m_streamSmall == null)
                throw new Exception("索引文件尚未初始化");

            long lStart = (long)nIndex * 8;
            StreamUtil.Move(m_streamSmall,
                    lStart + 8 * nCount,
                    m_streamSmall.Length - lStart - 8 * nCount,
                    lStart);

            m_streamSmall.SetLength(m_streamSmall.Length - 8 * nCount);

            return 0;
        }

        // 2011/1/1
        // 插入一个事项
        public virtual void Insert(int nIndex,
            DpRecord record)
        {
            // 若不存在索引文件
            if (m_streamSmall == null)
                throw (new Exception("暂不支持无索引文件方式下的插入操作"));

            // 将数据文件指针置于尾部
            m_streamBig.Seek(0,
                SeekOrigin.End);

            // 若存在索引文件
            if (m_streamSmall != null)
            {
                // 插入一个新index条目
                long lStart = (long)nIndex * 8;
                StreamUtil.Move(m_streamSmall,
                    lStart,
                    m_streamSmall.Length - lStart,
                    lStart + 8);

                if (m_streamSmall.Position != lStart)  // 2012/2/15 ***
                    m_streamSmall.Seek(lStart, SeekOrigin.Begin);
                long nPosition = m_streamBig.Position;

                byte[] bufferPosition = new byte[8];
                bufferPosition = System.BitConverter.GetBytes((long)nPosition); // 原来缺乏(long), 是一个bug. 2006/10/1 修改
                Debug.Assert(bufferPosition.Length == 8, "");
                m_streamSmall.Write(bufferPosition, 0, 8);
            }

            /*
                byte[] bufferLength = System.BitConverter.GetBytes((Int32)item.Length);
                Debug.Assert(bufferLength.Length == 4, "");
                m_streamBig.Write(bufferLength, 0, 4);

                item.WriteData(m_streamBig);
                m_count++;             * */

            ByteList aBuffer = new ByteList(4096);

            int nLength = 0;
            byte[] bufferLength = new byte[4];

            //占位4字节，后面写总长度
            //m_streamBig.Write(bufferLength,0,4);
            WriteBuffer(aBuffer, bufferLength, ref nLength);

            //写ID
            //WriteString(record.ID,ref nLength);
            WriteBuffer(aBuffer, record.ID + "," + Convert.ToString(record.Index), ref nLength);

            // 写BrowseText
            // WriteString(record.BrowseText,ref nLength);
            if (String.IsNullOrEmpty(record.BrowseText) == false)
            {
                WriteBuffer(aBuffer, record.BrowseText, ref nLength);
            }

            //写总长度
            bufferLength = System.BitConverter.GetBytes((Int32)nLength);    // 4bytes!
            Debug.Assert(bufferLength.Length == 4, "");
            ReplaceBuffer(aBuffer, 0, bufferLength);

            byte[] bufferAll = new byte[aBuffer.Count];
            aBuffer.CopyTo(bufferAll);

            m_streamBig.Write(bufferAll, 0, bufferAll.Length);
            m_count++;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="left">数组第一个元素索引Index</param>
        /// <param name="right">数组最后一个元素索引Index</param>
        void QuickSort1(
            long left,
            long right)
        {
            //左边索引小于右边，则还未排序完成
            if (left >= right)
                return;

            //取中间的元素作为比较基准，小于他的往左边移，大于他的往右边移
            long i = left - 1;
            long j = right + 1;

            {
                // int middle = numbers[(left + right) / 2];
                long pMiddle = GetBigOffsetBySmall((left + right) / 2);   //GetRowPtr(nMiddle);
                while (true)
                {
                    // while (numbers[++i] < middle && i < right) ;
                    while (i < right)
                    {
                        long pTemp = GetBigOffsetBySmall(++i);
                        int nRet = this.Asc * Compare(pTemp, pMiddle);
                        if (nRet >= 0)
                            break;
                    }


                    // while (numbers[--j] > middle && j > 0) ;
                    while (j > 0)
                    {
                        long pTemp = GetBigOffsetBySmall(--j);
                        int nRet = this.Asc * Compare(pTemp, pMiddle);
                        if (nRet <= 0)
                            break;
                    }


                    if (i >= j)
                        break;
                    // Swap(numbers, i, j);
                    {
                        long pTemp_i = GetBigOffsetBySmall(i);
                        long pTemp_j = GetBigOffsetBySmall(j);
                        SetRowPtr(j, pTemp_i);
                        SetRowPtr(i, pTemp_j);
                    }

                }
            }
            QuickSort1(left, i - 1);
            QuickSort1(j + 1, right);

        }

        void Push(List<long> stack,
            long lStart,
            long lEnd,
            ref int nStackTop)
        {
            if (nStackTop < 0)
            {
                throw (new Exception("nStackTop不能小于0"));
            }
            if (lStart < 0)
            {
                throw (new Exception("nStart不能小于0"));
            }

            if (nStackTop * 2 != stack.Count)
            {
                throw (new Exception("nStackTop*2不等于stack.m_count"));
            }

            stack.Add(lStart);
            stack.Add(lEnd);

            nStackTop++;
        }

        void Pop(List<long> stack,
            ref long lStart,
            ref long lEnd,
            ref int nStackTop)
        {
            if (nStackTop <= 0)
            {
                throw (new Exception("pop以前,nStackTop不能小于等于0"));
            }

            if (nStackTop * 2 != stack.Count)
            {
                throw (new Exception("nStackTop*2不等于stack.m_count"));
            }

            lStart = (long)stack[(nStackTop - 1) * 2];
            lEnd = (long)stack[(nStackTop - 1) * 2 + 1];

            stack.RemoveRange((nStackTop - 1) * 2, 2);

            nStackTop--;
        }

        // 快速排序
        // 如何显示排序进度? 头疼的事情。可否用堆栈深度表示进度?
        // 需要辨别完全排序的部分中，item的数量，将这些部分从总item
        // 数量中去除，就是进度指示的依据。
        // return:
        //  0 succeed
        //  1 interrupted
        public int QuickSort()
        {
            List<long> stack = new List<long>(); // 堆栈
            int nStackTop = 0;
            long nMaxRow = m_streamSmall.Length / 8;  //m_count;
            long k = 0;
            long j = 0;
            long i = 0;

            if (nMaxRow == 0)
                return 0;

            /*
            if (nMaxRow >= 10) // 调试
             nMaxRow = 10;
            */

            Push(stack, 0, nMaxRow - 1, ref nStackTop);
            while (nStackTop > 0)
            {
                Pop(stack, ref k, ref j, ref nStackTop);
                while (k < j)
                {
                    Split(k, j, ref i);
                    Push(stack, i + 1, j, ref nStackTop);
                    j = i - 1;
                }
            }

            return 0;
        }


        void Split(long nStart,
            long nEnd,
            ref long nSplitPos)
        {
            // 取得中项
            long pStart = 0;
            long pEnd = 0;
            long pMiddle = 0;
            long pSplit = 0;
            long nMiddle;
            long m, n, i, j, k;
            long T = 0;
            int nRet;
            long nSplit;

            nMiddle = (nStart + nEnd) / 2;

            pStart = GetBigOffsetBySmall(nStart);
            pEnd = GetBigOffsetBySmall(nEnd);

            // 看起点和终点是否紧密相连
            if (nStart + 1 == nEnd)
            {
                nRet = this.Asc * Compare(pStart, pEnd);
                if (nRet > 0)
                { // 交换
                    T = pStart;
                    SetRowPtr(nStart, pEnd);
                    SetRowPtr(nEnd, T);
                }
                nSplitPos = nStart;
                return;
            }


            pMiddle = GetBigOffsetBySmall(nMiddle);   //GetRowPtr(nMiddle);

            nRet = this.Asc * Compare(pStart, pEnd);
            if (nRet <= 0)
            {
                nRet = this.Asc * Compare(pStart, pMiddle);
                if (nRet <= 0)
                {
                    pSplit = pMiddle;
                    nSplit = nMiddle;
                }
                else
                {
                    pSplit = pStart;
                    nSplit = nStart;
                }
            }
            else
            {
                nRet = this.Asc * Compare(pEnd, pMiddle);
                if (nRet <= 0)
                {
                    pSplit = pMiddle;
                    nSplit = nMiddle;
                }
                else
                {
                    pSplit = pEnd;
                    nSplit = nEnd;
                }
            }

            // 
            k = nSplit;
            m = nStart;
            n = nEnd;

            T = GetBigOffsetBySmall(k);
            // (m)-->(k)
            SetRowPtr(k, GetBigOffsetBySmall(m));
            i = m;
            j = n;
            while (i != j)
            {
                // Thread.Sleep(0);
                while (true)
                {
                    nRet = this.Asc * Compare(GetBigOffsetBySmall(j), T);
                    if (nRet >= 0 && i < j)
                        j = j - 1;
                    else
                        break;
                }
                if (i < j)
                {
                    // (j)-->(i)
                    SetRowPtr(i, GetBigOffsetBySmall(j) /*GetRowPtr(j)*/);
                    i = i + 1;
                    while (true)
                    {
                        nRet = this.Asc * Compare(/*GetRowPtr(i)*/ GetBigOffsetBySmall(i), T);
                        if (nRet <= 0 && i < j)
                            i = i + 1;
                        else
                            break;
                    }
                    if (i < j)
                    {
                        // (i)--(j)
                        SetRowPtr(j, GetBigOffsetBySmall(i) /*GetRowPtr(i)*/);
                        j = j - 1;
                    }
                }
            }
            SetRowPtr(i, T);
            nSplitPos = i;
        }

        public void SetRowPtr(long nIndex, long lPtr)
        {
            byte[] bufferOffset;

            //得到值
            bufferOffset = new byte[8];
            bufferOffset = BitConverter.GetBytes((long)lPtr);

            // 如果小文件已经在内存
            if (this.m_bufferSmall != null)
            {
                Array.Copy(bufferOffset,
                    0,
                    this.m_bufferSmall,
                    nIndex * 8,
                    8);
                return;
            }

            //覆盖值
            if (m_streamSmall.Position != nIndex * 8)  // 2012/2/15 ***
                m_streamSmall.Seek(nIndex * 8, SeekOrigin.Begin);

            Debug.Assert(bufferOffset.Length == 8, "");
            m_streamSmall.Write(bufferOffset, 0, 8);

        }

        public int Compare(long lPtr1, long lPtr2)
        {
            if (lPtr1 < 0 && lPtr2 < 0)
                return 0;
            else if (lPtr1 >= 0 && lPtr2 < 0)
                return 1;
            else if (lPtr1 < 0 && lPtr2 >= 0)
                return -1;

            if (m_nLoopCount++ % 1000 == 0)
            {
                Thread.Sleep(1);
                if (this.Idle != null)
                {
                    IdleEventArgs e = new IdleEventArgs();
                    this.Idle(this, e);
                }
            }

            DpRecord record1 = GetRecordByOffset(lPtr1);
            DpRecord record2 = GetRecordByOffset(lPtr2);

            return record1.CompareTo(record2);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DpResultSetEnumerator(this);
        }
    }

    public class DpResultSetEnumerator : IEnumerator
    {
        DpResultSet m_resultSet = null;
        long m_index = -1;

        public DpResultSetEnumerator(DpResultSet resultSet)
        {
            m_resultSet = resultSet;
        }

        public void Reset()
        {
            m_index = -1;
        }

        public bool MoveNext()
        {
            m_index++;
            if (m_index >= m_resultSet.Count)
                return false;
            return true;
        }

        public object Current
        {
            get
            {
                return (object)m_resultSet[m_index];
            }
        }
    }

    //设计意图:定义检索到记录的类型，作为DpResultSet的成员
    [Serializable]
    public class DpRecord : IComparable
    {
        public int Index = 0;
        public string m_strDebugInfo = "";

        //私有字段成员，存放记录的逻辑完整ID，格式为:"图书库:0000000001"
        public string m_id;

        //私有字段成员，存放记录的浏览HTML
        string m_strBrowseText = "";

        //公共字段成员，存放记录对应的数据dom
        //public XmlDocument m_dom = null;
        // public string m_strDom = "";

        public DpRecord()
        {
        }

        //非默认构造函数，给m_id赋值
        //myid: 传簇的完整逻辑ID参数
        public DpRecord(string myid)
        {
            m_id = myid;
        }

        //公共ID属性，表示记录完整逻辑ID，提供给外部代码访问
        public string ID
        {
            get
            {
                return m_id;
            }
        }


        //公共BrowseText属性，表示记录浏览HTML文本，提供给外部代码访问
        public string BrowseText
        {
            get
            {
                return m_strBrowseText;
            }
            set
            {
                m_strBrowseText = value;
            }
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
        public int CompareTo(object obj)
        {
            DpRecord myRecord = (DpRecord)obj;

            //m_strDebugInfo += strID1 + "---------" + strID2;

            //通过String类的静态方法Compare比较两个字符串的大小，返回值为小于0，等于0，大于0
            return String.Compare(this.ID, myRecord.ID);
        }

    }//end of class DpRecord

    public delegate void GetTempFilenameEventHandler(object sender,
            GetTempFilenameEventArgs e);

    public class GetTempFilenameEventArgs : EventArgs
    {
        public string TempFilename = "";
    }

    public class ByteList : List<byte>  // ArrayList   // List<byte>
    {
        public ByteList(int nCapacity)
        {
            this.Capacity = nCapacity;
        }
    }

    public enum LogicOper
    {
        OR = 0, // 
        AND = 1,
        SUB = 2,
    }
}
