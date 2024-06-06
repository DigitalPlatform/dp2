using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation.Order
{
    /*
    // 导出 Excel 文件时用到的上下文结构
    public class ExportDistributeContext
    {
    }
    */

    // 输出订购去向分配表 Excel 文件的相关功能
    public class DistributeExcelFile : ExportExcelFile
    {
        public List<string> LocationList { get; set; }

        // 去向栏的开始，结束列号。从 0 开始计数
        public int DistributeStartColumn { get; set; }
        public int DistributeEndColumn { get; set; }

        private int _copyNumberColumn = -1; // -1 表示尚未初始化

        // 复本数字栏的列号。从 0 开始计数
        public int CopyNumberColumn
        {
            get => _copyNumberColumn;
            set => _copyNumberColumn = value;
        }

        // 是否只输出空白状态的订购记录。否则就是任何状态的订购记录都输出
        public bool OnlyOutputBlankStateOrderRecord { get; set; }

        public override void Clear()
        {
            LocationList = null;
            _copyNumberColumn = -1; // -1 表示尚未初始化
            OnlyOutputBlankStateOrderRecord = false;
        }

        // 向 Excel 文件输出列标题行。包括内部命令行
        public override void OutputDistributeInfoTitleLine(string strStyle)
        {
            var context = this;

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

            int nStartColIndex = context.LeftBlankColCount;

            List<ColumnProperty> cols = new List<ColumnProperty>() {
                new ColumnProperty("序号", "no")
            };
            cols.AddRange(context.BiblioColList);
            cols.AddRange(context.OrderColList);

            /*
            context.FirstContentColumn = nStartColIndex;


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
                        SetMaxChars(context.ColumnMaxChars,
                        nStartColIndex + i,
                        ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
                    }

                    if (col.Type == "order_copyNumber")
                        context.CopyNumberColumn = nStartColIndex + i;

                    i++;
                }
            }

            // 把订购列做成一个 Group
            context.Sheet.Columns(nStartColIndex + 1 + 1 + context.BiblioColList.Count,
                nStartColIndex + 1 + 1 + context.BiblioColList.Count + context.OrderColList.Count - 1)
                .Group();
                */

            // 书目信息右边输出馆藏地列表
            if (context.LocationList != null)   // 2019/12/3
            {
                nStartColIndex += cols.Count;
                context.DistributeStartColumn = nStartColIndex;
                int i = 0;
                foreach (string location in context.LocationList)
                {
                    {
                        // 命令行
                        IXLCell cell = context.Sheet.Cell(context.RowIndex + 1, nStartColIndex + i + 1)
                                .SetValue("{location:" + location + "}");
                    }

                    {
                        // 供阅读的标题
                        IXLCell cell = context.Sheet.Cell((context.RowIndex + 1) + 1, nStartColIndex + i + 1)
                            .SetValue(location);

                        cell.Style.Font.Bold = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // 最大字符数
                        ClosedXmlUtil.SetMaxChars(context.ColumnMaxChars,
                        nStartColIndex + i,
                        ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
                    }

                    i++;
                }

                context.DistributeEndColumn = context.DistributeStartColumn + i - 1;
            }

            /*
            context.Sheet.Row(context.RowIndex + 1).Height = 0;
            context.Sheet.SheetView.FreezeRows(context.RowIndex + 1 + 1);

            context.RowIndex += 2;
            context.ContentStartRow = context.RowIndex;
            */

            base.OutputDistributeInfoTitleLine(strStyle);
        }

        public override void OutputSumLine(
            // ExportDistributeContext context
            )
        {
            var context = this;

            if (context.ContentStartRow != -1 && context.ContentEndRow != -1)
            {
                if (context.CopyNumberColumn != -1)
                {
                    IXLRange range = context.Sheet.Range(context.ContentStartRow + 1, context.CopyNumberColumn + 1,
                        context.ContentEndRow + 1, context.CopyNumberColumn + 1);
                    range.LastCell().CellBelow().FormulaA1 = "SUM(" + range.RangeAddress.ToString() + ")";
                }

                for (int column = context.DistributeStartColumn; column <= context.DistributeEndColumn; column++)
                {
                    if (context.DistributeStartColumn != -1 && context.DistributeEndColumn != -1)
                    {
                        IXLRange range = context.Sheet.Range(context.ContentStartRow + 1, column + 1,
            context.ContentEndRow + 1, column + 1);
                        IXLCell cell = range.LastCell().CellBelow();
                        cell.FormulaA1 = "SUM(" + range.RangeAddress.ToString() + ")";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                }
                if (context.FirstContentColumn != -1)
                {
                    IXLRange range = context.Sheet.Range(context.ContentEndRow + 1 + 1, context.FirstContentColumn + 1,
context.ContentEndRow + 1 + 1, context.DistributeEndColumn + 1);
                    range.FirstCell().Value = "合计";
                    range.Style.Fill.BackgroundColor = XLColor.LightGray;

#if NO
                    foreach (IXLCell cell in range.Cells())
                    {
                        cell.Value = cell.Value;
                    }
#endif
                }
            }

        }


        // 过滤订购记录
        // return:
        //      true    保留
        //      false   被过滤掉
        public static bool FilterOrderRecord(XmlDocument order_dom,
            string strSellerFilter,
            string strLibraryCode,
            bool bOnlyOutputBlankStateOrderRecord,
            string strOrderRecPath)
        {
            if (bOnlyOutputBlankStateOrderRecord)
            {
                string strState = DomUtil.GetElementText(order_dom.DocumentElement, "state");
                if (string.IsNullOrEmpty(strState) == false)
                {
                    Warning(string.Format("订购记录 {0} 因状态为 '{1}'，被忽略导出", strOrderRecPath, strState));
                    return false;   // 只处理状态为空的订购记录。也就是说 “已订购” 和 “已验收” 的都不会被处理
                }
            }

            string strSeller = DomUtil.GetElementText(order_dom.DocumentElement, "seller");
            if (String.IsNullOrEmpty(strSellerFilter) == true && string.IsNullOrEmpty(strSeller) == true)
            {

            }
            else if (strSellerFilter != "*")
            {
                if (StringUtil.IsInList(strSeller, strSellerFilter) == false)
                {
                    Warning(string.Format("订购记录 {0}因书商字段 '{1}' 不包含在过滤字符串 '{2}' 中，被忽略导出", strOrderRecPath, strSeller, strSellerFilter));
                    return false;
                }
            }

            {
                string strDistribute = DomUtil.GetElementInnerText(order_dom.DocumentElement, "distribute");

                // 观察一个馆藏分配字符串，看看是否在指定用户权限的管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                int nRet = dp2StringUtil.DistributeInControlled(strDistribute,
                    strLibraryCode,
                    out bool bAllOutOf,
                    out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
                if (nRet == 0)
                {
                    Warning(string.Format("订购记录 {0} 因去向字段 '{1}' 越过馆代码 '{2}' 控制，被忽略导出", strOrderRecPath, strDistribute, strLibraryCode));
                    return false;
                }
            }

            WarningGreen("订购记录 '" + strOrderRecPath + "' 导出");
            return true;
        }

        // 输出和一个书目记录有关的去向信息行
        // parameters:
        //      strSourceFilter   书商名列表。列出的书商有关的订购记录才参与输出。如果为 "*"，表示全部输出
        // return:
        //      共输出了多少条订购记录
        public int OutputDistributeInfos(
            // ExportDistributeContext context,
            MyForm form,
            string strSellerFilter,
            string strLibraryCode,
    string strBiblioRecPath,
    ref int nLineIndex,
    string strStyle,
    GetOrderRecord procGetOrderRecord
                )
        {
            var context = this;

            string strTableXml = "";

            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = form.GetTable(
                    strBiblioRecPath,
                    context.BiblioColList,
                    // StringUtil.MakePathList(ColumnProperty.GetTypeList(context.BiblioColList)),
                    null,
                    null,
                    out strTableXml,
                    out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }

            List<EntityInfo> orders = new List<EntityInfo>();

            // 遍历书目记录下属的所有订购记录。按照书商进行筛选
            /*
            LibraryChannel channel = form.GetChannel();
            */
            var looping = form.Looping(out LibraryChannel channel);
            try
            {
                SubItemLoader sub_loader = new SubItemLoader();
                sub_loader.BiblioRecPath = strBiblioRecPath;
                sub_loader.Channel = channel;
                sub_loader.Stop = looping.Progress;
                sub_loader.DbType = "order";

                sub_loader.Prompt += new MessagePromptEventHandler(form.OnLoaderPrompt);

                int i = 0;
                foreach (EntityInfo info in sub_loader)
                {
                    if (info.ErrorCode != ErrorCodeValue.NoError)
                    {
                        string strError = "路径为 '" + info.OldRecPath + "' 的订购记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                        throw new Exception(strError);
                    }

                    WarningRecPath(info.OldRecPath + " -- " + (i + 1) + "/" + sub_loader.TotalCount + ")", DomUtil.GetIndentXml(DomUtil.RemoveEmptyElements(info.OldRecord)));

                    XmlDocument order_dom = new XmlDocument();
                    order_dom.LoadXml(info.OldRecord);

                    ////
                    // 过滤订购记录
                    // return:
                    //      true    保留
                    //      false   被过滤掉
                    if (FilterOrderRecord(order_dom,
                        strSellerFilter,
                        strLibraryCode,
                        context.OnlyOutputBlankStateOrderRecord,
                        info.OldRecPath) == false)
                    {
                        i++;
                        continue;
                    }

                    orders.Add(info);
                    i++;
                }

                sub_loader.Prompt -= new MessagePromptEventHandler(form.OnLoaderPrompt);
            }
            finally
            {
                looping.Dispose();
                /*
                form.ReturnChannel(channel);
                */
            }

            int nOrderCount = 0;
            // 如果书目记录下属没有任何订购记录，则也输出一个订购信息为空的行
            if (orders.Count == 0)
            {
                OutputDistributeInfo(
                    // context,
                    form,
strBiblioRecPath,
ref nLineIndex,
strTableXml,
strStyle,
"", // 表示希望获得订购记录模板
procGetOrderRecord
);
                context.RowIndex++;
                nOrderCount++;
            }
            else
            {
                foreach (EntityInfo order in orders)
                {
                    OutputDistributeInfo(
                        // context,
                        form,
    strBiblioRecPath,
    ref nLineIndex,
    strTableXml,
    strStyle,
    order.OldRecPath,
    (biblio_recpath, order_recpath) =>
    {
        Debug.Assert(strBiblioRecPath == biblio_recpath, "");
        Debug.Assert(order_recpath == order.OldRecPath, "");
        return order;
    }
    );
                    context.RowIndex++;
                    nOrderCount++;
                }
            }

            return nOrderCount;
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
                    null,
                    null,
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
            context.BiblioColList,
            context.RowIndex);

            nStartColIndex += context.BiblioColList.Count;

            // 获得订购记录信息
            EntityInfo order = procGetOrderRecord(strBiblioRecPath, strOrderRecPath);

            // 把订购记录中的 copy 元素进一步拆分为 copyNumber 和 copyItems 元素。注意拆分时只处理订购部分，不处理验收部分
            order.OldRecord = SplitCopyString(order.OldRecord);
            string strOrderState = GetState(order.OldRecord);

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
context.OrderColList,
// ColumnProperty.GetTypeList(context.OrderColList),
ColumnProperty.GetDropDownList(context.OrderColList),
context.RowIndex,
XLColor.NoColor,
out copyNumberCell);
            }

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

            // 书目信息右边输出馆藏地列表
            if (context.LocationList != null)   // 2019/12/3
            {
                XmlDocument order_dom = new XmlDocument();
                order_dom.LoadXml(order != null ? order.OldRecord : "<root />");

                string strCopyNumber = GetCopyNumber(DomUtil.GetElementText(order_dom.DocumentElement, "copy"));
                if (int.TryParse(strCopyNumber, out int copy_number) == false)
                    throw new Exception("复本数字部分 '" + strCopyNumber + "' 不合法");

                string strDistribute = DomUtil.GetElementText(order_dom.DocumentElement, "distribute");

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(strDistribute, out string strError);
                if (nRet == -1)
                    throw new Exception("订购记录 '" + order.OldRecPath + "' 的去向字段格式错误:" + strError);

                // 对去向字符串进行规范化处理。注意 count 比 copyNumber 大或者小的情形，要进行修正
                if (locations.Count > copy_number)
                    locations = LocationCollection.CopyItems(locations, copy_number);
                else if (locations.Count < copy_number)
                    locations.EnsureItems(copy_number, "");

                List<IXLCell> location_cells = new List<IXLCell>();
                int i = 0;
                foreach (string location0 in context.LocationList)
                {
                    string location = location0;
                    if (location == NULL_LOCATION_CAPTION)
                        location = "";
                    int number = locations.GetLocationCopy(location);
                    // 文本为空，只要格子存在即可
                    IXLCell cell = null;

                    if (nLineIndex == -1)
                        cell = context.Sheet.Cell(context.RowIndex + 1, nStartColIndex + i + 1).SetValue(location);
                    else if (number == 0)
                        cell = context.Sheet.Cell(context.RowIndex + 1, nStartColIndex + i + 1).SetValue<string>("");
                    else
                        cell = context.Sheet.Cell(context.RowIndex + 1, nStartColIndex + i + 1).SetValue<int>(number);

                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Protection.SetLocked(false);
                    if (string.IsNullOrEmpty(location) || IsNullLocation(location))
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    location_cells.Add(cell);

                    i++;
                }

                if (location_cells.Count > 0)
                {
                    var dv1 = context.Sheet.Range(location_cells[0], location_cells[location_cells.Count - 1]).SetDataValidation();
                    dv1.WholeNumber.Between(0, 1000);

                    dv1.ErrorStyle = XLErrorStyle.Warning;
                    dv1.ErrorTitle = "数字超出范围";
                    dv1.ErrorMessage = "本单元只允许输入 0 到 1000 之间的数字";
                }

                // 设置汇总公式
                if (copyNumberCell != null && location_cells.Count > 0)
                {
                    var address = context.Sheet.Range(location_cells[0], location_cells[location_cells.Count - 1]).RangeAddress.ToString();
                    copyNumberCell.FormulaA1 = "SUM(" + address + ")";
                    // copyNumberCell.Value = copyNumberCell.Value;
                }

                if (location_cells.Count > 0)
                    last_cell = location_cells[location_cells.Count - 1];

            }

            // 订购记录状态不为空的行底色为灰色
            if (first_cell != null && last_cell != null
                && string.IsNullOrEmpty(strOrderState) == false)
            {
                var range = context.Sheet.Range(first_cell, last_cell);
                range.Style.Fill.BackgroundColor = XLColor.DarkGray;
            }

            nLineIndex++;
        }

        // 判断 馆代码/阅览室名 中 阅览室名部分是否为空
        static bool IsNullLocation(string strLocation)
        {
            // 解析
            Global.ParseCalendarName(strLocation,
        out string strLibraryCode,
        out string strRoom);
            if (string.IsNullOrEmpty(strRoom))
                return true;
            return false;
        }

        // 根据分馆身份过滤馆藏地列表。并在列表末尾自动添加空馆藏地事项
        public static List<string> FilterLocationList(List<string> location_list,
            string strLibraryCode)
        {
            location_list = dp2StringUtil.FilterLocationList(location_list, strLibraryCode);
            if (location_list.Count == 0)
                return location_list;

            // 增加 (空) 这一项
            string strBlank = Order.DistributeExcelFile.NULL_LOCATION_CAPTION;
            if (string.IsNullOrEmpty(strLibraryCode) == false && strLibraryCode != "[仅总馆]")
                strBlank = strLibraryCode + "/";
            if (location_list.IndexOf(strBlank) == -1)
                location_list.Add(strBlank);
            else
            {
                // 移动到列表末尾。这样在 Excel 文件中处在最末一栏显示，还需要变为深灰色背景表示不希望用户随便选用它
                location_list.Remove(strBlank);
                location_list.Add(strBlank);
            }

            return location_list;
        }



        public static void AdjectColumnWidth(IXLWorksheet sheet,
List<int> column_max_chars,
int MAX_CHARS = 50)
        {
            List<int> wrap_columns = new List<int>();
            // 字符数太多的列不要做 width auto adjust
            foreach (IXLColumn column in sheet.Columns())
            {
                // int MAX_CHARS = 50;   // 60

                int nIndex = column.FirstCell().Address.ColumnNumber - 1;
                if (nIndex >= column_max_chars.Count)
                    break;
                int nChars = column_max_chars[nIndex];
                if (nChars == 0)
                    continue;
#if NO
                if (nIndex == 1)
                {
                    column.Width = 10;
                    continue;
                }

                    if (nIndex == 3)
                        MAX_CHARS = 50;
                    else
                        MAX_CHARS = 24;
#endif

                //if (nChars < MAX_CHARS)
                //    column.AdjustToContents();
                //else
                {
                    column.Width = Math.Min(MAX_CHARS, nChars);
                    if (wrap_columns.IndexOf(nIndex) == -1)
                        wrap_columns.Add(nIndex);
                }
            }

            foreach (int index in wrap_columns)
            {
                sheet.Column(index + 1).Style.Alignment.WrapText = true;
            }
        }

        public delegate void ProcessOrderRecord(
            string strBiblioRecPath,
            string strOrderRecPath,
            string strDistributeString,
            Dictionary<string, string> order_content_table,
            IXLCell orderRecPathCell,
            IXLCell copyCell
            );

        // return:
        //      -1  导入过程出错，并且本函数已经 MessageBox 报错了
        //      0   放弃导入
        //      1   导入完成
        public static int ImportFromOrderDistributeExcelFile(ProcessOrderRecord proc)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "请指定要导入的订购去向 Excel 文件名",
                // dlg.FileName = this.RecPathFilePath;
                // dlg.InitialDirectory = 
                Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return 0;

            string strError = "";
            try
            {
                using (XLWorkbook doc = new XLWorkbook(dlg.FileName))
                {
                    // 第一个 Worksheet
                    var sheet = doc.Worksheets.Worksheet(1);

                    ImportContext context = new ImportContext(sheet);

                    while (context.NextDataRow(sheet) == false)
                    {
                        // 保存一条订购记录
                        proc(
                            context._content_name_value_table["biblio_recpath"],
                            context._content_name_value_table["order_recpath"],
                            context.DistributeString,
                            context.GetOrderTable(),
                            context.OrderRecPathCell,
                            context.CopyCell
                            );
                    }

                    doc.Save();
                }
            }
            catch (Exception ex)
            {
                strError = "导入过程出现异常: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            return 1;
            ERROR1:
            MessageBox.Show(Program.MainForm, strError);
            return -1;
        }

        public class ImportContext
        {
            // 类型定义 type name --> column index
            Dictionary<string, int> _type_name_index_table { get; set; }

            // 类型定义 column index --> type name
            Dictionary<int, string> _type_index_name_table { get; set; }


            public IXLRow CommandRow { get; set; }

            public IXLRow CurrentDataRow { get; set; }

            // 当前行内订购记录路径格子。用于修改这个格子的值
            public IXLCell OrderRecPathCell { get; set; }

            // 当前行内复本格子。用于修改这个格子的值
            public IXLCell CopyCell { get; set; }

            // 内容 type name --> data value
            internal Dictionary<string, string> _content_name_value_table { get; set; }

            internal string DistributeString { get; set; }

            public Dictionary<string, string> GetBiblioTable()
            {
                Dictionary<string, string> results = new Dictionary<string, string>();
                foreach (string key in _content_name_value_table.Keys)
                {
                    if (key.StartsWith("biblio_"))
                        results.Add(key.Substring("biblio_".Length), _content_name_value_table[key]);
                }

                return results;
            }

            public Dictionary<string, string> GetOrderTable()
            {
                Dictionary<string, string> results = new Dictionary<string, string>();
                foreach (string key in _content_name_value_table.Keys)
                {
                    if (key.StartsWith("order_"))
                        results.Add(key.Substring("order_".Length), _content_name_value_table[key]);
                }

                return results;
            }

            // 导入 Excel 文件时，用到的上下文结构
            public ImportContext(IXLWorksheet sheet)
            {
                this._type_name_index_table = new Dictionary<string, int>();
                this._type_index_name_table = new Dictionary<int, string>();
                this._content_name_value_table = null;

                // 找到 command row
                IXLRow row = FindCommandRow(sheet);
                if (row == null)
                    throw new Exception("命令行没有找到");

                this.CommandRow = row;
                this.CurrentDataRow = row;

                IXLCell cell = row.FirstCellUsed();
                while (cell != null)
                {
                    string type = cell.GetValue<string>();
                    type = StringUtil.Unquote(type, "{}");

                    this._type_name_index_table.Add(type, cell.Address.ColumnNumber);
                    this._type_index_name_table.Add(cell.Address.ColumnNumber, type);

                    if (cell == row.LastCellUsed())
                        break;
                    cell = cell.CellRight();
                }
            }

            void ClearCurrentDataRow()
            {
                this.CurrentDataRow = null;
                this.OrderRecPathCell = null;
                this.CopyCell = null;
            }

            // 获得下一个数据行的信息
            // return:
            //      true    已经读完所有数据行。本次操作没有设置有效数据
            //      false   还没有读完数据行
            public bool NextDataRow(IXLWorksheet sheet)
            {
                if (this.CurrentDataRow == null)
                    return true;
                else
                    this.CurrentDataRow = this.CurrentDataRow.RowBelow();

                while (this.CurrentDataRow != null)
                {
                    IXLCell cell = this.CurrentDataRow.FirstCellUsed();
                    if (cell == null)
                    {
                        ClearCurrentDataRow();
                        return true;
                    }
                    string value = cell.GetValue<string>();
                    if (Int64.TryParse(value, out Int64 result) == true)
                        break;

                    if (this.CurrentDataRow == sheet.LastRowUsed())
                    {
                        ClearCurrentDataRow();
                        return true;
                    }
                    this.CurrentDataRow = this.CurrentDataRow.RowBelow();
                }

                // 填充信息
                {
                    if (this._content_name_value_table == null)
                        this._content_name_value_table = new Dictionary<string, string>();
                    this._content_name_value_table.Clear();

                    List<string> location_list = new List<string>();

                    IXLCell cell = this.CurrentDataRow.FirstCellUsed();
                    while (cell != null)
                    {
                        int column = cell.Address.ColumnNumber;

                        string type = _type_index_name_table[column];

                        this._content_name_value_table.Add(type, cell.GetValue<string>());

                        if (type == "order_recpath")
                            this.OrderRecPathCell = cell;
                        else if (type == "order_copy")
                            this.CopyCell = cell;

                        if (type.StartsWith("location:"))
                        {
                            string strNumber = cell.GetValue<string>();
                            if (string.IsNullOrEmpty(strNumber) || strNumber == "0")
                            {
                                // 空或者 "0" 表示这个馆藏地没有册。跳过，不处理
                            }
                            else
                            {
                                string strLocation = type.Substring("location:".Length);
                                if (strLocation == NULL_LOCATION_CAPTION)
                                    strLocation = "";
                                location_list.Add(strLocation + ":" + strNumber);
                            }
                        }

                        if (cell == this.CurrentDataRow.LastCellUsed())
                            break;
                        cell = cell.CellRight();
                    }

                    this.DistributeString = StringUtil.MakePathList(location_list, ";");
                }

                return false;
            }

            static IXLRow FindCommandRow(IXLWorksheet sheet)
            {
                // 找到 command row
                IXLRow row = sheet.FirstRowUsed();
                while (row != null)
                {
                    IXLCell cell = row.FirstCellUsed();
                    if (cell == null)
                        break;
                    if (cell.GetValue<string>().StartsWith("{"))
                        return row;
                    if (row == sheet.LastRowUsed())
                        break;
                    row = row.RowBelow();
                }
                return null;
            }
        }

        internal static List<ColumnProperty> BuildList(
            List<Column> columns,
            bool change_evalue_type = false)
        {
            List<ColumnProperty> results = new List<ColumnProperty>();
            foreach (var column in columns)
            {
                string strCaption = column.Caption;

                // 如果没有caption定义，就挪用name定义
                if (String.IsNullOrEmpty(strCaption) == true)
                    strCaption = column.Name;

                string strClass = StringUtil.GetLeft(column.Name);

                ColumnProperty prop = new ColumnProperty(strCaption, strClass);
                prop.Evalue = column.Evalue;

                // 2023/4/7
                // 为具有 .Evalue 的列修改 .Type 名字，增加一个前缀，以便和普通 Column 区别
                if (change_evalue_type
                    && string.IsNullOrEmpty(prop.Evalue) == false)
                    prop.Type = "evalue-" + prop.Type.Replace("_", "-");

                results.Add(prop);
            }

            return results;
        }

#if REMOVED
        internal static List<ColumnProperty> BuildList(PrintOption option)
        {
            List<ColumnProperty> results = new List<ColumnProperty>();
            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strCaption = column.Caption;

                // 如果没有caption定义，就挪用name定义
                if (String.IsNullOrEmpty(strCaption) == true)
                    strCaption = column.Name;

                string strClass = StringUtil.GetLeft(column.Name);

                ColumnProperty prop = new ColumnProperty(strCaption, strClass);
                prop.Evalue = column.Evalue;

                // 2023/4/7
                // 为具有 .Evalue 的列修改 .Type 名字，增加一个前缀，以便和普通 Column 区别
                if (string.IsNullOrEmpty(prop.Evalue) == false)
                    prop.Type = "evalue-" + prop.Type.Replace("_", "-");

                results.Add(prop);
            }

            return results;
        }

#endif

        static string SplitCopyString(string strXml)
        {
            if (string.IsNullOrEmpty(strXml))
                return strXml;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);

            if (dom.DocumentElement == null)
                return "";

            string strCopyString = DomUtil.GetElementText(dom.DocumentElement, "copy");

            // 分离 "old[new]" 内的两个值
            dp2StringUtil.ParseOldNewValue(strCopyString,
                out string strOldCopy,
                out string strNewCopy);

            if (string.IsNullOrEmpty(strOldCopy))
                strOldCopy = "0";

            // 对 strOldCopy 进一步分解
            string strLeft = dp2StringUtil.GetCopyFromCopyString(strOldCopy);
            string strRight = dp2StringUtil.GetRightFromCopyString(strOldCopy);
            if (string.IsNullOrEmpty(strRight))
                strRight = "1"; // 默认 1

            DomUtil.SetElementText(dom.DocumentElement, "copyNumber", strLeft);
            if (string.IsNullOrEmpty(strRight) == false)
                DomUtil.SetElementText(dom.DocumentElement, "copyItems", strRight);
            else
            {
                Debug.Assert(false, "不应该不写入 copyItems 元素。因为这样会被后面当作空或者 0");
            }

            return dom.DocumentElement.OuterXml;
        }


        static string GetCopyNumber(string strCopyString)
        {
            // 分离 "old[new]" 内的两个值
            dp2StringUtil.ParseOldNewValue(strCopyString,
                out string strOldCopy,
                out string strNewCopy);

            if (string.IsNullOrEmpty(strOldCopy))
                strOldCopy = "0";

            // 对 strOldCopy 进一步分解
            return dp2StringUtil.GetCopyFromCopyString(strOldCopy);
        }

    }

    // 列属性
    public class ColumnProperty
    {
        // 用于阅读的标题
        public string Caption { get; set; }

        // 用于程序识别的类型
        public string Type { get; set; }

        // 可用值列表
        public List<string> ValueList { get; set; }

        // 2023/4/7
        // 脚本代码
        public string Evalue { get; set; }

        public ColumnProperty(string caption, string type)
        {
            this.Caption = caption;
            this.Type = type;
        }

        public static List<string> GetDropDownList(List<ColumnProperty> property_list)
        {
            // 2023/4/7
            if (property_list == null)
                return null;
            List<string> results = new List<string>();
            property_list.ForEach((o) =>
            {
                results.Add(StringUtil.MakePathList(o.ValueList));
            });

            return results;
        }

        // parameters:
        //      bRemovePrefix   是否移除 xxx_ 这样的前缀
        //      bRemoveEvalue   是否移除 .Evalue 为非空的值
        public static List<string> GetTypeListEx(List<ColumnProperty> property_list,
    bool bRemovePrefix = true,
    bool bRemoveEvalue = true)
        {
            // 2023/4/7
            if (property_list == null)
                return null;
            List<string> results = new List<string>();
            property_list.ForEach((o) =>
            {
                // 2023/4/11
                if (bRemoveEvalue && string.IsNullOrEmpty(o.Evalue) == false)
                    return;

                string type = o.Type;

                if (bRemovePrefix)
                {
                    /*
                    // 如果为 "xxxx_xxxxx" 形态，则取 _ 右边的部分
                    int nRet = type.IndexOf("_");
                    if (nRet != -1)
                        type = type.Substring(nRet + 1).Trim();
                    */
                    type = MyForm.RemovePrefix(type);
                }
                results.Add(type);
            });

            return results;
        }


        // parameters:
        //      bRemovePrefix   是否移除 xxx_ 这样的前缀
        //      bRemoveEvalue   是否移除 .Evalue 为非空的值
        public static List<string> GetTypeList(List<ColumnProperty> property_list,
            bool bRemovePrefix = true)
        {
            // 2023/4/7
            if (property_list == null)
                return null;
            List<string> results = new List<string>();
            property_list.ForEach((o) =>
            {
                string type = o.Type;

                if (bRemovePrefix)
                {
                    // 如果为 "xxxx_xxxxx" 形态，则取 _ 右边的部分
                    int nRet = type.IndexOf("_");
                    if (nRet != -1)
                        type = type.Substring(nRet + 1).Trim();
                }
                results.Add(type);
            });

            return results;
        }

        public static int FillValueList(LibraryChannel channel,
            string strLibraryCode,
            List<ColumnProperty> order_title_list,
            out string strError)
        {
            strError = "";

            string[] names = new string[] { "orderSeller",
                "orderSource",
                "orderClass",
            };

            foreach (string name in names)
            {
                ColumnProperty seller_prop = order_title_list.Find((o) =>
                { if (o.Type == name.Replace("order", "order_").ToLower()) return true; return false; });

                if (seller_prop == null)
                    continue;

                // order_seller 列表
                int nRet = Program.MainForm.GetValueTable(name,
                "",
                out string[] values,
                out strError);
                if (nRet == -1)
                    return -1;

                var list = new List<string>(values);
                if (strLibraryCode == "[仅总馆]")
                {
                    list = Global.FilterValuesWithLibraryCode("", list);

                    // 去掉每个元素内的 {} 部分
                    list = StringUtil.FromListString(StringUtil.GetPureSelectedValue(StringUtil.MakePathList(list)));

                    StringUtil.RemoveDupNoSort(ref list);
                }

                if (seller_prop != null)
                    seller_prop.ValueList = new List<string>(list);
            }

            return 0;
        }
    }

    internal class BiblioColumnOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public BiblioColumnOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

#if NO
            this.PageHeaderDefault = "%date% 原始订购数据 - %recpathfilename% - (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% 原始订购数据";

            this.LinesPerPageDefault = 20;
#endif

            // Columns缺省值
            Columns.Clear();
            this.Columns.AddRange(GetAllColumns(true));
        }

        internal static string GetRightPart(string strText)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
                return "";

            return strText.Substring(nRet + 2).Trim();
        }

        public override List<Column> GetAllColumns(bool bDefault)
        {
            List<Column> results = new List<Column>();

            string[] lines = new string[] {
            "biblio_recpath -- 书目记录路径",
            "biblio_title -- 题名",
            "biblio_titlepinyin -- 题名拼音",
            "biblio_author -- 责任者",
            "biblio_title_area -- 题名与责任者",
            "biblio_edition_area -- 版本项",
            "biblio_material_specific_area -- 资料特殊细节项",
            "biblio_publication_area -- 出版发行项",
            "biblio_material_description_area -- 载体形态项",
            "biblio_series_area -- 丛编项",
            "biblio_notes_area -- 附注项",
            "biblio_resource_identifier_area -- 获得方式项",
            "biblio_isbn -- ISBN",
            "biblio_issn -- ISSN",
            "biblio_price -- 价格",
            "biblio_publisher -- 出版者",
            "biblio_publishtime -- 出版时间",
            "biblio_pages -- 页数",
            "biblio_summary -- 提要文摘",
            "biblio_subjects -- 主题分析",
            "biblio_classes -- 分类号",
            "biblio_clc_class -- 中图法分类号",
            "biblio_ktf_class -- 科图法分类号",
            "biblio_rdf_class -- 人大法分类号",
            "author_accesspoint -- 责任者检索点",
        };

            foreach (string line in lines)
            {
                Column column = new Column();
                column.Name = line;
                column.Caption = GetRightPart(line);
                column.MaxChars = -1;
                results.Add(column);
            }

            return results;
        }
    }

    // 适合导出册信息的书目列定义
    internal class ExportBiblioColumnOption : Order.BiblioColumnOption
    {
        public ExportBiblioColumnOption(string strDataDir) : base(strDataDir, "")
        {
            this.DataDir = strDataDir;

            // Columns缺省值
            Columns.Clear();
            this.Columns.AddRange(GetAllColumns(true));
        }

        public override List<Column> GetAllColumns(bool bDefault)
        {
            if (bDefault)
            {
                List<Column> results = new List<Column>();

                string[] lines = new string[] {
            "biblio_title -- 题名",
            "biblio_author -- 责任者",
            "biblio_edition_area -- 版本项",
            "biblio_publisher -- 出版者",
            "biblio_publishtime -- 出版时间",
            "biblio_isbn -- ISBN",
            "biblio_issn -- ISSN",
            "biblio_classes -- 分类号",
            "biblio_author_accesspoint -- 责任者检索点",
            "biblio_recpath -- 书目记录路径",
            };

                foreach (string line in lines)
                {
                    Column column = new Column();
                    column.Name = line;
                    column.Caption = GetRightPart(line);
                    column.MaxChars = -1;
                    results.Add(column);
                }

                return results;
            }

            {
                var results = base.GetAllColumns(false);

                string[] lines = new string[] {
            "biblio_accessNo -- 索取号",   // 2023/11/9
            "biblio_itemCount -- 册数",   // 2023/11/9
            "biblio_itemPrice -- 册价格总和",   // 2024/5/17
                };

                foreach (string line in lines)
                {
                    Column column = new Column();
                    column.Name = line;
                    column.Caption = GetRightPart(line);
                    column.MaxChars = -1;
                    results.Add(column);
                }

                return results;
            }
        }
    }


    internal class OrderColumnOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public OrderColumnOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            // Columns缺省值
            Columns.Clear();
            this.Columns.AddRange(GetAllColumns(true));
        }

        public override List<Column> GetAllColumns(bool bDefault)
        {
            List<Column> results = new List<Column>();

            string[] lines = new string[] {
            "order_recpath -- 订购记录路径",
            "order_seller -- 渠道(书商)",

            // 2018/8/22
            "order_fixedPrice -- 码洋",
            "order_discount -- 折扣",

            "order_price -- 订购价",
            "order_source -- 经费来源",
            // "order_copy -- 复本数",
            "order_copyNumber -- 套数",
            "order_copyItems -- 每套册数",
            "order_orderID -- 订单号",
            "order_class -- 类别",
            "order_batchNo -- 批次号",
            "order_catalogNo -- 书目号",
            "order_comment -- 附注",
            "order_state -- 订购记录状态",
        };

            foreach (string line in lines)
            {
                Column column = new Column();
                column.Name = line;
                column.Caption = BiblioColumnOption.GetRightPart(line);
                column.MaxChars = -1;
                results.Add(column);
            }

            return results;
        }
    }


    // 2019/12/3
    internal class EntityColumnOption : PrintOption
    {
        string PublicationType = "图书"; // 图书 连续出版物

        public EntityColumnOption(string strDataDir,
            string strPublicationType)
        {
            this.DataDir = strDataDir;
            this.PublicationType = strPublicationType;

            // Columns缺省值
            Columns.Clear();
            this.Columns.AddRange(GetAllColumns(true));
        }

        public override List<Column> GetAllColumns(bool bDefault)
        {
            List<Column> results = new List<Column>();

            if (bDefault == true)
            {
                // 按照字段的常用顺序高低排列
                string[] lines = new string[] {
            "item_state -- 册记录状态",
            "item_barcode -- 册条码号",
            "item_location -- 馆藏地",
            "item_bookType -- 图书类型",
            "item_price -- 册价格",
            "item_volume -- 卷册号",
            "item_accessNo -- 索取号",

            "item_currentLocation -- 当前位置",
            "item_shelfNo -- 架号",
            "item_batchNo -- 批次号",
            "item_comment -- 附注",

            "item_publishTime -- 出版时间",
            "item_recpath -- 册记录路径",
        };

                foreach (string line in lines)
                {
                    Column column = new Column();
                    column.Name = line;
                    column.Caption = BiblioColumnOption.GetRightPart(line);
                    column.MaxChars = -1;
                    results.Add(column);
                }

                return results;

            }

            {
                // 按照字段的常用顺序高低排列
                string[] lines = new string[] {
            "item_state -- 册记录状态",
            "item_barcode -- 册条码号",
            "item_location -- 馆藏地",
            "item_bookType -- 图书类型",
            "item_price -- 册价格",
            "item_volume -- 卷册号",
            "item_accessNo -- 索取号",

            "item_currentLocation -- 当前位置",
            "item_shelfNo -- 架号",
            "item_batchNo -- 批次号",
            "item_comment -- 附注",

            "item_publishTime -- 出版时间",
            "item_seller -- 渠道(书商)",
            "item_source -- 经费来源",
            "item_registerNo -- 登录号",
            "item_intact -- 完好度",
            "item_bindingCost -- 装订价",

            "item_mergeComment -- 合并附注",
            "item_refID -- 参考ID",
            "item_recpath -- 册记录路径",
        };

                foreach (string line in lines)
                {
                    Column column = new Column();
                    column.Name = line;
                    column.Caption = BiblioColumnOption.GetRightPart(line);
                    column.MaxChars = -1;
                    results.Add(column);
                }

                return results;
            }
        }


    }

}
