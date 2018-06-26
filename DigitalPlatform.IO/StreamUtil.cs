using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DigitalPlatform.IO
{
    public static class StreamExtention
    {
        public static void FastSeek(this Stream stream, long lOffset)
        {
#if NO
            // 2017/9/5
            if (lOffset != 0 && lOffset == stream.Length)
            {
                stream.Seek(0, SeekOrigin.End);
                return;
            }
#endif

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
    }
    /// <remarks>
    /// StreamUtil类，Stream功能扩展函数
    /// </remarks>
    public class StreamUtil
    {
        // 2016/5/14 修正 Bug。
        // 快速移动文件指针到相对于文件头部的 lOffset 位置
        // 根据要 seek 到的位置距离当前位置和文件头的远近，选择近的起点来进行移动
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

        public static long DumpStream(Stream streamSource, Stream streamTarget)
        {
            return DumpStream(streamSource, streamTarget, false);
        }

        // 连Flush带看IsConnected状态
        // 如果返回true表示正常，false表示输出流已经切断，没有必要再继续dump了

        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            FlushOutput flushOutputMethod)
        {
            int nChunkSize = 8192;
            byte[] bytes = new byte[nChunkSize];
            long lLength = 0;
            while (true)
            {
                int n = streamSource.Read(bytes, 0, nChunkSize);

                if (n != 0)	// 2005/6/8
                    streamTarget.Write(bytes, 0, n);

                if (flushOutputMethod != null)
                {
                    if (flushOutputMethod() == false)
                        break;
                }

                if (n <= 0)
                    break;

                lLength += n;
                //if (n<1000)
                //	break;
            }

            return lLength;
        }

        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            ProgressOutput progressOutputMethod)
        {
            int nChunkSize = 8192;
            byte[] bytes = new byte[nChunkSize];
            long lLength = 0;
            while (true)
            {
                if (progressOutputMethod != null)
                {
                    if (progressOutputMethod(lLength) == false)
                        break;
                }

                int n = streamSource.Read(bytes, 0, nChunkSize);

                if (n != 0)	// 2005/6/8
                    streamTarget.Write(bytes, 0, n);


                if (n <= 0)
                    break;

                lLength += n;
                //if (n<1000)
                //	break;
            }

            if (progressOutputMethod != null)
            {
                progressOutputMethod(lLength);
            }

            return lLength;
        }

        // return:
        //      true    需要中断
        //      false   继续处理
        public delegate bool Delegate_isStop();

        // return:
        //      -1  表示操作被中断
        //      其它  实际 Dump 的字节数
        public static long LockingDumpStream(Stream streamSource,
    Stream streamTarget,
    bool bFlush,
            Delegate_isStop func_isStop)
        {
            int nChunkSize = 4096 * 10; // 8192;
            byte[] bytes = new byte[nChunkSize];
            long lLength = 0;
            while (true)
            {
                if (func_isStop != null)
                {
                    if (func_isStop() == true)
                        return -1;
                }
                int n = streamSource.Read(bytes, 0, nChunkSize);

                if (n != 0)
                {
                    streamTarget.LockingWrite(bytes, 0, n);

                    if (bFlush == true)
                        streamTarget.Flush();
                }

                if (n <= 0)
                    break;

                lLength += n;
            }

            return lLength;
        }

        /// <summary>
        /// 将源流输入到目标流
        /// 调用前要确保文件指针在适当的位置。在那个位置，就从那个位置开始dump
        /// </summary>
        /// <param name="streamSource">源流</param>
        /// <param name="streamTarget">目标流</param>
        /// <returns>成功执行返回0或者以上，返回值表明本次写入的长度</returns>
        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            bool bFlush)
        {
            int nChunkSize = 8192;
            byte[] bytes = new byte[nChunkSize];
            long lLength = 0;
            while (true)
            {
                int n = streamSource.Read(bytes, 0, nChunkSize);

                if (n != 0)	// 2005/6/8
                    streamTarget.Write(bytes, 0, n);

                if (bFlush == true)
                    streamTarget.Flush();

                if (n <= 0)
                    break;

                lLength += n;
                //if (n<1000)
                //	break;
            }

            return lLength;
        }

        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            long lLength)
        {
            return DumpStream(streamSource, streamTarget, lLength, false);
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

        public static long DumpStream(Stream streamSource,
            Stream streamTarget,
            long lLength,
            FlushOutput flushOutputMethod)
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

                if (n != 0)	// 2005/6/8
                    streamTarget.Write(bytes, 0, (int)n);

                if (flushOutputMethod != null)
                {
                    if (flushOutputMethod() == false)
                        break;
                }


                //if (n<nChunkSize)
                //	break;
                if (n <= 0)
                    break;

                lWrited += n;
            }

            return lWrited;
        }

        // 写入文本文件。
        // 如果文件不存在, 会自动创建新文件
        // 如果文件已经存在，则追加在尾部。
        // Exception:
        //      可能会抛出异常。
        //      System.UnauthorizedAccessException 对路径的访问被拒绝。
        public static void WriteText(string strFileName,
            string strText)
        {
            using (FileStream file = File.Open(
strFileName,
FileMode.Append,	// append
FileAccess.Write,
FileShare.ReadWrite))
            using (StreamWriter sw = new StreamWriter(file,
                System.Text.Encoding.UTF8))
            {
                sw.Write(strText);
            }
        }

#if NO
		// 写入文本文件。
        // 如果文件不存在, 会自动创建新文件
		// 如果文件已经存在，则追加在尾部。
		public static void WriteText(string strFileName, 
			string strText)
		{
            using (StreamWriter sw = new StreamWriter(strFileName,
                true,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strText);
            }
		}
#endif



        // 替换文件某一部分
        // 注意，如果当前文件为UNICODE字符集，不要破坏文件开头的UNICODE方向标志
        // parameters:
        //		nStartOffs	被替换区间的开始偏移(以byte计算)
        //		nEndOffs	被替换区间的结束偏移(以byte计算) nEndOffs可以和nStartOffs重合，表示插入内容
        //		baNew		新的内容。如果想删除文件中一段内容，可以用刚构造的新CByteArray对象作为本参数。
        public static int Replace(Stream s,
            long nStartOffs,
            long nEndOffs,
            byte[] baNew)
        {
            long nFileLen;
            long nOldBytes;
            int nNewBytes;
            long nDelta = 0;
            byte[] baBuffer = null;
            int nMaxChunkSize = 4096;
            int nChunkSize;
            long nRightPartLen;
            long nHead;
            // BOOL bRet;
            int nReadBytes/*, nWriteBytes*/;

            // 检测开始与结束部位是否合法
            if (nStartOffs < 0)
            {
                throw (new Exception("nStartOffs不能小于0"));
            }
            if (nEndOffs < nStartOffs)
            {
                throw (new Exception("nStartOffs不能大于nEndOffs"));
            }
            nFileLen = s.Length;

            if (nEndOffs > nFileLen)
            {
                throw (new Exception("nEndOffs不能大于文件长度"));
            }


            // 计算被替换区间原来的长度
            nOldBytes = nEndOffs - nStartOffs;

            // 计算新长度
            nNewBytes = baNew.Length;

            if (nNewBytes != nOldBytes)
            { // 新旧长度不等，需要移动区间后面的文件内容

                // 根据增加还是减少长度，需要采取不同的移动策略
                // | prev | cur | trail |
                // | prev | cur + delta --> | trial | 先移右方向的块
                // | prev | cur - delta <-- | trial | 先移动左方向的块
                nDelta = nNewBytes - nOldBytes;
                if (nDelta > 0)
                {	// 整体向右移动， 从右面的块开始作
                    nRightPartLen = nFileLen - nStartOffs - nOldBytes;
                    nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
                    nHead = nFileLen - nChunkSize;
                    if (baBuffer == null || baBuffer.Length < nChunkSize)
                    {
                        baBuffer = new byte[nChunkSize];
                    }
                    // baBuffer.SetSize(nChunkSize);
                    for (; ; )
                    {
                        s.Seek(nHead, SeekOrigin.Begin);
                        //SetFilePointer(m_hFile,nHead,NULL,FILE_BEGIN);

                        nReadBytes = s.Read(baBuffer, 0, nChunkSize);
                        /*
                        bRet = ReadFile(m_hFile, baBuffer.GetData(), nChunkSize,
                            (LPDWORD)&nReadBytes, NULL);
                        if (bRet == FALSE)
                            goto ERROR1;
                        */

                        s.Seek(nHead + nDelta, SeekOrigin.Begin);
                        // SetFilePointer(m_hFile,nHead+nDelta,NULL,FILE_BEGIN);

                        s.Write(baBuffer, 0, nReadBytes);
                        /*
                        bRet = WriteFile(m_hFile, baBuffer.GetData(), nReadBytes, 
                            (LPDWORD)&nWriteBytes, NULL);
                        if (bRet == FALSE)
                            goto ERROR1;
                        if (nWriteBytes != nReadBytes) 
                        {
                            // 磁盘满
                            m_nErrNo = ERROR_DISK_FULL;
                            return -1;
                        }
                        */
                        // 移动全部完成
                        if (nHead <= nStartOffs + nOldBytes)	// < 是为了安全
                            break;
                        // 移动头位置
                        nHead -= nChunkSize;
                        if (nHead < nStartOffs + nOldBytes)
                        {
                            nChunkSize -= (int)(nStartOffs + nOldBytes - nHead);
                            nHead = nStartOffs + nOldBytes;
                        }

                    }
                }

                if (nDelta < 0)
                {	// 整体向左移动， 从左面的块开始作
                    nRightPartLen = nFileLen - nStartOffs - nOldBytes;
                    nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
                    if (baBuffer == null || baBuffer.Length < nChunkSize)
                    {
                        baBuffer = new byte[nChunkSize];
                    }

                    //baBuffer.SetSize(nChunkSize);
                    nHead = nStartOffs + nOldBytes;
                    for (; ; )
                    {
                        s.Seek(nHead, SeekOrigin.Begin);
                        // SetFilePointer(m_hFile,nHead,NULL,FILE_BEGIN);

                        nReadBytes = s.Read(baBuffer, 0, nChunkSize);
                        /*
                        bRet = ReadFile(m_hFile, baBuffer.GetData(), nChunkSize,
                            (LPDWORD)&nReadBytes, NULL);
                        if (bRet == FALSE)
                            goto ERROR1;
                        */

                        s.Seek(nHead + nDelta, SeekOrigin.Begin);
                        // SetFilePointer(m_hFile,nHead+nDelta,NULL,FILE_BEGIN);

                        s.Write(baBuffer, 0, nReadBytes);
                        /*
                        bRet = WriteFile(m_hFile, baBuffer.GetData(), nReadBytes, 
                            (LPDWORD)&nWriteBytes, NULL);
                        if (bRet == FALSE)
                            goto ERROR1;
                        if (nWriteBytes != nReadBytes) 
                        {
                            // 磁盘满
                            m_nErrNo = ERROR_DISK_FULL;
                            return -1;
                        }
                        */
                        // 移动全部完成
                        if (nHead + nChunkSize >= nFileLen)
                            break;
                        // 移动头位置
                        nHead += nChunkSize;
                        if (nHead + nChunkSize > nFileLen)
                        { // >是为了安全
                            nChunkSize -= (int)(nHead + nChunkSize - nFileLen);
                        }

                    }
                    // 截断文件(因为缩小了文件尺寸)
                    //ASSERT(nFileLen+nDelta>=0);
                    if (nFileLen + nDelta < 0)
                    {
                        throw (new Exception("nFileLen + nDelta < 0"));
                    }

                    s.SetLength(nFileLen + nDelta);
                    /*
                    SetFilePointer(m_hFile, nFileLen+nDelta, NULL, FILE_BEGIN);
                    SetEndOfFile(m_hFile);
                    */
                }

                //ASSERT(nDelta != 0);
                if (nDelta == 0)
                {
                    throw (new Exception("nDelta == 0"));
                }

            }

            // 将新内容写入

            if (nNewBytes != 0)
            {
                s.Seek(nStartOffs, SeekOrigin.Begin);
                //SetFilePointer(m_hFile,nStartOffs,NULL,FILE_BEGIN);

                s.Write(baNew, 0, nNewBytes);
                /*
                bRet = WriteFile(m_hFile, baNew.GetData(), nNewBytes, 
                    (LPDWORD)&nWriteBytes, NULL);
                if (bRet == FALSE)
                    goto ERROR1;
                if (nWriteBytes != nNewBytes) 
                {
                    // 磁盘满
                    m_nErrNo = ERROR_DISK_FULL;
                    return -1;
                }
                */
            }

            /*
            // 恢复写入前的文件指针，并且迫使缓冲区失效
            if (m_nPos > nFileLen + nDelta)
                m_nPos = nFileLen + nDelta;
            SetFilePointer(m_hFile, m_nPos, NULL, FILE_BEGIN);
            m_baCache.RemoveAll();
            m_bEOF = FALSE;
            return 0;
            ERROR1:
                m_nErrNo = GetLastError();
            return -1;
            */

            return 0;
        }

        // 移动文件某一部分
        // 本函数调用前后，是否保证了文件指针不变?
        // parameters:
        //		nSourceOffs	被移动区间的开始偏移(以byte计算)
        //		nLength		被移动区间的长度(以byte计算)
        //		nTargetOffs	要移动到的目的位置偏移(以byte计算)
        public static int Move(Stream s,
            long nSourceOffs,
            long nLength,
            long nTargetOffs)
        {
            long nFileLen;
            long nDelta = 0;
            byte[] baBuffer = null;
            int nMaxChunkSize = 4096;
            int nChunkSize;
            long nRightPartLen;
            long nHead;
            int nReadBytes;

            // 检测开始与结束部位是否合法
            if (nSourceOffs < 0)
            {
                throw (new Exception("nSourceOffs不能小于0"));
            }
            if (nTargetOffs < 0)
            {
                throw (new Exception("nTargetOffs不能小于0"));
            }
            nFileLen = s.Length;

            if (nSourceOffs + nLength > nFileLen)
            {
                throw (new Exception("移动前区域尾部不能越过文件长度"));
            }


            if (nSourceOffs != nTargetOffs)
            {

                // 根据增加还是减少长度，需要采取不同的移动策略
                // | prev | block | 
                // | prev | + delta --> | block | 先移右方向的块
                // | prev | - delta <-- | block | 先移动左方向的块
                nDelta = nTargetOffs - nSourceOffs;	//nNewBytes - nOldBytes;
                if (nDelta > 0)
                {	// 整体向右移动， 从右面的块开始作
                    nRightPartLen = nLength;	// nRightMost - nStartOffs - nOldBytes;
                    nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
                    nHead = nSourceOffs + nLength - nChunkSize;
                    if (baBuffer == null || baBuffer.Length < nChunkSize)
                    {
                        baBuffer = new byte[nChunkSize];
                    }

                    for (; ; )
                    {
                        s.Seek(nHead, SeekOrigin.Begin);


                        nReadBytes = s.Read(baBuffer, 0, nChunkSize);

                        s.Seek(nHead + nDelta, SeekOrigin.Begin);


                        s.Write(baBuffer, 0, nReadBytes);

                        // 移动全部完成
                        if (nHead <= nSourceOffs)	// < 是为了安全
                            break;
                        // 移动头位置
                        nHead -= nChunkSize;
                        if (nHead < nSourceOffs) // 不足一个chunk
                        {
                            nChunkSize -= (int)(nSourceOffs - nHead);
                            if (nChunkSize <= 0)
                            {
                                throw (new Exception("nChunkSize小于或等于0!"));
                            }
                            nHead = nSourceOffs;
                        }

                    }
                }

                if (nDelta < 0)
                {	// 整体向左移动， 从左面的块开始作
                    nRightPartLen = nLength;	// nRightMost - nStartOffs - nOldBytes;
                    nChunkSize = (int)Math.Min(nRightPartLen, nMaxChunkSize);
                    if (baBuffer == null || baBuffer.Length < nChunkSize)
                    {
                        baBuffer = new byte[nChunkSize];
                    }

                    nHead = nSourceOffs;
                    for (; ; )
                    {
                        s.Seek(nHead, SeekOrigin.Begin);

                        nReadBytes = s.Read(baBuffer, 0, nChunkSize);

                        s.Seek(nHead + nDelta, SeekOrigin.Begin);


                        s.Write(baBuffer, 0, nReadBytes);
                        // 移动全部完成
                        if (nHead + nChunkSize >= nSourceOffs + nLength)
                            break;
                        // 移动头位置
                        nHead += nChunkSize;
                        if (nHead + nChunkSize > nSourceOffs + nLength) // 最后一个块，零头
                        { // >是为了安全
                            nChunkSize -= (int)(nHead + nChunkSize - (nSourceOffs + nLength));
                            if (nChunkSize <= 0)
                            {
                                throw (new Exception("nChunkSize小于或等于0!"));
                            }
                        }

                    }


                }

                if (nDelta == 0)
                {
                    throw (new Exception("nDelta == 0"));
                }

            }

            return 0;
        }
    } // end of class StreamUtil
}
