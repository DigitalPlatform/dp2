using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using ClosedXML.Excel;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 和输出 Excel 文件有关的实用功能
    /// </summary>
    public class ExcelUtility
    {
        public static void OutputBiblioTable(
            string strBiblioRecPath,
            string strXml,
            int nBiblioIndex,
            IXLWorksheet sheet,
            int nColIndex,
            ref int nRowIndex)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            List<IXLCell> cells = new List<IXLCell>();

            // 序号
            {
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nBiblioIndex + 1);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontSize = 20;
                cells.Add(cell);
            }

            int nNameWidth = 2;
            int nValueWidth = 4;

            int nStartRow = nRowIndex;
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("line");
            foreach (XmlElement line in nodes)
            {
                string strName = line.GetAttribute("name");
                string strValue = line.GetAttribute("value");
                string strType = line.GetAttribute("type");

                if (strName == "_coverImage")
                    continue;

                // name
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex + 1).SetValue(strName);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontColor = XLColor.DarkGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cells.Add(cell);

                    {
                        var rngData = sheet.Range(nRowIndex, nColIndex + 1, nRowIndex, nColIndex + 1 + nNameWidth - 1);
                        rngData.Merge();

                        rngData.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Hair;
                    }
                }

                // value
                {
                    {
                        var rngData = sheet.Range(nRowIndex, nColIndex + 1 + nNameWidth, nRowIndex, nColIndex + 1 + nNameWidth + nValueWidth - 1);
                        rngData.Merge();
                    }

                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex + 1 + nNameWidth).SetValue(strValue);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

#if NO
                    if (line == 0)
                    {
                        cell.Style.Font.FontName = "微软雅黑";
                        cell.Style.Font.FontSize = 20;
                        if (string.IsNullOrEmpty(strState) == false)
                        {
                            cell.Style.Font.FontColor = XLColor.White;
                            cell.Style.Fill.BackgroundColor = XLColor.DarkRed;
                        }
                    }
#endif
                    cells.Add(cell);


                }
                nRowIndex++;
            }

            // 序号的右边竖线
            {
                var rngData = sheet.Range(nStartRow, nColIndex, nRowIndex + nRowIndex - nStartRow, nColIndex);
                rngData.Merge();
                // rngData.LastColumn().Style.Border.RightBorder = XLBorderStyleValues.Hair;

                // 第一行上面的横线
                //rngData = sheet.Range(nRowIndex, 2, nRowIndex, 2 + 7 - 1);
                //rngData.FirstRow().Style.Border.TopBorder = XLBorderStyleValues.Medium;
            }
        }

        // 输出一行书目信息
        public static void OutputBiblioLine(
    string strBiblioRecPath,
    string strXml,
    // int nBiblioIndex,
    IXLWorksheet sheet,
    int nStartColIndex,     // 从 0 开始计数
    List<string> col_list,
    int nRowIndex)  // 从 0 开始计数。
        {
            string strError = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                // 2019/12/3
                strError = $"!error: 装载书目记录 XML 到 DOM 时出错: {ex.Message}";
                dom.LoadXml("<root />");
            }

            List<IXLCell> cells = new List<IXLCell>();

            int i = 0;
            foreach (string col in col_list)
            {
                string strValue = "";
                if (col == "recpath" || col.EndsWith("_recpath"))
                    strValue = strBiblioRecPath;
                else
                {
                    if (string.IsNullOrEmpty(strError) == false)
                        strValue = strError;
                    else
                        strValue = FindBiblioTableContent(dom, col);
                }

                {
                    IXLCell cell = sheet.Cell(nRowIndex + 1, nStartColIndex + (i++) + 1).SetValue(strValue);
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    if (col == "recpath" || col.EndsWith("_recpath"))
                        cell.Style.Font.FontColor = XLColor.LightGray;
                    else if (string.IsNullOrEmpty(strError) == false)
                    {
                        cell.Style.Fill.SetBackgroundColor(XLColor.DarkRed);
                        cell.Style.Font.SetFontColor(XLColor.White);
                    }
                }
            }
        }

        // 根据 type 在 Table XML 中获得一个内容值
        static string FindBiblioTableContent(XmlDocument dom, string type)
        {
            // TODO: 可以用 XPath 直接找到特定的 line 元素
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("line");
            int i = 0;
            foreach (XmlElement line in nodes)
            {
                string strName = line.GetAttribute("name");
                string strValue = line.GetAttribute("value");
                string strType = line.GetAttribute("type");

                if (strName == "_coverImage")
                    continue;

                if (strType == type)
                    return strValue;
            }

            return "";
        }

        // 输出一行实体/订购/期刊/评注信息
        // parameters:
        //      dropdown_list   下拉列表内容数组。每个元素为这样的格式：value1,value2,value3
        //                      也可以选择在此函数以外设置 cell range 的 valuelist。
        public static void OutputItemLine(
    string strRecPath,
    string strXml,
    int nBiblioIndex,
    IXLWorksheet sheet,
    int nStartColIndex,     // 从 0 开始计数
    List<string> col_list,
    List<string> dropdown_list,
    int nRowIndex,  // 从 0 开始计数。
    XLColor backColor,
    out IXLCell copyNumberCell)
        {
            copyNumberCell = null;

            //string strState = "";
            //List<IXLCell> cells = new List<IXLCell>();

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            int i = 0;
            foreach (string col in col_list)
            {
                List<string> value_list = null;
                if (dropdown_list != null)
                    value_list = StringUtil.SplitList(dropdown_list[i]);

                string strValue = "";
                if (col == "recpath" || col.EndsWith("_recpath"))
                    strValue = strRecPath;
                else
                    strValue = FindItemContent(dom, col);

                //if (col == "state" || col.EndsWith("_state"))
                //    strState = strValue;

                {
                    IXLCell cell = sheet.Cell(nRowIndex + 1, nStartColIndex + i + 1);
                    //cells.Add(cell);

                    if (col == "copyNumber" || col == "copyItems")
                    {
                        Int32 value = 0;
                        if (string.IsNullOrEmpty(strValue) == false)
                        {
                            if (Int32.TryParse(strValue, out value) == false)
                                throw new Exception("列 '" + col + "' 的值 '" + strValue + "' 应该为纯数字");
                        }
                        cell.SetValue<Int32>(value);
                    }
                    else
                        cell.SetValue(strValue);


                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //if (backColor != XLColor.NoColor)
                    //    cell.Style.Fill.BackgroundColor = backColor;
                    if (i == 0)
                    {
                        cell.Style.Border.SetLeftBorderColor(XLColor.Black);
                        cell.Style.Border.SetLeftBorder(XLBorderStyleValues.Medium);
                    }
                    if (i == col_list.Count - 1)
                    {
                        cell.Style.Border.SetRightBorderColor(XLColor.Black);
                        cell.Style.Border.SetRightBorder(XLBorderStyleValues.Medium);
                    }

                    if (value_list != null && value_list.Count > 0)
                    {
                        //Pass a string in this format: "Option1,Option2,Option3"
                        // var options = new List<string> { "Option1", "Option2", "Option3" };
                        var validOptions = $"\"{String.Join(",", value_list)}\"";
                        cell.DataValidation.List(validOptions, true);
                    }

                    if (col == "copy")
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    if ((col == "recpath" || col.EndsWith("_recpath"))
                        || (col == "state" || col.EndsWith("_state")))
                    {
                        cell.Style.Font.FontColor = XLColor.LightGray;
                    }
                    else
                        cell.Style.Protection.SetLocked(false);

                    if (col == "copyNumber")
                        copyNumberCell = cell;
                }

                i++;
            }

        }

        static string FindItemContent(XmlDocument dom, string element_name)
        {
            XmlElement element = dom.DocumentElement.SelectSingleNode(element_name) as XmlElement;

            if (element == null)
                return "";
            return element.InnerText.Trim();
        }
    }

}
