using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    // 一个对照关系
    public class Relation
    {
        public string DbName = "";  // 词典库
        public string SourceDef = "";   // 源定义。格式为 字段名+子字段名 一共四个字符
        public string TargetDef = "";   // 目标定义。格式为 字段名+子字段名 一共四个字符
        public List<string> Keys = null;
        public string Color = "";

        public Relation(string strDbName,
            string strSourceDef,
            string strTargetDef,
            List<string> keys,
            string strColor)
        {
            this.DbName = strDbName;
            this.SourceDef = strSourceDef;
            this.TargetDef = strTargetDef;
            this.Keys = keys;
            this.Color = strColor;
        }

    }

    // 关系集合
    public class RelationCollection
    {
        List<Relation> _collection = new List<Relation>();

        public string MARC
        {
            get;
            set;
        }

        public IEnumerator GetEnumerator()
        {
            return this._collection.GetEnumerator();
        }

        // 构建集合对象
        // parameters:
        //      strDef  定义字符串。分为若干行，每行定义一个对照关系。行之间用分号间隔。
        //              行格式为 dbname=数据库名,source=源字段名子字段名,target=目标字段名子字段名,color=#000000
        public int Build(string strMARC,
            string strDef,
            out string strError)
        {
            strError = "";

            this.MARC = strMARC;

            MarcRecord record = new MarcRecord(strMARC);

            string[] lines = strDef.Split(new char[] { ';' });
            foreach (string line in lines)
            {
                Hashtable table = StringUtil.ParseParameters(line, ',', '=');
                string strDbName = (string)table["dbname"];
                string strSource = (string)table["source"];
                string strTarget = (string)table["target"];
                string strColor = (string)table["color"];

                if (string.IsNullOrEmpty(strSource) == true
                    || strSource.Length != 4)
                {
                    strError = "行 '" + line + "' 中 source 参数值 '" + strSource + "' 格式错误，应为 4 字符";
                    return -1;
                }

                if (string.IsNullOrEmpty(strTarget) == true
    || strTarget.Length != 4)
                {
                    strError = "行 '" + line + "' 中 target 参数值 '" + strTarget + "' 格式错误，应为 4 字符";
                    return -1;
                }

                string fieldname = strSource.Substring(0, 3);
                string subfieldname = strSource.Substring(3, 1);
                MarcNodeList subfields = record.select("field[@name='" + fieldname + "']/subfield[@name='" + subfieldname + "']");
                if (subfields.count == 0)
                    continue;

                List<string> keys = new List<string>();
                foreach (MarcSubfield subfield in subfields)
                {
                    if (string.IsNullOrEmpty(subfield.Content) == true)
                        continue;
                    keys.Add(subfield.Content);
                }

                if (keys.Count == 0)
                    continue;

                Relation relation = new Relation(strDbName, 
                    strSource, 
                    strTarget,
                    keys,
                    strColor);
                this._collection.Add(relation);
            }

            return 0;
        }
    }

}
