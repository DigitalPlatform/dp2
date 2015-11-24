using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using DigitalPlatform.IO;

namespace DigitalPlatform.rms.Client
{
#if NOOOOOOOOOOOOO
    /// <summary>
    /// 把一个对象模拟为可以读的Stream
    /// </summary>
    public class RmsStream : Stream
    {
        public RmsChannel RmsChannel = null;

        string m_strObjectPath;	// 对象路径
        long m_lLength = 0;	// 长度
        long m_lCurrent = 0;	// 文件指针当前位置


        public RmsStream()
        {
        }



        public RmsStream(RmsChannel channel,
            string strObjectPath,
            long lStart,
            long lLength)
        {
            this.RmsChannel = channel;
            this.m_strObjectPath = strObjectPath;

            // 获得元数据。最重要的是长度

            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            string strMetaData = "";
            string strOutputPath;
            byte[] baOutputTimeStamp = null;

            // 获得媒体类型
            long lRet = channel.GetRes(
                strPath,
                null,	// Response.OutputStream,
                stop,
                "metadata",
                null,	// byte [] input_timestamp,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "GetRes() (for metadata) Error : " + strError;
                return -1;
            }

            if (Page.Response.IsClientConnected == false)
                return -1;

            // 取metadata中的mime类型信息
            Hashtable values = ParseMedaDataXml(strMetaData,
                out strError);

            if (values == null)
            {
                strError = "ParseMedaDataXml() Error :" + strError;
                return -1;
            }

            string strMime = (string)values["mimetype"];
            string strClientPath = (string)values["localpath"];
            if (strClientPath != "")
                strClientPath = PathUtil.PureName(strClientPath);

            // TODO: 如果是非image/????类型，都要加入content-disposition
            // 是否出现另存为对话框
            if (bSaveAs == true)
            {
                string strEncodedFileName = HttpUtility.UrlEncode(strClientPath, Encoding.UTF8);
                Page.Response.AddHeader("content-disposition", "attachment; filename=" + strEncodedFileName);
            }

            /*
            Page.Response.AddHeader("Accept-Ranges", "bytes");
            Page.Response.AddHeader("Last-Modified", "Wed, 21 Nov 2007 07:10:54 GMT");
             * */

            // 用 text/plain IE XML 搜索google
            // http://support.microsoft.com/kb/329661
            // http://support.microsoft.com/kb/239750/EN-US/
            /*
To use this fix, you must add the following registry value to the key listed below: 
Key: HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings

Value name: IsTextPlainHonored
Value type: DWORD
Value data: HEX 0x1 
             * */

            /*

            Page.Response.CacheControl = "no-cache";    // 如果不用此句，text/plain会被当作xml文件打开
            Page.Response.AddHeader("Pragma", "no-cache");
            Page.Response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
//            Page.Response.AddHeader("Cache-Control", "public");
            Page.Response.AddHeader("Expires", "0");
            Page.Response.AddHeader("Content-Transfer-Encoding", "binary");
             * */


            // 设置媒体类型
            if (strMime == "text/plain")
                strMime = "text";
            Page.Response.ContentType = strMime;

            string strSize = (string)values["size"];
            if (String.IsNullOrEmpty(strSize) == false)
            {
                Page.Response.AddHeader("Content-Length", strSize);
            }
        }



        public override void Close()
        {
            if (m_stream != null)
                m_stream.Close();
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


        public override void Flush()
        {

        }

        public override long Length
        {
            get
            {
                return Length;
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

        public override int Read(/*[In,Out]*/ byte[] buffer,
            int offset,
            int count)
        {
            if (m_stream == null)
            {
                throw (new Exception("内部stream尚未打开..."));
            }

            if (m_lCurrent >= m_lLength)
                return 0;

            m_stream.Seek(m_lStart + m_lCurrent, SeekOrigin.Begin);

            long lMaxCount = m_lLength - m_lCurrent;
            if (count > (int)lMaxCount)
                count = (int)lMaxCount;

            int nRet = m_stream.Read(buffer, offset, count);
            if (nRet != count)
            {
                throw (new Exception("内部stream read异常..."));
            }

            m_lCurrent += count;
            return count;
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

#endif
}
