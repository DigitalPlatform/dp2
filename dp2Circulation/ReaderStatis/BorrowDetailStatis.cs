using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;
using DigitalPlatform;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 借阅详情。
    /// 输出读者的在借册信息和借阅历史信息，两者都放在同一个表格中，有多少行信息，每行里面都重复出现读者字段
    /// </summary>
    public class BorrowDetailStatis : ReaderStatis
    {
        public string _outputFileName = "";

        XLWorkbook _doc = null;
        IXLWorksheet _sheet = null;
        bool _launchExcel = true;
        bool _useAdvanceXml = true;
        int _rowIndex = 1;
        string _timeRange = "";

        // 累计每个列的最大字符数
        List<int> _column_max_chars = new List<int>();


        public override void OnBegin(object sender, StatisEventArgs e)
        {
            string strError = "";

            this.ClearConsoleForPureTextOutputing();
            this.XmlFormat = "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary";

            // 获得借阅历史时间范围
            // _timeRange = "20250101-20251231";
            _timeRange = InputDlg.GetInput(this.ReaderStatisForm,
                "请指定借阅历史时间范围",
                "时间范围",
                "20250101-20251231",
                this.ReaderStatisForm.Font);
            if (_timeRange == null)
                return;

            // 询问输出的 Excel 文件名
            // _outputFileName = @"c:\temp\test.xlsx";
            using (SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "请指定要输出的 Excel 文件名",
                CreatePrompt = false,
                OverwritePrompt = true,
                FileName = _outputFileName,
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                RestoreDirectory = true
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                _outputFileName = dlg.FileName;
            }

            try
            {
                // 提前保存一下，如果此时文件扩展名不正确，就能当时抛出异常
                File.Delete(_outputFileName);
                using (_doc = new XLWorkbook(XLEventTracking.Disabled))
                {
                    _doc.Worksheets.Add("表格");
                    _doc.SaveAs(_outputFileName);
                }

                File.Delete(_outputFileName);
                _doc = new XLWorkbook(XLEventTracking.Disabled);
            }
            catch (Exception ex)
            {
                strError = "new XLWorkbook() exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            _sheet = _doc.Worksheets.Add("表格");

            OutputTitle();
            return;
        ERROR1:
            MessageBox.Show(this.ReaderStatisForm, strError);
            e.Continue = ContinueType.SkipAll;
            return;
        }



        public override void OnRecord(object sender, StatisEventArgs e)
        {
            var dom = this.ReaderDom;

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            string readerType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");
            string deparatment = DomUtil.GetElementText(dom.DocumentElement,
    "department");

            var common_cols = new List<string>()
            {
                strBarcode,
                strName,
                readerType,
                deparatment,
            };

            // 输出在借信息
            OutputBorrows(common_cols);

            // 输出借阅历史信息
            var looping = this.ReaderStatisForm.Looping(out LibraryChannel channel);
            try
            {
                ChargingHistoryLoader history_loader = new ChargingHistoryLoader();
                history_loader.Channel = channel;
                history_loader.Stop = looping.Progress;
                history_loader.PatronBarcode = strBarcode;
                history_loader.TimeRange = ChargingHistoryLoader.GetTimeRange(_timeRange);
                history_loader.Actions = "return,lost,transferIdTo:itemBarcode|readerBarcode";
                history_loader.Order = "descending";

                CacheableBiblioLoader summary_loader = new CacheableBiblioLoader();
                summary_loader.Channel = channel;
                summary_loader.Stop = looping.Progress;
                summary_loader.Format = "summary";
                summary_loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                // summary_loader.RecPaths = biblio_recpaths;

                // 输出借阅历史表格
                // 可能会抛出异常，例如权限不够
                OutputBorrowHistory(_sheet,
        this.ReaderDom,
        history_loader,
        summary_loader,
        common_cols,
        ref _rowIndex,
        ref _column_max_chars);
            }
            catch (ChannelException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string strErrorText = "输出借阅历史时出现异常: " + ex.Message;
                throw new Exception(strErrorText, ex);
            }
            finally
            {
                looping.Dispose();
            }
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            /*
            if (sheet != null)
                ClosedXmlUtil.AdjustColumnWidth(sheet,
                column_max_chars,
                50);
            */

            if (_doc != null)
            {
                _doc.SaveAs(_outputFileName);
                _doc.Dispose();

                if (_launchExcel)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(_outputFileName);
                    }
                    catch
                    {

                    }
                }
            }
        }



        static string ToLocalTime(string strRfc1123, string strFormat)
        {
            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123, strFormat);
            }
            catch (Exception ex)
            {
                return "时间字符串 '" + strRfc1123 + "' 格式不正确: " + ex.Message;
            }
        }

        static string GetDisplayTimePeriodString(string strText)
        {
            strText = strText.Replace("day", "天");

            return strText.Replace("hour", "小时");
        }

        void OutputTitle()
        {
            // 册信息若干行的标题
            {
                List<string> titles = new List<string>();

                titles.Add("证条码号,15");
                titles.Add("姓名,10");
                titles.Add("读者类型,10");
                titles.Add("单位,20");

                titles.Add("册条码号,10");
                titles.Add("书目摘要,30");
                titles.Add("借阅日期,20");
                titles.Add("借期,10");
                titles.Add("应还日期,20");
                titles.Add("还书日期,20");

                int nColIndex = 1;
                foreach (string s in titles)
                {
                    var parts = s.Split(new char[] { ',' }, 2);

                    if (string.IsNullOrEmpty(parts[1]) == false
                        && Int32.TryParse(parts[1], out int chars) == true)
                    {
                        var col = _sheet.Column(nColIndex);
                        col.Width = chars;
                    }

                    IXLCell cell = _sheet.Cell(_rowIndex, nColIndex).SetValue(parts[0]);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    //cell.Style.Font.Bold = true;
                    //cell.Style.Font.FontColor = XLColor.DarkGray;
                    //cell.Style.Font.FontName = strFontName;
                    //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                    // cells.Add(cell);
                }
                _rowIndex++;
            }
        }

        void OutputBorrows(IEnumerable<string> common_cols)
        {
            // 在借的册
            XmlNodeList borrow_nodes = this.ReaderDom.DocumentElement.SelectNodes("borrows/borrow");
            foreach (XmlElement borrow in borrow_nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                string strBorrowDate = ToLocalTime(borrow.GetAttribute("borrowDate"), "yyyy-MM-dd HH:mm:ss");
                string strBorrowPeriod = GetDisplayTimePeriodString(borrow.GetAttribute("borrowPeriod"));
                string strReturningDate = ToLocalTime(borrow.GetAttribute("returningDate"), "yyyy-MM-dd HH:mm:ss");    // 注意这里显示时分，只有当 period 为 hour 时才有必要
                string strRecPath = borrow.GetAttribute("recPath");
                string strIsOverdue = borrow.GetAttribute("isOverdue");
                bool bIsOverdue = DomUtil.IsBooleanTrue(strIsOverdue, false);
                string strOverdueInfo = borrow.GetAttribute("overdueInfo1");

                if (_useAdvanceXml == false)
                {
                    string strPeriod = borrow.GetAttribute("borrowPeriod");
                    string strRfc1123String = borrow.GetAttribute("returningDate");

                    if (string.IsNullOrEmpty(strRfc1123String) == false)
                    {
                        try
                        {
                            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strRfc1123String);
                            TimeSpan delta = DateTime.Now - time.ToLocalTime();
                            if (strPeriod.IndexOf("hour") != -1)
                            {
                                if (delta.Hours > 0)
                                {
                                    strOverdueInfo = "已超期 " + delta.Hours + " 小时";
                                    bIsOverdue = true;
                                }
                            }
                            else
                            {
                                if (delta.Days > 0)
                                {
                                    strOverdueInfo = "已超期 " + delta.Days + " 天";
                                    bIsOverdue = true;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }



                string strSummary = borrow.GetAttribute("summary");
                if (string.IsNullOrEmpty(strItemBarcode) == false
                    && string.IsNullOrEmpty(strSummary) == true)
                {
                    string strError = "";
                    int nRet = this.ReaderStatisForm.GetBiblioSummary(
                        strItemBarcode,
    strRecPath,
    null,
    out _,
    out strSummary,
    out strError);
                    /*
                    int nRet = procGetBiblioSummary(strItemBarcode,
                        strRecPath, // strConfirmItemRecPath,
                        false,
                        out strSummary,
                        out strError);
                    */
                    if (nRet == -1)
                        strSummary = strError;
                }

                List<string> cols = new List<string>();
                cols.AddRange(common_cols);

                // 册条码号
                cols.Add(strItemBarcode);
                // 书目摘要
                cols.Add(strSummary);
                // 借阅时间
                cols.Add(strBorrowDate);
                // 借期
                cols.Add(strBorrowPeriod);
                // 应还日期
                cols.Add(strReturningDate);

                // 是否超期
                //if (bIsOverdue)
                //    cols.Add(strOverdueInfo);
                //else
                //    cols.Add("");

                int nColIndex = 1;
                foreach (string s in cols)
                {
                    // 统计最大字符数
                    ClosedXmlUtil.SetMaxChars(_column_max_chars, 
                        nColIndex - 1,
                        s);

                    IXLCell cell = null;
                    cell = _sheet.Cell(_rowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    nColIndex++;
                    // cells.Add(cell);
                }

                // 超期的行为黄色背景
                if (bIsOverdue)
                {
                    var line = _sheet.Range(_rowIndex, 2, _rowIndex, 2 + cols.Count - 1);
                    line.Style.Fill.BackgroundColor = XLColor.Yellow;
                }

                _rowIndex++;
            }

        }

        // parameters:
        //      bAdvanceXml 是否为 AdvanceXml 情况
        static void OutputBorrowHistory(
            IXLWorksheet sheet,
            XmlDocument reader_dom,
            ChargingHistoryLoader history_loader,
            CacheableBiblioLoader summary_loader,
            IEnumerable<string> common_cols,
            ref int nRowIndex,
            ref List<int> column_max_chars)
        {
            int nStartRow = nRowIndex;

            string readerBarcode = DomUtil.GetElementText(reader_dom.DocumentElement,
                "barcode");

            List<string> item_barcodes = new List<string>();
            List<Point> points = new List<Point>();
            foreach (ChargingItemWrapper wrapper in history_loader)
            {
                if (wrapper.Item.Action != "return")
                    continue;
                ChargingItem item = wrapper.Item;
                ChargingItem rel = wrapper.RelatedItem;

                string strItemBarcode = item.ItemBarcode;
                string strBorrowDate = item.BorrowDate;
                if (string.IsNullOrEmpty(strBorrowDate)
                    && rel != null)
                    strBorrowDate = rel.OperTime;
                string strBorrowPeriod = GetDisplayTimePeriodString(rel == null ? "" : rel.Period);
                string strReturnDate = item.OperTime;

                string strReturningDate = "";
                if (rel != null
                    && string.IsNullOrEmpty(rel.Period) == false
                    && DateTime.TryParse(strBorrowDate, out DateTime value) == true)
                {
                    strReturningDate = (value + GetTimeLength(rel.Period)).ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (history_loader.Stop != null)
                    history_loader.Stop.SetMessage($"装载读者 {readerBarcode} 的借阅历史 {strItemBarcode} {strBorrowDate} {strBorrowPeriod} {strReturnDate} ...");

                string strSummary = "";
#if NO
                if (string.IsNullOrEmpty(strItemBarcode) == false
                    && string.IsNullOrEmpty(strSummary) == true)
                {
                    string strError = "";
                    int nRet = procGetBiblioSummary(strItemBarcode,
                        "", // strConfirmItemRecPath,
                        false,
                        out strSummary,
                        out strError);
                    if (nRet == -1)
                        strSummary = strError;
                }
#endif
                item_barcodes.Add("@itemBarcode:" + strItemBarcode);

                List<string> cols = new List<string>();
                // 读者信息列
                cols.AddRange(common_cols);

                // 册条码号
                cols.Add(strItemBarcode);
                // 书目摘要
                cols.Add(strSummary);

                // 借阅日期
                cols.Add(strBorrowDate);
                // 期限
                cols.Add(strBorrowPeriod);

                // 应还日期
                cols.Add(strReturningDate);

                // 借阅操作者
                //cols.Add(rel == null ? "" : rel.Operator);

                // 还书时间
                cols.Add(strReturnDate);
                // 还书操作者
                //cols.Add(item.Operator);

                int nColIndex = 1;
                points.Add(new Point(nColIndex + common_cols.Count() + 1, nRowIndex));
                foreach (string s in cols)
                {
                    // 统计最大字符数
                    ClosedXmlUtil.SetMaxChars(column_max_chars,
                        nColIndex - 1,
                        s);

                    var cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    //cell.Style.Font.FontName = strFontName;
                    //cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                    // cells.Add(cell);
                }

                nRowIndex++;
            }

            // 加入书目摘要
            summary_loader.RecPaths = item_barcodes;
            int i = 0;
            foreach (BiblioItem biblio in summary_loader)
            {
                if (summary_loader.Stop != null)
                    summary_loader.Stop.SetMessage($"装载读者 {readerBarcode} 借阅历史中 {biblio.RecPath} 的书目摘要 ...");

                Point point = points[i];
                int nColIndex = point.X;
                // 统计最大字符数
                ClosedXmlUtil.SetMaxChars(column_max_chars,
                    nColIndex - 1,
                    biblio.Content);

                var cell = sheet.Cell(point.Y, nColIndex).SetValue(biblio.Content);
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                i++;
            }
        }
        static TimeSpan GetTimeLength(string strPeriod)
        {
            var ret = DateTimeUtil.ParsePeriodUnit(strPeriod,
    "day",
    out long value,
    out string strUnit,
    out string strError);
            if (ret == -1)
                throw new ArgumentException($"时间长度值 '{strPeriod}' 不合法");

            if (strUnit == "day")
                return new TimeSpan((int)value, 0, 0, 0);
            else if (strUnit == "hour")
                return new TimeSpan((int)value, 0, 0);
            else
            {
                throw new ArgumentException("未知的时间单位 '" + strUnit + "'");
            }
        }

    }
}
