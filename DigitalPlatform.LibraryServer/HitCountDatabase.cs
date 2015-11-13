using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 对象计数器数据库。为每个 URL 维持一个计数值
    /// </summary>
    public class HitCountDatabase
    {
        MongoClient m_mongoClient = null;

        string m_strHitCountDatabaseName = "";
        MongoCollection<HitCountItem> _hitCountCollection = null;

        // 初始化
        // parameters:
        public int Open(
            string strMongoDbConnStr,
            string strInstancePrefix,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strMongoDbConnStr) == true)
            {
                strError = "strMongoDbConnStr 参数值不应为空";
                return -1;
            }

            if (string.IsNullOrEmpty(strInstancePrefix) == false)
                strInstancePrefix = strInstancePrefix + "_";

            m_strHitCountDatabaseName = strInstancePrefix + "hitcount";

            try
            {
                this.m_mongoClient = new MongoClient(strMongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 MongoClient 时出错: " + ex.Message;
                return -1;
            }

            var server = m_mongoClient.GetServer();

            {
                var db = server.GetDatabase(this.m_strHitCountDatabaseName);

                _hitCountCollection = db.GetCollection<HitCountItem>("hitcount");
                if (_hitCountCollection.GetIndexes().Count == 0)
                    _hitCountCollection.CreateIndex(new IndexKeysBuilder().Ascending("URL"),
                        IndexOptions.SetUnique(true));
            }

            return 0;
        }

        public MongoCollection<HitCountItem> HitCountCollection
        {
            get
            {
                return this._hitCountCollection;
            }
        }

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
