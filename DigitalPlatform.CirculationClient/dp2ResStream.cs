using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 把一个 dp2library 对象模拟为可以读的Stream
    /// </summary>
    public class dp2ResStream : Stream
    {
        static Semaphore _limit = new Semaphore(2, 2);

        CancellationTokenSource _cancel = new CancellationTokenSource();

        public IChannelManager Manager { get; set; }

        // public LibraryChannel Channel { get; set; }

        public string ResPath { get; set; }

        long m_lLength = 0;	// 长度
        long m_lCurrent = 0;	// 文件指针当前位置

        //int _inSearch = 0;

        ProgressChanged _progressChanged = null;

        byte[] _timestamp = null;

        public dp2ResStream(IChannelManager manager,
            string strObjectPath,
            ProgressChanged progressFunc)
        {
            this.Manager = manager;
            this.ResPath = strObjectPath;
            this._progressChanged = progressFunc;


            // 获得元数据。最重要的是长度

            string strError = "";
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            string strMetaData = "";
            string strOutputPath;
            byte[] baOutputTimeStamp = null;
            byte[] content = null;

            if (WaitHandle.WaitAny(new WaitHandle[] { _limit, _cancel.Token.WaitHandle }) == 1)
                throw new Exception("canceled");

            // Thread.Sleep(1000); // testing

            //_limit.WaitOne();
            LibraryChannel Channel = this.GetChannel();
            //_inSearch++;
            try
            {

                // 获得媒体类型
                long lRet = Channel.GetRes(
                    null,
                    this.ResPath,
                    0,
                    0,
                    "metadata,timestamp",
                    out content,
                    out strMetaData,
                    out strOutputPath,
                    out baOutputTimeStamp,
                    out strError);
                if (lRet == -1)
                {
                    strError = "GetRes() (for metadata) Error : " + strError;
                    throw new Exception(strError);
                }

                _timestamp = baOutputTimeStamp;
            }
            finally
            {
                //_inSearch--;

                this.ReturnChannel(Channel);
                _limit.Release();
            }

            // 取metadata中的mime类型信息
            Hashtable values = StringUtil.ParseMetaDataXml(strMetaData,
                out strError);

            if (values == null)
            {
                strError = "ParseMedaDataXml() Error :" + strError;
                throw new Exception(strError);
            }

            string strMime = (string)values["mimetype"];
            string strSize = (string)values["size"];
            if (String.IsNullOrEmpty(strSize) == false)
            {
                this.m_lLength = Convert.ToInt64(strSize);
                TriggerProgressChanged();
            }
        }

        void TriggerProgressChanged()
        {
            if (this._progressChanged != null)
                this._progressChanged(this.ResPath, this.m_lCurrent, this.m_lLength);
        }

        public override void Close()
        {
            _cancel.Cancel();

#if NO
            if (this.Channel != null || this.Manager != null)
            {
                if (_inSearch > 0)
                    this.Channel.Abort();
                this.Manager.ReturnChannel(this.Channel);
                this.Channel = null;
            }
#endif
            StopChannels();
        }

        public LibraryChannel GetChannel()
        {
            LibraryChannel channel = this.Manager.GetChannel();
            lock (syncRoot)
            {
                _channelList.Add(channel);
            }
            // TODO: 检查数组是否溢出
            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this.Manager.ReturnChannel(channel);
            lock (syncRoot)
            {
                _channelList.Remove(channel);
            }
        }

        List<LibraryChannel> _channelList = new List<LibraryChannel>();
        private static readonly Object syncRoot = new Object();

        public void StopChannels()
        {
            lock (syncRoot)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {

        }

        public override long Length
        {
            get
            {
                return Length;  // TODO: ???
            }
        }

        public override long Position
        {
            get
            {
                return m_lCurrent;
            }
            set
            {
                m_lCurrent = value;
            }

        }

        public override int Read(byte[] buffer,
            int offset,
            int count)
        {
            if (m_lCurrent >= m_lLength)
                return 0;

            string strError = "";
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            string strMetaData = "";
            string strOutputPath;
            byte[] baOutputTimeStamp = null;

            if (WaitHandle.WaitAny(new WaitHandle[] { _limit, _cancel.Token.WaitHandle }) == 1)
                return 0;

            // Thread.Sleep(1000); // testing

            //_limit.WaitOne();
            LibraryChannel Channel = this.GetChannel();
            //_inSearch++;
            try
            {
                int fechted = 0;    // 已经获取的字节数
                for (; ; )
                {
                    byte[] content = null;
                    long lRet = Channel.GetRes(
                        null,
                        this.ResPath,
                        m_lCurrent,
                        count - fechted,
                        "data,timestamp",   // TODO: 中途比对 timesamp，若发生变化则抛出异常
                        out content,
                        out strMetaData,
                        out strOutputPath,
                        out baOutputTimeStamp,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetRes() (for metadata) Error : " + strError;
                        throw new Exception(strError);
                    }

                    if (ByteArray.Compare(_timestamp, baOutputTimeStamp) != 0)
                        throw new Exception("下载中途对象的时间戳发生了变化"); // TODO: 或者尝试重新从头开始？

                    Array.Copy(content, 0, buffer, offset + fechted, content.Length);

                    fechted += content.Length;
                    m_lCurrent += content.Length;
                    TriggerProgressChanged();

                    if (fechted >= count || m_lCurrent >= lRet)
                        break;
                }
                return fechted;
            }
            finally
            {
                //_inSearch--;

                this.ReturnChannel(Channel);
                _limit.Release();
            }
        }

        public override long Seek(
            long offset,
            SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                m_lCurrent = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                m_lCurrent += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                m_lCurrent = m_lLength - offset;
            }
            else
            {
                throw (new Exception("不支持的origin参数"));
            }

            TriggerProgressChanged();
            return m_lCurrent;
        }

        public override void Write(
            byte[] buffer,
            int offset,
            int count
            )
        {
            throw (new NotSupportedException("PartStream不支持Write()"));
        }

        public override void SetLength(long value)
        {
            throw (new NotSupportedException("PartStream不支持SetLength()"));
        }

    }

}
