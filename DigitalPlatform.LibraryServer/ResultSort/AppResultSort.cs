using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    // 和结果集排序有关的函数
    public partial class LibraryApplication
    {
        // 内存结果集 字典
        Dictionary<string, MemorySet> _memorySets = new Dictionary<string, MemorySet>();
        object _syncRoot_memorySets = new object();

        const int MAX_MEMORYSET_COUNT = 10000;

        // 查找一个内存结果集
        public MemorySet FindMemorySet(string name)
        {
            lock (_syncRoot_memorySets)
            {
                if (_memorySets.TryGetValue(name, out MemorySet value) == false)
                    return null;
                return value;
            }
        }

        // 获得文件名前缀
        public string GetMemorySetFilePathPrefix(SessionInfo sessioninfo,
            string strResultSetName)
        {
            if (string.IsNullOrEmpty(strResultSetName))
                strResultSetName = "default";

            // 全局结果集
            if (strResultSetName.StartsWith("#"))
                return Path.Combine(this.TempDir, $"~grs_{strResultSetName}_");

            return Path.Combine(sessioninfo.TempDir,
$"sort_{strResultSetName}_");
        }

        // 获得单个精确的文件名
        // parameters:
        //      strSection  用于进一步区分结果集不同特性的名字(2022/10/27)
        public string GetMemorySetFilePath(SessionInfo sessioninfo,
            string strResultSetName,
            string strBrowseInfoStyle)
        {
            if (string.IsNullOrEmpty(strResultSetName))
                strResultSetName = "default";

            var sort_cols = StringUtil.GetParameterByPrefix(strBrowseInfoStyle, "sort");
            var sort_max_count = StringUtil.GetParameterByPrefix(strBrowseInfoStyle, "sortmaxcount");

            // 注: sort_cols 形如 "-1|0|1|2"
            if (string.IsNullOrEmpty(sort_cols) == false)
                sort_cols = sort_cols.Replace("|", "_");
            else
                sort_cols = "";

            if (string.IsNullOrEmpty(sort_max_count) == false)
                sort_max_count = "m" + sort_max_count;
            else
                sort_max_count = "";

            // 全局结果集
            if (strResultSetName.StartsWith("#"))
                return Path.Combine(this.TempDir, $"~grs_{strResultSetName}_{sort_cols}_{sort_max_count}");

            return Path.Combine(sessioninfo.TempDir,
$"sort_{strResultSetName}_{sort_cols}_{sort_max_count}");
        }

        // 根据名字精确删除
        public bool RemoveMemorySet(string name)
        {
            lock (_syncRoot_memorySets)
            {
                if (_memorySets.TryGetValue(name, out MemorySet value) == false)
                    return false;
                value.DeleteFile();
                return _memorySets.Remove(name);
            }
        }

        // 根据前缀名删除
        public int RemoveMemorySetByPrefix(string prefix)
        {
            lock (_syncRoot_memorySets)
            {
                List<string> delete_names = new List<string>();
                foreach (var name in _memorySets.Keys)
                {
                    if (name.StartsWith(prefix))
                    {
                        var value = _memorySets[name];
                        value.DeleteFile();
                        delete_names.Add(name);
                    }
                }

                foreach (var name in delete_names)
                {
                    _memorySets.Remove(name);
                }

                return delete_names.Count;
            }
        }

        // 删除全部 MemorySet 对象(和物理文件)
        public void RemoveAllMemorySet()
        {
            lock (_syncRoot_memorySets)
            {
                foreach (var name in _memorySets.Keys)
                {
                    var value = _memorySets[name];
                    value.DeleteFile();
                }
                _memorySets.Clear();
            }
        }

        public MemorySet GetMemorySet(string name)
        {
            lock (_syncRoot_memorySets)
            {
                if (_memorySets.TryGetValue(name, out MemorySet value) == true)
                    return value;

                if (_memorySets.Count > MAX_MEMORYSET_COUNT)
                    throw new OutofMemorySetException($"当前本地结果集总数超过 {MAX_MEMORYSET_COUNT}，无法创建新的结果集");

                value = new MemorySet { FilePath = name };
                _memorySets[name] = value;
                return value;
            }
        }

        // 删除属于一个 Session 的所有 MemorySet 对象
        public void RemoveSesssionMemorySet(SessionInfo sessioninfo)
        {
            List<MemorySet> sets = new List<MemorySet>();
            string prefix = GetSessionMemorySetFilePathPrefix(sessioninfo);
            lock (_syncRoot_memorySets)
            {
                List<string> delete_paths = new List<string>();
                foreach (var path in _memorySets.Keys)
                {
                    if (path != null && path.StartsWith(prefix))
                        delete_paths.Add(path);
                }

                foreach (var path in delete_paths)
                {
                    sets.Add(_memorySets[path]);
                    _memorySets.Remove(path);
                }
            }

            // 删除物理文件。也可以不在这里删除，等 Session 释放时候自动删除临时目录
            foreach (var value in sets)
            {
                value.DeleteFile();
            }
        }

        // 清理长期空闲的全局 MemorySets
        public void CleanIdleGlobalMemorySets(TimeSpan length)
        {
            if (_memorySets == null)
                return;

            string prefix = this.TempDir.Replace("/", "\\");
            List<MemorySet> delete_sets = new List<MemorySet>();
            DateTime now = DateTime.Now;
            lock (_syncRoot_memorySets)
            {
                List<string> delete_paths = new List<string>();
                foreach (var item in _memorySets)
                {
                    var path = item.Key;
                    if (string.IsNullOrEmpty(path))
                        continue;
                    path = path.Replace("/", "\\");
                    if (path.StartsWith(prefix)
                        && now - item.Value.LastTime > length)
                        delete_paths.Add(path);
                }

                foreach (var path in delete_paths)
                {
                    delete_sets.Add(_memorySets[path]);
                    _memorySets.Remove(path);
                    // TODO: 如何让 memorySet 对象感知到此后不能继续使用了
                }
            }

            // 删除物理文件
            foreach (var value in delete_sets)
            {
                value.DeleteFile();
            }
        }

        /*
// 是否为 session 从属的本地结果集文件名?
public static bool IsSessionMemorySetFilePath(
    SessionInfo sessioninfo,
    string filePath)
{
    var prefix = Path.Combine(sessioninfo.TempDir, "sort_");
    if (filePath.StartsWith(prefix))
        return true;
    return false;
}
*/

        // 获得 session 从属的本地结果集文件名的前缀部分，用于比较
        public static string GetSessionMemorySetFilePathPrefix(
            SessionInfo sessioninfo)
        {
            return Path.Combine(sessioninfo.TempDir, "sort_");
        }


        public long GetSearchResult(
            RmsChannel channel,
    string filePath,
    long lStart,
    long lCount,
    string strBrowseInfoStyle,
    string strLang,
    out Record[] searchresults,
    out string strError)
        {
            searchresults = null;
            strError = "";

            var memorySet = FindMemorySet(filePath);
            if (memorySet == null)
            {
                strError = $"内存结果集 {filePath} 没有找到";
                return -1;
            }

            List<Record> results = new List<Record>();
            // parameters:
            //      length  要读取多少个。-1 表示尽量多地读取
            List<string> paths;
            try
            {
                paths = memorySet.GetPaths(lStart, (int)lCount);
            }
            catch (FileNotFoundException ex)
            {
                // 删除 memorySet 对象，避免前端再次请求反复报错
                this.RemoveMemorySet(memorySet.FilePath);
                strError = $"MemorySet::GetPaths() 没有找到物理文件。详细信息已写入错误日志文件";
                this.WriteErrorLog($"MemorySet::GetPaths() 没有找到物理文件: {ex.Message}");
                return -1;
            }

            // TODO: paths 中间有空的路径怎么办?

            RmsBrowseLoader loader = new RmsBrowseLoader();
            loader.Channel = channel;
            loader.Format = strBrowseInfoStyle;
            loader.RecPaths = paths;

            foreach (Record record in loader)
            {
                results.Add(record);
            }

            /*
            foreach (var path in paths)
            {
                var record = new Record { Path = path };
                results.Add(record);
            }
            */

            searchresults = results.ToArray();
            return memorySet.TotalCount;
        }

        // 确保创建内存结果集
        // return:
        //      false   本地结果集没有创建。这是因为 dp2kernel 一端结果集不存在或者记录数为 0
        //      true    本地结果集已经创建
        public bool EnsureCreateLocalResultSet(RmsChannel channel,
            string filePath,
            string resultset_name,
            string browse_style)
        {
            var memorySet = FindMemorySet(filePath);
            if (memorySet != null)
                return true; // 已经存在
            else
            {
                try
                {
                    memorySet = GetMemorySet(filePath);
                }
                catch (OutOfMemoryException ex)
                {
                    this.WriteErrorLog(ex.Message);
                    // 尝试清除休眠五分钟以上的本地全局结果集
                    CleanIdleGlobalMemorySets(TimeSpan.FromMinutes(5));
                    // TODO: 如果还没有腾出结果集，那么再尝试清除全部本地结果集(也就说也包括 Session 本地结果集)
                    throw;
                }
            }

            if (memorySet.State == "creating")
                throw new Exception($"本地结果集 {filePath} 正在创建中，请稍候重试获取结果集操作");

            memorySet.IncUsing();
            try
            {
                if (memorySet.Using > 1)
                    throw new Exception($"本地结果集 {filePath} 正在创建中，请稍候重试获取结果集操作");

                int max_count = -1; // -1 表示不限制
                var sort_max_count = StringUtil.GetParameterByPrefix(browse_style, "sortmaxcount");
                if (string.IsNullOrEmpty(sort_max_count) == false)
                {
                    if (Int32.TryParse(sort_max_count, out max_count) == false)
                        throw new ArgumentException($"browse_style 参数值 '{browse_style}' 中 sortmaxcount 子参数值 '{sort_max_count}' 不合法。应为一个整数");
                }

                // sort:xxx
                var sort_cols = StringUtil.GetParameterByPrefix(browse_style, "sort");

                // 只传递回来需要排序的列即可
                StringUtil.SetInList(ref browse_style, "xml", false);

                // 确保这一次需要传递过来 id 列。不然写入磁盘文件的时候没有记录路径
                StringUtil.SetInList(ref browse_style, "id", true);

                if (sort_cols != null)
                {
                    // 确保 sort:xxx 参数中包含的列都在 cols (format:@coldef://parent) 参数中，不然无法排序
                    StringUtil.SetInList(ref browse_style, "cols", true);
                }

                SearchResultLoader loader = new SearchResultLoader(
                    channel,
                    null,
                    resultset_name,
                    browse_style);
                loader.ElementType = "Record";
                if (max_count != -1)
                    loader.MaxResultCount = max_count;

                foreach (Record record in loader)
                {
                    memorySet.Append(record.Path, record.Cols);
                }

                if (loader.ResultCount <= 0)
                {
                    RemoveMemorySet(filePath);
                    return false;
                }

                // 如果需要排序
                if (string.IsNullOrEmpty(sort_cols) == false)
                {
                    var indices = ParseSortCols(sort_cols);
                    memorySet.Sort(indices);
                }

                memorySet.MemoryTotalCount();
                memorySet.PushToDisk();
                return true;
            }
            finally
            {
                memorySet.DecUsing();
            }
        }

        // return:
        //      要参与排序的列 index 数组。1 表示 Path, 从 2 开始表示 Cols 的列。负数表示倒序
        static int[] ParseSortCols(string def)
        {
            string[] list = def.Split(new char[] { '|' });
            List<int> results = new List<int>();
            foreach (var s in list)
            {
                if (s == "-0")
                    results.Add(-1);
                else
                {
                    int v = Convert.ToInt32(s);
                    if (v >= 0)
                        v++;
                    else
                        v--;
                    results.Add(v);
                }
            }

            return results.ToArray();
        }
    }

    public class OutofMemorySetException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="error"></param>
        /// <param name="strText"></param>
        public OutofMemorySetException(string strText)
            : base(strText)
        {
        }
    }
}
