using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.IO;

using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System.Net.Http;
// using SixLabors.ImageSharp;

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
            // 检查 wordFileName 是否可写
            if (File.Exists(wordFileName))
            {
                try
                {
                    using (var file = File.Open(wordFileName, FileMode.Open, FileAccess.Write))
                    {

                    }
                }
                catch (IOException ex)
                {
                    throw new Exception($"文件 {wordFileName} 已经被占用。请关闭 Word 再重试一次");
                }
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(xmlFileName);

            // 2023/7/25
            AdjustTable(dom);

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

                var settings_nodes = dom.DocumentElement.SelectSingleNode("settings") as XmlElement;
                if (settings_nodes != null)
                    CreateSettings(doc, settings_nodes);
            }
        }

        // https://github.com/jgm/pandoc/issues/6983
        // 这是为了避免 word docx 的一个 bug。如果(td 下的 table 后面兄弟)这里没有 p，Word 打开 docx 时会说文件被破坏

        static void AdjustTable(XmlDocument dom)
        {
            var nodes = dom.DocumentElement.SelectNodes("//table");
            foreach (XmlElement table in nodes)
            {
                if (IsParentTd(table) == false)
                    continue;
                // 判断 table 的兄弟中是否已经有 p 了
                if (HasSiblingP(table) == false)
                {
                    table.ParentNode.InsertAfter(dom.CreateElement("p"), table);
                }
            }

            // 判断上级元素是否为 td 元素
            bool IsParentTd(XmlElement current)
            {
                current = current.ParentNode as XmlElement;

                while (current != null)
                {
                    if (current.Name == "table" || current.Name == "tr")
                        return false;   // 已经到上级 table 了，从此不必继续判断了
                    if (current.Name == "td")
                        return true;
                    current = current.ParentNode as XmlElement;
                }
                return false;
            }

            bool HasSiblingP(XmlElement current)
            {
                foreach (XmlElement sibling in current.ParentNode.ChildNodes)
                {
                    if (sibling == null)
                        continue;
                    if (sibling.Name == "p")
                        return true;
                }
                return false;
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

        static void CreateSettings(WordprocessingDocument doc,
            XmlElement settings_node)
        {
            SectionProperties sectPr = EnsureSectionProperty(doc);

            string pageNumberStart_attr = settings_node.GetAttribute("pageNumberStart");
            if (string.IsNullOrEmpty(pageNumberStart_attr) == false)
            {
                var start = sectPr.AppendChild<PageNumberType>(new PageNumberType());
                if (Int32.TryParse(pageNumberStart_attr, out int value) == false)
                    throw new Exception($"属性 pageNumberStart 值 '{pageNumberStart_attr}' 不合法。应为一个整数");
                start.Start = value;
            }
        }

        static void CreateColumns(WordprocessingDocument doc,
            XmlElement columns_node)
        {
            // 创建全局 sectPr
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

                    var text = new DocumentFormat.OpenXml.Wordprocessing.Text(node.Value);
                    // text.Space = SpaceProcessingModeValues.Preserve;
                    p.AppendChild(new Run(text));

                    // body.AppendChild(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(node.Value))));
                    // TableCell tc1 = new TableCell(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(td.InnerText))));
                    continue;
                }

                // 2023/7/12
                if (node.Name == "style")
                {
                    // 2023/7/13
                    CreateParagraphIfNeed();

                    EnsureStyles(doc);
                    var new_style = CreateStyle(doc,
                        node as XmlElement,
                        null,
                        StyleValues.Character);
                    if (new_style != null)
                        CreateTextStream(doc,
                            p,
                            node.ChildNodes,
                            new_style);
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

            OpenXmlElement CreateParagraphIfNeed()
            {
                if (p == null)
                {
                    p = new Paragraph();
                    body.AppendChild(p);
                }

                return p;
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
                // part.Styles.Append(CreateStyle(doc, style_node, null, StyleValues.Paragraph));
                CreateStyle(doc, style_node, null, StyleValues.Paragraph);
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
            EnsureStyles(doc);  // 2023/7/18

            var styletype = style_node.GetAttribute("type");
            if (styletype == "td")
                return null;    // td 类型的 style 元素，并不对应 OpenXml 的任何结构

            var use_attr = style_node.GetAttribute("use");
            if (string.IsNullOrEmpty(use_attr) == false)
            {
                // 注意用了 use 属性后，就不允许再有其它属性了
                if (style_node.Attributes.Count > 1)
                    throw new Exception($"使用了 use 属性时，style 元素就不应出现其它属性: {style_node.OuterXml}");
                var ref_style_node = style_node.SelectSingleNode($"//style[@name='{use_attr}']") as XmlElement;
                if (ref_style_node == null)
                    throw new Exception($"use 属性引用的名为 '{use_attr}' 的 style 元素没有找到");
                var exist_style = GetStyleFromStyleName(doc,
    use_attr,
    default_type);
                if (exist_style == null)
                    throw new Exception($"use 属性引用的名为 '{use_attr}' 的(类型为 {default_type.ToString()}) style 对象在 Word 文档中没有找到");
                return exist_style;
            }

            StyleValues type = default_type;
            var stylename = style_node.GetAttribute("name");
            if (string.IsNullOrEmpty(stylename))
                stylename = NewStyleName();

            if (string.IsNullOrEmpty(styletype) == false)
            {
                type = GetStyleType(styletype);
            }

            // Parent Style ID
            // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.style.basedon?view=openxml-2.8.1#documentformat-openxml-wordprocessing-style-basedon
            var baseOnName = style_node.GetAttribute("baseOn");

            string baseOn_style_id = null;
            // baseOn 属性优先于 base_style 参数
            if (string.IsNullOrEmpty(baseOnName) == false)
                baseOn_style_id = GetStyleIdFromStyleName(doc, baseOnName, type);
            else if (base_style != null)
                baseOn_style_id = base_style.StyleId;

            var styleid = NewStyleID();

            Style style = new Style()
            {
                Type = type,    // StyleValues.Paragraph,
                StyleId = styleid,
                CustomStyle = true
            };
            StyleName styleName1 = new StyleName() { Val = stylename };
            style.Append(styleName1);
            if (string.IsNullOrEmpty(baseOn_style_id) == false)
            {
                BasedOn basedOn1 = new BasedOn() { Val = baseOn_style_id };
                style.Append(basedOn1);
            }
            NextParagraphStyle nextParagraphStyle1 = new NextParagraphStyle() { Val = baseOn_style_id };
            style.Append(nextParagraphStyle1);

            // 2023/7/18
            if (type == StyleValues.Paragraph)
            {
                SetParagraphAttributes(style_node, () =>
                {
                    var pProperties = new ParagraphProperties();
                    style.Append(pProperties);
                    return pProperties;
                });

#if REMOVED
                ParagraphProperties pProperties = new ParagraphProperties();
                style.Append(pProperties);

                string alignment = style_node.GetAttribute("alignment");
                if (string.IsNullOrEmpty(alignment) == false)
                {
                    if (Enum.TryParse<JustificationValues>(alignment, true, out JustificationValues value) == false)
                    {
                        throw new Exception($"alignment 属性值 '{alignment}' 不合法");
                    }
                    Justification objJustification =
        new Justification() { Val = value };
                    pProperties.Append(objJustification);
                }
#endif
            }

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
                if (StringUtil.IsInList("strike", style_attr))
                {
                    styleRunProperties1.Append(new Strike { Val = true });
                }
            }

            /*
            // Color color1 = new Color() { ThemeColor = ThemeColorValues.Accent2 };
            Color color1 = new Color() { Val = "0000FF" };
            styleRunProperties1.Append(color1);
            */
            // 2023/7/15
            var color = style_node.GetAttribute("color");
            if (string.IsNullOrEmpty(color) == false)
            {
                Color color1 = new Color() { Val = color };
                styleRunProperties1.Append(color1);
            }

            var size = style_node.GetAttribute("size");
            if (string.IsNullOrEmpty(size) == false)
            {
                // FontSize::Val: Half Point Measurement 二分之一点
                styleRunProperties1.Append(GetFontSize(size));
            }

#if REMOVED
            // http://officeopenxml.com/WPtableProperties.php
            var table_property_node = style_node.SelectSingleNode("tableProperties") as XmlElement;
            if (table_property_node != null)
            {
                var table_properties = style.AppendChild<StyleTableProperties>(new StyleTableProperties());

                var may_break_between_pages_attr = table_property_node.GetAttribute("mayBreakBetweenPages");
                // table_properties.break
            }
#endif

            doc.MainDocumentPart.StyleDefinitionsPart.Styles.Append(style);
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

            ParagraphProperties EnsureProperty()
            {
                if (p.Elements<ParagraphProperties>().Count() == 0)
                {
                    p.PrependChild<ParagraphProperties>(new ParagraphProperties());
                }

                return p.Elements<ParagraphProperties>().First();
            }

            // p 元素的 style 属性
            string style_name = paragraph.GetAttribute("style");
            if (string.IsNullOrEmpty(style_name) == false)
            {
                /*
                if (p.Elements<ParagraphProperties>().Count() == 0)
                {
                    p.PrependChild<ParagraphProperties>(new ParagraphProperties());
                }

                ParagraphProperties pPr = p.Elements<ParagraphProperties>().First();
                */
                var pPr = EnsureProperty();

                // 使用本元素内嵌的 style 元素
                if (style_name == ".")
                {
                    // TODO: 找到内嵌的特定 type 的第一个 style 元素
                    var style_element = paragraph.SelectSingleNode(".//style") as XmlElement;
                    var new_style = CreateStyle(doc, style_element, null, StyleValues.Paragraph);
                    if (new_style == null)
                        throw new Exception($"创建 style 相关元素失败，可能是 style 元素的 type 属性不正确");
                    pPr.ParagraphStyleId = new ParagraphStyleId() { Val = new_style.StyleId };
                }
                else
                {
                    // style name 找到 style id
                    var style_id = GetStyleIdFromStyleName(doc, style_name, StyleValues.Paragraph);
                    if (style_id == null)
                    {
                        throw new Exception($"Style Name '{style_name}' not found");
                    }
                    pPr.ParagraphStyleId = new ParagraphStyleId() { Val = style_id };
                }
            }

            {
                SetParagraphAttributes(paragraph, () => EnsureProperty());
            }
#if REMOVED
            string alignment = paragraph.GetAttribute("alignment");
            if (string.IsNullOrEmpty(alignment) == false)
            {
                var pPr = EnsureProperty();

                if (Enum.TryParse<JustificationValues>(alignment, true, out JustificationValues value) == false)
                {
                    throw new Exception($"alignment 属性值 '{alignment}' 不合法");
                }
                Justification objJustification =
    new Justification() { Val = value };
                pPr.Append(objJustification);
            }

            string spacing = paragraph.GetAttribute("spacing");
            if (string.IsNullOrEmpty(spacing) == false)
            {
                var pPr = EnsureProperty();
                SetParagraphSpacing(pPr, spacing);
            }

            // 2022/8/25
            string pageBreakBefore = paragraph.GetAttribute("pageBreakBefore");
            if (string.IsNullOrEmpty(pageBreakBefore) == false)
            {
                var pPr = EnsureProperty();
                var before = pPr.AppendChild<PageBreakBefore>(new PageBreakBefore());
                before.Val = DomUtil.IsBooleanTrue(pageBreakBefore);
            }
#endif

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

        delegate ParagraphProperties delegate_ensureProperty();

        // 给 ParagraphProperties 设置 p 元素或 style 元素的一些属性。这些属性是和 paragraph 有关的
        // parameters:
        //      func_ensure 按需确保创建 ParagraphProperties
        static void SetParagraphAttributes(XmlElement paragraph,
            delegate_ensureProperty func_ensure)
        {
            string alignment = paragraph.GetAttribute("alignment");
            if (string.IsNullOrEmpty(alignment) == false)
            {
                var pPr = func_ensure();

                if (Enum.TryParse<JustificationValues>(alignment, true, out JustificationValues value) == false)
                {
                    throw new Exception($"alignment 属性值 '{alignment}' 不合法");
                }
                Justification objJustification =
    new Justification() { Val = value };
                pPr.Append(objJustification);
            }

            string spacing = paragraph.GetAttribute("spacing");
            if (string.IsNullOrEmpty(spacing) == false)
            {
                var pPr = func_ensure();
                SetParagraphSpacing(pPr, spacing);
            }

            // 2022/8/25
            string pageBreakBefore = paragraph.GetAttribute("pageBreakBefore");
            if (string.IsNullOrEmpty(pageBreakBefore) == false)
            {
                var pPr = func_ensure();
                var before = pPr.AppendChild<PageBreakBefore>(new PageBreakBefore());
                before.Val = DomUtil.IsBooleanTrue(pageBreakBefore);
            }

            // http://www.officeopenxml.com/WPsectionBorders.php
            /*
<border>  
  <top value="single" size="2" space="1" w:color="FF0000" />   
  <left value="single" size="2" space="4" w:color="FF0000" />   
  <bottom value="single" size="2" space="1" w:color="FF0000" />   
  <right value="single" size="2" space="4" w:color="FF0000" />   
  <between value="single" size="4" space="1" w:color="4D5D2C" />   
</border>   
             * */
            var border = paragraph.SelectSingleNode("border") as XmlElement;
            if (border != null)
            {
                var pPr = func_ensure();
                var b = pPr.AppendChild<ParagraphBorders>(new ParagraphBorders());
                CreateBorderChildren(border, b);
            }
        }

        static void CreateBorderChildren(XmlElement border_node,
            OpenXmlCompositeElement b)
        {
            // top
            {
                var top_node = border_node.SelectSingleNode("top") as XmlElement;
                if (top_node != null)
                {
                    var top = b.AppendChild<TopBorder>(new TopBorder());
                    GetBorderAttributes(top_node, top);
                }
            }

            // left
            {
                var top_node = border_node.SelectSingleNode("left") as XmlElement;
                if (top_node != null)
                {
                    var top = b.AppendChild<LeftBorder>(new LeftBorder());
                    GetBorderAttributes(top_node, top);
                }
            }

            // bottom
            {
                var top_node = border_node.SelectSingleNode("bottom") as XmlElement;
                if (top_node != null)
                {
                    var top = b.AppendChild<BottomBorder>(new BottomBorder());
                    GetBorderAttributes(top_node, top);
                }
            }

            // right
            {
                var top_node = border_node.SelectSingleNode("right") as XmlElement;
                if (top_node != null)
                {
                    var top = b.AppendChild<RightBorder>(new RightBorder());
                    GetBorderAttributes(top_node, top);
                }
            }

            // between
            {
                var top_node = border_node.SelectSingleNode("between") as XmlElement;
                if (top_node != null)
                {
                    var top = b.AppendChild<BetweenBorder>(new BetweenBorder());
                    GetBorderAttributes(top_node, top);
                }
            }
        }

        static void GetBorderAttributes(XmlElement top_node,
            BorderType top)
        {
            string s = top_node.GetAttribute("value");
            if (string.IsNullOrEmpty(s) == false)
            {
                if (Enum.TryParse<BorderValues>(s, true, out BorderValues v) == false)
                    throw new ArgumentException($"'{s}' 不是合法的 BorderValues 枚举值");
                top.Val = v;
            }

            s = top_node.GetAttribute("size");
            // top.Size 单位是 1/8 point
            if (string.IsNullOrEmpty(s) == false)
            {
                s = GetEights(s);
                top.Size = UInt32Value.FromUInt32(Convert.ToUInt32(s));
            }

            string GetEights(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                int nRet = ParseUnit(text,
    out string value,
    out string unit,
    out string error);
                if (nRet == -1)
                    throw new Exception(error);

                if (string.IsNullOrEmpty(unit) || unit == "pt")
                {
                    var v = (int)(Convert.ToDouble(value) * 8D);
                    return v.ToString();
                }

                // 2022/10/11
                if (unit == "cm")
                {
                    var v = (int)((Convert.ToDouble(value) * 72D * 20D) / 2.54D);
                    return v.ToString();
                }

                if (unit == "ets")
                {
                    return value;
                }

                throw new Exception($"border/@size 属性值 '{text}' 中出现未知的单位 '{unit}'");
            }

            // top.Space 单位是 pt
            s = top_node.GetAttribute("space");
            if (string.IsNullOrEmpty(s) == false)
            {
                s = GetPoint(s);
                top.Space = UInt32Value.FromUInt32(Convert.ToUInt32(s));
            }

            string GetPoint(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                int nRet = ParseUnit(text,
    out string value,
    out string unit,
    out string error);
                if (nRet == -1)
                    throw new Exception(error);

                if (string.IsNullOrEmpty(unit) || unit == "pt")
                {
                    var v = (int)(Convert.ToDouble(value) * 1D);
                    return v.ToString();
                }

                if (unit == "cm")
                {
                    var v = (int)((Convert.ToDouble(value) * 72D * 1D) / 2.54D);
                    return v.ToString();
                }

                if (unit == "hps")  // 二分之一点
                {
                    return value;
                }

                throw new Exception($"space 属性值 {text} 中出现未知的单位 '{unit}'");
            }


            s = top_node.GetAttribute("color");
            if (string.IsNullOrEmpty(s) == false)
                top.Color = s;
        }

        static void SetParagraphSpacing(ParagraphProperties pPr, string text)
        {
            /*
<w:pPr>
<w:spacing w:before="360" w:after="120" w:line="480" w:lineRule="auto" w:beforeAutospacing="0" w:afterAutospacing="0"/>
</w:pPr>
            */
            /*
After
AfterAutoSpacing
AfterLines
Before
BeforeAutoSpacing
BeforeLines
Line
LineRule
            * */
            // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.spacingbetweenlines?view=openxml-2.8.1
            var spacing = pPr.AppendChild<SpacingBetweenLines>(new SpacingBetweenLines());

            {
                // Specifies the spacing (in absolute units) that should be added after the last line of the paragraph.
                var after = StringUtil.GetParameterByPrefix(text, "after");
                if (string.IsNullOrEmpty(after) == false)
                    spacing.After = GetTwentieths(after);
            }

            {
                // Specifies the spacing (in absolute units) that should be added before the first line of the paragraph.
                var before = StringUtil.GetParameterByPrefix(text, "before");
                if (string.IsNullOrEmpty(before) == false)
                    spacing.Before = GetTwentieths(before);
            }

            {
                // Specifies the amount of vertical spacing between lines of text within the paragraph.
                // Note: If the value of the lineRule attribute is atLeast or exactly, then the value of the lineRule attribute is interpreted as 240th of a point. If the value of line is auto, then the value of line is interpreted as 240th of a line.
                var line = StringUtil.GetParameterByPrefix(text, "line");
                if (string.IsNullOrEmpty(line) == false)
                {
                    var result = ParseLineParameter(line);
                    spacing.Line = result.Line;
                    spacing.LineRule = result.LineRule;
                }
            }

            {
                var afterAutoSpacing = StringUtil.GetParameterByPrefix(text, "afterAutoSpacing");
                if (string.IsNullOrEmpty(afterAutoSpacing) == false)
                    spacing.AfterAutoSpacing = DomUtil.IsBooleanTrue(afterAutoSpacing);
            }

            {
                var beforeAutoSpacing = StringUtil.GetParameterByPrefix(text, "beforeAutoSpacing");
                if (string.IsNullOrEmpty(beforeAutoSpacing) == false)
                    spacing.BeforeAutoSpacing = DomUtil.IsBooleanTrue(beforeAutoSpacing);
            }

            {
                var afterLines = StringUtil.GetParameterByPrefix(text, "afterLines");
                if (string.IsNullOrEmpty(afterLines) == false)
                    spacing.AfterLines = GetBeforeLines(afterLines);    // spacing.AfterLines: The value of this attribute is specified in one hundredths of a line.
            }

            {
                var beforeLines = StringUtil.GetParameterByPrefix(text, "beforeLines");
                if (string.IsNullOrEmpty(beforeLines) == false)
                    spacing.BeforeLines = GetBeforeLines(beforeLines);
            }
        }

        static int GetBeforeLines(string text)
        {
            return (int)(Convert.ToDouble(text) * 100D);
        }

        // 1.5unit
        public static int ParseUnit(string strText,
out string strValue,
out string strUnit,
out string strError)
        {
            strValue = "";
            strUnit = "";
            strError = "";

            if (String.IsNullOrEmpty(strText) == true)
            {
                strError = "strText 值不应为空";
                return -1;
            }

            strText = strText.Trim();

            if (String.IsNullOrEmpty(strText) == true)
            {
                strError = "strText 值除去两端空格后不应为空";
                return -1;
            }

            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (char ch in strText)
            {
                if ((ch >= '0' && ch <= '9') || ch == '.')
                {
                    text.Append(ch);
                }
                else
                {
                    strUnit = strText.Substring(i).Trim();
                    break;
                }
                i++;
            }

            strValue = text.ToString();
            return 0;
        }


        class LineValue
        {
            public string Line { get; set; }
            public LineSpacingRuleValues LineRule { get; set; }
        }
        static LineValue ParseLineParameter(string text)
        {
            int nRet = ParseUnit(text,
out string value,
out string unit,
out string error);
            if (nRet == -1)
                throw new Exception(error);

            if (string.IsNullOrEmpty(unit) || unit == "auto")
            {
                // then the value of the line attribute shall be interpreted as 240ths of a line
                var v = (int)(Convert.ToDouble(value) * 240D);
                return new LineValue
                {
                    Line = v.ToString(),
                    LineRule = LineSpacingRuleValues.Auto,
                };
            }

            if (unit == "atLeast" || unit == "exact")
            {
                // TODO: value 内容中应该允许带 pt 等单位

                // If the value of this attribute is either atLeast or exactly, then the value of the line attribute shall be interpreted as twentieths of a point
                var v = (int)(Convert.ToDouble(value) * 20D);

                if (Enum.TryParse<LineSpacingRuleValues>(unit, true, out LineSpacingRuleValues rule) == false)
                {
                    throw new Exception($"line 属性值中的单位 '{unit}' 不合法");
                }
                return new LineValue
                {
                    Line = v.ToString(),
                    LineRule = rule,
                };
            }

            throw new Exception($"未知的单位 '{unit}' ('{text}')");
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

                // 2023/7/12
                if (child_node.Name == "table")
                {
                    /*
                    CreateTable(doc,
                        p == null ? doc.MainDocumentPart.Document.Body : p,
                        child_node as XmlElement);
                    continue;
                    */
                    CreateTable(doc,
                        p == null ? doc.MainDocumentPart.Document.Body : p,
                        child_node as XmlElement);
                    // temp_paragraph = null;  // 打断
                    continue;
                }

                if (child_node.Name == "img")
                {
                    XmlElement e = child_node as XmlElement;
                    string src = e.GetAttribute("src");

                    HttpClient client = new HttpClient();
                    var response = client.GetAsync(src).Result;

                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    int width = 0;
                    int height = 0;
                    ImagePart imagePart = null;
                    using (var stream = response.Content.ReadAsStreamAsync().Result)
                    {
                        //    Stream theStream = client.GetStreamAsync(src).Result;

                        /*
                        var ext = Path.GetExtension(imageFileName);
                        ImagePartType type = ImagePartType.Jpeg;
                        Enum.TryParse<ImagePartType>(ext, true, out type);
                        */
                        var type = GetImageType(contentType);
                        imagePart = doc.MainDocumentPart.AddImagePart(type);

                        using (var image = SixLabors.ImageSharp.Image.Load(stream))
                        {
                            width = image.Width;
                            height = image.Height;
                        }
                    }

                    response = client.GetAsync(src).Result;
                    using (var stream = response.Content.ReadAsStreamAsync().Result)
                    {
                        imagePart.FeedData(stream);
                    }
                    var relationshipId = doc.MainDocumentPart.GetIdOfPart(imagePart);
                    var d = NewDrawing(relationshipId, width, height);
                    Run run = CreateParagraphIfNeed().AppendChild(new Run(d));
                    continue;
                }

                if (child_node.Name == "br")
                {
                    Run run = CreateParagraphIfNeed().AppendChild(new Run());
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.break?view=openxml-2.8.1
                    run.AppendChild(new Break());
                    continue;
                }

                // 一个或者若干个空格字符
                if (child_node.Name == "blk")
                {
                    // TODO: 增加 count 属性
                    var text = new DocumentFormat.OpenXml.Wordprocessing.Text(" ");
                    text.Space = SpaceProcessingModeValues.Preserve;
                    CreateParagraphIfNeed().AppendChild(new Run(text));
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

                /*
                // TODO: 可以考虑和附近的一个 Run 合并?
                if (child_node.Name == "pageNumber")
                {
                    Run run = CreateParagraphIfNeed().AppendChild(new Run());
                    // run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(child_node.Value));

                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.pagenumber?view=openxml-2.8.1
                    run.AppendChild<PageNumber>(new PageNumber());
                    continue;
                }
                */

                if (child_node.Name == "pageNumber")
                {
                    /*
<w:txbxContent><w:p><w:pPr><w:pStyle w:val="2"/></w:pPr><w:r><w:fldChar w:fldCharType="begin"/></w:r><w:r><w:instrText xml:space="preserve"> PAGE  \* MERGEFORMAT </w:instrText></w:r><w:r><w:fldChar w:fldCharType="separate"/></w:r><w:r><w:t>1</w:t></w:r><w:r><w:fldChar w:fldCharType="end"/></w:r></w:p></w:txbxContent>
                    * */
                    SimpleField simpleField1 = new SimpleField()
                    {
                        Instruction = " PAGE   \\* MERGEFORMAT "
                    };
                    CreateParagraphIfNeed().AppendChild(simpleField1);
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
                    /*
                    StyleDefinitionsPart part =
            doc.MainDocumentPart.StyleDefinitionsPart;
                    part.Styles.Append(new_style);
                    */
                    if (new_style != null)
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
            if (table.ParentNode.Name == "p")
                throw new ArgumentException($"p 元素下级不允许使用 table 元素");

            // Create a table.
            Table tbl = new Table();
            body.AppendChild(tbl);

            // Set the style and width for the table.
            TableProperties tableProp = new TableProperties();
            tbl.AppendChild(tableProp);

            // table/@cellMarginDefault 左 上 右 下
            var cellMargin_attr = table.GetAttribute("cellMarginDefault");
            if (string.IsNullOrEmpty(cellMargin_attr) == false)
            {
                string left = "", top = "", right = "", bottom = "";
                var parts = StringUtil.SplitList(cellMargin_attr);
                if (parts.Count > 0)
                    left = parts[0];
                if (parts.Count > 1)
                    top = parts[1];
                if (parts.Count > 2)
                    right = parts[2];
                if (parts.Count > 3)
                    bottom = parts[3];
                var cell_margin = tableProp.AppendChild<TableCellMarginDefault>(new TableCellMarginDefault());
                cell_margin.StartMargin = GetStartMargin(left);
                cell_margin.TopMargin = GetTopMargin(top);
                cell_margin.EndMargin = GetEndMargin(right);
                cell_margin.BottomMargin = GetBottomMargin(bottom);
            }

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

            // 2022/10/11
            // table 的 layout 属性
            {
                // https://www.docx4java.org/forums/docx-java-f6/setting-column-width-of-a-table-and-fix-this-t1819.html
                var layout = table.GetAttribute("layout");
                if (string.IsNullOrEmpty(layout) == false)
                {
                    var tbl_layout = new TableLayout();
                    if (layout == "fixed")
                        tbl_layout.Type = TableLayoutValues.Fixed;
                    else if (layout == "autofit")
                        tbl_layout.Type = TableLayoutValues.Autofit;
                    else
                        throw new Exception($"table/@layout 属性值 '{layout}' 不合法。应为 'fixed' 'autofit' 之一");

                    tableProp.Append(tbl_layout);
                }
            }

            /*
<w:tblGrid>  
  <w:gridCol w:w="6888"/>  
  <w:gridCol w:w="248"/>  
  <w:gridCol w:w="886"/>  
  <w:gridCol w:w="1554"/>  
</w:tblGrid>  
            * */
            var gridColumns = table.SelectNodes("tableGrid[1]/gridColumn");
            if (gridColumns.Count > 0)
            {
                var tg = tbl.AppendChild(new TableGrid());
                foreach (XmlElement gridColumn in gridColumns)
                {
                    var column = new GridColumn();
                    // GridColumn:
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.gridcolumn?view=openxml-2.8.1
                    tg.AppendChild(column);

                    var width = gridColumn.GetAttribute("gridWidth");
                    if (string.IsNullOrEmpty(width) == false)
                        column.Width = GetTwentieths(width);
                }
            }
            else
            {
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

                        var width = header.GetAttribute("gridWidth");
                        if (string.IsNullOrEmpty(width) == false)
                            column.Width = GetTwentieths(width);

                        string GetTwentieths(string text)
                        {
                            if (string.IsNullOrEmpty(text))
                                return text;

                            int nRet = ParseUnit(text,
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

                            // 2022/10/11
                            if (unit == "cm")
                            {
                                var v = (int)((Convert.ToDouble(value) * 72D * 20D) / 2.54D);
                                return v.ToString();
                            }

                            if (unit == "dxa")
                            {
                                return value;
                            }

                            throw new Exception($"{header.Name}/@gridWidth 属性值 '{text}' 中出现未知的单位 '{unit}'");
                        }
                    }
                }
            }

            var trs = table.SelectNodes("tr");
            foreach (XmlElement tr in trs)
            {
                TableRow tr1 = new TableRow();
                tbl.AppendChild(tr1);

                var cantSplit_attr = tr.GetAttribute("cantSplit");
                if (string.IsNullOrEmpty(cantSplit_attr) == false
                    && DomUtil.IsBooleanTrue(cantSplit_attr))
                {
                    // https://docs.microsoft.com/en-us/dotnet/api/documentformat.openxml.wordprocessing.cantsplit?view=openxml-2.8.1
                    var trPr = tr1.AppendChild<TableRowProperties>(new TableRowProperties());
                    trPr.AppendChild<CantSplit>(new CantSplit());
                }

                var tds = tr.SelectNodes("*[name()='th' or name()='td']");
                foreach (XmlElement td in tds)
                {
                    TableCell tc1 = new TableCell();
                    tr1.Append(tc1);

                    TableCellProperties EnsureProperty()
                    {
                        if (tc1.Elements<TableCellProperties>().Count() == 0)
                        {
                            tc1.PrependChild<TableCellProperties>(new TableCellProperties());
                        }

                        return tc1.Elements<TableCellProperties>().First();
                    }

                    SetTdAttributes(td, () => EnsureProperty());
#if REMOVED
                    var width = td.GetAttribute("width");
                    if (string.IsNullOrEmpty(width) == false)
                    {
                        // Specify the width property of the table cell.
                        // Dxa:  Width in Twentieths of a Point. 二十分之一点
                        /*
                        tc1.Append(new TableCellProperties(
                            GetTableCellWidth(width)));
                        */
                        var prop = EnsureProperty();
                        prop.AppendChild(GetTableCellWidth(width));
                    }

                    // 2022/10/11
                    var style = td.GetAttribute("style");
                    if (StringUtil.IsInList("noWrap", style))
                    {
                        var prop = EnsureProperty();
                        prop.AppendChild(new NoWrap());
                    }
#endif

                    // 检查 td 下级是否有 p 元素
                    var p_nodes = td.ChildNodes.Cast<XmlNode>()
                        .Where(n => n.NodeType == XmlNodeType.Element && n.Name == "p")
                        .ToList();
                    var has_table_nodes = td.ChildNodes.Cast<XmlNode>()
                        .Where(n => n.NodeType == XmlNodeType.Element && n.Name == "table")
                        .Any();
                    Paragraph new_paragraph = null;
                    if (p_nodes.Count == 0 && has_table_nodes == false)
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

        delegate TableCellProperties delegate_ensureTcProperty();

        static void SetTdAttributes(XmlElement td,
    delegate_ensureTcProperty func_ensure)
        {
            // 先探测 td 元素是否有 style 属性
            var style_name = td.GetAttribute("style");
            if (string.IsNullOrEmpty(style_name) == false)
            {
                var style_node = FindStyleNode(td, style_name, "td");
                if (style_node == null)
                    throw new Exception($"没有找到类型为 td 的 name 为 '{style_name}' 的 style 元素");
                SetCellAttributes(style_node, func_ensure);
            }

            // 然后处理 td 元素自己的各个属性
            SetCellAttributes(td, func_ensure);

            // 注: 因为 td 元素自己的属性后处理，所以效果上它会覆盖掉 style 元素中的同名属性
        }

        // 查找指定 type 类型的 style 元素。注意 style 元素的 name 属性值是可能会出现重复的
        static XmlElement FindStyleNode(XmlElement start,
            string style_name,
            string type)
        {
            if (style_name == ".")
                return start.SelectSingleNode(".//style") as XmlElement;

            var style_nodes = start.SelectNodes($"//style[@name='{style_name}']");
            foreach (XmlElement style_node in style_nodes)
            {
                string current_type = style_node.GetAttribute("type");
                if (string.IsNullOrEmpty(current_type))
                    current_type = "paragraph";
                if (type == current_type)
                    return style_node;
            }

            return null;
        }

        // parameters:
        //      node    可能是 td 元素，或者 style 元素
        static void SetCellAttributes(XmlElement node,
            delegate_ensureTcProperty func_ensure)
        {
            var width = node.GetAttribute("width");
            if (string.IsNullOrEmpty(width) == false)
            {
                // Specify the width property of the table cell.
                // Dxa:  Width in Twentieths of a Point. 二十分之一点
                /*
                tc1.Append(new TableCellProperties(
                    GetTableCellWidth(width)));
                */
                var prop = func_ensure();
                prop.AppendChild(GetTableCellWidth(width));
            }

            /*
            // 2022/10/11
            var style = td.GetAttribute("style");
            if (StringUtil.IsInList("noWrap", style))
            {
                var prop = func_ensure();
                prop.AppendChild(new NoWrap());
            }
            */
            // 2023/7/19
            if (node.HasAttribute("noWrap"))
            {
                var noWrap = node.GetAttribute("noWrap");
                if (DomUtil.IsBooleanTrue(noWrap, false) == true)
                {
                    var prop = func_ensure();
                    prop.AppendChild(new NoWrap());
                }
            }

            // 2023/7/20
            var span = node.GetAttribute("colspan");
            if (string.IsNullOrEmpty(span) == false)
            {
                var prop = func_ensure();
                if (Int32.TryParse(span, out Int32 v) == false)
                    throw new Exception($"colspan 属性值 '{span}' 不合法。应为一个整数");
                prop.AppendChild(new GridSpan() { Val = v });
            }

            // 2023/7/19
            var border = node.SelectSingleNode("border") as XmlElement;
            if (border != null)
            {
                var pPr = func_ensure();
                var b = pPr.AppendChild<TableCellBorders>(new TableCellBorders());
                CreateBorderChildren(border, b);
            }
        }

        static LeftMargin GetLeftMargin(string text)
        {
            var ref_obj = GetTableWidth(text);
            return new LeftMargin
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
        }

        static StartMargin GetStartMargin(string text)
        {
            var ref_obj = GetTableWidth(text);
            return new StartMargin
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
        }

        static TopMargin GetTopMargin(string text)
        {
            var ref_obj = GetTableWidth(text);
            return new TopMargin
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
        }

        static RightMargin GetRightMargin(string text)
        {
            var ref_obj = GetTableWidth(text);
            return new RightMargin
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
        }

        static EndMargin GetEndMargin(string text)
        {
            var ref_obj = GetTableWidth(text);
            return new EndMargin
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
        }

        static BottomMargin GetBottomMargin(string text)
        {
            var ref_obj = GetTableWidth(text);
            return new BottomMargin
            {
                Type = ref_obj.Type,
                Width = ref_obj.Width
            };
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

            int nRet = ParseUnit(text,
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

            // 2022/10/11
            if (unit == "cm")
            {
                var v = (int)((Convert.ToDouble(value) * 72D * 20D) / 2.54D);
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

            int nRet = ParseUnit(text,
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

            // 2022/10/11
            if (unit == "cm")
            {
                var v = (int)((Convert.ToDouble(value) * 72D * 20D) / 2.54D);
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
            int nRet = ParseUnit(text,
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

            // 2022/10/11
            // TODO: 注意测试
            if (unit == "cm")
            {
                var v = (int)((Convert.ToDouble(value) * 72D * 2D) / 2.54D);
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
                    string styleidFromName = GetStyleIdFromStyleName(doc, stylename, StyleValues.Paragraph);
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
        public static string GetStyleIdFromStyleName(WordprocessingDocument doc,
            string styleName,
            StyleValues style_type)
        {
            StyleDefinitionsPart stylePart = doc.MainDocumentPart.StyleDefinitionsPart;
            string styleId = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) &&
                    (((Style)s.Parent).Type == /*StyleValues.Paragraph*/style_type))
                .Select(n => ((Style)n.Parent).StyleId).FirstOrDefault();
            return styleId;
        }

        public static Style GetStyleFromStyleName(WordprocessingDocument doc,
    string styleName,
    StyleValues style_type)
        {
            StyleDefinitionsPart stylePart = doc.MainDocumentPart.StyleDefinitionsPart;
            var style = stylePart.Styles.Descendants<StyleName>()
                .Where(s => s.Val.Value.Equals(styleName) &&
                    (((Style)s.Parent).Type == /*StyleValues.Paragraph*/style_type))
                .Select(n => ((Style)n.Parent)).FirstOrDefault();
            return style;
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


        #region Image

        /*
        public static void InsertAPicture(
            MainDocumentPart mainPart,
            string imageFileName)
        {
            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using (FileStream stream = new FileStream(imageFileName, FileMode.Open))
            {
                imagePart.FeedData(stream);
            }

            AddImageToBody(mainPart, mainPart.GetIdOfPart(imagePart));
        }
        */
        static ImagePartType GetImageType(string content_type)
        {
            switch (content_type)
            {
                case "image/pjpeg":
                case "image/jpeg":
                    return ImagePartType.Jpeg;
                case "image/gif":
                    return ImagePartType.Gif;
                case "image/bmp":
                    return ImagePartType.Bmp;
                case "image/tiff":
                    return ImagePartType.Tiff;
                case "image/x-icon":
                    return ImagePartType.Icon;
                case "image/x-png":
                case "image/png":
                    return ImagePartType.Png;
                case "image/x-emf":
                    return ImagePartType.Emf;
                case "image/x-wmf":
                    return ImagePartType.Wmf;
                case "image/x-pcx":
                    return ImagePartType.Pcx;
            }

            var parts = StringUtil.ParseTwoPart(content_type, "/");
            string ext = parts[1];
            if (Enum.TryParse<ImagePartType>(ext, true, out ImagePartType type) == true)
                return type;
            if (ext.ToLower() == "jpeg" || ext.ToLower() == "jpg")
                return ImagePartType.Jpeg;
            if (ext.ToLower() == "x-png" || ext.ToLower() == "png")
                return ImagePartType.Png;
            throw new Exception($"无法识别 Content-Type '{content_type}' 的图像文件类型");
        }


#if REMOVED
        static ImagePartType GetImageType(string imageFileName)
        {
            var ext = Path.GetExtension(imageFileName).Substring(1);
            ImagePartType type = ImagePartType.Jpeg;
            if (Enum.TryParse<ImagePartType>(ext, true, out type) == true)
                return type;
            if (ext.ToLower() == "jpeg" || ext.ToLower() == "jpg")
                return ImagePartType.Jpeg;
            throw new Exception($"无法识别文件 '{imageFileName}' 的图像文件类型");
        }
#endif

        // https://learn.microsoft.com/en-us/office/open-xml/how-to-insert-a-picture-into-a-word-processing-document
        private static Drawing NewDrawing(// MainDocumentPart mainPart, 
            string relationshipId,
            int w,
            int h)
        {
            // https://riptutorial.com/openxml/example/29113/refer-to-the-image-in-the-word-document
            Int64Value width = w * 9525;
            Int64Value height = h * 9525;

            // Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = width/*990000L*/, Cy = height/*792000L*/ },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Picture 1"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
                         new A.Graphic(
                             new A.GraphicData(
                                 new PIC.Picture(
                                     new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
                                             new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = width/*990000L*/, Cy = height/*792000L*/ }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });

            return element;
            // Append the reference to body, the element should be in a Run.
            // mainPart.Document.Body.AppendChild(new Paragraph(new Run(element)));
        }

        #endregion
    }

    /*
DigitalPlatform.Typography

因安装 SixLabors.ImageSharp 连带更新:

System.Buffers.4.4.0 -> System.Buffers.4.5.1
System.Memory.4.5.0 -> System.Memory.4.5.4
System.Numerics.Vectors.4.4.0 -> System.Numerics.Vectors.4.5.0
System.Runtime.CompilerServices.Unsafe.4.5.0 -> System.Runtime.CompilerServices.Unsafe.5.0.0
    * */
}
