using DigitalPlatform.Marc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public static class test
    {
        // 运行全部测试项目
        public static void TestAll(LibraryContext context)
        {
            TestAddOrUpdateBiblio(ref context);
            TestQueryBiblios(context);
            TestLeftJoin(context);
            TestDeleteBiblioKeys(context);
            TestLeftJoinMissingKeys(context);
        }

        #region

        public static void TestAddOrUpdateBiblio(ref LibraryContext context)
        {
            string recpath = "test/1";
            // 删除可能存在的记录
            var record = context.Biblios
                .Where(x => x.RecPath == recpath).FirstOrDefault();
            if (record != null)
            {
                context.Biblios.Remove(record);
                context.SaveChanges();
            }

            // 创建一条新的记录
            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle1$fauthor1"));
                marc.add(new MarcField('$', "690  $aclass_string1"));
                Biblio biblio = new Biblio { RecPath = recpath };
                if (MarcUtil.Marc2Xml(marc.Text,
        "unimarc",
        out string xml,
        out string error) == -1)
                    throw new Exception(error);
                biblio.RecPath = recpath;
                biblio.Xml = xml;
                biblio.CreateKeys(biblio.Xml, biblio.RecPath);
                context.Add(biblio);
                context.SaveChanges();
            }

            context.Dispose();
            context = new LibraryContext();

            // 用于最后阶段比对检索点
            List<Key> save_keys = new List<Key>();

            // 更新上述书目记录
            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle2$fauthor2"));
                marc.add(new MarcField('$', "690  $aclass_string2"));
                if (MarcUtil.Marc2Xml(marc.Text,
        "unimarc",
        out string xml,
        out string error) == -1)
                    throw new Exception(error);

                var biblio = context.Biblios.SingleOrDefault(c => c.RecPath == recpath)
    ?? new Biblio { RecPath = recpath};

                Debug.Assert(biblio.RecPath == recpath);

                biblio.RecPath = recpath;
                biblio.Xml = xml;
                biblio.CreateKeys(biblio.Xml, biblio.RecPath);

                save_keys.AddRange(biblio.Keys);

                context.AddOrUpdate(biblio);
                context.SaveChanges();
            }

            context.Dispose();
            context = new LibraryContext();

            // 检查检索点是否正确
            var keys = context.Keys
                .Where(x => x.BiblioRecPath == recpath)
                .ToList();

            if (keys.Count != save_keys.Count)
                throw new Exception($"keys.Count ({keys.Count}) 和 save_keys.Count ({save_keys.Count})应该相等");

            foreach (var current in save_keys)
            {
                if (Key.IndexOf(keys, current) == -1)
                    throw new Exception($"key '{current.ToString()}' 在检索结果中没有找到");
            }
        }

        #endregion

        // 测试查询书目记录
        public static void TestQueryBiblios(LibraryContext context)
        {
            // 删除可能存在的记录
            {
                var records = context.Biblios
                    .Where(x => x.RecPath.StartsWith("test/")).ToList();
                if (records.Count > 0)
                {
                    context.Biblios.RemoveRange(records);
                    context.SaveChanges();
                }
            }

            // 创建两条书目记录
            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle1$fauthor"));
                marc.add(new MarcField('$', "690  $aclass_string1_1"));
                marc.add(new MarcField('$', "690  $aclass_string1_2"));

                CreateTestRecord(context,
        "test/1",
        marc.Text);
            }

            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle2$fauthor"));
                marc.add(new MarcField('$', "690  $aclass_string2_1"));
                marc.add(new MarcField('$', "690  $aclass_string2_2"));

                CreateTestRecord(context,
        "test/2",
        marc.Text);
            }

            context.SaveChanges();

            // 查询
            var biblios = context.Biblios
                .Where(x => x.RecPath == "test/1")
                .ToList();

            // 验证
        }

        // 测试删除书目记录的时候是否也正确删除了 keys
        public static void TestDeleteBiblioKeys(LibraryContext context)
        {
            // 删除可能存在的记录
            {
                var records = context.Biblios
                    .Where(x => x.RecPath.StartsWith("test/")).ToList();
                if (records.Count > 0)
                {
                    context.Biblios.RemoveRange(records);
                    context.SaveChanges();
                }
            }

            // 创建两条书目记录
            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle1$fauthor"));
                marc.add(new MarcField('$', "690  $aclass_string1_1"));
                marc.add(new MarcField('$', "690  $aclass_string1_2"));

                CreateTestRecord(context,
        "test/1",
        marc.Text);
            }

            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle2$fauthor"));
                marc.add(new MarcField('$', "690  $aclass_string2_1"));
                marc.add(new MarcField('$', "690  $aclass_string2_2"));

                CreateTestRecord(context,
        "test/2",
        marc.Text);
            }

            context.SaveChanges();

            // 删除第一条书目记录
            {
                var records = context.Biblios
                    .Where(x => x.RecPath == "test/1").ToList();
                if (records.Count > 0)
                {
                    context.Biblios.RemoveRange(records);
                    context.SaveChanges();
                }
            }

            // 检查 keys 第二条是否还存在
            var keys = context.Keys
                .Where(x => x.BiblioRecPath == "test/2")
                .ToList();

            //if (keys.Count != 3)
            //    throw new Exception("left join 的结果元素应该只有 3 个");

            foreach (var key in keys)
            {
                if (key.BiblioRecPath != "test/2")
                    throw new Exception("剩下的应该都是 第 2 条书目记录的检索点才对");
            }
        }


        // 测试 biblio left join keys 在丢失对应的 keys 的情况下的正确性
        public static void TestLeftJoinMissingKeys(LibraryContext context)
        {
            string recpath = "test/1";

            // 删除可能存在的记录
            {
                var records = context.Biblios
                    .Where(x => x.RecPath.StartsWith("test/")).ToList();
                if (records.Count > 0)
                {
                    context.Biblios.RemoveRange(records);
                    context.SaveChanges();
                }
            }

            // 创建一条书目记录
            {
                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle1$fauthor"));
                marc.add(new MarcField('$', "690  $aclass_string1"));
                marc.add(new MarcField('$', "690  $aclass_string2"));

                CreateTestRecord(context,
        recpath,
        marc.Text);
            }

            context.SaveChanges();

            // 删除第一条书目记录的 keys。注意书目记录依然存在
            {
                context.RemoveRange(context.Keys
                    .Where(x => x.BiblioRecPath == recpath)
                    .ToList());
                context.SaveChanges();
            }

            // 进行 left join
            {
                var biblios = context.Biblios.Where(x => x.RecPath == recpath)
                        .LeftJoin(
                        context.Keys,
                        biblio => new { recpath = biblio.RecPath, type = "class_clc", index = 0 },
                        key => new { recpath = key.BiblioRecPath, type = key.Type, index = key.Index },
                        (biblio, key) => new
                        {
                            biblio.RecPath,
                            Class = key.Text,   // 这一句不会抛出异常
                        }
                    ).ToList();

                if (biblios.Count != 1)
                    throw new Exception("left join 得到的元素个数不正确");

                if (biblios[0].Class != null)
                    throw new Exception("得到的分类号字符串不正确");
            }
        }

#if NO
        public static void UpdateBiblio(
            LibraryContext context,
            Biblio biblio)
        {
            /*
            bool changed = false;
            var records = context.Biblios.Where(x => x.RecPath == biblio.RecPath).ToList();
            if (records.Count > 0)
            {
                context.RemoveRange(records);
                changed = true;
            }
            */

            context.AddOrUpdate(biblio);
            context.SaveChanges();
        }
#endif

        public static void CreateTestRecord(LibraryContext context,
            string recpath,
            string strMARC)
        {
            // 删除可能存在的记录
            var record = context.Biblios
                .Where(x => x.RecPath == recpath).FirstOrDefault();
            if (record != null)
            {
                context.Biblios.Remove(record);
                context.SaveChanges();
            }

            // 创建一条新的记录
            Biblio biblio = new Biblio { RecPath = recpath };

            MarcRecord marc = new MarcRecord(strMARC);
            /*
            marc.add(new MarcField('$', "200  $atitle$fauthor"));
            marc.add(new MarcField('$', "690  $aclass_string1"));
            marc.add(new MarcField('$', "690  $aclass_string2"));
            */

            if (MarcUtil.Marc2Xml(marc.Text,
                "unimarc",
                out string xml,
                out string error) == -1)
                throw new Exception(error);
            biblio.RecPath = recpath;
            biblio.Xml = xml;
            biblio.CreateKeys(biblio.Xml, biblio.RecPath);
            context.Biblios.Add(biblio);
        }


        public static void CreateTestRecords(LibraryContext context)
        {
            // context.Database.Migrate();

            string recpath = "test/1";
            // 删除可能存在的记录
            var record = context.Biblios
                .Where(x => x.RecPath == recpath).FirstOrDefault();
            if (record != null)
            {
                context.Biblios.Remove(record);
                context.SaveChanges();
            }

            // 创建一条新的记录
            Biblio biblio = new Biblio { RecPath = recpath };

            MarcRecord marc = new MarcRecord();
            marc.add(new MarcField('$', "200  $atitle$fauthor"));
            marc.add(new MarcField('$', "690  $aclass_string1"));
            marc.add(new MarcField('$', "690  $aclass_string2"));

            if (MarcUtil.Marc2Xml(marc.Text,
                "unimarc",
                out string xml,
                out string error) == -1)
                throw new Exception(error);
            biblio.RecPath = recpath;
            biblio.Xml = xml;
            biblio.CreateKeys(biblio.Xml, biblio.RecPath);
            context.Biblios.Add(biblio);
            context.SaveChanges();
        }

        public static void TestLeftJoin(LibraryContext context)
        {
            // *** 第一步，准备数据
            string recpath = "test/1";

            {
                // 删除可能存在的记录
                var record = context.Biblios
                    .Where(x => x.RecPath == recpath).FirstOrDefault();
                if (record != null)
                {
                    context.Biblios.Remove(record);
                    context.SaveChanges();
                }

                // 创建一条新的记录
                Biblio biblio = new Biblio { RecPath = recpath };

                MarcRecord marc = new MarcRecord();
                marc.add(new MarcField('$', "200  $atitle$fauthor"));
                marc.add(new MarcField('$', "690  $aclass_string1"));
                marc.add(new MarcField('$', "690  $aclass_string2"));

                if (MarcUtil.Marc2Xml(marc.Text,
                    "unimarc",
                    out string xml,
                    out string error) == -1)
                    throw new Exception(error);
                biblio.RecPath = recpath;
                biblio.Xml = xml;
                biblio.CreateKeys(biblio.Xml, biblio.RecPath);
                context.Biblios.Add(biblio);
                context.SaveChanges();

            }

            // *** 第二步，验证 left join
            {
                var biblios = context.Biblios.Where(x => x.RecPath == recpath)
                        .LeftJoin(
                        context.Keys,
                        biblio => new { recpath = biblio.RecPath, type = "class_clc", index = 0 },
                        key => new { recpath = key.BiblioRecPath, type = key.Type, index = key.Index },
                        (biblio, key) => new
                        {
                            biblio.RecPath,
                            Class = key.Text,
                        }
                    ).ToList();

                if (biblios.Count != 1)
                    throw new Exception("left join 得到的元素个数不正确");

                if (biblios[0].Class != "class_string1")
                    throw new Exception("得到的分类号字符串不正确");

                /*
                StringBuilder text = new StringBuilder();
                foreach (var biblio in biblios)
                {
                    text.AppendLine($"bibliorecpath={biblio.RecPath}, class_clc={biblio.Class}");
                }

                return text.ToString();
                */
            }
        }

        // TODO: 测试 left join 当 right 对象不存在时候的空指针可能性

        public static bool isInList(string sub, string list)
        {
            if (sub == null)
                throw new ArgumentException("sub 参数值不应为 null", "sub");
            if (list == null)
                throw new ArgumentException("list 参数值不应为 null", "list");

            if (list.StartsWith(sub + ","))
                return true;
            if (list.EndsWith("," + "sub"))
                return true;
            return list.IndexOf("," + sub + ",") != -1;
        }
    }
}
