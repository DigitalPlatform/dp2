using System.Runtime.Caching;
using System.Xml;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是书目摘要缓存相关的代码
    /// 书目摘要被缓存在一个 mongodb 数据库里
    /// </summary>
    public partial class LibraryApplication
    {
#if NO
        public MemoryCache BiblioSummaryCache = null;

        internal void InitialBiblioSummaryCache()
        {
            // this.BiblioSummaryCache = new MemoryCache("bibliosummary");
        }

        // 清除 BiblioSummaryCache
        internal void ClearBiblioSummaryCache(string strItemRecPath)
        {
            if (this.BiblioSummaryCache != null)
            {
                // 清除全部事项
                if (string.IsNullOrEmpty(strItemRecPath) == true)
                {
                    MemoryCache old = this.LoginCache;
                    this.BiblioSummaryCache = new MemoryCache("bibliosummary");
                    old.Dispose();
                    return;
                }
                this.BiblioSummaryCache.Remove(strItemRecPath);
            }
        }
#endif

        public string MongoDbConnStr = "";
        public string MongoDbInstancePrefix = ""; // MongoDB 的实例字符串。用于区分不同的 dp2OPAC 实例在同一 MongoDB 实例中创建的数据库名，这个实例名被用作数据库名的前缀字符串

        internal MongoClient _mongoClient = null;

        string _summaryDbName = "";

        // 打开书目摘要数据库
        // parameters:
        public int OpenSummaryStorage(out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this.MongoDbConnStr) == true)
            {
                strError = "library.xml 中尚未配置 <mongoDB> 元素的 connectString 属性，无法打开书目摘要库";
                return -1;
            }

            string strPrefix = this.MongoDbInstancePrefix;
            if (string.IsNullOrEmpty(strPrefix) == false)
                strPrefix = strPrefix + "_";

            _summaryDbName = strPrefix + "bibliosummary";

#if NO
            try
            {
                this._mongoClient = new MongoClient(this.MongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 MongoClient 时出错: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
#endif
            if (this._mongoClient == null)
            {
                strError = "this._mongoClient == null";
                return -1;
            }

            // var server = this._mongoClient.GetServer();

            {
                // var db = server.GetDatabase(this._summaryDbName);
                var db = this._mongoClient.GetDatabase(this._summaryDbName);

                this._summaryCollection = db.GetCollection<SummaryItem>("summary");
                // if (_summaryCollection.GetIndexes().Count == 0)
                if (_summaryCollection.Indexes.List().ToList().Count == 0)
                {
                    //_summaryCollection.CreateIndex(new IndexKeysBuilder().Ascending("BiblioRecPath"),
                    //    IndexOptions.SetUnique(true));
                    CreateBiblioSummaryIndex();
                }
            }

            // 2021/12/23
            // 执行 library.xml 中的 commands/command
            var commands = this.LibraryCfgDom.DocumentElement.SelectNodes("commands/command[@name='_initial_biblioSummary_db']");
            if (commands.Count > 0)
            {
                ClearBiblioSummaryDb();
                foreach(XmlElement command in commands)
                {
                    command.ParentNode.RemoveChild(command);
                }

                // 通知 library.xml 发生了变化
                this.Changed = true;
                this.ActivateManagerThread();
            }

            return 0;
        }

        IMongoCollection<SummaryItem> _summaryCollection = null;

        public IMongoCollection<SummaryItem> SummaryCollection
        {
            get
            {
                return this._summaryCollection;
            }
        }

        // 设置书目摘要
        public void SetBiblioSummary(string strBiblioRecPath, string strSummary, string strImageFragment)
        {
            IMongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            /*
            var query = new QueryDocument("BiblioRecPath", strBiblioRecPath);
            var update = Update.SetOnInsert("BiblioRecPath", strBiblioRecPath)
            .Set("Summary", strSummary)
            .Set("ImageFragment", strImageFragment);
            collection.Update(
    query,
    update,
    UpdateFlags.Upsert);
    */
            var updateDef = Builders<SummaryItem>.Update
                .SetOnInsert(_ => _.BiblioRecPath, strBiblioRecPath)
                .Set(_ => _.Summary, strSummary)
                .Set(_ => _.ImageFragment, strImageFragment);

            collection.UpdateOne(
                o => o.BiblioRecPath == strBiblioRecPath,
                updateDef,
                new UpdateOptions { IsUpsert = true });

        }

        // 删除书目摘要
        public void DeleteBiblioSummary(string strBiblioRecPath)
        {
            IMongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            collection.FindOneAndDelete(_ => _.BiblioRecPath == strBiblioRecPath);
            /*
            var query = new QueryDocument("BiblioRecPath", strBiblioRecPath);
            collection.Remove(query);
            */
        }

        // 删除书目摘要
        public void DeleteBiblioSummaryByDbName(string strBiblioDbName)
        {
            IMongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            collection.FindOneAndDelete(_ => _.BiblioRecPath.StartsWith(strBiblioDbName + "/"));

            /*
            var query = Query.Matches("BiblioRecPath", new BsonRegularExpression("^" + strBiblioDbName + "/\\d+"));
            collection.Remove(query);
            */
        }

        // 获得书目摘要
        public SummaryItem GetBiblioSummary(string strBiblioRecPath)
        {
            IMongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return null;

            return collection.Find(_ => _.BiblioRecPath == strBiblioRecPath).FirstOrDefault();

            /*
            var query = new QueryDocument("BiblioRecPath", strBiblioRecPath);

            return collection.FindOne(query);
            */
        }

        // 清除集合内的全部内容
        public void ClearBiblioSummaryDb()
        {
            IMongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            // WriteConcernResult result = collection.RemoveAll();
            var result = collection.DeleteMany(FilterDefinition<SummaryItem>.Empty);
            CreateBiblioSummaryIndex();
        }

        public void CreateBiblioSummaryIndex()
        {
            {
                var indexModel = new CreateIndexModel<SummaryItem>(
        Builders<SummaryItem>.IndexKeys.Ascending(_ => _.BiblioRecPath),
        new CreateIndexOptions() { Unique = true });
                _summaryCollection.Indexes.CreateOne(indexModel);
            }
        }

#if OLD
        public void CreateBiblioSummaryIndex()
        {
            _summaryCollection.CreateIndex(new IndexKeysBuilder().Ascending("BiblioRecPath"),
    IndexOptions.SetUnique(true));
        }
#endif
    }

    // 书目摘要对象
    public class SummaryItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }

        [BsonRepresentation(BsonType.String)]
        public string BiblioRecPath { get; set; }  // 书目记录路径
        [BsonRepresentation(BsonType.String)]
        public string Summary { get; set; }   // 书目摘要
        [BsonRepresentation(BsonType.String)]
        public string ImageFragment { get; set; } // Image 片段
    }
}
