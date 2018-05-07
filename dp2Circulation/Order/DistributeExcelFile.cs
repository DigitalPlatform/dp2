using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation.Order
{
    // 输出订购去向分配表 Excel 文件的相关功能
    public static class DistributeExcelFile
    {
        // 向 Excel 文件输出列标题行。包括内部命令行
        public static void OutputDistributeInfoTitleLine(
List<string> location_list,
IXLWorksheet sheet,
string strStyle,
List<ColumnProperty> biblio_col_list,
List<ColumnProperty> order_col_list,
ref int nRowIndex,
ref List<int> column_max_chars)
        {
            int nStartColIndex = 2;

            List<ColumnProperty> cols = new List<ColumnProperty>() {
                new ColumnProperty("序号", "no")
            };
            cols.AddRange(biblio_col_list);
            cols.AddRange(order_col_list);

            {
                // 输出书目记录列标题
                int i = 0;
                foreach (ColumnProperty col in cols)
                {
                    {
                        IXLCell cell = sheet.Cell(nRowIndex + 1, nStartColIndex + i + 1)
                            .SetValue("{" + col.Type + "}");
                    }

                    {
                        IXLCell cell = sheet.Cell((nRowIndex + 1) + 1, nStartColIndex + i + 1).SetValue(col.Caption);

                        // 最大字符数
                        PrintOrderForm.SetMaxChars(ref column_max_chars,
                        nStartColIndex + i,
                        ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
                    }

                    i++;
                }
            }

            // 把订购列做成一个 Group
            sheet.Columns(nStartColIndex + 1 + 1 + biblio_col_list.Count,
                nStartColIndex + 1 + 1 + biblio_col_list.Count + order_col_list.Count - 1)
                .Group();

            // 书目信息右边输出馆藏地列表
            {
                nStartColIndex += cols.Count;
                int i = 0;
                foreach (string location in location_list)
                {
                    {
                        // 命令行
                        IXLCell cell = sheet.Cell(nRowIndex + 1, nStartColIndex + i + 1)
                                .SetValue("{location:" + location + "}");
                    }

                    {
                        // 供阅读的标题
                        IXLCell cell = sheet.Cell((nRowIndex + 1) + 1, nStartColIndex + i + 1)
                            .SetValue(location);

                        cell.Style.Font.Bold = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // 最大字符数
                        PrintOrderForm.SetMaxChars(ref column_max_chars,
                        nStartColIndex + i,
                        ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
                    }

                    i++;
                }
            }

            sheet.Row(nRowIndex + 1).Height = 0;
            sheet.SheetView.FreezeRows(nRowIndex + 1 + 1);

            nRowIndex += 2;
        }


        // 输出和一个书目记录有关的去向信息行
        // parameters:
        //      strSourceFilter   书商名列表。列出的书商有关的订购记录才参与输出。如果为 "*"，表示全部输出
        public static void OutputDistributeInfos(
            MyForm form,
            List<string> location_list,
            string strSellerFilter,
            string strLibraryCode,
            IXLWorksheet sheet,
    // XmlDocument dom,
    string strRecPath,
    ref int nLineIndex,
    string strStyle,
    List<Order.ColumnProperty> biblio_col_list,
    ref int nRowIndex,
    List<Order.ColumnProperty> order_col_list,
    GetOrderRecord procGetOrderRecord,
                ref List<int> column_max_chars)
        {
            string strTableXml = "";

            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = form.GetTable(
                    strRecPath,
                    out strTableXml,
                    out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }

            List<EntityInfo> orders = new List<EntityInfo>();

            // 遍历书目记录下属的所有订购记录。按照书商进行筛选
            LibraryChannel channel = form.GetChannel();
            try
            {
                SubItemLoader sub_loader = new SubItemLoader();
                sub_loader.BiblioRecPath = strRecPath;
                sub_loader.Channel = channel;
                sub_loader.Stop = form.stop;
                sub_loader.DbType = "order";

                sub_loader.Prompt += new MessagePromptEventHandler(form.OnLoaderPrompt);

                foreach (EntityInfo info in sub_loader)
                {
                    if (info.ErrorCode != ErrorCodeValue.NoError)
                    {
                        string strError = "路径为 '" + info.OldRecPath + "' 的订购记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                        throw new Exception(strError);
                    }

                    XmlDocument order_dom = new XmlDocument();
                    order_dom.LoadXml(info.OldRecord);

                    string strState = DomUtil.GetElementText(order_dom.DocumentElement, "state");
                    if (string.IsNullOrEmpty(strState) == false)
                        continue;   // 只处理状态为空的订购记录。也就是说 “已订购” 和 “已验收” 的都不会被处理

                    string strSeller = DomUtil.GetElementText(order_dom.DocumentElement, "seller");
                    if (String.IsNullOrEmpty(strSellerFilter) == true && string.IsNullOrEmpty(strSeller) == true)
                    {

                    }
                    else if (strSellerFilter != "*")
                    {
                        if (StringUtil.IsInList(strSeller, strSellerFilter) == false)
                            continue;
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
                            true,
                            out string strError);
                        if (nRet == -1)
                            throw new Exception(strError);
                        if (nRet == 0)
                            continue;
                    }

                    orders.Add(info);
                }

                sub_loader.Prompt -= new MessagePromptEventHandler(form.OnLoaderPrompt);
            }
            finally
            {
                form.ReturnChannel(channel);
            }

            // 如果书目记录下属没有任何订购记录，则也输出一个订购信息为空的行
            if (orders.Count == 0)
            {
                OutputDistributeInfo(
                    form,
location_list,
sheet,
strRecPath,
ref nLineIndex,
strTableXml,
strStyle,
biblio_col_list,
nRowIndex,
order_col_list,
"", // 表示希望获得订购记录模板
procGetOrderRecord,
ref column_max_chars);
                nRowIndex++;
            }
            else
            {
                foreach (EntityInfo order in orders)
                {
                    OutputDistributeInfo(
                        form,
    location_list,
    sheet,
    strRecPath,
    ref nLineIndex,
    strTableXml,
    strStyle,
    biblio_col_list,
    nRowIndex,
    order_col_list,
    order.OldRecPath,
    (biblio_recpath, order_recpath) =>
    {
        Debug.Assert(strRecPath == biblio_recpath, "");
        Debug.Assert(order_recpath == order.OldRecPath, "");
        return order;
    },
    ref column_max_chars);
                    nRowIndex++;
                }
            }
        }

        // “获得一个订购记录”的回调函数原型
        // parameters:
        //      strOrderRecPath 订购记录路径。如果为空，表示希望获得默认记录模板内容
        public delegate EntityInfo GetOrderRecord(string strBiblioRecPath,
            string strOrderRecPath);


        // 输出和一个订购记录有关的去向信息行
        // parameters:
        //      order   订购记录信息。如果为 null，表示订购信息为空
        static public void OutputDistributeInfo(
            MyForm form,
    List<string> location_list,
    IXLWorksheet sheet,
string strBiblioRecPath,
ref int nLineIndex,
string strTableXml,
string strStyle,
List<Order.ColumnProperty> biblio_col_list,
int nRowIndex,
List<Order.ColumnProperty> order_col_list,
// EntityInfo order,
string strOrderRecPath,
GetOrderRecord procGetOrderRecord,
        ref List<int> column_max_chars)
        {
            int nStartColIndex = 2;

            int nOldStartColIndex = nStartColIndex;

            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = form.GetTable(
                    strBiblioRecPath,
                    out strTableXml,
                    out string strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }

            // 行序号
            {
                IXLCell cell = sheet.Cell(nRowIndex + 1, nStartColIndex + 1).SetValue(nLineIndex + 1);
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                nStartColIndex++;
            }


            // 输出一行书目信息
            ExcelUtility.OutputBiblioLine(
            strBiblioRecPath,
            strTableXml,
            // ref nLineIndex,
            sheet,
            nStartColIndex,  // nColIndex,
            ColumnProperty.GetTypeList(biblio_col_list),
            nRowIndex);

            nStartColIndex += biblio_col_list.Count;

            // 获得订购记录信息
            EntityInfo order = procGetOrderRecord(strBiblioRecPath, strOrderRecPath);

            // 输出订购信息列
            if (order != null)
            {
                ExcelUtility.OutputItemLine(
order.OldRecPath,
order.OldRecord,
0,
sheet,
nStartColIndex,  // nColIndex,
ColumnProperty.GetTypeList(order_col_list),
ColumnProperty.GetDropDownList(order_col_list),
nRowIndex,
XLColor.NoColor);
            }

            nStartColIndex += order_col_list.Count;

            // 记载 cell 最大宽度
            for (int j = 0; j < nStartColIndex - nOldStartColIndex; j++)
            {
                // string col = biblio_col_list[j];
                //if (col == "recpath" || col == "书目记录路径")
                //    continue;

                IXLCell cell = sheet.Cell(nRowIndex + 1, nOldStartColIndex + j + 1);

                // 最大字符数
                PrintOrderForm.SetMaxChars(ref column_max_chars,
                nOldStartColIndex + j,
                ReaderSearchForm.GetCharWidth(cell.GetValue<string>()));
            }

            // 书目信息右边输出馆藏地列表
            {
                XmlDocument order_dom = new XmlDocument();
                order_dom.LoadXml(order != null ? order.OldRecord : "<root />");
                string strDistribute = DomUtil.GetElementText(order_dom.DocumentElement, "distribute");

                LocationCollection locations = new LocationCollection();
                int nRet = locations.Build(strDistribute, out string strError);
                if (nRet == -1)
                    throw new Exception("订购记录 '" + order.OldRecPath + "' 的去向字段格式错误:" + strError);

                List<IXLCell> cells = new List<IXLCell>();
                int i = 0;
                foreach (string location in location_list)
                {
                    int number = locations.GetLocationCopy(location);
                    // 文本为空，只要格子存在即可
                    IXLCell cell = null;

                    if (nLineIndex == -1)
                        cell = sheet.Cell(nRowIndex + 1, nStartColIndex + i + 1).SetValue(location);
                    else if (number == 0)
                        cell = sheet.Cell(nRowIndex + 1, nStartColIndex + i + 1).SetValue<string>("");
                    else
                        cell = sheet.Cell(nRowIndex + 1, nStartColIndex + i + 1).SetValue<int>(number);

                    cell.Style.Font.Bold = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cells.Add(cell);

                    i++;
                }

                if (cells.Count > 0)
                {
                    var dv1 = sheet.Range(cells[0], cells[cells.Count - 1]).SetDataValidation();
                    dv1.WholeNumber.Between(0, 1000);

                    dv1.ErrorStyle = XLErrorStyle.Warning;
                    dv1.ErrorTitle = "数字超出范围";
                    dv1.ErrorMessage = "本单元只允许输入 0 到 1000 之间的数字";
                }
            }

            nLineIndex++;
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

        public static void ImportFromOrderDistributeExcelFile(ProcessOrderRecord proc)
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
                return;

            string strError = "";
            try
            {
                using (XLWorkbook doc = new XLWorkbook(dlg.FileName))
                {
                    // 第一个 Worksheet
                    var sheet = doc.Worksheets.Worksheet(1);

                    CommandRowInfo info = new CommandRowInfo(sheet);

                    while (info.NextDataRow(sheet) == false)
                    {
                        // 保存一条订购记录
                        proc(
                            info._content_name_value_table["biblio_recpath"],
                            info._content_name_value_table["order_recpath"],
                            info.DistributeString,
                            info.GetOrderTable(),
                            info.OrderRecPathCell,
                            info.CopyCell
                            );
                    }

                    doc.Save();
                }
            }
            catch (Exception ex)
            {
                strError = "ImportFromOrderDistributeExcelFile new XLWorkbook() exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            return;
            ERROR1:
            MessageBox.Show(Program.MainForm, strError);
        }

        public class CommandRowInfo
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

            public CommandRowInfo(IXLWorksheet sheet)
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
                            location_list.Add(type.Substring("location:".Length) + ":" + cell.GetValue<string>());

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
                results.Add(prop);
            }

            return results;
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

        public ColumnProperty(string caption, string type)
        {
            this.Caption = caption;
            this.Type = type;
        }

        public static List<string> GetDropDownList(List<ColumnProperty> property_list)
        {
            List<string> results = new List<string>();
            property_list.ForEach((o) =>
            {
                results.Add(StringUtil.MakePathList(o.ValueList));
            });

            return results;
        }


        public static List<string> GetTypeList(List<ColumnProperty> property_list,
            bool bRemovePrefix = true)
        {
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

                var list = Global.FilterValuesWithLibraryCode(strLibraryCode, new List<string>(values));

                // 去掉每个元素内的 {} 部分
                list = StringUtil.FromListString(StringUtil.GetPureSelectedValue(StringUtil.MakePathList(list)));

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

            Column column = new Column();
            column.Name = "biblio_recpath -- 书目记录路径";
            column.Caption = "书目记录路径";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "biblio_title -- 题名";
            column.Caption = "题名";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "biblio_author -- 责任者";
            column.Caption = "责任者";
            column.MaxChars = -1;
            this.Columns.Add(column);


            column = new Column();
            column.Name = "biblio_publication_area -- 出版者";
            column.Caption = "出版者";
            column.MaxChars = -1;
            this.Columns.Add(column);
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

            Column column = new Column();
            column.Name = "order_recpath -- 订购记录路径";
            column.Caption = "订购记录路径";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_seller -- 渠道(书商)";
            column.Caption = "渠道(书商)";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_price -- 订购价";
            column.Caption = "订购价";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_source -- 经费来源";
            column.Caption = "经费来源";
            column.MaxChars = -1;
            this.Columns.Add(column);

#if NO
            column = new Column();
            column.Name = "range -- 时间范围";
            column.Caption = "时间范围";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "issueCount -- 包含期数";
            column.Caption = "包含期数";
            column.MaxChars = -1;
            this.Columns.Add(column);
#endif
            column = new Column();
            column.Name = "order_copy -- 复本数";
            column.Caption = "复本数";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_orderID -- 订单号";
            column.Caption = "订单号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_class -- 类别";
            column.Caption = "类别";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_batchNo -- 批次号";
            column.Caption = "批次号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_catalogNo -- 书目号";
            column.Caption = "书目号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            column = new Column();
            column.Name = "order_comment -- 附注";
            column.Caption = "附注";
            column.MaxChars = -1;
            this.Columns.Add(column);
        }
    }

}
