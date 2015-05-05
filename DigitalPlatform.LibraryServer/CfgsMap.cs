using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;

namespace DigitalPlatform.LibraryServer
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
            PathUtil.CreateDirIfNeed(this.RootDir);

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
            PathUtil.CreateDirIfNeed(this.RootDir);
        }

        // 将内核网络配置文件映射到本地
        // return:
        //      -1  出错
        //      0   不存在
        //      1   找到
        public int MapFileToLocal(
            RmsChannelCollection Channels,
            string strPath,
            out string strLocalPath,
            out string strError)
        {
            strLocalPath = "";
            strError = "";

            strLocalPath = this.RootDir + "/" + strPath;

            // 确保目录存在
            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strLocalPath));

            this.locks.LockForRead(strLocalPath);
            try {
                // 看看物理文件是否存在
                FileInfo fi = new FileInfo(strLocalPath);
                if (fi.Exists == true) {
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
            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strLocalPath));


            this.locks.LockForWrite(strLocalPath);
            try
            {
                RmsChannel channel = Channels.GetChannel(this.ServerUrl);
                if (channel == null)
                {
                    strError = "GetChannel error";
                    return -1;
                }

                string strMetaData = "";
                byte[] baOutputTimestamp = null;
                string strOutputPath = "";

                long lRet = channel.GetRes(strPath,
                    strLocalPath,
                    (Stop)null,
                    out strMetaData,
                    out baOutputTimestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        // 为了避免以后再次从网络获取耗费时间, 需要在本地写一个0字节的文件
                        FileStream fs = File.Create(strLocalPath);
                        fs.Close();
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
