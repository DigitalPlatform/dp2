using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Data;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Typography;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dp2Circulation
{
    /*
1.dp2 系统输出书本式目录，按照选定的书目记录来输出。
2.同一书目信息的册显示在一条书目信息里。
3.方案是处理dp2系统中册记录，不处理书目记录中的905字段。
4.书本式目录输出信息包括“题名、责任者、出版者、出版地、出版时间、下挂册条码号、下挂册的索书号”。
5.三列多行的格式，第一列显示序号，第二列显示册条码号，第三列显示图书信息及索书号。
6.第二列需要显示同一书目信息下属所有册条码号。
7.当同一书目下属册超过3册且册条码号是连续的情况，把册条码合并显示。例如：一条书目信息下有4册实体，021001，021002，021003，021004，显示成：021001-021004，“021001”“-”“021004”各占一行，一共占3行。
8.当同一书目信息下属册条码号不连续的时候，直接显示所有册条码号。
9.同一书目信息下属册的索书号不同时，只显示第一个册的索书号，其他不显示。
10.索书号为单独一行，且与图书信息间隔略大。
11.可以自定义序号，即打印时按照自定义的序号开始连续排序。
12.书本式目录按照要求的格式输出到Word，在word 中排版成双栏效果。
13.第三方系统的书目信息，需要将ISO2709格式文件先导入dp2系统，从dp2系统再导出书本式样式的信息到word。
    * */
    public class OutputDocxCatalog : BiblioStatis
    {
        string _outputFileName;

        int _index = 0;

        XmlTextWriter _writer = null;

        public XmlTextWriter Writer
        {
            get
            {
                return _writer;
            }
        }

        public override void FreeResources()
        {
            if (_writer != null)
                _writer.Close();
        }

        public override void OnBegin(object sender, StatisEventArgs e)
        {
            string strError = "";

            if (InputSettings() == false)
            {
                e.Continue = ContinueType.Error;
                e.ParamString = "取消";
                return;
            }

            _index = _BiblioNoStart;

            _outputFileName = Path.Combine(Program.MainForm.UserTempDir, "~output_docx_catalog.xml");
            try
            {
                _writer = new XmlTextWriter(_outputFileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = $"创建文件 {_outputFileName} 失败，原因: {ex.Message}";
                goto ERROR1;
            }

            this.BiblioFormat = "table";

            _writer.Formatting = Formatting.Indented;
            _writer.Indentation = 4;

            _writer.WriteStartDocument();
            _writer.WriteStartElement("dprms", "collection", DpNs.dprms);

            // _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

            /*
<styles>
<style name="footer"
alignment="center"/>
</styles>
* */
            {
                _writer.WriteStartElement("styles");

                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "index");
                _writer.WriteAttributeString("font", _NoFontName); // "ascii:Times New Roman"
                _writer.WriteAttributeString("size", _NoFontSize); // "8pt"
                _writer.WriteAttributeString("style", "bold");
                _writer.WriteEndElement();  // style

                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "accessNo");
                _writer.WriteAttributeString("font", _AccessNoFontName); // "ascii:Times New Roman"
                _writer.WriteAttributeString("size", _AccessNoFontSize); // "8pt"
                _writer.WriteEndElement();  // style


                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "biblio");
                _writer.WriteAttributeString("font", _ContentFontName); // "ascii:Times New Roman,eastAsia:宋体"
                _writer.WriteAttributeString("size", _ContentFontSize); // "9pt"
                _writer.WriteEndElement();  // style

                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "barcode");
                _writer.WriteAttributeString("font", _BarcodeFontName); // "ascii:Times New Roman"
                _writer.WriteAttributeString("size", _BarcodeFontSize); // "8pt"
                _writer.WriteEndElement();  // style

                _writer.WriteEndElement();  // styles
            }

            /*
	<footers>
		<footer><p>page number: <pageNumber/></p>
		</footer>
	</footers>
            * */
            {
                _writer.WriteStartElement("footers");
                _writer.WriteStartElement("footer");
                _writer.WriteStartElement("p");
                _writer.WriteAttributeString("alignment", "center");
                _writer.WriteString("- ");
                _writer.WriteElementString("pageNumber", "");
                _writer.WriteString(" -");
                _writer.WriteEndElement();  // p
                _writer.WriteEndElement();  // footer
                _writer.WriteEndElement();  // footers
            }

            /*
	<columns columnCount="2" separator="true" space="36pt" equalWidth="true">
	</columns>
            * */
            {
                _writer.WriteStartElement("columns");

                _writer.WriteAttributeString("columnCount", "2");
                _writer.WriteAttributeString("separator", "true");
                _writer.WriteAttributeString("space", "10pt");
                _writer.WriteAttributeString("equalWidth", "true");

                _writer.WriteEndElement();  // columns
            }

            /*
	<settings pageNumberStart="100"/>
            * */
            {
                _writer.WriteStartElement("settings");
                _writer.WriteAttributeString("pageNumberStart", _PageNumberStart.ToString());
                _writer.WriteEndElement();  // settings
            }

            _writer.WriteStartElement("table");
            _writer.WriteAttributeString("width", "100%");

            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnRecord(object sender, StatisEventArgs e)
        {
            string strError = "";

            // 装载所有索取号和册条码号
            var result = LoadItems(this.CurrentRecPath);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                goto ERROR1;
            }

            var accessNos = result.AccessNoList;
            StringUtil.RemoveBlank(ref accessNos);
            StringUtil.RemoveDupNoSort(ref accessNos);

            string accessNo = "";
            if (accessNos.Count > 0)
                accessNo = accessNos[0];
            accessNo = StringUtil.GetPlainTextCallNumber(accessNo);

            var barcodes = result.BarcodeList;
            for (int i = 0; i < barcodes.Count; i++)
            {
                if (string.IsNullOrEmpty(barcodes[i]))
                    barcodes[i] = "(空)";
            }

            // 归并连续的号码
            barcodes.Sort((a, b) =>
            {
                if (a.Length != b.Length)
                    return a.Length - b.Length; // 位数少的在前
                return string.CompareOrdinal(a, b); // 同样位数的比较先后
            });
            // barcodes = OutputDocxCatalog.CompactNumbersEx(barcodes);
            barcodes = CompactNumbersEx(barcodes);

            bool first = _index == _BiblioNoStart;

            string book_string = BuildBookString(this.BiblioDom);

            _writer.WriteStartElement("tr");
            // 序号
            {
                _writer.WriteStartElement("td");
                //if (first)
                _writer.WriteAttributeString("width", "auto");    // "20"
                {
                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "index");
                    _writer.WriteString($"{_index++}");
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
            }

            // 册条码号
            {
                _writer.WriteStartElement("td");
                //if (first)
                _writer.WriteAttributeString("width", "auto");    // "50"

                // 条码号
                if (barcodes.Count > 0)
                {
                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "barcode");
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
                                _writer.WriteElementString("br", "");
                            _writer.WriteString(line);
                            i++;
                        }
                    }
                    // _writer.WriteString(StringUtil.MakePathList(barcodes, " "));
                    _writer.WriteEndElement();
                }

                _writer.WriteEndElement();
            }

            // 正文、索取号
            {
                _writer.WriteStartElement("td");
                //if (first)
                _writer.WriteAttributeString("width", "auto");

                // 书目 ISBD
                {
                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "biblio");
                    _writer.WriteAttributeString("alignment", "both");

                    _writer.WriteAttributeString("spacing", "after:3pt");

                    _writer.WriteString(book_string);
                    _writer.WriteEndElement();
                }

                // 索取号
                if (string.IsNullOrEmpty(accessNo) == false)
                {
                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "accessNo");
                    _writer.WriteAttributeString("spacing", "after:3pt");

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
                                _writer.WriteElementString("br", "");
                            _writer.WriteString(line);
                            i++;
                        }
                    }
#endif
                    _writer.WriteString(accessNo.Replace("/", " / "));
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();  // </tr>
            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            if (_writer != null)
            {
                _writer.WriteEndElement();  // </table>

                _writer.WriteEndElement();   // </collection>
                _writer.WriteEndDocument();

                _writer.Close();
                _writer = null;
            }

            string wordFileName = Path.Combine(Program.MainForm.UserTempDir, "~output_docx_catalog.docx");

            TypoUtility.XmlToWord(_outputFileName, wordFileName);
            Process.Start(wordFileName);
        }

        int _BiblioNoStart = 1;
        int _PageNumberStart = 1;
        // 序号字体
        string _NoFontName = "";
        string _NoFontSize = "";
        string _BarcodeFontName = "";
        string _BarcodeFontSize = "";
        string _ContentFontName = "";
        string _ContentFontSize = "";
        string _AccessNoFontName = "";
        string _AccessNoFontSize = "";

        bool InputSettings()
        {
            using (OutputDocxCatalogDialog dlg = new OutputDocxCatalogDialog())
            {
                dlg.UiState = Program.MainForm.AppInfo.GetString(
    "OutputDocxCatalog",
    "uiState",
    "");

                dlg.ShowDialog(this.BiblioStatisForm);

                Program.MainForm.AppInfo.SetString(
    "OutputDocxCatalog",
    "uiState",
    dlg.UiState);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return false;

                _BiblioNoStart = dlg.BiblioNoStart;
                _PageNumberStart = dlg.PageNumberStart;
                _NoFontName = dlg.NoFontName;
                _NoFontSize = dlg.NoFontSize;
                _BarcodeFontName = dlg.BarcodeFontName;
                _BarcodeFontSize = dlg.BarcodeFontSize;
                _ContentFontName = dlg.ContentFontName;
                _ContentFontSize = dlg.ContentFontSize;
                _AccessNoFontName = dlg.AccessNoFontName;
                _AccessNoFontSize = dlg.AccessNoFontSize;

                {
                    if (string.IsNullOrEmpty(_NoFontName))
                        _NoFontName = "ascii:Times New Roman";
                    if (string.IsNullOrEmpty(_NoFontSize))
                        _NoFontSize = "8pt";

                    if (string.IsNullOrEmpty(_BarcodeFontName))
                        _BarcodeFontName = "ascii:Times New Roman";
                    if (string.IsNullOrEmpty(_BarcodeFontSize))
                        _BarcodeFontSize = "8pt";

                    if (string.IsNullOrEmpty(_ContentFontName))
                        _ContentFontName = "ascii:Times New Roman,eastAsia:宋体,hAnsi:Times New Roman";
                    if (string.IsNullOrEmpty(_ContentFontSize))
                        _ContentFontSize = "9pt";

                    if (string.IsNullOrEmpty(_AccessNoFontName))
                        _AccessNoFontName = "ascii:Times New Roman,hAnsi:Times New Roman";
                    if (string.IsNullOrEmpty(_AccessNoFontSize))
                        _AccessNoFontSize = "8pt";
                }
                return true;
            }
        }

        static string BuildBookString(XmlDocument table_dom)
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
                var type_name = types.Find((s) => s.EndsWith("_area"));
                if (type_name == null)
                    continue;

                if (text.Length > 0)
                    text.Append(". -- ");
                text.Append(value);
            }

            return text.ToString();
        }

        class LoadItemsResult : NormalResult
        {
            public List<string> AccessNoList { get; set; }
            public List<string> BarcodeList { get; set; }
        }

        LoadItemsResult LoadItems(string biblio_recpath)
        {
            var channel = this.BiblioStatisForm.GetChannel();
            try
            {
                SubItemLoader loader = new SubItemLoader();
                loader.BiblioRecPath = biblio_recpath;
                loader.Channel = channel;
                loader.Stop = null;
                loader.DbType = "item";
                loader.Format = "xml";

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                List<string> barcodes = new List<string>();
                List<string> accessNos = new List<string>();
                foreach (EntityInfo info in loader)
                {
                    if (info.ErrorCode != ErrorCodeValue.NoError)
                    {
                        string error = "路径为 '" + info.OldRecPath + "' 的册记录装载中发生错误: " + info.ErrorInfo; // NewRecPath
                        return new LoadItemsResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                    }

                    if (string.IsNullOrEmpty(info.OldRecord))
                        continue;

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldRecord);
                    }
                    catch (Exception ex)
                    {
                        return new LoadItemsResult
                        {
                            Value = -1,
                            ErrorInfo = $"册记录 {info.OldRecPath} XML 装入 XMLDOM 出现异常: {ex.Message}"
                        };
                    }
                    string barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    string accessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");
                    barcodes.Add(barcode);
                    accessNos.Add(accessNo);
                }

                return new LoadItemsResult
                {
                    Value = 0,
                    BarcodeList = barcodes,
                    AccessNoList = accessNos,
                };
            }
            finally
            {
                this.BiblioStatisForm.ReturnChannel(channel);
            }
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this.BiblioStatisForm,
        e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
        20 * 1000,
        "OutputDocxCatalog");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

#if OLD
        // 合并连续的号码
        // 一条书目信息下有4册实体，021001，021002，021003，021004，显示成：021001-021004，“021001”“-”“021004”各占一行，一共占3行。
        public static List<string> CompactNumbersEx(List<string> source)
        {
            List<string> results = new List<string>();

            string strPrev = "";
            string strStart = "";
            string strTail = "";
            int delta = 0;  // strStart 和 strTail 之间间隔的号码个数
            for (int i = 0; i < source.Count; i++)
            {
                string strCurrent = source[i];
                if (string.IsNullOrEmpty(strPrev) == false)
                {
                    string strResult = "";

                    string strError = "";
                    // 给一个被字符引导的数字增加一个数量。
                    // 例如 B019X + 1 变成 B020X
                    int nRet = StringUtil.IncreaseNumber(strPrev,
            1,
            out strResult,
            out strError);
                    if (nRet == -1)
                        continue;
                    delta++;
                    if (strCurrent == strResult)
                    {
                        if (strStart == "")
                        {
                            strStart = strPrev;
                            delta = 0;
                        }
                        strTail = strCurrent;
                    }
                    else
                    {
                        if (strStart != "")
                        {
                            //int nLengh = GetCommonPartLength(strStart, strTail);
                            //results.Add(strStart + "-" + strTail.Substring(nLengh));
                            {
                                results.Add(strStart);
                                if (delta > 1)
                                    results.Add("-");
                                results.Add(strTail);
                                delta = 0;
                            }
                            strStart = "";
                            strTail = "";
                        }
                        else
                        {
                            // results.Add(strCurrent);
                            results.Add(strPrev);
                            delta = 0;
                        }
                    }
                }

                strPrev = strCurrent;
            }

            if (strStart != "")
            {
                //int nLengh = GetCommonPartLength(strStart, strTail);
                //results.Add(strStart + "-" + strTail.Substring(nLengh));
                {
                    results.Add(strStart);
                    if (delta > 1)
                        results.Add("-");
                    results.Add(strTail);
                    delta = 0;
                }
                strStart = "";
                strTail = "";
                strPrev = "";   // 2022/8/23
            }

            if (string.IsNullOrEmpty(strPrev) == false)
            {
                results.Add(strPrev);
            }
            return results;
        }
#endif

        public static List<string> CompactNumbersEx(List<string> source)
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
                    results.Add("-");
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Test_compactNumber_02()
        {
            List<string> source = new List<string>() { "0000001" };
            var result = OutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("0000001", result[0]);
        }

        [TestMethod]
        public void Test_compactNumber_03()
        {
            // 只有两个号码的连续范围，合并后依然表达为两个独立号码
            List<string> source = new List<string>() { "0000001",
                "0000002" };
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("B0000001", result[0]);
        }

        [TestMethod]
        public void Test_compactNumber_13()
        {
            List<string> source = new List<string>() { "B0000001",
                "B0000002" };
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
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
            var result = OutputDocxCatalog.CompactNumbersEx(source);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("B0000001", result[0]);
            Assert.AreEqual("-", result[1]);
            Assert.AreEqual("B0000004", result[2]);
        }
    }
}
