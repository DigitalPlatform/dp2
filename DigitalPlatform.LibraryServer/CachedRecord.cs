using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 管理一批缓存的记录。避免中途多次反复不必要保存
    /// </summary>
    public class CachedRecordCollection : IEnumerable
    {
        List<CachedRecord> _records = new List<CachedRecord>();

        public void Add(CachedRecord record)
        {
            _records.Add(record);
        }

        public void Add(string recpath, XmlDocument dom, byte [] timestamp)
        {
            CachedRecord record = new CachedRecord();
            record.RecPath = recpath;
            record.Dom = dom;
            record.Timestamp = timestamp;
            _records.Add(record);
        }

        public CachedRecord Find(string recpath)
        {
            foreach(CachedRecord record in _records)
            {
                if (record.RecPath == recpath)
                    return record;
            }

            return null;
        }

        public void Clear()
        {
            _records.Clear();
        }

        public void Remove(string recpath)
        {
            CachedRecord record = Find(recpath);
            if (record != null)
                _records.Remove(record);
        }

        public IEnumerator GetEnumerator()
        {
            foreach(CachedRecord record in _records)
            {
                yield return record;
            }
        }
    }

    public class CachedRecord
    {
        public XmlDocument Dom { get; set; }
        public string RecPath { get; set; }
        public byte[] Timestamp { get; set; }

        public bool Changed { get; set; }
    }
}
