using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Security.Cryptography;
using MongoDB.Bson.Serialization.Attributes;

namespace DigitalPlatform.OPAC.Server
{
    /// <summary>
    /// 检索日志
    /// </summary>
    public class SearchLog : List<SearchLogItem>
    {
        OpacApplication App = null;

        ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();
        static int m_nLockTimeout = 5000;	// 5000=5秒

        MongoClient m_mongoClient = null;

        string m_strDatabaseName = "dp2opac_searchlog";

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

        // 写入数据库的时候， 可以锁定一个范围，后面可以继续膑并发追加记录，这样效率就很高了。
        // 只有在删除已经写入数据库的部分记录的瞬间，才需要锁定整个集合
        public void AddItem(SearchLogItem item)
        {
            if (this.m_lock.TryEnterReadLock(m_nLockTimeout) == false)
                throw new ApplicationException("锁定尝试中超时");
            try
            {
                base.Add(item);
            }
            finally
            {
                this.m_lock.ExitReadLock();
            }
        }

        public int Open(OpacApplication app,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(app.MongoDbConnStr) == true)
            {
                strError = "opac.xml 中尚未配置 <mongoDB> 元素的 connectString 属性，无法初始化 SearchLog 对象";
                return -1;
            }

            this.App = app;

            try
            {
                this.m_mongoClient = new MongoClient(app.MongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 SearchLog 时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

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

        // 将积累的内存对象保存到数据库中
        public int Flush(out string strError)
        {
            strError = "";

            try
            {
                List<SearchLogItem> whole = new List<SearchLogItem>();

                // 将打算写入数据库的内存对象移出容器，这样可以减少锁定时间
                if (this.m_lock.TryEnterWriteLock(m_nLockTimeout) == false)
                    throw new ApplicationException("锁定尝试中超时");
                try
                {
                    if (this.Count == 0)
                        return 0;

                    whole.AddRange(this);
                    this.Clear();
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
                    MongoCollection<SearchLogItem> db_items = this.DbItems;
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
}
