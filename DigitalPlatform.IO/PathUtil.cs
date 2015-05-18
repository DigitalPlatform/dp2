using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DigitalPlatform.IO
{

    /// <summary>
    /// Path功能扩展函数
    /// </summary>
    public class PathUtil
    {
        // 去除路径第一字符的 '/'
        public static string RemoveRootSlash(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;
            if (strPath[0] == '\\' || strPath[0] == '/')
                return strPath.Substring(1);
            return strPath;
        }

        // 获得一个目录下的全部文件的尺寸总和。包括子目录中的
        public static long GetAllFileSize(string strDataDir, ref long count)
        {
            long size = 0;
            DirectoryInfo di = new DirectoryInfo(strDataDir);
            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
                count++;
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                size += GetAllFileSize(subdir.FullName, ref count);
            }

            return size;
        }

        // get clickonce shortcut filename
        // parameters:
        //      strApplicationName  "DigitalPlatform/dp2 V2/dp2内务 V2"
        public static string GetShortcutFilePath(string strApplicationName)
        {
            // string publisherName = "Publisher Name";
            // string applicationName = "Application Name";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), strApplicationName) + ".appref-ms";
        }

        public static void DeleteDirectory(string strDirPath)
        {
            try
            {
                Directory.Delete(strDirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // 不存在就算了
            }
        }

        // 移除文件目录内所有文件的 ReadOnly 属性
        public static void RemoveReadOnlyAttr(string strSourceDir)
        {
            string strCurrentDir = Directory.GetCurrentDirectory();

            DirectoryInfo di = new DirectoryInfo(strSourceDir);

            FileSystemInfo[] subs = di.GetFileSystemInfos();

            for (int i = 0; i < subs.Length; i++)
            {

                // 递归
                if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    RemoveReadOnlyAttr(subs[i].FullName);
                }
                else
                    File.SetAttributes(subs[i].FullName, FileAttributes.Normal);

            }
        }

        // 拷贝目录
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            bool bDeleteTargetBeforeCopy,
            out string strError)
        {
            strError = "";

            try
            {

                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                        Directory.Delete(strTargetDir, true);
                }

                CreateDirIfNeed(strTargetDir);


                FileSystemInfo[] subs = di.GetFileSystemInfos();

                for (int i = 0; i < subs.Length; i++)
                {
                    // 复制目录
                    if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(subs[i].FullName,
                            strTargetDir + "\\" + subs[i].Name,
                            bDeleteTargetBeforeCopy,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        continue;
                    }
                    // 复制文件
                    File.Copy(subs[i].FullName, strTargetDir + "\\" + subs[i].Name, true);
                }

            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }


            return 0;
        }

        // 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
        public static bool CreateDirIfNeed(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        // 删除一个目录内的所有文件和目录
        public static bool ClearDir(string strDir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(strDir);
                if (di.Exists == false)
                    return true;

                // 删除所有的下级目录
                DirectoryInfo[] dirs = di.GetDirectories();
                foreach (DirectoryInfo childDir in dirs)
                {
                    Directory.Delete(childDir.FullName, true);
                }

                // 删除所有文件
                FileInfo[] fis = di.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    File.Delete(fi.FullName);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // 获得纯文件名部分
        public static string PureName(string strPath)
        {
            // 2012/11/30
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;

            string sResult = "";
            sResult = strPath;
            sResult = sResult.Replace("/", "\\");
            if (sResult.Length > 0)
            {
                if (sResult[sResult.Length - 1] == '\\')
                    sResult = sResult.Substring(0, sResult.Length - 1);
            }
            int nRet = sResult.LastIndexOf("\\");
            if (nRet != -1)
                sResult = sResult.Substring(nRet + 1);

            return sResult;
        }

        public static string PathPart(string strPath)
        {
            string sResult = "";
            sResult = strPath;
            sResult = sResult.Replace("/", "\\");
            if (sResult.Length > 0)
            {
                if (sResult[sResult.Length - 1] == '\\')
                    sResult = sResult.Substring(0, sResult.Length - 1);
            }
            int nRet = sResult.LastIndexOf("\\");
            if (nRet != -1)
                sResult = sResult.Substring(0, nRet);
            else
                sResult = "";

            return sResult;
        }

        public static string MergePath(string s1, string s2)
        {
            string sResult = "";

            if (s1 != null)
            {
                sResult = s1;
                sResult = sResult.Replace("/", "\\");
                if (sResult != "")
                {
                    if (sResult[sResult.Length - 1] != '\\')
                        sResult += "\\";
                }
                else
                {
                    sResult += "\\";
                }
            }
            if (s2 != null)
            {
                s2 = s2.Replace("/", "\\");
                if (s2 != "")
                {
                    if (s2[0] == '\\')
                        s2 = s2.Remove(0, 1);
                    sResult += s2;
                }

            }

            return sResult;
        }

        // 正规化目录路径名。把所有字符'/'替换为'\'，并且为末尾确保有字符'\'
        public static string CanonicalizeDirectoryPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return "";

            strPath = strPath.Replace('/', '\\');

            if (strPath[strPath.Length - 1] != '\\')
                strPath += "\\";

            return strPath;
        }

        // 测试strPath1是否为strPath2的下级目录或文件
        //	strPath1正好等于strPath2的情况也返回true
        public static bool IsChildOrEqual(string strPath1, string strPath2)
        {
            FileSystemInfo fi1 = new DirectoryInfo(strPath1);

            FileSystemInfo fi2 = new DirectoryInfo(strPath2);

            string strNewPath1 = fi1.FullName;
            string strNewPath2 = fi2.FullName;

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            // 路径1字符串长度比路径2短，说明路径1已不可能是儿子，因为儿子的路径会更长
            if (strNewPath1.Length < strNewPath2.Length)
                return false;


            // 截取路径1中前面一段进行比较
            string strPart = strNewPath1.Substring(0, strNewPath2.Length);
            strPart.ToUpper();
            strNewPath2.ToUpper();

            if (strPart != strNewPath2)
                return false;

            return true;
        }

        // 测试strPath1是否和strPath2为同一文件或目录
        public static bool IsEqual(string strPath1, string strPath2)
        {
            if (String.IsNullOrEmpty(strPath1) == true
                && String.IsNullOrEmpty(strPath2) == true)
                return true;

            if (String.IsNullOrEmpty(strPath1) == true)
                return false;

            if (String.IsNullOrEmpty(strPath2) == true)
                return false;

            if (strPath1 == strPath2)
                return true;

            FileSystemInfo fi1 = new DirectoryInfo(strPath1);
            FileSystemInfo fi2 = new DirectoryInfo(strPath2);

            string strNewPath1 = fi1.FullName;
            string strNewPath2 = fi2.FullName;

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            if (strNewPath1.Length != strNewPath2.Length)
                return false;

            strNewPath1.ToUpper();
            strNewPath2.ToUpper();

            if (strNewPath1 == strNewPath2)
                return true;

            return false;
        }

        // 测试strPath1是否和strPath2为同一文件或目录
        public static bool IsEqualEx(string strPath1, string strPath2)
        {
            string strNewPath1 = strPath1;
            string strNewPath2 = strPath2;

            if (strNewPath1.Length != 0)
            {
                if (strNewPath1[strNewPath1.Length - 1] != '\\')
                    strNewPath1 += "\\";
            }
            if (strNewPath2.Length != 0)
            {
                if (strNewPath2[strNewPath2.Length - 1] != '\\')
                    strNewPath2 += "\\";
            }

            if (strNewPath1.Length != strNewPath2.Length)
                return false;

            strNewPath1.ToUpper();
            strNewPath2.ToUpper();

            if (strNewPath1 == strNewPath2)
                return true;

            return false;
        }


        public static string EnsureTailBackslash(string strPath)
        {
            if (strPath == "")
                return "\\";

            string sResult = "";

            sResult = strPath.Replace("/", "\\");

            if (sResult[sResult.Length - 1] != '\\')
                sResult += "\\";

            return sResult;
        }

        // 末尾是否有'\'。如果具备，表示这是一个目录路径。
        public static bool HasTailBackslash(string strPath)
        {
            if (strPath == "")
                return true;	// 理解为'\'

            string sResult = "";

            sResult = strPath.Replace("/", "\\");

            if (sResult[sResult.Length - 1] == '\\')
                return true;

            return false;
        }


        // 测试strPathChild是否为strPathParent的下级目录或文件
        // 如果是下级，则将strPathChild中和strPathParent重合的部分替换为
        // strMacro中的宏字符串返回在strResult中，并且函数返回true。
        // 否则函数返回false，strResult虽返回内容，但不替换。
        //	strPath1正好等于strPath2的情况也返回true
        // 
        // Exception:
        //	System.NotSupportedException
        // Testing:
        //	在testIO.exe中
        public static bool MacroPathPart(string strPathChild,
            string strPathParent,
            string strMacro,
            out string strResult)
        {
            strResult = strPathChild;

            FileSystemInfo fiChild = new DirectoryInfo(strPathChild);

            FileSystemInfo fiParent = new DirectoryInfo(strPathParent);

            string strNewPathChild = fiChild.FullName;
            string strNewPathParent = fiParent.FullName;

            if (strNewPathChild.Length != 0)
            {
                if (strNewPathChild[strNewPathChild.Length - 1] != '\\')
                    strNewPathChild += "\\";
            }
            if (strNewPathParent.Length != 0)
            {
                if (strNewPathParent[strNewPathParent.Length - 1] != '\\')
                    strNewPathParent += "\\";
            }

            // 路径1字符串长度比路径2短，说明路径1已不可能是儿子，因为儿子的路径会更长
            if (strNewPathChild.Length < strNewPathParent.Length)
                return false;


            // 截取路径1中前面一段进行比较
            string strPart = strNewPathChild.Substring(0, strNewPathParent.Length);
            strPart.ToUpper();
            strNewPathParent.ToUpper();

            if (strPart != strNewPathParent)
                return false;

            strResult = strMacro + "\\" + fiChild.FullName.Substring(strNewPathParent.Length);
            // fiChild.FullName是尾部未加'\'以前的形式

            return true;
        }

        // 将路径中的%%宏部分替换为具体内容
        // parameters:
        //		macroTable	宏名和内容的对照表
        //		bThrowMacroNotFoundException	是否抛出MacroNotFoundException异常
        // Exception:
        //	MacroNotFoundException
        //	MacroNameException	函数NextMacro()可能抛出
        // Testing:
        //	在testIO.exe中
        public static string UnMacroPath(Hashtable macroTable,
            string strPath,
            bool bThrowMacroNotFoundException)
        {
            int nCurPos = 0;
            string strPart = "";

            string strResult = "";

            for (; ; )
            {
                strPart = NextMacro(strPath, ref nCurPos);
                if (strPart == "")
                    break;

                if (strPart[0] == '%')
                {
                    string strValue = (string)macroTable[strPart];
                    if (strValue == null)
                    {
                        if (bThrowMacroNotFoundException)
                        {
                            MacroNotFoundException ex = new MacroNotFoundException("macro " + strPart + " not found in macroTable");
                            throw ex;
                        }
                        else
                        {
                            // 将没有找到的宏放回结果中
                            strResult += strPart;
                            continue;
                        }
                    }

                    strResult += strValue;
                }
                else
                {
                    strResult += strPart;
                }

            }

            return strResult;
        }

        // 本函数为UnMacroPath()的服务函数
        // 顺次得到下一个部分
        // nCurPos在第一次调用前其值必须设置为0，然后，调主不要改变其值
        // Exception:
        //	MacroNameException
        static string NextMacro(string strText,
            ref int nCurPos)
        {
            if (nCurPos >= strText.Length)
                return "";

            string strResult = "";
            bool bMacro = false;	// 本次是否在macro上

            if (strText[nCurPos] == '%')
                bMacro = true;

            int nRet = -1;

            if (bMacro == false)
                nRet = strText.IndexOf("%", nCurPos);
            else
                nRet = strText.IndexOf("%", nCurPos + 1);

            if (nRet == -1)
            {
                strResult = strText.Substring(nCurPos);
                nCurPos = strText.Length;
                if (bMacro == true)
                {
                    // 这是异常情况，表明%只有头部一个
                    throw (new MacroNameException("macro " + strResult + " format error"));
                }
                return strResult;
            }

            if (bMacro == true)
            {
                strResult = strText.Substring(nCurPos, nRet - nCurPos + 1);
                nCurPos = nRet + 1;
                return strResult;
            }
            else
            {
                Debug.Assert(strText[nRet] == '%', "当前位置不是%，异常");
                strResult = strText.Substring(nCurPos, nRet - nCurPos);
                nCurPos = nRet;
                return strResult;
            }

        }

        public static string GetShortFileName(string strFileName)
        {
            StringBuilder shortPath = new StringBuilder(300);
            int nRet = API.GetShortPathName(
                strFileName,
                shortPath,
                shortPath.Capacity);
            if (nRet == 0 || nRet >= 300)
            {
                // MessageBox.Show("file '" +strFileName+ "' get short error");
                // return strFileName;
                throw (new Exception("GetShortFileName error"));
            }

            return shortPath.ToString();
        }
    }
}
