﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
// using System.Windows.Forms.VisualStyles;
using System.Drawing;

using ClosedXML.Excel;

using DigitalPlatform.Xml;
using DigitalPlatform.Core;
using System.Diagnostics;

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


        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentOutOfRangeException
Message: 索引超出范围。必须为非负值并小于集合大小。
参数名: index
Stack:
在 System.ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
在 System.Collections.Generic.List`1.get_Item(Int32 index)
在 DigitalPlatform.dp2.Statis.ClosedXmlUtil.ExportToExcel(Stop stop, List`1 items, String& strError)
在 dp2Circulation.ItemSearchForm.menu_exportExcelFile_Click(Object sender, EventArgs e)
在 System.Windows.Forms.MenuItem.OnClick(EventArgs e)
在 System.Windows.Forms.MenuItem.MenuItemData.Execute()
在 System.Windows.Forms.Command.Invoke()
在 System.Windows.Forms.Control.WmCommand(Message& m)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ListView.WndProc(Message& m)
在 DigitalPlatform.GUI.ListViewNF.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=3.7.7278.20124, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.2.9200.0
本机 MAC 地址: 94C691840CE9 
操作时间 2020/1/2 14:44:36 (Thu, 02 Jan 2020 14:44:36 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
         * */
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
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "请指定要输出的 Excel 文件名",
                CreatePrompt = false,
                OverwritePrompt = true,
                // dlg.FileName = this.ExportExcelFilename;
                // dlg.InitialDirectory = Environment.CurrentDirectory;
                Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",

                RestoreDirectory = true
            };

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

            Debug.Assert(alignments.Count == list.Columns.Count, "");

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

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return 0;
                }

                // List<CellData> cells = new List<CellData>();

                nColIndex = 1;
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    // 统计最大字符数
                    // int nChars = column_max_chars[nColIndex - 1];
                    if (subitem.Text != null)
                    {
                        SetMaxChars(/*ref*/ column_max_chars, nColIndex - 1, subitem.Text.Length);
                    }
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(subitem.Text, '*'));
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.FontName = strFontName;
                    // 2020/1/6 增加保护代码
                    if (nColIndex - 1 < alignments.Count)
                        cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    else
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    nColIndex++;
                }

                if (stop != null)
                    stop.SetProgressValue(nRowIndex - 1);

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
                // int nChars = column_max_chars[i];
                int nChars = GetMaxChars(column_max_chars, i);

                if (nChars < MAX_CHARS)
                    column.AdjustToContents();
                else
                {
                    int nColumnWidth = 100;
                    // 2020/1/6 增加保护判断
                    if (i >= 0 && i < list.Columns.Count)
                        nColumnWidth = list.Columns[i].Width;
                    column.Width = (double)nColumnWidth / char_width;  // Math.Min(MAX_CHARS, nChars);
                }
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


        public static int GetMaxChars(List<int> column_max_chars, int index)
        {
            if (index < 0)
                throw new ArgumentException($"index 参数必须大于等于零 (而现在是 {index})");

            if (index >= column_max_chars.Count)
                return 0;
            return column_max_chars[index];
        }

        public static void SetMaxChars(/*ref*/ List<int> column_max_chars, int index, int chars)
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
    }
}
