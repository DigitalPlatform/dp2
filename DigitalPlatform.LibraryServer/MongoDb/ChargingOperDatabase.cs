using System;
using System.Collections.Generic;
using System.Linq;
using DigitalPlatform.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 存储流通借还操作动作信息的数据库
    /// </summary>
    public class ChargingOperDatabase : MongoDatabase<ChargingOperItem>
    {
        public ChargingOperDatabase()
        {
            _databaseName = "chargingOper";
        }

        public override void CreateIndex()
        {
            {
                var indexModel = new CreateIndexModel<ChargingOperItem>(
        Builders<ChargingOperItem>.IndexKeys.Ascending(_ => _.OperTime),
        new CreateIndexOptions() { Unique = false });
                _collection.Indexes.CreateOne(indexModel);
            }

            {
                var indexModel = new CreateIndexModel<ChargingOperItem>(
        Builders<ChargingOperItem>.IndexKeys.Ascending(_ => _.ItemBarcode),
        new CreateIndexOptions() { Unique = false });
                _collection.Indexes.CreateOne(indexModel);
            }

            {
                var indexModel = new CreateIndexModel<ChargingOperItem>(
        Builders<ChargingOperItem>.IndexKeys.Ascending(_ => _.PatronBarcode),
        new CreateIndexOptions() { Unique = false });
                _collection.Indexes.CreateOne(indexModel);
            }

            {
                var indexModel = new CreateIndexModel<ChargingOperItem>(
        Builders<ChargingOperItem>.IndexKeys.Ascending(_ => _.BiblioRecPath),
        new CreateIndexOptions() { Unique = false });
                _collection.Indexes.CreateOne(indexModel);
            }
        }

#if OLD
        public override void CreateIndex()
        {
            _collection.CreateIndex(new IndexKeysBuilder().Ascending("OperTime"),
                IndexOptions.SetUnique(false));

            _collection.CreateIndex(new IndexKeysBuilder().Ascending("ItemBarcode"),
                IndexOptions.SetUnique(false));

            _collection.CreateIndex(new IndexKeysBuilder().Ascending("PatronBarcode"),
                IndexOptions.SetUnique(false));

            // 2016/1/9
            _collection.CreateIndex(new IndexKeysBuilder().Ascending("BiblioRecPath"),
                IndexOptions.SetUnique(false));
        }
#endif
        // parameters:
        public bool Add(ChargingOperItem item)
        {
            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return false;
            // collection.Insert(item);
            collection.InsertOne(item);
            return true;
        }

        public FilterDefinition<ChargingOperItem> BuildQuery(
            string patronBarcode,
            string itemBarcode,
            string biblioRecPath,
            string volume,
            DateTime startTime,
            DateTime endTime,
            string operTypes)
        {
            List<FilterDefinition<ChargingOperItem>> and_items = new List<FilterDefinition<ChargingOperItem>>();
            {
                var time_query = Builders<ChargingOperItem>.Filter.And(
                    Builders<ChargingOperItem>.Filter.Gte("OperTime", startTime),
                    Builders<ChargingOperItem>.Filter.Lt("OperTime", endTime));

                if (startTime == new DateTime(0) && endTime == new DateTime(0))
                    time_query = Builders<ChargingOperItem>.Filter.Gte("OperTime", startTime);
                else if (startTime == new DateTime(0))
                    time_query = Builders<ChargingOperItem>.Filter.Lt("OperTime", endTime);
                else if (endTime == new DateTime(0))
                    time_query = Builders<ChargingOperItem>.Filter.Gte("OperTime", startTime);

                and_items.Add(time_query);
            }

            if (string.IsNullOrEmpty(patronBarcode) == false)
                and_items.Add(Builders<ChargingOperItem>.Filter.Eq("PatronBarcode", patronBarcode));

            if (string.IsNullOrEmpty(itemBarcode) == false
                && string.IsNullOrEmpty(biblioRecPath) == false)
            {
                // 两个条件只要满足一个即可
                and_items.Add(
                        Builders<ChargingOperItem>.Filter.Or(
                            Builders<ChargingOperItem>.Filter.Eq("ItemBarcode", itemBarcode),
                            Builders<ChargingOperItem>.Filter.Eq("BiblioRecPath", biblioRecPath))
                    );
            }
            else
            {
                if (string.IsNullOrEmpty(itemBarcode) == false)
                    and_items.Add(Builders<ChargingOperItem>.Filter.Eq("ItemBarcode", itemBarcode));
                if (string.IsNullOrEmpty(biblioRecPath) == false)
                    and_items.Add(Builders<ChargingOperItem>.Filter.Eq("BiblioRecPath", biblioRecPath));
            }

            if (string.IsNullOrEmpty(volume) == false
                && string.IsNullOrEmpty(itemBarcode) == true)   // 只有 itemBarcode 为空的时候，才匹配 volume
            {
                // and_items.Add(Query.EQ("No", volume));
                and_items.Add(Builders<ChargingOperItem>.Filter.Eq("Volume", volume));
            }

            {
                List<FilterDefinition<ChargingOperItem>> action_items = new List<FilterDefinition<ChargingOperItem>>();

                string[] types = operTypes.Split(new char[] { ',' });
                foreach (string type in types)
                {
                    if (type == "borrow")
                        action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "borrow"));
                    if (type == "return")
                        action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "return"));
                    if (type == "renew")
                        action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "renew"));
                    if (type == "lost")
                        action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "lost"));
                    if (type == "read")
                        action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "read"));
                }

                var type_query = Builders<ChargingOperItem>.Filter.And(
                    Builders<ChargingOperItem>.Filter.Or(
                        Builders<ChargingOperItem>.Filter.Eq("Operation", "borrow"),
                        Builders<ChargingOperItem>.Filter.Eq("Operation", "return")),
                    Builders<ChargingOperItem>.Filter.Or(action_items));
                and_items.Add(type_query);
            }

            return Builders<ChargingOperItem>.Filter.And(and_items);
        }

#if OLD
        // 构造检索特定读者特定册条码号的特定事项的 Query
        // parameters:
        //      patronBarcode   读者证条码号。纯净的号码
        //      itemBarcode 册条码号
        public IMongoQuery BuildQuery(
            string patronBarcode,
            string itemBarcode,
            string biblioRecPath,
            string volume,
            DateTime startTime,
            DateTime endTime,
            string operTypes)
        {
            List<IMongoQuery> and_items = new List<IMongoQuery>();

            {
                var time_query = Query.And(Query.GTE("OperTime", startTime),
                    Query.LT("OperTime", endTime));

                if (startTime == new DateTime(0) && endTime == new DateTime(0))
                    time_query = Query.GTE("OperTime", startTime);
                else if (startTime == new DateTime(0))
                    time_query = Query.LT("OperTime", endTime);
                else if (endTime == new DateTime(0))
                    time_query = Query.GTE("OperTime", startTime);

                and_items.Add(time_query);
            }

            if (string.IsNullOrEmpty(patronBarcode) == false)
                and_items.Add(Query.EQ("PatronBarcode", patronBarcode));

            if (string.IsNullOrEmpty(itemBarcode) == false
                && string.IsNullOrEmpty(biblioRecPath) == false)
            {
                // 两个条件只要满足一个即可
                and_items.Add(
                        Query.Or(Query.EQ("ItemBarcode", itemBarcode),
                            Query.EQ("BiblioRecPath", biblioRecPath))
                    );
            }
            else
            {
                if (string.IsNullOrEmpty(itemBarcode) == false)
                    and_items.Add(Query.EQ("ItemBarcode", itemBarcode));
                if (string.IsNullOrEmpty(biblioRecPath) == false)
                    and_items.Add(Query.EQ("BiblioRecPath", biblioRecPath));
            }

            if (string.IsNullOrEmpty(volume) == false
                && string.IsNullOrEmpty(itemBarcode) == true)   // 只有 itemBarcode 为空的时候，才匹配 volume
            {
                // and_items.Add(Query.EQ("No", volume));
                and_items.Add(Query.EQ("Volume", volume));
            }

            {
                List<IMongoQuery> action_items = new List<IMongoQuery>();
                string[] types = operTypes.Split(new char[] { ',' });
                foreach (string type in types)
                {
                    if (type == "borrow")
                        action_items.Add(Query.EQ("Action", "borrow"));
                    if (type == "return")
                        action_items.Add(Query.EQ("Action", "return"));
                    if (type == "renew")
                        action_items.Add(Query.EQ("Action", "renew"));
                    if (type == "lost")
                        action_items.Add(Query.EQ("Action", "lost"));
                    if (type == "read")
                        action_items.Add(Query.EQ("Action", "read"));
                }

                var type_query = Query.And(Query.Or(Query.EQ("Operation", "borrow"), Query.EQ("Operation", "return")),
                    Query.Or(action_items));
                and_items.Add(type_query);
            }

            return Query.And(and_items);
        }
#endif

        // 构造 Query
        // parameters:
        //      patronBarcode   读者证条码号。
        //                      如果 以 "@itemBarcode:" 前缀引导，表示这是册条码号
        //                      如果 以 "@itemRefID:" 前缀引导，表示这是册参考 ID
        //                      如果 以 "@readerRefID:" 或 "@refID:" 前缀引导，表示这是读者参考 ID
        //      orderTypes      希望获取信息的操作类型列表。逗号间隔的内容
        //                      由以下内容组合而成
        //                      "borrow"
        //                      "return"
        //                      "renew"
        //                      "lost"
        //                      "read"
        //                      如果没有指定，表示所有操作类型都满足条件
        public FilterDefinition<ChargingOperItem> BuildQuery(
            string patronBarcode,
            DateTime startTime,
            DateTime endTime,
            string operTypes)
        {
            /*
            if (string.IsNullOrEmpty(operTypes) || string.IsNullOrWhiteSpace(operTypes))
                operTypes = "borrow,return,renew,lost,read";
            */

            var time_query = Builders<ChargingOperItem>.Filter.And(
                Builders<ChargingOperItem>.Filter.Gte("OperTime", startTime),
                Builders<ChargingOperItem>.Filter.Lt("OperTime", endTime));

            if (startTime == new DateTime(0) && endTime == new DateTime(0))
                time_query = Builders<ChargingOperItem>.Filter.Gte("OperTime", startTime);
            else if (startTime == new DateTime(0))
                time_query = Builders<ChargingOperItem>.Filter.Lt("OperTime", endTime);
            else if (endTime == new DateTime(0))
                time_query = Builders<ChargingOperItem>.Filter.Gte("OperTime", startTime);

            string itemBarcodePrefix = "@itemBarcode:";
            string itemRefIdPrefix = "@itemRefID:";
            string readerRefIdPrefix = "@readerRefID:";

            FilterDefinition<ChargingOperItem> patron_query = null;
            if (patronBarcode == "!all")    // 所有读者和图书的借阅历史
                patron_query = Builders<ChargingOperItem>.Filter.Or(
                    Builders<ChargingOperItem>.Filter.Exists("PatronBarcode"),
                    Builders<ChargingOperItem>.Filter.Exists("ItemBarcode"));
            else if (patronBarcode == "!patron")    // 所有读者的借阅历史
                patron_query = Builders<ChargingOperItem>.Filter.Exists("PatronBarcode");
            else if (patronBarcode == "!item")  // 所有图书的借阅历史
                patron_query = Builders<ChargingOperItem>.Filter.Exists("ItemBarcode");
            else if (patronBarcode != null
                && patronBarcode.StartsWith(itemBarcodePrefix) == true)
                patron_query = Builders<ChargingOperItem>.Filter.Eq("ItemBarcode", patronBarcode.Substring(itemBarcodePrefix.Length));
            else if (patronBarcode != null
                && patronBarcode.StartsWith(itemRefIdPrefix) == true)
                patron_query = Builders<ChargingOperItem>.Filter.Eq("ItemBarcode", "@refID:" + patronBarcode.Substring(itemRefIdPrefix.Length));
            else if (patronBarcode != null
                && patronBarcode.StartsWith(readerRefIdPrefix) == true)
                patron_query = Builders<ChargingOperItem>.Filter.Eq("PatronBarcode", "@refID:" + patronBarcode.Substring(readerRefIdPrefix.Length));
            else
                patron_query = Builders<ChargingOperItem>.Filter.Eq("PatronBarcode", patronBarcode);

            List<FilterDefinition<ChargingOperItem>> action_items = new List<FilterDefinition<ChargingOperItem>>();
            string[] types = operTypes.Split(new char[] { ',' });
            foreach (string type in types)
            {
                if (type == "borrow")
                    action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "borrow"));
                if (type == "return")
                    action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "return"));
                if (type == "renew")
                    action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "renew"));
                if (type == "lost")
                    action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "lost"));
                if (type == "read")
                    action_items.Add(Builders<ChargingOperItem>.Filter.Eq("Action", "read"));
            }

            FilterDefinition<ChargingOperItem> type_query = null;
            // 2025/4/27
            if (action_items.Count == 0)
                type_query = Builders<ChargingOperItem>.Filter.Or(
                    Builders<ChargingOperItem>.Filter.Eq("Operation", "borrow"),
                    Builders<ChargingOperItem>.Filter.Eq("Operation", "return"));
            else
                type_query = Builders<ChargingOperItem>.Filter.And(
                    Builders<ChargingOperItem>.Filter.Or(
                        Builders<ChargingOperItem>.Filter.Eq("Operation", "borrow"),
                        Builders<ChargingOperItem>.Filter.Eq("Operation", "return")),
                    Builders<ChargingOperItem>.Filter.Or(action_items));

            return Builders<ChargingOperItem>.Filter.And(
                patron_query,
                time_query, 
                type_query);
        }

#if OLD
        // 构造 Query
        // parameters:
        //      patronBarcode   读者证条码号。如果 以 "@itemBarcode:" 前缀引导，表示这是册条码号
        public IMongoQuery BuildQuery(
            string patronBarcode,
            DateTime startTime,
            DateTime endTime,
            string operTypes)
        {
            var time_query = Query.And(Query.GTE("OperTime", startTime),
                Query.LT("OperTime", endTime));

            if (startTime == new DateTime(0) && endTime == new DateTime(0))
                time_query = Query.GTE("OperTime", startTime);
            else if (startTime == new DateTime(0))
                time_query = Query.LT("OperTime", endTime);
            else if (endTime == new DateTime(0))
                time_query = Query.GTE("OperTime", startTime);

            string itemBarcodePrefix = "@itemBarcode:";
            string itemRefIdPrefix = "@itemRefID:";

            IMongoQuery patron_query = null;
            if (patronBarcode == "!all")    // 所有读者和图书的借阅历史
                patron_query = Query.Or(Query.Exists("PatronBarcode"), Query.Exists("ItemBarcode"));
            else if (patronBarcode == "!patron")    // 所有读者的借阅历史
                patron_query = Query.Exists("PatronBarcode");
            else if (patronBarcode == "!item")  // 所有图书的借阅历史
                patron_query = Query.Exists("ItemBarcode");
            else if (patronBarcode != null
                && patronBarcode.StartsWith(itemBarcodePrefix) == true)
                patron_query = Query.EQ("ItemBarcode", patronBarcode.Substring(itemBarcodePrefix.Length));
            else if (patronBarcode != null
                && patronBarcode.StartsWith(itemRefIdPrefix) == true)
                patron_query = Query.EQ("ItemBarcode", "@refID:" + patronBarcode.Substring(itemRefIdPrefix.Length));
            else
                patron_query = Query.EQ("PatronBarcode", patronBarcode);

            List<IMongoQuery> action_items = new List<IMongoQuery>();
            string[] types = operTypes.Split(new char[] { ',' });
            foreach (string type in types)
            {
                if (type == "borrow")
                    action_items.Add(Query.EQ("Action", "borrow"));
                if (type == "return")
                    action_items.Add(Query.EQ("Action", "return"));
                if (type == "renew")
                    action_items.Add(Query.EQ("Action", "renew"));
                if (type == "lost")
                    action_items.Add(Query.EQ("Action", "lost"));
                if (type == "read")
                    action_items.Add(Query.EQ("Action", "read"));
            }

            var type_query = Query.And(Query.Or(Query.EQ("Operation", "borrow"), Query.EQ("Operation", "return")),
                Query.Or(action_items));

            return Query.And(patron_query, time_query, type_query);
        }

#endif

#if NO
        static IMongoQuery _rel_type_query = null;   // 存储起来，避免每次创建的消耗
                    if (_rel_type_query == null)
            {
                List<IMongoQuery> action_items = new List<IMongoQuery>();
                {
                    action_items.Add(Query.EQ("Action", "return"));
                    action_items.Add(Query.EQ("Action", "renew"));
                    action_items.Add(Query.EQ("Action", "lost"));
                }

                _rel_type_query = Query.And(Query.Or(Query.EQ("Operation", "borrow"), Query.EQ("Operation", "return")),
        Query.Or(action_items));
            }
#endif

        // 查找和本还书 item 关联的的借书操作 item
        public ChargingOperItem FindRelativeBorrowItem(ChargingOperItem return_item)
        {
            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return null;

            // TODO: 应该改进为利用事务 ID 来查找相关的 ChargingOperItem 事项
            var query = Builders<ChargingOperItem>.Filter.And(
                Builders<ChargingOperItem>.Filter.And(
                    Builders<ChargingOperItem>.Filter.Eq("Operation", "borrow"),
                    Builders<ChargingOperItem>.Filter.Eq("Action", "borrow")),
                Builders<ChargingOperItem>.Filter.Eq("PatronBarcode", return_item.PatronBarcode),
                Builders<ChargingOperItem>.Filter.Eq("ItemBarcode", return_item.ItemBarcode),
                Builders<ChargingOperItem>.Filter.Lte("OperTime", return_item.OperTime));
            // 获得最近的一个 borrow item
            return collection.Find(query).SortByDescending(_ => _.OperTime).FirstOrDefault();
        }

#if OLD
        // 查找和本还书 item 关联的的借书操作 item
        public ChargingOperItem FindRelativeBorrowItem(ChargingOperItem return_item)
        {
            MongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return null;

            var query = Query.And(
                Query.And(Query.EQ("Operation", "borrow"), Query.EQ("Action", "borrow")),
                Query.EQ("PatronBarcode", return_item.PatronBarcode),
                Query.EQ("ItemBarcode", return_item.ItemBarcode),
                Query.LTE("OperTime", return_item.OperTime));
            // 获得最近的一个 borrow item
            MongoCursor<ChargingOperItem> cursor = collection.Find(query).SetSortOrder(SortBy.Descending("OperTime")).SetLimit(1);
            foreach (ChargingOperItem item in cursor.Take(1))
            {
                return item;
            }
            return null;
        }
#endif

        // parameters:
        //      patronBarcode   读者证条码号。
        //                      如果 以 "@itemBarcode:" 前缀引导，表示这是册条码号
        //                      如果 以 "@itemRefID:" 前缀引导，表示这是册参考 ID
        //                      如果 以 "@readerRefID:" 或 "@refID:" 前缀引导，表示这是读者参考 ID
        //      order   排序方式。ascending/descending 之一。默认 ascending
        //              注: descending 也可以简写为 desc
        //      start   要跳过这么多个记录
        //      totalCount  [out] 返回命中的记录总数
        public IEnumerable<ChargingOperItem> Find(
            string patronBarcode,
            DateTime startTime,
            DateTime endTime,
            string operTypes,
            string order,
            int start,
            out long totalCount)
        {
            totalCount = 0;
            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return null;

            var query = BuildQuery(patronBarcode,
                startTime,
                endTime,
                operTypes);
            /*
            IMongoSortBy sortBy = SortBy.Ascending("OperTime");
            if (order == "descending")
                sortBy = SortBy.Descending("OperTime");

            MongoCursor<ChargingOperItem> cursor = collection.Find(query).SetSortOrder(sortBy);
            IEnumerable<ChargingOperItem> results = cursor.Skip(start);
            totalCount = cursor.Count();
            */


            var results0 = collection.Find(query);
            if (order == "descending" || order == "desc")
                results0 = results0.SortByDescending(o => o.OperTime);
            else
                results0 = results0.SortBy(o => o.OperTime);
            totalCount = results0.CountDocuments();
            var results = results0.Skip(start)
                .ToEnumerable();
            return results;
        }

#if OLD
        public int GetItemCount(IMongoQuery query)
        {
            MongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return -1;

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
#endif

        // 探测是否存在这样的事项
        // 当 itemBarcode 不为空的时候，不使用 volume 参数的值
        // parameters:
        public IEnumerable<ChargingOperItem> Exists(
            string patronBarcode,
            string itemBarcode,
            string biblioRecPath,
            string volume,
            DateTime startTime,
            DateTime endTime,
            string operTypes)
        {
            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return null;

            var query = BuildQuery(patronBarcode,
                itemBarcode,
                biblioRecPath,
                volume,
                startTime,
                endTime,
                operTypes);

            return collection.Find(query).ToEnumerable();
            // return collection.Find(query).Count() > 0;
        }

        public void ChangePatronBarcode(string strOldBarcode, string strNewBarcode)
        {
            if (strOldBarcode == strNewBarcode)
                return; // 没有必要修改
            // 2024/1/19
            if (string.IsNullOrEmpty(strOldBarcode)
                || string.IsNullOrEmpty(strNewBarcode))
                return; // 没有必要修改

            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return;

            /*
            var query = new QueryDocument("PatronBarcode", strOldBarcode);
            var update = Update.Set("PatronBarcode", strNewBarcode);
            collection.Update(
    query,
    update,
    UpdateFlags.Multi);
    */

            var updateDef = Builders<ChargingOperItem>.Update.Set(o => o.PatronBarcode, strNewBarcode);

            collection.UpdateMany(
                o => o.PatronBarcode == strOldBarcode,
                updateDef);
        }

        // 2024/1/19
        public void DeletePatronBarcode(string strOldBarcode)
        {
            if (string.IsNullOrEmpty(strOldBarcode))
                return; // 没有必要修改
            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return;

            collection.DeleteMany(
                o => o.PatronBarcode == strOldBarcode);
        }

        // 2024/2/5
        public void ChangeItemBarcode(string strOldBarcode, string strNewBarcode)
        {
            if (strOldBarcode == strNewBarcode)
                return; // 没有必要修改
            if (string.IsNullOrEmpty(strOldBarcode)
                || string.IsNullOrEmpty(strNewBarcode))
                return; // 没有必要修改

            IMongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return;

            var updateDef = Builders<ChargingOperItem>.Update.Set(o => o.ItemBarcode, strNewBarcode);

            collection.UpdateMany(
                o => o.ItemBarcode == strOldBarcode,
                updateDef);
        }

    }

    public class ChargingOperItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; }

        public string LibraryCode { get; set; } // 访问者的图书馆代码
        public string Operation { get; set; } // 操作名
        public string Action { get; set; }  // 动作

        public string ItemBarcode { get; set; } // 2024/2/5 内容改为 @refID:xxx 形态
        public string PatronBarcode { get; set; } // 2024/2/5 内容改为 @refID:xxx 形态

        public string BiblioRecPath { get; set; }

        public string Period { get; set; }  // 期限
        public string No { get; set; }  // 续借次，序号

        // 2017/5/22
        public string Volume { get; set; }  // 卷册

        public string ClientAddress { get; set; }  // 访问者的IP地址

        public string Operator { get; set; }  // 操作者(访问者)
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OperTime { get; set; } // 操作时间

        // 2023/6/20
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BorrowDate { get; set; }    // 借书时间。仅用于还书动作。作为对“关联借书记录”找不到时的补充
    }

}
