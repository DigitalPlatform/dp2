using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

using DigitalPlatform.Marc;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class Biblio
    {
        public string RecPath { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        [MaxLength(4096)]
        public string Summary { get; set; }

        // 书目记录 XML 内容
        [MaxLength(4096)]
        public string Xml { get; set; }

        // 书目记录的检索点
        public virtual List<Key> Keys { get; set; }

        public void CreateSummary(string strXml, string recpath)
        {
            int nRet = MarcUtil.Xml2Marc(strXml,
false,
null,
out string strOutMarcSyntax,
out string strMARC,
out string strError);
            if (nRet == -1)
            {
                this.Summary = "error:" + strError;
                return;
            }

            this.Summary = CreateSummary(
                recpath,
                strMARC,
                strOutMarcSyntax);
        }

        // 创建书目摘要
        public static string CreateSummary(
            string strRecPath,
            string strMARC,
            string syntax)
        {
            string strError = "";
            int nRet = 0;
            List<NameValueLine> results = null;

            if (syntax == "usmarc")
                nRet = MarcTable.ScriptMarc21(
strRecPath,
strMARC,
"areas",
null,
out results,
out strError);
            else
                nRet = MarcTable.ScriptUnimarc(
    strRecPath,
    strMARC,
    "areas",
    null,
    out results,
    out strError);
            if (nRet == -1)
                return "error:" + strError;

            StringBuilder text = new StringBuilder();
            foreach (var line in results)
            {
                if (text.Length > 0)
                    text.Append(". -- ");
                text.Append(line.Value);
            }

            return text.ToString();
        }

        public void Create(string strXml,
            string strBiblioRecPath)
        {
            var result = CreateKeys(strXml, strBiblioRecPath);
            if (result.Value == -1)
            {
                this.Summary = "error:" + result.ErrorInfo;
                return;
            }

            this.Summary = CreateSummary(strBiblioRecPath, result.MARC, result.Syntax);
        }

        // 返回创建检索点过程中的中间结果
        public class CreateKeysResult : NormalResult
        {
            public string MARC { get; set; }
            public string Syntax { get; set; }
        }

        // 创建检索点对象
        public CreateKeysResult CreateKeys(string strXml,
            string strBiblioRecPath)
        {
            if (this.Keys == null)
                this.Keys = new List<Key>();

            int nRet = MarcUtil.Xml2Marc(strXml,
    false,
    null,
    out string strOutMarcSyntax,
    out string strMARC,
    out string strError);
            if (nRet == -1)
                return new CreateKeysResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            // TODO: 新创建的 keys 先进入一个临时区。然后排序，和当前已有的 keys 合并
            List<Key> keys = new List<Key>();

            MarcRecord record = new MarcRecord(strMARC);
            if (strOutMarcSyntax == "unimarc")
            {
                keys.AddRange(BuildKeys(record,
strBiblioRecPath,
"field[@name='200']/subfield[@name='a']",
"title"));

                keys.AddRange(BuildKeys(record,
strBiblioRecPath,
"field[@name='690']/subfield[@name='a']",
"class_clc"));
            }
            else
            {
                keys.AddRange(BuildKeys(record,
strBiblioRecPath,
"field[@name='245']/subfield[@name='a']",
"title"));
            }

            // 对所有检索点排序，赋予 Index
            SetIndex(keys);

            MergeKeys(keys, this.Keys);

            return new CreateKeysResult
            {
                MARC = strMARC,
                Syntax = strOutMarcSyntax
            };
        }

        // 把 source 里面的元素合并到 target 中。
        // 算法要点是，如果 target 里面的某个 Key 没有变化，就不要去先删除再插入
        public static void MergeKeys(List<Key> source, List<Key> target)
        {
            if (target.Count == 0)
            {
                target.AddRange(source);
                return;
            }

            List<Key> target_found = new List<Key>();

            // 需要新增的 keys
            List<Key> new_keys = new List<Key>();
            foreach (var source_key in source)
            {
                Key found = null;
                foreach (var target_key in target)
                {
                    // TODO: 如果只是 Index 不同，则直接修改 target 里面的 Index 即可?
                    if (Key.IsEqual(source_key, target_key))
                        found = target_key;
                }
                if (found != null)
                    target_found.Add(found);
                else
                    new_keys.Add(source_key);
            }

            // 需要删除的 keys
            List<Key> delete_keys = new List<Key>();
            foreach (var key in target)
            {
                var founds = target_found.Where(x => Equals(key, x)).ToList();
                if (founds.Count == 0)
                    delete_keys.Add(key);
            }

            foreach (var key in delete_keys)
            {
                target.Remove(key);
            }

            target.AddRange(new_keys);
        }





        public static void SetIndex(List<Key> keys)
        {
            keys.Sort((a, b) =>
            {
                int nRet = string.Compare(a.Type, b.Type);
                if (nRet != 0)
                    return nRet;
                // 继续比较 index
                return a.Index - b.Index;
            });

            // 重新分配 Index
            string prev_type = "";
            int prev_index = 0;
            foreach (var key in keys)
            {
                if (key.Type != prev_type)
                {
                    prev_type = key.Type;
                    prev_index = 0;
                }

                key.Index = prev_index;
                prev_index++;
            }
        }

        public static List<Key> BuildKeys(MarcRecord record,
            string strBiblioRecPath,
            string xpath,
            string type)
        {
            List<string> values = new List<string>();
            foreach (MarcSubfield subfield in record.select(xpath))
            {
                values.Add(subfield.Content);
            }

            // TODO: SQL Server 无法区分大小写 key 字符串，会认为重复
            values.Sort((a, b) => string.Compare(a, b, true));
            _removeDup(ref values);
            StringUtil.RemoveBlank(ref values);

            List<Key> results = new List<Key>();
            int index = 0;
            foreach (string class_string in values)
            {
                Key key = new Key
                {
                    Text = class_string,
                    Type = type,
                    BiblioRecPath = strBiblioRecPath,
                    Index = index++,
                };
                results.Add(key);
            }

            return results;
        }

        public static void _removeDup(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i].ToUpper();
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j].ToUpper())
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
            }
        }

        // 更新 biblios 表 和 keys 表中的行
        public static void UpdateBiblioRecord(
            LibraryContext context,
            string strBiblioRecPath,
            string strBiblioXml,
            bool saveChanges = false)
        {
            /*
            strError = "";
            string strDbName = GetDbName(strBiblioRecPath);
            if (IsBiblioDbName(strDbName) == false)
                return 0;
                */

            var biblio = context.Biblios.SingleOrDefault(c => c.RecPath == strBiblioRecPath)
?? new Biblio { RecPath = strBiblioRecPath };

            Debug.Assert(biblio.RecPath == strBiblioRecPath);

            biblio.RecPath = strBiblioRecPath;
            biblio.Xml = strBiblioXml;
            biblio.Create(biblio.Xml, biblio.RecPath);
            context.AddOrUpdate(biblio);
            if (saveChanges)
                context.SaveChanges();
        }
    }

}
