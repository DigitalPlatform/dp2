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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Typography;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.CirculationClient;

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

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

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

            this.BiblioFormat = $"table:{_areas}";

            _writer.Formatting = Formatting.Indented;
            _writer.Indentation = 4;

            OutputBegin();
            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public virtual void OutputBegin()
        {
            _writer.WriteStartDocument();
            _writer.WriteStartElement("dprms", "collection", DpNs.dprms);

            // _writer.WriteAttributeString("xmlns", "dprms", null, DpNs.dprms);

            OutputStyles();

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
        }

        public virtual void OutputStyles()
        {
            /*
<styles>
<style name="footer"
alignment="center"/>
</styles>
* */
            {
                _writer.WriteStartElement("styles");

                OutputStyleElements();

                _writer.WriteEndElement();  // styles
            }
        }

        public virtual void OutputStyleElements()
        {
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

            // 定义一个 bold 样式，以节省 Word 文档中的样式总数
            _writer.WriteStartElement("style");
            _writer.WriteAttributeString("name", "bold");
            _writer.WriteAttributeString("type", "character");
            _writer.WriteAttributeString("style", "bold");
            _writer.WriteEndElement();  // style
        }

        public virtual void OutputEnd()
        {
            if (_writer != null)
            {
                _writer.WriteEndElement();  // </table>

                _writer.WriteEndElement();   // </collection>
                _writer.WriteEndDocument();

                _writer.Close();
                _writer = null;
            }
        }

        public virtual void OutputRecord(
            string accessNo,
            List<string> barcodes,
            string book_string)
        {

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

            // 索取号和册条码号
            {
                _writer.WriteStartElement("td");
                //if (first)
                _writer.WriteAttributeString("width", "auto");    // "50"

                // 索取号
                if (string.IsNullOrEmpty(accessNo) == false)
                {
                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "accessNo");

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

                    // _writer.WriteString(accessNo.Replace("/", " / "));
                    _writer.WriteEndElement();
                }

                // 条码号
                if (barcodes.Count > 0)
                {
                    // barcodes = OutputDocxCatalog.CompactNumbersEx(barcodes);
                    barcodes = StringUtil.CompactNumbers(barcodes);

                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "barcode");
                    _writer.WriteAttributeString("spacing", "before:8pt,after:3pt");
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

            // 正文
            {
                _writer.WriteStartElement("td");
                //if (first)
                _writer.WriteAttributeString("width", "auto");

                /*
                // 书目 ISBD
                {
                    _writer.WriteStartElement("p");
                    _writer.WriteAttributeString("style", "biblio");
                    _writer.WriteAttributeString("alignment", "both");

                    _writer.WriteAttributeString("spacing", "after:3pt");

                    _writer.WriteString(book_string);
                    _writer.WriteEndElement();
                }
                */

                // 书目 ISBD
                OutputContentParagraph(book_string);

                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();  // </tr>
        }

        public virtual void OutputContentParagraph(string book_string)
        {
            // 书目 ISBD
            {
                _writer.WriteStartElement("p");
                _writer.WriteAttributeString("style", "biblio");
                _writer.WriteAttributeString("alignment", "both");

                _writer.WriteAttributeString("spacing", "after:3pt");

                {
                    // 注意 book_string 是 InnerXml 形态
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml("<root />");
                    dom.DocumentElement.InnerXml = book_string;
                    dom.DocumentElement.WriteContentTo(_writer);
                }
                _writer.WriteEndElement();
            }
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

            bool first = _index == _BiblioNoStart;

            // 注意 book_string 是 InnerXml 形态
            string book_string = BuildBookString(this.BiblioDom, StringUtil.SplitList(_areas, "|"));

            OutputRecord(accessNo, barcodes, book_string);
            return;
        ERROR1:
            e.Continue = ContinueType.Error;
            e.ParamString = strError;
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            OutputEnd();

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

        bool _boldTitleArea = false;

        string _areas = "";

        public string _defaultNoFontName = "ascii:Times New Roman";
        public string _defaultNoFontSize = "8pt";
        public string _defaultBarcodeFontName = "ascii:Times New Roman";
        public string _defaultBarcodeFontSize = "7pt";
        public string _defaultContentFontName = "ascii:Times New Roman,eastAsia:宋体,hAnsi:Times New Roman";
        public string _defaultContentFontSize = "9pt";
        public string _defaultAccessNoFontName = "ascii:Times New Roman,hAnsi:Times New Roman";
        public string _defaultAccessNoFontSize = "8pt";

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
                _boldTitleArea = dlg.BoldTitleArea;
                _areas = dlg.AreaList;

                {
                    if (string.IsNullOrEmpty(_NoFontName))
                        _NoFontName = _defaultNoFontName;
                    if (string.IsNullOrEmpty(_NoFontSize))
                        _NoFontSize = _defaultNoFontSize;

                    if (string.IsNullOrEmpty(_BarcodeFontName))
                        _BarcodeFontName = _defaultBarcodeFontName;
                    if (string.IsNullOrEmpty(_BarcodeFontSize))
                        _BarcodeFontSize = _defaultBarcodeFontSize;

                    if (string.IsNullOrEmpty(_ContentFontName))
                        _ContentFontName = _defaultContentFontName;
                    if (string.IsNullOrEmpty(_ContentFontSize))
                        _ContentFontSize = _defaultContentFontSize;

                    if (string.IsNullOrEmpty(_AccessNoFontName))
                        _AccessNoFontName = _defaultAccessNoFontName;
                    if (string.IsNullOrEmpty(_AccessNoFontSize))
                        _AccessNoFontSize = _defaultAccessNoFontSize;
                }
                return true;
            }
        }

        // “题名与责任者项”加粗的版本
        // 返回 InnerXml
        public virtual string BuildBookString(XmlDocument table_dom, List<string> field_list)
        {
            if (table_dom == null || table_dom.DocumentElement == null)
                return null;

            char tail_char = (char)0;  // 前次处理的最后一个字符

            XmlDocument output_dom = new XmlDocument();
            output_dom.LoadXml("<root />");

            var line_nodes = table_dom.DocumentElement.SelectNodes("line");
            foreach (XmlElement line in line_nodes)
            {
                // line @type @value @name
                string type = line.GetAttribute("type");

                var types = StringUtil.SplitList(type);
                // var type_name = types.Find((s) => s.EndsWith("_area"));
                var type_name = types.Find((s) => field_list.IndexOf(s) != -1);
                if (type_name == null)
                    continue;

                string name = line.GetAttribute("name");
                string value = line.GetAttribute("value");

                // 2022/8/26
                if (string.IsNullOrEmpty(value) == false)
                    value = value.Replace("\n", "; ");

                if (output_dom.DocumentElement.ChildNodes.Count > 0)
                {
                    if (tail_char == '.')
                        AppendTextNode(" -- ");
                    else
                        AppendTextNode(". -- ");

                    tail_char = ' ';
                }

                if (type_name == "title_area" && _boldTitleArea)
                {
                    var style_node = output_dom.CreateElement("style");
                    output_dom.DocumentElement.AppendChild(style_node);

                    style_node.SetAttribute("use", "bold");
                    style_node.InnerText = value;
                }
                else
                {
                    AppendTextNode(value);
                }

                tail_char = GetTailChar(value);
            }

            void AppendTextNode(string text)
            {
                var text_node = output_dom.CreateTextNode(text);
                output_dom.DocumentElement.AppendChild(text_node);
            }

            char GetTailChar(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return (char)0;
                return text[text.Length - 1];
            }

            return output_dom.DocumentElement.InnerXml;
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
                SubItemLoader loader = new SubItemLoader
                {
                    BiblioRecPath = biblio_recpath,
                    Channel = channel,
                    Stop = null,
                    DbType = "item",
                    Format = "xml",
                    ItemDbNotDefAsError = false,
                };

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

                    // 过滤册记录
                    if (FilterItem(dom) == true)
                        continue;

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

        // 过滤册记录
        // return:
        //      true    要跳过输出
        //      false   不跳过
        public virtual bool FilterItem(XmlDocument itemdom)
        {
            // 2022/8/26
            // 跳过期刊合订后的单册记录
            // 注意：并没有跳过现刊单册记录(也就是未装订的单册记录)
            XmlNode nodeParentItem = itemdom.DocumentElement.SelectSingleNode("binding/bindingParent");
            if (nodeParentItem != null)
                return true;

            // 跳过已经注销的册
            string state = DomUtil.GetElementText(itemdom.DocumentElement, "state");
            if (StringUtil.IsInList("注销,丢失,加工中", state))
                return true;

            return false;
        }

        PromptManager _prompt = new PromptManager(-1);

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            _prompt.Prompt(this.BiblioStatisForm, e);

            /*
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
            */
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

    }

}
