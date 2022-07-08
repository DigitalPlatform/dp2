using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestGeneral
{
    [TestClass]
    public class TestScript
    {
        [TestMethod]
        public void TestMethod_parseAuthorString_1()
        {
            var infos = ParseAuthorString("(美)肯·费雪(Ken Fisher)");
            Assert.AreEqual(1, infos.Count);

            Assert.AreEqual("(美)", infos[0].Country);
            Assert.AreEqual("肯·费雪", infos[0].Author);
            Assert.AreEqual("(Ken Fisher)", infos[0].OriginName);
            Assert.AreEqual(null, infos[0].CreateType);
        }

        [TestMethod]
        public void TestMethod_parseAuthorString_2()
        {
            var infos = ParseAuthorString("(美)肯·费雪(Ken Fisher), (美)劳拉·霍夫曼斯(Lara Hoffmans)");
            Assert.AreEqual(2, infos.Count);

            {
                var info = infos[0];
                Assert.AreEqual("(美)", info.Country);
                Assert.AreEqual("肯·费雪", info.Author);
                Assert.AreEqual("(Ken Fisher)", info.OriginName);
                Assert.AreEqual(null, info.CreateType);
            }

            {
                var info = infos[1];
                Assert.AreEqual("(美)", info.Country);
                Assert.AreEqual("劳拉·霍夫曼斯", info.Author);
                Assert.AreEqual("(Lara Hoffmans)", info.OriginName);
                Assert.AreEqual(null, info.CreateType);
            }
        }

        [TestMethod]
        public void TestMethod_parseAuthorString_3()
        {
            var infos = ParseAuthorString("(美)珍妮弗·周(Jennifer Chou)著");
            Assert.AreEqual(1, infos.Count);

            {
                var info = infos[0];
                Assert.AreEqual("(美)", info.Country);
                Assert.AreEqual("珍妮弗·周", info.Author);
                Assert.AreEqual("(Jennifer Chou)", info.OriginName);
                Assert.AreEqual("著", info.CreateType);
            }
        }

        public class AuthorInfo
        {
            public string Country { get; set; }
            public string Author { get; set; }
            public string OriginName { get; set; }
            public string CreateType { get; set; }

            // 获得下一个部分
            public static string NextPart(ref string text)
            {
                if (string.IsNullOrEmpty(text))
                    return null;

                string result = "";
                if (text[0] == '(')
                {
                    int pos = text.IndexOf(')');
                    if (pos == -1)
                    {
                        result = text;
                        text = "";
                        return result;
                    }

                    result = text.Substring(0, pos + 1);
                    text = text.Substring(pos + 1);
                    return result;
                }

                {
                    int pos = text.IndexOf('(');
                    if (pos == -1)
                    {
                        result = text;
                        text = "";
                        return result;
                    }

                    result = text.Substring(0, pos);
                    text = text.Substring(pos);
                    return result;
                }
            }

        }


        /*
         * 
如将$f(美)肯·费雪(Ken Fisher), (美)劳拉·霍夫曼斯(Lara Hoffmans), (美)珍妮弗·周(Jennifer Chou)著填充为：
$c(美)$a费雪$g(Ken Fisher)$4著
$c(美)$a霍夫曼斯$g(Hoffmans, Lara)$4著
$c(美)$a周$g(Chou, Jennifer)$4著
         * */
        static List<AuthorInfo> ParseAuthorString(string text)
        {
            List<AuthorInfo> results = new List<AuthorInfo>();
            // 先用逗号切割为多个区段
            var segments = text.Split(new char[] { ',' });
            foreach (var segment in segments)
            {
                string line = segment.Trim();

                var info = new AuthorInfo();
                while (true)
                {
                    var result = AuthorInfo.NextPart(ref line);
                    if (result == null)
                        break;
                    if (result[0] == '(')
                    {
                        if (string.IsNullOrEmpty(info.Country))
                            info.Country = result;
                        else
                            info.OriginName = result;
                        continue;
                    }

                    {
                        if (string.IsNullOrEmpty(info.Author))
                            info.Author = result;
                        else
                            info.CreateType = result;
                    }
                }

                results.Add(info);
            }

            return results;
        }
    }

}
