﻿// #define WRITE_LOG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 操作日志
    /// </summary>
    public class OperLog
    {
        // 状态
        string _state = "disabled";   // 空/disabled
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public LibraryApplication App = null;

        public OperLogFileCache Cache = new OperLogFileCache();

        string m_strDirectory = "";   // 文件存放目录

        string m_strFileName = "";    // 文件名 包含路径部分

        Stream m_stream = null;

        private ReaderWriterLock m_lock = new ReaderWriterLock();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        Stream m_streamSpare = null;
        string m_strSpareOperLogFileName = "";

        bool _bSmallFileMode = false;   // 是否为小文件模式。小文件模式，每次写入动作都写入一个单独的日志文件

        // 准备备用日志文件
        // 所谓备用日志文件，就是当普通日志文件写入发现空间不够时，临时启用的、预先准备好的另一文件
        // parameters:
        //      strFileName 备用日志文件名，不含路径的纯文件名
        // return:
        //      -1  error
        int PrepareSpareOperLogFile(out string strError)
        {
            strError = "";
            if (String.IsNullOrEmpty(m_strDirectory) == true)
            {
                strError = "尚未定义m_strDirectory成员值";
                return -1;
            }

            string strFileName = PathUtil.MergePath(m_strDirectory,
                "spare_operlog.bin");

            m_strSpareOperLogFileName = strFileName;

            try
            {
                // 如果文件存在，就打开，如果文件不存在，就创建一个新的
                m_streamSpare = File.Open(
    strFileName,
    FileMode.OpenOrCreate,
    FileAccess.ReadWrite,
    FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                strError = "打开或创建文件 '" + strFileName + "' 发生错误: " + ex.Message;
                return -1;
            }

            // 第一次创建
            if (m_streamSpare.Length == 0)
            {
                // 写入空白数据
                int nRet = ResetSpareFileContent(out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        // 把备用文件内容清为空白
        int ResetSpareFileContent(out string strError)
        {
            strError = "";

            Debug.Assert(this.m_streamSpare != null, "");

            this.m_streamSpare.Seek(0, SeekOrigin.Begin);

            // 写入空白数据
            byte[] buffer = new byte[4096];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }

            try
            {
                // 一共写入4M空白数据
                for (int i = 0; i < 1024; i++)
                {
                    m_streamSpare.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                strError = "针对文件 " + m_strSpareOperLogFileName + " 写入4M空白信息时出错: " + ex.Message;
                return -1;
            }

            this.m_streamSpare.SetLength(this.m_streamSpare.Position);
            return 0;
        }

        // 把应急写入临时文件的信息，转入当天正常文件
        // return:
        //      -1  出错。
        //      0   普通情况，不用恢复
        //      1   已恢复
        int DoRecover(out string strError)
        {
            strError = "";

            if (m_streamSpare == null)
                return 0;

            if (m_streamSpare.Length == 0)
                return 0;

            this.m_streamSpare.Seek(0, SeekOrigin.Begin);

            // 观察是否有应急写入的内容?
            byte[] length = new byte[8];
            int nRet = m_streamSpare.Read(length, 0, 8);
            if (nRet != 8)
            {
                strError = "应急文件格式不正确1";
                return -1;
            }

            long lLength = BitConverter.ToInt64(length, 0);

            if (lLength > m_streamSpare.Length - m_streamSpare.Position)
            {
                strError = "应急文件格式不正确2";
                return -1;
            }

            if (lLength == 0)
            {
                // 为加倍保险，防止文件启动前被污染影响启动后，此时是否需要写入空白内容
                return 0;   // 没有应急写入的内容
            }

            // 有，并加以处理
            nRet = OpenCurrentStream(out strError);
            if (nRet == -1)
                return -1;
            Debug.Assert(m_stream != null, "");

            // 保存当日日志文件的原始尺寸，以备出错时截断回去
            long lSaveLength = this.m_stream.Length;
            bool bSucceed = false;
            try
            {
                // 把备用文件中的内容，复制到当日日志文件尾部
                m_streamSpare.Seek(0, SeekOrigin.Begin);
                for (int i = 0; ; i++)
                {
                    length = new byte[8];
                    nRet = m_streamSpare.Read(length, 0, 8);
                    if (nRet != 8)
                    {
                        if (i == 0)
                        {
                            strError = "应急文件格式不正确1";
                            return -1;
                        }
                        break;   // 结束？
                    }

                    lLength = BitConverter.ToInt64(length, 0);

                    if (lLength > m_streamSpare.Length - m_streamSpare.Position)
                    {
                        strError = "应急文件格式不正确2";
                        return -1;
                    }

                    if (lLength == 0)
                        break;   // 没有应急写入的内容

                    // 写入长度
                    try
                    {
                        this.m_stream.Write(length, 0, 8);
                        this.m_stream.Flush();  // 迫使问题早些暴露
                    }
                    catch (Exception ex)
                    {
                        strError = "写入当日日志文件时出错: " + ex.Message;
                        return -1;
                    }

                    // 读入内容，追加到当日文件末尾
                    int nWrited = 0;
                    int nThisLen = 0;
                    for (; ; )
                    {
                        byte[] buffer = new byte[4096];
                        nThisLen = Math.Min((int)lLength - nWrited, 4096);
                        nRet = this.m_streamSpare.Read(buffer, 0, nThisLen);
                        if (nRet != nThisLen)
                        {
                            strError = "读入备用文件时出错";
                            return -1;
                        }
                        try
                        {
                            this.m_stream.Write(buffer, 0, nThisLen);
                            this.m_stream.Flush();  // 迫使问题早些暴露
                        }
                        catch (Exception ex)
                        {
                            strError = "写入当日日志文件时出错: " + ex.Message;
                            return -1;
                        }

                        nWrited += nThisLen;
                        if (nWrited >= lLength)
                            break;
                    }
                }

                bSucceed = true;
            }
            finally
            {
                // 截断文件
                if (bSucceed == false)
                {
                    // 通知系统挂起
                    //this.App.HangupReason = HangupReason.OperLogError;

                    this.App.WriteErrorLog("系统启动时，试图将备用日志文件中的信息写入当日日志文件，以使系统恢复正常，但是这一努力失败了。请试着为数据目录腾出更多富余磁盘空间，然后重新启动系统。");
                    this.App.AddHangup("OperLogError");

                    this.m_stream.SetLength(lSaveLength);
                }
            }

            Debug.Assert(bSucceed == true, "");

            // 将备用文件清为空白内容
            nRet = ResetSpareFileContent(out strError);
            if (nRet == -1)
                return -1;

            // 通知系统解挂
            // this.App.HangupReason = HangupReason.None;
            this.App.ClearHangup("OperLogError");
            this.App.WriteErrorLog("系统启动时，发现备用日志文件中有上次紧急写入的日志信息，现已经成功移入当日日志文件。");
            return 1;   // 恢复成功
        }

        // 日志文件存放的目录
        public string Directory
        {
            get
            {
                return this.m_strDirectory;
            }
        }

        // 初始化对象
        public int Initial(
            LibraryApplication app,
            string strDirectory,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.App = app;

            this.Close();

            // 2013/12/1
            ////Debug.WriteLine("begin write lock 4");
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                m_strDirectory = strDirectory;

                PathUtil.TryCreateDir(m_strDirectory);

                // 2013/6/16
                nRet = this.VerifyLogFiles(true, out strError);
                if (nRet == -1)
                {
                    this.App.WriteErrorLog("校验操作日志时出错: " + strError);
                    return -1;
                }
                if (nRet == 1)
                {
                    this.App.WriteErrorLog("校验操作日志时发现错误，已经自动修复：" + strError);
                }

                // 将全部小文件合并到大文件
                // return:
                //      -1  运行出错
                //      0   没有错误
                //      1   有错误
                nRet = MergeTempLogFiles(true, out strError);
                if (nRet == -1)
                {
                    this.App.WriteErrorLog("合并临时日志文件时出错: " + strError);
                    return -1;
                }
                if (nRet == 1)
                {
                    this.App.WriteErrorLog("合并临时日志文件时发现错误，已经自动修复：" + strError);
                }

                nRet = PrepareSpareOperLogFile(out strError);
                if (nRet == -1)
                    return -1;

                Debug.Assert(this.m_streamSpare != null, "");

                // return:
                //      -1  出错。
                //      0   普通情况，不用恢复
                //      1   已恢复
                nRet = DoRecover(out strError);
                if (nRet == -1)
                {
                    // 从备用文件中恢复，失败
                    return -1;
                }

                // 文件指针需处于迎接异常的状态
                this.m_streamSpare.Seek(0, SeekOrigin.Begin);

                // this._bSmallFileMode = true;    // 测试
                this._state = "";   // 表示可用了
                return 0;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
                ////Debug.WriteLine("end write lock 4");
            }
        }

        public void CloseLogStream()
        {
            if (this.m_stream != null)
            {
                this.m_stream.Close();
                this.m_stream = null;
                this.m_strFileName = "";
            }
        }

        // parameters:
        //      bEnterSmallFileMode 是否在本对象关闭后自动进入小文件状态
        public void Close(bool bEnterSmallFileMode = false)
        {
            // 2013/12/1
            ////Debug.WriteLine("begin write lock 5");
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                CloseLogStream();

                if (this.m_streamSpare != null)
                {
                    this.m_streamSpare.Close();
                    this.m_streamSpare = null;
                    this.m_strSpareOperLogFileName = "";
                }

                if (bEnterSmallFileMode == true)
                    this._bSmallFileMode = true;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
                ////Debug.WriteLine("end write lock 5");
            }
        }

        // 写入一个事项(string类型)
        // parameters:
        //      bWrite  是否真的写入文件？如果为 false，表示仅仅测算即将写入的长度
        // return:
        //      返回测算的写入长度
        public static long WriteEntry(
            Stream stream,
            string strMetaData,
            string strBody,
            bool bWrite = true,
            long lTotalLength = 0)
        {
            // 仅仅测算长度
            if (bWrite == false)
            {
                long lSize = 8; // 总长度

                lSize += 8;// metadata长度

                // metadata
                if (String.IsNullOrEmpty(strMetaData) == false)
                {
                    lSize += Encoding.UTF8.GetByteCount(strMetaData);
                }

                lSize += 8;// strBody长度

                // strBody
                lSize += Encoding.UTF8.GetByteCount(strBody);

                return lSize;
            }

            byte[] length = new byte[8];	// 临时写点数据

            if (lTotalLength != 0)
                length = BitConverter.GetBytes(lTotalLength - 8);

            // 记忆起始位置
            long lEntryStart = stream.Position;

            // 事项总长度
            stream.Write(length, 0, 8);

            byte[] metadatabody = null;

            // metadata长度
            if (String.IsNullOrEmpty(strMetaData) == false)
            {
                metadatabody = Encoding.UTF8.GetBytes(strMetaData);
                length = BitConverter.GetBytes((long)metadatabody.Length);
            }
            else
            {
                length = BitConverter.GetBytes((long)0);
            }

            stream.Write(length, 0, 8);	// metadata长度

            // metadata数据
            if (metadatabody != null)
            {
                stream.Write(metadatabody, 0, metadatabody.Length);
                // 如果metadatabody为空, 则此部分空缺
            }


            // strBody长度
            byte[] xmlbody = Encoding.UTF8.GetBytes(strBody);

            length = BitConverter.GetBytes((long)xmlbody.Length);

            stream.Write(length, 0, 8);  // body长度

            // xml body本身
            stream.Write(xmlbody, 0, xmlbody.Length);

            // 事项收尾
            long lEntryLength = stream.Position - lEntryStart - 8;

            if (lTotalLength == 0)
            {
                // 写入单项总长度
                if (stream.Position != lEntryStart)
                {
                    // stream.Seek(lEntryStart, SeekOrigin.Begin);  // 速度慢!
                    long lDelta = lEntryStart - stream.Position;
                    stream.Seek(lDelta, SeekOrigin.Current);
                }

                length = BitConverter.GetBytes((long)lEntryLength);

                stream.Write(length, 0, 8);

                // 文件指针回到末尾位置
                stream.Seek(lEntryLength, SeekOrigin.Current);
            }

            return lEntryLength + 8;
        }

#if NO
        // 创建可用于直接写入的 byte []
        public static byte [] BuildEntry(
    Stream stream,
    string strMetaData,
    string strBody)
        {
            List<byte> result = new List<byte>();

            byte[] length = new byte[8];

            // 缺乏总长度

            byte[] metadatabody = null;

            // metadata长度
            if (String.IsNullOrEmpty(strMetaData) == false)
            {
                metadatabody = Encoding.UTF8.GetBytes(strMetaData);
                length = BitConverter.GetBytes((long)metadatabody.Length);
            }
            else
            {
                length = BitConverter.GetBytes((long)0);
            }

            result.AddRange(length);
            if (metadatabody != null)
                result.AddRange(metadatabody);

            // strBody长度
            byte[] xmlbody = Encoding.UTF8.GetBytes(strBody);
            length = BitConverter.GetBytes((long)xmlbody.Length);

            result.AddRange(length);
            result.AddRange(xmlbody);

            // 事项收尾
            length = BitConverter.GetBytes((long)result.Count); // 总长度差 8 bytes
            result.InsertRange(0, length);

            byte[] array = new byte[result.Count];
            result.CopyTo(array);
            return array;
        }
#endif

        // 注：本函数要预先知道 stream 的长度似乎稍微困难了一些
        // 写入一个事项(Stream类型)
        // parameters:
        //      streamBody  包含数据的流。调用本函数前，要保证文件指针在数据开始位置，本函数会一直从中读取数据到流的末尾
        public static long WriteEntry(
            Stream stream,
            string strMetaData,
            Stream streamBody,
            bool bWrite = true,
            long lTotalLength = 0)
        {
            // 仅仅测算长度
            if (bWrite == false)
            {
                long lSize = 8; // 总长度

                lSize += 8;// metadata长度

                // metadata
                if (String.IsNullOrEmpty(strMetaData) == false)
                {
                    lSize += Encoding.UTF8.GetByteCount(strMetaData);
                }

                lSize += 8;// body 长度

                // body
                long lStremBodyLength = 0;
                if (streamBody != null)
                    lStremBodyLength = (streamBody.Length - streamBody.Position);
                lSize += lStremBodyLength;

                return lSize;
            }

            {
                byte[] length = new byte[8];	// 临时写点数据
                if (lTotalLength != 0)
                    length = BitConverter.GetBytes(lTotalLength - 8);

                // 记忆entry起始位置
                long lEntryStart = stream.Position;

                // 事项总长度
                stream.Write(length, 0, 8);

                byte[] metadatabody = null;

                // metadata长度
                if (String.IsNullOrEmpty(strMetaData) == false)
                {
                    metadatabody = Encoding.UTF8.GetBytes(strMetaData);
                    length = BitConverter.GetBytes((long)metadatabody.Length);
                }
                else
                {
                    length = BitConverter.GetBytes((long)0);
                }

                stream.Write(length, 0, 8);	// metadata长度

                // metadata数据
                if (metadatabody != null)
                {
                    stream.Write(metadatabody, 0, metadatabody.Length);
                    // 如果metadatabody为空, 则此部分空缺
                }

                // 记忆stream起始位置
                long lStreamStart = stream.Position;

                // stream长度已知
                long lStremBodyLength = 0;
                if (streamBody != null)
                    lStremBodyLength = (streamBody.Length - streamBody.Position);
                length = BitConverter.GetBytes((long)lStremBodyLength);
                stream.Write(length, 0, 8);

                if (streamBody != null)
                {
                    // stream本身
                    int chunk_size = 4096;
                    byte[] chunk = new byte[chunk_size];
                    for (; ; )
                    {
                        int nReaded = streamBody.Read(chunk, 0, chunk_size);
                        if (nReaded > 0)
                            stream.Write(chunk, 0, nReaded);

                        if (nReaded < chunk_size)
                            break;
                    }
                }

                // 整个事项长度已知
                long lEntryLength = stream.Position - lEntryStart - 8;


                // stream长度现在已知
                long lStreamLength = stream.Position - lStreamStart - 8;

                if (lTotalLength == 0)
                {
                    if (stream.Position != lStreamStart)
                    {
                        // stream.Seek(lStreamStart, SeekOrigin.Begin);      // 速度慢!
                        long lDelta = lStreamStart - stream.Position;
                        stream.Seek(lDelta, SeekOrigin.Current);
                    }

                    length = BitConverter.GetBytes((long)lStreamLength);

                    stream.Write(length, 0, 8);

                    // 事项收尾

                    // 写入单项总长度
                    if (stream.Position != lEntryStart)
                    {
                        // stream.Seek(lEntryStart, SeekOrigin.Begin);      // 速度慢!
                        long lDelta = lEntryStart - stream.Position;
                        stream.Seek(lDelta, SeekOrigin.Current);
                    }

                    length = BitConverter.GetBytes((long)lEntryLength);

                    stream.Write(length, 0, 8);

                    // 文件指针回到末尾位置
                    stream.Seek(lEntryLength, SeekOrigin.Current);
                }

                return lEntryLength + 8;
            }
        }

        // 构建当日日志文件名
        string BuildCurrentLogFileName()
        {
            DateTime now = DateTime.Now;    // 采用本地时区，主要是方便在半夜12点的时候切换日志文件名。一般图书馆在半夜都是不开馆。
            // DateTime.UtcNow;
            return Path.Combine(this.m_strDirectory, now.ToString("yyyyMMdd") + ".log");
        }

        public string CurrentFileName
        {
            get
            {
                return this.m_strFileName;
            }
        }

        // 获得当日文件流的尺寸
        public long GetCurrentStreamLength()
        {
            if (m_stream == null)
                return -1;
            return m_stream.Length;
        }

        // 打开当天日志文件流，并将文件指针放在文件尾部
        int OpenCurrentStream(out string strError)
        {
            strError = "";

            string strFileName = BuildCurrentLogFileName();

            if (strFileName == this.m_strFileName)
            {
                // 如果文件名存在，那流也应当打开
                Debug.Assert(this.m_stream != null, "");
            }
            else
            {
                this.CloseLogStream();   // 先关闭已经存在的流

                try
                {
                    // 如果文件存在，就打开，如果文件不存在，就创建一个新的
                    m_stream = File.Open(
        strFileName,
        FileMode.OpenOrCreate,
        FileAccess.ReadWrite,
        FileShare.ReadWrite);
                }
                catch (Exception ex)
                {
                    strError = "打开或创建文件 '" + strFileName + "' 发生错误: " + ex.Message;
                    return -1;
                }

                m_strFileName = strFileName;

                m_stream.Seek(0, SeekOrigin.End);
            }

            return 0;
        }

        string _strPrevTime = "";   // 前一次产生日志文件名的时间部分
        long _lSeed = 0;    // 单个小文件名的种子。新的一秒开始后，要复位重新开始

        // 获得小文件名。锁定的目的主要是为了让种子不会发生出重复的号码
        string GetSmallLogFileName()
        {
            ////Debug.WriteLine("begin write lock 6");
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                DateTime now = DateTime.Now;    // 采用本地时区，主要是方便在半夜12点的时候切换日志文件名。一般图书馆在半夜都是不开馆。
                string strTime = now.ToString("yyyyMMdd_HHmmss");

                if (strTime != _strPrevTime)
                    this._lSeed = 0;
                else
                    this._lSeed++;

                string strFileName = "";
                if (this._lSeed == 0)
                    strFileName = strTime + ".tlog";
                else
                    strFileName = strTime + "_" + _lSeed.ToString().PadLeft(4, '0') + ".tlog";

                this._strPrevTime = strTime;

                return Path.Combine(this.m_strDirectory, strFileName);
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
                ////Debug.WriteLine("end write lock 6");
            }
        }

        bool IsEnabled()
        {
            return string.IsNullOrEmpty(this._state);
        }

        // 向日志文件中写入一条日志记录
        // parameters:
        //      attachment  附件。如果为 null，表示没有附件
        public int WriteEnventLog(string strXmlBody,
            Stream attachment,
            out string strError)
        {
            strError = "";

            ////Debug.WriteLine("begin write lock");
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (this.IsEnabled() == false)
                {
                    strError = "操作日志系统尚未准备就绪";
                    // 2019/4/26
                    this.App?.WriteErrorLog($"WriteEnventLog() error:{strError} strXmlBody:{strXmlBody}");
                    return -1;
                }


                // 在锁定范围内判断这个布尔变量，比较安全
                // 在锁定范围外面前部判断，可能会出现锁定中途布尔变量才修改的情况，会遗漏此种情况的处理
                if (this._bSmallFileMode == true)
                    goto SMALL_MODE;

                int nRet = OpenCurrentStream(out strError);
                if (nRet == -1)
                    return -1;

                // 写操作
                // 如果磁盘空间满，要妥善收尾

                long lStart = this.m_stream.Position;	// 记忆起始位置

                try
                {

                    /*
                    byte[] length = new byte[8];

                    this.m_stream.Write(length, 0, 8);	// 临时写点数据,占据记录总长度位置


                    // 写入xml事项
                    WriteEntry(this.m_stream,
                        null,
                        strXmlBody);

                    // 写入attachment事项
                    WriteEntry(this.m_stream,
                        null,
                        attachment);

                    long lRecordLength = this.m_stream.Position - lStart - 8;

                    // 写入记录总长度
                    this.m_stream.Seek(lStart, SeekOrigin.Begin);

                    length = BitConverter.GetBytes((long)lRecordLength);

                    this.m_stream.Write(length, 0, 8);

                    // 迫使写入物理文件
                    this.m_stream.Flush();

                    // 文件指针回到末尾位置
                    this.m_stream.Seek(lRecordLength, SeekOrigin.Current);
                     * */
                    // 将日志写入文件
                    // 不处理异常
                    WriteEnventLog(
                        this.m_stream,
                        strXmlBody,
                        attachment);

#if WRITE_LOG
                    this.App.WriteDebugInfo($"WriteEventLog() fileLength={this.m_stream.Length}");
#endif

                }
                catch (Exception ex)
                {
                    // 怎么知道是空间满?
                    this.App.WriteErrorLog("严重错误：写入日志文件时，发生错误：" + ex.Message + "。日志文件断点为: " + lStart.ToString());

                    // 通知系统挂起
                    // this.App.HangupReason = HangupReason.OperLogError;
                    this.App.AddHangup("OperLogError");

                    this.App.WriteErrorLog("系统因此挂起。请检查数据目录是否有足够的富余磁盘空间。问题解决后，重启系统。");

                    // 转而写入备用文件

                    try
                    {
                        // 不处理异常
                        WriteEnventLog(
                            this.m_streamSpare,
                            strXmlBody,
                            attachment);
                    }
                    catch (Exception ex0)
                    {
                        this.App.WriteErrorLog("致命错误：当写入日志文件发生错误后，转而写入备用日志文件，但后者也发生异常：" + ex0.Message);
                    }


                    // 如果抛出异常，别忘记了先截断文件
                    this.m_stream.SetLength(lStart);
                    // 迫使写入物理文件
                    this.m_stream.Flush();

                    throw ex;
                }
                return 0;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
                ////Debug.WriteLine("end write lock");
            }

        SMALL_MODE:
            // 小日志文件模式
            // 可以并发，因为文件名已经严格区分了
            {
                // 获得小日志文件名
                string strFileName = GetSmallLogFileName();
                try
                {
                    // 如果文件存在，就打开，如果文件不存在，就创建一个新的
                    using (Stream stream = File.Open(
        strFileName,
        FileMode.OpenOrCreate,
        FileAccess.ReadWrite,
        FileShare.ReadWrite))
                    {
                        stream.Seek(0, SeekOrigin.End);
                        try
                        {
                            // 将日志写入文件
                            // 不处理异常
                            WriteEnventLog(
                                stream,
                                strXmlBody,
                                attachment);
                        }
                        catch (Exception ex)
                        {
                            // 怎么知道是空间满?
                            this.App.WriteErrorLog("严重错误：写入临时日志文件 '" + strFileName + "' 时，发生错误：" + ex.Message);

                            // 通知系统挂起
                            // this.App.HangupReason = HangupReason.OperLogError;
                            this.App.AddHangup("OperLogError");
                            this.App.WriteErrorLog("系统因此挂起。请检查数据目录是否有足够的富余磁盘空间。问题解决后，重启系统。");
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                {
                    strError = "打开或创建文件 '" + strFileName + "' 发生错误: " + ex.Message;
                    return -1;
                }
                return 0;
            }
        }

#if NO
        // TODO: 如果能预先知道数据事项的长度，在开头就写好长度位，不用让文件指针往返，速度就更快了
        // 将日志写入文件
        // 不处理异常
        // parameters:
        //      attachment  附件。如果为 null，表示没有附件
        static void WriteEnventLog(
            Stream stream,
            string strXmlBody,
            Stream attachment)
        {
            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            // 清空
            for (int i = 0; i < length.Length; i++)
            {
                length[i] = 0;
            }

            stream.Write(length, 0, 8);	// 临时写点数据,占据记录总长度位置

            // 写入xml事项
            WriteEntry(
                stream,
                null,
                strXmlBody);

            // 写入attachment事项
            WriteEntry(
                stream,
                null,
                attachment);

            long lRecordLength = stream.Position - lStart - 8;

            // 写入记录总长度
            if (stream.Position != lStart)
            {
                // stream.Seek(lStart, SeekOrigin.Begin);  // 速度慢!
                long lDelta = lStart - stream.Position;
                stream.Seek(lDelta, SeekOrigin.Current);
            }

            length = BitConverter.GetBytes((long)lRecordLength);

            stream.Write(length, 0, 8);

            // 迫使写入物理文件
            stream.Flush();

            // 文件指针回到末尾位置
            stream.Seek(lRecordLength, SeekOrigin.Current);
        }

#endif

        // 改进后版本
        // TODO: 如果能预先知道数据事项的长度，在开头就写好长度位，不用让文件指针往返，速度就更快了
        // 将日志写入文件
        // 不处理异常
        // parameters:
        //      attachment  附件。如果为 null，表示没有附件
        static void WriteEnventLog(
            Stream stream,
            string strXmlBody,
            Stream attachment)
        {
            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            // 清空
            for (int i = 0; i < length.Length; i++)
            {
                length[i] = 0;
            }

            // 获得 XML 部分的长度
            long lXmlBodyLength = WriteEntry(
                stream,
                null,
                strXmlBody,
                false,
                0);
            // 获得 attachment 部分的长度
            long lAttachmentLength = WriteEntry(
                stream,
                null,
                attachment,
                false,
                0);
            length = BitConverter.GetBytes(lXmlBodyLength + lAttachmentLength);

            stream.Write(length, 0, 8);	// 写入总长度

            // 真正写入 XML 部分
            WriteEntry(
                stream,
                null,
                strXmlBody,
                true,
                lXmlBodyLength);

            // 真正写入 attachment 部分
            WriteEntry(
                stream,
                null,
                attachment,
                true,
                lAttachmentLength);

            long lRecordLength = stream.Position - lStart - 8;

            Debug.Assert(lRecordLength == lXmlBodyLength + lAttachmentLength, "");

#if NO
            // 写入记录总长度
            if (stream.Position != lStart)
            {
                // stream.Seek(lStart, SeekOrigin.Begin);  // 速度慢!
                long lDelta = lStart - stream.Position;
                stream.Seek(lDelta, SeekOrigin.Current);
            }

            length = BitConverter.GetBytes((long)lRecordLength);

            stream.Write(length, 0, 8);

            // 迫使写入物理文件
            stream.Flush();

            // 文件指针回到末尾位置
            stream.Seek(lRecordLength, SeekOrigin.Current);
#endif
            // 迫使写入物理文件
            stream.Flush();
        }

        // TODO: 可以被文件系统查询当日日志文件时调用?
        public void ReOpen()
        {
            if (this.m_stream != null)
            {
                this.m_stream.Close();

                this.m_stream = null;
                this.m_stream = File.Open(
                    this.m_strFileName,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                this.m_stream.Seek(0, SeekOrigin.End);
            }
        }

        // 包装后的版本
        public int WriteOperLog(XmlDocument dom,
            string strClientAddress,
            out string strError)
        {
            string strRefID = "";
            return WriteOperLog(dom, strClientAddress, new DateTime(0), out strRefID, out strError);
        }

        // 写入一条操作日志
        // parameters:
        //      start_time  操作开始的时间。本函数会用它算出整个操作耗费的时间。如果 ticks == 0，表示不使用这个值
        public int WriteOperLog(XmlDocument dom,
            string strClientAddress,
            DateTime start_time,
            out string strRefID,
            out string strError)
        {
            strRefID = "";

            // 2013/11/20
            if (this._bSmallFileMode == false
                && this.m_streamSpare == null)
            {
                strError = "日志备用文件未正确初始化";
                return -1;
            }

#if DEBUG
            if (this._bSmallFileMode == false)
            {
                Debug.Assert(this.m_streamSpare != null, "m_streamSpare == null");
            }
#endif

            WriteClientAddress(dom, strClientAddress);
            // 1.01 (2014/3/8) 修改了 operation=amerce;action=expire 记录中元素名 oldReeaderRecord 为 oldReaderRecord
            // 1.02 (2015/9/8) 日志中增加了 time 元素 linkUID 和 uid 元素
            // 1.03 (2015/9/13) SetReaderInfo 中增加了 changedEntityRecord 元素 
            // 1.04 (2017/1/12) Borrow() Return() 中，readerRecord 元素增加了 clipping 属性，如果值为 "true"，表示这里记载的读者记录是不完全的，不应用于快照恢复读者记录
            // 1.05 (2017/1/16) CopyBiblioINfo() API 的操作日志增加了 overwritedRecord 元素。记载被覆盖以前的记录内容
            // 1.06 (2017/5/16) 对 ManageDatabase() API 也写入日志了
            // 1.07 (2018/3/7) passgate 日志记录中增加了 readerRefID 元素
            // 1.08 (2019/4/25) changeReaderPassword 日志此前版本中少了 readerBarcode 和 newPassword 元素。现在补上
            DomUtil.SetElementText(dom.DocumentElement, "version", "1.08");

            if (start_time != new DateTime(0))
            {
                XmlElement time = dom.CreateElement("time");
                dom.DocumentElement.AppendChild(time);
                DateTime now = DateTime.Now;
                time.SetAttribute("start", start_time.ToString("s"));
                time.SetAttribute("end", now.ToString("s"));
                time.SetAttribute("seconds", (now - start_time).TotalSeconds.ToString("F3"));

                // 日志记录的唯一 ID
                strRefID = Guid.NewGuid().ToString();
                DomUtil.SetElementText(dom.DocumentElement, "uid", strRefID);
            }

            int nRet = WriteEnventLog(dom.OuterXml,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            // 2015/12/27
            if (this.App != null && this.App.operLogThread != null)
            {
                string strOperation = DomUtil.GetElementText(dom.DocumentElement,
"operation");
                if (OperLogThread.NeedAdd(strOperation))
                    this.App.operLogThread.AddOperLog(dom);
            }

            // ReOpen();          
            return 0;
        }

        public static void WriteClientAddress(XmlDocument dom,
            string strClientAddress)
        {
            if (string.IsNullOrEmpty(strClientAddress) == true)
                return;

            string strVia = "";
            int nRet = strClientAddress.IndexOf("@");
            if (nRet != -1)
            {
                strVia = strClientAddress.Substring(nRet + 1);
                strClientAddress = strClientAddress.Substring(0, nRet);
            }
            XmlNode node = DomUtil.SetElementText(dom.DocumentElement,
                "clientAddress",
                strClientAddress);
            if (string.IsNullOrEmpty(strVia) == false)
                DomUtil.SetAttr(node, "via", strVia);
        }

        // 写入一条操作日志(另一版本)
        public int WriteOperLog(XmlDocument dom,
            string strClientAddress,
            Stream attachment,
            out string strError)
        {
            Debug.Assert(this.m_streamSpare != null, "");

            WriteClientAddress(dom, strClientAddress);
            DomUtil.SetElementText(dom.DocumentElement, "version", "1.06");

            int nRet = WriteEnventLog(dom.OuterXml,
                attachment,
                out strError);
            if (nRet == -1)
                return -1;

            // ReOpen();          

            return 0;
        }

        // 注: 这个函数旧版本的缺点是，每次都要重新打开一下文件。新版本改用了 CacheFileItem
        // 原先版本
        // 获得一个日志记录
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strFileName 纯文件名,不含路径部分。但要包括".log"部分。
        //      lIndex  记录序号。从0开始计数。lIndex为-1时调用本函数，表示希望获得整个文件尺寸值，将返回在lHintNext中。
        //      lHint   记录位置暗示性参数。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。
        //              目前的含义是记录起始位置。
        //      strStyle    如果不包含 supervisor，则本函数会自动过滤掉日志记录中读者记录的 password 字段
        //      attachment  承载输出的附件部分的 Stream 对象。如果为 null，表示不输出附件部分
        //                  本函数返回后，attachment 的文件指针在文件末尾。调用时需引起注意
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public int GetOperLog(
            string strLibraryCodeList,
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            string strFilter,
            out long lHintNext,
            out string strXml,
            // ref Stream attachment,
            Stream attachment,
            out string strError)
        {
            strError = "";
            strXml = "";
            lHintNext = -1;

            int nRet = 0;

#if OLD
            Stream stream = null;
#else
            Stream stream = null;
            CacheFileItem cache_item = null;
#endif

            if (string.IsNullOrEmpty(this.m_strDirectory) == true)
            {
                strError = "日志目录 m_strDirectory 尚未初始化";
                return -1;
            }
            Debug.Assert(this.m_strDirectory != "", "");

            string strFilePath = this.m_strDirectory + "\\" + strFileName;

            try
            {

#if OLD
                stream = File.Open(
                    strFilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
#else
                cache_item = this.Cache.Open(strFilePath);
                stream = cache_item.Stream;
#endif
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "日志文件 " + strFileName + "没有找到";
                lHintNext = 0;
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "打开日志文件 '" + strFileName + "' 发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                long lFileSize = 0;
                // 定位

                // 加锁
                // 在获得文件整个长度的过程中，必须要小心提防另外并发的正在对文件进行写的操作
                bool bLocked = false;

                // 如果读取的是当前正在写入的热点日志文件，则需要加锁（读锁）
                if (PathUtil.IsEqual(strFilePath, this.m_strFileName) == true)
                {
                    ////Debug.WriteLine("begin read lock 1");
                    this.m_lock.AcquireReaderLock(m_nLockTimeout);
                    bLocked = true;
                }

                try
                {   // begin of lock try
                    lFileSize = stream.Length;

#if WRITE_LOG
                    FileInfo fi = new FileInfo(strFilePath);

                    this.App.WriteDebugInfo($"lFileSize={lFileSize} fi.Length={fi.Length} (1)");
#endif

                }   // end of lock try
                finally
                {
                    if (bLocked == true)
                    {
                        this.m_lock.ReleaseReaderLock();
                        ////Debug.WriteLine("end read lock 1");
                    }
                }
                // lIndex == -1表示希望获得文件整个的尺寸
                if (lIndex == -1)
                {
                    lHintNext = lFileSize;  //  stream.Length;
                    return 1;   // 成功
                }

                // 没有暗示，只能从头开始找
                if (lHint == -1 || lIndex == 0)
                {
                    // return:
                    //      -1  error
                    //      0   成功
                    //      1   到达文件末尾或者超出
                    nRet = LocationRecord(stream,
                        lFileSize,
                        lIndex,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                        return 2;
                }
                else
                {
                    // 根据暗示找到
                    if (lHint == stream.Length)
                        return 2;

                    if (lHint > stream.Length)
                    {
                        strError = "lHint参数值不正确";
                        return -1;
                    }
                    if (stream.Position != lHint)
                        stream.Seek(lHint, SeekOrigin.Begin);
                }

                //////

                // MemoryStream attachment = null; // new MemoryStream();
                // TODO: 是否可以优化为，先读出XML部分，如果需要再读出attachment? 并且attachment可以按需读出分段
                // return:
                //      1   出错
                //      0   成功
                //      1   文件结束，本次读入无效
                nRet = ReadEnventLog(
                    stream,
                    out strXml,
                    // ref attachment,
                    attachment,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 后面会通过 strStyle 参数决定是否过滤读者记录的 password 元素

                // 限制记录观察范围
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    // 检查和过滤日志XML记录
                    // return:
                    //      -1  出错
                    //      0   不允许返回当前日志记录
                    //      1   允许范围当前日志记录
                    nRet = FilterXml(
                        strLibraryCodeList,
                        strStyle,
                        strFilter,
                        ref strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        nRet = 1;   // 只好返回

                        // return -1;
                    }
                    if (nRet == 0)
                    {
                        strXml = "";    // 清空，让前端看不到内容
                        attachment.SetLength(0);    // 清空附件
                    }
                }
                else
                {
                    // 虽然是全局用户，也要限制记录尺寸
                    nRet = ResizeXml(
                        strStyle,
                        strFilter,
                        ref strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        nRet = 1;   // 只好返回
                        // return -1;
                    }
                }

                lHintNext = stream.Position;
                return 1;
            }
            finally
            {
#if OLD
                stream.Close();
#else
                this.Cache.Close(cache_item);
#endif
            }
        }

        // 获得详细级别
        // return:
        //      0   全部
        //      1   删除 读者记录和册记录
        //      2   删除 读者记录和册记录中的 <borrowHistory>
        static int GetLevel(string strStyle)
        {
            // 2013/11/6
            if (string.IsNullOrEmpty(strStyle) == true)
                return 0;

            string[] parts = strStyle.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                if (StringUtil.HasHead(s, "level-") == true)
                {
                    string strNumber = s.Substring("level-".Length).Trim();
                    int v = 0;
                    Int32.TryParse(strNumber, out v);
                    return v;
                }
            }

            return 0;
        }

        // parameters:
        //      strStyle    如果不包含 supervisor，则需要过滤读者记录的 password 元素
        // return:
        //      -1  出错
        //      0   没有改变
        //      1   发生了改变
        static int ResizeXml(
            string strStyle,
            string strFilter,
            ref string strXml,
            out string strError)
        {
            strError = "";

            bool bSupervisor = StringUtil.IsInList("supervisor", strStyle);

            int nLevel = -1;
            // 先检测一次，可以提高某些情况下的运行速度
            if (string.IsNullOrEmpty(strFilter) == true
                && bSupervisor == true)
            {
                // 获得详细级别
                // return:
                //      0   全部
                //      1   删除 读者记录和册记录
                //      2   删除 读者记录和册记录中的 <borrowHistory>
                nLevel = GetLevel(strStyle);
                if (nLevel == 0)
                    return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "日志记录XML内容装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            // 2013/11/22
            if (string.IsNullOrEmpty(strFilter) == false
                && StringUtil.IsInList(strOperation, strFilter) == false)
            {
                strXml = "";
                return 1;
            }

            if (bSupervisor == false)
            {
                // 过滤日志记录中，读者记录的 password 元素
                RemoveReaderPassword(ref dom);
            }

            if (nLevel == -1)
            {
                // 获得详细级别
                // return:
                //      0   全部
                //      1   删除 读者记录和册记录
                //      2   删除 读者记录和册记录中的 <borrowHistory>
                nLevel = GetLevel(strStyle);
                if (nLevel == 0)
                    return 0;
            }

            {
#if NO
                // 也要减少尺寸
                if (strOperation == "borrow")
                {
                    ResizeBorrow(nLevel, ref dom);
                }
                else if (strOperation == "return")
                {
                    ResizeReturn(nLevel, ref dom);
                }
                else if (strOperation == "setEntity")
                {
                    ResizeSetEntity(nLevel, ref dom);
                }
                else if (strOperation == "setReaderInfo")
                {
                    ResizeSetReaderInfo(nLevel, ref dom);
                }
                else if (strOperation == "amerce")
                {
                    ResizeAmerce(nLevel, ref dom);
                } 
#endif
                // 减少尺寸
                ResizeXml(strOperation,
                    nLevel,
                    ref dom);
                strXml = dom.DocumentElement.OuterXml;
            }

            return 1;
        }

        // 检查和过滤日志XML记录
        // parameters:
        //      strStyle    如果不包含 supervisor，则需要过滤日志记录中读者记录的 password 元素
        // return:
        //      -1  出错
        //      0   不允许返回当前日志记录
        //      1   允许返回当前日志记录
        static int FilterXml(
            string strLibraryCodeList,
            string strStyle,
            string strFilter,
            ref string strXml,
            out string strError)
        {
            strError = "";

            // 获得详细级别
            // return:
            //      0   全部
            //      1   删除 读者记录和册记录
            //      2   删除 读者记录和册记录中的 <borrowHistory>
            int nLevel = GetLevel(strStyle);

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "日志记录XML内容装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            // 分馆用户不允许看到 setUser 操作信息
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            if (strOperation == "setUser")
            {
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                    return 0;
            }

            if (StringUtil.IsInList("supervisor", strStyle) == false)
            {
                // 过滤日志记录中，读者记录的 password 元素
                RemoveReaderPassword(ref dom);
            }

            // 2013/11/22
            if (string.IsNullOrEmpty(strFilter) == false
                && StringUtil.IsInList(strOperation, strFilter) == false)
                return 0;

            XmlNode node = null;
            string strLibraryCodes = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
            if (node == null)
                return 1;  // 不需要过滤
            string strSourceLibraryCode = "";
            string strTargetLibraryCode = "";
            ParseLibraryCodes(strLibraryCodes,
out strSourceLibraryCode,
out strTargetLibraryCode);

            // source在管辖范围内，target不在管辖范围内
            // 日志记录需要变换。相当于告诉访问者，这条记录被移走了，但新修改的信息不要透露
            if (strSourceLibraryCode != strTargetLibraryCode
                && StringUtil.IsInList(strSourceLibraryCode, strLibraryCodeList) == true
                && StringUtil.IsInList(strTargetLibraryCode, strLibraryCodeList) == false)
            {
                if (strOperation == "devolveReaderInfo")
                {
                    FilterDovolveReaderInfo(ref dom);
                }
                else if (strOperation == "setEntity")
                {
                    FilterSetEntity(// nLevel, 
                        ref dom);
                }
                else if (strOperation == "setReaderInfo")
                {
                    FilterSetReaderInfo(// nLevel, 
                        ref dom);
                }
#if NO
                else if (strOperation == "borrow" && nLevel > 0)
                {
                    ResizeBorrow(nLevel, ref dom);
                }
                else if (strOperation == "return" && nLevel > 0)
                {
                    ResizeReturn(nLevel, ref dom);
                }
                else if (strOperation == "setEntity" && nLevel > 0)
                {
                    ResizeSetEntity(nLevel, ref dom);
                }
                else if (strOperation == "setReaderInfo" && nLevel > 0)
                {
                    ResizeSetReaderInfo(nLevel, ref dom);
                }
                else if (strOperation == "amerce" && nLevel > 0)
                {
                    ResizeAmerce(nLevel, ref dom);
                }
#endif

                {
                    // 减少尺寸
                    ResizeXml(strOperation,
                        nLevel,
                        ref dom);
                    strXml = dom.DocumentElement.OuterXml;
                }
                return 1;
            }

            if (StringUtil.IsInList(strTargetLibraryCode, strLibraryCodeList) == false)
                return 0;

            {
                // 减少尺寸
                ResizeXml(strOperation,
                    nLevel,
                    ref dom);
                strXml = dom.DocumentElement.OuterXml;
            }

            // 完成返回日志记录
            return 1;
        }

        // 对日志记录进行减小尺寸的操作
        // 注：本函数不负责过滤读者记录中的 password 元素
        static void ResizeXml(string strOperation,
            int nLevel,
            ref XmlDocument dom)
        {
            if (strOperation == "borrow" && nLevel > 0)
            {
                ResizeBorrow(nLevel, ref dom);
            }
            else if (strOperation == "return" && nLevel > 0)
            {
                ResizeReturn(nLevel, ref dom);
            }
            else if (strOperation == "setEntity" && nLevel > 0)
            {
                ResizeSetEntity(nLevel, ref dom);
            }
            else if (strOperation == "setOrder" && nLevel > 0)
            {
                ResizeSetOrder(nLevel, ref dom);
            }
            else if (strOperation == "setIssue" && nLevel > 0)
            {
                ResizeSetIssue(nLevel, ref dom);
            }
            else if (strOperation == "setComment" && nLevel > 0)
            {
                ResizeSetComment(nLevel, ref dom);
            }
            else if (strOperation == "setReaderInfo" && nLevel > 0)
            {
                ResizeSetReaderInfo(nLevel, ref dom);
            }
            else if (strOperation == "amerce" && nLevel > 0)
            {
                ResizeAmerce(nLevel, ref dom);
            }
            else if (strOperation == "hire" && nLevel > 0)
            {
                ResizeHire(nLevel, ref dom);
            }
            else if (strOperation == "foregift" && nLevel > 0)
            {
                ResizeForegift(nLevel, ref dom);
            }
            else if (strOperation == "settlement" && nLevel > 0)
            {
                ResizeSettlement(nLevel, ref dom);
            }
            else if (strOperation == "changeReaderPassword" && nLevel > 0)
            {
                ResizeChangeReaderPassword(nLevel, ref dom);
            }
            else if (strOperation == "setBiblioInfo" && nLevel > 0)
            {
                ResizeSetBiblioInfo(nLevel, ref dom);
            }
        }

        // 过滤转移读者的日志记录
        static void FilterDovolveReaderInfo(ref XmlDocument dom)
        {
            // 删除<targetReaderRecord>元素
            DomUtil.DeleteElement(dom.DocumentElement, "targetReaderRecord");
        }

        // 过滤设置实体的日志记录
        static void FilterSetEntity(// int nLevel,
            ref XmlDocument dom)
        {
            // 删除<record>元素
            DomUtil.DeleteElement(dom.DocumentElement, "record");
            // ResizeSetEntity(nLevel, ref dom);
        }

        // 过滤设置读者记录的日志记录
        static void FilterSetReaderInfo(// int nLevel, 
            ref XmlDocument dom)
        {
            // 删除<record>元素
            DomUtil.DeleteElement(dom.DocumentElement, "record");
            // ResizeSetReaderInfo(nLevel, ref dom);
        }

        // 书目
        static void RemoveBiblioRecord(int nLevel,
    string strElementName,
    ref XmlDocument dom)
        {
            if (nLevel == 0)
                return;

            {
                string strBiblioReacord = DomUtil.GetElementText(dom.DocumentElement, strElementName);
                if (string.IsNullOrEmpty(strBiblioReacord) == false)
                {
                    if (nLevel == 2)
                    {
                        DomUtil.SetElementText(dom.DocumentElement, strElementName, "");
                        return;
                    }

                }
            }
        }

        static string GetParentID(string strRecord)
        {
            XmlDocument reader_dom = new XmlDocument();
            try
            {
                reader_dom.LoadXml(strRecord);
                return DomUtil.GetElementText(reader_dom.DocumentElement, "parent");
            }
            catch
            {
                return null;
            }
        }

        // 册
        static void RemoveEntityRecord(int nLevel,
    string strElementName,
    ref XmlDocument dom)
        {
            if (nLevel == 0)
                return;

            {
                XmlNode record_node = null;
                string strItemReacord = DomUtil.GetElementText(dom.DocumentElement, strElementName, out record_node);
                if (string.IsNullOrEmpty(strItemReacord) == false)
                {
                    if (nLevel == 2)
                    {
                        // 设置 parent_id 属性，清除 InnerXml
                        if (record_node != null)
                        {
                            string strParentID = (record_node as XmlElement).GetAttribute("parent_id");
                            if (string.IsNullOrEmpty(strParentID) == true)
                            {
                                strParentID = GetParentID(strItemReacord);

                                if (string.IsNullOrEmpty(strParentID) == false)
                                    (record_node as XmlElement).SetAttribute("parent_id", strParentID);
                            }

                            record_node.InnerXml = "";
                        }

                        // DomUtil.SetElementText(dom.DocumentElement, strElementName, "");
                        return;
                    }

                    // nLevel == 1
                    XmlDocument reader_dom = new XmlDocument();
                    try
                    {
                        reader_dom.LoadXml(strItemReacord);
                        XmlNode node = reader_dom.DocumentElement.SelectSingleNode("borrowHistory");
                        if (node != null)
                            node.ParentNode.RemoveChild(node);
                        DomUtil.SetElementText(dom.DocumentElement, strElementName, reader_dom.DocumentElement.OuterXml);
                    }
                    catch
                    {
                    }
                }
            }
        }

        // 订购 期 评注
        static void RemoveItemRecord(int nLevel,
            string strElementName,
            ref XmlDocument dom)
        {
            if (nLevel == 0)
                return;

            {
                XmlNode node = null;
                string strItemReacord = DomUtil.GetElementText(dom.DocumentElement, strElementName, out node);
                if (string.IsNullOrEmpty(strItemReacord) == false)
                {
                    if (nLevel == 2)
                    {
                        // 设置 parent_id 属性，清除 InnerXml
                        if (node != null)
                        {
                            string strParentID = (node as XmlElement).GetAttribute("parent_id");
                            if (string.IsNullOrEmpty(strParentID) == true)
                            {
                                strParentID = GetParentID(strItemReacord);

                                if (string.IsNullOrEmpty(strParentID) == false)
                                    (node as XmlElement).SetAttribute("parent_id", strParentID);
                            }

                            node.InnerXml = "";
                        }

                        // DomUtil.SetElementText(dom.DocumentElement, strElementName, "");
                        return;
                    }
                }
            }
        }

        // 从日志记录中，删除读者记录的 password 元素
        static void RemoveReaderPassword(ref XmlDocument dom)
        {
            // operation:borrow -- readerRecord
            // operation:return -- readerRecord
            // operation:changeReaderPassword -- readerRecord 注意 newPassword 元素里面有新密码的 hash 字符串
            // operation:changeReaderTempPassword -- readerRecord 注意 tempPassword 元素里面有新密码的 hash 字符串
            // operation:setReaderInfo -- record 和 oldRecord 元素
            // operation:amerce -- readerRecord 和 oldReaderRecord 元素
            // operation:devolveReaderInfo -- sourceReaderRecord 和 targetRecordRecord 元素
            // operation:hire -- readerRecord
            // operation:foregift -- readerRecord
            // !TODO: operation:writeRes 要留意它是不是写入了读者库。如果是，则要防范泄露 password 元素内容
            // operation:setUser -- account 和 oldAccount 元素里面的 password 元素; 根元素的 newPassword 元素
            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            if (strOperation == "borrow"
    || strOperation == "return"
    || strOperation == "hire"
    || strOperation == "foregift")
            {
                RemoveReaderPassword(ref dom, "readerRecord");
                return;
            }
            if (strOperation == "changeReaderPassword")
            {
                RemoveReaderPassword(ref dom, "readerRecord");
                DomUtil.DeleteElement(dom.DocumentElement, "newPassword");
                return;
            }
            if (strOperation == "changeReaderTempPassword")
            {
                RemoveReaderPassword(ref dom, "readerRecord");
                DomUtil.DeleteElement(dom.DocumentElement, "tempPassword");
                return;
            }
            if (strOperation == "setReaderInfo")
            {
                RemoveReaderPassword(ref dom, "record");
                RemoveReaderPassword(ref dom, "oldRecord");
                return;
            }
            if (strOperation == "amerce")
            {
                RemoveReaderPassword(ref dom, "readerRecord");
                RemoveReaderPassword(ref dom, "oldReaderRecord");
                return;
            }
            if (strOperation == "devolveReaderInfo")
            {
                RemoveReaderPassword(ref dom, "sourceReaderRecord");
                RemoveReaderPassword(ref dom, "targetReaderRecord");
                return;
            }
            if (strOperation == "setUser")
            {
                DomUtil.DeleteElement(dom.DocumentElement, "newPassword");
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("account | oldAccount");
                foreach (XmlElement account in nodes)
                {
                    account.RemoveAttribute("password");
                }
                return;
            }
        }

        static void RemoveReaderPassword(ref XmlDocument dom, string strElementName)
        {
            string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, strElementName);
            if (string.IsNullOrEmpty(strReaderRecord))
                return;
            XmlDocument reader_dom = new XmlDocument();
            try
            {
                reader_dom.LoadXml(strReaderRecord);
                XmlNode node = reader_dom.DocumentElement.SelectSingleNode("password");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                    DomUtil.SetElementText(dom.DocumentElement, strElementName, reader_dom.DocumentElement.OuterXml);
                }
            }
            catch
            {
            }
        }

        static void RemoveReaderRecord(int nLevel,
            string strElementName,
            ref XmlDocument dom)
        {
            if (nLevel == 0)
                return;

            {
                string strReaderRecord = DomUtil.GetElementText(dom.DocumentElement, strElementName);
                if (string.IsNullOrEmpty(strReaderRecord) == false)
                {
                    if (nLevel == 2)
                    {
                        DomUtil.SetElementText(dom.DocumentElement, strElementName, "");
                        return;
                    }

                    // nLevel == 1
                    XmlDocument reader_dom = new XmlDocument();
                    try
                    {
                        reader_dom.LoadXml(strReaderRecord);
                        XmlNode node = reader_dom.DocumentElement.SelectSingleNode("borrowHistory");
                        if (node != null)
                            node.ParentNode.RemoveChild(node);
                        DomUtil.SetElementText(dom.DocumentElement, strElementName, reader_dom.DocumentElement.OuterXml);
                    }
                    catch
                    {
                    }
                }
            }
        }

        static void RemoveAmerceRecord(int nLevel,
    string strElementName,
    ref XmlDocument dom)
        {
            if (nLevel == 0)
                return;

            {
                string strReaderReacord = DomUtil.GetElementText(dom.DocumentElement, strElementName);
                if (string.IsNullOrEmpty(strReaderReacord) == false)
                {
                    if (nLevel == 2)
                    {
                        DomUtil.SetElementText(dom.DocumentElement, strElementName, "");
                        return;
                    }
                }
            }
        }

        // 过滤借阅的日志记录
        static void ResizeBorrow(int nLevel, ref XmlDocument dom)
        {
            RemoveReaderRecord(nLevel, "readerRecord", ref dom);
            RemoveEntityRecord(nLevel, "itemRecord", ref dom);
        }

        // 过滤还书的日志记录
        static void ResizeReturn(int nLevel, ref XmlDocument dom)
        {
            RemoveReaderRecord(nLevel, "readerRecord", ref dom);
            RemoveEntityRecord(nLevel, "itemRecord", ref dom);
        }

        static void ResizeSetEntity(int nLevel, ref XmlDocument dom)
        {
            // 对 <record> 元素最多删除 <borrowHistory>
            RemoveEntityRecord(nLevel > 1 ? 1 : nLevel, "record", ref dom);
            RemoveEntityRecord(nLevel, "oldRecord", ref dom);
        }

        static void ResizeSetOrder(int nLevel, ref XmlDocument dom)
        {
            RemoveItemRecord(nLevel, "oldRecord", ref dom);
        }

        static void ResizeSetIssue(int nLevel, ref XmlDocument dom)
        {
            RemoveItemRecord(nLevel, "oldRecord", ref dom);
        }

        static void ResizeSetComment(int nLevel, ref XmlDocument dom)
        {
            RemoveItemRecord(nLevel, "oldRecord", ref dom);
        }

        static void ResizeSetReaderInfo(int nLevel, ref XmlDocument dom)
        {
            // 对 <record> 元素最多删除 <borrowHistory>
            RemoveReaderRecord(nLevel > 1 ? 1 : nLevel, "record", ref dom);

            // 如果 <record> 中记录为空，则需要从 <oldRecord> 中挪用
            if (nLevel > 1)
            {
                string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
                if (strAction == "move" || strAction == "copy")
                {
                    string strReaderReacord = DomUtil.GetElementText(dom.DocumentElement, "record");
                    if (string.IsNullOrEmpty(strReaderReacord) == true)
                    {
                        RemoveReaderRecord(nLevel > 1 ? 1 : nLevel, "oldRecord", ref dom);
                        return;
                    }
                }
            }

            RemoveReaderRecord(nLevel, "oldRecord", ref dom);
        }

        static void ResizeSetBiblioInfo(int nLevel, ref XmlDocument dom)
        {
            RemoveBiblioRecord(nLevel > 1 ? 1 : nLevel, "record", ref dom);

            // 如果 <record> 中记录为空，则需要从 <oldRecord> 中挪用
            if (nLevel > 1)
            {
                string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");
                if (strAction == "move" || strAction == "copy"
                    || strAction == "onlymovebiblio" || strAction == "onlycopybiblio")
                {
                    string strBiblioReacord = DomUtil.GetElementText(dom.DocumentElement, "record");
                    if (string.IsNullOrEmpty(strBiblioReacord) == true)
                    {
                        RemoveBiblioRecord(nLevel > 1 ? 1 : nLevel, "oldRecord", ref dom);
                        return;
                    }
                }
            }

            RemoveBiblioRecord(nLevel, "oldRecord", ref dom);
        }

        static void ResizeAmerce(int nLevel, ref XmlDocument dom)
        {
            RemoveReaderRecord(nLevel, "readerRecord", ref dom);
            // 注: oldReaderRecord 要用来体现修改前的 overdue 金额，因而不能清除 2016/5/21
            // RemoveReaderRecord(nLevel, "oldReaderRecord", ref dom);
        }

        static void ResizeHire(int nLevel, ref XmlDocument dom)
        {
            RemoveReaderRecord(nLevel, "readerRecord", ref dom);
        }

        static void ResizeForegift(int nLevel, ref XmlDocument dom)
        {
            RemoveReaderRecord(nLevel, "readerRecord", ref dom);
        }

        static void ResizeSettlement(int nLevel, ref XmlDocument dom)
        {
            RemoveAmerceRecord(nLevel, "oldAmerceRecord", ref dom);
        }

        static void ResizeChangeReaderPassword(int nLevel, ref XmlDocument dom)
        {
            RemoveReaderRecord(nLevel, "readerRecord", ref dom);
        }

        // 2012/9/23
        // 获得一个日志记录的附件片断
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strFileName 纯文件名,不含路径部分。但要包括".log"部分。
        //      lIndex  记录序号。从0开始计数。lIndex为-1时调用本函数，表示希望获得整个文件尺寸值，将返回在lHintNext中。
        //      lHint   记录位置暗示性参数。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。
        //              目前的含义是记录起始位置。
        //      nAttachmentFragmentLength   要读出的附件内容字节数。如果为 -1，表示尽可能多读出内容
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public int GetOperLogAttachment(
            string strLibraryCodeList,
            string strFileName,
            long lIndex,
            long lHint,
            long lAttachmentFragmentStart,
            int nAttachmentFragmentLength,
            out byte[] attachment_data,
            out long lAttachmentLength,
            out string strError)
        {
            strError = "";
            attachment_data = null;
            long lHintNext = -1;
            lAttachmentLength = 0;

            int nRet = 0;

            Stream stream = null;

            if (string.IsNullOrEmpty(this.m_strDirectory) == true)
            {
                strError = "日志目录 m_strDirectory 尚未初始化";
                return -1;
            }
            Debug.Assert(this.m_strDirectory != "", "");

            string strFilePath = this.m_strDirectory + "\\" + strFileName;

            try
            {
                stream = File.Open(
                    strFilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "日志文件 " + strFileName + "没有找到";
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "打开日志文件 '" + strFileName + "' 发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                long lFileSize = 0;
                // 定位

                // 加锁
                // 在获得文件整个长度的过程中，必须要小心提防另外并发的正在对文件进行写的操作
                bool bLocked = false;

                // 如果读取的是当前正在写入的热点日志文件，则需要加锁（读锁）
                if (PathUtil.IsEqual(strFilePath, this.m_strFileName) == true)
                {
                    ////Debug.WriteLine("begin read lock 2");
                    this.m_lock.AcquireReaderLock(m_nLockTimeout);
                    bLocked = true;
                }

                try
                {   // begin of lock try
                    lFileSize = stream.Length;

                }   // end of lock try
                finally
                {
                    if (bLocked == true)
                    {
                        this.m_lock.ReleaseReaderLock();
                        ////Debug.WriteLine("end read lock 2");
                    }
                }
                // lIndex == -1表示希望获得文件整个的尺寸
                if (lIndex == -1)
                {
                    lHintNext = lFileSize;  // stream.Length;
                    return 1;   // 成功
                }

                // 没有暗示，只能从头开始找
                if (lHint == -1 || lIndex == 0)
                {
                    // return:
                    //      -1  error
                    //      0   成功
                    //      1   到达文件末尾或者超出
                    nRet = LocationRecord(stream,
                        lFileSize,
                        lIndex,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                        return 2;
                }
                else
                {
                    // 根据暗示找到
                    if (lHint == stream.Length)
                        return 2;

                    if (lHint > stream.Length)
                    {
                        strError = "lHint参数值不正确";
                        return -1;
                    }
                    if (stream.Position != lHint)
                        stream.Seek(lHint, SeekOrigin.Begin);
                }


                // parameters:
                //      nAttachmentFragmentLength   要读出的附件内容字节数。如果为 -1，表示尽可能多读出内容
                // return:
                //      -1  出错
                //      >=0 整个附件的尺寸
                lAttachmentLength = ReadEnventLogAttachment(
                    stream,
                    lAttachmentFragmentStart,
                    nAttachmentFragmentLength,
                    out attachment_data,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 无法限制记录观察范围
                END1:
                lHintNext = stream.Position;

                return 1;
            }
            finally
            {
                stream.Close();
            }
        }

        const int MAX_FILENAME_COUNT = 100;

        // parameters:
        //      nCount  本次希望获取的记录数。如果==-1，表示希望尽可能多地获取
        //      strStyle    如果不包含 supervisor，则本函数会自动过滤掉日志记录中读者记录的 password 字段
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围，本次调用无效
        public int GetOperLogs(
            string strLibraryCodeList,
            string strFileName,
            long lIndex,
            long lHint,
            int nCount,
            string strStyle,
            string strFilter,
            out OperLogInfo[] records,
            out string strError)
        {
            records = null;
            strError = "";
            List<OperLogInfo> results = new List<OperLogInfo>();

#if WRITE_LOG
            this.App.WriteDebugInfo($"GetOperLogs() strFileName{strFileName} lIndex={lIndex} nCount={nCount} strStyle={strStyle} strFilter={strFilter}");
#endif


            if (StringUtil.IsInList("getfilenames", strStyle) == true)
            {
                DirectoryInfo di = new DirectoryInfo(this.m_strDirectory);
                FileInfo[] fis = di.GetFiles("????????.log");

                if (fis.Length == 0)
                {
                    records = new OperLogInfo[0];   // 2019/7/31 补充。避免前端处理起来麻烦
                    return 0;   // 一个文件也没有
                }

                // 日期小者在前
                Array.Sort(fis, new FileInfoCompare(true));

                int nStart = (int)lIndex;
                int nEnd = fis.Length;
                if (nCount == -1)
                    nEnd = fis.Length;
                else
                    nEnd = Math.Min(nStart + nCount, fis.Length);

                // 一次不让超过最大数量
                if (nEnd - nStart > MAX_FILENAME_COUNT)
                    nEnd = nStart + MAX_FILENAME_COUNT;
                for (int i = nStart; i < nEnd; i++)
                {
                    OperLogInfo info = new OperLogInfo();
                    info.Index = i;
                    info.Xml = fis[i].Name;
                    info.AttachmentLength = fis[i].Length;
                    results.Add(info);
                }

                records = new OperLogInfo[results.Count];
                results.CopyTo(records);
                return fis.Length;  // 返回事项总数
            }

            int nPackageLength = 0;

            string strXml = "";
            long lAttachmentLength = 0;
            long lHintNext = -1;
            for (int i = 0; i < nCount || nCount == -1; i++)
            {
#if WRITE_LOG
                this.App.WriteDebugInfo($"Call {i} GetOperLog() strFileName{strFileName} lIndex={lIndex} strStyle={strStyle} strFilter={strFilter}");
#endif
                // return:
                //      -1  error
                //      0   file not found
                //      1   succeed
                //      2   超过范围
                int nRet = GetOperLog(
                    strLibraryCodeList,
                    strFileName,
                    lIndex,
                    lHint,
                    strStyle,
                    strFilter,
                    out lHintNext,
                    out strXml,
                    out lAttachmentLength,
                    out strError);

#if WRITE_LOG
                this.App.WriteDebugInfo($"Call GetOperLog() nRet={nRet} strXml={strXml}");
#endif

                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 0;
                if (nRet == 2)
                {
                    if (i == 0)
                        return 2;   // 本次调用无效
                    break;
                }

                nPackageLength += strXml.Length + 100;  // 边角尺寸

#if WRITE_LOG
                this.App.WriteDebugInfo($"nPackageLength={nPackageLength}");
#endif

                if (nPackageLength > 500 * 1024
                    && i > 0)
                {
#if WRITE_LOG
                    this.App.WriteDebugInfo($"break");
#endif
                    break;
                }

                OperLogInfo info = new OperLogInfo();
                info.Index = lIndex;
                info.HintNext = lHintNext;
                info.Xml = strXml;
                info.AttachmentLength = lAttachmentLength;
                results.Add(info);

                lIndex++;
                lHint = lHintNext;
            }

            records = new OperLogInfo[results.Count];
            results.CopyTo(records);

#if WRITE_LOG
            this.App.WriteDebugInfo($"records.Length={records.Length}");
#endif
            return 1;
        }

        // 获得一个日志记录
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strFileName 纯文件名,不含路径部分。但要包括".log"部分。
        //      lIndex  记录序号。从0开始计数。lIndex为-1时调用本函数，表示希望获得整个文件尺寸值，将返回在lHintNext中。
        //      lHint   记录位置暗示性参数。这是一个只有服务器才能明白含义的值，对于前端来说是不透明的。
        //              目前的含义是记录起始位置。
        //      strStyle    如果不包含 supervisor，则本函数会自动过滤掉日志记录中读者记录的 password 字段
        //                  如果包含 dont_return_xml 表示在 strXml 不返回内容
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围。本次调用无效
        public int GetOperLog(
            string strLibraryCodeList,
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            string strFilter,
            out long lHintNext,
            out string strXml,
            out long lAttachmentLength,
            out string strError)
        {
            strError = "";
            strXml = "";
            lHintNext = -1;
            lAttachmentLength = 0;

            int nRet = 0;

            CacheFileItem cache_item = null;

            if (string.IsNullOrEmpty(this.m_strDirectory) == true)
            {
                strError = "日志目录 m_strDirectory 尚未初始化";
                return -1;
            }
            Debug.Assert(this.m_strDirectory != "", "");

            string strFilePath = this.m_strDirectory + "\\" + strFileName;

            // 是否需要获得总记录数
            bool bGetCount = StringUtil.IsInList("getcount", strStyle) == true;

            try
            {
                cache_item = this.Cache.Open(strFilePath);
                /*
                stream = File.Open(
                    strFilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
                 * */
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "日志文件 " + strFileName + "没有找到";
                lHintNext = 0;
                return 0;   // file not found
            }
            catch (Exception ex)
            {
                strError = "打开日志文件 '" + strFileName + "' 发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                long lFileSize = 0;
                // 定位

                // 加锁
                // 在获得文件整个长度的过程中，必须要小心提防另外并发的正在对文件进行写的操作
                bool bLocked = false;

                // 如果读取的是当前正在写入的热点日志文件，则需要加锁（读锁）
                if (PathUtil.IsEqual(strFilePath, this.m_strFileName) == true)
                {
                    ////Debug.WriteLine("begin read lock 3");
                    this.m_lock.AcquireReaderLock(m_nLockTimeout);
                    bLocked = true;
                }
                try
                {   // begin of lock try

                    lFileSize = cache_item.Stream.Length;

#if WRITE_LOG
                    FileInfo fi = new FileInfo(strFilePath);

                    this.App.WriteDebugInfo($"lFileSize={lFileSize} fi.Length={fi.Length} (2)");
#endif
                }   // end of lock try
                finally
                {
                    if (bLocked == true)
                    {
                        this.m_lock.ReleaseReaderLock();
                        ////Debug.WriteLine("end read lock 3");
                    }
                }

                // lIndex == -1表示希望获得文件整个的尺寸
                if (lIndex == -1)
                {
                    if (bGetCount == false)
                    {
                        lHintNext = lFileSize;  // cache_item.Stream.Length;
                        return 1;   // 成功
                    }

                    // 获得记录总数
                    // parameters:
                    // return:
                    //      -1  error
                    //      >=0 记录总数
                    lHintNext = GetRecordCount(cache_item.Stream,
                        lFileSize,
                        out strError);
                    if (lHintNext == -1)
                        return -1;

                    return 1;   // 成功
                }

                // 没有暗示，只能从头开始找
                if (lHint == -1 || lIndex == 0)
                {
                    // return:
                    //      -1  error
                    //      0   成功
                    //      1   到达文件末尾或者超出
                    nRet = LocationRecord(cache_item.Stream,
                        lFileSize,
                        lIndex,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                        return 2;
                }
                else
                {
                    // 根据暗示找到
                    if (lHint == cache_item.Stream.Length)
                        return 2;

                    if (lHint > cache_item.Stream.Length)
                    {
                        strError = "lHint参数值不正确";
                        return -1;
                    }
                    if (cache_item.Stream.Position != lHint)
                        cache_item.Stream.Seek(lHint, SeekOrigin.Begin);
                }

                /////

                // MemoryStream attachment = null; // new MemoryStream();
                // TODO: 是否可以优化为，先读出XML部分，如果需要再读出attachment? 并且attachment可以按需读出分段
                // return:
                //      1   出错
                //      0   成功
                //      1   文件结束，本次读入无效
                nRet = ReadEnventLog(
                    cache_item.Stream,
                    lFileSize,
                    true,
                    out strXml,
                    out lAttachmentLength,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 后面会通过 strStyle 参数决定是否过滤读者记录的 password 元素

                // 限制记录观察范围
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
#if NO
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strXml内容装入XMLDOM时出错: " + ex.Message;
                        return -1;
                    }
                    XmlNode node = null;
                    string strLibraryCodes = DomUtil.GetElementText(dom.DocumentElement, "libraryCode", out node);
                    if (node == null)
                        goto END1;  // 不需要过滤
                    string strSourceLibraryCode = "";
                    string strTargetLibraryCode = "";
                    ParseLibraryCodes(strLibraryCodes,
    out strSourceLibraryCode,
    out strTargetLibraryCode);
                    if (StringUtil.IsInList(strTargetLibraryCode, strLibraryCodeList) == false)
                    {
                        strXml = "";    // 清空，让前端看不到内容
                        lAttachmentLength = 0;    // 清空附件
                    }
#endif
                    // 检查和过滤日志XML记录
                    // return:
                    //      -1  出错
                    //      0   不允许返回当前日志记录
                    //      1   允许返回当前日志记录
                    nRet = FilterXml(
                        strLibraryCodeList,
                        strStyle,
                        strFilter,
                        ref strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        nRet = 1;   // 只好返回
                        // return -1;
                    }
                    if (nRet == 0)
                    {
                        strXml = "";    // 清空，让前端看不到内容
                    }
                }
                else
                {
                    // 虽然是全局用户，也要限制记录尺寸
                    nRet = ResizeXml(
                        strStyle,
                        strFilter,
                        ref strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        nRet = 1;   // 只好返回
                        // return -1;
                    }
                }

                // END1:
                lHintNext = cache_item.Stream.Position;

                // 2017/12/5
                if (StringUtil.IsInList("dont_return_xml", strStyle) == true)
                    strXml = "";
                return 1;
            }
            finally
            {
                // stream.Close();
                this.Cache.Close(cache_item);
            }
        }

        // 解析左右两个部分
        // 图书馆1,图书馆2
        static void ParseLibraryCodes(string strText,
            out string strSource,
            out string strTarget)
        {
            strSource = "";
            strTarget = "";

            strText = strText.Trim();

            int nRet = strText.IndexOf(",");
            if (nRet == -1)
            {
                strSource = strText;
                strTarget = strText;
                return;
            }

            strSource = strText.Substring(0, nRet).Trim();
            strTarget = strText.Substring(nRet + 1).Trim();
        }

        // 根据记录编号，定位到记录起始位置
        // parameters:
        //      lMaxFileSize    文件最大尺寸。如果为 -1，表示不限制。如果不为 -1，表示需要在这个范围内探测
        // return:
        //      -1  error
        //      0   成功
        //      1   到达文件末尾或者超出
        static int LocationRecord(Stream stream,
            long lMaxFileSize,
            long lIndex,
            out string strError)
        {
            strError = "";

            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            for (long i = 0; i < lIndex; i++)
            {
                if (lMaxFileSize != -1
    && stream.Position >= lMaxFileSize)
                    return 1;

                byte[] length = new byte[8];

                int nRet = stream.Read(length, 0, 8);
                if (nRet < 8)
                {
                    strError = "起始位置不正确";
                    return -1;
                }

                Int64 lLength = BitConverter.ToInt64(length, 0);

                stream.Seek(lLength, SeekOrigin.Current);
            }

            if (lMaxFileSize != -1
&& stream.Position >= lMaxFileSize)
                return 1;
            if (stream.Position >= stream.Length)
                return 1;

            return 0;
        }

        // 2013/11/21
        // 获得记录总数
        // parameters:
        //      lMaxFileSize    文件最大尺寸。如果为 -1，表示不限制。如果不为 -1，表示需要在这个范围内探测
        // return:
        //      -1  error
        //      >=0 记录总数
        static long GetRecordCount(Stream stream,
            long lMaxFileSize,
            out string strError)
        {
            strError = "";

            if (stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            for (long i = 0; ; i++)
            {
                if (lMaxFileSize != -1
                    && stream.Position >= lMaxFileSize)
                    return i;

                byte[] length = new byte[8];

                int nRet = stream.Read(length, 0, 8);
                if (nRet == 0)
                    return i;

                if (nRet < 8)
                {
                    strError = "起始位置不正确";
                    return -1;
                }

                Int64 lLength = BitConverter.ToInt64(length, 0);

                stream.Seek(lLength, SeekOrigin.Current);
            }
        }

        // 从日志文件当前位置读出一条日志记录
        // 要读出附件
        // parameters:
        //      attachment  承载输出的附件部分的 Stream 对象。如果为 null，表示不输出附件部分
        //                  本函数返回后，attachment 的文件指针在文件末尾。调用时需引起注意
        // return:
        //      1   出错
        //      0   成功
        //      1   文件结束，本次读入无效
        public static int ReadEnventLog(
            Stream stream,
            out string strXmlBody,
            // ref Stream attachment,
            Stream attachment,
            out string strError)
        {
            strError = "";
            strXmlBody = "";

            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet == 0)
                return 1;
            if (nRet < 8)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lRecordLength = BitConverter.ToInt64(length, 0);

            if (lRecordLength == 0)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始读入了8个byte，其整数值为0，表明日志文件出现了错误";
                return -1;
            }

            Debug.Assert(lRecordLength != 0, "");

            string strMetaData = "";

            // 读出xml事项
            nRet = ReadEntry(stream,
                true,
                out strMetaData,
                out strXmlBody,
                out strError);
            if (nRet == -1)
                return -1;

            // 读出attachment事项
            nRet = ReadEntry(
                stream,
                out strMetaData,
                // ref attachment,
                attachment,
                out strError);
            if (nRet == -1)
                return -1;

            // 文件指针自然指向末尾位置
            // this.m_stream.Seek(lRecordLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lRecordLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "Record长度经检验不正确: stream.Position - lStart ["
                    + (stream.Position - lStart).ToString()
                    + "] 不等于 lRecordLength + 8 ["
                    + (lRecordLength + 8).ToString()
                    + "]";
                return -1;
            }

            return 0;
        }

        // 2012/9/23
        // 从日志文件当前位置读出一条日志记录
        // 只探测附件的长度，并不读出附件
        // parameters:
        //      lMaxFileSize    文件最大尺寸。如果为 -1，表示不限制。如果不为 -1，表示需要在这个范围内探测
        //      bRead   是否真正要读出信息。 == false 表示不读出信息，只是验证一下结构
        // return:
        //      1   出错
        //      0   成功
        //      1   文件结束，本次读入无效
        public static int ReadEnventLog(
            Stream stream,
            long lMaxFileSize,
            bool bRead,
            out string strXmlBody,
            out long lAttachmentLength,
            out string strError)
        {
            strError = "";
            strXmlBody = "";
            lAttachmentLength = 0;

            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet == 0)
                return 1;
            if (lMaxFileSize != -1
    && stream.Position >= lMaxFileSize)
                return 1;
            if (nRet < 8)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lRecordLength = BitConverter.ToInt64(length, 0);

            if (lRecordLength == 0)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始读入了8个byte，其整数值为0，表明日志文件出现了错误";
                return -1;
            }

            Debug.Assert(lRecordLength != 0, "");

            string strMetaData = "";

            // 读出xml事项
            nRet = ReadEntry(stream,
                bRead,
                out strMetaData,
                out strXmlBody,
                out strError);
            if (nRet == -1)
                return -1;

            // 读出attachment事项
            nRet = ReadEntry(
                stream,
                bRead,
                out strMetaData,
                out lAttachmentLength,
                out strError);
            if (nRet == -1)
                return -1;

            // 文件指针自然指向末尾位置
            // this.m_stream.Seek(lRecordLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lRecordLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "Record长度经检验不正确: stream.Position - lStart ["
                    + (stream.Position - lStart).ToString()
                    + "] 不等于 lRecordLength + 8 ["
                    + (lRecordLength + 8).ToString()
                    + "]";
                return -1;
            }

            return 0;
        }

        // 2012/9/23
        // 从日志文件当前位置读出一条日志记录的附件部分
        // parameters:
        //      nAttachmentFragmentLength   要读出的附件内容字节数。如果为 -1，表示尽可能多读出内容
        // return:
        //      -1  出错
        //      >=0 整个附件的尺寸
        public static long ReadEnventLogAttachment(
            Stream stream,
            long lAttachmentFragmentStart,
            int nAttachmentFragmentLength,
            out byte[] attachment_data,
            out string strError)
        {
            strError = "";
            attachment_data = null;
            long lAttachmentLength = 0;

            long lStart = stream.Position;	// 记忆起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始试图读入8个byte，但是只读入了 " + nRet.ToString() + " 个。起始位置不正确";
                return -1;
            }

            Int64 lRecordLength = BitConverter.ToInt64(length, 0);

            if (lRecordLength == 0)
            {
                strError = "ReadEnventLog()从偏移量 " + lStart.ToString() + " 开始读入了8个byte，其整数值为0，表明日志文件出现了错误";
                return -1;
            }

            Debug.Assert(lRecordLength != 0, "");

            string strMetaData = "";
            string strXmlBody = "";

            // 读出xml事项
            nRet = ReadEntry(stream,
                true,
                out strMetaData,
                out strXmlBody,
                out strError);
            if (nRet == -1)
                return -1;

            // 读出attachment事项
            // parameters:
            //      nAttachmentFragmentLength   要读出的附件内容字节数。如果为 -1，表示尽可能多读出内容
            // return:
            //      -1  出错
            //      >=0 整个附件的尺寸
            lAttachmentLength = ReadEntry(
                stream,
                out strMetaData,
                lAttachmentFragmentStart,
                nAttachmentFragmentLength,
                out attachment_data,
                out strError);
            if (lAttachmentLength == -1)
                return -1;

            // 文件指针自然指向末尾位置
            // this.m_stream.Seek(lRecordLength, SeekOrigin.Current);

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lRecordLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "Record长度经检验不正确: stream.Position - lStart ["
                    + (stream.Position - lStart).ToString()
                    + "] 不等于 lRecordLength + 8 ["
                    + (lRecordLength + 8).ToString()
                    + "]";
                return -1;
            }

            return lAttachmentLength;
        }

        // 读出一个事项(string类型)
        // parameters:
        public static int ReadEntry(
            Stream stream,
            bool bRead,
            out string strMetaData,
            out string strBody,
            out string strError)
        {
            strMetaData = "";
            strBody = "";
            strError = "";

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "起始位置不正确";
                return -1;
            }

            Int64 lEntryLength = BitConverter.ToInt64(length, 0);

            // metadata长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "metadata长度位置不足8bytes";
                return -1;
            }

            Int64 lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength > 100 * 1024)
            {
                strError = "记录格式不正确，metadata长度超过100K";
                return -1;
            }

            if (lMetaDataLength > 0)
            {
                if (bRead)
                {
                    byte[] metadatabody = new byte[(int)lMetaDataLength];

                    nRet = stream.Read(metadatabody, 0, (int)lMetaDataLength);
                    if (nRet < (int)lMetaDataLength)
                    {
                        strError = "metadata不足其长度定义";
                        return -1;
                    }

                    strMetaData = Encoding.UTF8.GetString(metadatabody);
                }
                else
                    stream.Seek(lMetaDataLength, SeekOrigin.Current);
            }




            // strBody长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "strBody长度位置不足8bytes";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength > 1000 * 1024)
            {
                strError = "记录格式不正确，body长度超过1000K";
                return -1;
            }

            if (lBodyLength > 0)
            {
                if (bRead)
                {
                    byte[] xmlbody = new byte[(int)lBodyLength];

                    nRet = stream.Read(xmlbody, 0, (int)lBodyLength);
                    if (nRet < (int)lBodyLength)
                    {
                        strError = "body不足其长度定义";
                        return -1;
                    }

                    strBody = Encoding.UTF8.GetString(xmlbody);
                }
                else
                    stream.Seek(lBodyLength, SeekOrigin.Current);

            }

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lEntryLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "entry长度经检验不正确";
                return -1;
            }

            return 0;
        }

        // 读出一个事项(Stream类型)
        // parameters:
        //      streamBody  承载输出的 body 部分的 Stream 对象。如果为 null，表示不输出这部分
        //                  本函数返回后，streamBody 的文件指针在文件末尾。调用时需引起注意
        public static int ReadEntry(
            Stream stream,
            out string strMetaData,
            // ref Stream streamBody,
            Stream streamBody,
            out string strError)
        {
            strError = "";
            strMetaData = "";

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "起始位置不正确";
                return -1;
            }

            Int64 lEntryLength = BitConverter.ToInt64(length, 0);

            // metadata长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "metadata长度位置不足8bytes";
                return -1;
            }

            Int64 lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength > 100 * 1024)
            {
                strError = "记录格式不正确，metadata长度超过100K";
                return -1;
            }

            if (lMetaDataLength > 0)
            {
                byte[] metadatabody = new byte[(int)lMetaDataLength];

                nRet = stream.Read(metadatabody, 0, (int)lMetaDataLength);
                if (nRet < (int)lMetaDataLength)
                {
                    strError = "metadata不足其长度定义";
                    return -1;
                }

                strMetaData = Encoding.UTF8.GetString(metadatabody);
            }

            // body长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "strBody长度位置不足8bytes";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength > stream.Length - stream.Position)
            {
                strError = "记录格式不正确，body长度超过文件剩余部分尺寸";
                return -1;
            }

            if (lBodyLength > 0)
            {
                if (streamBody == null)
                {
                    // 优化
                    stream.Seek(lBodyLength, SeekOrigin.Current);
                }
                else
                {
                    // 把数据dump到输出流中
                    int chunk_size = 4096;
                    byte[] chunk = new byte[chunk_size];
                    long writed_length = 0;
                    for (; ; )
                    {
                        int nThisSize = Math.Min(chunk_size, (int)(lBodyLength - writed_length));
                        int nReaded = stream.Read(chunk, 0, nThisSize);
                        if (nReaded < nThisSize)
                        {
                            strError = "读入不足";
                            return -1;
                        }

                        if (streamBody != null)
                            streamBody.Write(chunk, 0, nReaded);

                        writed_length += nReaded;
                        if (writed_length >= lBodyLength)
                            break;
                    }
                }
            }

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lEntryLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "entry长度经检验不正确";
                return -1;
            }

            return 0;
        }

        // 2012/9/23
        // 读出一个事项(byte []类型)
        // parameters:
        //      nAttachmentFragmentLength   要读出的附件内容字节数。如果为 -1，表示尽可能多读出内容
        // return:
        //      -1  出错
        //      >=0 整个附件的尺寸
        public static long ReadEntry(
            Stream stream,
            out string strMetaData,
            long lAttachmentFragmentStart,
            int nAttachmentFragmentLength,
            out byte[] attachment_data,
            out string strError)
        {
            strError = "";
            strMetaData = "";
            attachment_data = null;

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "起始位置不正确";
                return -1;
            }

            Int64 lEntryLength = BitConverter.ToInt64(length, 0);

            // metadata长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "metadata长度位置不足8bytes";
                return -1;
            }

            Int64 lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength > 100 * 1024)
            {
                strError = "记录格式不正确，metadata长度超过100K";
                return -1;
            }

            if (lMetaDataLength > 0)
            {
                byte[] metadatabody = new byte[(int)lMetaDataLength];

                nRet = stream.Read(metadatabody, 0, (int)lMetaDataLength);
                if (nRet < (int)lMetaDataLength)
                {
                    strError = "metadata不足其长度定义";
                    return -1;
                }

                strMetaData = Encoding.UTF8.GetString(metadatabody);
            }

            // body长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "strBody长度位置不足8bytes";
                return -1;
            }

            Int64 lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength > stream.Length - stream.Position)
            {
                strError = "记录格式不正确，body长度超过文件剩余部分尺寸";
                return -1;
            }

            if (lBodyLength > 0)
            {
                if (nAttachmentFragmentLength > 0 || nAttachmentFragmentLength == -1)   // 2017/5/16 添加的 -1
                {
                    long lTemp = (lBodyLength - lAttachmentFragmentStart);
                    // 尽量多读入
                    if (nAttachmentFragmentLength == -1)
                    {
                        // 看看是否超过每次的限制尺寸
                        nAttachmentFragmentLength = (int)Math.Min((long)(100 * 1024), lTemp);
                    }
                    else
                    {
                        // 2017/5/16 确保 nAttachmentFragmentLength 不超过附件剩余部分长度
                        nAttachmentFragmentLength = (int)(Math.Min(lTemp, (long)nAttachmentFragmentLength));
                    }

                    attachment_data = new byte[nAttachmentFragmentLength];
                    stream.Seek(lAttachmentFragmentStart, SeekOrigin.Current);
                    int nReaded = stream.Read(attachment_data, 0, nAttachmentFragmentLength);
                    if (nReaded < nAttachmentFragmentLength)
                    {
                        strError = "读入不足";
                        return -1;
                    }

                    if (lAttachmentFragmentStart + nAttachmentFragmentLength < lBodyLength)
                    {
                        // 确保文件指针在读完的位置
                        stream.Seek(lBodyLength - (lAttachmentFragmentStart + nAttachmentFragmentLength), SeekOrigin.Current);
                    }
                }
                else
                    stream.Seek(lBodyLength, SeekOrigin.Current);

            }

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lEntryLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "entry长度经检验不正确";
                return -1;
            }

            return lBodyLength;
        }

        // 2012/9/23
        // 读出一个事项(只观察长度)
        public static int ReadEntry(
            Stream stream,
            bool bRead,
            out string strMetaData,
            out long lBodyLength,
            out string strError)
        {
            strError = "";
            strMetaData = "";
            lBodyLength = 0;

            long lStart = stream.Position;  // 保留起始位置

            byte[] length = new byte[8];

            int nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "起始位置不正确";
                return -1;
            }

            Int64 lEntryLength = BitConverter.ToInt64(length, 0);

#if NO
            if (lEntryLength == 0)
            {
                // Debug.Assert(false, "");
                // 文件指针此时自然在末尾
                if (stream.Position - lStart != lEntryLength + 8)
                {
                    strError = "entry长度经检验不正确 1";
                    return -1;
                }

                return 0;
            }
#endif

            // metadata长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "metadata长度位置不足8bytes";
                return -1;
            }

            Int64 lMetaDataLength = BitConverter.ToInt64(length, 0);

            if (lMetaDataLength > 100 * 1024)
            {
                strError = "记录格式不正确，metadata长度超过100K";
                return -1;
            }

            if (lMetaDataLength > 0)
            {
                if (bRead)
                {
                    byte[] metadatabody = new byte[(int)lMetaDataLength];

                    nRet = stream.Read(metadatabody, 0, (int)lMetaDataLength);
                    if (nRet < (int)lMetaDataLength)
                    {
                        strError = "metadata不足其长度定义";
                        return -1;
                    }

                    strMetaData = Encoding.UTF8.GetString(metadatabody);
                }
                else
                    stream.Seek(lMetaDataLength, SeekOrigin.Current);
            }

            // body长度
            nRet = stream.Read(length, 0, 8);
            if (nRet < 8)
            {
                strError = "strBody长度位置不足8bytes";
                return -1;
            }

            lBodyLength = BitConverter.ToInt64(length, 0);

            if (lBodyLength > stream.Length - stream.Position)
            {
                strError = "记录格式不正确，body长度超过文件剩余部分尺寸";
                return -1;
            }

            if (lBodyLength > 0)
            {
                // 虽然不读内容，但文件指针要到位
                stream.Seek(lBodyLength, SeekOrigin.Current);
            }

            // 文件指针此时自然在末尾
            if (stream.Position - lStart != lEntryLength + 8)
            {
                // Debug.Assert(false, "");
                strError = "entry长度经检验不正确";
                return -1;
            }

            return 0;
        }

        public class FileInfoCompare : IComparer
        {
            public bool Asc = true;

            public FileInfoCompare(bool bAsc)
            {
                this.Asc = bAsc;
            }

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(Object x, Object y)
            {
                return (this.Asc == false ? -1 : 1) * ((new CaseInsensitiveComparer()).Compare(((FileInfo)x).Name, ((FileInfo)y).Name));
            }
        }

        // return:
        //      -1  运行出错
        //      0   没有错误
        //      1   有错误
        public int VerifyLogFiles(
            bool bRepair,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.m_strDirectory) == true)
            {
                strError = "尚未指定操作日志目录";
                return -1;
            }

            // 列出所有日志文件
            DirectoryInfo di = new DirectoryInfo(this.m_strDirectory);

            FileInfo[] fis = di.GetFiles("????????.log");
            if (fis.Length == 0)
                return 0;

            // 日期大者在前
            Array.Sort(fis, new FileInfoCompare(false));

            DateTime now = DateTime.Now;
            string strToday = now.ToString("yyyyMMdd") + ".log";
            bool bFound = false;

            List<string> filenames = new List<string>();

            // 获得靠前的最多两个文件名
            foreach (FileInfo fi in fis)
            {
                if (strToday == fi.Name)
                    bFound = true;
                filenames.Add(fi.FullName);

                if (filenames.Count >= 2)
                    break;
            }

            // 加入当天的日志文件名
            // 如果目录中存在一个超大号码的文件名，加入当天的日志文件名可以增强可靠性
            if (bFound == false)
            {
                string strFileName = Path.Combine(this.m_strDirectory, strToday);
                if (File.Exists(strFileName) == true)
                    filenames.Add(strFileName);
            }

            string strErrorText = "";
            foreach (string strFileName in filenames)
            {
                // return:
                //      -1  出错
                //      0   没有错误
                //      1   有错误
                int nRet = VerifyLogFile(strFileName,
                    bRepair,
                    out strError);
                if (nRet == -1)
                {
                    strError = "验证操作日志文件 '" + strFileName + "' 时发生运行错误: " + strError;
                    return -1;
                }
                if (nRet == 1)
                    strErrorText += strError + " ";
            }

            if (string.IsNullOrEmpty(strErrorText) == false)
            {
                strError = strErrorText;
                return 1;
            }

            return 0;
        }

#if NO
        // return:
        //      -1  出错
        //      0   没有错误
        //      1   有错误
        int VerifyLogFile(string strSourceFilename,
            bool bRepair,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strSourceFilename) == true)
            {
                strError = "源文件名不能为空";
                return -1;
            }

            try
            {
                using (Stream source = File.Open(
                        strSourceFilename,
                        FileMode.Open,
                        FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                        FileShare.ReadWrite))
                {
                    long lStart = 0;
                    for (long i = 0; ; i++)
                    {
                        lStart = source.Position;

                        byte[] length = new byte[8];

                        nRet = source.Read(length, 0, 8);
                        if (nRet == 0)
                            break;
                        if (nRet < 8)
                        {
                            strError = "剩余尺寸不足 8 bytes。";
                            if (bRepair == true)
                            {
                                source.SetLength(lStart);
                                strError += "已经将文件在位置 "+lStart.ToString()+" 截断。";
                            }
                            return 1;
                        }

                        Int64 lLength = BitConverter.ToInt64(length, 0);

                        if (source.Position + lLength > source.Length)
                        {
                            strError = "头部 8 bytes 存储的数字太大，超过文件当前尾部。";
                            if (bRepair == true)
                            {
                                source.SetLength(lStart);
                                strError += "已经将文件在位置 " + lStart.ToString() + " 截断。";
                            }
                            return 1;
                        }

                        source.Seek(lLength, SeekOrigin.Current);
                    }
                }
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "源日志文件 " + strSourceFilename + "没有找到";
                return -1;   // file not found
            }
            catch (Exception ex)
            {
                strError = "操作源日志文件 '" + strSourceFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            return 0;
        }
#endif

        // return:
        //      -1  出错
        //      0   没有错误
        //      1   有错误
        int VerifyLogFile(string strSourceFilename,
            bool bRepair,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strSourceFilename) == true)
            {
                strError = "源文件名不能为空";
                return -1;
            }

            try
            {
                using (Stream source = File.Open(
                        strSourceFilename,
                        FileMode.Open,
                        FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                        FileShare.ReadWrite))
                {
                    long lStart = 0;
                    for (long i = 0; ; i++)
                    {
                        lStart = source.Position;

                        string strXmlBody = "";
                        long lAttachmentLength = 0;

                        // 从日志文件当前位置读出一条日志记录
                        // 只探测附件的长度，并不读出附件
                        // return:
                        //      1   出错
                        //      0   成功
                        //      1   文件结束，本次读入无效
                        nRet = ReadEnventLog(
            source,
            -1,
            false,  // 不读入数据
            out strXmlBody,
            out lAttachmentLength,
            out strError);
                        if (nRet == -1)
                        {
                            if (bRepair == true)
                            {
                                source.SetLength(lStart);
                                strError = "文件 " + strSourceFilename + " " + strError + " 已经将文件在位置 " + lStart.ToString() + " 截断。";
                            }
                            return 1;   // TODO: 两个以上文件都坏了的可能性很小?
                        }
                        if (nRet == 1)
                            break;
                    }
                }
            }
            catch (FileNotFoundException /*ex*/)
            {
                strError = "源日志文件 " + strSourceFilename + "没有找到";
                return -1;   // file not found
            }
            catch (Exception ex)
            {
                strError = "操作源日志文件 '" + strSourceFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        // 将全部小文件合并到大文件
        // return:
        //      -1  运行出错
        //      0   没有错误
        //      1   有错误
        public int MergeTempLogFiles(
            bool bVerifySmallFiles,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(this.m_strDirectory) == true)
            {
                strError = "尚未指定操作日志目录";
                return -1;
            }

            // 列出所有日志文件
            DirectoryInfo di = new DirectoryInfo(this.m_strDirectory);

            FileInfo[] fis = di.GetFiles("*.tlog");
            if (fis.Length == 0)
                return 0;

            List<string> filenames = new List<string>();
            foreach (FileInfo fi in fis)
            {
                filenames.Add(fi.FullName);
            }

            // 日期小者在前
            filenames.Sort();

            string strErrorText = "";
            foreach (string strFileName in filenames)
            {
                if (bVerifySmallFiles == true)
                {
                    // return:
                    //      -1  出错
                    //      0   没有错误
                    //      1   有错误
                    nRet = VerifyLogFile(strFileName,
                        true,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "验证操作日志文件 '" + strFileName + "' 时发生运行错误: " + strError;
                        return -1;
                    }
                    if (nRet == 1)
                        strErrorText += strError + " ";
                }

                string strPureFileName = Path.GetFileNameWithoutExtension(strFileName);
                if (strPureFileName.Length < 8)
                    continue;
                string strBigFileName = Path.Combine(Path.GetDirectoryName(strFileName), strPureFileName.Substring(0, 8) + ".log");

                try
                {
                    using (Stream target = File.Open(
    strBigFileName,
    FileMode.OpenOrCreate,
    FileAccess.ReadWrite,
    FileShare.ReadWrite))
                    {
                        target.Seek(0, SeekOrigin.End);

                        using (Stream source = File.Open(
    strFileName,
    FileMode.OpenOrCreate,
    FileAccess.ReadWrite,
    FileShare.ReadWrite))
                        {
                            StreamUtil.DumpStream(source, target);
                        }
                    }

                    File.Delete(strFileName);

                    this.App.WriteErrorLog("成功合并临时日志文件 " + Path.GetFileName(strFileName) + "  到 " + Path.GetFileName(strBigFileName));
                }
                catch (Exception ex)
                {
                    strError = "合并临时日志文件 " + strFileName + "  到 " + strBigFileName + " 的过程中出现异常：" + ex.Message;

                    // 通知系统挂起
                    //this.App.HangupReason = HangupReason.OperLogError;

                    this.App.WriteErrorLog("系统启动时，试图合并临时日志文件，但是这一努力失败了 [" + strError + "]。请试着为数据目录腾出更多富余磁盘空间，然后重新启动系统。");
                    this.App.AddHangup("OperLogError");
                    return -1;
                }
            }

            if (string.IsNullOrEmpty(strErrorText) == false)
            {
                strError = strErrorText;
                return 1;
            }

            return 0;
        }
    }

    // API GetOperLogs()所使用的结构
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class OperLogInfo
    {
        [DataMember]
        public long Index = -1; // 日志记录序号
        [DataMember]
        public long HintNext = -1; // 下一记录暗示

        [DataMember]
        public string Xml = ""; // 日志记录XML
        [DataMember]
        public long AttachmentLength = 0;   // 附件尺寸
    }
}
