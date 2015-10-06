using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是书目缓存相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
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

        public string MongoDbConnStr = "";
        public string MongoDbInstancePrefix = ""; // MongoDB 的实例字符串。用于区分不同的 dp2OPAC 实例在同一 MongoDB 实例中创建的数据库名，这个实例名被用作数据库名的前缀字符串

        MongoClient m_mongoClient = null;

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

            try
            {
                this.m_mongoClient = new MongoClient(this.MongoDbConnStr);
            }
            catch (Exception ex)
            {
                strError = "初始化 MongoClient 时出错: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            var server = m_mongoClient.GetServer();

            {
                var db = server.GetDatabase(this._summaryDbName);

                this._summaryCollection = db.GetCollection<SummaryItem>("summary");
                if (_summaryCollection.GetIndexes().Count == 0)
                    _summaryCollection.CreateIndex(new IndexKeysBuilder().Ascending("BiblioRecPath"),
                        IndexOptions.SetUnique(true));
            }
            return 0;
        }

        MongoCollection<SummaryItem> _summaryCollection = null;

        public MongoCollection<SummaryItem> SummaryCollection
        {
            get
            {
                return this._summaryCollection;
            }
        }

        // 设置书目摘要
        public void SetBiblioSummary(string strBiblioRecPath, string strSummary, string strImageFragment)
        {
            MongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            var query = new QueryDocument("BiblioRecPath", strBiblioRecPath);
            var update = Update.SetOnInsert("BiblioRecPath", strBiblioRecPath).Set("Summary", strSummary).Set("ImageFragment", strImageFragment);
            collection.Update(
    query,
    update,
    UpdateFlags.Upsert);
        }

        // 删除书目摘要
        public void DeleteBiblioSummary(string strBiblioRecPath)
        {
            MongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            var query = new QueryDocument("BiblioRecPath", strBiblioRecPath);
            collection.Remove(
    query);
        }

        // 删除书目摘要
        public void DeleteBiblioSummaryByDbName(string strBiblioDbName)
        {
            MongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return;

            var query = Query.Matches("BiblioRecPath", new BsonRegularExpression("^" + strBiblioDbName + "/\\d+"));
            collection.Remove(query);
        }

        // 获得书目摘要
        public SummaryItem GetBiblioSummary(string strBiblioRecPath)
        {
            MongoCollection<SummaryItem> collection = this.SummaryCollection;
            if (collection == null)
                return null;

            var query = new QueryDocument("BiblioRecPath", strBiblioRecPath);

            return collection.FindOne(query);
        }
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
