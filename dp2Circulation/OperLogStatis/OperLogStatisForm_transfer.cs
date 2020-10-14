using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    partial class OperLogStatisForm
    {
        #region “典藏移交”内置统计方案

        // 典藏移交清单。内置统计方案
        // return:
        //      -1  出错
        //      0   成功
        //      1   用户中断
        int TransferList(out string strError)
        {
            strError = "";

            var items = new List<TransferItem>();

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在执行脚本 ...");
            stop.BeginLoop();
            try
            {
                // 搜集信息
                int nRet = DoLoop((string strLogFileName,
                    string strXml,
                    bool bInCacheFile,
                    long lHint,
                    long lIndex,
                    long lAttachmentTotalLength,
                    object param,
                    out string strError1) =>
                {
                    strError1 = "";

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(strXml);

                    // 搜集全部相关日志记录
                    string operation = DomUtil.GetElementText(dom.DocumentElement, "operation");
                    if (operation != "setEntity")
                        return 0;

                    string action = DomUtil.GetElementText(dom.DocumentElement, "action");
                    if (action != "transfer")
                        return 0;

                    var item = new TransferItem();
                    item.BatchNo = DomUtil.GetElementText(dom.DocumentElement, "batchNo");
                    item.Operator = DomUtil.GetElementText(dom.DocumentElement, "operator");
                    string operTime = DomUtil.GetElementText(dom.DocumentElement, "operTime");
                    item.OperTime = DateTimeUtil.FromRfc1123DateTimeString(operTime).ToLocalTime();

                    XmlDocument old_itemdom = new XmlDocument();
                    old_itemdom.LoadXml(DomUtil.GetElementText(dom.DocumentElement, "oldRecord"));

                    item.SourceLocation = DomUtil.GetElementText(old_itemdom.DocumentElement, "location");

                    string new_xml = DomUtil.GetElementText(dom.DocumentElement, "record", out XmlNode node);
                    XmlDocument new_itemdom = new XmlDocument();
                    new_itemdom.LoadXml(new_xml);

                    item.TargetLocation = DomUtil.GetElementText(new_itemdom.DocumentElement, "location");
                    item.Barcode = DomUtil.GetElementText(new_itemdom.DocumentElement, "barcode");
                    item.RecPath = ((XmlElement)node).GetAttribute("recPath");
                    item.NewXml = new_xml;
                    item.Style = DomUtil.GetElementText(dom.DocumentElement, "style");
                    items.Add(item);
                    return 0;
                },
                    out strError);
                if (nRet == -1 || nRet == 1)
                    return nRet;

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            // 让用户选择需要统计的范围。根据批次号、目标位置来进行选择
            var list = items.GroupBy(
                x => new { x.BatchNo, x.TargetLocation },
                (key, item_list) => new TransferGroup
                {
                    BatchNo = key.BatchNo,
                    TargetLocation = key.TargetLocation,
                    Items = new List<TransferItem>(item_list)
                }).ToList();

            List<TransferGroup> groups = null;
            bool output_one_sheet = true;
            using (var dlg = new SelectOutputRangeDialog())
            {
                dlg.Font = this.Font;
                dlg.Groups = list;


                dlg.UiState = Program.MainForm.AppInfo.GetString(
"OperLogStatisForm",
"SelectOutputRangeDialog_uiState",
"");
                Program.MainForm.AppInfo.LinkFormState(dlg, "SelectOutputRangeDialog_formstate");
                dlg.ShowDialog(this);

                Program.MainForm.AppInfo.SetString(
"OperLogStatisForm",
"SelectOutputRangeDialog_uiState",
dlg.UiState);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return 1;
                groups = dlg.SelectedGroups;
                output_one_sheet = dlg.OutputOneSheet;
            }

            this.ShowMessage("正在创建 Excel 报表 ...");

            string fileName = "";
            // 创建 Excel 报表
            // 询问文件名
            using (SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "请指定要输出的 Excel 文件名",
                CreatePrompt = false,
                OverwritePrompt = true,
                // dlg.FileName = this.ExportExcelFilename;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                RestoreDirectory = true
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                fileName = dlg.FileName;

                XLWorkbook doc = null;
                try
                {
                    doc = new XLWorkbook(XLEventTracking.Disabled);
                    File.Delete(dlg.FileName);
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                if (output_one_sheet)
                {
                    CreateSheet(doc,
    "典藏移交清单",
    groups);
                }
                else
                {
                    int count = groups.Count;
                    int i = 0;
                    foreach (var group in groups)
                    {
                        CreateSheet(doc,
                            $"{++i} {count} {group.BatchNo}-{group.TargetLocation}",
                            // Cut($"({++i} of {count})", 31),
                            new List<TransferGroup> { group });
                    }
                }

                doc.SaveAs(dlg.FileName);

            }

            this.ClearMessage();

            try
            {
                System.Diagnostics.Process.Start(fileName);
            }
            catch
            {

            }
            return 0;
        }

        static string Cut(string text, int length)
        {
            if (text.Length > length)
                return text.Substring(0, length);
            return text;
        }

        static string GetSheetName(string text)
        {
            StringBuilder result = new StringBuilder();
            foreach (var ch in text)
            {
                if (":\\/?*[]".IndexOf(ch) == -1)
                    result.Append(ch);
            }

            return result.ToString();
        }

        void WriteBiblioColumns(
            string strEntityRecPath,
            string strParentID,
            List<Order.ColumnProperty> biblio_title_list,
            IXLWorksheet sheet,
            int col,
            int row)
        {
            string strBiblioRecPath = Program.MainForm.BuildBiblioRecPath(
    "item",
    strEntityRecPath,
    strParentID);
            if (string.IsNullOrEmpty(strBiblioRecPath))
            {
                throw new Exception("获取对应的书目记录路径时出错");
            }

            List<string> type_list1 = new List<string>();
            List<string> type_list2 = new List<string>();
            biblio_title_list.ForEach(o =>
            {
                if (o.Type.StartsWith("biblio_"))
                {
                    string type = o.Type.Substring("biblio_".Length);
                    type_list1.Add(type);
                    type_list2.Add(type);
                }
                else
                    type_list2.Add("");
            });
            string styleList = StringUtil.MakePathList(type_list1);

            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = this.GetTable(
                strBiblioRecPath,
                styleList,
                out string strTableXml,
                out string strError1);
            if (nRet == -1)
                throw new Exception(strError1);

            // 输出一行书目信息
            var cells = ExcelUtility.OutputBiblioLine(
            strBiblioRecPath,
            strTableXml,
            sheet,
            col,
            type_list2,
            row);

            foreach (var cell in cells)
            {
                cell.Style.Alignment.WrapText = true;
            }
        }

        // 创建一个 Sheet
        IXLWorksheet CreateSheet(XLWorkbook doc,
            string sheet_name,
            List<TransferGroup> groups)
        {
            IXLWorksheet sheet = doc.Worksheets.Add(Cut(GetSheetName(sheet_name), 31));

            // 每个列的最大字符数
            // List<int> column_max_chars = new List<int>();

            // 准备书目列标题
            var biblio_column_option = new TransferColumnOption(Program.MainForm.UserDir);
            biblio_column_option.LoadData(Program.MainForm.AppInfo,
            ColumnDefPath);

            List<Order.ColumnProperty> biblio_title_list = Order.DistributeExcelFile.BuildList(biblio_column_option);

            List<string> headers = new List<string>();
            biblio_title_list.ForEach(o => headers.Add(o.Caption));

            int nRowIndex = 1;
            int nColIndex = 1;

            // 输出表格标题
            {
                var range = sheet.Range(nRowIndex, nColIndex, nRowIndex, nColIndex + headers.Count - 1);
                range.Merge();
                range.SetValue(sheet_name);
                range.Style.Alignment.WrapText = true;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Font.Bold = true;
                nRowIndex++;
            }

            int lines = 0;
            groups.ForEach(o => lines += o.Items.Count);
            {
                var range = sheet.Range(nRowIndex, nColIndex, nRowIndex, nColIndex + headers.Count - 1);
                range.Merge();
                range.SetValue($"行数: {lines}, 打印日期: {DateTime.Now.ToString("yyyy-M-d")}");
                range.Style.Alignment.WrapText = true;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                nRowIndex++;
            }

            nRowIndex++;
            foreach (string header in headers)
            {
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(header, '*'));
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                //cell.Style.Font.FontName = strFontName;
                //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                nColIndex++;
            }
            {
                // 设置边框
                var range = sheet.Range(nRowIndex, 1, nRowIndex, biblio_title_list.Count);
                range.Style.Border.SetBottomBorderColor(XLColor.Black);
                range.Style.Border.SetBottomBorder(XLBorderStyleValues.Medium);
            }

            nRowIndex++;

            // 用于发现重复的册
            // recpath --> (此路径的第一个) TransferItem
            Hashtable recpath_table = new Hashtable();
            int dup_count = 0;      // 记录路径发生重复的行数
            int notchange_count = 0;    // 馆藏地没有发生改变的行数

            int nNo = 1;
            foreach (var group in groups)
            {
                foreach (var item in group.Items)
                {
                    bool dup = false;
                    if (recpath_table.ContainsKey(item.RecPath))
                    {
                        dup = true;
                        dup_count++;
                    }
                    else
                        recpath_table[item.RecPath] = item;

                    bool onlyWriteLog = StringUtil.IsInList("onlyWriteLog", item.Style);

                    nColIndex = 1;
                    // WriteCell(nNo.ToString());
                    // WriteCell(item.Barcode);

                    // 获得册记录
                    XmlDocument itemdom = new XmlDocument();
                    itemdom.LoadXml(item.NewXml);
                    string strParentID = DomUtil.GetElementText(itemdom.DocumentElement, "parent");

                    // 输出书目列
                    WriteBiblioColumns(
                        item.RecPath,
                        strParentID,
                        biblio_title_list,
                        sheet,
                        nColIndex - 1,
                        nRowIndex - 1);

                    OutputTransferColumns(
                        nNo,
                        item,
                        sheet,
                        nColIndex - 1,
                        Order.ColumnProperty.GetTypeList(biblio_title_list, false),
                        nRowIndex - 1);

                    /*
                    nColIndex += biblio_title_list.Count;

                    WriteCell(item.SourceLocation);
                    WriteCell(item.TargetLocation);
                    WriteCell(item.BatchNo);
                    WriteCell(item.OperTime.ToString());
                    WriteCell(item.Operator);
                    */

                    // 设置边框
                    var range = sheet.Range(nRowIndex, 1, nRowIndex, biblio_title_list.Count);
                    range.Style.Border.SetBottomBorderColor(XLColor.Black);
                    range.Style.Border.SetBottomBorder(XLBorderStyleValues.Hair);

                    if (dup)
                        range.Style.Fill.BackgroundColor = XLColor.Yellow;

                    if (onlyWriteLog)
                    {
                        notchange_count++;
                        // 寻找 SourceLocation 和 TargetLocation 列
                        {
                            int index = biblio_title_list.FindIndex(o => o.Type == "log_sourceLocation");
                            if (index != -1)
                                sheet.Cell(nRowIndex, 1 + index).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        }

                        {
                            int index = biblio_title_list.FindIndex(o => o.Type == "log_targetLocation");
                            if (index != -1)
                                sheet.Cell(nRowIndex, 1 + index).Style.Fill.BackgroundColor = XLColor.LightBlue;
                        }
                    }

                    nRowIndex++;
                    nNo++;
                }
            }

            // 警告行
            if (dup_count > 0)
            {
                nRowIndex++;
                var range = sheet.Range(nRowIndex, nColIndex, nRowIndex, nColIndex + headers.Count - 1);
                range.Merge();

                range.Style.Fill.BackgroundColor = XLColor.DarkGray;
                range.Style.Font.FontColor = XLColor.White;
                range.Style.Font.Bold = true;

                range.SetValue($"警告：表中有 {dup_count} 个重复的册记录行。这些行已显示为黄色背景色");
                range.Style.Alignment.WrapText = true;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                nRowIndex++;
            }

            // 警告行
            if (notchange_count > 0)
            {
                var range = sheet.Range(nRowIndex, nColIndex, nRowIndex, nColIndex + headers.Count - 1);
                range.Merge();

                range.Style.Fill.BackgroundColor = XLColor.DarkGray;
                range.Style.Font.FontColor = XLColor.White;

                range.SetValue($"提醒：表中有 {notchange_count} 个册记录行在移交操作中馆藏地并没有发生变化。这些行的源、目标馆藏地列已显示为蓝色背景色");
                range.Style.Alignment.WrapText = true;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                nRowIndex++;
            }

            // TODO: 设置 header 和 footer
            // https://stackoverflow.com/questions/34104107/closedxml-change-existing-footer-header

            double char_width = DigitalPlatform.dp2.Statis.ClosedXmlUtil.GetAverageCharPixelWidth(Program.MainForm);

            // 字符数太多的列不要做 width auto adjust
            const int MAX_CHARS = 30;   // 60
            int i = 0;
            foreach (IXLColumn column in sheet.ColumnsUsed())
            {
                int nChars = GetMaxChars(column);

                column.Width = Math.Min(nChars, MAX_CHARS);

#if NO
                if (nChars < MAX_CHARS)
                {
                    // column.AdjustToContents();
                    column.Width = nChars;
                }
                else
                {
                    int nColumnWidth = 100;
                    /*
                    // 2020/1/6 增加保护判断
                    if (i >= 0 && i < list.Columns.Count)
                        nColumnWidth = list.Columns[i].Width;
                    */
                    column.Width = (double)nColumnWidth / char_width;  // Math.Min(MAX_CHARS, nChars);
                }
#endif

                i++;
            }

            return sheet;

#if NO
            void WriteCell(string text)
            {
                // 统计最大字符数
                // int nChars = column_max_chars[nColIndex - 1];
                if (text != null)
                {
                    DigitalPlatform.dp2.Statis.ClosedXmlUtil.SetMaxChars(column_max_chars, nColIndex - 1, text.Length);
                }
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(text, '*'));
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                // cell.Style.Font.FontName = strFontName;
                /*
                if (nColIndex - 1 < alignments.Count)
                    cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                else
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                */
                nColIndex++;
            }
#endif
        }

        // 统计一列里面所有单元的最大字符数。注：字符数约定按照西文字符数计算，一个汉字等于两个西文字符
        static int GetMaxChars(IXLColumn column)
        {
            int max = 0;
            foreach (IXLCell cell in column.CellsUsed())
            {
                // 跳过 Merged 的 Cell。也就是表格标题
                if (cell.IsMerged())
                    continue;

                string text = cell.GetString();
                int current = GetCharWidth(text);
                if (current > max)
                    max = current;
            }

            return max;
        }

        // 计算一个字符串的“西文字符宽度”。汉字相当于两个西文字符宽度
        static int GetCharWidth(string strText)
        {
            int result = 0;
            foreach (char c in strText)
            {
                result += StringUtil.IsHanzi(c) == true ? 2 : 1;
            }

            return result;
        }

        static string GetPropertyOrField(object obj, string name)
        {
            var pi = obj.GetType().GetProperty(name);
            if (pi != null)
                return (string)pi.GetValue(obj);

            var fi = obj.GetType().GetField(name);
            if (fi == null)
                return null;
            return (string)fi.GetValue(obj);
        }

        // 输出一行日志或册信息
        static void OutputTransferColumns(
            int no,
            TransferItem transfer_item,
    IXLWorksheet sheet,
    int nStartColIndex,     // 从 0 开始计数
    List<string> col_list,
    int nRowIndex)  // 从 0 开始计数。
        {
            string strError = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(transfer_item.NewXml);
            }
            catch (Exception ex)
            {
                // 2019/12/3
                strError = $"!error: 装载册记录 XML 到 DOM 时出错: {ex.Message}";
                dom.LoadXml("<root />");
            }

            List<IXLCell> cells = new List<IXLCell>();

            int i = 0;
            foreach (string col in col_list)
            {
                string strValue = "";
                if (col == "log_no")
                    strValue = no.ToString();
                else if (col == "log_operTime")
                    strValue = transfer_item.OperTime.ToString();
                else if (col == "item_recPath")
                    strValue = transfer_item.RecPath;
                else if (col.StartsWith("log_"))
                {
                    string name = col.Substring("log_".Length).Trim();
                    // 把第一个字母大写
                    name = char.ToUpper(name[0]) + name.Substring(1);
                    strValue = GetPropertyOrField(transfer_item, name);
                    if (strValue == null)
                    {
                        i++;
                        continue;
                    }
                }
                else if (col.StartsWith("item_"))
                {
                    string name = col.Substring("item_".Length).Trim();
                    strValue = DomUtil.GetElementText(dom.DocumentElement, name);
                }
                else
                {
                    i++;
                    continue;
                }


                {
                    IXLCell cell = sheet.Cell(nRowIndex + 1, nStartColIndex + (i++) + 1).SetValue(strValue);

                    // 统计最大字符数
                    // DigitalPlatform.dp2.Statis.ClosedXmlUtil.SetMaxChars(column_max_chars, cell.Address.ColumnNumber - 1, strValue?.Length);

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


#if NO
        string GetItemXml(string strRecPath)
        {
            var channel = this.GetChannel();
            try
            {
                long lRet = channel.GetItemInfo(
    stop,
    "@path:" + strRecPath,
    "xml",
    out string strXml,
    out string strOutputRecPath,
    out byte [] baTimestamp,
    "",
    out string strBiblio,
    out string strBiblioRecPath,
    out string strError);
                if (lRet == -1)
                    return null;
                return strXml;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }
#endif

#if NO
        int _transferList(string strLogFileName,
    string strXml,
    bool bInCacheFile,
    long lHint,
    long lIndex,
    long lAttachmentTotalLength,
    object param,
    out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            string strDate = "";
            int nRet = strLogFileName.IndexOf(".");
            if (nRet != -1)
                strDate = strLogFileName.Substring(0, nRet);
            else
                strDate = strLogFileName;

            DateTime currentDate = DateTimeUtil.Long8ToDateTime(strDate);


            return 0;
        }
#endif

        #endregion

        static string ColumnDefPath
        {
            get
            {
                return "column_" + typeof(TransferColumnOption).ToString();
            }
        }



        // 选项。可为内置统计方案设置参数
        private void button_option_Click(object sender, EventArgs e)
        {
            string name = this.comboBox_projectName.Text;
            switch (name)
            {
                case "#典藏移交清单":
                    {
                        TransferColumnOption option = new TransferColumnOption(Program.MainForm.UserDir);
                        option.LoadData(Program.MainForm.AppInfo,
                            ColumnDefPath);

                        PrintOptionDlg dlg = new PrintOptionDlg();
                        MainForm.SetControlFont(dlg, this.Font, false);
                        dlg.HidePage("tabPage_normal");
                        dlg.HidePage("tabPage_templates");

                        dlg.Text = "输出列配置";
                        dlg.PrintOption = option;
                        dlg.DataDir = Program.MainForm.UserDir;
                        dlg.ColumnItems = option.GetAllColumnItems();

                        dlg.UiState = Program.MainForm.AppInfo.GetString(
            "OperLogStatisForm",
            "columnDialog_uiState",
            "");
                        Program.MainForm.AppInfo.LinkFormState(dlg, "OperLogStatisForm_transferOption_formstate");
                        dlg.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(dlg);

                        Program.MainForm.AppInfo.SetString(
            "OperLogStatisForm",
            "columnDialog_uiState",
            dlg.UiState);

                        if (dlg.DialogResult != DialogResult.OK)
                            return;

                        option.SaveData(Program.MainForm.AppInfo,
                            ColumnDefPath);
                    }
                    break;
            }
        }

    }

    // 适合导出册信息的书目列定义
    internal class TransferColumnOption : Order.BiblioColumnOption
    {
        public TransferColumnOption(string strDataDir) : base(strDataDir, "")
        {
            this.DataDir = strDataDir;

            // Columns缺省值
            Columns.Clear();
            this.Columns.AddRange(GetAllColumns(true));
        }

        public override List<Column> GetAllColumns(bool bDefault)
        {
            string[] lines;
            if (bDefault)
            {
                // 缺省的
                lines = new string[] {
                    "log_no -- 序号",
                    "log_barcode -- 册条码号",

                    "biblio_title -- 题名",
                    "biblio_author -- 责任者",
                    "biblio_publisher -- 出版者",
                    "biblio_publishtime -- 出版时间",

                    "log_sourceLocation -- 源馆藏地",
                    "log_targetLocation -- 目标馆藏地",
                    "log_batchNo -- 批次号",
                    "log_operTime -- 操作时间",
                    "log_operator -- 操作者",
                    "log_recPath -- 册记录路径",

                    "item_price -- 册价格",
                    "item_volume -- 卷册号",
                    "item_accessNo -- 索取号",
                };
            }
            else
            {
                // 全部
                lines = new string[] {
                    // TransferItem 成员
                    "log_no -- 序号",
                    "log_barcode -- 册条码号",
                    "log_sourceLocation -- 源馆藏地",
                    "log_targetLocation -- 目标馆藏地",
                    "log_batchNo -- 批次号",
                    "log_operTime -- 操作时间",
                    "log_operator -- 操作者",
                    "log_recPath -- 册记录路径",

                    // 书目列
                    "biblio_recpath -- 书目记录路径",
                    "biblio_title -- 题名",
                    "biblio_titlepinyin -- 题名拼音",
                    "biblio_author -- 责任者",
                    "biblio_title_area -- 题名与责任者",
                    "biblio_edition_area -- 版本项",
                    "biblio_material_specific_area -- 资料特殊细节项",
                    "biblio_publication_area -- 出版发行项",
                    "biblio_material_description_area -- 载体形态项",
                    "biblio_material_series_area -- 丛编项",
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
                    "biblio_author_accesspoint -- 责任者检索点",
                    // 册列
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
            }

            List<Column> results = new List<Column>();

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
