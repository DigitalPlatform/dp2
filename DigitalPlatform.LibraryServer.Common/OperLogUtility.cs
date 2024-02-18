using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.LibraryServer.Common
{
    public static class OperLogUtility
    {
        // 改进后版本
        // TODO: 如果能预先知道数据事项的长度，在开头就写好长度位，不用让文件指针往返，速度就更快了
        // 将日志写入文件
        // 不处理异常
        // parameters:
        //      attachment  附件。如果为 null，表示没有附件
        public static void WriteEnventLog(
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

    }
}
