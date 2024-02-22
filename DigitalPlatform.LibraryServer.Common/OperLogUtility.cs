using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer.Common
{
    public static class OperLogUtility
    {
        #region 写日志记录

        // 不带 lHintNext 的版本
        public static int AppendOperLog(
    OperLogFileCache fileCache,
    string strDirectory,
    string strFileName,
    string strXmlBody,
    Stream attachment,
    out string strError)
        {
            return AppendOperLog(
            fileCache,
            strDirectory,
            strFileName,
            strXmlBody,
            attachment,
            out _,
            out strError);
        }

        // 追加一条日志记录
        // 操作完成后，fileCache 中文件指针处在插入这条记录的末尾位置
        // 注: 如果日志文件不存在，会新创建一个日志文件
        // parameters:
        //      strXmlBody  要追加的日志记录的 XML 部分
        //      attachment  要追加的日志记录携带的附件
        //      lHintNext   [out] 返回当前记录被插入后，下一条记录的起始偏移量。也就是文件末尾
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public static int AppendOperLog(
            OperLogFileCache fileCache,
            string strDirectory,
            string strFileName,
            string strXmlBody,
            Stream attachment,
            out long lHintNext,
            out string strError)
        {
            lHintNext = -1;
            // 如果文件不存在，则预先创建文件
            string strFilePath = Path.Combine(strDirectory, strFileName);
            if (File.Exists(strFilePath) == false)
            {
                try
                {
                    using (var stream = File.Create(strFilePath))
                    {

                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    strError = $"目录 '{strDirectory}' 不存在，无法创建日志文件";
                    return -1;
                }
            }
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围
            return InsertOperLog(
                fileCache,
                strDirectory,
                strFileName,
                -1,
                -1,
                strXmlBody,
                attachment,
                out lHintNext,
                out strError);
        }

        // 插入一条日志记录
        // 操作完成后，fileCache 中文件指针处在新插入这条记录的末端位置
        // parameters:
        //      lIndex  日志记录在日志文件中的序号。从 0 开始计算
        //      lHint   暗示的偏移量。
        //              如果为 -1 表示不使用此参数
        //              当 lHint 中不为 -1 时，优先使用 lHint，忽略 lIndex 参数值
        //      strXmlBody  要插入的日志记录的 XML 部分
        //      attachment  要插入的日志记录携带的附件
        //      lHintNext   [out] 返回当前记录被插入后，下一条记录的起始偏移量。
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public static int InsertOperLog(
            OperLogFileCache fileCache,
            string strDirectory,
            string strFileName,
            long lIndex,
            long lHint,
            string strXmlBody,
            Stream attachment,
            out long lHintNext,
            out string strError)
        {
            // TODO: 如果 attachment 尺寸太大，可以考虑将日志文件写入一个物理文件的临时文件(而不是 MemoryStream)
            if (attachment != null
                && attachment.Length > 5 * 1024 * 1024)
                throw new ArgumentException("attachment 尺寸太大。不应超过 5M");

            using (var stream = new MemoryStream())
            {
                WriteEnventLog(
        stream,
        strXmlBody,
        attachment);
                stream.Seek(0, SeekOrigin.Begin);

                return ReplaceOperLog(fileCache,
                    strDirectory,
                    strFileName,
                    lIndex,
                    lHint,
                    "insert",
                    stream,
                    out lHintNext,
                    out strError);
            }
        }

        // 删除一条日志记录
        // 操作完成后，fileCache 中文件指针处在删除前这条记录的开始位置
        // parameters:
        //      lIndex  日志记录在日志文件中的序号。从 0 开始计算
        //      lHint   暗示的偏移量。
        //              如果为 -1 表示不使用此参数
        //              当 lHint 中不为 -1 时，优先使用 lHint，忽略 lIndex 参数值
        // parameters:
        //      lHintNext   [out] 把当前记录删除掉，那么函数返回时 lHintNext 实际上指向了当前记录删除前的那个开始位置
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public static int DeleteOperLog(
            OperLogFileCache fileCache,
            string strDirectory,
            string strFileName,
            long lIndex,
            long lHint,
            out long lHintNext,
            out string strError)
        {
            return ReplaceOperLog(
                fileCache,
                strDirectory,
                strFileName,
                lIndex,
                lHint,
                "",
                (Stream)null,
                out lHintNext,
                out strError);
        }

        // 根据指定的 xml 和附件，替换一个已经存在的日志记录
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public static int ReplaceOperLog(
            OperLogFileCache fileCache,
            string strDirectory,
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            string strXmlBody,
            Stream attachment,
            out long lHintNext,
            out string strError)
        {
            lHintNext = -1;
            /*
            // 如果文件不存在，则预先创建文件
            string strFilePath = Path.Combine(strDirectory, strFileName);
            if (File.Exists(strFilePath) == false)
            {
                try
                {
                    using (var stream = File.Create(strFilePath))
                    {

                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    strError = $"目录 '{strDirectory}' 不存在，无法创建日志文件";
                    return -1;
                }
            }
            */

            // TODO: 如果 attachment 尺寸太大，可以考虑将日志文件写入一个物理文件的临时文件(而不是 MemoryStream)
            if (attachment != null
                && attachment.Length > 5 * 1024 * 1024)
                throw new ArgumentException("attachment 尺寸太大。不应超过 5M");

            using (var stream = new MemoryStream())
            {
                WriteEnventLog(
        stream,
        strXmlBody,
        attachment);
                stream.Seek(0, SeekOrigin.Begin);

                return ReplaceOperLog(fileCache,
                    strDirectory,
                    strFileName,
                    lIndex,
                    lHint,
                    strStyle,
                    stream,
                    out lHintNext,
                    out strError);
            }
        }

        // byte [] 版本 替换一个日志记录
        // parameters:
        //      lHintNext   [out] 返回当前记录被替换后，下一条记录的起始偏移量。
        //                  注意如果 new_content 为 null，表示把当前记录删除掉，那么函数返回时 lHintNext 实际上指向了当前记录删除前的那个开始位置。否则是下一条记录的开始位置
        public static int ReplaceOperLog(
    OperLogFileCache fileCache,
    string strDirectory,
    string strFileName,
    long lIndex,
    long lHint,
    string strStyle,
    byte[] new_content,
    out long lHintNext,
    out string strError)
        {
            if (new_content == null)
                return ReplaceOperLog(fileCache,
    strDirectory,
    strFileName,
    lIndex,
    lHint,
    strStyle,
    (Stream)null,
    out lHintNext,
    out strError);
            using (var stream = new MemoryStream(new_content))
            {
                return ReplaceOperLog(fileCache,
                    strDirectory,
                    strFileName,
                    lIndex,
                    lHint,
                    strStyle,
                    stream,
                    out lHintNext,
                    out strError);
            }
        }

        // 替换一条日志记录
        // parameters:
        //      lIndex  日志记录在日志文件中的序号。从 0 开始计算
        //      lHint   暗示的偏移量。
        //              如果为 -1 表示不使用此参数
        //              当 lHint 中不为 -1 时，优先使用 lHint，忽略 lIndex 参数值
        //      strStyle    如果包含 insert，表示希望在指定位置插入记录，而不是替换记录
        //      new_content 要替换当前记录的新内容。如果为 null 或者 .Length == 0，相当于删除了当前记录
        //              注意这个 Stream 的全部内容都会被使用。调用前不要求文件指针在文件头部
        //      lHintNext   [out] 返回当前记录被替换后，下一条记录的起始偏移量。
        //                  注意如果 new_content 为 null，表示把当前记录删除掉，那么函数返回时 lHintNext 实际上指向了当前记录删除前的那个开始位置。否则是下一条记录的开始位置
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围
        public static int ReplaceOperLog(
            OperLogFileCache fileCache,
            string strDirectory,
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            Stream new_content,
            out long lHintNext,
            out string strError)
        {
            strError = "";
            lHintNext = -1;

            int nRet = 0;

            var insert = StringUtil.IsInList("insert", strStyle);

            var current_cache = fileCache;
            if (current_cache == null)
                current_cache = new OperLogFileCache();

#if OLD
            Stream stream = null;
#else
            Stream stream = null;
            CacheFileItem cache_item = null;
#endif

            if (string.IsNullOrEmpty(strDirectory))
            {
                strError = "strDirectory 参数值不应为空";
                return -1;
            }

            string strFilePath = Path.Combine(strDirectory, strFileName);

            try
            {

#if OLD
                stream = File.Open(
                    strFilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
#else

                cache_item = current_cache.Open(strFilePath);
                stream = cache_item.Stream;
#endif
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

                {   // begin of lock try
                    lFileSize = stream.Length;

#if WRITE_LOG
                    FileInfo fi = new FileInfo(strFilePath);

                    this.App.WriteDebugInfo($"lFileSize={lFileSize} fi.Length={fi.Length} (1)");
#endif

                }   // end of lock try

                long start_offs = -1;

                // lIndex == -1表示希望获得文件整个的尺寸
                if (lIndex == -1)
                {
                    // throw new ArgumentException("lIndex 参数值不允许小于 0");
                    stream.Seek(0, SeekOrigin.End);
                    start_offs = stream.Position;
                }
                else
                {
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

                    start_offs = stream.Position;

                    if (insert == false)
                    {
                        // return:
                        //      1   出错
                        //      0   成功
                        //      1   文件结束，本次读入无效
                        nRet = ReadEnventLog(
                            stream,
                            out _,
                            null,   // attachment,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

                Debug.Assert(start_offs != -1);

                long end_offs = stream.Position;    // 本条记录的末端

                // long old_content_length = (end_offs - start_offs);
                long new_content_length = new_content == null ? 0 : new_content.Length;

                // 已经到了文件末尾，并且要替换的内容为空
                if (lIndex == -1
                    && start_offs == stream.Position
                    && new_content_length == 0)
                {
                    return 1;
                }

                long new_end_offs = start_offs + new_content_length;

                if (end_offs >= stream.Length)
                    stream.SetLength(new_end_offs);
                else
                {
                    // 移动前，文件的总尺寸
                    long old_length = stream.Length;
                    // 注意，往左移动的时候，此函数不会缩小文件尺寸
                    StreamUtil.Move(stream,
                        end_offs,
                        old_length - end_offs,
                        new_end_offs);
                    // 移动后，文件应有的总尺寸
                    long new_length = new_end_offs + (old_length - end_offs);
                    if (new_length < old_length)
                        stream.SetLength(new_length);
                }
                if (new_content != null && new_content.Length > 0)
                {
                    stream.Seek(start_offs, SeekOrigin.Begin);
                    new_content.Seek(0, SeekOrigin.Begin);
                    StreamUtil.DumpStream(new_content, stream);
                }
                else
                    stream.Seek(new_end_offs, SeekOrigin.Begin);
                lHintNext = stream.Position;
                return 1;
            }
            finally
            {
#if OLD
                stream.Close();
#else
                current_cache.Close(cache_item);
                if (fileCache == null)
                    current_cache.Close();
#endif
            }
        }


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

        #endregion

        #region 读日志记录

        public delegate int delegate_filterRecord(
            /*
            string strLibraryCodeList,
            string strStyle,
            string strFilter,
            */
            ref string strXml,
            out string strError);

        // 注: 这个函数旧版本的缺点是，每次都要重新打开一下文件。新版本改用了 CacheFileItem
        // 原先版本
        // 获得一个日志记录
        // parameters:
        //      fileCache       文件缓存机制。可以加快日志记录读取速度。如果设置为 null，表示不使用文件缓存
        //      strDirectory    操作日志文件所在目录。如果为空，表示使用 m_strDirectory
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
        public static int GetOperLog(
            OperLogFileCache fileCache,
            string strDirectory,
            // string strLibraryCodeList,
            string strFileName,
            long lIndex,
            long lHint,
            string strStyle,
            string strFilter,
            delegate_filterRecord func_filterRecord,
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

            var current_cache = fileCache;
            if (current_cache == null)
                current_cache = new OperLogFileCache();

#if OLD
            Stream stream = null;
#else
            Stream stream = null;
            CacheFileItem cache_item = null;
#endif

            if (string.IsNullOrEmpty(strDirectory))
            {
                strError = "strDirectory 参数值不应为空";
                return -1;
            }
            /*
            if (string.IsNullOrEmpty(strDirectory))
            {
                if (string.IsNullOrEmpty(this.m_strDirectory) == true)
                {
                    strError = "日志目录 m_strDirectory 尚未初始化";
                    return -1;
                }
                Debug.Assert(this.m_strDirectory != "", "");
                strDirectory = this.m_strDirectory;
            }
            */

            string strFilePath = Path.Combine(strDirectory, strFileName);

            try
            {

#if OLD
                stream = File.Open(
                    strFilePath,
                    FileMode.Open,
                    FileAccess.ReadWrite, // Read会造成无法打开 2007/5/22
                    FileShare.ReadWrite);
#else

                cache_item = current_cache.Open(strFilePath);
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

                /*
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
                */

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
                    /*
                    if (bLocked == true)
                    {
                        this.m_lock.ReleaseReaderLock();
                        ////Debug.WriteLine("end read lock 1");
                    }
                    */
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

                if (func_filterRecord != null)
                {
                    nRet = func_filterRecord(
                        /*
                        strLibraryCodeList,
                        strStyle,
                        strFilter,
                        */
                        ref strXml,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
#if REMOVED
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
#endif

                lHintNext = stream.Position;
                return 1;
            }
            finally
            {
#if OLD
                stream.Close();
#else
                current_cache.Close(cache_item);
                if (fileCache == null)
                    current_cache.Close();
#endif
            }
        }

        public static long GetOperLogCount(
            string strDirectory,
            string strFileName)
        {
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            var ret = GetOperLogCount(null,
                strDirectory,
                strFileName,
                out long count,
                out string strError);
            if (ret != 1)
                return -1;
            return count;
        }

        // 获得指定日志文件中包含的日志记录条数
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        public static int GetOperLogCount(OperLogFileCache fileCache,
            string strDirectory,
            string strFileName,
            out long count,
            out string strError)
        {
            strError = "";
            count = 0;

            var current_cache = fileCache;
            if (current_cache == null)
                current_cache = new OperLogFileCache();

            Stream stream = null;
            CacheFileItem cache_item = null;

            if (string.IsNullOrEmpty(strDirectory))
            {
                strError = "strDirectory 参数值不应为空";
                return -1;
            }

            string strFilePath = Path.Combine(strDirectory, strFileName);

            try
            {

                cache_item = current_cache.Open(strFilePath);
                stream = cache_item.Stream;
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
                long lFileSize = stream.Length;
                long lIndex = 0;
                while (true)
                {
                    // return:
                    //      -1  error
                    //      0   成功
                    //      1   到达文件末尾或者超出
                    int nRet = LocationRecord(stream,
                        lFileSize,
                        lIndex++,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                        return 1;
                    count++;
                }
            }
            finally
            {
                current_cache.Close(cache_item);
                if (fileCache == null)
                    current_cache.Close();
            }
        }

        // 根据记录编号，定位到记录起始位置
        // parameters:
        //      lMaxFileSize    文件最大尺寸。如果为 -1，表示不限制。如果不为 -1，表示需要在这个范围内探测
        // return:
        //      -1  error
        //      0   成功
        //      1   到达文件末尾或者超出
        public static int LocationRecord(Stream stream,
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



        #endregion
    }
}
