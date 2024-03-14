using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Ionic.Zip;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Core;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// 和数据库管理有关的实用函数
    /// 2017/8/27
    /// </summary>
    public class DatabaseUtility
    {

        static void CopyTempFile(string strSourcePath,
    string strTempDir,
    string strDatabaseName)
        {
            if (string.IsNullOrEmpty(strTempDir))
                return;
            string strTarget = Path.Combine(strTempDir, strDatabaseName + "\\cfgs", Path.GetFileName(strSourcePath));
            PathUtil.TryCreateDir(Path.GetDirectoryName(strTarget));
            File.Copy(strSourcePath, strTarget);
        }

        public delegate void delegate_created(string strDatabaseName);

        // 根据数据库定义文件，创建若干数据库
        // 注: 如果发现同名数据库已经存在，先删除再创建
        // parameters:
        //      strTempDir  临时目录路径。调用前要创建好这个临时目录。调用后需要删除这个临时目录
        //      style       如果包含 continueLoop，在出错的时候会尽量继续循环向后处理
        public static int CreateDatabases(
            Stop stop,
            RmsChannel channel,
            string strDbDefFileName,
            string strTempDir,
            string style,
            delegate_created func_created,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            var continueLoop = StringUtil.IsInList("continueLoop", style);
            List<string> skip_warnings = new List<string>();

            // 展开压缩文件
            if (stop != null)
                stop.SetMessage("正在展开压缩文件 " + strDbDefFileName);
            try
            {
                using (ZipFile zip = ZipFile.Read(strDbDefFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        Debug.WriteLine(e.ToString());
                        e.Extract(strTempDir, ExtractExistingFileAction.OverwriteSilently);
                    }
                }
            }
            catch (Exception ex)
            {
                strError = "展开压缩文件 '" + strDbDefFileName + "' 到目录 '" + strTempDir + "' 时出现异常: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            DirectoryInfo di = new DirectoryInfo(strTempDir);

            DirectoryInfo[] dis = di.GetDirectories();

            int i = 0;
            foreach (DirectoryInfo info in dis)
            {
                // 跳过 '_datadir'
                if (info.Name == "_datadir")
                    continue;

                string strTemplateDir = Path.Combine(info.FullName, "cfgs");
                string strDatabaseName = info.Name;
                // 数据库是否已经存在？
                // return:
                //      -1  error
                //      0   not exist
                //      1   exist
                //      2   其他类型的同名对象已经存在
                nRet = DatabaseUtility.IsDatabaseExist(
                    channel,
                    strDatabaseName,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    if (stop != null)
                        stop.SetMessage("正在删除数据库 " + strDatabaseName);

                    // 如果发现同名数据库已经存在，先删除
                    long lRet = channel.DoDeleteDB(strDatabaseName, out strError);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound() == false)
                            return -1;
                    }
                }

                if (stop != null)
                    stop.SetMessage("正在创建数据库 " + strDatabaseName + " (" + (i + 1) + ")");

                // 根据数据库模板的定义，创建一个数据库
                // parameters:
                //      strTempDir  将创建数据库过程中，用到的配置文件会自动汇集拷贝到此目录。如果 == null，则不拷贝
                nRet = DatabaseUtility.CreateDatabase(channel,
            strTemplateDir,
            strDatabaseName,
            null,
            out strError);
                if (nRet == -1)
                {
                    if (continueLoop)
                    {
                        skip_warnings.Add(strError);
                        continue;
                    }
                    return -1;
                }

                func_created?.Invoke(strDatabaseName);
                i++;
            }

            // 2024/2/24
            if (skip_warnings.Count > 0)
            {
                strError = StringUtil.MakePathList(skip_warnings, "; ");
                return -1;
            }

            return 0;
        }

        // 根据数据库模板的定义，创建一个数据库
        // parameters:
        //      strTempDir  将创建数据库过程中，用到的配置文件会自动汇集拷贝到此目录。如果 == null，则不拷贝
        public static int CreateDatabase(RmsChannel channel,
    string strTemplateDir,
    string strDatabaseName,
    string strTempDir,
    out string strError)
        {
            strError = "";

            int nRet = 0;

            List<string[]> logicNames = new List<string[]>();

            string[] cols = new string[2];
            cols[0] = strDatabaseName;
            cols[1] = "zh";
            logicNames.Add(cols);

            string strKeysDefFileName = PathUtil.MergePath(strTemplateDir, "keys");
            string strBrowseDefFileName = PathUtil.MergePath(strTemplateDir, "browse");

            nRet = FileUtil.ConvertGb2312TextfileToUtf8(strKeysDefFileName,
                out strError);
            if (nRet == -1)
                return -1;

            CopyTempFile(strKeysDefFileName, strTempDir, strDatabaseName);

            nRet = FileUtil.ConvertGb2312TextfileToUtf8(strBrowseDefFileName,
                out strError);
            if (nRet == -1)
                return -1;

            CopyTempFile(strBrowseDefFileName, strTempDir, strDatabaseName);

            string strKeysDef = "";
            string strBrowseDef = "";

            try
            {
                using (StreamReader sr = new StreamReader(strKeysDefFileName, Encoding.UTF8))
                {
                    strKeysDef = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                strError = "装载文件 " + strKeysDefFileName + " 时发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                using (StreamReader sr = new StreamReader(strBrowseDefFileName, Encoding.UTF8))
                {
                    strBrowseDef = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                strError = "装载文件 " + strBrowseDefFileName + " 时发生错误: " + ex.Message;
                return -1;
            }

            long lRet = channel.DoCreateDB(logicNames,
                "", // strType,
                "", // strSqlDbName,
                strKeysDef,
                strBrowseDef,
                out strError);
            if (lRet == -1)
            {
                strError = "创建数据库 " + strDatabaseName + " 时发生错误: " + strError;
                return -1;
            }

            lRet = channel.DoInitialDB(strDatabaseName,
                out strError);
            if (lRet == -1)
            {
                strError = "初始化数据库 " + strDatabaseName + " 时发生错误: " + strError;
                // TODO: 初始化数据库失败，但数据库创建是成功的。需要在退出前尝试删除这个数据库
                {
                    // 如果发现同名数据库已经存在，先删除
                    lRet = channel.DoDeleteDB(strDatabaseName, out string strError1);
                    if (lRet == -1)
                    {
                        if (channel.IsNotFound() == false)
                        {
                            strError += "\r\n尝试删除刚创建的此数据库时又遇到出错: " + strError1;
                            return -1;
                        }
                    }
                    else
                        strError += "\r\n已经删除此数据库";
                }
                return -1;
            }

            // 增补其他数据从属对象

            /*
            List<string> subdirs = new List<string>();
            // 创建所有目录对象
            GetSubdirs(strTemplateDir, ref subdirs);
            for (int i = 0; i < subdirs.Count; i++)
            {
                string strDiskPath = subdirs[i];

                // 反过来推算为逻辑路径
                // 或者预先在获得的数组中就存放为部分(逻辑)路径？
                string strPath = "";

                // 在服务器端创建对象
                // parameters:
                //      strStyle    风格。当创建目录的时候，为"createdir"，否则为空
                // return:
                //		-1	错误
                //		1	以及存在同名对象
                //		0	正常返回
                nRet = NewServerSideObject(
                    channel,
                    strPath,
                    "createdir",
                    null,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
                // 列出每个目录中的文件，并在服务器端创建之
                // 注意模板目录下的文件，被当作cfgs中的文件来创建
             * */

            DirectoryInfo di = new DirectoryInfo(strTemplateDir);
            FileInfo[] fis = di.GetFiles();

            // 创建所有文件对象
            for (int i = 0; i < fis.Length; i++)
            {
                string strName = fis[i].Name;
                if (strName == "." || strName == "..")
                    continue;

                if (strName.ToLower() == "keys"
                    || strName.ToLower() == "browse")
                    continue;

                string strFullPath = fis[i].FullName;

                nRet = FileUtil.ConvertGb2312TextfileToUtf8(strFullPath,
                    out strError);
                if (nRet == -1)
                    return -1;

                CopyTempFile(strFullPath, strTempDir, strDatabaseName);

                using (Stream s = new FileStream(strFullPath, FileMode.Open))
                {
                    string strPath = strDatabaseName + "/cfgs/" + strName;
                    // 在服务器端创建对象
                    // parameters:
                    //      strStyle    风格。当创建目录的时候，为"createdir"，否则为空
                    // return:
                    //		-1	错误
                    //		1	以及存在同名对象
                    //		0	正常返回
                    nRet = NewServerSideObject(
                        channel,
                        strPath,
                        "",
                        s,
                        null,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            return 0;
        }

        // 在服务器端创建对象
        // parameters:
        //      strStyle    风格。当创建目录的时候，为"createdir"，否则为空
        // return:
        //		-1	错误
        //		1	以及存在同名对象
        //		0	正常返回
        public static int NewServerSideObject(
            RmsChannel channel,
            string strPath,
            string strStyle,
            Stream stream,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            byte[] baOutputTimestamp = null;
            string strOutputPath = "";

            string strRange = "";
            if (stream != null && stream.Length != 0)
            {
                Debug.Assert(stream.Length != 0, "test");
                strRange = "0-" + Convert.ToString(stream.Length - 1);
            }
            long lRet = channel.DoSaveResObject(strPath,
                stream,
                (stream != null && stream.Length != 0) ? stream.Length : 0,
                strStyle,
                "",	// strMetadata,
                strRange,
                true,
                baTimeStamp,	// timestamp,
                out baOutputTimestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.AlreadyExist)
                {
                    return 1;	// 已经存在同名同类型对象
                }
                strError = "写入 '" + strPath + "' 发生错误: " + strError;
                return -1;
            }

            return 0;
        }

        // 数据库是否已经存在？
        // return:
        //      -1  error
        //      0   not exist
        //      1   exist
        //      2   其他类型的同名对象已经存在
        public static int IsDatabaseExist(
            RmsChannel channel,
            string strDatabaseName,
            out string strError)
        {
            strError = "";

            // 看看数据库是否已经存在
            ResInfoItem[] items = null;
            long lRet = channel.DoDir("",
                "zh",
                "", // style
                out items,
                out strError);
            if (lRet == -1)
            {
                strError = "列服务器 " + channel.Url + " 下全部数据库目录的时候出错: " + strError;
                return -1;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Name == strDatabaseName)
                {
                    if (items[i].Type == ResTree.RESTYPE_DB)
                    {
                        strError = "数据库 " + strDatabaseName + " 已经存在。";
                        return 1;
                    }
                    else
                    {
                        strError = "和数据库 " + strDatabaseName + " 同名的非数据库类型对象已经存在。";
                        return 2;
                    }
                }
            }

            return 0;
        }
    }
}
