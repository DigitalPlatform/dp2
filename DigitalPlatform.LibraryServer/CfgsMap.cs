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

        // public string ServerUrl = "";

        RecordLockCollection locks = new RecordLockCollection();

        public CfgsMap(string strRootDir/*,
            string strServerUrl*/)
        {
            this.RootDir = strRootDir;
            PathUtil.TryCreateDir(this.RootDir);

            // this.ServerUrl = strServerUrl;

        }

        // 清除全部本地缓存的配置文件
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

            try
            {
                PathUtil.TryCreateDir(this.RootDir);
            }
            catch(Exception ex)
            {
                // 2020/4/24
                throw new Exception($"重新创建目录 '{this.RootDir}' 时出现异常：{ex.Message}", ex);
            }
        }

        // 清除一个本地缓存的配置文件
        // parameters:
        //      strPath 这是配置文件路径，例如 cfgs/summary.fltx
        // return:
        //      返回本地文件名
        public string Clear(string strPath)
        {
            string strLocalPath = this.RootDir + "/" + strPath;

            this.locks.LockForRead(strLocalPath);
            try
            {
                // 看看物理文件是否存在
                FileInfo fi = new FileInfo(strLocalPath);
                if (fi.Exists == true)
                {
                    try
                    {
                        // TODO: 注意安全性风险，要限制在 this.RootDir 以下的位置
                        File.Delete(strLocalPath);
                    }
                    catch(Exception ex)
                    {
                        string strText = ex.Message;
                    }
                }
            }
            finally
            {
                this.locks.UnlockForRead(strLocalPath);
            }

            return strLocalPath;
        }

        // 将内核网络配置文件映射到本地
        // return:
        //      -1  出错
        //      0   不存在
        //      1   找到
        public int MapFileToLocal(
            RmsChannel channel,
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
            PathUtil.TryCreateDir(Path.GetDirectoryName(strLocalPath));

            this.locks.LockForWrite(strLocalPath);
            try
            {
#if NO
                RmsChannel channel = Channels.GetChannel(this.ServerUrl);
                if (channel == null)
                {
                    strError = "GetChannel error";
                    return -1;
                }
#endif

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
                    if (channel.IsNotFound())
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
