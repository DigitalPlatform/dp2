using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Xml;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Interop;
using DigitalPlatform.Text;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 流通权限编辑器控件
    /// LoanPolicyControl.xaml 的交互逻辑
    /// </summary>
    public partial class LoanPolicyControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // public event TextChangedEventHandler TextChanged = null;

        XmlDocument _dom = null;
        // List<string> _readerTypes = new List<string>();
        List<string> _bookTypes = new List<string>();

        List<Row> _rows = new List<Row>();

        bool _uiChanged = false;    // 界面上是否修改了数据？
        bool _domChanged = false;   // this._dom 内数据是否发生了变化

        public bool Changed
        {
            get
            {
                if (this._uiChanged == true || this._domChanged == true)
                    return true;
                return false;
            }
        }

        public LoanPolicyControl()
        {
            InitializeComponent();
            this.DataContext = this;
            if (null == System.Windows.Application.Current)
                new System.Windows.Application();
        }

        // TODO: 如果 strXml 为空，需要让馆代码列表设置为当前用户可以管辖的那些馆代码
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码。也就是说馆代码列表中，至少要出现这些事项
        public int SetData(
            string strLibraryCodeList,
            string strXml,
            out string strError)
        {
            strError = "";

            this._dom = new XmlDocument();
            try
            {
                if (string.IsNullOrEmpty(strXml) == false)
                    this._dom.LoadXml(strXml);
                else
                    this._dom.LoadXml("<rightsTable />");
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            if (_comboBox_libraryCode == null)
            {
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());

                this._grid.Children.Add(grid);
                Grid.SetColumn(grid, 0);
                Grid.SetRow(grid, 0);

                Label label = new Label();
                label.Content = "馆代码";
                label.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
                label.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                grid.Children.Add(label);
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, 0);

                _comboBox_libraryCode = new ComboBox();
                _comboBox_libraryCode.Margin = new Thickness(4, 0, 0, 0);
                _comboBox_libraryCode.DropDownClosed += new EventHandler(_comboBox_libraryCode_DropDownClosed);
                grid.Children.Add(_comboBox_libraryCode);
                Grid.SetColumn(_comboBox_libraryCode, 0);
                Grid.SetRow(_comboBox_libraryCode, 1);
#if NO
                this._grid.Children.Add(_comboBox_libraryCode);
                Grid.SetColumn(_comboBox_libraryCode, 0);
                Grid.SetRow(_comboBox_libraryCode, 0);
#endif
            }
            FillLibraryCodeList(strLibraryCodeList);

            this._strCurrentLibraryCode = this.CurrentLibraryCode;


            int nRet = Initial(out strError);
            if (nRet == -1)
                return -1;


            this._uiChanged = false;
            this._domChanged = false;
            return 0;
        }

        string _strCurrentLibraryCode = "";

        public string CurrentLibraryCode
        {
            get
            {
                if (_comboBox_libraryCode == null)
                    return _strCurrentLibraryCode;
                return _comboBox_libraryCode.Text;
            }
            set
            {
                if (_comboBox_libraryCode == null)
                    _strCurrentLibraryCode = value;
                else
                {
                    if (_comboBox_libraryCode.Items.IndexOf(value) == -1)
                        _comboBox_libraryCode.Items.Add(value);
                    _comboBox_libraryCode.Text = value;
                }

            }
        }

        ComboBox _comboBox_libraryCode = null;

        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码。也就是说馆代码列表中，至少要出现这些事项
        void FillLibraryCodeList(string strLibraryCodeList)
        {
            Debug.Assert(_comboBox_libraryCode != null, "");

            List<string> librarycodes = new List<string>();
            XmlNodeList nodes = this._dom.DocumentElement.SelectNodes("library");
            foreach (XmlNode node in nodes)
            {
                string strCode = DomUtil.GetAttr(node, "code");
                librarycodes.Add(strCode);
            }

            // 看看是否还有不属于任何<library>元素的
            nodes = this._dom.DocumentElement.SelectNodes("//param[count(ancestor::library) = 0]");
            if (nodes.Count > 0)
            {
                if (librarycodes.IndexOf("") == -1)
                    librarycodes.Insert(0, "");
            }

            // 增补
            string[] list = strLibraryCodeList.Split(new char[] { ',' }); // , StringSplitOptions.RemoveEmptyEntries
            foreach (string s in list)
            {
                if (librarycodes.IndexOf(s) == -1)
                {
                    // 如果要增补空字符串，需要放在第一个元素
                    if (string.IsNullOrEmpty(s) == true)
                        librarycodes.Insert(0, s);
                    else
                        librarycodes.Add(s);
                }
            }

            string strCurrent = _comboBox_libraryCode.Text;
            _comboBox_libraryCode.Items.Clear();
            foreach (string s in librarycodes)
            {
                _comboBox_libraryCode.Items.Add(s);
            }

            if (_comboBox_libraryCode.Items.IndexOf(strCurrent) != -1)
                _comboBox_libraryCode.Text = strCurrent;
            else
            {
                if (_comboBox_libraryCode.Items.Count > 0)
                    _comboBox_libraryCode.SelectedIndex = 0;
                // _comboBox_libraryCode.Text = _comboBox_libraryCode.Items[0].ToString();
            }

        }

        void AddEvents(bool bAdd)
        {
            if (this._inInitial == true)
                return; // 初始化阶段暂时不要添加事件

            foreach (Row row in this._rows)
            {
                AddRowEvents(row, bAdd);
            }
        }

        // parameters:
        //      nColIndex   按内容计算。0 表示第一个内容列，注意，不是读者参数列
        void AddColEvents(int nColIndex, bool bAdd)
        {
            if (this._inInitial == true)
                return; // 初始化阶段暂时不要添加事件

            foreach (Row row in this._rows)
            {
                PolicyCell cell = row.Cells[nColIndex];

                if (bAdd == true)
                {
                    cell.PropertyChanged += new PropertyChangedEventHandler(cell_PropertyChanged);
                }
                else
                {
                    cell.PropertyChanged -= new PropertyChangedEventHandler(cell_PropertyChanged);
                }
            }
        }

        void AddRowEvents(Row row, bool bAdd)
        {
            if (this._inInitial == true)
                return; // 初始化阶段暂时不要添加事件

            if (bAdd == true)
            {
                // row.PatronPolicyCell.TextChanged += new TextChangedEventHandler(PatronPolicyCell_TextChanged);
                row.PatronPolicyCell.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PatronPolicyCell_PropertyChanged);
                row.Label.MouseDown += new MouseButtonEventHandler(Label_row_MouseDown);
            }
            else
            {
                // row.PatronPolicyCell.TextChanged -= new TextChangedEventHandler(PatronPolicyCell_TextChanged);
                row.PatronPolicyCell.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(PatronPolicyCell_PropertyChanged);
                row.Label.MouseDown -= new MouseButtonEventHandler(Label_row_MouseDown);
            }
            foreach (PolicyCell cell in row.Cells)
            {
                if (bAdd == true)
                {
                    cell.PropertyChanged += new PropertyChangedEventHandler(cell_PropertyChanged);
                    //cell.TextChanged += new TextChangedEventHandler(cell_TextChanged);
                }
                else
                {
                    cell.PropertyChanged -= new PropertyChangedEventHandler(cell_PropertyChanged);
                    //cell.TextChanged -= new TextChangedEventHandler(cell_TextChanged);
                }
            }
        }

        void Label_row_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            SelectTitle(label);
        }

        void cell_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CommentText")
                return;
            this._uiChanged = true;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        void PatronPolicyCell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CalendarList" || e.PropertyName == "CommentText")
                return;

            this._uiChanged = true;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        void ClearControls()
        {
            // AddEvents(false);

            foreach (Row row in this._rows)
            {
                this._grid.Children.Remove(row.Label);
                this._grid.Children.Remove(row.PatronPolicyCell);
                foreach (PolicyCell cell in row.Cells)
                {
                    this._grid.Children.Remove(cell);
                }
            }

            foreach (Label label in this._columnLabels)
            {
                this._grid.Children.Remove(label);
            }

            this._rows.Clear();
        }

        // 栏目标题 Label 数组
        List<Label> _columnLabels = new List<Label>();

        bool _inInitial = false;

        // 初始化行列，各种单元
        int Initial(out string strError)
        {
            strError = "";

            // 摘掉所有 TextChanged 事件
            AddEvents(false);
            this._inInitial = true;
            try
            {
                foreach (Label label in this._columnLabels)
                {
                    this._grid.Children.Remove(label);
                }
                this._columnLabels.Clear();

                this._selectedTitle = null;

                ClearControls();

                string strLibraryCode = this.CurrentLibraryCode;

                this._bookTypes.Clear();
#if NO
            // 若干图书类型
            XmlNodeList booktype_nodes = this._dom.DocumentElement.SelectNodes("bookTypes/item");
            foreach (XmlNode booktype_node in booktype_nodes)
            {
                this._bookTypes.Add(booktype_node.InnerText.Trim());
            }
#endif
                this._bookTypes = LoanParam.GetBookTypes(this._dom.DocumentElement,
                    strLibraryCode);

                List<string> readerTypes = new List<string>();
#if NO
            // 若干读者类型
            XmlNodeList readertype_nodes = this._dom.DocumentElement.SelectNodes("readerTypes/item");
            foreach (XmlNode readertype_node in readertype_nodes)
            {
                readerTypes.Add(readertype_node.InnerText.Trim());
            }
#endif
                readerTypes = LoanParam.GetReaderTypes(this._dom.DocumentElement,
                    strLibraryCode);

                // 增补内容中发现的，<readerTypes> 和 <bookTypes> 元素下没有列出来的读者和图书类型
                {
                    List<string> content_readertypes = null;
                    List<string> content_booktypes = null;

                    // 从<rightsTable>的权限定义代码中(而不是从<readerTypes>和<bookTypes>元素下)获得读者和图书类型列表
                    LoanParam.GetReaderAndBookTypes(
                        this._dom.DocumentElement,
                        strLibraryCode,
                        out content_readertypes,
                        out content_booktypes);
                    AppendList(ref readerTypes, content_readertypes);
                    AppendList(ref this._bookTypes, content_booktypes);
                }


                // 创建第一行
                int nRowIndex = 0;

                List<string> titles = new List<string>();
                titles.Add("");
                titles.Add("");
                titles.AddRange(this._bookTypes);

                {
                    int nColIndex = 0;
                    foreach (string title in titles)
                    {
                        if (nColIndex == 0)
                        {
                            nColIndex++;
                            continue;
                        }

#if NO
                        Label label = new Label();
                        label.Content = title;
                        label.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        label.FontSize = 24;
                        label.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        label.MouseDown += new MouseButtonEventHandler(label_column_MouseDown);
                        Grid.SetColumn(label, nColIndex++);
                        Grid.SetRow(label, nRowIndex);
                        this._grid.Children.Add(label);

                        _columnLabels.Add(label);
#endif
                        InsertColumnTitle(title, nColIndex);
                        nColIndex++;
                    }

                    // 按钮
                    {
                        SetColButton(nColIndex);
                    }
                }

                // 其余行
                // 其他若干行
                {
                    nRowIndex++;

                    foreach (string strReaderType in readerTypes)
                    {
                        InsertContentLine(strReaderType, ref nRowIndex);
                    }

                }

                // 最后一行，只有一个按钮
                {
                    SetRowButton(nRowIndex);
                }

                SetColRowDefinitions();

                int nRet = SetCells(out strError);
                if (nRet == -1)
                    return -1;

                CalendarList = CalendarList;

                return 0;
            }
            finally
            {
                // 挂上所有 TextChanged 事件
                this._inInitial = false;
                AddEvents(true);
            }
        }

        static void AppendList(ref List<string> l1, List<string> l2)
        {
            if (l2 == null)
                return;
            foreach (string s in l2)
            {
                if (l1.IndexOf(s) == -1)
                    l1.Add(s);
            }
        }

        // parameters:
        //      nColIndex   全部列的索引
        Label InsertColumnTitle(string title,
            int nColIndex)
        {
            Debug.Assert(nColIndex > 0, "");

            Label label = new Label();
            label.Content = title;
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            //label.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            label.FontSize = 24;
            label.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            label.MouseDown += new MouseButtonEventHandler(label_column_MouseDown);
            Grid.SetColumn(label, nColIndex);   // ++
            Grid.SetRow(label, 0);  // nRowIndex
            this._grid.Children.Add(label);

            _columnLabels.Insert(nColIndex - 1, label);

            // 右边的移动
            for (int i = nColIndex + 1; i < _columnLabels.Count + 1; i++)
            {
                Label current = _columnLabels[i - 1];
                Grid.SetColumn(current, i);
            }

            return label;
        }

        // 当前选定的 栏或者行标题
        Label _selectedTitle = null;

        void SelectTitle(Label control)
        {
            if (control == _selectedTitle)
            {
                // _selectedTitle.Editable = true;
                return;
            }
            // 还原以前的背景色
            if (_selectedTitle != null)
                _selectedTitle.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));

            control.Background = new SolidColorBrush(Color.FromArgb(200, 255, 0, 0));    // Colors.Red

#if NO
            control.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            control.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;


            Grid.SetColumn(control, Grid.GetColumn(control));
            Grid.SetRow(control, Grid.GetRow(control));
#endif

            _selectedTitle = control;
        }

        void label_column_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            SelectTitle(label);
        }

        // 选择了新的馆代码
        void _comboBox_libraryCode_DropDownClosed(object sender, EventArgs e)
        {
            string strOldLibraryCode = this._strCurrentLibraryCode;
            if (this._comboBox_libraryCode.Text == strOldLibraryCode)
                return;
            // 如果有修改，修改 _dom 中的数据
            UpdateDom(strOldLibraryCode);

            // 重新设置界面数据
            this._strCurrentLibraryCode = this.CurrentLibraryCode;

            string strError = "";
            int nRet = Initial(out strError);
            if (nRet == -1)
                ShowMessageBox(strError);

            // 将全部日历名进行筛选，只给读者参数里面设置分馆的日历
            this.CalendarList = _calendarList;
        }

        // "新增图书类型" 按钮
        Button _button_newBookType = null;

        // 首次或者后面设置用于增加一栏的按钮
        // parameters:
        //      nColIndex   栏目索引位置。按照实际显示的全部栏目计算
        void SetColButton(int nColIndex)
        {
            if (_button_newBookType == null)
            {
                _button_newBookType = new Button();
                _button_newBookType.Margin = new Thickness(4, 8, 4, 8);
                _button_newBookType.Padding = new Thickness(4);
                _button_newBookType.Content = "新增图书类型";
                _button_newBookType.Click += new RoutedEventHandler(button_newBookType_Click);
                this._grid.Children.Add(_button_newBookType);
            }

            Grid.SetColumn(_button_newBookType, nColIndex++);
            Grid.SetRow(_button_newBookType, 0);
        }


        Button _button_newReaderType = null;

        // 首次或者后面设置用于增加一行的按钮
        // parameters:
        //      nRowIndex   行索引位置。按照实际显示的全部行计算
        void SetRowButton(int nRowIndex)
        {

            int nColIndex = 0;
            if (_button_newReaderType == null)
            {
                _button_newReaderType = new Button();
                _button_newReaderType.Margin = new Thickness(4);
                _button_newReaderType.Padding = new Thickness(4);
                _button_newReaderType.Content = "新增读者类型";
                _button_newReaderType.Click += new RoutedEventHandler(button_newReaderType_Click);
                this._grid.Children.Add(_button_newReaderType);
            }


            Grid.SetColumn(_button_newReaderType, nColIndex++);
            Grid.SetRow(_button_newReaderType, nRowIndex);
        }

        // 新增一个内容行
        // parameters:
        //      nRowIndex   全部行的坐标
        Row InsertContentLine(string strReaderType,
            ref int nRowIndex)
        {
            int nColIndex = 0;

            Row new_row = new Row();
            this._rows.Insert(nRowIndex - 1, new_row);
            new_row.ReaderType = strReaderType;
            new_row.PatronPolicyCell = new PatronPolicyCell();
            // row.PatronPolicyCell.TextChanged += new TextChangedEventHandler(PatronPolicyCell_TextChanged);
            new_row.PatronPolicyCell.CommentText = strReaderType;

            // 将全部日历名进行筛选，只给读者参数里面设置分馆的日历
            List<string> temp = GetCarlendarNamesByLibraryCode(this._strCurrentLibraryCode,
                _calendarList);
            new_row.PatronPolicyCell.CalendarList = temp;

            // 0
            Label label = new Label();
            label.Content = strReaderType;
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            label.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            label.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            label.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            label.FontSize = 24;
            label.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            Grid.SetColumn(label, nColIndex++);
            Grid.SetRow(label, nRowIndex);
            this._grid.Children.Add(label);

            new_row.Label = label;
            new_row.Label.MouseDown += new MouseButtonEventHandler(Label_row_MouseDown);

            // 1 读者参数
            Grid.SetColumn(new_row.PatronPolicyCell, nColIndex++);
            Grid.SetRow(new_row.PatronPolicyCell, nRowIndex);
            this._grid.Children.Add(new_row.PatronPolicyCell);

            // 其他栏
            // 若干图书类型
            foreach (string strBookType in this._bookTypes)
            {
                PolicyCell cell = new PolicyCell();
                cell.CommentText = strReaderType + " - " + strBookType;
                // cell.TextChanged += new TextChangedEventHandler(cell_TextChanged);
                Grid.SetColumn(cell, nColIndex++);
                Grid.SetRow(cell, nRowIndex);
                this._grid.Children.Add(cell);

                new_row.Cells.Add(cell);
            }

            // 下面的行移动
            for (int i = nRowIndex; i < this._rows.Count; i++)
            {
                Row row = this._rows[i];

                Grid.SetRow(row.Label, i + 1);
                Grid.SetRow(row.PatronPolicyCell, i + 1);
                foreach (PolicyCell cell in row.Cells)
                {
                    Grid.SetRow(cell, i + 1);
                }
            }

            nRowIndex++;

            if (this._inInitial == false)
                AddRowEvents(new_row, true);
            return new_row;
        }

        void DoInsertRow()
        {
            string strError = "";

            if (this._selectedTitle == null)
            {
                strError = "尚未选定插入位置的行标题";
                goto ERROR1;
            }
            int index = this._columnLabels.IndexOf(this._selectedTitle as Label);
            if (index != -1)
            {
                strError = "请选定一个行标题，表示插入位置";
                goto ERROR1;
            }
            int nRowIndex = -1;
            foreach (Row row in this._rows)
            {
                if (row.Label == this._selectedTitle)
                {
                    nRowIndex = this._rows.IndexOf(row);
                    break;
                }
            }
            if (nRowIndex == -1)
            {
                strError = "rows 中没有找到 _selectedTitle";
                goto ERROR1;
            }
            InsertNewRow(nRowIndex + 1);
            return;
        ERROR1:
            ShowMessageBox(strError);
        }

        // 在选定的列位置插入一个新的列
        void DoInsertColumn()
        {
            string strError = "";

            if (this._selectedTitle == null)
            {
                strError = "尚未选定插入位置的列标题";
                goto ERROR1;
            }
            int index = this._columnLabels.IndexOf(this._selectedTitle as Label);
            if (index == -1)
            {
                strError = "请选定一个列标题，表示插入位置";
                goto ERROR1;
            }
            if (index == 0)
            {
                strError = "无法在读者参数列以前插入新的列";
                goto ERROR1;
            }

            InsertNewColumn(index - 1);
            return;
        ERROR1:
            ShowMessageBox(strError);
        }

        // 新增一个内容列
        // parameters:
        //      nColIndex   按内容计算。0 表示第一个内容列，注意，不是读者参数列
        void NewContentCol(string strBookType,
            int nColIndex)
        {
            this._bookTypes.Insert(nColIndex, strBookType);

            // 第一行
            {
#if NO
                Label label = new Label();
                label.Content = strBookType;
                label.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                label.FontSize = 24;
                label.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                label.MouseDown += new MouseButtonEventHandler(label_column_MouseDown);

                Grid.SetColumn(label, nColIndex + 2);
                Grid.SetRow(label, 0);
                this._grid.Children.Add(label);

                _columnLabels.Add(label);
#endif
                Label label = InsertColumnTitle(strBookType, nColIndex + 2);
                SelectTitle(label); // 选中
            }

            int nRowIndex = 1;
            // 其余行
            foreach (Row row in this._rows)
            {
                PolicyCell cell = new PolicyCell();
                cell.CommentText = row.ReaderType + " - " + strBookType;
                row.Cells.Insert(nColIndex, cell);

                Grid.SetColumn(cell, nColIndex + 2);
                Grid.SetRow(cell, nRowIndex);
                this._grid.Children.Add(cell);


                // 右边重新设置
                for (int i = nColIndex + 1; i < row.Cells.Count; i++)
                {
                    PolicyCell current = row.Cells[i];

                    Grid.SetColumn(current, i + 2);
                }

                nRowIndex++;
            }

            SetColButton(this._columnLabels.Count + 1);
            SetColRowDefinitions();
        }

        // 删除一个内容列
        // parameters:
        //      nColIndex   按内容计算。0 表示第一个内容列，注意，不是读者参数列
        void DeleteContentCol(int nColIndex)
        {
            this._bookTypes.RemoveAt(nColIndex);

            // 第一行
            Label label = this._columnLabels[nColIndex + 1];
            label.MouseDown -= new MouseButtonEventHandler(label_column_MouseDown);

            this._grid.Children.Remove(label);
            this._columnLabels.RemoveAt(nColIndex + 1);

            if (this._selectedTitle == label)
                this._selectedTitle = null;
            // 右边的每个 Cell 都挪动了位置
            for (int i = nColIndex + 1; i < _columnLabels.Count; i++)
            {
                label = this._columnLabels[i];
                Grid.SetColumn(label, i + 1);
            }

            // 按钮
            Grid.SetColumn(this._button_newBookType, this._columnLabels.Count + 1);

            // 为一列删除事件
            AddColEvents(nColIndex, false);

            int nRowIndex = 1;
            // 其余行
            foreach (Row row in this._rows)
            {
                PolicyCell cell = row.Cells[nColIndex];

                row.Cells.RemoveAt(nColIndex);
                this._grid.Children.Remove(cell);

                // 右边的每个 Cell 都挪动了位置
                for (int i = nColIndex; i < row.Cells.Count; i++)
                {
                    PolicyCell current = row.Cells[i];

                    Grid.SetColumn(current, i + 2);
                }

                nRowIndex++;
            }



            // SetColButton(_columnLabels.Count + 1);
            SetColRowDefinitions();

            this._uiChanged = true;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("DeleteColumn"));
        }

        void DeleteContentLine(int nRowIndex)
        {
            if (nRowIndex >= this._rows.Count)
                throw new ArgumentException("行索引值超过范围", "nRowIndex");

            {
                Row row = this._rows[nRowIndex];

                // 为一行删除事件
                if (this._inInitial == false)
                    AddRowEvents(row, false);

                this._rows.RemoveAt(nRowIndex);

                {
                    this._grid.Children.Remove(row.Label);
                    this._grid.Children.Remove(row.PatronPolicyCell);
                    foreach (PolicyCell cell in row.Cells)
                    {
                        this._grid.Children.Remove(cell);
                    }
                }

            }

            // this._grid.RowDefinitions.RemoveAt(nRowIndex + 1);

            // 将下面的行的 grid 坐标值 减少
            for (int i = nRowIndex; i < this._rows.Count; i++)
            {
                Row row = this._rows[i];
                Grid.SetRow(row.Label, i + 1);
                Grid.SetRow(row.PatronPolicyCell, i + 1);
                foreach (PolicyCell cell in row.Cells)
                {
                    Grid.SetRow(cell, i + 1);
                }
            }

            // button
            Grid.SetRow(this._button_newReaderType, this._rows.Count + 1);
            SetColRowDefinitions();

            this._uiChanged = true;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("DeleteRow"));
        }

        public System.Windows.Forms.IWin32Window Owner = null;

        // parameters:
        //      nColIndex   按内容计算。0 表示第一个内容列，注意，不是读者参数列
        void InsertNewColumn(int nColIndex)
        {
            string strBookType = "";
        REDO:
            strBookType = InputDlg.GetInput(this.Owner != null ? this.Owner : Wpf32Window.GetMainWindow(),
                "新增图书类型",
                "图书类型:",
                strBookType);
            if (strBookType == null)
                return;

            /*
            if (strBookType == "")
            {
                ShowMessageBox("图书类型不能为空，请重新输入");
                goto REDO;
            }
            if (strBookType == "*")
            {
                ShowMessageBox("图书类型不能为 *，请重新输入");
                goto REDO;
            }
            */
            var error = CheckBookType(strBookType);
            if (error != null)
            {
                ShowMessageBox(error);
                goto REDO;
            }

            // 查重
            if (this._bookTypes.IndexOf(strBookType) != -1)
            {
                ShowMessageBox("图书类型 '" + strBookType + "' 已经存在，请重新输入");
                goto REDO;
            }

            NewContentCol(strBookType,
                nColIndex);

            // 为一列添加事件
            AddColEvents(nColIndex, true);

            this._uiChanged = true;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("NewColumn"));

        }

        void button_newBookType_Click(object sender, RoutedEventArgs e)
        {
            int nColIndex = this._bookTypes.Count;
            InsertNewColumn(nColIndex);
        }

        // 检查输入的图书类型是否合法
        string CheckBookType(string strBookType)
        {
            if (strBookType == "")
            {
                return ("图书类型不能为空，请重新输入");
            }
            if (strBookType == "*")
            {
                return ("图书类型不能为 *，请重新输入");
            }

            if (strBookType.IndexOfAny(new char[] { '*', '?', '/' }) != -1)
            {
                return ("图书类型，不允许包含 * ? / 字符，请重新输入");
            }

            return null;
        }

        // 检查输入的读者类型是否合法
        string CheckReaderType(string strReaderType)
        {
            if (strReaderType == "")
            {
                return ("读者类型不能为空，请重新输入");
            }
            if (strReaderType == "*")
            {
                return ("读者类型不能为 *，请重新输入");
            }

            // 2022/3/8
            if (strReaderType.Contains("/") == false)
            {
                if (strReaderType.IndexOfAny(new char[] { '*', '?' }) != -1)
                {
                    return ("短形态(即不含 / 的)读者类型，不允许包含 * ? 字符，请重新输入");
                }
            }
            else
            {
                // 2022/3/10
                // 检查长形态的左侧不能等于当前馆代码
                var parts = StringUtil.ParseTwoPart(strReaderType, "/");
                string left = parts[0];
                var libraryCode = this._comboBox_libraryCode.Text;
                if (left == libraryCode)
                    return $"长形态(即包含 / 的)读者类型，左侧不允许使用当前馆代码 '{libraryCode}'";
            }

            return null;
        }

        // parameters:
        //      nRowIndex   全部行的坐标
        void InsertNewRow(int nRowIndex)
        {
            string strReaderType = "";
        REDO:
            strReaderType = InputDlg.GetInput(this.Owner != null ? this.Owner : Wpf32Window.GetMainWindow(),
                "新增读者类型",
                "读者类型:",
                strReaderType);
            if (strReaderType == null)
                return;

            /*
            if (strReaderType == "")
            {
                ShowMessageBox("读者类型不能为空，请重新输入");
                goto REDO;
            }
            if (strReaderType == "*")
            {
                ShowMessageBox("读者类型不能为 *，请重新输入");
                goto REDO;
            }
            */

            var error = CheckReaderType(strReaderType);
            if (error != null)
            {
                ShowMessageBox(error);
                goto REDO;
            }

            // 查重
            foreach (Row row in this._rows)
            {
                if (row.ReaderType == strReaderType)
                {
                    ShowMessageBox("读者类型 '" + strReaderType + "' 已经存在，请重新输入");
                    goto REDO;
                }
            }

            Row new_row = InsertContentLine(strReaderType, ref nRowIndex);
            SetRowButton(this._rows.Count + 1);
            SetColRowDefinitions();

            SelectTitle(new_row.Label);

            this._uiChanged = true;
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs("NewRow"));
        }

        void button_newReaderType_Click(object sender, RoutedEventArgs e)
        {
            int nRowIndex = this._rows.Count + 1;
            InsertNewRow(nRowIndex);
#if NO
        REDO:
            string strBookType = InputDlg.GetInput(this.Owner != null ? this.Owner : Wpf32Window.GetMainWindow(),
                "新增读者类型",
                "读者类型:",
                "");
            if (strReaderType == null)
                return;

            if (strReaderType == "")
            {
                ShowMessageBox( "读者类型不能为空，请重新输入");
                goto REDO;
            }

            // 查重
            foreach (Row row in this._rows)
            {
                if (row.ReaderType == strReaderType)
                {
                    ShowMessageBox( "读者类型 '" + strReaderType + "' 已经存在，请重新输入");
                    goto REDO;
                }
            }

            int nRowIndex = this._rows.Count + 1;
            // string strReaderType = "new reader";
            NewContentLine(strReaderType, ref nRowIndex);
            SetRowButton(nRowIndex);
            SetColRowDefinitions();
#endif
        }

        void SetColRowDefinitions()
        {
            int size = this._bookTypes.Count + 3;
            while (this._grid.ColumnDefinitions.Count < size)
            {
                ColumnDefinition def = new ColumnDefinition();
                //def.Width = new GridLength(200); // GridLength.Auto;
                //def.Width = new GridLength(1, GridUnitType.Star); 
                this._grid.ColumnDefinitions.Add(def);
            }

            // 删除多余的
            if (this._grid.ColumnDefinitions.Count > size)
            {
                this._grid.ColumnDefinitions.RemoveRange(
                    size,
                    this._grid.ColumnDefinitions.Count - size);
            }

            size = this._rows.Count + 2;
            while (this._grid.RowDefinitions.Count < size)
            {
                RowDefinition def = new RowDefinition();
                //def.Height = new GridLength(100);   // GridLength.Auto;
                this._grid.RowDefinitions.Add(def);
            }

            // 删除多余的
            if (this._grid.RowDefinitions.Count > size)
            {
                this._grid.RowDefinitions.RemoveRange(
                    size,
                    this._grid.RowDefinitions.Count - size);
            }
        }

        // 为每个单元设置参数值
        int SetCells(out string strError)
        {
            strError = "";

#if NO
            string strLibraryCode = "";
            if (this._dom.DocumentElement.Name == "library")
            {
                strLibraryCode = DomUtil.GetAttr(this._dom.DocumentElement, "code");
            }
#endif
            string strLibraryCode = this.CurrentLibraryCode;

            foreach (Row row in this._rows)
            {
                // 左边第二列：只和读者类型相关的参数
                {
                    for (int k = 0; k < LoanParam.reader_d_paramnames.Length; k++)
                    {
                        string strParamName = LoanParam.reader_d_paramnames[k];

                        // return:
                        //      reader和book类型均匹配 算4分
                        //      只有reader类型匹配，算3分
                        //      只有book类型匹配，算2分
                        //      reader和book类型都不匹配，算1分
                        int nRet = LoanParam.GetLoanParam(
                            this._dom.DocumentElement,
                            strLibraryCode,
                            row.ReaderType,
                            "",   // 实际上为空
                            strParamName,
                            out string strParamValue,
                            out MatchResult matchresult,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet >= 3)
                            row.PatronPolicyCell.SetValue(strParamName, strParamValue);
                        else
                            row.PatronPolicyCell.SetValue(strParamName, "");
                    } // end of for
                }

                for (int j = 0; j < this._bookTypes.Count; j++)
                {
                    string strBookType = this._bookTypes[j];

                    for (int k = 0; k < LoanParam.two_d_paramnames.Length; k++)
                    {
                        string strParamName = LoanParam.two_d_paramnames[k];

                        string strParamValue = "";
                        MatchResult matchresult;
                        // return:
                        //      reader和book类型均匹配 算4分
                        //      只有reader类型匹配，算3分
                        //      只有book类型匹配，算2分
                        //      reader和book类型都不匹配，算1分
                        int nRet = LoanParam.GetLoanParam(
                            this._dom.DocumentElement,
                            strLibraryCode,
                            row.ReaderType,
                            strBookType,
                            strParamName,
                            out strParamValue,
                            out matchresult,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (nRet >= 4)
                            row.Cells[j].SetValue(strParamName, strParamValue);
                        else
                            row.Cells[j].SetValue(strParamName, "");

                    } // end of for
                }

            }

            return 0;
        }

        void UpdateDom(string strLibraryCode)
        {
            // string strFilter = "";
            XmlNode root = null;
            if (string.IsNullOrEmpty(strLibraryCode) == false)
            {
                // descendant
                XmlNode temp = this._dom.SelectSingleNode("//descendant-or-self::library[@code='" + strLibraryCode + "']");
                if (temp == null)
                {
                    temp = this._dom.CreateElement("library");
                    this._dom.DocumentElement.AppendChild(temp);
                    DomUtil.SetAttr(temp, "code", strLibraryCode);
                }
                root = temp;
            }
            else
            {
                root = this._dom.DocumentElement;
                // strFilter = "[count(ancestor::library) = 0]";
            }

            // 删除原来的下级元素
            XmlNodeList nodes = root.SelectNodes("child::*[not(self::library)]");
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            XmlNode insert_pos = root.SelectSingleNode("library");

            List<string> reader_types = new List<string>();

            foreach (Row row in this._rows)
            {
                reader_types.Add(row.ReaderType);

                XmlNode reader = this._dom.CreateElement("type");
                if (insert_pos != null)
                    root.InsertBefore(reader, insert_pos);
                else
                    root.AppendChild(reader);
                DomUtil.SetAttr(reader, "reader", row.ReaderType);

                foreach (string strName in LoanParam.reader_d_paramnames)
                {
                    string strValue = row.PatronPolicyCell.GetValue(strName);
                    XmlNode param = this._dom.CreateElement("param");
                    reader.AppendChild(param);
                    DomUtil.SetAttr(param, "name", strName);
                    DomUtil.SetAttr(param, "value", strValue);
                }

                for (int j = 0; j < this._bookTypes.Count; j++)
                {
                    string strBookType = this._bookTypes[j];



                    XmlNode book = this._dom.CreateElement("type");
                    reader.AppendChild(book);
                    DomUtil.SetAttr(book, "book", strBookType);

                    foreach (string strName in LoanParam.two_d_paramnames)
                    {
                        string strValue = row.Cells[j].GetValue(strName);
                        XmlNode param = this._dom.CreateElement("param");
                        book.AppendChild(param);
                        DomUtil.SetAttr(param, "name", strName);
                        DomUtil.SetAttr(param, "value", strValue);
                    }
                }

            }


            // TODO: 最好插入在兄弟 <library> 元素以前
            XmlNode readertypes_node = this._dom.CreateElement("readerTypes");
            if (insert_pos != null)
                root.InsertBefore(readertypes_node, insert_pos);
            else
                root.AppendChild(readertypes_node);
            foreach (string s in reader_types)
            {
                // 2022/3/8
                // 为来客配置的 xxx/xxx 形态的读者类型，要跳过
                if (s.Contains("/"))
                    continue;

                XmlNode node = this._dom.CreateElement("item");
                readertypes_node.AppendChild(node);
                node.InnerText = s;
            }

            XmlNode booktypes_node = this._dom.CreateElement("bookTypes");
            if (insert_pos != null)
                root.InsertBefore(booktypes_node, insert_pos);
            else
                root.AppendChild(booktypes_node);
            foreach (string s in this._bookTypes)
            {
                XmlNode node = this._dom.CreateElement("item");
                booktypes_node.AppendChild(node);
                node.InnerText = s;
            }

            this._domChanged = true;
        }

        public string GetData()
        {
            if (this._uiChanged == true)
            {
                UpdateDom(this.CurrentLibraryCode);
                this._uiChanged = false;
            }
            return DomUtil.GetIndentXml(this._dom.DocumentElement);
        }

        void EditCmdExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (target != this)
                return;

            string strName = ((RoutedCommand)e.Command).Name;
            string strTarget = ((FrameworkElement)target).Name;

            if (strName == "Delete")
            {
            }
            else if (strName == "ChangeTitle")
            {
                ChangeSelectedTitle();
            }
            else if (strName == "DeleteTitle")
            {
                DeleteSelectedTitle();
            }
            else if (strName == "InsertColumn")
            {
                DoInsertColumn();
            }
            else if (strName == "InsertRow")
            {
                DoInsertRow();
            }
        }

        void EditCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            string strName = ((RoutedCommand)e.Command).Name;

            if (strName == "Delete")
            {
#if NO
                if (this.m_selectedShelfs.Count > 0)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
#endif
            }
            else if (strName == "ChangeTitle")
            {
                if (this._selectedTitle == null)
                {
                    e.CanExecute = false;
                    return;
                }
                e.CanExecute = true;
            }
            else if (strName == "DeleteTitle")
            {
                if (this._selectedTitle == null)
                {
                    e.CanExecute = false;
                    return;
                }
                e.CanExecute = true;
            }
            else if (strName == "InsertColumn")
            {
                if (this._selectedTitle == null || this._columnLabels.Count == 0)
                {
                    e.CanExecute = false;
                    return;
                }
                int index = this._columnLabels.IndexOf(this._selectedTitle as Label);
                if (index != -1)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
            else if (strName == "InsertRow")
            {
                if (this._selectedTitle == null || this._rows.Count == 0)
                {
                    e.CanExecute = false;
                    return;
                }
                int index = this._columnLabels.IndexOf(this._selectedTitle as Label);
                if (index == -1)
                    e.CanExecute = true;
                else
                    e.CanExecute = false;
            }
        }

        // 修改行标题或者列标题
        void ChangeSelectedTitle()
        {
            string strError = "";

            if (this._selectedTitle == null)
            {
                strError = "尚未选定要修改的标题";
                goto ERROR1;
            }

            int index = this._columnLabels.IndexOf(this._selectedTitle as Label);
            if (index != -1)
            {
                if (index == 0)
                {
                    strError = "读者参数栏目标题不允许修改";
                    goto ERROR1;
                }
                string strBookType = this._bookTypes[index - 1];
            REDO_GET_BOOKTYPE:
                strBookType = InputDlg.GetInput(this.Owner != null ? this.Owner : Wpf32Window.GetMainWindow(),
    "修改图书类型",
    "图书类型:",
    strBookType);
                if (strBookType == null)
                    return; // 放弃修改
                /*
                if (strBookType == "")
                {
                    ShowMessageBox("图书类型不能为空，请重新输入");
                    goto REDO_GET_BOOKTYPE;
                }
                if (strBookType == "*")
                {
                    ShowMessageBox("图书类型不能为 *，请重新输入");
                    goto REDO_GET_BOOKTYPE;
                }
                */
                var error = CheckBookType(strBookType);
                if (error != null)
                {
                    ShowMessageBox(error);
                    goto REDO_GET_BOOKTYPE;
                }

                if (strBookType == this._bookTypes[index - 1])
                    return; // 没有修改


                // 查重
                int nDupIndex = this._bookTypes.IndexOf(strBookType);
                if (nDupIndex != -1 && nDupIndex != index)
                {
                    ShowMessageBox("图书类型 '" + strBookType + "' 已经存在，请重新输入");
                    goto REDO_GET_BOOKTYPE;
                }

                this._bookTypes[index - 1] = strBookType;

                Label label = this._columnLabels[index];
                label.Content = strBookType;

                // 每行的同样位置的 PolicyCell 的 CommentText 都要修改
                foreach (Row row in this._rows)
                {
                    row.Cells[index - 1].CommentText = row.ReaderType + " - " + strBookType;
                }

                this._uiChanged = true;
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("ChangeColumn"));

                return;
            }

            foreach (Row row in this._rows)
            {
                if (this._selectedTitle == row.Label)
                {
                    string strReaderType = row.ReaderType;
                REDO_GET_READERTYPE:
                    strReaderType = InputDlg.GetInput(this.Owner != null ? this.Owner : Wpf32Window.GetMainWindow(),
                        "修改读者类型",
                        "读者类型:",
                        strReaderType);
                    if (strReaderType == null)
                        return;

                    /*
                    if (strReaderType == "")
                    {
                        ShowMessageBox("读者类型不能为空，请重新输入");
                        goto REDO_GET_READERTYPE;
                    }
                    if (strReaderType == "*")
                    {
                        ShowMessageBox("读者类型不能为 *，请重新输入");
                        goto REDO_GET_READERTYPE;
                    }

                    // 2022/3/8
                    if (strReaderType.Contains("/") == false)
                    {
                        if (strReaderType.IndexOfAny(new char[] { '*', '?' }) != -1)
                        {
                            ShowMessageBox("短形态(即不含 / 的)读者类型，不允许包含 * ? 字符，请重新输入");
                            goto REDO_GET_READERTYPE;
                        }
                    }
                    */
                    var error = CheckReaderType(strReaderType);
                    if (error != null)
                    {
                        ShowMessageBox(error);
                        goto REDO_GET_READERTYPE;
                    }

                    if (strReaderType == row.ReaderType)
                        return; // 没有修改

                    // 查重
                    foreach (Row current_row in this._rows)
                    {
                        if (current_row != row
                            && current_row.ReaderType == strReaderType)
                        {
                            ShowMessageBox("读者类型 '" + strReaderType + "' 已经存在，请重新输入");
                            goto REDO_GET_READERTYPE;
                        }
                    }

                    row.Label.Content = strReaderType;
                    row.ReaderType = strReaderType;
                    row.PatronPolicyCell.CommentText = strReaderType;

                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        row.Cells[i].CommentText = row.ReaderType + " - " + this._bookTypes[i];
                    }

                    this._uiChanged = true;
                    if (this.PropertyChanged != null)
                        this.PropertyChanged(this, new PropertyChangedEventArgs("ChangeRow"));

                    return;
                }
            }
            return;
        ERROR1:
            ShowMessageBox(strError);
        }


        // 删除当前选定的标题和所代表的行或列
        void DeleteSelectedTitle()
        {
            string strError = "";

            if (this._selectedTitle == null)
            {
                strError = "尚未选定要删除的标题";
                goto ERROR1;
            }

            int index = this._columnLabels.IndexOf(this._selectedTitle as Label);
            if (index != -1)
            {
                // 删除一个栏目
                if (index == 0)
                {
                    strError = "读者参数栏目不允许删除";
                    goto ERROR1;
                }

                MessageBoxResult result = MessageBox.Show( // Application.Current.MainWindow,
                    "确实要删除栏目 '" + this._selectedTitle.Content + "'?",
                    "",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No,
                    MessageBoxOptions.None);
                if (result == MessageBoxResult.No)
                    return;
                DeleteContentCol(index - 1);
                return;
            }

            foreach (Row row in this._rows)
            {
                if (this._selectedTitle == row.Label)
                {
                    MessageBoxResult result = MessageBox.Show( // Application.Current.MainWindow,
    "确实要删除行 '" + this._selectedTitle.Content + "'?",
    "",
    MessageBoxButton.YesNo,
    MessageBoxImage.Question,
    MessageBoxResult.No,
    MessageBoxOptions.None);
                    if (result == MessageBoxResult.No)
                        return;
                    DeleteContentLine(this._rows.IndexOf(row));
                    return;
                }
            }
            return;
        ERROR1:
            ShowMessageBox(strError);
        }

        public static void ShowMessageBox(string strText)
        {
            if (Application.Current.MainWindow == null)
                MessageBox.Show(strText);
            else
                MessageBox.Show(Application.Current.MainWindow, strText);
        }

        List<string> _calendarList = new List<string>();

        public List<string> CalendarList
        {
            get
            {
                return _calendarList;
            }
            set
            {
                _calendarList = value;

                // 将全部日历名进行筛选，只给读者参数里面设置分馆的日历
                List<string> temp = GetCarlendarNamesByLibraryCode(this._strCurrentLibraryCode,
                    value);

                foreach (Row row in this._rows)
                {
                    row.PatronPolicyCell.CalendarList = temp;
                }
            }
        }

        /// <summary>
        /// 解析日历名。例如 "海淀分馆/基本日历"
        /// </summary>
        /// <param name="strName">完整的日历名</param>
        /// <param name="strLibraryCode">返回馆代码部分</param>
        /// <param name="strPureName">返回纯粹日历名部分</param>
        public static void ParseCalendarName(string strName,
            out string strLibraryCode,
            out string strPureName)
        {
            strLibraryCode = "";
            strPureName = "";
            int nRet = strName.IndexOf("/");
            if (nRet == -1)
            {
                strPureName = strName;
                return;
            }
            strLibraryCode = strName.Substring(0, nRet).Trim();
            strPureName = strName.Substring(nRet + 1).Trim();
        }

        static List<string> GetCarlendarNamesByLibraryCode(string strLibraryCode,
            List<string> list)
        {
            List<string> results = new List<string>();
            foreach (string s in list)
            {
                string strLibraryCode1 = "";
                string strPureName1 = "";

                ParseCalendarName(s,
            out strLibraryCode1,
            out strPureName1);

                if (strLibraryCode == strLibraryCode1)
                    results.Add(s);
            }

            return results;
        }
    }

    class Row
    {
        public Label Label = null;
        public string ReaderType = "";
        public PatronPolicyCell PatronPolicyCell = null;
        public List<PolicyCell> Cells = new List<PolicyCell>();
    }

    public static class MyCommand
    {
        public static readonly RoutedUICommand DeleteTitle = new RoutedUICommand("删除", "DeleteTitle", typeof(LoanPolicyControl));
        public static readonly RoutedUICommand ChangeTitle = new RoutedUICommand("改名", "ChangeTitle", typeof(LoanPolicyControl));
        public static readonly RoutedUICommand InsertColumn = new RoutedUICommand("插入新栏目", "InsertColumn", typeof(LoanPolicyControl));
        public static readonly RoutedUICommand InsertRow = new RoutedUICommand("插入新行", "InsertRow", typeof(LoanPolicyControl));
    }

    public class Wpf32Window : System.Windows.Forms.IWin32Window
    {
        public IntPtr Handle { get; private set; }

        public Wpf32Window(Window wpfWindow)
        {
            Handle = new WindowInteropHelper(wpfWindow).Handle;
        }

        public static System.Windows.Forms.IWin32Window GetHandle(Window window)
        {
            return (System.Windows.Forms.IWin32Window)new Wpf32Window(window);
        }

        public static System.Windows.Forms.IWin32Window GetMainWindow()
        {
            return GetHandle(Application.Current.MainWindow);
        }
    }
}
