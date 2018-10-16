using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Threading;

namespace DigitalPlatform.LibraryServer
{
    public class AccessLogDatabase
    {
        // MongoClient _mongoClient = null;

        string _logDatabaseName = "";

        MongoCollection<AccessLogItem> _logCollection = null;

        DailyItemCount _itemCount = null;

        // 初始化
        // parameters:
        public int Open(
            // string strMongoDbConnStr,
            MongoClient client,
            string strInstancePrefix,
            out string strError)
        {
            strError = "";

#if NO
            if (string.IsNullOrEmpty(strMongoDbConnStr) == true)
            {
                strError = "strMongoDbConnStr 参数值不应为空";
                return -1;
            }
#endif

            if (string.IsNullOrEmpty(strInstancePrefix) == false)
                strInstancePrefix = strInstancePrefix + "_";

            _logDatabaseName = strInstancePrefix + "accessLog";

#if NO
            try
            {
                this._mongoClient = new MongoClient(strMongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 MongoClient 时出错: " + ex.Message;
                return -1;
            }
#endif

            var server = client.GetServer();

            {
                var db = server.GetDatabase(this._logDatabaseName);

                _logCollection = db.GetCollection<AccessLogItem>("accessLog");
                // _logCollection.DropAllIndexes();
                if (_logCollection.GetIndexes().Count == 0)
                {
#if NO
                    _logCollection.CreateIndex(new IndexKeysBuilder().Ascending("OperTime"),
                        IndexOptions.SetUnique(false));
#endif
                    CreateIndex();
                }
            }

            _itemCount = new DailyItemCount();
            string date = GetToday();
            _itemCount.SetValue(date, GetItemCount(date));
            return 0;
        }

        public void CreateIndex()
        {
            _logCollection.CreateIndex(new IndexKeysBuilder().Ascending("OperTime"),
    IndexOptions.SetUnique(false));
        }

        // 清除集合内的全部内容
        public int Clear(out string strError)
        {
            strError = "";

            if (_logCollection == null)
            {
                strError = "访问日志 mongodb 集合尚未初始化";
                return -1;
            }

            WriteConcernResult result = _logCollection.RemoveAll();
            CreateIndex();
            return 0;
        }

        // 获得今天的日期，8 字符格式
        static string GetToday()
        {
            return DateTimeUtil.DateTimeToString8(DateTime.Now);
        }

        public MongoCollection<AccessLogItem> LogCollection
        {
            get
            {
                return this._logCollection;
            }
        }

#if NO
        // parameters:
        //      maxItemCount    最大事项数。如果为 -1 表示不限制
        public bool Add(string operation,
            string path,
            long size,
            string mime,
            string clientAddress,
            long initial_hitcount,
            string operator_param,
            DateTime opertime,
            long maxItemCount)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return false;

            // 限制最大事项数
            {
                string date = GetToday();
                long newValue = _itemCount.GetValue(date, 1);
                if (maxItemCount != -1 && newValue > maxItemCount)
                {
                    _itemCount.GetValue(date, -1);
                    return false;
                }
            }

            var query = new QueryDocument("Path", path);
            query.Add("Operation", operation)
                .Add("Size", size)
                .Add("MIME", mime)
                .Add("ClientAddress", clientAddress)
                .Add("HitCount", initial_hitcount)
                .Add("Operator", operator_param)
                .Add("OperTime", opertime);
            collection.Insert(query);
            return true;
        }
#endif
        public bool Add(string operation,
    string path,
    long size,
    string mime,
    string clientAddress,
    long initial_hitcount,
    string operator_param,
    DateTime opertime,
    long maxItemCount)
        {
            AccessLogItem item = new AccessLogItem();
            item.Operation = operation;
            item.Path = path;
            item.Size = size;
            item.MIME = mime;
            item.ClientAddress = clientAddress;
            item.HitCount = initial_hitcount;
            item.Operator = operator_param;
            item.OperTime = opertime;

            return Add(item, maxItemCount);
        }

        // parameters:
        //      maxItemCount    最大事项数。如果为 -1 表示不限制
        public bool Add(AccessLogItem item,
            long maxItemCount)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return false;

            // 限制最大事项数
            {
                string date = GetToday();
                long newValue = _itemCount.GetValue(date, 1);
                if (maxItemCount != -1 && newValue > maxItemCount)
                {
                    _itemCount.GetValue(date, -1);
                    return false;
                }
            }

            collection.Insert(item);
            return true;
        }

#if NO
        // 增加一次访问计数
        // return:
        //      false   没有成功。通常因为 mongodb 无法打开等原因
        //      true    成功
        public bool IncHitCount(string strPath)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return false;

            var query = new QueryDocument("Path", strPath);
            var update = Update.Inc("HitCount", 1);
            collection.Update(
    query,
    update,
    UpdateFlags.Upsert);
            return true;
        }

        public long GetHitCount(string strPath)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return -1;

            var query = new QueryDocument("Path", strPath);

            var item = collection.FindOne(query);
            if (item == null)
                return 0;
            return item.HitCount;
        }
#endif

        public IEnumerable<AccessLogItem> Find(string date, int start)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return null;

            DateTime start_time = DateTimeUtil.Long8ToDateTime(date);
            DateTime end_time = start_time.AddDays(1);

            var query = Query.And(Query.GTE("OperTime", start_time),
                Query.LT("OperTime", end_time));
            return collection.Find(query).Skip(start);
        }

        // 获得一个日期的事项个数
        // return:
        //      -1  集合不存在
        //      >=0 数量
        public int GetItemCount(string date)
        {
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return -1;

            DateTime start_time = DateTimeUtil.Long8ToDateTime(date);
            DateTime end_time = start_time.AddDays(1);

            var query = Query.And(Query.GTE("OperTime", start_time),
                Query.LT("OperTime", end_time));

            // var keyFunction = (BsonJavaScript)@"{}";
            var keyFunction = (BsonJavaScript)@"function(doc) {
return { None : '' };
}"; // mongodb v3.4

            var document = new BsonDocument("count", 0);
            var result = collection.Group(
                query,
                keyFunction,
                document,
                new BsonJavaScript("function(doc, out){ out.count++; }"),
                null
            ).ToArray();

            foreach (BsonDocument doc in result)
            {
                return doc.GetValue("count", 0).ToInt32();
            }

            return 0;
        }

        public class ValueCount
        {
            public string Value = "";
            public int Count = 0;
        }

        // parameters:
        //      max 返回最多多少个元素。如果为 -1，表示不限制
        //      hit_count   返回命中总数。是命中的整个集合的数量，除去 start 以后的值
        public List<ValueCount> ListDates(int start, int max, out int hit_count)
        {
            hit_count = 0;
            MongoCollection<AccessLogItem> collection = this.LogCollection;
            if (collection == null)
                return null;

#if NO
            BasicDBObject key = new BasicDBObject();   
14.key.put("name", "true");   
15.BasicDBObject initial = new BasicDBObject();   
16.initial.put("count", 0);   
17.BasicDBObject condition = new BasicDBObject();   
18.// condition.put("name", "liguohui");   
19.String reduceString = "function(obj,prev) { prev.count++; }";   

20.DBObject dbo = coll.group(key, condition , initial, reduceString); 
#endif
            var document = new BsonDocument("count", 0);
            var keyFunction = (BsonJavaScript)@"function(doc) {
        var date = new Date(doc.OperTime);
        var dateKey = date.toISOString().slice(0, 10);
        return {'day':dateKey};
    }";
            //         var dateKey = date.getFullYear()+'|'+(date.getMonth()+1)+'|'+date.getDate();
            var result = collection.Group(
                Query.Null,
                keyFunction,
                document,
                new BsonJavaScript("function(doc, out){ out.count++; }"),
                null
            ).Skip<BsonDocument>(start);

            List<ValueCount> values = new List<ValueCount>();
            int nCount = 0;
            foreach (BsonDocument doc in result)
            {
                ValueCount item = new ValueCount();
                item.Value = doc.GetValue("day", "").ToString().Replace("-", "");
                item.Count = doc.GetValue("count", 0).ToInt32();
                values.Add(item);
                nCount++;
                if (max != -1 && nCount >= max)
                    break;
            }

            hit_count = result.Count<BsonDocument>();
            return values;
        }

        static string GetXml(AccessLogItem item)
        {
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");

            if (string.IsNullOrEmpty(item.LibraryCode) == false)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "libraryCode", item.LibraryCode);
            }
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation", "getRes");
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "path", item.Path);
            if (item.Size != 0)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "size", item.Size.ToString());
            }
            if (string.IsNullOrEmpty(item.MIME) == false)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "mime", item.MIME);
            }
            if (string.IsNullOrEmpty(item.ClientAddress) == false)
            {
                OperLog.WriteClientAddress(domOperLog, item.ClientAddress);
            }
            if (item.HitCount != 1)
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "hitCount", item.HitCount.ToString());
            }

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                item.Operator);

            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                DateTimeUtil.Rfc1123DateTimeStringEx(item.OperTime.ToLocalTime()));
            return domOperLog.OuterXml;
        }

        static void WriteClientAddress(XmlDocument dom,
    string strClientAddress)
        {
            if (string.IsNullOrEmpty(strClientAddress) == true)
                return;

            string strVia = "";
            int nRet = strClientAddress.IndexOf("@");
            if (nRet != -1)
            {
                strVia = strClientAddress.Substring(nRet + 1);
                strClientAddress = strClientAddress.Substring(0, nRet);
            }
            XmlNode node = DomUtil.SetElementText(dom.DocumentElement,
                "clientAddress",
                strClientAddress);
            if (string.IsNullOrEmpty(strVia) == false)
                DomUtil.SetAttr(node, "via", strVia);
        }

        const int MAX_FILENAME_COUNT = 100;

        // parameters:
        //      nCount  本次希望获取的记录数。如果==-1，表示希望尽可能多地获取
        // return:
        //      -1  error
        //      0   file not found
        //      1   succeed
        //      2   超过范围，本次调用无效
        public int GetOperLogs(
            string strLibraryCodeList,
            string strFileName,
            long lIndex,
            long lHint,
            int nCount,
            string strStyle,
            string strFilter,
            out OperLogInfo[] records,
            out string strError)
        {
            records = null;
            strError = "";
            List<OperLogInfo> results = new List<OperLogInfo>();

            if (StringUtil.IsInList("getfilenames", strStyle) == true)
            {
                int hit_count = 0;
                int nStart = (int)lIndex;
                List<ValueCount> dates = ListDates(nStart,   // bug???
                    MAX_FILENAME_COUNT, out hit_count);
                int nEnd = dates.Count;
                if (nCount == -1)
                    nEnd = dates.Count;
                else
                    nEnd = Math.Min(nStart + nCount, dates.Count);

                // 一次不让超过最大数量
                if (nEnd - nStart > MAX_FILENAME_COUNT)
                    nEnd = nStart + MAX_FILENAME_COUNT;

                for (int i = nStart; i < nEnd; i++)
                {
                    ValueCount item = dates[i];
                    OperLogInfo info = new OperLogInfo();
                    info.Index = i;
                    info.Xml = item.Value + ".log";
                    info.AttachmentLength = item.Count;
                    results.Add(info);
                }

#if NO
                records = new OperLogInfo[results.Count];
                results.CopyTo(records);
#endif
                records = results.ToArray();
                return (int)lIndex + hit_count;
#if NO
                DirectoryInfo di = new DirectoryInfo(this.m_strDirectory);
                FileInfo[] fis = di.GetFiles("????????.log");

                if (fis.Length == 0)
                    return 0;   // 一个文件也没有

                // 日期小者在前
                Array.Sort(fis, new FileInfoCompare(true));

                int nStart = (int)lIndex;
                int nEnd = fis.Length;
                if (nCount == -1)
                    nEnd = fis.Length;
                else
                    nEnd = Math.Min(nStart + nCount, fis.Length);

                // 一次不让超过最大数量
                if (nEnd - nStart > MAX_FILENAME_COUNT)
                    nEnd = nStart + MAX_FILENAME_COUNT;
                for (int i = nStart; i < nEnd; i++)
                {
                    OperLogInfo info = new OperLogInfo();
                    info.Index = i;
                    info.Xml = fis[i].Name;
                    info.AttachmentLength = fis[i].Length;
                    results.Add(info);
                }

                records = new OperLogInfo[results.Count];
                results.CopyTo(records);
                return 1;
#endif
            }

            if (string.IsNullOrEmpty(strFileName) == true
                || strFileName.Length < 8)
            {
                strError = "strFileName 参数值不能为空，或者长度小于 8 字符";
                return -1;
            }

            string date = strFileName.Substring(0, 8);
            IEnumerable<AccessLogItem> collection = Find(date, (int)lIndex);
            if (collection == null)
                return 0;
            List<OperLogInfo> infos = new List<OperLogInfo>();
            foreach (AccessLogItem item in collection)
            {
                OperLogInfo info = new OperLogInfo();
                info.AttachmentLength = 0;
                info.HintNext = 0;
                info.Index = lIndex++;
                info.Xml = GetXml(item);
                infos.Add(info);
            }
            if (infos.Count == 0)
                return 2;

            records = infos.ToArray();
            return 1;
        }
    }

    public class AccessLogItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }

        public string LibraryCode { get; set; } // 访问者的图书馆代码
        public string Operation { get; set; } // 操作名
        public string Path { get; set; } // 所获取的记录路径
        public long Size { get; set; }    // 对象大小
        public string MIME { get; set; }  // 媒体类型
        public string ClientAddress { get; set; }  // 访问者的IP地址
        public long HitCount { get; set; } // 访问次数。一般为 1

        public string Operator { get; set; }  // 操作者(访问者)
        //[BsonRepresentation(BsonType.DateTime)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OperTime { get; set; } // 操作时间
    }

    public class DailyItemCount
    {
        private static readonly Object syncRoot = new Object();

        public string Date = "";    // 日期。8 字符格式
        public long Count = 0;  // 当日内计数

        public long GetValue(string strDate, int nInc)
        {
            lock (syncRoot)
            {
                if (strDate != this.Date)
                {
                    this.Date = strDate;
                    this.Count = nInc;
                    return this.Count;
                }

                this.Count += nInc;
                return this.Count;
            }
        }

        public void SetValue(string strDate, long lValue)
        {
            lock (syncRoot)
            {
                this.Date = strDate;
                this.Count = lValue;
            }
        }
    }

    public class DailyItemCountTable
    {
        public int MAX_COUNT = 1000;
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        Dictionary<string, DailyItemCount> _table = new Dictionary<string, DailyItemCount>();

        public DailyItemCount FindItem(string name)
        {
            _lock.EnterReadLock();
            try
            {
                if (_table.ContainsKey(name) == false)
                    return null;
                return _table[name];
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public DailyItemCount GetItem(string name)
        {
            DailyItemCount item = FindItem(name);
            if (item != null)
                return item;

            _lock.EnterWriteLock();
            try
            {
                if (_table.ContainsKey(name))
                    return _table[name];
                if (_table.Count > MAX_COUNT)
                    throw new Exception("DailyItem 数量超过 " + MAX_COUNT);

                item = new DailyItemCount();
                _table.Add(name, item);
                return item;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public long GetValue(string name,
            string strDate,
            int nInc)
        {
            DailyItemCount item = GetItem(name);

            return item.GetValue(strDate, nInc);
        }

        public void SetValue(string name,
            string strDate, 
            long lValue)
        {
            DailyItemCount item = GetItem(name);

            item.SetValue(strDate, lValue);
        }

    }
}
