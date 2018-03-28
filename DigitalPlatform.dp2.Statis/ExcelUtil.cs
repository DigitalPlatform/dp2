using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;


namespace DigitalPlatform.dp2.Statis
{
    [Flags]
    public enum WriteExcelLineStyle
    {
        /// <summary>
        /// 没有任何状态
        /// </summary>
        None = 0,

        /// <summary>
        /// 自动判断 string 还是 number
        /// </summary>
        AutoString = 0x1,

        /// <summary>
        /// 把字符串存储到 ShareString 区域
        /// </summary>
        ShareString = 0x02,
    }

    /// <summary>
    /// 表示即将创建的单元 数据
    /// </summary>
    public class CellData
    {
        // int Row = 0;
        public int Col = 0;
        public string Value = "";
        public UInt32Value StyleIndex = 0;
        public bool IsString = true;

        public CellData()
        {
        }

        public CellData(int nColIndex, string value)
        {
            this.Col = nColIndex;
            this.Value = DomUtil.ReplaceControlCharsButCrLf(value, '*');
        }

        public CellData(int nColIndex,
            string value,
            bool bIsString,
            uint style_index)
        {
            this.Col = nColIndex;
            this.Value = DomUtil.ReplaceControlCharsButCrLf(value, '*');
            this.StyleIndex = new UInt32Value(style_index);
            this.IsString = bIsString;
        }
    }

    /// <summary>
    /// 用于实现输出到 Excel 文件的文档类
    /// </summary>
    public class ExcelDocument
    {
        SpreadsheetDocument spreadsheetDocument = null;

        public WorkbookPart workbookpart = null;

        Sheets sheets = null;
        public WorksheetPart worksheetPart = null;
        // Sheet sheet = null;

        int m_nTailSheetNo = 1;
        public Sheet CurrentSheet = null;

        public ExcelDocument()
        {
        }

        public ExcelDocument(SpreadsheetDocument doc)
        {
            if (doc != null)
            {
                spreadsheetDocument = doc;
                this.Initial();
            }
        }

        public static ExcelDocument Create(string strFilename)
        {
            ExcelDocument doc = new ExcelDocument();
            doc.spreadsheetDocument = SpreadsheetDocument.Create(strFilename,
                SpreadsheetDocumentType.Workbook);
            doc.Initial();
            return doc;
        }

        public Stylesheet Stylesheet
        {
            get
            {
                return null;
            }
            set
            {
                WorkbookStylesPart stylesPart = spreadsheetDocument.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = value;
                stylesPart.Stylesheet.Save();
            }
        }

        public void Initial()
        {
            {
                // Add a WorkbookPart to the document.
                workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                /*
                // Add a WorksheetPart to the WorkbookPart.
                worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                 * */

                // Add Sheets to the Workbook.
                sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
            }
        }

        public Sheet NewSheet(string strName)
        {
            // Add a WorksheetPart to the WorkbookPart.
            worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = (UInt32)m_nTailSheetNo ++,
                Name = strName
            };
            sheets.Append(sheet);

            this.CurrentSheet = sheet;  // 随时可以从 CurrentSheet 中找到
            return sheet;
        }

        public void Close()
        {
            spreadsheetDocument.WorkbookPart.Workbook.Save();
            // Close the document.
            spreadsheetDocument.Close();
        }

        public void SaveWorksheet()
        {
            worksheetPart.Worksheet.Save();
        }

        public Worksheet WorkSheet
        {
            get
            {
                return worksheetPart.Worksheet;
            }
        }

        // parameters:
        //      nRowIndex   行号。从 0 开始计算
        public void WriteExcelLine(
            int nRowIndex,
            List<CellData> cells,
            WriteExcelLineStyle style = WriteExcelLineStyle.AutoString)
        {
            // 创建一个 Row。不负责查重
            // row 的 cell 创建完成后，记得 ws.Save()
            Row row = ExcelUtil.CreateRow(
                worksheetPart.Worksheet,
                (UInt32)(nRowIndex + 1));

            foreach (CellData data in cells)
            {
                string strCellName = GetColumnName(data.Col) + (nRowIndex + 1).ToString();

                // 追加一个 Cell
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = ExcelUtil.AppendCell(
                    row,
                    strCellName);


                bool isString = data.IsString;
                if ((style & WriteExcelLineStyle.AutoString) == WriteExcelLineStyle.AutoString)
                    isString = !IsExcelNumber(data.Value);

                if (isString)
                {
                    if ((style & WriteExcelLineStyle.ShareString) == WriteExcelLineStyle.ShareString)
                    {
                        // Either retrieve the index of an existing string,
                        // or insert the string into the shared string table
                        // and get the index of the new item.
                        int stringIndex = ExcelUtil.InsertSharedStringItem(this.workbookpart, data.Value);
                        cell.CellValue = new CellValue(stringIndex.ToString());
                        cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    }
                    else
                    {
                        cell.CellValue = new CellValue(data.Value);
                        cell.DataType = new EnumValue<CellValues>(CellValues.String);
                    }
                }
                else
                {
                    cell.CellValue = new CellValue(data.Value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (data.StyleIndex > 0)
                    cell.StyleIndex = data.StyleIndex;
            }
        }

        public void WriteExcelLine(
            int nLineIndex,
            string strName,
            string strValue)
        {
#if NO
            ExcelUtil.UpdateValue(
    workbookpart,
    worksheetPart.Worksheet,
    "A" + (nLineIndex + 1).ToString(),
    strName,
    0,
    true);
            ExcelUtil.UpdateValue(
                workbookpart,
                worksheetPart.Worksheet,
                "B" + (nLineIndex + 1).ToString(),
                strValue,
                0,
                bString);
#endif
            WriteExcelCell(
                nLineIndex,
                0,
                strName.Trim());    // 2018/3/23
            WriteExcelCell(
                nLineIndex,
                1,
                strValue.Trim());
        }

        // 2014/1/12
        public void WriteExcelTitle(int nLineIndex,
            int nStartCol,
            int nCols,
            string strTitle,
            int nStyleIndex = 0)
        {
            string strStartCellName = GetColumnName(nStartCol) + (nLineIndex + 1).ToString();
            string strEndCellName = GetColumnName(nStartCol + nCols - 1) + (nLineIndex + 1).ToString();

            ExcelUtil.UpdateValue(
workbookpart,
worksheetPart.Worksheet,
strStartCellName,
strTitle,
UInt32Value.FromUInt32((uint)nStyleIndex),
true);

            if (nCols > 1)
            {
                ExcelUtil.UpdateValue(
    workbookpart,
    worksheetPart.Worksheet,
    strEndCellName,
    "",
    0,
    false);
            }
            else
            {
                return;
            }

            ExcelUtil.InsertMergeCell(worksheetPart.Worksheet,
                strStartCellName,
                strEndCellName);
        }


        public void WriteExcelTitle(int nLineIndex,
            int nCols,
            string strTitle,
            int nStyleIndex = 0)
        {
            // strTitle = strTitle.Replace(" ", "_");  // 2018/3/23

            string strStartCellName = "A" + (nLineIndex + 1).ToString();
            string strEndCellName = GetColumnName(nCols - 1) + (nLineIndex + 1).ToString();

            ExcelUtil.UpdateValue(
workbookpart,
worksheetPart.Worksheet,
strStartCellName,
strTitle,
UInt32Value.FromUInt32((uint)nStyleIndex),
true);

            if (nCols > 1)
            {
                ExcelUtil.UpdateValue(
    workbookpart,
    worksheetPart.Worksheet,
    strEndCellName,
    "",
    0,
    false);
            }
            else
            {
                return;
            }

            ExcelUtil.InsertMergeCell(worksheetPart.Worksheet,
                strStartCellName,
                strEndCellName);
        }

        public void InsertMergeCell(
            int nLineIndex,
            int nColIndex,
            int nColspan)
        {
            Debug.Assert(nColspan >= 2, "");

            string strStartCellName = GetCellName(nColIndex, nLineIndex);
            string strEndCellName = GetCellName(nColIndex + nColspan - 1, nLineIndex);

            ExcelUtil.InsertMergeCell(worksheetPart.Worksheet,
    strStartCellName,
    strEndCellName);
        }

        // 检测字符串是否为纯数字(前面可以包含一个'-'号)
        public static bool IsExcelNumber(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return false;

            bool bFoundNumber = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '-' && bFoundNumber == false)
                {
                    continue;
                }
                if (s[i] == '%' && i == s.Length - 1)
                {
                    // 最末一个字符为 %
                    continue;
                }
                if (s[i] > '9' || s[i] < '0')
                    return false;
                bFoundNumber = true;
            }

            if (s.Length > 15)
                return false;

            if (s[0] == '0' && s.Length > 1)
                return false;

            return true;
        }

        // 自动识别数字内容
        public void WriteExcelCell(
    int nLineIndex,
    int nColIndex,
    string strValue,
    int nStyleIndex = 0)
        {
            if (IsExcelNumber(strValue) == false)
                WriteExcelCell(
                    nLineIndex,
                    nColIndex,
                    strValue,
                    true,
                    nStyleIndex);
            else
                WriteExcelCell(
                    nLineIndex,
                    nColIndex,
                    strValue,
                    false,
                    nStyleIndex);
        }

        public void WriteExcelCell(
            int nLineIndex,
            int nColIndex,
            string strValue,
            bool bString,
            int nStyleIndex = 0)
        {
            string strCellName = GetColumnName(nColIndex) + (nLineIndex + 1).ToString();
            ExcelUtil.UpdateValue(
                workbookpart,
                worksheetPart.Worksheet,
                strCellName,
                strValue,
                UInt32Value.FromUInt32((uint)nStyleIndex),
                bString);
        }

        // 获得列名
        static string GetColumnName(int index)
        {
            // return new string((char)((int)'A' + index), 1); 
            int dividend = index + 1;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static string GetCellName(int nColIndex, int nLineIndex)
        {
            return GetColumnName(nColIndex) + (nLineIndex + 1).ToString();
        }
    }

    public class ExcelUtil
    {
        public static string GetWindowsBaseDllPath()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(System.IO.Packaging.Package)).Location;
        }

        public static MergeCell InsertMergeCell(Worksheet ws,
    string addressName1,
    string addressName2)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            MergeCells cells = ws.GetFirstChild<MergeCells>();
            if (cells == null)
            {
                cells = new MergeCells();
                ws.InsertAfter<MergeCells>(cells, sheetData);
            }

            string s = addressName1 + ":" + addressName2;

            MergeCell cell = cells.Elements<MergeCell>().Where(c => c.Reference == s).FirstOrDefault();
            if (cell == null)
            {
                cell = new MergeCell();
                cell.Reference = s;
                cells.AppendChild<MergeCell>(cell);
            }

            return cell;
        }

        // 创建一个 Row。不负责查重
        // row 的 cell 创建完成后，记得 ws.Save()
        public static Row CreateRow(
            Worksheet ws,
            // string addressName,
            UInt32 rowNumber)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();

            // UInt32 rowNumber = GetRowIndex(addressName);
            Row row = new Row();
            row.RowIndex = rowNumber;
            sheetData.Append(row);

            return row;
        }

        // 追加一个 Cell
        public static DocumentFormat.OpenXml.Spreadsheet.Cell AppendCell(
            // Worksheet ws,
            Row row,
            string addressName)
        {
            // SheetData sheetData = ws.GetFirstChild<SheetData>();
            DocumentFormat.OpenXml.Spreadsheet.Cell cell = null;

            cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            cell.CellReference = addressName;

            row.InsertBefore(cell, null);

            return cell;
        }

        public static void UpdateValue(
            WorkbookPart wbPart,
            Worksheet ws, 
            string addressName, 
            string value,
            UInt32Value styleIndex, 
            bool isString)
        {
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // Either retrieve the index of an existing string,
                    // or insert the string into the shared string table
                    // and get the index of the new item.
                    int stringIndex = InsertSharedStringItem(wbPart, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                // Save the worksheet.
                ws.Save();
        }

        // Given the main workbook part, and a text value, insert the text into 
        // the shared string table. Create the table if necessary. If the value 
        // already exists, return its index. If it doesn't exist, insert it and 
        // return its new index.
        internal static int InsertSharedStringItem(WorkbookPart wbPart, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = wbPart
                .GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // If the shared string table is missing, something's wrong.
            // Just return the index that you found in the cell.
            // Otherwise, look up the correct text in the table.
            if (stringTablePart == null)
            {
                // Create it.
                stringTablePart = wbPart.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
                stringTablePart.SharedStringTable = stringTable;
            }

            // Iterate through all the items in the SharedStringTable. 
            // If the text already exists, return its index.
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(value)));
                stringTable.Save();
            }

            return index;
        }

        // Given a Worksheet and an address (like "AZ254"), either return a 
        // cell reference, or create the cell reference and return it.
        private static DocumentFormat.OpenXml.Spreadsheet.Cell InsertCellInWorksheet(Worksheet ws,
            string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            DocumentFormat.OpenXml.Spreadsheet.Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            // If the cell you need already exists, return it.
            // If there is not a cell with the specified column name, insert one.  
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>().
                Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        // Add a cell with the specified address to a row.
        private static DocumentFormat.OpenXml.Spreadsheet.Cell CreateCell(Row row, String address)
        {
            DocumentFormat.OpenXml.Spreadsheet.Cell cellResult;
            DocumentFormat.OpenXml.Spreadsheet.Cell refCell = null;

            // Cells must be in sequential order according to CellReference. 
            // Determine where to insert the new cell.
            foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
            {
                // if (string.Compare(cell.CellReference.Value, address, true) > 0)
                if (CompareCellName(cell.CellReference.Value, address) > 0)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new DocumentFormat.OpenXml.Spreadsheet.Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        // 把 AA12 切分为 AA 和 12 两个部分
        static void ParseCellName(string s,
            out string s1,
            out string s2)
        {
            s1 = "";
            s2 = "";

            foreach (char c in s)
            {
                if (Char.IsLetter(c) == true)
                    s1 += c;
                else
                    s2 += c;
            }
        }

        public static int CompareCellName(string s1, string s2)
        {
            string l1, r1, l2, r2;
            ParseCellName(s1, out l1, out r1);
            ParseCellName(s2, out l2, out r2);

            // 先比较行
            int nRet = StringUtil.RightAlignCompare(r1, r2, '0');
            if (nRet != 0)
                return nRet;

            // 后比较列
            return StringUtil.RightAlignCompare(l1, l2, ' ');
        }

        // Return the row at the specified rowIndex located within
        // the sheet data passed in via wsData. If the row does not
        // exist, create it.
        private static Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().
            Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        // Given an Excel address such as E5 or AB128, GetRowIndex
        // parses the address and returns the row index.
        private static UInt32 GetRowIndex(string address)
        {
            string rowPart;
            UInt32 l;
            UInt32 result = 0;

            for (int i = 0; i < address.Length; i++)
            {
                if (UInt32.TryParse(address.Substring(i, 1), out l))
                {
                    rowPart = address.Substring(i, address.Length - i);
                    if (UInt32.TryParse(rowPart, out l))
                    {
                        result = l;
                        break;
                    }
                }
            }
            return result;
        }

    }
}
