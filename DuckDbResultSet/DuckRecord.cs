using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DuckDbResultSet
{
    public class DuckRecord // : IComparable
    {
        public long _rowID = 0;

        public string m_strDebugInfo = "";

        // 存放记录的逻辑完整ID，格式为:"@1/0000000001"
        string _id;

        // 存放记录的浏览HTML
        string _key;

        long _count = 0;

        // id
        public DuckRecord(string id,
            long row_id)
        {
            _id = id;
            _rowID = row_id;
        }

        // key+id
        public DuckRecord(string id,
            string key,
            long row_id)
        {
            _id = id;
            _key = key;
            _rowID = row_id;
        }

        // key+count
        public DuckRecord(string key,
            long count,
            long row_id)
        {
            _key = key;
            _count = count;
            _rowID = row_id;
        }

        //公共ID属性，表示记录完整逻辑ID，提供给外部代码访问
        public string ID
        {
            get
            {
                return _id;
            }
        }


        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        public long Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
            }
        }

        public long RowID
        {
            get
            {
                return _rowID;
            }
            set
            {
                _rowID = value;
            }
        }

#if REMOVED
        // 实现IComparable接口的CompareTo()方法,
        // 根据ID比较两个对象的大小，以便排序，
        // 按右对齐方式比较
        // obj: An object to compare with this instance
        // 返回值 A 32-bit signed integer that indicates the relative order of the comparands. The return value has these meanings:
        // Less than zero: This instance is less than obj.
        // Zero: This instance is equal to obj.
        // Greater than zero: This instance is greater than obj.
        // 异常: ArgumentException,obj is not the same type as this instance.
        public int CompareTo(object obj)
        {
            DuckRecord myRecord = (DuckRecord)obj;

            //m_strDebugInfo += strID1 + "---------" + strID2;

            //通过String类的静态方法Compare比较两个字符串的大小，返回值为小于0，等于0，大于0
            return String.Compare(this.ID, myRecord.ID);
        }

        // 2025/1/21
        public int CompareToKey(object obj)
        {
            DuckRecord myRecord = (DuckRecord)obj;
            var ret = String.Compare(this.BrowseText, myRecord.BrowseText);
            if (ret == 0)   // 如果 Key 相等，继续比较 ID 部分
            {
                return String.Compare(this.ID, myRecord.ID);
            }

            return ret;
        }
#endif
    }

}
