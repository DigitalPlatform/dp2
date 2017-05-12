using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
// using System.Windows.Forms.VisualStyles;
using System.Drawing;

using ClosedXML.Excel;

using DigitalPlatform.Xml;

namespace DigitalPlatform.dp2.Statis
{
    public class ClosedXmlUtil
    {
        public static double GetAverageCharPixelWidth(Control control)
        {
            StringBuilder sb = new StringBuilder();

            // Using the typical printable range
            for (int i = 32; i < 127; i++)
            {
                sb.Append((char)i);
            }

            string printableChars = sb.ToString();

            // Choose your font
            Font stringFont = control.Font;

            // Now pass printableChars into MeasureString
            SizeF stringSize = new SizeF();
            using (Graphics g = Graphics.FromHwnd(control.Handle))
            {
                stringSize = g.MeasureString(printableChars, stringFont);
            }

            // Work out average width of printable characters
            return stringSize.Width / (double)printableChars.Length;
        }

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        public static int ExportToExcel(
            Stop stop,
            List<ListViewItem> items,
            out string strError)
        {
            strError = "";
            if (items == null || items.Count == 0)
            {
                strError = "items == null || items.Count == 0";
                return -1;
            }
            ListView list = items[0].ListView;

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            // dlg.FileName = this.ExportExcelFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return 0;

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

            IXLWorksheet sheet = null;
            sheet = doc.Worksheets.Add("表格");
            // sheet.Style.Font.FontName = this.Font.Name;

            if (stop != null)
                stop.SetProgressRange(0, items.Count);

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();

            List<XLAlignmentHorizontalValues> alignments = new List<XLAlignmentHorizontalValues>();
            foreach (ColumnHeader header in list.Columns)
            {
                if (header.TextAlign == HorizontalAlignment.Center)
                    alignments.Add(XLAlignmentHorizontalValues.Center);
                else if (header.TextAlign == HorizontalAlignment.Right)
                    alignments.Add(XLAlignmentHorizontalValues.Right);
                else
                    alignments.Add(XLAlignmentHorizontalValues.Left);

                column_max_chars.Add(0);
            }


            string strFontName = list.Font.FontFamily.Name;

            int nRowIndex = 1;
            int nColIndex = 1;
            foreach (ColumnHeader header in list.Columns)
            {
                IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(header.Text, '*'));
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontName = strFontName;
                cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                nColIndex++;
            }
            nRowIndex++;

            //if (stop != null)
            //    stop.SetMessage("");
            foreach (ListViewItem item in items)
            {
                Application.DoEvents();

                if (stop != null
    && stop.State != 0)
                {
                    strError = "用户中断";
                    return 0;
                }

                // List<CellData> cells = new List<CellData>();

                nColIndex = 1;
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    // 统计最大字符数
                    int nChars = column_max_chars[nColIndex - 1];
                    if (subitem.Text != null && subitem.Text.Length > nChars)
                    {
                        column_max_chars[nColIndex - 1] = subitem.Text.Length;
                    }
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(subitem.Text, '*'));
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.FontName = strFontName;
                    cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                }

                if (stop != null)
                    stop.SetProgressValue(nRowIndex-1);

                nRowIndex++;
            }

            if (stop != null)
                stop.SetMessage("正在调整列宽度 ...");
            Application.DoEvents();

            double char_width = GetAverageCharPixelWidth(list);

            // 字符数太多的列不要做 width auto adjust
            const int MAX_CHARS = 30;   // 60
            int i = 0;
            foreach (IXLColumn column in sheet.Columns())
            {
                int nChars = column_max_chars[i];
                if (nChars < MAX_CHARS)
                    column.AdjustToContents();
                else
                    column.Width = (double)list.Columns[i].Width / char_width;  // Math.Min(MAX_CHARS, nChars);
                i++;
            }

            // sheet.Columns().AdjustToContents();

            // sheet.Rows().AdjustToContents();

            doc.SaveAs(dlg.FileName);
            doc.Dispose();

            try
            {
                System.Diagnostics.Process.Start(dlg.FileName);
            }
            catch
            {

            }
            return 1;
        }
    }
}
