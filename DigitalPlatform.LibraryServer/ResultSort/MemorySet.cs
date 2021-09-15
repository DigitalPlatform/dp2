using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 内存结果集
    /// </summary>
    public class MemorySet
    {
        // 创建时刻
        public DateTime CreateTime { get; set; }
        // 最近一次使用时刻
        public DateTime LastTime { get; set; }

        // 正在使用的计数器。用完后减1
        int _inUse = 0;
        public int Using
        {
            get
            {
                return _inUse;
            }
        }

        // "creating" 表示正在创建过程中
        public string State
        {
            get
            {
                if (_inUse == 0)
                    return "";
                return "creating";
            }
        }

        public string FilePath { get; set; }
        public long TotalCount { get; set; }

        // 数据库名和整数
        Dictionary<string, short> _nameTable = new Dictionary<string, short>();

        // 如果 _sortItems == null 表示已经压入磁盘
        List<SortItem> _sortItems = new List<SortItem>();

        public MemorySet()
        {
            this.CreateTime = DateTime.Now;
        }

        public void Append(string path, string[] cols)
        {
            _sortItems.Add(new SortItem { Path = path, Cols = cols });
        }

        public void Clear()
        {
            _sortItems.Clear();
        }

        public void Touch()
        {
            this.LastTime = DateTime.Now;
        }

        public void IncUsing()
        {
            _inUse++;
        }

        public void DecUsing()
        {
            _inUse--;
        }

        /*
        // 从磁盘读回内存
        // 一般是需要重新排序才需要读回内存。分段读取 Path 并不需要读回内存，直接在磁盘文件上读就可以的
        public void LoadFromDisk()
        {
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                aspectRatio = reader.ReadSingle();
                tempDirectory = reader.ReadString();
                autoSaveTime = reader.ReadInt32();
                showStatusBar = reader.ReadBoolean();
            }
        }
        */

        public void MemoryTotalCount()
        {
            TotalCount = _sortItems.Count;
        }

        public void DeleteFile()
        {
            if (string.IsNullOrEmpty(FilePath))
                return;
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }

        // parameters:
        //      length  要读取多少个。-1 表示尽量多地读取
        public List<string> GetPaths(long start, int length)
        {
            if (_sortItems != null)
            {
                if (start >= _sortItems.Count)
                    throw new ArgumentException($"start {start} 越过集合个数 {_sortItems.Count}");

                if (length == -1)
                    length = _sortItems.Count - (int)start;

                if (length > 100)
                    length = 100;

                List<string> results = new List<string>();
                for (long i = start;
                    i < Math.Min(start + length, _sortItems.Count);
                    i++)
                {
                    results.Add(_sortItems[(int)i].Path);
                }

                return results;
            }

            {
                Dictionary<short, string> table = new Dictionary<short, string>();
                foreach (var dbName in _nameTable.Keys)
                {
                    var db_number = _nameTable[dbName];
                    table[db_number] = dbName;
                }

                List<string> results = new List<string>();
                using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open)))
                {
                    long offset = start * (sizeof(short) + sizeof(long));
                    if (offset > 0)
                        reader.BaseStream.Seek(offset, SeekOrigin.Begin);

                    long count = reader.BaseStream.Length / (sizeof(short) + sizeof(long));

                    if (length == -1)
                        length = (int)(count - start);

                    if (length > 100)
                        length = 100;

                    for (long i = start;
        i < start + length;
        i++)
                    {

                        try
                        {
                            var db_number = reader.ReadInt16();
                            var id_number = reader.ReadInt64();

                            if (db_number == -1)
                                results.Add(null);
                            else
                            {
                                if (table.TryGetValue(db_number, out string dbName) == false)
                                    throw new Exception($"db_number:{db_number} 在对照表中没有找到对应的字符串");

                                results.Add(dbName + "/" + id_number.ToString());
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                    }
                }

                return results;
            }
        }

        // 将 _sortItems 压入磁盘
        public void PushToDisk()
        {
            _nameTable.Clear();

            DeleteFile();

            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(FilePath));

            using (BinaryWriter binWriter =
new BinaryWriter(File.Open(FilePath, FileMode.Create)))
            {
                foreach (var item in _sortItems)
                {
                    short db_number = -1;
                    long id_number = -1;
                    if (string.IsNullOrEmpty(item.Path) == false)
                    {
                        var parts = StringUtil.ParseTwoPart(item.Path, "/");
                        string dbName = parts[0];
                        string id = parts[1];

                        db_number = GetDbNumber(dbName);
                        if (Int64.TryParse(id, out id_number) == false)
                            id_number = -1;
                    }

                    binWriter.Write(db_number);
                    binWriter.Write(id_number);
                }
            }

            _sortItems.Clear();
            _sortItems = null;
        }

        short GetDbNumber(string dbName)
        {
            if (_nameTable.TryGetValue(dbName, out short value) == true)
                return value;
            value = (short)_nameTable.Count;
            _nameTable[dbName] = value;
            return value;
        }

        // 按照一列 Cols 排序
        // parameters:
        //      indices 要参与排序的列 index。1 表示 Path, 从 2 开始表示 Cols 的列。负数表示倒序
        public void Sort(int [] indices)
        {
            _sortItems.Sort((a, b) => {
                // return string.Compare(a.Cols[column], b.Cols[column]);
                return CompareItems(a, b, indices);
            });
        }

        // parameters:
        //      indices 要参与排序的列 index。1 表示 Path, 从 2 开始表示 Cols 的列。负数表示倒序
        static int CompareItems(SortItem item1, 
            SortItem item2,
            int [] indices)
        {
            foreach(int index_param in indices)
            {
                if (index_param == 0)
                    throw new ArgumentException("indices 中元素不允许出现 0。必须是从 1 (或者 -1)开始的值");

                // 1 表示 Path
                if (index_param == 1 || index_param == -1)
                {
                    int ret = ComparePath(item1.Path, item2.Path);
                    if (ret != 0)
                        return index_param * ret;
                }

                int index = Math.Abs(index_param) - 2;
                string s1 = "";
                if (item1.Cols != null && index < item1.Cols.Length)
                    s1 = item1.Cols[index];
                string s2 = "";
                if (item2.Cols != null && index < item2.Cols.Length)
                    s2 = item2.Cols[index];

                {
                    int ret = string.Compare(s1, s2);
                    if (ret != 0)
                    {
                        if (index_param < 0)
                            return -1 * ret;
                        return ret;
                    }
                }
            }

            return 0;
        }

        static int ComparePath(string path1, string path2)
        {
            var parts1 = StringUtil.ParseTwoPart(path1, "/");
            var parts2 = StringUtil.ParseTwoPart(path2, "/");
            int ret = string.Compare(parts1[0], parts2[0]);
            if (ret != 0)
                return ret;
            Int64.TryParse(parts1[1], out long id1);
            Int64.TryParse(parts2[1], out long id2);
            return (int)(id1 - id2);
        }
    }

    public class SortItem
    {
        public string Path { get; set; }
        public string[] Cols { get; set; }

    }
}
