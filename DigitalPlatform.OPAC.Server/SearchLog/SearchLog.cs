using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security.Cryptography;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 检索日志
    /// </summary>
    public class SearchLog 
    {
        OpacApplication App = null;

        // 用于缓冲的内存集合结构
        // TODO: 如果尺寸达到一个极限，则需要强制写入数据库
        List<SearchLogItem> _searchLogCache = new List<SearchLogItem>();

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        MongoClient m_mongoClient = null;

        string m_strSearchLogDatabaseName = "";

        public static string BuildLogQueryString(
            string strDbName,
            string strWord,
            string strFrom,
            string strMatchStyle)
        {
            return "d=" + strDbName
                + ",w=" + strWord
                + ",f=" + strFrom
                + ",m=" + strMatchStyle;
        }

        // 写入数据库的时候， 可以锁定一个范围，后面可以继续并发追加记录，这样效率就很高了。
        // 只有在删除已经写入数据库的部分记录的瞬间，才需要锁定整个集合
        public void AddLogItem(SearchLogItem item)
        {
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                this._searchLogCache.Add(item);
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        // 初始化 SearchLog 对象
        // parameters:
        //      strEnable   启用哪些 log 功能? searchlog,hitcount
        public int Open(OpacApplication app,
            string strEnable,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(app.MongoDbConnStr) == true)
            {
                strError = "opac.xml 中尚未配置 <mongoDB> 元素的 connectString 属性，无法初始化 SearchLog 对象";
                return -1;
            }

            this.App = app;

            string strPrefix = app.MongoDbInstancePrefix;
            if (string.IsNullOrEmpty(strPrefix) == false)
                strPrefix = strPrefix + "_"; 

            m_strSearchLogDatabaseName = strPrefix + "searchlog";
            m_strHitCountDatabaseName = strPrefix + "hitcount";

            try
            {
                this.m_mongoClient = new MongoClient(app.MongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 SearchLog 时出错: " + ex.Message;
                return -1;
            }

            var server = m_mongoClient.GetServer();

            if (StringUtil.IsInList("hitcount", strEnable) == true)
            {
                var db = server.GetDatabase(this.m_strHitCountDatabaseName);

                _hitCountCollection = db.GetCollection<HitCountItem>("hitcount");
                if (_hitCountCollection.GetIndexes().Count == 0)
                    _hitCountCollection.CreateIndex(new IndexKeysBuilder().Ascending("URL"),
                        IndexOptions.SetUnique(true));
            }

            if (StringUtil.IsInList("searchlog", strEnable) == true)
            {
                var db = server.GetDatabase(this.m_strSearchLogDatabaseName);

                this._searchLogCollection = db.GetCollection<SearchLogItem>("log");
#if NO
                if (_searchLogCollection.GetIndexes().Count == 0)
                    _searchLogCollection.CreateIndex(new IndexKeysBuilder().Ascending("???"),
                        IndexOptions.SetUnique(true));
#endif
            }
            return 0;
        }

        MongoCollection<SearchLogItem> _searchLogCollection = null;

        public MongoCollection<SearchLogItem> SearchLogCollection
        {
            get
            {
                return this._searchLogCollection;
            }
        }

#if NO
        MongoCollection<SearchLogItem> DbItems
        {
            get
            {
                // var server = MongoServer.Create(this.App.MongoDbConnStr);
                var client = new MongoClient(this.App.MongoDbConnStr);
                var server = client.GetServer();

                var db = server.GetDatabase(this.m_strDatabaseName);

                //var options = CollectionOptions
                //    .SetCapped(true).

                return db.GetCollection<SearchLogItem>("log");
            }
        }
#endif

        // 将积累的内存对象保存到数据库中
        public int Flush(out string strError)
        {
            strError = "";

            if (this.SearchLogCollection == null)
            {
                this._searchLogCache.Clear();
                return 0;
            }

            try
            {
                List<SearchLogItem> whole = new List<SearchLogItem>();

                // 将打算写入数据库的内存对象移出容器，这样可以减少锁定时间
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new ApplicationException("锁定尝试中超时");
                try
                {
                    if (this._searchLogCache.Count == 0)
                        return 0;

                    whole.AddRange(this._searchLogCache);
                    this._searchLogCache.Clear();
                    // this.RemoveRange(0, nCount);
                }
                finally
                {
                    this.m_lock.ExitWriteLock();
                }

                if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                    throw new ApplicationException("锁定尝试中超时");
                try
                {
                    MongoCollection<SearchLogItem> db_items = this.SearchLogCollection;
                    MongoInsertOptions options = new MongoInsertOptions() { WriteConcern = WriteConcern.Unacknowledged };
                    foreach (SearchLogItem item in whole)
                    {
                        db_items.Insert(item, options);
                    }
                }
                finally
                {
                    this.m_lock.ExitReadLock();
                }

                // TODO: 是否考虑失败后把元素重新插入回this数组?

                return 1;
            }
            catch (Exception ex)
            {
                strError = "检索日志写入数据库的过程发生错误: " + ex.Message;
                return -1;
            }
        }

        #region 访问计数

        string m_strHitCountDatabaseName = "";
        MongoCollection<HitCountItem> _hitCountCollection = null;

        public MongoCollection<HitCountItem> HitCountCollection
        {
            get
            {
                return this._hitCountCollection;
            }
        }
#if NO
        MongoCollection<HitCountItem> HitCountItems
        {
            get
            {
                var client = new MongoClient(this.App.MongoDbConnStr);
                var server = client.GetServer();

                var db = server.GetDatabase(this.m_strHitCountDatabaseName);

                var result = db.GetCollection<HitCountItem>("hitcount");
                if (result.GetIndexes().Count == 0)
                    result.CreateIndex(new IndexKeysBuilder().Ascending("URL"),
                        IndexOptions.SetUnique(true));

                return result;
            }
        }
#endif

        // 增加一次访问计数
        public void IncHitCount(string strURL)
        {
            MongoCollection<HitCountItem> collection = this.HitCountCollection;
            if (collection == null)
                return;

            var query = new QueryDocument("URL", strURL);
            var update = Update.Inc("HitCount", 1);
            collection.Update(
    query,
    update,
    UpdateFlags.Upsert);
        }

        public long GetHitCount(string strURL)
        {
            MongoCollection<HitCountItem> collection = this.HitCountCollection;
            if (collection == null)
                return -1;

            var query = new QueryDocument("URL", strURL);

            var item = collection.FindOne(query);
            if (item == null)
                return 0;
            return item.HitCount;
        }

        #endregion
    }

    public class SearchLogItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }

        public DateTime Time {get;set;} // 访问时间
        public string IP {get;set;}  // 访问者的IP地址
        public string Query {get;set;}   // 检索词，或检索式
        public int HitCount { get; set; } // 命中记录数
        public string RecPath { get; set; } // 所获取的记录路径
        public string Format { get; set; }  // 呈现数据的格式
    }

    public class HitCountItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }

        public string URL { get; set; }  // 资源 URL
        public long HitCount { get; set; }   // 次数
    }

}
