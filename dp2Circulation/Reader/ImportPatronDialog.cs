using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
// using DocumentFormat.OpenXml.Wordprocessing;

namespace dp2Circulation.Reader
{
    /// <summary>
    /// 从外部文件导入读者信息 的 对话框
    /// </summary>
    public partial class ImportPatronDialog : MyForm
    {
        public bool MergeMode { get; set; }

        DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

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

        // 获得 合并键 字段名
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

        public IEnumerable<DataGridViewRow> Rows
        {
            get
            {
                return this.dataGridView1.Rows.Cast<DataGridViewRow>();
            }
        }

        public ImportPatronDialog()
        {
            this.UseLooping = true;
            this.Floating = true;

            InitializeComponent();
        }

        private void ImportPatronDialog_Load(object sender, EventArgs e)
        {
            {
                stopManager.Initial(
    this,
    this.toolStripButton_stop,
    (object)this.toolStripLabel1,
    (object)this.toolStripProgressBar1);
                // 本窗口独立管理 stopManager
                this._loopingHost.StopManager = stopManager;
                this._loopingHost.GroupName = "";
            }
        }

        private void ImportPatronDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportPatronDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.MergeMode)
            {
                var merge_key_columns = _columns.Where(o => o.IsMergeKey).ToList();
                if (merge_key_columns.Count == 0)
                {
                    MessageBox.Show(this, "尚未指定合并键列");
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void toolStripButton_load_Click(object sender, EventArgs e)
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

            _ = Task.Factory.StartNew(
                () =>
                {
                    _loadExcel(dlg.FileName);
                },
                default,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void _loadExcel(string filename)
        {
            using (var looping = Looping("正在从 Excel 文件装载数据 ...",
                "disableControl"))
            {
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
                        grid_row.Cells[col].Value = cell.GetString();
                    }

                    if (first_row == false)
                    {
                        AutoSetFieldName(cells);
                        first_row = true;
                    }

                    i++;
                    looping.Progress.SetProgressValue(i);
                }
            }
        }


        public override void UpdateEnable(bool bEnable)
        {
            this.dataGridView1.Enabled = bEnable;
            // this.toolStrip1.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
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

        public XmlDocument BuildPatronXml(DataGridViewRow row)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<patron />");
            foreach (var s in _patron_field_names)
            {
                string field_name = GetLeft(s);
                int index = GetColumnIndex(field_name);
                if (index == -1)
                    continue;
                var cell = row.Cells[index];

                // email 元素内容特殊处理
                if (field_name == "email")
                {
                    string old_value = DomUtil.GetElementText(dom.DocumentElement, "email");
                    StringBuilder text = new StringBuilder();
                    if (string.IsNullOrEmpty(old_value) == false)
                        text.Append(old_value);
                    var values = StringUtil.SplitList((string)cell.Value);
                    foreach (var value in values)
                    {
                        if (text.Length > 0)
                            text.Append(",");
                        text.Append($"email:{value}");
                    }
                    DomUtil.SetElementText(dom.DocumentElement, field_name, text.ToString());
                }
                else
                    DomUtil.SetElementText(dom.DocumentElement, field_name, (string)cell.Value);
            }

            return dom;
        }

        public static string[] PatronFieldNames
        {
            get
            {
                return _patron_field_names;
            }
        }

        // 适合用作合并键的字段名列表
        static string[] _mergekey_field_names = new string[]
        {
            "barcode",
            "name",
            "idCardNumber",
        };

        // 读者字段名列表
        static string[] _patron_field_names = new string[]
        {
            "barcode:证条码号,条码号",
            "state:状态",
            "readerType:读者类型",
            "createDate:创建日期,办证日期",
            "expireDate:失效日期",
            "name:姓名",
            "namePinyin:姓名拼音",   // 姓名拼音
            "gender:性别",
            // "birthday:",     // 注：逐步废止这个元素，用 dateOfBirth 替代
            "dateOfBirth:生日",
            "idCardNumber:身份证号",
            "department:单位,部门,班级",
            "post:邮政编码,邮编",
            "address:地址",
            "tel:电话号码,电话",
            "email:Email地址",
            "comment:注释",
            // "zhengyuan",
            "hire:租金",
            "cardNumber:证号",   // 借书证号。为和原来的(100$b)兼容，也为了将来放RFID卡号 2008/10/14 
            "foregift:押金", // 押金。
            "displayName:显示名",  // 显示名
            "preference:个性化参数",   // 个性化参数
            "outofReservations:预约未取参数",    // 预约未取参数
            "nation:民族",   // 民族
            "fingerprint:指纹", // 指纹数据
            "palmprint:掌纹",    // 掌纹数据
            "rights:权限", // 权限
            "personalLibrary:个人书斋", // 个人书斋
            "friends:好友", // 好友
            "access:存取定义",   // 存取定义
            "refID:参考ID", // 参考 ID
            "face:人脸", // 人脸数据
        };

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            {
                menuItem = new MenuItem("设置读者字段名");
                // menuItem.Click += new System.EventHandler(this.menu_openSelectedRecord_Click);
                contextMenu.MenuItems.Add(menuItem);

                foreach (var s in _patron_field_names)
                {
                    string name = GetLeft(s);
                    string captions = GetRight(s);
                    string caption = StringUtil.SplitList(captions).FirstOrDefault();

                    MenuItem subMenuItem = new MenuItem(name + "\t" + caption);
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
                    }; subMenuItem.Click += new System.EventHandler(this.menu_setFieldName_Click);
                    menuItem.MenuItems.Add(subMenuItem);
                }
            }

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            var current_column = _columns[e.ColumnIndex];

            menuItem = new MenuItem("合并键");
            if (current_column.IsMergeKey)
                menuItem.Checked = true;
            if (string.IsNullOrEmpty(current_column.PatronFieldName)
                || Array.IndexOf(_mergekey_field_names, current_column.PatronFieldName) == -1)
                menuItem.Enabled = false;
            menuItem.Tag = current_column;
            menuItem.Click += new System.EventHandler(this.menu_toggleMergeKey_Click);
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Control.MousePosition));
            // contextMenu.Show(this.dataGridView1, new Point(e.X, e.Y));
            /*
            {
                MessageBox.Show(this, $"ColumnHeaderMouseClick ColIndex={e.ColumnIndex},RowIndex={e.RowIndex}");
            }
            */
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

        /*
        static string RemoveMergeKeyChar(string text)
        {
            if (text == null)
                return text;
            if (text.StartsWith("*"))
                return text.Substring(1);
            return text;
        }

        static string AddMergeKeyChar(string text)
        {
            if (text == null)
                return text;
            if (text.StartsWith("*") == false)
                return "*" + text;
            return text;
        }
        */

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
            
            // 如果是不适合作为合并键的字段，要清除 IsMergeKey 状态
            if (Array.IndexOf(_mergekey_field_names, info.Column.PatronFieldName) == -1)
                info.Column.IsMergeKey = false;
            
            info.Column.ViewColumn.HeaderText = BuildHeaderText(info.Name, info.Caption, info.Column.IsMergeKey);
        }

        static string BuildHeaderText(
            string field_name,
            string field_caption,
            bool isMergeKey)
        {
            return $"{(isMergeKey ? "*" : "")}[{field_name}]\r\n{field_caption}";
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            var current_row = this.dataGridView1.Rows[e.RowIndex];
            menuItem = new MenuItem("移除行");
            if (current_row.IsNewRow)
                menuItem.Enabled = false;
            menuItem.Tag = e.RowIndex;
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedLine_Click);
            contextMenu.MenuItems.Add(menuItem);

            /*
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);
            */

            contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Control.MousePosition));

            /*
            if (e.Button == MouseButtons.Right)
            {
                MessageBox.Show(this, $"RowHeaderMouseClick ColIndex={e.ColumnIndex},RowIndex={e.RowIndex}");
            }
            */
        }

        void menu_removeSelectedLine_Click(object sender, EventArgs e)
        {
            MenuItem menu = sender as MenuItem;
            int row_index = (int)menu.Tag;

            var row = this.dataGridView1.Rows[row_index];
            if (row.IsNewRow)
                return;
            this.dataGridView1.Rows.RemoveAt(row_index);
        }

        void ClearColumns()
        {
            this.TryInvoke(() =>
            {
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

        // 根据显示名找到(dp2 读者记录)字段名
        static string FindFieldNameByCaption(string caption,
            out string field_caption)
        {
            field_caption = "";
            foreach (var s in _patron_field_names)
            {
                string captions = GetRight(s);
                if (string.IsNullOrEmpty(captions))
                    continue;
                if (StringUtil.IsInList(caption, captions))
                {
                    string field_name = GetLeft(s);
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

    }

    // 存储一个列的配置参数
    class PatronColumn
    {
        // 本列对应于 dp2 读者库记录的字段名
        public string PatronFieldName { get; set; }

        // 是否为合并 Key
        public bool IsMergeKey { get; set; }

        public DataGridViewColumn ViewColumn { get; set; }
    }

}
