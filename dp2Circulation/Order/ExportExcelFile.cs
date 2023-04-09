using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using ClosedXML.Excel;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation.Order
{
    public class ExportExcelFile
    {
        // 左边留白栏数
        public int LeftBlankColCount { get; set; }

        private int _contentStartRow = -1;
        private int _contentEndRow = -1;
        private int _firstContentColumn = -1;

        public IXLWorksheet Sheet { get; set; }

        public List<ColumnProperty> BiblioColList { get; set; }
        public List<ColumnProperty> OrderColList { get; set; }
        public int RowIndex { get; set; }
        public List<int> ColumnMaxChars { get; set; }

        // 内容行的开始，结束行号。从 0 开始计数
        public int ContentStartRow { get => _contentStartRow; set => _contentStartRow = value; }
        public int ContentEndRow { get => _contentEndRow; set => _contentEndRow = value; }

        // 内容区域第一列的列号
        public int FirstContentColumn { get => _firstContentColumn; set => _firstContentColumn = value; }

        public virtual void Clear()
        {
            _contentStartRow = -1;
            _contentEndRow = -1;
        }

        // 向 Excel 文件输出列标题行。包括内部命令行
        public virtual void OutputDistributeInfoTitleLine(string strStyle)
        {
            var context = this;

            /*
            // 检查订购列定义中，copy 是否和 copyNumber copyItems 同时存在了
            ColumnProperty c1 = context.OrderColList.Find((o) =>
            {
                if (o.Type == "order_copy")
                    return true;
                return false;
            });

            ColumnProperty c2 = context.OrderColList.Find((o) =>
            {
                if (o.Type == "order_copyNumber" || o.Type == "order_copyItems")
                    return true;
                return false;
            });

            if (c1 != null && c2 != null)
                throw new Exception("copy(复本) 栏 和 copyNumber(套数) copyItems(每套册数) 栏不允许同时出现");
                */

            int nStartColIndex = context.LeftBlankColCount;

            context.FirstContentColumn = nStartColIndex;

            List<ColumnProperty> cols = new List<ColumnProperty>() {
                new ColumnProperty("序号", "no")
            };
            if (context.BiblioColList != null)
                cols.AddRange(context.BiblioColList);
            if (context.OrderColList != null)
                cols.AddRange(context.OrderColList);

            {
                // 输出书目记录列标题和订购记录列标题
                int i = 0;
                foreach (ColumnProperty col in cols)
                {
                    {
                        IXLCell cell = context.Sheet.Cell(context.RowIndex + 1, nStartColIndex + i + 1)
                            .SetValue("{" + col.Type + "}");
                    }

                    {
                        IXLCell cell = context.Sheet.Cell((context.RowIndex + 1) + 1, nStartColIndex + i + 1).SetValue(col.Caption);

                        // 最大字符数
                        ClosedXmlUtil.SetMaxChars(context.ColumnMaxChars,
                        nStartColIndex + i,
                        ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
                    }

                    //if (col.Type == "order_copyNumber")
                    //    context.CopyNumberColumn = nStartColIndex + i;

                    i++;
                }
            }

            // 把订购列做成一个 Group
            if (context.OrderColList != null)
            {
                context.Sheet.Columns(nStartColIndex + 1 + 1 + context.BiblioColList.Count,
                    nStartColIndex + 1 + 1 + context.BiblioColList.Count + context.OrderColList.Count - 1)
                    .Group();
            }

            context.Sheet.Row(context.RowIndex + 1).Height = 0;
            context.Sheet.SheetView.FreezeRows(context.RowIndex + 1 + 1);

            context.RowIndex += 2;
            context.ContentStartRow = context.RowIndex;
        }

        // “获得一个订购记录”的回调函数原型
        // parameters:
        //      strOrderRecPath 订购记录路径。如果为空，表示希望获得默认记录模板内容
        public delegate EntityInfo GetOrderRecord(string strBiblioRecPath,
            string strOrderRecPath);

        // 输出和一个订购记录有关的去向信息行
        // parameters:
        //      order   订购记录信息。如果为 null，表示订购信息为空
        public virtual void OutputDistributeInfo(
            // ExportDistributeContext context,
            MyForm form,
string strBiblioRecPath,
ref int nLineIndex,
string strTableXml,
string strStyle,
string strOrderRecPath,
GetOrderRecord procGetOrderRecord)
        {
            var context = this;

            int nStartColIndex = context.LeftBlankColCount;

            int nOldStartColIndex = nStartColIndex;

            if (string.IsNullOrEmpty(strTableXml))
            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = form.GetTable(
                    strBiblioRecPath,
                    context.BiblioColList,
                    // StringUtil.MakePathList(ColumnProperty.GetTypeList(context.BiblioColList)),
                    out strTableXml,
                    out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }

            IXLCell first_cell = null;
            IXLCell last_cell = null;

            // 行序号
            {
                IXLCell cell = context.Sheet.Cell(context.RowIndex + 1, nStartColIndex + 1).SetValue(nLineIndex + 1);
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                nStartColIndex++;
                first_cell = cell;
            }


            // 输出一行书目信息
            ExcelUtility.OutputBiblioLine(
            strBiblioRecPath,
            strTableXml,
            // ref nLineIndex,
            context.Sheet,
            nStartColIndex,  // nColIndex,
            ColumnProperty.GetTypeList(context.BiblioColList),
            context.RowIndex);

            nStartColIndex += context.BiblioColList.Count;

            // 获得订购记录信息
            EntityInfo order = procGetOrderRecord(strBiblioRecPath, strOrderRecPath);

            // 把订购记录中的 copy 元素进一步拆分为 copyNumber 和 copyItems 元素。注意拆分时只处理订购部分，不处理验收部分

            IXLCell copyNumberCell = null;
            // 输出订购信息列
            if (order != null)
            {
                ExcelUtility.OutputItemLine(
order.OldRecPath,
order.OldRecord,
0,
context.Sheet,
nStartColIndex,  // nColIndex,
ColumnProperty.GetTypeList(context.OrderColList),
ColumnProperty.GetDropDownList(context.OrderColList),
context.RowIndex,
XLColor.NoColor,
out copyNumberCell);
            }

            {
                // order.OldRecord = SplitCopyString(order.OldRecord);
                string strOrderState = order == null ? null : GetState(order.OldRecord);

                if (context.OrderColList != null)
                    nStartColIndex += context.OrderColList.Count;

                // 记载 cell 最大宽度
                for (int j = 0; j < nStartColIndex - nOldStartColIndex; j++)
                {
                    // string col = biblio_col_list[j];
                    //if (col == "recpath" || col == "书目记录路径")
                    //    continue;

                    IXLCell cell = context.Sheet.Cell(context.RowIndex + 1, nOldStartColIndex + j + 1);

                    last_cell = cell;   // 2019/12/3

                    // 最大字符数
                    ClosedXmlUtil.SetMaxChars(context.ColumnMaxChars,
                    nOldStartColIndex + j,
                    ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
                }

                // 订购记录状态不为空的行底色为灰色
                if (first_cell != null && last_cell != null
                    && string.IsNullOrEmpty(strOrderState) == false)
                {
                    var range = context.Sheet.Range(first_cell, last_cell);
                    range.Style.Fill.BackgroundColor = XLColor.DarkGray;
                }
            }

            nLineIndex++;
        }

        public virtual void OutputSumLine()
        {

        }

        /*
        public static void SetMaxChars(List<int> column_max_chars, int index, int chars)
        {
            // 确保空间足够
            while (column_max_chars.Count < index + 1)
            {
                column_max_chars.Add(0);
            }

            // 统计最大字符数
            int nOldChars = column_max_chars[index];
            if (chars > nOldChars)
            {
                column_max_chars[index] = chars;
            }
        }
        */

        public static void Warning(string strText)
        {
            Program.MainForm.OperHistory.AppendHtml("<div class='debug warning'>" + HttpUtility.HtmlEncode(strText) + "</div>");
        }

        public static void WarningRecPath(string strRecPath, string strXml)
        {
            if (string.IsNullOrEmpty(strXml) == false)
                Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(strRecPath) + "<br/>" + HttpUtility.HtmlEncode(strXml).Replace("\r", "<br/>").Replace(" ", "&nbsp;") + "</div>");
            else
                Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(strRecPath) + "</div>");
        }

        public static void WarningGreen(string strText)
        {
            Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode(strText) + "</div>");
        }

        public static string GetState(string strXml)
        {
            if (string.IsNullOrEmpty(strXml))
                return "";
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            if (dom.DocumentElement == null)
                return "";

            return DomUtil.GetElementText(dom.DocumentElement, "state");
        }


        public static string NULL_LOCATION_CAPTION = "(空)";


    }

}
