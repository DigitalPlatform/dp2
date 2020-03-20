using System.Linq;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 对象计数器数据库。为每个 URL 维持一个计数值
    /// </summary>
    public class HitCountDatabase
    {
        string m_strHitCountDatabaseName = "";
        IMongoCollection<HitCountItem> _hitCountCollection = null;

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

            m_strHitCountDatabaseName = strInstancePrefix + "hitcount";

#if NO
            try
            {
                this.m_mongoClient = new MongoClient(strMongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 MongoClient 时出错: " + ex.Message;
                return -1;
            }
#endif

            // var server = client.GetServer();

            {
                // var db = server.GetDatabase(this.m_strHitCountDatabaseName);
                var db = client.GetDatabase(this.m_strHitCountDatabaseName);

                _hitCountCollection = db.GetCollection<HitCountItem>("hitcount");
                // if (_hitCountCollection.GetIndexes().Count == 0)
                if (_hitCountCollection.Indexes.List().ToList().Count == 0)
                    CreateIndex();
            }

            return 0;
        }

        public void CreateIndex()
        {

            {
                var indexModel = new CreateIndexModel<HitCountItem>(
        Builders<HitCountItem>.IndexKeys.Ascending(_ => _.URL),
        new CreateIndexOptions() { Unique = true });
                _hitCountCollection.Indexes.CreateOne(indexModel);
            }
        }

#if OLD
        public void CreateIndex()
        {
            _hitCountCollection.CreateIndex(new IndexKeysBuilder().Ascending("URL"),
                IndexOptions.SetUnique(true));
        }
#endif
        // 清除集合内的全部内容
        public int Clear(out string strError)
        {
            strError = "";

            if (_hitCountCollection == null)
            {
                strError = "访问计数 mongodb 集合尚未初始化";
                return -1;
            }

            // WriteConcernResult result = _hitCountCollection.RemoveAll();
            var result = _hitCountCollection.DeleteMany(Builders<HitCountItem>.Filter.Empty);    //  RemoveAll();
            CreateIndex();
            return 0;
        }

        public IMongoCollection<HitCountItem> HitCountCollection
        {
            get
            {
                return this._hitCountCollection;
            }
        }

        // 增加一次访问计数
        // return:
        //      false   没有成功。通常因为 mongodb 无法打开等原因
        //      true    成功
        public bool IncHitCount(string strURL)
        {
            IMongoCollection<HitCountItem> collection = this.HitCountCollection;
            if (collection == null)
                return false;

            var updateDef = Builders<HitCountItem>.Update.Inc(o => o.HitCount, 1);

            collection.UpdateOne(
                o => o.URL == strURL,
                updateDef, 
                new UpdateOptions { IsUpsert = true });
            return true;
        }

#if OLD
        // 增加一次访问计数
        // return:
        //      false   没有成功。通常因为 mongodb 无法打开等原因
        //      true    成功
        public bool IncHitCount(string strURL)
        {
            IMongoCollection<HitCountItem> collection = this.HitCountCollection;
            if (collection == null)
                return false;

            var query = new QueryDocument("URL", strURL);
            var update = Update.Inc("HitCount", 1);
            collection.Update(
    query,
    update,
    UpdateFlags.Upsert);
            return true;
        }
#endif

        public long GetHitCount(string strURL)
        {
            IMongoCollection<HitCountItem> collection = this.HitCountCollection;
            if (collection == null)
                return -1;

            var item = collection.Find(Builders<HitCountItem>.Filter.Eq("URL", strURL))
                .FirstOrDefault();
            if (item == null)
                return 0;
            return item.HitCount;

            /*
            var query = new QueryDocument("URL", strURL);

            var item = collection.FindOne(query);
            if (item == null)
                return 0;
            return item.HitCount;
            */
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
