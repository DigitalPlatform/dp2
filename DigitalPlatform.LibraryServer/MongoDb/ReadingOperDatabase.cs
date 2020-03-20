using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DigitalPlatform.LibraryServer
{
#if REMOVED
    /// <summary>
    /// 存储“读过”动作信息的数据库
    /// </summary>
    public class ReadingOperDatabase : MongoDatabase<ReadingOperItem>
    {
        public ReadingOperDatabase()
        {
            _databaseName = "readingOper";
        }
    }

    public class ReadingOperItem
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
#endif
}
