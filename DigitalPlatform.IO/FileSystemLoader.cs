using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DigitalPlatform.IO
{
    [Flags]
    public enum ListStyle
    {
        None = 0,
        ExpandDirectory = 0x01, // 当没有匹配模式的时候，针对 Directory，要展开遍历它的下级
    }

    /// <summary>
    /// 文件、目录系统枚举器
    /// 元素类型是 FileSystemInfo
    /// </summary>
    public class FileSystemLoader : IEnumerable
    {
        public ListStyle ListStyle = ListStyle.ExpandDirectory; // 缺省的，适合 Dir 命令效果

        /// <summary>
        /// 当前目录
        /// </summary>
        public string CurrentDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// 用于搜索的路径
        /// </summary>
        public string Pattern
        {
            get;
            set;
        }

        public FileSystemLoader(string strCurrentDirectory, string strPath)
        {
            this.CurrentDirectory = strCurrentDirectory;
            this.Pattern = strPath;
        }

        // 是否为盘符 ?
        static bool IsDisk(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return false;
            if (strText.Length != 2)
                return false;
            if (strText[1] == ':')
                return true;
            return false;
        }

        // 第一个元素表示 disk
        static List<string> MakeLevels(string strPath)
        {
#if NO
            if (string.IsNullOrEmpty(strPath) == true)
                return new List<string>();
            string strDisk = "";
            if (strPath.Length >= 2 && strPath[1] == ':')
            {
                strDisk = strPath.Substring(0, 2);
                strPath = strPath.Substring(2);
            }
            List<string> results = new List<string>();
            results.Add(strDisk);
            results.AddRange(
    strPath.Replace("/", "\\").Split(new[] { '\\' })
);
            return results;
#endif
            List<string> results = new List<string>();
            results.AddRange(
    strPath.Replace("/", "\\").Split(new[] { '\\' })
);
            return results;

        }

        static string MakePath(List<string> levels)
        {
            StringBuilder result = new StringBuilder();
            foreach (string level in levels)
            {
                if (string.IsNullOrEmpty(level) == true)
                    result.Append("\\");
                else
                    result.Append(level + "\\");
            }

            return result.ToString();
        }

        static void RemoveTailNullLevel(ref List<string> dir_levels)
        {
            // 去掉末尾的连续空 level。因为这些 level 会干扰后面的遍历过程，让人以为要回到根目录
            for (int i = dir_levels.Count - 1; i >= 1; i--)
            {
                if (string.IsNullOrEmpty(dir_levels[i]) == true)
                    dir_levels.RemoveAt(i);
                else
                    break;
            }
        }

        // 切换当前目录
        // parameters:
        //      strRootDir  根的物理路径。保证了 cd \ 这样的命令不越过根。如果本参数为空，表示不限制根，也就是直接用物理磁盘的根
        public static void ChangeDirectory(
            // string strRootDir,
            string strCurrentDirectory,
            string strPattern,
            out string strDirectory)
        {
            strDirectory = "";

            List<string> current_levels = MakeLevels(strCurrentDirectory);

            List<string> dir_levels = new List<string>();
            if (string.IsNullOrEmpty(strPattern) == false)
            {
                dir_levels = MakeLevels(strPattern);
            }
            // 去掉末尾的连续空 level。因为这些 level 会干扰后面的遍历过程，让人以为要回到根目录
#if NO
            for (int i = dir_levels.Count - 1; i >= 1; i--)
            {
                if (string.IsNullOrEmpty(dir_levels[i]) == true)
                    dir_levels.RemoveAt(i);
                else
                    break;
            }
#endif
            RemoveTailNullLevel(ref current_levels);
            RemoveTailNullLevel(ref dir_levels);

            // 遍历 dir_levels 的每一级
            foreach (string level in dir_levels)
            {
                if (level.IndexOfAny(new char[] { '*', '?' }) != -1)
                {
                    // TODO: 是否支持 cd 路径中携带模式？
                    throw new Exception("不允许使用匹配模式");
                }

                if (IsDisk(level) == true)
                {
                    current_levels[0] = level;
                    while (current_levels.Count > 1)
                        current_levels.RemoveAt(1);
                    continue;
                }
                if (string.IsNullOrEmpty(level) == true)
                {
                    // 到相同盘的根
                    while (current_levels.Count > 1)
                        current_levels.RemoveAt(1);
                    // current_levels.Add(level);
                    continue;
                }
                if (level == ".")
                    continue;
                if (level == "..")
                {
                    // 去掉最后一级
                    if (current_levels.Count == 0)
                        throw new Exception("上溯越过根");
                    current_levels.RemoveAt(current_levels.Count - 1);
                    continue;
                }

                current_levels.Add(level);
            }

            strDirectory = MakePath(current_levels);
        }


        // 把一个可能带有匹配模式的路径字符串，解析为 (绝对)路径 和 模式两个部分
        // 例如 ..\test\*.* 解析为 c:\test 和 *.* 两个部分
        public static void Parse(
            string strCurrentDirectory,
            string strPath,
            out string strDirectory,
            out string strPattern)
        {
            strDirectory = "";
            strPattern = "";

            List<string> current_levels = MakeLevels(strCurrentDirectory);

            List<string> dir_levels = new List<string>();
            if (string.IsNullOrEmpty(strPath) == false)
            {
                dir_levels = MakeLevels(strPath);
            }
            // 去掉末尾的连续空 level。因为这些 level 会干扰后面的遍历过程，让人以为要回到根目录
            for (int i = dir_levels.Count - 1; i >= 1; i--)
            {
                if (string.IsNullOrEmpty(dir_levels[i]) == true)
                    dir_levels.RemoveAt(i);
                else
                    break;
            }

#if NO
            string strDisk = "";
            if (current_levels.Count > 0)
            {
                strDisk = current_levels[0];
                current_levels.RemoveAt(0); // 盘符不再包含在 level 中。这是为了方便后面随时切换盘符
            }
#endif

            // 遍历 dir_levels 的每一级
            foreach (string level in dir_levels)
            {
                if (level.IndexOfAny(new char[] { '*', '?' }) != -1)
                {
                    // 模式应该在最后一级
                    strPattern = level;
                    // TODO: 是否警告模式出现在中间一级的情况?
                    break;
                }
                if (IsDisk(level) == true)
                {
                    current_levels[0] = level;
                    while (current_levels.Count > 1)
                        current_levels.RemoveAt(1);
                    continue;
                }
                if (string.IsNullOrEmpty(level) == true)
                {
                    // 到相同盘的根
                    while (current_levels.Count > 1)
                        current_levels.RemoveAt(1);
                    // current_levels.Add(level);
                    continue;
                }
                if (level == ".")
                    continue;
                if (level == "..")
                {
                    // 去掉最后一级
                    if (current_levels.Count == 0)
                        throw new Exception("上溯越过根");
                    current_levels.RemoveAt(current_levels.Count - 1);
                    continue;
                }

                current_levels.Add(level);
            }

            strDirectory = MakePath(current_levels);
        }

        static string TrimRightSlash(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return strPath;
            if (strPath[strPath.Length - 1] == '\\'
                || strPath[strPath.Length - 1] == '/')
                return strPath.Substring(0, strPath.Length - 1);

            return strPath;
        }

        public IEnumerator GetEnumerator()
        {
            // 测试 ..
            // *.*
            // ..\
            // ..\*.*
            // \
            // \*.*
            // ..
            // .

            string strDirectory = "";
            string strPattern = "";
            // 把一个可能带有匹配模式的路径字符串，解析为 (绝对)路径 和 模式两个部分
            // 例如 ..\test\*.* 解析为 c:\test 和 *.* 两个部分
            Parse(this.CurrentDirectory,
                this.Pattern,
                out strDirectory,
                out strPattern);

            if (string.IsNullOrEmpty(strPattern) == true)
            {
                // 有可能是文件
                // 对 strDirectory 要去掉末尾的 '\'
                strDirectory = TrimRightSlash(strDirectory);
                if (File.Exists(strDirectory) == true)
                {
                    FileInfo fi = new FileInfo(strDirectory);
                    yield return fi;
                    yield break;
                }

                if (Directory.Exists(strDirectory) == true
                    && (this.ListStyle & ListStyle.ExpandDirectory) == 0)
                {
                    DirectoryInfo fi = new DirectoryInfo(strDirectory);
                    yield return fi;
                    yield break;
                }
            }

            if (string.IsNullOrEmpty(strPattern) == true)
                strPattern = "*.*";

            DirectoryInfo di = new DirectoryInfo(strDirectory);
            FileSystemInfo[] sis = di.GetFileSystemInfos(strPattern);

            foreach (FileSystemInfo si in sis)
            {
                yield return si;
            }
        }

#if NO
        public IEnumerator GetEnumerator()
        {
            // 测试 ..
            // *.*
            // ..\
            // ..\*.*
            // \
            // \*.*
            // ..
            // .

            string strDir = "*.*"; // this._currentLocalDir;
            if (string.IsNullOrEmpty(this.DirPath) == false)
                strDir = this.DirPath;

            string strFullPath = Path.Combine(this.CurrentDirectory, strDir);
            try
            {
                strFullPath = Path.GetFullPath(strFullPath);  // 甩掉末尾的 \..
            }
            catch (Exception ex)
            {

            }

            string strDirectory = Path.GetDirectoryName(strFullPath); // c:\ --> null
            if (strDirectory == null)
                strDirectory = strFullPath;
            else
            {
                try
                {
                    strDirectory = Path.GetFullPath(strDirectory);  // 甩掉末尾的 \..
                }
                catch (Exception ex)
                {

                }
            }
            string strPattern = Path.GetFileName(strFullPath);
            if (string.IsNullOrEmpty(strPattern) == true)
                strPattern = "*.*";

            DirectoryInfo di = new DirectoryInfo(strDirectory);
            FileSystemInfo[] sis = di.GetFileSystemInfos(strPattern);

            foreach (FileSystemInfo si in sis)
            {
                yield return si;
            }
        }
#endif
    }
}
