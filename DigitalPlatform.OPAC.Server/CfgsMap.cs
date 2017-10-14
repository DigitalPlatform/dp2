using System.IO;

using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 映射内核配置文件到本地
    /// </summary>
    public class CfgsMap
    {
        public string RootDir = "";

        public string ServerUrl = "";

        RecordLockCollection locks = new RecordLockCollection();

        public CfgsMap(string strRootDir,
            string strServerUrl)
        {
            this.RootDir = strRootDir;
            PathUtil.TryCreateDir(this.RootDir);

            this.ServerUrl = strServerUrl;

        }

        public void Clear()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(this.RootDir);
                di.Delete(true);
            }
            catch
            {
            }
            PathUtil.TryCreateDir(this.RootDir);
        }

        // 将内核网络配置文件映射到本地
        // return:
        //      -1  出错
        //      0   不存在
        //      1   找到
        public int MapFileToLocal(
            LibraryChannel Channel,
            string strPath,
            out string strLocalPath,
            out string strError)
        {
            strLocalPath = "";
            strError = "";

            strLocalPath = this.RootDir + "/" + strPath;

            // 确保目录存在
            PathUtil.TryCreateDir(Path.GetDirectoryName(strLocalPath));

            this.locks.LockForRead(strLocalPath);
            try
            {
                // 看看物理文件是否存在
                FileInfo fi = new FileInfo(strLocalPath);
                if (fi.Exists == true)
                {
                    if (fi.Length == 0)
                        return 0;   // not exist
                    return 1;
                }
            }
            finally
            {
                this.locks.UnlockForRead(strLocalPath);
            }

            // 确保目录存在
            PathUtil.TryCreateDir(Path.GetDirectoryName(strLocalPath));

            this.locks.LockForWrite(strLocalPath);
            try
            {
                string strMetaData = "";
                byte[] baOutputTimestamp = null;
                string strOutputPath = "";

                long lRet = Channel.GetRes(
                    null,
            strPath,
            strLocalPath,
            "content,data,metadata,timestamp,outputpath,gzip",  // 2017/10/7 增加 gzip
            out strMetaData,
            out baOutputTimestamp,
            out strOutputPath,
            out strError);
                /*
                long lRet = channel.GetRes(strPath,
                    strLocalPath,
                    (Stop)null,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                 * */
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == LibraryClient.localhost.ErrorCode.NotFound)
                    {
                        // 为了避免以后再次从网络获取耗费时间, 需要在本地写一个0字节的文件
                        using(FileStream fs = File.Create(strLocalPath))
                        {

                        }
                        return 0;
                    }
                    return -1;
                }

                return 1;
            }
            finally
            {
                this.locks.UnlockForWrite(strLocalPath);
            }
        }
    }
}
