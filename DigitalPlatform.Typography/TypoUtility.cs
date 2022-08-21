using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using DocumentFormat.OpenXml.VariantTypes;
using System.Collections.Generic;


using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System.Xml.Linq;

namespace DigitalPlatform.Typography
{
    // https://devhints.io/xpath
    // XPath 语法参考
    // https://github.com/EvotecIT/OfficeIMO
    // https://github.com/OfficeDev/Open-XML-SDK

    public static class TypoUtility
    {
        public static void XmlToWord(string xmlFileName,
            string wordFileName)
        {
            XmlDocument dom = new XmlDocument();
            dom.Load(xmlFileName);

            using (WordprocessingDocument doc = WordprocessingDocument.Create(wordFileName, WordprocessingDocumentType.Document))
            {
                // Add a main document part. 
                MainDocumentPart mainPart = doc.AddMainDocumentPart();

                // Create the document structure and add some text.
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());
                Debug.Assert(body == mainPart.Document.Body);

                var first_level_nodes = dom.DocumentElement.SelectNodes("*");
                CreateNodes(doc,
    mainPart.Document.Body,
    first_level_nodes);
#if REMOVED
                foreach (XmlElement node in first_level_nodes)
                {
                    if (node.Name == "styles")
                    {
                        CreateStyles(mainPart, node);
                    }

                    // p 元素
                    if (node.Name == "p")
                    {
                        CreateParagraph(doc, body, node);
                    }

                    // table 元素
                    if (node.Name == "table")
                    {
                        CreateTable(body, node);
                    }
                }
#endif

                var columns_node = dom.DocumentElement.SelectSingleNode("columns") as XmlElement;
                if (columns_node != null)
                    CreateColumns(doc, columns_node);

                var headers_node = dom.DocumentElement.SelectSingleNode("headers") as XmlElement;
                if (headers_node != null)
                    CreateHeaders(doc, headers_node);

                var footers_node = dom.DocumentElement.SelectSingleNode("footers") as XmlElement;
                if (footers_node != null)
                    CreateFooters(doc, footers_node);
            }
        }

        // https://social.technet.microsoft.com/Forums/en-US/afbb713d-00b6-42d3-b045-2cc3aa5dd338/how-to-change-the-header-and-footer-in-the-section-breaks-next-page-using-openxml
        static void CreateHeaders(WordprocessingDocument doc,
    XmlElement headers_node)
        {
            SectionProperties sectPr = EnsureSectionProperty(doc);

            HeaderPart headerPart2 = doc.MainDocumentPart.AddNewPart<HeaderPart>("rIdHeader");
            var header_node = headers_node.SelectSingleNode("header");
            if (header_node != null)
            {
                if (headerPart2.Header == null)
                    headerPart2.Header = new Header();
                CreateNodes(doc, headerPart2.Header, header_node.ChildNodes);
            }

            // GenerateHeaderPartContent(headerPart2);

            sectPr.GetFirstChild<HeaderReference>()?.Remove();
            HeaderReference headerReference1 = new HeaderReference()
            {
                Type = HeaderFooterValues.Default,
                Id = "rIdHeader"
            };
            sectPr.Append(headerReference1);
        }

        /*
        static void GenerateHeaderPartContent(HeaderPart hpart)
        {
            Header header1 = new Header();
            Paragraph paragraph1 = new Paragraph();
            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "Header" };
            paragraphProperties1.Append(paragraphStyleId1);
            Run run1 = new Run();
            Text text1 = new Text();
            text1.Text = "";
            run1.Append(text1);
            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            header1.Append(paragraph1);
            hpart.Header = header1;
        }
        */

        static void CreateFooters(WordprocessingDocument doc,
XmlElement footers_node)
        {
            SectionProperties sectPr = EnsureSectionProperty(doc);

            var footerPart2 = doc.MainDocumentPart.AddNewPart<FooterPart>("rIdFooter");
            var footer_node = footers_node.SelectSingleNode("footer");
            if (footer_node != null)
            {
                if (footerPart2.Footer == null)
                    footerPart2.Footer = new Footer();
                CreateNodes(doc, footerPart2.Footer, footer_node.ChildNodes);
            }

            sectPr.GetFirstChild<FooterReference>()?.Remove();
            FooterReference footerReference1 = new FooterReference()
            {
                Type = HeaderFooterValues.Default,
                Id = "rIdFooter"
            };
            sectPr.Append(footerReference1);
        }

        static SectionProperties EnsureSectionProperty(WordprocessingDocument doc)
        {
            var sectPrs =
doc.MainDocumentPart.Document.Body.Elements<SectionProperties>().ToList();
            if (sectPrs.Count == 0)
            {
                return doc.MainDocumentPart.Document.Body.AppendChild<SectionProperties>(new SectionProperties());
            }
            else
                return sectPrs[0];
        }

        // 创建全局 sectPr
        static void CreateColumns(WordprocessingDocument doc,
            XmlElement columns_node)
        {
            SectionProperties sectPr = EnsureSectionProperty(doc);
            /*
            var sectPrs =
doc.MainDocumentPart.Document.Body.Elements<SectionProperties>().ToList();
            if (sectPrs.Count == 0)
            {
                sectPr = doc.MainDocumentPart.Document.Body.AppendChild<SectionProperties>(new SectionProperties());
            }
            else
                sectPr = sectPrs[0];
            */

            sectPr.RemoveAllChildren<Columns>();
            var columns = sectPr.PrependChild<Columns>(new Columns());

            {
                var columnCount_attr = columns_node.GetAttribute("columnCount");
                if (string.IsNullOrEmpty(columnCount_attr) == false)
                    columns.ColumnCount = Convert.ToInt16(columnCount_attr);

                var equalWidth_attr = columns_node.GetAttribute("equalWidth");
                if (string.IsNullOrEmpty(equalWidth_attr) == false)
                    columns.EqualWidth = DomUtil.IsBooleanTrue(equalWidth_attr);

                var separator_attr = columns_node.GetAttribute("separator");
                if (string.IsNullOrEmpty(separator_attr) == false)
                    columns.Separator = DomUtil.IsBooleanTrue(separator_attr);

                var space_attr = columns_node.GetAttribute("space");
                if (string.IsNullOrEmpty(space_attr) == false)
                {
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.columns.space?view=openxml-2.8.1
                    // unit: twentieths of a point
                    columns.Space = GetTwentieths(space_attr);
                }
            }

            var column_nodes = columns_node.SelectNodes("column");
            foreach (XmlElement column_node in column_nodes)
            {
                var column = new Column();
                columns.AppendChild(column);

                var width_attr = column_node.GetAttribute("width");
                if (string.IsNullOrEmpty(width_attr) == false)
                {
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.columns.space?view=openxml-2.8.1
                    // unit: twentieths of a point
                    column.Width = GetTwentieths(width_attr);
                }

                var space_attr = column_node.GetAttribute("space");
                if (string.IsNullOrEmpty(space_attr) == false)
                {
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.columns.space?view=openxml-2.8.1
                    // unit: twentieths of a point
                    column.Space = GetTwentieths(space_attr);
                }
            }
        }

        static void CreateNodes(WordprocessingDocument doc,
            OpenXmlElement body,
            XmlNodeList nodes)
        {
            // var p_nodes = nodes.Cast<XmlNode>().Where(n => n.Name == "p").ToList();

            Paragraph p = null;
            // var mainPart = doc.MainDocumentPart;
            foreach (XmlNode node in nodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    if (p == null)
                    {
                        // 裸露的 XmlNode.Text 节点，要用 p 元素包住
                        p = new Paragraph();
                        body.AppendChild(p);
                    }

                    p.AppendChild(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(node.Value)));

                    // body.AppendChild(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(node.Value))));
                    // TableCell tc1 = new TableCell(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(td.InnerText))));
                    continue;
                }

                // 在全局位置创建 styles
                if (node.Name == "styles")
                {
                    CreateStyles(doc, node as XmlElement);
                    continue;
                }

                // p 元素
                if (node.Name == "p")
                {
                    CreateParagraph(doc,
                        p == null ? body : p,
                        node as XmlElement);
                    continue;
                }

                // table 元素
                if (node.Name == "table")
                {
                    CreateTable(doc,
                        p == null ? body : p,
                        node as XmlElement);
                    continue;
                }
            }
        }

        static void EnsureStyles(WordprocessingDocument doc)
        {
            StyleDefinitionsPart part =
doc.MainDocumentPart.StyleDefinitionsPart;
            if (part == null)
            {
                part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            }

            if (part.Styles == null)
            {
                var root = new Styles();
                root.Save(part);
            }

            Debug.Assert(part.Styles != null);
        }

        static Styles CreateStyles(WordprocessingDocument doc,
            XmlElement styles)
        {
            EnsureStyles(doc);

            StyleDefinitionsPart part =
doc.MainDocumentPart.StyleDefinitionsPart;

            var style_nodes = styles.SelectNodes("style");
            foreach (XmlElement style_node in style_nodes)
            {
                part.Styles.Append(CreateStyle(doc, style_node, null, StyleValues.Paragraph));
            }

            return part.Styles;
        }

        static int _styleIdSeed = 1;
        static string NewStyleID()
        {
            return (_styleIdSeed++).ToString();
        }

        static int _styleNameSeed = 1;
        static string NewStyleName()
        {
            return "temp" + (_styleNameSeed++).ToString();
        }

        // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.runproperties?redirectedfrom=MSDN&view=openxml-2.8.1
        // https://docs.microsoft.com/en-us/office/open-xml/how-to-create-and-add-a-paragraph-style-to-a-word-processing-document
        // https://docs.microsoft.com/en-us/office/open-xml/how-to-apply-a-style-to-a-paragraph-in-a-word-processing-document
        // https://docs.microsoft.com/en-us/office/open-xml/how-to-create-and-add-a-character-style-to-a-word-processing-document
        static Style CreateStyle(
            WordprocessingDocument doc,
            XmlElement style_node,
            Style base_style,
            StyleValues default_type)
        {
            StyleValues type = default_type;
            var stylename = style_node.GetAttribute("name");
            if (string.IsNullOrEmpty(stylename))
                stylename = NewStyleName();

            var styletype = style_node.GetAttribute("type");
            if (string.IsNullOrEmpty(styletype) == false)
            {
                type = GetStyleType(styletype);
            }

            // Parent Style ID
            // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.style.basedon?view=openxml-2.8.1#documentformat-openxml-wordprocessing-style-basedon
            var baseOnName = style_node.GetAttribute("baseOn");

            string baseOn = null;
            // baseOn 属性优先于 base_style 参数
            if (string.IsNullOrEmpty(baseOnName) == false)
                baseOn = GetStyleIdFromStyleName(doc, baseOnName);
            else if (base_style != null)
                baseOn = base_style.StyleId;

            var styleid = NewStyleID();

            Style style = new Style()
            {
                Type = type,    // StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            StyleName styleName1 = new StyleName() { Val = stylename };
            style.Append(styleName1);
            if (string.IsNullOrEmpty(baseOn) == false)
            {
                BasedOn basedOn1 = new BasedOn() { Val = baseOn };
                style.Append(basedOn1);
            }
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = baseOn };
            style.Append(nextParagraphStyle1);

            StyleRunProperties styleRunProperties1 = new StyleRunProperties();
            style.Append(styleRunProperties1);

            var font = style_node.GetAttribute("font");
            if (string.IsNullOrEmpty(font) == false)
            {
                RunFonts font1 = new RunFonts();
                styleRunProperties1.Append(font1);

                var ascii = StringUtil.GetParameterByPrefix(font, "ascii");
                if (string.IsNullOrEmpty(ascii) == false)
                    font1.Ascii = ascii;

                var hAnsi = StringUtil.GetParameterByPrefix(font, "hAnsi");
                if (string.IsNullOrEmpty(hAnsi) == false)
                    font1.HighAnsi = hAnsi;

                var eastAsia = StringUtil.GetParameterByPrefix(font, "eastAsia");
                if (string.IsNullOrEmpty(eastAsia) == false)
                    font1.EastAsia = eastAsia;

                var cs = StringUtil.GetParameterByPrefix(font, "cs");
                if (string.IsNullOrEmpty(cs) == false)
                    font1.ComplexScript = cs;
            }

            {
                var style_attr = style_node.GetAttribute("style");
                if (StringUtil.IsInList("bold", style_attr))
                {
                    styleRunProperties1.Append(new Bold());
                }
                if (StringUtil.IsInList("italic", style_attr))
                {
                    styleRunProperties1.Append(new Italic());
                }
            }

            /*
            Color color1 = new Color() { ThemeColor = ThemeColorValues.Accent2 };
            styleRunProperties1.Append(color1);
            */
            var size = style_node.GetAttribute("size");
            if (string.IsNullOrEmpty(size) == false)
            {
                // FontSize::Val: Half Point Measurement 二分之一点
                styleRunProperties1.Append(GetFontSize(size));
            }

            return style;
        }

        static StyleValues GetStyleType(string text)
        {
            if (text == "paragraph")
                return StyleValues.Paragraph;
            else if (text == "character")
                return StyleValues.Character;
            else if (text == "table")
                return StyleValues.Table;
            else if (text == "numbering")
                return StyleValues.Numbering;
            else
                return StyleValues.Paragraph;
        }

        // https://docs.microsoft.com/en-us/office/open-xml/how-to-apply-a-style-to-a-paragraph-in-a-word-processing-document
        // https://docs.microsoft.com/en-us/office/open-xml/working-with-paragraphs
        static Paragraph CreateParagraph(WordprocessingDocument doc,
            OpenXmlElement body,
            XmlElement paragraph)
        {
            // Body body = doc.MainDocumentPart.Document.Body;

            Paragraph p = body.AppendChild(new Paragraph());
            // p 元素的 style 属性
            string style_name = paragraph.GetAttribute("style");
            if (string.IsNullOrEmpty(style_name) == false)
            {
                if (p.Elements<ParagraphProperties>().Count() == 0)
                {
                    p.PrependChild<ParagraphProperties>(new ParagraphProperties());
                }

                ParagraphProperties pPr = p.Elements<ParagraphProperties>().First();

                // style name 找到 style id
                var style_id = GetStyleIdFromStyleName(doc, style_name);
                if (style_id == null)
                    throw new Exception($"Style Name '{style_name}' not found");

                pPr.ParagraphStyleId = new ParagraphStyleId() { Val = style_id };
            }

            string alignment = paragraph.GetAttribute("alignment");
            if (string.IsNullOrEmpty(alignment) == false)
            {
                if (p.Elements<ParagraphProperties>().Count() == 0)
                {
                    p.PrependChild<ParagraphProperties>(new ParagraphProperties());
                }

                ParagraphProperties pPr = p.Elements<ParagraphProperties>().First();
                if (Enum.TryParse<JustificationValues>(alignment, true, out JustificationValues value) == false)
                {
                    throw new Exception($"alignment 属性值 '{alignment}' 不合法");
                }
                Justification objJustification =
    new Justification() { Val = value };
                pPr.Append(objJustification);
            }

            // p 元素的下级节点
            CreateTextStream(doc, p, paragraph.ChildNodes);
#if REMOVED
            foreach (XmlNode child_node in paragraph.ChildNodes)
            {
                if (child_node.NodeType == XmlNodeType.Text)
                {
                    Run run = p.AppendChild(new Run());
                    run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(child_node.Value));
                    continue;
                }

                if (child_node.Name == "style")
                {
                    EnsureStyles(doc);
                    var style = CreateStyle(child_node as XmlElement, StyleValues.Character);
                    StyleDefinitionsPart part =
            doc.MainDocumentPart.StyleDefinitionsPart; 
                    part.Styles.Append(style);

                    Run run = p.AppendChild(new Run());
                    run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(child_node.Value));

                    if (run.Elements<RunProperties>().Count() == 0)
                    {
                        run.PrependChild<RunProperties>(new RunProperties());
                    }
                    RunProperties rPr = run.RunProperties;

                    if (rPr.RunStyle == null)
                        rPr.RunStyle = new RunStyle();
                    rPr.RunStyle.Val = style.StyleId;
                    continue;
                }
            }
#endif

            return p;
        }

        // parameters:
        //      style    氛围 style
        static void CreateTextStream(
            WordprocessingDocument doc,
            OpenXmlElement p,
            XmlNodeList nodes,
            Style style = null)
        {
            bool parent_is_paragraph = p is Paragraph;
            Paragraph temp_paragraph = null;
            // p 元素的下级节点
            foreach (XmlNode child_node in nodes)
            {
                if (child_node.NodeType == XmlNodeType.Text)
                {
                    Run run = CreateParagraphIfNeed().AppendChild(new Run());
                    run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(child_node.Value));

                    if (style != null)
                    {
                        if (run.Elements<RunProperties>().Count() == 0)
                        {
                            run.PrependChild<RunProperties>(new RunProperties());
                        }
                        RunProperties rPr = run.RunProperties;

                        if (rPr.RunStyle == null)
                            rPr.RunStyle = new RunStyle();
                        rPr.RunStyle.Val = style.StyleId;
                    }

                    continue;
                }

                // p 元素
                if (child_node.Name == "p")
                {
                    CreateParagraph(doc,
                        p,
                        child_node as XmlElement);
                    temp_paragraph = null;  // 打断
                    continue;
                }

                OpenXmlElement CreateParagraphIfNeed()
                {
                    if (p is Paragraph)
                        return p;
                    if (temp_paragraph != null)
                        return temp_paragraph;
                    temp_paragraph = p.AppendChild(new Paragraph());
                    return temp_paragraph;
                }

                // TODO: 可以考虑和附近的一个 Run 合并?
                if (child_node.Name == "pageNumber")
                {
                    Run run = CreateParagraphIfNeed().AppendChild(new Run());
                    // run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(child_node.Value));

                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.pagenumber?view=openxml-2.8.1
                    run.AppendChild<PageNumber>(new PageNumber());
                    continue;
                }

                if (child_node.Name == "pageCount")
                {
                    // https://social.msdn.microsoft.com/Forums/office/en-US/5e89bb1d-543b-407b-8d9c-eb19f654c227/force-updating-numpages-field-using-c?forum=oxmlsdk
                    SimpleField simpleField1 = new SimpleField()
                    {
                        Instruction = " NUMPAGES   \\* MERGEFORMAT "
                    };
                    CreateParagraphIfNeed().AppendChild(simpleField1);
                    continue;
                }

                if (child_node.Name == "style")
                {
                    EnsureStyles(doc);
                    var new_style = CreateStyle(doc, child_node as XmlElement, style, StyleValues.Character);
                    StyleDefinitionsPart part =
            doc.MainDocumentPart.StyleDefinitionsPart;
                    part.Styles.Append(new_style);

                    CreateTextStream(doc,
                        p,
                        child_node.ChildNodes,
                        new_style);
                    /*
                    Run run = p.AppendChild(new Run());
                    run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(child_node.Value));

                    if (run.Elements<RunProperties>().Count() == 0)
                    {
                        run.PrependChild<RunProperties>(new RunProperties());
                    }
                    RunProperties rPr = run.RunProperties;

                    if (rPr.RunStyle == null)
                        rPr.RunStyle = new RunStyle();
                    rPr.RunStyle.Val = style.StyleId;
                    continue;
                    */
                }
            }

        }

        // https://docs.microsoft.com/en-us/office/open-xml/how-to-insert-a-table-into-a-word-processing-document?view=openxml-2.8.1
        static Table CreateTable(
            WordprocessingDocument doc,
            OpenXmlElement body,
            XmlElement table)
        {
            // Create a table.
            Table tbl = new Table();
            body.AppendChild(tbl);

            // Set the style and width for the table.
            TableProperties tableProp = new TableProperties();
            tbl.AppendChild(tableProp);

            TableStyle tableStyle = new TableStyle() { Val = "TableGrid" };
            tableProp.Append(tableStyle);

            // table 的 width 属性
            {
                var width = table.GetAttribute("width");
                if (string.IsNullOrEmpty(width) == false)
                {
                    // Specify the width property of the table cell.
                    // Dxa:  Width in Twentieths of a Point. 二十分之一点
                    tableProp.Append(GetTableWidth(width));
                }
            }
            /*
            // Make the table width 100% of the page width.
            // Pct: Width in Fiftieths of a Percent. 百分之五十分之一
            TableWidth tableWidth = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };

            // Apply
            tableProp.Append(tableStyle, tableWidth);
            */

            // 选择第一个 tr
            var headers = table.SelectNodes("tr[1]/*[name()='th' or name()='td']");
            if (headers.Count > 0)
            {
                // Add columns to the table.
                // TableGrid tg = new TableGrid(new GridColumn(), new GridColumn(), new GridColumn());
                TableGrid tg = new TableGrid();
                tbl.AppendChild(tg);
                foreach (XmlElement header in headers)
                {
                    var column = new GridColumn();
                    // GridColumn:
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.gridcolumn?view=openxml-2.8.1
                    tg.AppendChild(column);

                    /*
                    var width = header.GetAttribute("width");
                    if (string.IsNullOrEmpty(width) == false)
                        column.Width = width;
                    */
                }
            }

            var trs = table.SelectNodes("tr");
            foreach (XmlElement tr in trs)
            {
                TableRow tr1 = new TableRow();
                tbl.AppendChild(tr1);

                var tds = tr.SelectNodes("*[name()='th' or name()='td']");
                foreach (XmlElement td in tds)
                {
                    TableCell tc1 = new TableCell();
                    tr1.Append(tc1);

                    var width = td.GetAttribute("width");
                    if (string.IsNullOrEmpty(width) == false)
                    {
                        // Specify the width property of the table cell.
                        // Dxa:  Width in Twentieths of a Point. 二十分之一点
                        tc1.Append(new TableCellProperties(
                            GetTableCellWidth(width)));
                    }

                    // 检查 td 下级是否有 p 元素
                    var p_nodes = td.ChildNodes.Cast<XmlNode>()
                        .Where(n => n.NodeType == XmlNodeType.Element && n.Name == "p")
                        .ToList();
                    Paragraph new_paragraph = null;
                    if (p_nodes.Count == 0)
                    {
                        new_paragraph = new Paragraph();
                        tc1.AppendChild(new_paragraph);
                    }

                    CreateTextStream(doc,
                        new_paragraph == null ? (OpenXmlElement)tc1 : new_paragraph,
                        td.ChildNodes);
                    /*
                    CreateNodes(doc,
tc1,
td.ChildNodes);
                    */

                    // TableCell tc1 = new TableCell(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(td.InnerText))));
                }
            }

            return tbl;
        }

        static TableWidth GetTableWidth(string text)
        {
            var ref_obj = GetTableCellWidth(text);
            return new TableWidth
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
        }

        static TableCellWidth GetTableCellWidth(string text)
        {
            if (string.IsNullOrEmpty(text)
                || text == "auto")
                return new TableCellWidth { Type = TableWidthUnitValues.Auto };

            int nRet = StringUtil.ParseUnit(text,
out string value,
out string unit,
out string error);
            if (nRet == -1)
                throw new Exception(error);

            if (string.IsNullOrEmpty(unit) || unit == "pt")
            {
                var v = (int)(Convert.ToDouble(value) * 20D);
                return new TableCellWidth
                {
                    Type = TableWidthUnitValues.Dxa,
                    Width = v.ToString()
                };
            }

            if (unit == "dxa")
            {
                return new TableCellWidth
                {
                    Type = TableWidthUnitValues.Dxa,
                    Width = value
                };
            }

            if (unit == "%")
            {
                var v = (int)(Convert.ToDouble(value) * 50D);
                return new TableCellWidth
                {
                    Type = TableWidthUnitValues.Pct,
                    Width = v.ToString()
                };
            }

            throw new Exception($"未知的单位 '{unit}' ('{text}')");
        }

        static string GetTwentieths(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            int nRet = StringUtil.ParseUnit(text,
out string value,
out string unit,
out string error);
            if (nRet == -1)
                throw new Exception(error);

            if (string.IsNullOrEmpty(unit) || unit == "pt")
            {
                var v = (int)(Convert.ToDouble(value) * 20D);
                return v.ToString();
            }

            if (unit == "dxa")
            {
                return value;
            }

            throw new Exception($"未知的单位 '{unit}' ('{text}')");
        }


        static FontSize GetFontSize(string text)
        {
            int nRet = StringUtil.ParseUnit(text,
out string value,
out string unit,
out string error);
            if (nRet == -1)
                throw new Exception(error);

            if (string.IsNullOrEmpty(unit) || unit == "pt")
            {
                var v = (int)(Convert.ToDouble(value) * 2D);
                return new FontSize
                {
                    Val = v.ToString()
                };
            }

            if (unit == "hps")  // 二分之一点
            {
                return new FontSize
                {
                    Val = value
                };
            }

            throw new Exception($"未知的单位 '{unit}' ('{text}')");
        }

        #region Styles

        // Apply a style to a paragraph.
        public static void ApplyStyleToParagraph(WordprocessingDocument doc,
            string styleid, string stylename, Paragraph p)
        {
            // If the paragraph has no ParagraphProperties object, create one.
            if (p.Elements<ParagraphProperties>().Count() == 0)
            {
                p.PrependChild<ParagraphProperties>(new ParagraphProperties());
            }

            // Get the paragraph properties element of the paragraph.
            ParagraphProperties pPr = p.Elements<ParagraphProperties>().First();

            // Get the Styles part for this document.
            StyleDefinitionsPart part =
                doc.MainDocumentPart.StyleDefinitionsPart;

            // If the Styles part does not exist, add it and then add the style.
            if (part == null)
            {
                part = AddStylesPartToPackage(doc);
                AddNewStyle(part, styleid, stylename);
            }
            else
            {
                // If the style is not in the document, add it.
                if (IsStyleIdInDocument(doc, styleid) != true)
                {
                    // No match on styleid, so let's try style name.
                    string styleidFromName = GetStyleIdFromStyleName(doc, stylename);
                    if (styleidFromName == null)
                    {
                        AddNewStyle(part, styleid, stylename);
                    }
                    else
                        styleid = styleidFromName;
                }
            }

            // Set the style of the paragraph.
            pPr.ParagraphStyleId = new ParagraphStyleId() { Val = styleid };
        }

        // Return true if the style id is in the document, false otherwise.
        public static bool IsStyleIdInDocument(WordprocessingDocument doc,
            string styleid)
        {
            // Get access to the Styles element for this document.
            Styles s = doc.MainDocumentPart.StyleDefinitionsPart.Styles;

            // Check that there are styles and how many.
            int n = s.Elements<Style>().Count();
            if (n == 0)
                return false;

            // Look for a match on styleid.
            Style style = s.Elements<Style>()
                .Where(st => (st.StyleId == styleid) && (st.Type == StyleValues.Paragraph))
                .FirstOrDefault();
            if (style == null)
                return false;

            return true;
        }

        // Return styleid that matches the styleName, or null when there's no match.
        public static string GetStyleIdFromStyleName(WordprocessingDocument doc, string styleName)
        {
            StyleDefinitionsPart stylePart = doc.MainDocumentPart.StyleDefinitionsPart;
            string styleId = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) &&
                    (((Style)s.Parent).Type == StyleValues.Paragraph))
                .Select(n => ((Style)n.Parent).StyleId).FirstOrDefault();
            return styleId;
        }

        // Create a new style with the specified styleid and stylename and add it to the specified
        // style definitions part.
        private static void AddNewStyle(StyleDefinitionsPart styleDefinitionsPart,
            string styleid, string stylename)
        {
            // Get access to the root element of the styles part.
            Styles styles = styleDefinitionsPart.Styles;

            // Create a new paragraph style and specify some of the properties.
            Style style = new Style()
            {
                Type = StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            StyleName styleName1 = new StyleName() { Val = stylename };
            BasedOn basedOn1 = new BasedOn() { Val = "Normal" };
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = "Normal" };
            style.Append(styleName1);
            style.Append(basedOn1);
            style.Append(nextParagraphStyle1);

            // Create the StyleRunProperties object and specify some of the run properties.
            StyleRunProperties styleRunProperties1 = new StyleRunProperties();
            Bold bold1 = new Bold();
            Color color1 = new Color() { ThemeColor = ThemeColorValues.Accent2 };
            RunFonts font1 = new RunFonts() { Ascii = "Lucida Console" };
            Italic italic1 = new Italic();
            // Specify a 12 point size.
            FontSize fontSize1 = new FontSize() { Val = "24" };
            styleRunProperties1.Append(bold1);
            styleRunProperties1.Append(color1);
            styleRunProperties1.Append(font1);
            styleRunProperties1.Append(fontSize1);
            styleRunProperties1.Append(italic1);

            // Add the run properties to the style.
            style.Append(styleRunProperties1);

            // Add the style to the styles part.
            styles.Append(style);
        }

        // Add a StylesDefinitionsPart to the document.  Returns a reference to it.
        public static StyleDefinitionsPart AddStylesPartToPackage(WordprocessingDocument doc)
        {
            StyleDefinitionsPart part;
            part = doc.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            Styles root = new Styles();
            root.Save(part);
            return part;
        }

        #endregion
    }
}
