using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClosedXML.Excel;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using dp2Circulation.Order;

namespace dp2Circulation
{
    public class ExportItemExcelFile : ExportExcelFile
    {
        Hashtable _biblioRecPathTable = new Hashtable();

        private int _priceColumn = -1; // -1 表示尚未初始化

        // 册价格栏的列号。从 0 开始计数
        public int PriceColumn
        {
            get => _priceColumn;
            set => _priceColumn = value;
        }

        public override void OutputDistributeInfoTitleLine(string strStyle)
        {
            base.OutputDistributeInfoTitleLine(strStyle);

            int nStartColIndex = this.LeftBlankColCount;

            List<ColumnProperty> cols = new List<ColumnProperty>() {
                new ColumnProperty("序号", "no")
            };
            cols.AddRange(this.BiblioColList);
            cols.AddRange(this.OrderColList);

            int i = 0;
            foreach (ColumnProperty col in cols)
            {
                if (col.Type == "item_price")
                    this.PriceColumn = nStartColIndex + i;

                i++;
            }

            this.EndContentColumn = this.FirstContentColumn + i;
        }

        private int _endContentColumn = -1;

        // 内容区域第一列的列号
        public int EndContentColumn
        {
            get => _endContentColumn;
            set => _endContentColumn = value;
        }

        // 输出和一个订购记录有关的去向信息行
        // parameters:
        //      order   订购记录信息。如果为 null，表示订购信息为空
        public override void OutputDistributeInfo(
            // ExportDistributeContext context,
            MyForm form,
string strBiblioRecPath,
ref int nLineIndex,
string strTableXml,
string strStyle,
string strOrderRecPath,
GetOrderRecord procGetOrderRecord)
        {
            base.OutputDistributeInfo(form,
                strBiblioRecPath,
                ref nLineIndex,
                strTableXml,
                strStyle,
                strOrderRecPath,
                procGetOrderRecord);
            _biblioRecPathTable[strBiblioRecPath] = true;
        }

        public override void OutputSumLine(
    // ExportDistributeContext context
    )
        {
            var context = this;

            if (context.ContentStartRow != -1 && context.ContentEndRow != -1)
            {
                if (context.PriceColumn != -1)
                {
                    List<string> prices = new List<string>();
                    IXLRange range = context.Sheet.Range(context.ContentStartRow + 1, context.PriceColumn + 1,
                        context.ContentEndRow + 1, context.PriceColumn + 1);
                    foreach (var cell in range.Cells())
                    {
                        var value = cell.GetString();
                        if (string.IsNullOrEmpty(value) == false)
                            prices.Add(value);
                    }
                    var total_price = PriceUtil.TotalPrice(prices);
                    context.Sheet.Cell(context.ContentEndRow + 1 + 1, context.PriceColumn + 1)
                    .SetValue(total_price);
                }

                if (context.FirstContentColumn != -1)
                {
                    IXLRange range = context.Sheet.Range(context.ContentEndRow + 1 + 1, context.FirstContentColumn + 1,
context.ContentEndRow + 1 + 1, context.EndContentColumn + 1);
                    range.FirstCell().Value = "合计";
                    range.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // TODO: 种数，册数
                    var biblio_count = _biblioRecPathTable.Count;
                    var entity_count = context.ContentEndRow - context.ContentStartRow + 1;

                    range.FirstCell().CellRight().SetValue($"种:{biblio_count}, 册:{entity_count}");
                }

            }

        }

        // 过滤册记录
        // return:
        //      true    保留
        //      false   被过滤掉
        public static bool FilterItemRecord(XmlDocument item_dom,
            string strLibraryCodeFilter,
            string strItemRecPath)
        {
            string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
            strLocation = StringUtil.GetPureLocation(strLocation);

            {
                // 观察一个馆藏分配字符串，看看是否在指定用户权限的管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                int nRet = LocationInControlled(strLocation,
                    strLibraryCodeFilter,
                    out bool bAllOutOf,
                    out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
                if (nRet == 0)
                {
                    Warning($"册购记录 {strItemRecPath} 因馆藏地字段 '{strLocation}' 不包含在过滤字符串 '{strLibraryCodeFilter}' 中，被忽略导出");
                    return false;
                }
            }

            WarningGreen("册记录 '" + strItemRecPath + "' 导出");
            return true;
        }

        // 观察一个馆藏分配字符串，看看是否在指定用户权限的管辖范围内
        // parameters:
        // return:
        //      -1  出错
        //      0   超过管辖范围(至少出现一处超过范围)。strError中有解释
        //      1   在管辖范围内
        public static int LocationInControlled(string strLocation,
            string strLibraryCodeList,
            out bool bAllOutOf,
            out string strError)
        {
            strError = "";
            bAllOutOf = false;

            //      bNarrow 如果为 true，表示 馆代码 "" 只匹配总馆，不包括各个分馆；如果为 false，表示 馆代码 "" 匹配总馆和所有分馆
            bool bNarrow = strLibraryCodeList == "[仅总馆]";
            if (strLibraryCodeList == "[仅总馆]")
                strLibraryCodeList = "";

            if (bNarrow == false && Global.IsGlobalUser(strLibraryCodeList) == true)
                return 1;

            /*
            // 2018/5/9
            if (string.IsNullOrEmpty(strLocation))
            {
                // 去向分配字符串为空，表示谁都可以控制它。这样便于分馆用户修改。
                // 若这种情况返回 0，则分馆用户修改不了，只能等总馆用户才有权限修改
                return 1;
            }
            */

            List<string> outof_list = new List<string>();
            {
                // 空的馆藏地点被视为不在分馆用户管辖范围内
                if (bNarrow == false && string.IsNullOrEmpty(strLocation) == true)
                {
                    //strError = "馆代码 '' 不在范围 '" + strLibraryCodeList + "' 内";
                    //return 0;
                    outof_list.Add("");
                    goto SKIP1;
                }

                // 解析
                Global.ParseCalendarName(strLocation,
            out string strLibraryCode,
            out string strPureName);

                if (string.IsNullOrEmpty(strLibraryCode) && string.IsNullOrEmpty(strLibraryCodeList))
                    goto SKIP1;

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                {
                    outof_list.Add(strLibraryCode);
                }
            }

            SKIP1:

            if (outof_list.Count > 0)
            {
                strError = "馆代码 '" + StringUtil.MakePathList(outof_list) + "' 不在范围 '" + strLibraryCodeList + "' 内";
                if (outof_list.Count == 1)
                    bAllOutOf = true;
                return 0;
            }

            return 1;
        }
    }
}
