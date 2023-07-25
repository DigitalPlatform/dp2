using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;

namespace dp2Circulation
{
    public class ByjOutputDocxCatalog : OutputDocxCatalog
    {
        // 册条码号数量溢出的阈值 (> 它表示溢出)
        const int _overflowThreshold = 10;

        public override void OnBegin(object sender, StatisEventArgs e)
        {
            base.OnBegin(sender, e);

            this.BiblioFormat += ",xml";
        }

        public const string HYPHEN = "┉";   // ┉〜〰㇐

        // 切分 barcodes 为基本和溢出两部分。切割要避免把范围(三行)切断
        public static List<string> SplitOverflow(ref List<string> barcodes, int index)
        {
            if (barcodes.Count <= index)
                return new List<string>();
            List<string> results = new List<string>();
            int start = 0;
            if (index - 1 >= 0
                && barcodes[index] == HYPHEN)
                start = index - 1;  // index + 2;
            else if (barcodes.Count > index - 1
                && index - 2 >= 0
                && barcodes[index - 1] == HYPHEN)
                start = index - 2;
            else
                start = index;

            for (int i = start; i < barcodes.Count; i++)
            {
                results.Add(barcodes[i]);
            }
            barcodes.RemoveRange(start, barcodes.Count - start);
            return results;
        }

        public override void OutputStyleElements()
        {
            base.OutputStyleElements();

            // 定义一个 strike 样式，以节省 Word 文档中的样式总数
            Writer.WriteStartElement("style");
            Writer.WriteAttributeString("name", "strike");
            Writer.WriteAttributeString("type", "character");
            Writer.WriteAttributeString("style", "strike");
            Writer.WriteEndElement();  // style
        }

        // bool first = true;

        public override void OutputRecord(string accessNo,
    List<string> barcodes,
    string book_string)
        {
            // int barcodes_count = barcodes.Count;

#if !DISPLAY_DELETE_BARCODE
            // 2022/10/14
            // 过滤掉不该显示的册条码号
            barcodes = barcodes.Where(o => IsDeleteBarcode(o) == false).ToList();
#endif

            barcodes = CompactNumbersEx(barcodes, HYPHEN);

            var barcodes_has_overflow = barcodes.Count > _overflowThreshold;
            List<string> overflow = new List<string>();
            if (barcodes_has_overflow)
            {
                // 分开为两个集合
                overflow = SplitOverflow(ref barcodes, _overflowThreshold);
                // 尾部增加注释
                barcodes.Add("(接右栏)");
            }

            Writer.WriteStartElement("tr");
            if (barcodes_has_overflow == false)
                Writer.WriteAttributeString("cantSplit", "1");
            // 序号
            {
                Writer.WriteStartElement("td");
                if (string.IsNullOrEmpty(_firstColumnWidth))
                {
                    // Writer.WriteAttributeString("style", "noWrap");
                    Writer.WriteAttributeString("noWrap", "true");
                }


                /*
                if (first)
                Writer.WriteAttributeString("gridWidth", string.IsNullOrEmpty(_firstColumnWidth) ? "auto" : _firstColumnWidth);    // "20"
                */

                //if (first)
                Writer.WriteAttributeString("width", string.IsNullOrEmpty(_firstColumnWidth) ? "auto" : _firstColumnWidth);    // "20"
                {
                    Writer.WriteStartElement("p");
                    Writer.WriteAttributeString("style", "index");
                    Writer.WriteString($"{Index++}");
                    Writer.WriteEndElement();
                }
                Writer.WriteEndElement();
            }

            // 册条码号
            {
                Writer.WriteStartElement("td");
                if (string.IsNullOrEmpty(_secondColumnWidth))
                {
                    // Writer.WriteAttributeString("style", "noWrap");
                    Writer.WriteAttributeString("noWrap", "true");
                }
                /*
                if (first)
                Writer.WriteAttributeString("gridWidth", string.IsNullOrEmpty(_secondColumnWidth) ? "auto" : _secondColumnWidth);    // "50"
                */

                //if (first)
                Writer.WriteAttributeString("width", string.IsNullOrEmpty(_secondColumnWidth) ? "auto" : _secondColumnWidth);    // "50"

                // 条码号
                if (barcodes.Count > 0)
                {
                    Writer.WriteStartElement("p");
                    Writer.WriteAttributeString("style", "barcode");

#if OLD
                    {
                        /*
                         * 0000001
                         * <br/>
                         * 0000002
                         * */
                        int i = 0;
                        foreach (string line in barcodes)
                        {
                            if (i > 0)
                                Writer.WriteElementString("br", "");
                            Writer.WriteString(line);
                            i++;
                        }
                    }
#endif
                    WriteBarcodes(barcodes);

                    // Writer.WriteString(StringUtil.MakePathList(barcodes, " "));
                    Writer.WriteEndElement();
                }

                Writer.WriteEndElement();
            }

            // 正文、索取号
            {
                Writer.WriteStartElement("td");

                /*
                if (first)
                Writer.WriteAttributeString("gridWidth", string.IsNullOrEmpty(_thirdColumnWidth) ? "auto" : _thirdColumnWidth);
                */

                //if (first)
                Writer.WriteAttributeString("width", string.IsNullOrEmpty(_thirdColumnWidth) ? "auto" : _thirdColumnWidth);

                /*
                // 书目 ISBD
                {
                    Writer.WriteStartElement("p");
                    Writer.WriteAttributeString("style", "biblio");
                    Writer.WriteAttributeString("alignment", "both");

                    Writer.WriteAttributeString("spacing", "after:3pt");

                    Writer.WriteString(book_string);
                    Writer.WriteEndElement();
                }
                */
                OutputContentParagraph(book_string);

                // 索取号
                if (string.IsNullOrEmpty(accessNo) == false)
                {
                    Writer.WriteStartElement("p");
                    Writer.WriteAttributeString("style", "accessNo");
                    Writer.WriteAttributeString("spacing", $"before:3pt");

#if REMOVED
                    {
                        /*
                         * line1
                         * <br/>
                         * line2
                         * */
                        var lines = StringUtil.SplitList(accessNo, '/');
                        int i = 0;
                        foreach (string line in lines)
                        {
                            if (i > 0)
                                Writer.WriteElementString("br", "");
                            Writer.WriteString(line);
                            i++;
                        }
                    }
#endif
                    Writer.WriteString(accessNo.Replace("/", " / "));
                    Writer.WriteEndElement();
                }

                // 溢出的条码号
                if (overflow.Count > 0)
                {
                    Writer.WriteStartElement("p");
                    Writer.WriteAttributeString("style", "barcode");
                    Writer.WriteAttributeString("spacing", $"before:3pt");
#if REMOVED
                    {
                        /*
                         * 0000001
                         * <br/>
                         * 0000002
                         * */
                        int i = 0;
                        foreach (string line in overflow)
                        {
                            if (i > 0)
                                Writer.WriteElementString("br", "");
                            Writer.WriteString(line);
                            i++;
                        }
                    }
#endif
                    // Writer.WriteString(StringUtil.MakePathList(overflow, " "));

                    WriteOverflowBarcodes(overflow);

                    Writer.WriteEndElement();
                }

                Writer.WriteEndElement();
            }
            Writer.WriteEndElement();  // </tr>

            /*
            first = false;
            */
        }

        static bool IsDeleteBarcode(string barcode)
        {
            // “GW、JH、XK、XB” 开头的不显示
            if (barcode.StartsWith("GW")
                || barcode.StartsWith("JH")
                || barcode.StartsWith("XK")
                || barcode.StartsWith("XB")
                || (barcode.StartsWith("W") && barcode.StartsWith("WS") == false && barcode.StartsWith("WK") == false && barcode.StartsWith("WB") == false)) // W 开头的删除，除了其中 WS WK WB 开头的以外
                return true;
            return false;
        }

        void WriteBarcodes(List<string> barcodes)
        {
            int i = 0;
            foreach (var barcode in barcodes)
            {
                if (IsDeleteBarcode(barcode))
                {
                    if (i > 0)
                        Writer.WriteElementString("br", "");

                    Writer.WriteStartElement("style");
                    Writer.WriteAttributeString("use", "strike");
                    Writer.WriteString(barcode);
                    Writer.WriteEndElement();
                }
                else
                {
                    if (i > 0)
                        Writer.WriteElementString("br", "");

                    Writer.WriteString(barcode);
                }
                i++;
            }
        }

        void WriteOverflowBarcodes(List<string> barcodes)
        {
            int i = 0;
            string prev = "";
            foreach (var barcode in barcodes)
            {
                if (IsDeleteBarcode(barcode))
                {
                    if (i > 0)
                        Writer.WriteElementString("blk", "");
                    Writer.WriteStartElement("style");
                    Writer.WriteAttributeString("use", "strike");
                    Writer.WriteString(barcode);
                    Writer.WriteEndElement();
                }
                else
                {
                    if (i > 0 && prev != HYPHEN && barcode != HYPHEN)
                        Writer.WriteString(" ");

                    Writer.WriteString(barcode);
                }
                i++;
                prev = barcode;
            }
        }


        public override LoadItemsResult LoadItems(string biblio_recpath)
        {
            var result = base.LoadItems(biblio_recpath);

            // 看书目记录中是否有 686 字段？有了字段，并且字段内容必须是 X 开头，才当作唯一的索取号返回
            string biblio_xml = this.Contents[1];
            int nRet = MarcUtil.Xml2Marc(biblio_xml,
    true,
    "",
    out string marc_syntax,
    out string marc,
    out string error);
            if (nRet == -1)
            {
                throw new Exception($"书目记录 XML 转换为 MARC 格式时出现异常: {error}");
            }

            if (marc_syntax == "unimarc")
            {
                MarcRecord record = new MarcRecord(marc);
                var content = record.select("field[@name='686']/subfield[@name='a']").FirstContent;
                if (string.IsNullOrEmpty(content) == false
                    && content.StartsWith("X"))
                {
                    result.AccessNoList.AddRange(new List<string> { content });
                    return result;
                }
            }

            return result;
        }

        // 过滤册记录
        // return:
        //      true    要跳过输出
        //      false   不跳过
        public override bool FilterItem(XmlDocument itemdom)
        {
            string barcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
            // W 开头的删除，除了其中 WS WK WB 开头的以外
            if (string.IsNullOrEmpty(barcode)
                /*|| barcode.StartsWith("X")
                || barcode.StartsWith("J")
                || barcode.StartsWith("W")*/)
                return true;
            return false;
        }

        /*
        // “题名与责任者项”不加粗的版本
        public override string BuildBookString(XmlDocument table_dom, List<string> field_list)
        {
            if (table_dom == null || table_dom.DocumentElement == null)
                return null;

            StringBuilder text = new StringBuilder();

            var line_nodes = table_dom.DocumentElement.SelectNodes("line");
            foreach (XmlElement line in line_nodes)
            {
                // line @type @value @name
                string type = line.GetAttribute("type");
                string name = line.GetAttribute("name");
                string value = line.GetAttribute("value");

                var types = StringUtil.SplitList(type);
                // var type_name = types.Find((s) => s.EndsWith("_area"));
                var type_name = types.Find((s) => field_list.IndexOf(s) != -1);
                if (type_name == null)
                    continue;

                if (text.Length > 0)
                    text.Append(". -- ");
                text.Append(value);
            }

            return text.ToString();
        }
        */

        public static List<string> CompactNumbersEx(List<string> source,
            string hypen = "-")
        {
            List<string> results = new List<string>();
            int start = 0;
            while (start < source.Count)
            {
                var break_pos = FindBreak(start);
                Debug.Assert(break_pos > start);
                if (break_pos <= start + 3) // 三个及以内
                {
                    for (int i = start; i < break_pos; i++)
                    {
                        results.Add(source[i]);
                    }
                }
                else
                {
                    results.Add(source[start]);
                    results.Add(hypen);
                    results.Add(source[break_pos - 1]);
                }
                start = break_pos;

            }
            return results;

            // 从指定位置开始，探测连续号码范围的断点位置。断点位置就是下一个不连续的开头位置
            int FindBreak(int start_param)
            {
                string prev = source[start_param];
                for (int i = start_param + 1; i < source.Count; i++)
                {
                    string current = source[i];
                    // 给一个被字符引导的数字增加一个数量。
                    // 例如 B019X + 1 变成 B020X
                    int nRet = StringUtil.IncreaseNumber(prev,
            1,
            out string larger,
            out string strError);
                    if (nRet == -1)
                        return i;   // TODO: 抛出异常
                    if (current != larger)
                        return i;
                    prev = current;
                }

                return source.Count;
            }
        }
    }

    [TestClass]
    public class TestOutputDocxCatalog
    {
        [TestMethod]
        public void Test_compactNumber_01()
        {
            List<string> source = new List<string>();
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Test_compactNumber_02()
        {
            List<string> source = new List<string>() { "0000001" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("0000001", result[0]);
        }

        [TestMethod]
        public void Test_compactNumber_03()
        {
            // 只有两个号码的连续范围，合并后依然表达为两个独立号码
            List<string> source = new List<string>() { "0000001",
                "0000002" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("0000001", result[0]);
            Assert.AreEqual("0000002", result[1]);
        }

        [TestMethod]
        public void Test_compactNumber_04()
        {
            List<string> source = new List<string>() {
                "0000001",
                "0000002",
                "0000003",
                "0000004",
                "0000005",
                "0000006",
                "0000007",
                "0000008",
                "0000009",
                "0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("0000001", result[0]);
            Assert.AreEqual("-", result[1]);
            Assert.AreEqual("0000010", result[2]);

        }

        [TestMethod]
        public void Test_compactNumber_05()
        {
            List<string> source = new List<string>() {
                "0000001",
                "0000002",
                "0000009",
                "0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("0000001", result[0]);
            Assert.AreEqual("0000002", result[1]);
            Assert.AreEqual("0000009", result[2]);
            Assert.AreEqual("0000010", result[3]);
        }

        [TestMethod]
        public void Test_compactNumber_06()
        {
            List<string> source = new List<string>() {
                "0000001",
                "0000009",
                "0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("0000001", result[0]);
            Assert.AreEqual("0000009", result[1]);
            Assert.AreEqual("0000010", result[2]);
        }

        [TestMethod]
        public void Test_compactNumber_07()
        {
            List<string> source = new List<string>() {
                "0000001",
                "0000002",
                "0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("0000001", result[0]);
            Assert.AreEqual("0000002", result[1]);
            Assert.AreEqual("0000010", result[2]);
        }

        [TestMethod]
        public void Test_compactNumber_08()
        {
            List<string> source = new List<string>() {
                "0000001",
                "0000003",
                "0000005" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("0000001", result[0]);
            Assert.AreEqual("0000003", result[1]);
            Assert.AreEqual("0000005", result[2]);
        }

        // 

        [TestMethod]
        public void Test_compactNumber_12()
        {
            List<string> source = new List<string>() { "B0000001" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("B0000001", result[0]);
        }

        [TestMethod]
        public void Test_compactNumber_13()
        {
            List<string> source = new List<string>() { "B0000001",
                "B0000002" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("B0000002", result[1]);
        }

        [TestMethod]
        public void Test_compactNumber_14()
        {
            List<string> source = new List<string>() {
                "B0000001",
                "B0000002",
                "B0000003",
                "B0000004",
                "B0000005",
                "B0000006",
                "B0000007",
                "B0000008",
                "B0000009",
                "B0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("-", result[1]);
            Assert.AreEqual("B0000010", result[2]);
        }

        [TestMethod]
        public void Test_compactNumber_15()
        {
            List<string> source = new List<string>() {
                "B0000001",
                "B0000002",
                "B0000009",
                "B0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("B0000002", result[1]);
            Assert.AreEqual("B0000009", result[2]);
            Assert.AreEqual("B0000010", result[3]);
        }

        [TestMethod]
        public void Test_compactNumber_16()
        {
            List<string> source = new List<string>() {
                "B0000001",
                "B0000009",
                "B0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("B0000009", result[1]);
            Assert.AreEqual("B0000010", result[2]);
        }

        [TestMethod]
        public void Test_compactNumber_17()
        {
            List<string> source = new List<string>() {
                "B0000001",
                "B0000002",
                "B0000010" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("B0000002", result[1]);
            Assert.AreEqual("B0000010", result[2]);
        }

        [TestMethod]
        public void Test_compactNumber_20()
        {
            List<string> source = new List<string>() {
                "B0000001",
                "B0000002",
                "B0000003" };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("B0000002", result[1]);
            Assert.AreEqual("B0000003", result[2]);
        }

        [TestMethod]
        public void Test_compactNumber_21()
        {
            List<string> source = new List<string>() {
                "B0000001",
                "B0000002",
                "B0000003",
                "B0000004"
            };
            var result = ByjOutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("-", result[1]);
            Assert.AreEqual("B0000004", result[2]);
        }

        [TestMethod]
        public void Test_splitOverfow_01()
        {
            List<string> barcodes = new List<string>();
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes, 0);
            Assert.AreEqual(0, overflows.Count);
            Assert.AreEqual(0, barcodes.Count);
        }

        [TestMethod]
        public void Test_splitOverfow_02()
        {
            List<string> barcodes = new List<string>();
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes, 1);
            Assert.AreEqual(0, overflows.Count);
            Assert.AreEqual(0, barcodes.Count);
        }

        [TestMethod]
        public void Test_splitOverfow_03()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
            };
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes, 0);
            Assert.AreEqual(1, overflows.Count);
            Assert.AreEqual(0, barcodes.Count);
            CompareStringList(
                new List<string>() {
                "B0000001",
                },
                overflows);
        }

        [TestMethod]
        public void Test_splitOverfow_04()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
            };
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes, 1);
            Assert.AreEqual(0, overflows.Count);
            Assert.AreEqual(1, barcodes.Count);
            CompareStringList(
                new List<string>() {
                "B0000001",
                },
                barcodes);
        }

        void CompareStringList(List<string> list1, List<string> list2)
        {
            Assert.AreEqual(list1.Count, list2.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list2[i]);
            }
        }

        [TestMethod]
        public void Test_splitOverfow_10()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004"
            };
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes, 1);

            Assert.AreEqual(4, overflows.Count);
            CompareStringList(
                new List<string>() {
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004"
                },
                overflows);
            Assert.AreEqual(1, barcodes.Count);
            CompareStringList(
    new List<string>() {
                "B0000001",
    },
    barcodes);
        }

        [TestMethod]
        public void Test_splitOverfow_11()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,        // <-
                "B0000003",
                "B0000004"
            };
            // 注意，往前调整切割点
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes,
                2);
            CompareStringList(
    new List<string>() {
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004",
    },
    overflows);
            CompareStringList(
                new List<string>() {
                "B0000001",
                },
                barcodes);
        }

        [TestMethod]
        public void Test_splitOverfow_12()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003", // <--
                "B0000004"
            };
            // 注意，切割点从 B0000003 调整到 B0000002 位置
            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes,
                3);
            CompareStringList(
                new List<string>() {
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004"
                },
                overflows);
            CompareStringList(
                new List<string>() {
                "B0000001",
                },
                barcodes);
        }

        [TestMethod]
        public void Test_splitOverfow_13()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004", // <--
            };

            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes,
                4);
            CompareStringList(
                new List<string>() {
                "B0000004"
                },
                overflows);
            CompareStringList(
                new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                },
                barcodes);

        }

        [TestMethod]
        public void Test_splitOverfow_14()
        {
            List<string> barcodes = new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004",
                            // <--
            };

            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes,
                5);
            Assert.AreEqual(0, overflows.Count);
            CompareStringList(
                new List<string>() {
                "B0000001",
                "B0000002",
                ByjOutputDocxCatalog.HYPHEN,
                "B0000003",
                "B0000004"
                },
                barcodes);
        }


        [TestMethod]
        public void Test_splitOverfow_15()
        {
            // 病态情况
            List<string> barcodes = new List<string>() {
                ByjOutputDocxCatalog.HYPHEN,      // <--
                "B0000001",
                "B0000002",
                "B0000003",
                "B0000004",

            };

            var overflows = ByjOutputDocxCatalog.SplitOverflow(ref barcodes,
                0);
            CompareStringList(
                new List<string>() {
                ByjOutputDocxCatalog.HYPHEN,
                "B0000001",
                "B0000002",
                "B0000003",
                "B0000004"
                },
                overflows);
            Assert.AreEqual(0, barcodes.Count);
        }

    }

}
