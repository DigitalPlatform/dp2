using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.dp2.Statis;
using dp2Circulation.Reader;
using static dp2Circulation.BiblioSearchForm;


namespace dp2Circulation
{
    /// <summary>
    /// 书目检索，具有多字段过滤功能
    /// </summary>
    // public class FilterSearch
    public partial class BiblioSearchForm : MyForm
    {

        public IEnumerable<DataGridViewRow> Rows
        {
            get
            {
                return this.dataGridView1.Rows.Cast<DataGridViewRow>();
            }
        }

        List<PatronColumn> _columns = new List<PatronColumn>();

        // 字段名 --> col index
        public Hashtable ColumnTable
        {
            get
            {
                Hashtable table = new Hashtable();
                foreach (var column in _columns)
                {
                    if (string.IsNullOrEmpty(column.PatronFieldName) == false)
                    {
                        table[column.PatronFieldName] = _columns.IndexOf(column);
                    }
                }

                return table;
            }
        }

        // 获得 检索键 字段名
        public string GetMergeKeyName()
        {
            var merge_key_columns = _columns.Where(o => o.IsMergeKey).ToList();
            if (merge_key_columns.Count == 0)
                return null;
            return merge_key_columns[0].PatronFieldName;
        }

        public int GetColumnIndex(string fieldName)
        {
            var table = this.ColumnTable;
            if (table.ContainsKey(fieldName) == false)
                return -1;
            return (int)table[fieldName];
        }


        public void LoadQueryFromExcel(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    Title = "请指定要导入的文件",
                    // dlg.FileName = this.RecPathFilePath;
                    // dlg.InitialDirectory = 
                    // Multiselect = true,
                    Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    RestoreDirectory = true
                };

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
                fileName = dlg.FileName;
            }

            _ = Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        _loadExcel(fileName);
                    }
                    catch (Exception ex)
                    {
                        this.MessageBoxShow($"_loadExcel() 出现异常: {ex.Message}");
                    }
                },
                default,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        // 异常: 可能会抛出异常。尤其是当即将被打开的 Excel 文件被别的应用锁定的时候
        public void _loadExcel(string filename)
        {
            using (var looping = Looping("正在从 Excel 文件装载数据 ...",
                "disableControl"))
            {
                /*
                this.TryInvoke(() =>
                {
                    this.toolStrip1.Focus();
                });
                */

                var doc = new XLWorkbook(filename);

                string sheet_name = null;
                var sheet_names = doc.Worksheets.Select(x => x.Name).ToList();
                if (sheet_names.Count > 0)
                {
                    // 选定一个 sheet
                    sheet_name = ListDialog.GetInput(
                    this,
                    $"从 {filename} 装载",
                    "请选择一个 Sheet",
                    sheet_names,
                    0,
                    this.Font);
                    if (sheet_name == null)
                        return;
                }
                else
                    sheet_name = sheet_names[0];

                _sheet_name = sheet_name;

                this.ClearColumns();

                bool first_row = false;
                var sheet = doc.Worksheets.Where(x => x.Name == sheet_name).FirstOrDefault();
                EnsureColumnsCount(1);
                var sheet_rows = sheet.Rows();
                looping.Progress.SetProgressRange(0, sheet_rows.Count());
                int i = 0;
                foreach (var source_row in sheet_rows)
                {
                    if (looping.Stopped)
                        return;

                    int index = 0;
                    this.TryInvoke(() =>
                    {
                        index = this.dataGridView1.Rows.Add();
                    });
                    DataGridViewRow grid_row = this.dataGridView1.Rows[index];

                    var cells = source_row.CellsUsed();
                    foreach (var cell in cells)
                    {
                        var col = cell.Address.ColumnNumber - 1;
                        EnsureColumnsCount(col + 1);
                        /*
                        if (this.dataGridView1.ColumnCount < col + 1)
                            this.dataGridView1.ColumnCount = col + 1;
                        */
                        if (cell.DataType == XLDataType.DateTime)
                            grid_row.Cells[col].Value = ((DateTime)cell.Value);
                        else
                            grid_row.Cells[col].Value = cell.GetString();
                    }

                    if (first_row == false)
                    {
                        AutoSetFieldName(cells);
                        first_row = true;
                    }

                    // grid_row.HeaderCell.Value = (i + 1).ToString();
                    // grid_row.HeaderCell.Tag = null;

                    i++;
                    looping.Progress.SetProgressValue(i);
                }
            }
        }

        void ClearColumns()
        {
            this.TryInvoke(() =>
            {
                this.dataGridView1.Columns.Clear(); // 2023/6/17
                this.dataGridView1.Rows.Clear();
            });
            this._columns.Clear();
        }

        // 确保 Columns 数量足够匹配内容行
        void EnsureColumnsCount(int count)
        {
            int index = _columns.Count;
            while (_columns.Count < count)
            {
                var view_column = new DataGridViewTextBoxColumn
                {
                    HeaderText = (index + 1).ToString(),
                    Width = 200,
                    CellTemplate = new DataGridViewTextBoxCell()
                };
                this.TryInvoke(() =>
                {
                    this.dataGridView1.Columns.Add(
                        view_column
                    );
                });
                _columns.Add(new PatronColumn { ViewColumn = view_column });
            }
        }


        void AutoSetFieldName(IXLCells cells)
        {
            foreach (var cell in cells)
            {
                var col = cell.Address.ColumnNumber - 1;
                var text = cell.Value.ToString();
                var field_name = FindFieldNameByCaption(text, out string field_caption);
                if (field_name != null)
                {
                    var column = this._columns[col];
                    column.PatronFieldName = field_name;
                    this.TryInvoke(() =>
                    {
                        column.ViewColumn.HeaderText = BuildHeaderText(field_name, field_caption, column.IsMergeKey);
                    });
                }
            }
        }

        static string BuildHeaderText(
    string field_name,
    string field_caption,
    bool isMergeKey)
        {
            return $"{(isMergeKey ? "*" : "")}[{field_name}]\r\n{field_caption}";
        }

        static string GetLeft(string text)
        {
            var parts = StringUtil.ParseTwoPart(text, ":");
            return parts[0];
        }

        static string GetRight(string text)
        {
            var parts = StringUtil.ParseTwoPart(text, ":");
            return parts[1];
        }

        // 根据 dp2library 服务器一端的检索途径信息，刷新
        // _mergekey_field_names 和 _patron_field_names
        public void RefreshFilterFieldNames()
        {
            List<string> mergekey_list = new List<string>();
            List<string> list = new List<string>(_patron_field_names);
            foreach (var info in Program.MainForm.BiblioDbFromInfos)
            {
                var style = info.Style;
                // 去掉 _ 开头的 style
                var parts = StringUtil.SplitList(style, ",");
                var first_style = parts.Where(o => o.StartsWith("_") == false).FirstOrDefault();

                if (string.IsNullOrEmpty(first_style) == false)
                    mergekey_list.Add(first_style);

                if (list.Where(o => GetLeft(o) == first_style).Count() == 0)
                    list.Add($"{first_style}:{info.Caption}");
            }

            _mergekey_field_names = mergekey_list.ToArray();
            _patron_field_names = list.ToArray();
        }

        // 适合用作检索键的字段名列表
        static string[] _mergekey_field_names = new string[]
        {
            "title",
            "pinyin_title",
            "contributor",
            "pinyin_contributor",
            "publisher",
            "publishtime",
            "clc",
            "lcc",
            "ddc",
            "class",
            "subject",
            "isbn",
            "issn",
            "state",
            "batchno",
            "recpath",
            "targetrecpath",
            "opertime",
            "recid",
            "ukey",
            "ucode",
            "crkey",
        };

        // 书目字段名列表
        static string[] _patron_field_names = new string[]
        {
            "title:书名,题名,刊名",
            "pinyin_title:书名拼音,题名拼音,刊名拼音",
            "contributor:作者,著者,编者",
            "pinyin_contributor:著者拼音,作者拼音",
            "publisher:出版社,出版者",
            "publishtime:出版日期,出版时间,出版年月,出版年",
            "clc:中图法分类号,中图法",
            "lcc:美国国会图书馆分类号,国会法分类号,国会法类号,国会法",
            "ddc:杜威十进制分类号,杜威分类号,杜威十进制",
            "class:分类法",
            "subject:主题词,主题,关键词,自由词",
            "isbn:ISBN,国际统一书号",
            "issn:ISSN,国际统一刊号",
            "state:状态",
            "batchno:批次号,编目批次号",
            "recpath:记录路径,路径,读者记录路径",
            "targetrecpath:目标记录路径",
            "opertime:操作时间",
            "recid:记录ID",
            "ukey:查重键",
            "ucode:查重码",
            "crkey:编目规则码",

            "ktf:科学院图书分类法,科图法",
            "rdf:中国人民大学图书馆分类法,人大法",
        };

        /*
isbn	ISBN
issn	ISSN
title	题名
pinyin_title	题名拼音
subject	主题词
clc	中图法
lcc	国会法
ddc	杜威十进制分类法
class	分类法
contributor	作者
pinyin_contributor	作者拼音
publisher	出版者
publishtime	出版时间
batchno	编目批次号
targetrecpath	目标记录路径
state	状态
opertime	操作时间
identifier	其它标识符
ukey	查重键
ucode	查重码
crkey	编目规则码
recid	记录ID
        * 
         * */


        // 根据显示名找到(dp2 读者记录)字段名
        static string FindFieldNameByCaption(string caption,
            out string field_caption)
        {
            field_caption = "";
            foreach (var s in _patron_field_names)
            {
                string field_name = GetLeft(s);
                string captions = GetRight(s);

                if (field_name == caption)
                {
                    field_caption = StringUtil.SplitList(captions).FirstOrDefault();
                    return field_name;
                }

                if (string.IsNullOrEmpty(captions))
                    continue;
                if (StringUtil.IsInList(caption, captions))
                {
                    field_caption = StringUtil.SplitList(captions).FirstOrDefault();
                    return field_name;
                }
            }

            return null;
        }

        // 根据字段名找到 caption
        static string FindCaption(string field_name)
        {
            foreach (var s in _patron_field_names)
            {
                string current_field_name = GetLeft(s);
                if (current_field_name != field_name)
                    continue;
                string captions = GetRight(s);
                return StringUtil.SplitList(captions).FirstOrDefault();
            }

            return null;
        }

        // 查看一个字段名是否已经被 _columns 中使用过了
        bool HasFieldNameUsed(string field_name)
        {
            return _columns.Where(o => o.PatronFieldName == field_name).Any();
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.dataGridView1.Rows.Count > 0)
                return;

            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();

            AppendCommonItems(contextMenu);

            contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Control.MousePosition));
        }

        static string[] _match_styles = new string[] {
        "前方一致\tleft",
        "精确一致\texact",
        "中间一致\tmiddle",
        "后方一致\tright",
        };

        string _selectedMatchStyle = "left";

        void AppendCommonItems(ContextMenu contextMenu)
        {
            var menuItem = new MenuItem("从 Excel 文件装载(&L) ...");
            menuItem.Click += MenuItem_loadFromExcel_Click;
            contextMenu.MenuItems.Add(menuItem);

            if (this.dataGridView1.Rows.Count == 0)
            {
                bool bHasClipboardObject = false;
                IDataObject iData = Clipboard.GetDataObject();
                if (iData != null
                    && (iData.GetDataPresent(DataFormats.UnicodeText) == true
                    || iData.GetDataPresent(DataFormats.Text) == true))
                    bHasClipboardObject = true;

                menuItem = new MenuItem($"粘贴 (&P)");
                if (bHasClipboardObject == false)
                    menuItem.Enabled = false;
                menuItem.Tag = 0;
                menuItem.Click += new System.EventHandler(this.menu_pasteQueryFromClipboard_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            // TODO: 设定检索键的匹配方式
            menuItem = new MenuItem("检索键的匹配方式");
            contextMenu.MenuItems.Add(menuItem);
            foreach (var style_string in _match_styles)
            {
                var style_value = StringUtil.ParseTwoPart(style_string, "\t")[1];
                var submenu = new MenuItem(style_string);
                if (style_string.EndsWith(_selectedMatchStyle))
                    submenu.Checked = true;
                submenu.Tag = style_value;
                submenu.Click += (o2, e2) =>
                {
                    _selectedMatchStyle = (o2 as MenuItem).Tag as string;
                };
                menuItem.MenuItems.Add(submenu);
            }

            menuItem = new MenuItem($"开始检索 [{this.dataGridView1.Rows.Count}](&S)");
            menuItem.Click += MenuItem_beginSearch_Click;
            contextMenu.MenuItems.Add(menuItem);

        }

        private async void MenuItem_beginSearch_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 检查检索键是否具备了
            {
                var merge_key_columns = _columns.Where(o => o.IsMergeKey).ToList();
                if (merge_key_columns.Count == 0)
                {
                    strError = "尚未指定检索键列";
                    goto ERROR1;
                }

                if (merge_key_columns.Count > 1)
                {
                    strError = $"检索键列不允许超过 1 个。(但现在是 {merge_key_columns.Count} 个)";
                    goto ERROR1;
                }
            }

            await DoSearch(false, false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_loadFromExcel_Click(object sender, EventArgs e)
        {
            this.LoadQueryFromExcel(null);
        }

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            {
                menuItem = new MenuItem("设置书目字段名");
                contextMenu.MenuItems.Add(menuItem);

                foreach (var s in _patron_field_names)
                {
                    string name = GetLeft(s);
                    string captions = GetRight(s);
                    string caption = StringUtil.SplitList(captions).FirstOrDefault();

                    MenuItem subMenuItem = new MenuItem(name + "\t" + caption);
                    //if (HasFieldNameUsed(name))
                    //    subMenuItem.Enabled = false;
                    subMenuItem.Tag = new MenuInfo
                    {
                        Caption = caption,
                        Name = name,
                        Column = _columns[e.ColumnIndex]
                    };
                    subMenuItem.Click += new System.EventHandler(this.menu_setFieldName_Click);
                    menuItem.MenuItems.Add(subMenuItem);
                }

                {
                    MenuItem subMenuItem = new MenuItem("<清除>");
                    subMenuItem.Tag = new MenuInfo
                    {
                        Caption = "<清除>",
                        Name = "<清除>",
                        Column = _columns[e.ColumnIndex]
                    };
                    subMenuItem.Click += new System.EventHandler(this.menu_setFieldName_Click);
                    menuItem.MenuItems.Add(subMenuItem);
                }
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            var current_column = _columns[e.ColumnIndex];

            if (true)
            {
                menuItem = new MenuItem("检索键");
                // menuItem.Enabled = this.MergeMode;
                if (current_column.IsMergeKey)
                    menuItem.Checked = true;
                if (string.IsNullOrEmpty(current_column.PatronFieldName)
                    || Array.IndexOf(_mergekey_field_names, current_column.PatronFieldName) == -1)
                    menuItem.Enabled = false;
                menuItem.Tag = current_column;
                menuItem.Click += new System.EventHandler(this.menu_toggleMergeKey_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            AppendCommonItems(contextMenu);

            contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Control.MousePosition));
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            {
                int count = this.dataGridView1.Rows.Count;
                menuItem = new MenuItem($"全选 [{count}]");
                if (count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += (o1, e1) =>
                {
                    foreach (var row in this.Rows)
                    {
                        if (row.IsNewRow)
                            continue;
                        row.Selected = true;
                    }
                };
                contextMenu.MenuItems.Add(menuItem);
            }

            /*
            {
                int hit_count = GetHitCount();
                menuItem = new MenuItem($"移除已命中行[{hit_count}]");
                if (hit_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_removeHitLine_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            {
                int nothit_count = GetNotHitCount();
                menuItem = new MenuItem($"移除未命中行[{nothit_count}]");
                if (nothit_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_removeNotHitLine_Click);
                contextMenu.MenuItems.Add(menuItem);
            }
            */

            int hit_count = GetHitRows().Count;
            {
                menuItem = new MenuItem($"选定全部已命中行[{hit_count}]");
                if (hit_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_selectHitLine_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            {
                int nothit_count = this.Rows.Count() - hit_count - 1;
                menuItem = new MenuItem($"选定全部未命中行[{nothit_count}]");
                if (nothit_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_selectNotHitLine_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            {
                menuItem = new MenuItem($"按照命中数排序");
                menuItem.Click += new System.EventHandler(this.menu_sortByHitCount_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.MenuItems.Add(new MenuItem("-"));

            int selected_count = dataGridView1
    .SelectedRows
    .Cast<DataGridViewRow>()
    .Where(o => o.IsNewRow == false)
    .Count();

            {
                menuItem = new MenuItem($"剪切 [{selected_count}] (&T)");
                if (selected_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_cutQueryToClipboard_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            {
                menuItem = new MenuItem($"复制 [{selected_count}] (&C)");
                if (selected_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_copyQueryToClipboard_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            {
                bool bHasClipboardObject = false;
                IDataObject iData = Clipboard.GetDataObject();
                if (iData != null
                    && (iData.GetDataPresent(DataFormats.UnicodeText) == true
                    || iData.GetDataPresent(DataFormats.Text) == true))
                    bHasClipboardObject = true;

                menuItem = new MenuItem($"粘贴 (&P)");
                if (bHasClipboardObject == false)
                    menuItem.Enabled = false;
                menuItem.Tag = e.RowIndex;
                menuItem.Click += new System.EventHandler(this.menu_pasteQueryFromClipboard_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.MenuItems.Add(new MenuItem("-"));

            {
                menuItem = new MenuItem($"复制到 Excel 文件 [{selected_count}] ... (&C)");
                if (selected_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_copyQueryToExcelFile_Click);
                contextMenu.MenuItems.Add(menuItem);
            }

            contextMenu.MenuItems.Add(new MenuItem("-"));


            {

                // GetSelectedRowCount();
                menuItem = new MenuItem($"移除选定行[{selected_count}]");
                /*
                var current_row = this.dataGridView1.Rows[e.RowIndex];
                if (current_row.IsNewRow)
                    menuItem.Enabled = false;
                menuItem.Tag = e.RowIndex;
                */
                if (selected_count == 0)
                    menuItem.Enabled = false;
                menuItem.Click += new System.EventHandler(this.menu_removeSelectedLine_Click);
                contextMenu.MenuItems.Add(menuItem);
            }


            AppendCommonItems(contextMenu);

            contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Control.MousePosition));
        }

        static string ToString(DataGridViewRow row)
        {
            StringBuilder text = new StringBuilder();
            foreach (DataGridViewCell cell in row.Cells)
            {
                var value = cell.Value?.ToString();
                if (text.Length > 0)
                    text.Append("\t");
                text.Append(value);
            }

            return text.ToString();
        }

        void menu_copyQueryToExcelFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            using (var looping = Looping("正在保存到 Excel 文件..."))
            {
                var ret = SaveToExcel(looping.Progress,
                    _sheet_name,
                    out strError);
                if (ret == -1)
                    goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        string _sheet_name = null;

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        public int SaveToExcel(
            Stop stop,
            string sheet_name,
            out string strError)
        {
            strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "请指定要输出的 Excel 文件名",
                CreatePrompt = false,
                OverwritePrompt = true,
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

            using (doc)
            {
                if (string.IsNullOrEmpty(sheet_name))
                    sheet_name = "表格";

                IXLWorksheet sheet = null;
                sheet = doc.Worksheets.Add(sheet_name);

                var rows = GetSelectedRows();

                if (stop != null)
                    stop.SetProgressRange(0, rows.Count);

                // 每个列的最大字符数
                List<int> column_max_chars = new List<int>();

                /*
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
                */

                string strFontName = this.Font.FontFamily.Name;

                int nRowIndex = 1;
                int nColIndex = 1;
                foreach (var column in _columns)
                {
                    IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(column.PatronFieldName, '*'));
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontName = strFontName;
                    // cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                    nColIndex++;
                }
                nRowIndex++;

                foreach (var row in rows)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    nColIndex = 1;
                    foreach (DataGridViewCell cell0 in row.Cells)
                    {
                        // 统计最大字符数
                        // int nChars = column_max_chars[nColIndex - 1];
                        var value = cell0.Value?.ToString();
                        if (value != null)
                        {
                            ClosedXmlUtil.SetMaxChars(/*ref*/ column_max_chars, nColIndex - 1, value.Length);
                        }
                        IXLCell cell = sheet.Cell(nRowIndex, nColIndex).SetValue(DomUtil.ReplaceControlCharsButCrLf(value, '*'));
                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Font.FontName = strFontName;
                        /*
                        if (nColIndex - 1 < alignments.Count)
                            cell.Style.Alignment.Horizontal = alignments[nColIndex - 1];
                        else
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        */
                        nColIndex++;
                    }

                    if (stop != null)
                        stop.SetProgressValue(nRowIndex - 1);

                    nRowIndex++;
                }

                if (stop != null)
                    stop.SetMessage("正在调整列宽度 ...");
                Application.DoEvents();

                double char_width = ClosedXmlUtil.GetAverageCharPixelWidth(this.dataGridView1);

                // 字符数太多的列不要做 width auto adjust
                const int MAX_CHARS = 30;   // 60
                int i = 0;
                foreach (IXLColumn column in sheet.Columns())
                {
                    // int nChars = column_max_chars[i];
                    int nChars = ClosedXmlUtil.GetMaxChars(column_max_chars, i);

                    if (nChars < MAX_CHARS)
                        column.AdjustToContents();
                    else
                    {
                        int nColumnWidth = 100;
                        /*
                        if (i >= 0 && i < _columns.Count)
                            nColumnWidth = _columns[i].Width;
                        */
                        column.Width = (double)nColumnWidth / char_width;  // Math.Min(MAX_CHARS, nChars);
                    }
                    i++;
                }

                // sheet.Columns().AdjustToContents();

                // sheet.Rows().AdjustToContents();

                doc.SaveAs(dlg.FileName);
            }

            try
            {
                System.Diagnostics.Process.Start(dlg.FileName);
            }
            catch
            {

            }
            return 1;
        }


        void menu_copyQueryToClipboard_Click(object sender, EventArgs e)
        {
            StringBuilder text = new StringBuilder();
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (row.Selected)
                    text.AppendLine(ToString(row));
            }

            ClipboardUtil.SetClipboardText(text.ToString());
        }

        void menu_cutQueryToClipboard_Click(object sender, EventArgs e)
        {
            StringBuilder text = new StringBuilder();
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (row.Selected)
                {
                    text.AppendLine(ToString(row));
                    this.dataGridView1.Rows.Remove(row);
                }
            }

            ClipboardUtil.SetClipboardText(text.ToString());
        }

        void menu_pasteQueryFromClipboard_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var row_index = 0;
            if (menuItem.Tag != null)
                row_index = (int)menuItem.Tag;
            var text = ClipboardUtil.GetClipboardText();
            if (text == null)
                return;

            EnsureColumnsCount(1);

            using (StringReader sr = new StringReader(text))
            {
                //if (text != null)
                //    text = text.Replace("\r\n", "\n");
                //var lines = text.Split('\n');
                //foreach (var line in lines)
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;

                    this.dataGridView1.Rows.Insert(row_index, 1);
                    DataGridViewRow grid_row = this.dataGridView1.Rows[row_index];

                    var cols = line.Split('\t');
                    EnsureColumnsCount(cols.Length);
                    int i = 0;
                    foreach (var s in cols)
                    {
                        grid_row.Cells[i].Value = s;
                        i++;
                    }

                    row_index++;
                }
            }
        }

        /*
        void menu_removeHitLine_Click(object sender, EventArgs e)
        {
            var selected_count = GetHitCount(true);
            MessageBox.Show(this, $"共移除 {selected_count} 行");
        }

        void menu_removeNotHitLine_Click(object sender, EventArgs e)
        {
            var selected_count = GetNotHitCount(true);
            MessageBox.Show(this, $"共移除 {selected_count} 行");
        }
        */

        // 按照命中数对所有行排序
        void menu_sortByHitCount_Click(object sender, EventArgs e)
        {
            this.dataGridView1.Sort(new RowComparer());
        }

        private class RowComparer : System.Collections.IComparer
        {
            public RowComparer()
            {
            }

            public int Compare(object x, object y)
            {
                DataGridViewRow row1 = (DataGridViewRow)x;
                DataGridViewRow row2 = (DataGridViewRow)y;

                var count1 = BiblioSearchForm.GetHitCount(row1);
                var count2 = BiblioSearchForm.GetHitCount(row2);

                return count1 - count2;
            }
        }

        void menu_selectHitLine_Click(object sender, EventArgs e)
        {
            foreach (var row in this.Rows)
            {
                if (row.IsNewRow == true)
                    row.Selected = false;
                else if (GetHitCount(row) > 0)
                    row.Selected = true;
                else
                    row.Selected = false;
            }
        }

        void menu_selectNotHitLine_Click(object sender, EventArgs e)
        {
            foreach (var row in this.Rows)
            {
                if (row.IsNewRow == true)
                    row.Selected = false;
                else if (GetHitCount(row) > 0)
                    row.Selected = false;
                else
                    row.Selected = true;
            }
        }

        internal static int GetHitCount(DataGridViewRow row)
        {
            var query_line = row.Tag as QueryLine;
            if (query_line == null || query_line.HitItems == null)
                return 0;
            return query_line.HitItems.Count;
        }

        List<DataGridViewRow> GetHitRows()
        {
            List<DataGridViewRow> results = new List<DataGridViewRow>();
            foreach (var row in this.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (GetHitCount(row) > 0)
                    results.Add(row);
            }
            return results;
        }

        List<DataGridViewRow> GetNotHitRows()
        {
            List<DataGridViewRow> results = new List<DataGridViewRow>();
            foreach (var row in this.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (GetHitCount(row) == 0)
                    results.Add(row);
            }
            return results;
        }

        List<DataGridViewRow> GetSelectedRows()
        {
            List<DataGridViewRow> results = new List<DataGridViewRow>();
            foreach (var row in this.Rows)
            {
                if (row.IsNewRow)
                    continue;
                if (row.Selected)
                    results.Add(row);
            }
            return results;
        }

        /*
        
        int GetHitCount(bool remove = false)
        {
            int selected_count = 0;
            var rows = new List<DataGridViewRow>(this.Rows);
            foreach(var row in rows)
            {
                if (row.IsNewRow)
                    continue;
                var query_line = row.Tag as QueryLine;
                if (query_line == null || query_line.HitItems == null)
                    continue;
                if (query_line.HitItems.Count > 0)
                {
                    selected_count++;
                    if (remove)
                        this.dataGridView1.Rows.Remove(row);
                }
            }
            return selected_count;
        }

        int GetNotHitCount(bool remove = false)
        {
            int selected_count = 0;
            var rows = new List<DataGridViewRow>(this.Rows);
            foreach (var row in rows)
            {
                if (row.IsNewRow)
                    continue;

                var query_line = row.Tag as QueryLine;
                if (query_line == null || query_line.HitItems == null)
                {
                    selected_count++;
                    if (remove)
                        this.dataGridView1.Rows.Remove(row);
                    continue;
                }
                if (query_line.HitItems.Count == 0)
                {
                    selected_count++;
                    if (remove)
                        this.dataGridView1.Rows.Remove(row);
                }
            }
            return selected_count;
        }
        */

        // 双击行标题。加亮显示此行关联的命中 ListViewItem 行
        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var current_row = this.dataGridView1.Rows[e.RowIndex];
            var query_line = current_row.Tag as QueryLine;
            this.listView_records.SelectedItems.Clear();
            if (query_line != null
                && query_line.HitItems != null)
            {
                int i = 0;
                foreach (var item in query_line.HitItems)
                {
                    if (i == 0)
                        item.ListView.FocusedItem = item;
                    item.Selected = true;
                    item.EnsureVisible();
                    i++;
                }
            }
        }

        class MenuInfo
        {
            // 中文名称
            public string Caption { get; set; }

            // 字段名
            public string Name { get; set; }

            public PatronColumn Column { get; set; }
        }

        void menu_setFieldName_Click(object sender, EventArgs e)
        {
            var menu = (sender as MenuItem);
            var info = menu.Tag as MenuInfo;
            if (info.Name == "<清除>")
            {
                info.Column.PatronFieldName = null;
                info.Column.ViewColumn.Name = null;
                info.Column.ViewColumn.HeaderText = null;
                info.Column.IsMergeKey = false;
                return;
            }
            info.Column.PatronFieldName = info.Name;
            info.Column.ViewColumn.Name = info.Name;

            // 如果是不适合作为检索键的字段，要清除 IsMergeKey 状态
            if (Array.IndexOf(_mergekey_field_names, info.Column.PatronFieldName) == -1)
                info.Column.IsMergeKey = false;

            info.Column.ViewColumn.HeaderText = BuildHeaderText(info.Name, info.Caption, info.Column.IsMergeKey);
        }

        void menu_toggleMergeKey_Click(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var current_column = (PatronColumn)menuItem.Tag;
            current_column.IsMergeKey = !current_column.IsMergeKey;

            current_column.ViewColumn.HeaderText = BuildHeaderText(current_column.PatronFieldName,
                FindCaption(current_column.PatronFieldName),
                current_column.IsMergeKey);
            /*
            if (current_column.IsMergeKey == false)
                current_column.ViewColumn.HeaderText = RemoveMergeKeyChar(current_column.ViewColumn.HeaderText);
            else
                current_column.ViewColumn.HeaderText = AddMergeKeyChar(current_column.ViewColumn.HeaderText);
            */
        }

        void menu_removeSelectedLine_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                if (row.IsNewRow)
                    continue;
                this.dataGridView1.Rows.Remove(row);
            }
            /*
            MenuItem menu = sender as MenuItem;
            int row_index = (int)menu.Tag;
            var row = this.dataGridView1.Rows[row_index];
            if (row.IsNewRow)
                return;
            this.dataGridView1.Rows.RemoveAt(row_index);
            */
        }

        // 构造书目记录 XML
        // return:
        //      null    空行
        //      其它      书目记录 XML 格式
        public XmlDocument BuildBiblioXml(DataGridViewRow row)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<biblio />");
            foreach (var s in _patron_field_names)
            {
                string field_name = GetLeft(s);
                int index = GetColumnIndex(field_name);
                if (index == -1)
                    continue;
                var cell = row.Cells[index];

                string value = null;
                if (field_name == "dateOfBirth"
                    || field_name == "expireDate"
                    || field_name == "createDate")
                {
                    if (cell.Value is DateTime)
                        value = DateTimeUtil.Rfc1123DateTimeStringEx((DateTime)cell.Value);
                    else
                        value = cell.Value?.ToString();
                }
                else
                    value = (string)cell.Value;

                if (string.IsNullOrEmpty(value))
                    continue;

                if (field_name == "email")
                {
                    DomUtil.SetElementText(dom.DocumentElement, field_name, value);
                }
                else
                    DomUtil.SetElementText(dom.DocumentElement, field_name, value);
            }

            if (dom.DocumentElement.ChildNodes.Count == 0)
                return null;
            return dom;
        }

        QueryLine BuildQueryLine(DataGridViewRow row,
    bool clear_hitcount)
        {
            QueryLine line = new QueryLine();
            line.FilterItems = new List<QueryFilterItem>();
            int i = 0;
            foreach (DataGridViewCell cell in row.Cells)
            {
                var column = _columns[i];
                var field_name = column.PatronFieldName;
                if (string.IsNullOrEmpty(field_name))
                    continue;

                string value = null;
                if (cell.Value is DateTime)
                {
                    if (field_name == "publishtime")
                        value = ((DateTime)cell.Value).ToString("yyyy.M.d");
                    else
                        value = ((DateTime)cell.Value).ToString();
                }
                /*
                if (field_name.ToLower().Contains("time")
                    || field_name.ToLower().Contains("date"))
                {
                    if (cell.Value is DateTime)
                        value = DateTimeUtil.Rfc1123DateTimeStringEx((DateTime)cell.Value);
                    else
                        value = cell.Value?.ToString();
                }
                */
                else
                    value = cell.Value?.ToString();  //  (string)cell.Value;

                if (string.IsNullOrEmpty(value))
                    goto CONTINUE;

                if (column.IsMergeKey)
                {
                    line.QueryWord = value;
                    line.FieldName = field_name;
                    line.MatchStyle = _selectedMatchStyle;
                }
                else
                {
                    line.FilterItems.Add(new QueryFilterItem
                    {
                        FieldName = field_name,
                        FilterWord = value
                    });
                }

            CONTINUE:
                i++;
            }

            if (string.IsNullOrEmpty(line.QueryWord))
                return null;
            line.Tag = row;
            line.UpdateViewRowHitCount();
            return line;
        }


#if REMOVED
        QueryLine BuildQueryLine(DataGridViewRow row,
            bool clear_hitcount)
        {
            QueryLine line = new QueryLine();
            line.FilterItems = new List<QueryFilterItem>();
            foreach (var s in _patron_field_names)
            {
                string field_name = GetLeft(s);
                int index = GetColumnIndex(field_name);
                if (index == -1)
                    continue;

                var current_column = _columns[index];

                var cell = row.Cells[index];

                string value = null;
                if (field_name == "dateOfBirth"
                    || field_name == "expireDate"
                    || field_name == "createDate")
                {
                    if (cell.Value is DateTime)
                        value = DateTimeUtil.Rfc1123DateTimeStringEx((DateTime)cell.Value);
                    else
                        value = cell.Value?.ToString();
                }
                else
                    value = (string)cell.Value;

                if (string.IsNullOrEmpty(value))
                    continue;

                if (current_column.IsMergeKey)
                {
                    line.QueryWord = value;
                    line.FieldName = field_name;
                    line.MatchStyle = _selectedMatchStyle;
                }
                else
                {
                    line.FilterItems.Add(new QueryFilterItem
                    {
                        FieldName = field_name,
                        FilterWord = value
                    });
                }
            }

            if (string.IsNullOrEmpty(line.QueryWord))
                return null;
            line.Tag = row;
            line.UpdateViewRowHitCount();
            return line;
        }
#endif

        public List<QueryLine> BuildQueryLines(bool clear_hitcount)
        {
            List<QueryLine> lines = new List<QueryLine>();
            foreach (var row in this.Rows)
            {
                var line = BuildQueryLine(row, clear_hitcount);
                if (line != null)
                {
                    lines.Add(line);
                    row.Tag = line;
                }
                else
                    row.Tag = null;
            }

            return lines;
        }

        // 匹配一个字段
        // TODO: 把中间过程，命中和不命中的都显示到“操作历史”面板，提供分析之用
        static bool Match(
            string recpath,
            MarcRecord record,
            string syntax,
            QueryFilterItem filter_item)
        {
            MarcNodeList nodes = null;
            if (syntax == "unimarc")
            {
                switch (filter_item.FieldName)
                {
                    case "title":
                        nodes = record.select("field[@name='200' or @name='225']/subfield[@name='a'] | field[@name='200']/subfield[@name='e']");
                        break;
                    case "pinyin_title":
                        nodes = record.select("field[@name='200']/subfield[@name='A' or @name='9']");
                        break;
                    case "contributor":
                        return MatchUnimarcAuthor(record, filter_item.FilterWord);
                    //nodes = record.select("field[@name='700' or @name='701' or @name='702' or @name='711' or @name='712']/subfield[@name='a']");
                    //break;
                    case "pinyin_contributor":
                        nodes = record.select("field[@name='700' or @name='701' or @name='702' or @name='711' or @name='712']/subfield[@name='A' or @name='9']");
                        break;
                    case "publisher":
                        // TODO: 可以考虑模糊匹配。去掉一些“出版社”“出版公司”等等的非用字以后再进行匹配
                        nodes = record.select("field[@name='210']/subfield[@name='c']");
                        break;
                    case "publishtime":
                        // nodes = record.select("field[@name='210']/subfield[@name='d']");
                        // 归一化为中立形态以后进行匹配。大小范围按照是否有交叉来判断
                        return MatchUnimarcPublishTime(record, filter_item.FilterWord);
                    case "clc":
                        nodes = record.select("field[@name='690']/subfield[@name='a']");
                        break;
                    case "ktf":
                        nodes = record.select("field[@name='692']/subfield[@name='a']");
                        break;
                    case "rdf":
                        nodes = record.select("field[@name='694']/subfield[@name='a']");
                        break;
                    case "class":
                        nodes = record.select("field[@name='690' or @name='692' or @name='694']/subfield[@name='a']");
                        break;
                    case "subject":
                        // TODO: 注意自由词是如何连接到一起的
                        nodes = record.select("field[@name='600' or @name='601' or @name='606' or @name='610']/subfield[@name='a']");
                        break;
                    case "isbn":
                        // 归一化以后再匹配
                        return MatchUnimarcIsbn(record, filter_item.FilterWord);
                        // nodes = record.select("field[@name='010']/subfield[@name='a' or @name='z']");
                        // break;
                    case "issn":
                        // TODO: 归一化以后再匹配
                        nodes = record.select("field[@name='011']/subfield[@name='a' or @name='z']");
                        break;
                    case "state":
                        break;
                    case "batchno":
                        nodes = record.select("field[@name='998']/subfield[@name='a']");
                        break;
                    case "targetrecpath":
                        nodes = record.select("field[@name='998']/subfield[@name='t']");
                        break;
                    case "opertime":
                        // TODO: 归一化以后再匹配
                        nodes = record.select("field[@name='998']/subfield[@name='u']");
                        break;
                    case "operator":
                        nodes = record.select("field[@name='998']/subfield[@name='z']");
                        break;
                    case "ukey":
                        nodes = record.select("field[@name='997']/subfield[@name='a']");
                        break;
                    case "ucode":
                        nodes = record.select("field[@name='997']/subfield[@name='h']");
                        break;
                    case "crkey":
                        // TODO: 998$l + 998$c 需要一个专用函数
                        nodes = record.select("field[@name='998']/subfield[@name='c']");
                        break;
                    case "recpath":
                        if (recpath == filter_item.FilterWord)
                            return true;
                        return false;
                    default:
                        throw new ArgumentException($"无法识别的 filter_item.FieldName '{filter_item.FieldName}'");
                        return false;
                }
            }
            else if (syntax == "usmarc")
            {
                switch (filter_item.FieldName)
                {
                    default:
                        return false;
                }
            }
            else
                throw new ArgumentException($"无法识别的 MARC 语法 '{syntax}'");

            foreach (MarcSubfield subfield in nodes)
            {
                if (subfield.Content == filter_item.FilterWord)
                    return true;
            }

            /*
            // 补充检测
            if (filter_item.FieldName == "contributor")
            {
                nodes = record.select("field[@name='200']/subfield[@name='f' or @name='g']");
                foreach (MarcSubfield subfield in nodes)
                {
                    var segments = filter_item.FilterWord.Split(',', ';');
                    foreach (var segment in segments)
                    {
                        string text = segment.Trim();
                        if (subfield.Content.Contains(text))
                            return true;
                    }
                }
            }
            */

            return false;
        }

        #region 匹配著者

        public static bool MatchUnimarcIsbn(MarcRecord record,
            string isbn_string)
        {
            var nodes = record.select("field[@name='010' or @name='701']/subfield[@name='a' or @name='z']");
            var isbn_words = isbn_string.Split(' ', ',', '，', ';', '；', '、');
            foreach (MarcSubfield subfield in nodes)
            {
                foreach (var word in isbn_words)
                {
                    var content = CanonicalizeISBN(subfield.Content);
                    if (content == CanonicalizeISBN(word))
                        return true;
                }
            }

            return false;
        }

        // 归一化 ISBN 字符串
        public static string CanonicalizeISBN(string input)
        {
            string isbn = input.Trim();
            isbn = isbn.Replace("-", "");   //去除ISBN中的"-"连接符号
            isbn = isbn.Replace("—", ""); //为稳妥，去除ISBN中的全角"—"连接符号

            if (isbn.Length < 3)
                return isbn; //如果ISBN不足3位，原样输出

            string head = isbn.Substring(0, 3);       //获得新旧ISBN号的判断依据

            if (head == "978" || head == "979")
            {
                isbn = isbn.Substring(3, isbn.Length - 3);

                if (isbn.Length >= 10)
                    isbn = isbn.Substring(0, 9);
            }
            else
            {
                if (isbn.Length >= 10)
                    isbn = isbn.Substring(0, 9);
            }

            return isbn;
        }

        public static bool MatchUnimarcAuthor(MarcRecord record,
            string author_string)
        {
            var nodes = record.select("field[@name='700' or @name='701' or @name='702' or @name='711' or @name='712']/subfield[@name='a']");
            var author_words = SplitAuthor(author_string);
            foreach (MarcSubfield subfield in nodes)
            {
                foreach (var word in author_words)
                {
                    if (subfield.Content == word)
                        return true;
                }
            }

            nodes = record.select("field[@name='200']/subfield[@name='f' or @name='g']");
            foreach (MarcSubfield subfield in nodes)
            {
                foreach (var word in author_words)
                {
                    if (subfield.Content.Contains(word))
                        return true;
                }
            }

            return false;
        }

        public static List<string> SplitAuthor(string text)
        {
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(text))
                return results;
            var segments = text.Split(',', '，', ';', '；', '、');
            foreach (var s in segments)
            {
                var author = GetPureAuthor(s).Trim();
                if (string.IsNullOrEmpty(author) == false)
                    results.Add(author);
            }

            return results;
        }

        static string[] _bianzhu_table = new string[] {
            "文/图",
            "图/文",
            "图文",
            "编撰",
            "编著",
            "编写",
            "翻译",
            "绘图",
            "绘制",
            "改编",
            "创作",
            "撰",
            "编",
            "著",
            "译",
            "绘",
            "画",
            "文",
            "图",
        };

        static string[] _deng_table = new string[] {
            "等",
            "等人",
            "[等]",
            "...[等]",
            "... [等]",
            "...等",
            "... 等",
        };

        // (英)xxx文/图
        public static string GetPureAuthor(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // 去掉左侧的 (英)
            text = RemoveLeftQuotePart(text);

            foreach (var bianzhu in _bianzhu_table)
            {
                if (text.EndsWith(bianzhu))
                    text = text.Substring(0, text.Length - bianzhu.Length);
            }

            foreach (var deng in _deng_table)
            {
                if (text.EndsWith(deng))
                    text = text.Substring(0, text.Length - deng.Length);
            }

            return text;
        }

        // 去掉左侧的 (英) 部分
        static string RemoveLeftQuotePart(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            text = text.Trim();
            if (string.IsNullOrEmpty(text))
                return text;
            string new_text = text.Replace("（", "(").Replace("）", ")");
            int index = new_text.IndexOf("(");
            if (index != 0)
                return text;
            int end = new_text.IndexOf(")", 1);
            if (end == -1)
                return text;
            return new_text.Substring(end + 1).Trim();
        }


        #endregion

        #region 匹配出版时间

        public static bool MatchUnimarcPublishTime(MarcRecord record,
            string publish_time)
        {
            var nodes = record.select("field[@name='210']/subfield[@name='d']");
            var ret = DateRange.TryParse(publish_time, out DateRange publish_time_range);
            foreach (MarcSubfield subfield in nodes)
            {
                DateRange.TryParse(subfield.Content, out DateRange current);
                if (DateRange.IsCross(publish_time_range, current))
                    return true;
            }

            return false;
        }

        // 表达出版时间(范围)的类
        public class DateRange
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            // 原始文本形态的时间字符串
            public string OriginText { get; set; }

            // 是否为解析失败状态的 DateRange。所谓解析失败就是 Start 和 End 无效，只有 OriginText 有效
            public bool ParseFailed
            {
                get
                {
                    return Start == DateTime.MinValue && End == DateTime.MinValue;
                }
            }

            // 检查一个时间点是否包含在当前对象的范围内
            public bool Contains(DateTime time)
            {
                if (time >= this.Start && time <= this.End)
                    return true;
                return false;
            }

            public static bool TryParse(string text,
                out DateRange range)
            {
                range = new DateRange();
                range.OriginText = text;

                string[] formats_year = {
                "yyyy",
                };

                string[] formats_month = {
                "yyyy.M",
                "yyyy.MM",
                "yyyy.MMM",
                "yyyy/M",
                "yyyy/MM",
                "yyyy/MMM",
                };

                string[] formats_day = {
                "yyyy.M.d",
                "yyyy.MM.dd",
                "yyyy.MMM.ddd",

                "yyyy/M/d",
                "yyyy/MM/dd",
                "yyyy/MMM/ddd",
                };

                {
                    var ret = DateTime.TryParseExact(text,
        formats_year,
        DateTimeFormatInfo.InvariantInfo,
        DateTimeStyles.None,
        out DateTime time);
                    if (ret == true)
                    {
                        range.Start = new DateTime(time.Year, 1, 1);
                        range.End = new DateTime(time.Year, 12, 31);
                        return true;
                    }
                }

                {
                    var ret = DateTime.TryParseExact(text,
        formats_month,
        DateTimeFormatInfo.InvariantInfo,
        DateTimeStyles.None,
        out DateTime time);
                    if (ret == true)
                    {
                        range.Start = new DateTime(time.Year, time.Month, 1);
                        if (time.Month < 12)
                            range.End = new DateTime(time.Year, time.Month + 1, 1) - TimeSpan.FromDays(1);
                        else
                            range.End = new DateTime(time.Year + 1, 1, 1) - TimeSpan.FromDays(1);
                        return true;
                    }
                }

                {
                    var ret = DateTime.TryParseExact(text,
        formats_day,
        DateTimeFormatInfo.InvariantInfo,
        DateTimeStyles.None,
        out DateTime time);
                    if (ret == true)
                    {
                        range.Start = new DateTime(time.Year, time.Month, time.Day);
                        range.End = range.Start;
                        return true;
                    }
                }

                {
                    range.Start = DateTime.MinValue;
                    range.End = DateTime.MinValue;
                    return false;
                }
            }

            // 检查两个 range 之间是否存在交叉
            public static bool IsCross(DateRange range1,
                DateRange range2)
            {
                if (range1.ParseFailed == false
                    && range2.ParseFailed == false)
                {
                    if (range1.Contains(range2.Start) || range1.Contains(range2.End)
                        || range2.Contains(range1.Start) || range2.Contains(range1.End))
                        return true;
                    return false;
                }

                return range1.OriginText == range2.OriginText;
            }
        }

        #endregion

        // return:
        //      true    表示记录要被利用
        //      false   表示记录要被忽略
        public static bool FilterRecord(
            QueryLine query_line,
            string recpath,
            string xml,
            StringBuilder process_info = null)
        {
            process_info?.Append($"针对 {recpath}");
            var ret = MarcUtil.Xml2Marc(xml,
                true,
                null,
                out string syntax,
                out string marc,
                out string error);
            if (ret == -1)
            {
                process_info?.AppendLine($"Xml2Marc 出错: {error}");
                return true;
            }
            MarcRecord record = new MarcRecord(marc);
            int i = 0;
            foreach (var item in query_line.FilterItems)
            {
                process_info?.Append($" {i + 1}) 以 '{item.FilterWord}:{item.FieldName}' 过滤，");
                var ret1 = Match(
                    recpath,
                    record,
                    syntax,
                    item);
                if (ret1 == false)
                {
                    process_info?.AppendLine("不匹配");
                    return false;
                }

                process_info?.AppendLine("匹配");
                i++;
            }

            return true;
        }
    }

    public class QueryLine
    {
        // 主检索词
        public string QueryWord { get; set; }

        // 主检索途径
        public string FieldName { get; set; }

        public string MatchStyle { get; set; }

        public object Tag { get; set; }

        public List<ListViewItem> HitItems { get; set; }

        // 筛选事项集合
        public List<QueryFilterItem> FilterItems { get; set; }

        public void AddHitItem(ListViewItem item)
        {
            if (this.HitItems == null)
                this.HitItems = new List<ListViewItem>();
            this.HitItems.Add(item);
        }

        // 更新行标题上的命中结果数字文字显示
        public void UpdateViewRowHitCount()
        {
            var row = this.Tag as DataGridViewRow;
            if (row == null)
                return;
            row.HeaderCell.Value = this.HitItems?.Count.ToString();
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            text.Append($"主检索词:'{QueryWord}', 字段名:'{FieldName}', 筛选列=");
            List<string> items = new List<string>();
            if (this.FilterItems != null
                && this.FilterItems.Count > 0)
            {
                foreach (var item in this.FilterItems)
                {
                    items.Add(item.ToString());
                }

                text.Append(StringUtil.MakePathList(items, "|"));
            }

            return text.ToString();
        }

        // 把检索字符串根据标点符号切割为多个部分。注意第一个元素是没有切割的检索字符串
        public static List<string> SplitQueryString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string> { text };
            List<string> results = new List<string>();
            var segments = text.Split(' ', ';', '；', ':', '：', '/', '.');
            if (segments.Length == 1)
                return new List<string> { text };
            results.Add(text);
            foreach (var s in segments)
            {
                string current = s.Trim();
                if (string.IsNullOrEmpty(current) == false)
                    results.Add(current);
            }

            return results;
        }
    }

    public class QueryFilterItem
    {
        // 筛选字段名
        public string FieldName { get; set; }

        // 筛选检索词
        public string FilterWord { get; set; }

        public override string ToString()
        {
            return $"{FilterWord}:{FieldName}";
        }
    }

    [TestClass]
    public class TestTimeRange
    {
        [TestMethod]
        public void test_timerange_01()
        {
            string time_string1 = "1990.01";
            string time_string2 = "1990.01.02";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void test_timerange_02()
        {
            string time_string1 = "1990";
            string time_string2 = "1990.01";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void test_timerange_03()
        {
            string time_string1 = "1990";
            string time_string2 = "1990.12.31";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void test_timerange_11()
        {
            string time_string1 = "1990";
            string time_string2 = "1991";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void test_timerange_12()
        {
            string time_string1 = "1990.12";
            string time_string2 = "1991";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void test_timerange_13()
        {
            string time_string1 = "1990.12.1";
            string time_string2 = "1991";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void test_timerange_14()
        {
            string time_string1 = "1990.12.1";
            string time_string2 = "1991.5";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(false, ret);
        }

        [TestMethod]
        public void test_timerange_15()
        {
            string time_string1 = "1990.12.1";
            string time_string2 = "1990.12.2";

            var ret = DateRange.TryParse(
                time_string1,
                out DateRange range1);
            Assert.AreEqual(true, ret);

            ret = DateRange.TryParse(
                time_string2,
                out DateRange range2);
            Assert.AreEqual(true, ret);

            ret = DateRange.IsCross(range1, range2);
            Assert.AreEqual(false, ret);
        }

    }

}
