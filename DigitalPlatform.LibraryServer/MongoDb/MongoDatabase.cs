using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MongoDB.Driver;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// Mongo 数据库的基础类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoDatabase<T>
    {
        internal string _databaseName = "";
        internal MongoCollection<T> _collection = null;
        internal string _collectionName = "collection";

        public string CollectionName
        {
            get
            {
                return _collectionName;
            }
            set
            {
                _collectionName = value;
            }
        }

        // 初始化
        // 默认的初始化函数，只初始化一个 collection
        // parameters:
        public virtual int Open(MongoClient client,
            string strInstancePrefix,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strInstancePrefix) == false)
                strInstancePrefix = strInstancePrefix + "_";

            _databaseName = strInstancePrefix + _collectionName;

            var server = client.GetServer();

            {
                var db = server.GetDatabase(_databaseName);

                this._collection = db.GetCollection<T>(_collectionName);
                if (this._collection.GetIndexes().Count == 0)
                    CreateIndex();
            }

            return 0;
        }

        public virtual void CreateIndex()
        {

        }

        // 清除集合内的全部内容
        public virtual int Clear(out string strError)
        {
            strError = "";

            if (_collection == null)
            {
                strError = "_collection 尚未初始化";
                return -1;
            }

            WriteConcernResult result = _collection.RemoveAll();
            CreateIndex();
            return 0;
        }

    }

}
