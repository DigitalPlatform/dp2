using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using DigitalPlatform.Marc;

namespace dp2Catalog
{
    /// <summary>
    /// 和MARC记录窗连接的ISO2709文件。
    /// 可以前后翻看记录。
    /// </summary>
    public class LinkMarcFile
    {
        List<long> heads = new List<long>(); // 记录头部偏移量数组

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName = "";


        Stream _file = null; // 文件流

        /// <summary>
        /// 文件的 Stream 对象
        /// </summary>
        public Stream Stream
        {
            get
            {
                return this._file;
            }
        }

        /// <summary>
        /// MARC 格式。
        /// 为 unimarc/usmarc 之一
        /// </summary>
        public string MarcSyntax = "unimarc";

        /// <summary>
        /// 编码方式
        /// </summary>
        public Encoding Encoding = Encoding.GetEncoding(936);   // GB2312

        /// <summary>
        /// 当前记录的索引。
        /// -1 表示尚未初始化
        /// </summary>
        public int CurrentIndex = -1;  // 当前记录索引

        // 打开文件
        // return:
        //      -1  error
        //      0   succeed
        public int Open(string strFilename,
            out string strError)
        {
            strError = "";

            if (this._file != null)
                this.Close();

            this._file = File.Open(
                strFilename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            /*
            this.file = File.Open(strFilename,
                FileMode.Open,
                FileAccess.Read);
             * */
            if (_file == null)
            {
                strError = "文件 '" + strFilename + "' 打开失败";
                return -1;
            }

            this.FileName = strFilename;

            return 0;
        }

        // 获得下一条记录
        // return:
        //      -1  error
        //      0   succeed
        //      1   reach end(当前返回的记录有效)
        //	    2	结束(当前返回的记录无效)
        public int NextRecord(out string strMARC,
            out byte [] baRecord,
            out string strError)
        {
            strError = "";
            strMARC = "";
            baRecord = null;

            int nRet = 0;

            int index = this.CurrentIndex + 1;

            if (index < this.heads.Count)
                _file.Position = heads[index];
            else
            {
                // 记忆头部
                this.heads.Add(_file.Position);
            }

            // 从ISO2709文件中读入一条MARC记录
            // return:
            //	-2	MARC格式错
            //	-1	出错
            //	0	正确
            //	1	结束(当前返回的记录有效)
            //	2	结束(当前返回的记录无效)
            nRet = MarcUtil.ReadMarcRecord(_file,
                Encoding,
                true,	// bRemoveEndCrLf,
                true,	// bForce,
                out strMARC,
                out baRecord,
                out strError);
            if (nRet == -2 || nRet == -1)
            {
                strError = "读入MARC记录(" + index.ToString() + ")出错: " + strError;
                return -1;
            }

#if NO
            if (nRet != 0 && nRet != 1)
                return 1;
#endif
            // 2013/5/26
            if (nRet == 1 || nRet == 2)
            {
                if (nRet == 2)
                    strError = "到尾";
                return nRet;
            }

            this.CurrentIndex = index;

            return 0;
        }

        // 获得上一条记录
        // return:
        //      -1  error
        //      0   succeed
        //      1   reach head
        public int PrevRecord(out string strMARC,
            out byte[] baRecord,
            out string strError)
        {
            strError = "";
            strMARC = "";
            baRecord = null;
            int nRet = 0;

            if (this.CurrentIndex <= 0)
            {
                strError = "到头";
                return 1;
            }

            int index = this.CurrentIndex - 1;

            _file.Position = heads[index];

            // 从ISO2709文件中读入一条MARC记录
            // return:
            //	-2	MARC格式错
            //	-1	出错
            //	0	正确
            //	1	结束(当前返回的记录有效)
            //	2	结束(当前返回的记录无效)
            nRet = MarcUtil.ReadMarcRecord(_file,
                Encoding,
                true,	// bRemoveEndCrLf,
                true,	// bForce,
                out strMARC,
                out baRecord,
                out strError);
            if (nRet == -2 || nRet == -1)
            {
                strError = "读入MARC记录(" + index.ToString() + ")出错: " + strError;
                return -1;
            }

            if (nRet != 0 && nRet != 1)
                return 1;

            this.CurrentIndex = index;

            return 0;
        }

        // 重新获得当前记录
        // return:
        //      -1  error
        //      0   succeed
        //      1   reach head
        public int CurrentRecord(out string strMARC,
            out byte[] baRecord,
            out string strError)
        {
            strError = "";
            strMARC = "";
            baRecord = null;
            int nRet = 0;

            if (this.CurrentIndex < 0)
            {
                strError = "越过头部";
                return 1;
            }

            int index = this.CurrentIndex;

            _file.Position = heads[index];

            // 从ISO2709文件中读入一条MARC记录
            // return:
            //	-2	MARC格式错
            //	-1	出错
            //	0	正确
            //	1	结束(当前返回的记录有效)
            //	2	结束(当前返回的记录无效)
            nRet = MarcUtil.ReadMarcRecord(_file,
                Encoding,
                true,	// bRemoveEndCrLf,
                true,	// bForce,
                out strMARC,
                out baRecord,
                out strError);
            if (nRet == -2 || nRet == -1)
            {
                strError = "读入MARC记录(" + index.ToString() + ")出错: " + strError;
                return -1;
            }

            if (nRet != 0 && nRet != 1)
            {
                strError = "越过尾部";
                return 1;
            }

            // this.CurrentIndex = index;
            return 0;
        }

        public void Close()
        {
            if (this._file != null)
            {
                _file.Close();
                _file = null;
            }
        }
    }
}
