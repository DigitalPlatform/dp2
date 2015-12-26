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
    /// 存储流通借还操作动作信息的数据库
    /// </summary>
    public class ChargingOperDatabase : MongoDatabase<ChargingOperItem>
    {

        public override void CreateIndex()
        {
            _collection.CreateIndex(new IndexKeysBuilder().Ascending("OperTime"),
    IndexOptions.SetUnique(false));

            _collection.CreateIndex(new IndexKeysBuilder().Ascending("ItemBarcode"),
IndexOptions.SetUnique(false));

            _collection.CreateIndex(new IndexKeysBuilder().Ascending("PatronBarcode"),
IndexOptions.SetUnique(false));

        }

        // parameters:
        public bool Add(ChargingOperItem item)
        {
            MongoCollection<ChargingOperItem> collection = this._collection;
            if (collection == null)
                return false;

            collection.Insert(item);
            return true;
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

        public string ItemBarcode { get; set; }
        public string PatronBarcode { get; set; }

        public string ClientAddress { get; set; }  // 访问者的IP地址

        public string Operator { get; set; }  // 操作者(访问者)
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime OperTime { get; set; } // 操作时间
    }

}
