using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
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

using DocumentFormat.OpenXml.InkML;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Typography;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

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
                _writer.WriteAttributeString("font", "ascii:Times New Roman");
                _writer.WriteAttributeString("size", "8pt");
                _writer.WriteAttributeString("style", "bold");
                _writer.WriteEndElement();  // style

                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "accessNo");
                _writer.WriteAttributeString("font", "ascii:Times New Roman");
                _writer.WriteAttributeString("size", "8pt");
                _writer.WriteEndElement();  // style


                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "biblio");
                _writer.WriteAttributeString("font", "ascii:Times New Roman,eastAsia:宋体");
                _writer.WriteAttributeString("size", "9pt");
                _writer.WriteEndElement();  // style

                _writer.WriteStartElement("style");
                _writer.WriteAttributeString("name", "barcode");
                _writer.WriteAttributeString("font", "ascii:Times New Roman");
                _writer.WriteAttributeString("size", "8pt");
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
            barcodes.Sort((a, b) => {
                if (a.Length != b.Length)
                    return a.Length - b.Length; // 位数少的在前
                return string.CompareOrdinal(a, b); // 同样位数的比较先后
            });
            barcodes = StringUtil.CompactNumbers(barcodes);

            bool first = _index == 0;

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
                    _writer.WriteString($"{++_index}");
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
                    _writer.WriteString(StringUtil.MakePathList(barcodes, " "));
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

        // 合并连续的号码

    }
}
